namespace TMCAnalyzer
{
	partial class frmSplash
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSplash));
			this._Label1_4 = new System.Windows.Forms.Label();
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.CmdDEMO = new System.Windows.Forms.Button();
			this.s_port = new System.Windows.Forms.ComboBox();
			this.s_Baud = new System.Windows.Forms.ComboBox();
			this.s_Length = new System.Windows.Forms.ComboBox();
			this.cmdSaveSetup = new System.Windows.Forms.Button();
			this.cmdLoadPort = new System.Windows.Forms.Button();
			this.s_Parity = new System.Windows.Forms.ComboBox();
			this.s_stop = new System.Windows.Forms.ComboBox();
			this.lblVersion = new System.Windows.Forms.Label();
			this.cmdConnect = new System.Windows.Forms.Button();
			this._Label1_1 = new System.Windows.Forms.Label();
			this.txtHost = new System.Windows.Forms.TextBox();
			this.ComboIPlist = new System.Windows.Forms.ComboBox();
			this._Label1_6 = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this._Label1_5 = new System.Windows.Forms.Label();
			this._Label1_10 = new System.Windows.Forms.Label();
			this._Label1_8 = new System.Windows.Forms.Label();
			this.lblWarning = new System.Windows.Forms.Label();
			this.dlgCommonDialogPrint = new System.Windows.Forms.PrintDialog();
			this.dlgCommonDialogColor = new System.Windows.Forms.ColorDialog();
			this.dlgCommonDialogFont = new System.Windows.Forms.FontDialog();
			this.dlgCommonDialogSave = new System.Windows.Forms.SaveFileDialog();
			this.lblInternalVersion = new System.Windows.Forms.Label();
			this._Label1_7 = new System.Windows.Forms.Label();
			this.dlgCommonDialogOpen = new System.Windows.Forms.OpenFileDialog();
			this.FrameCOM = new System.Windows.Forms.GroupBox();
			this._Label1_9 = new System.Windows.Forms.Label();
			this._OptConnection_0 = new System.Windows.Forms.RadioButton();
			this.lbl_IP_tip = new System.Windows.Forms.Label();
			this._OptConnection_1 = new System.Windows.Forms.RadioButton();
			this.FrameTelnet = new System.Windows.Forms.GroupBox();
			this.Frame1 = new System.Windows.Forms.GroupBox();
			this.Image2 = new System.Windows.Forms.PictureBox();
			this._imgLogo_1 = new System.Windows.Forms.PictureBox();
			this._imgLogo_0 = new System.Windows.Forms.PictureBox();
			this.FrameCOM.SuspendLayout();
			this.FrameTelnet.SuspendLayout();
			this.Frame1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Image2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._imgLogo_1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._imgLogo_0)).BeginInit();
			this.SuspendLayout();
			// 
			// _Label1_4
			// 
			this._Label1_4.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_4.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_4.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_4.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_4.Location = new System.Drawing.Point(24, 51);
			this._Label1_4.Name = "_Label1_4";
			this._Label1_4.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_4.Size = new System.Drawing.Size(65, 17);
			this._Label1_4.TabIndex = 18;
			this._Label1_4.Text = "Select Baud";
			this._Label1_4.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// CmdDEMO
			// 
			this.CmdDEMO.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.CmdDEMO.Cursor = System.Windows.Forms.Cursors.Default;
			this.CmdDEMO.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.CmdDEMO.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CmdDEMO.Location = new System.Drawing.Point(12, 291);
			this.CmdDEMO.Name = "CmdDEMO";
			this.CmdDEMO.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.CmdDEMO.Size = new System.Drawing.Size(91, 46);
			this.CmdDEMO.TabIndex = 29;
			this.CmdDEMO.Text = "DEMO mode";
			this.ToolTip1.SetToolTip(this.CmdDEMO, "No serial connection to controller");
			this.CmdDEMO.UseVisualStyleBackColor = false;
			this.CmdDEMO.Click += new System.EventHandler(this.CmdDEMO_Click);
			// 
			// s_port
			// 
			this.s_port.BackColor = System.Drawing.SystemColors.Window;
			this.s_port.Cursor = System.Windows.Forms.Cursors.Default;
			this.s_port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.s_port.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.s_port.ForeColor = System.Drawing.SystemColors.WindowText;
			this.s_port.Location = new System.Drawing.Point(105, 18);
			this.s_port.Name = "s_port";
			this.s_port.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.s_port.Size = new System.Drawing.Size(65, 22);
			this.s_port.TabIndex = 15;
			this.ToolTip1.SetToolTip(this.s_port, "valid range COM1-COM16");
			// 
			// s_Baud
			// 
			this.s_Baud.BackColor = System.Drawing.SystemColors.Window;
			this.s_Baud.Cursor = System.Windows.Forms.Cursors.Default;
			this.s_Baud.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.s_Baud.ForeColor = System.Drawing.SystemColors.WindowText;
			this.s_Baud.Items.AddRange(new object[] {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200",
            "230400"});
			this.s_Baud.Location = new System.Drawing.Point(105, 48);
			this.s_Baud.Name = "s_Baud";
			this.s_Baud.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.s_Baud.Size = new System.Drawing.Size(65, 22);
			this.s_Baud.TabIndex = 16;
			this.s_Baud.Text = "115200";
			this.ToolTip1.SetToolTip(this.s_Baud, "default=115200");
			this.s_Baud.SelectedIndexChanged += new System.EventHandler(this.s_Baud_SelectedIndexChanged);
			// 
			// s_Length
			// 
			this.s_Length.BackColor = System.Drawing.SystemColors.Window;
			this.s_Length.Cursor = System.Windows.Forms.Cursors.Default;
			this.s_Length.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.s_Length.ForeColor = System.Drawing.SystemColors.WindowText;
			this.s_Length.Items.AddRange(new object[] {
            "8",
            "7",
            "6",
            "5"});
			this.s_Length.Location = new System.Drawing.Point(105, 105);
			this.s_Length.Name = "s_Length";
			this.s_Length.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.s_Length.Size = new System.Drawing.Size(65, 22);
			this.s_Length.TabIndex = 14;
			this.s_Length.Text = "8";
			this.ToolTip1.SetToolTip(this.s_Length, "default=8");
			// 
			// cmdSaveSetup
			// 
			this.cmdSaveSetup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
			this.cmdSaveSetup.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdSaveSetup.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.cmdSaveSetup.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdSaveSetup.Location = new System.Drawing.Point(117, 234);
			this.cmdSaveSetup.Name = "cmdSaveSetup";
			this.cmdSaveSetup.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdSaveSetup.Size = new System.Drawing.Size(91, 46);
			this.cmdSaveSetup.TabIndex = 6;
			this.cmdSaveSetup.Text = "Save Port Setup";
			this.ToolTip1.SetToolTip(this.cmdSaveSetup, "Save Port type");
			this.cmdSaveSetup.UseVisualStyleBackColor = false;
			this.cmdSaveSetup.Click += new System.EventHandler(this.cmdSaveSetup_Click);
			// 
			// cmdLoadPort
			// 
			this.cmdLoadPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.cmdLoadPort.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdLoadPort.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.cmdLoadPort.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdLoadPort.Location = new System.Drawing.Point(12, 234);
			this.cmdLoadPort.Name = "cmdLoadPort";
			this.cmdLoadPort.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdLoadPort.Size = new System.Drawing.Size(91, 46);
			this.cmdLoadPort.TabIndex = 5;
			this.cmdLoadPort.Text = " Load Port Setup";
			this.ToolTip1.SetToolTip(this.cmdLoadPort, "Load saved Port type");
			this.cmdLoadPort.UseVisualStyleBackColor = false;
			this.cmdLoadPort.Click += new System.EventHandler(this.cmdLoadPort_Click);
			// 
			// s_Parity
			// 
			this.s_Parity.BackColor = System.Drawing.SystemColors.Window;
			this.s_Parity.Cursor = System.Windows.Forms.Cursors.Default;
			this.s_Parity.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.s_Parity.ForeColor = System.Drawing.SystemColors.WindowText;
			this.s_Parity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even",
            "Mark"});
			this.s_Parity.Location = new System.Drawing.Point(105, 132);
			this.s_Parity.Name = "s_Parity";
			this.s_Parity.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.s_Parity.Size = new System.Drawing.Size(65, 22);
			this.s_Parity.TabIndex = 13;
			this.s_Parity.Text = "None";
			this.ToolTip1.SetToolTip(this.s_Parity, "default \'None\'");
			// 
			// s_stop
			// 
			this.s_stop.BackColor = System.Drawing.SystemColors.Window;
			this.s_stop.Cursor = System.Windows.Forms.Cursors.Default;
			this.s_stop.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.s_stop.ForeColor = System.Drawing.SystemColors.WindowText;
			this.s_stop.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
			this.s_stop.Location = new System.Drawing.Point(105, 159);
			this.s_stop.Name = "s_stop";
			this.s_stop.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.s_stop.Size = new System.Drawing.Size(65, 22);
			this.s_stop.TabIndex = 12;
			this.s_stop.Text = "1";
			this.ToolTip1.SetToolTip(this.s_stop, "default=1");
			// 
			// lblVersion
			// 
			this.lblVersion.AutoSize = true;
			this.lblVersion.BackColor = System.Drawing.Color.White;
			this.lblVersion.Cursor = System.Windows.Forms.Cursors.Default;
			this.lblVersion.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.lblVersion.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblVersion.Location = new System.Drawing.Point(12, 12);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.lblVersion.Size = new System.Drawing.Size(169, 19);
			this.lblVersion.TabIndex = 2;
			this.lblVersion.Text = " pn 95-38961-02 rev.F";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.ToolTip1.SetToolTip(this.lblVersion, "TMC part number");
			// 
			// cmdConnect
			// 
			this.cmdConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.cmdConnect.Cursor = System.Windows.Forms.Cursors.Default;
			this.cmdConnect.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.cmdConnect.ForeColor = System.Drawing.SystemColors.ControlText;
			this.cmdConnect.Image = ((System.Drawing.Image)(resources.GetObject("cmdConnect.Image")));
			this.cmdConnect.Location = new System.Drawing.Point(117, 291);
			this.cmdConnect.Name = "cmdConnect";
			this.cmdConnect.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.cmdConnect.Size = new System.Drawing.Size(310, 73);
			this.cmdConnect.TabIndex = 28;
			this.cmdConnect.Text = "Connect to Controller";
			this.cmdConnect.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.ToolTip1.SetToolTip(this.cmdConnect, "Attempt to connect to controller using Port# and Baud Rate");
			this.cmdConnect.UseVisualStyleBackColor = false;
			this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
			// 
			// _Label1_1
			// 
			this._Label1_1.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_1.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_1.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_1.Location = new System.Drawing.Point(24, 18);
			this._Label1_1.Name = "_Label1_1";
			this._Label1_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_1.Size = new System.Drawing.Size(65, 17);
			this._Label1_1.TabIndex = 17;
			this._Label1_1.Text = "Select Port";
			this._Label1_1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtHost
			// 
			this.txtHost.AcceptsReturn = true;
			this.txtHost.BackColor = System.Drawing.SystemColors.Window;
			this.txtHost.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtHost.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtHost.ForeColor = System.Drawing.SystemColors.WindowText;
			this.txtHost.Location = new System.Drawing.Point(84, 16);
			this.txtHost.MaxLength = 0;
			this.txtHost.Name = "txtHost";
			this.txtHost.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.txtHost.Size = new System.Drawing.Size(93, 20);
			this.txtHost.TabIndex = 24;
			this.txtHost.Text = "169.254.20.20";
			this.txtHost.TextChanged += new System.EventHandler(this.txtHost_TextChanged);
			// 
			// ComboIPlist
			// 
			this.ComboIPlist.BackColor = System.Drawing.SystemColors.Window;
			this.ComboIPlist.Cursor = System.Windows.Forms.Cursors.Default;
			this.ComboIPlist.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboIPlist.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ComboIPlist.ForeColor = System.Drawing.SystemColors.WindowText;
			this.ComboIPlist.Items.AddRange(new object[] {
            "192.168.168.37",
            "192.168.1.37",
            "169.254.20.20",
            "192.168.6.180"});
			this.ComboIPlist.Location = new System.Drawing.Point(84, 15);
			this.ComboIPlist.Name = "ComboIPlist";
			this.ComboIPlist.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.ComboIPlist.Size = new System.Drawing.Size(111, 22);
			this.ComboIPlist.TabIndex = 25;
			this.ComboIPlist.SelectedIndexChanged += new System.EventHandler(this.ComboIPlist_SelectedIndexChanged);
			// 
			// _Label1_6
			// 
			this._Label1_6.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_6.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_6.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_6.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_6.Location = new System.Drawing.Point(33, 135);
			this._Label1_6.Name = "_Label1_6";
			this._Label1_6.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_6.Size = new System.Drawing.Size(65, 17);
			this._Label1_6.TabIndex = 20;
			this._Label1_6.Text = "Parity";
			this._Label1_6.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// txtPort
			// 
			this.txtPort.AcceptsReturn = true;
			this.txtPort.BackColor = System.Drawing.SystemColors.Window;
			this.txtPort.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.txtPort.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtPort.ForeColor = System.Drawing.SystemColors.WindowText;
			this.txtPort.Location = new System.Drawing.Point(84, 42);
			this.txtPort.MaxLength = 0;
			this.txtPort.Name = "txtPort";
			this.txtPort.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.txtPort.Size = new System.Drawing.Size(47, 20);
			this.txtPort.TabIndex = 23;
			this.txtPort.Text = "2020";
			// 
			// _Label1_5
			// 
			this._Label1_5.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_5.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_5.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_5.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_5.Location = new System.Drawing.Point(33, 108);
			this._Label1_5.Name = "_Label1_5";
			this._Label1_5.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_5.Size = new System.Drawing.Size(65, 17);
			this._Label1_5.TabIndex = 19;
			this._Label1_5.Text = "Word Length";
			this._Label1_5.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// _Label1_10
			// 
			this._Label1_10.AutoSize = true;
			this._Label1_10.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_10.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_10.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this._Label1_10.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_10.Location = new System.Drawing.Point(12, 33);
			this._Label1_10.Name = "_Label1_10";
			this._Label1_10.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_10.Size = new System.Drawing.Size(147, 32);
			this._Label1_10.TabIndex = 7;
			this._Label1_10.Text = "\nV1.1.04 @ 04-Jan-2026";
			this._Label1_10.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// _Label1_8
			// 
			this._Label1_8.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_8.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_8.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_8.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_8.Location = new System.Drawing.Point(18, 18);
			this._Label1_8.Name = "_Label1_8";
			this._Label1_8.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_8.Size = new System.Drawing.Size(63, 14);
			this._Label1_8.TabIndex = 8;
			this._Label1_8.Text = "IP Address:";
			// 
			// lblWarning
			// 
			this.lblWarning.BackColor = System.Drawing.SystemColors.Control;
			this.lblWarning.Cursor = System.Windows.Forms.Cursors.Default;
			this.lblWarning.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.lblWarning.ForeColor = System.Drawing.SystemColors.Highlight;
			this.lblWarning.Location = new System.Drawing.Point(21, 366);
			this.lblWarning.Name = "lblWarning";
			this.lblWarning.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.lblWarning.Size = new System.Drawing.Size(407, 16);
			this.lblWarning.TabIndex = 4;
			this.lblWarning.Text = "Choose Communication port and hit \'Connect\' or hit \'DEMO\'";
			// 
			// lblInternalVersion
			// 
			this.lblInternalVersion.BackColor = System.Drawing.SystemColors.Control;
			this.lblInternalVersion.Cursor = System.Windows.Forms.Cursors.Default;
			this.lblInternalVersion.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.lblInternalVersion.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblInternalVersion.Location = new System.Drawing.Point(252, 9);
			this.lblInternalVersion.Name = "lblInternalVersion";
			this.lblInternalVersion.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.lblInternalVersion.Size = new System.Drawing.Size(174, 17);
			this.lblInternalVersion.TabIndex = 1;
			this.lblInternalVersion.Text = "internal version";
			this.lblInternalVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// _Label1_7
			// 
			this._Label1_7.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_7.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_7.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_7.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_7.Location = new System.Drawing.Point(36, 162);
			this._Label1_7.Name = "_Label1_7";
			this._Label1_7.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_7.Size = new System.Drawing.Size(65, 17);
			this._Label1_7.TabIndex = 21;
			this._Label1_7.Text = "Stop Bit(s)";
			this._Label1_7.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// FrameCOM
			// 
			this.FrameCOM.BackColor = System.Drawing.SystemColors.Control;
			this.FrameCOM.Controls.Add(this.s_Baud);
			this.FrameCOM.Controls.Add(this.s_port);
			this.FrameCOM.Controls.Add(this.s_Length);
			this.FrameCOM.Controls.Add(this.s_Parity);
			this.FrameCOM.Controls.Add(this.s_stop);
			this.FrameCOM.Controls.Add(this._Label1_7);
			this.FrameCOM.Controls.Add(this._Label1_6);
			this.FrameCOM.Controls.Add(this._Label1_5);
			this.FrameCOM.Controls.Add(this._Label1_4);
			this.FrameCOM.Controls.Add(this._Label1_1);
			this.FrameCOM.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FrameCOM.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FrameCOM.Location = new System.Drawing.Point(222, 189);
			this.FrameCOM.Name = "FrameCOM";
			this.FrameCOM.Padding = new System.Windows.Forms.Padding(0);
			this.FrameCOM.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FrameCOM.Size = new System.Drawing.Size(205, 94);
			this.FrameCOM.TabIndex = 11;
			this.FrameCOM.TabStop = false;
			this.FrameCOM.Text = "COM port";
			// 
			// _Label1_9
			// 
			this._Label1_9.BackColor = System.Drawing.SystemColors.Control;
			this._Label1_9.Cursor = System.Windows.Forms.Cursors.Default;
			this._Label1_9.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._Label1_9.ForeColor = System.Drawing.SystemColors.ControlText;
			this._Label1_9.Location = new System.Drawing.Point(48, 45);
			this._Label1_9.Name = "_Label1_9";
			this._Label1_9.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._Label1_9.Size = new System.Drawing.Size(29, 17);
			this._Label1_9.TabIndex = 27;
			this._Label1_9.Text = "Port:";
			// 
			// _OptConnection_0
			// 
			this._OptConnection_0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this._OptConnection_0.Checked = true;
			this._OptConnection_0.Cursor = System.Windows.Forms.Cursors.Default;
			this._OptConnection_0.ForeColor = System.Drawing.SystemColors.ControlText;
			this._OptConnection_0.Location = new System.Drawing.Point(225, 165);
			this._OptConnection_0.Name = "_OptConnection_0";
			this._OptConnection_0.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._OptConnection_0.Size = new System.Drawing.Size(100, 22);
			this._OptConnection_0.TabIndex = 10;
			this._OptConnection_0.TabStop = true;
			this._OptConnection_0.Text = "USB->COM";
			this._OptConnection_0.UseVisualStyleBackColor = false;
			this._OptConnection_0.CheckedChanged += new System.EventHandler(this._OptConnection_Click);
			// 
			// lbl_IP_tip
			// 
			this.lbl_IP_tip.BackColor = System.Drawing.SystemColors.Control;
			this.lbl_IP_tip.Cursor = System.Windows.Forms.Cursors.Default;
			this.lbl_IP_tip.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbl_IP_tip.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lbl_IP_tip.Location = new System.Drawing.Point(30, 63);
			this.lbl_IP_tip.Name = "lbl_IP_tip";
			this.lbl_IP_tip.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.lbl_IP_tip.Size = new System.Drawing.Size(142, 28);
			this.lbl_IP_tip.TabIndex = 26;
			this.lbl_IP_tip.Text = "DC-2020 default static IP address";
			this.lbl_IP_tip.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// _OptConnection_1
			// 
			this._OptConnection_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
			this._OptConnection_1.Cursor = System.Windows.Forms.Cursors.Default;
			this._OptConnection_1.ForeColor = System.Drawing.SystemColors.ControlText;
			this._OptConnection_1.Location = new System.Drawing.Point(333, 165);
			this._OptConnection_1.Name = "_OptConnection_1";
			this._OptConnection_1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this._OptConnection_1.Size = new System.Drawing.Size(94, 22);
			this._OptConnection_1.TabIndex = 9;
			this._OptConnection_1.Text = "Telnet";
			this._OptConnection_1.UseVisualStyleBackColor = false;
			this._OptConnection_1.CheckedChanged += new System.EventHandler(this._OptConnection_Click);
			// 
			// FrameTelnet
			// 
			this.FrameTelnet.BackColor = System.Drawing.SystemColors.Control;
			this.FrameTelnet.Controls.Add(this.txtHost);
			this.FrameTelnet.Controls.Add(this.ComboIPlist);
			this.FrameTelnet.Controls.Add(this.txtPort);
			this.FrameTelnet.Controls.Add(this._Label1_8);
			this.FrameTelnet.Controls.Add(this._Label1_9);
			this.FrameTelnet.Controls.Add(this.lbl_IP_tip);
			this.FrameTelnet.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FrameTelnet.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FrameTelnet.Location = new System.Drawing.Point(222, 189);
			this.FrameTelnet.Name = "FrameTelnet";
			this.FrameTelnet.Padding = new System.Windows.Forms.Padding(0);
			this.FrameTelnet.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.FrameTelnet.Size = new System.Drawing.Size(205, 94);
			this.FrameTelnet.TabIndex = 22;
			this.FrameTelnet.TabStop = false;
			this.FrameTelnet.Text = "Telnet";
			this.FrameTelnet.Visible = false;
			// 
			// Frame1
			// 
			this.Frame1.BackColor = System.Drawing.SystemColors.Control;
			this.Frame1.Controls.Add(this.CmdDEMO);
			this.Frame1.Controls.Add(this.cmdConnect);
			this.Frame1.Controls.Add(this._OptConnection_0);
			this.Frame1.Controls.Add(this._OptConnection_1);
			this.Frame1.Controls.Add(this.cmdSaveSetup);
			this.Frame1.Controls.Add(this.cmdLoadPort);
			this.Frame1.Controls.Add(this.Image2);
			this.Frame1.Controls.Add(this._Label1_10);
			this.Frame1.Controls.Add(this.lblVersion);
			this.Frame1.Controls.Add(this.lblWarning);
			this.Frame1.Controls.Add(this._imgLogo_1);
			this.Frame1.Controls.Add(this._imgLogo_0);
			this.Frame1.Controls.Add(this.lblInternalVersion);
			this.Frame1.Controls.Add(this.FrameCOM);
			this.Frame1.Controls.Add(this.FrameTelnet);
			this.Frame1.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Frame1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Frame1.Location = new System.Drawing.Point(12, 12);
			this.Frame1.Name = "Frame1";
			this.Frame1.Padding = new System.Windows.Forms.Padding(0);
			this.Frame1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame1.Size = new System.Drawing.Size(436, 388);
			this.Frame1.TabIndex = 1;
			this.Frame1.TabStop = false;
			// 
			// Image2
			// 
			this.Image2.Cursor = System.Windows.Forms.Cursors.Default;
			this.Image2.Image = ((System.Drawing.Image)(resources.GetObject("Image2.Image")));
			this.Image2.Location = new System.Drawing.Point(252, 27);
			this.Image2.Name = "Image2";
			this.Image2.Size = new System.Drawing.Size(181, 136);
			this.Image2.TabIndex = 32;
			this.Image2.TabStop = false;
			// 
			// _imgLogo_1
			// 
			this._imgLogo_1.Cursor = System.Windows.Forms.Cursors.Default;
			this._imgLogo_1.Image = ((System.Drawing.Image)(resources.GetObject("_imgLogo_1.Image")));
			this._imgLogo_1.Location = new System.Drawing.Point(81, 42);
			this._imgLogo_1.Name = "_imgLogo_1";
			this._imgLogo_1.Size = new System.Drawing.Size(40, 38);
			this._imgLogo_1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this._imgLogo_1.TabIndex = 33;
			this._imgLogo_1.TabStop = false;
			// 
			// _imgLogo_0
			// 
			this._imgLogo_0.Cursor = System.Windows.Forms.Cursors.Default;
			this._imgLogo_0.Image = ((System.Drawing.Image)(resources.GetObject("_imgLogo_0.Image")));
			this._imgLogo_0.Location = new System.Drawing.Point(12, 51);
			this._imgLogo_0.Name = "_imgLogo_0";
			this._imgLogo_0.Size = new System.Drawing.Size(172, 168);
			this._imgLogo_0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this._imgLogo_0.TabIndex = 34;
			this._imgLogo_0.TabStop = false;
			// 
			// frmSplash
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(461, 415);
			this.Controls.Add(this.Frame1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmSplash";
			this.Text = "PortSelector";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSplash_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSplash_FormClosed);
			this.Load += new System.EventHandler(this.frmSplash_Load);
			this.FrameCOM.ResumeLayout(false);
			this.FrameTelnet.ResumeLayout(false);
			this.FrameTelnet.PerformLayout();
			this.Frame1.ResumeLayout(false);
			this.Frame1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Image2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._imgLogo_1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._imgLogo_0)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.Label _Label1_4;
		public System.Windows.Forms.ToolTip ToolTip1;
		public System.Windows.Forms.Button CmdDEMO;
		public System.Windows.Forms.ComboBox s_port;
		public System.Windows.Forms.ComboBox s_Baud;
		public System.Windows.Forms.Button cmdConnect;
		public System.Windows.Forms.ComboBox s_Length;
		public System.Windows.Forms.Button cmdSaveSetup;
		public System.Windows.Forms.Button cmdLoadPort;
		public System.Windows.Forms.ComboBox s_Parity;
		public System.Windows.Forms.ComboBox s_stop;
		public System.Windows.Forms.Label lblVersion;
		public System.Windows.Forms.Label _Label1_1;
		public System.Windows.Forms.TextBox txtHost;
		public System.Windows.Forms.PictureBox Image2;
		public System.Windows.Forms.ComboBox ComboIPlist;
		public System.Windows.Forms.Label _Label1_6;
		public System.Windows.Forms.TextBox txtPort;
		public System.Windows.Forms.Label _Label1_5;
		public System.Windows.Forms.Label _Label1_10;
		public System.Windows.Forms.Label _Label1_8;
		public System.Windows.Forms.Label lblWarning;
		public System.Windows.Forms.PrintDialog dlgCommonDialogPrint;
		public System.Windows.Forms.PictureBox _imgLogo_1;
		public System.Windows.Forms.ColorDialog dlgCommonDialogColor;
		public System.Windows.Forms.FontDialog dlgCommonDialogFont;
		public System.Windows.Forms.PictureBox _imgLogo_0;
		public System.Windows.Forms.SaveFileDialog dlgCommonDialogSave;
		public System.Windows.Forms.Label lblInternalVersion;
		public System.Windows.Forms.Label _Label1_7;
		public System.Windows.Forms.OpenFileDialog dlgCommonDialogOpen;
		public System.Windows.Forms.GroupBox FrameCOM;
		public System.Windows.Forms.Label _Label1_9;
		public System.Windows.Forms.RadioButton _OptConnection_0;
		public System.Windows.Forms.Label lbl_IP_tip;
		public System.Windows.Forms.RadioButton _OptConnection_1;
		public System.Windows.Forms.GroupBox FrameTelnet;
		public System.Windows.Forms.GroupBox Frame1;
	}
}