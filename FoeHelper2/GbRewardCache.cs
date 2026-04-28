using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FoeHelper2;

public static class GbRewardCache
{
	private static string cacheFile;

	private static Dictionary<int, int> cache;

	static GbRewardCache()
	{
		cacheFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gb_reward_cache.json");
		cache = new Dictionary<int, int>();
		Load();
	}

	public static string DebugCache()
	{
		return string.Join(", ", cache.Select((KeyValuePair<int, int> kv) => $"{kv.Key}={kv.Value}"));
	}

	public static void Load()
	{
		try
		{
			if (File.Exists(cacheFile))
			{
				cache = JsonSerializer.Deserialize<Dictionary<int, int>>(File.ReadAllText(cacheFile), (JsonSerializerOptions)null) ?? new Dictionary<int, int>();
			}
		}
		catch
		{
			cache = new Dictionary<int, int>();
		}
	}

	public static void Save()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		try
		{
			JsonSerializerOptions val = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			string contents = JsonSerializer.Serialize<Dictionary<int, int>>(cache, val);
			File.WriteAllText(cacheFile, contents);
		}
		catch (Exception ex)
		{
			FoeTools.outInfo("failed to save cache " + ex.ToString());
		}
	}

	public static bool TryGetReward(int gbRequired, out int reward)
	{
		return cache.TryGetValue(gbRequired, out reward);
	}

	public static void AddOrUpdate(int gbRequired, int reward)
	{
		cache[gbRequired] = reward;
		Save();
	}
}
