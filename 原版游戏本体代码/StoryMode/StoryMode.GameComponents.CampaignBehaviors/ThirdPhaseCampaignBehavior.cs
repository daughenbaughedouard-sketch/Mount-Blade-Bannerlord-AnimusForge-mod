using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Library;

namespace StoryMode.GameComponents.CampaignBehaviors;

public class ThirdPhaseCampaignBehavior : CampaignBehaviorBase
{
	private List<Tuple<Kingdom, Kingdom>> _warsToEnforcePeaceNextWeek = new List<Tuple<Kingdom, Kingdom>>();

	public override void RegisterEvents()
	{
		CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
		CampaignEvents.WeeklyTickEvent.AddNonSerializedListener((object)this, (Action)WeeklyTick);
		CampaignEvents.CanKingdomBeDiscontinuedEvent.AddNonSerializedListener((object)this, (ReferenceAction<Kingdom, bool>)CanKingdomBeDiscontinued);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddGameMenus(starter);
	}

	private void AddGameMenus(CampaignGameStarter starter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		starter.AddGameMenu("siege_ended_by_last_conspiracy_kingdom_defeat", "{=3pEDvftb} The conspiracy has collapsed. The defenders of their final stronghold send out a delegation under flag of truce and agree to surrender. Your men take possession of their fortress.", new OnInitDelegate(game_menu_last_conspiracy_kingdom_defeated_when_player_besiege_menu_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		starter.AddGameMenuOption("siege_ended_by_last_conspiracy_kingdom_defeat", "leave_from_besieged_last_conspiracy_settlement", "{=WVkc4UgX}Continue.", new OnConditionDelegate(siege_ended_by_last_conspiracy_kingdom_defeat_condition), new OnConsequenceDelegate(siege_ended_by_last_conspiracy_kingdom_defeat_consequence), true, -1, false, (object)null);
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail detail)
	{
		Kingdom val;
		Kingdom val2;
		if ((val = (Kingdom)(object)((faction1 is Kingdom) ? faction1 : null)) != null && (val2 = (Kingdom)(object)((faction2 is Kingdom) ? faction2 : null)) != null && StoryModeManager.Current.MainStoryLine.ThirdPhase != null)
		{
			MBReadOnlyList<Kingdom> oppositionKingdoms = StoryModeManager.Current.MainStoryLine.ThirdPhase.OppositionKingdoms;
			MBReadOnlyList<Kingdom> allyKingdoms = StoryModeManager.Current.MainStoryLine.ThirdPhase.AllyKingdoms;
			if ((((List<Kingdom>)(object)oppositionKingdoms).IndexOf(val) >= 0 && ((List<Kingdom>)(object)oppositionKingdoms).IndexOf(val2) >= 0) || (((List<Kingdom>)(object)allyKingdoms).IndexOf(val) >= 0 && ((List<Kingdom>)(object)allyKingdoms).IndexOf(val2) >= 0))
			{
				_warsToEnforcePeaceNextWeek.Add(new Tuple<Kingdom, Kingdom>(val, val2));
			}
		}
	}

	private void WeeklyTick()
	{
		foreach (Tuple<Kingdom, Kingdom> item in new List<Tuple<Kingdom, Kingdom>>(_warsToEnforcePeaceNextWeek))
		{
			MakePeaceAction.Apply((IFaction)(object)item.Item1, (IFaction)(object)item.Item2);
		}
	}

	private void CanKingdomBeDiscontinued(Kingdom kingdom, ref bool result)
	{
		if (StoryModeManager.Current.MainStoryLine.ThirdPhase != null && ((List<Kingdom>)(object)StoryModeManager.Current.MainStoryLine.ThirdPhase.OppositionKingdoms).Contains(kingdom))
		{
			result = false;
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<Tuple<Kingdom, Kingdom>>>("_warsToEnforcePeaceNextWeek", ref _warsToEnforcePeaceNextWeek);
	}

	private void siege_ended_by_last_conspiracy_kingdom_defeat_consequence(MenuCallbackArgs args)
	{
		GameMenu.ExitToLast();
	}

	private bool siege_ended_by_last_conspiracy_kingdom_defeat_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)16;
		return true;
	}

	private void game_menu_last_conspiracy_kingdom_defeated_when_player_besiege_menu_on_init(MenuCallbackArgs args)
	{
		Debug.Print("Game loaded when the player siege is left on last conspiracy kingdom is defeated by some other reasons", 0, (DebugColor)12, 17592186044416uL);
	}
}
