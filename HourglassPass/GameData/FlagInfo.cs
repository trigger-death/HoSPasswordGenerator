using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HourglassPass.GameData {
	public sealed class FlagInfo {

		public string Name { get; }
		public string Description { get; }
		public FlagOperation[] FlagOps { get; }

		public FlagLifetime Lifetime { get; }

		public bool HasFlagOps => FlagOps != null && FlagOps.Length != 0;
	}
}
