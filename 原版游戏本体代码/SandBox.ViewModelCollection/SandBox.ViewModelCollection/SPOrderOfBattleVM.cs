using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.OrderOfBattle;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection;

public class SPOrderOfBattleVM : OrderOfBattleVM
{
	private OrderOfBattleCampaignBehavior _orderOfBattleBehavior;

	private static readonly TextObject _perkDefinitionText = new TextObject("{=jCdZY3i4}{PERK_NAME} ({SKILL_LEVEL} - {SKILL})", (Dictionary<string, object>)null);

	private readonly TextObject _captainPerksText = new TextObject("{=pgXuyHxH}Captain Perks", (Dictionary<string, object>)null);

	private readonly TextObject _infantryInfluenceText = new TextObject("{=SSLUHH6j}Infantry Influence", (Dictionary<string, object>)null);

	private readonly TextObject _rangedInfluenceText = new TextObject("{=0DMM0agr}Ranged Influence", (Dictionary<string, object>)null);

	private readonly TextObject _cavalryInfluenceText = new TextObject("{=X8i3jZn8}Cavalry Influence", (Dictionary<string, object>)null);

	private readonly TextObject _horseArcherInfluenceText = new TextObject("{=gZIOG0wl}Horse Archer Influence", (Dictionary<string, object>)null);

	private readonly TextObject _noPerksText = new TextObject("{=7yaDnyKb}There is no additional perk influence.", (Dictionary<string, object>)null);

	private readonly PerkObjectComparer _perkComparer = new PerkObjectComparer();

