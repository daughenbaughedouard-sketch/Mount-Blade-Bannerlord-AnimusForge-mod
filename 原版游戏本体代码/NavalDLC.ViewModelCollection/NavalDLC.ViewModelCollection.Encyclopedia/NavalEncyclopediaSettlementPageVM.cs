using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Encyclopedia;

[EncyclopediaViewModel(typeof(Settlement))]
public class NavalEncyclopediaSettlementPageVM : EncyclopediaSettlementPageVM
{
	private string _shipyardText;

	private BasicTooltipViewModel _shipyardHint;

	[DataSourceProperty]
	public string ShipyardText
	{
		get
		{
			return _shipyardText;
		}
		set
		{
			if (value != _shipyardText)
			{
				_shipyardText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ShipyardText");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel ShipyardHint
	{
		get
		{
			return _shipyardHint;
		}
		set
		{
			if (value != _shipyardHint)
			{
				_shipyardHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "ShipyardHint");
			}
		}
	}

	public NavalEncyclopediaSettlementPageVM(EncyclopediaPageArgs args)
		: base(args)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)


	public override void Refresh()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		((EncyclopediaSettlementPageVM)this).Refresh();
		if (base._settlement.Town?.GetShipyard() == null)
		{
			return;
		}
		TextObject val = default(TextObject);
		bool flag = CampaignUIHelper.IsSettlementInformationHidden(base._settlement, ref val);
		string text = ((object)GameTexts.FindText("str_missing_info_indicator", (string)null)).ToString();
		object shipyardText;
		if (!flag)
		{
			Town town = base._settlement.Town;
			if (town == null)
			{
				shipyardText = null;
			}
			else
			{
				Building shipyard = town.GetShipyard();
				shipyardText = ((shipyard != null) ? shipyard.CurrentLevel.ToString() : null);
			}
		}
		else
		{
			shipyardText = text;
		}
		ShipyardText = (string)shipyardText;
		ShipyardHint = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => NavalUIHelper.GetShipyardTooltip(base._settlement.Town)));
		for (int num = 0; num < ((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).LeftSideProperties).Count; num++)
		{
			if (((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).LeftSideProperties)[num].TypeString == "Wall")
			{
				EncyclopediaSettlementPageStatItemVM item = ((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).LeftSideProperties)[((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).LeftSideProperties).Count - 1];
				((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).LeftSideProperties).Remove(item);
				((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).RightSideProperties).Insert(0, item);
				((Collection<EncyclopediaSettlementPageStatItemVM>)(object)((EncyclopediaSettlementPageVM)this).LeftSideProperties).Insert(num + 1, new EncyclopediaSettlementPageStatItemVM(ShipyardHint, (DescriptionType)1, ShipyardText));
				break;
			}
		}
	}
}
