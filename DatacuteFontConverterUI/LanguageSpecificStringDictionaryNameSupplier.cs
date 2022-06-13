using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

namespace DatacuteFontConverterUI
{
	public class LanguageSpecificStringDictionaryNameSupplier
	{
		protected readonly XmlLanguage _language;

		public LanguageSpecificStringDictionaryNameSupplier(XmlLanguage language)
		{
			_language = language;
		}

		public string GetName(IDictionary<XmlLanguage, string> dictionary)
		{
			if (dictionary?.Keys == null) return string.Empty;
			if (dictionary.TryGetValue(_language, out var typefaceName)) return typefaceName;
			return dictionary.Values?.FirstOrDefault() ?? string.Empty;
		}

	}
}