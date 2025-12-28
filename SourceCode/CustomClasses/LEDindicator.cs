using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NationalInstruments.UI;

namespace TMCAnalyzer {
	/// <summary>
	/// Customized control that adds to base NI control "Led"
	/// 1. The text label with fonts, foregraunds, and locations separate for Checked / Unchecked
	/// 2. The extra "blink" color. The base NI class blinks between OnColor - OffColor
	///    which is not enough and not compatible with older ComponetWorks control
	///    The extra color allows to blink NOT between On/Off colors but between "ON - blink" and "OFF - blink" colors
	///    The base property "OnColor" and "OffColor" are hidden in design and run time and replaced with exposed properties of this class
	/// </summary>
	public class LEDindicator:NationalInstruments.UI.WindowsForms.Led {
		public Color LEDtextColor;		// color of LEDtext
		public Color OriginalBlinkDimColor;	// color when blinking - substituting On / OffColor
		public Color OriginalOnColor;	// saved original color when blinking
		public Color OriginalOffColor;	// saved original color when blinking

		public Font LEDtextFont;		// LEDtext font
		public Point LEDtextLocation;	// LEDtext location
		public Size LEDtextSize;		// LEDtext Size
		public bool LEDtextAutoSize;    // LEDtext AutoSize

		private readonly Size DefaultTextSize = new Size(20, 10); // something not too small or large to use during initialization
		private string _onText;         // Text when on
		private string _offText;        // Text when off
		private Color _onTextColor;     // text color when on
		private Color _offTextColor;    // text color when off
		private Color _OnColor;			// color when on
		private Color _OffColor;		// color when off
		private Color _BlinkDimColor;   // color when LED is blinking - need "dimmer" color because NI LED blinks OnColor - OffColor
		private Font _onTextFont;       // text font when on
		private Font _offTextFont;      // text font when off
		private Point _onTextLocation;  // text location when on
		private Point _offTextLocation; // text location when off
		private bool _LEDtextVisible;   // text Visible
		private bool _LEDtextCentered;  // text centered on LED

		//constructor is called from *.Designer.cs: "this.NI_LED = new TMCAnalyzer.LEDindicator();
		// public void LEDindicator() turns constractor into ordinary method, "void" is incorrect
		public LEDindicator() {
			base.CreateControl();
			LEDtextFont = base.CaptionFont;
			_onTextFont = base.CaptionFont;
			_offTextFont = base.CaptionFont;
			_LEDtextVisible = true;
			_onTextColor = base.CaptionBackColor;
			_offTextColor = base.CaptionBackColor;
			_onText = "ON";
			_offText = "OFF";
			//base.OnColor = Color.Aqua;	//test, at ctor time, base.OnColor = Lime
			//base.OffColor = Color.Beige;	//test, at ctor time, base.OnColor = Darkgreen
			//OriginalOnBlinkDimColor = this.OnBlinkDimColor; // at ctor time (when called from *.Designer.cs) this property is Empty
			//OriginalOnColor = this.OnColor;		// at ctor time, color is Empty
			//OriginalOffColor = this.OffColor;		// at ctor time, color is Empty
			//_onTextLocation = new Point(1, 1);	//test, at ctor time, {0, 0}
			//_offTextLocation = new Point(2, 2);	//test, at ctor time, {0, 0}
			//_onTextAutoSize = true;	//test, at ctor time, false
			//_offTextAutoSize = true;	//test, at ctor time, false

			this.StateChanged += new NationalInstruments.UI.ActionEventHandler(LEDindicator_StateChanged);
		}

		public string OrgOnColor {
			get { return base.OnColor.ToString(); }
		}
		public string OrgOffColor {
			get { return base.OffColor.ToString(); }
		}

		// LEDtext as a property to see in Designer and to show on control
		public string LEDtext {
			get { return (Value) ? OnText : OffText; }
		}

		public bool LEDtextVisible {
			get { return _LEDtextVisible; }
			set {
				_LEDtextVisible = value;
				UpdateLedTextProperties();
			}
		}

		// LED_Text
		public string OnText {
			get { return _onText; }
			set {
				_onText = value;
				UpdateLedTextProperties();
			}
		}
		public string OffText {
			get { return _offText; }
			set {
				_offText = value;
				UpdateLedTextProperties();
			}
		}
		/// <summary>
		/// This property is called AFTER ctor created object, when *.Designer.cs reaches statement assigning actual Color
		/// "this.NI_Led.OnBlinkDimColor = System.Drawing.Color.DarkGreen;"
		/// When blinking, the "dim" color - to be different from ON/OFF color
		/// </summary>
		public System.Drawing.Color OnBlinkDimColor {
			get { return _BlinkDimColor; }
			set {
				_BlinkDimColor = value;
				if(value != Color.Empty )  //as soon as color is assigned in *.Designer.cs file, update class variable
					OriginalBlinkDimColor = _BlinkDimColor;
			}
		}

