using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TMCAnalyzer
{
	public partial class frmDrivingOutput : Form
	{
		public int OutputChosen = 0;
		public string LTFtestOutputName = string.Empty;


		public frmDrivingOutput()
		{
			InitializeComponent();
			InitializeControls();

		}



		List<RadioButton> optExcitOutput = new List<RadioButton>();
		private void InitializeControls()
		{
			optExcitOutput.Clear();
			optExcitOutput.Add(_optExcitOutput_0); optExcitOutput.Add(_optExcitOutput_1); optExcitOutput.Add(_optExcitOutput_2);
			optExcitOutput.Add(_optExcitOutput_3); optExcitOutput.Add(_optExcitOutput_4); optExcitOutput.Add(_optExcitOutput_5);
			optExcitOutput.Add(_optExcitOutput_6); optExcitOutput.Add(_optExcitOutput_7); optExcitOutput.Add(_optExcitOutput_8);
			optExcitOutput.Add(_optExcitOutput_9); optExcitOutput.Add(_optExcitOutput_10); optExcitOutput.Add(_optExcitOutput_11);
			optExcitOutput.Add(_optExcitOutput_12); optExcitOutput.Add(_optExcitOutput_13); optExcitOutput.Add(_optExcitOutput_14);
			optExcitOutput.Add(_optExcitOutput_15); optExcitOutput.Add(_optExcitOutput_16); optExcitOutput.Add(_optExcitOutput_17);
			optExcitOutput.Add(_optExcitOutput_18); optExcitOutput.Add(_optExcitOutput_19); optExcitOutput.Add(_optExcitOutput_20);
			optExcitOutput.Add(_optExcitOutput_21); optExcitOutput.Add(_optExcitOutput_22); optExcitOutput.Add(_optExcitOutput_23);
			optExcitOutput.Add(_optExcitOutput_24); optExcitOutput.Add(_optExcitOutput_25); optExcitOutput.Add(_optExcitOutput_26);
			optExcitOutput.Add(_optExcitOutput_27); optExcitOutput.Add(_optExcitOutput_28); optExcitOutput.Add(_optExcitOutput_29);
			optExcitOutput.Add(_optExcitOutput_30); optExcitOutput.Add(_optExcitOutput_31);
			foreach (var item in optExcitOutput)
			{
				MethodInfo m = typeof(RadioButton).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
				if (m != null)
					m.Invoke(item, new object[] { ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true });
				item.MouseDoubleClick += new MouseEventHandler(radioButton_MouseDoubleClick);
			}
		}

		private void radioButton_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Leaving_Form();
		}

		private void frmDrivingOutput_Activated(object sender, EventArgs e)
		{
			if (Program.FormMain.LTFadvanced)
				_optExcitOutput_25.Visible = true;
			else
				_optExcitOutput_25.Visible = false;
		   // this.Text = "Choose Driving Output. Now = " + optExcitOutput[OutputChosen].Text;
		}

		private void frmDrivingOutput_Load(object sender, EventArgs e)
		{
			Text = "Choose Driving Output. Now = "+ optExcitOutput[OutputChosen].Text;
			Program.LTFtestOutputName = optExcitOutput[OutputChosen].Tag.ToString();
		}

		private void frmDrivingOutput_Click(object sender, EventArgs e)
		{
			Program.LTFtestOutputName = optExcitOutput[OutputChosen].Tag.ToString();
			Text = "Choose Driving Output. Now = " + optExcitOutput[OutputChosen].Text;
			Program.LTFtestOutputName = optExcitOutput[OutputChosen].Tag.ToString();

		}

		private void frmDrivingOutput_Deactivate(object sender, EventArgs e)
		{
			Program.LTFtestOutputName = optExcitOutput[OutputChosen].Tag.ToString();
			Text = "Choose Driving Output. Now = " + optExcitOutput[OutputChosen].Text;
			Program.FormMain.cmdChooseExcitation.Text = "Output = "  + optExcitOutput[OutputChosen].Text;
			Program.LTFtestOutputName = optExcitOutput[OutputChosen].Tag.ToString();
			optExcitOutput[OutputChosen].Checked = true;
		}

		private void frmDrivingOutput_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.ApplicationExitCall)
			{
				Leaving_Form();
				e.Cancel = true;
			}

		}

		private void Leaving_Form()
		{
			Program.LTFtestOutputName = optExcitOutput[OutputChosen].Tag.ToString();
			Program.FormMain.cmdChooseExcitation.Text = "Output = " + optExcitOutput[OutputChosen].Text;
			Program.FormMain.cmdChooseExcitation.Update();
			this.Hide();
		}

		private void optExcitOutput_Click(object sender, EventArgs e)
		{
			RadioButton Output = (RadioButton)sender;
			OutputChosen = GetIndex(Output);
			Text = "Choose Driving Output. Now = " + optExcitOutput[OutputChosen].Text;

		}

		private int GetIndex( RadioButton output)
		{
			int index = 0;
			for (index = 0; index < optExcitOutput.Count; index++)
			{
				if (output == optExcitOutput[index])

				return index;
			}
			return index;
		}

		private void optSystemType_Click(object sender, EventArgs e)
		{
			RadioButton optSystemType = (RadioButton)sender;
			if(optSystemType == _optSystemType_0)
			{
				optExcitOutput[4].Enabled = true;  // 'X1
				optExcitOutput[5].Enabled = false;  // 'Y1
				optExcitOutput[7].Enabled = false;  // 'X2
				optExcitOutput[8].Enabled = true;  // 'Y2
				optExcitOutput[10].Enabled = true;  // 'X3
				optExcitOutput[11].Enabled = false;  // 'Y3
				optExcitOutput[13].Enabled = false;  // 'X4
				optExcitOutput[14].Enabled = true;  // 'Y4

			}
			if (optSystemType == _optSystemType_1)
			{
				optExcitOutput[4].Enabled = false;  // 'X1
				optExcitOutput[5].Enabled = true;  // 'Y1
				optExcitOutput[7].Enabled = true;  // 'X2
				optExcitOutput[8].Enabled = false;  // 'Y2
				optExcitOutput[10].Enabled = false;  // 'X3
				optExcitOutput[11].Enabled = true;  // 'Y3
				optExcitOutput[13].Enabled = true;  // 'X4
				optExcitOutput[14].Enabled = false;  // 'Y4
			}
			if (optSystemType == _optSystemType_2)
			{
				optExcitOutput[4].Enabled = true;  // 'X1
				optExcitOutput[5].Enabled = true;  // 'Y1
				optExcitOutput[7].Enabled = true;  // 'X2
				optExcitOutput[8].Enabled = true;  // 'Y2
				optExcitOutput[10].Enabled = true;  // 'X3
				optExcitOutput[11].Enabled = true;  // 'Y3
				optExcitOutput[13].Enabled = true;  // 'X4
				optExcitOutput[14].Enabled = true;  // 'Y4
			}
			if(chkZmotors.Checked) //'Vertical Motors Present
			{
				optExcitOutput[6].Enabled = true;  // 'Z1
				optExcitOutput[9].Enabled = true;  // 'Z2
				optExcitOutput[12].Enabled = true;  // 'Z3
				optExcitOutput[15].Enabled = true;  // 'Z4
				optExcitOutput[22].Enabled = true;  // 'Z2
				optExcitOutput[23].Enabled = true;  // 'Z3
				optExcitOutput[24].Enabled = true;  // 'Z4

			}
			else  //NO Vertical Motors
			{
				optExcitOutput[6].Enabled = false;  // 'Z1
				optExcitOutput[9].Enabled = false;  // 'Z2
				optExcitOutput[12].Enabled = false;  // 'Z3
				optExcitOutput[15].Enabled = false;  // 'Z4
				optExcitOutput[22].Enabled = false;  // 'Z2
				optExcitOutput[23].Enabled = false;  // 'Z3
				optExcitOutput[24].Enabled = false;  // 'Z4
			}
		}

		private void chkZmotors_Click(object sender, EventArgs e)
		{
			if (chkZmotors.Checked) //'Vertical Motors Present
			{
				optExcitOutput[6].Enabled = true;  // 'Z1
				optExcitOutput[9].Enabled = true;  // 'Z2
				optExcitOutput[12].Enabled = true;  // 'Z3
				optExcitOutput[15].Enabled = true;  // 'Z4
				optExcitOutput[22].Enabled = true;  // 'Z2
				optExcitOutput[23].Enabled = true;  // 'Z3
				optExcitOutput[24].Enabled = true;  // 'Z4

			}
			else  //NO Vertical Motors
			{
				optExcitOutput[6].Enabled = false;  // 'Z1
				optExcitOutput[9].Enabled = false;  // 'Z2
				optExcitOutput[12].Enabled = false;  // 'Z3
				optExcitOutput[15].Enabled = false;  // 'Z4
				optExcitOutput[22].Enabled = false;  // 'Z2
				optExcitOutput[23].Enabled = false;  // 'Z3
				optExcitOutput[24].Enabled = false;  // 'Z4
			}
		}


	}
}
