using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;

namespace SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;

public class TroopRecruitmentNotificationItemVM : SettlementNotificationItemBaseVM
{
	private int _recruitAmount;

	public Hero RecruiterHero { get; private set; }

	public TroopRecruitmentNotificationItemVM(Action<SettlementNotificationItemBaseVM> onRemove, Hero recruiterHero, int amount, int createdTick)
		: base(onRemove, createdTick)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		base.Text = SandBoxUIHelper.GetRecruitNotificationText(amount);
		_recruitAmount = amount;
		RecruiterHero = recruiterHero;
		base.CharacterName = ((recruiterHero != null) ? ((object)recruiterHero.Name).ToString() : "null hero");
		base.CharacterVisual = ((recruiterHero != null) ? new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(recruiterHero.CharacterObject)) : new CharacterImageIdentifierVM((CharacterCode)null));
		base.RelationType = 0;
		base.CreatedTick = createdTick;
		if (recruiterHero != null)
		{
			base.RelationType = ((!recruiterHero.Clan.IsAtWarWith((IFaction)(object)Hero.MainHero.Clan)) ? 1 : (-1));
		}
	}

	public void AddNewAction(int addedAmount)
	{
		_recruitAmount += addedAmount;
		base.Text = SandBoxUIHelper.GetRecruitNotificationText(_recruitAmount);
	}
}
