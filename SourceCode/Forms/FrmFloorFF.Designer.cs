using System.Windows.Forms;
using NationalInstruments.UI;
namespace TMCAnalyzer.Forms {
    partial class FrmFloorFF {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
			this.cmbExcitAxis = new System.Windows.Forms.ComboBox();
			this.cmdPulse = new System.Windows.Forms.Button();
			this.FrameVelocityFB = new System.Windows.Forms.GroupBox();
			this.cwButAxisEn = new TMCAnalyzer.StateButton();
			this.cwAdaptive1 = new TMCAnalyzer.StateButton();
			this.cwAdaptive2 = new TMCAnalyzer.StateButton();
			this.cwAdaptive3 = new TMCAnalyzer.StateButton();
			this.cwAdaptive4 = new TMCAnalyzer.StateButton();
			this.FrameVelocityFB.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmbExcitAxis
			// 
			this.cmbExcitAxis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbExcitAxis.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
			this.cmbExcitAxis.FormattingEnabled = true;
			this.cmbExcitAxis.Items.AddRange(new object[] {
            "X",
            "Y",
            "Z",
            "tX",
            "tY",
            "tZ"});
			this.cmbExcitAxis.Location = new System.Drawing.Point(1530, 3960);
			this.cmbExcitAxis.Name = "cmbExcitAxis";
			this.cmbExcitAxis.Size = new System.Drawing.Size(2055, 23);
			this.cmbExcitAxis.TabIndex = 41;
			// 
			// cmdPulse
			// 
			this.cmdPulse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.cmdPulse.Font = new System.Drawing.Font("Arial", 9F);
			this.cmdPulse.Location = new System.Drawing.Point(90, 3600);
			this.cmdPulse.Name = "cmdPulse";
			this.cmdPulse.Size = new System.Drawing.Size(1095, 735);
			this.cmdPulse.TabIndex = 40;
			this.cmdPulse.Text = "Apply Pulse";
			this.cmdPulse.UseVisualStyleBackColor = false;
			this.cmdPulse.Click += new System.EventHandler(this.cmdPulse_Click);
			// 
			// FrameVelocityFB
			// 
			this.FrameVelocityFB.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.FrameVelocityFB.Controls.Add(this.cwButAxisEn);
			this.FrameVelocityFB.Controls.Add(this.cwAdaptive1);
			this.FrameVelocityFB.Location = new System.Drawing.Point(90, 2295);
			this.FrameVelocityFB.Name = "FrameVelocityFB";
			this.FrameVelocityFB.Size = new System.Drawing.Size(12075, 1215);
			this.FrameVelocityFB.TabIndex = 22;
			this.FrameVelocityFB.TabStop = false;
			// 
			// cwButAxisEn
			// 
			this.cwButAxisEn.Location = new System.Drawing.Point(90, 360);
			this.cwButAxisEn.Name = "cwButAxisEn";
			this.cwButAxisEn.OffColor = System.Drawing.Color.Empty;
			this.cwButAxisEn.OffImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwButAxisEn.OffPicture = null;
			this.cwButAxisEn.OffText = null;
			this.cwButAxisEn.OffTextColor = System.Drawing.Color.Empty;
			this.cwButAxisEn.OnColor = System.Drawing.Color.Empty;
			this.cwButAxisEn.OnImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwButAxisEn.OnPicture = null;
			this.cwButAxisEn.OnText = null;
			this.cwButAxisEn.OnTextColor = System.Drawing.Color.Empty;
			this.cwButAxisEn.Size = new System.Drawing.Size(2715, 600);
			this.cwButAxisEn.TabIndex = 35;
			this.cwButAxisEn.Click += new System.EventHandler(this.cwButAxisEn_Click);
			// 
			// cwAdaptive1
			// 
			this.cwAdaptive1.Location = new System.Drawing.Point(7425, 855);
			this.cwAdaptive1.Name = "cwAdaptive1";
			this.cwAdaptive1.OffColor = System.Drawing.Color.Empty;
			this.cwAdaptive1.OffImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive1.OffPicture = null;
			this.cwAdaptive1.OffText = null;
			this.cwAdaptive1.OffTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive1.OnColor = System.Drawing.Color.Empty;
			this.cwAdaptive1.OnImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive1.OnPicture = null;
			this.cwAdaptive1.OnText = null;
			this.cwAdaptive1.OnTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive1.Size = new System.Drawing.Size(900, 300);
			this.cwAdaptive1.TabIndex = 16;
			this.cwAdaptive1.Click += new System.EventHandler(this.cwAdaptive1_Click);
			// 
			// cwAdaptive2
			// 
			this.cwAdaptive2.Location = new System.Drawing.Point(0, 0);
			this.cwAdaptive2.Name = "cwAdaptive2";
			this.cwAdaptive2.OffColor = System.Drawing.Color.Empty;
			this.cwAdaptive2.OffImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive2.OffPicture = null;
			this.cwAdaptive2.OffText = null;
			this.cwAdaptive2.OffTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive2.OnColor = System.Drawing.Color.Empty;
			this.cwAdaptive2.OnImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive2.OnPicture = null;
			this.cwAdaptive2.OnText = null;
			this.cwAdaptive2.OnTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive2.Size = new System.Drawing.Size(104, 24);
			this.cwAdaptive2.TabIndex = 0;
			// 
			// cwAdaptive3
			// 
			this.cwAdaptive3.Location = new System.Drawing.Point(0, 0);
			this.cwAdaptive3.Name = "cwAdaptive3";
			this.cwAdaptive3.OffColor = System.Drawing.Color.Empty;
			this.cwAdaptive3.OffImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive3.OffPicture = null;
			this.cwAdaptive3.OffText = null;
			this.cwAdaptive3.OffTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive3.OnColor = System.Drawing.Color.Empty;
			this.cwAdaptive3.OnImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive3.OnPicture = null;
			this.cwAdaptive3.OnText = null;
			this.cwAdaptive3.OnTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive3.Size = new System.Drawing.Size(104, 24);
			this.cwAdaptive3.TabIndex = 0;
			// 
			// cwAdaptive4
			// 
			this.cwAdaptive4.Location = new System.Drawing.Point(0, 0);
			this.cwAdaptive4.Name = "cwAdaptive4";
			this.cwAdaptive4.OffColor = System.Drawing.Color.Empty;
			this.cwAdaptive4.OffImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive4.OffPicture = null;
			this.cwAdaptive4.OffText = null;
			this.cwAdaptive4.OffTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive4.OnColor = System.Drawing.Color.Empty;
			this.cwAdaptive4.OnImageLayout = System.Windows.Forms.ImageLayout.None;
			this.cwAdaptive4.OnPicture = null;
			this.cwAdaptive4.OnText = null;
			this.cwAdaptive4.OnTextColor = System.Drawing.Color.Empty;
			this.cwAdaptive4.Size = new System.Drawing.Size(104, 24);
			this.cwAdaptive4.TabIndex = 0;
			// 
			// FrmFloorFF
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(5124, 1421);
			this.Controls.Add(this.cmbExcitAxis);
			this.Controls.Add(this.cmdPulse);
			this.Controls.Add(this.FrameVelocityFB);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmFloorFF";
			this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
			this.Text = "Feed Forward and Feed Back Gains";
			this.FrameVelocityFB.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        private ComboBox cmbExcitAxis;
        private System.Windows.Forms.ToolTip toolTip1;
        private Button cmdPulse;
        private GroupBox FrameVelocityFB;
        private TMCAnalyzer.StateButton cwButAxisEn;
        private TMCAnalyzer.StateButton cwAdaptive1;
        private TMCAnalyzer.StateButton cwAdaptive2;
        private TMCAnalyzer.StateButton cwAdaptive3;
        private TMCAnalyzer.StateButton cwAdaptive4;
    }
}