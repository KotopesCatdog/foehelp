using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using HDLibrary.Wpf.Input;

namespace FoeHelper2;

internal class FoeTools
{
	public enum ShoppingProp
	{
		Offer,
		Demand
	}

	private FoeBattle fb;

	public static Point shopGoodSelectHeight = new Point(0, 27);

	private static int shortWait = 300;

	private static int midWait = 400;

	private static int longWait = 500;

	private List<Point> points;

	public static Point p1;

	public static Point p2;

	public string foeOut;

	public static int status;

	public static MainWindow window;

	public static bool fightUntilOCR;

	public static bool stopLoss;

	public static int MaxEras = 22;

	public static bool DoubleCheckUnits = false;

	public static bool DoHelp;

	public static float demandRatio = 2f;

	public static int SafetyBreak = 75;

	public static int ScaredFactor = 0;

	public static bool LossesDebug = false;

	public static bool UnitsDebug = false;

	public static bool BattleDebug = false;

	public static bool OCRDebug = false;

	public static bool DoPub;

	public static bool colorWaiterDisable = false;

	public static double colorDiff;

	public static string FightType;

	public static int offersMultiplier = 1;

	private int supplyVol;

	private int demandVol;

	public static Coordinates coord;

	private int shopOffer;

	private int shopDemand;

	public static Bitmap poharBmpSave;

	public static Bitmap poharBmpSave2;

	public static bool DebugMode = false;

	public static int mainState;

	public static bool shopInitDone = false;

	public static bool fightInitDone = false;

	private bool offerSet;

	private bool demandSet;

	private ShopFinder shopFinderOffer;

	private ShopFinder shopFinderDemand;

	public static Random random = new Random();

	public static double debugDouble;

	public static double debugDouble2;

	public static HotKeyHost hotKeyHost;

	private static List<HotKey> hotKeys;

	public static CancellationTokenSource ts;

	public static bool RunDebug = false;

	public static bool ZoomBattlesIndentified = false;

	public static bool ZoomMainIdentified = false;

	public static string Signal { get; set; }

	public static string SignalText { get; set; }

	public static int SelectedEra { get; set; }

	public static int OfferEra { get; set; }

	public static int DemandEra { get; set; }

	public static int shoppingMode { get; set; }

	public FoeTools(MainWindow w)
	{
		window = w;
		Signal = "";
		_ = new string[3] { "2B1708", "B78642", "8D4B1D" };
		_ = new string[4] { "904C1D", "935321", "935321", "261608" };
		_ = new string[5] { "160D06", "757E90", "4B566E", "4B566E", "7E869A" };
		_ = new string[5] { "757C8D", "414B60", "4C576E", "4C576E", "160E07" };
		ClearLogFile();
	}

