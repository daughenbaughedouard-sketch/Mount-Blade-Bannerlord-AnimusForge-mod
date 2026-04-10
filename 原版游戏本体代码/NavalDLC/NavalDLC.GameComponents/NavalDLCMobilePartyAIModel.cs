using System;
using System.Collections.Generic;
using NavalDLC.Map;
using NavalDLC.Storyline;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace NavalDLC.GameComponents;

public class NavalDLCMobilePartyAIModel : MobilePartyAIModel
{
	private IPiratePatrolBehavior _piratePatrolBehavior;

	private IPiratePatrolBehavior PiratePatrolBehavior
	{
		get
		{
			if (_piratePatrolBehavior == null)
			{
				_piratePatrolBehavior = Campaign.Current.GetCampaignBehavior<IPiratePatrolBehavior>();
			}
			return _piratePatrolBehavior;
		}
	}

	public override float AiCheckInterval => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.AiCheckInterval;

	public override float FleeToNearbyPartyRadius => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.FleeToNearbyPartyRadius;

	public override float FleeToNearbySettlementRadius => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.FleeToNearbySettlementRadius;

	public override float HideoutPatrolDistanceAsDays => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.HideoutPatrolDistanceAsDays;

	public override float FortificationPatrolDistanceAsDays => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.FortificationPatrolDistanceAsDays;

	public override float FortificationPortPatrolDistanceAsDays => 0.5f;

	public override float VillagePatrolDistanceAsDays => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.VillagePatrolDistanceAsDays;

	public override float SettlementDefendingNearbyPartyCheckRadius => 20f;

	public override float SettlementDefendingWaitingPositionRadius => 3f;

	public override float NeededFoodsInDaysThresholdForSiege => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.NeededFoodsInDaysThresholdForSiege;

	public override float NeededFoodsInDaysThresholdForRaid => ((MBGameModel<MobilePartyAIModel>)this).BaseModel.NeededFoodsInDaysThresholdForRaid;

	public override float GetPatrolRadius(MobileParty mobileParty, CampaignVec2 patrolPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!patrolPoint.IsOnLand && ((CampaignVec2)(ref patrolPoint)).IsValid())
		{
			if (mobileParty.IsBandit && PiratePatrolBehavior != null)
			{
				return PiratePatrolBehavior.GetPatrolRadius(mobileParty);
			}
			if (mobileParty.IsLordParty)
			{
				if (!mobileParty.IsCurrentlyAtSea)
				{
					return 0f;
				}
				float num = 1f;
				if (mobileParty.TargetSettlement.MapFaction == mobileParty.MapFaction)
				{
					num = MBMath.Map(mobileParty.TargetSettlement.NearbyNavalThreatIntensity, 0f, 2f, 1f, 0.5f);
				}
				return ((MBGameModel<MobilePartyAIModel>)this).BaseModel.GetPatrolRadius(mobileParty, patrolPoint) * num;
			}
			if (mobileParty.IsPatrolParty)
			{
				return Campaign.Current.EstimatedAverageBanditPartyNavalSpeed * (float)CampaignTime.HoursInDay * 0.5f;
			}
		}
		return ((MBGameModel<MobilePartyAIModel>)this).BaseModel.GetPatrolRadius(mobileParty, patrolPoint);
	}

	public override float GetSettlementNearbyThreatAndAllyCheckRadius(Settlement settlement, bool isPort)
	{
		return ((MBGameModel<MobilePartyAIModel>)this).BaseModel.GetSettlementNearbyThreatAndAllyCheckRadius(settlement, isPort);
	}

	public override bool ShouldPartyCheckInitiativeBehavior(MobileParty mobileParty)
	{
		return ((MBGameModel<MobilePartyAIModel>)this).BaseModel.ShouldPartyCheckInitiativeBehavior(mobileParty);
	}

	public override void GetBestInitiativeBehavior(MobileParty mobileParty, out AiBehavior bestInitiativeBehavior, out MobileParty bestInitiativeTargetParty, out float bestInitiativeBehaviorScore, out Vec2 averageEnemyVec)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		((MBGameModel<MobilePartyAIModel>)this).BaseModel.GetBestInitiativeBehavior(mobileParty, ref bestInitiativeBehavior, ref bestInitiativeTargetParty, ref bestInitiativeBehaviorScore, ref averageEnemyVec);
		float num = (((int)mobileParty.ShortTermBehavior == 10 && mobileParty.ShortTermTargetParty == null) ? 0.7f : 0.5f);
		Storm storm = null;
		float num2 = float.MaxValue;
		CampaignVec2 position;
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			if (item.IsActive)
			{
				Vec2 currentPosition = item.CurrentPosition;
				position = mobileParty.Position;
				num2 = ((Vec2)(ref currentPosition)).Distance(((CampaignVec2)(ref position)).ToVec2());
				if (num2 < item.EffectRadius)
				{
					storm = item;
				}
			}
		}
		if (storm == null || !mobileParty.IsCurrentlyAtSea)
		{
			return;
		}
		float num3 = 1f - num2 / storm.EffectRadius;
		float num4 = LinQuick.SumQ<Ship>((List<Ship>)(object)mobileParty.Ships, (Func<Ship, float>)((Ship x) => x.HitPoints / x.MaxHitPoints)) / (float)((List<Ship>)(object)mobileParty.Ships).Count - num;
		if (num3 - num4 > 0f)
		{
			bestInitiativeBehaviorScore = 5f;
			bestInitiativeTargetParty = null;
			if (NavalDLCManager.Instance.GameModels.MapStormModel.CanPartyGetDamagedByStorm(mobileParty))
			{
				_ = NavalDLCManager.Instance.StormManager.DebugVisualsEnabled;
				Vec2 currentPosition2 = storm.CurrentPosition;
				position = mobileParty.Position;
				averageEnemyVec = currentPosition2 - ((CampaignVec2)(ref position)).ToVec2();
				bestInitiativeBehavior = (AiBehavior)10;
			}
			else if (mobileParty.CurrentSettlement != null)
			{
				bestInitiativeBehavior = (AiBehavior)0;
			}
		}
	}

	public override bool ShouldConsiderAttacking(MobileParty party, MobileParty targetParty)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (NavalStorylineData.IsNavalStoryLineActive() && (targetParty.IsNavalStorylineQuestParty() || targetParty.IsMainParty) && !party.IsBandit)
		{
			return false;
		}
		if (party.IsBandit && party.IsCurrentlyAtSea && Campaign.Current.Models.BanditDensityModel.IsPositionInsideNavalSafeZone(targetParty.Position))
		{
			return false;
		}
		return ((MBGameModel<MobilePartyAIModel>)this).BaseModel.ShouldConsiderAttacking(party, targetParty);
	}

	public override bool ShouldConsiderAvoiding(MobileParty party, MobileParty targetParty)
	{
		if (party.IsCurrentlyAtSea != targetParty.IsCurrentlyAtSea && party.CurrentSettlement != null)
		{
			return false;
		}
		return ((MBGameModel<MobilePartyAIModel>)this).BaseModel.ShouldConsiderAvoiding(party, targetParty);
	}
}
