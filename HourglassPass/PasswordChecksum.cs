using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using HourglassPass.Internal;

namespace HourglassPass {
	/// <summary>
	///  A single letter in a password used as a checksum to mark off all letters that are using garbage characters.
	/// </summary>
	[Serializable]
	public class PasswordChecksum : ILetterString, IEquatable<Letter>, IEquatable<char> {
		#region Constants

		/// <summary>
		///  The minimum value representable by a Garbage Checksum.
		/// </summary>
		public const int MinValue = Letter.MinValue;
		/// <summary>
		///  The maximum value representable by a Garbage Checksum.
		/// </summary>
		public const int MaxValue = Letter.MaxValue;
		/// <summary>
		///  The number of letters in this password structure.
		/// </summary>
		public const int Length = 1;

		#region ILetterString Constants

		int IReadOnlyLetterString.MinValue => Length;
		int IReadOnlyLetterString.MaxValue => Length;
		int IReadOnlyCollection<Letter>.Count => Length;

		#endregion

		#endregion

		#region Fields

		/// <summary>
		///  The single letter in the garbage checksum
		/// </summary>
		private Letter letter = new Letter(0, false);

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs an empty Garbage Checksum with no garbage letters in use.
		/// </summary>
		public PasswordChecksum() { }
		/// <summary>
		///  Constructs a Garbage Checksum with the specified letter arrays's characters.
		/// </summary>
		/// <param name="letters">The letter array whose characters are used.</param>
		public PasswordChecksum(Letter[] letters) {
			ValidateLetters(letters, nameof(letters));
			CopyFromLetters(letters);
		}
		/// <summary>
		///  Constructs a Garbage Checksum with the specified letter's character.
		/// </summary>
		/// <param name="letter">The letter whose character is used.</param>
		public PasswordChecksum(Letter letter) {
			CopyFromLetter(letter);
		}
		/// <summary>
		///  Constructs a Garbage Checksum with the specified string.
		/// </summary>
		/// <param name="character">The string's value to use.</param>
		public PasswordChecksum(string s) {
			ValidateString(ref s, nameof(s));
			CopyFromString(s);
		}
		/// <summary>
		///  Constructs a Garbage Checksum with the specified character.
		/// </summary>
		/// <param name="character">The character's value to use.</param>
		public PasswordChecksum(char character) {
			ValidateChar(ref character, nameof(character));
			CopyFromChar(character);
		}
		/// <summary>
		///  Constructs a Garbage Checksum with the specified numiric value.
		/// </summary>
		/// <param name="value">The numiric value to use.</param>
		public PasswordChecksum(int value) {
			ValidateValue(value, nameof(value));
			CopyFromValue(value);
		}
		/// <summary>
		///  Constructs a copy of the Garbage Checksum.
		/// </summary>
		/// <param name="checksum">The Garbage Checksum to construct a copy of.</param>
		public PasswordChecksum(PasswordChecksum checksum) {
			if (checksum == null)
				throw new ArgumentNullException(nameof(checksum));
			CopyFromLetter(checksum.letter);
		}

		#endregion

		#region Properties

		/// <summary>
		///  Gets or sets the single letter in the Garbage Checksum.
		/// </summary>
		public Letter Letter {
			get => letter;
			set => CopyFromLetter(letter);
		}
		/// <summary>
		///  Gets or sets the single character in the Garbage Checksum.
		/// </summary>
		public char Character {
			get => letter.Character;
			set {
				ValidateChar(ref value, nameof(Character));
				CopyFromChar(value);
			}
		}

