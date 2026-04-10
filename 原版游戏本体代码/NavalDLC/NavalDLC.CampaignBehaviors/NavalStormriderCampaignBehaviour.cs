using System;
using System.Collections.Generic;
using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace NavalDLC.CampaignBehaviors;

public class NavalStormriderCampaignBehaviour : CampaignBehaviorBase
{
	private Dictionary<MobileParty, CampaignTime> _partiesEnteredStorm = new Dictionary<MobileParty, CampaignTime>();

	private CampaignTime _playerLastStormEnterTime = CampaignTime.Never;

	public override void RegisterEvents()
	{
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)OnHourlyTick);
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)TickEvent);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
	}

	private void TickEvent(float deltaTime)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!(((CampaignTime)(ref _playerLastStormEnterTime)).ElapsedDaysUntilNow > 1f) && !(_playerLastStormEnterTime == CampaignTime.Never))
		{
			return;
		}
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			CampaignVec2 position = MobileParty.MainParty.Position;
			if (((CampaignVec2)(ref position)).DistanceSquared(item.CurrentPosition) <= item.EffectRadius * item.EffectRadius)
			{
				_playerLastStormEnterTime = CampaignTime.Now;
				AddXpToTroops(MobileParty.MainParty, MathF.Round(NavalPerks.Shipmaster.Stormrider.PrimaryBonus));
			}
		}
	}

	private void OnHourlyTick()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			LocatableSearchData<MobileParty> val = MobileParty.StartFindingLocatablesAroundPosition(item.CurrentPosition, item.EffectRadius);
			MobileParty val2 = MobileParty.FindNextLocatable(ref val);
			while (val2 != null)
			{
				if (val2 == MobileParty.MainParty)
				{
					val2 = MobileParty.FindNextLocatable(ref val);
					continue;
				}
				if (val2.IsCurrentlyAtSea && val2.MapEvent == null)
				{
					if (_partiesEnteredStorm.ContainsKey(val2))
					{
						CampaignTime val3 = _partiesEnteredStorm[val2];
						if (!(((CampaignTime)(ref val3)).ElapsedDaysUntilNow > 1f))
						{
							goto IL_0091;
						}
					}
					OnPartyEnteredStorm(val2);
				}
				goto IL_0091;
				IL_0091:
				val2 = MobileParty.FindNextLocatable(ref val);
			}
		}
	}

	private void OnPartyEnteredStorm(MobileParty party)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (party.HasPerk(NavalPerks.Shipmaster.Stormrider, false))
		{
			_partiesEnteredStorm[party] = CampaignTime.Now;
			AddXpToTroops(party, MathF.Round(NavalPerks.Shipmaster.Stormrider.PrimaryBonus));
		}
	}

	private static void AddXpToTroops(MobileParty party, int amount)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		TroopRoster memberRoster = party.MemberRoster;
		int val = default(int);
		for (int i = 0; i < memberRoster.Count; i++)
		{
			TroopRosterElement elementCopyAtIndex = memberRoster.GetElementCopyAtIndex(i);
			if (!((BasicCharacterObject)elementCopyAtIndex.Character).IsHero && MobilePartyHelper.CanTroopGainXp(party.Party, elementCopyAtIndex.Character, ref val))
			{
				int num = Math.Min(val, amount);
				memberRoster.AddXpToTroopAtIndex(i, num);
			}
		}
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase party)
	{
		_partiesEnteredStorm.Remove(mobileParty);
	}

	private void DoCleanUp()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		List<MobileParty> list = new List<MobileParty>();
		foreach (KeyValuePair<MobileParty, CampaignTime> item in _partiesEnteredStorm)
		{
			CampaignTime value = item.Value;
			if (((CampaignTime)(ref value)).ElapsedDaysUntilNow > 1f)
			{
				list.Add(item.Key);
			}
		}
		foreach (MobileParty item2 in list)
		{
			_partiesEnteredStorm.Remove(item2);
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
		DoCleanUp();
		dataStore.SyncData<Dictionary<MobileParty, CampaignTime>>("_partiesEnteredStorm", ref _partiesEnteredStorm);
		dataStore.SyncData<CampaignTime>("_playerLastStormEnterTime", ref _playerLastStormEnterTime);
	}
}
