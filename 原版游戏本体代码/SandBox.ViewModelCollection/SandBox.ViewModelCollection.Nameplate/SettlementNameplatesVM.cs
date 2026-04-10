using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Nameplate;

public class SettlementNameplatesVM : ViewModel
{
	private readonly Camera _mapCamera;

	private Vec3 _cachedCameraPosition;

	private readonly ParallelForAuxPredicate UpdateNameplateAuxMTPredicate;

	private readonly Action<CampaignVec2> _fastMoveCameraToPosition;

	private IEnumerable<Tuple<Settlement, GameEntity>> _allHideouts;

	private IEnumerable<Tuple<Settlement, GameEntity>> _allRetreats;

	private IEnumerable<Tuple<Settlement, GameEntity>> _allRegularSettlements;

	private MBList<SettlementNameplateVM> _allNameplates;

	private Dictionary<Settlement, SettlementNameplateVM> _allNameplatesBySettlements;

	private MBBindingList<SettlementNameplateVM> _smallNameplates;

	private MBBindingList<SettlementNameplateVM> _mediumNameplates;

	private MBBindingList<SettlementNameplateVM> _largeNameplates;

	public MBReadOnlyList<SettlementNameplateVM> AllNameplates => (MBReadOnlyList<SettlementNameplateVM>)(object)_allNameplates;

