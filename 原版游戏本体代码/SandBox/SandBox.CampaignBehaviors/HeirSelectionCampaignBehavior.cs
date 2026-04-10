using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.CampaignBehaviors;

public class HeirSelectionCampaignBehavior : CampaignBehaviorBase
{
	private readonly ItemRoster _itemsThatWillBeInherited = new ItemRoster();

	private readonly ItemRoster _equipmentsThatWillBeInherited = new ItemRoster();

	public override void RegisterEvents()
	{
		CampaignEvents.OnBeforeMainCharacterDiedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, KillCharacterActionDetail, bool>)OnBeforeMainCharacterDied);
		CampaignEvents.OnBeforePlayerCharacterChangedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero>)OnBeforePlayerCharacterChanged);
		CampaignEvents.OnPlayerCharacterChangedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, MobileParty, bool>)OnPlayerCharacterChanged);
		CampaignEvents.OnHeirSelectionOverEvent.AddNonSerializedListener((object)this, (Action<Hero>)OnHeirSelectionOver);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void OnBeforePlayerCharacterChanged(Hero oldPlayer, Hero newPlayer)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		foreach (ItemRosterElement item in MobileParty.MainParty.ItemRoster)
		{
			_itemsThatWillBeInherited.Add(item);
		}
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement val = oldPlayer.BattleEquipment[i];
			if (!((EquipmentElement)(ref val)).IsEmpty)
			{
				_equipmentsThatWillBeInherited.AddToCounts(oldPlayer.BattleEquipment[i], 1);
			}
			val = oldPlayer.CivilianEquipment[i];
			if (!((EquipmentElement)(ref val)).IsEmpty)
			{
				_equipmentsThatWillBeInherited.AddToCounts(oldPlayer.CivilianEquipment[i], 1);
			}
		}
	}

	private void OnPlayerCharacterChanged(Hero oldPlayer, Hero newPlayer, MobileParty newMainParty, bool isMainPartyChanged)
	{
		foreach (Alley item in oldPlayer.OwnedAlleys.ToList())
		{
			item.SetOwner(newPlayer);
		}
		if (isMainPartyChanged)
		{
			newMainParty.ItemRoster.Add((IEnumerable<ItemRosterElement>)_itemsThatWillBeInherited);
		}
		newMainParty.ItemRoster.Add((IEnumerable<ItemRosterElement>)_equipmentsThatWillBeInherited);
		_itemsThatWillBeInherited.Clear();
		_equipmentsThatWillBeInherited.Clear();
	}

	private void OnBeforeMainCharacterDied(Hero victim, Hero killer, KillCharacterActionDetail detail, bool showNotification = true)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<Hero, int> heirApparents = Hero.MainHero.Clan.GetHeirApparents();
		Hero.MainHero.AddDeathMark(killer, detail);
		if (heirApparents.Count == 0)
		{
			if (PlayerEncounter.Current != null && (PlayerEncounter.Battle == null || !PlayerEncounter.Battle.IsFinalized))
			{
				PlayerEncounter.Finish(true);
			}
			Dictionary<TroopRosterElement, int> dictionary = new Dictionary<TroopRosterElement, int>();
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)MobileParty.MainParty.Party.MemberRoster.GetTroopRoster())
			{
				TroopRosterElement current = item;
				if (current.Character != CharacterObject.PlayerCharacter)
				{
					dictionary.Add(current, ((TroopRosterElement)(ref current)).Number);
				}
			}
			foreach (KeyValuePair<TroopRosterElement, int> item2 in dictionary)
			{
				MobileParty.MainParty.Party.MemberRoster.RemoveTroop(item2.Key.Character, item2.Value, default(UniqueTroopDescriptor), 0);
			}
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnGameOver();
			GameOverCleanup();
			ShowGameStatistics();
			Campaign.Current.OnGameOver();
		}
		else
		{
			if (Hero.MainHero.IsPrisoner)
			{
				EndCaptivityAction.ApplyByDeath(Hero.MainHero);
			}
			if (PlayerEncounter.Current != null && (PlayerEncounter.Battle == null || !PlayerEncounter.Battle.IsFinalized))
			{
				PlayerEncounter.Finish(true);
			}
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnHeirSelectionRequested(heirApparents);
		}
		if (Campaign.Current.CurrentMenuContext != null)
		{
			GameMenu.ExitToLast();
		}
	}

	private void OnHeirSelectionOver(Hero selectedHeir)
	{
		ApplyHeirSelectionAction.ApplyByDeath(selectedHeir);
	}

	private void ShowGameStatistics()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_003e: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		TextObject val = new TextObject("{=oxb2FVz5}Clan Destroyed", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=T2GbF6lK}With no suitable heirs, the {CLAN_NAME} clan is no more. Your journey ends here.", (Dictionary<string, object>)null);
		val2.SetTextVariable("CLAN_NAME", Clan.PlayerClan.Name);
		TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), "", (Action)delegate
		{
			GameOverState val4 = Game.Current.GameStateManager.CreateState<GameOverState>(new object[1] { (object)(GameOverReason)1 });
			Game.Current.GameStateManager.CleanAndPushState((GameState)(object)val4, 0);
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void GameOverCleanup()
	{
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, Hero.MainHero.Gold, true);
		Campaign.Current.MainParty.Party.ItemRoster.Clear();
		Campaign.Current.MainParty.Party.MemberRoster.Clear();
		Campaign.Current.MainParty.Party.PrisonRoster.Clear();
		Campaign.Current.MainParty.IsVisible = false;
		Campaign.Current.CameraFollowParty = null;
		Campaign.Current.MainParty.IsActive = false;
		PartyBase.MainParty.SetVisualAsDirty();
		if (Hero.MainHero.MapFaction.IsKingdomFaction && Clan.PlayerClan.Kingdom.Leader == Hero.MainHero)
		{
			DestroyKingdomAction.ApplyByKingdomLeaderDeath(Clan.PlayerClan.Kingdom);
		}
	}
}
