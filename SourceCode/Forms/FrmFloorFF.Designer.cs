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
            this.lblAxis2 = new System.Windows.Forms.Label();
            this.lblFFgain0 = new System.Windows.Forms.Label();
            this.numFFgain0 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking0 = new TMCAnalyzer.StateButton();
            this.cwAdaptive0 = new TMCAnalyzer.StateButton();
            this.lblFFgain1 = new System.Windows.Forms.Label();
            this.numFFgain1 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking1 = new TMCAnalyzer.StateButton();
            this.cwAdaptive1 = new TMCAnalyzer.StateButton();
            this.lblFFgain2 = new System.Windows.Forms.Label();
            this.numFFgain2 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking2 = new TMCAnalyzer.StateButton();
            this.cwAdaptive2 = new TMCAnalyzer.StateButton();
            this.lblFFgain3 = new System.Windows.Forms.Label();
            this.numFFgain3 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cwWorking3 = new TMCAnalyzer.StateButton();
            this.cwAdaptive3 = new TMCAnalyzer.StateButton();
            this.cwbutAllFFLoops = new TMCAnalyzer.StateButton();
            this.cwbutFFAdaptive = new TMCAnalyzer.StateButton();
            this.cwbutFFtoMotors = new TMCAnalyzer.StateButton();
            this.FrameVelocityFB = new System.Windows.Forms.GroupBox();
            this.label1_13 = new System.Windows.Forms.Label();
            this.label1_0 = new System.Windows.Forms.Label();
            this.lblAxis1 = new System.Windows.Forms.Label();
            this.cwButAxisEn = new TMCAnalyzer.StateButton();
            this.cwStatus = new TMCAnalyzer.StateButton();
            this.cwX = new TMCAnalyzer.StateButton();
            this.cwY = new TMCAnalyzer.StateButton();
            this.cwtZ = new TMCAnalyzer.StateButton();
            this.cwZ = new TMCAnalyzer.StateButton();
            this.cwtX = new TMCAnalyzer.StateButton();
            this.cwtY = new TMCAnalyzer.StateButton();
            this.numGainX = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGainY = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGaintZ = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGainZ = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGaintX = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.numGaintY = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.cmdPulse = new System.Windows.Forms.Button();
            this.cmbExcitAxis = new System.Windows.Forms.ComboBox();
            this.CmdRestorefromFlash = new System.Windows.Forms.Button();
            this.cmdLoadDefaults = new System.Windows.Forms.Button();
            this.cmdSaveParams = new System.Windows.Forms.Button();
            this.ImageGains = new System.Windows.Forms.PictureBox();
            this.FrameVelocityFB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImageGains)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.cmdRefresh.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmdRefresh.Location = new System.Drawing.Point(12, 12);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(100, 35);
            this.cmdRefresh.TabIndex = 0;
            this.cmdRefresh.Text = "REFRESH";
            this.cmdRefresh.UseVisualStyleBackColor = false;
            // 
            // lblAxis2
            // 
            this.lblAxis2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAxis2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxis2.Location = new System.Drawing.Point(12, 55);
            this.lblAxis2.Name = "lblAxis2";
            this.lblAxis2.Size = new System.Drawing.Size(800, 30);
            this.lblAxis2.TabIndex = 20;
            this.lblAxis2.Text = "Floor Feed Forward";
            this.lblAxis2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFFgain0
            // 
            this.lblFFgain0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain0.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFFgain0.Location = new System.Drawing.Point(50, 95);
            this.lblFFgain0.Name = "lblFFgain0";
            this.lblFFgain0.Size = new System.Drawing.Size(240, 25);
            this.lblFFgain0.TabIndex = 4;
            this.lblFFgain0.Text = "FF to Motors adaptive rate";
            this.lblFFgain0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain0
            // 
            this.numFFgain0.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numFFgain0.Location = new System.Drawing.Point(300, 95);
            this.numFFgain0.Name = "numFFgain0";
            this.numFFgain0.Size = new System.Drawing.Size(120, 25);
            this.numFFgain0.TabIndex = 5;
            // 
            // cwWorking0
            // 
            this.cwWorking0.Location = new System.Drawing.Point(430, 95);
            this.cwWorking0.Name = "cwWorking0";
            this.cwWorking0.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.cwWorking0.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking0.OffPicture")));
            this.cwWorking0.OffText = "No work";
            this.cwWorking0.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.cwWorking0.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking0.OnPicture")));
            this.cwWorking0.OnText = "Working";
            this.cwWorking0.Size = new System.Drawing.Size(80, 25);
            this.cwWorking0.TabIndex = 6;
            this.toolTip1.SetToolTip(this.cwWorking0, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // cwAdaptive0
            // 
            this.cwAdaptive0.Location = new System.Drawing.Point(520, 95);
            this.cwAdaptive0.Name = "cwAdaptive0";
            this.cwAdaptive0.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.cwAdaptive0.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive0.OffPicture")));
            this.cwAdaptive0.OffText = "Not adapt";
            this.cwAdaptive0.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.cwAdaptive0.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive0.OnPicture")));
            this.cwAdaptive0.OnText = "Adaptive";
            this.cwAdaptive0.Size = new System.Drawing.Size(80, 25);
            this.cwAdaptive0.TabIndex = 7;
            this.toolTip1.SetToolTip(this.cwAdaptive0, "Adaptive mode for adaptive rate");
            // 
            // lblFFgain1
            // 
            this.lblFFgain1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFFgain1.Location = new System.Drawing.Point(50, 125);
            this.lblFFgain1.Name = "lblFFgain1";
            this.lblFFgain1.Size = new System.Drawing.Size(240, 25);
            this.lblFFgain1.TabIndex = 8;
            this.lblFFgain1.Text = "X floor FF -> X_motors";
            this.lblFFgain1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain1
            // 
            this.numFFgain1.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numFFgain1.Location = new System.Drawing.Point(300, 125);
            this.numFFgain1.Name = "numFFgain1";
            this.numFFgain1.Size = new System.Drawing.Size(120, 25);
            this.numFFgain1.TabIndex = 9;
            // 
            // cwWorking1
            // 
            this.cwWorking1.Location = new System.Drawing.Point(430, 125);
            this.cwWorking1.Name = "cwWorking1";
            this.cwWorking1.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.cwWorking1.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking1.OffPicture")));
            this.cwWorking1.OffText = "No work";
            this.cwWorking1.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.cwWorking1.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking1.OnPicture")));
            this.cwWorking1.OnText = "Working";
            this.cwWorking1.Size = new System.Drawing.Size(80, 25);
            this.cwWorking1.TabIndex = 10;
            this.toolTip1.SetToolTip(this.cwWorking1, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // cwAdaptive1
            // 
            this.cwAdaptive1.Location = new System.Drawing.Point(520, 125);
            this.cwAdaptive1.Name = "cwAdaptive1";
            this.cwAdaptive1.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.cwAdaptive1.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive1.OffPicture")));
            this.cwAdaptive1.OffText = "Not adapt";
            this.cwAdaptive1.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.cwAdaptive1.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive1.OnPicture")));
            this.cwAdaptive1.OnText = "Adaptive";
            this.cwAdaptive1.Size = new System.Drawing.Size(80, 25);
            this.cwAdaptive1.TabIndex = 11;
            this.toolTip1.SetToolTip(this.cwAdaptive1, "Adaptive mode for X axis");
            // 
            // lblFFgain2
            // 
            this.lblFFgain2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFFgain2.Location = new System.Drawing.Point(50, 155);
            this.lblFFgain2.Name = "lblFFgain2";
            this.lblFFgain2.Size = new System.Drawing.Size(240, 25);
            this.lblFFgain2.TabIndex = 12;
            this.lblFFgain2.Text = "Y floor FF -> Y_motors";
            this.lblFFgain2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain2
            // 
            this.numFFgain2.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numFFgain2.Location = new System.Drawing.Point(300, 155);
            this.numFFgain2.Name = "numFFgain2";
            this.numFFgain2.Size = new System.Drawing.Size(120, 25);
            this.numFFgain2.TabIndex = 13;
            // 
            // cwWorking2
            // 
            this.cwWorking2.Location = new System.Drawing.Point(430, 155);
            this.cwWorking2.Name = "cwWorking2";
            this.cwWorking2.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.cwWorking2.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking2.OffPicture")));
            this.cwWorking2.OffText = "No work";
            this.cwWorking2.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.cwWorking2.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking2.OnPicture")));
            this.cwWorking2.OnText = "Working";
            this.cwWorking2.Size = new System.Drawing.Size(80, 25);
            this.cwWorking2.TabIndex = 14;
            this.toolTip1.SetToolTip(this.cwWorking2, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // cwAdaptive2
            // 
            this.cwAdaptive2.Location = new System.Drawing.Point(520, 155);
            this.cwAdaptive2.Name = "cwAdaptive2";
            this.cwAdaptive2.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.cwAdaptive2.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive2.OffPicture")));
            this.cwAdaptive2.OffText = "Not adapt";
            this.cwAdaptive2.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.cwAdaptive2.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive2.OnPicture")));
            this.cwAdaptive2.OnText = "Adaptive";
            this.cwAdaptive2.Size = new System.Drawing.Size(80, 25);
            this.cwAdaptive2.TabIndex = 15;
            this.toolTip1.SetToolTip(this.cwAdaptive2, "Adaptive mode for Y axis");
            // 
            // lblFFgain3
            // 
            this.lblFFgain3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.lblFFgain3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFFgain3.Location = new System.Drawing.Point(50, 185);
            this.lblFFgain3.Name = "lblFFgain3";
            this.lblFFgain3.Size = new System.Drawing.Size(240, 25);
            this.lblFFgain3.TabIndex = 16;
            this.lblFFgain3.Text = "Z floor FF -> Z_motors";
            this.lblFFgain3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numFFgain3
            // 
            this.numFFgain3.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numFFgain3.Location = new System.Drawing.Point(300, 185);
            this.numFFgain3.Name = "numFFgain3";
            this.numFFgain3.Size = new System.Drawing.Size(120, 25);
            this.numFFgain3.TabIndex = 17;
            // 
            // cwWorking3
            // 
            this.cwWorking3.Location = new System.Drawing.Point(430, 185);
            this.cwWorking3.Name = "cwWorking3";
            this.cwWorking3.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.cwWorking3.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking3.OffPicture")));
            this.cwWorking3.OffText = "No work";
            this.cwWorking3.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.cwWorking3.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwWorking3.OnPicture")));
            this.cwWorking3.OnText = "Working";
            this.cwWorking3.Size = new System.Drawing.Size(80, 25);
            this.cwWorking3.TabIndex = 18;
            this.toolTip1.SetToolTip(this.cwWorking3, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // cwAdaptive3
            // 
            this.cwAdaptive3.Location = new System.Drawing.Point(520, 185);
            this.cwAdaptive3.Name = "cwAdaptive3";
            this.cwAdaptive3.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.cwAdaptive3.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive3.OffPicture")));
            this.cwAdaptive3.OffText = "Not adapt";
            this.cwAdaptive3.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.cwAdaptive3.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwAdaptive3.OnPicture")));
            this.cwAdaptive3.OnText = "Adaptive";
            this.cwAdaptive3.Size = new System.Drawing.Size(80, 25);
            this.cwAdaptive3.TabIndex = 19;
            this.toolTip1.SetToolTip(this.cwAdaptive3, "Adaptive mode for Z axis");
            // 
            // FrameVelocityFB
            // 
            this.FrameVelocityFB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.FrameVelocityFB.Controls.Add(this.label1_13);
            this.FrameVelocityFB.Controls.Add(this.label1_0);
            this.FrameVelocityFB.Controls.Add(this.lblAxis1);
            this.FrameVelocityFB.Controls.Add(this.cwButAxisEn);
            this.FrameVelocityFB.Controls.Add(this.cwStatus);
            this.FrameVelocityFB.Controls.Add(this.cwX);
            this.FrameVelocityFB.Controls.Add(this.cwY);
            this.FrameVelocityFB.Controls.Add(this.cwtZ);
            this.FrameVelocityFB.Controls.Add(this.cwZ);
            this.FrameVelocityFB.Controls.Add(this.cwtX);
            this.FrameVelocityFB.Controls.Add(this.cwtY);
            this.FrameVelocityFB.Controls.Add(this.numGainX);
            this.FrameVelocityFB.Controls.Add(this.numGainY);
            this.FrameVelocityFB.Controls.Add(this.numGaintZ);
            this.FrameVelocityFB.Controls.Add(this.numGainZ);
            this.FrameVelocityFB.Controls.Add(this.numGaintX);
            this.FrameVelocityFB.Controls.Add(this.numGaintY);
            this.FrameVelocityFB.Location = new System.Drawing.Point(12, 220);
            this.FrameVelocityFB.Name = "FrameVelocityFB";
            this.FrameVelocityFB.Size = new System.Drawing.Size(800, 90);
            this.FrameVelocityFB.TabIndex = 21;
            this.FrameVelocityFB.TabStop = false;
            // 
            // label1_13
            // 
            this.label1_13.AutoSize = true;
            this.label1_13.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1_13.Location = new System.Drawing.Point(195, 51);
            this.label1_13.Name = "label1_13";
            this.label1_13.Size = new System.Drawing.Size(39, 18);
            this.label1_13.TabIndex = 37;
            this.label1_13.Text = "Gain";
            // 
            // label1_0
            // 
            this.label1_0.AutoSize = true;
            this.label1_0.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1_0.Location = new System.Drawing.Point(186, 21);
            this.label1_0.Name = "label1_0";
            this.label1_0.Size = new System.Drawing.Size(50, 18);
            this.label1_0.TabIndex = 38;
            this.label1_0.Text = "Status";
            // 
            // lblAxis1
            // 
            this.lblAxis1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblAxis1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAxis1.Location = new System.Drawing.Point(0, 0);
            this.lblAxis1.Name = "lblAxis1";
            this.lblAxis1.Size = new System.Drawing.Size(800, 20);
            this.lblAxis1.TabIndex = 36;
            this.lblAxis1.Text = "Velocity Feed Back";
            this.lblAxis1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblAxis1, "Suppress payload motion by using geophones (velocity sensors) and linear motors");
            // 
            // cwButAxisEn
            // 
            this.cwButAxisEn.Location = new System.Drawing.Point(6, 24);
            this.cwButAxisEn.Name = "cwButAxisEn";
            this.cwButAxisEn.OffColor = System.Drawing.Color.Gray;
            this.cwButAxisEn.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwButAxisEn.OffPicture")));
            this.cwButAxisEn.OffText = "ALL FB AXES OFF";
            this.cwButAxisEn.OnColor = System.Drawing.Color.Blue;
            this.cwButAxisEn.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwButAxisEn.OnPicture")));
            this.cwButAxisEn.OnText = "ALL FB AXES ON";
            this.cwButAxisEn.Size = new System.Drawing.Size(181, 40);
            this.cwButAxisEn.TabIndex = 35;
            this.toolTip1.SetToolTip(this.cwButAxisEn, "Controls All Feed Back loops");
            // 
            // cwStatus
            // 
            this.cwStatus.Location = new System.Drawing.Point(195, 24);
            this.cwStatus.Name = "cwStatus";
            this.cwStatus.OffColor = System.Drawing.Color.Gray;
            this.cwStatus.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwStatus.OffPicture")));
            this.cwStatus.OffText = "OFF";
            this.cwStatus.OnColor = System.Drawing.Color.Green;
            this.cwStatus.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwStatus.OnPicture")));
            this.cwStatus.OnText = "ON";
            this.cwStatus.Size = new System.Drawing.Size(50, 20);
            this.cwStatus.TabIndex = 34;
            this.toolTip1.SetToolTip(this.cwStatus, "Overall FB status");
            // 
            // cwX
            // 
            this.cwX.Location = new System.Drawing.Point(255, 24);
            this.cwX.Name = "cwX";
            this.cwX.OffColor = System.Drawing.Color.Gray;
            this.cwX.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwX.OffPicture")));
            this.cwX.OffText = "OFF";
            this.cwX.OnColor = System.Drawing.Color.Green;
            this.cwX.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwX.OnPicture")));
            this.cwX.OnText = "X";
            this.cwX.Size = new System.Drawing.Size(50, 20);
            this.cwX.TabIndex = 33;
            this.toolTip1.SetToolTip(this.cwX, "X axis FB");
            // 
            // cwY
            // 
            this.cwY.Location = new System.Drawing.Point(315, 24);
            this.cwY.Name = "cwY";
            this.cwY.OffColor = System.Drawing.Color.Gray;
            this.cwY.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwY.OffPicture")));
            this.cwY.OffText = "OFF";
            this.cwY.OnColor = System.Drawing.Color.Green;
            this.cwY.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwY.OnPicture")));
            this.cwY.OnText = "Y";
            this.cwY.Size = new System.Drawing.Size(50, 20);
            this.cwY.TabIndex = 32;
            this.toolTip1.SetToolTip(this.cwY, "Y axis FB");
            // 
            // cwtZ
            // 
            this.cwtZ.Location = new System.Drawing.Point(375, 24);
            this.cwtZ.Name = "cwtZ";
            this.cwtZ.OffColor = System.Drawing.Color.Gray;
            this.cwtZ.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwtZ.OffPicture")));
            this.cwtZ.OffText = "OFF";
            this.cwtZ.OnColor = System.Drawing.Color.Green;
            this.cwtZ.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwtZ.OnPicture")));
            this.cwtZ.OnText = "tZ";
            this.cwtZ.Size = new System.Drawing.Size(50, 20);
            this.cwtZ.TabIndex = 31;
            this.toolTip1.SetToolTip(this.cwtZ, "tZ axis FB");
            // 
            // cwZ
            // 
            this.cwZ.Location = new System.Drawing.Point(435, 24);
            this.cwZ.Name = "cwZ";
            this.cwZ.OffColor = System.Drawing.Color.Gray;
            this.cwZ.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwZ.OffPicture")));
            this.cwZ.OffText = "OFF";
            this.cwZ.OnColor = System.Drawing.Color.Green;
            this.cwZ.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwZ.OnPicture")));
            this.cwZ.OnText = "Z";
            this.cwZ.Size = new System.Drawing.Size(50, 20);
            this.cwZ.TabIndex = 30;
            this.toolTip1.SetToolTip(this.cwZ, "Z axis FB");
            // 
            // cwtX
            // 
            this.cwtX.Location = new System.Drawing.Point(495, 24);
            this.cwtX.Name = "cwtX";
            this.cwtX.OffColor = System.Drawing.Color.Gray;
            this.cwtX.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwtX.OffPicture")));
            this.cwtX.OffText = "OFF";
            this.cwtX.OnColor = System.Drawing.Color.Green;
            this.cwtX.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwtX.OnPicture")));
            this.cwtX.OnText = "tX";
            this.cwtX.Size = new System.Drawing.Size(50, 20);
            this.cwtX.TabIndex = 29;
            this.toolTip1.SetToolTip(this.cwtX, "tX axis FB");
            // 
            // cwtY
            // 
            this.cwtY.Location = new System.Drawing.Point(555, 24);
            this.cwtY.Name = "cwtY";
            this.cwtY.OffColor = System.Drawing.Color.Gray;
            this.cwtY.OffPicture = ((System.Drawing.Image)(resources.GetObject("cwtY.OffPicture")));
            this.cwtY.OffText = "OFF";
            this.cwtY.OnColor = System.Drawing.Color.Green;
            this.cwtY.OnPicture = ((System.Drawing.Image)(resources.GetObject("cwtY.OnPicture")));
            this.cwtY.OnText = "tY";
            this.cwtY.Size = new System.Drawing.Size(50, 20);
            this.cwtY.TabIndex = 28;
            this.toolTip1.SetToolTip(this.cwtY, "tY axis FB");
            // 
            // numGainX
            // 
            this.numGainX.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numGainX.Location = new System.Drawing.Point(255, 51);
            this.numGainX.Name = "numGainX";
            this.numGainX.Size = new System.Drawing.Size(60, 25);
            this.numGainX.TabIndex = 27;
            // 
            // numGainY
            // 
            this.numGainY.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numGainY.Location = new System.Drawing.Point(315, 51);
            this.numGainY.Name = "numGainY";
            this.numGainY.Size = new System.Drawing.Size(60, 25);
            this.numGainY.TabIndex = 26;
            // 
            // numGaintZ
            // 
            this.numGaintZ.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numGaintZ.Location = new System.Drawing.Point(375, 51);
            this.numGaintZ.Name = "numGaintZ";
            this.numGaintZ.Size = new System.Drawing.Size(60, 25);
            this.numGaintZ.TabIndex = 25;
            // 
            // numGainZ
            // 
            this.numGainZ.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numGainZ.Location = new System.Drawing.Point(435, 51);
            this.numGainZ.Name = "numGainZ";
            this.numGainZ.Size = new System.Drawing.Size(60, 25);
            this.numGainZ.TabIndex = 24;
            // 
            // numGaintX
            // 
            this.numGaintX.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numGaintX.Location = new System.Drawing.Point(495, 51);
            this.numGaintX.Name = "numGaintX";
            this.numGaintX.Size = new System.Drawing.Size(60, 25);
            this.numGaintX.TabIndex = 23;
            // 
            // numGaintY
            // 
            this.numGaintY.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.numGaintY.Location = new System.Drawing.Point(555, 51);
            this.numGaintY.Name = "numGaintY";
            this.numGaintY.Size = new System.Drawing.Size(60, 25);
            this.numGaintY.TabIndex = 22;
            // 
            // cmdPulse
            // 
            this.cmdPulse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.cmdPulse.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdPulse.Location = new System.Drawing.Point(12, 320);
            this.cmdPulse.Name = "cmdPulse";
            this.cmdPulse.Size = new System.Drawing.Size(100, 50);
            this.cmdPulse.TabIndex = 30;
            this.cmdPulse.Text = "Apply Pulse";
            this.cmdPulse.UseVisualStyleBackColor = false;
            this.toolTip1.SetToolTip(this.cmdPulse, "Applies pulse to chosen axis (If excitation was ON, it is disabled after pulse)");
            // 
            // cmbExcitAxis
            // 
            this.cmbExcitAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExcitAxis.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbExcitAxis.FormattingEnabled = true;
            this.cmbExcitAxis.Items.AddRange(new object[] {
            "X",
            "Y",
            "Z",
            "tX",
            "tY",
            "tZ"});
            this.cmbExcitAxis.Location = new System.Drawing.Point(130, 335);
            this.cmbExcitAxis.Name = "cmbExcitAxis";
            this.cmbExcitAxis.Size = new System.Drawing.Size(150, 23);
            this.cmbExcitAxis.TabIndex = 31;
            this.toolTip1.SetToolTip(this.cmbExcitAxis, "Choose axis OUTPUT for excitation");
            // 
            // CmdRestorefromFlash
            // 
            this.CmdRestorefromFlash.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.CmdRestorefromFlash.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.CmdRestorefromFlash.Location = new System.Drawing.Point(300, 320);
            this.CmdRestorefromFlash.Name = "CmdRestorefromFlash";
            this.CmdRestorefromFlash.Size = new System.Drawing.Size(140, 50);
            this.CmdRestorefromFlash.TabIndex = 32;
            this.CmdRestorefromFlash.Text = "Restore Params\r\nfrom FLASH";
            this.CmdRestorefromFlash.UseVisualStyleBackColor = false;
            this.toolTip1.SetToolTip(this.CmdRestorefromFlash, "Sends command to controller to restore parameters from FLASH (parameters must be al" +
        "ready saved)");
            // 
            // cmdLoadDefaults
            // 
            this.cmdLoadDefaults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.cmdLoadDefaults.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmdLoadDefaults.Location = new System.Drawing.Point(460, 320);
            this.cmdLoadDefaults.Name = "cmdLoadDefaults";
            this.cmdLoadDefaults.Size = new System.Drawing.Size(140, 50);
            this.cmdLoadDefaults.TabIndex = 33;
            this.cmdLoadDefaults.Text = "Load Default\r\nParameters";
            this.cmdLoadDefaults.UseVisualStyleBackColor = false;
            this.toolTip1.SetToolTip(this.cmdLoadDefaults, "Temporary sets controller parameters to factory default in RAM. Parameters in FLASH " +
        "are not affected until \"save\" command");
            // 
            // cmdSaveParams
            // 
            this.cmdSaveParams.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.cmdSaveParams.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmdSaveParams.Location = new System.Drawing.Point(620, 320);
            this.cmdSaveParams.Name = "cmdSaveParams";
            this.cmdSaveParams.Size = new System.Drawing.Size(140, 50);
            this.cmdSaveParams.TabIndex = 34;
            this.cmdSaveParams.Text = "Save Parameters\r\ninto FLASH";
            this.cmdSaveParams.UseVisualStyleBackColor = false;
            this.toolTip1.SetToolTip(this.cmdSaveParams, "Sends command to controller to save parameters into FLASH");
            // 
            // ImageGains
            // 
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
            this.ClientSize = new System.Drawing.Size(824, 392);
            this.Controls.Add(this.ImageGains);
            this.Controls.Add(this.cmdSaveParams);
            this.Controls.Add(this.cmdLoadDefaults);
            this.Controls.Add(this.CmdRestorefromFlash);
            this.Controls.Add(this.cmbExcitAxis);
            this.Controls.Add(this.cmdPulse);
            this.Controls.Add(this.FrameVelocityFB);
            this.Controls.Add(this.cwbutFFtoMotors);
            this.Controls.Add(this.cwbutFFAdaptive);
            this.Controls.Add(this.cwbutAllFFLoops);
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
            this.Controls.Add(this.lblAxis2);
            this.Controls.Add(this.cmdRefresh);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmFloorFF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.Text = "Feed Forward and Feed Back Gains";
            this.FrameVelocityFB.ResumeLayout(false);
            this.FrameVelocityFB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFFgain3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGainZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numGaintY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImageGains)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button cmdRefresh;
        private System.Windows.Forms.Label lblAxis2;
        private System.Windows.Forms.Label lblFFgain0;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain0;
        private TMCAnalyzer.StateButton cwWorking0;
        private TMCAnalyzer.StateButton cwAdaptive0;
        private System.Windows.Forms.Label lblFFgain1;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain1;
        private TMCAnalyzer.StateButton cwWorking1;
        private TMCAnalyzer.StateButton cwAdaptive1;
        private System.Windows.Forms.Label lblFFgain2;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain2;
        private TMCAnalyzer.StateButton cwWorking2;
        private TMCAnalyzer.StateButton cwAdaptive2;
        private System.Windows.Forms.Label lblFFgain3;
        private NationalInstruments.UI.WindowsForms.NumericEdit numFFgain3;
        private TMCAnalyzer.StateButton cwWorking3;
        private TMCAnalyzer.StateButton cwAdaptive3;
        private TMCAnalyzer.StateButton cwbutAllFFLoops;
        private TMCAnalyzer.StateButton cwbutFFAdaptive;
        private TMCAnalyzer.StateButton cwbutFFtoMotors;
        private System.Windows.Forms.GroupBox FrameVelocityFB;
        private System.Windows.Forms.Label label1_13;
        private System.Windows.Forms.Label label1_0;
        private System.Windows.Forms.Label lblAxis1;
        private TMCAnalyzer.StateButton cwButAxisEn;
        private TMCAnalyzer.StateButton cwStatus;
        private TMCAnalyzer.StateButton cwX;
        private TMCAnalyzer.StateButton cwY;
        private TMCAnalyzer.StateButton cwtZ;
        private TMCAnalyzer.StateButton cwZ;
        private TMCAnalyzer.StateButton cwtX;
        private TMCAnalyzer.StateButton cwtY;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGainX;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGainY;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGaintZ;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGainZ;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGaintX;
        private NationalInstruments.UI.WindowsForms.NumericEdit numGaintY;
        private System.Windows.Forms.Button cmdPulse;
        private System.Windows.Forms.ComboBox cmbExcitAxis;
        private System.Windows.Forms.Button CmdRestorefromFlash;
        private System.Windows.Forms.Button cmdLoadDefaults;
        private System.Windows.Forms.Button cmdSaveParams;
        private System.Windows.Forms.PictureBox ImageGains;
    }
}