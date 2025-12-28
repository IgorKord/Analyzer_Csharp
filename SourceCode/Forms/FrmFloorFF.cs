using System;
using System.Windows.Forms;
using TMCAnalyzer; // For Globals, CommPort
//using TMCAnalyzer.CustomClasses; // For StateButton if needed
using NationalInstruments.UI.WindowsForms; // For NumericEdit

namespace TMCAnalyzer.Forms {
    public partial class FrmFloorFF:Form {
        public FrmFloorFF() {
            InitializeComponent();
        }

        private void FrmFloorFF_Load(object sender, EventArgs e) {
            //if (Globals.HasFloorFF) {
                // Fetch floor FF params
                // CommPort.GetSend("get floorff", true);
                // Populate numerics and state buttons from response (implement parsing if not in original)
            //}
        }

        // Event for ALL FF Loops toggle
        private void cwbutAllFFLoops_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // Similar for FF to Motors
        private void cwbutFFtoMotors_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // For global Adaptive
        private void cwbutFFAdaptive_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // For Working buttons (0 for adaptive rate, 1-3 for axes)
        private void cwWorking0_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // Similar for cwWorking1, cwWorking2, cwWorking3

        // For Adaptive buttons (0 for adaptive rate, 1-3 for axes)
        private void cwAdaptive0_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // Similar for cwAdaptive1,2,3

        // For numeric gains (ValueChanged to send new value)
        private void numFFgain0_ValueChanged(object sender, EventArgs e) {
            NumericEdit num = (NumericEdit)sender;
            string command = num.Tag.ToString() + "=" + num.Value.ToString();
            // CommPort.GetSend(command, true);
        }

        // Similar for numFFgain1,2,3, and FB gains (numGainX, etc.)

        // For ALL FB AXES
        private void cwButAxisEn_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // For FB status and per-axis toggles
        private void cwStatus_CheckedChanged(object sender, EventArgs e) {
            StateButton btn = (StateButton)sender;
            string command = btn.Tag.ToString() + "=" + (btn.Checked ? "1" : "0");
            // CommPort.GetSend(command, true);
        }

        // Similar for cwX, cwY, cwtZ, cwZ, cwtX, cwtY

        // For pulse
        private void cmdPulse_Click(object sender, EventArgs e) {
            string axis = cmbExcitAxis.SelectedItem?.ToString() ?? "X";
            // CommPort.GetSend("puls " + axis, true);
        }

        // For restore, load, save
        private void cmdRestorefromFlash_Click(object sender, EventArgs e) {
            // CommPort.GetSend("restore", true);
        }

        private void cmdLoadDefaults_Click(object sender, EventArgs e) {
            // CommPort.GetSend("default", true);
        }

        private void cmdSaveParams_Click(object sender, EventArgs e) {
            // CommPort.GetSend("save", true);
        }
    }
}