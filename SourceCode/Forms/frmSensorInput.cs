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
	public partial class frmSensorInput : Form
	{
		public int inputChosen;
		public int input2ndChosen;
		public List<RadioButton> listOfRadio = new List<RadioButton>();
	   // public string LTFtestInputName;
		public int DiagnosticAxis;

		public frmSensorInput()
		{
			InitializeComponent();
			InitializeControls();


		}

		List<RadioButton> optSensorInput = new List<RadioButton>();
		private void InitializeControls()
		{
			optSensorInput.Clear();
			optSensorInput.Add(_optSensorInput_0); optSensorInput.Add(_optSensorInput_1); optSensorInput.Add(_optSensorInput_2);
			optSensorInput.Add(_optSensorInput_3); optSensorInput.Add(_optSensorInput_4); optSensorInput.Add(_optSensorInput_5);
			optSensorInput.Add(_optSensorInput_6); optSensorInput.Add(_optSensorInput_7); optSensorInput.Add(_optSensorInput_8);
			optSensorInput.Add(_optSensorInput_9); optSensorInput.Add(_optSensorInput_10); optSensorInput.Add(_optSensorInput_11);
			optSensorInput.Add(_optSensorInput_12); optSensorInput.Add(_optSensorInput_13); optSensorInput.Add(_optSensorInput_14);
			optSensorInput.Add(_optSensorInput_15); optSensorInput.Add(_optSensorInput_16); optSensorInput.Add(_optSensorInput_17);
			optSensorInput.Add(_optSensorInput_18); optSensorInput.Add(_optSensorInput_19); optSensorInput.Add(_optSensorInput_20);
			optSensorInput.Add(_optSensorInput_21); optSensorInput.Add(_optSensorInput_22); optSensorInput.Add(_optSensorInput_23);
			optSensorInput.Add(_optSensorInput_24); optSensorInput.Add(_optSensorInput_25); optSensorInput.Add(_optSensorInput_26);
			optSensorInput.Add(_optSensorInput_27); optSensorInput.Add(_optSensorInput_28); optSensorInput.Add(_optSensorInput_29);
			optSensorInput.Add(_optSensorInput_30); optSensorInput.Add(_optSensorInput_31); optSensorInput.Add(_optSensorInput_32);
			optSensorInput.Add(_optSensorInput_33); optSensorInput.Add(_optSensorInput_34); optSensorInput.Add(_optSensorInput_35);
			optSensorInput.Add(_optSensorInput_36); optSensorInput.Add(_optSensorInput_37); optSensorInput.Add(_optSensorInput_38);
			optSensorInput.Add(_optSensorInput_39); optSensorInput.Add(_optSensorInput_40); optSensorInput.Add(_optSensorInput_41);
			optSensorInput.Add(_optSensorInput_42); optSensorInput.Add(_optSensorInput_43); optSensorInput.Add(_optSensorInput_44);
			optSensorInput.Add(_optSensorInput_45); optSensorInput.Add(_optSensorInput_46); optSensorInput.Add(_optSensorInput_47);

			foreach (var item in optSensorInput)
			{
				MethodInfo m = typeof(RadioButton).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
				if (m != null)
				{
					m.Invoke(item, new object[] { ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true });
				}

				item.MouseDoubleClick += new MouseEventHandler(radioButton_MouseDoubleClick);

			}
		}

		private void radioButton_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			LeavingForm();
		}

		private void frmSensorInput_FormClosing(object sender, FormClosingEventArgs e)
		{
			LeavingForm();

			 if (e.CloseReason != CloseReason.ApplicationExitCall)
			{
				//if(  sender.GetType() == typeof(Form1
				e.Cancel = true;
				this.Hide();
			}
		}

		private void LeavingForm()
		{
			UndateInputs();
			this.Hide();
		}


		private void frmSensorInput_Activated(object sender, EventArgs e)
		{
			if(Program.FormMain.SecondInputSelection)
			{
				this.Text = "Choose 1st Test Input. Now = " + optSensorInput[inputChosen].Text;
			}
			else
			{
				this.Text = "Choose 2nd Test Input. Now = " + optSensorInput[input2ndChosen].Text;
			}
		}

		private void frmSensorInput_Load(object sender, EventArgs e)
		{
			if(Program.FormMain.SecondInputSelection)
			{
				Program.LTFtestInputName = optSensorInput[inputChosen].Tag.ToString();
				this.Text = "Choose 1st Test Input. Now = " + optSensorInput[inputChosen].Text;
			}
			else
			{
				Program.LTFtestInputName = optSensorInput[input2ndChosen].Tag.ToString();
				this.Text = "Choose 2nd Test Input. Now = " + optSensorInput[inputChosen].Text;
			}
		}

		private void frmSensorInput_Click(object sender, EventArgs e)
		{
			Program.LTFtestInputName = optSensorInput[inputChosen].Tag.ToString();
			this.Text = "Choose Test Input. Now = " + optSensorInput[inputChosen].Text;
		}

		private void frmSensorInput_Deactivate(object sender, EventArgs e)
		{
			UndateInputs();
		}

		private void UndateInputs()
		{
			string ch_name = string.Empty;
			if (input2ndChosen != 23)
				ch_name = optSensorInput[input2ndChosen].Text;
			else
				ch_name = "None";

			if(!Program.FormMain.SecondInputSelection)
			{
				Program.LTFtestInputName = optSensorInput[inputChosen].Tag.ToString();
				this.Text = "Choos 1st Test Input. Now = " + optSensorInput[inputChosen].Text;
				if (inputChosen >= 32) DiagnosticAxis = inputChosen - 32;
				Program.FormMain.CmdChooseInputSignal.Text = "Input = " + optSensorInput[inputChosen].Text;

			}
			else
			{
				Program.LTFtestInputName = optSensorInput[input2ndChosen].Tag.ToString();
				this.Text = "Choos 2nd e Test Input. Now = " + optSensorInput[input2ndChosen].Text;
				Program.FormMain.CmdChoose2ndInputSignal.Text = "Inp2 = " + ch_name;
			}
			optSensorInput[inputChosen].Checked = true;
		}

		private void optSensorInput_Click(object sender, EventArgs e)
		{
			RadioButton input = (RadioButton)sender;

			if (!Program.FormMain.SecondInputSelection)
			{
				inputChosen = GetIndex(input);
				this.Text = "Choose 1st Test Input. Now = " + optSensorInput[inputChosen].Text;
			}
			else
			{
				input2ndChosen = GetIndex(input);
				this.Text = "Choose 2nd Test Input. Now = " + optSensorInput[input2ndChosen].Text;
			}
		}

		private int GetIndex(RadioButton output)
		{
			int index = 0;
			for (index = 0; index < optSensorInput.Count; index++)
			{
				if (output == optSensorInput[index])

					return index;
			}
			return index;
		}

		private void optSystemType_Click(object sender, EventArgs e)
		{
			RadioButton optSystemType = (RadioButton)sender;
			if (optSystemType == _optSystemType_0)
			{
				if(chkHprox.Checked)
				{
					optSensorInput[0].Enabled = true;
					optSensorInput[1].Enabled = false;
					optSensorInput[3].Enabled = false;
					optSensorInput[4].Enabled = true;
					optSensorInput[6].Enabled = true;
					optSensorInput[7].Enabled = false;
				}
				else
				{
					optSensorInput[0].Enabled = false;
					optSensorInput[1].Enabled = false;
					optSensorInput[3].Enabled = false;
					optSensorInput[4].Enabled = false;
					optSensorInput[6].Enabled = false;
					optSensorInput[7].Enabled = false;
				}
				if(chkHgeophones.Checked)
				{
					optSensorInput[9].Enabled = true;
					optSensorInput[10].Enabled = false;
					optSensorInput[12].Enabled = false;
					optSensorInput[13].Enabled = true;
					optSensorInput[15].Enabled = true;
					optSensorInput[16].Enabled = false;
				}
				else
				{
					optSensorInput[9].Enabled = false;
					optSensorInput[10].Enabled = false;
					optSensorInput[12].Enabled = false;
					optSensorInput[13].Enabled = false;
					optSensorInput[15].Enabled = false;
					optSensorInput[16].Enabled = false;
				}
			}


			if (optSystemType == _optSystemType_1)
			{
				if (chkHprox.Checked)
				{
					optSensorInput[0].Enabled = false;
					optSensorInput[1].Enabled = true;
					optSensorInput[3].Enabled = true;
					optSensorInput[4].Enabled = false;
					optSensorInput[6].Enabled = false;
					optSensorInput[7].Enabled = true;
				}
				else
				{
					optSensorInput[0].Enabled = false;
					optSensorInput[1].Enabled = false;
					optSensorInput[3].Enabled = false;
					optSensorInput[4].Enabled = false;
					optSensorInput[6].Enabled = false;
					optSensorInput[7].Enabled = false;
				}
				if (chkHgeophones.Checked)
				{
					optSensorInput[9].Enabled = false;
					optSensorInput[10].Enabled = true;
					optSensorInput[12].Enabled = true;
					optSensorInput[13].Enabled = false;
					optSensorInput[15].Enabled = false;
					optSensorInput[16].Enabled = true;
				}
				else
				{
					optSensorInput[9].Enabled = false;
					optSensorInput[10].Enabled = false;
					optSensorInput[12].Enabled = false;
					optSensorInput[13].Enabled = false;
					optSensorInput[15].Enabled = false;
					optSensorInput[16].Enabled = false;
				}
			}
			if (optSystemType == _optSystemType_2)
			{
				if (chkHprox.Checked)
				{
					optSensorInput[0].Enabled = true;
					optSensorInput[1].Enabled = true;
					optSensorInput[3].Enabled = true;
					optSensorInput[4].Enabled = false;
					optSensorInput[6].Enabled = true;
					optSensorInput[7].Enabled = true;
				}
				else
				{
					optSensorInput[0].Enabled = false;
					optSensorInput[1].Enabled = false;
					optSensorInput[3].Enabled = false;
					optSensorInput[4].Enabled = false;
					optSensorInput[6].Enabled = false;
					optSensorInput[7].Enabled = false;
				}
				if (chkHgeophones.Checked)
				{
					optSensorInput[9].Enabled = true;
					optSensorInput[10].Enabled = true;
					optSensorInput[12].Enabled = true;
					optSensorInput[13].Enabled = true;
					optSensorInput[15].Enabled = true;
					optSensorInput[16].Enabled = true;
				}
				else
				{
					optSensorInput[9].Enabled = false;
					optSensorInput[10].Enabled = false;
					optSensorInput[12].Enabled = false;
					optSensorInput[13].Enabled = false;
					optSensorInput[15].Enabled = false;
					optSensorInput[16].Enabled = false;
				}
			}
			if (chkZgeophones.Checked)
			{
				optSensorInput[11].Enabled = true;
				optSensorInput[14].Enabled = true;
				optSensorInput[17].Enabled = true;
				optSensorInput[41].Enabled = true;
				optSensorInput[42].Enabled = true;
				optSensorInput[43].Enabled = true;
			}
			else
			{
				optSensorInput[11].Enabled = false;
				optSensorInput[14].Enabled = false;
				optSensorInput[17].Enabled = false;
				optSensorInput[41].Enabled = false;
				optSensorInput[42].Enabled = false;
				optSensorInput[43].Enabled = false;
			}
		}

		private void chkHprox_CheckedChanged(object sender, EventArgs e)
		{
			if (chkHprox.Checked)
			{
				optSensorInput[0].Enabled = true;
				optSensorInput[1].Enabled = true;
				optSensorInput[3].Enabled = true;
				optSensorInput[4].Enabled = true;
				optSensorInput[6].Enabled = true;
				optSensorInput[7].Enabled = true;
				optSensorInput[32].Enabled = true;
				optSensorInput[33].Enabled = true;
				optSensorInput[34].Enabled = true;
			}
			else
			{
				optSensorInput[0].Enabled = false;
				optSensorInput[1].Enabled = false;
				optSensorInput[3].Enabled = false;
				optSensorInput[4].Enabled = false;
				optSensorInput[6].Enabled = false;
				optSensorInput[7].Enabled = false;
				optSensorInput[32].Enabled = false;
				optSensorInput[33].Enabled = false;
				optSensorInput[34].Enabled = false;
			}
		}

		private void chkHgeophones_CheckedChanged(object sender, EventArgs e)
		{
			if (chkHgeophones.Checked)
			{
				optSensorInput[9].Enabled = true;
				optSensorInput[10].Enabled = true;
				optSensorInput[12].Enabled = true;
				optSensorInput[13].Enabled = true;
				optSensorInput[15].Enabled = true;
				optSensorInput[16].Enabled = true;
				optSensorInput[38].Enabled = true;
				optSensorInput[39].Enabled = true;
				optSensorInput[40].Enabled = true;
			}
			else
			{
				optSensorInput[9].Enabled = false;
				optSensorInput[10].Enabled = false;
				optSensorInput[12].Enabled = false;
				optSensorInput[13].Enabled = false;
				optSensorInput[15].Enabled = false;
				optSensorInput[16].Enabled = false;
				optSensorInput[38].Enabled = false;
				optSensorInput[39].Enabled = false;
				optSensorInput[40].Enabled = false;
			}
		}

		private void chkZgeophones_CheckedChanged(object sender, EventArgs e)
		{
			if (chkZgeophones.Checked)
			{
				optSensorInput[11].Enabled = true;
				optSensorInput[14].Enabled = true;
				optSensorInput[17].Enabled = true;
				optSensorInput[41].Enabled = true;
				optSensorInput[42].Enabled = true;
				optSensorInput[43].Enabled = true;
			}
			else
			{
				optSensorInput[11].Enabled = false;
				optSensorInput[14].Enabled = false;
				optSensorInput[17].Enabled = false;
				optSensorInput[41].Enabled = false;
				optSensorInput[42].Enabled = false;
				optSensorInput[43].Enabled = false;
			}
		}
	}
}
