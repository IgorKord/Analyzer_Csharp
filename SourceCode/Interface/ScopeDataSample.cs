using System;

namespace TMCAnalyzer {
    /// <summary>
    /// Data sample, a series of channel values</summary>
    public class DataSample {
        /// <summary>Constant bytes per channel.</summary>
        public const int BytesPerChannel = 2;

        /// <summary>Constant bytes per sample.</summary>
        public const int BytesPerSample = BytesPerChannel * ChannelsPerSample;

        /// <summary>Constant channelsBase per sample.</summary>
        public const int ChannelsPerSample = Program.MaxScopeChannels;

        /// <summary>Channel data values</summary>
        public int[] Channels;

        /// <summary>
        /// Channels the value from array.
        ///
        /// Channels are encoded like:
        /// 0xXX 0xXn where the upper 12 bits are the channel value
        /// and the lower 4 bits are the channel index</summary>
        /// <param name="ChannelIndex"/>
        /// <param name="ChannelValue"/>
        /// <param name="inputData"/>
        /// <param name="offset"/>
        public static void ChannelValueFromArray(byte[] inputData, int offset, out int ChannelValue, out int ChannelIndex) {
            // Scope format: 0xXX,0xX1,0xYY,0xY2,0xZZ,0xZ3
            // 0xXX,0xX1,0xYY,0xY2,0xZZ,0xZ3
            // 0xXX,0xX1,0xYY,0xY2,0xZZ,0xZ3
            //
            // Each data-read from the scope returnes 3 channelsBase (X, Y, and Z) with two 8-bit bytes per channel (16 bits).
            // Of the 16 bits of each channel the first 12 are the channel data and the next 4 are the channel number.
            // It is easiest to think of those 16 bits as 4 hexadecimal values.
            //
            // Example:
            // Binary data: 0110 0111 0011 0010
            // Hex Repredentation: 6 7 3 1
            // So the Data is: Hex 673 (Decimal 1651)
            // Channel is: Channel 1
            //---
            // So we start with two 8-bit bytes, represented as four 4-bit HEX digits. Hex: 6731
            // We shift the first byte (HEX 67) left by 4-bits, giving us: Hex: 670
            // We shift the second byts (HEX 31) right by 4-bits, giving us: Hex: 3
            // Adding those together we get the channel's data-value: Hex: 673
            ChannelValue = (inputData[offset] << 4) + (inputData[offset + 1] >> 4);

            // This gets the last 4-bits, which is the channel number.
            ChannelIndex = GetChannelIndexFromByte(inputData[offset + 1]);
        }

