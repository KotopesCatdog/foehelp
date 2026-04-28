using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace FoeHelper2;

public class DirectBitmap : IDisposable
{
	private bool disposed;

	public Bitmap Bitmap { get; private set; }

	public int[] Bits { get; private set; }

	public Color[,] BitsColors { get; private set; }

	public int Height { get; private set; }

	public int Width { get; private set; }

	protected GCHandle BitsHandle { get; private set; }

	public DirectBitmap(int width, int height)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		Width = width;
		Height = height;
		Bits = new int[width * height];
		BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		Bitmap = new Bitmap(width, height, width * 4, (PixelFormat)925707, BitsHandle.AddrOfPinnedObject());
	}

	public DirectBitmap()
	{
	}

	public bool isEqual(int[] cmpBits)
	{
		if (cmpBits.Length == Bits.Length)
		{
			return cmpBits.SequenceEqual(Bits);
		}
		return false;
	}

	public void SetPixel(int x, int y, Color colour)
	{
		int num = x + y * Width;
		Bits[num] = colour.ToArgb();
	}

	public void LoadBitmap(Bitmap bmp)
	{
		Bitmap bitmap = Bitmap;
		if (bitmap != null)
		{
			((Image)bitmap).Dispose();
		}
		if (BitsHandle.IsAllocated)
		{
			BitsHandle.Free();
		}
		Bitmap = bmp;
		Width = ((Image)bmp).Width;
		Height = ((Image)bmp).Height;
		Bits = new int[Width * Height];
		BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		BitmapData val = bmp.LockBits(new Rectangle(0, 0, Width, Height), (ImageLockMode)1, (PixelFormat)2498570);
		IntPtr scan = val.Scan0;
		int num = Math.Abs(val.Stride) * Height;
		byte[] array = new byte[num];
		Marshal.Copy(scan, array, 0, num);
		for (int i = 0; i < Height; i++)
		{
			for (int j = 0; j < Width; j++)
			{
				int num2 = i * val.Stride + 4 * j;
				int num3 = (array[num2 + 3] << 24) | (array[num2 + 2] << 16) | (array[num2 + 1] << 8) | array[num2];
				Bits[i * Width + j] = num3;
			}
		}
		bmp.UnlockBits(val);
	}

	public Color GetPixel(int x, int y)
	{
		return Color.FromArgb(Bits[x + y * Width]);
	}

	public int GetPixelInt(int x, int y)
	{
		return Bits[x + y * Width];
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			Bitmap bitmap = Bitmap;
			if (bitmap != null)
			{
				((Image)bitmap).Dispose();
			}
		}
		if (BitsHandle.IsAllocated)
		{
			BitsHandle.Free();
		}
		disposed = true;
	}

	~DirectBitmap()
	{
		Dispose(disposing: false);
	}
}
