namespace TMCAnalyzer
{
	partial class TerminalControl
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.loadSetupCntrlOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveSetupCntrlSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToClipboardCntrlCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearScreenCntrlDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToHistoryCntrlAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemComPort = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemConnect = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDisconnect = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemCallMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemQuitMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemHistory = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.textBoxTerminal = new System.Windows.Forms.TextBox();
			this.timerCursorBlink = new System.Windows.Forms.Timer(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.menuStrip1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.textBoxTerminal, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(661, 355);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// menuStrip1
			//
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.menuItemFile,
			this.menuItemEdit,
			this.menuItemComPort,
			this.menuItemConnect,
			this.menuItemDisconnect,
			this.menuItemCallMenu,
			this.menuItemQuitMenu,
			this.menuItemHistory,
			this.menuItemHelp});
			this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(661, 23);
			this.menuStrip1.TabIndex = 10;
			this.menuStrip1.Text = "menuStrip1";
			//
			// menuItemFile
			//
			this.menuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.loadSetupCntrlOToolStripMenuItem,
			this.saveSetupCntrlSToolStripMenuItem,
			this.exitToolStripMenuItem});
			this.menuItemFile.Name = "menuItemFile";
			this.menuItemFile.Size = new System.Drawing.Size(37, 19);
			this.menuItemFile.Text = "File";
			//
			// loadSetupCntrlOToolStripMenuItem
			//
			this.loadSetupCntrlOToolStripMenuItem.Name = "loadSetupCntrlOToolStripMenuItem";
			this.loadSetupCntrlOToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.loadSetupCntrlOToolStripMenuItem.Text = "Load Setup (Cntrl+O)";
			this.loadSetupCntrlOToolStripMenuItem.Click += new System.EventHandler(this.loadSetupCntrlOToolStripMenuItem_Click);
			//
			// saveSetupCntrlSToolStripMenuItem
			//
			this.saveSetupCntrlSToolStripMenuItem.Name = "saveSetupCntrlSToolStripMenuItem";
			this.saveSetupCntrlSToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.saveSetupCntrlSToolStripMenuItem.Text = "Save Setup (Cntrl+S)";
			this.saveSetupCntrlSToolStripMenuItem.Click += new System.EventHandler(this.saveSetupCntrlSToolStripMenuItem_Click);
			//
			// exitToolStripMenuItem
			//
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			//
			// menuItemEdit
			//
			this.menuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.copyToClipboardCntrlCToolStripMenuItem,
			this.clearScreenCntrlDToolStripMenuItem,
			this.copyToHistoryCntrlAToolStripMenuItem});
			this.menuItemEdit.Name = "menuItemEdit";
			this.menuItemEdit.Size = new System.Drawing.Size(39, 19);
			this.menuItemEdit.Text = "Edit";
			//
			// copyToClipboardCntrlCToolStripMenuItem
			//
			this.copyToClipboardCntrlCToolStripMenuItem.Name = "copyToClipboardCntrlCToolStripMenuItem";
			this.copyToClipboardCntrlCToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
			this.copyToClipboardCntrlCToolStripMenuItem.Text = "Copy to Clipboard (Cntrl+C)";
			this.copyToClipboardCntrlCToolStripMenuItem.Click += new System.EventHandler(this.copyToClipboardCntrlCToolStripMenuItem_Click);
			//
			// clearScreenCntrlDToolStripMenuItem
			//
			this.clearScreenCntrlDToolStripMenuItem.Name = "clearScreenCntrlDToolStripMenuItem";
			this.clearScreenCntrlDToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
			this.clearScreenCntrlDToolStripMenuItem.Text = "Clear Screen (Cntrl+D)";
			this.clearScreenCntrlDToolStripMenuItem.Click += new System.EventHandler(this.clearScreenCntrlDToolStripMenuItem_Click);
			//
			// copyToHistoryCntrlAToolStripMenuItem
			//
			this.copyToHistoryCntrlAToolStripMenuItem.Name = "copyToHistoryCntrlAToolStripMenuItem";
			this.copyToHistoryCntrlAToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
			this.copyToHistoryCntrlAToolStripMenuItem.Text = "Copy to History (Cntrl+A)";
			this.copyToHistoryCntrlAToolStripMenuItem.Click += new System.EventHandler(this.copyToHistoryCntrlAToolStripMenuItem_Click);
			//
			// menuItemComPort
			//
			this.menuItemComPort.Name = "menuItemComPort";
			this.menuItemComPort.Size = new System.Drawing.Size(72, 19);
			this.menuItemComPort.Text = "COM Port";
			this.menuItemComPort.Click += new System.EventHandler(this.menuItemComPort_Click);
			//
			// menuItemConnect
			//
			this.menuItemConnect.Name = "menuItemConnect";
			this.menuItemConnect.Size = new System.Drawing.Size(64, 19);
			this.menuItemConnect.Text = "Connect";
			this.menuItemConnect.Click += new System.EventHandler(this.menuItemConnect_Click);
			//
			// menuItemDisconnect
			//
			this.menuItemDisconnect.Name = "menuItemDisconnect";
			this.menuItemDisconnect.Size = new System.Drawing.Size(78, 19);
			this.menuItemDisconnect.Text = "Disconnect";
			this.menuItemDisconnect.Click += new System.EventHandler(this.menuItemDisconnect_Click);
			//
			// menuItemCallMenu
			//
			this.menuItemCallMenu.Name = "menuItemCallMenu";
			this.menuItemCallMenu.Size = new System.Drawing.Size(73, 19);
			this.menuItemCallMenu.Text = "Call Menu";
			this.menuItemCallMenu.Click += new System.EventHandler(this.menuItemCallMenu_Click);
			//
			// menuItemQuitMenu
			//
			this.menuItemQuitMenu.Name = "menuItemQuitMenu";
			this.menuItemQuitMenu.Size = new System.Drawing.Size(73, 19);
			this.menuItemQuitMenu.Text = "QuitMenu";
			this.menuItemQuitMenu.Click += new System.EventHandler(this.menuItemQuitMenu_Click);
			//
			// menuItemHistory
			//
			this.menuItemHistory.Name = "menuItemHistory";
			this.menuItemHistory.Size = new System.Drawing.Size(124, 19);
			this.menuItemHistory.Text = "History Log (Ctrl+L)";
			this.menuItemHistory.Click += new System.EventHandler(this.menuItemHistory_Click);
			//
			// menuItemHelp
			//
			this.menuItemHelp.Name = "menuItemHelp";
			this.menuItemHelp.Size = new System.Drawing.Size(44, 19);
			this.menuItemHelp.Text = "Help";
			this.menuItemHelp.Click += new System.EventHandler(this.menuItemHelp_Click);
			//
			// textBoxTerminal
			//
			this.textBoxTerminal.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.textBoxTerminal.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxTerminal.Location = new System.Drawing.Point(3, 26);
			this.textBoxTerminal.Multiline = true;
			this.textBoxTerminal.Name = "textBoxTerminal";
			this.textBoxTerminal.Size = new System.Drawing.Size(655, 326);
			this.textBoxTerminal.TabIndex = 11;
			this.textBoxTerminal.Click += new System.EventHandler(this.textBoxTerminal_Click);
			this.textBoxTerminal.SizeChanged += new System.EventHandler(this.textBoxTerminal_SizeChanged);
			this.textBoxTerminal.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxTerminal_KeyDown);
			this.textBoxTerminal.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxTerminal_KeyPress);
			this.textBoxTerminal.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxTerminal_Click);
			this.textBoxTerminal.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textBoxTerminal_MouseDown);
			this.textBoxTerminal.MouseLeave += new System.EventHandler(this.textBoxTerminal_Click);
			this.textBoxTerminal.MouseMove += new System.Windows.Forms.MouseEventHandler(this.textBoxTerminal_Click);
			//
			// timerCursorBlink
			//
			this.timerCursorBlink.Interval = 200;
			this.timerCursorBlink.Tick += new System.EventHandler(this.timerCursorBlink_Tick);
			//
			// TerminalControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "TerminalControl";
			this.Size = new System.Drawing.Size(661, 355);
			this.Load += new System.EventHandler(this.TerminalControl_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		internal System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem menuItemFile;
		private System.Windows.Forms.ToolStripMenuItem loadSetupCntrlOToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveSetupCntrlSToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuItemEdit;
		private System.Windows.Forms.ToolStripMenuItem copyToClipboardCntrlCToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearScreenCntrlDToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToHistoryCntrlAToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuItemComPort;
		private System.Windows.Forms.ToolStripMenuItem menuItemConnect;
		private System.Windows.Forms.ToolStripMenuItem menuItemDisconnect;
		private System.Windows.Forms.ToolStripMenuItem menuItemCallMenu;
		private System.Windows.Forms.ToolStripMenuItem menuItemQuitMenu;
		private System.Windows.Forms.ToolStripMenuItem menuItemHelp;
		private System.Windows.Forms.TextBox textBoxTerminal;
		internal System.Windows.Forms.Timer timerCursorBlink;
		private System.Windows.Forms.ToolStripMenuItem menuItemHistory;
	}
}
