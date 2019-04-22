using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HourglassPass.Testing {
	class Program {
		static void Main(string[] args) {
			Console.WriteLine($"{23424323:N} {12341231221:D}");
			Console.WriteLine($"{Convert.ToString(14, 2).PadLeft(4, '0')}");
			while (true) {
				try {
					Console.Write("> ");
					Password p = new Password(Console.ReadLine());
					Console.WriteLine($"  Password: {p}");
					Console.WriteLine($"Normalized: {p.Normalized()}");
					Console.WriteLine($"Randomized: {p.Randomized()}");
					//Console.WriteLine($"Scene: {p.Scene.Value.ToString().PadLeft(4)}, Flags: {p.Flags:B1} ({p.Flags:X})");
					Console.WriteLine($"Scene: {p.Scene,4:D}, Flags: {p.Flags:B1} ({p.Flags:X})");
				} catch (Exception ex) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(ex);
					Console.ResetColor();
				}
				Console.WriteLine();
			}
			/*Password p1 = Password.Initial;
			Password p2 = Password.Initial;
			Console.WriteLine(Password.Initial);
			Console.WriteLine(Password.Initial.Randomized());
			Console.WriteLine($"Scene: {p1.Scene.Value.ToString().PadLeft(4)}, Flags: {p1.Flags.Value:X5}");
			Console.WriteLine($"Scene: {p2.Scene.Value.ToString().PadLeft(4)}, Flags: {p2.Flags.Value:X5}");
			Console.ReadLine();*/
		}
	}
}
