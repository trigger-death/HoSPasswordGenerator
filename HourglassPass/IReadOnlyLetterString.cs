using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HourglassPass {
	/// <summary>
	///  An interface for all password structures containing <see cref="Letter"/> arrays.
	/// </summary>
	public interface IReadOnlyLetterString : IReadOnlyList<Letter>, IFormattable,
		IEquatable<Letter[]>, IEquatable<string>, IEquatable<int>
	{
		#region Constants

		/// <summary>
		///  Gets the mininum value constant for the letter string.
		/// </summary>
		int MinValue { get; }
		/// <summary>
		///  Gets the maximum value constant for the letter string.
		/// </summary>
		int MaxValue { get; }
		/*/// <summary>
		///  Gets the length constant for the letter string.
		/// </summary>
		int Count { get; }*/

		#endregion

		#region Properties

		/// <summary>
		///  Gets the letter at the specified index in the letter string.
		/// </summary>
		/// <param name="index">The index of the letter.</param>
		/// <returns>The letter at the specified index in the letter string.</returns>
		new Letter this[int index] { get; }
		/// <summary>
		///  Gets the letter string with an array of <see cref="Count"/> letters.
		/// </summary>
		Letter[] Letters { get; }
		/// <summary>
		///  Gets the letter string with a string of <see cref="Count"/> letters.
		/// </summary>
		string String { get; }
		/// <summary>
		///  Gets the letter string with a numeric value.
		/// </summary>
		int Value { get; }

		#endregion

		#region IFormattable

		/// <summary>
		///  Gets the string representation of the Scene ID with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <returns>The formatted string representation of the Scene ID.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		string ToString(string format);
		/// <summary>
		///  Gets the string representation of the Scene ID with the specified formatting.
		/// </summary>
		/// <param name="format">
		///  The format to display the letter string in.<para/>
		///  Password: P(Format)[spacing]. Format: S/s = Default, N/n = Normalize, R/r = Randomize, B/b = Binary, D/d = Decimal, X/x = Hexidecimal.<para/>
		///  Binary: VB[spacing] = Binary value format.<para/>
		///  Value: V[format] = Integer value format.
		/// </param>
		/// <param name="formatProvider">Unused.</param>
		/// <returns>The formatted string representation of the Scene ID.</returns>
		/// 
		/// <exception cref="FormatException">
		///  <paramref name="format"/> is invalid.
		/// </exception>
		new string ToString(string format, IFormatProvider formatProvider);

		#endregion

		#region Mutate

		/// <summary>
		///  Returns a letter string with normalized interchangeable characters.
		/// </summary>
		/// <param name="garbageChar">The character to use for garbage letters.</param>
		/// <returns>The normalized Password with consistent interchangeable characters.</returns>
		IReadOnlyLetterString Normalized(char garbageChar = Letter.GarbageChar);
		/// <summary>
		///  Returns a letter string with randomized interchangeable characters.
		/// </summary>
		/// <returns>The randomized Password with random interchangable characters.</returns>
		IReadOnlyLetterString Randomized();

		#endregion
	}
}
