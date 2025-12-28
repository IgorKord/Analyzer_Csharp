using System;

namespace TMCAnalyzer {
    /// <summary/>
    public enum AlarmStates {
        /// <summary/>
        On,
        /// <summary/>
        Off,
        /// <summary/>
        Clear
    }

    public partial class TMCController {
        /// <summary>Access alarm</summary>
        /// <value>The alarm</value>
        public AlarmStates MonitoringForAlarms {
            get {
                var /*response*/ rsp = string.Empty;
                var /*command*/ cmd = "alarm>?";
                SendInternal(cmd, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

                // strip off "alarm>" from the response
                const string /*text to be trimmed*/ TRM = "alarm>";
                rsp = rsp.Substring(TRM.Length, rsp.Length - TRM.Length);

                // convert the type from "PASSIVE" and "ACTIVE" to a loop mode
                if (rsp.Equals("on", StringComparison.OrdinalIgnoreCase)) {
                    return AlarmStates.On;
                } else if (rsp.Equals("off", StringComparison.OrdinalIgnoreCase)) {
                    return AlarmStates.Off;
                } else { throw new System.InvalidOperationException("Unknown response of \"" + rsp + "\""); }
            }
            set {
                if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
                    Tools.IsOK(value, /*exception if bad*/ true);
                    var /*command*/ cmd = string.Empty;
                    if (value == AlarmStates.On) {
                        cmd = "alarm>on";
                    } else if (value == AlarmStates.Off) {
                        cmd = "alarm>off";
                    } else if (value == AlarmStates.Clear) {
                        cmd = "alarm>clear";
                    }
                    var /*response (unused)*/ rsp = string.Empty;
                    SendInternal(cmd, CommandTypes.NoResponseExpected, out rsp, CommandAckTypes.AckExpected);
                }
            }
        }
        /// <summary/>
        public int AlarmLimit {
            get {
                var /*command*/ cmd = "alarm?";
                var /*response*/ rsp = string.Empty;
                SendInternal(cmd, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

                // strip off "alarm=" from the response
                const string /*text to be trimmed*/ TRM = "alarm=";
                rsp = rsp.Substring(TRM.Length, rsp.Length - TRM.Length);
                return System.Int32.Parse(rsp);
            }
            set {
                if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
                    var /*command*/ cmd = string.Format("alarm={0}", value);
                    var /*response (unused)*/ rsp = string.Empty;
                    SendInternal(cmd, CommandTypes.NoResponseExpected, out rsp, CommandAckTypes.AckExpected);
                }
            }
        }
    }
}