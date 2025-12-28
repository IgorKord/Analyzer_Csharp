using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;


namespace TMCAnalyzer {
	/// <summary>
	/// Custom control that adds state properties
	/// custom ON and OFF text, ON and OFF pictures
	/// </summary>
	public class StateButton : System.Windows.Forms.CheckBox
	{
		private Color _onColor;     // background color when on
		private Color _offColor;    // background color when off
		private string _onText;     // Text when on
		private string _offText;    // Text when off
		private Color _onTextColor;     // text color when on
		private Color _offTextColor;    // text color when off
		private Image _onPicture;     // Picture when on
		private Image _offPicture;    // Picture when off
		private ImageLayout _onImageLayout;    // Picture layout when on (strech, tile, etc)
		private ImageLayout _offImageLayout;    // Picture layout when off (strech, tile, etc)

		// background color
		public System.Drawing.Color OnColor {
			get { return _onColor; }
			set {
				_onColor = value;
				UpdateParentProperties();
			}
		}
		public System.Drawing.Color OffColor {
			get { return _offColor; }
			set {
				_offColor = value;
				UpdateParentProperties();
			}
		}

		// background picture
		public System.Drawing.Image OnPicture {
			get { return _onPicture; }
			set {
				_onPicture = value;
				UpdateParentProperties();
			}
		}
		public System.Drawing.Image OffPicture {
			get { return _offPicture; }
			set {
				_offPicture = value;
				UpdateParentProperties();
			}
		}

		// background picture layout
		public ImageLayout OnImageLayout {
			get { return _onImageLayout; }
			set {
				_onImageLayout = value;
				UpdateParentProperties();
			}
		}
		public ImageLayout OffImageLayout {
			get { return _offImageLayout; }
			set {
				_offImageLayout = value;
				UpdateParentProperties();
			}
		}

		// button text
		public string OnText {
			get { return _onText; }
			set {
				_onText = value;
				UpdateParentProperties();
			}
		}
		public string OffText {
			get { return _offText; }
			set {
				_offText = value;
				UpdateParentProperties();
			}
		}

		// button color of text (font color)
		public System.Drawing.Color OnTextColor {
			get { return _onTextColor; }
			set {
				_onTextColor = value;
				UpdateParentProperties();
			}
		}
		public System.Drawing.Color OffTextColor {
			get { return _offTextColor; }
			set {
				_offTextColor = value;
				UpdateParentProperties();
			}
		}

		public StateButton() {
			this.CheckedChanged += new EventHandler(StateButton_CheckedChanged);
		}

		/// <summary>
		/// Update the properties based on the current settings
		/// </summary>
		private void UpdateParentProperties() {
			Text = (Checked) ? OnText : OffText;
			ForeColor = (Checked) ? OnTextColor : OffTextColor;
			BackColor = (Checked) ? OnColor : OffColor;
			BackgroundImage = (Checked) ? OnPicture : OffPicture;
			BackgroundImageLayout = (Checked) ? OnImageLayout : OffImageLayout; ;
		}

		void StateButton_CheckedChanged(object sender, EventArgs e) {
			UpdateParentProperties();
		}
	}
}
