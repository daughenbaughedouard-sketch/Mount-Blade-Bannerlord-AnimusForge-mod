using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.Tracker;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Map.Tracker;

public class MapTrackerProvider
{
	private class TrackerContainer
	{
		private readonly Dictionary<ITrackableCampaignObject, MapTrackerItemVM> _trackers;

		public OnTrackerAddedOrRemovedDelegate OnTrackerAddedOrRemoved;

		public TrackerContainer()
		{
			_trackers = new Dictionary<ITrackableCampaignObject, MapTrackerItemVM>();
		}

		public MapTrackerItemVM[] GetTrackers()
		{
			return _trackers.Values.ToArray();
		}

		public bool HasTrackerFor(ITrackableCampaignObject trackable)
		{
			return GetTrackerFor(trackable) != null;
		}

		public MapTrackerItemVM GetTrackerFor(ITrackableCampaignObject trackable)
		{
			if (_trackers.TryGetValue(trackable, out var value))
			{
				return value;
			}
			return null;
		}

		public void AddTracker(MapTrackerItemVM tracker)
		{
			if (_trackers.ContainsKey(tracker.TrackedObject))
			{
				Debug.FailedAssert("Trying to add a tracker that was already added", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Map\\Tracker\\MapTrackerProvider.cs", "AddTracker", 54);
				return;
			}
			_trackers.Add(tracker.TrackedObject, tracker);
			OnTrackerAddedOrRemoved?.Invoke(tracker, added: true);
		}

		public void RemoveTracker(MapTrackerItemVM tracker)
		{
			if (!_trackers.ContainsKey(tracker.TrackedObject))
			{
				Debug.FailedAssert("Trying to remove a tracker that was not added", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Map\\Tracker\\MapTrackerProvider.cs", "RemoveTracker", 66);
				return;
			}
			_trackers.Remove(tracker.TrackedObject);
			OnTrackerAddedOrRemoved?.Invoke(tracker, added: false);
		}

		public void ClearTrackers()
		{
			MapTrackerItemVM[] array = _trackers.Values.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				RemoveTracker(array[i]);
			}
		}
	}

	public delegate void OnTrackerAddedOrRemovedDelegate(MapTrackerItemVM tracker, bool added);

	private TrackerContainer _trackerContainer;

	public event OnTrackerAddedOrRemovedDelegate OnTrackerAddedOrRemoved
	{
		add
		{
			TrackerContainer trackerContainer = _trackerContainer;
			trackerContainer.OnTrackerAddedOrRemoved = (OnTrackerAddedOrRemovedDelegate)Delegate.Combine(trackerContainer.OnTrackerAddedOrRemoved, value);
		}
		remove
		{
			TrackerContainer trackerContainer = _trackerContainer;
			trackerContainer.OnTrackerAddedOrRemoved = (OnTrackerAddedOrRemovedDelegate)Delegate.Remove(trackerContainer.OnTrackerAddedOrRemoved, value);
		}
	}

