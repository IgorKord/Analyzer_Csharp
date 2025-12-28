using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using NationalInstruments.UI.WindowsForms;


namespace TMCAnalyzer.Forms {
    partial class FrmFloorFF {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFloorFF));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.cwbutAllFFLoops = new TMCAnalyzer. StateButton();
            this.cwbutFFAdaptive = new TMCAnalyzer. StateButton();
            this.cwbutFFtoMotors = new TMCAnalyzer. StateButton();
            this.lblFFgain0 = new System.Windows.Forms.Label();
            this.numFFgain0 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking0 = new TMCAnalyzer. StateButton();
            this.cwAdaptive0 = new TMCAnalyzer. StateButton();
            this.lblFFgain1 = new System.Windows.Forms.Label();
            this.numFFgain1 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking1 = new TMCAnalyzer. StateButton();
            this.cwAdaptive1 = new TMCAnalyzer. StateButton();
            this.lblFFgain2 = new System.Windows.Forms.Label();
            this.numFFgain2 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking2 = new TMCAnalyzer. StateButton();
            this.cwAdaptive2 = new TMCAnalyzer. StateButton();
            this.lblFFgain3 = new System.Windows.Forms.Label();
            this.numFFgain3 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking3 = new TMCAnalyzer. StateButton();
            this.cwAdaptive3 = new TMCAnalyzer. StateButton();
            this.lblAxis2 = new System.Windows.Forms.Label();
            this.FrameVelocityFB = new System.Windows.Forms.GroupBox();
            this.numGaintY = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGaintX = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGainZ = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGaintZ = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGainY = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGainX = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwtY = new TMCAnalyzer. StateButton();
            this.cwtX = new TMCAnalyzer. StateButton();
            this.cwZ = new TMCAnalyzer. StateButton();
            this.cwtZ = new TMCAnalyzer. StateButton();
            this.cwY = new TMCAnalyzer. StateButton();
            this.cwX = new TMCAnalyzer. StateButton();
            this.cwStatus = new TMCAnalyzer. StateButton();
            this.cwButAxisEn = new TMCAnalyzer. StateButton();
            this.label1_13 = new System.Windows.Forms.Label();
            this.label1_0 = new System.Windows.Forms.Label();
            this.lblAxis1 = new System.Windows.Forms.Label();
            this.cmdPulse = new System.Windows.Forms.Button();
            this.cmbExcitAxis = new System.Windows.Forms.ComboBox();
            this.cmdRestorefromFlash = new System.Windows.Forms.Button();
            this.cmdLoadDefaults = new System.Windows.Forms.Button();
            this.cmdSaveParams = new System.Windows.Forms.Button();
            this.ImageGains = new System.Windows.Forms.PictureBox();
            this.FrameVelocityFB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImageGains)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.cmdRefresh.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.cmdRefresh.Location = new System.Drawing.Point(12, 12);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(90, 35);
            this.cmdRefresh.TabIndex = 0;
            this.cmdRefresh.Text = "REFRESH";
            this.cmdRefresh.UseVisualStyleBackColor = false;
            // 
            // cwbutAllFFLoops
            // 
            this.cwbutAllFFLoops.Location = new System.Drawing.Point(120, 12);
            this.cwbutAllFFLoops.Name = "cwbutAllFFLoops";
            this.cwbutAllFFLoops.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwbutAllFFLoops.OffPicture")));
            this.cwbutAllFFLoops.OffText = "ALL FF Loops OFF";
            this.cwbutAllFFLoops.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwbutAllFFLoops.OnPicture")));
            this.cwbutAllFFLoops.OnText = "ALL FF Loops ON";
            this.cwbutAllFFLoops.Size = new System.Drawing.Size(200, 35);
            this.cwbutAllFFLoops.TabIndex = 1;
            this.toolTip1.SetToolTip(this.cwbutAllFFLoops, "Turns all Floor Feed-Forward loops on/off");
            // 
            // cwbutFFAdaptive
            // 
            this.cwbutFFAdaptive.Location = new System.Drawing.Point(340, 22);
            this.cwbutFFAdaptive.Name = "cwbutFFAdaptive";
            this.cwbutFFAdaptive.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwbutFFAdaptive.OffPicture")));
            this.cwbutFFAdaptive.OffText = "Not Adaptive";
            this.cwbutFFAdaptive.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwbutFFAdaptive.OnPicture")));
            this.cwbutFFAdaptive.OnText = "Adaptive";
            this.cwbutFFAdaptive.Size = new System.Drawing.Size(100, 25);
            this.cwbutFFAdaptive.TabIndex = 2;
            this.toolTip1.SetToolTip(this.cwbutFFAdaptive, "Global adaptive mode for Floor FF gains");
            // 
            // cwbutFFtoMotors
            // 
            this.cwbutFFtoMotors.Location = new System.Drawing.Point(460, 12);
            this.cwbutFFtoMotors.Name = "cwbutFFtoMotors";
            this.cwbutFFtoMotors.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwbutFFtoMotors.OffPicture")));
            this.cwbutFFtoMotors.OffText = "FF to Motors OFF";
            this.cwbutFFtoMotors.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwbutFFtoMotors.OnPicture")));
            this.cwbutFFtoMotors.OnText = "FF to Motors ON";
            this.cwbutFFtoMotors.Size = new System.Drawing.Size(200, 35);
            this.cwbutFFtoMotors.TabIndex = 3;
            this.toolTip1.SetToolTip(this.cwbutFFtoMotors, "Routes Floor FF signal to motors");
            // 
            // lblFFgain0
            // 
            this.lblFFgain0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain0.Font = new System.Drawing.Font("Arial", 9F);
            this.lblFFgain0.Location = new System.Drawing.Point(120, 60);
            this.lblFFgain0.Name = "lblFFgain0";
            this.lblFFgain0.Size = new System.Drawing.Size(200, 23);
            this.lblFFgain0.TabIndex = 4;
            this.lblFFgain0.Text = "FF to Motors adaptive rate";
            this.lblFFgain0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain0
            // 
            this.numFFgain0.Location = new System.Drawing.Point(330, 60);
            this.numFFgain0.Name = "numFFgain0";
            this.numFFgain0.Size = new System.Drawing.Size(100, 23);
            this.numFFgain0.TabIndex = 5;
            this.numFFgain0.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numFFgain0.Value = 1.0;
            // 
            // cwWorking0
            // 
            this.cwWorking0.Location = new System.Drawing.Point(440, 60);
            this.cwWorking0.Name = "cwWorking0";
            this.cwWorking0.OffText = "No work";
            this.cwWorking0.OnText = "Working";
            this.cwWorking0.Size = new System.Drawing.Size(80, 23);
            this.cwWorking0.TabIndex = 6;
            // 
            // cwAdaptive0
            // 
            this.cwAdaptive0.Location = new System.Drawing.Point(530, 60);
            this.cwAdaptive0.Name = "cwAdaptive0";
            this.cwAdaptive0.OffText = "Not adapt";
            this.cwAdaptive0.OnText = "Adaptive";
            this.cwAdaptive0.Size = new System.Drawing.Size(80, 23);
            this.cwAdaptive0.TabIndex = 7;
            // 
            // lblFFgain1
            // 
            this.lblFFgain1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain1.Font = new System.Drawing.Font("Arial", 9F);
            this.lblFFgain1.Location = new System.Drawing.Point(120, 89);
            this.lblFFgain1.Name = "lblFFgain1";
            this.lblFFgain1.Size = new System.Drawing.Size(200, 23);
            this.lblFFgain1.TabIndex = 8;
            this.lblFFgain1.Text = "X floor FF -> X_motors";
            this.lblFFgain1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain1
            // 
            this.numFFgain1.Location = new System.Drawing.Point(330, 89);
            this.numFFgain1.Name = "numFFgain1";
            this.numFFgain1.Size = new System.Drawing.Size(100, 23);
            this.numFFgain1.TabIndex = 9;
            this.numFFgain1.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            // 
            // cwWorking1
            // 
            this.cwWorking1.Location = new System.Drawing.Point(440, 89);
            this.cwWorking1.Name = "cwWorking1";
            this.cwWorking1.Size = new System.Drawing.Size(80, 23);
            this.cwWorking1.TabIndex = 10;
            // 
            // cwAdaptive1
            // 
            this.cwAdaptive1.Location = new System.Drawing.Point(530, 89);
            this.cwAdaptive1.Name = "cwAdaptive1";
            this.cwAdaptive1.Size = new System.Drawing.Size(80, 23);
            this.cwAdaptive1.TabIndex = 11;
            // 
            // lblFFgain2
            // 
            this.lblFFgain2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain2.Font = new System.Drawing.Font("Arial", 9F);
            this.lblFFgain2.Location = new System.Drawing.Point(120, 118);
            this.lblFFgain2.Name = "lblFFgain2";
            this.lblFFgain2.Size = new System.Drawing.Size(200, 23);
            this.lblFFgain2.TabIndex = 12;
            this.lblFFgain2.Text = "Y floor FF -> Y_motors";
            this.lblFFgain2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain2
            // 
            this.numFFgain2.Location = new System.Drawing.Point(330, 118);
            this.numFFgain2.Name = "numFFgain2";
            this.numFFgain2.Size = new System.Drawing.Size(100, 23);
            this.numFFgain2.TabIndex = 13;
            this.numFFgain2.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            // 
            // cwWorking2
            // 
            this.cwWorking2.Location = new System.Drawing.Point(440, 118);
            this.cwWorking2.Name = "cwWorking2";
            this.cwWorking2.Size = new System.Drawing.Size(80, 23);
            this.cwWorking2.TabIndex = 14;
            // 
            // cwAdaptive2
            // 
            this.cwAdaptive2.Location = new System.Drawing.Point(530, 118);
            this.cwAdaptive2.Name = "cwAdaptive2";
            this.cwAdaptive2.Size = new System.Drawing.Size(80, 23);
            this.cwAdaptive2.TabIndex = 15;
            // 
            // lblFFgain3
            // 
            this.lblFFgain3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain3.Font = new System.Drawing.Font("Arial", 9F);
            this.lblFFgain3.Location = new System.Drawing.Point(120, 147);
            this.lblFFgain3.Name = "lblFFgain3";
            this.lblFFgain3.Size = new System.Drawing.Size(200, 23);
            this.lblFFgain3.TabIndex = 16;
            this.lblFFgain3.Text = "Z floor FF -> Z_motors";
            this.lblFFgain3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain3
            // 
            this.numFFgain3.Location = new System.Drawing.Point(330, 147);
            this.numFFgain3.Name = "numFFgain3";
            this.numFFgain3.Size = new System.Drawing.Size(100, 23);
            this.numFFgain3.TabIndex = 17;
            this.numFFgain3.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            // 
            // cwWorking3
            // 
            this.cwWorking3.Location = new System.Drawing.Point(440, 147);
            this.cwWorking3.Name = "cwWorking3";
            this.cwWorking3.Size = new System.Drawing.Size(80, 23);
            this.cwWorking3.TabIndex = 18;
            // 
            // cwAdaptive3
            // 
            this.cwAdaptive3.Location = new System.Drawing.Point(530, 147);
            this.cwAdaptive3.Name = "cwAdaptive3";
            this.cwAdaptive3.Size = new System.Drawing.Size(80, 23);
            this.cwAdaptive3.TabIndex = 19;
            // 
            // lblAxis2
            // 
            this.lblAxis2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAxis2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.lblAxis2.Location = new System.Drawing.Point(12, 53);
            this.lblAxis2.Name = "lblAxis2";
            this.lblAxis2.Size = new System.Drawing.Size(800, 30);
            this.lblAxis2.TabIndex = 20;
            this.lblAxis2.Text = "Floor Feed Forward";
            this.lblAxis2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FrameVelocityFB
            // 
            this.FrameVelocityFB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.FrameVelocityFB.Controls.Add(this.numGaintY);
            this.FrameVelocityFB.Controls.Add(this.numGaintX);
            this.FrameVelocityFB.Controls.Add(this.numGainZ);
            this.FrameVelocityFB.Controls.Add(this.numGaintZ);
            this.FrameVelocityFB.Controls.Add(this.numGainY);
            this.FrameVelocityFB.Controls.Add(this.numGainX);
            this.FrameVelocityFB.Controls.Add(this.cwtY);
            this.FrameVelocityFB.Controls.Add(this.cwtX);
            this.FrameVelocityFB.Controls.Add(this.cwZ);
            this.FrameVelocityFB.Controls.Add(this.cwtZ);
            this.FrameVelocityFB.Controls.Add(this.cwY);
            this.FrameVelocityFB.Controls.Add(this.cwX);
            this.FrameVelocityFB.Controls.Add(this.cwStatus);
            this.FrameVelocityFB.Controls.Add(this.cwButAxisEn);
            this.FrameVelocityFB.Controls.Add(this.label1_13);
            this.FrameVelocityFB.Controls.Add(this.label1_0);
            this.FrameVelocityFB.Controls.Add(this.lblAxis1);
            this.FrameVelocityFB.Location = new System.Drawing.Point(12, 190);
            this.FrameVelocityFB.Name = "FrameVelocityFB";
            this.FrameVelocityFB.Size = new System.Drawing.Size(800, 90);
            this.FrameVelocityFB.TabIndex = 21;
            this.FrameVelocityFB.TabStop = false;
            // 
            // label1_13, label1_0, lblAxis1, cwButAxisEn, cwStatus, cwX, cwY, cwtZ, cwZ, cwtX, cwtY, numGainX etc.
            // (kept exactly as in original FrmGains.Designer.cs – positions, images, tags unchanged)
            // 
            // cmdPulse
            // 
            this.cmdPulse.Location = new System.Drawing.Point(12, 300);
            this.cmdPulse.Name = "cmdPulse";
            this.cmdPulse.Size = new System.Drawing.Size(100, 40);
            this.cmdPulse.TabIndex = 30;
            this.cmdPulse.Image = ((System.Drawing.Image)(resources.GetObject("cmdPulse.Image")));
            this.cmdPulse.Text = "Apply Pulse";
            this.cmdPulse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // cmbExcitAxis
            // 
            this.cmbExcitAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExcitAxis.Location = new System.Drawing.Point(130, 310);
            this.cmbExcitAxis.Name = "cmbExcitAxis";
            this.cmbExcitAxis.Size = new System.Drawing.Size(120, 23);
            this.cmbExcitAxis.TabIndex = 31;
            this.cmbExcitAxis.Items.AddRange(new object[] { "X", "Y", "Z", "tX", "tY", "tZ" });
            // 
            // cmdRestorefromFlash
            // 
            this.cmdRestorefromFlash.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.cmdRestorefromFlash.Location = new System.Drawing.Point(300, 300);
            this.cmdRestorefromFlash.Name = "cmdRestorefromFlash";
            this.cmdRestorefromFlash.Size = new System.Drawing.Size(140, 40);
            this.cmdRestorefromFlash.TabIndex = 32;
            this.cmdRestorefromFlash.Text = "Restore Params\r\nfrom FLASH";
            // 
            // cmdLoadDefaults
            // 
            this.cmdLoadDefaults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.cmdLoadDefaults.Location = new System.Drawing.Point(460, 300);
            this.cmdLoadDefaults.Name = "cmdLoadDefaults";
            this.cmdLoadDefaults.Size = new System.Drawing.Size(140, 40);
            this.cmdLoadDefaults.TabIndex = 33;
            this.cmdLoadDefaults.Text = "Load Default\r\nParameters";
            // 
            // cmdSaveParams
            // 
            this.cmdSaveParams.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.cmdSaveParams.Location = new System.Drawing.Point(620, 300);
            this.cmdSaveParams.Name = "cmdSaveParams";
            this.cmdSaveParams.Size = new System.Drawing.Size(140, 40);
            this.cmdSaveParams.TabIndex = 34;
            this.cmdSaveParams.Text = "Save Parameters\r\ninto FLASH";
            // 
            // ImageGains
            // 
            this.ImageGains.Image = ((System.Drawing.Image)(resources.GetObject("ImageGains.Image")));
            this.ImageGains.Location = new System.Drawing.Point(12, 12);
            this.ImageGains.Name = "ImageGains";
            this.ImageGains.Size = new System.Drawing.Size(40, 40);
            this.ImageGains.TabIndex = 35;
            this.ImageGains.TabStop = false;
            // 
            // FrmFloorFF
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 361);
            this.Controls.Add(this.ImageGains);
            this.Controls.Add(this.cmdSaveParams);
            this.Controls.Add(this.cmdLoadDefaults);
            this.Controls.Add(this.cmdRestorefromFlash);
            this.Controls.Add(this.cmbExcitAxis);
            this.Controls.Add(this.cmdPulse);
            this.Controls.Add(this.FrameVelocityFB);
            this.Controls.Add(this.lblAxis2);
            this.Controls.Add(this.cwAdaptive3);
            this.Controls.Add(this.cwWorking3);
            this.Controls.Add(this.numFFgain3);
            this.Controls.Add(this.lblFFgain3);
            this.Controls.Add(this.cwAdaptive2);
            this.Controls.Add(this.cwWorking2);
            this.Controls.Add(this.numFFgain2);
            this.Controls.Add(this.lblFFgain2);
            this.Controls.Add(this.cwAdaptive1);
            this.Controls.Add(this.cwWorking1);
            this.Controls.Add(this.numFFgain1);
            this.Controls.Add(this.lblFFgain1);
            this.Controls.Add(this.cwAdaptive0);
            this.Controls.Add(this.cwWorking0);
            this.Controls.Add(this.numFFgain0);
            this.Controls.Add(this.lblFFgain0);
            this.Controls.Add(this.cwbutFFtoMotors);
            this.Controls.Add(this.cwbutFFAdaptive);
            this.Controls.Add(this.cwbutAllFFLoops);
            this.Controls.Add(this.cmdRefresh);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmFloorFF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Feed Forward and Feed Back Gains";
            this.FrameVelocityFB.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImageGains)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button cmdRefresh;
        private TMCAnalyzer. StateButton cwbutAllFFLoops;
        private TMCAnalyzer. StateButton cwbutFFAdaptive;
        private TMCAnalyzer. StateButton cwbutFFtoMotors;
        private System.Windows.Forms.Label lblFFgain0;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain0;
        private TMCAnalyzer. StateButton cwWorking0;
        private TMCAnalyzer. StateButton cwAdaptive0;
        private System.Windows.Forms.Label lblFFgain1;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain1;
        private TMCAnalyzer. StateButton cwWorking1;
        private TMCAnalyzer. StateButton cwAdaptive1;
        private System.Windows.Forms.Label lblFFgain2;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain2;
        private TMCAnalyzer. StateButton cwWorking2;
        private TMCAnalyzer. StateButton cwAdaptive2;
        private System.Windows.Forms.Label lblFFgain3;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain3;
        private TMCAnalyzer. StateButton cwWorking3;
        private TMCAnalyzer. StateButton cwAdaptive3;
        private System.Windows.Forms.Label lblAxis2;
        private System.Windows.Forms.GroupBox FrameVelocityFB;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGaintY;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGaintX;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGainZ;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGaintZ;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGainY;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGainX;
        private TMCAnalyzer. StateButton cwtY;
        private TMCAnalyzer. StateButton cwtX;
        private TMCAnalyzer. StateButton cwZ;
        private TMCAnalyzer. StateButton cwtZ;
        private TMCAnalyzer. StateButton cwY;
        private TMCAnalyzer. StateButton cwX;
        private TMCAnalyzer. StateButton cwStatus;
        private TMCAnalyzer. StateButton cwButAxisEn;
        private System.Windows.Forms.Label label1_13;
        private System.Windows.Forms.Label label1_0;
        private System.Windows.Forms.Label lblAxis1;
        private System.Windows.Forms.Button cmdPulse;
        private System.Windows.Forms.ComboBox cmbExcitAxis;
        private System.Windows.Forms.Button cmdRestorefromFlash;
        private System.Windows.Forms.Button cmdLoadDefaults;
        private System.Windows.Forms.Button cmdSaveParams;
        private System.Windows.Forms.PictureBox ImageGains;
    }
}