using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using HourglassPass.Internal;

namespace HourglassPass {
	/// <summary>
	///  Password data for in-game flags.
	/// </summary>
	[Serializable]
	public sealed class PasswordFlagData : IEquatable<PasswordFlagData>, ILetterString {
		#region Constants

		/// <summary>
		///  The minimum value representable by Flag Data.
		/// </summary>
		public const int MinValue = 0x0000;
		/// <summary>
		///  The maximum value representable by Flag Data.
		/// </summary>
		public const int MaxValue = 0xFFFF;
		/// <summary>
		///  The number of letters in this password structure.
		/// </summary>
		public const int Length = 4;

		#region ILetterString Constants

		int IReadOnlyLetterString.MinValue => Length;
		int IReadOnlyLetterString.MaxValue => Length;
		int IReadOnlyCollection<Letter>.Count => Length;

		#endregion

		#endregion

		#region Fields

		/// <summary>
		///  The array <see cref="Length"/> letters that make up the Flag Data.
		/// </summary>
		private readonly Letter[] letters = {
			new Letter(0, false),
			new Letter(0, true),
			new Letter(0, true),
			new Letter(0, true),
		};

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Flag Data with all flags unset (zero).
		/// </summary>
		public PasswordFlagData() { }
		/// <summary>
		///  Constructs a Flag Data with an array of <see cref="Length"/> letters.
		/// </summary>
		/// <param name="letters">The array containing the letters of the Flag Data.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="letters"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <paramref name="letters"/> is not <see cref="Length"/>.
		/// </exception>
		public PasswordFlagData(Letter[] letters) {
			ValidateLetters(letters, nameof(letters));
			CopyFromLetters(letters);
		}
		/// <summary>
		///  Constructs a Flag Data with a string of <see cref="Length"/> letters.
		/// </summary>
		/// <param name="flags">The string containing the letters of the Flag Data.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="flags"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <paramref name="flags"/> is not <see cref="Length"/>.-or- A character in
		///  <paramref name="flags"/> is not a valid letter character.
		/// </exception>
		public PasswordFlagData(string flags) {
			ValidateString(ref flags, nameof(flags));
			CopyFromString(flags);
		}
		/// <summary>
		///  Constructs a Flag Data with a numeric value.
		/// </summary>
		/// <param name="value">The value of the Flag Data.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public PasswordFlagData(int value) {
			ValidateValue(value, nameof(value));
			CopyFromValue(value);
		}
		/// <summary>
		///  Constructs a copy of the Flag Data.
		/// </summary>
		/// <param name="flagData">The Flag Data to construct a copy of.</param>
		public PasswordFlagData(PasswordFlagData flagData) {
			if (flagData == null)
				throw new ArgumentNullException(nameof(flagData));
			Array.Copy(flagData.letters, letters, Length);
		}

		#endregion

		#region Properties

