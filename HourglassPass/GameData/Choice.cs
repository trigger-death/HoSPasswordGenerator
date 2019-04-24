using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HourglassPass.GameData {
	public class Choice {
		public string Text { get; }
		public FlagOperation[] FlagOps { get; }

		public bool HasFlagOps => FlagOps != null && FlagOps.Length != 0;
	}
}
