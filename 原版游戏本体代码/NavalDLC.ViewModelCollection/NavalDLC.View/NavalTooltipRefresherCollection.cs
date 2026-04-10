using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.View;

public static class NavalTooltipRefresherCollection
{
	private static string ExtendKeyId = "ExtendModifier";

	private static string FollowModifierKeyId = "FollowModifier";

	private static string MapClickKeyId = "MapClick";

	public static void RefreshShipHullTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected O, but got Unknown
		if (args == null || args.Length == 0)
		{
			Debug.FailedAssert("Invalid ship hull arguments for tooltip", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshShipHullTooltip", 28);
			return;
		}
		object obj = args[0];
		ShipHull val;
		if ((val = (ShipHull)((obj is ShipHull) ? obj : null)) == null)
		{
			Debug.FailedAssert("Invalid ship hull arguments for tooltip", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshShipHullTooltip", 34);
			return;
		}
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty(((object)val.Name).ToString(), string.Empty, 0, (TooltipPropertyFlags)4096);
		propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=sqdzHOPe}Class", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("str_ship_type", ((object)val.Type/*cast due to .constrained prefix*/).ToString().ToLowerInvariant())).ToString(), 0, (TooltipPropertyFlags)0);
		propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=UbZL2BJQ}Hitpoints", (Dictionary<string, object>)null)).ToString(), val.MaxHitPoints.ToString(), 0, (TooltipPropertyFlags)0);
		int num = val.TotalCrewCapacity - val.MainDeckCrewCapacity;
		string text = ((num <= 0) ? val.TotalCrewCapacity.ToString() : ((object)new TextObject("{=r2fvxfwZ}{TOTAL} ({MAIN_DECK}+{RESERVE})", (Dictionary<string, object>)null).SetTextVariable("TOTAL", val.TotalCrewCapacity.ToString()).SetTextVariable("MAIN_DECK", val.MainDeckCrewCapacity.ToString()).SetTextVariable("RESERVE", num.ToString())).ToString());
		propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=oqVVGxgb}Crew Capacity", (Dictionary<string, object>)null)).ToString(), text, 0, (TooltipPropertyFlags)0);
	}

	public static void RefreshShipPieceTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Expected O, but got Unknown
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Expected O, but got Unknown
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Expected O, but got Unknown
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Expected O, but got Unknown
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Expected O, but got Unknown
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Expected O, but got Unknown
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Expected O, but got Unknown
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Expected O, but got Unknown
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Expected O, but got Unknown
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Expected O, but got Unknown
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0628: Expected O, but got Unknown
		//IL_0671: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Expected O, but got Unknown
		//IL_06c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Expected O, but got Unknown
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Expected O, but got Unknown
		if (args == null || args.Length == 0)
		{
			Debug.FailedAssert("Invalid ship piece arguments for tooltip", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshShipPieceTooltip", 66);
			return;
		}
		object obj = args[0];
		ShipUpgradePiece val = (ShipUpgradePiece)((obj is ShipUpgradePiece) ? obj : null);
		if (val == null)
		{
			Debug.FailedAssert("Invalid ship piece arguments for tooltip", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshShipPieceTooltip", 73);
			return;
		}
		bool flag = false;
		if (args.Length > 1 && args[1] is bool flag2)
		{
			flag = flag2;
		}
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty(((object)((MBObjectBase)val).GetName()).ToString(), "", 0, (TooltipPropertyFlags)4096);
		if (flag)
		{
			if (val.RequiredCulture1 != null && val.RequiredCulture2 != null)
			{
				TextObject commaSeparatedText = CampaignUIHelper.GetCommaSeparatedText((TextObject)null, (IEnumerable<TextObject>)(object)new TextObject[2]
				{
					val.RequiredCulture1.Name,
					val.RequiredCulture2.Name
				});
				propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=n0R6yfth}Required Cultures", (Dictionary<string, object>)null)).ToString(), ((object)commaSeparatedText).ToString(), 0, (TooltipPropertyFlags)0);
			}
			else if (val.RequiredCulture1 != null || val.RequiredCulture2 != null)
			{
				BasicCultureObject val2 = val.RequiredCulture1 ?? val.RequiredCulture2;
				propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=11c9lb6E}Required Culture", (Dictionary<string, object>)null)).ToString(), ((object)val2.Name).ToString(), 0, (TooltipPropertyFlags)0);
			}
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=gGWVrUPh}Required Port Level", (Dictionary<string, object>)null)).ToString(), val.RequiredPortLevel.ToString(), 0, (TooltipPropertyFlags)0);
			return;
		}
		TextObject val3 = GameTexts.FindText("str_plus_with_number", (string)null);
		if (val.SeaWorthinessBonus != 0)
		{
			val3.SetTextVariable("NUMBER", val.SeaWorthinessBonus);
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=cN03zpII}Seaworthiness", (Dictionary<string, object>)null)).ToString(), ((object)val3).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.AdditionalAmmoBonus != 0)
		{
			val3.SetTextVariable("NUMBER", val.AdditionalAmmoBonus);
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=pJz8SBGB}Additional Ammo Bonus", (Dictionary<string, object>)null)).ToString(), ((object)val3).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.ArcherQuiverBonus != 0)
		{
			val3.SetTextVariable("NUMBER", val.ArcherQuiverBonus);
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=EqJiCbQL}Quivers", (Dictionary<string, object>)null)).ToString(), ((object)val3).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.ThrowingWeaponStackBonus != 0)
		{
			val3.SetTextVariable("NUMBER", val.ThrowingWeaponStackBonus);
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=bbAzBnhC}Throwing Weapon Stacks", (Dictionary<string, object>)null)).ToString(), ((object)val3).ToString(), 0, (TooltipPropertyFlags)0);
		}
		TextObject val4 = GameTexts.FindText("str_NUMBER_percent", (string)null);
		if (val.CrewCapacityBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.CrewCapacityBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=oqVVGxgb}Crew Capacity", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.ShipWeightBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.ShipWeightBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=4Dd2xgPm}Weight", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.DecreaseForwardDragMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.DecreaseForwardDragMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=AOpCa0ZB}Top Speed", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.CampaignSpeedBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.CampaignSpeedBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=DbERaPfF}Travel Speed", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.MaxHitPointsBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.MaxHitPointsBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=lfEJZZfG}Ship Hitpoints", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.MaxSailHitPointsBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.MaxSailHitPointsBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=EAnQtOuG}Sail Hitpoints", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.CrewShieldHitPointsBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.CrewShieldHitPointsBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=4ZbgDw60}Crew Shield Hitpoints", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.InventoryCapacityBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.InventoryCapacityBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=IE1KbkaH}Cargo Capacity", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.MaxOarPowerBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.MaxOarPowerBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=VLugPMkM}Oar Speed", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.MaxOarForceBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.MaxOarForceBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=gOM8Eibs}Oar Power", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.SailForceBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.SailForceBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=ruAdMru6}Sail Power", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.CrewMeleeDamageBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.CrewMeleeDamageBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=vGqCgA6v}Crew Melee Damage", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.SailRotationSpeedBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.SailRotationSpeedBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=idjVMLKe}Sail Rotation Speed", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.RudderSurfaceAreaBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.RudderSurfaceAreaBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=b6dbh1uN}Rudder Effectiveness", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (val.MaxRudderForceBonusMultiplier != 0f)
		{
			val4.SetTextVariable("NUMBER", (val.MaxRudderForceBonusMultiplier * 100f).ToString("#"));
			propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=djdlcniG}Rudder Power", (Dictionary<string, object>)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
	}

	public static void RefreshFigureheadTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		Figurehead val;
		if (args == null || args.Length == 0 || (val = (Figurehead)/*isinst with value type is only supported in some contexts*/) == null)
		{
			Debug.FailedAssert("Invalid arguments for figurehead tooltip", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshFigureheadTooltip", 249);
			return;
		}
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty(((object)((PropertyObject)val).Name).ToString(), "", 0, (TooltipPropertyFlags)4096);
		if (val.Culture != null)
		{
			propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_culture", (string)null)).ToString(), ((object)((BasicCultureObject)val.Culture).Name).ToString(), 0, (TooltipPropertyFlags)0);
		}
		StringHelpers.SetEffectIncrementTypeTextVariable("EFFECT_AMOUNT", ((PropertyObject)val).Description, val.EffectAmount, val.EffectIncrementType);
		propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=opVqBNLh}Effect", (Dictionary<string, object>)null)).ToString(), ((object)((PropertyObject)val).Description).ToString(), 0, (TooltipPropertyFlags)0);
	}

	public static void RefreshAnchorPointTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		AnchorPoint val;
		if (args == null || args.Length == 0 || (val = (AnchorPoint)/*isinst with value type is only supported in some contexts*/) == null)
		{
			Debug.FailedAssert("Invalid anchor arguments for tooltip", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshAnchorPointTooltip", 273);
			return;
		}
		if (!val.IsValid)
		{
			Debug.FailedAssert("Anchor tooltip should not be visible when its not at a valid position", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalTooltipRefresherCollection.cs", "RefreshAnchorPointTooltip", 279);
			return;
		}
		propertyBasedTooltipVM.Mode = 1;
		propertyBasedTooltipVM.AddProperty(((object)val.Name).ToString(), "", 0, (TooltipPropertyFlags)4096);
		if (val.IsMovingToPoint)
		{
			return;
		}
		MBReadOnlyList<Settlement> all = Settlement.All;
		Settlement val2 = null;
		for (int i = 0; i < ((List<Settlement>)(object)all).Count; i++)
		{
			if (((List<Settlement>)(object)all)[i].HasPort && val.IsAtSettlement(((List<Settlement>)(object)all)[i]))
			{
				val2 = ((List<Settlement>)(object)all)[i];
				break;
			}
		}
		if (val2 != null)
		{
			TextObject val3 = new TextObject("{=a6vEx1tM}Anchored at {SETTLEMENT}", (Dictionary<string, object>)null).SetTextVariable("SETTLEMENT", ((object)val2.Name).ToString());
			propertyBasedTooltipVM.AddProperty("", ((object)val3).ToString(), 0, (TooltipPropertyFlags)1);
		}
	}

	public static void RefreshSettlementTooltip(PropertyBasedTooltipVM propertyBasedTooltipVM, object[] args)
	{
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Expected O, but got Unknown
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Expected O, but got Unknown
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Expected O, but got Unknown
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_054f: Expected O, but got Unknown
		//IL_0d6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d76: Expected O, but got Unknown
		Settlement settlement = default(Settlement);
		ref Settlement reference = ref settlement;
		object obj = args[0];
		reference = (Settlement)((obj is Settlement) ? obj : null);
		PartyBase settlementAsParty = settlement.Party;
		if (settlementAsParty == null)
		{
			return;
		}
		if (FactionManager.IsAtWarAgainstFaction(settlementAsParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 3;
		}
		else if (settlementAsParty.MapFaction == PartyBase.MainParty.MapFaction || DiplomacyHelper.IsSameFactionAndNotEliminated(settlementAsParty.MapFaction, PartyBase.MainParty.MapFaction))
		{
			propertyBasedTooltipVM.Mode = 2;
		}
		else
		{
			propertyBasedTooltipVM.Mode = 1;
		}
		if (Game.Current.IsDevelopmentMode)
		{
			string text = ((object)settlement.Name).ToString();
			int num = 1;
			string text2 = "";
			if (settlement.IsHideout)
			{
				text2 = settlement.LocationComplex.GetScene("hideout_center", num);
				propertyBasedTooltipVM.AddProperty("", text + "( id: " + settlementAsParty.Id + ")\n(Scene: " + text2 + ")", 1, (TooltipPropertyFlags)0);
			}
			else
			{
				if (settlement.IsFortification)
				{
					num = settlement.Town.GetWallLevel();
					text2 = settlement.LocationComplex.GetScene("center", num);
				}
				else if (settlement.IsVillage)
				{
					text2 = settlement.LocationComplex.GetScene("village_center", num);
				}
				propertyBasedTooltipVM.AddProperty("", text + " (" + text2 + ")", 0, (TooltipPropertyFlags)4096);
			}
			if (settlement.IsFortification)
			{
				propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)512);
				string text3 = "[DEBUG WALL DATA]\n";
				text3 = text3 + "Current wall level: " + settlement.Town.GetWallLevel() + "\n";
				text3 = text3 + "Current wall hp: " + settlement.SettlementTotalWallHitPoints + "\n";
				text3 = text3 + "Max wall hp: " + settlement.MaxWallHitPoints + "\n";
				propertyBasedTooltipVM.AddProperty("", text3, 0, (TooltipPropertyFlags)4096);
			}
		}
		else
		{
			propertyBasedTooltipVM.AddProperty("", ((object)settlement.Name).ToString(), 0, (TooltipPropertyFlags)4096);
		}
		TextObject val = default(TextObject);
		bool flag = !CampaignUIHelper.IsSettlementInformationHidden(settlement, ref val);
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
		propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_owner", (string)null)).ToString(), " ", 0, (TooltipPropertyFlags)0);
		propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)512);
		TextObject val2 = new TextObject("{=!}{PARTY_OWNERS_FACTION}", (Dictionary<string, object>)null);
		TextObject val3 = (TextObject)((settlement.OwnerClan != null) ? ((object)settlement.OwnerClan.Name) : ((object)new TextObject("{=3PzgpFGq}Neutral", (Dictionary<string, object>)null)));
		val2.SetTextVariable("PARTY_OWNERS_FACTION", val3);
		propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_clan", (string)null)).ToString(), ((object)val2).ToString(), 0, (TooltipPropertyFlags)0);
		if (settlementAsParty.MapFaction != null)
		{
			TextObject val4 = new TextObject("{=!}{MAP_FACTION}", (Dictionary<string, object>)null);
			IFaction mapFaction = settlementAsParty.MapFaction;
			val4.SetTextVariable("MAP_FACTION", (TextObject)(((object)((mapFaction != null) ? mapFaction.Name : null)) ?? ((object)new TextObject("{=!}ERROR", (Dictionary<string, object>)null))));
			propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_faction", (string)null)).ToString(), ((object)val4).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (settlement.Culture != null && !TextObject.IsNullOrEmpty(((BasicCultureObject)settlement.Culture).Name))
		{
			TextObject val5 = new TextObject("{=!}{CULTURE}", (Dictionary<string, object>)null);
			val5.SetTextVariable("CULTURE", ((BasicCultureObject)settlement.Culture).Name);
			propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_culture", (string)null)).ToString(), ((object)val5).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (flag)
		{
			if (settlementAsParty.IsSettlement && (settlementAsParty.Settlement.IsVillage || settlementAsParty.Settlement.IsTown || settlementAsParty.Settlement.IsCastle))
			{
				propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
				propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_information", (string)null)).ToString(), " ", 0, (TooltipPropertyFlags)0);
				propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)512);
			}
			if (settlement.IsFortification)
			{
				int wallLevel = settlementAsParty.Settlement.Town.GetWallLevel();
				propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_map_tooltip_wall_level", (string)null)).ToString(), wallLevel.ToString(), 0, (TooltipPropertyFlags)0);
			}
			Building val6 = settlement.Town?.GetShipyard();
			if (val6 != null)
			{
				propertyBasedTooltipVM.AddProperty(((object)new TextObject("{=NfhYN9yt}Shipyard Level", (Dictionary<string, object>)null)).ToString(), val6.CurrentLevel.ToString(), 0, (TooltipPropertyFlags)0);
			}
			if (settlement.IsFortification)
			{
				Func<string> func = delegate
				{
					//IL_0034: Unknown result type (might be due to invalid IL or missing references)
					//IL_0039: Unknown result type (might be due to invalid IL or missing references)
					//IL_0046: Unknown result type (might be due to invalid IL or missing references)
					//IL_005a: Unknown result type (might be due to invalid IL or missing references)
					//IL_006c: Expected O, but got Unknown
					int num7 = (int)settlementAsParty.Settlement.Town.FoodChange;
					int num8 = (int)((Fief)settlementAsParty.Settlement.Town).FoodStocks;
					TextObject val13 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})", (Dictionary<string, object>)null);
					val13.SetTextVariable("VALUE", num8);
					val13.SetTextVariable("POSITIVE", (num7 > 0) ? 1 : 0);
					val13.SetTextVariable("DELTA_VALUE", num7);
					return ((object)val13).ToString();
				};
				propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_map_tooltip_food_stocks", (string)null)).ToString(), func, 0, (TooltipPropertyFlags)0);
			}
			if (settlement.IsVillage || settlement.IsFortification)
			{
				Func<string> func2 = delegate
				{
					//IL_0085: Unknown result type (might be due to invalid IL or missing references)
					//IL_008a: Unknown result type (might be due to invalid IL or missing references)
					//IL_0097: Unknown result type (might be due to invalid IL or missing references)
					//IL_00af: Unknown result type (might be due to invalid IL or missing references)
					//IL_00c2: Expected O, but got Unknown
					float num7 = (settlementAsParty.Settlement.IsFortification ? settlementAsParty.Settlement.Town.ProsperityChange : settlementAsParty.Settlement.Village.HearthChange);
					int num8 = (int)(settlementAsParty.Settlement.IsFortification ? settlementAsParty.Settlement.Town.Prosperity : settlementAsParty.Settlement.Village.Hearth);
					TextObject val13 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})", (Dictionary<string, object>)null);
					val13.SetTextVariable("VALUE", num8);
					val13.SetTextVariable("POSITIVE", (num7 > 0f) ? 1 : 0);
					val13.SetTextVariable("DELTA_VALUE", num7, 2);
					return ((object)val13).ToString();
				};
				propertyBasedTooltipVM.AddProperty(settlementAsParty.Settlement.IsFortification ? ((object)GameTexts.FindText("str_map_tooltip_prosperity", (string)null)).ToString() : ((object)GameTexts.FindText("str_map_tooltip_hearths", (string)null)).ToString(), func2, 0, (TooltipPropertyFlags)0);
			}
			if (settlement.IsFortification)
			{
				Func<string> func3 = delegate
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					//IL_004f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0071: Expected O, but got Unknown
					TextObject val13 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})", (Dictionary<string, object>)null);
					val13.SetTextVariable("VALUE", settlement.Town.Loyalty, 2);
					val13.SetTextVariable("POSITIVE", (settlement.Town.LoyaltyChange > 0f) ? 1 : 0);
					val13.SetTextVariable("DELTA_VALUE", settlement.Town.LoyaltyChange, 2);
					return ((object)val13).ToString();
				};
				propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_loyalty", (string)null)).ToString(), func3, 0, (TooltipPropertyFlags)0);
				Func<string> func4 = delegate
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					//IL_004f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0071: Expected O, but got Unknown
					TextObject val13 = new TextObject("{=Jyfkahka}{VALUE} ({?POSITIVE}+{?}{\\?}{DELTA_VALUE})", (Dictionary<string, object>)null);
					val13.SetTextVariable("VALUE", settlement.Town.Security, 2);
					val13.SetTextVariable("POSITIVE", (settlement.Town.SecurityChange > 0f) ? 1 : 0);
					val13.SetTextVariable("DELTA_VALUE", settlement.Town.SecurityChange, 2);
					return ((object)val13).ToString();
				};
				propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_security", (string)null)).ToString(), func4, 0, (TooltipPropertyFlags)0);
			}
		}
		if (settlement.IsVillage)
		{
			string text4 = ((object)GameTexts.FindText("str_bound_settlement", (string)null)).ToString();
			string text5 = ((object)settlementAsParty.Settlement.Village.Bound.Name).ToString();
			propertyBasedTooltipVM.AddProperty(text4, text5, 0, (TooltipPropertyFlags)0);
			if (settlementAsParty.Settlement.Village.TradeBound != null)
			{
				string text6 = ((object)GameTexts.FindText("str_trade_bound_settlement", (string)null)).ToString();
				string text7 = ((object)settlementAsParty.Settlement.Village.TradeBound.Name).ToString();
				propertyBasedTooltipVM.AddProperty(text6, text7, 0, (TooltipPropertyFlags)0);
			}
			ItemObject primaryProduction = settlementAsParty.Settlement.Village.VillageType.PrimaryProduction;
			propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_primary_production", (string)null)).ToString(), ((object)primaryProduction.Name).ToString(), 0, (TooltipPropertyFlags)0);
		}
		if (((List<Village>)(object)settlement.BoundVillages).Count > 0)
		{
			string text8 = ((object)GameTexts.FindText("str_bound_village", (string)null)).ToString();
			IEnumerable<TextObject> enumerable = ((IEnumerable<Village>)settlementAsParty.Settlement.BoundVillages).Select((Village x) => ((SettlementComponent)x).Name);
			propertyBasedTooltipVM.AddProperty(text8, ((object)CampaignUIHelper.GetCommaNewlineSeparatedText(TextObject.GetEmpty(), enumerable)).ToString(), 0, (TooltipPropertyFlags)0);
			if (((TooltipBaseVM)propertyBasedTooltipVM).IsExtended && settlement.IsTown && ((List<Village>)(object)settlement.Town.TradeBoundVillages).Count > 0)
			{
				string text9 = ((object)GameTexts.FindText("str_trade_bound_village", (string)null)).ToString();
				IEnumerable<TextObject> enumerable2 = ((IEnumerable<Village>)settlement.Town.TradeBoundVillages).Select((Village x) => ((SettlementComponent)x).Name);
				propertyBasedTooltipVM.AddProperty(text9, ((object)CampaignUIHelper.GetCommaNewlineSeparatedText(TextObject.GetEmpty(), enumerable2)).ToString(), 0, (TooltipPropertyFlags)0);
			}
		}
		if (Game.Current.IsDevelopmentMode && settlement.IsTown)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
			propertyBasedTooltipVM.AddProperty("[DEV] " + ((object)GameTexts.FindText("str_shops", (string)null)).ToString(), " ", 0, (TooltipPropertyFlags)0);
			propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)512);
			int num2 = 1;
			Workshop[] workshops = settlementAsParty.Settlement.Town.Workshops;
			foreach (Workshop val7 in workshops)
			{
				if (val7.WorkshopType != null)
				{
					propertyBasedTooltipVM.AddProperty("[DEV] Shop " + num2, ((object)val7.WorkshopType.Name).ToString(), 0, (TooltipPropertyFlags)0);
					num2++;
				}
			}
		}
		TroopRoster val8 = TroopRoster.CreateDummyTroopRoster();
		TroopRoster val9 = TroopRoster.CreateDummyTroopRoster();
		TroopRoster.CreateDummyTroopRoster();
		Func<TroopRoster> func5 = delegate
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			TroopRoster val13 = TroopRoster.CreateDummyTroopRoster();
			foreach (MobileParty item in (List<MobileParty>)(object)settlement.Parties)
			{
				if (!FactionManager.IsAtWarAgainstFaction(item.MapFaction, settlementAsParty.MapFaction) && (!(item.Aggressiveness < 0.01f) || item.IsGarrison || item.IsMilitia) && !item.IsMainParty)
				{
					for (int i = 0; i < item.MemberRoster.Count; i++)
					{
						TroopRosterElement elementCopyAtIndex = item.MemberRoster.GetElementCopyAtIndex(i);
						val13.AddToCounts(elementCopyAtIndex.Character, ((TroopRosterElement)(ref elementCopyAtIndex)).Number, false, ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber, 0, true, -1);
					}
				}
			}
			return val13;
		};
		Func<TroopRoster> func6 = delegate
		{
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			TroopRoster val13 = TroopRoster.CreateDummyTroopRoster();
			foreach (MobileParty item2 in (List<MobileParty>)(object)settlement.Parties)
			{
				if (!item2.IsMainParty && !FactionManager.IsAtWarAgainstFaction(item2.MapFaction, settlementAsParty.MapFaction))
				{
					for (int i = 0; i < item2.PrisonRoster.Count; i++)
					{
						TroopRosterElement elementCopyAtIndex = item2.PrisonRoster.GetElementCopyAtIndex(i);
						val13.AddToCounts(elementCopyAtIndex.Character, ((TroopRosterElement)(ref elementCopyAtIndex)).Number, false, ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber, 0, true, -1);
					}
				}
			}
			for (int j = 0; j < settlementAsParty.PrisonRoster.Count; j++)
			{
				TroopRosterElement elementCopyAtIndex2 = settlementAsParty.PrisonRoster.GetElementCopyAtIndex(j);
				val13.AddToCounts(elementCopyAtIndex2.Character, ((TroopRosterElement)(ref elementCopyAtIndex2)).Number, false, ((TroopRosterElement)(ref elementCopyAtIndex2)).WoundedNumber, 0, true, -1);
			}
			return val13;
		};
		val9 = func6();
		if (!settlement.IsHideout && ((TooltipBaseVM)propertyBasedTooltipVM).IsExtended)
		{
			val8 = func5();
			if (val8.Count > 0)
			{
				AddPartyTroopProperties(propertyBasedTooltipVM, val8, GameTexts.FindText("str_map_tooltip_troops", (string)null), flag, func5);
			}
		}
		else if (!settlement.IsHideout)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
			if (flag)
			{
				List<MobileParty> list = new List<MobileParty>();
				Town town = settlement.Town;
				bool flag2 = town == null || !town.InRebelliousState;
				for (int num4 = 0; num4 < ((List<MobileParty>)(object)settlement.Parties).Count; num4++)
				{
					MobileParty val10 = ((List<MobileParty>)(object)settlement.Parties)[num4];
					bool flag3 = flag2 && val10.IsMilitia;
					if (DiplomacyHelper.IsSameFactionAndNotEliminated(settlementAsParty.MapFaction, val10.MapFaction) && (val10.IsLordParty || flag3 || val10.IsGarrison))
					{
						list.Add(val10);
					}
				}
				list.Sort((IComparer<MobileParty>?)CampaignUIHelper.MobilePartyPrecedenceComparerInstance);
				List<MobileParty> list2 = ((IEnumerable<MobileParty>)settlement.Parties).Where((MobileParty p) => !p.IsLordParty && !p.IsMilitia && !p.IsGarrison).ToList();
				list2.Sort((IComparer<MobileParty>?)CampaignUIHelper.MobilePartyPrecedenceComparerInstance);
				if (list.Count > 0)
				{
					int num5 = list.Sum((MobileParty p) => p.Party.NumberOfHealthyMembers);
					int num6 = list.Sum((MobileParty p) => p.Party.NumberOfWoundedTotalMembers);
					string text10 = num5 + ((num6 > 0) ? ("+" + num6 + ((object)GameTexts.FindText("str_party_nameplate_wounded_abbr", (string)null)).ToString()) : "");
					propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_map_tooltip_defenders", (string)null)).ToString(), text10, 0, (TooltipPropertyFlags)0);
					propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)512);
					foreach (MobileParty item3 in list)
					{
						propertyBasedTooltipVM.AddProperty(((object)item3.Name).ToString(), CampaignUIHelper.GetPartyNameplateText(item3, false), 0, (TooltipPropertyFlags)0);
					}
					propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
				}
				if (list2.Count > 0)
				{
					propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)1024);
					foreach (MobileParty item4 in list2)
					{
						propertyBasedTooltipVM.AddProperty(((object)item4.Name).ToString(), CampaignUIHelper.GetPartyNameplateText(item4, false), 0, (TooltipPropertyFlags)0);
					}
				}
			}
			else
			{
				string text11 = ((object)GameTexts.FindText("str_missing_info_indicator", (string)null)).ToString();
				propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_map_tooltip_parties", (string)null)).ToString(), text11, 0, (TooltipPropertyFlags)0);
			}
		}
		if (!settlement.IsHideout && val9.Count > 0 && flag)
		{
			AddPartyTroopProperties(propertyBasedTooltipVM, val9, GameTexts.FindText("str_map_tooltip_prisoners", (string)null), flag, func6);
		}
		if (settlement.IsFortification && settlement.Town.InRebelliousState)
		{
			propertyBasedTooltipVM.AddProperty(string.Empty, ((object)GameTexts.FindText("str_settlement_rebellious_state", (string)null)).ToString(), -1, (TooltipPropertyFlags)0);
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
		if (!settlement.IsHideout && !((TooltipBaseVM)propertyBasedTooltipVM).IsExtended && flag)
		{
			TextObject val11 = GameTexts.FindText("str_map_tooltip_info", (string)null);
			val11.SetTextVariable("EXTEND_KEY", propertyBasedTooltipVM.GetKeyText(ExtendKeyId));
			propertyBasedTooltipVM.AddProperty(string.Empty, ((object)val11).ToString(), -1, (TooltipPropertyFlags)0);
		}
		if (Campaign.Current.Models.EncounterModel.CanMainHeroDoParleyWithParty(settlementAsParty, ref val))
		{
			TextObject val12 = new TextObject("{=uEeLvYXT}Press '{MODIFIER_KEY}' + '{CLICK_KEY}' to parley.", (Dictionary<string, object>)null);
			val12.SetTextVariable("MODIFIER_KEY", propertyBasedTooltipVM.GetKeyText(FollowModifierKeyId));
			val12.SetTextVariable("CLICK_KEY", propertyBasedTooltipVM.GetKeyText(MapClickKeyId));
			propertyBasedTooltipVM.AddProperty(string.Empty, ((object)val12).ToString(), -1, (TooltipPropertyFlags)0);
		}
	}

	private static void AddPartyTroopProperties(PropertyBasedTooltipVM propertyBasedTooltipVM, TroopRoster troopRoster, TextObject title, bool isInspected, Func<TroopRoster> funcToDoBeforeLambda = null)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
		propertyBasedTooltipVM.AddProperty(((object)title).ToString(), (Func<string>)delegate
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			TroopRoster val3 = ((funcToDoBeforeLambda != null) ? funcToDoBeforeLambda() : troopRoster);
			int num5 = 0;
			int num6 = 0;
			for (int i = 0; i < val3.Count; i++)
			{
				TroopRosterElement elementCopyAtIndex3 = val3.GetElementCopyAtIndex(i);
				num5 += ((TroopRosterElement)(ref elementCopyAtIndex3)).Number - ((TroopRosterElement)(ref elementCopyAtIndex3)).WoundedNumber;
				num6 += ((TroopRosterElement)(ref elementCopyAtIndex3)).WoundedNumber;
			}
			TextObject val4 = new TextObject("{=iXXTONWb} ({PARTY_SIZE})", (Dictionary<string, object>)null);
			val4.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(num5, num6, isInspected));
			return ((object)val4).ToString();
		}, 0, (TooltipPropertyFlags)0);
		if (isInspected)
		{
			propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)512);
		}
		if (isInspected)
		{
			Dictionary<FormationClass, Tuple<int, int>> dictionary = new Dictionary<FormationClass, Tuple<int, int>>();
			for (int num = 0; num < troopRoster.Count; num++)
			{
				TroopRosterElement elementCopyAtIndex = troopRoster.GetElementCopyAtIndex(num);
				if (dictionary.ContainsKey(((BasicCharacterObject)elementCopyAtIndex.Character).DefaultFormationClass))
				{
					Tuple<int, int> tuple = dictionary[((BasicCharacterObject)elementCopyAtIndex.Character).DefaultFormationClass];
					dictionary[((BasicCharacterObject)elementCopyAtIndex.Character).DefaultFormationClass] = new Tuple<int, int>(tuple.Item1 + ((TroopRosterElement)(ref elementCopyAtIndex)).Number - ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber, tuple.Item2 + ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber);
				}
				else
				{
					dictionary.Add(((BasicCharacterObject)elementCopyAtIndex.Character).DefaultFormationClass, new Tuple<int, int>(((TroopRosterElement)(ref elementCopyAtIndex)).Number - ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber, ((TroopRosterElement)(ref elementCopyAtIndex)).WoundedNumber));
				}
			}
			foreach (KeyValuePair<FormationClass, Tuple<int, int>> item in dictionary.OrderBy((KeyValuePair<FormationClass, Tuple<int, int>> x) => x.Key))
			{
				TextObject val = new TextObject("{=Dqydb21E} {PARTY_SIZE}", (Dictionary<string, object>)null);
				val.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(item.Value.Item1, item.Value.Item2, true));
				TextObject val2 = GameTexts.FindText("str_troop_type_name", FormationClassExtensions.GetName(item.Key));
				propertyBasedTooltipVM.AddProperty(((object)val2).ToString(), ((object)val).ToString(), 0, (TooltipPropertyFlags)0);
			}
		}
		if (!(((TooltipBaseVM)propertyBasedTooltipVM).IsExtended && isInspected))
		{
			return;
		}
		propertyBasedTooltipVM.AddProperty(string.Empty, string.Empty, -1, (TooltipPropertyFlags)0);
		propertyBasedTooltipVM.AddProperty(((object)GameTexts.FindText("str_troop_types", (string)null)).ToString(), " ", 0, (TooltipPropertyFlags)0);
		propertyBasedTooltipVM.AddProperty("", "", 0, (TooltipPropertyFlags)1024);
		for (int num2 = 0; num2 < troopRoster.Count; num2++)
		{
			TroopRosterElement elementCopyAtIndex2 = troopRoster.GetElementCopyAtIndex(num2);
			if (!((BasicCharacterObject)elementCopyAtIndex2.Character).IsHero)
			{
				continue;
			}
			CharacterObject hero = elementCopyAtIndex2.Character;
			propertyBasedTooltipVM.AddProperty(((object)((BasicCharacterObject)elementCopyAtIndex2.Character).Name).ToString(), (Func<string>)delegate
			{
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				TroopRoster val3 = ((funcToDoBeforeLambda != null) ? funcToDoBeforeLambda() : troopRoster);
				int num5 = val3.FindIndexOfTroop(hero);
				if (num5 == -1)
				{
					return string.Empty;
				}
				TroopRosterElement elementCopyAtIndex3 = val3.GetElementCopyAtIndex(num5);
				TextObject obj = GameTexts.FindText("str_NUMBER_percent", (string)null);
				obj.SetTextVariable("NUMBER", elementCopyAtIndex3.Character.HeroObject.HitPoints * 100 / ((BasicCharacterObject)elementCopyAtIndex3.Character).MaxHitPoints());
				return ((object)obj).ToString();
			}, 0, (TooltipPropertyFlags)0);
		}
		for (int num3 = 0; num3 < troopRoster.Count; num3++)
		{
			int num4 = num3;
			CharacterObject character = troopRoster.GetElementCopyAtIndex(num4).Character;
			if (((BasicCharacterObject)character).IsHero)
			{
				continue;
			}
			propertyBasedTooltipVM.AddProperty(((object)((BasicCharacterObject)character).Name).ToString(), (Func<string>)delegate
			{
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_0053: Unknown result type (might be due to invalid IL or missing references)
				//IL_0061: Unknown result type (might be due to invalid IL or missing references)
				//IL_007e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0083: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b0: Expected O, but got Unknown
				TroopRoster val3 = ((funcToDoBeforeLambda != null) ? funcToDoBeforeLambda() : troopRoster);
				int num5 = val3.FindIndexOfTroop(character);
				if (num5 != -1)
				{
					if (num5 > val3.Count)
					{
						return string.Empty;
					}
					TroopRosterElement elementCopyAtIndex3 = val3.GetElementCopyAtIndex(num5);
					if (elementCopyAtIndex3.Character == null)
					{
						return string.Empty;
					}
					CharacterObject character2 = elementCopyAtIndex3.Character;
					if (character2 != null && !((BasicCharacterObject)character2).IsHero)
					{
						TextObject val4 = new TextObject("{=!}{PARTY_SIZE}", (Dictionary<string, object>)null);
						val4.SetTextVariable("PARTY_SIZE", PartyBaseHelper.GetPartySizeText(((TroopRosterElement)(ref elementCopyAtIndex3)).Number - ((TroopRosterElement)(ref elementCopyAtIndex3)).WoundedNumber, ((TroopRosterElement)(ref elementCopyAtIndex3)).WoundedNumber, true));
						return ((object)val4).ToString();
					}
				}
				return string.Empty;
			}, 0, (TooltipPropertyFlags)0);
		}
	}
}
