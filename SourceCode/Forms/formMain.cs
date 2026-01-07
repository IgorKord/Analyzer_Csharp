// I USE THE FOLLOWING MARKERS THROUGHOUT THIS PROJECT TO MAKE FINDING SPECIAL CODE COMMENTS EASY:
// GARY-NOTE:       Notes that go beyond standard code comments. Used to point out unusual things, features, or ideas.
// GARY-FIX:        Something that probably isn't right or that needs to be done to make the code ore robust.
// GARY-ERROR:      A place in the code that generates exceptions (errors) and that should be fixed.
// GARY-DEBUG:      Code that is only here for debugging.
// GARY-CHANGE:     A change that I (Gary) have made to the original (Venture-Technology) code that I'm concerned might break something.
// GARY-REMOVE:     Code or comments that I need to remember to remove once I'm sure I don't want them anymore.
//
// SEE THE TOP OF THE PROGRAM.CS FILE FOR MORE DOCUMENTATION NOTES.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TMCAnalyzer.Properties;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using NationalInstruments.UI.WindowsForms;

namespace TMCAnalyzer {

	/// <summary/>
	public enum CommStatusStates {
		/// <summary/>
		NotConnected,
		/// <summary/>
		DemoMode,
		/// <summary/>
		Connected,
		/// <summary/>
		CommError
	}

	public partial class formMain : Form {
		const double WRONG_VALUE = 77777.7;

		//*** DEBUGGING TO SHOW WHEN EVENTS OCCUR/FIRE
		private void formMain_VisibleChanged(object sender, EventArgs e) { System.Diagnostics.Debug.WriteLine("--- formMAIN_VisibleChanged ---"); }

		private void formMain_Activated(object sender, EventArgs e) { System.Diagnostics.Debug.WriteLine("--- formMAIN_Activated ---"); }

