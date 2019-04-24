using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace HourglassPass.Generator {
	public sealed class PasswordTextBox2 : Control {
		#region Constants

		//private const string InvalidText = "--------";
		private const string InvalidText = "••••••••";
		private const char Dot = '᛫';
		private const char Bullet = '•';

		#endregion

		#region Dependency Properties

		/*public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnTextChanged));*/
		public static readonly DependencyProperty TextProperty =
			TextBox.TextProperty.AddOwner(typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnTextChanged));

		private static readonly DependencyPropertyKey PasswordPropertyKey =
			DependencyProperty.RegisterReadOnly("Password", typeof(Password), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(null));

		private static readonly DependencyPropertyKey IsValidPasswordPropertyKey =
			DependencyProperty.RegisterReadOnly("IsValidPassword", typeof(bool), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(false));

		/*private static readonly DependencyPropertyKey OverlayTextPropertyKey =
			DependencyProperty.RegisterReadOnly("OverlayText", typeof(string), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(new string(Bullet, Password.Length)));*/

		private static readonly DependencyPropertyKey NormalizedTextPropertyKey =
			DependencyProperty.RegisterReadOnly("NormalizedText", typeof(string), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(InvalidText));

		public static readonly DependencyProperty OverlayForegroundProperty =
			DependencyProperty.Register("OverlayForeground", typeof(Brush), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(Brushes.IndianRed, OnOverlayForegroundChanged));

		public static readonly DependencyProperty InvalidForegroundProperty =
			DependencyProperty.Register("InvalidForeground", typeof(Brush), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(Brushes.IndianRed));
		public static readonly DependencyProperty InvalidBackgroundProperty =
			DependencyProperty.Register("InvalidBackground", typeof(Brush), typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(Brushes.White));

		public static readonly DependencyProperty PasswordProperty = PasswordPropertyKey.DependencyProperty;
		public static readonly DependencyProperty IsValidPasswordProperty = IsValidPasswordPropertyKey.DependencyProperty;
		public static readonly DependencyProperty NormalizedTextProperty = NormalizedTextPropertyKey.DependencyProperty;

		[Localizability(LocalizationCategory.Text)]
		public string Text {
			get => (string) GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
		[Localizability(LocalizationCategory.Text)]
		public string NormalizedText {
			get => (string) GetValue(NormalizedTextProperty);
			private set => SetValue(NormalizedTextPropertyKey, value);
		}
		/*[Localizability(LocalizationCategory.Text)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string OverlayText {
			get => (string) GetValue(OverlayTextProperty);
			private set => SetValue(OverlayTextPropertyKey, value);
		}*/

		[Category("Appearance")]
		public Brush OverlayForeground {
			get => (Brush) GetValue(OverlayForegroundProperty);
			set => SetValue(OverlayForegroundProperty, value);
		}
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

		public Password Password {
			get => (Password) GetValue(PasswordProperty);
			private set => SetValue(PasswordPropertyKey, value);
		}
		public bool IsValidPassword {
			get => (bool) GetValue(IsValidPasswordProperty);
			private set => SetValue(IsValidPasswordPropertyKey, value);
		}

		/*private static void OnPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox2 textBox = (PasswordTextBox2) sender;
			button.image.Source = button.Source;

			button.CoerceValue(ContentProperty);
		}*/
		/*private static void OnPasswordChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox2 textBox = (PasswordTextBox2) sender;
			Password p = textBox.Password;
			if (p == null) {
				textBox.Text = string.Empty;
			}
			else {
				textBox.Text = textBox.Password.String;
			}
		}*/
		private static void OnOverlayForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox2 textBox = (PasswordTextBox2) sender;
			textBox.UpdateOverlayText();
		}
		private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
			PasswordTextBox2 textBox = (PasswordTextBox2) sender;
			textBox.UpdateText();
		}
		#endregion

		#region Fields

		private TextBox PART_TextBox;
		private TextBlock PART_Overlay;

		#endregion

		#region Static Constructor

		static PasswordTextBox2() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordTextBox2),
				new FrameworkPropertyMetadata(typeof(PasswordTextBox2)));
			//TextBox.TextProperty.AddOwner()
			//TextProperty.OverrideMetadata(typeof(PasswordTextBox2),
			//	new FrameworkPropertyMetadata(InvalidText, OnTextChanged));
		}

		#endregion

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			PART_TextBox = (TextBox) GetTemplateChild("PART_TextBox");
			PART_Overlay = (TextBlock) GetTemplateChild("PART_Overlay");

			var sv = FindVisualChild<ScrollViewer>(PART_TextBox);
			PART_TextBox.TextChanged += PART_TextBox_TextChanged;
			//Loaded += PasswordTextBox2_Loaded;
			PART_TextBox.Loaded += PART_TextBox_Initialized;
		}

		private void PART_TextBox_Initialized(object sender, EventArgs e) {
			var sv = FindVisualChild<ScrollViewer>(PART_TextBox);
			var textBoxBlock = FindVisualChild<ScrollViewer>(PART_TextBox);
			
			Point pt = ((Visual) sv.Content).TransformToAncestor(PART_TextBox).Transform(new Point(0, 0));

			textBoxBlock.Margin = new Thickness(pt.X, pt.Y, 0, 0);
			//this.Initialized
			//PART_TextBox.TextChanged += PART_TextBox_TextChanged;
			UpdateText();
		}

		private void PasswordTextBox2_Loaded(object sender, RoutedEventArgs e) {
			var sv = FindVisualChild<ScrollViewer>(PART_TextBox);
			TextBlock textBoxBlock = FindVisualChild<TextBlock>(PART_TextBox);

			Point pt = textBoxBlock.TransformToAncestor(this).Transform(new Point(0, 0));

			textBoxBlock.Margin = new Thickness(pt.X, pt.Y, 0, 0);
			//this.Initialized
			//PART_TextBox.TextChanged += PART_TextBox_TextChanged;
			UpdateText();
		}

		internal static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
			if (parent == null) {
				return null;
			}

			DependencyObject parentObject = parent;
			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++) {
				DependencyObject childObject = VisualTreeHelper.GetChild(parentObject, i);
				if (childObject == null) {
					continue;
				}

				var child = childObject as T;
				return child ?? FindVisualChild<T>(childObject);
			}

			return null;
		}

		private void PART_TextBox_TextChanged(object sender, TextChangedEventArgs e) {
			Text = PART_TextBox.Text;
		}

		private void UpdateText() {
			if (PART_TextBox == null || PART_Overlay == null)
				return;
			PART_TextBox.Text = Text;
			if (Password.TryParse(Text, out Password p)) {
				p.Normalize();
				NormalizedText = p.String;
			}
			else {
				NormalizedText = InvalidText;
			}
			UpdateOverlayText();
			IsValidPassword = p != null;
		}

		private void UpdateOverlayText() {
			if (PART_TextBox == null || PART_Overlay == null)
				return;
			PART_Overlay.Inlines.Clear();
			if (Text.Length < Password.Length) {
				PART_Overlay.Inlines.Add(new Run(Text) {
					Foreground = OverlayForeground,
					//Foreground = Brushes.Transparent,
					Background = Brushes.Transparent,
				});
				PART_Overlay.Inlines.Add(new Run(new string(Bullet, Password.Length - Text.Length)) {
					Foreground = OverlayForeground,
					Background = Brushes.Transparent,
				});
			}
			Console.WriteLine(PART_TextBox.HorizontalOffset);
			Console.WriteLine(PART_TextBox.VerticalOffset);
		}

		/*public PasswordTextBox2() {
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
