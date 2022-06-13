using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using DatacuteFontConverterUI.ConversionRights;

namespace DatacuteFontConverterUI
{
	public static class Describer
	{
		public static string EmptyString(object _)
		{
			return string.Empty;
		}

		public static string XmlLanguage(XmlLanguage xmlLanguage) => $"{xmlLanguage.IetfLanguageTag} : {xmlLanguage.GetSpecificCulture().DisplayName}";

		public static string FontFamily(FontFamily fontFamily)
		{
			var currentUiLanguage = System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.IetfLanguageTag);
			if (!fontFamily.FamilyNames.TryGetValue(currentUiLanguage, out var familyName))
			{
				familyName = fontFamily.Source;
			}
			return familyName;
		}

		public static string UnicodeCategoryName(UnicodeCategory unicodeCategory) => Enum.GetName(unicodeCategory).CapitalsToWords();
		public static string CodepointRange(CodepointRangeProperty codepointRange) => $"{Codepoint(codepointRange.From)} to {Codepoint(codepointRange.To)}";

		public static string Codepoint(int c) => $"{UnicodeNumber(c)} {DrawnUnicodeChars(c)}";

		public static string UnicodeNumber(int c)
		{
			return c switch
			{
				>= 0x100000 => $"U+{c:X6}",
				>= 0x10000 => $"U+{c:X5}",
				_ => $"U+{c:X4}"
			};
		}

		public static string UnicodeRangeSample(this UnicodeRange unicodeRange)
		{
			return UnicodeRangeSample(unicodeRange.FirstCodePoint,
				unicodeRange.FirstCodePoint + unicodeRange.Length - 1);
		}

		public static string UnicodeRangeSample(int firstCharacter, int lastCharacter)
		{
			StringBuilder sb = new();
			var characters = lastCharacter - firstCharacter + 1;
			switch (characters)
			{
				case <= 64:
				{
					foreach (var c in Enumerable.Range(firstCharacter, characters))
					{
						sb.Append(DrawnUnicodeChars(c));
					}

					break;
				}
				case <= 256:
				{
					foreach (var c in Enumerable.Range(firstCharacter, characters))
					{
						if (c % 16 == 0 && sb.Length > 0)
							sb.Append(Environment.NewLine);
						sb.Append(DrawnUnicodeChars(c));
					}

					break;
				}
				default:
				{
					foreach (var c in Enumerable.Range(firstCharacter, 256))
					{
						if (c % 16 == 0 && sb.Length > 0)
							sb.Append(Environment.NewLine);
						sb.Append(DrawnUnicodeChars(c));
					}
					sb.Append(Environment.NewLine);
					sb.Append("...");

					break;
				}
			}
			return sb.ToString();
		}
		public static string DrawnUnicodeChars(int c)
		{
			string chars = " ";
			if (!Rune.IsValid(c)) 
				return Rune.ReplacementChar.ToString();

			switch (CharUnicodeInfo.GetUnicodeCategory(c))
			{
				case UnicodeCategory.LineSeparator:
				case UnicodeCategory.SpaceSeparator:
					break;
				case UnicodeCategory.Control:
				case UnicodeCategory.Format:
				case UnicodeCategory.Surrogate:
					chars = Rune.ReplacementChar.ToString();
					break;
				case UnicodeCategory.SpacingCombiningMark:
				case UnicodeCategory.EnclosingMark:
				case UnicodeCategory.NonSpacingMark:
					chars = $" {char.ConvertFromUtf32(c)}";
					break;
				default:
					chars = char.ConvertFromUtf32(c);
					break;
			}

			return chars;
		}

		private static Dictionary<string, string> unicodeRangePropertyInfoDescriptions = new()
		{
			{"IpaExtensions", "IPA Extensions"},
			{"NKo", "N'Ko"},
			{"CombiningDiacriticalMarksforSymbols", "Combining Diacritical Marks for Symbols"}
		};
		public static string UnicodeRangePropertyInfo(PropertyInfo rangePropertyInfo)
		{
			var propertyDescription = rangePropertyInfo.Name;
			if (unicodeRangePropertyInfoDescriptions.TryGetValue(propertyDescription, out var description))
				return description;
			description = propertyDescription.CapitalsToWords();
			unicodeRangePropertyInfoDescriptions.Add(propertyDescription, description);
			return description;
		}

		private static Regex spaceBeforeLastCapital = new Regex(@"(\P{Ll})(\P{Ll}\p{Ll})", RegexOptions.Compiled);
		private static Regex spaceBeforeFirstCapital = new Regex(@"(\p{Ll})(\P{Ll})", RegexOptions.Compiled);
		private static Regex andEndingWords = new Regex(@"(\p{Ll})(and )", RegexOptions.Compiled);
		public static string CapitalsToWords( this string str )
		{
			str = spaceBeforeLastCapital.Replace(str, "$1 $2");
			str = spaceBeforeFirstCapital.Replace(str, "$1 $2");
			str = andEndingWords.Replace(str, "$1 $2");
			str = str.Replace("Cjk", "CJK");
			str = str.Replace(" ,", ",");
			str = str.Replace("  ", " ");
			return str;
		}

		public static string FontStretch(FontStretch fontStretch) => fontStretch.ToString();

		public static string FontWeight(FontWeight fontWeight) => fontWeight.ToOpenTypeWeight() == 350 ? "SemiLight" : fontWeight.ToString();

		public static string FontStyle(FontStyle fontStyle) => fontStyle.ToString();

		public static int UnicodeNumberParse(string codepoint)
		{
			if (codepoint is null)
				throw new ArgumentNullException(nameof(codepoint));

			int runeCount = 0;
			int firstRuneCodepoint = 0;

			foreach (Rune rune in codepoint.EnumerateRunes())
			{
				if (runeCount == 0)
				{
					firstRuneCodepoint = rune.Value;
				}
				runeCount++;
			}
			if (runeCount == 1)
				return firstRuneCodepoint;

			if (codepoint.StartsWith("U+") || codepoint.StartsWith("0x"))
				return int.Parse(codepoint[2..], NumberStyles.HexNumber);
			if (codepoint.StartsWith("#") || codepoint.StartsWith("x"))
				return int.Parse(codepoint[1..], NumberStyles.HexNumber);

			return int.Parse(codepoint);
		}

		public static string SafeFileName(string suggestedFilename)
		{
			return suggestedFilename;
		}

		public static string CheckedItem(CheckedItem checkedItem) => Enum.GetName(checkedItem).CapitalsToWords();
		public static string CheckStyle(CheckStyle checkStyle) => Enum.GetName(checkStyle).CapitalsToWords();
		public static string Rights(Rights rights) => Enum.GetName(rights).CapitalsToWords();
	}
}