using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Tesseract;

namespace FoeHelper2;

internal class FoeBattle
{
	private Bitmap poharBmp;

	private static FoeSectors foeSectors;

	private static Random random = new Random();

	private int LastMaxBattleScore;

	private int LastBattleTotal;

	private const int unitsCnt = 8;

	private Point unitDimension = new Point(50, 50);

	private Point unitHalfDimension = new Point(35, 35);

	private Point unitIconDimension = new Point(48, 25);

	internal Point battleScrTopLeft;

	internal Point battleScrBotRight;

	internal Point patternMatchTopLeft;

	internal Point patternMatchBotRight;

	private static Point currentSectorBattle;

	private List<FoeUnit> foeUnits;

	private List<FoeUnit> foeUnitsActual;

	public static bool tooManyLosses;

	private string battleEndMessage;

	public int fewBattlesRemain;

	private int debugCntr;

	public int lockedUnitsCnt;

	private bool stopFighting;

	private Stopwatch rescueStopWatch = new Stopwatch();

	private int lossesInRow;

	private DirectBitmap wrongUnit16;

	private DirectBitmap wrongUnit17;

	private DirectBitmap inventoryRegionBmp;

	private List<string> actionLog;

	private List<string> resultsLog;

	private Point blankPoint = new Point(0, 0);

	public int MinHealthAllowed { get; set; }

	private Coordinates coord => FoeTools.coord;

	public FoeBattle(bool initBattleScreen = true, bool quantum = false)
	{
		if (initBattleScreen)
		{
			poharBmp = ImageWorks.getBmp("./images/pohar.png", coord.zoomX, coord.zoomY);
			battleScrTopLeft = new Point(coord.get("ramecekTopleft").X, coord.get("ramecekTopleft").Y + 240);
			if (quantum)
			{
				battleScrBotRight = new Point(coord.get("ramecekBotright").X - 50, coord.get("ramecekBotright").Y + 50);
			}
			else
			{
				battleScrBotRight = new Point(coord.get("ramecekBotright").X - 50, coord.get("ramecekBotright").Y);
			}
		}
		initActionLog();
	}