	public MapTrackerProvider()
	{
		CampaignEvents.ArmyCreated.AddNonSerializedListener((object)this, (Action<Army>)OnArmyCreated);
		CampaignEvents.ArmyDispersed.AddNonSerializedListener((object)this, (Action<Army, ArmyDispersionReason, bool>)OnArmyDispersed);
		CampaignEvents.MobilePartyCreated.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyCreated);
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnPartyDestroyed);
		CampaignEvents.MobilePartyQuestStatusChanged.AddNonSerializedListener((object)this, (Action<MobileParty, bool>)OnPartyQuestStatusChanged);
		CampaignEvents.OnPartyDisbandedEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnPartyDisbanded);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangedKingdom);
		CampaignEvents.OnClanCreatedEvent.AddNonSerializedListener((object)this, (Action<Clan, bool>)OnCompanionClanCreated);
		CampaignEvents.OnMapMarkerCreatedEvent.AddNonSerializedListener((object)this, (Action<MapMarker>)OnMapMarkerCreated);
		CampaignEvents.OnMapMarkerRemovedEvent.AddNonSerializedListener((object)this, (Action<MapMarker>)OnMapMarkerRemoved);
		_trackerContainer = new TrackerContainer();
		ResetTrackers();
	}

	private void OnFinalize()
	{
		_trackerContainer.ClearTrackers();
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	public MapTrackerItemVM[] GetTrackers()
	{
		return _trackerContainer.GetTrackers();
	}

	private void ResetTrackers()
	{
		_trackerContainer.ClearTrackers();
		MBReadOnlyList<MobileParty> all = MobileParty.All;
		for (int i = 0; i < ((List<MobileParty>)(object)all).Count; i++)
		{
			MobileParty party = ((List<MobileParty>)(object)all)[i];
			AddIfEligible(party);
		}
		Army[] array = ((IEnumerable<Kingdom>)Kingdom.All).SelectMany((Kingdom k) => (IEnumerable<Army>)k.Armies).ToArray();
		foreach (Army army in array)
		{
			AddIfEligible(army);
		}
		if (Campaign.Current.MapMarkerManager == null)
		{
			return;
		}
		foreach (MapMarker item in (List<MapMarker>)(object)Campaign.Current.MapMarkerManager.MapMarkers)
		{
			AddIfEligible(item);
		}
	}

	private bool CanAddMobileParty(MobileParty party)
	{
		if (!party.IsMainParty && !party.IsMilitia && !party.IsGarrison && !party.IsVillager && !party.IsBandit && !party.IsPatrolParty && !party.IsBanditBossParty && !party.IsCurrentlyUsedByAQuest && (!party.IsCaravan || party.CaravanPartyComponent.Owner == Hero.MainHero))
		{
			if (party.IsLordParty)
			{
				for (int i = 0; i < ((List<WarPartyComponent>)(object)Clan.PlayerClan.WarPartyComponents).Count; i++)
				{
					if (((PartyComponent)((List<WarPartyComponent>)(object)Clan.PlayerClan.WarPartyComponents)[i]).MobileParty == party)
					{
						return true;
					}
				}
			}
			for (int j = 0; j < ((List<Hero>)(object)Clan.PlayerClan.Heroes).Count; j++)
			{
				Hero val = ((List<Hero>)(object)Clan.PlayerClan.Heroes)[j];
				for (int k = 0; k < val.OwnedCaravans.Count; k++)
				{
					if (((PartyComponent)val.OwnedCaravans[k]).MobileParty == party)
					{
						return true;
					}
				}
			}
		}
		if (party.LeaderHero == null && party.IsCurrentlyUsedByAQuest && Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)party))
		{
			return true;
		}
		return false;
	}

	private bool CanAddArmy(Army army)
	{
		if ((object)army.Kingdom == Hero.MainHero.MapFaction)
		{
			return !((List<MobileParty>)(object)army.Parties).Contains(MobileParty.MainParty);
		}
		return false;
	}

	private void RemoveIfExists(ITrackableCampaignObject trackable)
	{
		MapTrackerItemVM trackerFor = _trackerContainer.GetTrackerFor(trackable);
		if (trackerFor != null)
		{
			_trackerContainer.RemoveTracker(trackerFor);
		}
	}

	private void AddIfEligible(MobileParty party)
	{
		if (CanAddMobileParty(party) && !_trackerContainer.HasTrackerFor((ITrackableCampaignObject)(object)party))
		{
			_trackerContainer.AddTracker(new MapMobilePartyTrackItemVM(party));
		}
	}

	private void AddIfEligible(Army army)
	{
		if (CanAddArmy(army) && !_trackerContainer.HasTrackerFor((ITrackableCampaignObject)(object)army))
		{
			_trackerContainer.AddTracker(new MapArmyTrackItemVM(army));
		}
	}

	private void AddIfEligible(MapMarker mapMarker)
	{
		if (!_trackerContainer.HasTrackerFor((ITrackableCampaignObject)(object)mapMarker))
		{
			_trackerContainer.AddTracker(new MapMarkerTrackerItemVM(mapMarker));
		}
	}

	private void OnPartyDestroyed(MobileParty mobileParty, PartyBase arg2)
	{
		RemoveIfExists((ITrackableCampaignObject)(object)mobileParty);
	}

	private void OnPartyQuestStatusChanged(MobileParty mobileParty, bool isUsedByQuest)
	{
		if (isUsedByQuest)
		{
			if (mobileParty.LeaderHero == null && Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)mobileParty))
			{
				AddIfEligible(mobileParty);
			}
			else
			{
				RemoveIfExists((ITrackableCampaignObject)(object)mobileParty);
			}
		}
		else
		{
			AddIfEligible(mobileParty);
		}
	}

	private void OnPartyDisbanded(MobileParty disbandedParty, Settlement relatedSettlement)
	{
		RemoveIfExists((ITrackableCampaignObject)(object)disbandedParty);
	}

	private void OnMobilePartyCreated(MobileParty mobileParty)
	{
		AddIfEligible(mobileParty);
	}

	private void OnArmyDispersed(Army army, ArmyDispersionReason arg2, bool arg3)
	{
		RemoveIfExists((ITrackableCampaignObject)(object)army);
	}

	private void OnArmyCreated(Army army)
	{
		AddIfEligible(army);
	}

	private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification)
	{
		if (clan == Clan.PlayerClan)
		{
			ResetTrackers();
		}
	}

	private void OnCompanionClanCreated(Clan clan, bool isCompanion)
	{
		if (isCompanion && clan.Leader.PartyBelongedTo != null)
		{
			RemoveIfExists((ITrackableCampaignObject)(object)clan.Leader.PartyBelongedTo);
		}
	}

	private void OnMapMarkerRemoved(MapMarker marker)
	{
		RemoveIfExists((ITrackableCampaignObject)(object)marker);
	}

	private void OnMapMarkerCreated(MapMarker marker)
	{
		AddIfEligible(marker);
	}
}
