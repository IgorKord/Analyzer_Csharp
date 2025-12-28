namespace TMCAnalyzer
{
	partial class HistoryWindow
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelLinesCount = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonClearHistory = new System.Windows.Forms.Button();
            this.textBoxHistory = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxAutoClear = new System.Windows.Forms.CheckBox();
            this.checkBoxSentCmd = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelCharsCount = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.numAutoAdd = new System.Windows.Forms.NumericUpDown();
            this.checkBoxAutoAdd = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAutoAdd)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BackColor = System.Drawing.Color.LightCyan;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.MinimumSize = new System.Drawing.Size(40, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Lines";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelLinesCount
            // 
            this.labelLinesCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLinesCount.BackColor = System.Drawing.Color.LightCyan;
            this.labelLinesCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLinesCount.Location = new System.Drawing.Point(3, 19);
            this.labelLinesCount.MinimumSize = new System.Drawing.Size(40, 0);
            this.labelLinesCount.Name = "labelLinesCount";
            this.labelLinesCount.Size = new System.Drawing.Size(54, 20);
            this.labelLinesCount.TabIndex = 3;
            this.labelLinesCount.Text = "0000";
            this.labelLinesCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.BackColor = System.Drawing.SystemColors.Info;
            this.label3.Location = new System.Drawing.Point(3, 3);
            this.label3.MinimumSize = new System.Drawing.Size(40, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 18);
            this.label3.TabIndex = 4;
            this.label3.Text = "Chars";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonClearHistory
            // 
            this.buttonClearHistory.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonClearHistory.BackColor = System.Drawing.Color.PeachPuff;
            this.buttonClearHistory.Location = new System.Drawing.Point(385, 8);
            this.buttonClearHistory.MinimumSize = new System.Drawing.Size(75, 0);
            this.buttonClearHistory.Name = "buttonClearHistory";
            this.buttonClearHistory.Size = new System.Drawing.Size(80, 30);
            this.buttonClearHistory.TabIndex = 6;
            this.buttonClearHistory.Text = "Clear History";
            this.buttonClearHistory.UseVisualStyleBackColor = false;
            this.buttonClearHistory.Click += new System.EventHandler(this.buttonClearHistory_Click);
            // 
            // textBoxHistory
            // 
            this.textBoxHistory.AcceptsReturn = true;
            this.textBoxHistory.BackColor = System.Drawing.Color.White;
            this.textBoxHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxHistory.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxHistory.Location = new System.Drawing.Point(3, 49);
            this.textBoxHistory.Multiline = true;
            this.textBoxHistory.Name = "textBoxHistory";
            this.textBoxHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxHistory.Size = new System.Drawing.Size(578, 310);
            this.textBoxHistory.TabIndex = 7;
            this.textBoxHistory.WordWrap = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxHistory, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(584, 362);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.36175F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17.36175F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.97085F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.30565F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 114F));
            this.tableLayoutPanel2.Controls.Add(this.checkBoxAutoClear, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxSentCmd, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonClearHistory, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel3, 3, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(584, 46);
            this.tableLayoutPanel2.TabIndex = 10;
            // 
            // checkBoxAutoClear
            // 
            this.checkBoxAutoClear.AutoSize = true;
            this.checkBoxAutoClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.checkBoxAutoClear.Dock = System.Windows.Forms.DockStyle.Right;
            this.checkBoxAutoClear.Location = new System.Drawing.Point(471, 3);
            this.checkBoxAutoClear.MinimumSize = new System.Drawing.Size(60, 0);
            this.checkBoxAutoClear.Name = "checkBoxAutoClear";
            this.checkBoxAutoClear.Padding = new System.Windows.Forms.Padding(3);
            this.checkBoxAutoClear.Size = new System.Drawing.Size(110, 40);
            this.checkBoxAutoClear.TabIndex = 10;
            this.checkBoxAutoClear.Text = "Auto clear each 2000 lines";
            this.checkBoxAutoClear.UseVisualStyleBackColor = false;
            // 
            // checkBoxSentCmd
            // 
            this.checkBoxSentCmd.AutoSize = true;
            this.checkBoxSentCmd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.checkBoxSentCmd.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkBoxSentCmd.Location = new System.Drawing.Point(3, 3);
            this.checkBoxSentCmd.MinimumSize = new System.Drawing.Size(60, 0);
            this.checkBoxSentCmd.Name = "checkBoxSentCmd";
            this.checkBoxSentCmd.Padding = new System.Windows.Forms.Padding(3);
            this.checkBoxSentCmd.Size = new System.Drawing.Size(94, 40);
            this.checkBoxSentCmd.TabIndex = 9;
            this.checkBoxSentCmd.Text = "Include Sent Command";
            this.checkBoxSentCmd.UseVisualStyleBackColor = false;
            this.checkBoxSentCmd.CheckedChanged += new System.EventHandler(this.checkBoxSentCmd_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.labelLinesCount);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(103, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(58, 40);
            this.panel1.TabIndex = 11;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.labelCharsCount);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(167, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(58, 40);
            this.panel2.TabIndex = 12;
            // 
            // labelCharsCount
            // 
            this.labelCharsCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCharsCount.BackColor = System.Drawing.SystemColors.Info;
            this.labelCharsCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCharsCount.Location = new System.Drawing.Point(3, 19);
            this.labelCharsCount.MinimumSize = new System.Drawing.Size(40, 0);
            this.labelCharsCount.Name = "labelCharsCount";
            this.labelCharsCount.Size = new System.Drawing.Size(54, 20);
            this.labelCharsCount.TabIndex = 5;
            this.labelCharsCount.Text = "0000";
            this.labelCharsCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.numAutoAdd);
            this.panel3.Controls.Add(this.checkBoxAutoAdd);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(231, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(130, 40);
            this.panel3.TabIndex = 13;
            // 
            // numAutoAdd
            // 
            this.numAutoAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numAutoAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.numAutoAdd.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAutoAdd.Location = new System.Drawing.Point(29, 18);
            this.numAutoAdd.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numAutoAdd.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAutoAdd.Name = "numAutoAdd";
            this.numAutoAdd.Size = new System.Drawing.Size(59, 21);
            this.numAutoAdd.TabIndex = 13;
            this.numAutoAdd.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numAutoAdd.ValueChanged += new System.EventHandler(this.numAutoAdd_ValueChanged);
            // 
            // checkBoxAutoAdd
            // 
            this.checkBoxAutoAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAutoAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.checkBoxAutoAdd.Location = new System.Drawing.Point(26, 1);
            this.checkBoxAutoAdd.MinimumSize = new System.Drawing.Size(60, 0);
            this.checkBoxAutoAdd.Name = "checkBoxAutoAdd";
            this.checkBoxAutoAdd.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.checkBoxAutoAdd.Size = new System.Drawing.Size(100, 20);
            this.checkBoxAutoAdd.TabIndex = 12;
            this.checkBoxAutoAdd.Text = "Auto add each";
            this.checkBoxAutoAdd.UseVisualStyleBackColor = false;
            this.checkBoxAutoAdd.CheckedChanged += new System.EventHandler(this.checkBoxAutoAdd_CheckedChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label2.Location = new System.Drawing.Point(26, 19);
            this.label2.MinimumSize = new System.Drawing.Size(40, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 18);
            this.label2.TabIndex = 14;
            this.label2.Text = "Chars";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HistoryWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 362);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(550, 100);
            this.Name = "HistoryWindow";
            this.Text = "History";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HistoryWindow_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numAutoAdd)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelLinesCount;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button buttonClearHistory;
		public System.Windows.Forms.TextBox textBoxHistory;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.CheckBox checkBoxSentCmd;
		private System.Windows.Forms.Label labelCharsCount;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.CheckBox checkBoxAutoClear;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.NumericUpDown numAutoAdd;
		private System.Windows.Forms.CheckBox checkBoxAutoAdd;
		private System.Windows.Forms.Label label2;
	}
}
