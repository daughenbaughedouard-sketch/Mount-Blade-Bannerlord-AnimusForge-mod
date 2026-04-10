using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace NavalDLC.GameComponents;

public class NavalDLCPartyTransitionModel : PartyTransitionModel
{
	private const float MinHoursToMoveAnchor = 3f;

	private const float MaxHoursToMoveAnchor = 48f;

	private const float AnchorMoveSpeedPerHour = 35f;

	private const float DisembarkHours = 2f;

	private const float InstantEmbarkDistanceThresholdForAI = 10f;

	public override CampaignTime GetTransitionTimeForEmbarking(MobileParty mobileParty)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!mobileParty.Anchor.IsValid)
		{
			return CampaignTime.Hours(48f);
		}
		float distance;
		if (mobileParty.CurrentSettlement == null)
		{
			MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
			CampaignVec2 interactionPosition = mobileParty.Anchor.GetInteractionPosition(mobileParty);
			float num = default(float);
			distance = mapDistanceModel.GetDistance(mobileParty, ref interactionPosition, (NavigationType)1, ref num);
		}
		else
		{
			MapDistanceModel mapDistanceModel2 = Campaign.Current.Models.MapDistanceModel;
			Settlement currentSettlement = mobileParty.CurrentSettlement;
			CampaignVec2 position = mobileParty.Anchor.Position;
			distance = mapDistanceModel2.GetDistance(currentSettlement, ref position, true, (NavigationType)2);
		}
		float num2 = distance;
		if (num2 < 10f)
		{
			return CampaignTime.Zero;
		}
		return CampaignTime.Hours(GetAnchorReachDurationInHours(num2));
	}

	public override CampaignTime GetTransitionTimeDisembarking(MobileParty mobileParty)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		CampaignTime result = CampaignTime.Zero;
		if (!mobileParty.IsInRaftState)
		{
			result = CampaignTime.Hours(2f);
			if (mobileParty.HasPerk(NavalPerks.Shipmaster.Unflinching, false))
			{
				float num = NavalPerks.Shipmaster.Unflinching.PrimaryBonus * 100f;
				float num2 = (0f - num * 100f) / (100f + num);
				result = CampaignTime.Hours((float)((CampaignTime)(ref result)).ToHours * num2);
			}
		}
		return result;
	}

	public override CampaignTime GetFleetTravelTimeToSettlement(MobileParty mobileParty, Settlement targetSettlement)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		AnchorPoint anchor = mobileParty.Anchor;
		CampaignVec2 val = anchor.Position;
		if (((CampaignVec2)(ref val)).IsValid() || anchor.IsMovingToPoint)
		{
			float num;
			if (!anchor.IsMovingToPoint)
			{
				num = 0f;
			}
			else
			{
				CampaignTime val2 = anchor.ArrivalTime - CampaignTime.Now;
				num = (float)((CampaignTime)(ref val2)).ToHours;
			}
			float currentTravelTime = num;
			MapDistanceModel mapDistanceModel = Campaign.Current.Models.MapDistanceModel;
			val = anchor.Position;
			val = (((CampaignVec2)(ref val)).IsValid() ? anchor.Position : anchor.TargetPosition);
			float distance = mapDistanceModel.GetDistance(targetSettlement, ref val, true, (NavigationType)2);
			return CampaignTime.Hours(GetAnchorReachDurationInHours(distance, currentTravelTime));
		}
		CampaignTime result = CampaignTime.Hours(48f);
		if (mobileParty.HasPerk(NavalPerks.Shipmaster.ShoreMaster, false))
		{
			result = CampaignTime.Hours((float)((CampaignTime)(ref result)).ToHours * NavalPerks.Shipmaster.ShoreMaster.PrimaryBonus * -1f);
		}
		return result;
	}

	private float GetAnchorReachDurationInHours(float distance, float currentTravelTime = 0f)
	{
		distance = MathF.Pow(distance, 0.95f);
		return MBMath.ClampFloat(distance / 35f + currentTravelTime, 3f, 48f);
	}
}
