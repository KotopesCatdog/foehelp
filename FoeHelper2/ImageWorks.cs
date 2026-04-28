using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FoeHelper2;

internal class ImageWorks
{
	public static float zoomLevel = 1f;

	public static Point LastFound;

	public static Bitmap ResizeBicubic(Bitmap sourceBitmap, int xSize, int ySize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		Mat val = new Mat();
		CvInvoke.Resize((IInputArray)(object)new Image<Bgr, byte>(sourceBitmap), (IOutputArray)(object)val, new Size(xSize, ySize), 0.0, 0.0, (Inter)2);
		return val.ToImage<Bgr, byte>(false).Bitmap;
	}

	public static void savePngDebug(Bitmap bmp, string name = "dbg")
	{
		if (FoeTools.DebugMode)
		{
			((Image)bmp).Save("c:\\mik\\" + name + ".png");
		}
	}

	public static Point? FindTemplateEmgu(Bitmap sourceBitmap, Bitmap templateBitmap, double precisionThreshold = 0.9200000166893005, bool includeTemplateSize = false)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		if (((Image)sourceBitmap).Width < ((Image)templateBitmap).Width && ((Image)sourceBitmap).Height < ((Image)templateBitmap).Height)
		{
			Bitmap obj = sourceBitmap;
			sourceBitmap = templateBitmap;
			templateBitmap = obj;
		}
		Image<Bgr, byte> val = new Image<Bgr, byte>(sourceBitmap);
		try
		{
			Image<Bgr, byte> val2 = new Image<Bgr, byte>(templateBitmap);
			try
			{
				Mat val3 = new Mat();
				try
				{
					CvInvoke.MatchTemplate((IInputArray)(object)val2, (IInputArray)(object)val, (IOutputArray)(object)val3, (TemplateMatchingType)5, (IInputArray)null);
					double num = 0.0;
					Point point = default(Point);
					double num2 = 0.0;
					Point point2 = default(Point);
					CvInvoke.MinMaxLoc((IInputArray)(object)val3, ref num2, ref num, ref point2, ref point, (IInputArray)null);
					FoeTools.debugDouble = FoeTools.debugDouble2;
					FoeTools.debugDouble2 = num;
					if (num >= precisionThreshold)
					{
						if (includeTemplateSize)
						{
							point.X += ((Image)templateBitmap).Width;
							point.Y += ((Image)templateBitmap).Height;
						}
						LastFound = point;
						return point;
					}
					return null;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
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
	}

	public static List<Point> FindAllTemplateMatchesEmgu(Bitmap sourceBitmap, Bitmap templateBitmap, double precisionThreshold = 0.92, bool includeTemplateSize = false)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		List<Point> list = new List<Point>();
		if (((Image)sourceBitmap).Width < ((Image)templateBitmap).Width && ((Image)sourceBitmap).Height < ((Image)templateBitmap).Height)
		{
			Bitmap obj = sourceBitmap;
			sourceBitmap = templateBitmap;
			templateBitmap = obj;
		}
		Image<Bgr, byte> val = new Image<Bgr, byte>(sourceBitmap);
		try
		{
			Image<Bgr, byte> val2 = new Image<Bgr, byte>(templateBitmap);
			try
			{
				Mat val3 = new Mat();
				try
				{
					CvInvoke.MatchTemplate((IInputArray)(object)val, (IInputArray)(object)val2, (IOutputArray)(object)val3, (TemplateMatchingType)5, (IInputArray)null);
					Image<Gray, float> val4 = val3.ToImage<Gray, float>(false);
					try
					{
						for (int i = 0; i < ((CvArray<float>)(object)val4).Height; i++)
						{
							for (int j = 0; j < ((CvArray<float>)(object)val4).Width; j++)
							{
								if ((double)val4.Data[i, j, 0] >= precisionThreshold)
								{
									Point item = new Point(j, i);
									if (includeTemplateSize)
									{
										item.X += ((Image)templateBitmap).Width;
										item.Y += ((Image)templateBitmap).Height;
									}
									list.Add(item);
								}
							}
						}
						return list;
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
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
	}

	public static bool FindTemplateEmguExist(Bitmap sourceBitmap, Bitmap templateBitmap, double precisionThreshold = 0.9200000166893005, bool includeTemplateSize = false)
	{
		if (FindTemplateEmgu(sourceBitmap, templateBitmap, precisionThreshold, includeTemplateSize).HasValue)
		{
			return true;
		}
		return false;
	}

	public static bool WaitFindTemplateEmguExist(Bitmap sourceBitmap, Point p, Point q, double precisionThreshold = 0.9200000166893005, bool includeTemplateSize = false, int timeOutSeconds = 4)
	{
		bool flag = false;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (!flag)
		{
			Bitmap templateBitmap = PixelTools.CaptureScreen(p, q);
			if (FindTemplateEmgu(sourceBitmap, templateBitmap, precisionThreshold, includeTemplateSize).HasValue)
			{
				return true;
			}
			if (stopwatch.Elapsed.TotalSeconds >= (double)timeOutSeconds)
			{
				break;
			}
			Thread.Sleep(100);
		}
		return false;
	}

	public static bool WaitFindTemplateEmguExist(Bitmap sourceBitmap, Rectangle r, double precisionThreshold = 0.9200000166893005, bool includeTemplateSize = false, int timeOutSeconds = 4)
	{
		Point p = new Point(r.X, r.Y);
		Point q = new Point(r.X + r.Width, r.Y + r.Height);
		return WaitFindTemplateEmguExist(sourceBitmap, p, q, precisionThreshold, includeTemplateSize, timeOutSeconds);
	}

	public static Point? FindTemplateEmguMulti(Bitmap sourceBitmap, Bitmap templateBitmap, double precisionThreshold)
	{
		Image<Bgr, byte> obj = new Image<Bgr, byte>(sourceBitmap);
		Image<Bgr, byte> val = new Image<Bgr, byte>(templateBitmap);
		Image<Gray, float> obj2 = obj.MatchTemplate(val, (TemplateMatchingType)5);
		Point value = new Point(0, 0);
		double num = 0.0;
		float[,,] data = obj2.Data;
		for (int i = 0; i < data.GetLength(0); i++)
		{
			for (int j = 0; j < data.GetLength(1); j++)
			{
				double num2 = data[i, j, 0];
				if (num2 > 0.82 && num < num2)
				{
					value = new Point(j, i);
					num = num2;
				}
			}
		}
		if (num > 0.0)
		{
			return value;
		}
		return null;
	}

	public static Bitmap ConvertToGrayscale(Bitmap sourceBitmap)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		Image<Bgr, byte> obj = new Image<Bgr, byte>(sourceBitmap);
		Mat val = new Mat();
		CvInvoke.CvtColor((IInputArray)(object)obj, (IOutputArray)(object)val, (ColorConversion)6, 0);
		Mat val2 = new Mat();
		CvInvoke.Threshold((IInputArray)(object)val, (IOutputArray)(object)val2, 0.0, 255.0, (ThresholdType)8);
		CvInvoke.BitwiseNot((IInputArray)(object)val2, (IOutputArray)(object)val2, (IInputArray)null);
		return val2.Bitmap;
	}

	public static Bitmap InvertColors(Bitmap originalBitmap)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Image<Bgr, byte> val = new Image<Bgr, byte>(originalBitmap);
		for (int i = 0; i < ((CvArray<byte>)(object)val).Height; i++)
		{
			for (int j = 0; j < ((CvArray<byte>)(object)val).Width; j++)
			{
				Bgr val2 = val[i, j];
				((Bgr)(ref val2)).Blue = (int)(byte)(255.0 - ((Bgr)(ref val2)).Blue);
				((Bgr)(ref val2)).Green = (int)(byte)(255.0 - ((Bgr)(ref val2)).Green);
				((Bgr)(ref val2)).Red = (int)(byte)(255.0 - ((Bgr)(ref val2)).Red);
				val[i, j] = val2;
			}
		}
		return val.ToBitmap();
	}

	public static (float scalingFactorX, float scalingFactorY) GetScreenZoomSettings()
	{
		Graphics val = Graphics.FromHwnd(IntPtr.Zero);
		try
		{
			float dpiX = val.DpiX;
			float dpiY = val.DpiY;
			float item = dpiX / 96f;
			float item2 = dpiY / 96f;
			return (scalingFactorX: item, scalingFactorY: item2);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static Bitmap SharpenBitmapWithUnsharpMask(Bitmap bitmap, double strength)
	{
		Image<Bgr, byte> val = new Image<Bgr, byte>(bitmap);
		Image<Bgr, byte> val2 = val.SmoothGaussian(9);
		try
		{
			return (val - val2 * strength).ToBitmap();
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static Bitmap getBmp(string path, float resizeX = 1f, float resizeY = 1f, bool sharpen = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		Bitmap val = null;
		try
		{
			val = (Bitmap)Image.FromFile(path);
		}
		catch (Exception)
		{
			FoeTools.outInfo("error: template image " + path + " not found, check installation folder");
			return null;
		}
		if (resizeX != 1f || resizeY != 1f)
		{
			val = ResizeBicubic(val, (int)((float)((Image)val).Width * resizeX), (int)((float)((Image)val).Height * resizeY));
		}
		if (sharpen)
		{
			return SharpenBitmapWithUnsharpMask(val, 0.01);
		}
		return val;
	}

	public static Bitmap getBmp(string path, float resizeX, float resizeY, string alternativeImage)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		Bitmap val = null;
		if ((double)resizeX >= 1.5 && (double)resizeX <= 1.6)
		{
			try
			{
				return getBmp(alternativeImage);
			}
			catch
			{
				FoeTools.outInfo("error: template resizing issue, incorrect zoom");
				return null;
			}
		}
		try
		{
			val = (Bitmap)Image.FromFile(path);
		}
		catch (Exception)
		{
			FoeTools.outInfo("error: template image not found, check installation folder");
			return null;
		}
		if (resizeX != 1f || resizeY != 1f)
		{
			val = ResizeBicubic(val, (int)((float)((Image)val).Width * resizeX), (int)((float)((Image)val).Height * resizeY));
		}
		return val;
	}

	public static Bitmap getBmp(Bitmap bmp, float resizeX = 1f, float resizeY = 1f)
	{
		float num = Math.Abs(resizeX - 1f);
		float num2 = Math.Abs(resizeY - 1f);
		if (num > 0.002f || num2 > 0.002f)
		{
			bmp = ResizeBicubic(bmp, (int)((float)((Image)bmp).Width * resizeX), (int)((float)((Image)bmp).Height * resizeY));
		}
		return bmp;
	}

	public static Bitmap rescale(Size size, Bitmap origin)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		Bitmap val = new Bitmap(size.Width, size.Height, ((Image)origin).PixelFormat);
		Graphics val2 = Graphics.FromImage((Image)(object)val);
		try
		{
			val2.DrawImage((Image)(object)origin, 0, 0, size.Width, size.Height);
			return val;
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public static Bitmap ChangeOtherColorsToWhite(List<Color> colorsToMatch, Bitmap originalBitmap, float prec)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		bool flag = false;
		if (originalBitmap == null)
		{
			return originalBitmap;
		}
		Bitmap val = new Bitmap((Image)(object)originalBitmap);
		for (int i = 0; i < ((Image)val).Height; i++)
		{
			for (int j = 0; j < ((Image)val).Width; j++)
			{
				flag = false;
				Color pixel = val.GetPixel(j, i);
				if (colorsToMatch.Contains(pixel))
				{
					flag = true;
				}
				foreach (Color item in colorsToMatch)
				{
					if (PixelTools.CompareColors(pixel, item, prec))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					val.SetPixel(j, i, Color.Black);
				}
			}
		}
		return val;
	}
}
