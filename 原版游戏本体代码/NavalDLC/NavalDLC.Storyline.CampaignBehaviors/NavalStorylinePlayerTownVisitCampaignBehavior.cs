using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.CampaignBehaviors;

public class NavalStorylinePlayerTownVisitCampaignBehavior : CampaignBehaviorBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static OnConsequenceDelegate _003C_003E9__3_0;

		public static OnConsequenceDelegate _003C_003E9__3_2;

		public static Func<string, bool> _003C_003E9__7_0;

		public static Func<Ship, bool> _003C_003E9__12_0;

		public static Func<Ship, bool> _003C_003E9__14_0;

		internal void _003CAddGameMenus_003Eb__3_0(MenuCallbackArgs x)
		{
			GameMenu.SwitchToMenu("naval_storyline_exit");
		}

		internal void _003CAddGameMenus_003Eb__3_2(MenuCallbackArgs x)
		{
			GameMenu.SwitchToMenu("naval_storyline_virtualport");
		}

		internal bool _003Cvisit_port_menu_on_condition_003Eb__7_0(string x)
		{
			return x == "port";
		}

		internal bool _003Cvirtual_port_menu_gather_reinforcements_on_condition_003Eb__12_0(Ship s)
		{
			return s.IsUsedByQuest;
		}

		internal bool _003CReinforceMainParty_003Eb__14_0(Ship s)
		{
			return s.IsUsedByQuest;
		}
	}

	public override void RegisterEvents()
	{
		if (!NavalStorylineData.IsNavalStorylineCanceled())
		{
			CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnAfterSessionLaunched(CampaignGameStarter campaignGameSystemStarter)
	{
		AddGameMenus(campaignGameSystemStarter);
	}

	private void AddGameMenus(CampaignGameStarter campaignGameSystemStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0081: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00b2: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00e3: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_0158: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_019d: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected O, but got Unknown
		//IL_01e1: Expected O, but got Unknown
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		campaignGameSystemStarter.AddGameMenu("naval_storyline_virtualport", "{=!}{VIRTUAL_PORT_TEXT}", new OnInitDelegate(virtual_port_menu_on_init), (MenuOverlayType)2, (MenuFlags)0, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_virtualport", "repair_ships", "{=hqGD0o4E}Repair ships ({TOTAL_AMOUNT}{GOLD_ICON})", new OnConditionDelegate(virtual_port_menu_repair_ships_on_condition), new OnConsequenceDelegate(virtual_port_menu_repair_ships_on_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_virtualport", "gather_reinforcements", "{=2NRLzk5K}Gather Reinforcements", new OnConditionDelegate(virtual_port_menu_gather_reinforcements_on_condition), new OnConsequenceDelegate(virtual_port_menu_gather_reinforcements_on_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_virtualport", "trade", "{=GmcgoiGy}Trade", new OnConditionDelegate(virtual_port_menu_trade_on_condition), new OnConsequenceDelegate(virtual_port_menu_trade_on_consequence), false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_virtualport", "visit_port", "{=sq7Qoh4Z}Visit the port", new OnConditionDelegate(visit_port_menu_on_condition), new OnConsequenceDelegate(visit_port_menu_on_consequence), false, -1, false, (object)null);
		OnConditionDelegate val = virtual_port_menu_naval_storyline_exit_on_condition;
		object obj = _003C_003Ec._003C_003E9__3_0;
		if (obj == null)
		{
			OnConsequenceDelegate val2 = delegate
			{
				GameMenu.SwitchToMenu("naval_storyline_exit");
			};
			_003C_003Ec._003C_003E9__3_0 = val2;
			obj = (object)val2;
		}
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_virtualport", "naval_storyline_exit", "{=0hA4wOqV}Exit Story Mode", val, (OnConsequenceDelegate)obj, false, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_virtualport", "port_leave", "{=fbCbFqyj}Set sail", new OnConditionDelegate(virtual_port_menu_leave_on_condition), new OnConsequenceDelegate(virtual_port_menu_leave_on_consequence), true, -1, false, (object)null);
		campaignGameSystemStarter.AddGameMenu("naval_storyline_exit", "{=dV92VE8i}When you leave story mode, you will be returned to Ostican. You can speak to Gunnar in port to try again later. Do you wish to continue?", (OnInitDelegate)null, (MenuOverlayType)2, (MenuFlags)0, (object)null);
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_exit", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(naval_storyline_exit_continue_on_condition), (OnConsequenceDelegate)delegate
		{
			ExitStoryMode();
		}, false, -1, false, (object)null);
		OnConditionDelegate val3 = naval_storyline_exit_cancel_on_condition;
		object obj2 = _003C_003Ec._003C_003E9__3_2;
		if (obj2 == null)
		{
			OnConsequenceDelegate val4 = delegate
			{
				GameMenu.SwitchToMenu("naval_storyline_virtualport");
			};
			_003C_003Ec._003C_003E9__3_2 = val4;
			obj2 = (object)val4;
		}
		campaignGameSystemStarter.AddGameMenuOption("naval_storyline_exit", "cancel", "{=3CpNUnVl}Cancel", val3, (OnConsequenceDelegate)obj2, true, -1, false, (object)null);
	}

	public void virtual_port_menu_on_init(MenuCallbackArgs args)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		MBTextManager.SetTextVariable("VIRTUAL_PORT_TEXT", new TextObject("{=2p7Z6OAb}You are at the port", (Dictionary<string, object>)null), false);
		if (!MenuHelper.CheckAndOpenNextLocation(args))
		{
			string backgroundMeshName = ((MBObjectBase)Settlement.CurrentSettlement.Culture).StringId + "_port";
			args.MenuContext.SetBackgroundMeshName(backgroundMeshName);
			args.MenuContext.SetAmbientSound("event:/map/ambient/node/settlements/2d/port");
			UpdateMenuLocations();
			if (IsPortInteractionDisabled())
			{
				MBTextManager.SetTextVariable("VIRTUAL_PORT_TEXT", new TextObject("{=fs3uB3y4}Gunnar says to return after the siege is over.", (Dictionary<string, object>)null), false);
			}
		}
	}

	private void UpdateMenuLocations()
	{
		Campaign.Current.GameMenuManager.MenuLocations.Clear();
		Campaign.Current.GameMenuManager.MenuLocations.Add(Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("port"));
	}

	private static bool IsPortInteractionDisabled()
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement == null)
		{
			return true;
		}
		return currentSettlement.IsUnderSiege;
	}

	private bool visit_port_menu_on_condition(MenuCallbackArgs args)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (IsPortInteractionDisabled())
		{
			return false;
		}
		List<Location> list = Settlement.CurrentSettlement.LocationComplex.FindAll((Func<string, bool>)((string x) => x == "port")).ToList();
		MenuHelper.SetIssueAndQuestDataForLocations(args, list);
		args.optionLeaveType = (LeaveType)1;
		return true;
	}

	private void visit_port_menu_on_consequence(MenuCallbackArgs args)
	{
		_ = PlayerEncounter.LocationEncounter;
		Campaign.Current.GameMenuManager.NextLocation = LocationComplex.Current.GetLocationWithId("port");
		Campaign.Current.GameMenuManager.PreviousLocation = LocationComplex.Current.GetLocationWithId("center");
		PlayerEncounter.LocationEncounter.CreateAndOpenMissionController(Campaign.Current.GameMenuManager.NextLocation, (Location)null, (CharacterObject)null, (string)null);
		Campaign.Current.GameMenuManager.NextLocation = null;
		Campaign.Current.GameMenuManager.PreviousLocation = null;
	}

	private bool virtual_port_menu_repair_ships_on_condition(MenuCallbackArgs args)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		if (IsPortInteractionDisabled())
		{
			return false;
		}
		float goldCostToRepairShips = GetGoldCostToRepairShips();
		if (goldCostToRepairShips > 0f)
		{
			if (goldCostToRepairShips > (float)Hero.MainHero.Gold)
			{
				args.IsEnabled = false;
				args.Tooltip = new TextObject("{=d0kbtGYn}You don't have enough gold.", (Dictionary<string, object>)null);
			}
			MBTextManager.SetTextVariable("TOTAL_AMOUNT", (int)goldCostToRepairShips);
			return true;
		}
		return false;
	}

	private float GetGoldCostToRepairShips()
	{
		float num = 0f;
		foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints)
			{
				num += Campaign.Current.Models.ShipCostModel.GetShipRepairCost(item, PartyBase.MainParty);
			}
		}
		return num;
	}

	private void virtual_port_menu_repair_ships_on_consequence(MenuCallbackArgs args)
	{
		foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
		{
			if (item.HitPoints < item.MaxHitPoints)
			{
				RepairShipAction.Apply(item, Settlement.CurrentSettlement);
			}
		}
		args.MenuContext.Refresh();
	}

	private bool virtual_port_menu_gather_reinforcements_on_condition(MenuCallbackArgs args)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		if (IsPortInteractionDisabled())
		{
			return false;
		}
		args.optionLeaveType = (LeaveType)37;
		if (MobileParty.MainParty.IsNavalStorylineQuestParty(out var partyData) && partyData != null && partyData.IsQuestParty)
		{
			int num = ((IEnumerable<Ship>)PartyBase.MainParty.Ships).Where((Ship s) => s.IsUsedByQuest).Count();
			int num2 = 0;
			foreach (ShipTemplateStack item in (List<ShipTemplateStack>)(object)partyData.Template.ShipHulls)
			{
				num2 += item.MaxValue;
			}
			if (MobileParty.MainParty.MemberRoster.TotalManCount >= MobileParty.MainParty.Party.PartySizeLimit && num2 <= num)
			{
				args.IsEnabled = false;
				args.Tooltip = new TextObject("{=Tbg46Xm3}Party does not need any more reinforcement.", (Dictionary<string, object>)null);
			}
			return true;
		}
		return false;
	}

	private void virtual_port_menu_gather_reinforcements_on_consequence(MenuCallbackArgs args)
	{
		if (MobileParty.MainParty.Party.IsNavalStorylineQuestParty(out var partyData) && partyData != null && partyData.IsQuestParty)
		{
			ReinforceMainParty(partyData);
			args.MenuContext.Refresh();
		}
	}

	private void ReinforceMainParty(NavalStorylinePartyData data)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Expected O, but got Unknown
		int totalManCount = MobileParty.MainParty.MemberRoster.TotalManCount;
		int num = data.PartySize - totalManCount;
		ExplainedNumber maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(data.Template);
		int num2 = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber;
		float num3 = (float)num / (float)num2;
		foreach (PartyTemplateStack item in (List<PartyTemplateStack>)(object)data.Template.Stacks)
		{
			CharacterObject character = item.Character;
			int num4 = MathF.Floor((float)item.MaxValue * num3);
			num -= num4;
			MobileParty.MainParty.MemberRoster.AddToCounts(character, num4, false, 0, 0, true, -1);
		}
		for (int i = 0; i < num; i++)
		{
			int index = MBRandom.RandomInt(((List<PartyTemplateStack>)(object)data.Template.Stacks).Count);
			CharacterObject character2 = ((List<PartyTemplateStack>)(object)data.Template.Stacks)[index].Character;
			MobileParty.MainParty.MemberRoster.AddToCounts(character2, 1, false, 0, 0, true, -1);
		}
		List<Ship> source = ((IEnumerable<Ship>)PartyBase.MainParty.Ships).Where((Ship s) => s.IsUsedByQuest).ToList();
		foreach (ShipTemplateStack stack in (List<ShipTemplateStack>)(object)data.Template.ShipHulls)
		{
			int num5 = source.Where((Ship s) => ((MBObjectBase)s.ShipHull).StringId == ((MBObjectBase)stack.ShipHull).StringId).Count();
			if (num5 < stack.MaxValue)
			{
				for (int num6 = 0; num6 < stack.MaxValue - num5; num6++)
				{
					Ship val = new Ship(stack.ShipHull)
					{
						IsTradeable = false,
						IsUsedByQuest = true
					};
					ChangeShipOwnerAction.ApplyByMobilePartyCreation(PartyBase.MainParty, val);
				}
			}
		}
	}

	private bool virtual_port_menu_trade_on_condition(MenuCallbackArgs args)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (IsPortInteractionDisabled())
		{
			return false;
		}
		bool flag2 = default(bool);
		TextObject val = default(TextObject);
		bool flag = Campaign.Current.Models.SettlementAccessModel.CanMainHeroDoSettlementAction(Settlement.CurrentSettlement, (SettlementAction)5, ref flag2, ref val);
		args.optionLeaveType = (LeaveType)14;
		return MenuHelper.SetOptionProperties(args, flag, flag2, val);
	}

	private void virtual_port_menu_trade_on_consequence(MenuCallbackArgs args)
	{
		_ = PlayerEncounter.LocationEncounter;
		InventoryScreenHelper.OpenScreenAsTrade(Settlement.CurrentSettlement.ItemRoster, (SettlementComponent)(object)Settlement.CurrentSettlement.Town, (InventoryCategoryType)(-1), (Action)null);
	}

	private bool naval_storyline_exit_continue_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private bool naval_storyline_exit_cancel_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private bool virtual_port_menu_naval_storyline_exit_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)21;
		return true;
	}

	private void ExitStoryMode()
	{
		NavalStorylineData.DeactivateNavalStoryline();
	}

	private bool virtual_port_menu_leave_on_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)43;
		return true;
	}

	private void virtual_port_menu_leave_on_consequence(MenuCallbackArgs args)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		MobileParty.MainParty.SetSailAtPosition(Settlement.CurrentSettlement.PortPosition);
		PlayerEncounter.Finish(true);
	}
}
