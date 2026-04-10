using System;
using System.Collections.Generic;
using NavalDLC.ComponentInterfaces;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace NavalDLC.GameComponents;

public class NavalDLCShipDistributionModel : ShipDistributionModel
{
	private const float CulturePenalty = 0.96f;

	public override bool CanPartyTakeShip(PartyBase party, Ship ship)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (party.IsMobile && party.MobileParty.IsBandit && (int)ship.ShipHull.Type == 2)
		{
			return false;
		}
		return true;
	}

	public override bool CanSendShipToParty(Ship ship, MobileParty mobileParty)
	{
		if (mobileParty != MobileParty.MainParty && mobileParty.IsActive && (!mobileParty.IsCurrentlyAtSea || mobileParty.MapEvent == null) && !mobileParty.IsDisbanding && !mobileParty.IsCaravan && !mobileParty.IsCurrentlyUsedByAQuest && !mobileParty.IsMilitia && !mobileParty.IsPatrolParty)
		{
			return !mobileParty.IsVillager;
		}
		return false;
	}

	public override float GetScoreForPartyShipComposition(MobileParty party, MBReadOnlyList<Ship> shipsToConsider)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected I4, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (((List<Ship>)(object)shipsToConsider).Count == 0)
		{
			return 0f;
		}
		float num = 1f;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		CultureObject val = null;
		if (party.ActualClan != null)
		{
			val = party.ActualClan.Culture;
		}
		else if (party.MapFaction != null)
		{
			val = party.MapFaction.Culture;
		}
		foreach (Ship item in (List<Ship>)(object)shipsToConsider)
		{
			if (!LinQuick.ContainsQ<ShipHull>((List<ShipHull>)(object)val.AvailableShipHulls, item.ShipHull))
			{
				num *= 0.96f;
			}
			ShipType type = item.ShipHull.Type;
			switch ((int)type)
			{
			case 0:
				num2++;
				break;
			case 1:
				num3++;
				break;
			case 2:
				num4++;
				break;
			}
		}
		if (num2 < 1)
		{
			num *= 0.85f;
		}
		if (num3 < 1)
		{
			num *= 0.9f;
		}
		if (num4 < 1)
		{
			num *= 0.95f;
		}
		int num5 = LinQuick.SumQ<Ship>((List<Ship>)(object)shipsToConsider, (Func<Ship, int>)((Ship x) => x.SkeletalCrewCapacity));
		ExplainedNumber partyMemberSizeLimit = Campaign.Current.Models.PartySizeLimitModel.GetPartyMemberSizeLimit(party.Party, false);
		int num6 = (int)((ExplainedNumber)(ref partyMemberSizeLimit)).ResultNumber;
		float num7 = (float)num6 * 0.5f;
		int idealShipNumber = Campaign.Current.Models.PartyShipLimitModel.GetIdealShipNumber(party);
		if (num7 < (float)num5)
		{
			float num8 = 1f - ((float)num5 - num7) / (float)num5 * 0.5f;
			num *= num8;
		}
		else if (((List<Ship>)(object)shipsToConsider).Count > idealShipNumber)
		{
			num *= 2f / (float)(((List<Ship>)(object)shipsToConsider).Count - idealShipNumber + 1);
		}
		int num9 = LinQuick.SumQ<Ship>((List<Ship>)(object)shipsToConsider, (Func<Ship, int>)((Ship x) => x.ShipHull.TotalCrewCapacity));
		if ((float)num9 < (float)num6 * 0.85f)
		{
			num *= (float)num9 / (float)num6 * 15f / 85f + 0.85f;
		}
		else if ((float)num9 > (float)num6 * 1.3f)
		{
			num *= 0.8f;
		}
		return num;
	}
}