		/// <summary>
		///  Gets or sets the letter at the specified index in the Flag Data.
		/// </summary>
		/// <param name="index">The index of the letter.</param>
		/// <returns>The letter at the specified index in the Flag Data.</returns>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Length"/>.
		/// </exception>
		public Letter this[int index] {
			get => letters[index];
			set => Set(index, value);
		}
		/// <summary>
		///  Gets or sets the Flag Data with an array of <see cref="Length"/> letters.
		/// </summary>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <see cref="Letters"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <see cref="Letters"/> is not <see cref="Length"/>.
		/// </exception>
		public Letter[] Letters {
			get {
				Letter[] newLetters = new Letter[Length];
				Array.Copy(letters, newLetters, Length);
				return newLetters;
			}
			set {
				ValidateLetters(value, nameof(Letters));
				CopyFromLetters(value);
			}
		}
		/// <summary>
		///  Gets or sets the Flag Data with a string of <see cref="Length"/> letters.
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
			get => string.Join("", letters);
			set {
				ValidateString(ref value, nameof(String));
				CopyFromString(value);
			}
		}
		/// <summary>
		///  Gets or sets the Flag Data with a numeric value.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <see cref="Value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public int Value {
			get {
				int v = 0;
				// Values are refersed that they print nicely in hex
				for (int i = 0; i < Length; i++)
					v |= letters[i].Value << (i * 4);
				return v;
			}
			set {
				ValidateValue(value, nameof(Value));
				CopyFromValue(value);
			}
		}

		#endregion

		#region Object Overrides

		/// <summary>
		///  Gets the string representation of the Flag Data.
		/// </summary>
		/// <returns>The string representation of the Flag Data.</returns>
		public override string ToString() => String;

		/// <summary>
		///  Gets the string representation of the Flag Data with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, C/c = Corrected, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <returns>The formatted string representation of the Flag Data.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);
		/// <summary>
		///  Gets the string representation of the Flag Data with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, C/c = Corrected, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the Flag Data.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		public string ToString(string format, IFormatProvider formatProvider) {
			return this.Format(format, formatProvider, "Flag Data");
		}

		/// <summary>
		///  Gets the hash code as the Flag Data's value.
		/// </summary>
		/// <returns>The Flag Data's value.</returns>
		public override int GetHashCode() => Value;

		/// <summary>
		///  Checks if the object is a <see cref="PasswordFlagData"/>, <see cref="Letter"/>[], <see cref="string"/>, or
		///  <see cref="int"/> and Checks for equality between the values of the letter strings.
		/// </summary>
		/// <param name="obj">The object to check for equality with.</param>
		/// <returns>The object is a compatible type and has the same value as this letter string.</returns>
		public override bool Equals(object obj) {
			if (ReferenceEquals(this, obj)) return true;
			if (obj is PasswordFlagData fd) return Equals(fd);
			if (obj is Letter[] l) return Equals(l);
			if (obj is string s) return Equals(s);
			if (obj is int i) return Equals(i);
			return false;
		}
		/// <summary>
		///  Checks for equality between the values of the letter strings.
		/// </summary>
		/// <param name="other">The letter string to check for equality with.</param>
		/// <returns>The letter string has the same value as this letter string.</returns>
		public bool Equals(PasswordFlagData other) => other != null && Value == other.Value;
		/// <summary>
		///  Checks for equality between the value of the letter string and that of the letter array.
		/// </summary>
		/// <param name="other">The letter array to check for equality with values.</param>
		/// <returns>The letter array has the same value as this letter string.</returns>
		public bool Equals(Letter[] other) => other != null && Value == new PasswordFlagData(other).Value;
		/// <summary>
		///  Checks for equality between the value of the letter string and that of the string.
		/// </summary>
		/// <param name="other">The string to check for equality with values.</param>
		/// <returns>The string has the same value as this letter string.</returns>
		public bool Equals(string other) => other != null && Value == new PasswordFlagData(other).Value;
		/// <summary>
		///  Compares the value with that of this letter string.
		/// </summary>
		/// <param name="other">The value to check for equality with.</param>
		/// <returns>The value is the same as this letter string's value.</returns>
		public bool Equals(int other) => Value == other;

		#endregion

		#region Parse

		/// <summary>
		///  Parses the string representation of the Flag Data.
		/// </summary>
		/// <param name="s">The string representation of the Flag Data.</param>
		/// <returns>The parsed Flag Data.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid Flag Data.
		/// </exception>
		public static PasswordFlagData Parse(string s) {
			return Parse(s, PasswordStyles.PasswordOrValue);
		}
		/// <summary>
		///  Parses the string representation of the Flag Data.
		/// </summary>
		/// <param name="s">The string representation of the Flag Data.</param>
		/// <param name="style">The style to parse the Flag Data in.</param>
		/// <returns>The parsed Flag Data.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid Flag Data.-or-<paramref name="style"/> is not a valid
		///  <see cref="PasswordStyles"/>.
		/// </exception>
		public static PasswordFlagData Parse(string s, PasswordStyles style) {
			Letter[] letters = LetterUtils.ParseLetterString(s, style, "Flag Data", Length, out int value);
			return (letters != null ? new PasswordFlagData(letters) : new PasswordFlagData(value));
		}
		/// <summary>
		///  Parses the string representation of the Flag Data's value.
		/// </summary>
		/// <param name="s">The string representation of the Flag Data.</param>
		/// <param name="style">The style to parse the Flag Data's value in.</param>
		/// <returns>The parsed Flag Data.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="s"/> is not a valid Flag Data.-or-<paramref name="style"/> is not a valid
		///  <see cref="NumberStyles"/>.
		/// </exception>
		/// <exception cref="FormatException">
		///  <paramref name="s"/> does not follow the number format.
		/// </exception>
		public static PasswordFlagData Parse(string s, NumberStyles style) {
			return new PasswordFlagData(int.Parse(s, style));
		}

		/// <summary>
		///  Tries to parse the string representation of the Flag Data.
		/// </summary>
		/// <param name="s">The string representation of the Flag Data.</param>
		/// <param name="flagData">The output Flag Data on success.</param>
		/// <returns>True if the Flag Data was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, out PasswordFlagData flagData) {
			return TryParse(s, PasswordStyles.PasswordOrValue, out flagData);
		}
		/// <summary>
		///  Tries to parse the string representation of the Flag Data.
		/// </summary>
		/// <param name="s">The string representation of the Flag Data.</param>
		/// <param name="style">The style to parse the Flag Data in.</param>
		/// <param name="flagData">The output Flag Data on success.</param>
		/// <returns>True if the Flag Data was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, PasswordStyles style, out PasswordFlagData flagData) {
			if (LetterUtils.TryParseLetterString(s, style, "Flag Data", Length, out Letter[] letters, out int value)) {
				flagData = (letters != null ? new PasswordFlagData(letters) : new PasswordFlagData(value));
				return true;
			}
			flagData = null;
			return false;
		}
		/// <summary>
		///  Tries to parse the string representation of the Flag Data's value.
		/// </summary>
		/// <param name="s">The string representation of the Flag Data's value.</param>
		/// <param name="style">The style to parse the Flag Data's value in.</param>
		/// <param name="password">The output Flag Data on success.</param>
		/// <returns>True if the Flag Data was successfully parsed, otherwise false.</returns>
		public static bool TryParse(string s, NumberStyles style, out PasswordFlagData flagData) {
			if (int.TryParse(s, style, CultureInfo.CurrentCulture, out int value)) {
				flagData = new PasswordFlagData(value);
				return true;
			}
			flagData = null;
			return false;
		}

		#endregion

		#region Operations

		public PasswordFlagData Zero(int index) => Set(index, 0);
		public PasswordFlagData Negate(int index) => Operation(index, v => ~v);

		public PasswordFlagData Set(int index, Letter value) => Set(index, value.Value);
		public PasswordFlagData Add(int index, Letter value) => Add(index, value.Value);
		public PasswordFlagData Sub(int index, Letter value) => Sub(index, value.Value);
		public PasswordFlagData And(int index, Letter value) => And(index, value.Value);
		public PasswordFlagData Or(int index, Letter value) => Or(index, value.Value);
		public PasswordFlagData Xor(int index, Letter value) => Xor(index, value.Value);

		public PasswordFlagData Set(int index, char value) => Set(index, new Letter(value).Value);
		public PasswordFlagData Add(int index, char value) => Add(index, new Letter(value).Value);
		public PasswordFlagData Sub(int index, char value) => Sub(index, new Letter(value).Value);
		public PasswordFlagData And(int index, char value) => And(index, new Letter(value).Value);
		public PasswordFlagData Or(int index, char value) => Or(index, new Letter(value).Value);
		public PasswordFlagData Xor(int index, char value) => Xor(index, new Letter(value).Value);

		public PasswordFlagData Set(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => value);
		}
		public PasswordFlagData Add(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => Math.Min(Letter.MaxValue, v + value));
		}
		public PasswordFlagData Sub(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => Math.Max(Letter.MinValue, v - value));
		}
		public PasswordFlagData And(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => v & value);
		}
		public PasswordFlagData Or(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => v | value);
		}
		public PasswordFlagData Xor(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => v ^ value);
		}

		public PasswordFlagData Operation(FlagOperation[] fops) {
			foreach (FlagOperation fop in fops)
				Operation(fop);
			return this;
		}
		public PasswordFlagData Operation(FlagOperation fop) {
			foreach (FlagLetter op in fop.Flags) {
				switch (fop.Type) {
				case OpType.Zero: return Zero(op.Index);
				case OpType.Negate: return Negate(op.Index);

				case OpType.Set: return Set(op.Index, op.Value);
				case OpType.Add: return Add(op.Index, op.Value);
				case OpType.Sub: return Sub(op.Index, op.Value);
				case OpType.Or: return Or(op.Index, op.Value);
				case OpType.Xor: return Xor(op.Index, op.Value);
				}
			}
			return this;
		}

		private PasswordFlagData Operation(int index, Func<int, int> operation) {
			int oldValue = letters[index].Value;
			int newValue = operation(oldValue);
			if (oldValue != newValue)
				letters[index].Value = newValue;
			return this;
		}

		#endregion

		#region ILetterString Mutate

		/// <summary>
		///  Returns a Flag Data with normalized interchangeable characters.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		/// <returns>The normalized Flag Data with consistent interchangeable characters.</returns>
		public PasswordFlagData Normalized(char garbageChar = Letter.GarbageChar) {
			PasswordFlagData fd = new PasswordFlagData(this);
			fd.Normalize(garbageChar);
			return fd;
		}
		/// <summary>
		///  Returns a Flag Data with randomized interchangeable characters.
		/// </summary>
		/// <returns>The randomized Flag Data with random interchangable characters.</returns>
		public PasswordFlagData Randomized() {
			PasswordFlagData fd = new PasswordFlagData(this);
			fd.Randomize();
			return fd;
		}
		ILetterString ILetterString.Normalized(char garbageChar) => Normalized(garbageChar);
		ILetterString ILetterString.Randomized() => Randomized();
		IReadOnlyLetterString IReadOnlyLetterString.Normalized(char garbageChar) => Normalized(garbageChar);
		IReadOnlyLetterString IReadOnlyLetterString.Randomized() => Randomized();

		/// <summary>
		///  Normalizes the Flag Data's interchangeable characters.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		public void Normalize(char garbageChar = Letter.GarbageChar) {
			for (int i = 1; i < Length; i++)
				letters[i].Normalize(garbageChar);
		}
		/// <summary>
		///  Randomizes the Flag Data's interchangeable characters.
		/// </summary>
		public void Randomize() {
			for (int i = 1; i < Length; i++)
				letters[i].Randomize();
		}

		#endregion

		#region IEnumerable Implementation

		/// <summary>
		///  Gets the enumerator for the letters in the Flag Data.
		/// </summary>
		/// <returns>An enumerator to traverse the letters in the Flag Data.</returns>
		public IEnumerator<Letter> GetEnumerator() => ((IEnumerable<Letter>) letters).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => letters.GetEnumerator();

		#endregion

		#region Comparison Operators

		public static bool operator ==(PasswordFlagData a, PasswordFlagData b) {
			if (a is null)
				return (b is null);
			else if (b is null)
				return false;
			return a.Equals(b);
		}
		public static bool operator !=(PasswordFlagData a, PasswordFlagData b) {
			if (a is null)
				return !(b is null);
			else if (b is null)
				return true;
			return !a.Equals(b);
		}

		#endregion

		#region Casting

		public static explicit operator PasswordFlagData(string s) => new PasswordFlagData(s);
		public static explicit operator PasswordFlagData(int v) => new PasswordFlagData(v);

		public static explicit operator string(PasswordFlagData fd) => fd.String;
		public static explicit operator int(PasswordFlagData fd) => fd.Value;

		#endregion

		#region Helpers

		/// <summary>
		///  Returns true if the string is a valid Flag Data letter string.
		/// </summary>
		/// <param name="s">The string to validate.</param>
		/// <returns>True if the string is a valid Flag Data letter string.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsValidString(string s) => Letter.IsValidString(s, Length);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateLetters(Letter[] letters, string paramName) {
			if (letters == null)
				throw new ArgumentNullException(paramName);
			if (letters.Length != Length)
				throw new ArgumentException($"Flag Data letters must be {Length} letters long, got {letters.Length} letters!",
					paramName);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateString(ref string s, string paramName) {
			if (s == null)
				throw new ArgumentNullException(paramName);
			if (s.Length != Length)
				throw new ArgumentException($"Flag Data string must be {Length} letters long, got {s.Length} letters!",
					paramName);
			s = s.ToUpper();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ValidateValue(int v, string paramName) {
			if (v < MinValue || v > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, v,
					$"Flag Data value must be between {MinValue} and {MaxValue}, got {v}!");
		}
		private void CopyFromLetters(Letter[] l) {
			for (int i = 0; i < Length; i++)
				letters[i].Character = l[i].Character;
		}
		private void CopyFromString(string s) {
			for (int i = 0; i < Length; i++)
				letters[i].Character = s[i];
		}
		private void CopyFromValue(int v) {
			for (int i = 0; i < Length; i++)
				letters[i].Value = (v >> (i * Letter.ShiftValue)) & Letter.MaskValue;
		}

		#endregion
	}
}
