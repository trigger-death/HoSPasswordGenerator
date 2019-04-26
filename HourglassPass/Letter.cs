using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using HourglassPass.Internal;

namespace HourglassPass {
	/// <summary>
	///  A single letter in a password structure. A letter contains both a 4-bit value, and the option of representing
	///  zero with Garbage characters.
	/// </summary>
	[Serializable]
	public struct Letter : IFormattable, IEquatable<Letter>, IEquatable<char>, IEquatable<int>,
		IComparable, IComparable<Letter>, IComparable<char>, IComparable<int>
	{
		#region Constants

		/// <summary>
		///  Used for letter randomization.
		/// </summary>
		private static readonly Random random = new Random();

		/// <summary>
		///  The minimum value representable by a single letter.
		/// </summary>
		public const int MinValue = 0x0;
		/// <summary>
		///  The maximum value representable by a single letter.
		/// </summary>
		public const int MaxValue = 0xF;
		/// <summary>
		///  The value used when multiplying and or moduloing a password value.
		/// </summary>
		public const int ModValue = 0x10;
		/// <summary>
		///  The number of bits to check by for a letter.
		/// </summary>
		public const int ShiftValue = 4;
		/// <summary>
		///  The the mask to use when shifting letters.
		/// </summary>
		public const int MaskValue = MaxValue;

		/// <summary>
		///  The minimum characters in the range of valid letter characters.
		/// </summary>
		public const char MinChar = 'A';
		/// <summary>
		///  The maximum character in the range of valid letter characters.
		/// </summary>
		public const char MaxChar = 'Z';
		/// <summary>
		///  The default character used for garbage letters.
		/// </summary>
		public const char GarbageChar = 'B';

		/// <summary>
		///  All valid letter characters from 0 to 15.
		/// </summary>
		private static readonly char[] Valid = {
			'Z', // 0000  0   0
			'A', // 0001  1   1
			'U', // 0010  2   2
			'E', // 0011  3   3
			'C', // 0100  4   4
			'G', // 0101  5   5
			'W', // 0110  6   6
			'J', // 0111  7   7
			'L', // 1000  8   8
			'H', // 1001  9   9
			'N', // 1010  A  10
			'P', // 1011  B  11
			'R', // 1100  C  12
			'T', // 1101  D  13
			'O', // 1110  E  14
			'X', // 1111  F  15
		};
		/// <summary>
		///  All garbage letter characters that resolve to zero.
		/// </summary>
		private static readonly char[] Garbage = {
			'B', // 1 XXXX
			'D', // 1 XXXX
			'F', // 1 XXXX
			'I', // 1 XXXX
			'K', // 1 XXXX
			'M', // 1 XXXX
			'Q', // 1 XXXX
			'S', // 1 XXXX
			'V', // 1 XXXX
			'Y', // 1 XXXX
		};

		/// <summary>
		///  A letter representing the minimum letter value.
		/// </summary>
		public static readonly Letter MinLetter = new Letter(MinValue);
		/// <summary>
		///  A letter representing the maximum letter value.
		/// </summary>
		public static readonly Letter MaxLetter = new Letter(MaxValue);

		/// <summary>
		///  Gets all valid letter characters from 0 to 15.
		/// </summary>
		public static char[] ValidCharacters {
			get {
				char[] valid = new char[Valid.Length];
				Array.Copy(Valid, valid, valid.Length);
				return valid;
			}
		}
		/// <summary>
		///  Gets all garbage letter characters that resolve to zero.
		/// </summary>
		public static char[] GarbageCharacters {
			get {
				char[] garbage = new char[Garbage.Length];
				Array.Copy(Garbage, garbage, garbage.Length);
				return garbage;
			}
		}

		#endregion

		#region Fields

		/// <summary>
		///  The actual character being displayed. If '\0', then the default character of 'Z' or 'B' is returned based
		///  on <see cref="allowGarbage"/>.
		/// </summary>
		private char character;
		/// <summary>
		///  True if the <see cref="character"/> can use garbage characters in place of zero 'Z'.
		/// </summary>
		private bool allowGarbage;

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a letter from a character and assigns if it's a garbage character based on its type.
		/// </summary>
		/// <param name="character">The character for the letter.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="character"/> is not in <see cref="ValidCharacters"/> or <see cref="GarbageCharacters"/>.
		/// </exception>
		public Letter(char character) {
			ValidateChar(ref character, nameof(character));
			this.character = character;
			allowGarbage = IsGarbageChar(character);
		}
		/// <summary>
		///  Constructs a letter from a character and assigns if it's a garbage character based on its type.
		/// </summary>
		/// <param name="character">The character for the letter.</param>
		/// <param name="allowGarbage">True if zero can be represented with garbage characters.</param>
		/// 
		/// <remarks>
		///  If <paramref name="character"/> and <paramref name="allowGarbage"/> do not co-operate,
		///  <paramref name="character"/> will be converted.
		/// </remarks>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="character"/> is not in <see cref="ValidCharacters"/> or <see cref="GarbageCharacters"/>.
		/// </exception>
		public Letter(char character, bool allowGarbage) {
			ValidateChar(ref character, nameof(character));
			EnsureGarbageChar(ref character, allowGarbage);
			this.character = character;
			this.allowGarbage = allowGarbage;
		}
		/// <summary>
		///  Constructs a letter from a value between <see cref="MinValue"/> and <see cref="MaxValue"/> and sets
		///  <see cref="AllowGarbage"/> to false.
		/// </summary>
		/// <param name="value">The numeric value for the letter.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public Letter(int value) {
			ValidateValue(value, nameof(value));
			character = Valid[value];
			allowGarbage = false;
		}
		/// <summary>
		///  Constructs a letter from a value between <see cref="MinValue"/> and <see cref="MaxValue"/>.
		/// </summary>
		/// <param name="value">The numeric value for the letter.</param>
		/// <param name="allowGarbage">True if zero can be represented with garbage characters.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public Letter(int value, bool allowGarbage) {
			ValidateValue(value, nameof(value));
			// Garbage characters can be 'Z'.
			/*if (allowGarbage && value == 0)
				character = Garbage[0];
			else*/
			character = Valid[value];
			this.allowGarbage = allowGarbage;
		}

		#endregion

		#region Properties

		/// <summary>
		///  Gets or sets the character assigned to this letter.
		/// </summary>
		/// 
		/// <remarks>
		///  If <see cref="Character"/> and <see cref="AllowGarbage"/> do not co-operate,
		///  <see cref="Character"/> will be converted.
		/// </remarks>
		/// 
		/// <exception cref="ArgumentException">
		///  <see cref="Character"/> is not in <see cref="ValidCharacters"/> or <see cref="GarbageCharacters"/>.
		/// </exception>
		public char Character {
			get {
				// Handle default constructor Letters
				if (character == '\0') {
					// Garbage characters can be 'Z'.
					//if (garbage)
					//	return Garbage[0];
					//else
						return Valid[0];
				}
				return character;
			}
			set {
				ValidateChar(ref value, nameof(Character));
				EnsureGarbageChar(ref value, allowGarbage);
				character = value;
			}
		}
		/// <summary>
		///  Gets or sets the numeric value of the letter.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <see cref="Value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public int Value {
			get => GetValueOfChar(Character);
			set {
				ValidateValue(value, nameof(Value));
				if (Value != value) {
					// Garbage characters can be 'Z'.
					character = Valid[value];
				}
			}
		}

		/// <summary>
		///  Gets or sets if the letter is represented as garbage letter when zero.
		/// </summary>
		/// 
		/// <remarks>
		///  If <see cref="Character"/> and <see cref="AllowGarbage"/> do not co-operate,
		///  <see cref="Character"/> will be converted.
		/// </remarks>
		public bool AllowGarbage {
			get => allowGarbage;
			set {
				allowGarbage = value;
				// Garbage characters can be 'Z'.
				//if (value && (character == Valid[0] || character == '\0'))
				//	character = Garbage[0];
				if (!value && IsGarbage)
					character = Valid[0];
			}
		}

		/// <summary>
		///  Gets or sets if the letter is currently zero, and being represented by a garbage letter.
		/// </summary>
		public bool IsGarbage {
			get => IsGarbageChar(character);
			set {
				if (Value != 0)
					throw new InvalidOperationException(
						$"Cannot assign to {nameof(IsGarbage)} if {nameof(Value)} is non-zero!");
				if (value)
					character = Garbage[0];
				else
					character = Valid[0];
			}
		}

		#endregion

		#region Object Overrides

		/// <summary>
		///  Gets the string representation of the letter.
		/// </summary>
		/// <returns>The string representation of the letter.</returns>
		public override string ToString() => new string(Character, 1);

		/// <summary>
		///  Gets the string representation of the letter with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter in.<para/>
		///  S/s = Default, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.
		/// </param>
		/// <returns>The formatted string representation of the letter.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);
		/// <summary>
		///  Gets the string representation of the letter with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter in.<para/>
		///  S/s = Default, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the letter.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		public string ToString(string format, IFormatProvider formatProvider) => this.Format(format, formatProvider);

		/// <summary>
		///  Gets the hash code as the letter's value.
		/// </summary>
		/// <returns>The letter's value.</returns>
		public override int GetHashCode() => Value;

		/// <summary>
		///  Checks if the object is a <see cref="Letter"/>, <see cref="char"/>, or <see cref="int"/> and checks for
		///  equality between the values of the letters.
		/// </summary>
		/// <param name="obj">The object to check for equality with.</param>
		/// <returns>The object is a compatible type and has the same value as this letter.</returns>
		public override bool Equals(object obj) {
			if (obj is Letter l) return Equals(l);
			if (obj is char c) return Equals(c);
			if (obj is int i) return Equals(i);
			return false;
		}
		/// <summary>
		///  Checks for equality between the values of the letters.
		/// </summary>
		/// <param name="other">The letter to check for equality with.</param>
		/// <returns>The letter has the same value as this letter.</returns>
		public bool Equals(Letter other) => Value == other.Value;
		/// <summary>
		///  Checks for equality between the value of the letter to that of the character.
		/// </summary>
		/// <param name="other">The character to check for equality with values.</param>
		/// <returns>The character has the same value as this letter.</returns>
		public bool Equals(char other) => Value == GetValueOfChar(other);
		/// <summary>
		///  Checks for equality between the value with that of this letter.
		/// </summary>
		/// <param name="other">The value to check for equality with.</param>
		/// <returns>The value is the same as this letter's value.</returns>
		public bool Equals(int other) => Value == other;

		/// <summary>
		///  Checks if the object is a <see cref="Letter"/>, <see cref="char"/>, or <see cref="int"/> and compares the
		///  values.
		/// </summary>
		/// <param name="obj">The object to compare values with.</param>
		/// <returns>The comparison of the two objects.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="obj"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="obj"/> is not a <see cref="Letter"/>, <see cref="char"/>, or <see cref="int"/>.
		/// </exception>
		public int CompareTo(object obj) {
			if (obj is Letter l) return CompareTo(l);
			if (obj is char c) return CompareTo(c);
			if (obj is int i) return CompareTo(i);
			if (obj is null)
				throw new ArgumentNullException(nameof(obj));
			throw new ArgumentException($"Letter cannot be compared against type {obj.GetType().Name}!");
		}
		/// <summary>
		///  Compares the values of the letters.
		/// </summary>
		/// <param name="obj">The letter to compare with.</param>
		/// <returns>The comparison of the two letters.</returns>
		public int CompareTo(Letter other) => Value.CompareTo(other.Value);
		/// <summary>
		///  Compares the values of the letter and character.
		/// </summary>
		/// <param name="obj">The character to compare with.</param>
		/// <returns>The comparison of the letter and character.</returns>
		public int CompareTo(char other) => Value.CompareTo(GetValueOfChar(other));
		/// <summary>
		///  Compares the value with the letter's value.
		/// </summary>
		/// <param name="obj">The value to compare with.</param>
		/// <returns>The comparison of the letter and value.</returns>
		public int CompareTo(int other) => Value.CompareTo(other);

		#endregion

		#region Parse

		/// <summary>
		///  Parses the string representation of the letter.
		/// </summary>
		/// <param name="s">The string representation of the letter.</param>
		/// <returns>The parsed letter.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid letter.
		/// </exception>
		public static Letter Parse(string s) {
			return LetterUtils.ParseLetter(s, PasswordStyles.PasswordOrValue);
		}
		/// <summary>
		///  Parses the string representation of the letter.
		/// </summary>
		/// <param name="s">The string representation of the letter.</param>
		/// <param name="style">The style to parse the letter in.</param>
		/// <returns>The parsed letter.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid letter.-or-<paramref name="style"/> is not a valid
		///  <see cref="PasswordStyles"/>.
		/// </exception>
		public static Letter Parse(string s, PasswordStyles style) {
			return LetterUtils.ParseLetter(s, style);
		}

		/// <summary>
		///  Tries to parse the string representation of the letter.
		/// </summary>
		/// <param name="s">The string representation of the letter.</param>
		/// <param name="letter">The output letter on success.</param>
		/// <returns>True if the letter was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, out Letter letter) {
			return LetterUtils.TryParseLetter(s, PasswordStyles.PasswordOrValue, out letter);
		}
		/// <summary>
		///  Tries to parse the string representation of the letter.
		/// </summary>
		/// <param name="s">The string representation of the letter.</param>
		/// <param name="style">The style to parse the letter in.</param>
		/// <param name="letter">The output letter on success.</param>
		/// <returns>True if the letter was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, PasswordStyles style, out Letter letter) {
			return LetterUtils.TryParseLetter(s, style, out letter);
		}

		#endregion

		#region Mutate

		/// <summary>
		///  Returns a normalized version of the letter if it's a garbage letter.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		/// <returns>The normalized letter with only one garbage character type.</returns>
		public Letter Normalized(char garbageChar = GarbageChar) {
			if (allowGarbage) {
				if (!IsGarbageChar(garbageChar))
					throw new ArgumentException($"'{garbageChar}' is not a garbage character!", nameof(garbageChar));
				// Normalize garbage characters to zero 'Z'.
				if (IsGarbageChar(character))
					character = Valid[0];
			}
			return this;
		}
		/// <summary>
		///  Returns a letter with a randomized garbage character, if currently using one.
		/// </summary>
		/// <returns>The randomized letter.</returns>
		public Letter Randomized() {
			if (allowGarbage && Value == 0) {
				// Randomizing should always return a garbage character, because that's how it's done.
				return new Letter(Garbage[random.Next(Garbage.Length)], true);
			}
			return this;
		}

		/// <summary>
		///  Normalizes the letter if it's a garbage letter.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		public void Normalize(char garbageChar = GarbageChar) {
			if (allowGarbage) {
				if (!IsGarbageChar(garbageChar))
					throw new ArgumentException($"'{garbageChar}' is not a garbage character!", nameof(garbageChar));
				// Normalize garbage characters to zero 'Z'.
				if (IsGarbageChar(character))
					character = Valid[0];
			}
		}
		/// <summary>
		///  Randomizes the letter's garbage character, if currently using one.
		/// </summary>
		public void Randomize() {
			if (allowGarbage && Value == 0) {
				// Randomizing should always return a garbage character, because that's how it's done.
				character = Garbage[random.Next(Garbage.Length)];
			}
		}

		#endregion

		#region Comparison Operators

		public static bool operator ==(Letter a, Letter b) => a.Equals(b);
		public static bool operator !=(Letter a, Letter b) => !a.Equals(b);

		public static bool operator <(Letter a, Letter b) => a.CompareTo(b) < 0;
		public static bool operator >(Letter a, Letter b) => a.CompareTo(b) > 0;

		public static bool operator <=(Letter a, Letter b) => a.CompareTo(b) <= 0;
		public static bool operator >=(Letter a, Letter b) => a.CompareTo(b) >= 0;

		#endregion

		#region Casting

		public static explicit operator Letter(char c) => new Letter(c);
		public static explicit operator Letter(int v) => new Letter(v);

		public static explicit operator char(Letter l) => l.Character;
		public static explicit operator int(Letter l) => l.Value;

		#endregion

		#region Helpers

		/// <summary>
		///  Returns true if the string is a valid letter string.
		/// </summary>
		/// <param name="s">The string to validate.</param>
		/// <returns>True if the string is a valid letter string.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidString(string s) {
			if (s == null)
				throw new ArgumentNullException(nameof(s));
			for (int i = 0; i < s.Length; i++) {
				char c = s[i];
				if (!IsValidChar(c))
					return false;
			}
			return true;
		}
		/// <summary>
		///  Returns true if the string is a valid letter string with the correct length.
		/// </summary>
		/// <param name="s">The string to validate.</param>
		/// <param name="length">The length to check for.</param>
		/// <returns>True if the string is a valid letter string with the correct length.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidString(string s, int length) {
			if (s == null)
				throw new ArgumentNullException(nameof(s));
			if (s.Length != length)
				return false;
			for (int i = 0; i < length; i++) {
				char c = s[i];
				if (!IsValidChar(c))
					return false;
			}
			return true;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateChar(ref char c, string paramName) {
			c = char.ToUpper(c);
			if (c < MinChar || c > MaxChar)
				throw new ArgumentOutOfRangeException(paramName, c,
					$"Letter must be between '{MinChar}' and '{MaxChar}', got '{c}'!");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateValue(int v, string paramName) {
			if (v < MinValue || v > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, v,
					$"Letter value must be between {MinValue} and {MaxValue}, got {v}!");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidChar(char c) {
			c = char.ToUpper(c);
			return (c >= MinChar && c <= MaxChar);
		}
		/*[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidChar(ref char c) {
			c = char.ToUpper(c);
			return (c >= MinChar && c <= MaxChar);
		}*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetValueOfChar(char c) {
			c = char.ToUpper(c);
			if (IsValidChar(c))
				// If outside of valid array, must be garbage letter, which evaluates to zero.
				return Math.Max(0, Array.IndexOf(Valid, c));
			return -1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGarbageValueChar(char c) {
			return c == Valid[0] || Array.IndexOf(Garbage, c) != -1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsGarbageChar(char c) {
			return Array.IndexOf(Garbage, c) != -1;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void EnsureGarbageChar(ref char c, bool allowGarbage) {
			// Garbage characters can be 'Z'.
			//if (garbage && c == Valid[0])
			//	c = Garbage[0];
			if (!allowGarbage && IsGarbageChar(c))
				c = Valid[0];
		}

		#endregion
	}
}
