using System.Collections.Generic;
using System.Linq;
using NavalDLC.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection.Port.PortScreenHandlers;

public class PortScreenTradeModeHandler : PortScreenHandler
{
	private readonly PartyBase _leftOwner;

	private readonly PartyBase _rightOwner;

	public PortScreenTradeModeHandler(PartyBase leftOwner, PartyBase rightOwner)
		: base(leftOwner.Ships, rightOwner.Ships)
	{
		_leftOwner = leftOwner;
		_rightOwner = rightOwner;
	}

	public override TextObject GetLeftRosterName()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (_leftOwner.IsSettlement)
		{
			return new TextObject("{=UeUkbDVz}Port of {SETTLEMENT}", (Dictionary<string, object>)null).SetTextVariable("SETTLEMENT", _leftOwner.Name);
		}
		return _leftOwner.Name;
	}

	public override TextObject GetRightRosterName()
	{
		return _rightOwner.Name;
	}

	public override PartyBase GetLeftSideOwnerParty()
	{
		return _leftOwner;
	}

	public override PartyBase GetRightSideOwnerParty()
	{
		return _rightOwner;
	}

	protected override PortActionInfo CanBuyShip(Ship ship)
	{
		bool num = ((IEnumerable<ShipTradeInfo>)base.ShipsToSell).Any((ShipTradeInfo x) => x.Ship == ship);
		int goldCost = (num ? ((IEnumerable<ShipTradeInfo>)base.ShipsToSell).FirstOrDefault((ShipTradeInfo x) => x.Ship == ship).Price : GetTradeCostOfShip(ship, isRightSideSelling: false));
		TextObject name = (num ? GameTexts.FindText("str_port_buy_ship_back", (string)null) : GameTexts.FindText("str_port_buy_ship", (string)null));
		return PortActionInfo.CreateValid(isEnabled: true, goldCost, name, TextObject.GetEmpty());
	}

	protected override PortActionInfo CanSellShip(Ship ship)
	{
		bool num = ((IEnumerable<ShipTradeInfo>)base.ShipsToBuy).Any((ShipTradeInfo x) => x.Ship == ship);
		int goldCost = (num ? ((IEnumerable<ShipTradeInfo>)base.ShipsToBuy).FirstOrDefault((ShipTradeInfo x) => x.Ship == ship).Price : GetTradeCostOfShip(ship, isRightSideSelling: true));
		TextObject name = (num ? GameTexts.FindText("str_port_sell_ship_back", (string)null) : GameTexts.FindText("str_port_sell_ship", (string)null));
		if (MobileParty.MainParty.IsCurrentlyAtSea && ((List<Ship>)(object)base.RightShips).Count == 1)
		{
			Debug.FailedAssert("Trade mode should not be accessible from the sea!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\PortScreenHandlers\\PortScreenTradeModeHandler.cs", "CanSellShip", 67);
			PortActionInfo.CreateValid(isEnabled: false, goldCost, name, GameTexts.FindText("str_cannot_give_all_ships", (string)null));
		}
		return PortActionInfo.CreateValid(isEnabled: true, goldCost, name, TextObject.GetEmpty());
	}

	protected override PortActionInfo CanRepairShip(Ship ship)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (((List<Ship>)(object)base.ShipsToRepair).Contains(ship))
		{
			return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_ship", (string)null), new TextObject("{=Ma26nyeo}Already repaired", (Dictionary<string, object>)null));
		}
		return PortActionInfo.CreateValid(isEnabled: true, GetRepairCostOfShip(ship, isRightSideRepairing: true), GameTexts.FindText("str_port_repair_ship", (string)null), TextObject.GetEmpty());
	}

	protected override PortActionInfo CanRepairAll()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		MBList<Ship> val = new MBList<Ship>();
		int num = 0;
		foreach (Ship item in (List<Ship>)(object)base.RightShips)
		{
			if (!((List<Ship>)(object)base.ShipsToRepair).Contains(item) && item.HitPoints < item.MaxHitPoints)
			{
				((List<Ship>)(object)val).Add(item);
				num += GetRepairCostOfShip(item, isRightSideRepairing: true);
			}
		}
		if (((List<Ship>)(object)val).Count == 0)
		{
			return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_all_ships", (string)null), new TextObject("{=Ma26nyeo}Already repaired", (Dictionary<string, object>)null));
		}
		return PortActionInfo.CreateValid(isEnabled: true, num, GameTexts.FindText("str_port_repair_all_ships", (string)null), TextObject.GetEmpty());
	}

	protected override PortActionInfo CanUpgradeShip(Ship ship)
	{
		return PortActionInfo.CreateValid(isEnabled: true, 0, GameTexts.FindText("str_port_upgrade_ship", (string)null), TextObject.GetEmpty());
	}

	protected override PortActionInfo CanRenameShip(Ship ship)
	{
		return PortActionInfo.CreateValid(isEnabled: true, 0, GameTexts.FindText("str_port_rename_ship", (string)null), TextObject.GetEmpty());
	}

	protected override PortActionInfo CanSendToClan(Ship ship)
	{
		int num = ((List<Ship>)(object)base.ShipsToSend).Count * Campaign.Current.Models.FleetManagementModel.MinimumTroopCountRequiredToSendShips;
		TextObject tooltip = default(TextObject);
		return PortActionInfo.CreateValid(Campaign.Current.Models.FleetManagementModel.CanSendShipToPlayerClan(ship, ((List<Ship>)(object)base.RightShips).Count, num, ref tooltip), 0, GameTexts.FindText("str_port_send_ship_to_clan", (string)null), tooltip);
	}

	public override int GetTradeCostOfShip(Ship ship, bool isRightSideSelling)
	{
		PartyBase val = (isRightSideSelling ? _rightOwner : _leftOwner);
		PartyBase val2 = (isRightSideSelling ? _leftOwner : _rightOwner);
		return (int)Campaign.Current.Models.ShipCostModel.GetShipTradeValue(ship, val, val2);
	}

	public override int GetRepairCostOfShip(Ship ship, bool isRightSideRepairing)
	{
		PartyBase val = (isRightSideRepairing ? _rightOwner : _leftOwner);
		return (int)Campaign.Current.Models.ShipCostModel.GetShipRepairCost(ship, val);
	}

	public override int GetUpgradeCostOfShip(Ship ship, ShipUpgradePiece piece, bool isRightSideUpgrading)
	{
		PartyBase val = (isRightSideUpgrading ? _rightOwner : _leftOwner);
		return Campaign.Current.Models.ShipCostModel.GetShipUpgradePieceCost(ship, piece, val);
	}

	public override int GetTotalGoldCost()
	{
		int num = 0;
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			num += ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Price;
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			num -= ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Price;
		}
		for (int k = 0; k < ((List<Ship>)(object)base.ShipsToRepair).Count; k++)
		{
			Ship ship = ((List<Ship>)(object)base.ShipsToRepair)[k];
			num += GetRepairCostOfShip(ship, isRightSideRepairing: true);
		}
		for (int l = 0; l < ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces).Count; l++)
		{
			Ship ship2 = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[l].Ship;
			ShipUpgradePiece piece = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[l].Piece;
			if (piece != null)
			{
				num += GetUpgradeCostOfShip(ship2, piece, isRightSideUpgrading: true);
			}
		}
		return num;
	}

	public override bool GetCanConfirm(out TextObject disabledHint)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if (GetTotalGoldCost() > Hero.MainHero.Gold)
		{
			disabledHint = new TextObject("{=RYJdU43V}Not Enough Gold", (Dictionary<string, object>)null);
			return false;
		}
		disabledHint = null;
		return true;
	}

	public override void OnConfirmChanges()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			Ship ship = ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship;
			ChangeShipOwnerAction.ApplyByTrade(_rightOwner, ship);
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			Ship ship2 = ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Ship;
			ChangeShipOwnerAction.ApplyByTrade(_leftOwner, ship2);
		}
		for (int k = 0; k < ((List<Ship>)(object)base.ShipsToRepair).Count; k++)
		{
			RepairShipAction.Apply(((List<Ship>)(object)base.ShipsToRepair)[k], Settlement.CurrentSettlement);
		}
		for (int l = 0; l < ((List<ShipRenameInfo>)(object)base.ShipsToRename).Count; l++)
		{
			ShipRenameInfo shipRenameInfo = ((List<ShipRenameInfo>)(object)base.ShipsToRename)[l];
			shipRenameInfo.Ship.SetName(new TextObject("{=!}" + shipRenameInfo.NewName, (Dictionary<string, object>)null));
		}
		for (int m = 0; m < ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces).Count; m++)
		{
			Ship ship3 = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Ship;
			string shipSlotTag = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].ShipSlotTag;
			ShipUpgradePiece piece = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Piece;
			int num = 0;
			if (piece != null)
			{
				num += GetUpgradeCostOfShip(ship3, piece, isRightSideUpgrading: true);
			}
			ship3.EquipUpgradePiece(shipSlotTag, piece);
			if (num > 0)
			{
				GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, _leftOwner.Settlement, num, false);
			}
			else
			{
				GiveGoldAction.ApplyForSettlementToCharacter(_leftOwner.Settlement, Hero.MainHero, -num, false);
			}
		}
		for (int n = 0; n < ((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads).Count; n++)
		{
			Ship ship4 = ((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads)[n].Ship;
			Figurehead figurehead = ((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads)[n].Figurehead;
			ship4.ChangeFigurehead(figurehead);
		}
		IFleetManagementCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IFleetManagementCampaignBehavior>();
		for (int num2 = 0; num2 < ((List<Ship>)(object)base.ShipsToSend).Count; num2++)
		{
			campaignBehavior.SendShipToClan(((List<Ship>)(object)base.ShipsToSend)[num2], Clan.PlayerClan);
		}
		if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count == 0 && MobileParty.MainParty.Anchor.IsValid)
		{
			MobileParty.MainParty.Anchor.ResetPosition();
		}
		else if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count > 0 && !MobileParty.MainParty.Anchor.IsValid && _leftOwner.IsSettlement)
		{
			MobileParty.MainParty.Anchor.SetSettlement(_leftOwner.Settlement);
		}
	}

	public override List<PortChangeInfo> GetChanges()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		List<PortChangeInfo> list = new List<PortChangeInfo>();
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			list.Add(new PortChangeInfo(((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Price, ((object)new TextObject("{=9AIOcUuH}Buy {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship.Name)).ToString()));
		}
		for (int j = 0; j < ((List<ShipRenameInfo>)(object)base.ShipsToRename).Count; j++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=Fidoxgd1}Rename {SHIP_NAME} to {NEW_SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipRenameInfo>)(object)base.ShipsToRename)[j].Ship.Name).SetTextVariable("NEW_SHIP_NAME", ((List<ShipRenameInfo>)(object)base.ShipsToRename)[j].NewName)).ToString()));
		}
		for (int k = 0; k < ((List<Ship>)(object)base.ShipsToRepair).Count; k++)
		{
			list.Add(new PortChangeInfo(GetRepairCostOfShip(((List<Ship>)(object)base.ShipsToRepair)[k], isRightSideRepairing: true), ((object)new TextObject("{=HQK9kUD9}Repair {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<Ship>)(object)base.ShipsToRepair)[k]))).ToString()));
		}
		for (int l = 0; l < ((List<Ship>)(object)base.ShipsToSend).Count; l++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=L1x30kUJ}Send {SHIP_NAME} to clan", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<Ship>)(object)base.ShipsToSend)[l]))).ToString()));
		}
		for (int m = 0; m < ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces).Count; m++)
		{
			ShipUpgradePiece piece = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Piece;
			ShipUpgradePiece pieceAtSlot = ((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Ship.GetPieceAtSlot(((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].ShipSlotTag);
			if (pieceAtSlot != null)
			{
				list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=PniFsE6M}Remove {PIECE_NAME} from {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Ship)).SetTextVariable("PIECE_NAME", ((MBObjectBase)pieceAtSlot).GetName())).ToString()));
			}
			if (piece != null)
			{
				list.Add(new PortChangeInfo(GetUpgradeCostOfShip(((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Ship, piece, isRightSideUpgrading: true), ((object)new TextObject("{=jwgUwyKO}Add {PIECE_NAME} to {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<ShipUpgradePieceInfo>)(object)base.SelectedShipPieces)[m].Ship)).SetTextVariable("PIECE_NAME", ((MBObjectBase)piece).GetName())).ToString()));
			}
		}
		for (int n = 0; n < ((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads).Count; n++)
		{
			Figurehead figurehead = ((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads)[n].Figurehead;
			Figurehead figurehead2 = ((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads)[n].Ship.Figurehead;
			if (figurehead2 != null)
			{
				list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=PniFsE6M}Remove {PIECE_NAME} from {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads)[n].Ship)).SetTextVariable("PIECE_NAME", ((MBObjectBase)figurehead2).GetName())).ToString()));
			}
			if (figurehead != null)
			{
				list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=jwgUwyKO}Add {PIECE_NAME} to {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<ShipFigureheadInfo>)(object)base.SelectedFigureheads)[n].Ship)).SetTextVariable("PIECE_NAME", ((MBObjectBase)figurehead).GetName())).ToString()));
			}
		}
		for (int num = 0; num < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; num++)
		{
			list.Add(new PortChangeInfo(-((List<ShipTradeInfo>)(object)base.ShipsToSell)[num].Price, ((object)new TextObject("{=1Yaq0qy1}Sell {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToSell)[num].Ship.Name)).ToString()));
		}
		return list;
	}

	private TextObject GetShipNameConsideringRenames(Ship ship)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		TextObject result = ship.Name;
		if (((IEnumerable<ShipRenameInfo>)base.ShipsToRename).Any((ShipRenameInfo x) => x.Ship == ship))
		{
			result = new TextObject("{=!}" + ((IEnumerable<ShipRenameInfo>)base.ShipsToRename).First((ShipRenameInfo x) => x.Ship == ship).NewName, (Dictionary<string, object>)null);
		}
		return result;
	}
}
