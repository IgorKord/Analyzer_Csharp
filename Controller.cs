using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Numerics;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
namespace TMCAnalyzer {
	/// <summary/>


	// FORMER IController - Interface to controller file
	//=== DELEGATE FUNCTIONS DEFINED
	/// <summary>On scope data captured delegate.</summary>
	public delegate void OnScopeDataCapturedDelegate(byte[] data, int dataLength);

	/// <summary>On data processing starting delegate.</summary>
	public delegate void OnFFTBegin(List<DataSample> samples, int sampleElementCount);

	/// <summary>Called when a data sample is found</summary>
	/// <param name="dataSample"></param>
	public delegate void OnDataSampleFoundDelegate(DataSample dataSample);

	/// <summary>On data processing complete delegate.</summary>
	public delegate void OnFFTEnd(Complex[] result, int channel); // GARY-FFT-CHANGE

	/// <summary>
	/// High level controller class, manages the equipment state etc.
	///
	/// NOTE: When in scope mode data must be read in order to ensure that
	/// outgoing commands are handled or processed. Not sure whether the issue is on the
	/// PC or system side. Reading data is taken care of automatically by this class.
	///
	/// NOTE: Commands usually return ">~OK" but some commands that take a while to execute
	/// will return ">~Doing CMD". All subsequent commands will continue to return doing command
	/// until the command has been finished. One such long command is the 'mode' command.</summary>
	///
	public partial class TMCController {
		/// <summary>
		/// Occurs when scope data is captured, this is raw serial data</summary>
		public event OnScopeDataCapturedDelegate OnScopeDataCaptured;

		/// <summary>
		/// Called when a data sample is found</summary>
		public event OnDataSampleFoundDelegate OnDataSampleFound;

		/// <summary>
		/// Occurs when a data set is ready and being issued for processing</summary>
		public event OnFFTBegin OnDataProcessingStarting;

		/// <summary>
		/// Occurs when on data processing complete.</summary>
		public event OnFFTEnd OnDataProcessingComplete;


		public delegate void OnSampleReadyDelegate(DataSample dataSample);
		//public event OnSampleReadyDelegate OnSampleReady = null;

		/**********************************************************************************************************************/
		/// <summary>
		/// Initializes a new instance of the <see cref="TMCController"/> class.</summary>
		/// <param name='muxedPort'>The port to communicate over</param>
		public TMCController(SerPort muxedPort) {
			if ( muxedPort == null) { throw new System.ArgumentNullException(); }//bad?
			//log.DebugFormat(System.Reflection.MethodBase.GetCurrentMethod().Name + "(" + System.Reflection.MethodBase.GetCurrentMethod().GetParameters()[0].Name + "==" + muxedPort.Dump + ")");
			ControllerPort = muxedPort;
			ControllerPort.ReadTimeout = ControllerPort.WriteTimeout = 100; //ms
/*
			// GARY-FIX:    Why is MinimumBytesPerRetrieval = 256??
			//              This means the SerialPortDataReader object will not report anything until (256/6=42) 42 samples are found.
			//              Example:    On the 10 milli-second setting, we show only 20 samples on the screen.
			//                          If we aren't even reporting less than 42 samples, many samples are not being shown at all.
			const int  RMn = 256;// minimum bytes per retrieval
			if (ControllerPort.PortName != Program.ControllerPortDouble) {
				serialPortDataReader = new SerialPortDataReader(ControllerPort, RMn);
			} else { serialPortDataReader = new SerialPortDataReader(Program.SerialPortData, RMn); }
			serialPortDataReader.OnDataCaptured += HandleSerialPortDataReaderOnDataCaptured;*/
		}

		/**********************************************************************************************************************/
		static readonly object LockObj = new object();
		public int BaudRate { get { return ControllerPort.BaudRate; } }

		[System.ComponentModel.DefaultValue(null)]
		public ChannelCollection Channels { get; /*set in Open*/ private set; }

		/**********************************************************************************************************************/
		public void Close() {
			try {
				ControllerPort.Close();
			} catch (System.Exception ex) {
				throw ex;
			} finally { CommStatus = CommStatusStates.NotConnected; }
		}

