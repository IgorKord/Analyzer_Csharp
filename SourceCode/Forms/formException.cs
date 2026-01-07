using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TMCAnalyzer {
	public partial class formException : Form {
		//public CustomDialogBox() {
		//    InitializeComponent();
		//}
		//string msg_str = "";
		//string src_str = "";
		//string mailto_str = "";

		private void btnMailTo_Click(object sender, EventArgs e) {
			//mailto_str = "mailto:tmc.SoftwareFeedback@ametek.com";
			//mailto_str += "?subject=";
			//mailto_str += LblMsgSrc.Text;
			//mailto_str += "&body=";
			//mailto_str += txtMsgTest.Text;
			//System.Diagnostics.Process.Start(mailto_str);
			DialogResult = DialogResult.Yes;
		}

		private void btnContinue_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Ignore;
		}

		private void btnQuitApp_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Abort;
		}

		public formException() { InitializeComponent(); }

		private void formLogin_FormClosing(object sender, FormClosingEventArgs e) { /*user pressed (X)*/}

		private void formException_Load(object sender, EventArgs e) {
		}
	}
}