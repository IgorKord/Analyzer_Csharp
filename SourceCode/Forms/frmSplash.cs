using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TMCAnalyzer
{
	public partial class frmSplash : Form
	{
		#region Fields
		private bool _isPortUSB = true;
		public bool IsDemo = false;
		public bool Windows_is_64bit = false;
		static readonly object _lockObj = new object();
		#endregion

		#region Properties
		public bool IsPortUSB
		{
			get { return _isPortUSB; }
			set { _isPortUSB = value; }
		}

		public string SelctedCOMPort
		{
			get
			{
				return s_port?.Text ?? string.Empty;
			}
		}
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
			try
			{
				var COMPorts = SerialPort.GetPortNames();
				Array.Sort(COMPorts);
				s_port.Items.Clear();
				s_port.Items.AddRange(COMPorts);

				Program.DualPortController = s_port.Items.Count > 1;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error enumerating COM ports: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void OpenMainForm()
		{
			if ( Program.FormMain != null) // form already active?
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

		/// <summary>
		/// After loading settings from file, update UI to reflect whether saved port/IP is present
		/// Similar to VB6 update_ports_from_file
		/// </summary>
		private void UpdatePortsFromLoadedFile()
		{
			// If telnet was selected (GetPortSettingsFromFile sets _OptConnection_1.Checked), enable connect
			if (_OptConnection_1.Checked)
			{
				cmdConnect.Enabled = true;
				// show tip
				lbl_IP_tip.Text = string.IsNullOrEmpty(txtHost.Text) ? lbl_IP_tip.Text : lbl_IP_tip.Text;
				return;
			}

			// Otherwise handle COM saved value
			var portText = SelctedCOMPort;
			if (!string.IsNullOrEmpty(portText))
			{
				// Try to find the saved port in current list
				int index = s_port.FindString(portText);
				if (index != -1)
				{
					s_port.SelectedIndex = index;
					s_port.BackColor = SystemColors.Window; // white
					cmdConnect.Enabled = true;
				}
				else
				{
					// saved port not present on this machine
					s_port.BackColor = Color.FromArgb(0xC0, 0xC0, 0xFF); // light orange-ish like VB6 used
					cmdConnect.Enabled = false;
				}
			}
		}
		#endregion

		private void frmSplash_FormClosed(object sender, FormClosedEventArgs e)
		{
			Program.FormSplash = null;
		}

		private void _OptConnection_Click(object sender, EventArgs e)
		{
			if (_OptConnection_0.Checked)  //USB is selected
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
				if (String.IsNullOrEmpty(s_Baud?.Text))
				{
					if (s_Baud.Items.Count > 4) s_Baud.SelectedIndex = 4;
					else if (s_Baud.Items.Count > 0) s_Baud.SelectedIndex = 0;
				}

					//TMC_Scope.Program.MainFormComPortName = SelctedCOMPort;
			} // USB
			else //Telnet
			{
				//Program.DisconnectScope_ConnSwitch();
				Program.ConnectionType = (int)Program.ConnType.ConnTelnet;
				//TMC_Scope.Program.MainFormConnType = 0;
				if (String.IsNullOrEmpty(txtHost?.Text) || String.IsNullOrEmpty(txtPort?.Text))
				{
					MessageBox.Show("Please Select the port and settings..!");
					return;
				}
			} // telnet

			if (Program.FormMain == null)
			{
				OpenMainForm();
				Program.FormMain.ConnectToController(true, true);
			}
			else
			{
				Program.FormMain.ConnectToController(true, false);
			}

			this.Hide();
		}

		private void s_Baud_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (s_Baud.SelectedItem != null)
				s_Baud.Text = s_Baud.SelectedItem.ToString();
		}

		private void ComboIPlist_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ComboIPlist.SelectedItem != null)
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
			if (e.CloseReason != CloseReason.ApplicationExitCall && Program.FormMain != null)
			{
				this.Hide();
				e.Cancel = true;
			}
		}

		private void frmSplash_Load(object sender, EventArgs e)
		{
			// Populate ports first, so GetPortSettingsFromFile can find saved COM
			SearchCOMPOrts();
			if (s_port.Items.Count > 0) s_port.SelectedIndex = 0;
			Windows_is_64bit = Environment.Is64BitOperatingSystem;

			// Try to load last-used port settings saved in TMC.port (VB6 used App.Path + "\TMC.port")
			try
			{
				string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;
				string tmcPath = Path.Combine(appDir, "TMC.port");
				if (File.Exists(tmcPath))
				{
					// Populate controls from file, but do NOT auto-connect here (let user click Connect)
					GetPortSettingsFromFile(tmcPath);

					// Update UI state similar to VB6 update_ports_from_file
					UpdatePortsFromLoadedFile();
				}
			}
			catch (Exception ex)
			{
				// don't block startup — log or show non-fatal message
				MessageBox.Show("Error loading saved port settings: " + ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void cmdLoadPort_Click(object sender, EventArgs e)
		{
			ReadPortSetup();
		}

		/// <summary>
		/// Read port setting file and try to connect with the port. Throw if any errors
		/// </summary>
		/// <param name="skipDialog"></param>
		/// <returns></returns>
		public bool ReadPortSetup(bool skipDialog = false)
		{
			bool success = false;
			string DataFileName = string.Empty;
			try
			{
				if (!skipDialog)
				{
					using (OpenFileDialog ReadDialog = new OpenFileDialog())
					{
						ReadDialog.Title = "Choose the Port Setup file to load";
						string ApplicationDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
						if (!string.IsNullOrEmpty(ApplicationDir))
							ReadDialog.InitialDirectory = ApplicationDir;
						ReadDialog.DefaultExt = "port";
						ReadDialog.Filter = "Data File (*.port)|*.port|All files (*.*)|*.*";
						if (ReadDialog.ShowDialog() == DialogResult.Cancel)
						{
							return success;
						}
						DataFileName = ReadDialog.FileName;
					}
				}
				else
				{
					// DataFileName = DefaultFile;
				}

				if (!File.Exists(DataFileName))
				{
					MessageBox.Show("File does not exist");
					return success;
				}

				if (GetPortSettingsFromFile(DataFileName))
				{
					// Note: when invoked interactively via the Load button we keep the existing behavior of opening main form and connecting.
					if (!skipDialog)
					{
						if (Program.FormMain == null)
							OpenMainForm();
						if (Program.formTerminal != null)
							Device.Instance.StopTimerToReadData();
						Program.FormMain.ConnectToController(true, true);
						this.Hide();
					}
					success = true;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error reading port setup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			try
			{
				// Extract only the file name from full path to show in MessageBox titles
				string FileNameOnly = Path.GetFileName(Filename);
				string Wrong_Port_Settings_file_name = "Wrong Port Settings file: " + FileNameOnly;

				if (!File.Exists(Filename))
				{
					MessageBox.Show($"File '{FileNameOnly}' does not exist", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return success;
				}

				string[] rows = File.ReadAllLines(Filename, Encoding.Default)
									.Select(r => r.Trim('\r', '\n').Trim()).ToArray();
				///Trim and remove \r from each row
				if (rows.Length == 0)
				{
					MessageBox.Show("File is empty", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return success;
				}

				if (rows[0].Contains("TMC PORT SETUP"))
				{
					string[] portNum = rows[1].Split(' ');
					if (portNum[0].Contains("PORT"))  //Com/Usb
					{
						// portSetup.IsCOMConnectionSelected = true;
						this._OptConnection_0.Checked = true;
						////COM Port number validation
						if (portNum.Length > 1)
						{
							if (IsNumeric(rows[1].Split(' ')[1].Last()))
							{
								string PortName = rows[1].Split(' ')[1];
								if (!FindString(PortName))
								{
									MessageBox.Show("Port Number '" + PortName + "' is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									return success;
								}
							}
							else
							{
								MessageBox.Show("Port Number '" + portNum[1] + "' is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return success;
							}
						}
						else
						{
							MessageBox.Show("Port Number is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return success;
						}

						if (rows.Length < 3)
						{
							MessageBox.Show("Port settings lines are missing", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return success;
						}
						//split each row with comma to get individual values
						string[] portSettings = rows[2].Split(',');

						////COM Port settings validation
						if (portSettings.Length > 0 && IsNumeric(portSettings[0]))
						{
							Device.Instance.Settings = portSettings;
							// set the selected baud safely
							if (s_Baud.Items.Contains(portSettings[0]))
								s_Baud.SelectedItem = portSettings[0];
							else
								s_Baud.Text = portSettings[0]; // fallback
						}
						else
						{
							MessageBox.Show("Port Settings Line '" + rows[2] + "' is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return success;
						}
					}
					else  //Telnet
					{
						//portSetup.IsCOMConnectionSelected = false;
						this._OptConnection_1.Checked = true;
						string[] ipSettings = rows[1].Split(' ');
						if (ipSettings.Length < 3)
						{
							MessageBox.Show("Telnet IP is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							return success;
						}
						else
						{
							string[] Ip = ipSettings[2].Split('.');
							foreach (var value in Ip)
							{
								if (!IsNumeric(value))
								{
									MessageBox.Show("Telnet IP '" + ipSettings[2] + "' is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									return success;
								}
							}
							this.txtHost.Text = ipSettings[2];
							string port = rows[2].Split(' ').Length > 2 ? rows[2].Split(' ')[2] : string.Empty;
							if (IsNumeric(port))
							{
								this.txtPort.Text = port;
							}
							else
							{
								MessageBox.Show("Telnet port '" + port + "' is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
								return success;
							}
						}
					}
				}
				else
				{
					MessageBox.Show("File Signature 'TMC PORT SETUP' is not recognized", Wrong_Port_Settings_file_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return success;
				}

				success = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error parsing port settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			try
			{
				// Read selected UI values or fall back to saved device settings
				var portSelected = s_port?.SelectedItem?.ToString() ?? s_port?.Text ?? "COM?";
				var baud = s_Baud?.SelectedItem?.ToString() ?? s_Baud?.Text ?? (Device.Instance.Settings.Length > 0 ? Device.Instance.Settings[0] : "115200");
				var parityText = s_Parity?.SelectedItem?.ToString() ?? (Device.Instance.Settings.Length > 1 ? Device.Instance.Settings[1] : "None");
				var dataBits = s_Length?.SelectedItem?.ToString() ?? (Device.Instance.Settings.Length > 2 ? Device.Instance.Settings[2] : "8");
				var stopBitsRaw = s_stop?.SelectedItem?.ToString() ?? (Device.Instance.Settings.Length > 3 ? Device.Instance.Settings[3] : "1");

				// Normalize parity to single-letter code similar to VB6: None->n, Even->e, Odd->o, Mark->m, Space->s
				char parityChar = 'n';
				if (!string.IsNullOrWhiteSpace(parityText))
				{
					var p = parityText.Trim().ToLower();
					if (p.StartsWith("n")) parityChar = 'n';
					else if (p.StartsWith("e")) parityChar = 'e';
					else if (p.StartsWith("o")) parityChar = 'o';
					else if (p.StartsWith("m")) parityChar = 'm';
					else if (p.StartsWith("s")) parityChar = 's';
					else parityChar = p[0];
				}

				// Normalize stop bits to numeric string (1,2,...)
				string stopBits = "1";
				if (!string.IsNullOrWhiteSpace(stopBitsRaw))
				{
					// try parse integer first
					if (int.TryParse(stopBitsRaw, out int sb))
					{
						stopBits = sb.ToString();
					}
					else
					{
						// map common word forms
						var sbLower = stopBitsRaw.Trim().ToLower();
						if (sbLower.StartsWith("one")) stopBits = "1";
						else if (sbLower.StartsWith("two")) stopBits = "2";
						else
						{
							// fallback: extract digits if present
							var digits = new string(sbLower.Where(char.IsDigit).ToArray());
							if (!string.IsNullOrEmpty(digits)) stopBits = digits;
						}
					}
				}

				// Build a sensible default filename: TMC_{PORT}_{BAUD}.port
				var safePort = portSelected.Replace(" ", "_");
				var defaultFileName = $"TMC_{safePort}_{baud}.port";

				using (SaveFileDialog saveFileDialog = new SaveFileDialog())
				{
					saveFileDialog.Title = "Choose name for port setup";
					string applicationDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
					if (!string.IsNullOrEmpty(applicationDir))
						saveFileDialog.InitialDirectory = applicationDir;
					saveFileDialog.DefaultExt = "port";
					saveFileDialog.FileName = defaultFileName;
					saveFileDialog.Filter = "Data File (*.port)|*.port|All files (*.*)|*.*";

					if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
						return;

					var SaveFileName = saveFileDialog.FileName;
					var FileNameOnly = Path.GetFileName(SaveFileName);

					using (var stm = new System.IO.StreamWriter(SaveFileName, false, Encoding.UTF8))
					{
						stm.WriteLine("TMC PORT SETUP " + FileNameOnly);
						if (this.IsPortUSB)
						{
							// Use the selected port and normalized settings
							var portValue = portSelected;
							stm.WriteLine("PORT " + portValue);

							// Compose settings exactly in VB6-like order: baud,parity,dataBits,stopBits
							stm.WriteLine($"{baud},{parityChar},{dataBits},{stopBits}");

							// Use consistent date/time formatting: MM-dd-yyyy and HH:mm:ss
							stm.WriteLine("Date: " + DateTime.Now.ToString("MM-dd-yyyy"));
							stm.WriteLine("Time: " + DateTime.Now.ToString("HH:mm:ss"));
						}
						else
						{
							stm.WriteLine("TELNET IP: " + this.txtHost.Text);
							stm.WriteLine("TELNET PORT: " + this.txtPort.Text);
							stm.WriteLine("Date: " + DateTime.Now.ToString("MM-dd-yyyy"));
							stm.WriteLine("Time: " + DateTime.Now.ToString("HH:mm:ss"));
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error saving port setup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