		/**********************************************************************************************************************/
		internal AckTypes CommandDEMO(string command, CommandTypes commandType, out string response, CommandAckTypes ackType) {
			System.Diagnostics.Debug.Assert(command != null);
			System.Diagnostics.Debug.Assert(Tools.IsOK(commandType, /*just check*/ false));
			response = string.Empty;
			System.Diagnostics.Debug.Assert(Tools.IsOK(ackType, /*just check*/ false));
			var /*is a command?*/ isCmd = false;
			var /*length*/ len = 0;
			var /*position*/ pos = 0;
			var /*command token*/ tkn = string.Empty;
			var /*text*/ txt = string.Empty;
			if (command.Contains("alarm>o")) {  //command setting status
				len = command.Length;
				isCmd = true;
			} else if (command.Contains("alarm>")) {    //query of status
				len = 6;
			} else if (command.Contains("alarm?")) {   //query of value
				len = 6;
				command = "alarm="; //but search for"alarm="
			} else if (command.Contains("alarm=")) {   //command setting value
				len = 6;
				isCmd = true;
			} else {
				pos = command.IndexOf("?"); //inspect string - is it query?
				len = command.Length;
				if (pos < 0) {  //no '?'
					pos = command.IndexOf(">"); //inspect string - is it query?
					var /*index*/ idx = command.IndexOf("=");
					if ((pos + idx) > 0) { //it is a query, no "=" or ">"
						isCmd = true;
					}
				} else {    //there is '?', check is in alarm>?
					if (command.IndexOf(">?") > 0) {    //inspect string - is it query?
						pos++; //yes this query 'alarm>?'
					}
					// ...no NOT this query 'alarm>?'
					len = pos;  //use  string without '?'
				}
			}
			tkn = command.Substring(0, len).ToLower(System.Globalization.CultureInfo.InvariantCulture);
			var /*RCI command size*/ siz = RCIcommands.Count();
			for (var /*line number*/ lNo = 0; lNo < siz; lNo++) {
				txt = RCIcommands[lNo];
				if (txt.Substring(0, len).ToLower(System.Globalization.CultureInfo.InvariantCulture).Contains(tkn)) {
					if (!isCmd) {   //it is a query, no "=" or ">"
						response = RCIcommands[lNo].ToLower(System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine + OkResponse;
						pos = response.IndexOf(" //");
						if (/*trim verbose echo?*/ pos > 0) { response = response.Substring(0, pos); }
						return AckTypes.Ok;
					}
					// ...else it is a command
					Parce_CMD(command, RCIcommands[lNo], out RCIcommands[lNo]);
					break;
				}
			}
			response = OkResponse;
			return AckTypes.Ok;
		}

		private object commandLock = new object();

		/**********************************************************************************************************************/
		/// <summary/>
		[System.ComponentModel.DefaultValue(CommStatusStates.NotConnected)]
		public CommStatusStates CommStatus
		{
			get { return communicationsStatusBase; }
			set
			{
				Tools.IsOK(value, /*exception*/ true);
				communicationsStatusBase = value;
				string spd = string.Empty;
				string baudRate = string.Empty;
				if (Program.FormSplash.InvokeRequired)
					Program.FormSplash.BeginInvoke((MethodInvoker)delegate { baudRate = Program.FormSplash.s_Baud.Text; });
				else
					baudRate = Program.FormSplash.s_Baud.Text;
				if (Program.FormMain != null) {
					var /*port identity*/ idy = (Program.ConnectedController == null) ? "unknown" : (string.IsNullOrWhiteSpace(Program.ConnectedController.PortName) ? "unidentified" : Program.ConnectedController.PortName);
					if (Program.FormSplash.IsPortUSB)
						spd = (Program.ConnectedController == null) ? string.Empty : (string.IsNullOrWhiteSpace(Program.ConnectedController.PortName) ? string.Empty : (" @ " + baudRate + "b/s"));
					switch (communicationsStatusBase) {
						case CommStatusStates.CommError:
						Program.FormMain.ConnectionStatus("Communications fault on " + idy + spd, System.Drawing.Color.Black, System.Drawing.Color.Chartreuse);
						Program.ConnectedController.Close();
						if (Program.FormMain.ToggleCOMconnection.InvokeRequired)
							Program.FormMain.ToggleCOMconnection.BeginInvoke((MethodInvoker)delegate
							{
								Program.FormMain.ToggleCOMconnection.Checked = false;
							});
						else { Program.FormMain.ToggleCOMconnection.Checked = false; }
						break;
						case CommStatusStates.Connected:
						Program.FormMain.ConnectionStatus("Connected to " + idy + spd, System.Drawing.Color.Black, System.Drawing.Color.Chartreuse);
						Program.FormMain.ConnectionStatusSet(idy + " is open", true);
						if (Program.FormMain.ToggleCOMconnection.InvokeRequired)
							Program.FormMain.ToggleCOMconnection.BeginInvoke((MethodInvoker)delegate
							{
								Program.FormMain.ToggleCOMconnection.Checked = true;
							});
						else { Program.FormMain.ToggleCOMconnection.Checked = true; }
						break;
						case CommStatusStates.DemoMode:
						Program.FormMain.ConnectionStatus("DEMO Mode", System.Drawing.Color.Black, System.Drawing.Color.Orange);
						Program.FormMain.ConnectionStatusSet("Demo mode is open", true);
						if (Program.FormMain.ToggleCOMconnection.InvokeRequired)
							Program.FormMain.ToggleCOMconnection.BeginInvoke((MethodInvoker)delegate
							{
								Program.FormMain.ToggleCOMconnection.Checked = true;
							});
						else { Program.FormMain.ToggleCOMconnection.Checked = true; }
						break;
						case CommStatusStates.NotConnected:
						Program.FormMain.ConnectionStatus("disconnected", System.Drawing.Color.Black, System.Drawing.Color.Orange);
						Program.FormMain.ConnectionStatusSet(idy + " is Closed", false);
						if (Program.FormMain.ToggleCOMconnection.InvokeRequired)
							Program.FormMain.ToggleCOMconnection.BeginInvoke((MethodInvoker)delegate
							{
								Program.FormMain.ToggleCOMconnection.Checked = false;
							});
						else { Program.FormMain.ToggleCOMconnection.Checked = false; }
						break;
					}
				}
			}
		}
		private CommStatusStates communicationsStatusBase = CommStatusStates.NotConnected;

		/**********************************************************************************************************************/
		/// <summary>Get the controller's communication port.</summary>
		/// <value><see cref="SerPort"/></value>
		[System.ComponentModel.DefaultValue(null)]
		public static SerPort ControllerPort { get; /*set in _ctor*/ set; }

		// ...used to prevent infinite recursion...
		[System.ComponentModel.DefaultValue(false)]
		public bool DemoIsBusy { get; private set; }

		/// <summary>Gets whether or not in DEMO mode</summary>
		/// <returns><code>true</code> if in DEMO mode; <code>false</code> otherwise</returns>
		public bool IsDemoMode { get { return (ControllerPort != null) && (ControllerPort.PortName == "DEMO"); } }

		/// <summary>Detects the devices by searching all available serial ports</summary>
		/// <returns>The devices.</returns>
		public static List<DetectedDeviceEntry> DetectDevices(int[] BaudRates) {
			// GARY-NOTE:   It seems there could be a faster way to search all the COM ports and all the BAUD
			//              rates to find controllers. I'm thinking that it could save the last COM & BAUD and
			//              put that up as the initial / default that you want to connect to. Then it could
			//              start a seperate thread that goes and searched for other contollers.
			//
			//              ALSO: Once it has found a controller at a certain BAUD on a a PORT, no need to keep
			//              looking for a controller at the other speeds on that port.
			//
			//              ALSO: Do we really need the overhead of MUXER when we're just starting up and looking
			//              for Controllers? Would it be quicker to use simple serial communication at this early
			//              stage?
			var /*devices*/ dvc = new List<DetectedDeviceEntry>();
			TMCController /*controller/controller*/ ctl = null;        // GARY-FIX: this should be "controller"
			foreach (var /*port*/ prt in SerialPort.GetPortNames()) {
				ctl = null;

				//                string PortDESCRIPTIONS = "COMx";
				// GARY-DEBUG: I added this line so that when searching for Controllers it only really looks
				//              on COM4. This is because my PC has 17 COM ports and searching all of them
				//              at each baud rate takes just way too long.
				//  if (port != "COM4") continue;        // GARY-REMOVE:   Remove this line.
				foreach (var /*baud rate*/ rat in BaudRates) {
					try {
						var /*serial port*/ sPt = new SerialPort(prt, rat);
						var /*muxer*/ mux = new SerPort(sPt);
						var /*port description*/ dsc = mux.PortDescription;
						mux.ReadTimeout = mux.WriteTimeout = 100 /*ms*/;
						ctl = new TMCController(mux); // LOCATION-TAG: CONTROLLER-OBJECT-CREATED
						ctl.Open();

						// Retrieve the version
						var /*version*/ ver = ctl.FirmwareVersion;
						dvc.Add(new DetectedDeviceEntry(prt, rat));
						ctl.Close();
					} catch (System.Exception) {
						if (ctl != null) { ctl.Close(); }
						continue;
					}
				}
			}
			return dvc;
		}

		/**********************************************************************************************************************/
		/// <summary>Releases all resource used by the <see cref="TMCController"/> object.</summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="TMCController"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="TMCController"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="TMCController"/> so
		/// the garbage collector can reclaim the memory that the <see cref="TMCController"/> was occupying.</remarks>
		public void Dispose() {
			if (!WhenCommIsTakenDontTry)
				lock (commandLock) {
					// Stop all of the background threads.
					if(serialPortDataReader!=null) serialPortDataReader.Stop();
					Close();
				}
		}

		/// <summary>Serial response from the system if a command is being performed</summary>
		public const string DoingCommandResponse = ">~Doing CMD";

		/**********************************************************************************************************************/
		/// <summary/>
		public EchoStates EchoState
		{
			get { return echoStateBase; }
			set
			{
				lock (commandLock) {
					/* Commented by Tismo_23/08/2018. Since it looks redundant as DC2020 scope is integrated by tismo
					if (Program.Scope_on_Old_Controller == true)
						throw new System.InvalidOperationException("can't set in ScopeCapture mode");
					/***************************************************************************************************/

					// Change the echo state before sending the new command.
					echoStateBase = value;
					var /*echo command*/ cmd = String.Format("echo>{0}", (value == EchoStates.Enabled) ? "enabled" : "disabled");
					Send(cmd, CommandAckTypes.NoAckExpected);
				}
			}
		}
		private EchoStates echoStateBase = EchoStates.Unknown;

		/**********************************************************************************************************************/
		/// <summary/>
		public double ExcitationAmplitude
		{
			get
			{
				// Response looks like "ampl=0.100"
				var /*response*/ rsp = string.Empty;
				SendInternal("ampl?", CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

				// Trim 'ampl=' from the start of the response and parse the value.
				const string /*text to be trimmed*/ TRM = "ampl=";
				rsp = rsp.Substring(TRM.Length, rsp.Length - TRM.Length);
				var /*value*/ vlu = double.Parse(rsp, System.Globalization.CultureInfo.InvariantCulture);
				return vlu;
			}
			set
			{
				if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
					if ((value < ControllerLimits.ExcitationAmplitudeMinValue) || (value > ControllerLimits.ExcitationAmplitudeMaxValue))
						throw new System.InvalidOperationException("value out of range");
					var /*command*/ cmd = String.Format("ampl={0:F4}", value);
					Send(cmd, CommandAckTypes.AckExpected);
				}
			}
		}

		/**********************************************************************************************************************/
		/// <summary/>
		public double ExcitationFrequency
		{
			get
			{
				// Response is like "freq=4000.000"
				var /*response*/ rsp = string.Empty;
				SendInternal("freq?", CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

				// Trim "freq=" from the start of the response and parse the value.
				const string /*text to be trimmed*/ TRM = "freq=";
				rsp = rsp.Substring(TRM.Length, rsp.Length - TRM.Length);
				var /*value*/ vlu = double.Parse(rsp, System.Globalization.CultureInfo.InvariantCulture);
				return vlu;
			}
			set
			{
				if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
					Tools.IsOK(value, ControllerLimits.ExcitationFrequencyMinValue, ControllerLimits.ExcitationFrequencyMaxValue, /*exception if bad*/ true);
					var /*command*/ cmd = String.Format("freq={0:F3}", value);
					Send(cmd, CommandAckTypes.AckExpected);
				}
			}
		}

		/**********************************************************************************************************************/
		/// <summary><see cref="IController.ExcitationIsEnabled"/>
		public bool ExcitationIsEnabled
		{
			get
			{
				var /*response*/ rsp = string.Empty;
				;
				SendInternal("excit?", CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);
				if (rsp.Equals(ExcitationStop)) { return false; }
				if (rsp.Equals(ExcitationStart)) { return true; }
				throw new System.InvalidOperationException("unknown response \"" + rsp + "\"");
			}
			set
			{
				if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
					Send((value ? ExcitationStart : ExcitationStop), CommandAckTypes.AckExpected);
				}
			}
		}

