using System;
using System.IO;

namespace DatacuteFontConverter;

public class ErrorReportingFontConverter : FontConverter
{
	// todo present errors to the user
	private readonly string _message;
	public ErrorReportingFontConverter(string message) => _message = message;
	public override IFontConverter Write(IFontWriter fontwriter) => this;
}