using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using FoeHelper2;

public static class ClipboardHelper
{
	public static void CopyToClipboard(string text, TextBox fallbackTextBox)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		((DispatcherObject)Application.Current).Dispatcher.BeginInvoke((DispatcherPriority)4, (Delegate)(Action)delegate
		{
			try
			{
				Clipboard.SetText(text);
				FoeTools.outInfo("\ud83d\udccb Copied to clipboard");
			}
			catch (COMException)
			{
				FoeTools.outInfo("\ud83d\udccb Clipboard busy — text selected, press Ctrl+C");
				if (fallbackTextBox != null)
				{
					((UIElement)fallbackTextBox).Focus();
					((TextBoxBase)fallbackTextBox).SelectAll();
				}
			}
			catch (Exception ex2)
			{
				FoeTools.outInfo("❌ Clipboard error: " + ex2.Message);
			}
		});
	}
}
