namespace TMCAnalyzer {
    /// <summary/>
    public enum SettingsActions {
        /// <summary/>
        SaveRamSettingsToFlash,
        /// <summary/>
        SetFactoryDefaultsToRamSettings
    }
    public partial class TMCController {
        /// <summary/>
        public SettingsActions SettingsInRam {
            set {
                Tools.IsOK(value, /*exception*/ true);
                var /*command*/ cmd = string.Empty;
                if (value == SettingsActions.SaveRamSettingsToFlash) {
                    cmd = "save";
                } else if (value == SettingsActions.SetFactoryDefaultsToRamSettings) {
                    cmd = "dflt";
                }
                var /*response (unused)*/ rsp = string.Empty;
                SendInternal(cmd, CommandTypes.NoResponseExpected, out rsp, CommandAckTypes.AckExpected);
            }
        }
    }
}