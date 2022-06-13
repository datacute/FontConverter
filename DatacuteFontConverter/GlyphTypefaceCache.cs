using System.Collections.Generic;
using System.Windows.Media;

namespace DatacuteFontConverter;

public class GlyphTypefaceCache : Dictionary<Typeface, GlyphTypeface>
{
	private static readonly GlyphTypefaceCache Cache = new();
	public static bool TryGet(Typeface typeface, out GlyphTypeface glyphTypeface)
	{
		// Typeface only caches 4 GlyphTypefaces.
		// Lets cache all of the ones we access.
		if (Cache.TryGetValue(typeface, out var glyphTypefaceFromCache))
		{
			glyphTypeface = glyphTypefaceFromCache;
			return true;
		}
		if (!typeface.TryGetGlyphTypeface(out glyphTypeface)) return false;
		Cache.Add(typeface, glyphTypeface);
		return true;
	} 	
}