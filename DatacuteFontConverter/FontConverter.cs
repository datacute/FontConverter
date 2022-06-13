using System.Collections.Generic;

namespace DatacuteFontConverter;

public abstract class FontConverter : IFontConverter
{
	public virtual IFontConverter Read() => this;
		
	public virtual void Combine(List<IFontConverter> conversions) { }
	public virtual IFontConverter Convert() => this;
	
	protected int Characters;
	public string?[]? CharacterData;
	public int[]? Widths;

	public int FirstCharacter;
	public int LastCharacter;
	public int Width;
	public int Height;
	public int PagesHigh;
	public int Spacing;
	public int SpaceWidth;

	public virtual IFontConverter Write(IFontWriter fontWriter)
	{
		if (CharacterData == null || CharacterData.Length == 0) return new ErrorReportingFontConverter("Output file would have no characters");

		return fontWriter.Write(this);
	}

}