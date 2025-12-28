using System.ComponentModel;

namespace TMCAnalyzer {
	/// <summary>
	/// Sine Wave Properties holds values that control how a mathematical function is used to
	/// create a sine-wave. This sine-wave is the input for both the SCOPE and for the FFT plots.</summary>
	/// <remarks>
	/// In the case of the SCOPE, the procedure GetDataSamples in the DataSample class generates
	/// "fake" data samples to take the place of channel #1.</remarks>
	/// <remarks>
	/// FFT has its own (separate from SCOPE) algorithum to create a sine wave to feed into the
	/// FFT process. We do this so that the data fed into FFT are doubles (more precision) and not
	/// integers.</remarks>
	/// <remarks>
	/// METHODS THAT ACCESS THIS CLASS:
	/// frmScope                            -- To set the values
	/// DataSample > GetDataSample          -- To Read the values
	/// FFT_DataProcessor > ProcessDataSamples  -- To read the values
	/// </remarks>
	public static class SineWaveProperties {
		[DefaultValue(0.0)]
		public static double SineWaveAmplitude { get; set; }

		[DefaultValue(0.0)]
		public static double SineWaveFrequency { get; set; }

		[DefaultValue(0.0)]
		public static double SineWaveOffset { get; set; }

		[DefaultValue(0.0)]
		public static double SineWavePhase { get; set; }

		[DefaultValue(false)]
		public static bool SineWaveUseMathFunction { get; set; }
	}
}