	public static void AddHotkeys()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		hotKeys = new List<HotKey>();
		Settings.Instance.HotkeyText = "ctr + f3 to break current job &#xA;ctrl+f2 to quit";
		hotKeyHost = new HotKeyHost((HwndSource)PresentationSource.FromVisual((Visual)(object)Application.Current.MainWindow));
		try
		{
			hotKeys.Add(new MainWindow.CustomHotKey("ShowPopup", (Key)60, (ModifierKeys)6, enabled: true));
			hotKeys.Add(new MainWindow.CustomHotKey("Debug", (Key)90, (ModifierKeys)2, enabled: true));
			hotKeys.Add(new MainWindow.CustomHotKey("Terminate", (Key)91, (ModifierKeys)2, enabled: true));
			hotKeys.Add(new MainWindow.CustomHotKey("UnBreak", (Key)93, (ModifierKeys)2, enabled: true));
			hotKeys.Add(new MainWindow.CustomHotKey("Break", (Key)92, (ModifierKeys)2, enabled: true));
			foreach (HotKey hotKey in hotKeys)
			{
				hotKeyHost.AddHotKey(hotKey);
			}
		}
		catch (HotKeyAlreadyRegisteredException)
		{
			Settings.Instance.HotkeyText = "hotkey already registered error";
		}
	}

	public static void Quit()
	{
		if (ts != null)
		{
			ts.Cancel();
			try
			{
				Task.WaitAll(Task.Delay(10));
			}
			catch (AggregateException ex)
			{
				ex.InnerException.ToString();
			}
		}
		Application.Current.Shutdown();
	}

	public static void SetDebug()
	{
		DebugMode = File.Exists("C:\\mik\\foedebug.txt");
		LossesDebug = File.Exists("C:\\mik\\foelossdebug.txt");
		UnitsDebug = File.Exists("C:\\mik\\foeunitsdebug.txt");
		BattleDebug = File.Exists("C:\\mik\\foebattledebug.txt");
		OCRDebug = File.Exists("C:\\mik\\foeocrdebug.txt");
	}

	public static bool resimJednotky()
	{
		return !Settings.Instance.ReplaceUnitsR;
	}

	public static string getShoppingRatio(int shoppingMode)
	{
		switch (shoppingMode)
		{
		case 0:
		case 2:
			return "2:1";
		case 1:
			return "1:1";
		default:
			return "1:1";
		}
	}

	public static int getShoppingCount(ShoppingProp prop, int otherAmount)
	{
		if (prop != ShoppingProp.Demand)
		{
			return (int)Math.Ceiling((float)otherAmount / demandRatio);
		}
		return (int)Math.Ceiling((float)otherAmount * demandRatio);
	}

	public static string getShoppingCount(ShoppingProp prop, string otherAmount)
	{
		if (int.TryParse(otherAmount, out var result))
		{
			return getShoppingCount(prop, result).ToString();
		}
		return "";
	}

	public static void BreakTask()
	{
		window.BreakTask();
	}

	public static void ResumeTask()
	{
		mainState = 0;
		outInfo("resumed");
		window.ResumeTask();
	}

	public static int getXfrom()
	{
		if (PixelTools.MaxX >= 1290)
		{
			return PixelTools.MaxX / 4;
		}
		return 50;
	}

	public static int getXto()
	{
		return PixelTools.MaxX * 3 / 4;
	}

	public static int getYfrom()
	{
		if (PixelTools.MaxY >= 730)
		{
			return PixelTools.MaxY / 5;
		}
		return 100;
	}

	public static int getYto()
	{
		return PixelTools.MaxY * 3 / 5;
	}

	public static void WriteLogFile(string txt, string filePath = "foelog.txt")
	{
		using StreamWriter streamWriter = new StreamWriter(filePath, append: true);
		streamWriter.WriteLine(txt);
	}

	public static void deleteDebugFile()
	{
		try
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch
		{
		}
	}

	public static void writeDebugFile(string txt)
	{
		try
		{
			using StreamWriter streamWriter = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.txt"), append: true);
			streamWriter.WriteLine(txt);
		}
		catch
		{
		}
	}

	public static void ClearLogFile(string filePath = "foelog.txt")
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	public static void Wait(int howLong = 0, bool randomize = true)
	{
		if (window.ct.IsCancellationRequested)
		{
			window.ct.ThrowIfCancellationRequested();
		}
		if (howLong == 0)
		{
			howLong = shortWait;
		}
		if (ScaredFactor > 0)
		{
			howLong += 100 * ScaredFactor;
		}
		int num = 0;
		if (randomize)
		{
			num = random.Next(-howLong / 5, howLong / 5);
		}
		Thread.Sleep(howLong + num);
	}

	public static void outInfo(string txt, bool clear = false, bool debugInfo = false)
	{
		if (debugInfo)
		{
			if (clear)
			{
				Settings.Instance.DebugInfo = txt;
				return;
			}
			Settings instance = Settings.Instance;
			instance.DebugInfo = instance.DebugInfo + "\n" + getStopky() + ": " + txt;
		}
		else if (clear)
		{
			Settings.Instance.Mout = txt;
		}
		else
		{
			Settings instance = Settings.Instance;
			instance.Mout = instance.Mout + "\n" + getStopky() + ": " + txt;
		}
	}

	public void CoordDebug()
	{
		string coordDebug = Settings.Instance.CoordDebug;
		new Point(0, 0);
		if (coordDebug != null)
		{
			try
			{
				MouseTools.MoveMouse(coord.get(coordDebug));
			}
			catch
			{
			}
			Thread.Sleep(3000);
		}
	}

	public void GoShopDebug()
	{
		_ = Settings.Instance.CoordDebug;
		new Point(0, 0);
		coord.add(new Point(-126, 173), "debug1", coord.fromCudlik);
		coord.add(new Point(37, 193), "debug2", coord.fromCudlik);
		MouseTools.MoveMouse(coord.get("debug1"));
		Thread.Sleep(3000);
		MouseTools.MoveMouse(coord.get("debug2"));
		Thread.Sleep(3000);
	}

	public void ColorDebug()
	{
		int num = 0;
		int num2 = 0;
		if (coord != null && coord.coordExists("relatedPointTo"))
		{
			try
			{
				num = int.Parse(Settings.Instance.ColorXDebug);
				num2 = int.Parse(Settings.Instance.ColorYDebug);
			}
			catch (Exception)
			{
			}
			Point point = coord.get("relatedPointTo");
			int x = MouseTools.GetCursorPosition().X;
			int y = MouseTools.GetCursorPosition().Y;
			Color colorAt = PixelTools.GetColorAt(x, y);
			int num3 = x - point.X;
			int num4 = y - point.Y;
			outInfo($"abs:{x},{y} relat:{num3},{num4} {colorAt} relat: {point.X},{point.Y}", clear: true);
			if (num > 0 && num2 > 0)
			{
				num3 = num - point.X;
				num4 = num2 - point.Y;
				colorAt = PixelTools.GetColorAt(num, num2);
				outInfo($"abs:{num},{num2} relat:{num3},{num4} {colorAt} relat: {point.X},{point.Y}", clear: true, debugInfo: true);
			}
			Thread.Sleep(200);
		}
	}

	public static string getStopky(bool zeros = false)
	{
		if (window == null)
		{
			if (!zeros)
			{
				return "";
			}
			return "00000";
		}
		double num = Math.Round(window.stopwatch.Elapsed.TotalSeconds, 1);
		if (zeros)
		{
			return (num * 10.0).ToString().PadLeft(5, '0');
		}
		return num.ToString();
	}

	public int goShopInit(bool debug = false)
	{
		outInfo("Init shop started, please wait..");
		if (!shopInitDone)
		{
			PixelTools.reinit();
			coord = new Coordinates();
			coord.add(new Point(1920, 1080), "resDefault");
			coord.add(new Point(PixelTools.MaxX, PixelTools.MaxY), "resThis");
			status = 0;
			PixelTools.CaptureScreenDBM();
			bool pixelHunt = Settings.Instance.PixelHunt;
			string[] seq = new string[2] { "7E869C", "535E77" };
			Point point = new Point(0, 0);
			try
			{
				if (pixelHunt)
				{
					point = PixelTools.FindPixelSequenceLeftToRight(seq, getXfrom(), getXto(), getYfrom(), getYto()).Value;
				}
				else
				{
					(float scalingFactorX, float scalingFactorY) screenZoomSettings = ImageWorks.GetScreenZoomSettings();
					float item = screenZoomSettings.scalingFactorX;
					float item2 = screenZoomSettings.scalingFactorY;
					Bitmap bmp = ImageWorks.getBmp("./images/otaznik-fh.png", item, item2, "./images/otaznik-150.png");
					Bitmap templateBitmap = PixelTools.CaptureScreen(getXfrom(), getYfrom(), getXto(), getYto());
					if (!ImageWorks.FindTemplateEmguExist(bmp, templateBitmap))
					{
						outInfo("Init shop failed #13 (is create offer window opened?)");
						WriteLogFile("cudlik not found");
						return -1;
					}
					point = PixelTools.MovePoint(ImageWorks.LastFound, new Point(getXfrom(), getYfrom()));
				}
				coord.add(point, "shopHorniThis");
			}
			catch (Exception)
			{
				outInfo("Init shop failed #12 (is create offer window opened?)");
				WriteLogFile("cudlik not found");
				return -1;
			}
			if (!nalezeniZoomuRamecekImage())
			{
				return -1;
			}
			WriteLogFile($"shopHorniThis {point.X},{point.Y}");
			status = 1;
			Settings.Instance.ShopEnabled = true;
			coord.add(coord.get("shopHorniThis"), "relatedPointTo");
			coord.add(new Point(-430, 216), "shopZboziCislaSloupec1Start", coord.fromCudlik);
			coord.add(new Point(-383, 485), "shopZboziCislaSloupec1End", coord.fromCudlik);
			coord.add(new Point(-349, 120), "offerRozklikCheck", coord.fromCudlik);
			coord.add(new Point(-348, 190), "sipkaUpDemand", coord.fromCudlik);
			coord.add(new Point(-348, 379), "sipkaDownDemand", coord.fromCudlik);
			coord.add(new Point(-348, 321), "offerScrollBot", coord.fromCudlik);
			coord.add(new Point(-348, 363), "demandScrollBot", coord.fromCudlik);
			coord.add(new Point(-349, 149), "sipkaUpOffer", coord.fromCudlik);
			coord.add(new Point(-398, 149), "firstOffer", coord.fromCudlik);
			coord.add(new Point(-349, 338), "sipkaDownOffer", coord.fromCudlik);
			coord.add(new Point(-398, 190), "firstDemand", coord.fromCudlik);
			coord.add(new Point(-328, 252), "offerSuccessCheck", coord.fromCudlik);
			coord.addRandomize(new Point(-449, 134), new Point(-386, 150), "offerVolume", coord.fromCudlik);
			coord.addRandomize(new Point(-61, 134), new Point(-1, 150), "demandVolume", coord.fromCudlik);
			coord.addRandomize(new Point(-557, 134), new Point(-507, 141), "offer", coord.fromCudlik);
			coord.addRandomize(new Point(-151, 134), new Point(-121, 141), "demand", coord.fromCudlik);
			coord.addRandomize(new Point(-16, 173), new Point(37, 193), "offerButton", coord.fromCudlik);
			coord.add(new Point(-600, 500), "shopDebugVlevoDole", coord.fromCudlik);
		}
		shopInitDone = true;
		if (!debug)
		{
			if (shoppingMode == 0)
			{
				shopFinderOffer = new ShopFinder(coord, SelectedEra);
				if (!shopFinderOffer.loadShopOverView())
				{
					outInfo("error era not found");
					return -1;
				}
				shopFinderOffer.shopSetOfferAndDemandNoWheel(0, 0, -1, -1);
				shopFinderDemand = shopFinderOffer;
				OfferEra = SelectedEra;
				DemandEra = SelectedEra;
			}
			else
			{
				OfferEra = SelectedEra;
				DemandEra = SelectedEra + 1;
				if (shoppingMode == 2)
				{
					OfferEra = SelectedEra;
					DemandEra = SelectedEra - 1;
				}
				shopFinderDemand = new ShopFinder(coord, DemandEra);
				if (!shopFinderDemand.loadShopOverView(goFull: true, setDemandCheckBoxes: true, setSupplyCheckBoxes: false))
				{
					outInfo("demand error era not found");
					return -1;
				}
				shopFinderDemand.shopSetOfferAndDemandNoWheel(-1, 0, -1, -1);
				shopFinderOffer = new ShopFinder(coord, OfferEra);
				if (!shopFinderOffer.loadShopOverView(goFull: true, setDemandCheckBoxes: false))
				{
					outInfo("offer error era not found");
					return -1;
				}
				shopFinderOffer.shopSetOfferAndDemandNoWheel(0, -1, -1, -1);
			}
		}
		if (false)
		{
			((Image)PixelTools.CaptureRegionDBM(coord.get("relatedPointTo"), coord.get("shopDebugVlevoDole")).Bitmap).Save("debugshop.png");
		}
		outInfo("Init shop ok");
		return 1;
	}

	public bool nalezeniZoomuRamecekImage(int roztecDefaultX = 816, int roztecDefaultY = 560, bool isShop = true, bool isBattles = false)
	{
		if (ZoomBattlesIndentified && coord.coordExists("ramecekTopleft"))
		{
			coord.zoomX = Settings.Instance.ZoomSettings[0];
			coord.zoomY = Settings.Instance.ZoomSettings[1];
			return true;
		}
		float relativeResolutionX = coord.getRelativeResolutionX();
		float relativeResolutionY = coord.getRelativeResolutionY();
		int num = (int)(relativeResolutionX * 900f);
		int num2 = (int)(relativeResolutionY * 50f);
		int num3 = (int)(relativeResolutionX * 70f);
		int num4 = (int)(relativeResolutionY * 700f);
		Point topLeft = new Point(0, 0);
		Point botRight = new Point(0, 0);
		Point p = new Point(0, 0);
		Point p2 = new Point(0, 0);
		Point p3 = new Point(0, 0);
		Point p4 = new Point(0, 0);
		bool flag = false;
		try
		{
			var (resizeX, resizeY) = ImageWorks.GetScreenZoomSettings();
			if (isShop)
			{
				Bitmap bitmap = PixelTools.GetActiveDMB().Bitmap;
				Bitmap bmp = ImageWorks.getBmp("./images/levy-ramecek-fh.png", resizeX, resizeY, "./images/levy-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(bitmap, bmp))
				{
					p3 = ImageWorks.LastFound;
				}
				bmp = ImageWorks.getBmp("./images/pravy-ramecek-fh.png", resizeX, resizeY, "./images/pravy-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(bitmap, bmp, 0.9200000166893005, includeTemplateSize: true))
				{
					p4 = ImageWorks.LastFound;
				}
				bmp = ImageWorks.getBmp("./images/horni-ramecek-fh.png", resizeX, resizeY, "./images/horni-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(bitmap, bmp))
				{
					p = ImageWorks.LastFound;
				}
				bmp = ImageWorks.getBmp("./images/fr-bot-sh.png", resizeX, resizeY, "./images/fr-bot-sh150.png");
				if (ImageWorks.FindTemplateEmguExist(bitmap, bmp, 0.9200000166893005, includeTemplateSize: true))
				{
					p2 = ImageWorks.LastFound;
				}
			}
			else
			{
				int num5 = p1.X - num;
				int num6 = p1.Y - num2;
				int num7 = p1.X + num3;
				int num8 = p1.Y + num4;
				if (num5 < 0)
				{
					num5 = 0;
				}
				if (num6 < 0)
				{
					num6 = 0;
				}
				if (num7 > PixelTools.MaxX)
				{
					num7 = PixelTools.MaxX - 1;
				}
				if (num8 > PixelTools.MaxY)
				{
					num8 = PixelTools.MaxY - 1;
				}
				Rectangle r = new Rectangle(num5, num6, num7 - num5, num8 - num6);
				Bitmap sourceBitmap = PixelTools.CaptureScreenRect(r);
				Bitmap bmp = ImageWorks.getBmp("./images/levy-ramecek-fh.png", resizeX, resizeY, "./images/levy-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp))
				{
					p3 = new Point(ImageWorks.LastFound.X + r.X, ImageWorks.LastFound.Y + r.Y);
				}
				bmp = ImageWorks.getBmp("./images/pravy-ramecek-fh.png", resizeX, resizeY, "./images/pravy-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp, 0.9200000166893005, includeTemplateSize: true))
				{
					p4 = new Point(ImageWorks.LastFound.X + r.X, ImageWorks.LastFound.Y + r.Y);
				}
				bmp = ImageWorks.getBmp("./images/horni-ramecek-fh.png", resizeX, resizeY, "./images/horni-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp))
				{
					p = new Point(ImageWorks.LastFound.X + r.X, ImageWorks.LastFound.Y + r.Y);
				}
				bmp = ImageWorks.getBmp("./images/dolni-ramecek-fh.png", resizeX, resizeY, "./images/dolni-ramecek150.png");
				if (ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp, 0.9200000166893005, includeTemplateSize: true))
				{
					p2 = new Point(ImageWorks.LastFound.X + r.X, ImageWorks.LastFound.Y + r.Y);
				}
				if (PixelTools.pointIsNull(p3) || PixelTools.pointIsNull(p4) || PixelTools.pointIsNull(p) || PixelTools.pointIsNull(p2))
				{
					outInfo("fullscr mode");
					Bitmap sourceBitmap2 = PixelTools.CaptureScreen();
					bmp = ImageWorks.getBmp("./images/levy-ramecek-fh.png", resizeX, resizeY, "./images/levy-ramecek150.png");
					if (ImageWorks.FindTemplateEmguExist(sourceBitmap2, bmp))
					{
						p3 = new Point(ImageWorks.LastFound.X, ImageWorks.LastFound.Y);
					}
					bmp = ImageWorks.getBmp("./images/pravy-ramecek-fh.png", resizeX, resizeY, "./images/pravy-ramecek150.png");
					if (ImageWorks.FindTemplateEmguExist(sourceBitmap2, bmp, 0.9200000166893005, includeTemplateSize: true))
					{
						p4 = new Point(ImageWorks.LastFound.X, ImageWorks.LastFound.Y);
					}
					bmp = ImageWorks.getBmp("./images/horni-ramecek-fh.png", resizeX, resizeY, "./images/horni-ramecek150.png");
					if (ImageWorks.FindTemplateEmguExist(sourceBitmap2, bmp))
					{
						p = new Point(ImageWorks.LastFound.X, ImageWorks.LastFound.Y);
					}
					bmp = ImageWorks.getBmp("./images/dolni-ramecek-fh.png", resizeX, resizeY, "./images/dolni-ramecek150.png");
					if (ImageWorks.FindTemplateEmguExist(sourceBitmap2, bmp, 0.9200000166893005, includeTemplateSize: true))
					{
						p2 = new Point(ImageWorks.LastFound.X, ImageWorks.LastFound.Y);
					}
				}
			}
			topLeft = new Point(p3.X, p.Y);
			botRight = new Point(p4.X, p2.Y);
			_ = DebugMode;
			flag = true;
		}
		catch (Exception)
		{
			outInfo("frame not found #1");
			WriteLogFile("ramecek not found #1");
		}
		int num9;
		int num10;
		Point p5;
		Point p6;
		if (!flag)
		{
			outInfo("Warning- frame init failed. Program will likely not work correctly.");
			WriteLogFile("ramecek not found using default");
			num9 = roztecDefaultX;
			num10 = roztecDefaultY;
			p5 = new Point(0, 0);
			p6 = new Point(PixelTools.MaxX, PixelTools.MaxY);
		}
		else
		{
			num9 = botRight.X - topLeft.X;
			num10 = botRight.Y - topLeft.Y;
			p5 = new Point(p3.X, p.Y);
			p6 = new Point(p4.X, p2.Y);
		}
		coord.add(p5, "ramecekTopleft");
		coord.add(p6, "ramecekBotright");
		coord.setZoom((float)num9 / (float)roztecDefaultX, (float)num10 / (float)roztecDefaultY);
		coord.zoomX = coord.zoomY;
		if (coord.zoomX <= 0f || coord.zoomY <= 0f)
		{
			outInfo("Error- init failed - zoom detection error. Check your zoom settings in windows and in browser (ctrl+mousewheel)");
			return false;
		}
		ImageWorks.zoomLevel = coord.zoomX;
		Settings.Instance.ZoomSettings = new float[2] { coord.zoomX, coord.zoomY };
		ZoomBattlesIndentified = true;
		if (isBattles)
		{
			Bitmap bmp2 = ImageWorks.getBmp("./images/chautobattle.png", coord.zoomX, coord.zoomY);
			if (!ImageWorks.FindTemplateEmguExist(PixelTools.CaptureScreenRect(topLeft, botRight), bmp2))
			{
				outInfo("cant init, seems like we don't have the window with autobattle button opened");
				return false;
			}
		}
		return true;
	}

	private void selectFirstDemand()
	{
		MouseTools.ClickMouse(coord.get("firstDemand"));
		Wait(shortWait);
	}

	private void selectFirstOffer()
	{
		MouseTools.ClickMouse(coord.get("firstOffer"));
		Wait(shortWait);
	}

	private void shopMakeDeal()
	{
		bool pixelHunt = Settings.Instance.PixelHunt;
		MouseTools.ClickMouse(coord.get("offerButton"));
		if (pixelHunt)
		{
			if (!new ColorCheck("E3D3A9", 3.0).WaitForColor(coord.get("offerSuccessCheck")))
			{
				throw new Exception("offer timeout");
			}
			outInfo("offer made");
		}
		else
		{
			if (!ImageWorks.WaitFindTemplateEmguExist(ImageWorks.getBmp("./images/offer-succ-fh.png", coord.zoomX, coord.zoomY, "./images/offer-succ-150.png"), coord.get("ramecekTopleft"), coord.get("ramecekBotright")))
			{
				outInfo("offer timeout");
				_ = RunDebug;
				return;
			}
			outInfo("offer made");
		}
		Wait(shortWait);
		SendKeys.SendWait("{ESC}");
		Wait(100);
		Wait(shortWait);
		if (pixelHunt)
		{
			if (!new ColorCheck("B3BACA", 3.0).WaitForColor(coord.get("offerRozklikCheck")))
			{
				throw new Exception("after offer timeout");
			}
			outInfo("ready for next offer");
		}
		else if (ImageWorks.WaitFindTemplateEmguExist(ImageWorks.getBmp("./images/offer-arrow-fh.png", coord.zoomX, coord.zoomY, "./images/offer-arrow-150.png"), coord.get("ramecekTopleft"), coord.get("ramecekBotright")))
		{
			outInfo("offer made");
		}
		else
		{
			outInfo("after offer timeout");
		}
	}

	private void shopSetOfferAndDemand(int reqS, int reqD, int actS, int actD)
	{
		if (reqS != actS)
		{
			MouseTools.ClickMouse(coord.get("offer"));
			Wait(midWait);
			bool flag = false;
			Color colorAt = PixelTools.GetColorAt(coord.get("offerScrollBot"));
			if (PixelTools.CompareColors(colorAt, PixelTools.StrColorToColor("#B0B8C7"), 0.9))
			{
				flag = true;
			}
			else if (PixelTools.CompareColors(colorAt, PixelTools.StrColorToColor("#8892A8"), 0.9))
			{
				flag = true;
			}
			offerSet = true;
			if (flag)
			{
				offerSet = false;
				if (reqS > actS)
				{
					int num = ShopFinder.endEraGoods(OfferEra, reqS);
					Point p = new Point(0, shopGoodSelectHeight.Y * num);
					MouseTools.ClickMouse(coord.MoveRelative(p, "firstOffer", coord.useZoom));
				}
			}
			else
			{
				if (!new ColorCheck("B3BACA", 3.0).WaitForColor(coord.get("sipkaDownOffer")))
				{
					outInfo("can't set demand. Maybe you have more than 1000 trades already.");
					BreakTask();
					return;
				}
				outInfo("offer select ok");
				if (reqS > actS)
				{
					MouseTools.MoveMouse(coord.get("sipkaUpOffer"));
					for (int i = 0; i < reqS - actS; i++)
					{
						MouseTools.DoMouseWheel();
						Wait(shortWait);
						shopOffer++;
					}
					selectFirstOffer();
				}
				if (reqS < actS)
				{
					MouseTools.MoveMouse(coord.get("sipkaUpOffer"));
					for (int j = 0; j < actS - reqS; j++)
					{
						MouseTools.DoMouseWheel("up");
						Wait(shortWait);
						shopOffer--;
					}
					selectFirstOffer();
				}
			}
		}
		if (reqD == actD || (reqD == reqS && shoppingMode == 0))
		{
			return;
		}
		Wait(midWait);
		MouseTools.ClickMouse(coord.get("demand"));
		Wait(midWait);
		bool flag2 = false;
		Color colorAt2 = PixelTools.GetColorAt(coord.get("demandScrollBot"));
		if (PixelTools.CompareColors(colorAt2, PixelTools.StrColorToColor("#AFB7C6"), 0.9))
		{
			flag2 = true;
		}
		else if (PixelTools.CompareColors(colorAt2, PixelTools.StrColorToColor("#8892A8"), 0.9))
		{
			flag2 = true;
		}
		demandSet = true;
		if (flag2)
		{
			int num2 = DemandEra;
			if (shoppingMode != 0)
			{
				num2++;
			}
			int num3 = ShopFinder.endEraGoods(num2, reqD);
			Point p = new Point(0, shopGoodSelectHeight.Y * num3);
			MouseTools.ClickMouse(coord.MoveRelative(p, "firstDemand", coord.useZoom));
		}
		else if (new ColorCheck("B4BBCA", 3.0).WaitForColor(coord.get("sipkaDownDemand")))
		{
			outInfo("demand select ok");
			if (reqD > actD)
			{
				MouseTools.MoveMouse(coord.get("sipkaUpDemand"));
				for (int k = 0; k < reqD - actD; k++)
				{
					MouseTools.DoMouseWheel();
					Wait(shortWait);
					shopDemand++;
				}
				selectFirstDemand();
			}
			if (reqD < actD)
			{
				MouseTools.MoveMouse(coord.get("sipkaUpDemand"));
				for (int l = 0; l < actD - reqD; l++)
				{
					MouseTools.DoMouseWheel("up");
					Wait(shortWait);
					shopDemand--;
				}
				selectFirstDemand();
			}
		}
		else
		{
			outInfo("can't set demand. Maybe you have more than 1000 trades already.");
			BreakTask();
		}
	}

	private bool getCheckBox(string name, int num)
	{
		int index = ((!(name == "supply")) ? (5 + (num - 1)) : (num - 1));
		if (Settings.Instance.ShopCheckBox[index].GetValueOrDefault())
		{
			return true;
		}
		return false;
	}

	public void goShop(int offersCount)
	{
		shopOffer = 1;
		shopDemand = 1;
		int num = 1;
		int num2 = 1;
		try
		{
			supplyVol = int.Parse(Settings.Instance.SupplyVol);
		}
		catch
		{
			supplyVol = 0;
			outInfo("Please provide correct supply volume amount");
			return;
		}
		MouseTools.ClickMouse(coord.get("offerVolume"), doubleClick: true);
		Wait(shortWait);
		SendKeys.SendWait(supplyVol.ToString() ?? "");
		Wait(shortWait);
		try
		{
			demandVol = int.Parse(Settings.Instance.DemandVol);
		}
		catch
		{
			demandVol = 0;
			outInfo("Please provide correct demand volume amount");
			return;
		}
		MouseTools.ClickMouse(coord.get("demandVolume"), doubleClick: true);
		Wait(shortWait);
		SendKeys.SendWait(demandVol.ToString() ?? "");
		Wait(shortWait);
		int num3 = -1;
		int num4 = -1;
		for (int i = 1; i < 6; i++)
		{
			for (int j = 1; j < 6; j++)
			{
				if ((num != num2 || shoppingMode != 0) && getCheckBox("supply", num) && getCheckBox("demand", num2))
				{
					if (shoppingMode == 0)
					{
						if (!offerSet && num3 != i)
						{
							shopFinderOffer.rescan();
							shopFinderOffer.shopSetOfferAndDemandNoWheel(num - 1, -1, shopOffer - 1, -1);
						}
						if (!demandSet && num4 != j)
						{
							shopFinderDemand.rescan();
							shopFinderDemand.shopSetOfferAndDemandNoWheel(-1, num2 - 1, -1, shopDemand - 1);
						}
					}
					else
					{
						shopSetOfferAndDemand(num, num2, shopOffer, shopDemand);
					}
					for (int k = 0; k < offersCount; k++)
					{
						num3 = i;
						num4 = j;
						try
						{
							shopMakeDeal();
						}
						catch
						{
							outInfo("shopping deal error");
							return;
						}
						if (shoppingMode == 0)
						{
							shopOffer = num;
							shopDemand = num2;
						}
					}
				}
				num2++;
			}
			num2 -= 5;
			num++;
		}
	}

	public void GoFightFill(bool quantum = false)
	{
		PixelTools.setAcceptableColorPrecision(1.2000000476837158);
		fb.fillUnitsWithCheck(checkForInv: true, 0, quantum);
	}

	public void GoFightBattle(int battlesCount, int minUnits, int fewBattlesRemain, bool skipInit = false)
	{
		if (!fightInitDone && !skipInit)
		{
			GoFightInit();
			if (!fightInitDone)
			{
				outInfo("error, fight init failed. If you do not check option to fight 20% sectors automatically, you need to have the window with units and autobattle button opened");
				return;
			}
			Settings.Instance.FightEnabled = true;
		}
		PixelTools.setAcceptableColorPrecision(1.2000000476837158);
		fb.fewBattlesRemain = fewBattlesRemain;
		fb.GoGuildBattle(battlesCount, minUnits);
	}

	public void GoFightExpeditions(int battlesCount, int minUnits)
	{
		PixelTools.setAcceptableColorPrecision(1.2000000476837158);
		fb.GoFightExpeditions(battlesCount, minUnits);
	}

	public void GoFightSetMinHealth(int unitsMinHealth)
	{
		PixelTools.setAcceptableColorPrecision(1.2000000476837158);
		if (fb != null)
		{
			fb.MinHealthAllowed = unitsMinHealth;
		}
	}

	public bool GoFightDebug()
	{
		Bitmap srcBmp = PixelTools.CaptureScreen();
		string battleResult = null;
		fb = new FoeBattle(initBattleScreen: false);
		coord = new Coordinates();
		coord.zoomX = 1f;
		coord.zoomY = 1f;
		if (!fightInitDone)
		{
			GoFightInit();
		}
		battleResult = fb.imgBattleResult("autoBattleCheck", srcBmp, battleResult);
		battleResult = fb.imgBattleResult("nextRound", srcBmp, battleResult);
		battleResult = null;
		battleResult = fb.imgBattleResult("fightOk", srcBmp, battleResult);
		outInfo(battleResult);
		Thread.Sleep(1000);
		return true;
	}

	public bool findXcudlik(bool silent = false)
	{
		(float scalingFactorX, float scalingFactorY) screenZoomSettings = ImageWorks.GetScreenZoomSettings();
		float item = screenZoomSettings.scalingFactorX;
		float item2 = screenZoomSettings.scalingFactorY;
		float relativeResolutionX = coord.getRelativeResolutionX();
		float relativeResolutionY = coord.getRelativeResolutionY();
		int x = (int)(relativeResolutionX * 1043f);
		int y = (int)(relativeResolutionY * 169f);
		int width = (int)(relativeResolutionX * 558f);
		int height = (int)(relativeResolutionY * 285f);
		Rectangle r = new Rectangle(x, y, width, height);
		Bitmap sourceBitmap = PixelTools.CaptureScreenRect(r);
		Bitmap bmp = ImageWorks.getBmp("./images/battle-init-fh.png", item, item2, "./images/battle-init150.png");
		if (ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp))
		{
			Point lastFound = ImageWorks.LastFound;
			p1 = new Point(lastFound.X + r.X, lastFound.Y + r.Y);
			return true;
		}
		if (ImageWorks.FindTemplateEmguExist(PixelTools.CaptureScreen(), bmp))
		{
			Point lastFound2 = ImageWorks.LastFound;
			p1 = new Point(lastFound2.X, lastFound2.Y);
			return true;
		}
		if (!silent)
		{
			outInfo("error #12, make sure GB window is opened");
		}
		return false;
	}

	public bool GoFightInit(bool silent = false, bool quantumIncursions = false, bool historicalAllies = false)
	{
		if (!silent)
		{
			outInfo("battle init started, please wait..");
		}
		PixelTools.reinit();
		PixelTools.CaptureScreenDBM();
		PixelTools.setAcceptableColorPrecision();
		coord = new Coordinates();
		if (Settings.Instance.PixelHunt)
		{
			outInfo("pixel hunt no longer supported, change settings");
			return false;
		}
		if (!findXcudlik(historicalAllies))
		{
			if (historicalAllies)
			{
				Bitmap bmp = ImageWorks.getBmp("./images/foe-hist-start.png");
				if (ImageWorks.FindTemplateEmguExist(PixelTools.CaptureScreen(), bmp))
				{
					outInfo("Hub detected, starting loop...");
					fb = new FoeBattle(initBattleScreen: false);
					fightInitDone = true;
					return true;
				}
			}
			return false;
		}
		return FinishGoFightInit(silent, quantumIncursions, historicalAllies);
	}

	public bool FinishGoFightInit(bool silent = false, bool quantumIncursions = false, bool historicalAllies = false)
	{
		coord.add(p1, "battleHorniThis");
		if (resimJednotky() || quantumIncursions)
		{
			if (!silent)
			{
				outInfo("battle init, wait for unit init..");
			}
		}
		else if (!silent)
		{
			outInfo("battle init");
		}
		if (quantumIncursions)
		{
			if (!nalezeniZoomuRamecekImage(761, 521, isShop: false, isBattles: true))
			{
				return false;
			}
		}
		else if (!nalezeniZoomuRamecekImage(1011, 603, isShop: false, !historicalAllies))
		{
			return false;
		}
		coord.add(coord.get("battleHorniThis"), "relatedPointTo");
		if (DebugMode)
		{
			outInfo($"relatedpoint {p1.X}{p1.Y}");
		}
		coord.add(new Point(-693, 88), "battleUnit1", coord.fromCudlik);
		coord.add(new Point(-626, 88), "battleUnit3", coord.fromCudlik);
		coord.add(new Point(-559, 88), "battleUnit5", coord.fromCudlik);
		coord.add(new Point(-492, 88), "battleUnit7", coord.fromCudlik);
		coord.add(new Point(-693, 157), "battleUnit2", coord.fromCudlik);
		coord.add(new Point(-626, 157), "battleUnit4", coord.fromCudlik);
		coord.add(new Point(-559, 157), "battleUnit6", coord.fromCudlik);
		coord.add(new Point(-492, 157), "battleUnit8", coord.fromCudlik);
		int num = 71;
		int num2 = -618;
		int num3 = 383;
		for (int i = 0; i < 18; i++)
		{
			if ((i + 1) % 2 == 0)
			{
				num3 = 453;
			}
			else
			{
				num3 = 383;
				if (i > 1)
				{
					num2 += 70;
				}
			}
			coord.add(new Point(num2, num3 - num - 13), "invUnit" + i, coord.fromCudlik);
		}
		coord.add(new Point(-649, 512 - num), "unitInvscrollBarImgStart", coord.fromCudlik);
		coord.add(new Point(27, 546 - num), "unitInvscrollBarImgEnd", coord.fromCudlik);
		coord.add(new Point(-621, 530 - num), "unitInvscrollBarStart", coord.fromCudlik);
		coord.add(new Point(-12, 532 - num), "unitInvscrollBarEnd", coord.fromCudlik);
		coord.add(new Point(-631, 534 - num), "unitInvscrollBarLeft", coord.fromCudlik);
		coord.add(new Point(2, 535 - num), "unitInvscrollBarRight", coord.fromCudlik);
		coord.add(new Point(-300, 533 - num), "unitInvscrollBarMid", coord.fromCudlik);
		coord.add(new Point(-694, 496), "autoBattleCheck", coord.fromCudlik);
		coord.add(new Point(-380, 560), "autoBattleClick", coord.fromCudlik);
		coord.add(new Point(-424, 566), "fightOkCheck", coord.fromCudlik);
		coord.add(new Point(-337, 566), "fightOkClick", coord.fromCudlik);
		coord.add(new Point(-424, 513), "fightOkCheck2", coord.fromCudlik);
		coord.add(new Point(-337, 513), "fightOkClick2", coord.fromCudlik);
		coord.add(new Point(-485, 395), "goFightCheck", coord.fromCudlik);
		coord.add(new Point(-485, 325), "goFightCheckTagginOn", coord.fromCudlik);
		coord.add(new Point(-453, 371), "goFightClick", coord.fromCudlik);
		coord.add(new Point(-380, 410), "goFightQuantumClick", coord.fromCudlik);
		coord.add(new Point(-441, 559), "multiRoundBattleCheck", coord.fromCudlik);
		coord.add(new Point(-368, 557), "nextBattleRoundClick", coord.fromCudlik);
		coord.add(new Point(-410, 407), "rewardCheck", coord.fromCudlik);
		coord.add(new Point(-399, 406), "rewardOkClick", coord.fromCudlik);
		coord.add(new Point(-399, 406), "surrenderCancel", coord.fromCudlik);
		coord.add(new Point(-308, 547), "safetyRescue", coord.fromCudlik);
		fb = new FoeBattle(initBattleScreen: true, quantumIncursions);
		if (resimJednotky() || quantumIncursions)
		{
			if (fb.getUnitsInit() > 0)
			{
				if (!silent)
				{
					outInfo("units init");
				}
				return true;
			}
			((Image)PixelTools.GetActiveDMB().Bitmap).Save("16-debug.png");
			if (!silent)
			{
				outInfo("units init failed #16, please check you have army management window visible and you don't use zoom in the browser");
			}
			return false;
		}
		fightInitDone = true;
		return true;
	}

	internal FoeBattle getFb()
	{
		return fb;
	}

	public int getLockedUnitsCount()
	{
		if (fb != null)
		{
			return fb.lockedUnitsCnt;
		}
		return 0;
	}

	public static void GetZoomImageHuntOnMainScreen()
	{
		if (ZoomMainIdentified)
		{
			return;
		}
		int num = 704;
		int num2 = 118;
		bool flag = false;
		Bitmap bmp = ImageWorks.getBmp("./images/init-cudlik2-150.png");
		PixelTools.CaptureScreenDBM();
		Bitmap bitmap = PixelTools.GetActiveDMB().Bitmap;
		if (ImageWorks.FindTemplateEmguExist(bitmap, bmp))
		{
			flag = true;
			p1 = ImageWorks.LastFound;
			bmp = ImageWorks.getBmp("./images/cudlik-dolni150.png");
			if (ImageWorks.FindTemplateEmguExist(bitmap, bmp))
			{
				p2 = ImageWorks.LastFound;
			}
		}
		if (!flag)
		{
			bmp = ImageWorks.getBmp("./images/init-cudlik-fh.png");
			if (ImageWorks.FindTemplateEmguExist(bitmap, bmp))
			{
				p1 = ImageWorks.LastFound;
				flag = true;
				bmp = ImageWorks.getBmp("./images/cudlik-dolni-fh.png");
				if (ImageWorks.FindTemplateEmguExist(bitmap, bmp))
				{
					p2 = ImageWorks.LastFound;
				}
			}
		}
		if (PixelTools.IsPoint0(p1) && PixelTools.IsPoint0(p2))
		{
			outInfo("warning: using default zoom", clear: true);
			coord.setZoom(1f, 1f);
		}
		else
		{
			status = 1;
			int num3 = p2.X - p1.X;
			int num4 = p2.Y - p1.Y;
			coord.setZoom((float)num3 / (float)num, (float)num4 / (float)num2);
		}
		ZoomMainIdentified = true;
	}

	public void GoQuantumBattle(int battlesCount, int minUnits, int fewBattlesRemain)
	{
		PixelTools.setAcceptableColorPrecision(1.2000000476837158);
		if (fb != null)
		{
			fb.fewBattlesRemain = fewBattlesRemain;
			fb.GoQuantumBattle(battlesCount, minUnits);
		}
	}

	public void GoHistoricalAlliesBattle(int battlesCount, int minUnits, int fewBattlesRemain, bool doAttack, bool doDefend, bool doFp, bool doMedals)
	{
		PixelTools.setAcceptableColorPrecision(1.2000000476837158);
		if (fb == null)
		{
			outInfo("Historical Allies: not initialized, make sure you start on the army management screen");
			return;
		}
		fb.fewBattlesRemain = fewBattlesRemain;
		fb.GoHistoricalBattle(battlesCount, minUnits, doAttack, doDefend, doFp, doMedals);
	}

	public bool GoHelpInit()
	{
		PixelTools.reinit();
		coord = new Coordinates();
		coord.add(new Point(1920, 1080), "resDefault");
		coord.add(new Point(PixelTools.MaxX, PixelTools.MaxY), "resThis");
		outInfo("Init screen start", clear: true);
		object obj = null;
		if (Settings.Instance.PixelHunt)
		{
			outInfo("Sorry, Pixel hunt no longer supported", clear: true);
			return false;
		}
		GetZoomImageHuntOnMainScreen();
		coord.add(new Point(243, 908), "cudlikHorniDefault");
		coord.add(p1, "cudlikHorniThis");
		outInfo("Init screen done", clear: true);
		return true;
	}

	public void goHelpAndPub()
	{
		if (status != 1)
		{
			outInfo("Make init first!", clear: true);
			return;
		}
		outInfo("Going help players");
		coord.add(coord.get("cudlikHorniDefault"), "relatedPointFrom");
		coord.add(coord.get("cudlikHorniThis"), "relatedPointTo");
		Point[] relatedPoints = new Point[5]
		{
			new Point(356, 990),
			new Point(470, 990),
			new Point(584, 990),
			new Point(698, 990),
			new Point(811, 990)
		};
		Point[] pointsArray = coord.getPointsArray(relatedPoints);
		coord.add(pointsArray, "coordsKnajpaCheck");
		coord.add(new int[5, 2]
		{
			{ 112, 108 },
			{ 218, 108 },
			{ 325, 108 },
			{ 432, 108 },
			{ 539, 108 }
		}, "coordsHelp");
		coord.add(new Point(22, 50), "heroLineStart", coord.fromCudlik);
		coord.add(new Point(547, 50), "heroLineEnd", coord.fromCudlik);
		coord.add(new Point(709, 57), "pravaSipka", coord.fromCudlik);
		int[,] array = coord.getArray("coordsKnajpaCheck", coord.fromCudlik);
		int[,] array2 = coord.getArray("coordsHelp", coord.fromCudlik);
		ColorCheck colorCheck = new ColorCheck(PixelTools.GetColorAtDBM(coord.get("cudlikHorniThis")));
		bool flag = true;
		var (resizeX, resizeY) = ImageWorks.GetScreenZoomSettings();
		while (flag)
		{
			for (int i = 0; i < 5; i++)
			{
				Point p = coord.MoveAbsoluteRelative(coord.get("cudlikHorniThis"), new Point(14 + 114 * i, 0), coord.useZoom);
				Point q = coord.MoveAbsoluteRelative(p, new Point(114, 120), coord.useZoom);
				Bitmap val = PixelTools.CaptureScreen(p, q);
				if (DebugMode)
				{
					((Image)val).Save("c:\\mik\\srcdbg.png");
				}
				if (DoHelp)
				{
					if (ImageWorks.FindTemplateEmguExist(ImageWorks.getBmp("./images/help-fh.png", resizeX, resizeY, "./images/help-150.png"), val))
					{
						outInfo(" help " + i + "ok");
						Point randomizedPoint = coord.getRandomizedPoint(new Point(array2[i, 0] - 30 * (int)coord.zoomX, array2[i, 1]), 30 * (int)coord.zoomX, 4 * (int)coord.zoomY);
						MouseTools.ClickMouse(randomizedPoint);
						Wait(shortWait);
						if (!colorCheck.ClickUntilColor(coord.get("cudlikHorniThis"), randomizedPoint))
						{
							outInfo("help click timeout");
							return;
						}
					}
					else
					{
						outInfo(" help " + i + "not ok");
					}
				}
				if (DoPub && ImageWorks.FindTemplateEmguExist(ImageWorks.getBmp("./images/pub-fh.png", resizeX, resizeY, "./images/pub-150.png"), val))
				{
					Point randomizedPoint2 = coord.getRandomizedPoint(new Point(array[i, 0], array[i, 1]), 4 * (int)coord.zoomX, 4 * (int)coord.zoomY);
					MouseTools.ClickMouse(randomizedPoint2);
					Wait(shortWait);
					if (!colorCheck.ClickUntilColor(coord.get("cudlikHorniThis"), randomizedPoint2))
					{
						outInfo("pub click timeout");
						return;
					}
					outInfo(" tavern" + i + " ok");
					Wait();
				}
			}
			ColorCheck colorCheck2 = new ColorCheck(coord.get("heroLineStart"), coord.get("heroLineEnd"));
			MouseTools.ClickMouse(coord.get("pravaSipka").X, coord.get("pravaSipka").Y);
			Wait(2000);
			ColorCheck colCheck = new ColorCheck(coord.get("heroLineStart"), coord.get("heroLineEnd"));
			if (colorCheck2.CheckLine(colCheck))
			{
				flag = false;
			}
		}
		outInfo("End of help bar reached");
	}

	public void goHelpAndPubOld()
	{
		if (status != 1)
		{
			outInfo("Make init first!", clear: true);
			return;
		}
		outInfo("Going help players");
		coord.add(coord.get("cudlikHorniDefault"), "relatedPointFrom");
		coord.add(coord.get("cudlikHorniThis"), "relatedPointTo");
		Point[] relatedPoints = new Point[5]
		{
			new Point(356, 990),
			new Point(470, 990),
			new Point(584, 990),
			new Point(698, 990),
			new Point(811, 990)
		};
		Point[] pointsArray = coord.getPointsArray(relatedPoints);
		coord.add(pointsArray, "coordsKnajpaCheck");
		coord.add(new int[5, 2]
		{
			{ 112, 108 },
			{ 218, 108 },
			{ 325, 108 },
			{ 432, 108 },
			{ 539, 108 }
		}, "coordsHelp");
		coord.add(new Point(22, 50), "heroLineStart", coord.fromCudlik);
		coord.add(new Point(547, 50), "heroLineEnd", coord.fromCudlik);
		coord.add(new Point(709, 57), "pravaSipka", coord.fromCudlik);
		int[,] array = coord.getArray("coordsKnajpaCheck", coord.fromCudlik);
		int[,] array2 = coord.getArray("coordsHelp", coord.fromCudlik);
		Color color = PixelTools.StrColorToColor("94845D");
		Color colorAtDBM = PixelTools.GetColorAtDBM(coord.get("cudlikHorniThis"));
		ColorCheck colorCheck = new ColorCheck("331C0A", 5.0, posit: false);
		ColorCheck colorCheck2 = new ColorCheck(colorAtDBM);
		bool flag = false;
		bool flag2 = true;
		while (flag2)
		{
			for (int i = 0; i < 5; i++)
			{
				if (DoHelp)
				{
					if (colorCheck.Check(array2[i, 0], array2[i, 1]))
					{
						outInfo(" help " + i + "ok");
						Point randomizedPoint = coord.getRandomizedPoint(new Point(array2[i, 0] - 30 * (int)coord.zoomX, array2[i, 1]), 30 * (int)coord.zoomX, 4 * (int)coord.zoomY);
						MouseTools.ClickMouse(randomizedPoint);
						Wait(shortWait);
						if (!colorCheck2.ClickUntilColor(coord.get("cudlikHorniThis"), randomizedPoint))
						{
							outInfo("help click timeout");
							return;
						}
					}
					else
					{
						outInfo(" help " + i + "not ok");
					}
				}
				if (!DoPub)
				{
					continue;
				}
				Color colorAt = PixelTools.GetColorAt(new Point(array[i, 0], array[i, 1]));
				if (PixelTools.CompareColors(color, colorAt) || flag)
				{
					Point randomizedPoint2 = coord.getRandomizedPoint(new Point(array[i, 0], array[i, 1]), 4 * (int)coord.zoomX, 4 * (int)coord.zoomY);
					MouseTools.ClickMouse(randomizedPoint2);
					Wait(shortWait);
					if (!colorCheck2.ClickUntilColor(coord.get("cudlikHorniThis"), randomizedPoint2))
					{
						outInfo("pub click timeout");
						return;
					}
					outInfo(" tavern" + i + " ok");
					Wait();
				}
			}
			ColorCheck colorCheck3 = new ColorCheck(coord.get("heroLineStart"), coord.get("heroLineEnd"));
			MouseTools.ClickMouse(coord.get("pravaSipka").X, coord.get("pravaSipka").Y);
			Wait(2000);
			ColorCheck colCheck = new ColorCheck(coord.get("heroLineStart"), coord.get("heroLineEnd"));
			if (colorCheck3.CheckLine(colCheck))
			{
				flag2 = false;
			}
		}
		outInfo("End of help bar reached");
	}
}