	[DataSourceProperty]
	public MBBindingList<SettlementNameplateVM> SmallNameplates
	{
		get
		{
			return _smallNameplates;
		}
		set
		{
			if (_smallNameplates != value)
			{
				_smallNameplates = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SettlementNameplateVM>>(value, "SmallNameplates");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SettlementNameplateVM> MediumNameplates
	{
		get
		{
			return _mediumNameplates;
		}
		set
		{
			if (_mediumNameplates != value)
			{
				_mediumNameplates = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SettlementNameplateVM>>(value, "MediumNameplates");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SettlementNameplateVM> LargeNameplates
	{
		get
		{
			return _largeNameplates;
		}
		set
		{
			if (_largeNameplates != value)
			{
				_largeNameplates = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SettlementNameplateVM>>(value, "LargeNameplates");
			}
		}
	}

	public SettlementNameplatesVM(Camera mapCamera, Action<CampaignVec2> fastMoveCameraToPosition)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		_allNameplates = new MBList<SettlementNameplateVM>(400);
		_allNameplatesBySettlements = new Dictionary<Settlement, SettlementNameplateVM>(400);
		SmallNameplates = new MBBindingList<SettlementNameplateVM>();
		MediumNameplates = new MBBindingList<SettlementNameplateVM>();
		LargeNameplates = new MBBindingList<SettlementNameplateVM>();
		_mapCamera = mapCamera;
		_fastMoveCameraToPosition = fastMoveCameraToPosition;
		CampaignEvents.PartyVisibilityChangedEvent.AddNonSerializedListener((object)this, (Action<PartyBase>)OnPartyBaseVisibilityChange);
		CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
		CampaignEvents.MakePeace.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, MakePeaceDetail>)OnPeaceDeclared);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangeKingdom);
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.OnSiegeEventStartedEvent.AddNonSerializedListener((object)this, (Action<SiegeEvent>)OnSiegeEventStartedOnSettlement);
		CampaignEvents.OnSiegeEventEndedEvent.AddNonSerializedListener((object)this, (Action<SiegeEvent>)OnSiegeEventEndedOnSettlement);
		CampaignEvents.RebelliousClanDisbandedAtSettlement.AddNonSerializedListener((object)this, (Action<Settlement, Clan>)OnRebelliousClanDisbandedAtSettlement);
		CampaignEvents.OnAllianceStartedEvent.AddNonSerializedListener((object)this, (Action<Kingdom, Kingdom>)OnAllianceStarted);
		CampaignEvents.OnAllianceEndedEvent.AddNonSerializedListener((object)this, (Action<Kingdom, Kingdom>)OnAllianceEnded);
		UpdateNameplateAuxMTPredicate = new ParallelForAuxPredicate(UpdateNameplateAuxMT);
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
		for (int i = 0; i < ((List<SettlementNameplateVM>)(object)_allNameplates).Count; i++)
		{
			((ViewModel)((List<SettlementNameplateVM>)(object)_allNameplates)[i]).OnFinalize();
		}
		((List<SettlementNameplateVM>)(object)_allNameplates).Clear();
		_allNameplatesBySettlements.Clear();
		((Collection<SettlementNameplateVM>)(object)SmallNameplates).Clear();
		((Collection<SettlementNameplateVM>)(object)MediumNameplates).Clear();
		((Collection<SettlementNameplateVM>)(object)LargeNameplates).Clear();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		for (int i = 0; i < ((List<SettlementNameplateVM>)(object)_allNameplates).Count; i++)
		{
			((ViewModel)((List<SettlementNameplateVM>)(object)_allNameplates)[i]).RefreshValues();
		}
	}

	public void Initialize(IEnumerable<Tuple<Settlement, GameEntity>> settlements)
	{
		_allRegularSettlements = settlements.Where((Tuple<Settlement, GameEntity> x) => !x.Item1.IsHideout && !(x.Item1.SettlementComponent is RetirementSettlementComponent));
		_allHideouts = settlements.Where((Tuple<Settlement, GameEntity> x) => x.Item1.IsHideout && !(x.Item1.SettlementComponent is RetirementSettlementComponent));
		_allRetreats = settlements.Where((Tuple<Settlement, GameEntity> x) => !x.Item1.IsHideout && x.Item1.SettlementComponent is RetirementSettlementComponent);
		foreach (Tuple<Settlement, GameEntity> allRegularSettlement in _allRegularSettlements)
		{
			if (allRegularSettlement.Item1.IsVisible)
			{
				SettlementNameplateVM nameplate = new SettlementNameplateVM(allRegularSettlement.Item1, allRegularSettlement.Item2, _mapCamera, _fastMoveCameraToPosition);
				AddNameplate(nameplate);
			}
		}
		foreach (Tuple<Settlement, GameEntity> allHideout in _allHideouts)
		{
			if (allHideout.Item1.Hideout.IsSpotted)
			{
				SettlementNameplateVM nameplate2 = new SettlementNameplateVM(allHideout.Item1, allHideout.Item2, _mapCamera, _fastMoveCameraToPosition);
				AddNameplate(nameplate2);
			}
		}
		foreach (Tuple<Settlement, GameEntity> allRetreat in _allRetreats)
		{
			SettlementComponent settlementComponent = allRetreat.Item1.SettlementComponent;
			RetirementSettlementComponent val;
			if ((val = (RetirementSettlementComponent)(object)((settlementComponent is RetirementSettlementComponent) ? settlementComponent : null)) != null)
			{
				if (val.IsSpotted)
				{
					SettlementNameplateVM nameplate3 = new SettlementNameplateVM(allRetreat.Item1, allRetreat.Item2, _mapCamera, _fastMoveCameraToPosition);
					AddNameplate(nameplate3);
				}
			}
			else
			{
				Debug.FailedAssert("A seetlement which is IsRetreat doesn't have a retirement component.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SettlementNameplatesVM.cs", "Initialize", 120);
			}
		}
		for (int num = 0; num < ((List<SettlementNameplateVM>)(object)_allNameplates).Count; num++)
		{
			SettlementNameplateVM settlementNameplateVM = ((List<SettlementNameplateVM>)(object)_allNameplates)[num];
			Settlement settlement = settlementNameplateVM.Settlement;
			if (((settlement != null) ? settlement.SiegeEvent : null) != null)
			{
				Settlement settlement2 = settlementNameplateVM.Settlement;
				settlementNameplateVM.OnSiegeEventStartedOnSettlement((settlement2 != null) ? settlement2.SiegeEvent : null);
			}
			else if (settlementNameplateVM.Settlement.IsTown || settlementNameplateVM.Settlement.IsCastle)
			{
				Clan ownerClan = settlementNameplateVM.Settlement.OwnerClan;
				if (ownerClan != null && ownerClan.IsRebelClan)
				{
					settlementNameplateVM.OnRebelliousClanFormed(settlementNameplateVM.Settlement.OwnerClan);
				}
			}
		}
		RefreshRelationsOfNameplates();
	}

	private void AddNameplate(SettlementNameplateVM nameplate)
	{
		((List<SettlementNameplateVM>)(object)_allNameplates).Add(nameplate);
		_allNameplatesBySettlements[nameplate.Settlement] = nameplate;
		switch (nameplate.SettlementTypeEnum)
		{
		case SettlementNameplateVM.Type.Village:
			((Collection<SettlementNameplateVM>)(object)SmallNameplates).Add(nameplate);
			break;
		case SettlementNameplateVM.Type.Castle:
			((Collection<SettlementNameplateVM>)(object)MediumNameplates).Add(nameplate);
			break;
		case SettlementNameplateVM.Type.Town:
			((Collection<SettlementNameplateVM>)(object)LargeNameplates).Add(nameplate);
			break;
		}
	}

	private void RemoveNameplate(SettlementNameplateVM nameplate)
	{
		((List<SettlementNameplateVM>)(object)_allNameplates).Remove(nameplate);
		_allNameplatesBySettlements.Remove(nameplate.Settlement);
		((Collection<SettlementNameplateVM>)(object)SmallNameplates).Remove(nameplate);
		((Collection<SettlementNameplateVM>)(object)MediumNameplates).Remove(nameplate);
		((Collection<SettlementNameplateVM>)(object)LargeNameplates).Remove(nameplate);
	}

	private void UpdateNameplateAuxMT(int startInclusive, int endExclusive)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		for (int i = startInclusive; i < endExclusive; i++)
		{
			((List<SettlementNameplateVM>)(object)_allNameplates)[i].UpdateNameplateMT(_cachedCameraPosition);
		}
	}

	public void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_cachedCameraPosition = _mapCamera.Position;
		TWParallel.For(0, ((List<SettlementNameplateVM>)(object)_allNameplates).Count, UpdateNameplateAuxMTPredicate, 16);
		for (int i = 0; i < ((List<SettlementNameplateVM>)(object)_allNameplates).Count; i++)
		{
			((List<SettlementNameplateVM>)(object)_allNameplates)[i].RefreshBindValues();
		}
	}

