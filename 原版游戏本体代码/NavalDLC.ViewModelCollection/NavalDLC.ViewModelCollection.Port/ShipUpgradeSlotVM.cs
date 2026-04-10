using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipUpgradeSlotVM : ShipUpgradeSlotBaseVM
{
	private readonly ShipUpgradePiece _initialSelectedPiece;

	public ShipUpgradeSlotVM(Ship ship, TextObject slotName, string shipSlotTag, string slotTypeId, Action<ShipUpgradeSlotBaseVM> onSelected)
		: base(ship, slotName, shipSlotTag, slotTypeId, onSelected)
	{
		_initialSelectedPiece = Ship.GetPieceAtSlot(ShipSlotTag);
		List<ShipUpgradePiece> list = ((IEnumerable<ShipUpgradePiece>)MBObjectManager.Instance.GetObjectTypeList<ShipUpgradePiece>()).Where((ShipUpgradePiece x) => !x.NotMerchandise && x.DoesPieceMatchSlot(Ship.ShipHull.AvailableSlots[ShipSlotTag])).ToList();
		List<ShipUpgradePiece> list2 = new List<ShipUpgradePiece>();
		if (Settlement.CurrentSettlement?.Town != null)
		{
			list2 = (from x in Settlement.CurrentSettlement.Town.GetAvailableShipUpgradePieces()
				where x.DoesPieceMatchSlot(Ship.ShipHull.AvailableSlots[ShipSlotTag])
				select x).ToList();
		}
		if (_initialSelectedPiece != null && !list.Contains(_initialSelectedPiece))
		{
			list.Add(_initialSelectedPiece);
		}
		if (Ship.UnlockedUpgradePieces != null)
		{
			foreach (ShipUpgradePiece item in (List<ShipUpgradePiece>)(object)Ship.UnlockedUpgradePieces)
			{
				if (item.DoesPieceMatchSlot(Ship.ShipHull.AvailableSlots[ShipSlotTag]) && !list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		if (_initialSelectedPiece != null && !list2.Contains(_initialSelectedPiece))
		{
			list2.Add(_initialSelectedPiece);
		}
		if (Ship.UnlockedUpgradePieces != null)
		{
			foreach (ShipUpgradePiece item2 in (List<ShipUpgradePiece>)(object)Ship.UnlockedUpgradePieces)
			{
				if (item2.DoesPieceMatchSlot(Ship.ShipHull.AvailableSlots[ShipSlotTag]) && !list2.Contains(item2))
				{
					list2.Add(item2);
				}
			}
		}
		_ = Ship.ShipHull.AvailableSlots[ShipSlotTag];
		for (int num = 0; num < list.Count; num++)
		{
			ShipUpgradePiece val = list[num];
			ShipUpgradePieceVM shipUpgradePieceVM = new ShipUpgradePieceVM(val, Ship, OnPieceSelected)
			{
				IsDisabled = !list2.Contains(val)
			};
			((Collection<ShipUpgradePieceBaseVM>)(object)base.AvailablePieces).Add((ShipUpgradePieceBaseVM)shipUpgradePieceVM);
			if (val == _initialSelectedPiece)
			{
				base.SelectedPiece = shipUpgradePieceVM;
			}
		}
		base.AvailablePieces.Sort((IComparer<ShipUpgradePieceBaseVM>)new UpgradePieceComparer());
		UpdateAnyBetterPiecesAvailable();
	}

	public override void ResetPieces()
	{
		base.SelectedPiece = ((IEnumerable<ShipUpgradePieceBaseVM>)base.AvailablePieces).FirstOrDefault((ShipUpgradePieceBaseVM x) => (x as ShipUpgradePieceVM)?.Piece == _initialSelectedPiece);
		base.IsChanged = false;
	}

	protected override bool GetIsChanged()
	{
		return (base.SelectedPiece as ShipUpgradePieceVM)?.Piece != _initialSelectedPiece;
	}
}
