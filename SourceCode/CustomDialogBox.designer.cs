namespace TMCAnalyzer {  //namespace UserControNameSpace { 
    partial class CustomDialogBox {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomDialogBox));
            this.txtMsgTest = new System.Windows.Forms.TextBox();
            this.pictMsgIcon = new System.Windows.Forms.PictureBox();
            this.LblMsgSrc = new System.Windows.Forms.Label();
            this.lblMsgCaption = new System.Windows.Forms.Label();
            this.btnMailTo = new System.Windows.Forms.Button();
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnQuitApp = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictMsgIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // txtMsgTest
            // 
            this.txtMsgTest.Location = new System.Drawing.Point(11, 67);
            this.txtMsgTest.Multiline = true;
            this.txtMsgTest.Name = "txtMsgTest";
            this.txtMsgTest.ReadOnly = true;
            this.txtMsgTest.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMsgTest.Size = new System.Drawing.Size(271, 82);
            this.txtMsgTest.TabIndex = 0;
            this.txtMsgTest.Text = "Error Message here";
            // 
            // pictMsgIcon
            // 
            this.pictMsgIcon.Image = ((System.Drawing.Image)(resources.GetObject("pictMsgIcon.Image")));
            this.pictMsgIcon.Location = new System.Drawing.Point(3, 3);
            this.pictMsgIcon.Name = "pictMsgIcon";
            this.pictMsgIcon.Size = new System.Drawing.Size(34, 24);
            this.pictMsgIcon.TabIndex = 1;
            this.pictMsgIcon.TabStop = false;
            // 
            // LblMsgSrc
            // 
            this.LblMsgSrc.AutoSize = true;
            this.LblMsgSrc.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.LblMsgSrc.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblMsgSrc.Location = new System.Drawing.Point(19, 51);
            this.LblMsgSrc.Name = "LblMsgSrc";
            this.LblMsgSrc.Size = new System.Drawing.Size(67, 13);
            this.LblMsgSrc.TabIndex = 2;
            this.LblMsgSrc.Text = "Err Source";
            // 
            // lblMsgCaption
            // 
            this.lblMsgCaption.AutoSize = true;
            this.lblMsgCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMsgCaption.Location = new System.Drawing.Point(57, 7);
            this.lblMsgCaption.Name = "lblMsgCaption";
            this.lblMsgCaption.Size = new System.Drawing.Size(100, 20);
            this.lblMsgCaption.TabIndex = 3;
            this.lblMsgCaption.Text = "Err Caption";
            // 
            // btnMailTo
            // 
            this.btnMailTo.Location = new System.Drawing.Point(11, 221);
            this.btnMailTo.Name = "btnMailTo";
            this.btnMailTo.Size = new System.Drawing.Size(75, 23);
            this.btnMailTo.TabIndex = 4;
            this.btnMailTo.Text = "E-mail";
            this.btnMailTo.UseVisualStyleBackColor = true;
            this.btnMailTo.Click += new System.EventHandler(this.btnMailTo_Click);
            // 
            // btnContinue
            // 
            this.btnContinue.Location = new System.Drawing.Point(113, 223);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(75, 23);
            this.btnContinue.TabIndex = 5;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // btnQuitApp
            // 
            this.btnQuitApp.Location = new System.Drawing.Point(207, 223);
            this.btnQuitApp.Name = "btnQuitApp";
            this.btnQuitApp.Size = new System.Drawing.Size(75, 23);
            this.btnQuitApp.TabIndex = 6;
            this.btnQuitApp.Text = "Quit App";
            this.btnQuitApp.UseVisualStyleBackColor = true;
            this.btnQuitApp.Click += new System.EventHandler(this.btnQuitApp_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(11, 157);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(256, 49);
            this.textBox1.TabIndex = 7;
            this.textBox1.Text = "\'Quit\' button - Terminate application\r\n\'Email\' button - Send e-mail then quit the" +
    "  application\r\n\'Continue \' button  -  Continue run application";
            // 
            // CustomDialogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnQuitApp);
            this.Controls.Add(this.btnContinue);
            this.Controls.Add(this.btnMailTo);
            this.Controls.Add(this.lblMsgCaption);
            this.Controls.Add(this.LblMsgSrc);
            this.Controls.Add(this.pictMsgIcon);
            this.Controls.Add(this.txtMsgTest);
            this.Name = "CustomDialogBox";
            this.Size = new System.Drawing.Size(287, 268);
            ((System.ComponentModel.ISupportInitialize)(this.pictMsgIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictMsgIcon;
        private System.Windows.Forms.Button btnMailTo;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnQuitApp;
        private System.Windows.Forms.TextBox textBox1;
        public System.Windows.Forms.TextBox txtMsgTest;
        public System.Windows.Forms.Label LblMsgSrc;
        public System.Windows.Forms.Label lblMsgCaption;
    }
}
