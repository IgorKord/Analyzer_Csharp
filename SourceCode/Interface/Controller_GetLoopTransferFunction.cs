using System;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;

namespace TMCAnalyzer {
	/// <summary>
	/// Contains the fields found in an ltf entry
	///
	/// Ltf output format:
	///
	/// ltf>x
	///
	/// TMC Mag-NetX (c), FW 95-37938-01 Rev.B3 #4 @01-Jul-2010
	///
	/// Analog Loop, ACTIVE , Analog gain=+64,
	/// Digital Loop, DISABLED, Digital gain=21.20
	/// freq, X-Suppression dB gain=+64,X-dB_Open, X-deg_Open, X-dB_Clos, X-deg_Clos,
	/// Doing command
	/// 5040.0, 0.09, -48.90, 145.22, -48.81, 170.88, #
	/// 4990.0, 0.09, -47.39, 52.00, -47.30, -60.64, #
	///
	/// fff.ff, sss.ss, ooo.gg, ooo.pp, ccc.gg, ccc.pp, #\r\n
	/// fff.ff - frequency
	/// sss.ss - suppression in dB
	/// ooo.gg - open loop gain in dB
	/// ooo.pp - open loop phase in degrees wrapped
	/// ccc.gg - open loop gain in dB
	/// ccc.pp - open loop phase in degrees wrapped</summary>

	/// <summary>Loop transfer progress changed event arguments.</summary>
	public class LoopTransferProgressChangedEventArgs:ProgressChangedEventArgs {
		/// <summary>The LTF entry.</summary>
		public LoopTransferFunctionEntry LTFEntry;

		/// <summary>
		/// Initializes a new instance of the <see cref="TMCAnalyzer.LoopTransferProgressChangedEventArgs"/> class.</summary>
		/// <param name='ltfEntry'/>
		/// <param name='state'/>
		public LoopTransferProgressChangedEventArgs(LoopTransferFunctionEntry ltfEntry, object state) : base(0, state) {
			this.LTFEntry = ltfEntry;
		}
	}

	/// <summary>On loop transfer entry.</summary>
	public delegate void OnLoopTransferEntryDelegate(LoopTransferProgressChangedEventArgs e);

	/// <summary>Loop transfer complete event arguments.</summary>
	public class LoopTransferCompleteEventArgs:AsyncCompletedEventArgs {
		/// <summary>
		/// Initializes a new instance of the <see cref="TMCAnalyzer.LoopTransferCompleteEventArgs"/> class.</summary>
		/// <param name='e'/>
		/// <param name='canceled'/>
		/// <param name='state'/>
		public LoopTransferCompleteEventArgs(Exception e, bool canceled, object state) : base(e, canceled, state) { }
	}

	/// <summary>On loop complete.</summary>
	public delegate void OnLoopTransferCompleteDelegate(object sender, LoopTransferCompleteEventArgs e);

	public class LoopTransferFunctionEntry {
		/// <summary>The frequency.</summary>
		public double Frequency { get; }

		/// <summary>Magnitude in deciBels.</summary>
		public double Magnitude_dB { get; }

		/// <summary>The phase degrees, wrapped</summary>
		public double PhaseDegrees { get; }

		/// <summary>The coherence of measurement.</summary>
		public double Coherence { get; }

		///**********************************************************************************************************************/
		///// <summary>
		///// Initializes a new instance of the <see cref="TMCAnalyzer.LoopTransferFunctionEntry"/> class.</summary>
		/////      1040.0, 0.09, -48.90, 0.03 #
		///// <param name='line'/>
		//public LoopTransferFunctionEntry(string line) {
		//    try {
		//        var entries = line.Split(new char[] { ',' });
		//        var member_num = entries.GetUpperBound(0);
		//        if (member_num >= 3) { // [0 to 2]; 3 members, older firmware
		//            Frequency = double.Parse(entries[0]);
		//            Magnitude_dB = double.Parse(entries[1]);
		//            PhaseDegrees = double.Parse(entries[2]);
		//        } if (member_num >= 4) { // [0 to 3]; 4 members, newer firmware
		//            Coherence = double.Parse(entries[3]);
		//        } else { //error in string
		//            Frequency = -1.0;
		//            Magnitude_dB = -99.0;
		//            PhaseDegrees = 0;
		//            Coherence = -1.0;
		//        }
		//    } catch (System.Exception ex) {
		//        System.Diagnostics.Debug.WriteLine(ex);
		//    }
		//}

