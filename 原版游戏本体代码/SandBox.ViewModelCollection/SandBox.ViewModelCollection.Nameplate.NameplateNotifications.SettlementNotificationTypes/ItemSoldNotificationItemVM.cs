using System;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;

namespace SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;

public class ItemSoldNotificationItemVM : SettlementNotificationItemBaseVM
{
	private int _number;

	private PartyBase _heroParty;

	public ItemRosterElement Item { get; }

	public PartyBase ReceiverParty { get; }

	public PartyBase PayerParty { get; }

	public ItemSoldNotificationItemVM(Action<SettlementNotificationItemBaseVM> onRemove, PartyBase receiverParty, PartyBase payerParty, ItemRosterElement item, int number, int createdTick)
		: base(onRemove, createdTick)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		Item = item;
		ReceiverParty = receiverParty;
		PayerParty = payerParty;
		_number = number;
		_heroParty = (receiverParty.IsSettlement ? payerParty : receiverParty);
		base.Text = SandBoxUIHelper.GetItemSoldNotificationText(Item, _number, _number < 0);
		base.CharacterName = ((_heroParty.LeaderHero != null) ? ((object)_heroParty.LeaderHero.Name).ToString() : ((object)_heroParty.Name).ToString());
		CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(_heroParty);
		base.CharacterVisual = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(visualPartyLeader));
		base.RelationType = 0;
		base.CreatedTick = createdTick;
		if (_heroParty.LeaderHero != null)
		{
			base.RelationType = ((!_heroParty.LeaderHero.Clan.IsAtWarWith((IFaction)(object)Hero.MainHero.Clan)) ? 1 : (-1));
		}
	}

	public void AddNewTransaction(int amount)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		_number += amount;
		if (_number == 0)
		{
			ExecuteRemove();
		}
		else
		{
			base.Text = SandBoxUIHelper.GetItemSoldNotificationText(Item, _number, _number < 0);
		}
	}
}
