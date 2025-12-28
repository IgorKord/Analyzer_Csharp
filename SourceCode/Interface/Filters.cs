
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMCAnalyzer {
	public enum filt_types {
		GAIN_ONLY = 0,
		LPF_1stOr = 1,          // STACIS (normalized)
		HPF_1stOr = 2,          // STACIS (normalized)
		LEADLAG_1stOr = 3,      // STACIS !NOT normalized! LEAD or LAG DOES depend on F1,F2: if F1>F2 - it is "-\_LAG, HiFreq, LoFreq"; if F1<F2 - it is "_/-LEAD, LoFreq, HiFreq";
		LEADLAG_2ndOr_wPk = 4,  // STACIS !NOT normalized! LEAD or LAG DOES depend on F1,F2: if F1>F2 - it is "-\_LAG, HiFreq, LoFreq"; if F1<F2 - it is "_/-LEAD, LoFreq, HiFreq";

		//ADDITIONAL filters for ELDAMP
		LEAD_1stOrNorm = 5,     // __/-- (normalized) does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		LAG_1stOr = 6,          //--\__ NOT normalized! potentially high gain at low freq! does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		LPF_2ndOr_wPk = 7,      //(normalized)
		HPF_2ndOr_wPk = 8,      //(normalized)
		PID = 9,                //(Leaky)
		PEAK_on_FLAT = 10,      // ____/\____ peak regulates by Q1, smooth phase transition
		NOTCH = 11,         // ----\/---- depth regulates by Q1, smooth phase transition
							//      second column in the menu
		LEAD_2ndOr_wPk,         // __/-- (normalized) does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		LAG_2ndOr_wPk,          // --\__ (normalized) does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		NOTCH_Sharp,            // ----\/---- Very deep (~ -40 dB), width regulates by Q1, sharp phase transition
		BPF,                    //"roof shape BPF" +20dB/dec-peak-20dB/dec; F1=peak freq, Q1= peak magnitude
		VELOCITY,               // approximate system velocity transfer function
		Position,               // approximate system position transfer function
		COMBINED_LAG1_LEAD1,    //, // combines LAG and LEAD 1st order into single 2nd order --\____/--;  --\__ determined by F1,Q1 (float frequency); __/-- determined by F2,Q2
		DIRECT_COEFF,           //,       // directly set a, b factors:

		// NOT IN USE does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		//    LEAD_1stOrSemiNorm '__/-- (SemiNormalized) potentially high gain at high freq! Gain=1.0 @ (F1+F2)/2.
		//    LAG_1stOrNorm  ' --\__ (normalized) does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		//    LAG_1stOrSemiNorm  '--\__ Semi Normalized! potentially high gain at low freq! Gain=1.0 @ (F1+F2)/2. Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
		//    GEO_OPT        '(geo optimization) gain = 1 when f > F2, INTENDED USE: F1=0.2, F2=0.5; it is LEDLAG --2nd order with Q1=Q2~0.1
		MaxFilterTyp
	}
	class IIRfilter {
		private static double PI = Program.PI;

		//define indexes for float_iir.par[index]
		////define indexes for float_iir.par[index]
		//    Const freq_1 As Long = 1
		//    Const freq_2 As Long = 2
		//    Const Q_f_1  As Long = 3
		//    Const Q_f_2  As Long = 4
		//    Const FilterGain   As Long = 0
		public const int FilterGain = 0;
		public const int freq_1 = 1;
		public const int freq_2 = 2;
		public const int Q_f_1 = 3;
		public const int Q_f_2 = 4;
		const int PID_P_gain = FilterGain;
		const int PID_I_gain = freq_1;
		const int PID_D_gain = freq_2;

		public static double LOOP_FREQUENCY;
		//Public Const ONE_OVER_FREQUENCY As Single = 1 / LOOP_FREQUENCY
		public static double PI_x2_OVER_FREQUENCY;

		public struct iir_history {
			// order of coefficients: float a1,a2,b0,b1,b2;   //coefficients are rarely change
			// order of history data and operations should match order of coefficients
			// y[n] = + a1*y[n-1] + a2*y[n-2] + b0*x[n] + b1*x[n-1] + b2*x[n-2]
			//t_f = (g->a1 * h->F_out) + (g->a2 * h->F_out1) + (g->b0 * in_out) + (g->b1 * h->d_0) + (g->b2 * h->d_1);
			public double d_1, d_0;			// only accessible by one thread, type "float"
			public double F_out1;			//mdr 042116 F_out -> y[n], F_out1 -> y[n-1]
			public double F_out;			//accessible by MORE than one thread, type "volatile float"
		};
		public const int USER_PARAM_NUMBER = 5;
		public struct float_iir {
			public long ftyp;
			public double[] par;
			public double b2, b1, b0, a2, a1;   //coefficients are rarely change
			public double d_1, d_0;			// only accessible by one thread, type "float"
			public double F_out1;			//mdr 042116 F_out -> y[n], F_out1 -> y[n-1]
			public double F_out;			//accessible by MORE than one thread, type "volatile float"
//			public iir_history HistBuff;	//POINTER to 3 words for calculation should be in the fastest RAM, this is pointer to structure
			public void Initialize() {
				par = new double[USER_PARAM_NUMBER];
			}
		};

		public struct tComplex {
			public double Re;
			public double Im;
		}

		public struct tF_coeff {
			public double a1;
			public double a2;
			public double b0;
			public double b1;
			public double b2;
		}

		public struct tMag_Phase {
			public double Mag;
			public double Phase;
		}

		public const int TEST_LTF_ARRAY_LENGTH = 200;
		//*********************************************************
		public static double[] Freq_points = new double[TEST_LTF_ARRAY_LENGTH + 1];
		public static double[] OneFilter_Mag = new double[TEST_LTF_ARRAY_LENGTH + 1];// data arrays
		public static double[] OneFilter_Phase = new double[TEST_LTF_ARRAY_LENGTH + 1];// data arrays


		private double Prev_Phase;		// from the privious line to detect jump

		/// <summary>Initializes a new instance of the <see cref="IIRfilters"/> class</summary>
		/// <param name='fftPower'/>
		public IIRfilter() {
			LOOP_FREQUENCY = Program.ScopeFrequency;
			PI_x2_OVER_FREQUENCY = Program.PI * 2 / LOOP_FREQUENCY;
			SetDefaultFiltParams(); // NO_FILTER, gain = 1
			fill_Freq_array();  // to plot filter TF on FFT plot
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="IIRfilters"/> is reclaimed by garbage collection.</summary>
		// In C#, the Tilde "~" designates this method as the DESTRUCTOR for this class/object.
		~IIRfilter() {
			//  Stop();
		}
		//filt type, 5 params, 5 coefficients
		const int FILTER_PARAMS_TOTAL = 11;
		//#define PID_P_gain FilterGain
		//#define PID_I_gain freq_1
		//#define PID_D_gain freq_2
		const int MAX_FILTERS_IN_AXIS = 6;
		// or it will assume all zeros and Filter_FT will be -100 dB and Axis_TF will be -500 dB
		// (0-5, 0-9) red from controller filters
		static double[,] FilterParamArray = new double[MAX_FILTERS_IN_AXIS, FILTER_PARAMS_TOTAL];// FilterParamArray[6,11]

		public static float_iir[,] filters = new float_iir[Program.MaxScopeChannels, MAX_FILTERS_IN_AXIS];

		/// <summary>
		/// Fiter itself, 2 nd order IIR
		/// <see cref="IIRfilters"/>
		/// </summary>

		/************************************************** /
		public double doiir2(double in_out, float_iir g, ref iir_history h) // a1 and a2 coefficients are reversed (i.e. a1 = -a1;) compared to classic form to use only addition command
		{
			double t_f;
			iir_history hh = h;//new g.HistBuff; //pointer to history
			//formException my_window = new formException();
			// y[n] = + a1*y[n-1] + a2*y[n-2] + b0*x[n] + b1*x[n-1] + b2*x[n-2]
			// in_out - x[n], h->d_0 -x[n-1], h->d_1 - x[n-2], h->F_out - y[n-1], h->F_out1 - y[n-2]
			t_f = (g.b0 * in_out) + (g.b1 * h.d_0) + (g.b2 * h.d_1) + g.a1 * h.F_out + g.a2 * h.F_out1;
			h.d_1 = h.d_0;
			h.d_0 = in_out;
			h.F_out1 = h.F_out;
			h.F_out = t_f;
			in_out = t_f;
			return in_out;
		}

		/**************************************************/
		public double doiir2(double in_out, ref float_iir g) // a1 and a2 coefficients are reversed (i.e. a1 = -a1;) compared to classic form to use only addition command
		{
			double t_f;
			//iir_history hh = h;//new g.HistBuff; //pointer to history
			//formException my_window = new formException();
			// y[n] = + a1*y[n-1] + a2*y[n-2] + b0*x[n] + b1*x[n-1] + b2*x[n-2]
			// in_out - x[n], h->d_0 -x[n-1], h->d_1 - x[n-2], h->F_out - y[n-1], h->F_out1 - y[n-2]
			t_f = (g.b0 * in_out) + (g.b1 * g.d_0) + (g.b2 * g.d_1) + g.a1 * g.F_out + g.a2 * g.F_out1;
			g.d_1 = g.d_0;
			g.d_0 = in_out;
			g.F_out1 = g.F_out;
			g.F_out = t_f;
			in_out = t_f;
			return in_out;
		}

		public void SetDefaultFiltParams() {
			int f_num = 0;
			for (f_num = 0; f_num <= MAX_FILTERS_IN_AXIS - 1; f_num++) {
				FilterParamArray[f_num, 0] = 0;                //filter type = NO_FILTER
				FilterParamArray[f_num, 1] = 1;                //gain
				FilterParamArray[f_num, 2] = 1;                //F1
				FilterParamArray[f_num, 3] = 1;                //F2
				FilterParamArray[f_num, 4] = 0;                //Q1
				FilterParamArray[f_num, 5] = 0;                //Q2
				FilterParamArray[f_num, 6] = 0;                //a1
				FilterParamArray[f_num, 7] = 0;                //a2
				FilterParamArray[f_num, 8] = 1;                //b0
				FilterParamArray[f_num, 9] = 0;                //b1
				FilterParamArray[f_num, 10] = 0;               //b2
				set_filt_params_from_user_input(f_num);
			}
		}


		/*********************************** params in: Filter Type,   F1,     Q1,     F2,     Q2     gain **/
		private void set_filt_params(ref float_iir sfp, long filter_type, double par_freq_1, double par_Q_1, double par_freq_2, double par_Q_2, double filter_gain) {
			sfp.Initialize(); //par[5] need to be not null
			sfp.ftyp = (long)filter_type;//it determines type of filter == filter coefficients
			sfp.par[freq_1] = par_freq_1; // frequency 1,
			sfp.par[Q_f_1] = par_Q_1;    // Q-factor for Freq1
			sfp.par[freq_2] = par_freq_2; // frequency 2,
			sfp.par[Q_f_2] = par_Q_2;    // Q-factor for Freq2
			sfp.par[FilterGain] = filter_gain;

			//calculate filter coefficients
			set_float_iir(ref sfp);
			//reset history
			sfp.d_0 = 0;
			sfp.d_1 = 0;
			sfp.F_out = 0;
			sfp.F_out1 = 0;
		}

		/**********************************************************************************************************************/
		public void SET_FILTER(long Faxis, long Fnumber, filt_types Ftype, double Freq1, double Q_1, double Freq2, double Q_2, double Fgain) {
			//System.Diagnostics.Debug.Assert(Tools.IsOK(Fnumber, 0, MAX_FILTERS_IN_AXIS - 1));
			if ((Fnumber > MAX_FILTERS_IN_AXIS - 1) || (Fnumber < 0))
				return;
			if ((Faxis > Program.MaxScopeChannels - 1) || (Faxis < 0))
				return;
			if ((Ftype >= filt_types.MaxFilterTyp - 1) || ((int)Ftype < 0))
				return;

			set_filt_params(ref filters[Faxis, Fnumber], (long)Ftype, Freq1, Q_1, Freq2, Q_2, Fgain);// filters[6, 3]
		}


		/**********************************************************************************************************************/
		// test array to show TF
		public void fill_Freq_array() {
			int i = 0;
			double delta_freq = 0;
			float freq_values_per_decade = 0;
			Freq_points[0] = 0.1;
			freq_values_per_decade = TEST_LTF_ARRAY_LENGTH / 4;
			delta_freq = Math.Pow(10, (1 / freq_values_per_decade));
			for (i = 1; i <= TEST_LTF_ARRAY_LENGTH; i++) {
				Freq_points[i] = Freq_points[i - 1] * delta_freq;
			}
		}

		/**********************************************************************************************************************/
		//Function calculate_test_transfer_function(frequency As Single) As Double
		//    Dim displacement_x As Double
		//    Dim displacement_u As Double
		//    Dim resonance As Single
		//    Dim damping As Single
		//    resonance = 2.5
		//    damping = 0.25
		//    displacement_x = 1 + (2 * damping * frequency / resonance) ^ 2
		//       displacement_u = Abs((1 - (frequency ^ 2 / resonance ^ 2) ^ 2 + (2 * damping * frequency / resonance) ^ 2))
		//       calculate_test_transfer_function = 5 * Log((displacement_x / displacement_u) ^ 0.5)
		//
		//End Function
		//Function calculate_test_phase(frequency As Single) As Double
		//    calculate_test_phase = (-180 + 2 * frequency)
		//    calculate_test_phase = ((calculate_test_phase + 180) Mod 360) - 180
		//End Function

		/**********************************************************************************************************************/
		public void CalculateFilterTF(ref int Filt_num) {
			int i = 0;
			tMag_Phase Mag_Phase = new tMag_Phase { };
			//'temp structure
			//UPGRADE_WARNING: Arrays in structure ThisFilterCoeff may need to be initialized before they can be used. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="814DF224-76BD-4BB4-BFFB-EA359CB9FC48"'
			float_iir ThisFilterCoeff = default(float_iir);
			double freq_sum_for_test = 0;
			double prev_freq = 0;
			double t_float = 0;
			set_filt_params_from_user_input(Filt_num);
			//    ThisFilterCoeff.ftyp = FilterParamArray[Filt_Number, 0) 'ComboFilterTYPE.ListIndex
			//    ThisFilterCoeff.par(FilterGain) = FilterParamArray[Filt_Number, 1)
			//    ThisFilterCoeff.par(freq_1) = FilterParamArray[Filt_Number, 2)
			//    ThisFilterCoeff.par(freq_2) = FilterParamArray[Filt_Number, 3)
			//    ThisFilterCoeff.par(Q_f_1) = FilterParamArray[Filt_Number, 4)
			//    ThisFilterCoeff.par(Q_f_2) = FilterParamArray[Filt_Number, 5)
			//
			//    If ChkRealTimeUpdateTF.Value = Checked Then Call set_float_iir(ThisFilterCoeff)
			//
			ThisFilterCoeff.b2 = FilterParamArray[Filt_num, 6];
			ThisFilterCoeff.b1 = FilterParamArray[Filt_num, 7];
			ThisFilterCoeff.b0 = FilterParamArray[Filt_num, 8];
			ThisFilterCoeff.a2 = FilterParamArray[Filt_num, 9];
			ThisFilterCoeff.a1 = FilterParamArray[Filt_num, 10];

			//check if frequency array is filled, not zero
			freq_sum_for_test = 0;
			//0 to 200
			for (i = 0; i <= TEST_LTF_ARRAY_LENGTH; i++) {
				//prepare test array for axis OLTF calculation
				if (Freq_points[i] > 0) {
					prev_freq = Freq_points[i];
					//prepare test array for axis OLTF calculation
					Freq_points[i] = prev_freq;
					//Freq_points are empty, maybe Reference_Freq_data is filled?
				} else {
					//if (Reference_Freq_data[i] > 0) {
					//    prev_freq = Reference_Freq_data[i];
					//    // update array
					//    Freq_points[i] = prev_freq;
					//} else {
					//    //                Freq_points[i] = prev_freq ' fill up to the end with the last valid frequency
					//}
				}
				freq_sum_for_test = freq_sum_for_test + prev_freq;
			}
			i = i - 1;
			// array is not filled
			if (freq_sum_for_test == 0) {
				fill_Freq_array();
			}


			// starts from lower frequency, 0.1 Hz
			for (i = 0; i <= TEST_LTF_ARRAY_LENGTH; i++) {
				Mag_Phase = Mag_Ph_vs_Freq(ref ThisFilterCoeff, ref Freq_points[i], LOOP_FREQUENCY);
				OneFilter_Mag[i] = 20 * System.Math.Log(Mag_Phase.Mag) / System.Math.Log(10);
				t_float = Mag_Phase.Phase;
				// returns between -90 and +90

				//7 = LPF 2nd order, need to reverse phase because it calculates phase 180 at low freq
				if (ThisFilterCoeff.ftyp == (long)filt_types.LPF_2ndOr_wPk | ThisFilterCoeff.ftyp == (long)filt_types.Position) {
					t_float = t_float - 180;
				}

				if (i == 0)
					Prev_Phase = t_float;
				OneFilter_Phase[i] = Phase_Filter_Expand_plus_minus_180(ref t_float);
				//OneFilter_Phase(i) = (t_float) ' returns between -180 and +180
			}
			//if (((FilterNumberInChain == Filt_num) & (ChkShowOneFilterTF.CheckState == System.Windows.Forms.CheckState.Checked))) {
			//    Analyzer.CWGraph.Plots.Item(OneFilterTF_Num).PlotXvsY(Freq_points, OneFilter_Mag);
			//    Analyzer.CWGraphPhase.Plots.Item(OneFilterTF_Num).PlotXvsY(Freq_points, OneFilter_Phase);
			//}
		}

		public double Phase_Filter_Expand_plus_minus_180(ref double phase_in) {
			double phase_diff = 0;
			// cleaning phase, which might jump because ATAN returns between -90 and +90
			// example: prev phase is still +87, but this phase has jumped to -88
			// example: prev phase is still -87, but this phase has jumped to +88
			// check starts from lower frequency, 0.1 Hz

			// > 179.9999999
			if ((Prev_Phase >= 180)) {
				Prev_Phase = Prev_Phase - 180;
				//
			} else {
				//<= -179.9999999
				if ((Prev_Phase <= -180)) {
					Prev_Phase = Prev_Phase + 180;
					//
				}
			}

			phase_diff = phase_in - Prev_Phase;
			//ex: phase_diff=-136.0: prevPhase=+46.5,phase_in=-89.5
			//ex: phase_diff=+175.0: prevPhase = -87,phase_in = +88
			if ((phase_diff >= 179.9999999)) {
				phase_in = phase_in - 180;
				//
			} else {
				//ex: phase_diff=+175.0: prevPhase = -87,phase_in = +88
				if ((phase_diff >= 90)) {
					phase_in = phase_in - 180;
					//becomes +88-180=83
				} else {
					if ((phase_diff <= -179.9999999)) {
						phase_in = phase_in + 180;
						//
					} else {
						//ex: phase_diff=-136.0: prevPhase=+46.5,phase_in=-89.5
						if ((phase_diff < -90)) {
							phase_in = phase_in + 180;
							//becomes -89.5+180=+90.5
						} else {
							//phase_in = phase_in;
						}
					}
				}
			}
			Prev_Phase = phase_in;
			return phase_in;
		}

		public double Phase_Limit_TF_plus_minus_180(ref double phase_in) {
			// float phase_diff = 0;
			// cleaning phase, which might jump because sum of all filters can go beyond 360
			// example: prev phase is still +174, but this phase has jumped to -188
			if ((phase_in > 360)) {
				phase_in = phase_in - 360;
				//
			} else {
				if ((phase_in > 180)) {
					phase_in = phase_in - 360;
				} else {
					if ((phase_in < -360)) {
						phase_in = phase_in + 360;
						//
					} else {
						if ((phase_in < -180)) {
							phase_in = phase_in + 360;
						} else {
							phase_in = phase_in;
						}
					}
				}
			}
			//Prev_Phase = phase_in
			return phase_in;
		}

		/**********************************************************************************************************************/
		//FilterParams As float_iir
		public void set_filt_params_from_user_input(int Filt_Number) {
			var FilterParams = new float_iir { };// temporary filter
			int param = 0;
			float testVar = 0;
			FilterParams.Initialize(); // par[5]
			// copy to temp FilterParams one of the axis's 6 filters
			FilterParams.ftyp = (long)FilterParamArray[Filt_Number, 0];// FilterParamArray[6,11]
			FilterParams.par[FilterGain] = FilterParamArray[Filt_Number, 1];
			FilterParams.par[freq_1] = FilterParamArray[Filt_Number, 2];
			FilterParams.par[freq_2] = FilterParamArray[Filt_Number, 3];
			FilterParams.par[Q_f_1] = FilterParamArray[Filt_Number, 4];
			FilterParams.par[Q_f_2] = FilterParamArray[Filt_Number, 5];
			for (param = 0; param <= USER_PARAM_NUMBER - 1; param++) {
				if (FilterParams.par[param] != 0)
					testVar = testVar + 1;
			}
			if ((testVar == 0))//all params are zero, not initiated yet. it will cause 'dived by zero' exception
				return;

			set_float_iir(ref FilterParams);//this recalculates a, b coefficients

			// update coefficients so we can re-cal axis TF and re-plot
			FilterParamArray[Filt_Number, 6] = FilterParams.b2;
			FilterParamArray[Filt_Number, 7] = FilterParams.b1;
			FilterParamArray[Filt_Number, 8] = FilterParams.b0;
			FilterParamArray[Filt_Number, 9] = FilterParams.a2;
			FilterParamArray[Filt_Number, 10] = FilterParams.a1;
		}

		public static void FFT(ref double[] FR, ref double[] FI, ref int LnN) {

			//THE FAST FOURIER TRANSFORM
			//copyright © 1997-1999 by California Technical Publishing
			//published with  permission from Steven W Smith, www.dspguide.com
			//GUI by logix4u , www.logix4u.net
			//modified by logix4u, www.logix4.net
			//Upon entry, N% contains the number of points in the DFT, FR( ) and
			//FI( ) contain the real and imaginary parts of the input. Upon return,
			//FR( ) and FI( ) contain the DFT output. All signals run from 0 to N%-1.
			int ND2 = 0;
			int k = 0;
			int i = 0;
			int j = 0;
			int L = 0;
			int le = 0;
			int le2 = 0;
			int ip = 0;
			int NP_FFT = 0;

			double wr = 0;
			double ur1 = 0;
			double s = 0;
			double ur = 0;
			double ui = 0;
			double wi = 0;
			double ti = 0;
			double tr = 0;
			//double DivN = 0;

			NP_FFT = (int)Math.Pow(2, LnN);
			//Number of points for use in FFT

			ND2 = NP_FFT / 2;
			//Half points
			j = 1;

			for (i = 1; i <= NP_FFT - 1; i++) {

				if (i < j) {
					s = FR[i];
					FR[i] = FR[j];
					FR[j] = s;
					s = FI[i];
					FI[i] = FI[j];
					FI[j] = s;
				}

				k = ND2;

				while (k < j) {
					j = j - k;
					k = k / 2;
				}

				j = j + k;
			}

			//Loop for each stage
			for (L = 1; L <= LnN; L++) {

				le = (int)Math.Pow(2, L);
				le2 = le / 2;
				ur = 1;
				ui = 0;
				wr = System.Math.Cos(PI / le2);
				//Calculate sine & cosine values
				wi = -System.Math.Sin(PI / le2);
				//Loop for each sub DFT
				for (j = 1; j <= le2; j++) {
					i = j;
					while ((i <= NP_FFT)) {
						ip = i + le2;
						tr = FR[ip] * ur - FI[ip] * ui;
						//Butterfly calculation
						ti = FR[ip] * ui + FI[ip] * ur;
						FR[ip] = FR[i] - tr;
						FI[ip] = FI[i] - ti;
						FR[i] = FR[i] + tr;
						FI[i] = FI[i] + ti;
						i = i + le;
					}
					ur1 = ur * wr - ui * wi;
					ui = ur * wi + ui * wr;
					ur = ur1;
				}
			}
			//{l - l›kke}

		}

		public static double Avg(double[] Aray, int Size = -1) {

			double DataSum = 0;
			int counter = 0;
			if (Aray.GetUpperBound(0) < 1)
				// ERROR: Not supported in C#: ReDimStatement

				if (Size == -1)
					Size = Aray.GetUpperBound(0);
			//argument is missing
			DataSum = 0;
			for (counter = 0; counter <= Size - 1; counter++) {
				DataSum = DataSum + Aray[counter];
			}
			return DataSum / Size;
		}
		/* mdr 062218 */
		public static int FindMaxMin(ref double[] inArray, ref int StartIndex, ref double maxV, ref double minV, ref int MaxIndex, ref int MinIndex, bool IgnoreZeroes = false) {
			int functionReturnValue = 0;
			int i = 0;
			double max = 0;
			double min = 0;
			int ArrSize = 0;
			max = -1E+308;
			//set max and min possible values
			min = 1E+308;
			ArrSize = inArray.GetUpperBound(0);

			if (StartIndex < ArrSize) {
				for (i = StartIndex; i <= ArrSize - 1; i++) {
					if (i > ArrSize)
						break; // TODO: might not be correct. Was : Exit For
					if (max < inArray[i]) {
						max = inArray[i];
						MaxIndex = i;
					}
					if (IgnoreZeroes == true & inArray[i] == 0) {
						i = i + 1;
					} else {
						if (min > inArray[i]) {
							min = inArray[i];
							MinIndex = i;
						}
					}
				}
				minV = min;
				maxV = max;
				functionReturnValue = 1;
				//sucsess
			} else {
				functionReturnValue = 0;
			}
			return functionReturnValue;
		}

		public static double St_Dev(ref double[] Aray, int Size = -1, bool IgnoreZeroes = false) {
			double functionReturnValue = 0;

			double sum_of_squares = 0;
			double array_sum = 0;
			double diff = 0;
			int array_index = 0;
			int final_size = 0;
			if (Aray.GetUpperBound(0) < 1)
				// ERROR: Not supported in C#: ReDimStatement


				if (Size == -1)
					Size = Aray.GetUpperBound(0);
			//argument is missing ****
			final_size = Size;
			sum_of_squares = 0;
			array_sum = 0;
			for (array_index = 0; array_index <= Size - 1; array_index++) {
				array_sum = array_sum + Aray[array_index];
				sum_of_squares = sum_of_squares + (Math.Pow(Aray[array_index], 2));
				if (IgnoreZeroes == true & Aray[array_index] == 0) {
					final_size = final_size - 1;
				}
			}
			if (final_size < 2)
				final_size = 2;
			//prevent calc errors
			diff = (final_size * sum_of_squares - Math.Pow(array_sum, 2));
			//because of limited accuracy, sometimes diff can be negative
			if (diff > 0) {
				functionReturnValue = Math.Pow((diff / (final_size * (final_size - 1))), 0.5);
			} else {
				functionReturnValue = 0;
			}
			return functionReturnValue;
		}

		public static string BIN(ref int inbyte) {
			// subprogram translates byte to BIN$ format, 127 = 01111111
			string m = null;
			string b = null;
			int k = 0;
			int j = 0;
			int bt = 0;

			bt = inbyte;
			m = "";
			for (j = 0; j <= 7; j++) {
				if (bt < 2)
					goto end_conv;
				b = (bt - 2 * (bt / 2)).ToString();
				bt = (bt / 2);
				m = b + m;
			}
		end_conv:
			m = bt.ToString() + m;
			for (k = j; k <= 6; k++) {
				m = " 0" + m;
			}
			return m;
		}
	/***********************************************************************/

		public static double Make180phaseShift(ref double phaseIN) {
			double phaseOut = 0;
			phaseOut = phaseIN + 180;
			if (phaseOut > 180) {
				phaseOut = phaseOut - 360;
				//<=0
			} else {
				//        phaseOut = phaseIN - 180
				//        If phaseOut < -180 Then phaseOut = phaseOut + 180
			}
			return phaseOut;
		}

		//-----------------------------------------------
		// Complex Number Math Functions
		//-----------------------------------------------

		public static tComplex Complex(double dReal, double dImag) {
			tComplex functionReturnValue = default(tComplex);
			//Set the real and imaginary numbers in a complex number type
			functionReturnValue.Re = dReal;
			functionReturnValue.Im = dImag;
			return functionReturnValue;
		}

		public static tComplex IM_NEG(ref tComplex arg1) {
			//Compute unary - operation on a single complex number,
			//returns a complex number
			return Complex(-(arg1.Re), -(arg1.Im));
		}

		public static tComplex IM_ADD(ref tComplex arg1, ref tComplex arg2) {
			//Addition of two complex numbers, returns a complex number
			return Complex(arg1.Re + arg2.Re, arg1.Im + arg2.Im);
		}

		public static tComplex IM_SUB(ref tComplex arg1, ref tComplex arg2) {
			//Subtraction of two complex numbers, returns a complex number
			return Complex(arg1.Re - arg2.Re, arg1.Im - arg2.Im);
		}

		public static tComplex IM_MULT(ref tComplex arg1, ref tComplex arg2) {
			//Multiplication of two complex numbers, returns a complex number
			return Complex((arg1.Re * arg2.Re) - (arg1.Im * arg2.Im), (arg1.Re * arg2.Im) + (arg2.Re * arg1.Im));
		}

		public static tComplex IM_DIV(ref tComplex arg1, ref tComplex arg2) {
			//division of two complex numbers:
			// multiply the numerator and denominator by the complex conjugate of the denominator,
			//for example, with z_1=a+bi and z_2=c+di, z=z_1/z_2 is given by
			//numerator and denomerator  two complex numbers, returns a complex number
			tComplex Denom_Conj = default(tComplex);
			double Denom_Mag = 0;
			//UPGRADE_WARNING: Couldn't resolve default property of object Denom_Conj. Click for more: 'ms-help://MS.VSCC.v90/dv_commoner/local/redirect.htm?keyword="6A50421D-15FE-4896-8A1B-2EC21E9037B2"'
			Denom_Conj = Complex(arg2.Re, -arg2.Im);
			Denom_Mag = arg2.Re * arg2.Re + arg2.Im * arg2.Im;
			return Complex(((arg1.Re * arg2.Re) + (arg1.Im * arg2.Im)) / Denom_Mag, ((arg1.Im * arg2.Re) - (arg1.Re * arg2.Im)) / Denom_Mag);
		}

		//===============================================================================================================================
		// this function calculates magnitude-phase prediction based on filter coefficients
		//===============================================================================================================================
		public static tMag_Phase Mag_Ph_vs_Freq(ref float_iir Coeff, ref double freq, double LoopFreq) {
			tMag_Phase functionReturnValue = default(tMag_Phase);
			// ERROR: Not supported in C#: OnErrorStatement

			// if frequency = 0 it gives division by zero
			double Omega = 0;
			tComplex H = default(tComplex);
			tComplex Num = default(tComplex);
			tComplex Den = default(tComplex);

			Omega = freq * 2 * PI / LoopFreq;
			Num = Numerator_Complex(ref Coeff, ref Omega);
			Den = Denominator_Complex(ref Coeff, ref Omega);
			H = IM_DIV(ref Num, ref Den);
			functionReturnValue.Mag = System.Math.Sqrt(H.Re * H.Re + H.Im * H.Im);
			if ((functionReturnValue.Mag == 0))
				functionReturnValue.Mag = 1E-05;
			//we take a Log and it cannot be a zero' 1e-5 means it is an error
			if ((H.Re != 0)) {
				functionReturnValue.Phase = -System.Math.Atan(H.Im / H.Re) * 180 / PI;
				// returns between -90 and +90
			} else {
				functionReturnValue.Phase = -360;
				//this means it is an error
			}
			return functionReturnValue;
		}

		public static tComplex Numerator_Complex(ref float_iir Coeff, ref double Omega) {
			double NumRe = 0;
			double NumIm = 0;
			NumRe = Coeff.b0 + Coeff.b1 * System.Math.Cos(Omega) + Coeff.b2 * System.Math.Cos(2 * Omega);
			NumIm = Coeff.b1 * System.Math.Sin(Omega) + Coeff.b2 * System.Math.Sin(2 * Omega);
			return Complex(NumRe, NumIm);
		}

		public static tComplex Denominator_Complex(ref float_iir Coeff, ref double Omega) {
			double DenRe = 0;
			double DenIm = 0;
			DenRe = 1 - Coeff.a1 * System.Math.Cos(Omega) - Coeff.a2 * System.Math.Cos(2 * Omega);
			DenIm = -Coeff.a1 * System.Math.Sin(Omega) - Coeff.a2 * System.Math.Sin(2 * Omega);
			return Complex(DenRe, DenIm);
		}
		//'''''''''''' End of complex math '''''''''''''''''''''''''''''''''''''''''''''''

		//'================FILTER SETTING ++++++++++++++++++++++++++

		public static void set_float_iir(ref float_iir fp) {
			// ERROR: Not supported in C#: OnErrorStatement

			// CAUTION !! it can miss divide by zero

			//' RUNS FROM FLASH ONLY AT STARTUP or FITER CHANGE, DOES NOT NEED ramfuncs
			//#define omega_1pole omega_1pole 'alias name: omega_Numenator
			//#define omega_2zero omega_2zero 'alias name: omega_DEnominator
			//Dim fp As float_iir
			double h1 = 0;
			double h2 = 0;
			//Double h1, h2'
			double t_gain = 0;
			double s_freq_gain_corr = 0;
			//Double t_gain, s_freq_gain_corr, cotan_of_half_omega_1'
			double cotan_of_half_omega_1 = 0;
			double freq_1_correction = 0;
			double freq_2_correction = 0;
			double omega_1pole = 0;
			double omega_2zero = 0;
			double testVar = 0;
			int filt_par_Q_f_1 = Q_f_1;//index 3,  par[3]
			int filt_par_Q_f_2 = Q_f_2;//index 4,  par[4]
			int filt_par_freq_1 = freq_1;//index 1, par[1]
			int filt_par_freq_2 = freq_2;//index 2, par[2]
			int filt_par_FilterGain = FilterGain;//index 0, par[0]

			PI_x2_OVER_FREQUENCY = PI * 2 / LOOP_FREQUENCY;

			if (fp.par[filt_par_Q_f_1] == 0)
				fp.par[filt_par_Q_f_1] = 0.5f;
			// prevent division by zero
			if (fp.par[filt_par_Q_f_2] == 0)
				fp.par[filt_par_Q_f_2] = 0.5f;
			// prevent division by zero

			testVar = (fp.par[filt_par_Q_f_1]) * (fp.par[filt_par_Q_f_1]); //Q1*Q1
			//it is checked above! If testVar <> 0 Then ' denominator is not zero
			freq_1_correction = 1 / System.Math.Sqrt(1 + 0.25 / ((fp.par[filt_par_Q_f_1]) * (fp.par[filt_par_Q_f_1])));
			// 'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
			//Else
			//    freq_1_correction = 1 'when denominator is zero
			//End If
			testVar = (fp.par[filt_par_Q_f_2]) * (fp.par[filt_par_Q_f_1]);//Q2*Q1
			//it is checked above! If testVar <> 0 Then ' denominator is not zero
			freq_2_correction = 1 / System.Math.Sqrt(1 + 0.25 / (testVar));
			//
			//Else
			//    freq_2_correction = 1 'when denominator is zero
			//End If

			omega_1pole = PI_x2_OVER_FREQUENCY * fp.par[filt_par_freq_1];
			//par[1) is filter corner frequency
			omega_2zero = PI_x2_OVER_FREQUENCY * fp.par[filt_par_freq_2];
			//

			//choose bigger of two frequencies for gain correction
			if ((omega_1pole > omega_2zero)) {
				s_freq_gain_corr = (LOOP_FREQUENCY + fp.par[filt_par_freq_1]) / LOOP_FREQUENCY;
			} else {
				s_freq_gain_corr = (LOOP_FREQUENCY + fp.par[filt_par_freq_2]) / LOOP_FREQUENCY;
			}
			h1 = System.Math.Tan(omega_1pole / 2);
			cotan_of_half_omega_1 = 1 / h1;

			t_gain = fp.par[FilterGain];
			// set default parameters as GAIN_ONLY for start (needed gains will be replaced)
			fp.b2 = 0.0f;
			fp.b1 = 0.0f;
			fp.b0 = t_gain;
			fp.a2 = 0.0f;
			fp.a1 = 0.0f;

			double a11 = 0;
			double omega_2zero_tmp = 0;
			double b11 = 0;
			double a12 = 0;
			double omega_1pole_tmp = 0;
			double b12 = 0;
			//;// temp a11, b11 for the LEG, b02 is gain ==1 by default ';// temp a11, b11 for the LEG, b01 is gain ==1 by default
			switch ((fp.ftyp)) {

				// ============== STACIS FILTERS: only 4 needed ============================================================================================
			case (int)filt_types.LPF_1stOr:
				//case 1: sharp drop at high freq, notch at Nyquist, phase = -90
				h1 = 1.0 / (1.0 + cotan_of_half_omega_1);
				fp.b1 = Convert.ToSingle(t_gain * h1);
				fp.b0 = fp.b1;
				fp.a1 = Convert.ToSingle((1.0 - cotan_of_half_omega_1) * h1);
			break;

			case (int)filt_types.HPF_1stOr:
				//case 2: normalized
				h1 = 1.0 / (1.0 + cotan_of_half_omega_1);
				fp.a1 = Convert.ToSingle((1.0 - cotan_of_half_omega_1) * h1);
				fp.b1 = Convert.ToSingle(-cotan_of_half_omega_1 * h1 * t_gain);
				fp.b0 = -fp.b1;
			break;

			case (int)filt_types.LEADLAG_1stOr:
				//case 3: normalized maximum unity gain at band pass frequency
				//  if (omega_1pole <= omega_2zero) t_gain = omega_1pole / omega_2zero * t_gain'
				fp.a1 = Convert.ToSingle(omega_2zero - 1);
				//  fp.a2 = 0'set to 0 above "switch(fp.ftyp){}"
				fp.b0 = (float)t_gain;
				//1# '
				fp.b1 = (float)((omega_1pole - 1) * t_gain);
				//  fp.b2 = 0'set to 0 above "switch(fp.ftyp){}"
			break;

			case (int)filt_types.LEADLAG_2ndOr_wPk:
				//case 4: general form with Q factors (NOT normalized) LEAD or LAG DOES depend on F1,F2: if F1>F2 - it is "-\_LAG, HiFreq, LoFreq"' if F1<F2 - it is "_/-LEAD, LoFreq, HiFreq"'
				// Max gain can be bigger than 1 if any of Q is bigger than 0.5, generally max gain approx = Q.
				// *****!!***** BE CAREFUL WITH Q!!  IT CAN OVERFLOW IN FIXED POINT CALCULATIONS
				//  Normalized to unity gain at frequencies much bigger/smaller than F1 or F2.
				//  omega_1pole *= freq_1_correction'  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				//  omega_2zero *= freq_2_correction'  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				h2 = 1 - (omega_2zero / (2 * fp.par[filt_par_Q_f_2]));
				fp.a1 = Convert.ToSingle(-2 * h2 * System.Math.Cos(omega_2zero));
				fp.a2 = Convert.ToSingle(h2 * h2);
				h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
				fp.b0 = Convert.ToSingle(t_gain);
				//1# '
				fp.b1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole) * t_gain);
				fp.b2 = Convert.ToSingle(h1 * h1 * t_gain);
			break;

			case (int)filt_types.DIRECT_COEFF:
				// directly enter 2nd order coeffs (b0+b1*z^-1+b2*z^-2)/(1+a1*z^-1+a2*z^-2)
				fp.b0 = fp.par[filt_par_FilterGain];
				fp.b1 = fp.par[filt_par_freq_1];
				fp.b2 = fp.par[filt_par_freq_2];
				fp.a1 = fp.par[filt_par_Q_f_1];
				fp.a2 = fp.par[filt_par_Q_f_2];
			break;

				// FilterGain  Q_f_1  Q_f_2 freq_1 freq_2
				// break '
				// ============== STACIS FILTERS END only 4 needed ============================================================================================

				// ============== ADDITIONAL FILTERS FOR ELECTRODUMP ==========================================================================================

			case (int)filt_types.LEAD_1stOrNorm:
				//, LEAD_1stOrSemiNorm     'case 5: __/-- F1<F2' HPF with max gain = param4 above F2, !! CAREFUL with FIX POINT !!gain at F1 can be much bigger than 1!!
				//  Case LEAD_1stOrSemiNorm:  'case 18: --\__ Semi Normalized! potentially high gain at high freq! Gain=1 @ (F1+F2)/2.
				//  Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
				h1 = t_gain;
				//1# '
				if ((omega_1pole >= omega_2zero)) {
					fp.a1 = Convert.ToSingle(omega_1pole - 1);
					fp.b1 = Convert.ToSingle(omega_2zero - 1);
					//        If (fp.ftyp = LEAD_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_1pole / omega_2zero) '
				} else {
					fp.a1 = Convert.ToSingle(omega_2zero - 1);
					fp.b1 = Convert.ToSingle(omega_1pole - 1);
					//        If (fp.ftyp = LEAD_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_2zero / omega_1pole) '
				}
				fp.b0 = Convert.ToSingle(h1);
				fp.b1 = Convert.ToSingle(fp.b1 * h1);
			break;

			case (int)filt_types.LAG_1stOr:
				//, LAG_1stOrSemiNorm          'case 6: --\__  NOT normalized gain at lowest frequency, at highest frequency gain = 1
				//  Case LAG_1stOrSemiNorm:   'case 20:--\__ Semi Normalized! potentially high gain at low freq! Gain=1 @ (F1+F2)/2.
				//  Does not depend on F1<F2 or F1>F2, the lowest of F1,F2 is low corner freq, the highest of F1,F2 - high corner freq
				h1 = t_gain;
				//1# '
				if ((omega_1pole <= omega_2zero)) {
					fp.a1 = Convert.ToSingle(omega_1pole - 1);
					fp.b1 = Convert.ToSingle(omega_2zero - 1);
					//        If (fp.ftyp = LAG_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_1pole / omega_2zero) '
				} else {
					fp.a1 = Convert.ToSingle(omega_2zero - 1);
					fp.b1 = Convert.ToSingle(omega_1pole - 1);
					//        If (fp.ftyp = LAG_1stOrSemiNorm) Then h1 = t_gain * Sqr(omega_2zero / omega_1pole) '
				}
				fp.b0 = Convert.ToSingle(h1);
				fp.b1 = Convert.ToSingle(fp.b1 * h1);
			break;

			case (int)filt_types.COMBINED_LAG1_LEAD1:
				//// combines LAG and LEAD 1st order into single 2nd order --\____/--;  --\__ determined by F1,Q1 (float frequency); __/-- determined by F2,Q2
				//               omega_1pole = PI_x2_OVER_FREQUENCY * fp->par[freq_1]; //par[1] is filter corner frequency
				//               omega_2zero = PI_x2_OVER_FREQUENCY * fp->par[freq_2];
				omega_2zero_tmp = (float)(PI_x2_OVER_FREQUENCY * fp.par[filt_par_Q_f_1]);
				a11 = Convert.ToSingle(omega_1pole - 1);
				//                // normally, and here as well, comes from F1
				b11 = Convert.ToSingle(omega_2zero_tmp - 1);
				//            // from Q1
				////alredy known         omega_2zero = PI_x2_OVER_FREQUENCY * fp->par[freq_2];
				omega_1pole_tmp = (float)(PI_x2_OVER_FREQUENCY * fp.par[filt_par_Q_f_2]);
				//; // filter corner 2nd frequency __F2_/-Q2--
				b12 = Convert.ToSingle(omega_2zero - 1);
				//;               // normally, and here as well, comes from F2
				a12 = Convert.ToSingle(omega_1pole_tmp - 1);
				//;   // now comes from Q2
				fp.b2 = Convert.ToSingle(b11 * b12 * t_gain);		// IK20210425 fixed
				fp.b1 = Convert.ToSingle((b11 + b12)* t_gain);		// IK20210425 fixed
				fp.b0 = Convert.ToSingle(t_gain);
				fp.a2 = Convert.ToSingle(a11 * a12);
				fp.a1 = Convert.ToSingle(a11 + a12);
			break;

			//  Case LAG_1stOrNorm:   'case 19: --\__  normalized maximum unity gain at lowest  frequency
			//    If (omega_1pole <= omega_2zero) Then
			//        h1 = omega_1pole / omega_2zero * t_gain '
			//        fp.a1 = CSng(omega_1pole - 1) '
			//        fp.b1 = CSng(omega_2zero - 1) '
			//    Else
			//        h1 = omega_2zero / omega_1pole * t_gain '
			//        fp.a1 = CSng(omega_2zero - 1) '
			//        fp.b1 = CSng(omega_1pole - 1) '
			//    End If
			//    fp.b0 = CSng(h1)  '
			//    fp.b1 = CSng(fp.b1 * h1) '
			//     ' Break '


			case (int)filt_types.LPF_2ndOr_wPk:
				// case 7: par1 = F1 = corner freq, par2,4 not used, par3 = Q factor Q=0.5 - classic, Q=2, peak approximately 2 times over unity

				// IK20210425 calculations were wrong !
				// gain = (omega_1pole/2)^2    * 1/(4 *(Q_1]^2) +1) * (FilterGain) <<<< WAS
				// gain = (omega_1pole/2)^2    * (1/4 /(Q_1]^2) +1) * (FilterGain) <<<< should be
				// gain = 0.25*(omega_1pole)^2 * (0.25/(Q_1]^2) +1) * (FilterGain)

				// omega_1pole ;  //correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1

				h2 = omega_1pole * omega_1pole * 0.25;						// IK20210425 fixed, temp use

				h1 = 0.25 * fp.par[filt_par_Q_f_1] * fp.par[filt_par_Q_f_1] + 1;	// IK20210425 fixed, temp use

				h2 = h2 * h1 * t_gain * freq_1_correction; // overall gain * correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				//overall gain
				h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
				fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole));
				fp.a2 = Convert.ToSingle(h1 * h1);
				fp.b0 = Convert.ToSingle(h2);
				fp.b1 = Convert.ToSingle(2.0 * h2);
				fp.b2 = Convert.ToSingle(h2);
			break;

			case (int)filt_types.HPF_2ndOr_wPk:
				//case 8:
				omega_1pole = omega_1pole * freq_1_correction;
				//  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
				fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole));
				fp.a2 = Convert.ToSingle(h1 * h1);
				fp.b0 = Convert.ToSingle(t_gain);
				fp.b1 = Convert.ToSingle(-2.0 * t_gain);
				fp.b2 = Convert.ToSingle(t_gain);
			break;

			case (int)filt_types.LEAD_2ndOr_wPk:
				//case 12:  __/--- , normalized (max gain = Q, can be > 1)
				//reverse F1 and F2 to maintain LEAD shape
				omega_1pole = omega_1pole * freq_1_correction;
				//  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				omega_2zero = omega_2zero * freq_2_correction;
				//  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				// gain_corr = s_freq_gain_corr * (omega_1pole/omega_2zero)^2
				if ((omega_1pole > omega_2zero)) {
					h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
					h2 = 1 - (omega_2zero / (2 * fp.par[filt_par_Q_f_2]));
					fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole));
					fp.a2 = Convert.ToSingle(h1 * h1);
					t_gain = t_gain / s_freq_gain_corr;
					h1 = System.Math.Cos(omega_2zero);
					//temp use for b1 calculation
					//omega_1pole <= omega_2zero
				} else {
					h1 = 1 - (omega_2zero / (2 * fp.par[filt_par_Q_f_2]));
					h2 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
					fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_2zero));
					fp.a2 = Convert.ToSingle(h1 * h1);
					t_gain = t_gain / s_freq_gain_corr;
					h1 = System.Math.Cos(omega_1pole);
					//temp use for b1 calculation
				}
				fp.b0 = Convert.ToSingle(t_gain);
				fp.b1 = Convert.ToSingle(-2 * h2 * h1 * t_gain);
				fp.b2 = Convert.ToSingle(h2 * h2 * t_gain);
			break;

			case (int)filt_types.LAG_2ndOr_wPk:
				//case 13:--\__  normalized (max gain = Q, can be > 1)
				// Q-factor gain correction: =(1+1/(2*Q_1)^2)/(1+1/(2*Q_2)^2)
				omega_1pole = omega_1pole * freq_1_correction;
				//  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1
				omega_2zero = omega_2zero * freq_2_correction;
				//  'correction for 2nd order LPF, HPF, Lead, Lag to have 90 degree phase at any Q_1

				h1 = omega_1pole / omega_2zero;
				h1 = h1 * h1;
				h2 = 1 + 0.25 / ((fp.par[filt_par_Q_f_2]) * (fp.par[filt_par_Q_f_2]));
				h2 = (1 + 0.25 / ((fp.par[filt_par_Q_f_1]) * (fp.par[filt_par_Q_f_1]))) / h2;
				h1 = h1 * h2;
				// h1 includes F2/F1, Q2 and Q1 gain correction

				//reverse F1 and F2 to maintain LEAD shape
				// gain_corr = s_freq_gain_corr * (omega_1pole/omega_2zero)^2
				if ((omega_1pole <= omega_2zero)) {
					t_gain = s_freq_gain_corr * t_gain * h1;
					//' h1 includes F2/F1, Q2 and Q1 gain correction
					h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
					h2 = 1 - (omega_2zero / (2 * fp.par[filt_par_Q_f_2]));
					fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole));
					fp.a2 = Convert.ToSingle(h1 * h1);
					h1 = System.Math.Cos(omega_2zero);
					//temp use for b1 calculation
					//omega_1pole > omega_2zero
				} else {
					t_gain = s_freq_gain_corr * t_gain / h1;
					//' h1 includes F2/F1, Q2 and Q1 gain correction
					h1 = 1 - (omega_2zero / (2 * fp.par[filt_par_Q_f_2]));
					h2 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
					fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_2zero));
					fp.a2 = Convert.ToSingle(h1 * h1);
					h1 = System.Math.Cos(omega_1pole);
					//temp use for b1 calculation
				}
				fp.b0 = Convert.ToSingle(t_gain);
				fp.b1 = Convert.ToSingle(-2 * h2 * h1 * t_gain);
				fp.b2 = Convert.ToSingle(h2 * h2 * t_gain);
			break;

			case (int)filt_types.PEAK_on_FLAT:
				// made from leadlag
				h2 = 0.89 + 0.064 / fp.par[filt_par_Q_f_1];
				//' Experimental automatic second frequency calculation to have unity gain from both sides of peak
				omega_2zero = omega_1pole * h2;
				h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
				h2 = 1 - (omega_2zero / 2);
				fp.a1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole));
				fp.a2 = Convert.ToSingle(h1 * h1);
				t_gain = t_gain * s_freq_gain_corr;
				fp.b0 = Convert.ToSingle(t_gain);
				fp.b1 = Convert.ToSingle(-2 * h2 * System.Math.Cos(omega_2zero) * t_gain);
				fp.b2 = Convert.ToSingle(h2 * h2 * t_gain);
			break;

			case (int)filt_types.NOTCH:
				// made from leadlag
				//  h1  = (fp.par[Q_f_1) - 1)'' automatic second frequency calculation to have unity gain from both sides of notch
				//  h2  = 1 / (fp.par[Q_f_1) + 1)'
				//  h2  = h2 * h2'
				//  omega_2zero = omega_1pole * (1 - h1 * h2)' = (1 - (Q - 1)/((Q + 1)^2))
				//    h2 = 0.89 + 0.064 / fp.par[Q_f_1) '' Experimental automatic second frequency calculation to have unity gain from both sides of peak
				h2 = 0.8208 + 0.1673 / fp.par[filt_par_Q_f_1] + 0.009105 * fp.par[filt_par_Q_f_1] + 1.825E-05 * fp.par[filt_par_Q_f_1] * fp.par[filt_par_freq_1];
				omega_2zero = omega_1pole * h2;
				h1 = 1 - (omega_1pole / (2 * fp.par[filt_par_Q_f_1]));
				h2 = 1 - (omega_2zero / 2);
				fp.a1 = Convert.ToSingle(-2 * h2 * System.Math.Cos(omega_2zero));
				fp.a2 = Convert.ToSingle(h2 * h2);
				t_gain = t_gain * s_freq_gain_corr;
				fp.b0 = Convert.ToSingle(t_gain);
				fp.b1 = Convert.ToSingle(-2 * h1 * System.Math.Cos(omega_1pole) * t_gain);
				fp.b2 = Convert.ToSingle(h1 * h1 * t_gain);
			break;

			case (int)filt_types.NOTCH_Sharp:
				//Q regulates "width" of a notch
				h1 = 1.0 / (1.0 + cotan_of_half_omega_1 / fp.par[filt_par_Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1);
				h2 = 2.0 * (1.0 - cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1;
				fp.a1 = Convert.ToSingle(h2);
				fp.a2 = Convert.ToSingle((1.0 - cotan_of_half_omega_1 / fp.par[filt_par_Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1);
				fp.b0 = Convert.ToSingle((1.0 + cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1 * t_gain);
				fp.b1 = Convert.ToSingle(h2 * t_gain); //IK 20210422 fix: added * t_gain; otherwise TF is correct only when filter gain == 1
				fp.b2 = fp.b0;
			break;

			case (int)filt_types.BPF:
				//"roof shape BPF" +20dB/dec-peak-20dB/dec' par1=peak freq, par3=Q1=peak mag
				h1 = 1.0 / (1.0 + cotan_of_half_omega_1 / fp.par[filt_par_Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1);
				h2 = t_gain * cotan_of_half_omega_1 * h1 / fp.par[filt_par_Q_f_1];
				//= (fp.par[FilterGain]*cotan_of_half_omega_1) / fp.par[Q_f_1) * h1'
				fp.a1 = Convert.ToSingle(2.0 * (1.0 - cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1);
				fp.a2 = Convert.ToSingle((1.0 - cotan_of_half_omega_1 / fp.par[filt_par_Q_f_1] + cotan_of_half_omega_1 * cotan_of_half_omega_1) * h1);
				fp.b0 = Convert.ToSingle(h2);
				fp.b1 = 0.0f;
				fp.b2 = Convert.ToSingle(-h2);
			break;

			case (int)filt_types.PID:
				//"leaky" integral
				h1 = (float)(fp.par[PID_I_gain] * 1 / LOOP_FREQUENCY);
				//      ' Integral
				h2 = (float)(fp.par[PID_D_gain] * LOOP_FREQUENCY / 1000);
				//   ' differential, /1000 because D gain is too big, should be in the range 0.001 then mag bend up happens at 100 Hz
				//par[3) is filter Q, here used as frequency of leak
				fp.a1 = (float)(PI_x2_OVER_FREQUENCY * fp.par[filt_par_Q_f_1] - 1.0);
				//slightly more, then -1.0f - "leaky" integral - ALLOWS GRADUALLY PURGE delay buffers
				// experimentally found 0.9999 cannot efficiently remove offset at low gain if OP is way too low,
				// but too much 0.99998 can overflow filter internal delay buffers and flip over at start, especially if input pressure only 10 psi above max iso pressure.
				//IT DEPENDS ON SAMPLE FREQUENCY -same a1, higher freq gives faster purge!!!
				fp.b0 = Convert.ToSingle(fp.par[PID_P_gain] + h1 + h2);
				fp.b1 = Convert.ToSingle(-(fp.par[PID_P_gain] + (2.0 * h2)));
				fp.b2 = Convert.ToSingle(h2);
			break;

			//  Case GEO_OPT: 'gain = 1 when f > F2, INTENDED USE: F1=0.2, F2=0.5' it is LEDLAG --2nd order with Q1=Q2~0.1
			//      '**** CAUTION WITH FIX POINT !!, VERY HIGH GAIN AT LOW FREQUENCY!!!
			//    h1 = Tan(omega_2zero / 2) 'use as temporary to calculate h2
			//    h2 = (1 - h1) / (1 + h1) '         ' = (1 - tan(PI*f2/fs) / (1 + tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
			//' Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
			//    h1 = (cotan_of_half_omega_1 - 1) / (cotan_of_half_omega_1 + 1) ' = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
			//
			//    fp.a1 = CSng(-2 * h2) '
			//    fp.a2 = CSng(h2 * h2) '
			//    fp.b0 = CSng(t_gain) '
			//    fp.b1 = CSng(-2 * h1 * t_gain) '
			//    fp.b2 = CSng(h1 * h1 * t_gain) '
			//     ' Break '

			case (int)filt_types.VELOCITY:
				h1 = System.Math.Tan(omega_2zero / 2);
				//use as temporary to calculate h2
				h2 = (1 - h1) / (1 + h1);
				//         ' = (1 - tan(PI*f2/fs) / (1 + tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
				// Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
				h1 = (cotan_of_half_omega_1 - 1) / (cotan_of_half_omega_1 + 1);
				// = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
				fp.a1 = Convert.ToSingle(-(h1 + h2));
				fp.a2 = Convert.ToSingle(h1 * h2);
				if ((omega_1pole > omega_2zero)) {                // when F2 > F1
					t_gain = t_gain * LOOP_FREQUENCY / (fp.par[filt_par_freq_2]) * 0.08;
				} else {                // when F2 < F1
					t_gain = t_gain * LOOP_FREQUENCY / (fp.par[filt_par_freq_1]) * 0.08;
				}
				fp.b0 = Convert.ToSingle((1 + (fp.a1) + (fp.a2)) * t_gain);
				//gain correction '-!- PHASE CORRECTED 09-25-09
				fp.b2 = -(fp.b0);
			break;

			case (int)filt_types.Position:
				h1 = System.Math.Tan(omega_2zero / 2);
				//use as temporary to calculate h2
				h2 = (1 - h1) / (1 + h1);
				//         ' = (1 - tan(PI*f2/fs) / (1 + tan(PI*f2/fs) approx = exp(-f2* PI_x2_OVER_FREQUENCY), better than 1% when fs/f0 > 63
				// Note: (1 - tan(x)) / (1 + tan(x)) = ((cotan(x) - 1) /  ((cotan(x) + 1)
				h1 = (cotan_of_half_omega_1 - 1) / (cotan_of_half_omega_1 + 1);
				// = (1 - tan(PI*f1/fs) / (1 + tan(PI*f1/fs)~= exp(-f1* PI_x2_OVER_FREQUENCY)
				fp.a1 = Convert.ToSingle(-(h1 + h2));
				fp.a2 = Convert.ToSingle(h1 * h2);
				fp.b0 = Convert.ToSingle(0.5 * (1 + fp.a1 + fp.a2) * t_gain);
				//        'fp.b0= -(1.0 + fp.a2 + fp.a1)/2''-!- PHASE CORRECTED 09-25-09
				fp.b1 = fp.b0;
			break;

			case (int)filt_types.GAIN_ONLY:
			break;

			default:
			break;
			}
			//'Reverse a1, a2 to use only ADD (summing) and do not use SUB (no subtraction)
			fp.a1 = -fp.a1;
			fp.a2 = -fp.a2;
		}
	}
}
