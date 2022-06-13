using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatacuteFontConverter;

public class Tiny4kOledFontWriter : IFontWriter
{
	private static readonly Regex FontNameReplaceRegex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);

	private readonly Conversion _conversion;
	private readonly DirectoryInfo _destFontDirectoryInfo;
	private readonly string _destinationFontBaseName;

	public Tiny4kOledFontWriter(Conversion conversion, DirectoryInfo destFontDirectoryInfo, string destinationFontBaseName)
	{
		_conversion = conversion;
		_destFontDirectoryInfo = destFontDirectoryInfo;
		_destinationFontBaseName = destinationFontBaseName;

		_fontBaseName = FontNameReplaceRegex.Replace(_destinationFontBaseName, "_");
		var nameIncludesFont = _fontBaseName.Contains("font", StringComparison.InvariantCultureIgnoreCase);
		_fontPrefixLower = nameIncludesFont ? string.Empty : "font";
		_fontPrefixUpper = nameIncludesFont ? string.Empty : "Font";
		_fontPrefixCaps = nameIncludesFont ? string.Empty : "FONT";
	}

	//private int Width;
	//private int Height;
	private string FontBaseName(FontConverter fontConverter) => $"{_fontBaseName}_{fontConverter.FirstCharacter:X4}_{fontConverter.LastCharacter:X4}";
	private string FontHeaderName(FontConverter fontConverter) => $"{FontBaseName(fontConverter)}_{(fontConverter as TypefaceConverter)?.Width ?? 0}x{(fontConverter as TypefaceConverter)?.Height ?? 0}.h";
	private string FontObjectName(FontConverter fontConverter) => $"TinyOLED{_fontPrefixUpper}{FontBaseName(fontConverter)}";
	private string FontDataName(FontConverter fontConverter)  => $"ssd1306xled_{_fontPrefixLower}{FontBaseName(fontConverter)}";
	private string WidthsDataName(FontConverter fontConverter) => $"TinyOLED{_fontPrefixUpper}{FontBaseName(fontConverter)}_widths";
	private string Widths16sDataName(FontConverter fontConverter) => $"{WidthsDataName(fontConverter)}_16s";
	private string FontDefine(FontConverter fontConverter) => $"{_fontPrefixCaps}{FontBaseName(fontConverter).ToUpper()}";

	private string? _fontBaseName;
	private string? _fontPrefixLower;
	private string? _fontPrefixUpper;
	private string? _fontPrefixCaps;

	public FontConverter Write(FontConverter converter)
	{
		//Width = (converter as TypefaceConverter)?.Width ?? 0;
		//Height = (converter as TypefaceConverter)?.Height ?? 0;

		var outputFileInfo = new FileInfo(Path.Combine(_destFontDirectoryInfo.FullName, FontHeaderName(converter)));
		if (outputFileInfo.Exists) return new ErrorReportingFontConverter($"{outputFileInfo.FullName} already exists");
		if (string.IsNullOrEmpty(_fontBaseName)) return new ErrorReportingFontConverter("Output file has invalid base name");

		using var writer = outputFileInfo.CreateText();

		WriteTop(writer, converter);

		WriteCharacterData(writer, converter);

		WriteWidths(writer, converter);
		WriteFontObjectProp(writer, converter);

		WriteBottom(writer, converter);

		return converter;
	}

	private void WriteTop(StreamWriter writer, FontConverter converter)
	{
		writer.WriteLine(@$"/*
  {_fontBaseName}
  Datacute Proportional Font
  {converter.CharacterData?.Length} characters: {Describe(converter.FirstCharacter)} to {Describe(converter.LastCharacter)}
  Width: typically {converter.Width} pixels wide including {converter.Spacing} pixel spacing between characters
  Height: {converter.Height} pixels, {converter.PagesHigh} pages (bottom {converter.PagesHigh * 8 - converter.Height} rows are always blank)
  Total bytes: {converter.Widths?.Sum()} * {converter.PagesHigh} + {converter.CharacterData?.Length} + {Width16sLength(converter.CharacterData?.Length)} * 2 + 11 = {converter.Widths?.Sum() * converter.PagesHigh + converter.CharacterData?.Length + Width16sLength(converter.CharacterData?.Length) * 2 + 11}

{Metadata}

  To use:

#include ""{FontHeaderName(converter)}""
const DCfont *currentFont = {FontDefine(converter)};

However, this font was created as a portion of a unicode font, see {_destinationFontBaseName}_Unicode.h
*/
");

		writer.WriteLine("#include <avr/pgmspace.h>");
	}

	private string Metadata => _conversion.Metadata.ToString();

	private static int Width16sLength(int? characters)
	{
		return ((characters ?? 0)+15) >> 4;
	}

	private void WriteCharacterData(StreamWriter writer, FontConverter converter)
	{
		writer.WriteLine($"const uint8_t {FontDataName(converter)} [] PROGMEM = {{");
		char c = (char) converter.FirstCharacter;
		if (converter.CharacterData != null)
			foreach (var characterData in converter.CharacterData)
			{
				if (characterData?.Length > 0)
					writer.WriteLine($"  {characterData}, // {Describe(c)}");
				else
					writer.WriteLine($"  // {Describe(c)}");
				c++;
			}

		writer.WriteLine("};");
	}

	private void WriteWidths(StreamWriter writer, FontConverter converter)
	{
		WriteWidths(writer, WidthsDataName(converter), "8", ItemCombiner.Comma, converter);
		writer.WriteLine();
		WriteWidths(writer, Widths16sDataName(converter), "16", ItemCombiner.Plus, converter);
	}

	private void WriteWidths(StreamWriter writer, string objectName, string bits, ItemCombiner combiner, FontConverter converter)
	{
		writer.WriteLine($"const uint{bits}_t {objectName} [] PROGMEM = {{");
		var widthsWritten = 0;
		if (converter.Widths != null)
			foreach (var w in converter.Widths)
			{
				combiner.CombineItems(writer, widthsWritten);

				writer.Write($"{w}");
				widthsWritten++;
			}

		writer.WriteLine();
		writer.WriteLine("  };");
	}

	private static string Describe(in int c)
	{
		//if (c >= '!' && c <= '[') return $"0x{c:X2} {c} {char.ConvertFromUtf32(c)}";
		//if (c >= ']' && c <= '~') return $"0x{c:X2} {c} {char.ConvertFromUtf32(c)}";
		if (c == ' ') return $"0x{c:X2} {c}   (space)";
		if (c == '\\') return $"0x{c:X2} {c} \\ backslash";
		if (c >= 0x100000) return $"U+{c:X6} {c} {char.ConvertFromUtf32(c)}";
		if (c >= 0x10000) return $"U+{c:X5} {c} {char.ConvertFromUtf32(c)}";
		if (c >= 0x80) return $"U+{c:X4} {c} {char.ConvertFromUtf32(c)}";
		return $"0x{c:X2} {c} {char.ConvertFromUtf32(c)}";
	}

	private void WriteFontObjectProp(StreamWriter writer, FontConverter converter)
	{
		var firstCharacterLowByte = converter.FirstCharacter & 0xFF;
		var lastCharacterLowByte = converter.LastCharacter & 0xFF;
		writer.WriteLine($"const DCfont {FontObjectName(converter)} = {{");
		writer.WriteLine($"  (uint8_t *){FontDataName(converter)},");
		writer.WriteLine( "  0, // character width in pixels 0 for proportional fonts");
		writer.WriteLine($"  {converter.PagesHigh}, // character height in pages (8 pixels)");
		writer.WriteLine($"  {firstCharacterLowByte},{lastCharacterLowByte}, // first and last low byte defining range of included character codepoints");
		writer.WriteLine($"  (uint16_t *){Widths16sDataName(converter)},");
		writer.WriteLine($"  (uint8_t *){WidthsDataName(converter)},");
		writer.WriteLine($"  {converter.Spacing} // spacing");
		writer.WriteLine( "  };");
	}

	private void WriteBottom(StreamWriter writer, FontConverter converter)
	{
		writer.WriteLine($"#define {FontDefine(converter)} (&{FontObjectName(converter)})");
		WriteLicense(writer);
	}

	private void WriteLicense(StreamWriter writer)
	{
		if (!string.IsNullOrWhiteSpace(_conversion.Metadata.LicenceDescription) && _conversion.Metadata.LicenceDescription.Length > 250)
		{
			writer.WriteLine("/*");
			writer.Write(_conversion.Metadata.LicenceDescription);
			writer.WriteLine();
			writer.WriteLine("*/");
		}
	}

	public void WriteUnicodeFont(List<IFontConverter> conversions)
	{
		Regex fontNameReplaceRegex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);
		var fontBaseName = fontNameReplaceRegex.Replace(_destinationFontBaseName, "_");

		// This used to exclude _ too
		Regex fontDefineReplaceRegex = new(@"[^a-zA-Z0-9_]", RegexOptions.Compiled);
		var fontDefineName = fontDefineReplaceRegex.Replace(fontBaseName, "").ToUpper();

		var outputFileInfo =
			new FileInfo(Path.Combine(_destFontDirectoryInfo.FullName, fontBaseName + "_Unicode.h"));
		if (outputFileInfo.Exists)
		{
			Console.Error.WriteLine($"{outputFileInfo.FullName} already exists");
			return;
		}

		using var writer = outputFileInfo.CreateText();

		writer.WriteLine(@$"/*
  {_destinationFontBaseName}
  Datacute Unicode Font

{_conversion.Metadata}
  To use:

#include ""{_destinationFontBaseName}_Unicode.h""
const DCUnicodeFont *currentUnicodeFont = FONT{fontDefineName};
or
oled.setUnicodeFont(FONT{fontDefineName});

*/
");

		foreach (var conversion in conversions)
		{
			if (conversion is not TypefaceConverter converter)
				continue;
			writer.WriteLine($"#include \"{FontHeaderName(converter)}\"");
		}

		writer.WriteLine();
		writer.WriteLine($"const DCUnicodeFontRef TinyOLEDUnicodeFont{fontBaseName}_Fonts[{conversions.Count}] = {{");
		int spaceWidth = 0;
		foreach (var conversion in conversions)
		{
			if (conversion is not TypefaceConverter converter)
				continue;
			if (spaceWidth > 0)
				writer.WriteLine(",");
			var unicodePlane = (converter.FirstCharacter & 0x1F0000) >> 16;
			var unicodeBlock = (converter.FirstCharacter & 0xFF00) >> 8;
			writer.Write($"  {{{unicodePlane,2},{unicodeBlock,3}, &{FontObjectName(converter)} }}");
			spaceWidth = converter.SpaceWidth;
		}

		writer.WriteLine();
		writer.WriteLine("};");
		writer.WriteLine();
		writer.WriteLine($"const DCUnicodeFont TinyOLEDUnicodeFont{fontBaseName} = {{");
		writer.WriteLine(
			$"  {spaceWidth}, // Space width. The space character does not need to be included in the font glyphs.");
		writer.WriteLine($"  {conversions.Count}, // Number of character ranges in this unicode font");
		writer.WriteLine($"  TinyOLEDUnicodeFont{fontBaseName}_Fonts // the font references");
		writer.WriteLine("};");
		writer.WriteLine();

		writer.WriteLine($"#define FONT{fontDefineName} (&TinyOLEDUnicodeFont{fontBaseName})");
		WriteLicense(writer);
	}

}