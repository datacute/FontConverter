# Datacute Font Converter
This is a windows application for converting OpenType fonts to a format that is able to be used with the [Tiny4kOLED](https://github.com/datacute/Tiny4kOLED) library.

Currently, fonts need to be installed to show up in the application.
The main screen has sections to select the font and typeface, and filters to help prepare the ranges of characters to convert.

![Main screen](https://user-images.githubusercontent.com/325854/171967217-6bff17d8-c058-4eab-94af-a236342e3961.png)

The (i) icon opens a window displaying details about the selected typeface:

![Typeface details screen](https://user-images.githubusercontent.com/325854/171967434-0288171c-45e2-4afe-af8d-f0e6446e7a36.png)

Since most copyright and licenses either do not grant the rights, or explicitly prohibit converting font files to other formats, this application requires you to read the licenses, and configure rules used to determine whether a specific typeface is able to be converted:

![Typeface details screen](https://user-images.githubusercontent.com/325854/171967494-193eba77-369b-45a0-a5e8-2e0cfa61120e.png)

Converting a font to a different format isn't guaranteed to preserve everything about the font, such as its kerning and hinting. Conversion bugs or introduced limitation, character remapping, or producing a subset of the font, can result in the text appearing in such a way that the copyright holder would not want it being identified as nor mistaken for their font. SIL's Open Font License includes the idea of "Reserved Font Names" so that copyright holders can allow font conversion, so long as the resulting work is not identified with the same name.

This part of the application to maintain conversion rights is not yet complete, and pull requests are welcomed. See [Github Issue #1](https://github.com/datacute/FontConverter/issues/1)

If you have used the application to produce a font that you wish to share, then add a pull request to the [TinyOLED-Fonts](https://github.com/datacute/TinyOLED-Fonts) library.
