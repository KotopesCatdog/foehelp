using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Tesseract;

namespace FoeHelper2;

internal class GB
{
	private readonly TextBox _outputTextBlock;

	private readonly Dispatcher _dispatcher;

	private Bitmap gbWindw;

	private Bitmap fpBmp;

	private Point levyHorni;

	private Point pravyDolni;

	private Point cudlik;

	public TextBox OutputTextBox => _outputTextBlock;

	public GB(TextBox outputTextBox, Dispatcher dispatcher)
	{
		_outputTextBlock = outputTextBox;
		_dispatcher = dispatcher;
	}

	public static int MRound(double value, int factor)
	{
		return (int)(Math.Round(value / (double)factor) * (double)factor);
	}

	public static List<int> GenerateReward(int firstRankReward)
	{
		List<int> obj = new List<int> { firstRankReward };
		obj.Add(MRound((double)obj[obj.Count - 1] / 2.0, 5));
		obj.Add(MRound((double)obj[obj.Count - 1] / 3.0, 5));
		obj.Add(MRound((double)obj[obj.Count - 1] / 4.0, 5));
		obj.Add(MRound((double)obj[obj.Count - 1] / 5.0, 5));
		return obj;
	}

	public void AppendGBText(string message)
	{
		_dispatcher.BeginInvoke((DispatcherPriority)4, (Delegate)(Action)delegate
		{
			((TextBoxBase)_outputTextBlock).AppendText(message + Environment.NewLine);
			_outputTextBlock.CaretIndex = _outputTextBlock.Text.Length;
			((TextBoxBase)_outputTextBlock).ScrollToEnd();
		});
	}

	public void SetGBText(string message)
	{
		_dispatcher.BeginInvoke((DispatcherPriority)4, (Delegate)(Action)delegate
		{
			((TextBoxBase)_outputTextBlock).AppendText(message + Environment.NewLine);
			_outputTextBlock.CaretIndex = _outputTextBlock.Text.Length;
			((TextBoxBase)_outputTextBlock).ScrollToEnd();
		});
	}

	public void ClearGBText()
	{
		if (_dispatcher.CheckAccess())
		{
			_outputTextBlock.Clear();
			return;
		}
		_dispatcher.Invoke((Action)delegate
		{
			_outputTextBlock.Clear();
		});
	}

