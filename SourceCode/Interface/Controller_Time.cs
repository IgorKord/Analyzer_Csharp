namespace TMCAnalyzer {
    public partial class TMCController {
        /// <summary>
        /// Sets the time.
        /// NOTE: Includes only the Time, not the date
        /// </summary>
        /// <value><see cref="System.DateTime"/></value>
        public System.DateTime Time {
            get {
                var /*command*/ cmd = "time?";
                var /*response*/ rsp = string.Empty;
                SendInternal(cmd, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

                // strip off the command text from the response
                const string /*text to be trimmed*/ TRM = "time=";
                rsp = rsp.Substring(TRM.Length, rsp.Length - TRM.Length);

                // split the string with ':'
                var /*delimiter(s)*/ dlm = rsp.Split(':');
                return new System.DateTime(1970, 1, 1, int.Parse(dlm[0]), int.Parse(dlm[1]), int.Parse(dlm[2]), 0);
            }
            set {
                if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
                    var /*command*/ cmd = string.Format("time={0:D2}:{1:D2}:{2:D2}", value.Hour, value.Minute, value.Second);
                    var /*response (unused)*/ rsp = string.Empty;
                    SendInternal(cmd, CommandTypes.NoResponseExpected, out rsp, CommandAckTypes.AckExpected);
                }
            }
        }
    }
}