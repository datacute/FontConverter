using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;

namespace DatacuteFontConverter;

public class Conversion
{
	public Conversion(Typeface typeface, OutputFormat outputFormat, double emSize, double pixelsPerDip,
		List<CodepointRange> desiredCodepointRanges)
	{
		Typeface = typeface;
		OutputFormat = outputFormat;
		EmSize = emSize;
		PixelsPerDip = pixelsPerDip;
		DesiredCodepointRanges = desiredCodepointRanges;
		Result = new ConversionResults();

		GlyphTypefaceCache.TryGet(Typeface, out GlyphTypeface glyphTypeface);
		GlyphTypeface = glyphTypeface;
		CharacterToGlyphMap = glyphTypeface.CharacterToGlyphMap;
		if (CharacterToGlyphMap.TryGetValue(' ', out ushort spaceIndex))
			SpaceWidth = (int)Math.Ceiling(glyphTypeface.AdvanceWidths[spaceIndex] * EmSize); 

		var cultureInfo = CultureInfo.GetCultureInfo("en-US");
		Metadata = new Metadata
		{
			FontFamily = Typeface.FontFamily.Source,
			Typeface = Typeface.FaceNames[XmlLanguage.GetLanguage("en-us")],
			Copyright = glyphTypeface.Copyrights[cultureInfo],
			Description = glyphTypeface.Descriptions[cultureInfo],
			LicenceDescription = glyphTypeface.LicenseDescriptions[cultureInfo],
			ManufacturerName = glyphTypeface.ManufacturerNames[cultureInfo],
			Trademark = glyphTypeface.Trademarks[cultureInfo],
			VendorUrl = glyphTypeface.VendorUrls[cultureInfo],
			Version = glyphTypeface.VersionStrings[cultureInfo]
		};
	}

	public Typeface Typeface { get; }

	public OutputFormat OutputFormat { get; }

	public double EmSize { get; }

	public double PixelsPerDip { get; }

	public List<CodepointRange> DesiredCodepointRanges { get; }

	public IConversionController Controller => new TypefaceConversionController(this);

	public ConversionResults Result { get; }

	public GlyphTypeface GlyphTypeface { get; }

	public IDictionary<int, ushort>? CharacterToGlyphMap { get; }

	public int SpaceWidth { get; }

	public Metadata Metadata { get; }

}