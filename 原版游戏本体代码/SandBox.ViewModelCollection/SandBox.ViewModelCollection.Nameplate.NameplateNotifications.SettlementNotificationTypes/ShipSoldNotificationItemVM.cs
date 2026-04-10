using System;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;

namespace SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;

public class ShipSoldNotificationItemVM : SettlementNotificationItemBaseVM
{
	private int _amount;

	public Ship Ship { get; }

	public PartyBase SettlementParty { get; }

	public PartyBase HeroParty { get; }

	public ShipSoldNotificationItemVM(Action<SettlementNotificationItemBaseVM> onRemove, Ship ship, PartyBase settlementParty, PartyBase heroParty, int amount, int createdTick)
		: base(onRemove, createdTick)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		Ship = ship;
		SettlementParty = settlementParty;
		HeroParty = heroParty;
		_amount = amount;
		base.Text = SandBoxUIHelper.GetShipSoldNotificationText(Ship, Math.Abs(_amount), _amount < 0);
		Hero leaderHero = HeroParty.LeaderHero;
		base.CharacterName = ((leaderHero != null) ? ((object)leaderHero.Name).ToString() : null) ?? ((object)HeroParty.Name).ToString();
		CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(HeroParty);
		if (visualPartyLeader != null)
		{
			base.CharacterVisual = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(visualPartyLeader));
		}
		else if (HeroParty.Owner != null)
		{
			base.CharacterVisual = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(HeroParty.Owner.CharacterObject));
		}
		base.RelationType = 0;
		base.CreatedTick = createdTick;
		if (HeroParty.LeaderHero != null)
		{
			base.RelationType = ((!HeroParty.LeaderHero.Clan.IsAtWarWith((IFaction)(object)Hero.MainHero.Clan)) ? 1 : (-1));
		}
	}

	public void AddNewTransaction(int amount)
	{
		_amount += amount;
		if (_amount == 0)
		{
			ExecuteRemove();
		}
		else
		{
			base.Text = SandBoxUIHelper.GetShipSoldNotificationText(Ship, Math.Abs(_amount), _amount < 0);
		}
	}
}
