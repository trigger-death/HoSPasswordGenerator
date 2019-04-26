using System;

namespace HourglassPass.GameData {
	/// <summary>
	///  A structure that represents a start and end scene as the lifetime of a flag's usage.
	/// </summary>
	public struct FlagLifetime {
		#region Fields

		/// <summary>
		///  Gets the Scene ID where the flag first appears.
		/// </summary>
		public SceneId StartSceneId { get; }
		/// <summary>
		///  Gets the Scene ID that wipes the flag or changes its usage.
		/// </summary>
		public SceneId EndSceneId { get; }

		#endregion

		#region Properties

		/// <summary>
		///  Gets the number of Scenes between the <see cref="StartSceneId"/> and <see cref="EndSceneId"/>.
		/// </summary>
		public int Count => EndSceneId.Value - StartSceneId.Value;

		#endregion

		#region Constructors

		/// <summary>
		///  Constructs a Flag Lifetime with a starting and ending Scene ID.
		/// </summary>
		/// <param name="startSceneId">The Scene ID where the flag first appears.</param>
		/// <param name="endSceneId">The Scene ID that wipes the flag or changes its usage.</param>
		/// 
		/// <exception cref="ArgumentException">
		///  <paramref name="startSceneId"/> is greater than <paramref name="endSceneId"/>.
		/// </exception>
		public FlagLifetime(SceneId startSceneId, SceneId endSceneId) {
			if (startSceneId > endSceneId)
				throw new ArgumentException($"{nameof(startSceneId)} cannot be greater than {nameof(endSceneId)}, " +
					$"got {startSceneId} and {endSceneId}!");
			StartSceneId = startSceneId;
			EndSceneId = endSceneId;
		}

		#endregion
	}
}
