using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Overlay;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace NavalDLC.ViewModelCollection.Map;

[MenuOverlay("SettlementMenuOverlay")]
public class NavalSettlementMenuOverlayVM : SettlementMenuOverlayVM
{
	public NavalSettlementMenuOverlayVM(MenuOverlayType type)
		: base(type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		((SettlementMenuOverlayVM)this).ShipyardHint = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => NavalUIHelper.GetShipyardTooltip(base._settlement.Town)));
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((SettlementMenuOverlayVM)this).RefreshValues();
		Building val = base._settlement.Town?.GetShipyard();
		((SettlementMenuOverlayVM)this).IsShipyardEnabled = val != null;
		((SettlementMenuOverlayVM)this).ShipyardLbl = (((SettlementMenuOverlayVM)this).IsShipyardEnabled ? val.CurrentLevel.ToString() : string.Empty);
	}
}