		public const string ExcitationStart = "excit>start";
		public const string ExcitationStop = "excit>stop";

		/**********************************************************************************************************************/
		/// <summary>Gets the firmware version.</summary>
		/// <value>firmware version <code>string</code></value>
		public string FirmwareVersion
		{
			get
			{
				// Command is based on equipment mode so lock around the whole operation
				var /*response*/ rsp = string.Empty;
				try {
					lock (commandLock) {
						const string CMD = "vers";  //command
						if (IsDemoMode) {
							if (CommandDEMO(CMD, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected) > AckTypes.Ok) {
								throw new System.IO.IOException("CommandDEMO != AckTypes.Ok or AckTypes.DoingCommand");
							}
						}

						// Firmware looks like:
						// "TMC Mag-NetX(TM) FW 95-37938-01 Rev.C7 #7 @24-Feb-2011"
						// GARY-CHANGE: I commented this out. It could cause a problem if we are in scope mode
						//              on a single port system....  ???
						// GARY-FIX: This isn't true anymore with 2-port controllers...

						///**Commented by Tismo_23/08/2018. Since it looks redundant as DC2020 scope is integrated by tismo
						/////***************************************************************************************************//
						//if (Program.Scope_on_Old_Controller == true)
						//{
						//    throw new System.InvalidOperationException("Must be in control mode to retrieve firmware version; mode == " + EquipmentMode.ToString());
						//}
						/////***************************************************************************************************//

						if (SendInternal(CMD, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected) > AckTypes.Ok) {
							if (SendInternal(CMD, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected) > AckTypes.Ok)
								throw new System.IO.IOException("InternalSendCommand != AckTypes.Ok or AckTypes.DoingCommand");
						}
					}
				} catch (System.Exception ex) { MessageBox.Show("error in FirmwareVersion"); throw ex; }
				return rsp;//.ToLowerInvariant()
			}
		}


