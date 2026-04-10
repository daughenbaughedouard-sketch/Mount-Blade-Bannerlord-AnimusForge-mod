using System.Collections.Generic;
using System.Linq;
using NavalDLC.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.ViewModelCollection.Port.PortScreenHandlers;

public class PortScreenLootModeHandler : PortScreenHandler
{
	private readonly TextObject _leftSideName;

	private readonly PartyBase _rightSide;

	public PortScreenLootModeHandler(TextObject leftSideName, PartyBase rightSide, MBReadOnlyList<Ship> initialLeftShips, MBReadOnlyList<Ship> initialRightShips)
		: base(initialLeftShips, initialRightShips)
	{
		_leftSideName = leftSideName;
		_rightSide = rightSide;
	}

	protected override PortActionInfo CanBuyShip(Ship ship)
	{
		return PortActionInfo.CreateValid(isEnabled: true, 0, GameTexts.FindText("str_take", (string)null), null);
	}

	protected override PortActionInfo CanSellShip(Ship ship)
	{
		if (MobileParty.MainParty.IsCurrentlyAtSea && ((List<Ship>)(object)base.RightShips).Count == 1)
		{
			return PortActionInfo.CreateValid(isEnabled: false, 0, GameTexts.FindText("str_port_discard_ship", (string)null), GameTexts.FindText("str_cannot_give_all_ships", (string)null));
		}
		return PortActionInfo.CreateValid(isEnabled: true, 0, GameTexts.FindText("str_port_discard_ship", (string)null), null);
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

	protected override PortActionInfo CanUpgradeShip(Ship ship)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		return PortActionInfo.CreateInvalid(new TextObject("{=4d7XLElL}You can't upgrade ships outside a port.", (Dictionary<string, object>)null));
	}

	protected override PortActionInfo CanSendToClan(Ship ship)
	{
		int num = ((List<Ship>)(object)base.ShipsToSend).Count * Campaign.Current.Models.FleetManagementModel.MinimumTroopCountRequiredToSendShips;
		TextObject tooltip = default(TextObject);
		return PortActionInfo.CreateValid(Campaign.Current.Models.FleetManagementModel.CanSendShipToPlayerClan(ship, ((List<Ship>)(object)base.RightShips).Count, num, ref tooltip), 0, GameTexts.FindText("str_port_send_ship_to_clan", (string)null), tooltip);
	}

	public override bool GetCanConfirm(out TextObject disabledHint)
	{
		disabledHint = null;
		return true;
	}

	public override TextObject GetLeftRosterName()
	{
		return _leftSideName;
	}

	public override PartyBase GetLeftSideOwnerParty()
	{
		return null;
	}

	public override TextObject GetRightRosterName()
	{
		return _rightSide.Name;
	}

	public override PartyBase GetRightSideOwnerParty()
	{
		return _rightSide;
	}

	public override int GetTradeCostOfShip(Ship ship, bool isSelling)
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

	public override int GetTotalGoldCost()
	{
		return 0;
	}

	public override void OnConfirmChanges()
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		foreach (ShipTradeInfo item in (List<ShipTradeInfo>)(object)base.ShipsToBuy)
		{
			ChangeShipOwnerAction.ApplyByLooting(PartyBase.MainParty, item.Ship);
		}
		foreach (ShipTradeInfo item2 in (List<ShipTradeInfo>)(object)base.ShipsToSell)
		{
			DestroyShipAction.ApplyByDiscard(item2.Ship);
		}
		IFleetManagementCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IFleetManagementCampaignBehavior>();
		for (int i = 0; i < ((List<Ship>)(object)base.ShipsToSend).Count; i++)
		{
			campaignBehavior.SendShipToClan(((List<Ship>)(object)base.ShipsToSend)[i], Clan.PlayerClan);
		}
		for (int j = 0; j < ((List<ShipRenameInfo>)(object)base.ShipsToRename).Count; j++)
		{
			ShipRenameInfo shipRenameInfo = ((List<ShipRenameInfo>)(object)base.ShipsToRename)[j];
			shipRenameInfo.Ship.SetName(new TextObject("{=!}" + shipRenameInfo.NewName, (Dictionary<string, object>)null));
		}
		if (((List<Ship>)(object)MobileParty.MainParty.Ships).Count == 0 && MobileParty.MainParty.Anchor.IsValid)
		{
			MobileParty.MainParty.Anchor.ResetPosition();
		}
	}

	public override List<PortChangeInfo> GetChanges()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		List<PortChangeInfo> list = new List<PortChangeInfo>();
		for (int i = 0; i < ((List<ShipTradeInfo>)(object)base.ShipsToBuy).Count; i++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=TsQzdjvd}Take {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToBuy)[i].Ship.Name)).ToString()));
		}
		for (int j = 0; j < ((List<ShipTradeInfo>)(object)base.ShipsToSell).Count; j++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=cItrQpwh}Discard {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipTradeInfo>)(object)base.ShipsToSell)[j].Ship.Name)).ToString()));
		}
		for (int k = 0; k < ((List<ShipRenameInfo>)(object)base.ShipsToRename).Count; k++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=Fidoxgd1}Rename {SHIP_NAME} to {NEW_SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", ((List<ShipRenameInfo>)(object)base.ShipsToRename)[k].Ship.Name).SetTextVariable("NEW_SHIP_NAME", ((List<ShipRenameInfo>)(object)base.ShipsToRename)[k].NewName)).ToString()));
		}
		for (int l = 0; l < ((List<Ship>)(object)base.ShipsToSend).Count; l++)
		{
			list.Add(new PortChangeInfo(0f, ((object)new TextObject("{=L1x30kUJ}Send {SHIP_NAME} to clan", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", GetShipNameConsideringRenames(((List<Ship>)(object)base.ShipsToSend)[l]))).ToString()));
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
