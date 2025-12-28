// I USE THE FOLLOWING MARKERS THROUGHOUT THIS PROJECT TO MAKE FINDING SPECIAL CODE COMMENTS EASY:
// GARY-NOTE:       Notes that go beyond standard code comments. Used to point out unusual things, features, or ideas.
// GARY-FIX:        Something that probably isn't right or that needs to be done to make the code more robust.
// GARY-ERROR:      A place in the code that generates exceptions (errors) and that should be fixed.
// GARY-DEBUG:      Code that is only here for debugging.
// GARY-CHANGE:     A change that I (Gary) have made to the original (Venture-Technology) code that I'm concerned might break something.
// GARY-REMOVE:     Code or comments that I need to remember to remove once I'm sure I don't want them anymore.
//
// THE FOLLOWING TAGS CAN BE SEARCHED FOR TO FIND WHERE CERTAIN MAJOR (and not necessarily intuitive) THINGS ARE HANDLED
// LOCATION-TAG: FIRST-CODE-RUN                     Where the program starts execution when it very first begins.
// LOCATION-TAG: CONTROLER-OBJECT-CREATED           Anywhere that a CONTROLL-OBJECT (that represents a physical or simulated Mag-NetX controller) is created.
// LOCATION-TAG: MUXER-SERIAL-PORT-CREATED          Anywhere a MUXER is created.
// LOCATION-TAG: MULTI-THREADDING-STARTED           Anywhere that Multi-Threadding is started.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TMCAnalyzer;
//using TMCAnalyzer.Forms;

//////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.VisualBasic; // for String.len(), InStr(), etc
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;

// ===
// The MAIN() function of this PROGRAM class is the first code to run in the TMC-Analyzer project.
// To change which project or code runs first:
// * TMCAnalyzer Solution > RIGHT-CLICK > PROPERTIES > Where you can select which project is the startup project (TMCAnalyzer)
// * TMCAnalyzer      > RIGHT-CLICK > PROPERTIES > APPLICATION > STARTUP OBJECT > Were you choose .PROGRAM (this clss) to be the first code run.
// ===
namespace TMCAnalyzer {
	public static partial class Program {
		//=== PROGRAM LEVEL "GLOBAL" VARIABLES
		public const double ControllerSampleFrequency = 5000; // ELDAMP - 5 kHz
		//*** WHAT RUNNING MODE THE PROGRAM IS IN
		public static bool DevelopmentMode = false;         // TRUE if we want to skip some things (like login & searching for ports)
		public static bool IDEmode = false;                 // TRUE if we are running in the Development Environment (not stand alone)
		public static bool NoControllerDemoMode = false;                // The program will run but not try to connect to any controller.
		//        public static bool SimulationMode = false;          // The program will run and SIMULATE data from a controller (not supported yet)
		public static bool WhiteMode = false;               // The program run with COLORED tabs & Controls, ! SWITCHING by clicking TMC LOGO !
		// public static bool WhiteMode = true;             // The program run with a WHITE based theme.
		public static bool IsReadingControllerSettings = false;

		// ---
		//public static bool DebugFormTerminal = false;
		//public static bool DebugFormRTmon = false;
		//public static bool DebugFormScope = false;

		// *** HANDLES FOR SUB-FORMS
		public static formMain FormMain = null;
		public static formMonitoring FormRTMonitoring = null;
		//public static formScope FormScope = null;
		public static Terminal formTerminal = null;
		public static frmSplash FormSplash = null;
		public static formGains FormGains = null;
		public static FrmValveTestResult FormValveTestResult = null;

		public const int MaxScopeChannels = 16;      // Maximum simultaneous channels
		const double ScopeFrequencyDefValue = 5000;  //Hz must be [ScopeFrequencyMinValue, ScopeFrequencyMaxValue]
		const double ScopeFrequencyMaxValue = 10000; //Hz
		const double ScopeFrequencyMinValue = 1000;  //Hz
		public static double ScopeFrequency = ScopeFrequencyDefValue; //2500.0f;
		public const double PI = Math.PI; //3.14159265358979;

		public static double StopWatchFreq = 2.0E6; //= Stopwatch.Frequency is read only; to start, use 2 MHz;  //= 2628291 Hz, or 2.68 MHz (laptop Dell Precizion M4700); 2727841 Hz (on desktop Dell 3800)
		public static double StopWatchTick_to_us = 2.0; // to start, use 2 MHz; = 1.0E6 / Stopwatch.Frequency;

		public static bool DualPortController = false;
		public static bool UpdateLPFfilter = false;
		public static bool DoLPF_onScopeData = false;
		public static double ScopeFilterFreq1 = 1000.0;     //
		public static double ScopeFilterFreq2 = 1.0;        //
		public static double ScopeFilter_Q1 = 0.5;          //
		public static double ScopeFilter_Q2 = 0.5;          //
		public static double ScopeFilterGain = 1.0;         //
		public static int ScopeFilterType = 0;              // NO_FILTER

		//*** INTERNAL WARNINGS
		// These are warnings that are set when unexpected things happen as the program runs.
		// The cause of the warning will also be logged to the Log4Net file.
		// The planned/intended use of the warnings is
		// * Set all warnings off (false) at the beginning of the program.
		// * As situations arrise, turn warnings on
		// * On main screen, if warnings come on, turn some indicator to Yellow or Red
		// * This lets the user know something isn't running right.
		// * We could search the logs for the warnings
		// * We could also have a screen that tells/shows which warnings are on/true.
		// Result of STAT command sent to controller is not as expected
		public static bool WarningSTATNotFormattedAsExpected = false;

		//*** OBJECTS FOR CONNECTION TO THE CONTROLLER
		[DefaultValue(null)]
		public static TMCController ConnectedController { get; set; }             //active controller
		[DefaultValue(new string[] { /*same indices as ComPortNumbers*/ })]
		public static string[] ControllerPortDescriptions { get; set; }
		[DefaultValue(/*can't use string.Empty*/ "")]
		public static string ControllerPortData { get; set; }
		[DefaultValue(new string[] { /*same indices as PortDescriptions*/ })]
		public static string[] ControllerPortNumbers { get; set; }

		[DefaultValue(/*can't use string.Empty*/ "")]
		public static string ControllerPortSingle { get; set; }
		[DefaultValue(/*can't use string.Empty*/ "")]
		public static string ControllerPortDouble { get; set; }

		[DefaultValue(null)]
		public static SerialPort SerialPortBase { get; set; } //base serial port from which other port items are derived
		[DefaultValue(null)]
		public static TcpClient TcpClientBase { get; set; }

