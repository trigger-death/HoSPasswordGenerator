using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HourglassPass.Generator {
	public sealed class PasswordTextBox3 : TextBox {
		#region Constants

		//private const string InvalidText = "--------";
		public const string InvalidText = "••••••••";
		private const char Dot = '᛫';
		private const char Bullet = '•';

		#endregion

		#region Dependency Properties

		private static readonly DependencyPropertyKey PasswordPropertyKey =
			DependencyProperty.RegisterReadOnly("Password", typeof(Password), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(null));

		private static readonly DependencyPropertyKey IsValidPasswordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsValidPassword", typeof(bool), typeof(PasswordTextBox3),
				new FrameworkPropertyMetadata(false));

		private static readonly DependencyPropertyKey NormalizedTextPropertyKey =
			DependencyProperty.RegisterReadOnly("NormalizedText", typeof(string), typeof(PasswordTextBox3),
				new FrameworkPropertyMetadata(InvalidText));

		public static readonly DependencyProperty InvalidForegroundProperty =
			DependencyProperty.Register("InvalidForeground", typeof(Brush), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(Brushes.IndianRed));
		public static readonly DependencyProperty InvalidBackgroundProperty =
			DependencyProperty.Register("InvalidBackground", typeof(Brush), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(Brushes.White));

		public static readonly DependencyProperty PasswordProperty = PasswordPropertyKey.DependencyProperty;
		public static readonly DependencyProperty IsValidPasswordProperty = IsValidPasswordPropertyKey.DependencyProperty;
		public static readonly DependencyProperty NormalizedTextProperty = NormalizedTextPropertyKey.DependencyProperty;

		[Category("Appearance")]
		public Brush InvalidForeground {
			get => (Brush) GetValue(InvalidForegroundProperty);
			set => SetValue(InvalidForegroundProperty, value);
		}
		[Category("Appearance")]
		public Brush InvalidBackground {
			get => (Brush) GetValue(InvalidBackgroundProperty);
			set => SetValue(InvalidBackgroundProperty, value);
		}

		[Category("Hourglass")]
		public Password Password {
			get => (Password) GetValue(PasswordProperty);
			private set => SetValue(PasswordPropertyKey, value);
		}
		[Category("Hourglass")]
		public bool IsValidPassword {
			get => (bool) GetValue(IsValidPasswordProperty);
			private set => SetValue(IsValidPasswordPropertyKey, value);
		}
		[Localizability(LocalizationCategory.Text)]
		public string NormalizedText {
			get => (string) GetValue(NormalizedTextProperty);
			private set => SetValue(NormalizedTextPropertyKey, value);
		}

		/*private static void OnPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox3 textBox = (PasswordTextBox3) sender;
			button.image.Source = button.Source;

			button.CoerceValue(ContentProperty);
		}*/
		private static void OnTextChanged2(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox3 textBox = (PasswordTextBox3) sender;
			string text = textBox.Text;
			if (Password.TryParse(text, out Password p)) {
				textBox.NormalizedText = p.Normalized().String;
			}
			else {
				textBox.NormalizedText = InvalidText;
			}
			Console.WriteLine($"1: {(textBox.Password == p)}");
			if (textBox.Password != null) {
				Console.WriteLine($"2: {textBox.Password.Equals(p)}");
			}
			else if (p != null) {
				Console.WriteLine($"3: {p.Equals(textBox.Password)}");
			}
			textBox.Password = p;
			textBox.IsValidPassword = p != null;
		}
		protected override void OnTextChanged(TextChangedEventArgs e) {
			base.OnTextChanged(e);
			UpdateText();
		}
		private void UpdateText() {
			int realLength = RealLength;
			if (Text.Length < Password.Length || (realLength < Text.Length && Text.Length > Password.Length)) {
				int start = SelectionStart;
				int length = SelectionLength;
				Text = Text.Substring(0, realLength) + new string(Bullet, Math.Max(0, Password.Length - realLength));
				UpdateSelection(start, length, true);
			}
		}
		private void UpdateSelection() {
			UpdateSelection(SelectionStart, SelectionLength);
		}
		private void UpdateSelection(int start, int length, bool force = false) {
			int realLength = RealLength;
			if (realLength != Text.Length) {
				if (start > realLength) {
					Select(realLength, 0);
				}
				else if (start + length > realLength) {
					Select(start, realLength - start);
				}
				else if (force) {
					Select(start, length);
				}
			}
			else if (force) {
				Select(start, length);
			}
		}
		protected override void OnPreviewKeyDown(KeyEventArgs e) {
			base.OnPreviewKeyDown(e);
			// Prevent space, beacuse it's not triggered by OnPreviewTextInput
			e.Handled = (e.Key == Key.Space || e.Key == Key.Tab);
		}
		protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
			base.OnPreviewTextInput(e);
			e.Handled = !IsTextAllowed(e.Text);
		}
		private static bool IsTextAllowed(string s) {
			return Regex.IsMatch(s, @"^[A-Za-z]*$");
		}
		private void PastingHandler(object sender, DataObjectPastingEventArgs e) {
			if (e.DataObject.GetDataPresent(typeof(string))) {
				string text = (string) e.DataObject.GetData(typeof(string));
				if (!IsTextAllowed(text)) e.CancelCommand();
			}
			else e.CancelCommand();
		}
		/*protected override void OnTextInput(TextCompositionEventArgs e) {
			base.OnTextInput(e);
			
		}*/
		protected override void OnSelectionChanged(RoutedEventArgs e) {
			UpdateSelection();
		}

		public int RealLength {
			get {
				int index = Text.IndexOf(Bullet);
				return (index != -1 ? index : Text.Length);
			}
		}

		#endregion

		#region Static Constructor

		static PasswordTextBox3() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordTextBox3),
				new FrameworkPropertyMetadata(typeof(PasswordTextBox3)));
			TextProperty.OverrideMetadata(typeof(PasswordTextBox3),
				new FrameworkPropertyMetadata(InvalidText, OnTextChanged2));
			CharacterCasingProperty.OverrideMetadata(typeof(PasswordTextBox3),
				new FrameworkPropertyMetadata(CharacterCasing.Upper));
		}


		public PasswordTextBox3() {
			DataObject.AddPastingHandler(this, PastingHandler);
		}
		// Use the DataObject.Pasting Handler  
		#endregion

		#region Properties

		/*public Password Password {
			get {
				Password.TryParse(Text, out Password password);
				return password;
			}
			set=> Text = Password.String;
		}*/

		#endregion

		/*public PasswordTextBox3() {
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
	/*public class TextBoxRestriction : DependencyObject {
		// RestrictDeleteTo:  Set this to the characters that may be deleted
		public static string GetRestrictDeleteTo(DependencyObject obj) { return (string) obj.GetValue(RestrictDeleteToProperty); }
		public static void SetRestrictDeleteTo(DependencyObject obj, string value) { obj.SetValue(RestrictDeleteToProperty, value); }
		public static readonly DependencyProperty RestrictDeleteToProperty = DependencyProperty.RegisterAttached("RestrictDeleteTo", typeof(string), typeof(TextBoxRestriction), new PropertyMetadata {
			PropertyChangedCallback = (obj, e) =>
			{
				var box = (TextBox) obj;
				box.TextChanged += (obj2, changeEvent) =>
				{
					var oldText = GetUnrestrictedText(box);
					var allowedChars = GetRestrictDeleteTo(box);
					if (box.Text==oldText || allowdChars==null) return;

					foreach (var change in changeEvent.Changes)
						if (change.RemovedLength>0) {
							string deleted = box.Text.Substring(change.Offset, change.RemovedLength);
							if (deleted.Any(ch => !allowedChars.Contains(ch)))
								box.Text = oldText;
						}
					SetUnrestrictedText(box, box.Text);
				};
			}
		});

		// UnrestrictedText:  Bind or access this property to update the Text property bypassing all restrictions
		public static string GetUnrestrictedText(DependencyObject obj) { return (string) obj.GetValue(UnrestrictedTextProperty); }
		public static void SetUnrestrictedText(DependencyObject obj, string value) { obj.SetValue(UnrestrictedTextProperty, value); }
		public static readonly DependencyProperty UnrestrictedTextProperty = DependencyProperty.RegisterAttached("UnrestrictedText", typeof(string), typeof(TextBoxRestriction), new PropertyMetadata {
			DefaultValue = "",
			PropertyChangedCallback = (obj, e) =>
			{
				var box = (TextBox) obj;
				box.Text = (string) e.NewValue;
			}
		});

	}*/
}