	public SPOrderOfBattleVM()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
	}

	protected override void LoadConfiguration()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Invalid comparison between Unknown and I4
		//IL_057b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Invalid comparison between Unknown and I4
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Invalid comparison between Unknown and I4
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Invalid comparison between Unknown and I4
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Invalid comparison between Unknown and I4
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Invalid comparison between Unknown and I4
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Invalid comparison between Unknown and I4
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Invalid comparison between Unknown and I4
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Invalid comparison between Unknown and I4
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Invalid comparison between Unknown and I4
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Invalid comparison between Unknown and I4
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		((OrderOfBattleVM)this).LoadConfiguration();
		_orderOfBattleBehavior = Campaign.Current.GetCampaignBehavior<OrderOfBattleCampaignBehavior>();
		if (_orderOfBattleBehavior == null || !((OrderOfBattleVM)this).IsPlayerGeneral)
		{
			return;
		}
		for (int i = 0; i < ((OrderOfBattleVM)this).TotalFormationCount; i++)
		{
			OrderOfBattleFormationData formationInfo = _orderOfBattleBehavior.GetFormationDataAtIndex(i, Mission.Current.IsSiegeBattle, MobileParty.MainParty.Army != null);
			if (formationInfo != null && (int)formationInfo.FormationClass != 0)
			{
				bool flag = formationInfo.PrimaryClassWeight > 0 || formationInfo.SecondaryClassWeight > 0;
				if ((int)formationInfo.FormationClass == 1)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[0].Class = (FormationClass)0;
				}
				else if ((int)formationInfo.FormationClass == 2)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[0].Class = (FormationClass)1;
				}
				else if ((int)formationInfo.FormationClass == 3)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[0].Class = (FormationClass)2;
				}
				else if ((int)formationInfo.FormationClass == 4)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[0].Class = (FormationClass)3;
				}
				else if ((int)formationInfo.FormationClass == 5)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[0].Class = (FormationClass)0;
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[1].Class = (FormationClass)1;
				}
				else if ((int)formationInfo.FormationClass == 6)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[0].Class = (FormationClass)2;
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[i].Classes)[1].Class = (FormationClass)3;
				}
				if (flag)
				{
					formationInfo.Filters.TryGetValue((FormationFilterType)1, out var value);
					formationInfo.Filters.TryGetValue((FormationFilterType)2, out var value2);
					formationInfo.Filters.TryGetValue((FormationFilterType)3, out var value3);
					formationInfo.Filters.TryGetValue((FormationFilterType)4, out var value4);
					formationInfo.Filters.TryGetValue((FormationFilterType)5, out var value5);
					formationInfo.Filters.TryGetValue((FormationFilterType)6, out var value6);
					((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)base._allFormations[i].FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 1)).IsActive = value;
					((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)base._allFormations[i].FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 2)).IsActive = value2;
					((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)base._allFormations[i].FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 3)).IsActive = value3;
					((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)base._allFormations[i].FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 4)).IsActive = value4;
					((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)base._allFormations[i].FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 5)).IsActive = value5;
					((IEnumerable<OrderOfBattleFormationFilterSelectorItemVM>)base._allFormations[i].FilterItems).FirstOrDefault((Func<OrderOfBattleFormationFilterSelectorItemVM, bool>)((OrderOfBattleFormationFilterSelectorItemVM f) => (int)f.FilterType == 6)).IsActive = value6;
				}
				else
				{
					((OrderOfBattleVM)this).ClearFormationItem(base._allFormations[i]);
				}
				DeploymentFormationClass val = formationInfo.FormationClass;
				if (Mission.Current.IsSiegeBattle)
				{
					if ((int)val == 4)
					{
						val = (DeploymentFormationClass)2;
					}
					else if ((int)val == 3)
					{
						val = (DeploymentFormationClass)1;
					}
					else if ((int)val == 6)
					{
						val = (DeploymentFormationClass)5;
					}
				}
				base._allFormations[i].RefreshFormation(base._allFormations[i].Formation, val, flag);
				if (flag && formationInfo.Captain != null)
				{
					OrderOfBattleHeroItemVM val2 = ((IEnumerable<OrderOfBattleHeroItemVM>)base._allHeroes).FirstOrDefault((Func<OrderOfBattleHeroItemVM, bool>)((OrderOfBattleHeroItemVM c) => (object)c.Agent.Character == formationInfo.Captain.CharacterObject));
					if (val2 != null)
					{
						((OrderOfBattleVM)this).AssignCaptain(val2.Agent, base._allFormations[i]);
					}
				}
				if (!flag || formationInfo.HeroTroops == null)
				{
					continue;
				}
				Hero[] heroTroops = formationInfo.HeroTroops;
				foreach (Hero heroTroop in heroTroops)
				{
					OrderOfBattleHeroItemVM val3 = ((IEnumerable<OrderOfBattleHeroItemVM>)base._allHeroes).FirstOrDefault((Func<OrderOfBattleHeroItemVM, bool>)((OrderOfBattleHeroItemVM ht) => (object)ht.Agent.Character == heroTroop.CharacterObject));
					if (val3 != null)
					{
						base._allFormations[i].AddHeroTroop(val3);
					}
				}
			}
			else if (formationInfo != null)
			{
				((OrderOfBattleVM)this).ClearFormationItem(base._allFormations[i]);
			}
		}
		for (int num2 = 0; num2 < ((OrderOfBattleVM)this).TotalFormationCount; num2++)
		{
			OrderOfBattleFormationData formationDataAtIndex = _orderOfBattleBehavior.GetFormationDataAtIndex(num2, Mission.Current.IsSiegeBattle, MobileParty.MainParty.Army != null);
			if (formationDataAtIndex != null && (int)formationDataAtIndex.FormationClass != 0)
			{
				if ((int)((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[num2].Classes)[0].Class != 10)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[num2].Classes)[0].Weight = formationDataAtIndex.PrimaryClassWeight;
				}
				if ((int)((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[num2].Classes)[1].Class != 10)
				{
					((Collection<OrderOfBattleFormationClassVM>)(object)base._allFormations[num2].Classes)[1].Weight = formationDataAtIndex.SecondaryClassWeight;
				}
			}
		}
	}

	protected override void SaveConfiguration()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Invalid comparison between Unknown and I4
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		((OrderOfBattleVM)this).SaveConfiguration();
		bool flag = MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle();
		if (_orderOfBattleBehavior != null && !(((OrderOfBattleVM)this).IsPlayerGeneral && flag))
		{
			return;
		}
		List<OrderOfBattleFormationData> list = new List<OrderOfBattleFormationData>();
		for (int i = 0; i < ((OrderOfBattleVM)this).TotalFormationCount; i++)
		{
			OrderOfBattleFormationItemVM formationItemVM = base._allFormations[i];
			Hero val = null;
			if (formationItemVM.Captain.Agent != null)
			{
				val = Hero.FindFirst((Func<Hero, bool>)((Hero h) => (object)h.CharacterObject == formationItemVM.Captain.Agent.Character));
			}
			Hero[] array = (from ht in (IEnumerable<OrderOfBattleHeroItemVM>)formationItemVM.HeroTroops
				select Hero.FindFirst((Func<Hero, bool>)((Hero hero) => (object)hero.CharacterObject == ht.Agent.Character)) into h
				where h != null
				select h).ToArray();
			DeploymentFormationClass orderOfBattleClass = formationItemVM.GetOrderOfBattleClass();
			bool flag2 = (int)orderOfBattleClass == 0;
			int num = ((!flag2) ? ((Collection<OrderOfBattleFormationClassVM>)(object)formationItemVM.Classes)[0].Weight : 0);
			int num2 = ((!flag2) ? ((Collection<OrderOfBattleFormationClassVM>)(object)formationItemVM.Classes)[1].Weight : 0);
			Dictionary<FormationFilterType, bool> dictionary = new Dictionary<FormationFilterType, bool>
			{
				[(FormationFilterType)1] = !flag2 && formationItemVM.HasFilter((FormationFilterType)1),
				[(FormationFilterType)2] = !flag2 && formationItemVM.HasFilter((FormationFilterType)2),
				[(FormationFilterType)3] = !flag2 && formationItemVM.HasFilter((FormationFilterType)3),
				[(FormationFilterType)4] = !flag2 && formationItemVM.HasFilter((FormationFilterType)4),
				[(FormationFilterType)5] = !flag2 && formationItemVM.HasFilter((FormationFilterType)5),
				[(FormationFilterType)6] = !flag2 && formationItemVM.HasFilter((FormationFilterType)6)
			};
			list.Add(new OrderOfBattleFormationData(val, array, orderOfBattleClass, num, num2, dictionary));
		}
		_orderOfBattleBehavior.SetFormationInfos(list, Mission.Current.IsSiegeBattle, MobileParty.MainParty.Army != null);
	}

	protected override List<TooltipProperty> GetAgentTooltip(Agent agent)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected O, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Expected O, but got Unknown
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Expected O, but got Unknown
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Expected O, but got Unknown
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Expected O, but got Unknown
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Expected O, but got Unknown
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Expected O, but got Unknown
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Expected O, but got Unknown
		List<TooltipProperty> agentTooltip = ((OrderOfBattleVM)this).GetAgentTooltip(agent);
		if (agent == null)
		{
			return agentTooltip;
		}
		Hero val = Hero.FindFirst((Func<Hero, bool>)((Hero h) => ((MBObjectBase)h).StringId == ((MBObjectBase)agent.Character).StringId));
		if (val == null)
		{
			return agentTooltip;
		}
		agentTooltip.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0));
		foreach (SkillObject item in (List<SkillObject>)(object)Skills.All)
		{
			if (((MBObjectBase)item).StringId == "OneHanded" || ((MBObjectBase)item).StringId == "TwoHanded" || ((MBObjectBase)item).StringId == "Polearm" || ((MBObjectBase)item).StringId == "Bow" || ((MBObjectBase)item).StringId == "Crossbow" || ((MBObjectBase)item).StringId == "Throwing" || ((MBObjectBase)item).StringId == "Riding" || ((MBObjectBase)item).StringId == "Athletics" || ((MBObjectBase)item).StringId == "Tactics" || ((MBObjectBase)item).StringId == "Leadership")
			{
				agentTooltip.Add(new TooltipProperty(((object)((PropertyObject)item).Name).ToString(), agent.Character.GetSkillValue(item).ToString(), 0, false, (TooltipPropertyFlags)0)
				{
					OnlyShowWhenNotExtended = true
				});
			}
		}
		agentTooltip.Add(new TooltipProperty("", string.Empty, 0, false, (TooltipPropertyFlags)1024)
		{
			OnlyShowWhenNotExtended = true
		});
		List<PerkObject> first = default(List<PerkObject>);
		float captainRatingForTroopUsages = Campaign.Current.Models.BattleCaptainModel.GetCaptainRatingForTroopUsages(val, FormationClassExtensions.GetTroopUsageFlags((FormationClass)0), ref first);
		List<PerkObject> second = default(List<PerkObject>);
		float captainRatingForTroopUsages2 = Campaign.Current.Models.BattleCaptainModel.GetCaptainRatingForTroopUsages(val, FormationClassExtensions.GetTroopUsageFlags((FormationClass)1), ref second);
		List<PerkObject> second2 = default(List<PerkObject>);
		float captainRatingForTroopUsages3 = Campaign.Current.Models.BattleCaptainModel.GetCaptainRatingForTroopUsages(val, FormationClassExtensions.GetTroopUsageFlags((FormationClass)2), ref second2);
		List<PerkObject> second3 = default(List<PerkObject>);
		float captainRatingForTroopUsages4 = Campaign.Current.Models.BattleCaptainModel.GetCaptainRatingForTroopUsages(val, FormationClassExtensions.GetTroopUsageFlags((FormationClass)3), ref second3);
		agentTooltip.Add(new TooltipProperty(((object)_infantryInfluenceText).ToString(), ((int)(captainRatingForTroopUsages * 100f)).ToString(), 0, false, (TooltipPropertyFlags)0)
		{
			OnlyShowWhenNotExtended = true
		});
		agentTooltip.Add(new TooltipProperty(((object)_rangedInfluenceText).ToString(), ((int)(captainRatingForTroopUsages2 * 100f)).ToString(), 0, false, (TooltipPropertyFlags)0)
		{
			OnlyShowWhenNotExtended = true
		});
		agentTooltip.Add(new TooltipProperty(((object)_cavalryInfluenceText).ToString(), ((int)(captainRatingForTroopUsages3 * 100f)).ToString(), 0, false, (TooltipPropertyFlags)0)
		{
			OnlyShowWhenNotExtended = true
		});
		agentTooltip.Add(new TooltipProperty(((object)_horseArcherInfluenceText).ToString(), ((int)(captainRatingForTroopUsages4 * 100f)).ToString(), 0, false, (TooltipPropertyFlags)0)
		{
			OnlyShowWhenNotExtended = true
		});
		agentTooltip.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0)
		{
			OnlyShowWhenNotExtended = true
		});
		List<PerkObject> list = first.Union(second).Union(second2).Union(second3)
			.ToList();
		list.Sort(_perkComparer);
		bool num = list.Count != 0;
		if (num)
		{
			AddPerks(_captainPerksText, agentTooltip, list);
		}
		if (!num)
		{
			agentTooltip.Add(new TooltipProperty(((object)_noPerksText).ToString(), string.Empty, 0, true, (TooltipPropertyFlags)0));
		}
		if (Input.IsGamepadActive)
		{
			GameTexts.SetVariable("EXTEND_KEY", ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", "MapFollowModifier")).ToString());
		}
		else
		{
			GameTexts.SetVariable("EXTEND_KEY", ((object)Game.Current.GameTextManager.FindText("str_game_key_text", "anyalt")).ToString());
		}
		agentTooltip.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_map_tooltip_info", (string)null)).ToString(), -1, false, (TooltipPropertyFlags)0)
		{
			OnlyShowWhenNotExtended = true
		});
		return agentTooltip;
	}

	private static void AddPerks(TextObject title, List<TooltipProperty> tooltipProperties, List<PerkObject> perks)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		tooltipProperties.Add(new TooltipProperty(((object)title).ToString(), string.Empty, 0, true, (TooltipPropertyFlags)4096));
		foreach (PerkObject perk in perks)
		{
			if ((int)perk.PrimaryRole == 13 || (int)perk.SecondaryRole == 13)
			{
				TextObject val = (((int)perk.PrimaryRole == 13) ? perk.PrimaryDescription : perk.SecondaryDescription);
				string genericImageText = HyperlinkTexts.GetGenericImageText(CampaignUIHelper.GetSkillMeshId(perk.Skill, true), 2);
				_perkDefinitionText.SetTextVariable("PERK_NAME", ((PropertyObject)perk).Name).SetTextVariable("SKILL", genericImageText).SetTextVariable("SKILL_LEVEL", perk.RequiredSkillValue, 2);
				tooltipProperties.Add(new TooltipProperty(((object)_perkDefinitionText).ToString(), ((object)val).ToString(), 0, true, (TooltipPropertyFlags)0));
			}
		}
	}
}
