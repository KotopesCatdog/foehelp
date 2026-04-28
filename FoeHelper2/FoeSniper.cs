using System;
using System.Collections.Generic;
using System.Linq;

namespace FoeHelper2;

public static class FoeSniper
{
	public static List<Contributor> Contributors = new List<Contributor>();

	public static double ArcMulti = 1.9;

	public static int[] BaseRewards = new int[5];

	public static int GbCost { get; set; }

	public static int TotalFpOnGb { get; set; }

	public static int getHighestFreeContributor(List<Contributor> contributors)
	{
		for (int i = 0; i < contributors.Count; i++)
		{
			if (!contributors[i].IsOwner && contributors[i].IsFree)
			{
				return i;
			}
		}
		return -1;
	}

	public static void resetFreeContributors(List<Contributor> contributors)
	{
		for (int i = 0; i < contributors.Count; i++)
		{
			contributors[i].IsFree = true;
		}
	}

	public static List<SlotInfo> CalculatePowerLockWithContributors(int gbCost, List<Contributor> contributors, double arcMultiplier, IReadOnlyList<int> baseRewards)
	{
		int num = contributors?.Sum((Contributor c) => c.FpInvested) ?? 0;
		int num2 = contributors.FirstOrDefault((Contributor c) => c.IsOwner)?.FpInvested ?? 0;
		List<SlotInfo> list = new List<SlotInfo>();
		for (int i = 0; i < 5; i++)
		{
			int arcContribution = (int)Math.Ceiling((double)baseRewards[i] * arcMultiplier);
			list.Add(new SlotInfo
			{
				SlotNumber = i + 1,
				BaseReward = baseRewards[i],
				ArcContribution = arcContribution,
				OwnerThreshold = -1,
				IsLocked = false
			});
		}
		int num3 = 0;
		int num4 = gbCost - num;
		int num5 = 0;
		int num6 = 0;
		for (int j = 0; j <= num4; j++)
		{
			int num7 = num2 + j;
			num5 = gbCost - (num + j + num6);
			_ = 17;
			if (num5 < 1)
			{
				break;
			}
			resetFreeContributors(contributors);
			for (int k = 0; k < 5; k++)
			{
				int highestFreeContributor = getHighestFreeContributor(contributors);
				int num8 = 0;
				if (highestFreeContributor != -1)
				{
					num8 = contributors[highestFreeContributor].FpInvested;
				}
				int num9 = list[k].ArcContribution;
				if (num8 > num9)
				{
					num9 = num8;
				}
				if (list[k].IsLocked || num5 > 2 * num9)
				{
					continue;
				}
				int num10 = num9 + 1;
				if (num10 > num5)
				{
					num10 = num5;
				}
				int num11 = 0;
				if (num8 >= num10)
				{
					contributors[highestFreeContributor].IsFree = false;
					continue;
				}
				num11 = num10 - num8;
				if (num5 - num9 - num11 < 0)
				{
					num6 += num9;
					num5 = gbCost - (num + j + num6);
					if (num5 < 0)
					{
						break;
					}
					list[k].IsLocked = true;
					list[k].OwnerThreshold = num7;
					if (num2 >= num7)
					{
						list[k].IsSecured = true;
					}
					else
					{
						list[k].IsSecured = false;
					}
					num3++;
				}
			}
			if (num3 == 5)
			{
				break;
			}
		}
		return list;
	}

	public static (int maxProfit, int bestSpent)? GetBestSnipingOpportunity(int gbCost, List<Contributor> contributors, double arcMultiplier, IReadOnlyList<int> baseRewards)
	{
		int num = contributors?.Sum((Contributor c) => c.FpInvested) ?? 0;
		int num2 = gbCost - num;
		int num3 = 0;
		int item = 0;
		new List<(int, int, int)>();
		List<Contributor> list = (from c in contributors
			where !c.IsOwner
			orderby c.FpInvested descending
			select c).ToList();
		List<SlotInfo> list2 = new List<SlotInfo>();
		for (int i = 0; i < 5; i++)
		{
			int arcContribution = (int)Math.Ceiling((double)baseRewards[i] * arcMultiplier);
			list2.Add(new SlotInfo
			{
				SlotNumber = i + 1,
				BaseReward = baseRewards[i],
				ArcContribution = arcContribution,
				IsLocked = false
			});
		}
		int num4 = num2;
		int num5 = 0;
		for (int j = 1; j < num4; j++)
		{
			num2 = num4 - j;
			int num6 = 0;
			foreach (Contributor item2 in list)
			{
				if (item2.FpInvested >= j)
				{
					num6++;
				}
			}
			num5 = ((num6 < list2.Count) ? list2[num6].ArcContribution : 0) - j;
			if (num5 < 1)
			{
				continue;
			}
			num2 = gbCost - (num + j);
			foreach (Contributor item3 in list)
			{
				if (item3.FpInvested < j)
				{
					int num7 = j - item3.FpInvested + 1;
					if (num2 - num7 < 0)
					{
						break;
					}
					num6++;
					num2 -= num7;
				}
			}
			if (num6 > 4)
			{
				continue;
			}
			num5 = ((num6 < list2.Count) ? list2[num6].ArcContribution : 0) - j;
			if (num5 < 1)
			{
				continue;
			}
			while (num6 < 5 && num2 >= 0)
			{
				int num8 = j + 1;
				if (num2 - num8 < 0)
				{
					break;
				}
				num6++;
				num2 -= num8;
			}
			if (num6 <= 4)
			{
				num5 = ((num6 < list2.Count) ? list2[num6].ArcContribution : 0) - j;
				if (num5 >= 1 && num5 > num3)
				{
					num3 = num5;
					item = j;
				}
			}
		}
		return (num3, item);
	}

