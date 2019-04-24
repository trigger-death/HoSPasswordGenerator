using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HourglassPass.Internal {
	/*internal enum LetterFormatType {
		Numeric,
		String,
		NormalizedString,
		Binary,
		Hexidecimal,
		Decimal,
	}*/
	[Flags]
	public enum PasswordStyles : uint {
		Any = 0,
		Password = 1,
		Binary = 2,
		Hex = 3,
		Hexidecimal = Hex,
		//Decimal = 4,

		AllowSpecifier = (1 << 8),

		FlagsMask = 0xFFFFFF00,
	}
	internal static class LetterUtils {
		/// <summary>
		///  Formats the letter.
		/// </summary>
		/// <param name="l">The letter to format.</param>
		/// <param name="format">
		///  The format to display the letter in.<para/>
		///  S/s = Default, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <param name="ignoreNR">True if randomize and normalize are not applied.</param>
		/// <returns>The formatted letter.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		internal static string Format(this Letter l, string format, IFormatProvider formatProvider, bool ignoreNR = false) {
			if (format == null || format.Length == 0)
				return l.ToString();

			if (format.Length != 1)
				throw new FormatException($"Letter format can only contain one letter, got \"{format}\"!");

			switch (format[0]) {
			case 'S': return l.ToString();
			case 's': return l.ToString().ToLower();
			case 'N': return (ignoreNR ? l : l.Normalized()).ToString();
			case 'n': return (ignoreNR ? l : l.Normalized()).ToString().ToLower();
			case 'R': return (ignoreNR ? l : l.Randomized()).ToString();
			case 'r': return (ignoreNR ? l : l.Randomized()).ToString().ToLower();
			case 'B':
			case 'b': return Convert.ToString(l.Value, 2).PadLeft(4, '0');
			case 'X': return Convert.ToString(l.Value, 16).ToUpper();
			case 'x': return Convert.ToString(l.Value, 16).ToLower();
			case 'D':
			case 'd': return l.Value.ToString();
			default:
				throw new FormatException($"Letter format must be one of the following: [SNRBDXx], got \"{format}\"!");
			}
		}
		/// <summary>
		///  Formats the letter string.
		/// </summary>
		/// <param name="ls">The letter string to format.</param>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <param name="name">The readable name of the letter string type being formatted.</param>
		/// <returns>The formatted letter string.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="ls"/> is null.
		/// </exception>
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		internal static string Format(this ILetterString ls, string format, IFormatProvider formatProvider, string name) {
			if (ls == null)
				throw new ArgumentNullException(nameof(ls));
			if (format == null || format.Length == 0)
				return string.Join("", ls);

			if (format.StartsWith("P")) {
				if (format.Length == 1) {
					throw new FormatException(
						$"{name} format P requires a second character of [SsNnRrBbDdXx], got \"\"!");
				}
				string lf = format.Substring(1, 1);
				ushort spacing = 0;
				if (format.Length > 2) {
					if (!ushort.TryParse(format.Substring(2), out spacing))
						throw new FormatException($"{name} format P failed to parse spacing, got \"{format.Substring(2)}\"!");
				}
				else if (lf == "D") {
					// Decimal is unreadable without spacing
					spacing = 1;
				}
				if (lf == "N" || lf == "n")
					ls = ls.Normalized();
				else if (lf == "R" || lf == "r")
					ls = ls.Randomized();
				return string.Join(new string(' ', spacing), ls.Select(l => l.Format(lf, formatProvider, true)));
			}
			else if (format.StartsWith("VB")) {
				string binStr = Convert.ToString(ls.Value, 2).PadLeft(ls.Count * 4, '0');
				ushort spacing = 0;
				if (format.Length > 2) {
					if (!ushort.TryParse(format.Substring(2), out spacing))
						throw new FormatException($"{name} format NB failed to parse spacing, got \"{format.Substring(2)}\"!");
				}
				if (spacing == 0)
					return binStr;
				// Space binary in sets of 4 bits.
				StringBuilder str = new StringBuilder(binStr);
				string space = new string(' ', spacing);
				for (int i = binStr.Length - 4; i > 0; i-= 4)
					str.Insert(i, space);
				return str.ToString();
			}
			else if (format.StartsWith("V")) {
				return ls.Value.ToString(format.Substring(1), formatProvider);
			}
			else {
				throw new FormatException($"{name} format expected prefix P, VB, or V, got \"{format}\"!");
			}
		}

		internal static Letter ParseLetter(string s, PasswordStyles style) {
			if (s == null)
				throw new ArgumentNullException(nameof(s));
			switch (style) {
			case PasswordStyles.Any:
			case PasswordStyles.Password:
				if (s.Length != 1)
					throw new ArgumentException($"Letter Password string must be one character long, got {s.Length}!");
				return new Letter(s[0]);
			case PasswordStyles.Hex:
				if (s.Length != 1)
					throw new ArgumentException($"Letter Hex string must be one character long, got {s.Length}!");
				return new Letter(Convert.ToInt32(s, 16));
			case PasswordStyles.Binary:
				if (s.Length != 4)
					throw new ArgumentException($"Letter Binary string must be four characters long, got {s.Length}!");
				return new Letter(Convert.ToInt32(s, 2));
			default:
				throw new ArgumentException($"Invalid Password Style {style}!");
			}
		}
		internal static bool TryParseLetter(string s, PasswordStyles style, out Letter letter) {
			letter = new Letter();
			if (s == null || s.Length == 0)
				return false;
			char c; ;
			switch (style) {
			case PasswordStyles.Any:
			case PasswordStyles.Password:
				c = s[0];
				if (s.Length != 1 || !Letter.IsValidChar(c))
					return false;
				letter = new Letter(c);
				return true;
			case PasswordStyles.Hex:
				c = char.ToUpper(s[0]);
				if (s.Length != 1 || (!char.IsDigit(c) && c < 'A' && c > 'F'))
					return false;
				letter = new Letter(Convert.ToInt32(s, 16));
				return true;
			case PasswordStyles.Binary:
				if (s.Length != 4)
					return false;
				for (int i = 0; i < 4; i++) {
					if (s[i] != '0' && s[i] != '1')
						return false;
				}
				letter = new Letter(Convert.ToInt32(s, 2));
				return true;
			default:
				return false;
			}
		}

		internal static Letter[] ParseLetterString(string s, PasswordStyles style, string name, int length) {
			if (s == null)
				throw new ArgumentNullException(nameof(s));
			bool allowSpecifier = style.HasFlag(PasswordStyles.AllowSpecifier);
			style &= ~PasswordStyles.FlagsMask;
			PasswordStyles specifier = PasswordStyles.AllowSpecifier;
			if (s.StartsWith("0B") || s.StartsWith("0b"))
				specifier = PasswordStyles.Binary;
			else if (s.StartsWith("0X") || s.StartsWith("0x"))
				specifier = PasswordStyles.Hex;
			if (specifier != PasswordStyles.AllowSpecifier) {
				if (!allowSpecifier)
					throw new ArgumentException($"{name} format specifier is not allowed!");
				if (style != PasswordStyles.Any && specifier != style)
					throw new ArgumentException($"{name} format specifier does not match style!");
				style = specifier;
				s = s.Substring(2);
			}
			s = Regex.Replace(s, @"\s+", "");
			if (s.Length != length * (style == PasswordStyles.Binary ? 4 : 1))
				throw new ArgumentException($"{name} string must be {length} letters long, got {s.Length} letters!",
					nameof(s));
			if (length == 0)
				return Array.Empty<Letter>();

			Letter[] letters = new Letter[length];
			switch (style) {
			case PasswordStyles.Any:
			case PasswordStyles.Password:
				for (int i = 0; i < length; i++)
					letters[i] = new Letter(s[i]);
				return letters;
			case PasswordStyles.Hex:
				for (int i = 0; i < length; i++)
					letters[i] = new Letter(Convert.ToInt32(s.Substring(i, 1), 16));
				return letters;
			case PasswordStyles.Binary:
				length *= 4;
				for (int i = 0; i < length; i += 4)
					letters[i] = new Letter(Convert.ToInt32(s.Substring(i, 4), 2));
				return letters;
			default:
				throw new ArgumentException($"Invalid {name} Password Style {style}!");
			}
		}

		internal static bool TryParseLetterString(string s, PasswordStyles style, string name, int length, out Letter[] letters) {
			letters = null;
			if (s == null)
				return false;
			bool allowSpecifier = style.HasFlag(PasswordStyles.AllowSpecifier);
			style &= ~PasswordStyles.FlagsMask;
			PasswordStyles specifier = PasswordStyles.AllowSpecifier;
			if (s.StartsWith("0B") || s.StartsWith("0b"))
				specifier = PasswordStyles.Binary;
			else if (s.StartsWith("0X") || s.StartsWith("0x"))
				specifier = PasswordStyles.Hex;
			if (specifier != PasswordStyles.AllowSpecifier) {
				if (!allowSpecifier || style != PasswordStyles.Any && specifier != style)
					return false;
				style = specifier;
				s = s.Substring(2);
			}
			s = Regex.Replace(s, @"\s+", "");
			if (s.Length != length * (style == PasswordStyles.Binary ? 4 : 1))
				return false;
			if (length == 0) {
				letters = Array.Empty<Letter>();
				return true;
			}

			Letter[] newLetters = new Letter[length];
			switch (style) {
			case PasswordStyles.Any:
			case PasswordStyles.Password:
			case PasswordStyles.Hex:
				for (int i = 0; i < length; i++) {
					if (!TryParseLetter(s.Substring(i, 1), style, out newLetters[i]))
						return false;
				}
				break;
			case PasswordStyles.Binary:
				for (int i = 0; i < length; i++) {
					if (!TryParseLetter(s.Substring(i * 4, 4), style, out newLetters[i]))
						return false;
				}
				break;
			default:
				return false;
			}
			letters = newLetters;
			return true;
		}
	}
}
