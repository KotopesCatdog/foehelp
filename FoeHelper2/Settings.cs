using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FoeHelper2;

public class Settings : INotifyPropertyChanged
{
	[XmlIgnore]
	private HashSet<string> presentXmlFields = new HashSet<string>();

	private bool autobattlesKey;

	private bool betterMoveMouseSafeSpot;

	private bool escBattlesKey;

	private bool pixelHunt;

	private bool replaceUnitsR;

	private bool doubleCheckUnits;

	private bool fightUntilOCR;

	private bool stopLoss;

	private bool doHelp;

	private bool doPub;

	private bool shopEnabled;

	private bool enableDebug;

	private int lastTab;

	private int lastTop;

	private int lastLeft;

	private bool colorDebug;

	private bool quantumDebug;

	private bool gbSecureMode;

	private bool gbSnipingMode;

	private bool fightDebug;

	private bool shopDebug;

	private bool helpEnabled;

	private bool fightEnabled;

	private string colorXdebug;

	private string hotkeyText;

	private string colorYdebug;

	private string coordDebug;

	private bool expeditionsEnabled;

	private bool gbEnabled;

	private bool gbScanning;

	private string gbSecureText;

	private string shopOffers;

	private int eraSelected;

	[XmlIgnore]
	private ObservableCollection<int> gbInvestor = new ObservableCollection<int> { 0, 0, 0, 0, 0 };

	[XmlIgnore]
	private ObservableCollection<bool> gbInvestorOwner = new ObservableCollection<bool> { false, false, false, false, false };

	private float[] zoomSettings;

	private bool cacheZoomSettings;

	private List<ShopItem> shopComboBoxList;

	private Visibility debugVisibility;

	private string mOut;

	private string debugInfo;

	[XmlIgnore]
	private bool overrideInvestors;

	public string supplyText = "amount..";

	public string GBpositionsPlaceholder = "{positions}";

	private string supplyVol;

	public string demandText = "amount..";

	private string demandVol;

	private string gbProgress;

	private string gbReward1;

	private bool gbRewardCache;

	private bool histAlliesAttack = true;

	private bool histAlliesDefend = true;

	private bool histAlliesFp = true;

	private bool histAlliesMedals = true;

	private bool gbAuto20;

	public bool gbManualEntered = true;

	private static Settings _instance;

	public bool AutoBattlesKey
	{
		get
		{
			return autobattlesKey;
		}
		set
		{
			autobattlesKey = value;
			RaisePropertyChanged("AutoBattlesKey");
		}
	}

	public bool BetterMoveMouseSafeSpot
	{
		get
		{
			return betterMoveMouseSafeSpot;
		}
		set
		{
			betterMoveMouseSafeSpot = value;
			RaisePropertyChanged("BetterMoveMouseSafeSpot");
		}
	}

	public bool EscBattlesKey
	{
		get
		{
			return escBattlesKey;
		}
		set
		{
			escBattlesKey = value;
			RaisePropertyChanged("EscBattlesKey");
		}
	}

	public bool PixelHunt
	{
		get
		{
			return pixelHunt;
		}
		set
		{
			pixelHunt = value;
			RaisePropertyChanged("PixelHunt");
		}
	}

	public bool ReplaceUnitsR
	{
		get
		{
			return replaceUnitsR;
		}
		set
		{
			replaceUnitsR = value;
			RaisePropertyChanged("ReplaceUnitsR");
		}
	}

	public bool DoubleCheckUnits
	{
		get
		{
			return doubleCheckUnits;
		}
		set
		{
			doubleCheckUnits = value;
			RaisePropertyChanged("DoubleCheckUnits");
		}
	}

	public bool FightUntilOCR
	{
		get
		{
			return fightUntilOCR;
		}
		set
		{
			fightUntilOCR = value;
			RaisePropertyChanged("FightUntilOCR");
		}
	}

	public bool StopLoss
	{
		get
		{
			return stopLoss;
		}
		set
		{
			stopLoss = value;
			RaisePropertyChanged("StopLoss");
		}
	}

	public bool DoHelp
	{
		get
		{
			return doHelp;
		}
		set
		{
			doHelp = value;
			RaisePropertyChanged("DoHelp");
		}
	}

	public bool DoPub
	{
		get
		{
			return doPub;
		}
		set
		{
			doPub = value;
			RaisePropertyChanged("DoPub");
		}
	}

	public bool ShopEnabled
	{
		get
		{
			return shopEnabled;
		}
		set
		{
			shopEnabled = value;
			RaisePropertyChanged("ShopEnabled");
		}
	}

	public bool EnableDebug
	{
		get
		{
			return enableDebug;
		}
		set
		{
			enableDebug = value;
			RaisePropertyChanged("EnableDebug");
		}
	}