        /// <summary>Gets the channel index from byte.</summary>
        /// <returns>The channel index from byte.</returns>
        /// <param name='dataByte'/>
        public static int GetChannelIndexFromByte(byte dataByte) {
            // The input is one byte (8-bits). Lets say it's HEX 31. In binary that is: 0011 0010
            // The last four bit are the channel, so that's all we want. & 0000 1111
            // We do a bit-wise AND with the HEX F = 1111 = 0000 1111 0000 0010
            // This has given us the last four bits.
            return dataByte & 0xF;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSample"/> class.</summary>
        public DataSample() {
            Channels = new int[ChannelsPerSample];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSample"/> class.
        /// From scope data sampled values</summary>
        /// <param name="channel1"/>
        /// <param name="channel2"/>
        /// <param name="channel3"/>
        public DataSample(int channel1, int channel2, int channel3) {
            Channels = new int[ChannelsPerSample];
            Channels[0] = channel1;
            Channels[1] = channel2;
            Channels[2] = channel3;
        }

        /// <summary>Bytes from channel values.</summary>
        /// <returns>The from channel values.</returns>
        /// <param name="channel1Value"/>
        /// <param name="channel2Value"/>
        /// <param name="channel3Value"/>
        public static byte[] BytesFromChannelValues(int channel1Value, int channel2Value, int channel3Value) {
            int channelValue;
            var output = new byte[BytesPerSample];
            int outputPos = 0;
            channelValue = 1;
            output[outputPos++] = (byte)((channel1Value >> 4) & 0xFF);
            output[outputPos++] = (byte)(((channel1Value & 0xF) << 4) | channelValue);
            channelValue = 2;
            output[outputPos++] = (byte)((channel2Value >> 4) & 0xFF);
            output[outputPos++] = (byte)(((channel2Value & 0xF) << 4) | channelValue);
            channelValue = 3;
            output[outputPos++] = (byte)((channel3Value >> 4) & 0xFF);
            output[outputPos++] = (byte)(((channel3Value & 0xF) << 4) | channelValue);
            return output;
        }

        // Variables & Constants for producing mathematical sine-wave data for testing
        private static double radians = 0;

        private static int SineWaveValue() {
            const double TwoPI = Math.PI * 2;
            double SampleFrequency = Program.ScopeFrequency;
            double Freq = SineWaveProperties.SineWaveFrequency;
            double Amplitude = SineWaveProperties.SineWaveAmplitude;
            double Offset = SineWaveProperties.SineWaveOffset;
            double StepsPeriod = SampleFrequency / Freq;
            double Phase = SineWaveProperties.SineWavePhase;
            // Debug.WriteLine("Sine Wave Frequency: {0}", Freq);
            double rdstep = (TwoPI / StepsPeriod);
            radians = radians + rdstep;
            if (radians > TwoPI) radians = radians - TwoPI;
            int rv = (int)((Math.Sin(radians + (Phase * Math.PI / 180)) * (Amplitude / 2)) + Offset);

            // The following is if you wanted to return RANDOM data values instead of a sine-wave.
            //Random random = new Random();
            //int randomNumber = random.Next(0, 100);
            //int rv = randomNumber + 500;
            return rv;
        }

        /// <summary>Decode encoded scope data</summary>
        /// <param name="inputData"/>
        /// <param name="offset"/>
        public static DataSample GetDataSample(byte[] inputData, int offset) {
            var dataSample = new DataSample();
            int channelValue, channelIndex;
            for (int i = 0; i < ChannelsPerSample; i++) {
                ChannelValueFromArray(inputData, offset + (i * BytesPerChannel),
                out channelValue, out channelIndex);
                if (channelIndex != (i + 1)) return null;
                dataSample.Channels[i] = channelValue;
            }
            // GARY-DEBUG: GARY-FIX: Remove the following line when done debugging
            if (SineWaveProperties.SineWaveUseMathFunction == true) dataSample.Channels[0] = SineWaveValue();
            return dataSample;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="DataSample"/>.</summary>
        /// <param name='obj'>
        /// The <see cref="System.Object"/> to compare with the current <see cref="DataSample"/>.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="DataSample"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) { return false; }
            var otherSample = (DataSample)obj;
            if (Channels.Length != otherSample.Channels.Length) { return false; }
            for (int x = 0; x < Channels.Length; x++) {
                if (Channels[x] != otherSample.Channels[x]) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="DataSample"/> object.</summary>
        /// <returns>
        /// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode() {
            int hashCode = 0;
            foreach (var c in Channels) { hashCode += c.GetHashCode(); }
            return hashCode;
        }

        /// <summary>
        /// Returns a <code>string</code> that represents the current <see cref="DataSample"/>.</summary>
        /// <returns>
        /// A <code>string</code> that represents the current <see cref="DataSample"/>.</returns>
        public override string ToString() {
            string output = string.Empty;
            for (int channel = 0; channel < Channels.Length; channel++) {
                // separate values
                if (channel != 0) { output += ", "; }
                output += string.Format("Channel[{0}] = {1} (0x{1:x})",
                channel, Channels[channel]);
            }
            return string.Format("[DataSample {0}]", output);
        }
    }
}