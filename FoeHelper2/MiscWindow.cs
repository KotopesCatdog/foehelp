using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace FoeHelper2;

public class MiscWindow : Window, IComponentConnector
{
	internal TextBlock enableDebugText;

	internal CheckBox enableDebugCb;

	private bool _contentLoaded;

	public Settings Settings { get; }

	public MiscWindow()
	{
		InitializeComponent();
		Settings = Settings.Instance;
		((FrameworkElement)this).DataContext = Settings;
		if (!Directory.Exists("c:\\mik\\"))
		{
			((UIElement)enableDebugText).IsEnabled = false;
			((UIElement)enableDebugCb).IsEnabled = false;
		}
	}

	private void CheckBox_Checked(object sender, RoutedEventArgs e)
	{
		if (!Directory.Exists("c:\\mik\\"))
		{
			enableDebugText.Text = "Enable debug - not allowed";
			Settings.Instance.EnableDebug = false;
		}
		Settings.Instance.ToggleDebug();
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		FoeTools.Quit();
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
		((Window)this).Hide();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri uri = new Uri("/FoeHelper2;component/miscwindow.xaml", UriKind.Relative);
			Application.LoadComponent((object)this, uri);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		switch (connectionId)
		{
		case 1:
			enableDebugText = (TextBlock)target;
			break;
		case 2:
			enableDebugCb = (CheckBox)target;
			((ToggleButton)enableDebugCb).Checked += new RoutedEventHandler(CheckBox_Checked);
			break;
		case 3:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(Button_Click_1);
			break;
		case 4:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(Button_Click);
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
