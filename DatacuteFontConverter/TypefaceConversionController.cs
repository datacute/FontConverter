using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatacuteFontConverter;

public class TypefaceConversionController : IConversionController
{
	private readonly Conversion _conversion;

	public TypefaceConversionController(Conversion conversion)
	{
		_conversion = conversion;
	}


	public List<IFontConverter> Convert(IFontWriter fontWriter)
	{
		List<IFontConverter> conversions = new(_conversion.DesiredCodepointRanges.Count);
		foreach (var codepointRange in _conversion.DesiredCodepointRanges)
		{
			conversions.Add(OpenFont(codepointRange).Read());
		}

		foreach (var conversion in conversions)
		{
			conversion.Combine(conversions);
		}

		foreach (var conversion in conversions)
		{
			conversion.Convert()
				.Write(fontWriter);
			_conversion.Result.TotalBytes += 4; // 4 bytes for the DCUnicodeFontRef structure
		}

		fontWriter.WriteUnicodeFont(conversions);
		_conversion.Result.TotalBytes += 4; // 4 bytes for the DCUnicodeFont structure

		return conversions;
	}

	public void Preview(string text,BoolBitmap previewArray)
	{
		List<IFontConverter> conversions = Convert(new NullFontWriter());
		RenderPreview(text, previewArray, conversions);
	}

	private void RenderPreview(string text, BoolBitmap previewArray, List<IFontConverter> conversions)
	{
		// Initial position
		var cursor = new Cursor(0, 0);
		if (conversions.FirstOrDefault() is not TypefaceConverter commonConverter)
			return;

		foreach (var rune in text.EnumerateRunes())
		{
			RenderRune(previewArray, conversions, rune, cursor, commonConverter);

			if (cursor.Y >= previewArray.Height)
				return;
		}
	}

	private void RenderRune(BoolBitmap previewArray, List<IFontConverter> conversions, Rune rune, Cursor cursor,
		TypefaceConverter commonConverter)
	{
		switch (rune.Value)
		{
			// Carriage return
			case 13:
				cursor.X = 0;
				break;
			// Line Feed
			case 10:
				cursor.Y += commonConverter.PagesHigh * 8;
				break;
			// Space
			case 32:
				cursor.X += commonConverter.SpaceWidth;
				break;
			default:
				RenderCharacter(previewArray, conversions, rune, cursor, commonConverter);
				break;
		}

		if (cursor.X >= previewArray.Width)
		{
			cursor.X = 0;
			cursor.Y += commonConverter.PagesHigh * 8;
		}
	}

	private void RenderCharacter(BoolBitmap previewArray, List<IFontConverter> conversions, Rune rune, Cursor cursor,
		TypefaceConverter commonConverter)
	{
		var conversion = ConversionForRune(rune, conversions);
		if (conversion == null)
			return;

		// Wrap?
		var w = conversion.CharacterWidth(rune);
		if (cursor.X + w > previewArray.Width)
		{
			cursor.X = 0;
			cursor.Y += commonConverter.PagesHigh * 8;
		}

		if (cursor.Y > previewArray.Height)
			return;

		cursor.X += conversion.Render(rune, previewArray, cursor);
		cursor.X += commonConverter.Spacing;
	}

	private TypefaceConverter? ConversionForRune(Rune rune, List<IFontConverter> converters)
	{
		foreach (var conversion in converters)
		{
			if (conversion is not TypefaceConverter converter)
				continue;
			if (converter.CodepointRange.Contains(rune.Value))
				return converter;
		}

		return null;
	}

	private IFontConverter OpenFont(CodepointRange codepointRange)
	{
		try
		{
			return new TypefaceConverter(_conversion, codepointRange);
		}
		catch (Exception e)
		{
			return new ExceptionReportingFontConverter(e);
		}
	}
}