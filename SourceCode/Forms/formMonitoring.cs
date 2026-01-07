using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NationalInstruments;
using TMCAnalyzer.Properties;
using Microsoft.VisualBasic;
using NationalInstruments.UI.WindowsForms;

namespace TMCAnalyzer
{
	public partial class formMonitoring : Form
	{
		#region fields
		static int MAX_SCAN_ARRAY_SIZE = 100000;
		static long MAX_CH = 10;
	//    long PositionMonitoring_file = 2;
	//    double Valve_Counts_to_mA = 0.00252;
	//    double Valve_mA_Offset = 90.72;
   //     long Xsize;
   //     long Ysize;
		float StartTime;
		float delayTime;
		int line_counter;
		long MAXmeasINDEX;
		string ASCIIDataStr;
		string[] DataHeaderStr = new string[10];
		long Status;
	//    bool test_timer_ready;
		double CMDperiod;
	//    string start_saving_time;
		//  bool Excitation_status = false;
		internal bool FormTerminalVisible = false;

		double[] tmp_array = new double[MAX_SCAN_ARRAY_SIZE]; //Private((float)(tmp_array(MAX_SCAN_ARRAY_SIZE)));
		//   scan lines, temp array
		object[] OneScanValues = new object[MAX_CH];
		double[,] g_RT_ScanValues = new double[MAX_CH, MAX_SCAN_ARRAY_SIZE]; //Private((float)(g_RT_ScanValues((MAX_CH - 1), MAX_SCAN_ARRAY_SIZE)));
																			 //  100000 scan lines, 12 channels array
		double[,] ScanValues = new double[MAX_CH, MAX_SCAN_ARRAY_SIZE];
		double[,] RelativeScanValues = new double[MAX_CH, MAX_SCAN_ARRAY_SIZE];

		double[,] gRMS_ScanValues = new double[MAX_CH, MAX_SCAN_ARRAY_SIZE];//Private((float)(gRMS_ScanValues((MAX_CH - 1), MAX_SCAN_ARRAY_SIZE)));
		//  100000 scan lines, 12 channels array
		double[,] p_RT_ScanValues = new double[MAX_CH, MAX_SCAN_ARRAY_SIZE]; //Private((float)(p_RT_ScanValues((MAX_CH - 1), MAX_SCAN_ARRAY_SIZE)));
		//  100000 scan lines, 12 channels array
		double[,] pRMS_ScanValues = new double[MAX_CH, MAX_SCAN_ARRAY_SIZE];// Private((float)(pRMS_ScanValues((MAX_CH - 1), MAX_SCAN_ARRAY_SIZE)));
		//  100000 scan lines, 12 channels array
		//long[,] ch_plot_index;
		long Recorded_SCAN_ARRAY_SIZE;
		double[] ScanTimeStamps = new double[MAX_SCAN_ARRAY_SIZE];//Private((double)(ScanTimeStamps(MAX_SCAN_ARRAY_SIZE)));
		double ScanSTARTtimeStamp;
		double MaxScanTimeStamp;
		double[] plt_X_coord;
		double[,] ScanValuesToPlot = new double[0, 0];
		//  10 channels array, 10000 scan lines .
		double[] ScanStDev = new double[MAX_CH];
		//  to calculate how vell it holds postion
		double[] ScanAverage = new double[MAX_CH];
		//  to calculate how vell it holds postion
		double[] ScanSNR =  new double[MAX_CH];
		//  to calculate how vell it holds postion
		double[] ScanPk_Pk = new double[MAX_CH];
		//  to calculate how vell it holds postion
		double[] MinValue = new double[MAX_CH];
		double[] MaxValue = new double[MAX_CH];
		int[] MinIndex = new int[MAX_CH];
		int[] MaxIndex = new int[MAX_CH];
		float XminScale;
		float XmaxScale;
		float YminScale;
		float YmaxScale;
		int ScanNumber = 0;
		long PreviousCh;
		long ChosenCh;
		long Number_of_Records;
		long Total_Number_of_Records;
		bool SaveExsistingData;
		string SaveFileName;
		string DefaultDirectory = "";
		double Time_Start_Saving;
		// for multiple files savings
		object Date_Start_Saving;
		bool File_Saving;
		bool File_Reading;
		//float start_cmd;
		//float interval_cmd;
		//bool User_has_been_Warned;
		//bool RecGeoRMS;
		//bool RecGeoRT;
		//bool RecPiezoRMS;
		//bool RecPiezoRT;
		private formMain HandleToCallingForm = null;
		const int MaxPlotPoints = 100000;
		double[][] RawValues = new double[MAX_CH][];                    // Jagged array of ACTUAL values read from controller. [0=XDC, 1=YDC, 2=ZDC, 3=XAC, 4=YAC, 5=ZAC] [Each-Data-Element]
		double[][] RelativeValues = new double[MAX_CH][];
		bool FormLoading = true;
		bool PlotDataRelativeValues = false;
		bool Zoomed = true;
		List<RadioButton> optChooseCh = new List<RadioButton>();
		List<CheckBox> chkPOSvisible = new List<CheckBox>();
		List<ComboBox> cmbSelData = new List<ComboBox>();
		DateTime[] TimeStamp = new DateTime[MAX_SCAN_ARRAY_SIZE];
		List<Label> lbPosition = new List<Label>();
		List<TextBox> txtVsn = new List<TextBox>();
		public int MonitoringTestType = 0;
		public int XaxisUnits = 0;
		private const int EXCEL_SCAN_ARRAY_SIZE = 60000;
		#endregion

		#region Enums
		public enum TestType
		{
			Monitor_Pos = 0,
			Monitor_Pressure = 1,
			Valves_2020 = 2,
			// Valve_box = 3
			//Buffer_rec = 4
		}
		public enum ValveTestResults
		{
			NO_ERROR = 0,
			ERR_INP_PRESSURE_TOO_LOW =1,
			ERR_DEFL_PRESSURE_DID_NOT_CROSS_40 = 2,
			ERR_DEFL_PRESSURE_DID_NOT_CROSS_20 = 3,
			ERR_VALVES_WERE_NOT_COMPLITELY_SHUT = 4,
			ERR_INFL_PRESSURE_DID_NOT_CROSS_20 = 5,
			ERR_INFL_PRESSURE_DID_NOT_CROSS_40 = 6,
		}

		#endregion

		public formMonitoring()
		{
			InitializeComponent();
		}

		public formMonitoring(formMain HandleToMainForm)
		{
			FormLoading = true;

			InitializeComponent();
			HandleToCallingForm = HandleToMainForm;
			cwGraphDataAcq.Annotations[0].Visible = false;
			InitializeCtrls();
			PlotDataRelativeValues = true;
			cmb_ExcitAxis.SelectedIndex = 0;
			FormLoading = false;
			cmb_TestType.SelectedIndex = 0;
			Program.FormValveTestResult = new FrmValveTestResult();

		}
		string[] XaxisCaption = new string[6];
		private void formMonitoring_Load(object sender, EventArgs e)
		{
			FormLoading = true;
			// User_has_been_Warned = false;
			XminScale = 0;
			XmaxScale = 100;
			PC_RealTime = false;
			YminScale = -60000;
			YmaxScale = 60000;
			cwGraphDataAcq.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			Cmb_X_scale.SelectedIndex = Convert.ToInt32(TestType.Monitor_Pos);
			XaxisUnits = Cmb_X_scale.SelectedIndex;
			XaxisUnits = Cmb_X_scale.SelectedIndex;

			XaxisCaption[0] = "INDEX (sample #) of measurement";
			XaxisCaption[1] = "1st Column";
			XaxisCaption[2] = "2nd Column";
			XaxisCaption[3] = "3rd Column";
			XaxisCaption[4] = "4th Column";
			XaxisCaption[5] = "Time, sec";
			slideXAxis.Caption = XaxisCaption[XaxisUnits];

			MAXmeasINDEX = MAX_SCAN_ARRAY_SIZE;
			this.cwNumExcitAmpl.Value = Program.FormMain.TestGain;
			this.cwNumExcitFreq.Value = Program.FormMain.TestFrequency;
			this.ToggleExcitation.Checked = Program.FormMain.ExcitationStatus;
			FormLoading = false;

			Recorded_SCAN_ARRAY_SIZE = Convert.ToInt64(cwNumFileRecords.Value);
			if (ChkExcelFriendly.Checked)
			{
				if (Recorded_SCAN_ARRAY_SIZE > EXCEL_SCAN_ARRAY_SIZE)
					Recorded_SCAN_ARRAY_SIZE = EXCEL_SCAN_ARRAY_SIZE;
			}
			else
			{
				if (Recorded_SCAN_ARRAY_SIZE > MAX_SCAN_ARRAY_SIZE)
					Recorded_SCAN_ARRAY_SIZE = MAX_SCAN_ARRAY_SIZE;
			}
			cwNumFileRecords.Value = Recorded_SCAN_ARRAY_SIZE;
		}

		private void InitializeCtrls()
		{
			lbPosition.Clear();
			lbPosition.Add(lbPosition0); lbPosition.Add(lbPosition1); lbPosition.Add(lbPosition2); lbPosition.Add(lbPosition3);
			lbPosition.Add(lbPosition4); lbPosition.Add(lbPosition5); lbPosition.Add(lbPosition6); lbPosition.Add(lbPosition7);
			lbPosition.Add(lbPosition8); lbPosition.Add(lbPosition9);

			optChooseCh.Clear();
			optChooseCh.Add(_optChooseCh_1); optChooseCh.Add(_optChooseCh_2); optChooseCh.Add(_optChooseCh_3);
			optChooseCh.Add(_optChooseCh_4); optChooseCh.Add(_optChooseCh_5); optChooseCh.Add(_optChooseCh_6);
			optChooseCh.Add(_optChooseCh_7); optChooseCh.Add(_optChooseCh_8); optChooseCh.Add(_optChooseCh_9);
			optChooseCh.Add(_optChooseCh_10);
			optChooseCh1.Checked = true;

			chkPOSvisible.Clear();
			chkPOSvisible.Add(chkPOSvisible0); chkPOSvisible.Add(chkPOSvisible1); chkPOSvisible.Add(chkPOSvisible2);
			chkPOSvisible.Add(chkPOSvisible3); chkPOSvisible.Add(chkPOSvisible4); chkPOSvisible.Add(chkPOSvisible5);
			chkPOSvisible.Add(chkPOSvisible6); chkPOSvisible.Add(chkPOSvisible7); chkPOSvisible.Add(chkPOSvisible8);
			chkPOSvisible.Add(chkPOSvisible9);

			txtVsn.Clear();
			txtVsn.Add(txtV1sn); txtVsn.Add(txtV2sn); txtVsn.Add(txtV3sn); txtVsn.Add(txtV4sn);

			foreach (var item in chkPOSvisible)
			{
				item.Checked = true;
			}
			int selIndex = 0;
			foreach (var item in cmbSelData)
			{
				item.SelectedIndex = selIndex;
				selIndex = selIndex + 3;
			}

			// Initialize the Jagged Array -- For each of the SIX arrays-pointers, create an array with MaxRecords number of doubles
			for (int x = 0; x < 10; x++) { RawValues[x] = new double[MaxPlotPoints]; RelativeValues[x] = new double[MaxPlotPoints]; }
		}


