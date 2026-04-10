using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.ObjectSystem;

namespace NavalDLC;

public static class NavalDLCCheats
{
	public const string DLCNotActive = "DLC is not active.";

	public static bool CheckCheatUsage(ref string message)
	{
		if (!CampaignCheats.CheckCheatUsage(ref message))
		{
			return false;
		}
		ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo("NavalDLC");
		if (moduleInfo == null || !moduleInfo.IsActive)
		{
			message = "DLC is not active.";
			return false;
		}
		return true;
	}

	[CommandLineArgumentFunction("damage_player_ships", "naval")]
	public static string DamagePlayerShips(List<string> strings)
	{
		string message = string.Empty;
		if (!CheckCheatUsage(ref message))
		{
			return message;
		}
		MobileParty mainParty = MobileParty.MainParty;
		MBReadOnlyList<Ship> val = ((mainParty != null) ? mainParty.Ships : null);
		if (val == null || ((List<Ship>)(object)val).Count == 0)
		{
			return "Player does not have any ships";
		}
		float result = 0.5f;
		if (strings.Count == 1 && float.TryParse(strings[0], out result) && (MBMath.ApproximatelyEqualsTo(result, 0f, 1E-05f) || result < 0f))
		{
			result = 0.5f;
		}
		float num2 = default(float);
		for (int num = ((List<Ship>)(object)val).Count - 1; num >= 0; num--)
		{
			Ship obj = ((List<Ship>)(object)val)[num];
			obj.OnShipDamaged(obj.MaxHitPoints * result, (IShipOrigin)null, ref num2);
		}
		return "All ship hit points are reduced";
	}

	[CommandLineArgumentFunction("add_ship_to_player", "naval")]
	public static string AddShipToPlayer(List<string> strings)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		string message = string.Empty;
		if (!CheckCheatUsage(ref message))
		{
			return message;
		}
		string text = "Format is \"naval.add_ship_to_player [ShipName] | [Count] - (Empty = Random 1 Ship)\".";
		if (CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		if (!CampaignCheats.IsPartySuitableToUseCheat(PartyBase.MainParty))
		{
			return "Main party not suitable to take ship right now";
		}
		List<string> separatedNames = CampaignCheats.GetSeparatedNames(strings, true);
		MBList<ShipHull> shipHulls = Extensions.ToMBList<ShipHull>(((IEnumerable<Kingdom>)Kingdom.All).SelectMany((Kingdom x) => (IEnumerable<ShipHull>)x.Culture.AvailableShipHulls));
		string text2 = string.Empty;
		int result = 1;
		ShipHull val = null;
		if (separatedNames.Count == 0 || string.IsNullOrEmpty(separatedNames[0]))
		{
			val = Extensions.GetRandomElement<ShipHull>(shipHulls);
		}
		else if (separatedNames.Count == 1)
		{
			text2 = separatedNames[0];
		}
		else
		{
			if (separatedNames.Count != 2)
			{
				return text;
			}
			text2 = separatedNames[0];
			int.TryParse(separatedNames[1], out result);
		}
		if (result <= 0 || result > 33)
		{
			return $"Ship count must between 0-{33}";
		}
		string text3 = default(string);
		if (val != null || CampaignCheats.TryGetObject<ShipHull>(text2, ref val, ref text3, (Func<ShipHull, bool>)((ShipHull x) => ((List<ShipHull>)(object)shipHulls).Contains(x))))
		{
			for (int num = 0; num < result; num++)
			{
				Ship val2 = new Ship(val);
				ChangeShipOwnerAction.ApplyByLooting(PartyBase.MainParty, val2);
			}
			if (!MobileParty.MainParty.IsCurrentlyAtSea && !MobileParty.MainParty.Anchor.IsValid)
			{
				MobileParty.MainParty.Anchor.SetSettlement(FindAnchorSettlementForParty(MobileParty.MainParty));
			}
			return $"{result} {val.Name} were added to main party.";
		}
		return text3 + "    " + text;
	}

	public static Settlement FindAnchorSettlementForParty(MobileParty party)
	{
		IEnumerable<Town> enumerable = ((IEnumerable<Town>)Town.AllTowns).Where((Town x) => ((SettlementComponent)x).Settlement.HasPort && !party.MapFaction.IsAtWarWith(((SettlementComponent)x).MapFaction));
		if (Extensions.IsEmpty<Town>(enumerable))
		{
			enumerable = ((IEnumerable<Town>)Town.AllTowns).Where((Town x) => ((SettlementComponent)x).Settlement.HasPort);
		}
		return ((SettlementComponent)Extensions.MinBy<Town, float>(enumerable, (Func<Town, float>)delegate(Town x)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			CampaignVec2 portPosition = ((SettlementComponent)x).Settlement.PortPosition;
			return ((CampaignVec2)(ref portPosition)).Distance(party.Position);
		})).Settlement;
	}

	[CommandLineArgumentFunction("unlock_figurehead", "naval")]
	public static string UnlockFigurehead(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string result = "Format is \"naval.unlock_figurehead [figurehead_id or all]\".";
		if (!CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
		{
			return result;
		}
		if (!CampaignCheats.IsPartySuitableToUseCheat(PartyBase.MainParty))
		{
			return "Main party not suitable to take figurehead right now";
		}
		string text = strings[0];
		if (string.Equals(text, "all", StringComparison.OrdinalIgnoreCase))
		{
			foreach (Figurehead item in (List<Figurehead>)(object)MBObjectManager.Instance.GetObjectTypeList<Figurehead>())
			{
				if (!Campaign.Current.UnlockedFigureheadsByMainHero.Contains(item))
				{
					Campaign.Current.UnlockFigurehead(item);
				}
			}
			return "All figureheads unlocked for the player";
		}
		Figurehead val = default(Figurehead);
		string text2 = default(string);
		if (CampaignCheats.TryGetObject<Figurehead>(text, ref val, ref text2, (Func<Figurehead, bool>)null))
		{
			if (!Campaign.Current.UnlockedFigureheadsByMainHero.Contains(val))
			{
				Campaign.Current.UnlockFigurehead(val);
				return $"Figurehead {((PropertyObject)val).Name} is unlocked";
			}
			return "This figurehead already unlocked by the player";
		}
		return "Figurehead with id " + text + " does not exist.";
	}

	[CommandLineArgumentFunction("list_all_ship_names", "naval")]
	public static string ListAllShipNames(List<string> strings)
	{
		string message = string.Empty;
		if (!CheckCheatUsage(ref message))
		{
			return message;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ShipHull item in (List<ShipHull>)(object)MBObjectManager.Instance.GetObjects<ShipHull>((Func<ShipHull, bool>)((ShipHull x) => x != null)))
		{
			stringBuilder.AppendLine(((object)item.Name).ToString() + "   -   " + ((MBObjectBase)item).StringId);
		}
		return stringBuilder.ToString();
	}

	[CommandLineArgumentFunction("list_all_figurehead_names", "naval")]
	public static string ListAllFigureheads(List<string> strings)
	{
		string message = string.Empty;
		if (!CheckCheatUsage(ref message))
		{
			return message;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Figurehead item in (List<Figurehead>)(object)MBObjectManager.Instance.GetObjects<Figurehead>((Func<Figurehead, bool>)((Figurehead x) => x != null)))
		{
			stringBuilder.AppendLine(((object)((PropertyObject)item).Name).ToString() + "   -   " + ((MBObjectBase)item).StringId);
		}
		return stringBuilder.ToString();
	}
}
