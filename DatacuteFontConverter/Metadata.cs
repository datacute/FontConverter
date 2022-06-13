using System.Text;

namespace DatacuteFontConverter;

public class Metadata
{
	public string? FontFamily { get; init; }
	public string? Typeface { get; init; }
	public string? Copyright { get; init; }
	public string? Description { get; init; }
	public string? LicenceDescription { get; init; }
	public string? ManufacturerName { get; init; }
	public string? Trademark { get; init; }
	public string? VendorUrl { get; init; }
	public string? Version { get; init; }

	public override string ToString()
	{
		StringBuilder sb = new();
		sb.AppendLine("  Converted by Datacute Font Converter");
		sb.AppendLine("  Source Information:");
		sb.AppendLine($"    Font Family: {FontFamily}");
		sb.AppendLine($"    Typeface: {Typeface}");
		if (!string.IsNullOrWhiteSpace(ManufacturerName)) sb.AppendLine($"    Manufacturer: {ManufacturerName}");
		if (!string.IsNullOrWhiteSpace(Description)) sb.AppendLine($"    Description: {Description}");
		if (!string.IsNullOrWhiteSpace(Version)) sb.AppendLine($"    Version: {Version}");
		if (!string.IsNullOrWhiteSpace(VendorUrl)) sb.AppendLine($"    Vendor URL: {VendorUrl}");
		if (!string.IsNullOrWhiteSpace(Trademark)) sb.AppendLine($"    Trademark: {Trademark}");
		if (!string.IsNullOrWhiteSpace(Copyright)) sb.AppendLine($"    Copyright: {Copyright}");
		if (!string.IsNullOrWhiteSpace(LicenceDescription))
		{
			if (LicenceDescription.Length > 250)
				sb.AppendLine($"    Licence at end of file.");
			else
				sb.AppendLine($"    Licence: {LicenceDescription}");
		}
		return sb.ToString();

	}
}