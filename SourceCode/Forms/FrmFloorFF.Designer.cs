using System.Windows.Forms;
using NationalInstruments.UI.WindowsForms;
using System.Drawing;
using System.ComponentModel;
namespace TMCAnalyzer.Forms
{
    partial class FrmFloorFF
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new ToolTip(this.components);
            this.cmbExcitAxis = new ComboBox();
            this.cmdPulse = new Button();
            this.FrameVelocityFB = new GroupBox();
            this.label1_0 = new Label();
            this.label1_13 = new Label();
            this.lblAxis1 = new Label();
            this.cwButAxisEn = new StateButton();
            this.cwStatus = new StateButton();
            this.cwX = new StateButton();
            this.cwY = new StateButton();
            this.cwtZ = new StateButton();
            this.cwZ = new StateButton();
            this.cwtX = new StateButton();
            this.cwtY = new StateButton();
            this.numGainX = new NumericEdit();
            this.numGainY = new NumericEdit();
            this.numGaintZ = new NumericEdit();
            this.numGainZ = new NumericEdit();
            this.numGaintX = new NumericEdit();
            this.numGaintY = new NumericEdit();
            this.lblAxis2 = new Label();
            this.lblFFgain0 = new Label();
            this.lblFFgain1 = new Label();
            this.lblFFgain2 = new Label();
            this.lblFFgain3 = new Label();
            this.numFFgain0 = new NumericEdit();
            this.numFFgain1 = new NumericEdit();
            this.numFFgain2 = new NumericEdit();
            this.numFFgain3 = new NumericEdit();
            this.cwbutAllFFLoops = new StateButton();
            this.cwbutFFAdaptive = new StateButton();
            this.cwbutFFtoMotors = new StateButton();
            this.cwWorking0 = new StateButton();
            this.cwAdaptive0 = new StateButton();
            this.cwWorking1 = new StateButton();
            this.cwAdaptive1 = new StateButton();
            this.cwWorking2 = new StateButton();
            this.cwAdaptive2 = new StateButton();
            this.cwWorking3 = new StateButton();
            this.cwAdaptive3 = new StateButton();
            this.cmdRefresh = new Button();
            this.cmdRestorefromFlash = new Button();
            this.cmdLoadDefaults = new Button();
            this.cmdSaveParams = new Button();
            this.ImageGains = new PictureBox();
            this.FrameVelocityFB.SuspendLayout();
            ((ISupportInitialize)(this.ImageGains)).BeginInit();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 10000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // cmbExcitAxis
            // 
            this.cmbExcitAxis.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbExcitAxis.Font = new Font("Arial", 9F, FontStyle.Bold);
            this.cmbExcitAxis.Items.AddRange(new object[] { "X", "Y", "Z", "tX", "tY", "tZ" });
            this.cmbExcitAxis.Location = new Point(102, 264);
            this.cmbExcitAxis.Name = "cmbExcitAxis";
            this.cmbExcitAxis.Size = new Size(137, 23);
            this.cmbExcitAxis.TabIndex = 41;
            this.toolTip1.SetToolTip(this.cmbExcitAxis, "Choose axis OUTPUT for excitation");
            // 
            // cmdPulse
            // 
            this.cmdPulse.BackColor = Color.FromArgb(128, 255, 255);
            this.cmdPulse.Font = new Font("Arial", 9F);
            this.cmdPulse.Location = new Point(6, 240);
            this.cmdPulse.Name = "cmdPulse";
            this.cmdPulse.Size = new Size(73, 49);
            this.cmdPulse.TabIndex = 40;
            this.cmdPulse.Text = "Apply Pulse";
            this.toolTip1.SetToolTip(this.cmdPulse, "Applies pulse to chosen axis (If excitation was ON, it is disabled after pulse)");
            this.cmdPulse.UseVisualStyleBackColor = false;
            this.cmdPulse.Click += cmdPulse_Click;
            // 
            // FrameVelocityFB
            // 
            this.FrameVelocityFB.BackColor = Color.FromArgb(224, 224, 224);
            this.FrameVelocityFB.Controls.Add(this.label1_0);
            this.FrameVelocityFB.Controls.Add(this.label1_13);
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
            this.FrameVelocityFB.Location = new Point(6, 153);
            this.FrameVelocityFB.Name = "FrameVelocityFB";
            this.FrameVelocityFB.Size = new Size(805, 81);
            this.FrameVelocityFB.TabIndex = 22;
            this.FrameVelocityFB.TabStop = false;
            // 
            // label1_0
            // 
            this.label1_0.AutoSize = true;
            this.label1_0.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            this.label1_0.Location = new Point(186, 21);
            this.label1_0.Name = "label1_0";
            this.label1_0.Size = new Size(50, 18);
            this.label1_0.TabIndex = 38;
            this.label1_0.Text = "Status";
            // 
            // label1_13
            // 
            this.label1_13.AutoSize = true;
            this.label1_13.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            this.label1_13.Location = new Point(195, 51);
            this.label1_13.Name = "label1_13";
            this.label1_13.Size = new Size(39, 18);
            this.label1_13.TabIndex = 37;
            this.label1_13.Text = "Gain";
            // 
            // lblAxis1
            // 
            this.lblAxis1.BorderStyle = BorderStyle.FixedSingle;
            this.lblAxis1.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            this.lblAxis1.Location = new Point(0, 0);
            this.lblAxis1.Name = "lblAxis1";
            this.lblAxis1.Size = new Size(804, 80);
            this.lblAxis1.TabIndex = 36;
            this.lblAxis1.Text = "Velocity Feed Back";
            this.lblAxis1.TextAlign = ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblAxis1, "Suppress payload motion by using geophones (velocity sensors) and linear motors");
            // 
            // cwButAxisEn
            // 
            this.cwButAxisEn.Appearance = Appearance.Button;
            this.cwButAxisEn.Location = new Point(6, 24);
            this.cwButAxisEn.Name = "cwButAxisEn";
            this.cwButAxisEn.Size = new Size(181, 40);
            this.cwButAxisEn.TabIndex = 35;
            this.cwButAxisEn.Tag = "loop_fba";
            this.cwButAxisEn.OnText = "ALL FB AXES ON";
            this.cwButAxisEn.OffText = "ALL FB AXES OFF";
            this.cwButAxisEn.OnColor = Color.Blue;
            this.cwButAxisEn.OffColor = Color.Gray;
            this.cwButAxisEn.OnImageLayout = ImageLayout.Stretch;
            this.cwButAxisEn.OffImageLayout = ImageLayout.Stretch;
            this.toolTip1.SetToolTip(this.cwButAxisEn, "Controls All Feed Back loops");
            this.cwButAxisEn.CheckedChanged += cwButAxisEn_CheckedChanged;
            // 
            // cwStatus
            // 
            this.cwStatus.Appearance = Appearance.Button;
            this.cwStatus.Location = new Point(240, 24);
            this.cwStatus.Name = "cwStatus";
            this.cwStatus.Size = new Size(60, 20);
            this.cwStatus.TabIndex = 34;
            this.cwStatus.Tag = "status_fb";
            this.cwStatus.OnText = "ON";
            this.cwStatus.OffText = "OFF";
            this.cwStatus.OnColor = Color.Green;
            this.cwStatus.OffColor = Color.Gray;
            this.toolTip1.SetToolTip(this.cwStatus, "Overall FB status");
            this.cwStatus.CheckedChanged += cwStatus_CheckedChanged;
            // 
            // cwX
            // 
            this.cwX.Appearance = Appearance.Button;
            this.cwX.Location = new Point(310, 24);
            this.cwX.Name = "cwX";
            this.cwX.Size = new Size(60, 20);
            this.cwX.TabIndex = 33;
            this.cwX.Tag = "loop_fb_x";
            this.cwX.OnText = "X";
            this.cwX.OffText = "X";
            this.cwX.OnColor = Color.Green;
            this.cwX.OffColor = Color.Gray;
            this.toolTip1.SetToolTip(this.cwX, "X axis FB");
            //this.cwX.CheckedChanged += cwX_CheckedChanged;
            // 
            // Add similar for cwY, cwtZ, cwZ, cwtX, cwtY with positions +70 each
            // 
            // numGainX
            // 
            this.numGainX.Location = new Point(310, 51);
            this.numGainX.Name = "numGainX";
            this.numGainX.Size = new Size(70, 20);
            this.numGainX.TabIndex = 32;
            this.numGainX.Tag = "gain_fb_x";
            this.numGainX.Value = 1.11;
            //this.numGainX.ValueChanged += numGainX_ValueChanged;
            // 
            // Add similar for numGainY, numGaintZ, numGainZ, numGaintX, numGaintY with positions +70 each
            // 
            // lblAxis2
            // 
            this.lblAxis2.BorderStyle = BorderStyle.FixedSingle;
            this.lblAxis2.Font = new Font("Arial", 11.25F, FontStyle.Bold);
            this.lblAxis2.Location = new Point(6, 0);
            this.lblAxis2.Name = "lblAxis2";
            this.lblAxis2.Size = new Size(804, 146);
            this.lblAxis2.TabIndex = 39;
            this.lblAxis2.Text = "Floor Feed Forward";
            this.lblAxis2.TextAlign = ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblAxis2, "Motor FF loops Adaptive");
            // 
            // lblFFgain0
            // 
            this.lblFFgain0.BackColor = Color.FromArgb(192, 255, 255);
            this.lblFFgain0.Font = new Font("Arial", 8.25F);
            this.lblFFgain0.Location = new Point(165, 27);
            this.lblFFgain0.Name = "lblFFgain0";
            this.lblFFgain0.Size = new Size(140, 20);
            this.lblFFgain0.TabIndex = 30;
            this.lblFFgain0.Text = "FF to Motors adaptive rate";
            this.lblFFgain0.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblFFgain1
            // 
            this.lblFFgain1.BackColor = Color.FromArgb(192, 255, 255);
            this.lblFFgain1.Font = new Font("Arial", 8.25F);
            this.lblFFgain1.Location = new Point(165, 57);
            this.lblFFgain1.Name = "lblFFgain1";
            this.lblFFgain1.Size = new Size(140, 20);
            this.lblFFgain1.TabIndex = 31;
            this.lblFFgain1.Text = "X floor FF -> X_motors";
            this.lblFFgain1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Add similar for lblFFgain2, lblFFgain3 at Top +30 each
            // 
            // numFFgain0
            // 
            this.numFFgain0.Location = new Point(310, 27);
            this.numFFgain0.Name = "numFFgain0";
            this.numFFgain0.Size = new Size(100, 20);
            this.numFFgain0.TabIndex = 29;
            this.numFFgain0.Tag = "ff_m_adapt_rate";
            this.numFFgain0.Value = 1.000E+000;
            this.numFFgain0.ValueChanged += numFFgain0_ValueChanged;
            // 
            // Add similar for numFFgain1,2,3 at Top +30 each
            // 
            // cwbutAllFFLoops
            // 
            this.cwbutAllFFLoops.Appearance = Appearance.Button;
            this.cwbutAllFFLoops.Location = new Point(140, 10);
            this.cwbutAllFFLoops.Name = "cwbutAllFFLoops";
            this.cwbutAllFFLoops.Size = new Size(180, 40);
            this.cwbutAllFFLoops.TabIndex = 28;
            this.cwbutAllFFLoops.Tag = "loop_ffa";
            this.cwbutAllFFLoops.OnText = "ALL FF Loops ON";
            this.cwbutAllFFLoops.OffText = "ALL FF Loops OFF";
            this.cwbutAllFFLoops.OnColor = Color.Blue;
            this.cwbutAllFFLoops.OffColor = Color.Gray;
            this.cwbutAllFFLoops.OnImageLayout = ImageLayout.Stretch;
            this.cwbutAllFFLoops.OffImageLayout = ImageLayout.Stretch;
            this.toolTip1.SetToolTip(this.cwbutAllFFLoops, "Controls All Feed Forward loops");
            this.cwbutAllFFLoops.CheckedChanged += cwbutAllFFLoops_CheckedChanged;
            // 
            // cwbutFFAdaptive
            // 
            this.cwbutFFAdaptive.Appearance = Appearance.Button;
            this.cwbutFFAdaptive.Location = new Point(330, 40);
            this.cwbutFFAdaptive.Name = "cwbutFFAdaptive";
            this.cwbutFFAdaptive.Size = new Size(80, 20);
            this.cwbutFFAdaptive.TabIndex = 27;
            this.cwbutFFAdaptive.Tag = "loop_mad";
            this.cwbutFFAdaptive.OnText = "Adaptive";
            this.cwbutFFAdaptive.OffText = "Adaptive"; // Likely a typo in VB6, perhaps "Not Adaptive" for Off
            this.cwbutFFAdaptive.OnColor = Color.Green;
            this.cwbutFFAdaptive.OffColor = Color.Gray;
            this.toolTip1.SetToolTip(this.cwbutFFAdaptive, "Global 'Adaptive Feed Forward' switch: If individual gain is 'Adaptive', this gain will slowly change to minimize FB signal");
            this.cwbutFFAdaptive.CheckedChanged += cwbutFFAdaptive_CheckedChanged;
            // 
            // cwbutFFtoMotors
            // 
            this.cwbutFFtoMotors.Appearance = Appearance.Button;
            this.cwbutFFtoMotors.Location = new Point(420, 10);
            this.cwbutFFtoMotors.Name = "cwbutFFtoMotors";
            this.cwbutFFtoMotors.Size = new Size(180, 40);
            this.cwbutFFtoMotors.TabIndex = 26;
            this.cwbutFFtoMotors.Tag = "ff_to_motors";
            this.cwbutFFtoMotors.OnText = "FF to Motors ON";
            this.cwbutFFtoMotors.OffText = "FF to Motors OFF";
            this.cwbutFFtoMotors.OnColor = Color.Blue;
            this.cwbutFFtoMotors.OffColor = Color.Gray;
            this.cwbutFFtoMotors.OnImageLayout = ImageLayout.Stretch;
            this.cwbutFFtoMotors.OffImageLayout = ImageLayout.Stretch;
            this.toolTip1.SetToolTip(this.cwbutFFtoMotors, "Enables FF to motors");
            this.cwbutFFtoMotors.CheckedChanged += cwbutFFtoMotors_CheckedChanged;
            // 
            // cwWorking0
            // 
            this.cwWorking0.Appearance = Appearance.Button;
            this.cwWorking0.Location = new Point(420, 27);
            this.cwWorking0.Name = "cwWorking0";
            this.cwWorking0.Size = new Size(58, 20);
            this.cwWorking0.TabIndex = 25;
            this.cwWorking0.Tag = "ff_m_work_adapt";
            this.cwWorking0.OnText = "Working";
            this.cwWorking0.OffText = "No work";
            this.cwWorking0.OnColor = Color.FromArgb(0, 128, 128);
            this.cwWorking0.OffColor = Color.FromArgb(175, 175, 175);
            this.toolTip1.SetToolTip(this.cwWorking0, "If 'ALL FF Loops ON' and 'FF to Motors ON' and 'Working': FF signal multiplied by FF gain goes to Axis");
            this.cwWorking0.CheckedChanged += cwWorking0_CheckedChanged;
            // 
            // Add similar for cwWorking1 (Top = 57), cwWorking2 (87), cwWorking3 (117)
            // 
            // cwAdaptive0
            // 
            this.cwAdaptive0.Appearance = Appearance.Button;
            this.cwAdaptive0.Location = new Point(480, 27);
            this.cwAdaptive0.Name = "cwAdaptive0";
            this.cwAdaptive0.Size = new Size(60, 20);
            this.cwAdaptive0.TabIndex = 24;
            this.cwAdaptive0.Tag = "ff_m_adapt_adapt";
            this.cwAdaptive0.OnText = "Adaptive";
            this.cwAdaptive0.OffText = "Not adapt";
            this.cwAdaptive0.OnColor = Color.FromArgb(122, 235, 126);
            this.cwAdaptive0.OffColor = Color.FromArgb(0, 224, 255);
            this.toolTip1.SetToolTip(this.cwAdaptive0, "Adaptive mode for adaptive rate");
            this.cwAdaptive0.CheckedChanged += cwAdaptive0_CheckedChanged;
            // 
            // Add similar for cwAdaptive1 (Top = 57, ToolTip "Adaptive mode for X axis"), 2,3
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new Point(50, 10);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new Size(80, 30);
            this.cmdRefresh.TabIndex = 23;
            this.cmdRefresh.Text = "REFRESH";
            this.cmdRefresh.UseVisualStyleBackColor = true;
            //this.cmdRefresh.Click += cmdRefresh_Click;
            // 
            // cmdRestorefromFlash
            // 
            this.cmdRestorefromFlash.BackColor = Color.FromArgb(192, 255, 192);
            this.cmdRestorefromFlash.Font = new Font("Arial", 9.75F, FontStyle.Bold);
            this.cmdRestorefromFlash.Location = new Point(309, 246);
            this.cmdRestorefromFlash.Name = "cmdRestorefromFlash";
            this.cmdRestorefromFlash.Size = new Size(124, 40);
            this.cmdRestorefromFlash.TabIndex = 42;
            this.cmdRestorefromFlash.Text = "Restore Params from FLASH";
            this.toolTip1.SetToolTip(this.cmdRestorefromFlash, "Sends command to controller to restore parameters from FLASH (parameters must be already saved)");
            this.cmdRestorefromFlash.UseVisualStyleBackColor = false;
            this.cmdRestorefromFlash.Click += cmdRestorefromFlash_Click;
            // 
            // cmdLoadDefaults
            // 
            this.cmdLoadDefaults.BackColor = Color.FromArgb(128, 192, 255);
            this.cmdLoadDefaults.Font = new Font("Arial", 9.75F, FontStyle.Bold);
            this.cmdLoadDefaults.Location = new Point(489, 246);
            this.cmdLoadDefaults.Name = "cmdLoadDefaults";
            this.cmdLoadDefaults.Size = new Size(124, 40);
            this.cmdLoadDefaults.TabIndex = 43;
            this.cmdLoadDefaults.Text = "Load Default Parameters";
            this.toolTip1.SetToolTip(this.cmdLoadDefaults, "Temporary sets controller parameters to factory default in RAM. Parameters in FLASH are not affected until \"save\" command");
            this.cmdLoadDefaults.UseVisualStyleBackColor = false;
            this.cmdLoadDefaults.Click += cmdLoadDefaults_Click;
            // 
            // cmdSaveParams
            // 
            this.cmdSaveParams.BackColor = Color.FromArgb(255, 255, 128);
            this.cmdSaveParams.Font = new Font("Arial", 9.75F, FontStyle.Bold);
            this.cmdSaveParams.Location = new Point(672, 246);
            this.cmdSaveParams.Name = "cmdSaveParams";
            this.cmdSaveParams.Size = new Size(124, 40);
            this.cmdSaveParams.TabIndex = 44;
            this.cmdSaveParams.Text = "Save Parameters into FLASH";
            this.toolTip1.SetToolTip(this.cmdSaveParams, "Sends command to controller to save parameters into FLASH");
            this.cmdSaveParams.UseVisualStyleBackColor = false;
            this.cmdSaveParams.Click += cmdSaveParams_Click;
            // 
            // ImageGains
            // 
            this.ImageGains.Location = new Point(6, 1);
            this.ImageGains.Name = "ImageGains";
            this.ImageGains.Size = new Size(36, 32);
            this.ImageGains.TabIndex = 45;
            this.ImageGains.TabStop = false;
            this.ImageGains.SizeMode = PictureBoxSizeMode.StretchImage;
            // 
            // FrmFloorFF
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(814, 294);
            this.Controls.Add(this.ImageGains);
            this.Controls.Add(this.cmdSaveParams);
            this.Controls.Add(this.cmdLoadDefaults);
            this.Controls.Add(this.cmdRestorefromFlash);
            this.Controls.Add(this.cmdRefresh);
            this.Controls.Add(this.cwbutAllFFLoops);
            this.Controls.Add(this.cwbutFFAdaptive);
            this.Controls.Add(this.cwbutFFtoMotors);
            this.Controls.Add(this.numFFgain0);
            this.Controls.Add(this.numFFgain1);
            this.Controls.Add(this.numFFgain2);
            this.Controls.Add(this.numFFgain3);
            this.Controls.Add(this.lblFFgain0);
            this.Controls.Add(this.lblFFgain1);
            this.Controls.Add(this.lblFFgain2);
            this.Controls.Add(this.lblFFgain3);
            this.Controls.Add(this.lblAxis2);
            this.Controls.Add(this.cmbExcitAxis);
            this.Controls.Add(this.cmdPulse);
            this.Controls.Add(this.FrameVelocityFB);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmFloorFF";
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;
            this.Text = "Feed Forward and Feed Back Gains";
            this.Load += FrmFloorFF_Load;
            this.FrameVelocityFB.ResumeLayout(false);
            this.FrameVelocityFB.PerformLayout();
            ((ISupportInitialize)(this.ImageGains)).EndInit();
            this.ResumeLayout(false);
        }

        private ToolTip toolTip1;
        private ComboBox cmbExcitAxis;
        private Button cmdPulse;
        private GroupBox FrameVelocityFB;
        private Label label1_0;
        private Label label1_13;
        private Label lblAxis1;
        private StateButton cwButAxisEn;
        private StateButton cwStatus;
        private StateButton cwX;
        private StateButton cwY;
        private StateButton cwtZ;
        private StateButton cwZ;
        private StateButton cwtX;
        private StateButton cwtY;
        private NumericEdit numGainX;
        private NumericEdit numGainY;
        private NumericEdit numGaintZ;
        private NumericEdit numGainZ;
        private NumericEdit numGaintX;
        private NumericEdit numGaintY;
        private Label lblAxis2;
        private Label lblFFgain0;
        private Label lblFFgain1;
        private Label lblFFgain2;
        private Label lblFFgain3;
        private NumericEdit numFFgain0;
        private NumericEdit numFFgain1;
        private NumericEdit numFFgain2;
        private NumericEdit numFFgain3;
        private StateButton cwbutAllFFLoops;
        private StateButton cwbutFFAdaptive;
        private StateButton cwbutFFtoMotors;
        private StateButton cwWorking0;
        private StateButton cwAdaptive0;
        private StateButton cwWorking1;
        private StateButton cwAdaptive1;
        private StateButton cwWorking2;
        private StateButton cwAdaptive2;
        private StateButton cwWorking3;
        private StateButton cwAdaptive3;
        private Button cmdRefresh;
        private Button cmdRestorefromFlash;
        private Button cmdLoadDefaults;
        private Button cmdSaveParams;
        private PictureBox ImageGains;
    }
}