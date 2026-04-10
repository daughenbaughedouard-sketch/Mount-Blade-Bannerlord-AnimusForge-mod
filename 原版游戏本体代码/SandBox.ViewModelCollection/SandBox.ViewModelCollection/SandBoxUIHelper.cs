using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection;

public static class SandBoxUIHelper
{
	public enum SortState
	{
		Default,
		Ascending,
		Descending
	}

	public enum MapEventVisualTypes
	{
		None,
		Raid,
		Siege,
		Battle,
		Rebellion,
		SallyOut
	}

	public const float AgentMarkerWorldHeightOffset = 0.35f;

	private static readonly TextObject _soldStr = new TextObject("{=YgyHVu8S}Sold{ITEMS}", (Dictionary<string, object>)null);

	private static readonly TextObject _purchasedStr = new TextObject("{=qIeDZoSx}Purchased{ITEMS}", (Dictionary<string, object>)null);

	private static readonly TextObject _itemTransactionStr = new TextObject("{=CqAhj27p} {ITEM_NAME} x{ITEM_NUMBER}", (Dictionary<string, object>)null);

	private static readonly TextObject _lootStr = new TextObject("{=nvemmBZz}You earned {AMOUNT}% of the loot and prisoners", (Dictionary<string, object>)null);

	private static void TooltipAddExplanation(List<TooltipProperty> properties, ref ExplainedNumber explainedNumber)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		List<(string, float)> lines = ((ExplainedNumber)(ref explainedNumber)).GetLines();
		if (lines.Count > 0)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				(string, float) tuple = lines[i];
				string item = tuple.Item1;
				string changeValueString = GetChangeValueString(tuple.Item2);
				properties.Add(new TooltipProperty(item, changeValueString, 0, false, (TooltipPropertyFlags)0));
			}
		}
	}

	public static List<TooltipProperty> GetExplainedNumberTooltip(ref ExplainedNumber explanation)
	{
		List<TooltipProperty> list = new List<TooltipProperty>();
		TooltipAddExplanation(list, ref explanation);
		return list;
	}

	public static List<TooltipProperty> GetBattleLootAwardTooltip(float lootPercentage)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		GameTexts.SetVariable("AMOUNT", lootPercentage);
		list.Add(new TooltipProperty(string.Empty, ((object)_lootStr).ToString(), 0, false, (TooltipPropertyFlags)0));
		return list;
	}

	public static List<TooltipProperty> GetFigureheadTooltip(Figurehead figurehead)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		object[] array = new object[1] { figurehead };
		return ((IEnumerable<TooltipProperty>)new PropertyBasedTooltipVM(typeof(Figurehead), array).TooltipPropertyList).ToList();
	}

	public static string GetSkillEffectText(SkillEffect effect, int skillLevel)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		TextObject effectDescriptionForSkillLevel = SkillHelper.GetEffectDescriptionForSkillLevel(effect, skillLevel);
		if ((int)effect.Role != 0)
		{
			TextObject val = GameTexts.FindText("role", ((object)effect.Role/*cast due to .constrained prefix*/).ToString());
			return $"({((object)val).ToString()}) {effectDescriptionForSkillLevel} ";
		}
		return ((object)effectDescriptionForSkillLevel).ToString();
	}

	public static string GetRecruitNotificationText(int recruitmentAmount)
	{
		TextObject obj = GameTexts.FindText("str_settlement_recruit_notification", (string)null);
		MBTextManager.SetTextVariable("RECRUIT_AMOUNT", recruitmentAmount);
		MBTextManager.SetTextVariable("ISPLURAL", (object)(recruitmentAmount > 1));
		return ((object)obj).ToString();
	}

	public static string GetItemSoldNotificationText(ItemRosterElement item, int itemAmount, bool fromHeroToSettlement)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref item)).EquipmentElement;
		string text = ((object)((MBObjectBase)((EquipmentElement)(ref equipmentElement)).Item.ItemCategory).GetName()).ToString();
		TextObject obj = GameTexts.FindText("str_settlement_item_sold_notification", (string)null);
		MBTextManager.SetTextVariable("IS_POSITIVE", (object)(!fromHeroToSettlement));
		MBTextManager.SetTextVariable("ITEM_AMOUNT", itemAmount);
		MBTextManager.SetTextVariable("ITEM_TYPE", text, false);
		return ((object)obj).ToString();
	}

	public static string GetShipSoldNotificationText(Ship ship, int itemAmount, bool fromHeroToSettlement)
	{
		string text = ((object)ship.ShipHull.Name).ToString();
		TextObject val = ((!fromHeroToSettlement) ? _purchasedStr : _soldStr);
		TextObject itemTransactionStr = _itemTransactionStr;
		itemTransactionStr.SetTextVariable("ITEM_NAME", text);
		itemTransactionStr.SetTextVariable("ITEM_NUMBER", itemAmount);
		val.SetTextVariable("ITEMS", ((object)itemTransactionStr).ToString());
		return ((object)val).ToString();
	}

	public static string GetTroopGivenToSettlementNotificationText(int givenAmount)
	{
		TextObject obj = GameTexts.FindText("str_settlement_given_troop_notification", (string)null);
		MBTextManager.SetTextVariable("TROOP_AMOUNT", givenAmount);
		MBTextManager.SetTextVariable("ISPLURAL", (object)(givenAmount > 1));
		return ((object)obj).ToString();
	}

	internal static string GetItemsTradedNotificationText(List<(EquipmentElement, int)> items, bool isSelling)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = ((!isSelling) ? _purchasedStr : _soldStr);
		List<IGrouping<ItemCategory, (EquipmentElement, int)>> list = (from i in items
			group i by ((EquipmentElement)(ref i.Item1)).Item.ItemCategory into i
			orderby i.Sum(((EquipmentElement, int) e) => e.Item2 * ((EquipmentElement)(ref e.Item1)).Item.Value)
			select i).ToList();
		MBStringBuilder val2 = default(MBStringBuilder);
		((MBStringBuilder)(ref val2)).Initialize(16, "GetItemsTradedNotificationText");
		int num = MathF.Min(3, list.Count);
		for (int num2 = 0; num2 < num; num2++)
		{
			IGrouping<ItemCategory, (EquipmentElement, int)> grouping = list[num2];
			int num3 = MathF.Abs(grouping.Sum(((EquipmentElement, int) x) => x.Item2));
			((object)((MBObjectBase)grouping.Key).GetName()).ToString();
			_itemTransactionStr.SetTextVariable("ITEM_NAME", ((MBObjectBase)grouping.Key).GetName());
			_itemTransactionStr.SetTextVariable("ITEM_NUMBER", num3);
			((MBStringBuilder)(ref val2)).Append<string>(((object)_itemTransactionStr).ToString());
		}
		val.SetTextVariable("ITEMS", ((MBStringBuilder)(ref val2)).ToStringAndRelease());
		return ((object)val).ToString();
	}

	public static List<TooltipProperty> GetSiegeEngineInProgressTooltip(SiegeEngineConstructionProgress engineInProgress)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		if (engineInProgress?.SiegeEngine != null)
		{
			int num = ((IEnumerable<SiegeEngineConstructionProgress>)PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide(PlayerSiege.PlayerSide).SiegeEngines.DeployedSiegeEngines).Where((SiegeEngineConstructionProgress e) => !e.IsActive).ToList().IndexOf(engineInProgress);
			list = GetSiegeEngineTooltip(engineInProgress.SiegeEngine);
			if (engineInProgress.IsActive)
			{
				string text = ((int)(engineInProgress.Hitpoints / engineInProgress.MaxHitPoints * 100f)).ToString();
				GameTexts.SetVariable("NUMBER", text);
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_NUMBER_percent", (string)null));
				GameTexts.SetVariable("LEFT", ((int)engineInProgress.Hitpoints).ToString());
				GameTexts.SetVariable("RIGHT", ((int)engineInProgress.MaxHitPoints).ToString());
				GameTexts.SetVariable("STR2", GameTexts.FindText("str_LEFT_over_RIGHT_in_paranthesis", (string)null));
				list.Add(new TooltipProperty(((object)GameTexts.FindText("str_hitpoints", (string)null)).ToString(), ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
			}
			else
			{
				string text2 = MathF.Round((engineInProgress.IsBeingRedeployed ? engineInProgress.RedeploymentProgress : engineInProgress.Progress) / 1f * 100f).ToString();
				GameTexts.SetVariable("NUMBER", text2);
				TextObject val = (engineInProgress.IsBeingRedeployed ? GameTexts.FindText("str_redeploy", (string)null) : GameTexts.FindText("str_inprogress", (string)null));
				list.Add(new TooltipProperty(((object)val).ToString(), ((object)GameTexts.FindText("str_NUMBER_percent", (string)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
				if (num == 0)
				{
					list.Add(new TooltipProperty(((object)GameTexts.FindText("str_currently_building", (string)null)).ToString(), " ", 0, false, (TooltipPropertyFlags)0));
				}
				else if (num > 0)
				{
					list.Add(new TooltipProperty(((object)GameTexts.FindText("str_in_queue", (string)null)).ToString(), num.ToString(), 0, false, (TooltipPropertyFlags)0));
				}
			}
		}
		return list;
	}

	public static List<TooltipProperty> GetSiegeEngineTooltip(SiegeEngineType engine)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		if (PlayerSiege.PlayerSiegeEvent != null && engine != null)
		{
			list.Add(new TooltipProperty("", ((object)engine.Name).ToString(), 0, false, (TooltipPropertyFlags)4096));
			list.Add(new TooltipProperty("", ((object)engine.Description).ToString(), 0, false, (TooltipPropertyFlags)1));
			list.Add(new TooltipProperty(((object)new TextObject("{=Ahy035gM}Build Cost", (Dictionary<string, object>)null)).ToString(), engine.ManDayCost.ToString("F1"), 0, false, (TooltipPropertyFlags)0));
			float siegeEngineHitPoints = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineHitPoints(PlayerSiege.PlayerSiegeEvent, engine, PlayerSiege.PlayerSide);
			list.Add(new TooltipProperty(((object)new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null)).ToString(), siegeEngineHitPoints.ToString(), 0, false, (TooltipPropertyFlags)0));
			if (engine.Difficulty > 0)
			{
				list.Add(new TooltipProperty(((object)new TextObject("{=raD9MK3O}Difficulty", (Dictionary<string, object>)null)).ToString(), engine.Difficulty.ToString(), 0, false, (TooltipPropertyFlags)0));
			}
			if (engine.ToolCost > 0)
			{
				list.Add(new TooltipProperty(((object)new TextObject("{=lPMYSSAa}Tools Required", (Dictionary<string, object>)null)).ToString(), engine.ToolCost.ToString(), 0, false, (TooltipPropertyFlags)0));
			}
			if (engine.IsRanged)
			{
				list.Add(new TooltipProperty(((object)GameTexts.FindText("str_daily_rate_of_fire", (string)null)).ToString(), engine.CampaignRateOfFirePerDay.ToString("F1"), 0, false, (TooltipPropertyFlags)0));
				list.Add(new TooltipProperty(((object)GameTexts.FindText("str_projectile_damage", (string)null)).ToString(), engine.Damage.ToString("F1"), 0, false, (TooltipPropertyFlags)0));
				list.Add(new TooltipProperty(" ", " ", 0, false, (TooltipPropertyFlags)0));
			}
		}
		return list;
	}

	public static List<TooltipProperty> GetWallSectionTooltip(Settlement settlement, int wallIndex)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		if (settlement.IsFortification)
		{
			list.Add(new TooltipProperty("", ((object)GameTexts.FindText("str_wall", (string)null)).ToString(), 0, false, (TooltipPropertyFlags)4096));
			list.Add(new TooltipProperty(" ", " ", 0, false, (TooltipPropertyFlags)0));
			float maxHitPointsOfOneWallSection = settlement.MaxHitPointsOfOneWallSection;
			float num = ((List<float>)(object)settlement.SettlementWallSectionHitPointsRatioList)[wallIndex] * maxHitPointsOfOneWallSection;
			if (num > 0f)
			{
				string text = ((int)(num / maxHitPointsOfOneWallSection * 100f)).ToString();
				GameTexts.SetVariable("NUMBER", text);
				GameTexts.SetVariable("STR1", GameTexts.FindText("str_NUMBER_percent", (string)null));
				GameTexts.SetVariable("LEFT", ((int)num).ToString());
				GameTexts.SetVariable("RIGHT", ((int)maxHitPointsOfOneWallSection).ToString());
				GameTexts.SetVariable("STR2", GameTexts.FindText("str_LEFT_over_RIGHT_in_paranthesis", (string)null));
				list.Add(new TooltipProperty(((object)GameTexts.FindText("str_hitpoints", (string)null)).ToString(), ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
			}
			else
			{
				list.Add(new TooltipProperty(((object)GameTexts.FindText("str_wall_breached", (string)null)).ToString(), " ", 0, false, (TooltipPropertyFlags)0));
			}
		}
		return list;
	}

	public static string GetPrisonersSoldNotificationText(int soldPrisonerAmount)
	{
		TextObject obj = GameTexts.FindText("str_settlement_prisoner_sold_notification", (string)null);
		MBTextManager.SetTextVariable("PRISONERS_AMOUNT", soldPrisonerAmount);
		MBTextManager.SetTextVariable("ISPLURAL", (object)(soldPrisonerAmount > 1));
		return ((object)obj).ToString();
	}

	public static int GetPartyHealthyCount(MobileParty party)
	{
		int num = party.Party.NumberOfHealthyMembers;
		if (party.Army != null && party.Army.LeaderParty == party)
		{
			foreach (MobileParty item in (List<MobileParty>)(object)party.Army.LeaderParty.AttachedParties)
			{
				num += item.Party.NumberOfHealthyMembers;
			}
		}
		return num;
	}

	public static string GetPartyWoundedText(int woundedAmount)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		TextObject val = new TextObject("{=O9nwLrYp}+{WOUNDEDAMOUNT}w", (Dictionary<string, object>)null);
		val.SetTextVariable("WOUNDEDAMOUNT", woundedAmount);
		return ((object)val).ToString();
	}

	public static string GetPartyPrisonerText(int prisonerAmount)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		TextObject val = new TextObject("{=CiIWjF3f}+{PRISONERAMOUNT}p", (Dictionary<string, object>)null);
		val.SetTextVariable("PRISONERAMOUNT", prisonerAmount);
		return ((object)val).ToString();
	}

	public static int GetAllWoundedMembersAmount(MobileParty party)
	{
		int num = party.Party.NumberOfWoundedTotalMembers;
		if (party.Army != null && party.Army.LeaderParty == party)
		{
			num += ((IEnumerable<MobileParty>)party.Army.LeaderParty.AttachedParties).Sum((MobileParty p) => p.Party.NumberOfWoundedTotalMembers);
		}
		return num;
	}

	public static int GetAllPrisonerMembersAmount(MobileParty party)
	{
		int num = party.Party.NumberOfPrisoners;
		if (party.Army != null && party.Army.LeaderParty == party)
		{
			num += ((IEnumerable<MobileParty>)party.Army.LeaderParty.AttachedParties).Sum((MobileParty p) => p.Party.NumberOfPrisoners);
		}
		return num;
	}

	public static CharacterCode GetCharacterCode(CharacterObject character, bool useCivilian = false)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		if (((BasicCharacterObject)character).IsHero && IsHeroInformationHidden(character.HeroObject, out var _))
		{
			return CharacterCode.CreateEmpty();
		}
		if (((BasicCharacterObject)character).IsHero && character.HeroObject.IsLord)
		{
			string[] array = CharacterCode.CreateFrom((BasicCharacterObject)(object)character).Code.Split(new string[1] { "@---@" }, StringSplitOptions.RemoveEmptyEntries);
			Equipment val = ((useCivilian && ((BasicCharacterObject)character).FirstCivilianEquipment != null) ? ((BasicCharacterObject)character).FirstCivilianEquipment.Clone(false) : ((BasicCharacterObject)character).Equipment.Clone(false));
			val[(EquipmentIndex)5] = new EquipmentElement((ItemObject)null, (ItemModifier)null, (ItemObject)null, false);
			for (EquipmentIndex val2 = (EquipmentIndex)0; (int)val2 < 5; val2 = (EquipmentIndex)(val2 + 1))
			{
				EquipmentElement val3 = val[val2];
				ItemObject item = ((EquipmentElement)(ref val3)).Item;
				if (item != null)
				{
					WeaponComponent weaponComponent = item.WeaponComponent;
					bool? obj;
					if (weaponComponent == null)
					{
						obj = null;
					}
					else
					{
						WeaponComponentData primaryWeapon = weaponComponent.PrimaryWeapon;
						obj = ((primaryWeapon != null) ? new bool?(primaryWeapon.IsShield) : ((bool?)null));
					}
					if (obj == true)
					{
						EquipmentIndex val4 = val2;
						val3 = default(EquipmentElement);
						val.AddEquipmentToSlotWithoutAgent(val4, val3);
					}
				}
			}
			array[0] = val.CalculateEquipmentCode();
			return CharacterCode.CreateFrom(string.Join("@---@", array));
		}
		return CharacterCode.CreateFrom((BasicCharacterObject)(object)character);
	}

	public static bool IsHeroInformationHidden(Hero hero, out TextObject disableReason)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !Campaign.Current.Models.InformationRestrictionModel.DoesPlayerKnowDetailsOf(hero);
		disableReason = ((!flag) ? ((TextObject)null) : new TextObject("{=akHsjtPh}You haven't met this hero yet.", (Dictionary<string, object>)null));
		return flag;
	}

	public static MapEventVisualTypes GetMapEventVisualTypeFromMapEvent(MapEvent mapEvent)
	{
		if (mapEvent.MapEventSettlement != null)
		{
			if (mapEvent.IsSiegeAssault || mapEvent.IsSiegeOutside)
			{
				return MapEventVisualTypes.Siege;
			}
			if (mapEvent.IsSallyOut || mapEvent.IsBlockadeSallyOut)
			{
				return MapEventVisualTypes.SallyOut;
			}
			return MapEventVisualTypes.Raid;
		}
		return MapEventVisualTypes.Battle;
	}

	private static string GetChangeValueString(float value)
	{
		string text = value.ToString("0.##");
		if (value > 0.001f)
		{
			MBTextManager.SetTextVariable("NUMBER", text, false);
			return ((object)GameTexts.FindText("str_plus_with_number", (string)null)).ToString();
		}
		return text;
	}

	public static bool IsAgentInVisibilityRangeApproximate(Agent seerAgent, Agent seenAgent)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current == null || seerAgent == null || seenAgent == null)
		{
			return false;
		}
		Vec3 position = seerAgent.Position;
		float num = ((Vec3)(ref position)).Distance(seenAgent.Position);
		return 250f / (16f + num * num) * 0.95f > 0.05f;
	}

	public static bool CanAgentBeAlarmed(Agent agent)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		if (((agent != null) ? agent.Team : null) != null && agent.Team != Team.Invalid && !agent.Team.IsPlayerAlly && !agent.IsMainAgent)
		{
			return (agent.GetAgentFlags() & 0x10000) == 65536;
		}
		return false;
	}
}
