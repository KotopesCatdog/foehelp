using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace FoeHelper2;

internal static class PixelTools
{
	private static Bitmap screenPixel = new Bitmap(1, 1, (PixelFormat)2498570);

	private static Bitmap wholeScreen = new Bitmap(MaxX, MaxY, (PixelFormat)2498570);

	private static Dictionary<string, Bitmap> imagesPool;

	private static double ColorPrecision = 0.0;

	private static Graphics mGraph;

	private static int[,] mScreenColors;

	private static DirectBitmap dbm;

	public static bool pointNotFound = false;

	public static int MaxX => Screen.PrimaryScreen.Bounds.Width - 1;

	public static int MaxY => Screen.PrimaryScreen.Bounds.Height - 1;

	[DllImport("user32.dll")]
	public static extern IntPtr GetDC(IntPtr hwnd);

	[DllImport("user32.dll")]
	public static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(ref Point lpPoint);

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

	public static void reinit()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		setAcceptableColorPrecision();
		screenPixel = new Bitmap(1, 1, (PixelFormat)2498570);
		wholeScreen = new Bitmap(MaxX, MaxY, (PixelFormat)2498570);
	}

	public static int StrColorToInt(string color)
	{
		if (!color.Substring(0, 1).Equals("#"))
		{
			color = "#" + color;
		}
		return ColorTranslator.FromHtml(color).ToArgb();
	}

	public static Color StrColorToColor(string color)
	{
		if (!color.Substring(0, 1).Equals("#"))
		{
			color = "#" + color;
		}
		return ColorTranslator.FromHtml(color);
	}

	public static string IntColorToStr(int color)
	{
		return ColorTranslator.ToHtml(Color.FromArgb(color));
	}

	public static void setAcceptableColorPrecision(double prec = 0.5)
	{
		ColorPrecision = prec;
	}

	public static Color IntColorToColor(int color)
	{
		return Color.FromArgb(color);
	}

	public static Color GetColorAt(int x, int y)
	{
		Graphics val = Graphics.FromImage((Image)(object)screenPixel);
		try
		{
			Graphics val2 = Graphics.FromHwnd(IntPtr.Zero);
			try
			{
				IntPtr hdc = val2.GetHdc();
				BitBlt(val.GetHdc(), 0, 0, 1, 1, hdc, x, y, 13369376);
				val.ReleaseHdc();
				val2.ReleaseHdc();
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return screenPixel.GetPixel(0, 0);
	}

	public static Color GetColorAt(Point p)
	{
		return GetColorAt(p.X, p.Y);
	}

	public static List<PixelColor> GetColorsAt(Point p, int xTolerance = 0, int yTolerance = 0)
	{
		List<PixelColor> list = new List<PixelColor>();
		for (int i = p.X - xTolerance; i < p.X + xTolerance + 1; i++)
		{
			for (int j = p.Y - yTolerance; j < p.Y + yTolerance + 1; j++)
			{
				Point p2 = new Point(i, j);
				list.Add(new PixelColor(i, j, GetColorAt(p2)));
			}
		}
		return list;
	}

	public static List<PixelColor> GetColorsAt(Point p, int xToleranceMin = 0, int yToleranceMin = 0, int xToleranceMax = 0, int yToleranceMax = 0)
	{
		List<PixelColor> list = new List<PixelColor>();
		for (int i = p.X - xToleranceMin; i < p.X + xToleranceMax; i++)
		{
			for (int j = p.Y - yToleranceMin; j < p.Y + yToleranceMax; j++)
			{
				Point p2 = new Point(i, j);
				list.Add(new PixelColor(i, j, GetColorAt(p2)));
			}
		}
		return list;
	}

	public static int GetColorIntAt(int x, int y)
	{
		Graphics val = Graphics.FromImage((Image)(object)screenPixel);
		try
		{
			Graphics val2 = Graphics.FromHwnd(IntPtr.Zero);
			try
			{
				IntPtr hdc = val2.GetHdc();
				BitBlt(val.GetHdc(), 0, 0, 1, 1, hdc, x, y, 13369376);
				val.ReleaseHdc();
				val2.ReleaseHdc();
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return screenPixel.GetPixel(0, 0).ToArgb();
	}

	public static Point getResolution()
	{
		return new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
	}

	public static int[] GetHorizontalLineColors2(int xStart, int xEnd, int y)
	{
		int[] array = new int[xEnd - xStart];
		DirectBitmap directBitmap = CaptureDBM(xStart, y, xEnd, y);
		for (int i = 0; i < xEnd - xStart; i++)
		{
			array[i] = directBitmap.GetPixelInt(i, 0);
		}
		return array;
	}

	public static void CaptureScreenSlow()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		mGraph = Graphics.FromImage((Image)(object)wholeScreen);
		mGraph.CopyFromScreen(0, 0, 0, 0, ((Image)wholeScreen).Size);
		mScreenColors = new int[MaxX, MaxY];
		BitmapData val = wholeScreen.LockBits(new Rectangle(0, 0, ((Image)wholeScreen).Width, ((Image)wholeScreen).Height), (ImageLockMode)3, ((Image)wholeScreen).PixelFormat);
		IntPtr scan = val.Scan0;
		int num = Math.Abs(val.Stride) * ((Image)wholeScreen).Height;
		byte[] array = new byte[num];
		Marshal.Copy(scan, array, 0, num);
		int num2 = 0;
		for (int i = 0; i < MaxY; i++)
		{
			for (int j = 0; j < MaxX; j++)
			{
				Color color = Color.FromArgb(array[num2 + 3], array[num2 + 2], array[num2 + 1], array[num2]);
				mScreenColors[j, i] = color.ToArgb();
				num2 += 4;
			}
		}
		wholeScreen.UnlockBits(val);
	}

	public unsafe static Bitmap ChangeColors(Bitmap bmp, Color sourceColor, Color targetColor, int threshold)
	{
		int num = threshold * threshold;
		int num2 = targetColor.ToArgb();
		BitmapData val = bmp.LockBits(new Rectangle(0, 0, ((Image)bmp).Width, ((Image)bmp).Height), (ImageLockMode)3, (PixelFormat)925707);
		int r = sourceColor.R;
		int g = sourceColor.G;
		int b = sourceColor.B;
		int* ptr = (int*)((byte*)(void*)val.Scan0 + (nint)(((Image)bmp).Height * ((Image)bmp).Width) * (nint)4);
		for (int* ptr2 = (int*)(void*)val.Scan0; ptr2 < ptr; ptr2++)
		{
			int num3 = ((*ptr2 >> 16) & 0xFF) - r;
			int num4 = ((*ptr2 >> 8) & 0xFF) - g;
			int num5 = (*ptr2 & 0xFF) - b;
			if (num3 * num3 + num4 * num4 + num5 * num5 <= num)
			{
				*ptr2 = num2;
			}
		}
		bmp.UnlockBits(val);
		return bmp;
	}

	public unsafe static Bitmap ChangeColorsCustom(Bitmap bmp, int minR = 100, int minG = 98)
	{
		int num = Color.FromArgb(255, 0, 0, 0).ToArgb();
		BitmapData val = bmp.LockBits(new Rectangle(0, 0, ((Image)bmp).Width, ((Image)bmp).Height), (ImageLockMode)3, (PixelFormat)925707);
		int* ptr = (int*)((byte*)(void*)val.Scan0 + (nint)(((Image)bmp).Height * ((Image)bmp).Width) * (nint)4);
		for (int* ptr2 = (int*)(void*)val.Scan0; ptr2 < ptr; ptr2++)
		{
			int num2 = (*ptr2 >> 16) & 0xFF;
			int num3 = (*ptr2 >> 8) & 0xFF;
			_ = *ptr2;
			if (num2 < minR || num3 < minG)
			{
				*ptr2 = num;
			}
		}
		bmp.UnlockBits(val);
		return bmp;
	}

	public unsafe static Bitmap ChangeColorsAllButColor(Bitmap bmp, Color sourceColor, Color targetColor, int threshold)
	{
		int num = threshold * threshold;
		int num2 = targetColor.ToArgb();
		BitmapData val = bmp.LockBits(new Rectangle(0, 0, ((Image)bmp).Width, ((Image)bmp).Height), (ImageLockMode)3, (PixelFormat)925707);
		int r = sourceColor.R;
		int g = sourceColor.G;
		int b = sourceColor.B;
		int* ptr = (int*)((byte*)(void*)val.Scan0 + (nint)(((Image)bmp).Height * ((Image)bmp).Width) * (nint)4);
		for (int* ptr2 = (int*)(void*)val.Scan0; ptr2 < ptr; ptr2++)
		{
			int num3 = ((*ptr2 >> 16) & 0xFF) - r;
			int num4 = ((*ptr2 >> 8) & 0xFF) - g;
			int num5 = (*ptr2 & 0xFF) - b;
			if (num3 * num3 + num4 * num4 + num5 * num5 > num)
			{
				*ptr2 = num2;
			}
		}
		bmp.UnlockBits(val);
		return bmp;
	}

	public static Bitmap CaptureScreen(Rectangle r)
	{
		Point p = new Point(r.X, r.Y);
		Point q = new Point(r.X + r.Width, r.Y + r.Height);
		return CaptureScreen(p, q);
	}

	public static Bitmap CaptureScreen()
	{
		return CaptureScreen(new Point(0, 0), new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
	}

	public static Bitmap CaptureScreen(int x1, int y1, int x2, int y2)
	{
		return CaptureScreen(new Point(x1, y1), new Point(x2, y2));
	}

	public static Bitmap CaptureScreen(Point p, Point q)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_00ac: Expected O, but got Unknown
		int num = Math.Abs(q.X - p.X);
		int num2 = Math.Abs(q.Y - p.Y);
		if (num <= 0 || num2 <= 0)
		{
			throw new Exception("screen capture failed");
		}
		Bitmap val = new Bitmap(num, num2);
		Graphics obj = Graphics.FromImage((Image)val);
		IntPtr dC = GetDC(IntPtr.Zero);
		IntPtr hdc = obj.GetHdc();
		BitBlt(hdc, 0, 0, num, num2, dC, Math.Min(p.X, q.X), Math.Min(p.Y, q.Y), 13369376);
		obj.ReleaseHdc(hdc);
		obj.Dispose();
		ReleaseDC(IntPtr.Zero, dC);
		return val;
	}

	public static Bitmap CaptureScreenRect(Rectangle r)
	{
		Point p = new Point(r.X, r.Y);
		Point q = new Point(r.X + r.Width, r.Y + r.Height);
		return CaptureScreen(p, q);
	}

	public static Bitmap CaptureScreenRect(Point topLeft, Point botRight)
	{
		return CaptureScreen(topLeft, botRight);
	}

	public static void CaptureScreenDBM(int x1 = -1, int y1 = -1, int x2 = -1, int y2 = -1, string saveToPath = "")
	{
		if (x1 == -1)
		{
			x1 = 0;
		}
		if (y1 == -1)
		{
			y1 = 0;
		}
		if (x2 == -1)
		{
			x2 = MaxX;
		}
		if (y2 == -1)
		{
			y2 = MaxY;
		}
		dbm = new DirectBitmap();
		dbm.LoadBitmap(CaptureScreen(new Point(x1, y1), new Point(x2, y2)));
		if (saveToPath != "")
		{
			((Image)dbm.Bitmap).Save(saveToPath);
		}
	}

	public static void CaptureScreenDBMOld(int x1 = -1, int y1 = -1, int x2 = -1, int y2 = -1, string saveToPath = "")
	{
		if (x1 == -1)
		{
			x1 = 0;
		}
		if (y1 == -1)
		{
			y1 = 0;
		}
		if (x2 == -1)
		{
			x2 = MaxX;
		}
		if (y2 == -1)
		{
			y2 = MaxY;
		}
		dbm = new DirectBitmap(x2 - x1 + 1, y2 - y1 + 1);
		mGraph = Graphics.FromImage((Image)(object)dbm.Bitmap);
		mGraph.CopyFromScreen(x1, y1, 0, 0, ((Image)dbm.Bitmap).Size);
		if (saveToPath != "")
		{
			((Image)dbm.Bitmap).Save(saveToPath);
		}
	}

	public static DirectBitmap CaptureDBM(int x1 = -1, int y1 = -1, int x2 = -1, int y2 = -1)
	{
		if (x1 == -1)
		{
			x1 = 0;
		}
		if (y1 == -1)
		{
			y1 = 0;
		}
		if (x2 == -1)
		{
			x2 = MaxX;
		}
		if (y2 == -1)
		{
			y2 = MaxY;
		}
		DirectBitmap directBitmap = new DirectBitmap(x2 - x1 + 1, y2 - y1 + 1);
		mGraph = Graphics.FromImage((Image)(object)directBitmap.Bitmap);
		mGraph.CopyFromScreen(x1, y1, 0, 0, ((Image)directBitmap.Bitmap).Size);
		return directBitmap;
	}

	public static Bitmap CaptureRegion(int x1, int y1, int x2, int y2, bool save = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Bitmap val = new Bitmap(x2 - x1, y2 - y1, (PixelFormat)2498570);
		Graphics obj = Graphics.FromImage((Image)(object)val);
		Size size = new Size(x2 - x1, y2 - y1);
		obj.CopyFromScreen(x1, y1, 0, 0, size, (CopyPixelOperation)13369376);
		if (save)
		{
			((Image)val).Save("region.bmp");
		}
		return val;
	}

	public static Bitmap ChangeResolution(Bitmap bmp, int resizeMultiplier = 3, float dpix = 300f, float dpiy = 300f)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		int width = ((Image)bmp).Width;
		int height = ((Image)bmp).Height;
		int num = width * resizeMultiplier;
		int num2 = height * resizeMultiplier;
		Bitmap val = new Bitmap(num, num2);
		Point[] array = new Point[3]
		{
			new Point(0, 0),
			new Point(num, 0),
			new Point(0, num2)
		};
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			val2.DrawImage((Image)(object)bmp, array);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		val.SetResolution(dpix, dpiy);
		return val;
	}

	public static Bitmap CaptureRegion(Point a, Point b, bool save = false)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		int num = Math.Abs(a.X - b.X);
		int num2 = Math.Abs(a.Y - b.Y);
		int num3 = ((a.X < b.X) ? a.X : b.X);
		int num4 = ((a.Y < b.Y) ? a.Y : b.Y);
		Bitmap val = new Bitmap(num, num2, (PixelFormat)2498570);
		Graphics obj = Graphics.FromImage((Image)(object)val);
		Size size = new Size(num, num2);
		obj.CopyFromScreen(num3, num4, 0, 0, size, (CopyPixelOperation)13369376);
		if (save)
		{
			((Image)val).Save("region.bmp");
		}
		return val;
	}

	public static Point getTopLeft(Point a, Point b)
	{
		int x = ((a.X < b.X) ? a.X : b.X);
		int y = ((a.Y < b.Y) ? a.Y : b.Y);
		return new Point(x, y);
	}

	public static DirectBitmap CaptureRegionDBM(Point a, Point b, bool save = false, string savePath = "region.bmp")
	{
		int width = Math.Abs(a.X - b.X);
		int height = Math.Abs(a.Y - b.Y);
		int num = ((a.X < b.X) ? a.X : b.X);
		int num2 = ((a.Y < b.Y) ? a.Y : b.Y);
		DirectBitmap directBitmap = new DirectBitmap(width, height);
		Graphics obj = Graphics.FromImage((Image)(object)directBitmap.Bitmap);
		Size size = new Size(width, height);
		obj.CopyFromScreen(num, num2, 0, 0, size, (CopyPixelOperation)13369376);
		if (save)
		{
			((Image)directBitmap.Bitmap).Save(savePath);
		}
		return directBitmap;
	}

	public static Color GetColorAtDBM(int x, int y, DirectBitmap srcDbm = null)
	{
		if (srcDbm == null)
		{
			srcDbm = dbm;
		}
		return srcDbm.GetPixel(x, y);
	}

	public static Color GetColorAtDBM(Point p, DirectBitmap srcDbm = null)
	{
		if (srcDbm == null)
		{
			srcDbm = dbm;
		}
		return srcDbm.GetPixel(p.X, p.Y);
	}

	public static DirectBitmap GetActiveDMB()
	{
		return dbm;
	}

	public static Rectangle RectangleFromPoints(Point a, Point b)
	{
		int width = Math.Abs(a.X - b.X);
		int height = Math.Abs(a.Y - b.Y);
		int x = ((a.X < b.X) ? a.X : b.X);
		int y = ((a.Y < b.Y) ? a.Y : b.Y);
		return new Rectangle(x, y, width, height);
	}

	public static Bitmap cropAtRect(Bitmap b, Rectangle r)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		Bitmap val = new Bitmap(r.Width, r.Height);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			val2.DrawImage((Image)(object)b, -r.X, -r.Y);
			return val;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static DirectBitmap cropAtRect(DirectBitmap b, Rectangle r)
	{
		DirectBitmap directBitmap = new DirectBitmap(r.Width, r.Height);
		Graphics val = Graphics.FromImage((Image)(object)directBitmap.Bitmap);
		try
		{
			val.DrawImage((Image)(object)b.Bitmap, -r.X, -r.Y);
			return directBitmap;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static Point GetRandomizedPoint(Point basePoint, int xDiff = 20, int yDiff = 20)
	{
		Size size = Screen.PrimaryScreen.Bounds.Size;
		Size size2 = new Size(1920, 1080);
		float num = (float)size.Width / (float)size2.Width;
		float num2 = (float)size.Height / (float)size2.Height;
		Graphics val = Graphics.FromHwnd(IntPtr.Zero);
		try
		{
			float dpiX = val.DpiX;
			float dpiY = val.DpiY;
			num *= dpiX / 96f;
			num2 *= dpiY / 96f;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		int num3 = (int)((float)xDiff * num);
		int num4 = (int)((float)yDiff * num2);
		Random random = new Random();
		int num5 = random.Next(-num3 / 2, num3 / 2 + 1);
		int num6 = random.Next(-num4 / 2, num4 / 2 + 1);
		return new Point(basePoint.X + num5, basePoint.Y + num6);
	}

	public static Point? FindPixelSequenceLeftToRight(string[] seq, int fromX = 0, int toX = -1, int fromY = 0, int toY = -1, DirectBitmap srcDbm = null)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = seq.Length;
		Color[] array = new Color[seq.Length];
		if (srcDbm == null)
		{
			srcDbm = dbm;
		}
		for (num = 0; num < seq.Length; num++)
		{
			array[num] = StrColorToColor(seq[num]);
		}
		if (toY == -1)
		{
			toY = srcDbm.Height;
		}
		if (toX == -1)
		{
			toX = srcDbm.Width;
		}
		if (toX < fromX)
		{
			toX = fromX;
		}
		if (toY < fromY)
		{
			toY = fromY;
		}
		for (num2 = fromY; num2 < toY; num2++)
		{
			for (num = fromX; num < toX; num++)
			{
				Color pixel = srcDbm.GetPixel(num, num2);
				if (CompareColors(array[num3], pixel, ColorPrecision))
				{
					if (num3 + 1 == num4)
					{
						return new Point(num, num2);
					}
					num3++;
				}
				else
				{
					num3 = 0;
					if (CompareColors(array[num3], pixel, ColorPrecision))
					{
						num3 = 1;
					}
				}
			}
		}
		pointNotFound = true;
		return null;
	}

	public static Point? FindPixelSequenceTopToBottom(string[] seq, int fromX = 0, int toX = -1, int fromY = 0, int toY = -1, DirectBitmap srcDbm = null)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = seq.Length;
		Color[] array = new Color[seq.Length];
		if (srcDbm == null)
		{
			srcDbm = dbm;
		}
		for (num = 0; num < seq.Length; num++)
		{
			array[num] = StrColorToColor(seq[num]);
		}
		if (toY == -1)
		{
			toY = srcDbm.Height;
		}
		if (toX == -1)
		{
			toX = srcDbm.Width;
		}
		if (toX < fromX)
		{
			toX = fromX;
		}
		if (toY < fromY)
		{
			toY = fromY;
		}
		for (num = fromX; num < toX; num++)
		{
			for (num2 = fromY; num2 < toY; num2++)
			{
				Color pixel = srcDbm.GetPixel(num, num2);
				if (CompareColors(array[num3], pixel, ColorPrecision))
				{
					if (num3 + 1 == num4)
					{
						return new Point(num, num2);
					}
					num3++;
				}
				else
				{
					num3 = 0;
					if (CompareColors(array[num3], pixel, ColorPrecision))
					{
						num3 = 1;
					}
				}
			}
		}
		pointNotFound = true;
		return null;
	}

	public static bool IsPoint0(Point point)
	{
		if (point.X == 0)
		{
			return point.Y == 0;
		}
		return false;
	}

	public static bool CompareColors(string color1, string color2, double precision = 0.0)
	{
		if (!color1.StartsWith("#"))
		{
			color1 = "#" + color1;
		}
		if (!color2.StartsWith("#"))
		{
			color2 = "#" + color2;
		}
		if (precision == 0.0)
		{
			return color1.Equals(color2, StringComparison.OrdinalIgnoreCase);
		}
		return CompareColors(StrColorToColor(color1), StrColorToColor(color2), precision);
	}

	public static bool CompareColors(Color color1, Color color2, double precision = 0.0)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		if (precision == 0.0)
		{
			if (color1 == color2)
			{
				return true;
			}
			return false;
		}
		double num = 15.0;
		double num2 = Math.Abs(color1.R - color2.R);
		double num3 = Math.Abs(color1.G - color2.G);
		double num4 = Math.Abs(color1.B - color2.B);
		if (num2 >= num || num3 >= num || num4 >= num)
		{
			return false;
		}
		Rgb val = new Rgb
		{
			R = (int)color1.R,
			G = (int)color1.G,
			B = (int)color1.B
		};
		Rgb val2 = new Rgb
		{
			R = (int)color2.R,
			G = (int)color2.G,
			B = (int)color2.B
		};
		double num5 = ((ColorSpace)val).Compare((IColorSpace)(object)val2, (IColorSpaceComparison)new Cie1976Comparison());
		if (FoeTools.DebugMode)
		{
			FoeTools.colorDiff = num5;
		}
		if (num5 < precision)
		{
			return true;
		}
		return false;
	}

	public static bool CompareColors(Color color1, Color[] colors2, double precision = 0.0)
	{
		return colors2.Any((Color c) => CompareColors(color1, c, precision));
	}

	public static bool CompareColors(Color color1, List<Color> colors2, double precision = 0.0)
	{
		return colors2.Any((Color c) => CompareColors(color1, c, precision));
	}

	public static bool CompareColors(List<Color> colorsLookFor, List<Color> colorsBeingSearched, double precision = 0.0)
	{
		return colorsLookFor.Any((Color c) => CompareColors(c, colorsBeingSearched, precision));
	}

	public static Point FindNearestColor(Point fromP, Point toP, List<Color> colorsSearch, double precision = 0.0)
	{
		for (int i = fromP.X; i < toP.X; i++)
		{
			for (int j = fromP.Y; j < toP.Y; j++)
			{
				foreach (Color item in colorsSearch)
				{
					if (CompareColors(GetColorAt(i, j), item, precision))
					{
						return new Point(i, j);
					}
				}
			}
		}
		return new Point(0, 0);
	}

	public static Point MovePoint(Point p, Point q)
	{
		return new Point(p.X + q.X, p.Y + q.Y);
	}

	public static double CompareColorsGetDelta(Color color1, Color color2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		Rgb val = new Rgb
		{
			R = (int)color1.R,
			G = (int)color1.G,
			B = (int)color1.B
		};
		Rgb val2 = new Rgb
		{
			R = (int)color2.R,
			G = (int)color2.G,
			B = (int)color2.B
		};
		return ((ColorSpace)val).Compare((IColorSpace)(object)val2, (IColorSpaceComparison)new Cie1976Comparison());
	}

	public static double CompareColorsGetDelta(string color1, string color2)
	{
		return CompareColorsGetDelta(StrColorToColor(color1), StrColorToColor(color2));
	}

	private static string GetPixelAtCoordsSlow(int x, int y)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		Bitmap val = new Bitmap(1, 1);
		Graphics.FromImage((Image)(object)val).CopyFromScreen(x, y, x, y, ((Image)val).Size);
		return ColorTranslator.ToHtml(val.GetPixel(x, y));
	}

	public static Bitmap ConvertToFormat(Image image, PixelFormat format)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		Bitmap val = new Bitmap(image.Width, image.Height, format);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			val2.DrawImage(image, new Rectangle(0, 0, ((Image)val).Width, ((Image)val).Height));
			return val;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static List<Color> PixelColorsToColors(List<PixelColor> pixelColors)
	{
		List<Color> list = new List<Color>();
		foreach (PixelColor pixelColor in pixelColors)
		{
			list.Add(pixelColor.C);
		}
		return list;
	}

	public static Point GetRandomPointInRectangle(Point topLeft, Point bottomRight)
	{
		Random random = new Random();
		int x = topLeft.X;
		int x2 = bottomRight.X;
		int y = topLeft.Y;
		int y2 = bottomRight.Y;
		int x3 = random.Next(x, x2 + 1);
		int y3 = random.Next(y, y2 + 1);
		return new Point(x3, y3);
	}

	public static Bitmap IncreaseContrast(Bitmap image, float contrast)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Image)image).Width, ((Image)image).Height);
		float num = (100f + contrast) / 100f;
		num *= num;
		for (int i = 0; i < ((Image)image).Height; i++)
		{
			for (int j = 0; j < ((Image)image).Width; j++)
			{
				Color pixel = image.GetPixel(j, i);
				float num2 = (float)(int)pixel.R / 255f;
				float num3 = (float)(int)pixel.G / 255f;
				float num4 = (float)(int)pixel.B / 255f;
				float num5 = ((num2 - 0.5f) * num + 0.5f) * 255f;
				num3 = ((num3 - 0.5f) * num + 0.5f) * 255f;
				num4 = ((num4 - 0.5f) * num + 0.5f) * 255f;
				int red = Math.Min(Math.Max((int)num5, 0), 255);
				int green = Math.Min(Math.Max((int)num3, 0), 255);
				int blue = Math.Min(Math.Max((int)num4, 0), 255);
				val.SetPixel(j, i, Color.FromArgb(red, green, blue));
			}
		}
		return val;
	}

	public static bool pointIsNull(Point p)
	{
		if (p.X == 0 && p.Y == 0)
		{
			return true;
		}
		return false;
	}

	public static Bitmap Sharpen(Bitmap image)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Image)image).Width, ((Image)image).Height);
		float[,] array = new float[3, 3]
		{
			{ -1f, -1f, -1f },
			{ -1f, 9f, -1f },
			{ -1f, -1f, -1f }
		};
		int width = ((Image)image).Width;
		int height = ((Image)image).Height;
		for (int i = 1; i < height - 1; i++)
		{
			for (int j = 1; j < width - 1; j++)
			{
				float num = 0f;
				float num2 = 0f;
				float num3 = 0f;
				for (int k = -1; k <= 1; k++)
				{
					for (int l = -1; l <= 1; l++)
					{
						Color pixel = image.GetPixel(j + l, i + k);
						float num4 = array[k + 1, l + 1];
						num += (float)(int)pixel.R * num4;
						num2 += (float)(int)pixel.G * num4;
						num3 += (float)(int)pixel.B * num4;
					}
				}
				int red = Math.Min(Math.Max((int)num, 0), 255);
				int green = Math.Min(Math.Max((int)num2, 0), 255);
				int blue = Math.Min(Math.Max((int)num3, 0), 255);
				val.SetPixel(j, i, Color.FromArgb(red, green, blue));
			}
		}
		return val;
	}
}
