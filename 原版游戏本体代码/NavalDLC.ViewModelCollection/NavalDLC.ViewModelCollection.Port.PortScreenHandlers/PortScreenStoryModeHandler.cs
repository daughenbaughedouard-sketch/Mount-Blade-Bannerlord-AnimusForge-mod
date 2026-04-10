using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port.PortScreenHandlers;

public class PortScreenStoryModeHandler : PortScreenHandler
{
	private readonly PartyBase _leftParty;

	private readonly PartyBase _rightParty;

	public PortScreenStoryModeHandler(PartyBase leftParty, PartyBase rightParty)
		: base(leftParty.Ships, rightParty.Ships)
	{
		_leftParty = leftParty;
		_rightParty = rightParty;
	}

	public override TextObject GetLeftRosterName()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (_leftParty.IsSettlement)
		{
			return new TextObject("{=UeUkbDVz}Port of {SETTLEMENT}", (Dictionary<string, object>)null).SetTextVariable("SETTLEMENT", _leftParty.Name);
		}
		return _leftParty.Name;
	}

	public override TextObject GetRightRosterName()
	{
		return _rightParty.Name;
	}

	public override PartyBase GetLeftSideOwnerParty()
	{
		return _leftParty;
	}

	public override PartyBase GetRightSideOwnerParty()
	{
		return _rightParty;
	}

	public override int GetTradeCostOfShip(Ship ship, bool isRightSideSelling)
	{
		PartyBase val = (isRightSideSelling ? _rightParty : _leftParty);
		PartyBase val2 = (isRightSideSelling ? _leftParty : _rightParty);
		return (int)Campaign.Current.Models.ShipCostModel.GetShipTradeValue(ship, val, val2);
	}

	public override int GetRepairCostOfShip(Ship ship, bool isRightSideRepairing)
	{
		return 0;
	}

	public override int GetUpgradeCostOfShip(Ship ship, ShipUpgradePiece piece, bool isRightSideUpgrading)
	{
		return 0;
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
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			Ship ship = ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship;
			ChangeShipOwnerAction.ApplyByTrade(_rightParty, ship);
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			Ship ship2 = ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Ship;
			ChangeShipOwnerAction.ApplyByTrade(_leftParty, ship2);
		}
		if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count == 0 && MobileParty.MainParty.Anchor.IsValid)
		{
			MobileParty.MainParty.Anchor.ResetPosition();
		}
		else if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count > 0 && !MobileParty.MainParty.Anchor.IsValid && _leftParty.IsSettlement)
		{
			MobileParty.MainParty.Anchor.SetPosition(_leftParty.Settlement.PortPosition);
		}
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
		return PortActionInfo.CreateValid(isEnabled: true, goldCost, name, TextObject.GetEmpty());
	}

	protected override PortActionInfo CanRenameShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_rename_ship", (string)null), new TextObject("{=i6BBEAXI}You can't rename ships at this stage", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRepairShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_ship", (string)null), new TextObject("{=HqraYjwT}You can't repair ships at this stage", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRepairAll()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_all_ships", (string)null), new TextObject("{=HqraYjwT}You can't repair ships at this stage", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanUpgradeShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_upgrade_ship", (string)null), new TextObject("{=b3eIbvr0}You can't upgrade ships at this stage", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanSendToClan(Ship ship)
	{
		return PortActionInfo.CreateInvalid();
	}

	public override List<PortChangeInfo> GetChanges()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		List<PortChangeInfo> list = new List<PortChangeInfo>();
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			list.Add(new PortChangeInfo(((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Price, ((object)new TextObject("{=9AIOcUuH}Buy {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship.Name)).ToString()));
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			list.Add(new PortChangeInfo(((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Price, ((object)new TextObject("{=1Yaq0qy1}Sell {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Ship.Name)).ToString()));
		}
		return list;
	}
}