		/**********************************************************************************************************************/
		internal void FlushDeviceAndSerialPortBuffers() {
			try {
				// Send a newline to flush out any pending command and flush the port input if we
				// aren't in scope mode.
				//				if (EquipmentMode == EquipmentModes.ScopeCapture) {
				//					ControllerPort.Write(System.Environment.NewLine);
				//				} else {
				ControllerPortFlush(System.Environment.NewLine);
				//                }
			} catch (System.Exception ex) { throw ex; }
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// something was in the buffer: returns true,
		/// nothing in the buffer - returns false
		/// </summary>
		private bool FlushSerialPortInput(string sendText = null) {
			var /*result (returned)*/ rsl = false;
			try {
				var bufChars = ControllerPortFlush(sendText);
				rsl = (bufChars > 0);// something was in the buffer, returns true, nothing - returns false
			} catch (System.Exception ex) { throw ex; }
			return rsl;
		}

		/**********************************************************************************************************************/
		internal int ControllerPortFlush(string sendText = null, bool isIgnoreIO = true) {
			//gives :
			//A first chance exception of type 'System.TimeoutException' occurred in System.dll
			//A first chance exception of type 'System.TimeoutException' occurred in MagNetXAnalyzer.exe
			//A first chance exception of type 'System.TimeoutException' occurred in MagNetXAnalyzer.exe
			//A first chance exception of type 'System.TimeoutException' occurred in MagNetXAnalyzer.exe
			var /*tally (returned)*/ tly = 0;
			if (/*OK?*/ CommStatus == CommStatusStates.Connected) {
				// If text is specified, send it first in order to avoid human/machine conflicts.
				if (/*pre-send?*/ !string.IsNullOrEmpty(sendText)) {
					ControllerPort.Write(sendText);
					Thread.Sleep(10);
				}
				// OK ... time to do some flushing...
				try {
					var rslt = ControllerPort.GetEntireBuffer; //read all bytes
					tly = rslt.Length;
				} catch (System.IO.IOException ex) {
					if (!isIgnoreIO) { throw ex; }
				} catch (System.TimeoutException) {
					/*expected (buffer is empty); ignore*/
					System.Diagnostics.Trace.WriteLine("ControllerPortFlush[629]");
				} catch (System.Exception ex) { throw ex; }
			}
			return tly;
		}

		/// <summary>Gets the open/closed status of the port.</summary>
		/// <value>The IsOpen?.</value>
		public bool IsPortOpen
		{
			get { return ControllerPort.IsOpen; }
		}

		/// <summary>Gets the name of the port.</summary>
		/// <value>The name of the port.</value>
		public string PortName
		{
			get { return ControllerPort.PortName; }
		}

		/// <summary>Gets the description of the port.</summary>
		/// <value>The Description of the port.</value>
		public string PortDescription
		{
			get { return ControllerPort.PortDescription; }
		}

		/// <summary>Used to read data when in scope mode</summary>
		private SerialPortDataReader serialPortDataReader;

		/// <summary>Serial response from the system if a command is accepted</summary>
		private const string OkResponse = ">~OK";

		/// <summary>errors look like:
		/// ">~ER COMMAND Param, Error Code = 2" for invalid parameters
		/// or
		/// ">~Unrecognized Cmd, Error Code = 1" for an invalid command
		/// search for "Error Code = "</summary>
		private const string errorCodeString = "Error Code = ";


		[System.ComponentModel.DefaultValue("")]
		public static string LastCommandSent { get; private set; }

		/// <summary>Send a command</summary>
		/// <returns><see cref="AckTypes"/></returns>
		/// <param name='command'/>
		/// <param name='ackType'/>
		public AckTypes Send(string command, CommandAckTypes ackType) {
			AckTypes retAck = AckTypes.Unknown;
			var /*response*/ rsp = string.Empty;
			if (Program.ConnectedController.CommStatus == CommStatusStates.Connected) {
				return SendInternal(command, CommandTypes.NoResponseExpected, out rsp, ackType);
			} else {
				rsp = string.Empty;
				retAck = AckTypes.PortError;
				//sendDataManualEvent.Set();
				return retAck;
			}
		}


		/**********************************************************************************************************************/

		private Thread sendThread = null;
		[DefaultValue(null)]
		public ManualResetEvent sendDataManualEvent = new System.Threading.ManualResetEvent(false);
		/**********************************************************************************************************************/
		internal AckTypes SendInternal(string command, CommandTypes commandType, out string response, CommandAckTypes ackType) {
			Tools.IsOK(ackType, /*exception*/ true);
			Tools.IsOK(commandType, /*exception*/ true);
			response = /*just a default value for the out*/ DoingCommandResponse;
			var /*(result*/ rsl = AckTypes.Unknown;
			double elapsed_time_us = 0.0;
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			try {
				if (IsDemoMode) {
					if (DemoIsBusy) {
						response = DoingCommandResponse;
						rsl = AckTypes.DoingCommand;
					} else {
						DemoIsBusy = true;
						try {
							// Fake the result.
							rsl = CommandDEMO(command, commandType, out response, ackType);
							response = response.ToLower(System.Globalization.CultureInfo.InvariantCulture);
						} catch (System.Exception ex) {
							throw ex;
						} finally { DemoIsBusy = false; }
					}
				} else {
					// lock to avoid more than one command at one time
					lock (commandLock) {
						// If a response is expected and we are in scope capture mode we have an invalid
						// condition as no responses are returned in scope capture mode.

						///**Commented by Tismo_23/08/2018. Since it looks redundant as DC2020 scope is integrated by tismo
						/////***************************************************************************************************//
						//if ((Program.Scope_on_Old_Controller == true) && (commandType != CommandTypes.NoResponseExpected))
						//{
						//    throw new System.InvalidOperationException("unable to get response in ScopeCapture mode");
						//}
						/////***************************************************************************************************//


						// Acquire the least necessary lock so we can do things like sending commands
						// even when in scope mode.
						//bool /*is write only?*/ isWrt = (commandType == CommandTypes.NoResponseExpected) && (EquipmentMode == EquipmentModes.ScopeCapture);
						//if (isWrt) {
						//    ControllerPort.WriteLock();
						//} else { ControllerPort.Lock(); }
						try {
							string ResultResponse = string.Empty;
							lock (LockObj) {
								sendThread = new System.Threading.Thread(() =>
								{
									rsl = SendSupport(command, commandType, out ResultResponse, ackType);
								});
								sendThread.Name = "Send";
								sendThread.IsBackground = true;
								sendDataManualEvent.Reset();
								sendThread.Start();
								sendDataManualEvent.WaitOne(300);
								response = ResultResponse;
								sendThread.Abort();
							}
							//  rsl = SendSupport(command, commandType, out response, ackType);
							System.Diagnostics.Debug.Assert(response != null);
							// response = response.ToLowerInvariant();
						} catch (System.Exception ex) {
							throw ex;
						} finally {
							//if (isWrt) {
							//    ControllerPort.WriteUnlock();
							//} else { ControllerPort.Unlock(); }
						}
					}
				}
			} catch (System.Exception ex) {
				throw ex;
			} finally {
				stopwatch.Stop();
				elapsed_time_us = stopwatch.ElapsedTicks / 10;
				if (Program.FormMain != null) {
					if (Program.FormMain.Created) {
						var elapsTime = string.Format("{0:#0.00}", elapsed_time_us / 1000);
						Program.FormMain.DiagScreenTimeToExecuteCommandShow(elapsTime); // ms took
						Program.FormMain.DiagScreenLastCommandShow(command);
					}
				}
				/*if command takes longer, send diag message*/
				if (elapsed_time_us > 10000)// >10 ms
				{
					var test_msg = "cmd= " + command + "; resp= " + response + "; took " + ((int)elapsed_time_us).ToString() + " us; prev_cmd = " + LastCommandSent;
					System.Diagnostics.Debug.WriteLine(test_msg);
				}
				LastCommandSent = command;
				sendDataManualEvent.Set();
			}
			return rsl;
		}



		/**********************************************************************************************************************/
		private AckTypes SendSupport(string command, CommandTypes commandType, out string response, CommandAckTypes ackType) {
			var /*result (returned)*/ rsl = AckTypes.Unknown; //Program.ConnectedController.EquipmentMode;
			response = string.Empty;    //keep compiler happy
			var /*trace*/ trc = System.Reflection.MethodBase.GetCurrentMethod().Name;
			var Not_timeout = true;
			var ackResponse = string.Empty;
			//#if DEBUG //with enabled, it cannot read correctly port. TOO FAST?
			Tools.DebugTimer = /*start timer; value doesn't matter*/ TimeSpan.MinValue;
			var /*time to just write command*/ tmW = Tools.DebugTimer;
			var /*time to get response*/ RSPtime = Tools.DebugTimer.TotalMilliseconds;
			//#endif
			try {
				// Validate the command.
				if (/*bad?*/ string.IsNullOrWhiteSpace(command)) { throw new ArgumentException(); }
				command = command.Trim();
				trc += "(\"" + Tools.TextTidy(command) + "\"...)";  //make trace prettier
				if (!command.EndsWith("\r\n")) {
					if (command.Length == 1)
						command += "\r";
					else
						command += "\r\n";
				}

				try {
					if ((CommStatus == CommStatusStates.CommError) && WhenCommErrorDontTry) {
						response = "Not Trying";
						sendDataManualEvent.Set();
						return AckTypes.Unknown;
					}
					var /*acknowledged?*/ acked = -1;
					response = string.Empty; //used to be null;
					var charcnt = ControllerPortFlush(); //this simply reads all the buffer and discards it without sending new lone
														 //FlushDeviceAndSerialPortBuffers();

					// we want to retry commands if we get an error, it may be that the command
					// simply didn't go through correctly, a character was dropped etc
					var consecutiveErrors = 0;
					const int MaximumConsecutiveErrors = 3;

					if (Program.ValidateScopeCheck) {
						Program.FormScope.datReceived.Reset();
						Program.FormScope.mainFormQueue.Clear();
					}

					// send the command
					ControllerPort.Write(command);
					//#if DEBUG  //with enabled, it cannot read correctly port. TOO FAST?
					tmW = Tools.DebugTimer; //get time to write
											//#endif
					if (ackType == CommandAckTypes.AckExpected) {
						// read input to see if the system acked the command
						//							int readRetries = 20;
						do {
							// make sure we can recover in the case where the board may not have received the command
							// otherwise we can get stuck in this while()
							//								if ((this.CommStatus == CommStatusStates.CommError)) readRetries = -1;
							//								if (readRetries <= 0) { break; }
							try {
								//									readRetries = readRetries - 1;
								if (Program.ValidateScopeCheck)
									Program.FormScope.datReceived.WaitOne(300);
								ackResponse += ControllerPort.GetEntireBuffer; // collect incoming chars
								if (ackResponse.Contains(OkResponse)) {
									acked = ackResponse.IndexOf(OkResponse);
									rsl = AckTypes.Ok;
									break;
								} else if (ackResponse.Contains(DoingCommandResponse)) {
									acked = ackResponse.IndexOf(DoingCommandResponse);
									rsl = AckTypes.DoingCommand;
									break;
								} else {    //see if we got an error instead
									int errorCodeNumber;
									if (IsResponseAnErrorCode(ackResponse, out errorCodeNumber)) {
										acked = ackResponse.IndexOf(errorCodeString);//"Error Code = "
										consecutiveErrors++;

										// if we hit our error limit OR
										// if we have no retries left then throw the exception
										//
										// The idea being that we don't want a retried 'error' to end up being
										// returned as a lack of ack, as they aren't at all the same
										// condition
										if ((consecutiveErrors > MaximumConsecutiveErrors)
											/*||((retries - 1) <= 0)*/) {
											var error = ackResponse.Substring(acked);
											throw new System.InvalidOperationException(error);
										}
									}
									// commands could be out of sync, lets flush things out and retry
									//if (!FlushSerialPortInput()) { return AckTypes.PortError; }
								}
							} catch (System.TimeoutException) {
								/*expected (buffer is empty); ignore*/
								System.Diagnostics.Debug.WriteLine(">> TimeoutException - is scope running?");
								continue;
							}
							Not_timeout = (Tools.DebugTimer.TotalMilliseconds < ControllerPort.ReadTimeout);// if less than ControllerPort.ReadTimeout, TRUE
						} while ((acked < 0) && (Not_timeout));
						//#if DEBUG  //with enabled, it cannot read correctly port. TOO FAST?
						RSPtime = Tools.DebugTimer.TotalMilliseconds;
						//here we have received >~OK or >~ER or  >~Doing, OR WE TIMEOUT

						Debug.WriteLine("Rx'd ={0}, {1} ms", ackResponse, RSPtime);
						//#endif
						//var pos = 0; //wait and search fo the final LfCr
						//if (Not_timeout) { //process response
						//    do {
						//        Thread.Sleep(1); // let it finish transmission
						//        ackResponse += ControllerPort.GetEntireBuffer; // collect last incoming chars

						//        pos = ackResponse.IndexOf("\n", acked); // the next '\n' after acknowledgement token
						//        if (pos > 0) {
						//            ackResponse = ackResponse.Remove(acked); //remove end of line, it could be: "\n>~Unrecognized Cmd, Error Code = 1\r\nscope>freq=2000.00\r\n>~OK\r\n\n>~Unrecognized Cmd, Error Code = 1"
						//            ackResponse = ackResponse.TrimEnd('\n', '\r');
						//        }
						//    } while (pos <= 0);
						//} else {//was timeout
						//    rsl = AckTypes.TimeOut;
						//    response = "Timeout";
						//    return rsl;
						//}
					} else {//control mode we already sent command, if no ack is expected don't bother to read the response
						rsl = AckTypes.NoAck;
						sendDataManualEvent.Set();
						return rsl;
					}
					// Read the echoed command and discard it, if echo is enabled and we are in control mode
					var TempResponse = ackResponse;
					command = command.TrimEnd('\n');
					var /*index*/ idx = ackResponse.IndexOf(command); //find echo
					if (idx >= 0) { TempResponse = ackResponse.Substring(idx + command.Length); }

					// Read the response, if we expect one
					if (commandType == CommandTypes.ResponseExpected) {
						TempResponse = TempResponse.TrimStart('\r', '\n');
						var pos = TempResponse.IndexOf("\r");
						// if ((pos < acked) && (pos > 0)) TempResponse = TempResponse.Substring(0, pos); //here is actual response
						TempResponse = TempResponse.TrimEnd('\r', '\n');
						TempResponse = TempResponse.TrimStart('\r', '\n');
						response = TempResponse;
						// see if the response was an error, the command could have been invalid
						int errorCodeNumber;
						if (IsResponseAnErrorCode(response, out errorCodeNumber)) {
							throw new System.InvalidOperationException(response);
						}
					}
					// see if we ran out of retries looking for an ack
				} catch (System.Exception ex) {
					throw ex;
				}
			} catch (System.Exception ex) {
				throw ex;
			} finally {
				//#if DEBUG  //with enabled, it cannot read correctly port. TOO FAST?
				//The "vers" command gets response "TMC Mag-NetX(TM) FW 95-37938-01 R.Jt3 #10 @ Aug 07 2015 11:51:43 \r\n>~OK",
				// len = 70 bytes, with sending cmd and getting echo ~76 bytes.
				// VB6 Analyzer calculates cmd_time between 10.1 ~ 22 ms. This program gives ~140 mS !?
				var /*interval*/ ivl = Tools.DebugTimer;
				System.Diagnostics.Debug.Write(trc + " [" + command.Length.ToString() + " bytes]");
				var /*speed*/ spd = command.Length / Math.Max(1.0, tmW.TotalMilliseconds);
				System.Diagnostics.Debug.WriteLine(" took " + tmW.TotalMilliseconds.ToString() + " ms to send (" + spd.ToString() + " bytes/ms)");
				System.Diagnostics.Debug.Indent();
				System.Diagnostics.Debug.Write("response=\"" + Tools.TextTidy(response) + "\" [" + response.Length.ToString() + " bytes]");
				spd = (command.Length + response.Length) / ivl.TotalMilliseconds;
				System.Diagnostics.Debug.WriteLine("took " + ivl.TotalMilliseconds.ToString() + " ms to receive  (" + spd.ToString() + " bytes/ms)");
				System.Diagnostics.Debug.WriteLine("LastCommandSent=\"" + LastCommandSent + "\"");
				System.Diagnostics.Debug.Unindent();
				sendDataManualEvent.Set();
			}
			return rsl;
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// Returns true if an error code can be extracted, false if not</summary>
		/// <returns>
		/// <c>true</c> if this instance is response an error code the specified response errorCodeNumber; otherwise, <c>false</c>.</returns>
		/// <param name='rsp'>
		/// If set to <c>true</c> rsp.</param>
		/// <param name='errorCodeNumber'>
		/// If set to <c>true</c> error code number.</param>
		/// <exception cref='System.InvalidOperationException'>
		/// Is thrown when an operation cannot be performed.</exception>
		internal bool IsResponseAnErrorCode(string response, out int errorCodeNumber) {
			errorCodeNumber = -1;
			//FIXME: Consider throwing an exception with a specific type of
			// error depending on the error code
			// errors look like:
			//
			// ">~ER <COMMAND> Param, Error Code = 2" for invalid parameters
			// or
			// ">~Unrecognized Cmd, Error Code = 1" for an invalid command
			// search for "Error Code = "
			//var errorCodeString = "Error Code = ";
			response = response.Trim();
			int pos = response.IndexOf(errorCodeString);
			// if we found the substring see what error code we got and throw it
			if (pos != -1) {
				var errorCodeValue = response.Substring(pos + errorCodeString.Length);
				if (errorCodeValue.Length > 1)
					errorCodeValue = errorCodeValue.Substring(0, 2);
				if (!String.IsNullOrEmpty(errorCodeValue))
					errorCodeNumber = Int32.Parse(errorCodeValue);
				return true;
			} else { return false; }
		}

		/**********************************************************************************************************************/
		/// <summary>Open this instance.</summary>
		public void Open() {
			try {
				if (!IsDemoMode) {
					ControllerPort.WriteLock();
					try {
						ControllerPort.Open();
						//if (ControllerPort.IsOpen == false) {
						//    var msg = ("Cannot open " + ControllerPort.PortName + " port");
						//    System.IO.IOException exp = new System.IO.IOException();
						//    System.Diagnostics.Debug.Write(msg);
						//    throw exp;
						//}

						CommStatus = CommStatusStates.Connected;
						// In case the system is in TMC menu mode, send ESC, ESC, ESC, q to ensure it leaves menu mode
						// const string ESC = "\u001b";

						//**Commented by Tismo_23/08/2018. Since it looks redundant as DC2020 scope is integrated by tismo
						/////***************************************************************************************************//
						//if (Program.Scope_on_Old_Controller != true)
						//{
						//    ControllerPort.Write(ESC); Delay_sec(0.2f); //Thread.Sleep(100); //send ESC & wait 100ms
						//    ControllerPort.Write(ESC); Delay_sec(0.2f); //Thread.Sleep(100); //send ESC & wait 100ms
						//    ControllerPort.Write(ESC); Delay_sec(0.2f); //Thread.Sleep(100); //send ESC & wait 100ms
						//    ControllerPort.Write("q");    //send q
						//    var SerBuffNOTempty = FlushSerialPortInput(); //if buffer is not empty again, is it still in the menu mode?
						//    Thread.Sleep(50 /*ms*/);
						//    SerBuffNOTempty = FlushSerialPortInput();   // something was in the buffer, returns true, nothing - returns false
						//    Thread.Sleep(150 /*ms*/);
						//    SerBuffNOTempty = FlushSerialPortInput();   // something was in the buffer, returns true, nothing - returns false

						//    if (SerBuffNOTempty)
						//    { //Controller still transmits, try to send command in 230400 baud
						//      //-!-                               MessageBox.Show("Controller.cs, line [1031]", "Unable to clear buffer!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
						//        Program.CommunicationsSpeed = 230400;
						//        var command = "scope>stop\r";
						//        var timeBetweenCharacters = new TimeSpan(0, 0, 0, 0, 5);
						//        // split the command into bytes
						//        var splitCommand = command.ToCharArray();
						//        // write characters one at a time with the appropriate delay between each one
						//        for (int byteOffset = 0; byteOffset < splitCommand.Length; byteOffset++)
						//        {
						//            ControllerPort.WriteChar(splitCommand[byteOffset]);
						//            Thread.Sleep(timeBetweenCharacters);
						//        }
						//        SerBuffNOTempty = FlushSerialPortInput();   // something was in the buffer, returns true, nothing - returns false
						//        Thread.Sleep(50 /*ms*/);
						//        SerBuffNOTempty = FlushSerialPortInput();   // something was in the buffer, returns true, nothing - returns false
						//    }
						//}
						/////***************************************************************************************************//
					} catch (System.Exception ex) { //when Terminal is connected to COM4, it gives message "Access to the port 'COM4' is denied".
						var msg = ex.ToString();// it could be: The port is already open.
						if (msg.Contains("denied"))
							WhenCommIsTakenDontTry = true;
						throw ex;
					} finally { ControllerPort.WriteUnlock(); }
				}


				///**Commented by Tismo_23/08/2018. Since it looks redundant as DC2020 scope is integrated by tismo
				/////***************************************************************************************************//
				//if (Program.Scope_on_Old_Controller != true)
				//{
				//    // Set the equipment to a known state
				//    EquipmentMode = EquipmentModes.Control;
				//    ACFactor = new ACFactorCollection(this);
				//    Channels = new ChannelCollection();
				//    CrossTalk = new CrossTalkCollection(this);
				//    CustomDC = new CustomDCCollection(this);
				//    GainAnalog = new GainAnalogCollection(this);
				//    InputDCOffset = new InputDCOffsetCollection(this);
				//    Loop = new LoopCollection(this);
				//    EchoState = EchoStates.Disabled;

				//    ControllerPortFlush();
				//    ScopeFrequency = /*value irrelevant; it's the set that matters*/ double.MinValue;
				//    ControllerPortFlush();
				//}// it could be: The port is already open.
				//***************************************************************************************************//
			} catch (System.Exception ex) {
				Close();
				throw ex;
			}
		}
		public void Delay_sec(float delay_time) {
			double CmdStartTime = Microsoft.VisualBasic.DateAndTime.Timer;
			double WaitTime;
			do {
				System.Windows.Forms.Application.DoEvents();
				WaitTime = Microsoft.VisualBasic.DateAndTime.Timer - CmdStartTime;
			} while (!((WaitTime > delay_time)));
		}
		/**********************************************************************************************************************/
		private void TheScopeDataProcessor_OnDataSampleFound(DataSample datum) {
			if (OnDataSampleFound != null) { OnDataSampleFound(datum); }
		}

		/**********************************************************************************************************************/
		private void FFT_OnStarting(List<DataSample> samples, int sampleElementCount) {
			if (OnDataProcessingStarting != null) { OnDataProcessingStarting(samples, sampleElementCount); }
		}

		/**********************************************************************************************************************/
		/// <summary>Handles the data processor on data processing complete.</summary>
		/// <param name="channel"/>
		/// <param name="result"/>
		private void FFT_OnComplete(System.Numerics.Complex[] result, int channel) {
			if (OnDataProcessingComplete != null) { OnDataProcessingComplete(result, channel); }
		}

		/********************************************************************************************************************** /
		void HandleSerialPortDataReaderOnDataCaptured(Byte[] data, int dataLength) {
			// Pass the results to anyone that might be monitoring
			//--- BdG THIS is where the defect is!
			if ( OnScopeDataCaptured != null) { OnScopeDataCaptured(data, dataLength); }// subscribers?

			// Pass data to the DataProcessor ... this is where data enters the FFT/Scope stream.
			FFTProcessor.ProcessingStart(data, dataLength);
		}

		/**********************************************************************************************************************/
		/// <summary>Returns a <code>string</code> that represents the current <see cref="TMCController"/>.</summary>
		/// <returns> A <code>string</code> that represents the current <see cref="TMCController"/>.</returns>
		public override string ToString() {
			return string.Format("[Analyzer: Port={0}, FirmwareVersion={1}]", ControllerPort.PortName, FirmwareVersion);
		}

		/// <summary>The baud rates that are checked during detection if no list is passed in</summary>
		public static readonly int[] StandardBaudRates = new int[] { 115200, 230400, 460800 };

		public class DetectedDeviceEntry {
			public string PortName;
			public int BaudRate;
			public string PortDescription;

			/// <summary>
			/// Initializes a new instance of the <see cref="TMCAnalyzer.TMCController.DetectedDeviceEntry"/> class.</summary>
			/// <param name='PortName'/>
			/// <param name='BaudRate'/>
			public DetectedDeviceEntry(string PortName, int BaudRate) {
				this.PortName = PortName;
				this.BaudRate = BaudRate;
				//this.PortDescription = PortDescription;
			}
		}

		[System.ComponentModel.DefaultValue(false)]
		public bool WhenCommErrorDontTry { get; set; }

		[System.ComponentModel.DefaultValue(false)]
		public bool WhenCommIsTakenDontTry { get; set; }

		/**********************************************************************************************************************/
		public static string[] RCIcommands = {
	"File name, DC-2020 parameters 02-16-2017 23-38.param"
	,"ison=4 // Number of active isolators                                                 "
	,"post>enabled // Power-On Self Test                                                   "
	,"eipa=169.254.20.20 // IP address // ! no match w saved IP adr: 0.0.0.0               "
	,"emsk=255.255.0.0 // Net mask // ! no match w saved NetMask: 0.0.0.0                  "
	,"e_gw=0.0.0.0 // Gateway                                                              "
	,"e_am>Static // Address assigning mode                                                "
	,"emac=A8.63.F2.00.00.80 // MAC adr //no match w saved MACadr: 00.00.00.00.00.00       "
	,"etpt=2020 // Telnet port 2020 // ! no match w saved Telnet Port: 0                   "
	,"emir>Disabled //Ethernet MirrorPort: Disabled, u_USB, F_USB, R_USB, RS232            "
	,"freq=+5.000 // excitation frequency                                                  "
	,"ampl=+1.20E+03 // excitation amplitude                                               "
	,">~ER drvo Param, Error Code =2                                                       "
	,"drvo=0 // Output to Exciteprup=+1.500 // Excitation pulse / slope, cnts/loop         "
	,"prdn=+1.500 // Excitation pulse slope, cnts/loop                                   "
	,"shaker=XXIO                                                                          "
	,">~ER oiso Param, Error Code =2                                                       "
	,"//// ALARM SETTINGS                                                                  "
	,"alarm>on                                                                             "
	,"alarmO>on // Oscillation Monitoring                                                  "
	,"alarmS>on // Saturation Monitoring                                                   "
	,"alarmH>on // High Freq Monitoring                                                    "
	,"alarmL>on // Low Freq Monitoring                                                     "
	,"alarm_thre=15000 // Oscill Alarm FFT threshold ampl, dflt 15000 cnts (200...15000)   "
	,"alarm_rate=+10.000 // Gain auto adjustment Rate,  dflt 10 dB/min (1...40)            "
	,"alarm_gred=-12 // Allowed gain reduction -dB; dflt -12dB=25% (-3...-40)              "
	,"alarm_secd=60 // Delay: Alarm detected->Alarm FLAG set, dflt 60 sec (10...120)       "
	,"alarm_lock=120 // Delay AlarmSET->Axis reduced gain locked, dflt 120s(10...180)      "
	,"safeg=28000 // Saturation Geo signal LIMIT                                           "
	,"//// CONTROL LOOPS STATUS                                                            "
	,"loop_fb_0>ACTIVE  // Feed Back axis Geo_1X                                           "
	,"loop_fb_1>ACTIVE  // Feed Back axis Geo_1Y                                           "
	,"loop_fb_2>ACTIVE  // Feed Back axis Geo_1Z                                           "
	,"loop_fb_3>ACTIVE  // Feed Back axis Geo_2X                                           "
	,"loop_fb_4>ACTIVE  // Feed Back axis Geo_2Y                                           "
	,"loop_fb_5>ACTIVE  // Feed Back axis Geo_2Z                                           "
	,"loop_fb_6>ACTIVE  // Feed Back axis Geo_3X                                           "
	,"loop_fb_7>ACTIVE  // Feed Back axis Geo_3Y                                           "
	,"loop_fb_8>ACTIVE  // Feed Back axis Geo_3Z                                           "
	,"loop_fb_9>ACTIVE  // Feed Back axis Geo_4X                                           "
	,"loop_fb_A>ACTIVE  // Feed Back axis Geo_4Y                                           "
	,"loop_fb_B>ACTIVE  // Feed Back axis Geo_4Z                                           "
	,"//// MOTOR-SENSOR TEST GAINS                                                         "
	,"gain10=+20.000 // MotPiezo1X->Vert_1 test gain                                       "
	,"gain11=+20.000 // MotPiezo1Y->Vert_2 test gain                                       "
	,"gain12=+20.000 // MotPiezo1Z->Vert_3 test gain                                       "
	,"gain13=+20.000 // MotPiezo2X->Vert_4 test gain                                       "
	,"gain14=+20.000 // MotPiezo2Y->Hor_X1 test gain                                       "
	,"gain15=+20.000 // MotPiezo2Z->Hor_X2 test gain                                       "
	,"gain16=+20.000 // MotPiezo3X->Hor_X3 test gain                                       "
	,"gain17=+20.000 // MotPiezo3Y->Hor_X4 test gain                                       "
	,"gain18=+20.000 // MotPiezo3Z->Hor_Y1 test gain                                       "
	,"gain19=+20.000 // MotPiezo4X->Hor_Y2 test gain                                       "
	,"gain1A=+20.000 // MotPiezo4Y->Hor_Y3 test gain                                       "
	,"gain1B=+20.000 // MotPiezo4Z->Hor_Y4 test gain                                       "
	,"gain1C=+20.000 // Axis Geo_3X  test gain                                             "
	,"gain1D=+20.000 // Axis Geo_3Y  test gain                                             "
	,"gain1E=  0      // NOT USED                                                          "
	,"gain1F=+5.000 // Frequency used @ test                                               "
	,"//// MOTOR-SENSOR TEST PHASES                                                        "
	,"gain20=-117.000 // MotPiezo1X->Vert_1 test phase                                     "
	,"gain21=-117.000 // MotPiezo1Y->Vert_2 test phase                                     "
	,"gain22=-117.000 // MotPiezo1Z->Vert_3 test phase                                     "
	,"gain23=-117.000 // MotPiezo2X->Vert_4 test phase                                     "
	,"gain24=-110.000 // MotPiezo2Y->Hor_X1 test phase                                     "
	,"gain25=-110.000 // MotPiezo2Z->Hor_X2 test phase                                     "
	,"gain26=-110.000 // MotPiezo3X->Hor_X3 test phase                                     "
	,"gain27=-110.000 // MotPiezo3Y->Hor_X4 test phase                                     "
	,"gain28=-110.000 // MotPiezo3Z->Hor_Y1 test phase                                     "
	,"gain29=-110.000 // MotPiezo4X->Hor_Y2 test phase                                     "
	,"gain2A=-110.000 // MotPiezo4Y->Hor_Y3 test phase                                     "
	,"gain2B=-110.000 // MotPiezo4Z->Hor_Y4 test phase                                     "
	,"gain2C=-110.000 // Axis Geo_3X  test phase                                           "
	,"gain2D=-110.000 // Axis Geo_3Y  test phase                                           "
	,"gain2E=  0      // NOT USED                                                          "
	,"gain2F=+1.20E+03 // Amplitude used @ test                                            "
	,"//// SPI DIAGNOSTIC STATUS, FRAME & OFFSET                                           "
	,"spi->started// High speed SPI diagnostic data                                        "
	,"frame0=1 // SPI data Geo_1Z  ->frame #0(0)                                           "
	,"scale0=1.0 // SPI frame #0 scale                                                     "
	,"frame1=2 // SPI data Geo_2Z  ->frame #1(1)                                           "
	,"scale1=1.0 // SPI frame #1 scale                                                     "
	,"frame2=3 // SPI data Geo_3Z  ->frame #2(2)                                           "
	,"scale2=1.0 // SPI frame #2 scale                                                     "
	,"frame3=4 // SPI data Geo_4Z  ->frame #3(3)                                           "
	,"scale3=1.0 // SPI frame #3 scale                                                     "
	,"frame4=5 // SPI data Geo_1X  ->frame #4(4)                                           "
	,"scale4=1.0 // SPI frame #4 scale                                                     "
	,"frame5=6 // SPI data Geo_2X  ->frame #5(5)                                           "
	,"scale5=1.0 // SPI frame #5 scale                                                     "
	,"frame6=7 // SPI data Geo_3X  ->frame #6(6)                                           "
	,"scale6=1.0 // SPI frame #6 scale                                                     "
	,"frame7=8 // SPI data Geo_4X  ->frame #7(7)                                           "
	,"scale7=1.0 // SPI frame #7 scale                                                     "
	,"frame8=9 // SPI data Geo_1Y  ->frame #8(8)                                           "
	,"scale8=1.0 // SPI frame #8 scale                                                     "
	,"frame9=10 // SPI data Geo_2Y  ->frame #9(9)                                          "
	,"scale9=1.0 // SPI frame #9 scale                                                     "
	,"frameA=11 // SPI data Geo_3Y  ->frame #A(10)                                         "
	,"scaleA=1.0 // SPI frame #A scale                                                     "
	,"frameB=12 // SPI data Geo_4Y  ->frame #B(11)                                         "
	,"scaleB=1.0 // SPI frame #B scale                                                     "
	,"frameC=13 // SPI data Height Iso 1 ->frame #C(12)                                    "
	,"scaleC=1.0 // SPI frame #C scale                                                     "
};


		/**********************************************************************************************************************/
		string MID(string instr, int start, int length, string replaced) {
			int strlen = instr.Length;
			string tstr;
			if (strlen < start + length)
				return instr;//cannot do it
			else {
				tstr = instr.Remove(start, length);
				tstr = tstr.Insert(start, replaced);
				return tstr;
			}
		}

		/**********************************************************************************************************************/
		// ONLY UPDATES CONTROL VARIABLES
		// NEED TO UPDATE string array to new values
		public void Parce_CMD(string CMDstring, string ORG_string, out string CHANGED_string) {
			string tempString;
			string CMDtoken;
			CHANGED_string = ORG_string;
			string valueString;
			int tempPos, tempPos2;
			//for ( ictr = 4; ictr < CMDstring.Count(); ictr++) {
			//    tempString = CMDstring[ictr].ToString().ToLower(System.Globalization.CultureInfo.InvariantCulture);        // get a line from the listbox
			tempString = ORG_string.ToLower(System.Globalization.CultureInfo.InvariantCulture);        // convert to lower case
			tempPos = CMDstring.IndexOf("//");
			if (tempPos < 0)
				CMDstring += " //";  // increase command string to include comment token
			valueString = "?"; //set default query just in case
			tempPos = tempString.IndexOf(" //");                         // find the first pair of slashes
			tempPos2 = tempString.Length;           // find total string length
			if (((tempPos2 > 8) && (tempPos == 0)) ||  // not found "//", but length is ok
			((tempPos > 0) && (tempPos2 >= tempPos))) { // found  "//"
				tempString = CMDstring;
				CMDtoken = tempString.Substring(0, 4);    // Get the first four characters from the string
				if (CMDtoken.Equals("freq")) {
					tempPos2 = tempString.IndexOf("=");                                 // find the "=" token, variable
					valueString = tempString.Substring(tempPos2 + 1, tempPos - tempPos2 - 1).ToLower(System.Globalization.CultureInfo.InvariantCulture);   // get the string of the value that after token
					this.ExcitationFrequency = float.Parse(valueString);
					CHANGED_string = MID(ORG_string, tempPos2 + 1, tempPos - tempPos2 - 1, valueString);
				} else if (CMDtoken.Equals("ampl")) {
					tempPos2 = tempString.IndexOf("=");                                 // find the "=" token, variable
					valueString = tempString.Substring(tempPos2 + 1, tempPos - tempPos2 - 1).ToLower(System.Globalization.CultureInfo.InvariantCulture);   // get the string of the value that after token
					this.ExcitationAmplitude = float.Parse(valueString);
					CHANGED_string = MID(ORG_string, tempPos2 + 1, tempPos - tempPos2 - 1, valueString);
				} else if (CMDtoken.Equals("exci")) {
					tempPos2 = tempString.IndexOf(">");                                 // find the ">" token
					valueString = tempString.Substring(tempPos2 + 1, tempPos - tempPos2 - 1).ToLower(System.Globalization.CultureInfo.InvariantCulture);   // get the string of the value that after token
					this.ExcitationIsEnabled = (valueString.Contains("stop") ? false : true);
					CHANGED_string = MID(ORG_string, tempPos2 + 1, tempPos - tempPos2 - 1, valueString);
				}
			} // valid command string found
			  //            } //for (lines in list)
		}

		/**********************************************************************************************************************/
		/// <summary/>
		public double ScopeFrequency
		{
			get
			{
				if (/*prepare?*/ scopeFrequencyBase <= 0.0) { ScopeFrequency = /*value irrelevant; it's the set that matters*/ double.MinValue; }
				return scopeFrequencyBase;
			}
			private set
			{   //MUST BE CALLED (ScopeFrequency = double.MinValue) EARLY AFTER OPENING!
				try {
					const string /*command*/ CMD = "scope>freq";
					var /*response*/ rsp = string.Empty;
					SendInternal(CMD, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);
					if (/*nothing?*/ string.IsNullOrWhiteSpace(rsp)) { throw new System.ArgumentNullException(); }
					var /*index*/ idx = rsp.IndexOf("=");
					if (/*bad?*/ (idx < 0) || (idx >= (rsp.Length - 1))) { throw new System.FormatException(rsp); }
					rsp = rsp.Substring(idx + 1);
					var /*candidate value*/ vlu = double.Parse(rsp, System.Globalization.CultureInfo.InvariantCulture);
					Tools.IsOK(vlu, 1000.0, 10000.0, /*exception*/ true);
					scopeFrequencyBase = vlu;
				} catch (System.Exception ex) {
					/*ignore*/
					System.Diagnostics.Debug.Assert(ex != null);
				} finally { if (scopeFrequencyBase <= 0.0) { scopeFrequencyBase = 2000.0 /*Hz*/; } }
			}
		}
		private double scopeFrequencyBase = /*negative causes preparation*/ double.MinValue;

		public const int FFTPowerDefValue = /*2^11 == 2048*/ 11;
		public const int FFTPowerMaxValue = /*2^13 == 8192*/ 13;
		public const int FFTPowerMinValue = /*2^9 == 512*/ 9;
	}

	/// <summary> Controller limit constants </summary>
	public class ControllerLimits {
		public const double ACFactorMaxValue = 1.0;

		public const double ACFactorMinValue = -1.0;

		public const int AutoCalOscMaxValue = 32768;

		public const int AutoCalOscMinValue = 0;

		public const double DCInputOffsetMaxValue = +300.0; //250 uTesla sensor

		public const double DCInputOffsetMinValue = -300.0; //250 uTesla sensor

		public const double ExcitationFrequencyMaxValue = 5000;

		/// <summary> Constant minimum excitation amplitude. </summary>
		public const double ExcitationAmplitudeMinValue = 0.0;

		/// <summary> Constant max excitation amplitude. </summary>
		public const double ExcitationAmplitudeMaxValue = 10.0;

		public const double ExcitationFrequencyMinValue = 0;

		public const double GainAnalogMaxValue = 254.0;

		public const double GainAnalogMinValue = -254.0;

		public const double GainDigitalMaxValue = 999.0;

		public const double GainDigitalMinValue = -999.0;

		public const int TrackDCJumpMaxValue = 131072;//or 4569 mA

		public const int TrackDCJumpMaxDelay = 30;//sec

		public const int TrackDCJumpMinValue = 0;
	}

	////*** DEFINE OBJECT TO REPRESENT ONE CONNECTION
	//// This is a data structure that represents ONE connection to a controller.
	//// When we run "FindControllers" one of these data-structures is created for each controller we find.
	//// Then a LIST of these objects is returned as the result of the search.
	////
	//// INTERNAL = Accessable by any code in the same assembly (.EXE or .DLL) but not from another assembly.
	////
	//public class ControllerConnection
	//{
	//    public string ConnectionType;       // "Serial" or "Ethernet"
	//    public string ConnectionName;       // This is the name that the Enhanced Serial Port command finds. (Not Implemented)
	//    public string IPaddress;            // String of the TCP/IP address. When not used = "NA" (Not Applicable)
	//    public string Port;                 // For serial this is like "COM1". For ethernet it's like "2020"
	//    public int Speed;                   // For serial this is like "115200" or "230400". For ethernet it's "NA"
	//    public ControllerConnection(string ConnectionType, string ConnectionName, string IPaddress, string Port, int Speed)
	//    {
	//        this.ConnectionType = ConnectionType;
	//        this.ConnectionName = ConnectionName;
	//        this.IPaddress = IPaddress;
	//        this.Port = Port;
	//        this.Speed = Speed;
	//    }
	//}
	////*** FIND CONTROLLERS
	//// This method will look for a Mag-NetX controller. There are two ways it can be used:
	////    * If no argument/parameter is passed in, it will search all the COM/Serial ports looking for controllers.
	////
	////    * Optionally the caller may supply a LIST of "ControllerConnections" definine where to search for controllers.
	////      It will look for controllers on the connections specified. It will NOT search anywhere else.
	////      If the StopAtFirstFound parameter = TRUE, it will return once ONE controller has been found.
	////      Otherwise it will continue searching at all the places defined on the ControllerConnections list.
	////
	////    * NOTE:   The first time this procedure is called, we will probably want to stop as soon as one controller
	////              is found -- since this is probably the controller the user wants. But IF that isn't the droid
	////              uh, I mean controller...they are looking for -- then on the SECOND call we will want to NOT STOP
	////              at the first controller found. We will want to look for the other specified controllers.
	////
	////              This will become particularly important if we even get into ETHERNET / INTERNET connections
	////              because we can't just do and seach every TCP/IP address and every IP PORT the way we do
	////              with local Serial/COM ports. So we will need to have this LIST of connection places to search.
	////
	////     * NOTE:  While the data structure supports Ethernet TCP/IP connections I am not supporting them in
	////              this procedure because we aren't even sure the controller will ever have Ethernet/IP cabability.
	////
	////     * NOTE:  This works using the "Analyzer" object that Venture Technologies created. This is a slick
	////              and simple way to do this. Still, I might want to use the Enhanced Serial Port so I can
	////              get full USB port names. I might also want to NOT encure the overhead (speed hit) of creating
	////              the muxer & muxed object every time?  I might also end up re-doing the ANALYZER object so that
	////              is uses it's OWN serial port, which may be an ENHANCED serial port.   // GARY-NOTE:
	////                                                                                         ~g.warner Aug 2014
	////
	//// INTERNAL = Accessable by any code in the same assembly (.EXE or .DLL) but not from another assembly.
	//// STATIC   = Can be accessed without creating an entire instance of this (Analyzer) class.
	//public static List<ControllerConnection> FindControllers(
	//                                                List<ControllerConnection> ControllersToLookFor = null, //                                                Boolean StopAtFirstFound = true, //                                                Boolean TryAllBaudRates = false)
	//{
	//    var FoundControllers = new List<ControllerConnection>();
	//    if (ControllersToLookFor == null)
	//    {
	//        // We don't have a list of controllers (places) they want to look for. So just search all COM ports.
	//        FoundControllers = LookForAllControllers(StopAtFirstFound, TryAllBaudRates);
	//    }
	//    else
	//    {   // Look for the ones they suggested.
	//        FoundControllers = LookForSpecificControllers(ControllersToLookFor, StopAtFirstFound);
	//        // If we didn't find one from their list of COM or ETHERNET Ports
	//        //      ...OR... if they want to find all possible controllers search the COM ports as well
	//        if ((FoundControllers == null) || StopAtFirstFound == false)
	//            FoundControllers = LookForAllControllers(StopAtFirstFound, TryAllBaudRates);
	//    }
	//    return FoundControllers;
	//}
	////***
	//private static int[] SupportedBaudRates = { 115200, 230400 };
	//private static int[] AllBaudRates = { 115200, 230400, 9600, 430800, 921600 };
	////***
	//private static List<ControllerConnection> LookForSpecificControllers(List<ControllerConnection> ControllersToLookFor = null, //                                                                Boolean StopAtFirstFound = true)
	//{
	//    ControllerConnection TheControllerWeFound = null;
	//    var FoundControllers = new List<ControllerConnection>();
	//    foreach (var CheckHere in ControllersToLookFor)
	//    {
	//        TheControllerWeFound = TryToFindControllerOnPort(CheckHere.Port, CheckHere.Speed);
	//        if (TheControllerWeFound != null)
	//        {
	//            // Add the controller to the list
	//            FoundControllers.Add(new ControllerConnection(
	//                ConnectionType: "Serial", ConnectionName: "N/A", IPaddress: "N/A", //                Port: TheControllerWeFound.Port.ToUpper(System.Globalization.CultureInfo.InvariantCulture), Speed: TheControllerWeFound.Speed));
	//            // Stop looking if we don't need to find more than one
	//            if (StopAtFirstFound == true) break;
	//        }
	//    }
	//    return FoundControllers;
	//}
	////***
	//private static List<ControllerConnection> LookForAllControllers(Boolean StopAtFirstFound = true, Boolean TryAllBaudRates = false)
	//{
	//    Boolean WeFoundOne = false;
	//    ControllerConnection TheControllerWeFound = null;
	//    var FoundControllers = new List<ControllerConnection>();
	//    // GARY-NOTE:   I have defined "SupportedBaudRates"
	//    //              NOTE that there is also a "StandardBaudRates" defined elsewhere.
	//    int[] ListOfBaudRates = SupportedBaudRates;
	//    if (TryAllBaudRates == true) ListOfBaudRates = AllBaudRates;
	//    // Create a list of all the serial ports attached to this computer.
	//    var ListOfSerialPorts = SerialPort.GetPortNames();
	//    foreach (var Port in ListOfSerialPorts)
	//    {
	//        foreach (int BaudRate in ListOfBaudRates)
	//        {
	//            TheControllerWeFound = TryToFindControllerOnPort(Port, BaudRate);
	//            if (TheControllerWeFound != null)
	//            {
	//                // Add the controller to the list then BREAK, no need to look for more speeds on THIS port.
	//                WeFoundOne = true;
	//                FoundControllers.Add(new ControllerConnection(
	//                    ConnectionType: "Serial", ConnectionName: "N/A", IPaddress: "N/A", //                    Port: TheControllerWeFound.Port.ToUpper(System.Globalization.CultureInfo.InvariantCulture), Speed: TheControllerWeFound.Speed));
	//                // Don't check any more speeds on this port, we already found the attached controller
	//                break;
	//            }
	//        }
	//        // If we've found a controller and all we need is one, don't try anymore ports.
	//        if ((StopAtFirstFound == true) & (WeFoundOne == true)) break;
	//    }
	//    // Return the list of whatever controllers we have found.
	//    return FoundControllers;
	//}
	////***
	//private static ControllerConnection TryToFindControllerOnPort(string Port, int BaudRate)
	//{
	//    // Create an object to hold one controller
	//    IAnalyzer controllerBase = null;                          // A ANALYZER Object that represent ONE controller
	//    ControllerConnection ControllerConnectionFound = null;      // The connection we found to a controller
	//    try
	//    {
	//        var SerialPort = new SerialPort(Port, BaudRate);        // Create a basic/native serial port
	//        var muxer = new SerPort(SerialPort);            // Create a muxer...so that we can...
	//        var muxed = muxer.GetSerialPortMuxed();                 // Create a muxed...so that we can...
	//        controllerBase = new Analyzer(muxed);
	//        controllerBase.Open();
	//        var Version = controllerBase.FirmwareVersion;
	//        ControllerConnectionFound = new ControllerConnection(
	//        ConnectionType: "Serial", //               ConnectionName: "Mag-NetX", //        IPaddress: "N/A",
	//                                    Port: Port,                     // Could also be set to controllerBase.PortName;
	//                                    Speed: BaudRate);               // Could also be set to controllerBase.BaudRate;
	//    }
	//    catch (System.Exception)
	//    {
	//        if (controllerBase != null) controllerBase.Close();
	//    }
	//    // Return what we found. Will be NULL if nothing found
	//    return ControllerConnectionFound;
	//} // --- END OF METHOD ---
	//===  END OF FILE
	//===  END OF FILE
	//===  END OF FILE
	// ***

}