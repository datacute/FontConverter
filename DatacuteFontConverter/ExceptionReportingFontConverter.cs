using System;
using System.IO;

namespace DatacuteFontConverter;

public class ExceptionReportingFontConverter : ErrorReportingFontConverter
{
	// todo present errors to the user
	private readonly Exception _exception;
	public ExceptionReportingFontConverter(Exception exception) : base(exception.Message) => _exception = exception;
	public override IFontConverter Write(IFontWriter fontwriter) => this;
}