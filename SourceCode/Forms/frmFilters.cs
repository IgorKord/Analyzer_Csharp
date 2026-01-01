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
		const int PID_P_gain = (int)filt_par.FilterGain;
		const int PID_I_gain = (int)filt_par.freq_1;
		const int PID_D_gain = (int)filt_par.freq_2;
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
		filt_prec[] Reference_Gain_data = new filt_prec[TEST_LTF_ARRAY_LENGTH];
		filt_prec[] Reference_Phase_data = new filt_prec[TEST_LTF_ARRAY_LENGTH];

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

		//filt type, 5 params, 5 coefficients
		const int FILTER_PARAMS_TOTAL = 11;
		//#define PID_P_gain FilterGain
		//#define PID_I_gain freq_1
		//#define PID_D_gain freq_2
		const int MAX_FILTERS_IN_AXIS = 6;
		const int MAX_PARAMS_IN_FILTER = 6;  // Type F1,F2,Q1,Q2 = 6;
		const int USER_PARAM_NUMBER = 5;//USER_PARAM_NUMBER As Long = 5
										//7 filters (0 to 6) in Axis chain, 10 parameters (0 to 5)-user, 6=a1,7=a2,8=b0,9=b1,0xA=b2
										//0 to 5 for STACIS
		const int FB_FILTERS_IN_AXIS = MAX_FILTERS_IN_AXIS - 1;
		//#5
		const int FF_ADAPTIVE_FILT_NUM = MAX_FILTERS_IN_AXIS - 1;

		// (0-5, 0-9) red from controller filters
		filt_prec[,] OriginalFilterParamArray = new filt_prec[MAX_FILTERS_IN_AXIS, FILTER_PARAMS_TOTAL]; //mdr// float -> double
																										 //editing copy of filters
		filt_prec[,] CHANGED_FilterParamArray = new filt_prec[MAX_FILTERS_IN_AXIS, FILTER_PARAMS_TOTAL]; //mdr// float -> double
																										 //check change of 5 params AND FILTER TYPE
		bool[,] Filter_CHANGED = new bool[MAX_FILTERS_IN_AXIS, USER_PARAM_NUMBER + 1];

		//graphical Y coordinate
		//mdr// int FrameFilterTop;
		//New member variables for filter parameter file output
		bool demoMode;                          //artem 060518
		AxisData currentAxis = new AxisData();  //artem 060518
		bool setAllAxes;


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
			Ready = true;
		}

		private void frmFilters_Load(object sender, EventArgs e) {
			ComboFilterAxis.SelectedIndex = 0;
			ComboFilterTYPE.SelectedIndex = 0;
			TxtFreq.Text = "1.0";
			CheckPneumFilters.Checked = true;
			fill_Freq_array();
		}

		private void InitializeLocalControls() {
			Lbl_FilterType.Add(Lbl_FilterType0); Lbl_FilterType.Add(Lbl_FilterType1); Lbl_FilterType.Add(Lbl_FilterType2); Lbl_FilterType.Add(Lbl_FilterType3); Lbl_FilterType.Add(Lbl_FilterType4); Lbl_FilterType.Add(Lbl_FilterType5);
			Lbl_FilterFreq1.Add(Lbl_FilterFreq10); Lbl_FilterFreq1.Add(Lbl_FilterFreq11); Lbl_FilterFreq1.Add(Lbl_FilterFreq12); Lbl_FilterFreq1.Add(Lbl_FilterFreq13); Lbl_FilterFreq1.Add(Lbl_FilterFreq14); Lbl_FilterFreq1.Add(Lbl_FilterFreq15);
			Lbl_FilterQ1.Add(Lbl_FilterQ10); Lbl_FilterQ1.Add(Lbl_FilterQ11); Lbl_FilterQ1.Add(Lbl_FilterQ12); Lbl_FilterQ1.Add(Lbl_FilterQ13); Lbl_FilterQ1.Add(Lbl_FilterQ14); Lbl_FilterQ1.Add(Lbl_FilterQ15);
			Lbl_FilterFreq2.Add(Lbl_FilterFreq20); Lbl_FilterFreq2.Add(Lbl_FilterFreq21); Lbl_FilterFreq2.Add(Lbl_FilterFreq22); Lbl_FilterFreq2.Add(Lbl_FilterFreq23); Lbl_FilterFreq2.Add(Lbl_FilterFreq24); Lbl_FilterFreq2.Add(Lbl_FilterFreq25);
			Lbl_FilterQ2.Add(Lbl_FilterQ20); Lbl_FilterQ2.Add(Lbl_FilterQ21); Lbl_FilterQ2.Add(Lbl_FilterQ22); Lbl_FilterQ2.Add(Lbl_FilterQ23); Lbl_FilterQ2.Add(Lbl_FilterQ24); Lbl_FilterQ2.Add(Lbl_FilterQ25);
			Lbl_FilterGain.Add(Lbl_FilterGain0); Lbl_FilterGain.Add(Lbl_FilterGain1); Lbl_FilterGain.Add(Lbl_FilterGain2); Lbl_FilterGain.Add(Lbl_FilterGain3); Lbl_FilterGain.Add(Lbl_FilterGain4); Lbl_FilterGain.Add(Lbl_FilterGain5);
			LblFilter.Add(LblFilter0); LblFilter.Add(LblFilter1); LblFilter.Add(LblFilter2); LblFilter.Add(LblFilter3); LblFilter.Add(LblFilter4); LblFilter.Add(LblFilter5);
			cwNumFilterParam.Add(cwNumFilterParam0); cwNumFilterParam.Add(cwNumFilterParam1); cwNumFilterParam.Add(cwNumFilterParam2); cwNumFilterParam.Add(cwNumFilterParam3); cwNumFilterParam.Add(cwNumFilterParam4);
			//mdr 010618// enough in frmFilters()//Ready = true;
		}


		// or it will assume all zeros and Filter_FT will be -100 dB and Axis_TF will be -500 dB
		public void SetDefaultFiltParams() {
			int f_num = 0;
			int f_par = 0;
			ComboFilterTYPE.ForeColor = Color.Black;                   //artem 060518

			for (f_num = 0; f_num <= MAX_FILTERS_IN_AXIS - 1; f_num++) {
				OriginalFilterParamArray[f_num, 0] = 0;                //filter type = NO_FILTER
				OriginalFilterParamArray[f_num, 1] = 1;                //gain
				OriginalFilterParamArray[f_num, 2] = 1;                //F1
				OriginalFilterParamArray[f_num, 3] = 0.5;                //F2
				OriginalFilterParamArray[f_num, 4] = 0.5;                //Q1
				OriginalFilterParamArray[f_num, 5] = 0.5;                //Q2
				OriginalFilterParamArray[f_num, 6] = 0;                //a1
				OriginalFilterParamArray[f_num, 7] = 0;                //a2
				OriginalFilterParamArray[f_num, 8] = 1;                //b0
				OriginalFilterParamArray[f_num, 9] = 0;                //b1
				OriginalFilterParamArray[f_num, 10] = 0;                //b2
				Lbl_FilterType[f_num].Text = ComboFilterTYPE.Items[0].ToString();  //set NO_Filters

				//---
				Lbl_FilterGain[f_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[f_num, 1]);
				Lbl_FilterFreq1[f_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[f_num, 2]);
				Lbl_FilterFreq2[f_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[f_num, 3]);
				Lbl_FilterQ1[f_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[f_num, 4]);
				Lbl_FilterQ2[f_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[f_num, 5]);

				for (f_par = 0; f_par <= FILTER_PARAMS_TOTAL - 1; f_par++) {
					CHANGED_FilterParamArray[f_num, f_par] = OriginalFilterParamArray[f_num, f_par];

				}
				FilterNumberInChain = 0;
				cwNumFilterParam[(int)filt_par.FilterGain].Value = OriginalFilterParamArray[FilterNumberInChain, 1]; //Gain
				cwNumFilterParam[(int)filt_par.freq_1].Value = OriginalFilterParamArray[FilterNumberInChain, 2]; //F1
				cwNumFilterParam[(int)filt_par.freq_2].Value = OriginalFilterParamArray[FilterNumberInChain, 3]; //F2
				cwNumFilterParam[(int)filt_par.Q_f_1].Value = OriginalFilterParamArray[FilterNumberInChain, 4]; //Q1
				cwNumFilterParam[(int)filt_par.Q_f_2].Value = OriginalFilterParamArray[FilterNumberInChain, 5]; //Q2
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
			if ((ComboFilterAxis.SelectedIndex > 20)) // Parameter "$": index of axis : position Xp=0,Yp=1,tZp=2,Zp=3,tXp=4,tYp=5; velocity Xv=6,Yv=7,tZv=8,Zv=9,tXv=A,tYv=B; damping Zd=C,tXd=D,tYd=E, balance diagonal=F
			{
				// error, there are only 12 axis gains, gain#$, #=0,1,2,3, $=0-A
				cwNumAxisGain.BackColor = Color.Red;// 12632319;
				cwNumAxisGain.Value = 0;
				cwNumAxisGain.Enabled = false;
				// mdr 060318 // Refresh_FilterParams();
				//  but, refresh  filter chain - there are 20 filter chains
				return;
			}

			if (demoMode) {
				SetDefaultFiltParams();                      //Initialize controls again for easier update from file
				file_filt_params = LoadParamsFromFile();     //Load filter parameters from input file as string List
				ParseFilterParams(ref file_filt_params);     //Setup controls, labels, filter arrays using data from file
			} else {
				cwNumAxisGain.BackColor = Color.White;
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
								cwNumAxisGain.BackColor = Color.White;
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
			if (ComboFilterAxis.SelectedIndex > 21) {
				Refresh_FilterParams();
				return;
			}

			Program.IsReadingControllerSettings = true;


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
					cwNumAxisGain.BackColor = Color.White;
				} else {
					cwNumAxisGain.BackColor = System.Drawing.Color.FromArgb(0xC0C0FF);
				}
			}
			Program.IsReadingControllerSettings = false;
			Refresh_FilterParams();
		}

		void Refresh_FilterParams() {
			// //define indexes for float_iir.par[index]
			// #define FilterGain   0
			// #define freq_1 1
			// #define freq_2 2
			// #define Q_f_1  3
			// #define Q_f_2  4
			// #define PID_P_gain FilterGain
			// #define PID_I_gain freq_1
			// #define PID_D_gain freq_2
			// fpar&$#? returns "fpar&$#=GGG.gg" scientific format displayed; fpar&$#=ggg.gg sets filter parameter
			// & = axis [0-F]== first index of SysData.filters[6][max_filt_in_channel]; SysData.PROXfilt[6][]; SysData.FF_Filt[4][],
			// $= filter number in the axis [0,1,2,3,4,5] = = second index in SysData.filters[NUM_FILT_CHANs][max_filt_in_channel];
			//
			// #= filter parameter number [0,1,2,3,4,5] == long ftyp; float par[5];
			int filter_num;
			FrameFilter.Visible = false;
			if (!demoMode) {
				// retval = Analyzer.GetSend("echo>enab", true);
				string response = string.Empty;
				// Program.ConnectedController.SendInternal("echo>enab", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
				formMain.SendInternal("echo>enab", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			}
			// disable "echo>verb" to speed up
			for (filter_num = 0; (filter_num < (MAX_FILTERS_IN_AXIS)); filter_num++) {
				// 0 to 5 5 params AND FILTER_TYPE
				Fill_in_filter_params(ref filter_num);
				Lbl_FilterType[filter_num].Text = ComboFilterTYPE.Items[(int)OriginalFilterParamArray[filter_num, 0]].ToString();

				Lbl_FilterGain[filter_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[filter_num, 1]);
				// index=0
				Lbl_FilterFreq1[filter_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[filter_num, 2]);
				// index=1
				Lbl_FilterFreq2[filter_num].Text = string.Format("{0:0.00}", OriginalFilterParamArray[filter_num, 3]);
				// index=2
				Lbl_FilterQ1[filter_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[filter_num, 4]);
				// index=3
				Lbl_FilterQ2[filter_num].Text = string.Format("{0:0.000}", OriginalFilterParamArray[filter_num, 5]);
				// index=4
				CheckIfFilterChanged(filter_num);
			}

			Copy_Params_for_Edit(FilterNumberInChain);
		}

		public enum filt_par {

			FilterGain = 0,
			freq_1 = 1,
			freq_2 = 2,
			Q_f_1 = 3,
			Q_f_2 = 4,
		}


		// '''''''''changes color of control and set/clear Boolean Filter_CHANGED(filt_NUM,, FilterParamNum)
		void CheckIfFilterChanged(int Filt_num) {
			int NN;
			bool FilterChanged;
			FilterChanged = false;
			for (NN = 0; (NN <= (USER_PARAM_NUMBER - 1)); NN++) {
				// check params change
				Filter_CHANGED[Filt_num, NN] = false;
			}

			for (NN = 0; (NN <= USER_PARAM_NUMBER); NN++) {
				// check 5 params change AND FILTER TYPE
				if ((CHANGED_FilterParamArray[Filt_num, NN] != OriginalFilterParamArray[Filt_num, NN])) {
					Filter_CHANGED[Filt_num, NN] = true;
					FilterChanged = true;
					if (((NN - 1) == (int)filt_par.FilterGain)) {
						Lbl_FilterGain[Filt_num].BackColor = Color.LightBlue;
					}

					//  -1 because we skip FilterTYPE with index 0
					if (((NN - 1) == (int)filt_par.freq_1)) {
						Lbl_FilterFreq1[Filt_num].BackColor = Color.LightBlue; // 12632319;
					}

					if (((NN - 1) == (int)filt_par.freq_2)) {
						Lbl_FilterFreq2[Filt_num].BackColor = Color.LightBlue;
					}

					if (((NN - 1) == (int)filt_par.Q_f_1)) {
						Lbl_FilterQ1[Filt_num].BackColor = Color.LightBlue;
					}

					if (((NN - 1) == (int)filt_par.Q_f_2)) {
						Lbl_FilterQ2[Filt_num].BackColor = Color.LightBlue;
					}
				}
			}

			// NN;
			if ((FilterChanged == true)) {
				LblFilter[Filt_num].ForeColor = Color.Red;// 2105599;
				LblFilter[Filt_num].Text = ("*" + (((Filt_num + 1)).ToString()));
			} else {
				// not changed, restore white color
				LblFilter[Filt_num].Text = ("  "
							+ (((Filt_num + 1)).ToString() + " "));
				LblFilter[Filt_num].ForeColor = Color.Black;//Color.FromArgb(214, 748, 365);//2147483656;
															// black
				Lbl_FilterGain[Filt_num].BackColor = Color.White;//Color.FromArgb(214, 748, 365);//2147483653;
																 // white
				Lbl_FilterFreq1[Filt_num].BackColor = Color.White;//Color.FromArgb(214, 748, 365);//2147483653;
																  // white
				Lbl_FilterFreq2[Filt_num].BackColor = Color.White;//Color.FromArgb(214, 748, 365);// 2147483653;
																  // white
				Lbl_FilterQ1[Filt_num].BackColor = Color.White;//Color.FromArgb(214, 748, 365); // 2147483653;
															   // white
				Lbl_FilterQ2[Filt_num].BackColor = Color.White;//Color.FromArgb(214, 748, 365);//2147483653;
															   // white
			}

			CalculateFilterTF(Filt_num);
			if ((ChkShowAxisTF.Checked == true)) {
				CalculateAxis_TF();
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

			if ((reverse_Ph == true)) {
				phase_reversal = 180;
			} else {
				phase_reversal = 0;
			}

			//  calculation "horizontally" NOT changing Filt_num untill all freq are calculated
			// initial conditions
			for (i = 0; (i < TEST_LTF_ARRAY_LENGTH); i++) {
				//  0 to 200
				TMpFiltPlot_Phase[i] = 0;
				if ((cwNumAxisGain.Value > 0)) {
					// means it was updated
					TMpFiltPlot_Mag[i] = (20 * (Math.Log(cwNumAxisGain.Value) / Math.Log(10)));
					// convert Axis gain to dB
				} else {
					TMpFiltPlot_Mag[i] = 0;
					//  gain not updated: assume gain of 1 = 0 dB
				}

			}

			//  calculation "horizontally"
			for (Filt_num = 0; (Filt_num <= FB_FILTERS_IN_AXIS); Filt_num++) {
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
			public filt_prec ftyp;
			public filt_prec[] par = new filt_prec[USER_PARAM_NUMBER];
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

			ThisFilterCoeff.b2 = CHANGED_FilterParamArray[Filt_num, 6];
			ThisFilterCoeff.b1 = CHANGED_FilterParamArray[Filt_num, 7];
			ThisFilterCoeff.b0 = CHANGED_FilterParamArray[Filt_num, 8];
			ThisFilterCoeff.a2 = CHANGED_FilterParamArray[Filt_num, 9];
			ThisFilterCoeff.a1 = CHANGED_FilterParamArray[Filt_num, 10];
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
				retTMag.Phase = ((Math.Atan((H.Im / H.Re)) * (180 / Math.PI)) * -1);
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
			tComplex Denom_Conj;
			filt_prec Denom_Mag;
			Denom_Conj = Complex(arg2.Re, -arg2.Im);
			Denom_Mag = arg2.Re * arg2.Re + arg2.Im * arg2.Im;
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
			// FilterParams As float_iir)
			double_iir FilterParams = new double_iir();
			long param;
			float testVar = 0;
			FilterParams.ftyp = CHANGED_FilterParamArray[Filt_Number, 0];
			// ComboFilterTYPE.ListIndex
			FilterParams.par[(int)filt_par.FilterGain] = CHANGED_FilterParamArray[Filt_Number, 1];
			FilterParams.par[(int)filt_par.freq_1] = CHANGED_FilterParamArray[Filt_Number, 2];
			FilterParams.par[(int)filt_par.freq_2] = CHANGED_FilterParamArray[Filt_Number, 3];
			FilterParams.par[(int)filt_par.Q_f_1] = CHANGED_FilterParamArray[Filt_Number, 4];
			FilterParams.par[(int)filt_par.Q_f_2] = CHANGED_FilterParamArray[Filt_Number, 5];
			for (param = 0; (param
						<= (USER_PARAM_NUMBER - 1)); param++) {
				if ((FilterParams.par[param] != 0)) {
					testVar = (testVar + 1);
				}
			}

			if ((testVar == 0)) {
				return;
			}

			// all params are zero, not initiated yet. it will cause 'dived by zero' exception
			//xxx if ((ChkRealTimeUpdateTF.Checked == true))
			{
				set_iir(FilterParams);
				// this recalculates a, b coefficients
				//  update coefficients so we can re-cal axis TF and re-plot
				CHANGED_FilterParamArray[Filt_Number, 6] = FilterParams.b2;
				CHANGED_FilterParamArray[Filt_Number, 7] = FilterParams.b1;
				CHANGED_FilterParamArray[Filt_Number, 8] = FilterParams.b0;
				CHANGED_FilterParamArray[Filt_Number, 9] = FilterParams.a2;
				CHANGED_FilterParamArray[Filt_Number, 10] = FilterParams.a1;
			}

			//  update screen
			if ((Filt_Number == FilterNumberInChain)) {
				Lbl_PC_a1.Text = string.Format("{0:0.0#######}", CHANGED_FilterParamArray[Filt_Number, 10]);
				Lbl_PC_a2.Text = string.Format("{0:0.0#######}", CHANGED_FilterParamArray[Filt_Number, 9]);
				Lbl_PC_b0.Text = string.Format("{0:0.0#######}", CHANGED_FilterParamArray[Filt_Number, 8]);
				Lbl_PC_b1.Text = string.Format("{0:0.0#######}", CHANGED_FilterParamArray[Filt_Number, 7]);
				Lbl_PC_b2.Text = string.Format("{0:0.0#######}", CHANGED_FilterParamArray[Filt_Number, 6]);
				UpdateCoefficientDiffs(Filt_Number);
			}
		}



		public void set_iir(double_iir fp) {
			//On Error Resume Next VBConversions Warning: On Error Resume Next not supported in C# // CAUTION !! it can miss divide by zero

			//' RUNS FROM FLASH ONLY AT STARTUP or FITER CHANGE, DOES NOT NEED ramfuncs
			//#define omega_1pole omega_1pole 'alias name: omega_Numenator
			//#define omega_2zero omega_2zero 'alias name: omega_DEnominator
			filt_prec h1 = 0; //Double h1, h2'
			filt_prec h2 = 0;
			filt_prec t_gain = 0; //Double t_gain, s_freq_gain_corr, cotan_of_half_omega_1'
			filt_prec s_freq_gain_corr = 0;
			filt_prec cotan_of_half_omega_1 = 0;
			filt_prec freq_1_correction = 0;
			filt_prec freq_2_correction = 0;
			filt_prec omega_1pole = 0;
			filt_prec omega_2zero = 0;
			filt_prec testVar = 0;

			PI_x2_OVER_FREQUENCY = Math.PI * 2 / LOOP_FREQUENCY;

			if (fp.par[(int)filt_par.Q_f_1] == 0) {
				fp.par[(int)filt_par.Q_f_1] = 0.5; // prevent division by zero
			}
			if (fp.par[(int)filt_par.Q_f_2] == 0) {
				fp.par[(int)filt_par.Q_f_2] = 0.5; // prevent division by zero
			}

			//mdr//testVar = System.Convert.ToDouble((fp.par[(int)filt_par.Q_f_1]) * (fp.par[(int)filt_par.Q_f_1]));
			testVar = (fp.par[(int)filt_par.Q_f_1]) * (fp.par[(int)filt_par.Q_f_1]);
			//it is checked above! If testVar <> 0 Then ' denominator is not zero
			freq_1_correction = (1 / Math.Sqrt(1 + 0.25 / ((fp.par[(int)filt_par.Q_f_1]) * (fp.par[(int)filt_par.Q_f_1])))); // 'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
																															 //Else
																															 //    freq_1_correction = 1 'when denominator is zero
																															 //End If
			testVar = (fp.par[(int)filt_par.Q_f_2]) * (fp.par[(int)filt_par.Q_f_1]);
			//it is checked above! If testVar <> 0 Then ' denominator is not zero
			freq_2_correction = (1 / Math.Sqrt(1 + 0.25 / (testVar)));
			//Else
			//    freq_2_correction = 1 'when denominator is zero
			//End If

			omega_1pole = (PI_x2_OVER_FREQUENCY * fp.par[(int)filt_par.freq_1]); //par(1) is filter corner frequency
			omega_2zero = (PI_x2_OVER_FREQUENCY * fp.par[(int)filt_par.freq_2]);

			if (omega_1pole > omega_2zero) //choose bigger of two frequencies for gain correction
			{
				s_freq_gain_corr = ((LOOP_FREQUENCY + fp.par[(int)filt_par.freq_1]) / LOOP_FREQUENCY);
			} else {
				s_freq_gain_corr = ((LOOP_FREQUENCY + fp.par[(int)filt_par.freq_2]) / LOOP_FREQUENCY);
			}
			h1 = (Math.Tan(omega_1pole / 2));
			cotan_of_half_omega_1 = (1 / h1);

			t_gain = (fp.par[(int)filt_par.FilterGain]);
			// set default parameters as GAIN_ONLY for start (needed gains will be replaced)
			fp.b2 = 0;
			fp.b1 = 0;
			fp.b0 = t_gain;//mdr //(float)t_gain;
			fp.a2 = 0;
			fp.a1 = 0;

			// ============== STACIS FILTERS: only 4 needed ============================================================================================
			if ((fp.ftyp) == (filt_prec)filt_types.LPF_1stOr) //case 1: sharp drop at high freq, notch at Nyquist, phase = -90
			{
				h1 = (1 / (1 + cotan_of_half_omega_1));
				fp.b1 = (t_gain * h1); //
				fp.b0 = fp.b1; //
				fp.a1 = (filt_prec)((1 - cotan_of_half_omega_1) * h1);

			} //case 2: normalized
			else if ((fp.ftyp) == (filt_prec)filt_types.HPF_1stOr) {
				h1 = (filt_prec)(1 / (1 + cotan_of_half_omega_1));
				fp.a1 = (filt_prec)((1 - cotan_of_half_omega_1) * h1);
				fp.b1 = (filt_prec)(-cotan_of_half_omega_1 * h1 * t_gain);
				fp.b0 = -fp.b1;

			} //case 3: normalized maximum unity gain at band pass frequency
			else if ((fp.ftyp) == (filt_prec)filt_types.LEADLAG_1stOr) {
				//  if (omega_1pole <= omega_2zero) t_gain = omega_1pole / omega_2zero * t_gain'
				fp.a1 = (omega_2zero - 1);
				//  fp.a2 = 0'set to 0 above "switch(fp.ftyp){}"
				fp.b0 = t_gain;
				fp.b1 = (omega_1pole - 1) * t_gain;
				//  fp.b2 = 0'set to 0 above "switch(fp.ftyp){}"

			} //case 4: general form with Q factors (NOT normalized) LEAD or LAG DOES depend on F1,F2: if F1>F2 - it is "-\_LAG, HiFreq, LoFreq"' if F1<F2 - it is "_/-LEAD, LoFreq, HiFreq"'
			else if ((fp.ftyp) == (filt_prec)filt_types.LEADLAG_2ndOr_wPk) {
				// Max gain can be bigger than 1 if any of Q is bigger than 0.5, generally max gain approx = Q.
				// *****!!***** BE CAREFUL WITH Q!!  IT CAN OVERFLOW IN FIXED POINT CALCULATIONS
				//  Normalized to unity gain at frequencies much bigger/smaller than F1 or F2.
				//  omega_1pole *= freq_1_correction'  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				//  omega_2zero *= freq_2_correction'  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1

				h2 = (1 - (omega_2zero / (2 * fp.par[(int)filt_par.Q_f_2])));
				fp.a1 = (-2 * h2 * Math.Cos(omega_2zero)); //
				fp.a2 = (h2 * h2);

				h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
				fp.b0 = t_gain;
				fp.b1 = (-2 * h1 * Math.Cos(omega_1pole)) * t_gain;
				fp.b2 = (h1 * h1) * t_gain;

			} // directly enter 2nd order coeffs (b0+b1*z^-1+b2*z^-2)/(1+a1*z^-1+a2*z^-2)
			else if ((fp.ftyp) == (filt_prec)filt_types.DIRECT_COEFF) {
				fp.b0 = fp.par[(int)filt_par.FilterGain];
				fp.b1 = fp.par[(int)filt_par.freq_1];
				fp.b2 = fp.par[(int)filt_par.freq_2];

				fp.a1 = fp.par[(int)filt_par.Q_f_1];
				fp.a2 = fp.par[(int)filt_par.Q_f_2];

				// FilterGain  Q_f_1  Q_f_2 freq_1 filt_par.freq_2

				// ============== STACIS FILTERS END only 4 needed ============================================================================================

				// ============== ADDITIONAL FILTERS FOR ELECTRODUMP ==========================================================================================
			} //, LEAD_1stOrSemiNorm     'case 5: __/-- F1<F2' HPF with max gain = param4 above F2, !! CAREFUL with FIX POINT !!gain at F1 can be much bigger than 1!!
			else if ((fp.ftyp) == (filt_prec)filt_types.LEAD_1stOrNorm) {
				//  Case LEAD_1stOrSemiNorm:  'case 18: --\__ Semi Normalized! potentially high gain at high freq! Gain=1 @ (F1+F2)/2.
				//  Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
				h1 = t_gain; //1# '
				if (omega_1pole >= omega_2zero) {
					fp.a1 = (omega_1pole - 1);
					fp.b1 = (omega_2zero - 1);
					//        If (fp.ftyp = LEAD_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_1pole / omega_2zero) '
				} else {
					fp.a1 = (omega_2zero - 1);
					fp.b1 = (omega_1pole - 1);
					//        If (fp.ftyp = LEAD_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_2zero / omega_1pole) '
				}
				fp.b0 = h1;
				fp.b1 = (filt_prec)(fp.b1 * h1);

			} //, LAG_1stOrSemiNorm          'case 6: --\__  NOT normalized gain at lowest frequency, at highest frequency gain = 1
			else if ((fp.ftyp) == (filt_prec)filt_types.LAG_1stOr) {
				//  Case LAG_1stOrSemiNorm:   'case 20:--\__ Semi Normalized! potentially high gain at low freq! Gain=1 @ (F1+F2)/2.
				//  Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
				h1 = t_gain; //1# '
				if (omega_1pole <= omega_2zero) {
					fp.a1 = (omega_1pole - 1);
					fp.b1 = (omega_2zero - 1);
					//        If (fp.ftyp = LAG_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_1pole / omega_2zero) '
				} else {
					fp.a1 = (omega_2zero - 1);
					fp.b1 = (omega_1pole - 1);
					//        If (fp.ftyp = LAG_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_2zero / omega_1pole) '
				}
				fp.b0 = h1;
				fp.b1 = (filt_prec)(fp.b1 * h1);

			} //// combines LAG and LEAD 1st order into single 2nd order --\____/--;  --\__ determined by F1,Q1 (float frequency); __/-- determined by F2,Q2
			else if ((fp.ftyp) == (filt_prec)filt_types.COMBINED_LAG1_LEAD1) {
				//               omega_1pole = PI_x2_OVER_FREQUENCY * fp->par[freq_1]; //par[1] is filter corner frequency
				//               omega_2zero = PI_x2_OVER_FREQUENCY * fp->par[freq_2];

				filt_prec omega_2zero_tmp = 0; //;// temp a11, b11 for the LEG, b01 is gain ==1 by default
				filt_prec a11 = 0;
				filt_prec b11 = 0;
				filt_prec omega_1pole_tmp = 0; //;// temp a11, b11 for the LEG, b02 is gain ==1 by default
				filt_prec a12 = 0;
				filt_prec b12 = 0;

				omega_2zero_tmp = (PI_x2_OVER_FREQUENCY * fp.par[(int)filt_par.Q_f_1]);
				a11 = (omega_1pole - 1);                 // normally, and here as well, comes from F1
				b11 = omega_2zero_tmp - 1;               // from Q1

				////alredy known         omega_2zero = PI_x2_OVER_FREQUENCY * fp->par[freq_2];

				omega_1pole_tmp = (PI_x2_OVER_FREQUENCY * fp.par[(int)filt_par.Q_f_2]); //; // filter corner 2nd frequency __F2_/-Q2--
				b12 = (omega_2zero - 1);                  // normally, and here as well, comes from F2
				a12 = omega_1pole_tmp - 1;                // now comes from Q2

				fp.b2 = b11 * b12 * t_gain;
				fp.b1 = (b11 + b12) * t_gain;
				fp.b0 = t_gain;
				fp.a2 = a11 * a12;
				fp.a1 = a11 + a12;


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
			else if ((fp.ftyp) == (filt_prec)filt_types.LPF_2ndOr_wPk) {
				// case 7: par1 = F1 = corner freq, par2,4 not used, par3 = Q factor Q=0.5 - classic, Q=2, peak approximately 2 times over unity

				// IK20210425 calculations were wrong !
				// gain = (omega_1pole/2)^2    * 1/(4 *(Q_1]^2) +1) * (FilterGain) <<<< WAS
				// gain = (omega_1pole/2)^2    * (1/4 /(Q_1]^2) +1) * (FilterGain) <<<< should be
				// gain = 0.25*(omega_1pole)^2 * (0.25/(Q_1]^2) +1) * (FilterGain)

				// omega_1pole ;  //correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				//omega_1pole = omega_1pole * freq_1_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				h2 = omega_1pole * omega_1pole * 0.25; //      'temp use
				h1 = (0.25 / fp.par[(int)filt_par.Q_f_1] * fp.par[(int)filt_par.Q_f_1]) + 1;    // IK20210425 fixed, temp use
				h2 = h2 * h1 * t_gain; //overall gain * correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1

				h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));

				fp.a1 = (-2 * h1 * Math.Cos(omega_1pole));
				fp.a2 = (h1 * h1);
				fp.b0 = h2;
				fp.b1 = (2 * h2);
				fp.b2 = h2;

			} //case 8:
			else if ((fp.ftyp) == (filt_prec)filt_types.HPF_2ndOr_wPk) {
				omega_1pole = omega_1pole * freq_1_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));

				fp.a1 = (-2 * h1 * Math.Cos(omega_1pole));
				fp.a2 = (h1 * h1);

				fp.b0 = t_gain;
				fp.b1 = (-2 * t_gain);
				fp.b2 = t_gain;


			} //case 12:  __/--- , normalized (max gain = Q, can be > 1)
			else if ((fp.ftyp) == (filt_prec)filt_types.LEAD_2ndOr_wPk) {
				//reverse F1 and F2 to maintain LEAD shape
				omega_1pole = omega_1pole * freq_1_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				omega_2zero = omega_2zero * freq_2_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				if (omega_1pole > omega_2zero) // gain_corr = s_freq_gain_corr * (omega_1pole/omega_2zero)^2
				{
					h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
					h2 = (1 - (omega_2zero / (2 * fp.par[(int)filt_par.Q_f_2])));
					fp.a1 = (-2 * h1 * Math.Cos(omega_1pole));
					fp.a2 = (h1 * h1);

					t_gain = t_gain / s_freq_gain_corr;
					h1 = (Math.Cos(omega_2zero)); //temp use for b1 calculation
				} else //omega_1pole <= omega_2zero
				  {
					h1 = (1 - (omega_2zero / (2 * fp.par[(int)filt_par.Q_f_2])));
					h2 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
					fp.a1 = (-2 * h1 * Math.Cos(omega_2zero));
					fp.a2 = (h1 * h1);

					t_gain = t_gain / s_freq_gain_corr;
					h1 = (filt_prec)(Math.Cos(omega_1pole)); //temp use for b1 calculation
				}
				fp.b0 = t_gain;
				fp.b1 = (-2 * h2 * h1 * t_gain);
				fp.b2 = (h2 * h2 * t_gain);

			} //case 13:--\__  normalized (max gain = Q, can be > 1)
			else if ((fp.ftyp) == (filt_prec)filt_types.LAG_2ndOr_wPk) {

				// Q-factor gain correction: =(1+1/(2*Q_1)^2)/(1+1/(2*Q_2)^2)
				omega_1pole = omega_1pole * freq_1_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				omega_2zero = omega_2zero * freq_2_correction; //  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1

				h1 = omega_1pole / omega_2zero;
				h1 = h1 * h1;
				h2 = (1 + 0.25 / ((fp.par[(int)filt_par.Q_f_2]) * (fp.par[(int)filt_par.Q_f_2])));
				h2 = ((1 + 0.25 / ((fp.par[(int)filt_par.Q_f_1]) * (fp.par[(int)filt_par.Q_f_1]))) / h2);
				h1 = h1 * h2; // h1 includes F2/F1, Q2 and Q1 gain correction

				//reverse F1 and F2 to maintain LEAD shape
				if (omega_1pole <= omega_2zero) // gain_corr = s_freq_gain_corr * (omega_1pole/omega_2zero)^2
				{
					t_gain = s_freq_gain_corr * t_gain * h1; //' h1 includes F2/F1, Q2 and Q1 gain correction

					h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
					h2 = (1 - (omega_2zero / (2 * fp.par[(int)filt_par.Q_f_2])));
					fp.a1 = (-2 * h1 * Math.Cos(omega_1pole));
					fp.a2 = (h1 * h1);
					h1 = (filt_prec)(Math.Cos(omega_2zero)); //temp use for b1 calculation
				} else //omega_1pole > omega_2zero
				  {
					t_gain = s_freq_gain_corr * t_gain / h1; //' h1 includes F2/F1, Q2 and Q1 gain correction

					h1 = (1 - (omega_2zero / (2 * fp.par[(int)filt_par.Q_f_2])));
					h2 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
					fp.a1 = (-2 * h1 * Math.Cos(omega_2zero));
					fp.a2 = (h1 * h1);
					h1 = (Math.Cos(omega_1pole)); //temp use for b1 calculation
				}
				fp.b0 = t_gain;
				fp.b1 = (-2 * h2 * h1 * t_gain);
				fp.b2 = (h2 * h2 * t_gain);

			} // made from leadlag
			else if ((fp.ftyp) == (filt_prec)filt_types.PEAK_on_FLAT) {
				h2 = (0.89 + 0.064 / fp.par[(int)filt_par.Q_f_1]); //' Experimental automatic second frequency calculation to have unity gain from both sides of peak
				omega_2zero = omega_1pole * h2;
				h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
				h2 = (1 - (omega_2zero / 2));
				fp.a1 = (-2 * h1 * Math.Cos(omega_1pole));
				fp.a2 = (h1 * h1);
				t_gain = t_gain * s_freq_gain_corr;
				fp.b0 = t_gain;
				fp.b1 = (-2 * h2 * Math.Cos(omega_2zero) * t_gain);
				fp.b2 = (h2 * h2 * t_gain);

			} // made from leadlag
			else if ((fp.ftyp) == (filt_prec)filt_types.NOTCH) {

				//  h1  = (fp.par[Q_f_1) - 1)'' automatic second frequency calculation to have unity gain from both sides of notch
				//  h2  = 1 / (fp.par(Q_f_1) + 1)'
				//  h2  = h2 * h2'
				//  omega_2zero = omega_1pole * (1 - h1 * h2)' = (1 - (Q - 1)/((Q + 1)^2))

				//    h2 = 0.89 + 0.064 / fp.par(Q_f_1) '' Experimental automatic second frequency calculation to have unity gain from both sides of peak
				h2 = (0.8208 + 0.1673 / fp.par[(int)filt_par.Q_f_1] + 0.009105 * fp.par[(int)filt_par.Q_f_1] + 0.00001825 * fp.par[(int)filt_par.Q_f_1] * fp.par[(int)filt_par.freq_1]);
				omega_2zero = omega_1pole * h2; //
				h1 = (1 - (omega_1pole / (2 * fp.par[(int)filt_par.Q_f_1])));
				h2 = (1 - (omega_2zero / 2));
				fp.a1 = (-2 * h2 * Math.Cos(omega_2zero));
				fp.a2 = (h2 * h2);

				t_gain = t_gain * s_freq_gain_corr;

				fp.b0 = t_gain;
				fp.b1 = (-2 * h1 * Math.Cos(omega_1pole) * t_gain);
				fp.b2 = (h1 * h1 * t_gain);

			} //Q regulates "width" of a notch
			else if ((fp.ftyp) == (filt_prec)filt_types.NOTCH_Sharp) {
				h1 = (1 / (1 + cotan_of_half_omega_1 / fp.par[(int)filt_par.Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1));
				h2 = (2 * (1 - cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1);

				fp.a1 = h2;
				fp.a2 = ((1 - cotan_of_half_omega_1 / fp.par[(int)filt_par.Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1);
				fp.b0 = ((1 + cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1 * t_gain);
				fp.b1 = h2;
				fp.b2 = fp.b0;

			} //"roof shape BPF" +20dB/dec-peak-20dB/dec' par1=peak freq, par3=Q1=peak mag
			else if ((fp.ftyp) == (filt_prec)filt_types.BPF) {
				h1 = (1 / (1 + cotan_of_half_omega_1 / fp.par[(int)filt_par.Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1));
				h2 = t_gain * cotan_of_half_omega_1 * h1 / fp.par[(int)filt_par.Q_f_1]; //= (fp.par(FilterGain)*cotan_of_half_omega_1) / fp.par(Q_f_1) * h1'

				fp.a1 = (2 * (1 - cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1); //
				fp.a2 = ((1 - cotan_of_half_omega_1 / fp.par[(int)filt_par.Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1);
				fp.b0 = h2;
				fp.b1 = 0;
				fp.b2 = (-h2);

			} //"leaky" integral
			else if ((fp.ftyp) == (filt_prec)filt_types.PID) {
				h1 = (fp.par[PID_I_gain] * 1 / LOOP_FREQUENCY); //      ' Integral
				h2 = (fp.par[PID_D_gain] * LOOP_FREQUENCY / 1000); //   ' differential, /1000 because D gain is too big, should be in the range 0.001 then mag bend up happens at 100 Hz
																   //par(3) is filter Q, here used as frequency of leak
				fp.a1 = PI_x2_OVER_FREQUENCY * fp.par[(int)filt_par.Q_f_1] - 1; //slightly more, then -1.0f - "leaky" integral - ALLOWS GRADUALLY PURGE delay buffers
																				// experimentally found 0.9999 cannot efficiently remove offset at low gain if OP is way too low,
																				// but too much 0.99998 can overflow filter internal delay buffers and flip over at start, especially if input pressure only 10 psi above max iso pressure.
																				//IT DEPENDS ON SAMPLE FREQUENCY -same a1, higher freq gives faster purge!!!
				fp.b0 = (fp.par[PID_P_gain] + h1 + h2);
				fp.b1 = (-(fp.par[PID_P_gain] + (2 * h2)));
				fp.b2 = h2;


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

			} else if ((fp.ftyp) == (filt_prec)filt_types.VELOCITY) {
				h1 = (Math.Tan(omega_2zero / 2)); //use as temporary to calculate h2
				h2 = ((1 - h1) / (1 + h1)); //         ' = (1 - Math.Tan(PI*f2/fs) / (1 + Math.Tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
											// Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
				h1 = ((cotan_of_half_omega_1 - 1) / (cotan_of_half_omega_1 + 1)); // = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
				fp.a1 = (-(h1 + h2));
				fp.a2 = (float)(h1 * h2);

				if (omega_1pole > omega_2zero) {
					t_gain = t_gain * LOOP_FREQUENCY / (fp.par[(int)filt_par.freq_2]) * 0.08; // when F2 > F1
				} else {
					t_gain = t_gain * LOOP_FREQUENCY / (fp.par[(int)filt_par.freq_1]) * 0.08; // when F2 < F1
				}
				fp.b0 = ((1 + (fp.a1) + (fp.a2)) * t_gain); //gain correction '-!- PHASE CORRECTED 09-25-09
				fp.b2 = -(fp.b0);

			} else if ((fp.ftyp) == (filt_prec)filt_types.Position) {
				h1 = (Math.Tan(omega_2zero / 2)); //use as temporary to calculate h2
				h2 = ((1 - h1) / (1 + h1)); //         ' = (1 - tan(PI*f2/fs) / (1 + tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
											// Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
				h1 = ((cotan_of_half_omega_1 - 1) / (cotan_of_half_omega_1 + 1)); // = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
				fp.a1 = (-(h1 + h2));
				fp.a2 = (h1 * h2);
				fp.b0 = (0.5 * (1 + fp.a1 + fp.a2) * t_gain); //        'fp.b0= -(1.0 + fp.a2 + fp.a1)/2''-!- PHASE CORRECTED 09-25-09
				fp.b1 = fp.b0;

			} else if ((fp.ftyp) == (filt_prec)filt_types.GAIN_ONLY) {
				//h1 = h1
			} else {
				//h1 = h1
			}
			//'Reverse a1, a2 to use only ADD (summing) and do not use SUB (no subtraction)
			fp.a1 = -fp.a1;
			fp.a2 = -fp.a2;
		}

		void Copy_Params_for_Edit(int filter_num)  //???
		{
			float paramValue;
			int selectedIndex = 0;

			if ((Program.ConnectionType == (int)Program.ConnType.ConnDEMO)) {
				return;
			}

			FilterNumberInChain = filter_num;
			foreach (var item in ComboFilterTYPE.Items) {
				if ((Lbl_FilterType[filter_num].Text == item.ToString())) {
					selectedIndex = ComboFilterTYPE.Items.IndexOf(item);
					Lbl_FilterType[filter_num].BackColor = Color.White;
					break;
				}
			}
			if (selectedIndex >= (ComboFilterTYPE.Items.Count - 1))
				selectedIndex = 0;
			Lbl_FilterType[filter_num].Text = ComboFilterTYPE.Items[selectedIndex].ToString();
			ComboFilterTYPE.SelectedIndex = selectedIndex;
			float tryParseVal;
			if (float.TryParse(Lbl_FilterGain[filter_num].Text, out tryParseVal)) {
				paramValue = (float)tryParseVal; //.Parse(Lbl_FilterGain[filter_num].Text);
				cwNumFilterParam[(int)filt_par.FilterGain].Range = new NationalInstruments.UI.Range(-100, 100);// 0.01;
				cwNumFilterParam[(int)filt_par.FilterGain].Value = (filt_prec)paramValue;
			}
			if (float.TryParse(Lbl_FilterFreq1[filter_num].Text, out tryParseVal)) {
				paramValue = (float)tryParseVal; //.float.Parse(Lbl_FilterFreq1[filter_num].Text);
				cwNumFilterParam[(int)filt_par.freq_1].Range = new NationalInstruments.UI.Range(0.001, 1000);// 0.01;
				cwNumFilterParam[(int)filt_par.freq_1].Value = (filt_prec)paramValue;
			}
			if (float.TryParse(Lbl_FilterFreq2[filter_num].Text, out tryParseVal)) {
				paramValue = (float)tryParseVal; //.paramValue = float.Parse(Lbl_FilterFreq2[filter_num].Text);
				cwNumFilterParam[(int)filt_par.freq_2].Range = new NationalInstruments.UI.Range(0.001, 1000);// 0.01;
				cwNumFilterParam[(int)filt_par.freq_2].Value = (filt_prec)paramValue;
			}

			if (float.TryParse(Lbl_FilterQ1[filter_num].Text, out tryParseVal)) {
				paramValue = (float)tryParseVal; //.paramValue = float.Parse(Lbl_FilterQ1[filter_num].Text);
				cwNumFilterParam[(int)filt_par.Q_f_1].Range = new NationalInstruments.UI.Range(0.01, 1000);// 0.01;
				cwNumFilterParam[(int)filt_par.Q_f_1].Value = (filt_prec)paramValue;
			}

			if (float.TryParse(Lbl_FilterQ2[filter_num].Text, out tryParseVal)) {
				paramValue = (float)tryParseVal; //.paramValue = float.Parse(Lbl_FilterQ2[filter_num].Text);
				cwNumFilterParam[(int)filt_par.Q_f_2].Range = new NationalInstruments.UI.Range(0.01, 1000);// 0.01;
				cwNumFilterParam[(int)filt_par.Q_f_2].Value = (filt_prec)paramValue;
			}

			Lbl_a1.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, 10]);
			Lbl_a2.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, 9]);
			Lbl_b0.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, 8]);
			Lbl_b1.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, 7]);
			Lbl_b2.Text = string.Format("{0:0.0#######}", OriginalFilterParamArray[filter_num, 6]);

			UpdateCoefficientDiffs(filter_num);
		}

		void UpdateCoefficientDiffs(int filterIndex) {
			// Coefficient indices in arrays: a1=10, a2=9, b0=8, b1=7, b2=6
			// PPM formula: ppm = 100000 * (PC_coeff - Controller_coeff) / PC_coeff

			// Use Controls.Find for null-safety in case labels don't exist
			Control[] diffLabels = new Control[5];
			diffLabels[0] = FrameFiltCoefficients.Controls.Find("Lbl_a1_diff", true).FirstOrDefault();
			diffLabels[1] = FrameFiltCoefficients.Controls.Find("Lbl_a2_diff", true).FirstOrDefault();
			diffLabels[2] = FrameFiltCoefficients.Controls.Find("Lbl_b0_diff", true).FirstOrDefault();
			diffLabels[3] = FrameFiltCoefficients.Controls.Find("Lbl_b1_diff", true).FirstOrDefault();
			diffLabels[4] = FrameFiltCoefficients.Controls.Find("Lbl_b2_diff", true).FirstOrDefault();

			int[] coeffIndices = { 10, 9, 8, 7, 6 }; // a1, a2, b0, b1, b2

			for (int i = 0; i < 5; i++) {
				if (diffLabels[i] != null && diffLabels[i] is Label) {
					Label diffLabel = (Label)diffLabels[i];
					int coeffIndex = coeffIndices[i];

					double pcCoeff = CHANGED_FilterParamArray[filterIndex, coeffIndex];
					double ctrlCoeff = OriginalFilterParamArray[filterIndex, coeffIndex];

					if (pcCoeff == 0) {
						diffLabel.Text = "N/A";
					} else {
						double ppm = 100000.0 * (pcCoeff - ctrlCoeff) / pcCoeff;
						diffLabel.Text = ppm.ToString("+0;-0;0");
					}
				}
			}
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
			float paramValue = 0;
			if ((Program.ConnectionType == (int)Program.ConnType.ConnDEMO))
				return;

			filt_axis_code = ExtendedHEX(ComboFilterAxis.SelectedIndex);

			//filter_param_str = Analyzer.GetSend("fpar" + filt_axis_code + Convert.ToString(filter_num) + "0", true);

			//Program.ConnectedController.SendInternal("fpar" + filt_axis_code + Convert.ToString(filter_num) + "0", CommandTypes.ResponseExpected, out filter_param_str, CommandAckTypes.AckExpected);
			formMain.SendInternal("fpar" + filt_axis_code + Convert.ToString(filter_num) + "0", CommandTypes.ResponseExpected, out filter_param_str, CommandAckTypes.AckExpected);

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
					paramValue = (float)double.Parse(retval);
					OriginalFilterParamArray[filter_num, 0] = paramValue;
					//filter type
					CHANGED_FilterParamArray[filter_num, 0] = paramValue;
					//filter type
					Filter_CHANGED[filter_num, 0] = false;
				}
			}
			///''''''''''' end filter type
			//'fill parameters
			//0 til 10
			for (i = 1; i <= FILTER_PARAMS_TOTAL - 1; i++) {
				filter_param_str = Convert.ToString(i);
				//#= filter parameter number [0,1,2,3,4,5], where 0== long ftyp; the rest-> float par[5];
				if (i == 10)
					filter_param_str = "A";
				filter_param_str = Convert.ToString(filter_num) + filter_param_str;
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
						paramValue = (float)double.Parse(retval);
						OriginalFilterParamArray[filter_num, i] = paramValue;
						// filter parameter as float[0 to 4], coefficient [5 to 9]
						CHANGED_FilterParamArray[filter_num, i] = paramValue;
						// make a copy for editing
						if ((i < USER_PARAM_NUMBER)) {
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
			Copy_Params_for_Edit(FilterNumberInChain);
			Calc_Prediction();  // mdr 060318 // //CalculateFilterTF(FilterNumberInChain);
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
				if ((formMain.LTFreferenceFreq[freq_pt] > 0)) {
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

			freq_pt = (freq_pt - 1);

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
					for (Fnum = 0; (Fnum <= (MAX_FILTERS_IN_AXIS - 1)); Fnum++) {
						for (Fpar = 0; (Fpar <= USER_PARAM_NUMBER); Fpar++) {
							// check change of 5 params AND FILTER TYPE
							if ((Opt_ALLfilters_or_ChangedOnly1.Checked == true)) {
								//  update only changed filters
								if ((Filter_CHANGED[Fnum, Fpar] == true)) {
									// flag for individual param
									filter_param_str = ("fpar"
												+ ComboFilterAxis.SelectedIndex.ToString("X")/* (ExtendedHEX(FilterAxis) */
												+ (Fnum.ToString() + Fpar.ToString()));
									filter_param_str = (filter_param_str + ("=" + CHANGED_FilterParamArray[Fnum, Fpar].ToString()));
									//   response = Analyzer.GetSend(filter_param_str, true);
									//  Program.ConnectedController.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
									formMain.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
								}

							} else {
								// update ALL filters
								filter_param_str = ("fpar"
											+ ComboFilterAxis.SelectedIndex.ToString("X")
											+ (Fnum.ToString() + Fpar.ToString()));
								filter_param_str = (filter_param_str + ("=" + CHANGED_FilterParamArray[Fnum, Fpar]));
								// Program.ConnectedController.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
								formMain.SendInternal(filter_param_str, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
								//  response = Analyzer.GetSend(filter_param_str, true);
							}
						}

						Fill_in_filter_params(ref Fnum);
						// read back changed filter // update label colors
						// mdr 060418 // in Demo Mode -  do not call function  below
						CheckIfFilterChanged(Fnum);

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

		private bool Ready = false;

		private void cwNumFilterParam_ValueChanged(object sender, EventArgs e) {
			if (Ready)
				cwNumFilterParam_ValueChanged(int.Parse(((NationalInstruments.UI.WindowsForms.NumericEdit)sender).Tag.ToString()));
		}

		private void cwNumFilterParam_ValueChanged(int Index) {

			if ((Index == (int)filt_par.FilterGain)) {
				Lbl_FilterGain[FilterNumberInChain].Text = String.Format("{0:0.00}", cwNumFilterParam[(int)filt_par.FilterGain].Value);
				CHANGED_FilterParamArray[FilterNumberInChain, 1] = (filt_prec)cwNumFilterParam[(int)filt_par.FilterGain].Value;
			}
			if ((Index == (int)filt_par.freq_1)) {
				Lbl_FilterFreq1[FilterNumberInChain].Text = String.Format("{0:0.00}", cwNumFilterParam[(int)filt_par.freq_1].Value);
				CHANGED_FilterParamArray[FilterNumberInChain, 2] = (filt_prec)cwNumFilterParam[(int)filt_par.freq_1].Value;
			}
			if ((Index == (int)filt_par.freq_2)) {
				Lbl_FilterFreq2[FilterNumberInChain].Text = String.Format("{0:0.00}", cwNumFilterParam[(int)filt_par.freq_2].Value);
				CHANGED_FilterParamArray[FilterNumberInChain, 3] = (filt_prec)cwNumFilterParam[(int)filt_par.freq_2].Value;
			}
			if ((Index == (int)filt_par.Q_f_1)) {
				Lbl_FilterQ1[FilterNumberInChain].Text = String.Format("{0:0.00}", cwNumFilterParam[(int)filt_par.Q_f_1].Value);
				CHANGED_FilterParamArray[FilterNumberInChain, 4] = (filt_prec)cwNumFilterParam[(int)filt_par.Q_f_1].Value;
			}
			if ((Index == (int)filt_par.Q_f_2)) {
				Lbl_FilterQ2[FilterNumberInChain].Text = String.Format("{0:0.00}", cwNumFilterParam[(int)filt_par.Q_f_2].Value);
				CHANGED_FilterParamArray[FilterNumberInChain, 5] = (filt_prec)cwNumFilterParam[(int)filt_par.Q_f_2].Value;
			}
			CheckIfFilterChanged((FilterNumberInChain));
			Calc_Prediction();
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
			CHANGED_FilterParamArray[FilterNumberInChain, 0] = ComboFilterTYPE.SelectedIndex;
			Lbl_FilterType[FilterNumberInChain].Text = ComboFilterTYPE.Items[FilterTypeNumber].ToString(); //artem 060118                                                                                                                     // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv/
			CheckIfFilterChanged(FilterNumberInChain);
			Calc_Prediction();
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
			if (Ready)
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

		private void NumEdit_UpButtonClicked(object sender, EventArgs e) {
			Program.FormMain.SetTOMaxValue((NumericEdit)sender, false);
		}

		private void NumEdit_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyData == Keys.Up)
				Program.FormMain.SetTOMaxValue((NumericEdit)sender, true);
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
			if ((filt_params.Count >= paramStartIdx + MAX_PARAMS_IN_FILTER * MAX_FILTERS_IN_AXIS) &&
						(filt_params[paramStartIdx + MAX_PARAMS_IN_FILTER * MAX_FILTERS_IN_AXIS - 1].Contains(searchStr)))
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
					for (Fpar = 0; (Fpar <= USER_PARAM_NUMBER); Fpar++) {
						//if no filter selected, break out of inner for loop and continue to next filter in array
						if (CHANGED_FilterParamArray[Fnum, 0] == 0) { break; }

						filter_param_tmp = "fpar" + curIdx.ToString("X") + (Fnum.ToString() + Fpar.ToString()); // mdr 111618
																												//if filter type parameter, format to int
						if (Fpar == 0) {
							filter_param_tmp += "=" + String.Format("{0}", CHANGED_FilterParamArray[Fnum, Fpar]);
						} else {
							//Format to float/exp notation depending on value
							if (CHANGED_FilterParamArray[Fnum, Fpar] < 200) {
								filter_param_tmp += "=" + String.Format("{0:+##0.000}", CHANGED_FilterParamArray[Fnum, Fpar]);
							} else {
								filter_param_tmp += "=" + String.Format("{0:+0.00e+00}", CHANGED_FilterParamArray[Fnum, Fpar]);
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
			filt_prec[] filter;
			int f_num = 0;

			for (f_num = 0; f_num < numFilters; f_num++) {
				filter = parseFilter(start, start + MAX_PARAMS_IN_FILTER, filt_params);
				//Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}",  filter[0], filter[1], filter[2], filter[3], filter[4], filter[5]);
				start = start + MAX_PARAMS_IN_FILTER;

				//Copying filter values to changed array and labels for use later
				CHANGED_FilterParamArray[f_num, 0] = filter[0];
				CHANGED_FilterParamArray[f_num, 1] = filter[1];
				CHANGED_FilterParamArray[f_num, 2] = filter[2];
				CHANGED_FilterParamArray[f_num, 3] = filter[3];
				CHANGED_FilterParamArray[f_num, 4] = filter[4];
				CHANGED_FilterParamArray[f_num, 5] = filter[5];
				//---vvv mdr 111418  vvv---
				OriginalFilterParamArray[f_num, 0] = filter[0];
				OriginalFilterParamArray[f_num, 1] = filter[1];
				OriginalFilterParamArray[f_num, 2] = filter[2];
				OriginalFilterParamArray[f_num, 3] = filter[3];
				OriginalFilterParamArray[f_num, 4] = filter[4];
				OriginalFilterParamArray[f_num, 5] = filter[5];
				//---^^^^^^^^^^^^^--------
				int filtType = (Int32)filter[0];
				Lbl_FilterType[f_num].Text = ComboFilterTYPE.Items[filtType].ToString();
				Lbl_FilterGain[f_num].Text = string.Format("{0:0.00}", CHANGED_FilterParamArray[f_num, 1]);
				Lbl_FilterFreq1[f_num].Text = string.Format("{0:0.00}", CHANGED_FilterParamArray[f_num, 2]);
				Lbl_FilterFreq2[f_num].Text = string.Format("{0:0.00}", CHANGED_FilterParamArray[f_num, 3]);
				Lbl_FilterQ1[f_num].Text = string.Format("{0:0.00}", CHANGED_FilterParamArray[f_num, 4]);
				Lbl_FilterQ2[f_num].Text = string.Format("{0:0.00}", CHANGED_FilterParamArray[f_num, 5]);
			}
			FilterNumberInChain = 0;
			//Copying filter array values to controls
			cwNumFilterParam[(int)filt_par.FilterGain].Value = CHANGED_FilterParamArray[FilterNumberInChain, 1]; //Gain
			cwNumFilterParam[(int)filt_par.freq_1].Value = CHANGED_FilterParamArray[FilterNumberInChain, 2]; //F1
			cwNumFilterParam[(int)filt_par.freq_2].Value = CHANGED_FilterParamArray[FilterNumberInChain, 3]; //F2
			cwNumFilterParam[(int)filt_par.Q_f_1].Value = CHANGED_FilterParamArray[FilterNumberInChain, 4]; //Q1
			cwNumFilterParam[(int)filt_par.Q_f_2].Value = CHANGED_FilterParamArray[FilterNumberInChain, 5]; //Q2
			Calc_Prediction();                                                              //calculating filter transfer functions
			ComboFilterTYPE.SelectedIndex = (Int32)CHANGED_FilterParamArray[0, 0];          //Selecting first filter type as active
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

