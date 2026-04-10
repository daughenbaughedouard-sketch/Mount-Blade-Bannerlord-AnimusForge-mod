using System;
using System.Collections.Generic;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class ShipCollisionOutcomeLogic : MissionLogic
{
	private const float EffectCooldownForShipInSeconds = 2f;

	private static readonly int _ramCollisionSoundEffectSoundId = SoundManager.GetEventGlobalIndex("event:/physics/vessel/ship_ramming");

	private readonly Mission _mission;

	private NavalShipsLogic _navalShipsLogic;

	private float _cameraShakeStartTime;

	private float _cameraShakeCurrentTimeWithFrequency;

	private float _cameraShakeIntensity;

	private Vec2 _cameraShakeInitialVelocity;

	private readonly Dictionary<MissionShip, float> _shipCollisionEffectCooldowns;

	private readonly Queue<(MissionShip, Vec3, Vec2, float)> _agentActionQueue;

	private MBFastRandom _effectRandom;

	public ShipCollisionOutcomeLogic(Mission mission)
	{
		_mission = mission;
		_shipCollisionEffectCooldowns = new Dictionary<MissionShip, float>();
		_agentActionQueue = new Queue<(MissionShip, Vec3, Vec2, float)>();
	}

	public override void OnBehaviorInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		((MissionBehavior)this).OnBehaviorInitialize();
		_effectRandom = new MBFastRandom();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.ShipRammingEvent += OnShipRamming;
		_navalShipsLogic.ShipCollisionEvent += OnShipCollision;
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		_navalShipsLogic.ShipRammingEvent -= OnShipRamming;
		_navalShipsLogic.ShipCollisionEvent -= OnShipCollision;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		while (_agentActionQueue.Count > 0)
		{
			(MissionShip, Vec3, Vec2, float) tuple = _agentActionQueue.Dequeue();
			HandleAgentActions(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}
		if (!(_cameraShakeStartTime > 0f))
		{
			return;
		}
		float currentTime = Mission.Current.CurrentTime;
		if (_cameraShakeStartTime > currentTime - 2f)
		{
			float num = 1f - MathF.Pow((currentTime - _cameraShakeStartTime) / 2f, 0.4f);
			float num2 = num * _cameraShakeIntensity * 0.6f;
			float num3 = num2 * 0.02f;
			_cameraShakeCurrentTimeWithFrequency += dt * 15f * num;
			if (num2 > 0f)
			{
				Vec3 val = MBPerlin.NoiseVec3(_cameraShakeCurrentTimeWithFrequency);
				float num4 = (currentTime - _cameraShakeStartTime) / 2f;
				_mission.SetCustomCameraLocalOffset2(new Vec3(val.x * num2, 0f, val.z * num2, -1f));
				_mission.SetCustomCameraGlobalOffset(new Vec3(_cameraShakeInitialVelocity * (9.821568f * num4 - 32.17632f * num4 * num4 + 41.68837f * num4 * num4 * num4 - 25.76999f * num4 * num4 * num4 * num4 + 6.436929f * num4 * num4 * num4 * num4 * num4), 0f, -1f));
				_mission.SetCustomCameraLocalRotationalOffset(new Vec3(val.x * num3, val.y * num3, 0f, -1f));
			}
		}
		else
		{
			_cameraShakeStartTime = 0f;
			_mission.SetCustomCameraLocalOffset2(Vec3.Zero);
			_mission.SetCustomCameraGlobalOffset(Vec3.Zero);
			_mission.SetCustomCameraLocalRotationalOffset(Vec3.Zero);
		}
	}

	private void OnShipRamming(MissionShip rammingShip, MissionShip rammedShip, float damagePercent, bool isFirstImpact, CapsuleData capsuleData, int ramQuality)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (isFirstImpact)
		{
			Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)rammingShip).GameEntity, ((CapsuleData)(ref capsuleData)).P2);
			Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody2 = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)rammedShip).GameEntity, ((CapsuleData)(ref capsuleData)).P2);
			Vec3 collisionDirection = linearVelocityAtGlobalPointForEntityWithDynamicBody - linearVelocityAtGlobalPointForEntityWithDynamicBody2;
			((Vec3)(ref collisionDirection)).Normalize();
			ShipCollisionEffect(rammingShip, ((ScriptComponentBehavior)rammedShip).GameEntity, ((CapsuleData)(ref capsuleData)).P2, collisionDirection, shouldMakeSound: false);
			ShipCollisionEffect(rammedShip, ((ScriptComponentBehavior)rammingShip).GameEntity, ((CapsuleData)(ref capsuleData)).P2, collisionDirection, shouldMakeSound: false);
		}
	}

	private void OnShipCollision(MissionShip ship, WeakGameEntity targetEntity, Vec3 averageContactPoint, Vec3 totalImpulseOnShip, bool isFirstImpact)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (isFirstImpact)
		{
			Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)ship).GameEntity, averageContactPoint);
			Vec3 val = ((!((WeakGameEntity)(ref targetEntity)).IsValid || !Extensions.HasAnyFlag<BodyFlags>(((WeakGameEntity)(ref targetEntity)).BodyFlag, (BodyFlags)40)) ? Vec3.Zero : GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(targetEntity, averageContactPoint));
			Vec3 val2 = linearVelocityAtGlobalPointForEntityWithDynamicBody - val;
			((Vec3)(ref val2)).Normalize();
			ShipCollisionEffect(ship, targetEntity, averageContactPoint, -val2, shouldMakeSound: true);
		}
	}

	private void ShipCollisionEffect(MissionShip ship, WeakGameEntity targetEntity, Vec3 collisionGlobalPosition, Vec3 collisionDirection, bool shouldMakeSound)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		float currentTime = Mission.Current.CurrentTime;
		if (_shipCollisionEffectCooldowns.TryGetValue(ship, out var value) && !(currentTime - value >= 2f))
		{
			return;
		}
		bool num = ((WeakGameEntity)(ref targetEntity)).IsValid && Extensions.HasAnyFlag<BodyFlags>(((WeakGameEntity)(ref targetEntity)).BodyFlag, (BodyFlags)40);
		Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(((ScriptComponentBehavior)ship).GameEntity, collisionGlobalPosition);
		Vec3 val = ((!num) ? Vec3.Zero : GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(targetEntity, collisionGlobalPosition));
		Vec3 val2 = linearVelocityAtGlobalPointForEntityWithDynamicBody - val;
		float num2 = ((Vec3)(ref val2)).Normalize();
		float num3 = (num ? GameEntityPhysicsExtensions.GetMass(targetEntity) : float.MaxValue);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
		float mass = ((WeakGameEntity)(ref gameEntity)).Mass;
		float num4 = 1f / mass + 1f / num3;
		float num5 = num2 * num2 * (1f / num4);
		float num6 = 0.15f * (num5 / mass);
		if (!(num6 >= 1f))
		{
			return;
		}
		_shipCollisionEffectCooldowns[ship] = currentTime;
		Vec3 linearVelocity = ship.Physics.LinearVelocity;
		Vec2 asVec = ((Vec3)(ref linearVelocity)).AsVec2;
		((Vec2)(ref asVec)).Normalize();
		Agent mainAgent = _mission.MainAgent;
		if (mainAgent != null && mainAgent.IsActive() && ship.GetIsAgentOnShip(_mission.MainAgent))
		{
			_cameraShakeStartTime = currentTime;
			_cameraShakeIntensity = MathF.Clamp(num6 * 0.3f, 1f, 3f);
			_cameraShakeInitialVelocity = asVec * num6 * 0.5f;
			_cameraShakeCurrentTimeWithFrequency = 0f;
		}
		MissionShip firstScriptOfType = ((WeakGameEntity)(ref targetEntity)).GetFirstScriptOfType<MissionShip>();
		shouldMakeSound = shouldMakeSound && (firstScriptOfType == null || !_shipCollisionEffectCooldowns.TryGetValue(firstScriptOfType, out value) || currentTime - value >= 2f);
		if (shouldMakeSound)
		{
			SoundEventParameter val3 = default(SoundEventParameter);
			((SoundEventParameter)(ref val3))._002Ector("Force", MathF.Min(num6 * 0.1f, 0.5f));
			MBSoundEvent.PlaySound(_ramCollisionSoundEffectSoundId, ref val3, collisionGlobalPosition);
		}
		_agentActionQueue.Enqueue((ship, collisionGlobalPosition, ((Vec3)(ref collisionDirection)).AsVec2, num6));
		MatrixFrame globalFrameImpreciseForFixedTick;
		foreach (ShipUnmannedOar item in (List<ShipUnmannedOar>)(object)ship.ShipUnmannedOars)
		{
			gameEntity = ((ScriptComponentBehavior)item).GameEntity;
			globalFrameImpreciseForFixedTick = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick();
			float num7 = ((Vec3)(ref globalFrameImpreciseForFixedTick.origin)).DistanceSquared(collisionGlobalPosition);
			if (num7 < 900f)
			{
				float num8 = num6 * 0.04f * (30f / (MathF.Sqrt(num7) + 0.1f)) * _effectRandom.NextFloat();
				if (num8 > 1f)
				{
					item.SetSlowDownPhaseForDuration(Math.Max(1f - num8 * 0.3f, 0f), Math.Min(num8, 3f));
				}
			}
		}
		foreach (ShipOarMachine item2 in (List<ShipOarMachine>)(object)ship.LeftSideShipOarMachines)
		{
			gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
			globalFrameImpreciseForFixedTick = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick();
			float num9 = ((Vec3)(ref globalFrameImpreciseForFixedTick.origin)).DistanceSquared(collisionGlobalPosition);
			if (num9 < 900f)
			{
				float num10 = num6 * 0.04f * (30f / (MathF.Sqrt(num9) + 0.1f)) * _effectRandom.NextFloat();
				if (num10 > 1f)
				{
					item2.SetSlowDownPhaseForDuration(Math.Max(1f - num10 * 0.3f, 0f), Math.Min(num10, 3f));
				}
			}
		}
		foreach (ShipOarMachine item3 in (List<ShipOarMachine>)(object)ship.RightSideShipOarMachines)
		{
			gameEntity = ((ScriptComponentBehavior)item3).GameEntity;
			globalFrameImpreciseForFixedTick = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick();
			float num11 = ((Vec3)(ref globalFrameImpreciseForFixedTick.origin)).DistanceSquared(collisionGlobalPosition);
			if (num11 < 900f)
			{
				float num12 = num6 * 0.04f * (30f / (MathF.Sqrt(num11) + 0.1f)) * _effectRandom.NextFloat();
				if (num12 > 1f)
				{
					item3.SetSlowDownPhaseForDuration(Math.Max(1f - num12 * 0.3f, 0f), Math.Min(num12, 3f));
				}
			}
		}
	}

	public void ActivateCooldownForShip(MissionShip ship, float cooldown)
	{
		float currentTime = Mission.Current.CurrentTime;
		if (!_shipCollisionEffectCooldowns.TryGetValue(ship, out var value) || currentTime - value > 0f - cooldown)
		{
			_shipCollisionEffectCooldowns[ship] = currentTime - (2f - cooldown);
		}
	}

	private void HandleAgentActions(MissionShip ship, Vec3 collisionGlobalPosition, Vec2 shipDirection, float impactFactor)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		foreach (Agent item in (List<Agent>)(object)_mission.Agents)
		{
			if (item.IsUsingGameObject && Extensions.HasAnyFlag<AnimFlags>(item.GetCurrentAnimationFlag(0), (AnimFlags)17592186044416L))
			{
				continue;
			}
			Vec3 position = item.Position;
			float num = ((Vec3)(ref position)).DistanceSquared(collisionGlobalPosition);
			if (!(num < 900f) || !ship.GetIsAgentOnShip(item))
			{
				continue;
			}
			int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(item, NavalSkills.Mariner);
			float num2 = impactFactor * 0.15f * (30f / (MathF.Sqrt(num) + 0.1f)) * (0.5f + _effectRandom.NextFloat() * 0.5f) * (100f / ((float)effectiveSkill + 100f));
			if (!(num2 > 1f))
			{
				continue;
			}
			ShipControllerMachine shipControllerMachine = ship.ShipControllerMachine;
			if (((shipControllerMachine != null) ? ((UsableMachine)shipControllerMachine).PilotAgent : null) == item)
			{
				num2 = Math.Min(num2, 2f);
			}
			Vec2 val = item.GetMovementDirection();
			float num3 = ((Vec2)(ref val)).DotProduct(shipDirection);
			if (num3 > 0.7f)
			{
				ActionIndexCache val2 = ((num2 >= 3f) ? ActionIndexCache.act_stagger_backward_3 : ((num2 >= 2f) ? ActionIndexCache.act_stagger_backward_2 : ActionIndexCache.act_stagger_backward));
				item.SetActionChannel(0, ref val2, false, (AnimFlags)0, 0f, _effectRandom.NextFloatRanged(0.7f, 1.3f), -0.2f, 0.4f, _effectRandom.NextFloatRanged(0f, 0.3f), false, -0.2f, 0, true);
				continue;
			}
			if (num3 < -0.7f)
			{
				ActionIndexCache val2 = ((num2 >= 3f) ? ActionIndexCache.act_stagger_forward_3 : ((num2 >= 2f) ? ActionIndexCache.act_stagger_forward_2 : ActionIndexCache.act_stagger_forward));
				item.SetActionChannel(0, ref val2, false, (AnimFlags)0, 0f, _effectRandom.NextFloatRanged(0.7f, 1.3f), -0.2f, 0.4f, _effectRandom.NextFloatRanged(0f, 0.3f), false, -0.2f, 0, true);
				continue;
			}
			val = item.GetMovementDirection();
			val = ((Vec2)(ref val)).RightVec();
			if (((Vec2)(ref val)).DotProduct(shipDirection) > 0f)
			{
				ActionIndexCache val2 = ((num2 >= 3f) ? ActionIndexCache.act_stagger_left_3 : ((num2 >= 2f) ? ActionIndexCache.act_stagger_left_2 : ActionIndexCache.act_stagger_left));
				item.SetActionChannel(0, ref val2, false, (AnimFlags)0, 0f, _effectRandom.NextFloatRanged(0.7f, 1.3f), -0.2f, 0.4f, _effectRandom.NextFloatRanged(0f, 0.3f), false, -0.2f, 0, true);
			}
			else
			{
				ActionIndexCache val2 = ((num2 >= 3f) ? ActionIndexCache.act_stagger_right_3 : ((num2 >= 2f) ? ActionIndexCache.act_stagger_right_2 : ActionIndexCache.act_stagger_right));
				item.SetActionChannel(0, ref val2, false, (AnimFlags)0, 0f, _effectRandom.NextFloatRanged(0.7f, 1.3f), -0.2f, 0.4f, _effectRandom.NextFloatRanged(0f, 0.3f), false, -0.2f, 0, true);
			}
		}
	}
}
