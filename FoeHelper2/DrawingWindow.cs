using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FoeHelper2;

public class DrawingWindow : Window, IComponentConnector
{
	[Flags]
	public enum ExtendedWindowStyles
	{
		WS_EX_LAYERED = 0x80000,
		WS_EX_TRANSPARENT = 0x20
	}

	public enum GetWindowLongFields
	{
		GWL_EXSTYLE = -20
	}

	internal Canvas drawingCanvas;

	private bool _contentLoaded;

	public DrawingWindow()
	{
		InitializeComponent();
		InitializeTransparentWindow();
	}

	private void InitializeTransparentWindow()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WindowInteropHelper val = new WindowInteropHelper((Window)(object)this);
		int num = (int)GetWindowLong(val.Handle, -20);
		num |= 0x80020;
		SetWindowLong(val.Handle, -20, (IntPtr)num);
	}

	public void DrawOnCanvas()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		Rectangle val = new Rectangle
		{
			Width = 200.0,
			Height = 200.0,
			Stroke = (Brush)(object)Brushes.Red,
			StrokeThickness = 5.0
		};
		Canvas.SetLeft((UIElement)(object)val, 100.0);
		Canvas.SetTop((UIElement)(object)val, 100.0);
		((Panel)drawingCanvas).Children.Add((UIElement)(object)val);
	}

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri uri = new Uri("/FoeHelper2;component/drawingwindow.xaml", UriKind.Relative);
			Application.LoadComponent((object)this, uri);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		if (connectionId == 1)
		{
			drawingCanvas = (Canvas)target;
		}
		else
		{
			_contentLoaded = true;
		}
	}
}
