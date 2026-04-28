using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;

namespace FoeHelper2;

internal class Coordinates
{
	public delegate void ApplyMod();

	private class CoordinatesData
	{
		public Dictionary<string, Point> SouradniceThis { get; set; }

		public Dictionary<string, List<Point>> Souradnice { get; set; }
	}

	private Dictionary<string, Point> souradniceThis;

	private Dictionary<string, List<Point>> souradnice;

	public ApplyMod fromBottom;

	public ApplyMod useZoom;

	public ApplyMod useZoomX;

	public ApplyMod zoomBottom;

	public ApplyMod fromCudlik;

	private Point activePoint;

	public Point detekovanyPosun = new Point(0, 0);

	private Random rand = new Random();

	public float zoomX;

	public float zoomY;

	public float relResX;

	public float relResY;

	public Coordinates()
	{
		souradniceThis = new Dictionary<string, Point>();
		souradnice = new Dictionary<string, List<Point>>();
		fromBottom = relativeCoord;
		fromCudlik = relativeByPoint;
		useZoom = zoomCoord;
		useZoomX = zoomCoordX;
		zoomBottom = (ApplyMod)Delegate.Combine(fromBottom, useZoom);
	}

	public void setZoom(float rx, float ry)
	{
		if ((double)rx < 0.1)
		{
			rx = 1f;
		}
		zoomX = rx;
		if ((double)ry < 0.1)
		{
			ry = rx;
		}
		zoomY = ry;
	}

	public float getRelativeResolutionX()
	{
		return (float)PixelTools.MaxX / 1920f;
	}

	public float getRelativeResolutionY()
	{
		return (float)PixelTools.MaxY / 1080f;
	}

	public void relativeByPointDiff()
	{
		Point point = get("relatedPointFrom");
		Point point2 = get("relatedPointTo");
		activePoint.X = point2.X + (int)((float)(activePoint.X - point.X) * zoomX);
		activePoint.Y = point2.Y + (int)((float)(activePoint.Y - point.Y) * zoomY);
	}

	public void relativeByPoint()
	{
		Point point = get("relatedPointTo");
		activePoint.X = point.X + (int)((float)activePoint.X * zoomX);
		activePoint.Y = point.Y + (int)((float)activePoint.Y * zoomY);
	}

	public void relativeCoord()
	{
		activePoint.Y = get("resThis").Y - (int)((float)(get("resDefault").Y - activePoint.Y) * zoomY);
	}

	public void zoomCoord()
	{
		activePoint.X = (int)((float)activePoint.X * zoomX);
		activePoint.Y = (int)((float)activePoint.Y * zoomY);
	}

	public void zoomCoordX()
	{
		activePoint.X = (int)((float)activePoint.X * zoomX);
	}

	public void add(Point p, string name, ApplyMod Mod = null)
	{
		if (Mod != null)
		{
			activePoint = p;
			Mod();
			p = activePoint;
		}
		souradniceThis[name] = p;
	}

	public void addRandomize(Point p1, Point p2, string name, ApplyMod Mod = null)
	{
		int minValue = Math.Min(p1.X, p2.X);
		int maxValue = Math.Max(p1.X, p2.X);
		int minValue2 = Math.Min(p1.Y, p2.Y);
		int maxValue2 = Math.Max(p1.Y, p2.Y);
		Point value = new Point(rand.Next(minValue, maxValue), rand.Next(minValue2, maxValue2));
		if (Mod != null)
		{
			activePoint = value;
			Mod();
			value = activePoint;
		}
		souradniceThis[name] = value;
	}

	public Point getRandomizedPoint(Point p, int diffMaxX, int diffMaxY = -1)
	{
		int x = rand.Next(p.X - (int)((float)diffMaxX * zoomX), p.X + (int)((float)diffMaxX * zoomX));
		if (diffMaxY == -1)
		{
			diffMaxY = diffMaxX;
		}
		int y = rand.Next(p.Y - (int)((float)diffMaxY * zoomY), p.Y + (int)((float)diffMaxY * zoomY));
		return new Point(x, y);
	}

	public Point getAbsPoint(Point p)
	{
		int x = (int)((float)p.X * getRelativeResolutionX());
		int y = (int)((float)p.Y * getRelativeResolutionY());
		return new Point(x, y);
	}

	public Point getRandomizedPoint(string pointName, int diffMaxX, int diffMaxY = -1)
	{
		return getRandomizedPoint(souradniceThis[pointName], diffMaxX, diffMaxY);
	}

	public void add(int[,] c, string name)
	{
		souradnice[name] = new List<Point>();
		for (int i = 0; i < c.GetLength(0); i++)
		{
			souradnice[name].Add(new Point(c[i, 0], c[i, 1]));
		}
	}

