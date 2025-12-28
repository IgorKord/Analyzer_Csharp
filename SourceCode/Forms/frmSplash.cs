using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TMCAnalyzer
{
	public partial class frmSplash : Form
	{
		#region Fields
		private bool _isPortUSB = true;
		private string _selctedCOMPort = string.Empty;
		public bool IsDemo = false;
		public bool Windows_is_64bit = false;
		static readonly object _lockObj = new object();
		#endregion

		#region Properties
		public bool IsPortUSB
		{
			get
			{
				return _isPortUSB;
			}

			set
			{
				_isPortUSB = value;
			}
		}

		public string SelctedCOMPort
		{
			get
			{
				return s_port.Text;
				//if (s_port.SelectedItem != null)
				//    return s_port.SelectedItem.ToString().Trim();
				//else
				//    return string.Empty;
			}
		}



		#endregion

		#region Delegates
		#endregion

		#region Constructor

		public frmSplash()
		{
			InitializeComponent();


		}


		#endregion

		#region Private Methods
		public void SearchCOMPOrts()
		{
			var COMPorts = SerialPort.GetPortNames();
			s_port.Items.Clear();
			s_port.Items.AddRange(COMPorts);

			if (s_port.Items.Count > 1)
				Program.DualPortController = true;
			else
				Program.DualPortController = false;
		}

		private void OpenMainForm()
		{
			if (/*form already active?*/ Program.FormMain != null)
			{
				// Make the already running form visible and give it focus.
				Program.FormMain.Select();
				Program.FormMain.Activate();
				Program.FormMain.Show();
				return;
			}

			// Create and show the form.
			Program.FormMain = new formMain();
			Program.FormMain.Visible = true;
			Program.FormMain.Activate();
		}
		#endregion

		#region Public Methods

		#endregion

		private void frmSplash_FormClosed(object sender, FormClosedEventArgs e)
		{
			Program.FormSplash = null;
		}

		private void _OptConnection_Click(object sender, EventArgs e)
		{
			if (_OptConnection_0.Checked)  //USb is selected
			{
				IsPortUSB = true;
				SearchCOMPOrts();
				FrameCOM.Visible = true;
				FrameTelnet.Visible = false;
			}
			else  //Telnet is selected
			{
				IsPortUSB = false;
				FrameCOM.Visible = false;
				FrameTelnet.Visible = true;
			}
		}

		private void cmdConnect_Click(object sender, EventArgs e)
		{
				if (IsPortUSB)  //Com
				{
					Program.ConnectionType = (int)Program.ConnType.ConnCOMport;
					//TMC_Scope.Program.MainFormConnType = 1;
					// formMain.LastSelectedController = s_port.SelectedIndex;
					if (String.IsNullOrEmpty(SelctedCOMPort))
					{
						MessageBox.Show("Please Select the port and settings..!");
						return;
					}
					if (String.IsNullOrEmpty(s_Baud.Text))
					{
						s_Baud.SelectedIndex = 4;
					}

					//TMC_Scope.Program.MainFormComPortName = SelctedCOMPort;
				}
				else //Telnet
				{
					//Program.DisconnectScope_ConnSwitch();
					Program.ConnectionType = (int)Program.ConnType.ConnTelnet;
					//TMC_Scope.Program.MainFormConnType = 0;
					if (String.IsNullOrEmpty(txtHost.Text) || String.IsNullOrEmpty(txtPort.Text))
					{
						MessageBox.Show("Please Select the port and settings..!");
						return;
					}

			   //telnet
			}
			if (Program.FormMain == null)
			{
				OpenMainForm();
				Program.FormMain.ConnectToController(true, true);


			}
			else
			{
				Program.FormMain.ConnectToController(true, false);
			}

			Program.FormSplash.Hide();

		}

		private void s_Baud_SelectedIndexChanged(object sender, EventArgs e)
		{
			s_Baud.Text = s_Baud.SelectedItem.ToString();
		}

		private void ComboIPlist_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtHost.Text = ComboIPlist.SelectedItem.ToString();
		}

		private void txtHost_TextChanged(object sender, EventArgs e)
		{

			if (txtHost.Text.Contains("192.168."))
				lbl_IP_tip.Text = "Public Wireless IP address usually starts from 192.168.1.";
			if (txtHost.Text.Contains("192.168.168."))
				lbl_IP_tip.Text = "Custom IP address starts from 192.168.168.";
			if (txtHost.Text.Contains("169.254."))
				lbl_IP_tip.Text = "Auto generated IP address usually starts from 169.254.";
			if (txtHost.Text.Contains("169.254.20.20"))
				lbl_IP_tip.Text = "DC-2020 default static IP address";
		}

		private void frmSplash_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(e.CloseReason != CloseReason.ApplicationExitCall && Program.FormMain != null)
			{
				Program.FormSplash.Hide();
				e.Cancel = true;
			}
		}

		private void frmSplash_Load(object sender, EventArgs e)
		{
			SearchCOMPOrts();
			if (s_port.Items.Count > 1) s_port.SelectedIndex = 0;
			Windows_is_64bit = Environment.Is64BitOperatingSystem;
		}

		private void cmdLoadPort_Click(object sender, EventArgs e)
		{
			ReadPortSetup();
		}

		/// <summary>
		/// Read port setting file and try to connect with the port. THrow if any errors
		/// </summary>
		/// <param name="skipDialog"></param>
		/// <returns></returns>
		public bool ReadPortSetup(bool skipDialog = false)
		{
			bool success = false;
			string DataFileName = string.Empty;
			if (!skipDialog)
			{
				OpenFileDialog ReadDialog = new OpenFileDialog();
				ReadDialog.Title = "Choose the Port Setup file to load";
				string Application_Dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).ToString();
				ReadDialog.InitialDirectory = Application_Dir;
				ReadDialog.DefaultExt = "port";
				ReadDialog.Filter = "Data File (*.port)|*.port|All files (*.*)|*.*";
				if (ReadDialog.ShowDialog() == DialogResult.Cancel)
				{
					return success;
				}
				DataFileName = ReadDialog.FileName;
			}
			else
			{
				//DataFileName = DefaultFile;
			}
			if (!File.Exists(DataFileName))
			{
				MessageBox.Show("File does not Exists");
				return success;
			}
			if (GetPortSettingsFromFile(DataFileName))
			{
				if (Program.FormMain == null)
					OpenMainForm();
				if (Program.formTerminal != null)
					Device.Instance.StopTimerToReadData();
				Program.FormMain.ConnectToController(true, true);
				Program.FormSplash.Hide();
				success = true;

			}
			return success;
		}

		/// <summary>
		/// Get the port settings. throw error message box if any format errors
		/// </summary>
		/// <param name="Filename"></param>
		/// <returns></returns>
		private bool GetPortSettingsFromFile(string Filename)
		{
			bool success = false;
			string PortName = string.Empty;
			string[] portSettings = new string[4];
			using (StreamReader sr = new StreamReader(Filename))
			{
				string Fulltext = sr.ReadToEnd().ToString();
				string[] rows = Fulltext.Split('\n'); //split full file text into rows
				///Trim and remove \r from each row
				for (int i = 0; i < rows.Length; i++)
				{
					if (rows[i].Contains("\r"))
					{
						rows[i] = rows[i].Replace("\r", "");
						rows[i] = rows[i].Trim();
					}
				}
				if (rows[0].Contains("TMC PORT SETUP"))
				{
					string[] portNum = rows[1].Split(' ');
					if (portNum[0].Contains("PORT"))  //Com/Usb
					{
					   // portSetup.IsCOMConnectionSelected = true;
						Program.FormSplash._OptConnection_0.Checked = true;
						////COM Port number validation
						if (portNum.Length > 1)
						{
							if (IsNumeric(rows[1].Split(' ')[1].Last()))
							{
								PortName = rows[1].Split(' ')[1];
							}
							else
							{
								MessageBox.Show("Port Number '" + rows[1].Split(' ')[1] + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								sr.Close();
								return success;
							}
						}
						else
						{
							MessageBox.Show("Port Number is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							sr.Close();
							return success;
						}
						if (!FindString(PortName))
						{
							MessageBox.Show("Port Number '" + PortName + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							sr.Close();
							return success;
						}
						//split each row with comma to get individual values
						portSettings = rows[2].Split(',');

						////COM Port settings validation
						if (IsNumeric(rows[2].Split(',')[0]))
						{
							Device.Instance.Settings = rows[2].Split(',');
							Program.FormSplash.s_Baud.SelectedItem = Device.Instance.Settings[0];
						//	Program.FormSplash.s_Baud.Text = Device.Instance.Settings[0];
						}
						else
						{
							MessageBox.Show("Port Settings Line '" + rows[2] + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							sr.Close();
							return success;
						}
					}
					else  //Telnet
					{
						//portSetup.IsCOMConnectionSelected = false;
						Program.FormSplash._OptConnection_1.Checked = true;
						string[] ipSettings = rows[1].Split(' ');
						if (ipSettings.Length < 3)
						{
							MessageBox.Show("Tellnet IP is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							sr.Close();
							return success;
						}
						else
						{
							string[] Ip = rows[1].Split(' ')[2].Split('.');
							foreach (var value in Ip)
							{
								if (!IsNumeric(value))
								{
									MessageBox.Show("Tellnet IP '" + rows[1].Split(' ')[2] + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									sr.Close();
									return success;
								}
							}
							Program.FormSplash.txtHost.Text = rows[1].Split(' ')[2];
							string port = rows[2].Split(' ')[2];
							if (IsNumeric(port))
							{
								Program.FormSplash.txtPort.Text = port;
							}
							else
							{
								MessageBox.Show("Telnet port '" + port + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								sr.Close();
								return success;
							}
						}
					}
				}
				else
				{
					MessageBox.Show("File Signature 'TMC PORT SETUP' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					sr.Close();   // FileSystem.FileClose(1);
					return success;
				}
				sr.Close();
				success = true;
			}

			return success;
		}

		private bool FindString(string searchString)
		{
			// Ensure we have a proper string to search for.
			if (searchString != string.Empty)
			{
				// Find the item in the list and store the index to the item.
				int index = Program.FormSplash.s_port.FindString(searchString);
				// Determine if a valid index is returned. Select the item if it is valid.
				if (index != -1)
				{
					Program.FormSplash.s_port.SelectedIndex = index;
					//formMain.LastSelectedController = index;
					return true;
				}
				else
					return false;
			}
			return false;
		}

		private static bool IsNumeric(object Expression)
		{
			double retNum;
			bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
			return isNum;
		}

		public void OnSaveSetupCommand()
		{
			string SaveFileName = null;
			string FileName = null;
			string[] PortSettings = Device.Instance.Settings;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "Choose name for port setup";
			string applicationDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).ToString();
			saveFileDialog.InitialDirectory = applicationDir;
			saveFileDialog.DefaultExt = "port";
			saveFileDialog.Filter = "Data File (*.port)|*.port|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
			{
				return;
			}
			else { }
			SaveFileName = saveFileDialog.FileName;
			FileName = Path.GetFileName(SaveFileName);
			var /*stream*/ stm = new System.IO.StreamWriter(SaveFileName);
			stm.WriteLine("TMC PORT SETUP " + FileName);
			if (Program.FormSplash.IsPortUSB)
			{
				stm.WriteLine("PORT " + Program.FormSplash.s_port.SelectedItem);
				stm.WriteLine(s_Baud.SelectedText + "," + Device.Instance.Settings[1] + "," + Device.Instance.Settings[2] + "," + Device.Instance.Settings[3]);
				stm.WriteLine("Date: " + DateTime.Now.ToShortDateString());
				stm.WriteLine("Time: " + DateTime.Now.ToLongTimeString());
				stm.Close();
			}
			else
			{
				stm.WriteLine("TELNET IP: " + Program.FormSplash.txtHost.Text);
				stm.WriteLine("TELNET PORT: " + Program.FormSplash.txtPort.Text);
				stm.WriteLine("Date: " + DateTime.Now.ToShortDateString());
				stm.WriteLine("Time: " + DateTime.Now.ToLongTimeString());
				stm.Close();
			}
		}

		private void cmdSaveSetup_Click(object sender, EventArgs e)
		{
			OnSaveSetupCommand();
		}

		private void CmdDEMO_Click(object sender, EventArgs e)
		{
			Program.ConnectionType = (int)Program.ConnType.ConnDEMO;
			//TMC_Scope.Program.MainFormConnType = 3;
			IsDemo = true;
			_OptConnection_0.Checked = true;
			Program.NoControllerDemoMode = true;     //artem 060518
		//	Program.FormSplash.IsPortUSB = true;

			if (Program.FormMain == null)
			{
				OpenMainForm();
			}
			Program.FormMain.ConnectToController(true, true);
			Program.FormSplash.Hide();
		}
	}
}
