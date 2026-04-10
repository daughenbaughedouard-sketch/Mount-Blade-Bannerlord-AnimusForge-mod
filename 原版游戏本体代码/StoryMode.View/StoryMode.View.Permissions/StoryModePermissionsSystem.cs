using System;
using System.Collections.Generic;
using SandBox.View.Map.Navigation.NavigationElements;
using StoryMode.StoryModePhases;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Events;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StoryMode.View.Permissions;

public class StoryModePermissionsSystem
{
	private static StoryModePermissionsSystem Current;

	private StoryModePermissionsSystem()
	{
		RegisterEvents();
	}

	public static void OnInitialize()
	{
		if (Current == null)
		{
			Current = new StoryModePermissionsSystem();
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

	private void OnPartyScreenCharacterTalkPermission(PartyScreenCharacterTalkPermissionEvent obj)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		bool num = StoryModeManager.Current != null;
		StoryModeManager current = StoryModeManager.Current;
		bool flag = current != null && current.MainStoryLine?.TutorialPhase.IsCompleted == true;
		if (num && !flag)
		{
			obj.IsTalkAvailable(arg1: false, new TextObject("{=epQYhd1A}Cannot talk to hero right now", (Dictionary<string, object>)null));
		}
	}

	private void OnClanScreenPermission(ClanScreenPermissionEvent obj)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		StoryModeManager current = StoryModeManager.Current;
		if (current != null && current.MainStoryLine.IsPlayerInteractionRestricted)
		{
			obj.IsClanScreenAvailable(arg1: false, new TextObject("{=75nwCTEn}Clan Screen is disabled during Tutorial.", (Dictionary<string, object>)null));
		}
	}

	private void OnSettlementOverlayTalkPermission(SettlementOverlayTalkPermissionEvent obj)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		bool num = StoryModeManager.Current != null;
		TutorialPhase instance = TutorialPhase.Instance;
		bool flag = instance != null && instance.TutorialQuestPhase >= TutorialQuestPhase.RecruitAndPurchaseStarted;
		StoryModeManager current = StoryModeManager.Current;
		bool flag2 = current != null && current.MainStoryLine?.TutorialPhase.IsCompleted == true;
		if (num && !flag && !flag2)
		{
			obj.IsTalkAvailable(arg1: false, new TextObject("{=UjERCi2F}This feature is disabled.", (Dictionary<string, object>)null));
		}
	}

	private void OnSettlementOverlayQuickTalkPermission(SettlementOverylayQuickTalkPermissionEvent obj)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		bool num = StoryModeManager.Current != null;
		TutorialPhase instance = TutorialPhase.Instance;
		bool flag = instance != null && instance.TutorialQuestPhase >= TutorialQuestPhase.Finalized;
		StoryModeManager current = StoryModeManager.Current;
		bool flag2 = current != null && current.MainStoryLine?.TutorialPhase.IsCompleted == true;
		if (num && !flag && !flag2)
		{
			obj.IsTalkAvailable(arg1: false, new TextObject("{=UjERCi2F}This feature is disabled.", (Dictionary<string, object>)null));
		}
	}

	private void OnSettlementOverlayLeaveMemberPermission(SettlementOverlayLeaveCharacterPermissionEvent obj)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		bool num = StoryModeManager.Current != null;
		TutorialPhase instance = TutorialPhase.Instance;
		bool flag = instance != null && instance.TutorialQuestPhase >= TutorialQuestPhase.RecruitAndPurchaseStarted;
		StoryModeManager current = StoryModeManager.Current;
		bool flag2 = current != null && current.MainStoryLine?.TutorialPhase.IsCompleted == true;
		if (num && !flag && !flag2)
		{
			obj.IsLeaveAvailable(arg1: false, new TextObject("{=UjERCi2F}This feature is disabled.", (Dictionary<string, object>)null));
		}
	}

	private void OnLeaveKingdomPermissionEvent(LeaveKingdomPermissionEvent obj)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if (StoryModeManager.Current?.MainStoryLine.PlayerSupportedKingdom != null && Clan.PlayerClan.Kingdom == StoryModeManager.Current.MainStoryLine.PlayerSupportedKingdom)
		{
			obj.IsLeaveKingdomPossbile?.Invoke(arg1: true, new TextObject("{=WFNLizqL}You've supported a kingdom through main story line. Leaving this kingdom will fail your quest.{newline}{newline}Are you sure?", (Dictionary<string, object>)null));
		}
	}

	private void RegisterEvents()
	{
		Game.Current.EventManager.RegisterEvent<ClanScreenPermissionEvent>((Action<ClanScreenPermissionEvent>)OnClanScreenPermission);
		Game.Current.EventManager.RegisterEvent<PartyScreenCharacterTalkPermissionEvent>((Action<PartyScreenCharacterTalkPermissionEvent>)OnPartyScreenCharacterTalkPermission);
		Game.Current.EventManager.RegisterEvent<SettlementOverlayTalkPermissionEvent>((Action<SettlementOverlayTalkPermissionEvent>)OnSettlementOverlayTalkPermission);
		Game.Current.EventManager.RegisterEvent<SettlementOverylayQuickTalkPermissionEvent>((Action<SettlementOverylayQuickTalkPermissionEvent>)OnSettlementOverlayQuickTalkPermission);
		Game.Current.EventManager.RegisterEvent<SettlementOverlayLeaveCharacterPermissionEvent>((Action<SettlementOverlayLeaveCharacterPermissionEvent>)OnSettlementOverlayLeaveMemberPermission);
		Game.Current.EventManager.RegisterEvent<LeaveKingdomPermissionEvent>((Action<LeaveKingdomPermissionEvent>)OnLeaveKingdomPermissionEvent);
	}

	internal void UnregisterEvents()
	{
		Game.Current.EventManager.UnregisterEvent<ClanScreenPermissionEvent>((Action<ClanScreenPermissionEvent>)OnClanScreenPermission);
		Game.Current.EventManager.RegisterEvent<PartyScreenCharacterTalkPermissionEvent>((Action<PartyScreenCharacterTalkPermissionEvent>)OnPartyScreenCharacterTalkPermission);
		Game.Current.EventManager.UnregisterEvent<SettlementOverlayTalkPermissionEvent>((Action<SettlementOverlayTalkPermissionEvent>)OnSettlementOverlayTalkPermission);
		Game.Current.EventManager.UnregisterEvent<SettlementOverylayQuickTalkPermissionEvent>((Action<SettlementOverylayQuickTalkPermissionEvent>)OnSettlementOverlayQuickTalkPermission);
		Game.Current.EventManager.UnregisterEvent<SettlementOverlayLeaveCharacterPermissionEvent>((Action<SettlementOverlayLeaveCharacterPermissionEvent>)OnSettlementOverlayLeaveMemberPermission);
		Game.Current.EventManager.UnregisterEvent<LeaveKingdomPermissionEvent>((Action<LeaveKingdomPermissionEvent>)OnLeaveKingdomPermissionEvent);
	}
}
