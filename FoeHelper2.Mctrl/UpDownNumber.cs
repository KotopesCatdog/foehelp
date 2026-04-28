using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace FoeHelper2.Mctrl;

public class UpDownNumber : UserControl, IComponentConnector
{
	private float minvalue;

	private float maxvalue = 100f;

	private float startvalue = 10f;

	private float increments = 1f;

	internal TextBox NUDTextBox;

	internal RepeatButton NUDButtonUP;

	internal RepeatButton NUDButtonDown;

	private bool _contentLoaded;

	public float Increments
	{
		get
		{
			return increments;
		}
		set
		{
			increments = value;
		}
	}

	public float Minvalue
	{
		get
		{
			return minvalue;
		}
		set
		{
			minvalue = value;
		}
	}

	public float Maxvalue
	{
		get
		{
			return maxvalue;
		}
		set
		{
			maxvalue = value;
		}
	}

	public float Startvalue
	{
		get
		{
			return startvalue;
		}
		set
		{
			startvalue = value;
			NUDTextBox.Text = Startvalue.ToString();
		}
	}

	public string CurrentValue
	{
		get
		{
			return NUDTextBox.Text;
		}
		set
		{
			NUDTextBox.Text = value;
		}
	}

	public UpDownNumber()
	{
		((FrameworkElement)this).DataContext = this;
		InitializeComponent();
		NUDTextBox.Text = Startvalue.ToString();
	}

	private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
	{
		float num = ((!(NUDTextBox.Text != "")) ? 0f : float.Parse(NUDTextBox.Text));
		if (num < maxvalue)
		{
			NUDTextBox.Text = Convert.ToString(num + increments);
		}
	}

	private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
	{
		float num = ((!(NUDTextBox.Text != "")) ? 0f : float.Parse(NUDTextBox.Text));
		if (num > Minvalue)
		{
			NUDTextBox.Text = Convert.ToString(num - increments);
		}
	}

	private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		if ((int)e.Key == 24)
		{
			((UIElement)NUDButtonUP).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[1] { true });
		}
		if ((int)e.Key == 26)
		{
			((UIElement)NUDButtonDown).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[1] { true });
		}
	}

	private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		if ((int)e.Key == 24)
		{
			typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonUP, new object[1] { false });
		}
		if ((int)e.Key == 26)
		{
			typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(NUDButtonDown, new object[1] { false });
		}
	}

	private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		float result = 0f;
		if (NUDTextBox.Text != "" && !float.TryParse(NUDTextBox.Text, out result))
		{
			NUDTextBox.Text = Startvalue.ToString();
		}
		if (result > Maxvalue)
		{
			NUDTextBox.Text = Maxvalue.ToString();
		}
		if (result < Minvalue)
		{
			NUDTextBox.Text = Minvalue.ToString();
		}
		NUDTextBox.SelectionStart = NUDTextBox.Text.Length;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri uri = new Uri("/FoeHelper2;component/updownnumber.xaml", UriKind.Relative);
			Application.LoadComponent((object)this, uri);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		switch (connectionId)
		{
		case 1:
			NUDTextBox = (TextBox)target;
			((UIElement)NUDTextBox).PreviewKeyDown += new KeyEventHandler(NUDTextBox_PreviewKeyDown);
			((UIElement)NUDTextBox).PreviewKeyUp += new KeyEventHandler(NUDTextBox_PreviewKeyUp);
			((TextBoxBase)NUDTextBox).TextChanged += new TextChangedEventHandler(NUDTextBox_TextChanged);
			break;
		case 2:
			NUDButtonUP = (RepeatButton)target;
			((ButtonBase)NUDButtonUP).Click += new RoutedEventHandler(NUDButtonUP_Click);
			break;
		case 3:
			NUDButtonDown = (RepeatButton)target;
			((ButtonBase)NUDButtonDown).Click += new RoutedEventHandler(NUDButtonDown_Click);
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
