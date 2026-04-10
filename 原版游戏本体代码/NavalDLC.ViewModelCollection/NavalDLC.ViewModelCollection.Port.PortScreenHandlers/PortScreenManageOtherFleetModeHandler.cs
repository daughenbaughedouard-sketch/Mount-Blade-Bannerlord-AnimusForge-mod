using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port.PortScreenHandlers;

public class PortScreenManageOtherFleetModeHandler : PortScreenHandler
{
	private readonly PartyBase _other;

	public PortScreenManageOtherFleetModeHandler(PartyBase other)
		: base(other.Ships, MobileParty.MainParty.Ships)
	{
		_other = other;
	}

	public override bool GetCanConfirm(out TextObject disabledHint)
	{
		disabledHint = null;
		return true;
	}

	public override PartyBase GetLeftSideOwnerParty()
	{
		return _other;
	}

	public override PartyBase GetRightSideOwnerParty()
	{
		return MobileParty.MainParty.Party;
	}

	public override TextObject GetLeftRosterName()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (_other.IsSettlement)
		{
			return new TextObject("{=UeUkbDVz}Port of {SETTLEMENT}", (Dictionary<string, object>)null).SetTextVariable("SETTLEMENT", _other.Name);
		}
		return _other.Name;
	}

	public override int GetTradeCostOfShip(Ship ship, bool isRightSideSelling)
	{
		return 0;
	}

	public override int GetRepairCostOfShip(Ship ship, bool isRightSideRepairing)
	{
		return 0;
	}

	public override int GetUpgradeCostOfShip(Ship ship, ShipUpgradePiece piece, bool isRightSideUpgrading)
	{
		return 0;
	}

	public override TextObject GetRightRosterName()
	{
		return MobileParty.MainParty.Name;
	}

	public override int GetTotalGoldCost()
	{
		return 0;
	}

	public override void OnConfirmChanges()
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			Ship ship = ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship;
			ChangeShipOwnerAction.ApplyByTransferring(MobileParty.MainParty.Party, ship);
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			Ship ship2 = ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Ship;
			ChangeShipOwnerAction.ApplyByTransferring(_other, ship2);
		}
		if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count == 0 && MobileParty.MainParty.Anchor.IsValid)
		{
			MobileParty.MainParty.Anchor.ResetPosition();
		}
		if (((List<Ship>)(object)_other.Ships).Count == 0 && _other.IsMobile && _other.MobileParty.Anchor.IsValid)
		{
			_other.MobileParty.Anchor.ResetPosition();
		}
		for (int k = 0; k < ((List<ShipRenameInfo>)(object)base.ShipsToRename).Count; k++)
		{
			ShipRenameInfo shipRenameInfo = ((List<ShipRenameInfo>)(object)base.ShipsToRename)[k];
			shipRenameInfo.Ship.SetName(new TextObject("{=!}" + shipRenameInfo.NewName, (Dictionary<string, object>)null));
		}
	}

	protected override PortActionInfo CanBuyShip(Ship ship)
	{
		TextObject name = (((IEnumerable<ShipTradeInfo>)base.ShipsToSell).Any((ShipTradeInfo x) => x.Ship == ship) ? GameTexts.FindText("str_take_ship_back", (string)null) : GameTexts.FindText("str_take", (string)null));
		TextObject disabledHint;
		return PortActionInfo.CreateValid(CanBuyShip(ship, out disabledHint), 0, name, disabledHint);
	}

	protected override PortActionInfo CanSellShip(Ship ship)
	{
		TextObject name = (((IEnumerable<ShipTradeInfo>)base.ShipsToBuy).Any((ShipTradeInfo x) => x.Ship == ship) ? GameTexts.FindText("str_give_ship_back", (string)null) : GameTexts.FindText("str_give", (string)null));
		TextObject disabledHint;
		return PortActionInfo.CreateValid(CanSellShip(ship, out disabledHint), 0, name, disabledHint);
	}

	protected override PortActionInfo CanUpgradeShip(Ship ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		return PortActionInfo.CreateInvalid(new TextObject("{=4d7XLElL}You can't upgrade ships outside a port.", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRenameShip(Ship ship)
	{
		return PortActionInfo.CreateValid(isEnabled: true, 0, GameTexts.FindText("str_port_rename_ship", (string)null), TextObject.GetEmpty());
	}

	protected override PortActionInfo CanRepairShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_ship", (string)null), new TextObject("{=Pm6JbaXa}You can't repair ships outside a port.", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRepairAll()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_all_ships", (string)null), new TextObject("{=Pm6JbaXa}You can't repair ships outside a port.", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanSendToClan(Ship ship)
	{
		return PortActionInfo.CreateInvalid();
	}

	private bool CanSellShip(Ship ship, out TextObject disabledHint)
	{
		disabledHint = TextObject.GetEmpty();
		if (((IEnumerable<ShipTradeInfo>)base.ShipsToSell).Any((ShipTradeInfo x) => x.Ship == ship))
		{
			return false;
		}
		if (MobileParty.MainParty.IsCurrentlyAtSea && ((List<Ship>)(object)base.RightShips).Count == 1)
		{
			disabledHint = GameTexts.FindText("str_cannot_give_all_ships", (string)null);
			return false;
		}
		return true;
	}

	private bool CanBuyShip(Ship ship, out TextObject disabledHint)
	{
		disabledHint = TextObject.GetEmpty();
		if (((IEnumerable<ShipTradeInfo>)base.ShipsToBuy).Any((ShipTradeInfo x) => x.Ship == ship))
		{
			return false;
		}
		if (_other.MobileParty.IsCurrentlyAtSea && ((List<Ship>)(object)_other.Ships).Count + ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count - ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count <= 1)
		{
			disabledHint = GameTexts.FindText("str_cannot_take_all_ships", (string)null);
			return false;
		}
		return true;
	}

	public override List<PortChangeInfo> GetChanges()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		List<PortChangeInfo> list = new List<PortChangeInfo>();
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=TsQzdjvd}Take {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship.Name)).ToString()));
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=LZsY5SyD}Give {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Ship.Name)).ToString()));
		}
		for (int k = 0; k < ((List<ShipRenameInfo>)(object)base.ShipsToRename).Count; k++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=Fidoxgd1}Rename {SHIP_NAME} to {NEW_SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipRenameInfo>)(object)base.ShipsToRename)[k].Ship.Name).SetTextVariable("NEW_SHIP_NAME", ((List<ShipRenameInfo>)(object)base.ShipsToRename)[k].NewName)).ToString()));
		}
		return list;
	}
}
