using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DatacuteFontConverter;
using DatacuteFontConverterUI.ConversionRights;
using Application = System.Windows.Application;

namespace DatacuteFontConverterUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private double _emSize;
		private double _previewScale;
		private readonly UserSettings _userSettings;
		public NamedCollection<XmlLanguage> FontFamilyNameLanguageItems { get; } = new(Describer.XmlLanguage);
		public NamedCollection<XmlLanguage> TypefaceNameLanguageItems { get; } = new(Describer.XmlLanguage);
		public NamedCollection<FontFamily> FontFamilyItems { get; } = new(Describer.FontFamily);
		public NamedCollection<Typeface> TypefaceItems { get; } = new(Describer.EmptyString);
		public NamedCollection<UnicodeCategory> CategoryItems { get; } = new(Describer.UnicodeCategoryName);
		public NamedCollection<CodepointRangeProperty> UnicodeBlockItems { get; } = new(Describer.CodepointRange);
		public NamedCollection<CodepointRangeProperty> CodepointRangeItems { get; } = new(Describer.CodepointRange);
		public NamedCollection<CodepointRangeProperty> ConversionCodepointRangeItems { get; } = new(Describer.CodepointRange);
		public NamedCollection<FontStretch> FontStretchItems { get; } = new(Describer.FontStretch);
		public NamedCollection<FontWeight> FontWeightItems { get; } = new(Describer.FontWeight);
		public NamedCollection<FontStyle> FontStyleItems { get; } = new(Describer.FontStyle);

		public double EmSize
		{
			get => _emSize;
			set
			{
				_emSize = Math.Round(value, 2);
				UpdateSampleTextTypeface();
			}
		}

		public double PreviewScale
		{
			get => _previewScale;
			set
			{
				_previewScale = Math.Round(value, 2);
				UpdateSampleTextTypeface();
			}
		}

		public MainWindow()
		{

			_userSettings = UserSettings.LoadUserSettings();
			_emSize = _userSettings.EmSize;
			_previewScale = _userSettings.PreviewScale;

			InitializeComponent();
			this.DataContext = this;
			SetupFilters();
			SetupTypefaceCharacteristics();
			SetupNamedCollections();

			SaveDirectory.Text = _userSettings.SaveDirectory;
			SampleTextInTypeface.Text = _userSettings.SampleText;

			foreach (var range in _userSettings.DesiredCodepointRanges)
			{
				ConversionCodepointRangeItems.Add(range);
			}

			PreviewEnabled.IsChecked = true;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_userSettings.SaveDirectory = SaveDirectory.Text;
			_userSettings.SampleText = SampleTextInTypeface.Text;
			_userSettings.EmSize = _emSize;
			_userSettings.DesiredCodepointRanges = new List<CodepointRangeProperty>(ConversionCodepointRangeItems.Select(codepointRangeItem => codepointRangeItem.Item)).ToArray();

			_userSettings.Save();
			base.OnClosing(e);
		}

		private void SetupNamedCollections()
		{
			SetupFontFamilyNameLanguageItems();

			FontFamilyItems.Clear();

			foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
			{
				if (RestrictToConvertible?.IsChecked ?? false)
				{
					foreach (var typeface in fontFamily.GetTypefaces())
					{
						if (RestrictedLicense(typeface)) 
							continue;
						FontFamilyItems.Add(fontFamily);
						break;
					}
				}
				else
				{
					FontFamilyItems.Add(fontFamily);
				}
			}

			SelectClosestFontFamilyLanguage();
		}

		private void SetupFilters()
		{
			CategoryItems.Add(new NamedItem<UnicodeCategory>((UnicodeCategory) (-1), "All"));
			foreach (var unicodeCategory in Enum.GetValues<UnicodeCategory>())
				CategoryItems.Add(unicodeCategory);

			List<NamedItem<CodepointRangeProperty>> ranges = new();
			foreach (var rangePropertyInfo in typeof(UnicodeRanges).GetProperties())
			{
				var range = rangePropertyInfo.GetValue(null) as UnicodeRange;
				if (range == null) continue;
				if (range.Length == 0) continue;
				var codepointRange = new CodepointRangeProperty(range.FirstCodePoint, range.FirstCodePoint + range.Length - 1);
				//var name = $"{Describer.UnicodeRangePropertyInfo(rangePropertyInfo)} : {Describer.CodepointRange(codepointRange)}";
				var name = Describer.UnicodeRangePropertyInfo(rangePropertyInfo);
				ranges.Add(new NamedItem<CodepointRangeProperty>(codepointRange, name));
			}

			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x0870, 0x089F), "Arabic Extended B")); // Unicode 14.0
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x2FE0, 0x2FEF), "Unallocated")); // as of Unicode 14.0
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xD800, 0xDB7F), "UTF-16 High Surrogates"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xDB80, 0xDBFF), "UTF-16 Private Use Surrogates"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xDC00, 0xDFFF), "UTF-16 Low Surrogates"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xE000, 0xF8FF), "Private Use Area"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x10000, 0x1FFFF), "Supplementary Multilingual Plane")); // 145 blocks as of Unicode 14.0
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x20000, 0x2FFFF), "Supplementary Ideographic Plane")); // 6 blocks as of Unicode 14.0
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x30000, 0x3134F), "CJK Unified Ideographs Extension G"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x31350, 0x3FFFF), "Tertiary Ideographic Plane"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0x40000, 0xDFFFF), "Unassigned"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xE0000, 0xE007F), "Tags"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xE0080, 0xE00FF), "Unassigned"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xE0100, 0xE01EF), "Variation Selectors Supplement"));
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xE01F0, 0xEFFFF), "Supplementary Special-purpose Plane")); // 2 blocks as of Unicode 14.0
			ranges.Add(new NamedItem<CodepointRangeProperty>(new CodepointRangeProperty(0xF0000, 0x10FFFF), "Supplementary Private Use Area Planes"));

			foreach (var namedItem in ranges.OrderBy(ni => ni.Item.From))
			{
				UnicodeBlockItems.Add(namedItem);
			}
		}

		private void SetupTypefaceCharacteristics()
		{
			SetupFontStretches();
			SetupFontWeights();
			SetupFontStyles();
		}

		private void SetupFontStretches()
		{
			FontStretchItems.Add(FontStretches.UltraCondensed);
			FontStretchItems.Add(FontStretches.ExtraCondensed);
			FontStretchItems.Add(FontStretches.Condensed);
			FontStretchItems.Add(FontStretches.SemiCondensed);
			FontStretchItems.Add(FontStretches.Normal);
			FontStretchItems.Add(FontStretches.SemiExpanded);
			FontStretchItems.Add(FontStretches.Expanded);
			FontStretchItems.Add(FontStretches.ExtraExpanded);
			FontStretchItems.Add(FontStretches.UltraExpanded);
		}

		private void SetupFontWeights()
		{
			FontWeightItems.Add(FontWeights.Thin);
			FontWeightItems.Add(FontWeights.ExtraLight);
			FontWeightItems.Add(FontWeights.Light);
			FontWeightItems.Add(FontWeight.FromOpenTypeWeight(350)); // SemiLight
			FontWeightItems.Add(FontWeights.Normal);
			FontWeightItems.Add(FontWeights.Medium);
			FontWeightItems.Add(FontWeights.SemiBold);
			FontWeightItems.Add(FontWeights.Bold);
			FontWeightItems.Add(FontWeights.ExtraBold);
			FontWeightItems.Add(FontWeights.Black);
			FontWeightItems.Add(FontWeights.ExtraBlack);
		}

		private void SetupFontStyles()
		{
			FontStyleItems.Add(FontStyles.Normal);
			FontStyleItems.Add(FontStyles.Oblique);
			FontStyleItems.Add(FontStyles.Italic);
		}

		private void SetupFontFamilyNameLanguageItems()
		{
			FontFamilyNameLanguageItems.Clear();
			var languages = new List<XmlLanguage>();
			foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
			{
				if (fontFamily.FamilyNames?.Keys == null)
					continue;
				languages.AddRange(fontFamily.FamilyNames.Keys);
			}

			foreach (XmlLanguage language in languages.Distinct())
				FontFamilyNameLanguageItems.Add(language);

		}

		private void SelectClosestFontFamilyLanguage()
		{
			var uiLanguage = SelectClosestLanguage(FontFamilyNameLanguageItems);
			//if (uiLanguage != null) FontFamilyLanguage.SelectedItem = uiLanguage;
			FontFamilyLanguage_OnSelectionChanged(null, null);
		}

		private static NamedItem<XmlLanguage>? SelectClosestLanguage(NamedCollection<XmlLanguage> languageItems)
		{
			var uiLanguage = languageItems.FirstOrDefault(
				                 li => li.Item.IetfLanguageTag == CultureInfo.CurrentUICulture.IetfLanguageTag) ??
			                 languageItems.FirstOrDefault(
				                 li => li.Item.GetSpecificCulture().TwoLetterISOLanguageName ==
				                       CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ??
			                 languageItems.FirstOrDefault();
			return uiLanguage;
		}

		
		private void FontFamilyLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//var currentLanguage = FontFamilyLanguage.SelectedItem as NamedItem<XmlLanguage>;
			//if (currentLanguage == null) return;

			FontFamilyNameSupplier namer = new(XmlLanguage.GetLanguage("en-us"));
			FontFamilyItems.RenameAll(namer.GetName);

			var sortedFontFamilyItems = Resources["SortedFontFamilyItems"] as CollectionViewSource;
			if (sortedFontFamilyItems != null)
			{
				var fontFamilyItemsListView = sortedFontFamilyItems.View as ListCollectionView;
				if (fontFamilyItemsListView != null)
					fontFamilyItemsListView.CustomSort = new NameComparer<FontFamily>();
			}
		}

		private void FontFamiliesListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RelistTypefacesForFont();
		}

		private void RelistTypefacesForFont()
		{
			var currentFontFamily = FontFamiliesListBox?.SelectedItem as NamedItem<FontFamily>;
			if (currentFontFamily == null) return;

			TypefaceNameLanguageItems.Clear();

			var previouslySelectedStretch = FontStretchList?.SelectedItem as NamedItem<FontStretch>;
			var previouslySelectedWeight = FontWeightList?.SelectedItem as NamedItem<FontWeight>;
			var previouslySelectedStyle = FontStyleList?.SelectedItem as NamedItem<FontStyle>;

			TypefaceItems.Clear();
			TypefaceNameSupplier typefaceNameSupplier = null;
			var restrictToNonSimulated = RestrictToNonSimulated?.IsChecked ?? true;

			foreach (var typeface in currentFontFamily.Item.GetTypefaces())
			{
				if (restrictToNonSimulated && (typeface.IsBoldSimulated || typeface.IsObliqueSimulated))
					continue;

				foreach (XmlLanguage language in typeface.FaceNames.Keys)
				{
					if (language == null) continue;
					if (typefaceNameSupplier == null)
					{
						typefaceNameSupplier = new TypefaceNameSupplier(language);
						TypefaceItems.RenameAll(typefaceNameSupplier.GetName);
					}

					if (TypefaceNameLanguageItems.All(li => li.Item != language))
					{
						TypefaceNameLanguageItems.Add(language);
					}
				}

				TypefaceItems.Add(typeface);
			}

			TypefaceLanguage_OnSelectionChanged(null, null);

			SelectMatchingTypeface(previouslySelectedStretch, previouslySelectedWeight, previouslySelectedStyle);

			UpdateSampleTextTypeface();
		}

		private void SelectMatchingTypeface(NamedItem<FontStretch> previouslySelectedStretch, NamedItem<FontWeight> previouslySelectedWeight,
			NamedItem<FontStyle> previouslySelectedStyle)
		{
			if (previouslySelectedStretch != null && previouslySelectedWeight != null && previouslySelectedStyle != null)
			{
				var typefaceItemToSelect = TypefaceItems.FirstOrDefault(tf =>
					tf.Item.Stretch == previouslySelectedStretch.Item &&
					tf.Item.Weight == previouslySelectedWeight.Item &&
					tf.Item.Style == previouslySelectedStyle.Item);
				if (typefaceItemToSelect != null)
					TypefaceList.SelectedItem = typefaceItemToSelect;
				else
				{
					TypefaceList.SelectedItem = null;
					CodepointRangeItems.Clear();
				}
			}
		}

		/*
		private void SelectLanguage(Selector selector, NamedCollection<XmlLanguage> source, NamedItem<XmlLanguage> previouslySelectedNamedItem)
		{
			var languageItemToSelect = source.FirstOrDefault(li => li.Item.Equals(previouslySelectedNamedItem?.Item));
			if (languageItemToSelect == null)
			{
				previouslySelectedNamedItem = FontFamilyLanguage?.SelectedItem as NamedItem<XmlLanguage>;
				if (previouslySelectedNamedItem != null)
					languageItemToSelect = source.FirstOrDefault(li => li.Item.Equals(previouslySelectedNamedItem.Item));
			}

			if (languageItemToSelect == null) languageItemToSelect = SelectClosestLanguage(source);
			if (languageItemToSelect != null)
			{
				if (selector != null)
					selector.SelectedItem = languageItemToSelect;
			}
		}
		*/
		
		private void TypefaceLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//var currentLanguage = TypefaceLanguage?.SelectedItem as NamedItem<XmlLanguage>;
			//if (currentLanguage == null) return;

			TypefaceNameSupplier typefaceNameSupplier = new(XmlLanguage.GetLanguage("en-us"));
			TypefaceItems.RenameAll(typefaceNameSupplier.GetName);
		}


		private void OpenFontCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = false;

			var typeface = (TypefaceList?.SelectedItem as NamedItem<Typeface>)?.Item;

			if (RestrictedLicense(typeface)) return;

			if (ConversionCodepointRangeItems.Count == 0) return;

			if (!Directory.Exists(SaveDirectory?.Text)) return;
			if (string.IsNullOrWhiteSpace(SaveBaseFilename?.Text)) return;

			e.CanExecute = true;
		}

		private bool RestrictedLicense(Typeface typeface)
		{
			if (typeface == null) return true;

			var rights = RightsCheck.GetRights(typeface, _userSettings.ConversionRights);
			if (rights.HasFlag(Rights.ConversionRestricted)) return true;
			if (rights.HasFlag(Rights.ConversionAllowed)) return false;

			// Default to restricted, not allowing conversion
			return true;
		}

		private bool IsRenameRequired(Typeface typeface)
		{
			if (typeface == null) return false;

			if (!App.TryGetGlyphTypeface(typeface, out GlyphTypeface glyphTypeface)) return false;

			return glyphTypeface.Copyrights.Values.Any(copyright => copyright.Contains("Reserved Font Name", StringComparison.InvariantCultureIgnoreCase));
		}

		private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var typefaceListSelectedItem = TypefaceList?.SelectedItem as NamedItem<Typeface>;
			var typeface = typefaceListSelectedItem?.Item;
			if (typeface == null) return;
			
			// Skip saving if we can't get the glyphs
			if (!typeface.TryGetGlyphTypeface(out _))
				return; // todo need to inform the user

			var fontInfo = new FontInfo(_userSettings, typeface, RightsCheck.GetRights(typeface, _userSettings.ConversionRights), SaveBaseFilename.Text)
			{
				Owner = this
			};
			var infoOk = fontInfo.ShowDialog();
			if (infoOk != true)
				return;

			var saveDirectory = new DirectoryInfo(SaveDirectory.Text);
			var conversion = GetConversion(typeface, OutputFormat.Tiny4kOLEDUnicode);
			var fontWriter = new Tiny4kOledFontWriter(conversion, saveDirectory, SaveBaseFilename.Text);
			conversion.Controller.Convert(fontWriter);

			var results = conversion.Result;
			MetricsRanges.Text = results.Ranges.ToString();
			MetricsCharacters.Text = results.Characters.ToString();
			MetricsMissingCharacters.Text = results.MissingCharacters.ToString();
			MetricsHeightInPages.Text = results.HeightInPages.ToString();
			MetricsTallestCharacter.Text = results.TallestCharacter.ToString();
			MetricsPixelBytes.Text = results.PixelBytes.ToString();
			MetricsTotalBytes.Text = results.TotalBytes.ToString();
		}

		private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void TypefaceList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CodepointRangeItems.Clear();

			var selectedItem = TypefaceList?.SelectedItem as NamedItem<Typeface>;
			if (selectedItem == null)
				return;

			var typeface = selectedItem.Item;
			if (typeface == null)
				return;

			FontStretchList.SelectedItem = FontStretchItems.FirstOrDefault(si => si.Item == typeface.Stretch);
			FontWeightList.SelectedItem = FontWeightItems.FirstOrDefault(si => si.Item == typeface.Weight);
			FontStyleList.SelectedItem = FontStyleItems.FirstOrDefault(si => si.Item == typeface.Style);

			if (!App.TryGetGlyphTypeface(typeface, out GlyphTypeface glyphTypeface)) return;

			var min = int.MaxValue;
			var max = int.MinValue;
			foreach (var c in glyphTypeface.CharacterToGlyphMap.Keys.OrderBy(x => x))
			{
				if (c < min)
				{
					min = c;
					max = c;
					continue;
				}

				if (c > max + 1)
				{
					AddCodepointRangeIfInSelectedUnicodeRanges(min, max);
					min = c;
					max = c;
					continue;
				}

				max = c;
			}

			if (max >= min)
				AddCodepointRangeIfInSelectedUnicodeRanges(min, max);

			var currentFontFamily = FontFamiliesListBox?.SelectedItem as NamedItem<FontFamily>;

			Regex FontNameReplaceRegex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);
			var combinedName = $"{currentFontFamily?.Name ?? "Unknown"}_{Describer.SafeFileName(selectedItem.Name)}_{_emSize}";
			SaveBaseFilename.Text = FontNameReplaceRegex.Replace(combinedName, "_");

			UpdateSampleTextTypeface();
		}

		private void AddCodepointRangeIfInSelectedUnicodeRanges(int minCodepoint, int maxCodepoint)
		{
			var selectedNamedUnicodeRanges = UnicodeBlockList.SelectedItems;
			foreach (NamedItem<CodepointRangeProperty> namedUnicodeRange in selectedNamedUnicodeRanges)
			{
				var min = namedUnicodeRange.Item.From;
				var max = namedUnicodeRange.Item.To;
				if (min <= minCodepoint)
				{
					if (maxCodepoint <= max)
					{
						AddCodepointRangeIfInSelectedCategories(minCodepoint, maxCodepoint);
						return;
					}
					else
					{
						if (minCodepoint <= max)
							AddCodepointRangeIfInSelectedCategories(minCodepoint, max);
					}
				}
				else
				{
					if (maxCodepoint >= min)
					{
						if (maxCodepoint <= max)
						{
							AddCodepointRangeIfInSelectedCategories(min, maxCodepoint);
						}
						else
						{
							AddCodepointRangeIfInSelectedCategories(min, max);
						}
					}
				}
			}
		}

		private void AddCodepointRangeIfInSelectedCategories(int minCodepoint, int maxCodepoint)
		{
			var selectedNamedCategories = CategoryList.SelectedItems;
			if (selectedNamedCategories.Cast<NamedItem<UnicodeCategory>>().Any(namedUnicodeCategory => namedUnicodeCategory.Item == (UnicodeCategory) (-1)))
			{
				CodepointRangeItems.Add(new CodepointRangeProperty(minCodepoint, maxCodepoint));
				return;
			}

			var selectedUnicodeCategories = selectedNamedCategories.Cast<NamedItem<UnicodeCategory>>()
				.Select(namedUnicodeCategory => namedUnicodeCategory.Item).ToHashSet();

			bool creatingBlock = false;
			int blockStartingCodepoint = minCodepoint;
			int blockEndingCodepoint = maxCodepoint;

			for (int c = minCodepoint; c <= maxCodepoint; c++)
			{
				if (selectedUnicodeCategories.Contains(CharUnicodeInfo.GetUnicodeCategory(c)))
				{
					blockEndingCodepoint = c;
					if (!creatingBlock)
					{
						blockStartingCodepoint = c;
						creatingBlock = true;
					}
				}
				else
				{
					if (creatingBlock)
					{
						CodepointRangeItems.Add(new CodepointRangeProperty(blockStartingCodepoint, blockEndingCodepoint));
						creatingBlock = false;
					}
				}
			}
			if (creatingBlock)
				CodepointRangeItems.Add(new CodepointRangeProperty(blockStartingCodepoint, blockEndingCodepoint));
		}

		private void UnicodeBlockList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//IndicateFontsContainingBlocks();

			TypefaceList_OnSelectionChanged(sender, e);
		}