		/**********************************************************************************************************************/
		/// <summary>
		/// Initializes a new instance of the <see cref="TMCAnalyzer.LoopTransferFunctionEntry"/> class.</summary>
		/// Ex: 1040.0, 0.09, -48.90, 0.03 #
		///       Freq, Gain,  Phase, Coherence, end of TFline token
		/// <param name='line'/>
		public LoopTransferFunctionEntry(string line)
		{
			try
			{
				line = line.Replace("\t#", "");
				double tmpD;
				var entries = line.Split(new char[] { ',' });
				var member_num = entries.GetUpperBound(0);
				if (member_num < 3) { // less than 3 members, error in string
					Frequency = -1;
					Magnitude_dB = 0;
					PhaseDegrees = 0;
					Coherence = -1;
				} else { // [0 to 2] or [0 to 3], at least 3 members
					if (double.TryParse(entries[0], out tmpD))
						 Frequency = double.Parse(entries[0]);
					else Frequency = -2;	// the "-2" means cannot convert to double
					if (double.TryParse(entries[1], out tmpD))
						 Magnitude_dB = double.Parse(entries[1]);
					else Magnitude_dB = -99; // cannot convert to double
					if (double.TryParse(entries[2], out tmpD))
						 PhaseDegrees = double.Parse(entries[2]);
					else PhaseDegrees = 0;
				} 
				if (member_num >= 3) {// [0 to 3], at least 4 members - coherence is present
					if (double.TryParse(entries[3], out tmpD))
						 Coherence = double.Parse(entries[3]);
					else Coherence = -2;	// the "-2" means cannot convert to double
				}
			}
			catch (System.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// Initializes a new instance of the <see cref="TMCAnalyzer.LoopTransferFunctionEntry"/> class.</summary>
		/// <param name="TFfrequency", first real number; must always be present/>
		/// <param name="TFgain",     second real number; must always be present/>
		/// <param name="TFphase",     third real number; must always be present/>
		/// <param name="TFcoherence", fourth real number; might not be present with older firmware/>
		public LoopTransferFunctionEntry(double TFfrequency, double TFgain, double TFphase, double TFcoherence = -1.0)
		{
			this.Frequency = TFfrequency;
			this.Magnitude_dB = TFgain;
			this.PhaseDegrees = TFphase;
			//if (TFcoherence >= 0) this.Coherence = TFcoherence;
			//else this.Coherence = -1;
			this.Coherence = TFcoherence;
		}

		/// <summary>
		/// Returns a <code>string</code> that represents the current <see cref="TMCAnalyzer.LoopTransferFunctionEntry"/>.</summary>
		/// <returns>
		/// A <code>string</code> that represents the current <see cref="TMCAnalyzer.LoopTransferFunctionEntry"/>.</returns>
		public override string ToString()
		{
			return string.Format("[LoopTransferFunctionEntry: Frequency={0}, GainDB={1}, Phase={2}, Coherence={3}]",
					Frequency,
					Magnitude_dB,
					PhaseDegrees,
					Coherence);
		}
	}

	/// <summary>
	/// Analyzer.
	/// http://msdn.microsoft.com/en-us/library/wewwczdw.aspx
	/// </summary>
	public partial class TMCController {
		private object asyncOperationLocker = new object();
		private AsyncOperation getLoopTransferAsyncOperation;

		/// <summary>
		/// Used to marshall from the async context back to the context
		/// that can call the user
		/// </summary>
		private SendOrPostCallback onLoopTransferProgressChangedDelegate;
		private SendOrPostCallback onLoopTransferCompletedDelegate;

		private class LTFOperationStatus {
			public bool IsCancelled = false;
			public OnLoopTransferEntryDelegate OnLoopTransferEntry;
			public OnLoopTransferCompleteDelegate OnLoopTransferComplete;
			public LTFOperationStatus(OnLoopTransferEntryDelegate OnLoopTransferEntry,
					OnLoopTransferCompleteDelegate OnLoopTransferComplete) {
						this.OnLoopTransferEntry = OnLoopTransferEntry;
						this.OnLoopTransferComplete = OnLoopTransferComplete;
					}
		}

		private delegate void WorkerLTFEventHandler(OnLoopTransferEntryDelegate onLoopTransferEntry, AsyncOperation asyncOp);

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
		/// <param name='onLoopTransferEntry'>
		/// On loop transfer entry.
		/// </param>
		/// <param name='onLoopTransferComplete'>
		/// On loop transfer complete.
		/// </param>
		public void LoopTransferFunctionAsync(OnLoopTransferEntryDelegate onLoopTransferEntry, OnLoopTransferCompleteDelegate onLoopTransferComplete) {
			// Setup the marshaller from async completion context
			if (onLoopTransferProgressChangedDelegate == null) {
				onLoopTransferProgressChangedDelegate = new SendOrPostCallback(LoopTransferProgressChanged);
			}
			if (onLoopTransferCompletedDelegate == null) {
				onLoopTransferCompletedDelegate = new SendOrPostCallback(LoopTransferCompleted);
			}
			var ltfOperationStatus = new LTFOperationStatus(onLoopTransferEntry, onLoopTransferComplete);

			lock (asyncOperationLocker) {
				getLoopTransferAsyncOperation = AsyncOperationManager.CreateOperation(ltfOperationStatus);

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
				var workerDelegate = new WorkerLTFEventHandler(GetLoopTransferFunction);
				workerDelegate.BeginInvoke(onLoopTransferEntry, getLoopTransferAsyncOperation, null, null);
			}
		}

		/**********************************************************************************************************************/
		private bool TaskCancelled() {
			lock (asyncOperationLocker) {
				if (getLoopTransferAsyncOperation != null) {
					var ltfOperationStatus = getLoopTransferAsyncOperation.UserSuppliedState as LTFOperationStatus;
					return ltfOperationStatus.IsCancelled;
				} else { return true; }
			}
		}

		/**********************************************************************************************************************/
		private void GetLoopTransferFunction(OnLoopTransferEntryDelegate onLoopTransferEntry, AsyncOperation asyncOp) {
			// it is here on Worker Thread
			if (IsDemoMode) return;
			formMain.LTF_Lines_Counter = 0;
			bool UserCancelled = false; //value does not matter yet
			{
				var  cmd = string.Empty;
				var dataLine = string.Empty;
				var lineReadTimeout = new TimeSpan(0, 0, 31); // 31 seconds because taking measurement of 0.1 Hz for 3 periods takes 30 sec
				bool ValidValue = false;
				string first_chars = string.Empty;
				double ParsedValue = 0;
				do {
#if DEBUG
					prevLine = dataLine;
#endif
					dataLine = Device.ReadLineWithTimeout(lineReadTimeout);
					if (dataLine.Length > 4)
						first_chars = dataLine.Substring(0, 4); //mdr 112018 // 6);
					ValidValue = Double.TryParse(first_chars, out ParsedValue);
					if (ValidValue) { break; }
				} while (!dataLine.Equals(DoingCommandResponse));

				/*                     output is starting
				4750.0,   -1.73,  -59.98, -130.83,  -61.71, -136.72,   #
				4650.0,   -0.30,  -60.93, -130.66,  -61.23, -138.25,   #
				*/
				// read the expected lines in
				while (true) {
					UserCancelled = TaskCancelled();
					if (UserCancelled) break;

#if DEBUG
					prevLine = dataLine;
#endif
					if (ValidValue)
					{ goto got_line; } //we have read line in previous do { } while((!dataLine.Equals(DoingCommandResponse));
					Tools.msSleep(5);
					dataLine = Device.ReadLineWithTimeout(lineReadTimeout);
got_line:
					// see if we got an empty line or terminator line, if so thats the end of the sequence
					if(dataLine.Contains("*")) { break; }

					// if echo is enabled we'll see the command here again and we want
					// to just read the next line
					if (dataLine.Equals(""))  { dataLine = Device.ReadLineWithTimeout(lineReadTimeout); }
					else if (dataLine.Equals(cmd)) { dataLine = Device.ReadLineWithTimeout(lineReadTimeout); }
					else if (dataLine.Equals("\n")) { dataLine = Device.ReadLineWithTimeout(lineReadTimeout); } // left over from the previous line?
				check_line:
					if (dataLine.Length > 4) first_chars = dataLine.Substring(0, 4); //mdr 112018 // 6);
					ValidValue = Double.TryParse(first_chars, out ParsedValue);
					if (!ValidValue) {
						System.Diagnostics.Debug.WriteLine("Wrong LTF line: " + dataLine+ "len:" + dataLine.Length);
						dataLine = Device.ReadLineWithTimeout(lineReadTimeout); //read another line
						goto check_line;
					} else { //valid
						// convert the line into a loop transfer entry
						var loopTransferEntry = new LoopTransferFunctionEntry(dataLine);
						if (loopTransferEntry.Frequency != -1) { //conversion was OK
							formMain.LTF_Lines_Counter++;
							LoopTransferProgressChangedEventArgs e = new LoopTransferProgressChangedEventArgs(loopTransferEntry, getLoopTransferAsyncOperation.UserSuppliedState);
							asyncOp.Post(onLoopTransferProgressChangedDelegate, e);
						} else {
							ValidValue = false; //for break point
						}
						ValidValue = false; //prepare to read next one
					}
				}

				// if the ltf was cancelled then indicate to the controller that it should stop
				// sending ltf output

				if (UserCancelled) { // value comes from the above while(true) loop
					// stop the ltf command, we'll be relatively confident that
					// the command has stopped because FlushSerialPortInput() will
					// return true, indicating that the read buffer is empty
					do {
						// initiate the ltf command
						ControllerPort.WriteLine("ltf>stop");
					} while (!FlushSerialPortInput());
				}

				// indicate that the method has completed
				CompleteGetLoopTransferFunction(TaskCancelled(), asyncOp);
			}
		} // Worker thread ends here


#if DEBUG
				string prevLine = string.Empty;
#endif

		/**********************************************************************************************************************/
		/// <summary>
		/// Determines whether this instance cancel get loop transfer function.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance cancel get loop transfer function; otherwise, <c>false</c>.
		/// </returns>
		public void CancelLoopTransferFunction() {
			lock (asyncOperationLocker) {
				if (getLoopTransferAsyncOperation != null) {
					var ltfOperationStatus = getLoopTransferAsyncOperation.UserSuppliedState as LTFOperationStatus;
					ltfOperationStatus.IsCancelled = true;
				}
			}
		}

		/**********************************************************************************************************************/
		// called from main thread, NOT worker, but it is started by worker thread asyncOp.Post(onLoopTransferProgressChangedDelegate, e);
		private void LoopTransferProgressChanged(object operationState) {
			var ltfEntryArgs = operationState as LoopTransferProgressChangedEventArgs;
			var ltfOperationStatus = ltfEntryArgs.UserState as LTFOperationStatus;
			ltfOperationStatus.OnLoopTransferEntry(ltfEntryArgs);
		}

		/**********************************************************************************************************************/
		// called from main thread
		private void LoopTransferCompleted(object operationState) {
			var e = operationState as LoopTransferCompleteEventArgs;
			var ltfOperationStatus = e.UserState as LTFOperationStatus;
			ltfOperationStatus.OnLoopTransferComplete(this, e);
		}

		/**********************************************************************************************************************/
		// called from worker thread
		private void CompleteGetLoopTransferFunction(bool cancelled, AsyncOperation asyncOp) {
			var ltfOperationStatus = asyncOp.UserSuppliedState as LTFOperationStatus;
			var e = new LoopTransferCompleteEventArgs(null, cancelled, ltfOperationStatus);

			// end the task, the asyncOp object is responsible for marshaling the call
			asyncOp.PostOperationCompleted(onLoopTransferCompletedDelegate, e);
			//workerDelegate.EndInvoke (axis, onLoopTransferEntry, getLoopTransferAsyncOperation, null, null);
		}
	}
}