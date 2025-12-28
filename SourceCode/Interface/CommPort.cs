using System;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Windows.Forms;

// NOTE: It's really more of a MUTEX-ER than a MUX-ER
//
// Muxing (multiplexing) implies multiple streams of data going over the same thing at the same time.
// Mutex implies something that controls...so that two things don't use the same resource at the same time.
// ---
//
// SERIAL PORT MUXER IS A CLASS THAT PROVIDES EXTRA SERVICES AROUND THE NATIVE (SYSTEM.IO) SERIAL PORT:
//
// A MUXER object is created by passing it a regular System.IO serial port. You can then call the MUXER
// object and receive MUXED ports. Each MUXED port communicates with the base (System.IO) port via the
// MUXER object. The MUXER provides these special benefits to the MUXED ports:
//
// Each MUXED port can be configured as ONE of the following:
//
// NONE -- Act as a regular serial port.
//
// MIRROR DATA RECEIVE -- Data RECEIVED on other ports is also copied so that it is received on this port.
//
// ECHO TX DATA TO MUXED PORTS -- Data SENT from this port is copied to other ports that are set as "MIRROR" ports.
//
// ---
//
// EXCEPTION (ERROR) HANDLING BY SERIAL PORT MUXER:
//
// Search for "// GARY-COM-ERROR" to find all places in Muxer where I have wrapped the serial port with exception handling.
//
// Each serial-port access is wrapped with a TRY/CATCH block that raises the ComPortErrorEvent if a COM error happens.
//
// The following methods in this class (Muxer) access the serial-port and so have been wrapped with exception handling.
// Open, Read, ReadLine, BytesToRead,
// Write, WriteLine, Write (byte-array), Write (char-array)
//
// The following methods also access the serial-port but I have NOT wrapped them with exception handling:
// Close, IsOpen, BaudRate, PortName
namespace TMCAnalyzer {
	/// <summary>
	/// Serial port muxer, handles the muxing of data received from a SerialPort such that
	/// there can be multiple users of a single SerialPort instance without conflict.
	///
	/// Useful for having multiple accessors of a SerialPort without having to build a system
	/// to exchange data between these accessors</summary>
	public class SerPort {

		/// <summary>Initializes a new instance of the <see cref="SerPort"/> class.</summary>
		/// <param name='port'/>
		public SerPort(SerialPort port) { serialPortMuxerBase = port; }

		/// <summary>Initializes a new instance of the <see cref="SerPort"/> class.</summary>
		/// <param name='client'/>
		public SerPort(TcpClient client) { tcpClient = client; }    //telnet

		public SerialPort serialPortMuxerBase = null;
		public TcpClient tcpClient = null;                       //telnet
		public NetworkStream networkStream = null;

		/// <summary>Gets the baud rate.</summary>
		/// <value>serial port's Baudot rate (b/s)</value>
		public int BaudRate { get { return serialPortMuxerBase.BaudRate; } }

		private long ThisRxCount = 0;   // Rx buffer
		static object BufLocker = new object();

		/**********************************************************************************************************************/
		public string GetEntireBuffer { //called from DIFFERENT THREADS: Terminal.timerTrollForCharacters_Tick(), Controller.ControllerPortFlush(), Controller.SendSupport(),
			get {
				var retstr = BufferBase;
				string tstr = string.Empty;
				if (Program.FormMain.LTFIsRunning == true)  // mdr 112018
					return tstr;
				try {
					lock (BufLocker) {
						if (Program.FormSplash.IsPortUSB)
							tstr = ReadWholeBuffer(serialPortMuxerBase); //whatever in the serial port buffer;
						else {
							tstr = ReadWholeBufferFromTelnet(tcpClient);
						}
						if (ThisRxCount > 0) {
							retstr = BufferBase + tstr;
						} else if (ThisRxCount == 0) {
							retstr = BufferBase;
						} else {//negative ThisRxCount means error, handle
								//return if anything in the buffer
							retstr = BufferBase;
							// ADD Error handler
						}
					}
				} catch (System.Exception ex) {
					Debug.WriteLine(ex);
					//throw;
				} finally {
					BufferBase = string.Empty; // read clears GetEntireBuffer
				}
				return retstr;
			}
		}

