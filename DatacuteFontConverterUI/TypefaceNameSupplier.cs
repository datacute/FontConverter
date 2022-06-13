using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace DatacuteFontConverterUI
{
	public class TypefaceNameSupplier : LanguageSpecificStringDictionaryNameSupplier
	{
		private readonly bool _indicateSimulated;

		public TypefaceNameSupplier(XmlLanguage language, bool indicateSimulated = true) : base(language)
		{
			_indicateSimulated = indicateSimulated;
		}

		public string GetName(Typeface typeface)
		{
			// find FamilyTypefaces
			var familyTypeface = typeface.FontFamily.FamilyTypefaces.FirstOrDefault(ftf =>
				ftf.Stretch == typeface.Stretch &&
				ftf.Style == typeface.Style &&
				ftf.Weight == typeface.Weight);

			var name = GetName(familyTypeface != null ? familyTypeface.AdjustedFaceNames : typeface.FaceNames);
			if (_indicateSimulated && (typeface.IsBoldSimulated || typeface.IsObliqueSimulated)) name += " (Simulated)";
			return name;
		}
	}
}