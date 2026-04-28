using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Tesseract;

namespace FoeHelper2;

internal class FoeOCR
{
	public static string engineName = "";

	public static TesseractEngine engine;

	public static TesseractEngine textEngine;

	public static Bitmap PrepareImageNew(Bitmap bmp, int modif = 1, bool modif2 = false, string fileDebug = "", string allowedChars = "1/234567890", PageSegMode pageSq = 6, float enlarge = 1.5f)
	{
		if (enlarge != 1f)
		{
			bmp = ResizeNearest(bmp, (int)((float)((Image)bmp).Width * enlarge), (int)((float)((Image)bmp).Height * enlarge));
		}
		bmp = PixelTools.ConvertToFormat((Image)(object)bmp, (PixelFormat)137224);
		if (modif > 0)
		{
			bmp = PixelTools.ChangeColorsCustom(bmp, 200);
		}
		if (modif > 1)
		{
			bmp = PixelTools.IncreaseContrast(bmp, 1.2f);
		}
		if (modif2)
		{
			bmp = ImageWorks.ChangeOtherColorsToWhite(new List<Color>
			{
				ColorTranslator.FromHtml("#F3D6A0"),
				ColorTranslator.FromHtml("#E7CB97"),
				ColorTranslator.FromHtml("#C2A87C"),
				ColorTranslator.FromHtml("#866E4F"),
				ColorTranslator.FromHtml("#A9906A"),
				ColorTranslator.FromHtml("#371B08"),
				ColorTranslator.FromHtml("#624C34"),
				ColorTranslator.FromHtml("#4D3723"),
				ColorTranslator.FromHtml("#CEB385"),
				ColorTranslator.FromHtml("#DABF8E"),
				ColorTranslator.FromHtml("#3A2412"),
				ColorTranslator.FromHtml("#F9F2E2"),
				ColorTranslator.FromHtml("#75685A"),
				ColorTranslator.FromHtml("#C4BBAB"),
				ColorTranslator.FromHtml("#CEC5B6"),
				ColorTranslator.FromHtml("#B8AEA0"),
				ColorTranslator.FromHtml("#D9D2C2"),
				ColorTranslator.FromHtml("#988C7F"),
				ColorTranslator.FromHtml("#E2DCCC"),
				ColorTranslator.FromHtml("#EFE7D7")
			}, bmp, 0.95f);
		}
		bmp = ImageWorks.ConvertToGrayscale(bmp);
		bmp = BinarizeFixed(bmp, 140);
		bmp = Erode1px(bmp);
		if (!string.IsNullOrEmpty(fileDebug))
		{
			((Image)bmp).Save(fileDebug);
		}
		return bmp;
	}

	public static Bitmap PrepareImage(Bitmap bmp, int modif = 1, bool modif2 = false, string fileDebug = "", string allowedChars = "1/234567890", PageSegMode pageSq = 6, float enlarge = 1.5f)
	{
		if (enlarge != 1f)
		{
			bmp = ImageWorks.ResizeBicubic(bmp, (int)((float)((Image)bmp).Width * enlarge), (int)((float)((Image)bmp).Height * enlarge));
		}
		if (modif > 0)
		{
			bmp = PixelTools.ChangeColorsCustom(bmp, 200);
			bmp = PixelTools.ConvertToFormat((Image)(object)bmp, (PixelFormat)137224);
		}
		if (modif > 1)
		{
			bmp = PixelTools.IncreaseContrast(bmp, 1.2f);
		}
		if (modif2)
		{
			bmp = ImageWorks.ChangeOtherColorsToWhite(new List<Color>
			{
				ColorTranslator.FromHtml("#F3D6A0"),
				ColorTranslator.FromHtml("#E7CB97"),
				ColorTranslator.FromHtml("#C2A87C"),
				ColorTranslator.FromHtml("#866E4F"),
				ColorTranslator.FromHtml("#A9906A"),
				ColorTranslator.FromHtml("#371B08"),
				ColorTranslator.FromHtml("#E7CB97"),
				ColorTranslator.FromHtml("#624C34"),
				ColorTranslator.FromHtml("#4D3723"),
				ColorTranslator.FromHtml("#CEB385"),
				ColorTranslator.FromHtml("#371B08"),
				ColorTranslator.FromHtml("#DABF8E"),
				ColorTranslator.FromHtml("#3A2412"),
				ColorTranslator.FromHtml("#F9F2E2"),
				ColorTranslator.FromHtml("#75685A"),
				ColorTranslator.FromHtml("#C4BBAB"),
				ColorTranslator.FromHtml("#CEC5B6"),
				ColorTranslator.FromHtml("#B8AEA0"),
				ColorTranslator.FromHtml("#75685A"),
				ColorTranslator.FromHtml("#D9D2C2"),
				ColorTranslator.FromHtml("#988C7F"),
				ColorTranslator.FromHtml("#E2DCCC"),
				ColorTranslator.FromHtml("#EFE7D7")
			}, bmp, 0.95f);
			_ = FoeTools.DebugMode;
			bmp = PixelTools.ConvertToFormat((Image)(object)bmp, (PixelFormat)137224);
			bmp = ImageWorks.ResizeBicubic(bmp, (int)((float)((Image)bmp).Width * 2f), (int)((float)((Image)bmp).Height * 2f));
			bmp = ImageWorks.ConvertToGrayscale(bmp);
			bmp = ImageWorks.InvertColors(bmp);
		}
		bmp = BinarizeFixed(bmp, 115);
		return bmp;
	}

