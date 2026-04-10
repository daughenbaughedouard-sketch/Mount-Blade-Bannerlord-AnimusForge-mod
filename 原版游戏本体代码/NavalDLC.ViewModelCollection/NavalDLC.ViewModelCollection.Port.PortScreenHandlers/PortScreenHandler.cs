using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port.PortScreenHandlers;

public abstract class PortScreenHandler
{
	public readonly struct ShipUpgradePieceInfo
	{
		public readonly Ship Ship;

		public readonly string ShipSlotTag;

		public readonly ShipUpgradePiece Piece;

		public ShipUpgradePieceInfo(Ship ship, string shipSlotTag, ShipUpgradePiece piece)
		{
			Ship = ship;
			ShipSlotTag = shipSlotTag;
			Piece = piece;
		}
	}

	public readonly struct ShipFigureheadInfo
	{
		public readonly Ship Ship;

		public readonly Figurehead Figurehead;

		public ShipFigureheadInfo(Ship ship, Figurehead figurehead)
		{
			Ship = ship;
			Figurehead = figurehead;
		}
	}

	public readonly struct ShipRenameInfo
	{
		public readonly Ship Ship;

		public readonly string NewName;

		public ShipRenameInfo(Ship ship, string newName)
		{
			Ship = ship;
			NewName = newName;
		}
	}

	public readonly struct ShipTradeInfo
	{
		public readonly Ship Ship;

		public readonly int Price;

		public ShipTradeInfo(Ship ship, int price)
		{
			Ship = ship;
			Price = price;
		}
	}

	protected MBReadOnlyList<Ship> _initialLeftShips;

	protected MBReadOnlyList<Ship> _initialRightShips;

	private MBList<Ship> _leftShips;

	private MBList<Ship> _rightShips;

	private MBList<ShipTradeInfo> _shipsToBuy;

	private MBList<ShipTradeInfo> _shipsToSell;

	private MBList<Ship> _shipsToRepair;

	private MBList<Ship> _shipsToSend;

	private MBList<ShipRenameInfo> _shipsToRename;

	private MBList<ShipUpgradePieceInfo> _selectedShipPieces;

	private MBList<ShipFigureheadInfo> _selectedFigureheads;

	public MBReadOnlyList<Ship> LeftShips => (MBReadOnlyList<Ship>)(object)_leftShips;

	public MBReadOnlyList<Ship> RightShips => (MBReadOnlyList<Ship>)(object)_rightShips;

	public MBReadOnlyList<ShipTradeInfo> ShipsToBuy => (MBReadOnlyList<ShipTradeInfo>)(object)_shipsToBuy;

	public MBReadOnlyList<ShipTradeInfo> ShipsToSell => (MBReadOnlyList<ShipTradeInfo>)(object)_shipsToSell;

	public MBReadOnlyList<Ship> ShipsToRepair => (MBReadOnlyList<Ship>)(object)_shipsToRepair;

	public MBReadOnlyList<Ship> ShipsToSend => (MBReadOnlyList<Ship>)(object)_shipsToSend;

	public MBReadOnlyList<ShipRenameInfo> ShipsToRename => (MBReadOnlyList<ShipRenameInfo>)(object)_shipsToRename;

	public MBReadOnlyList<ShipUpgradePieceInfo> SelectedShipPieces => (MBReadOnlyList<ShipUpgradePieceInfo>)(object)_selectedShipPieces;

	public MBReadOnlyList<ShipFigureheadInfo> SelectedFigureheads => (MBReadOnlyList<ShipFigureheadInfo>)(object)_selectedFigureheads;

	public PortScreenHandler(MBReadOnlyList<Ship> initialLeftShips, MBReadOnlyList<Ship> initialRightShips)
	{
		_initialLeftShips = initialLeftShips;
		_initialRightShips = initialRightShips;
		_leftShips = new MBList<Ship>((List<Ship>)(object)_initialLeftShips);
		_rightShips = new MBList<Ship>((List<Ship>)(object)_initialRightShips);
		_shipsToBuy = new MBList<ShipTradeInfo>();
		_shipsToSell = new MBList<ShipTradeInfo>();
		_shipsToRepair = new MBList<Ship>();
		_shipsToRename = new MBList<ShipRenameInfo>();
		_shipsToSend = new MBList<Ship>();
		_selectedShipPieces = new MBList<ShipUpgradePieceInfo>();
		_selectedFigureheads = new MBList<ShipFigureheadInfo>();
	}

	public abstract TextObject GetLeftRosterName();

	public abstract TextObject GetRightRosterName();

	public abstract PartyBase GetLeftSideOwnerParty();

	public abstract PartyBase GetRightSideOwnerParty();