	public int LastTab
	{
		get
		{
			return lastTab;
		}
		set
		{
			lastTab = value;
			RaisePropertyChanged("LastTab");
		}
	}

	public int LastTop
	{
		get
		{
			return lastTop;
		}
		set
		{
			lastTop = value;
			RaisePropertyChanged("LastTop");
		}
	}

	public int LastLeft
	{
		get
		{
			return lastLeft;
		}
		set
		{
			lastLeft = value;
			RaisePropertyChanged("LastLeft");
		}
	}

	public bool ColorDebug
	{
		get
		{
			return colorDebug;
		}
		set
		{
			colorDebug = value;
			RaisePropertyChanged("ColorDebug");
		}
	}

	public bool QuantumDebug
	{
		get
		{
			return quantumDebug;
		}
		set
		{
			quantumDebug = value;
			RaisePropertyChanged("QuantumDebug");
		}
	}

	public bool GBsecureMode
	{
		get
		{
			return gbSecureMode;
		}
		set
		{
			gbSecureMode = value;
			RaisePropertyChanged("GBsecureMode");
			if (gbSecureMode)
			{
				GBsnipingMode = false;
			}
		}
	}

	public bool GBsnipingMode
	{
		get
		{
			return gbSnipingMode;
		}
		set
		{
			gbSnipingMode = value;
			RaisePropertyChanged("GBsnipingMode");
			if (gbSnipingMode)
			{
				GBsecureMode = false;
			}
		}
	}

	public bool FightDebug
	{
		get
		{
			return fightDebug;
		}
		set
		{
			fightDebug = value;
			RaisePropertyChanged("FightDebug");
		}
	}

	public bool ShopDebug
	{
		get
		{
			return shopDebug;
		}
		set
		{
			shopDebug = value;
			RaisePropertyChanged("ShopDebug");
		}
	}

	public bool HelpEnabled
	{
		get
		{
			return helpEnabled;
		}
		set
		{
			helpEnabled = value;
			RaisePropertyChanged("HelpEnabled");
		}
	}

	[XmlIgnore]
	public bool FightEnabled
	{
		get
		{
			return fightEnabled;
		}
		set
		{
			fightEnabled = value;
			RaisePropertyChanged("FightEnabled");
		}
	}

	[XmlIgnore]
	public string ColorXDebug
	{
		get
		{
			return colorXdebug;
		}
		set
		{
			colorXdebug = value;
			RaisePropertyChanged("ColorXDebug");
		}
	}

	[XmlIgnore]
	public string HotkeyText
	{
		get
		{
			return hotkeyText;
		}
		set
		{
			hotkeyText = value;
			RaisePropertyChanged("HotkeyText");
		}
	}

	[XmlIgnore]
	public string ColorYDebug
	{
		get
		{
			return colorYdebug;
		}
		set
		{
			colorYdebug = value;
			RaisePropertyChanged("ColorYDebug");
		}
	}

	[XmlIgnore]
	public string CoordDebug
	{
		get
		{
			return coordDebug;
		}
		set
		{
			coordDebug = value;
			RaisePropertyChanged("CoordDebug");
		}
	}

	[XmlIgnore]
	public bool ExpeditionsEnabled
	{
		get
		{
			return expeditionsEnabled;
		}
		set
		{
			expeditionsEnabled = value;
			RaisePropertyChanged("ExpeditionsEnabled");
		}
	}

	[XmlIgnore]
	public bool GBEnabled
	{
		get
		{
			return gbEnabled;
		}
		set
		{
			gbEnabled = value;
			RaisePropertyChanged("GBEnabled");
		}
	}

	[XmlIgnore]
	public bool GBScanning
	{
		get
		{
			return gbScanning;
		}
		set
		{
			gbScanning = value;
			RaisePropertyChanged("GBScanning");
		}
	}

	public string GBsecureText
	{
		get
		{
			return gbSecureText;
		}
		set
		{
			gbSecureText = value;
			RaisePropertyChanged("GBsecureText");
		}
	}

	public string ShopOffers
	{
		get
		{
			return shopOffers;
		}
		set
		{
			shopOffers = value;
			RaisePropertyChanged("ShopOffers");
		}
	}

	public int EraSelected
	{
		get
		{
			return eraSelected;
		}
		set
		{
			eraSelected = value;
			RaisePropertyChanged("EraSelected");
		}
	}

	public ObservableCollection<int> GbInvestor
	{
		get
		{
			return gbInvestor;
		}
		set
		{
			if (gbInvestor != value)
			{
				gbInvestor = value;
				RaisePropertyChanged("GbInvestor");
			}
		}
	}

