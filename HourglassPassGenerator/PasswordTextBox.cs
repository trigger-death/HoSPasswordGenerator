using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HourglassPass.Generator {
	public sealed class PasswordTextBox : TextBox {
		#region Constants

		private const string InvalidText = "--------";
		private const char Dot = '᛫';

		#endregion

		#region Dependency Properties
		
		private static readonly DependencyPropertyKey IsValidPasswordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsValidPassword", typeof(bool), typeof(PasswordTextBox),
				new FrameworkPropertyMetadata(false));

		private static readonly DependencyPropertyKey NormalizedTextPropertyKey =
			DependencyProperty.RegisterReadOnly("NormalizedText", typeof(string), typeof(PasswordTextBox),
				new FrameworkPropertyMetadata(InvalidText));
		
		public static readonly DependencyProperty IsValidPasswordProperty = IsValidPasswordPropertyKey.DependencyProperty;
		public static readonly DependencyProperty NormalizedTextProperty = NormalizedTextPropertyKey.DependencyProperty;
		
		public bool IsValidPassword {
			get => (bool) GetValue(IsValidPasswordProperty);
			private set => SetValue(IsValidPasswordPropertyKey, value);
		}
		public string NormalizedText {
			get => (string) GetValue(NormalizedTextProperty);
			private set => SetValue(NormalizedTextPropertyKey, value);
		}

		/*private static void OnPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox textBox = (PasswordTextBox) sender;
			button.image.Source = button.Source;

			button.CoerceValue(ContentProperty);
		}*/
		private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox textBox = (PasswordTextBox) sender;
			string text = textBox.Text;
			Password p = textBox.Password;
			if (p != null) {
				p.Normalize();
				textBox.NormalizedText = p.String;
			}
			else {
				textBox.NormalizedText = InvalidText;
			}
			textBox.IsValidPassword = p != null;
		}
		#endregion

		#region Static Constructor

		static PasswordTextBox() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordTextBox),
				new FrameworkPropertyMetadata(typeof(PasswordTextBox)));
			TextProperty.OverrideMetadata(typeof(PasswordTextBox),
				new FrameworkPropertyMetadata(InvalidText, OnTextChanged));
			CharacterCasingProperty.OverrideMetadata(typeof(PasswordTextBox),
				new FrameworkPropertyMetadata(CharacterCasing.Upper));
		}

		#endregion

		#region Properties

		public Password Password {
			get {
				Password.TryParse(Text, out Password password);
				return password;
			}
			set=> Text = Password.String;
		}

		#endregion

		/*public PasswordTextBox() {
			TextInput
		}*/

		/*protected override void OnTextChanged(TextChangedEventArgs e) {
			Text = Text.ToUpper();
		}*/

		/*protected override void OnTextInput(TextCompositionEventArgs e) {
			string text = e.Text
			if (e.)
			base.OnTextInput(e);
		}*/
	}
}
