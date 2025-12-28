using System;

namespace TMCAnalyzer {
    /// <summary>Generates a stream of scope data</summary>
    public class ScopeDataGenerator {
        private void FillBufferWithSamples(byte[] samples, int count) {
            //FIXME: modify values as we go
            var /*X value*/ x = 0x0;
            var /*Y value*/ y = 0xFF;
            var /*Z value*/ z = 0x0;
            var /*output position*/ pos = 0;
            while (count > 0) {
                // Write 0xXX 0xX1 0xYY 0xY2 0xZZ 0xZ3
                var /*data*/ dta = DataSample.BytesFromChannelValues(x, y, z);
                Array.Copy(dta, 0, samples, pos, dta.Length);
                pos += dta.Length;
                count--;
            }
        }

        /// <summary>Provisions buffer with simulated scope data.</summary>
        /// <param name="buffer"/>
        /// <param name="count"/>
        public void GenerateSimulatedData(byte[] buffer, int count) {
            int /*output buffer position*/ pos = 0;
            while (count > 0) {
                int /*write count*/ cnt = Math.Min(count, scopeData.Length - scopeDatumIndex);
                Array.Copy(scopeData, scopeDatumIndex, buffer, pos, cnt);
                pos += cnt;
                count -= cnt;
                scopeDatumIndex += cnt;
                if (/*ran out of data?*/ scopeDatumIndex == scopeData.Length) {
                    // Start again at the buffer's beginning.
                    scopeDatumIndex = 0;
                }
            }
        }

        private byte[] scopeData = new byte[] {/*empty; set in _ctor*/};

        private int scopeDatumIndex = 0;

        /// <summary>Size (in samples) of the 'scope buffer.</summary>
        /// <remarks>
        /// actual size is ScopeSampleSize * <see cref="DataSample.BytesPerSample"/></remarks>
        public const int ScopeSampleSize = 2048;

        /// <summary>Creates and prepares a new instance.</summary>
        public ScopeDataGenerator() {
            scopeData = new byte[ScopeSampleSize * DataSample.BytesPerSample];
            FillBufferWithSamples(scopeData, ScopeSampleSize);
        }
    }
}