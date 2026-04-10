using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port.PortScreenHandlers;

public class PortScreenRestrictedModeHandler : PortScreenHandler
{
	private readonly PartyBase _leftOwner;

	private readonly PartyBase _rightOwner;

	public PortScreenRestrictedModeHandler(PartyBase leftOwner, PartyBase rightOwner)
		: base(leftOwner.Ships, new MBReadOnlyList<Ship>())
	{
		_leftOwner = leftOwner;
		_rightOwner = rightOwner;
	}

	protected override PortActionInfo CanBuyShip(Ship ship)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, GetTradeCostOfShip(ship, isRightSideSelling: false), GameTexts.FindText("str_port_buy_ship", (string)null), new TextObject("{=a2oyqIOU}You cannot buy ships when your fleet is away", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanSellShip(Ship ship)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, GetTradeCostOfShip(ship, isRightSideSelling: true), GameTexts.FindText("str_port_sell_ship", (string)null), new TextObject("{=YCwajsdL}You cannot sell ships when your fleet is away", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRenameShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_rename_ship", (string)null), new TextObject("{=xmmYDcyd}You cannot rename ships when your fleet is away", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRepairShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_ship", (string)null), new TextObject("{=7ccDIA8H}You cannot repair ships when your fleet is away", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanRepairAll()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_repair_all_ships", (string)null), new TextObject("{=7ccDIA8H}You cannot repair ships when your fleet is away", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanUpgradeShip(Ship ship)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_upgrade_ship", (string)null), new TextObject("{=5CXQsbqV}You cannot upgrade ships when your fleet is away", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanSendToClan(Ship ship)
	{
		return PortActionInfo.CreateInvalid();
	}

	public override bool GetCanConfirm(out TextObject disabledHint)
	{
		disabledHint = TextObject.GetEmpty();
		return true;
	}

	public override TextObject GetLeftRosterName()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		PartyBase leftOwner = _leftOwner;
		if (leftOwner != null && leftOwner.IsSettlement)
		{
			return new TextObject("{=UeUkbDVz}Port of {SETTLEMENT}", (Dictionary<string, object>)null).SetTextVariable("SETTLEMENT", _leftOwner.Name);
		}
		PartyBase leftOwner2 = _leftOwner;
		if (leftOwner2 == null)
		{
			return null;
		}
		return leftOwner2.Name;
	}

	public override int GetTradeCostOfShip(Ship ship, bool isRightSideSelling)
	{
		PartyBase val = (isRightSideSelling ? _rightOwner : _leftOwner);
		PartyBase val2 = (isRightSideSelling ? _leftOwner : _rightOwner);
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

	public override TextObject GetRightRosterName()
	{
		PartyBase rightOwner = _rightOwner;
		if (rightOwner == null)
		{
			return null;
		}
		return rightOwner.Name;
	}

	public override PartyBase GetLeftSideOwnerParty()
	{
		return _leftOwner;
	}

	public override PartyBase GetRightSideOwnerParty()
	{
		return _rightOwner;
	}

	public override int GetTotalGoldCost()
	{
		return 0;
	}

	public override void OnConfirmChanges()
	{
	}

	public override List<PortChangeInfo> GetChanges()
	{
		return new List<PortChangeInfo>();
	}
}
