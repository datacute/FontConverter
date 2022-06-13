using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace DatacuteFontConverterUI
{
	public class UnicodeValueConverter : MarkupExtension, IValueConverter
	{
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertToType(value, targetType);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertToType(value, targetType);
		}

		private static object ConvertToType(object value, Type targetType)
		{
			try
			{
				if (targetType == typeof(object)) return value;

				if (targetType == typeof(string) && value is double doubleValue)
					return Describer.UnicodeNumber((int)doubleValue);
				if (targetType == typeof(double) && value is string stringValueForDouble)
					return (double)Describer.UnicodeNumberParse(stringValueForDouble);

				if (targetType == typeof(string) && value is int intValue)
					return Describer.UnicodeNumber(intValue);
				if (targetType == typeof(int) && value is string stringValueForInt)
					return Describer.UnicodeNumberParse(stringValueForInt);

				var converter = TypeDescriptor.GetConverter(targetType);
				return converter.ConvertFrom(value.ToString());
			}
			catch (Exception e)
			{
				return value;
			}
		}
	}
}