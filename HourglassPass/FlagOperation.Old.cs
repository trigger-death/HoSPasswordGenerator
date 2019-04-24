using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HourglassPass {
	/// <summary>
	///  The type of operation to perform for a flag.
	/// </summary>
	public enum OpType : byte {
		/// <summary>This is a <see cref="FlagMultiOperation"/>.</summary>
		Multi,

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
	/// <summary>
	///  A flag operation that can either be <see cref="FlagOperation"/> or <see cref="FlagMultiOperation"/>.
	/// </summary>
	public interface IFlagOperation {
		/// <summary>
		///  The type of this operation. <see cref="OpType.Multi"/> is only valid with
		///  <see cref="FlagMultiOperation"/>.
		/// </summary>
		OpType Type { get; }
		/// <summary>
		///  The index of the flag to mutate. Ignored for <see cref="OpType.Multi"/>.
		/// </summary>
		int Index { get; }
		/// <summary>
		///  The operand to use in the operation type. Ignored for <see cref="OpType.Multi"/>.
		/// </summary>
		Letter Operand { get; }
		/// <summary>
		///  The ordered list of operations to perform on the flag data. Only for <see cref="OpType.Multi"/>.
		/// </summary>
		IReadOnlyList<FlagOperation> Operations { get; }
	}
	/// <summary>
	///  A single Flag Data operation.
	/// </summary>
	public sealed class FlagOperation : IFlagOperation {
		#region Constants

		/// <summary>
		///  Empty flag operations array that cannot be modified. Used when not <see cref="OpType.Multi"/>.
		/// </summary>
		private static readonly IReadOnlyList<FlagOperation> EmptyOperations = Array.AsReadOnly(new FlagOperation[0]);

		#endregion

		#region Fields

		/// <summary>
		///  The type of operation to perform.
		/// </summary>
		public OpType Type { get; }
		/// <summary>
		///  The index of the flag to mutate.
		/// </summary>
		public int Index { get; }
		/// <summary>
		///  The operand value to use in the operation type.
		/// </summary>
		public int Value { get; }
		/// <summary>
		///  The ordered list of operations to perform on the flag data. Only for <see cref="OpType.Multi"/>.
		/// </summary>
		public IReadOnlyList<FlagOperation> Operations { get; }

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Flag Data operation for an <see cref="OpType"/> that does not use operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enum value.-or-
		///  <paramref name="type"/> is an <see cref="OpType"/> that requires an operand.
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
			Index = index;
		}
		/// <summary>
		///  Constructs a Flag Data operation with a letter operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="letter">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, Letter letter) {
			ValidateOpType(type, nameof(type));
			ValidateIndex(index, nameof(index));
			Type = type;
			Index = index;
			Value = letter.Value;
		}
		/// <summary>
		///  Constructs a Flag Data operation with a character operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="character">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, char character) : this(type, index, new Letter(character)) { }
		/// <summary>
		///  Constructs a Flag Data operation with a numeric operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="value">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enum value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, int value) : this(type, index, new Letter(value)) { }

		/// <summary>
		///  Constructs a Flag Data operation with multiple mutations.
		/// </summary>
		/// <param name="ops">The operations to perform.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="ops"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="ops"/> has a length of zero.
		/// </exception>
		public FlagOperation(params FlagOperation[] ops) {
			if (ops == null)
				throw new ArgumentNullException(nameof(ops));
			if (ops.Length == 0) {
				throw new ArgumentException(
					$"Must pass at least one {nameof(FlagOperation)} when using multi constructor!", nameof(ops));
			}
			FlagOperation[] newOps = new FlagOperation[ops.Length];
			Array.Copy(ops, newOps, ops.Length);
			Type = OpType.Multi;
			Operations = Array.AsReadOnly(newOps);
		}

		#endregion

		#region Helpers

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ValidateOpType(OpType type, string paramName) {
			if (type == OpType.Multi) {
				throw new ArgumentException(
					$"Cannot use {nameof(OpType)} {nameof(OpType.Multi)} outside of multi constructor!", paramName);
			}
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

		#endregion
	}
	/// <summary>
	///  A collection of Flag Data operations.
	/// </summary>
	public sealed class FlagMultiOperation : IFlagOperation {
		#region Fields
		
		/// <summary>
		///  The ordered list of operations to perform on the flag data.
		/// </summary>
		public IReadOnlyList<FlagOperation> Operations { get; }

		#endregion

		#region Properties

		/// <summary>
		///  The type of operation to perform. Always <see cref="OpType.Multi"/>.
		/// </summary>
		public OpType Type => OpType.Multi;

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Flag Data operation with multiple mutations.
		/// </summary>
		/// <param name="ops">The operations to perform.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="ops"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		///  <paramref name="ops"/> has a length of zero.
		/// </exception>
		public FlagMultiOperation(params FlagOperation[] ops) {
			if (ops == null)
				throw new ArgumentNullException(nameof(ops));
			if (ops.Length == 0)
				throw new ArgumentException(
					$"Must pass at least one {nameof(FlagOperation)} to {nameof(FlagMultiOperation)}!", nameof(ops));
			FlagOperation[] newOps = new FlagOperation[ops.Length];
			Array.Copy(ops, newOps, ops.Length);
			Operations = Array.AsReadOnly(newOps);
		}

		#endregion
	}
}
