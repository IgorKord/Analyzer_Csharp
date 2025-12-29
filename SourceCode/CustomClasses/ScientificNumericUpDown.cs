using System;
using System.Windows.Forms;

namespace TMCAnalyzer.CustomClasses {
    public class ScientificNumericUpDown:NumericUpDown {
        public new void UpdateEditText()  // Made public by shadowing the protected method
        {
            if (Value == 0) {
                Text = "0";
            } else {
                Text = Value.ToString("0.000E+000");
            }
        }

        protected override void ValidateEditText() {
            try {
                Value = decimal.Parse(Text, System.Globalization.NumberStyles.Any);
            } catch {
                // Revert to previous if invalid
                UpdateEditText();
            }
            base.ValidateEditText();
        }
    }
}