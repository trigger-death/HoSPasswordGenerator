using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace HourglassPass {
	/// <summary>
	///  A password identifier for an in-game Scene.
	/// </summary>
	[Serializable]
	public sealed class SceneId : IEquatable<SceneId>, IEquatable<string>, IEquatable<int>,
		IComparable, IComparable<string>, IComparable<int>, IFormattable, IEnumerable<Letter>
	{
		#region Constants

		/// <summary>
		///  Used for 3rd letter randomization.
		/// </summary>
		private static readonly Random random = new Random();

		/// <summary>
		///  The minimum value representable by a Scene ID.
		/// </summary>
		public const int MinValue = 0x000;
		/// <summary>
		///  The maximum value representable by a Scene ID.
		/// </summary>
		public const int MaxValue = 0x3FF;
		/// <summary>
		///  The number of letters in this password structure.
		/// </summary>
		public const int Length = 3;

		#endregion

		#region Fields

		/// <summary>
		///  The array <see cref="Length"/> letters that make up the Scene ID.
		/// </summary>
		private readonly Letter[] letters = new Letter[Length];

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Scene ID at zero.
		/// </summary>
		public SceneId() { }
		/// <summary>
		///  Constructs a Scene ID with an array of <see cref="Length"/> letters.
		/// </summary>
		/// <param name="letters">The array containing the letters of the Scene ID.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="letters"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <paramref name="letters"/> is not <see cref="Length"/>.
		/// </exception>
		public SceneId(Letter[] letters) {
			ValidateLetters(letters, nameof(letters));
			CopyFromLetters(letters);
		}
		/// <summary>
		///  Constructs a Scene ID with a string of <see cref="Length"/> letters.
		/// </summary>
		/// <param name="scene">The string containing the letters of the Scene ID.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="scene"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <paramref name="scene"/> is not <see cref="Length"/>.-or- A character in
		///  <paramref name="scene"/> is not a valid letter character.
		/// </exception>
		public SceneId(string scene) {
			ValidateString(ref scene, nameof(scene));
			CopyFromString(scene);
		}
		/// <summary>
		///  Constructs a Scene ID with a numeric value.
		/// </summary>
		/// <param name="value">The value of the Scene ID.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public SceneId(int value) {
			ValidateValue(value, nameof(value));
			CopyFromValue(value);
		}
		/// <summary>
		///  Constructs a copy of the Scene ID.
		/// </summary>
		/// <param name="sceneId">The Scene ID to construct a copy of.</param>
		public SceneId(SceneId sceneId) {
			if (sceneId == null)
				throw new ArgumentNullException(nameof(sceneId));
			Array.Copy(sceneId.letters, letters, Length);
		}

		#endregion

		#region Properties

		/// <summary>
		///  Gets or sets the letter at the specified index in the Scene ID.
		/// </summary>
		/// <param name="index">The index of the letter.</param>
		/// <returns>The letter at the specified index in the Scene ID.</returns>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Length"/>.
		/// </exception>
		public Letter this[int index] {
			get => letters[index];
			set {
				letters[index].Character = value.Character;
				UpdateGarbageChar();
			}
		}
		/// <summary>
		///  Gets or sets the Scene ID with an array of <see cref="Length"/> letters.
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
		///  Gets or sets the Scene ID with a string of <see cref="Length"/> letters.
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
		///  Gets or sets the Scene ID with a numeric value.
		/// </summary>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <see cref="Value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public int Value {
			get {
				int v = 0;
				for (int i = 0; i < Length; i++)
					v |= letters[i].Value << (i * 4);
				return v & MaxValue; // Make sure to cutoff the last two bits of the last letter
			}
			//get => letters[0].Value | (letters[1].Value << 4) | ((letters[2].Value & 0x3) << 8);
			set {
				ValidateValue(value, nameof(Value));
				CopyFromValue(value);
			}
		}

		#endregion

		#region Utilities

		public void Normalize(char garbageChar = Letter.GarbageChar) {
			letters[1].Normalize(garbageChar);
			letters[2].Value &= 0x3;
		}
		public SceneId Normalized(char garbageChar = Letter.GarbageChar) {
			SceneId id = new SceneId(this);
			id.Normalize(garbageChar);
			return id;
		}
		public void Randomize() {
			letters[1].Randomize();
			// The 3rd letter randomizes the last 2 bits.
			letters[2].Value = (letters[2].Value % 4) + random.Next(4) * 4;
		}
		public SceneId Randomized() {
			SceneId id = new SceneId(this);
			id.Randomize();
			return id;
		}

		#endregion

		#region Comparison Operators

		public static bool operator ==(SceneId a, SceneId b) {
			return (!(a is null) ? (!(b is null) ? a.Equals(b) : false) :  (b is null));
		}
		public static bool operator !=(SceneId a, SceneId b) {
			return (!(a is null) ? (!(b is null) ? !a.Equals(b) : true) : !(b is null));
		}

		public static bool operator <(SceneId a, SceneId b) => a.CompareTo(b) < 0;
		public static bool operator >(SceneId a, SceneId b) => a.CompareTo(b) > 0;

		public static bool operator <=(SceneId a, SceneId b) => a.CompareTo(b) <= 0;
		public static bool operator >=(SceneId a, SceneId b) => a.CompareTo(b) >= 0;

		#endregion

		#region Casting

		public static explicit operator SceneId(string s) => new SceneId(s);
		public static explicit operator SceneId(int v) => new SceneId(v);

		public static explicit operator string(SceneId id) => id.String;
		public static explicit operator int(SceneId id) => id.Value;

		#endregion

		#region Object Overrides

		/// <summary>
		///  Gets the string representation of the Scene ID.
		/// </summary>
		/// <returns>The string representation of the Scene ID.</returns>
		public override string ToString() => String;

		/// <summary>
		///  Gets the string representation of the Scene ID with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the Scene ID in.<para/>
		///  S/empty = String, B = Binary, D = Decimal, N = Decimal with Commas, X = Hexidecimal.
		/// </param>
		/// <returns>The formatted string representation of the Scene ID.</returns>
		public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);
		/// <summary>
		///  Gets the string representation of the Scene ID with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the Scene ID in.<para/>
		///  S/empty = String, B = Binary, D = Decimal, N = Decimal with Commas, X = Hexidecimal.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the Scene ID.</returns>
		public string ToString(string format, IFormatProvider formatProvider) {
			if (format == null)
				return ToString();

			switch (format) {
			case "":
			case "S": return ToString();
			case "B": return $"{letters[2].ToString("B").Substring(2)} {letters[1]:B} {letters[0]:B}";
			case "X": return $"{Value:X3}";
			case "D": return Value.ToString();
			case "N": return Value.ToString("N0");
			}
			throw new FormatException($"Invalid Scene ID format \"{format}\"!");
		}

		public override int GetHashCode() => Value;

		public override bool Equals(object obj) {
			if (obj is SceneId id) return Equals(id);
			if (obj is Letter[] l) return Equals(l);
			if (obj is string s) return Equals(s);
			if (obj is int i) return Equals(i);
			return false;
		}
		public bool Equals(SceneId other) => other != null && Value == other.Value;
		public bool Equals(Letter[] other) => other != null && Value == new SceneId(other).Value;
		public bool Equals(string other) => other != null && Value == new SceneId(other).Value;
		public bool Equals(int other) => Value == other;

		public int CompareTo(object obj) {
			if (obj is SceneId id) return CompareTo(id);
			if (obj is Letter[] l) return CompareTo(l);
			if (obj is string s) return CompareTo(s);
			if (obj is int i) return CompareTo(i);
			throw new ArgumentException($"SceneId cannot be compared against type {obj.GetType().Name}!");
		}
		public int CompareTo(SceneId other) => Value.CompareTo(other.Value);
		public int CompareTo(Letter[] other) => Value.CompareTo(new SceneId(other).Value);
		public int CompareTo(string other) => Value.CompareTo(new SceneId(other).Value);
		public int CompareTo(int other) => Value.CompareTo(other);

		#endregion

		#region IEnumerable Implementation

		/// <summary>
		///  Gets the enumerator for the letters in the Scene ID.
		/// </summary>
		/// <returns>An enumerator to traverse the letters in the Scene ID.</returns>
		public IEnumerator<Letter> GetEnumerator() => ((IEnumerable<Letter>) letters).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => letters.GetEnumerator();

		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateLetters(Letter[] l, string paramName) {
			if (l == null)
				throw new ArgumentNullException(paramName);
			if (l.Length != Length)
				throw new ArgumentException($"Scene ID letters must be {Length} letters long, got {l.Length} letters!",
					paramName);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateString(ref string s, string paramName) {
			if (s == null)
				throw new ArgumentNullException(paramName);
			if (s.Length != Length)
				throw new ArgumentException($"Scene ID string must be {Length} letters long, got {s.Length} letters!",
					paramName);
			s = s.ToUpper();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateValue(int v, string paramName) {
			if (v < MinValue || v > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, v,
					$"Scene ID value must be between {MinValue} and {MaxValue}, got {v}!");
		}

		private void CopyFromLetters(Letter[] l) {
			letters[0].Character = l[0].Character;
			letters[2].Character = l[2].Character;
			UpdateGarbageChar(); // Allow 2nd letter to retain it's garbage letter
			letters[1].Character = l[1].Character;
		}
		private void CopyFromString(string s) {
			letters[0].Character = s[0];
			letters[2].Character = s[2];
			UpdateGarbageChar(); // Allow 2nd letter to retain it's garbage letter
			letters[1].Character = s[1];
		}

		private void CopyFromValue(int v) {
			for (int i = 0; i < Length; i++) {
				letters[i].Value = v % Letter.ModValue;
				v /= Letter.ModValue;
			}
			UpdateGarbageChar();
		}

		private void UpdateGarbageChar() {
			// 2nd Letter is garbage when zero, if 3rd letter is not zero.
			letters[1].IsGarbage = letters[2].Value != 0;
		}

		/*public static int LettersToValue(Letter[] letters, string paramName) {
			int value = letters[0].Value;
			if (letters.Length >= 2) {
				value |= letters[1].Value << 4;
				if (letters.Length >= 3)
					value |= (letters[2].Value & 0x3) << 8;
			}

			return value;
		}*/

		#endregion
	}
}
