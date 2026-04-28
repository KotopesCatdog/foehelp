using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace HDLibrary.Wpf.Input;

public sealed class HotKeyHost : IDisposable
{
	public class SerialCounter
	{
		public int Current { get; private set; }

		public SerialCounter(int start)
		{
			Current = start;
		}

		public int Next()
		{
			return ++Current;
		}
	}

	private const int WM_HotKey = 786;

	private HwndSourceHook hook;

	private HwndSource hwndSource;

	private Dictionary<int, HotKey> hotKeys = new Dictionary<int, HotKey>();

	private static readonly SerialCounter idGen = new SerialCounter(1);

	private bool disposed;

	public IEnumerable<HotKey> HotKeys => hotKeys.Values;

	public event EventHandler<HotKeyEventArgs> HotKeyPressed;

	public HotKeyHost(HwndSource hwndSource)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		if (hwndSource == null)
		{
			throw new ArgumentNullException("hwndSource");
		}
		hook = new HwndSourceHook(WndProc);
		this.hwndSource = hwndSource;
		hwndSource.AddHook(hook);
	}

	[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	private static extern int RegisterHotKey(IntPtr hwnd, int id, int modifiers, int key);

	[DllImport("user32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	private static extern int UnregisterHotKey(IntPtr hwnd, int id);

	private void RegisterHotKey(int id, HotKey hotKey)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected I4, but got Unknown
		if ((int)hwndSource.Handle != 0)
		{
			RegisterHotKey(hwndSource.Handle, id, (int)hotKey.Modifiers, KeyInterop.VirtualKeyFromKey(hotKey.Key));
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				Exception ex = new Win32Exception(lastWin32Error);
				if (lastWin32Error == 1409)
				{
					throw new HotKeyAlreadyRegisteredException(ex.Message, hotKey, ex);
				}
				throw ex;
			}
			return;
		}
		throw new InvalidOperationException("Handle is invalid");
	}

	private void UnregisterHotKey(int id)
	{
		if ((int)hwndSource.Handle != 0)
		{
			UnregisterHotKey(hwndSource.Handle, id);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
	}

	private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == 786 && hotKeys.ContainsKey((int)wParam))
		{
			HotKey hotKey = hotKeys[(int)wParam];
			hotKey.RaiseOnHotKeyPressed();
			if (this.HotKeyPressed != null)
			{
				this.HotKeyPressed(this, new HotKeyEventArgs(hotKey));
			}
		}
		return new IntPtr(0);
	}

	private void hotKey_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		KeyValuePair<int, HotKey> keyValuePair = hotKeys.FirstOrDefault((KeyValuePair<int, HotKey> h) => h.Value == sender);
		if (keyValuePair.Value == null)
		{
			return;
		}
		if (e.PropertyName == "Enabled")
		{
			if (keyValuePair.Value.Enabled)
			{
				RegisterHotKey(keyValuePair.Key, keyValuePair.Value);
			}
			else
			{
				UnregisterHotKey(keyValuePair.Key);
			}
		}
		else if ((e.PropertyName == "Key" || e.PropertyName == "Modifiers") && keyValuePair.Value.Enabled)
		{
			UnregisterHotKey(keyValuePair.Key);
			RegisterHotKey(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public void AddHotKey(HotKey hotKey)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (hotKey == null)
		{
			throw new ArgumentNullException("value");
		}
		if ((int)hotKey.Key == 0)
		{
			throw new ArgumentNullException("value.Key");
		}
		if (hotKeys.ContainsValue(hotKey))
		{
			throw new HotKeyAlreadyRegisteredException("HotKey already registered!", hotKey);
		}
		int num = idGen.Next();
		if (hotKey.Enabled)
		{
			RegisterHotKey(num, hotKey);
		}
		hotKey.PropertyChanged += hotKey_PropertyChanged;
		hotKeys[num] = hotKey;
	}

	public bool RemoveHotKey(HotKey hotKey)
	{
		KeyValuePair<int, HotKey> keyValuePair = hotKeys.FirstOrDefault((KeyValuePair<int, HotKey> h) => h.Value == hotKey);
		if (keyValuePair.Value != null)
		{
			keyValuePair.Value.PropertyChanged -= hotKey_PropertyChanged;
			if (keyValuePair.Value.Enabled)
			{
				UnregisterHotKey(keyValuePair.Key);
			}
			return hotKeys.Remove(keyValuePair.Key);
		}
		return false;
	}

	private void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				hwndSource.RemoveHook(hook);
			}
			for (int num = hotKeys.Count - 1; num >= 0; num--)
			{
				RemoveHotKey(hotKeys.Values.ElementAt(num));
			}
			disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~HotKeyHost()
	{
		Dispose(disposing: false);
	}
}