		/**********************************************************************************************************************/
		public long BufLen { get { return BufferLenBase = BufferBase.Length; } }
		private string BufferBase = string.Empty;   // Rx buffer
		private long BufferLenBase = 0;   // Rx buffer

		/**********************************************************************************************************************/
		public string ReadLine(out int duration) {
			var indexOfLfCr = 0;
			string theLine = string.Empty;
#if DEBUG
		//	var tpm = System.TimeSpan.TicksPerMillisecond; // it is long and == 10000 (???? Always ???)
#endif
			var /*duration*/ dur = ((int)Tools.DebugTimer.Milliseconds);
			var hasReadLock = OperationReadTryLock();
			var tstr = string.Empty;
			if (!hasReadLock) {
				System.Diagnostics.Debug.WriteLine(">> TimeoutException - CommPort.ReadLine[108]");
				throw new System.TimeoutException();
			}
			try {
				Tools.DebugTimer = /*value irrelevant*/ TimeSpan.MinValue;
				do {
					indexOfLfCr = -1;
					if (Program.FormSplash.IsPortUSB)
						tstr = ReadWholeBuffer(serialPortMuxerBase); //try to read whatever in the serial port buffer;
					else
						tstr = ReadWholeBufferFromTelnet(tcpClient);
					if (ThisRxCount > 0) {
						BufferBase = BufferBase + tstr; // if get something, and add it to internal buffer
					}
					// read, return, and remove line from the buffer
					indexOfLfCr = BufferBase.IndexOf("\r\n");
					if (indexOfLfCr > 0) {
						theLine = BufferBase.Substring(0, indexOfLfCr);
						BufferBase = BufferBase.Substring(indexOfLfCr + 2); //cut buffer
					} else if (0 < (indexOfLfCr = BufferBase.IndexOf("\r\n"))) {
						theLine = BufferBase.Substring(0, indexOfLfCr);
						BufferBase = BufferBase.Substring(indexOfLfCr + 2); //cut buffer
					} else if (0 < (indexOfLfCr = BufferBase.IndexOf("\r"))) {
						theLine = BufferBase.Substring(0, indexOfLfCr);
						BufferBase = BufferBase.Substring(indexOfLfCr + 1); //cut buffer
					} else if (0 < (indexOfLfCr = BufferBase.IndexOf("\n"))) {
						theLine = BufferBase.Substring(0, indexOfLfCr);
						BufferBase = BufferBase.Substring(indexOfLfCr + 1); //cut buffer
					} else { // no Lf or Cr yet
						dur = ((int)Tools.DebugTimer.Milliseconds);
					}   // at 115200, one char duration is 87 us, send 80 chars string takes 7 ms.
				} while ((theLine == string.Empty) && (dur < 100));

			} catch (System.Exception ex) {
				indexOfLfCr++;// to put break pointer
				throw ex;
			} finally {
				OperationReadUnlock();
				duration = dur;
			}
			// theLine = theLine.Trim(); //sometimes it starts from \n; like "\n2250.0,   -0.92,  -29.45,   -9.93,  -30.37,   18.37, \t#"
			//catch alarms
			if (theLine.ToLowerInvariant().Contains("*alarm")) {
				formMain fm = new formMain(); //new object reference to formMain
											  // fm.ProcessAlarm(theLine);
			}
			return theLine;
		}

		/**********************************************************************************************************************/
		internal int BytesToRead(SerPort muxed) {
			var /*is read locked?*/ isLck = OperationReadTryLock();
			var /*count of bytes available to be read*/ cnt = -1;
			if (/*lock taken?*/ isLck) {
				try {
					if (Program.FormSplash.IsPortUSB)
						cnt = serialPortMuxerBase.BytesToRead;
					else
						cnt = tcpClient.ReceiveBufferSize;
				} catch (Exception ex) {
					Debug.Assert(cnt == 0);
					if (ex is System.IO.IOException || ex is System.InvalidOperationException) {
						COMPortErrorFromAnyFunctionAccessingSerialPort();
					}
				} finally { OperationReadUnlock(); }
			}
			Debug.Assert(cnt < (long)int.MaxValue);
			return (int)cnt;
		}

