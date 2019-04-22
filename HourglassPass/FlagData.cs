using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HourglassPass {
	/// <summary>
	///  Password data for in-game flags.
	/// </summary>
	public class FlagData : IFormattable, IEnumerable<Letter>,
		IEquatable<FlagData>, IEquatable<string>, IEquatable<int>
	{
		#region Constants

		/// <summary>
		///  The minimum value representable by Flag Data.
		/// </summary>
		public const int MinValue = 0x00000;
		/// <summary>
		///  The maximum value representable by Flag Data.
		/// </summary>
		public const int MaxValue = 0xFFFFF;
		/// <summary>
		///  The number of letters in this password structure.
		/// </summary>
		public const int Length = 5;

		#endregion

		#region Fields

		/// <summary>
		///  The array <see cref="Length"/> letters that make up the Flag Data.
		/// </summary>
		private readonly Letter[] letters = {
			new Letter(0, false),
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
		public FlagData() { }
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
		public FlagData(Letter[] letters) {
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
		public FlagData(string flags) {
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
		public FlagData(int value) {
			ValidateValue(value, nameof(value));
			CopyFromValue(value);
		}
		/// <summary>
		///  Constructs a copy of the Flag Data.
		/// </summary>
		/// <param name="flagData">The Flag Data to construct a copy of.</param>
		public FlagData(FlagData flagData) {
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
					v |= letters[Length - i - 1].Value << (i * 4);
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
		///  The format to display the Flag Data in.<para/>
		///  S/empty = String, B = Binary, D/N = Decimal, X = Hexidecimal.
		///  # = spacing between values. Default = 0.
		/// </param>
		/// <returns>The formatted string representation of the Flag Data.</returns>
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);
		/// <summary>
		///  Gets the string representation of the Flag Data with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the Flag Data in.<para/>
		///  S/empty = String, B = Binary, D/N = Decimal, X = Hexidecimal.
		///  # = spacing between values. Default = 0.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the Flag Data.</returns>
		public string ToString(string format, IFormatProvider formatProvider) {
			if (format == null)
				return ToString();
			Match match = Regex.Match(format, @"^(?'letter'[BDNSX])?(?'space'\d+)?$");
			if (!match.Success)
				throw new FormatException($"Invalid Flag Data format \"{format}\"!");
			int space = 0;
			if (match.Groups["space"].Success)
				space = int.Parse(match.Groups["space"].Value);

			string lf = match.Groups["letter"].Value;
			return string.Join(new string(' ', space), letters.Select(l => l.ToString(lf, formatProvider)));
		}

		public override int GetHashCode() => Value;

		public override bool Equals(object obj) {
			if (obj is FlagData fd) return Equals(fd);
			if (obj is Letter[] l) return Equals(l);
			if (obj is string s) return Equals(s);
			if (obj is int i) return Equals(i);
			return false;
		}
		public bool Equals(FlagData other) => Value == other.Value;
		public bool Equals(Letter[] other) => Value == new FlagData(other).Value;
		public bool Equals(string other) => Value == new FlagData(other).Value;
		public bool Equals(int other) => Value == other;

		#endregion

		#region IEnumerable Implementation

		/// <summary>
		///  Gets the enumerator for the letters in the Flag Data.
		/// </summary>
		/// <returns>An enumerator to traverse the letters in the Flag Data.</returns>
		public IEnumerator<Letter> GetEnumerator() => ((IEnumerable<Letter>) letters).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => letters.GetEnumerator();

		#endregion

		#region Casting

		public static explicit operator FlagData(string s) => new FlagData(s);
		public static explicit operator FlagData(int v) => new FlagData(v);

		public static explicit operator string(FlagData fd) => fd.String;
		public static explicit operator int(FlagData fd) => fd.Value;

		#endregion

		#region Utilities

		public void Normalize(char garbageChar = Letter.GarbageChar) {
			for (int i = 2; i < Length; i++)
				letters[i].Normalize(garbageChar);
		}
		public FlagData Normalized(char garbageChar = Letter.GarbageChar) {
			FlagData fd = new FlagData(this);
			fd.Normalize(garbageChar);
			return fd;
		}
		public void Randomize() {
			for (int i = 2; i < Length; i++)
				letters[i].Randomize();
		}
		public FlagData Randomized() {
			FlagData fd = new FlagData(this);
			fd.Randomize();
			return fd;
		}

		#endregion

		#region Operations

		public FlagData Zero(int index) => Set(index, 0);
		public FlagData Negate(int index) => Operation(index, v => ~v);

		public FlagData Set(int index, Letter value) => Set(index, value.Value);
		public FlagData Add(int index, Letter value) => Add(index, value.Value);
		public FlagData Sub(int index, Letter value) => Sub(index, value.Value);
		public FlagData And(int index, Letter value) => And(index, value.Value);
		public FlagData Or(int index, Letter value) => Or(index, value.Value);
		public FlagData Xor(int index, Letter value) => Xor(index, value.Value);

		public FlagData Set(int index, char value) => Set(index, new Letter(value).Value);
		public FlagData Add(int index, char value) => Add(index, new Letter(value).Value);
		public FlagData Sub(int index, char value) => Sub(index, new Letter(value).Value);
		public FlagData And(int index, char value) => And(index, new Letter(value).Value);
		public FlagData Or(int index, char value) => Or(index, new Letter(value).Value);
		public FlagData Xor(int index, char value) => Xor(index, new Letter(value).Value);

		public FlagData Set(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => value);
		}
		public FlagData Add(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => Math.Min(Letter.MaxValue, v + value));
		}
		public FlagData Sub(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => Math.Max(Letter.MinValue, v - value));
		}
		public FlagData And(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => v & value);
		}
		public FlagData Or(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => v | value);
		}
		public FlagData Xor(int index, int value) {
			Letter.ValidateValue(value, nameof(value));
			return Operation(index, v => v ^ value);
		}

		public FlagData Operation(IFlagOperation iop) {
			if (iop.Type == OpType.Multi) {
				FlagMultiOperation mop = (FlagMultiOperation) iop;
				foreach (FlagOperation op in mop.Operations)
					Operation(op);
			}
			else {
				Operation((FlagOperation) iop);
			}
			return this;
		}
		public FlagData Operation(FlagOperation op) {
			switch (op.Type) {
			case OpType.Zero: return Zero(op.Index);
			case OpType.Negate: return Negate(op.Index);

			case OpType.Set: return Set(op.Index, op.Operand);
			case OpType.Add: return Add(op.Index, op.Operand);
			case OpType.Sub: return Sub(op.Index, op.Operand);
			case OpType.Or: return Or(op.Index, op.Operand);
			case OpType.Xor: return Xor(op.Index, op.Operand);
			}
			return this;
		}

		private FlagData Operation(int index, Func<int, int> operation) {
			int oldValue = letters[index].Value;
			int newValue = operation(oldValue);
			if (oldValue != newValue)
				letters[index].Value = newValue;
			return this;
		}

		#endregion

		#region Helpers

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
			// Values are refersed that they print nicely in hex
			for (int i = 0; i < Length; i++) {
				letters[Length - i - 1].Value = v % Letter.ModValue;
				v /= Letter.ModValue;
			}
		}

		#endregion
	}
}
