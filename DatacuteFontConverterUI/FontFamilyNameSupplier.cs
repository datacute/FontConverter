using System.Windows.Markup;
using System.Windows.Media;

namespace DatacuteFontConverterUI
{
	public class FontFamilyNameSupplier : LanguageSpecificStringDictionaryNameSupplier
	{
		public FontFamilyNameSupplier(XmlLanguage language) : base(language)
		{
		}

		public string GetName(FontFamily fontFamily)
		{
			return GetName(fontFamily.FamilyNames);
		}

	}
}