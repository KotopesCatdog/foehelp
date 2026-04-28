using System;

namespace HDLibrary.Wpf.Input;

public class HotKeyEventArgs : EventArgs
{
	public HotKey HotKey { get; private set; }

	public HotKeyEventArgs(HotKey hotKey)
	{
		HotKey = hotKey;
	}
}
