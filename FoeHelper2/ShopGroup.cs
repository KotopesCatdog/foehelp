using System.Drawing;

namespace FoeHelper2;

internal class ShopGroup
{
	private string[] colors;

	private int xStart;

	private int yStart;

	private float ResizeX;

	private float ResizeY;

	private string iconFn;

	public int[] Counts { get; private set; }

	public Point StartPoint { get; private set; }

	public Point LastAbsolute { get; set; }

	public Point[] RadekAbs { get; set; }

	public int Num { get; set; }

	public bool Found { get; set; }

	public int FoundBmpNum { get; set; }

	public Bitmap icon { get; set; }

	public Bitmap foundPic { get; set; }

	public Bitmap countsPic { get; set; }

	public ShopGroup(int num)
	{
		Num = num;
		Counts = new int[5];
		RadekAbs = new Point[5];
	}

	public ShopGroup(string[] findColors, int num)
	{
		colors = findColors;
		Num = num;
		RadekAbs = new Point[5];
	}

	public void addBmp(string path, float resizeX = 1f, float resizeY = 1f)
	{
		icon = ImageWorks.getBmp(path, resizeX, resizeY);
	}

	public void addBmp(Bitmap bmp, float resizeX = 1f, float resizeY = 1f)
	{
		icon = ImageWorks.getBmp(bmp, resizeX, resizeY);
	}

	public void addBmpFile(string fileName, float resizeX = 1f, float resizeY = 1f)
	{
		iconFn = fileName;
		ResizeX = resizeX;
		ResizeY = resizeY;
	}

	public Bitmap getBmp()
	{
		return ImageWorks.getBmp(iconFn, ResizeX, ResizeY);
	}

	public void debugIcon()
	{
		((Image)icon).Save("c:\\mik\\shopicon " + Num + ".png");
	}

	public bool findStart(DirectBitmap bmp)
	{
		try
		{
			Point value = PixelTools.FindPixelSequenceLeftToRight(colors, 0, -1, 0, -1, bmp).Value;
			xStart = value.X;
			yStart = value.Y;
			StartPoint = new Point(xStart, yStart);
			Found = true;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void addCount(int radek, string foundText)
	{
		foundText = foundText.Replace("\r", "");
		foundText = foundText.Replace("\n", "");
		foundText = foundText.Replace(" ", "");
		int num = 0;
		try
		{
			num = int.Parse(foundText);
		}
		catch
		{
			FoeTools.outInfo("goods count read failed..");
		}
		Counts[radek] = num;
	}

	public bool findStartBmp(DirectBitmap bmp1, DirectBitmap bmp2, int minimalY = 0)
	{
		float num = 0.9f;
		Point? point = ImageWorks.FindTemplateEmgu(bmp1.Bitmap, getBmp(), num);
		if (FoeTools.DebugMode)
		{
			_ = FoeTools.debugDouble2;
		}
		if (point.HasValue)
		{
			FoundBmpNum = 1;
		}
		else
		{
			point = ImageWorks.FindTemplateEmgu(bmp2.Bitmap, getBmp(), num);
			if (FoeTools.DebugMode)
			{
				_ = FoeTools.debugDouble2;
			}
			if (point.HasValue)
			{
				FoundBmpNum = 2;
			}
		}
		if (point.HasValue)
		{
			Point value = point.Value;
			if (value.Y < minimalY)
			{
				return false;
			}
			xStart = value.X;
			yStart = value.Y;
			StartPoint = new Point(xStart, yStart);
			Found = true;
			return true;
		}
		_ = FoeTools.DebugMode;
		return false;
	}
}
