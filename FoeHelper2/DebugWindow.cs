using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace FoeHelper2;

public class DebugWindow : Window, IComponentConnector
{
	internal Button CoordDebugButton;

	internal ScrollViewer logScrollViewer;

	private bool _contentLoaded;

	public Settings Settings { get; }

	public DebugWindow()
	{
		InitializeComponent();
		Settings = Settings.Instance;
		((FrameworkElement)this).DataContext = Settings;
	}

	private void CoordDebugButton_Click(object sender, RoutedEventArgs e)
	{
		Settings.Instance.DebugInfo += "coord debug";
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri uri = new Uri("/FoeHelper2;component/debugwindow.xaml", UriKind.Relative);
			Application.LoadComponent((object)this, uri);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		switch (connectionId)
		{
		case 1:
			CoordDebugButton = (Button)target;
			((ButtonBase)CoordDebugButton).Click += new RoutedEventHandler(CoordDebugButton_Click);
			break;
		case 2:
			logScrollViewer = (ScrollViewer)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
