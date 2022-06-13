using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DatacuteFontConverter;
using DatacuteFontConverterUI.ConversionRights;
using StartupEventArgs = System.Windows.StartupEventArgs;

namespace DatacuteFontConverterUI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public static readonly Dictionary<Typeface, Rights> RightsCache = new();

		public static bool TryGetGlyphTypeface(Typeface typeface, out GlyphTypeface glyphTypeface)
		{
			return GlyphTypefaceCache.TryGet(typeface, out glyphTypeface);
		} 

		private void AppStartup(object sender, StartupEventArgs args)
		{
			var prevShutdownMode = Current.ShutdownMode;
			Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			var rightsAcknowledgementResult = MessageBox.Show(
				@"This Datacute Font Converter application produces modified versions of fonts, which are considered derived works.

Please read the license and copyright of the font you wish to convert.

While TrueType fonts contain ""Embedding Rights"" such as ""Installable"", embedding rights do NOT imply the right to convert the fonts to new formats.

Microsoft does NOT grant the right to convert fonts that come with Windows.

The SIL Open Font License allows format conversion, but requires that the font is renamed to not use any of the Reserved Font Names listed in the copyright.

This Datacute Font Conversion application must ONLY be used to convert fonts that you have the right to convert.",
				"Font Format Conversion Legal Rights",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Question,
				MessageBoxResult.Cancel);
			if (rightsAcknowledgementResult != MessageBoxResult.OK) 
				Current.Shutdown();
			else
				Current.ShutdownMode = prevShutdownMode;
		}
	}
}