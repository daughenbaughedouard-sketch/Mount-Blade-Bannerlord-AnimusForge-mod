using System;
using System.Collections.Generic;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCFleetManagementModel : FleetManagementModel
{
	public override int MinimumTroopCountRequiredToSendShips => 8;

	public override bool CanTroopsReturn()
	{
		if (!Hero.MainHero.IsPrisoner && (!MobileParty.MainParty.IsCurrentlyAtSea || Settlement.CurrentSettlement != null))
		{
			return MobileParty.MainParty.MapEvent == null;
		}
		return false;
	}

	public override CampaignTime GetReturnTimeForTroops(Ship ship)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Hero mainHero = Hero.MainHero;
		CampaignTime now = CampaignTime.Now;
		return CampaignTime.DaysFromNow(RandomOwnerExtensions.RandomFloatWithSeed((IRandomOwner)(object)mainHero, (uint)((CampaignTime)(ref now)).ToMinutes, 3f, 6f));
	}

	public override bool CanSendShipToPlayerClan(Ship ship, int playerShipsCount, int troopsCountToSend, out TextObject hint)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		hint = TextObject.GetEmpty();
		bool result = true;
		if (NavalStorylineData.IsNavalStoryLineActive())
		{
			hint = new TextObject("{=lwbwTg5b}You can't perform this action during this time.", (Dictionary<string, object>)null);
			result = false;
		}
		else if (!ship.IsTradeable || ship.IsUsedByQuest)
		{
			hint = GameTexts.FindText("str_port_cant_take_action_quest_ship", (string)null);
			result = false;
		}
		else if (playerShipsCount == 1 && MobileParty.MainParty.IsCurrentlyAtSea)
		{
			hint = GameTexts.FindText("str_cannot_give_all_ships", (string)null);
			result = false;
		}
		else if (LinQuick.AllQ<WarPartyComponent>((List<WarPartyComponent>)(object)Clan.PlayerClan.WarPartyComponents, (Func<WarPartyComponent, bool>)((WarPartyComponent x) => !NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanSendShipToParty(ship, ((PartyComponent)x).MobileParty))))
		{
			hint = new TextObject("{=SwV5iZbN}There are no suitable parties in your clan to send ships to.", (Dictionary<string, object>)null);
			result = false;
		}
		else if (MobileParty.MainParty.MemberRoster.TotalRegulars - troopsCountToSend < Campaign.Current.Models.FleetManagementModel.MinimumTroopCountRequiredToSendShips)
		{
			hint = new TextObject("{=U4avdcnH}You need at least {NUMBER} troops to send with the ship.", (Dictionary<string, object>)null);
			hint.SetTextVariable("NUMBER", Campaign.Current.Models.FleetManagementModel.MinimumTroopCountRequiredToSendShips);
			result = false;
		}
		else if (MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.PlayerSide != MobileParty.MainParty.MapEvent.WinningSide)
		{
			hint = GameTexts.FindText("str_action_disabled_reason_encounter", (string)null);
			result = false;
		}
		else
		{
			hint = new TextObject("{=iRfrlsB8}{NUMBER} troops will spend {DAYS} {?DAYS > 1}days{?}day{\\?} to deliver the ship and return to your party.", (Dictionary<string, object>)null);
			hint.SetTextVariable("NUMBER", Campaign.Current.Models.FleetManagementModel.MinimumTroopCountRequiredToSendShips);
			CampaignTime returnTimeForTroops = Campaign.Current.Models.FleetManagementModel.GetReturnTimeForTroops(ship);
			int num = MathF.Round(((CampaignTime)(ref returnTimeForTroops)).RemainingDaysFromNow);
			hint.SetTextVariable("DAYS", num);
		}
		return result;
	}
}