	public static Bitmap BinarizeFixed(Bitmap src, byte threshold)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Image)src).Width, ((Image)src).Height, (PixelFormat)137224);
		for (int i = 0; i < ((Image)src).Height; i++)
		{
			for (int j = 0; j < ((Image)src).Width; j++)
			{
				byte r = src.GetPixel(j, i).R;
				val.SetPixel(j, i, (r < threshold) ? Color.Black : Color.White);
			}
		}
		return val;
	}

	public static Bitmap Blur3x3(Bitmap src)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Image)src).Width, ((Image)src).Height, ((Image)src).PixelFormat);
		for (int i = 1; i < ((Image)src).Height - 1; i++)
		{
			for (int j = 1; j < ((Image)src).Width - 1; j++)
			{
				int num = 0;
				for (int k = -1; k <= 1; k++)
				{
					for (int l = -1; l <= 1; l++)
					{
						num += src.GetPixel(j + l, i + k).R;
					}
				}
				int num2 = num / 9;
				val.SetPixel(j, i, Color.FromArgb(num2, num2, num2));
			}
		}
		return val;
	}

	public static string GetHighestFoeTraineddataBasename()
	{
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
		if (!Directory.Exists(path))
		{
			return null;
		}
		Regex regex = new Regex("^foe(\\d+)\\.traineddata$", RegexOptions.IgnoreCase);
		return (from filename in Directory.GetFiles(path, "foe*.traineddata").Select(Path.GetFileName)
			select regex.Match(filename) into m
			where m.Success
			select new
			{
				Number = int.Parse(m.Groups[1].Value),
				Basename = "foe" + m.Groups[1].Value
			} into x
			orderby x.Number descending
			select x).FirstOrDefault()?.Basename;
	}

	public static string ReadNumbersCharles(Bitmap bmp, int modif = 1, bool modif2 = false, string fileDebug = "", string allowedChars = "1/234567890", PageSegMode pageSq = 6, float enlarge = 1.5f)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		string text = "";
		bmp = PrepareImage(bmp, modif, modif2, fileDebug, allowedChars, pageSq, enlarge);
		if (fileDebug != "")
		{
			((Image)bmp).Save(fileDebug);
		}
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
		directoryName = Path.Combine(directoryName, "tessdata");
		directoryName = directoryName.Replace("file:\\", "");
		if (engine == null)
		{
			engineName = GetHighestFoeTraineddataBasename();
			if (string.IsNullOrEmpty(engineName))
			{
				FoeTools.outInfo("error, ocr engine not found");
				return "";
			}
			engine = new TesseractEngine(directoryName, engineName, (EngineMode)0);
			engine.SetVariable("tessedit_char_whitelist", allowedChars);
			engine.SetVariable("tessedit_unrej_any_wd", true);
			engine.SetVariable("classify_bln_numeric_mode", "1");
			engine.SetVariable("load_system_dawg", "0");
			engine.SetVariable("load_freq_dawg", "0");
		}
		engine.SetVariable("tessedit_char_whitelist", allowedChars);
		Page val = engine.Process(bmp, (PageSegMode?)pageSq);
		try
		{
			return val.GetText();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static string ReadTextCharles(Bitmap bmp, string lang = "eng", bool invert = true)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		string text = "";
		bmp = PrepareImageNew(bmp, 0, modif2: false, "", "1/234567890", (PageSegMode)6, 2f);
		if (invert)
		{
			bmp = ImageWorks.InvertColors(bmp);
		}
		((Image)bmp).Save("dbg_text_prep.png");
		string text2 = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace("file:\\", ""), "tessdata");
		if (textEngine == null)
		{
			try
			{
				textEngine = new TesseractEngine(text2, lang, (EngineMode)3);
				textEngine.SetVariable("tessedit_unrej_any_wd", true);
			}
			catch (Exception)
			{
				FoeTools.outInfo("Failed to load '" + lang + ".traineddata'. Make sure it is in your tessdata folder!");
				return "";
			}
		}
		textEngine.SetVariable("tessedit_char_whitelist", "");
		Page val = textEngine.Process(bmp, (PageSegMode?)(PageSegMode)7);
		try
		{
			return val.GetText();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static string outDebug(Point[] pointList)
	{
		string text = "";
		for (int i = 0; i < pointList.Length; i++)
		{
			_ = ref pointList[i];
			text += " {p.X}, {p.Y} \n";
		}
		return text;
	}

	public static Bitmap ResizeNearest(Bitmap src, int newW, int newH)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		Bitmap val = new Bitmap(newW, newH, ((Image)src).PixelFormat);
		for (int i = 0; i < newH; i++)
		{
			int num = i * ((Image)src).Height / newH;
			for (int j = 0; j < newW; j++)
			{
				int num2 = j * ((Image)src).Width / newW;
				val.SetPixel(j, i, src.GetPixel(num2, num));
			}
		}
		return val;
	}

	public static Bitmap Erode1px(Bitmap src)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Image)src).Width, ((Image)src).Height);
		for (int i = 1; i < ((Image)src).Height - 1; i++)
		{
			for (int j = 1; j < ((Image)src).Width - 1; j++)
			{
				if (src.GetPixel(j, i).R < 128)
				{
					bool flag = src.GetPixel(j - 1, i).R > 128 || src.GetPixel(j + 1, i).R > 128 || src.GetPixel(j, i - 1).R > 128 || src.GetPixel(j, i + 1).R > 128;
					val.SetPixel(j, i, flag ? Color.White : Color.Black);
				}
				else
				{
					val.SetPixel(j, i, Color.White);
				}
			}
		}
		return val;
	}

	private Bitmap GetBinaryzationImage1(Bitmap image)
	{
		object obj = ((Image)image).Clone();
		Bitmap val = (Bitmap)((obj is Bitmap) ? obj : null);
		List<int> list = new List<int>();
		for (int i = 0; i < ((Image)val).Width; i++)
		{
			for (int j = 0; j < ((Image)val).Height; j++)
			{
				list.Add(val.GetPixel(i, j).R);
			}
		}
		double num = list.Average();
		Color color = default(Color);
		for (int k = 0; k < ((Image)val).Width; k++)
		{
			for (int l = 0; l < ((Image)val).Height; l++)
			{
				color = val.GetPixel(k, l);
				if ((double)((color.R + color.G + color.B) / 3) > num)
				{
					val.SetPixel(k, l, Color.White);
				}
				else
				{
					val.SetPixel(k, l, Color.Black);
				}
			}
		}
		return val;
	}

	public static Bitmap GrayReverse(Bitmap bmp)
	{
		for (int i = 0; i < ((Image)bmp).Width; i++)
		{
			for (int j = 0; j < ((Image)bmp).Height; j++)
			{
				Color pixel = bmp.GetPixel(i, j);
				Color color = Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
				bmp.SetPixel(i, j, color);
			}
		}
		return bmp;
	}

	public static Bitmap ConvertTo1Bpp1(Bitmap bmp)
	{
		int num = 0;
		for (int i = 0; i < ((Image)bmp).Width; i++)
		{
			for (int j = 0; j < ((Image)bmp).Height; j++)
			{
				num += bmp.GetPixel(i, j).B;
			}
		}
		num /= ((Image)bmp).Width * ((Image)bmp).Height;
		for (int k = 0; k < ((Image)bmp).Width; k++)
		{
			for (int l = 0; l < ((Image)bmp).Height; l++)
			{
				Color color = ((255 - bmp.GetPixel(k, l).B > num) ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 255, 255));
				bmp.SetPixel(k, l, color);
			}
		}
		return bmp;
	}

	public static Bitmap ConvertTo1Bpp2(Bitmap img)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		int width = ((Image)img).Width;
		int height = ((Image)img).Height;
		Bitmap val = new Bitmap(width, height, (PixelFormat)196865);
		BitmapData val2 = val.LockBits(new Rectangle(0, 0, width, height), (ImageLockMode)3, (PixelFormat)196865);
		for (int i = 0; i < height; i++)
		{
			byte[] array = new byte[(width + 7) / 8];
			for (int j = 0; j < width; j++)
			{
				if ((double)img.GetPixel(j, i).GetBrightness() >= 0.5)
				{
					array[j / 8] |= (byte)(128 >> j % 8);
				}
			}
			Marshal.Copy(array, 0, (IntPtr)((int)val2.Scan0 + val2.Stride * i), array.Length);
		}
		return val;
	}
}
