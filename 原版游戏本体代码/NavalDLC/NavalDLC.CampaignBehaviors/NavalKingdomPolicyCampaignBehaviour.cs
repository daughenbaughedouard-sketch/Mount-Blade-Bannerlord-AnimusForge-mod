using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class NavalKingdomPolicyCampaignBehaviour : CampaignBehaviorBase
{
	private const float KingsPardonForPiratesSearchRadius = 50f;

	private const float KingsPardonForPiratesArriveDistance = 5f;

	private const float KingsPardonDailyCheckChance = 0.05f;

	private const float KingsPardonRecruitPercentage = 0.25f;

	private Dictionary<MobileParty, Settlement> _settlementToSurrenderByParty = new Dictionary<MobileParty, Settlement>();

	public override void RegisterEvents()
	{
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
		CampaignEvents.RaidCompletedEvent.AddNonSerializedListener((object)this, (Action<BattleSideEnum, RaidEventComponent>)OnRaidCompleted);
		CampaignEvents.DailyTickEvent.AddNonSerializedListener((object)this, (Action)OnDailyTick);
		CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnHourlyTickParty);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.OnBarterAcceptedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero, List<Barterable>>)OnBarterAccepted);
	}

	private void OnBarterAccepted(Hero offererHero, Hero otherHero, List<Barterable> barters)
	{
		Clan clan = offererHero.Clan;
		Kingdom val = ((clan != null) ? clan.Kingdom : null);
		if (val == null || val.RulingClan == clan || !val.HasPolicy(NavalPolicies.RoyalRansomClaim))
		{
			return;
		}
		IEnumerable<SetPrisonerFreeBarterable> enumerable = barters.Where((Barterable x) => x is SetPrisonerFreeBarterable).Cast<SetPrisonerFreeBarterable>();
		float num = 0f;
		foreach (SetPrisonerFreeBarterable item in enumerable)
		{
			num = ((Barterable)item).GetUnitValueForFaction(otherHero.MapFaction);
		}
		int num2 = MathF.Round(num * 0.15f);
		GiveGoldAction.ApplyBetweenCharacters(offererHero, val.Leader, num2, false);
	}

	private void OnDailyTick()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Kingdom item in (List<Kingdom>)(object)Kingdom.All)
		{
			if (!item.HasPolicy(NavalPolicies.KingsPardonForPirates))
			{
				continue;
			}
			foreach (Settlement item2 in (List<Settlement>)(object)item.Settlements)
			{
				if (MBRandom.RandomFloat <= 0.05f && item2.IsTown && item2.HasPort)
				{
					MobileParty availableNearbyPirateParty = GetAvailableNearbyPirateParty(item2);
					if (availableNearbyPirateParty != null)
					{
						availableNearbyPirateParty.SetMoveGoToPoint(item2.PortPosition, (NavigationType)2);
						availableNearbyPirateParty.Ai.SetDoNotMakeNewDecisions(true);
						_settlementToSurrenderByParty.Add(availableNearbyPirateParty, item2);
					}
				}
			}
		}
	}

	private void OnHourlyTickParty(MobileParty party)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		if (!_settlementToSurrenderByParty.TryGetValue(party, out var value) || !(Campaign.Current.Models.MapDistanceModel.GetDistance(party, value, true, (NavigationType)2, ref num) <= 5f))
		{
			return;
		}
		MobileParty garrisonParty = ((Fief)value.Town).GarrisonParty;
		if (garrisonParty != null)
		{
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)party.MemberRoster.GetTroopRoster())
			{
				TroopRosterElement current = item;
				int num2 = Math.Min(garrisonParty.GetAvailableWageBudget() / Campaign.Current.Models.PartyWageModel.GetCharacterWage(current.Character), MathF.Round((float)((TroopRosterElement)(ref current)).Number * 0.25f));
				if (num2 > 0)
				{
					garrisonParty.MemberRoster.AddToCounts(current.Character, num2, false, 0, 0, true, -1);
				}
			}
		}
		Town town = value.Town;
		town.Security -= 5f;
		for (int num3 = ((List<Ship>)(object)party.Ships).Count - 1; num3 >= 0; num3--)
		{
			ChangeShipOwnerAction.ApplyByTransferring(value.Party, ((List<Ship>)(object)party.Ships)[num3]);
		}
		if (party.MapEvent != null)
		{
			party.MapEventSide = null;
		}
		DestroyPartyAction.Apply((PartyBase)null, party);
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase destroyerParty)
	{
		_settlementToSurrenderByParty.Remove(mobileParty);
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		KeyValuePair<MobileParty, Settlement> keyValuePair = _settlementToSurrenderByParty.FirstOrDefault((KeyValuePair<MobileParty, Settlement> x) => x.Value == settlement);
		Clan clan = newOwner.Clan;
		Kingdom val = ((clan != null) ? clan.Kingdom : null);
		MobileParty key = keyValuePair.Key;
		if (key != null && (val == null || !val.HasPolicy(NavalPolicies.KingsPardonForPirates)))
		{
			if (key.MapEvent != null)
			{
				key.MapEventSide = null;
			}
			DestroyPartyAction.Apply((PartyBase)null, key);
		}
	}

	private void OnRaidCompleted(BattleSideEnum winnerSide, RaidEventComponent raidEvent)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)winnerSide != 1)
		{
			return;
		}
		Hero owner = raidEvent.AttackerSide.LeaderParty.Owner;
		Clan obj = ((owner != null) ? owner.Clan : null);
		Kingdom val = ((obj != null) ? obj.Kingdom : null);
		if (val == null || !val.HasPolicy(NavalPolicies.RaidersSpoils))
		{
			return;
		}
		foreach (MapEventParty item in (List<MapEventParty>)(object)raidEvent.AttackerSide.Parties)
		{
			GainKingdomInfluenceAction.ApplyForDefault(item.Party.LeaderHero, 5f);
		}
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase oldOwner, ShipOwnerChangeDetail details)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)details == 0 && oldOwner.IsSettlement)
		{
			Clan ownerClan = oldOwner.Settlement.OwnerClan;
			Kingdom val = ((ownerClan != null) ? ownerClan.Kingdom : null);
			if (val != null && val.RulingClan != ownerClan && val.HasPolicy(NavalPolicies.KingsTitheOnKeels))
			{
				int num = MathF.Round(Campaign.Current.Models.ShipCostModel.GetShipTradeValue(ship, oldOwner, ship.Owner) * 0.15f);
				GiveGoldAction.ApplyForPartyToCharacter(oldOwner, val.Leader, num, false);
			}
		}
	}

	private MobileParty GetAvailableNearbyPirateParty(Settlement settlement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 portPosition = settlement.PortPosition;
		LocatableSearchData<MobileParty> val = MobileParty.StartFindingLocatablesAroundPosition(((CampaignVec2)(ref portPosition)).ToVec2(), 50f);
		for (MobileParty val2 = MobileParty.FindNextLocatable(ref val); val2 != null; val2 = MobileParty.FindNextLocatable(ref val))
		{
			if (val2.IsBandit && val2.MapEvent == null && val2.HasNavalNavigationCapability && !_settlementToSurrenderByParty.ContainsKey(val2))
			{
				return val2;
			}
		}
		return null;
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Dictionary<MobileParty, Settlement>>("_settlementsToSurrenderByParties", ref _settlementToSurrenderByParty);
	}
}
