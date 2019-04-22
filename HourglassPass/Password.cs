using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HourglassPass {
	/// <summary>
	///  A password structure containing both a Scene ID and Flag Data.
	/// </summary>
	public class Password : IEnumerable<Letter>, IEquatable<Password>, IEquatable<string>, IEquatable<int> {
		#region Constants

		/// <summary>
		///  The amount to shift <see cref="Scene"/>'s value by when combining with <see cref="Flags"/>'s value.
		/// </summary>
		private const int SceneIdShift = 20;

		/// <summary>
		///  The initial VALID password state during the start of the game. The game gets it wrong. This works.
		/// </summary>
		public static readonly Password Initial = new Password("AZZOZBBB");

		/// <summary>
		///  The minimum value representable by a Password.
		/// </summary>
		public const int MinValue = 0;
		/// <summary>
		///  The maximum value representable by a Password.
		/// </summary>
		public const int MaxValue = (SceneId.MaxValue << SceneIdShift) | FlagData.MaxValue;
		/// <summary>
		///  The number of letters in this password structure.
		/// </summary>
		public const int Length = SceneId.Length + FlagData.Length;

		#endregion

		#region Fields

		/// <summary>
		///  The identifier for the scene to jump to.
		/// </summary>
		public SceneId Scene { get; } = new SceneId();
		/// <summary>
		///  The currently set game flags.
		/// </summary>
		public FlagData Flags { get; } = new FlagData();

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Password with all values unset (zero).
		/// </summary>
		public Password() { }
		/// <summary>
		///  Constructs a Password with an array of <see cref="Length"/> letters.
		/// </summary>
		/// <param name="letters">The array containing the letters of the Password.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="letters"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <paramref name="letters"/> is not <see cref="Length"/>.
		/// </exception>
		public Password(Letter[] letters) {
			ValidateLetters(letters, nameof(letters));
			CopyFromLetters(letters);
		}
		/// <summary>
		///  Constructs a Password with a string of <see cref="Length"/> letters.
		/// </summary>
		/// <param name="flags">The string containing the letters of the Password.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="flags"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  The length of <paramref name="flags"/> is not <see cref="Length"/>.-or- A character in
		///  <paramref name="flags"/> is not a valid letter character.
		/// </exception>
		public Password(string pass) {
			ValidateString(ref pass, nameof(pass));
			CopyFromString(pass);
		}
		/// <summary>
		///  Constructs a Password with a numeric value.
		/// </summary>
		/// <param name="value">The value of the Password.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="value"/> is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public Password(int value) {
			ValidateValue(value, nameof(value));
			CopyFromValue(value);
		}
		/// <summary>
		///  Constructs a copy of the Password.
		/// </summary>
		/// <param name="flagData">The Password to construct a copy of.</param>
		public Password(Password password) {
			CopyFromLetters(password.Letters);
		}

		#endregion

		#region Properties

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
				if (index < 0 || index >= Length)
					throw new ArgumentOutOfRangeException(nameof(index), index,
						$"Index must be between 0 and {(Length-1)}, got {index}!");
				if (index < SceneId.Length)
					return Scene[index];
				else
					return Flags[index - SceneId.Length];
			}
			set {
				if (index < 0 || index >= Length)
					throw new ArgumentOutOfRangeException(nameof(index), index,
						$"Index must be between 0 and {(Length-1)}, got {index}!");
				if (index < SceneId.Length)
					Scene[index] = value;
				else
					Flags[index - SceneId.Length] = value;
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
			get {
				Letter[] letters = new Letter[Length];
				Array.Copy(Scene.Letters, letters, SceneId.Length);
				Array.Copy(Flags.Letters, 0, letters, SceneId.Length, FlagData.Length);
				return letters;
			}
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
			get => $"{Scene}{Flags}";
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
			get => (Scene.Value << SceneIdShift) | Flags.Value;
			set {
				ValidateValue(value, nameof(Value));
				CopyFromValue(value);
			}
		}

		#endregion

		#region Object Overrides

		public override string ToString() => String;

		public override int GetHashCode() => Value;

		public override bool Equals(object obj) {
			if (obj is Password pw) return Equals(pw);
			if (obj is Letter[] l) return Equals(l);
			if (obj is string s) return Equals(s);
			if (obj is int i) return Equals(i);
			return false;
		}
		public bool Equals(Password other) => Scene.Equals(other.Scene) && Flags.Equals(other.Flags);
		public bool Equals(Letter[] other) => Equals(new Password(other));
		public bool Equals(string other) => Equals(new Password(other));
		public bool Equals(int other) => Value == other;

		#endregion

		#region IEnumerable Implementation

		/// <summary>
		///  Gets the enumerator for the letters in the Password.
		/// </summary>
		/// <returns>An enumerator to traverse the letters in the Password.</returns>
		public IEnumerator<Letter> GetEnumerator() => Scene.Concat(Flags).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

		#region Casting

		public static explicit operator Password(string s) => new Password(s);
		public static explicit operator Password(int v) => new Password(v);

		public static explicit operator string(Password pw) => pw.String;
		public static explicit operator int(Password pw) => pw.Value;

		#endregion

		#region Utilities
		
		public void Normalize(char garbageChar = Letter.GarbageChar) {
			Scene.Normalize(garbageChar);
			Flags.Normalize(garbageChar);
		}
		public Password Normalized(char garbageChar = Letter.GarbageChar) {
			Password pw = new Password(this);
			pw.Normalize(garbageChar);
			return pw;
		}
		public void Randomize() {
			Scene.Randomize();
			Flags.Randomize();
		}
		public Password Randomized() {
			Password pw = new Password(this);
			pw.Randomize();
			return pw;
		}

		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateLetters(Letter[] letters, string paramName) {
			if (letters == null)
				throw new ArgumentNullException(paramName);
			if (letters.Length != Length)
				throw new ArgumentException($"Password letters must be {Length} letters long, got {letters.Length} letters!",
					paramName);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateString(ref string s, string paramName) {
			if (s == null)
				throw new ArgumentNullException(paramName);
			if (s.Length != Length)
				throw new ArgumentException($"Password string must be {Length} letters long, got {s.Length} letters!",
					paramName);
			s = s.ToUpper();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateValue(int v, string paramName) {
			if (v < MinValue || v > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, v, $"Password value must be between {MinValue} and {MaxValue}, got {v}!");
		}

		private void CopyFromLetters(Letter[] l) {
			Letter[] sceneLetters = new Letter[SceneId.Length];
			Letter[] flagsLetters = new Letter[FlagData.Length];
			Array.Copy(l, sceneLetters, SceneId.Length);
			Array.Copy(l, SceneId.Length, flagsLetters, 0, FlagData.Length);
			Scene.Letters = sceneLetters;
			Flags.Letters = flagsLetters;
		}
		private void CopyFromString(string s) {
			Scene.String = s.Substring(0, SceneId.Length);
			Flags.String = s.Substring(SceneId.Length, FlagData.Length);
		}
		private void CopyFromValue(int value) {
			Scene.Value = (value >> SceneIdShift) & SceneId.MaxValue;
			Flags.Value = value & FlagData.MaxValue;
		}


		#endregion
	}
}
