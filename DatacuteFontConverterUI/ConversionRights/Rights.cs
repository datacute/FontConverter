using System;

namespace DatacuteFontConverterUI.ConversionRights
{
	[Flags]
	public enum Rights
	{
		Unknown = 0,
		ConversionRestricted = 1,
		ConversionAllowed = 2,
		CheckReservedFontNames = 4,
	}
}