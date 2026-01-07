using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace TMCAnalyzer.CustomClasses
{
	public class ScientificNumericUpDown:NumericUpDown
	{
		/// <summary>
		/// Enum representing numeric display modes (Regular or Scientific).
		/// </summary>
		public enum NumericFormatMode
		{
			Regular,
			Scientific
		}

		private TextBox _overlayTextBox; // Overlay text box
		private NumericFormatMode _formatMode = NumericFormatMode.Regular;

		public ScientificNumericUpDown()
		{
			this.DecimalPlaces = 3; // Default precision
			this.Increment = 1m;    // Default increment value
			this.Minimum = -100m;   // Default minimum value
			this.Maximum = 100m;    // Default maximum value

			// Create overlay TextBox
			_overlayTextBox = new TextBox
			{
				BorderStyle = BorderStyle.None, // Remove borders
				ReadOnly = true,               // Prevent direct editing
				TabStop = false,               // Ignore tab navigation
				TextAlign = HorizontalAlignment.Right,
				Font = this.Font,              // Inherit font
				ForeColor = this.ForeColor,    // Inherit foreground color
				BackColor = this.BackColor     // Inherit background color
			};

			this.Controls.Add(_overlayTextBox); // Add overlay to NumericUpDown
			this.Controls.SetChildIndex(_overlayTextBox, 0);

			// Attach event handlers
			this.ValueChanged += UpdateOverlayTextHandler;   // Update overlay text on value change
			this.SizeChanged += ResizeOverlayHandler;        // Adjust overlay dimensions dynamically
			this.FontChanged += SynchronizeOverlayAppearance; // Sync font with overlay
			this.ForeColorChanged += SynchronizeOverlayAppearance; // Sync foreground color
			this.BackColorChanged += SynchronizeOverlayAppearance; // Sync background color

			// Initial placement and sizing of the overlay
			ResizeOverlayHandler(null, null);
		}

		/// <summary>
		/// Specifies whether the numeric value should be displayed in Regular or Scientific format.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public NumericFormatMode FormatMode
		{
			get => _formatMode;
			set
			{
				_formatMode = value;
				UpdateScientificText(); // Immediately update the overlay text
			}
		}

		/// <summary>
		/// Alias for 'Increment', kept for compatibility with older code that used 'CoercionInterval'.
		/// </summary>
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public decimal CoercionInterval
		{
			get => Increment;
			set => Increment = value; // Syncs with Increment
		}

		/// <summary>
		/// Updates the overlay TextBox text based on the numeric value and format mode.
		/// </summary>
		internal void UpdateScientificText()		{
			if (_formatMode == NumericFormatMode.Scientific)
			{
				_overlayTextBox.Text = Value.ToString("0.000E+000", CultureInfo.InvariantCulture);
			} else
			{
				_overlayTextBox.Text = Value.ToString($"F{DecimalPlaces}", CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// Handles ValueChanged to update overlay text dynamically.
		/// </summary>
		private void UpdateOverlayTextHandler(object sender, EventArgs e)
		{
			UpdateScientificText();
		}

		/// <summary>
		/// Resizes and repositions the overlay TextBox to match the NumericUpDown text area.
		/// </summary>
		private void ResizeOverlayHandler(object sender, EventArgs e)
		{
			// Adjust position and size to match text area
			int padding = 2; // Adjust for borders
			_overlayTextBox.Location = new Point(padding, padding);
			_overlayTextBox.Size = new Size(this.Width - (2 * padding), this.Height - (2 * padding));
		}

		/// <summary>
		/// Synchronizes overlay appearance with the NumericUpDown control (font, colors).
		/// </summary>
		private void SynchronizeOverlayAppearance(object sender, EventArgs e)
		{
			_overlayTextBox.Font = this.Font;
			_overlayTextBox.ForeColor = this.ForeColor;
			_overlayTextBox.BackColor = this.BackColor;
		}
	}
}