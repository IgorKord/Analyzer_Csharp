using System;
using System.Collections.Generic;
using NationalInstruments.UI.WindowsForms;
using System.Windows.Forms;
using TMCAnalyzer.CustomClasses;
namespace TMCAnalyzer {
	public partial class frmFloorFF:Form {
		public frmFloorFF() {
			InitializeComponent();
		}
		private bool FormLoading = true;
		private formMain HandleToCallingForm = null;
		internal bool FormTerminalVisible = false;
		const double WRONG_VALUE = 77777.7;

		List<NumericEdit> axisFBgain = new List<NumericEdit>();
		List<NumericEdit> numericFFgain = new List<NumericEdit>();
		List<ScientificNumericUpDown> numUD_axisFBgain = new List<ScientificNumericUpDown>();
		List<ScientificNumericUpDown> numUD_FFgain = new List<ScientificNumericUpDown>();
		List<StateButton> sbWorking = new List<StateButton>();
		List<StateButton> sbAdaptive = new List<StateButton>();
		List<StateButton> cbAxisEn = new List<StateButton>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="handleToMainForm"></param>
		public frmFloorFF(formMain handleToMainForm) {
			FormLoading = true;
			InitializeComponent();
			HandleToCallingForm = handleToMainForm;
			Initialize_Controls();
			FormLoading = false;
		}

		/// <summary>
		/// Initialize the variables and lists
		/// </summary>
		private void Initialize_Controls() {
			//FB gains - NI
			axisFBgain.Clear();
			axisFBgain.Add(AxisFBgain_0); axisFBgain.Add(AxisFBgain_1); axisFBgain.Add(AxisFBgain_2);
			axisFBgain.Add(AxisFBgain_3); axisFBgain.Add(AxisFBgain_4); axisFBgain.Add(AxisFBgain_5);

			//FF gains - NI
			numericFFgain.Clear();
			numericFFgain.Add(FFgain0); numericFFgain.Add(FFgain1); numericFFgain.Add(FFgain2); numericFFgain.Add(FFgain3);

			//FB gains - MS
			numUD_axisFBgain.Clear();
			numUD_axisFBgain.Add(numAxisFBgain_0); numUD_axisFBgain.Add(numAxisFBgain_1); numUD_axisFBgain.Add(numAxisFBgain_2);
			numUD_axisFBgain.Add(numAxisFBgain_3); numUD_axisFBgain.Add(numAxisFBgain_4); numUD_axisFBgain.Add(numAxisFBgain_5);

			//FF gains - MS
			numUD_FFgain.Clear();
			numUD_FFgain.Add(numFFgain0); numUD_FFgain.Add(numFFgain1); numUD_FFgain.Add(numFFgain2); numUD_FFgain.Add(numFFgain3);

			//Working state buttons
			sbWorking.Clear();
			sbWorking.Add(Working0); sbWorking.Add(Working1); sbWorking.Add(Working2); sbWorking.Add(Working3);

			//Adaptive state buttons
			sbAdaptive.Clear();
			sbAdaptive.Add(Adaptive0); sbAdaptive.Add(Adaptive1); sbAdaptive.Add(Adaptive2); sbAdaptive.Add(Adaptive3);

			//FB Axis enable
			cbAxisEn.Clear();
			cbAxisEn.Add(ToggleAllFBaxes);
			cbAxisEn.Add(AxisEn0); cbAxisEn.Add(AxisEn1); cbAxisEn.Add(AxisEn2);
			cbAxisEn.Add(AxisEn3); cbAxisEn.Add(AxisEn4); cbAxisEn.Add(AxisEn5);
		}

		/// <summary>
		/// Handler for form load event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void frmFloorFF_Load(object sender, EventArgs e)
		{
			Application.DoEvents();
			this.Show();

			if (formMain.HasFloorFF)
			{
				Refresh_gains();
			}
			this.Text = "FB & FF Gains";
		}

		/// <summary>
		/// Handler for refresh button click.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void cmdRefresh_Click(object sender, EventArgs e) {
			Refresh_gains();
		}


