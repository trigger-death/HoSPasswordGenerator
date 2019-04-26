using System;
using System.ComponentModel;

namespace HourglassPass {
	[Flags]
	public enum PasswordStyles : uint {
		None = 0,
		Any = AllowSpecifier,
		PasswordOrValue = 1,
		Password = 2,
		Value = 3,
		Binary = 4,
		Hex = 5,
		Hexidecimal = Hex,

		AllowSpecifier = (1 << 8),

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		FlagsMask = 0xFFFFFF00,
	}
}
