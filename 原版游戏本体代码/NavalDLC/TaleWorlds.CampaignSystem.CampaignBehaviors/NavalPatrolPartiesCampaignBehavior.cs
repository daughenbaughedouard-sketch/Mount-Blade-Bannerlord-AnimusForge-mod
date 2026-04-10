using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC;
using NavalDLC.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.CampaignBehaviors;

public class NavalPatrolPartiesCampaignBehavior : CampaignBehaviorBase, INavalPatrolPartiesCampaignBehavior
{
	private Dictionary<Settlement, CampaignTime> _partyGenerationQueue = new Dictionary<Settlement, CampaignTime>();

	private Dictionary<Settlement, MobileParty> _patrolParties = new Dictionary<Settlement, MobileParty>();

	public override void RegisterEvents()
	{
		CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener((object)this, (Action<Settlement>)DailyTickSettlement);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChangedEvent);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter, int>)OnNewGameCreated);
		CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)SettlementEntered);
		CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener((object)this, (Action<MobileParty, PartyThinkParams>)AiHourlyTick);
		CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
	}

	private void OnMobilePartyDestroyed(MobileParty party, PartyBase destroyerParty)
	{
		if (party.IsPatrolParty && party.PatrolPartyComponent.IsNaval)
		{
			_patrolParties.Remove(party.HomeSettlement);
		}
	}

	private void SettlementEntered(MobileParty party, Settlement settlement, Hero hero)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		if (party == null || !party.IsPatrolParty || !party.PatrolPartyComponent.IsNaval || settlement != party.HomeSettlement)
		{
			return;
		}
		for (int i = 0; i < ((List<Ship>)(object)party.Ships).Count; i++)
		{
			RepairShipAction.ApplyForFree(((List<Ship>)(object)party.Ships)[i]);
		}
		foreach (ShipTemplateStack item in (List<ShipTemplateStack>)(object)Campaign.Current.Models.SettlementPatrolModel.GetPartyTemplateForPatrolParty(settlement, true).ShipHulls)
		{
			ShipHull shipHull = item.ShipHull;
			int num = ((IEnumerable<Ship>)party.Ships).Count((Ship x) => x.ShipHull == shipHull);
			if (num < item.MaxValue)
			{
				for (int num2 = 0; num2 < item.MaxValue - num; num2++)
				{
					Ship val = new Ship(shipHull);
					ChangeShipOwnerAction.ApplyByTransferring(party.Party, val);
				}
			}
		}
	}

	private void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
	{
		if (mobileParty.IsPatrolParty && !mobileParty.IsDisbanding && mobileParty.PatrolPartyComponent.IsNaval)
		{
			Settlement currentSettlement = mobileParty.CurrentSettlement;
			if (((currentSettlement != null) ? currentSettlement.SiegeEvent : null) == null || !mobileParty.CurrentSettlement.SiegeEvent.IsBlockadeActive)
			{
				CalculateVisitHomeSettlementScoreDueToShipHealth(mobileParty, p);
			}
		}
	}

	private void CalculateVisitHomeSettlementScoreDueToShipHealth(MobileParty mobileParty, PartyThinkParams p)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!CanVisitSettlement(mobileParty, mobileParty.HomeSettlement))
		{
			return;
		}
		float overallShipHealthRatio = GetOverallShipHealthRatio(mobileParty);
		if (overallShipHealthRatio < 0.95f)
		{
			float num = 1f / MathF.Max(overallShipHealthRatio, 0.01f);
			NavigationType val = default(NavigationType);
			float num2 = default(float);
			bool flag = default(bool);
			AiHelper.GetBestNavigationTypeAndAdjustedDistanceOfSettlementForMobileParty(mobileParty, mobileParty.HomeSettlement, true, ref val, ref num2, ref flag);
			AIBehaviorData item = default(AIBehaviorData);
			((AIBehaviorData)(ref item))._002Ector((IMapPoint)(object)mobileParty.HomeSettlement, (AiBehavior)2, val, false, flag, true);
			float num3 = default(float);
			if (p.TryGetBehaviorScore(ref item, ref num3))
			{
				p.SetBehaviorScore(ref item, num + num3);
				return;
			}
			(AIBehaviorData, float) tuple = (item, num);
			p.AddBehaviorScore(ref tuple);
		}
	}

	private bool CanVisitSettlement(MobileParty mobileParty, Settlement settlement)
	{
		if (settlement.SiegeEvent != null)
		{
			return !settlement.SiegeEvent.IsBlockadeActive;
		}
		return true;
	}

	private float GetOverallShipHealthRatio(MobileParty mobileParty)
	{
		float num = ((IEnumerable<ShipTemplateStack>)Campaign.Current.Models.SettlementPatrolModel.GetPartyTemplateForPatrolParty(mobileParty.HomeSettlement, true).ShipHulls).Sum((ShipTemplateStack x) => x.ShipHull.MaxHitPoints * x.MaxValue);
		return ((IEnumerable<Ship>)mobileParty.Ships).Sum((Ship x) => MathF.Min(x.HitPoints, (float)x.ShipHull.MaxHitPoints)) / num;
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (party.IsPatrolParty && party.PatrolPartyComponent.IsNaval)
		{
			_ = party.HomeSettlement;
		}
	}

	private void DailyTickSettlement(Settlement settlement)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (CanSettlementSpawnNewPartyCurrently(settlement, includeReason: false, out var _))
		{
			if (!_partyGenerationQueue.TryGetValue(settlement, out var value))
			{
				UpdateSettlementQueue(settlement, CampaignTime.Now + Campaign.Current.Models.SettlementPatrolModel.GetPatrolPartySpawnDuration(settlement, true));
			}
			else if (((CampaignTime)(ref value)).IsPast)
			{
				SpawnPatrolParty(settlement);
			}
		}
		else
		{
			UpdateSettlementParties(settlement);
		}
	}

	private void OnNewGameCreated(CampaignGameStarter starter, int index)
	{
		if (index != 88)
		{
			return;
		}
		foreach (Town allFief in Town.AllFiefs)
		{
			if (CanSettlementSpawnNewPartyCurrently(((SettlementComponent)allFief).Settlement, includeReason: false, out var _))
			{
				SpawnPatrolParty(((SettlementComponent)allFief).Settlement);
			}
		}
	}

	private void OnSettlementOwnerChangedEvent(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		if (GetNavalPatrolParty(settlement) != null)
		{
			RemoveSettlementParties(settlement);
		}
	}

	private bool CanSettlementSpawnNewPartyCurrently(Settlement settlement, bool includeReason, out TextObject reason)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		reason = null;
		if (!Campaign.Current.Models.SettlementPatrolModel.CanSettlementHavePatrolParties(settlement, true))
		{
			PolicyObject coastalGuardEdict = NavalPolicies.CoastalGuardEdict;
			if (includeReason)
			{
				reason = new TextObject("{=ipat9DbO}No {POLICY_NAME}", (Dictionary<string, object>)null);
				reason.SetTextVariable("POLICY_NAME", ((PropertyObject)coastalGuardEdict).Name);
			}
			return false;
		}
		if (settlement.InRebelliousState)
		{
			if (includeReason)
			{
				reason = new TextObject("{=UHDv0qer}Rebellious", (Dictionary<string, object>)null);
			}
			return false;
		}
		if (settlement.Town.IsUnderSiege || settlement.Party.MapEvent != null)
		{
			if (includeReason)
			{
				reason = new TextObject("{=BhiOmgst}Under Siege", (Dictionary<string, object>)null);
			}
			return false;
		}
		if (includeReason)
		{
			reason = TextObject.GetEmpty();
		}
		return GetNavalPatrolParty(settlement) == null;
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<Dictionary<Settlement, CampaignTime>>("_partyGenerationQueue", ref _partyGenerationQueue);
		dataStore.SyncData<Dictionary<Settlement, MobileParty>>("_patrolParties", ref _patrolParties);
	}

	private void UpdateSettlementParties(Settlement settlement)
	{
		if (!Campaign.Current.Models.SettlementPatrolModel.CanSettlementHavePatrolParties(settlement, true) && GetNavalPatrolParty(settlement) != null)
		{
			RemoveSettlementParties(settlement);
		}
	}

	private void RemoveSettlementParties(Settlement settlement)
	{
		_partyGenerationQueue.Remove(settlement);
		MobileParty navalPatrolParty = GetNavalPatrolParty(settlement);
		navalPatrolParty.MapEventSide = null;
		if (navalPatrolParty.IsActive)
		{
			DestroyPartyAction.Apply((PartyBase)null, navalPatrolParty);
		}
	}

	private void UpdateSettlementQueue(Settlement settlement, CampaignTime time)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_partyGenerationQueue[settlement] = time;
	}

	private void SpawnPatrolParty(Settlement settlement)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_partyGenerationQueue.Remove(settlement);
		PartyTemplateObject partyTemplateForPatrolParty = Campaign.Current.Models.SettlementPatrolModel.GetPartyTemplateForPatrolParty(settlement, true);
		MobileParty value = PatrolPartyComponent.CreatePatrolParty("naval_patrol_party_1", settlement.PortPosition, 8f * Campaign.Current.EstimatedAverageBanditPartySpeed, settlement, partyTemplateForPatrolParty);
		_patrolParties[settlement] = value;
	}

	public TextObject GetSettlementPatrolStatus(Settlement settlement)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		TextObject empty = TextObject.GetEmpty();
		MobileParty navalPatrolParty = GetNavalPatrolParty(settlement);
		TextObject reason;
		CampaignTime value;
		if (navalPatrolParty != null)
		{
			empty = new TextObject("{=sUb6FHIE}{REMAINING_TROOP_COUNT}/{TOTAL_TROOP_COUNT}", (Dictionary<string, object>)null);
			empty.SetTextVariable("REMAINING_TROOP_COUNT", navalPatrolParty.MemberRoster.TotalManCount);
			empty.SetTextVariable("TOTAL_TROOP_COUNT", navalPatrolParty.Party.PartySizeLimit);
		}
		else if (!CanSettlementSpawnNewPartyCurrently(settlement, includeReason: true, out reason))
		{
			empty = reason;
		}
		else if (_partyGenerationQueue.TryGetValue(settlement, out value))
		{
			int num = ((value == CampaignTime.Zero) ? 1 : Math.Max((int)Math.Ceiling(((CampaignTime)(ref value)).RemainingDaysFromNow), 1));
			empty = new TextObject("{=LvwUsZ9p}Ready in {DAYS} {?DAYS > 1}days{?}day{\\?}", (Dictionary<string, object>)null);
			empty.SetTextVariable("DAYS", num);
		}
		else
		{
			empty = new TextObject("{=trainingPatrolParties}Training", (Dictionary<string, object>)null);
		}
		return empty;
	}

	public MobileParty GetNavalPatrolParty(Settlement settlement)
	{
		if (_patrolParties.TryGetValue(settlement, out var value))
		{
			return value;
		}
		return null;
	}
}
