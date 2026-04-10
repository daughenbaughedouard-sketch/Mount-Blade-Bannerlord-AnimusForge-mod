using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class RaftStateCampaignBehavior : CampaignBehaviorBase
{
	private Dictionary<Ship, List<ShipUpgradePiece>> _playerCachedShips = new Dictionary<Ship, List<ShipUpgradePiece>>();

	private AnchorPoint _cachedAnchorPoint;

	public override void RegisterEvents()
	{
		CampaignEvents.OnMobilePartyRaftStateChangedEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyRaftStateChanged);
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener((object)this, (Action<Hero, PartyBase, IFaction, EndCaptivityDetail, bool>)OnHeroPrisonerReleased);
		CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener((object)this, (Action<PartyBase, Hero>)OnHeroPrisonerTaken);
		CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
		CampaignEvents.OnShipDestroyedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, Ship, ShipDestroyDetail>)OnShipDestroyed);
		CampaignEvents.OnPartyLeftArmyEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Army>)OnPartyLeftArmy);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
		CampaignEvents.CanHaveCampaignIssuesEvent.AddNonSerializedListener((object)this, (ReferenceAction<Hero, bool>)CanHaveCampaignIssues);
		CampaignEvents.OnPlayerCharacterChangedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, MobileParty, bool>)OnPlayerCharacterChanged);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Dictionary<Ship, List<ShipUpgradePiece>>>("_playerCachedShips", ref _playerCachedShips);
		dataStore.SyncData<AnchorPoint>("_cachedAnchorPoint", ref _cachedAnchorPoint);
	}

	private void OnSessionLaunched(CampaignGameStarter gameStarter)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00a0: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_010d: Expected O, but got Unknown
		gameStarter.AddGameMenu("player_raft_state", "{=5ROdLNNo}You no longer have a seaworthy ship. Your party will land on the nearest shore.", new OnInitDelegate(player_raft_state_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenuOption("player_raft_state", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(continue_condition), new OnConsequenceDelegate(player_raft_state_continue_on_consequence), false, -1, false, (object)null);
		gameStarter.AddGameMenu("player_raft_state_after_prisoner", "{=BF4ybrgP}You are no longer a prisoner. Since you don't have a seaworthy ship, your party will land on the nearest shore.", new OnInitDelegate(player_raft_state_on_init), (MenuOverlayType)0, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenuOption("player_raft_state_after_prisoner", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(continue_condition), new OnConsequenceDelegate(player_raft_state_after_prisoner_on_consequence), false, -1, false, (object)null);
		gameStarter.AddWaitGameMenu("player_raft_state_wait", "{=nxA52tGB}Your party is stranded at sea.", (OnInitDelegate)null, (OnConditionDelegate)null, (OnConsequenceDelegate)null, new OnTickDelegate(player_raft_state_menu_on_tick), (MenuAndOptionType)3, (MenuOverlayType)0, 0f, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenu("player_raft_state_end", "{=iQkp5KSA}Your party has washed ashore.", (OnInitDelegate)null, (MenuOverlayType)0, (MenuFlags)0, (object)null);
		gameStarter.AddGameMenuOption("player_raft_state_end", "continue", "{=DM6luo3c}Continue", new OnConditionDelegate(continue_condition), new OnConsequenceDelegate(player_raft_state_end_continue_on_consequence), false, -1, false, (object)null);
	}

	[GameMenuInitializationHandler("player_raft_state")]
	[GameMenuInitializationHandler("player_raft_state_after_prisoner")]
	[GameMenuInitializationHandler("player_raft_state_wait")]
	public static void game_menu_player_raft_state_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("raft_state");
	}

	[GameMenuInitializationHandler("player_raft_state_end")]
	public static void game_menu_player_raft_state_end_on_init(MenuCallbackArgs args)
	{
		args.MenuContext.SetBackgroundMeshName("captive_at_sea_escape");
	}

	private bool continue_condition(MenuCallbackArgs args)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		args.optionLeaveType = (LeaveType)17;
		return true;
	}

	private void player_raft_state_end_continue_on_consequence(MenuCallbackArgs args)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		GameMenu.ExitToLast();
		if (_playerCachedShips.Count > 0)
		{
			GiveCachedShipsToMainParty();
			MobileParty.MainParty.Anchor.SetPosition(_cachedAnchorPoint.Position);
			MobileParty.MainParty.Anchor.SetLastUsedDisembarkPosition(_cachedAnchorPoint.GetLastUsedDisembarkPosition());
			_cachedAnchorPoint = null;
		}
	}

	private void player_raft_state_menu_on_tick(MenuCallbackArgs args, CampaignTime dt)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!MobileParty.MainParty.IsCurrentlyAtSea)
		{
			GameMenu.SwitchToMenu("player_raft_state_end");
			MobileParty.MainParty.SetMoveGoToPoint(MobileParty.MainParty.Position, MobileParty.MainParty.NavigationCapability);
		}
	}

	private void player_raft_state_after_prisoner_on_consequence(MenuCallbackArgs args)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)4;
		if (!MobileParty.MainParty.IsInRaftState)
		{
			if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count > 0)
			{
				_cachedAnchorPoint = new AnchorPoint(MobileParty.MainParty.Anchor);
				for (int num = ((List<Ship>)(object)MobileParty.MainParty.Ships).Count - 1; num >= 0; num--)
				{
					Ship val = ((List<Ship>)(object)MobileParty.MainParty.Ships)[num];
					_playerCachedShips.Add(val, ((IEnumerable<ShipUpgradePiece>)val.UnlockedUpgradePieces).ToList());
					val.Owner = null;
				}
			}
			HandleRaftStateActivate(MobileParty.MainParty, null);
		}
		GameMenu.SwitchToMenu("player_raft_state_wait");
	}

	private void player_raft_state_continue_on_consequence(MenuCallbackArgs args)
	{
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)4;
		if (!MobileParty.MainParty.IsInRaftState)
		{
			HandleRaftStateActivate(MobileParty.MainParty, null);
		}
		GameMenu.SwitchToMenu("player_raft_state_wait");
	}

	private void player_raft_state_on_init(MenuCallbackArgs args)
	{
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
	}

	private static void HandleRaftStateActivate(MobileParty mobileParty, MapEvent mapEvent)
	{
		if (mobileParty.HasLandNavigationCapability)
		{
			RaftStateChangeAction.ActivateRaftStateForParty(mobileParty);
			return;
		}
		if (mobileParty.IsCaravan && mobileParty.LeaderHero != null)
		{
			mobileParty.LeaderHero.ChangeState((CharacterStates)2);
		}
		DestroyPartyAction.Apply((mapEvent != null) ? mapEvent.Winner.LeaderParty : null, mobileParty);
	}

	private bool ShouldActivateRaftStateForMobileParty(MobileParty mobileParty)
	{
		if (mobileParty.IsCurrentlyAtSea && !mobileParty.IsInRaftState && !mobileParty.HasNavalNavigationCapability)
		{
			return mobileParty.IsActive;
		}
		return false;
	}

	private void ConsiderMemberAndArmyRaftStateStatus(MobileParty party, Army army)
	{
		if (ShouldActivateRaftStateForMobileParty(party))
		{
			HandleRaftStateActivate(party, party.MapEvent);
		}
		if (army != null && army.LeaderParty.IsCurrentlyAtSea && !army.LeaderParty.HasNavalNavigationCapability)
		{
			DisbandArmyAction.ApplyByNoShip(army);
		}
	}

	private void ConsiderArmyRaftState(MobileParty mobileParty)
	{
		if (!mobileParty.Army.LeaderParty.HasNavalNavigationCapability)
		{
			DisbandArmyAction.ApplyByNoShip(mobileParty.Army);
		}
		else
		{
			mobileParty.Army = null;
		}
	}

	private void OnMapEventEnded(MapEvent mapEvent)
	{
		foreach (PartyBase item in mapEvent.InvolvedParties.ToList())
		{
			if (item.IsMobile && ShouldActivateRaftStateForMobileParty(item.MobileParty))
			{
				if (item.MobileParty.Army != null)
				{
					ConsiderMemberAndArmyRaftStateStatus(item.MobileParty, item.MobileParty.Army);
				}
				else
				{
					HandleRaftStateActivate(item.MobileParty, mapEvent);
				}
			}
		}
	}

	private void OnShipDestroyed(PartyBase owner, Ship ship, ShipDestroyDetail detail)
	{
		if (owner != null && owner.MapEvent == null && owner.IsMobile && ShouldActivateRaftStateForMobileParty(owner.MobileParty))
		{
			if (owner.MobileParty.Army != null)
			{
				ConsiderMemberAndArmyRaftStateStatus(owner.MobileParty, owner.MobileParty.Army);
			}
			else
			{
				HandleRaftStateActivate(owner.MobileParty, null);
			}
		}
	}

	private void OnPartyLeftArmy(MobileParty party, Army army)
	{
		if (party.IsCurrentlyAtSea || army.LeaderParty.IsCurrentlyAtSea)
		{
			ConsiderMemberAndArmyRaftStateStatus(party, army);
		}
	}

	private void OnHeroPrisonerTaken(PartyBase party, Hero hero)
	{
		if (hero == Hero.MainHero && MobileParty.MainParty.IsInRaftState)
		{
			RaftStateChangeAction.DeactivateRaftStateForParty(MobileParty.MainParty);
		}
	}

	private void OnSettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
	{
		if (mobileParty != null && mobileParty.IsInRaftState)
		{
			Debug.FailedAssert("this should not be possible natively.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\CampaignBehaviors\\RaftStateCampaignBehavior.cs", "OnSettlementEntered", 274);
			RaftStateChangeAction.DeactivateRaftStateForParty(MobileParty.MainParty);
		}
	}

	private void OnHeroPrisonerReleased(Hero prisoner, PartyBase party, IFaction capturerFaction, EndCaptivityDetail detail, bool showNotification = true)
	{
		if (prisoner != Hero.MainHero)
		{
			MakeHeroFugitiveAction.Apply(prisoner, false);
		}
		else if (MobileParty.MainParty.IsCurrentlyAtSea)
		{
			GameMenu.ActivateGameMenu("player_raft_state_after_prisoner");
		}
	}

	public void OnPlayerCharacterChanged(Hero oldPlayer, Hero newPlayer, MobileParty newMainParty, bool isMainPartyChanged)
	{
		if (ShouldActivateRaftStateForMobileParty(newMainParty))
		{
			RaftStateChangeAction.ActivateRaftStateForParty(newMainParty);
		}
		else if (_playerCachedShips.Count > 0)
		{
			_cachedAnchorPoint = null;
			GiveCachedShipsToMainParty();
		}
		Army army = newMainParty.Army;
		if (army != null && army.LeaderParty.IsCurrentlyAtSea && !army.LeaderParty.HasNavalNavigationCapability)
		{
			DisbandArmyAction.ApplyByNoShip(army);
		}
	}

	private void OnMobilePartyRaftStateChanged(MobileParty mobileParty)
	{
		if (mobileParty.IsMainParty && mobileParty.IsActive && mobileParty.IsInRaftState)
		{
			GameMenu.ActivateGameMenu("player_raft_state");
		}
	}

	private void GiveCachedShipsToMainParty()
	{
		foreach (KeyValuePair<Ship, List<ShipUpgradePiece>> playerCachedShip in _playerCachedShips)
		{
			Ship key = playerCachedShip.Key;
			List<ShipUpgradePiece> value = playerCachedShip.Value;
			ChangeShipOwnerAction.ApplyByTransferring(PartyBase.MainParty, key);
			if (value.Count <= 0)
			{
				continue;
			}
			foreach (KeyValuePair<string, ShipSlot> availableSlot in key.ShipHull.AvailableSlots)
			{
				string key2 = availableSlot.Key;
				ShipUpgradePiece pieceAtSlot = key.GetPieceAtSlot(key2);
				foreach (ShipUpgradePiece item in value)
				{
					if (item.DoesPieceMatchSlot(availableSlot.Value))
					{
						key.EquipUpgradePiece(key2, item);
					}
				}
				if (pieceAtSlot != null)
				{
					key.EquipUpgradePiece(key2, pieceAtSlot);
				}
			}
		}
		_playerCachedShips.Clear();
	}

	private void CanHaveCampaignIssues(Hero hero, ref bool canHaveCampaignIssues)
	{
		if ((hero.PartyBelongedTo != null) & canHaveCampaignIssues)
		{
			canHaveCampaignIssues = !hero.PartyBelongedTo.IsCurrentlyAtSea;
		}
	}
}
