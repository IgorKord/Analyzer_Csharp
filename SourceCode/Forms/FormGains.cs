using System;
using System.Collections.Generic;
using NationalInstruments.UI.WindowsForms;
using System.Windows.Forms;

namespace TMCAnalyzer {
	public partial class formGains:Form {

		private bool FormLoading = true;
		private formMain HandleToCallingForm = null;
		internal bool FormTerminalVisible = false;
		const double WRONG_VALUE = 77777.7;

		List<NumericEdit> axisFBgain = new List<NumericEdit>();
		List<NumericEdit> numericFFgain = new List<NumericEdit>();
		List<StateButton> sbWorking = new List<StateButton>();
		List<StateButton> sbAdaptive = new List<StateButton>();
		List<StateButton> cbAxisEn = new List<StateButton>();
		List<NumericEdit> numericFloatingPressures = new List<NumericEdit>();
		List<NumericEdit> numericJerkGains = new List<NumericEdit>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="handleToMainForm"></param>
		public formGains(formMain handleToMainForm) {
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
			//FB gains
			axisFBgain.Clear();
			axisFBgain.Add(AxisFBgain_0); axisFBgain.Add(AxisFBgain_1); axisFBgain.Add(AxisFBgain_2);
			axisFBgain.Add(AxisFBgain_3); axisFBgain.Add(AxisFBgain_4); axisFBgain.Add(AxisFBgain_5);

			//FF gains
			numericFFgain.Clear();
			numericFFgain.Add(FF_CoM_Xpos); numericFFgain.Add(FFgain01); numericFFgain.Add(FFgain02); numericFFgain.Add(FFgain03); numericFFgain.Add(FFgain04);
			numericFFgain.Add(FFgain05); numericFFgain.Add(FFgain06); numericFFgain.Add(FF_CoM_Ypos); numericFFgain.Add(FFgain08); numericFFgain.Add(FFgain09);
			numericFFgain.Add(FFgain10); numericFFgain.Add(FFgain11); numericFFgain.Add(FFgain12); numericFFgain.Add(FFgain13);

			//Working state buttons
			sbWorking.Clear();
			sbWorking.Add(Working00); sbWorking.Add(Working01); sbWorking.Add(Working02); sbWorking.Add(Working03);
			sbWorking.Add(Working04); sbWorking.Add(Working05); sbWorking.Add(Working06); sbWorking.Add(Working07);
			sbWorking.Add(Working08); sbWorking.Add(Working09); sbWorking.Add(Working10); sbWorking.Add(Working11);
			sbWorking.Add(Working12); sbWorking.Add(Working13);

			//Adaptive state buttons
			sbAdaptive.Clear();
			sbAdaptive.Add(Adaptive00); sbAdaptive.Add(Adaptive01); sbAdaptive.Add(Adaptive02); sbAdaptive.Add(Adaptive03);
			sbAdaptive.Add(Adaptive04); sbAdaptive.Add(Adaptive05); sbAdaptive.Add(Adaptive06); sbAdaptive.Add(Adaptive07);
			sbAdaptive.Add(Adaptive08); sbAdaptive.Add(Adaptive09); sbAdaptive.Add(Adaptive10); sbAdaptive.Add(Adaptive11);
			sbAdaptive.Add(Adaptive12); sbAdaptive.Add(Adaptive13);

			// FB Axis enable
			cbAxisEn.Clear();
			cbAxisEn.Add(ToggleAllFBaxes);
			cbAxisEn.Add(AxisEn0); cbAxisEn.Add(AxisEn1); cbAxisEn.Add(AxisEn2);
			cbAxisEn.Add(AxisEn3); cbAxisEn.Add(AxisEn4); cbAxisEn.Add(AxisEn5);

			//Pressure
			numericFloatingPressures.Clear();
			numericFloatingPressures.Add(numPressure1); numericFloatingPressures.Add(numPressure2);
			numericFloatingPressures.Add(numPressure3); numericFloatingPressures.Add(numPressure3);

			//Jerk gains
			numericJerkGains.Add(FF_Jerk_0); numericJerkGains.Add(FF_Jerk_1); numericJerkGains.Add(FF_Jerk_2); numericJerkGains.Add(FF_Jerk_3);
			numericJerkGains.Add(FF_Jerk_4); numericJerkGains.Add(FF_Jerk_5); numericJerkGains.Add(FF_Jerk_6); numericJerkGains.Add(FF_Jerk_7);
			numericJerkGains.Add(FF_Jerk_8); numericJerkGains.Add(FF_Jerk_9); numericJerkGains.Add(FF_Jerk_10); numericJerkGains.Add(FF_Jerk_11);

		}

		/// <summary>
		/// Handler for form load event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void formGains_Load(object sender, EventArgs e) {
			FormLoading = true;
			Application.DoEvents();
			this.Show();