	private void OnSiegeEventStartedOnSettlement(SiegeEvent siegeEvent)
	{
		if (_allNameplatesBySettlements.TryGetValue(siegeEvent.BesiegedSettlement, out var value))
		{
			value.OnSiegeEventStartedOnSettlement(siegeEvent);
		}
	}

	private void OnSiegeEventEndedOnSettlement(SiegeEvent siegeEvent)
	{
		if (_allNameplatesBySettlements.TryGetValue(siegeEvent.BesiegedSettlement, out var value))
		{
			value.OnSiegeEventEndedOnSettlement(siegeEvent);
		}
	}

	private void OnMapEventStartedOnSettlement(MapEvent mapEvent, PartyBase attackerParty, PartyBase defenderParty)
	{
		if (_allNameplatesBySettlements.TryGetValue(mapEvent.MapEventSettlement, out var value))
		{
			value.OnMapEventStartedOnSettlement(mapEvent);
		}
	}

	private void OnMapEventEndedOnSettlement(MapEvent mapEvent)
	{
		if (_allNameplatesBySettlements.TryGetValue(mapEvent.MapEventSettlement, out var value))
		{
			value.OnMapEventEndedOnSettlement();
		}
	}

	private void OnPartyBaseVisibilityChange(PartyBase party)
	{
		if (!party.IsSettlement)
		{
			return;
		}
		Tuple<Settlement, GameEntity> tuple = null;
		tuple = (party.Settlement.IsHideout ? _allHideouts.SingleOrDefault((Tuple<Settlement, GameEntity> h) => h.Item1.Hideout == party.Settlement.Hideout) : ((!(party.Settlement.SettlementComponent is RetirementSettlementComponent)) ? _allRegularSettlements.SingleOrDefault((Tuple<Settlement, GameEntity> h) => h.Item1 == party.Settlement) : _allRetreats.SingleOrDefault(delegate(Tuple<Settlement, GameEntity> h)
		{
			SettlementComponent settlementComponent = h.Item1.SettlementComponent;
			SettlementComponent obj = ((settlementComponent is RetirementSettlementComponent) ? settlementComponent : null);
			SettlementComponent settlementComponent2 = party.Settlement.SettlementComponent;
			return obj == ((settlementComponent2 is RetirementSettlementComponent) ? settlementComponent2 : null);
		})));
		if (tuple != null)
		{
			SettlementNameplateVM value = null;
			if (tuple.Item1 != null)
			{
				_allNameplatesBySettlements.TryGetValue(tuple.Item1, out value);
			}
			if (party.IsVisible && value == null)
			{
				SettlementNameplateVM settlementNameplateVM = new SettlementNameplateVM(tuple.Item1, tuple.Item2, _mapCamera, _fastMoveCameraToPosition);
				AddNameplate(settlementNameplateVM);
				settlementNameplateVM.RefreshRelationStatus();
			}
			else if (!party.IsVisible && value != null)
			{
				RemoveNameplate(value);
			}
		}
	}

