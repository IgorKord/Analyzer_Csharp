using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace TMCAnalyzer {
	/// <summary>Tools provides a set of useful tools and utilities.</summary>
	/// <remarks>
	/// Many of the declarations are dispersed to be with their associated constructs.</remarks>
	public static class Tools {

		/// <summary>Constrain constrains a value to be within a domain.</summary>
		/// <typeparam name="T">T may be any <see cref="System.IComparable"/> type.</typeparam>
		/// <param name="minValue">
		/// Minimum Value specifies the minimum allowed value. If this argument's value is greater
		/// than that for <paramref name="maxValue"/>, both arguments' values are automatically
		/// and internally swapped.</param>
		/// <param name="value">
		/// Value specifies the candidate value, which, on return, whose value shall be
		/// constrained to lie within the domain specified by [<paramref name="minValue"/>,
		/// <paramref name="maxValue"/>].</param>
		/// <param name="maxValue">
		/// Maximum Value specifies the maximum allowed value. If this argument's value is less
		/// than that for <paramref name="minValue"/>, both arguments' values are automatically
		/// and internally swapped.</param>
		/// <returns>constrained <paramref name="value"/></returns>
		/// <example><code>
		/// var /*int value*/ vlu = Tool.Constrain(0, 0, 9);
		/// System.Diagnostics.Debug.Assert(vlu == 0);
		/// vlu = Tool.Constrain(0, 1, 9);  System.Diagnostics.Debug.Assert(vlu == 1);
		/// vlu = Tool.Constrain(0, 9, 9);  System.Diagnostics.Debug.Assert(vlu == 9);
		/// vlu = Tool.Constrain(0, -1, 9); System.Diagnostics.Debug.Assert(vlu == 0);
		/// vlu = Tool.Constrain(0, 10, 9); System.Diagnostics.Debug.Assert(vlu == 9);
		/// vlu = Tool.Constrain(9, 5, 0); //treated as Constrain(0, 5, 9)</code></example>
		public static T Constrain<T>(T minValue, T value, T maxValue) where T : System.IComparable<T> {
			if (/*fix?*/ minValue.CompareTo(maxValue) > 0) { Swap(ref minValue, ref maxValue); }
			if (/*constrain?*/ value.CompareTo(minValue) < 0) {
				value = minValue;
			} else /*>= minValue*/ if (/*constrain?*/ value.CompareTo(maxValue) > 0) {
				value = maxValue;
			}
			return value;
		}


		/// <summary>
		/// Is OK is a support method for <code>string</code> internet protocol address and IP port
		/// validation and verification.</summary>
		/// </summary>
		/// <param name="ipAddress">
		/// IP Address specifies, in <code>string</code> form, the IP address.</param>
		/// <param name="portNumber">
		/// Port Number specifies the IP port number.</param>
		/// <param name="isThrow">
		/// Is Throw? specifies whether or not an invalid <paramref name="value"/> value
		/// should cause an exception to be thrown: <code>true</code> to throw an exception when
		/// <paramref name="value"/> is invalid, <code>false</code> to merely return a flag
		/// indicating validity. If omitted, this parameter's value defaults to
		/// <code>false</code>.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="value"/> is determined to be valid,
		/// <code>false</code> otherwise</returns>
		private static bool IsOK(string ipAddress, int portNumber, bool isThrow = false) {
			bool rsl = IsOK(ipAddress, IsOkStringModes.Populated, isThrow);
			var adr = System.Net.IPAddress.None; // address
			try {
				rsl &= System.Net.IPAddress.TryParse(ipAddress, out adr);
			} catch { /*suppress*/ }
			if (/*bad?*/ !rsl && isThrow) {
				throw new System.ArgumentException("\"" + ipAddress.Trim() + "\" " + NotIn + " {System.Net.IPAddress}");
			}
			rsl &= IsOK(portNumber, System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, isThrow);
			return rsl;
		}

		/// <summary>
		/// Diagnostic forms a <code>string</code> that states, using set notation, that a value is
		/// not a member of the set of valid values.</summary>
		/// <param name="value">
		/// Candidate value specifies, as a <code>string</code>, the candidate value.</param>
		/// <param name="leadText">
		/// Lead Text specifies, as a <code>string</code>, the leading value.</param>
		/// <param name="trailText">
		/// Trail Text specifies, optionally (default value of <code>null</code>) and as a
		/// <code>string</code>, the trailing value.</param>
		/// <param name="isLowerInclusive">
		/// Is Lower Inclusive specifies whether or not the domain is inclusive of
		/// <paramref name="minValue"/>, that is, whether or not the domain may be specified as
		/// [<paramref name="minValue"/>... or must be specified as (<paramref name="minValue"/>...
		/// If omitted, the default value for this argument is <code>true</code>.</param>
		/// <param name="isUpperInclusive">
		/// Is Upper Inclusive specifies whether or not the domain is inclusive of
		/// <paramref name="maxValue"/>, that is, whether or not the domain may be specified as
		/// ...<paramref name="maxValue"/>] or must be specified as
		/// ...<paramref name="maxValue"/>). If omitted, the default value for this argument is
		/// <code>true</code>.</param>
		/// <returns>diagnostic <code>string</code></returns>
		/// <example><code>
		/// var /*diagnostic message*/ dta = Tools.Diagnostic("-1", "0", "3");
		/// System.Diagnostics.Debug.Assert(dta.Equals("-1 ∉ {[0, 3]}"));
		/// dta = Tools.Diagnostic("George", "Fred, Mark, Sally");
		/// System.Diagnostics.Debug.Assert(dta.Equals("George ∉ {Fred, Mark, Sally}"));
		/// dta = Tools.Diagnostic("-1", "0", "3", false);
		/// System.Diagnostics.Debug.Assert(dta.Equals("-1 ∉ {(0, 3]}"));
		/// dta = Tools.Diagnostic("-1", "0", "3", true, false);
		/// System.Diagnostics.Debug.Assert(dta.Equals("-1 ∉ {[0, 3)}"));
		/// dta = Tools.Diagnostic("-1", "0", "3", false, false);
		/// System.Diagnostics.Debug.Assert(dta.Equals("-1 ∉ {(0, 3)}"));
		/// </code></example>
		public static string Diagnostic(string value, string leadText, string trailText = null, bool isLowerInclusive = true, bool isUpperInclusive = true) {

			value = string.IsNullOrWhiteSpace(value) ? "<unknown>" : value.Trim();
			value = string.IsNullOrWhiteSpace(value) ? "<unknown>" : value;    //yes, there really should be two assignment statements per parameter!
			leadText = string.IsNullOrWhiteSpace(leadText) ? "<unknown>" : leadText.Trim();
			leadText = string.IsNullOrWhiteSpace(leadText) ? "<unknown>" : leadText;    //yes, there really should be two assignment statements per parameter!
			string txt = value + NotIn;
			trailText = string.IsNullOrWhiteSpace(trailText) ? string.Empty : trailText.Trim();

			if (string.IsNullOrEmpty(trailText)) { //set notation?
				txt += "{" + leadText + "}";
			} else { // domain notation
				if (isLowerInclusive) {
					txt += "{[" + leadText + ", " + trailText.Trim();
				} else {
					txt += "{(" + leadText + ", " + trailText.Trim();  // (...
				}
				if (isUpperInclusive) {
					txt += "]}";
				} else {
					txt += ")}";  // ...)
				}
			}
			return txt;
		}

		/// <summary>
		/// Generic Limits provides <code>maxValue</code> and <code>minValue</code> for any numeric
		/// type.</summary>
		public static class GenericLimits<T> {
			public static readonly T MaxValue = (T)MaxValue.GetType().GetField("MaxValue").GetValue(MaxValue);
			public static readonly T MinValue = (T)MinValue.GetType().GetField("MinValue").GetValue(MinValue);
		}

		/// <summary>
		/// Is OK verifies the validity of an <code>enum</code> value.</summary>
		/// <param name="value">Value specifies an <code>enum</code> value to be examined.</param>
		/// <param name="isThrow">
		/// Is Throw? specifies whether or not an invalid <paramref name="value"/> value
		/// should cause an exception to be thrown: <code>true</code> to throw an exception when
		/// <paramref name="value"/> is invalid, <code>false</code> to merely return a flag
		/// indicating validity. If omitted, this parameter's value defaults to
		/// <code>false</code>.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="value"/> is determined to be valid,
		/// <code>false</code> otherwise</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="value"/> specifies an invalid value (and
		/// <paramref name="isThrow"/> specifies <code>true</code>), an exception is
		/// thrown.</exception>
		/// <example><code>
		/// var /*bool result*/ rsl = Tools.IsOK(</code>valid value<code>);
		/// System.Diagnostics.Debug.Assert(rsl);
		/// Tools.IsOK(</code>invalid value<code>, /*exception if invalid*/ true);  //exception
		/// rsl = Tools.IsOK(</code>invalid value<code>);
		/// System.Diagnostics.Debug.Assert(!rsl);</code></example>
		/// <seealso cref="Tools.Diagnostic"/>
		public static bool IsOK<T>(T value, bool isThrow = false) {
			bool rsl = Enum.IsDefined(typeof(T), value);
			if (rsl || !isThrow) return rsl;

			Diagnostic("<value>", value.GetType().ToString());
			try {
				// Making use of Tools.Diagnostic, throw an exception.
				string msg = Diagnostic(value.ToString(), value.GetType().ToString());
				throw new ArgumentOutOfRangeException(msg);
			} catch {
				// ignored
			}

			return false;
		}

		/// <summary>Is OK is a support method for value validation and verification.</summary>
		/// <param name="value">Value specifies the value to be examined.</param>
		/// <param name="minValue">
		/// Minimum Value specifies the minimum allowed value. If this argument's value is greater
		/// than that for <paramref name="maxValue"/>, both arguments' values are automatically
		/// and internally swapped.</param>
		/// <param name="maxValue">
		/// Maximum Value specifies the maximum allowed value. If this argument's value is less
		/// than that for <paramref name="minValue"/>, both arguments' values are automatically
		/// and internally swapped.</param>
		/// <param name="isThrow">
		/// Is Throw? specifies whether or not an invalid <paramref name="value"/> value
		/// should cause an exception to be thrown: <code>true</code> to throw an exception when
		/// <paramref name="value"/> is invalid, <code>false</code> to merely return a flag
		/// indicating validity. If omitted, this parameter's value defaults to
		/// <code>false</code>.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="value"/> is determined to be valid,
		/// <code>false</code> otherwise</returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="value"/>, <paramref name="maxValue"/> or
		/// <paramref name="minValue"/> specify <code>null</code>, an exception is
		/// thrown.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="value"/>'s value does not lie within the domain
		/// [<paramref name="minValue"/>, <paramref name="maxValue"/>], an exception is
		/// thrown.</exception>
		/// <seealso cref="Tools.Diagnostic"/>
		public static bool IsOK<T>(T value, T minValue, T maxValue, bool isThrow = false) where T : IComparable<T>
		{
			bool rsl = value != null && minValue != null && maxValue != null;
			if (rsl) return true;
			if (isThrow) throw new ArgumentNullException();

			if (minValue != null && minValue.CompareTo(maxValue) > 0) Swap(ref minValue, ref maxValue);

			return false;
		}

		/// <summary>Is OK is a support method for value validation and verification.</summary>
		/// <param name="value">Value specifies the value to be examined.</param>
		/// <param name="minValue">
		/// Minimum Value specifies the minimum allowed value. If this argument's value is greater
		/// than that for <paramref name="maxValue"/>, both arguments' values are automatically
		/// and internally swapped.</param>
		/// <param name="maxValue">
		/// Maximum Value specifies the maximum allowed value. If this argument's value is less
		/// than that for <paramref name="minValue"/>, both arguments' values are automatically
		/// and internally swapped.</param>
		/// <param name="isThrow">
		/// Is Throw? specifies whether or not an invalid <paramref name="value"/> value
		/// should cause an exception to be thrown: <code>true</code> to throw an exception when
		/// <paramref name="value"/> is invalid, <code>false</code> to merely return a flag
		/// indicating validity. If omitted, this parameter's value defaults to
		/// <code>false</code>.</param>
		/// <param name="isLowerInclusive">
		/// Is Lower Inclusive specifies whether or not the domain is inclusive of
		/// <paramref name="minValue"/>, that is, whether or not the domain may be specified as
		/// [<paramref name="minValue"/>... or must be specified as (<paramref name="minValue"/>...
		/// If omitted, the default value for this argument is <code>true</code>.</param>
		/// <param name="isUpperInclusive">
		/// Is Upper Inclusive specifies whether or not the domain is inclusive of
		/// <paramref name="maxValue"/>, that is, whether or not the domain may be specified as
		/// ...<paramref name="maxValue"/>] or must be specified as
		/// ...<paramref name="maxValue"/>). If omitted, the default value for this argument is
		/// <code>true</code>.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="value"/> is determined to be valid,
		/// <code>false</code> otherwise</returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="value"/>, <paramref name="maxValue"/> or
		/// <paramref name="minValue"/> specify <code>null</code>, an exception is
		/// thrown.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// If <paramref name="value"/>'s value does not lie within the domain
		/// [<paramref name="minValue"/>, <paramref name="maxValue"/>], an exception is
		/// thrown.</exception>
		/// <seealso cref="Tools.Diagnostic"/>
		public static bool IsOK<T>(T value, T minValue, T maxValue, bool isThrow = false, bool isLowerInclusive = true, bool isUpperInclusive = true) where T : System.IComparable<T>
		{
			bool rsl = (value != null) && (minValue != null) && (maxValue != null);
			if (/*bad (null)?*/ !rsl) {
				if (isThrow) { throw new System.ArgumentNullException(); }
			} else /*not null*/ {
				if (/*fix?*/ minValue.CompareTo(maxValue) > 0) { Swap(ref minValue, ref maxValue); }
				if (isLowerInclusive) { //[...
					rsl &= value.CompareTo(minValue) >= 0;
				} else /*(...*/ { rsl &= value.CompareTo(minValue) > 0; }
				if (isUpperInclusive) { //...]
					rsl &= value.CompareTo(maxValue) <= 0;
				} else /*...)*/ { rsl &= value.CompareTo(maxValue) < 0; }
				if (/*bad?*/ !rsl && /*throw?*/ isThrow) {
					var /*diagnostic message*/ msg = Tools.Diagnostic("<value>", minValue.ToString(), maxValue.ToString());
					try {
						// Making use of Tools.Diagnostic, throw an exception.
						msg = Tools.Diagnostic(value.ToString(), value.GetType().ToString());
						throw new System.ArgumentOutOfRangeException(msg);
					} catch { /*suppress*/ }
				}
			}
			return rsl;
		}

		/// <summary>
		/// Is OK is a support method for <code>string</code> validation and verification.</summary>
		/// <param name="value">Value specifies the candidate value.</param>
		/// <param name="mode">
		/// Mode specifies the type of validation to be performed:
		/// <br/><see cref="IsOkStringModes.EmptyWhiteOk"/>: <paramref name="value"/> merely must
		/// specify non-<code>null</code>,
		/// <br/><see cref="IsOkStringModes.Populated"/>: <paramref name="value"/> must specify a
		/// <code>string</code> for which <code>string.IsNullOrWhiteSpace</code> yields
		/// <code>false</code>,
		/// <br/><see cref="IsOkStringModes.WhiteOk"/>: <paramref name="value"/> must specify a
		/// <code>string</code> for which <code>string.IsNullOrEmpty</code> yields
		/// <code>false</code>.
		/// </param>
		/// <param name="isThrow">
		/// Is Throw? specifies whether or not an invalid <paramref name="value"/> value
		/// should cause an exception to be thrown: <code>true</code> to throw an exception when
		/// <paramref name="value"/> is invalid, <code>false</code> to merely return a flag
		/// indicating validity. If omitted, this parameter's value defaults to
		/// <code>false</code>.</param>
		/// <returns>
		/// <code>true</code> if <paramref name="value"/> is determined to be valid,
		/// <code>false</code> otherwise</returns>
		/// <exception cref="System.ArgumentNullException">
		/// If <paramref name="isThrow"/> specifies <code>true</code> and if
		/// <paramref name="value"/> specifies <code>null</code>, an exception is
		/// thrown.</exception>
		/// <exception cref="System.ArgumentException">
		/// If <paramref name="isThrow"/> specifies <code>true</code>, if
		/// <paramref name="value"/> resolves to <code>string.Empty</code> after
		/// Trimming and emptyWhiteIsOK does not specify
		/// <code>true</code>, an exception is thrown.</exception>
		/// <example><code>
		/// var /*text*/ txt = </code>some function returning <code>string</code>...<code>;
		/// Tools.IsOK(txt, /*empty OK*/ true, /*exception*/ true);</code></example>
		public static bool IsOK(string value, IsOkStringModes mode, bool isThrow = false) {
			bool rsl = false;
			IsOK(mode, true);
			if (value != null) return false;

			if (isThrow) throw new ArgumentNullException();
			switch (mode) {
				case IsOkStringModes.EmptyWhiteOk:
					rsl = true; //essentially, this is just it's not null
					break;
				// not empty
				case IsOkStringModes.WhiteOk:
					break;
				case IsOkStringModes.Populated:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}

			return rsl;
		}

		/// <summary>See <see cref="IsOK(string, IsOkStringModes, bool)"/></summary>
		public enum IsOkStringModes { EmptyWhiteOk, Populated, WhiteOk };

		/// <summary>
		/// Not In specifies the character to be used to announce that a value is not in a
		/// set. This character is outside the ISO-646US ("ASCII") domain of code points and
		/// should be specified as an escape sequence rather than directly.</summary>
		private const char NotIn = '\u2209';  // ∉

		/// <summary>
		/// Micro specifies the miniscule, Greek micro character used to denote one
		/// one-millionths.</summary>
		public const char Micro = '\u03BC'; // μ

		/// <summary>Swap exchanges two values.</summary>
		/// <param name="value0">
		/// Value 0 references the former of a pair of identically typed scalar value
		/// items.</param>
		/// <param name="value1">
		/// Value 1 references the latter of a pair of identically typed scalar value
		/// items.</param>
		/// <example><code>
		/// var /*former value*/ fmr = 1.2;
		/// var /*latter value*/ ltr = 3.4;
		/// Swap(ref fmr, ref ltr);
		/// System.Diagnostics.Debug.Assert((fmr == 3.4) && (ltr == 1.2));</code></example>
		private static void Swap<T>(ref T value0, ref T value1) {
			var tmp = value0;
			value0 = value1;
			value1 = tmp;
		}

		/// <summary>
		/// Text Clean ensures that a <code>string</code> is not <code>null</code> (replacing the
		/// value with <code>string.Empty</code>) and is
		/// Trimmed.</summary>
		/// <param name="theText">The Text specifies the text to be cleansed.</param>
		/// <returns>cleansed text from <paramref name="theText"/></returns>
		public static string TextClean(string theText) {
			theText = string.IsNullOrWhiteSpace(theText) ? string.Empty : theText.Trim();   //yes, there must be...
			theText = string.IsNullOrWhiteSpace(theText) ? string.Empty : theText;          //...two statements
			return theText;
		}

		/// <summary>
		/// Tidy Text by removing leading and trailing white space, converting tabs to spaces,
		/// changing line-ending sequences to simple carriage-returns, eliminating blank lines and
		/// making unprintable characters visible.</summary>
		/// <remarks>
		/// The carriage-return only default value for <paramref name="lineEnd"/> is to support
		/// <see cref="System.Windows.Controls.ListBox"/>, which treats carriage-return and
		/// line-feed, each, as unique line-ends.</remarks>
		/// <param name="sourceText">
		/// Text specifies the source text. If <code>null</code> or empty, the result is
		/// <code>string.Empty</code>.</param>
		/// <param name="tabSize">
		/// Tabulation Size specifies the tabulation interval and must be in the domain [1, ...).
		/// Values outside of the valid domain are treated as if they were unity.</param>
		/// <param name="lineEnd">
		/// Line End specifies, optionally, a desired, line ending sequence to be used for all line
		/// endings. If this argument's value is empty, <code>null</code>, omitted or "white
		/// space," it is treated as if it were <code>"\r"</code>.</param>
		/// <returns>tidied text</returns>
		/// <example><code>
		/// string /*text*/ trm = Tool.TextTidy("Some\t\"text\"\r\n\n\ron\nmany\tlines.");
		/// System.Diagnostics.Debug.Assert(trm == "Some \"text\"\ron\r\nmany  lines.");</code></example>
		public static string TextTidy(string sourceText, int tabSize = 2, string lineEnd = null) {
			if (string.IsNullOrEmpty(sourceText)) {// insubstantial
				sourceText = string.Empty;
			} else {
				if ( string.IsNullOrWhiteSpace(lineEnd)) { lineEnd = "\r"; } // replace EOS "\0" with "\r"
				sourceText = sourceText.Trim();	//get rid of leading/trailing crap

				// Convert horizontal tabulations to spaces and line ends to simple returns.
				var col = 1;
				string NewText = string.Empty; // target string
				tabSize = Math.Max(tabSize, 1); // ensure at least one

				foreach (char chr in sourceText) {
					switch (chr) {
						case '\t': { // tabulation
							do {
								NewText += ' ';
								col++;
							} while (col % tabSize != 0); // until next tabulation point

							break;
						}
						case '\n': //line end?
						case '\r': {
							if (col >= 2) NewText += '\r';
							col = 1;
							break;
						}
						default: {
							if (char.IsControl(chr)) // unprintable?
								NewText += @"\u" + ((int)chr).ToString("X4") + ' ';
							else
								NewText += chr; // printable; use as-is

							col++;
							break;
						}
					}
				}

				sourceText = NewText;

				// Remove space(s) before line ends and then empty lines. Finally, replace the
				// simple line end with the specified sequence.
				while (sourceText.IndexOf(" \r", StringComparison.Ordinal) >= 0)
					sourceText = sourceText.Replace(" \r", "\r");

				while (sourceText.IndexOf("\r\r", StringComparison.Ordinal) >= 0)
					sourceText = sourceText.Replace("\r\r", "\r");

				if (lineEnd != "\r") sourceText = sourceText.Replace("\r", lineEnd);
			}

			return sourceText;
		}

		/// <summary>Tidy Text for use as an element in a Comma Separated Values file.</summary>
		/// <param name="sourceText">
		/// Source Text specifies the source text. If this argument's value is <code>null</code>,
		/// it is treated as if it were <code>string.Empty</code>.</param>
		/// <returns>text suitable for use in a CSV file</returns>
		/// <example><code>
		/// string /*text*/ trm = TextTidyCSV("Some \"text\"\r\n\ron\nmany lines.");
		/// System.Diagnostics.Debug.Assert(trm == "\"Some \"\"text\"\"\r\non\r\nmany lines.\"");</code></example>
		public static string TextTidyCSV(string sourceText) {
			if (/*invalid?*/ sourceText == null) { sourceText = string.Empty; }
			sourceText = TextTidy(sourceText, 2, "\r\n");
			sourceText = sourceText.Replace("\"", "\"\"");
			sourceText = "\"" + sourceText + "\"";
			return sourceText;
		}

		/// <summary>
		/// TraceLevel Parse extracts a <see cref="System.Diagnostics.TraceLevel"/> from a
		/// <code>string</code>.</summary>
		/// <param name="sourceText">
		/// Source Text specifies the <code>string</code> from which the value is extracted. Before
		/// use, the value is <see cref="System.String.Trim"/>'d and, optionally (see
		/// <paramref name="isCaseLess"/>), case-normalized. The enumeration is expected to be the
		/// trailing, non-white substring in the value; for example: " A LastWarning " is treated
		/// as "A LastWarning", from which, "Warning" is recognized as
		/// <see cref="System.Diagnostics.TraceLevel.Warning"/>.</param>
		/// <param name="isCaseLess">
		/// Caseless specifies whether or not text is to be managed in a case-independent manner.
		/// If this argument is omitted or specifies <code>false</code>, case-sensitive parsing is
		/// performed. Note that since C# is a case-sensitive language, enumerations of, for
		/// example, <code>off</code>, <code>Off</code>, <code>OFF</code>, <code>OfF</code> and
		/// <code>oFf</code> are treated as distinct, unique and valid values (regardless of
		/// whether such declarations are desirable coding practice).</param>
		/// <returns><see cref="System.Diagnostics.TraceLevel"/></returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="sourceText"/> specifies <code>null</code> or just empty/white text,
		/// an exception is thrown.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="sourceText"/> specifies an unknown text representation of
		/// <see cref="System.Diagnostics.TraceLevel"/>, an exception is thrown.</exception>
		/// <example><see cref="TraceLevelTryParse"/></example>
		/// <remarks>This method could be rewritten to use
		/// <see cref="System.Collections.IEnumerable"/> and thereby support generic
		/// enumerations.</remarks>
		private static TraceLevel TraceLevelParse(string sourceText, bool isCaseLess = false)
		{
			if (string.IsNullOrWhiteSpace(sourceText)) throw new ArgumentNullException();

			sourceText = sourceText.Trim();

			if (isCaseLess) sourceText = sourceText.ToLower(CultureInfo.CurrentCulture);

			foreach (TraceLevel lvl in Enum.GetValues(typeof(TraceLevel))) {
				var txt = lvl.ToString();
				if (isCaseLess) txt = txt.ToLower(CultureInfo.CurrentCulture);
				if (sourceText.EndsWith(txt, StringComparison.OrdinalIgnoreCase)) return lvl;
			}

			throw new ArgumentOutOfRangeException();
		}

		/// <summary>
		/// TraceLevel Try Parse extracts a <see cref="System.Diagnostics.TraceLevel"/> from a
		/// <code>string</code>.</summary>
		/// <param name="sourceText">
		/// Source Text specifies the <code>string</code> from which the value is extracted. Before
		/// use, the value is Trimmed and, optionally (see
		/// <paramref name="isCaseLess"/>), case-normalized. The enumeration is expected to be the
		/// trailing, non-white substring in the value; for example: " A LastWarning " is treated
		/// as "A LastWarning", from which, "Warning" is recognized as
		/// <see cref="System.Diagnostics.TraceLevel.Warning"/>. If this argument specifies
		/// <code>null</code> or is otherwise empty, an <see cref="System.ArgumentNullException"/>
		/// is thrown.</param>
		/// <param name="parsedValue">
		/// Parsed Value specifies by reference the item to received the converted value.</param>
		/// <param name="isCaseLess">
		/// Caseless specifies whether or not text is to be managed in a case-independent manner.
		/// If this argument is omitted or specifies <code>false</code>, case-sensitive parsing is
		/// performed. Note that since C# is a case-sensitive language, enumerations of, for
		/// example, <code>off</code>, <code>Off</code>, <code>OFF</code>, <code>OfF</code> and
		/// <code>oFf</code> are treated as distinct, unique and valid values (regardless of
		/// whether such declarations are desirable coding practice).</param>
		/// <returns><code>true</code> if and only if the conversion succeeded</returns>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="sourceText"/> specifies <code>null</code> or just empty/white text,
		/// an exception is thrown.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="sourceText"/> specifies an unknown text representation of
		/// <see cref="System.Diagnostics.TraceLevel"/>, an exception is thrown.</exception>
		/// <example><code>
		/// System.Diagnostics.TraceLevel logLevel = System.Diagnostics.TraceLevel.Off;
		/// if (TraceLevelTryParse(" Bad error   ", ref logLevel, /*caseless*/ true)) {</code>...
		/// </example>
		/// <remarks>This method could be rewritten to use
		/// <see cref="System.Collections.IEnumerable"/> and thereby support generic
		/// enumerations.</remarks>
		public static bool TraceLevelTryParse(string sourceText, ref TraceLevel parsedValue, bool isCaseLess = false) {
			bool rsl = true; //result (returned)
			try {
				parsedValue = TraceLevelParse(sourceText, isCaseLess);
			} catch { rsl = false; }
			return rsl;
		}

		/// <summary>
		/// Trace Prepare prepares a simple logging facility. If the project's build properties
		/// specify <code>TRACE</code> (different from <code>DEBUG</code>) for all builds, any
		/// application code may call <see cref="System.Diagnostics.Trace.WriteLine"/> and have the
		/// specified text written to applicationName<code>Log.txt</code> and residing in the same
		/// folder as the application's .EXE file.</summary>
		/// <example><code>public App() {
		/// Tool.TracePrepare();</code>...</example>
		public static void TracePrepare() {
			// Provide a default and simple logging facility.
			var declaringType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
			if (declaringType == null) return;

			string nam = declaringType.Name; // application's name
			const string SFX = "Log.txt"; // suffix
			Trace.Listeners.Add(new TextWriterTraceListener(new System.IO.StreamWriter(nam + SFX, false)));
			Trace.AutoFlush = true;
			const string FMT = "o"; // time format. shows fractions of seconds
			Trace.WriteLine(nam + " started " + DateTime.Now.ToString(FMT, CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Version, Text gets, as a <code>string</code>, the version and build time for the
		/// application.</summary>
		/// <returns>
		/// <code>string</code> description of version ... for example:
		/// <code>"1.0 2015-01-01T01:02:03 (</code>b<code>.</code>r<code>)"</code>
		/// where b and r are build and revision values</returns>
		/// <remarks>
		/// Version information for an assembly consists of the following four values: Major
		/// version, Minor version, Build number and Revision. In the project's Properties
		/// AssemblyInfo.cs file, you can specify all the values or you can default the Build and
		/// Revision numbers by using the '*' as
		/// <br/><code>[assembly: AssemblyVersion("1.0.*")]</code>
		/// <br/>which loads, on each build, the Build number with a count of days since 2000-01-01
		/// and the Revision with a count of pairs of seconds since midnight (local time rather
		/// than UTC "Zulu," unfortunately). Be aware that using Visual Studio to edit the assembly
		/// values can sometimes overwrite <code>[assembly: AssemblyVersion("1.0.*")]</code>,
		/// requiring you to manually modify, again, the file.</remarks>
		/// <example>
		/// <code>string /*text*/ trm = "version " + VersionText;</code>
		/// <br/>Why doesn't the property supply the <code>"version "</code> text?
		/// Internationalization! If the returned value incorporated a text it would have to be
		/// trimmed and re-prefixed; for example, if the application were to be localized for The
		/// Netherlands, <code>"version"</code> would have to be replaced by
		/// <code>"uitvoering"</code> (Nederlandse for "version").</example>
		public static string VersionText {
			get { // there is no set
				var ver = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
				var tim = new DateTime(2000, 1, 1);
				Debug.Assert(ver != null, nameof(ver) + " != null");
				tim = tim.AddDays(ver.Build);
				tim = tim.AddSeconds(2 * ver.Revision); // count of PAIRS of seconds
				const string FMT = "s"; // time format. no fractions of seconds: not enough data
				return ver.Major + "." + ver.Minor + " " + tim.ToString(FMT, CultureInfo.CurrentCulture) + " (" +
					   ver.Build + "." + ver.Revision + ")";
			}
		}

		private static System.DateTime debugTimerBase = System.DateTime.Now;
		public static System.TimeSpan DebugTimer {
			get { return System.DateTime.Now.Subtract(debugTimerBase); }
			set { debugTimerBase = System.DateTime.Now; }
		}
		private static DateTime RefTimerBase = DateTime.Now;

		public static TimeSpan RefTimer {
			get => DateTime.Now.Subtract(RefTimerBase);
			set => RefTimerBase = DateTime.Now;
		}

		public static void msSleep(int ms) {
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			while (stopwatch.ElapsedMilliseconds < ms) {
				Application.DoEvents();
			}
		}

		/// <summary>
		/// function analyzes received parameter string for presence of "="
		/// if found, tries to convert string between "=" and possible "//" to double
		/// example: cmd "g_ff14\r"; response "g_ff14wn=-0.690"
		/// </summary>
		/// <param name="controller_param">response on "get" command like "g_ff14"</param>
		/// <param name="paramValue">returned double by ref</param>
		/// <returns>if succeeds, returns true; if cannot convert, false </returns>
		public static bool UpdateValueFromResponse(string controller_param, out double paramValue) {
			int sep_pos = controller_param.IndexOf("=");
			if (sep_pos != -1) {
				string StrVal = controller_param.Substring(sep_pos + 1);
				sep_pos = StrVal.IndexOf("//");  // if verbose response
				if (sep_pos != -1)
					StrVal = StrVal.Substring(0, sep_pos - 1);
				return double.TryParse(StrVal, out paramValue);
			} else {
				paramValue = 0;
				return false;
			}
		}

	}
}