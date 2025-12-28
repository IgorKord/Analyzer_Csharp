using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TMCAnalyzer {
    public partial class CustomDialogBox : UserControl {
        public CustomDialogBox() {
            InitializeComponent();
        }

        private void btnMailTo_Click(object sender, EventArgs e) {
            ((Form)this.Parent).DialogResult = DialogResult.Yes;
        }

        private void btnContinue_Click(object sender, EventArgs e) {
            ((Form)this.Parent).DialogResult = DialogResult.Ignore;
        }

        private void btnQuitApp_Click(object sender, EventArgs e) {
            ((Form)this.Parent).DialogResult = DialogResult.Abort;
        }
    }
}
