using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using static DatacuteFontConverterUI.ConversionRights.CheckedItem;
using static DatacuteFontConverterUI.ConversionRights.CheckStyle;
using static DatacuteFontConverterUI.ConversionRights.Rights;

namespace DatacuteFontConverterUI.ConversionRights
{
	public record RightsCheck(CheckedItem Item, CheckStyle Style, string Search, Rights ApplicableRights)
	{
		public CheckedItem Item { get; set; } = Item;
		public CheckStyle Style { get; set; } = Style;
		public Rights Applies(Typeface typeface)
		{
			if (typeface == null) return Unknown;

			if (!App.TryGetGlyphTypeface(typeface, out GlyphTypeface glyphTypeface)) return Unknown;

			return Applies(glyphTypeface) ? ApplicableRights : Unknown;
		}

		public Rights Applies(Typeface typeface, GlyphTypeface glyphTypeface)
		{
			if (typeface == null) return Unknown;

			if (glyphTypeface == null) return Unknown;

			return Applies(glyphTypeface) ? ApplicableRights : Unknown;
		}

		private bool Applies(GlyphTypeface glyphTypeface)
		{
			return Item switch
			{
				Copyright => CopyrightApplies(glyphTypeface),
				License => LicenseApplies(glyphTypeface),
				FamilyName => FontFamilyApplies(glyphTypeface),
				_ => false
			};
		}

		private bool CopyrightApplies(GlyphTypeface glyphTypeface)
		{
			return glyphTypeface.Copyrights.Values.Any(CheckItem);
		}
		private bool LicenseApplies(GlyphTypeface glyphTypeface)
		{
			return glyphTypeface.LicenseDescriptions.Values.Any(CheckItem);
		}
		private bool FontFamilyApplies(GlyphTypeface glyphTypeface)
		{
			return glyphTypeface.FamilyNames.Values.Any(CheckItem);
		}

		private bool CheckItem(string item)
		{
			return Style switch
			{
				Exactly => item.Equals(Search, StringComparison.InvariantCultureIgnoreCase),
				StartsWith => item.StartsWith(Search, StringComparison.InvariantCultureIgnoreCase),
				Contains => item.Contains(Search, StringComparison.InvariantCultureIgnoreCase),
				ExactHash => Hash(item).Equals(Search, StringComparison.InvariantCultureIgnoreCase),
				_ => false
			};
		}

		public static string Hash(string item)
		{
			return BitConverter.ToString(
				MD5.Create().ComputeHash(
					new UTF8Encoding().GetBytes(item)
				)
			).Replace("-", string.Empty);
		}
		public static Rights GetRights(Typeface typeface, List<RightsCheck> conversionRights)
		{
			if (typeface == null) return Unknown;
			
			// Getting the glyph typeface (for every font) is expensive
			// So the results are cached
			// and the cache is cleared whenever the rights rules are changed

			if (App.RightsCache.TryGetValue(typeface, out var cachedResult))
				return cachedResult;

			if (!App.TryGetGlyphTypeface(typeface, out GlyphTypeface glyphTypeface))
			{
				cachedResult = Unknown;
				App.RightsCache.Add(typeface, cachedResult);
				return cachedResult;
			}

			foreach (var r in conversionRights)
			{
				var applies = r.Applies(typeface, glyphTypeface);
				if (applies != Unknown)
				{
					cachedResult = applies;
					App.RightsCache.Add(typeface, cachedResult);
					return cachedResult;
				}
			}

			// Default to restricted, not allowing conversion
			cachedResult = Unknown;
			App.RightsCache.Add(typeface, cachedResult);
			return cachedResult;
		}

		public string ItemDesc => Describer.CheckedItem(Item);
		public string StyleDesc => Describer.CheckStyle(Style);
		public string ApplicableRightsDesc => ApplicableRights.ToString().CapitalsToWords();
	}
}