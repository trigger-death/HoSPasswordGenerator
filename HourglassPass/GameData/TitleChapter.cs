using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HourglassPass.GameData {
	/// <summary>
	///  A ID pair that refers to a DVD's Title-Chapter combination.
	/// </summary>
	public struct TitleChapter : IEquatable<TitleChapter>, IComparable, IComparable<TitleChapter> {
		#region Constants

		/// <summary>
		///  The minimum allowed value for titles and chapters.
		/// </summary>
		public const int MinValue = 1;
		/// <summary>
		///  The maximum allowed value for titles and chapters.
		/// </summary>
		public const int MaxValue = 999;

		/// <summary>
		///  The regex used to parse Title-Chapter pairs.
		/// </summary>
		private static readonly Regex ParseRegex = new Regex(@"^(?'t'\d+)-(?'c'\d+)$");

		#endregion

		#region Fields

		/// <summary>
		///  The DVD-PG format title ID of the scene.
		/// </summary>
		private short title;
		/// <summary>
		///  The DVD-PG format chapter ID of the scene.
		/// </summary>
		private short chapter;

		#endregion

		#region Fields

		/// <summary>
		///  Gets or sets the DVD-PG format title ID of the scene.
		/// </summary>
		public int Title {
			get => Math.Max(1, (int) title); // Fix for zero-initialized struct values
			set {
				ValidateTitle(value, nameof(Title));
				title = (short) value;
			}
		}
		/// <summary>
		///  Gets or sets the DVD-PG format chapter ID of the scene.
		/// </summary>
		public int Chapter {
			get => Math.Max(1, (int) chapter); // Fix for zero-initialized struct values
			set {
				ValidateChapter(value, nameof(Chapter));
				chapter = (short) value;
			}
		}
		/// <summary>
		///  Gets the integer value of the combined title and chapter.
		/// </summary>
		internal int Value => (title << 16) | (int) chapter;

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a title and chapter structure with the specified IDs.
		/// </summary>
		/// <param name="title">The DVD-PG format title ID.</param>
		/// <param name="chapter">The DVD-PG format chapter ID.</param>
		/// 
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="title"/> or <paramref name="chapter"/> is less than <see cref="MinValue"/> or greater than
		///  <see cref="MaxValue"/>.
		/// </exception>
		public TitleChapter(int title, int chapter) {
			ValidateTitle(title, nameof(title));
			ValidateTitle(chapter, nameof(chapter));
			this.title = (short) title;
			this.chapter = (short) chapter;
		}

		#endregion
		
		#region Object Overrides

		/// <summary>
		///  Gets the string representation of the Title-Chapter.
		/// </summary>
		/// <returns>The string representation of the Title-Chapter.</returns>
		public override string ToString() => $"{Title}-{Chapter}";

		/// <summary>
		///  Gets the hash code as the Title-Chapter.
		/// </summary>
		/// <returns>The Title-Chapter's hash code.</returns>
		public override int GetHashCode() => Value;

		/// <summary>
		///  Checks if the object is a <see cref="TitleChapter"/> and checks for equality between the Title-Chapters.
		/// </summary>
		/// <param name="obj">The object to check for equality with.</param>
		/// <returns>The object is a Title-Chapter and has the same title and chapter values.</returns>
		public override bool Equals(object obj) {
			if (obj is TitleChapter tc) return Equals(tc);
			return false;
		}
		/// <summary>
		///  Checks for equality between the Title-Chapters.
		/// </summary>
		/// <param name="other">The Title-Chapter to check for equality with.</param>
		/// <returns>The Title-Chapters are the same.</returns>
		public bool Equals(TitleChapter other) => title == other.title && chapter == other.chapter;

		/// <summary>
		///  Checks if the object is a <see cref="TitleChapter"/> and compares it to this Title-Chapter.
		/// </summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>The comparison between the object and the Title-Chapter.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="obj"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="obj"/> is not a <see cref="TitleChapter"/>.
		/// </exception>
		public int CompareTo(object obj) {
			if (obj is TitleChapter tc) return CompareTo(tc);
			if (obj is null)
				throw new ArgumentNullException(nameof(obj));
			throw new ArgumentException($"Title-Chapter cannot be compared against type {obj.GetType().Name}!");
		}
		/// <summary>
		///  Compares the two Title-Chapters.
		/// </summary>
		/// <param name="obj">The Title-Chapter to compare with.</param>
		/// <returns>The between the Title-Chapters.</returns>
		public int CompareTo(TitleChapter other) => Value.CompareTo(other.Value);

		#endregion

		#region Parse

		/// <summary>
		///  Parses the string representation of the Title-Chapter. Must be in format #-#.
		/// </summary>
		/// <param name="s">The string representation of the Title-Chapter.</param>
		/// <returns>The parsed Title-Chapter.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="s"/> is null.
		/// </exception>
		/// <exception cref="FormatException">
		///  <paramref name="s"/> is not of format "#-#".
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  Title or chapter is less than <see cref="MinValue"/> or greater than <see cref="MaxValue"/>.
		/// </exception>
		public static TitleChapter Parse(string s) {
			if (s == null)
				throw new ArgumentNullException(nameof(s));

			Match match = ParseRegex.Match(s);
			if (!match.Success)
				throw new FormatException($"Title-Chapter must be formatted as \"Title-Chapter\", got \"{s}\"!");

			return new TitleChapter(int.Parse(match.Groups["t"].Value), int.Parse(match.Groups["c"].Value));
		}
		/// <summary>
		///  Tries to parse the string representation of the Title-Chapter.
		/// </summary>
		/// <param name="s">The string representation of the Title-Chapter.</param>
		/// <param name="titleChapter">The output parsed Title-Chapter.</param>
		/// <returns>True if the parse was successful, otherwise false.</returns>
		public static bool TryParse(string s, out TitleChapter titleChapter) {
			titleChapter = new TitleChapter();
			if (s == null)
				return false;

			Match match = ParseRegex.Match(s);
			if (!match.Success)
				return false;

			if (!int.TryParse(match.Groups["t"].Value, out int t) || !IsValidValue(t))
				return false;
			if (!int.TryParse(match.Groups["c"].Value, out int c) || !IsValidValue(c))
				return false;

			titleChapter = new TitleChapter(t, c);
			return true;
		}

		#endregion

		#region Comparison Operators
		
		public static bool operator ==(TitleChapter a, TitleChapter b) => a.Equals(b);
		public static bool operator !=(TitleChapter a, TitleChapter b) => !a.Equals(b);

		public static bool operator <(TitleChapter a, TitleChapter b) => a.CompareTo(b) < 0;
		public static bool operator >(TitleChapter a, TitleChapter b) => a.CompareTo(b) > 0;

		public static bool operator <=(TitleChapter a, TitleChapter b) => a.CompareTo(b) <= 0;
		public static bool operator >=(TitleChapter a, TitleChapter b) => a.CompareTo(b) >= 0;

		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateTitle(int t, string paramName) {
			if (t < MinValue || t > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, t,
					$"Title must be between {MinValue} and {MaxValue}, got {t}!");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateChapter(int c, string paramName) {
			if (c < MinValue || c > MaxValue)
				throw new ArgumentOutOfRangeException(paramName, c,
					$"Chapter must be between {MinValue} and {MaxValue}, got {c}!");
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsValidValue(int v) {
			return (v >= MinValue && v <= MaxValue);
		}

		#endregion
	}
}
