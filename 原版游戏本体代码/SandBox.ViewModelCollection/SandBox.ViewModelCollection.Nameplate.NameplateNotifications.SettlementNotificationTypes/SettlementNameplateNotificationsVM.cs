using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;

public class SettlementNameplateNotificationsVM : ViewModel
{
	private readonly Settlement _settlement;

	private int _tickSinceEnabled;

	private const int _maxTickDeltaToCongregate = 10;

	private MBBindingList<SettlementNotificationItemBaseVM> _notifications;

	public bool IsEventsRegistered { get; private set; }

	public MBBindingList<SettlementNotificationItemBaseVM> Notifications
	{
		get
		{
			return _notifications;
		}
		set
		{
			if (value != _notifications)
			{
				_notifications = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SettlementNotificationItemBaseVM>>(value, "Notifications");
			}
		}
	}

	public SettlementNameplateNotificationsVM(Settlement settlement)
	{
		_settlement = settlement;
		Notifications = new MBBindingList<SettlementNotificationItemBaseVM>();
	}

	public void Tick()
	{
		_tickSinceEnabled++;
	}

	private void OnTroopRecruited(Hero recruiterHero, Settlement settlement, Hero troopSource, CharacterObject troop, int amount)
	{
		if (amount > 0 && settlement == _settlement && _settlement.IsInspected && recruiterHero != null && (recruiterHero.CurrentSettlement == _settlement || (recruiterHero.PartyBelongedTo != null && recruiterHero.PartyBelongedTo.LastVisitedSettlement == _settlement)))
		{
			TroopRecruitmentNotificationItemVM updatableNotificationByPredicate = GetUpdatableNotificationByPredicate((TroopRecruitmentNotificationItemVM n) => n.RecruiterHero == recruiterHero);
			if (updatableNotificationByPredicate != null)
			{
				updatableNotificationByPredicate.AddNewAction(amount);
			}
			else
			{
				((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new TroopRecruitmentNotificationItemVM(RemoveItem, recruiterHero, amount, _tickSinceEnabled));
			}
		}
	}

	private void OnCaravanTransactionCompleted(MobileParty caravanParty, Town town, List<(EquipmentElement, int)> items)
	{
		if (_settlement == ((SettlementComponent)town).Owner.Settlement)
		{
			CaravanTransactionNotificationItemVM updatableNotificationByPredicate = GetUpdatableNotificationByPredicate((CaravanTransactionNotificationItemVM n) => n.CaravanParty == caravanParty);
			if (updatableNotificationByPredicate != null)
			{
				updatableNotificationByPredicate.AddNewItems(items);
			}
			else
			{
				((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new CaravanTransactionNotificationItemVM(RemoveItem, caravanParty, items, _tickSinceEnabled));
			}
		}
	}

	private void OnPrisonerSold(PartyBase sellerParty, PartyBase buyerParty, TroopRoster prisoners)
	{
		if (sellerParty.IsMobile && buyerParty != null && buyerParty.IsSettlement && buyerParty.Settlement == _settlement && _settlement.IsInspected && prisoners.Count > 0 && sellerParty.LeaderHero != null)
		{
			MobileParty sellerMobileParty = sellerParty.MobileParty;
			PrisonerSoldNotificationItemVM updatableNotificationByPredicate = GetUpdatableNotificationByPredicate((PrisonerSoldNotificationItemVM n) => n.Party == sellerMobileParty);
			if (updatableNotificationByPredicate != null)
			{
				updatableNotificationByPredicate.AddNewPrisoners(prisoners);
			}
			else
			{
				((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new PrisonerSoldNotificationItemVM(RemoveItem, sellerMobileParty, prisoners, _tickSinceEnabled));
			}
		}
	}

	private void OnTroopGivenToSettlement(Hero giverHero, Settlement givenSettlement, TroopRoster givenTroops)
	{
		if (_settlement.IsInspected && givenTroops.TotalManCount > 0 && giverHero != null && givenSettlement == _settlement)
		{
			TroopGivenToSettlementNotificationItemVM updatableNotificationByPredicate = GetUpdatableNotificationByPredicate((TroopGivenToSettlementNotificationItemVM n) => n.GiverHero == giverHero);
			if (updatableNotificationByPredicate != null)
			{
				updatableNotificationByPredicate.AddNewAction(givenTroops);
			}
			else
			{
				((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new TroopGivenToSettlementNotificationItemVM(RemoveItem, giverHero, givenTroops, _tickSinceEnabled));
			}
		}
	}

	private void OnItemSold(PartyBase receiverParty, PartyBase payerParty, ItemRosterElement item, int number, Settlement currentSettlement)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (_settlement.IsInspected && number > 0 && currentSettlement == _settlement)
		{
			int num = ((!receiverParty.IsSettlement) ? 1 : (-1));
			ItemSoldNotificationItemVM updatableNotificationByPredicate = GetUpdatableNotificationByPredicate(delegate(ItemSoldNotificationItemVM n)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				ItemRosterElement item2 = n.Item;
				EquipmentElement equipmentElement = ((ItemRosterElement)(ref item2)).EquipmentElement;
				ItemObject item3 = ((EquipmentElement)(ref equipmentElement)).Item;
				equipmentElement = ((ItemRosterElement)(ref item)).EquipmentElement;
				return item3 == ((EquipmentElement)(ref equipmentElement)).Item && (n.PayerParty == receiverParty || n.PayerParty == payerParty);
			});
			if (updatableNotificationByPredicate != null)
			{
				updatableNotificationByPredicate.AddNewTransaction(number * num);
			}
			else
			{
				((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new ItemSoldNotificationItemVM(RemoveItem, receiverParty, payerParty, item, number * num, _tickSinceEnabled));
			}
		}
	}

	private void OnIssueUpdated(IssueBase issue, IssueUpdateDetails updateType, Hero relatedHero)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)updateType == 7 && relatedHero != null && relatedHero.CurrentSettlement == _settlement)
		{
			((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new IssueSolvedByLordNotificationItemVM(RemoveItem, relatedHero, _tickSinceEnabled));
		}
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase oldOwner, ShipOwnerChangeDetail detail)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (_settlement.IsInspected && (int)detail == 0 && (oldOwner == _settlement.Party || ship.Owner == _settlement.Party))
		{
			bool flag = ship.Owner == _settlement.Party;
			int amount = ((!flag) ? 1 : (-1));
			PartyBase heroParty = (flag ? oldOwner : ship.Owner);
			ShipSoldNotificationItemVM updatableNotificationByPredicate = GetUpdatableNotificationByPredicate((ShipSoldNotificationItemVM n) => n.Ship.ShipHull.Name == ship.ShipHull.Name && n.SettlementParty == _settlement.Party && n.HeroParty == heroParty);
			if (updatableNotificationByPredicate != null)
			{
				updatableNotificationByPredicate.AddNewTransaction(amount);
			}
			else
			{
				((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Add((SettlementNotificationItemBaseVM)new ShipSoldNotificationItemVM(RemoveItem, ship, _settlement.Party, heroParty, amount, _tickSinceEnabled));
			}
		}
	}

	private void RemoveItem(SettlementNotificationItemBaseVM item)
	{
		((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Remove(item);
	}

	public void RegisterEvents()
	{
		if (!IsEventsRegistered)
		{
			CampaignEvents.OnTroopRecruitedEvent.AddNonSerializedListener((object)this, (Action<Hero, Settlement, Hero, CharacterObject, int>)OnTroopRecruited);
			CampaignEvents.OnPrisonerSoldEvent.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase, TroopRoster>)OnPrisonerSold);
			CampaignEvents.OnCaravanTransactionCompletedEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Town, List<(EquipmentElement, int)>>)OnCaravanTransactionCompleted);
			CampaignEvents.OnTroopGivenToSettlementEvent.AddNonSerializedListener((object)this, (Action<Hero, Settlement, TroopRoster>)OnTroopGivenToSettlement);
			CampaignEvents.OnItemSoldEvent.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase, ItemRosterElement, int, Settlement>)OnItemSold);
			CampaignEvents.OnIssueUpdatedEvent.AddNonSerializedListener((object)this, (Action<IssueBase, IssueUpdateDetails, Hero>)OnIssueUpdated);
			CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
			_tickSinceEnabled = 0;
			IsEventsRegistered = true;
		}
	}

	public void UnloadEvents()
	{
		if (IsEventsRegistered)
		{
			((IMbEventBase)CampaignEvents.OnTroopRecruitedEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnItemSoldEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnPrisonerSoldEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnCaravanTransactionCompletedEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnTroopGivenToSettlementEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnIssueUpdatedEvent).ClearListeners((object)this);
			((IMbEventBase)CampaignEvents.OnShipOwnerChangedEvent).ClearListeners((object)this);
			_tickSinceEnabled = 0;
			IsEventsRegistered = false;
		}
	}

	public bool IsValidItemForNotification(ItemRosterElement item)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected I4, but got Unknown
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref item)).EquipmentElement;
		ItemTypeEnum type = ((EquipmentElement)(ref equipmentElement)).Item.Type;
		switch ((int)type)
		{
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 23:
		case 24:
		case 25:
			return true;
		default:
			return false;
		}
	}

	private T GetUpdatableNotificationByPredicate<T>(Func<T, bool> predicate) where T : SettlementNotificationItemBaseVM
	{
		for (int i = 0; i < ((Collection<SettlementNotificationItemBaseVM>)(object)Notifications).Count; i++)
		{
			SettlementNotificationItemBaseVM settlementNotificationItemBaseVM = ((Collection<SettlementNotificationItemBaseVM>)(object)Notifications)[i];
			if (_tickSinceEnabled - settlementNotificationItemBaseVM.CreatedTick < 10 && settlementNotificationItemBaseVM is T val && predicate(val))
			{
				return val;
			}
		}
		return null;
	}
}
