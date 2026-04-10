using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NavalDLC.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;

namespace NavalDLC.ViewModelCollection.Kingdom;

public class NavalKingdomSettlementItemVM : KingdomSettlementItemVM
{
	public NavalKingdomSettlementItemVM(Settlement settlement, Action<KingdomSettlementItemVM> onSelect)
		: base(settlement, onSelect)
	{
	}

	protected override void UpdateProperties()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		((KingdomSettlementItemVM)this).UpdateProperties();
		Building val = base.Settlement.Town?.GetShipyard();
		if (val != null)
		{
			BasicTooltipViewModel val2 = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => NavalUIHelper.GetShipyardTooltip(base.Settlement.Town)));
			int currentLevel = val.CurrentLevel;
			((Collection<SelectableFiefItemPropertyVM>)(object)((KingdomSettlementItemVM)this).ItemProperties).Insert(1, new SelectableFiefItemPropertyVM(((object)GameTexts.FindText("str_shipyard", (string)null)).ToString(), currentLevel.ToString(), 0, (PropertyType)8, val2, false));
		}
		if (base.Settlement.IsTown && base.Settlement.HasPort)
		{
			BasicTooltipViewModel val3 = new BasicTooltipViewModel((Func<string>)(() => NavalUIHelper.GetTownCoastalPatrolTooltip(base.Settlement.Town)));
			((Collection<SelectableFiefItemPropertyVM>)(object)((KingdomSettlementItemVM)this).ItemProperties).Add(new SelectableFiefItemPropertyVM(((object)GameTexts.FindText("str_coastal_patrol", (string)null)).ToString(), ((object)Campaign.Current.GetCampaignBehavior<INavalPatrolPartiesCampaignBehavior>().GetSettlementPatrolStatus(base.Settlement)).ToString(), 0, (PropertyType)10, val3, false));
		}
	}
}