/*
		private void IndicateFontsContainingBlocks()
		{
			var codepointRanges = UnicodeBlockList?.SelectedItems;
			foreach (var fontFamilyItem in FontFamilyItems)
			{
				if (fontFamilyItem.Name.EndsWith('*'))
					fontFamilyItem.Name = fontFamilyItem.Name.TrimEnd('*', ' ');
			}

			if (codepointRanges != null)
			{
				foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
				{
					if (fontFamily.FamilyMaps == null) continue;
					var familyMaps = fontFamily.FamilyMaps;
					foreach (NamedItem<CodepointRangeProperty> namedCodepointRange in codepointRanges)
					{
						foreach (var familyMap in familyMaps.Where(fm =>
							fm.Target != null && IsFontFamilyMapForCodepointRange(fm, namedCodepointRange.Item)))
						{
							var targets = familyMap.Target.Split(',', StringSplitOptions.TrimEntries);
							foreach (var fontFamilyItem in FontFamilyItems)
							{
								var familyName = fontFamilyItem.Item.Source;
								if (targets.Contains(familyName) && !fontFamilyItem.Name.EndsWith('*'))
									fontFamilyItem.Name = $"{fontFamilyItem.Name} *";
							}
						}
					}
				}
			}
		}

		private static bool IsFontFamilyMapForCodepointRange(FontFamilyMap familyMap, CodepointRangeProperty codepointRange)
		{
			var codepointRanges = ParseUnicodeRanges(familyMap.Unicode);
			return codepointRanges.Any(r => r.Intersects(codepointRange));
		}

		private static void ParseHexNumber(string numString, ref int index, out int number)
		{
			while (index<numString.Length && numString[index] == ' ') index++;

			number = 0;

			while (index < numString.Length) 
			{
				int n = numString[index];
				if (n is >= '0' and <= '9')
				{
					number = number * 0x10 + (n - '0');
					index++;
				}
				else
				{
					n |= 0x20; // [A-F] --> [a-f]
					if (n is >= 'a' and <= 'f')
					{
						number = (number * 0x10) + (n - ('a' - 10));
						index++;
					}
					else
						break;
				}
			} 

			while (index < numString.Length && numString[index] == ' ') index++;
		}

		private static CodepointRangeProperty[] ParseUnicodeRanges(string unicodeRanges)
		{
			const int lastUnicodeScalar = 0x10ffff;
			
			var ranges = new List<CodepointRangeProperty>(3);
			var index = 0;
			while (index < unicodeRanges.Length)
			{
				ParseHexNumber(unicodeRanges, ref index, out var firstNum);

				var lastNum = firstNum;

				if (index < unicodeRanges.Length)
				{
					switch (unicodeRanges[index])
					{
						case '?':
						{
							do
							{
								firstNum *= 16;
								lastNum = lastNum * 16 + 0x0F;
								index++;
							} while (
								index < unicodeRanges.Length && 
								unicodeRanges[index] == '?' &&
								lastNum <= lastUnicodeScalar);

							break;
						}
						case '-':
							index++; // pass '-' character
							ParseHexNumber(unicodeRanges, ref index, out lastNum);
							break;
					}
				}

				ranges.Add(new CodepointRangeProperty(firstNum, lastNum));

				index++; // ranges seperator comma
			}

			return ranges.ToArray();
		}
*/
		private void IncludeCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (CodepointRangeList?.SelectedItems.Count ?? 0) > 0;

		private void IncludeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedItems = CodepointRangeList?.SelectedItems;
			if (selectedItems == null) return;
			foreach (NamedItem<CodepointRangeProperty> namedCodepointRange in selectedItems)
			{
				ConversionCodepointRangeItems.Add(new CodepointRangeProperty(namedCodepointRange.Item.From, namedCodepointRange.Item.To));
			}
		}

		private void CombineCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (CodepointRangeList?.SelectedItems.Count ?? 0) > 1;

		private void CombineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedItems = CodepointRangeList?.SelectedItems;
			if (selectedItems == null) return;
			var minmin = int.MaxValue;
			var maxmax = int.MinValue;
			foreach (NamedItem<CodepointRangeProperty> namedCodepointRange in selectedItems)
			{
				var min = namedCodepointRange.Item.From;
				var max = namedCodepointRange.Item.To;
				if (min < minmin) minmin = min;
				if (max > maxmax) maxmax = max;
			}
			ConversionCodepointRangeItems.Add(new CodepointRangeProperty(minmin, maxmax));
		}

		private void TypefaceInfoCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (TypefaceList?.SelectedItems.Count ?? 0) > 0;

		private void TypefaceInfoCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var typefaceListSelectedItem = TypefaceList?.SelectedItem as NamedItem<Typeface>;
			var typeface = typefaceListSelectedItem?.Item;

			if (typeface == null) return;

			var fontInfo = new FontInfo(_userSettings, typeface, RightsCheck.GetRights(typeface, _userSettings.ConversionRights))
			{
				Owner = this
			};
			fontInfo.ShowDialog();
		}

		private void SampleCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (ConversionCodepointRangeList?.SelectedItems.Count ?? 0) > 0;

		private void SampleCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedItems = ConversionCodepointRangeList?.SelectedItems;
			if (selectedItems == null)
				return;

			var typefaceListSelectedItem = TypefaceList?.SelectedItem as NamedItem<Typeface>;
			var typeface = typefaceListSelectedItem?.Item;
			if (typeface == null)
				return;

			if (!App.TryGetGlyphTypeface(typeface, out GlyphTypeface glyphTypeface)) return;
			var characterToGlyphMap = glyphTypeface.CharacterToGlyphMap;

			StringBuilder sb = new();
			foreach (NamedItem<CodepointRangeProperty> selectedItem in selectedItems)
			{
				bool rangeIsEmpty = true;
				foreach (int i in selectedItem.Item)
				{
					if (characterToGlyphMap.ContainsKey(i))
					{
						if (rangeIsEmpty && sb.Length > 0) sb.Append(' ');
						sb.Append(Describer.DrawnUnicodeChars(i));
						rangeIsEmpty = false;
					}
				}
			}
			SampleTextInTypeface.Text = sb.ToString();
			UpdateSampleTextTypeface();
		}

		private void UpdateSampleTextTypeface()
		{
			var fontFamily = (FontFamiliesListBox?.SelectedItem as NamedItem<FontFamily>)?.Item;
			if (fontFamily == null) return;

			var typefaceListSelectedItem = TypefaceList?.SelectedItem as NamedItem<Typeface>;
			var typeface = typefaceListSelectedItem?.Item;

			var stretch = (FontStretchList?.SelectedItem as NamedItem<FontStretch>)?.Item ?? typeface?.Stretch ?? FontStretches.Normal;
			var weight = (FontWeightList?.SelectedItem as NamedItem<FontWeight>)?.Item ?? typeface?.Weight ?? FontWeights.Normal;
			var style = (FontStyleList?.SelectedItem as NamedItem<FontStyle>)?.Item ?? typeface?.Style ?? FontStyles.Normal;

			SampleTextInTypeface.FontFamily = fontFamily;
			SampleTextInTypeface.FontStretch = stretch;
			SampleTextInTypeface.FontWeight = weight;
			SampleTextInTypeface.FontStyle = style;
			SampleTextInTypeface.FontSize = EmSize * PreviewScale;

			UpdatePreview(typeface);
		}

		private void UpdatePreview(Typeface typeface)
		{
			if (!(PreviewEnabled?.IsChecked ?? false)) return;
			if (typeface == null) return;

			// Skip preview if we can't get the glyphs
			if (!typeface.TryGetGlyphTypeface(out _))
				return;

			var previewWidth = 128;
			var previewHeight = 64;
			var previewArray = new BoolBitmap(previewWidth, previewHeight);

			var conversion = GetConversion(typeface, OutputFormat.Tiny4kOLEDUnicode);
			conversion.Controller.Preview(SampleTextInTypeface.Text, previewArray);

			var buffer = new byte[previewWidth * previewHeight / 8];
			for (int y = 0; y < previewHeight; y++)
			{
				for (int x = 0; x < previewWidth; x++)
				{
					buffer[y * (previewWidth / 8) + (x >> 3)] += (byte) (previewArray.Get(x, y) ? 1 << (7 - (x & 7)) : 0);
				}
			}

			var dpiScale = VisualTreeHelper.GetDpi(this);
			var bitmap = BitmapSource.Create(previewWidth, previewHeight, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY,
				PixelFormats.BlackWhite, null, buffer, previewWidth / 8);
			SampleTextImage.Source = bitmap;
			RenderOptions.SetEdgeMode(SampleTextImage, EdgeMode.Aliased);
			RenderOptions.SetBitmapScalingMode(SampleTextImage, BitmapScalingMode.NearestNeighbor);

			var results = conversion.Result;
			MetricsRanges.Text = results.Ranges.ToString();
			MetricsCharacters.Text = results.Characters.ToString();
			MetricsMissingCharacters.Text = results.MissingCharacters.ToString();
			MetricsHeightInPages.Text = results.HeightInPages.ToString();
			MetricsTallestCharacter.Text = results.TallestCharacter.ToString();
			MetricsPixelBytes.Text = results.PixelBytes.ToString();
			MetricsTotalBytes.Text = results.TotalBytes.ToString();
		}

		private Conversion GetConversion(Typeface typeface, OutputFormat outputFormat)
		{
			var dpiScale = VisualTreeHelper.GetDpi(this);
			var pixelsPerDip = dpiScale.PixelsPerDip;

			var codepointRanges = new List<CodepointRange>();
			foreach (var codepointRangeItem in ConversionCodepointRangeItems)
			{
				codepointRanges.Add(codepointRangeItem.Item.AsRange());
			}

			return new Conversion(typeface, outputFormat, _emSize, pixelsPerDip, codepointRanges);
		}

		private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = (ConversionCodepointRangeList?.SelectedItems.Count ?? 0) > 0;

		private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var selectedItems = ConversionCodepointRangeList?.SelectedItems.Cast<NamedItem<CodepointRangeProperty>>().ToList();
			if (selectedItems == null || selectedItems.Count == 0)
				return;

			var index = ConversionCodepointRangeList.SelectedIndex;

			foreach (NamedItem<CodepointRangeProperty> selectedItem in selectedItems)
			{
				ConversionCodepointRangeItems.Remove(selectedItem);
			}

			if (ConversionCodepointRangeItems.Count > 0)
			{
				if (index > ConversionCodepointRangeItems.Count - 1)
				{
					index = ConversionCodepointRangeItems.Count - 1;
				}

				ConversionCodepointRangeList.SelectedIndex = index;
			}
		}

		private void SelectMatchingTypeface()
		{
			var stretch = FontStretchList?.SelectedItem as NamedItem<FontStretch>;
			var weight = FontWeightList?.SelectedItem as NamedItem<FontWeight>;
			var style = FontStyleList?.SelectedItem as NamedItem<FontStyle>;
			SelectMatchingTypeface(stretch, weight, style);
			UpdateSampleTextTypeface();
		}

		private void FontStretchList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectMatchingTypeface();
		}

		private void FontWeightList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectMatchingTypeface();
		}

		private void FontStyleList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectMatchingTypeface();
		}

		private void SampleTextInTypeface_OnTextChanged(object sender, TextChangedEventArgs e)
		{
			UpdateSampleTextTypeface();
		}

		private void PreviewEnabled_OnChecked(object sender, RoutedEventArgs e)
		{
			UpdateSampleTextTypeface();
		}

		private void RestrictToConvertible_OnClick(object sender, RoutedEventArgs e)
		{
			SetupNamedCollections();
		}

		private void SelectDirectoryBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if (SaveDirectory == null) return;
			var defaultDirectory = SaveDirectory.Text;
			if (defaultDirectory.Length == 0)
				defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			if (!defaultDirectory.EndsWith(Path.DirectorySeparatorChar))
				defaultDirectory += Path.DirectorySeparatorChar;
			using var dialog = new FolderBrowserDialog
			{
				Description = "Time to select a folder",
				UseDescriptionForTitle = true,
				SelectedPath = defaultDirectory,
				ShowNewFolderButton = true,
				AutoUpgradeEnabled = true
			};
			var result = dialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				SaveDirectory.Text = dialog.SelectedPath;
			}
		}

		private void CategoryList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TypefaceList_OnSelectionChanged(sender, e);
		}

		private void RestrictToNonSimulated_OnClick(object sender, RoutedEventArgs e)
		{
			RelistTypefacesForFont();
		}

		private void SelectAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ConversionCodepointRangeItems.Clear();
			foreach (NamedItem<CodepointRangeProperty> namedCodepointRange in CodepointRangeItems)
			{
				ConversionCodepointRangeItems.Add(new CodepointRangeProperty(namedCodepointRange.Item.From, namedCodepointRange.Item.To));
			}
		}

		private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var newItem = DefaultNewCodepointRange();
			var newNamedItem = new NamedItem<CodepointRangeProperty>(newItem, Describer.CodepointRange(newItem));

			ConversionCodepointRangeItems.Add(newNamedItem);

			if (ConversionCodepointRangeList != null)
				ConversionCodepointRangeList.SelectedItem = newNamedItem;
		}

		private CodepointRangeProperty DefaultNewCodepointRange()
		{
			CodepointRangeProperty newItem;
			if (ConversionCodepointRangeList?.SelectedItem is NamedItem<CodepointRangeProperty> selectedItem &&
			    selectedItem.Item.To < 0x10FFFF)
				newItem = new CodepointRangeProperty(selectedItem.Item.To + 1, selectedItem.Item.To + 1);
			else
			{
				var lastItem = ConversionCodepointRangeItems.LastOrDefault();
				if (lastItem != null && lastItem.Item.To < 0x10FFFF)
					newItem = new CodepointRangeProperty(lastItem.Item.To + 1, lastItem.Item.To + 1);
				else
				{
					if (ConversionCodepointRangeItems.All(ni => !ni.Item.Contains('A')))
						newItem = new CodepointRangeProperty('A', 'Z');
					else
						newItem = new CodepointRangeProperty(1, 1);
				}
			}

			return newItem;
		}
	}

	internal class NameComparer<T> : IComparer
	{
		public int Compare(object? x, object? y)
		{
			return (x as NamedItem<T>).Name.CompareTo((y as NamedItem<T>).Name);
		}
	}
}