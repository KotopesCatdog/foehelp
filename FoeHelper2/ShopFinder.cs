using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tesseract;

namespace FoeHelper2;

internal class ShopFinder
{
	public static ShopGroup ActiveShopGroup;

	private Coordinates coord;

	private List<ShopGroup> shopGroups;

	private DirectBitmap shopItems;

	private DirectBitmap shopItems1;

	private DirectBitmap shopItems2;

	private Bitmap selectedOfferBmp;

	private Color posuvnikCheckNahoreThis;

	private Color posuvnikCheckDoleThis;

	private Color posuvnikCheckNahore;

	private Color posuvnikCheckDole;

	private Point p1;

	private Point p2;

	private Point pZero;

	private Rectangle[] radky;

	private Rectangle r;

	private Point[] iconStred;

	private float differenceThreshold = 0.05f;

	private int selectedEra;

	private bool jesteKousekDoluShopScrollbar;

	public ShopFinder(Coordinates c, int selEra)
	{
		coord = c;
		selectedEra = selEra;
		Init();
	}

	public void Init()
	{
		radky = new Rectangle[5];
		iconStred = new Point[5];
		shopGroups = new List<ShopGroup>();
		posuvnikCheckNahore = PixelTools.StrColorToColor("8692A8");
		posuvnikCheckDole = PixelTools.StrColorToColor("838FA5");
		pZero = new Point(0, 0);
		coord.add(new Point(-737, 221), "shopItemsStart", coord.fromCudlik);
		coord.add(new Point(13, 494), "shopItemsEnd", coord.fromCudlik);
		coord.add(new Point(15, 210), "shopPosuvnikTopLeft", coord.fromCudlik);
		coord.add(new Point(50, 500), "shopPosuvnikBotRight", coord.fromCudlik);
		coord.add(new Point(0, 0), "shopItemsStartCol1", coord.useZoom);
		coord.add(new Point(30, 278), "shopItemsEndCol1", coord.useZoom);
		coord.add(new Point(382, 0), "shopItemsStartCol2", coord.useZoom);
		coord.add(new Point(412, 278), "shopItemsEndCol2", coord.useZoom);
		coord.add(new Point(30, 240), "shopPosuvnikNahore", coord.useZoom);
		coord.add(new Point(31, 474), "shopPosuvnikDole", coord.useZoom);
		coord.add(new Point(28, 330), "shopPosuvnikStred", coord.useZoom);
		coord.add(new Point(307, -107), "shopRelativeCountsNahore1", coord.useZoom);
		coord.add(new Point(358, 24), "shopRelativeCountsDole1", coord.useZoom);
		coord.add(new Point(687, -107), "shopRelativeCountsNahore2", coord.useZoom);
		coord.add(new Point(745, 24), "shopRelativeCountsDole2", coord.useZoom);
		radky[0] = coord.getRectangle(Rectangle.FromLTRB(3, 4, 58, 18), coord.useZoom);
		radky[1] = coord.getRectangle(Rectangle.FromLTRB(3, 31, 58, 46), coord.useZoom);
		radky[2] = coord.getRectangle(Rectangle.FromLTRB(3, 58, 58, 73), coord.useZoom);
		radky[3] = coord.getRectangle(Rectangle.FromLTRB(3, 85, 58, 100), coord.useZoom);
		radky[4] = coord.getRectangle(Rectangle.FromLTRB(3, 112, 58, 127), coord.useZoom);
		iconStred[4] = coord.getMod(new Point(13, 12), coord.useZoom);
		iconStred[3] = coord.getMod(new Point(13, -18), coord.useZoom);
		iconStred[2] = coord.getMod(new Point(13, -45), coord.useZoom);
		iconStred[1] = coord.getMod(new Point(13, -71), coord.useZoom);
		iconStred[0] = coord.getMod(new Point(13, -98), coord.useZoom);
		coord.add(new Point(50, 543), "shopZboziEnd", coord.fromCudlik);
	}

