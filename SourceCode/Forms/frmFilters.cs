using NationalInstruments.UI.WindowsForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;


namespace TMCAnalyzer {
	//artem 060618 filter precision type alias, either double or float
	using filt_prec = System.Double;

	public partial class frmFilters:Form {
		public filt_prec PI_x2_OVER_FREQUENCY;
		const int TEST_LTF_ARRAY_LENGTH = formMain.LTFarrayCount;
		const double LOOP_FREQUENCY = Program.ControllerSampleFrequency;// 5000 for ElDamp, 10000 for STACIS


		public filt_prec[] Freq_points = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		public filt_prec[] Reference_Freq_data = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		filt_prec[] OneFilter_Mag = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		filt_prec[] OneFilter_Phase = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		filt_prec[] Axis_Mag = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		filt_prec[] Axis_Phase = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		public filt_prec[] Predicted_Mag = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		public filt_prec[] Predicted_Phase = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		//filt_prec[] Reference_Gain_data = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		//filt_prec[] Reference_Phase_data = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		public Color OriginalColor = Color.White;
		public Color ChangedColor = Color.PaleTurquoise;

		const int CurrentPlot_Num = 0;
		const int PreviousPlot_Num = 1;
		const int RefPlot_Num = 2;
		const int OneFilterTF_Num = 3;
		const int Axis_TF_Num = 4;
		public const int Prediction_TF_Num = 5;
		const int Lower_Lim_TF_Num = 6;
		const int Upper_Lim_TF_Num = 7;

		string retval;
		// from the privious line to detect jump
		filt_prec Prev_Phase;
		int PreviousFiltNumber; //IK20260105 if user clicks on another filter line (LblFilter) allows to detect filter number change and re-calculate TF
		//filt type, 5 params, 5 coefficients
		//const int PID_P_gain = fGi;	//#define PID_P_gain FilterGain
		//const int PID_I_gain = f1i;	//#define PID_I_gain freq_1
		//const int PID_D_gain = f2i;	//#define PID_D_gain freq_2
		const int PID_P_gain = ParIndxFG;
		const int PID_I_gain = ParIndxF1;
		const int PID_D_gain = ParIndxF2;

		// 10 parameters;
		// 0 to 5 user accessible: Ftype=0, F1=1, Q1=2, F2=3, Q2=4, Fgain=5,
		// 6 to 10 accessible only via advanced functions 6=a1, 7=a2, 8=b0, 9=b1, 0xA=b2

		//IK20260103 indexes of fpar[] exactly as in firmware C
		//public enum filt_par {
		//	FilterGain = 0,
		//	freq_1 = 1,
		//	freq_2 = 2,
		//	Q_f_1 = 3,
		//	Q_f_2 = 4,
		//}
		// Indexes for FilterParameters, instead of enum
		const int ParIndxFG = 0;	// #define FilterGain 0
		const int ParIndxF1 = 1;	// #define freq_1     1
		const int ParIndxF2 = 2;	// #define freq_2     2
		const int ParIndxQ1 = 3;	// #define Q_f_1     1
		const int ParIndxQ2 = 4;	// #define Q_f_2     2

		// Indexes for OriginalFilterParamArray, CHANGING_FilterParamArray
		const int fTi = 0;	// FilterType = 0
		const int fGi = 1;	// #define FilterGain 0
		const int f1i = 2;	// #define freq_1 1
		const int f2i = 3;	// #define freq_2 2
		const int q1i = 4;	// #define Q_f_1  3
		const int q2i = 5;  // #define Q_f_2  4
		// filter coefficients indexes
		const int b2i = 6;
		const int b1i = 7;
		const int b0i = 8;
		const int a2i = 9;
		const int a1i = 10;

		const int FILTER_PARAMS_TOTAL = 11; // Indexes 0 to 10
		const int USER_PARAM_NUMBER = 6;  // Type F1,F2,Q1,Q2 = 6;

		const int MAX_FILTERS_IN_AXIS = 6;

#if true //IK20260103
		const int FB_FILTERS_IN_AXIS = MAX_FILTERS_IN_AXIS - 1;
		const int FB_error_filter_stage_to_adapt_FF = MAX_FILTERS_IN_AXIS - 1; // this is index [5] for SysData.filter[axis][FB_error_filter_stage_to_adapt_FF]
#else
#endif
		// [0-5, 0-9] read from controller filters
		filt_prec[,] OriginalFilterParamArray = new filt_prec[MAX_FILTERS_IN_AXIS, FILTER_PARAMS_TOTAL];	//editing copy of filters
		filt_prec[,] CHANGING_FilterParamArray = new filt_prec[MAX_FILTERS_IN_AXIS, FILTER_PARAMS_TOTAL];	//check change of 5 params AND FILTER TYPE
		bool[,] Filter_CHANGED = new bool[MAX_FILTERS_IN_AXIS, USER_PARAM_NUMBER];

		//graphical Y coordinate
		//mdr// int FrameFilterTop;
		//New member variables for filter parameter file output
		bool demoMode;                          //artem 060518
		AxisData currentAxis = new AxisData();  //artem 060518
		bool setAllAxes;
		//IK20260103 lists hold 6 labels each, per 6 filters
		List<Label> Lbl_FilterType = new List<Label>();
		List<Label> Lbl_FilterFreq1 = new List<Label>();
		List<Label> Lbl_FilterQ1 = new List<Label>();
		List<Label> Lbl_FilterFreq2 = new List<Label>();
		List<Label> Lbl_FilterQ2 = new List<Label>();
		List<NationalInstruments.UI.WindowsForms.NumericEdit> cwNumFilterParam = new List<NationalInstruments.UI.WindowsForms.NumericEdit>();
		List<Label> Lbl_FilterGain = new List<Label>();
		List<Label> LblFilter = new List<Label>();
		formMain formMain;
		int FilterNumberInChain;

		public frmFilters() {
			InitializeComponent();
			InitializeLocalControls();
		}

		public frmFilters(formMain formMain_arg) {
			InitializeComponent();
			InitializeLocalControls();
			SetDefaultFiltParams();  //mdr 010618// recommended by artem
			formMain = formMain_arg;
			ReadyForUserChange = true;
		}

		private void frmFilters_Load(object sender, EventArgs e) {
			ComboFilterAxis.SelectedIndex = 0;
			ComboFilterTYPE.SelectedIndex = 0;
			// Match VB6 2025 default for frequency text
			try {
				if (TxtFreq != null) TxtFreq.Text = "1.0";
			} catch { }
			// VB6 2025 had Pneumatic filters checkbox defaulted to checked; set if available
			try {
				if (CheckPneumFilters != null) CheckPneumFilters.Checked = true;
			} catch { }
			fill_Freq_array();
			ApplyHasFloorFFUI();
			try {
				// Mirror VB6: FramePneumatic / CheckPneumFilters according to AirIsoPresent
				if (formMain != null && formMain.AirIsoPresent) {
					FramePneumatic.Enabled = true;
					CheckPneumFilters.Checked = true;
				} else {
					FramePneumatic.Enabled = false;
					CheckPneumFilters.Checked = false;
				}
			} catch (Exception) {
				// Keep form load resilient if something unexpected occurs (designer mismatch, nulls).
			}
		}

		private void InitializeLocalControls() {
			// Filter type labels F0..F5
			Lbl_FilterType.AddRange(new Label[] {
				Lbl_FilterType0, Lbl_FilterType1, Lbl_FilterType2,
				Lbl_FilterType3, Lbl_FilterType4, Lbl_FilterType5
			});

			// Frequency 1 labels F0..F5
			Lbl_FilterFreq1.AddRange(new Label[] {
				Lbl_FilterFreq10, Lbl_FilterFreq11, Lbl_FilterFreq12,
				Lbl_FilterFreq13, Lbl_FilterFreq14, Lbl_FilterFreq15
			});

			// Q1 labels F0..F5
			Lbl_FilterQ1.AddRange(new Label[] {
				Lbl_FilterQ10, Lbl_FilterQ11, Lbl_FilterQ12,
				Lbl_FilterQ13, Lbl_FilterQ14, Lbl_FilterQ15
			});

			// Frequency 2 labels F0..F5
			Lbl_FilterFreq2.AddRange(new Label[] {
				Lbl_FilterFreq20, Lbl_FilterFreq21, Lbl_FilterFreq22,
				Lbl_FilterFreq23, Lbl_FilterFreq24, Lbl_FilterFreq25
			});

			// Q2 labels F0..F5
			Lbl_FilterQ2.AddRange(new Label[] {
				Lbl_FilterQ20, Lbl_FilterQ21, Lbl_FilterQ22,
				Lbl_FilterQ23, Lbl_FilterQ24, Lbl_FilterQ25
			});

			// Gain labels F0..F5
			Lbl_FilterGain.AddRange(new Label[] {
				Lbl_FilterGain0, Lbl_FilterGain1, Lbl_FilterGain2,
				Lbl_FilterGain3, Lbl_FilterGain4, Lbl_FilterGain5
			});

			// The visible filter row labels 0..5
			LblFilter.AddRange(new Label[] {
				LblFilter0, LblFilter1, LblFilter2, LblFilter3, LblFilter4, LblFilter5
			});

			// Be careful with  the ORDER of adding to the range!
			// Numeric edits for current filter params (tags 0..4)
			cwNumFilterParam.AddRange(new NationalInstruments.UI.WindowsForms.NumericEdit[] {
				cwNumFilterParam0, // Filter gain
				cwNumFilterParam1, // F1
				cwNumFilterParam2, // F2
				cwNumFilterParam3, // Q1
				cwNumFilterParam4  // Q2
			});

			formMain.HasFloorFFChanged += OnHasFloorFFChanged;
		}

		private void OnHasFloorFFChanged(object sender, EventArgs e) {
			// If we're on a non-UI thread, marshal to UI thread.
			if (this.IsHandleCreated && this.InvokeRequired) {
				this.BeginInvoke(new Action(ApplyHasFloorFFUI));
			} else {
				ApplyHasFloorFFUI();
			}
		}
		private int GetActiveFilterCount()
		{
			// If controller supports Floor FF then all MAX_FILTERS_IN_AXIS filters are active;
			// otherwise reserve last slot for adaptive-FF and treat it as not present.
			// formMain.HasFloorFF is the public static bool defined in frmMain.
			// Use the static directly (frmMain.HasFloorFF) so this works even if instance isn't set.
			return (formMain.HasFloorFF) ? MAX_FILTERS_IN_AXIS : (MAX_FILTERS_IN_AXIS - 1);
		}

		private int GetActiveFilterMaxIndex()
		{
			// return the max index (inclusive) for loops that use <= maxIndex
			return GetActiveFilterCount() - 1;
		}