		/**********************************************************************************************************************/
		internal void Close() {
			try {
				//if (Program.FormSplash.IsPortUSB)
				//{
				Program.StopIfTestRunning();
				//Program.DisconnectScope();
				if (serialPortMuxerBase != null && serialPortMuxerBase.IsOpen) {
					serialPortMuxerBase.Close();
					//if (Program.FormScope != null) {
						//if (TMC_Scope.TMCScopeController.portUSB.PortName.Equals(serialPortMuxerBase.PortName))
						//TMC_Scope.Program.MainFormComPortIsClosed = true;
					//}
				}
				//}
				//else
				//{
				if (networkStream != null) { networkStream.Close(); }
				if (tcpClient != null) { tcpClient.Close(); }
			} catch (System.InvalidOperationException ex) {
				COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
			} catch (System.IO.IOException ex) {
				COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
			} catch (System.UnauthorizedAccessException ex) {
				COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
			} catch (System.Exception ex) { throw ex; }
		}

		// GARY-CHANGE:
		// Create an actual event from the delegate---
		public event ComPortErrorEventHandler_FunctionPointer ComPortError_Event;

		// GARY-CHANGE:
		// Setup a DELEGATE (Function-Pointer) that clients of this class can use to register their own
		// event handler method (function) with us. This method will be called if we detect an error with
		// the COM port.
		// public delegate void ComPortErrorEventHandler(object sender, EventArgs e);
		public delegate void ComPortErrorEventHandler_FunctionPointer(string WhereCommunicationFailed, string MoreInfoAboutWhenCommunicationFailed);

		/**********************************************************************************************************************/
		private void COMPortErrorFromAnyFunctionAccessingSerialPort(System.Exception ex = null) {
			if (ComPortError_Event != null) {
				if (ex == null) {
					ComPortError_Event(string.Empty, string.Empty);
				} else {
					ComPortError_Event(ex.ToString(), string.Empty);
				}
			}
		}

		/// <summary>Gets whether this port is open.</summary>
		/// <value><c>true</c> if this instance is open; otherwise, <c>false</c>.</value>
		internal bool IsOpen {
			get {
				if (Program.FormSplash.IsPortUSB)
					return serialPortMuxerBase.IsOpen;
				else
					return tcpClient.Client.IsBound;
			}
		}

		/**********************************************************************************************************************/
		internal void Open() {
			try {
				if (Program.FormSplash.IsPortUSB) {
					serialPortMuxerBase.Close();
					serialPortMuxerBase.Open();
					//if (!Program.DualPortController)
					//	TMC_Scope.TMCScopeController.portUSB = serialPortMuxerBase;  //ScopeIntegration_20082018_Tismo
					//if (Program.FormScope != null) {
					//	TMC_Scope.Program.mainUsbPortCopy = serialPortMuxerBase;
					//	//if (TMC_Scope.TMCScopeController.portUSB.PortName.Equals(serialPortMuxerBase.PortName))
					//	TMC_Scope.Program.MainFormComPortIsClosed = false;
					//}
				} else {
					if (!tcpClient.Client.IsBound) {
						int port = (String.IsNullOrEmpty(Program.FormSplash.txtPort.Text) || !formMain.IsNumeric(Program.FormSplash.txtPort.Text)) ? 0 : Convert.ToInt32(Program.FormSplash.txtPort.Text);
						tcpClient.Connect(Program.FormSplash.txtHost.Text, port);
					}
					networkStream = tcpClient.GetStream();   ///open stream
					//Thread t = new Thread(() => OpenIPThread());
					//t.Start();
				}
			} catch (Exception ex) {
				if (ex is System.IO.IOException || ex is System.InvalidOperationException || ex is System.UnauthorizedAccessException) {
					COMPortErrorFromAnyFunctionAccessingSerialPort();
					throw;
				} else { throw; }
			}
		}
		public bool OpenIPThread() {

			try {
				if (!tcpClient.Client.IsBound)
					tcpClient.Connect(Program.FormSplash.txtHost.Text, Convert.ToInt32(Program.FormSplash.txtPort.Text));
				networkStream = tcpClient.GetStream();   ///open stream
			} catch (Exception ex) {

				if (ex is System.IO.IOException || ex is System.InvalidOperationException || ex is System.UnauthorizedAccessException) {
					COMPortErrorFromAnyFunctionAccessingSerialPort();
					throw;
				} else { throw; }
			}
			return true;
		}
		private object operationReadLockBase = new object();

