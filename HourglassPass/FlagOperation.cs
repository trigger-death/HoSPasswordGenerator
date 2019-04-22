using System;
using System.Collections.Generic;

namespace HourglassPass {
	/// <summary>
	///  The type of operation to perform for a flag.
	/// </summary>
	public enum OpType {
		/// <summary>This is a <see cref="FlagMultiOperation"/>.</summary>
		Multi,
		/// <summary>Flag = 0</summary>
		Zero,
		/// <summary>Flag = ~Flag</summary>
		Negate,
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
	}
	/// <summary>
	///  A single Flag Data operation.
	/// </summary>
	public class FlagOperation : IFlagOperation {
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
		///  The operand to use in the operation type.
		/// </summary>
		public Letter Operand { get; }

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Flag Data operation for an <see cref="OpType"/> that does not use operands.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is an <see cref="OpType"/> that requires an operand.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index) {
			if (type != OpType.Zero && type != OpType.Negate)
				throw new ArgumentException($"Invalid {nameof(OpType)} {type}! " +
					$"Constructor without operand must be an {nameof(OpType)} that does not require one!",
					nameof(type));
			Type = type;
			if (index < 0 || index >= 5)
				throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and 4, got {index}!");
			Index = index;
		}
		/// <summary>
		///  Constructs a Flag Data operation with a letter operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="operand">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enumeration value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, Letter operand) {
			if (type == OpType.Multi || !Enum.IsDefined(typeof(OpType), type))
				throw new ArgumentException($"Invalid {nameof(OpType)} {type}!", nameof(type));
			Type = type;
			if (index < 0 || index >= 5)
				throw new ArgumentOutOfRangeException(nameof(index), index, $"Index must be between 0 and 4, got {index}!");
			Index = index;
			Operand = operand;
		}
		/// <summary>
		///  Constructs a Flag Data operation with a character operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="operand">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enumeration value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, char operand) : this(type, index, new Letter(operand)) { }
		/// <summary>
		///  Constructs a Flag Data operation with a numeric operand.
		/// </summary>
		/// <param name="type">The type of the operation.</param>
		/// <param name="index">The index of the letter to mutate.</param>
		/// <param name="operand">The operand value to use with the operation type.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="type"/> is <see cref="OpType.Multi"/> or an invalid enumeration value.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///  <paramref name="index"/> is less than 0 or greater than or equal to <see cref="FlagData.Length"/>.
		/// </exception>
		public FlagOperation(OpType type, int index, int operand) : this(type, index, new Letter(operand)) { }

		#endregion
	}
	/// <summary>
	///  A collection of Flag Data operations.
	/// </summary>
	public class FlagMultiOperation : IFlagOperation {
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