		/// <summary>
		///  Gets or sets the letter at the specified index in the Password.
		/// </summary>
		/// <param name="index">The index of the letter.</param>
		/// <returns>The letter at the specified index in the Password.</returns>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Length"/>.
		/// </exception>
		public Letter this[int index] {
			get {
				if (index != 0)
					throw new ArgumentOutOfRangeException(nameof(index), index,
						$"Index must be 0, got {index}!");
				return letter;
			}
			set {
				if (index != 0)
					throw new ArgumentOutOfRangeException(nameof(index), index,
						$"Index must be 0, got {index}!");
				letter.Character = value.Character;
			}
		}
		/// <summary>
		///  Gets or sets the Password with an array of <see cref="Length"/> letters.
		/// </summary>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <see cref="Letters"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <see cref="Letters"/> is not <see cref="Length"/>.
		/// </exception>
		public Letter[] Letters {
			get => new[] { letter };
			set {
				ValidateLetters(value, nameof(Letters));
				CopyFromLetters(value);
			}
		}
		/// <summary>
		///  Gets or sets the Password with a string of <see cref="Length"/> letters.
		/// </summary>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <see cref="String"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <see cref="String"/> is not <see cref="Length"/>.-or- A character in <see cref="String"/> is
		///  not a valid letter character.
		/// </exception>
		public string String {
			get => letter.ToString();
			set {
				ValidateString(ref value, nameof(String));
				CopyFromString(value);
			}
		}
		/// <summary>
		///  Gets or sets the Password with a numeric value.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <see cref="Value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public int Value {
			get => letter.Value;
			set {
				ValidateValue(value, nameof(Value));
				CopyFromValue(value);
			}
		}

		#endregion

		#region Object Overrides

		/// <summary>
		///  Gets the string representation of the Garbage Checksum.
		/// </summary>
		/// <returns>The string representation of the Garbage Checksum.</returns>
		public override string ToString() => String;

