using System;
using System.Collections.Generic;
using NavalDLC.Storyline;
using NavalDLC.Storyline.Quests;
using SandBox.View.Map.Navigation.NavigationElements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Events;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.View.Permissions;

public class NavalPermissionsSystem
{
	private static NavalPermissionsSystem Current;

	private NavalPermissionsSystem()
	{
		RegisterEvents();
	}

	public static void OnInitialize()
	{
		if (Current == null)
		{
			Current = new NavalPermissionsSystem();
		}
	}

	internal static void OnUnload()
	{
		if (Current != null)
		{
			Current.UnregisterEvents();
			Current = null;
		}
	}

	private void OnClanScreenPermission(ClanScreenPermissionEvent obj)
	{
	}

	private void OnSettlementOverlayTalkPermission(SettlementOverlayTalkPermissionEvent obj)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (Settlement.CurrentSettlement == NavalStorylineData.HomeSettlement && obj.HeroToTalkTo == NavalStorylineData.Gunnar && Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)))
		{
			obj.IsTalkAvailable(arg1: false, new TextObject("{=bkppYuaB}Take a walk around the port and find Gunnar to talk to him.", (Dictionary<string, object>)null));
		}
	}

	private void OnSettlementOverlayQuickTalkPermission(SettlementOverylayQuickTalkPermissionEvent obj)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if (NavalStorylineData.IsNavalStorylineHero(obj.HeroToTalkTo) && (!NavalStorylineData.HasCompletedLast(NavalStorylineData.NavalStorylineStage.Act3Quest5) || Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest))))
		{
			if (Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(SpeakToGunnarAndSisterQuest)))
			{
				obj.IsTalkAvailable(arg1: false, new TextObject("{=bkppYuaB}Take a walk around the port and find Gunnar to talk to him.", (Dictionary<string, object>)null));
			}
			else
			{
				obj.IsTalkAvailable(arg1: false, new TextObject("{=UjERCi2F}This feature is disabled.", (Dictionary<string, object>)null));
			}
		}
	}

	private void OnSettlementOverlayLeaveMemberPermission(SettlementOverlayLeaveCharacterPermissionEvent obj)
	{
	}

	private void OnLeaveKingdomPermissionEvent(LeaveKingdomPermissionEvent obj)
	{
	}

	private void RegisterEvents()
	{
		Game.Current.EventManager.RegisterEvent<ClanScreenPermissionEvent>((Action<ClanScreenPermissionEvent>)OnClanScreenPermission);
		Game.Current.EventManager.RegisterEvent<SettlementOverlayTalkPermissionEvent>((Action<SettlementOverlayTalkPermissionEvent>)OnSettlementOverlayTalkPermission);
		Game.Current.EventManager.RegisterEvent<SettlementOverylayQuickTalkPermissionEvent>((Action<SettlementOverylayQuickTalkPermissionEvent>)OnSettlementOverlayQuickTalkPermission);
		Game.Current.EventManager.RegisterEvent<SettlementOverlayLeaveCharacterPermissionEvent>((Action<SettlementOverlayLeaveCharacterPermissionEvent>)OnSettlementOverlayLeaveMemberPermission);
		Game.Current.EventManager.RegisterEvent<LeaveKingdomPermissionEvent>((Action<LeaveKingdomPermissionEvent>)OnLeaveKingdomPermissionEvent);
	}

	internal void UnregisterEvents()
	{
		Game.Current.EventManager.UnregisterEvent<ClanScreenPermissionEvent>((Action<ClanScreenPermissionEvent>)OnClanScreenPermission);
		Game.Current.EventManager.UnregisterEvent<SettlementOverlayTalkPermissionEvent>((Action<SettlementOverlayTalkPermissionEvent>)OnSettlementOverlayTalkPermission);
		Game.Current.EventManager.UnregisterEvent<SettlementOverylayQuickTalkPermissionEvent>((Action<SettlementOverylayQuickTalkPermissionEvent>)OnSettlementOverlayQuickTalkPermission);
		Game.Current.EventManager.UnregisterEvent<SettlementOverlayLeaveCharacterPermissionEvent>((Action<SettlementOverlayLeaveCharacterPermissionEvent>)OnSettlementOverlayLeaveMemberPermission);
		Game.Current.EventManager.UnregisterEvent<LeaveKingdomPermissionEvent>((Action<LeaveKingdomPermissionEvent>)OnLeaveKingdomPermissionEvent);
	}
}