	private void OnPeaceDeclared(IFaction faction1, IFaction faction2, MakePeaceDetail detail)
	{
		OnPeaceOrWarDeclared(faction1, faction2);
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail arg3)
	{
		OnPeaceOrWarDeclared(faction1, faction2);
	}

	private void OnPeaceOrWarDeclared(IFaction faction1, IFaction faction2)
	{
		if (faction1 == Hero.MainHero.MapFaction || (object)faction1 == Hero.MainHero.Clan || faction2 == Hero.MainHero.MapFaction || (object)faction2 == Hero.MainHero.Clan)
		{
			RefreshRelationsOfNameplates();
		}
	}

	private void OnClanChangeKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomActionDetail detail, bool showNotification)
	{
		RefreshRelationsOfNameplates();
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero previousOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		SettlementNameplateVM value = null;
		if (_allNameplatesBySettlements.TryGetValue(settlement, out value))
		{
			value.RefreshDynamicProperties(forceUpdate: true);
			value.RefreshRelationStatus();
			if ((int)detail == 6)
			{
				value.OnRebelliousClanFormed(newOwner.Clan);
			}
			else if (previousOwner != null && previousOwner.IsRebel)
			{
				value.OnRebelliousClanDisbanded(previousOwner.Clan);
			}
		}
		for (int i = 0; i < ((List<Village>)(object)settlement.BoundVillages).Count; i++)
		{
			Village val = ((List<Village>)(object)settlement.BoundVillages)[i];
			if (_allNameplatesBySettlements.TryGetValue(((SettlementComponent)val).Settlement, out value))
			{
				value.RefreshDynamicProperties(forceUpdate: true);
				value.RefreshRelationStatus();
			}
		}
	}

	private void OnAllianceEnded(Kingdom kingdom1, Kingdom kingdom2)
	{
		OnAllianceStateChanged(kingdom1, kingdom2);
	}

	private void OnAllianceStarted(Kingdom kingdom1, Kingdom kingdom2)
	{
		OnAllianceStateChanged(kingdom1, kingdom2);
	}

	private void OnAllianceStateChanged(Kingdom kingdom1, Kingdom kingdom2)
	{
		if ((object)kingdom1 == Hero.MainHero.MapFaction || (object)kingdom2 == Hero.MainHero.MapFaction)
		{
			RefreshRelationsOfNameplates();
		}
	}

	public SettlementNameplateVM GetNameplateOfSettlement(Settlement settlement)
	{
		if (_allNameplatesBySettlements.TryGetValue(settlement, out var value))
		{
			return value;
		}
		return null;
	}

	public void OnRebelliousClanDisbandedAtSettlement(Settlement settlement, Clan clan)
	{
		if (_allNameplatesBySettlements.TryGetValue(settlement, out var value))
		{
			value.OnRebelliousClanDisbanded(clan);
		}
	}

	public void RefreshRelationsOfNameplates()
	{
		for (int i = 0; i < ((List<SettlementNameplateVM>)(object)_allNameplates).Count; i++)
		{
			((List<SettlementNameplateVM>)(object)_allNameplates)[i].RefreshRelationStatus();
		}
	}

	public void RefreshDynamicPropertiesOfNameplates(bool forceUpdate)
	{
		for (int i = 0; i < ((List<SettlementNameplateVM>)(object)_allNameplates).Count; i++)
		{
			((List<SettlementNameplateVM>)(object)_allNameplates)[i].RefreshDynamicProperties(forceUpdate);
		}
	}
}
