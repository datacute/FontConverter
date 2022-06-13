using DatacuteFontConverter;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DatacuteFontConverterUI;

public class CodepointRangeProperty : INotifyPropertyChanged
{
	private readonly CodepointRange _codepointRange;

	public int From
	{
		get => _codepointRange.From;
		set
		{
			if (_codepointRange.From != value)
			{
				_codepointRange.From = value;
				OnPropertyChanged(nameof(From));
				OnPropertyChanged(nameof(FromDesc));
				OnPropertyChanged(nameof(Example));
			}
		}
	}

	public int To
	{
		get => _codepointRange.To;
		set
		{
			if (_codepointRange.To != value)
			{
				_codepointRange.To = value;
				OnPropertyChanged(nameof(To));
				OnPropertyChanged(nameof(ToDesc));
				OnPropertyChanged(nameof(Example));
			}
		}
	}

	public string FromDesc => $"{Describer.Codepoint(_codepointRange.From)}";
	public string ToDesc => $"{Describer.Codepoint(_codepointRange.To)}";

	public string Example => Describer.UnicodeRangeSample(_codepointRange.From, _codepointRange.To);

	public CodepointRangeProperty(int from, int to)
	{
		_codepointRange = new CodepointRange(from, to);
	}

	public IEnumerator<int> GetEnumerator() => AsEnumerable().GetEnumerator();

	public IEnumerable<int> AsEnumerable() => Enumerable.Range(From, To - From + 1);

	public event PropertyChangedEventHandler PropertyChanged;

	public void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public CodepointRangeProperty Intersection(CodepointRangeProperty codepointRange)
	{
		if (codepointRange == null) return null;

		var from = System.Math.Max(_codepointRange.From, codepointRange._codepointRange.From);
		var to = System.Math.Min(_codepointRange.To, codepointRange._codepointRange.To);
		if (from > to)
		{
			return null;
		}

		return new CodepointRangeProperty(from, to);
	}

	public bool Intersects(CodepointRangeProperty codepointRange)
	{
		if (codepointRange == null) return false;

		var from = System.Math.Max(_codepointRange.From, codepointRange._codepointRange.From);
		var to = System.Math.Min(_codepointRange.To, codepointRange._codepointRange.To);
		return from <= to;
	}

	public bool Contains(int c) => _codepointRange.From <= c && c <= _codepointRange.To;

	public CodepointRange AsRange() => new(From, To);
}