		/// <summary>
		///  Gets the string representation of the Garbage Checksum with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, C/c = Corrected, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <returns>The formatted string representation of the Garbage Checksum.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);
		/// <summary>
		///  Gets the string representation of the Garbage Checksum with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, C/c = Corrected, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the Garbage Checksum.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		public string ToString(string format, IFormatProvider formatProvider) {
			return this.Format(format, formatProvider, "Garbage Checksum");
		}

		/// <summary>
		///  Gets the hash code as the Garbage Checksum's value.
		/// </summary>
		/// <returns>The Garbage Checksum's value.</returns>
		public override int GetHashCode() => Value;

		/// <summary>
		///  Checks if the object is a <see cref="PasswordChecksum"/>, <see cref="HourglassPass.Letter"/>[],
		///  <see cref="HourglassPass.Letter"/>, <see cref="string"/>, <see cref="char"/> or <see cref="int"/> and
		///  Checks for equality between the values of the letter strings.
		/// </summary>
		/// <param name="obj">The object to check for equality with.</param>
		/// <returns>The object is a compatible type and has the same value as this letter string.</returns>
		public override bool Equals(object obj) {
			if (ReferenceEquals(this, obj)) return true;
			if (obj is PasswordChecksum gc) return Equals(gc);
			if (obj is Letter[] la) return Equals(la);
			if (obj is Letter l) return Equals(l);
			if (obj is string s) return Equals(s);
			if (obj is char c) return Equals(c);
			if (obj is int i) return Equals(i);
			return false;
		}
		/// <summary>
		///  Checks for equality between the values of the letter strings.
		/// </summary>
		/// <param name="other">The letter string to check for equality with.</param>
		/// <returns>The letter string has the same value as this letter string.</returns>
		public bool Equals(PasswordChecksum other) => other != null && Value == other.Value;
		/// <summary>
		///  Checks for equality between the value of the letter string and that of the letter array.
		/// </summary>
		/// <param name="other">The letter array to check for equality with values.</param>
		/// <returns>The letter array has the same value as this letter string.</returns>
		public bool Equals(Letter[] other) => other != null && Value == new PasswordChecksum(other).Value;
		/// <summary>
		///  Checks for equality between the value of the letter string and that of the letter.
		/// </summary>
		/// <param name="other">The letter to check for equality with values.</param>
		/// <returns>The letter has the same value as this letter string.</returns>
		public bool Equals(Letter other) => Value == other.Value;
		/// <summary>
		///  Checks for equality between the value of the letter string and that of the string.
		/// </summary>
		/// <param name="other">The string to check for equality with values.</param>
		/// <returns>The string has the same value as this letter string.</returns>
		public bool Equals(string other) => other != null && Value == new PasswordChecksum(other).Value;
		/// <summary>
		///  Checks for equality between the value of the letter string and that of the character.
		/// </summary>
		/// <param name="other">The character to check for equality with values.</param>
		/// <returns>The character has the same value as this letter string.</returns>
		public bool Equals(char other) => Value == new PasswordChecksum(other).Value;
		/// <summary>
		///  Compares the value with that of this letter string.
		/// </summary>
		/// <param name="other">The value to check for equality with.</param>
		/// <returns>The value is the same as this letter string's value.</returns>
		public bool Equals(int other) => Value == other;

		#endregion

		#region Parse

		/// <summary>
		///  Parses the string representation of the Garbage Checksum.
		/// </summary>
		/// <param name="s">The string representation of the Garbage Checksum.</param>
		/// <returns>The parsed Garbage Checksum.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid Garbage Checksum.
		/// </exception>
		public static PasswordChecksum Parse(string s) {
			return Parse(s, PasswordStyles.PasswordOrValue);
		}
		/// <summary>
		///  Parses the string representation of the Garbage Checksum.
		/// </summary>
		/// <param name="s">The string representation of the Garbage Checksum.</param>
		/// <param name="style">The style to parse the Garbage Checksum in.</param>
		/// <returns>The parsed Garbage Checksum.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid Garbage Checksum.-or-<paramref name="style"/> is not a valid
		///  <see cref="PasswordStyles"/>.
		/// </exception>
		public static PasswordChecksum Parse(string s, PasswordStyles style) {
			Letter[] letters = LetterUtils.ParseLetterString(s, style, "Garbage Checksum", Length, out int value);
			return (letters != null ? new PasswordChecksum(letters) : new PasswordChecksum(value));
		}
		/// <summary>
		///  Parses the string representation of the Garbage Checksum's value.
		/// </summary>
		/// <param name="s">The string representation of the Garbage Checksum.</param>
		/// <param name="style">The style to parse the Garbage Checksum's value in.</param>
		/// <returns>The parsed Garbage Checksum.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid Garbage Checksum.-or-<paramref name="style"/> is not a valid
		///  <see cref="NumberStyles"/>.
		/// </exception>
		/// <exception cref="FormatException">
		///  <paramref name="s"/> does not follow the number format.
		/// </exception>
		public static PasswordChecksum Parse(string s, NumberStyles style) {
			return new PasswordChecksum(int.Parse(s, style));
		}

		/// <summary>
		///  Tries to parse the string representation of the Garbage Checksum.
		/// </summary>
		/// <param name="s">The string representation of the Garbage Checksum.</param>
		/// <param name="checksum">The output Garbage Checksum on success.</param>
		/// <returns>True if the Garbage Checksum was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, out PasswordChecksum checksum) {
			return TryParse(s, PasswordStyles.PasswordOrValue, out checksum);
		}
		/// <summary>
		///  Tries to parse the string representation of the Garbage Checksum.
		/// </summary>
		/// <param name="s">The string representation of the Garbage Checksum.</param>
		/// <param name="style">The style to parse the Garbage Checksum in.</param>
		/// <param name="checksum">The output Garbage Checksum on success.</param>
		/// <returns>True if the Garbage Checksum was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, PasswordStyles style, out PasswordChecksum checksum) {
			if (LetterUtils.TryParseLetterString(s, style, "Garbage Checksum", Length, out Letter[] letters, out int value)) {
				checksum = (letters != null ? new PasswordChecksum(letters) : new PasswordChecksum(value));
				return true;
			}
			checksum = null;
			return false;
		}
		/// <summary>
		///  Tries to parse the string representation of the Garbage Checksum's value.
		/// </summary>
		/// <param name="s">The string representation of the Garbage Checksum's value.</param>
		/// <param name="style">The style to parse the Garbage Checksum's value in.</param>
		/// <param name="checksum">The output Garbage Checksum on success.</param>
		/// <returns>True if the Garbage Checksum was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, NumberStyles style, out PasswordChecksum checksum) {
			if (int.TryParse(s, style, CultureInfo.CurrentCulture, out int value)) {
				checksum = new PasswordChecksum(value);
				return true;
			}
			checksum = null;
			return false;
		}

		#endregion

		#region ILetterString Mutate

		/// <summary>
		///  Returns a Garbage Checksum with normalized interchangeable characters.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		/// <returns>The normalized Garbage Checksum with consistent interchangeable characters.</returns>
		public PasswordChecksum Normalized(char garbageChar = Letter.GarbageChar) => this;
		/// <summary>
		///  Returns a Garbage Checksum with randomized interchangeable characters.
		/// </summary>
		/// <returns>The randomized Garbage Checksum with random interchangable characters.</returns>
		public PasswordChecksum Randomized() => this;
		ILetterString ILetterString.Normalized(char garbageChar) => Normalized(garbageChar);
		ILetterString ILetterString.Randomized() => Randomized();
		IReadOnlyLetterString IReadOnlyLetterString.Normalized(char garbageChar) => Normalized(garbageChar);
		IReadOnlyLetterString IReadOnlyLetterString.Randomized() => Randomized();

		/// <summary>
		///  Normalizes the Garbage Checksum's interchangeable characters.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		public void Normalize(char garbageChar = Letter.GarbageChar) { /* Do nothing */ }
		/// <summary>
		///  Randomizes the Garbage Checksum's interchangeable characters.
		/// </summary>
		public void Randomize() { /* Do nothing */ }

		#endregion

		#region IEnumerable Implementation

		/// <summary>
		///  Gets the enumerator for the letters in the Garbage Checksum.
		/// </summary>
		/// <returns>An enumerator to traverse the letters in the Garbage Checksum.</returns>
		public IEnumerator<Letter> GetEnumerator() {
			yield return letter;
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region Comparison Operators

		public static bool operator ==(PasswordChecksum a, PasswordChecksum b) => a.Equals(b);
		public static bool operator !=(PasswordChecksum a, PasswordChecksum b) => !a.Equals(b);

		#endregion

		#region Casting

		public static explicit operator PasswordChecksum(string s) => new PasswordChecksum(s);
		public static explicit operator PasswordChecksum(char c) => new PasswordChecksum(c);
		public static explicit operator PasswordChecksum(int v) => new PasswordChecksum(v);

		public static explicit operator string(PasswordChecksum l) => l.String;
		public static explicit operator char(PasswordChecksum l) => l.Character;
		public static explicit operator int(PasswordChecksum l) => l.Value;

		#endregion

		#region Helpers

		/// <summary>
		///  Returns true if the string is a valid Garbage Checksum letter string.
		/// </summary>
		/// <param name="s">The string to validate.</param>
		/// <returns>True if the string is a valid Garbage Checksum letter string.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidString(string s) => Letter.IsValidString(s, Length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateLetters(Letter[] letters, string paramName) {
			if (letters == null)
				throw new ArgumentNullException(paramName);
			if (letters.Length != Length)
				throw new ArgumentException($"Garbage Checksum letters must be {Length} letter long, got {letters.Length} letters!",
					paramName);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateString(ref string s, string paramName) {
			if (s == null)
				throw new ArgumentNullException(paramName);
			if (s.Length != Length)
				throw new ArgumentException($"Garbage Checksum string must be {Length} letter long, got {s.Length} letters!",
					paramName);
			s = s.ToUpper();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateChar(ref char c, string paramName) {
			c = char.ToUpper(c);
			if (c < Letter.MinChar || c > Letter.MaxChar)
				throw new ArgumentOutOfRangeException(paramName, c,
					$"Garbage Checksum character must be between '{Letter.MinChar}' and '{Letter.MaxChar}', got '{c}'!");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateValue(int v, string paramName) {
			if (v < MinValue || v > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, v,
					$"Garbage Checksum value must be between {MinValue} and {MaxValue}, got {v}!");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyFromLetters(Letter[] l) {
			letter.Character = l[0].Character;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyFromLetter(Letter l) {
			letter.Character = l.Character;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyFromString(string s) {
			letter.Character = s[0];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyFromChar(char c) {
			letter.Character = c;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyFromValue(int v) {
			letter.Value = v;
		}

		#endregion
	}
}
