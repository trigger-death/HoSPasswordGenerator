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
	public struct TitleChapter {
		#region Constants

		/// <summary>
		///  The minimum allowed value for titles and chapters.
		/// </summary>
		public const int MinValue = 1;
		/// <summary>
		///  The maximum allowed value for titles and chapters.
		/// </summary>
		public const int MaxValue = 999;

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

		public override string ToString() => $"{Title}-{Chapter}";

		public static TitleChapter Parse(string s) {
			Match match = Regex.Match(s, @"(?'t'\d+)-(?'c'\d+)");
			if (!match.Success)
				throw new FormatException($"Title Chapter must be formatted as \"Title-Chapter\", got \"{s}\"!");

			return new TitleChapter(
				int.Parse(match.Groups["t"].Value),
				int.Parse(match.Groups["c"].Value));
		}

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

		#endregion
	}
}
