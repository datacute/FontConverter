namespace DatacuteFontConverter;

public class ConversionResults
{
	public int Ranges { get; set; }
	public int Characters { get; set; }
	public int MissingCharacters { get; set; }
	public int HeightInPages { get; set; }
	public int TallestCharacter { get; set; }
	// todo this need to be determined based on the output format
	public int PixelBytes { get; set; }
	// todo this need to be determined based on the output format
	public int TotalBytes { get; set; }
}