using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;

namespace TMCAnalyzer {
	/// <summary>
	/// Sends "expo" command  with optopnal arguments
	/// receives command strings from controller, stores them in a list
	///
	/// After controller finishes export and sends >~OK offers to save arguments into file
	///

	/// <summary>Export progress changed event arguments.</summary>
	public class ExportParametersProgressChangedEventArgs:ProgressChangedEventArgs {
		/// <summary>The Export entry.</summary>
		public ExportParametersEntry ExportParamEntry;

		/// <summary>
		/// Initializes a new instance of the <see cref="TMCAnalyzer.ExportParametersProgressChangedEventArgs"/> class.</summary>
		/// <param name='exportParEntry'/>
		/// <param name='state'/>
		public ExportParametersProgressChangedEventArgs(ExportParametersEntry exportParEntry, object state) : base(0, state) {
			this.ExportParamEntry = exportParEntry;
		}
	}

	/// <summary>
	/// On Export Parameters entry.
	/// Collects commands into list
	/// </summary>
	public delegate void OnExportParametersEntryDelegate(ExportParametersProgressChangedEventArgs e);

	/// <summary>Export Parameters complete event arguments.
	/// offers to save parameters into file
	/// </summary>
	public class ExportParametersCompleteEventArgs:AsyncCompletedEventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="TMCAnalyzer.ExportParametersCompleteEventArgs"/> class.</summary>
		/// <param name='e'/>
		/// <param name='canceled'/>
		/// <param name='state'/>
		public ExportParametersCompleteEventArgs(Exception e, bool canceled, object state) : base(e, canceled, state) { }
	}

	/// <summary>
	/// On Export complete.
	/// offers to save parameters into file
	/// </summary>
	public delegate void OnExportParametersCompleteDelegate(object sender, ExportParametersCompleteEventArgs e);

	public class ExportParametersEntry {
		public string aCommand { get; set; }
		static List<string> ParamList = new List<string>();

		/**********************************************************************************************************************/
		/// <summary>
		/// Adding line  to list
		/// <param name='line'/>
		public ExportParametersEntry(string line) {
			try {
				ParamList.Add(line);
			} catch (System.Exception ex) {
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

	}

	/// <summary>
	/// Analyzer.
	/// http://msdn.microsoft.com/en-us/library/wewwczdw.aspx
	/// </summary>
	public partial class TMCController {
		private object asyncExportOperationLocker = new object();
		private AsyncOperation getExportParametersAsyncOperation;

		/// <summary>
		/// Used to marshall from the async context back to the context
		/// that can call the user
		/// </summary>
		private SendOrPostCallback onExportParametersProgressChangedDelegate;
		private SendOrPostCallback onExportParametersCompletedDelegate;

		private class ExportParametersStatus {
			public bool IsCancelled = false;
			public OnExportParametersEntryDelegate OnExportParametersEntry;
			public OnExportParametersCompleteDelegate OnExportParametersComplete;
			public ExportParametersStatus(OnExportParametersEntryDelegate OnExportParametersEntry,
					OnExportParametersCompleteDelegate OnExportParametersComplete) {
						this.OnExportParametersEntry = OnExportParametersEntry;
						this.OnExportParametersComplete = OnExportParametersComplete;
			}
		}

		private delegate void WorkerExportEventHandler(OnExportParametersEntryDelegate onExportParametersEntry, AsyncOperation asyncOp);

		/**********************************************************************************************************************/
		/// <summary>
		/// Loops the transfer function async.
		/// </summary>
		/// <returns>
		/// The transfer function async.
		/// </returns>
		/// <param name='axis'>
		/// Axis.
		/// </param>
		/// <param name='onExportParametersEntry'>
		/// On loop transfer entry.
		/// </param>
		/// <param name='onExportParametersComplete'>
		/// On loop transfer complete.
		/// </param>
		public void ExportParametersFunctionAsync(OnExportParametersEntryDelegate onExportParametersEntry, OnExportParametersCompleteDelegate onExportParametersComplete)
		{
			// Setup the marshaller from async completion context
			if (onExportParametersProgressChangedDelegate == null) {
				onExportParametersProgressChangedDelegate = new SendOrPostCallback(ExportParametersProgressChanged);
			}
			if (onExportParametersCompletedDelegate == null) {
				onExportParametersCompletedDelegate = new SendOrPostCallback(ExportParametersCompleted);
			}
			var exportParamStatus = new ExportParametersStatus(onExportParametersEntry, onExportParametersComplete);

			lock (asyncExportOperationLocker) {
				getExportParametersAsyncOperation = AsyncOperationManager.CreateOperation(exportParamStatus);

				/* there are times when you want to invoke a delegate and wait for its execution to complete before the current thread continues.
				 * In those cases the Invoke call is what you want.
				 *
				 * In multi-threading applications, you may not want a thread to wait on a delegate to finish execution, especially if that delegate
				 * performs I/O (which could make the delegate and your thread block).In those cases the BeginInvoke would be useful.
				 * By calling it, you're telling the delegate to start but then your thread is free to do other things in parallel with the delegate.
				 * Using BeginInvoke increases the complexity of your code but there are times when the improved performance is worth the complexity.
				 *
				 * Delegate.Invoke: Executes synchronously, on the same thread
				 * Delegate.BeginInvoke: Executes asynchronously, on a threadpool thread.
				 * Control.Invoke: Executes on the UI thread, but calling thread waits for completion before continuing.
				 * Control.BeginInvoke: Executes on the UI thread, and calling thread doesn't wait for completion
				 * Control.BeginInvoke is easier to get right, and will avoid your background thread from having to wait for no good reason. Note that the Windows
				 * Forms team has guaranteed that you can use Control.BeginInvoke in a "fire and forget" manner - i.e. without ever calling EndInvoke.
				 * This is not true of async calls in general: normally every BeginXXX should have a corresponding EndXXX call, usually in the callback.
				*/
				// start the async operation
				var workerDelegate = new WorkerExportEventHandler(GetExportParametersFunction);
				workerDelegate.BeginInvoke(onExportParametersEntry, getExportParametersAsyncOperation, null, null);
			}
		}

		/**********************************************************************************************************************/
		private bool ExportCancelled() {
			lock (asyncExportOperationLocker) {
				if (getExportParametersAsyncOperation != null) {
					var exportParamStatus = getExportParametersAsyncOperation.UserSuppliedState as ExportParametersStatus;
					return exportParamStatus.IsCancelled;
				} else { return true; }
			}
		}

		/**********************************************************************************************************************/
		private void GetExportParametersFunction(OnExportParametersEntryDelegate onExportParametersEntry, AsyncOperation asyncOp) {
			// it is here on Worker Thread
			if (IsDemoMode) return;
			formMain.ParamLinesCount = 0;
			bool UserCancelled = false; //value does not matter yet

			var dataLine = string.Empty;
			var lineReadTimeout = new TimeSpan(0, 0, 1); // 1 second per line timeout
			do {
#if DEBUG
				previousLine = dataLine;
#endif
				dataLine = Device.ReadLineWithTimeout(lineReadTimeout);
				if (dataLine.Length > 0) break; // valid non zero string

			} while (!dataLine.Equals(DoingCommandResponse));

			//                     output is starting with version
			//// version STACIS_IV 95-66282-01 r.A-d @  @ 17-Apr-2021; compiled Apr 14 2021 01:06:42
			//
			// read the expected lines in
			while (true) {
				UserCancelled = ExportCancelled();
				if (UserCancelled) 
					break;

#if DEBUG
				previousLine = dataLine;
#endif
				// see if we got an empty line or terminator line, if so thats the end of the sequence
				if(dataLine.ToLower().Contains("~ok"))
					break; 

				// if echo is enabled we'll see the command here again and we want
				// to just read the next line
				if (dataLine.Equals(""))  { dataLine = Device.ReadLineWithTimeout(lineReadTimeout); }
				else if (dataLine.Equals("\n")) { dataLine = Device.ReadLineWithTimeout(lineReadTimeout); } // left over from the previous line?
			check_line:
				if (dataLine.Length == 0) {
					System.Diagnostics.Debug.WriteLine("empty param line");
					dataLine = Device.ReadLineWithTimeout(lineReadTimeout); //read another line
					goto check_line;
				} else { //valid, add to the list
					var ExportParametersEntry = new ExportParametersEntry(dataLine);
					formMain.ParamLinesCount++;
					ExportParametersProgressChangedEventArgs e = new ExportParametersProgressChangedEventArgs(ExportParametersEntry, getExportParametersAsyncOperation.UserSuppliedState);
					asyncOp.Post(onExportParametersProgressChangedDelegate, e);
				}
			}

			if (UserCancelled) { // not applicable to the export command
				//do {
				//	ControllerPort.WriteLine("ltf>stop");
				//} while (!FlushSerialPortInput());
			}

			// indicate that the method has completed
			CompleteGetExportParametersFunction(ExportCancelled(), asyncOp);
		} // Worker thread ends here

#if DEBUG
				string previousLine = string.Empty;
#endif

		/**********************************************************************************************************************/
		/// <summary>
		/// Determines whether this instance cancel get loop transfer function.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance cancel get loop transfer function; otherwise, <c>false</c>.
		/// </returns>
		public void CancelExportParametersFunction() {
			lock (asyncOperationLocker) {
				if (getExportParametersAsyncOperation != null) {
					var exportParamStatus = getExportParametersAsyncOperation.UserSuppliedState as ExportParametersStatus;
					exportParamStatus.IsCancelled = true;
				}
			}
		}

		/**********************************************************************************************************************/
		// called from main thread, NOT worker, but it is started by worker thread asyncOp.Post(onExportParametersProgressChangedDelegate, e);
		private void ExportParametersProgressChanged(object operationState) {
			var ltfEntryArgs = operationState as ExportParametersProgressChangedEventArgs;
			var exportParamStatus = ltfEntryArgs.UserState as ExportParametersStatus;
			exportParamStatus.OnExportParametersEntry(ltfEntryArgs);
		}

		/**********************************************************************************************************************/
		// called from main thread
		private void ExportParametersCompleted(object operationState) {
			var e = operationState as ExportParametersCompleteEventArgs;
			var exportParamStatus = e.UserState as ExportParametersStatus;
			exportParamStatus.OnExportParametersComplete(this, e);
		}

		/**********************************************************************************************************************/
		// called from worker thread
		private void CompleteGetExportParametersFunction(bool cancelled, AsyncOperation asyncOp) {
			var exportParamStatus = asyncOp.UserSuppliedState as ExportParametersStatus;
			var e = new ExportParametersCompleteEventArgs(null, cancelled, exportParamStatus);

			// end the task, the asyncOp object is responsible for marshaling the call
			asyncOp.PostOperationCompleted(onExportParametersCompletedDelegate, e);
			//workerDelegate.EndInvoke (axis, onExportParametersEntry, getExportParametersAsyncOperation, null, null);
		}
	}
}