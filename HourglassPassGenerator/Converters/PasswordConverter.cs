using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace HourglassPass.Generator.Converters {
	public sealed class PasswordConverter : MarkupExtension, IValueConverter {
		public static readonly PasswordConverter Instance = new PasswordConverter();

		public override object ProvideValue(IServiceProvider serviceProvider) => Instance;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			Password p = null;
			if (value is Password pass)
				p = pass;
			else if (value is string s && Password.IsValidString(s))
				p = new Password(s);

			return p?.ToString() ?? PasswordTextBox3.InvalidText;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
