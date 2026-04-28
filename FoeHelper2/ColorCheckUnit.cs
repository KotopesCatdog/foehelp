using System.Collections.Generic;
using System.Drawing;

namespace FoeHelper2;

internal class ColorCheckUnit
{
	public List<Color> colorsSearch;

	public string name;

	public Point checkedPoint;

	public double diff;

	public Color foundColor;

	public ColorCheckUnit(string nm, Point p, List<Color> colors)
	{
		colorsSearch = colors;
		checkedPoint = p;
		name = nm;
	}
}
