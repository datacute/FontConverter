using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DatacuteFontConverterUI
{
	public partial class LocalizedTextBlock
	{
		public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(LocalizedTextBlock));

		public string Caption
		{
			get => (string)GetValue(CaptionProperty);
			set => SetValue(CaptionProperty, value);
		}

		public static readonly DependencyProperty LocalizedTextProperty = DependencyProperty.Register("LocalizedText", typeof(string), typeof(LocalizedTextBlock));
		public string LocalizedText
		{
			get => (string)GetValue(LocalizedTextProperty);
			set => SetValue(LocalizedTextProperty, value);
		}

		
		/// <summary>
		/// Gets or sets the Label which is displayed next to the field
		/// </summary>
		public IDictionary<CultureInfo, string> Translations
		{
			get => (IDictionary<CultureInfo, string>)GetValue(TranslationsProperty);
			set => SetValue(TranslationsProperty, value);
		}

		public static DependencyProperty TranslationsProperty =
			DependencyProperty.Register("Translations", typeof(IDictionary<CultureInfo, string>),
				typeof(LocalizedTextBlock), new PropertyMetadata((TranslationsChangedCallback)));

		private static void TranslationsChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs args)
		{
			var context = (LocalizedTextBlock) o;
			context.LanguageItems.Clear();
			var dictionary = (IDictionary<CultureInfo, string>) args.NewValue;
			foreach (var language in dictionary.Keys.OrderBy(ci => ci.DisplayName))
			{
				context.LanguageItems.Add(language);
			}

			var selectedItem = SelectClosestLanguage(context.LanguageItems);
			if (selectedItem == null)
			{
				context.LayoutRoot.Visibility = Visibility.Collapsed;
				context.LocalizedText = null;
			}
			else
			{
				context.LayoutRoot.Visibility = Visibility.Visible;
				context.LocalizedText = context.Translations[selectedItem];
			}
			if (context.LanguageItems.Count < 2) selectedItem = null;
			context.LanguageSelection.SelectedItem = selectedItem;
		}

		private static CultureInfo SelectClosestLanguage(ObservableCollection<CultureInfo> languageItems)
		{
			var uiLanguage = languageItems.FirstOrDefault(
				                 li => li.IetfLanguageTag == CultureInfo.CurrentUICulture.IetfLanguageTag) ??
			                 languageItems.FirstOrDefault(
				                 li => li.TwoLetterISOLanguageName ==
				                       CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ??
			                 languageItems.FirstOrDefault();
			return uiLanguage;
		}

		public ObservableCollection<CultureInfo> LanguageItems { get; set; } = new();
		public LocalizedTextBlock()
		{
			InitializeComponent();
			LayoutRoot.DataContext = this;
		}

		private void LanguageSelection_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedItem = LanguageSelection.SelectedItem as CultureInfo;
			if (selectedItem == null)
				return;

			LocalizedText = Translations[selectedItem];
		}
	}
}