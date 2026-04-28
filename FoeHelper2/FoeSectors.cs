using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Tesseract;

namespace FoeHelper2;

internal class FoeSectors
{
	private List<FoeSector> sectorsList;

	public FoeSectors()
	{
		sectorsList = new List<FoeSector>();
	}

	public void AddSector(Point p, int fightMax, int fightsCurrent)
	{
		FoeSector sector = GetSector(p);
		if (sector == null)
		{
			sectorsList.Add(new FoeSector(p, fightMax, fightsCurrent));
			return;
		}
		sector.FightsMax = fightMax;
		sector.FightsCurrent = fightsCurrent;
	}

	public bool SectorIsFightable(Point p)
	{
		return sectorsList.FirstOrDefault((FoeSector s) => s.SectorCoord == p)?.IsFightable() ?? false;
	}

	public bool SectorExists(Point p)
	{
		return sectorsList.Any((FoeSector s) => s.SectorCoord == p);
	}

	public FoeSector GetSector(Point p)
	{
		return sectorsList.Find((FoeSector s) => s.SectorCoord == p);
	}

	public void OpenFightScreen(Point p)
	{
		MouseTools.ClickMouse(PixelTools.GetRandomizedPoint(p, 10));
		FoeTools.Wait(2500);
	}

	public void StartFight()
	{
		SendKeys.SendWait("a");
		FoeTools.Wait(1500);
	}

	public static List<Point> FilterUniquePoints(List<Point> points, int minDistance = 10)
	{
		List<Point> list = new List<Point>();
		foreach (Point p in points)
		{
			if (!list.Any((Point existing) => Math.Abs(existing.X - p.X) < minDistance && Math.Abs(existing.Y - p.Y) < minDistance))
			{
				list.Add(p);
			}
		}
		return list;
	}

	public int[] getBattleScores()
	{
		int[] array = new int[2];
		Coordinates coord = FoeTools.coord;
		Point q = coord.get("goFightCheck");
		Point p = new Point(184, -57);
		Point p2 = new Point(268, 40);
		p = coord.MoveRelative(p, q, coord.useZoom);
		p2 = coord.MoveRelative(p2, q, coord.useZoom);
		string text = FoeOCR.ReadNumbersCharles(PixelTools.CaptureScreen(p.X, p.Y, p2.X, p2.Y), 1, modif2: false, "", "1/234567890", (PageSegMode)6, 3f);
		char[] separator = new char[1] { '\n' };
		string[] array2 = text.Split(separator);
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
					array[0] = num;
					array[1] = num2;
					return array;
				}
			}
			catch
			{
			}
		}
		return array;
	}

	public void CloseFightScreen()
	{
		SendKeys.SendWait("{ESC}");
	}

	public void CloseStartFightScreen()
	{
		SendKeys.SendWait("{ESC}");
		FoeTools.Wait(2500);
	}

	public static void SendEsc()
	{
		SendKeys.SendWait("{ESC}");
	}
}
