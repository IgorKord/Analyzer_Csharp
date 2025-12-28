using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TMCAnalyzer
{
	public partial class Terminal : Form
	{
		public bool FormTerminalVisible = false;

		#region Properties
		/// <summary>
		/// Indicates whether menu mode is active
		/// </summary>
		public bool IsMenuActive
		{
			get { return terminalControl1.IsMenuActive; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of Terminal
		/// </summary>
		public Terminal()
		{
			InitializeComponent();
		}
		#endregion

		#region Private&Protected Implementations
		/// <summary>
		/// Handler for the terminal form load event.
		/// Subscribe for the required events of terminal control.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Terminal_Load(object sender, EventArgs e)
		{
			TerminalControl.StatusMsg += UpdateStatus;
		}

		/// <summary>
		/// Updates the status message on the window header.
		/// </summary>
		/// <param name="message"></param>
		private void UpdateStatus(string message)
		{
			this.Text = message;
		}

		/// <summary>
		/// Handles the form resize and changes the height proportional to the width of the form.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnResize(EventArgs e)
		{
			Control myControl = this;
			terminalControl1.Hide();
			if (!this.ClientRectangle.IsEmpty)
			{
				float scaleFactor = 0.69f;
				int lineCount = 33; //25 line + 8 for line spacing
				if (myControl.Size.Height != myControl.Size.Width * scaleFactor || myControl.Size.Height < TerminalControl.Pixel_Font_Height * lineCount)
				{
					int h1 = (int)(TerminalControl.Pixel_Font_Height * lineCount);
					int h2 = (int)(myControl.Width * scaleFactor);
					myControl.Height = (h1 > h2) ? h1 : h2;
					//myControl.Height = (int)(myControl.Width * 0.73);
				}
			}
			terminalControl1.Show();
			terminalControl1.Invalidate();
		}

		private void Terminal_FormClosing(object sender, FormClosingEventArgs e)
		{
			Program.FormMain.OpenTerminalForm();
			this.Hide();
			e.Cancel = true;
		}

		#endregion

		#region Public Implementations
		public void Delay_sec(float delay_time)
		{
			terminalControl1.Delay_sec(delay_time);
		}

		public void MainForm_SendCallBack(string command, string response)
		{
			terminalControl1.MainForm_SendCallBack(command, response);
		}

		public void OnQuitMenuCommand()
		{
			terminalControl1.OnQuitMenuCommand();
		}

		public void UpdateStatus()
		{
			terminalControl1.UpdateStatus();
		}
		#endregion
	}
}
