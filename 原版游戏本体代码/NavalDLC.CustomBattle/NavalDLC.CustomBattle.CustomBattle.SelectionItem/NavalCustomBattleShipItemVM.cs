using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CustomBattle.CustomBattle.SelectionItem;

public class NavalCustomBattleShipItemVM : NavalCustomBattleShipHullItemVM
{
	private readonly Action _onUpgraded;

	private int _tier;

	private HintViewModel _cycleTierHint;

	public CustomBattleShip Ship { get; private set; }

	[DataSourceProperty]
	public int Tier
	{
		get
		{
			return _tier;
		}
		set
		{
			if (value != _tier)
			{
				_tier = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Tier");
				OnTierSelection();
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CycleTierHint
	{
		get
		{
			return _cycleTierHint;
		}
		set
		{
			if (value != _cycleTierHint)
			{
				_cycleTierHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CycleTierHint");
			}
		}
	}

	public NavalCustomBattleShipItemVM(ShipHull shipHull, bool isPlayerShip, Action onUpgraded)
		: base(shipHull, null)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		Ship = new CustomBattleShip(ShipHull, isPlayerShip);
		_onUpgraded = onUpgraded;
		CycleTierHint = new HintViewModel(new TextObject("{=zbkzFaWE}Change upgrade tier", (Dictionary<string, object>)null), (string)null);
	}

	public void ExecuteCycleUpgradeTier()
	{
		Tier = (Tier + 1) % 4;
	}

	public void RandomizeUpgrades()
	{
		Tier = MBRandom.RandomInt(0, 4);
	}

	private void OnTierSelection()
	{
		if (Tier == 0)
		{
			foreach (KeyValuePair<string, ShipSlot> availableSlot in ShipHull.AvailableSlots)
			{
				Ship.SetPieceAtSlot(availableSlot.Key, null);
			}
		}
		else
		{
			IEnumerable<ShipUpgradePiece> source = ((IEnumerable<ShipUpgradePiece>)MBObjectManager.Instance.GetObjectTypeList<ShipUpgradePiece>()).Where((ShipUpgradePiece x) => !x.NotMerchandise);
			IEnumerable<ShipUpgradePiece> source2 = source.Where((ShipUpgradePiece x) => x.RequiredPortLevel == Tier);
			foreach (KeyValuePair<string, ShipSlot> slot in ShipHull.AvailableSlots)
			{
				ShipUpgradePiece upgradePiece = ((source2.Count() != 0) ? Extensions.GetRandomElementInefficiently<ShipUpgradePiece>(source2.Where((ShipUpgradePiece x) => x.DoesPieceMatchSlot(slot.Value))) : Extensions.GetRandomElementInefficiently<ShipUpgradePiece>(source.Where((ShipUpgradePiece x) => x.RequiredPortLevel <= Tier && x.DoesPieceMatchSlot(slot.Value))));
				Ship.SetPieceAtSlot(slot.Key, upgradePiece);
			}
		}
		_onUpgraded?.Invoke();
	}

	protected override List<TooltipProperty> GetTooltip()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>
		{
			new TooltipProperty(base.Name.ToString(), string.Empty, 0, false, (TooltipPropertyFlags)4096),
			new TooltipProperty(((object)new TextObject("{=sqdzHOPe}Class", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("str_ship_type", ((object)ShipHull.Type/*cast due to .constrained prefix*/).ToString().ToLowerInvariant())).ToString(), 0, false, (TooltipPropertyFlags)0),
			new TooltipProperty(((object)new TextObject("{=UbZL2BJQ}Hitpoints", (Dictionary<string, object>)null)).ToString(), ((int)Ship.MaxHitPoints).ToString(), 0, false, (TooltipPropertyFlags)0)
		};
		int num = Ship.TotalCrewCapacity - Ship.MainDeckCrewCapacity;
		string text = ((num <= 0) ? Ship.TotalCrewCapacity.ToString() : ((object)new TextObject("{=r2fvxfwZ}{TOTAL} ({MAIN_DECK}+{RESERVE})", (Dictionary<string, object>)null).SetTextVariable("TOTAL", Ship.TotalCrewCapacity.ToString()).SetTextVariable("MAIN_DECK", Ship.MainDeckCrewCapacity.ToString()).SetTextVariable("RESERVE", num.ToString())).ToString());
		list.Add(new TooltipProperty(((object)new TextObject("{=oqVVGxgb}Crew Capacity", (Dictionary<string, object>)null)).ToString(), text, 0, false, (TooltipPropertyFlags)0));
		List<ShipSlotAndPieceName> shipSlotAndPieceNames = Ship.GetShipSlotAndPieceNames();
		if (shipSlotAndPieceNames.Count > 0)
		{
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024));
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=zMvUzdKR}Ship Upgrades", (Dictionary<string, object>)null)).ToString(), -1, false, (TooltipPropertyFlags)0));
			foreach (ShipSlotAndPieceName item in shipSlotAndPieceNames)
			{
				list.Add(new TooltipProperty(item.SlotName, item.PieceName, 0, false, (TooltipPropertyFlags)0));
			}
		}
		return list;
	}
}