		/// <summary>
		/// Refresh the gains page.
		/// Reads the data from controller and updates on UI
		/// </summary>
		public void Refresh_gains() {
			int sep_pos;
			string controller_param;
			double paramValue;
			int max_indx;
			string retval;

			if (Program.ConnectionType == (int)Program.ConnType.ConnDEMO) return;

			FormLoading = true; // prevent sending commands back to controller

			#region Read FB gains
			// NI
			max_indx = axisFBgain.Count;
			for (int i = 0; i < max_indx; i++) {
				HandleToCallingForm.SendInternal(axisFBgain[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (Tools.UpdateValueFromResponse(controller_param, out paramValue))
					axisFBgain[i].Value = paramValue;
			}
			// MS
			max_indx = numUD_axisFBgain.Count;
			for (int i = 0; i < max_indx; i++)
			{
				HandleToCallingForm.SendInternal(numUD_axisFBgain[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (Tools.UpdateValueFromResponse(controller_param, out paramValue))
					numUD_axisFBgain[i].Value = (decimal)paramValue;
			}

			#endregion

			#region Read axis enable
			max_indx = cbAxisEn.Count;
			for (int i = 0; i < max_indx; i++)
			{
				HandleToCallingForm.SendInternal(cbAxisEn[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				sep_pos = controller_param.IndexOf(">");
				if (sep_pos != -1) {
					retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
					cbAxisEn[i].Checked = retval == "acti";
				}
			}
			#endregion

			#region Read FF gains
			// NI
			max_indx = numericFFgain.Count;
			for (int i = 0; i < max_indx; i++) {
				HandleToCallingForm.SendInternal((numericFFgain[i].Tag.ToString() + "?"), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (controller_param == "Timeout" || controller_param == "") return;

				sep_pos = controller_param.IndexOf("=");
				if (sep_pos != -1) {
					retval = controller_param.Substring(sep_pos + 1);
					if (sep_pos < 3) sep_pos = 3; // to prevent errors

					if (controller_param.Substring(sep_pos - 1, 1).ToLower() == "a") {
						sbAdaptive[i].Checked = true;
					}
					if (controller_param.Substring(sep_pos - 1, 1).ToLower() == "n") {
						sbAdaptive[i].Checked = false;
					}
					if (controller_param.Substring(sep_pos - 2, 1).ToLower() == "w") {
						sbWorking[i].Checked = true;
					}
					if (controller_param.Substring(sep_pos - 2, 1).ToLower() == "p") {
						sbWorking[i].Checked = false;
					}

					sep_pos = retval.IndexOf("//");  // if verbose response
					if (sep_pos != -1) {
						retval = retval.Substring(0, sep_pos - 1);
					}
					if (double.TryParse(retval, out paramValue)) {
						numericFFgain[i].Value = paramValue;
					}
				}
			}
			// MS
						max_indx = numUD_FFgain.Count;
			for (int i = 0; i < max_indx; i++)
			{
				HandleToCallingForm.SendInternal((numUD_FFgain[i].Tag.ToString() + "?"), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				sep_pos = controller_param.IndexOf("=");
				if (sep_pos != -1)
				{
					retval = controller_param.Substring(sep_pos + 1);
					if (sep_pos < 3) sep_pos = 3; // to prevent errors

					sbAdaptive[i].Checked = controller_param.Substring(sep_pos - 1, 1).ToLower() == "a";
					sbWorking[i].Checked = controller_param.Substring(sep_pos - 2, 1).ToLower() == "w";

					sep_pos = retval.IndexOf("//");  // if verbose response
					if (sep_pos != -1)
					{
						retval = retval.Substring(0, sep_pos - 1);
					}
					if (double.TryParse(retval, out paramValue))
					{
						numUD_FFgain[i].Value = (decimal)paramValue;
					}
				}
			}

			#endregion

			#region Read FF all
			HandleToCallingForm.SendInternal(ToggleFFall.Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
			sep_pos = controller_param.IndexOf(">");
			if (sep_pos != -1) {
				retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
				ToggleFFall.Checked = retval == "acti";
			}
			#endregion

			#region Read FF motors
			HandleToCallingForm.SendInternal(ToggleFFmotors.Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
			sep_pos = controller_param.IndexOf(">");
			if (sep_pos != -1) {
				retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
				ToggleFFmotors.Checked = retval == "acti";
			}
			#endregion

			double paramVal = HandleToCallingForm.RectifyValueResponse("drvo?");
			if (paramVal != WRONG_VALUE) {
				Program.FormMain.ExcitationAxis = Convert.ToInt32(paramVal);
				if (Program.FormRTMonitoring != null)
					Program.FormRTMonitoring.cmb_ExcitAxis.SelectedIndex = Program.FormMain.ExcitationAxis;
				if (Program.FormMain.ExcitationAxis < cmb_ExcitAxis.Items.Count)
					this.cmb_ExcitAxis.SelectedIndex = Program.FormMain.ExcitationAxis;
				else
					this.cmb_ExcitAxis.SelectedIndex = cmb_ExcitAxis.Items.Count - 1;
			}
			FormLoading = false;
		}

		/// <summary>
		/// Send the command associated with toggle switch
		/// </summary>
		/// <param name="checkBoxCtrl"></param>
		private void SendToggleSwitchCommand(CheckBox checkBoxCtrl) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			string toggle_state = checkBoxCtrl.Tag.ToString();
			if (checkBoxCtrl.Checked)
				toggle_state += ">active";
			else
				toggle_state += ">passive";
			HandleToCallingForm.SendInternal(toggle_state, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		private void ToggleFF_ALL_CheckedChanged(object sender, EventArgs e) {
			SendToggleSwitchCommand((CheckBox)sender);
		}

		private void ToggleFFmotAdaptive_CheckedChanged(object sender, EventArgs e) {
			SendToggleSwitchCommand((CheckBox)sender);
		}

		private void ToggleFFmotors_CheckedChanged(object sender, EventArgs e) {
			SendToggleSwitchCommand((CheckBox)sender);
		}

		private void ToggleAllFBaxes_CheckedChanged(object sender, EventArgs e) {
			SendToggleSwitchCommand((CheckBox)sender);
		}

		private void AxisEn_CheckedChanged(object sender, EventArgs e) {
			SendToggleSwitchCommand((CheckBox)sender);
		}

		private void AxisFBgain_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			NumericEdit NInumFBgain = (NumericEdit)sender;
			string gain_cmd = NInumFBgain.Tag.ToString() + "=" + NInumFBgain.Value.ToString("0.00");
			HandleToCallingForm.SendInternal(gain_cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		// IK20251228 added this event for FB gains
		private void AxisFBgain_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {
			var num_gain = (NumericEdit)sender;
			if (num_gain.Value == 0) // show zero as "0", not as "0.000E+000"
				num_gain.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(0);  // example: " 0 "
			else
				num_gain.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(3);  // example: "10.323"
				//num_gain.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
		}

		private void FBgain_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) { // Then Enter key was pressed
				AxisFBgain_ValueChanged(sender, null);
				e.Handled = true; // suppress "ding" sound
			}
		}

		private void FFjerk_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			NumericEdit ffJerk = (NumericEdit)sender;
			string gain_cmd = ffJerk.Tag.ToString() + "wn=" + ffJerk.Value.ToString("0.00");
			HandleToCallingForm.SendInternal(gain_cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		private void FFjerk_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {
			var numFFjerk = (NumericEdit)sender;
			if (numFFjerk.Value == 0) // show zero as "0", not as "0.000E+000"
				numFFjerk.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(0);  // example: " 0 "
			else
				numFFjerk.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(2);  // example: "10.32"
		}

		private void FFjerk_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) { // Then Enter key was pressed
				FFjerk_ValueChanged(sender, null);
				e.Handled = true; // suppress "ding" sound
			}
		}

		private void FFgain_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			NumericEdit numFFgain = (NumericEdit)sender;
			int index = numericFFgain.IndexOf(numFFgain);
			string resp = string.Empty;
			string gain_cmd = numFFgain.Tag.ToString() + sbWorking[index].Tag.ToString() + sbAdaptive[index].Tag.ToString()
				+ "=" + numFFgain.Value.ToString("E3");
			HandleToCallingForm.SendInternal(gain_cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		/// <summary>
		/// show zero as "0", not as "0.000E+000"
		/// NOTE: if numeric has zero already and Scientific format, function WOULD NOT BE CALLED and re-format!
		/// because Value has not been changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NumericFFgain_AfterChangeValue(object sender, NationalInstruments.UI.AfterChangeNumericValueEventArgs e) {
			var numFFgain = (NumericEdit)sender;
			// IK20220102 these simply do not have this event: if ((numFFgain.Name != "FF_CoM_Xpos") || (numFFgain.Name != "FF_CoM_Ypos"))
			{
				if (numFFgain.Value == 0) // show zero as "0", not as "0.000E+000"
					numFFgain.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateSimpleDoubleMode(0);
				else
					numFFgain.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
			}
		}

		private void FFgain_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) { // Then Enter key was pressed
				FFgain_ValueChanged(sender, null);
				e.Handled = true; // suppress "ding" sound
			}
		}
		// IK20251228 added this event for FB gains
		private void AxisFBgain_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) { // Then Enter key was pressed
				AxisFBgain_ValueChanged(sender, null);
				e.Handled = true; // suppress "ding" sound
			}
		}

		private void Working_CheckedChanged(object sender, EventArgs e) {
			StateButton working = sender as StateButton;
			if (working.Checked)
				working.Tag = "W";
			else
				working.Tag = "P";

			if (Program.IsReadingControllerSettings || FormLoading) return;

			int index = sbWorking.IndexOf(working);
			string resp = string.Empty;
			string cmd = numericFFgain[index].Tag.ToString() + sbWorking[index].Tag.ToString() + sbAdaptive[index].Tag.ToString()
				+ "=" + numericFFgain[index].Value.ToString();
			HandleToCallingForm.SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void Adaptive_CheckedChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			StateButton adaptive = sender as StateButton;
			if (adaptive.Checked)
				adaptive.Tag = "A";
			else
				adaptive.Tag = "N";

			int index = sbAdaptive.IndexOf(adaptive);
			string resp = string.Empty;
			string cmd = numericFFgain[index].Tag.ToString() + sbWorking[index].Tag.ToString() + sbAdaptive[index].Tag.ToString()
				+ "=" + numericFFgain[index].Value.ToString();
			HandleToCallingForm.SendInternal(cmd, CommandTypes.ResponseExpected, out resp, CommandAckTypes.AckExpected);
		}

		private void cmdPulse_Click(object sender, EventArgs e) {
			string controller_command;
			if (!HandleToCallingForm.p_cmd_as_PN)
				controller_command = "p";
			else
				controller_command = "puls";
			string response;
			HandleToCallingForm.SendInternal(controller_command, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			if (response.IndexOf("95") != -1)   //Bruker Insight AFP 95-43009-01r.At3dbg @26-Oct-16;compiled Oct 28 2016 13:48:19
				HandleToCallingForm.p_cmd_as_PN = true;
			else
				HandleToCallingForm.p_cmd_as_PN = false;
			Program.IsReadingControllerSettings = true;
			HandleToCallingForm.ExcitationStatus = false;
			HandleToCallingForm.ToggleExcitation.Checked = HandleToCallingForm.ExcitationStatus;
			if (Program.FormRTMonitoring != null) Program.FormRTMonitoring.ToggleExcitation.Checked = HandleToCallingForm.ExcitationStatus;
			Program.IsReadingControllerSettings = false;
		}

		/// <summary>
		/// "restore params from flash" command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CmdRestorefromFlash_Click(object sender, EventArgs e) {
			string response;
			HandleToCallingForm.SendInternal("rsto", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
			HandleToCallingForm.GetSysData();
		}

		private void cmdLoadDefaults_Click(object sender, EventArgs e) {
			string response;
			HandleToCallingForm.SendInternal("dflt", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
		}

		private void cmdSaveParams_Click(object sender, EventArgs e) {
			string response;
			HandleToCallingForm.SendInternal("save", CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
		}


		private void cmb_ExcitAxis_SelectedIndexChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			Program.FormMain.ExcitationAxis = cmb_ExcitAxis.SelectedIndex;
			string command_Renamed = "drvo=" + cmb_ExcitAxis.SelectedIndex.ToString(); // HandleToCallingForm.ExcitationAxis.ToString();
			string response;
			HandleToCallingForm.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
		}

		private void numFFgain_ValueChanged(object sender, EventArgs e) {
			if (FormLoading) return;
			ScientificNumericUpDown numFFgain = (ScientificNumericUpDown)sender;
			int index = numUD_FFgain.IndexOf(numFFgain);
			string gain_cmd = numFFgain.Tag.ToString() + sbWorking[index].Tag.ToString() + sbAdaptive[index].Tag.ToString()
				+ "=" + numFFgain.Value.ToString("E3");
			HandleToCallingForm.SendInternal(gain_cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		private void NumericFFgain_AfterChangeValue(object sender, EventArgs e) {
			ScientificNumericUpDown numFFgain = (ScientificNumericUpDown)sender;
			numFFgain.UpdateEditText();
		}

		private void numFFgain_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) {
				numFFgain_ValueChanged(sender, e);
				e.Handled = true;
			}
		}

		private void numAxisFBgain_ValueChanged(object sender, EventArgs e) {
			if (FormLoading) return;

			ScientificNumericUpDown numFBgain = (ScientificNumericUpDown)sender;
			string gain_cmd = numFBgain.Tag.ToString() + "=" + numFBgain.Value.ToString("0.00");
			HandleToCallingForm.SendInternal(gain_cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		private void numAxisFBgain_AfterChangeValue(object sender, EventArgs e) {
			ScientificNumericUpDown numFBgain = (ScientificNumericUpDown)sender;
			numFBgain.UpdateEditText();
		}

		private void numAxisFBgain_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) {
				numAxisFBgain_ValueChanged(sender, e);
				e.Handled = true;
			}
		}
		private void formGains_FormClosed(object sender, FormClosedEventArgs e) {
			//HandleToCallingForm.Toggle_FB_FFgains_Controls() 'changes FormGainsVisible = not FormGainsVisible
		}

		private void formGains_FormClosing(object sender, FormClosingEventArgs e) {
			if (e.CloseReason != CloseReason.ApplicationExitCall) {
				Program.FormMain.OpenCloseGainsForm();
				e.Cancel = true;
			}
		}

		private void formGains_VisibleChanged(object sender, EventArgs e) {
			if (this.Visible) Refresh_gains();
		}
	}

}