	public void add(Point[] addPoints, string name)
	{
		souradnice[name] = new List<Point>(addPoints);
	}

	public bool coordExists(string key)
	{
		return souradniceThis.ContainsKey(key);
	}

	public Point get(string name)
	{
		return new Point(souradniceThis[name].X + detekovanyPosun.X, souradniceThis[name].Y + detekovanyPosun.Y);
	}

	public Point get(string name, ApplyMod Mod)
	{
		activePoint = souradniceThis[name];
		Mod();
		return activePoint;
	}

	public Point getMod(Point p, ApplyMod Mod)
	{
		activePoint = p;
		Mod();
		return activePoint;
	}

	public Point getMod(int x, int y, ApplyMod Mod)
	{
		activePoint = new Point(x, y);
		Mod();
		return activePoint;
	}

	public Rectangle getRectangle(Rectangle r, ApplyMod Mod)
	{
		activePoint = new Point(r.X, r.Y);
		Mod();
		Point point = activePoint;
		activePoint = new Point(r.X + r.Width, r.Y + r.Height);
		Mod();
		Point point2 = activePoint;
		return new Rectangle(point.X, point.Y, point2.X - point.X, point2.Y - point.Y);
	}

	public Rectangle getRectangle(int x, int y, int width, int height, ApplyMod Mod)
	{
		Rectangle r = new Rectangle(x, y, width, height);
		return getRectangle(r, Mod);
	}

	public Point getRelated(string name, Point relativePoint)
	{
		return new Point(souradniceThis[name].X + relativePoint.X, souradniceThis[name].Y + relativePoint.Y);
	}

	public Point[] getPointsArray(Point[] relatedPoints)
	{
		Point point = get("relatedPointFrom");
		for (int i = 0; i < relatedPoints.Length; i++)
		{
			relatedPoints[i].X -= point.X;
			relatedPoints[i].Y -= point.Y;
		}
		return relatedPoints;
	}

	public int[,] getArray(string name, ApplyMod Mod = null)
	{
		int[,] array = new int[souradnice[name].Count, 2];
		List<Point> list = souradnice[name];
		int num = 0;
		foreach (Point item in list)
		{
			activePoint = item;
			Mod?.Invoke();
			array[num, 0] = activePoint.X;
			array[num, 1] = activePoint.Y;
			num++;
		}
		return array;
	}

	public Point MoveRelative(Point p, string name, ApplyMod Mod = null, bool noNegative = false)
	{
		return MoveRelative(p, souradniceThis[name], Mod, noNegative);
	}

	public Point MoveRelative(Point p, Point q, ApplyMod Mod = null, bool noNegative = false)
	{
		activePoint = p;
		Mod?.Invoke();
		p = activePoint;
		Point result = new Point(p.X + q.X, p.Y + q.Y);
		if (noNegative)
		{
			if (result.X < 0)
			{
				result.X = 0;
			}
			if (result.Y < 0)
			{
				result.Y = 0;
			}
		}
		return result;
	}

	public int MoveRelativeX(int x, int xDiff, ApplyMod Mod = null)
	{
		activePoint = new Point(xDiff, 0);
		Mod?.Invoke();
		return activePoint.X + x;
	}

	public int MoveRelativeY(int y, int yDiff, ApplyMod Mod = null)
	{
		activePoint = new Point(0, yDiff);
		Mod?.Invoke();
		return activePoint.Y + y;
	}

	public Point MoveAbsoluteRelative(Point p, Point q, ApplyMod Mod = null)
	{
		activePoint = q;
		Mod?.Invoke();
		return new Point(p.X + activePoint.X, p.Y + activePoint.Y);
	}

	public Point DistanceFrom(Point p, Point q)
	{
		return new Point(p.X - q.X, p.Y - q.Y);
	}

	public string ToJson()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		CoordinatesData obj = new CoordinatesData
		{
			SouradniceThis = souradniceThis,
			Souradnice = souradnice
		};
		JsonSerializerOptions val = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		return JsonSerializer.Serialize<CoordinatesData>(obj, val);
	}

	public void SaveToJsonFile(string filePath)
	{
		File.WriteAllText(filePath, ToJson());
	}

	public void LoadFromJson(string json)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		JsonSerializerOptions val = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};
		CoordinatesData coordinatesData = JsonSerializer.Deserialize<CoordinatesData>(json, val);
		if (coordinatesData == null)
		{
			throw new InvalidOperationException("Invalid JSON for Coordinates.");
		}
		souradniceThis = coordinatesData.SouradniceThis ?? new Dictionary<string, Point>();
		souradnice = coordinatesData.Souradnice ?? new Dictionary<string, List<Point>>();
	}

	public void LoadFromJsonFile(string filePath)
	{
		string json = File.ReadAllText(filePath);
		LoadFromJson(json);
	}
}
