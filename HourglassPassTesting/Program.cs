using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HourglassPass.Testing {
	class Program {
		static void Main(string[] args) {
			Console.WriteLine(int.TryParse(null, out _));
			Console.WriteLine($"{23424323:N} {12341231221:D}");
			Console.WriteLine($"{Convert.ToString(14, 2).PadLeft(4, '0')}");
			SceneId sid = 0x30F;
			Console.WriteLine(new Password("BBBBYYYQ").Corrected());
			Console.WriteLine(new Password("ZZZZZYYY").ToString("PC"));
			Password.TryParse("ZZZZZYYY", out var pw);
			Console.WriteLine(pw);
			Console.WriteLine(pw.ToString("PC"));
			sid = "AZE";
			Console.WriteLine(sid.String);
			Console.WriteLine(sid.Value);
			Console.WriteLine(new PasswordSceneId(sid).Randomized());
			Console.ReadLine();
			TestParse("15", s => Letter.Parse(s));
			TestParse($"{0x3FF}", s => SceneId.Parse(s), "V");
			TestParse($"0x3FF", s => SceneId.Parse(s, PasswordStyles.Any), "V");
			TestParse($"0b0011 0000 0010", s => SceneId.Parse(s, PasswordStyles.Any), "PB1");
			TestParse($"0b1101 0011 0000 0100", s => PasswordFlagData.Parse(s, PasswordStyles.Any), "PB1");
			while (true) {
				try {
					Console.Write("> ");
					Password p = new Password(Console.ReadLine());
					Console.WriteLine($"   Password: {p}");
					//Password.TryParse(Console.ReadLine(), out p);
					//Console.WriteLine($"  Password: {p}");
					Console.WriteLine($" Normalized: {p:PN}");
					Console.WriteLine($" Randomized: {p:PR}");
					Console.WriteLine($"Hexidecimal: {p:PX}");
					//Console.WriteLine($"Scene: {p.Scene.Value.ToString().PadLeft(4)}, Flags: {p.Flags:B1} ({p.Flags:X})");
					Console.WriteLine($"Scene: {p.Scene,4:V}, Flags: {p.Flags:PB1} ({p.Flags:PX}) ({p.Flags:PD})");
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

		static void TestParse(string s, Func<string, IFormattable> parse, string format) {
			Console.WriteLine($"\"{s}\" -> {parse(s).ToString(format, CultureInfo.CurrentCulture)}");
		}
		static void TestParse(string s, Func<string, object> parse) {
			Console.WriteLine($"\"{s}\" -> {parse(s)}");
		}
	}
}