		/// <summary>
		/// Hides NI definition of OnColor, substitutes it with new definition so the base.OnColor is hidden
		/// because it would change based on Blink setting
		/// ------ Changing base.OnColor is actually changing LED color on control -----
		/// the NI_LED.OnColor is still accessible using base.OnColor
		/// This property is called AFTER ctor created object, when *.Designer.cs reaches statement assigning actual Color
		/// "this.NI_Led.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));"
		/// </summary>
		public new System.Drawing.Color OnColor {
			get { return _OnColor; }
			set {
				_OnColor = (value);
				if(value != Color.Empty )
					OriginalOnColor = _OnColor;   //as soon as color is assigned in *.Designer.cs file, update class variable
			}
		}

		/// <summary>
		/// Hides NI definition of OffColor, substitutes it with new definition so the base.OnColor is hidden
		/// because it would change based on Blink setting
		/// ------ Changing base.OffColor is actually changing LED color on control -----
		/// the NI_LED.OffColor is still accessible using base.OffColor
		/// Changing base.OffColor is actually changing LED color on control
		/// This property is called AFTER ctor created object, when *.Designer.cs reaches statement assigning actual Color
		/// "this.NI_Led.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));"
		/// </summary>
		public new System.Drawing.Color OffColor {
			get { return _OffColor; }
			set {
				_OffColor = (value);
				if(value != Color.Empty )
					OriginalOffColor = _OffColor;   //as soon as color is assigned in *.Designer.cs file, update class variable
			}
		}

		// foreground (LED_Text) color
		public System.Drawing.Color OnTextColor {
			get { return _onTextColor; }
			set {
				_onTextColor = (value == Color.Empty ? Color.Black : value);
				UpdateLedTextProperties();
			}
		}
		public System.Drawing.Color OffTextColor {
			get { return _offTextColor; }
			set {
				_offTextColor = (value == Color.Empty ? Color.Black : value);
				UpdateLedTextProperties();
			}
		}

		// LED_Text font
		public System.Drawing.Font OnTextFont {
			get { return _onTextFont; }
			set {
				_onTextFont = (value == null ? new Font("Arial", 8.75F, FontStyle.Regular) : value);	// need to be defined even if "null" is passed
				UpdateLedTextProperties();
			}
		}
		public System.Drawing.Font OffTextFont {
			get { return _offTextFont; }
			set {
				_offTextFont = (value == null ? new Font("Courier New", 7.75F, FontStyle.Regular) : value);	// need to be defined even if "null" is passed
				UpdateLedTextProperties();
			}
		}

		/// <summary>
		/// LED_Text (the actual text shown) location is updated from either OnTextLocation or OffTextLocation
		/// This property is called AFTER ctor created object, when *.Designer.cs reaches statement assigning actual location
		/// "this.NI_Led.OnTextLocation = new System.Drawing.Point(20, 20);"
		/// </summary>
		public System.Drawing.Point OnTextLocation {
			get { return _onTextLocation; }
			set {
				_onTextLocation = value;
				UpdateLedTextProperties();
			}
		}
		public System.Drawing.Point OffTextLocation {
			get { return _offTextLocation; }
			set {
				_offTextLocation = value;
				UpdateLedTextProperties();
			}
		}
#if USE_TEXT_Size // The DrawLEDtext() does not use Size, it calculates rectangle based on string and Font
		private Size _onTextSize;       // text Size when on
		private Size _offTextSize;      // text Size when off
		private bool _onTextAutoSize;   // text AutoSize when on
		private bool _offTextAutoSize;  // text AutoSize when off

		/// <summary>
		/// LED_Text.Size is updated either from OnTextSize or OfTextSize
		/// It defines rectangle for Text if OnTextAutoSize == false
		/// and it should not be too small
		/// </summary>
		public System.Drawing.Size OnTextSize {
			get { return _onTextSize; }
			set {
				_onTextSize = (value == Size.Empty ? DefaultTextSize : value);  // need to be defined even if {0,0} is passed
				UpdateLedTextProperties();
			}
		}
		public System.Drawing.Size OffTextSize {
			get { return _offTextSize; }
			set {
				_offTextSize = (value == Size.Empty ? DefaultTextSize : value);  // need to be defined even if {0,0} is passed
				UpdateLedTextProperties();
			}
		}

		// LED_Text AutoSize
		public bool OnTextAutoSize {
			get { return _onTextAutoSize; }
			set {
				_onTextAutoSize = value;
				UpdateLedTextProperties();
			}
		}
		public bool OffTextAutoSize {
			get { return _offTextAutoSize; }
			set {
				_offTextAutoSize = value;
				UpdateLedTextProperties();
			}
		}
#endif
		public bool LEDTextCentered {
			get { return _LEDtextCentered; }
			set {
				_LEDtextCentered = value;
				UpdateLedTextProperties();
			}
		}