		internal void OperationReadLock() {
#if USE_SERIAL_LOCKS
Monitor.Enter(operationReadLockBase);
#endif
		}
		internal bool OperationReadTryLock() {
#if !USE_SERIAL_LOCKS
			return true;
#else
var /*lock is taken?*/ isLkd = false;
Monitor.TryEnter(operationReadLockBase, ref isLkd);
return isLkd;
#endif
		}
		internal void OperationReadUnlock() {
#if USE_SERIAL_LOCKS
Monitor.Exit(operationReadLockBase);
#endif
		}

		private object operationWriteLockBase = new object();
		private int bytesTempBuffMax = 1024;

		internal void OperationWriteLock() {
#if USE_SERIAL_LOCKS
Monitor.Enter(operationWriteLockBase);
#endif
		}
		internal bool OperationWriteTryLock() {
#if !USE_SERIAL_LOCKS
			return true;
#else
var /*lock is taken?*/ isLkd = false;
Monitor.TryEnter(operationReadLockBase, ref isLkd);
return isLkd;
#endif
		}
		internal void OperationWriteUnlock() {
#if USE_SERIAL_LOCKS
Monitor.Exit(operationWriteLockBase);
#endif
		}
		/// <summary>
		/// Locks the muxed serial port write lock, must call WriteUnlock() to unlock</summary>
		public void WriteLock() {
			OperationWriteLock();
		}

		/// <summary>Access write timout</summary>
		public int WriteTimeout
		{
			get
			{

				return serialPortMuxerBase.WriteTimeout;

			}
			set
			{
				if (Program.FormSplash.IsPortUSB)
					serialPortMuxerBase.WriteTimeout = value;
			}
		}

		/// <summary>Unlocks the serial port write lock</summary>
		public void WriteUnlock() {
			OperationWriteUnlock();
		}

		/**********************************************************************************************************************/
		/// <summary>Gets the description of the port.</summary>
		/// <returns>description of port</returns>
		internal string PortDescription
		{
			get
			{
				var /*port name*/ nam = serialPortMuxerBase.PortName;
				try {
					const string /*caption*/ CPN = "Caption";
					var /*seeker*/ skr = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
					foreach (var /*query object*/ qry in skr.Get()) {
						if (/*found?*/ qry[CPN].ToString().Contains(nam)) {
							return qry[CPN].ToString();
						}
					}
				} catch (System.Management.ManagementException ex) {
					return "invalid description " + Tools.TextTidy(ex.ToString());
				}
				return "description not found";
			}
		}

		/// <summary>Gets the name of the port.</summary>
		/// <returns>name of port</returns>
		internal string PortName
		{
			get
			{
				if (Program.FormSplash.IsPortUSB)
					return serialPortMuxerBase.PortName;
				else
					return Program.FormSplash.txtHost.Text;
			}
		}

