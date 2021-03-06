namespace DatacuteFontConverter;

/// <summary>
/// These ascii character frequencies are used for describing a font's width
/// The width of more frequently used characters carry a greater weighting
/// than the width of less frequently used characters
/// </summary>
/// <remarks>
/// Source https://link.springer.com/content/pdf/10.3758%2FBF03195586.pdf
/// </remarks> 
public static class CharacterFrequencies
{
	private static readonly decimal[] F = {
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00000m,
		0.00003m,
		0.00401m,
		0.00001m,
		0.00073m,
		0.00003m,
		0.00009m,
		0.00288m,
		0.00075m,
		0.00076m,
		0.00029m,
		0.00001m,
		0.01387m,
		0.00355m,
		0.01332m,
		0.00011m,
		0.00769m,
		0.00649m,
		0.00470m,
		0.00264m,
		0.00271m,
		0.00527m,
		0.00217m,
		0.00169m,
		0.00257m,
		0.00398m,
		0.00076m,
		0.00052m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.00017m,
		0.00001m,
		0.00396m,
		0.00239m,
		0.00323m,
		0.00183m,
		0.00195m,
		0.00142m,
		0.00131m,
		0.00174m,
		0.00314m,
		0.00111m,
		0.00066m,
		0.00151m,
		0.00365m,
		0.00289m,
		0.00149m,
		0.00203m,
		0.00016m,
		0.00206m,
		0.00429m,
		0.00458m,
		0.00081m,
		0.00044m,
		0.00151m,
		0.00011m,
		0.00133m,
		0.00008m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.07411m,
		0.01219m,
		0.02760m,
		0.03336m,
		0.10899m,
		0.01826m,
		0.01699m,
		0.04161m,
		0.06374m,
		0.00093m,
		0.00649m,
		0.03594m,
		0.02066m,
		0.06385m,
		0.06658m,
		0.01768m,
		0.00076m,
		0.05826m,
		0.05894m,
		0.07754m,
		0.02271m,
		0.00920m,
		0.01430m,
		0.00174m,
		0.01495m,
		0.00094m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.00001m,
		0.00001m
	};
	
	public static decimal Freq(int asciiCode) => F[asciiCode];
}