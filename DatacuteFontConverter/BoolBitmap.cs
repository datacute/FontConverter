namespace DatacuteFontConverter;

public class BoolBitmap
{
	private readonly bool[] _bitmap;
	public int Width { get; }
	public int Height { get; }
	public MinMaxXY Coverage { get; } = new();

	public BoolBitmap(int width, int height)
	{
		Width = width;
		Height = height;
		_bitmap = new bool[width * height];
	}

	public void Set(XY offset, int x, int y, bool value)
	{
		Set(offset.X + x, offset.Y + y, value);
	}

	public void Set(int x, int y, bool value)
	{
		if (x < Width && y < Height)
		{
			_bitmap[y * Width + x] = value;
			if (value)
				Coverage.Include(x, y);
		}
	}

	public bool Get(int x, int y)
	{
		if (x < Width && y < Height)
			return _bitmap[y * Width + x];
		return false;
	}
}