using System;
using System.Windows.Forms;

namespace TMCAnalyzer.CustomClasses {
    public class ScientificNumericUpDown:NumericUpDown {
        protected override void UpdateEditText() {
            if (Value == 0) {
                this.Text = "0";
            } else {
                this.Text = Value.ToString("0.000E+000");
            }
        }

        // Override to handle parsing from scientific input
        protected override void ValidateEditText() {
            try {
                Value = Decimal.Parse(Text, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowExponent);
            } catch {
                // Revert to previous value if invalid
                UpdateEditText();
            }
            base.ValidateEditText();
        }
    }
}