		/// <summary>
		/// Update LED_Text properties based on the current settings
		/// </summary>
		private void UpdateLedTextProperties() {
			// happens in DrawLEDtext if (!LEDTextCentered) LEDtextLocation = (Value) ? OnTextLocation : OffTextLocation;

			LEDtextColor = (Value) ? OnTextColor : OffTextColor;
			LEDtextFont = (Value) ? OnTextFont : OffTextFont;
#if USE_TEXT_Size // The DrawLEDtext() does not use Size, it calculates rectangle based on string and Font
			LEDtextSize = (Value) ? OnTextSize : OffTextSize;
			LEDtextAutoSize = (Value) ? OnTextAutoSize : OffTextAutoSize;
#endif
			this.Invalidate();
		}

		//draw label on the top of LED function
		void DrawLEDtext(Graphics g) {
			LEDtextColor = (Value) ? OnTextColor : OffTextColor;
			string TextToShow = (Value) ? OnText : OffText; // LEDtext is a property and is copied from either OnText or OffText, depends on the Value
			var gRect = g.ClipBounds;	// Correction for Caption happens automatically because g.ClipBonds changes with / without caption

			if (LEDTextCentered){
				SizeF TXTsize = g.MeasureString(TextToShow, LEDtextFont);
				string BaseCaption = Caption;
				float CaptionXcorrection = 0;
				float CaptionYcorrection = -TXTsize.Height / 2;
				// without Caption:                 gRect = {X = 6 Y = 6 Width = 208 Height = 188};
				// WITH Caption="Advanced LED Demo" gRect = {X = 6 Y = 29 Width = 208 Height = 165}
				//LEDtextLocation.X = (int)(gRect.Location.X + (gRect.Width - TXTsize.Width) / 2); // + CaptionXcorrection
				//LEDtextLocation.Y = (int)(gRect.Location.Y + (gRect.Height - TXTsize.Height) / 2 );// + CaptionYcorrection
				if ((BaseCaption.Length != 0)  && CaptionVisible){
					SizeF CAPTIONsize = g.MeasureString(BaseCaption, CaptionFont);
					switch (CaptionPosition) {
						case CaptionPosition.Top:
							CaptionYcorrection = CAPTIONsize.Height;
							break;
						case CaptionPosition.Bottom:
							CaptionYcorrection = -CAPTIONsize.Height;
							break;
						case CaptionPosition.Left:
							CaptionXcorrection = CAPTIONsize.Height;
							break;
						case CaptionPosition.Right:
							CaptionXcorrection = -CAPTIONsize.Height;
							break;
					}
				}
				LEDtextLocation.X = (int)( (Width - TXTsize.Width + CaptionXcorrection) / 2); //
				LEDtextLocation.Y = (int)( (Height - TXTsize.Height/2 + CaptionYcorrection) / 2);//
			} else {
				LEDtextLocation = (Value) ? OnTextLocation : OffTextLocation; // the Value is taken from base class
			}

			if (LEDtextVisible) {
				SolidBrush frcolor = new SolidBrush(LEDtextColor);
				//if (gRect.Location.X > 0)
				{ // during switch ON/OFF/ON gRect.Location becomes 0,0 which does shift Text location once; on next entry, gRect.Location becomes real, >0
					g.DrawString(TextToShow, LEDtextFont, frcolor, LEDtextLocation);
					//System.Diagnostics.Debug.Print("{0:HH:mm:ss.ff} | State = {1} ClipBounds = {2}: LEDtextLocation = {3}", DateTime.Now, Value, gRect.ToString(), LEDtextLocation.ToString());
				}
			}
		}

		void LEDindicator_StateChanged(object sender, EventArgs e) {
			//base.OnColor = this.OnColor;
			//base.OffColor = this.OffColor;
			UpdateLedTextProperties();
		}

		/// <summary>
		/// Changing LED color of a shown control on a Form  is achieved by chaging base.OnColor, base.OffColor.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e) {
			if (this.BlinkMode == LedBlinkMode.None) {
				base.OnColor = OriginalOnColor;
				base.OffColor = OriginalOffColor;
			} else {
				if (this.BlinkMode == LedBlinkMode.BlinkWhenOff) {
					if (this.Value == false) {
						base.OnColor = OriginalOnColor;
						base.OffColor = OriginalBlinkDimColor;
					} else {
						base.OnColor = OriginalOnColor;
						base.OffColor = OriginalOffColor;
					}
				}
				if (this.BlinkMode == LedBlinkMode.BlinkWhenOn) {
					if (this.Value == true) {
						base.OnColor = OriginalOnColor;
						base.OffColor = OriginalBlinkDimColor;
					} else {
						base.OnColor = OriginalOnColor;
						base.OffColor = OriginalOffColor;
					}
				}
			}
			base.OnPaint(e);
			this.DrawLEDtext(e.Graphics);
		}

		protected override void OnStateChanged(ActionEventArgs e) {
			base.OnStateChanged(e);
			Invalidate(); // to clear remnants of previous LEDtext
		}
	}
}