		/**********************************************************************************************************************/
		/// <summary>Read the whole buffer</summary>
		/// <param name="buffer"/>
		/// <param name="count"/>
		/// <param name="offset"/>
		/// <param name="reader"/>
		internal string ReadWholeBuffer(SerialPort rdr) { //called from Main Thread
			var /*tally/total bytes read (returned)*/ tly = 0;
			var outstr = string.Empty;
			try {
				if (!rdr.IsOpen) {
					try {
						rdr.Open();
					} catch (System.Exception ex) { System.Diagnostics.Debug.Assert(ex != null); }
				}
				var /*is lock acquired?*/ isLkd = OperationReadTryLock();

				// if the readlock wasn't able to be acquired then stop here
				// as we don't have permission to read from the physical device
				if (isLkd) {
					try {
						// Read all we can from comm port buffer
						//if (Program.ValidateScopeCheck) {
						//	outstr = GetSplitDataFromHighSpeed();
						//} else {
							outstr = rdr.ReadExisting();
						//}
						tly = outstr.Length;
					} catch (System.InvalidOperationException ex) {
						COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
						tly = -4;
					} catch (System.IO.IOException ex) {
						COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
						tly = -3;
					} catch (System.TimeoutException ex) {
						System.Diagnostics.Debug.WriteLine(">> TimeoutException - CommPort.Read[312]" + ex);
						tly = -2;
					} catch (System.Exception ex) {
						if (ex.Message.Contains("is closed")) {
							tly = -1;
						}
						throw ex;
					}
				} else { /*error, is port is busy?*/
					tly = -3;
				}
			} catch (System.Exception ex) { throw ex; }
			ThisRxCount = tly;
			return outstr;
		}

#if false
public string GetSplitDataFromHighSpeed() {
		string recievedData = string.Empty;
			byte[] bytesTempBuf = new byte[bytesTempBuffMax];
			int byteCnt = 0;
			int qLen = 0;
			//lock (ComPortBusy)
			if (Program.FormScope.InvokeRequired) {
				Program.FormScope.Invoke((System.Action)(() =>
				{
					byteCnt = Program.FormScope.ReadToPrint(ref bytesTempBuf);
					qLen = Program.FormScope.mainFormQueue.Count;
				}));
			} else {
				byteCnt = Program.FormScope.ReadToPrint(ref bytesTempBuf);
				qLen = Program.FormScope.mainFormQueue.Count;
			}
			if (qLen > 0) {
				lock (Program.FormScope.mainFormQueue) {
					for (int i = 0; i < qLen; i++) {
						//if (byteCnt >= bytesTempBuf.Length)
						//    break;
						bytesTempBuf[byteCnt++] = Program.FormScope.mainFormQueue.Dequeue();
					}
					//  Program.FormScope.mainFormQueue.Clear();
					bytesTempBuf = Program.TrimEnd(bytesTempBuf);

				}
			}
			bytesTempBuf = Program.TrimEnd(bytesTempBuf);
			recievedData = bytesTempBuf.Length > 0 ? Encoding.ASCII.GetString(bytesTempBuf) : string.Empty;
			return recievedData;
		}
#endif
		/// <summary>Read the whole buffer</summary>
		/// <param name="buffer"/>
		/// <param name="count"/>
		/// <param name="offset"/>
		/// <param name="reader"/>
		internal string ReadWholeBufferFromTelnet(TcpClient rdr) { //called from Main Thread
			var /*tally/total bytes read (returned)*/ tly = 0;
			var outstr = string.Empty;
			int byte_cnt = 0;
			byte[] byteArrayData = new byte[4096];
			List<byte> receivedData = new List<byte>();
			try {
				var /*is lock acquired?*/ isLkd = OperationReadTryLock();

				// if the readlock wasn't able to be acquired then stop here
				// as we don't have permission to read from the physical device
				if (isLkd) {
					try {
						if (networkStream.DataAvailable) {
							byte_cnt += networkStream.Read(byteArrayData, byte_cnt, 4096);
							for (int i = 0; i < byteArrayData.Length; i++) {
								if (byteArrayData[i] != 0)
									receivedData.Add(byteArrayData[i]);
								else
									break;
							}
							outstr = System.Text.Encoding.UTF8.GetString(receivedData.ToArray());
						}
						// Read all we can from telnet port buffer
						// outstr = rdr.ReadExisting();
						tly = outstr.Length;
					} catch (System.InvalidOperationException ex) {
						COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
						tly = -4;
					} catch (System.IO.IOException ex) {
						COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
						tly = -3;
					} catch (System.TimeoutException ex) {
						System.Diagnostics.Debug.WriteLine(">> TimeoutException - CommPort.Read[312]" + ex);
						tly = -2;
					} catch (System.Exception ex) {
						if (ex.Message.Contains("is closed")) {
							tly = -1;
						}
						throw ex;
					}
				} else { //error, is port is busy?
					tly = -3;
				}
			} catch (System.Exception ex) { throw ex; }
			ThisRxCount = tly;
			return outstr;
		}


		/**********************************************************************************************************************/
		internal int ReadTimeout
		{
			get
			{
				if (Program.FormSplash.IsPortUSB)
					return serialPortMuxerBase.ReadTimeout;
				else
					return tcpClient.Client.ReceiveTimeout;
			}
			set
			{
				if (Program.FormSplash.IsPortUSB)
					serialPortMuxerBase.ReadTimeout = value;
				else
					tcpClient.Client.ReceiveTimeout = value;
			}
		}

		/// <summary>Write to the port "writer" string "text"
		/// returns mucrosecond taken
		/// </summary>
		/// <param name="writer"/>
		/// <param name="text"/>

