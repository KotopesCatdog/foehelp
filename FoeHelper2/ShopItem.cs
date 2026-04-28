using System;
using System.Windows.Media.Imaging;

namespace FoeHelper2;

public class ShopItem
{
	public string Name { get; set; }

	public string IconPath { get; set; }

	public BitmapImage Bi { get; set; }

	public ShopItem(string n, string path)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		Name = n;
		IconPath = path;
		Bi = new BitmapImage();
		Bi.BeginInit();
		Bi.UriSource = new Uri(path, UriKind.Relative);
		Bi.EndInit();
	}
}