		private void ApplyHasFloorFFUI()
		{
			try
			{
				int active = GetActiveFilterCount();
				// Show/hide UI rows for filter slots according to active count
				for (int i = 0; i < MAX_FILTERS_IN_AXIS; i++)
				{
					bool visible = i < active;
					// Attempt to hide all per-row controls if they exist.
					if (LblFilter.Count > i) LblFilter[i].Visible = visible;
					if (Lbl_FilterType.Count > i) Lbl_FilterType[i].Visible = visible;
					if (Lbl_FilterFreq1.Count > i) Lbl_FilterFreq1[i].Visible = visible;
					if (Lbl_FilterFreq2.Count > i) Lbl_FilterFreq2[i].Visible = visible;
					if (Lbl_FilterQ1.Count > i) Lbl_FilterQ1[i].Visible = visible;
					if (Lbl_FilterQ2.Count > i) Lbl_FilterQ2[i].Visible = visible;
					if (Lbl_FilterGain.Count > i) Lbl_FilterGain[i].Visible = visible;
				}
				// Update axis names 12..21 depending on firmware mode (HasFloorFF).
				// Ensure ComboFilterAxis has at least 22 entries (0..21)
				if (ComboFilterAxis != null && ComboFilterAxis.Items.Count >= 22) {
					if (formMain != null && formMain.HasFloorFF) {
						ComboFilterAxis.Items[12] = "ZfloorFF";
						ComboFilterAxis.Items[13] = "XfloorFF";
						ComboFilterAxis.Items[14] = "YfloorFF";
						ComboFilterAxis.Items[15] = "UnusedFF";
						ComboFilterAxis.Items[16] = "UnusedV1";
						ComboFilterAxis.Items[17] = "UnusedV2";
						ComboFilterAxis.Items[18] = "UnusedV3";
						ComboFilterAxis.Items[19] = "UnusedV4";
						ComboFilterAxis.Items[20] = "UnusedBal";
						ComboFilterAxis.Items[21] = "Aux Filters";
					} else {
						ComboFilterAxis.Items[12] = "FF X acc";
						ComboFilterAxis.Items[13] = "FF Y acc";
						ComboFilterAxis.Items[14] = "FF X pos";
						ComboFilterAxis.Items[15] = "FF Y pos";
						ComboFilterAxis.Items[16] = "pressure V1";
						ComboFilterAxis.Items[17] = "pressure V2";
						ComboFilterAxis.Items[18] = "pressure V3";
						ComboFilterAxis.Items[19] = "pressure V4";
						ComboFilterAxis.Items[20] = "pressure Bal";
						ComboFilterAxis.Items[21] = "Aux Filters";
					}
				}
			} catch
			{
				// keep silent if designer differs; do not throw on visibility adjustments
				MessageBox.Show("Not enough DropDownControl FilterTypes in colection", "Fiter Type Selection problem", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdateCoefficientDiffs(int filtNum) {
			try {
				// mapping per current code: index 10 = a1, 9 = a2, 8 = b0, 7 = b1, 6 = b2
				var mapping = new Dictionary<string, int>
				{
					{"Lbl_a1_diff", 10},
					{"Lbl_a2_diff", 9},
					{"Lbl_b0_diff", 8},
					{"Lbl_b1_diff", 7},
					{"Lbl_b2_diff", 6}
				};

				foreach (var kv in mapping) {
					string lblName = kv.Key;
					int idx = kv.Value;

					// find control in the form (robust if designer not updated)
					var ctrls = this.Controls.Find(lblName, true);
					if (ctrls != null && ctrls.Length > 0 && ctrls[0] is Label lbl) {
						double pc = CHANGING_FilterParamArray[filtNum, idx];
						double ctrl = OriginalFilterParamArray[filtNum, idx];

						if (Math.Abs(pc) < 1e-12) {
							lbl.Text = "N/A";
						} else {
							double ppm = 100000.0 * (pc - ctrl) / pc;
							// Format as integer PPM with sign
							lbl.Text = string.Format("{0:0}", ppm);
						}
					}
					// else: designer hasn't been updated — do nothing silently
				}
			} catch {
				// keep UI stable if anything goes wrong
			}
		}

		// or it will assume all zeros and Filter_FT will be -100 dB and Axis_TF will be -500 dB
		public void SetDefaultFiltParams() {
			int f_num;
			int f_par;
			ComboFilterTYPE.ForeColor = Color.Black;                   //artem 060518

			for (f_num = 0; f_num < MAX_FILTERS_IN_AXIS; f_num++) {
				OriginalFilterParamArray[f_num, fTi] = 0;                //filter type = NO_FILTER
				OriginalFilterParamArray[f_num, fGi] = 1;                //gain
				OriginalFilterParamArray[f_num, f1i] = 1;                //F1
				OriginalFilterParamArray[f_num, f2i] = 0.5;              //F2
				OriginalFilterParamArray[f_num, q1i] = 0.5;              //Q1
				OriginalFilterParamArray[f_num, q2i] = 0.5;              //Q2
				// coefficients below for NO_FILTER or GAIN_ONLY
				OriginalFilterParamArray[f_num, b2i] = 0;                //b2
				OriginalFilterParamArray[f_num, b1i] = 0;                //b1
				OriginalFilterParamArray[f_num, b0i] = 1;                //b0 <== filter gain = 1
				OriginalFilterParamArray[f_num, a2i] = 0;                //a2
				OriginalFilterParamArray[f_num, a1i] = 0;                //a1
				Lbl_FilterType[f_num].Text = ComboFilterTYPE.Items[0].ToString();  //set NO_Filters

				//---
				Lbl_FilterGain[f_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[f_num,  fGi]);
				Lbl_FilterFreq1[f_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[f_num, f1i]);
				Lbl_FilterFreq2[f_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[f_num, f2i]);
				Lbl_FilterQ1[f_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[f_num,    q1i]);
				Lbl_FilterQ2[f_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[f_num,    q2i]);

				for (f_par = 0; f_par < FILTER_PARAMS_TOTAL; f_par++) {
					CHANGING_FilterParamArray[f_num, f_par] = OriginalFilterParamArray[f_num, f_par];

				}
				FilterNumberInChain = 0;
				cwNumFilterParam[ParIndxFG].Value = OriginalFilterParamArray[FilterNumberInChain, fGi]; //Gain
				cwNumFilterParam[ParIndxF1].Value = OriginalFilterParamArray[FilterNumberInChain, f1i]; //F1
				cwNumFilterParam[ParIndxF2].Value = OriginalFilterParamArray[FilterNumberInChain, f2i]; //F2
				cwNumFilterParam[ParIndxQ1].Value = OriginalFilterParamArray[FilterNumberInChain, q1i]; //Q1
				cwNumFilterParam[ParIndxQ2].Value = OriginalFilterParamArray[FilterNumberInChain, q2i]; //Q2
			}
		}
		private string response;
		private void cmdLoadDefaults_Click(System.Object eventSender, System.EventArgs eventArgs) // command "load default parameters"
		{
			updateDemoStatus();
			if (!demoMode)      //artem 0601518
			{
				string response = "";
				//		cmd_sent = false;
				//-!- IK190505 use either one or another call
				Program.ConnectedController.SendInternal("dflt", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
				//IK190505		formMain.SendInternal("dflt", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			}
		}

		private void frmFilters_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason != CloseReason.ApplicationExitCall) {
				e.Cancel = true;
				this.Hide();
			}
		}

		private void CmdFilterRefresh_Click(object sender, EventArgs e) {
			string gain_str;
			string retval;
			int sep_pos;
			List<string> file_filt_params = new List<string>();
			if (Program.formTerminal != null) Program.formTerminal.OnQuitMenuCommand();
			//if ((ComboFilterAxis.SelectedIndex > 20)) // Parameter "$": index of axis : position Xp=0,Yp=1,tZp=2,Zp=3,tXp=4,tYp=5; velocity Xv=6,Yv=7,tZv=8,Zv=9,tXv=A,tYv=B; damping Zd=C,tXd=D,tYd=E, balance diagonal=F
			//{
			//	// error, there are only 12 axis gains, gain#$, #=0,1,2,3, $=0-A
			//	cwNumAxisGain.BackColor = Color.Red;// 12632319;
			//	cwNumAxisGain.Value = 0;
			//	cwNumAxisGain.Enabled = false;
			//	// mdr 060318 // Refresh_FilterParams();
			//	//  but, refresh  filter chain - there are 20 filter chains
			//	return;
			//}
			Program.IsReadingControllerSettings = true;
			ReadyForUserChange = false; //IK prevent mutiple updates while reading parameters
			if (demoMode) {
				SetDefaultFiltParams();                      //Initialize controls again for easier update from file
				file_filt_params = LoadParamsFromFile();     //Load filter parameters from input file as string List
				ParseFilterParams(ref file_filt_params);     //Setup controls, labels, filter arrays using data from file
			} else {
				cwNumAxisGain.BackColor = OriginalColor;
				cwNumAxisGain.Enabled = true;
				update_damp_controls();
				if (CheckPneumFilters.Checked)  //read aux damping gain
				{
					if (ComboFilterAxis.SelectedIndex >= 3 && ComboFilterAxis.SelectedIndex <= 5) {
						gain_str = "gain0" + ExtendedHEX(ComboFilterAxis.SelectedIndex + 9);
						formMain.SendInternal(gain_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
						sep_pos = (response.IndexOf("=") + 1);
						if ((sep_pos != 0)) {
							retval = response.Substring(sep_pos);
							sep_pos = (retval.IndexOf("//") + 1);
							// 'if verbose response
							if ((sep_pos != 0)) {
								retval = retval.Substring(0, (sep_pos - 1));
							}

							double tryParseVal;
							if (double.TryParse(retval, out tryParseVal)) {
								cwNumAxisGain.Value = double.Parse(retval);
								cwNumAxisGain.BackColor = OriginalColor;
							} else {
								cwNumAxisGain.BackColor = Color.Red;
							}
						}
						formMain.SendInternal("loop_FBD", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
						sep_pos = (response.IndexOf(">") + 1);
						if ((sep_pos != 0)) {
							if (response.Substring(sep_pos, 4).ToLower() == "acti") {
								ChkAuxDamping.Checked = true;
							}
							if (response.Substring(sep_pos + 1, 4).ToLower() == "pass") {
								ChkAuxDamping.Checked = false;
							}
						}
					}
				} else {
					PneumFilterChain = false;
				}
			}
			if (ComboFilterAxis.SelectedIndex > 21) { //-!- IK20260103 NEED fix, above is return if index >20
				Refresh_FilterParams();
				return;
			}
			//Ready = true;
			//Program.IsReadingControllerSettings = true;


			//Init_sys_data = true;
			//  gain_str = ("gain0" + ExtendedHEX(ComboFilterAxis.ListIndex));
			gain_str = ("gain0" + ExtendedHEX(ComboFilterAxis.SelectedIndex));

			// response = Analyzer.GetSend(gain_str, true);
			// Program.ConnectedController.SendInternal(gain_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			formMain.SendInternal(gain_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			sep_pos = (response.IndexOf("=") + 1);
			if ((sep_pos != 0)) {
				retval = response.Substring(sep_pos);
				sep_pos = (retval.IndexOf("//") + 1);
				// 'if verbose response
				if ((sep_pos != 0)) {
					retval = retval.Substring(0, (sep_pos - 1));
				}

				double tryParseVal;
				if (double.TryParse(retval, out tryParseVal)) {
					cwNumAxisGain.Value = double.Parse(retval);
					cwNumAxisGain.BackColor = OriginalColor;
				} else {
					cwNumAxisGain.BackColor = System.Drawing.Color.FromArgb(0xC0C0FF);
				}
			}
			Program.IsReadingControllerSettings = false;
			Refresh_FilterParams();
			ReadyForUserChange = true;
		}

		void Refresh_FilterParams() {
			// //define indexes for float_iir.par[index]
			// #define PID_P_gain FilterGain
			// #define PID_I_gain freq_1
			// #define PID_D_gain freq_2
			// fpar&$#? returns "fpar&$#=GGG.gg" scientific format displayed; fpar&$#=ggg.gg sets filter parameter
			// & = axis [0-F]== first index of SysData.filters[6][max_filt_in_channel]; SysData.PROXfilt[6][]; SysData.FF_Filt[4][],
			// $= filter number in the axis [0,1,2,3,4,5] = = second index in SysData.filters[NUM_FILT_CHANs][max_filt_in_channel];
			// #= filter parameter number [0,1,2,3,4,5] == long ftyp; float par[5];
			//
			// #= filter parameter number [0,1,2,3,4,5] == long ftyp; float par[5];
			ReadyForUserChange = false;
			int Filt_num;
			FrameFilter.Visible = false;
			if (!demoMode) {
				// retval = Analyzer.GetSend("echo>enab", true);
				string response = string.Empty;
				// Program.ConnectedController.SendInternal("echo>enab", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
				formMain.SendInternal("echo>enab", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			}
			// disable "echo>verb" to speed up
			// >>>>>>>>> CoPilot 20260104 suggested
			// but IK20260104 s not agree - it is just refreshing labels, not calculating
			// IK int maxCount = GetActiveFilterCount();
			// IK for (Filt_num = 0; Filt_num < maxCount; Filt_num++)
			for (Filt_num = 0; Filt_num < MAX_FILTERS_IN_AXIS; Filt_num++)       // 0 to 5:  5 params AND FILTER_TYPE
			{
				Fill_in_filter_params(ref Filt_num);
				Lbl_FilterType[Filt_num].Text = ComboFilterTYPE.Items[(int)OriginalFilterParamArray[Filt_num, fTi]].ToString();

				Lbl_FilterGain[Filt_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[Filt_num, fGi]);
				Lbl_FilterFreq1[Filt_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[Filt_num, f1i]);
				Lbl_FilterFreq2[Filt_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[Filt_num, f2i]);
				Lbl_FilterQ1[Filt_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[Filt_num, q1i]);
				Lbl_FilterQ2[Filt_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[Filt_num, q2i]);
				CheckIfFilterChangedAndReCalculateTFsAndPrediction(Filt_num);
			}
			Copy_Params_for_Edit(FilterNumberInChain);
			ReadyForUserChange = true;
		}

		/// <summary>
		/// function checks all user params in the current filter associated with CHANGING_FilterParamArray[Filt_num, *]
		/// compares them with OriginalFilterParamArray[Filt_num, *]
		/// and updates array Filter_CHANGED[Filt_num, *]
		/// </summary>
		/// <param name="Filt_num"> 0 to 5, either Editing Frame position or when checking whole filter chain</param>
		/// <optional param name="UpdateBackClolor"> default = false. If true, changes label's BackColors</param>
		/// <returns>true if any parameter changed (can be more than one); otherwise, false</returns>
		bool CheckIfFilterChanged(int Filt_num, bool UpdateBackColor = false) {
			bool FilterChanged = false;
			// check params change [0 to 5] are user accessible:
			for (int Filt_Param_num = 0; (Filt_Param_num < USER_PARAM_NUMBER); Filt_Param_num++) // Fgain=0,  F1=1, Q1=2, F2=3, Q2=4, Ftype=0,
			{
				// check changes in 6 params including FILTER TYPE
				if (CHANGING_FilterParamArray[Filt_num, Filt_Param_num] != OriginalFilterParamArray[Filt_num, Filt_Param_num]) {
					Filter_CHANGED[Filt_num, Filt_Param_num] = true;
					FilterChanged = true;
					if (UpdateBackColor) {
						LblFilter[Filt_num].ForeColor = Color.Red;
						LblFilter[Filt_num].Text = "*" + (Filt_num + 1).ToString();

						if ((Filt_Param_num ) == fTi) //IK20260104 intetionally uses fTi = 0 to update color of filter name label
						{
							Lbl_FilterType[Filt_num].BackColor = ChangedColor;
						}
						else if ((Filt_Param_num - 1) == ParIndxFG) {//  -1 because we skip FilterTYPE with index 0
							Lbl_FilterGain[Filt_num].BackColor = ChangedColor;
						} else if ((Filt_Param_num - 1) == ParIndxF1) {
							Lbl_FilterFreq1[Filt_num].BackColor = ChangedColor; // 12632319;
						} else if ((Filt_Param_num - 1) == ParIndxF2) {
							Lbl_FilterFreq2[Filt_num].BackColor = ChangedColor;
						} else if ((Filt_Param_num - 1) == ParIndxQ1) {
							Lbl_FilterQ1[Filt_num].BackColor = ChangedColor;
						} else if ((Filt_Param_num - 1) == ParIndxQ2) {
							Lbl_FilterQ2[Filt_num].BackColor = ChangedColor;
						}
					}
				} else // no change
				  {
					Filter_CHANGED[Filt_num, Filt_Param_num] = false;
					if (UpdateBackColor) {
						LblFilter[Filt_num].Text = "  " + (Filt_num + 1).ToString() + " ";
						LblFilter[Filt_num].ForeColor = Color.Black;
						if ((Filt_Param_num) == fTi) //IK20260104 intetionally uses fTi = 0 to update color of filter name label
						{
							Lbl_FilterType[Filt_num].BackColor = OriginalColor;
						}
						else if ((Filt_Param_num - 1) == ParIndxFG) {//  -1 because we skip FilterTYPE with index 0
							Lbl_FilterGain[Filt_num].BackColor = OriginalColor;
						} else if ((Filt_Param_num - 1) == ParIndxF1) {
							Lbl_FilterFreq1[Filt_num].BackColor = OriginalColor; // 12632319;
						} else if ((Filt_Param_num - 1) == ParIndxF2) {
							Lbl_FilterFreq2[Filt_num].BackColor = OriginalColor;
						} else if ((Filt_Param_num - 1) == ParIndxQ1) {
							Lbl_FilterQ1[Filt_num].BackColor = OriginalColor;
						} else if ((Filt_Param_num - 1) == ParIndxQ2) {
							Lbl_FilterQ2[Filt_num].BackColor = OriginalColor;
						}
					}
				}
			}
			return FilterChanged;
		}

		// '''''''''changes color of control and set/clear Boolean Filter_CHANGED(filt_NUM,, FilterParamNum)
		void CheckIfFilterChangedAndReCalculateTFsAndPrediction(int Filt_num) {
			if (Program.IsReadingControllerSettings || !ReadyForUserChange)
				return;
			bool FilterChanged = CheckIfFilterChanged(Filt_num, true);
			bool FilterNumberChanged = (PreviousFiltNumber != Filt_num);
			if (FilterChanged  || FilterNumberChanged) {
				PreviousFiltNumber = Filt_num;
				CalculateAxis_TF();
				if(formMain.ChkShowPredictionPlot.Checked)
					Calc_Prediction();
			} else {
				// not changed, restore white color
			}

			if (ChkShowAxisTF.Checked == true) {
				formMain.ScatterGraphMag.Plots[Axis_TF_Num].Visible = true;
				formMain.ScatterGraphPhase.Plots[Axis_TF_Num].Visible = true;
			} else {
				formMain.ScatterGraphMag.Plots[Axis_TF_Num].Visible = false;
				formMain.ScatterGraphPhase.Plots[Axis_TF_Num].Visible = false;
			}
		}

		public void CalculateAxis_TF(bool reverse_Ph = false) {
			long i;
			// Warning!!! Optional parameters not supported
			int Filt_num;
			//tMag_Phase Mag_Phase;
			//// 'temp structure
			//tMag_Phase Axis_mag_phase;
			//double_iir ThisFilterCoeff;
			filt_prec[] TMpFiltPlot_Mag = new filt_prec[TEST_LTF_ARRAY_LENGTH];
			filt_prec[] TMpFiltPlot_Phase = new filt_prec[TEST_LTF_ARRAY_LENGTH];
			int phase_reversal;

			if (reverse_Ph == true) {
				phase_reversal = 180;
			} else {
				phase_reversal = 0;
			}

			//  calculation "horizontally" NOT changing Filt_num untill all freq are calculated
			// initial conditions
			for (i = 0; (i < TEST_LTF_ARRAY_LENGTH); i++) {
				//  0 to 200
				TMpFiltPlot_Phase[i] = 0;
				if (cwNumAxisGain.Value > 0) {
					// means it was updated
					TMpFiltPlot_Mag[i] = (20 * (Math.Log(cwNumAxisGain.Value) / Math.Log(10)));
					// convert Axis gain to dB
				} else {
					TMpFiltPlot_Mag[i] = 0;
					//  gain not updated: assume gain of 1 = 0 dB
				}

			}

			//  calculation "horizontally"
			//for (Filt_num = 0; (Filt_num <= FB_FILTERS_IN_AXIS); Filt_num++)// CoPilot 20260104
			int maxFiltIdx = GetActiveFilterMaxIndex(); // inclusive
			for (Filt_num = 0; Filt_num <= maxFiltIdx; Filt_num++) // CoPilot 20260104
			{
				// 0 to 4, the 5th filter in ElectroDamp is for stage FF adaptive algorithm
				//mdr 053118//if ((ChkRealTimeUpdateTF.Checked == true))
				//mdr 053118// should be done prior and w/o ref. to ChkRealTimeUpdateTF//set_filt_params_from_user_input(Filt_num);

				CalculateFilterTF(Filt_num); //mdr 060118 // CalculateFilterTF() should be called prior call CalculateAxis_TF() // CalculateFilterTF(Filt_num);
											 //  it fills       OneFilter_Mag(200),   OneFilter_Phase(200)
				for (i = 0; (i < TEST_LTF_ARRAY_LENGTH); i++) {
					TMpFiltPlot_Mag[i] = (TMpFiltPlot_Mag[i] + OneFilter_Mag[i]);
					//  dB added
					TMpFiltPlot_Phase[i] = (TMpFiltPlot_Phase[i] + OneFilter_Phase[i]);
					//  phase added
				}
			}

			for (i = 0; (i < TEST_LTF_ARRAY_LENGTH); i++) {
				//  0 to 200
				Axis_Mag[i] = TMpFiltPlot_Mag[i];
				Axis_Phase[i] = Phase_Limit_TF_plus_minus_180((TMpFiltPlot_Phase[i] - phase_reversal));
			}

			i = (i - 1);
			formMain.ScatterGraphMag.Plots[Axis_TF_Num].PlotXY(Freq_points, Axis_Mag);
			formMain.ScatterGraphPhase.Plots[Axis_TF_Num].PlotXY(Freq_points, Axis_Phase);
		}

		filt_prec Phase_Limit_TF_plus_minus_180(filt_prec phase_in) {
			//  cleaning phase, which might jump because sum of all filters can go beyond 360
			//  example: prev phase is still +174, but this phase has jumped to -188
			if ((phase_in > 360)) {
				phase_in = (phase_in - 360);
				//
			} else if ((phase_in > 180)) {
				phase_in = (phase_in - 360);
			} else if ((phase_in < -360)) {
				phase_in = (phase_in + 360);
				//
			} else if ((phase_in < -180)) {
				phase_in = (phase_in + 360);
			}
			//else
			//{
			//    phase_in = phase_in;
			//}

			// Prev_Phase = phase_in
			return phase_in;
		}
#if false
#endif

		public class double_iir {
			public int ftyp;
			public filt_prec[] par = new filt_prec[USER_PARAM_NUMBER - 1]; // because ftyp is int, rest is filt_prec: 1 + (6 - 1) = 6 == Max parameters
			public filt_prec a1;
			public filt_prec a2;
			public filt_prec b0;
			public filt_prec b1;
			public filt_prec b2;
		}
		public struct tMag_Phase {
			public filt_prec Mag;
			public filt_prec Phase;
		}

		public struct tComplex {
			public filt_prec Re;
			public filt_prec Im;
		}

		void CalculateFilterTF(int Filt_num) {
			long i;
			tMag_Phase Mag_Phase;
			// 'temp structure
			double_iir ThisFilterCoeff = new double_iir();
			filt_prec freq_sum_for_test;
			filt_prec prev_freq = 0;
			filt_prec t_float;
			set_filt_params_from_user_input(Filt_num);

			ThisFilterCoeff.b2 = CHANGING_FilterParamArray[Filt_num, b2i];
			ThisFilterCoeff.b1 = CHANGING_FilterParamArray[Filt_num, b1i];
			ThisFilterCoeff.b0 = CHANGING_FilterParamArray[Filt_num, b0i];
			ThisFilterCoeff.a2 = CHANGING_FilterParamArray[Filt_num, a2i];
			ThisFilterCoeff.a1 = CHANGING_FilterParamArray[Filt_num, a1i];
			// check if frequency array is filled, not zero
			freq_sum_for_test = 0;

			try {
				for (i = 0; (i < TEST_LTF_ARRAY_LENGTH); i++) {
					// 0 to 200
					if ((Freq_points[i] > 0)) {
						// prepare test array for axis OLTF calculation
						prev_freq = Freq_points[i];
						// prepare test array for axis OLTF calculation
						Freq_points[i] = prev_freq;
					} else {
						// Freq_points are empty, maybe Reference_Freq_data is filled?
						if ((Reference_Freq_data[i] > 0)) {
							prev_freq = Reference_Freq_data[i];
							//  update array
							Freq_points[i] = prev_freq;
						} else {
							//                 Freq_points(i) = prev_freq ' fill up to the end with the last valid frequency
						}
					}

					freq_sum_for_test = (freq_sum_for_test + prev_freq);
				}

				i = (i - 1);
				if ((freq_sum_for_test == 0)) {
					//  array is not filled
					fill_Freq_array();
				}

				for (i = 0; (i < TEST_LTF_ARRAY_LENGTH); i++) {
					//  starts from lower frequency, 0.1 Hz
					Mag_Phase = Mag_Ph_vs_Freq(ThisFilterCoeff, Freq_points[i], LOOP_FREQUENCY);
					OneFilter_Mag[i] = (20 * (Math.Log(Mag_Phase.Mag) / Math.Log(10)));
					t_float = Mag_Phase.Phase;
					//  returns between -90 and +90
					/* IK20210425 fixed
					if (((ThisFilterCoeff.ftyp == (int)filt_types.LPF_2ndOr_wPk)
								|| (ThisFilterCoeff.ftyp == (int)filt_types.Position)))
					{
						// 7 = LPF 2nd order, need to reverse phase because it calculates phase 180 at low freq
						t_float = (t_float - 180);
					}
					*/
					if ((i == 0)) {
						Prev_Phase = (float)t_float;
					}

					OneFilter_Phase[i] = Phase_Filter_Expand_plus_minus_180(t_float);
					//  returns between -180 and +180
					// OneFilter_Phase(i) = (t_float) ' returns between -180 and +180
				}

				if (((FilterNumberInChain == Filt_num)
							&& (ChkShowOneFilterTF.Checked == true))) {
					formMain.ScatterGraphMag.Plots[OneFilterTF_Num].PlotXY(Freq_points, OneFilter_Mag);
					formMain.ScatterGraphPhase.Plots[OneFilterTF_Num].PlotXY(Freq_points, OneFilter_Phase);
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		// double Prev_Phase;

		public filt_prec Phase_Filter_Expand_plus_minus_180(filt_prec phase_in) {
			filt_prec phase_diff;

			if ((Prev_Phase >= 180)) {
				//  > 179.9999999
				Prev_Phase = (Prev_Phase - 180);
				//
			} else if ((Prev_Phase <= -180)) {
				// <= -179.9999999
				Prev_Phase = (Prev_Phase + 180);
				//
			}

			phase_diff = (phase_in - Prev_Phase);

			if (phase_diff >= 179.9999999)
				phase_in = phase_in - 180;
			else {
				if (phase_diff >= 90)
					phase_in = phase_in - 180;
				else {
					if (phase_diff <= -179.9999999)
						phase_in = phase_in + 180;
					else {
						if (phase_diff < -90)
							phase_in = phase_in + 180;
						//else
						//    phase_in = phase_in;    //artem 060718 useless
					}
				}
			}

			Prev_Phase = phase_in;
			return phase_in;

		}

		//===============================================================================================================================
		// this function calculates magnitude-phase prediction based on filter coefficients
		// this simulates DSP, which works with 32 bit floats. DO NOT replace float with double
		//===============================================================================================================================
		public tMag_Phase Mag_Ph_vs_Freq(double_iir Coeff, filt_prec freq, filt_prec LoopFreq) {
			// TODO: On Error Resume Next Warning!!!: The statement is not translatable
			tMag_Phase retTMag = new tMag_Phase();
			filt_prec Omega;
			tComplex H;
			tComplex Num;
			tComplex Den;
			Omega = (freq * (2 * (Math.PI / LoopFreq)));
			Num = Numerator_Complex(Coeff, Omega);
			Den = Denominator_Complex(Coeff, Omega);
			H = IM_DIV(ref Num, ref Den);
			retTMag.Mag = Math.Sqrt(((H.Re * H.Re) + (H.Im * H.Im)));
			if ((retTMag.Mag == 0)) {
				retTMag.Mag = 1E-05;
			}

			// we take a Log and it cannot be a zero' 1e-5 means it is an error
			if ((H.Re != 0)) {
				// retTMag.Phase = ((Math.Atan((H.Im / H.Re)) * (180 / Math.PI)) * -1); // 20260102Original
				retTMag.Phase = -(Math.Atan2((double)H.Im, (double)H.Re)) * (180 / Math.PI); // 20260102 CoPilot
				//  returns between -90 and +90
			} else {
				retTMag.Phase = -360;
				// this means it is an error
			}

			return retTMag;

		}

		public tComplex IM_DIV(ref tComplex arg1, ref tComplex arg2) {
			// division of two complex numbers:
			//  multiply the numerator and denominator by the complex conjugate of the denominator,
			// for example, with z_1=a+bi and z_2=c+di, z=z_1/z_2 is given by
			// numerator and denomerator  two complex numbers, returns a complex number
			tComplex Denom_Conj = Complex(arg2.Re, -arg2.Im);
			filt_prec Denom_Mag = arg2.Re * arg2.Re + arg2.Im * arg2.Im;
			const double eps = 1e-18;
			if (Math.Abs((double)Denom_Mag) < eps) { //20260102 CoPilot
				// Denominator near zero: return zero complex (or decide an alternate sentinel)
				Debug.WriteLine("IM_DIV: denominator magnitude near zero, returning (0,0)");
				return Complex(0, 0);
			}
			return Complex((((arg1.Re * arg2.Re)
							+ (arg1.Im * arg2.Im))
							/ Denom_Mag), (((arg1.Im * arg2.Re)
							- (arg1.Re * arg2.Im))
							/ Denom_Mag));
		}

		public tComplex Complex(filt_prec dReal, filt_prec dImag) {
			tComplex retComplex;
			// Set the real and imaginary numbers in a complex number type
			retComplex.Re = dReal;
			retComplex.Im = dImag;
			return retComplex;
		}

		public tComplex Numerator_Complex(double_iir Coeff, filt_prec Omega) {
			filt_prec NumRe;
			filt_prec NumIm;
			NumRe = Coeff.b0 + Coeff.b1 * Math.Cos(Omega) + Coeff.b2 * Math.Cos((2 * Omega));
			NumIm = Coeff.b1 * Math.Sin(Omega) + Coeff.b2 * Math.Sin((2 * Omega));
			return Complex(NumRe, NumIm);
		}

		public tComplex Denominator_Complex(double_iir Coeff, filt_prec Omega) {
			filt_prec DenRe;
			filt_prec DenIm;
			DenRe = 1 - Coeff.a1 * Math.Cos(Omega) - Coeff.a2 * Math.Cos((2 * Omega));
			DenIm = -Coeff.a1 * Math.Sin(Omega) - Coeff.a2 * Math.Sin((2 * Omega));
			return Complex(DenRe, DenIm);
		}

		void fill_Freq_array() {
			long i;
			filt_prec delta_freq;
			filt_prec freq_values_per_decade;
			Freq_points[0] = 0.1;
			freq_values_per_decade = (TEST_LTF_ARRAY_LENGTH / 4);
			// delta_freq = (10 ^ (1 / freq_values_per_decade));
			delta_freq = Math.Pow(10, (1 / freq_values_per_decade));

			// TODO: Warning!!! The operator should be an XOR ^ instead of an OR, but not available in CodeDOM
			for (i = 1; (i < TEST_LTF_ARRAY_LENGTH); i++) {
				Freq_points[i] = (Freq_points[(i - 1)] * delta_freq);
			}
		}

		void set_filt_params_from_user_input(int Filt_Number) {
			double_iir FilterParams = new double_iir();
			long param;
			float testVar = 0;
			FilterParams.ftyp = (int)CHANGING_FilterParamArray[Filt_Number, fTi];
			// ComboFilterTYPE.ListIndex
			FilterParams.par[ParIndxFG] = CHANGING_FilterParamArray[Filt_Number, fGi];
			FilterParams.par[ParIndxF1] = CHANGING_FilterParamArray[Filt_Number, f1i];
			FilterParams.par[ParIndxF2] = CHANGING_FilterParamArray[Filt_Number, f2i];
			FilterParams.par[ParIndxQ1] = CHANGING_FilterParamArray[Filt_Number, q1i];
			FilterParams.par[ParIndxQ2] = CHANGING_FilterParamArray[Filt_Number, q2i];
			for (param = 0; (param < (USER_PARAM_NUMBER - 1)); param++) {
				if ((FilterParams.par[param] != 0)) {
					testVar ++;
				}
			}

			if ((testVar == 0)) {
				return;
			}

			// all params are zero, not initiated yet. it will cause 'dived by zero' exception
			//xxx if ((ChkRealTimeUpdateTF.Checked == true))
			{
				set_iir(FilterParams);	// this recalculates a, b coefficients

				//  update coefficients so we can re-cal axis TF and re-plot
				CHANGING_FilterParamArray[Filt_Number, b2i] = FilterParams.b2;
				CHANGING_FilterParamArray[Filt_Number, b1i] = FilterParams.b1;
				CHANGING_FilterParamArray[Filt_Number, b0i] = FilterParams.b0;
				CHANGING_FilterParamArray[Filt_Number, a2i] = FilterParams.a2;
				CHANGING_FilterParamArray[Filt_Number, a1i] = FilterParams.a1;
			}

			//  update screen
			if ((Filt_Number == FilterNumberInChain)) {
				Lbl_PC_b2.Text = string.Format("{0:0.0#######}", CHANGING_FilterParamArray[Filt_Number, b2i]);
				Lbl_PC_b1.Text = string.Format("{0:0.0#######}", CHANGING_FilterParamArray[Filt_Number, b1i]);
				Lbl_PC_b0.Text = string.Format("{0:0.0#######}", CHANGING_FilterParamArray[Filt_Number, b0i]);
				Lbl_PC_a2.Text = string.Format("{0:0.0#######}", CHANGING_FilterParamArray[Filt_Number, a2i]);
				Lbl_PC_a1.Text = string.Format("{0:0.0#######}", CHANGING_FilterParamArray[Filt_Number, a1i]);

				// update difference labels (PPM) if the controls exist in the designer
				UpdateCoefficientDiffs(Filt_Number);
			}
		}

		private static readonly object s_setIirLogLock = new object();
		private static string SetIirLogPath => Path.Combine(Application.UserAppDataPath, "set_iir_log.txt");

		private static void AppendSetIirLog(string text) {
			try {
				lock (s_setIirLogLock) {
					Directory.CreateDirectory(Path.GetDirectoryName(SetIirLogPath));
					File.AppendAllText(SetIirLogPath, text);
				}
			} catch (Exception ex) {
				Debug.WriteLine("AppendSetIirLog error: " + ex.Message);
			}
		}
		public void set_iir(double_iir fp) {
			// CoPilot 20260101 Translated/updated to match VB6 Analyzer.BAS set_float_iir (latest)
			// Includes debug logging to set_iir_log.txt for intermediate values.
			try {
				// local temporaries (use filt_prec for consistent precision in the project)
				//#define omega_1pole omega_1pole 'alias name: omega_Numenator
				//#define omega_2zero omega_2zero 'alias name: omega_DEnominator
				filt_prec h1 = 0;
				filt_prec h2 = 0;
				filt_prec t_gain = 0;
				filt_prec s_freq_gain_corr = 0;
				filt_prec cotan_of_half_omega_1 = 0;
				filt_prec freq_1_correction = 0;
				filt_prec freq_2_correction = 0;
				filt_prec omega_1pole = 0;
				filt_prec omega_2zero = 0;
				filt_prec testVar = 0;
				filt_prec f1 = fp.par[ParIndxF1];
				filt_prec f2 = fp.par[ParIndxF2];
				filt_prec q1 = fp.par[ParIndxQ1];
				filt_prec q2 = fp.par[ParIndxQ2];

				// compute constant
				PI_x2_OVER_FREQUENCY = (filt_prec)(Math.PI * 2.0 / LOOP_FREQUENCY);

				// prevent division by zero for Qs
				if (q1 == 0) q1 = (filt_prec)0.5;
				if (q2 == 0) q2 = (filt_prec)0.5;

				// freq corrections
				testVar = (filt_prec)(q1 * q1);
				freq_1_correction = (filt_prec)(1.0 / Math.Sqrt(1.0 + 0.25 / (q1 * q1)));
				testVar = (filt_prec)(q2 * q2);
				freq_2_correction = (filt_prec)(1.0 / Math.Sqrt(1.0 + 0.25 / testVar));

				// angular frequencies
				omega_1pole = (filt_prec)(PI_x2_OVER_FREQUENCY * f1); //par(1) is filter corner frequency
				omega_2zero = (filt_prec)(PI_x2_OVER_FREQUENCY * f2);

				// choose bigger frequency for gain correction (VB6 logic)
				if (omega_1pole > omega_2zero)
					s_freq_gain_corr = (filt_prec)(((double)LOOP_FREQUENCY + (double)f1) / (double)LOOP_FREQUENCY);
				else
					s_freq_gain_corr = (filt_prec)(((double)LOOP_FREQUENCY + (double)f2) / (double)LOOP_FREQUENCY);

				// preliminary computations
				if (omega_1pole == 0) omega_1pole = (filt_prec)1e-12; // guard
				h1 = (filt_prec)Math.Tan((double)omega_1pole / 2.0);
				if ((double)h1 == 0.0) cotan_of_half_omega_1 = (filt_prec)1e12;
				else cotan_of_half_omega_1 = (filt_prec)(1.0 / (double)h1);

				t_gain = fp.par[ParIndxFG];
				// set default parameters as GAIN_ONLY for start (needed gains will be replaced)
				fp.b2 = 0;
				fp.b1 = 0;
				fp.b0 = t_gain;
				fp.a2 = 0;
				fp.a1 = 0;

				// --- debug: write header and inputs (timestamped) ---
				try {
					string header = string.Format("=== set_iir RUN: {0:O} ===\r\n", DateTime.UtcNow);
					string inputs = string.Format("ftyp={0}, FilterGain={1:R}, freq1={2:R}, Q1={3:R}, freq2={4:R}, Q2={5:R}\r\n",
						fp.ftyp,
						fp.par[ParIndxFG],
						f1,
						q1,
						f2,
						q2
					);
					AppendSetIirLog( header + inputs);
				} catch { /* swallow logging errors */ }
				// Now handle filter type cases (translated from VB6)
				// ============== STACIS FILTERS: only 4 types are needed ======================================================================================
				if (fp.ftyp == (int)filt_types.LPF_1stOr) //case 1: sharp drop at high freq, notch at Nyquist, phase = -90
				{
					// first-order LPF
					h1 = (filt_prec)(1.0 / (1.0 + cotan_of_half_omega_1));
					fp.b1 = (filt_prec)(t_gain * h1);
					fp.b0 = fp.b1;
					fp.a1 = (filt_prec)((1.0 - cotan_of_half_omega_1) * h1);
				} //case 2: normalized
				else if (fp.ftyp == (filt_prec)filt_types.HPF_1stOr) {
					h1 = (filt_prec)(1.0 / (1.0 + cotan_of_half_omega_1));
					fp.a1 = (filt_prec)((1.0 - cotan_of_half_omega_1) * h1);
					fp.b1 = (filt_prec)(-cotan_of_half_omega_1 * h1 * t_gain);
					fp.b0 = -fp.b1;
				} //case 3: normalized maximum unity gain at band pass frequency
				else if (fp.ftyp == (filt_prec)filt_types.LEADLAG_1stOr) {
					// first-order lead/lag general
					fp.a1 = (filt_prec)(omega_2zero - 1.0);
					fp.b0 = t_gain;
					fp.b1 = (filt_prec)((omega_1pole - 1.0) * t_gain);
				} //case 4: general form with Q factors (NOT normalized) LEAD or LAG DOES depend on F1,F2: if F1>F2 - it is "-\_LAG, HiFreq, LoFreq"' if F1<F2 - it is "_/-LEAD, LoFreq, HiFreq"'
				else if (fp.ftyp == (filt_prec)filt_types.LEADLAG_2ndOr_wPk)
				// Max gain can be bigger than 1 if any of Q is bigger than 0.5, generally max gain approx = Q.
				// *****!!***** BE CAREFUL WITH Q!!  IT CAN OVERFLOW IN FIXED POINT CALCULATIONS
				//  Normalized to unity gain at frequencies much bigger/smaller than F1 or F2.
				//  omega_1pole *= freq_1_correction'  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				//  omega_2zero *= freq_2_correction'  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				{
					// second-order lead/lag with peak (general)
					h2 = (filt_prec)(1.0 - (omega_2zero / (2.0 * q2)));
					fp.a1 = (filt_prec)(-2.0 * h2 * Math.Cos((double)omega_2zero));
					fp.a2 = (filt_prec)(h2 * h2);

					h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
					fp.b0 = t_gain;
					fp.b1 = (filt_prec)((-2.0 * h1 * Math.Cos((double)omega_1pole)) * (double)t_gain);
					fp.b2 = (filt_prec)((h1 * h1) * (double)t_gain);
				} // directly enter 2nd order coeffs (b0+b1*z^-1+b2*z^-2)/(1+a1*z^-1+a2*z^-2)
				else if (fp.ftyp == (filt_prec)filt_types.DIRECT_COEFF) {
					// direct coefficients: par fields contain coefficients
					fp.b0 = fp.par[ParIndxFG];
					fp.b1 = f1;
					fp.b2 = f2;
					fp.a1 = -q1; // 20260102 fixed bug, need reversion here because a1,a2 are reversed at the end of function
					fp.a2 = -q2; // 20260102 fixed bug
					// ============== STACIS FILTERS END only 4 needed ============================================================================================

					// ============== ADDITIONAL FILTERS FOR ELECTRODUMP ==========================================================================================
				} //, LEAD_1stOrSemiNorm     'case 5: __/-- F1<F2' HPF with max gain = param4 above F2, !! CAREFUL with FIX POINT !!gain at F1 can be much bigger than 1!!
				else if (fp.ftyp == (filt_prec)filt_types.LEAD_1stOrNorm) {
					//  Case LEAD_1stOrSemiNorm:  'case 18: --\__ Semi Normalized! potentially high gain at high freq! Gain=1 @ (F1+F2)/2.
					//  Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
					// LEAD 1st order normalized
					h1 = t_gain;
					if (omega_1pole >= omega_2zero) {
						fp.a1 = (filt_prec)(omega_1pole - 1.0);
						fp.b1 = (filt_prec)(omega_2zero - 1.0);
						//        If (fp.ftyp = LEAD_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_1pole / omega_2zero) '
					} else {
						fp.a1 = (filt_prec)(omega_2zero - 1.0);
						fp.b1 = (filt_prec)(omega_1pole - 1.0);
						//        If (fp.ftyp = LEAD_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_2zero / omega_1pole) '
					}
					fp.b0 = h1;
					fp.b1 = (filt_prec)(fp.b1 * h1);
				} //, LAG_1stOrSemiNorm          'case 6: --\__  NOT normalized gain at lowest frequency, at highest frequency gain = 1
				else if (fp.ftyp == (filt_prec)filt_types.LAG_1stOr) {
					//  Case LAG_1stOrSemiNorm:   'case 20:--\__ Semi Normalized! potentially high gain at low freq! Gain=1 @ (F1+F2)/2.
					//  Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
					// LAG 1st order
					h1 = t_gain;
					if (omega_1pole <= omega_2zero) {
						fp.a1 = (filt_prec)(omega_1pole - 1.0);
						fp.b1 = (filt_prec)(omega_2zero - 1.0);
						//        If (fp.ftyp = LAG_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_1pole / omega_2zero) '
					} else {
						fp.a1 = (filt_prec)(omega_2zero - 1.0);
						fp.b1 = (filt_prec)(omega_1pole - 1.0);
						//        If (fp.ftyp = LAG_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_2zero / omega_1pole) '
					}
					fp.b0 = h1;
					fp.b1 = (filt_prec)(fp.b1 * h1);
				} //// combines LAG and LEAD 1st order into single 2nd order --\____/--;  --\__ determined by F1,Q1 (float frequency); __/-- determined by F2,Q2
				else if (fp.ftyp == (filt_prec)filt_types.COMBINED_LAG1_LEAD1) {
					//               omega_1pole = PI_x2_OVER_FREQUENCY * fp->par[freq_1]; //par[1] is filter corner frequency
					//               omega_2zero = PI_x2_OVER_FREQUENCY * fp->par[freq_2];

					// combined lag1 + lead1 (build 2nd-order from two 1st-order blocks)
					filt_prec omega_2zero_tmp = 0;  // temp a11, b11 for the LEG, b01 is gain ==1 by default
					filt_prec a11 = 0, b11 = 0;
					filt_prec omega_1pole_tmp = 0;  // temp a11, b11 for the LEG, b02 is gain ==1 by default
					filt_prec a12 = 0, b12 = 0;

					omega_2zero_tmp = (filt_prec)(PI_x2_OVER_FREQUENCY * q1);
					a11 = (filt_prec)(omega_1pole - 1.0);       // normally, and here as well, comes from F1
					b11 = (filt_prec)(omega_2zero_tmp - 1.0);   // from Q1

					// alredy known         omega_2zero = PI_x2_OVER_FREQUENCY * fp->par[freq_2];
					omega_1pole_tmp = (filt_prec)(PI_x2_OVER_FREQUENCY * q2);   // filter corner 2nd frequency __F2_/-Q2--
					b12 = (filt_prec)(omega_2zero - 1.0);       // normally, and here as well, comes from F2
					a12 = (filt_prec)(omega_1pole_tmp - 1.0);   // now comes from Q2

					fp.b2 = (filt_prec)(b11 * b12 * (double)t_gain);
					fp.b1 = (filt_prec)((b11 + b12) * (double)t_gain);
					fp.b0 = t_gain;
					fp.a2 = (filt_prec)(a11 * a12);
					fp.a1 = (filt_prec)(a11 + a12);


					//  Case LAG_1stOrNorm:   'case 19: --\__  normalized maximum unity gain at lowest  frequency
					//    If (omega_1pole <= omega_2zero) Then
					//        h1 = omega_1pole / omega_2zero * t_gain '
					//        fp.a1 = CSng(omega_1pole - 1) '
					//        fp.b1 = CSng(omega_2zero - 1) '
					//    Else
					//        h1 = omega_2zero / omega_1pole * t_gain '
					//        fp.a1 = CSng(omega_2zero - 1) '
					//        fp.b1 = CSng(omega_1pole - 1) '
					//    End If
					//    fp.b0 = CSng(h1)  '
					//    fp.b1 = CSng(fp.b1 * h1) '

				} //case 7: par1 = F1 = corner freq, par2,4 not used, par3 = Q factor Q=0.5 - classic, Q=2, peak approximately 2 times over unity
				else if (fp.ftyp == (filt_prec)filt_types.LPF_2ndOr_wPk) {
					// Updated VB6 logic (IK20210425 fix) for LPF 2nd order with peak
					// case 7: par1 = F1 = corner freq, par2,4 not used, par3 = Q factor Q=0.5 - classic, Q=2, peak approximately 2 times over unity
					// gain = (omega_1pole/2)^2    * 1/(4 *(Q_1]^2) +1) * (FilterGain) <<<< WAS
					// gain = (omega_1pole/2)^2    * (1/4 /(Q_1]^2) +1) * (FilterGain) <<<< should be
					// gain = 0.25*(omega_1pole)^2 * (0.25/(Q_1]^2) +1) * (FilterGain)

					// omega_1pole ;  //correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
					//omega_1pole = omega_1pole * freq_1_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
					h2 = (filt_prec)(omega_1pole * omega_1pole * 0.25);
					h1 = (filt_prec)((0.25 / (q1 * q1)) + 1.0); // IK20210425 fixed, temp use
					h2 = (filt_prec)(h2 * h1 * (double)t_gain * (double)freq_1_correction); //overall gain * correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1

					h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
					fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_1pole));
					fp.a2 = (filt_prec)(h1 * h1);
					fp.b0 = h2;
					fp.b1 = (filt_prec)(2.0 * h2);
					fp.b2 = h2;
				} //case 8:
				else if (fp.ftyp == (filt_prec)filt_types.HPF_2ndOr_wPk) {
					omega_1pole = (filt_prec)(omega_1pole * freq_1_correction);
					h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));

					fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_1pole));
					fp.a2 = (filt_prec)(h1 * h1);

					fp.b0 = t_gain;
					fp.b1 = (filt_prec)(-2.0 * t_gain);
					fp.b2 = t_gain;
				} //case 12:  __/--- , normalized (max gain = Q, can be > 1)
				else if (fp.ftyp == (filt_prec)filt_types.LEAD_2ndOr_wPk) {
					//reverse F1 and F2 to maintain LEAD shape
					omega_1pole = (filt_prec)(omega_1pole * freq_1_correction);     //  correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
					omega_2zero = (filt_prec)(omega_2zero * freq_2_correction);

					if (omega_1pole > omega_2zero) // gain_corr = s_freq_gain_corr * (omega_1pole/omega_2zero)^2
					{
						h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
						h2 = (filt_prec)(1.0 - (omega_2zero / (2.0 * q2)));
						fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_1pole));
						fp.a2 = (filt_prec)(h1 * h1);
						t_gain = (filt_prec)((double)t_gain / (double)s_freq_gain_corr);
						h1 = (filt_prec)(Math.Cos((double)omega_2zero));    // temp use of 'h1' for 'b1' calculation
					} else //omega_1pole <= omega_2zero
					  {
						h1 = (filt_prec)(1.0 - (omega_2zero / (2.0 * q2)));
						h2 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
						fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_2zero));
						fp.a2 = (filt_prec)(h1 * h1);
						t_gain = (filt_prec)((double)t_gain / (double)s_freq_gain_corr);
						h1 = (filt_prec)Math.Cos((double)omega_1pole);  // temp use of 'h1' for 'b1' calculation
					}
					fp.b0 = t_gain;
					fp.b1 = (filt_prec)(-2.0 * h2 * h1 * (double)t_gain);
					fp.b2 = (filt_prec)((h2 * h2) * (double)t_gain);
				} //case 13:--\__  normalized (max gain = Q, can be > 1)
				else if (fp.ftyp == (filt_prec)filt_types.LAG_2ndOr_wPk) {
					// Q-factor gain correction: =(1+1/(2*Q_1)^2)/(1+1/(2*Q_2)^2)
					omega_1pole = (filt_prec)(omega_1pole * freq_1_correction);     // correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
					omega_2zero = (filt_prec)(omega_2zero * freq_2_correction);

					h1 = (filt_prec)(omega_1pole / omega_2zero);
					h1 = (filt_prec)(h1 * h1);
					h2 = (filt_prec)(1.0 + 0.25 / (q2 * q2));
					h2 = (filt_prec)((1.0 + 0.25 / (q1 * q1)) / (double)h2);
					h1 = (filt_prec)(h1 * h2);  // h1 includes F2/F1, Q2 and Q1 gain correction

					//reverse F1 and F2 to maintain LEAD shape
					if (omega_1pole <= omega_2zero) // gain_corr = s_freq_gain_corr * (omega_1pole/omega_2zero)^2
					{
						t_gain = (filt_prec)(s_freq_gain_corr * t_gain * h1);   //' h1 includes F2/F1, Q2 and Q1 gain correction
						h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
						h2 = (filt_prec)(1.0 - (omega_2zero / (2.0 * q2)));
						fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_1pole));
						fp.a2 = (filt_prec)(h1 * h1);
						h1 = (filt_prec)Math.Cos((double)omega_2zero);  // temp use of 'h1' for 'b1' calculation
					} else {
						t_gain = (filt_prec)((double)s_freq_gain_corr * (double)t_gain / (double)h1);
						h1 = (filt_prec)(1.0 - (omega_2zero / (2.0 * q2)));
						h2 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
						fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_2zero));
						fp.a2 = (filt_prec)(h1 * h1);
						h1 = (filt_prec)Math.Cos((double)omega_1pole);  // temp use of 'h1' for 'b1' calculation
					}
					fp.b0 = t_gain;
					fp.b1 = (filt_prec)(-2.0 * h2 * h1 * (double)t_gain);
					fp.b2 = (filt_prec)((h2 * h2) * (double)t_gain);
				} // made from leadlag
				else if (fp.ftyp == (filt_prec)filt_types.PEAK_on_FLAT) {
					h2 = (filt_prec)(0.89 + 0.064 / q1); // Experimental automatic second frequency calculation to have unity gain from both sides of peak
					omega_2zero = (filt_prec)(omega_1pole * h2);
					h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
					h2 = (filt_prec)(1.0 - (omega_2zero / 2.0));
					fp.a1 = (filt_prec)(-2.0 * h1 * Math.Cos((double)omega_1pole));
					fp.a2 = (filt_prec)(h1 * h1);
					t_gain = (filt_prec)((double)t_gain * (double)s_freq_gain_corr);
					fp.b0 = t_gain;
					fp.b1 = (filt_prec)((-2.0 * h2 * Math.Cos((double)omega_2zero)) * (double)t_gain);
					fp.b2 = (filt_prec)((h2 * h2) * (double)t_gain);
				} // made from leadlag
				else if (fp.ftyp == (filt_prec)filt_types.NOTCH) {
					//  h1  = (fp.par[Q_f_1) - 1)'' automatic second frequency calculation to have unity gain from both sides of notch
					//  h2  = 1 / (fp.par(Q_f_1) + 1)'
					//  h2  = h2 * h2'
					//  omega_2zero = omega_1pole * (1 - h1 * h2)' = (1 - (Q - 1)/((Q + 1)^2))

					//    h2 = 0.89 + 0.064 / fp.par(Q_f_1) '' Experimental automatic second frequency calculation to have unity gain from both sides of peak
					h2 = (filt_prec)(0.8208 + 0.1673 / q1 + 0.009105 * q1 + 0.00001825 * q1 * f1);
					omega_2zero = (filt_prec)(omega_1pole * h2);
					h1 = (filt_prec)(1.0 - (omega_1pole / (2.0 * q1)));
					h2 = (filt_prec)(1.0 - (omega_2zero / 2.0));
					fp.a1 = (filt_prec)(-2.0 * h2 * Math.Cos((double)omega_2zero));
					fp.a2 = (filt_prec)(h2 * h2);
					t_gain = (filt_prec)((double)t_gain * (double)s_freq_gain_corr);
					fp.b0 = t_gain;
					fp.b1 = (filt_prec)((-2.0 * h1 * Math.Cos((double)omega_1pole)) * (double)t_gain);
					fp.b2 = (filt_prec)((h1 * h1) * (double)t_gain);
				} //Q regulates "width" of a notch
				else if (fp.ftyp == (filt_prec)filt_types.NOTCH_Sharp) {
					h1 = (filt_prec)(1.0 / (1.0 + cotan_of_half_omega_1 / q1 + cotan_of_half_omega_1 * cotan_of_half_omega_1));
					h2 = (filt_prec)(2.0 * (1.0 - cotan_of_half_omega_1 * cotan_of_half_omega_1) * (double)h1);
					fp.a1 = (filt_prec)h2;
					fp.a2 = (filt_prec)((1.0 - cotan_of_half_omega_1 / q1 + cotan_of_half_omega_1 * cotan_of_half_omega_1) * (double)h1);
					fp.b0 = (filt_prec)(((1.0 + cotan_of_half_omega_1 * cotan_of_half_omega_1) * (double)h1 * (double)t_gain));
					fp.b1 = (filt_prec)h2 * t_gain; // fix: include t_gain on b1 (controller has b1 *= t_gain)
					fp.b2 = fp.b0;
				} //"roof shape BPF" +20dB/dec-peak-20dB/dec' par1=peak freq, par3=Q1=peak mag
				else if (fp.ftyp == (filt_prec)filt_types.BPF) {
					h1 = (filt_prec)(1.0 / (1.0 + cotan_of_half_omega_1 / q1 + cotan_of_half_omega_1 * cotan_of_half_omega_1));
					h2 = (filt_prec)((double)t_gain * (double)cotan_of_half_omega_1 * (double)h1 / (double)q1); //= (fp.par(FilterGain)*cotan_of_half_omega_1) / fp.par(Q_f_1) * h1'
					fp.a1 = (filt_prec)(2.0 * (1.0 - cotan_of_half_omega_1 * cotan_of_half_omega_1) * (double)h1);
					fp.a2 = (filt_prec)((1.0 - cotan_of_half_omega_1 / q1 + cotan_of_half_omega_1 * cotan_of_half_omega_1) * (double)h1);
					fp.b0 = h2;
					fp.b1 = 0;
					fp.b2 = (filt_prec)(-1.0 * (double)h2);
				} //"leaky" integral
				else if (fp.ftyp == (filt_prec)filt_types.PID) {
					h1 = (filt_prec)(fp.par[PID_I_gain] * (1.0 / LOOP_FREQUENCY));      // Integral
					h2 = (filt_prec)(fp.par[PID_D_gain] * (LOOP_FREQUENCY / 1000.0));   // differential, /1000 because D gain is too big, should be in the range 0.001 then mag bend up happens at 100 Hz
																						// par(3) is filter Q, here used as frequency of leak
					fp.a1 = (filt_prec)(PI_x2_OVER_FREQUENCY * q1 - 1.0); //slightly more, then -1.0f - "leaky" integral - ALLOWS GRADUALLY PURGE delay buffers
																		  // experimentally found 0.9999 cannot efficiently remove offset at low gain if OP is way too low,
																		  // but too much 0.99998 can overflow filter internal delay buffers and flip over at start, especially if input pressure only 10 psi above max iso pressure.
																		  //IT DEPENDS ON SAMPLE FREQUENCY -same a1, higher freq gives faster purge!!!
					fp.b0 = (filt_prec)(fp.par[PID_P_gain] + (double)h1 + (double)h2);
					fp.b1 = (filt_prec)(-(fp.par[PID_P_gain] + (2.0 * (double)h2)));
					fp.b2 = h2;
				}
				//  Case GEO_OPT: 'gain = 1 when f > F2, INTENDED USE: F1=0.2, F2=0.5' it is LEDLAG --2nd order with Q1=Q2~0.1
				//      '**** CAUTION WITH FIX POINT !!, VERY HIGH GAIN AT LOW FREQUENCY!!!
				//    h1 = Tan(omega_2zero / 2) 'use as temporary to calculate h2
				//    h2 = (1 - h1) / (1 + h1) '         ' = (1 - tan(PI*f2/fs) / (1 + tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
				//' Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
				//    h1 = (cotan_of_half_omega_1 - 1) / (cotan_of_half_omega_1 + 1) ' = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
				//
				//    fp.a1 = CSng(-2 * h2) '
				//    fp.a2 = CSng(h2 * h2) '
				//    fp.b0 = CSng(t_gain) '
				//    fp.b1 = CSng(-2 * h1 * t_gain) '
				//    fp.b2 = CSng(h1 * h1 * t_gain) '

				else if (fp.ftyp == (filt_prec)filt_types.VELOCITY) {
					h1 = (filt_prec)Math.Tan((double)omega_2zero / 2.0);    //use 'h1' as temporary to calculate 'h2'
					h2 = (filt_prec)((1.0 - (double)h1) / (1.0 + (double)h1));  //= (1 - Math.Tan(PI*f2/fs) / (1 + Math.Tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
																				// Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
					h1 = (filt_prec)((cotan_of_half_omega_1 - 1.0) / (cotan_of_half_omega_1 + 1.0));    // = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
					fp.a1 = (filt_prec)(-(double)(h1 + h2));
					fp.a2 = (filt_prec)(h1 * h2);

					if (omega_1pole > omega_2zero)
						t_gain = (filt_prec)(t_gain * LOOP_FREQUENCY / (double)f2 * 0.08);  // when F2 > F1
					else
						t_gain = (filt_prec)(t_gain * LOOP_FREQUENCY / (double)f1 * 0.08);  // when F2 < F1

					fp.b0 = (filt_prec)(((1.0 + (double)fp.a1 + (double)fp.a2) * (double)t_gain));  //gain correction '-!- PHASE CORRECTED 2009-25-09
					fp.b2 = (filt_prec)(-1.0 * (double)fp.b0);
				} else if (fp.ftyp == (filt_prec)filt_types.Position) {
					h1 = (filt_prec)Math.Tan((double)omega_2zero / 2.0);        // use 'h1' as temporary to calculate 'h2'
					h2 = (filt_prec)((1.0 - (double)h1) / (1.0 + (double)h1));  // = (1 - tan(PI*f2/fs) / (1 + tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
																				// Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
					h1 = (filt_prec)((cotan_of_half_omega_1 - 1.0) / (cotan_of_half_omega_1 + 1.0));    // = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
					fp.a1 = (filt_prec)(-(double)(h1 + h2));
					fp.a2 = (filt_prec)(h1 * h2);
					fp.b0 = (filt_prec)(0.5 * ((1.0 + (double)fp.a1 + (double)fp.a2) * (double)t_gain)); // 'fp.b0= -(1.0 + fp.a2 + fp.a1)/2''-!- PHASE CORRECTED 09-25-09
					fp.b1 = fp.b0;
				} else if (fp.ftyp == (filt_prec)filt_types.GAIN_ONLY) {
					// keep defaults
				} else {
					// keep defaults
				}

				// --- debug: log intermediates and final before negation ---
				try {
					string mid = string.Format("omega1={0:R}, omega2={1:R}, freq1_corr={2:R}, freq2_corr={3:R}, s_freq_gain_corr={4:R}, h1={5:R}, h2={6:R}, t_gain={7:R}\r\n",
						omega_1pole, omega_2zero, freq_1_correction, freq_2_correction, s_freq_gain_corr, h1, h2, t_gain);
					System.IO.File.AppendAllText("set_iir_log.txt", mid);

					string before = string.Format("FINAL_BEFORE_NEGATION: a1={0:R}, a2={1:R}, b0={2:R}, b1={3:R}, b2={4:R}\r\n",
						fp.a1, fp.a2, fp.b0, fp.b1, fp.b2);
					System.IO.File.AppendAllText("set_iir_log.txt", before);
				} catch { }
				// Reverse signs of a1,a2 to keep consistent sign convention (VB6: fp.a1 = -fp.a1; fp.a2 = -fp.a2)
				// Reverse a1, a2 to use only ADD (summing) and do not use SUB (no subtraction)
				fp.a1 = (filt_prec)(-fp.a1);
				fp.a2 = (filt_prec)(-fp.a2);

				// --- debug: final after negation
				try {
					string after = string.Format("FINAL_AFTER_NEGATION: a1={0:R}, a2={1:R}, b0={2:R}, b1={3:R}, b2={4:R}\r\n=== set_iir END ===\r\n",
						fp.a1, fp.a2, fp.b0, fp.b1, fp.b2);
					AppendSetIirLog( after);
				} catch { }
			} catch {
				// VB6 used On Error Resume Next; swallow exceptions to mimic that tolerant behavior.
				// If you want visible exceptions while debugging, remove the catch block.
			}
		}

		void Copy_Params_for_Edit(int filter_num)  //???
		{
			filt_prec paramValue;
			int selectedIndex = 0;

			if ((Program.ConnectionType == (int)Program.ConnType.ConnDEMO)) {
				return;
			}

			FilterNumberInChain = filter_num; //-!- IK20260104 function called with this parameter, simplify!
			foreach (var item in ComboFilterTYPE.Items) {
				if ((Lbl_FilterType[filter_num].Text == item.ToString())) {
					selectedIndex = ComboFilterTYPE.Items.IndexOf(item);
					Lbl_FilterType[filter_num].BackColor = OriginalColor;
					break;
				}
			}
			if (selectedIndex >= (ComboFilterTYPE.Items.Count - 1))
				selectedIndex = 0;
			Lbl_FilterType[filter_num].Text = ComboFilterTYPE.Items[selectedIndex].ToString();
			if(ComboFilterTYPE.SelectedIndex != selectedIndex) //IK20261014 prevent triggering event ComboFilterTYPE_SelectedIndexChanged()
				ComboFilterTYPE.SelectedIndex = selectedIndex;
			filt_prec tryParseVal;
			if (filt_prec.TryParse(Lbl_FilterGain[filter_num].Text, out tryParseVal)) {
				paramValue = (filt_prec)tryParseVal; //.Parse(Lbl_FilterGain[filter_num].Text);
				cwNumFilterParam[ParIndxFG].Range = new NationalInstruments.UI.Range(-100, 100);// 0.01;
				cwNumFilterParam[ParIndxFG].Value = (filt_prec)paramValue;
			}
			if (filt_prec.TryParse(Lbl_FilterFreq1[filter_num].Text, out tryParseVal)) {
				paramValue = (filt_prec)tryParseVal; //.filt_prec.Parse(Lbl_FilterFreq1[filter_num].Text);
				cwNumFilterParam[ParIndxF1].Range = new NationalInstruments.UI.Range(0.0001, 1000);// 0.01;
				cwNumFilterParam[ParIndxF1].Value = (filt_prec)paramValue;
			}
			if (filt_prec.TryParse(Lbl_FilterFreq2[filter_num].Text, out tryParseVal)) {
				paramValue = (filt_prec)tryParseVal; //.paramValue = filt_prec.Parse(Lbl_FilterFreq2[filter_num].Text);
				cwNumFilterParam[ParIndxF2].Range = new NationalInstruments.UI.Range(0.0001, 1000);// 0.01;
				cwNumFilterParam[ParIndxF2].Value = (filt_prec)paramValue;
			}

			if (filt_prec.TryParse(Lbl_FilterQ1[filter_num].Text, out tryParseVal)) {
				paramValue = (filt_prec)tryParseVal; //.paramValue = filt_prec.Parse(Lbl_FilterQ1[filter_num].Text);
				cwNumFilterParam[ParIndxQ1].Range = new NationalInstruments.UI.Range(0.001, 1000);// 0.01;
				cwNumFilterParam[ParIndxQ1].Value = (filt_prec)paramValue;
			}

			if (filt_prec.TryParse(Lbl_FilterQ2[filter_num].Text, out tryParseVal)) {
				paramValue = (filt_prec)tryParseVal; //.paramValue = filt_prec.Parse(Lbl_FilterQ2[filter_num].Text);
				cwNumFilterParam[ParIndxQ2].Range = new NationalInstruments.UI.Range(0.001, 1000);// 0.01;
				cwNumFilterParam[ParIndxQ2].Value = (filt_prec)paramValue;
			}

			Lbl_b2.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, b2i]);
			Lbl_b1.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, b1i]);
			Lbl_b0.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, b0i]);
			Lbl_a2.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, a2i]);
			Lbl_a1.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, a1i]);
			set_filt_params_from_user_input(filter_num);
			UpdateCoefficientDiffs(filter_num);
		}


		private string ExtendedHEX(int int_in) {
			string retVal = string.Empty;
			string ConvertedCHAR = string.Empty;
			if (int_in > (9 + 27)) {
				ConvertedCHAR = "-";
				retVal = ConvertedCHAR;
			}
			if (int_in < 10)
				ConvertedCHAR = int_in.ToString("X");
			else
				ConvertedCHAR = Convert.ToChar(((int)'A') + int_in - 10).ToString();
			retVal = ConvertedCHAR;
			return retVal;
		}

		public void Fill_in_filter_params(ref int filter_num) {
			int i = 0;
			int sep_pos = 0;
			string filt_axis_code = null;
			string filter_param_str = null;
			filt_prec paramValue = 0;
			if ((Program.ConnectionType == (int)Program.ConnType.ConnDEMO))
				return;

			filt_axis_code = ExtendedHEX(ComboFilterAxis.SelectedIndex);

			//filter_param_str = Analyzer.GetSend("fpar" + filt_axis_code + Convert.ToString(filter_num) + "0", true);

			//Program.ConnectedController.SendInternal("fpar" + filt_axis_code + Convert.ToString(filter_num) + "0", CommandTypes.ResponseExpected, out filter_param_str, CommandAckTypes.AckExpected);
			formMain.SendInternal("fpar" + filt_axis_code + filter_num.ToString() + "0", CommandTypes.ResponseExpected, out filter_param_str, CommandAckTypes.AckExpected);

			sep_pos = filter_param_str.IndexOf("=") + 1; // Strings.InStr(filter_param_str, "=");
			if ((sep_pos != 0)) {
				retval = filter_param_str.Substring(sep_pos);
				sep_pos = (retval.IndexOf("//") + 1);
				//'if verbose response
				if ((sep_pos != 0)) {
					retval = retval.Substring(0, (sep_pos - 1));
				}
				double tryParseVal;
				if (double.TryParse(retval, out tryParseVal)) {
					paramValue = (filt_prec)double.Parse(retval);
					OriginalFilterParamArray[filter_num, fTi] = paramValue;
					//filter type
					CHANGING_FilterParamArray[filter_num, fTi] = paramValue;
					//filter type
					Filter_CHANGED[filter_num, 0] = false;
				}
			}
			///''''''''''' end filter type
			//'fill parameters
			//0 til 10
			for (i = 1; i < FILTER_PARAMS_TOTAL; i++) {
				filter_param_str = i.ToString();
				//#= filter parameter number [0,1,2,3,4,5], where 0== long ftyp; the rest-> float par[5];
				if (i == 10)
					filter_param_str = "A";
				filter_param_str = filter_num.ToString() + filter_param_str;
				filter_param_str = filt_axis_code + filter_param_str;
				filter_param_str = "fpar" + filter_param_str;

				//filter_param_str = Analyzer.GetSend(filter_param_str, true);
				//Program.ConnectedController.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out filter_param_str, CommandAckTypes.AckExpected);
				formMain.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out filter_param_str, CommandAckTypes.AckExpected);
				sep_pos = (filter_param_str.IndexOf("=") + 1);
				if ((sep_pos != 0)) {
					retval = filter_param_str.Substring(sep_pos);
					sep_pos = (retval.IndexOf("//") + 1);
					// 'if verbose response
					if ((sep_pos != 0)) {
						retval = retval.Substring(0, (sep_pos - 1));
					}

					double tryParseVal;
					if (double.TryParse(retval, out tryParseVal)) {
						paramValue = (filt_prec)double.Parse(retval);
						OriginalFilterParamArray[filter_num, i] = paramValue;
						// filter parameter as float[0 to 4], coefficient [5 to 9]
						CHANGING_FilterParamArray[filter_num, i] = paramValue;
						// make a copy for editing
						if (i < USER_PARAM_NUMBER - 1) {
							Filter_CHANGED[filter_num, i] = false;
						}
					}
				}
			}
		}

		private void LblFilter_Click(object sender, MouseEventArgs e) {
			Label lbl = (Label)sender;
			FilterNumberInChain = int.Parse(lbl.Tag.ToString());
			FrameFilter.Top = (LblFilter[FilterNumberInChain].Top);
			FrameFilter.Visible = true;
			ReadyForUserChange = false; // prevent multiple re-painting On ValueChange
			Copy_Params_for_Edit(FilterNumberInChain);
			ReadyForUserChange = true;
			CheckIfFilterChangedAndReCalculateTFsAndPrediction(FilterNumberInChain);
		}

		private void ChkShowOneFilterTF_CheckedChanged(object sender, EventArgs e) {
			if (((CheckBox)sender).Checked) {
				formMain.ScatterGraphMag.Plots[OneFilterTF_Num].Visible = true;
				formMain.ScatterGraphPhase.Plots[OneFilterTF_Num].Visible = true;
			} else {
				formMain.ScatterGraphMag.Plots[OneFilterTF_Num].Visible = false;
				formMain.ScatterGraphPhase.Plots[OneFilterTF_Num].Visible = false;
			}
		}

		private void ChkShowAxisTF_CheckedChanged(object sender, EventArgs e) {
			if (((CheckBox)sender).Checked) {
				formMain.ScatterGraphMag.Plots[Axis_TF_Num].Visible = true;
				formMain.ScatterGraphPhase.Plots[Axis_TF_Num].Visible = true;
			} else {
				formMain.ScatterGraphMag.Plots[Axis_TF_Num].Visible = false;
				formMain.ScatterGraphPhase.Plots[Axis_TF_Num].Visible = false;
			}
		}

		private void ChkRealTimeUpdateTF_CheckedChanged(object sender, EventArgs e) {
			if (((CheckBox)sender).Checked == true) { // mdr 060518 //not sure it is the best solution to synchronize with ChkShowPredictionPlot
				formMain.ChkShowPredictionPlot.Checked = true;
				formMain.ScatterGraphMag.Plots[Prediction_TF_Num].Visible = true;        //mdr 060318 //
				formMain.ScatterGraphPhase.Plots[Prediction_TF_Num].Visible = true;  //mdr 060318 //

			} else {
				formMain.ChkShowPredictionPlot.Checked = false;
				formMain.ScatterGraphMag.Plots[Prediction_TF_Num].Visible = false;
				formMain.ScatterGraphPhase.Plots[Prediction_TF_Num].Visible = false;

			}
		}

		public void Calc_Prediction() {
			long freq_pt;
			filt_prec prev_freq = 0;
			filt_prec freq_sum_for_test;
			freq_sum_for_test = 0;
			for (freq_pt = 0; (freq_pt < TEST_LTF_ARRAY_LENGTH); freq_pt++) {
				// 0 to 200
				if (formMain.LTFreferenceFreq[freq_pt] > 0) {
					// prepare test array for axis OLTF calculation
					prev_freq = formMain.LTFreferenceFreq[freq_pt];
				}

				Freq_points[freq_pt] = prev_freq;
				//  fill up to the end with the last valid frequency
				freq_sum_for_test = (freq_sum_for_test + prev_freq);
			}

			/* ??? mdr 053118 ??? vvvvvvvvvvvvvvvvvvvvvv
			freq_pt = (freq_pt - 1);
			if ((freq_sum_for_test == 0))
			{
				MessageBox.Show(("Need reference for prediction" + ("\r\n" + ("Please load Reference Transfer Function" + ("\r\n" + "Use \'Get Ref Plot\' button")))), "Reference plot not loaded",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
				return;
			}
			??? ^^^^^^^^^^^^^^^^^^^^^^ ??? */

			//  recalculate axis TF based on real frequecy points
			CalculateAxis_TF(formMain.ChkReversePhase.Checked);
			for (freq_pt = 0; (freq_pt < TEST_LTF_ARRAY_LENGTH); freq_pt++) {
				// 0 to 200
				//  this is reference plot * Axis_Mag
				//mdr 060218 Predicted_Mag[freq_pt] = (Reference_Gain_data[freq_pt] + Axis_Mag[freq_pt]);
				Predicted_Mag[freq_pt] = (formMain.LTFreferenceGain[freq_pt] + Axis_Mag[freq_pt]);
				// in dB already
				//mdr 060218 Predicted_Phase[freq_pt] = Phase_Limit_TF_plus_minus_180((Reference_Phase_data[freq_pt] + Axis_Phase[freq_pt]));
				Predicted_Phase[freq_pt] = Phase_Limit_TF_plus_minus_180((formMain.LTFreferencePhase[freq_pt] + Axis_Phase[freq_pt]));
			}

			//freq_pt --;

			formMain.ScatterGraphMag.Plots[Prediction_TF_Num].PlotXY(Freq_points, Predicted_Mag);
			formMain.ScatterGraphPhase.Plots[Prediction_TF_Num].PlotXY(Freq_points, Predicted_Phase);
			formMain.ScatterGraphMag.Plots[Prediction_TF_Num].Visible = true;
			formMain.ScatterGraphPhase.Plots[Prediction_TF_Num].Visible = true;
		}

		private void CmdUpdateFilter_Click(object sender, EventArgs e) {
			if (Program.formTerminal != null) Program.formTerminal.OnQuitMenuCommand();
			CmdUpdateFil();
		}

		/* artem 060518 updated function, adding support for exporting filter parameters to file */
		private void CmdUpdateFil() {
			int Fnum;
			long Fpar;
			string filter_param_str = string.Empty;
			List<string> file_filt_params = new List<string>();
			string response;
			updateDemoStatus();

			//Checking if Axis value is selected before sending out data
			if (ComboFilterAxis.Text == "") {
				MessageBox.Show("Please enter a valid filter Axis!", "Invalid Axis Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			} else {
				if (!demoMode) {
					for (Fnum = 0; Fnum < MAX_FILTERS_IN_AXIS; Fnum++) {
						for (Fpar = 0; (Fpar < USER_PARAM_NUMBER); Fpar++) {
							// check change of 5 params AND FILTER TYPE
							if ((Opt_ALLfilters_or_ChangedOnly1.Checked == true)) {
								//  update only changed filters
								if ((Filter_CHANGED[Fnum, Fpar] == true)) {
									// flag for individual param
									filter_param_str = ("fpar"
												+ ComboFilterAxis.SelectedIndex.ToString("X")/* (ExtendedHEX(FilterAxis) */
												+ (Fnum.ToString() + Fpar.ToString()));
									filter_param_str = (filter_param_str + ("=" + CHANGING_FilterParamArray[Fnum, Fpar].ToString()));
									//   response = Analyzer.GetSend(filter_param_str, true);
									//  Program.ConnectedController.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
									formMain.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
								}

							} else {
								// update ALL filters
								filter_param_str = ("fpar"
											+ ComboFilterAxis.SelectedIndex.ToString("X")
											+ (Fnum.ToString() + Fpar.ToString()));
								filter_param_str = (filter_param_str + ("=" + CHANGING_FilterParamArray[Fnum, Fpar]));
								// Program.ConnectedController.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
								formMain.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
								//  response = Analyzer.GetSend(filter_param_str, true);
							}
						}

						Fill_in_filter_params(ref Fnum);
						// read back changed filter // update label colors
						// mdr 060418 // in Demo Mode -  do not call function  below
						CheckIfFilterChangedAndReCalculateTFsAndPrediction(Fnum);
					}
				}

				//Writing filter parameter file to all 4 axes
				else if (setAllAxes && demoMode) {
					WriteAxesData(ref file_filt_params, 4);
					SaveParamsToFile(ref file_filt_params);
				}

				//Writing filter parameter file to currently selected axis
				else if (!setAllAxes && demoMode) {
					WriteAxesData(ref file_filt_params, 1);
					SaveParamsToFile(ref file_filt_params);
				}
			}
		}

		private bool ReadyForUserChange = false;

		private void cwNumFilterParam_ValueChanged(object sender, EventArgs e) {
			if (ReadyForUserChange)
				cwNumFilterParam_ValueChanged(int.Parse(((NationalInstruments.UI.WindowsForms.NumericEdit)sender).Tag.ToString()));
		}

		private void cwNumFilterParam_ValueChanged(int Index) {
			if (Index == ParIndxFG) {
				Lbl_FilterGain[FilterNumberInChain].Text = String.Format("{0:0.000}", cwNumFilterParam[ParIndxFG].Value);
				CHANGING_FilterParamArray[FilterNumberInChain, fGi] = (filt_prec)cwNumFilterParam[ParIndxFG].Value;
			}
			else if (Index == ParIndxF1) {
				Lbl_FilterFreq1[FilterNumberInChain].Text = String.Format("{0:0.000}", cwNumFilterParam[ParIndxF1].Value);
				CHANGING_FilterParamArray[FilterNumberInChain, f1i] = (filt_prec)cwNumFilterParam[ParIndxF1].Value;
			}
			else if (Index == ParIndxF2) {
				Lbl_FilterFreq2[FilterNumberInChain].Text = String.Format("{0:0.000}", cwNumFilterParam[ParIndxF2].Value);
				CHANGING_FilterParamArray[FilterNumberInChain, f2i] = (filt_prec)cwNumFilterParam[ParIndxF2].Value;
			}
			else if (Index == ParIndxQ1) {
				Lbl_FilterQ1[FilterNumberInChain].Text = String.Format("{0:0.000}", cwNumFilterParam[ParIndxQ1].Value);
				CHANGING_FilterParamArray[FilterNumberInChain, q1i] = (filt_prec)cwNumFilterParam[ParIndxQ1].Value;
			}
			else if (Index == ParIndxQ2) {
				Lbl_FilterQ2[FilterNumberInChain].Text = String.Format("{0:0.000}", cwNumFilterParam[ParIndxQ2].Value);
				CHANGING_FilterParamArray[FilterNumberInChain, q2i] = (filt_prec)cwNumFilterParam[ParIndxQ2].Value;
			}
			CheckIfFilterChangedAndReCalculateTFsAndPrediction((FilterNumberInChain));
		}


		private void CmdCalcMagPhaseNOW_Click(object sender, EventArgs e) {
			tMag_Phase TF;
			double_iir FiltCoef = new double_iir(); //mdr//float_iir();
			filt_prec dB;
			filt_prec frq;
			int pos;
			string frq_str;
			FiltCoef.a1 = filt_prec.Parse(Lbl_a1.Text);//mdr// float.Parse(Lbl_a1.Text);
			FiltCoef.a2 = filt_prec.Parse(Lbl_a2.Text); //mdr//
			FiltCoef.b0 = filt_prec.Parse(Lbl_b0.Text); //mdr//
			FiltCoef.b1 = filt_prec.Parse(Lbl_b1.Text); //mdr//
			FiltCoef.b2 = filt_prec.Parse(Lbl_b2.Text); //mdr//
			frq_str = TxtFreq.Text;
			filt_prec parsedNumber;
			if (double.TryParse(frq_str, out parsedNumber)) {
				pos = (frq_str.IndexOf(",") + 1);
				if ((pos != 0)) {
					frq_str = frq_str.Substring((pos - 1), 1);//= ".";
				}

				frq = double.Parse(frq_str);
			} else {
				return;
			}

			TF = Mag_Ph_vs_Freq(FiltCoef, frq, LOOP_FREQUENCY);
			dB = (20 * (Math.Log(TF.Mag) / Math.Log(10)));
			LblMag.Text = String.Format("{0:0.00}", dB);
			LblPhase.Text = String.Format("{0:0.00}", TF.Phase);
		}

		private void _cmdSaveParamsIntoFLASH_0_Click(object sender, EventArgs e) {
			updateDemoStatus();
			if (!demoMode)         //artem 060518
			{
				string response = "";
				//-!- IK190505 use either one or another call
				Program.ConnectedController.SendInternal("save", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
				//formMain.SendInternal("save", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			}
		}


		int FilterTypeNumber;
		private void ComboFilterTYPE_SelectedIndexChanged(object sender, EventArgs e) {
			FilterTypeNumber = ComboFilterTYPE.SelectedIndex;
			CHANGING_FilterParamArray[FilterNumberInChain, 0] = ComboFilterTYPE.SelectedIndex;
			Lbl_FilterType[FilterNumberInChain].Text = ComboFilterTYPE.Items[FilterTypeNumber].ToString(); //artem 060118 // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv/
			CheckIfFilterChangedAndReCalculateTFsAndPrediction(FilterNumberInChain);
		}

		private void cmdClearFilterHistory_Click(object sender, EventArgs e) {
			string resp = string.Empty;
			Program.FormMain.SendInternal("fpar>histclr", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void ComboFilterAxis_SelectedIndexChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings) return;

			currentAxis.selectAxis(ComboFilterAxis.SelectedIndex);
			update_damp_controls();
		}

		private bool PneumFilterChain = false;
		private void CheckPneumFilters_CheckedChanged(object sender, EventArgs e) {
			if (CheckPneumFilters.Checked) {
				PneumFilterChain = true;
				cwAuxDampGain.Visible = true;
				LblDampGain.Visible = true;
				PneumFilterChain = true;
				ChkAuxDamping.Visible = true;
			} else {
				PneumFilterChain = false;
				cwAuxDampGain.Visible = false;
				LblDampGain.Visible = false;
				PneumFilterChain = false;
				ChkAuxDamping.Visible = false;
			}
			Calc_Prediction();
		}

		private void cwAuxDampGain_ValueChanged(object sender, EventArgs e) {
			if (PneumFilterChain) {
				Calc_Prediction();
			}
		}

		private void ChkAuxDamping_CheckedChanged(object sender, EventArgs e) {
			Calc_Prediction();
		}



		private void cwNumAxisGain_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings) return;
			Calc_Prediction();
		}

		private void update_damp_controls() {
			if (CheckPneumFilters.Checked) {
				if (ComboFilterAxis.SelectedIndex >= 3 && ComboFilterAxis.SelectedIndex <= 5) {
					cwAuxDampGain.Enabled = true;
					PneumFilterChain = true;
					ChkAuxDamping.Enabled = true;
				} else {
					cwAuxDampGain.Enabled = false;
					PneumFilterChain = false;
					ChkAuxDamping.Enabled = false;
				}
			}
		}
		//IK20260104 disabled set to Max, it is actually bad and dangerous!
		private void NumEdit_UpButtonClicked(object sender, EventArgs e) {
		//	Program.FormMain.SetTOMaxValue((NumericEdit)sender, false);
		}

		//IK20260104 disabled set to Max, it is actually bad and dangerous!
		private void NumEdit_KeyDown(object sender, KeyEventArgs e) {
			//	if (e.KeyData == Keys.Up)
			//		Program.FormMain.SetTOMaxValue((NumericEdit)sender, true);
		}

		//artem 060518 class to hold current axis data, mostly for convenient filter param file output.
		//It's either this or a bunch of case statements for all axis types
		public class AxisData {
			public int axisIndex;
			public string axisType;
			public string[] axisOptions = { "X pos", "Y pos", "tZ pos", "Z pos", "tX pos", "tY pos",
											"X vel", "Y vel", "tZ vel", "Z vel", "tX vel", "tY vel",
											"FF X acc", "FF Y acc", "FF X pos", "FF Y pos",
											"Pressure V1", "Pressure V2", "Pressure V3", "Pressure V4", "Pressure Bal",
											"Aux Filters" };
			public AxisData() {
				axisIndex = 0;
				axisType = "";
			}

			public void selectAxis(int index) {
				axisIndex = index;
				axisType = axisOptions[axisIndex];
			}
		}

		//artem 060518 function for checking demo status
		private bool updateDemoStatus() {
			if (Program.ConnectedController != null)
				demoMode = Program.ConnectedController.CommStatus.Equals(CommStatusStates.DemoMode);
			else
				demoMode = true;
			//Changing tooltip text for demo mode
			if (demoMode) {
				toolTip1.SetToolTip(CmdUpdateFilter, "Export filter parameters to a file");
				toolTip1.SetToolTip(CmdFilterRefresh, "Import filter parameters from a file");
			}
			return demoMode;
		}
		private void frmFilters_Activated(object sender, EventArgs e) {
			updateDemoStatus();             //Run this only once per activation time
			ApplyHasFloorFFUI();
		}

		//artem 060718 function that loads filter parameters from .param file, a lot of parsing and string operations
		private List<string> LoadParamsFromFile() {
			List<string> file_params = new List<string>();
			OpenFileDialog openParameterFileDialog = new OpenFileDialog();
			openParameterFileDialog.Filter = "Data File (*.param)|*.param|All files (*.*)|*.*";
			System.IO.FileStream filestream = null;

			if (openParameterFileDialog.ShowDialog() == DialogResult.OK) {
				file_params.Clear();
				filestream = (System.IO.FileStream)openParameterFileDialog.OpenFile();

				using (StreamReader sr = new StreamReader(filestream)) {
					bool versionMatched = true;
					while (!sr.EndOfStream) {
						//Console.WriteLine(sr.ReadLine());
						string readLine = sr.ReadLine();
						if (versionMatched) {
							if (readLine != string.Empty) {
								if (readLine.Substring(0, 1) != "/") {
									int indexOfComment = readLine.IndexOf("//");
									if (indexOfComment != -1) {
										readLine = readLine.Substring(0, indexOfComment - 1);
									}
									file_params.Add(readLine);
									//SendInternal(readLine + Environment.NewLine, CommandTypes.ResponseExpected, out Buffer, CommandAckTypes.AckExpected);
								} else {
									file_params.Add(readLine);
								}
							}
						}
					}
					sr.Close();
				}
			}
			return file_params;
		}

		// artem 060718 function that parses the string contents of parameter file loaded in LoadParamsFromFile
		private void ParseFilterParams(ref List<string> filt_params) {
			int paramStartIdx, startIdx, axisIdx; //numFilters = 0,
			string searchStr = null;

			// CHECK 1 - find the Gain for the Axis
			axisIdx = currentAxis.axisIndex;
			startIdx = parseGain(ref filt_params);
			if (startIdx <= 0)
				return;
			// CHECK 2 - find the parameters  for the Axis - start Index
			searchStr = "fpar" + axisIdx.ToString("X"); //axisIdx;
			paramStartIdx = -1;
			for (int i = startIdx; i < filt_params.Count; i++) {
				if (filt_params[i].Contains(searchStr)) {
					paramStartIdx = i;
					break;
				}
			}
			//fpar command not found
			if (paramStartIdx == -1) {
				MessageBox.Show("Valid Filter Params Not Found!", "File Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			// CHECK 3 - verify that parameters list is completed -> MAX_PARAMS_IN_FILTER x MAX_FILTERS_IN_AXIS
			// by checking the size of filt_params AND if parameter of that filter has same searchStr pattern (fpar#)
			if ((filt_params.Count >= paramStartIdx + USER_PARAM_NUMBER * MAX_FILTERS_IN_AXIS) &&
						(filt_params[paramStartIdx + USER_PARAM_NUMBER * MAX_FILTERS_IN_AXIS - 1].Contains(searchStr)))
				updateFilterControls(paramStartIdx, MAX_FILTERS_IN_AXIS, filt_params);        //Now populate filter array and form controls
			else {
				MessageBox.Show("Filter Params list is incomplete", "File Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}

		//artem 060618 function that writes axes filter parameters to file from frmFilters
		private void WriteAxesData(ref List<string> filter_params, int numAxes) {
			int Fnum, stopIdx;
			long Fpar;
			string[] fparDescriptions = { "Par[0]=Type;", "Par[1]=Gain;", "Par[2]=Freq.1;", "Par[3]=Freq.2;", "Par[4]=Q_frq1;", "Par[5]=Q_frq2;" };
			//List<string> filter_params = new List<string>();
			string filter_param_tmp = string.Empty;

			filter_params.Add("//// AXES GAINS");
			int curIdx = currentAxis.axisIndex;
			int startIdx = curIdx;

			//Resetting starting and current idx appropriately so that data for all 4 axes is taken
			if (numAxes == 4) {
				if (curIdx % 4 != 0)  //For example if curIdx = 1 to 3, set curIdx=0 at start
				{
					if (curIdx >= 1 && curIdx <= 3) {              //if idx not 0
						curIdx = 0;
						startIdx = curIdx;
					} else if (curIdx >= 5 && curIdx <= 7) {        //if idx not 4
						curIdx = 4;
						startIdx = curIdx;
					} else if (curIdx >= 9 && curIdx <= 11) {     // if idx not 8
						curIdx = 8;
						startIdx = curIdx;
					}
				}
			}
			stopIdx = curIdx + numAxes;

			//Adding axis gains
			/*  mdr 111618
			while (curIdx < stopIdx)
			{
				if (curIdx < 10)  {
					filter_param_tmp = "gain0";
					filter_param_tmp += curIdx.ToString();
					filter_param_tmp += "=" + String.Format("{0:+#0.000}", cwNumAxisGain.Value);
					filter_param_tmp += " //" + currentAxis.axisOptions[curIdx];
				}
				else   {
					filter_param_tmp = "gain";
					filter_param_tmp += curIdx.ToString();
					filter_param_tmp += "=" + String.Format("{0:+#0.000}", cwNumAxisGain.Value);
					filter_param_tmp += " //" + currentAxis.axisOptions[curIdx];
				}
				filter_params.Add(filter_param_tmp);
				curIdx++;
			}
			*/
			while (curIdx < stopIdx) { // ??curIdx < stopIdx??
				filter_param_tmp = "gain" + curIdx.ToString("X2")
					+ "=" + String.Format("{0:+#0.000}", cwNumAxisGain.Value)
					+ " //" + currentAxis.axisOptions[curIdx];
				filter_params.Add(filter_param_tmp);
				curIdx++;
			}

			//Adding filter parameter values
			filter_params.Add("//// FILTERS PARAMETERS");
			for (curIdx = startIdx; curIdx < stopIdx; curIdx++) {
				filter_params.Add("//// Axis " + currentAxis.axisOptions[curIdx]);
				for (Fnum = 0; (Fnum <= (MAX_FILTERS_IN_AXIS - 1)); Fnum++) {
					for (Fpar = 0; (Fpar < USER_PARAM_NUMBER); Fpar++) { // 0 t 5
						//if no filter selected, break out of inner for loop and continue to next filter in array
						if (CHANGING_FilterParamArray[Fnum, 0] == 0) { break; }

						filter_param_tmp = "fpar" + curIdx.ToString("X") + (Fnum.ToString() + Fpar.ToString()); // mdr 111618
																												//if filter type parameter, format to int
						if (Fpar == 0) {
							filter_param_tmp += "=" + String.Format("{0}", CHANGING_FilterParamArray[Fnum, Fpar]);
						} else {
							//Format to float/exp notation depending on value
							if (CHANGING_FilterParamArray[Fnum, Fpar] < 200) {
								filter_param_tmp += "=" + String.Format("{0:+##0.000}", CHANGING_FilterParamArray[Fnum, Fpar]);
							} else {
								filter_param_tmp += "=" + String.Format("{0:+0.00e+00}", CHANGING_FilterParamArray[Fnum, Fpar]);
							}
						}
						filter_param_tmp += " // " + fparDescriptions[Fpar];
						filter_params.Add(filter_param_tmp);
					}
				}
			}
		}

		private void SaveParamsToFile(ref List<string> outData) {
			//Save the file logic

			SaveFileDialog saveParameterFileDialog = new SaveFileDialog();
			DateTime dateTimeNow = DateTime.Now;
			//DC-2020 parameters 04-04-2017 12-49.param
			saveParameterFileDialog.FileName = "DC-2020 parameters " + dateTimeNow.ToString("dd-MM-yyyy HH-mm") + ".param";
			// set filters - this can be done in properties as well
			saveParameterFileDialog.Filter = "Data files (*.param)|*.*";

			//// File name, DC-2020 parameters 04-04-2017 12-49.param
			//// DC-2020 parameters
			//// Date, 04-04-2017
			//// Time, 12:50:56,

			if (saveParameterFileDialog.ShowDialog() == DialogResult.OK) {
				using (StreamWriter sw = new StreamWriter(saveParameterFileDialog.FileName)) {
					sw.WriteLine("//// File name," + System.IO.Path.GetFileName(saveParameterFileDialog.FileName));
					sw.WriteLine("//// DC-2020 parameters");
					sw.WriteLine("//// Date," + dateTimeNow.ToString("dd-MM-yyyy"));
					sw.WriteLine("//// Time," + dateTimeNow.ToString("HH-mm-ss"));
					sw.WriteLine("//// version " + "DEMO");

					foreach (var item in outData) {
						sw.WriteLine(item.ToString());
					}
					sw.Close();
				}
			}
		}

		private void CmdCopyFilters_CheckedChanged(object sender, EventArgs e) {
			setAllAxes = CmdCopyFilters.Checked;
		}

		//artem 060818  function that parses gain, requires for //// AXES GAINS section to be present in file for valid operation
		private int parseGain(ref List<string> filt_params) {
			int gainIdx = -1, axis;  // startIdx1,
			filt_prec axisGain;
			string searchStr = null;

			//Setting axis and its gain
			axis = currentAxis.axisIndex;   // mdr 111518
											//Checking if axis gain section found
			for (int i = 0; i < filt_params.Count; i++) {
				if (filt_params[i].Contains("AXES GAINS")) {
					gainIdx = i;
					break;
				}
			}
			if (gainIdx == -1) {  //'AXES GAINS' section not found
				MessageBox.Show("Valid Filter Axis Gain Not Found!", "File Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return -1;
			}
			//for (int i = gainIdx+1; i < filt_params.Count; i++)
			// Max Number of filters  12 ????
			searchStr = "gain" + axis.ToString("X2");
			int ii = gainIdx + 1;
			int xi = ii + 12;
			gainIdx = -1;
			for (; ii < xi; ii++) {
				if (filt_params[ii].Contains(searchStr)) {
					gainIdx = ii;
					break;
				}
			}

			if (gainIdx == -1) {  //axis gain or axis section not found
				MessageBox.Show("Valid Filter Axis Gain Not Found!", "File Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return -1;
			}
			//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
			//(double.TryParse(filt_params[gainIdx].Substring(filt_params[gainIdx].IndexOf("=")+1, filt_params[gainIdx].Length - filt_params[gainIdx].IndexOf("=")-1) , out axisGain))
			if (double.TryParse(filt_params[gainIdx].Substring(filt_params[gainIdx].IndexOf("=") + 1), out axisGain)) {
				cwNumAxisGain.Value = axisGain;                            //setting axis control gain
			}
			return gainIdx;
		}

		//artem 060718 after parsing data from input parameter file,
		//time to update labels, arrays and other things to allow filter manipulation later
		private void updateFilterControls(int start, int numFilters, List<string> filt_params) {
			filt_prec[] local_filt_par;
			int f_num = 0;

			for (f_num = 0; f_num < numFilters; f_num++) {
				local_filt_par = parseFilter(start, start + USER_PARAM_NUMBER, filt_params);
				//Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}",  local_filt_par[0], local_filt_par[1], local_filt_par[2], local_filt_par[3], local_filt_par[4], local_filt_par[5]);
				start += USER_PARAM_NUMBER;

				//Copying filter values to changed array and labels for use later
				CHANGING_FilterParamArray[f_num, fTi] = local_filt_par[0];
				CHANGING_FilterParamArray[f_num, fGi] = local_filt_par[1];
				CHANGING_FilterParamArray[f_num, f1i] = local_filt_par[2];
				CHANGING_FilterParamArray[f_num, f2i] = local_filt_par[3];
				CHANGING_FilterParamArray[f_num, q1i] = local_filt_par[4];
				CHANGING_FilterParamArray[f_num, q2i] = local_filt_par[5];
				//---vvv mdr 111418  vvv---
				OriginalFilterParamArray[f_num, fTi] = local_filt_par[0];
				OriginalFilterParamArray[f_num, fGi] = local_filt_par[1];
				OriginalFilterParamArray[f_num, f1i] = local_filt_par[2];
				OriginalFilterParamArray[f_num, f2i] = local_filt_par[3];
				OriginalFilterParamArray[f_num, q1i] = local_filt_par[4];
				OriginalFilterParamArray[f_num, q2i] = local_filt_par[5];
				//---^^^^^^^^^^^^^--------
				int filtType = (Int32)local_filt_par[0];
				Lbl_FilterType[f_num].Text = ComboFilterTYPE.Items[filtType].ToString();
				Lbl_FilterGain[f_num].Text = string.Format("{0:0.00}", CHANGING_FilterParamArray[f_num, fGi]);
				Lbl_FilterFreq1[f_num].Text = string.Format("{0:0.00}", CHANGING_FilterParamArray[f_num, f1i]);
				Lbl_FilterFreq2[f_num].Text = string.Format("{0:0.00}", CHANGING_FilterParamArray[f_num, f2i]);
				Lbl_FilterQ1[f_num].Text = string.Format("{0:0.00}", CHANGING_FilterParamArray[f_num, q1i]);
				Lbl_FilterQ2[f_num].Text = string.Format("{0:0.00}", CHANGING_FilterParamArray[f_num, q2i]);
			}
			FilterNumberInChain = 0;
			//Copying filter array values to controls
			cwNumFilterParam[ParIndxFG].Value = CHANGING_FilterParamArray[FilterNumberInChain, fGi]; //Gain
			cwNumFilterParam[ParIndxF1].Value = CHANGING_FilterParamArray[FilterNumberInChain, f1i]; //F1
			cwNumFilterParam[ParIndxF2].Value = CHANGING_FilterParamArray[FilterNumberInChain, f2i]; //F2
			cwNumFilterParam[ParIndxQ1].Value = CHANGING_FilterParamArray[FilterNumberInChain, q1i]; //Q1
			cwNumFilterParam[ParIndxQ2].Value = CHANGING_FilterParamArray[FilterNumberInChain, q2i]; //Q2
			Calc_Prediction();                                                              //calculating filter transfer functions
			ComboFilterTYPE.SelectedIndex = (int)CHANGING_FilterParamArray[0, 0];          //Selecting first filter type as active
		}

		//artem 060718  parse filter data from 6 rows of list into filter array for saving into form controls and arrays later
		private filt_prec[] parseFilter(int start, int end, List<string> filt_params) {
			filt_prec[] filter = new filt_prec[6];          // new filter array to be filled in from param file string List
			string searchStr = null;
			int startIdx1;
			int j = 0;
			for (int i = start; i < end; i++) {
				searchStr = filt_params[i];
				startIdx1 = searchStr.LastIndexOf("=") + 1;               //parsing filt type
				double tempVar;
				if (double.TryParse(searchStr.Substring(startIdx1, searchStr.Length - startIdx1), out tempVar)) {
					filter[j] = tempVar;
					j++;
				}
			}
			return filter;
		}
	};

#if false // Move it down while enabling code

//Commented out code below removed
#endif //#if false
}