	public bool GoGbInit()
	{
		FoeTools.coord = new Coordinates();
		FoeTools.outInfo("gb init started, please wait..");
		PixelTools.reinit();
		PixelTools.setAcceptableColorPrecision();
		var (resizeX, resizeY) = ImageWorks.GetScreenZoomSettings();
		FoeTools.coord.zoomX = 1f;
		FoeTools.coord.zoomY = 1f;
		if (Settings.Instance.CacheZoomSettings)
		{
			if (Settings.Instance.ZoomSettings[0] != 0f && Settings.Instance.ZoomSettings[1] != 0f)
			{
				FoeTools.coord.zoomX = Settings.Instance.ZoomSettings[0];
				FoeTools.coord.zoomY = Settings.Instance.ZoomSettings[1];
			}
			else
			{
				FoeTools.GetZoomImageHuntOnMainScreen();
				Settings.Instance.ZoomSettings[0] = FoeTools.coord.zoomX;
				Settings.Instance.ZoomSettings[1] = FoeTools.coord.zoomY;
				Settings.Instance.SaveToXml();
			}
		}
		else
		{
			FoeTools.GetZoomImageHuntOnMainScreen();
		}
		float relativeResolutionX = FoeTools.coord.getRelativeResolutionX();
		float relativeResolutionY = FoeTools.coord.getRelativeResolutionY();
		int x = (int)(relativeResolutionX * 1043f);
		int y = (int)(relativeResolutionY * 169f);
		int width = (int)(relativeResolutionX * 558f);
		int height = (int)(relativeResolutionY * 285f);
		Rectangle r = new Rectangle(x, y, width, height);
		Bitmap sourceBitmap = PixelTools.CaptureScreenRect(r);
		Bitmap bmp = ImageWorks.getBmp("./images/battle-init-fh.png", resizeX, resizeY, "./images/battle-init150.png");
		bool flag = false;
		if (ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp))
		{
			flag = true;
			Point lastFound = ImageWorks.LastFound;
			FoeTools.p1 = new Point(lastFound.X + r.X, lastFound.Y + r.Y);
		}
		if (!flag)
		{
			FoeTools.outInfo("error #12, make sure GB window is opened");
			return false;
		}
		Point p = FoeTools.p1;
		FoeTools.coord.add(p, "relatedPointTo");
		FoeTools.coord.add(new Point(-757, 504), "dbLevyDolni", FoeTools.coord.fromCudlik);
		Point point = FoeTools.coord.get("dbLevyDolni");
		levyHorni = new Point(point.X, p.Y);
		pravyDolni = new Point(p.X, point.Y);
		_ = pravyDolni.X;
		_ = levyHorni.X;
		_ = pravyDolni.Y;
		_ = levyHorni.Y;
		gbWindw = PixelTools.CaptureScreen(levyHorni, pravyDolni);
		if (!CheckCorrectWindow())
		{
			FoeTools.outInfo("GB window not opened - please open some GB first");
			return false;
		}
		FoeTools.outInfo("GB init done");
		Settings.Instance.GBEnabled = true;
		return true;
	}

	public void addContrib(int place, int contrib, bool isOwner)
	{
		Settings.Instance.GbInvestor[place] = contrib;
		Settings.Instance.GbInvestorOwner[place] = isOwner;
	}

	public int getManualContribution(int place, int contrib)
	{
		if (Settings.Instance.OverrideInvestors)
		{
			int num = Settings.Instance.GbInvestor[place];
			if (num > 0)
			{
				contrib = num;
			}
		}
		return contrib;
	}

	private void SelectLastSecureLine()
	{
		_dispatcher.BeginInvoke((DispatcherPriority)2, (Delegate)(Action)delegate
		{
			Window mainWindow = Application.Current.MainWindow;
			if (mainWindow != null)
			{
				if (!mainWindow.IsActive)
				{
					mainWindow.Activate();
				}
				string text = _outputTextBlock.Text;
				if (!string.IsNullOrEmpty(text))
				{
					string[] array = text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
					int num = -1;
					for (int num2 = array.Length - 1; num2 >= 0; num2--)
					{
						if (array[num2].StartsWith("✅"))
						{
							num = num2;
							break;
						}
					}
					if (num != -1)
					{
						int num3 = 0;
						for (int i = 0; i < num; i++)
						{
							num3 += array[i].Length + Environment.NewLine.Length;
						}
						int length = array[num].Length;
						((UIElement)_outputTextBlock).Focus();
						Keyboard.Focus((IInputElement)(object)_outputTextBlock);
						_outputTextBlock.Select(num3, length);
						((TextBoxBase)_outputTextBlock).ScrollToEnd();
					}
				}
			}
		});
	}

	public static int CleanOcrReward(string ocrText, int gbTotalCost)
	{
		if (string.IsNullOrWhiteSpace(ocrText) || gbTotalCost <= 0)
		{
			return 0;
		}
		string text = new string(ocrText.Where(char.IsDigit).ToArray());
		if (string.IsNullOrEmpty(text))
		{
			return 0;
		}
		if (text.Length > 4)
		{
			text = text.Substring(0, 4);
		}
		long num = long.Parse(text);
		double num2 = (double)gbTotalCost * 0.4;
		while ((double)num > num2 && text.Length > 1)
		{
			text = text.Substring(0, text.Length - 1);
			num = long.Parse(text);
		}
		return (int)num;
	}

	public bool CheckCorrectWindow()
	{
		fpBmp = ImageWorks.getBmp("./images/fp.png", FoeTools.coord.zoomX, FoeTools.coord.zoomY);
		Bitmap templateBitmap = PixelTools.cropAtRect(gbWindw, FoeTools.coord.getRectangle(287, 89, 98, 50, FoeTools.coord.zoomCoord));
		if (!ImageWorks.FindTemplateEmguExist(fpBmp, templateBitmap))
		{
			return false;
		}
		return true;
	}

	public void GoGBrun()
	{
		Point point;
		try
		{
			point = FoeTools.coord.get("dbLevyDolni");
		}
		catch
		{
			FoeTools.outInfo("please init first");
			return;
		}
		cudlik = FoeTools.coord.get("relatedPointTo");
		levyHorni = new Point(point.X, cudlik.Y);
		pravyDolni = new Point(cudlik.X, point.Y);
		int num = pravyDolni.X - levyHorni.X;
		int num2 = pravyDolni.Y - levyHorni.Y;
		gbWindw = PixelTools.CaptureScreen(levyHorni, pravyDolni);
		if (!CheckCorrectWindow())
		{
			return;
		}
		bool flag = false;
		string input = FoeOCR.ReadNumbersCharles(PixelTools.cropAtRect(gbWindw, FoeTools.coord.getRectangle(328, 132, 106, 27, FoeTools.coord.zoomCoord)), 2, modif2: false, "", "1/234567890", (PageSegMode)6);
		input = Regex.Replace(input, "\\s+", "");
		string[] array = input.Split(new char[1] { '/' });
		_ = new int[2];
		if (array.Length != 2)
		{
			flag = true;
		}
		int totalFpOnGb = 0;
		int num3 = 0;
		try
		{
			totalFpOnGb = int.Parse(array[0]);
			num3 = int.Parse(array[1]);
		}
		catch (Exception)
		{
			flag = true;
		}
		if (flag)
		{
			if (!(Settings.Instance.GBProgress != ""))
			{
				FoeTools.outInfo("gb progress reading failed, enter progress manualy in format fpIvested/fpTotal");
				return;
			}
			FoeTools.outInfo("gb progress reading failed, using previous values");
			input = Regex.Replace(Settings.Instance.GBProgress, "\\s+", "");
			array = input.Split(new char[1] { '/' });
			try
			{
				totalFpOnGb = int.Parse(array[0]);
				num3 = int.Parse(array[1]);
			}
			catch (Exception)
			{
				FoeTools.outInfo("gb progress reading failed, invalid progress values entered");
				return;
			}
		}
		Settings.Instance.GBProgress = input;
		int[] array2 = new int[6] { 315, 347, 378, 410, 442, 475 };
		int[] array3 = new int[6];
		int[] array4 = new int[6];
		int num4 = 0;
		FoeSniper.ResetContributors();
		for (int i = 0; i < array2.Length; i++)
		{
			Point point2 = default(Point);
			point2.Y = array2[i];
			point2.X = 4;
			num = 33;
			num2 = 28;
			Rectangle rectangle = FoeTools.coord.getRectangle(point2.X, point2.Y, num, num2, FoeTools.coord.zoomCoord);
			input = FoeOCR.ReadNumbersCharles(PixelTools.cropAtRect(gbWindw, rectangle), 1, modif2: false, "", "1234567890", (PageSegMode)6);
			int num5 = 0;
			if (input != "")
			{
				try
				{
					num5 = (array3[i] = int.Parse(input));
				}
				catch (Exception)
				{
					FoeTools.outInfo("unable to read gb position");
				}
			}
			point2.Y = array2[i];
			point2.X = 572;
			num = 66;
			num2 = 28;
			rectangle = FoeTools.coord.getRectangle(point2.X, point2.Y, num, num2, FoeTools.coord.zoomCoord);
			input = FoeOCR.ReadNumbersCharles(PixelTools.cropAtRect(gbWindw, rectangle), 1, modif2: false, "", "1234567890", (PageSegMode)6);
			int num6 = 0;
			if (input != "")
			{
				input = input.Replace("\n", "");
				input = input.Replace(" ", "");
				try
				{
					num6 = int.Parse(input);
					if (input[0].Equals('8') && num6 > num3)
					{
						num6 = int.Parse("3" + input.Substring(1));
					}
					array4[i] = num6;
				}
				catch (Exception)
				{
					FoeTools.outInfo("unable to read gb contribution");
					return;
				}
			}
			if (num4 == 0)
			{
				num4 = array2[i];
				if (num6 > 0)
				{
					num6 = getManualContribution(num5, num6);
					if (num5 == 0)
					{
						FoeSniper.AddContributor("", isOwner: true, num6);
						addContrib(num5, num6, isOwner: true);
						num4 = array2[i + 1];
					}
					else
					{
						FoeSniper.AddContributor("", isOwner: false, num6);
						addContrib(num5, num6, isOwner: false);
					}
				}
			}
			else if (num6 > 0)
			{
				num6 = getManualContribution(num5, num6);
				if (num5 == 0)
				{
					addContrib(num5, num6, isOwner: true);
					FoeSniper.AddContributor("", isOwner: true, num6);
				}
				else
				{
					addContrib(num5, num6, isOwner: false);
					FoeSniper.AddContributor("", isOwner: false, num6);
				}
			}
		}
		FoeTools.outInfo("contributions reading finished");
		int num7 = 0;
		if (Settings.Instance.GBRewardCache)
		{
			int reward = 0;
			GbRewardCache.TryGetReward(num3, out reward);
			if (reward != 0)
			{
				num7 = reward;
				FoeTools.outInfo("using cached base reward");
			}
		}
		if (num7 == 0)
		{
			int x = FoeTools.coord.MoveRelative(new Point(-46, 0), cudlik, FoeTools.coord.useZoom).X;
			num4 = FoeTools.coord.getMod(0, num4, FoeTools.coord.useZoom).Y;
			num4 = cudlik.Y + num4;
			num4 = FoeTools.coord.MoveRelativeY(num4, 15, FoeTools.coord.useZoom);
			Point p = new Point(x, num4);
			p = FoeTools.coord.getRandomizedPoint(p, 10, 10);
			MouseTools.MoveMouse(p);
			Point p2 = FoeTools.coord.MoveRelative(new Point(-15, -106), p, FoeTools.coord.useZoom);
			Point q = FoeTools.coord.MoveRelative(new Point(14, -80), p, FoeTools.coord.useZoom);
			for (int j = 0; j < 4; j++)
			{
				FoeTools.Wait(500);
				Bitmap templateBitmap = PixelTools.CaptureScreen(p2, q);
				Point? point3 = ImageWorks.FindTemplateEmgu(fpBmp, templateBitmap);
				if (point3.HasValue)
				{
					Point q2 = new Point(point3.Value.X + p2.X, point3.Value.Y + p2.Y);
					Point p3 = FoeTools.coord.MoveRelative(new Point(35, -5), q2, FoeTools.coord.useZoom);
					Point q3 = FoeTools.coord.MoveRelative(new Point(65, 15), q2, FoeTools.coord.useZoom);
					input = FoeOCR.ReadNumbersCharles(PixelTools.CaptureScreen(p3, q3), 1, modif2: false, "", "+1234567890", (PageSegMode)7);
					input = input.Replace("\r", "").Replace("\n", "").Trim();
					input = input.Split(new char[2] { ' ', '\t' }, 2)[0];
					if (input[0] == '+')
					{
						input = input.Substring(1);
					}
					num7 = CleanOcrReward(input, num3);
					if (num7 == 0)
					{
						FoeTools.outInfo("unable to read base reward correctly");
						break;
					}
				}
			}
			if (num7 == 0)
			{
				FoeTools.outInfo("rewards reading failed, using prev values");
				try
				{
					num7 = int.Parse(Settings.Instance.GBReward1);
				}
				catch (Exception)
				{
					FoeTools.outInfo("rewards prev values invalid, reenter");
					return;
				}
			}
		}
		GbRewardCache.AddOrUpdate(num3, num7);
		Settings.Instance.gbManualEntered = false;
		Settings.Instance.GBReward1 = num7.ToString();
		Settings.Instance.gbManualEntered = true;
		List<int> list = GenerateReward(num7);
		List<string> list2 = new List<string>();
		new StringBuilder();
		bool gBsecureMode = Settings.Instance.GBsecureMode;
		bool gBsnipingMode = Settings.Instance.GBsnipingMode;
		int level = 0;
		string text = "";
		if (false)
		{
			text = FoeOCR.ReadTextCharles(PixelTools.cropAtRect(r: FoeTools.coord.getRectangle(150, 0, 450, 20, FoeTools.coord.zoomCoord), b: gbWindw));
			text = text.Replace("\r", "").Replace("\n", "").Trim();
		}
		if (!string.IsNullOrEmpty(text))
		{
			FoeTools.outInfo("GB Name read as: " + text);
		}
		FoeSniper.GbCost = num3;
		FoeSniper.TotalFpOnGb = totalFpOnGb;
		FoeSniper.ArcMulti = 1.9;
		FoeSniper.BaseRewards = list.ToArray();
		ClearGBText();
		if (gBsecureMode)
		{
			try
			{
				list2.AddRange(FoeSniper.GetSecuredSpots(text, level, out var secureTextOut));
				if (!string.IsNullOrEmpty(secureTextOut))
				{
					ClipboardHelper.CopyToClipboard("✅ " + secureTextOut, _outputTextBlock);
					SelectLastSecureLine();
				}
			}
			catch (Exception ex6)
			{
				list2.Add("❌ Secure mode error: " + ex6.Message);
			}
		}
		if (gBsnipingMode)
		{
			try
			{
				list2.AddRange(FoeSniper.GetBestSnipingOpportunity());
			}
			catch (Exception ex7)
			{
				FoeTools.outInfo("❌ Sniping mode failed: " + ex7.Message);
			}
		}
		foreach (string item in list2)
		{
			AppendGBText(item);
		}
	}
}