	public ObservableCollection<bool> GbInvestorOwner
	{
		get
		{
			return gbInvestorOwner;
		}
		set
		{
			if (gbInvestorOwner != value)
			{
				gbInvestorOwner = value;
				RaisePropertyChanged("GbInvestorOwner");
			}
		}
	}

	public float[] ZoomSettings
	{
		get
		{
			return zoomSettings;
		}
		set
		{
			zoomSettings = value;
			RaisePropertyChanged("ZoomSettings");
		}
	}

	public bool CacheZoomSettings
	{
		get
		{
			return cacheZoomSettings;
		}
		set
		{
			cacheZoomSettings = value;
			RaisePropertyChanged("CacheZoomSettings");
		}
	}

	[XmlIgnore]
	public ObservableCollection<bool?> ShopCheckBox { get; set; }

	[XmlIgnore]
	public ObservableCollection<string> ShopCountInfo { get; set; }

	[XmlIgnore]
	public List<ShopItem> ShopComboBoxList
	{
		get
		{
			return shopComboBoxList;
		}
		set
		{
			shopComboBoxList = value;
			RaisePropertyChanged("ShopComboBoxList");
		}
	}

	[XmlIgnore]
	public Visibility DebugVisibility
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return debugVisibility;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			debugVisibility = value;
			RaisePropertyChanged("DebugVisibility");
		}
	}

	[XmlIgnore]
	public string Mout
	{
		get
		{
			return mOut;
		}
		set
		{
			mOut = value;
			RaisePropertyChanged("Mout");
		}
	}

	[XmlIgnore]
	public string DebugInfo
	{
		get
		{
			return debugInfo;
		}
		set
		{
			debugInfo = value;
			RaisePropertyChanged("DebugInfo");
		}
	}

	public bool OverrideInvestors
	{
		get
		{
			return overrideInvestors;
		}
		set
		{
			if (overrideInvestors != value)
			{
				overrideInvestors = value;
				RaisePropertyChanged("OverrideInvestors");
			}
		}
	}

	public string SupplyVol
	{
		get
		{
			return supplyVol;
		}
		set
		{
			try
			{
				if (FoeTools.demandRatio > 1f && int.Parse(FoeTools.getShoppingCount(FoeTools.ShoppingProp.Demand, value)) > 1000)
				{
					value = ((int)(1000f / FoeTools.demandRatio)).ToString();
				}
			}
			catch
			{
			}
			supplyVol = value;
			demandVol = FoeTools.getShoppingCount(FoeTools.ShoppingProp.Demand, value);
			RaisePropertyChanged("SupplyVol");
			RaisePropertyChanged("DemandVol");
		}
	}

	public string DemandVol
	{
		get
		{
			return demandVol;
		}
		set
		{
			demandVol = value;
			supplyVol = FoeTools.getShoppingCount(FoeTools.ShoppingProp.Offer, value);
			RaisePropertyChanged("DemandVol");
			RaisePropertyChanged("SupplyVol");
		}
	}

	public string GBProgress
	{
		get
		{
			return gbProgress;
		}
		set
		{
			gbProgress = value;
			RaisePropertyChanged("GBProgress");
		}
	}

	public string GBReward1
	{
		get
		{
			return gbReward1;
		}
		set
		{
			gbReward1 = value;
			RaisePropertyChanged("GBReward1");
			if (gbManualEntered)
			{
				string[] array = GBProgress.Split(new char[1] { '/' });
				_ = new int[2];
				try
				{
					int gbRequired = int.Parse(array[1]);
					int reward = int.Parse(GBReward1);
					GbRewardCache.AddOrUpdate(gbRequired, reward);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString() ?? "");
				}
			}
		}
	}

	public bool GBRewardCache
	{
		get
		{
			return gbRewardCache;
		}
		set
		{
			gbRewardCache = value;
			RaisePropertyChanged("GBRewardCache");
		}
	}

	public bool HistAlliesAttack
	{
		get
		{
			return histAlliesAttack;
		}
		set
		{
			histAlliesAttack = value;
			RaisePropertyChanged("HistAlliesAttack");
		}
	}

	public bool HistAlliesDefend
	{
		get
		{
			return histAlliesDefend;
		}
		set
		{
			histAlliesDefend = value;
			RaisePropertyChanged("HistAlliesDefend");
		}
	}

	public bool HistAlliesFp
	{
		get
		{
			return histAlliesFp;
		}
		set
		{
			histAlliesFp = value;
			RaisePropertyChanged("HistAlliesFp");
		}
	}

	public bool HistAlliesMedals
	{
		get
		{
			return histAlliesMedals;
		}
		set
		{
			histAlliesMedals = value;
			RaisePropertyChanged("HistAlliesMedals");
		}
	}

	public bool GBauto20
	{
		get
		{
			return gbAuto20;
		}
		set
		{
			gbAuto20 = value;
			RaisePropertyChanged("GBauto20");
		}
	}

	public static Settings Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = LoadFromXml();
			}
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public bool IsInXml(string propName)
	{
		return presentXmlFields.Contains(propName);
	}

	private Settings()
	{
		DoubleCheckUnits = true;
		AutoBattlesKey = true;
		EscBattlesKey = true;
		DoHelp = true;
		DoPub = true;
		StopLoss = true;
		ReplaceUnitsR = true;
		FightEnabled = true;
		ExpeditionsEnabled = false;
		EnableDebug = false;
		LastTab = 4;
		LastLeft = 100;
		LastTop = 200;
		GBRewardCache = true;
		zoomSettings = new float[2];
		CacheZoomSettings = false;
		PixelHunt = false;
		GBsecureMode = true;
		GBsnipingMode = false;
		GBauto20 = false;
		GBsecureText = GBpositionsPlaceholder + " thanks!";
		OverrideInvestors = false;
		HistAlliesAttack = true;
		HistAlliesDefend = true;
		HistAlliesFp = true;
		HistAlliesMedals = true;
	}

	public void ToggleDebug()
	{
		if (EnableDebug)
		{
			DebugVisibility = (Visibility)0;
		}
		else
		{
			DebugVisibility = (Visibility)2;
		}
	}

	public void SaveToXml()
	{
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
		using StreamWriter textWriter = new StreamWriter(path);
		xmlSerializer.Serialize(textWriter, this);
	}

	public void FixMissingDefaults()
	{
		if (!IsInXml("GBauto20"))
		{
			GBauto20 = false;
		}
		if (!IsInXml("AutoBattlesKey"))
		{
			AutoBattlesKey = true;
		}
		if (!IsInXml("EscBattlesKey"))
		{
			EscBattlesKey = true;
		}
		if (!IsInXml("DoubleCheckUnits"))
		{
			DoubleCheckUnits = true;
		}
		if (!IsInXml("StopLoss"))
		{
			StopLoss = true;
		}
		if (!IsInXml("ReplaceUnitsR"))
		{
			ReplaceUnitsR = true;
		}
		if (!IsInXml("DoHelp"))
		{
			DoHelp = true;
		}
		if (!IsInXml("DoPub"))
		{
			DoPub = true;
		}
		if (!IsInXml("GBRewardCache"))
		{
			GBRewardCache = true;
		}
		if (!IsInXml("CacheZoomSettings"))
		{
			CacheZoomSettings = false;
		}
		if (!IsInXml("LastTab"))
		{
			LastTab = 4;
		}
		if (!IsInXml("LastLeft"))
		{
			LastLeft = 100;
		}
		if (!IsInXml("LastTop"))
		{
			LastTop = 200;
		}
		if (!IsInXml("GBsecureMode"))
		{
			GBsecureMode = true;
		}
		if (!IsInXml("GBsnipingMode"))
		{
			GBsnipingMode = false;
		}
		if (!IsInXml("HistAlliesAttack"))
		{
			HistAlliesAttack = true;
		}
		if (!IsInXml("HistAlliesDefend"))
		{
			HistAlliesDefend = true;
		}
		if (!IsInXml("HistAlliesFp"))
		{
			HistAlliesFp = true;
		}
		if (!IsInXml("HistAlliesMedals"))
		{
			HistAlliesMedals = true;
		}
		if (!IsInXml("GBsecureText"))
		{
			GBsecureText = GBpositionsPlaceholder + " thanks!";
		}
		FightEnabled = true;
		ExpeditionsEnabled = false;
	}

	public static Settings LoadFromXml()
	{
		string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
		if (!File.Exists(text))
		{
			return new Settings();
		}
		HashSet<string> hashSet = new HashSet<string>(from e in ((XContainer)XDocument.Load(text).Root).Elements()
			select e.Name.LocalName);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Settings));
		Settings settings;
		using (StreamReader textReader = new StreamReader(text))
		{
			settings = (Settings)xmlSerializer.Deserialize(textReader);
		}
		settings.presentXmlFields = hashSet;
		if (settings.EnableDebug && !Directory.Exists("c:\\mik"))
		{
			settings.EnableDebug = false;
			settings.ColorDebug = false;
			settings.QuantumDebug = false;
			settings.FightDebug = false;
		}
		settings.FightEnabled = true;
		settings.ExpeditionsEnabled = false;
		settings.FixMissingDefaults();
		settings.ToggleDebug();
		settings.PixelHunt = false;
		return settings;
	}

	private void RaisePropertyChanged(string propName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		if (propName == "ReplaceUnitsR")
		{
			_ = ReplaceUnitsR;
		}
	}
}
