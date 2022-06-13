using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace DatacuteFontConverter
{
	public class TypefaceConverter : FontConverter
	{
		private readonly Conversion _conversion;
		private readonly CodepointRange _codepointRange;
		private BoolBitmap[] _bitmaps;
		private MinMaxXY _pixelExtentsRange;
		private int[] _glyphWidths; 

		public TypefaceConverter(Conversion conversion, CodepointRange codepointRange)
		{
			_conversion = conversion ?? throw new ArgumentNullException(nameof(conversion));
			_codepointRange = codepointRange;
			_bitmaps = Array.Empty<BoolBitmap>();
			_pixelExtentsRange = new MinMaxXY();
			_glyphWidths = Array.Empty<int>();
		}

		public CodepointRange CodepointRange => _codepointRange;

		// public override string Metadata => _conversion.Metadata?.ToString() ?? string.Empty;
		//
		// public override void WriteLicense(StreamWriter writer)
		// {
		// 	if (!string.IsNullOrWhiteSpace(_conversion.Metadata?.LicenceDescription) && _conversion.Metadata.LicenceDescription.Length > 250)
		// 	{
		// 		writer.WriteLine("/*");
		// 		writer.Write(_conversion.Metadata.LicenceDescription);
		// 		writer.WriteLine();
		// 		writer.WriteLine("*/");
		// 	}
		// }

		public override IFontConverter Read()
		{
			FirstCharacter = _codepointRange.From;
			LastCharacter = _codepointRange.To;
			Characters = _codepointRange.Count;
			SpaceWidth = _conversion.SpaceWidth;

			_bitmaps = new BoolBitmap[Characters];
			_glyphWidths = new int[Characters];

			Widths = new int[Characters];
			
			if (_conversion.CharacterToGlyphMap == null || _conversion.GlyphTypeface == null) return this;

			int i = 0;
			foreach (var codepoint in _codepointRange)
			{
				RenderTargetBitmap? renderBitmap = null;
				// Only convert characters actually in this typeface.
				// This stops font fallback, which doesn't honor licensing.
				if (_conversion.CharacterToGlyphMap.ContainsKey(codepoint))
					renderBitmap = GetBitmap(codepoint, _conversion.Typeface, _conversion.EmSize, _conversion.PixelsPerDip);

				if (renderBitmap == null)
				{
					Widths[i++] = 0;
					continue; // need to output 0 width item instead
				}
				
				var bitmapData = GetBitmapData(renderBitmap);

				_pixelExtentsRange.Include(bitmapData.Coverage);

				var width = bitmapData.Coverage.Width;
				if (_conversion.GlyphTypeface.CharacterToGlyphMap.TryGetValue(codepoint, out ushort gi))
					_glyphWidths[i] = (int)Math.Ceiling(_conversion.GlyphTypeface.AdvanceWidths[gi] * _conversion.EmSize) - width;
				Widths[i] = width;

				_bitmaps[i] = bitmapData;

				// BitmapEncoder pngEncoder = new PngBitmapEncoder();
				// pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
				//
				// Directory.CreateDirectory("results");
				// using (var fs = File.OpenWrite(
				// 		$"results/{_conversion.SystemFontName}_{_conversion.SystemFontFaceName}_{codepoint}.png"))
				// 	//$"results/{fontFamily.Source}_{typeface.FaceNames[lang]}_{Char.ConvertFromUtf32(codepoint)}.png"))
				// {
				// 	pngEncoder.Save(fs);
				// }

				i++;
			}

			return this;
		}

		private BoolBitmap GetBitmapData(RenderTargetBitmap renderBitmap)
		{
			int bitmapWidth = renderBitmap.PixelWidth;
			int bitmapHeight = renderBitmap.PixelHeight;

			var bytesPerPixel = ((renderBitmap.Format.BitsPerPixel + 7) / 8);
			var stride = bitmapWidth * bytesPerPixel;
			var pixels = new byte[bitmapHeight * stride];
			renderBitmap.CopyPixels(pixels, stride, 0);

			var bitmap = new BoolBitmap(bitmapWidth, bitmapHeight);
			for (int x = 0; x < bitmapWidth; x++)
			{
				for (int y = 0; y < bitmapHeight; y++)
				{
					var pixelIndex = x * bytesPerPixel + y * stride;
					byte r = pixels[pixelIndex];
					byte g = pixels[pixelIndex + 1];
					byte b = pixels[pixelIndex + 2];
					if (r != 0 || g != 0 || b != 0)
					{
						bitmap.Set(x,  y, true);
					}
				}
			}

			return bitmap;
		}

		public override void Combine(List<IFontConverter> conversions)
		{
			foreach (var conversion in conversions)
			{
				if (conversion is not TypefaceConverter converter)
					continue;
				_pixelExtentsRange.Include(converter._pixelExtentsRange);
			}
		}

		public override IFontConverter Convert()
		{
			if (Widths == null) throw new InvalidOperationException("Call Read before Convert");

			CharacterData = new string[Characters];
			Spacing = (int)Math.Ceiling(_glyphWidths.Average());
			Height = _pixelExtentsRange.Height;
			var conversionResults = _conversion.Result;
			if (Height > conversionResults.TallestCharacter)
			{
				conversionResults.TallestCharacter = Height;
			}
			PagesHigh = (Height >> 3) + ((Height & 0x7) == 0 ? 0 : 1);
			if (PagesHigh > conversionResults.HeightInPages)
			{
				conversionResults.HeightInPages = PagesHigh;
			}

			var index = 0;
			List<decimal> weightedWidths = new();
			var totalWeight = 0m;
			foreach (var bitmap in _bitmaps)
			{
				totalWeight += ConvertCharacter(index++, bitmap, weightedWidths, conversionResults);
			}

			if (totalWeight > 0) Width = (int) Math.Ceiling(weightedWidths.Sum() / totalWeight) + Spacing;
			else Width = Widths.GroupBy(w => w).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() + Spacing;

			conversionResults.Ranges++;
			// todo depends on output format
			conversionResults.TotalBytes += 11; // 11 bytes for the DCfont structure
			return this;
		}

		// todo depends on output format
		private decimal ConvertCharacter(int index, BoolBitmap bitmap, List<decimal> weightedWidths, ConversionResults conversionResults)
		{
			if (Widths == null || CharacterData == null)
				throw new InvalidOperationException();

			conversionResults.Characters++;
			conversionResults.TotalBytes++; // 1 byte for the width
			if (index % 16 == 0)
			{
				conversionResults.TotalBytes += 2; // Each 16 characters takes another 2 bytes for 16 widths takes 
			}

			var characterWidth = Widths[index];
			if (characterWidth == 0)
			{
				conversionResults.MissingCharacters++;
				return 0m;
			}

			CharacterData[index] = ConvertCharacterPageData(bitmap, conversionResults);

			int ascii = FirstCharacter + index;
			var characterWidthWeight = GetWidthWeight(characterWidth, weightedWidths, ascii);
			return characterWidthWeight;
		}

		private string ConvertCharacterPageData(BoolBitmap bitmap, ConversionResults conversionResults)
		{
			var pageData = new StringBuilder();
			for (int page = 0; page < PagesHigh; page++)
			{
				ConvertCharacterSinglePage(bitmap, conversionResults, page, pageData);
			}

			return pageData.ToString();
		}

		private void ConvertCharacterSinglePage(BoolBitmap bitmap, ConversionResults conversionResults, int page,
			StringBuilder pageData)
		{
			var minX = bitmap.Coverage.Min.X;
			var maxX = bitmap.Coverage.Max.X;
			var minY = _pixelExtentsRange.Min.Y;
			for (var bitmapX = minX; bitmapX <= maxX; bitmapX++)
			{
				var b = 0;
				for (var bitNUmber = 0; bitNUmber < 8; bitNUmber++)
				{
					var bitmapY = page * 8 + bitNUmber + minY;
					b += bitmap.Get(bitmapX , bitmapY) ? 1 << bitNUmber : 0;
				}

				if (pageData.Length > 0) pageData.Append(',');
				pageData.Append($"0x{b:x2}");
				conversionResults.PixelBytes++;
				conversionResults.TotalBytes++;
			}
		}

		private decimal GetWidthWeight(int characterWidth, List<decimal> weightedWidths, int ascii)
		{
			decimal addedWeight = 0m;
			decimal ww = 0m;
			if (ascii is >= 0 and <= 127)
			{
				addedWeight = CharacterFrequencies.Freq(ascii);
				ww = addedWeight * characterWidth;
			}

			weightedWidths.Add(ww);
			return addedWeight;
		}

		// skipping the write out for now
		//public override IFluidFontConverter Write(DirectoryInfo destFontDirectoryInfo)
		//{
		//	return this;
		//}

		private static RenderTargetBitmap? GetBitmap(int codepoint, Typeface typeface, double emSize, double pixelsPerDip)
		{
			// Create the initial formatted text string.
			var formattedText = new FormattedText(
				$"{char.ConvertFromUtf32(codepoint)}",
				CultureInfo.GetCultureInfo("en-us"),
				FlowDirection.LeftToRight,
				typeface,
				emSize / pixelsPerDip,
				Brushes.White, null, TextFormattingMode.Display, pixelsPerDip);

			//var baseline = formattedText.Baseline;
			var textHeight = formattedText.Height;
			//var lineHeight = formattedText.LineHeight;
			//var overhangAfter = formattedText.OverhangAfter;
			//var typefaceCapsHeight = typeface.CapsHeight * emSize;
			//var typefaceXHeight = typeface.XHeight * emSize;
			//var typefaceUnderlinePosition = typeface.UnderlinePosition * emSize;
			//var typefaceUnderlineThickness = typeface.UnderlineThickness * emSize;

			var group = new DrawingGroup();
			SetRenderingOptions(group);
			using (var context = @group.Open())
			{
				context.DrawText(formattedText, new Point(0, emSize * pixelsPerDip));
			}

			var bounds = @group.Bounds;
			if (bounds == Rect.Empty)
				return null;

			var blackBackground =
				new GeometryDrawing(Brushes.Black, null, new RectangleGeometry(new Rect(0, 0,bounds.Width, textHeight)));
			// var brush = Brushes.Black.Clone();
			// brush.Opacity = 0;
			// var background =
			// 	new GeometryDrawing(brush, null, new RectangleGeometry(new Rect(0, 0,bounds.Width, textHeight)));
			var drawingGroup = new DrawingGroup
			{
				Children = new DrawingCollection
				{
					blackBackground,
					//background,
					@group
				}
			};

			var drawing = new DrawingImage(drawingGroup);
			var width = drawing.Width;
			var height = drawing.Height; // textHeight
			var size = new Size(width, height);
			var rect = new Rect(size);
			var image = new Image {Source = drawing};
			image.Arrange(rect);
			SetRenderingOptions(image);

			var pixelWidth = (int) (width * pixelsPerDip);
			var pixelHeight = (int) (height * pixelsPerDip);
			if (pixelWidth == 0)
				return null;

			//if (overhangAfter > 0 || pixelHeight == 34)
			//	return null;

			double dpiX = 96d * pixelsPerDip;
			double dpiY = 96d * pixelsPerDip;
			var renderBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
			renderBitmap.Render(image);
			return renderBitmap;
		}

		private static void SetRenderingOptions(DependencyObject target)
		{
			TextOptions.SetTextRenderingMode(target, TextRenderingMode.Aliased);
			TextOptions.SetTextHintingMode(target, TextHintingMode.Fixed);
			TextOptions.SetTextFormattingMode(target, TextFormattingMode.Display);
			RenderOptions.SetBitmapScalingMode(target, BitmapScalingMode.Fant);
			//RenderOptions.SetCachingHint(target, CachingHint.Unspecified);
			//RenderOptions.SetEdgeMode(target, EdgeMode.Unspecified);
			//RenderOptions.SetClearTypeHint(target, ClearTypeHint.Auto);
		}

		public int Render(Rune character, BoolBitmap previewArray, Cursor cursor)
		{
			if (Widths == null) throw new InvalidOperationException("Call Read before rendering");

			var index = character.Value - FirstCharacter;
			var characterWidth = Widths[index];
			if (characterWidth == 0)
				return 0;

			var bitmap = _bitmaps[index];

			for (int p = 0; p < PagesHigh; p++)
			{
				RenderPage(previewArray, cursor, p, bitmap);
			}

			return characterWidth;
		}

		private void RenderPage(BoolBitmap previewArray, Cursor cursor, int p, BoolBitmap bitmap)
		{
			var pixelLocationX = 0;
			var minX = bitmap.Coverage.Min.X;
			var maxX = bitmap.Coverage.Max.X;
			var minY = _pixelExtentsRange.Min.Y;
			for (int bitmapX = minX; bitmapX <= maxX; bitmapX++)
			{
				var pixelLocationY = p * 8;
				for (int l = 0; l < 8; l++)
				{
					var bitmapY = p * 8 + l + minY;
					previewArray.Set(cursor, pixelLocationX, pixelLocationY, bitmap.Get(bitmapX, bitmapY));
					pixelLocationY++;
				}
				pixelLocationX++;
			}
		}

		public int CharacterWidth(Rune character)
		{
			return Widths?[character.Value - FirstCharacter] ?? 0;
		}
	}
}