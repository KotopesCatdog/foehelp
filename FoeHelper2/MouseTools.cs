using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FoeHelper2;

internal static class MouseTools
{
	public struct MousePoint
	{
		public int X;

		public int Y;

		public MousePoint(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	private const int MOUSEEVENTF_LEFTDOWN = 2;

	private const int MOUSEEVENTF_LEFTUP = 4;

	private const int MOUSEEVENTF_RIGHTDOWN = 8;

	private const int MOUSEEVENTF_RIGHTUP = 16;

	private const int MOUSEEVENTF_WHEEL = 2048;

	private static Random random = new Random();

	private static int mouseSpeed = 18;

	private static int MovedX = 0;

	private static int MovedY = 0;

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	public static extern void mouse_event(uint dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetCursorPos(out MousePoint lpMousePoint);

	public static MousePoint GetCursorPosition()
	{
		if (!GetCursorPos(out var lpMousePoint))
		{
			lpMousePoint = new MousePoint(0, 0);
		}
		return lpMousePoint;
	}

	public static void ClickMouse(int x, int y, bool doubleClick = false)
	{
		MoveMouse(x, y);
		DoMouseClick(doubleClick);
	}

	public static void ClickMouse(Point p, bool doubleClick = false)
	{
		MoveMouse(p.X, p.Y);
		DoMouseClick(doubleClick);
	}

	public static void ClickMouse(int[] coords, bool doubleClick = false)
	{
		MoveMouse(coords[0], coords[1]);
		DoMouseClick(doubleClick);
	}

	public static void MoveMouse(int x, int y, int rx = 0, int ry = 0, bool doubleClick = false)
	{
		MousePoint lpMousePoint = default(MousePoint);
		GetCursorPos(out lpMousePoint);
		x += random.Next(rx);
		y += random.Next(ry);
		double num = 4.0;
		WindMouse(lpMousePoint.X, lpMousePoint.Y, x, y, 9.0, 3.0, 10.0 / num, 15.0 / num, 10.0 * num, 10.0 * num);
	}

	public static Point GetMousePoint()
	{
		MousePoint lpMousePoint = default(MousePoint);
		GetCursorPos(out lpMousePoint);
		return new Point(lpMousePoint.X, lpMousePoint.Y);
	}

	public static void RandomMove(int rxMin, int ryMin, int rxMax, int ryMax)
	{
		MousePoint lpMousePoint = default(MousePoint);
		GetCursorPos(out lpMousePoint);
		int num = random.Next(rxMin, rxMax);
		int num2 = random.Next(ryMin, ryMax);
		MoveMouse(lpMousePoint.X + num, lpMousePoint.Y + num2);
	}

	public static void MoveMouse(Point p, int rx = 0, int ry = 0)
	{
		MoveMouse(p.X, p.Y, rx, ry);
	}

	private static void WindMouse(double xs, double ys, double xe, double ye, double gravity, double wind, double minWait, double maxWait, double maxStep, double targetArea)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		int num5 = (int)Math.Round(xs);
		int num6 = (int)Math.Round(ys);
		double num7 = maxWait - minWait;
		double num8 = Math.Sqrt(2.0);
		double num9 = Math.Sqrt(3.0);
		double num10 = Math.Sqrt(5.0);
		double num11 = Hypot(xe - xs, ye - ys);
		while (num11 > 1.0)
		{
			wind = Math.Min(wind, num11);
			if (num11 >= targetArea)
			{
				int num12 = random.Next((int)Math.Round(wind) * 2 + 1);
				num = num / num9 + ((double)num12 - wind) / num10;
				num2 = num2 / num9 + ((double)num12 - wind) / num10;
			}
			else
			{
				num /= num8;
				num2 /= num8;
				maxStep = ((!(maxStep < 3.0)) ? (maxStep / num10) : ((double)random.Next(3) + 3.0));
			}
			num3 += num;
			num4 += num2;
			num3 += gravity * (xe - xs) / num11;
			num4 += gravity * (ye - ys) / num11;
			if (Hypot(num3, num4) > maxStep)
			{
				double num13 = maxStep / 2.0 + (double)random.Next((int)Math.Round(maxStep) / 2);
				double num14 = Hypot(num3, num4);
				num3 = num3 / num14 * num13;
				num4 = num4 / num14 * num13;
			}
			int num15 = (int)Math.Round(xs);
			int num16 = (int)Math.Round(ys);
			xs += num3;
			ys += num4;
			num11 = Hypot(xe - xs, ye - ys);
			num5 = (int)Math.Round(xs);
			num6 = (int)Math.Round(ys);
			if (num15 != num5 || num16 != num6)
			{
				SetCursorPos(num5, num6);
			}
			double num17 = Hypot(xs - (double)num15, ys - (double)num16);
			Thread.Sleep((int)Math.Round(num7 * (num17 / maxStep) + minWait));
		}
		int num18 = (int)Math.Round(xe);
		int num19 = (int)Math.Round(ye);
		if (num18 != num5 || num19 != num6)
		{
			SetCursorPos(num18, num19);
		}
	}

	private static double Hypot(double dx, double dy)
	{
		return Math.Sqrt(dx * dx + dy * dy);
	}

	public static void DoMouseClick(bool doubleClick = false)
	{
		int x = Cursor.Position.X;
		int y = Cursor.Position.Y;
		mouse_event(6u, x, y, 0, 0);
		if (doubleClick)
		{
			Thread.Sleep(50);
			mouse_event(6u, x, y, 0, 0);
		}
	}

	public static void DoMouseWheel(string move = "down")
	{
		if (move == "down")
		{
			mouse_event(2048u, 0, 0, -120, 0);
		}
		else
		{
			mouse_event(2048u, 0, 0, 120, 0);
		}
	}
}
