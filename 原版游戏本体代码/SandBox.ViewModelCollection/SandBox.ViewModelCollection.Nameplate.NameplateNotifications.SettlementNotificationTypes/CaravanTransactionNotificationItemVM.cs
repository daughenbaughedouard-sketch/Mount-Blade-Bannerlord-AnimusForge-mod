using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;

namespace SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;

public class CaravanTransactionNotificationItemVM : SettlementNotificationItemBaseVM
{
	private List<(EquipmentElement, int)> _items;

	private bool _isSelling;

	public MobileParty CaravanParty { get; private set; }

	public CaravanTransactionNotificationItemVM(Action<SettlementNotificationItemBaseVM> onRemove, MobileParty caravanParty, List<(EquipmentElement, int)> items, int createdTick)
		: base(onRemove, createdTick)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		_items = items;
		List<(EquipmentElement, int)> list = Extensions.DistinctBy<(EquipmentElement, int), EquipmentElement>((IEnumerable<(EquipmentElement, int)>)_items, (Func<(EquipmentElement, int), EquipmentElement>)(((EquipmentElement, int) i) => i.Item1)).ToList();
		if (list.Count < _items.Count)
		{
			_items = list;
		}
		CaravanParty = caravanParty;
		_isSelling = _items.Count > 0 && _items[0].Item2 > 0;
		base.Text = SandBoxUIHelper.GetItemsTradedNotificationText(_items, _isSelling);
		Hero leaderHero = caravanParty.LeaderHero;
		base.CharacterName = ((leaderHero != null) ? ((object)leaderHero.Name).ToString() : null) ?? ((object)caravanParty.Name).ToString();
		if (caravanParty.Party.Owner != null)
		{
			base.CharacterVisual = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(CaravanParty.Party.Owner.CharacterObject));
		}
		else
		{
			CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(CaravanParty.Party);
			if (visualPartyLeader != null)
			{
				base.CharacterVisual = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(visualPartyLeader));
			}
		}
		base.RelationType = 0;
		if (caravanParty != null)
		{
			IFaction mapFaction = caravanParty.MapFaction;
			base.RelationType = ((mapFaction == null || !mapFaction.IsAtWarWith((IFaction)(object)Hero.MainHero.Clan)) ? 1 : (-1));
		}
	}

	public void AddNewItems(List<(EquipmentElement, int)> newItems)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		int i;
		for (i = 0; i < newItems.Count; i++)
		{
			(EquipmentElement, int) tuple = _items.FirstOrDefault(delegate((EquipmentElement, int) t)
			{
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				ref EquipmentElement item = ref t.Item1;
				object obj = newItems[i].Item1;
				return ((object)Unsafe.As<EquipmentElement, EquipmentElement>(ref item)/*cast due to .constrained prefix*/).Equals(obj);
			});
			if (!((EquipmentElement)(ref tuple.Item1)).IsEmpty)
			{
				int index = _items.IndexOf(tuple);
				tuple.Item2 += newItems[i].Item2;
				_items[index] = tuple;
				if (_items[index].Item2 == 0)
				{
					_items.RemoveAt(index);
				}
			}
			else
			{
				_items.Add((newItems[i].Item1, newItems[i].Item2));
			}
		}
		_isSelling = _items.Count > 0 && _items[0].Item2 > 0;
		base.Text = SandBoxUIHelper.GetItemsTradedNotificationText(_items, _isSelling);
	}
}
