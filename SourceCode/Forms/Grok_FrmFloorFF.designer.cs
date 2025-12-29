namespace TMCAnalyzer.Forms
{
    partial class FrmFloorFF
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFloorFF));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.lblAxis2 = new System.Windows.Forms.Label();
            this.lblFFgain0 = new System.Windows.Forms.Label();
            this.FFgain06 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.Working00 = new TMCAnalyzer.StateButton();
            this.Adaptive00 = new TMCAnalyzer.StateButton();
            this.lblFFgain1 = new System.Windows.Forms.Label();
            this.FFgain01 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.Working01 = new TMCAnalyzer.StateButton();
            this.Adaptive01 = new TMCAnalyzer.StateButton();
            this.lblFFgain2 = new System.Windows.Forms.Label();
            this.FFgain02 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.Working02 = new TMCAnalyzer.StateButton();
            this.Adaptive02 = new TMCAnalyzer.StateButton();
            this.lblFFgain3 = new System.Windows.Forms.Label();
            this.FFgain03 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.Working03 = new TMCAnalyzer.StateButton();
            this.Adaptive03 = new TMCAnalyzer.StateButton();
            this.ToggleFFall = new TMCAnalyzer.StateButton();
            this.ToggleFFmotAdaptive = new TMCAnalyzer.StateButton();
            this.ToggleFFmotors = new TMCAnalyzer.StateButton();
            this.FrameVelocityFB = new System.Windows.Forms.GroupBox();
            this.AxisFBgain_5 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.AxisFBgain_4 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.AxisFBgain_3 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.AxisFBgain_2 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.AxisFBgain_1 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.AxisFBgain_0 = new NationalInstruments.UI.WindowsForms.NumericEdit();
            this.AxisEn5 = new TMCAnalyzer.StateButton();
            this.AxisEn4 = new TMCAnalyzer.StateButton();
            this.AxisEn3 = new TMCAnalyzer.StateButton();
            this.AxisEn2 = new TMCAnalyzer.StateButton();
            this.AxisEn1 = new TMCAnalyzer.StateButton();
            this.AxisEn0 = new TMCAnalyzer.StateButton();
            this.ToggleAllFBaxes = new TMCAnalyzer.StateButton();
            this.label1_13 = new System.Windows.Forms.Label();
            this.label1_0 = new System.Windows.Forms.Label();
            this.lblAxis1 = new System.Windows.Forms.Label();
            this.cmdPulse = new System.Windows.Forms.Button();
            this.cmbExcitAxis = new System.Windows.Forms.ComboBox();
            this.CmdRestorefromFlash = new System.Windows.Forms.Button();
            this.cmdLoadDefaults = new System.Windows.Forms.Button();
            this.cmdSaveParams = new System.Windows.Forms.Button();
            this.ImageGains = new System.Windows.Forms.PictureBox();
            this.FrameVelocityFB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain06)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain01)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain02)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain03)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImageGains)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.cmdRefresh.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cmdRefresh.Location = new System.Drawing.Point(12, 12);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(90, 35);
            this.cmdRefresh.TabIndex = 0;
            this.cmdRefresh.Text = "REFRESH";
            this.cmdRefresh.UseVisualStyleBackColor = false;
            this.cmdRefresh.Click += new System.EventHandler(this.cmdRefresh_Click);
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
            // FFgain06
            // 
            this.FFgain06.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.FFgain06.Location = new System.Drawing.Point(300, 95);
            this.FFgain06.Name = "FFgain06";
            this.FFgain06.Size = new System.Drawing.Size(120, 25);
            this.FFgain06.TabIndex = 5;
            this.FFgain06.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.FFgain_ValueChanged);
            // 
            // Working00
            // 
            this.Working00.Location = new System.Drawing.Point(430, 95);
            this.Working00.Name = "Working00";
            this.Working00.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.Working00.OffPicture = ((System.Drawing.Image)(resources.GetObject("Working00.OffPicture")));
            this.Working00.OffText = "No work";
            this.Working00.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.Working00.OnPicture = ((System.Drawing.Image)(resources.GetObject("Working00.OnPicture")));
            this.Working00.OnText = "Working";
            this.Working00.Size = new System.Drawing.Size(80, 25);
            this.Working00.TabIndex = 6;
            this.Working00.CheckedChanged += new System.EventHandler(this.Working_CheckedChanged);
            this.toolTip1.SetToolTip(this.Working00, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // Adaptive00
            // 
            this.Adaptive00.Location = new System.Drawing.Point(520, 95);
            this.Adaptive00.Name = "Adaptive00";
            this.Adaptive00.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.Adaptive00.OffPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive00.OffPicture")));
            this.Adaptive00.OffText = "Not adapt";
            this.Adaptive00.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.Adaptive00.OnPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive00.OnPicture")));
            this.Adaptive00.OnText = "Adaptive";
            this.Adaptive00.Size = new System.Drawing.Size(80, 25);
            this.Adaptive00.TabIndex = 7;
            this.Adaptive00.CheckedChanged += new System.EventHandler(this.Adaptive_CheckedChanged);
            this.toolTip1.SetToolTip(this.Adaptive00, "Adaptive mode for adaptive rate");
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
            // FFgain01
            // 
            this.FFgain01.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.FFgain01.Location = new System.Drawing.Point(300, 125);
            this.FFgain01.Name = "FFgain01";
            this.FFgain01.Size = new System.Drawing.Size(120, 25);
            this.FFgain01.TabIndex = 9;
            this.FFgain01.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.FFgain_ValueChanged);
            // 
            // Working01
            // 
            this.Working01.Location = new System.Drawing.Point(430, 125);
            this.Working01.Name = "Working01";
            this.Working01.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.Working01.OffPicture = ((System.Drawing.Image)(resources.GetObject("Working01.OffPicture")));
            this.Working01.OffText = "No work";
            this.Working01.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.Working01.OnPicture = ((System.Drawing.Image)(resources.GetObject("Working01.OnPicture")));
            this.Working01.OnText = "Working";
            this.Working01.Size = new System.Drawing.Size(80, 25);
            this.Working01.TabIndex = 10;
            this.Working01.CheckedChanged += new System.EventHandler(this.Working_CheckedChanged);
            this.toolTip1.SetToolTip(this.Working01, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // Adaptive01
            // 
            this.Adaptive01.Location = new System.Drawing.Point(520, 125);
            this.Adaptive01.Name = "Adaptive01";
            this.Adaptive01.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.Adaptive01.OffPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive01.OffPicture")));
            this.Adaptive01.OffText = "Not adapt";
            this.Adaptive01.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.Adaptive01.OnPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive01.OnPicture")));
            this.Adaptive01.OnText = "Adaptive";
            this.Adaptive01.Size = new System.Drawing.Size(80, 25);
            this.Adaptive01.TabIndex = 11;
            this.Adaptive01.CheckedChanged += new System.EventHandler(this.Adaptive_CheckedChanged);
            this.toolTip1.SetToolTip(this.Adaptive01, "Adaptive mode for X axis");
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
            // FFgain02
            // 
            this.FFgain02.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.FFgain02.Location = new System.Drawing.Point(300, 155);
            this.FFgain02.Name = "FFgain02";
            this.FFgain02.Size = new System.Drawing.Size(120, 25);
            this.FFgain02.TabIndex = 13;
            this.FFgain02.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.FFgain_ValueChanged);
            // 
            // Working02
            // 
            this.Working02.Location = new System.Drawing.Point(430, 155);
            this.Working02.Name = "Working02";
            this.Working02.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.Working02.OffPicture = ((System.Drawing.Image)(resources.GetObject("Working02.OffPicture")));
            this.Working02.OffText = "No work";
            this.Working02.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.Working02.OnPicture = ((System.Drawing.Image)(resources.GetObject("Working02.OnPicture")));
            this.Working02.OnText = "Working";
            this.Working02.Size = new System.Drawing.Size(80, 25);
            this.Working02.TabIndex = 14;
            this.Working02.CheckedChanged += new System.EventHandler(this.Working_CheckedChanged);
            this.toolTip1.SetToolTip(this.Working02, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // Adaptive02
            // 
            this.Adaptive02.Location = new System.Drawing.Point(520, 155);
            this.Adaptive02.Name = "Adaptive02";
            this.Adaptive02.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.Adaptive02.OffPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive02.OffPicture")));
            this.Adaptive02.OffText = "Not adapt";
            this.Adaptive02.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.Adaptive02.OnPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive02.OnPicture")));
            this.Adaptive02.OnText = "Adaptive";
            this.Adaptive02.Size = new System.Drawing.Size(80, 25);
            this.Adaptive02.TabIndex = 15;
            this.Adaptive02.CheckedChanged += new System.EventHandler(this.Adaptive_CheckedChanged);
            this.toolTip1.SetToolTip(this.Adaptive02, "Adaptive mode for Y axis");
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
            // FFgain03
            // 
            this.FFgain03.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.FFgain03.Location = new System.Drawing.Point(300, 185);
            this.FFgain03.Name = "FFgain03";
            this.FFgain03.Size = new System.Drawing.Size(120, 25);
            this.FFgain03.TabIndex = 17;
            this.FFgain03.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.FFgain_ValueChanged);
            // 
            // Working03
            // 
            this.Working03.Location = new System.Drawing.Point(430, 185);
            this.Working03.Name = "Working03";
            this.Working03.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(175)))), ((int)(((byte)(175)))), ((int)(((byte)(175)))));
            this.Working03.OffPicture = ((System.Drawing.Image)(resources.GetObject("Working03.OffPicture")));
            this.Working03.OffText = "No work";
            this.Working03.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.Working03.OnPicture = ((System.Drawing.Image)(resources.GetObject("Working03.OnPicture")));
            this.Working03.OnText = "Working";
            this.Working03.Size = new System.Drawing.Size(80, 25);
            this.Working03.TabIndex = 18;
            this.Working03.CheckedChanged += new System.EventHandler(this.Working_CheckedChanged);
            this.toolTip1.SetToolTip(this.Working03, "If \'ALL FF Loops ON\' and \'FF to Motors ON\' and \'Working\': FF signal multiplied by FF gain goes to Axis");
            // 
            // Adaptive03
            // 
            this.Adaptive03.Location = new System.Drawing.Point(520, 185);
            this.Adaptive03.Name = "Adaptive03";
            this.Adaptive03.OffColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(224)))), ((int)(((byte)(255)))));
            this.Adaptive03.OffPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive03.OffPicture")));
            this.Adaptive03.OffText = "Not adapt";
            this.Adaptive03.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(235)))), ((int)(((byte)(126)))));
            this.Adaptive03.OnPicture = ((System.Drawing.Image)(resources.GetObject("Adaptive03.OnPicture")));
            this.Adaptive03.OnText = "Adaptive";
            this.Adaptive03.Size = new System.Drawing.Size(80, 25);
            this.Adaptive03.TabIndex = 19;
            this.Adaptive03.CheckedChanged += new System.EventHandler(this.Adaptive_CheckedChanged);
            this.toolTip1.SetToolTip(this.Adaptive03, "Adaptive mode for Z axis");
            // 
            // ToggleFFall
            // 
            this.ToggleFFall.Location = new System.Drawing.Point(120, 12);
            this.ToggleFFall.Name = "ToggleFFall";
            this.ToggleFFall.OffColor = System.Drawing.Color.Gray;
            this.ToggleFFall.OffPicture = ((System.Drawing.Image)(resources.GetObject("ToggleFFall.OffPicture")));
            this.ToggleFFall.OffText = "ALL FF Loops OFF";
            this.ToggleFFall.OnColor = System.Drawing.Color.Blue;
            this.ToggleFFall.OnPicture = ((System.Drawing.Image)(resources.GetObject("ToggleFFall.OnPicture")));
            this.ToggleFFall.OnText = "ALL FF Loops ON";
            this.ToggleFFall.Size = new System.Drawing.Size(200, 35);
            this.ToggleFFall.TabIndex = 1;
            this.ToggleFFall.CheckedChanged += new System.EventHandler(this.ToggleFFall_CheckedChanged);
            this.toolTip1.SetToolTip(this.ToggleFFall, "Turns all Floor Feed-Forward loops on/off");
            // 
            // ToggleFFmotAdaptive
            // 
            this.ToggleFFmotAdaptive.Location = new System.Drawing.Point(340, 22);
            this.ToggleFFmotAdaptive.Name = "ToggleFFmotAdaptive";
            this.ToggleFFmotAdaptive.OffColor = System.Drawing.Color.Gray;
            this.ToggleFFmotAdaptive.OffPicture = ((System.Drawing.Image)(resources.GetObject("ToggleFFmotAdaptive.OffPicture")));
            this.ToggleFFmotAdaptive.OffText = "Not Adaptive";
            this.ToggleFFmotAdaptive.OnColor = System.Drawing.Color.Green;
            this.ToggleFFmotAdaptive.OnPicture = ((System.Drawing.Image)(resources.GetObject("ToggleFFmotAdaptive.OnPicture")));
            this.ToggleFFmotAdaptive.OnText = "Adaptive";
            this.ToggleFFmotAdaptive.Size = new System.Drawing.Size(100, 25);
            this.ToggleFFmotAdaptive.TabIndex = 2;
            this.ToggleFFmotAdaptive.CheckedChanged += new System.EventHandler(this.ToggleFFmotAdaptive_CheckedChanged);
            this.toolTip1.SetToolTip(this.ToggleFFmotAdaptive, "Global adaptive mode for Floor FF gains");
            // 
            // ToggleFFmotors
            // 
            this.ToggleFFmotors.Location = new System.Drawing.Point(460, 12);
            this.ToggleFFmotors.Name = "ToggleFFmotors";
            this.ToggleFFmotors.OffColor = System.Drawing.Color.Gray;
            this.ToggleFFmotors.OffPicture = ((System.Drawing.Image)(resources.GetObject("ToggleFFmotors.OffPicture")));
            this.ToggleFFmotors.OffText = "FF to Motors OFF";
            this.ToggleFFmotors.OnColor = System.Drawing.Color.Blue;
            this.ToggleFFmotors.OnPicture = ((System.Drawing.Image)(resources.GetObject("ToggleFFmotors.OnPicture")));
            this.ToggleFFmotors.OnText = "FF to Motors ON";
            this.ToggleFFmotors.Size = new System.Drawing.Size(200, 35);
            this.ToggleFFmotors.TabIndex = 3;
            this.ToggleFFmotors.CheckedChanged += new System.EventHandler(this.ToggleFFmotors_CheckedChanged);
            this.toolTip1.SetToolTip(this.ToggleFFmotors, "Routes Floor FF signal to motors");
            // 
            // FrameVelocityFB
            // 
            this.FrameVelocityFB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.FrameVelocityFB.Controls.Add(this.AxisFBgain_5);
            this.FrameVelocityFB.Controls.Add(this.AxisFBgain_4);
            this.FrameVelocityFB.Controls.Add(this.AxisFBgain_3);
            this.FrameVelocityFB.Controls.Add(this.AxisFBgain_2);
            this.FrameVelocityFB.Controls.Add(this.AxisFBgain_1);
            this.FrameVelocityFB.Controls.Add(this.AxisFBgain_0);
            this.FrameVelocityFB.Controls.Add(this.AxisEn5);
            this.FrameVelocityFB.Controls.Add(this.AxisEn4);
            this.FrameVelocityFB.Controls.Add(this.AxisEn3);
            this.FrameVelocityFB.Controls.Add(this.AxisEn2);
            this.FrameVelocityFB.Controls.Add(this.AxisEn1);
            this.FrameVelocityFB.Controls.Add(this.AxisEn0);
            this.FrameVelocityFB.Controls.Add(this.ToggleAllFBaxes);
            this.FrameVelocityFB.Controls.Add(this.label1_13);
            this.FrameVelocityFB.Controls.Add(this.label1_0);
            this.FrameVelocityFB.Controls.Add(this.lblAxis1);
            this.FrameVelocityFB.Location = new System.Drawing.Point(12, 220);
            this.FrameVelocityFB.Name = "FrameVelocityFB";
            this.FrameVelocityFB.Size = new System.Drawing.Size(800, 90);
            this.FrameVelocityFB.TabIndex = 21;
            this.FrameVelocityFB.TabStop = false;
            // 
            // AxisFBgain_5
            // 
            this.AxisFBgain_5.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.AxisFBgain_5.Location = new System.Drawing.Point(555, 51);
            this.AxisFBgain_5.Name = "AxisFBgain_5";
            this.AxisFBgain_5.Size = new System.Drawing.Size(60, 25);
            this.AxisFBgain_5.TabIndex = 22;
            this.AxisFBgain_5.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.AxisFBgain_ValueChanged);
            // 
            // AxisFBgain_4
            // 
            this.AxisFBgain_4.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.AxisFBgain_4.Location = new System.Drawing.Point(495, 51);
            this.AxisFBgain_4.Name = "AxisFBgain_4";
            this.AxisFBgain_4.Size = new System.Drawing.Size(60, 25);
            this.AxisFBgain_4.TabIndex = 23;
            this.AxisFBgain_4.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.AxisFBgain_ValueChanged);
            // 
            // AxisFBgain_3
            // 
            this.AxisFBgain_3.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.AxisFBgain_3.Location = new System.Drawing.Point(435, 51);
            this.AxisFBgain_3.Name = "AxisFBgain_3";
            this.AxisFBgain_3.Size = new System.Drawing.Size(60, 25);
            this.AxisFBgain_3.TabIndex = 24;
            this.AxisFBgain_3.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.AxisFBgain_ValueChanged);
            // 
            // AxisFBgain_2
            // 
            this.AxisFBgain_2.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.AxisFBgain_2.Location = new System.Drawing.Point(375, 51);
            this.AxisFBgain_2.Name = "AxisFBgain_2";
            this.AxisFBgain_2.Size = new System.Drawing.Size(60, 25);
            this.AxisFBgain_2.TabIndex = 25;
            this.AxisFBgain_2.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.AxisFBgain_ValueChanged);
            // 
            // AxisFBgain_1
            // 
            this.AxisFBgain_1.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.AxisFBgain_1.Location = new System.Drawing.Point(315, 51);
            this.AxisFBgain_1.Name = "AxisFBgain_1";
            this.AxisFBgain_1.Size = new System.Drawing.Size(60, 25);
            this.AxisFBgain_1.TabIndex = 26;
            this.AxisFBgain_1.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.AxisFBgain_ValueChanged);
            // 
            // AxisFBgain_0
            // 
            this.AxisFBgain_0.FormatMode = NationalInstruments.UI.NumericFormatMode.CreateScientificMode(3, true);
            this.AxisFBgain_0.Location = new System.Drawing.Point(255, 51);
            this.AxisFBgain_0.Name = "AxisFBgain_0";
            this.AxisFBgain_0.Size = new System.Drawing.Size(60, 25);
            this.AxisFBgain_0.TabIndex = 27;
            this.AxisFBgain_0.ValueChanged += new NationalInstruments.UI.ValueChangedEventHandler(this.AxisFBgain_ValueChanged);
            // 
            // AxisEn5
            // 
            this.AxisEn5.Location = new System.Drawing.Point(555, 24);
            this.AxisEn5.Name = "AxisEn5";
            this.AxisEn5.OffColor = System.Drawing.Color.Gray;
            this.AxisEn5.OffPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn5.OffPicture")));
            this.AxisEn5.OffText = "OFF";
            this.AxisEn5.OnColor = System.Drawing.Color.Green;
            this.AxisEn5.OnPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn5.OnPicture")));
            this.AxisEn5.OnText = "tY";
            this.AxisEn5.Size = new System.Drawing.Size(50, 20);
            this.AxisEn5.TabIndex = 28;
            this.AxisEn5.CheckedChanged += new System.EventHandler(this.cwAxisEn_CheckedChanged);
            this.toolTip1.SetToolTip(this.AxisEn5, "tY axis FB");
            // 
            // AxisEn4
            // 
            this.AxisEn4.Location = new System.Drawing.Point(495, 24);
            this.AxisEn4.Name = "AxisEn4";
            this.AxisEn4.OffColor = System.Drawing.Color.Gray;
            this.AxisEn4.OffPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn4.OffPicture")));
            this.AxisEn4.OffText = "OFF";
            this.AxisEn4.OnColor = System.Drawing.Color.Green;
            this.AxisEn4.OnPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn4.OnPicture")));
            this.AxisEn4.OnText = "tX";
            this.AxisEn4.Size = new System.Drawing.Size(50, 20);
            this.AxisEn4.TabIndex = 29;
            this.AxisEn4.CheckedChanged += new System.EventHandler(this.cwAxisEn_CheckedChanged);
            this.toolTip1.SetToolTip(this.AxisEn4, "tX axis FB");
            // 
            // AxisEn3
            // 
            this.AxisEn3.Location = new System.Drawing.Point(435, 24);
            this.AxisEn3.Name = "AxisEn3";
            this.AxisEn3.OffColor = System.Drawing.Color.Gray;
            this.AxisEn3.OffPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn3.OffPicture")));
            this.AxisEn3.OffText = "OFF";
            this.AxisEn3.OnColor = System.Drawing.Color.Green;
            this.AxisEn3.OnPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn3.OnPicture")));
            this.AxisEn3.OnText = "Z";
            this.AxisEn3.Size = new System.Drawing.Size(50, 20);
            this.AxisEn3.TabIndex = 30;
            this.AxisEn3.CheckedChanged += new System.EventHandler(this.cwAxisEn_CheckedChanged);
            this.toolTip1.SetToolTip(this.AxisEn3, "Z axis FB");
            // 
            // AxisEn2
            // 
            this.AxisEn2.Location = new System.Drawing.Point(375, 24);
            this.AxisEn2.Name = "AxisEn2";
            this.AxisEn2.OffColor = System.Drawing.Color.Gray;
            this.AxisEn2.OffPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn2.OffPicture")));
            this.AxisEn2.OffText = "OFF";
            this.AxisEn2.OnColor = System.Drawing.Color.Green;
            this.AxisEn2.OnPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn2.OnPicture")));
            this.AxisEn2.OnText = "tZ";
            this.AxisEn2.Size = new System.Drawing.Size(50, 20);
            this.AxisEn2.TabIndex = 31;
            this.AxisEn2.CheckedChanged += new System.EventHandler(this.cwAxisEn_CheckedChanged);
            this.toolTip1.SetToolTip(this.AxisEn2, "tZ axis FB");
            // 
            // AxisEn1
            // 
            this.AxisEn1.Location = new System.Drawing.Point(315, 24);
            this.AxisEn1.Name = "AxisEn1";
            this.AxisEn1.OffColor = System.Drawing.Color.Gray;
            this.AxisEn1.OffPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn1.OffPicture")));
            this.AxisEn1.OffText = "OFF";
            this.AxisEn1.OnColor = System.Drawing.Color.Green;
            this.AxisEn1.OnPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn1.OnPicture")));
            this.AxisEn1.OnText = "Y";
            this.AxisEn1.Size = new System.Drawing.Size(50, 20);
            this.AxisEn1.TabIndex = 32;
            this.AxisEn1.CheckedChanged += new System.EventHandler(this.cwAxisEn_CheckedChanged);
            this.toolTip1.SetToolTip(this.AxisEn1, "Y axis FB");
            // 
            // AxisEn0
            // 
            this.AxisEn0.Location = new System.Drawing.Point(195, 24);
            this.AxisEn0.Name = "AxisEn0";
            this.AxisEn0.OffColor = System.Drawing.Color.Gray;
            this.AxisEn0.OffPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn0.OffPicture")));
            this.AxisEn0.OffText = "OFF";
            this.AxisEn0.OnColor = System.Drawing.Color.Green;
            this.AxisEn0.OnPicture = ((System.Drawing.Image)(resources.GetObject("AxisEn0.OnPicture")));
            this.AxisEn0.OnText = "ON";
            this.AxisEn0.Size = new System.Drawing.Size(50, 20);
            this.AxisEn0.TabIndex = 34;
            this.AxisEn0.CheckedChanged += new System.EventHandler(this.cwStatus_CheckedChanged);
            this.toolTip1.SetToolTip(this.AxisEn0, "Overall FB status");
            // 
            // ToggleAllFBaxes
            // 
            this.ToggleAllFBaxes.Location = new System.Drawing.Point(6, 24);
            this.ToggleAllFBaxes.Name = "ToggleAllFBaxes";
            this.ToggleAllFBaxes.OffColor = System.Drawing.Color.Gray;
            this.ToggleAllFBaxes.OffPicture = ((System.Drawing.Image)(resources.GetObject("ToggleAllFBaxes.OffPicture")));
            this.ToggleAllFBaxes.OffText = "ALL FB AXES OFF";
            this.ToggleAllFBaxes.OnColor = System.Drawing.Color.Blue;
            this.ToggleAllFBaxes.OnPicture = ((System.Drawing.Image)(resources.GetObject("ToggleAllFBaxes.OnPicture")));
            this.ToggleAllFBaxes.OnText = "ALL FB AXES ON";
            this.ToggleAllFBaxes.Size = new System.Drawing.Size(181, 40);
            this.ToggleAllFBaxes.TabIndex = 35;
            this.ToggleAllFBaxes.CheckedChanged += new System.EventHandler(this.cwButAxisEn_CheckedChanged);
            this.toolTip1.SetToolTip(this.ToggleAllFBaxes, "Controls All Feed Back loops");
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
            this.cmdPulse.Click += new System.EventHandler(this.cmdPulse_Click);
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
            this.cmbExcitAxis.SelectedIndexChanged += new System.EventHandler(this.cmbExcitAxis_SelectedIndexChanged);
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
            this.CmdRestorefromFlash.Click += new System.EventHandler(this.CmdRestorefromFlash_Click);
            this.toolTip1.SetToolTip(this.CmdRestorefromFlash, "Sends command to controller to restore parameters from FLASH (parameters must be already saved)");
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
            this.cmdLoadDefaults.Click += new System.EventHandler(this.cmdLoadDefaults_Click);
            this.toolTip1.SetToolTip(this.cmdLoadDefaults, "Temporary sets controller parameters to factory default in RAM. Parameters in FLASH are not affected until \"save\" command");
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
            this.cmdSaveParams.Click += new System.EventHandler(this.cmdSaveParams_Click);
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
            this.Controls.Add(this.Adaptive03);
            this.Controls.Add(this.Working03);
            this.Controls.Add(this.FFgain03);
            this.Controls.Add(this.lblFFgain3);
            this.Controls.Add(this.Adaptive02);
            this.Controls.Add(this.Working02);
            this.Controls.Add(this.FFgain02);
            this.Controls.Add(this.lblFFgain2);
            this.Controls.Add(this.Adaptive01);
            this.Controls.Add(this.Working01);
            this.Controls.Add(this.FFgain01);
            this.lblFFgain1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Controls.Add(this.lblFFgain1);
            this.Controls.Add(this.Adaptive00);
            this.Controls.Add(this.Working00);
            this.Controls.Add(this.FFgain06);
            this.Controls.Add(this.lblFFgain0);
            this.Controls.Add(this.lblAxis2);
            this.Controls.Add(this.ToggleFFmotors);
            this.Controls.Add(this.ToggleFFmotAdaptive);
            this.Controls.Add(this.ToggleFFall);
            this.Controls.Add(this.cmdRefresh);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmFloorFF";
            this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
            this.Text = "Feed Forward and Feed Back Gains";
            this.FrameVelocityFB.ResumeLayout(false);
            this.FrameVelocityFB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain03)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain02)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain01)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FFgain06)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AxisFBgain_0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImageGains)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button cmdRefresh;
        private TMCAnalyzer.StateButton ToggleFFall;
        private TMCAnalyzer.StateButton ToggleFFmotAdaptive;
        private TMCAnalyzer.StateButton ToggleFFmotors;
        private System.Windows.Forms.Label lblFFgain0;
        private NationalInstruments.UI.WindowsForms.NumericEdit FFgain06;
        private TMCAnalyzer.StateButton Working00;
        private TMCAnalyzer.StateButton Adaptive00;
        private System.Windows.Forms.Label lblFFgain1;
        private NationalInstruments.UI.WindowsForms.NumericEdit FFgain01;
        private TMCAnalyzer.StateButton Working01;
        private TMCAnalyzer.StateButton Adaptive01;
        private System.Windows.Forms.Label lblFFgain2;
        private NationalInstruments.UI.WindowsForms.NumericEdit FFgain02;
        private TMCAnalyzer.StateButton Working02;
        private TMCAnalyzer.StateButton Adaptive02;
        private System.Windows.Forms.Label lblFFgain3;
        private NationalInstruments.UI.WindowsForms.NumericEdit FFgain03;
        private TMCAnalyzer.StateButton Working03;
        private TMCAnalyzer.StateButton Adaptive03;
        private System.Windows.Forms.Label lblAxis2;
        private System.Windows.Forms.GroupBox FrameVelocityFB;
        private NationalInstruments.UI.WindowsForms.NumericEdit AxisFBgain_5;
        private NationalInstruments.UI.WindowsForms.NumericEdit AxisFBgain_4;
        private NationalInstruments.UI.WindowsForms.NumericEdit AxisFBgain_3;
        private NationalInstruments.UI.WindowsForms.NumericEdit AxisFBgain_2;
        private NationalInstruments.UI.WindowsForms.NumericEdit AxisFBgain_1;
        private NationalInstruments.UI.WindowsForms.NumericEdit AxisFBgain_0;
        private TMCAnalyzer.StateButton AxisEn5;
        private TMCAnalyzer.StateButton AxisEn4;
        private TMCAnalyzer.StateButton AxisEn3;
        private TMCAnalyzer.StateButton AxisEn2;
        private TMCAnalyzer.StateButton AxisEn1;
        private TMCAnalyzer.StateButton AxisEn0;
        private TMCAnalyzer.StateButton ToggleAllFBaxes;
        private System.Windows.Forms.Label label1_13;
        private System.Windows.Forms.Label label1_0;
        private System.Windows.Forms.Label lblAxis1;
        private System.Windows.Forms.Button cmdPulse;
        private System.Windows.Forms.ComboBox cmbExcitAxis;
        private System.Windows.Forms.Button CmdRestorefromFlash;
        private System.Windows.Forms.Button cmdLoadDefaults;
        private System.Windows.Forms.Button cmdSaveParams;
        private System.Windows.Forms.PictureBox ImageGains;
    }
}