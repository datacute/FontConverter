
using System.Collections.Generic;

namespace DatacuteFontConverter;

public interface IConversionController
{
	public List<IFontConverter> Convert(IFontWriter fontwriter);
	public void Preview(string text, BoolBitmap previewArray);
}