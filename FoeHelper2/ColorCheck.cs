using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace FoeHelper2;

internal class ColorCheck
{
	private Color checkColor;

	private double difference;

	public int diffXdetected;

	public int diffYdetected;

	private bool positive;

	private int[] line;

	private List<Color> colorsSearch;

	private Color foundColor;

	private int lastX;

	private int lastY;

	public double precision { get; set; }

	public ColorCheck(double prec = 0.0, bool posit = true)
	{
		precision = prec;
		positive = posit;
	}

	public ColorCheck(string col, double prec = 0.0, bool posit = true)
	{
		checkColor = PixelTools.StrColorToColor(col);
		precision = prec;
		positive = posit;
	}

	public ColorCheck(Color color, double prec = 0.0, bool posit = true)
	{
		checkColor = color;
		precision = prec;
		positive = posit;
	}

	public ColorCheck(List<Color> colors, double prec = 0.0, bool posit = true)
	{
		colorsSearch = colors;
		precision = prec;
		positive = posit;
	}

	public ColorCheck(int xStart, int xEnd, int y)
	{
		line = PixelTools.GetHorizontalLineColors2(xStart, xEnd, y);
	}

	public ColorCheck(Point p1, Point p2)
	{
		line = PixelTools.GetHorizontalLineColors2(p1.X, p2.X, p1.Y);
	}

	public string getLastFound()
	{
		return $"{foundColor.A} {foundColor.R} {foundColor.G} {foundColor.B} {ColorTranslator.ToHtml(foundColor)} at  {lastX} {lastY}";
	}

	public bool Check(int x, int y)
	{
		lastX = x;
		lastY = y;
		foundColor = PixelTools.GetColorAt(x, y);
		difference = PixelTools.CompareColorsGetDelta(checkColor, foundColor);
		return difference <= precision == positive;
	}

	public bool Check(Point p)
	{
		lastX = p.X;
		lastY = p.Y;
		foundColor = PixelTools.GetColorAt(lastX, lastY);
		if (colorsSearch != null)
		{
			difference = colorsSearch.Min((Color c) => PixelTools.CompareColorsGetDelta(c, foundColor));
		}
		else
		{
			difference = PixelTools.CompareColorsGetDelta(checkColor, foundColor);
		}
		return difference <= precision == positive;
	}

	public bool WaitForColor(Point p, int cycles = 100, int xTolerance = 0, int yTolerance = 0)
	{
		if (FoeTools.colorWaiterDisable)
		{
			cycles = 1;
		}
		for (int i = 0; i < cycles; i++)
		{
			for (int j = p.X - xTolerance; j <= p.X + xTolerance; j++)
			{
				for (int k = p.Y - yTolerance; k <= p.Y + yTolerance; k++)
				{
					if (Check(new Point(j, k)))
					{
						diffXdetected = j - p.X;
						diffYdetected = k - p.Y;
						FoeTools.colorWaiterDisable = false;
						return true;
					}
				}
			}
			Thread.Sleep(100);
		}
		return false;
	}

	public bool WaitForColor(Point p, Color colorNeeded, int cycles = 100)
	{
		colorsSearch = null;
		checkColor = colorNeeded;
		return WaitForColor(p, cycles);
	}

	public bool WaitForColor(Point p, List<Color> colorNeeded, int cycles = 100)
	{
		colorsSearch = colorNeeded;
		return WaitForColor(p, cycles);
	}

	public bool WaitForColor(ColorCheckUnit cu, int cycles = 100, bool log = false, int xTolerance = 0, int yTolerance = 0)
	{
		colorsSearch = cu.colorsSearch;
		bool result = WaitForColor(cu.checkedPoint, cycles, xTolerance, yTolerance);
		cu.diff = difference;
		cu.foundColor = foundColor;
		if (log)
		{
			FoeTools.WriteLogFile($"{cu.name} {cu.diff} {cu.foundColor} {cu.checkedPoint}");
		}
		return result;
	}

	public string WaitForAnyColor(List<ColorCheckUnit> checkUnits, int cycles = 100)
	{
		if (FoeTools.colorWaiterDisable)
		{
			cycles = 1;
		}
		for (int i = 0; i < cycles; i++)
		{
			foreach (ColorCheckUnit checkUnit in checkUnits)
			{
				colorsSearch = checkUnit.colorsSearch;
				if (Check(checkUnit.checkedPoint))
				{
					FoeTools.colorWaiterDisable = false;
					return checkUnit.name;
				}
				checkUnit.diff = difference;
				checkUnit.foundColor = foundColor;
			}
			Thread.Sleep(100);
		}
		foreach (ColorCheckUnit checkUnit2 in checkUnits)
		{
			FoeTools.WriteLogFile($"{checkUnit2.name} {checkUnit2.diff} {checkUnit2.foundColor} {checkUnit2.checkedPoint}");
		}
		return null;
	}

	public bool ClickUntilColor(Point p, Point click)
	{
		for (int i = 0; i < 100; i++)
		{
			if (Check(p))
			{
				return true;
			}
			MouseTools.ClickMouse(click);
			Thread.Sleep(500);
		}
		return false;
	}

	public bool CheckLine(ColorCheck colCheck2)
	{
		if (line != null && colCheck2.line != null)
		{
			return line.SequenceEqual(colCheck2.line);
		}
		return false;
	}
}