		//private int Array_Line_Counter = 0;
		int PointsOnPlot = 0;
		//int ArrayPosition = 0;
		void ReadDataAcqFile()
		{
			// TODO: On Error Resume Next Warning!!!: The statement is not translatable
			PointsOnPlot = 0;
			//string DataFileName;
			//string FileName;
			long i;
			string temp_str;
			string t_str = string.Empty;
			int sep_pos;
			int line_read =0;
			int total_lines =0 ;
			bool MaxSizeReached = false;
			double tryParVal = 0;
			if (/*rejected?*/ dlgCommonDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
			{
				// cmdReadPositionFile.Checked = false;
			}

			else
			{
				var last_slash_pos = dlgCommonDialog.FileName.LastIndexOf("\\");
				if (last_slash_pos == 0) last_slash_pos = dlgCommonDialog.FileName.LastIndexOf("/");
				DefaultDirectory = dlgCommonDialog.FileName.Substring(0, last_slash_pos);
				dlgCommonDialog.InitialDirectory = DefaultDirectory;
			   var /*stream*/ stm = new System.IO.StreamReader(dlgCommonDialog.FileName);
				try
				{
					line_read = 0;
					total_lines = 0;
					MaxSizeReached = false;
					File_Reading = true;

					read_again:
					temp_str = stm.ReadLine();
					if (temp_str == "") goto read_again;

					sep_pos = temp_str.IndexOf(",") + 1;
					if (sep_pos == 0) goto read_again;

					if(temp_str.Substring(0, sep_pos-1) != "File name")
					{
						if (double.TryParse(temp_str.Substring(0, 3), out tryParVal))
						{
							line_read = 1;
							clear_DataAcqArrays();
							goto matlab;
						}
						else
						{
							stm.Close();
							File_Reading = false;
							MessageBox.Show("Unrecognizable file format", "Scan File Anomaly", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}

					}

					do
					{
						if (stm.EndOfStream) return;

						temp_str = stm.ReadLine();
						if (MaxSizeReached) return;


						sep_pos = temp_str.IndexOf("Scan Type:") + 1;
						if (sep_pos != 0)
						{
							sep_pos = temp_str.IndexOf("Valve Test") + 1;
							if (sep_pos > 6)
							{
								ChangeMonitoringTestType((int)(TestType.Valves_2020));
								Cmb_X_scale.SelectedIndex = 2;
							}
							else
							{
								Cmb_X_scale.SelectedIndex = 1;
								sep_pos = temp_str.IndexOf("Pressure") + 1;
								if (sep_pos > 6)
									ChangeMonitoringTestType((int)(TestType.Monitor_Pressure));
								else
									ChangeMonitoringTestType((int)(TestType.Monitor_Pos));
							}
						}


						sep_pos = temp_str.IndexOf("command:");
						if (sep_pos != -1)
						{
							sep_pos = temp_str.IndexOf("vtst");
							if (sep_pos > 6)
							{
								ChangeMonitoringTestType((int)(TestType.Valves_2020));
								PC_RealTime = true;
							}
							if (sep_pos > 6)
							{
								t_str = temp_str.Substring(sep_pos + 4);
								temp_str = (t_str + " ").TrimStart();
								sep_pos = temp_str.IndexOf(" ");
								t_str = temp_str.Substring(0, sep_pos);
								if (double.TryParse(t_str, out tryParVal))
									cwValveStartCurrent.Value = tryParVal;

								t_str = temp_str.Substring(sep_pos);
								temp_str = (t_str).TrimStart();
								sep_pos = temp_str.IndexOf(" ");
								t_str = temp_str.Substring(0, sep_pos);
								if (double.TryParse(t_str, out tryParVal))
									cwValveIncrementCurrent.Value = tryParVal;

								t_str = temp_str.Substring(sep_pos);
								temp_str = (t_str).TrimStart();
								sep_pos = temp_str.IndexOf(" ");
								t_str = temp_str.Substring(0, sep_pos);
								if (double.TryParse(t_str, out tryParVal))
									cwValveEndCurrent.Value = tryParVal;

								t_str = temp_str.Substring(sep_pos);
								temp_str = (t_str).TrimStart();
								sep_pos = temp_str.IndexOf(" ") ;
								t_str = temp_str.Substring(0, sep_pos);
								if (double.TryParse(t_str, out tryParVal))
								{
									cwValveMeasTime.Value = tryParVal;
									CMDperiod = tryParVal;
								}

								t_str = temp_str.Substring(sep_pos);
								temp_str = (t_str).TrimStart();
								sep_pos = temp_str.IndexOf(" ");
								t_str = temp_str.Substring(0, sep_pos);
								if (double.TryParse(t_str, out tryParVal))
									cwValveHoldCurrent.Value = tryParVal;

							}
						}

						sep_pos = temp_str.IndexOf(",#") + 1;
						if (sep_pos != 0)
						{
							stm.Close();
							MessageBox.Show("This is Transfer Function File, choose Real Time File");
							return;
						}

						if (MonitoringTestType == (int)TestType.Valves_2020)
							sep_pos = temp_str.IndexOf("lines:") + 1;
						if (sep_pos != 0)
						{
		// read_SN:
							sep_pos = temp_str.IndexOf(",") + 1;
							i = temp_str.Length;
							if (sep_pos != 0)
							{
								temp_str = temp_str.Substring(sep_pos + 1);
								sep_pos = temp_str.IndexOf("r,,");
								t_str = temp_str.Substring(sep_pos + 3);
								temp_str = t_str;
								sep_pos = temp_str.IndexOf(",") + 1;
								if (sep_pos > 1)
									t_str = temp_str.Substring(0, sep_pos - 1);
								if (t_str.Length > 1)
									txtV1sn.Text = t_str.Trim();

								t_str = temp_str.Substring(sep_pos);
								temp_str = t_str;

								sep_pos = temp_str.IndexOf(",") + 1;
								if (sep_pos > 1) t_str = temp_str.Substring(0, sep_pos - 1);
								txtV2sn.Text = t_str.Trim();
								t_str = temp_str.Substring(sep_pos );
								temp_str = t_str;

								sep_pos = temp_str.IndexOf(",") + 1;
								if (sep_pos > 1) t_str = temp_str.Substring(0, sep_pos - 1);
								txtV3sn.Text = t_str.Trim();
								t_str = temp_str.Substring(sep_pos);
								temp_str = t_str;

								sep_pos = temp_str.IndexOf(",") + 1;
								if (sep_pos > 1)
									t_str = temp_str.Substring(0, sep_pos - 1);
								txtV4sn.Text = t_str.Trim();
							}
							else
							{
								txtV1sn.Text = "";
								txtV2sn.Text = "";
								txtV3sn.Text = "";
								txtV4sn.Text = "";
							}
						}

						sep_pos = temp_str.IndexOf("line#") + 1;
						if (sep_pos != 0)
						{
							for (i = 0; i <= 9; i++)
							{
								sep_pos = temp_str.IndexOf(",") + 1;
								if (sep_pos != 0)
								{
									DataHeaderStr[i] = temp_str.Substring(0, sep_pos - 1);
									DataHeaderStr[i] = DataHeaderStr[i].TrimStart();
									temp_str = temp_str.Substring(sep_pos);
								}
								else
								{
									break;
								}
							}
						}
						else
						{
							sep_pos = temp_str.IndexOf("wait tme") + 1;
							if (sep_pos != 0)
							{
								t_str = temp_str.Substring(sep_pos + 1);
								sep_pos = temp_str.IndexOf(",") + 1;
								t_str = t_str.Substring(1, sep_pos - 1);
								if (double.TryParse(t_str, out tryParVal))
								{
									if (tryParVal == 0.01)
									{
										PC_RealTime = false;
										CMDperiod = 0.01;

									}
									else
									{
										PC_RealTime = true;
										cwContCMDperiod.Value = tryParVal;
										CMDperiod = cwContCMDperiod.Value;
									}
								}

							}



							sep_pos = temp_str.IndexOf("request period") + 1;
							if (sep_pos != 0)
							{
								sep_pos = temp_str.IndexOf(",") + 1;
								if (sep_pos != 0)
								{
									t_str = temp_str.Substring(sep_pos + 1);
									sep_pos = t_str.IndexOf(",") + 1;
									if (sep_pos != 0)
										t_str = t_str.Substring(0, sep_pos - 1);
									if (double.TryParse(t_str, out tryParVal))
									{
										if (tryParVal == 0.01)
										{
											PC_RealTime = false;
										}
										else
										{
											PC_RealTime = true;
											cwContCMDperiod.Value = tryParVal;
											CMDperiod = cwContCMDperiod.Value;
										}
									}

								}
							}
						}
						Application.DoEvents();
						if (stm.EndOfStream) { break; }
					}
					while (!(((temp_str.IndexOf("%") + 1) != 0) && ((temp_str.IndexOf("%") + 1) == temp_str.Length) || ((temp_str.IndexOf("End") + 1) != 0) || ((temp_str.IndexOf("*") + 1) != 0)));




					matlab:
					clear_DataAcqArrays();
					while (!stm.EndOfStream)
					{
						if (line_read > MAX_SCAN_ARRAY_SIZE - 1)
							MaxSizeReached = true;
						if (temp_str.Length < 4)
							goto read_data_line;
						if (!MaxSizeReached)
						{
							sep_pos = ExtractDataFromScanLine(temp_str, line_read);
							if (sep_pos >= 0) line_read = line_read + 1;
						}
						total_lines = total_lines + 1;
						if ((line_read > 200) && line_read % 10 == 0)
							lbLineNum.Text = line_read.ToString();

 read_data_line:
						temp_str = stm.ReadLine();
					}
					ScanNumber = line_read;
					lbLineNum.Text = line_read.ToString();
					if (MaxSizeReached)
					{
						DialogResult result = MessageBox.Show("This version only reads " + MAX_SCAN_ARRAY_SIZE + " data record lines." + "\r\n" + "File has " + total_lines + " record lines. The rest of lines is skipped", "Data Size LIMIT", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

					}

					stm.Close();
					File_Reading = false;
					HighlightChannel(ChosenCh, PreviousCh);

					if (MonitoringTestType == (int)TestType.Monitor_Pos)
					{
						XminScale = 0;
						XmaxScale = Convert.ToSingle(ScanTimeStamps[ScanNumber - 1] - ScanTimeStamps[0]);
						slideXAxis.Value = (XmaxScale * 0.25);
						slideXAxis.Caption = "Time, sec";
						TextXunits.Text = "sec";
					}


					if (MonitoringTestType == (int)TestType.Monitor_Pressure)
					{
						XminScale = 0;
						XmaxScale = Convert.ToSingle((ScanNumber - 1) - ScanTimeStamps[0]);
						slideXAxis.Value = (XmaxScale * 0.25);
						slideXAxis.Caption = "Time, sec";
						TextXunits.Text = "sec";
					}


					if (MonitoringTestType == (int)TestType.Valves_2020)
					{
						XminScale = Convert.ToSingle(ScanValues[2, 0]);
						if(ScanNumber > 0) XmaxScale = Convert.ToSingle(ScanValues[2, ScanNumber - 1]);
						if(XminScale > XmaxScale)
						{
							delayTime = XminScale;
							XminScale = XmaxScale;
							XmaxScale = delayTime;
						}

					//    XSliderUpdate(XmaxScale);
				  //      slideXAxis.Value = XminScale + 0.15 * (XmaxScale - XminScale);
						slideXAxis.Caption = "Valve current, mA";
						TextXunits.Text = "mA";
						stateButtonRelAbs.Checked = false;
					}
					line_counter = ScanNumber;

					if(PC_RealTime)
					{
						if (MonitoringTestType == (int)TestType.Valves_2020)
							temp_str = "recorded by Analyzer each " + cwValveMeasTime.Value.ToString() + " sec";
						else
							temp_str = "recorded by Analyzer each " + cwContCMDperiod.Value.ToString() + " sec";
					}
					else
					{
						temp_str = "CONTROLLER recorded data, Analyzer saved";
					}

					temp_str = "Loaded file '" + dlgCommonDialog.SafeFileName + "'; " + (ScanNumber - 1).ToString() + " data lines; " + temp_str;
					txtStatus.Text = temp_str;



				}
				catch (Exception ex)
				{

					stm.Close();
					File_Reading = false;
					MessageBox.Show("The file is NOT a valid scan file:" + "\r" + Tools.TextTidy(ex.ToString()), "Scan File Anomaly", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				finally { stm.Close(); }
			}
		}

		private void HighlightChannel(object Value, object PreviousValue)
		{
			int Y;
			int array_index = 0;
			//if ((Convert.ToInt32(Value)) == 0)
			//    Value = 1;
			//if((Convert.ToInt32(PreviousValue)) == 0)
			//    PreviousValue = 2;

			if (Convert.ToInt32(PreviousValue) > 10)
				PreviousValue = 10;
			cwGraphDataAcq.Plots[Convert.ToInt32(PreviousValue)].LineWidth = 1;
			cwGraphDataAcq.Plots[Convert.ToInt32(Value)].LineWidth = 3;
			chkPOSvisible[(Convert.ToInt32(PreviousValue))].Width = 435;
			chkPOSvisible[(Convert.ToInt32(Value))].Width = 535;

			if (Program.FormMain.Test_in_progress) return;

			cwGraphDataAcq.Cursors[0].Plot = cwGraphDataAcq.Plots[Convert.ToInt32(Value)];

			Update_labels();

			for (array_index = 0; array_index < MAX_SCAN_ARRAY_SIZE; array_index++)
			{
				tmp_array[array_index] = ScanValues[ChosenCh, array_index];
			}
			array_index = Convert.ToInt32(ChosenCh);
			panelStatistics.BackColor = optChooseCh[(int)ChosenCh].BackColor;
			RefreshNumDisplay(tmp_array, 1, MinIndex[array_index], MinValue[array_index], MaxIndex[array_index], MaxValue[array_index], 0, 0, 0, 0, array_index);
		}

		private void RefreshNumDisplay(double[] inArray, int start_index, int MinIndx, double minVal, int MaxIndx, double maxVal, double avrg, double St_Dev_Value, double snr, double max_min, int ch)
		{
			int array_size = MAX_SCAN_ARRAY_SIZE;
			if(MinIndx != -1 && MaxIndx != -1 && minVal != -1 && maxVal != -1) {
				Program.FindMaxMin(inArray, start_index, ref maxVal, ref minVal, ref MaxIndx, ref MinIndx);

				MinIndex[ch] = MinIndx;
				MinValue[ch] = minVal;
				MaxIndex[ch] = MaxIndx;
				MaxValue[ch] = maxVal;

				txtMinSignal_lineNum.Text = MinIndex[ch].ToString();
				txtMinSignal.Text = MinValue[ch].ToString("####0");
				txtMaxSignal_lineNum.Text = MaxIndex[ch].ToString();
				txtMaxSignal.Text = MaxValue[ch].ToString("####0");
			}

			if (avrg != -1) {
				avrg = Program.Avg(inArray, array_size);
				ScanAverage[ch] = avrg;
				measAvg.Text = ScanAverage[ch].ToString("####0.00");
			} else {
				measAvg.Text = " N/A ";
				ScanAverage[ch] = -1;
			}

			if (St_Dev_Value != -1) {
				St_Dev_Value = Program.St_Dev(ref inArray, array_size, false);
				if (St_Dev_Value > 0.01 && St_Dev_Value < 100)
					measError.Text = St_Dev_Value.ToString("0.0000");
				else
					measError.Text = St_Dev_Value.ToString("E2");
				ScanStDev[ch] = St_Dev_Value;
			} else {
				measError.Text = " N/A ";
				ScanStDev[ch] = -1;
			}

			if (St_Dev_Value != -1 && avrg != -1) {
				if (St_Dev_Value != 0) {
					snr = Math.Abs(avrg / St_Dev_Value);
					if (snr > 1 && snr < 10000)
						measSNR.Text = snr.ToString("0.0");
					else
						measSNR.Text = snr.ToString("E2");
					ScanSNR[ch] = snr;
				} else {
					measSNR.Text = "undefined";
					ScanSNR[ch] = -1;
				}
			} else {
				measSNR.Text = " N/A ";
				ScanSNR[ch] = -1;
			}
			if (max_min != -1) {
				txtPk_Pk.Text = (Math.Round(maxVal - minVal, 3)).ToString();
				ScanPk_Pk[ch] = (maxVal - minVal);
			} else {
				txtPk_Pk.Text = "N/A";
				ScanPk_Pk[ch] = -1;
			}
		}




		private void ChangeMonitoringTestType(int test_type)
		{
			MonitoringTestType = test_type;
			cmb_TestType.SelectedIndex = test_type;
			if (MonitoringTestType == Convert.ToInt32(TestType.Monitor_Pos))
				Cmb_X_scale.SelectedIndex = 1;
			if(MonitoringTestType == (int)TestType.Valves_2020)
				Cmb_X_scale.SelectedIndex = 4;
			if (MonitoringTestType == (int)TestType.Monitor_Pressure)
				Cmb_X_scale.SelectedIndex = 1;
		}

		private void PupulateRawValArray()
		{
			for (int x = 0; x < PointsOnPlot; x++)
			{


				for (int j = 0; j < 12; j++)
				{
					string typeOfChannel = cmbSelData[j].SelectedItem.ToString();
					var DataToPlotCH = new double[PointsOnPlot];

					switch (typeOfChannel)
					{
						case "g_RT_Z1":
							//plotIndex = 0;
							//recType = RecordType.g_RT;
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[0, x] * DCfactor;
							break;
						case "g_RT_X1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[1, x] * DCfactor;
							break;
						case "g_RT_Y1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[2, x] * DCfactor;
							break;

						case "gRMS_Z1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[0, x] * DCfactor;
							break;
						case "gRMS_X1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[1, x] * DCfactor;
							break;
						case "gRMS_Y1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[2, x] * DCfactor;
							break;

						case "p_RT_Z1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[0, x] * DCfactor;
							break;
						case "p_RT_X1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[1, x] * DCfactor;
							break;
						case "p_RT_Y1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[2, x] * DCfactor;
							break;

						case "pRMS_Z1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[0, x] * DCfactor;
							break;
						case "pRMS_X1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[1, x] * DCfactor;
							break;
						case "pRMS_Y1":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[2, x] * DCfactor;
							break;


						case "g_RT_Z2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[3, x] * DCfactor;
							break;
						case "g_RT_X2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[4, x] * DCfactor;
							break;
						case "g_RT_Y2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[5, x] * DCfactor;
							break;

						case "gRMS_Z2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[3, x] * DCfactor;
							break;
						case "gRMS_X2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[4, x] * DCfactor;
							break;
						case "gRMS_Y2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[5, x] * DCfactor;
							break;

						case "p_RT_Z2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[3, x] * DCfactor;
							break;
						case "p_RT_X2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[4, x] * DCfactor;
							break;
						case "p_RT_Y2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[5, x] * DCfactor;
							break;

						case "pRMS_Z2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[3, x] * DCfactor;
							break;
						case "pRMS_X2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[4, x] * DCfactor;
							break;
						case "pRMS_Y2":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[5, x] * DCfactor;
							break;


						case "g_RT_Z3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[6, x] * DCfactor;
							break;
						case "g_RT_X3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[7, x] * DCfactor;
							break;
						case "g_RT_Y3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[8, x] * DCfactor;
							break;

						case "gRMS_Z3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[6, x] * DCfactor;
							break;
						case "gRMS_X3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[7, x] * DCfactor;
							break;
						case "gRMS_Y3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[8, x] * DCfactor;
							break;

						case "p_RT_Z3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[6, x] * DCfactor;
							break;
						case "p_RT_X3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[7, x] * DCfactor;
							break;
						case "p_RT_Y3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[8, x] * DCfactor;
							break;

						case "pRMS_Z3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[6, x] * DCfactor;
							break;
						case "pRMS_X3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[7, x] * DCfactor;
							break;
						case "pRMS_Y3":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[8, x] * DCfactor;
							break;

						case "g_RT_Z4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[9, x] * DCfactor;
							break;
						case "g_RT_X4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[10, x] * DCfactor;
							break;
						case "g_RT_Y4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[11, x] * DCfactor;
							break;

						case "gRMS_Z4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[9, x] * DCfactor;
							break;
						case "gRMS_X4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[10, x] * DCfactor;
							break;
						case "gRMS_Y4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[11, x] * DCfactor;
							break;

						case "p_RT_Z4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[9, x] * DCfactor;
							break;
						case "p_RT_X4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[10, x] * DCfactor;
							break;
						case "p_RT_Y4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[11, x] * DCfactor;
							break;

						case "pRMS_Z4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[9, x] * DCfactor;
							break;
						case "pRMS_X4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[10, x] * DCfactor;
							break;
						case "pRMS_Y4":
							RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[11, x] * DCfactor;
							break;
					}
				}


			}
		}

		//=== METHODS TO MANIPULATE DATA VALUES
		//*** CONVERT [RawValues] INTO [RelativeValues] BY APPLYING RELATIVE & SCALING OPTIONS
		private void Convert_ALL_RawValues_to_RelativeValues()
		{
			// Debug.WriteLine("ENTERED Convert_All_RawValues_to_RelativeValues()");
			if (FormLoading == true) return;
			// GARY-FIX: How long will this take for 100,000 points of data??
			// For EACH data-point we have...call Convert_ONE... to convert ALL channelsBase of that Data-Point into the RelativeValues array.
			for (int DataPoint = 0; DataPoint <= PointsOnPlot; DataPoint++)
			{
				Convert_ONE_RawValue_To_RelativeValue(DataPoint);
			}
		}

		private void Convert_ONE_RawValue_To_RelativeValue(int RN)
		{
			// rn = Record Number
			// RawValues is a two-dimentional (jagged) array that holds the RAW (actual) data that came in from the controller.
			// Here we take that data and copy it to the RelativeValues array. As we do, we make it into RELATIVE data by...
			//
			//  First:  Subtracting the FIRST data value from all other data-values (including the first one)
			//          This essentially "zero's out" the data so that it is relative to some fixed starting point.
			//
			//  Second: Add a value (specified by the user) to each of the channelsBase (except the first one)
			//          This separates the plots so they dont' overlap on the graph.
			// MAKE ALL DATA RELATIVE TO FIRST DATA POINT...&...SEPARATE THE DATA BY ADDING A VALUE TO EACH PLOT.
			RelativeValues[0][RN] = RawValues[0][RN] - RawValues[0][0];
			RelativeValues[1][RN] = RawValues[1][RN] - RawValues[1][0] + (PlotDataSpreadRelativePlotsBy * 1);
			RelativeValues[2][RN] = RawValues[2][RN] - RawValues[2][0] + (PlotDataSpreadRelativePlotsBy * 2);
			RelativeValues[3][RN] = RawValues[3][RN] - RawValues[3][0] + (PlotDataSpreadRelativePlotsBy * 3);
			RelativeValues[4][RN] = RawValues[4][RN] - RawValues[4][0] + (PlotDataSpreadRelativePlotsBy * 4);
			RelativeValues[5][RN] = RawValues[5][RN] - RawValues[5][0] + (PlotDataSpreadRelativePlotsBy * 5);
			RelativeValues[6][RN] = RawValues[6][RN] - RawValues[6][0] + (PlotDataSpreadRelativePlotsBy * 6);
			RelativeValues[7][RN] = RawValues[7][RN] - RawValues[7][0] + (PlotDataSpreadRelativePlotsBy * 7);
			RelativeValues[8][RN] = RawValues[8][RN] - RawValues[8][0] + (PlotDataSpreadRelativePlotsBy * 8);
			RelativeValues[9][RN] = RawValues[9][RN] - RawValues[9][0] + (PlotDataSpreadRelativePlotsBy * 9);
			//RelativeValues[10][RN] = RawValues[10][RN] - RawValues[10][0] + (PlotDataSpreadRelativePlotsBy * 10);
			//RelativeValues[11][RN] = RawValues[11][RN] - RawValues[11][0] + (PlotDataSpreadRelativePlotsBy * 11);


			return;
		}

		private void graph_SpreadPlotsBy_TextChanged(object sender, EventArgs e) { ProcessChangeToRelativeValueAmount(); }

		double PlotDataSpreadRelativePlotsBy = 0;

		private void ProcessChangeToRelativeValueAmount()
		{
			double SpreadPlotsBy;
			SpreadPlotsBy = Convert.ToDouble(cwSpreadPlotsByY.Text);
			if (SpreadPlotsBy > 5000) SpreadPlotsBy = 5000;
			if (SpreadPlotsBy < 0) SpreadPlotsBy = 0;
			PlotDataSpreadRelativePlotsBy = SpreadPlotsBy;
			Convert_ALL_RawValues_to_RelativeValues();
			// SetActualTRUEorRelativeFALSEplotting(false);  // This will also call PlotAllPoints
			//PlotAllPoints();
		}

		////=== PUT DATA ON FORM
		////*** PLOT THE ENTIRE RAW-VALUES or RELATIVE-VALUES ARRAY
		//private void ReplotAndRezoomAllPoints()
		//{
		//    if (Zoomed == false)
		//    {
		//        PlotAllPoints();
		//        Debug.WriteLine("We got into ReplotAndRezoomAllPoints but Zoomed=False. Shouldn't happen.");
		//        return;
		//    }

		//    // Save Current Zoom Parameters
		//    SaveZoomedRange();

		//    // Plot All Points will just return immediately if ShowNewDataPoints is true...so fake it out.
		//    Boolean ShowNewDataPoints_PreviousValue = ShowNewDataPoints;
		//    ShowNewDataPoints = true;
		//    PlotAllPoints();
		//    ShowNewDataPoints = ShowNewDataPoints_PreviousValue;

		//    // Restore the ZOOM to what is was before we plotted-all-points
		//    RestoreZoomedXRange();
		//}
		bool ShowNewDataPoints = true;

		private void PlotAllPoints(double[,] ScanValuesToPlot)
		{
			if (FormLoading == true) return;

			HidePlotAreaImage();
			PointsOnPlot = ScanValuesToPlot.GetLength(1);
			// If we are ZOOMED-IN on a portion of the graph that is NOT at the end (showing the newest data-points)
			// then ShowNewDataPoints will be false and we should just EXIT this procedure without doing anything.
			//  if (ShowNewDataPoints == false) return;

			// Some actions a user takes when the plot is still blank can cause a call to PlotAllPoints. Don't shake the tree if the fruit ain't ripe.
			//  if (PointsOnPlot < 1) return;

			// GARY-DEBUG: It can be helpful to play a beep whenever we re-plot so we know how often (when) it is occuring
			// SystemSounds.Beep.Play();
			// Clear off the waveformGraph Plot
			if(cwGraphDataAcq.InvokeRequired) cwGraphDataAcq.BeginInvoke((MethodInvoker)delegate { cwGraphDataAcq.ClearData(); }); else cwGraphDataAcq.ClearData();

			// Since we are putting new data on the plot...we need to increase the X-Axis Range to show the new data-points
			// BUT...don't shrink the graph below 15 data-points, otherwise it just looks strange.
			NationalInstruments.UI.Range NewRange;
			//if (PointsOnPlot <= 15)
			//{
			//    NewRange = new NationalInstruments.UI.Range(0, 15);
			//    cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
			//}
			//else
			//{
			//if(XminScale == XmaxScale)
			NewRange = new NationalInstruments.UI.Range(0, PointsOnPlot);
			if (cwGraphDataAcq.InvokeRequired)  cwGraphDataAcq.BeginInvoke((MethodInvoker)delegate { cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose; });
			else cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			//}
			if (cwGraphDataAcq.InvokeRequired)  cwGraphDataAcq.BeginInvoke((MethodInvoker)delegate { cwGraphDataAcq.XAxes[0].Range = NewRange; });
			else cwGraphDataAcq.XAxes[0].Range = NewRange;

			// Since we are putting new data on the plot...we want the Y-Axes to Auto-Adjust to the data values.
			if (cwGraphDataAcq.InvokeRequired) cwGraphDataAcq.BeginInvoke((MethodInvoker)delegate
			{
				cwGraphDataAcq.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			});
			else cwGraphDataAcq.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			// cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			// buttonXautoScale.Checked = true;
			// SetXaxisScaleAutoOrManual();

			// Plot the data onto the Graph
			// Create arrays of the exact-appropriate size taht will hold the data-samples
			var DataToPlotCH1 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH2 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH3 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH4 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH5 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH6 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH7 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH8 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH9 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH10 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
																   //var DataToPlotCH11 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
																   //var DataToPlotCH12 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??

			// Copy the data from the persistent DataSamplesCH array to the DataToPlot arrays
			//  if (PlotDataRelativeValues == false)
			for (int x = 0; x < PointsOnPlot; x++)
			{
				DataToPlotCH1[x] = ScanValuesToPlot[0, x] * DCfactor;
				DataToPlotCH2[x] = ScanValuesToPlot[1, x] * DCfactor;
				DataToPlotCH3[x] = ScanValuesToPlot[2, x] * DCfactor;
				DataToPlotCH4[x] = ScanValuesToPlot[3, x] * ACfactor;
				DataToPlotCH5[x] = ScanValuesToPlot[4, x] * ACfactor;
				DataToPlotCH6[x] = ScanValuesToPlot[5, x] * ACfactor;
				DataToPlotCH7[x] = ScanValuesToPlot[6, x] * ACfactor;
				DataToPlotCH8[x] = ScanValuesToPlot[7, x] * ACfactor;
				DataToPlotCH9[x] = ScanValuesToPlot[8, x] * ACfactor;
				DataToPlotCH10[x] = ScanValuesToPlot[9, x] * ACfactor;
				//DataToPlotCH11[x] = ScanValuesToPlot[10,x] * ACfactor;
				//DataToPlotCH12[x] = ScanValuesToPlot[11,x] * ACfactor;

			}
			// if (PlotDataRelativeValues == false)
			//  else
			//for (int x = 0; x < PointsOnPlot; x++)
			//{
			//DataToPlotCH1[x] = RawValues[0][x] * DCfactor;
			//DataToPlotCH2[x] = RawValues[1][x] * DCfactor;
			//DataToPlotCH3[x] = RawValues[2][x] * DCfactor;
			//DataToPlotCH4[x] = RawValues[3][x] * ACfactor;
			//DataToPlotCH5[x] = RawValues[4][x] * ACfactor;
			//DataToPlotCH6[x] = RawValues[5][x] * ACfactor;
			//DataToPlotCH7[x] = RawValues[6][x] * ACfactor;
			//DataToPlotCH8[x] = RawValues[7][x] * ACfactor;
			//DataToPlotCH9[x] = RawValues[8][x] * ACfactor;
			//DataToPlotCH10[x] = RawValues[9][x] * ACfactor;
			//DataToPlotCH11[x] = RawValues[10][x] * ACfactor;
			//DataToPlotCH12[x] = RawValues[11][x] * ACfactor;

			//RawValues[0][x] = DataToPlotCH1[x] = g_RT_ScanValues[0,x] * DCfactor;
			//RawValues[1][x] = DataToPlotCH2[x] = g_RT_ScanValues[1, x] * DCfactor;
			//RawValues[2][x] = DataToPlotCH3[x] = g_RT_ScanValues[2, x] * DCfactor;
			//RawValues[3][x] = DataToPlotCH4[x] = g_RT_ScanValues[3, x] * ACfactor;
			//RawValues[4][x] = DataToPlotCH5[x] = g_RT_ScanValues[4, x] * ACfactor;
			//RawValues[5][x] = DataToPlotCH6[x] = g_RT_ScanValues[5, x] * ACfactor;
			//RawValues[6][x] = DataToPlotCH7[x] = g_RT_ScanValues[6, x] * ACfactor;
			//RawValues[7][x] = DataToPlotCH8[x] = g_RT_ScanValues[7, x] * ACfactor;
			//RawValues[8][x] = DataToPlotCH9[x] = g_RT_ScanValues[8, x] * ACfactor;
			//RawValues[9][x] = DataToPlotCH10[x] = g_RT_ScanValues[9, x] * ACfactor;
			//RawValues[10][x] = DataToPlotCH11[x] = g_RT_ScanValues[10, x] * ACfactor;
			//RawValues[11][x] = DataToPlotCH12[x] = g_RT_ScanValues[11, x] * ACfactor;

			//for (int j = 0; j < 12; j++)
			//{
			//    string typeOfChannel = cmbSelData[j].SelectedItem.ToString();
			//    var DataToPlotCH = new double[PointsOnPlot];
			//    //switch (j)
			//    //{
			//    //    case 0:
			//    //        DataToPlotCH = DataToPlotCH1;
			//    //        break;
			//    //    case 1:
			//    //        DataToPlotCH = DataToPlotCH2;
			//    //        break;
			//    //    case 2:
			//    //        DataToPlotCH = DataToPlotCH3;
			//    //        break;
			//    //    case 3:
			//    //        DataToPlotCH = DataToPlotCH4;
			//    //        break;
			//    //    case 4:
			//    //        DataToPlotCH = DataToPlotCH5;
			//    //        break;
			//    //    case 5:
			//    //        DataToPlotCH = DataToPlotCH6;
			//    //        break;
			//    //    case 6:
			//    //        DataToPlotCH = DataToPlotCH7;
			//    //        break;
			//    //    case 7:
			//    //        DataToPlotCH = DataToPlotCH8;
			//    //        break;
			//    //    case 8:
			//    //        DataToPlotCH = DataToPlotCH9;
			//    //        break;
			//    //    case 9:
			//    //        DataToPlotCH = DataToPlotCH10;
			//    //        break;
			//    //    case 10:
			//    //        DataToPlotCH = DataToPlotCH11;
			//    //        break;
			//    //    case 11:
			//    //        DataToPlotCH = DataToPlotCH12;
			//    //        break;
			//    //    default:
			//    //        break;
			//    //}
			//    switch (typeOfChannel)
			//    {
			//        case "g_RT_Z1":
			//            //plotIndex = 0;
			//            //recType = RecordType.g_RT;
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[0, x] * DCfactor;
			//            break;
			//        case "g_RT_X1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[1, x] * DCfactor;
			//            break;
			//        case "g_RT_Y1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[2, x] * DCfactor;
			//            break;

			//        case "gRMS_Z1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[0, x] * DCfactor;
			//            break;
			//        case "gRMS_X1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[1, x] * DCfactor;
			//            break;
			//        case "gRMS_Y1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[2, x] * DCfactor;
			//            break;

			//        case "p_RT_Z1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[0, x] * DCfactor;
			//            break;
			//        case "p_RT_X1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[1, x] * DCfactor;
			//            break;
			//        case "p_RT_Y1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[2, x] * DCfactor;
			//            break;

			//        case "pRMS_Z1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[0, x] * DCfactor;
			//            break;
			//        case "pRMS_X1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[1, x] * DCfactor;
			//            break;
			//        case "pRMS_Y1":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[2, x] * DCfactor;
			//            break;


			//        case "g_RT_Z2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[3, x] * DCfactor;
			//            break;
			//        case "g_RT_X2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[4, x] * DCfactor;
			//            break;
			//        case "g_RT_Y2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[5, x] * DCfactor;
			//            break;

			//        case "gRMS_Z2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[3, x] * DCfactor;
			//            break;
			//        case "gRMS_X2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[4, x] * DCfactor;
			//            break;
			//        case "gRMS_Y2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[5, x] * DCfactor;
			//            break;

			//        case "p_RT_Z2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[3, x] * DCfactor;
			//            break;
			//        case "p_RT_X2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[4, x] * DCfactor;
			//            break;
			//        case "p_RT_Y2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[5, x] * DCfactor;
			//            break;

			//        case "pRMS_Z2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[3, x] * DCfactor;
			//            break;
			//        case "pRMS_X2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[4, x] * DCfactor;
			//            break;
			//        case "pRMS_Y2":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[5, x] * DCfactor;
			//            break;


			//        case "g_RT_Z3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[6, x] * DCfactor;
			//            break;
			//        case "g_RT_X3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[7, x] * DCfactor;
			//            break;
			//        case "g_RT_Y3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[8, x] * DCfactor;
			//            break;

			//        case "gRMS_Z3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[6, x] * DCfactor;
			//            break;
			//        case "gRMS_X3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[7, x] * DCfactor;
			//            break;
			//        case "gRMS_Y3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[8, x] * DCfactor;
			//            break;

			//        case "p_RT_Z3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[6, x] * DCfactor;
			//            break;
			//        case "p_RT_X3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[7, x] * DCfactor;
			//            break;
			//        case "p_RT_Y3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[8, x] * DCfactor;
			//            break;

			//        case "pRMS_Z3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[6, x] * DCfactor;
			//            break;
			//        case "pRMS_X3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[7, x] * DCfactor;
			//            break;
			//        case "pRMS_Y3":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[8, x] * DCfactor;
			//            break;

			//        case "g_RT_Z4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[9, x] * DCfactor;
			//            break;
			//        case "g_RT_X4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[10, x] * DCfactor;
			//            break;
			//        case "g_RT_Y4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = g_RT_ScanValues[11, x] * DCfactor;
			//            break;

			//        case "gRMS_Z4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[9, x] * DCfactor;
			//            break;
			//        case "gRMS_X4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[10, x] * DCfactor;
			//            break;
			//        case "gRMS_Y4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = gRMS_ScanValues[11, x] * DCfactor;
			//            break;

			//        case "p_RT_Z4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[9, x] * DCfactor;
			//            break;
			//        case "p_RT_X4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[10, x] * DCfactor;
			//            break;
			//        case "p_RT_Y4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = p_RT_ScanValues[11, x] * DCfactor;
			//            break;

			//        case "pRMS_Z4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[9, x] * DCfactor;
			//            break;
			//        case "pRMS_X4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[10, x] * DCfactor;
			//            break;
			//        case "pRMS_Y4":
			//            RawValues[j][x] /*= DataToPlotCH[x]*/  = pRMS_ScanValues[11, x] * DCfactor;
			//            break;
			//    }
			//}

			// DataToPlotCH1[x] = RawValues[0][x];
			//    DataToPlotCH2[x] = RawValues[1][x];
			//    DataToPlotCH3[x] = RawValues[2][x];
			//    DataToPlotCH4[x] = RawValues[3][x];
			//    DataToPlotCH5[x] = RawValues[4][x];
			//    DataToPlotCH6[x] = RawValues[5][x];
			//    DataToPlotCH7[x] = RawValues[6][x];
			//    DataToPlotCH8[x] = RawValues[7][x];
			//    DataToPlotCH9[x] = RawValues[8][x];
			//    DataToPlotCH10[x] = RawValues[9][x];
			//    DataToPlotCH11[x] = RawValues[10][x];
			//    DataToPlotCH12[x] = RawValues[11][x];
			//    //RawValues[1][x] = DataToPlotCH2[x] = g_RT_ScanValues[1, x] * DCfactor;
			//    //RawValues[2][x] = DataToPlotCH3[x] = g_RT_ScanValues[2, x] * DCfactor;
			//    //RawValues[3][x] = DataToPlotCH4[x] = g_RT_ScanValues[3, x] * ACfactor;
			//    //RawValues[4][x] = DataToPlotCH5[x] = g_RT_ScanValues[4, x] * ACfactor;
			//    //RawValues[5][x] = DataToPlotCH6[x] = g_RT_ScanValues[5, x] * ACfactor;
			//    //RawValues[6][x] = DataToPlotCH7[x] = g_RT_ScanValues[6, x] * ACfactor;
			//    //RawValues[7][x] = DataToPlotCH8[x] = g_RT_ScanValues[7, x] * ACfactor;
			//    //RawValues[8][x] = DataToPlotCH9[x] = g_RT_ScanValues[8, x] * ACfactor;
			//    //RawValues[9][x] = DataToPlotCH10[x] = g_RT_ScanValues[9, x] * ACfactor;
			//    //RawValues[10][x] = DataToPlotCH11[x] = g_RT_ScanValues[10, x] * ACfactor;
			//    //RawValues[11][x] = DataToPlotCH12[x] = g_RT_ScanValues[11, x] * ACfactor;
			//}

		  //  Create a "WaveForms" array from our local arrays
		   var waveforms = new AnalogWaveform<double>[10];
			waveforms[0] = AnalogWaveform<double>.FromArray1D(DataToPlotCH1);
			waveforms[1] = AnalogWaveform<double>.FromArray1D(DataToPlotCH2);
			waveforms[2] = AnalogWaveform<double>.FromArray1D(DataToPlotCH3);
			waveforms[3] = AnalogWaveform<double>.FromArray1D(DataToPlotCH4);
			waveforms[4] = AnalogWaveform<double>.FromArray1D(DataToPlotCH5);
			waveforms[5] = AnalogWaveform<double>.FromArray1D(DataToPlotCH6);
			waveforms[6] = AnalogWaveform<double>.FromArray1D(DataToPlotCH7);
			waveforms[7] = AnalogWaveform<double>.FromArray1D(DataToPlotCH8);
			waveforms[8] = AnalogWaveform<double>.FromArray1D(DataToPlotCH9);
			waveforms[9] = AnalogWaveform<double>.FromArray1D(DataToPlotCH10);
			//waveforms[10] = AnalogWaveform<double>.FromArray1D(DataToPlotCH11);
			//waveforms[11] = AnalogWaveform<double>.FromArray1D(DataToPlotCH12);

			//V1.PlotY(DataToPlotCH1);
			//V2.PlotY(DataToPlotCH2);
			//V3.PlotY(DataToPlotCH3);
			//H1.PlotY(DataToPlotCH4);
			//H2.PlotY(DataToPlotCH5);
			//H3.PlotY(DataToPlotCH6);
			//FF_Xpos.PlotY(DataToPlotCH7);
			//FF_Ypos.PlotY(DataToPlotCH8);
			//FF_Xacc.PlotY(DataToPlotCH9);
			//FF_Yacc.PlotY(DataToPlotCH10);

		   // Plot the data onto the Graph      PLOTS - HERE:
			 this.BeginInvoke((MethodInvoker)delegate { cwGraphDataAcq.PlotWaveforms(waveforms); });

			// Anytime we plot the points, update the number of points
			//   this.Invoke((System.Action)(() => { textBoxRecordsOnPlot.Text = PointsOnPlot.ToString(); }));

			// Update the Get-Once text boxes with the data from the last sample on the plot
			//            UpdateGetOnceTextBoxes(PointsOnPlot - 1);    // GARY-FIX: Is this new line working out okay? (Below)
			//  UpdateGetOnceTextBoxes(ArrayPosition);

			// Set the X-Axis Slider Range to match how many points are on the graph
			// XSliderUpdate();
			cwGraphDataAcq.BeginInvoke((MethodInvoker)delegate { cwGraphDataAcq.Update(); });
		}

		//*** UPDATES THE POSITION (LOOK) OF THE X-AXIS SLIDER
		private void XSliderUpdate(double max)
		{
			// This was originally called by a timer that fired every 2/10th second.
			// Now it is called from/when:
			//  + After every plot point is plotted (whether we are zoomed out to see it or not)
			//  + After every change in Zoom / Zoom-In / Zoom-Out / PAN
			//
			if (Zoomed == false)
			{
				// When caption is shown...the slider is NOT shown.
				slideXAxis.Caption = "Viewing Entire Plot";
				return;
			}

			double RangeMin = cwGraphDataAcq.XAxes[0].Range.Minimum;
			double RangeMax = cwGraphDataAcq.XAxes[0].Range.Maximum;
			double RangeAve = (RangeMin + RangeMax) / 2;

			// GARY-FIX: // GARY-ERROR:  IF THEY DRAG/PAN THE RANGE TOO FAR OUT OF WACK IT ERRORS HERE
			// I think this (below) fixes it.
			if (RangeAve < 0) RangeAve = 0;
			if (RangeAve > PointsOnPlot) RangeAve = PointsOnPlot;
			NationalInstruments.UI.Range r = new NationalInstruments.UI.Range(0, max);
			slideXAxis.Range = r;
			slideXAxis.Value = RangeAve;
			slideXAxis.Caption = "";

		}


		double DCfactor = 1;
		double ACfactor = 1;

		//*** SET VALUES THAT WILL BE USED TO ADJUST THE Y-AXIS (LEFT & RIGHT) SCALES
		private void FigureACandDCfactors()
		{
			/* if (PlotDataScaledToMilliGauss == true) DCfactor = 0.01; else */
			DCfactor = 1;
			/* if (PlotDataACinRMS == true) ACfactor = DCfactor / 3; else*/
			ACfactor = DCfactor;
		}




		private void panel5_Paint(object sender, PaintEventArgs e)
		{

		}

		private void cmdReadPositionFile_Click(object sender, EventArgs e)
		{

			ReadDataAcqFile();
			ScanValuesToPlot = new double[0, 0];
			UpdatePositionPlot(-1);
			if(MonitoringTestType == Convert.ToInt32(TestType.Valves_2020))
			{
				Analyze_Valve_plot();
			}
		}

		private void stateButtonRelAbs_CheckedChanged(object sender, EventArgs e)
		{

			PlotDataRelativeValues = stateButtonRelAbs.Checked;
			if (MonitoringTestType == Convert.ToInt32(TestType.Valves_2020))
				PlotDataRelativeValues = false;

			if (!PlotDataRelativeValues)
			{
				cwSpreadPlotsByY.Visible = false;
				cwSpreadPlotsByY.Enabled = false;
				labelSpreadPlotsBy.Enabled = false;
				labelSpreadPlotsBy.Visible = false;
			}
			else
			{
				Convert_ALL_RawValues_to_RelativeValues();
				cwSpreadPlotsByY.Visible = true;
				cwSpreadPlotsByY.Enabled = true;
				labelSpreadPlotsBy.Enabled = true;
				labelSpreadPlotsBy.Visible = true;
			}

		 //   ProcessChangeToRelativeValueAmount();
			UpdatePositionPlot(-1);
		}



		int PlotSelected;
		private void optChooseCh_CheckedChanged(object sender, EventArgs e)
		{
			//reset signal buttons to unselected
			foreach (var signal in optChooseCh)
			{
				signal.FlatStyle = FlatStyle.Standard;
				signal.Size = new Size(80, 22);
			}
			if (((RadioButton)sender).Checked)
			{
				((RadioButton)sender).FlatStyle = FlatStyle.Popup;
				((RadioButton)sender).Size = new Size(90, 22);
				int Index = optChooseCh.IndexOf((RadioButton)sender);
				ChosenCh = Index;
				if (PreviousCh < 0) PreviousCh = 0;
				HighlightChannel(ChosenCh, PreviousCh);
				PreviousCh = ChosenCh;
			}

		}

		private void chkPOSvisible11_CheckedChanged(object sender, EventArgs e)
		{
			if (((CheckBox)sender).Checked)
			{
				cwGraphDataAcq.Plots[chkPOSvisible.IndexOf((CheckBox)sender)].Visible = true;
			}
			else
				cwGraphDataAcq.Plots[chkPOSvisible.IndexOf((CheckBox)sender)].Visible = false;
		}

		private void cmbSelData0_SelectedIndexChanged(object sender, EventArgs e)
		{
			PlotIndividualPoints(((ComboBox)sender));
			//switch (((ComboBox)sender).SelectedItem.ToString())
			//{
			//    case "g_RT_Z1":
			//        break;
			//    case "g_RT_X1":
			//        break;
			//    case "g_RT_Y1":
			//        break;

			//    case "gRMS_Z1":
			//        break;
			//    case "gRMS_X1":
			//        break;
			//    case "gRMS_Y1":
			//        break;

			//    case "p_RT_Z1":
			//        break;
			//    case "p_RT_X1":
			//        break;
			//    case "p_RT_Y1":
			//        break;

			//    case "pRMS_Z1":
			//        break;
			//    case "pRMS_X1":
			//        break;
			//    case "pRMS_Y1":
			//        break;


			//    case "g_RT_Z2":
			//        break;
			//    case "g_RT_X2":
			//        break;
			//    case "g_RT_Y2":
			//        break;

			//    case "gRMS_Z2":
			//        break;
			//    case "gRMS_X2":
			//        break;
			//    case "gRMS_Y2":
			//        break;

			//    case "p_RT_Z2":
			//        break;
			//    case "p_RT_X2":
			//        break;
			//    case "p_RT_Y2":
			//        break;

			//    case "pRMS_Z2":
			//        break;
			//    case "pRMS_X2":
			//        break;
			//    case "pRMS_Y2":
			//        break;


			//    case "g_RT_Z3":
			//        break;
			//    case "g_RT_X3":
			//        break;
			//    case "g_RT_Y3":
			//        break;

			//    case "gRMS_Z3":
			//        break;
			//    case "gRMS_X3":
			//        break;
			//    case "gRMS_Y3":
			//        break;

			//    case "p_RT_Z3":
			//        break;
			//    case "p_RT_X3":
			//        break;
			//    case "p_RT_Y3":
			//        break;

			//    case "pRMS_Z3":
			//        break;
			//    case "pRMS_X3":
			//        break;
			//    case "pRMS_Y3":
			//        break;

			//    case "g_RT_Z4":
			//        break;
			//    case "g_RT_X4":
			//        break;
			//    case "g_RT_Y4":
			//        break;

			//    case "gRMS_Z4":
			//        break;
			//    case "gRMS_X4":
			//        break;
			//    case "gRMS_Y4":
			//        break;

			//    case "p_RT_Z4":
			//        break;
			//    case "p_RT_X4":
			//        break;
			//    case "p_RT_Y4":
			//        break;

			//    case "pRMS_Z4":
			//        break;
			//    case "pRMS_X4":
			//        break;
			//    case "pRMS_Y4":
			//        break;
			//}



		}
		enum RecordType
		{
			g_RT,
			gRMS,
			p_RT,
			pRMS
		}

		private void PlotIndividualPoints(ComboBox cmb)
		{

			if (FormLoading == true) return;

			string typeOfChannel = cmb.SelectedItem.ToString();
			// If we are ZOOMED-IN on a portion of the graph that is NOT at the end (showing the newest data-points)
			// then ShowNewDataPoints will be false and we should just EXIT this procedure without doing anything.
			if (ShowNewDataPoints == false) return;

			// Some actions a user takes when the plot is still blank can cause a call to PlotAllPoints. Don't shake the tree if the fruit ain't ripe.
			if (PointsOnPlot < 1) return;

			// GARY-DEBUG: It can be helpful to play a beep whenever we re-plot so we know how often (when) it is occuring
			// SystemSounds.Beep.Play();
			// Clear off the waveformGraph Plot
			int plotIndex = -1;
			RecordType recType = RecordType.g_RT;
			switch (typeOfChannel)
			{
				case "g_RT_Z1":
					plotIndex = 0;
					recType = RecordType.g_RT;
					break;
				case "g_RT_X1":
					plotIndex = 1;
					recType = RecordType.g_RT;
					break;
				case "g_RT_Y1":
					plotIndex = 2;
					recType = RecordType.g_RT;
					break;

				case "gRMS_Z1":
					plotIndex = 0;
					recType = RecordType.gRMS;
					break;
				case "gRMS_X1":
					plotIndex = 1;
					recType = RecordType.gRMS;
					break;
				case "gRMS_Y1":
					plotIndex = 2;
					recType = RecordType.gRMS;
					break;

				case "p_RT_Z1":
					plotIndex = 0;
					recType = RecordType.p_RT;
					break;
				case "p_RT_X1":
					plotIndex = 1;
					recType = RecordType.p_RT;
					break;
				case "p_RT_Y1":
					plotIndex = 2;
					recType = RecordType.p_RT;
					break;

				case "pRMS_Z1":
					recType = RecordType.pRMS;
					plotIndex = 0;
					break;
				case "pRMS_X1":
					recType = RecordType.pRMS;
					plotIndex = 1;
					break;
				case "pRMS_Y1":
					recType = RecordType.pRMS;
					plotIndex = 2;
					break;


				case "g_RT_Z2":
					plotIndex = 3;
					recType = RecordType.g_RT;
					break;
				case "g_RT_X2":
					plotIndex = 4;
					recType = RecordType.g_RT;
					break;
				case "g_RT_Y2":
					plotIndex = 5;
					recType = RecordType.g_RT;
					break;

				case "gRMS_Z2":
					plotIndex = 3;
					recType = RecordType.gRMS;
					break;
				case "gRMS_X2":
					plotIndex = 4;
					recType = RecordType.gRMS;
					break;
				case "gRMS_Y2":
					plotIndex = 5;
					recType = RecordType.gRMS;
					break;

				case "p_RT_Z2":
					plotIndex = 3;
					recType = RecordType.p_RT;
					break;
				case "p_RT_X2":
					plotIndex = 4;
					recType = RecordType.p_RT;
					break;
				case "p_RT_Y2":
					plotIndex = 5;
					recType = RecordType.p_RT;
					break;

				case "pRMS_Z2":
					plotIndex = 3;
					recType = RecordType.pRMS;
					break;
				case "pRMS_X2":
					plotIndex = 4;
					recType = RecordType.pRMS;
					break;
				case "pRMS_Y2":
					plotIndex = 5;
					recType = RecordType.pRMS;
					break;


				case "g_RT_Z3":
					plotIndex = 6;
					recType = RecordType.g_RT;
					break;
				case "g_RT_X3":
					plotIndex = 7;
					recType = RecordType.g_RT;
					break;
				case "g_RT_Y3":
					plotIndex = 8;
					recType = RecordType.g_RT;
					break;

				case "gRMS_Z3":
					plotIndex = 6;
					recType = RecordType.gRMS;
					break;
				case "gRMS_X3":
					plotIndex = 7;
					recType = RecordType.gRMS;
					break;
				case "gRMS_Y3":
					plotIndex = 8;
					recType = RecordType.gRMS;
					break;

				case "p_RT_Z3":
					plotIndex = 6;
					recType = RecordType.p_RT;
					break;
				case "p_RT_X3":
					plotIndex = 7;
					recType = RecordType.p_RT;
					break;
				case "p_RT_Y3":
					plotIndex = 8;
					recType = RecordType.p_RT;
					break;

				case "pRMS_Z3":
					plotIndex = 6;
					recType = RecordType.pRMS;
					break;
				case "pRMS_X3":
					plotIndex = 7;
					recType = RecordType.pRMS;
					break;
				case "pRMS_Y3":
					recType = RecordType.pRMS;
					plotIndex = 8;
					break;

				case "g_RT_Z4":
					plotIndex = 9;
					recType = RecordType.g_RT;
					break;
				case "g_RT_X4":
					plotIndex = 10;
					recType = RecordType.g_RT;
					break;
				case "g_RT_Y4":
					plotIndex = 11;
					recType = RecordType.g_RT;
					break;

				case "gRMS_Z4":
					plotIndex = 9;
					recType = RecordType.gRMS;
					break;
				case "gRMS_X4":
					plotIndex = 10;
					recType = RecordType.gRMS;
					break;
				case "gRMS_Y4":
					plotIndex = 11;
					recType = RecordType.gRMS;
					break;

				case "p_RT_Z4":
					plotIndex = 9;
					recType = RecordType.p_RT;
					break;
				case "p_RT_X4":
					plotIndex = 10;
					recType = RecordType.p_RT;
					break;
				case "p_RT_Y4":
					plotIndex = 11;
					recType = RecordType.p_RT;
					break;

				case "pRMS_Z4":
					plotIndex = 9;
					recType = RecordType.pRMS;
					break;
				case "pRMS_X4":
					plotIndex = 10;
					recType = RecordType.pRMS;
					break;
				case "pRMS_Y4":
					recType = RecordType.pRMS;
					plotIndex = 11;
					break;
			}


			int cmbIndex = cmbSelData.IndexOf(cmb);

			cwGraphDataAcq.Plots[cmbIndex].ClearData();
			// int PointsOnPlotIndividual = 0;
			//switch (recType)
			//{
			//    case RecordType.g_RT:
			//        PointsOnPlotIndividual = g_RT_ScanValues.GetLength(1);
			//        break;
			//    case RecordType.gRMS:
			//        PointsOnPlotIndividual = gRMS_ScanValues.GetLength(1);
			//        break;
			//    case RecordType.p_RT:
			//        PointsOnPlotIndividual = p_RT_ScanValues.GetLength(1);
			//        break;
			//    case RecordType.pRMS:
			//        PointsOnPlotIndividual = pRMS_ScanValues.GetLength(1);
			//        break;
			//    default:
			//        break;
			//}
			// Since we are putting new data on the plot...we need to increase the X-Axis Range to show the new data-points
			// BUT...don't shrink the graph below 15 data-points, otherwise it just looks strange.
			NationalInstruments.UI.Range NewRange;
			if (PointsOnPlot <= 15)
			{
				NewRange = new NationalInstruments.UI.Range(0, 15);
				cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
			}
			else
			{
				NewRange = new NationalInstruments.UI.Range(0, PointsOnPlot);
				cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			}
			cwGraphDataAcq.XAxes[0].Range = NewRange;

			// Since we are putting new data on the plot...we want the Y-Axes to Auto-Adjust to the data values.
			cwGraphDataAcq.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
		   // cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
			// buttonXautoScale.Checked = true;
			// SetXaxisScaleAutoOrManual();

			// Plot the data onto the Graph
			// Create arrays of the exact-appropriate size taht will hold the data-samples
			var DataToPlotCH1 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH2 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH3 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH4 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH5 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH6 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH7 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH8 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH9 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH10 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH11 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??
			var DataToPlotCH12 = new double[PointsOnPlot];         // GARY-FIX: Should this maybe be ArrayPosition ??

			// Copy the data from the persistent DataSamplesCH array to the DataToPlot arrays
			if (PlotDataRelativeValues == false)
				for (int x = 0; x < PointsOnPlot; x++)
				{
					DataToPlotCH1[x] = RelativeValues[0][x] * DCfactor;
					DataToPlotCH2[x] = RelativeValues[1][x] * DCfactor;
					DataToPlotCH3[x] = RelativeValues[2][x] * DCfactor;
					DataToPlotCH4[x] = RelativeValues[3][x] * ACfactor;
					DataToPlotCH5[x] = RelativeValues[4][x] * ACfactor;
					DataToPlotCH6[x] = RelativeValues[5][x] * ACfactor;
					DataToPlotCH7[x] = RelativeValues[6][x] * ACfactor;
					DataToPlotCH8[x] = RelativeValues[7][x] * ACfactor;
					DataToPlotCH9[x] = RelativeValues[8][x] * ACfactor;
					DataToPlotCH10[x] = RelativeValues[9][x] * ACfactor;
					DataToPlotCH11[x] = RelativeValues[10][x] * ACfactor;
					DataToPlotCH12[x] = RelativeValues[11][x] * ACfactor;

				}
			// if (PlotDataRelativeValues == false)
			else
				for (int x = 0; x < PointsOnPlot; x++)
				{
					//DataToPlotCH1[x] = RawValues[0][x] * DCfactor;
					//DataToPlotCH2[x] = RawValues[1][x] * DCfactor;
					//DataToPlotCH3[x] = RawValues[2][x] * DCfactor;
					//DataToPlotCH4[x] = RawValues[3][x] * ACfactor;
					//DataToPlotCH5[x] = RawValues[4][x] * ACfactor;
					//DataToPlotCH6[x] = RawValues[5][x] * ACfactor;
					//DataToPlotCH7[x] = RawValues[6][x] * ACfactor;
					//DataToPlotCH8[x] = RawValues[7][x] * ACfactor;
					//DataToPlotCH9[x] = RawValues[8][x] * ACfactor;
					//DataToPlotCH10[x] = RawValues[9][x] * ACfactor;
					//DataToPlotCH11[x] = RawValues[10][x] * ACfactor;
					//DataToPlotCH12[x] = RawValues[11][x] * ACfactor;

					switch (recType)
					{
						case RecordType.g_RT:
							switch (plotIndex)
							{
								case 0:
									RawValues[cmbIndex][x] = DataToPlotCH1[x] = g_RT_ScanValues[0, x] * DCfactor;
									break;
								case 1:
									RawValues[cmbIndex][x] = DataToPlotCH2[x] = g_RT_ScanValues[1, x] * DCfactor;
									break;
								case 2:
									RawValues[cmbIndex][x] = DataToPlotCH3[x] = g_RT_ScanValues[2, x] * ACfactor;
									break;
								case 3:
									RawValues[cmbIndex][x] = DataToPlotCH4[x] = g_RT_ScanValues[3, x] * ACfactor;
									break;
								case 4:
									RawValues[cmbIndex][x] = DataToPlotCH5[x] = g_RT_ScanValues[4, x] * ACfactor;
									break;
								case 5:
									RawValues[cmbIndex][x] = DataToPlotCH6[x] = g_RT_ScanValues[5, x] * ACfactor;
									break;
								case 6:
									RawValues[cmbIndex][x] = DataToPlotCH7[x] = g_RT_ScanValues[6, x] * ACfactor;
									break;
								case 7:
									RawValues[cmbIndex][x] = DataToPlotCH8[x] = g_RT_ScanValues[7, x] * ACfactor;
									break;
								case 8:
									RawValues[cmbIndex][x] = DataToPlotCH9[x] = g_RT_ScanValues[8, x] * ACfactor;
									break;
								case 9:
									RawValues[cmbIndex][x] = DataToPlotCH10[x] = g_RT_ScanValues[9, x] * ACfactor;
									break;
								case 10:
									RawValues[cmbIndex][x] = DataToPlotCH11[x] = g_RT_ScanValues[10, x] * ACfactor;
									break;
								case 11:
									RawValues[cmbIndex][x] = DataToPlotCH12[x] = g_RT_ScanValues[11, x] * ACfactor;
									break;
								default:
									break;
							}

							break;
						case RecordType.gRMS:
							switch (plotIndex)
							{
								case 0:
									RawValues[cmbIndex][x] = DataToPlotCH1[x] = gRMS_ScanValues[0, x] * DCfactor;
									break;
								case 1:
									RawValues[cmbIndex][x] = DataToPlotCH2[x] = gRMS_ScanValues[1, x] * DCfactor;
									break;
								case 2:
									RawValues[cmbIndex][x] = DataToPlotCH3[x] = gRMS_ScanValues[2, x] * ACfactor;
									break;
								case 3:
									RawValues[cmbIndex][x] = DataToPlotCH4[x] = gRMS_ScanValues[3, x] * ACfactor;
									break;
								case 4:
									RawValues[cmbIndex][x] = DataToPlotCH5[x] = gRMS_ScanValues[4, x] * ACfactor;
									break;
								case 5:
									RawValues[cmbIndex][x] = DataToPlotCH6[x] = gRMS_ScanValues[5, x] * ACfactor;
									break;
								case 6:
									RawValues[cmbIndex][x] = DataToPlotCH7[x] = gRMS_ScanValues[6, x] * ACfactor;
									break;
								case 7:
									RawValues[cmbIndex][x] = DataToPlotCH8[x] = gRMS_ScanValues[7, x] * ACfactor;
									break;
								case 8:
									RawValues[cmbIndex][x] = DataToPlotCH9[x] = gRMS_ScanValues[8, x] * ACfactor;
									break;
								case 9:
									RawValues[cmbIndex][x] = DataToPlotCH10[x] = gRMS_ScanValues[9, x] * ACfactor;
									break;
								case 10:
									RawValues[cmbIndex][x] = DataToPlotCH11[x] = gRMS_ScanValues[10, x] * ACfactor;
									break;
								case 11:
									RawValues[cmbIndex][x] = DataToPlotCH12[x] = gRMS_ScanValues[11, x] * ACfactor;
									break;
								default:
									break;
							}
							break;
						case RecordType.p_RT:
							switch (plotIndex)
							{
								case 0:
									RawValues[cmbIndex][x] = DataToPlotCH1[x] = p_RT_ScanValues[0, x] * DCfactor;
									break;
								case 1:
									RawValues[cmbIndex][x] = DataToPlotCH2[x] = p_RT_ScanValues[1, x] * DCfactor;
									break;
								case 2:
									RawValues[cmbIndex][x] = DataToPlotCH3[x] = p_RT_ScanValues[2, x] * ACfactor;
									break;
								case 3:
									RawValues[cmbIndex][x] = DataToPlotCH4[x] = p_RT_ScanValues[3, x] * ACfactor;
									break;
								case 4:
									RawValues[cmbIndex][x] = DataToPlotCH5[x] = p_RT_ScanValues[4, x] * ACfactor;
									break;
								case 5:
									RawValues[cmbIndex][x] = DataToPlotCH6[x] = p_RT_ScanValues[5, x] * ACfactor;
									break;
								case 6:
									RawValues[cmbIndex][x] = DataToPlotCH7[x] = p_RT_ScanValues[6, x] * ACfactor;
									break;
								case 7:
									RawValues[cmbIndex][x] = DataToPlotCH8[x] = p_RT_ScanValues[7, x] * ACfactor;
									break;
								case 8:
									RawValues[cmbIndex][x] = DataToPlotCH9[x] = p_RT_ScanValues[8, x] * ACfactor;
									break;
								case 9:
									RawValues[cmbIndex][x] = DataToPlotCH10[x] = p_RT_ScanValues[9, x] * ACfactor;
									break;
								case 10:
									RawValues[cmbIndex][x] = DataToPlotCH11[x] = p_RT_ScanValues[10, x] * ACfactor;
									break;
								case 11:
									RawValues[cmbIndex][x] = DataToPlotCH12[x] = p_RT_ScanValues[11, x] * ACfactor;
									break;
								default:
									break;
							}
							break;
						case RecordType.pRMS:
							switch (plotIndex)
							{
								case 0:
									RawValues[cmbIndex][x] = DataToPlotCH1[x] = pRMS_ScanValues[0, x] * DCfactor;
									break;
								case 1:
									RawValues[cmbIndex][x] = DataToPlotCH2[x] = pRMS_ScanValues[1, x] * DCfactor;
									break;
								case 2:
									RawValues[cmbIndex][x] = DataToPlotCH3[x] = pRMS_ScanValues[2, x] * ACfactor;
									break;
								case 3:
									RawValues[cmbIndex][x] = DataToPlotCH4[x] = pRMS_ScanValues[3, x] * ACfactor;
									break;
								case 4:
									RawValues[cmbIndex][x] = DataToPlotCH5[x] = pRMS_ScanValues[4, x] * ACfactor;
									break;
								case 5:
									RawValues[cmbIndex][x] = DataToPlotCH6[x] = pRMS_ScanValues[5, x] * ACfactor;
									break;
								case 6:
									RawValues[cmbIndex][x] = DataToPlotCH7[x] = pRMS_ScanValues[6, x] * ACfactor;
									break;
								case 7:
									RawValues[cmbIndex][x] = DataToPlotCH8[x] = pRMS_ScanValues[7, x] * ACfactor;
									break;
								case 8:
									RawValues[cmbIndex][x] = DataToPlotCH9[x] = pRMS_ScanValues[8, x] * ACfactor;
									break;
								case 9:
									RawValues[cmbIndex][x] = DataToPlotCH10[x] = pRMS_ScanValues[9, x] * ACfactor;
									break;
								case 10:
									RawValues[cmbIndex][x] = DataToPlotCH11[x] = pRMS_ScanValues[10, x] * ACfactor;
									break;
								case 11:
									RawValues[cmbIndex][x] = DataToPlotCH12[x] = pRMS_ScanValues[11, x] * ACfactor;
									break;
								default:
									break;
							}
							break;
						default:
							break;
					}




				}


			lbPosition[cmbIndex].Text = RawValues[cmbIndex][PointsOnPlot - 1].ToString();

			// Create a "WaveForms" array from our local arrays
			//var waveforms = new AnalogWaveform<double>[12];
			//waveforms[0] = AnalogWaveform<double>.FromArray1D(DataToPlotCH1);
			//waveforms[1] = AnalogWaveform<double>.FromArray1D(DataToPlotCH2);
			//waveforms[2] = AnalogWaveform<double>.FromArray1D(DataToPlotCH3);
			//waveforms[3] = AnalogWaveform<double>.FromArray1D(DataToPlotCH4);
			//waveforms[4] = AnalogWaveform<double>.FromArray1D(DataToPlotCH5);
			//waveforms[5] = AnalogWaveform<double>.FromArray1D(DataToPlotCH6);
			//waveforms[6] = AnalogWaveform<double>.FromArray1D(DataToPlotCH7);
			//waveforms[7] = AnalogWaveform<double>.FromArray1D(DataToPlotCH8);
			//waveforms[8] = AnalogWaveform<double>.FromArray1D(DataToPlotCH9);
			//waveforms[9] = AnalogWaveform<double>.FromArray1D(DataToPlotCH10);
			//waveforms[10] = AnalogWaveform<double>.FromArray1D(DataToPlotCH11);
			//waveforms[11] = AnalogWaveform<double>.FromArray1D(DataToPlotCH12);
			var waveforms = new AnalogWaveform<double>(PointsOnPlot);
			switch (plotIndex)
			{
				case 0:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH1);
					break;
				case 1:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH2);
					break;
				case 2:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH3);
					break;
				case 3:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH4);
					break;
				case 4:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH5);
					break;
				case 5:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH6);
					break;
				case 6:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH7);
					break;
				case 7:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH8);
					break;
				case 8:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH9);
					break;
				case 9:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH10);
					break;
				case 10:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH11);
					break;
				case 11:
					waveforms = AnalogWaveform<double>.FromArray1D(DataToPlotCH12);
					break;
				default:
					break;
			}


			// Plot the data onto the Graph      PLOTS-HERE:
			this.BeginInvoke((MethodInvoker)delegate { cwGraphDataAcq.Plots[cmbIndex].PlotWaveform(waveforms); });

			// Anytime we plot the points, update the number of points
			//   this.Invoke((System.Action)(() => { textBoxRecordsOnPlot.Text = PointsOnPlot.ToString(); }));

			// Update the Get-Once text boxes with the data from the last sample on the plot
			//            UpdateGetOnceTextBoxes(PointsOnPlot - 1);    // GARY-FIX: Is this new line working out okay? (Below)
			//  UpdateGetOnceTextBoxes(ArrayPosition);

			// Set the X-Axis Slider Range to match how many points are on the graph
			//XSliderUpdate();
		}

		//=== HANDLE + PLOT DATA WHEN WE GET IT
		//*** UPDATE THE MIN / MAX / AVERAGE / STANDARD DEVIATION SECTION
		private void UpdateMinMaxAverageDisplay()
		{
			// If we are calling this and there are no records yet (as will happen when radio-button is initially selected) just set all boxes blank
			if (PointsOnPlot == 0)
			{
				txtMinSignal_lineNum.Text = "0";
				txtMinSignal.Text = "0";
				labelMinTime.Text = "0";
				txtMaxSignal_lineNum.Text = "0";
				txtMaxSignal.Text = "0";
				labelMaxTime.Text = "0";
				txtPk_Pk.Text = "0";
				measAvg.Text = "0";
				measError.Text = "0";
				measSNR.Text = "0";
				return; // Exit this method
			}

			//// GARY-DEBUG: SET A STOPWATCH TO SEE HOW LONG ALL OF THIS TAKES -- WRITE THE ELAPSED TIME TO THE CONSOLE @ END OF PROCEDURE
			//CalculationsStopwatch.Restart();
			// Find the Minimum & Maximum Values for the Selected Plot
			//if (PointsOnPlot > 20) System.Diagnostics.Debugger.Break();
			int MinIndex = 0; double MinValue = RawValues[PlotSelected][0];
			int MaxIndex = 0; double MaxValue = RawValues[PlotSelected][0];

			// NOTE:    PointsOnPlot is ONE-MORE-THAN the last array element's index.
			//          Example: When there are 21 points on the plot and 21 elements in the array
			//                   the array index goes from ZERO to 20 (21 elements)
			for (int x = 1; x < PointsOnPlot; x++)
			{
				if (RawValues[PlotSelected][x] < MinValue) { MinIndex = x; MinValue = RawValues[PlotSelected][x]; }
				if (RawValues[PlotSelected][x] > MaxValue) { MaxIndex = x; MaxValue = RawValues[PlotSelected][x]; }
			}

			// Update the Minimum Text Boxes:
			txtMinSignal_lineNum.Text = MinIndex.ToString();
			txtMinSignal.Text = MinValue.ToString();
			labelMinTime.Text = TimeStamp[MinIndex].ToString("HH:mm:ss");

			// Update the Maximum Text Boxes:
			txtMaxSignal_lineNum.Text = MaxIndex.ToString();
			txtMaxSignal.Text = MaxValue.ToString();
			labelMaxTime.Text = TimeStamp[MaxIndex].ToString("HH:mm:ss");

			// Update the Peak-To-Peak Value
			double PeakToPeak = MaxValue - MinValue;
			txtPk_Pk.Text = PeakToPeak.ToString();

			// Convert the RawValues array into a single dimensional array of ONLY the data-set we're interested in ---
			// ...The RawValues "Jagged" array has more array-elements than actual data (empty or zero elements on the end of the array)
			// ...It also has values for all of the six different data-sets that we got from the controller.
			// ...Here we copy just the actual data (not the non-valid end elements) from the ONE data-set we're looking for
			double[] ArrayOfValues = new double[PointsOnPlot];
			Array.Copy(RawValues[PlotSelected], 0, ArrayOfValues, 0, PointsOnPlot);

			// Update the Average
			double Average = ArrayOfValues.Average();
			measAvg.Text = Average.ToString("#####0.00");

			// Update the Standard-Deviation
			double DistanceFromMean;
			double DistanceFromMeanSquared;
			double SumOfDifferences = 0;
			foreach (double DataValue in ArrayOfValues)
			{
				DistanceFromMean = DataValue - Average;
				DistanceFromMeanSquared = DistanceFromMean * DistanceFromMean;
				SumOfDifferences = SumOfDifferences + DistanceFromMeanSquared;
			}
			double AverageOfTheSums = SumOfDifferences / PointsOnPlot;
			double StandardDeviation = Math.Sqrt(AverageOfTheSums);
			measError.Text = StandardDeviation.ToString("#####0.00");

			//// Update the Signal-To-Noise
			double SignalToNoise = Math.Abs(Average / StandardDeviation);
			measSNR.Text = SignalToNoise.ToString("#####0.00");

			// GARY-DEBUG: HOW LONG DID ALL OF THAT TAKE?
			//CalculationsStopwatch.Stop();
			//Debug.WriteLine("Calculations in Milliseconds: {0}", CalculationsStopwatch.ElapsedMilliseconds);
		}

		//=== X-AXIS SLIDER
		//    private void slideXAxis_BeforeChangeValue(object sender, BeforeChangeNumericValueEventArgs e) { }



		private void slideXAxis_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			if (FormLoading || Program.IsReadingControllerSettings)
				return;
			labelDebugXSliderValue.Text = slideXAxis.Value.ToString();

			// Figure Out the SIZE of the range we are displayig on the Real-Time-Monitoring (RTM) Plot
			// GARY-FIX: What if the range is negative?
			double RangeMin = XminScale;
			double RangeMax = XmaxScale;
			double RangeSize = RangeMax - RangeMin;
			double RangeSizeHalf = RangeSize / 2;

			// Move the PLOT's RANGE VIEW to be in line with the X-SLIDER
			double CenterValue = slideXAxis.Value;
			double NewRangeMin  = RangeSizeHalf==0 ? CenterValue - 10 :  CenterValue - RangeSizeHalf;
			double NewRangeMax  = RangeSizeHalf == 0 ? CenterValue + 10 : CenterValue + RangeSizeHalf;
			NationalInstruments.UI.Range r = new NationalInstruments.UI.Range(NewRangeMin, NewRangeMax);
			XminScale = Convert.ToSingle(NewRangeMin);
			XmaxScale = Convert.ToSingle(NewRangeMin);
			cwGraphDataAcq.XAxes[0].Range = r;
		}

		string FileSaveName = "";
		string FileSavePath = "";
		int FileSaveLastIndexWritten;
		string FileSaveFullPathAndName;
		string FileSaveExt;
		private void cwButContinuesSend_CheckedChanged(object sender, EventArgs e)
		{
			if ((cwButContinuesSend.Checked))
			{
				cmb_TestType.Enabled = false;
				// *********** set up for continuous position monitoring
				PC_RealTime = true;
				ScanNumber = 0;
				prepare_for_plotting();
				if ((cwContCMDperiod.Value == 0))
				{
					CMDperiod = 0.1;
					// 0.1 sec
				}
				else
				{
					CMDperiod = cwContCMDperiod.Value;
				}
				aTimerContMonitoring.Interval = (int)(CMDperiod * 1000);
				//  ScanNumber = 0;
				//  Array_Line_Counter = (int)ScanNumber;
				////  cwGraphDataAcq.ClearData();
				// ClearRTArray();


				// 1000 diagnostic gives 1.03-1.04, up to 1.21 interval at *1000
				// 960 gives interval 0.99925
				// 460-454 gives interval 0.49925 (0.484-0.51)
				// 258 gives interval 0.296-0.421
				// 158 gives interval 0.20275
				// 80 gives interval 0.125
				// 63 gives interval 0.0109
				// 62 gives interval 0.09225-0.099
				//  Interval    Timer setting
				//  1.035           1000
				//  0.99925         960
				//  0.49925         458
				//  0.296           258
				//  0.20275         158
				//  0.125           80
				//  0.109           63
				//  0.09925         62
				// y = 1006.2x - 43.432
				ScanValuesToPlot = new double[0, 0];
				cmdReadPositionFile.Enabled = false;
				cwSaveScan.Enabled = false;
				//  cwSaveScan.Enabled = false;

				if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
				{
					stateButtonRelAbs.Checked = false;
				}


				aTimerContMonitoring.Enabled = true;
				cwContCMDperiod.Enabled = false;
				if ((cwSaveScan.Checked == false))
				{
					//             File_Saving = False
					SaveExsistingData = false;
				}
				else
				{
					//             File_Saving = True
				}

			}
			else
			{
				// cwButContinuesSend = False - stop
				cmb_TestType.Enabled = true;
				aTimerContMonitoring.Enabled = false;
				cwContCMDperiod.Enabled = true;
				cmdReadPositionFile.Enabled = true;
				cwSaveScan.Enabled = true;
				// cwSaveScan.Enabled = true;
				// SaveToFile();

			}
		}

		private void prepare_for_plotting()
		{
			if (Program.formTerminal != null)
			{
				Program.formTerminal.OnQuitMenuCommand();
				Program.formTerminal.Visible = false;
			}
			HandleToCallingForm.lstResponse.Items.Clear();
			HandleToCallingForm.ToggleRTmonitoring.Checked = false;
			lbLineNum.Text = "0";
			clear_DataAcqArrays();
		}

		private void clear_DataAcqArrays()
		{
			int i = 0;
			int c = 0;
			int max_i = 0;
			//int max_c = 0;
			for (i = 0; i < MAX_SCAN_ARRAY_SIZE; i++)
			{
				for (c = 0; c < MAX_CH ; c++)
				{
					ScanValues[c, i] = 0;
				}
				ScanTimeStamps[i] = 0;
			}
			if (plt_X_coord != null)
				max_i = plt_X_coord.GetUpperBound(0);
			for (i = 0; i < max_i; i++)
			{
				plt_X_coord[i] = 0;
				for (c = 0; c < MAX_CH; c++)
				{
					ScanValues[c, i] = 0;
				}
			}
		}

		private void SaveToFile()
		{
			DateTime rightNow = DateTime.Now;
			string JustFilename;
			int Aindex;
			// Set a default File-Name and File-Extention like: 10-02-2016_22-53 Real_time
			FileSaveName = rightNow.Year.ToString("0000-") + rightNow.Month.ToString("00-") + rightNow.Day.ToString("00 ")
			+ rightNow.Hour.ToString("00") + "-" + rightNow.Minute.ToString("00") + " Real_time";
			FileSaveExt = ".csv";

			saveFileDialog.FileName = FileSaveName;
			if (FileSavePath == "")
				saveFileDialog.InitialDirectory = FileSavePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\TMC\\TMCAnalyzer\\data";

			DialogResult Dresult = saveFileDialog.ShowDialog();       // Show the dialog to the user
			if (Dresult == System.Windows.Forms.DialogResult.Cancel)
			{
				cwButContinuesSend.Checked = false;
				return;     //cancelled
			}
			Aindex = saveFileDialog.FileName.LastIndexOf("\\");
			JustFilename = saveFileDialog.FileName.Substring(Aindex + 1);
			FileSaveCreateNewFile();
			// Open the existing file and APPEND to it.
			System.IO.StreamWriter file = new System.IO.StreamWriter(FileSaveFullPathAndName, append: true);

			// Start writting from the index position that is ONE AFTER the last one we wrote.
			int StartAtIndexPosition = FileSaveLastIndexWritten + 1;

			// Write the file from the RawValues array
			for (int rec = 0; rec < ScanNumber; rec++)
			{
				string OutTime = TimeStamp[rec].ToString("HH:mm:ss");

				string index = rec.ToString();
				string Outg_RT = AppendValuesInString(ref g_RT_ScanValues, rec);
				string OutgRMS = AppendValuesInString(ref gRMS_ScanValues, rec);
				string Outp_RT = AppendValuesInString(ref p_RT_ScanValues, rec);
				string OutpRMS = AppendValuesInString(ref pRMS_ScanValues, rec);

				string FileLine = OutTime + "," + Outg_RT + ",%," + OutgRMS + ",%," + Outp_RT + ",%," + OutpRMS;
				file.WriteLine(FileLine);
			}
			file.Close();
			file.Dispose();

		}

		private string GetDefaultFileName()
		{

			DateTime rightNow = DateTime.Now;
			if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
				return rightNow.Year.ToString("0000-") + rightNow.Month.ToString("00-") + rightNow.Day.ToString("00 ")
			+ rightNow.Hour.ToString("00") + "-" + rightNow.Minute.ToString("00") + " Real_time";
			else if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
				return rightNow.Year.ToString("0000-") + rightNow.Month.ToString("00-") + rightNow.Day.ToString("00 ")
			+ rightNow.Hour.ToString("00") + "-" + rightNow.Minute.ToString("00") + " Pressure_vs_time";
			else
			{
				return rightNow.Year.ToString("0000-") + rightNow.Month.ToString("00-") + rightNow.Day.ToString("00 ")
			+ rightNow.Hour.ToString("00") + "-" + rightNow.Minute.ToString("00") + " Test_VlvBrd";
			}

		}

		StreamWriter file = new StreamWriter("test.txt");
		private bool SaveDataAcqFile(bool Next_Multiple_file)
		{
			DateTime rightNow = DateTime.Now;
			bool IsSaved = false;
			string DefaultFileName = string.Empty;
			string start_saving_time = string.Empty;
			string FileName = string.Empty;
			int i = 0;
			double m_p = 0;
			string rsp;

			if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
				DefaultFileName = rightNow.ToString("MM-dd-yyyy_HH-mm") + " Real_time";
			if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
				DefaultFileName = rightNow.ToString("MM-dd-yyyy_HH-mm") + " Pressure_vs_time";
			if (MonitoringTestType == (Int32)TestType.Valves_2020)
			{
				DefaultFileName = rightNow.ToString("MM-dd-yyyy_HH-mm") + " Test_VlvBrd";

				if (txtV1sn.Text != "")
					DefaultFileName = DefaultFileName + ",v1-" + txtV1sn.Text.Trim();
				if (txtV2sn.Text != "")
					DefaultFileName = DefaultFileName + ",v2-" + txtV2sn.Text.Trim();
				if (txtV3sn.Text != "")
					DefaultFileName = DefaultFileName + ",v3-" + txtV3sn.Text.Trim();
				if (txtV4sn.Text != "")
					DefaultFileName = DefaultFileName + ",v4-" + txtV4sn.Text.Trim();

				DefaultFileName = DefaultFileName + ".csv";
			}
			if (ScanNumber > 1)
			{
				DialogResult result = MessageBox.Show("There is data on the plot: " + ScanNumber.ToString() + " data record lines." + "\r\n" + "Press 'YES' to save THIS data or" + "\r\n" + "'NO' to clear, RECORD and SAVE NEW data" + "\r\n" + "Save Exsisting Data ?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
				if (result == DialogResult.Yes)
					SaveExsistingData = true;
				else
					SaveExsistingData = false;
				Program.FormMain.SendInternal("echo>enab", CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);
			}

			if (!Next_Multiple_file)
			{
				// User_has_been_Warned = false;
				saveFileDialog.Title = "Choose name for the measurement";
				saveFileDialog.DefaultExt = "csv";
				saveFileDialog.Filter = "Data File (*.csv)|*.csv|All files (*.*)|*.*";
				saveFileDialog.FileName = DefaultFileName;
				DialogResult result = saveFileDialog.ShowDialog();
				if (result == DialogResult.Cancel)
				{
					File_Saving = false;
					return IsSaved;
				}
				else
				{
					txtStatus.Text = "Saving data into file '" + saveFileDialog.FileName + "......";
					SaveFileName = saveFileDialog.FileName;
					i = saveFileDialog.FileName.LastIndexOf("\\");
					DefaultDirectory = saveFileDialog.FileName.Substring(0, i);
					saveFileDialog.InitialDirectory = DefaultDirectory;
					if (!SaveExsistingData)
					{
						cwButContinuesSend.Enabled = false;
						ChkExcelFriendly.Enabled = false;
						CalcNumber_ofRecords();
						Number_of_Records = Total_Number_of_Records;
						clear_DataAcqArrays();
					}
				}
			}
			else
			{
				i = saveFileDialog.FileName.LastIndexOf("\\");
				DefaultDirectory = saveFileDialog.FileName.Substring(0, i - 1);
				saveFileDialog.InitialDirectory = DefaultDirectory;
				SaveFileName = saveFileDialog.InitialDirectory + "\\" + DefaultFileName;
			}

			txtStatus.Text = "Saving data into file(s) " + saveFileDialog.FileName + ".......";

			i = SaveFileName.LastIndexOf("\\");
			FileName = SaveFileName.Substring(i + 1);
			File_Saving = true;
			file = new StreamWriter(SaveFileName, true);
			// {
			//file = new System.IO.StreamWriter(FileName, true);
			file.WriteLine("File name," + FileName);
			file.WriteLine(",Date," + rightNow.ToString());
			start_saving_time = rightNow.ToShortTimeString();
			file.WriteLine(",Time, " + start_saving_time);
			if (MonitoringTestType == (Int32)TestType.Valves_2020)
				m_p = cwValveMeasTime.Value;
			else
				m_p = cwContCMDperiod.Value;
			CMDperiod = cwContCMDperiod.Value;

			if (MonitoringTestType == (Int32)TestType.Valves_2020)
			{
				file.WriteLine("Scan Type:, Valve Test");
				file.Write("command:,");
				file.Write("vtst" + " ");
				file.Write(cwValveStartCurrent.Value.ToString() + " " + cwValveIncrementCurrent.Value.ToString() + " ");
				file.Write(cwValveEndCurrent.Value.ToString() + " " + cwValveMeasTime.Value.ToString() + " ");
				file.WriteLine(CwLowerPSIthreshold.Value + " " + CwUpperPSIthreshold.Value);
				file.WriteLine(" period:," + cwValveMeasTime.Value.ToString() + ",sec");
			}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
			{
				file.WriteLine("Scan Type:, Pressure vs time");
				file.WriteLine("request period, " + m_p + ",<<<");
			}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
			{
				file.WriteLine("Scan Type:, Position vs time");
				file.WriteLine("request period, " + m_p + ",<<<");
			}

			if (!SaveExsistingData)
			{
				line_counter = 0;
				file.Write(" lines:, collecting in real time");
			}
			else
				file.Write(" lines:, " + ScanNumber);

			if (MonitoringTestType == (Int32)TestType.Valves_2020)
			{
				file.WriteLine(",,Valve Serial Number,," + txtV1sn.Text + "," + txtV2sn.Text + "," + txtV3sn.Text + "," + txtV4sn.Text);
				file.WriteLine(" line#,InfCnt,EhxCnt, Inflat_mA,Exhaust_mA, PSI_1,PSI_2,PSI_3,PSI_4,PSI_in, time");
			}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
			{
				file.WriteLine("\r\n" + "time,  V1,    V2,    V3,    H1,    H2,    H3,  FF_Xpos,  FF_Ypos,  FF_Xacc,  FF_Yacc");
			}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
			{
				file.WriteLine("\r\n" + "time, PSI_1, PSI_2, PSI_3, PSI_4, PSI_in, ");
			}
			//Header is done

			Time_Start_Saving = DateAndTime.Timer;
			Date_Start_Saving = DateAndTime.Today;

			if (SaveExsistingData)
			{
				for (i = 0; i <= ScanNumber - 1; i++)
				{
					write_one_line(i);
				}
				file.WriteLine(", - End of PC-saved file");
				cwSaveScan.Checked = false;
			}
			else
			{
				if (MonitoringTestType == (Int32)TestType.Valves_2020)
					cwButValveTest.Checked = true;
				else
				{
					CalcNumber_ofRecords();
					cwButContinuesSend.Checked = true;
				}
				txtStatus.Text = "Saving data into file(s)'" + saveFileDialog.FileName + "......";

			}


			IsSaved = true;
			//  }

			return IsSaved;

		}


		private void write_one_line(int i)
		{
			int ch;
			int active_channels = 0;
			string t_str;
			// t_str = "'" + ConvertSnglToTime(ScanTimeStamps[i]);
			t_str = "'" + TimeSpan.FromSeconds(ScanTimeStamps[i]).ToString(@"hh\:mm\:ss\:fff");

			if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
			{
				active_channels = Convert.ToInt32(MAX_CH);
				file.Write(t_str + ",");
			}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
			{
				active_channels = 4;
				file.Write(t_str + ",");
			}
			if (MonitoringTestType == (Int32)TestType.Valves_2020)
			{
				file.Write(i + ",");
				active_channels = Convert.ToInt32(MAX_CH-1);
			}
			for (ch = 0; ch < active_channels; ch++)
			{
				file.Write(ScanValues[ch, i] + ",");
			}
			if (MonitoringTestType == (Int32)TestType.Valves_2020)
				file.Write(t_str + ",");
			file.WriteLine("%");
		}

		private string ConvertSnglToTime(double time_stamp)
		{
			string t_str;
			double tmp_val;
			double t_time;
			int t_hours;

			tmp_val = (time_stamp - 0.4999) % 60;
			t_str = tmp_val.ToString("00");
			t_time = time_stamp - tmp_val;
			tmp_val = (t_time - 0.4999) % 3600;
			tmp_val = tmp_val / 60;
			t_str =  tmp_val.ToString("00") + ":" + t_str;
			t_time = t_time - tmp_val;
			tmp_val = (t_time - 0.4999) % 86400;
			t_hours = Convert.ToInt16(((tmp_val / 3600) - 0.4999));
			t_str = t_hours.ToString("00") + ":" + t_str;
			t_time = (t_time - tmp_val)*1000;
			t_str = t_str +"."+ t_time.ToString("000");
			return t_str;
			}


		private string AppendValuesInString(ref double[,] ScanDataArray, int scan_num = 0)
		{
			string result;

			result =
			ScanDataArray[0, scan_num].ToString() + "," +
			ScanDataArray[1, scan_num].ToString() + "," +
			ScanDataArray[2, scan_num].ToString() + "," +
			ScanDataArray[3, scan_num].ToString() + "," +
			ScanDataArray[4, scan_num].ToString() + "," +
			ScanDataArray[5, scan_num].ToString() + "," +
			ScanDataArray[6, scan_num].ToString() + "," +
			ScanDataArray[7, scan_num].ToString() + "," +
			ScanDataArray[8, scan_num].ToString() + "," +
			ScanDataArray[9, scan_num].ToString() + "," +
			ScanDataArray[10, scan_num].ToString() + "," +
			ScanDataArray[11, scan_num].ToString();

			return result;
		}

		private void FileSaveCreateNewFile()
		{
			// Figure Out the File Path and Name
			DateTime rightNow = DateTime.Now;
			string year = rightNow.Year.ToString("0000");
			string month = rightNow.Month.ToString("00");
			string day = rightNow.Day.ToString("00");
			string hour = rightNow.Hour.ToString("00");
			string minute = rightNow.Minute.ToString("00");
			string second = rightNow.Second.ToString("00");
			string FileNameDate = year + "-" + month + "-" + day;
			string FileNameTime = hour + "h " + minute + "m " + second + "s";
			FileSaveFullPathAndName = FileSavePath + "\\" + FileSaveName + " ending " + FileNameDate + " @ " + FileNameTime + FileSaveExt;
			/*//////////////////////////////////////////////
						if (buttonStart.Checked) {
							DateTime rightNow = DateTime.Now;
							string JustFilename;

							String DefaultFileName = MagneticAxis.ToUpper(System.Globalization.CultureInfo.InvariantCulture) + "_TF " + rightNow.Year.ToString("0000-") + rightNow.Month.ToString("00-") + rightNow.Day.ToString("00 ")
							+ rightNow.Hour.ToString("00") + "-" + rightNow.Minute.ToString("00") + " " + MagneticAxis.ToUpper(System.Globalization.CultureInfo.InvariantCulture) + "_gain=" + gain.ToString() + InstallTypeStr;
							saveScanFileDialog.FileName = DefaultFileName;
							saveScanFileDialog.InitialDirectory = TMC_PathData;
							DialogResult Dresult = saveScanFileDialog.ShowDialog();       // Show the dialog to the user
							if (Dresult == System.Windows.Forms.DialogResult.Cancel) {
								buttonSaveScan.Checked = false;
								return;     //cancelled
							}
							Aindex = saveScanFileDialog.FileName.LastIndexOf("\\");
							JustFilename = saveScanFileDialog.FileName.Substring(Aindex + 1);
							//textBoxFileName.Text = saveScanFileDialog.FileName;     // put out file path/name to bottom of form (could be changed to JustFileName)
							TextWriter writer = new StreamWriter(saveScanFileDialog.FileName, true, System.Text.Encoding.ASCII, 1048576);

							// Put out header lines
							writer.WriteLine(String.Format("File Name,{0}", JustFilename));

			/////////////////////////////////////////////*/
			// Example of File Name:
			// RT Monitoring ending 2014-06-20 @ 13h 26m 07s.csv
			// GARY-DEBUG:
			// Debug.WriteLine("File name will be:   " + FileSaveFullPathAndName);

			// Setup a Stream-Writer so we can write to the file
			System.IO.StreamWriter file = new System.IO.StreamWriter(FileSaveFullPathAndName);

			// Write the header
			// file.WriteLine("Technical Manufacturing Corportion");
			file.WriteLine("File name,{0}", FileSaveName);
			file.WriteLine("Date  ,  {0}/{1}/{2}", year, month, day);
			file.WriteLine("Time  ,  {0}:{1}:{2}", hour, minute, second);
			file.WriteLine("Scan Type:, Signal vs time");
			file.WriteLine("request period , {0}", cwNumFileRecords.Value.ToString());
			file.WriteLine(" lines , {0} ", ScanNumber.ToString());
			file.WriteLine(" , g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, g_RT, | ,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,gRMS,|,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,p_RT,|,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS,pRMS");
			file.WriteLine("time,Z1,X1, Y1, Z2, X2, Y2, Z3,X3,Y3,Z4,X4, Y4,	|,Z1, X1,Y1,Z2,X2, Y2,Z3, X3,Y3,Z4, X4, Y4,|, Z1, X1, Y1, Z2, X2, Y2, Z3,X3	,Y3	,Z4	,X4	,Y4	,|,	Z1, X1, Y1	,Z2	, X2, Y2, Z3,X3,Y3,Z4, X4,Y4");
			file.Close();
			file.Dispose();


		}
		private void aTimerContMonitoring_Tick(object sender, EventArgs e)
		{
			string[] ret_data = new string[4];
			string[] command = new string[4];
			string command_Renamed = string.Empty;
			string response = string.Empty;
			int scan_line = 0;
			int ch = 0;
			double exact_time = 0;
			//if (chkRecGeoRT.Checked)
			//    command[0] = "g_rt";
			//if (chkRecGeoRMS.Checked)
			//    command[1] = "grms";
			//if (chkRecPiezoRT.Checked)
			//    command[2] = "p_rt";
			//if (chkRecPiezoRMS.Checked)
			//    command[3] = "prms";
			//if (chkRecGeoRT.Checked == false && chkRecGeoRMS.Checked == false &&
			//    chkRecPiezoRT.Checked == false && chkRecPiezoRMS.Checked == false)
			//{
			//    return;
			//}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
			{
				command_Renamed = "b";
			}
			if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
			{
				command_Renamed = "getpa";
			}
			HandleToCallingForm.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			scan_line = ExtractDataFromScanLine(response, ScanNumber);
			ScanTimeStamps[ScanNumber] = DateAndTime.Timer;
			if (cwSaveScan.Checked)
			{
				if ((Convert.ToDateTime(Date_Start_Saving).AddDays(1) == DateAndTime.Today) && (DateAndTime.Timer > Time_Start_Saving))
				{
					if (file != null) file.Close();
					SaveDataAcqFile(true);
				}
				if (ScanNumber <= Number_of_Records || ChkUntilSTOP.Checked)
				{
					ch = response.IndexOf(",");
					if (ch >= 5 && ch < 10)
					{
						command_Renamed = response.Substring(ch + 1);
						exact_time = DateAndTime.Timer;
						command_Renamed = "'" + TimeSpan.FromSeconds(exact_time).ToString(@"hh\:mm\:ss\:fff")  + "," + command_Renamed;
						if (file != null) file.WriteLine(command_Renamed);
					}
					else
					{
						if (file != null) file.WriteLine(response);
					}

				}
				else
				{
					cwSaveScan.Checked = false;
				}

			}

			for (ch = 0; ch < MAX_CH; ch++)
			{
				lbPosition[ch].Text = string.Format("{00000}", ScanValues[ch, ScanNumber].ToString());
			}

			UpdatePositionPlot(ScanNumber);

			ScanNumber = ScanNumber + 1;
			lbLineNum.Text = string.Format("{0000}", ScanNumber);
			if (ScanNumber > (Recorded_SCAN_ARRAY_SIZE - 1))
			{
				ScanNumber = ScanNumber - 1;
				if (cwSaveScan.Checked)
				{
					if (file != null) file.Close();
					SaveDataAcqFile(true);
				}

			}


			//if (ChkUntilSTOP.Checked == false)
			//{

			//    if (ScanNumber > cwNumFileRecords.Value)
			//    {
			//        this.Invoke((System.Action)(() =>
			//        {
			//            cwButContinuesSend.Checked = false;
			//        }));
			//    }
			//}

			//if (!string.IsNullOrEmpty(command[0]))
			//    this.Invoke((System.Action)(() =>
			//        {
			//            HandleToCallingForm.SendInternal(command[0], CommandTypes.ResponseExpected, out ret_data[0], CommandAckTypes.AckExpected);
			//            lblRawResponse.Text = ret_data[0]; //' display current line
			//        }));
			//if (!string.IsNullOrEmpty(command[1]))
			//    this.Invoke((System.Action)(() =>
			//        {
			//            HandleToCallingForm.SendInternal(command[1], CommandTypes.ResponseExpected, out ret_data[1], CommandAckTypes.AckExpected);
			//            lblRawResponse.Text = ret_data[1]; //' display current line
			//        }));
			//if (!string.IsNullOrEmpty(command[2]))
			//    this.Invoke((System.Action)(() =>
			//        {
			//            HandleToCallingForm.SendInternal(command[2], CommandTypes.ResponseExpected, out ret_data[2], CommandAckTypes.AckExpected);
			//            lblRawResponse.Text = ret_data[2]; //' display current line
			//        }));
			//if (!string.IsNullOrEmpty(command[3]))
			//    this.Invoke((System.Action)(() =>
			//        {
			//            HandleToCallingForm.SendInternal(command[3], CommandTypes.ResponseExpected, out ret_data[3], CommandAckTypes.AckExpected);
			//            lblRawResponse.Text = ret_data[3]; //' display current line
			//        }));
			////Just for testing
			////if (ret_data[0].ToLower() == "timeout" || ret_data[1].ToLower() == "timeout" || ret_data[2].ToLower() == "timeout" ||
			////    ret_data[3].ToLower() == "timeout")
			////{
			////    if (ScanNumber % 2 == 0)
			////    {
			////        ret_data[0] = "-478,13, 14,-164,26,-16,-154,-2, 23,-620,-7, -4,%";
			////        ret_data[1] = "31, 1, 2,-195, 3,14,37,-6,23,-42, 6,-8,%";
			////        ret_data[2] = "-478,13, 14,-164,26,-16,-154,-2, 23,-620,-7, -4,%";
			////        ret_data[3] = "234,10,-4,-258,-21, 7,-18,19,-3,-165,10, 4,%";
			////    }
			////    else if (ScanNumber % 3 == 0)
			////    {
			////        ret_data[3] = "-478,13, 14,-164,26,-16,-154,-2, 23,-620,-7, -4,%";
			////        ret_data[2] = "31, 1, 2,-195, 3,14,37,-6,23,-42, 6,-8,%";
			////        ret_data[1] = "-478,13, 14,-164,26,-16,-154,-2, 23,-620,-7, -4,%";
			////        ret_data[0] = "234,10,-4,-258,-21, 7,-18,19,-3,-165,10, 4,%";
			////    }
			////    else
			////    {
			////        ret_data[1] = "-478,13, 14,-164,26,-16,-154,-2, 23,-620,-7, -4,%";
			////        ret_data[0] = "31, 1, 2,-195, 3,14,37,-6,23,-42, 6,-8,%";
			////        ret_data[3] = "-478,13, 14,-164,26,-16,-154,-2, 23,-620,-7, -4,%";
			////        ret_data[2] = "234,10,-4,-258,-21, 7,-18,19,-3,-165,10, 4,%";
			////    }

			////}
			////end of testing
			////ScanTimeStamps[ScanNumber] = DateTime.Now;
			//this.Invoke((System.Action)(() =>
			//{
			//    label_DataLine.Text = ScanNumber.ToString();
			//}));

			//TimeStamp[ScanNumber] = DateTime.Now;

			//if (ret_data[0].ToLower() != "timeout" || ret_data[1].ToLower() != "timeout" || ret_data[2].ToLower() != "timeout" || ret_data[3].ToLower() != "timeout")
			//{
			//    ExtractDataFromScanLine(ret_data[0], ref g_RT_ScanValues, (int)ScanNumber);
			//    ExtractDataFromScanLine(ret_data[1], ref gRMS_ScanValues, (int)ScanNumber);
			//    ExtractDataFromScanLine(ret_data[2], ref p_RT_ScanValues, (int)ScanNumber);
			//    ExtractDataFromScanLine(ret_data[3], ref pRMS_ScanValues, (int)ScanNumber);

			//    PointsOnPlot = Array_Line_Counter + 1;
			//    ArrayPosition = Array_Line_Counter;

			//    PupulateRawValArray();

			//    Convert_ONE_RawValue_To_RelativeValue(Array_Line_Counter);

			//    PlotAllPoints();
			//    for (int index = 0; index < lbPosition.Count; index++)
			//    {
			//        lbPosition[index].Text = RawValues[index][Array_Line_Counter].ToString();
			//    }
			//    ScanNumber = ScanNumber + 1;
			//    Array_Line_Counter = (int)ScanNumber;


			//}


		}

		private void UpdatePositionPlot(int scan_num = -2)
		{
			int i = 0;
			int j = 0;
			int smpl = 0;
			if (MonitoringTestType == (Int32)TestType.Valves_2020)
			{
				cwGraphDataAcq.XAxes[0].Caption = "";
				cwGraphDataAcq.YAxes[0].Caption = "Pressure, PSI";
				if(TextXunits.InvokeRequired) TextXunits.BeginInvoke((MethodInvoker)delegate
				{
					TextXunits.Visible = true;
				});
				else TextXunits.Visible = true;

				if (Cmb_X_scale.InvokeRequired) Cmb_X_scale.BeginInvoke((MethodInvoker)delegate { XaxisUnits = Cmb_X_scale.SelectedIndex; });
				if (TextXunits.InvokeRequired) TextXunits.BeginInvoke((MethodInvoker)delegate
				{
					if (XaxisUnits == 0) TextXunits.Text = " line #";
					if (XaxisUnits == 1) TextXunits.Text = " sec";
					if (XaxisUnits == 1) TextXunits.Text = " Inflation_Cnt";
					if (XaxisUnits == 3) TextXunits.Text = " Ehxhaust_Cnt";
					if (XaxisUnits == 4) TextXunits.Text = " Inflation_mA";
					if (XaxisUnits == 5) TextXunits.Text = " Exhaust_mA";
				}); else
				{
					if (XaxisUnits == 0) TextXunits.Text = " line #";
					if (XaxisUnits == 1) TextXunits.Text = " sec";
					if (XaxisUnits == 1) TextXunits.Text = " Inflation_Cnt";
					if (XaxisUnits == 3) TextXunits.Text = " Ehxhaust_Cnt";
					if (XaxisUnits == 4) TextXunits.Text = " Inflation_mA";
					if (XaxisUnits == 5) TextXunits.Text = " Exhaust_mA";
				}

				if (!SaveExsistingData)
				{
					if (scan_num > -1)
						j = scan_num;
					else
						j = ScanNumber - 1;
				}
				else
				{
					j = line_counter - 1;
				}
				if (j <= 0) j = 0;
				if (j < MAX_SCAN_ARRAY_SIZE - 1)
				{
					ResizeArray(ref ScanValuesToPlot, j+1, Convert.ToInt32(MAX_CH));
					Array.Resize(ref plt_X_coord, j+1);
				}
				if (XaxisUnits == 0 || XaxisUnits == 1)
				{
					if (TextXunits.InvokeRequired)  TextXunits.BeginInvoke((MethodInvoker)delegate { TextXunits.Visible = true; }); else TextXunits.Visible = true;
					//if(XaxisUnits ==0 ) cwGraphDataAcq.XAxes[0].form    /////////Need to check what is equivalent property for FormatString

					if (scan_num == -1)
					{
						for (smpl = 0; smpl <= j; smpl++)
						{
							if (XaxisUnits == 0)
								plt_X_coord[smpl] = smpl;
							else
								plt_X_coord[j] = ScanTimeStamps[j] - ScanTimeStamps[0];
							for (i = 0; i < MAX_CH - 4; i++)
							{
								if (stateButtonRelAbs.Checked)
									ScanValuesToPlot[i, smpl] = RelativeScanValues[i + 4, smpl];
								else
									ScanValuesToPlot[i, smpl] = ScanValues[i + 4, smpl];
							}
						}
					}
					else
					{
						plt_X_coord[j] = ScanTimeStamps[j] - ScanTimeStamps[0];
						for (i = 0; i < MAX_CH - 4; i++)
						{
							if (stateButtonRelAbs.Checked)
								ScanValuesToPlot[i, j] = RelativeScanValues[i + 4, j];
							else
								ScanValuesToPlot[i, j] = ScanValues[i + 4, j];
						}
					}
					XminScale = Convert.ToSingle(plt_X_coord[0]);
					XmaxScale = Convert.ToSingle(plt_X_coord[Information.UBound(plt_X_coord)]);
					PlotAllPoints(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(XminScale, XmaxScale);
					//cwGraphDataAcq.PlotYAppendMultiple(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				}
				if (XaxisUnits > 1)
				{
					if (scan_num == -1)
					{
						for (smpl = 0; smpl <= j; smpl++)
						{
							plt_X_coord[smpl] = ScanValues[2, smpl];
							for (i = 0; i < MAX_CH - 4; i++)
							{
								if (stateButtonRelAbs.Checked == true)
									ScanValuesToPlot[i, smpl] = RelativeScanValues[i + 4, smpl];
								else
									ScanValuesToPlot[i, smpl] = ScanValues[i + 4, smpl];
							}
						}
					}
					else
					{
						plt_X_coord[j] = ScanValues[2, j];
						for (i = 0; i < MAX_CH - 4; i++)
						{
							if (stateButtonRelAbs.Checked == true)
								ScanValuesToPlot[i, j] = RelativeScanValues[i + 4, j];
							else
								ScanValuesToPlot[i, j] = ScanValues[i + 4, j];
						}
					}
					XminScale = Convert.ToSingle(plt_X_coord[0]);
					XmaxScale = plt_X_coord.GetUpperBound(0);
					PlotAllPoints(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(XminScale, XmaxScale);
					//cwGraphDataAcq.PlotYAppendMultiple(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				}
			}
			else  //position test
			{
				if (MonitoringTestType == (Int32)TestType.Monitor_Pos || MonitoringTestType == (Int32)TestType.Monitor_Pressure)
				{
					if (MonitoringTestType == (Int32)TestType.Monitor_Pos)
						cwGraphDataAcq.YAxes[0].Caption = "counts";
					if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
						cwGraphDataAcq.YAxes[0].Caption = "PSI";
					update_Graph_time_scale();
				}
				if(TextXunits.InvokeRequired)  TextXunits.BeginInvoke((MethodInvoker)delegate { TextXunits.Visible = true; }); else TextXunits.Visible = true;

				if (TextXunits.InvokeRequired)  TextXunits.BeginInvoke((MethodInvoker)delegate { TextXunits.Text = "sec"; }); else TextXunits.Text = "sec";
				if (!SaveExsistingData)
				{
					if (scan_num > -1)
						j = scan_num;
					else
						j = ScanNumber - 1;
				}
				else
					j = line_counter - 1;
				if (j <= 0) j = 0;
				if (j < MAX_SCAN_ARRAY_SIZE)
				{
					ResizeArray(ref ScanValuesToPlot, j + 1, Convert.ToInt32(MAX_CH));
					Array.Resize(ref plt_X_coord, j + 1);
				}
				plt_X_coord[j] = ScanTimeStamps[j] - ScanTimeStamps[0];
				MaxScanTimeStamp = plt_X_coord[j];
				//XSliderUpdate(plt_X_coord[j]);

				// cwGraphDataAcq.   Format String?
				if (scan_num == -1)
				{
					for (smpl = 0; smpl <= j; smpl++)
					{
						plt_X_coord[smpl] = ScanTimeStamps[smpl] - ScanTimeStamps[0];
						MaxScanTimeStamp = plt_X_coord[smpl];
						for (i = 0; i < MAX_CH; i++)
						{
							if (stateButtonRelAbs.Checked)
								ScanValuesToPlot[i, smpl] = RelativeScanValues[i, smpl];
							else
								ScanValuesToPlot[i, smpl] = ScanValues[i, smpl];
						}
					}
					XminScale = Convert.ToSingle(plt_X_coord[0]);
					XmaxScale = plt_X_coord.GetUpperBound(0);
					PlotAllPoints(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(XminScale, XmaxScale);
					//cwGraphDataAcq.PlotYAppendMultiple(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				}
				else
				{
					for (i = 0; i < MAX_CH; i++)
					{
						if (stateButtonRelAbs.Checked)
							ScanValuesToPlot[i, j] = RelativeScanValues[i, j];
						else
							ScanValuesToPlot[i, j] = ScanValues[i, j];
					}
				}
				XminScale = Convert.ToSingle(plt_X_coord[0]);
				XmaxScale = plt_X_coord.GetUpperBound(0);
				if (j < 1000)
				{
					PlotAllPoints(ScanValuesToPlot);
					// cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(XminScale, XmaxScale);
					//cwGraphDataAcq.PlotYAppendMultiple(ScanValuesToPlot);
					//cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				}
				else
				{
					if ((j > 1000) && (j % 10 == 0))
					{
						PlotAllPoints(ScanValuesToPlot);
						// cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(XminScale, XmaxScale);
						//cwGraphDataAcq.PlotYAppendMultiple(ScanValuesToPlot);
						//cwGraphDataAcq.YAxes[1].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
					}
				}
			}
		}

		private void update_Graph_time_scale()
		{
			int i_tmp = 0;
			Cmb_X_scale.SelectedIndex = 1;
			XaxisUnits = Cmb_X_scale.SelectedIndex;
			i_tmp = Convert.ToInt32(cwContCMDperiod.Value * 10);
			//  if (Cmb_X_scale.SelectedIndex == 1)
			// cwGraphDataAcq.YAxes[0].    //FormatString??
			// else
			// cwGraphDataAcq.YAxes[0].    //FormatString??

		}



		/// <summary>
		/// Mthod for resizing a multi dimensional array
		/// </summary>
		/// <param name="original">Original array you want to resize
		/// <param name="cols"># of columns in the new array
		/// <param name="rows"># of rows in the new array
		private void ResizeArray(ref double[,] original, int cols, int rows)
		{
			//create a new 2 dimensional array with
			//the size we want
			double[,] newArray = new double[rows, cols];
			//copy the contents of the old array to the new one
			for(int row=0; row<= original.GetUpperBound(0); row++)
			{
				for (int col = 0; col <= original.GetUpperBound(1); col++)
				{
					newArray[row, col] = original[row, col];
				}
			}
		  //  Array.Copy(original, newArray, original.Length);
			//set the original to the new array
			original = newArray;
		}


		private void ExtractDataFromScanLine(string data_line, ref double[,] ScanDataArray, int scan_num = 0)
		{

			// Data lines follow the format:
			// '23:22:51.863, 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,%, 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,%, 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,%, 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,%
			// for (var /*count*/ cnt = /*because 5 lines above*/ 3; /*more data?*/ stm.Peek() > 0; cnt++)
			{
				// txt = /*next line*/ stm.ReadLine();
				data_line = data_line.TrimEnd('\r', '\n');
				var /*entries*/ nts = data_line.Split(',');
				if (/*bad?*/ nts.Count() != 13)
				{
					//throw new System.InvalidOperationException("Data line[" + cnt.ToString() + "] is corrupt:\r\"" + txt + "\"");
				}
				else
				{
					ScanDataArray[0, scan_num] = double.Parse(nts[0]);
					ScanDataArray[1, scan_num] = double.Parse(nts[1]);
					ScanDataArray[2, scan_num] = double.Parse(nts[2]);
					ScanDataArray[3, scan_num] = double.Parse(nts[3]);
					ScanDataArray[4, scan_num] = double.Parse(nts[4]);
					ScanDataArray[5, scan_num] = double.Parse(nts[5]);
					ScanDataArray[6, scan_num] = double.Parse(nts[6]);
					ScanDataArray[7, scan_num] = double.Parse(nts[7]);
					ScanDataArray[8, scan_num] = double.Parse(nts[8]);
					ScanDataArray[9, scan_num] = double.Parse(nts[9]);
					ScanDataArray[10, scan_num] = double.Parse(nts[10]);
					ScanDataArray[11, scan_num] = double.Parse(nts[11]);
				}
			}
		}
		bool PC_RealTime = false;
		private int ExtractDataFromScanLine(string data_line, int scan_num = 0)
		{
			string tmpDataStr = string.Empty;
			string tmp_str = string.Empty;
			int ch = 0;
			int seperator = 0;
			int pt_seperator = 0;
			int str_len = 0;
			double tmp_time = 0;
			int retVal = -2;
			if (scan_num < 1000)
				if (lblRawResponse.InvokeRequired) lblRawResponse.BeginInvoke((MethodInvoker)delegate { lblRawResponse.Text = data_line; }); else lblRawResponse.Text = data_line;
			else
				{
				if ((scan_num % 10) == 0)
				{
					if (lblRawResponse.InvokeRequired) lblRawResponse.BeginInvoke((MethodInvoker)delegate
					{
						lblRawResponse.Text = data_line;
					}); else lblRawResponse.Text = data_line;
				}
				}
			seperator = data_line.IndexOf("%")+1;
			if (seperator == 0)
				seperator = data_line.IndexOf("*")+1;
			if (data_line != "Timeout" && data_line != "" && seperator != 0)
			{
				tmpDataStr = data_line.Substring(0, seperator - 2);
				lbLineNum.BackColor = ColorTranslator.FromHtml("#C0FFC0");
			}
			else
			{
				if (data_line.IndexOf("PC-save") != -1)
				{
					retVal = -1;
					return retVal;
				}
				else
				{
					lbLineNum.BackColor = ColorTranslator.FromHtml("#8080ff");
					retVal = -1;
					return retVal;
				}
			}
			if (tmpDataStr.Length < 2)
			{
				retVal = -1;
				return retVal;
			}
			if (scan_num > -1)
			{
				if (File_Reading)
				{
					pt_seperator = data_line.IndexOf(":");
					if (pt_seperator > 0)
						seperator = data_line.IndexOf(",", pt_seperator);
					else
						seperator = data_line.IndexOf(",");
					if (pt_seperator != 0)
					{
						tmp_str = data_line.Substring(pt_seperator - 2, seperator - pt_seperator + 3);
						tmp_time = COnvertTimeStringToSingle(tmp_str);
						if (scan_num > 0)
						{
							if (tmp_time == 0)
								tmp_time = ScanTimeStamps[scan_num - 1] + CMDperiod;
							else
							{
								if (scan_num == 3)
								{
									if (MonitoringTestType == (Int32)TestType.Valves_2020)
										CMDperiod = cwValveMeasTime.Value;
									else
									{
										CMDperiod = ScanTimeStamps[2] - ScanTimeStamps[1];
										CMDperiod = Math.Round(CMDperiod, 3);
										if (cwContCMDperiod.Value != 0)
										{
											if ((Math.Abs(CMDperiod - cwContCMDperiod.Value) / cwContCMDperiod.Value) < 0.1)
												CMDperiod = cwContCMDperiod.Value;
										}
										if (CMDperiod < 0.02) PC_RealTime = false;
									}
								}
							}
						}
						if (scan_num == 0) ScanSTARTtimeStamp = (tmp_time);
						ScanTimeStamps[scan_num] = tmp_time;
						if (MonitoringTestType == (Int32)TestType.Valves_2020)
						{
							if (ScanSTARTtimeStamp == 0 && scan_num > 0)
								ScanTimeStamps[scan_num] = ScanTimeStamps[scan_num - 1] + CMDperiod;
							if (!data_line.Substring(0, 1).Equals("'"))
								tmpDataStr = data_line.Substring(0, pt_seperator - 4);
							else
							{
								tmpDataStr = data_line.Substring(seperator + 1);
								tmpDataStr = "0000," + tmpDataStr;
							}
						}
						else
						{
							tmp_str = data_line.Substring(seperator + 1);
							tmpDataStr = "0000," + tmp_str;
						}
					}
					else
					{
						if (scan_num == 0)
						{
							ScanTimeStamps[scan_num] = 0;
							ScanSTARTtimeStamp = 0;
						}
						else
						{
							ScanTimeStamps[scan_num] = ScanTimeStamps[scan_num - 1] + CMDperiod;
						}
						if (MonitoringTestType != (Int32)TestType.Monitor_Pos)
						{
							tmpDataStr = data_line.Substring(seperator + 1);
							tmpDataStr = "0000," + tmpDataStr;
						}
					}
				}
				else
				{
					tmp_time = DateAndTime.Timer;
					if ((scan_num == 0) && (line_counter == 0))
						ScanSTARTtimeStamp = (tmp_time);
					if (!SaveExsistingData)
					{
						if (MonitoringTestType == (Int32)TestType.Monitor_Pressure)
							tmpDataStr = string.Format("0#", ScanNumber) + "," + data_line;
					}
					else
					{
						ScanTimeStamps[line_counter] = tmp_time;
					}
				}
			}
			seperator = tmpDataStr.IndexOf(",")+1;
			tmp_str = tmpDataStr.Substring(0, seperator - 1);
			if (formMain.IsNumeric(tmp_str))
				retVal = Convert.ToInt32(Conversion.Val(tmp_str));
			else
			{
				retVal = -1;
				return retVal;
			}

			tmpDataStr = tmpDataStr.Substring(seperator);


		// process_data:
			for (ch = 0; ch < MAX_CH; ch++)
			{
				str_len = tmpDataStr.Length;
				if (str_len == 0)
				{
					retVal = -1;
					return retVal;
				}
				else
				{
					OneScanValues[ch] = 0;
				}
				seperator = tmpDataStr.IndexOf(",");
				if (seperator != -1)
					tmp_str = tmpDataStr.Substring(0, seperator);
				else
					tmp_str = tmpDataStr;
				if (formMain.IsNumeric(tmp_str))
				{
					OneScanValues[ch] = Conversion.Val(tmp_str);
					if (scan_num == 0)
					{
						ScanValues[ch, retVal] = Conversion.Val(tmp_str);
						RelativeScanValues[ch, retVal] = Conversion.Val(tmp_str) - ScanValues[ch, 0] + cwSpreadPlotsByY.Value * (5 - ch);
					}
					else
					{
						ScanValues[ch, scan_num] = Conversion.Val(tmp_str);
						RelativeScanValues[ch, scan_num] = Conversion.Val(tmp_str) - ScanValues[ch, 0] + cwSpreadPlotsByY.Value * (5 - ch);
					}
				}
				tmpDataStr = tmpDataStr.Substring(seperator + 1);
			}

			retVal = scan_num;
			return retVal;

		}

		private double COnvertTimeStringToSingle(string time_string)
		{
			string t_str = string.Empty;
			string cut_str = string.Empty;
			double t_time = 0;
			int sep = 0;

			t_str = time_string;
			sep = t_str.IndexOf("'");
			if (sep != -1) t_str = t_str.Substring(2);

			sep = t_str.IndexOf(":");
			if (sep != -1)
			{
				cut_str = t_str.Substring(0, sep);
				t_time = Conversion.Val(cut_str) * 3600;
				t_str = t_str.Substring(sep + 1);
			}
			sep = t_str.IndexOf(":");
			if (sep != -1)
			{
				cut_str = t_str.Substring(0, sep );
				t_time = t_time + Conversion.Val(cut_str) * 60;
				t_str = t_str.Substring(sep + 1);
			}
			t_time = t_time + Conversion.Val(t_str);
			return t_time;
		}

		private void Lbl_StartStopLTF_Click(object sender, EventArgs e)
		{

		}


		private void cwBut_Excitation_CheckedChanged(object sender, EventArgs e)
		{
			if (HandleToCallingForm.Test_in_progress) return;
			if (FormLoading == true) return;

			Program.FormMain.ExcitationStatus = false;
			string resp = string.Empty;

			if (/*busy?*/ Program.IsReadingControllerSettings) return;


			CheckBox checkBoxCtrl = sender as CheckBox;
			if (checkBoxCtrl.Checked == true)
				checkBoxCtrl.BackgroundImage = Resources.SwitchOnYellow;
			else
				checkBoxCtrl.BackgroundImage = Resources.SwitchOffGreen;

			if ((checkBoxCtrl.Checked == false))
			{
				HandleToCallingForm.SendInternal("excit>stop", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				// none (cancel)
				HandleToCallingForm.ContinuousMonitoringOffOn(false);
				Program.FormMain.ExcitationStatus = false;
			}
			else
			{

				HandleToCallingForm.SendInternal("excit>start", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				// enable excitation
				Program.FormMain.ExcitationStatus = true;
			}

			checkBoxCtrl.Checked = Program.FormMain.ExcitationStatus;

		   // HandleToCallingForm.ContinuousMonitoringOffOn(Program.FormMain.Excitation_status);
		}

		private void cwNumExcitFreq_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			if (FormLoading == true) return;

			if (/*busy?*/ Program.IsReadingControllerSettings) return;

			string resp;
			Program.FormMain.TestFrequency = cwNumExcitFreq.Value;
			string controller_command = ("freq=" + ((NationalInstruments.UI.WindowsForms.NumericEdit)(sender)).Value.ToString());
			//retval = Analyzer.GetSend(gain_cmd, true);
			HandleToCallingForm.SendInternal(controller_command, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void cwNumExcitAmpl_ValueChanged(object sender, EventArgs e)
		{
			if (FormLoading == true) return;

			if (/*busy?*/ Program.IsReadingControllerSettings) return;

			string resp;
			Program.FormMain.TestGain = cwNumExcitAmpl.Value;
			string controller_command = ("ampl=" + ((NationalInstruments.UI.WindowsForms.NumericEdit)(sender)).Value.ToString());
			//retval = Analyzer.GetSend(gain_cmd, true);
			HandleToCallingForm.SendInternal(controller_command, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void buttonAutoScale_CheckedChanged(object sender, EventArgs e)
		{
			// Take care of auto or fixed scaling on graph
			if (buttonAutoScale.Checked)
			{ //AUTO
				cwGraphDataAcq.YAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				//-!- scatterGraphMag.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose; //-!- this sets scale 0.1 to 5000
				cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.AutoScaleLoose;
				cwGraphDataAcq.InteractionModeDefault = NationalInstruments.UI.GraphDefaultInteractionMode.ZoomXY; // in this mode user cannot move cursor
			}
			else
			{ //MANUAL (FIXED)
				cwGraphDataAcq.YAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
				cwGraphDataAcq.InteractionModeDefault = NationalInstruments.UI.GraphDefaultInteractionMode.None; // cursor can be moved
					var NewRange = new NationalInstruments.UI.Range(YminScale, YmaxScale);
					cwGraphDataAcq.YAxes[0].Range = NewRange;

			}
		}

		private void ChkUntilSTOP_CheckedChanged(object sender, EventArgs e)
		{
			///Commented because Vb6 has nothing inside this event
			//if (ChkUntilSTOP.Checked == false)
			//    cwNumFileRecords.Enabled = true;
			//else
			//    cwNumFileRecords.Enabled = false;
		}

		private void cwSaveScan_CheckedChanged(object sender, EventArgs e)
		{
			bool retVal = false;
			if (cwSaveScan.Checked == true)  //start saving
			{
				aTimerContMonitoring.Enabled = false;
				cmdReadPositionFile.Enabled = false;
				retVal = SaveDataAcqFile(false);
				if (!retVal)
				{
					cwSaveScan.Checked = false;
					File_Saving = false;
				}
				else
				{

					if (!SaveExsistingData)
					{
						ChkUntilSTOP.Enabled = false;
						cwNumFileRecords.Enabled = false;
						cwButContinuesSend.Enabled = false;
					}
				}
			}
			else
			{
				cmdReadPositionFile.Enabled = true;
				file.Close();
				cwButContinuesSend.Checked = false;
				cwButValveTest.Checked = false;
				cwButContinuesSend.Enabled = true;
				cwNumFileRecords.Enabled = true;
				ChkExcelFriendly.Enabled = true;
				ChkUntilSTOP.Enabled = true;
				File_Saving = false;
				if (retVal)
				{
					txtStatus.Text = "Measurement file '" + SaveFileName + "' Saved";
				}
			}
		}


		private void cmb_TestType_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cwButContinuesSend.Checked)
				cwButContinuesSend.Checked = false;
			if (cwButValveTest.Checked)
				cwButValveTest.Checked = false;
			MonitoringTestType = cmb_TestType.SelectedIndex;
			ChangeTestType(MonitoringTestType);
		}

		private void ChangeTestType(int Index)
		{
			if (Index == (Int32)TestType.Monitor_Pressure || Index == (Int32)TestType.Valves_2020)  // 1,2
			{
				optChooseCh[0].Text = "Iso1_psi";
				optChooseCh[1].Text = "Iso2_psi";
				optChooseCh[2].Text = "Iso3_psi";
				optChooseCh[3].Text = "Iso4_psi";
				optChooseCh[4].Text = "Inp_PSI";
				optChooseCh[5].Text = "Prox H3";
				optChooseCh[6].Text = "FF Xpos";
				optChooseCh[7].Text = "FF Ypos";
				optChooseCh[8].Text = "FF Xacc";
				optChooseCh[9].Text = "FF Yacc";
				optChooseCh[5].Visible = false;
				optChooseCh[6].Visible = false;
				optChooseCh[7].Visible = false;
				optChooseCh[8].Visible = false;
				optChooseCh[9].Visible = false;
				chkPOSvisible[5].Checked = false;
				chkPOSvisible[6].Checked = false;
				chkPOSvisible[7].Checked = false;
				chkPOSvisible[8].Checked = false;
				chkPOSvisible[9].Checked = false;
				chkPOSvisible[5].Visible = false;
				chkPOSvisible[6].Visible = false;
				chkPOSvisible[7].Visible = false;
				chkPOSvisible[8].Visible = false;
				chkPOSvisible[9].Visible = false;
				lbPosition[5].Visible = false;
				lbPosition[6].Visible = false;
				lbPosition[7].Visible = false;
				lbPosition[8].Visible = false;
				lbPosition[9].Visible = false;
				cmdGetCurrentPosition.Visible = false;
				cwButContinuesSend.Enabled = true;
				cwButContinuesSend.Visible = true;
				cwNumFileRecords.Enabled = true;
				ChkExcelFriendly.Enabled = true;
				if (Index == (Int32)TestType.Monitor_Pressure)
				{
					FrameValveTest.Visible = false;
					frameMonitoring.Visible = true;
					FrameInternalRecording.Visible = false;
					Cmb_X_scale.SelectedIndex = 1;
					XaxisUnits = Cmb_X_scale.SelectedIndex;
					TextXunits.Text = " sec";
				}
				else
				{
					FrameValveTest.Visible = true;
					FrameValveTest.BringToFront();
					frameMonitoring.Visible = false;
					FrameInternalRecording.Visible = false;
					Cmb_X_scale.SelectedIndex = 4;
					XaxisUnits = Cmb_X_scale.SelectedIndex;
					if (XaxisUnits == 4) TextXunits.Text = " Inflation_mA";
				}
			}
			else   //'index 0 or 1, 0=Monitor Pos; 1=Buffer Rec , show save and read buttons until test is done
			{
				cwSaveScan.Visible = true;
				if (Index == (Int32)TestType.Monitor_Pos)
				{
					FrameValveTest.Visible = false;
					frameMonitoring.Visible = true;
					FrameInternalRecording.Visible = false;
					Cmb_X_scale.SelectedIndex = 1;
					XaxisUnits = Cmb_X_scale.SelectedIndex;
					TextXunits.Text = " sec";
				}
				if (Index == 1)
				{
					FrameValveTest.Visible = false;
					frameMonitoring.Visible = false;
					FrameInternalRecording.Visible = true;
					Cmb_X_scale.SelectedIndex = 1;
					XaxisUnits = Cmb_X_scale.SelectedIndex;
					TextXunits.Text = " sec";
				}

				optChooseCh[0].Text = "Prox V1";
				optChooseCh[1].Text = "Prox V2";
				optChooseCh[2].Text = "Prox V3";
				optChooseCh[3].Text = "Prox H1";
				optChooseCh[4].Text = "Prox H2";
				optChooseCh[5].Text = "Prox H3";
				optChooseCh[6].Text = "FF Xpos";
				optChooseCh[7].Text = "FF Ypos";
				optChooseCh[8].Text = "FF Xacc";
				optChooseCh[9].Text = "FF Yacc";
				chkPOSvisible[5].Checked = true;
				chkPOSvisible[6].Checked = true;
				chkPOSvisible[7].Checked = true;
				chkPOSvisible[8].Checked = true;
				chkPOSvisible[9].Checked = true;
				optChooseCh[5].Visible = true;
				optChooseCh[6].Visible = true;
				optChooseCh[7].Visible = true;
				optChooseCh[8].Visible = true;
				optChooseCh[9].Visible = true;
				chkPOSvisible[5].Visible = true;
				chkPOSvisible[6].Visible = true;
				chkPOSvisible[7].Visible = true;
				chkPOSvisible[8].Visible = true;
				chkPOSvisible[9].Visible = true;
				lbPosition[5].Visible = true;
				lbPosition[6].Visible = true;
				lbPosition[7].Visible = true;
				lbPosition[8].Visible = true;
				lbPosition[9].Visible = true;
				cmdGetCurrentPosition.Visible = true;
				Cmb_X_scale.SelectedIndex = 0;
				XaxisUnits = Cmb_X_scale.SelectedIndex;
				if (XaxisUnits == 0) TextXunits.Text = "line#";
			}
		}

		private void cwValveEndCurrent_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{

		}

		private void cmdGetCurrentPosition_Click(object sender, EventArgs e)
		{
			GetOneScanLine();
		}

		private void GetOneScanLine()
		{
			string command_Renamed = string.Empty;
			int scan_line = 0;
			string response = string.Empty;
			command_Renamed = "b";

			HandleToCallingForm.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			scan_line = ExtractDataFromScanLine(response);
			Show_raw_Values();
		}

		private void Show_raw_Values()
		{
			object Y;
			int ch;
			lbLineNum.Text = string.Format("{0000}", ScanNumber);
			for (ch = 0; ch < MAX_CH; ch++)
			{
				Y = OneScanValues[ch];
				if (Math.Abs(Convert.ToInt32(Y)) > 100)
					lbPosition[ch].Text = string.Format("{0000}", Y.ToString());
				else
					lbPosition[ch].Text = string.Format("{0:00.00}", Y.ToString());
			}
		}

		private void Show_Scan_raw_values(int indx)
		{
			double Y =0;
			int ch;
			if(MonitoringTestType == (Int32)TestType.Valves_2020)
			{
				for(ch =0; ch<=MAX_CH -2; ch++)
				{
					Y = ScanValuesToPlot[ch, indx];
					if (Math.Abs(Y) > 100)
					{
						if (lbPosition[ch].InvokeRequired) lbPosition[ch].BeginInvoke(((MethodInvoker)delegate { lbPosition[ch].Text = Y.ToString("000"); }));
						else lbPosition[ch].Text = Y.ToString("000");
					}

					else
					{

						if (lbPosition[ch].InvokeRequired) lbPosition[ch].BeginInvoke(((MethodInvoker)delegate { lbPosition[ch].Text = Y.ToString("00.00"); }));
						else lbPosition[ch].Text = Y.ToString("00.00");
					}
				}
			}
			else
			{
				for (ch = 0; ch < MAX_CH; ch++)
				{
					Y = ScanValues[ch, indx];
					if (Math.Abs(Y) > 100)
					{
						if (lbPosition[ch].InvokeRequired) lbPosition[ch].BeginInvoke(((MethodInvoker)delegate { lbPosition[ch].Text = Y.ToString("00.000"); }));
						else lbPosition[ch].Text = Y.ToString("00.00");
					}

					else
					{

						if (lbPosition[ch].InvokeRequired) lbPosition[ch].BeginInvoke(((MethodInvoker)delegate { lbPosition[ch].Text = Y.ToString("00.00"); }));
						else lbPosition[ch].Text = Y.ToString("00.00");
					}
				}
			}
		}



		private void formMonitoring_FormClosed(object sender, FormClosedEventArgs e)
		{

		}

		private void formMonitoring_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.ApplicationExitCall)
			{
				Program.FormMain.OpenCloseMonitoringForm();
				e.Cancel = true;
				this.Hide();
			}
		}

		private void cwContCMDperiod_ValueChanged(object sender, EventArgs e)
		{
			double CMD_period = 0;
			if (cwContCMDperiod.Value == 0)
			{
				CMD_period = 0.1;
			}
			else
			{
				CMD_period = cwContCMDperiod.Value;
			}
			aTimerContMonitoring.Interval = Convert.ToInt32(CMD_period * 1000);
			CMDperiod = CMD_period;  //upate global value
			CalcNumber_ofRecords();
			update_Graph_time_scale();

		}

		private void CalcNumber_ofRecords()
		{
			if (cwSaveScan.Checked)
			{
				if (!ChkUntilSTOP.Checked)
					Number_of_Records = Number_of_Records - ScanNumber;
				else
				{
					if (ScanNumber == 0)
						Number_of_Records = 0;
					else
						Number_of_Records = Number_of_Records + ScanNumber + 1;
				}
				ScanNumber = 0;
			}
		}

		private void cwNumFileRecords_ValueChanged(object sender, EventArgs e)
		{
			Recorded_SCAN_ARRAY_SIZE = Convert.ToInt64(cwNumFileRecords.Value);
		}

		private void ChkExcelFriendly_CheckedChanged(object sender, EventArgs e)
		{
			if(ChkExcelFriendly.Checked)
			{
				if (Recorded_SCAN_ARRAY_SIZE > EXCEL_SCAN_ARRAY_SIZE)
					Recorded_SCAN_ARRAY_SIZE = EXCEL_SCAN_ARRAY_SIZE;
			}
			else
			{
				if (Recorded_SCAN_ARRAY_SIZE > MAX_SCAN_ARRAY_SIZE)
					Recorded_SCAN_ARRAY_SIZE = MAX_SCAN_ARRAY_SIZE;
			}
			cwNumFileRecords.Value = Recorded_SCAN_ARRAY_SIZE;
		}

		private void optExitation_CheckedChanged(object sender, EventArgs e)
		{
			if (FormLoading) return;
			if (/*busy?*/ Program.IsReadingControllerSettings) return;

			RadioButton optExitation = (RadioButton)sender;
			string resp = string.Empty;
			if(optExitation.Checked)
			{
				if(optExitation == _optExitation_0)
				{
					HandleToCallingForm.SendInternal("freq=" + cwNumExcitFreq.Value, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				}
				if (optExitation == _optExitation_1)
				{
					HandleToCallingForm.SendInternal("freq=0", CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
				}
				lblRawResponse.Text = resp;
			}
		}

		private void cmdPulse_Click(object sender, EventArgs e)
		{
			string controller_command = string.Empty;
			controller_command = "p";
			string resp = string.Empty;
			HandleToCallingForm.SendInternal(controller_command, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			Program.FormMain.ExcitationStatus = false;
			ToggleExcitation.Checked = Program.FormMain.ExcitationStatus;
			Program.FormMain.ToggleExcitation.Checked = Program.FormMain.ExcitationStatus;

		}

		private void cmb_ExcitAxis_SelectedIndexChanged(object sender, EventArgs e)
		{

			if (/*busy?*/ Program.IsReadingControllerSettings || FormLoading) return;
			string command_Renamed = string.Empty;
			string resp = string.Empty;
			if (FormLoading) return;
			Program.FormMain.ExcitationAxis = cmb_ExcitAxis.SelectedIndex;
			command_Renamed = "drvo=" + cmb_ExcitAxis.SelectedIndex.ToString();
			HandleToCallingForm.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void cwGraphDataAcq_AfterMoveCursor(object sender, NationalInstruments.UI.AfterMoveXYCursorEventArgs e)
		{
			//Update_labels();
			double X = 0, Y = 0;
			int array_index = 0;
			string time_str = string.Empty, data_string = string.Empty;

			array_index = cwGraphDataAcq.Cursors[0].GetCurrentIndex();

			if (array_index == -1) return;
			// if (ChosenCh - 1 < 0) return;
			//X = cwGraphDataAcq.Cursors[0].XPosition;
			X = plt_X_coord[array_index];
			Y = (MonitoringTestType == (Int32)TestType.Valves_2020) ? ScanValuesToPlot[ChosenCh, array_index] : ScanValues[ChosenCh, array_index];

			if (!cwButContinuesSend.Checked)
			{
				if (MonitoringTestType == (Int32)TestType.Valves_2020)
				{
					if (Math.Abs(Y) > 100)
						data_string = "Pressure " + optChooseCh[(int)ChosenCh].Text.Substring(0, 4) + "=" + Y.ToString("#0.00") + "psi @ Current=" + X.ToString("#0.00") + "mA";
					else
						data_string = "Pressure " + optChooseCh[(int)ChosenCh].Text.Substring(0, 4) + "=" + Y.ToString("#0.00") + "psi @ Current=" + X.ToString("#0.00") + "mA";
					cwGraphDataAcq.Caption = "Index = " + array_index.ToString("#0000") + "; " + data_string;
					Show_Scan_raw_values(array_index);
				}
				else
				{
					if (Math.Abs(Y) > 100)
						data_string = optChooseCh[(int)ChosenCh].Text + " Value = " + Y.ToString("000");
					else
						data_string = optChooseCh[(int)ChosenCh].Text + " Value = " + Y.ToString("00.00");
					time_str = TimeSpan.FromSeconds(X).ToString(@"hh\:mm\:ss\:fff"); //ConvertSnglToTime(X);
					cwGraphDataAcq.Caption = "Index = " + array_index.ToString("#0000") + "; Time = " + time_str + "; " + data_string;
					Show_Scan_raw_values(array_index);
				}
			}
		}

		private void Update_labels()
		{
			double X = 0, Y = 0;
			int array_index = 0;
			string time_str = string.Empty, data_string = string.Empty;

			array_index = cwGraphDataAcq.Cursors[0].GetCurrentIndex();

			if (array_index == -1) return;
		   // if (ChosenCh - 1 < 0) return;
		   // X = cwGraphDataAcq.Cursors[0].XPosition;
			X = plt_X_coord[array_index];
			Y = (MonitoringTestType == (Int32)TestType.Valves_2020) ? ScanValuesToPlot[ChosenCh, array_index] : ScanValues[ChosenCh, array_index];

			if(!cwButContinuesSend.Checked)
			{
				if(MonitoringTestType == (Int32)TestType.Valves_2020)
				{
					if (Math.Abs(Y) > 100)
						data_string = "Pressure " + optChooseCh[(int)ChosenCh].Text.Substring(0, 4) + "=" + Y.ToString("#0.0") + "psi @ Current=" + X.ToString("#0.00") + "mA";
					else
						data_string = "Pressure " + optChooseCh[(int)ChosenCh].Text.Substring(0, 4) + "=" + Y.ToString("#0.00") + "psi @ Current=" + X.ToString("#0.00") + "mA";
					cwGraphDataAcq.Caption = "Index = " + array_index.ToString("#0000") + "; " + data_string;
					Show_Scan_raw_values(array_index);
				}
				else
				{
					if (Math.Abs(Y) > 100)
						data_string = optChooseCh[(int)ChosenCh].Text + " Value = " + Y.ToString("000");
					else
						data_string = optChooseCh[(int)ChosenCh].Text + " Value = " + Y.ToString("00.00");
					time_str = TimeSpan.FromSeconds(X).ToString(@"hh\:mm\:ss\:fff"); //ConvertSnglToTime(X);
					cwGraphDataAcq.Caption = "Index = " + array_index.ToString("#0000") + "; Time = " + time_str + "; "+ data_string;
					Show_Scan_raw_values(array_index);
				}
			}
			else
			{
				cwGraphDataAcq.Caption = "elapsed time, sec = " + (X * CMDperiod).ToString("0000.0");
			}
		}

		private void cwGraphDataAcq_Click(object sender, EventArgs e)
		{
			int dummy = 0;
			if (cwGraphDataAcq.Cursors[0].XPosition <= cwGraphDataAcq.XAxes[0].Range.Minimum || cwGraphDataAcq.Cursors[0].XPosition >= cwGraphDataAcq.XAxes[0].Range.Maximum)
				cwGraphDataAcq.Cursors[0].XPosition = (cwGraphDataAcq.XAxes[0].Range.Minimum + cwGraphDataAcq.XAxes[0].Range.Maximum) / 2;
			cwGraphDataAcq.Cursors[0].Visible = true;
			if(PreviousCh == ChosenCh)
			{
				if (ChosenCh < 9) dummy = 9;
				if (ChosenCh == 9) dummy = 1;
				//if (ChosenCh == 0)
				//{
				//    ChosenCh = 1;
				//    PreviousCh = 1;
				//}
			}
			HighlightChannel(ChosenCh, dummy);
		}

		private void cwGraphDataAcq_DoubleClick(object sender, EventArgs e)
		{
			cwGraphDataAcq.Cursors[0].Visible = false;
		}

		private void graphRTClick_TraceThickness_Click(object sender, EventArgs e)
		{
			ChangeTracesThickess();
		}

		private void ChangeTracesThickess()
		{
			double thickness;
			thickness = Convert.ToDouble(graphRTClick_TraceThickness.Text);
			if (thickness > 5) thickness = 5;
			if (thickness < 0.5) thickness = 0.5;
			for (var plt = 0; plt < cwGraphDataAcq.Plots.Count; plt++)
			{
				cwGraphDataAcq.Plots[plt].LineWidth = (float)thickness;
			}
		}

		private void graphRTClick_TraceThickness_TextChanged(object sender, EventArgs e)
		{
			ChangeTracesThickess();
		}

		private void graphRTClick_ShowCursor_Click(object sender, EventArgs e)
		{
			TurnGraphCursorOn();
		}

		private void TurnGraphCursorOn()
		{
			cwGraphDataAcq.Cursors[0].Visible = true;

			// IF cursor is not between MIN & MAX range on PLOT...set so it is...
			double CursorXpos = cwGraphDataAcq.Cursors[0].XPosition;
			double RangeMin = cwGraphDataAcq.XAxes[0].Range.Minimum;
			double RangeMax = cwGraphDataAcq.XAxes[0].Range.Maximum;
			double RangeMid = (RangeMin + RangeMax) / 2;
			if ((CursorXpos < RangeMin) || (CursorXpos > RangeMax)) cwGraphDataAcq.Cursors[0].XPosition = RangeMid;
		}

		private void graphRTClick_HideCursor_Click(object sender, EventArgs e)
		{
			TurnGraphCursorOff();
		}

		private void TurnGraphCursorOff()
		{
			cwGraphDataAcq.Cursors[0].Visible = false;
			// NOTE: Even though this is just one line, it's done as a procedure so that all cursor changes are centralized.
		}

		private void graphRTClick_HideGridlines_Click(object sender, EventArgs e)
		{
			HideTheGridLines();
		}

		public void HideTheGridLines()
		{
			cwGraphDataAcq.XAxes[0].MajorDivisions.GridVisible = false;
			cwGraphDataAcq.YAxes[0].MajorDivisions.GridVisible = false;
		}

		private void graphRTClick_ShowGridlines_Click(object sender, EventArgs e)
		{
			ShowTheGridLines();
		}
		public void ShowTheGridLines()
		{
			cwGraphDataAcq.XAxes[0].MajorDivisions.GridVisible = true;
			cwGraphDataAcq.YAxes[0].MajorDivisions.GridVisible = true;  // don't use the grid lines on the second y-axis
		}

		private void xAxisSlider1MinuteWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetXAxisWindow(60.0);
		}

		public void SetXAxisWindow(double visible_sec)
		{
			// When we zoom out to full scale, turn off the cursor (user can turn it back on if they like)
			TurnGraphCursorOff();
			NationalInstruments.UI.Range XplotSRange = cwGraphDataAcq.XAxes[0].Range;
			var XplotStart = XplotSRange.Minimum;// waveformGraphRTM.XAxes[0].Position;
			var XplotEnd = XplotStart + visible_sec;
			// Set scaling to so it includes start point till start point + interval
			cwGraphDataAcq.XAxes[0].Mode = NationalInstruments.UI.AxisMode.Fixed;
			NationalInstruments.UI.Range r = new NationalInstruments.UI.Range(XplotStart, XplotEnd);
			cwGraphDataAcq.XAxes[0].Range = r;
		}

		private void xAxisSlider5MinuteWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetXAxisWindow(300.0);
		}

		private void xAxisSlider10MinuteWindowToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetXAxisWindow(600.0);
		}

		private string ADC_help(int value)
		{
			string TipText = "change channel to see signal description";
			switch(value)
			{
				case 0:
					TipText = "ADC0, Socket 'Isolator 1', pin 16";
					break;
				case 1:
					TipText = "ADC1, Socket 'Isolator 1', pin 17";
					break;
				case 2:
					TipText = "ADC2, Socket 'Isolator 1', pin 18";
					break;
				case 3:
					TipText = "ADC3, W4 jumper 1-2 (default): BNC 0 input; W4 jumper 2-3: Socket 'Isolator 1', pin 19";
					break;
				case 4:
					TipText = "ADC4, W5 jumper 1-2 (default): Socket 'Isolator 2', pin 16; W5 jumper 2-3: BNC 0 input";
					break;
				case 5:
					TipText = "ADC5, Socket 'Isolator 2', pin 17";
					break;
				case 6:
					TipText = "ADC6, Socket 'Isolator 2', pin 18";
					break;
				case 7:
					TipText = "ADC7, W6 jumper 1-2 (default): BNC 1 input; W6 jumper 2-3: Socket 'Isolator 2', pin 19";
					break;
				case 8:
					TipText = "ADC8, Socket 'Isolator 3', pin 16";
					break;
				case 9:
					TipText = "ADC9, Socket 'Isolator 3', pin 17";
					break;
				case 10:
					TipText = "ADC10, Socket 'Isolator 3', pin 18";
					break;
				case 11:
					TipText = "ADC11, Socket 'Isolator 3', pin 19";
					break;
				case 12:
					TipText = "ADC12, Socket 'Isolator 4', pin 16";
					break;
				case 13:
					TipText = "ADC13, Socket 'Isolator 4', pin 17";
					break;
				case 14:
					TipText = "ADC14, Socket 'Isolator 4', pin 18";
					break;
				case 15:
					TipText = "ADC15, W7 jumper 1-2 (default): Socket 'Isolator 4', pin 19; W7 jumper 2-3: BNC 1 input";
					break;
				case 16:
					TipText = "ADC16, Socket 'FeedForward+I/O', pin 2";
					break;
				case 17:
					TipText = "ADC17, Socket 'FeedForward+I/O', pin 5";
					break;
				case 18:
					TipText = "ADC18, Socket 'FeedForward+I/O', pin 8";
					break;
				case 19:
					TipText = "ADC19, Socket 'FeedForward+I/O', pin 11";
					break;
				case 20:
					TipText = "ADC20, internal pressure sensor 'Isolator 1'";
					break;
				case 21:
					TipText = "ADC21, internal pressure sensor 'Isolator 2'";
					break;
				case 22:
					TipText = "ADC22, internal pressure sensor 'Isolator 3'";
					break;
				case 23:
					TipText = "ADC23, internal pressure sensor 'Isolator 4'";
					break;
				case 24:
					TipText = "ADC24, internal pressure sensor 'Input Pressure'";
					break;
				case 25:
					TipText = "ADC25, internal Temperature sensor";
					break;
				case 26:
					TipText = "ADC26, 'Vert Prox Green Socket', pin 2 from the right";
					break;
				case 27:
					TipText = "ADC27, 'Vert Prox Green Socket', pin 4 from the right";
					break;
				case 28:
					TipText = "ADC28, 'Vert Prox Green Socket', pin 6 from the right";
					break;
				case 29:
					TipText = "ADC29, 'Horiz Prox Black Socket', pin 2(current) or 3(voltage) from the LEFT";
					break;
				case 30:
					TipText = "ADC30, 'Horiz Prox Black Socket', pin 6(current) or 7(voltage) from the LEFT";
					break;
				case 31:
					TipText = "ADC31, 'Horiz Prox Black Socket', pin 10(current) or 11(voltage) from the LEFT";
					break;

			}
			return TipText;
		}

		private void cwBNC1channel_ValueChanged(object sender, EventArgs e)
		{
			string cmd = string.Empty, resp = string.Empty;
			toolTip1.SetToolTip(cwBNC1channel, ADC_help(31 - Convert.ToInt32(cwBNC1channel.Value)));
			cmd = "bnc1=adc" + (31 - Convert.ToInt32(cwBNC1channel.Value));
			if(HandleToCallingForm != null) HandleToCallingForm.SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void cwBNC0channel_ValueChanged(object sender, EventArgs e)
		{
			string cmd = string.Empty, resp = string.Empty;
			toolTip1.SetToolTip(cwBNC0channel, ADC_help(31 - Convert.ToInt32(cwBNC0channel.Value)));
			cmd = "bnc0=adc" + (31 - Convert.ToInt32(cwBNC0channel.Value));
			if (HandleToCallingForm != null)  HandleToCallingForm.SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void cwSpreadPlotsByY_ValueChanged(object sender, EventArgs e)
		{
			recalculate_Rel_plot();
			UpdatePositionPlot(-1);
		}

		private void recalculate_Rel_plot()
		{
			int indx = 0;
			indx = ScanNumber;
			indx = line_counter;
			for(indx=0; indx< MAX_SCAN_ARRAY_SIZE; indx++)
			{
				Re_Calculate_Relative_Data_line(indx);
			}
		}

		private void Re_Calculate_Relative_Data_line(int indx)
		{
			int ch = 0;
			for(ch=0; ch < MAX_CH; ch++)
			{
				RelativeScanValues[ch, indx] = ScanValues[ch, indx] - ScanValues[ch, 0] + cwSpreadPlotsByY.Value * (5 - ch);
			}
		}

		private void cmdZoom2x_Click(object sender, EventArgs e)
		{
			//' 2x zoom - magnification
			YminScale = YminScale / 2;
			YmaxScale = YmaxScale / 2;
			cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(YminScale, YmaxScale);
		}

		private void cmdZoom_05x_Click(object sender, EventArgs e)
		{
			YminScale = YminScale * 2;
			YmaxScale = YmaxScale *2;
			cwGraphDataAcq.YAxes[0].Range = new NationalInstruments.UI.Range(YminScale, YmaxScale);
		}

		private void HidePlotAreaImage()
		{
			cwGraphDataAcq.PlotAreaImage = null;
			ShowTheGridLines();
		}

		private void NumEdit_UpButtonClicked(object sender, EventArgs e)
		{
			HandleToCallingForm.SetTOMaxValue((NumericEdit)sender, false);
		}

		private void NumEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Up)
				HandleToCallingForm.SetTOMaxValue((NumericEdit)sender, true);
		}

		private void Cmb_X_scale_SelectedIndexChanged(object sender, EventArgs e)
		{
			//XaxisUnits = Cmb_X_scale.SelectedIndex; //'column #;
			//if(XaxisUnits == 0) TextXunits.Text = " line #";
			//if(XaxisUnits == 1) TextXunits.Text = " sec";
			//if(XaxisUnits == 2) TextXunits.Text = " Inflation_Cnt";
			//if(XaxisUnits == 3) TextXunits.Text = " Ehxhaust_Cnt";
			//if(XaxisUnits == 4) TextXunits.Text = " Inflation_mA";
			//if(XaxisUnits == 5) TextXunits.Text = " Exhaust_mA";
			//slideXAxis.Caption = Cmb_X_scale.Items[XaxisUnits].ToString();
		}

		private void CmdShowAnalysis_Click(object sender, EventArgs e)
		{
			OpenValveTestResultsForm();
		}

		private void OpenValveTestResultsForm()
		{
			if (Program.FormValveTestResult == null)
			{
				Program.FormValveTestResult = new FrmValveTestResult();
			}
		   else
			{
				Program.FormValveTestResult.Show();
				Program.FormValveTestResult.Focus();
			}


		}

		double avg_slope = 0;
		double[] inflation_slope = new double[4];
		double[] Slope_Correction = new double[4];

		public void Calc_SlopeCorrection()
		{
			int ch = 0;
			double wanted_slope = 0;
			wanted_slope = Program.FormValveTestResult.CwStandardSlope.Value;
			avg_slope = 0;
			for (ch = 0; ch <= 3; ch++)
				avg_slope = avg_slope + inflation_slope[ch] * 0.25;

			for(ch=0; ch <= 3; ch++)
			{
				if (inflation_slope[ch] != -99 && inflation_slope[ch] != 0)
				{
					Slope_Correction[ch] = wanted_slope / inflation_slope[ch];
					Program.FormValveTestResult.lbl_SlopeCorrection[ch].Text = Slope_Correction[ch].ToString("0.00");
				}
				else
					Program.FormValveTestResult.lbl_SlopeCorrection[ch].Text = "N/A";
			}
		}

		double Input_pressure_at_valves_OFF = 0;
		double Input_pressure_at_valves_FULL_ON = 0;
		double[] deflation_slope = new double[4];
		double[] shutdown_pressure = new double[4];
		double[] Valve_opening_mA = new double[4];
		double[] fully_open_pressure = new double[4];
		public int Analyze_Valve_plot()
		{
			int returnVal = 0, error_code = 0;
			double[] time_at_40psi = new double[8];
			double[] time_at_20psi = new double[8];
			double[] time_down_at5psi = new double[4];
			double[] time_goingup_at5psi = new double[4];
			double[] mA_at_20psi = new double[8];
			double[] mA_at_40psi = new double[8];

			int[] index_at_40psi = new int[8];
			int[] index_at_20psi = new int[8];
			int[] index_down_at5psi = new int[4];
			int[] index_goingup_at5psi = new int[4];
			int[] index_mA_at_20psi = new int[8];
			int[] index_mA_at_40psi = new int[8];

			double pressure = 0;
			double mA = 0;

			double delta_t = 0;
			double time_correction = 0;
			double delta_p = 0;
			double delta_mA = 0;
			double mA_correction = 0;

			int ch = 0;
			int i = 0;
			int tmp_indx = 0;
			int min_pressure = 0;
			int min_pressure_indx = 0;
			int max_i = 0;

			error_code = (int)ValveTestResults.NO_ERROR;
			returnVal = error_code;

			max_i = plt_X_coord.GetUpperBound(0);
			if (max_i == 0) goto show_results;
			if(Program.FormValveTestResult != null) Program.FormValveTestResult.setDefaults();
			for(ch =0; ch < 4; ch++)
			{
				for (i = 0; i <= max_i; i++)
				{
					pressure = ScanValuesToPlot[ch, i];
					if(pressure > CwUpperPSIthreshold.Value)
					{
						time_at_40psi[ch] = ScanTimeStamps[i];
						time_at_40psi[ch + 4] = ScanTimeStamps[i + 1];
						index_at_40psi[ch] = i;
						index_at_40psi[ch + 4] = i + 1;
					}
					else
					{
						if(pressure > CwLowerPSIthreshold.Value)
						{
							time_at_20psi[ch] = ScanTimeStamps[i];
							time_at_20psi[ch + 4] = ScanTimeStamps[i + 1];
							index_at_20psi[ch] = i;
							index_at_20psi[ch + 4] = i + 1;
						}
						else
						{
							if (pressure > 5)
							{
								time_down_at5psi[ch] = ScanTimeStamps[i];
								index_down_at5psi[ch] = i;
							}
							else
								break;
						}
					}
				}
				if (index_at_20psi[ch] > index_down_at5psi[ch] || index_at_40psi[ch] > index_down_at5psi[ch])
				{
					error_code = (int)ValveTestResults.ERR_VALVES_WERE_NOT_COMPLITELY_SHUT;
					returnVal = error_code;
				}
				min_pressure = 100;
				for(tmp_indx = 0; tmp_indx <= max_i; tmp_indx++)
				{
					if(ScanValuesToPlot[ch, tmp_indx] < min_pressure)
					{
						min_pressure = Convert.ToInt32(ScanValuesToPlot[ch, tmp_indx]);
						min_pressure_indx = tmp_indx;
					}
				}
				for(i = min_pressure_indx; i < max_i; i++)
				{
					pressure = ScanValuesToPlot[ch, i];
					mA = plt_X_coord[i];
					if(pressure < 5)
					{
						time_goingup_at5psi[ch] = ScanTimeStamps[i]; //'time stamp will be updated until pressure increases above 5 psi
						index_goingup_at5psi[ch] = i;
					}
					else
					{
						if(pressure < CwLowerPSIthreshold.Value)
						{
							mA_at_20psi[ch] = mA; // 'mA will be updated until pressure increases above 20 psi, so the last recoded will be just below 20 mA
							mA_at_20psi[ch + 4] = plt_X_coord[i + 1]; //'mA assuming next pressure will be above 20 psi
							index_mA_at_20psi[ch] = i;
							index_mA_at_20psi[ch + 4] = i + 1;
						}
						else
						{
							if (pressure < CwUpperPSIthreshold.Value)
							{
								mA_at_40psi[ch] = mA; // 'mA will be updated until pressure increases above 40 psi, so the last recoded will be just below 40 mA
								mA_at_40psi[ch + 4] = plt_X_coord[i + 1];  // 'mA assuming mext pressure will be above 40 psi
								index_mA_at_40psi[ch] = i;
								index_mA_at_40psi[ch + 4] = i + 1;
							}
							else
								break;
						}
					}
				}
				if(index_goingup_at5psi[ch] < index_down_at5psi[ch] || index_at_40psi[ch] > index_down_at5psi[ch])
				{
					error_code = (int)ValveTestResults.ERR_VALVES_WERE_NOT_COMPLITELY_SHUT;
					returnVal = error_code;
				}
				if(index_mA_at_20psi[ch] >= max_i)
				{
					error_code = (int)ValveTestResults.ERR_INFL_PRESSURE_DID_NOT_CROSS_20;
					returnVal = error_code;
				}
				if(index_mA_at_40psi[ch] >= max_i)
				{
					error_code = (int)ValveTestResults.ERR_INFL_PRESSURE_DID_NOT_CROSS_40;
					returnVal = error_code;
				}
			}

			Input_pressure_at_valves_OFF = ScanValuesToPlot[4, 0];
			Program.FormValveTestResult.lbl_Inp_PSI_valves_OFF.Text = Input_pressure_at_valves_OFF.ToString("0.00");

			Input_pressure_at_valves_FULL_ON = ScanValuesToPlot[4, max_i];
			Program.FormValveTestResult.lbl_Inp_PSI_valves_FULL_ON.Text = Input_pressure_at_valves_FULL_ON.ToString("0.00");

			//now calculate more precise time moments and mA when pressure crosses test criteria 20 and 40 psi
			//update this delay first, because FrmValveTestResult uses it as criterion
			Program.FormValveTestResult.LblDelay.Text = CMDperiod.ToString("0.0");
			for (ch = 0; ch < 4; ch++)
			{
				if (index_at_40psi[ch + 4] <= max_i && (index_at_20psi[ch + 4] <= max_i))
				{
					delta_p = ScanValuesToPlot[ch, index_at_40psi[ch + 4]] - ScanValuesToPlot[ch, index_at_40psi[ch]];
					delta_t = time_at_40psi[ch + 4] - time_at_40psi[ch];
					time_correction = (CwUpperPSIthreshold.Value - ScanValuesToPlot[ch, index_at_40psi[ch]]) / delta_p; //'must be between zero and 1
					time_correction = double.IsInfinity(time_correction) ? 0 : time_correction;
					time_at_40psi[ch] = time_at_40psi[ch] + delta_t * time_correction;
					delta_p = ScanValuesToPlot[ch, index_at_20psi[ch + 4]] - ScanValuesToPlot[ch, index_at_20psi[ch]];
					delta_t = time_at_20psi[ch + 4] - time_at_20psi[ch];
					time_correction = (CwLowerPSIthreshold.Value - ScanValuesToPlot[ch, index_at_20psi[ch]]) / delta_p;// 'must be between zero and 1
					time_correction = double.IsInfinity(time_correction) ? 0 : time_correction;
					time_at_20psi[ch] = time_at_20psi[ch] + delta_t * time_correction;
					if (time_at_40psi[ch] - time_at_20psi[ch] != 0)
					{
						deflation_slope[ch] = (CwUpperPSIthreshold.Value - CwLowerPSIthreshold.Value) / (time_at_40psi[ch] - time_at_20psi[ch]); // ' in psi/sec
						Program.FormValveTestResult.lbl_DeflationSlope_PSI_per_sec[ch].Text = deflation_slope[ch].ToString("0.00");
					}
					else
					{
						deflation_slope[ch] = -99; // '-99 means error
						Program.FormValveTestResult.lbl_DeflationSlope_PSI_per_sec[ch].Text = "noTimeInfo";
					}
				}
				else
				{
					deflation_slope[ch] = -99; // '-99 means error
					Program.FormValveTestResult.lbl_DeflationSlope_PSI_per_sec[ch].Text = "--N/A--";
				}
				//shutdown_pressure is measured at middle index between pressure goes down below 5 psi and pressure goes up above 5 psi
				shutdown_pressure[ch] = ScanValuesToPlot[ch, (index_down_at5psi[ch] + index_goingup_at5psi[ch]) / 2];
				Program.FormValveTestResult.lbl_PSI_deflated[ch].Text = shutdown_pressure[ch].ToString("0.00");
				Valve_opening_mA[ch] = plt_X_coord[index_goingup_at5psi[ch]];
				Program.FormValveTestResult.lbl_Opening_Current[ch].Text = Valve_opening_mA[ch].ToString("00.00");
				delta_mA = mA_at_20psi[ch + 4] - mA_at_20psi[ch];
				delta_p = ScanValuesToPlot[ch, index_mA_at_20psi[ch + 4]] - ScanValuesToPlot[ch, index_mA_at_20psi[ch]];

				if (delta_p != 0)
				{
					mA_correction = (20 - ScanValuesToPlot[ch, index_mA_at_20psi[ch]]) / delta_p; // 'must be between zero and 1
					mA_correction = double.IsInfinity(mA_correction) ? 0 : mA_correction;
				}
				else
					mA_correction = 0;
				mA_at_20psi[ch] = mA_at_20psi[ch] + delta_mA * mA_correction;
				delta_mA = mA_at_40psi[ch + 4] - mA_at_40psi[ch];
				delta_p = ScanValuesToPlot[ch, index_mA_at_40psi[ch + 4]] - ScanValuesToPlot[ch, index_mA_at_40psi[ch]];

				mA_correction = (40 - ScanValuesToPlot[ch, index_mA_at_40psi[ch]]) / delta_p;  //'must be between zero and 1
				mA_correction = double.IsInfinity(mA_correction) ? 0 : mA_correction;

				mA_at_40psi[ch] = mA_at_40psi[ch] + delta_mA * mA_correction;


				if ((mA_at_40psi[ch] - mA_at_20psi[ch]) != 0 && (mA_at_40psi[ch] != 0) && (mA_at_20psi[ch] != 0))
				{
					inflation_slope[ch] = (CwUpperPSIthreshold.Value - CwLowerPSIthreshold.Value) / (mA_at_40psi[ch] - mA_at_20psi[ch]); //' in psi/mA
					Program.FormValveTestResult.lbl_InflationSlope_PSI_per_mA[ch].Text = inflation_slope[ch].ToString("0.00");
				}
				else
				{
					inflation_slope[ch] = -99;   //'means error
					Program.FormValveTestResult.lbl_InflationSlope_PSI_per_mA[ch].Text = "--N/A--";
				}
				fully_open_pressure[ch] = ScanValuesToPlot[ch, max_i];
				Program.FormValveTestResult.lbl_PSI_at_FULLY_OPEN[ch].Text = fully_open_pressure[ch].ToString("0.00");
			}

			Calc_SlopeCorrection();

show_results:
			OpenValveTestResultsForm();
			switch(error_code)
			{
				case 1:
					MessageBox.Show("Input pressure is " +ScanValues[4, 0].ToString("0.00") + " psi. It is recommended to" + "\r\n" + "increase input pressure > 80 psi (standard 90 psi)" + "\r\n" + "and repeat the test, continue?", "Input pressure < 80 psi", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 2:
					MessageBox.Show("While deflating, pressure did not cross " + CwUpperPSIthreshold.Text + " psi" + "\r\n" + "SOME RESULTS WERE NOT RECALCULATED!", "RESULTS MIGHT BE WRONG!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 3:
					MessageBox.Show("While deflating, pressure did not cross " + CwLowerPSIthreshold.Text + " psi" + "\r\n" + "SOME RESULTS WERE NOT RECALCULATED!", "RESULTS MIGHT BE WRONG!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 4:
					MessageBox.Show("While deflating, pressure did not cross 5 psi" + "\r\n" + "Valves were not complitely shut!" + "\r\n" + "Recommended to increase 'Interval, sec' " + "\r\n"+ "and / or decrease 'Start, mA' than repeat test", "RESULTS MIGHT BE WRONG!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 5:
					MessageBox.Show("While deflating, pressure did not cross 20 psi" + "\r\n" + "Maybe not enough input pressure" + "\r\n" + "Check pressure and tubing connections than repeat test", "RESULTS MIGHT BE WRONG!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
				case 6:
					MessageBox.Show("While deflating, pressure did not cross 40 psi" + "\r\n" + "Maybe not enough input pressure" + "\r\n" + "Check pressure and tubing connections than repeat test", "RESULTS MIGHT BE WRONG!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					break;
			}
			return returnVal;
		}


		System.Threading.Thread ValveTestThread;
		bool IsValveTestStarted = false;
		private void cwButValveTest_CheckedChanged(object sender, EventArgs e)
		{
			string command_Renamed = string.Empty;
			string capture_txt = string.Empty;
			int scan_line = 0;
			string resp = string.Empty;
			if(cwButValveTest.Checked)
			{
				cmb_TestType.Enabled = false;
				ScanValuesToPlot = new double[0, 0];
				ScanNumber = 0;
				command_Renamed = "getpa";
				Program.FormMain.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out command_Renamed, CommandAckTypes.AckExpected);
				scan_line = ExtractDataFromScanLine("0," + command_Renamed, 0);
				if(ScanValues[4,0] < 80)
				{
					DialogResult result = MessageBox.Show("Input pressure is " + ScanValues[4, 0].ToString("0.00") + " psi. It is recommended to" + "\r\n" + "increase input pressure > 80 psi (standard 90 psi)" + "\r\n" + "and repeat the test, continue?", "Input pressure < 80 psi", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
					if (result == DialogResult.No)
					{
						cwButValveTest.Checked = false;
						return;
					}
				}
				cmdReadPositionFile.Enabled = false;
				cwSaveScan.Enabled = false;
				prepare_for_plotting();
				stateButtonRelAbs.Checked = false;
				if (MonitoringTestType == (int)TestType.Valves_2020)
					command_Renamed = "vtst " + cwValveStartCurrent.Value + " " + cwValveIncrementCurrent.Value + " " + cwValveEndCurrent.Value + " " + cwValveMeasTime.Value + " " + cwValveHoldCurrent.Value + " " + CwLowerPSIthreshold.Value + " " + CwUpperPSIthreshold.Value;
				Program.FormMain.txtManualCMD.Text = command_Renamed;
			 //   ScanValuesToPlot = new double[0, 0];
				ValveTestThread = new System.Threading.Thread(() => GetPosScan(command_Renamed, cwValveMeasTime.Value + 0.1));
				IsValveTestStarted = true;
				ValveTestThread.Start();

			 //   ScanNumber = GetPosScan(command_Renamed, cwValveMeasTime.Value + 0.1);
				if (ScanNumber > 10)
					cwButValveTest.Checked = false;
				cwSaveScan.Visible = true;
			}
			else
			{
				IsValveTestStarted = false;
				cmb_TestType.Enabled = true;
				// if (ValveTestThread != null) {   ValveTestThread.Abort(); ValveTestThread.Abort(); ValveTestThread = null; }
				cmdReadPositionFile.Enabled = true;
				cwSaveScan.Enabled = true;
				command_Renamed = "st";
				Program.FormMain.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
			}
		}

		const string end_of_ScanString_token =  "%";


		private int GetPosScan(string cmd, double WaitingTime)
		{
			int returnVal = 0;
			if (IsValveTestStarted)
			{

				string temp_str = string.Empty;
				string scan_buf = string.Empty;
				string str_len = string.Empty;
				int seperator = 0;
				int percent_divider = 0;
				int lines_received = 0, ch = 0;
				string resp = string.Empty;

				PC_RealTime = false;
				Program.FormMain.Test_in_progress = true;
				line_counter = 0;
				if (Program.FormMain.lstResponse.InvokeRequired)
					Program.FormMain.lstResponse.BeginInvoke((MethodInvoker)delegate
					{
						Program.FormMain.lstResponse.Items.Clear();
					});
				Program.FormMain.Buffer = "";
				percent_divider = Convert.ToInt32(MAXmeasINDEX);
				if (WaitingTime == 0) WaitingTime = 0.3;
				Program.FormMain.SendInternal(cmd + "\r\n", CommandTypes.NoResponseExpected, out resp, CommandAckTypes.NoAckExpected);
				StartTime = Convert.ToSingle(DateAndTime.Timer);
				if (MonitoringTestType == (int)TestType.Valves_2020)
				{
					txtStatus.BeginInvoke((MethodInvoker)delegate { txtStatus.Text = "Testing valve board...."; });
					cwGraphDataAcq.Text = "Iso Pressure vs. Valve Current";
					StartTime = StartTime + 1.2f;
					CMDperiod = cwValveMeasTime.Value;
				}
				if (MonitoringTestType == (int)TestType.Monitor_Pressure)
				{
					cwGraphDataAcq.Text = "Pressure Readings";
					txtStatus.BeginInvoke((MethodInvoker)delegate
					{
						txtStatus.Text = "Getting pressure monitoring data from controller....";
					});
					StartTime = StartTime + 0.2f;
				}
				delayTime = Convert.ToSingle(DateAndTime.Timer - StartTime);
				while (delayTime < WaitingTime)
				{
					System.Windows.Forms.Application.DoEvents();
					delayTime = Convert.ToSingle(DateAndTime.Timer - StartTime);
				}
another_line:
				if (!IsValveTestStarted) return returnVal;
				scan_buf = Device.ReadLineWithTimeout(TimeSpan.FromSeconds(WaitingTime + 0.5));
				//scan_buf = resp;
				if (scan_buf == "UserStop")
				{
					returnVal = -1;
					Program.FormMain.Test_in_progress = false;
					return returnVal;
				}
				scan_buf = scan_buf.TrimStart();
				seperator = scan_buf.IndexOf(end_of_ScanString_token) + 1;
				if (seperator == 0)
				{
					if (scan_buf.Length > 0)
					{
						if (Program.FormMain.lstResponse.InvokeRequired) Program.FormMain.lstResponse.BeginInvoke((MethodInvoker)delegate { Program.FormMain.lstResponse.Items.Add(line_counter.ToString("000") + "->" + scan_buf); });
						else Program.FormMain.lstResponse.Items.Add(line_counter.ToString("000") + "->" + scan_buf);
						//if (File_Saving)
						//    file.WriteLine(scan_buf);
					}
					else
					{
						if (Program.FormMain.lstResponse.InvokeRequired) Program.FormMain.lstResponse.BeginInvoke((MethodInvoker)delegate { Program.FormMain.lstResponse.Items.Add("Empty string"); });
						else Program.FormMain.lstResponse.Items.Add("Empty string");
					}
					goto another_line;
				}
				else
				{
					ASCIIDataStr = scan_buf.Substring(0, seperator);
					seperator = ASCIIDataStr.IndexOf("\r") + 1;
					if (seperator != 0)
					{
						ASCIIDataStr = ASCIIDataStr.Substring(seperator);
					}
					if (ASCIIDataStr != "")
					{
						lines_received = ExtractDataFromScanLine(ASCIIDataStr);
						ScanTimeStamps[line_counter] = DateAndTime.Timer;
						if (File_Saving)
							write_one_line(line_counter);
					}
					if (Program.FormMain.lstResponse.InvokeRequired) Program.FormMain.lstResponse.Invoke((MethodInvoker)delegate { Program.FormMain.lstResponse.Items.Add(lines_received.ToString("000") + "->" + ASCIIDataStr); });
					else Program.FormMain.lstResponse.Items.Add(lines_received.ToString("000") + "->" + ASCIIDataStr);
				}
				if (scan_buf.IndexOf("OK") + 1 != 0 || ASCIIDataStr == "")
					goto show_plot;

				if (ASCIIDataStr == "Timeout")
				{
					if (Program.FormMain.lstResponse.InvokeRequired) Program.FormMain.lstResponse.Invoke((MethodInvoker)delegate { Program.FormMain.lstResponse.Items.Add("Timeout"); });
					else Program.FormMain.lstResponse.Items.Add("Timeout");
					MessageBox.Show("No communication over serial interface. Check connections");
					Program.FormMain.Test_in_progress = false;
					returnVal = 0;
					return returnVal;
				}

				if (ASCIIDataStr.Length == 0)
				{
					MessageBox.Show("No communication over serial interface. Check connections");
					goto show_plot;
				}

				if (ASCIIDataStr.Length > 50)
				{
					temp_str = ASCIIDataStr;
					if (line_counter == 30)
						Status = 0;
				}
				else
					goto another_line;
				UpdatePositionPlot(line_counter);
				line_counter = line_counter + 1;
				ScanNumber = line_counter;
				lbLineNum.BeginInvoke((MethodInvoker)delegate { lbLineNum.Text = line_counter.ToString(); });
				lbLineNum.BeginInvoke((MethodInvoker)delegate { lbLineNum.Update(); });
				Program.FormMain.lstResponse.BeginInvoke((MethodInvoker)delegate { Program.FormMain.lstResponse.Update(); });
				goto another_line;

				show_plot:
				if (File_Saving)
					cwSaveScan.Checked = false;

				if (line_counter < 0)
					return returnVal;
				if (lines_received < 0)
					return returnVal;
				if (lines_received > 0 || line_counter > 0)
				{
					ScanNumber = line_counter;
					UpdatePositionPlot(-1);
					if (MonitoringTestType == (int)TestType.Valves_2020)
						Analyze_Valve_plot();
					else
					{
						for (ch = 0; ch < MAX_CH; ch++)
						{
							tmp_array[line_counter] = ScanValues[ch, line_counter];
						}
						RefreshNumDisplay(tmp_array, 1, MinIndex[ch], MinValue[ch], MaxIndex[ch], MaxValue[ch], 0, 0, 0, 0, ch);
						lbPosition[ch].Text = ScanAverage[ch].ToString("00000");
						Program.St_Dev(ref ScanAverage,-1, false);   //-!- IK 20211230 CHECK
					}
				}

				Program.FormMain.Test_in_progress = false;
				Program.FormMain.ProgressTank.BeginInvoke((MethodInvoker)delegate
				{
					Program.FormMain.ProgressTank.Value = 100;
					Program.FormMain.ProgressTank.Caption = "! DONE !";
				});

				if (lines_received > line_counter)
					returnVal = lines_received;
				else
					returnVal = line_counter;


				return returnVal;
			}
			return returnVal;
		}

		private void cmdGetRecordFromBuffer_Click(object sender, EventArgs e)
		{
			string command_renamed = string.Empty;
			string capture_txt = string.Empty;
			command_renamed = "g";
			cmdGetRecordFromBuffer.Enabled = false;
			capture_txt = cmdGetRecordFromBuffer.Text;
			cmdGetRecordFromBuffer.Text = "Wait for " + MAXmeasINDEX + " lines";
			if (FormTerminalVisible)
				Program.FormMain.OpenTerminalForm();
			ScanNumber = GetPosScan(command_renamed, 0.2);
			cmdGetRecordFromBuffer.Text = capture_txt;
			cmdGetRecordFromBuffer.Enabled = true;
		}

		private void txtVsn_TextChanged(object sender, EventArgs e)
		{
			if (FormLoading || Program.IsReadingControllerSettings) return;

			TextBox txtSN = (TextBox)sender;
			int index = 0; double tryParVal = 0;
			if (txtSN.Name.Equals("txtV1sn")) index = 0;
			else if (txtSN.Name.Equals("txtV2sn")) index = 1;
			else if (txtSN.Name.Equals("txtV3sn")) index = 2;
			else index = 3;
			if (txtSN.Text == "")
			{
				txtSN.BackColor = Color.White;
				Program.FormValveTestResult.Lbl_Vsn[index].Text = "N/A";
			}
			else
			{
				if (double.TryParse(txtSN.Text, out tryParVal))
				{
					txtSN.BackColor = Color.White;
					Program.FormValveTestResult.Lbl_Vsn[index].Text = txtSN.Text;
				}
				else
					txtSN.BackColor = Color.Yellow;
			}
		}
		private void cwValveMeasTime_ValueChanged(object sender, EventArgs e)
		{
			CMDperiod = cwValveMeasTime.Value;
		}
	}

}
