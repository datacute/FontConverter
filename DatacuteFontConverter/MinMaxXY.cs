using System;

namespace DatacuteFontConverter;

public class MinMaxXY
{
	public XY Min { get; }
	public XY Max { get; }

	private MinMaxXY(int minX, int minY, int maxX, int maxY)
	{
		Min = new XY(minX, minY);
		Max = new XY(maxX, maxY);
	}

	public MinMaxXY() : this(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue)
	{
	}

	public void Include(int x, int y)
	{
		Min.X = Math.Min(Min.X, x);
		Min.Y = Math.Min(Min.Y, y);
		Max.X = Math.Max(Max.X, x);
		Max.Y = Math.Max(Max.Y, y);
	}

	public void Include(MinMaxXY minMaxXy)
	{
		Min.X = Math.Min(Min.X, minMaxXy.Min.X);
		Min.Y = Math.Min(Min.Y, minMaxXy.Min.Y);
		Max.X = Math.Max(Max.X, minMaxXy.Max.X);
		Max.Y = Math.Max(Max.Y, minMaxXy.Max.Y);
	}

	public int Width => Max.X - Min.X + 1;
	public int Height => Max.Y - Min.Y + 1;
}