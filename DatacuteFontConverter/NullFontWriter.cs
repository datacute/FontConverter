using System.Collections.Generic;

namespace DatacuteFontConverter;

public class NullFontWriter : IFontWriter
{
	public void SetFontBaseName(string fontBaseName)
	{
	}

	public FontConverter Write(FontConverter converter)
	{
		return converter;
	}

	public void WriteUnicodeFont(List<IFontConverter> conversions)
	{
	}
}