		/**********************************************************************************************************************/
		public double Write(string text) { //called from Main Thread
			var stopwatch = new Stopwatch();
			var sWatchTick_to_us = Program.StopWatchTick_to_us;
			double test_time_write = 0.0;
			string resp = string.Empty;

			stopwatch.Start();
			try {
				OperationWriteLock();
				try {
					if (Program.FormSplash.IsPortUSB) {
						//if (Program.ValidateScopeCheck) {
						//	Program.FormScope.Send(text, ref resp, TMC_Scope.CommandAckTypes.NoAckExpected);
						//} else {
							serialPortMuxerBase.Write(text);
//						}
					} else {
						byte[] ByteCmd = System.Text.Encoding.ASCII.GetBytes(text);
						tcpClient.Client.Send(ByteCmd);
						//Thread.Sleep(20);
					}
				} catch (Exception ex) {
					if (ex.Message.Contains("is closed")) {
						// MessageBox.Show("Port Closed" + serialPort.PortName , "Port Closed", MessageBoxButtons.OK);
						stopwatch.Stop();
						test_time_write = (sWatchTick_to_us * stopwatch.ElapsedTicks);//write time
						return test_time_write;
					}
					if (ex is System.IO.IOException || ex is System.InvalidOperationException) {
						COMPortErrorFromAnyFunctionAccessingSerialPort();
					} else {
						throw;
					}
				}
			} catch { throw; } finally { stopwatch.Stop(); OperationWriteUnlock(); }
			test_time_write = (sWatchTick_to_us * stopwatch.ElapsedTicks);//write time
			var timing = "Written text ='" + text + "'; timing: writing to port =" + ((long)test_time_write).ToString() + " us";
			System.Diagnostics.Trace.WriteLine(timing);
			return test_time_write;
		}



		/**********************************************************************************************************************/
		/// <summary>Write to the port "writer" string "text" terminated by "r\n"
		/// returns mucrosecond taken
		/// </summary>
		/// <param name="writer"/>
		/// <param name="text"/>
		public double WriteLine(string text) {
			text = text + "\r\n";
			return Write(text);
		}

		/**********************************************************************************************************************/
		public double WriteChar(char chr) {
			string text = string.Empty;
			text += chr;
			return Write(text);
		}

		/**********************************************************************************************************************/
		/// <summary>Read the specified buffer, offset and count.</summary>
		/// <param name='buffer'/>
		/// <param name='offset'/>
		/// <param name='count'/>
		public int Read(byte[] buffer, int offset, int count) {
#if EXTENDED_DEBUGGING
log.DebugFormat("offset {0}, count {1}", offset, count);
#endif
			return Read(this, buffer, offset, count);
		}

		/**********************************************************************************************************************/
		/// <summary>Read the specified muxed, buffer, offset and count.</summary>
		/// <param name="buffer"/>
		/// <param name="count"/>
		/// <param name="offset"/>
		/// <param name="reader"/>
		internal int Read(SerPort reader, byte[] buffer, int offset, int count) {
			var /*tally/total bytes read (returned)*/ tly = 0;
			if (!reader.IsOpen) {
				try {
					reader.Open();
				} catch (System.Exception ex) { System.Diagnostics.Debug.Assert(ex != null); }
			}
			var /*is lock acquired?*/ isLkd = OperationReadTryLock();
			try {   //ensure unlock in finally

				// if the readlock wasn't able to be acquired then stop here
				// as we don't have permission to read from the physical device
				if (isLkd) {
					if (count > 0) {
						// and read the rest of the data directly from the serial port
						var /*buffer*/ bfr = new byte[count];
						try {
							// The original line was: var bytesRead = serialPort.Read(dataRead, 0, count);
							var /*read count*/ cnt = serialPortMuxerBase.Read(bfr, 0, count); //exception: bfr=0?
							System.Array.Copy(bfr, 0, buffer, offset + tly, cnt);
							tly += cnt;
						} catch (System.InvalidOperationException ex) {
							COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
						} catch (System.IO.IOException ex) {
							COMPortErrorFromAnyFunctionAccessingSerialPort(ex);
						} catch (System.TimeoutException ex) {
							System.Diagnostics.Debug.WriteLine(">> TimeoutException - SerialPortMuxer.Read[424" + ex);
							return -2;

						} catch (System.Exception ex) {
							if (ex.Message.Contains("is closed")) {
								return -1;
							}
							throw ex;
						}
					}
				}
			} catch (System.Exception ex) {
				throw ex;
			} finally {
				OperationReadUnlock();
			}
			return tly;
		}
	}
}