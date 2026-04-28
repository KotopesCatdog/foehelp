using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace HDLibrary.Wpf.Input;

[Serializable]
public class HotKey : INotifyPropertyChanged, ISerializable, IEquatable<HotKey>
{
	private Key key;

	private ModifierKeys modifiers;

	private bool enabled;

	public Key Key
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return key;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (key != value)
			{
				key = value;
				OnPropertyChanged("Key");
			}
		}
	}

	public ModifierKeys Modifiers
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return modifiers;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (modifiers != value)
			{
				modifiers = value;
				OnPropertyChanged("Modifiers");
			}
		}
	}

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			if (value != enabled)
			{
				enabled = value;
				OnPropertyChanged("Enabled");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public event EventHandler<HotKeyEventArgs> HotKeyPressed;

	public HotKey()
	{
	}

	public HotKey(Key key, ModifierKeys modifiers)
		: this(key, modifiers, enabled: true)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	public HotKey(Key key, ModifierKeys modifiers, bool enabled)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Key = key;
		Modifiers = modifiers;
		Enabled = enabled;
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is HotKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(HotKey other)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (Key == other.Key)
		{
			return Modifiers == other.Modifiers;
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected I4, but got Unknown
		return Modifiers + 10 * Key;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return string.Format("{0} + {1} ({2}Enabled)", Key, Modifiers, Enabled ? "" : "Not ");
	}

	protected virtual void OnHotKeyPress()
	{
		if (this.HotKeyPressed != null)
		{
			this.HotKeyPressed(this, new HotKeyEventArgs(this));
		}
	}

	internal void RaiseOnHotKeyPressed()
	{
		OnHotKeyPress();
	}

	protected HotKey(SerializationInfo info, StreamingContext context)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Key = (Key)info.GetValue("Key", typeof(Key));
		Modifiers = (ModifierKeys)info.GetValue("Modifiers", typeof(ModifierKeys));
		Enabled = info.GetBoolean("Enabled");
	}

	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		info.AddValue("Key", Key, typeof(Key));
		info.AddValue("Modifiers", Modifiers, typeof(ModifierKeys));
		info.AddValue("Enabled", Enabled);
	}
}
