using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection;

public static class NavalUIHelper
{
	public static float GetHealthPercent(this Ship ship)
	{
		if (ship.MaxHitPoints == 0f)
		{
			return 0f;
		}
		return ship.HitPoints / ship.MaxHitPoints * 100f;
	}

	private static Tuple<bool, TextObject> GetIsStringApplicableForShipName(string name)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		if (string.IsNullOrEmpty(name))
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=aw2fR5fK}Ship name cannot be empty", (Dictionary<string, object>)null));
		}
		if ((name.Length < 3 && !name.Any((char c) => Common.IsCharAsian(c))) || name.Length > 50)
		{
			TextObject val = new TextObject("{=cSLiAJUw}Ship name should be between {MIN} and {MAX} characters", (Dictionary<string, object>)null);
			val.SetTextVariable("MIN", 3);
			val.SetTextVariable("MAX", 50);
			return new Tuple<bool, TextObject>(item1: false, val);
		}
		if (!name.All((char x) => (char.IsLetterOrDigit(x) || char.IsWhiteSpace(x) || char.IsPunctuation(x)) && x != '{' && x != '}'))
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=t9bmsau2}Ship name cannot contain special characters", (Dictionary<string, object>)null));
		}
		if (name.StartsWith(" ") || name.EndsWith(" "))
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=ol9uYvPl}Ship name cannot start or end with a white space", (Dictionary<string, object>)null));
		}
		if (name.Contains("  "))
		{
			return new Tuple<bool, TextObject>(item1: false, new TextObject("{=bX4OPIPP}Ship name cannot contain consecutive white spaces", (Dictionary<string, object>)null));
		}
		return new Tuple<bool, TextObject>(item1: true, TextObject.GetEmpty());
	}

	public static Tuple<bool, string> IsStringApplicableForShipName(string name)
	{
		Tuple<bool, TextObject> isStringApplicableForShipName = GetIsStringApplicableForShipName(name);
		return new Tuple<bool, string>(isStringApplicableForShipName.Item1, ((object)isStringApplicableForShipName.Item2).ToString());
	}

	public static Ship GetFlagship(PartyBase party)
	{
		return party.FlagShip;
	}

	public static List<TooltipProperty> GetShipyardTooltip(Town town)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		if (town == null)
		{
			return new List<TooltipProperty>();
		}
		List<TooltipProperty> list = new List<TooltipProperty>();
		Building shipyard = town.GetShipyard();
		list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=4vkUyYui}Shipyard{newline}Level {LEVEL}", (Dictionary<string, object>)null).SetTextVariable("LEVEL", shipyard.CurrentLevel)).ToString(), 0, false, (TooltipPropertyFlags)0));
		return list;
	}

	public static string GetTownCoastalPatrolTooltip(Town town)
	{
		TextObject val = GameTexts.FindText("str_string_newline_string", (string)null);
		val.SetTextVariable("newline", "\n");
		val.SetTextVariable("STR1", GameTexts.FindText("str_coastal_patrol", (string)null));
		INavalPatrolPartiesCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<INavalPatrolPartiesCampaignBehavior>();
		TextObject val2 = default(TextObject);
		if (CampaignUIHelper.IsSettlementInformationHidden(((SettlementComponent)town).Settlement, ref val2))
		{
			val.SetTextVariable("STR2", ((object)GameTexts.FindText("str_missing_info_indicator", (string)null)).ToString());
		}
		else if (campaignBehavior.GetNavalPatrolParty(((SettlementComponent)town).Settlement) != null)
		{
			val.SetTextVariable("STR2", ((object)campaignBehavior.GetNavalPatrolParty(((SettlementComponent)town).Settlement).GetBehaviorText()).ToString());
		}
		else
		{
			val.SetTextVariable("STR2", ((object)campaignBehavior.GetSettlementPatrolStatus(((SettlementComponent)town).Settlement)).ToString());
		}
		return ((object)val).ToString();
	}

	public static string GetPrefabIdOfShipHull(ShipHull shipHull)
	{
		MissionShipObject obj = MBObjectManager.Instance.GetObject<MissionShipObject>(shipHull.MissionShipObjectId);
		return ((obj != null) ? obj.Prefab : null) ?? string.Empty;
	}
}
