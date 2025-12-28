using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace TMCAnalyzer
{
	public partial class TerminalControl : UserControl
	{
		public static HistoryWindow formHistory = null;
		object lockObj = new object();
		#region Constants
		static int x_max = 80;
		static int y_max = 25;
		#endregion

		#region Fields
		string Status = "Closed";
		private string DefaultFileName = "TMC.port";
		private string DefaultFile = string.Empty;
		private string terminalText = string.Empty;
		private bool isMenuActive = false;
		string LastSentCommand = string.Empty;
		byte static_Display_first_arg;
		byte static_Display_second_arg;
		byte static_Display_PosOfSecondArg;
		string static_Display_ESCcomStr;
		byte static_Display_commode;
		int BufferLEN;
		private int line_X_index;
		private int line_Y_index;
		int x_save;
		int y_save;
		static string[] ScreenLine = new string[31];
		static string RxLine = string.Empty;
		int LastKeyPressed;
		private static float px;
		private static float py;
		string strSpaces = "                                                                                ";
		float Font_Size;
		float Pixel_Font_Width;
		short border = 5;
		float frmH;
		float frmW;
		float m_x;
		float m_y;
		float Font_W_to_H_Ratio;
		int static_txtTerm_MouseDown_xcommand;
		int static_txtTerm_MouseDown_dot_pos;
		private Point mouse_pt_clicked = new Point(0, 0);
		private Point mouse_prev_pt_clicked = new Point(0, 0);
		private static Point char_location = new Point(0, 0);
		private string str_MouseX;
		private string str_MouseY;
		public static float Pixel_Font_Height;
		static Font the_font;
		//private static Terminal m_FormDefInstance;
		private bool m_IsInitializing = true;
		static System.Drawing.Pen pen_wht = new System.Drawing.Pen(System.Drawing.Color.White, 1);
		byte static_Timer1_Tick_flash;
		static System.Drawing.Pen pen_BLK = new System.Drawing.Pen(System.Drawing.Color.Black, 1);
		string tempText = string.Empty;
		public string char_at_mouse_click;
		TableLayoutRowStyleCollection tableLayoutRows;
		float PrevFormWidth;
		float PrevFormHeight;
		public static bool IsCOMSelected = false;

		public bool retryConnection = false;
		private string _status = "NotConnected";
		private bool IsConnecting = false;

		#endregion

		#region Properties
		public string ConnectionState
		{
			get
			{
				if (Program.ConnectedController != null)
				{
					switch (Program.ConnectedController.CommStatus)
					{
						case CommStatusStates.Connected:
							_status = "Connected";
							break;
						case CommStatusStates.DemoMode:
							_status = "DemoMode";
							break;
						case CommStatusStates.NotConnected:
							_status = "NotConnected";
							break;
						case CommStatusStates.CommError:
							_status = "Error";
							break;
					}
				}
				else
				{
					_status = "NotConnected";
				}
				return _status;
			}
			set
			{
				_status = value;
			}
		}

		public bool IsInitializing
		{
			get { return m_IsInitializing; }
			set { m_IsInitializing = value; }
		}

		/// <summary>
		/// Sets/gets a flag to tue when application is in advanced menu mode
		/// </summary>
		public bool IsMenuActive
		{
			get { return isMenuActive; }
			set { isMenuActive = value; }
		}

		public static HistoryWindow FormHistory
		{
			get
			{
				if (formHistory == null)
				{
					formHistory = new HistoryWindow();
				}
				return formHistory;
			}
		}
		#endregion

		#region Events & Delegates
		public static StatusUpdate StatusMsg;
		public delegate void StatusUpdate(string status);
		[System.Runtime.InteropServices.DllImport("user32")]
		private static extern bool HideCaret(IntPtr hWnd);
		#endregion

		public TerminalControl()
		{
			InitializeComponent();
			textBoxTerminal.SelectionLength = 0;
			textBoxTerminal.Select();
			HideCaret(textBoxTerminal.Handle);
		}

		#region Private Implementation

		#region Form event methods
		/// <summary>
		/// Form load event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TerminalControl_Load(object sender, EventArgs e)
		{
			textBoxTerminal.GotFocus += OnGotFocus;
			textBoxTerminal.LostFocus += OnLostFocus;
			SetScreen();
			//SetDefaultPort();

			timerCursorBlink.Enabled = true;

			textBoxTerminal.SelectionLength = 0;
			textBoxTerminal.Select();
			tableLayoutRows = tableLayoutPanel1.RowStyles;

			HideCaret(textBoxTerminal.Handle);

			Device.Instance.ReceivedPackets.Subscribe(DisplayRecievedData);

			this.Disposed += Terminal_Disposed;
		}

		private void Terminal_Disposed(object sender, EventArgs e)
		{
			Device.Instance.StopTimerToReadData();
		}

		private void timerCursorBlink_Tick(object sender, EventArgs e)
		{
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_BLK, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			if (static_Timer1_Tick_flash == 0)
			{
				textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
				static_Timer1_Tick_flash = 1;
			}
			else {
				textBoxTerminal.CreateGraphics().DrawRectangle(pen_BLK, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
				static_Timer1_Tick_flash = 0;
			}
			UpdateStatus();
		}

		#endregion

		#region Connect
		/// <summary>
		/// Connects to the COM port and add listeners to the ReceivedPackets and IsConnected
		/// </summary>
		/// <param name="obj"></param>
		private void Setup_NotifyToConnect(string obj, string[] settings)
		{
			if (!String.IsNullOrEmpty(obj))
			{
				if (obj.StartsWith("COM"))
				{
					Device.Instance.Connect(obj, settings);
					//Device.Instance.IsConnected.Subscribe(ConnectionStatus);
					//Device.Instance.ReceivedPackets.Subscribe(DisplayRecievedData);
					//Device.Instance.ReceivedErrorMessage.Subscribe(UpdateStatus);
					//Device.Instance.OpenCOM();
					// Device.Instance.RequestResponse(Device.Instance);
					if (Status.Equals("Closed"))
					{
						PortProblems();
					}
					UpdateStatus();
				}
			}
			else
				UpdateStatus("Not Connected.. Select a valid Port..!!");
		}
		/// <summary>
		/// Connects to the IP port
		/// </summary>
		/// <param name="obj"></param>
		private void Setup_NotifyToConnectIP(string ipAdrs, int telnetPort)
		{
			if (!String.IsNullOrEmpty(ipAdrs))
			{
				Device.Instance.IPInit();
				//Device.Instance.TelnetState.Subscribe(TelnetConnectionStatus);
				//Device.Instance.TelnetReceivedPackets.Subscribe(DisplayRecievedData);
				//Device.Instance.TelnetReceivedErrorMessage.Subscribe(UpdateStatus);
				Device.Instance.OpenIP(ipAdrs, telnetPort);

				UpdateStatus();
			}
			else {
				UpdateStatus("Not Connected.. Select a valid Port..!!");
			}
		}
		#endregion

		#region MenuItem Click events

		#region FileMenu
		private void loadSetupCntrlOToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ReadPortSetup();
		}
		private void saveSetupCntrlSToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.FormSplash.OnSaveSetupCommand();
		}
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Program.FormMain.OpenTerminalForm();
		}
		#endregion

		#region EditMenu
		private void copyToClipboardCntrlCToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CopyToClipBoard();
		}
		private void clearScreenCntrlDToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Clr_screen();
		}
		private void copyToHistoryCntrlAToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OnCopyToHistoryCommand();
		}
		#endregion

		/// <summary>
		/// Click event. Open Port setup form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void menuItemComPort_Click(object sender, EventArgs e)
		{
			Device.Instance.StopTimerToReadData();
			Program.FormMain.OpenSplashForm();
		}

		private void menuItemConnect_Click(object sender, EventArgs e)
		{
			IsConnecting = true;
			if (!ConnectionState.Equals("Connected"))
				Program.FormMain.ConnectToController(true, false);
			UpdateStatus();
			IsConnecting = false;
		}
		private void menuItemDisconnect_Click(object sender, EventArgs e)
		{
			if (!IsConnecting)
			{
				menuItemDisconnect.Enabled = false;
				Device.Instance.StopTimerToReadData();
				if (ConnectionState.Equals("Connected"))
					Program.FormMain.ConnectToController(false);
				UpdateStatus();
				menuItemDisconnect.Enabled = true;
			}
			else
			{
				return;
			}
		}
		private void menuItemCallMenu_Click(object sender, EventArgs e)
		{
			OnCallMenuCommand();
		}
		private void menuItemQuitMenu_Click(object sender, EventArgs e)
		{
			OnQuitMenuCommand();
		}
		private void menuItemHelp_Click(object sender, EventArgs e)
		{
			OnHelpCommand();
		}

		#endregion

		#region MenuItem Click event commands

		#region FileMenu
		private void OnSaveSetupCommand()
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
			if (IsCOMSelected)
			{
				stm.WriteLine("PORT " + Device.Instance.SelectedCOM);
				stm.WriteLine(Device.Instance.Settings[0] + "," + Device.Instance.Settings[1] + "," + Device.Instance.Settings[2] + "," + Device.Instance.Settings[3]);
				stm.WriteLine("Date: " + DateTime.Now.ToShortDateString());
				stm.WriteLine("Time: " + DateTime.Now.ToLongTimeString());
				stm.Close();
			}
			else
			{
				stm.WriteLine("TELNET IP: " + Device.Instance.SelectedIP);
				stm.WriteLine("TELNET PORT: " + Device.Instance.SelectedTelnetPort);
				stm.WriteLine("Date: " + DateTime.Now.ToShortDateString());
				stm.WriteLine("Time: " + DateTime.Now.ToLongTimeString());
				stm.Close();

			}
		}
		public bool OnExitCommand()
		{
			bool functionReturnValue = false;
			if (ConnectionState.Equals("Connected"))
			{
				OnQuitMenuCommand();
				Device.Instance.Dispose();
				functionReturnValue = true;
			}
			else
			{
				functionReturnValue = true;
			}
			return functionReturnValue;
		}
		#endregion

		#region Edit
		private void CopyToClipBoard()
		{
			string tempText = string.Empty;
			for (int i = 0; i < 25; i++)
			{
				tempText = tempText + ScreenLine[i].TrimEnd() + "\r\n";
			}
			Clipboard.Clear();
			Clipboard.SetText(tempText);
		}
		private void OnCopyToHistoryCommand()
		{
			string tempText = string.Empty;
			for (int i = 0; i < 25; i++)
			{
				tempText = ScreenLine[i].TrimEnd();
				FormHistory.AddHistory(tempText);
			}
		}
		#endregion
		private void OnDisconnectCommand()
		{
			Device.Instance.Dispose();
			UpdateStatus();
		}
		private void OnConnectCommand()
		{
			UpdateStatus();
		}
		private void OnCallMenuCommand()
		{
			if (Status.Contains("Closed"))
				OnConnectCommand();

			SendCommand("menu\r");
		}
		public void OnQuitMenuCommand()
		{
			if (!IsMenuActive) return;

			if (Status.Contains("Closed"))
				OnConnectCommand();

			SendCommand(Convert.ToChar(27).ToString());
			Delay_sec(0.2f);
			SendCommand(Convert.ToChar(27).ToString());
			Delay_sec(0.2f);
			SendCommand(Convert.ToChar(27).ToString());
			Delay_sec(0.2f);
			SendCommand(Convert.ToChar(27).ToString());
			Delay_sec(0.2f);
			SendCommand("q");
			LastSentCommand = "";
			Delay_sec(0.2f);
			IsMenuActive = false;
		}
		private void OnHelpCommand()
		{
			string Title = "TMC Terminal p/n 95-38913-01 rev.- @ 07-May-2016 Help";
			string message = "Emulates ANSI terminal, Connects to COM and Telnet ports.\n"
				+ "Keeps Log of communication history: open CTRL-L, add whole screen CTRL-A.\n"
				+ "Accepts keyboard input, CTRL-C, CTRL - V.\n"
				+ "Accepts mouse clicks if menu is called.\n"
				+ "Left Button chooses menu line, Right Button works as ESC.\n"
				+ "Window is resizable and automatically adjusts font size.";
			MessageBox.Show(message, Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		#endregion

		#region Screen
		/// <summary>
		/// Clearing the terminal window text
		/// </summary>
		public void Clr_screen()
		{
			int y_index = 0;
			string fill_box = "";
			for (y_index = 0; y_index <= y_max + 1; y_index++)
			{
				ScreenLine[y_index] = strSpaces;
				// assign 80 spaces
				if (y_index < y_max)
					fill_box = fill_box + strSpaces + "\r\n";
			}
			line_Y_index = 0;
			line_X_index = 0;
			py = 0;
			px = 0;
			textBoxTerminal.BeginInvoke((MethodInvoker)delegate { textBoxTerminal.Text = fill_box; });
			// textBoxTerminal.Text = fill_box;
		}
		/// <summary>
		/// Set the default size and font to the application window
		/// </summary>
		private void SetScreen()
		{
			int commapos = 0;
			Size frm_size = default(Size);
			x_max = 80; // standard screen 80 * 25
			y_max = 25;
			Pixel_Font_Height = this.Font.GetHeight(); //get or set font of control. 'FontHeight' is protected property of control
			Font_Size = 10;
			the_font = new Font("Courier New", Font_Size, FontStyle.Bold);
			this.Font = the_font;
			textBoxTerminal.Font = the_font;
			Pixel_Font_Width = measure_font_width(the_font);
			border = 2;
			frm_size = new Size((Int32)(Pixel_Font_Width * x_max + border), (Int32)(Pixel_Font_Height * y_max + border));
			frmH = frm_size.Height;
			frmW = frm_size.Width;
			commapos = (Int32)(x_max) * ((Int32)(y_max) + 6);
			strSpaces = "                                                                                ";
			Clr_screen();
			this.Show();
			set_txtBox_size();
			PrevFormHeight = frmH = this.Height;
			PrevFormWidth = frmW = this.Width;
			m_x = 0;
			m_y = 0;
			IsInitializing = false;
			textBoxTerminal.Select(0, 0);
		}
		/// <summary>
		/// Set Default size and font to the Terminal text box
		/// </summary>
		public void set_txtBox_size()
		{
			textBoxTerminal.Font = the_font;
			Pixel_Font_Width = measure_font_width(the_font);
			//DefInstance.textBoxTerminal.AutoSize = false;
			//  DefInstance.textBoxTerminal.Width = (Int32)((x_max) * Pixel_Font_Width) + 22;
			//tableLayoutPanel1.Width = (Int32)((x_max) * Pixel_Font_Width) + 22;

			// 20 for scroll bar
			// DefInstance.textBoxTerminal.Height = (Int32)((y_max + 1) * Pixel_Font_Height) + 20;
			//tableLayoutPanel1.Height = (Int32)((y_max + 1) * Pixel_Font_Height) + 42;
			//  DefInstance.textBoxTerminal.Size = new Size(frmW, frmH);
			//  DefInstance.textBoxTerminal.Location = new Point(0,24);
		}
		/// <summary>
		/// Measuring font width
		/// </summary>
		/// <returns></returns>
		public float measure_font_width(Font mFont)
		{
			float functionReturnValue = 0;
			// BECAUSE (Me.txtTerm.CreateGraphics()().MeasureString(tststr, mFont).Width) / Len(tststr) is NOT ACCURATE on text box
			// create hidden text box, put test string on it, measure coordinates of the same known character, calulate height and width
			// The client area of a control is the bounds of the control, minus the nonclient elements such as scroll bars, borders, title bars, and menus.
			Point tst_char_pos = new Point();
			TextBox tstTxtBox = new TextBox();
			tstTxtBox.Multiline = true;
			tstTxtBox.Left = 20;
			tstTxtBox.Top = 20;
			tstTxtBox.Width = 600;
			tstTxtBox.Height = 600; // if multiline = false, tstTxtBox.Height = 25, value accepted but ignored
			tstTxtBox.Font = the_font;

			//Dim tststr As String= "12345    |         |         |         |         |         |         |         |"	  'LEN=80
			//Dim tststr As String= "`1234567890-= ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz ~!@#$%^&*()_+ []\{}| ;':,./ <>?|"		  'LEN=100
			//Dim tststr As String = "`1234567890-= ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz ~!@#$%^&*()_+ []\{}| ;':,./ <>?|"		  'LEN=100
			//Dim Font_Width As Single = (Me.txtTerm.CreateGraphics()().MeasureString(tststr, mFont).Width) / Len(tststr)
			string tststr = "`1234567890-= ABCDEFGHIJKLMNOPQRSTUVWXYZ abc";
			//LEN=44
			float Font_Width = 0;
			// NOT ACCURATE ON textBox = (Me.txtTerm.CreateGraphics()().MeasureString(tststr, mFont).Width) / Len(tststr)
			tstTxtBox.Text = "0" + "\r\n" + "\r\n" + "\r\n" + "\r\n" + "\r\n" + tststr;
			//Dim tst_char As String = tstTxtBox.Text(50) 'for test, it is "Z"
			tst_char_pos = tstTxtBox.GetPositionFromCharIndex(50);
			Pixel_Font_Height = Convert.ToSingle(tst_char_pos.Y) / 5;
			//single
			Font_Width = Convert.ToSingle(tst_char_pos.X) / 39;
			Font_W_to_H_Ratio = Font_Width / Pixel_Font_Height;
			//approx 0.6		 'for "Courier New" font
			functionReturnValue = Font_Width;
			tstTxtBox.Dispose();
			return functionReturnValue;
		}
		#endregion

		#region Data Send and Receive Methods
		/// <summary>
		/// Read port setting file and try to connect with the port. Throw if any errors
		/// </summary>
		/// <param name="skipDialog"></param>
		/// <returns></returns>
		private bool ReadPortSetup(bool skipDialog = false)
		{
			bool success = Program.FormSplash.ReadPortSetup();
			UpdateStatus();
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
					if (rows[1].Split(' ')[0].Contains("PORT"))  //Com/Usb
					{
						IsCOMSelected = true;
						////COM Port number validation
						if (IsNumeric(rows[1].Split(' ')[1].Last()))
						{
							//Device.Instance.SelectedCOM = rows[1].Split(' ')[1];
						}
						else
						{
							MessageBox.Show("Port Number '" + rows[1].Split(' ')[1] + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
							sr.Close();
							return success;
						}
						//split each row with comma to get individual values
						portSettings = rows[2].Split(',');

						////COM Port settings validation
						if (IsNumeric(rows[2].Split(',')[0]))
						{
							Device.Instance.Settings = rows[2].Split(',');
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
						IsCOMSelected = false;
						string[] ipSettings = rows[1].Split(' ');
						if (ipSettings.Length < 3)
						{
							MessageBox.Show("Telnet IP is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
									MessageBox.Show("Telnet IP '" + rows[1].Split(' ')[2] + "' is not recognized", "Wrong Port Settings file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
									sr.Close();
									return success;
								}
							}
							Device.Instance.SelectedIP = rows[1].Split(' ')[2];
							string port = rows[2].Split(' ')[2];
							if (IsNumeric(port))
							{
								Device.Instance.SelectedTelnetPort = Convert.ToInt32(port);
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
		/// <summary>
		/// Display Retry Message box and updates based on selection
		/// </summary>
		/// <param name="prog_called"></param>
		private void PortProblems(string prog_called = "")
		{
			string Title = null;
			string Msg = null;
			Msg = "PC port: " + Device.Instance.SelectedCOM + " is not available, or used by other application." + "\r\n" + "Retry again / try other port? 'Cancel' will close dialog";

			if (!string.IsNullOrEmpty(prog_called))
			{
				Title = prog_called + " COM port is not available";
			}
			else {
				Title = " COM port is not available";
			}
			var answer = MessageBox.Show(Msg, Title, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);
			// User choose Retry.
			if (answer == DialogResult.Retry)
			{
				//portSetup.ExitPortSetupWindow();
				//OpenPortSetupDialog();
				retryConnection = true;
				return;
			}
			else
			{
				Status = "Not Avail";
			}
			UpdateStatus();
		}

		/// <summary>
		/// Calls Display method
		/// </summary>
		/// <param name="recievedBuffer">Received buffer from the controller</param>
		private void DisplayRecievedData(string recievedBuffer)
		{
			if (!String.IsNullOrEmpty(recievedBuffer))
			{
				Display(recievedBuffer);
			}
		}
		/// <summary>
		/// Handles displaying the received data on the terminal window in manual mode and menu mode
		/// </summary>
		/// <param name="sInBuffer"></param>
		private void Display(string sInBuffer)
		{
			lock (lockObj)
			{
				int i = 0;
				int coord = 0;
				int y_pos = 0;
				BufferLEN = sInBuffer.Length;
				if (BufferLEN == 0)
					return;
				for (i = 0; i < BufferLEN;)
				{
					string STRsym = sInBuffer.Substring(i, 1);
					char sym = Convert.ToChar(STRsym);
					//If these events are too fast, and not enough time to process and exit this sub, stack is overflown
					//        Buffer = Buffer & sInBuffer 'read whole com buffer, it overflows if not clean periodically
					int ASCsym = Convert.ToInt32(sym);
					RxLine += STRsym;
					switch (ASCsym)
					{
						// check incoming chars: is it ESC?
						case 27:
							static_Display_commode = 1;
							//if ESC, SET COMMAND MODE: read next byte(s) and interprete it(them) as command
							static_Display_ESCcomStr = STRsym;
							//begin building command string
							goto nextsym;
						// if symbol is not ESC, check, is it printable symbol?
						case 13:
							// CODE of Enter
							line_X_index = 0;
							goto nextsym;
						case 10:
							scroll_scr:
							if (!IsMenuActive)
							{
								if (RxLine.Last() == '\r') RxLine = RxLine.Remove(RxLine.Length - 1);
								if (!String.IsNullOrEmpty(RxLine))
									if (RxLine.Last() == '\n') RxLine = RxLine.Remove(RxLine.Length - 1);
								if (!String.IsNullOrEmpty(RxLine))
									if (RxLine.Last() == '\r') RxLine = RxLine.Remove(RxLine.Length - 1);
								/*textBoxTerminal.BeginInvoke((MethodInvoker)delegate {*/ FormHistory.AddHistory(RxLine); //});
							}
							RxLine = "";
							line_Y_index = line_Y_index + 1;
							// ***** CR + Line_forward *****
							if (line_Y_index < y_max + 1)
							{
								// ************ >>>>>>>>>>> scroll screen
							}
							else {
								for (y_pos = 0; y_pos <= y_max - 1; y_pos++)
								{
									ScreenLine[y_pos] = ScreenLine[y_pos + 1];
								}
								ScreenLine[y_max] = strSpaces;
								line_Y_index = y_max;
							}
							goto show_cursor;
						case 9:
							// Tab: move cursor ->, to the next position muptiple of 8
							int cursor_Xpos = line_X_index;
							line_X_index = (Int32)(((cursor_Xpos / 8) + 1) * 8);
							if (line_X_index >= x_max)
							{
								line_X_index = line_X_index - x_max;
								goto scroll_scr;
							}
							ScreenLine[line_Y_index] = ReplaceMid(ScreenLine[line_Y_index], cursor_Xpos + 0, Space(line_X_index - cursor_Xpos));
							break;
						case 8:
							// Back Space: move cursor <-, erase position, where cursor
							LastKeyPressed = 8;
							Cursor_left();
							ScreenLine[line_Y_index] = ReplaceMid(ScreenLine[line_Y_index], line_X_index + 0, " ");
							goto nextsym;
						case '\a': // 7:
							// Beep:
							System.Media.SystemSounds.Beep.Play();
							goto nextsym;
						case 0:
							// zero "" can be on error
							sym = ' ';
							STRsym = "";
							break;
						default:
							break;
					}

					if (static_Display_commode == 0)
					{
						goto put_sym;
						// NOT command, print symbol on screen and exit
						//**********************************
						// if command mode - START WORKING with command sequence
					}
					else {
						static_Display_ESCcomStr = static_Display_ESCcomStr + sym;
						// ADD new symbol to command string
						static_Display_commode = Convert.ToByte(static_Display_commode + 1);
						// increase byte counter
						if (static_Display_ESCcomStr.Substring(1, 1) != "[")
						{
							static_Display_commode = 0;
							goto put_sym;
							// NOT command, print symbol on screen and exit
							// we've captured beginning of command sequence and next byte must be part of it
						}
						else
						{
							IsMenuActive = true;// next time we call function, we would receive next byte
						}

						//3-rd received byte in command mode = sym
						if (static_Display_commode == 3)
						{
							switch (sym)
							{
								case 'K':                        // Escape sequence ESC[K - clear to end of line
							//	ClearEnd:
									var tempString = String.Empty;
									for (var k = 0; k < x_max; k++)
									{
										tempString += ' ';
									}
									ScreenLine[line_Y_index] = ScreenLine[line_Y_index].Substring(0, line_X_index) + tempString;
								goto next_pos;                         // end command mode
								case 'S':
							//	Save_pos:
									//*saves cursor position for recall later*,  == ESC[s
									x_save = line_X_index;
									y_save = line_Y_index;
									goto show_cursor;
								case 'U':
							//	Restore_pos:
									//*Return to saved cursor position*,         == ESC[u
									line_X_index = x_save;
									line_Y_index = y_save;
									goto show_cursor;
							}
							goto nextsym;
						}
						// End decoding 3-rd byte
						if (static_Display_commode < 4)
							goto nextsym;
					}
					//If commode = 0
					//If commode < 6 Then ' we've received at least 4-th and 5-th bytes
					switch (sym)
					{
						// check current position - if it is count or letter (NOT count)-exit
						case '0':
						case '1':
						case '2':
						case '3':
						case '4':
						case '5':
						case '6':
						case '7':
						case '8':
						case '9':
							// - wait for command - exit
							goto nextsym;
						//---->>>>>>>  ------->>>>>  ------>>>>>
						//break;
						default:
							// if sym is NOT count(NOT ARGUMENT), it must be COMMAND
							if (static_Display_second_arg == 0)
							{
								var tmpstr = static_Display_ESCcomStr.Substring(2, static_Display_commode - 3);
								static_Display_first_arg = Convert.ToByte(tmpstr);
								//-3? convert string from
								static_Display_PosOfSecondArg = static_Display_commode;
								//3-rd to symbol before last to integer
								// conversion for second argument
							}
							else {
								// var tmpstr = Conversion.Val(Strings.Mid(static_Display_strCom, static_Display_PosOfSecondArg + 1, static_Display_commode - static_Display_PosOfSecondArg - 1));
								var tmpstr = static_Display_ESCcomStr.Substring(static_Display_PosOfSecondArg, static_Display_commode - static_Display_PosOfSecondArg - 1);
								static_Display_second_arg = Convert.ToByte(tmpstr);
							}
							break;
					}

					switch (sym)
					{
						case 'A':
					//	Cursor_up:
							//*go_up # LINES*        == ESC[#A
							for (coord = 1; coord <= static_Display_first_arg; coord++)
							{
								Cursor_up();
							}

							break;
						case 'B':
					//	Cursor_down:
							//*go_down # LINES*      == ESC[#B
							for (coord = 1; coord <= static_Display_first_arg; coord++)
							{
								Cursor_down();
							}

							break;
						case 'C':
					//	Cursor_right:
							//*go_left #symbols*     == ESC[#C
							for (coord = 1; coord <= static_Display_first_arg; coord++)
							{
								Cursor_right();
							}

							break;
						case 'D':
					//	Cursor_left:
							//*go_right # SYMBOLS*   == ESC[#D
							for (coord = 1; coord <= static_Display_first_arg; coord++)
							{
								Cursor_left();
							}

							break;
						case 'J':
							//*clear screen & cursor home*   == ESC[2J
							Clr_screen();
							break;

						case ';':
							//*Reports current cursor line & column* ,   == ESC[#;#R
							//*put cursor in X,Y_mouse screen position ==ESC[#;#H or ESC[#;#f
							// expect second argument

							static_Display_second_arg = 1;
							// second argument will follow
							goto nextsym;
						//------>>>> ------->>>>> ------->>>>>
						// cases "H","f" & "R" - TWO ARGUMENTS
						case 'H':
						case 'f': // -!- used ToUpper, 'f' will be 'F'
					//	set_scr_pos:
							//==ESC[#;#H or ESC[#;#f
							line_X_index = static_Display_second_arg - 1;
							line_Y_index = static_Display_first_arg - 1;
							break;
						case 'R':
					//	Report_pos:
							//== ESC[#,#R
							goto show_cursor;
					}
					//End If ' end if commode < 6 then - decoding first argument
					goto show_cursor;
					put_sym:
					//End If
					// if yes - put char on the screen  position line_X_index,yx, move cursor -->
					if (ScreenLine[line_Y_index].Length > line_X_index)
					{
						var tmpstr = ScreenLine[line_Y_index].Substring(line_X_index, 1);
						ScreenLine[line_Y_index] = ReplaceMid(ScreenLine[line_Y_index], line_X_index + 0, sym.ToString());
						// change line in array
					}
					next_pos:

					if (line_X_index < x_max)
					{
						line_X_index = line_X_index + 1;
					}
					else {
						if (line_Y_index < y_max - 1)
						{
							line_Y_index = line_Y_index + 1;
							line_X_index = 0;
							//************  here should be scroll
						}
						else {
							line_X_index = 0;
							line_Y_index = 0;
						}
					}
					show_cursor:
					static_Display_commode = 0;
					static_Display_ESCcomStr = "";
					static_Display_PosOfSecondArg = 0;
					static_Display_first_arg = 0;
					static_Display_second_arg = 0;
					if (line_Y_index > y_max | line_Y_index < 0)
					{
						MessageBox.Show("wrong Y position=" + line_Y_index.ToString());
						goto nextsym;
					}
					if (line_X_index > x_max | line_X_index < 0)
					{
						MessageBox.Show("wrong X position=" + line_X_index.ToString());
					}
					nextsym:
					i++;
				}
				//-!- in filter design mode , ScreenLine(5) should be "Select:            "
				//-!- but in has ZERO right after : asc(mid(ScreenLine(5),8,1))=0; asc(mid(ScreenLine(5),9,1))=32
				//-!- so string has earlier termination char, which stops updating txtTerm.Text

				// // GARY: Limit Text Box Size
				string tstr = "";
				for (i = 0; i <= y_max; i++)
				{
					tstr = tstr + ScreenLine[i] + "\r\n";
				}
				lock (lockObj)
				{
					textBoxTerminal.BeginInvoke((MethodInvoker)delegate { textBoxTerminal.Text = tstr; });
				}

				return;
		//	error_lbl:
				MessageBox.Show("exception");
			}
		}
		/// <summary>
		/// Sends the command to the controller based on selected connection
		/// </summary>
		/// <param name="command"></param>
		private void SendCommand(string command)
		{
			if (Program.ValidateScopeCheck)
			{
				if (Device.Instance.PortIsOpen)
				{
					//Print the command to history window if it is not menu mode
					if (command.Contains("\r") || IsMenuActive)
					{
						DisplayRecievedData("\r\n");
						Device.Instance.Send(LastSentCommand + command);
						if (!IsMenuActive && FormHistory.IsSentCmdChecked)
							FormHistory.AddHistory("->" + LastSentCommand);
						LastSentCommand = "";
					}
					else
					{
						LastSentCommand = LastSentCommand + command;
						DisplayRecievedData(command);
					}
				}
			}
			else
			{
				if (Device.Instance.PortIsOpen)
				{
					Device.Instance.Send(command);
					//Print the command to history window if it is not menu mode
					if (command.Contains("\r"))
					{
						if (!IsMenuActive && FormHistory.IsSentCmdChecked)
							FormHistory.AddHistory("->" + LastSentCommand);
						LastSentCommand = "";
					}
					else
					{
						LastSentCommand = LastSentCommand + command;
					}
				}
			}
		}

		public void Delay_sec(float delay_time)
		{
			var CmdStartTime = DateTime.Now;
			var TimeStamp = DateTime.Now;
			TimeSpan WaitTime;
			do {
				System.Windows.Forms.Application.DoEvents();
				WaitTime = DateTime.Now - TimeStamp;
			} while (WaitTime.TotalSeconds < delay_time);
		}
		#endregion

		#region Status Update Methods

		/// <summary>
		/// Set the COM/Usb connection Status
		/// </summary>
		/// <param name="connected">if it is true, connection is open</param>
		private void ConnectionStatus(bool connected)
		{
			if (connected)
			{
				Status = "Open";
			}
			else
			{
				Status = "Closed";
			}
		}
		/// <summary>
		/// Set the Telnetb connection Status
		/// </summary>
		/// <param name="connectionState">if it is true, connection is open</param>
		private void TelnetConnectionStatus(int connectionState)
		{
			if (connectionState == 0)
			{
				Status = "Connecting";
			}
			else if (connectionState == 1)
			{
				Status = "Open";
			}
			else
			{
				Status = "Closed";
			}
		}

		/// <summary>
		/// update the status in the title bar
		/// </summary>
		/// <param name="c"></param>
		public void UpdateStatus()
		{
			try
			{
				string message = string.Empty;

				string portSetting = string.Empty;
				if (Program.FormSplash.IsPortUSB)
					portSetting = Program.FormSplash.s_Baud.Text + "," + Device.Instance.Settings[1] + "," + Device.Instance.Settings[2] + "," + Device.Instance.Settings[3];
				else
					portSetting = "port: " + Program.FormSplash.txtPort.Text;
				if (ConnectionState.Equals("Connected"))
					message = "Terminal <==> " + Device.Instance.SelectedCOM + ":  " + ConnectionState + "; at " + portSetting;
				else
					message = "Terminal <-X-> " + Device.Instance.SelectedCOM + "  " + ConnectionState;


				//if (Program.FormSplash.IsPortUSB)
				//{
				//    string strPortSetting = Device.Instance.Settings[0] + "," + Device.Instance.Settings[1] + "," + Device.Instance.Settings[2] + "," + Device.Instance.Settings[3];
				//    if (Status.Equals("Open"))
				//        message = "Terminal <==> " + Device.Instance.SelectedCOM + ":  " + Status + "; at " + strPortSetting;
				//    else
				//        message = "Terminal <-X-> " + Device.Instance.SelectedCOM + "  " + Status;
				//}
				//else
				//{
				//    if (Status.Equals("Connecting"))
				//        message = "Terminal <==> IP: " + Device.Instance.SelectedIP + ",  port: " + Device.Instance.SelectedTelnetPort + "," + Status;
				//    else
				//    {
				//        if (Status.Equals("Open"))
				//            message = "Terminal <==> IP: " + Device.Instance.SelectedIP + ",  port: " + Device._connectedDevice.SelectedTelnetPort + "," + Status;
				//        else
				//            message = "Terminal <==> IP: " + Device.Instance.SelectedIP + ",  port: " + Device.Instance.SelectedTelnetPort + "," + Status;
				//    }
				//}
				if (StatusMsg != null)
				{
					StatusMsg(message);
				}
			}
			catch (Exception ex)
			{

			}
		}

		/// <summary>
		/// testing purpose
		/// </summary>
		/// <param name="c"></param>
		private void UpdateStatus(string status)
		{
			// MessageBox.Show(status);
		}

		#endregion

		/// <summary>
		/// Set the default port setting once the application is opened
		/// </summary>
		private void SetDefaultPort()
		{
			DefaultFile = GetAbsoluteDirectoryName(System.IO.Path.GetDirectoryName(
			System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)) + "\\" + DefaultFileName;
			if (File.Exists(DefaultFile))
			{
				if (!ReadPortSetup(true))
				{
					if (!IsCOMSelected)
					{
						Status = "Closed";
					}
				}
			}
			else
				Status = "Closed";
			UpdateStatus();
		}

		#region Arrow Keys Methods

		private void Cursor_home()
		{
			line_X_index = 0;
			line_Y_index = 0;
		}
		private void Cursor_left()
		{
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			line_X_index = line_X_index - 1;
			//if cursor was on first column
			if (line_X_index < 0)
			{
				//- it goes up and to the end of line
				if (line_Y_index > 0)
				{
					line_Y_index = line_Y_index - 1;
					line_X_index = x_max;
				}
				else {
					line_X_index = 0;
					line_Y_index = 0;
				}
				// cursor was NOT on first column
			}
			else {
			}
			//if it was not "back space"
			if (LastKeyPressed != 8)
			{
				SendCommand(Convert.ToChar(27).ToString() + "[D");
			}
			else {
				LastKeyPressed = 0;
			}
		}
		private void Cursor_up()
		{
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			if (line_Y_index > 1)
				line_Y_index = line_Y_index - 1;
			SendCommand(Convert.ToChar(27).ToString() + "[A");
		}

		private void Cursor_right()
		{
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			line_X_index = line_X_index + 1;
			//if cursor was on last column
			if (line_X_index >= x_max)
			{
				//- it goes down and to the beginning of line
				if (line_Y_index < y_max)
				{
					line_Y_index = line_Y_index + 1;
					line_X_index = 0;
				}
				else {
					line_X_index = 0;
					line_Y_index = 0;
				}
				// cursor was NOT on first column
			}
			else {
			}
			SendCommand(Convert.ToChar(27).ToString() + "[C");
		}
		private void Cursor_down()
		{
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			if (line_Y_index < y_max)
				line_Y_index = line_Y_index + 1;
			SendCommand(Convert.ToChar(27).ToString() + "[B");
		}

		#endregion

		#region Extra Utils
		private string ReplaceMid(string org_str, int startPos, string ReplacementString)
		{
			var tmpstr = string.Empty;
			int HowManyChars = ReplacementString.Length;
			if (org_str.Length > startPos)
			{
				tmpstr = org_str.Substring(0, startPos);     // preserve beginning
				tmpstr = tmpstr + ReplacementString;       // beginnig with replacement
				var end_str = string.Empty;
				if (org_str.Length > (startPos + HowManyChars))
				{
					end_str = org_str.Substring(startPos + HowManyChars);     // preserve ending
				}
				tmpstr = tmpstr + end_str;       // beginnig + replacement + ending
				org_str = tmpstr;
			}

			return org_str;
		}
		private string Space(int HowManySpaces)
		{
			var tmpstr = string.Empty;
			for (var n = 0; n < HowManySpaces; n++) tmpstr += " ";
			return tmpstr;
		}
		private static bool IsNumeric(object Expression)
		{
			double retNum;
			bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
			return isNum;
		}
		private bool IsMenuChoice(ref string str_in)
		{
			bool functionReturnValue = false;
			string menu_char = null;
			menu_char = (str_in);
			//add hex

			//if ((Convert.ToByte(menu_char) >= 0) && (Convert.ToByte(menu_char) <= 9)
			//    | menu_char == "U"
			//    | (Convert.ToByte(menu_char) >= Convert.ToByte("A") && Convert.ToByte(menu_char) <= Convert.ToByte("N")))
			if (IsNumeric(menu_char) || menu_char == "U" || ((Int32)(Convert.ToChar(menu_char)) >= (Int32)('A') && ((Int32)(Convert.ToChar(menu_char)) <= (Int32)('N'))))
			{
				functionReturnValue = true;
			}
			else {
				functionReturnValue = false;
			}
			return functionReturnValue;
		}
		/// <summary>
		/// GetAbsoluteDirectoryName
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static string GetAbsoluteDirectoryName(string path)
		{
			if (path.StartsWith("file:\\"))
				path = path.Substring(6);
			return path;
		}

		private void CopyClipBoardToScreen()
		{
			string tempText = string.Empty;
			string singleChar = string.Empty;
			tempText = Clipboard.GetText();
			for (int i = 0; i < tempText.Length; i++)
			{
				if (ScreenLine[line_Y_index].Length > line_X_index)
				{
					singleChar = tempText.Substring(i, 1);
					SendCommand(singleChar);
				}
			}
		}

		#endregion

		#region Textbox terminal window event methods
		private void textBoxTerminal_KeyPress(object sender, KeyPressEventArgs e)
		{
			string[] IgnoreList = { "\b", " " };  //avoid backspace and space
			foreach (var item in IgnoreList)
			{
				if (Convert.ToString(e.KeyChar.ToString()) == item)
					return;
			}

			int KeyAscii = Convert.ToInt32(e.KeyChar);
			switch (KeyAscii)
			{
				case 13: //Enter
					SendCommand(e.KeyChar.ToString());
					return;
				case 3: //CTRL-C
					CopyToClipBoard();
					return;
				case 22: //CTRL-V
					CopyClipBoardToScreen();
					return;
				case 12: //CTRL-L
					ShowHistoryWindow();
					e.Handled = true;
					return;
				case 19: //CTRL-S
					OnSaveSetupCommand();
					e.Handled = true;
					return;
				case 15: //CTRL-O
					ReadPortSetup();
					e.Handled = true;
					return;
				case 1: //CTRL-A
					OnCopyToHistoryCommand();
					e.Handled = true;  // suppress "ding" sound
					return;
				case 4: //CTRL-D
					Clr_screen();
					e.Handled = true;
					return;
			}
			SendCommand(e.KeyChar.ToString());
			e.Handled = true;
		}

		private void textBoxTerminal_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.F1:
					OnHelpCommand();
					return;
				case Keys.Home:
					Cursor_home();
					return;
				case Keys.Left:
					Cursor_left();
					return;
				case Keys.Right:
					Cursor_right();
					return;
				case Keys.Up:
					Cursor_up();
					return;
				case Keys.Down:
					Cursor_down();
					return;
				case Keys.Back:
					{
						SendCommand(Convert.ToChar(8).ToString());
						textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
					}
					return;
				case Keys.Space:
					SendCommand(Convert.ToChar(32).ToString());
					return;
			}
			e.Handled = true;
		}

		private void textBoxTerminal_MouseDown(object sender, MouseEventArgs e)
		{
			int btnCode = Convert.ToInt32(e.Button);
			int Button = btnCode / 0x100000;
			int shiftCode = Convert.ToInt32(System.Windows.Forms.Control.ModifierKeys);
			int Shift = shiftCode / 0x10000;
			float X_mouse = (e.X);
			float Y_mouse = (e.Y);
			string[] menu_char = new string[5];
			// one char to check if it is menu choice item: 0...9, A...F (may be up to K)
			bool[] menu_item = new bool[5];
			// if true, the char before ". " is menu choice, i.e: "0. " or "D. " NOTE: "d. " IS NOT Menu choice (small letter)
			// Create a Pen object white.

			//  mouse_released = false;
			// erase previous cursor mark
			//textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, px, py, (Pixel_Font_Width), (Pixel_Font_Height));
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_wht, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			// textBoxTerminal.TabStop = false;
			HideCaret(textBoxTerminal.Handle);
			mouse_prev_pt_clicked = mouse_pt_clicked;
			mouse_pt_clicked = e.Location;
			px = X_mouse;
			py = Y_mouse;
			// store current x,y as previous coordinates
			m_x = Convert.ToInt16(X_mouse / (Pixel_Font_Width));
			//convert mouse click coordinates into rectangular cursor coordinates
			//m_y = Int(Y_mouse / (Pixel_Font_Height))
			char_at_mouse_click = Convert.ToString(textBoxTerminal.GetCharFromPosition(mouse_pt_clicked));
			//test
			m_y = textBoxTerminal.GetLineFromCharIndex(textBoxTerminal.GetCharIndexFromPosition(mouse_pt_clicked));
			char_location = textBoxTerminal.GetPositionFromCharIndex((textBoxTerminal.GetCharIndexFromPosition(mouse_pt_clicked)));
			m_x = textBoxTerminal.GetCharIndexFromPosition(mouse_pt_clicked) - textBoxTerminal.GetFirstCharIndexOfCurrentLine();
			if (m_x < 0)
				m_x = 0;
			if (m_y > y_max - 1)
				m_y = y_max - 1;
			//put mouse within working area
			float txtFont_height = 0;
			if (m_y > 0)
			{
				txtFont_height = char_location.Y / (m_y);
			}
			else {
				txtFont_height = Pixel_Font_Height;
			}
			Pixel_Font_Height = txtFont_height;
			//test
			str_MouseX = "X=" + X_mouse;
			str_MouseY = "Y=" + Y_mouse;
			if (Button == 2)
			{
				// RIGHT button is ESC
				SendCommand("\u001b");
				textBoxTerminal.ContextMenu = new ContextMenu();
				return;
			}

			if (m_y > 0)
			{
				// mouse click sends ESC
				//var ESC_pos = ScreenLine[(Int32)(m_y)].IndexOf('\x1b'); // no esc returns -1
				var ESC_pos = ScreenLine[(Int32)(m_y)].IndexOf("<ESC>");// no esc returns -1
				if (ESC_pos >= 0)
				{
					SendCommand(Convert.ToChar(27).ToString());
					return;
				}
				static_txtTerm_MouseDown_xcommand = ScreenLine[(Int32)(m_y)].IndexOf(". ", 1) - 1;
			}

			/// MENU DISCOVERY
			// MENU DISCOVERED, search the second, third, etc instance of ". "
			if (static_txtTerm_MouseDown_xcommand > 0)
			{
				menu_char[0] = (ScreenLine[(Int32)(m_y)].Substring(static_txtTerm_MouseDown_xcommand, 1));
				menu_item[0] = IsMenuChoice(ref menu_char[0]);
				//menu choice
				static_txtTerm_MouseDown_dot_pos = ScreenLine[(Int32)(m_y)].LastIndexOf(". ", static_txtTerm_MouseDown_xcommand + 2) - 1;
				//binary search & compare
				// there is second instance of ". ", but it can be inside text, like: "  1. FLOAT vs. DOCKpos, rel.Micr  3200"
				if (static_txtTerm_MouseDown_dot_pos > 0)
				{
					if (static_txtTerm_MouseDown_dot_pos > m_x)
						goto menu_item_found;
					// if click is to the left of the menu, when there is choice and subchoice in THW COLUMNS, and user clicks on FIRST subchoice
					menu_char[1] = ScreenLine[(Int32)(m_y)].Substring(static_txtTerm_MouseDown_dot_pos, 1);
					menu_item[1] = IsMenuChoice(ref menu_char[1]);
					//menu choice or NOT menu choice, ". " inside text, search next
					static_txtTerm_MouseDown_xcommand = static_txtTerm_MouseDown_dot_pos;
					//, search next instance
					static_txtTerm_MouseDown_dot_pos = ScreenLine[(Int32)(m_y)].IndexOf(". ", static_txtTerm_MouseDown_xcommand + 2) - 1;
					//binary search & compare
					// there is third instance of ". "
					if (static_txtTerm_MouseDown_dot_pos > 0)
					{
						if (static_txtTerm_MouseDown_dot_pos > m_x)
							goto menu_item_found;
						// if click is to the left of the menu, when there is choice and subchoice in THW COLUMNS, and user clicks on FIRST subchoice
						menu_char[2] = ScreenLine[(Int32)(m_y)].Substring(static_txtTerm_MouseDown_dot_pos, 1);
						menu_item[2] = IsMenuChoice(ref menu_char[2]);
						//menu choice
						static_txtTerm_MouseDown_xcommand = static_txtTerm_MouseDown_dot_pos;
						static_txtTerm_MouseDown_dot_pos = ScreenLine[(Int32)(m_y)].IndexOf(". ", static_txtTerm_MouseDown_xcommand + 2) - 1;
						//binary search & compare
						// there is forth instance of ". "
						if (static_txtTerm_MouseDown_dot_pos > 0)
						{
							if (static_txtTerm_MouseDown_dot_pos > m_x)
								goto menu_item_found;
							//if click is to the left of the menu, when there is choice and subchoice in THW COLUMNS, and user clicks on FIRST subchoice
							// menu_char(3) = (Mid(ScreenLine(CInt(m_y)), xcommand, 1))        ' The next line is the right one. This one is the older/wrong one.
							menu_char[3] = ScreenLine[(Int32)(m_y)].Substring(static_txtTerm_MouseDown_dot_pos, 1);
							menu_item[3] = IsMenuChoice(ref menu_char[3]);
							//menu choice
						}
					}
				}
				menu_item_found:
				for (static_txtTerm_MouseDown_dot_pos = 3; static_txtTerm_MouseDown_dot_pos >= 0; static_txtTerm_MouseDown_dot_pos--)
				{
					if (menu_item[static_txtTerm_MouseDown_dot_pos] == true)
					{
						SendCommand((menu_char[static_txtTerm_MouseDown_dot_pos]));
						// str_Help = "symbol sent: " + menu_char[static_txtTerm_MouseDown_dot_pos];
						break; // TODO: might not be correct. Was : Exit For
						//sens only the right-most char which is a menu choice, and exit
					}
				}
				return;
				// do not update place where to print if it is menu
			}
			//END OF MENU DISCOVERY
			line_X_index = (Int32)(m_x);
			// save coordinates and char on screen - it might be updated in menu
			line_Y_index = (Int32)(m_y);
			textBoxTerminal.CreateGraphics().DrawRectangle(pen_BLK, ((line_X_index) * Pixel_Font_Width) + border, line_Y_index * Pixel_Font_Height, (Pixel_Font_Width), (Pixel_Font_Height));
			px = char_location.X;
			py = char_location.Y;
		}

		private void textBoxTerminal_Click(object sender, EventArgs e)
		{
			((TextBox)sender).SelectionLength = 0;
		}

		private void OnGotFocus(object sender, EventArgs e)
		{
			textBoxTerminal.Select(0, 0);
			HideCaret(textBoxTerminal.Handle);
			timerCursorBlink.Enabled = true;
		}

		private void OnLostFocus(object sender, EventArgs e)
		{
			textBoxTerminal.Select(0, 0);
			HideCaret(textBoxTerminal.Handle);
			timerCursorBlink.Enabled = false;
		}
		#endregion

		#endregion

		private void textBoxTerminal_SizeChanged(object sender, EventArgs e)
		{
			if (IsInitializing == true)
				return;
			if (this.ParentForm != null && this.ParentForm.WindowState == FormWindowState.Maximized)
				this.ParentForm.WindowState = FormWindowState.Normal;
			//Prevent go maximized 1 - Minimized or 2 - Maximized.
			if (this.ParentForm == null || this.ParentForm.WindowState != FormWindowState.Normal)
				return;

			Control myControl = this;
			float baseWidth = 1200;
			float baseHeight = 864;
			float baseFontSize = 16.4f;

			if (PrevFormWidth != myControl.Width)
			{
				float percentage = (myControl.Width - baseWidth) / baseWidth;
				Font_Size = baseFontSize + baseFontSize * percentage;
				if (Font_Size > 20) Font_Size = 20;
				PrevFormWidth = myControl.Width;
			}
			else if (PrevFormHeight != myControl.Height)
			{
				float percentage = (myControl.Height - baseHeight) / baseHeight;
				Font_Size = baseFontSize + baseFontSize * percentage;
				if (Font_Size < 1) Font_Size = 1;
				PrevFormHeight = myControl.Height;
			}

			the_font = new Font("Courier New", Font_Size, FontStyle.Bold);
			Pixel_Font_Width = measure_font_width(the_font);
			textBoxTerminal.Font = the_font;
			HideCaret(textBoxTerminal.Handle);

			//textBoxTerminal.Show();
			//this.Invalidate();
		}

		private void menuItemHistory_Click(object sender, EventArgs e)
		{
			ShowHistoryWindow();
		}

		private void ShowHistoryWindow()
		{
			FormHistory.Show();
			FormHistory.Visible = true;
			FormHistory.BringToFront();
		}

		public static void CloseHistoryWindow()
		{
			//FormHistory.Hide();
			FormHistory.Visible = false;
		}

		#region public Implementation
		public void MainForm_SendCallBack(string command, string response)
		{
			DisplayRecievedData(command + "\r\n");
			DisplayRecievedData(response + "\r\n");
			textBoxTerminal.BeginInvoke((MethodInvoker)delegate { textBoxTerminal.Update(); });
		}
		#endregion
	}
}