	public PortActionInfo GetCanBuyShip(Ship ship)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		if (!((List<Ship>)(object)LeftShips).Contains(ship))
		{
			return PortActionInfo.CreateInvalid();
		}
		PortActionInfo result = CanBuyShip(ship);
		if (result.IsRelevant && (!ship.IsTradeable || ship.IsUsedByQuest))
		{
			return PortActionInfo.CreateValid(isEnabled: false, 0, result.ActionName, new TextObject("{=pWd0AQm8}You cannot buy this ship", (Dictionary<string, object>)null));
		}
		return result;
	}

	public PortActionInfo GetCanSellShip(Ship ship)
	{
		if (!((List<Ship>)(object)RightShips).Contains(ship))
		{
			return PortActionInfo.CreateInvalid();
		}
		PortActionInfo result = CanSellShip(ship);
		if (result.IsRelevant && (!ship.IsTradeable || ship.IsUsedByQuest))
		{
			return PortActionInfo.CreateValid(isEnabled: false, 0, result.ActionName, GameTexts.FindText("str_port_cant_take_action_quest_ship", (string)null));
		}
		return result;
	}

	public PortActionInfo GetCanRepairShip(Ship ship)
	{
		if (!((List<Ship>)(object)RightShips).Contains(ship) || ship.HitPoints >= ship.MaxHitPoints)
		{
			return PortActionInfo.CreateInvalid();
		}
		return CanRepairShip(ship);
	}

	public PortActionInfo GetCanRepairAll(Ship selectedShip)
	{
		if (!((List<Ship>)(object)RightShips).Contains(selectedShip) || ((List<Ship>)(object)RightShips).TrueForAll((Predicate<Ship>)((Ship ship) => ship.HitPoints >= ship.MaxHitPoints)))
		{
			return PortActionInfo.CreateInvalid();
		}
		return CanRepairAll();
	}

	public PortActionInfo GetCanUpgradeShip(Ship ship)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (!((List<Ship>)(object)RightShips).Contains(ship))
		{
			return PortActionInfo.CreateInvalid(new TextObject("{=hlBSanaL}You can't upgrade ships that don't belong to you", (Dictionary<string, object>)null));
		}
		if (ship.HitPoints < ship.MaxHitPoints && !((List<Ship>)(object)ShipsToRepair).Contains(ship))
		{
			return PortActionInfo.CreateInvalid(new TextObject("{=8KEmXkaT}You can't upgrade ships that need repairs", (Dictionary<string, object>)null));
		}
		return CanUpgradeShip(ship);
	}

	public PortActionInfo GetCanRenameShip(Ship ship)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (!((List<Ship>)(object)RightShips).Contains(ship))
		{
			return PortActionInfo.CreateInvalid(new TextObject("{=NmWkD50x}You can't rename ships that don't belong to you", (Dictionary<string, object>)null));
		}
		return CanRenameShip(ship);
	}

	public PortActionInfo GetCanSendToClan(Ship ship)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (!((List<Ship>)(object)RightShips).Contains(ship))
		{
			return PortActionInfo.CreateInvalid();
		}
		PortActionInfo result = CanSendToClan(ship);
		if (result.IsRelevant && ((List<Ship>)(object)RightShips).Count == 1)
		{
			return PortActionInfo.CreateValid(isEnabled: false, 0, result.ActionName, new TextObject("{=DSoB9VCu}You can't send your only ship to your clan", (Dictionary<string, object>)null));
		}
		return result;
	}

	public abstract int GetTradeCostOfShip(Ship ship, bool isRightSideSelling);

	public abstract int GetRepairCostOfShip(Ship ship, bool isRightSideRepairing);

	public abstract int GetUpgradeCostOfShip(Ship ship, ShipUpgradePiece piece, bool isRightSideUpgrading);

	public abstract int GetTotalGoldCost();

	public abstract bool GetCanConfirm(out TextObject disabledHint);

	public abstract void OnConfirmChanges();

	public abstract List<PortChangeInfo> GetChanges();

	protected abstract PortActionInfo CanBuyShip(Ship ship);

	protected abstract PortActionInfo CanSellShip(Ship ship);

	protected abstract PortActionInfo CanRepairShip(Ship ship);

	protected abstract PortActionInfo CanRepairAll();

	protected abstract PortActionInfo CanUpgradeShip(Ship ship);

	protected abstract PortActionInfo CanRenameShip(Ship ship);

	protected abstract PortActionInfo CanSendToClan(Ship ship);

	public virtual bool AreThereAnyChanges()
	{
		if (((List<ShipTradeInfo>)(object)ShipsToBuy).Count <= 0 && ((List<ShipTradeInfo>)(object)ShipsToSell).Count <= 0 && ((List<Ship>)(object)ShipsToSend).Count <= 0 && ((List<ShipRenameInfo>)(object)ShipsToRename).Count <= 0 && ((List<Ship>)(object)ShipsToRepair).Count <= 0 && ((List<ShipUpgradePieceInfo>)(object)SelectedShipPieces).Count <= 0)
		{
			return ((List<ShipFigureheadInfo>)(object)SelectedFigureheads).Count > 0;
		}
		return true;
	}

	public void OnBuyShip(Ship ship)
	{
		bool flag = false;
		if (((IEnumerable<ShipTradeInfo>)_shipsToSell).Any((ShipTradeInfo x) => x.Ship == ship))
		{
			flag = true;
			((List<ShipTradeInfo>)(object)_shipsToSell).RemoveAll((Predicate<ShipTradeInfo>)((ShipTradeInfo x) => x.Ship == ship));
		}
		else if (!((IEnumerable<ShipTradeInfo>)_shipsToBuy).Any((ShipTradeInfo x) => x.Ship == ship))
		{
			((List<ShipTradeInfo>)(object)_shipsToBuy).Add(new ShipTradeInfo(ship, GetTradeCostOfShip(ship, isRightSideSelling: false)));
		}
		if (((List<Ship>)(object)_leftShips).Contains(ship))
		{
			((List<Ship>)(object)_leftShips).Remove(ship);
		}
		if (!((List<Ship>)(object)_rightShips).Contains(ship))
		{
			((List<Ship>)(object)_rightShips).Insert(0, ship);
		}
		ClearCurrentFigurehead(ship);
		if (flag)
		{
			ReequipPreviousFigurehead(ship);
		}
	}

	public void OnSellShip(Ship ship)
	{
		OnResetShipName(ship);
		OnResetShipUpgrade(ship);
		bool flag = false;
		if (((List<Ship>)(object)_shipsToRepair).Contains(ship))
		{
			((List<Ship>)(object)_shipsToRepair).Remove(ship);
		}
		if (((IEnumerable<ShipTradeInfo>)_shipsToBuy).Any((ShipTradeInfo x) => x.Ship == ship))
		{
			flag = true;
			((List<ShipTradeInfo>)(object)_shipsToBuy).RemoveAll((Predicate<ShipTradeInfo>)((ShipTradeInfo x) => x.Ship == ship));
		}
		else if (!((IEnumerable<ShipTradeInfo>)_shipsToSell).Any((ShipTradeInfo x) => x.Ship == ship))
		{
			((List<ShipTradeInfo>)(object)_shipsToSell).Add(new ShipTradeInfo(ship, GetTradeCostOfShip(ship, isRightSideSelling: true)));
		}
		if (((List<Ship>)(object)_rightShips).Contains(ship))
		{
			((List<Ship>)(object)_rightShips).Remove(ship);
		}
		if (!((List<Ship>)(object)_leftShips).Contains(ship))
		{
			((List<Ship>)(object)_leftShips).Insert(0, ship);
		}
		if (!flag)
		{
			ClearCurrentFigurehead(ship);
		}
	}

	public void OnRepairShip(Ship ship)
	{
		if (!((List<Ship>)(object)_shipsToRepair).Contains(ship))
		{
			((List<Ship>)(object)_shipsToRepair).Add(ship);
		}
	}

	public void OnSendToClan(Ship ship)
	{
		if (!((List<Ship>)(object)_shipsToSend).Contains(ship))
		{
			((List<Ship>)(object)_shipsToSend).Add(ship);
			((List<Ship>)(object)_rightShips).Remove(ship);
		}
		ClearCurrentFigurehead(ship);
	}

	public void OnRenameShip(Ship ship, string newName)
	{
		bool flag = false;
		for (int i = 0; i < ((List<ShipRenameInfo>)(object)_shipsToRename).Count; i++)
		{
			if (((List<ShipRenameInfo>)(object)_shipsToRename)[i].Ship == ship)
			{
				flag = true;
				((List<ShipRenameInfo>)(object)_shipsToRename)[i] = new ShipRenameInfo(ship, newName);
				break;
			}
		}
		if (!flag)
		{
			((List<ShipRenameInfo>)(object)_shipsToRename).Add(new ShipRenameInfo(ship, newName));
		}
	}

	public void OnResetShipName(Ship ship)
	{
		for (int num = ((List<ShipRenameInfo>)(object)_shipsToRename).Count - 1; num >= 0; num--)
		{
			if (((List<ShipRenameInfo>)(object)_shipsToRename)[num].Ship == ship)
			{
				((List<ShipRenameInfo>)(object)_shipsToRename).RemoveAt(num);
			}
		}
	}

	public void OnResetShipUpgrade(Ship ship)
	{
		for (int num = ((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces).Count - 1; num >= 0; num--)
		{
			if (((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces)[num].Ship == ship)
			{
				((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces).RemoveAt(num);
			}
		}
		for (int num2 = ((List<ShipFigureheadInfo>)(object)_selectedFigureheads).Count - 1; num2 >= 0; num2--)
		{
			if (((List<ShipFigureheadInfo>)(object)_selectedFigureheads)[num2].Ship == ship)
			{
				((List<ShipFigureheadInfo>)(object)_selectedFigureheads).RemoveAt(num2);
			}
		}
	}

	public void OnUpgradePieceSelected(Ship ship, string shipSlotTag, ShipUpgradePiece piece)
	{
		bool flag = false;
		bool flag2 = ship.GetPieceAtSlot(shipSlotTag) == piece;
		for (int i = 0; i < ((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces).Count; i++)
		{
			ShipUpgradePieceInfo shipUpgradePieceInfo = ((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces)[i];
			if (shipUpgradePieceInfo.Ship == ship && shipUpgradePieceInfo.ShipSlotTag == shipSlotTag)
			{
				flag = true;
				if (flag2)
				{
					((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces).RemoveAt(i);
				}
				else
				{
					((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces)[i] = new ShipUpgradePieceInfo(ship, shipSlotTag, piece);
				}
				break;
			}
		}
		if (!flag && !flag2)
		{
			((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces).Add(new ShipUpgradePieceInfo(ship, shipSlotTag, piece));
		}
	}

	public void OnFigureheadSelected(Ship ship, Figurehead figurehead)
	{
		bool flag = false;
		bool flag2 = figurehead == ship.Figurehead;
		for (int i = 0; i < ((List<ShipFigureheadInfo>)(object)_selectedFigureheads).Count; i++)
		{
			if (((List<ShipFigureheadInfo>)(object)_selectedFigureheads)[i].Ship == ship)
			{
				flag = true;
				if (flag2)
				{
					((List<ShipFigureheadInfo>)(object)_selectedFigureheads).RemoveAt(i);
				}
				else
				{
					((List<ShipFigureheadInfo>)(object)_selectedFigureheads)[i] = new ShipFigureheadInfo(ship, figurehead);
				}
				break;
			}
		}
		if (!flag && !flag2)
		{
			((List<ShipFigureheadInfo>)(object)_selectedFigureheads).Add(new ShipFigureheadInfo(ship, figurehead));
		}
	}

	public void ResetChanges()
	{
		((List<ShipTradeInfo>)(object)_shipsToBuy).Clear();
		((List<ShipTradeInfo>)(object)_shipsToSell).Clear();
		((List<ShipRenameInfo>)(object)_shipsToRename).Clear();
		((List<Ship>)(object)_shipsToRepair).Clear();
		((List<ShipUpgradePieceInfo>)(object)_selectedShipPieces).Clear();
		((List<ShipFigureheadInfo>)(object)_selectedFigureheads).Clear();
		((List<Ship>)(object)_shipsToSend).Clear();
		((List<Ship>)(object)_leftShips).Clear();
		((List<Ship>)(object)_rightShips).Clear();
		((List<Ship>)(object)_leftShips).AddRange((IEnumerable<Ship>)_initialLeftShips);
		((List<Ship>)(object)_rightShips).AddRange((IEnumerable<Ship>)_initialRightShips);
	}

	private void ClearCurrentFigurehead(Ship ship)
	{
		Figurehead figurehead = ship.Figurehead;
		for (int i = 0; i < ((List<ShipFigureheadInfo>)(object)_selectedFigureheads).Count; i++)
		{
			ShipFigureheadInfo shipFigureheadInfo = ((List<ShipFigureheadInfo>)(object)_selectedFigureheads)[i];
			if (shipFigureheadInfo.Ship == ship)
			{
				figurehead = shipFigureheadInfo.Figurehead;
				break;
			}
		}
		if (figurehead != null)
		{
			OnFigureheadSelected(ship, null);
		}
	}

	private void ReequipPreviousFigurehead(Ship ship)
	{
		Figurehead figurehead = ship.Figurehead;
		bool flag = false;
		for (int i = 0; i < ((List<ShipFigureheadInfo>)(object)_selectedFigureheads).Count; i++)
		{
			ShipFigureheadInfo shipFigureheadInfo = ((List<ShipFigureheadInfo>)(object)_selectedFigureheads)[i];
			if (shipFigureheadInfo.Figurehead == figurehead && shipFigureheadInfo.Ship != null)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			OnFigureheadSelected(ship, figurehead);
		}
	}
}
