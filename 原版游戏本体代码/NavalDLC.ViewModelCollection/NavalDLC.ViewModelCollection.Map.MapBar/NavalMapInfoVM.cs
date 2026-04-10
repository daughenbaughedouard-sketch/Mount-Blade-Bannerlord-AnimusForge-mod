using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapBar;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Map.MapBar;

public class NavalMapInfoVM : MapInfoVM
{
	private MapInfoItemVM _shipHealthInfo;

	private string _invalidShipHealthText;

	private readonly ShipHealthPercentageComparer _shipHealthPercentageComparer = new ShipHealthPercentageComparer();

	public NavalMapInfoVM()
	{
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((MapInfoVM)this).RefreshValues();
		_invalidShipHealthText = ((object)new TextObject("{=4NaOKslb}-", (Dictionary<string, object>)null)).ToString();
	}

	protected override void CreateItems()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		((MapInfoVM)this).CreateItems();
		_shipHealthInfo = new MapInfoItemVM("ship_health", (Func<List<TooltipProperty>>)GetShipTooltip);
		((Collection<MapInfoItemVM>)(object)((MapInfoVM)this).PrimaryInfoItems).Insert(2, _shipHealthInfo);
	}

	protected override void UpdatePlayerInfo(bool updateForced)
	{
		((MapInfoVM)this).UpdatePlayerInfo(updateForced);
		MobileParty mainParty = MobileParty.MainParty;
		if (((mainParty != null) ? mainParty.Ships : null) == null || ((List<Ship>)(object)MobileParty.MainParty.Ships).Count == 0)
		{
			_shipHealthInfo.Value = _invalidShipHealthText;
			return;
		}
		float num = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).Average((Ship s) => s.GetHealthPercent());
		_shipHealthInfo.HasWarning = num < 20f;
		if (_shipHealthInfo.FloatValue != num)
		{
			_shipHealthInfo.Value = ((object)GameTexts.FindText("str_NUMBER_percent", (string)null).SetTextVariable("NUMBER", MathF.Ceiling(num).ToString())).ToString();
		}
	}

	private List<TooltipProperty> GetShipTooltip()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		MobileParty mainParty = MobileParty.MainParty;
		if (((mainParty != null) ? mainParty.Ships : null) == null || ((List<Ship>)(object)MobileParty.MainParty.Ships).Count == 0)
		{
			return new List<TooltipProperty>
			{
				new TooltipProperty("", ((object)new TextObject("{=lb2hbQyx}You don't have any ships", (Dictionary<string, object>)null)).ToString(), 0, false, (TooltipPropertyFlags)0)
			};
		}
		List<TooltipProperty> list = new List<TooltipProperty>();
		float num = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).Average((Ship s) => s.GetHealthPercent());
		list.Add(new TooltipProperty(((object)new TextObject("{=oTM78wf6}Fleet Condition", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("str_NUMBER_percent", (string)null).SetTextVariable("NUMBER", MathF.Ceiling(num).ToString())).ToString(), 0, false, (TooltipPropertyFlags)4096));
		List<Ship> list2 = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).ToList();
		list2.Sort(_shipHealthPercentageComparer);
		foreach (Ship item in list2)
		{
			string text = ((object)GameTexts.FindText("str_NUMBER_percent", (string)null).SetTextVariable("NUMBER", MathF.Ceiling(item.GetHealthPercent()).ToString())).ToString();
			list.Add(new TooltipProperty(((object)item.Name).ToString(), text, 0, false, (TooltipPropertyFlags)0));
		}
		return list;
	}
}