	private void debugShopGroupsIcons()
	{
		foreach (ShopGroup shopGroup in shopGroups)
		{
			shopGroup.debugIcon();
		}
	}

	private string kdePosuvnik()
	{
		Point p = coord.get("shopPosuvnikTopLeft");
		Point q = coord.get("shopPosuvnikBotRight");
		Bitmap val = PixelTools.CaptureScreen(p, q);
		Bitmap bmp = ImageWorks.getBmp("./images/shopscrolltop.png", coord.zoomX, coord.zoomY);
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			return "nahore";
		}
		bmp = ImageWorks.getBmp("./images/shopscrollbartop2.png");
		ImageWorks.savePngDebug(val);
		ImageWorks.savePngDebug(bmp, "sb");
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			return "nahore";
		}
		bmp = ImageWorks.getBmp("./images/shopscrollbot.png", coord.zoomX, coord.zoomY);
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			if (jesteKousekDoluShopScrollbar)
			{
				return "dole";
			}
			jesteKousekDoluShopScrollbar = true;
		}
		bmp = ImageWorks.getBmp("./images/shopscrollbarbot2.png");
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			if (jesteKousekDoluShopScrollbar)
			{
				return "dole";
			}
			jesteKousekDoluShopScrollbar = true;
		}
		return "mezi";
	}

	private void jedPosuvnik(string kam)
	{
		MouseTools.MoveMouse(coord.getRelated("shopPosuvnikStred", coord.get("relatedPointTo")));
		if (kam == "up")
		{
			MouseTools.DoMouseWheel("up");
		}
		else
		{
			MouseTools.DoMouseWheel();
		}
		FoeTools.Wait(100);
	}

	public static int endEraGoods(int era, int goodPosition)
	{
		if (era == FoeTools.MaxEras - 1)
		{
			goodPosition--;
		}
		if (era == FoeTools.MaxEras)
		{
			goodPosition += 2;
		}
		return goodPosition;
	}

	private bool haveShopIcon(bool goFull = true)
	{
		bool result = true;
		shopItems = PixelTools.CaptureRegionDBM(coord.get("shopItemsStart"), coord.get("shopItemsEnd"));
		foreach (ShopGroup shopGroup in shopGroups)
		{
			if (shopGroup.Found)
			{
				continue;
			}
			r = PixelTools.RectangleFromPoints(coord.get("shopItemsStartCol1"), coord.get("shopItemsEndCol1"));
			shopItems1 = PixelTools.cropAtRect(shopItems, r);
			r = PixelTools.RectangleFromPoints(coord.get("shopItemsStartCol2"), coord.get("shopItemsEndCol2"));
			shopItems2 = PixelTools.cropAtRect(shopItems, r);
			shopGroup.findStartBmp(shopItems1, shopItems2, (int)(137f * coord.zoomY));
			if (shopGroup.Found)
			{
				int x;
				int y;
				if (shopGroup.FoundBmpNum == 1)
				{
					p1 = coord.MoveRelative(coord.get("shopRelativeCountsNahore1"), shopGroup.StartPoint);
					p2 = coord.MoveRelative(coord.get("shopRelativeCountsDole1"), shopGroup.StartPoint);
					x = shopGroup.StartPoint.X + coord.get("shopItemsStartCol1").X + coord.get("shopItemsStart").X;
					y = shopGroup.StartPoint.Y + coord.get("shopItemsStartCol1").Y + coord.get("shopItemsStart").Y;
				}
				else
				{
					p1 = coord.MoveRelative(coord.get("shopRelativeCountsNahore2"), shopGroup.StartPoint);
					p2 = coord.MoveRelative(coord.get("shopRelativeCountsDole2"), shopGroup.StartPoint);
					x = shopGroup.StartPoint.X + coord.get("shopItemsStartCol2").X + coord.get("shopItemsStart").X;
					y = shopGroup.StartPoint.Y + coord.get("shopItemsStartCol2").Y + coord.get("shopItemsStart").Y;
				}
				shopGroup.LastAbsolute = new Point(x, y);
				ActiveShopGroup = shopGroup;
				if (!goFull)
				{
					continue;
				}
				r = PixelTools.RectangleFromPoints(p1, p2);
				shopGroup.countsPic = PixelTools.cropAtRect(shopItems.Bitmap, r);
				Bitmap countsPic = shopGroup.countsPic;
				if (FoeTools.DebugMode)
				{
					((Image)countsPic).Save("counts.png");
				}
				string text = "";
				for (int i = 0; i < 5; i++)
				{
					Bitmap val = PixelTools.cropAtRect(countsPic, radky[i]);
					if (FoeTools.DebugMode)
					{
						((Image)val).Save($"foundradek{shopGroup.Num}-{i}.png");
					}
					text = FoeOCR.ReadNumbersCharles(val, 1, modif2: false, "", "1234567890", (PageSegMode)6);
					shopGroup.addCount(i, text);
					shopGroup.RadekAbs[i] = coord.MoveRelative(shopGroup.LastAbsolute, iconStred[i]);
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	public void rescan()
	{
		foreach (ShopGroup shopGroup in shopGroups)
		{
			shopGroup.Found = false;
		}
		loadShopOverView(goFull: false);
	}

	public bool loadShopOverView(bool goFull = true, bool setDemandCheckBoxes = true, bool setSupplyCheckBoxes = true)
	{
		bool flag = false;
		if (false)
		{
			coord.add(new Point(-830, 816), "shopDebugPoint", coord.fromCudlik);
			PixelTools.CaptureRegion(coord.get("shopDebugPoint"), coord.get("shopHorniThis"), save: true);
		}
		if (goFull && selectedEra > 0)
		{
			shopGroups = new List<ShopGroup>();
			shopGroups.Add(new ShopGroup(1));
			shopGroups[0].addBmpFile($"./images/b{selectedEra}.png", coord.zoomX, coord.zoomY);
		}
		flag = haveShopIcon(goFull);
		if (FoeTools.DebugMode && !flag)
		{
			flag = haveShopIcon(goFull);
		}
		if (!flag)
		{
			jesteKousekDoluShopScrollbar = false;
			for (int i = 1; i < 100; i++)
			{
				if (kdePosuvnik() == "nahore")
				{
					jedPosuvnik("up");
					jedPosuvnik("up");
					break;
				}
				jedPosuvnik("up");
			}
			for (int j = 1; j < 100; j++)
			{
				flag = haveShopIcon(goFull);
				if (flag)
				{
					break;
				}
				if (kdePosuvnik() == "dole")
				{
					FoeTools.outInfo("shop scrollbar is at bottom");
					break;
				}
				jedPosuvnik("down");
			}
		}
		if (flag && goFull)
		{
			double num = ActiveShopGroup.Counts.DefaultIfEmpty(0).Average((int x) => x);
			double num2 = ActiveShopGroup.Counts.DefaultIfEmpty(0).Min();
			if (FoeTools.offersMultiplier < 1)
			{
				FoeTools.offersMultiplier = 1;
			}
			double num3 = ((double)(20 + (selectedEra / 2 - 1) * 5) + Math.Pow(selectedEra, 1.5)) * (double)FoeTools.offersMultiplier + (double)FoeTools.random.Next(10);
			num3 = Math.Round(num3 / 5.0) * 5.0;
			if (num3 * 8.0 >= num2)
			{
				num3 = num2 / 8.0 - 10.0;
				num3 = Math.Round(num3 / 5.0) * 5.0;
			}
			if (FoeTools.shoppingMode == 0)
			{
				if (num3 * 8.0 < num2)
				{
					Settings.Instance.DemandVol = ((int)(2.0 * num3)).ToString() ?? "";
				}
				if (num3 * 8.0 < num2)
				{
					Settings.Instance.SupplyVol = ((int)num3).ToString() ?? "";
				}
			}
			else
			{
				if (num3 * 8.0 < num2)
				{
					Settings.Instance.DemandVol = ((int)(2.0 * num3)).ToString() ?? "";
				}
				if (num3 * 8.0 < num2)
				{
					Settings.Instance.SupplyVol = ((int)(2.0 * num3)).ToString() ?? "";
				}
			}
			for (int k = 0; k < 5; k++)
			{
				int num4 = ActiveShopGroup.Counts[k];
				Settings.Instance.ShopCountInfo[k] = num4.ToString() ?? "";
				Settings.Instance.ShopCheckBox[k] = true;
				Settings.Instance.ShopCheckBox[k + 5] = true;
				if (setDemandCheckBoxes && (double)num4 - num > (double)differenceThreshold * num)
				{
					Settings.Instance.ShopCheckBox[k + 5] = false;
				}
				if (setSupplyCheckBoxes && num - (double)num4 > (double)differenceThreshold * num)
				{
					Settings.Instance.ShopCheckBox[k] = false;
				}
			}
		}
		return flag;
	}

	public void shopSetOfferAndDemandNoWheel(int reqS, int reqD, int actS, int actD)
	{
		List<Color> list = new List<Color>();
		list.Add(Color.FromArgb(255, 39, 23, 10));
		list.Add(Color.FromArgb(255, 40, 24, 14));
		list.Add(Color.FromArgb(255, 34, 18, 2));
		Point p = new Point(18, -11);
		Point p2 = new Point(129, -12);
		Point p3 = new Point(127, 10);
		if (reqS != actS && reqS >= 0)
		{
			Point randomizedPoint = coord.getRandomizedPoint(ActiveShopGroup.RadekAbs[reqS], 8);
			Point p4 = coord.MoveRelative(p, randomizedPoint, coord.useZoom);
			Color colorAt = PixelTools.GetColorAt(p4.X, p4.Y);
			MouseTools.ClickMouse(randomizedPoint);
			ColorCheck colorCheck = new ColorCheck(list, 1.1);
			if (!colorCheck.WaitForColor(p4, 10, 1, 1))
			{
				MouseTools.MoveMouse(p4);
				FoeTools.outInfo("setting supply failed, check your resolution is 1080p and no zoom");
				FoeTools.WriteLogFile($"setting supply failed {p4.X} {p4.Y} {colorCheck.getLastFound()}");
				throw new Exception("shop error");
			}
			Point p5 = coord.MoveRelative(p2, randomizedPoint, coord.useZoom);
			FoeTools.Wait(200);
			MouseTools.ClickMouse(coord.getRandomizedPoint(p5, 10, 5));
			colorCheck = new ColorCheck(colorAt);
			if (!colorCheck.WaitForColor(p4))
			{
				FoeTools.outInfo("setting supply after failed, check your resolution is 1080p and no zoom");
				throw new Exception("shop error");
			}
			FoeTools.Wait(500);
		}
		if (reqD != actD && reqD >= 0)
		{
			Point randomizedPoint = coord.getRandomizedPoint(ActiveShopGroup.RadekAbs[reqD], 8);
			Point p4 = coord.MoveRelative(p, randomizedPoint, coord.useZoom);
			Color colorAt2 = PixelTools.GetColorAt(p4.X, p4.Y);
			FoeTools.Wait(200);
			MouseTools.ClickMouse(randomizedPoint);
			ColorCheck colorCheck = new ColorCheck(list, 1.1);
			if (!colorCheck.WaitForColor(p4, 10, 1, 1))
			{
				FoeTools.outInfo("setting demand failed, check your resolution is 1080p and no zoom");
				throw new Exception("shop error");
			}
			Point p5 = coord.MoveRelative(p3, randomizedPoint, coord.useZoom);
			MouseTools.ClickMouse(coord.getRandomizedPoint(p5, 10, 5));
			colorCheck = new ColorCheck(colorAt2);
			if (!colorCheck.WaitForColor(p4))
			{
				FoeTools.outInfo("setting demand after failed, check your resolution is 1080p and no zoom");
				throw new Exception("shop error");
			}
			FoeTools.Wait(500);
		}
	}
}
