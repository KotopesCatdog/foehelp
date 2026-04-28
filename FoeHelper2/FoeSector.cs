using System.Drawing;

namespace FoeHelper2;

internal class FoeSector
{
	public Point SectorCoord { get; private set; }

	public int FightsCurrent { get; set; }

	public int FightsMax { get; set; }

	public FoeSector(Point coord, int max, int current)
	{
		SectorCoord = coord;
		FightsMax = max;
		FightsCurrent = current;
	}

	public bool IsFightable()
	{
		if (FightsCurrent >= FightsMax - 5)
		{
			return FightsMax == 0;
		}
		return true;
	}
}
