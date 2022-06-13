using System.IO;

namespace DatacuteFontConverter;

public class ItemCombiner
{
	public static readonly ItemCombiner Comma = new(",");
	public static readonly ItemCombiner Plus = new("+");

	private readonly string _combiner;
	private ItemCombiner(string combiner)
	{
		_combiner = combiner;
	}

	public void CombineItems(StreamWriter writer, int itemsWritten)
	{
		//newline every 16
		if (itemsWritten % 16 == 0)
		{
			if (itemsWritten > 0) writer.WriteLine(",");
			writer.Write("  ");
		}
		else
		{
			if (itemsWritten > 0) writer.Write(_combiner);
		}
	}

}