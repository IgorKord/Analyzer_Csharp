using System;
using System.Windows.Forms;
using NationalInstruments.UI.WindowsForms; // Measurement Studio

namespace TMCAnalyzer.Forms {
    public partial class FrmFloorFF:Form {
        public FrmFloorFF() {
            InitializeComponent();
        }

        // Event for pulse button - send command (adapt to your comm logic)
        private void cmdPulse_Click(object sender, EventArgs e) {
            // Example: Send pulse to selected axis via existing Analyzer comm
            string axis = cmbExcitAxis.SelectedItem?.ToString() ?? "X";
            // Analyzer.GetSend($"puls {axis}", true); // Placeholder - use your method
            MessageBox.Show($"Pulse applied to {axis} axis.");
        }

        // Event for axis enable Led toggle
        private void cwButAxisEn_Click(object sender, EventArgs e) {
            // Toggle all FB axes - send command
            bool on = cwButAxisEn.Checked;
            // Analyzer.GetSend(on ? "loop_fba=1" : "loop_fba=0", true);
        }

        // Adaptive Led events (similar for each index)
        private void cwAdaptive1_Click(object sender, EventArgs e) {
            // Send adaptive command for index 1
            bool on = cwAdaptive1.Checked;
            // Analyzer.GetSend(on ? "adaptive1=1" : "adaptive1=0", true);
        }

        // Add similar for other cwAdaptive indices...
    }
}
