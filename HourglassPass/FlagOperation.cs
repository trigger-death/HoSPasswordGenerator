using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace HourglassPass {
	/// <summary>
	///  The type of operation to perform for a flag.
	/// </summary>
	public enum OpType : byte {
		// Unary Operations:

		/// <summary>Flag = 0</summary>
		Zero,
		/// <summary>Flag = ~Flag</summary>
		Negate,

		// Binary Operations:

		/// <summary>Flag = operand</summary>
		Set,
		/// <summary>Flag += operand (clamped)</summary>
		Add,
		/// <summary>Flag -= operand (clamped)</summary>
		Sub,
		/// <summary>Flag &= operand</summary>
		And,
		/// <summary>Flag |= operand</summary>
		Or,
		/// <summary>Flag ^= operand</summary>
		Xor,
	}
	public struct FlagLetter {
		#region Fields

		/// <summary>
		///  The data containing both operand value and mutation index.
		/// </summary>
		private readonly byte value;

		#endregion

		#region Properties

		/// <summary>
		///  Gets the value of the operand letter.
		/// </summary>
		public int Value => value & 0xF;
		/// <summary>
		///  Gets the index of the flag to mutate.
		/// </summary>
		public int Index => (value >> 4) & 0xF;
		/// <summary>
		///  Gets the letter of the operand value.
		/// </summary>
		public Letter Letter => new Letter(Value);

		#endregion

		#region Constructors

		public FlagLetter(int index, Letter letter) {
			ValidateIndex(index, nameof(index));
			value = (byte) ((index << 4) | letter.Value);
		}

		public FlagLetter(int index, char character) : this(index, new Letter(character)) {
			ValidateIndex(index, nameof(index));
			Letter.ValidateChar(ref character, nameof(character));
			value = (byte) ((index << 4) | Letter.GetValueOfChar(character));
		}
		public FlagLetter(int index, int value) {
			ValidateIndex(index, nameof(index));
			Letter.ValidateValue(value, nameof(value));
			this.value = (byte) ((index << 4) | value);
		}

		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateIndex(int index, string paramName) {
			if (index < 0 || index >= FlagData.Length) {
				throw new ArgumentOutOfRangeException(paramName, index,
					$"Index must be between 0 and {(FlagData.Length-1)}, got {index}!");
			}
		}

		#endregion
	}
	public static class FlagOperationExtensions {
		public static string ToFlagsString(this FlagLetter[] flags, OpType type = OpType.Set) {
			if (flags == null)
				throw new ArgumentNullException(nameof(flags));
			StringBuilder str = new StringBuilder();
			for (int i = 0; i < FlagData.Length; i++) {
				var flag = flags.Where(f => f.Index == i);
				if (flag.Any()) {
					if (type == OpType.Zero || type == OpType.Negate)
						str.Append('Z');
					else
						str.Append(flag.First().Letter);
				}
				else if (i % 2 == 0)
					str.Append('-');
				else
					str.Append('.');
			}
			return str.ToString();
		}
		public static string ToOpString(this OpType type) {
			switch (type) {
			case OpType.Zero: return "0";
			case OpType.Negate: return "~";
			case OpType.Set: return "=";
			case OpType.Add: return "+";
			case OpType.Sub: return "-";
			case OpType.And: return "&";
			case OpType.Or: return "|";
			case OpType.Xor: return "^";
			default: throw new ArgumentException($"Invalid {nameof(OpType)} {type}!", nameof(type));
			}
		}
	}
	/// <summary>
	///  A single Flag Data operation.
	/// </summary>
	public sealed class FlagOperation {
		#region Fields

		/// <summary>
		///  Gets the type of operation to perform.
		/// </summary>
		public OpType Type { get; }
		/*/// <summary>
		///  Gets the index of the flag to mutate.
		/// </summary>
		public int[] Index { get; }
		/// <summary>
		///  Gets the operand value to use in the operation type.
		/// </summary>
		public int[] Value { get; }*/

		/// <summary>
		///  Gets the flags to use in this operation.
		/// </summary>
		public FlagLetter[] Flags { get; }

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Flag Data operation for an <see cref="OpType"/> that does not use operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.-or- <paramref name="type"/> is an <see cref="OpType"/>
		///  that requires an operand.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index) {
			ValidateOpType(type, nameof(type));
			ValidateIndex(index, nameof(index));
			if (type >= OpType.Set) {
				throw new ArgumentException(
					$"Cannot use {nameof(OpType)} {type} outside of binary constructor!", nameof(type));
			}
			Type = type;
			Flags = new[] { new FlagLetter(index, 0) };
		}
		/// <summary>
		///  Constructs a Flag Data operation with a letter operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="letter">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, Letter letter) {
			ValidateOpType(type, nameof(type));
			Type = type;
			Flags = new[] { new FlagLetter(index, letter) };
		}
		/// <summary>
		///  Constructs a Flag Data operation with a character operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="character">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, char character) {
			ValidateOpType(type, nameof(type));
			Type = type;
			Flags = new[] { new FlagLetter(index, character) };
		}
		/// <summary>
		///  Constructs a Flag Data operation with a numeric operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="value">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, int value) {
			ValidateOpType(type, nameof(type));
			Type = type;
			Flags = new[] { new FlagLetter(index, value) };
		}
		/// <summary>
		///  Constructs a Flag Data operation with index letter operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="flags">The index letter operands.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="flags"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, params (int index, Letter letter)[] flags) {
			ValidateOpType(type, nameof(type));
			ValidateFlags(flags, nameof(flags));
			Type = type;
			Flags = new FlagLetter[flags.Length];
			for (int i = 0; i < flags.Length; i++) {
				var (index, letter) = flags[i];
				Flags[i] = new FlagLetter(index, letter);
			}
		}
		/// <summary>
		///  Constructs a Flag Data operation with index character operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="flags">The index character operands.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="flags"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, params (int index, char character)[] flags) {
			ValidateOpType(type, nameof(type));
			ValidateFlags(flags, nameof(flags));
			Type = type;
			Flags = new FlagLetter[flags.Length];
			for (int i = 0; i < flags.Length; i++) {
				var (index, character) = flags[i];
				Flags[i] = new FlagLetter(index, character);
			}
		}
		/// <summary>
		///  Constructs a Flag Data operation with index numeric operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="flags">The index numeric operands.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="flags"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, params (int index, int value)[] flags) {
			ValidateOpType(type, nameof(type));
			ValidateFlags(flags, nameof(flags));
			Type = type;
			Flags = new FlagLetter[flags.Length];
			for (int i = 0; i < flags.Length; i++) {
				var (index, value) = flags[i];
				Flags[i] = new FlagLetter(index, value);
			}
		}
		/// <summary>
		///  Constructs a Flag Data operation with Flag Letter index operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="flags">The Flag Letter index operands.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="flags"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, params FlagLetter[] flags) {
			ValidateOpType(type, nameof(type));
			ValidateFlags(flags, nameof(flags));
			Type = type;
			Flags = new FlagLetter[flags.Length];
			Array.Copy(flags, Flags, flags.Length);
		}

		#endregion

		public override string ToString() {
			return $"{Type.ToOpString()} {Flags.ToFlagsString(Type)}";
		}

		#region Parse

		public static FlagOperation Parse(string s) {
			Match match = Regex.Match(s, @"^(?'op'[0~=+\-&|^])(?:\s+(?'letter'\S*))$");
			if (!match.Success)
				throw new FormatException("Expected \"op\" or \"op letters\", got \"{s}\"!");

			OpType type = ParseOpType(match.Groups["op"].Value);
			FlagLetter[] flags = ParseFlags(match.Groups["letter"].Value);

			return new FlagOperation(type, flags);
		}

		public static FlagLetter[] ParseFlags(string s) {
			if (s == null)
				throw new ArgumentNullException(nameof(s));
			if (s.Length != FlagData.Length)
				throw new FormatException(
					$"Flag Letter index must have a length of {FlagData.Length}, got {s.Length}!");

			List<FlagLetter> flags = new List<FlagLetter>();

			for (int i = 0; i < FlagData.Length; i++) {
				char c = s[i];
				if (c >= Letter.MinChar && c <= Letter.MaxChar)
					flags.Add(new FlagLetter(i, c));
				else if (c != '-' && c != '.')
					throw new FormatException(
						$"Flag Letter index only allows characters '-', '.', and 'A' - 'Z', got \"{s}\"!");
			}

			if (flags.Count == 0)
				throw new ArgumentException("Flag Letter index requires at least one letter!", nameof(s));

			return flags.ToArray();

		}

		public static OpType ParseOpType(string s) {
			switch (s) {
			case "0": return OpType.Zero;
			case "~": return OpType.Negate;
			case "=": return OpType.Set;
			case "+": return OpType.Add;
			case "-": return OpType.Sub;
			case "&": return OpType.Add;
			case "|": return OpType.Or;
			case "^": return OpType.Xor;
			default: throw new ArgumentException($"Invalid {nameof(OpType)} \"{s}\"!", nameof(s));
			}
		}

		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateOpType(OpType type, string paramName) {
			if (!Enum.IsDefined(typeof(OpType), type)) {
				throw new ArgumentException($"Invalid {nameof(OpType)} {type}!", paramName);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateIndex(int index, string paramName) {
			if (index < 0 || index >= FlagData.Length) {
				throw new ArgumentOutOfRangeException(paramName, index,
					$"Index must be between 0 and {(FlagData.Length-1)}, got {index}!");
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateFlags(Array array, string paramName) {
			if (array == null)
				throw new ArgumentNullException(paramName);
			if (array.Length == 0)
				throw new ArgumentException("Flags list must have at least one Flag Letter index!", paramName);
		}

		#endregion
	}
}
