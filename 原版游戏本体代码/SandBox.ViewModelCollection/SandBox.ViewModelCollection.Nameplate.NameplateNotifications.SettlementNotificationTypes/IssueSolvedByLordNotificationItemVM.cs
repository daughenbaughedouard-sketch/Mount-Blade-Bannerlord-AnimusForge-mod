using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;

public class IssueSolvedByLordNotificationItemVM : SettlementNotificationItemBaseVM
{
	public IssueSolvedByLordNotificationItemVM(Action<SettlementNotificationItemBaseVM> onRemove, Hero hero, int createdTick)
		: base(onRemove, createdTick)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		base.Text = ((object)new TextObject("{=TFJTOYea}Solved an issue", (Dictionary<string, object>)null)).ToString();
		base.CharacterName = ((hero == null) ? null : ((object)hero.Name)?.ToString()) ?? "";
		base.CharacterVisual = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(hero.CharacterObject));
		base.RelationType = 0;
		base.CreatedTick = createdTick;
		int relationType;
		if (hero != null)
		{
			Clan clan = hero.Clan;
			if (((clan != null) ? new bool?(clan.IsAtWarWith((IFaction)(object)Hero.MainHero.Clan)) : ((bool?)null)) == true)
			{
				relationType = -1;
				goto IL_00b2;
			}
		}
		relationType = 1;
		goto IL_00b2;
		IL_00b2:
		base.RelationType = relationType;
	}
}
