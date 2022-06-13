using System.Collections.Generic;

namespace DatacuteFontConverter;

public interface IFontWriter
{
	FontConverter Write(FontConverter converter);
	void WriteUnicodeFont(List<IFontConverter> conversions);
}