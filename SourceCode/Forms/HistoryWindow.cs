using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TMCAnalyzer
{
	public partial class HistoryWindow : Form
	{
		#region Fields
		private bool _isAutoClearChecked = true;
		private bool _isSentCmdChecked = false;
		private int LineCount = 0;
		#endregion

		#region Properties
		public bool IsSentCmdChecked
		{
			get { return _isSentCmdChecked; }
			set { _isSentCmdChecked = value;}
		}
		#endregion

		public HistoryWindow()
		{
			InitializeComponent();
			checkBoxSentCmd.Checked = true;
		}

		#region Private Implementation
		/// <summary>
		/// Triggers by ClearHistoryCommand
		/// Calls ClearHistory method
		/// </summary>
		private void OnClearHistoryCommand()
		{
			ClearHistory();
		}
		#endregion

		#region Public Implementation
		/// <summary>
		/// Adding text to the history window
		/// </summary>
		/// <param name = "message" > input text which needs to add/print in the history window</param>
		public void AddHistory(string message)
		{
			if (InvokeRequired) textBoxHistory.BeginInvoke((MethodInvoker)delegate { textBoxHistory.Text = textBoxHistory.Text + message + "\r\n"; });
			else textBoxHistory.Text = textBoxHistory.Text + message + "\r\n";
			if (_isAutoClearChecked && (LineCount > 2000))
			{
				ClearHistory();
				LineCount = -1;
			}
			LineCount = LineCount + 1;
			if (InvokeRequired) labelLinesCount.BeginInvoke((MethodInvoker)delegate { labelLinesCount.Text = LineCount.ToString(); });
			else labelLinesCount.Text = LineCount.ToString();

			if (InvokeRequired) labelCharsCount.BeginInvoke((MethodInvoker)delegate { labelCharsCount.Text = textBoxHistory.Text.Length.ToString(); });
			else labelCharsCount.Text = textBoxHistory.Text.Length.ToString();
		}
		/// <summary>
		/// Clears the history text and makes the count of lines and chars to zero
		/// </summary>
		public void ClearHistory()
		{
			if (InvokeRequired) textBoxHistory.BeginInvoke((MethodInvoker)delegate { textBoxHistory.Text = ""; });
			else textBoxHistory.Text = "";
			LineCount = 0;
			if (InvokeRequired) labelLinesCount.BeginInvoke((MethodInvoker)delegate { labelLinesCount.Text = LineCount.ToString(); });
			else labelLinesCount.Text = LineCount.ToString();

			if (InvokeRequired) labelCharsCount.BeginInvoke((MethodInvoker)delegate { labelCharsCount.Text = textBoxHistory.Text.Length.ToString(); });
			else labelCharsCount.Text = textBoxHistory.Text.Length.ToString();
		}
		#endregion

		private void buttonClearHistory_Click(object sender, EventArgs e)
		{
			ClearHistory();
		}

		private void checkBoxSentCmd_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxSentCmd.Checked)
				IsSentCmdChecked = true;
			else
				IsSentCmdChecked = false;
		}

		private void checkBoxAutoClear_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxAutoClear.Checked)
				_isAutoClearChecked = true;
			else
				_isAutoClearChecked = false;
		}

		private void HistoryWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			TerminalControl.CloseHistoryWindow();
			this.Hide();
			e.Cancel = true;
		}

		public int AutoAddChars = 100;
		public bool AutoAdd = false;
		private void numAutoAdd_ValueChanged(object sender, EventArgs e) {
			AutoAddChars = (int)numAutoAdd.Value;
		}

		private void checkBoxAutoAdd_CheckedChanged(object sender, EventArgs e) {
		   if(checkBoxAutoAdd.Checked ) AutoAdd = true;
		   else AutoAdd = false;
		}
	}
}
