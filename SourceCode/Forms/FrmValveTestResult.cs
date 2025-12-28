using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TMCAnalyzer
{
	public partial class FrmValveTestResult : Form
	{
		#region fields
		const string NAString = "-NA-";
		bool criteria_loaded = false;
		bool IsFormLoading = true;
		double CriteriaMAXinputPSI = 0;
		double CriteriaMINinputPSI = 0;
		double CriteriaMAXdeflationSLOPE = 0;
		double CriteriaMINdeflationSLOPE = 0;
		double CriteriaMAXremainingPressure = 0;
		double CriteriaMAXopeningCurrent = 0;
		double CriteriaMINopeningCurrent = 0;
		double CriteriaMAXinflationSLOPE = 0;
		double CriteriaMINinflationSLOPE = 0;
		double CriteriaMAXvalveOPENpsi = 0;
		double CriteriaMINvalveOPENpsi = 0;
		const int Criteria_file = 4;
		int ParamLINEScount = 0;

		public List<Label> lbl_DeflationSlope_PSI_per_sec = new List<Label>();
		public List<Label> lbl_PSI_deflated = new List<Label>();
		public List<Label> lbl_Opening_Current = new List<Label>();
		public List<Label> lbl_InflationSlope_PSI_per_mA = new List<Label>();
		public List<Label> lbl_PSI_at_FULLY_OPEN = new List<Label>();
		public List<Label> lbl_SlopeCorrection = new List<Label>();
		public List<Label> Lbl_Vsn = new List<Label>();
		#endregion

		public FrmValveTestResult()
		{
			IsFormLoading = true;
			InitializeComponent();
			InitializeControls();
			CriteriaMAXinputPSI = cwInpPsiMAX.Value;
			CriteriaMINinputPSI = cwInpPsiMIN.Value;
			CriteriaMAXdeflationSLOPE = cwDeflRateMAX.Value;
			CriteriaMINdeflationSLOPE = cwDeflRateMIN.Value;
			CriteriaMAXremainingPressure = cwPSIvalveOffMAX.Value;
			CriteriaMAXopeningCurrent = cwValveOpeningMAXmA.Value;
			CriteriaMINopeningCurrent = cwValveOpeningMINmA.Value;
			CriteriaMAXinflationSLOPE = cwInflationRateMAX.Value;
			CriteriaMINinflationSLOPE = cwInflationRateMIN.Value;
			CriteriaMAXvalveOPENpsi = cwPSIvalveFullyOpenMAX.Value;
			CriteriaMINvalveOPENpsi = cwPSIvalveFullyOpenMIN.Value;
			setDefaults();
			criteria_loaded = true;
			IsFormLoading = false;

		}

		private void InitializeControls()
		{
			lbl_DeflationSlope_PSI_per_sec.Clear();
			lbl_DeflationSlope_PSI_per_sec.Add(_lbl_DeflationSlope_PSI_per_sec_0);
			lbl_DeflationSlope_PSI_per_sec.Add(_lbl_DeflationSlope_PSI_per_sec_1);
			lbl_DeflationSlope_PSI_per_sec.Add(_lbl_DeflationSlope_PSI_per_sec_2);
			lbl_DeflationSlope_PSI_per_sec.Add(_lbl_DeflationSlope_PSI_per_sec_3);

			lbl_PSI_deflated.Clear();
			lbl_PSI_deflated.Add(_lbl_PSI_deflated_0);
			lbl_PSI_deflated.Add(_lbl_PSI_deflated_1);
			lbl_PSI_deflated.Add(_lbl_PSI_deflated_2);
			lbl_PSI_deflated.Add(_lbl_PSI_deflated_3);

			lbl_Opening_Current.Clear();
			lbl_Opening_Current.Add(_lbl_Opening_Current_0);
			lbl_Opening_Current.Add(_lbl_Opening_Current_1);
			lbl_Opening_Current.Add(_lbl_Opening_Current_2);
			lbl_Opening_Current.Add(_lbl_Opening_Current_3);

			lbl_InflationSlope_PSI_per_mA.Clear();
			lbl_InflationSlope_PSI_per_mA.Add(_lbl_InflationSlope_PSI_per_mA_0);
			lbl_InflationSlope_PSI_per_mA.Add(_lbl_InflationSlope_PSI_per_mA_1);
			lbl_InflationSlope_PSI_per_mA.Add(_lbl_InflationSlope_PSI_per_mA_2);
			lbl_InflationSlope_PSI_per_mA.Add(_lbl_InflationSlope_PSI_per_mA_3);

			lbl_PSI_at_FULLY_OPEN.Clear();
			lbl_PSI_at_FULLY_OPEN.Add(_lbl_PSI_at_FULLY_OPEN_0);
			lbl_PSI_at_FULLY_OPEN.Add(_lbl_PSI_at_FULLY_OPEN_1);
			lbl_PSI_at_FULLY_OPEN.Add(_lbl_PSI_at_FULLY_OPEN_2);
			lbl_PSI_at_FULLY_OPEN.Add(_lbl_PSI_at_FULLY_OPEN_3);

			lbl_SlopeCorrection.Clear();
			lbl_SlopeCorrection.Add(_lbl_SlopeCorrection_0);
			lbl_SlopeCorrection.Add(_lbl_SlopeCorrection_1);
			lbl_SlopeCorrection.Add(_lbl_SlopeCorrection_2);
			lbl_SlopeCorrection.Add(_lbl_SlopeCorrection_3);

			Lbl_Vsn.Clear();
			Lbl_Vsn.Add(Lbl_V1sn); Lbl_Vsn.Add(Lbl_V2sn);
			Lbl_Vsn.Add(Lbl_V3sn); Lbl_Vsn.Add(Lbl_V4sn);
		}

		private void FrmValveTestResult_Load(object sender, EventArgs e)
		{

		}


		public void setDefaults()
		{
			int indx = 0;
			lbl_Inp_PSI_valves_OFF.Text = NAString;
			lbl_Inp_PSI_valves_FULL_ON.Text = NAString;
			LblDelay.Text = NAString;
			for (indx = 0; indx <= 3; indx++ )
			{
				lbl_DeflationSlope_PSI_per_sec[indx].Text = NAString;
				lbl_PSI_deflated[indx].Text = NAString;
				lbl_Opening_Current[indx].Text = NAString;
				lbl_InflationSlope_PSI_per_mA[indx].Text = NAString;
				lbl_PSI_at_FULLY_OPEN[indx].Text = NAString;
			}
		}

		private void cwInpPsiMIN_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMINinputPSI = cwInpPsiMIN.Value;
			lbl_MinInpPSI_ValvesClosed.Text = CriteriaMINinputPSI.ToString();
		}

		private void cwInpPsiMAX_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMAXinputPSI = cwInpPsiMAX.Value;
			lbl_MaxInpPSI_ValvesClosed.Text = CriteriaMAXinputPSI.ToString();
		}

		private void cwDeflRateMAX_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMAXdeflationSLOPE = cwDeflRateMAX.Value;
			lbl_minDeflSlope.Text = CriteriaMAXdeflationSLOPE.ToString("0.00");
		}

		private void cwDeflRateMIN_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMINdeflationSLOPE = cwDeflRateMIN.Value;
			lbl_maxDeflSlope.Text = CriteriaMINdeflationSLOPE.ToString("0.00");
		}

		private void cwPSIvalveOffMAX_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMAXremainingPressure = cwPSIvalveOffMAX.Value;
			lbl_maxPSI_off.Text = CriteriaMAXremainingPressure.ToString("0.0");
		}

		private void cwValveOpeningMAXmA_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMAXopeningCurrent = cwValveOpeningMAXmA.Value;
			lbl_maxOpening_mA.Text = CriteriaMAXopeningCurrent.ToString("0.0");
		}

		private void cwValveOpeningMINmA_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMINopeningCurrent = cwValveOpeningMINmA.Value;
			lbl_minOpening_mA.Text = CriteriaMINopeningCurrent.ToString("0.0");
		}

		private void cwInflationRateMAX_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMAXinflationSLOPE = cwInflationRateMAX.Value;
			lbl_maxInflSlope.Text = CriteriaMAXinflationSLOPE.ToString("0.0");
		}

		private void cwInflationRateMIN_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			CriteriaMINinflationSLOPE = cwInflationRateMIN.Value;
			lbl_minInflSlope.Text = CriteriaMINinflationSLOPE.ToString("0.0");
		}

		private void cwPSIvalveFullyOpenMAX_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			double tryVal = 0, new_val = 0;
			if(double.TryParse(lbl_Inp_PSI_valves_FULL_ON.Text, out tryVal))
			{
				new_val = tryVal + cwPSIvalveFullyOpenMIN.Value;
				lbl_maxPSI_at_FULLY_OPEN.Text = new_val.ToString("0.00");
			}
		}

		private void cmdReadCriteria_Click(object sender, EventArgs e)
		{
			ReadCriteriaFile();
		}

		private void ReadCriteriaFile()
		{
			//bool cont_field_monitoring = false;
			//int line_error_number = 0;
			string DefaultFilename = string.Empty;
			string DataFileName = string.Empty;
			string FileName = string.Empty;
			int i = 0;
			string temp_str = string.Empty;
			string val_str = string.Empty;
			int sep_pos = 0;
			int line_read = 0;
			string line_NO_str = string.Empty;
			string search_str = string.Empty;
			double tryVal = 0;


			DefaultFilename = "DC-2020 valve test criteria " + DateTime.Now.ToString("MM-dd-yyyy_HH - mm") + ".criteria";
			openFileDialog1.Title = "Choose name for DC-2020 parameters to load";
			openFileDialog1.DefaultExt = "criteria";
			openFileDialog1.Filter = "Data File (*.criteria)|*.criteria|All files (*.*)|*.*";
			openFileDialog1.FileName = "";
		   // openFileDialog1.ShowDialog();
			if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
				return;
			else
				lblMsg.Text = "Reading Saved Criteria file '" + openFileDialog1.FileName + " .....";

			DataFileName = openFileDialog1.FileName;
			i = DataFileName.LastIndexOf("\\");
			if (i != -1) openFileDialog1.InitialDirectory = openFileDialog1.FileName.Substring(0, i);
			DataFileName = openFileDialog1.FileName;

			try
			{
				var /*stream*/ file = new System.IO.StreamReader(openFileDialog1.FileName);
				line_read = 0;
				temp_str = file.ReadLine();  //'Print #Criteria_file, "File name, "; FileName
				sep_pos = temp_str.IndexOf(",");
				if ((temp_str.ToLower().IndexOf("file name") + 1) == 0)
				{
					MessageBox.Show("Unrecognizable file format");
					return;
				}

				temp_str = file.ReadLine();  //Print #Criteria_file, "Date, "; Date$
				temp_str = file.ReadLine();  //Print #Criteria_file, "Time, "; Time$

				read_again:
				while (!file.EndOfStream)
				{
					temp_str = file.ReadLine();  // read settings line
					line_read = line_read + 1;

					sep_pos = temp_str.IndexOf("////") + 1;
					if (sep_pos != 0)
						goto read_again;
					sep_pos = temp_str.IndexOf("//") + 1;
					if (sep_pos != 0)
						temp_str = temp_str.Substring(0, sep_pos - 1);
					if (temp_str.Length > 0)  //it was criteria with comment, parse
					{
						search_str = "vtst_start_curr";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.cwValveStartCurrent.Value = tryVal;
						}
						search_str = "vtst_increment";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.cwValveIncrementCurrent.Value = tryVal;
						}
						search_str = "vtst_end_curr";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.cwValveEndCurrent.Value = tryVal;
						}
						search_str = "vtst_step_sec";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.cwValveMeasTime.Value = tryVal;
						}
						search_str = "vtst_hold_curr";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.cwValveHoldCurrent.Value = tryVal;
						}
						search_str = "max_input_psi";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwInpPsiMAX.Value = tryVal;
						}
						search_str = "min_input_psi";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwInpPsiMIN.Value = tryVal;
						}
						search_str = "max_defl_slope";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwDeflRateMAX.Value = tryVal;
						}
						search_str = "min_defl_slope";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwDeflRateMIN.Value = tryVal;
						}
						search_str = "max_closed_psi";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwPSIvalveOffMAX.Value = tryVal;
						}
						search_str = "max_opening_ma";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwValveOpeningMAXmA.Value = tryVal;
						}
						search_str = "min_opening_ma";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwValveOpeningMINmA.Value = tryVal;
						}
						search_str = "max_infl_slope";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwInflationRateMAX.Value = tryVal;
						}
						search_str = "min_infl_slope";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwInflationRateMIN.Value = tryVal;
						}
						search_str = "max_open_psi";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwPSIvalveFullyOpenMAX.Value = tryVal;
						}
						search_str = "min_open_psi";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								cwPSIvalveFullyOpenMIN.Value = tryVal;
						}
						search_str = "slope_high_curr";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.CwUpperPSIthreshold.Value = tryVal;
						}
						search_str = "slope_low_curr";
						sep_pos = temp_str.IndexOf(search_str) + 1;
						if (sep_pos != 0)
						{
							val_str = temp_str.Substring(search_str.Length + sep_pos);
							if (double.TryParse(val_str, out tryVal))
								Program.FormRTMonitoring.CwLowerPSIthreshold.Value = tryVal;
						}
					}
				}
				file.Close();
				criteria_loaded = true;
				Program.FormRTMonitoring.Analyze_Valve_plot();

				file.Close();
			}
			catch (System.IO.FileNotFoundException ex)
			{
				MessageBox.Show("File not found");
				return;
			}
		}

		private void cmdSaveCriteria_Click(object sender, EventArgs e)
		{
			SaveCriteriaFile();
		}

		private void SaveCriteriaFile()
		{
			string DefaultFileName = string.Empty;
			string FileName = string.Empty;
			string SaveFileName = string.Empty;
			int i = 0;
			//int colon_pos = 0;
			string param_line = string.Empty;

			DefaultFileName = "DC-2020 valve test criteria " + DateTime.Now.ToString("MM-dd-yyyy_HH-mm") + ".criteria";
			saveFileDialog1.Title = "Choose name for DC-2020 valve test criteria";
			saveFileDialog1.DefaultExt = "criteria";
			saveFileDialog1.Filter = "Data File (*.criteria)|*.criteria|All files (*.*)|*.*";
			saveFileDialog1.FileName = DefaultFileName;
			if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
				return;
			else
			{
				openFileDialog1.FileName = saveFileDialog1.FileName;
				lblMsg.Text = "File=" + openFileDialog1.FileName;
			}

			SaveFileName = openFileDialog1.FileName;
			i = openFileDialog1.FileName.LastIndexOf("\\") + 1;
			FileName = SaveFileName.Substring(i);
			SaveFileName = openFileDialog1.FileName;
			i = openFileDialog1.FileName.LastIndexOf("\\");
			if (i != -1) openFileDialog1.InitialDirectory = saveFileDialog1.InitialDirectory =openFileDialog1.FileName.Substring(0, i - 1);

			var /*stream*/ file = new System.IO.StreamWriter(SaveFileName);

			file.WriteLine("//// File name, " + FileName);
			file.WriteLine("//// DC-2020 valve test criteria");
			file.WriteLine("//// Date, " + DateTime.Now.ToString("MM-dd-yyyy_HH"));
			file.WriteLine("//// Time, " + DateTime.Now.ToString("HH-mm") + ", ");

			file.WriteLine("////'vtst' command parameters");
			file.WriteLine("vtst_start_curr" + "\t" + Program.FormRTMonitoring.cwValveStartCurrent.Value.ToString() + "\t" + "// mA");
			file.WriteLine("vtst_increment" + "\t" + Program.FormRTMonitoring.cwValveIncrementCurrent.Value.ToString() + "\t" + "// mA");
			file.WriteLine("vtst_end_curr" + "\t" + Program.FormRTMonitoring.cwValveEndCurrent.Value.ToString() + "\t" + "// mA");
			file.WriteLine("vtst_step_sec" + "\t" + Program.FormRTMonitoring.cwValveMeasTime.Value.ToString() + "\t" + "// sec");
			 file.WriteLine("vtst_hold_curr" + "\t" + Program.FormRTMonitoring.cwValveHoldCurrent.Value.ToString() + "\t" + "// mA");
			file.WriteLine("slope_high_curr" + "\t" + Program.FormRTMonitoring.CwUpperPSIthreshold.Value.ToString() + "// mA slope measurement upper threshold current");
			file.WriteLine("slope_low_curr" + "\t" + Program.FormRTMonitoring.CwLowerPSIthreshold.Value.ToString() + "// mA slope measurement upper threshold current");

			file.WriteLine("//// MAX & MIN input pressure PSI affect 'opening Current','remaining pressure','inflation SLOPE','valve OPEN PSI'");
			file.WriteLine("max_input_psi" + "\t" + CriteriaMAXinputPSI + "\t" + "// PSI   " + "\t" + "MAX input pressure");
			file.WriteLine("min_input_psi" + "\t" + CriteriaMINinputPSI + "\t" + "// PSI   " + "\t" + "MIN input pressure");
			file.WriteLine("max_defl_slope" + "\t" + CriteriaMAXdeflationSLOPE + "\t" + "// PSI/sec" + "\t" + "MAX deflation SLOPE");
			file.WriteLine("min_defl_slope" + "\t" + CriteriaMINdeflationSLOPE + "\t" + "// PSI   " + "\t" + "MIN deflation SLOPE");
			file.WriteLine("max_closed_psi" + "\t" + CriteriaMAXremainingPressure + "\t" + "// PSI   " + "\t" + "Valve closed MAX remaining pressure");
			file.WriteLine("max_opening_ma" + "\t" + CriteriaMAXopeningCurrent + "\t" + "// PSI   " + "\t" + "MAX opening current");
			file.WriteLine("min_opening_ma" + "\t" + CriteriaMINopeningCurrent + "\t" + "// PSI   " + "\t" + "MIN opening current");
			file.WriteLine("max_infl_slope" + "\t" + CriteriaMAXinflationSLOPE + "\t" + "// PSI   " + "\t" + "MAX inflation SLOPE");
			file.WriteLine("min_infl_slope" + "\t" + CriteriaMINinflationSLOPE + "\t" + "// PSI   " + "\t" + "MIN inflation SLOPE");
			file.WriteLine("max_open_psi" + "\t" + CriteriaMAXvalveOPENpsi + "\t" + "// PSI   " + "\t" + "Valve fully OPEN MAX pressure");
			file.WriteLine("min_open_psi" + "\t" + CriteriaMINvalveOPENpsi + "\t" + "// PSI   " + "\t" + "Valve fully OPEN MIN pressure");

			file.Close();
		}

		private void lbl_Inp_PSI_valves_OFF_TextChanged(object sender, EventArgs e)
		{
			string captn = string.Empty;
			double tryParVal = 0;
			double inp_pressure = 0;
			captn = lbl_Inp_PSI_valves_OFF.Text;
			if (captn.Equals(NAString))
				lbl_Inp_PSI_valves_OFF.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal))
				{
					inp_pressure = tryParVal;
					lbl_MaxInpPSI_ValvesClosed.Text = cwInpPsiMAX.Value.ToString();
					lbl_MinInpPSI_ValvesClosed.Text = cwInpPsiMIN.Value.ToString();
					if (inp_pressure > cwInpPsiMIN.Value && inp_pressure < cwInpPsiMAX.Value)
						lbl_Inp_PSI_valves_OFF.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_Inp_PSI_valves_OFF.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");

				}
			}
		}

		private void lbl_DeflationSlope_PSI_per_sec_Change(object sender, EventArgs e)
		{
			Label lbl_DeflationSlope_PSI_per_sec = (Label)sender;
			string captn = string.Empty;
			double tryParVal = 0, slope = 0;
			captn = lbl_DeflationSlope_PSI_per_sec.Text;
			if (captn.Equals(NAString) || captn.Equals("noTimeInfo"))
				lbl_DeflationSlope_PSI_per_sec.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal))
				{
					slope = tryParVal;
					lbl_minDeflSlope.Text = cwDeflRateMIN.Value.ToString("0.00");
					lbl_maxDeflSlope.Text = cwDeflRateMAX.Value.ToString("0.00");
					if (slope < cwDeflRateMIN.Value && slope > cwDeflRateMAX.Value)
						lbl_DeflationSlope_PSI_per_sec.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_DeflationSlope_PSI_per_sec.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");

				}
			}
		}

		private void lbl_PSI_deflated_Change(object sender, EventArgs e)
		{
			Label lbl_PSI_deflated = (Label)sender;
			string captn = string.Empty;
			double tryParVal = 0, min_pressure = 0;
			captn = lbl_PSI_deflated.Text;
			if (captn.Equals(NAString) )
				lbl_PSI_deflated.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal))
				{
					min_pressure = tryParVal;
					lbl_minPSI_off.Text = "0.0";
					lbl_maxPSI_off.Text = cwPSIvalveOffMAX.Value.ToString("0.0");
					if (min_pressure < cwPSIvalveOffMAX.Value )
						lbl_PSI_deflated.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_PSI_deflated.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");

				}
			}
		}

		private void lbl_Opening_Current_Change(object sender, EventArgs e)
		{
			Label lbl_Opening_Current = (Label)sender;
			string captn = string.Empty;
			double tryParVal = 0, opening_mA = 0;
			captn = lbl_Opening_Current.Text;
			if (captn.Equals(NAString))
				lbl_Opening_Current.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal))
				{
					opening_mA = tryParVal;
					lbl_minOpening_mA.Text = cwValveOpeningMINmA.Value.ToString("0.0");
					lbl_maxOpening_mA.Text = cwValveOpeningMAXmA.Value.ToString("0.0");
					if (opening_mA > cwValveOpeningMINmA.Value && opening_mA < cwValveOpeningMAXmA.Value)
						lbl_Opening_Current.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_Opening_Current.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");

				}
			}
		}


		private void lbl_InflationSlope_PSI_per_mA_Change(object sender, EventArgs e)
		{
			Label lbl_InflationSlope_PSI_per_mA = (Label)sender;
			string captn = string.Empty;
			double tryParVal = 0, slope = 0;
			captn = lbl_InflationSlope_PSI_per_mA.Text;
			if (captn.Equals(NAString))
				lbl_InflationSlope_PSI_per_mA.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal))
				{
					slope = tryParVal;
					lbl_minInflSlope.Text = cwInflationRateMIN.Value.ToString("0.0");
					lbl_maxInflSlope.Text = cwInflationRateMAX.Value.ToString("0.0");
					if (slope > cwInflationRateMIN.Value && slope < cwInflationRateMAX.Value)
						lbl_InflationSlope_PSI_per_mA.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_InflationSlope_PSI_per_mA.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");

				}
			}
		}

		private void lbl_PSI_at_FULLY_OPEN_Change(object sender, EventArgs e)
		{
			Label lbl_PSI_at_FULLY_OPEN = (Label)sender;
			string captn = string.Empty;
			double tryParVal = 0, tryParVal1 =0, ValveFullyOpenPSI = 0, calculated_InPSI=0, min_psi=0, max_psi=0;
			captn = lbl_PSI_at_FULLY_OPEN.Text;
			if (captn.Equals(NAString))
				lbl_PSI_at_FULLY_OPEN.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal) && double.TryParse(lbl_Inp_PSI_valves_FULL_ON.Text, out tryParVal1) )
				{
					ValveFullyOpenPSI = tryParVal;
					calculated_InPSI = tryParVal1;
					min_psi = calculated_InPSI + cwPSIvalveFullyOpenMAX.Value;
					max_psi = calculated_InPSI + cwPSIvalveFullyOpenMIN.Value;
					lbl_minPSI_at_FULLY_OPEN.Text = min_psi.ToString("0.00");
					lbl_maxPSI_at_FULLY_OPEN.Text = max_psi.ToString("0.00");
					if (ValveFullyOpenPSI > min_psi && ValveFullyOpenPSI < max_psi)
						lbl_PSI_at_FULLY_OPEN.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_PSI_at_FULLY_OPEN.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");

				}
			}
		}

		private void lbl_Inp_PSI_valves_FULL_ON_Change(object sender, EventArgs e)
		{
			Label lbl_Inp_PSI_valves_FULL_ON = (Label)sender;
			string captn = string.Empty;
			double tryParVal = 0, tryParVal1 = 0, pressure = 0, min_pressure = 0, in_pressure = 0;
			captn = lbl_Inp_PSI_valves_FULL_ON.Text;
			if (captn.Equals(NAString))
				lbl_Inp_PSI_valves_FULL_ON.BackColor = ColorTranslator.FromHtml("#c0ffff");
			else
			{
				if (double.TryParse(captn, out tryParVal) && double.TryParse(lbl_Inp_PSI_valves_OFF.Text, out tryParVal1))
				{
					pressure = tryParVal;
					in_pressure = tryParVal1;
					min_pressure = in_pressure - 10;
					lbl_MinInpPSI_at_FullyOpen.Text = min_pressure.ToString("0.00");
					lbl_MaxInpPSI_at_FullyOpen.Text = in_pressure.ToString("0.00");
					if (pressure > min_pressure)
						lbl_Inp_PSI_valves_FULL_ON.BackColor = System.Drawing.ColorTranslator.FromHtml("#80FF80");
					else
						lbl_Inp_PSI_valves_FULL_ON.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFFC0");
				}
			}
		}

		private void CwStandardSlope_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e)
		{
			if (IsFormLoading) return;
			Program.FormRTMonitoring.Calc_SlopeCorrection();
		}

		private void CmdRecalculate_Click(object sender, EventArgs e)
		{
			Program.FormRTMonitoring.Analyze_Valve_plot();
		}

		private void FrmValveTestResult_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.ApplicationExitCall)
			{
				Program.FormValveTestResult.Hide();
				e.Cancel = true;
			}
		}
	}
}
