using System.Collections.Generic;

namespace DatacuteFontConverter;

public interface IFontConverter
{
	IFontConverter Read();
	IFontConverter Convert();
	IFontConverter Write(IFontWriter fontWriter);
	void Combine(List<IFontConverter> conversions);
}