using System.Windows;
using System.Windows.Controls;

namespace FoeHelper2;

public static class AutoScrollBehavior
{
	public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata((object)false, new PropertyChangedCallback(AutoScrollPropertyChanged)));

	public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		ScrollViewer val = (ScrollViewer)(object)((obj is ScrollViewer) ? obj : null);
		if (val != null && (bool)((DependencyPropertyChangedEventArgs)(ref args)).NewValue)
		{
			val.ScrollChanged += new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
			val.ScrollToEnd();
		}
		else
		{
			val.ScrollChanged -= new ScrollChangedEventHandler(ScrollViewer_ScrollChanged);
		}
	}

	private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (e.ExtentHeightChange != 0.0)
		{
			object obj = ((sender is ScrollViewer) ? sender : null);
			if (obj != null)
			{
				((ScrollViewer)obj).ScrollToBottom();
			}
		}
	}

	public static bool GetAutoScroll(DependencyObject obj)
	{
		return (bool)obj.GetValue(AutoScrollProperty);
	}

	public static void SetAutoScroll(DependencyObject obj, bool value)
	{
		obj.SetValue(AutoScrollProperty, (object)value);
	}
}
