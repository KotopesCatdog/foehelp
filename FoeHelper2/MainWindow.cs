using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using FoeHelper2.Mctrl;
using HDLibrary.Wpf.Input;

namespace FoeHelper2;

public class MainWindow : Window, INotifyPropertyChanged, IComponentConnector
{
	[Serializable]
	public class CustomHotKey : HotKey
	{
		private string name;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (value != name)
				{
					name = value;
					OnPropertyChanged(name);
				}
			}
		}

		public CustomHotKey(string name, Key key, ModifierKeys modifiers, bool enabled)
			: base(key, modifiers, enabled)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			Name = name;
		}

		protected override void OnHotKeyPress()
		{
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			if (Name == "Terminate")
			{
				Settings.Instance.SaveToXml();
				FoeTools.Quit();
			}
			else if (Name == "Break")
			{
				if (FoeTools.Signal == "break")
				{
					FoeTools.Signal = "";
					FoeTools.SignalText = "resume";
					FoeTools.ResumeTask();
				}
				else
				{
					FoeTools.Signal = "break";
					FoeTools.SignalText = "break pressed";
					FoeTools.BreakTask();
				}
			}
			else if (Name == "UnBreak")
			{
				FoeTools.Signal = "";
				FoeTools.SignalText = "resume";
				FoeTools.mainState = 0;
				FoeTools.outInfo("resumed");
			}
			else if (Name == "Debug")
			{
				_ = 1;
				FoeTools.RunDebug = true;
			}
			else
			{
				MessageBox.Show($"'{Name}' has been pressed ({this})");
			}
			base.OnHotKeyPress();
		}

		protected CustomHotKey(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Name = info.GetString("Name");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Name", Name);
		}
	}

	private Task workerTask;

	public CancellationToken ct;

	internal FoeTools foeTools;

	public Stopwatch stopwatch;

	private bool shopHidden;

	private bool helpHidden;

	private bool fightHidden;

	private bool expHidden;

	private bool customBattleOptionsHidden;

	private bool gbHidden;

	private bool isTaskRunning;

	private GB gb;

	private MiscWindow miscWindow;

	internal TextBlock mainStatusTextBlock;

	internal TextBlock hotkeyTextBlock;

	internal TabControl FoeClickerTabControl;

	internal StackPanel FightPanel;

	internal Button GoGuildBattleButton;

	internal UpDownNumber BattlesCount;

	internal UpDownNumber SlowDown;

	internal CheckBox FightUntilOCRcb;

	internal UpDownNumber FewBattlesRemain;

	internal CheckBox StopLosscb;

	internal TextBlock CustomBattleOptionsToggle;

	internal Grid CustomBattleOptionsStack;

	internal CheckBox AutoBattlescb;

	internal UpDownNumber MinUnits;

	internal UpDownNumber MinHealth;

	internal CheckBox DoubleCheckUnitsCheckbox;

	internal UpDownNumber QuantumBattlesCount;

	internal Button GoQuantumBattleInitButton;

	internal Button GoQuantumBattleButton;

	internal UpDownNumber QuantumMinUnits;

	internal UpDownNumber QuantumMinHealth;

	internal UpDownNumber HistAlliesBattlesCount;

	internal CheckBox HistAlliesAttackCb;

	internal CheckBox HistAlliesDefendCb;

	internal CheckBox HistAlliesFpCb;

	internal CheckBox HistAlliesMedalsCb;

	internal Button GoHistAlliesBattleInitButton;

	internal Button GoHistAlliesBattleButton;

	internal UpDownNumber HistAlliesMinUnits;

	internal UpDownNumber HistAlliesMinHealth;

	internal Grid ShopGrid;

	internal ComboBox shopModeComboBox;

	internal UpDownNumber DemandRatio;

	internal ComboBox shopComboBox;

	internal UpDownNumber ErasCycle;

	internal CheckBox s1;

	internal CheckBox s2;

	internal CheckBox s3;

	internal CheckBox s4;

	internal CheckBox s5;

	internal CheckBox d1;

	internal CheckBox d2;

	internal CheckBox d3;

	internal CheckBox d4;

	internal CheckBox d5;

	internal TextBox supplyVolumeTextBox;

	internal TextBox demandVolumeTextBox;

	internal UpDownNumber OffersCount;

	internal UpDownNumber OffersMultiplier;

	internal Button GoShopButton;

	internal StackPanel HelpPanel;

	internal CheckBox DoHelpCB;

	internal CheckBox DoPubCB;

	internal Button GoHelpAndPubButton;

	internal StackPanel ExpPanel;

	internal Button GoExpButton;

	internal StackPanel GBPanel;

	internal TextBox GBOutputTextBox;

	internal Button DebugButton;

	private bool _contentLoaded;

	public Bitmap TestImg { get; set; }

	public Settings Settings { get; }

	public event PropertyChangedEventHandler PropertyChanged;

	public void setCountInfo(int cnt, string info)
	{
		Settings.Instance.ShopCountInfo[cnt] = info;
		RaisePropertyChanged("ShopCountInfo");
	}

	public MainWindow()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Invalid comparison between Unknown and I4
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Invalid comparison between Unknown and I4
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Invalid comparison between Unknown and I4
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Invalid comparison between Unknown and I4
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Invalid comparison between Unknown and I4
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Expected O, but got Unknown
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Expected O, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Expected O, but got Unknown
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Expected O, but got Unknown
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Expected O, but got Unknown
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Expected O, but got Unknown
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Expected O, but got Unknown
		((Window)this).Closing += MainWindow_Closing;
		stopwatch = new Stopwatch();
		stopwatch.Start();
		FoeTools.SetDebug();
		Settings.Instance.ShopComboBoxList = new List<ShopItem>();
		for (int i = 1; i < FoeTools.MaxEras + 1; i++)
		{
			Settings.Instance.ShopComboBoxList.Add(new ShopItem("Era " + i, $"images\\b{i}.png"));
		}
		Settings.Instance.ShopCountInfo = new ObservableCollection<string>();
		for (int j = 0; j < 5; j++)
		{
			Settings.Instance.ShopCountInfo.Add("");
		}
		Settings.Instance.ShopCheckBox = new ObservableCollection<bool?>();
		for (int k = 0; k < 10; k++)
		{
			Settings.Instance.ShopCheckBox.Add(true);
		}
		Settings = Settings.Instance;
		Settings.LoadFromXml();
		InitializeComponent();

		// Hide all tabs except "Guild Battles" and "Historical Allies"
		foreach (var item in FoeClickerTabControl.Items)
		{
			if (item is TabItem tab)
			{
				string header = tab.Header?.ToString() ?? "";
				if (header != "Guild Battles" && header != "Historical Allies")
				{
					tab.Visibility = Visibility.Collapsed;
				}
			}
		}

		// Change default BattlesCount from 5 to 350
		BattlesCount.Startvalue = 350;

		// Change title text
		mainStatusTextBlock.Text = "СD-clicker";

		((UIElement)GoQuantumBattleInitButton).Visibility = (Visibility)2;
		((UIElement)GoHistAlliesBattleInitButton).Visibility = (Visibility)2;
		shopHidden = (int)((UIElement)ShopGrid).Visibility == 2;
		helpHidden = (int)((UIElement)HelpPanel).Visibility == 2;
		fightHidden = (int)((UIElement)FightPanel).Visibility == 2;
		expHidden = (int)((UIElement)ExpPanel).Visibility == 2;
		gbHidden = (int)((UIElement)GBPanel).Visibility == 2;
		customBattleOptionsHidden = true;
		if (customBattleOptionsHidden)
		{
			((UIElement)CustomBattleOptionsStack).Visibility = (Visibility)2;
			((UIElement)CustomBattleOptionsToggle).Visibility = (Visibility)2;
		}
		if (Settings.Instance.ReplaceUnitsR)
		{
			((UIElement)MinHealth).IsEnabled = false;
		}
		miscWindow = new MiscWindow();
		Settings.Instance.ToggleDebug();
		((Selector)shopComboBox).SelectionChanged += new SelectionChangedEventHandler(ShopComboBox_SelectionChanged);
		((Selector)shopModeComboBox).SelectionChanged += new SelectionChangedEventHandler(shopModeComboBox_SelectionChanged);
		Settings.Instance.SupplyVol = Settings.Instance.supplyText;
		((UIElement)supplyVolumeTextBox).GotFocus += new RoutedEventHandler(RemoveText);
		((UIElement)supplyVolumeTextBox).LostFocus += new RoutedEventHandler(AddText);
		Settings.Instance.DemandVol = Settings.Instance.demandText;
		((UIElement)demandVolumeTextBox).GotFocus += new RoutedEventHandler(RemoveText);
		((UIElement)demandVolumeTextBox).LostFocus += new RoutedEventHandler(AddText);
		((ToggleButton)FightUntilOCRcb).Checked += new RoutedEventHandler(FightUntilOCRcb_Checked);
		((ToggleButton)FightUntilOCRcb).Unchecked += new RoutedEventHandler(FightUntilOCRcb_Unchecked);
		foeTools = new FoeTools(this);
		((FrameworkElement)this).DataContext = Settings;
		FoeTools.deleteDebugFile();
	}

	public void BreakTask()
	{
		if (workerTask != null)
		{
			FoeTools.ts.Cancel();
		}
	}

	public void ResumeTask()
	{
	}

	public void FightUntilOCRcb_Checked(object sender, EventArgs e)
	{
		((UIElement)BattlesCount).IsEnabled = false;
		((UIElement)FewBattlesRemain).IsEnabled = true;
	}

	public void FightUntilOCRcb_Unchecked(object sender, EventArgs e)
	{
		((UIElement)BattlesCount).IsEnabled = true;
		((UIElement)FewBattlesRemain).IsEnabled = false;
	}

	public void RemoveText(object sender, EventArgs e)
	{
		object obj = ((sender is TextBox) ? sender : null);
		if (((FrameworkElement)obj).Name == "supplyVolumeTextBox" && Settings.Instance.SupplyVol == Settings.Instance.supplyText)
		{
			Settings.Instance.SupplyVol = "";
		}
		if (((FrameworkElement)obj).Name == "demandVolumeTextBox" && Settings.Instance.DemandVol == Settings.Instance.demandText)
		{
			Settings.Instance.DemandVol = "";
		}
	}

	public void AddText(object sender, EventArgs e)
	{
		object obj = ((sender is TextBox) ? sender : null);
		if (((FrameworkElement)obj).Name == "supplyVolumeTextBox" && string.IsNullOrWhiteSpace(Settings.Instance.SupplyVol))
		{
			Settings.Instance.SupplyVol = Settings.Instance.supplyText;
		}
		if (((FrameworkElement)obj).Name == "demandVolumeTextBox" && string.IsNullOrWhiteSpace(Settings.Instance.DemandVol))
		{
			Settings.Instance.DemandVol = Settings.Instance.demandText;
		}
	}

	private void ShopComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		FoeTools.SelectedEra = ((Selector)shopComboBox).SelectedIndex + 1;
	}

	private void shopModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		FoeTools.shoppingMode = ((Selector)shopModeComboBox).SelectedIndex;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		FoeTools.AddHotkeys();
		int selectedIndex = 0;
		_ = Settings.Instance.LastTab;
		_ = 0;
		((Selector)FoeClickerTabControl).SelectedIndex = selectedIndex;
		((Window)this).Topmost = true;
		((Window)this).Top = Settings.Instance.LastTop;
		((Window)this).Left = Settings.Instance.LastLeft;
		((Window)this).Title = "СD-clicker";
		gb = new GB(GBOutputTextBox, ((DispatcherObject)this).Dispatcher);
	}

	private void GoGBInit(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			gb.GoGbInit();
			FoeTools.mainState = 0;
		}, ct);
	}

	private void GoGB(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		workerTask = Task.Factory.StartNew(delegate
		{
			do
			{
				FoeTools.mainState = 1;
				try
				{
					gb.GoGBrun();
				}
				catch (Exception)
				{
				}
				FoeTools.mainState = 0;
				FoeTools.Wait(3000);
			}
			while (!ct.IsCancellationRequested && Settings.Instance.GBScanning);
		}, ct);
	}

	private void GoDebug(object sender, RoutedEventArgs e)
	{
		DebugWindow debugWindow = new DebugWindow();
		((Window)debugWindow).Show();
		((Window)debugWindow).Top = 0.0;
		((Window)debugWindow).Left = 0.0;
		((Window)debugWindow).Topmost = true;
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			bool flag = false;
			if (true)
			{
				while (true)
				{
					if (Settings.Instance.CoordDebug != "")
					{
						foeTools.CoordDebug();
					}
					if (Settings.Instance.ShopDebug)
					{
						if (!flag)
						{
							foeTools.goShopInit(debug: true);
						}
						flag = true;
						if (flag)
						{
							foeTools.GoShopDebug();
						}
					}
					if (Settings.Instance.FightDebug)
					{
						foeTools.GoFightDebug();
					}
					if (Settings.Instance.ColorDebug)
					{
						foeTools.ColorDebug();
					}
					if (Settings.Instance.QuantumDebug)
					{
						foeTools.ColorDebug();
					}
				}
			}
			FoeTools.mainState = 0;
		}, ct);
	}

	private void GoTakeScreenshot(object sender, RoutedEventArgs e)
	{
		((Image)PixelTools.CaptureScreen()).Save("screenshot " + FoeTools.getStopky(zeros: true) + ".png");
		FoeTools.outInfo("screenshot saved");
	}

	private bool TryStartTask(Button button)
	{
		if (isTaskRunning)
		{
			if (((ContentControl)button).Content != null && (((ContentControl)button).Content.ToString() == "ctrl+f3 to stop" || ((ContentControl)button).Content.ToString() == "stop task"))
			{
				FoeTools.BreakTask();
			}
			else
			{
				FoeTools.outInfo("Task is already running.");
			}
			return false;
		}
		isTaskRunning = true;
		UpdateButtonContent(button, "stop task");
		return true;
	}

	private void EndTask(Button button, string defaultText)
	{
		((DispatcherObject)this).Dispatcher.Invoke((Action)delegate
		{
			isTaskRunning = false;
			((UIElement)button).IsEnabled = true;
			UpdateButtonContent(button, defaultText);
		});
	}

	private void GoShopInit(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		int shopMode = ((Selector)shopModeComboBox).SelectedIndex;
		float demandRatio = float.Parse(DemandRatio.NUDTextBox.Text);
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		FoeTools.offersMultiplier = int.Parse(OffersMultiplier.NUDTextBox.Text);
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.shoppingMode = shopMode;
			FoeTools.demandRatio = demandRatio;
			if (Settings.Instance.EraSelected == 0 && FoeTools.shoppingMode == 2)
			{
				FoeTools.outInfo("Invalid selection. Please select at least era 2 or higher.");
			}
			else
			{
				FoeTools.mainState = 1;
				foeTools.goShopInit();
				FoeTools.mainState = 0;
			}
		}, ct);
	}

	private void GoShop(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoShopButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		string offersCount = OffersCount.NUDTextBox.Text;
		int erasCycle = int.Parse(ErasCycle.NUDTextBox.Text);
		FoeTools.outInfo("About to start shopping, eras to go: " + erasCycle);
		workerTask = Task.Factory.StartNew(delegate
		{
			try
			{
				if (Settings.Instance.EraSelected == 0 && FoeTools.shoppingMode == 2)
				{
					FoeTools.outInfo("Invalid selection. Please select at least era 2 or higher.");
				}
				else
				{
					for (int i = 0; i < erasCycle; i++)
					{
						FoeTools.mainState = 1;
						foeTools.goShop(int.Parse(offersCount));
						FoeTools.mainState = 0;
						if (erasCycle > 1)
						{
							FoeTools.outInfo("About to go shopping in the next era.");
							Settings.Instance.EraSelected++;
							FoeTools.SelectedEra = Settings.Instance.EraSelected + 1;
							if (foeTools.goShopInit() == -1)
							{
								break;
							}
						}
					}
				}
			}
			catch (OperationCanceledException)
			{
				FoeTools.outInfo("Task was canceled.");
			}
			catch (Exception ex2)
			{
				FoeTools.outInfo("Error: " + ex2.Message);
			}
		}, ct).ContinueWith(delegate
		{
			EndTask(GoShopButton, "Go shopping");
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoHelpInit(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			Settings.Instance.HelpEnabled = foeTools.GoHelpInit();
			FoeTools.mainState = 0;
		}, ct);
	}

	private void GoFightInit(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth = int.Parse(MinHealth.NUDTextBox.Text);
		int unitsCnt = 0;
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.FightType = "battles";
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			Settings.Instance.FightEnabled = foeTools.GoFightInit();
			FoeTools.mainState = 0;
			try
			{
				unitsCnt = foeTools.getLockedUnitsCount();
			}
			catch (Exception)
			{
				unitsCnt = 0;
			}
		}, ct).ContinueWith(delegate
		{
			if (unitsCnt > 0)
			{
				MinUnits.Maxvalue = unitsCnt;
				MinUnits.Startvalue = unitsCnt;
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoExpInit(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth = int.Parse(MinHealth.NUDTextBox.Text);
		int unitsCnt = 0;
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.FightType = "expeditions";
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			Settings.Instance.ExpeditionsEnabled = foeTools.GoFightInit();
			FoeTools.mainState = 0;
			unitsCnt = foeTools.getLockedUnitsCount();
		}, ct).ContinueWith(delegate
		{
			if (unitsCnt > 0)
			{
				MinUnits.Maxvalue = unitsCnt;
				MinUnits.Startvalue = unitsCnt;
			}
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoFightFill(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth = int.Parse(MinHealth.NUDTextBox.Text);
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			foeTools.GoFightFill();
			FoeTools.mainState = 0;
		}, ct);
	}

	private void GoFightFillQuantum(object sender, RoutedEventArgs e)
	{
		if (isTaskRunning)
		{
			FoeTools.outInfo("Task is already running.");
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth = int.Parse(MinHealth.NUDTextBox.Text);
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			foeTools.GoFightFill(quantum: true);
			FoeTools.mainState = 0;
		}, ct);
	}

	private void GoFightGuild(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoGuildBattleButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int battlesCount;
		int unitsMinHealth;
		int slowDown;
		int minUnits;
		int fewBattlesRemain;
		try
		{
			battlesCount = int.Parse(BattlesCount.NUDTextBox.Text);
			unitsMinHealth = int.Parse(MinHealth.NUDTextBox.Text);
			slowDown = int.Parse(SlowDown.NUDTextBox.Text);
			minUnits = int.Parse(MinUnits.NUDTextBox.Text);
			fewBattlesRemain = int.Parse(FewBattlesRemain.NUDTextBox.Text);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("Invalid input: " + ex.Message);
			EndTask(GoGuildBattleButton, "Fight guild battles");
			return;
		}
		workerTask = Task.Factory.StartNew(delegate
		{
			try
			{
				FoeTools.mainState = 1;
				foeTools.GoFightSetMinHealth(unitsMinHealth);
				FoeTools.fightUntilOCR = Settings.Instance.FightUntilOCR;
				FoeTools.stopLoss = Settings.Instance.StopLoss;
				FoeTools.ScaredFactor = slowDown;
				FoeTools.DoubleCheckUnits = Settings.Instance.DoubleCheckUnits;
				if (Settings.Instance.GBauto20)
				{
					if (foeTools.GoFightInit(silent: true))
					{
						foeTools.GoFightBattle(battlesCount, minUnits, fewBattlesRemain);
					}
					else
					{
						while (!ct.IsCancellationRequested)
						{
							if (FoeBattle.find20(foeTools))
							{
								FoeTools.outInfo("Found sector.");
								foeTools.GoFightBattle(battlesCount, minUnits, fewBattlesRemain);
								if (FoeBattle.tooManyLosses)
								{
									break;
								}
								FoeTools.outInfo("Sector finished.");
								FoeTools.Wait(1500);
								FoeSectors.SendEsc();
								FoeTools.Wait(1500);
								FoeSectors.SendEsc();
								FoeTools.Wait(1500);
							}
							else
							{
								FoeTools.Wait(5000);
							}
						}
					}
				}
				else
				{
					foeTools.GoFightBattle(battlesCount, minUnits, fewBattlesRemain);
				}
				FoeTools.mainState = 0;
			}
			catch (OperationCanceledException)
			{
				FoeTools.outInfo("Task was canceled.");
			}
			catch (Exception ex3)
			{
				FoeTools.outInfo("Error: " + ex3.Message);
			}
		}, ct).ContinueWith(delegate
		{
			EndTask(GoGuildBattleButton, "Fight guild battles");
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoQuantumBattleInit(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoQuantumBattleButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth;
		int slowDown;
		try
		{
			int.Parse(QuantumBattlesCount.NUDTextBox.Text);
			unitsMinHealth = int.Parse(QuantumMinHealth.NUDTextBox.Text);
			slowDown = int.Parse(SlowDown.NUDTextBox.Text);
			int.Parse(QuantumMinUnits.NUDTextBox.Text);
			int.Parse(FewBattlesRemain.NUDTextBox.Text);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("Invalid input: " + ex.Message);
			EndTask(GoQuantumBattleButton, "Fight quantum battles");
			return;
		}
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			Settings.Instance.FightEnabled = foeTools.GoFightInit(silent: false, quantumIncursions: true);
			FoeTools.mainState = 0;
			try
			{
				foeTools.getLockedUnitsCount();
			}
			catch (Exception)
			{
			}
			try
			{
				FoeTools.mainState = 1;
				foeTools.GoFightSetMinHealth(unitsMinHealth);
				FoeTools.fightUntilOCR = Settings.Instance.FightUntilOCR;
				FoeTools.stopLoss = Settings.Instance.StopLoss;
				FoeTools.ScaredFactor = slowDown;
				FoeTools.DoubleCheckUnits = Settings.Instance.DoubleCheckUnits;
				FoeTools.mainState = 0;
			}
			catch (OperationCanceledException)
			{
				FoeTools.outInfo("Task was canceled.");
			}
			catch (Exception ex4)
			{
				FoeTools.outInfo("Error: " + ex4.Message);
			}
		}, ct).ContinueWith(delegate
		{
			EndTask(GoQuantumBattleButton, "Fight quantum battles");
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoQuantumBattle(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoQuantumBattleButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int battlesCount;
		int unitsMinHealth;
		int slowDown;
		int minUnits;
		int fewBattlesRemain;
		try
		{
			battlesCount = int.Parse(QuantumBattlesCount.NUDTextBox.Text);
			unitsMinHealth = int.Parse(QuantumMinHealth.NUDTextBox.Text);
			slowDown = int.Parse(SlowDown.NUDTextBox.Text);
			minUnits = int.Parse(QuantumMinUnits.NUDTextBox.Text);
			fewBattlesRemain = int.Parse(FewBattlesRemain.NUDTextBox.Text);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("Invalid input: " + ex.Message);
			EndTask(GoQuantumBattleButton, "Fight quantum battles");
			return;
		}
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			Settings.Instance.FightEnabled = foeTools.GoFightInit(silent: false, quantumIncursions: true);
			FoeTools.mainState = 0;
			try
			{
				foeTools.getLockedUnitsCount();
			}
			catch (Exception)
			{
			}
			try
			{
				FoeTools.mainState = 1;
				foeTools.GoFightSetMinHealth(unitsMinHealth);
				FoeTools.fightUntilOCR = Settings.Instance.FightUntilOCR;
				FoeTools.stopLoss = Settings.Instance.StopLoss;
				FoeTools.ScaredFactor = slowDown;
				FoeTools.DoubleCheckUnits = Settings.Instance.DoubleCheckUnits;
				foeTools.GoQuantumBattle(battlesCount, minUnits, fewBattlesRemain);
				FoeTools.mainState = 0;
			}
			catch (OperationCanceledException)
			{
				FoeTools.outInfo("Task was canceled.");
			}
			catch (Exception ex4)
			{
				FoeTools.outInfo("Error: " + ex4.Message);
			}
		}, ct).ContinueWith(delegate
		{
			EndTask(GoQuantumBattleButton, "Fight quantum battles");
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoHistAlliesBattleInit(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoHistAlliesBattleButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth;
		int slowDown;
		try
		{
			int.Parse(HistAlliesBattlesCount.NUDTextBox.Text);
			unitsMinHealth = int.Parse(HistAlliesMinHealth.NUDTextBox.Text);
			slowDown = int.Parse(SlowDown.NUDTextBox.Text);
			int.Parse(HistAlliesMinUnits.NUDTextBox.Text);
			int.Parse(FewBattlesRemain.NUDTextBox.Text);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("Invalid input: " + ex.Message);
			EndTask(GoHistAlliesBattleButton, "Fight historical allies battles");
			return;
		}
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			Settings.Instance.FightEnabled = foeTools.GoFightInit(silent: false, quantumIncursions: true);
			FoeTools.mainState = 0;
			try
			{
				foeTools.getLockedUnitsCount();
			}
			catch (Exception)
			{
			}
			try
			{
				FoeTools.mainState = 1;
				foeTools.GoFightSetMinHealth(unitsMinHealth);
				FoeTools.fightUntilOCR = Settings.Instance.FightUntilOCR;
				FoeTools.stopLoss = Settings.Instance.StopLoss;
				FoeTools.ScaredFactor = slowDown;
				FoeTools.DoubleCheckUnits = Settings.Instance.DoubleCheckUnits;
				FoeTools.mainState = 0;
			}
			catch (OperationCanceledException)
			{
				FoeTools.outInfo("Task was canceled.");
			}
			catch (Exception ex4)
			{
				FoeTools.outInfo("Error: " + ex4.Message);
			}
		}, ct).ContinueWith(delegate
		{
			EndTask(GoHistAlliesBattleButton, "Fight historical allies battles");
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoHistAlliesBattle(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoHistAlliesBattleButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		bool doAttack = Settings.Instance.HistAlliesAttack;
		bool doDefend = Settings.Instance.HistAlliesDefend;
		bool doFp = Settings.Instance.HistAlliesFp;
		bool doMedals = Settings.Instance.HistAlliesMedals;
		int battlesCount;
		int unitsMinHealth;
		int slowDown;
		int minUnits;
		int fewBattlesRemain;
		try
		{
			battlesCount = int.Parse(HistAlliesBattlesCount.NUDTextBox.Text);
			unitsMinHealth = int.Parse(HistAlliesMinHealth.NUDTextBox.Text);
			slowDown = int.Parse(SlowDown.NUDTextBox.Text);
			minUnits = int.Parse(HistAlliesMinUnits.NUDTextBox.Text);
			fewBattlesRemain = int.Parse(FewBattlesRemain.NUDTextBox.Text);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("Invalid input: " + ex.Message);
			EndTask(GoHistAlliesBattleButton, "Fight historical allies battles");
			return;
		}
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			Settings.Instance.FightEnabled = foeTools.GoFightInit(silent: false, quantumIncursions: false, historicalAllies: true);
			FoeTools.mainState = 0;
			try
			{
				foeTools.getLockedUnitsCount();
			}
			catch (Exception)
			{
			}
			try
			{
				FoeTools.mainState = 1;
				foeTools.GoFightSetMinHealth(unitsMinHealth);
				FoeTools.fightUntilOCR = Settings.Instance.FightUntilOCR;
				FoeTools.stopLoss = Settings.Instance.StopLoss;
				FoeTools.ScaredFactor = slowDown;
				FoeTools.DoubleCheckUnits = Settings.Instance.DoubleCheckUnits;
				foeTools.GoHistoricalAlliesBattle(battlesCount, minUnits, fewBattlesRemain, doAttack, doDefend, doFp, doMedals);
				FoeTools.mainState = 0;
			}
			catch (OperationCanceledException)
			{
				FoeTools.outInfo("Task was canceled.");
			}
			catch (Exception ex4)
			{
				FoeTools.outInfo("Error: " + ex4.Message);
			}
		}, ct).ContinueWith(delegate
		{
			EndTask(GoHistAlliesBattleButton, "Fight historical allies battles");
		}, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void GoFightFillHistoricalAllies(object sender, RoutedEventArgs e)
	{
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int unitsMinHealth = int.Parse(HistAlliesMinHealth.NUDTextBox.Text);
		workerTask = Task.Factory.StartNew(delegate
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			foeTools.GoFightFill(quantum: true);
			FoeTools.mainState = 0;
		}, ct);
	}

	private async void GoFightExp(object sender, RoutedEventArgs e)
	{
		if (!TryStartTask(GoExpButton))
		{
			return;
		}
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		int battlesCount;
		int unitsMinHealth;
		int scaredFactor;
		int minUnits;
		try
		{
			battlesCount = int.Parse(BattlesCount.NUDTextBox.Text);
			unitsMinHealth = int.Parse(MinHealth.NUDTextBox.Text);
			scaredFactor = int.Parse(SlowDown.NUDTextBox.Text);
			minUnits = int.Parse(MinUnits.NUDTextBox.Text);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("Invalid input: " + ex.Message);
			EndTask(GoExpButton, "Fight expeditions");
			return;
		}
		try
		{
			FoeTools.mainState = 1;
			foeTools.GoFightSetMinHealth(unitsMinHealth);
			FoeTools.fightUntilOCR = Settings.Instance.FightUntilOCR;
			FoeTools.stopLoss = Settings.Instance.StopLoss;
			FoeTools.ScaredFactor = scaredFactor;
			FoeTools.DoubleCheckUnits = Settings.Instance.DoubleCheckUnits;
			await Task.Run(delegate
			{
				foeTools.GoFightExpeditions(battlesCount, minUnits);
			}, ct);
		}
		catch (OperationCanceledException)
		{
			FoeTools.outInfo("Task was canceled.");
		}
		catch (Exception ex3)
		{
			FoeTools.outInfo("Error: " + ex3.Message);
		}
		finally
		{
			FoeTools.mainState = 0;
			EndTask(GoExpButton, "Fight expeditions");
		}
	}

	private void UpdateButtonContent(Button button, string content)
	{
		try
		{
			((DispatcherObject)this).Dispatcher.Invoke((Action)delegate
			{
				((ContentControl)button).Content = content;
			});
		}
		catch (Exception)
		{
		}
	}

	private void GoHelpAndPub(object sender, RoutedEventArgs e)
	{
		FoeTools.ts = new CancellationTokenSource();
		ct = FoeTools.ts.Token;
		mainStatusTextBlock.Text = "Running";
		workerTask = Task.Factory.StartNew(delegate
		{
			try
			{
				FoeTools.DoPub = Settings.Instance.DoPub;
				FoeTools.DoHelp = Settings.Instance.DoHelp;
				FoeTools.mainState = 1;
				foeTools.goHelpAndPub();
				FoeTools.mainState = 0;
			}
			catch (OperationCanceledException)
			{
				((DispatcherObject)this).Dispatcher.Invoke((Action)delegate
				{
					((ContentControl)GoHelpAndPubButton).Content = "Start";
					FoeTools.outInfo("cancel");
				});
			}
			finally
			{
				((ContentControl)GoHelpAndPubButton).Content = "Start";
			}
		}, ct);
		((ContentControl)GoHelpAndPubButton).Content = "ctrl+f3 to stop";
	}

	private void SaveConfig(object sender, RoutedEventArgs e)
	{
		Settings.Instance.SaveToXml();
	}

	private void LoadConfig(object sender, RoutedEventArgs e)
	{
		Settings.Instance = Settings.LoadFromXml();
	}

	private void SaveCoords(object sender, RoutedEventArgs e)
	{
		if (FoeTools.coord != null)
		{
			FoeTools.coord.SaveToJsonFile("coords.json");
		}
	}

	private void LoadCoords(object sender, RoutedEventArgs e)
	{
		if (FoeTools.coord != null)
		{
			FoeTools.coord = new Coordinates();
			FoeTools.coord.LoadFromJsonFile("coords.json");
		}
	}

	private void HideWindow(object sender, RoutedEventArgs e)
	{
		((Window)this).Topmost = false;
		((Window)this).WindowState = (WindowState)1;
	}

	private void MiscWindowOpen(object sender, RoutedEventArgs e)
	{
		if (!((UIElement)miscWindow).IsVisible)
		{
			((Window)miscWindow).Show();
			((Window)miscWindow).Topmost = true;
		}
		else
		{
			((Window)miscWindow).Hide();
		}
	}

	private void RaisePropertyChanged(string propName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	}

	private void ToggleCustomOptions(object sender, RoutedEventArgs e)
	{
		customBattleOptionsHidden = !customBattleOptionsHidden;
		((UIElement)CustomBattleOptionsStack).Visibility = (Visibility)(customBattleOptionsHidden ? 2 : 0);
	}

	private void ComboBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		Settings.SaveToXml();
		FoeTools.Quit();
	}

	private void MainWindow_Closing(object sender, CancelEventArgs e)
	{
		Settings.SaveToXml();
		FoeTools.Quit();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri uri = new Uri("/FoeHelper2;component/mainwindow.xaml", UriKind.Relative);
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
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Expected O, but got Unknown
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Expected O, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Expected O, but got Unknown
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Expected O, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Expected O, but got Unknown
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Expected O, but got Unknown
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Expected O, but got Unknown
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Expected O, but got Unknown
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Expected O, but got Unknown
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Expected O, but got Unknown
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Expected O, but got Unknown
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Expected O, but got Unknown
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Expected O, but got Unknown
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Expected O, but got Unknown
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Expected O, but got Unknown
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Expected O, but got Unknown
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Expected O, but got Unknown
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Expected O, but got Unknown
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Expected O, but got Unknown
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Expected O, but got Unknown
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Expected O, but got Unknown
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Expected O, but got Unknown
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Expected O, but got Unknown
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Expected O, but got Unknown
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Expected O, but got Unknown
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Expected O, but got Unknown
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Expected O, but got Unknown
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Expected O, but got Unknown
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Expected O, but got Unknown
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Expected O, but got Unknown
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Expected O, but got Unknown
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Expected O, but got Unknown
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Expected O, but got Unknown
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Expected O, but got Unknown
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Expected O, but got Unknown
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0527: Expected O, but got Unknown
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Expected O, but got Unknown
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_054b: Expected O, but got Unknown
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Expected O, but got Unknown
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Expected O, but got Unknown
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Expected O, but got Unknown
		//IL_058a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Expected O, but got Unknown
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ac: Expected O, but got Unknown
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Expected O, but got Unknown
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Expected O, but got Unknown
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Expected O, but got Unknown
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0601: Expected O, but got Unknown
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Expected O, but got Unknown
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Expected O, but got Unknown
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_063e: Expected O, but got Unknown
		//IL_064b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Expected O, but got Unknown
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Expected O, but got Unknown
		//IL_066f: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Expected O, but got Unknown
		//IL_0687: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Expected O, but got Unknown
		switch (connectionId)
		{
		case 1:
			((FrameworkElement)(MainWindow)target).Loaded += new RoutedEventHandler(Window_Loaded);
			break;
		case 2:
			mainStatusTextBlock = (TextBlock)target;
			break;
		case 3:
			hotkeyTextBlock = (TextBlock)target;
			break;
		case 4:
			FoeClickerTabControl = (TabControl)target;
			break;
		case 5:
			FightPanel = (StackPanel)target;
			break;
		case 6:
			GoGuildBattleButton = (Button)target;
			((ButtonBase)GoGuildBattleButton).Click += new RoutedEventHandler(GoFightGuild);
			break;
		case 7:
			BattlesCount = (UpDownNumber)target;
			break;
		case 8:
			SlowDown = (UpDownNumber)target;
			break;
		case 9:
			FightUntilOCRcb = (CheckBox)target;
			break;
		case 10:
			FewBattlesRemain = (UpDownNumber)target;
			break;
		case 11:
			StopLosscb = (CheckBox)target;
			break;
		case 12:
			CustomBattleOptionsToggle = (TextBlock)target;
			((UIElement)CustomBattleOptionsToggle).MouseUp += new MouseButtonEventHandler(ToggleCustomOptions);
			break;
		case 13:
			CustomBattleOptionsStack = (Grid)target;
			break;
		case 14:
			AutoBattlescb = (CheckBox)target;
			break;
		case 15:
			MinUnits = (UpDownNumber)target;
			break;
		case 16:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoFightFill);
			break;
		case 17:
			MinHealth = (UpDownNumber)target;
			break;
		case 18:
			DoubleCheckUnitsCheckbox = (CheckBox)target;
			break;
		case 19:
			QuantumBattlesCount = (UpDownNumber)target;
			break;
		case 20:
			GoQuantumBattleInitButton = (Button)target;
			((ButtonBase)GoQuantumBattleInitButton).Click += new RoutedEventHandler(GoQuantumBattleInit);
			break;
		case 21:
			GoQuantumBattleButton = (Button)target;
			((ButtonBase)GoQuantumBattleButton).Click += new RoutedEventHandler(GoQuantumBattle);
			break;
		case 22:
			QuantumMinUnits = (UpDownNumber)target;
			break;
		case 23:
			QuantumMinHealth = (UpDownNumber)target;
			break;
		case 24:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoFightFillQuantum);
			break;
		case 25:
			HistAlliesBattlesCount = (UpDownNumber)target;
			break;
		case 26:
			HistAlliesAttackCb = (CheckBox)target;
			break;
		case 27:
			HistAlliesDefendCb = (CheckBox)target;
			break;
		case 28:
			HistAlliesFpCb = (CheckBox)target;
			break;
		case 29:
			HistAlliesMedalsCb = (CheckBox)target;
			break;
		case 30:
			GoHistAlliesBattleInitButton = (Button)target;
			((ButtonBase)GoHistAlliesBattleInitButton).Click += new RoutedEventHandler(GoHistAlliesBattleInit);
			break;
		case 31:
			GoHistAlliesBattleButton = (Button)target;
			((ButtonBase)GoHistAlliesBattleButton).Click += new RoutedEventHandler(GoHistAlliesBattle);
			break;
		case 32:
			HistAlliesMinUnits = (UpDownNumber)target;
			break;
		case 33:
			HistAlliesMinHealth = (UpDownNumber)target;
			break;
		case 34:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoFightFillHistoricalAllies);
			break;
		case 35:
			ShopGrid = (Grid)target;
			break;
		case 36:
			shopModeComboBox = (ComboBox)target;
			break;
		case 37:
			DemandRatio = (UpDownNumber)target;
			break;
		case 38:
			shopComboBox = (ComboBox)target;
			break;
		case 39:
			ErasCycle = (UpDownNumber)target;
			break;
		case 40:
			s1 = (CheckBox)target;
			break;
		case 41:
			s2 = (CheckBox)target;
			break;
		case 42:
			s3 = (CheckBox)target;
			break;
		case 43:
			s4 = (CheckBox)target;
			break;
		case 44:
			s5 = (CheckBox)target;
			break;
		case 45:
			d1 = (CheckBox)target;
			break;
		case 46:
			d2 = (CheckBox)target;
			break;
		case 47:
			d3 = (CheckBox)target;
			break;
		case 48:
			d4 = (CheckBox)target;
			break;
		case 49:
			d5 = (CheckBox)target;
			break;
		case 50:
			supplyVolumeTextBox = (TextBox)target;
			break;
		case 51:
			demandVolumeTextBox = (TextBox)target;
			break;
		case 52:
			OffersCount = (UpDownNumber)target;
			break;
		case 53:
			OffersMultiplier = (UpDownNumber)target;
			break;
		case 54:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoShopInit);
			break;
		case 55:
			GoShopButton = (Button)target;
			((ButtonBase)GoShopButton).Click += new RoutedEventHandler(GoShop);
			break;
		case 56:
			HelpPanel = (StackPanel)target;
			break;
		case 57:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoHelpInit);
			break;
		case 58:
			DoHelpCB = (CheckBox)target;
			break;
		case 59:
			DoPubCB = (CheckBox)target;
			break;
		case 60:
			GoHelpAndPubButton = (Button)target;
			((ButtonBase)GoHelpAndPubButton).Click += new RoutedEventHandler(GoHelpAndPub);
			break;
		case 61:
			ExpPanel = (StackPanel)target;
			break;
		case 62:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoExpInit);
			break;
		case 63:
			GoExpButton = (Button)target;
			((ButtonBase)GoExpButton).Click += new RoutedEventHandler(GoFightExp);
			break;
		case 64:
			GBPanel = (StackPanel)target;
			break;
		case 65:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoGBInit);
			break;
		case 66:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoGB);
			break;
		case 67:
			GBOutputTextBox = (TextBox)target;
			break;
		case 68:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(LoadConfig);
			break;
		case 69:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(SaveConfig);
			break;
		case 70:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(LoadCoords);
			break;
		case 71:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(SaveCoords);
			break;
		case 72:
			DebugButton = (Button)target;
			((ButtonBase)DebugButton).Click += new RoutedEventHandler(GoDebug);
			break;
		case 73:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(GoTakeScreenshot);
			break;
		case 74:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(HideWindow);
			break;
		case 75:
			((ButtonBase)(Button)target).Click += new RoutedEventHandler(MiscWindowOpen);
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
