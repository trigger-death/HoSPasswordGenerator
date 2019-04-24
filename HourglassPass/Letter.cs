using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
		///  on <see cref="garbage"/>.
		/// </summary>
		private char character;
		/// <summary>
		///  True if the <see cref="character"/> should use garbage characters in place of zero 'Z'.
		/// </summary>
		private bool garbage;

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
			garbage = IsGarbageChar(character);
		}
		/// <summary>
		///  Constructs a letter from a character and assigns if it's a garbage character based on its type.
		/// </summary>
		/// <param name="character">The character for the letter.</param>
		/// <param name="garbage">True if zero should be represented with garbage characters.</param>
		/// 
		/// <remarks>
		///  If <paramref name="character"/> and <paramref name="garbage"/> do not co-operate,
		///  <paramref name="character"/> will be converted.
		/// </remarks>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="character"/> is not in <see cref="ValidCharacters"/> or <see cref="GarbageCharacters"/>.
		/// </exception>
		public Letter(char character, bool garbage) {
			ValidateChar(ref character, nameof(character));
			EnsureGarbageChar(ref character, garbage);
			this.character = character;
			this.garbage = garbage;
		}
		/// <summary>
		///  Constructs a letter from a value between <see cref="MinValue"/> and <see cref="MaxValue"/> and sets
		///  <see cref="IsGarbage"/> to false.
		/// </summary>
		/// <param name="value">The numeric value for the letter.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public Letter(int value) {
			ValidateValue(value, nameof(value));
			character = Valid[value];
			garbage = false;
		}
		/// <summary>
		///  Constructs a letter from a value between <see cref="MinValue"/> and <see cref="MaxValue"/>.
		/// </summary>
		/// <param name="value">The numeric value for the letter.</param>
		/// <param name="garbage">True if zero should be represented with garbage characters.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public Letter(int value, bool garbage) {
			ValidateValue(value, nameof(value));
			if (garbage && value == 0)
				character = Garbage[0];
			else
				character = Valid[value];
			this.garbage = garbage;
		}

		#endregion

		#region Properties

		/// <summary>
		///  Gets or sets the character assigned to this letter.
		/// </summary>
		/// 
		/// <remarks>
		///  If <see cref="Character"/> and <see cref="IsGarbage"/> do not co-operate,
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
					if (garbage)
						return Garbage[0];
					else
						return Valid[0];
				}
				return character;
			}
			set {
				ValidateChar(ref value, nameof(Character));
				EnsureGarbageChar(ref value, garbage);
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
				if (garbage && value == 0)
					character = Garbage[0];
				else
					character = Valid[value];
			}
		}

		/// <summary>
		///  Gets or sets if the letter is represented as garbage letter when zero.
		/// </summary>
		/// 
		/// <remarks>
		///  If <see cref="Character"/> and <see cref="IsGarbage"/> do not co-operate,
		///  <see cref="Character"/> will be converted.
		/// </remarks>
		public bool IsGarbage {
			get => garbage;
			set {
				garbage = value;
				if (value && (character == Valid[0] || character == '\0'))
					character = Garbage[0];
				if (!value && IsGarbageChar(character))
					character = Valid[0];
			}
		}

		#endregion

		#region Utilities
		
		/// <summary>
		///  Returns a normalized version of the letter if it's a garbage letter.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		/// <returns>The normalized letter with only one garbage character type.</returns>
		public Letter Normalized(char garbageChar = GarbageChar) {
			if (garbage) {
				if (!IsGarbageChar(garbageChar))
					throw new ArgumentException($"'{garbageChar}' is not a garbage character!", nameof(garbageChar));
				if (IsGarbageChar(character))
					return new Letter(garbageChar, true);
			}
			return this;
		}
		/// <summary>
		///  Normalizes the letter if it's a garbage letter.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		public void Normalize(char garbageChar = GarbageChar) {
			if (garbage) {
				if (!IsGarbageChar(garbageChar))
					throw new ArgumentException($"'{garbageChar}' is not a garbage character!", nameof(garbageChar));
				if (IsGarbageChar(character))
					character = garbageChar;
			}
		}
		/// <summary>
		///  Returns a letter with a randomized garbage character, if currently using one.
		/// </summary>
		/// <returns>The randomized letter.</returns>
		public Letter Randomized() {
			if (garbage) {
				if (IsGarbageChar(character))
					return new Letter(Garbage[random.Next(Garbage.Length)], true);
			}
			return this;
		}
		/// <summary>
		///  Randomized the letter's garbage character, if currently using one.
		/// </summary>
		public void Randomize() {
			if (garbage) {
				if (IsGarbageChar(character))
					character = Garbage[random.Next(Garbage.Length)];
			}
		}

		#endregion

		#region Object Overrides

		/// <summary>
		///  Gets the string representation of the letter.
		/// </summary>
		/// <returns>The string representation of the letter.</returns>
		public override string ToString() => new string(Character, 1);
		/*/// <summary>
		///  Gets the string representation of the letter in binary.
		/// </summary>
		/// <returns>The binary string representation of the letter.</returns>
		public string ToBinString() => Convert.ToString(Value, 2).PadLeft(4, '0');
		/// <summary>
		///  Gets the string representation of the letter in hexidecimal.
		/// </summary>
		/// <returns>The hexidecimal string representation of the letter.</returns>
		public string ToHexString() => Convert.ToString(Value, 16).ToUpper();*/

		/// <summary>
		///  Gets the string representation of the letter with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter in.<para/>
		///  S/empty = String, B = Binary, D/N = Decimal, X = Hexidecimal.
		/// </param>
		/// <returns>The formatted string representation of the letter.</returns>
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);
		/// <summary>
		///  Gets the string representation of the letter with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter in.<para/>
		///  S/empty = String, B = Binary, D/N = Decimal, X = Hexidecimal.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the letter.</returns>
		public string ToString(string format, IFormatProvider formatProvider) {
			if (format == null)
				return ToString();
			switch (format) {
			case "":
			case "S": return ToString();
			case "B": return Convert.ToString(Value, 2).PadLeft(4, '0');
			case "X": return Convert.ToString(Value, 16).ToUpper();
			case "N": // We can't get large enough to use commas, ignore.
			case "D": return Value.ToString();
			}
			throw new FormatException($"Invalid Letter format \"{format}\"!");
		}

		/// <summary>
		///  Gets the hash code for the letter's value.
		/// </summary>
		/// <returns>The letter's value.</returns>
		public override int GetHashCode() => Value;

		/// <summary>
		///  Checks if the object is a <see cref="Letter"/>, <see cref="char"/>, or <see cref="int"/> and compares the
		///  values of the letters.
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
		///  Compares the values of the letters.
		/// </summary>
		/// <param name="other">The letter to check for equality with.</param>
		/// <returns>The letter has the same value as this letter.</returns>
		public bool Equals(Letter other) => Value == other.Value;
		/// <summary>
		///  Compares the value of the letter to that of the character.
		/// </summary>
		/// <param name="other">The character to check for equality with values.</param>
		/// <returns>The character has the same value as this letter.</returns>
		public bool Equals(char other) => Value == GetValueOfChar(other);
		/// <summary>
		///  Compares the value with that of this letter.
		/// </summary>
		/// <param name="other">The value to check for equality with.</param>
		/// <returns>The value haiss the same as this letter's value.</returns>
		public bool Equals(int other) => Value == other;

		public int CompareTo(object obj) {
			if (obj is Letter l) return CompareTo(l);
			if (obj is char c) return CompareTo(c);
			if (obj is int i) return CompareTo(i);
			throw new ArgumentException($"Letter cannot be compared against type {obj.GetType().Name}!");
		}
		public int CompareTo(Letter other) => Value.CompareTo(other.Value);
		public int CompareTo(char other) => Value.CompareTo(GetValueOfChar(other));
		public int CompareTo(int other) => Value.CompareTo(other);

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
		public static bool IsValidChar(ref char c) {
			c = char.ToUpper(c);
			return (c >= MinChar && c <= MaxChar);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetValueOfChar(char c) {
			if (IsValidChar(ref c))
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
		private static void EnsureGarbageChar(ref char c, bool garbage) {
			if (garbage && c == Valid[0])
				c = Garbage[0];
			if (!garbage && IsGarbageChar(c))
				c = Valid[0];
		}

		#endregion
	}
}