	public static void ResetContributors()
	{
		Contributors.Clear();
	}

	public static void AddContributor(string name, bool isOwner, int fpInvested, bool isFree = true)
	{
		Contributors.Add(new Contributor
		{
			Name = name,
			IsOwner = isOwner,
			FpInvested = fpInvested,
			IsFree = isFree
		});
	}

	public static void SetBaseRewards(int[] baseRewards)
	{
		BaseRewards = baseRewards;
	}

	public static List<string> GetSecuredSpots(string gbName, int level, out string secureTextOut)
	{
		secureTextOut = "";
		List<string> list = new List<string>();
		List<SlotInfo> list2 = CalculatePowerLockWithContributors(GbCost, Contributors, ArcMulti, BaseRewards);
		int num = Contributors.FirstOrDefault((Contributor c) => c.IsOwner)?.FpInvested ?? 0;
		string text = "";
		list.Add($"GB Cost: {GbCost}");
		list.Add("   | Reward | Secure FP need | Secured");
		foreach (SlotInfo item in list2)
		{
			string text2 = "";
			text = $"{item.OwnerThreshold}";
			if (item.OwnerThreshold == -1)
			{
				text = "0";
				text2 = ((!item.IsSecured) ? "occupied" : "\ud83d\udd12secured");
			}
			else if (item.IsSecured)
			{
				text2 = "\ud83d\udd12secured";
			}
			else
			{
				text += $" ({item.OwnerThreshold - num})";
				text2 = "could be";
			}
			list.Add($"{item.SlotNumber,2} | {item.BaseReward,6} | {text,14} | {text2,8}");
		}
		string text3 = "";
		bool flag = false;
		foreach (SlotInfo item2 in list2)
		{
			if (item2.IsSecured)
			{
				flag = true;
				text3 += $"(P{item2.SlotNumber}) {item2.ArcContribution} ";
			}
		}
		if (flag)
		{
			text3 = text3 + " " + gbName;
			if (level > 0)
			{
				text3 += $" {level}->{level + 1}";
			}
			if (Settings.Instance.GBsecureText != "" && Settings.Instance.GBsecureText.Contains(Settings.Instance.GBpositionsPlaceholder))
			{
				text3 = Settings.Instance.GBsecureText.Replace(Settings.Instance.GBpositionsPlaceholder, text3);
			}
			secureTextOut = text3;
			list.Add("✅ " + text3);
		}
		return list;
	}

	public static List<string> GetBestSnipingOpportunity()
	{
		List<string> list = new List<string>();
		list.Add($"GB Cost: {GbCost}");
		list.Add($"Already on GB: {Contributors?.Sum((Contributor c) => c.FpInvested) ?? 0}");
		list.Add($"Owner already invested: {Contributors.FirstOrDefault((Contributor c) => c.IsOwner)?.FpInvested ?? 0}");
		list.Add("");
		(int, int)? bestSnipingOpportunity = GetBestSnipingOpportunity(GbCost, Contributors, ArcMulti, BaseRewards);
		if (bestSnipingOpportunity.HasValue)
		{
			if (bestSnipingOpportunity.Value.Item2 > 0)
			{
				list.Add($"✅Best Sniping Opportunity - FP Needed: {bestSnipingOpportunity.Value.Item2} | Profit: {bestSnipingOpportunity.Value.Item1}");
			}
			else
			{
				list.Add("No Sniping Opportunity");
			}
		}
		else
		{
			list.Add("No viable sniping opportunities available.");
		}
		return list;
	}
}