		public static void SerialPortCreate(string portIdentity, int portSpeed) {
			SerialPortDestroy();
			try {
				// Create the base serial port, from that, a multiplexer and, from that, the
				// multiplexed ports.
				SerialPortBase = new SerialPort(portIdentity, portSpeed);
				SerialPortMultiplexer = new SerPort(Program.SerialPortBase);
				SerialPortMultiplexed4Main = Program.SerialPortMultiplexer;
				SerialPortMultiplexed4Terminal = Program.SerialPortMultiplexer;
				if (portIdentity == ControllerPortDouble) {
					SerialPortDataBase = new SerialPort(ControllerPortData, 230400);
					SerialPortDataMultiplexer = new SerPort(Program.SerialPortDataBase);
					SerialPortData = SerialPortDataMultiplexer;
				}
				SerialPortMultiplexed4Main.ReadTimeout = SerialPortMultiplexed4Main.WriteTimeout = 100; //ms per byte
			} catch (Exception ex) {
				try { ConnectedController.Close(); } catch { /*ignore; bigger problems than this*/ }
				string diagnostic = "A significant fault has been detected in the communication link!\r" +
						"It may be necessary to terminate the application, remove both connections and power from the controller,\r" +
						" reconnect, repower and then restart the application.\r"
#if DEBUG
						+ "Diagnostic Details:\r" + ex.ToString()
#endif
				;
				MessageBox.Show(diagnostic, "Anomaly Detected!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
		}

		public static void TelentPortCreate() {

			SerialPortDestroy();
			try {
				TcpClientBase = new TcpClient(Program.FormSplash.txtHost.Text, Convert.ToInt32(Program.FormSplash.txtPort.Text));
				SerialPortMultiplexer = new SerPort(Program.TcpClientBase);
				SerialPortMultiplexed4Main = Program.SerialPortMultiplexer;
				SerialPortMultiplexed4Terminal = Program.SerialPortMultiplexer;

				//  SerialPortMultiplexed4Main.ReadTimeout = SerialPortMultiplexed4Main.WriteTimeout = 100 /*ms per byte*/;
			} catch (Exception ex) {
				try { ConnectedController.Close(); } catch { /*ignore; bigger problems than this*/ }
				string diagnostic = "A significant fault has been detected in the communication link!\r" +
						"It may be necessary to terminate the application, remove both connections and power from the controller,\r" +
						" reconnect, repower and then restart the application.\r"
#if DEBUG
						+ "Diagnostic Details:\r" + ex.ToString()
#endif
				;
				MessageBox.Show(diagnostic, "Anomaly Detected!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}

		}
		[DefaultValue(null)]
		public static SerialPort SerialPortDataBase { get; set; }
		[DefaultValue(null)]
		public static TcpClient TcpClientDataBase { get; set; }
		[DefaultValue(null)]
		public static SerPort SerialPortDataMultiplexer { get; set; }       //multiplexer on which multiplexed ports are made
		[DefaultValue(null)]
		public static SerPort SerialPortData { get; set; }

		/**********************************************************************************************************************/
		public static void SerialPortDestroy() {
			Device.Instance.StopTimerToReadData();
			try { Program.ConnectedController?.Close(); } catch { /*ignore*/ }
			try { Program.ConnectedController?.Dispose(); } catch { /*ignore*/ } finally { Program.ConnectedController = null; }
			try { Program.SerialPortMultiplexed4Main?.Close(); } catch { /*ignore*/ } finally { Program.SerialPortMultiplexed4Main = null; }
			try { Program.SerialPortMultiplexed4Terminal?.Close(); } catch { /*ignore*/ } finally { Program.SerialPortMultiplexed4Terminal = null; }
			Program.SerialPortMultiplexer = null;
			Program.SerialPortBase = null;
			Program.SerialPortData = null;
			Program.TcpClientBase = null;
			Program.SerialPortDataMultiplexer = null;
			Program.SerialPortDataBase = null;
		}

		[DefaultValue(null)]
		public static SerPort SerialPortMultiplexer { get; set; }       //multiplexer on which multiplexed ports are made
		[DefaultValue(null)]
		public static SerPort SerialPortMultiplexed4Main { get; set; }    //multiplexed serial port for the application
		[DefaultValue(null)]
		public static SerPort SerialPortMultiplexed4Terminal = null;      //multiplexed serial port for terminal form

		/**********************************************************************************************************************/
		public static int CommunicationsSpeed {
			get {
				if (ConnectedController == null) {
					throw new System.ArgumentNullException();
				}
				return ConnectedController.BaudRate;
			}
			set {
				if (Program.FormSplash.IsPortUSB) {
					if ((value != 115200) && (value != 230400)) { // ? bad
						throw new System.ArgumentOutOfRangeException(Tools.Diagnostic(value.ToString(), "115200, 230400"));
					}
					try {
						//-!-                    MessageBox.Show("Program.cs, line [178]", "Changing baud!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
						// If the new speed is different from the current speed, close the port, apply the
						// new speed and then reopen the port.
						if (/*change?*/ value != SerialPortBase.BaudRate) {
							ConnectedController.Close();
							SerialPortBase.BaudRate = value;
							ConnectedController.Open();
						}
					} catch (Exception ex) {
						try {
							ConnectedController.Close();
						} catch {
							//ignore; bigger problems than this
						}
						string diagnostic = "A significant fault has been detected while trying to adjust the communications link!\r" +
								"It may be necessary to terminate the application, remove both connections and power from the controller,\r" +
								" reconnect, repower and then restart the application.\r"
#if DEBUG
							+ "Diagnostic Details:\r" + ex.ToString()
#endif
							;
						MessageBox.Show(diagnostic, "Anomaly Detected!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
				}
			}
		}

		/// <summary>Path gets the fully qualified path to the application-state folder.</summary>
		/// <value>
		/// fully qualified path (ending with reverse-solidus) to the application's state
		/// folder</value>
		/// <example><code>Debug.Assert(System.IO.Directory.Exists(TMC_Path));</code></example>
		public static string TMC_Path {
			get {
				if (string.IsNullOrWhiteSpace(tmc_PathBase)) { // prepare?
					TMC_Path = /*use defaults*/ null;
				}
				return tmc_PathBase;
			}
			private set {
				if (string.IsNullOrWhiteSpace(value)) {    // use default?
					value = TMC_PathValue;
				}
				if (!value.EndsWith("\\")) {  // append slash?
					value += "\\";
				}
				tmc_PathBase = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + value;

				// Ensure that the data and logs subfolders exist.
				System.IO.Directory.CreateDirectory(TMC_PathData);
				System.IO.Directory.CreateDirectory(TMC_PathLogs);
			}
		}
		private static string tmc_PathBase = string.Empty;

		/// <summary>
		/// Path, Data gets the fully qualified path to the application's data folder.</summary>
		public static string TMC_PathData { get { return TMC_Path + TMC_PathDataValue; } }

		/// <summary>
		/// Path Data Value specifies the subfolder for the application's data.</summary>
		public const string TMC_PathDataValue = "Data\\";

		/// <summary>
		/// Path, Logs gets the fully qualified path to the application's logging folder.</summary>
		public static string TMC_PathLogs { get { return TMC_Path + TMC_PathLogsValue; } }

		/// <summary>
		/// Path Logs Value specifies the subfolder for the application's log(s).</summary>
		public const string TMC_PathLogsValue = "Logs\\";

		/// <summary>
		/// Path Value specifies the default subfolder for the application-state.</summary>
		public const string TMC_PathValue = "\\TMC\\TMCAnalyzer";

		/// <summary>
		/// The Last Resort Exception Handler, as a last resort, manages exceptions that are not
		/// fielded deeper in the code.</summary>
		/// <example><code>
		/// Application.ThreadException += new ThreadExceptionEventHandler(LastResortOnException);</code>
		/// as the first executable statement in <see cref="Main"/></example>
		/// <remarks>
		/// If running the application in DEBUG mode (under Visual Studio), the code will break
		/// immediately when the exception is thrown and at the point that it is thrown. This is
		/// exactly the desired behaviour; you want to be able to see immediately what's happened
		/// so that you can fix it and not flounder around trying to recreate the exception with
		/// breakpoints in appropriate places.</remarks>
		/// <remarks>
		/// While testing (DEBUG), if you want your top-level exception handler to execute, anyway,
		/// after the code has broken, you can just hit F5 to continue. If you don't want your code
		/// breaking on exceptions in DEBUG at all, you can disable this behaviour in Visual
		/// Studio via the Exceptions option on the Debug menu and clearing the 'Break when
		/// exception is: User-unhandled' checkbox under 'Common Language Runtime Exceptions'.
		/// If you expand Common Language Runtime Exceptions you can enable or disable this
		/// behaviour for individual exception types.</remarks>
		///
		public enum ErrEventEnum { _UnhandledEvent, _ThreadEvent };
		public static bool ExceptionUnhandledFunction(object sender, object e, ErrEventEnum eventType) {//UnhandledExceptionEventArgs e) {
			string msg_str = "";
			string src_str = "";
			string mailto_str = "";
			formException my_window = new formException();
			mailto_str = "mailto:tmc.SoftwareFeedback@ametek.com?subject=";
			if (eventType == ErrEventEnum._UnhandledEvent) {
				src_str = "Unhandled Event ";
				msg_str = (((UnhandledExceptionEventArgs)e).ExceptionObject).ToString();
			} else { //_ThreadEvent
				src_str = "System Exception ";
				msg_str = String.Format("Exception: {0}\n\r", ((Exception)e).ToString());
				// msg_str += String.Format("Exception Type: {0}\n\r", ((Exception)e).GetType().Name);
				// msg_str += "\nStack Trace:\n" + ((Exception)e).StackTrace;
			}
			src_str = "ElectroDamp Analyzer " + src_str;
			my_window.lblMsgCaption.Text = "Unhandled Anomaly";
			my_window.LblMsgSrc.Text = src_str;
			my_window.txtMsgTest.Text = msg_str;
			DialogResult res = my_window.ShowDialog();
			//my_window.Dispose(); // NO>>> ? dispose does not give enough time to transfer whole string??? >>> IT WAS '&' truncating "body"
			//my_window.Hide(); //it is already hidden if button is pressed
			switch (res) {
				case DialogResult.Yes:
					mailto_str = "mailto:tmc.SoftwareFeedback@ametek.com";
					mailto_str += "?subject=";
					mailto_str += src_str;
					// message gets truncated at first &: WmMouseUp(Message& in: at System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)\r\n
					msg_str = msg_str.Replace('&', '+'); //replace "&" with "plus" to avoid it
					mailto_str += "&body=";
					mailto_str += msg_str;
					System.Diagnostics.Process.Start(mailto_str);
					break;
				case DialogResult.Ignore:
					break;
				case DialogResult.Abort:
				default:
					return true; // true - Terminate application
			}
			return false; // try to continue
		}
		public static void LastResortOnException(object sender, UnhandledExceptionEventArgs e) {
			/*var dgn = Tools.TextTidy(((Exception)e.ExceptionObject).ToString());    // diagnostic
			//var  dgn = Tools.TrimHard(((Exception)e.ExceptionObject).ToString()); // diagnostic
			System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(dgn));
			MessageBox.Show(dgn, "Unexpected Anomaly!", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
			System.Environment.Exit(0);
			*/
			if (ExceptionUnhandledFunction(sender, e, ErrEventEnum._UnhandledEvent))
				System.Environment.Exit(0);
		}
		public static void CatchThreadExceptionEvent(object sender, ThreadExceptionEventArgs e) {
			if (ExceptionUnhandledFunction(sender, e.Exception, ErrEventEnum._ThreadEvent))
				System.Environment.Exit(0);
		}

		//===  FIRST CODE TO RUN IN THE PROGRAM
		[STAThread]     // [STAThread] marks the MAIN program thread so it uses the "Single Threaded Apartment" model.
		// * This is required for being able to use Component Object Model (COM) objects.
		// * If not present the application uses the multi-threaded apartment model, which is not supported for Windows Forms
		// * This does NOT mean the application is single threaded!
		// * It means is that COM objects we consume (message boxes, etc) will only be called from ONE thread, not MANY
		// * This is necessary because many COM objects are not "thread safe" and can't handle being called from multiple threads
		static void Main() {
			Tools.TracePrepare();
			Program.StopWatchFreq = Stopwatch.Frequency; //read only; = 2628291 Hz, or 2.68 MHz (laptop Dell Precision M4700); 2727841 Hz (on desktop Dell 3800)
			StopWatchTick_to_us = 1.0E6 / Stopwatch.Frequency; //
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException); //Automatic, CatchException, ThrowException
			Application.ThreadException += new ThreadExceptionEventHandler(CatchThreadExceptionEvent);
			//mdr #if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(LastResortOnException);
			//#endif
			Program.TMC_Path = /*use defaults*/ null;

			// If this application is ever globalized, there will have to be an override of the
			// user interface culture. For now, using InvariantCulture for both the application and
			// the user interface is the safest approach. Even if the Windows is "regionalized" to,
			// say, the Klingon culture (ISO-639 "tlh" ... yes, it exists), the parsing and
			// presentation of values will be according to the invariant culture; for example,
			// 123.456 will be parsed and presented as 123.456 rather than 123456 (as would happen
			// in Nederlands under Dutch ("nl-NL") Windows.
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			Application.EnableVisualStyles();

			// Adopt and use the visual styles and colors from the operating system.
			// In general this is a good idea to keep the application's look consistent with the
			// settings of the computer and the user.
			Application.SetCompatibleTextRenderingDefault(false);
			Debug.WriteLine("---");
			Debug.WriteLine("--- STARTING:    Solution:  ED_Analyzer  ");
			Debug.WriteLine("                  Project:  TMCAnalyzer  ");
			Debug.WriteLine("                   Class:   Program.cs   ");
			Debug.WriteLine("                 Function:  Main()       ");
			Debug.WriteLine("");
			Debug.WriteLine("--- Thread ID:   " + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
			Debug.WriteLine("---");
			Application.DoEvents();

			// Setup the debug and other modes of the program.
			// GARY-FIX: Read these settings from the APP.CONFIG file and set them from the Advanced-Options form.
			if (System.Diagnostics.Debugger.IsAttached == true)
				IDEmode = true;
			DevelopmentMode = true;

			// Initialize all warnings to FALSE. Once they are set to true, they remain true for the rest of the program run.
			WarningSTATNotFormattedAsExpected = false;

			FormSplash = new frmSplash();
			Debug.WriteLine("--- Program.cs --- BEFORE Application.Run(FormMain)");

			Application.Run(FormSplash);
			Debug.WriteLine("--- Program.cs --- AFTER Application.Run(FormMain)");
		}

#if false //TEST_DATA_STREAM
		private static void ExerciseScope(object sender, DoWorkEventArgs e) {
			var t = 0.0;
			var randomMaker = new Random();
			for (var /*sample count*/ samples = 0; !exerciseScopeWorker.CancellationPending; samples++) {
				var sineValue = Math.Sin(t);
				var scaleFactor = 100.0;
				t += 0.01;
				if (t > 2 * Math.PI) { t = 0.0; }
				var randVlu = randomMaker.NextDouble();
				var randSine = randVlu + sineValue;
				var newDatum = new DataSample((int)(scaleFactor * t), (int)(scaleFactor * randVlu), (int)(scaleFactor * sineValue));
				ExerciseScopeSample = newDatum;
 //               Program.FormScope.AddSampleToArray(samples, ExerciseScopeSample);   //set min & max
				Program.FormScope.ProvideDataSampleToScope(newDatum);
				System.Threading.Thread.Sleep(1);
			}
			e.Cancel = true;
		}
		private static void ExerciseScopeHasEnded(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) {
			} else if (e.Error != null) {
				MessageBox.Show(e.Error.ToString(), "Anomaly!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			} else {
			}
			exerciseScopeWorker = null;
			FormScope.btnTestScope_Reset();
		}
		public static DataSample ExerciseScopeSample {
			get {
				lock (exerciseScopeSampleLock) {
					return exerciseScopeSampleBase;
				}
			}
			private set {
				lock (exerciseScopeSampleLock) {
					exerciseScopeSampleBase = value;
				}
			}
		}
		private static DataSample exerciseScopeSampleBase = new DataSample(0, 0, 0);
		private static object exerciseScopeSampleLock = new object();
		public static void ExerciseScopeStart() {
			if (exerciseScopeWorker == null) {
				exerciseScopeWorker = new BackgroundWorker();
//              exerciseScopeWorker.WorkerReportsProgress = true;//To add support for progress reporting
				exerciseScopeWorker.WorkerSupportsCancellation = true;
				exerciseScopeWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExerciseScopeHasEnded);
				exerciseScopeWorker.DoWork += new DoWorkEventHandler(ExerciseScope);
				exerciseScopeWorker.RunWorkerAsync();
			} else {
				MessageBox.Show("Exerciser is running already!");
			}
		}
		public static void ExerciseScopeStop() {
			if (exerciseScopeWorker != null) {
				exerciseScopeWorker.CancelAsync();
			}
		}
		private static BackgroundWorker exerciseScopeWorker = null;
#endif
		public static int ConnectionType;       //=true (or NOT zero) if serial port, =false (or 0) if telnet
		public enum ConnType {
			ConnTelnet = 0,     // it is "False" (was boolean type)
			ConnCOMport = 1,    // it is "True"
			ConnSimDLL = 2,
			ConnDEMO = 4
		}

		public static bool ValidateScopeCheck {
			get {
			//	return Program.FormScope != null && TMC_Scope.Program.ScopeIsRunning && Program.ConnectionType == (int)Program.ConnType.ConnCOMport
			//	   && TMC_Scope.TMCScopeController.portUSB != null && TMC_Scope.TMCScopeController.portUSB.PortName.Equals(Program.ConnectedController.PortName);
			return false;
			}
		}

		public static void StopIfTestRunning() {
			string result = string.Empty;
			if (Program.FormMain != null)
			if (Program.FormMain.Test_in_progress) {
				Program.FormMain.ButStartStopLTF.Checked = false;
			}
		}
#if false
		public static void DisconnectScope() {
			if (Program.FormScope != null && Program.ConnectionType == (int)Program.ConnType.ConnCOMport
				   && TMC_Scope.TMCScopeController.portUSB != null && TMC_Scope.TMCScopeController.portUSB.PortName.Equals(Program.ConnectedController.PortName)) {
				if (TMC_Scope.Program.ScopeIsRunning)
					Program.FormScope.ScopeSetStateUI(States.DATATRANSFER);
				if (TMC_Scope.TMCScopeController.portUSB.IsOpen)
					Program.FormScope.ConnectScope();
			}
		}
#endif
		//public static void DisconnectScope_ConnSwitch() {
		//	if (Program.FormScope != null && TMC_Scope.Program.MainFormConnType == 1
		//		   && TMC_Scope.TMCScopeController.portUSB != null && TMC_Scope.TMCScopeController.portUSB.PortName.Equals(TMC_Scope.Program.MainFormComPortName)) {
		//		if (TMC_Scope.Program.ScopeIsRunning)
		//			Program.FormScope.ScopeSetStateUI(States.DATATRANSFER);
		//		if (TMC_Scope.TMCScopeController.portUSB.IsOpen)
		//			Program.FormScope.ConnectScope();
		//	}
		//}
		public static string LTFtestOutputName;
		public static string LTFtestInputName;
		public static bool MotorSensorTest_READY;	//-!- IK20211230 only set: when initiated driver motor and measure sensor
		public static byte[] TrimEnd(byte[] array) { // used in CommPort.cs
			int lastIndex = Array.FindLastIndex(array, b => b != 0);
			Array.Resize(ref array, lastIndex + 1);
			return array;
		}

		/// <summary>
		/// Analyze array and return in provided ref values max and min Values and Indexes
		/// </summary>
		/// <param name="inArray"></param>
		/// <param name="StartIndex"></param>
		/// <param name="maxV"> by reference, need double</param>
		/// <param name="minV"> by reference, need double</param>
		/// <param name="MaxIndex"> by reference, need int</param>
		/// <param name="MinIndex"> by reference, need int</param>
		/// <param name="IgnoreZeroes">optional, excludes zero values from search.
		/// COULD be USEFUL if inArray is initialized to zeros, but known to have MEANINGFUL DATA as all positive or all negative values only</param>
		/// <returns>true if Min-Max can be found starting from StartIndex, false if start index is larger than ArraySize</returns>
		public static bool FindMaxMin(double[] inArray, int StartIndex, ref double maxV, ref double minV, ref int MaxIndex, ref int MinIndex, bool IgnoreZeroes = false) {
			int maxIndex = 0;
			int minIndex = 0;
			double max = -1E+308;			//set max and min possible values
			double min = 1E+308;
			int ArrSize = inArray.GetUpperBound(0);

			if (StartIndex > ArrSize - 1) return false; // fail - start index is larger than ArraySize
			for (int i = StartIndex; i <= ArrSize - 1; i++) {
				if (i > (ArrSize - 1)) break; // if skipping zeroes 'i' can be incremented twice
				if (max < inArray[i]) {
					max = inArray[i];
					maxIndex = i;
				}
				if (IgnoreZeroes == true & inArray[i] == 0) {
					i = i + 1;
				} else {
					if (min > inArray[i]) {
						min = inArray[i];
						minIndex = i;
					}
				}
			}
			minV = min;
			maxV = max;
			MaxIndex = maxIndex;
			MinIndex = minIndex;
			return true; //sucsess
		}

		/// <summary>
		/// StdDeviation is calculated, if there are at least two members
		/// </summary>
		/// <param name="Aray"></param>
		/// <param name="StartIndex">optional, if need to calculate StdDev on part of array not starting from zero</param>
		/// <param name="Size">optional, if need to calculate StdDev on part of array </param>
		/// <param name="IgnoreZeroes"></param>
		/// <returns>positive value if sucess or negative value if array is empty or has only one member</returns>
		public static double St_Dev(ref double[] Aray, int StartIndex = 0, int Size = -1, bool IgnoreZeroes = false) {
			double functionReturnValue;

			if (Aray.GetUpperBound(0) > 1) {    // there are at least two members in Array
				if (Size == -1) // Not specified, argument is missing
					Size = Aray.GetUpperBound(0);
			} else {
				return -1; // error - StdDev cannot be negative
			}
			if (StartIndex > (Size - 2)) return -1; // error - need at least two members for StdDev
			int final_size = Size;
			double sum_of_squares = 0;
			double array_sum = 0;
			for (int array_index = StartIndex; array_index <= Size - 1; array_index++) {
				array_sum += Aray[array_index];
				sum_of_squares += (Math.Pow(Aray[array_index], 2));
				if (IgnoreZeroes == true & Aray[array_index] == 0) {
					final_size --;
				}
			}
			if (final_size < 2)
				final_size = 2;
			//prevent calc errors
			double diff = (final_size * sum_of_squares - Math.Pow(array_sum, 2));
			//because of limited accuracy, sometimes diff can be negative
			if (diff > 0) {
				functionReturnValue = Math.Pow((diff / (final_size * (final_size - 1))), 0.5);
			} else {
				functionReturnValue = 0;
			}
			return functionReturnValue;
		}

		/// <summary>
		/// StdDeviation is calculated starting from index zero, if there are at least two members
		/// </summary>
		/// <param name="Aray"></param>
		/// <param name="Size">optional, if need to calculate StdDev on part of array </param>
		/// <param name="IgnoreZeroes"></param>
		/// <returns> positive value if sucess or negative value if array is empty or has only one member</returns>
		public static double St_Dev(ref double[] Aray, int Size = -1, bool IgnoreZeroes = false) {
			return St_Dev(ref Aray, 0, Size, IgnoreZeroes);
		}

			public static double Avg(double[] Aray, int Size = -1) {
			if (Aray.GetUpperBound(0) >= 1)  // there are at least ONE member in Array
				if (Size == -1) //argument is missing
					Size = Aray.GetUpperBound(0);
				else return 0;
			double DataSum = 0;
			for (int counter = 0; counter <= Size - 1; counter++) {
				DataSum = DataSum + Aray[counter];
			}
			return DataSum / Size;
		}


		/**********************************************************************************************************************/
		/**********************************************************************************************************************/
		/**********************************************************************************************************************/
		/*********************     FROM VB6 Module1 Analyzer.bas        *******************************************************/
		/**********************************************************************************************************************/
		/**********************************************************************************************************************/
		/**********************************************************************************************************************/


		static class Module1 {
			//Telnet buffer
			public static string TelnetBuf;
			public static string response;
			//need this to send only one command like "ltf>x"
			public static bool cmd_sent;
			public static string SendString;
			public static bool waitresponse;
			// =true when request is sent, but OK is not received yet. Prevent getting response from one command to the other
			public static bool GUI_doing_command;
			//when controller sends >~Doing CMD
			public static bool CONTROLLER_doing_CMD;
			//initial read and update of controls should not trigger reaction (i.e. sending command) on their change
			public static bool Init_sys_data;
			public static bool Test_in_progress;
			//when initiated driver motor and measure sensor
			public static bool MotorSensorTest_active;

			public static bool FormTerminalVisible;
			public static bool AnalyzerVisible;
			public static bool FormPOSmonitoringVisble;
			//when controller shows Advanced menu on Terminal
			public static bool Menu_Active;

			// MY COMM declarations
			public static string str_MouseX;
			public static string str_MouseY;
			public static string str_Help;
			public static string strPortNum;
			public static string strPortStatus;
			public static string strPortSetting;
			public static string strBaud;
			public static string strParity;
			public static string strBitnum;
			public static string strBitstop;
			public static bool Port_chosen;

			//Any non-zero value is considered as true. In VB 6,
			//True has a numeric value of -1. False has a numeric value of 0.
			//The reason for this is because the Boolean data type is stored as a 16-bit signed integer.
			public static bool Windows_is_64bit;

			public static string strTelnet_IP_ADR;
			public static string strTelnet_PORT;

			public static string DLL_version;
			public static bool DLL_is_available;
			public static bool DLL_is_Initialized;
			public static bool DLL_runs_in_separate_thread;
			public static bool DLL_CallBackInitialized;
			public static string Demo_Simul_string;
			public static bool DLL_CharArrived;
			// it is actually Integer
			public static string CharFromDLL;

			public static bool ID_valid;	//=true if vers returns a string containing "95-"
			public static int Port;
			public static int idComDev;
			public static string Application_Dir;
			public static string DataFileName;
			public static int MonitoringTestType;	//on frmMonitoring
			public static int ExcitationAxis;		//where to apply excitation
			public static bool Excitation_status;	// global excitation status for monitoring and main forms
			public static int BNC_0_index;		//index of bncd0=BNC_0_index command
			public static int BNC_1_index;		//index of bncd0=BNC_1_index command

			// plot number assignment
			public const short CurrentPlot_Num = 1;
			public const short PreviousPlot_Num = 2;
			public const short RefPlot_Num = 3;
			public const short OneFilterTF_Num = 4;
			public const short Axis_TF_Num = 5;
			public const short Prediction_TF_Num = 6;
			public const short Lower_Lim_TF_Num = 7;
			public const short Upper_Lim_TF_Num = 8;

			//filter frame variables

			public static double LOOP_FREQUENCY;		//10000 for STACIS
			//Public Const ONE_OVER_FREQUENCY As Single = 1 / LOOP_FREQUENCY
			public static double PI_x2_OVER_FREQUENCY;

			public static int FilterAxis;		//filter axis to change parameters
			public static int FilterNumberInChain;		//filter number in the axis to change parameters
			public static int FilterParamNumber;		//0 to 10, filter user parameter number 0-5, 0(Filter Type, integer), 1-5 params, floats,6-10 filter coefficients
			public static int FilterTypeNumber;
			public static float TestGain;
			public static float TestFrequency;
			public static float TestPulsePosSlope;
			public static float TestPulseNegSlope;
			public static float[] DeflationOffset_mA = new float[5];
			public static int BufferLEN;
			public static float Locale_tmp_number;
			public static string LastStringSent;
			public static string LastStringReceived;
			public static string SystemFWversion;
			public static int LastKeyPressed;
			public const string DLL_Name = "ElectroDampSim";
			public static int DiagnosticAxis;		//X pos, Y pos, tZ pos, Z pos, tx pos, tY pos, X vel, Y vel, tZ vel, Z vel, tX vel, tY vel, maybe more
			public static int SystemType;		    //gets value from vers string if it has "STACIS" and assigns value from SysType enum
			public static string SystemName;		//gets value from vers string
			public static bool PhaseReversed;		//=true if 180 added, =false if direct (default)
			public const int TEST_LTF_ARRAY_LENGTH = 200;
			//*********************************************************
			public static float[] Reference_Freq_data = new float[TEST_LTF_ARRAY_LENGTH + 1];
			// data arrays
			public static double[] OneFilter_Mag = new double[TEST_LTF_ARRAY_LENGTH + 1];
			public static double[] OneFilter_Phase = new double[TEST_LTF_ARRAY_LENGTH + 1];
			// data arrays
			public static double[] Freq_points = new double[TEST_LTF_ARRAY_LENGTH + 1];
			public static double[] Axis_Mag = new double[TEST_LTF_ARRAY_LENGTH + 1];
			public static double[] Axis_Phase = new double[TEST_LTF_ARRAY_LENGTH + 1];
			public static int M3v_Cmd_Word;
			public static int AlarmWord;
			public static bool ProdTestStatus;
			//is 32-bit Prod_Test_BITS
			public static int self_test_word;
			public static bool ShowExpertControls;
			public static int M3v_Diagnostic_running;
			public static int M3v_measurement_done;
			public static int InputChosen;
			public static int OutputChosen;

			public const short CMD_LEN = 4;
			public static int[] ChannelPower = new int[17];
			// not basic (more than 2 bytes) command ends with ">~OK xxxxx" & vbCrLf or ">~ERR xxxxx" & vbCrLf line
			public const string Acknowledgment_token = ">~";
			// position monitoring scan data END
			public const string end_of_ScanTransmission = "OK\r\n";
			public const string OK_message = ">~OK\r\n";

			//filt type, 5 params, 5 coefficients
			public const int FILTER_PARAMS_TOTAL = 11;
			//#define PID_P_gain FilterGain
			//#define PID_I_gain freq_1
			//#define PID_D_gain freq_2
			public const int MAX_FILTERS_IN_AXIS = 6;
			//7 filters (0 to 6) in Axis chain, 10 parameters (0 to 5)-user, 6=a1,7=a2,8=b0,9=b1,0xA=b2
			static bool gISIDE;

			private const int OF_EXIST = 0x4000;
			private const int OFS_MAXPATHNAME = 128;
			private const int HFILE_ERROR = -1;

			private struct OFSTRUCT {
				public byte cBytes;
				public byte fFixedDisk;
				public short nErrCode;
				public short Reserved1;
				public short Reserved2;
				public byte[] szPathName;

				//UPGRADE_TODO: "Initialize" must be called to initialize instances of this structure. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"'
				public void Initialize() {
					szPathName = new byte[OFS_MAXPATHNAME + 1];
				}
			}

			public struct tComplex {
				public double Re;
				public double Im;
			}

			public struct tF_coeff {
				public double a1;
				public double a2;
				public double b0;
				public double b1;
				public double b2;
			}

			public struct tMag_Phase {
				public double Mag;
				public double Phase;
			}

			public const int USER_PARAM_NUMBER = 5;
			public struct double_iir {
				public double ftyp;     //mdr// same as in frmFilters,cs //int ftyp;
				public double[] par;
				public double a1;
				public double a2;
				public double b0;
				public double b1;
				public double b2;
				//coefficients are rarely change
				//    Private HistBuff As p_iir_history

				//UPGRADE_TODO: "Initialize" must be called to initialize instances of this structure. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="B4BFF9E0-8631-45CF-910E-62AB3970F27B"'
				public void Initialize() {
					par = new double[USER_PARAM_NUMBER];// mdr float[USER_PARAM_NUMBER];
				}
			}

			////define indexes for float_iir.par[index]
			//    Const freq_1 As Long = 1
			//    Const freq_2 As Long = 2
			//    Const Q_f_1  As Long = 3
			//    Const Q_f_2  As Long = 4
			//    Const FilterGain   As Long = 0
			public const int FilterGain = 0;
			public const int freq_1 = 1;
			public const int freq_2 = 2;
			public const int Q_f_1 = 3;
			public const int Q_f_2 = 4;
			const int PID_P_gain = FilterGain;
			const int PID_I_gain = freq_1;
			const int PID_D_gain = freq_2;


			public enum SysType {
				UNKNOWN_system = 0,
				STACIS_system = 1,
				EVERSTILL_system = 2,
				SEM_BASE_system = 3,
				STACISiX_system = 4
			}
			[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			//UPGRADE_WARNING: Structure OFSTRUCT may require marshalling attributes to be passed as an argument in this Declare statement. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"'
			private static extern int OpenFile(string lpFileName, ref OFSTRUCT lpReOpenBuff, int wStyle);
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
			private static extern void DLL_Init();
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			private static extern short DLL_IsRunning();
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
			private static extern void DLL_Stop();
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			// returns char array converted to BSTR ar parameter
			//Private Declare Sub DLL_version Lib "ElectroDampSim.dll" _
			//'       (ByVal sMyString As String, ByVal cBufferSize As Long)

			// returns BSTR (string of chars with length info)
			private static extern string VB6_DLL_version();
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			// returns char * to global array char DLL_response_buf[5000], accepts char * commands
			private static extern string Controller(string sMyString);
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			// returns BSTR ; accepts LPCSTR command
			private static extern void VB6controller(string cmd, string rsp, int rspSize);
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			// reads internal DLL output buffer, returns BSTR ; clears internal DLL output buffer
			private static extern void DLL_ReadBuf(string rsp, int rspSize);
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			// send command to DLL and exits without waiting for response
			private static extern void DLL_SendCMD(string cmd);
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			private static extern void InitStringCallBackFuncPtr(int lpfnMyFunc);
			[DllImport("ElectroDampSim.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

			private static extern void InitCharCallBackFuncPtr(int lpfnMyFunc);

			public static bool IsIDE() {
				bool functionReturnValue = false;
				//
				functionReturnValue = false;
				//This line is only executed if running in the IDE and then returns True
				System.Diagnostics.Debug.Assert(CheckIDE(), "");
				if (gISIDE) {
					functionReturnValue = true;
				}
				return functionReturnValue;
			}

			private static bool CheckIDE() {
				// this is a helper function for Public Function IsIDE()
				gISIDE = true;
				//set global flag
				return true;
			}

			//==================================================================
			public static bool Is_DLL_available() {
				bool exists = false;
				string start_dir = null;
				//-!-        start_dir = Application.Info.DirectoryPath;
				exists = FileExists(start_dir + "\\" + DLL_Name + ".dll");
				if (exists) {
					exists = FileExists(start_dir + "\\" + DLL_Name + ".lib");
				}
				//    Buffer = CurDir ' just to check
				DLL_is_available = exists;
				// global boolean
				return exists;
				// function return
			}

			// >>>>>>>>> DO NOT USE StringCallBack_to_DLL <<<<<<<<<<<<<<<
			//Public Sub Init_StringCallBack_to_DLL() ' >>> BUG>>> after few commands VB6 throws " out of String Space (Error 14)". I use char-by-char transfer instead
			//'if call initialization, output from DLL happens via CallBack and after few commands
			//' VB6 throws  "out of string space (error 14)"
			//    InitStringCallBackFuncPtr AddressOf DLLhasString_CallBackFunction
			//    DLL_CallBackInitialized = True
			//End Sub
			// >>>>>>>>> DO NOT USE StringCallBack_to_DLL <<<<<<<<<<<<<<<

			public static void Init_CharCallBack_to_DLL() {
				//-!- InitCharCallBackFuncPtr((DLLhasCHAR_CallBackFunction(NewChar)));
				//UPGRADE_ISSUE: The preceding line couldn't be parsed. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="82EBB1AE-1FCB-4FEF-9E6C-8736A316F8A7"'
				DLL_CallBackInitialized = true;
			}

			public static void Init_DLL() {
				if (Is_DLL_available()) {
					DLL_Init();
					DLL_version = VB6_DLL_version();
					Init_CharCallBack_to_DLL();
					System.Windows.Forms.Application.DoEvents();
					DLL_is_Initialized = true;
				} else {
					DLL_is_Initialized = false;
				}
			}

			public static void Cmd_Stop_DLL() {
				DLL_is_Initialized = false;
				DLL_Stop();
			}

			public static bool Is_DLL_running() {
				bool functionReturnValue = false;
				short running_1_not_0 = 0;
				running_1_not_0 = DLL_IsRunning();
				if (running_1_not_0 == 1) {
					functionReturnValue = true;
				} else {
					functionReturnValue = false;
				}
				DLL_runs_in_separate_thread = functionReturnValue;
				return functionReturnValue;
			}

			public static string Get_DLL_version() {
				string DLLvers = null;
				if (Is_DLL_available()) {
					DLLvers = VB6_DLL_version();
				} else {
					DLLvers = "ElectroDampSim DLL or LIB not found";
				}
				//    MsgBox DLLvers, vbInformation, "DLL version as BSTR"
				return DLLvers;
			}

			//SIZE IS LIMITED BY FrmTerminal.Timer1_Timer -
			public static string Buffer;
			//it checks and limits size: If Len(Buffer) > 32000 Then Buffer = Mid(Buffer, 32000) 'cut the beginning of buffer if it gets too long

			public static string Send_DLL_cmd(ref string cmd) {
				string functionReturnValue = null;
				string resp = null;
				// wchar_t wide char 16 bits
				if (Is_DLL_available()) {
					resp = new string(' ', 5000); // VB Space(5000)
					//    Dim Cmd As String
					//    Cmd = TextCMD.Text
					VB6controller(cmd, resp, 5000);
					// response is returned into Buffer by DLLhasCHAR_CallBackFunction
					//    TextResp.Text = resp
					//    MsgBox resp, vbInformation, "VB got response"
				} else {
					resp = "ElectroDampSim DLL or LIB not found";
				}
				if ((DLL_CallBackInitialized == true)) {
					functionReturnValue = Buffer;
				} else {
					functionReturnValue = resp;
				}
				resp = "";
				return functionReturnValue;
			}

			public static void Send_CMD_toDLLwoWaitForResponse(ref string cmd) {
				DLL_SendCMD(cmd);
			}

			public static string Read_DLL_whole_buffer() {
				int rsp_len = 0;
				string resp = null;
				// wchar_t wide char 16 bits
				if (DLL_is_available) {
					resp = new string(' ', 10000); // VB Space(10000);
					DLL_ReadBuf(resp, 10000);
					//    TextResp.Text = resp
					//    MsgBox resp, vbInformation, "VB got response"
				} else {
					resp = "";
					//    resp = "ElectroDampSim DLL or LIB not found"
				}
				rsp_len = resp.Length;
				if (rsp_len != 0) {
					rsp_len = resp.IndexOf('\0');
					// test if DLL trasfers zeroes.
					if (rsp_len != 0) {
						//resp = Strings.Replace(resp, "\0", " ");
						resp = resp.Replace('\0', ' ');
						// break point here
					}
				}
				//resp = Strings.RTrim(resp);
				resp = resp.TrimEnd();
				// cut end of long buffer containing spaces
				resp = resp.Replace('\0', ' ');
				// replace possible 0 on the end with space
				resp = resp.TrimEnd();
				// cut end of string which was 0 and now it is space
				return resp;
			}

			// >>>>>>>>> DO NOT USE StringCallBack_to_DLL <<<<<<<<<<<<<<<
			//Function DLLhasString_CallBackFunction(ByVal DLLstring As String, ByVal strlen As Long) As Long
			//' after few commands VB6 throws " out of String Space (Error 14)"
			//    Dim resp As String ' wchar_t wide char 16 bits
			//   Dim tpos As Long
			//    resp = Space$(500)
			//    Call DLL_ReadBuf(resp, Len(resp)) 'it includes EoS 0x00, cut resp up to it
			//    tpos = InStr(resp, Chr(0))
			//    If tpos <> 0 Then resp = Left(resp, tpos - 1)
			//    If frmTerminal.Visible = True Then 'Print sym
			//        frmTerminal.Receive (resp)
			//    Else ' add to Buffer without printing
			//        Buffer = Buffer + resp ' do not use trim, resp might have valid space
			//    End If
			//    DLLhasString_CallBackFunction = strlen
			//    resp = ""
			//End Function

			public static void DLLhasCHAR_CallBackFunction(int NewChar) {
				string resp = string.Empty;
				var c = Convert.ToChar(NewChar);
				// wchar_t wide char 16 bits
				//resp = Strings.Chr(NewChar);
				resp = string.Join("", c);

				var frmTerminal = new Terminal();
				//Print sym
				if (frmTerminal.Visible == true) {
					//-!-            frmTerminal.Receive(resp);
					// add to Buffer without printing
				} else {
					Buffer = Buffer + resp;
					// do not use trim, resp might have valid space
				}
			}

			public static bool FileExists(string Fname) {
				bool functionReturnValue = false;
				int lRetVal = 0;
				//UPGRADE_WARNING: Arrays in structure OfSt may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
				OFSTRUCT OfSt = default(OFSTRUCT);

				lRetVal = OpenFile(Fname, ref OfSt, OF_EXIST);
				if (lRetVal != HFILE_ERROR) {
					functionReturnValue = true;
				} else {
					functionReturnValue = false;
				}
				return functionReturnValue;
			}

			public static void FFT(ref double[] FR, ref double[] FI, ref int LnN) {

				//THE FAST FOURIER TRANSFORM
				//copyright © 1997-1999 by California Technical Publishing
				//published with  permission from Steven W Smith, www.dspguide.com
				//GUI by logix4u , www.logix4u.net
				//modified by logix4u, www.logix4.net
				//Upon entry, N% contains the number of points in the DFT, FR( ) and
				//FI( ) contain the real and imaginary parts of the input. Upon return,
				//FR( ) and FI( ) contain the DFT output. All signals run from 0 to N%-1.
				int ND2 = 0;
				int k = 0;
				int i = 0;
				int j = 0;
				int L = 0;
				int le = 0;
				int le2 = 0;
				int ip = 0;
				int NP_FFT = 0;

				double wr = 0;
				double ur1 = 0;
				double s = 0;
				double ur = 0;
				double ui = 0;
				double wi = 0;
				double ti = 0;
				double tr = 0;
				//double DivN = 0;

				NP_FFT = (int)Math.Pow(2, LnN);
				//Number of points for use in FFT

				ND2 = NP_FFT / 2;
				//Half points
				j = 1;

				for (i = 1; i <= NP_FFT - 1; i++) {

					if (i < j) {
						s = FR[i];
						FR[i] = FR[j];
						FR[j] = s;
						s = FI[i];
						FI[i] = FI[j];
						FI[j] = s;
					}

					k = ND2;

					while (k < j) {
						j = j - k;
						k = k / 2;
					}

					j = j + k;
				}

				//Loop for each stage
				for (L = 1; L <= LnN; L++) {

					le = (int)Math.Pow(2, L);
					le2 = le / 2;
					ur = 1;
					ui = 0;
					wr = System.Math.Cos(PI / le2);
					//Calculate sine & cosine values
					wi = -System.Math.Sin(PI / le2);
					//Loop for each sub DFT
					for (j = 1; j <= le2; j++) {
						i = j;
						while ((i <= NP_FFT)) {
							ip = i + le2;
							tr = FR[ip] * ur - FI[ip] * ui;
							//Butterfly calculation
							ti = FR[ip] * ui + FI[ip] * ur;
							FR[ip] = FR[i] - tr;
							FI[ip] = FI[i] - ti;
							FR[i] = FR[i] + tr;
							FI[i] = FI[i] + ti;
							i = i + le;
						}
						ur1 = ur * wr - ui * wi;
						ui = ur * wi + ui * wr;
						ur = ur1;
					}
				}
				//{l - l›kke}

			}

			public static string BIN(ref int inbyte) {
				// subprogram translates byte to BIN$ format, 127 = 01111111
				string m = null;
				string b = null;
				int k = 0;
				int j = 0;
				int bt = 0;

				bt = inbyte;
				m = "";
				for (j = 0; j <= 7; j++) {
					if (bt < 2)
						goto end_conv;
					b = (bt - 2 * (bt / 2)).ToString();
					bt = (bt / 2);
					m = b + m;
				}
			end_conv:
				m = bt.ToString() + m;
				for (k = j; k <= 6; k++) {
					m = " 0" + m;
				}
				return m;
			}
#if false
		//read everything into Buffer
		public static void Get_Serial_Input() {
			if ((ConnectionType == (int)ConnType.ConnCOMport)) {
				Buffer = Buffer + frmSplash.SerialComm.Input;
			}
			if ((ConnectionType == (int)ConnType.ConnTelnet)) {
				Buffer = Buffer + TelnetBuf;
			}
			if ((ConnectionType == (int)ConnType.ConnSimDLL)) {
				//        Buffer = Buffer & Read_DLL_whole_buffer 'Buffer is updated it happens automatically - no need here
			}
		}

		public static void Clean_COM_Buffer() {
			Get_Serial_Input();
			//it reads everything into Buffer
			Buffer = "";
			//clean the Comm port RxBuffer
		}

		//sets doing_cmd=True and sends cmd to port
		public static void Send_GUI_command(ref string cmd) {
			GUI_doing_command = true;
			// this must be cleaned when command finishes
			SendString = cmd;
			//COM port
			if ((ConnectionType == (int)ConnType.ConnCOMport)) {
				if (frmSplash.SerialComm.PortOpen == true)
					frmSplash.SerialComm.Output = cmd;
				//send command
			}
			if ((ConnectionType == (int)ConnType.ConnTelnet)) {
				//UPGRADE_NOTE: State was upgraded to CtlState. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
				if (frmSplash.Winsock.CtlState == MSWinsockLib.StateConstants.sckConnected) {
					frmSplash.Winsock.SendData((cmd));
				} else {
					Analyzer.txtStatus.Text = "Telnet is not ready";
				}
			}
			if ((ConnectionType == (int)ConnType.ConnSimDLL)) {
				Send_CMD_toDLLwoWaitForResponse(ref cmd);
			}

			cmd_sent = true;
			LastStringSent = LastStringSent + cmd;
			//for test
			if (Strings.InStr(LastStringSent, "menu" + Constants.vbCr) != 0) {
				Menu_Active = true;
				//sending "menu" command
			}
			if (Strings.InStr(LastStringSent, Strings.Chr(27) + "q" + Constants.vbCr) != 0) {
				Menu_Active = false;
				//sending esc+q (leave "menu") command
			}
			if (cmd == Constants.vbCr) {
				if (((Menu_Active == false) & (FrmHistory.ChkLocalEcho.CheckState == 1))) {
					FrmHistory.AddToHistory(("->" + LastStringSent + "<ENTER>"));
				}
				LastStringSent = "";
			}
		}

		public static void open_port_if_closed() {
			// ERROR: Not supported in C#: OnErrorStatement

			TimeSpan StartTime ;
			float WaitTime = 0;
			//COM port
			if ((ConnectionType == (int)ConnType.ConnCOMport)) {
				if (frmSplash.SerialComm.PortOpen == false) {
					//MsgBox ("Port COM " & Port & ": closed - opening...")
					frmSplash.SerialComm.CommPort = Port;
					frmSplash.SerialComm.PortOpen = true;
					// port cannot be open
					if (frmSplash.SerialComm.PortOpen == false) {
						strPortStatus = " COM" + Port + " not avail";
					} else {
						strPortStatus = " open";
					}
				}
			}
			// ConnCOMport
			//telnet
			if ((ConnectionType == (int)ConnType.ConnTelnet)) {
				//UPGRADE_NOTE: State was upgraded to CtlState. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
				if (frmSplash.Winsock.CtlState == MSWinsockLib.StateConstants.sckClosed) {
					frmSplash.Winsock.RemoteHost = strTelnet_IP_ADR;
					frmSplash.Winsock.RemotePort = Convert.ToInt32(strTelnet_PORT);
					frmSplash.Winsock.Connect();
					StartTime = Tools.DebugTimer;
					//small delay
					do {
						System.Windows.Forms.Application.DoEvents();
						WaitTime = /*System.Windows.Forms.*/ System.Threading.Timer() - StartTime;
						System.Windows.Forms.Application.DoEvents();
					} while (!((WaitTime > 0.05)));

					//UPGRADE_NOTE: State was upgraded to CtlState. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
					if (frmSplash.Winsock.CtlState != MSWinsockLib.StateConstants.sckConnecting & frmSplash.Winsock.CtlState != MSWinsockLib.StateConstants.sckConnected & frmSplash.Winsock.CtlState != MSWinsockLib.StateConstants.sckConnectionPending) {
						//UPGRADE_NOTE: State was upgraded to CtlState. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
						Interaction.MsgBox("Telnet connection error: " + (frmSplash.Winsock.CtlState));
						strPortStatus = " Telnet not avail";
						//updated once in frmSplash.search_COM_ports Analyzer.cwCOMdisconnect.Caption = Demo_Simul_string
						Analyzer.cwCOMdisconnect.Text = Demo_Simul_string;
					} else {
						strPortStatus = " open";
					}
				}
			}
			// ConnTelnet

			//simulation DLL
			if ((ConnectionType == (int)ConnType.ConnSimDLL)) {
				if ((DLL_is_Initialized == false))
					Init_DLL();
				//DLL checks if it is already initialized and ignores second attempt
			}
		}

		public static void Close_Port() {
			//COM port
			if ((ConnectionType == (int)ConnType.ConnCOMport)) {
				if ((frmSplash.SerialComm.PortOpen == true)) {
					frmSplash.SerialComm.PortOpen = false;
					// close the port to change settings
					//            FramePort.Caption = "COM" & frmSplash.SerialComm.CommPort & " DISconnected"
				}
				//telnet
			} else {
				//UPGRADE_NOTE: State was upgraded to CtlState. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"'
				// close the port to change settings
				if (frmSplash.Winsock.CtlState != MSWinsockLib.StateConstants.sckClosed) {
					frmSplash.Winsock.Close();
				}
			}
			//simulation DLL
			if ((ConnectionType == (int)ConnType.ConnSimDLL)) {
				if ((DLL_is_Initialized == true))
					Cmd_Stop_DLL();
				//DLL works in real time on separate thread. This stops that thread
			}
			strPortStatus = " closed";
			frmTerminal.Update_Form_Caption();
		}

		public static void portproblems() {
			if (DLL_is_available == true) {
				if ((ConnectionType == (int)ConnType.ConnCOMport)) {
					response = Convert.ToString(Interaction.MsgBox("Switch to Simulation mode?", MsgBoxStyle.YesNo, strPortNum + " port is not available, probably used by other application"));
				} else {
					response = Convert.ToString(Interaction.MsgBox("Switch to Simulation mode?", MsgBoxStyle.YesNo, "IP address: " + strTelnet_IP_ADR + ", port" + strTelnet_PORT + " is not available"));
				}
				if (response == Convert.ToString(MsgBoxResult.Yes)) {
					ConnectionType = (int)ConnType.ConnSimDLL;
					if ((DLL_is_Initialized == false))
						Init_DLL();
					//DLL checks if it is already initialized and ignores second attempt
					Analyzer.cwCOMdisconnect.Text = Demo_Simul_string;
					Analyzer.UpdateConnectControls((false));
				}
			} else {
				if ((ConnectionType == (int)ConnType.ConnCOMport)) {
					response = Convert.ToString(Interaction.MsgBox("Switch to DEMO mode?", MsgBoxStyle.YesNo, strPortNum + " port is not available, probably used by other application"));
				} else {
					response = Convert.ToString(Interaction.MsgBox("Switch to DEMO mode?", MsgBoxStyle.YesNo, "IP address: " + strTelnet_IP_ADR + ", port" + strTelnet_PORT + " is not available"));
				}
				if (response == Convert.ToString(MsgBoxResult.Yes)) {
					ConnectionType = (int)ConnType.ConnDEMO;
					Analyzer.cwCOMdisconnect.Text = Demo_Simul_string;
					Analyzer.UpdateConnectControls((false));
				}
			}
		}

		public static bool ReadPortSetup(bool SkipDialog = false)
	{
		bool functionReturnValue = false;
		//DataFileName must be set in advance if argument is true
		 // ERROR: Not supported in C#: OnErrorStatement

		//user wants to read a test file
		//First get the filename
		string DefaultFilename = null;
		string FileName = null;
		int i = 0;
		string temp_str = null;

		functionReturnValue = false;
		// prepare for the worse - file eroor, not recognized, etc
		if (SkipDialog == false) {
			DefaultFilename = "TMC.port";
			//UPGRADE_WARNING: The CommonDialog CancelError property is not supported in Visual Basic .NET. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="8B377936-3DF7-4745-AA26-DD00FA5B9BE1"'
			frmSplash.dlgCommonDialog.CancelError = true;
			frmSplash.dlgCommonDialogOpen.Title = "Choose the Port Setup file to load";
			frmSplash.dlgCommonDialogSave.Title = "Choose the Port Setup file to load";
			// frmSplash.dlgCommonDialog.FileName = DefaultFilename
			frmSplash.dlgCommonDialogOpen.InitialDirectory = Application_Dir;
			frmSplash.dlgCommonDialogSave.InitialDirectory = Application_Dir;
			frmSplash.dlgCommonDialogOpen.DefaultExt = "port";
			frmSplash.dlgCommonDialogSave.DefaultExt = "port";
			//UPGRADE_WARNING: Filter has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
			frmSplash.dlgCommonDialogOpen.Filter = "Data File (*.port)|*.port|All files (*.*)|*.*";
			frmSplash.dlgCommonDialogSave.Filter = "Data File (*.port)|*.port|All files (*.*)|*.*";
			frmSplash.dlgCommonDialogOpen.ShowDialog();
			//dlgCommonDialog.ShowSave
			//Cancel is selected
			if (Err.Number == 32755) {
				return functionReturnValue;
			} else {
				//        txtFileName.Text = "Reading measurement from file '" & dlgCommonDialog.FileName & " ....."
			}
			//SaveFileName becomes "C:\test\my_test.txt"

			//    SaveFileName = Left(dlgCommonDialog.FileName, Len(dlgCommonDialog.FileName) - 4) ' remove ".txt" from full file path
			DataFileName = frmSplash.dlgCommonDialogOpen.FileName;
			//SaveFileName becomes "C:\test\my_test"
		} else {
			if (FileExists(".\\TMC.port"))
				DataFileName = "TMC.port";
		}
		//SkipDialog
		if (string.IsNullOrEmpty(DataFileName)) {
			Interaction.MsgBox("File name is empty", MsgBoxStyle.Critical, "File 'TMC.port' not found or corrupt");
			return functionReturnValue;
		}
		i = Strings.InStrRev(DataFileName, "\\");
		FileName = Strings.Right(DataFileName, Strings.Len(DataFileName) - i);
		//i.e. "my_test"

		//now lets open the file and read the data
		FileSystem.FileOpen(1, DataFileName, OpenMode.Input);

		temp_str = FileSystem.LineInput(1);
		//    "TMC PORT SETUP " FileName
		// decode line
		i = Strings.InStr(temp_str, "TMC PORT SETUP");
		if (i != 0) {
			//        Print #1, "PORT COM"' Port
			//        Print #1, strPortSetting
			temp_str = FileSystem.LineInput(1);
			//    "PORT COM"; Port OR "TELNET 192.168.1.37, port 2020"
			i = Strings.InStrRev(temp_str, "M");
			//string PORT COM
			if (i != 0) {
				//port number
				if (IsNumber( temp_str.Substring( i + 1))) {
					Port = Convert.ToInt32(Strings.Mid(temp_str, i + 1));
				} else {
					i = (int)Interaction.MsgBox("Port Number 'COM" + temp_str + "' is not recognized in the file" + Constants.vbCrLf + DataFileName, Microsoft.VisualBasic.MsgBoxStyle.Exclamation, "Wrong Port Settings in the file");
					FileSystem.FileClose(1);
					return functionReturnValue;
				}

				temp_str = FileSystem.LineInput(1);
				//    "115200,N,8,1"
				if ((IsNumber( Strings.Left(temp_str, 3)))) {
					strPortSetting = temp_str;
					strPortNum = "COM" + Strings.LTrim(Conversion.Str(Port));
					ConnectionType = (int)ConnType.ConnCOMport;
				} else {
					i = (int)Interaction.MsgBox("Port Settings Line '" + temp_str + "' is not recognized in the file" + Constants.vbCrLf + DataFileName, Microsoft.VisualBasic.MsgBoxStyle.Exclamation, "Wrong Port Settings in the file");
					FileSystem.FileClose(1);
					return functionReturnValue;
				}
			// no 'M'
			} else {
				i = Strings.InStrRev(temp_str, "IP:");
				//string TELNET IP:
				if (i != 0) {
					strPortNum = Strings.LTrim(Strings.Mid(temp_str, i + 3));
					strTelnet_IP_ADR = strPortNum;
				} else {
					i = (int)Interaction.MsgBox("IP Address '" + temp_str + "' is not recognized in the file" + Constants.vbCrLf + DataFileName, Microsoft.VisualBasic.MsgBoxStyle.Exclamation, "Wrong Port Settings in the file");
					FileSystem.FileClose(1);
					return functionReturnValue;
				}

				temp_str = FileSystem.LineInput(1);
				//TELNET PORT: 2020
				i = Strings.InStrRev(temp_str, "RT:");
				//string TELNET PORT:
				if (i != 0) {
					if ((Information.IsNumeric(Strings.Mid(temp_str, i + 4)))) {
						strPortSetting = Strings.LTrim(Strings.Mid(temp_str, i + 3));
						//telnet port
						strTelnet_PORT = strPortSetting;
						ConnectionType = (int)ConnType.ConnDEMO;//false;
					}
				} else {
					i = (int)Interaction.MsgBox("Settings Line '" + temp_str + "' is not recognized in the file" + Constants.vbCrLf + DataFileName, Microsoft.VisualBasic.MsgBoxStyle.Exclamation, "Wrong Port Settings file");
					FileSystem.FileClose(1);
					return functionReturnValue;
				}
			}
		} else {
			i = Interaction.MsgBox("File Signature '" + temp_str + "' is not recognized", Microsoft.VisualBasic.MsgBoxStyle.Exclamation, "Wrong Port Settings in the file");
			FileSystem.FileClose(1);
			return functionReturnValue;
		}
		FileSystem.FileClose(1);
		functionReturnValue = true;
		return functionReturnValue;
		// success
		//open_port_if_closed sets port name according to the TMC.port, but user might want to change it
		//    Call open_port_if_closed
	}


		public static void SavePortSetup() {
			// ERROR: Not supported in C#: OnErrorStatement

			//user wants to save a test file
			//First get the filename
			string DefaultFilename = null;
			string SaveFileName = null;
			string FileName = null;
			int i = 0;

			DefaultFilename = "TMC.port";
			//UPGRADE_WARNING: The CommonDialog CancelError property is not supported in Visual Basic .NET. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="8B377936-3DF7-4745-AA26-DD00FA5B9BE1"'
			frmSplash.dlgCommonDialog.CancelError = true;
			frmSplash.dlgCommonDialogOpen.Title = "Choose name for the Port Setup";
			frmSplash.dlgCommonDialogSave.Title = "Choose name for the Port Setup";
			frmSplash.dlgCommonDialogOpen.InitialDirectory = Application_Dir;
			frmSplash.dlgCommonDialogSave.InitialDirectory = Application_Dir;
			frmSplash.dlgCommonDialogOpen.DefaultExt = "port";
			frmSplash.dlgCommonDialogSave.DefaultExt = "port";
			//UPGRADE_WARNING: Filter has a new behavior. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="9B7D5ADD-D8FE-4819-A36C-6DEDAF088CC7"'
			frmSplash.dlgCommonDialogOpen.Filter = "Port Setup File (*.port)|*.port|All files (*.*)|*.*";
			frmSplash.dlgCommonDialogSave.Filter = "Port Setup File (*.port)|*.port|All files (*.*)|*.*";
			frmSplash.dlgCommonDialogOpen.FileName = DefaultFilename;
			frmSplash.dlgCommonDialogSave.FileName = DefaultFilename;
			//if enabled,  FILE NAME SHOULD NOT HAVE ':'
			frmSplash.dlgCommonDialogSave.ShowDialog();
			frmSplash.dlgCommonDialogOpen.FileName = frmSplash.dlgCommonDialogSave.FileName;
			//Cancel is selected
			if (Err.Number == 32755) {
				return;
			} else {
				//        txtFileName.Text = "File(s)=" & dlgCommonDialog.FileName
			}
			//SaveFileName becomes "C:\test\my_test.txt"

			//    SaveFileName = Left(dlgCommonDialog.FileName, Len(dlgCommonDialog.FileName) - 4) ' remove ".txt" from full file path
			SaveFileName = frmSplash.dlgCommonDialogOpen.FileName;
			//SaveFileName becomes "C:\test\my_test"
			i = Strings.InStrRev(SaveFileName, "\\");
			FileName = Strings.Right(SaveFileName, Strings.Len(SaveFileName) - i);
			//i.e. "my_test"

			//    SaveFileName = dlgCommonDialog.FileName
			//now lets open the file and write the data
			FileSystem.FileOpen(1, SaveFileName, OpenMode.Output);
			//Values
			FileSystem.PrintLine(1, "TMC PORT SETUP " + FileName);
			if ((ConnectionType == (int)ConnType.ConnCOMport)) {
				strPortSetting = strBaud + "," + strParity.Value + "," + strBitnum.Value + "," + strBitstop;
				strPortNum = frmSplash.s_port.Text;
				FileSystem.PrintLine(1, "PORT " + strPortNum);
				FileSystem.PrintLine(1, strPortSetting);
			} else {
				FileSystem.PrintLine(1, "TELNET IP: " + strPortNum);
				FileSystem.PrintLine(1, "TELNET PORT: " + strTelnet_PORT);
			}
			FileSystem.PrintLine(1, "Date: " + DateString);
			FileSystem.PrintLine(1, "Time: " + TimeString);
			FileSystem.FileClose(1);
		}
#endif //#if false

			public static double Make180phaseShift(ref double phaseIN) {
				double phaseOut = 0;
				phaseOut = phaseIN + 180;
				if (phaseOut > 180) {
					phaseOut = phaseOut - 360;
					//<=0
				} else {
					//        phaseOut = phaseIN - 180
					//        If phaseOut < -180 Then phaseOut = phaseOut + 180
				}
				return phaseOut;
			}

			//-----------------------------------------------
			// Complex Number Math Functions
			//-----------------------------------------------

			public static tComplex Complex(double dReal, double dImag) {
				tComplex functionReturnValue = default(tComplex);
				//Set the real and imaginary numbers in a complex number type
				functionReturnValue.Re = dReal;
				functionReturnValue.Im = dImag;
				return functionReturnValue;
			}

			public static tComplex IM_NEG(ref tComplex arg1) {
				//Compute unary - operation on a single complex number,
				//returns a complex number
				return Complex(-(arg1.Re), -(arg1.Im));
			}

			public static tComplex IM_ADD(ref tComplex arg1, ref tComplex arg2) {
				//Addition of two complex numbers, returns a complex number
				return Complex(arg1.Re + arg2.Re, arg1.Im + arg2.Im);
			}

			public static tComplex IM_SUB(ref tComplex arg1, ref tComplex arg2) {
				//Subtraction of two complex numbers, returns a complex number
				return Complex(arg1.Re - arg2.Re, arg1.Im - arg2.Im);
			}

			public static tComplex IM_MULT(ref tComplex arg1, ref tComplex arg2) {
				//Multiplication of two complex numbers, returns a complex number
				return Complex((arg1.Re * arg2.Re) - (arg1.Im * arg2.Im), (arg1.Re * arg2.Im) + (arg2.Re * arg1.Im));
			}

			public static tComplex IM_DIV(ref tComplex arg1, ref tComplex arg2) {
				//division of two complex numbers:
				// multiply the numerator and denominator by the complex conjugate of the denominator,
				//for example, with z_1=a+bi and z_2=c+di, z=z_1/z_2 is given by
				//numerator and denomerator  two complex numbers, returns a complex number
				tComplex Denom_Conj = default(tComplex);
				double Denom_Mag = 0;
				//UPGRADE_WARNING: Couldn't resolve default property of object Denom_Conj. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
				Denom_Conj = Complex(arg2.Re, -arg2.Im);
				Denom_Mag = arg2.Re * arg2.Re + arg2.Im * arg2.Im;
				return Complex(((arg1.Re * arg2.Re) + (arg1.Im * arg2.Im)) / Denom_Mag, ((arg1.Im * arg2.Re) - (arg1.Re * arg2.Im)) / Denom_Mag);
			}

			//===============================================================================================================================
			// this function calculates magnitude-phase prediction based on filter coefficients
			// this simulates DSP, which works with 32 bit floats. DO NOT replace float with double
			//===============================================================================================================================
			/*
				public static tMag_Phase Mag_Ph_vs_Freq(ref double_iir Coeff, ref double freq, double LoopFreq) {
					tMag_Phase functionReturnValue = default(tMag_Phase);
					// ERROR: Not supported in C#: OnErrorStatement

					// if frequency = 0 it gives division by zero
					double Omega = 0;
					tComplex H = default(tComplex);
					tComplex Num = default(tComplex);
					tComplex Den = default(tComplex);

					Omega = freq * 2 * PI / LoopFreq;
					Num = Numerator_Complex(ref Coeff, ref Omega);
					Den = Denominator_Complex(ref Coeff, ref Omega);
					H = IM_DIV(ref Num, ref Den);
					functionReturnValue.Mag = System.Math.Sqrt(H.Re * H.Re + H.Im * H.Im);
					if ((functionReturnValue.Mag == 0))
						functionReturnValue.Mag = 1E-05;
					//we take a Log and it cannot be a zero' 1e-5 means it is an error
					if ((H.Re != 0)) {
						functionReturnValue.Phase = -System.Math.Atan(H.Im / H.Re) * 180 / PI;
						// returns between -90 and +90
					} else {
						functionReturnValue.Phase = -360;
						//this means it is an error
					}
					return functionReturnValue;
				}

				public static tComplex Numerator_Complex(ref double_iir Coeff, ref double Omega) {
					double NumRe = 0;
					double NumIm = 0;
					NumRe = Coeff.b0 + Coeff.b1 * System.Math.Cos(Omega) + Coeff.b2 * System.Math.Cos(2 * Omega);
					NumIm = Coeff.b1 * System.Math.Sin(Omega) + Coeff.b2 * System.Math.Sin(2 * Omega);
					return Complex(NumRe, NumIm);
				}

				public static tComplex Denominator_Complex(ref double_iir Coeff, ref double Omega) {
					double DenRe = 0;
					double DenIm = 0;
					DenRe = 1 - Coeff.a1 * System.Math.Cos(Omega) - Coeff.a2 * System.Math.Cos(2 * Omega);
					DenIm = -Coeff.a1 * System.Math.Sin(Omega) - Coeff.a2 * System.Math.Sin(2 * Omega);
					return Complex(DenRe, DenIm);
				}
		 */
			//'''''''''''' End of complex math '''''''''''''''''''''''''''''''''''''''''''''''

			public static string ExtendedHEX(int int_in) {
				string functionReturnValue = null;
				// returns 0-9, A-F, G-Z. if error, returns '-'
				string ConvertedCHAR = null;
				//error, return '='
				if (int_in > (9 + 27)) {
					ConvertedCHAR = "-";
					functionReturnValue = ConvertedCHAR;
					return functionReturnValue;
				}

				if ((int_in < 10)) {
					ConvertedCHAR = Conversion.Hex(int_in);
				} else {
					ConvertedCHAR = Conversion.Hex('A' + int_in - 10);
				}
				functionReturnValue = ConvertedCHAR;
				return functionReturnValue;
			}

			public static bool IsNumber(string Expression) {
				bool functionReturnValue = false;
				bool Negative = false;
				bool Exponent = false;
				bool Number = false;
				bool Period = false;
				bool Positive = false;
				string a_char = null;
				int X = 0;
				functionReturnValue = false;
				for (X = 1; X <= Strings.Len(Expression); X++) {
					a_char = Strings.Mid(Expression, X, 1);
					switch (a_char) {
						case "0": // TODO: to "9"
						Number = true;
						break;

						case "-":
						if (Negative == false) {
							Negative = true;
							//Negative = True =====> second '-'
						} else {
							if (!Exponent)
								return functionReturnValue;
							// catch third non valid '-' if in exponent
							if (Period | Number | Negative)
								return functionReturnValue;
							// catch second minus
						}
						break;

						case ".":
						if (Period)
							return functionReturnValue;
						// catch second '.'
						Period = true;
						break;

						case " ":
						case Constants.vbTab:
						case Constants.vbVerticalTab:
						case Constants.vbCr:
						case Constants.vbLf:
						case Constants.vbFormFeed:
						// if it is in the last position after some numbers, Val will convert it
						if (Period | Number | Negative) {
							// number was already detected
							if (Number) {
								functionReturnValue = Number;
								// so Val can decode up to this separator
							}
							return functionReturnValue;
							// catch separator inside string
						}
						break;

						case "E":
						case "e":
						if (!Number)
							return functionReturnValue;
						// catch second 'e' or 'E'
						if (Exponent)
							return functionReturnValue;
						Exponent = true;
						Number = false;
						Negative = false;
						Period = false;
						Positive = false;
						break;

						case "+":
						if ((Positive == false)) {
							Positive = true;
							//Positive = True =====> second '+'
						} else {
							if (!Exponent)
								return functionReturnValue;
							// catch third non valid '+' if in exponent
							if (Number | Negative | Positive)
								return functionReturnValue;
							// catch second '+'
							Positive = true;
						}
						break;

						case ",":
						// if it is in the last position after some numbers, Val will convert it
						if (Number) {
							functionReturnValue = Number;
							return functionReturnValue;
							// catch separator inside string
						}
						break;
						default:
						return functionReturnValue;
					}
				}
				functionReturnValue = Number;
				return functionReturnValue;
			}

			public static bool IsDirectory(string sFile) {
				bool functionReturnValue = false;
				functionReturnValue = false;
				functionReturnValue = ((FileSystem.GetAttr(sFile) & FileAttribute.Directory) == FileAttribute.Directory);
				return functionReturnValue;
			}

			public static bool IsWindows64() {
				bool functionReturnValue = false;
				if (FileExists("C:\\WINDOWS\\SysWOW64\\format.com")) {
					functionReturnValue = true;
				} else {
					functionReturnValue = false;
				}
				return functionReturnValue;
			}


#if false
//------ temporary fake function to compile ---------------------
		public string GetSend(ref string message, ref bool Wait) {
			string functionReturnValue = null;
			functionReturnValue = message;
			return functionReturnValue;
		}
		//public string GetSend(ref string message, ref bool Wait) {
			string functionReturnValue = null;
			//    On Error GoTo port_problems
			float CmdStartTime = 0;
			float WaitTime = 0;
			string Cmd_String = null;
			// local copy in order not to collide from different calls
			int CR_pos = 0;
			int pos_OK = 0;
			int pos_response = 0;
			int got_response = 0;
			string temp_str = null;
			VB6.FixedLengthString TMP_BUF = new VB6.FixedLengthString(10000);
			// zero-filled, defined length (not resizable).
			float totalTime = 0;
			// VB6 bug? Buffer is limited in length. even if Len(Buffer)= 320, the watch window shows ~256 chars,
			// and when copied to output string only first 250 chars are copied?
			// TMP_BUF = Space(10000)

			//!!!! >>>> Replace(TMP_BUF, Chr(0), " ") TAKES ~0.6 sec if DEBUG UNDER Visual Studio 2010 !!!!
			//TMP_BUF = Replace(TMP_BUF, Chr(0), " ") ' replace  0 with space.

			CmdStartTime = VB.Timer();
			functionReturnValue = "";
			// for some locals, replace ',' with '.'
			message = Strings.Replace(message, ",", ".");
			txtManualCMD.Text = message;
			// diagnostic
			Cmd_String = message + Constants.vbCr;
			// Lf
			SendStrLen = Strings.Len(Cmd_String);
			pos_response = 1;
			// or InStr(pos_response, Buffer, vbLf) gives error

			if (ConnectionType == Module1.ConnType.ConnSimDLL) {
				//        Call Is_DLL_available ' it sets "DLL_is_available" which is global boolean
				if (DLL_is_available) {
					if ((DLL_is_Initialized == false))
						Init_DLL();
					Buffer = "";
					TMP_BUF.Value = Send_DLL_cmd(Cmd_String);
					goto get_response;
				} else {
					ConnectionType = Module1.ConnType.ConnDEMO;
					functionReturnValue = "DEMO MODE";
					goto finish_function;
				}
			}
		send_cmd:
			// program is here if serial port was available, and ConnType <> ConnSimDLL
			open_port_if_closed();
			//if port not avilable, it will set ConnType = ConnSimDLL or ConnDEMO
			if ((ConnectionType == Module1.ConnType.ConnSimDLL | ConnectionType == Module1.ConnType.ConnDEMO)) {
				//COM port
				if ((ConnectionType == Module1.ConnType.ConnCOMport)) {
					functionReturnValue = "Port not avail";
				}
				//winsock
				if ((ConnectionType == Module1.ConnType.ConnTelnet)) {
					functionReturnValue = "Telnet not avail";
				}
				//demo
				if ((ConnectionType == Module1.ConnType.ConnDEMO)) {
					functionReturnValue = "DEMO MODE";
				}
				LblCOMdisconnect.Text = Demo_Simul_string;
				goto finish_function;
				//port cannot be open
			}
			UpdateConnectControls(true);
			txtManualCMD.Text = message;
			// diagnostic
			// Send the  command
			Clean_COM_Buffer();
			//'clean the Comm port buffer
			txtStatus.Text = Cmd_String;
			// to debug
			//   txtStatus.Refresh 'this causes flickering of the screen
			//    CmdStartTime = Timer
			Send_GUI_command(Cmd_String);
			// >>>>>>>>>
			pos_response = 1;
			got_response = 0;
			if (Wait) {
				// Wait for data to come back to the serial port.
				do {
					System.Windows.Forms.Application.DoEvents();
					//basic command
					if (Strings.Len(Cmd_String) < 4) {
						//small delay
						do {
							System.Windows.Forms.Application.DoEvents();
							//  here another call of GetSend might happen
							//this does frmTerminal.Receive if GUI_doing_command if true
							WaitTime = VB.Timer() - CmdStartTime;
						} while (!((WaitTime > 0.01)));
					get_response:
						TMP_BUF.Value = Buffer;
						// local copy
						if (string.IsNullOrEmpty(TMP_BUF.Value)) {
							functionReturnValue = "Invalid answer";
							goto finish_function;
						}
						pos_OK = Strings.InStr(TMP_BUF.Value, Cmd_String);
						//too short command 'e' or 'f' getting response like "e" & vbCrLf & "Feedforward ENABLED...: 0f" & vbCrLf
						//         If (pos_OK < Len(Cmd_String) + 2) And (pos_OK > 0) Then ' If (pos_OK <>0) and it finds first command itself, then response is cut and leaves nothing
						// pos_OK =1 when controller sends echo
						if ((pos_OK == 1)) {
							TMP_BUF.Value = Strings.Mid(TMP_BUF.Value, pos_OK + Strings.Len(Cmd_String));
							//cut echo = beginning of TMP_BUF
						}
						pos_OK = Strings.InStr(Buffer, Constants.vbCr);
						//13;\r rear port string ends with vbCR only, no Lf
						if ((pos_OK != 0)) {
							pos_response = pos_OK;
							got_response = pos_OK;
							TMP_BUF.Value = TMP_BUF.Value + Constants.vbLf;
							//add Lf -because while() checks for it after Cr
						}
						// commands longer than 2 bytes+CrLf require return >~OK or >~ERR
					} else {
						TMP_BUF.Value = Buffer;
						// local copy
						pos_OK = Strings.InStr(TMP_BUF.Value, Acknowledgment_token);
						// ">~"
						if ((pos_OK != 0)) {
							got_response = pos_OK;
							//next, wait for final CrLf
							pos_OK = Strings.InStr(got_response, TMP_BUF.Value, Constants.vbCrLf);
							//13,10;\r\n
							if ((pos_OK != 0)) {
								pos_response = pos_OK;
								got_response = pos_OK;
							} else {
								pos_response = 1;
								got_response = 0;
							}
							//Timeout
						} else {
							pos_OK = Strings.InStr(TMP_BUF.Value, "Timeout");
							if (pos_OK) {
								functionReturnValue = TMP_BUF.Value;
								goto finish_function;
							}
						}
					}
					WaitTime = VB.Timer() - CmdStartTime;
					// 1-letter command is too short and miss end of response. LOOK for ":" in response
					// actual response: ASC(10) & "e" & ASC(13) & ASC(10) & "Feedforward ENABLED...". IT MUST BE IN ADDITION ":8e"
				} while (!((((got_response != 0) & (Strings.InStr(pos_response, Buffer, Constants.vbLf)) > got_response) | WaitTime > DefaultTimeOut)));
				lblCMDtime.Text = VB6.Format(WaitTime, "#0.####");
				//            If Cmd_String = "s" & vbCr Then
				//                DoEvents
				//            End If
				if (WaitTime > DefaultTimeOut) {
					Buffer = "Timeout";
					functionReturnValue = Buffer;
					goto finish_function;
				}
				// just show response
			} else {
				do {
					System.Windows.Forms.Application.DoEvents();
					//        call Get_Serial_Input
					TMP_BUF.Value = Buffer;
					WaitTime = VB.Timer() - CmdStartTime;
				} while (!(Strings.Len(TMP_BUF.Value) >= SendStrLen | WaitTime > 0.2));
				//safety delay 0.2 sec, if there is no transmission it can stack here
				//--        lblRawResponse.Caption = Buffer
			}
			//parse response
			pos_OK = Strings.InStr(TMP_BUF.Value, Acknowledgment_token);
			// ">~"
			if (pos_OK != 0) {
				Remove_echo(TMP_BUF.Value, Cmd_String);
				if ((echo_removed == true))
					echo_removed = false;
				// prepare for the next command
				pos_OK = Strings.InStr(TMP_BUF.Value, Constants.vbCrLf + Acknowledgment_token + "OK" + Constants.vbCr);
				// "\r\n>~OK\r"
				//last part of the response, like freq?\r\nfreq=5.0\r\n>~OK\r\n
				if (pos_OK != 0) {
					TMP_BUF.Value = VB.Left(TMP_BUF.Value, pos_OK - 1);
					//first part of response: s\r\ns\r\n>~OK The System Status is: 9e\r\n
				} else {
					got_response = Strings.InStr(pos_response + 5, TMP_BUF.Value, Constants.vbCrLf);
					// "\r\n>~OK\r" - search Cr Lf after >~OK
					if (got_response != 0)
						pos_response = got_response;
					if (pos_response != 0) {
						temp_str = VB.Left(TMP_BUF.Value, pos_response - 1);
						// cut either beginning or tail of response
					} else {
						temp_str = (TMP_BUF.Value);
						//  response
					}
					TMP_BUF.Value = temp_str;
				}
			}
			TMP_BUF.Value = Strings.Trim(TMP_BUF.Value);
			// if size is defined (As String * 10000), trim does not reduce size!
			if (VB.Right(TMP_BUF.Value, 2) == Constants.vbCrLf) {
				TMP_BUF.Value = VB.Left(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 2);
			}
			//ESC
			if (VB.Left(TMP_BUF.Value, 1) == Strings.Chr(27)) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 1);
			}

			if (VB.Left(TMP_BUF.Value, 2) == Constants.vbCrLf) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 2);
			}
			if (VB.Right(TMP_BUF.Value, 2) == Constants.vbCrLf) {
				TMP_BUF.Value = VB.Left(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 2);
			}
			if (VB.Left(TMP_BUF.Value, 1) == Constants.vbCr) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 1);
			}
			if (VB.Left(TMP_BUF.Value, 1) == Constants.vbCr) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 1);
			}
			if (VB.Left(TMP_BUF.Value, 1) == Constants.vbLf) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 1);
			}
			if (VB.Left(TMP_BUF.Value, 1) == Constants.vbLf) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 1);
			}
			if (VB.Left(TMP_BUF.Value, 1) == Constants.vbCr) {
				TMP_BUF.Value = VB.Right(TMP_BUF.Value, Strings.Len(TMP_BUF.Value) - 1);
			}

			functionReturnValue = Strings.Trim(TMP_BUF.Value);
			// <<<<<<<<<<<<  this is the return of the function, trim ending spaces when copied
			lstResponse.Items.Add(("-->" + message));
			// add command to the list
			Cmd_String = functionReturnValue;
			//update list
			do {
				System.Windows.Forms.Application.DoEvents();
				if (Cmd_String == "Timeout") {
					goto finish_function;
				}
				CR_pos = Strings.InStr(Cmd_String, Constants.vbLf);
				// there is a LF in the response on first position
				if (CR_pos == 1) {
					Cmd_String = Strings.Mid(Cmd_String, CR_pos + 1);
				}
				CR_pos = Strings.InStr(Cmd_String, Constants.vbCr);
				// there is a LF in the response on first position
				if (CR_pos == 1) {
					Cmd_String = Strings.Mid(Cmd_String, CR_pos + 1);
				}
				CR_pos = Strings.InStr(Cmd_String, Constants.vbCr);
				// there is no CR in the response
				if (CR_pos == 0) {
					CR_pos = Strings.InStr(Cmd_String, Constants.vbLf);
					// try LF in the response
				}
				// there is CR or LF in the response
				if (CR_pos) {
					lstResponse.Items.Add((Strings.Mid(Cmd_String, 1, CR_pos - 1)));
					Cmd_String = Strings.Mid(Cmd_String, CR_pos + 1);
					CR_pos = Strings.InStr(Cmd_String, Constants.vbLf);
					// there is a LF in the response on first position
					if (CR_pos == 1) {
						Cmd_String = Strings.Mid(Cmd_String, CR_pos + 1);
					}
					// no CR or LF left in string, add time taken
				} else {
					//            totalTime = Timer - CmdStartTime
					temp_str = VB6.Format(WaitTime * 1000, "0.0## ms");
					//            lstResponse.AddItem (RTrim(Cmd_String) & " // " & temp_str & " TotalT" & Format(totalTime * 1000, " 0.000 ms"))
					lstResponse.Items.Add((Strings.RTrim(Cmd_String) + " // " + temp_str));
					goto finish_function;
				}
			} while (!((Strings.Len(Cmd_String) < 1)));
		finish_function:
			// end adding to communication window
			GUI_doing_command = false;
			return functionReturnValue;
		port_problems:
			// release com port for next request

			portproblems();
			return functionReturnValue;
		}
#endif //#if false
		}
	}
}