			Refresh_gains();
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
			max_indx = axisFBgain.Count;
			for (int i = 0; i < max_indx; i++) {
				HandleToCallingForm.SendInternal(axisFBgain[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (Tools.UpdateValueFromResponse(controller_param, out paramValue))
					axisFBgain[i].Value = paramValue;
			}
			#endregion

			#region Read axis enable
			max_indx = cbAxisEn.Count;
			for (int i = 0; i < max_indx; i++) {
				HandleToCallingForm.SendInternal(cbAxisEn[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				sep_pos = controller_param.IndexOf(">");
				if (sep_pos != -1) {
					retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
					if (retval == "acti") {
						cbAxisEn[i].Checked = true;
					}
					if (retval == "pass") {
						cbAxisEn[i].Checked = false;
					}
				}
			}
			#endregion

			#region Read pressure
			max_indx = numericFloatingPressures.Count;
			for (int i = 0; i < max_indx; i++) {
				HandleToCallingForm.SendInternal(numericFloatingPressures[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (Tools.UpdateValueFromResponse(controller_param, out paramValue))
					numericFloatingPressures[i].Value = paramValue;
			}
			#endregion

			#region Read jerk gains
			max_indx = numericJerkGains.Count;
			for (int i = 0; i < max_indx; i++) {
				HandleToCallingForm.SendInternal(numericJerkGains[i].Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
				if (Tools.UpdateValueFromResponse(controller_param, out paramValue))
					numericJerkGains[i].Value = paramValue;
			}
			#endregion

			#region Read FF gains
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
			#endregion

			#region Read FF all
			HandleToCallingForm.SendInternal(ToggleFFall.Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
			sep_pos = controller_param.IndexOf(">");
			if (sep_pos != -1) {
				retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
				if (retval == "acti") {
					ToggleFFall.Checked = true;
				} else { //pass
					ToggleFFall.Checked = false;
				}
			}
			#endregion

			#region Read FF valves
			HandleToCallingForm.SendInternal(ToggleFFvalves.Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
			sep_pos = controller_param.IndexOf(">");
			if (sep_pos != -1) {
				retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
				if (retval == "acti") {
					ToggleFFvalves.Checked = true;
				} else { //pass
					ToggleFFvalves.Checked = false;
				}
			}
			#endregion

			#region Read FF motors
			HandleToCallingForm.SendInternal(ToggleFFmotors.Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
			sep_pos = controller_param.IndexOf(">");
			if (sep_pos != -1) {
				retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
				if (retval == "acti") {
					ToggleFFmotors.Checked = true;
				} else { //pass
					ToggleFFmotors.Checked = false;
				}
			}
			#endregion

			#region Read pressure loop active
			HandleToCallingForm.SendInternal(TogglePressureLoops.Tag.ToString(), CommandTypes.ResponseExpected, out controller_param, CommandAckTypes.AckExpected);
			sep_pos = controller_param.IndexOf(">");
			if (sep_pos != -1) {
				retval = controller_param.Substring(sep_pos + 1, 4).ToLower();
				if (retval == "acti") {
					TogglePressureLoops.Text = "Pressure Loops Enabled";
					TogglePressureLoops.CheckState = System.Windows.Forms.CheckState.Checked;
				} else { //pass
					TogglePressureLoops.Text = "Pressure Loops PASSIVE";
					TogglePressureLoops.CheckState = System.Windows.Forms.CheckState.Unchecked;
				}
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

		private void ToogleFFvalves_CheckedChanged(object sender, EventArgs e) {
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

		private void TogglePressureLoopActive_CheckStateChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;
			string response;
			string controller_command;
			if (TogglePressureLoops.CheckState == CheckState.Checked) {
				TogglePressureLoops.Text = "Pressure Loops Enabled";
				controller_command = TogglePressureLoops.Tag.ToString() + ">active";
			} else {
				TogglePressureLoops.Text = "Pressure Loops PASSIVE";
				controller_command = TogglePressureLoops.Tag.ToString() + ">passive";
			}
			HandleToCallingForm.SendInternal(controller_command, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
		}

		private void cmb_ExcitAxis_SelectedIndexChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			Program.FormMain.ExcitationAxis = cmb_ExcitAxis.SelectedIndex;
			string command_Renamed = "drvo=" + cmb_ExcitAxis.SelectedIndex.ToString(); // HandleToCallingForm.ExcitationAxis.ToString();
			string response;
			HandleToCallingForm.SendInternal(command_Renamed, CommandTypes.ResponseExpected, out response, CommandAckTypes.AckExpected);
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

		/// <summary>
		/// call dock/float command from main form (instead of direct call)
		/// this updates control on main form too
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ToggleVertDock_CheckedChanged(object sender, EventArgs e) {
			Program.FormMain.ToggleVertDock.Checked = this.ToggleVertDock.Checked;
		}

		/// <summary>
		/// call Update Pressure Setpoints command from main form (instead of direct call)
		/// this updates control on main form too
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CmdSetPressurePoints_Click(object sender, EventArgs e) {
			Program.FormMain.CmdSetPressurePoints();
			// form Main reads updated pressure setpoints into formMain.SystemPressures[]
			int max_indx = numericFloatingPressures.Count;
			for (int i = 0; i < max_indx; i++) {
				numericFloatingPressures[i].Value = formMain.SystemPressures[i];
			}
		}


		private void NInumDockParam_ValueChanged(object sender, EventArgs e) {
			if (Program.IsReadingControllerSettings || FormLoading) return;

			NumericEdit NInumDockParam = (NumericEdit)sender;
			string gain_cmd = NInumDockParam.Tag.ToString() + "=" + NInumDockParam.Value.ToString("0.00");
			HandleToCallingForm.SendInternal(gain_cmd, CommandTypes.ResponseExpected, out _, CommandAckTypes.AckExpected);
		}

		private void NInumDockParam_KeyPress(object sender, KeyPressEventArgs e) {
			if (e.KeyChar == (char)13) { // Then Enter key was pressed
				AxisFBgain_ValueChanged(sender, null);
				e.Handled = true; // suppress "ding" sound
			}
		}

	}
}
