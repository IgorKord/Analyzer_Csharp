using System;

namespace TMCAnalyzer {

    /// <summary>Represents a controller channel</summary>
    public class ChannelCollection {
        private Channel[] channelBase = new Channel[] {/*empty; set in _ctor*/};


        /// <summary>
        /// Size specifies the number of <see cref="Channel"/>s in the collection.</summary>
        public const int ChannelCount = /*=3 must be at least 1*/ Program.MaxScopeChannels;

        internal ChannelCollection() {
            channelBase = new Channel[ChannelCount];
            for (var /*index*/ ix = 0; ix < ChannelCount; ix++) { channelBase[ix] = new Channel( ix); }
        }

        /// <summary>
        /// Channel Count gets the number of <see cref="ScopeChannels"/>.</summary>
        /// <returns>number of <see cref="ScopeChannels"/></returns>
        //public int ChannelCount {
        //    get {
        //        var /*possible indices*/ idc = System.Enum.GetValues(typeof(ScopeChannels));
        //        return idc.Length;
        //    }
        //}

        /// <summary>
        /// Get, by index, a <see cref="Channel"/> from this instance's channel
        /// collection.</summary>
        /// <param name='ChSelectorIndex'>
        /// Channel Index specifies the index of the desired <see cref="Channel"/>.</param>
        /// <exception cref="System.ArgumentOutOfRange">
        /// If the value, specified for <paramref name="ChSelectorIndex"/>, is invalid, an exception
        /// is thrown.</exception>
        /// <example><code>
        /// var /*analyzer*/ anz = new TMCController(</code>...<code>
        /// var /*collection*/ clc = new ChannelCollection(anz);
        /// clc[3].ChVariableIndx = ChannelModes.Excitation;</code></example>
        public Channel this[int channelIndex] {
            get {
                Tools.IsOK(channelIndex, 0, channelBase.Length - 1, /*exception if bad*/ true);
                return channelBase[channelIndex];
            }
        }
    }

    /// <summary>Represents a controller channel</summary>
    public class Channel {
        internal Channel(int channelIndex) {
//            if (/*bad?*/ controller == null) { throw new System.ArgumentNullException(); }
            Tools.IsOK(channelIndex, 0, /*typically 16*/ DataSample.ChannelsPerSample - 1, /*exception if bad*/ true);
            channelIndexBase = channelIndex;
        }

        private int channelIndexBase = 0;


        /// <summary>
        /// Channel Names accesses a vector of all known channel names 
        ///(may be updated dynamically).</summary>
        public string[] ChannelNames {
            get {
                if (/*prepare?*/ channelNamesBase == null) { ChannelNames = new string[] { /*doesn't matter; it's the set that counts*/ }; }
                return channelNamesBase;
            }
            set {
                // todo: for (var /*index*/ ix = 0; ix < 256; ix++) {
                //  send "frame0=" + ix.ToString(); response should be OK (whatever that is)
                //  send "frame0?"
                //  response is "frame0=ix // name of channel/variable"
                //  extract // name and place in ChannelNamesPossible
                // }
                channelNamesBase = new string[256];
                for (var /*index*/ ix = 0; ix < channelNamesBase.Length; ix++) {
                    if (ix < ChannelNamesDefValue.Length) {
                        channelNamesBase[ix] = ChannelNamesDefValue[ix];
                    } else { channelNamesBase[ix] = string.Empty; }
                }
            }
        }

        private string[] channelNamesBase = null;
        private static string[] ChannelNamesDefValue = {
             "DC Sensor X"
            ,"DC Sensor Y"
            ,"DC Sensor Z"
            ,"AC Sensor X"
            ,"AC Sensor Y"
            ,"AC Sensor Z"
            ,"Control Signal X"
            ,"Control Signal Y"
            ,"Control Signal Z"
            ,"RMS Input X"
            ,"RMS Input Y"
            ,"RMS Input Z"
            ,"RMS Output X"
            ,"RMS Output Y"
            ,"RMS Output Z"
            ,"Excitation"
        };


            private Channel[] channelBase = new Channel[] {/*empty; set in _ctor*/};


