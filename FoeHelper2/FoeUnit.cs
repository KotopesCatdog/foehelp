using System.Drawing;

namespace FoeHelper2;

internal class FoeUnit
{
	public DirectBitmap unitBmp;

	public DirectBitmap healthBar;

	private static Color[] healthyGreens = new Color[10]
	{
		Color.FromArgb(120, 175, 41),
		Color.FromArgb(127, 185, 43),
		Color.FromArgb(147, 197, 77),
		Color.FromArgb(134, 196, 45),
		Color.FromArgb(164, 211, 98),
		Color.FromArgb(140, 204, 47),
		Color.FromArgb(181, 222, 120),
		Color.FromArgb(194, 229, 141),
		Color.FromArgb(149, 197, 82),
		Color.FromArgb(131, 198, 85)
	};

	private Color[] healthBarColors = new Color[10];

	public int UnitId { get; set; }

	public Point CenterPoint { get; set; }

	public bool mChecked { get; set; }

	public bool isMissing { get; set; }

	public bool Removed { get; set; }

	public bool Empty { get; set; }

	public int Health { get; set; }

	public float healthBarXdistance { get; set; }

	public FoeUnit()
	{
		healthBarXdistance = 5f * ImageWorks.zoomLevel;
	}

	public FoeUnit(DirectBitmap bmp, int n, Point center)
	{
		unitBmp = bmp;
		UnitId = n;
		CenterPoint = center;
		healthBarXdistance = 5f * ImageWorks.zoomLevel;
	}

	public override string ToString()
	{
		return $"id: {UnitId} center:{CenterPoint} isMissing:{isMissing} Removed:{Removed} Empty:{Empty} Health:{Health} ";
	}

	public Bitmap GetHealthBar()
	{
		return healthBar.Bitmap;
	}

	public Bitmap GetUnitBmp()
	{
		return unitBmp.Bitmap;
	}

	public static DirectBitmap getHealthBar(Point p1)
	{
		Point a = FoeTools.coord.MoveRelative(new Point(0, 52), p1, FoeTools.coord.useZoom);
		Point b = FoeTools.coord.MoveRelative(new Point(52, 59), p1, FoeTools.coord.useZoom);
		return PixelTools.CaptureRegionDBM(a, b);
	}

	public void AddHealthBar2(DirectBitmap bmp)
	{
		healthBar = bmp;
		int width = bmp.Width;
		int y = 4;
		Health = 0;
		bool flag = true;
		for (int i = 0; i < width; i++)
		{
			if (PixelTools.CompareColors(bmp.GetPixel(i, y), healthyGreens, 0.8))
			{
				if (flag)
				{
					Health++;
				}
				flag = false;
			}
			else
			{
				flag = true;
			}
		}
	}

	public DirectBitmap getDBM()
	{
		return unitBmp;
	}
}