		private void formMain_Shown(object sender, EventArgs e) { System.Diagnostics.Debug.WriteLine("--- formMAIN_Shown ---"); }
		//LTF plot numbers
		const int Perfor = 0;
		const int PrevLTF = 1;
		const int RefLTF = 2;
		const int CohLTF = 8;
		const long MAX_Position_Sensors = 6;       // STACIS has 4 Load Sensors, ELDAMP has 3 or 6 proximity
		public int OutputChosen;
		public int InputChosen;
		public string LTF_command;
		static readonly object _object = new object();
		public bool ID_valid = false;
		public double PlotMaxFreq = 1000;
		public bool IsEchoEnable = false;
		public bool Test_in_progress = false;
		public string Buffer;
		private int BNC_0_index;
		public static bool AirIsoPresent;
		//Converted bool HasFloorFF into property + event
		private static bool _hasFloorFF;
		public static bool HasFloorFF {
			get => _hasFloorFF;
			set {
				if (_hasFloorFF != value) {
					_hasFloorFF = value;
					// Raise change event (null sender because it's a static event)
					HasFloorFFChanged?.Invoke(null, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Raised when HasFloorFF value is changed.
		/// Handlers (UI) must marshal to UI thread if needed.
		/// </summary>
		public static event EventHandler HasFloorFFChanged;

		frmDrivingOutput frmDrivingOutput = new frmDrivingOutput();
		frmSensorInput frmSensorInput = new frmSensorInput();

		//private T EnsureForm<T>(ref T formRef) where T : Form, new() {
		//	if (formRef == null || formRef.IsDisposed) {
		//		formRef = new T();
		//		formRef.FormClosed += (s, e) => { formRef = null; };
		//	}
		//	return formRef;
		//}

		/**********************************************************************************************************************/
		//*** FIRES WHEN FORM-INSTANCE IS CREATED
		public formMain() {
			Program.IsReadingControllerSettings = true; // prevent sending SET commands to controller
			frmFilters = new frmFilters(this);

		//	_timerforFixedFreq = new System.Timers.Timer(10);
		//	_timerforFixedFreq.Elapsed += new System.Timers.ElapsedEventHandler(_timerforFixedFreq_Elapsed);

			_timerforRT = new System.Timers.Timer(1000);
			_timerforRT.Elapsed += new System.Timers.ElapsedEventHandler(_timerforRT_Elapsed);

			System.Diagnostics.Debug.WriteLine("--- formMAIN() ---");

			ContinuousMonitoringOn = false;
			// Register message filter.
			// GARY-FIX:        Venture Technologies added this message filter (defined in MessageFilter.cs)
			// it seems to be used to handle some mouse down error message in the terminal
			// screen -- when in Advanced Menu Mode.
			// I am commenting it out because it's messing with me on the frmConnect Screen (WHY??)
			// ALSO, should this be done here? Shouldn't this be done in PROGRAM
			//mdr 053118//Application.AddMessageFilter(msgFilter);

			TMC_Path = null;//use defaults

			/* mdr 053118 */
			//artem 060518 commented back in section commented out by mdr0531818
			var sPt = new SerialPort("DEMO", 115200); //serial port
			var tel = new TcpClient();
			SerPort mux;    //muxer
			if (Program.FormSplash.IsPortUSB)
				mux = new SerPort(sPt);
			else
				mux = new SerPort(tel);
			// mux.ReadTimeout = mux.WriteTimeout = 10 //ms;

			Program.ConnectedController = new TMCController(mux); // LOCATION-TAG: CONTROLLER-OBJECT-CREATED
			Program.ConnectedController.Open();

			/* mdr 053118 */
			LTFIsRunning = false;


			// Standard code added by visual studio...what is in formMain.Designer.cs
			// It creates all the controls on the form and sets their relations
			// (like in a group box or on a panel).
			//
			// It can not be run until now (after ConnectedController is open) because some of
			// the control-initializations access the controller.
			InitializeComponent();

			Initialize_Controls();
			Program.IsReadingControllerSettings = false;
		}

		/**********************************************************************************************************************/
		//*** FIRES WHEN FORM-OBJECT IS LOADED
		private void FormMainOnLoad(object sender, EventArgs e) {
			System.Diagnostics.Debug.Assert(timerContinuous.Enabled);
			BusyWarningPrepare(); // IK20211228 keep just in case if warning is needed
			ConnectionStatusPrepare();
			//  ClearControlValues();

			// When we create a new main form...be sure size is small enough to fit on the smallest screen resolution we are shooting for.
			// That is a 1366x768 screen. Subtracting about 5% of the width and 10% of the height (for the task bar) we need 1300x700.
			// I've standardized on 1320 x 700.
			// We set this here so that even if the developer changes something...we will notice the size is wrong as soon as it's run.
			//this.Size = new System.Drawing.Size(700, 1320);   // Width = 700   Height = 1320
			// Program.FormMain.Size = new System.Drawing.Size(700, 1320);   // Width = 700   Height = 1320
			Program.FormMain.Width = 1100;
			Program.FormMain.Height = 730;

			// Initialize the colors for all the menu items and page-tab-control backgrounds.
			GetDefaultColorsFromDevelopmentEnvironment();
			SetMenuAndTabToCOLORS();
			if (Program.WhiteMode)
				SetMenuAndTabColorsToWhiteMode();

			ContinuousMonitoringOn = false;
			ContinuousMonitoringSwitchShow();
			ExcitationStatus = ToggleExcitation.Checked;
			// Set All Switches to Off
			Program.IsReadingControllerSettings = true;
			try {
				Enable_frames();
				MenuPageChosenSETTINGS();
			} catch (System.Exception ex) {
				throw ex;
			} finally { Program.IsReadingControllerSettings = false; }
		}

		//double M3v_Cmd_Word;

		List<CheckBox> ToggleAxisEn = new List<CheckBox>();
		List<NumericEdit> NInumFBgain = new List<NumericEdit>();
		List<double> BNC_ptr = new List<double>();
		List<CheckBox> cwButEnDisPOSTtest = new List<CheckBox>();
		List<CheckBox> CheckAlarmBit = new List<CheckBox>();
		List<Label> LblChPower = new List<Label>();
		List<Label> lblProxSensor = new List<Label>();
		List<Label> lbl_IsoPressure = new List<Label>();
		List<Label> lblValveCurrent = new List<Label>();
		List<NumericEdit> NInumGoNoGOwindow = new List<NumericEdit>();
		List<NumericEdit> NInumHeightAdj = new List<NumericEdit>();
		List<NumericEdit> NInumPressureSetPoint = new List<NumericEdit>();

		double self_test_word;
		bool Init_sys_data;
		/**********************************************************************************************************************/
		// this function initializes controls by looking at authority level and initializes alarm list
		private void Initialize_Controls() {

			//TMC Analyzer related
			lblSystemDate.Text = DateTime.Now.Date.ToString();
			cwBNC1select.Visible = false;
			cwBNC0select.Visible = false;
			CmbBNC0.Visible = true;
			CmbBNC1.Visible = true;

			ToggleAxisEn.Clear();
			ToggleAxisEn.Add(ToggleAxisEn0);    // for ref: Tag="loop_FBA" , "Vel Feed Back ALL"
			ToggleAxisEn.Add(ToggleAxisEn1);	// for ref: Tag="loop_FB_6", "Vel FB X"
			ToggleAxisEn.Add(ToggleAxisEn2);    // for ref: Tag="loop_FB_7", "Vel FB Y"
			ToggleAxisEn.Add(ToggleAxisEn3);    // for ref: Tag="loop_FB_8", "Vel FB tZ"
			ToggleAxisEn.Add(ToggleAxisEn4);    // for ref: Tag="loop_FB_9", "Vel FB Z"
			ToggleAxisEn.Add(ToggleAxisEn5);    // for ref: Tag="loop_FB_A", "Vel FB tX"
			ToggleAxisEn.Add(ToggleAxisEn6);    // for ref: Tag="loop_FB_B", "Vel FB tY"

			NInumFBgain.Clear();
			NInumFBgain.Add(NInumFBgain0);
			NInumFBgain.Add(NInumFBgain1);
			NInumFBgain.Add(NInumFBgain2);
			NInumFBgain.Add(NInumFBgain3);
			NInumFBgain.Add(NInumFBgain4);
			NInumFBgain.Add(NInumFBgain5);
			NInumFBgain.Add(NInumFBgain6);
			NInumFBgain.Add(NInumFBgain7);
			NInumFBgain.Add(NInumFBgain8);
			NInumFBgain.Add(NInumFBgain9);
			NInumFBgain.Add(NInumFBgain10);
			NInumFBgain.Add(NInumFBgain11);
			NInumFBgain.Add(NInumFBgain12);
			NInumFBgain.Add(NInumFBgain13);
			NInumFBgain.Add(NInumFBgain14);
			NInumFBgain.Add(NInumFBgain15);
			NInumFBgain.Add(NInumFBgain16);
			NInumFBgain.Add(NInumFBgain17);
			NInumFBgain.Add(NInumFBgain18);
			NInumFBgain.Add(NInumFBgain19);
			NInumFBgain.Add(NInumFBgain20);
			NInumFBgain.Add(NInumFBgain21);

			NInumGoNoGOwindow.Clear();
			NInumGoNoGOwindow.Add(NInumGoNoGOwindow0);
			NInumGoNoGOwindow.Add(NInumGoNoGOwindow1);
			NInumGoNoGOwindow.Add(NInumGoNoGOwindow2);
			NInumGoNoGOwindow.Add(NInumGoNoGOtime);

			NInumHeightAdj.Clear();
			NInumHeightAdj.Add(NInumHeightAdj0);
			NInumHeightAdj.Add(NInumHeightAdj1);
			NInumHeightAdj.Add(NInumHeightAdj2);

			NInumPressureSetPoint.Clear();
			NInumPressureSetPoint.Add(NInumPressureSetPoint0);
			NInumPressureSetPoint.Add(NInumPressureSetPoint1);
			NInumPressureSetPoint.Add(NInumPressureSetPoint2);
			NInumPressureSetPoint.Add(NInumPressureSetPoint3);

			lblProxSensor.Clear();
			lblProxSensor.Add(_lblProxSensor0); // Horiz 1
			lblProxSensor.Add(_lblProxSensor1); // Horiz 2
			lblProxSensor.Add(_lblProxSensor2); // Horiz 3
			lblProxSensor.Add(_lblProxSensor3); // Vert 1
			lblProxSensor.Add(_lblProxSensor4); // Vert 2
			lblProxSensor.Add(_lblProxSensor5); // Vert 3

			//NInumFBgain.Add(NInumFBgain0); NInumFBgain.Add(NInumFBgain1); NInumFBgain.Add(NInumFBgain2); NInumFBgain.Add(NInumFBgain3);
			//NInumFBgain.Add(NInumFBgain4); NInumFBgain.Add(NInumFBgain5); NInumFBgain.Add(NInumFBgain6); NInumFBgain.Add(NInumFBgain7);
			//NInumFBgain.Add(NInumFBgain8); NInumFBgain.Add(NInumFBgain9); NInumFBgain.Add(NInumFBgain10); NInumFBgain.Add(NInumFBgain11);

			lbl_IsoPressure.Clear();
			lbl_IsoPressure.Add(_lblPressureIso1);	// Iso #1
			lbl_IsoPressure.Add(_lblPressureIso2);	// Iso #2
			lbl_IsoPressure.Add(_lblPressureIso3);	// Iso #3
			lbl_IsoPressure.Add(_lblPressureIso4);	// Iso #4
			lbl_IsoPressure.Add(_lblPressureInput); // Input

			lblValveCurrent.Clear();
			lblValveCurrent.Add(_lblCurrentValve1);	// Iso #1
			lblValveCurrent.Add(_lblCurrentValve2);	// Iso #2
			lblValveCurrent.Add(_lblCurrentValve3);	// Iso #3
			lblValveCurrent.Add(_lblCurrentValve4);	// Iso #4

			BNC_ptr.AddRange(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
			ReadBNCchannelsFromSlider();

			if (ChkBNCsliders.Checked == true) {
				ChkBNCsliders.BackColor = ColorTranslator.FromHtml("#c0ffff");
				ChkBNCsliders.Text = "drop down";
				cwBNC0select.Visible = true;
				cwBNC1select.Visible = true;
				CmbBNC0.Visible = false;
				CmbBNC1.Visible = false;
				//   ChkBNCsliders.Text = "Slider";

				cwBNC0select.BringToFront();
				cwBNC1select.BringToFront();
			} else {
				ChkBNCsliders.BackColor = ColorTranslator.FromHtml("#c0ffc0");
				ChkBNCsliders.Text = "SLIDERS";
				cwBNC0select.Visible = false;
				cwBNC1select.Visible = false;
				CmbBNC0.Visible = true;
				CmbBNC1.Visible = true;
				//  ChkBNCsliders.Text = "Drop";
				cwBNC0select.SendToBack();
				cwBNC1select.SendToBack();
			}

			AssignChNames();

			//End of Analyzer

			// Start the form with "Continuous Monitoring" turned off.
			// Calling this function sets the proper colors for all the controls
			// That have to do with continuous monitoring. It also sets the
			// switch to OFF and the procedure-level-variable that keeps track of
			// if we are monitoring to OFF
			// cwBut_RT_Monitoring.Value = false;
			// ContinuousMonitoringStatus = ContinuousMonitoringModes.Off;
			SetContinuousMonitoring(false);
			try {
				if (Program.ConnectedController.CommStatus == CommStatusStates.Connected)
					ReadSettingsFromController();
			} catch (System.Exception ex) { System.Diagnostics.Debug.Assert(ex != null); }
		}

		void ReadBNCchannelsFromSlider() {
			double ch_index;

			int slider_channels;
			double MAX_Channels;
			MAX_Channels = cwBNC0select.Range.Maximum;
			for (slider_channels = 1; (slider_channels <= MAX_Channels); slider_channels++) {
				ch_index = SlideItemValue(slider_channels - 1);
				BNC_ptr[(slider_channels - 1)] = ch_index;
			}
		}


		/**********************************************************************************************************************/
		//=== FORM UNLOAD
		// NOTE: ORDER OF CALLS WHEN FORM is CLOSED:
		// FormClosing         -- Occurs BEFORE form is closed. Setting CANCEL = TRUE will stop the from from closing.
		// FormClosed          -- Occurs when the form IS closing. Use this space to clean up resources
		// Deactivate          -- Occurs when for form loses focus (another form gets focus) BUT this form may still be open!
		//***
		private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
			// User has pressed the "X" button on the form to close the program.
			// REMOVED THIS CODE BECAUSE I DON'T WANT TO ASK AN "ARE YOU SURE" MESSAGE
			//DialogResult result;
			//result = MessageBox.Show("Close the Analyzer program?", "TMC Analyzer CLOSING", MessageBoxButtons.YesNo);
			//if (result.Equals(DialogResult.Yes))
			//{
			// Shut off the timers on the form.
			Timer_CrossTalk.Enabled = false;
			TimerChangingMode.Enabled = false;
			Timer_MonitorSimulationMode.Enabled = false;
			//mdr 053118//Application.RemoveMessageFilter(msgFilter);
			CloseDownAndExit();
			Application.Exit();
		}
		private void formMain_FormClosed(object sender, FormClosedEventArgs e) { }
		private void formMain_Deactivate(object sender, EventArgs e) { }


		/**********************************************************************************************************************/
		// GARY-FIX:    Is this really the best way to close?
		// Should I close the forms by calling the same routines they use when closing themselves?
		private void CloseDownAndExit() {
			//if (Program.FormScope != null) {
			//	Program.DisconnectScope();
			//	if (Program.FormScope.InvokeRequired) {
			//		Program.FormScope.BeginInvoke((MethodInvoker)delegate
			//		{
			//			Program.FormScope.Close();
			//			Program.FormScope.Dispose();
			//		});
			//	} else {
			//		Program.FormScope.Close();
			//		Program.FormScope.Dispose();
			//	}
			//	if (ScopeThread.ThreadState == System.Threading.ThreadState.Suspended)
			//		ScopeThread.Resume();
			//	ScopeThread.Abort();
			//}
			if (Program.FormRTMonitoring != null) {
				Program.FormRTMonitoring.Close();
				Program.FormRTMonitoring.Dispose();
			}
			if (Program.FormGains != null) {
				Program.FormGains.FormTerminalVisible = true;  // to avoid refresh while closing
				Program.FormGains.Close();
				Program.FormGains.Dispose();
			}
			if (Program.formTerminal != null) {
				Program.formTerminal.Close();
				Program.formTerminal.Dispose();
			}
			if (Program.ConnectedController != null) {
				Program.ConnectedController.Close();
				Program.ConnectedController.Dispose();
			}
			if (Program.FormSplash != null) {
				Program.FormSplash.Close();
				Program.FormSplash.Dispose();
			}
		}

		//--- MENU LABEL FONTS
		//private System.Drawing.Font FontBold = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Bold);
		//private System.Drawing.Font FontBold = new System.Drawing.Font("Calibri", 12.0F, System.Drawing.FontStyle.Bold);
		//private System.Drawing.Font FontRegular = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular);
		private static System.Drawing.Font FontWhenSelected = new System.Drawing.Font("Tahoma", 12.0F, System.Drawing.FontStyle.Bold);
		private static System.Drawing.Font FontWhenNotSelected = new System.Drawing.Font("Tahoma", 11.0F, System.Drawing.FontStyle.Regular);

		private Color TextColorWhenFormWindowIsOpen = Color.MidnightBlue;
		private Color TextColorWhenFormWindowIsNotOpen = Color.Maroon;

		//--- COLORS when in WHITE-MODE
		private Color WhiteModePage = Color.White;
		private Color WhiteModeMenuNormal = Color.White;
		private Color WhiteModeMenuSelected = Color.LightGreen;
		private Color WhiteModeTextNormal = Color.Black;
		private Color WhiteModeTextSelected = Color.Maroon;

		//--- COLORS when in COLOR MODE (variables get assigned values later)
		private Color DefaultColor_Connection;      // Color from the Integrated-Development-Environment as set at Design Time
		private Color ConnectionPage;               // Color of the BACKGROUND of the PAGE (the tab control)
		private Color ConnectionMenuNormal;         // Color of the BACKGROUND of the MENU  -- when it's selected
		private Color ConnectionMenuSelected;       // Color of the BACKGROUND of the MENU  -- when it's NOT selected
		private Color ConnectionTextNormal;         // Color of the TEXT of the MENU        -- when it's selected
		private Color ConnectionTextSelected;       // Color of the TEXT of the MENU        -- when it's NOT selected
		private Color DefaultColor_Settings;
		private Color SettingsPage;
		private Color SettingsMenuNormal;
		private Color SettingsMenuSelected;
		private Color SettingsTextNormal;
		private Color SettingsTextSelected;
		private Color DefaultColor_Diagnostics;
		private Color DiagnosticsPage;
		private Color DiagnosticsMenuNormal;
		private Color DiagnosticsMenuSelected;
		private Color DiagnosticsTextNormal;
		private Color DiagnosticsTextSelected;
		private Color DefaultColor_Performance;
		private Color PerformancePage;
		private Color PerformanceMenuNormal;
		private Color PerformanceMenuSelected;
		private Color PerformanceTextNormal;
		private Color PerformanceTextSelected;
		private Color DefaultColor_Firmware;
		private Color FirmwarePage;
		private Color FirmwareMenuNormal;
		private Color FirmwareMenuSelected;
		private Color FirmwareTextNormal;
		private Color FirmwareTextSelected;
		private Color DefaultColor_AlarmLogs;
		private Color AlarmLogsPage;
		private Color AlarmLogsMenuNormal;
		private Color AlarmLogsMenuSelected;
		private Color AlarmLogsTextNormal;
		private Color AlarmLogsTextSelected;
		private Color DefaultColor_About;
		private Color AboutPage;
		private Color AboutMenuNormal;
		private Color AboutMenuSelected;
		private Color AboutTextNormal;
		private Color AboutTextSelected;


		/**********************************************************************************************************************/
		//=== CHANGE COLOR SCHEME
		//*** CLICK ON TMC LOGO CHANGES COLOR SCHEME
		private void PictureBoxLogo_Click(object sender, EventArgs e) {
			System.Windows.Forms.TabPage CurrentlySelectedMenuPage = PageControl_Main.SelectedTab;
			Program.WhiteMode = !Program.WhiteMode;
			if (Program.WhiteMode)
				SetMenuAndTabColorsToWhiteMode();
			else
				SetMenuAndTabToCOLORS();
			MenuPageChosenSETTINGS();
		}

		/**********************************************************************************************************************/
		//*** WHEN FORM LOADS, REMEMBER COLOR SETTINGS FROM DEVELPOMENT ENVIRONMENT
		private void GetDefaultColorsFromDevelopmentEnvironment() {
			DefaultColor_Connection = ConnectionMenuPanel.BackColor;
			DefaultColor_Settings = SettingsMenuPanel.BackColor;
			DefaultColor_Diagnostics = DiagnosticsMenuPanel.BackColor;
			DefaultColor_Performance = PerformanceMenuPanel.BackColor;
			DefaultColor_Firmware = FirmwareMenuPanel.BackColor;
			//DefaultColor_AlarmLogs = AlarmLogsMenuPanel.BackColor;
			DefaultColor_About = AboutMenuPanel.BackColor;
		}

		/**********************************************************************************************************************/
		//*** RESET COLORS TO THE DEV.ENVIRONMENT COLORS
		private void SetMenuAndTabToCOLORS() {
			ConnectionPage = DefaultColor_Connection;
			ConnectionMenuNormal = DefaultColor_Connection;
			ConnectionMenuSelected = DefaultColor_Connection;
			ConnectionTextNormal = Color.Black;
			ConnectionTextSelected = Color.Maroon;
			SettingsPage = DefaultColor_Settings;
			SettingsMenuNormal = DefaultColor_Settings;
			SettingsMenuSelected = DefaultColor_Settings;
			SettingsTextNormal = Color.Black;
			SettingsTextSelected = Color.Maroon;
			DiagnosticsPage = DefaultColor_Diagnostics;
			DiagnosticsMenuNormal = DefaultColor_Diagnostics;
			DiagnosticsMenuSelected = DefaultColor_Diagnostics;
			DiagnosticsTextNormal = Color.Black;
			DiagnosticsTextSelected = Color.Maroon;
			PerformancePage = DefaultColor_Performance;
			PerformanceMenuNormal = DefaultColor_Performance;
			PerformanceMenuSelected = DefaultColor_Performance;
			PerformanceTextNormal = Color.Black;
			PerformanceTextSelected = Color.Maroon;
			FirmwarePage = DefaultColor_Firmware;
			FirmwareMenuNormal = DefaultColor_Firmware;
			FirmwareMenuSelected = DefaultColor_Firmware;
			FirmwareTextNormal = Color.Black;
			FirmwareTextSelected = Color.Maroon;
			AlarmLogsPage = DefaultColor_AlarmLogs;
			AlarmLogsMenuNormal = DefaultColor_AlarmLogs;
			AlarmLogsMenuSelected = DefaultColor_AlarmLogs;
			AlarmLogsTextNormal = Color.Black;
			AlarmLogsTextSelected = Color.Maroon;
			AboutPage = DefaultColor_About;
			AboutMenuNormal = DefaultColor_About;
			AboutMenuSelected = DefaultColor_About;
			AboutTextNormal = Color.Black;
			AboutTextSelected = Color.Maroon;
		}

		/**********************************************************************************************************************/
		//*** SET COLORS TO THE WHITE/SPARCE COLORS
		private void SetMenuAndTabColorsToWhiteMode() {
			ConnectionPage = WhiteModePage;
			ConnectionMenuNormal = WhiteModeMenuNormal;
			ConnectionMenuSelected = WhiteModeMenuSelected;
			ConnectionTextNormal = WhiteModeTextNormal;
			ConnectionTextSelected = WhiteModeTextSelected;
			SettingsPage = WhiteModePage;
			SettingsMenuNormal = WhiteModeMenuNormal;
			SettingsMenuSelected = WhiteModeMenuSelected;
			SettingsTextNormal = WhiteModeTextNormal;
			SettingsTextSelected = WhiteModeTextSelected;
			DiagnosticsPage = WhiteModePage;
			DiagnosticsMenuNormal = WhiteModeMenuNormal;
			DiagnosticsMenuSelected = WhiteModeMenuSelected;
			DiagnosticsTextNormal = WhiteModeTextNormal;
			DiagnosticsTextSelected = WhiteModeTextSelected;
			PerformancePage = WhiteModePage;
			PerformanceMenuNormal = WhiteModeMenuNormal;
			PerformanceMenuSelected = WhiteModeMenuSelected;
			PerformanceTextNormal = WhiteModeTextNormal;
			PerformanceTextSelected = WhiteModeTextSelected;
			FirmwarePage = WhiteModePage;
			FirmwareMenuNormal = WhiteModeMenuNormal;
			FirmwareMenuSelected = WhiteModeMenuSelected;
			FirmwareTextNormal = WhiteModeTextNormal;
			FirmwareTextSelected = WhiteModeTextSelected;
			AlarmLogsPage = WhiteModePage;
			AlarmLogsMenuNormal = WhiteModeMenuNormal;
			AlarmLogsMenuSelected = WhiteModeMenuSelected;
			AlarmLogsTextNormal = WhiteModeTextNormal;
			AlarmLogsTextSelected = WhiteModeTextSelected;
			AboutPage = WhiteModePage;
			AboutMenuNormal = WhiteModeMenuNormal;
			AboutMenuSelected = WhiteModeMenuSelected;
			AboutTextNormal = WhiteModeTextNormal;
			AboutTextSelected = WhiteModeTextSelected;
		}

		//=== SELECT TAB
		//*** SELECT TAB USING MENU LABEL (Text)
		private void LabelConnection_Click(object sender, EventArgs e) { MenuPageChosenCONNECTION(); }
		private void LabelSettings_Click(object sender, EventArgs e) { MenuPageChosenSETTINGS(); }
		private void LabelDiagnostics_Click(object sender, EventArgs e) { MenuPageChosenDIAGNOSTICS(); }
		private void LabelPerformance_Click(object sender, EventArgs e) { MenuPageChosenPERFORMANCE(); }
		private void LabelFirmware_Click(object sender, EventArgs e) { MenuPageChosenFIRMWARE(); }
		private void LabelAlarmLogs_Click(object sender, EventArgs e) { } //MenuPageChosenALARMLOGS(); }
		private void LabelAbout_Click(object sender, EventArgs e) { MenuPageChosenABOUT(); }

		//*** SELECT TAB USING MENU PANEL (Background)
		private void PanelConnection_Click(object sender, EventArgs e) { MenuPageChosenCONNECTION(); }
		private void PanelSettings_Click(object sender, EventArgs e) { MenuPageChosenSETTINGS(); }
		private void PanelDiagnostics_Click(object sender, EventArgs e) { MenuPageChosenDIAGNOSTICS(); }
		private void PanelPerformance_Click(object sender, EventArgs e) { MenuPageChosenPERFORMANCE(); }
		private void PanelFirmware_Click(object sender, EventArgs e) { MenuPageChosenFIRMWARE(); }
		private void PanelAlarmLogs_Click(object sender, EventArgs e) { }//MenuPageChosenALARMLOGS(); }
		private void PanelAbout_Click(object sender, EventArgs e) { MenuPageChosenABOUT(); }

		//*** SELECT TAB USING MENU ICON (Picture)
		private void PictureBoxConnection_Click(object sender, EventArgs e) { MenuPageChosenCONNECTION(); }
		private void PictureBoxSettings_Click(object sender, EventArgs e) { MenuPageChosenSETTINGS(); }
		private void PictureBoxDiagnostics_Click(object sender, EventArgs e) { MenuPageChosenDIAGNOSTICS(); }
		private void PictureBoxPerformance_Click(object sender, EventArgs e) { MenuPageChosenPERFORMANCE(); }
		private void PictureBoxFirmware_Click(object sender, EventArgs e) { MenuPageChosenFIRMWARE(); }
		private void PictureBoxAlarmLogs_Click(object sender, EventArgs e) { }//MenuPageChosenALARMLOGS(); }
		private void PictureBoxAbout_Click(object sender, EventArgs e) { MenuPageChosenABOUT(); }


		/**********************************************************************************************************************/
		//*** SELECT TAB USING TAB CONTROL
		private void PageControlMain_Selected(object sender, TabControlEventArgs e) {
			// GARY-NOTE: This code should not be needed anymore once we replace the TAB control with the PAGE control (where tabs don't show to user)
			if (PageControl_Main.SelectedIndex == 0) MenuPageChosenCONNECTION();
			if (PageControl_Main.SelectedIndex == 1) MenuPageChosenSETTINGS();
			if (PageControl_Main.SelectedIndex == 2) MenuPageChosenDIAGNOSTICS();
			if (PageControl_Main.SelectedIndex == 3) MenuPageChosenPERFORMANCE();
			if (PageControl_Main.SelectedIndex == 4) MenuPageChosenFIRMWARE();
			//if (PageControl_Main.SelectedIndex == 5) MenuPageChosenALARMLOGS();
			if (PageControl_Main.SelectedIndex == 5) MenuPageChosenABOUT();
		}

		/**********************************************************************************************************************/
		private void Enable_frames() {
			if ((Program.ConnectionType != (int)Program.ConnType.ConnDEMO)) {
				LblTerminalWindow.Enabled = true;
				FrameSystem.Enabled = true;
				ButStartStopLTF.Enabled = true;
				frmDiagnostic.Enabled = true;
				frmFWupgrade.Enabled = true;
			} else {
				LblTerminalWindow.Enabled = false;
				FrameSystem.Enabled = false;
				ButStartStopLTF.Enabled = false;
				frmDiagnostic.Enabled = false;
				frmFWupgrade.Enabled = false;
			}
		}

		/**********************************************************************************************************************/
		//=== ACTIVATE TAB
		//*** DEACTIVATE ALL TAB LABELS
		private void MenuPageChosenNONE() {
			// Change the MENU Labels to Non-Bold
			ConnectionMenuLabel.Font = FontWhenNotSelected;
			SettingsMenuLabel.Font = FontWhenNotSelected;
			DiagnosticsMenuLabel.Font = FontWhenNotSelected;
			PerformanceMenuLabel.Font = FontWhenNotSelected;
			FirmwareMenuLabel.Font = FontWhenNotSelected;
			//AlarmLogsMenuLabel.Font = FontWhenNotSelected;
			AboutMenuLabel.Font = FontWhenNotSelected;

			// Change the MENU Labels FORE-COLOR to
			ConnectionMenuLabel.ForeColor = ConnectionTextNormal;
			SettingsMenuLabel.ForeColor = SettingsTextNormal;
			DiagnosticsMenuLabel.ForeColor = DiagnosticsTextNormal;
			PerformanceMenuLabel.ForeColor = PerformanceTextNormal;
			FirmwareMenuLabel.ForeColor = FirmwareTextNormal;
			//AlarmLogsMenuLabel.ForeColor = AlarmLogsTextNormal;
			AboutMenuLabel.ForeColor = AboutTextNormal;

			// Set the Background-Color of each MENU "panel"
			ConnectionMenuPanel.BackColor = ConnectionMenuNormal;
			SettingsMenuPanel.BackColor = SettingsMenuNormal;
			DiagnosticsMenuPanel.BackColor = DiagnosticsMenuNormal;
			PerformanceMenuPanel.BackColor = PerformanceMenuNormal;
			FirmwareMenuPanel.BackColor = FirmwareMenuNormal;
			//AlarmLogsMenuPanel.BackColor = AlarmLogsMenuNormal;
			AboutMenuPanel.BackColor = AboutMenuNormal;
		}


		/**********************************************************************************************************************/
		//*** ACTIVATE INDIVIDUAL TAB LABELS
		private void MenuPageChosenCONNECTION() {
			Enable_frames();
			MenuPageChosenNONE();                                           // Turn off all menu labels
			ConnectionMenuLabel.Font = FontWhenSelected;                    // Set this label's font to show it's selected
			ConnectionMenuLabel.ForeColor = ConnectionTextSelected;         // Set this label's color to show it's selected
			ConnectionMenuPanel.BackColor = ConnectionMenuSelected;         // Set this label's PANEL background to show it's selected
			TabPage_Connection.BackColor = ConnectionPage;                  // Set the correct color for the PAGE background
			PageControl_Main.SelectedTab = TabPage_Connection;              // Switch to showing that page
			EnableSettingsCommonControls(false);     // Switch to showing that page
			//ConnectionTabWasSelected();                                     // OPTIONAL procedure to call when a PAGE is selected (a kind of "form load" for the page)
		}

		/**********************************************************************************************************************/
		private void MenuPageChosenSETTINGS() {
			Enable_frames();
			MenuPageChosenNONE();
			SettingsMenuLabel.Font = FontWhenSelected;
			SettingsMenuLabel.ForeColor = SettingsTextSelected;
			SettingsMenuPanel.BackColor = SettingsMenuSelected;
			TabPage_Settings.BackColor = SettingsPage;
			PageControl_Main.SelectedTab = TabPage_Settings;
			EnableSettingsCommonControls(true);
			get_ID();
			GetSysData();
		}

		/**********************************************************************************************************************/
		private void MenuPageChosenDIAGNOSTICS() {
			Enable_frames();
			MenuPageChosenNONE();
			DiagnosticsMenuLabel.Font = FontWhenSelected;               // Set this label's font to show it's selected
			DiagnosticsMenuLabel.ForeColor = DiagnosticsTextSelected;   // Set this label's color to show it's selected
			DiagnosticsMenuPanel.BackColor = DiagnosticsMenuSelected;   // Set this label's PANEL background to show it's selected
			TabPage_Diagnostics.BackColor = DiagnosticsPage;            // Set the correct color for the PAGE background
			PageControl_Main.SelectedTab = TabPage_Diagnostics;         // Switch to showing that page
			GetDiagData();
			EnableSettingsCommonControls(true);
		}

		/**********************************************************************************************************************/
		private void MenuPageChosenPERFORMANCE() {
			MenuPageChosenNONE();
			PerformanceMenuLabel.Font = FontWhenSelected;               // Set this label's font to show it's selected
			PerformanceMenuLabel.ForeColor = PerformanceTextSelected;   // Set this label's color to show it's selected
			PerformanceMenuPanel.BackColor = PerformanceMenuSelected;   // Set this label's PANEL background to show it's selected
			TabPagePerformance.BackColor = PerformancePage;             // Set the correct color for the PAGE background
			PageControl_Main.SelectedTab = TabPagePerformance;          // Switch to showing that page
			EnableSettingsCommonControls(false);
			Enable_frames();
			GetSysData();
		}

		/**********************************************************************************************************************/
		private void MenuPageChosenFIRMWARE() {
			MenuPageChosenNONE();
			FirmwareMenuLabel.Font = FontWhenSelected;                  // Set this label's font to show it's selected
			FirmwareMenuLabel.ForeColor = FirmwareTextSelected;         // Set this label's color to show it's selected
			FirmwareMenuPanel.BackColor = FirmwareMenuSelected;         // Set this label's PANEL background to show it's selected
			TabPageFirmware.BackColor = FirmwarePage;                   // Set the correct color for the PAGE background
			PageControl_Main.SelectedTab = TabPageFirmware;             // Switch to showing that page
			EnableSettingsCommonControls(false);
			Enable_frames();
		}

		/********************************************************************************************************************** /
		private void MenuPageChosenALARMLOGS() {
			MenuPageChosenNONE();
			AlarmLogsMenuLabel.Font = FontWhenSelected;                 // Set this label's font to show it's selected
			AlarmLogsMenuLabel.ForeColor = AlarmLogsTextSelected;       // Set this label's color to show it's selected
			AlarmLogsMenuPanel.BackColor = AlarmLogsMenuSelected;       // Set this label's PANEL background to show it's selected
			EnableSettingsCommonControls(false);
		}

		/**********************************************************************************************************************/
		private void MenuPageChosenABOUT() {
			MenuPageChosenNONE();
			AboutMenuLabel.Font = FontWhenSelected;                     // Set this label's font to show it's selected
			AboutMenuLabel.ForeColor = AboutTextSelected;               // Set this label's color to show it's selected
			AboutMenuPanel.BackColor = AboutMenuSelected;               // Set this label's PANEL background to show it's selected
			TabPageAbout.BackColor = AboutPage;                         // Set the correct color for the PAGE background
			PageControl_Main.SelectedTab = TabPageAbout;                // Switch to showing that page
			textBox2.Text = "Version " + Tools.VersionText;
			EnableSettingsCommonControls(false);
			Enable_frames();
		}

		/**********************************************************************************************************************/
		private void EnableSettingsCommonControls(bool Enable) {
			if (Enable) {
				FramePositionFeedBack.Visible = true;
				FrameVelocityFB.Visible = true;
				FramePneumFB.Visible = true;
				FramePositionFeedBack.BringToFront();
				FrameVelocityFB.BringToFront();
				FramePneumFB.BringToFront();
			} else {
				FramePositionFeedBack.Visible = false;
				FrameVelocityFB.Visible = false;
				FramePneumFB.Visible = false;
			}
		}

		/**********************************************************************************************************************/
		private void GetDiagData() {
			double Locale_tmp_number = 0;
			string temp_str = string.Empty;
			string retVal = string.Empty;

			double tryParVal = 0;

			Program.IsReadingControllerSettings = true;

			if (Program.ConnectionType == (int)Program.ConnType.ConnDEMO)
				return;

			SendInternal("freq?", CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			if (temp_str.Length > 6) {
				retVal = temp_str.Substring(6);
				if (double.TryParse(retVal, out tryParVal)) {
					Locale_tmp_number = tryParVal;
					TestFrequency = Locale_tmp_number;
					NInumExcitFreq.Value = TestFrequency;
					NInumExcitFreq0.Value = TestFrequency;
				}
			}

			SendInternal("ampl?", CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			if (temp_str.Length > 6) {
				retVal = temp_str.Substring(6);
				if (double.TryParse(retVal, out tryParVal)) {
					TestGain = tryParVal;
					NInumExcitAmpl.Value = TestGain;
					NInumExcitAmpl0.Value = TestGain;
				}
			}

			SendInternal("excit?", CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			if ((temp_str.ToLower().IndexOf("star") + 1) != 0)
				ExcitationStatus = true;
			else
				ExcitationStatus = false;

			ToggleExcitation.Checked = ExcitationStatus;
			if (Program.FormRTMonitoring != null)
				Program.FormRTMonitoring.ToggleExcitation.Checked = ExcitationStatus;
			Program.IsReadingControllerSettings = false;
		}

		/**********************************************************************************************************************/
		////=== CLASS LEVEL VARIABLES for MONITORING
		////--- INTERNAL BOOLEAN TO TRACK STATES OF THE X, Y, Z, and XYZ-ALL SWITCH STATES

		//--- CONTINUOUS MONITORING SWITCH
		private bool ContinuousMonitoringOn = false;
		private bool SavedMonitoringStatus;        // SAVED


		//--- TOP AREA -- VALVES MILLIAMPERE READINGS
		private int V1milliamps = 0;
		private int V2milliamps = 0;
		private int V3milliamps = 0;
		private int V4milliamps = 0;

		//--- TEXT and COLOR DEFAULTS FOR CONTINUOUS MONITORING
		public const string CoilTextOnButNoValue = "On";
		public const string CoilTextWhenDisabled = "Off";
		private System.Drawing.Color CoilTextColorON = System.Drawing.Color.LawnGreen;
		private System.Drawing.Color CoilTextColorOFF = System.Drawing.Color.DarkGray;

		//--- COLORS OF THE "LED" LIGHTS FOR CONTINUOUS MONITORING
		private enum CoilState { Enabled, Disabled };
		private System.Drawing.Color CoilLEDcolorON = System.Drawing.Color.Lime;
		private System.Drawing.Color CoilLEDcolorOFF = System.Drawing.Color.Gray;
		private System.Drawing.Color CoilLEDcolorDISABLED = System.Drawing.Color.Gray;

		private System.Drawing.Color TextBoxEnabledColor = System.Drawing.Color.White;
		private System.Drawing.Color TextBoxDisabledColor = System.Drawing.Color.Silver;

		/**********************************************************************************************************************/
		public void ContinuousMonitoringOffOn(bool isOn) {
			// If the continuous timer is not running...we can't do Monitoring
			System.Diagnostics.Debug.Assert(timerContinuous.Enabled);

			ContinuousMonitoringOn = isOn;  // Flip the status of the switch

			if (ToggleRTmonitoring.Checked != isOn)
				ToggleRTmonitoring.Checked = isOn;
			ContinuousMonitoringSwitchShow();
			if (isOn)
				_timerforRT.Start();
			else
				_timerforRT.Stop();
		}

		/**********************************************************************************************************************/
		private void ContinuousMonitoringSwitchShow() {
			var /*background color*/ clr = System.Drawing.Color.Black;
			if (ContinuousMonitoringOn == true) {
				this.BeginInvoke((MethodInvoker)delegate {
					ToggleRTmonitoring.BackgroundImage = Resources.SwitchOnYellow;
					clr = TextBoxEnabledColor;
				});
			} else {
				this.BeginInvoke((MethodInvoker)delegate {
					ToggleRTmonitoring.BackgroundImage = Resources.SwitchOffGreen;
					clr = TextBoxDisabledColor;
				});
			}
		}


		//=== TIMER for SIMULATED CONTINUOUS MONITOR DATA
		//--- RANDOM NUMBER GENERATOR -- ONLY INITIALIZED ONCE
		private Random RandomNumberGenerator = new Random();
		private void Timer_MonitorSimulationMode_Tick(object sender, EventArgs e) {
			int xr;                                     // Create xr  (X Range)
			int xpm;                                    // Create xpm (X Plus or Minus)
			xr = Convert.ToInt32(V1milliamps * .1);      // Give it a value that is 10% of the current value of X                    Say XMilliamps is 1243 so xr = 123
			xr = xr * 2;                                // We want a range PLUS or MINUS 10% so we need a value from 0 to 24        Now xr = 24 so we have a range of 246
			xpm = RandomNumberGenerator.Next(1, xr);    // Get a random value in the range                                          Get random number from 1 to 246
			xr = xr / 2;                                // Set the range back to one half it's amount.                              Now xr = 123 again.
			xpm = xpm - xr;                             // Subtract half the range so we get a positive or negative value.
			int ypm = RandomNumberGenerator.Next(1, Convert.ToInt32(V2milliamps * .2)) - Convert.ToInt32(V2milliamps * .1);
			int zpm = RandomNumberGenerator.Next(1, Convert.ToInt32(V3milliamps * .2)) - Convert.ToInt32(V3milliamps * .1);
		}


		/**********************************************************************************************************************/
		//=== Connection
		private string ConvertUnicode2ASCII(string UString) {
			Encoding ascii = Encoding.ASCII;
			Encoding unicode = Encoding.Unicode;

			//the string returned from a PortNames call may be unicode.  Use the
			//following block to convert to ASCII
			byte[] unicodeBytes = unicode.GetBytes(UString);
			byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);
			char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length) + 1];
			ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
			string asciiString = new string(asciiChars);
			asciiString = asciiString.Trim('\0', 'c');

			//this seems to be needed for some Bluetooth adapters?
			return asciiString.Trim('\0', '?');
			//then trim the trailing null and extraneous ? character
		}

		/**********************************************************************************************************************/
		// extract COMxx from the list of com ports
		private string ExtractCOMportNumberfromDescription(string PortDescripton) {
			int pos_START, pos_STOP;
			string PortName;
			pos_START = PortDescripton.IndexOf("(COM") + 1;
			pos_STOP = PortDescripton.IndexOf(")", pos_START); //start search after pos_START
			if (pos_STOP > pos_START)
				PortName = PortDescripton.Substring(pos_START, pos_STOP - pos_START);
			else
				PortName = "NoCOM";
			return PortName;
		}

		/******** IK181007 move to formSplash **************************************************************************************************************/
		// creates list of com ports
		private void MakeCOMportsList() {

			List<string> ports = new List<string>();
			foreach (var port in Program.FormSplash.s_port.Items) {
				ports.Add(port.ToString());
			}
			Program.ControllerPortNumbers = ports.ToArray(); //(ConvertUnicode2ASCII(PortNames[I]));
		}

		/******** IK181007 move to formSplash **************************************************************************************************************/
		private void ConnectionTabWasSelected() {
			MakeCOMportsList();
			CreateListBoxOfControllers();
		}

		private System.Collections.Generic.List<TMCController.DetectedDeviceEntry> DeviceList = null;              // A list of controllers.

		private void buttonRefreshList_Click(object sender, EventArgs e) { ConnectionTabWasSelected(); }


		/**********************************************************************************************************************/
		private void CreateListBoxOfControllers() {
			DeviceList = new System.Collections.Generic.List<TMCController.DetectedDeviceEntry>();              // A list of controllers.
			for (var /*index*/ ix = 0; ix < Program.ControllerPortNumbers.Count(); ix++) {
				//string PortName = ExtractCOMportNumberfromDescription(dsc[ix]);
				{
					Program.ControllerPortSingle = Program.ControllerPortNumbers[ix];
					TMCController.DetectedDeviceEntry FoundPort = new TMCController.DetectedDeviceEntry(Program.ControllerPortNumbers[ix], 15200);
					DeviceList.Add(FoundPort);
				}
			}
		}

		void LoadDemoFile() { // simulated command responses
		}

		/**********************************************************************************************************************/
		public void ConnectToController(bool isConnect = true, bool switchToSettingsPanel = false) {
			var /*enabled state*/ enb = FormEnable(false);
			// cwBut_RT_Monitoring.Value = false;
			try {
				Program.SerialPortDestroy();
				if (isConnect) {
					// Create the base, serial port, from that, a multiplexer and, from that, the
					// multiplexed ports.
					if (Program.ConnectionType != (int)Program.ConnType.ConnDEMO) {

						if (Program.FormSplash.IsPortUSB) {
							Program.ConnectionType = (int)Program.ConnType.ConnCOMport;
							var /*port identity*/ idy = Program.FormSplash.s_port.Text;
							var /*port speed*/ spd = Convert.ToInt32(Program.FormSplash.s_Baud.Text);
							Program.SerialPortCreate(idy, spd);
						} else {
							Program.ConnectionType = (int)Program.ConnType.ConnTelnet;
							Program.TelentPortCreate();
						}
					} else {
						var /*port identity*/ idy = "DEMO";

						var /*port speed*/ spd = 115200;
						Program.SerialPortCreate(idy, spd);
						Program.FormSplash.s_port.Text = "DEMO";
						Program.ConnectionType = (int)Program.ConnType.ConnDEMO;
						Program.FormSplash.IsDemo = false;
					}
					SetupMuxeREventHandler();

					// Create a ANALYZER-Object    // LOCATION-TAG: CONTROLER-OBJECT-CREATED
					Program.ConnectedController = new TMCController(Program.SerialPortMultiplexed4Main);

					//ScopeIntegration_01082018_Tismo
					//if ((/*older controller?*/ Program.ConnectedController.PortName.Equals(Program.ControllerPortSingle))) {
					//    Program.DualPortController = false;
					//} else { Program.DualPortController = true; }

					Program.ConnectedController.WhenCommErrorDontTry = false;

					// Try to open the port.
					if (Program.FormSplash.IsPortUSB)
						if (Program.ConnectedController.IsPortOpen) { Program.ConnectedController.Close(); }
					if (Program.ConnectedController.PortName == "DEMO") {
						Program.NoControllerDemoMode = true;
						Program.ConnectedController.CommStatus = CommStatusStates.DemoMode;
						try {
							LoadDemoFile(); // simulated command responses
							Program.ConnectedController.Open(); //open in DEMO mode
						} catch (System.Exception ex) {
							throw ex;
						} finally { Program.NoControllerDemoMode = false; }
					} else { //not DEMO
						try {
							var /*firmware version*/ ver = string.Empty;
							Program.ConnectedController.Open();   // Gary-Note: The Click in the ANALYZER Happens Here (somewhere in this call)
							ver = Program.ConnectedController.FirmwareVersion.ToLowerInvariant();  //test open by getting version
								if ((!ver.Contains("95-")) || (!ver.Contains("97-"))) { //  part number contains '95-' can be "STACIS" or "STACIS_LP"
//#if !DEBUG
//			throw new SystemException("Wrong firmware version response: \"" + ver + "\"");
//#endif
							} else {
								Program.ConnectedController.CommStatus = CommStatusStates.Connected;
								if (switchToSettingsPanel) {
									Program.IsReadingControllerSettings = true;
									MenuPageChosenSETTINGS();
									Program.IsReadingControllerSettings = false;
									ReadSettingsFromController();
									read_RT_diagnostic();
									//show Setting page
								}
							}
						} catch (SystemException ex) {
							if (ex.ToString().Contains("The port is already open") == false) {
								Program.SerialPortDestroy(/*does not throw exception*/);
								MessageBox.Show("Unable to open port " + "" + " at " + "" + " baud\rProbably "
									+ "" + " is used by other application"
#if DEBUG
			+ "\rException:\r" + ex.ToString()
#endif
		, "Unable to Open Port!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
							} //else port is open, do nothing
						}
					}
				}
			} catch (System.Exception ex) {
				throw ex;
			} finally { FormEnable(/*SHOULD BE enb!*/ true); }
		}


		/**********************************************************************************************************************/
		public void Connect_Or_Disconnect() {
			if (/*(re)prepare?*/ (Program.ConnectedController == null) && Program.ConnectionType != (int)Program.ConnType.ConnDEMO) {
				ConnectToController(/*connect*/ true, /*switch to settings*/ false);
			} else if (/*closed?*/ Program.ConnectedController == null && Program.ConnectionType == (int)Program.ConnType.ConnDEMO) {
				ConnectToController(/*connect*/ true, /*switch to settings*/ false);
			} else if (/*closed?*/ !Program.ConnectedController.IsPortOpen && Program.ConnectionType != (int)Program.ConnType.ConnDEMO) {
				ConnectToController(/*connect*/ true, false); // switch to control page
			} else { //open: disconnect
				ConnectToController(/*disconnect*/ false);
			}
		}

		/**********************************************************************************************************************/
		private void buttonConnectOnClick(object sender, EventArgs e) {
			Connect_Or_Disconnect();
		}

		/**********************************************************************************************************************/
		public void ConnectionStatusSet(string labelText, bool IsConnected) {
			if (/*OK (maybe)?*/ labelText != null) {
				labelText = labelText.Trim();
				if (/*OK?*/ !string.IsNullOrWhiteSpace(labelText)) {
					this.BeginInvoke((MethodInvoker)delegate { LblCONNparams.Text = labelText; });
				}
			}
			if (IsConnected)
				this.BeginInvoke((MethodInvoker)delegate { ImageConnected.Visible = true; ImageDisconnected.Visible = false; });
			else
				this.BeginInvoke((MethodInvoker)delegate { ImageDisconnected.Visible = true; ImageConnected.Visible = false; });
		}


		/**********************************************************************************************************************/
		//=== SETTINGS (SYSTEM CONTROL) TAB
		//=== READ (REFRESH) ALL PAGES FROM CONTROLLER
		//        private void Button_RefreshSettingsScreen_Click(object sender, EventArgs e) { ReadSettingsFromController(); }
		private void ReadSettingsFromController() {
			if (!Program.IsReadingControllerSettings) {
				Program.IsReadingControllerSettings = true;
				var enabled_state = FormEnable(false);
				try {
					ReadChannelNamesFromController();
					RefreshSettingsPage();
					//Read_Alarm_Status();
				} catch (System.Exception ex) {
					throw ex;
				} finally {
					FormEnable( true);//SHOULD BE enabled_state!
					Program.IsReadingControllerSettings = false;
				}
			}
		}

		/**********************************************************************************************************************/
		private void TurnOffOtherMessaging() {
			// Turn off some events while initializing system data
			Debug.Assert(Program.IsReadingControllerSettings);

			// Turn off Continuous-Monitoring so it's commands will not interfere with commands sent to get the controller settings.
			SavedMonitoringStatus = ContinuousMonitoringOn;             // Remember whether Continuous-Monitoring was On or Off
			SetContinuousMonitoring(false);         // Turn off Continuous-Monitoring

			// Turn on ECHO Mode -- so if we are watching on terminal it will make more sense.
			Program.ConnectedController.EchoState = EchoStates.Enabled;
		}

		/**********************************************************************************************************************/
		private void stateButtonPOSmonitoring_Click(object sender, EventArgs e) { }

		private void stateButtonRTMonitoring_CheckedChanged(object sender, EventArgs e) { RTClick(); }

		private void RTClick() { }

		/**********************************************************************************************************************/
		private void ButtonUpdateFirmware_Click(object sender, EventArgs e) {
			var txt = " 0. You should 'Retrieve and Save' Parameters before upgrading, -\r\n";
			txt += "     if it was not done yet - hit 'OK' and do 'Retrieve Parameters and Save to File'\r\n";
			txt += " 1. Because you clicked, COM port became 'DISconnected' to prepare for upgrade\r\n";
			txt += " 2. Start 'ARMWSD.exe' updater program, pick up new firmware image - '*.hex' file\r\n";
			txt += " 3. Make sure ARMWSD has right settings: COM Port number & 115200,n,8,1\r\n";
			txt += " 4. TURN OFF POWER SWITCH on controller\r\n";
			txt += " 5. Press and HOLD on controller the 'Return' menu key\r\n";
			txt += " 6. While still HOLDING DOWN the 'Return' key, disconnect\r\n";
			txt += "     and re-connect USB cable\r\n";
			txt += " 7. RELEASE the 'Return' menu key\r\n";
			txt += " 8. On ARMWSD screen, press the 'Start' button\r\n";
			txt += " 9. Wait while ARMWSD informs you: 'erasing'; 'downloading'; 'verifying'\r\n";
			txt += "10. When ARMWSD is done close it, disconnect USB cable, turn ON power switch,\r\n";
			txt += "     connect USB cable\r\n";
			txt += "11. Wait for controller performing initial tests, watch progress on LCD\r\n";
			txt += "12. As soon as you see 'Filters settling 9 sec' and counting down,\r\n";
			txt += "     press any LCD menu key\r\n";
			txt += "13. After pressing 'OK' on this dialog, then press 'Read and Restore Parameters'\r\n";
			txt += "14. Parameters will be restored in RAM, do not forget to save them to FLASH\r\n";
			txt += "15. Now press 'OK' to dismiss this screen";
			SavedMonitoringStatus = ContinuousMonitoringOn;
			SetContinuousMonitoring(false);

			//ScopeIntegration_01082018_Tismo
			// Stop scope
			//if (/*form already active?*/ Program.FormScope != null) {
			//    if (Program.FormScope.ScopeIsRunning)   //Scope integration_01/08/2018
			//        this.Invoke((System.Action)(() => {
			//            Program.FormScope.ScopeStartStop();  //Scope integration_01/08/2018
			//        }));
			//    Thread.Sleep(100);
			//}

			ConnectToController(/*disconnect*/ false);
			MessageBox.Show(txt, "How To Update Firmware - READ ALL before dismissing!", MessageBoxButtons.OK);
			ConnectToController(/*connect*/ true, /*switch to settings*/ false);
			ContinuousMonitoringOn = SavedMonitoringStatus;
		}


		private System.Drawing.Color busyWarningBackColorBase = System.Drawing.Color.Salmon;
		private System.Drawing.Color busyWarningForeColorBase = System.Drawing.Color.DarkRed;

		private int BusyWarningFlash(int timerTick) {   //called by only TimerContinuousOnTick
			timerTick %= 10;
			if (/*is supposed to be visible?*/ busyWarningIsVisibleBase) {
				this.Invoke((System.Action)(() =>
				{
					BusyWarningIsVisible = true;
					labelControllerIsBusy.BackColor = (timerTick >= 5) ? busyWarningForeColorBase : busyWarningBackColorBase;
					labelControllerIsBusy.ForeColor = (timerTick >= 5) ? busyWarningBackColorBase : busyWarningForeColorBase;
					// this is caption of formMain                    this.Text = "Analyzer" + timerTick.ToString();
					//labelControllerIsBusy.Refresh();
					//                    Update();
				}));
			}
			return timerTick;
		}

		/**********************************************************************************************************************/
		private bool busyWarningFlashToggle = false;
		public void BusyWarningFlash() {   //called from outside
			if (/*is supposed to be visible?*/ busyWarningIsVisibleBase) {
				this.Invoke((System.Action)(() =>
				{
					BusyWarningIsVisible = true;
					busyWarningFlashToggle = !busyWarningFlashToggle;
					if (busyWarningFlashToggle) {
						//labelControllerIsBusy.BackColor = busyWarningBackColorBase;
						//labelControllerIsBusy.ForeColor = busyWarningForeColorBase;
					} else {
						//labelControllerIsBusy.BackColor = busyWarningForeColorBase;
						//labelControllerIsBusy.ForeColor = busyWarningBackColorBase ;
					}
					//labelControllerIsBusy.Refresh();
				}));
			}
		}

		/**********************************************************************************************************************/
		public bool BusyWarningIsVisible
		{
			get { return busyWarningIsVisibleBase; }
			set {
				busyWarningIsVisibleBase = value;
				//this.Invoke((System.Action)(() => { labelControllerIsBusy.Visible = busyWarningIsVisibleBase; }));
			}
		}
		private bool busyWarningIsVisibleBase = false;

		/**********************************************************************************************************************/
		 // IK20211228 keep just in case if warning is needed
		private void BusyWarningPrepare() { //called once by only FormMainOnLoad
			this.Invoke((System.Action)(() =>
			{
				busyWarningBackColorBase = labelControllerIsBusy.BackColor;
				busyWarningForeColorBase = labelControllerIsBusy.ForeColor;
				busyWarningIsVisibleBase = labelControllerIsBusy.Visible;
			}));
		}

		/**********************************************************************************************************************/
		public void BusyWarningSet(string newText, System.Drawing.Color? backHue = null, System.Drawing.Color? foreHue = null) {
			newText = Tools.TextClean(newText);
			if (!string.IsNullOrEmpty(newText)) {
				this.Invoke((System.Action)(() =>
				{ //apply the change only if different to reduce possible flickering
				  //if (!labelControllerIsBusy.Text.Equals(newText)) { labelControllerIsBusy.Text = newText; }
				}));
			}
			if (/*color change?*/ backHue != null) {
				busyWarningBackColorBase = (System.Drawing.Color)backHue;
				this.Invoke((System.Action)(() =>
				{ //apply the change only if different to reduce possible flickering
				  //if (labelControllerIsBusy.BackColor != busyWarningBackColorBase) { labelControllerIsBusy.BackColor = busyWarningBackColorBase; }
				}));
			}
			if (/*color change?*/ foreHue != null) {
				busyWarningForeColorBase = (System.Drawing.Color)foreHue;
				this.Invoke((System.Action)(() =>
				{ //apply the change only if different to reduce possible flickering
				  //if (labelControllerIsBusy.ForeColor != busyWarningForeColorBase) { labelControllerIsBusy.ForeColor = busyWarningForeColorBase; }
				}));
			}
			BusyWarningIsVisible = true;    //if set, show
		}

		/**********************************************************************************************************************/
		public void ConnectionStatus(string newText, System.Drawing.Color? backHue = null, System.Drawing.Color? foreHue = null) {
			try {
				if (Program.formTerminal != null)
					Program.formTerminal.UpdateStatus();
				newText = Tools.TextClean(newText);
				if (!string.IsNullOrEmpty(newText)) {
					this.BeginInvoke((MethodInvoker)delegate { //apply the change only if different to reduce possible flickering
						if (!labelConnectionState.Text.Equals(newText)) { labelConnectionState.Text = newText; }
					});
				}
				if (/*color change?*/ backHue != null) {
					connectionStatusBackColorBase = (System.Drawing.Color)backHue;
					this.BeginInvoke((MethodInvoker)delegate { //apply the change only if different to reduce possible flickering
															   //if (labelConnectionState.BackColor != connectionStatusBackColorBase) { labelConnectionState.BackColor = connectionStatusBackColorBase; }
					});
				}
				if (/*color change?*/ foreHue != null) {
					connectionStatusForeColorBase = (System.Drawing.Color)foreHue;
					this.BeginInvoke((MethodInvoker)delegate { //apply the change only if different to reduce possible flickering
						if (labelConnectionState.ForeColor != connectionStatusForeColorBase) { labelConnectionState.ForeColor = connectionStatusForeColorBase; }
					});
				}
				this.BeginInvoke((MethodInvoker)delegate { labelConnectionState.Visible = true; });
				UpdateRefresh();
			} catch { /*ignore*/ }
		}

		/**********************************************************************************************************************/
		private System.Drawing.Color connectionStatusBackColorBase = System.Drawing.Color.Salmon;
		private System.Drawing.Color connectionStatusForeColorBase = System.Drawing.Color.DarkRed;

		private void ConnectionStatusPrepare() {
			//this.BeginInvoke((MethodInvoker)delegate {
			connectionStatusBackColorBase = labelConnectionState.BackColor;
			connectionStatusForeColorBase = labelConnectionState.ForeColor;
			labelConnectionState.Visible = true;
			//});
		}

		/**********************************************************************************************************************/
		private bool /*previous state*/ FormEnable(bool isEnabled) {
			var /*result (returned)*/ rsl = false;
			//this.Invoke((System.Action)(() =>
			//{
			//    rsl = Enabled;
			//    Panel_Menu.Enabled = GroupBoxCoilOutputs.Enabled = cwBut_RT_Monitoring.Enabled = isEnabled;
			//    //Enabled = isEnabled;
			//}));
			//BusyWarningIsVisible = !isEnabled;  //enabled==not busy & vise versa
			return rsl;
		}

		/**********************************************************************************************************************/
		//*** CONTINUOUS MONITORIING on SYSTEM TAB

		/**********************************************************************************************************************/
		private void LabelControllerTemperatureOnClick(object sender, EventArgs e) {
			// When click on the temperature, change it's units from Celcius to Fahrenheit
			ControllerTemperatureReportInFahrenheit = !ControllerTemperatureReportInFahrenheit;
		}
		private const char DegreeSymbol = '\u00B0'; // ° (ALT 0176)
		private void DisplayControllerTemperature() { // ELDAMP Analyzer does not need to show temperature, STACIS - needs
			// Convert to Fahrenheit if Necessary
			double DisplayTemp = ControllerTemperature;
			string Scale = "C";
			if (ControllerTemperatureReportInFahrenheit) { DisplayTemp = (DisplayTemp * 1.8) + 32; Scale = "F"; }

			// Display the current temperature
			labelControllerTemperature.Text = "Contr. Temp " + DisplayTemp.ToString("#00.0") + DegreeSymbol + Scale;
			labelControllerTemperature.ImageAlign=System.Drawing.ContentAlignment.TopRight;
			labelControllerTemperature.Visible = true;

			// Set Font & Color
			if (ControllerTemperature < CautionTemperature) {
				labelControllerTemperature.ForeColor = System.Drawing.Color.Gainsboro;
				labelControllerTemperature.Font = new System.Drawing.Font("Tahoma", 10.0F, System.Drawing.FontStyle.Regular);
				labelControllerTemperature.Visible = true;
			} else {
				labelControllerTemperature.ForeColor = System.Drawing.Color.Red;
				labelControllerTemperature.Font = new System.Drawing.Font("Tahoma", 12.0F, System.Drawing.FontStyle.Bold);
			}

			// If Temperature is CRITICAL...flash the temperature ON/OFF each time this function is called ---
			if (ControllerTemperature >= CriticalTemperature) {
				ControllerTemperatureFlashLabel = !ControllerTemperatureFlashLabel;
				//labelControllerTemperature.Visible = ControllerTemperatureFlashLabel;
			}
		}

		/**********************************************************************************************************************/
		private void labelControllerTemperatureOnClick(object sender, EventArgs e) {
			// When click on the temperature, change it's units from Celcius to Fahrenheit
			ControllerTemperatureReportInFahrenheit = !ControllerTemperatureReportInFahrenheit;
		}

		/********************************************************************************************************************** /
		private bool Ask_WantToOverwriteSettings() {
			DialogResult ButtonPressed = MessageBox.Show("Overwrite current settings?", "Flash Memory", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (ButtonPressed == DialogResult.No || ButtonPressed == DialogResult.Cancel)
				return false;
			else
				return true;
		}

		/**********************************************************************************************************************/
		// set the time from this PC to the hardware
		private void Button_SetControllerTime_Click(object sender, EventArgs e) {
			Program.ConnectedController.Time = DateTime.Now;
		}

		/**********************************************************************************************************************/
		private String current_Path = String.Empty;
		//private String current_directory = String.Empty;
		//private String current_FileName = String.Empty;
		/**********************************************************************************************************************/
		private void buttonReadScan_CheckedChanged(object sender, EventArgs e) {
			if (buttonReadScan.Checked) {
				if (ButtonAutoScale.Text.Equals("Auto"))
					ScatterGraphMag.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				//openMeasurementFileDialog.FileName = string.Empty;
				if (current_Path == "")
					openMeasurementFileDialog.InitialDirectory = Program.TMC_PathData;
				else {
					string current_FileName = "";
					if (current_Path.Contains("\\") || current_Path.Contains("/")) {// like: "C:\In_Out\Software\Mag-NetX_Analyzer_VB6\t\2014-08-29 -- Test 1X.csv"
																					// in C# it is double slash "C:\\In_Out\\Software\\Mag-NetX_Analyzer_VB6\\t\\2014-08-29 -- Test 1X.csv""
						var last_slash_pos = current_Path.LastIndexOf("\\");
						if (last_slash_pos == 0)
							last_slash_pos = current_Path.LastIndexOf("/"); // maybe in can be '/'?
						if (current_Path.Length > last_slash_pos + 2)
							current_FileName = current_Path.Substring(last_slash_pos + 1);
						var current_directory = current_Path.Substring(0, last_slash_pos);
						openMeasurementFileDialog.InitialDirectory = current_directory;
						openMeasurementFileDialog.FileName = current_FileName;
					}
				}
				if ( openMeasurementFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {//rejected?
					buttonReadScan.Checked = false;
				} else {
					current_Path = openMeasurementFileDialog.FileName;
					txtStatus.Text = "Reading measurement from file '" + openMeasurementFileDialog.FileName + " .....";
					var stm = new System.IO.StreamReader(openMeasurementFileDialog.FileName);// open stream
					try {
						var txt = stm.ReadLine().ToLower(System.Globalization.CultureInfo.InvariantCulture); // 1st line
						if (/*bad?*/ !txt.Contains("file name,")) { throw new System.InvalidOperationException("Data line[1] is missing file name:\r\"" + txt + "\""); }
						txt = /*2nd line*/ stm.ReadLine().ToLower(System.Globalization.CultureInfo.InvariantCulture);
						if (/*bad?*/ !txt.Contains("date,")) { throw new System.InvalidOperationException("Data line[2] is missing date:\r\"" + txt + "\""); }
						txt = /*3rd line*/ stm.ReadLine().ToLower(System.Globalization.CultureInfo.InvariantCulture);
						if (/*bad?*/ !txt.Contains("time,")) { throw new System.InvalidOperationException("Data line[3] is missing time:\r\"" + txt + "\""); }
						txt = /*4th line (ignored)*/ stm.ReadLine();

						// Read header (5th) line: "Frequency HZ,x-suppression dB,x-dB_Open,x-deg_Open,x-db_Clos,x-deg_Clos,x-deg_suppr"

						bool gotFreqHeader = false;
						for (int nGetStartLine = 1; nGetStartLine < 5; nGetStartLine++) {
							txt = /*5th line: header*/ stm.ReadLine();

							if (/*bad?*/ txt.Contains("Frequency")) {
								gotFreqHeader = true;
								nGetStartLine = 6;
							}
						}
						if (/*bad?*/ !gotFreqHeader) { throw new System.InvalidOperationException("Data line[5] must be a header:\r\"" + txt + "\""); }

						Update_Previous_Plot_Data();        // save current data to previous
						Clear_Performance_Data_Arrays();    // clean arrays

						// Data lines follow the format:
						// "{0:#0.00}, {1:#0.000}, {2:#0.000}, {3:#0.00}, {4:#0.000}, {5:#0.00}, {6:#0.00}, #"
						for (var /*count*/ cnt = /*because 5 lines above*/ 3; /*more data?*/ stm.Peek() > 0; cnt++) {
							txt = /*next line*/ stm.ReadLine();
							txt = txt.TrimEnd('\r', '\n');
							var nts = txt.Split(','); // entries
							if ((nts.Count() != 4 && nts.Count() != 5) || // without or with Coherence
								(  !nts[nts.Count() - 1].Contains("#"))){ // no "#" discovered, bad?
								throw new System.InvalidOperationException("Data line[" + cnt.ToString() + "] is corrupt:\r\"" + txt + "\"");
							}
							LTFfrequency[LTFlineCounter] = double.Parse(nts[0]);
							LTFdBgain[LTFlineCounter] = double.Parse(nts[1]);
							LTFphaseDeg[LTFlineCounter] = double.Parse(nts[2]);
							if (nts.Count() == 5)
								LTFcoherence[LTFlineCounter] = double.Parse(nts[3])*100;
							else
								LTFcoherence[LTFlineCounter] = 0;

							LTFnegPhase[LTFlineCounter] = Make180phaseShift(double.Parse(nts[2]));
							LTFlineCounter += 1;
						}
					} catch (Exception ex) {
						MessageBox.Show("The file is NOT a valid scan file:" + "\r" + Tools.TextTidy(ex.ToString()), "Scan File Anomaly", MessageBoxButtons.OK, MessageBoxIcon.Error);
					} finally { stm.Close(); }
					Update_Performance_Plots(); // ACTUAL PLOT
												//                    FindUnityGainPhase(Array_Line_Counter);
					buttonReadScan.Checked = false;
					txtStatus.Text = "Measurement file '" + openMeasurementFileDialog.FileName + "' read successfully";
				}
			}
		}

		/**********************************************************************************************************************/
		private void FindUnityGainPhase(int inCounter) {
			long IndexI;
			long MinIndex, MaxIndex;
			string MinString, MaxString;
			bool MinUnityPhasePresent;
			MinIndex = LTFfrequency.Count() - 1;
			MaxIndex = 0;
			MinString = string.Empty;
			MaxString = string.Empty;
			for (IndexI = inCounter - 1; IndexI >= 0; IndexI--) {
				if (Math.Abs(LTFdBgain[IndexI]) < 0.9 && MinIndex == (LTFfrequency.Count() - 1)) {
					MinString = string.Format(", {0:#0.0Hz})", LTFfrequency[IndexI]);
					MinIndex = IndexI;
					IndexI += 3;        // prevent catch "close next" point
				}
			}
			for (IndexI = MinIndex - 1; IndexI >= 0; IndexI--) {
				if (Math.Abs(LTFdBgain[IndexI]) < 0.2 && MaxIndex == 0) {
					MaxString = string.Format(", {0:#0.0Hz})", LTFfrequency[IndexI]);
					MaxIndex = IndexI;
				}
			}
			MinString = string.Format("Unity Gain ({0:##0.0}dB, {1:##0.0} degree", LTFdBgain[MinIndex], LTFphaseDeg[MinIndex]) + MinString;
			MaxString = string.Format("Unity Gain ({0:##0.0}dB, {1:##0.0} degree", LTFdBgain[MaxIndex], LTFphaseDeg[MaxIndex]) + MaxString;
			ScatterGraphMag.Caption = MinString + "   <= =>   " + MaxString;
			MinString = string.Empty;
			MaxString = string.Empty;

			// calculate Unity Phase
			MinIndex = LTFfrequency.Count() - 1;
			MaxIndex = 0;
			MinUnityPhasePresent = true;
			for (IndexI = inCounter - 1; IndexI >= 0; IndexI--) {
				if (Math.Abs(LTFphaseDeg[IndexI]) < 3 && MinIndex == (LTFfrequency.Count() - 1)) {
					if (IndexI > 180) {
						MinString = string.Format(", {0:#0.0Hz})", LTFfrequency[IndexI]);
						MinUnityPhasePresent = true;
						MinIndex = IndexI;
						IndexI += 3;        // prevent catch "close next" point
					} else {
						MinUnityPhasePresent = false;
						MaxString = string.Format(", {0:#0.0Hz})", LTFfrequency[IndexI]);
						MaxIndex = IndexI;
					}
				}
			}
			if (MinUnityPhasePresent) {
				for (IndexI = MinIndex - 1; IndexI >= 0; IndexI--) {
					if (Math.Abs(LTFdBgain[IndexI]) < 3 && MaxIndex == 0) {
						MaxString = string.Format(", {0:#0.0Hz})", LTFfrequency[IndexI]);
						MaxIndex = IndexI;
					}
				}
			}
			MinString = string.Format("Unity Phase ({0:##0.0}dB, {1:##0.0} degree", LTFdBgain[MinIndex], LTFphaseDeg[MinIndex]) + MinString;
			MaxString = string.Format("Unity Phase ({0:##0.0}dB, {1:##0.0} degree", LTFdBgain[MaxIndex], LTFphaseDeg[MaxIndex]) + MaxString;
			ScatterGraphPhase.Caption = MinString + "   <= =>   " + MaxString;
		}

		/**********************************************************************************************************************/
		private void ButtonAutoScale_CheckedChanged(object sender, EventArgs e) {
			// Take care of auto or fixed scaling on graph
			if (ButtonAutoScale.Checked) { //AUTO
				double maxF = LTFfrequency.Max();
				if ((maxF > 10) && (maxF < 10000))
					PlotMaxFreq = maxF;
				ScatterGraphMag.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				//-!-ScatterGraphMag.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose; //-!- this sets scale 0.1 to PlotMaxFreq
				ScatterGraphMag.XAxes[0].Range = new NationalInstruments.UI.Range(0.1, PlotMaxFreq);
				ScatterGraphMag.InteractionModeDefault = NationalInstruments.UI.GraphDefaultInteractionMode.None; // in this mode user cannot move cursor

				ScatterGraphPhase.XAxes[0].Range = new NationalInstruments.UI.Range(0.1, PlotMaxFreq);
			} else { //MANUAL (FIXED)
				ScatterGraphMag.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
				ScatterGraphMag.YAxes[0].Range = new NationalInstruments.UI.Range(-40, 40);
				ScatterGraphMag.XAxes[0].Range = new NationalInstruments.UI.Range(0.1, PlotMaxFreq);
				ScatterGraphMag.InteractionModeDefault = NationalInstruments.UI.GraphDefaultInteractionMode.None; // cursor can be moved

				ScatterGraphPhase.XAxes[0].Range = new NationalInstruments.UI.Range(0.1, PlotMaxFreq);
			}
			UpdateRefresh();
		}

		/**********************************************************************************************************************/
		private void buttonSaveScan_CheckedChanged(object sender, EventArgs e) {
			if (buttonSaveScan.Checked) {
				DateTime rightNow = DateTime.Now;
				string JustFilename;
				int Aindex;
				if (String.IsNullOrEmpty(Program.LTFtestOutputName) || String.IsNullOrEmpty(Program.LTFtestInputName)) {
					if (!buttonSaveScan.Checked)
						return;
					else {
						MessageBox.Show("Please choose Output and Input first", "Parameters are not defined", MessageBoxButtons.OK);
						buttonSaveScan.Checked = false;
						return;
					}
				}
				DateTime dateTimeNow = DateTime.Now;
				String DefaultFileName = LTF_command + dateTimeNow.ToString(" yyyy-MM-dd HH-mm");
				DefaultFileName += ", gain=1.00"; //-!- IK20211228 need real gain of axis
				if (ChkSavePredictionPlot.Checked) {
					DefaultFileName += ", prediction";
				} else {
					DefaultFileName += ", pwr=100"; //-!- IK200302 need real power (loop open, reduced gain, full gain)
				}
				// set filters - this can be done in properties as well
				//                if (FileSavePath == "")
				//                    SetDefaultFileSaveLocationAndName();

				saveScanFileDialog.FileName = DefaultFileName;
				if (current_Path == "")
					saveScanFileDialog.InitialDirectory = Program.TMC_PathData;
				DialogResult Dresult = saveScanFileDialog.ShowDialog();       // Show the dialog to the user
				if (Dresult == System.Windows.Forms.DialogResult.Cancel) {
					buttonSaveScan.Checked = false;
					return;     //cancelled
				}
				Aindex = saveScanFileDialog.FileName.LastIndexOf("\\");
				JustFilename = saveScanFileDialog.FileName.Substring(Aindex + 1);
				//textBoxFileName.Text = saveScanFileDialog.FileName;     // put out file path/name to bottom of form (could be changed to JustFileName)
				TextWriter writer = new StreamWriter(saveScanFileDialog.FileName, true, System.Text.Encoding.ASCII, 1048576);
				txtStatus.Text = "Saving measurement into file '" + saveScanFileDialog.FileName + " .....";
				// Put out header lines
				writer.WriteLine(String.Format("File name,{0}", JustFilename));//IK200225 "File Name" (capital 'N') is not recognized by VB6
				writer.WriteLine(String.Format("Date,{0:dd-MMM-yyyy}", dateTimeNow));
				writer.WriteLine(String.Format("Time,{0:HH:mm:ss}", dateTimeNow));
				writer.WriteLine(string.Empty);

				// Now the header for the data
				if (ChkSavePredictionPlot.Checked) {
					writer.WriteLine(string.Format("Frequency Hz,Predicted dB,Predicted Ph Deg"));
				} else {
					writer.WriteLine(string.Format("Frequency Hz,Gain dB,Phase Deg,Coherence"));
				}
				for (int i = 0; i < LTFarrayCount; i++) {
					if (ChkSavePredictionPlot.Checked)
					{
						if (frmFilters.Freq_points[i] > 0)
							writer.WriteLine(string.Format("{0:#0.00}, {1:#0.000}, {2:#0.000}, #", frmFilters.Freq_points[i], frmFilters.Predicted_Mag[i], frmFilters.Predicted_Phase[i]));
						else
							break;   // end loop, the rest of the data is empty
					} else {
						if (LTFfrequency[i] > 0)
							writer.WriteLine(string.Format("{0:#0.00}, {1:#0.000}, {2:#0.000}, {3:#0.0000},#", LTFfrequency[i], LTFdBgain[i], LTFphaseDeg[i], LTFcoherence[i] * 0.01));
						else
							break;   // end loop, the rest of the data is empty
					}
				}
				writer.Flush();
				writer.Close();
				txtStatus.Text = "File saved: " + saveScanFileDialog.FileName;
				//textBoxFileName.Text = "Measurement file " + fNm + " saved.";
				buttonSaveScan.Checked = false;
			}
		}

		/**********************************************************************************************************************/
		private void scatterGraph_Click(object sender, EventArgs e) {
			ScatterGraphMag.Cursors[0].Visible = true;
		}
		private void scatterGraph_DoubleClick(object sender, EventArgs e) {
			ScatterGraphMag.Cursors[0].Visible = false;
		}
		private void scatterGraphPhase_Click(object sender, EventArgs e) {
			ScatterGraphPhase.Cursors[0].Visible = true;
		}
		private void scatterGraphPhase_DoubleClick(object sender, EventArgs e) {
			ScatterGraphPhase.Cursors[0].Visible = false;
		}

		/**        private void scatterGraph1_CursorChanged(object sender, EventArgs e) {
if (!LTFIsRunning) { scatterGraphMag.Caption = "Frequency=" + scatterGraphMag.Cursors[0].XPosition.ToString("#00.0") + ", Magnitude=" + scatterGraphMag.Cursors[0].YPosition.ToString("0#.0"); }
}
private void scatterGraphPhase_CursorChanged(object sender, EventArgs e) {
if (!LTFIsRunning) { scatterGraphPhase.Caption = "Frequency=" + scatterGraphPhase.Cursors[0].XPosition.ToString("#00.0") + ", Phase=" + scatterGraphPhase.Cursors[0].YPosition.ToString("0##"); }
}

private void scatterGraphMag_MouseDown(object sender, MouseEventArgs e) {
// XPosition YPosition are updated after move !
if (!LTFIsRunning) { scatterGraphMag.Caption = "Frequency=" + scatterGraphMag.Cursors[0].XPosition.ToString("#00.0") + ", Magnitude=" + scatterGraphMag.Cursors[0].YPosition.ToString("0#.0"); }
}

private void scatterGraphMag_BeforeMoveCursor(object sender, NationalInstruments.UI.BeforeMoveXYCursorEventArgs e) {
Xpos = e.Cursor.XPosition;
Ypos = e.Cursor.YPosition;
if (!LTFIsRunning) { scatterGraphMag.Caption = "Frequency=" + Xpos.ToString("#00.0") + ", Magnitude=" + Ypos.ToString("0#.0"); }
}
		**/

		/**********************************************************************************************************************/
		private void scatterGraphMag_AfterMoveCursor(object sender, NationalInstruments.UI.AfterMoveXYCursorEventArgs e) {
			var Xpos = e.Cursor.XPosition;
			var Ypos = e.Cursor.YPosition;
			var Ytimes = Math.Pow(10.0, (Ypos / 20));
			string Mag_times = Ytimes.ToString("G4");
			string Reduction = (1.0 / Ytimes).ToString("G4");
			if (!LTFIsRunning) {
				var capt = "Frequency " + Xpos.ToString("#00.0 Hz") + ", Magnitude " + Ypos.ToString("0#.0 dB");
				var perf = "";
				if (Ypos < 0)
					perf = " ( reduction " + Reduction + " times)";
				else
					perf = " (increase " + Mag_times + " times)";
				ScatterGraphMag.Caption = capt + perf;
			}
		}

		/**********************************************************************************************************************/
		private void scatterGraphPhase_AfterMoveCursor(object sender, NationalInstruments.UI.AfterMoveXYCursorEventArgs e) {
			var Xpos = e.Cursor.XPosition;
			var Ypos = e.Cursor.YPosition;
			if (!LTFIsRunning) { ScatterGraphPhase.Caption = "Frequency " + Xpos.ToString("#00.0 Hz") + ", Phase " + Ypos.ToString("0##.0"); }
		}

		/**********************************************************************************************************************/
		//=== SWITCHES FOR AXES & MONITORING
		//*** USER CHANGED STATE OF THE CONTINUOUS-MONITORING SWITCH
		//*** PROGRAM CHANGED STATE OF CONTINUOUS-MONITORING MODE
		private void SetContinuousMonitoring(bool Mode) {
			ContinuousMonitoringOn = Mode;
			Thread.Sleep(200);          // wait for real time monitoring to finish displaying
		}

		#region ED_Varuables
		public static double[] SystemPressures = new double[5]; // Index0= input, 1-2-3-4 Isolators
		#endregion
		/**********************************************************************************************************************/
		//=== PERFORMANCE TAB
		// Performance tab arrays
		#region LTF_Variables
		private const int LTFarrayMinimum = 120;   // common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		private const int LTFarrayPad = 75;        // common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		public const int LTFarrayCount = LTFarrayMinimum + LTFarrayPad;     // common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers

		public int LTFlineCounter = 0;	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		public double[] LTFfrequency = new double[LTFarrayCount];	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		public double[] LTFdBgain = new double[LTFarrayCount]; 	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		public double[] LTFphaseDeg = new double[LTFarrayCount];	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		public double[] LTFcoherence = new double[LTFarrayCount];	// common name between STACIs, ElDamp Analyzers
		private double[] LTFnegPhase = new double[LTFarrayCount];	// common name between STACIs, ElDamp, PEPS Analyzers

		private double[] LTFprevFreq = new double[LTFarrayCount];	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		private double[] LTFprevGain = new double[LTFarrayCount];	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		private double[] LTFprevPhase = new double[LTFarrayCount];	// common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers
		private double[] LTFprevCoherence = new double[LTFarrayCount];  // common name between Mag-NetX, STACIs, ElDamp, PEPS Analyzers

		public int LTFreferenceLineCounter = 0;
		public double[] LTFreferenceFreq = new double[LTFarrayCount];	// common name between STACIs, ElDamp, PEPS Analyzers
		public double[] LTFreferenceGain = new double[LTFarrayCount];	// common name between STACIs, ElDamp, PEPS Analyzers
		public double[] LTFreferencePhase = new double[LTFarrayCount];	// common name between STACIs, ElDamp, PEPS Analyzers
		private double[] LTFrefNegativePh = new double[LTFarrayCount];	// common name between STACIs, ElDamp, PEPS Analyzers
		#endregion 
		/**********************************************************************************************************************/
		private void Clear_Performance_Data_Arrays() {
			for (int i = 0; i < LTFarrayCount; i++) {   // Could also consider setting to "double.NaN" (Not a Number) ...OR... using Array.Clear()
				LTFfrequency[i] = 0;
				LTFdBgain[i] = 0;
				LTFphaseDeg[i] = 0;
				LTFcoherence[i] = 0;
			}
			LTFlineCounter = 0;
		}


		/**********************************************************************************************************************/
		private void Update_Previous_Plot_Data() {
			for (int i = 0; i < LTFarrayCount; i++) {
				// Copy the current data arrays to previous
				LTFprevFreq[i] = LTFfrequency[i];
				LTFprevGain[i] = LTFdBgain[i];
				LTFprevPhase[i] = LTFphaseDeg[i];
			}
			// Plot previous Magnitude and previous Phase
			lock (ScatterGraphMag) { ScatterGraphMag.Plots[PrevLTF].PlotXY(LTFprevFreq, LTFprevGain); }
			lock (ScatterGraphPhase) { ScatterGraphPhase.Plots[PrevLTF].PlotXY(LTFprevFreq, LTFprevPhase); }
		}


		/**********************************************************************************************************************/
		//*** PLOT DATA ONTO THE GRAPHS (VENTURE TECHNOLOGY VERSION)
		private void Update_Performance_Plots() {
			double[] X_data = null;
			double[] Y_data = null;
			// Upper Graph -- Gain
			lock (ScatterGraphMag) {
				//X_data = new double[Frequency_Data.Length];
				//Y_data = new double[Gain_Data.Length];
				lock (LTFfrequency) {
					copy_arrays_to_plot(ref LTFfrequency, ref LTFdBgain, ref X_data, ref Y_data);
					if (ScatterGraphMag.InvokeRequired) {
						Invoke((Action)(() => {
							ScatterGraphMag.Plots[Perfor].PlotXY(X_data, Y_data);
							ScatterGraphMag.Update();
						}));
					} else { ScatterGraphMag.Plots[Perfor].PlotXY(X_data, Y_data); }
				}
			}
			double[] X_data1 = null;
			double[] Y_data1 = null;
			// Lower Graph -- Phase
			lock (ScatterGraphPhase) {
				lock (LTFfrequency) {
					if (ChkShowCoherencePlot.Checked) {
						copy_arrays_to_plot(ref LTFfrequency, ref LTFcoherence, ref X_data1, ref Y_data1);
						Invoke((Action)(() => {
							ScatterGraphPhase.Plots[CohLTF].PlotXY(X_data1, Y_data1);
							ScatterGraphPhase.Update();
						}));
					}
					//Y_data = new double[Phase_Data.Length];
					if (ChkReversePhase.Checked == false)  // NOT reversed phase
						copy_arrays_to_plot(ref LTFfrequency, ref LTFphaseDeg, ref X_data1, ref Y_data1);
					 else  // reversed phase
						copy_arrays_to_plot(ref LTFfrequency, ref LTFnegPhase, ref X_data1, ref Y_data1);

					if (ScatterGraphPhase.InvokeRequired) {
						Invoke((Action)(() => {
							ScatterGraphPhase.Plots[Perfor].PlotXY(X_data1, Y_data1);
							ScatterGraphPhase.Update();
						}));
					} else { ScatterGraphPhase.Plots[Perfor].PlotXY(X_data1, Y_data1); }
				}
			}
			UpdateRefresh();
		}

		/**********************************************************************************************************************/
		private void copy_arrays_to_plot(ref double[] Xdata, ref double[] Ydata, ref double[] Xplot, ref double[] Yplot) {
			List<double> XPlotList = new List<double>();
			List<double> YPlotList = new List<double>();
			int Arr_size = 0;
			Arr_size = Xdata.GetUpperBound(0);
			for (int i = 0; i <= Arr_size; i++) {
				if (Xdata[i] != 0 && Ydata[i] != 0) {
					XPlotList.Add(Xdata[i]);
					YPlotList.Add(Ydata[i]);
				}
			}
			XPlotList.TrimExcess();
			YPlotList.TrimExcess();
			Xplot = XPlotList.ToArray();
			Yplot = YPlotList.ToArray();
		}

		/**********************************************************************************************************************/
		private void Update_Ref_Plot() {
			// Upper Graph -- Gain
			lock(ScatterGraphMag)
			{ ScatterGraphMag.Plots[RefLTF].PlotXY(LTFreferenceFreq, LTFreferenceGain); }
			lock(ScatterGraphPhase)
			{
				if (!ChkReversePhase.Checked)
					ScatterGraphPhase.Plots[RefLTF].PlotXY(LTFreferenceFreq, LTFreferencePhase);
				else
					ScatterGraphPhase.Plots[RefLTF].PlotXY(LTFreferenceFreq, LTFrefNegativePh);
			}


			UpdateRefresh();
		}

		/**********************************************************************************************************************/
		private void Clear_Ref_Data_Arrays() {
			for (int i = 0; i < LTFarrayCount; i++) {   // Could also consider setting to "double.NaN" (Not a Number) ...OR... using Array.Clear()
				LTFreferenceFreq[i] = 0;
				LTFreferenceGain[i] = 0;
				LTFreferencePhase[i] = 0;
				LTFrefNegativePh[i] = 0;
			}
			LTFreferenceLineCounter = 0;
		}

		/**********************************************************************************************************************/
		public class SimpleQuitException : Exception { public SimpleQuitException() { } }

		/**********************************************************************************************************************/
		//=== OPEN OTHER FORMS
		//*** OPEN TERMINAL FORM
		/**********************************************************************************************************************/
		public void OpenTerminalForm(bool isVisible = true) {
			if (/*form already active?*/ Program.formTerminal != null) {
				if (!Program.formTerminal.Visible && !Program.formTerminal.FormTerminalVisible) {
					LblTerminalWindow.Font = new Font("Tahoma", 11.0F, FontStyle.Bold);
					LblTerminalWindow.ForeColor = Color.Maroon;
					Program.formTerminal.Show();
					Program.formTerminal.Visible = true;
					Program.formTerminal.FormTerminalVisible = true;
					return;
				} else {
					LblTerminalWindow.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
					LblTerminalWindow.ForeColor = Color.Black;
					Program.formTerminal.Hide();
					Program.formTerminal.FormTerminalVisible = false;
				}
			} else {
				LblTerminalWindow.Font = new Font("Tahoma", 11.0F, FontStyle.Bold);
				LblTerminalWindow.ForeColor = Color.Maroon;
				Program.formTerminal = new Terminal();
				Program.formTerminal.Show();
				Program.formTerminal.Visible = true;
				Program.formTerminal.FormTerminalVisible = true;
			}
		}

		/**********************************************************************************************************************/
		//public void TerminalFormHasClosed() {
		//	//ensure the TERMINAL form is Dead
		//	Program.formTerminal = null;
		//}

		/**********************************************************************************************************************/
		//public void RealTimeMonitoringFormHasClosed() {
		//	// Ensure that the real time monitoring form is dead.
		//	Program.FormRTMonitoring = null;

		//	// Reset the menu so we know the form has been closed.
		//	//MonitoringMenuLabel.Font = FontWhenNotSelected;
		//	//MonitoringMenuLabel.ForeColor = TextColorWhenFormWindowIsOpen;
		//}

		//*** OPEN SCOPE FORM
		private void ScopeMenuIcon_Click(object sender, EventArgs e) { ScopeFormOpen(); }
		private void ScopeMenuLabel_Click(object sender, EventArgs e) { ScopeFormOpen(); }
		private void ScopeMenuPanel_Click(object sender, EventArgs e) { ScopeFormOpen(); }


		/**********************************************************************************************************************/
		private void ScopeFormOpen() {
#if (true) // start external process DC-2020_Scope
			string exePath = @"C:\Program Files (x86)\TMC\DC 2020 Scope\DC-2020_scope.exe";
			try {
				Process.Start(new ProcessStartInfo {
					FileName = exePath,
					UseShellExecute = true   // important for .exe in Program Files
				});
			} catch (Exception ex) {
				MessageBox.Show("Failed to start application:\n" + ex.Message);
			}
#else
			ToggleRTmonitoring.Checked = false;
			ContinuousMonitoringOn = false;
			if (Program.formTerminal != null)
				Program.formTerminal.OnQuitMenuCommand();
			ContinuousMonitoringSwitchShow();
			ContinuousMonitoringOffOn(ContinuousMonitoringOn);
			if (/*form already active?*/ Program.FormScope != null) {
				//   Make the already running form visible and give it focus.

				if (ScopeThread.ThreadState == System.Threading.ThreadState.Suspended)
					ScopeThread.Resume();

				Program.FormScope.BeginInvoke((MethodInvoker)delegate
				{
					Program.FormScope.Show();      //ScopeIntegration_01082018_Tismo
					Program.FormScope.Select();
					Program.FormScope.Activate();
					Program.FormScope.WindowState = FormWindowState.Normal;
					Program.FormScope.initFormHelpButton();
				});
			} else {
				if (Program.ConnectedController != null) {
					ScopeThread = new Thread(() =>
					{
						Program.FormScope = new formScope(Program.DualPortController, TMCController.ControllerPort.serialPortMuxerBase, Program.ConnectionType); // GARY-FIX: don't pass scope a controller-object, just use the program level one  //ScopeIntegration_01082018_Tismo
						Program.FormScope.InitializeChannelNamesFromAnalyzer(ChName);
						Application.Run(Program.FormScope);
					});
					ScopeThread.SetApartmentState(ApartmentState.STA);
					ScopeThread.Start();
					//Program.FormScope.Visible = true;
					//Program.FormScope.Activate();
				}
			}
			TMC_Scope.Program.IsScopeFormHidden = false;
#endif
		}

		/**********************************************************************************************************************/
		//public void ScopeFormHasClosed() {//-!- IK20211230 not used
		//	// Ensure that the scope form is dead.
		//	Program.FormScope = null;
		//}

#if false
//=== PuTTY Terminal Emulator
//
// Early on in the development of moving Igor's Visual-Basic 6 version to the Visual-Studio.NET C# version,
// Venture Technology (that was doing the converstion) used the PuTTY open source terminal emulation software
// in-place of our own "home-grown" terminal emulator. The following is the code used to call & end PuTTY
//
// CODE TO START PuTTY
var /*PuTTY*/ pty = new System.Diagnostics.Process();
pty.StartInfo.FileName = "pty.exe"; // "-serial <comport>"
pty.StartInfo.Arguments = string.Format("-serial {0} -sercfg {1},8,n,1,N", Program.ConnectedController.PortName, Program.ConnectedController.BaudRate);
pty.EnableRaisingEvents = true;
pty.Exited += new EventHandler(putty_Exited);

// Put the controller into echo mode and disconnect before starting pty
Program.ConnectedController.EchoState = EchoStates.Enabled;
Program.ConnectedController.Close();
pty.Start();

// CODE WHEN PuTTY EXITED
void putty_Exited(object sender, EventArgs e) {
// Reconnect the Analyzer.
Program.ConnectedController.Open();
}
#endif


		// Boolean Variables That Will Limit Accessing ANALYZER If We Are Already Doing Other Things
		[DefaultValue(false)]
		public bool LTFIsRunning { get; private set; }
		public static DateTime LTFStartTime
		{
			get { return ltfStartTimeBase; }
			private set { ltfStartTimeBase = System.DateTime.Now; }
		}
		private static System.DateTime ltfStartTimeBase = System.DateTime.MinValue;
		public static int LTF_Lines_Counter;

		// controller temperature vars
		public static double ControllerTemperature;
		public bool ControllerTemperatureFlashLabel;
		public bool ControllerTemperatureReportInFahrenheit = false;
		public const int CautionTemperature = 45;
		public const int CriticalTemperature = 60;

		/// <summary>Path gets the fully qualified path to the application-state folder.</summary>
		/// <value>
		/// fully qualified path (ending with reverse-solidus) to the application's state
		/// folder</value>
		/// <example><code>Debug.Assert(System.IO.Directory.Exists(TMC_Path));</code></example>
		public static string TMC_Path {
			get {
				if (string.IsNullOrWhiteSpace(tmc_PathBase)) { // prepare
					TMC_Path = null;//use defaults
				}
				return tmc_PathBase;
			}
			private set {
				if (string.IsNullOrWhiteSpace(value)) {    // use default
					value = TMC_PathValue;
				}
				if (!value.EndsWith("\\")) {  // append slash
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

		public int BNC_1_index { get; private set; }

		/// <summary>
		/// Path Logs Value specifies the subfolder for the application's log(s).</summary>
		public const string TMC_PathLogsValue = "Logs\\";

		/// <summary>
		/// Path Value specifies the default subfolder for the application-state.</summary>
		public const string TMC_PathValue = "\\TMC\\TMCAnalyzer";

		private void buttonSaveToFlash_Click(object sender, EventArgs e) {
			Program.ConnectedController.SettingsInRam = SettingsActions.SaveRamSettingsToFlash;
		}

		private void SetupMuxeREventHandler() { Program.SerialPortMultiplexer.ComPortError_Event += new SerPort.ComPortErrorEventHandler_FunctionPointer(ConnectionLost); }

		private void ConnectionLost(string x, string y) {
			Program.ConnectedController.CommStatus = CommStatusStates.CommError;
		}


		/**********************************************************************************************************************/
		private void UpdateRefresh() {
			if (InvokeRequired)
				BeginInvoke((MethodInvoker)delegate { Refresh(); Update(); });
			else { Refresh(); Update(); }
		}

		private void labelConnectionState_Click(object sender, EventArgs e) {
			Connect_Or_Disconnect();
		}

		/**********************************************************************************************************************/
		public void DiagScreenTimeToExecuteCommandShow(string Time_as_Text) {
			if (/*OK (maybe)?*/ Time_as_Text != null) {
				Time_as_Text = Time_as_Text.Trim();
				if (/*OK?*/ !string.IsNullOrWhiteSpace(Time_as_Text)) {
					if (DiagScreenTimeToExecuteCommand.InvokeRequired)
						DiagScreenTimeToExecuteCommand.BeginInvoke((MethodInvoker)delegate
						{
							DiagScreenTimeToExecuteCommand.Text = Time_as_Text;
							DiagScreenTimeToExecuteCommand.Update();
							DiagScreenTimeToExecuteCommand.Refresh();
						});
					else {
						DiagScreenTimeToExecuteCommand.Text = Time_as_Text;
						DiagScreenTimeToExecuteCommand.Update();
						DiagScreenTimeToExecuteCommand.Refresh();
					}
					if (lblCMDtime.InvokeRequired)
						lblCMDtime.BeginInvoke((MethodInvoker)delegate
						{
							lblCMDtime.Text = Time_as_Text;
							lblCMDtime.Update();
							lblCMDtime.Refresh();
						});
					else {
						lblCMDtime.Text = Time_as_Text;
						lblCMDtime.Update();
						lblCMDtime.Refresh();
					}
				}
			}
		}

		/**********************************************************************************************************************/
		public void DiagScreenLastCommandShow(string LastCommand) {
			if (/*OK (maybe)?*/ LastCommand != null) {
				LastCommand = LastCommand.Trim();
				if (/*OK?*/ !string.IsNullOrWhiteSpace(LastCommand)) {
					this.Invoke((System.Action)(() => {
						textBoxLastCommand.Text = LastCommand;
						textBoxLastCommand.Update();
						txtManualCMD.Text = LastCommand;
						txtManualCMD.Update();
						txtStatus.Text = LastCommand;
						txtStatus.Update();
					}));
				}
			}
		}

		/**********************************************************************************************************************/
		public AckTypes SendInternal(string command, CommandTypes commandType, out string response, CommandAckTypes ackType) {
			AckTypes retAck = AckTypes.Ok; // mdr 053118
			bool IsCheckAutoClear = false;
			if (Program.ConnectedController == null) {
				if (InvokeRequired) {
					this.Invoke((Action)(() => { ConnectToController(true, false); }));
				} else {
					ConnectToController(true, false);
				}
			}
			if (Program.ConnectedController.CommStatus == CommStatusStates.Connected) {
				try {
					lock (this) {
						//    if (Chk_AutoClear.InvokeRequired) IsCheckAutoClear = (bool)Chk_AutoClear.Invoke(new Func<int>(checked)));
						//    else IsCheckAutoClear = Chk_AutoClear.Checked;
						Chk_AutoClear.BeginInvoke((MethodInvoker)delegate
						{
							IsCheckAutoClear = Chk_AutoClear.Checked;
						});
						lstResponse.BeginInvoke((MethodInvoker)delegate
						{
							if (IsCheckAutoClear) {
								if (lstResponse.Items.Count > 1000)
									lstResponse.Items.Remove(lstResponse.Items[0]);
							}
							lstResponse.Items.Add("-->" + command);
							lstResponse.Update();
						});

						retAck = Program.ConnectedController.SendInternal(command, commandType, out response, ackType);

						if (Program.formTerminal != null) {
							if (response != "")
								Program.formTerminal.MainForm_SendCallBack(command, response);
						}
						if (commandType != CommandTypes.NoResponseExpected || !String.IsNullOrEmpty(response)) {
							int acked = 0;
							if (response.Contains(">~OK")) {
								acked = response.IndexOf(">~OK");
							} else if (response.Contains(">~Doing CMD")) {
								acked = response.IndexOf(">~Doing CMD");
							} else {    //see if we got an error instead
								acked = response.IndexOf("Error Code = ");
							}

							if (acked != -1) {
								response = response.Remove(acked); //remove end of line, it could be: "\n>~Unrecognized Cmd, Error Code = 1\r\nscope>freq=2000.00\r\n>~OK\r\n\n>~Unrecognized Cmd, Error Code = 1"
								response = response.TrimEnd('\n', '\r');
							}
						}

						string responseFromDevice = response;
						List<string> responseWithMultipleLines;
						if (responseFromDevice.Contains("\r\n")) {
							responseWithMultipleLines = responseFromDevice.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
							foreach (var item in responseWithMultipleLines) {
								lstResponse.BeginInvoke((MethodInvoker)delegate { lstResponse.Items.Add(item); lstResponse.Update(); });
							}
						} else {
							lstResponse.BeginInvoke((MethodInvoker)delegate { lstResponse.Items.Add(responseFromDevice); lstResponse.Update(); });
						}
					}
					//sendDataManualEvent.Set();
					return retAck;
				} catch (Exception ex) {
					// sendDataManualEvent.Set();
				}
			} else {
				response = string.Empty;
				retAck = AckTypes.PortError;
				//sendDataManualEvent.Set();
				return retAck;
			}

			/* mdr 053118 */
			response = string.Empty;
			//sendDataManualEvent.Set();
			return retAck;
		}

		string[] ChName;

		/**********************************************************************************************************************/
		private void ReadChannelNamesFromController() {

			ChName = new string[256];
			int ch;
			string resp;
			// string response;
			var rsp = string.Empty;
			string cmdStr;
			int ch0_indx;
			long spos;
			bool dname_command;
			string NameOfVariable = string.Empty;
			//     Dim CmdTime As Single
			// StartTime = Timer;
			var watch = System.Diagnostics.Stopwatch.StartNew();

			//  response = GetSend("dnam0", true);
			SendInternal("dnam0", CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

			// // "dNam###[?]" - will print the name of the variable
			if (rsp.Contains("Unrec")) {
				dname_command = false;
				//  rsp = GetSend("echo>verb", true);
				SendInternal("echo>verb", CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

				// preserve actual index / name of ch0
				cmdStr = "frame0?";
				ch0_indx = GetIndexOfChannel(0);
				for (ch = 0; (ch <= 255); ch++) {
					watch = System.Diagnostics.Stopwatch.StartNew();
					//  "frameN=XX" - will replace pointer to the variable in frame "N"
					// cmd: frame0
					// resp: frame0=4 // SPI data Prx_1Z_um ->frame #0(0)
					cmdStr = ("frame0=" + ch.ToString());
					//  rsp = GetSend(cmdStr, true);
					SendInternal(cmdStr, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);
					//  Thread.Sleep(5);
					cmdStr = "frame0?";
					//  rsp = GetSend(cmdStr, true);
					SendInternal(cmdStr, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);
					resp = rsp;
					// keep original for debug
					// resp: frame0=4 // SPI data Prx_1Z_um ->frame #0(0)
					spos = (resp.IndexOf("data") + 1);
					NameOfVariable = "N/A";
					if ((spos > 0)) {
						NameOfVariable = resp.Substring(((int)spos + 4));
						// result: Prx_1Z_um ->frame #0(0)
						spos = (NameOfVariable.IndexOf(" ->") + 1);
						if ((spos > 0)) {
							NameOfVariable = NameOfVariable.Substring(0, (int)spos);
							// result: Prx_1Z_um
							ChName[ch] = NameOfVariable;
						} else {
							// error
							NameOfVariable = ("error" + ch);
						}
					} else {
						// error
						NameOfVariable = ("error" + ch);
					}

					watch.Stop();

					var elapsedMs = watch.ElapsedMilliseconds;
					txtStatus.Text = (String.Format("{0:0000}s", (TimeSpan.FromMilliseconds(elapsedMs).TotalSeconds)) + (" Reading Ch Name "
						+ (String.Format("{000}", ch) + NameOfVariable)));
				}

				//  restore original frame
				cmdStr = ("frame0=" + ch0_indx.ToString());
				//  response = GetSend(cmdStr, true);
				SendInternal(cmdStr, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

			} else {
				dname_command = true;
				for (ch = 0; (ch <= 255); ch++) {
					watch = System.Diagnostics.Stopwatch.StartNew();
					// cmd: dnamXXX
					// resp: dnam0=Prx_1Z_um
					cmdStr = ("dnam" + ch.ToString());
					// NameOfVariable = GetSend(cmdStr, true);

					SendInternal(cmdStr, CommandTypes.ResponseExpected, out NameOfVariable, CommandAckTypes.AckExpected);

					//             CmdTime = Timer - Time_Now
					//             DoEvents
					if ((NameOfVariable == "zero")) {
						NameOfVariable = "--zero--";
					}

					ChName[ch] = NameOfVariable;
					watch.Stop();
					var elapsedMs = watch.ElapsedMilliseconds;
					txtStatus.Text = TimeSpan.FromMilliseconds(elapsedMs).TotalSeconds.ToString() + "sec " + (" Reading Ch Name "
						+ (String.Format("{000}", ch) + NameOfVariable));
				}
			}

			// response = GetSend("echo>enab", False) ' disable verbose to speed up transmission
			// replace names
			CmbBNC0.Items.Clear();
			CmbBNC1.Items.Clear();
			for (ch = 0; (ch <= 255); ch++) {
				CmbBNC0.Items.Add(ChName[ch]);
				CmbBNC1.Items.Add(ChName[ch]);
			}
		}

		/**********************************************************************************************************************/
		// only called from one place
		private int GetIndexOfChannel(int chnl) {
			//  "frameN=XX" - will replace pointer to the variable in frame "N"
			// response
			string resp;
			resp = string.Empty;
			string cmdStr;
			long spos;
			int indx_int;
			string indx_str;
			cmdStr = ("frame"
			+ String.Format("{0:x4}", chnl)/*(Hex(chnl) */+ "?");
			//  resp = GetSend(cmdStr, true);
			SendInternal(cmdStr, CommandTypes.NoResponseExpected, out resp, CommandAckTypes.AckExpected);

			// resp: frameC=4 // SPI data Prx_1Z_um ->frame #0(0)
			spos = (resp.IndexOf("=") + 1);
			if ((spos > 0)) {
				indx_str = resp.Substring((int)spos);
				spos = (indx_str.IndexOf("//") + 1);
				// trim verbose comment
				if ((spos > 1)) {
					indx_str = indx_str.Substring(0, ((int)spos - 1));
				}

				spos = (indx_str.IndexOf(">~") + 1);
				if (IsNumeric(indx_str)) {
					indx_int = int.Parse(indx_str);
				} else {
					// error
					indx_int = -1;
				}
			} else {
				// error
				indx_int = -1;
			}
			//GetIndexOfChannel(indx_int);
			return indx_int;
		}

		/**********************************************************************************************************************/
		public static bool IsNumeric(object Expression) {
			double retNum;

			bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
			return isNum;
		}
		// Lis<string> ChName = new List<string>();
		/**********************************************************************************************************************/
		void AssignChNames() {
			ChName = new string[256];
			int ch;
			ch = -1;
			ch = (ch + 1);
			ChName[ch] = "----";
			ch = (ch + 1);
			ChName[ch] = "Prx_1X_um";
			ch = (ch + 1);
			ChName[ch] = "Prx_2Y_um";
			ch = (ch + 1);
			ChName[ch] = "Prx_3X_um";
			ch = (ch + 1);
			ChName[ch] = "Prx_1Z_um";
			ch = (ch + 1);
			ChName[ch] = "Prx_2Z_um";
			ch = (ch + 1);
			ChName[ch] = "Prx_3Z_um";
			ch = (ch + 1);
			ChName[ch] = "prx1Hraw";
			ch = (ch + 1);
			ChName[ch] = "prx2Hraw";
			ch = (ch + 1);
			ChName[ch] = "prx3Hraw";
			ch = (ch + 1);
			ChName[ch] = "pr1Hcnts";
			ch = (ch + 1);
			ChName[ch] = "pr2Hcnts";
			ch = (ch + 1);
			ChName[ch] = "pr3Hcnts";
			ch = (ch + 1);
			ChName[ch] = "Prx1Zraw";
			ch = (ch + 1);
			ChName[ch] = "Prx2Zraw";
			ch = (ch + 1);
			ChName[ch] = "Prx3Zraw";
			ch = (ch + 1);
			ChName[ch] = "FF_Xstg POS";
			ch = (ch + 1);
			ChName[ch] = "FF_Ystg POS";
			ch = (ch + 1);
			ChName[ch] = "FF_Xstg ACC";
			ch = (ch + 1);
			ChName[ch] = "FF_Ystg ACC";
			ch = (ch + 1);
			ChName[ch] = "P_iso1_cnts";
			ch = (ch + 1);
			ChName[ch] = "P_iso2_cnts";
			ch = (ch + 1);
			ChName[ch] = "P_iso3_cnts";
			ch = (ch + 1);
			ChName[ch] = "P_iso4_cnts";
			ch = (ch + 1);
			ChName[ch] = "Geo_1H";
			ch = (ch + 1);
			ChName[ch] = "Geo_2H";
			ch = (ch + 1);
			ChName[ch] = "Geo_3H";
			ch = (ch + 1);
			ChName[ch] = "Geo_1Z";
			ch = (ch + 1);
			ChName[ch] = "Geo_2Z";
			ch = (ch + 1);
			ChName[ch] = "Geo_3Z";
			ch = (ch + 1);
			ChName[ch] = "Pressure_IN_cnts";
			ch = (ch + 1);
			ChName[ch] = "Diag_In1";
			ch = (ch + 1);
			ChName[ch] = "Valve_1";
			ch = (ch + 1);
			ChName[ch] = "Valve_2";
			ch = (ch + 1);
			ChName[ch] = "Valve_3";
			ch = (ch + 1);
			ChName[ch] = "Valve_4";
			ch = (ch + 1);
			ChName[ch] = "Motor1Z";
			ch = (ch + 1);
			ChName[ch] = "Motor2Z";
			ch = (ch + 1);
			ChName[ch] = "Motor3Z";
			ch = (ch + 1);
			ChName[ch] = "Motor4Z";
			ch = (ch + 1);
			ChName[ch] = "Motor1H";
			ch = (ch + 1);
			ChName[ch] = "Motor2H";
			ch = (ch + 1);
			ChName[ch] = "Motor3H";
			ch = (ch + 1);
			ChName[ch] = "Motor4H";
			ch = (ch + 1);
			ChName[ch] = "DAC_ch6";
			ch = (ch + 1);
			ChName[ch] = "DAC_ch7";
			ch = (ch + 1);
			ChName[ch] = "BNCdiag0";
			ch = (ch + 1);
			ChName[ch] = "BNCdiag1";
			ch = (ch + 1);
			ChName[ch] = "X_vel";
			ch = (ch + 1);
			ChName[ch] = "Y_vel";
			ch = (ch + 1);
			ChName[ch] = "tZ_vel";
			ch = (ch + 1);
			ChName[ch] = "Z_vel";
			ch = (ch + 1);
			ChName[ch] = "tX_vel";
			ch = (ch + 1);
			ChName[ch] = "tY_vel";
			ch = (ch + 1);
			ChName[ch] = "X_Pos_BNC";
			ch = (ch + 1);
			ChName[ch] = "Y_Pos_BNC";
			ch = (ch + 1);
			ChName[ch] = "tZ_Pos_BNC";
			ch = (ch + 1);
			ChName[ch] = "Z_Pos_BNC";
			ch = (ch + 1);
			ChName[ch] = "tX_Pos_BNC";
			ch = (ch + 1);
			ChName[ch] = "tY_Pos_BNC";
			ch = (ch + 1);
			ChName[ch] = "BNC 0 Input";
			ch = (ch + 1);
			ChName[ch] = "BNC 1 Input";
			ch = (ch + 1);
			ChName[ch] = "---";
			ch = (ch + 1);
			ChName[ch] = "---";
			ch = (ch + 1);
			ChName[ch] = "ADC_00_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_01_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_02_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_03_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_04_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_05_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_06_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_07_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_08_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_09_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_10_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_11_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_12_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_13_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_14_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_15_16b";
			ch = (ch + 1);
			ChName[ch] = "ADC_16_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_17_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_18_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_19_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_20_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_21_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_22_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_23_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_24_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_25_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_26_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_27_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_28_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_29_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_30_12b";
			ch = (ch + 1);
			ChName[ch] = "ADC_31_12b";
			ch = (ch + 1);
			ChName[ch] = "DAC_00";
			ch = (ch + 1);
			ChName[ch] = "DAC_01";
			ch = (ch + 1);
			ChName[ch] = "DAC_02";
			ch = (ch + 1);
			ChName[ch] = "DAC_03";
			ch = (ch + 1);
			ChName[ch] = "DAC_04";
			ch = (ch + 1);
			ChName[ch] = "DAC_05";
			ch = (ch + 1);
			ChName[ch] = "DAC_06";
			ch = (ch + 1);
			ChName[ch] = "DAC_07";
			ch = (ch + 1);
			ChName[ch] = "DAC_08";
			ch = (ch + 1);
			ChName[ch] = "DAC_09";
			ch = (ch + 1);
			ChName[ch] = "DAC_10";
			ch = (ch + 1);
			ChName[ch] = "DAC_11";
			ch = (ch + 1);
			ChName[ch] = "DAC_12";
			ch = (ch + 1);
			ChName[ch] = "DAC_13";
			ch = (ch + 1);
			ChName[ch] = "DAC_14";
			ch = (ch + 1);
			ChName[ch] = "DAC_15";
			ch = (ch + 1);
			ChName[ch] = "DAC_16";
			ch = (ch + 1);
			ChName[ch] = "DAC_17";
			ch = (ch + 1);
			ChName[ch] = "DAC_18";
			ch = (ch + 1);
			ChName[ch] = "DAC_19";
			ch = (ch + 1);
			ChName[ch] = "DAC_20";
			ch = (ch + 1);
			ChName[ch] = "DAC_21";
			ch = (ch + 1);
			ChName[ch] = "DAC_22";
			ch = (ch + 1);
			ChName[ch] = "DAC_23";
			ch = (ch + 1);
			ChName[ch] = "Excitation";
			ch = (ch + 1);
			ChName[ch] = "zero";
			ch = (ch + 1);
			ChName[ch] = "zero";
			ch = (ch + 1);
			ChName[ch] = "XaccDetect";
			ch = (ch + 1);
			ChName[ch] = "YaccDetect";
			ch = (ch + 1);
			ChName[ch] = "XposDetect";
			ch = (ch + 1);
			ChName[ch] = "YposDetect";
			ch = (ch + 1);
			ChName[ch] = "zero";
			ch = (ch + 1);
			ChName[ch] = "Xv-Filt0";
			ch = (ch + 1);
			ChName[ch] = "Xv-Filt1";
			ch = (ch + 1);
			ChName[ch] = "Xv-Filt2";
			ch = (ch + 1);
			ChName[ch] = "Xv-Filt3";
			ch = (ch + 1);
			ChName[ch] = "Xv-Filt4";
			ch = (ch + 1);
			ChName[ch] = "Xv-Filt5";
			ch = (ch + 1);
			ChName[ch] = "Yv-Filt0";
			ch = (ch + 1);
			ChName[ch] = "Yv-Filt1";
			ch = (ch + 1);
			ChName[ch] = "Yv-Filt2";
			ch = (ch + 1);
			ChName[ch] = "Yv-Filt3";
			ch = (ch + 1);
			ChName[ch] = "Yv-Filt4";
			ch = (ch + 1);
			ChName[ch] = "Yv-Filt5";
			ch = (ch + 1);
			ChName[ch] = "tZv-Filt0";
			ch = (ch + 1);
			ChName[ch] = "tZv-Filt1";
			ch = (ch + 1);
			ChName[ch] = "tZv-Filt2";
			ch = (ch + 1);
			ChName[ch] = "tZv-Filt3";
			ch = (ch + 1);
			ChName[ch] = "tZv-Filt4";
			ch = (ch + 1);
			ChName[ch] = "tZv-Filt5";
			ch = (ch + 1);
			ChName[ch] = "Zv-Filt0";
			ch = (ch + 1);
			ChName[ch] = "Zv-Filt1";
			ch = (ch + 1);
			ChName[ch] = "Zv-Filt2";
			ch = (ch + 1);
			ChName[ch] = "Zv-Filt3";
			ch = (ch + 1);
			ChName[ch] = "Zv-Filt4";
			ch = (ch + 1);
			ChName[ch] = "Zv-Filt5";
			ch = (ch + 1);
			ChName[ch] = "tXv-Filt0";
			ch = (ch + 1);
			ChName[ch] = "tXv-Filt1";
			ch = (ch + 1);
			ChName[ch] = "tXv-Filt2";
			ch = (ch + 1);
			ChName[ch] = "tXv-Filt3";
			ch = (ch + 1);
			ChName[ch] = "tXv-Filt4";
			ch = (ch + 1);
			ChName[ch] = "tXv-Filt5";
			ch = (ch + 1);
			ChName[ch] = "tYv-Filt0";
			ch = (ch + 1);
			ChName[ch] = "tYv-Filt1";
			ch = (ch + 1);
			ChName[ch] = "tYv-Filt2";
			ch = (ch + 1);
			ChName[ch] = "tYv-Filt3";
			ch = (ch + 1);
			ChName[ch] = "tYv-Filt4";
			ch = (ch + 1);
			ChName[ch] = "tYv-Filt5";
			ch = (ch + 1);
			ChName[ch] = "Xp-Filt0";
			ch = (ch + 1);
			ChName[ch] = "Xp-Filt1";
			ch = (ch + 1);
			ChName[ch] = "Xp-Filt2";
			ch = (ch + 1);
			ChName[ch] = "Xp-Filt3";
			ch = (ch + 1);
			ChName[ch] = "Xp-Filt4";
			ch = (ch + 1);
			ChName[ch] = "Xp-Filt5";
			ch = (ch + 1);
			ChName[ch] = "Yp-Filt0";
			ch = (ch + 1);
			ChName[ch] = "Yp-Filt1";
			ch = (ch + 1);
			ChName[ch] = "Yp-Filt2";
			ch = (ch + 1);
			ChName[ch] = "Yp-Filt3";
			ch = (ch + 1);
			ChName[ch] = "Yp-Filt4";
			ch = (ch + 1);
			ChName[ch] = "Yp-Filt5";
			ch = (ch + 1);
			ChName[ch] = "tZp-Filt0";
			ch = (ch + 1);
			ChName[ch] = "tZp-Filt1";
			ch = (ch + 1);
			ChName[ch] = "tZp-Filt2";
			ch = (ch + 1);
			ChName[ch] = "tZp-Filt3";
			ch = (ch + 1);
			ChName[ch] = "tZp-Filt4";
			ch = (ch + 1);
			ChName[ch] = "tZp-Filt5";
			ch = (ch + 1);
			ChName[ch] = "Zp-Filt0";
			ch = (ch + 1);
			ChName[ch] = "Zp-Filt1";
			ch = (ch + 1);
			ChName[ch] = "Zp-Filt2";
			ch = (ch + 1);
			ChName[ch] = "Zp-Filt3";
			ch = (ch + 1);
			ChName[ch] = "Zp-Filt4";
			ch = (ch + 1);
			ChName[ch] = "Zp-Filt5";
			ch = (ch + 1);
			ChName[ch] = "tXp-Filt0";
			ch = (ch + 1);
			ChName[ch] = "tXp-Filt1";
			ch = (ch + 1);
			ChName[ch] = "tXp-Filt2";
			ch = (ch + 1);
			ChName[ch] = "tXp-Filt3";
			ch = (ch + 1);
			ChName[ch] = "tXp-Filt4";
			ch = (ch + 1);
			ChName[ch] = "tXp-Filt5";
			ch = (ch + 1);
			ChName[ch] = "tYp-Filt0";
			ch = (ch + 1);
			ChName[ch] = "tYp-Filt1";
			ch = (ch + 1);
			ChName[ch] = "tYp-Filt2";
			ch = (ch + 1);
			ChName[ch] = "tYp-Filt3";
			ch = (ch + 1);
			ChName[ch] = "tYp-Filt4";
			ch = (ch + 1);
			ChName[ch] = "tYp-Filt5";
			ch = (ch + 1);
			ChName[ch] = "XaFF-Filt0";
			ch = (ch + 1);
			ChName[ch] = "XaFF-Filt1";
			ch = (ch + 1);
			ChName[ch] = "XaFF-Filt2";
			ch = (ch + 1);
			ChName[ch] = "XaFF-Filt3";
			ch = (ch + 1);
			ChName[ch] = "XaFF-Filt4";
			ch = (ch + 1);
			ChName[ch] = "XaFF-Filt5";
			ch = (ch + 1);
			ChName[ch] = "YaFF-Filt0";
			ch = (ch + 1);
			ChName[ch] = "YaFF-Filt1";
			ch = (ch + 1);
			ChName[ch] = "YaFF-Filt2";
			ch = (ch + 1);
			ChName[ch] = "YaFF-Filt3";
			ch = (ch + 1);
			ChName[ch] = "YaFF-Filt4";
			ch = (ch + 1);
			ChName[ch] = "YaFF-Filt5";
			ch = (ch + 1);
			ChName[ch] = "XpFF-Filt0";
			ch = (ch + 1);
			ChName[ch] = "XpFF-Filt1";
			ch = (ch + 1);
			ChName[ch] = "XpFF-Filt2";
			ch = (ch + 1);
			ChName[ch] = "XpFF-Filt3";
			ch = (ch + 1);
			ChName[ch] = "XpFF-Filt4";
			ch = (ch + 1);
			ChName[ch] = "XpFF-Filt5";
			ch = (ch + 1);
			ChName[ch] = "YpFF-Filt0";
			ch = (ch + 1);
			ChName[ch] = "YpFF-Filt1";
			ch = (ch + 1);
			ChName[ch] = "YpFF-Filt2";
			ch = (ch + 1);
			ChName[ch] = "YpFF-Filt3";
			ch = (ch + 1);
			ChName[ch] = "YpFF-Filt4";
			ch = (ch + 1);
			ChName[ch] = "YpFF-Filt5";
			ch = (ch + 1);
			ChName[ch] = "PSI_I1fil0";
			ch = (ch + 1);
			ChName[ch] = "PSI_I1fil1";
			ch = (ch + 1);
			ChName[ch] = "PSI_I1fil2";
			ch = (ch + 1);
			ChName[ch] = "PSI_I2fil0";
			ch = (ch + 1);
			ChName[ch] = "PSI_I2fil1";
			ch = (ch + 1);
			ChName[ch] = "PSI_I2fil2";
			ch = (ch + 1);
			ChName[ch] = "PSI_I3fil0";
			ch = (ch + 1);
			ChName[ch] = "PSI_I3fil1";
			ch = (ch + 1);
			ChName[ch] = "PSI_I3fil2";
			ch = (ch + 1);
			ChName[ch] = "PSI_I4fil0";
			ch = (ch + 1);
			ChName[ch] = "PSI_I4fil1";
			ch = (ch + 1);
			ChName[ch] = "PSI_I4fil2";
			ch = (ch + 1);
			ChName[ch] = "Xpos-->Mot";
			ch = (ch + 1);
			ChName[ch] = "Ypos-->Mot";
			ch = (ch + 1);
			ChName[ch] = "tZpos->Mot";
			ch = (ch + 1);
			ChName[ch] = "Zpos-Valve";
			ch = (ch + 1);
			ChName[ch] = "tXpos-Valve";
			ch = (ch + 1);
			ChName[ch] = "tYpos-Valve";
			ch = (ch + 1);
			ChName[ch] = "Xvel-->Mot";
			ch = (ch + 1);
			ChName[ch] = "Yvel-->Mot";
			ch = (ch + 1);
			ChName[ch] = "tZvel->Mot";
			ch = (ch + 1);
			ChName[ch] = "Zvel-->Mot";
			ch = (ch + 1);
			ChName[ch] = "tXvel->Mot";
			ch = (ch + 1);
			ChName[ch] = "tYvel->Mot";
			ch = (ch + 1);
			ChName[ch] = "FFout->tXpos";
			ch = (ch + 1);
			ChName[ch] = "FFout->tYpos";
			ch = (ch + 1);
			ChName[ch] = "FFout->X_vel";
			ch = (ch + 1);
			ChName[ch] = "FFout->Y_vel";
			ch = (ch + 1);
			ChName[ch] = "FFout->tZvel";
			ch = (ch + 1);
			ChName[ch] = "FFout->tXvel";
			ch = (ch + 1);
			ChName[ch] = "FFout->tYvel";
			ch = (ch + 1);
			ChName[ch] = "zero";
			CmbBNC0.Items.Clear();
			CmbBNC1.Items.Clear();
			for (ch = 0; (ch <= 255); ch++) {
				CmbBNC0.Items.Add(ChName[ch]);
				CmbBNC1.Items.Add(ChName[ch]);
			}
		}
		private void ChkAirIsolatorsPresent_CheckedChanged(object sender, EventArgs e) {
			if (ChkAirIsolatorsPresent.Checked) {
				AirIsoPresent = true;
				ChkAirIsolatorsPresent.BackColor = Color.FromArgb(192, 255, 255);
				ChkAirIsolatorsPresent.Text = "Air Isolators";
				FramePneumFB.Visible = true;
				FrmVertPosToMotGains.Visible = false;

				var frmFilters = Application.OpenForms["frmFilters"] as frmFilters;
				if (frmFilters != null) {
					frmFilters.FramePneumatic.Text = "Air Isolators";
					frmFilters.FramePneumatic.Enabled = true;
				}
			} else {
				AirIsoPresent = false;
				FramePneumFB.Visible = false;
				FrmVertPosToMotGains.Visible = true;

				var frmFilters = Application.OpenForms["frmFilters"] as frmFilters;
				if (frmFilters != null) {
					frmFilters.FramePneumatic.Text = "NO AIR Isolators";
					frmFilters.FramePneumatic.Enabled = false;
				}

				ChkAirIsolatorsPresent.BackColor = Color.FromArgb(255, 128, 0);
				ChkAirIsolatorsPresent.Text = "NO Air Isolators";
			}
		}

		/**********************************************************************************************************************/
		private void cmdRefresh_Click(object sender, EventArgs e) {
			if (Test_in_progress)
				return;
			get_ID();
			GetSysData();
			read_RT_diagnostic();
		}

		public string SystemFWversion;


		/**********************************************************************************************************************/
		private void RefreshSettingsPage() {
			//     // TODO: On Error GoTo Warning!!!: The statement is not translatable
			int i;
			int sep_pos;
			double paramValue;
			long max_indx;
			string command;
			string contr_response;

			SystemFWversion = txtFW_Version.Text = Program.ConnectedController.FirmwareVersion;

			SendInternal("echo>enab", CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
			Read_Position_Sensors();
			max_indx = NInumFBgain.Count;

			for (i = 0; (i < max_indx); i++) {
				if (NInumFBgain[i].Visible == true) {
					paramValue = RectifyValueResponse(NInumFBgain[i].Tag.ToString());
					if (paramValue != WRONG_VALUE) {
						paramValue = ValidateRangeForNiNumericEdit(NInumFBgain[i], paramValue);
						NInumFBgain[i].Value = paramValue;
					}
				} // if visible
			}

			// enable.Tag
			max_indx = ToggleAxisEn.Count;
			for (i = 0; (i < max_indx); i++) {
				if (ToggleAxisEn[i].Visible == true) {
					// ref: ToggleSoftDock.Tag="Loop_FBA" or "loop_FB_#" contr_response can be "active" or "passive" only
					SendInternal(ToggleAxisEn[i].Tag.ToString(), CommandTypes.ResponseExpected, out contr_response, CommandAckTypes.AckExpected);
					// loop_FB_xx
					sep_pos = (contr_response.IndexOf(">") + 1);
					if ((sep_pos != 0)) {
						ToggleAxisEn[i].Checked = (contr_response.Substring(sep_pos, 4).ToLowerInvariant() == "acti");
					}
				} // if visible
			}
			// ref: ToggleSoftDock.Tag="Loop_FBD", contr_response can be "active" or "passive" only
			command = ToggleAuxDamping.Tag.ToString();
			SendInternal(command, CommandTypes.ResponseExpected, out contr_response, CommandAckTypes.AckExpected);
			sep_pos = contr_response.IndexOf(">") + 1;
			if (sep_pos > 0) {
				ToggleAuxDamping.Checked = (contr_response.Substring(sep_pos + 1, 4).ToLowerInvariant() == "acti");
			}

			// ref: ToggleSoftDock.Tag="horz" contr_response can be "hold" or "free" only
			SendInternal(ToggleSoftDock.Tag.ToString(), CommandTypes.ResponseExpected, out contr_response, CommandAckTypes.AckExpected);
			sep_pos = contr_response.IndexOf(">");
			if (sep_pos >= 0) { // contr_response can be "hold" or "free" only
				ToggleSoftDock.Checked = (contr_response.Substring(sep_pos + 1, 4).ToLowerInvariant() == "hold");
			}

			// ref: ToggleSoftDock.Tag="loop_ffa", contr_response can be "active" or "passive" only
			SendInternal(ToggleAllFF.Tag.ToString(), CommandTypes.ResponseExpected, out contr_response, CommandAckTypes.AckExpected);
			sep_pos = contr_response.IndexOf(">") + 1;
			if (sep_pos > 0) {
				ToggleAllFF.Checked = (contr_response.Substring(sep_pos, 4).ToLower() == "acti");
			}
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// For ELDAMP, these are proximity sensors - measuring distance to targets (between payload and frame)
		/// </summary>
		void Read_Position_Sensors() {
			string positionSensors;
			SendInternal("relm", CommandTypes.ResponseExpected, out positionSensors, CommandAckTypes.AckExpected);
			if(ExtractLoadDataFromScanLine(positionSensors))
				ShowPositionValues();
		}

		/**********************************************************************************************************************/
		private void NInumPulsePosSlope_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings) return;
			// if out of range, control does CoerceToRange - set in designer
			string controller_command = ("prup=" + ((NumericEdit)(sender)).Value.ToString());
			SendInternal(controller_command, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void NInumPulseNegSlope_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings) return;
			// if out of range, control does CoerceToRange - set in designer
			string controller_command = ("prdn=" + ((NumericEdit)(sender)).Value.ToString());
			SendInternal(controller_command, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		public bool ExcitationStatus = false;

		/**********************************************************************************************************************/
		//*** EXCITAION SWITCH TURNED ON OR OFF (USER OR PROGRAMATICALLY)
		private void ToggleExcitation_CheckedChanged(object sender, EventArgs e) {
			if (Test_in_progress)
				return;
			ExcitationStatus = false;
			string resp = string.Empty;

			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;

			CheckBox checkBoxCtrl = sender as CheckBox;

			if (checkBoxCtrl.Checked == true)
				checkBoxCtrl.BackgroundImage = Resources.SwitchOnYellow;
			else
				checkBoxCtrl.BackgroundImage = Resources.SwitchOffGreen;

			// avoid firing control event subroutine during initialization when values assigned
			//  cmd_sent = false;
			if ((checkBoxCtrl.Checked == false)) {
				SendInternal("excit>stop", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				// none (cancel)
				ContinuousMonitoringOffOn(false);
				ExcitationStatus = false;
			} else {
				if ((optExitation0.Checked == true)) {
					SendInternal("freq=0", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
					// enable noise excitation
				}

				SendInternal("excit>start", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				// enable excitation
				ExcitationStatus = true;
			}

			checkBoxCtrl.Checked = ExcitationStatus;
			if (Program.FormRTMonitoring != null)
				Program.FormRTMonitoring.ToggleExcitation.Checked = ExcitationStatus;
			// ContinuousMonitoringOffOn(Excitation_status);
			read_RT_diagnostic();
		}


		/**********************************************************************************************************************/
		void read_RT_diagnostic() {
			//  decodes 4 lines from the 'brif' command
			// Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI
			// Vrt um -7746 -7746 -7746  FF_is ON  X- Y-          tZ  Iso1 Iso2 Iso3 Iso4 Inp
			// Hrz um +6937 +6937 -6934 Excit=NONE   MotPWR   0%  PSI  0.6  0.6  0.5  0.5  0.7
			//                                  BncO=Zpos_mm       mA  8.0  8.0  8.0  8.0
			//  or when floating normally
			// Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: ACTIVE      ~~floating~~ OK
			// Vrt um +9    +9    +7     FF_is ON                 tZ  Iso1 Iso2 Iso3 Iso4 Inp
			// Hrz um -7    +14   -6    Excit=Pulse  MotPWR 100%  PSI 11.3 12.0 11.2 11.6 36.3
			//                                  BncO=GATZ          mA 74.1 75.6 78.9 77.5
			//  or when excitation is working
			// Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI
			// Vrt um -7746 -2323 -3333  FF_is ON  X- Y- Z- tX tY tZ    Rel_dB = +21.31
			// Hrz um +6937 +6937 -6933 Excit=Sine   MotPWR   0%        Rel_deg= +90.5
			//                                  BncO=Zpos_mm            Bnc1=p2Z_10V/mm
			// ELDAMP 20211229 ruturns
			// Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI
			// Vrt um -9656 -9293 -9882  FF_is ON  X- Y- Z- tX tY tZ  Iso1 Iso2 Iso3 Iso4 Inp
			// Hrz um -6300 -6608 +6706 Excit=NONE   MotPWR   0%  PSI  8.2 -1.6 -8.5 -8.1 -2.2
			//                                 BncO=GA_X          mA  1.0 42.3  8.0  8.0

			if (Test_in_progress)
				return;
			int ch = 0;
			bool tmp_GetSysData = false;
			int token_pos = 0;
			string brif_msg = string.Empty;
			string tmpDataStr = string.Empty;
			string tmp_str = string.Empty;
			int separator = 0;
			int str_len;
			bool excitation_is_working;
			string RCIcommand = "brif";
			string resp = string.Empty;

			if ((Program.ConnectionType == (int)Program.ConnType.ConnDEMO)) {
				if ((ExcitationStatus == true)) {
					brif_msg = create_test_brif(3);
				} else {
					brif_msg = create_test_brif(2);
				}
			} else {
				RCIcommand = "temp";// temperature
				double temp;
				SendInternal(RCIcommand, CommandTypes.ResponseExpected, out brif_msg, CommandAckTypes.AckExpected);
				if (double.TryParse(brif_msg, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out temp));
					ControllerTemperature = temp;
				DisplayControllerTemperature();
				RCIcommand = "brif";
				SendInternal(RCIcommand, CommandTypes.ResponseExpected, out brif_msg, CommandAckTypes.AckExpected);
			}

			if (brif_msg.Contains("meout") || String.IsNullOrEmpty(brif_msg)) {
				return;
			}

			token_pos = brif_msg.IndexOf("dB");
			if (token_pos != -1) {
				excitation_is_working = true;
				Frame_RT_Mag_Ph.Visible = true;
				tmpDataStr = brif_msg.Substring(token_pos + 4, 7);
				if (IsNumeric(tmpDataStr))
					lbl_Mag_dB.Text = tmpDataStr.Trim();
				token_pos = brif_msg.IndexOf("deg=");
				if (token_pos != -1) {
					tmpDataStr = brif_msg.Substring(token_pos + 4, 6);
					if (IsNumeric(tmpDataStr))
						lbl_Phase_diff.Text = tmpDataStr.Trim();
				}
				token_pos = brif_msg.IndexOf("Bnc0=");
				if (token_pos == -1)
					token_pos = brif_msg.IndexOf("BncO=");
				if (token_pos != -1) {
					separator = brif_msg.IndexOf("Bnc1=", token_pos + 5);
					if (separator != -1) {
						tmp_str = brif_msg.Substring(token_pos + 5, separator - token_pos - 5);
						lbl_BNC_0.Text = tmp_str.Trim();
						tmp_str = brif_msg.Substring(separator + 5);
						lbl_BNC_1.Text = tmp_str.Trim();
					}
				}
			} else {
				token_pos = brif_msg.IndexOf("Bnc0=");
				if (token_pos == -1)
					token_pos = brif_msg.IndexOf("BncO");
				if (token_pos != -1) {
					separator = brif_msg.IndexOf("mA", token_pos + 5);
					if (separator != -1)
						tmp_str = brif_msg.Substring(token_pos + 5, separator - token_pos - 5);
					lbl_BNC_0.Text = tmp_str.Trim();
				}
				excitation_is_working = false;
				Frame_RT_Mag_Ph.Visible = false;
			}
			tmp_GetSysData = Init_sys_data;
			Init_sys_data = true;
			ToggleExcitation.Checked = excitation_is_working;
			Init_sys_data = tmp_GetSysData;

			str_len = brif_msg.IndexOf("\r");
			separator = brif_msg.IndexOf("xes:"); // Axes:
			if (separator != -1) {
				if (str_len - separator - 4 > 0)
					tmpDataStr = brif_msg.Substring(separator + 4, str_len - separator - 4);
				tmp_str = tmpDataStr.ToLower();
				if (tmp_str.Contains("safetyoff")) {
					NIstatusBitIndicator3.OnText = "Velocity FB OFF by Safety";
					NIstatusBitIndicator3.Checked = true;
				} else {
					if (tmp_str.Contains("pass")) {
						NIstatusBitIndicator3.Checked = true;
						NIstatusBitIndicator3.OnText = "Velocity FB OFF";
					} else {
						if (tmp_str.Contains("acti")) {
							NIstatusBitIndicator3.OffText = "Velocity FB ON";
							NIstatusBitIndicator3.Checked = false;
						} else {
							NIstatusBitIndicator3.OnText = "Velocity FB unknown";
							NIstatusBitIndicator3.OffText = "Velocity FB unknown";
						}
					}
				}
				str_len = tmpDataStr.Length; //IK20211226 length was not updated when processing "TooLowInpPSI" in "brif" command
				token_pos = tmpDataStr.IndexOf( " ",2);  //find next space
				tmp_str = tmpDataStr.Substring(token_pos + 1, str_len - token_pos - 1);
				lbl_RTstatus.Text = tmp_str.Trim();
			}
			brif_msg = brif_msg.Substring(str_len + 2);  //cut first line
			str_len = brif_msg.IndexOf( '\r');
			/* IK20211229 the ShowPositionValues() function does it
			separator = brif_msg.IndexOf("um");
			tmpDataStr = brif_msg.Substring(separator + 3, 5);// Vert 1
			if (IsNumeric(tmpDataStr))
				lblProxSensor[3].Text = tmpDataStr.Trim();
			tmpDataStr = brif_msg.Substring(separator + 9, 5);// Vert 2
			if (IsNumeric(tmpDataStr))
				lblProxSensor[4].Text = tmpDataStr.Trim();
			tmpDataStr = brif_msg.Substring(separator + 15, 5);// Vert 3
			if (IsNumeric(tmpDataStr))
				lblProxSensor[5].Text = tmpDataStr.Trim();
			//IK20211229 the ShowPositionValues() function does it */
			separator = brif_msg.IndexOf("FF_is");
			tmp_str = brif_msg.Substring(separator + 6, 3);
			if (tmp_str.Contains("N"))
				statusBitIndicator7.Checked = false;
			else
				statusBitIndicator7.Checked = true;

			tmp_str = brif_msg.Substring(37, str_len - 37);

			if (tmp_str.Contains("X-"))
				StatusFBaxis0.Checked = true;
			else
				StatusFBaxis0.Checked = false;
			if (tmp_str.Contains("Y-"))
				StatusFBaxis1.Checked = true;
			else
				StatusFBaxis1.Checked = false;
			if (tmp_str.Contains("Z-"))
				StatusFBaxis2.Checked = true;
			else
				StatusFBaxis2.Checked = false;
			if (tmp_str.Contains("tX"))
				StatusFBaxis3.Checked = true;
			else
				StatusFBaxis3.Checked = false;
			if (tmp_str.Contains("tY"))
				StatusFBaxis4.Checked = true;
			else
				StatusFBaxis4.Checked = false;
			if (tmp_str.Contains("tZ"))
				StatusFBaxis5.Checked = true;
			else
				StatusFBaxis5.Checked = false;

			brif_msg = brif_msg.Substring(str_len + 2);

			/* IK20211229 the ShowPositionValues() function does it
			separator = brif_msg.IndexOf("um");
			tmpDataStr = brif_msg.Substring(separator + 3, 5);	// separate first Hprox
			if (IsNumeric(tmpDataStr))
				lblProxSensor[0].Text = tmpDataStr.Trim();
			tmpDataStr = brif_msg.Substring(separator + 9, 5);	// separate second Hprox
			if (IsNumeric(tmpDataStr))
				lblProxSensor[1].Text = tmpDataStr.Trim();
			tmpDataStr = brif_msg.Substring(separator + 15, 5);	// separate third Hprox
			if (IsNumeric(tmpDataStr))
				lblProxSensor[2].Text = tmpDataStr.Trim();
			// IK20211229 the ShowPositionValues() function does it*/

			token_pos = brif_msg.IndexOf("%",2);   // 'find percent char

			if (token_pos >= 0) {
				tmpDataStr = brif_msg.Substring(token_pos - 3, 4);   // power percent
				lbl_MotPower.Text = tmpDataStr;
			}

			if (!excitation_is_working) { // no excitation
				token_pos = brif_msg.ToUpperInvariant().IndexOf("PSI",3);   // 'PSI 50.2 53.3 40.3 42.7 75.9
				for (ch = 0; ch < lbl_IsoPressure.Count; ch++) {
					if (token_pos != 0) {// 'airless systems do not have this reading
						lbl_PSI.Visible = true;
						tmpDataStr = brif_msg.Substring(token_pos + 4 + ch * 5, 4);
						if (IsNumeric(tmpDataStr)) {
							lbl_IsoPressure[ch].Text = (tmpDataStr);
						}
					} else {// airless systems do not have this reading, hide labels
						lbl_PSI.Visible = false;
						lbl_IsoPressure[ch].Visible = false;
					}
				}
				token_pos = brif_msg.ToUpperInvariant().IndexOf("MA",2);   //mA 95.1 89.6 96.3 93.4
				for (ch = 0; ch < lblValveCurrent.Count; ch++) {
					if (token_pos != 0) { // airless systems do not have this reading
						lbl_mA.Visible = true;
						lblValveCurrent[ch].Visible = true;
						tmpDataStr = brif_msg.Substring(token_pos + 3 + ch * 5, 4);
						if (IsNumeric(tmpDataStr))
							lblValveCurrent[ch].Text = (tmpDataStr);
					} else {// airless systems do not have this reading, hide labels
						lbl_mA.Visible = false;
						lblValveCurrent[ch].Visible = false;
					}
				}
			} // end of excitation_is_working = false
			else { }  //'excitation_is_working = True ' there is frame on the top of these controls
		}

		/**********************************************************************************************************************/
		private string create_test_brif(int test) {
			string returnVal = string.Empty;
			if ((test == 1)) {
				returnVal =
				"Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI\r\n" +
				"Vrt um -7746 -7746 -7746  FF_is ON  X- Y-          tZ  Iso1 Iso2 Iso3 Iso4 Inp\r\n" +
				"Hrz um +6937 +6937 -6934 Excit=NONE   MotPWR   0%  PSI  0.6  0.6  0.5  0.5  0.7\r\n" +
				"                                 BncO=Zpos_mm       mA  8.0  8.0  8.0  8.0" + "\r\n";
			}

			if ((test == 2)) {
				returnVal =
				"Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: ACTIVE      ~~floating~~ OK\r\n" +
				"Vrt um +9    +9    +7     FF_is ON                 tZ  Iso1 Iso2 Iso3 Iso4 Inp\r\n" +
				"Hrz um -7    +14   -6    Excit=Pulse  MotPWR 100%  PSI 11.3 12.0 11.2 11.6 36.3\r\n" +
				"                                 BncO=GATZ          mA 74.1 75.6 78.9 77.5" + "\r\n";
			}

			if ((test == 3)) {
				returnVal =
				"Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI\r\n" +
				"Vrt um -46   -29   +71    FF_is ON  X- Y- Z- tX tY tZ    Rel_dB = +21.31\r\n" +
				"Hrz um +37   +6    -93   Excit=Sine   MotPWR   0%        Rel_deg= +90.5\r\n" +
				"                                 BncO=Zpos_mm            Bnc1=p2Z_10V/mm\r\n";
			}
			return returnVal;
		}

		/**********************************************************************************************************************/
		private void NInumExcitFreq_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings) return;

			TestFrequency = ((NumericEdit)(sender)).Value;
			string controller_command = ("freq=" + ((NumericEdit)(sender)).Value.ToString());
			SendInternal(controller_command, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void NInumExcitAmpl_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings) return;

			TestGain = ((NumericEdit)(sender)).Value;
			string controller_command = ("ampl=" + TestGain.ToString());
			SendInternal(controller_command, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		public bool p_cmd_as_PN = false; // some legacy firmware uses 'p' to print part number
		private void cmdPulse_Click(object sender, EventArgs e) {
			string controller_command;
			if (!p_cmd_as_PN)
				controller_command = "p";
			else
				controller_command = "puls";
			string response;
			SendInternal(controller_command, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			if (response.Contains("95-"))   //Then 'Bruker Insight AFP 95-43009-01r.At3dbg @26-Oct-16;compiled Oct 28 2016 13:48:19
				p_cmd_as_PN = true;
			else
				p_cmd_as_PN = false;
			Program.IsReadingControllerSettings = true;
			ExcitationStatus = false;
			ToggleExcitation.Checked = ExcitationStatus;
			Program.IsReadingControllerSettings = false;
		}

		double[] OneScanPositionValues = new double[MAX_Position_Sensors];
		/**********************************************************************************************************************/
		/// <summary>
		/// transfers position values to labels
		/// </summary>
		private void ShowPositionValues() {
			try {
				double Y;
				for (int ch = 0; (ch < (lblProxSensor.Count)); ch++) {
					//OneScanPositionValues[ch] = 1 + ch * 111;// test
					Y = OneScanPositionValues[ch];
						lblProxSensor[ch].Text = String.Format("{0:0000}", Y); //string.Format(Y, "");
				}
			} catch (Exception e) {
				MessageBox.Show(e.Message);
				return;
			}
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// extracts position data from a response on "relm" and fills OneScanPositionValues[] array
		/// </summary>
		/// <param name="data_line">for ELDAMP, it loks like: "-6306,-6622,+6712,-9684,-9309,-9906,*"</param>
		/// <returns></returns>
		bool ExtractLoadDataFromScanLine(string data_line) {
			// returns true if conversion OK or false if error
			string tmpDataStr;

			// process_data:
			var sensVal = data_line.Split(',');
			for (int ch = 0; (ch <= (sensVal.Count() - 1)); ch++) {
				tmpDataStr = sensVal[ch];
				if ((tmpDataStr.Length == 0)) {
					return false; //  means error
				} else {
					if (int.TryParse(tmpDataStr, out _)) {
						OneScanPositionValues[ch] = int.Parse(tmpDataStr);
					} else {
						if (tmpDataStr.Contains('*'))
							return true;    //  good
						else
							return false;   //  means error;
					}
				}
			}
			return true;    //  good
		}

		/**********************************************************************************************************************/
		private void cmdChooseExcitation_Click(object sender, EventArgs e) {
			if (frmDrivingOutput == null) {
				frmDrivingOutput = new frmDrivingOutput();
				frmDrivingOutput.Show();
			} else {
				frmDrivingOutput.Show();
				frmDrivingOutput.Focus();
			}
			// frmDrivingOutput.VisibleChanged += new EventHandler(frmDrivingOutput_VisibleChanged);
		}


		/**********************************************************************************************************************/
		// 181008 not used
		public bool CheckIfFormOpened(string name) {
			bool retVal = false;
			FormCollection fc = Application.OpenForms;
			foreach (Form frm in fc) {
				if (frm.Text.Contains(name)) {
					return retVal = true;
				}
			}
			return retVal;
		}

		/**********************************************************************************************************************/
		//IK181008 not used
		void frmDrivingOutput_VisibleChanged(object sender, EventArgs e) {
			// OutputChosen = frmDrivingOutput.OutputChosen;
			// Program.LTFtestOutputName = frmDrivingOutput.LTFtestOutputName;
		}

		/**********************************************************************************************************************/
		private void CmdChooseInputSignal_Click(object sender, EventArgs e) {
			SecondInputSelection = false;
			if (frmSensorInput == null) {
				frmSensorInput = new frmSensorInput();
				frmSensorInput.Show();
			} else {
				frmSensorInput.Show();
				frmSensorInput.Focus();
			}
			// frmSensorInput.VisibleChanged += new EventHandler(frmSensorInput_VisibleChanged);
		}
		private void Process_LTF_Data_Line_Handler(LoopTransferProgressChangedEventArgs eR) {
			// Save the received data into the arrays
			LTFfrequency[LTFlineCounter] = eR.LTFEntry.Frequency;
			LTFdBgain[LTFlineCounter] = eR.LTFEntry.Magnitude_dB;

			// cleaning phase difference, which might jump
			// because open or closed loop is wrapped between -180 and +180
			// example: open loop phase is still +170, but closed loop phase has jumped to -176
/*
			LTFphaseDeg[LTFlineCounter] = phaseDiff;

			if (LTFlineCounter == 1) {
				LTFfrequency[0] = LTFfrequency[1];
				LTFdBgain[0] = LTFdBgain[1];
				gainOpenData[0] = gainOpenData[1];
				phaseOpenData[0] = phaseOpenData[1];
				gainClosedData[0] = gainClosedData[1];
				phaseClosedData[0] = phaseClosedData[1];
				LTFphaseDeg[0] = LTFphaseDeg[1];
			}

			// Update the Progres-Bar Percent Done
			ProgressTank.Value = LTFlineCounter * 100 / (LTFarrayMinimum - 1);
			var tempInt = (int)ProgressTank.Value;

			if (tempInt > 100) tempInt = 100;

			ProgressTank.Caption = $"Progress {tempInt:#0} %";

			// Plot the new points onto the graphs
			Update_Performance_Plots();

			var opTime = Tools.RefTimer;
			Debug.WriteLine("line " + LTFlineCounter + "; Freq=" + LTFfrequency[LTFlineCounter]
				+ "; T_abs=" + opTime.TotalMilliseconds + "; dT=" + opTime.Subtract(thisTime).TotalMilliseconds);
			thisTime = opTime;
*/
			LTFlineCounter += 1;
		}

		System.Threading.Thread TFTestThread;
		bool IsTFTestStarted = false;
		/**********************************************************************************************************************/
		private void ButStartStopLTF_CheckedChanged(object sender, EventArgs e) {
			if (!ButStartStopLTF.Checked)
				IsTFTestStarted = false;
			if (ButtonAutoScale.Text.Equals("Auto"))
				ScatterGraphMag.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;

			if (String.IsNullOrEmpty(Program.LTFtestOutputName) || String.IsNullOrEmpty(Program.LTFtestInputName)) {
				if (!ButStartStopLTF.Checked)
					return;
				else {
					MessageBox.Show("Please choose Output and Input first", "Parameters are not defined", MessageBoxButtons.OK);
					ButStartStopLTF.Checked = false;
					return;
				}
			}
			ContinuousMonitoringOffOn(false);
			// Loop Transfer Function
			if (ButStartStopLTF.Checked) {
				Test_in_progress = true;
				create_LTF_command();
				string controller_command = txtManualCMD.Text;
				if (txtManualCMD.Text.Contains("mtst")) {
					MessageBox.Show("please choose ether \'plant_TF\' or \'Open Loop TF\'");
					ButStartStopLTF.Checked = false;
					return;
				}

				Update_Previous_Plot_Data();    // move current data to previous in arrays
				Clear_Performance_Data_Arrays();

				if (!ShowPrevPlotControl.Checked) {
					// User control is set to NOT show previous plots, turn off 'previous' plots
					ScatterGraphMag.Plots[PrevLTF].Visible = false;
					ScatterGraphPhase.Plots[PrevLTF].Visible = false;
				}

				// save the state of the cursors and then turn off
				// not used if (ScatterGraphMag.Cursors[0].Visible) { MagGraphVisible = true; }
				// not used if (ScatterGraphPhase.Cursors[0].Visible) { PhaseGraphVisible = true; }
				ScatterGraphPhase.Cursors[0].Visible = false;
				ScatterGraphMag.Cursors[0].Visible = false;
				LTFIsRunning = true;    //runs over a minute and interferes with stuff
				LTFStartTime = DateTime.Now;  // value doesn't matter: set to Now

				// go get the data
				LTFlineCounter = 0;
				Tools.RefTimer = TimeSpan.MinValue;  // value irrelevant

				//-!- stateButtonPOSmonitoring.Enabled = false;   //no monitoring

				int ret = 0;
				string result = string.Empty;
				//ret = DoTest(controller_command);
				/**/
				TFTestThread = new System.Threading.Thread(() =>
				{
					ret = DoTest(controller_command);
					if (ret < 0) {
						SendInternal("oltf stop", CommandTypes.ResponseExpected, out result, CommandAckTypes.AckExpected);
						IsTFTestStarted = false;
						Test_in_progress = false;
						ButStartStopLTF.Checked = false;
						TFTestThread.Abort();
					}
				});
				IsTFTestStarted = true;
				TFTestThread.Start();

				//  Program.ConnectedController.LoopTransferFunctionAsync(axs, Process_LTF_Data_Line_Handler, LoopTransferCompleted);
			} else {
				IsTFTestStarted = false;
				if (txtManualCMD.Text.IndexOf("mtst") != -1)
					return;
				string result = string.Empty;
				if (Test_in_progress) {

					SendInternal("oltf stop", CommandTypes.ResponseExpected, out result, CommandAckTypes.AckExpected);
					//if (Program.formTerminal != null) Program.formTerminal.Delay_sec(0.3f);
				}

				// User selected to stop test
				if (! LTFIsRunning) return;

				// cancel the operation
				//  Program.ConnectedController.CancelLoopTransferFunction();
				ButStartStopLTF.Checked = false;
				LTFIsRunning = false;
				ProgressTank.Caption = "! USER STOPPED !";
				//                    FindUnityGainPhase(Array_Line_Counter);
				Test_in_progress = false;
				SendInternal("freq=" + TestFrequency, CommandTypes.ResponseExpected, out result, CommandAckTypes.AckExpected);
				TFTestThread.Abort();
			}
		}

		/**********************************************************************************************************************/
		//*** Process Each Data Set
		// Venture Comment: delegate function called for each line of Loop transfer. we get here for each line of data
		// NOTES:   * Array_Line_Counter is set to ZERO the first time we come in here if are reading data from the controller.
		// * For some reason Array_Line_Counter is set to ONE the first time we come in here if we are reading from a file. Why do this?
		// * When done...Array_Line_Counter is ONE-MORE then the number of data-points we have. EX: If ZERO through 112 have valid data, Array_Line_Counter = 113
		// ---
		// * The arrays are FILLED from ZERO through ###
		// * But the FREQUENCY-VALUE of the itmes in the first positions will PLOT at the end/RIGHT of the graph
		// * ... the FREQUENCY-VALUE of the items nearer the END of the array will PLOT at the beginning/LEFT of the graph
		// ---
		// * The arrays are all of a fixed (constant) size. But elements above (Array_Line_Counter) are not value/useful-data
		// *
		string ASCIIDataStr = "";
		double percent_divider = 120;
		private int DoTest(string ScanCmd) {
			int retVal = 0;
			if (IsTFTestStarted) {
				string str_Frequency = string.Empty;
				string str_gain = string.Empty;
				string str_coherence = string.Empty;
				string str_phase = string.Empty;
				double Frequeny = 0;
				double gain = 0;
				double coherence = 0;
				double Phase = 0;
				string temp_str = string.Empty;
				string scan_buf = string.Empty;
				double WaitTime = 0;
				double WaitTimeout = 0;
				double tryParseVal;
				cmd_sent = false; // global, to signal GetLTFdata() that LTF has been started

				if (ToggleRTmonitoring.Checked) {
					if (ToggleRTmonitoring.InvokeRequired)
						ToggleRTmonitoring.BeginInvoke((MethodInvoker)delegate { ToggleRTmonitoring.Checked = false; });
					else { ToggleRTmonitoring.Checked = false; }
				}
				if (NInumLTFaveraging.InvokeRequired)
					NInumLTFaveraging.BeginInvoke((MethodInvoker)delegate { WaitTimeout = 60 * Convert.ToDouble(NInumLTFaveraging.Value); });
				else { WaitTimeout = 60 * Convert.ToDouble(NInumLTFaveraging.Value); }

				if (WaitTimeout < 60)
					WaitTimeout = 60;

				Test_in_progress = true;
				LTFlineCounter = 0;
				if (lstResponse.InvokeRequired)
					lstResponse.BeginInvoke((MethodInvoker)delegate { lstResponse.Items.Clear(); });
				else { lstResponse.Items.Clear(); }

				Buffer = "";
				LTFStartTime = DateTime.Now;
				if (textBoxTestTime.InvokeRequired)
					textBoxTestTime.BeginInvoke((MethodInvoker)delegate { textBoxTestTime.Text = "000"; });
				else { textBoxTestTime.Text = "000"; }

another_line:
				Tools.msSleep(100);
				try {
					if (!IsTFTestStarted) return retVal; // early exit if IsTFTestStarted changed to false

					scan_buf = GetLTFdata(ScanCmd, WaitTimeout);
					ASCIIDataStr = scan_buf.ToUpperInvariant(); // all in upper case to catch tokens

					// check general conditions first, if nothing - decode LTF line
					if (ASCIIDataStr.Contains("USERSTOP")) return -1;// return value of -1 means user stop
					if (ASCIIDataStr.Contains("OK") ||	// capture ">~OK" or
						ASCIIDataStr.Contains("CMD")) {	// "Doing CMD" - print it to lstResponse on Communication tab
						temp_str = scan_buf;
						if (lstResponse.InvokeRequired)
							lstResponse.BeginInvoke((MethodInvoker)delegate { lstResponse.Items.Add(temp_str); });
						else { lstResponse.Items.Add(temp_str); }
						goto another_line;
					}

					if (ASCIIDataStr.Contains('*') || // end of LTF
						ASCIIDataStr.Length == 0 ) {
						//MessageBox.Show("No communication over serial interface. Check connections");
						goto show_plot;
					}

					if (ASCIIDataStr == "TIMEOUT") {
						if (lstResponse.InvokeRequired)
							lstResponse.BeginInvoke((MethodInvoker)delegate {
								lstResponse.Items.Add("TImeout");
								MessageBox.Show("No communication over serial interface. Check connections");
							});
						else {
							lstResponse.Items.Add("TImeout");
							MessageBox.Show("No communication over serial interface. Check connections");
						}
						Test_in_progress = false;
						retVal = 0;
						return retVal;
					}

					if (ASCIIDataStr.Contains('#') == false) goto another_line; // error, might not be LTF line, it must contain "#"

					// everything looks good so far, add string to list and try to decode
					temp_str = LTFlineCounter.ToString("00#") + "->" + ASCIIDataStr;
					if (LabelCurrentLTFline.InvokeRequired)
						LabelCurrentLTFline.BeginInvoke((MethodInvoker)delegate { LabelCurrentLTFline.Text = temp_str; });
					else { LabelCurrentLTFline.Text = temp_str; }

					if (lstResponse.InvokeRequired)
						lstResponse.BeginInvoke((MethodInvoker)delegate { lstResponse.Items.Add(temp_str); });
					else { lstResponse.Items.Add(temp_str); }

					if (ASCIIDataStr.Length < 10) goto another_line; // too short, might not be LTF line even if it has '#'
					// start decoding
					temp_str = ASCIIDataStr;
					//if (Array_Line_Counter == 30) // test trap
					//	indx = 0;
					var LTFdata = temp_str.Split(',');
					var LTFdata_Count = LTFdata.Count();

					Frequeny = 0;
					gain = 0;
					Phase = 0;
					coherence = 0;
					if (LTFdata_Count >= 4) {
						str_coherence = LTFdata[3];
						if (double.TryParse(str_coherence, out tryParseVal))
							coherence = tryParseVal;
					}
					if (LTFdata_Count >= 3) {
						str_Frequency = LTFdata[0];
						if (double.TryParse(str_Frequency, out tryParseVal)) Frequeny = tryParseVal;
						str_gain = LTFdata[1];
						if (double.TryParse(str_gain, out tryParseVal)) gain = tryParseVal;
						str_phase = LTFdata[2];
						if (double.TryParse(str_phase, out tryParseVal)) Phase = tryParseVal;
					}
					LTFfrequency[LTFlineCounter] = Frequeny;
					LTFdBgain[LTFlineCounter] = gain;
					LTFphaseDeg[LTFlineCounter] = Phase;
					LTFcoherence[LTFlineCounter] = coherence * 100;
					LTFnegPhase[LTFlineCounter] = Make180phaseShift(Phase);

					// line decoded, arrays updated - now update controls on the form
					double tankValue = LTFlineCounter / (percent_divider + 1);
					updateProgress(tankValue);
					temp_str = (DateTime.Now - LTFStartTime).TotalSeconds.ToString("#00.0");
					if (textBoxTestTime.InvokeRequired)
						textBoxTestTime.BeginInvoke((MethodInvoker)delegate { textBoxTestTime.Text = temp_str; });
					else { textBoxTestTime.Text = temp_str; }
					Update_Performance_Plots();

					// IK20211230 NI Measurement Studio control does not give exception if caption is updated from worker thread !?
					temp_str = "Freq = " + Frequeny.ToString("0.0") + ";  Gain = " + gain.ToString("0.0");
					if (ScatterGraphMag.InvokeRequired)
						ScatterGraphMag.BeginInvoke((MethodInvoker)delegate { ScatterGraphMag.Caption = temp_str; });
					else
						ScatterGraphMag.Caption = temp_str;

					temp_str = "Freq = " + Frequeny.ToString("0.0") + ";  Phase = " + Phase.ToString("0.0");
					if (ScatterGraphPhase.InvokeRequired)
						ScatterGraphPhase.BeginInvoke((MethodInvoker)delegate { ScatterGraphPhase.Caption = temp_str; });
					else
						ScatterGraphPhase.Caption = temp_str;
					LTFlineCounter += 1;
				} catch (Exception ex) {
					return retVal;
				}
				goto another_line;

show_plot:
				percent_divider = LTFlineCounter;
				Test_in_progress = false;
				if (ButStartStopLTF.InvokeRequired)
					ButStartStopLTF.BeginInvoke((MethodInvoker)delegate { ButStartStopLTF.Checked = false; });
				else { ButStartStopLTF.Checked = false; }
				if (ProgressTank.InvokeRequired) {
					ProgressTank.BeginInvoke((MethodInvoker)delegate
					{
						ProgressTank.Value = 100;
						ProgressTank.Caption = "! DONE !";
					});
				} else {
					ProgressTank.Value = 100;
					ProgressTank.Caption = "! DONE !";
				}
				retVal = LTFlineCounter;
				FindUnityGainPhase(LTFlineCounter);
			}
			return retVal;
		}

		/**********************************************************************************************************************/
		private void updateProgress(double percent) {
			if (ProgressTank.InvokeRequired) {
				ProgressTank.BeginInvoke((MethodInvoker)delegate
				{
					ProgressTank.Value = (percent * 100) % 100;
					ProgressTank.Refresh();
					ProgressTank.Update();
					ProgressTank.Caption = "Progress " + (ProgressTank.Value / 100).ToString("#0%");
				});
			}
		}

		/**********************************************************************************************************************/
		double DefaultLTFtimeOut = 20;
		bool cmd_sent = false;// global, set to 'true' in DoTest() to signal GetLTFdata() that LTF has been started
		public string GetLTFdata(string message, double Wait_Time = -1) {
			string retVal = string.Empty;
			double WaitingTime = 0;
			string SendString = string.Empty;
			int SendStringLen = 0;
			string resp = "";

			if (Wait_Time == -1)
				WaitingTime = DefaultLTFtimeOut;
			else
				WaitingTime = Wait_Time;
			if (WaitingTime == 0)
				WaitingTime = DefaultLTFtimeOut;
//send_cmd:
			if (!IsTFTestStarted)
				return retVal;
			if (message != "" && !cmd_sent) {
				SendString = message;
				SendString = SendString + "\r\n";
				SendStringLen = SendString.Length;
				SendInternal(message, CommandTypes.NoResponseExpected, out resp, CommandAckTypes.NoAckExpected);
				cmd_sent = true;
			} else {
				SendStringLen = 0;
			}
			try {
get_response:
				retVal = Device.ReadLineWithTimeout(TimeSpan.FromSeconds(Wait_Time));
				//print command and response in terminal
				if (Program.formTerminal != null) {
					if (Program.formTerminal.InvokeRequired)
						Program.formTerminal.Invoke((MethodInvoker)delegate
						{
							Program.formTerminal.MainForm_SendCallBack(message, retVal);
						});
					else { Program.formTerminal.MainForm_SendCallBack(message, retVal); }
				}
				if (retVal.Contains("*"))
					return retVal;
				if (!String.IsNullOrEmpty(retVal)) {
					if (retVal.Equals(message))
						goto get_response;
				}
				if (!String.IsNullOrEmpty(retVal) && retVal.Length > 2) {
					int pos = retVal.IndexOf("\r\n");
					if (pos != -1 && pos != 0)
						retVal = retVal.Substring(0, pos);
					pos = retVal.IndexOf("\r\n");
					if (pos != -1 && pos != 0)
						retVal = retVal.Substring(0, pos);

					//if (retVal.Substring(retVal.Length - 2) == "\r\n")
					//    retVal = retVal.Substring(0, retVal.Length - 2);
					//if (!String.IsNullOrEmpty(retVal) && retVal.Length > 2)
					//{
					//    if (retVal.Substring(0, 2) == "\r\n")
					//        retVal = retVal.Substring(3);
					//}
					//else
					//    goto get_response;
				} else
					goto get_response;
			} catch (Exception ex) {
				throw ex;
			}
			return retVal;
		}


		/**********************************************************************************************************************/
		public string LTFtest2ndInputName = string.Empty;
		string create_LTF_command() {
			string TestCMD = string.Empty;
			if ((optTestInput0.Checked == true)) {
				// mot-sen test
				TestCMD = optTestInput0.Tag.ToString();
				//FrameLTF.Visible = false;
				//FrameMeasureAtFreq.Visible = true;
			}

			if ((optTestInput1.Checked == true)) {
				// OLTF test
				TestCMD = optTestInput1.Tag.ToString();
				//FrameMeasureAtFreq.Visible = false;
				//FrameLTF.Visible = true;
			}

			if ((optTestInput2.Checked == true)) {
				// plant TF test
				TestCMD = optTestInput2.Tag.ToString();
				//FrameMeasureAtFreq.Visible = false;
				//FrameLTF.Visible = true;
			}

			string create_LTF_command = TestCMD;
			// IsolatorNumber & IsolatorAxis 'CStr(cwIsolatorNumber.Value) & IsolatorAxis
			LTF_command = (create_LTF_command
			+ (Program.LTFtestOutputName + (" " + Program.LTFtestInputName)));
			if (LTFadvanced) {
				LTF_command += " " + LTFtest2ndInputName;
			}
			if (Convert.ToInt32(NInumLTFaveraging.Value) != 1)
				LTF_command += " " + NInumLTFaveraging.Value; //cwLTF_time_scale.Text;

			if (this.txtManualCMD.InvokeRequired) {
				this.BeginInvoke((MethodInvoker)delegate { this.txtManualCMD.Text = LTF_command; });
			} else {
				this.txtManualCMD.Text = LTF_command;
			}
			return LTF_command;
			// Check_Selected_Piezo_Geo();
		}

		delegate void SetTextCallback(string text);

		/**********************************************************************************************************************/
		private void Chk_EnDisPOSTtest0_CheckedChanged(object sender, EventArgs e) {
			// // "post>enab";">disa"  enables/disables Power On Self Test (POST)
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;

			// avoid firing control event subroutine during initialization when values assigned
			//  cmd_sent = false;
			string retVal = string.Empty;
			if (((CheckBox)sender).Checked == true) {
				// response = Analyzer.GetSend("post>enab", true);
				SendInternal("post>enab", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
				// enable
			} else {
				//  response = Analyzer.GetSend("post>disa", true);
				SendInternal("post>disa", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
				// disable
			}
		}

		/**********************************************************************************************************************/
		private void cmdSaveParamsIntoFLASH_Click(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;
			string retVal = string.Empty;
			SendInternal("save", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void CmdRestorefromFlash_Click(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;
			string retVal = string.Empty;
			SendInternal("rsto", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}



		/**********************************************************************************************************************/
//		bool ProdTestStatus = false;
		int M3v_Diagnostic_running = 0;
		double AlarmWord;
		/**********************************************************************************************************************/
		public long Read_Alarm_Status() {
			long functionReturnValue = 0;

			// * RCI command: "exst"
			// * Emits extended status in ascii-hex, to stdout.
			// * Output: 11111111,22222222,33333333,4
			// *      where   11111111 is 32-bit M3v.Cmd_Word
			// *              22222222 is 32-bit M3v.AlarmWord
			// *              33333333 is 32-bit Prod_Test_BITS
			// *              4        is M3v.Diagnostic_running (non-zero when test selected)
			// updates M3v_Diagnostic_running =
			//     NO_TESTS                   = 0 // no test
			//    ,LOGICAL_MotSens_TEST_Table = 1 // motor-sensor test: inject into one LOGICAL output control_signal[], see InGeo[] or InProx[],  PTR_FLOAT
			//    ,DAC_TO_ADC_loopback_TEST   = 2 // LOOP-BACK DB-37 SOCKETS REQUIRED! DAC-to-ADC loop back test, index the same, i.e. drive DAC[0], look at ADC[0], PTR_INT16
			//    ,PHYSICAL_PAIR_TEST         = 3 // DAC[d_index] to ADC[a_index] index to different index, i.e. drive DAC[6], look at ADC[15], PTR_INT16
			//    ,MotorSensorTEST_List       = 4 // for "test pair" command, output as a list
			//    ,EXCITATION_TO_INT16 = 5

			// * RCI command: "exst!" sets M3v.measurement_done = TRUE
			// * RCI command: "exst=0x12345678"   sets M3v.AlarmWord = 0x12345678
			string alarm_status = null;
			int separator = 0;
			string tmpDataStr = null;
			string binFormat = string.Empty;
			double tryParVal = 0;
			// ==============================================================================================
			// ===============  get buttons states  =========================================================
			//If (cwBut_RT_Monitoring.Value = True) Then
			Init_sys_data = true;

			// ==============================================================================================
			// ===============  Read extended status word  ==================================================
			functionReturnValue = 0;
			tmpDataStr = "exst";
			// cmd_sent = false;
			//  waitresponse = true;
			// alarm_status = Analyzer.GetSend(tmpDataStr, true);
			SendInternal(tmpDataStr, CommandTypes.ResponseExpected, out alarm_status, CommandAckTypes.AckExpected);
			//"0x120F0008,0x00000000,0x00000000,0>~Doing CMD"
			//separator = Strings.InStr(alarm_status, ",") - 1;
			separator = alarm_status.IndexOf(",") - 1;
			if ((separator > 0)) {
				// tmpDataStr = Strings.Left(alarm_status, separator);
				tmpDataStr = "&H" + alarm_status.Substring(0, separator);//-!- IK is it simply '0x120F0008'? Why "&H"
				if (double.TryParse(tmpDataStr, out tryParVal)) {

					AlarmWord = Val(tmpDataStr);
					self_test_word = AlarmWord;
					binFormat = BIN(Convert.ToInt32(self_test_word));
					self_test_word = self_test_word / 256;
					binFormat = BIN(Convert.ToInt32(self_test_word)) + binFormat;
					self_test_word = self_test_word / 256;
					binFormat = BIN(Convert.ToInt32(self_test_word)) + binFormat;
					self_test_word = self_test_word / 256;
					binFormat = BIN(Convert.ToInt32(self_test_word)) + binFormat;
					txtStatus.Text = "AlarmWord = " + tmpDataStr + " " + binFormat;
				}
				tmpDataStr = alarm_status.Substring(separator + 4);

				//// 32-bit M3v.Cmd_Word, "0x120F0008"
				//// tmpDataStr = "&H" + Strings.Mid(tmpDataStr, 3);
				////      tmpDataStr = "&H" + tmpDataStr.Substring(0, 3);
				//tmpDataStr = tmpDataStr.Replace("0x", "");
				//// 32-bit M3v.Cmd_Word, "120F0008"
				//// CAN USE IsNumeric, because 'tmpDataStr' is integer
				//if ((IsNumeric(tmpDataStr)))
				//{
				//    AlarmWord = Val(tmpDataStr);
				//    self_test_word = AlarmWord;
				//    binFormat = BIN(Convert.ToInt32(self_test_word));
				//    self_test_word = self_test_word / 256;
				//}
				//tmpDataStr = alarm_status.Substring(0, separator + 4);// Strings.Mid(alarm_status, separator + 4);
				//                                                      // remove "0x" and the piece of string from which we've converted information
			} else {
				functionReturnValue = functionReturnValue - 1;
				// adding -1 means error
			}

			alarm_status = tmpDataStr;
			separator = alarm_status.IndexOf(",") - 1; //Strings.InStr(alarm_status, ",") - 1;
			if ((separator > 0)) {
				tmpDataStr = "&H" + alarm_status.Substring(0, separator); //Strings.Left(alarm_status, separator);
				if (double.TryParse(tmpDataStr, out tryParVal)) {
					self_test_word = tryParVal;
					tmpDataStr = alarm_status.Substring(separator + 2);
				} else
					self_test_word = 0;

				//    AlarmWord = Convert.ToDouble(Int64.Parse(alarm_status.Split(',')[0].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber));
				////AlarmWord = Convert.ToDouble(double.Parse(alarm_status.Split(',')[0].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber));
				//ulong parsedAlarmWord = ulong.Parse(alarm_status.Split(',')[0].Replace("0x", ""), System.Globalization.NumberStyles.HexNumber);
				//tmpDataStr = parsedAlarmWord.ToString();
				//// 32-bit M3v.AlarmWord
				//// CAN USE IsNumeric, because 'tmpDataStr' is integer
				//if ((IsNumeric(tmpDataStr)))
				//{
				//    //AlarmWord = Val(tmpDataStr);
				//    //As Long
				//    // self_test_word = parsed;
				//    //  uint number_self_test_word = UInt32.Parse(self_test_word.ToString());// Convert.ToUInt32(self_test_word);
				//    //binFormat = Convert.ToString(parsedAlarmWord & 0xff, 2); //BIN(self_test_word & 0xff);
				//    //number_self_test_word = parsedAlarmWord / 256;
				//    //binFormat = Convert.ToString(parsedAlarmWord & 0xff, 2) + binFormat;//BIN(self_test_word & 0xff) + binFormat;
				//    //parsedAlarmWord = parsedAlarmWord / 256;
				//    //binFormat = Convert.ToString(parsedAlarmWord & 0xff, 2) + binFormat; //BIN(self_test_word & 0xff) + binFormat;
				//    //parsedAlarmWord = parsedAlarmWord / 256;
				//    //binFormat = Convert.ToString(parsedAlarmWord & 0xff, 2) + binFormat; // BIN(self_test_word & 0xff) + binFormat;
				//    byte[] buf = HexStringToByteArray(alarm_status.Split(',')[0].Replace("0x", ""));
				//    // binFormat = new BitArray(new double { BitConverter.ToDouble(buf, 0) }).ToString();
				//    binFormat = DisplayBitArray(new BitArray(new int[] { BitConverter.ToInt32(buf, 0) })); //.ToString();

				//    txtStatus.Text = "AlarmWord = " + String.Format("#{0:X}", parsedAlarmWord) + " " + binFormat;

				//    // Show_Alarm_flags();
				//    //Show_Alarm_flags(buf);
				//    //if (AlarmWord == 0)
				//    //{
				//    //    Chk_IndicatorAlarm.Checked = false;
				//    //}
				//    //else
				//    //{
				//    //    Chk_IndicatorAlarm.Checked = true;
				//    //}
				//}
				//tmpDataStr = alarm_status.Replace("0x", ""); //Strings.Mid(alarm_status, separator + 4);
				//                                             // remove "0x" and the piece of string from which we've converted information
			} else {
				functionReturnValue = functionReturnValue - 1;
				// adding -1 means error
			}
//read_err:
			Init_sys_data = false;
			return functionReturnValue;
		}

		/// <summary>
		/// Display bits as 0s and 1s.
		/// </summary>
		private string DisplayBitArray(BitArray bitArray) {
			string resultBitArray = "";
			for (int i = 0; i < bitArray.Count; i++) {
				bool bit = bitArray.Get(i);
				resultBitArray = resultBitArray + " " + Convert.ToInt16(bit);
				// Console.Write(bit ? 1 : 0);
			}
			return resultBitArray;
		}

		/**********************************************************************************************************************/
		void Get_axis_power() { // for STACIS only
			//  ==============================================================================================
			//  =====================  Acting gains?  ========================================================
			//                channel name Z1 Z2 Z3 Z4  X1  X2 X3 X4  Y1 Y2 Y3 Y4
			//  returns percent of power "100 100 98 88 100 98 100 25 99 100 100 100"
			string alarm_status;
			int separator;
			double pwr;
			string tmpDataStr;
			// alarm_status = Analyzer.GetSend("cpwr", true);
			SendInternal("cpwr", CommandTypes.ResponseExpected, out alarm_status, CommandAckTypes.AckExpected);
			for (int ch = 0; (ch < LblChPower.Count); ch++) {
				separator = ((alarm_status.IndexOf(" ") + 1) - 1);
				if ((separator > 0)) {
					tmpDataStr = alarm_status.Substring(0, separator);
					//  percent of power
					alarm_status = alarm_status.Substring((separator + 1));
					if (IsNumeric(tmpDataStr)) {
						pwr = double.Parse(tmpDataStr);
						// As Long
						LblChPower[ch].Text = pwr.ToString();
						// c(c) = pwr;
						if ((pwr == 100)) {
							LblChPower[ch].BackColor = Color.LightGreen;
							// light green
						} else {
							LblChPower[ch].BackColor = Color.Yellow;
							// yellow
						}

					} else {
						LblChPower[ch].Text = "n/a";
						LblChPower[ch].BackColor = Color.Red;
						// light red
					}
				} else {
					//  c = (LblChPower.UBound + 1);
					// 12 'exit for
				}
			}
		}

		/**********************************************************************************************************************/
		public static bool ChkBitState(ref byte[] bytes, int index, int bitIndexTobeChecked) {
			bool carryFlag = (bytes[index] << bitIndexTobeChecked & 0x80) > 0;
			return carryFlag;
		}

		/**********************************************************************************************************************/
		public static byte[] HexStringToByteArray(string s) {
			if (s == "CommsError" || s == "Comms Error")
				return null;
			if (s.Contains("NACK"))
				return new byte[] { 0x15 };
			if (s.Contains("Comms Error") || s.Contains("CommsError")) {
				return new byte[0];
			}
			int length;

			s = new StringBuilder(s).Replace("\n", String.Empty).ToString();
			s = new StringBuilder(s).Replace(" ", "").ToString();
			// s = s.Replace("\n", String.Empty);

			if (s.Length % 2 == 0) {
				length = s.Length / 2;
			} else {
				length = (s.Length + 1) / 2;
			}

			var buffer = new byte[length];
			//try
			//{
			for (int i = 0; i < s.Length; i += 2) {
				int subStrLength = 2;
				if (s.Length - i < 2) {
					subStrLength = s.Length - i;
				}

				buffer[i / 2] = Convert.ToByte(s.Substring(i, subStrLength), 16);
			}
			//}
			//catch(Exception e)
			//{
			//    buffer = new byte[1];
			//}

			return buffer;
		}

		/**********************************************************************************************************************/
		void Show_Alarm_flags(byte[] buf) {
		//	int WordBit;
			int tmp;
			BitArray bitArrayStatus = new BitArray(buf);
			for (tmp = 0; (tmp <= 23); tmp++) {
				CheckAlarmBit[tmp].Checked = bitArrayStatus.Get(tmp);
			}
		}

		/**********************************************************************************************************************/
		double Make180phaseShift(double phaseIN) {
			double phaseOut;
			phaseOut = (phaseIN + 180);
			if ((phaseOut > 180)) {
				phaseOut = (phaseOut - 360);
			} else {
				// <=0
				//         phaseOut = phaseIN - 180
				//         If phaseOut < -180 Then phaseOut = phaseOut + 180
			}
			return phaseOut;
		}

		/**********************************************************************************************************************/
		public static Double Val(string value) {
			String result = String.Empty;
			foreach (char c in value) {
				if (Char.IsNumber(c) || (c.Equals('.') && result.Count(x => x.Equals('.')) == 0))
					result += c;
				else if (!c.Equals(' '))
					return String.IsNullOrEmpty(result) ? 0 : Convert.ToDouble(result);
			}
			return String.IsNullOrEmpty(result) ? 0 : Convert.ToDouble(result);
		}

		/**********************************************************************************************************************/
		private void CheckAlarmBit_CheckedChanged(object sender, EventArgs e) {
			CheckBox checkBoxCtrl = (CheckBox)sender;
			if (checkBoxCtrl.Checked)
				checkBoxCtrl.BackgroundImage = Resources.checkmark_small_gray;
			else
				checkBoxCtrl.BackgroundImage = Resources.orange_gray_circle;
		}


		/**********************************************************************************************************************/
		private void cmdSaveParameters_Click(object sender, EventArgs e) {
			cmdSaveParameters.Enabled = false;
			Get_Sys_Params();
			cmdSaveParameters.Enabled = true;
		}

		/**********************************************************************************************************************/
		public static int ParamLinesCount = 0;
		public static int ParamCharCount = 0;

		private void Get_Sys_Params() {
			if (Program.formTerminal != null) {
				Program.formTerminal.Hide();
				Program.formTerminal.OnQuitMenuCommand();
			}
			sbAction.Checked = false;
			sbAction.Visible = true;
			ListShadow.Visible = true;
			//  ListParamsErrors.Visible = false;
			ListShadow.Items.Clear();
			ListParamsErrors.Items.Clear();
			ParamLinesCount = 0;
			ParamCharCount = 0;
			ToggleRTmonitoring.Checked = false;  // disable RT

			string TestCMD = "expo";
			if (_Opt_ALL_params_or_Essential_1.Checked) {
				if (ChkVerboseParams.Checked)
					TestCMD = TestCMD + " esse verb";
				else
					TestCMD = TestCMD + " esse";
			} else {
				if (ChkVerboseParams.Checked)
					TestCMD = TestCMD + " verb";
				else
					TestCMD = TestCMD + "";
			}
			ListShadow.Enabled = false;

			var watchParam = System.Diagnostics.Stopwatch.StartNew();
			SendInternal(TestCMD + Environment.NewLine, CommandTypes.NoResponseExpected, out Buffer, CommandAckTypes.NoAckExpected);

			Test_in_progress = true;
			string stringRead = string.Empty;
			bool IsSentCmd = false;

			do {
				stringRead = Device.ReadLineWithTimeout(new TimeSpan());
				if (stringRead.Contains(TestCMD))
					IsSentCmd = true;
				if (stringRead.ToLower().Contains("~ok")) {
					watchParam.Stop();

					var elapsedMs = watchParam.ElapsedMilliseconds;

					Test_in_progress = false;
					ListParamsErrors.Items.Add("Got Lines = " + ParamLinesCount.ToString());
					ListParamsErrors.Items.Add("Got Chars = " + ParamCharCount.ToString());
					ListParamsErrors.Items.Add("Time taken = " + TimeSpan.FromMilliseconds(elapsedMs).TotalSeconds.ToString() + " s");
					ListParamsErrors.Update();
					Save_Parameters_To_PC();
				} else {
					if (!IsSentCmd)
						AddToParameters(stringRead);
					else
						IsSentCmd = false;
					LabelParamLines.Text = ParamLinesCount.ToString();
					LabelParamLines.Update();
				}
			} while (Test_in_progress);

			ListShadow.Enabled = true;
			sbAction.Visible = false;
		}

		/**********************************************************************************************************************/
		private void AddToParameters(string msg_str) {
			ListShadow.Items.Add(msg_str);
			ListShadow.Update();
			ParamCharCount = ParamCharCount + msg_str.Length;
			ParamLinesCount++;
		}

		/**********************************************************************************************************************/
		private void Save_Parameters_To_PC() {
			//Save the file logic
			saveParameterFileDialog = new SaveFileDialog();
			DateTime dateTimeNow = DateTime.Now;
			//DC-2020 parameters 04-04-2017 12-49.param
			saveParameterFileDialog.FileName = "DC-2020 parameters " + dateTimeNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".param";
			// set filters - this can be done in properties as well
			//saveParameterFileDialog.Filter = "Data files (*.param)|*.txt|All files (*.*)|*.*";    //previous version, saved as txt file instead
			saveParameterFileDialog.Filter = "Data files (*.param)|*.*";                            //artem 060618

			//// File name, DC-2020 parameters 04-04-2017 12-49.param
			//// DC-2020 parameters
			//// Date, 04-04-2017
			//// Time, 12:50:56,

			if (saveParameterFileDialog.ShowDialog() == DialogResult.OK) {
				txtStatus.Text = "File(s)=" + saveParameterFileDialog.FileName;
				using (StreamWriter sw = new StreamWriter(saveParameterFileDialog.FileName)) {
					sw.WriteLine("//// File name," + System.IO.Path.GetFileName(saveParameterFileDialog.FileName));
					sw.WriteLine("//// DC-2020 parameters");
					sw.WriteLine("//// Date," + dateTimeNow.ToString("yyyy-MM-dd_HH-mm"));
					sw.WriteLine("//// Time," + dateTimeNow.ToString("HH-mm-ss"));
					sw.WriteLine("//// version " + SystemFWversion);

					foreach (var item in ListShadow.Items) {
						sw.WriteLine(item.ToString());
					}
					sw.Close();
				}
			}
		}

		/**********************************************************************************************************************/
		string Param_FW_version;
		private void cmdRestoreParameters_Click(object sender, EventArgs e) {
			openParameterFileDialog = new OpenFileDialog();
			openParameterFileDialog.Filter = "Data File (*.param)|*.param|All files (*.*)|*.*";
			int count = 0;
			ParamLinesCount = 0;
			int line_error_number = 0;
			FileStream filestream = null;
			// DateTime start_time = DateTime.Now;
			Stopwatch watchParam;
			if (openParameterFileDialog.ShowDialog() == DialogResult.OK) {
				sbAction.Checked = true;
				sbAction.Visible = true;
				ListShadow.Items.Clear();
				ListParamsErrors.Items.Clear();

				filestream = (FileStream)openParameterFileDialog.OpenFile();

				using (StreamReader sr = new StreamReader(filestream)) {
					// start_time = DateTime.Now;
					watchParam = Stopwatch.StartNew();
					bool versionMatched = false;
					while (!sr.EndOfStream) {
						string readLine = sr.ReadLine();
						if (!versionMatched) {
							if (readLine.Contains("//// version")) {
								Param_FW_version = readLine.Substring(13);
								if (!SystemFWversion.Contains(Param_FW_version)) {
									string Title = "Firmware and Parameters file have different versions";
									string Msg = ("   Firmware: " + (SystemFWversion + ("\r\n" + ("Parameters: " + (Param_FW_version + ("\r\n" + ("Continue?" +
										("\r\n" + "\'Yes\' will update only recognized parameters; \'No\' will not change parameters"))))))));
									if (MessageBox.Show(this, Msg, Title, MessageBoxButtons.YesNo) != DialogResult.Yes) {
										sr.Close();
										sbAction.Visible = false;
										return;
									}
									versionMatched = true;
									//Write code to disable RT functionality
									SendInternal("echo>enable" + Environment.NewLine, CommandTypes.ResponseExpected, out Buffer, CommandAckTypes.AckExpected);
								} else
									versionMatched = true;
							}
						}
						if (versionMatched) {
							if (readLine != string.Empty) {
								AddToParameters((count.ToString("0000") + ": " + readLine));
								LabelParamLines.Text = ParamLinesCount.ToString();
								if (readLine.Substring(0, 1) != "/") {
									int indexOfComment = readLine.IndexOf("//");
									if (indexOfComment != -1) {
										readLine = readLine.Substring(0, indexOfComment - 1);
									}
									if (readLine != "expo") {
										AckTypes respType = SendInternal(readLine + Environment.NewLine, CommandTypes.ResponseExpected, out Buffer, CommandAckTypes.AckExpected);
									}
									if (Buffer.ToLower().IndexOf(">~un") != -1 || Buffer.ToLower().IndexOf(">~er") != -1 || Buffer.ToLower().IndexOf("timeout") != -1) {
										ListParamsErrors.Items.Add("! Error on Line: " + (count).ToString("0000") + " '" + Buffer + " '");
										line_error_number++;
									} else if (Buffer.ToLower().IndexOf("<!") != -1) {
										ListParamsErrors.Items.Add("! Warning on Line: " + (count).ToString("0000") + " '" + Buffer + " '");
									}
									//else if (Buffer != "")
									//{
									//    ListParamsErrors.Items.Add("! Warning on Line: " + (count).ToString("0000") + " '" + Buffer + " '");
									//    AddToParameters(Buffer);
									//}
								}
								count++;
								LabelParamLines.Update();
								ListParamsErrors.Update();
							}
						}
					}
					sr.Close();
				}
				//  TimeSpan timeDiff = DateTime.Now - start_time;
				watchParam.Stop();
				var elapsedMs = watchParam.ElapsedMilliseconds;

				ListParamsErrors.Items.Add("Received " + count.ToString("0000") + " lines" + ", took " + TimeSpan.FromMilliseconds(elapsedMs).TotalSeconds.ToString() + " sec");
				ListParamsErrors.Items.Add("Errors " + line_error_number);
				ListParamsErrors.Update();
				sbAction.Visible = false;
			}
		}

		/**********************************************************************************************************************/
		private void CmdSaveParamList_Click(object sender, EventArgs e) {
			Save_Parameters_To_PC();
		}

		/**********************************************************************************************************************/
		private void CmdClearParamList_Click(object sender, EventArgs e) {
			ListShadow.Items.Clear();
			ListParamsErrors.Items.Clear();
			ParamLinesCount = 0;
			LabelParamLines.Text = ParamLinesCount.ToString();
		}

		/**********************************************************************************************************************/
		System.Threading.Thread UpdateThread;
		bool IsUpdateThreadStarted = false;

		//void _timerforFixedFreq_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			////UpdateThread = new System.Threading.Thread(() =>
			////{
			//    this.Invoke((System.Action)(() =>
			//    {
			//        Measure_MotSens_at_freq();
			//        //Program.formTerminal.Update();
			//    }));
			////});
			////IsUpdateThreadStarted = true;
			////UpdateThread.Start();
		//}


		/**********************************************************************************************************************/

		private void Chk_StartStopMotSenTest_CheckedChanged(object sender, EventArgs e) {
			string decoding_str;
			string response = string.Empty;
			ContinuousMonitoringOffOn(false);
			if ((Chk_StartStopMotSenTest1.Checked == true)) {
				if (((String.IsNullOrEmpty(Program.LTFtestOutputName))
					|| (String.IsNullOrEmpty(Program.LTFtestInputName)))) {
					MessageBox.Show("Please choose Output and Input first and try again", "Parameters Not Defined", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					Chk_StartStopMotSenTest1.Checked = false;
					// MotorSensorTest_active = false;
					return;
				}
				decoding_str = "echo";
				// response = GetSend(decoding_str, true);
				SendInternal(decoding_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
				// update freq
				if ((response.Contains("verb"))) {
					// response = GetSend("echo>enable", true);
					SendInternal("echo>enable", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
					// disable VERBOSE echo
				}

				decoding_str = ("freq=" + NInumExcitFreq0.Value);
				SendInternal(decoding_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
				//  response = GetSend(decoding_str, true);
				// update freq
				decoding_str = ("ampl=" + NInumExcitAmpl0.Value);
				SendInternal(decoding_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);

				//  Clear_Performance_Data_Arrays();
				UpdateThread = new System.Threading.Thread(() => MultiCallTo_Measure_MotSens_at_freq());
				IsUpdateThreadStarted = true;
				UpdateThread.Start();
				//    UpdateThread.Join();
				// _timerforFixedFreq.Start();

			} else {
				IsUpdateThreadStarted = false;
				// _timerforFixedFreq.Stop();
				return;
			}
		}

		/**********************************************************************************************************************/
		private void MultiCallTo_Measure_MotSens_at_freq() {
			if (IsUpdateThreadStarted) {
				Measure_MotSens_at_freq();
			}
		}


		/**********************************************************************************************************************/
		static readonly object LockObject = new object();
		void Measure_MotSens_at_freq() {
callagain:
			if (!IsUpdateThreadStarted)
				return;
			string decoding_str = string.Empty;
			string MotSen_testData;
			int pos_separator;
			ToggleRTmonitoring.Checked = false;
			string SendString = string.Empty;
			string result = string.Empty;
			int SendStrLen;
			double tryParVal = 0;

			try {
				//  Send the  command
				if (LTF_command != "") {
					// echo_removed = false;
					create_LTF_command();
					SendString = LTF_command;
					//lstResponse.AddItem(("=>" + LTF_command));
					SendString = (SendString + "");
					SendStrLen = SendString.Length;
					// open_port_if_closed();
					// Send_GUI_command(SendStringa
					SendInternal(SendString, CommandTypes.NoResponseExpected, out result, CommandAckTypes.NoAckExpected);

					// it will add to history
					// cmd_sent = true;
				} else {
					SendStrLen = 0;
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}

			//  Set up for response if query
			// Get_Serial_Input();
			Thread.Sleep(4000);
			MotSen_testData = TMCController.ControllerPort.GetEntireBuffer;
			if (MotSen_testData.Contains(SendString))
				MotSen_testData = MotSen_testData.Replace(SendString, "").Trim();

			//print command and response in terminal
			if (Program.formTerminal != null) {
				if (Program.formTerminal.InvokeRequired)
					Program.formTerminal.BeginInvoke((MethodInvoker)delegate
					{
						Program.formTerminal.MainForm_SendCallBack(SendString, MotSen_testData.Trim());
					});
				else { Program.formTerminal.MainForm_SendCallBack(SendString, MotSen_testData.Trim()); }
			}

			pos_separator = MotSen_testData.IndexOf(">~OK");
			if (pos_separator > 0 && MotSen_testData.Length > pos_separator + 2) {
				MotSen_testData = MotSen_testData.Substring(0, pos_separator - 2);
			}

			//print command and response in history window
			if (lstResponse.InvokeRequired)
				lstResponse.BeginInvoke((MethodInvoker)delegate { lstResponse.Items.Add(MotSen_testData); });
			else { lstResponse.Items.Add(MotSen_testData); }


			pos_separator = MotSen_testData.IndexOf(",");
			if ((pos_separator == -1)) {
				if (txtStatus.InvokeRequired)
					txtStatus.BeginInvoke((MethodInvoker)delegate { txtStatus.Text = "Motor-Sensor decoding error"; });
				else { txtStatus.Text = "Motor-Sensor decoding error"; }
				Program.MotorSensorTest_READY = true;
				return;
			}

			if (double.TryParse((MotSen_testData.Substring(0, pos_separator)), out tryParVal)) {
				decoding_str = MotSen_testData.Substring(0, pos_separator);
				//  separate magnitude
			} else if ((pos_separator > 7)) {
				decoding_str = MotSen_testData.Substring((pos_separator - 7), 7);
				//  separate magnitude
			}

			if (double.TryParse(decoding_str, out tryParVal)) {
				if (txtMag.InvokeRequired)
					txtMag.BeginInvoke((MethodInvoker)delegate
					{
						txtMag.Text = decoding_str; //double.Parse(decoding_str);
						txtMag.Update();
					});
				else {
					txtMag.Text = decoding_str; //double.Parse(decoding_str);
					txtMag.Update();
				}
			}

			decoding_str = MotSen_testData.Substring(pos_separator + 1);
			//  separate phase
			if (double.TryParse(decoding_str, out tryParVal)) {
				if (txtPhase.InvokeRequired)
					txtPhase.BeginInvoke((MethodInvoker)delegate
					{
						txtPhase.Text = decoding_str;//double.Parse(decoding_str);
						txtPhase.Update();
					});
				else {
					txtPhase.Text = decoding_str;//double.Parse(decoding_str);
					txtPhase.Update();
				}
			}

			Program.MotorSensorTest_READY = true;
			// _timerforFixedFreq.Enabled = true;
			goto callagain;
		}
		// ====  E N D  O F  ==== TEST MOTOR_SENSOR PAIR ============================================================================

		/**********************************************************************************************************************/
		private void optTestInput0_CheckedChanged(object sender, EventArgs e) {
			if (optTestInput0.Checked) {
				//FrameMeasureAtFreq.Visible = true;
				//FrameMeasureAtDiffFreq.Visible = false;
				FrameLTF.SendToBack();
			} else {
				FrameLTF.BringToFront();
			}
			create_LTF_command();
		}

		/**********************************************************************************************************************/
		private void ChkShowCurrentPlot_CheckedChanged(object sender, EventArgs e) {
			if (ChkShowCurrentPlot.Checked) {
				ScatterGraphMag.Plots[Perfor].Visible = true;
				ScatterGraphPhase.Plots[Perfor].Visible = true;
			} else {
				ScatterGraphMag.Plots[Perfor].Visible = false;
				ScatterGraphPhase.Plots[Perfor].Visible = false;
			}
		}

		/**********************************************************************************************************************/
		private void ChkShowPrevPlot_CheckedChanged(object sender, EventArgs e) {
			if (ShowPrevPlotControl.Checked) {
				ScatterGraphMag.Plots[PrevLTF].Visible = true;
				ScatterGraphPhase.Plots[PrevLTF].Visible = true;
			} else {
				ScatterGraphMag.Plots[PrevLTF].Visible = false;
				ScatterGraphPhase.Plots[PrevLTF].Visible = false;
			}
		}

		private void ChkShowLimits_CheckedChanged(object sender, EventArgs e) {
			//-!- 20211230 ADD
		}

		/**********************************************************************************************************************/
		private void cwPlotReference_Click(object sender, EventArgs e) {
			// if (buttonReadScan.Checked)
			{
				//openMeasurementFileDialog.FileName = string.Empty;
				cwPlotReference.Text = "Cancel";
				cwPlotReference.BackColor = ColorTranslator.FromHtml("#FFC0c80");
				if (current_Path == "")
					openMeasurementFileDialog.InitialDirectory = TMC_PathData;
				else {
					string current_FileName = "";
					if (current_Path.Contains("\\") || current_Path.Contains("/")) {// like: "C:\In_Out\Software\Mag-NetX_Analyzer_VB6\t\2014-08-29 -- Test 1X.csv"
																					// in C# it is double slash "C:\\In_Out\\Software\\Mag-NetX_Analyzer_VB6\\t\\2014-08-29 -- Test 1X.csv""
						var last_slash_pos = current_Path.LastIndexOf("\\");
						if (last_slash_pos == 0)
							last_slash_pos = current_Path.LastIndexOf("/"); // maybe in can be '/'?
						if (current_Path.Length > last_slash_pos + 2)
							current_FileName = current_Path.Substring(last_slash_pos + 1);
						var current_directory = current_Path.Substring(0, last_slash_pos);
						openMeasurementFileDialog.InitialDirectory = current_directory;
						openMeasurementFileDialog.FileName = current_FileName;
					}
				}
				if (/*rejected?*/ openMeasurementFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) {
					//  buttonReadScan.Checked = false;
				} else {
					current_Path = openMeasurementFileDialog.FileName;
					var /*stream*/ stm = new System.IO.StreamReader(openMeasurementFileDialog.FileName);
					try {
						var /*text*/ txt = /*1st line*/ stm.ReadLine().ToLower(System.Globalization.CultureInfo.InvariantCulture);
						if (/*bad?*/ !txt.Contains("file name,")) { throw new System.InvalidOperationException("Data line[1] is missing file name:\r\"" + txt + "\""); }
						txt = /*2nd line*/ stm.ReadLine().ToLower(System.Globalization.CultureInfo.InvariantCulture);
						if (/*bad?*/ !txt.Contains("date,")) { throw new System.InvalidOperationException("Data line[2] is missing date:\r\"" + txt + "\""); }
						txt = /*3rd line*/ stm.ReadLine().ToLower(System.Globalization.CultureInfo.InvariantCulture);
						if (/*bad?*/ !txt.Contains("time,")) { throw new System.InvalidOperationException("Data line[3] is missing time:\r\"" + txt + "\""); }
						txt = /*4th line (ignored)*/ stm.ReadLine();

						// Read header (5th) line: "Frequency HZ,x-suppression dB,x-dB_Open,x-deg_Open,x-db_Clos,x-deg_Clos,x-deg_suppr"
						//txt = /*5th line: header*/ stm.ReadLine();
						//if (/*bad?*/ !txt.Contains("Frequency")) { throw new System.InvalidOperationException("Data line[5] must be a header:\r\"" + txt + "\""); }
						bool gotFreqHeader = false;
						for (int nGetStartLine = 1; nGetStartLine < 5; nGetStartLine++) {
							txt = /*5th line: header*/ stm.ReadLine();

							if (/*bad?*/ txt.Contains("Frequency")) {
								gotFreqHeader = true;
								nGetStartLine = 6;
							}
						}
						if (/*bad?*/ !gotFreqHeader) { throw new System.InvalidOperationException("Data line[5] must be a header:\r\"" + txt + "\""); }

						Clear_Ref_Data_Arrays();

						// Data lines follow the format:
						// "{0:#0.00}, {1:#0.000}, {2:#0.000}, {3:#0.00}, {4:#0.000}, {5:#0.00}, {6:#0.00}, #"
						for (var /*count*/ cnt = /*because 5 lines above*/ 3; /*more data?*/ stm.Peek() > 0; cnt++) {
							txt = /*next line*/ stm.ReadLine();
							txt = txt.TrimEnd('\r', '\n');
							var /*entries*/ nts = txt.Split(',');
							if (/*bad?*/ nts.Count() != 4 || !nts[3].Contains("#")) {
								throw new System.InvalidOperationException("Data line[" + cnt.ToString() + "] is corrupt:\r\"" + txt + "\"");
							}
							LTFreferenceFreq[LTFreferenceLineCounter] = double.Parse(nts[0]);
							LTFreferenceGain[LTFreferenceLineCounter] = double.Parse(nts[1]);
							LTFreferencePhase[LTFreferenceLineCounter] = double.Parse(nts[2]);
							LTFrefNegativePh[LTFreferenceLineCounter] = Make180phaseShift(double.Parse(nts[2]));
							LTFreferenceLineCounter += 1;
						}
					} catch (Exception ex) {
						MessageBox.Show("The file is NOT a valid scan file:" + "\r" + Tools.TextTidy(ex.ToString()), "Scan File Anomaly", MessageBoxButtons.OK, MessageBoxIcon.Error);
					} finally { stm.Close(); }
					Update_Ref_Plot(); // ACTUAL PLOT
					ChkShowRefPlot.Checked = true;  // mdr 060518
					FindUnityGainPhase(LTFlineCounter);
					//  buttonReadScan.Checked = false;
				}
			}
			cwPlotReference.Text = "Get Ref Plot";
			cwPlotReference.BackColor = ColorTranslator.FromHtml("#FF8000");
		}

		/**********************************************************************************************************************/
		private void ChkShowRefPlot_CheckedChanged(object sender, EventArgs e) {
			if (ChkShowRefPlot.Checked) {
				ScatterGraphMag.Plots[RefLTF].Visible = true;
				ScatterGraphPhase.Plots[RefLTF].Visible = true;
			} else {
				ScatterGraphMag.Plots[RefLTF].Visible = false;
				ScatterGraphPhase.Plots[RefLTF].Visible = false;
			}
		}

		private void cmdClearTerminal_Click(object sender, EventArgs e) {
			lstResponse.Items.Clear();
		}

		/**********************************************************************************************************************/
		System.Timers.Timer _timerforRT = null;
		void _timerforRT_Elapsed(object sender, System.Timers.ElapsedEventArgs e) {
			this.BeginInvoke((MethodInvoker)delegate
			{
				//if (((System.Windows.Forms.TabControl)(PageControl_Main)).SelectedTab == TabPageFirmware)
				//    Read_Load_Sensors(); //STACIS
				ShowSysStatus();
				Read_Alarm_Status();
				read_RT_diagnostic();
			});

			//if( ((System.Windows.Forms.TabControl)(PageControl_Main)).SelectedTab == TabPageFirmware)
			//    Read_Load_Sensors();
			// else if (((System.Windows.Forms.TabControl)(PageControl_Main)).SelectedTab == TabPageAlarmLogs)
			// Read_Alarm_Status();
			// read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		private void ToggleRTmonitoring_CheckedChanged(object sender, EventArgs e) {
			if (Test_in_progress)
				return;
			ContinuousMonitoringOn = ToggleRTmonitoring.Checked;
			if (Program.formTerminal != null)
				Program.formTerminal.OnQuitMenuCommand();
			ContinuousMonitoringSwitchShow();
			ContinuousMonitoringOffOn(ContinuousMonitoringOn);
		}

		/**********************************************************************************************************************/
		private void PageControl_Main_SelectedIndexChanged(object sender, EventArgs e) {
			if (((PageControl)sender).SelectedIndex == 3) { //performance - all area (with top) becomes available for plots
				TableLayoutPanel_Main.RowStyles[0].Height = 0;
				TableLayoutPanel_Main.RowStyles[1].Height = 35 + 142;
				TableLayoutPanel_Main.RowStyles[0].SizeType = SizeType.Absolute;
				PageControl_Main.Height = 642;
				Panel_Menu.Location = new System.Drawing.Point(0, (135 + 142)); //-!- Does nor work even if no ancors?
				PageControl_Main.Size = new System.Drawing.Size(800, 630);
				TableLayoutPanel_PerformancePage.Height = 570;
				//    Refresh();
				// 825, 486
			} else //everything else - top area dedicated for real time panel
			  {
				TableLayoutPanel_Main.RowStyles[0].Height = 140;
				TableLayoutPanel_Main.RowStyles[1].Height = 20;
				TableLayoutPanel_Main.RowStyles[0].SizeType = SizeType.Absolute;
				PageControl_Main.Height = 500;
				Panel_Menu.Location = new System.Drawing.Point(0, 135);
				PageControl_Main.Size = new System.Drawing.Size(800, 480);
			}
		}

		/**********************************************************************************************************************/
		private void cmdLoadDefaults0_Click(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;
			SendInternal("dflt", CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void ToggleCOMconnection_CheckedChanged(object sender, EventArgs e) {
		//	Connect_Or_Disconnect(); // IK20220102 this hangs main form because it does connect-disconnect-connect etc.
		}

		/**********************************************************************************************************************/
		private void Chk_IndicatorAlarm_CheckedChanged(object sender, EventArgs e) {
			CheckBox checkBoxCtrl = sender as CheckBox;
			if (checkBoxCtrl.Checked == true)
				checkBoxCtrl.Image = Resources.checkmark_small_gray;
			else
				checkBoxCtrl.Image = Resources.exclamation;
		}

		/**********************************************************************************************************************/
		private void Chk_IndicatorSelfTest_CheckedChanged(object sender, EventArgs e) {
			CheckBox checkBoxCtrl = sender as CheckBox;
			if (checkBoxCtrl.Checked == true)
				checkBoxCtrl.Image = Resources.checkmark_small_gray;
			else
				checkBoxCtrl.Image = Resources.exclamation;
		}

		/**********************************************************************************************************************/
		// This is an "Easter Egg:" if the user knows enough to double-click the time text,
		// he/she gets access to special features ... of which, at this moment, there are none!
		private void lblSystemDate_MouseDoubleClick(object sender, MouseEventArgs e) {
#if DEBUG
			MessageBox.Show("Quit checking the time!", "Get back to work!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
#endif
			//if (ImageTerminal.Visible) {
			//	ImageTerminal.Visible = false;
			//	LblTerminalWindow.Visible = false;
			//	ImageMonitoring.Visible = false;
			//	LblMonitoring.Visible = false;
			//	ChkVerboseParams.Visible = false;
			//} else {
			//	ImageTerminal.Visible = true;
			//	LblTerminalWindow.Visible = true;
			//	ImageMonitoring.Visible = true;
			//	LblMonitoring.Visible = true;
			//	ChkVerboseParams.Visible = true;
			//}
		}

		/**********************************************************************************************************************/
		private void frmDiagnostic_Enter(object sender, EventArgs e) {

		}

		/**********************************************************************************************************************/
		private void CmbBNC_SelectedIndexChanged(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;

			string cmd;
			string resp;
			//if (valueToSet >= 0)
			{
				ComboBox cmbCtrl = sender as ComboBox;
				if (cmbCtrl.Name.Contains("BNC0")) {
					//  -other-- has ValuePairs.Value -1, do not send command
					cmd = ("bncd0=" + cmbCtrl.SelectedIndex.ToString());
					// Analyzer.GetSend(cmd, true);
				} else {
					//  -other-- has ValuePairs.Value -1, do not send command
					cmd = ("bncd1=" + cmbCtrl.SelectedIndex.ToString());
					// Analyzer.GetSend(cmd, true);
				}
				SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				//  Init_sys_data = true;
				// CmbBNC0.ListIndex = BNC_0_index;
				// Init_sys_data = false;
			}
		}
		//         formRTMon rtMonWindow;
		// formMonitoring rtFormMonitoring;

		/**********************************************************************************************************************/
		private void ImageMonitoring_Click(object sender, EventArgs e) {
			OpenCloseMonitoringForm();
		}

		/**********************************************************************************************************************/
		public void OpenCloseMonitoringForm(bool isVisible = true) {
			if (/*form already active?*/ Program.FormRTMonitoring != null) {
				if (!Program.FormRTMonitoring.Visible && !Program.FormRTMonitoring.FormTerminalVisible) {
					LblMonitoring.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
					LblMonitoring.ForeColor = Color.Maroon;
					Program.FormRTMonitoring.Show();
					Program.FormRTMonitoring.Visible = true;
					Program.FormRTMonitoring.FormTerminalVisible = true;
					return;
				} else {
					LblMonitoring.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
					LblMonitoring.ForeColor = Color.Black;
					Program.FormRTMonitoring.Hide();
					Program.FormRTMonitoring.FormTerminalVisible = false;
				}
			} else {
				LblMonitoring.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
				LblMonitoring.ForeColor = Color.Maroon;
				Program.FormRTMonitoring = new formMonitoring(this);
				Program.FormRTMonitoring.Show();
				Program.FormRTMonitoring.Visible = true;
				Program.FormRTMonitoring.FormTerminalVisible = true;
			}
		}

		/**********************************************************************************************************************/
		private void timerClock_Tick(object sender, EventArgs e) {
			lblSystemDate.Text = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
			if (Test_in_progress) { textBoxTestTime.Text = (DateTime.Now - LTFStartTime).TotalSeconds.ToString("#000"); }
		}

		/**********************************************************************************************************************/
		private void ConnectionMenuPanel_Paint(object sender, PaintEventArgs e) {
		}

		/**********************************************************************************************************************/
		private void ImageTerminal_Click(object sender, EventArgs e) {
			OpenTerminalForm();
		}

		/**********************************************************************************************************************/
		private void cmdCommSETUP_Click(object sender, EventArgs e) {
			Device.Instance.StopTimerToReadData();
			OpenSplashForm();
		}

		/**********************************************************************************************************************/
		private void OpenTerminalForm(object sender, EventArgs e) {

		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicatorClickEvent(object sender, EventArgs e) {
			string response = string.Empty;
			SendInternal("s", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			Decode_SysStatus(response);
		}

		/**********************************************************************************************************************/
		private string Decode_SysStatus(string stat_str) {
			string status = string.Empty;
			string retval;
			int token_pos;
			string bits_str;
			int sys_stat_int = 0;

			if (stat_str.Length > 2) {
				token_pos = stat_str.IndexOf(':');
				if (token_pos != 0 && stat_str.Length >= token_pos + 2 + 2) {
					retval = stat_str.Substring(token_pos + 2, 2);
				} else {
					status = "";
					return status;
				}
			} else {
				retval = stat_str;
			}
			if(int.TryParse(retval, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.InvariantInfo, out _))
				sys_stat_int = int.Parse((retval), System.Globalization.NumberStyles.HexNumber);
			// bits_str = Convert.ToString(sys_stat_int,2);
			bits_str = BIN(sys_stat_int);

			txtStatus.Text = "Status 0x" + retval + "; " + sys_stat_int.ToString() + "D; b" + bits_str;

			///cw_Status_Bit_Indicator_0
			if (bits_str.Substring(15 - 2 * 0, 1) == "1")
				NIstatusBitIndicator0.Value = true;
			else
				NIstatusBitIndicator0.Value = false;

			///cw_Status_Bit_Indicator_1
			if (bits_str.Substring(15 - 2 * 1, 1) == "1")
				NIstatusBitIndicator1.Value = true;
			else
				NIstatusBitIndicator1.Value = false;

			///cw_Status_Bit_Indicator_2
			if (bits_str.Substring(15 - 2 * 2, 1) == "1")
				NIstatusBitIndicator2.Value = true;
			else
				NIstatusBitIndicator2.Value = false;

			///cw_Status_Bit_Indicator_3
			if (bits_str.Substring(15 - 2 * 3, 1) == "1")
				NIstatusBitIndicator3.Checked = true;
			else
				NIstatusBitIndicator3.Checked = false;

			///cw_Status_Bit_Indicator_4
			if (bits_str.Substring(15 - 2 * 4, 1) == "1")
				NIstatusBitIndicator4.Value = true;
			else
				NIstatusBitIndicator4.Value = false;

			///cw_Status_Bit_Indicator_5
			if (bits_str.Substring(15 - 2 * 5, 1) == "1")
				NIstatusBitIndicator5.Value = true;
			else
				NIstatusBitIndicator5.Value = false;

			///cw_Status_Bit_Indicator_6
			if (bits_str.Substring(15 - 2 * 6, 1) == "1")
				NIstatusBitIndicator6.Value = true;
			else
				NIstatusBitIndicator6.Value = false;

			///cw_Status_Bit_Indicator_7
			if (bits_str.Substring(15 - 2 * 7, 1) == "1")
				statusBitIndicator7.Checked = true;
			else
				statusBitIndicator7.Checked = false;

			//int bit_N;
			//for ( bit_N = 0; bit_N < cw_Status_Bit_Indicator.Count; bit_N++)
			//{
			//    if (Strings.Mid(bits_str, 16 - 2 * bit_N, 1) == "1")
			//        cw_Status_Bit_Indicator[bit_N].Value = true;
			//    else
			//        cw_Status_Bit_Indicator[bit_N].Value = false;
			//}
			return status;
		}

		/**********************************************************************************************************************/
		private string BIN(int inbyte) {
			string binaryNum = string.Empty;
			string m = string.Empty, b = string.Empty;
			int k = 0, j = 0, bt = 0;
			bt = inbyte;
			for (j = 0; j <= 7; j++) {
				if (bt < 2)
					goto end_conv;
				b = (bt - 2 * (bt / 2)).ToString();
				bt = bt / 2;
				m = " " + b + m;
			}
end_conv:
			m = " " + bt.ToString() + m;
			for (k = j; k <= 6; k++) {
				m = " 0" + m;
			}
			binaryNum = m;
			return binaryNum;
		}

		/**********************************************************************************************************************/
		private void _cmdRestorefromFlash_Click(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;
			string retVal = string.Empty;
			SendInternal("rsto", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
			GetSysData();
		}

		/**********************************************************************************************************************/
		private void _cmdLoadDefaults_Click(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;
			string retVal = string.Empty;
			SendInternal("dflt", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void _cmdSaveParamsIntoFLASH_Click(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;
			string retVal = string.Empty;
			SendInternal("save", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		private void LblTerminalWindow_Click(object sender, EventArgs e) {
			OpenTerminalForm();
		}

		/**********************************************************************************************************************/
		// LTF Y scale controls
		private double Max_dB = 20;
		private double Min_dB = -20;

		private void num_MaxdB_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {
			Max_dB = num_MaxdB.Value;
			ScatterGraphMag.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
			if (Max_dB <= Min_dB) {
				Min_dB = Max_dB - 5;
				num_MindB.Value = Min_dB; // will call num_MindB_AfterChangeValue
			}
			ScatterGraphMag.YAxes[0].Range = new NationalInstruments.UI.Range(Min_dB, Max_dB);
		}

		/**********************************************************************************************************************/
		private void num_MindB_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {
			Min_dB = num_MindB.Value;
			ScatterGraphMag.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
			if (Min_dB >= Max_dB) {
				Max_dB = Min_dB + 5;
				num_MaxdB.Value = Max_dB; // will call num_MaxdB_AfterChangeValue
			}
			ScatterGraphMag.YAxes[0].Range = new NationalInstruments.UI.Range(Min_dB, Max_dB);
		}

		/**********************************************************************************************************************/
		private void ChkShowPredictionPlot_CheckedChanged(object sender, EventArgs e) {
			if (((CheckBox)sender).Checked == true) {
				frmFilters.ChkRealTimeUpdateTF.Checked = true;
				frmFilters.Calc_Prediction();
			} else {
				frmFilters.ChkRealTimeUpdateTF.Checked = false;
				ScatterGraphMag.Plots[frmFilters.Prediction_TF_Num].Visible = false;
				ScatterGraphPhase.Plots[frmFilters.Prediction_TF_Num].Visible = false;
			}
			UpdateRefresh();
		}
		/**********************************************************************************************************************/
		public void OpenSplashForm(bool isVisible = true) {
			if (/*form already active?*/ Program.FormSplash != null) {
				//Program.FormSplash.s_port.SelectedIndex = LastSelectedController;
				// Make the already running form visible and give it focus.
				Program.FormSplash.SearchCOMPOrts();
				Program.FormSplash.Select();
				Program.FormSplash.Activate();
				Program.FormSplash.Show();
				if (isVisible) { Program.FormSplash.WindowState = FormWindowState.Normal; }
				return;
			}

			// Create and show the form.
			Program.FormSplash = new frmSplash();
			Program.FormSplash.Visible = true;
			Program.FormSplash.Activate();

			if (!isVisible) { Program.formTerminal.WindowState = FormWindowState.Minimized; }
		}

		/**********************************************************************************************************************/
		private void ChkBNCsliders_ValueChanged(object sender, EventArgs e) {
			if (/*busy?*/ Program.IsReadingControllerSettings)
				return;

			if (ChkBNCsliders.Checked == true) {
				ChkBNCsliders.BackColor = ColorTranslator.FromHtml("#c0ffff");
				ChkBNCsliders.Text = "drop down";
				CmbBNC0.Visible = false;
				CmbBNC1.Visible = false;

				cwBNC1select.BringToFront();
				cwBNC0select.BringToFront();
				cwBNC1select.Visible = true;
				cwBNC0select.Visible = true;
			} else {
				ChkBNCsliders.BackColor = ColorTranslator.FromHtml("#c0ffc0");
				ChkBNCsliders.Text = "SLIDERS";
				CmbBNC0.Visible = true;
				CmbBNC1.Visible = true;

				cwBNC1select.Visible = false;
				cwBNC0select.Visible = false;
				cwBNC1select.SendToBack();
				cwBNC0select.SendToBack();
			}
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// Button on Communication Tab copying lstResponse to clipboard
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CmdCopyClipboard_Click(object sender, EventArgs e) {
			Copy_response_to_clipboard();
		}

		/**********************************************************************************************************************/
		private void Copy_response_to_clipboard() {
			string tmp_str = string.Empty;
			foreach (var item in lstResponse.Items) {
				tmp_str += item.ToString() + "\r\n";
			}
			Clipboard.Clear();
			if (!String.IsNullOrEmpty(tmp_str))
				Clipboard.SetText(tmp_str);
		}

		/**********************************************************************************************************************/
		private void CmdSaveResponsesToPC_Click(object sender, EventArgs e) {
			Save_response_to_PC();
		}

		/**********************************************************************************************************************/
		private void Save_response_to_PC() {
			string SaveFileName = null;
			string FileName = null;
			string[] PortSettings = Device.Instance.Settings;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "Choose name parameters file";
			string applicationDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).ToString();
			saveFileDialog.InitialDirectory = applicationDir;
			saveFileDialog.DefaultExt = "txt";
			saveFileDialog.Filter = "File (*.txt)|*.param|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.Cancel) {
				return;
			} else { }
			SaveFileName = saveFileDialog.FileName;
			FileName = Path.GetFileName(SaveFileName);
			var /*stream*/ stm = new System.IO.StreamWriter(SaveFileName);
			foreach (var item in lstResponse.Items) {
				stm.WriteLine(item.ToString());
			}
			stm.Close();
		}

		/**********************************************************************************************************************/
		private void cwBNC1select_PointerValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string cmd;
			string resp;
			BNC_1_index = SlideItemValue(Convert.ToInt32(cwBNC1select.Value - 1));
			string retVal = cwBNC1select.CustomDivisions[Convert.ToInt32(cwBNC1select.Value - 1)].Text;
			if (SlideItemValue(Convert.ToInt32(cwBNC1select.Value - 1)) >= 0) {
				cmd = "bncd1=" + SlideItemValue(Convert.ToInt32(cwBNC1select.Value - 1)).ToString();
				SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				if (CmbBNC1.Items.Count > 0)
					CmbBNC1.SelectedIndex = BNC_1_index;
			}
		}

		/**********************************************************************************************************************/
		private void cwBNC0select_PointerValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string cmd;
			string resp;
			BNC_0_index = SlideItemValue(Convert.ToInt32(cwBNC0select.Value - 1));
			string retVal = cwBNC0select.CustomDivisions[Convert.ToInt32(cwBNC0select.Value - 1)].Text;
			if (SlideItemValue(Convert.ToInt32(cwBNC0select.Value - 1)) >= 0) {
				cmd = "bncd0=" + SlideItemValue(Convert.ToInt32(cwBNC0select.Value - 1)).ToString();
				SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				if (CmbBNC0.Items.Count > 0)
					CmbBNC0.SelectedIndex = BNC_0_index;
			}
		}

		/**********************************************************************************************************************/
		private int SlideItemValue(int index) {
			int valueToSet = 0;
			switch (index) {
				case 0:
				valueToSet = 120;
				break;
				case 1:
				valueToSet = 59;
				break;
				case 2:
				valueToSet = 58;
				break;
				case 3:
				valueToSet = 57;
				break;
				case 4:
				valueToSet = 56;
				break;
				case 5:
				valueToSet = 55;
				break;
				case 6:
				valueToSet = 54;
				break;
				case 7:
				valueToSet = 53;
				break;
				case 8:
				valueToSet = 52;
				break;
				case 9:
				valueToSet = 51;
				break;
				case 10:
				valueToSet = 50;
				break;
				case 11:
				valueToSet = 49;
				break;
				case 12:
				valueToSet = 48;
				break;
				case 13:
				valueToSet = 39;
				break;
				case 14:
				valueToSet = 38;
				break;
				case 15:
				valueToSet = 37;
				break;
				case 16:
				valueToSet = 36;
				break;
				case 17:
				valueToSet = 43;
				break;
				case 18:
				valueToSet = 42;
				break;
				case 19:
				valueToSet = 41;
				break;
				case 20:
				valueToSet = 40;
				break;
				case 21:
				valueToSet = 29;
				break;
				case 22:
				valueToSet = 28;
				break;
				case 23:
				valueToSet = 27;
				break;
				case 24:
				valueToSet = 26;
				break;
				case 25:
				valueToSet = 25;
				break;
				case 26:
				valueToSet = 24;
				break;
				case 27:
				valueToSet = 17;
				break;
				case 28:
				valueToSet = 16;
				break;
				case 29:
				valueToSet = 19;
				break;
				case 30:
				valueToSet = 18;
				break;
				case 31:
				valueToSet = 6;
				break;
				case 32:
				valueToSet = 5;
				break;
				case 33:
				valueToSet = 4;
				break;
				case 34:
				valueToSet = 3;
				break;
				case 35:
				valueToSet = 2;
				break;
				case 36:
				valueToSet = 1;
				break;
				case 37:
				valueToSet = -1;
				break;
				default:
				break;
			}
			return valueToSet;
		}

		/**********************************************************************************************************************/
		public void GetSysData() {
			if (Test_in_progress)
				return;
			string temp_str;
			int indx;
			//bool cont_field_monitoring;
			double paramValue;
			bool bnc_index_found;
			string ControllerCommand;
			Program.IsReadingControllerSettings = true;
			if (Program.formTerminal != null)
				Program.formTerminal.OnQuitMenuCommand();

			if (M3v_Diagnostic_running == 1)
				SendInternal("oltf>stop", CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			if (!ID_valid)
				ID_valid = get_ID();
			if (!ID_valid)
				return;

			for (indx = 0; indx < NInumGoNoGOwindow.Count; indx++) {
				paramValue = RectifyValueResponse(NInumGoNoGOwindow[indx].Tag.ToString() + "?");
				if (paramValue != WRONG_VALUE) {
					paramValue = ValidateRangeForNiNumericEdit(NInumGoNoGOwindow[indx], paramValue);
					NInumGoNoGOwindow[indx].Value = Convert.ToInt32(paramValue);
				}
			}
			for (indx = 0; indx < NInumHeightAdj.Count; indx++) {
				paramValue = RectifyValueResponse(NInumHeightAdj[indx].Tag.ToString() + "?");
				if (paramValue != WRONG_VALUE) {
					paramValue = ValidateRangeForNiNumericEdit(NInumHeightAdj[indx], paramValue);
					NInumHeightAdj[indx].Value = Convert.ToInt32(paramValue);
				}
			}
			for (indx = 0; indx < NInumPressureSetPoint.Count; indx++) {
				paramValue = RectifyValueResponse(NInumPressureSetPoint[indx].Tag.ToString() + "?");
				if (paramValue != WRONG_VALUE) {
					paramValue = ValidateRangeForNiNumericEdit(NInumPressureSetPoint[indx], paramValue);
					NInumPressureSetPoint[indx].Value = paramValue;
				}
			}
			paramValue = RectifyValueResponse(cwNum_DockedPos.Tag + "?");
			if (paramValue == WRONG_VALUE) {
				cwNum_DockedPos.BackColor = ColorTranslator.FromHtml("#80c0ff");
				cwNum_DockedPos.Value = 0;
				cwNum_DockedPos.Enabled = false;
				toolTip1.SetToolTip(cwNum_DockedPos, "not available in this version of firmware, use Terminal menu->Pressure control to access");
			} else {
				cwNum_DockedPos.BackColor = ColorTranslator.FromHtml("#ffffff");
				paramValue = ValidateRangeForNiNumericEdit(cwNum_DockedPos, paramValue);
				cwNum_DockedPos.Value = Convert.ToInt32(paramValue);
				cwNum_DockedPos.Enabled = true;
			}

			ControllerCommand = "bncd0";
			bnc_index_found = false;
			paramValue = RectifyValueResponse(ControllerCommand);
			if (paramValue != WRONG_VALUE) {
				BNC_0_index = Convert.ToInt32(paramValue);
				if (CmbBNC0.Items.Count > BNC_0_index)
					CmbBNC0.SelectedIndex = BNC_0_index;
				bnc_index_found = Find_Index_Match_with_BNC_SLIDER(0, BNC_0_index);
				if (!bnc_index_found)
					cwBNC0select.Value = 38;
			} else
				cwBNC0select.Value = 38;

			ControllerCommand = "bncd1";
			bnc_index_found = false;
			paramValue = RectifyValueResponse(ControllerCommand);
			if (paramValue != WRONG_VALUE) {
				BNC_1_index = Convert.ToInt32(paramValue);
				if (CmbBNC1.Items.Count > BNC_1_index)
					CmbBNC1.SelectedIndex = BNC_1_index;
				bnc_index_found = Find_Index_Match_with_BNC_SLIDER(1, BNC_1_index);
				if (!bnc_index_found)
					cwBNC1select.Value = 38;
			} else
				cwBNC1select.Value = 38;

			if (PageControl_Main.SelectedTab != TabPage_Settings)
				Get_diag_params();
			RefreshSettingsPage();
			ShowSysStatus();
			Program.IsReadingControllerSettings = false;
		}

		/**********************************************************************************************************************/
		/// <summary>
		/// Validates and restricts the parameter value according to the valid range.
		/// </summary>
		/// <param name="NumEdit">The numeric edit control instance</param>
		/// <param name="paramValue">Parameter value</param>
		/// <returns>Valid value</returns>
		private double ValidateRangeForNiNumericEdit(NumericEdit NumEdit, double paramValue) {
			if (paramValue > NumEdit.Range.Maximum)
				paramValue = NumEdit.Range.Maximum;
			if (paramValue < NumEdit.Range.Minimum)
				paramValue = NumEdit.Range.Minimum;
			return paramValue;
		}

		/**********************************************************************************************************************/
		private void ShowSysStatus() {
			string response = string.Empty;
			SendInternal("s", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			Decode_SysStatus(response);
		}

		/**********************************************************************************************************************/
		public double TestFrequency;
		public double TestGain;
		public double TestPulseNegSlope;
		public double TestPulsePosSlope = 0;
		public int ExcitationAxis;

		private void Get_diag_params() {
			string temp_str;
			string retval;
			double paramValue;
			//int indx;
			//int token_pos;
			//string command_Renamed;
			paramValue = RectifyValueResponse("freq?");
			if (paramValue != WRONG_VALUE) {
				TestFrequency = paramValue;
				if (Program.FormRTMonitoring != null)
					Program.FormRTMonitoring.cwNumExcitFreq.Value = TestFrequency;   //uncomment when test & recoding is implemented
				TestFrequency = ValidateRangeForNiNumericEdit(this.NInumExcitFreq, paramValue);
				this.NInumExcitFreq.Value = TestFrequency;
				this.NInumExcitFreq0.Value = TestFrequency;
			}

			paramValue = RectifyValueResponse("ampl?");
			if (paramValue != WRONG_VALUE) {
				TestGain = paramValue;
				if (Program.FormRTMonitoring != null)
					Program.FormRTMonitoring.cwNumExcitAmpl.Value = TestGain;  // uncomment when test & recoding is implemented
				TestGain = ValidateRangeForNiNumericEdit(this.NInumExcitAmpl, paramValue);
				this.NInumExcitAmpl.Value = TestGain;
				this.NInumExcitAmpl0.Value = TestGain;
			}

			paramValue = RectifyValueResponse(NInumPulsePosSlope.Tag.ToString() + "?");
			if (paramValue != WRONG_VALUE) {
				TestPulsePosSlope = paramValue;
				TestPulsePosSlope = ValidateRangeForNiNumericEdit(this.NInumPulsePosSlope, paramValue);
				NInumPulsePosSlope.Value = TestPulsePosSlope;
			}

			paramValue = RectifyValueResponse(NInumPulseNegSlope.Tag.ToString() + "?");
			if (paramValue != WRONG_VALUE) {
				TestPulseNegSlope = paramValue;
				TestPulseNegSlope = ValidateRangeForNiNumericEdit(this.NInumPulseNegSlope, paramValue);
				NInumPulseNegSlope.Value = TestPulseNegSlope;
			}

			paramValue = RectifyValueResponse("drvo?");
			if (paramValue != WRONG_VALUE) {
				ExcitationAxis = Convert.ToInt32(paramValue);
				if (Program.FormRTMonitoring != null)
					Program.FormRTMonitoring.cmb_ExcitAxis.SelectedIndex = ExcitationAxis;       //uncomment when test & recoding is implemented
				if (ExcitationAxis < cmboxExcitAxis.Items.Count)
					this.cmboxExcitAxis.SelectedIndex = ExcitationAxis;
				else
					this.cmboxExcitAxis.SelectedIndex = cmboxExcitAxis.Items.Count - 1;
			}

			SendInternal("excit", CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			retval = temp_str.Substring(7).ToLower();
			if (retval.Contains( "star"))
				ExcitationStatus = true;
			else
				ExcitationStatus = false;
			ToggleExcitation.Checked = ExcitationStatus;
		}

		/**********************************************************************************************************************/
		private bool Find_Index_Match_with_BNC_SLIDER(int BNC_num, int paramValue) {
			int indx;
			//bool tmp_init_sys_data;

			bool Find_Index_Match_with_BNC_SLIDER = false;

			for (indx = 0; indx < BNC_ptr.Count; indx++) {
				if (BNC_ptr[indx] == paramValue) {
					if (BNC_num == 0)
						cwBNC1select.Value = indx + 1;
					else
						cwBNC0select.Value = indx + 1;
					return Find_Index_Match_with_BNC_SLIDER = true;
				}
			}
			if (BNC_num == 0)
				cwBNC1select.Value = Convert.ToInt32(cwBNC1select.Range.Maximum);
			else
				cwBNC0select.Value = Convert.ToInt32(cwBNC1select.Range.Maximum);
			return Find_Index_Match_with_BNC_SLIDER;
		}


		/**********************************************************************************************************************/

		private void ToggleSoftDock_ValueChanged(object sender, EventArgs e) {
			if (ToggleSoftDock.Checked) {
				ToggleSoftDock.BackgroundImage = Resources.ON_green_small_switch;
				label18.Text = "Click to release";
			} else {
				ToggleSoftDock.BackgroundImage = Resources.OFF_olive_small_switch;
				label18.Text = "Click to engage";
			}

			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;
			if (ToggleSoftDock.Checked) // Tag="horz"
				SendInternal(ToggleSoftDock.Tag.ToString() + ">hold", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			else
				SendInternal(ToggleSoftDock.Tag.ToString() + ">free", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		private void NInumFBgain_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;
			NumericEdit NInumFBgain = (NumericEdit)sender;
			string gain_cmd = NInumFBgain.Tag.ToString() + "=" + NInumFBgain.Value.ToString("0.00");
			SendInternal(gain_cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwNum_Go_NoGO_window_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;
			NumericEdit cwNum_Go_NoGO_window = (NumericEdit)sender;
			string val = cwNum_Go_NoGO_window.Tag.Equals("gngd") ? cwNum_Go_NoGO_window.Value.ToString("0.00") : cwNum_Go_NoGO_window.Value.ToString("0");
			string cmd = cwNum_Go_NoGO_window.Tag.ToString() + "=" + val;
			SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwNumHeightAdj_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;
			NumericEdit cwNumHeightAdj = (NumericEdit)sender;
			string cmd = cwNumHeightAdj.Tag.ToString() + "=" + cwNumHeightAdj.Value.ToString("0");
			SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void CmdZeroHorizProx_Click(object sender, EventArgs e) {
			string resp = string.Empty;
			SendInternal("z", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void CmdResetGeoOffsets_Click(object sender, EventArgs e) {
			SendInternal("zero", CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}


		/**********************************************************************************************************************/
		private void cwBut_AxisEn_ValueChanged(object sender, EventArgs e) {
			CheckBox cwBut_AxisEn = (CheckBox)sender;
			if (cwBut_AxisEn.Checked)
				cwBut_AxisEn.BackgroundImage = cwBut_AxisEn.Name.Equals("ToggleAxisEn0") ? Resources.ON_blue_slider : Resources.ON_green_small_switch;
			else
				cwBut_AxisEn.BackgroundImage = cwBut_AxisEn.Name.Equals("ToggleAxisEn0") ? Resources.OFF_gray_slider : Resources.OFF_olive_small_switch;

			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;

			if (cwBut_AxisEn.Checked)
				SendInternal(cwBut_AxisEn.Tag.ToString() + ">active", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			else
				SendInternal(cwBut_AxisEn.Tag.ToString() + ">passive", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		private void ToggleVertDock_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;
			string cmd;
			if (!ToggleVertDock.Checked) {
				label_BasicCMD2_0.Text = "Click to dock";
				ToggleVertDock.BackgroundImage = Resources.OFF_olive_small_switch;
				cmd = "u";
			} else {
				label_BasicCMD2_0.Text = "Click to float";
				ToggleVertDock.BackgroundImage = Resources.ON_green_small_switch;
				cmd = "l";
			}
			SendInternal(cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
			read_RT_diagnostic();
		}

		private void ToggleValveDither_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;
			string cmd;
			if (ToggleValveDither.Checked) {
				label_BasicCMD2.Text = "Click to disable";
				ToggleValveDither.BackgroundImage = Resources.ON_green_small_switch;
				cmd = "c";
			} else {
				label_BasicCMD2.Text = "Click to enable";
				ToggleValveDither.BackgroundImage = Resources.OFF_olive_small_switch;
				cmd = "h";
			}
			SendInternal(cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
			read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		private void cwNum_DockedPos_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;
			string cmd = cwNum_DockedPos.Tag.ToString() + "=" + cwNum_DockedPos.Value.ToString("0");
			SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwNum_PressureSetPoint_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;
			NumericEdit cwNum_PressureSetPoint = (NumericEdit)sender;
			string cmd = cwNum_PressureSetPoint.Tag.ToString() + "=" + cwNum_PressureSetPoint.Value.ToString("0.00");
			SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwBut_AuxDamping_ValueChanged(object sender, EventArgs e) {
			if (ToggleAuxDamping.Checked)
				ToggleAuxDamping.BackgroundImage = Resources.ON_green_small_switch;
			else
				ToggleAuxDamping.BackgroundImage = Resources.OFF_olive_small_switch;

			if (Program.IsReadingControllerSettings)
				return;

			string resp = string.Empty;

			if (ToggleAuxDamping.Checked)
				SendInternal(ToggleAuxDamping.Tag.ToString() + ">active", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			else
				SendInternal(ToggleAuxDamping.Tag.ToString() + ">passive", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		public bool LTFadvanced = false;
		public bool SecondInputSelection = false;
		private void ChkAdvTF_CheckedChanged(object sender, EventArgs e) {
			if (ChkAdvTF.Checked) {
				LTFadvanced = true;
				CmdChooseInputSignal.Width = 80;
				CmdChoose2ndInputSignal.Visible = true;
				//frmDrivingOutput.optExcitOutput25.Visible = true;
				cmdChooseExcitation.Width = 80;
				FrameLTFaveraging.Visible = true;
			} else {
				LTFadvanced = false;
				CmdChooseInputSignal.Width = 162;
				CmdChoose2ndInputSignal.Visible = false;
				//frmDrivingOutput.optExcitOutput25.Visible = true;
				cmdChooseExcitation.Width = 162;
				FrameLTFaveraging.Visible = false;
			}
		}

		/**********************************************************************************************************************/
		private void CmdChoose2ndInputSignal_Click(object sender, EventArgs e) {
			SecondInputSelection = true;
			if (frmSensorInput == null) {
				frmSensorInput = new frmSensorInput();
				frmSensorInput.Show();
			} else {
				frmSensorInput.Show();
				frmSensorInput.Focus();
			}
		}


		public const int TEST_LTF_ARRAY_LENGTH = 200;
		private bool PhaseReversed = false;
		public double[] Predicted_Mag = new double[TEST_LTF_ARRAY_LENGTH];
		public double[] Predicted_Phase = new double[TEST_LTF_ARRAY_LENGTH];

		//public void Calc_Prediction() in filters.cs
		//{
		//    int freq_pt;
		//    double prev_freq = 0; ;
		//    double freq_sum_for_test;
		//    freq_sum_for_test = 0;
		//    for (freq_pt = 0; freq_pt < TEST_LTF_ARRAY_LENGTH; freq_pt++)
		//    {
		//        if (frmFilters.Reference_Freq_data[freq_pt] > 0)
		//        {
		//            prev_freq = frmFilters.Reference_Freq_data[freq_pt];
		//        }
		//        frmFilters.Freq_points[freq_pt] = prev_freq;
		//        freq_sum_for_test += prev_freq;
		//    }
		//    freq_pt -= 1;
		//    if (freq_sum_for_test == 0)
		//    {
		//        MessageBox.Show("Need reference for prediction" + "\r\n" + "Please load Reference Transfer Function" + "\r\n" + "Use 'Get Ref Plot' button", "Reference plot not loaded", MessageBoxButtons.OK);
		//    }
		//    frmFilters.CalculateAxis_TF(PhaseReversed);
		//    for (freq_pt = 0; freq_pt < TEST_LTF_ARRAY_LENGTH; freq_pt++)
		//    {
		//        Predicted_Mag[freq_pt] = frmFilters.Reference_Gain_data[freq_pt] + Program.Axis_Mag[freq_pt];
		//        Predicted_Phase[freq_pt] = frmFilters.Phase_Limit_TF_plus_minus_180((frmFilters.Reference_Phase_data[freq_pt] + Program.Axis_Phase[freq_pt]));
		//    }
		//    CWGraph.Plots[frmFilters.Prediction_TF_Num].PlotXY(frmFilters.Freq_points, Predicted_Mag);
		//    CWGraphPhase.Plots[frmFilters.Prediction_TF_Num].PlotXY(frmFilters.Freq_points, Predicted_Phase);
		//    CWGraph.Plots[frmFilters.Prediction_TF_Num].Visible = true;
		//    CWGraphPhase.Plots[frmFilters.Prediction_TF_Num].Visible = true;
		//}

		/**********************************************************************************************************************/
		private void cmdPrint_Click(object sender, EventArgs e) {
			Bitmap bmpScreenshot = new Bitmap(this.Bounds.Width, this.Bounds.Height, PixelFormat.Format32bppArgb);
			// Create a graphics object from the bitmap
			Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
			// Take the screenshot from the upper left corner to the right bottom corner
			gfxScreenshot.CopyFromScreen(this.Bounds.X, this.Bounds.Y, 0, 0, this.Bounds.Size, CopyPixelOperation.SourceCopy);

			string DefaultFileName = DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss") + " TransferFunctionTest_Print";
			SaveFileDialog saveImageDialog = new SaveFileDialog();
			saveImageDialog.Title = "Select output file:";
			saveImageDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
			saveImageDialog.FileName = DefaultFileName;
			if (saveImageDialog.ShowDialog() == DialogResult.OK) {
				// Save the screenshot to the specified path that the user has chosen
				bmpScreenshot.Save(saveImageDialog.FileName, ImageFormat.Png);
			}
		}

		/**********************************************************************************************************************/
		private void ChkReversePhase_CheckedChanged(object sender, EventArgs e) {
			if (ChkReversePhase.Checked) {
				PhaseReversed = true;
			} else {
				PhaseReversed = false;
			}
			Update_Previous_Plot_Data();
			Update_Performance_Plots();
			frmFilters.Calc_Prediction();
		}

		/**********************************************************************************************************************/
		frmFilters frmFilters;
		private void CmdFilters0_Click(object sender, EventArgs e) {
			OpenFiltersForm();
		}
		private void ImageFilters_Click(object sender, EventArgs e) {
			OpenFiltersForm();
		}
		private void LblFilters_Click(object sender, EventArgs e) {
			OpenFiltersForm();
		}

		/**********************************************************************************************************************/
		private void OpenFiltersForm() {
			if (frmFilters == null) {
				frmFilters = new frmFilters();
				frmFilters.Show();
			} else {
				frmFilters.Show();
				frmFilters.Focus();
			}
			// frmFilters.Show(this);
		}

		/**********************************************************************************************************************/
		private void PerformanceMenuIcon_Click(object sender, EventArgs e) {
			MenuPageChosenPERFORMANCE();
		}

		/**********************************************************************************************************************/
		private void CommandDiagRefresh_Click(object sender, EventArgs e) {
			GetSysData();
			read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		private void CmboxExcitAxis_SelectedIndexChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)
				return;

			string resp;
			string ControllerCommand = "drvo=" + cmboxExcitAxis.SelectedIndex.ToString();
			SendInternal(ControllerCommand, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cmdUpdateFirmware_Click(object sender, EventArgs e) {
			Update_Help();
		}

		/**********************************************************************************************************************/
		private void LblPOSmonitoring_Click(object sender, EventArgs e) {
			OpenCloseMonitoringForm();
		}

		/**********************************************************************************************************************/
		private void Update_Help() {
			string appPath;
			SavedMonitoringStatus = ContinuousMonitoringOn;
			SetContinuousMonitoring(false);
			string resp;
			Program.ConnectedController.CommStatus = CommStatusStates.Connected;

			if (Program.FormSplash.Windows_is_64bit) {
				appPath = @"C:\Program Files (x86)\C2Prog\";
			} else {
				appPath = @"C:\Program Files\C2Prog\";
			}
			appPath = appPath + "C2Prog.exe";
			if (!File.Exists(appPath)) {
				MessageBox.Show("CodeSkin Firmware updater 'C2Prog' is not found at" + "\r\n" + appPath + "\r\n" + "Please install from www.CodeSkin.com ");
				return;
			}
			string title = "How To Update Firmware - READ ALL before dismissing!";
			string message = "You should 'Retrieve and Save' Parameters before upgrading, if not - press 'Cancel' and do save" + "\r\n";
			if (SystemFWversion.Contains("bld"))
				message = message + "Upgrade is possible via Front or Rear USB type A ports. DO NOT unplug USB cable !" + "\r\n";
			else
				message = message + "Upgrade is possible via Front micro USB. If micro USB cable is plugged, unplug it!" + "\r\n";
			message = message + "If you click OK, COM port becomes 'DISconnected' to prepare" + "\r\n" +
			"for upgrade, and C2Prog will start" + "\r\n" +
			"Now press 'OK' to continue update process or 'Cancel' to quit";
			DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.OKCancel);
			if (result == DialogResult.OK) {
				System.Diagnostics.Process proc = new System.Diagnostics.Process();
				proc.EnableRaisingEvents = false;
				proc.StartInfo.FileName = appPath;
				proc.Start();
				if (SystemFWversion.Contains("bld")) {
					SendInternal("boot load", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
					message = "1. C2Prog.exe' updater has been started, select new firmware image '1st_Upgrade*.ehx' file" + "\r\n";
					message += "2. On 'C2Prog' screen press 'Configure Ports', in new window press 'Scan Ports', " + "\r\n";
					message += "3. From drop down list select COM # Analyzer was connected to" + "\r\n";
					message += "4. Hit 'OK' to return to C2Prog main window" + "\r\n";
					message += "5. On 'C2Prog' screen, press the 'Program' button, then immediately" + "\r\n";
					message += "6. On DC-2020, turn off POWER switch, turn it ON. LED might blink RED" + "\r\n";
					message += "7. The 'C2Prog' should start programming, DC-2020 LED will blink yellow" + "\r\n";
					message += "8. Wait while 'C2Prog' informs you: 'erasing'; 'downloading'; 'verifying'" + "\r\n";
					message += "9. When programming is done, choose 2nd firmware image '2nd_Upgrade*.ehx' file" + "\r\n";
					message += "10. On 'C2Prog' screen, press the 'Program' button" + "\r\n";
					message += "11. When programing has been finished, controller should restart automatically" + "\r\n";
					message += "12. After pressing 'OK' on this dialog, then press 'Read and Restore Parameters'" + "\r\n";
					message += "13. Parameters will be restored in RAM, do not forget to save them to FLASH" + "\r\n";
					message += "14. Now press 'OK' to dismiss this screen";
				} else {
					SendInternal("restart", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
					message = "1. C2Prog.exe' updater had started, select new firmware image - ' * 1st.ehx' file" + "\r\n";
					message += "2. On DC-2020 turn off POWER switch, press and HOLD the UPGRADE button" + "\r\n";
					message += "3. While HOLDING DOWN the UPGRADE button, turn on DC-2020" + "\r\n";
					message += "4. RELEASE the UPGRADE button, connect micro-USB cable" + "\r\n";
					message += "5. Using Device Manager find new 'FTDI' COM port note COM number #" + "\r\n";
					message += "6. On 'C2Prog' screen press 'Configure Ports', in new window press 'Scan Ports', " + "\r\n";
					message += "7. Select that COM # from drop down list. Hit 'OK' to return to C2Prog window" + "\r\n";
					message += "8. On 'C2Prog' screen, press the 'Program' button" + "\r\n";
					message += "9. Wait while 'C2Prog' informs you: 'erasing'; 'downloading'; 'verifying'" + "\r\n";
					message += "10. When 1st file programming is done, choose firmware image - '*2nd.ehx' file" + "\r\n";
					message += "11. Press the 'Program' button" + "\r\n";
					message += "12. When programing has been finished, DC-2020 should restart automatically" + "\r\n";
					message += "13. After pressing 'OK' on this dialog, then press 'Read and Restore Parameters'" + "\r\n";
					message += "14. Parameters will be restored in RAM, do not forget to save them to FLASH" + "r\n";
					message += "15. Now press 'OK' to dismiss this screen";
				}
				MessageBox.Show(message, title, MessageBoxButtons.OK);
			}
		}

		/**********************************************************************************************************************/
		private void ListParamsErrors_SelectedIndexChanged(object sender, EventArgs e) {
			int index = 0;
			string selectedLine = string.Empty;
			if (ListParamsErrors.SelectedItem != null) {
				selectedLine = ListParamsErrors.SelectedItem.ToString();
				if (selectedLine.Contains("Error on Line") || selectedLine.Contains("Warning on Line")) {
					selectedLine = Regex.Replace(selectedLine, @"\D", "");  //Remove alphabets

					if (!string.IsNullOrEmpty(selectedLine)) {
						if (selectedLine.Length == 4 || selectedLine.Length > 4) {
							index = Convert.ToInt32(selectedLine.Substring(0, 4));
							if (ListShadow.Items.Count > index)
								ListShadow.SetSelected(index, true);
						}
					}
				} else {
					if (ListShadow.Items.Count > 0)
						ListShadow.SetSelected(0, true);
				}
			}
		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicator0_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e) {
			//if (NIstatusBitIndicator0.Value) {
			//	NIstatusBitIndicator0.Caption = "DOCKED";
			//	NIstatusBitIndicator0.OffColor = ColorTranslator.FromHtml("#c0c0ff");
			//} else {
			//	NIstatusBitIndicator0.Caption = "Floating";
			//	NIstatusBitIndicator0.OffColor = Color.Lime;
			//}
			//NIstatusBitIndicator0.Update();
		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicator1_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e) {
			//if (NIstatusBitIndicator1.Value) {
			//	NIstatusBitIndicator1.Caption = "Status: NoGO";
			//	NIstatusBitIndicator1.OffColor = ColorTranslator.FromHtml("#c0c0ff");
			//} else {
			//	NIstatusBitIndicator1.Caption = "Status: GO";
			//	NIstatusBitIndicator1.OffColor = Color.Lime;
			//}
			//NIstatusBitIndicator1.Update();
		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicator2_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e) {
			//if (NIstatusBitIndicator2.Value) {
			//	NIstatusBitIndicator2.Caption = "LM safety: Recover";
			//	NIstatusBitIndicator2.OffColor = ColorTranslator.FromHtml("#c0c0ff");
			//} else {
			//	NIstatusBitIndicator2.Caption = "LM safety: NORM";
			//	NIstatusBitIndicator2.OffColor = Color.Lime;
			//}
			//NIstatusBitIndicator2.Update();
		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicator4_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e) {
			//NIstatusBitIndicator4.CreateControl(); //.CreateGraphics(); //CreateControl()
			//if (	NIstatusBitIndicator4.Value) {
			//	NIstatusBitIndicator4.Caption = "Disabled";
			//	NIstatusBitIndicator4.OffColor = ColorTranslator.FromHtml("#c0c0ff");
			//} else {
			//	NIstatusBitIndicator4.Caption = "ON";
			//	NIstatusBitIndicator4.OffColor = Color.Lime;
			//}
			//NIstatusBitIndicator4.Update();
		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicator5_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e) {
			//if (NIstatusBitIndicator5.Value) {
			//	NIstatusBitIndicator5.Caption = "Holding";
			//	NIstatusBitIndicator5.OffColor = ColorTranslator.FromHtml("#c0c0ff");
			//} else {
			//	NIstatusBitIndicator5.Caption = "FREE";
			//	NIstatusBitIndicator5.OffColor = Color.Lime;
			//}
			//NIstatusBitIndicator5.Update();
		}

		/**********************************************************************************************************************/
		private void NIstatusBitIndicator6_StateChanged(object sender, NationalInstruments.UI.ActionEventArgs e) {
			//if (NIstatusBitIndicator6.Value) {
			//	NIstatusBitIndicator6.Caption = "Doing Test: YES";
			//	NIstatusBitIndicator6.OffColor = ColorTranslator.FromHtml("#c0c0ff");
			//} else {
			//	NIstatusBitIndicator6.Caption = "Doing Test: No";
			//	NIstatusBitIndicator6.OffColor = Color.Lime;
			//}
			//NIstatusBitIndicator6.Update();
		}

		/**********************************************************************************************************************/
		private void ToggleAllFF_ValueChange(object sender, EventArgs e) {
			if (ToggleAllFF.Checked)
				ToggleAllFF.BackgroundImage = Resources.ON_blue_slider;
			else
				ToggleAllFF.BackgroundImage = Resources.OFF_gray_slider;
			if (Program.IsReadingControllerSettings)
				return;
			string resp;
			if (ToggleAllFF.Checked)
				SendInternal(ToggleAllFF.Tag + ">active", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			else
				SendInternal(ToggleAllFF.Tag + ">passive", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			read_RT_diagnostic();
		}

		/**********************************************************************************************************************/
		private void lblProxSensorClick(object sender, EventArgs e) {
			if (!ToggleRTmonitoring.Checked)
				Read_Position_Sensors();
		}

		/**********************************************************************************************************************/
		private void NumEdit_UpButtonClicked(object sender, EventArgs e) {
			SetTOMaxValue((NumericEdit)sender, false);
		}

		/**********************************************************************************************************************/
		int count = 0;
		public void SetTOMaxValue(NumericEdit numeric, bool IsKeyboard) {
			if ((numeric.Range.Maximum - numeric.Value) < numeric.CoercionInterval && (numeric.Range.Maximum - numeric.Value) != 0) {
				if (IsKeyboard) {
					count++;
					if (count == 2) {
						count = 0;
						numeric.Value = numeric.Range.Maximum;
					}
				} else
					numeric.Value = numeric.Range.Maximum;
			}
		}

		/**********************************************************************************************************************/
		private void NumEdit_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyData == Keys.Up)
				SetTOMaxValue((NumericEdit)sender, true);
		}

		/**********************************************************************************************************************/
		private void LblFB_FF_Gains_Click(object sender, EventArgs e) {
			OpenCloseGainsForm();
		}

		/**********************************************************************************************************************/
		private void ImageGains_Click(object sender, EventArgs e) {
			OpenCloseGainsForm();
		}

		private frmFloorFF FrmFloorFF;
		private formGains FormGains;
		private void FrmFloorFF_FormClosed(object sender, FormClosedEventArgs e) {
			// release reference so next time a new instance will be created
			FrmFloorFF.FormClosed -= FrmFloorFF_FormClosed;
			FrmFloorFF = null;
		}

		private void FrmGains_FormClosed(object sender, FormClosedEventArgs e) {
			FormGains.FormClosed -= FrmGains_FormClosed;
			FormGains = null;
		}
		/**********************************************************************************************************************/
		public void OpenCloseGainsForm(bool isVisible = true) {
			if (HasFloorFF) {
				if (/*form already active?*/ Program.FrmFloorFF != null && !Program.FrmFloorFF.IsDisposed) {
					if (!Program.FrmFloorFF.Visible && !Program.FrmFloorFF.FormTerminalVisible ) {
						LblFB_FF_Gains.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
						LblFB_FF_Gains.ForeColor = Color.Maroon;
						Program.FrmFloorFF.Show();
						Program.FrmFloorFF.Visible = true;
						Program.FrmFloorFF.FormTerminalVisible = true;
						//Program.FormGains.Refresh_gains();
						return;
					} else {
						LblFB_FF_Gains.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
						LblFB_FF_Gains.ForeColor = Color.Black;
						Program.FrmFloorFF.Hide();
						Program.FrmFloorFF.FormTerminalVisible = false;
					}
				} else { // form was not created or was disposed
					LblFB_FF_Gains.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
					LblFB_FF_Gains.ForeColor = Color.Maroon;
					Program.FrmFloorFF = new frmFloorFF(this);
					Program.FrmFloorFF.Show();
					Program.FrmFloorFF.Visible = true;
					Program.FrmFloorFF.FormTerminalVisible = true;
				}
				} else {
				if (/*form already active?*/ Program.FormGains != null && !Program.FormGains.IsDisposed) {
					if (!Program.FormGains.Visible && !Program.FormGains.FormTerminalVisible) {
						LblFB_FF_Gains.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
						LblFB_FF_Gains.ForeColor = Color.Maroon;
						Program.FormGains.Show();
						Program.FormGains.Visible = true;
						Program.FormGains.FormTerminalVisible = true;
						//Program.FormGains.Refresh_gains();
						return;
					} else {
						LblFB_FF_Gains.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
						LblFB_FF_Gains.ForeColor = Color.Black;
						Program.FormGains.Hide();
						Program.FormGains.FormTerminalVisible = false;
					}
				} else {// form was not created or was disposed
					LblFB_FF_Gains.Font = new Font("Tahoma", 9.0F, FontStyle.Bold);
					LblFB_FF_Gains.ForeColor = Color.Maroon;
					Program.FormGains = new formGains(this);
					Program.FormGains.Show();
					Program.FormGains.Visible = true;
					Program.FormGains.FormTerminalVisible = true;
				}
			}
		}

		/**********************************************************************************************************************/
		//ScopeIntegration_01082018_Tismo
		Thread ScopeThread = null;
		private void CmdScope_Click(object sender, EventArgs e) {
			ScopeFormOpen();
			//timer_ScopeHiddenCheck.Enabled = true;  //start the timer to check when scope is hidden.
		}

		/**********************************************************************************************************************/
		//private void timer_ScopeHiddenCheck_Tick(object sender, EventArgs e) {
		//	if (TMC_Scope.Program.IsScopeFormHidden) {
		//		timer_ScopeHiddenCheck.Enabled = false;
		//		ScopeThread.Suspend();
		//	}
		//}

		private void splitContainerPerformance_Panel1_ClientSizeChanged(object sender, EventArgs e) {
			update_num_mindB_position();
		}

		/**********************************************************************************************************************/
		/**********************************************************************************************************************/
		private void splitContainerPerformance_Panel1_SizeChanged(object sender, EventArgs e) {
			update_num_mindB_position();
		}
		public void update_num_mindB_position() {
			var Ymax_X_location = splitContainerPerformance.Panel1.Location.X + 2;
			var Ymax_Y_location = splitContainerPerformance.Panel1.Location.Y + 20;
			var Ymin_X_location = splitContainerPerformance.Panel1.Location.X + 2;
			var Ymin_Y_location = splitContainerPerformance.Panel1.Location.Y + splitContainerPerformance.Panel1.Size.Height - num_MindB.Size.Height - 15;
			if (Ymax_X_location < 1)
				Ymax_X_location = 1;
			if (Ymin_X_location < 1)
				Ymin_X_location = 1;
			if ((num_MaxdB.IsHandleCreated == true) && (num_MindB.IsHandleCreated == true))
				this.Invoke((System.Action)(() => {
					num_MaxdB.Location = new System.Drawing.Point(Ymax_X_location, Ymax_Y_location);
					num_MindB.Location = new System.Drawing.Point(Ymin_X_location, Ymin_Y_location);
				}));
		}

		/**********************************************************************************************************************/
		public double RectifyValueResponse(string cmd) {
			string temp_str;
			string retval;
			int token_pos;
			double returnVal = WRONG_VALUE;
			SendInternal(cmd, CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			token_pos = temp_str.IndexOf(">~U");  //' ">~Unrecognized Cmd, Error Code =1"
			if (token_pos >= 0)
				return returnVal;
			token_pos = temp_str.IndexOf( ">~E"); //' ">~ER drvo Param, Error Code =2  drvo=0"
			if (token_pos >= 0) {
				token_pos = temp_str.IndexOf("drvo=");   //' ">~ER drvo Param, Error Code =2  drvo=0"
				if (token_pos >= 0) {
					temp_str = temp_str.Substring(token_pos);
				} else
					return returnVal;
			}
			token_pos = temp_str.IndexOf(">~D");  //' "freq=+2.5\r\n>~Doing CMD\r\n
			if (token_pos >= 0) {
				temp_str = temp_str.Substring(0, token_pos - 1);  // ' "freq=+2.5\r\n
			}
			token_pos = temp_str.IndexOf("=");  //'find value which starts after "="
			if (token_pos >= 0) {
				retval =temp_str.Substring(token_pos + 1);
				token_pos = retval.IndexOf("//");
				if (token_pos >= 0)
					retval = retval.Substring(0, token_pos - 1);
				if (IsNumeric(retval))
					returnVal = Convert.ToDouble(retval);
			}
			return returnVal;
		}

		/**********************************************************************************************************************/
		private bool get_ID() {
			string temp_str;
			bool getID = false;
			SendInternal("version?", CommandTypes.ResponseExpected, out temp_str, CommandAckTypes.AckExpected);
			if (temp_str != "Timeout") {
				if (temp_str.Contains( "95-")) {
					txtFW_Version.Text = temp_str;
					getID = true;
				} else {
					getID = false;
					return getID;
				}
			}
			ID_valid = getID;
			if (getID)
				txtFW_Version.Text = temp_str;
			else
				txtFW_Version.Text = "No or not valid response";
			SystemFWversion = txtFW_Version.Text;
			if (SystemFWversion.ToUpper().Contains("PONG")) {
				HasFloorFF = true;
				ChkAirIsolatorsPresent.Checked = false;    //' IK20251112 NO AIR auto set based on the presence of "PONG" in version string
			} else {
				HasFloorFF = false;
				ChkAirIsolatorsPresent.Checked = true;    //' IK20251112 NO AIR auto set based on the presence of "PONG" in version string
			}


			txtFW_Version.Update();
			return getID;
		}

		private void ChkShowCoherencePlot_CheckedChanged(object sender, EventArgs e)
		{
			if (ChkShowCoherencePlot.Checked)
				ScatterGraphPhase.Plots[CohLTF].Visible = true;
			else
				ScatterGraphPhase.Plots[CohLTF].Visible = false;
		}

		private void NumMaxFreq_ValueChanged(object sender, EventArgs e) {
		}

		private void NumMaxFreq_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {
			if (NumMaxFreq.Value <= 500)
				NumMaxFreq.CoercionInterval = 100;
			if (NumMaxFreq.Value == 600) {
				NumMaxFreq.Value = 1000;
				NumMaxFreq.CoercionInterval = 500;
			}
			ScatterGraphMag.XAxes[0].Range = new NationalInstruments.UI.Range(0.1, NumMaxFreq.Value);
			ScatterGraphPhase.XAxes[0].Range = new NationalInstruments.UI.Range(0.1, NumMaxFreq.Value);
		}

		public void CmdSetPressurePoints() {
			string gain_cmd = BtnSetPressurePoints.Tag.ToString();
			SendInternal(gain_cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
			Program.IsReadingControllerSettings = true; // prevent sending commands back to controller, just read and update controls with new values
			int max_indx = NInumPressureSetPoint.Count;
			for (int i = 0; i < max_indx; i++) {
				string controller_param;
				double paramValue;
				SendInternal(NInumPressureSetPoint[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (Tools.UpdateValueFromResponse(controller_param, out paramValue))
					NInumPressureSetPoint[i].Value = SystemPressures[i + 1] = paramValue;
			}
			Program.IsReadingControllerSettings = false;

		}

		private void CmdSetPressurePoints_Click(object sender, EventArgs e) {
			CmdSetPressurePoints();
		}

		private void NInumFBgain0_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {

		}


#if (false) // for ELDAMP
		//=== ALARMS
		bool AlarmMonitoringOn = false;// This is internal boolean that tracks whether alarm monitoring is ON or OFF.

		// SET ALARM "LED" LIGHT  COLORS
		private System.Drawing.Color LEDColorAlarmOn = System.Drawing.Color.Red;
		private System.Drawing.Color LEDColorAlarmCleared = System.Drawing.Color.Lime;
		private System.Drawing.Color LEDColorAlarmNotMonitoring = System.Drawing.Color.Gainsboro;
		private void PictureBox_AlarmMonitoringSwitch_Click(object sender, EventArgs e) {
			AlarmMonitoringOn = !AlarmMonitoringOn;
		}

		/**********************************************************************************************************************/
		private void CmdClearAlarms_Click(object sender, EventArgs e) {
			if ( Program.IsReadingControllerSettings)//busy?
				return;
			string retVal = string.Empty;
			SendInternal("alarm>clear", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwBut_LF_alarm_check_CheckedChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)//busy?
				return;
			string retVal = string.Empty;

			if (((CheckBox)sender).Checked)
				SendInternal("alarmL>on", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
			else
				SendInternal("alarmL>off", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwButSaturationControl_CheckedChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)//busy?
				return;
			string retVal = string.Empty;

			if (((CheckBox)sender).Checked)
				SendInternal("alarmS>on", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
			else
				SendInternal("alarmS>off", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwBut_HF_alarm_check_CheckedChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)//busy?
				return;
			string retVal = string.Empty;

			if (((CheckBox)sender).Checked)
				SendInternal("alarmH>on", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
			else
				SendInternal("alarmH>off", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void cwButOscillationControl_CheckedChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings)//busy?
				return;
			string retVal = string.Empty;

			if (((CheckBox)sender).Checked)
				SendInternal("alarmO>on", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
			else
				SendInternal("alarmO>off", CommandTypes.ResponseExpected, out retVal, CommandAckTypes.AckExpected);
		}

		/**********************************************************************************************************************/
		private void CmdRunSelfTest_Click(object sender, EventArgs e) {
			// //"test pair"
			long c;
			ToggleRTmonitoring.Checked = false;
			ContinuousMonitoringOffOn(false);
			FrameDoingTest.Visible = true;
			frameAlarmDisplay.Visible = false;
			SelfTest("test pair", 1);
			//  10 sec per line
			frameAlarmDisplay.Visible = true;
			Read_Alarm_Status();
			if ((self_test_word == 0)) {
				PostResult.Checked = true;
			} else {
				PostResult.Checked = false;
			}
		}
#endif //#if (false) for ELDAMP
		/**********************************************************************************************************************/
		//private void buttonConnect1_CheckedChanged(object sender, EventArgs e)
		//{
		//    Connect_Or_Disconnect(buttonConnect1.Checked);
		//}

		//private void read_RT_diagnostic()
		//{
		//    //  decodes 4 lines from the 'brif' command
		//    // Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI
		//    // Vrt um -7746 -7746 -7746  FF_is ON  X- Y-          tZ  Iso1 Iso2 Iso3 Iso4 Inp
		//    // Hrz um +6937 +6937 -6934 Excit=NONE   MotPWR   0%  PSI  0.6  0.6  0.5  0.5  0.7
		//    //                                  BncO=Zpos_mm       mA  8.0  8.0  8.0  8.0
		//    //  or when floating normally
		//    // Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: ACTIVE      ~~floating~~ OK
		//    // Vrt um +9    +9    +7     FF_is ON                 tZ  Iso1 Iso2 Iso3 Iso4 Inp
		//    // Hrz um -7    +14   -6    Excit=Pulse  MotPWR 100%  PSI 11.3 12.0 11.2 11.6 36.3
		//    //                                  BncO=GATZ          mA 74.1 75.6 78.9 77.5
		//    //  or when excitation is working
		//    // Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI
		//    // Vrt um -7746 -2323 -3333  FF_is ON  X- Y- Z- tX tY tZ    Rel_dB = +21.31
		//    // Hrz um +6937 +6937 -6933 Excit=Sine   MotPWR   0%        Rel_deg= +90.5
		//    //                                  BncO=Zpos_mm            Bnc1=p2Z_10V/mm
		//    // TODO: On Error Resume Next Warning!!!: The statement is not translatable
		//    //long ch;
		//    //bool tmp_GetSysData;
		//    //long token_pos;
		//    string brif_msg;
		//    string RCIcommand;
		//    //string tmpDataStr;
		//    //string tmp_str;
		//    //long separator;
		//    ////  position of symbol ','
		//    //long str_len;
		//    bool excitation_is_working;
		//    RCIcommand = "brif";
		//    //cmd_sent = false;
		//    //waitresponse = true;
		//    if ((ConnectionType == ConnDEMO))
		//    {
		//        if ((Excitation_status == true))
		//        {
		//            brif_msg = create_test_brif(3);
		//        }
		//        else
		//        {
		//            brif_msg = create_test_brif(2);
		//        }

		//    }
		//    else
		//    {
		//      //  GUI_doing_command = true;
		//      //  brif_msg = Analyzer.GetSend(RCIcommand, true);
		//        SendInternal(RCIcommand, CommandTypes.ResponseExpected, out brif_msg, CommandAckTypes.AckExpected);
		//    }

		//    if ((brif_msg.Contains("meout")))
		//    {
		//        return;
		//    }

		//    // "Timeout" sometimes seen as "imeout"
		//    txtRTdiag.Text = brif_msg;
		//}

		//string create_test_brif(int test)
		//{
		//    string retVal = "";
		//    if ((test == 1))
		//    {
		//        retVal = ("Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI" + ("\r\n" + ("Vrt um -7746 -7746 -7746  FF_is ON  X- Y-          tZ  Iso1 Iso2 Iso3 Iso4 Inp" + ("\r\n" + ("Hrz um +6937 +6937 -6934 Excit=NONE   MotPWR   0%  PSI  0.6  0.6  0.5  0.5  0.7" + ("\r\n" + ("                                 BncO=Zpos_mm       mA  8.0  8.0  8.0  8.0" + "\r\n")))))));
		//    }

		//    if ((test == 2))
		//    {
		//        retVal = ("Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: ACTIVE      ~~floating~~ OK" + ("\r\n" + ("Vrt um +9    +9    +7     FF_is ON                 tZ  Iso1 Iso2 Iso3 Iso4 Inp" + ("\r\n" + ("Hrz um -7    +14   -6    Excit=Pulse  MotPWR 100%  PSI 11.3 12.0 11.2 11.6 36.3" + ("\r\n" + ("                                 BncO=GATZ          mA 74.1 75.6 78.9 77.5" + "\r\n")))))));
		//    }

		//    if ((test == 3))
		//    {
		//        retVal = ("Prox:  Iso#1 Iso#2 Iso#3  Lin_Motor_FB_Axes: SafetyOFF TooLowInpPSI" + ("\r\n" + ("Vrt um -46   -29   +71    FF_is ON  X- Y- Z- tX tY tZ    Rel_dB = +21.31" + ("\r\n" + ("Hrz um +37   +6    -93   Excit=Sine   MotPWR   0%        Rel_deg= +90.5" + ("\r\n" + ("                                 BncO=Zpos_mm            Bnc1=p2Z_10V/mm" + "\r\n")))))));
		//    }
		//    return retVal;
		//}

		//public void Clean_COM_Buffer()
		//{
		//    Get_Serial_Input();
		//    Buffer = "";
		//}

		//void GetSysData()
		//{
		//    string temp_str;
		//    //     Dim monitor_stat As Boolean
		//    bool cont_field_monitoring;
		//    Init_sys_data = true;
		//    frmTerminal.QuitMenu;
		//    Read_Alarm_Status();
		//    //  updates M3v_Diagnostic_running =
		//    //      NO_TESTS                   = 0 // no test
		//    //     ,LOGICAL_MotSens_TEST_Table = 1 // motor-sensor test: inject into one LOGICAL output control_signal[], see InGeo[] or InProx[],  PTR_FLOAT
		//    //     ,DAC_TO_ADC_loopback_TEST   = 2 // LOOP-BACK DB-37 SOCKETS REQUIRED! DAC-to-ADC loop back test, index the same, i.e. drive DAC[0], look at ADC[0], PTR_INT16
		//    //     ,PHYSICAL_PAIR_TEST         = 3 // DAC[d_index] to ADC[a_index] index to different index, i.e. drive DAC[6], look at ADC[15], PTR_INT16
		//    //     ,MotorSensorTEST_List       = 4 // for "test pair" command, output as a list
		//    //     ,EXCITATION_TO_INT16 = 5
		//    if (M3v_Diagnostic_running)
		//    {
		//        temp_str = GetSend("oltf>stop", false);
		//        // cancel oltf just in case
		//    }

		//    //     If ID_valid = False Then
		//    if ((ID_valid == false))
		//    {
		//        ID_valid = get_ID();
		//    }

		//    //  try again, if fails THIS TIME, exit
		//    if ((ID_valid == false))
		//    {
		//        return;
		//    }

		//    // do not try if there is no proper response on "p" or "vers"
		//    GUI_doing_command = false;

		//}

		//long Read_Alarm_Status()
		//{
		//    // TODO: On Error GoTo Warning!!!: The statement is not translatable
		//    //  * RCI command: "exst"
		//    //  * Emits extended status in ascii-hex, to stdout.
		//    //  * Output: 11111111,22222222,33333333,4
		//    //  *      where   11111111 is 32-bit M3v.Cmd_Word
		//    //  *              22222222 is 32-bit M3v.AlarmWord
		//    //  *              33333333 is 32-bit Prod_Test_BITS
		//    //  *              4        is M3v.Diagnostic_running (non-zero when test selected)
		//    //  updates M3v_Diagnostic_running =
		//    //      NO_TESTS                   = 0 // no test
		//    //     ,LOGICAL_MotSens_TEST_Table = 1 // motor-sensor test: inject into one LOGICAL output control_signal[], see InGeo[] or InProx[],  PTR_FLOAT
		//    //     ,DAC_TO_ADC_loopback_TEST   = 2 // LOOP-BACK DB-37 SOCKETS REQUIRED! DAC-to-ADC loop back test, index the same, i.e. drive DAC[0], look at ADC[0], PTR_INT16
		//    //     ,PHYSICAL_PAIR_TEST         = 3 // DAC[d_index] to ADC[a_index] index to different index, i.e. drive DAC[6], look at ADC[15], PTR_INT16
		//    //     ,MotorSensorTEST_List       = 4 // for "test pair" command, output as a list
		//    //     ,EXCITATION_TO_INT16 = 5
		//    //  * RCI command: "exst!" sets M3v.measurement_done = TRUE
		//    //  * RCI command: "exst=0x12345678"   sets M3v.AlarmWord = 0x12345678
		//    string alarm_status;
		//    long separator;
		//    long c;
		//    string tmpDataStr;
		//    string binFormat;
		//    //  ==============================================================================================
		//    //  ===============  get buttons states  =========================================================
		//    // If (cwBut_RT_Monitoring.Value = True) Then
		//    Init_sys_data = true;
		//    alarm_status = Analyzer.GetSend("post", true);
		//    // "post>enab, post>disa"
		//    // // "post>enab";">disa"  enables/disables Power On Self Test (POST)
		//    if ((alarm_status.IndexOf(">enab") + 1))
		//    {
		//        cwButEnDisPOSTtest(0).Value = true;
		//        cwButEnDisPOSTtest(1).Value = true;
		//    }
		//    else
		//    {
		//        cwButEnDisPOSTtest(0).Value = false;
		//        cwButEnDisPOSTtest(1).Value = false;
		//    }

		//    alarm_status = Analyzer.GetSend("alarmH", true);
		//    // "alalrmH>on, off>~Doing CMD"
		//    // //"alarmH>on";">off"  enables/disables High Frequency monitoring
		//    if ((alarm_status.IndexOf(">on") + 1))
		//    {
		//        cwBut_HF_alarm_check.Value = true;
		//    }
		//    else
		//    {
		//        cwBut_HF_alarm_check.Value = false;
		//    }

		//    // //"alarmL>on";">off"  enables/disables Low Frequency monitoring
		//    alarm_status = Analyzer.GetSend("alarmL", true);
		//    // "alalrmL>on, off>~Doing CMD"
		//    if ((alarm_status.IndexOf(">on") + 1))
		//    {
		//        cwBut_LF_alarm_check.Value = true;
		//    }
		//    else
		//    {
		//        cwBut_LF_alarm_check.Value = false;
		//    }

		//    // // "alarmO>on";">off"  enables/disables Oscillation Control monitoring
		//    alarm_status = Analyzer.GetSend("alarmO", true);
		//    // "alalrmO>on, off>~Doing CMD"
		//    if ((alarm_status.IndexOf(">on") + 1))
		//    {
		//        cwButOscillationControl.Value = true;
		//    }
		//    else
		//    {
		//        cwButOscillationControl.Value = false;
		//    }

		//    // // "alarmS>on";">off"  enables/disables Saturation Control monitoring
		//    alarm_status = Analyzer.GetSend("alarmS", true);
		//    // "alalrmS>on, off>~Doing CMD"
		//    if ((alarm_status.IndexOf(">on") + 1))
		//    {
		//        cwButSaturationControl.Value = true;
		//    }
		//    else
		//    {
		//        cwButSaturationControl.Value = false;
		//    }

		//    //  ==============================================================================================
		//    //  ===============  Read extended status word  ==================================================
		//    Read_Alarm_Status = 0;
		//    tmpDataStr = "exst";
		//    cmd_sent = false;
		//    waitresponse = true;
		//    alarm_status = Analyzer.GetSend(tmpDataStr, true);
		//    // "0x120F0008,0x00000000,0x00000000,0>~Doing CMD"
		//    separator = ((alarm_status.IndexOf(",") + 1)
		//                - 1);
		//    if ((separator > 0))
		//    {
		//        tmpDataStr = alarm_status.Substring(0, separator);
		//        //  32-bit M3v.Cmd_Word, "0x120F0008"
		//        tmpDataStr = ("&H" + tmpDataStr.Substring(2));
		//        //  32-bit M3v.Cmd_Word, "120F0008"
		//        if (IsNumeric(tmpDataStr))
		//        {
		//            //  CAN USE IsNumeric, because 'tmpDataStr' is integer
		//            M3v_Cmd_Word = double.Parse(tmpDataStr);
		//            //
		//        }

		//        tmpDataStr = alarm_status.Substring((separator + 3));
		//        //  remove "0x" and the piece of string from which we've converted information
		//    }
		//    else
		//    {
		//        Read_Alarm_Status = (Read_Alarm_Status - 1);
		//        //  adding -1 means error
		//    }

		//    alarm_status = tmpDataStr;
		//    separator = ((alarm_status.IndexOf(",") + 1)
		//                - 1);
		//    if ((separator > 0))
		//    {
		//        tmpDataStr = ("&H" + alarm_status.Substring(0, separator));
		//        //  32-bit M3v.AlarmWord
		//        if (IsNumeric(tmpDataStr))
		//        {
		//            //  CAN USE IsNumeric, because 'tmpDataStr' is integer
		//            AlarmWord = double.Parse(tmpDataStr);
		//            // As Long
		//            self_test_word = AlarmWord;
		//            binFormat = BIN((self_test_word & 255));
		//            self_test_word = (self_test_word / 256);
		//            binFormat = (BIN((self_test_word & 255)) + binFormat);
		//            self_test_word = (self_test_word / 256);
		//            binFormat = (BIN((self_test_word & 255)) + binFormat);
		//            self_test_word = (self_test_word / 256);
		//            binFormat = (BIN((self_test_word & 255)) + binFormat);
		//            txtStatus.Text = ("AlarmWord = "
		//                        + (tmpDataStr + (" " + binFormat)));
		//            Show_Alarm_flags();
		//            if ((AlarmWord == 0))
		//            {
		//                cwIndicatorAlarm.Value = false;
		//            }
		//            else
		//            {
		//                cwIndicatorAlarm.Value = true;
		//            }
		//        }

		//        tmpDataStr = alarm_status.Substring((separator + 3));
		//        //  remove "0x" and the piece of string from which we've converted information
		//    }
		//    else
		//    {
		//        Read_Alarm_Status = (Read_Alarm_Status - 1);
		//        //  adding -1 means error
		//    }

		//    alarm_status = tmpDataStr;
		//    separator = ((alarm_status.IndexOf(",") + 1)
		//                - 1);
		//    if ((separator > 0))
		//    {
		//        tmpDataStr = ("&H" + alarm_status.Substring(0, separator));
		//        //  32-bit Prod_Test_BITS
		//        if (IsNumeric(tmpDataStr))
		//        {
		//            //  CAN USE IsNumeric, because 'tmpDataStr' is integer
		//            self_test_word = double.Parse(tmpDataStr);
		//            // As Long
		//            tmpDataStr = alarm_status.Substring((separator + 1));
		//            //  remove the piece of string from which we've converted information
		//        }
		//        else
		//        {
		//            self_test_word = 0;
		//        }
		//    }
		//    else
		//    {
		//        Read_Alarm_Status = (Read_Alarm_Status - 1);
		//        //  adding -1 means error
		//    }

		//    if ((self_test_word != 0))
		//    {
		//        ProdTestStatus = false;
		//        PostResult.Value = false;
		//    }
		//    else
		//    {
		//        ProdTestStatus = true;
		//        PostResult.Value = true;
		//    }

		//    cwIndicatorSelfTest.Value = ProdTestStatus;
		//    //  ==============================================================================================
		//    //  ===============  Diagnostic running?  ========================================================
		//    // M3v_Diagnostic_running
		//    alarm_status = tmpDataStr;
		//    // the last integer
		//    tmpDataStr = alarm_status.Trim();
		//    //  32-bit (actually, about 10) M3v_Diagnostic_running
		//    if ((M3v_Diagnostic_running != 0))
		//    {
		//        FrameDoingTest.Visible = true;
		//    }
		//    else
		//    {
		//        FrameDoingTest.Visible = false;
		//    }

		//    //  ==============================================================================================
		//    //  =====================  Acting gains?  ========================================================
		//    //                channel name Z1 Z2 Z3 Z4  X1  X2 X3 X4  Y1 Y2 Y3 Y4
		//    //  returns percent of power "100 100 98 88 100 98 100 25 99 100 100 100"
		//    Get_axis_power();
		//}

		//public void Check_Selected_Piezo_Geo()
		//{
		//    string piezo_name;
		//    string geo_name;
		//    piezo_name = Program.LTFtestOutputName;
		//    geo_name = Program.LTFtestInputName;
		//    if (((piezo_name == "")
		//                && (geo_name != "")))
		//    {
		//        return;
		//    }

		//    //  user have not selected piezo
		//    if (((piezo_name != "")
		//                && (geo_name == "")))
		//    {
		//        return;
		//    }

		//    //  user have not selected geo
		//    if ((piezo_name.Substring(2) == geo_name.Substring(2)))
		//    {
		//        // M_Z1 and G_Z1
		//        if ((piezo_name.Substring(2, 1) == "X"))
		//        {
		//            ReadGoNoGoPlot("X");
		//        }
		//        else if ((piezo_name.Substring(2, 1) == "Y"))
		//        {
		//            ReadGoNoGoPlot("Y");
		//        }
		//        else if ((piezo_name.Substring(2, 1) == "Z"))
		//        {
		//            ReadGoNoGoPlot("Z");
		//        }
		//        else
		//        {
		//            ReadGoNoGoPlot("-");
		//            // this will cause hiding limit plots and message
		//        }

		//    }
		//    else
		//    {
		//        // names are different, like M_Z1, G_Y2
		//        ReadGoNoGoPlot("-");
		//        // this will cause hiding limit plots and message
		//    }
		//}

		//       void ReadGoNoGoPlot(string axis) {
		//     // TODO: On Error GoTo Warning!!!: The statement is not translatable
		//     // user wants to read a test file
		//     // First get the filename
		//     string DataFileName;
		//     string FileName;
		//     long i;
		//     string temp_str;
		//     long sep_pos;
		//     long line_read;
		//     //     SaveFileName = Left(dlgCommonDialog.FileName, Len(dlgCommonDialog.FileName) - 4) ' remove ".txt" from full file path
		//     FileName = Environment.CurrentDirectory;
		//     if ((((axis != "X")
		//                 && ((axis != "Y")
		//                 && (axis != "Z")))
		//                 || (optTestInput(0).Value == true))) {
		//         CWGraph.Plots[Lower_Lim_TF_Num].Visible = false;
		//         CWGraph.Plots[Upper_Lim_TF_Num].Visible = false;
		//         CWGraphPhase.Plots[Lower_Lim_TF_Num].Visible = false;
		//         CWGraphPhase.Plots[Upper_Lim_TF_Num].Visible = false;
		//         return;
		//     }

		//     if ((optTestInput1.Value == true)) {
		//         // OLTF test
		//         if ((axis == "Z")) {
		//             DataFileName = (FileName + "\\Z_OLTF_Go_NoGo.txt");
		//         }
		//         else if ((axis == "X")) {
		//             DataFileName = (FileName + "\\X_OLTF_Go_NoGo.txt");
		//         }
		//         else if ((axis == "Y")) {
		//             DataFileName = (FileName + "\\Y_OLTF_Go_NoGo.txt");
		//         }
		//     }

		//     if ((optTestInput2.Value == true)) {
		//         // plant TF test
		//         if ((axis == "Z")) {
		//             DataFileName = (FileName + "\\Z_Plant_Go_NoGo.txt");
		//         }
		//         else if ((axis == "X")) {
		//             DataFileName = (FileName + "\\X_Plant_Go_NoGo.txt");
		//         }
		//         else if ((axis == "Y")) {
		//             DataFileName = (FileName + "\\Y_Plant_Go_NoGo.txt");
		//         }
		//     }

		//     // SaveFileName becomes "C:\test\X_go_nogo.txt"
		//     DataFileName = DataFileName.ToLower();
		////     i = InStrRev(DataFileName, "\\");
		//  //   FileName = DataFileName.Substring((DataFileName.Length - (DataFileName.Length - i)));
		//           FileName = Path.GetFileName(DataFileName);
		//     // i.e. "my_test"
		//    // Close;
		//     // TODO: # ... Warning!!! not translated
		//    // Transfer_Function_file;
		//    // Open;
		//   //  DataFileName;
		//     for (object Input; ; Input++) {
		//         // TODO: # ... Warning!!! not translated
		//         Transfer_Function_file;
		//         // Values
		//         Line;
		//         Input;
		//         // TODO: # ... Warning!!! not translated
		//         Transfer_Function_file;
		//         temp_str;
		//         //     "File name,"; vbTab; FileName
		//         //  decode line
		//         sep_pos = (temp_str.IndexOf("go_") + 1);
		//         if ((temp_str.Substring((sep_pos - 1), 11) != "go_nogo.txt")) {
		//             goto error_format;
		//         }
		//         else {
		//             if ((sep_pos > 9)) {
		//                 txtStatus = temp_str.Substring((sep_pos - 6));
		//             }

		//             Line;
		//             Input;
		//             // TODO: # ... Warning!!! not translated
		//             Transfer_Function_file;
		//             temp_str;
		//             // Print #Transfer_Function_file, "Frequency, Hz"; vbTab; "Gain, dB"; vbTab; "Phase, deg"
		//             // .......add code ................
		//             line_read = -1;
		//             //  to start from zero
		//             while (!EOF(Transfer_Function_file)) {
		//                 //  Loop until end of file.
		//                 line_read = (line_read + 1);
		//                 Line;
		//                 Input;
		//                 // TODO: # ... Warning!!! not translated
		//                 Transfer_Function_file;
		//                 temp_str;
		//                 // Print #Transfer_Function_file,
		//                 //  decode line
		//                 sep_pos = (temp_str.IndexOf('\t') + 1);
		//                 //  Print #Transfer_Function_file, Format(Freq_data(i), "#0.00"); vbTab;
		//                 if (IsNumber(temp_str.Substring(0, (sep_pos - 1)))) {
		//                     Freq_Lim_data(line_read) = double.Parse(temp_str.Substring(0, (sep_pos - 1)));
		//                     temp_str = temp_str.Substring(sep_pos);
		//                 }
		//                 else {
		//                     goto error_format;
		//                 }

		//                 // Print #Transfer_Function_file, Format(Gain_Low_data(i), "#0.000"); vbTab;
		//                 sep_pos = (temp_str.IndexOf('\t') + 1);
		//                 if (IsNumber(temp_str.Substring(0, (sep_pos - 1)))) {
		//                     Gain_Low_data(line_read) = double.Parse(temp_str.Substring(0, (sep_pos - 1)));
		//                     temp_str = temp_str.Substring(sep_pos);
		//                 }
		//                 else {
		//                     goto error_format;
		//                 }

		//                 // Print #Transfer_Function_file, Format(Phase_Low_data(i), "#0.0")
		//                 sep_pos = (temp_str.IndexOf('\t') + 1);
		//                 if (IsNumber(temp_str.Substring(0, (sep_pos - 1)))) {
		//                     Phase_Low_data(line_read) = double.Parse(temp_str.Substring(0, (sep_pos - 1)));
		//                     temp_str = temp_str.Substring(sep_pos);
		//                 }
		//                 else {
		//                     goto error_format;
		//                 }

		//                 // Print #Transfer_Function_file, Format(Gain_High_data(i), "#0.000"); vbTab;
		//                 sep_pos = (temp_str.IndexOf('\t') + 1);
		//                 if (IsNumber(temp_str.Substring(0, (sep_pos - 1)))) {
		//                     Gain_High_data(line_read) = double.Parse(temp_str.Substring(0, (sep_pos - 1)));
		//                     temp_str = temp_str.Substring(sep_pos);
		//                 }
		//                 else {
		//                     goto error_format;
		//                 }

		//                 // Print #Transfer_Function_file, Format(Phase_High_data(i), "#0.0")
		//                 if (IsNumber(temp_str)) {
		//                     Phase_High_data(line_read) = double.Parse(temp_str);
		//                 }
		//                 else {
		//                     goto error_format;
		//                 }

		//                 //  lblLines.Caption = line_read
		//             }

		//             Close;
		//             // TODO: # ... Warning!!! not translated
		//             Transfer_Function_file;
		//             update_go_nogo_plot();
		//             txtStatus.Text = ("Measurement file \'"
		//                         + (dlgCommonDialog.FileName + "\' read successfully"));
		//             return;
		//         error_format:
		//             Close;
		//             // TODO: # ... Warning!!! not translated
		//             Transfer_Function_file;
		//             MsgBox("Unrecognizable file format");
		//             return;
		//         error_noFile:
		//         }
		//     }
		// }
	} // end of public partial class formMain : Form
	  //https://stackoverflow.com/questions/31891299/form-designer-has-became-unavailable-for-a-form-in-visual-studio?rq=1
	public class PageControl:TabControl {
		protected override void WndProc(ref Message m) {
			// Hide tabs by trapping the TCM_ADJUSTRECT message
			if (m.Msg == 0x1328 && !DesignMode) m.Result = (IntPtr)1;
			else base.WndProc(ref m);
		}
	}
}