            /// <summary>Access the channel's <see cref="ChannelModes"/></summary>
            /// <value><see cref="ChannelModes"/></value>
            /// <summary>
            /// Get, by <see cref="ScopeChannels"/>, the <see cref="ChannelCollection"/></summary>
            /// <param name='channel'>
            /// Channel specifies the desired <see cref="Channel"/>.</param>
            /// <example><code>
            /// Program.ConnectedController.Channels[ScopeChannels.Channel0].ChVariableIndx = (ChannelModes)0;</code></example>
            public const string ChCommand = "chan"; // not used yet
            public int ChVariableIndx {
                get {
                    var /*command*/ cmd = String.Format("chan{0}?", channelIndexBase);//-!- NEED HEXADECIMAL
                    var /*response*/ rsp = string.Empty;
                    Program.ConnectedController.SendInternal(cmd, CommandTypes.ResponseExpected, out rsp, CommandAckTypes.AckExpected);

                    // The return format looks like "chan0=0 X Sensor DC"
                    // Use the letter after the '=' as the channel mode hex value.
                    var /*index of delimiter*/ idx = rsp.IndexOf('=');
                    rsp = rsp.Substring(idx + 1, 1);    //character after delimiter
                    var /*value*/ vlu = Int32.Parse(rsp, System.Globalization.NumberStyles.Integer);
                    return (int)vlu;
                }
                set {
                    if (/*NOT reading settings ?*/ Program.IsReadingControllerSettings == false) {
                        Tools.IsOK(value, 0, 16, /*exception if bad*/ true);

                        // NOTE: Channel modes are set via hex values.
                        // Mono doesn't appear to support formatting like "{0:x1}" so we need to
                        // extract the last character from the string like "0000000A" as the value.
                        System.Diagnostics.Debug.Assert(((int)value) <= 0xF);
                        var /*command*/ cmd = /*value as hex digit(s)*/ ((int)value).ToString(/*must be majiscule; B is OK, b is not*/ "X");
                        cmd = /*last digit*/ cmd.Substring(cmd.Length - 1, 1);
                        cmd = String.Format("chan{0}={1}", channelIndexBase, cmd);  //-!- NEED HEXADECIMAL make command
                        if (Program.ConnectedController != null)
                            Program.ConnectedController.Send(cmd, CommandAckTypes.AckExpected);   //set commands have no response
                    }
                }
            }
        }


    /// <summary>Mode that can be assigned to a channel</summary>
    public enum ChannelModes { //-!- REPLACE WITH INDEX
        /// <summary>X Sensor DC</summary>
        XSensorDC = 0,
        /// <summary>Y Sensor DC</summary>
        YSensorDC,
        /// <summary>Z Sensor DC</summary>
        ZSensorDC,
        /// <summary>X Sensor AC</summary>
        XSensorAC,
        /// <summary>Y Sensor AC</summary>
        YSensorAC,
        /// <summary>Z Sensor AC</summary>
        ZSensorAC,
        /// <summary>X Control Signal</summary>
        XCtrlSignal,
        /// <summary>Y Control Signal</summary>
        YCtrlSignal,
        /// <summary>Z Control Signal</summary>
        ZCtrlSignal,
        /// <summary>X Sensor Root-Mean-Square</summary>
        XSensorRMS,
        /// <summary>Y Sensor Root-Mean-Square</summary>
        YSensorRMS = 0xA,
        /// <summary>Z Sensor Root-Mean-Square</summary>
        ZSensorRMS,
        /// <summary>X Output Root-Mean-Square</summary>
        XOutputRMS,
        /// <summary>Y Output Root-Mean-Square</summary>
        YOutputRMS,
        /// <summary>Z Output Root-Mean-Square</summary>
        ZOutputRMS,
        /// <summary>ExcitationIsEnabled Mode; see "freq", "ampl", "hdeg", and "vdeg" commands</summary>
        Excitation
    }

    /// <summary>Available 'scope Channels</summary>
    public enum ScopeChannels {
        /// <summary>Channel 1 ([0])</summary>
        Channel0 = 0,
        /// <summary>Channel 2 ([1])</summary>
        Channel1,
        /// <summary>Channel 3 ([2])</summary>
        Channel2,
        /// <summary>Channel 4 ([3])</summary>
        Channel3,
        /// <summary>Channel 5 ([4])</summary>
        Channel4,
        /// <summary>Channel 6 ([5])</summary>
        Channel5,
        /// <summary>Channel 7 ([6])</summary>
        Channel6,
        /// <summary>Channel 8 ([7])</summary>
        Channel7,
        /// <summary>Channel 9 ([8])</summary>
        Channel8,
        /// <summary>Channel 10 ([9])</summary>
        Channel9,
        /// <summary>Channel 11 ([10])</summary>
        Channel10,
        /// <summary>Channel 12 ([11])</summary>
        Channel11,
        /// <summary>Channel 13 ([12])</summary>
        Channel12,
        /// <summary>Channel 14 ([13])</summary>
        Channel13,
        /// <summary>Channel 15 ([14])</summary>
        Channel14,
        /// <summary>Channel 16 ([15])</summary>
        Channel15
    }
}