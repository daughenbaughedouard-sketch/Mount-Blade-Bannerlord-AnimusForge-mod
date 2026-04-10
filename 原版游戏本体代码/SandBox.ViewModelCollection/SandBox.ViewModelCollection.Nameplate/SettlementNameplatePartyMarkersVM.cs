using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Nameplate;

public class SettlementNameplatePartyMarkersVM : ViewModel
{
	public class PartyMarkerItemComparer : IComparer<SettlementNameplatePartyMarkerItemVM>
	{
		public int Compare(SettlementNameplatePartyMarkerItemVM x, SettlementNameplatePartyMarkerItemVM y)
		{
			return x.SortIndex.CompareTo(y.SortIndex);
		}
	}

	private Settlement _settlement;

	private bool _eventsRegistered;

	private PartyMarkerItemComparer _itemComparer;

	private MBBindingList<SettlementNameplatePartyMarkerItemVM> _partiesInSettlement;

	public MBBindingList<SettlementNameplatePartyMarkerItemVM> PartiesInSettlement
	{
		get
		{
			return _partiesInSettlement;
		}
		set
		{
			if (value != _partiesInSettlement)
			{
				_partiesInSettlement = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SettlementNameplatePartyMarkerItemVM>>(value, "PartiesInSettlement");
			}
		}
	}

	public SettlementNameplatePartyMarkersVM(Settlement settlement)
	{
		_settlement = settlement;
		PartiesInSettlement = new MBBindingList<SettlementNameplatePartyMarkerItemVM>();
		_itemComparer = new PartyMarkerItemComparer();
	}

	private void PopulatePartyList()
	{
		((Collection<SettlementNameplatePartyMarkerItemVM>)(object)PartiesInSettlement).Clear();
		foreach (MobileParty item in ((IEnumerable<MobileParty>)_settlement.Parties).Where((MobileParty p) => IsMobilePartyValid(p)))
		{
			((Collection<SettlementNameplatePartyMarkerItemVM>)(object)PartiesInSettlement).Add(new SettlementNameplatePartyMarkerItemVM(item));
		}
		PartiesInSettlement.Sort((IComparer<SettlementNameplatePartyMarkerItemVM>)_itemComparer);
	}

	private bool IsMobilePartyValid(MobileParty party)
	{
		if (!party.IsGarrison && !party.IsMilitia)
		{
			if (!party.IsMainParty || (party.IsMainParty && !Campaign.Current.IsMainHeroDisguised))
			{
				if (party.Army != null)
				{
					Army army = party.Army;
					if (army != null && army.LeaderParty.IsMainParty)
					{
						return !Campaign.Current.IsMainHeroDisguised;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private void OnSettlementLeft(MobileParty party, Settlement settlement)
	{
		if (settlement == _settlement)
		{
			SettlementNameplatePartyMarkerItemVM settlementNameplatePartyMarkerItemVM = ((IEnumerable<SettlementNameplatePartyMarkerItemVM>)PartiesInSettlement).SingleOrDefault((SettlementNameplatePartyMarkerItemVM p) => p.Party == party);
			if (settlementNameplatePartyMarkerItemVM != null)
			{
				((Collection<SettlementNameplatePartyMarkerItemVM>)(object)PartiesInSettlement).Remove(settlementNameplatePartyMarkerItemVM);
			}
		}
	}

	private void OnSettlementEntered(MobileParty partyEnteredSettlement, Settlement settlement, Hero leader)
	{
		if (settlement == _settlement && partyEnteredSettlement != null && ((IEnumerable<SettlementNameplatePartyMarkerItemVM>)PartiesInSettlement).SingleOrDefault((SettlementNameplatePartyMarkerItemVM p) => p.Party == partyEnteredSettlement) == null && IsMobilePartyValid(partyEnteredSettlement))
		{
			((Collection<SettlementNameplatePartyMarkerItemVM>)(object)PartiesInSettlement).Add(new SettlementNameplatePartyMarkerItemVM(partyEnteredSettlement));
			PartiesInSettlement.Sort((IComparer<SettlementNameplatePartyMarkerItemVM>)_itemComparer);
		}
	}

	private void OnMapEventEnded(MapEvent obj)
	{
		if (obj.MapEventSettlement != null && obj.MapEventSettlement == _settlement)
		{
			PopulatePartyList();
		}
	}

	public void RegisterEvents()
	{
		if (!_eventsRegistered)
		{
			PopulatePartyList();
			CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, Hero>)OnSettlementEntered);
			CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement>)OnSettlementLeft);
			CampaignEvents.MapEventEnded.AddNonSerializedListener((object)this, (Action<MapEvent>)OnMapEventEnded);
			_eventsRegistered = true;
		}
	}

	public void UnloadEvents()
	{
		if (_eventsRegistered)
		{
			((IMbEventBase)CampaignEvents.SettlementEntered).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnSettlementLeftEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.MapEventEnded).ClearListeners((object)this);
			((Collection<SettlementNameplatePartyMarkerItemVM>)(object)PartiesInSettlement).Clear();
			_eventsRegistered = false;
		}
	}
}