	public static bool find20(FoeTools tools)
	{
		bool flag = false;
		if (foeSectors == null)
		{
			foeSectors = new FoeSectors();
		}
		Bitmap val = PixelTools.CaptureScreen();
		(float scalingFactorX, float scalingFactorY) screenZoomSettings = ImageWorks.GetScreenZoomSettings();
		float item = screenZoomSettings.scalingFactorX;
		float item2 = screenZoomSettings.scalingFactorY;
		Bitmap val2 = ((!((double)item >= 1.5) || !((double)item <= 1.6)) ? ImageWorks.getBmp("./images/20.png", item, item2) : ImageWorks.getBmp("./images/20-150.png"));
		List<Point> points = ImageWorks.FindAllTemplateMatchesEmgu(val, val2);
		points = FoeSectors.FilterUniquePoints(points);
		((Image)val).Dispose();
		((Image)val2).Dispose();
		if (points.Count > 0)
		{
			FoeTools.outInfo("20% sectors found");
			foreach (Point item3 in points)
			{
				if (foeSectors.SectorExists(item3))
				{
					if (!foeSectors.SectorIsFightable(item3))
					{
						continue;
					}
				}
				else
				{
					foeSectors.OpenFightScreen(item3);
					if (!FoeTools.fightInitDone)
					{
						foeSectors.StartFight();
						tools.GoFightInit();
						if (!FoeTools.fightInitDone)
						{
							FoeTools.outInfo("error, fight init failed");
							return false;
						}
						Settings.Instance.FightEnabled = true;
						foeSectors.CloseStartFightScreen();
					}
					int[] battleScores = foeSectors.getBattleScores();
					foeSectors.AddSector(item3, battleScores[1], battleScores[0]);
					if (!foeSectors.SectorIsFightable(item3))
					{
						foeSectors.CloseFightScreen();
						continue;
					}
				}
				foeSectors.StartFight();
				currentSectorBattle = item3;
				FoeTools.Wait(2500);
				flag = true;
				break;
			}
			if (!flag)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	private void doStopSurrender(bool isSurrender = false)
	{
		if (isSurrender)
		{
			MouseTools.ClickMouse(coord.getRandomizedPoint("surrenderCancel", 20, 2));
		}
		else
		{
			SendKeys.SendWait("{ESC}");
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " ESC pressed stop surrender");
		}
		FoeTools.Wait(200);
		resultsLog.Add("onHold");
	}

	public int getUnitsInit()
	{
		foeUnits = new List<FoeUnit>();
		return getUnits(foeUnits);
	}

	public void dumpUnits(List<FoeUnit> unitsList, string cap)
	{
		if (FoeTools.UnitsDebug)
		{
			FoeTools.ClearLogFile("c:\\mik\\foeunit" + cap + "log.txt");
			for (int i = 1; i <= 8; i++)
			{
				((Image)getUnit(i, unitsList).getDBM().Bitmap).Save($"c:\\mik\\foeunit{cap}{i}.png");
				FoeTools.WriteLogFile(getUnit(i, unitsList).ToString() + "\n", "c:\\mik\\foeunit" + cap + "log.txt");
			}
		}
	}

	public int getUnits(List<FoeUnit> unitsList, bool actual = false)
	{
		int num = 0;
		for (int i = 1; i <= 8; i++)
		{
			Point point = coord.get("battleUnit" + i);
			Point center = coord.MoveRelative(unitHalfDimension, point, coord.useZoom);
			Point point2 = coord.MoveRelative(new Point(2, 0), point, coord.useZoom);
			Point point3 = coord.MoveRelative(unitIconDimension, point2, coord.useZoom);
			if (actual)
			{
				point2 = coord.MoveRelative(new Point(-2, -2), point2, coord.useZoom);
				point3 = coord.MoveRelative(new Point(2, 2), point3, coord.useZoom);
			}
			DirectBitmap bmp = PixelTools.CaptureRegionDBM(point2, point3);
			unitsList.Add(new FoeUnit(bmp, i, center));
			getUnit(i, unitsList).AddHealthBar2(getHealthBar(point));
			if (FoeTools.DebugMode)
			{
				((Image)getUnit(i, unitsList).GetHealthBar()).Save($"c:\\mik\\unit{i}hb.png");
				((Image)getUnit(i, unitsList).GetUnitBmp()).Save($"c:\\mik\\unit{i}.png");
			}
			if (getUnit(i, unitsList).Health < 1)
			{
				getUnit(i, unitsList).Empty = true;
			}
			else
			{
				num++;
			}
		}
		lockedUnitsCnt = num;
		return num;
	}

	private DirectBitmap getHealthBar(Point p1)
	{
		Point a = coord.MoveRelative(new Point(0, 52), p1, coord.useZoom);
		Point b = coord.MoveRelative(new Point(52, 59), p1, coord.useZoom);
		return PixelTools.CaptureRegionDBM(a, b);
	}

	public bool checkWholeInv()
	{
		for (int i = 0; i < 18; i++)
		{
			if (checkInventory(i, testWrongUnits: false))
			{
				i--;
			}
			if (countMissingUnits() < 1)
			{
				return true;
			}
		}
		return false;
	}

	public void compareMissingUnits(List<FoeUnit> foeUnitsActual)
	{
		foeUnits.ForEach(delegate(FoeUnit fUnit)
		{
			fUnit.isMissing = false;
		});
		foeUnitsActual.ForEach(delegate(FoeUnit fUnit)
		{
			fUnit.mChecked = false;
		});
		for (int i = 1; i <= 8; i++)
		{
			FoeUnit unit = getUnit(i, foeUnits);
			if (unit.Empty)
			{
				continue;
			}
			bool flag = false;
			for (int j = 1; j <= 8; j++)
			{
				FoeUnit unit2 = getUnit(j, foeUnitsActual);
				if (!unit2.mChecked && !unit2.Removed && ImageWorks.FindTemplateEmguExist(unit.getDBM().Bitmap, unit2.getDBM().Bitmap))
				{
					flag = true;
					unit2.mChecked = true;
					break;
				}
			}
			if (!flag)
			{
				unit.isMissing = true;
			}
		}
	}

	public void removeWrongUnits()
	{
		bool flag = false;
		foeUnitsActual = new List<FoeUnit>();
		getUnits(foeUnitsActual, actual: true);
		foeUnits.ForEach(delegate(FoeUnit fUnit)
		{
			fUnit.mChecked = false;
		});
		for (int i = 1; i <= 8; i++)
		{
			FoeUnit unit = getUnit(i, foeUnitsActual);
			if (unit.Empty)
			{
				continue;
			}
			bool flag2 = false;
			for (int j = 1; j <= 8; j++)
			{
				FoeUnit unit2 = getUnit(j, foeUnits);
				if (!unit2.mChecked && !unit2.Removed && ImageWorks.FindTemplateEmguExist(unit2.getDBM().Bitmap, unit.getDBM().Bitmap))
				{
					flag2 = true;
					unit.mChecked = true;
					break;
				}
			}
			if (!flag2)
			{
				flag = true;
				removeUnit(i, unit.CenterPoint);
				for (int k = i; k <= 7; k++)
				{
					getUnit(k + 1, foeUnitsActual).CenterPoint = getUnit(k, foeUnitsActual).CenterPoint;
				}
			}
		}
		if (flag)
		{
			fillUnits();
		}
	}

	public bool fillUnitsWithCheck(bool checkForInv = true, int minUnits = 0, bool quantum = false)
	{
		Bitmap srcBmp = PixelTools.CaptureScreen(battleScrTopLeft, battleScrBotRight);
		if (imgBattleResult("autoBattleCheck", srcBmp) != "autoBattleCheck")
		{
			FoeTools.outInfo("can't refill, not on start fight screen");
			return false;
		}
		if (Settings.Instance.ReplaceUnitsR && !quantum)
		{
			doBattleAction("replaceUnitsWithR");
			return true;
		}
		int num = fillUnits(checkForInv);
		int num2 = countEmptyUnits();
		if (FoeTools.DoubleCheckUnits)
		{
			removeWrongUnits();
		}
		switch (num)
		{
		case -1:
			return false;
		case 0:
			FoeTools.outInfo("units refilled");
			break;
		default:
			if (8 - (num + num2) >= minUnits)
			{
				FoeTools.outInfo("units partly refilled");
			}
			else if (minUnits != 0)
			{
				FoeTools.outInfo("less units than set");
				return false;
			}
			break;
		}
		return true;
	}

	public int fillUnits(bool checkForInv = true)
	{
		Bitmap val = null;
		wrongUnit16 = null;
		wrongUnit17 = null;
		foeUnits.ForEach(delegate(FoeUnit fUnit)
		{
			fUnit.isMissing = false;
		});
		if (checkForInv)
		{
			bool flag = false;
			val = PixelTools.CaptureScreen(battleScrTopLeft, battleScrBotRight);
			if (imgBattleResult("autoBattleCheck", val) == "autoBattleCheck")
			{
				flag = true;
			}
			if (!flag)
			{
				ColorCheck colorCheck = new ColorCheck(3.0);
				coord.add(new Point(-630, 533), "invCheck", coord.fromCudlik);
				ColorCheckUnit cu = new ColorCheckUnit("invCheck", coord.get("invCheck"), new List<Color> { Color.FromArgb(179, 187, 202) });
				if (colorCheck.WaitForColor(cu, 5, log: false, 1, 1))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				FoeTools.outInfo("error: fill units failed, are we in pre battle screen?");
				return -1;
			}
		}
		foeUnitsActual = new List<FoeUnit>();
		getUnits(foeUnitsActual, actual: true);
		for (int num = 8; num >= 1; num--)
		{
			FoeUnit unit = getUnit(num, foeUnitsActual);
			unit.mChecked = false;
			if (unit.Health < MinHealthAllowed && unit.Health > 0)
			{
				removeUnit(num, unit.CenterPoint);
			}
			if (unit.Health < 1)
			{
				unit.Removed = true;
			}
		}
		if (FoeTools.DebugMode)
		{
			FoeTools.outInfo("units removed");
		}
		dumpUnits(foeUnitsActual, "actual");
		compareMissingUnits(foeUnitsActual);
		dumpUnits(foeUnits, "locked");
		int num2 = countMissingUnits();
		if (FoeTools.UnitsDebug)
		{
			debugCntr++;
			FoeTools.WriteLogFile($"units missing: {num2}\n", $"c:\\mik\\foeunitsdebug{debugCntr}.txt");
			PixelTools.CaptureScreenDBM(-1, -1, -1, -1, $"c:\\mik\\foeunitsdebug{debugCntr}.png");
			compareMissingUnits(foeUnitsActual);
		}
		if (num2 < 1)
		{
			return 0;
		}
		if (checkWholeInv())
		{
			return 0;
		}
		Bitmap scrBmp = PixelTools.CaptureScreen(coord.get("unitInvscrollBarImgStart"), coord.get("unitInvscrollBarImgEnd"));
		int invPos = getInvPos(skipLeftCheck: false, skipRightCheck: true, scrBmp);
		if (invPos == 9)
		{
			FoeTools.outInfo("error: some units still missing");
			return countMissingUnits();
		}
		if (invPos > -1)
		{
			moveInv(-2, scrBmp);
		}
		if (checkWholeInv())
		{
			return 0;
		}
		int num3 = 0;
		bool flag2 = false;
		do
		{
			if (flag2)
			{
				moveInv(1, scrBmp);
				num3++;
				if (checkWholeInv())
				{
					return 0;
				}
			}
			else
			{
				moveInv(8, scrBmp);
				if (checkWholeInv())
				{
					return 0;
				}
			}
			scrBmp = PixelTools.CaptureScreen(coord.get("unitInvscrollBarImgStart"), coord.get("unitInvscrollBarImgEnd"));
			invPos = getInvPos(skipLeftCheck: true, skipRightCheck: false, scrBmp);
		}
		while (invPos != 1 && invPos != 9);
		if (countMissingUnits() > 0)
		{
			FoeTools.outInfo("error: some units still missing");
			return countMissingUnits();
		}
		return 0;
	}

	public void GoBattleDebug()
	{
		MouseTools.MoveMouse(coord.get("autoBattleCheck"));
		FoeTools.Wait(1000);
		for (int num = 17; num >= 0; num--)
		{
			MouseTools.MoveMouse(coord.get("invUnit" + num));
			FoeTools.Wait(1000);
		}
	}

	public string imgBattleResult(string battleCheck, Bitmap srcBmp, string battleResult = null)
	{
		Bitmap val = null;
		Bitmap val2 = null;
		Bitmap val3 = null;
		bool flag = false;
		int num = 0;
		double num2 = 0.0;
		double num3 = 0.0;
		if (battleResult == null)
		{
			if (battleCheck == "autoBattleCheck")
			{
				val = ImageWorks.getBmp("./images/chautobattle.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "nextRound")
			{
				val = ImageWorks.getBmp("./images/chnextwave2.png", coord.zoomX, coord.zoomY);
				val2 = ImageWorks.getBmp("./images/chnextwave4.png", coord.zoomX, coord.zoomY);
				num = 200;
				num2 = 0.95;
			}
			if (battleCheck == "fightOk")
			{
				val = ImageWorks.getBmp("./images/chbattleresult.png", coord.zoomX, coord.zoomY);
				val3 = ImageWorks.getBmp("./images/ch-ab2.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "gotReward")
			{
				val = ImageWorks.getBmp("./images/chreward.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "gotQuantReward")
			{
				val = ImageWorks.getBmp("./images/chquantrw.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "onHold")
			{
				val = ImageWorks.getBmp("./images/chwait.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "startFight")
			{
				val = ImageWorks.getBmp("./images/chfight.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "startFightQuantum")
			{
				val = ImageWorks.getBmp("./images/chquant.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "newExpedition")
			{
				val = ((!((double)coord.zoomX >= 1.5) || !((double)coord.zoomX <= 1.6)) ? ImageWorks.getBmp("./images/expesipka-fh.png", coord.zoomX, coord.zoomY) : ImageWorks.getBmp("./images/expe-sipka150.png"));
			}
			if (battleCheck == "expeFight")
			{
				val = ImageWorks.getBmp("./images/expeFight.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "expeFight2")
			{
				val = ImageWorks.getBmp("./images/expeFight2.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "expe7")
			{
				val = ImageWorks.getBmp("./images/expe7.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "expe8")
			{
				val = ImageWorks.getBmp("./images/expe8.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "expeFinished")
			{
				val = ImageWorks.getBmp("./images/expe-fin.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "expeOutOfTries")
			{
				val = ImageWorks.getBmp("./images/expe-outoftries.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "expeReward")
			{
				val = ((!((double)coord.zoomX >= 1.5) || !((double)coord.zoomX <= 1.6)) ? ImageWorks.getBmp("./images/expe-reward-fh.png") : ImageWorks.getBmp("./images/expe-reward-150.png"));
			}
			if (battleCheck == "quantOOA")
			{
				val = ImageWorks.getBmp("./images/chquantooa.png", coord.zoomX, coord.zoomY);
			}
			if (battleCheck == "histHub")
			{
				val = ImageWorks.getBmp("./images/foe-hist-start.png", coord.zoomX, coord.zoomY);
			}
			if (val != null)
			{
				if (ImageWorks.FindTemplateEmguExist(srcBmp, val))
				{
					flag = true;
					num3 = FoeTools.debugDouble2;
				}
				else if (val2 != null && ImageWorks.FindTemplateEmguExist(srcBmp, val2))
				{
					flag = true;
					num3 = FoeTools.debugDouble2;
				}
				if (flag && val3 != null)
				{
					val3 = ImageWorks.getBmp("./images/ch-ab2.png", coord.zoomX, coord.zoomY);
					if (ImageWorks.FindTemplateEmguExist(srcBmp, val3))
					{
						return "nextRound";
					}
				}
				if (flag)
				{
					Point p = new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
					Point point = coord.DistanceFrom(p, patternMatchTopLeft);
					if ((num > 0 && point.Y < num) || (num2 > 0.0 && num3 > num2))
					{
						return battleCheck;
					}
					battleResult = battleCheck;
					if (battleCheck == "nextRound")
					{
						ImageWorks.savePngDebug(srcBmp, $"nextround-{FoeTools.getStopky(zeros: true)}-{point.X}-{point.Y}-{FoeTools.debugDouble2}");
					}
				}
				else
				{
					battleResult = null;
				}
			}
		}
		return battleResult;
	}

	private int safetyRescue()
	{
		if (rescueStopWatch.Elapsed.TotalSeconds > 10.0)
		{
			MouseTools.MoveMouse(coord.getRandomizedPoint("safetyRescue", 50, 50));
			rescueStopWatch.Restart();
		}
		return 0;
	}

	private bool checkInvChanged()
	{
		Point a = coord.get("invUnit16");
		Point b = coord.MoveRelative(unitHalfDimension, coord.get("invUnit17"), coord.useZoom);
		DirectBitmap directBitmap = PixelTools.CaptureRegionDBM(a, b);
		if (inventoryRegionBmp == null)
		{
			inventoryRegionBmp = directBitmap;
			return true;
		}
		if (inventoryRegionBmp != null && directBitmap.isEqual(inventoryRegionBmp.Bits))
		{
			return false;
		}
		inventoryRegionBmp = directBitmap;
		return true;
	}

	private bool checkInventory(int x, bool testWrongUnits = true)
	{
		bool flag = false;
		FoeUnit foeUnit = new FoeUnit();
		Point point = coord.get("invUnit" + x);
		Point b = coord.MoveRelative(unitDimension, point, coord.useZoom);
		point.X -= 2;
		b.Y -= 2;
		b.X++;
		b.Y++;
		DirectBitmap directBitmap = PixelTools.CaptureRegionDBM(point, b);
		if (testWrongUnits)
		{
			if (x == 16 && wrongUnit16 != null && directBitmap.isEqual(wrongUnit16.Bits))
			{
				FoeTools.outInfo($"same unit in inv {x}, skip");
				return false;
			}
			if (x == 17 && wrongUnit17 != null && directBitmap.isEqual(wrongUnit17.Bits))
			{
				FoeTools.outInfo($"same unit in inv {x}, skip");
				return false;
			}
		}
		foreach (FoeUnit foeUnit2 in foeUnits)
		{
			if (!foeUnit2.isMissing)
			{
				continue;
			}
			DirectBitmap dBM = foeUnit2.getDBM();
			if (FoeTools.DebugMode)
			{
				((Image)directBitmap.Bitmap).Save($"c:\\mik\\bmp-inv-{x}.png");
				((Image)dBM.Bitmap).Save($"c:\\mik\\bmp-curr-{foeUnit2.UnitId}.png");
			}
			if (!ImageWorks.FindTemplateEmguExist(dBM.Bitmap, directBitmap.Bitmap))
			{
				continue;
			}
			foeUnit.AddHealthBar2(getHealthBar(point));
			if (FoeTools.DebugMode)
			{
				((Image)foeUnit.GetHealthBar()).Save($"c:\\mik\\invunit{foeUnit2.UnitId}hb.png");
			}
			if (foeUnit.Health >= MinHealthAllowed)
			{
				point = coord.get("invUnit" + x);
				b = coord.MoveRelative(unitHalfDimension, point, coord.useZoom);
				Point randomizedPoint = coord.getRandomizedPoint(b, 10, 10);
				MouseTools.ClickMouse(randomizedPoint);
				FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked getunit {randomizedPoint}");
				FoeTools.Wait(100);
				if (FoeTools.UnitsDebug)
				{
					_ = FoeTools.debugDouble2;
					((Image)directBitmap.Bitmap).Save("c:\\mik\\foeunitfound.png");
					((Image)dBM.Bitmap).Save("c:\\mik\\foeunitneeded.png");
				}
				FoeTools.Wait(10);
				foeUnit2.isMissing = false;
				return true;
			}
			flag = true;
		}
		if (!flag)
		{
			if (x == 16)
			{
				wrongUnit16 = directBitmap;
			}
			if (x == 17)
			{
				wrongUnit17 = directBitmap;
			}
		}
		return false;
	}

	private int countMissingUnits()
	{
		int num = 0;
		foreach (FoeUnit foeUnit in foeUnits)
		{
			if (foeUnit.isMissing)
			{
				num++;
			}
		}
		return num;
	}

	private int countEmptyUnits()
	{
		int num = 0;
		foreach (FoeUnit foeUnit in foeUnits)
		{
			if (foeUnit.Empty)
			{
				num++;
			}
		}
		return num;
	}

	private int getInvPos(bool skipLeftCheck = false, bool skipRightCheck = false, Bitmap scrBmp = null)
	{
		Color item = Color.FromArgb(125, 130, 140);
		Color item2 = Color.FromArgb(151, 160, 180);
		Color item3 = Color.FromArgb(144, 154, 174);
		Color item4 = Color.FromArgb(180, 187, 202);
		Color item5 = Color.FromArgb(140, 150, 171);
		Color item6 = Color.FromArgb(136, 146, 168);
		if (!skipLeftCheck)
		{
			if (scrBmp != null && ((Image)scrBmp).Width > 0 && ((Image)scrBmp).Height > 0 && ImageWorks.FindTemplateEmguExist(ImageWorks.getBmp("./images/invleft.png", coord.zoomX, coord.zoomY, "./images/invleft150.png"), scrBmp))
			{
				return -1;
			}
			List<Color> colorsLookFor = new List<Color> { item, item2, item3, item4 };
			List<PixelColor> colorsAt = PixelTools.GetColorsAt(coord.get("unitInvscrollBarStart"), 0, 0, 5, 2);
			if (PixelTools.CompareColors(colorsLookFor, PixelTools.PixelColorsToColors(colorsAt), 0.9))
			{
				return -1;
			}
		}
		if (!skipRightCheck)
		{
			if (scrBmp != null && ((Image)scrBmp).Width > 0 && ((Image)scrBmp).Height > 0 && ImageWorks.FindTemplateEmguExist(ImageWorks.getBmp("./images/invright.png", coord.zoomX, coord.zoomY, "./images/invright150.png"), scrBmp))
			{
				return 1;
			}
			List<Color> colorsLookFor2 = new List<Color> { item5, item6, item, item2, item3 };
			List<PixelColor> colorsAt = PixelTools.GetColorsAt(coord.get("unitInvscrollBarEnd"), 0, 0, 5, 2);
			if (PixelTools.CompareColors(colorsLookFor2, PixelTools.PixelColorsToColors(colorsAt), 0.9))
			{
				return 1;
			}
		}
		if (scrBmp != null && ((Image)scrBmp).Width > 0 && ((Image)scrBmp).Height > 0 && ImageWorks.FindTemplateEmguExist(ImageWorks.getBmp("./images/noinvscrollbar.png", coord.zoomX, coord.zoomY, "./images/noinvscrollbar150.png"), scrBmp))
		{
			return 9;
		}
		return 0;
	}

	private void moveInv(int dir = 0, Bitmap scrBmp = null)
	{
		bool flag = true;
		Point randomizedPoint = coord.getRandomizedPoint(coord.get("unitInvscrollBarMid"), 1, 1);
		Point randomizedPoint2 = coord.getRandomizedPoint(coord.get("unitInvscrollBarRight"), 2, 2);
		Point randomizedPoint3 = coord.getRandomizedPoint(coord.get("unitInvscrollBarLeft"), 2, 2);
		if (dir == 1)
		{
			if (!flag)
			{
				MouseTools.ClickMouse(randomizedPoint2);
				FoeTools.Wait(1);
			}
			else
			{
				MouseTools.MoveMouse(randomizedPoint);
				MouseTools.DoMouseWheel();
				FoeTools.Wait(90, randomize: false);
			}
		}
		if (dir == 8)
		{
			for (int i = 0; i < 9; i++)
			{
				if (!flag)
				{
					MouseTools.ClickMouse(randomizedPoint2);
					FoeTools.Wait(1);
				}
				else
				{
					MouseTools.MoveMouse(randomizedPoint);
					MouseTools.DoMouseWheel();
					FoeTools.Wait(50);
				}
				scrBmp = PixelTools.CaptureScreen(coord.get("unitInvscrollBarImgStart"), coord.get("unitInvscrollBarImgEnd"));
				int invPos = getInvPos(skipLeftCheck: true, skipRightCheck: false, scrBmp);
				if (invPos == 1 || invPos == 9)
				{
					return;
				}
			}
			FoeTools.Wait(350);
		}
		if (dir == -1)
		{
			if (!flag)
			{
				MouseTools.ClickMouse(randomizedPoint3);
				FoeTools.Wait(1);
			}
			else
			{
				MouseTools.MoveMouse(randomizedPoint);
				MouseTools.DoMouseWheel("up");
				if (!checkInvChanged())
				{
					FoeTools.Wait(1, randomize: false);
				}
			}
		}
		if (dir == 2)
		{
			if (flag)
			{
				MouseTools.MoveMouse(randomizedPoint);
			}
			int invPos2 = getInvPos(skipLeftCheck: false, skipRightCheck: false, scrBmp);
			while (invPos2 != 1 && invPos2 != 9)
			{
				if (!flag)
				{
					randomizedPoint2 = coord.getRandomizedPoint(coord.get("unitInvscrollBarRight"), 2, 2);
					MouseTools.ClickMouse(randomizedPoint2);
					FoeTools.Wait(1);
				}
				else
				{
					MouseTools.DoMouseWheel();
				}
				scrBmp = PixelTools.CaptureScreen(coord.get("unitInvscrollBarImgStart"), coord.get("unitInvscrollBarImgEnd"));
				invPos2 = getInvPos(skipLeftCheck: false, skipRightCheck: false, scrBmp);
			}
		}
		if (dir != -2)
		{
			return;
		}
		if (flag)
		{
			MouseTools.MoveMouse(randomizedPoint);
		}
		int invPos3 = getInvPos(skipLeftCheck: false, skipRightCheck: false, scrBmp);
		while (invPos3 != -1 && invPos3 != 9)
		{
			if (!flag)
			{
				randomizedPoint3 = coord.getRandomizedPoint(coord.get("unitInvscrollBarLeft"), 2, 2);
				MouseTools.ClickMouse(randomizedPoint3);
				FoeTools.Wait(1);
			}
			else
			{
				MouseTools.DoMouseWheel("up");
			}
			scrBmp = PixelTools.CaptureScreen(coord.get("unitInvscrollBarImgStart"), coord.get("unitInvscrollBarImgEnd"));
			invPos3 = getInvPos(skipLeftCheck: false, skipRightCheck: false, scrBmp);
		}
	}

	public FoeUnit getUnit(int n, List<FoeUnit> fUnits)
	{
		foreach (FoeUnit fUnit in fUnits)
		{
			if (fUnit.UnitId == n)
			{
				return fUnit;
			}
		}
		return null;
	}

	public void removeUnit(int n, Point p2)
	{
		MouseTools.ClickMouse(p2);
		FoeTools.Wait(10);
		foreach (FoeUnit item in foeUnitsActual)
		{
			if (item.UnitId == n)
			{
				item.Removed = true;
				item.Empty = true;
			}
		}
	}

	public bool checkLoss()
	{
		if (!FoeTools.stopLoss)
		{
			return false;
		}
		Point q = coord.get("relatedPointTo");
		Point p = new Point(-491, 157);
		Point p2 = new Point(-390, 260);
		Point a = coord.MoveRelative(p, q, coord.useZoom);
		Point b = coord.MoveRelative(p2, q, coord.useZoom);
		Bitmap val = PixelTools.CaptureRegion(a, b);
		poharBmp = ImageWorks.getBmp("./images/pohar.png", coord.zoomX, coord.zoomY);
		if (FoeTools.DebugMode)
		{
			((Image)val).Save("c:\\mik\\oknopohar.png");
			((Image)poharBmp).Save("c:\\mik\\poharpohar.png");
			q = coord.get("relatedPointTo");
			Point mousePoint = MouseTools.GetMousePoint();
			FoeTools.outInfo($"cudlik: {q.X} {q.Y}");
			FoeTools.outInfo($"mouse: {mousePoint.X} {mousePoint.Y}");
		}
		if (ImageWorks.FindTemplateEmgu(poharBmp, val).HasValue)
		{
			FoeTools.outInfo("win detected");
			lossesInRow = 0;
		}
		else
		{
			lossesInRow++;
			FoeTools.outInfo($"loss detected! {lossesInRow} losses in row");
			if (FoeTools.LossesDebug)
			{
				((Image)val).Save("c:\\mik\\oknopohar.png");
				((Image)poharBmp).Save("c:\\mik\\poharpohar.png");
			}
		}
		return false;
	}

	public bool processBattleScores()
	{
		Point q = coord.get("goFightCheck");
		Point p = new Point(184, -57);
		Point p2 = new Point(268, 40);
		p = coord.MoveRelative(p, q, coord.useZoom);
		p2 = coord.MoveRelative(p2, q, coord.useZoom);
		Bitmap val = PixelTools.CaptureScreen(p.X, p.Y, p2.X, p2.Y);
		string text = FoeOCR.ReadNumbersCharles(val, 1, modif2: false, "", "1/234567890", (PageSegMode)6, 3f);
		char[] separator = new char[1] { '\n' };
		string[] array = text.Split(separator);
		bool flag = false;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			string text3 = text2;
			if (!text2.Contains("/") && text2.Length == 7)
			{
				text3 = text2.Substring(0, 3) + "/" + text2.Substring(4, 3);
			}
			if (!text2.Contains("/") && text2.Length == 6)
			{
				text3 = text2.Substring(0, 2) + "/" + text2.Substring(3, 3);
			}
			string[] array3 = text3.Split(new char[1] { '/' });
			if (array3.Length != 2)
			{
				continue;
			}
			try
			{
				int num = int.Parse(array3[0]);
				int num2 = int.Parse(array3[1]);
				if (num <= num2)
				{
					flag = true;
					if (LastMaxBattleScore < num)
					{
						LastMaxBattleScore = num;
					}
					LastMaxBattleScore = num;
					LastBattleTotal = num2;
					FoeTools.outInfo($"battle score: {num}/{num2}");
					if (foeSectors != null)
					{
						foeSectors.AddSector(currentSectorBattle, num2, num);
					}
					if (num >= num2 - fewBattlesRemain)
					{
						return true;
					}
				}
			}
			catch
			{
			}
		}
		if (!flag)
		{
			if (FoeTools.OCRDebug)
			{
				int num3 = random.Next(1, 999999);
				((Image)val).Save($"c:\\mik\\foe8-ocrfail-{num3}-ori.png");
				((Image)FoeOCR.PrepareImage(val, 1, modif2: false, "", "1/234567890", (PageSegMode)6, 3f)).Save($"c:\\mik\\foe8-ocrfail-mod-{num3}-ori.png");
			}
			FoeTools.outInfo("reading battle scores failed, please uncheck 'Until few battles remain' and set battles number");
			LastMaxBattleScore++;
			if (LastMaxBattleScore > 0 && LastBattleTotal > 0)
			{
				FoeTools.outInfo($"..assuming it's {LastMaxBattleScore}/{LastBattleTotal}");
				if (foeSectors != null)
				{
					foeSectors.AddSector(currentSectorBattle, LastBattleTotal, LastMaxBattleScore);
				}
				if (LastMaxBattleScore >= LastBattleTotal - 5)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void initActionLog()
	{
		actionLog = new List<string>();
		resultsLog = new List<string>();
	}

	public void doBattleAction(string actionType)
	{
		switch (actionType)
		{
		case "replaceUnitsWithR":
			SendKeys.SendWait("r");
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " r pressed");
			FoeTools.Wait(100);
			actionLog.Add("r");
			break;
		case "b":
			actionLog.Add("b");
			break;
		case "esc":
			actionLog.Add("esc");
			break;
		case "a":
			actionLog.Add("a");
			break;
		case "unstuck":
			actionLog.Add("unstuck");
			break;
		default:
			FoeTools.outInfo("unknown battleaction");
			throw new Exception("unknown battleaction");
		}
	}

	public void doAutoBattle(Point b, bool safeSpotMightBeNeeded = false, bool isNextRound = false, bool quantum = false)
	{
		if (Settings.Instance.AutoBattlesKey)
		{
			if (isNextRound)
			{
				FoeTools.Wait(200);
			}
			SendKeys.SendWait("b");
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " b pressed");
			doBattleAction("b");
		}
		else
		{
			MouseTools.ClickMouse(b);
			FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked {b}");
		}
		if (safeSpotMightBeNeeded && Settings.Instance.BetterMoveMouseSafeSpot)
		{
			doMoveMouseSafePosition();
		}
	}

	public void doEscWindow(Point b)
	{
		if (Settings.Instance.EscBattlesKey)
		{
			SendKeys.SendWait("{ESC}");
			FoeTools.Wait(300);
			FoeTools.outInfo("ESC pressed");
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " esc pressed");
			doBattleAction("esc");
		}
		else
		{
			MouseTools.ClickMouse(b);
			FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked {b}");
		}
	}

	public void doStartFight()
	{
		if (Settings.Instance.AutoBattlesKey)
		{
			SendKeys.SendWait("a");
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " a pressed");
			doBattleAction("a");
		}
		else
		{
			Point randomizedPoint = coord.getRandomizedPoint("goFightClick", 20, 10);
			MouseTools.ClickMouse(randomizedPoint);
			FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked doStartFight {randomizedPoint}");
		}
	}

	public void doAutoBattleFocus()
	{
		try
		{
			Point randomizedPoint = coord.getRandomizedPoint("relatedPointTo", 10, 4);
			Point q = new Point(30, 30);
			randomizedPoint = coord.MoveAbsoluteRelative(randomizedPoint, q, coord.useZoom);
			Point randomPointInRectangle = PixelTools.GetRandomPointInRectangle(coord.get("ramecekTopleft"), randomizedPoint);
			MouseTools.ClickMouse(randomPointInRectangle);
			FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked safe {randomPointInRectangle}");
		}
		catch (Exception)
		{
			FoeTools.outInfo("unable to move mouse into safe position");
		}
	}

	public void doMoveMouseSafePosition()
	{
		try
		{
			Point randomizedPoint = coord.getRandomizedPoint("relatedPointTo", 10, 4);
			Point q = new Point(30, 30);
			randomizedPoint = coord.MoveAbsoluteRelative(randomizedPoint, q, coord.useZoom);
			MouseTools.MoveMouse(PixelTools.GetRandomPointInRectangle(coord.get("ramecekTopleft"), randomizedPoint));
		}
		catch (Exception)
		{
			FoeTools.outInfo("unable to move mouse into safe position");
		}
	}

	private bool unStuckAction()
	{
		Bitmap val = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
		Bitmap bmp = ImageWorks.getBmp("./images/surr.png", coord.zoomX, coord.zoomY);
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			doStopSurrender(isSurrender: true);
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stuck - surrender");
			return true;
		}
		bmp = ImageWorks.getBmp("./images/build.png", coord.zoomX, coord.zoomY);
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			doStopSurrender();
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stuck - build");
			return true;
		}
		bmp = ImageWorks.getBmp("./images/chreward.png", coord.zoomX, coord.zoomY);
		if (ImageWorks.FindTemplateEmguExist(val, bmp))
		{
			doStopSurrender();
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stuck - reward");
			return true;
		}
		string battleResult = null;
		battleResult = imgBattleResult("autoBattleCheck", val, battleResult);
		if (battleResult == "autoBattleCheck")
		{
			Point randomizedPoint = coord.getRandomizedPoint("autoBattleClick", 10, 5);
			doAutoBattle(randomizedPoint, safeSpotMightBeNeeded: true);
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stuck - autobattle");
			return true;
		}
		return false;
	}

	private string unStuck(string battleResult)
	{
		string result = null;
		resultsLog.Add(battleResult);
		bool flag = true;
		bool flag2 = true;
		int num = 150;
		if (resultsLog.Count > num)
		{
			resultsLog.RemoveRange(0, resultsLog.Count - num);
		}
		if (actionLog.Count > num)
		{
			actionLog.RemoveRange(0, actionLog.Count - num);
		}
		if (actionLog.Count > 10)
		{
			string text = actionLog[actionLog.Count - 1];
			for (int i = actionLog.Count - 10; i < actionLog.Count; i++)
			{
				if (actionLog[i] != text)
				{
					flag2 = false;
				}
			}
		}
		else
		{
			flag2 = false;
		}
		if (flag2)
		{
			FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " action stuck");
			doBattleAction("unstuck");
		}
		if (resultsLog.Count > 5)
		{
			for (int j = resultsLog.Count - 5; j < resultsLog.Count; j++)
			{
				if (resultsLog[j] != "nextRound")
				{
					flag = false;
				}
			}
		}
		else
		{
			flag = false;
		}
		if (flag && unStuckAction())
		{
			return result;
		}
		bool flag3 = false;
		if (resultsLog.Count > 30)
		{
			flag3 = true;
			string text2 = resultsLog[resultsLog.Count - 1];
			if (text2 == "onHold")
			{
				flag3 = false;
			}
			for (int k = resultsLog.Count - 10; k < resultsLog.Count; k++)
			{
				if (resultsLog[k] != text2)
				{
					flag3 = false;
				}
			}
		}
		if (flag3)
		{
			if (unStuckAction())
			{
				return result;
			}
			return "stall stuck";
		}
		return result;
	}

	public void GoHistoricalBattle(int battlesCount, int minUnits, bool doAttack, bool doDefend, bool doFp, bool doMedals)
	{
		tooManyLosses = false;
		stopFighting = false;
		lossesInRow = 0;
		battleEndMessage = string.Empty;
		LastMaxBattleScore = 0;
		LastBattleTotal = 0;
		patternMatchTopLeft = battleScrTopLeft;
		patternMatchBotRight = battleScrBotRight;
		doAutoBattleFocus();
		rescueStopWatch.Start();
		string text = string.Empty;
		int num = 0;
		int howLong = 500;
		int howLong2 = 100;
		int howLong3 = 500;
		bool flag = false;
		int num2 = 0;
		bool flag2 = false;
		int num3 = 0;
		FoeTools.outInfo($"Historical Allies: starting dedicated battle loop for {battlesCount} battles");
		while (!FoeTools.ts.Token.IsCancellationRequested && !stopFighting)
		{
			if (num2 >= battlesCount)
			{
				FoeTools.outInfo($"Historical Allies: reached battles count {battlesCount}");
				break;
			}
			Bitmap sourceBitmap = PixelTools.CaptureScreen();
			Bitmap val = null;
			if (FoeTools.ZoomBattlesIndentified)
			{
				val = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
			}
			string text2 = null;
			Bitmap bmp = ImageWorks.getBmp("./images/foe-hist-donate.png", coord.zoomX, coord.zoomY);
			if (bmp != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp))
			{
				Bitmap bmp2 = ImageWorks.getBmp("./images/foe-hist-ok-donate.png", coord.zoomX, coord.zoomY);
				if (bmp2 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp2))
				{
					Point p = new Point(ImageWorks.LastFound.X + ((Image)bmp2).Width / 2, ImageWorks.LastFound.Y + ((Image)bmp2).Height / 2);
					FoeTools.outInfo("Hub: confirming donation");
					MouseTools.ClickMouse(p);
					FoeTools.Wait(1000);
					continue;
				}
			}
			if (!flag)
			{
				Bitmap bmp3 = ImageWorks.getBmp("./images/foe-hist-start.png", coord.zoomX, coord.zoomY);
				if (bmp3 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp3))
				{
					bool num4 = ImageWorks.FindTemplateEmguExist(sourceBitmap, ImageWorks.getBmp("./images/foe-hist-att.png", coord.zoomX, coord.zoomY));
					bool flag3 = ImageWorks.FindTemplateEmguExist(sourceBitmap, ImageWorks.getBmp("./images/foe-hist-def.png", coord.zoomX, coord.zoomY));
					bool flag4 = ImageWorks.FindTemplateEmguExist(sourceBitmap, ImageWorks.getBmp("./images/foe-hist-rew.png", coord.zoomX, coord.zoomY));
					bool flag5 = doFp && ImageWorks.FindTemplateEmguExist(sourceBitmap, ImageWorks.getBmp("./images/foe-hist-fp.png", coord.zoomX, coord.zoomY));
					bool flag6 = doMedals && ImageWorks.FindTemplateEmguExist(sourceBitmap, ImageWorks.getBmp("./images/foe-hist-medals.png", coord.zoomX, coord.zoomY));
					if (num4 || flag3 || flag4 || flag5 || flag6)
					{
						text2 = "histHub";
					}
				}
			}
			if (text2 == null && !FoeTools.ZoomBattlesIndentified)
			{
				Bitmap bmp4 = ImageWorks.getBmp("./images/battle-init-fh.png");
				if (bmp4 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp4))
				{
					FoeTools.outInfo("Battle: army window detected, performing late initialization");
					if (FoeTools.window.foeTools.findXcudlik() && FoeTools.window.foeTools.FinishGoFightInit(silent: true, quantumIncursions: false, historicalAllies: true))
					{
						FoeTools.outInfo("Battle: initialization successful");
						FoeBattle fb = FoeTools.window.foeTools.getFb();
						if (fb != null)
						{
							battleScrTopLeft = fb.battleScrTopLeft;
							battleScrBotRight = fb.battleScrBotRight;
						}
						patternMatchTopLeft = battleScrTopLeft;
						patternMatchBotRight = battleScrBotRight;
						FoeTools.Wait(500);
						continue;
					}
					FoeTools.outInfo("Battle: late initialization failed!");
				}
			}
			if (text2 == null && FoeTools.ZoomBattlesIndentified && val != null)
			{
				text2 = imgBattleResult("gotReward", val, text2);
				if (text2 == null)
				{
					text2 = imgBattleResult("gotQuantReward", val, text2);
					if (text2 == "gotQuantReward")
					{
						text2 = "gotReward";
					}
				}
				if (text2 == null)
				{
					text2 = imgBattleResult("autoBattleCheck", val, text2);
				}
				if (text2 == null)
				{
					text2 = imgBattleResult("nextRound", val, text2);
				}
				if (text2 == null)
				{
					text2 = imgBattleResult("startFight", val, text2);
				}
				if (text2 == null)
				{
					text2 = imgBattleResult("fightOk", val, text2);
				}
				if (text2 == null)
				{
					Bitmap bmp5 = ImageWorks.getBmp("./images/chok.png", coord.zoomX, coord.zoomY);
					if (bmp5 != null && ImageWorks.FindTemplateEmguExist(val, bmp5))
					{
						Bitmap bmp6 = ImageWorks.getBmp("./images/ch-ab2.png", coord.zoomX, coord.zoomY);
						if (!ImageWorks.FindTemplateEmguExist(val, bmp6))
						{
							text2 = "fightOk";
						}
					}
				}
				if (text2 == null)
				{
					text2 = imgBattleResult("onHold", val, text2);
				}
			}
			if (text2 != text)
			{
				flag2 = false;
				num3 = 0;
			}
			else
			{
				switch (text2)
				{
				case "fightOk":
				case "gotReward":
					num3++;
					if (num3 > 3)
					{
						FoeTools.outInfo("Battle: stuck on " + text2 + " - retrying dismissal with ESC");
						SendKeys.SendWait("{ESC}");
						flag2 = false;
						num3 = 0;
						FoeTools.Wait(800);
						continue;
					}
					break;
				}
			}
			switch (text2)
			{
			case "histHub":
			{
				flag = false;
				bool flag7 = false;
				Bitmap bmp7 = ImageWorks.getBmp("./images/foe-hist-rew.png", coord.zoomX, coord.zoomY);
				if (bmp7 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp7))
				{
					Point p2 = new Point(ImageWorks.LastFound.X + ((Image)bmp7).Width / 2, ImageWorks.LastFound.Y + ((Image)bmp7).Height / 2);
					num2++;
					FoeTools.outInfo($"Hub: collecting reward (action #{num2})");
					MouseTools.ClickMouse(p2);
					FoeTools.Wait(1200);
					flag7 = true;
				}
				if (!flag7 && doFp)
				{
					Bitmap bmp8 = ImageWorks.getBmp("./images/foe-hist-fp.png", coord.zoomX, coord.zoomY);
					if (bmp8 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp8))
					{
						Point p3 = new Point(ImageWorks.LastFound.X + ((Image)bmp8).Width / 2, ImageWorks.LastFound.Y + ((Image)bmp8).Height / 2);
						num2++;
						FoeTools.outInfo($"Hub: donating FP (action #{num2})");
						MouseTools.ClickMouse(p3);
						FoeTools.Wait(1200);
						flag7 = true;
					}
				}
				if (!flag7 && doMedals)
				{
					Bitmap bmp9 = ImageWorks.getBmp("./images/foe-hist-medals.png", coord.zoomX, coord.zoomY);
					if (bmp9 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp9))
					{
						Point p4 = new Point(ImageWorks.LastFound.X + ((Image)bmp9).Width / 2, ImageWorks.LastFound.Y + ((Image)bmp9).Height / 2);
						num2++;
						FoeTools.outInfo($"Hub: donating medals (action #{num2})");
						MouseTools.ClickMouse(p4);
						FoeTools.Wait(1200);
						flag7 = true;
					}
				}
				if (!flag7 && doAttack)
				{
					Bitmap bmp10 = ImageWorks.getBmp("./images/foe-hist-att.png", coord.zoomX, coord.zoomY);
					if (bmp10 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp10))
					{
						Point p5 = new Point(ImageWorks.LastFound.X + ((Image)bmp10).Width / 2, ImageWorks.LastFound.Y + ((Image)bmp10).Height / 2);
						num2++;
						FoeTools.outInfo($"Hub: transition to battle (Attack, action #{num2})");
						MouseTools.ClickMouse(p5);
						FoeTools.Wait(1500);
						flag7 = true;
					}
				}
				if (!flag7 && doDefend)
				{
					Bitmap bmp11 = ImageWorks.getBmp("./images/foe-hist-def.png", coord.zoomX, coord.zoomY);
					if (bmp11 != null && ImageWorks.FindTemplateEmguExist(sourceBitmap, bmp11))
					{
						Point p6 = new Point(ImageWorks.LastFound.X + ((Image)bmp11).Width / 2, ImageWorks.LastFound.Y + ((Image)bmp11).Height / 2);
						num2++;
						FoeTools.outInfo($"Hub: transition to battle (Defend, action #{num2})");
						MouseTools.ClickMouse(p6);
						FoeTools.Wait(1500);
						flag7 = true;
					}
				}
				if (!flag7)
				{
					if (num > 8)
					{
						FoeTools.outInfo("Hub: no more Action buttons visible — finishing");
						stopFighting = true;
						break;
					}
					FoeTools.outInfo("Hub: waiting for action buttons...");
					goto default;
				}
				num = 0;
				flag = false;
				continue;
			}
			case "autoBattleCheck":
				if (!flag)
				{
					FoeTools.outInfo("Battle: army management screen detected, checking units");
					if (!fillUnitsWithCheck(checkForInv: true, minUnits))
					{
						stopFighting = true;
						battleEndMessage = "out of units or refill failed";
						break;
					}
					FoeTools.outInfo("Battle: clicking Autobattle button");
					Point randomizedPoint3 = coord.getRandomizedPoint("autoBattleClick", 10, 5);
					doAutoBattle(randomizedPoint3, safeSpotMightBeNeeded: true);
					flag = true;
					FoeTools.Wait(howLong3);
				}
				goto default;
			case "nextRound":
			{
				FoeTools.outInfo("Battle: next round detected, clicking next");
				Point randomizedPoint2 = coord.getRandomizedPoint("nextBattleRoundClick", 20, 2);
				doAutoBattle(randomizedPoint2, safeSpotMightBeNeeded: false, isNextRound: true);
				FoeTools.Wait(howLong);
				goto default;
			}
			case "fightOk":
				if (!flag2)
				{
					FoeTools.outInfo("Battle: victory/result screen detected");
					Point b = coord.getRandomizedPoint("fightOkClick", 20, 2);
					if (val != null && !Settings.Instance.EscBattlesKey)
					{
						Point? point = ImageWorks.FindTemplateEmgu(ImageWorks.getBmp("./images/chok.png", coord.zoomX, coord.zoomY), val);
						if (point.HasValue)
						{
							Point value = point.Value;
							value.X += patternMatchTopLeft.X;
							value.Y += patternMatchTopLeft.Y;
							b = coord.MoveRelative(new Point(50, 10), value, coord.useZoom);
						}
					}
					doEscWindow(b);
					flag2 = true;
					flag = false;
					FoeTools.Wait(1200);
				}
				else
				{
					FoeTools.outInfo("Battle: skipping repeat result click");
				}
				goto default;
			case "gotReward":
				if (!flag2)
				{
					FoeTools.outInfo("Battle: reward popup detected");
					Point randomizedPoint = coord.getRandomizedPoint("rewardOkClick", 20, 2);
					doEscWindow(randomizedPoint);
					flag2 = true;
					flag = false;
					FoeTools.Wait(1200);
				}
				else
				{
					FoeTools.outInfo("Battle: skipping repeat reward click");
				}
				goto default;
			case "startFight":
				FoeTools.outInfo("Battle: start fight (sword icon) detected");
				doStartFight();
				FoeTools.Wait(howLong);
				goto default;
			default:
				if (text2 != null && text2 != "onHold")
				{
					num = 0;
					rescueStopWatch.Restart();
					text = text2;
				}
				else
				{
					num++;
					if (num > 15)
					{
						num = safetyRescue();
					}
				}
				FoeTools.Wait(howLong2);
				continue;
			}
			break;
		}
		if (!string.IsNullOrEmpty(battleEndMessage))
		{
			FoeTools.outInfo("Historical Allies done: " + battleEndMessage);
		}
		else
		{
			FoeTools.outInfo("Historical Allies done");
		}
	}

	public void GoFightExpeditions(int battlesToDo = 20, int minUnits = 0)
	{
		string text = "";
		string text2 = "";
		tooManyLosses = false;
		if (false)
		{
			GoBattleDebug();
			return;
		}
		int num = 0;
		ColorCheck colorCheck = new ColorCheck(3.0);
		int num2 = 0;
		string text3 = null;
		Bitmap val = null;
		Bitmap val2 = null;
		ColorCheckUnit item = new ColorCheckUnit("autoBattleCheck", coord.get("autoBattleCheck"), new List<Color> { Color.FromArgb(183, 62, 44) });
		ColorCheckUnit item2 = new ColorCheckUnit("autoBattleNoUnits", coord.get("autoBattleCheck"), new List<Color>
		{
			Color.FromArgb(101, 101, 101),
			Color.FromArgb(99, 99, 99),
			Color.FromArgb(112, 112, 112)
		});
		ColorCheckUnit item3 = new ColorCheckUnit("nextRound", coord.get("multiRoundBattleCheck"), new List<Color>
		{
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(147, 79, 31),
			Color.FromArgb(145, 78, 31)
		});
		ColorCheckUnit item4 = new ColorCheckUnit("fightOk", coord.get("fightOkCheck"), new List<Color>
		{
			Color.FromArgb(86, 132, 30),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(83, 127, 29)
		});
		ColorCheckUnit item5 = new ColorCheckUnit("fightOk2", coord.get("fightOkCheck2"), new List<Color>
		{
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(83, 127, 29)
		});
		ColorCheckUnit item6 = new ColorCheckUnit("gotReward", coord.get("rewardCheck"), new List<Color>
		{
			Color.FromArgb(146, 78, 31),
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(162, 92, 38)
		});
		ColorCheckUnit item7 = new ColorCheckUnit("startFight", coord.get("goFightCheck"), new List<Color>
		{
			Color.FromArgb(154, 74, 7),
			Color.FromArgb(151, 74, 7)
		});
		ColorCheckUnit item8 = new ColorCheckUnit("startFight2", coord.get("goFightCheckTagginOn"), new List<Color>
		{
			Color.FromArgb(215, 156, 42),
			Color.FromArgb(213, 151, 39)
		});
		List<ColorCheckUnit> checkUnits = new List<ColorCheckUnit> { item3, item4, item5, item6, item7, item8, item, item2 };
		if (FoeTools.fightUntilOCR)
		{
			battlesToDo = 99999;
		}
		stopFighting = false;
		lossesInRow = 0;
		battleEndMessage = "";
		LastMaxBattleScore = 0;
		LastBattleTotal = 0;
		patternMatchTopLeft = new Point(0, 0);
		patternMatchBotRight = new Point(PixelTools.MaxX, PixelTools.MaxY);
		doAutoBattleFocus();
		rescueStopWatch.Start();
		bool safeSpotMightBeNeeded = false;
		Point q = new Point(0, 0);
		int i;
		for (i = 0; i < battlesToDo; i++)
		{
			if (stopFighting)
			{
				break;
			}
			Point randomizedPoint = coord.getRandomizedPoint("autoBattleClick", 10, 5);
			text = "autoBattleCheck";
			if (i > 0)
			{
				safeSpotMightBeNeeded = true;
			}
			doAutoBattle(randomizedPoint, safeSpotMightBeNeeded);
			FoeTools.outInfo("autobattle click");
			_ = FoeTools.SafetyBreak;
			int howLong = 70;
			int howLong2 = 20;
			FoeTools.Wait(120);
			bool flag = false;
			bool pixelHunt = Settings.Instance.PixelHunt;
			int num3 = 0;
			int num4 = 0;
			num = 0;
			do
			{
				if (pixelHunt)
				{
					text3 = colorCheck.WaitForAnyColor(checkUnits, 10);
				}
				else
				{
					text3 = null;
					if (PixelTools.pointIsNull(patternMatchTopLeft) || PixelTools.pointIsNull(patternMatchBotRight))
					{
						patternMatchTopLeft = battleScrTopLeft;
						patternMatchBotRight = battleScrBotRight;
					}
					val2 = PixelTools.CaptureScreen(battleScrTopLeft, battleScrBotRight);
					_ = FoeTools.RunDebug;
					text3 = imgBattleResult("onHold", val2, text3);
					if (text != "autoBattleCheck")
					{
						text3 = imgBattleResult("autoBattleCheck", val2, text3);
						if (text3 == "autoBattleCheck")
						{
							num = num;
						}
					}
					text3 = imgBattleResult("nextRound", val2, text3);
					text3 = imgBattleResult("expeReward", val2, text3);
					text3 = imgBattleResult("gotReward", val2, text3);
					switch (text)
					{
					case "autoBattleCheck":
					case "gotReward":
					case null:
					case "nextRound":
					case "newExpedition":
						text3 = imgBattleResult("fightOk", val2, text3);
						break;
					}
					if (text == "gotReward" || text == null)
					{
						text3 = imgBattleResult("expeFinished", val2, text3);
					}
					if (text3 == null)
					{
						for (int j = 0; j < 2; j++)
						{
							if (j == 0)
							{
								patternMatchTopLeft = coord.MoveRelative(new Point(-300, -200), q, coord.useZoom, noNegative: true);
								patternMatchBotRight = coord.MoveRelative(new Point(300, 200), q, coord.useZoom, noNegative: true);
								val2 = PixelTools.CaptureScreen(battleScrTopLeft, battleScrBotRight);
							}
							if (j == 1)
							{
								patternMatchTopLeft = new Point(0, 0);
								patternMatchBotRight = new Point(PixelTools.MaxX, PixelTools.MaxY);
								val2 = PixelTools.CaptureScreen();
							}
							if (text3 == null && text == "newExpedition")
							{
								text3 = imgBattleResult("expeFight", val2, text3);
							}
							if (text3 == null && text == "newExpedition")
							{
								text3 = imgBattleResult("expeFight2", val2, text3);
							}
							if (text3 == null && text == "newExpedition")
							{
								text3 = imgBattleResult("expe7", val2, text3);
							}
							if (text3 == null && text == "newExpedition")
							{
								text3 = imgBattleResult("expe8", val2, text3);
							}
							if (text3 == null && (text == "newExpedition" || text == null))
							{
								text3 = imgBattleResult("expeOutOfTries", val2, text3);
							}
							bool flag2 = false;
							if (text == "expeReward" || text == null)
							{
								flag2 = true;
							}
							if (text == "gotReward" && text2 == "gotReward")
							{
								flag2 = true;
							}
							if (text == "fightOk" && text2 == "fightOk")
							{
								flag2 = true;
							}
							if (text == "newExpedition")
							{
								flag2 = true;
							}
							if (!(text3 == null && flag2))
							{
								continue;
							}
							text3 = imgBattleResult("newExpedition", val2, text3);
							if (text3 == "newExpedition")
							{
								PixelTools.MovePoint(ImageWorks.LastFound, patternMatchTopLeft);
								num4++;
								if (num4 > 3)
								{
									stopFighting = true;
									battleEndMessage = "seems like your out of expediton points!";
								}
							}
						}
					}
					if (text3 != "newExpedition")
					{
						num4 = 0;
					}
					if (num3 >= 2)
					{
						text = null;
					}
					if (text3 != null)
					{
						num3 = 0;
					}
					if (text3 == null)
					{
						num3++;
						if (text != "autoBattleCheck" && text != "nextRound")
						{
							val = ImageWorks.getBmp("./images/chfight.png", coord.zoomX, coord.zoomY);
							if (ImageWorks.FindTemplateEmguExist(val2, val))
							{
								if (false)
								{
									FoeTools.Wait(50);
									Bitmap sourceBitmap = PixelTools.CaptureScreen(battleScrTopLeft, battleScrBotRight);
									val = ImageWorks.getBmp("./images/chwait.png", coord.zoomX, coord.zoomY);
									if (!ImageWorks.FindTemplateEmguExist(sourceBitmap, val))
									{
										Bitmap sourceBitmap2 = PixelTools.CaptureScreen(patternMatchTopLeft, battleScrBotRight);
										val = ImageWorks.getBmp("./images/chfight.png", coord.zoomX, coord.zoomY);
										if (ImageWorks.FindTemplateEmguExist(sourceBitmap2, val))
										{
											text3 = "startFight";
										}
										else if (FoeTools.BattleDebug)
										{
											FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " startFight check, prev " + text + ", planej poplach1, bylo tam neco jinyho", "c:\\mik\\foebattleresult.txt");
										}
									}
									else if (FoeTools.BattleDebug)
									{
										FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " startFight check,prev " + text + ", planej poplach2, byl tam wait", "c:\\mik\\foebattleresult.txt");
									}
								}
								else
								{
									text3 = "startFight";
								}
							}
						}
					}
					if (FoeTools.BattleDebug)
					{
						FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " " + text3 + " prev: " + text, "c:\\mik\\foebattleresult.txt");
					}
				}
				Point p = new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
				FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} {text3} prev: {text}  {coord.DistanceFrom(p, patternMatchTopLeft)}");
				if (text3 == "expeOutOfTries")
				{
					stopFighting = true;
					battleEndMessage = "seems like your out of expediton points, buy some or wait and start again!";
				}
				if (text3 == "expeFinished")
				{
					stopFighting = true;
					battleEndMessage = "seems like you reached expedition end, collect rewards and proceed to next level!";
				}
				if (text3 == "newExpedition")
				{
					Point p2 = new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
					Point q2 = new Point(0, 120);
					p2 = coord.MoveAbsoluteRelative(p2, q2, coord.useZoom);
					Point randomizedPoint2 = coord.getRandomizedPoint(p2, 20, 2);
					MouseTools.ClickMouse(randomizedPoint2);
					FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked exp. spot {randomizedPoint2}");
					FoeTools.Wait(500);
					FoeTools.outInfo("expedition click");
					q = p2;
				}
				if (text3 == "expeFight" || text3 == "expeFight2")
				{
					Point randomizedPoint2 = new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
					Point q3 = new Point(60, 80);
					randomizedPoint2 = coord.MoveAbsoluteRelative(randomizedPoint2, q3, coord.useZoom);
					randomizedPoint2 = coord.getRandomizedPoint(randomizedPoint2, 20, 2);
					MouseTools.ClickMouse(randomizedPoint2);
					FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked exp fight {randomizedPoint2}");
					FoeTools.Wait(howLong);
					FoeTools.Wait(500);
					FoeTools.outInfo("expedition fight click");
				}
				if (text3 == "expe7" || text3 == "expe7")
				{
					Point randomizedPoint2 = new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
					Point q4 = new Point(5, 85);
					if (text3 == "expe8")
					{
						q4 = new Point(56, 88);
					}
					randomizedPoint2 = coord.MoveAbsoluteRelative(randomizedPoint2, q4, coord.useZoom);
					randomizedPoint2 = coord.getRandomizedPoint(randomizedPoint2, 20, 2);
					MouseTools.ClickMouse(randomizedPoint2);
					FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked exp fight {randomizedPoint2}");
					FoeTools.Wait(howLong);
					FoeTools.Wait(500);
					FoeTools.outInfo("expedition fight click");
				}
				if (text3 == "nextRound")
				{
					randomizedPoint = coord.getRandomizedPoint("nextBattleRoundClick", 20, 2);
					doAutoBattle(randomizedPoint);
					FoeTools.outInfo("next round click");
					FoeTools.Wait(howLong);
					FoeTools.Wait(500);
				}
				else if (text3 == "fightOk")
				{
					FoeTools.Wait(howLong);
					checkLoss();
					Point randomizedPoint2 = coord.getRandomizedPoint("fightOkClick", 20, 2);
					if (val2 != null && !Settings.Instance.EscBattlesKey)
					{
						Point? point = ImageWorks.FindTemplateEmgu(ImageWorks.getBmp("./images/chok.png", coord.zoomX, coord.zoomY), val2);
						if (point.HasValue)
						{
							Point value = point.Value;
							value.X += patternMatchTopLeft.X;
							value.Y += patternMatchTopLeft.Y;
							randomizedPoint2 = coord.MoveRelative(new Point(5, 5), value);
							randomizedPoint2 = coord.MoveRelative(new Point(50, 10), value, coord.useZoom);
						}
					}
					doEscWindow(randomizedPoint2);
					FoeTools.outInfo("fight ok click");
					FoeTools.Wait(howLong);
					FoeTools.Wait(500);
					flag = true;
				}
				if (text3 == "gotReward" || text3 == "expeReward")
				{
					Point randomizedPoint2 = coord.getRandomizedPoint("rewardOkClick", 20, 2);
					doEscWindow(randomizedPoint2);
					FoeTools.Wait(howLong);
					FoeTools.Wait(500);
					FoeTools.outInfo("reward click");
					flag = true;
				}
				if (text3 == "startFight")
				{
					if (text != "gotReward" && text != "fightOk" && text != "fightOk2" && text != "nextRound")
					{
						text3 = "onHold";
					}
					else
					{
						doStartFight();
						FoeTools.Wait(howLong);
					}
				}
				if (text3 == "startFight2")
				{
					if (text != "gotReward" && text != "fightOk" && text != "fightOk2")
					{
						text3 = "onHold";
					}
					else
					{
						doStartFight();
						FoeTools.Wait(howLong);
					}
				}
				if (text3 == "autoBattleCheck")
				{
					num2++;
					if (flag)
					{
						if (!fillUnitsWithCheck(checkForInv: false, minUnits))
						{
							stopFighting = true;
							battleEndMessage = "out of locked units";
						}
						else
						{
							doAutoBattle(randomizedPoint, safeSpotMightBeNeeded: true);
							FoeTools.outInfo("autobattle click");
							FoeTools.Wait(howLong);
						}
					}
				}
				if (text3 == "autoBattleNoUnits")
				{
					if (flag)
					{
						FoeTools.outInfo("next battle no units");
						break;
					}
					num = safetyRescue();
				}
				if (text3 != null)
				{
					num = 0;
					rescueStopWatch.Restart();
				}
				if (num > 3)
				{
					num = safetyRescue();
				}
				if (text3 != null)
				{
					FoeTools.Wait(howLong2);
				}
				num++;
				text2 = text;
				if (text3 != null && text3 != "onHold" && text3 != text)
				{
					text = text3;
				}
				if (lossesInRow > 2)
				{
					stopFighting = true;
					battleEndMessage = "too many losses in a row!";
					tooManyLosses = true;
				}
			}
			while (!stopFighting);
		}
		if (battleEndMessage != "")
		{
			FoeTools.outInfo("jobs done, " + battleEndMessage);
		}
		else
		{
			FoeTools.outInfo($"jobs done, {i} fought");
		}
	}

	public void GoGuildBattle(int battlesToDo = 20, int minUnits = 0)
	{
		tooManyLosses = false;
		string text = "";
		if (false)
		{
			GoBattleDebug();
			return;
		}
		int num = 0;
		ColorCheck colorCheck = new ColorCheck(3.0);
		string text2 = null;
		Bitmap val = null;
		Bitmap val2 = null;
		Bitmap val3 = null;
		Bitmap val4 = null;
		ColorCheckUnit item = new ColorCheckUnit("autoBattleCheck", coord.get("autoBattleCheck"), new List<Color> { Color.FromArgb(183, 62, 44) });
		ColorCheckUnit item2 = new ColorCheckUnit("autoBattleNoUnits", coord.get("autoBattleCheck"), new List<Color>
		{
			Color.FromArgb(101, 101, 101),
			Color.FromArgb(99, 99, 99),
			Color.FromArgb(112, 112, 112)
		});
		ColorCheckUnit item3 = new ColorCheckUnit("nextRound", coord.get("multiRoundBattleCheck"), new List<Color>
		{
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(147, 79, 31),
			Color.FromArgb(145, 78, 31)
		});
		ColorCheckUnit item4 = new ColorCheckUnit("fightOk", coord.get("fightOkCheck"), new List<Color>
		{
			Color.FromArgb(86, 132, 30),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(83, 127, 29)
		});
		ColorCheckUnit item5 = new ColorCheckUnit("fightOk2", coord.get("fightOkCheck2"), new List<Color>
		{
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(83, 127, 29)
		});
		ColorCheckUnit item6 = new ColorCheckUnit("gotReward", coord.get("rewardCheck"), new List<Color>
		{
			Color.FromArgb(146, 78, 31),
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(162, 92, 38)
		});
		ColorCheckUnit item7 = new ColorCheckUnit("startFight", coord.get("goFightCheck"), new List<Color>
		{
			Color.FromArgb(154, 74, 7),
			Color.FromArgb(151, 74, 7)
		});
		ColorCheckUnit item8 = new ColorCheckUnit("startFight2", coord.get("goFightCheckTagginOn"), new List<Color>
		{
			Color.FromArgb(215, 156, 42),
			Color.FromArgb(213, 151, 39)
		});
		List<ColorCheckUnit> checkUnits = new List<ColorCheckUnit> { item3, item4, item5, item6, item7, item8, item, item2 };
		if (FoeTools.fightUntilOCR)
		{
			battlesToDo = 99999;
		}
		stopFighting = false;
		lossesInRow = 0;
		battleEndMessage = "";
		LastMaxBattleScore = 0;
		LastBattleTotal = 0;
		patternMatchTopLeft = battleScrTopLeft;
		patternMatchBotRight = battleScrBotRight;
		doAutoBattleFocus();
		rescueStopWatch.Start();
		bool safeSpotMightBeNeeded = false;
		int i;
		for (i = 0; i < battlesToDo; i++)
		{
			if (stopFighting)
			{
				break;
			}
			FoeTools.outInfo($"battle #{i + 1} starting");
			Point randomizedPoint = coord.getRandomizedPoint("autoBattleClick", 10, 5);
			text = "autoBattleCheck";
			if (i > 0)
			{
				safeSpotMightBeNeeded = true;
			}
			FoeTools.Wait(500);
			doAutoBattle(randomizedPoint, safeSpotMightBeNeeded);
			_ = FoeTools.SafetyBreak;
			int howLong = 70;
			int howLong2 = 20;
			FoeTools.Wait(120);
			bool flag = false;
			bool pixelHunt = Settings.Instance.PixelHunt;
			num = 0;
			while (!FoeTools.ts.Token.IsCancellationRequested)
			{
				if (pixelHunt)
				{
					text2 = colorCheck.WaitForAnyColor(checkUnits, 10);
					FoeTools.outInfo("pixel hunt no longer supported");
					return;
				}
				text2 = null;
				val3 = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
				text2 = imgBattleResult("onHold", val3, text2);
				if (text == "startFight")
				{
					text2 = imgBattleResult("autoBattleCheck", val3, text2);
				}
				if (text == "autoBattleCheck")
				{
					text2 = imgBattleResult("nextRound", val3, text2);
				}
				if (text != "startFight" && text != "fightOk")
				{
					text2 = imgBattleResult("fightOk", val3, text2);
					if (text2 == "nextRound")
					{
						_ = text == "nextRound";
					}
				}
				text2 = imgBattleResult("gotReward", val3, text2);
				if (FoeTools.RunDebug)
				{
					FoeTools.RunDebug = FoeTools.RunDebug;
				}
				if (text2 == null && text != "autoBattleCheck" && text != "nextRound")
				{
					val = ImageWorks.getBmp("./images/chfight.png", coord.zoomX, coord.zoomY);
					val2 = ImageWorks.getBmp("./images/gb-def.png", coord.zoomX, coord.zoomY);
					if (ImageWorks.FindTemplateEmguExist(val3, val) || ImageWorks.FindTemplateEmguExist(val3, val2))
					{
						if (false)
						{
							FoeTools.Wait(50);
							Bitmap sourceBitmap = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
							val = ImageWorks.getBmp("./images/chwait.png", coord.zoomX, coord.zoomY);
							if (!ImageWorks.FindTemplateEmguExist(sourceBitmap, val))
							{
								val4 = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
								val = ImageWorks.getBmp("./images/chfight.png", coord.zoomX, coord.zoomY);
								val2 = ImageWorks.getBmp("./images/gb-def.png", coord.zoomX, coord.zoomY);
								if (ImageWorks.FindTemplateEmguExist(val4, val) || ImageWorks.FindTemplateEmguExist(val4, val2))
								{
									text2 = "startFight";
								}
								else if (FoeTools.BattleDebug)
								{
									FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " startFight check, prev " + text + ", planej poplach1, bylo tam neco jinyho", "c:\\mik\\foebattleresult.txt");
								}
							}
							else if (FoeTools.BattleDebug)
							{
								FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " startFight check,prev " + text + ", planej poplach2, byl tam wait", "c:\\mik\\foebattleresult.txt");
							}
						}
						else
						{
							text2 = "startFight";
						}
					}
				}
				if (unStuck(text2) == "stall stuck")
				{
					text = "";
					FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stall stuck");
				}
				new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
				bool flag2 = false;
				if (text2 == "nextRound")
				{
					randomizedPoint = coord.getRandomizedPoint("nextBattleRoundClick", 20, 2);
					doAutoBattle(randomizedPoint, safeSpotMightBeNeeded: false, isNextRound: true);
					FoeTools.Wait(howLong);
				}
				else if (text2 == "fightOk")
				{
					FoeTools.Wait(howLong);
					checkLoss();
					Point b = coord.getRandomizedPoint("fightOkClick", 20, 2);
					if (val3 != null && !Settings.Instance.EscBattlesKey)
					{
						Point? point = ImageWorks.FindTemplateEmgu(ImageWorks.getBmp("./images/chok.png", coord.zoomX, coord.zoomY), val3);
						if (point.HasValue)
						{
							Point value = point.Value;
							value.X += patternMatchTopLeft.X;
							value.Y += patternMatchTopLeft.Y;
							b = coord.MoveRelative(new Point(5, 5), value);
							b = coord.MoveRelative(new Point(50, 10), value, coord.useZoom);
						}
					}
					doEscWindow(b);
					FoeTools.Wait(howLong);
					flag = true;
				}
				if (text2 == "gotReward")
				{
					Point b = coord.getRandomizedPoint("rewardOkClick", 20, 2);
					doEscWindow(b);
					FoeTools.Wait(howLong);
					flag = true;
				}
				if (text2 == "startFight")
				{
					if (text != "gotReward" && text != "fightOk" && text != "fightOk2" && text != "nextRound" && (!(text == "") || num <= 3))
					{
						text2 = "onHold";
						flag2 = true;
					}
					else
					{
						if (FoeTools.fightUntilOCR && processBattleScores())
						{
							stopFighting = true;
							battleEndMessage = "seems like district is almost finished!";
						}
						doStartFight();
						FoeTools.Wait(howLong);
					}
				}
				if (text2 == "startFight2")
				{
					if (text != "gotReward" && text != "fightOk" && text != "fightOk2")
					{
						text2 = "onHold";
						flag2 = true;
					}
					else
					{
						if (FoeTools.fightUntilOCR && processBattleScores())
						{
							stopFighting = true;
							battleEndMessage = "seems like district is almost finished!";
						}
						doStartFight();
						FoeTools.Wait(howLong);
					}
				}
				if (text2 == "autoBattleCheck")
				{
					if (flag)
					{
						break;
					}
					flag2 = true;
				}
				if (text2 == "autoBattleNoUnits")
				{
					if (flag)
					{
						FoeTools.outInfo("next battle no units");
						break;
					}
					flag2 = true;
					num = safetyRescue();
				}
				if (text2 != null && !flag2)
				{
					num = 0;
					rescueStopWatch.Restart();
				}
				if (num > 5)
				{
					num = safetyRescue();
				}
				FoeTools.Wait(howLong2);
				num++;
				if (text2 != null && text2 != "onHold" && text2 != text)
				{
					text = text2;
				}
			}
			if (!fillUnitsWithCheck(checkForInv: false, minUnits))
			{
				stopFighting = true;
				battleEndMessage = "out of locked units";
			}
			else
			{
				FoeTools.Wait(howLong);
			}
			if (lossesInRow > 2)
			{
				stopFighting = true;
				battleEndMessage = "too many losses in a row!";
				tooManyLosses = true;
			}
			if (stopFighting)
			{
				battlesToDo = 0;
			}
		}
		if (battleEndMessage != "")
		{
			FoeTools.outInfo("jobs done, " + battleEndMessage);
		}
		else
		{
			FoeTools.outInfo($"jobs done, {i} fought");
		}
	}

	public void GoQuantumBattle(int battlesToDo = 20, int minUnits = 0)
	{
		tooManyLosses = false;
		string empty = string.Empty;
		if (false)
		{
			GoBattleDebug();
			return;
		}
		int num = 0;
		string text = null;
		Bitmap val = null;
		Bitmap val2 = null;
		new ColorCheckUnit("autoBattleCheck", coord.get("autoBattleCheck"), new List<Color> { Color.FromArgb(183, 62, 44) });
		new ColorCheckUnit("autoBattleNoUnits", coord.get("autoBattleCheck"), new List<Color>
		{
			Color.FromArgb(101, 101, 101),
			Color.FromArgb(99, 99, 99),
			Color.FromArgb(112, 112, 112)
		});
		new ColorCheckUnit("nextRound", coord.get("multiRoundBattleCheck"), new List<Color>
		{
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(147, 79, 31),
			Color.FromArgb(145, 78, 31)
		});
		new ColorCheckUnit("fightOk", coord.get("fightOkCheck"), new List<Color>
		{
			Color.FromArgb(86, 132, 30),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(83, 127, 29)
		});
		new ColorCheckUnit("fightOk2", coord.get("fightOkCheck2"), new List<Color>
		{
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(83, 127, 29)
		});
		new ColorCheckUnit("gotReward", coord.get("rewardCheck"), new List<Color>
		{
			Color.FromArgb(146, 78, 31),
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(162, 92, 38)
		});
		new ColorCheckUnit("startFight", coord.get("goFightCheck"), new List<Color>
		{
			Color.FromArgb(154, 74, 7),
			Color.FromArgb(151, 74, 7)
		});
		new ColorCheckUnit("startFight2", coord.get("goFightCheckTagginOn"), new List<Color>
		{
			Color.FromArgb(215, 156, 42),
			Color.FromArgb(213, 151, 39)
		});
		battlesToDo = 99999;
		stopFighting = false;
		lossesInRow = 0;
		battleEndMessage = string.Empty;
		LastMaxBattleScore = 0;
		LastBattleTotal = 0;
		patternMatchTopLeft = battleScrTopLeft;
		patternMatchBotRight = battleScrBotRight;
		doAutoBattleFocus();
		rescueStopWatch.Start();
		bool safeSpotMightBeNeeded = false;
		int i;
		for (i = 0; i < battlesToDo; i++)
		{
			if (stopFighting)
			{
				break;
			}
			FoeTools.outInfo($"battle #{i + 1} starting");
			Point randomizedPoint = coord.getRandomizedPoint("autoBattleClick", 10, 5);
			empty = "autoBattleCheck";
			if (i > 0)
			{
				safeSpotMightBeNeeded = true;
			}
			FoeTools.Wait(500);
			doAutoBattle(randomizedPoint, safeSpotMightBeNeeded, isNextRound: false, quantum: true);
			int howLong = 70;
			int howLong2 = 20;
			FoeTools.Wait(120);
			bool flag = false;
			num = 0;
			while (!FoeTools.ts.Token.IsCancellationRequested)
			{
				text = null;
				val2 = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
				if (text == null)
				{
					text = imgBattleResult("histHub", PixelTools.CaptureScreen(), text);
				}
				if (text == null)
				{
					text = imgBattleResult("onHold", val2, text);
				}
				if (empty == "startFightQuant")
				{
					text = imgBattleResult("autoBattleCheck", val2, text);
				}
				if (empty == "autoBattleCheck")
				{
					text = imgBattleResult("nextRound", val2, text);
				}
				if (empty != "startFightQuantum" && empty != "fightOk")
				{
					text = imgBattleResult("fightOk", val2, text);
				}
				text = imgBattleResult("gotReward", val2, text);
				if (text == null)
				{
					text = imgBattleResult("gotQuantReward", val2, text);
					if (text == "gotQuantReward")
					{
						text = "gotReward";
					}
				}
				if (text == null && empty != "autoBattleCheck" && empty != "nextRound")
				{
					val = ImageWorks.getBmp("./images/chquant.png", coord.zoomX, coord.zoomY);
					if (ImageWorks.FindTemplateEmguExist(val2, val))
					{
						text = "startFightQuant";
					}
				}
				if (text == null)
				{
					text = imgBattleResult("quantOOA", val2, text);
				}
				if (unStuck(text) == "stall stuck")
				{
					empty = string.Empty;
					FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stall stuck");
				}
				new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
				bool flag2 = false;
				switch (text)
				{
				case "nextRound":
					randomizedPoint = coord.getRandomizedPoint("nextBattleRoundClick", 20, 2);
					doAutoBattle(randomizedPoint, safeSpotMightBeNeeded: false, isNextRound: true, quantum: true);
					FoeTools.Wait(howLong);
					goto default;
				case "fightOk":
				{
					FoeTools.Wait(howLong);
					checkLoss();
					Point b = coord.getRandomizedPoint("fightOkClick", 20, 2);
					if (val2 != null && !Settings.Instance.EscBattlesKey)
					{
						Point? point = ImageWorks.FindTemplateEmgu(ImageWorks.getBmp("./images/chok.png", coord.zoomX, coord.zoomY), val2);
						if (point.HasValue)
						{
							Point value = point.Value;
							value.X += patternMatchTopLeft.X;
							value.Y += patternMatchTopLeft.Y;
							b = coord.MoveRelative(new Point(50, 10), value, coord.useZoom);
						}
					}
					doEscWindow(b);
					FoeTools.Wait(howLong);
					flag = true;
					goto default;
				}
				case "gotReward":
				{
					Point b = coord.getRandomizedPoint("rewardOkClick", 20, 2);
					doEscWindow(b);
					FoeTools.Wait(howLong);
					flag = true;
					goto default;
				}
				case "startFightQuant":
					if (empty != "gotReward" && empty != "fightOk" && empty != "fightOk2" && empty != "nextRound" && (!(empty == string.Empty) || num <= 3))
					{
						text = "onHold";
						flag2 = true;
					}
					else
					{
						doStartFightQuant();
						FoeTools.Wait(howLong);
					}
					goto default;
				case "autoBattleCheck":
					if (flag)
					{
						break;
					}
					flag2 = true;
					goto default;
				case "autoBattleNoUnits":
					FoeTools.outInfo("out of locked units detected");
					stopFighting = true;
					battleEndMessage = "out of locked units";
					break;
				case "quantOOA":
					FoeTools.outInfo("out of action points detected");
					stopFighting = true;
					battleEndMessage = "out of action points detected";
					break;
				default:
					if (text != null && !flag2)
					{
						num = 0;
						rescueStopWatch.Restart();
					}
					if (num > 5)
					{
						num = safetyRescue();
					}
					FoeTools.Wait(howLong2);
					num++;
					if (text != null && text != "onHold" && text != empty)
					{
						empty = text;
					}
					continue;
				case "histHub":
					break;
				}
				break;
			}
			if (!stopFighting)
			{
				if (!fillUnitsWithCheck(checkForInv: false, minUnits, quantum: true))
				{
					stopFighting = true;
					battleEndMessage = "out of locked units";
				}
				else
				{
					FoeTools.Wait(howLong);
				}
			}
			if (lossesInRow > 2)
			{
				stopFighting = true;
				battleEndMessage = "too many losses in a row!";
				tooManyLosses = true;
			}
			if (stopFighting)
			{
				battlesToDo = 0;
			}
		}
		if (!string.IsNullOrEmpty(battleEndMessage))
		{
			FoeTools.outInfo("jobs done, " + battleEndMessage);
		}
		else
		{
			FoeTools.outInfo($"jobs done, {i} fought");
		}
	}

	public void GoQuantumDebugResult(int battlesToDo = 20, int minUnits = 0)
	{
		tooManyLosses = false;
		string empty = string.Empty;
		if (false)
		{
			GoBattleDebug();
			return;
		}
		int num = 0;
		string text = null;
		Bitmap val = null;
		Bitmap val2 = null;
		new ColorCheckUnit("autoBattleCheck", coord.get("autoBattleCheck"), new List<Color> { Color.FromArgb(183, 62, 44) });
		new ColorCheckUnit("autoBattleNoUnits", coord.get("autoBattleCheck"), new List<Color>
		{
			Color.FromArgb(101, 101, 101),
			Color.FromArgb(99, 99, 99),
			Color.FromArgb(112, 112, 112)
		});
		new ColorCheckUnit("nextRound", coord.get("multiRoundBattleCheck"), new List<Color>
		{
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(147, 79, 31),
			Color.FromArgb(145, 78, 31)
		});
		new ColorCheckUnit("fightOk", coord.get("fightOkCheck"), new List<Color>
		{
			Color.FromArgb(86, 132, 30),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(83, 127, 29)
		});
		new ColorCheckUnit("fightOk2", coord.get("fightOkCheck2"), new List<Color>
		{
			Color.FromArgb(90, 139, 32),
			Color.FromArgb(89, 136, 31),
			Color.FromArgb(83, 127, 29)
		});
		new ColorCheckUnit("gotReward", coord.get("rewardCheck"), new List<Color>
		{
			Color.FromArgb(146, 78, 31),
			Color.FromArgb(151, 83, 34),
			Color.FromArgb(162, 92, 38)
		});
		new ColorCheckUnit("startFight", coord.get("goFightCheck"), new List<Color>
		{
			Color.FromArgb(154, 74, 7),
			Color.FromArgb(151, 74, 7)
		});
		new ColorCheckUnit("startFight2", coord.get("goFightCheckTagginOn"), new List<Color>
		{
			Color.FromArgb(215, 156, 42),
			Color.FromArgb(213, 151, 39)
		});
		battlesToDo = 99999;
		stopFighting = false;
		lossesInRow = 0;
		battleEndMessage = string.Empty;
		LastMaxBattleScore = 0;
		LastBattleTotal = 0;
		patternMatchTopLeft = battleScrTopLeft;
		patternMatchBotRight = battleScrBotRight;
		doAutoBattleFocus();
		rescueStopWatch.Start();
		bool safeSpotMightBeNeeded = false;
		int i;
		for (i = 0; i < battlesToDo; i++)
		{
			if (stopFighting)
			{
				break;
			}
			FoeTools.outInfo($"battle #{i + 1} starting");
			Point randomizedPoint = coord.getRandomizedPoint("autoBattleClick", 10, 5);
			empty = "autoBattleCheck";
			if (i > 0)
			{
				safeSpotMightBeNeeded = true;
			}
			FoeTools.Wait(500);
			doAutoBattle(randomizedPoint, safeSpotMightBeNeeded, isNextRound: false, quantum: true);
			int howLong = 70;
			int howLong2 = 20;
			FoeTools.Wait(120);
			bool flag = false;
			num = 0;
			while (!FoeTools.ts.Token.IsCancellationRequested)
			{
				text = null;
				val2 = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
				text = imgBattleResult("onHold", val2, text);
				if (empty == "startFightQuantum")
				{
					text = imgBattleResult("autoBattleCheck", val2, text);
				}
				if (empty == "autoBattleCheck")
				{
					text = imgBattleResult("nextRound", val2, text);
				}
				if (empty != "startFightQuantum" && empty != "fightOk")
				{
					text = imgBattleResult("fightOk", val2, text);
					if (text == "nextRound")
					{
						_ = empty == "nextRound";
					}
				}
				text = imgBattleResult("gotReward", val2, text);
				if (text == null)
				{
					text = imgBattleResult("gotQuantReward", val2, text);
					if (text == "gotQuantReward")
					{
						text = "gotReward";
					}
				}
				if (FoeTools.RunDebug)
				{
					FoeTools.RunDebug = FoeTools.RunDebug;
				}
				if (text == null && empty != "autoBattleCheck" && empty != "nextRound")
				{
					val = ImageWorks.getBmp("./images/chquant.png", coord.zoomX, coord.zoomY);
					if (ImageWorks.FindTemplateEmguExist(val2, val))
					{
						if (false)
						{
							FoeTools.Wait(50);
							Bitmap sourceBitmap = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
							val = ImageWorks.getBmp("./images/chwait.png", coord.zoomX, coord.zoomY);
							if (!ImageWorks.FindTemplateEmguExist(sourceBitmap, val))
							{
								Bitmap sourceBitmap2 = PixelTools.CaptureScreen(patternMatchTopLeft, patternMatchBotRight);
								val = ImageWorks.getBmp("./images/chquant.png", coord.zoomX, coord.zoomY);
								if (ImageWorks.FindTemplateEmguExist(sourceBitmap2, val))
								{
									text = "startFightQuant";
								}
								else if (FoeTools.BattleDebug)
								{
									FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " startFight check, prev " + empty + ", false positive 1", "c:\\mik\\foebattleresult.txt");
								}
							}
							else if (FoeTools.BattleDebug)
							{
								FoeTools.WriteLogFile(FoeTools.getStopky(zeros: true) + " startFight check, prev " + empty + ", false positive 2 (wait screen)", "c:\\mik\\foebattleresult.txt");
							}
						}
						else
						{
							text = "startFightQuant";
						}
					}
				}
				if (unStuck(text) == "stall stuck")
				{
					empty = string.Empty;
					FoeTools.writeDebugFile(FoeTools.getStopky(zeros: true) + " stall stuck");
				}
				new Point(ImageWorks.LastFound.X + patternMatchTopLeft.X, ImageWorks.LastFound.Y + patternMatchTopLeft.Y);
				bool flag2 = false;
				switch (text)
				{
				case "nextRound":
					randomizedPoint = coord.getRandomizedPoint("nextBattleRoundClick", 20, 2);
					doAutoBattle(randomizedPoint, safeSpotMightBeNeeded: false, isNextRound: true, quantum: true);
					FoeTools.Wait(howLong);
					goto default;
				case "fightOk":
				{
					FoeTools.Wait(howLong);
					checkLoss();
					Point b = coord.getRandomizedPoint("fightOkClick", 20, 2);
					if (val2 != null && !Settings.Instance.EscBattlesKey)
					{
						Point? point = ImageWorks.FindTemplateEmgu(ImageWorks.getBmp("./images/chok.png", coord.zoomX, coord.zoomY), val2);
						if (point.HasValue)
						{
							Point value = point.Value;
							value.X += patternMatchTopLeft.X;
							value.Y += patternMatchTopLeft.Y;
							b = coord.MoveRelative(new Point(50, 10), value, coord.useZoom);
						}
					}
					doEscWindow(b);
					FoeTools.Wait(howLong);
					flag = true;
					goto default;
				}
				case "gotReward":
				{
					Point b = coord.getRandomizedPoint("rewardOkClick", 20, 2);
					doEscWindow(b);
					FoeTools.Wait(howLong);
					flag = true;
					goto default;
				}
				case "startFightQuant":
					if (empty != "gotReward" && empty != "fightOk" && empty != "fightOk2" && empty != "nextRound" && (!(empty == string.Empty) || num <= 3))
					{
						text = "onHold";
						flag2 = true;
					}
					else
					{
						doStartFightQuant();
						FoeTools.Wait(howLong);
					}
					goto default;
				case "autoBattleCheck":
					if (flag)
					{
						break;
					}
					flag2 = true;
					goto default;
				case "autoBattleNoUnits":
					FoeTools.outInfo("out of locked units detected");
					stopFighting = true;
					battleEndMessage = "out of locked units";
					break;
				default:
					if (text != null && !flag2)
					{
						num = 0;
						rescueStopWatch.Restart();
					}
					if (num > 5)
					{
						num = safetyRescue();
					}
					FoeTools.Wait(howLong2);
					num++;
					if (text != null && text != "onHold" && text != empty)
					{
						empty = text;
					}
					continue;
				}
				break;
			}
			if (!stopFighting)
			{
				if (!fillUnitsWithCheck(checkForInv: false, minUnits, quantum: true))
				{
					stopFighting = true;
					battleEndMessage = "out of locked units";
				}
				else
				{
					FoeTools.Wait(howLong);
				}
			}
			if (lossesInRow > 2)
			{
				stopFighting = true;
				battleEndMessage = "too many losses in a row!";
				tooManyLosses = true;
			}
			if (stopFighting)
			{
				battlesToDo = 0;
			}
		}
		if (!string.IsNullOrEmpty(battleEndMessage))
		{
			FoeTools.outInfo("jobs done, " + battleEndMessage);
		}
		else
		{
			FoeTools.outInfo($"jobs done, {i} fought");
		}
	}

	public void doStartFightQuant()
	{
		Point randomizedPoint = coord.getRandomizedPoint("goFightQuantumClick", 20, 10);
		MouseTools.ClickMouse(randomizedPoint);
		FoeTools.writeDebugFile($"{FoeTools.getStopky(zeros: true)} mouse clicked doStartFight {randomizedPoint}");
	}
}
