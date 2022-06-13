using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DatacuteFontConverterUI.ConversionRights;

namespace DatacuteFontConverterUI
{
	public partial class FontInfo
	{
		private readonly UserSettings _userSettings;
		private readonly Typeface _typeface;
		private readonly string _baseFileName;
		private readonly GlyphTypeface _glyphTypeface;

		public FontInfo(UserSettings userSettings, Typeface typeface, Rights conversionRights, string baseFileName = null)
		{
			_userSettings = userSettings;
			_typeface = typeface;
			_baseFileName = baseFileName;
			App.TryGetGlyphTypeface(_typeface, out _glyphTypeface);
			
			// If these are duplicated, just show the designer details
			DesignerNames = _glyphTypeface.DesignerNames;
			ManufacturerNames = IfNotDuplicate(_glyphTypeface.ManufacturerNames, DesignerNames);

			// If these are duplicated, just show the designer details
			DesignerUrls = _glyphTypeface.DesignerUrls;
			VendorUrls = IfNotDuplicate(_glyphTypeface.VendorUrls, DesignerUrls);

			// If these are duplicated, don't show the Win32 details
			FamilyNames = _glyphTypeface.FamilyNames;
			Win32FamilyNames = IfNotDuplicate(_glyphTypeface.Win32FamilyNames, FamilyNames);

			// If these are duplicated, don't show the Win32 details
			FaceNames = _glyphTypeface.FaceNames;
			Win32FaceNames = IfNotDuplicate(_glyphTypeface.Win32FaceNames, FaceNames);

			IsConversionAllowed = conversionRights.HasFlag(Rights.ConversionAllowed);
			IsRenameRequired = conversionRights.HasFlag(Rights.CheckReservedFontNames);
			if (IsRenameRequired)
			{
				if (_userSettings.ReservedFontNames.TryGetValue(_typeface.FontFamily.Source, out var reservedNames))
				{
					IsRenameRequired = reservedNames.Count > 0;
				}
				else
				{
					IsRenameRequired = _glyphTypeface.Copyrights.Values.Any(copyright => copyright.Contains("Reserved Font Name", StringComparison.InvariantCultureIgnoreCase));
				}
			}

			InitializeComponent();
			DataContext = this;
		}

		private IDictionary<CultureInfo, string> IfNotDuplicate(IDictionary<CultureInfo, string> dictionaryToCheck, IDictionary<CultureInfo, string> existingDictionary)
		{
			return existingDictionary.Values.SequenceEqual(dictionaryToCheck.Values) &&
			       existingDictionary.Keys.SequenceEqual(dictionaryToCheck.Keys)
				? new Dictionary<CultureInfo, string>()
				: dictionaryToCheck;
		}

		private void OkButton_OnClick(object sender, RoutedEventArgs e)
		{
			DialogResult = IsConversionAllowed && !IsReservedName();
		}

		private bool IsReservedName()
		{
			// If renaming is not require, then there are no reserved names
			if (!IsRenameRequired) return false;
			if (_baseFileName == null) return true;
			if (_userSettings.ReservedFontNames.TryGetValue(_typeface.FontFamily.Source, out var reservedNames))
			{
				foreach (var reservedName in reservedNames)
				{
					if (_baseFileName != null &&
					    _baseFileName.Contains(reservedName, StringComparison.InvariantCultureIgnoreCase))
						return true;
				}
				// The base filename does not contain any of the reserved names
				return false;
			}
			return true;
		}

		public IDictionary<CultureInfo, string> FamilyNames { get; }

		public IDictionary<CultureInfo, string> FaceNames { get; }

		public IDictionary<CultureInfo, string> Win32FamilyNames { get; }

		public IDictionary<CultureInfo, string> Win32FaceNames { get; }

		public IDictionary<CultureInfo, string> ManufacturerNames { get; }

		public IDictionary<CultureInfo, string> DesignerNames { get; }

		public IDictionary<CultureInfo, string> VendorUrls { get; }

		public IDictionary<CultureInfo, string> DesignerUrls { get; }

		public IDictionary<CultureInfo, string> Copyrights => _glyphTypeface.Copyrights;
		public IDictionary<CultureInfo, string> Descriptions => _glyphTypeface.Descriptions;
		public IDictionary<CultureInfo, string> Trademarks => _glyphTypeface.Trademarks;
		public IDictionary<CultureInfo, string> LicenseDescriptions => _glyphTypeface.LicenseDescriptions;
		public IDictionary<CultureInfo, string> SampleTexts => _glyphTypeface.SampleTexts;
		public IDictionary<CultureInfo, string> VersionStrings => _glyphTypeface.VersionStrings;

		public bool IsConversionAllowed { get; set; }
		public bool IsRenameRequired { get; set; }
		private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void EditRightsButton_OnClick(object sender, RoutedEventArgs e)
		{
			var editor = new RightsEditor(_userSettings)
			{
				CurrentTypeface = _typeface,
				CurrentCopyrightHash = RightsCheck.Hash(LocalizedCopyrights.LocalizedText),
				CurrentLicenseDescriptionsHash = RightsCheck.Hash(LocalizedLicenseDescriptions.LocalizedText)
			};
			editor.Show();
		}
	}
}