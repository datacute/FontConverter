using System.Collections.Generic;
using System.Linq;

namespace DatacuteFontConverter;

public class CodepointRange // like System.Text.Unicode.UnicodeRange but with enumerable, and mutable
{
	public int From { get; set; }

	public int To { get; set; }

	public int Count => To - From + 1;

	public CodepointRange(int from, int to)
	{
		From = @from;
		To = to;
	}

	public IEnumerator<int> GetEnumerator() => AsEnumerable().GetEnumerator();

	public IEnumerable<int> AsEnumerable() => Enumerable.Range(From, Count);

	public bool Contains(int value) => From <= value && value <= To;
}