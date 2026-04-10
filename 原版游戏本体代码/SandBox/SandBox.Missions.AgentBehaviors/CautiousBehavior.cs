using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.AgentBehaviors;

public class CautiousBehavior : AgentBehavior
{
	private readonly Timer _waitTimer;

	public CautiousBehavior(AgentBehaviorGroup behaviorGroup)
		: base(behaviorGroup)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		_waitTimer = new Timer(base.Mission.CurrentTime, 10f, true);
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Invalid comparison between Unknown and I4
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (base.OwnerAgent.IsCautious())
		{
			if (base.OwnerAgent.IsAIAtMoveDestination())
			{
				base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_guard_cautious_look_around_1, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			else
			{
				base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_none, false, (AnimFlags)2, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			}
			if ((int)base.OwnerAgent.GetPrimaryWieldedItemIndex() != -1)
			{
				base.Mission.AddTickAction((MissionTickAction)0, base.OwnerAgent, 0, 1);
			}
			EquipmentIndex offhandWieldedItemIndex = base.OwnerAgent.GetOffhandWieldedItemIndex();
			if ((int)offhandWieldedItemIndex != -1 && (int)offhandWieldedItemIndex != 4)
			{
				base.Mission.AddTickAction((MissionTickAction)0, base.OwnerAgent, 1, 1);
			}
		}
		else if (base.OwnerAgent.IsPatrollingCautious())
		{
			bool num = base.OwnerAgent.IsAIAtMoveDestination();
			base.OwnerAgent.SetWeaponGuard((UsageDirection)3);
			if (num)
			{
				WorldPosition val = base.OwnerAgent.GetAIMoveDestination();
				Vec2 asVec = ((WorldPosition)(ref val)).AsVec2;
				val = base.OwnerAgent.GetAILastSuspiciousPosition();
				if (((Vec2)(ref asVec)).DistanceSquared(((WorldPosition)(ref val)).AsVec2) < 1f)
				{
					flag = false;
					if (_waitTimer.Check(base.Mission.CurrentTime))
					{
						_waitTimer.Reset(base.Mission.CurrentTime, MBRandom.RandomFloat * 4f + 8f);
						base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_none, false, (AnimFlags)2, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
						WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
						Vec2 asVec2 = ((WorldPosition)(ref worldPosition)).AsVec2;
						Vec2 movementDirection = base.OwnerAgent.GetMovementDirection();
						((Vec2)(ref movementDirection)).RotateCCW(MBRandom.RandomFloat * (MathF.PI * 2f));
						((WorldPosition)(ref worldPosition)).SetVec2(((WorldPosition)(ref worldPosition)).AsVec2 + movementDirection * base.OwnerAgent.Monster.BodyCapsuleRadius * MBRandom.RandomFloatRanged(20f, 35f));
						bool flag2 = default(bool);
						((WorldPosition)(ref worldPosition)).SetVec2(base.OwnerAgent.FindLongestDirectMoveToPosition(((WorldPosition)(ref worldPosition)).AsVec2, true, false, ref flag2));
						asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
						float num2 = ((Vec2)(ref asVec)).DistanceSquared(asVec2);
						if (num2 > base.OwnerAgent.Monster.BodyCapsuleRadius * base.OwnerAgent.Monster.BodyCapsuleRadius * 10f * 10f)
						{
							((WorldPosition)(ref worldPosition)).SetVec2(asVec2 + movementDirection * (MathF.Sqrt(num2) - base.OwnerAgent.Monster.BodyCapsuleRadius * 10f));
							base.OwnerAgent.SetAILastSuspiciousPosition(worldPosition, false);
						}
					}
					else
					{
						base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_guard_patrolling_cautious_look_around_1, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
					}
				}
			}
		}
		if (flag)
		{
			_waitTimer.Reset(base.Mission.CurrentTime);
		}
	}

	public override float GetAvailability(bool isSimulation)
	{
		if (!base.OwnerAgent.IsCautious() && !base.OwnerAgent.IsPatrollingCautious())
		{
			return 0f;
		}
		return 10f;
	}

	protected override void OnDeactivate()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		if (!base.OwnerAgent.IsAlarmed())
		{
			base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_none, false, (AnimFlags)2, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			if ((int)base.OwnerAgent.GetPrimaryWieldedItemIndex() != -1)
			{
				base.Mission.AddTickActionMT((MissionTickAction)0, base.OwnerAgent, 0, 0);
			}
			EquipmentIndex offhandWieldedItemIndex = base.OwnerAgent.GetOffhandWieldedItemIndex();
			if ((int)offhandWieldedItemIndex != -1 && (int)offhandWieldedItemIndex != 4)
			{
				base.Mission.AddTickActionMT((MissionTickAction)0, base.OwnerAgent, 1, 0);
			}
		}
	}

	protected override void OnActivate()
	{
		_waitTimer.Reset(base.Mission.CurrentTime);
	}

	public override string GetDebugInfo()
	{
		return string.Empty;
	}
}
