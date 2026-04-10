using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.MissionLogics;
using SandBox.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Missions.AgentBehaviors;

public class AlarmedBehaviorGroup : AgentBehaviorGroup
{
	public const float SafetyDistance = 15f;

	public const float SafetyDistanceSquared = 225f;

	private const float NearbyDistanceThreshold = 1f;

	private const float NearbyDistanceThresholdSquared = 1f;

	private readonly MissionFightHandler _missionFightHandler;

	public bool DisableCalmDown;

	private readonly BasicMissionTimer _alarmedTimer;

	private readonly BasicMissionTimer _checkCalmDownTimer;

	public bool DoNotCheckForAlarmFactorIncrease;

	public bool DoNotIncreaseAlarmFactorDueToSeeingOrHearingTheEnemy;

	private bool _canMoveWhenCautious = true;

	private readonly MissionTimer _lastSuspiciousPositionTimer;

	private readonly MissionTimer _alarmYellTimer;

	private readonly List<Agent> _ignoredAgentsForAlarm;

	private readonly MBList<GameEntity> _stealthIndoorLightingAreas;

	private readonly MBList<StealthBox> _stealthBoxes;

	private MissionTime _lastAlarmTriggerTime;

	public float AlarmFactor { get; private set; }

	public AlarmedBehaviorGroup(AgentNavigator navigator, Mission mission)
		: base(navigator, mission)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		_alarmedTimer = new BasicMissionTimer();
		_checkCalmDownTimer = new BasicMissionTimer();
		_missionFightHandler = base.Mission.GetMissionBehavior<MissionFightHandler>();
		_lastSuspiciousPositionTimer = new MissionTimer(10f);
		_alarmYellTimer = new MissionTimer(10f);
		_ignoredAgentsForAlarm = new List<Agent>(0);
		_lastAlarmTriggerTime = MissionTime.Zero;
		base.Mission.OnAddSoundAlarmFactorToAgents += new OnAddSoundAlarmFactorToAgentsDelegate(OnAddSoundAlarmFactor);
		List<GameEntity> list = new List<GameEntity>();
		base.OwnerAgent.Mission.Scene.GetAllEntitiesWithScriptComponent<StealthIndoorLightingArea>(ref list);
		_stealthIndoorLightingAreas = new MBList<GameEntity>(list);
		List<GameEntity> source = new List<GameEntity>();
		base.OwnerAgent.Mission.Scene.GetAllEntitiesWithScriptComponent<StealthBox>(ref source);
		_stealthBoxes = new MBList<StealthBox>(source.Select((GameEntity ge) => ge.GetFirstScriptOfType<StealthBox>()));
	}

	public void SetCanMoveWhenCautious(bool value)
	{
		_canMoveWhenCautious = value;
	}

	private void UpdateAgentAlarmState(float dt)
	{
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_071c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Invalid comparison between Unknown and I4
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Invalid comparison between Unknown and I4
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Invalid comparison between Unknown and I4
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Invalid comparison between Unknown and I4
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Invalid comparison between Unknown and I4
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val4;
		if (!base.OwnerAgent.IsAlarmed())
		{
			bool flag = base.OwnerAgent.IsAIAtMoveDestination();
			if ((!base.OwnerAgent.IsCautious() || flag) && ((MissionTime)(ref _lastAlarmTriggerTime)).ElapsedSeconds > 2f)
			{
				float alarmFactor = AlarmFactor;
				AlarmFactor = Math.Max(0f, AlarmFactor - (base.OwnerAgent.IsPatrollingCautious() ? 0.025f : (_canMoveWhenCautious ? 0.125f : 0.08f)) * dt);
				if (alarmFactor >= 1f && AlarmFactor < 1f)
				{
					AlarmFactor = 0.3f;
				}
			}
			bool hasVisualOnEnemy = false;
			bool hasVisualOnCorpse = false;
			bool flag2 = false;
			if (!DoNotCheckForAlarmFactorIncrease)
			{
				Vec3 val;
				if (!base.OwnerAgent.IsHuman || !base.OwnerAgent.AgentVisuals.IsValid())
				{
					val = base.OwnerAgent.LookDirection;
				}
				else
				{
					MatrixFrame frame = base.OwnerAgent.Frame;
					ref Mat3 rotation = ref frame.rotation;
					MatrixFrame boneEntitialFrame = base.OwnerAgent.GetBoneEntitialFrame(base.OwnerAgent.Monster.HeadLookDirectionBoneIndex, true);
					val = ((Mat3)(ref rotation)).TransformToParent(ref boneEntitialFrame.rotation.f);
				}
				Vec3 val2 = val;
				WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
				Vec2 asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
				Vec2 val3 = ((Vec3)(ref val2)).AsVec2;
				((WorldPosition)(ref worldPosition)).SetVec2(asVec + ((Vec2)(ref val3)).Normalized() * 1.25f);
				float num = MBMath.ClampFloat(MathF.Tan(((WorldPosition)(ref worldPosition)).GetGroundVec3().z - base.OwnerAgent.Position.z) * 1f, -0.025f, 0.55f);
				val4 = Vec3.CrossProduct(Vec3.Up, val2);
				val2 = ((Vec3)(ref val2)).RotateAboutAnArbitraryVector(((Vec3)(ref val4)).NormalizedCopy(), 0.02f - num);
				foreach (Agent item in (List<Agent>)(object)base.OwnerAgent.Mission.AllAgents)
				{
					float num2 = 0f;
					float num3 = 0f;
					AgentState state = item.State;
					bool flag3 = item.AgentVisuals.IsValid();
					if ((int)state == 5 || (int)state == 2 || (int)state == 0 || !flag3)
					{
						continue;
					}
					AgentFlag agentFlags = item.GetAgentFlags();
					bool flag4 = _ignoredAgentsForAlarm.IndexOf(item) >= 0;
					if (item == base.OwnerAgent || !Extensions.HasAllFlags<AgentFlag>(agentFlags, (AgentFlag)2056) || (((int)state == 1 || flag4) && ((int)state != 1 || (!item.IsAlarmed() && (!item.IsPatrollingCautious() || flag4 || !(item.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().AlarmFactor > AlarmFactor + 0.1f)) && !base.OwnerAgent.IsEnemyOf(item)))))
					{
						continue;
					}
					if (!DoNotIncreaseAlarmFactorDueToSeeingOrHearingTheEnemy)
					{
						int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(item, DefaultSkills.Roguery);
						float equipmentStealthBonus = MissionGameModels.Current.AgentStatCalculateModel.GetEquipmentStealthBonus(item);
						float sneakingNoiseMultiplier = Math.Max(0f, 1f - ((float)effectiveSkill * 0.0001f + equipmentStealthBonus * 0.002f));
						num2 += GetSoundFactor(item, sneakingNoiseMultiplier);
					}
					num3 += GetVisualFactor(val2, item, (MBReadOnlyList<GameEntity>)(object)_stealthIndoorLightingAreas, ref hasVisualOnCorpse, ref hasVisualOnEnemy);
					float num4 = Math.Min(3f, num2 + num3);
					if (num4 > 0f && (!hasVisualOnEnemy || !DoNotIncreaseAlarmFactorDueToSeeingOrHearingTheEnemy))
					{
						AlarmFactor += num4 * dt * Campaign.Current.Models.DifficultyModel.GetStealthDifficultyMultiplier();
						if ((int)state == 1)
						{
							val4 = item.Position;
							if (((Vec3)(ref val4)).DistanceSquared(base.OwnerAgent.Position) < 1f)
							{
								flag2 = true;
							}
						}
						_lastAlarmTriggerTime = MissionTime.Now;
					}
					if (AlarmFactor >= 1f && base.OwnerAgent.IsAlarmStateNormal())
					{
						base.OwnerAgent.SetAlarmState((AIStateFlag)1);
						WorldPosition lastSuspiciousPosition = item.GetWorldPosition();
						Vec2 asVec2 = ((WorldPosition)(ref lastSuspiciousPosition)).AsVec2;
						val4 = base.OwnerAgent.Position;
						val3 = ((Vec3)(ref val4)).AsVec2 - ((WorldPosition)(ref lastSuspiciousPosition)).AsVec2;
						Vec2 val5 = ((Vec2)(ref val3)).Normalized();
						val4 = base.OwnerAgent.Position;
						val3 = ((Vec3)(ref val4)).AsVec2 - ((WorldPosition)(ref lastSuspiciousPosition)).AsVec2;
						((WorldPosition)(ref lastSuspiciousPosition)).SetVec2(asVec2 + val5 * ((((Vec2)(ref val3)).LengthSquared < 25f) ? 0f : 2f));
						SetAILastSuspiciousPositionHelper(in lastSuspiciousPosition, checkNavMeshForCorrection: true);
						_lastSuspiciousPositionTimer.Reset();
					}
					else if (num4 > 0f && (base.OwnerAgent.IsCautious() || base.OwnerAgent.IsPatrollingCautious()) && _lastSuspiciousPositionTimer.Check(true))
					{
						WorldPosition lastSuspiciousPosition2 = item.GetWorldPosition();
						Vec2 asVec3 = ((WorldPosition)(ref lastSuspiciousPosition2)).AsVec2;
						val4 = base.OwnerAgent.Position;
						val3 = ((Vec3)(ref val4)).AsVec2 - ((WorldPosition)(ref lastSuspiciousPosition2)).AsVec2;
						Vec2 val6 = ((Vec2)(ref val3)).Normalized();
						val4 = base.OwnerAgent.Position;
						val3 = ((Vec3)(ref val4)).AsVec2 - ((WorldPosition)(ref lastSuspiciousPosition2)).AsVec2;
						((WorldPosition)(ref lastSuspiciousPosition2)).SetVec2(asVec3 + val6 * ((((Vec2)(ref val3)).LengthSquared < 25f) ? 0f : 2f));
						SetAILastSuspiciousPositionHelper(in lastSuspiciousPosition2, checkNavMeshForCorrection: true);
					}
					if (num3 > 0f && base.OwnerAgent.IsPatrollingCautious() && (!item.IsActive() || (!item.IsEnemyOf(base.OwnerAgent) && !item.IsAlarmed())))
					{
						_ignoredAgentsForAlarm.Add(item);
					}
				}
			}
			if ((AlarmFactor >= 2f && (hasVisualOnEnemy || flag2)) || (AlarmFactor >= 1f && hasVisualOnEnemy && flag2))
			{
				base.OwnerAgent.SetAlarmState((AIStateFlag)3);
				_alarmYellTimer.Set(-9f);
				AlarmFactor = 2f;
			}
			else if (_canMoveWhenCautious && AlarmFactor >= 2f && base.OwnerAgent.IsCautious() && hasVisualOnCorpse)
			{
				base.OwnerAgent.SetAlarmState((AIStateFlag)2);
			}
			else if (AlarmFactor < 0.0001f)
			{
				base.OwnerAgent.SetAlarmState((AIStateFlag)0);
			}
			for (int num5 = _ignoredAgentsForAlarm.Count - 1; num5 >= 0; num5--)
			{
				Agent val7 = _ignoredAgentsForAlarm[num5];
				if (val7.IsActive() && (val7.IsAlarmStateNormal() || val7.IsAlarmed()))
				{
					_ignoredAgentsForAlarm.RemoveAt(num5);
				}
			}
			AlarmFactor = Math.Min(AlarmFactor, 2f);
		}
		else if (_alarmYellTimer.Check(true))
		{
			base.OwnerAgent.MakeVoice(VoiceType.Yell, (CombatVoiceNetworkPredictionType)2);
			Mission mission = base.OwnerAgent.Mission;
			Agent ownerAgent = base.OwnerAgent;
			val4 = base.OwnerAgent.Position + new Vec3(0f, 0f, base.OwnerAgent.GetEyeGlobalHeight(), -1f);
			mission.AddSoundAlarmFactorToAgents(ownerAgent, ref val4, 10f);
		}
	}

	private void SetAILastSuspiciousPositionHelper(in WorldPosition lastSuspiciousPosition, bool checkNavMeshForCorrection)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (_canMoveWhenCautious)
		{
			base.OwnerAgent.SetAILastSuspiciousPosition(lastSuspiciousPosition, checkNavMeshForCorrection);
			return;
		}
		WorldPosition worldPosition = base.OwnerAgent.GetWorldPosition();
		Vec2 asVec = ((WorldPosition)(ref worldPosition)).AsVec2;
		WorldPosition val = lastSuspiciousPosition;
		Vec2 asVec2 = ((WorldPosition)(ref val)).AsVec2;
		Vec3 position = base.OwnerAgent.Position;
		Vec2 val2 = asVec2 - ((Vec3)(ref position)).AsVec2;
		((WorldPosition)(ref worldPosition)).SetVec2(asVec + ((Vec2)(ref val2)).Normalized() * 0.1f);
		base.OwnerAgent.SetAILastSuspiciousPosition(worldPosition, false);
	}

	private float GetSoundFactor(Agent currentAgent, float sneakingNoiseMultiplier)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Invalid comparison between Unknown and I4
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Invalid comparison between Unknown and I4
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected I4, but got Unknown
		Vec3 val = currentAgent.Velocity;
		float num;
		float num2;
		if (((Vec3)(ref val)).LengthSquared > 0.010000001f)
		{
			Vec3 val2 = currentAgent.Position + new Vec3(0f, 0f, currentAgent.GetEyeGlobalHeight(), -1f) - (base.OwnerAgent.Position + new Vec3(0f, 0f, currentAgent.GetEyeGlobalHeight(), -1f));
			num = ((Vec3)(ref val2)).Normalize();
			val = currentAgent.AverageVelocity;
			num2 = 125f * Math.Min(1f, ((Vec3)(ref val)).Length / currentAgent.GetMaximumForwardUnlimitedSpeed());
			bool flag = false;
			Scene scene = currentAgent.Mission.Scene;
			val = currentAgent.Position;
			if (scene.GetWaterLevelAtPosition(((Vec3)(ref val)).AsVec2, !GameNetwork.IsMultiplayer, true) > currentAgent.Position.z)
			{
				BodyFlags val3 = default(BodyFlags);
				currentAgent.Mission.Scene.GetGroundHeightAndBodyFlagsAtPosition(currentAgent.Position, ref val3, (BodyFlags)542224777);
				if ((val3 & 0x40000010) != 16)
				{
					flag = true;
					num2 *= 4f;
				}
			}
			if (!currentAgent.HasMount)
			{
				CapsuleData collisionCapsule = currentAgent.CollisionCapsule;
				if (!(num <= ((CapsuleData)(ref collisionCapsule)).Radius * 2.5f))
				{
					if ((int)currentAgent.State == 1 && currentAgent.AgentVisuals.IsValid())
					{
						HumanWalkingMovementMode movementMode = currentAgent.AgentVisuals.GetMovementMode();
						switch (movementMode - 1)
						{
						case 0:
							num2 *= 0.7f;
							break;
						case 1:
							num2 *= (flag ? 0.45f : 0.25f);
							break;
						case 2:
							num2 *= (flag ? 0.1f : 0f);
							break;
						}
					}
					goto IL_019f;
				}
			}
			num2 *= 12f;
			goto IL_019f;
		}
		goto IL_01bf;
		IL_01bf:
		return 0f;
		IL_019f:
		num2 *= sneakingNoiseMultiplier;
		num2 /= 20f + num * num * 2.5f;
		if (num2 > 0.125f)
		{
			return num2;
		}
		goto IL_01bf;
	}

	public float GetVisualFactor(Vec3 usedGlobalLookDirection, Agent currentAgent, MBReadOnlyList<GameEntity> stealthIndoorLightingAreas, ref bool hasVisualOnCorpse, ref bool hasVisualOnEnemy)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = currentAgent.Position + new Vec3(0f, 0f, currentAgent.GetEyeGlobalHeight(), -1f) - (base.OwnerAgent.Position + new Vec3(0f, 0f, currentAgent.GetEyeGlobalHeight(), -1f));
		float num = 0f;
		float num2 = Vec3.DotProduct(val, usedGlobalLookDirection);
		bool flag = ((Vec3)(ref val)).LengthSquared < 1f;
		if (num2 > 0f && (flag || !IsAgentCoveredByAStealthBox(currentAgent)))
		{
			float distance = ((Vec3)(ref val)).Normalize();
			Vec3 velocity = currentAgent.Velocity;
			bool currentAgentHasSpeed = ((Vec3)(ref velocity)).LengthSquared > 0.010000001f;
			float equipmentStealthBonus = MissionGameModels.Current.AgentStatCalculateModel.GetEquipmentStealthBonus(currentAgent);
			float visualStrength = GetVisualStrength(val, usedGlobalLookDirection, currentAgent, currentAgentHasSpeed, distance, equipmentStealthBonus);
			if (visualStrength > 0.001f)
			{
				bool isDayTime = base.OwnerAgent.Mission.Scene.IsDayTime;
				Vec3 position = currentAgent.Position;
				float num3 = (isDayTime ? 0.7f : 0.2f);
				float num4 = (isDayTime ? 1f : 0.15f);
				foreach (GameEntity item in (List<GameEntity>)(object)stealthIndoorLightingAreas)
				{
					StealthIndoorLightingArea firstScriptOfType = item.GetFirstScriptOfType<StealthIndoorLightingArea>();
					if (((VolumeBox)firstScriptOfType).IsPointIn(position))
					{
						num3 = firstScriptOfType.AmbientLightStrength;
						num4 = firstScriptOfType.SunMoonLightStrength;
						break;
					}
				}
				float visualStrengthOfAgentVisual = base.OwnerAgent.AgentVisuals.GetVisualStrengthOfAgentVisual(currentAgent.AgentVisuals, base.OwnerAgent.Mission, num3, num4, base.OwnerAgent.Index);
				visualStrength *= visualStrengthOfAgentVisual;
				if (visualStrength > 0.3f)
				{
					num += visualStrength;
					if (!currentAgent.IsActive())
					{
						hasVisualOnCorpse = true;
					}
					else if (base.OwnerAgent.IsEnemyOf(currentAgent))
					{
						hasVisualOnEnemy = true;
						if (currentAgent != Agent.Main && Agent.Main != null && currentAgent.IsFriendOf(Agent.Main))
						{
							num *= 0.5f;
						}
					}
				}
			}
		}
		return num;
	}

	private float GetVisualStrength(Vec3 positionDifferenceDirection, Vec3 usedGlobalLookDirection, Agent currentAgent, bool currentAgentHasSpeed, float distance, float equipmentStealthBonus)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Invalid comparison between Unknown and I4
		float num = MathF.PI * 19f / 40f;
		float num2 = MathF.PI * 57f / 200f;
		Vec3 val = ((Vec3)(ref usedGlobalLookDirection)).CrossProductWithUp();
		val = ((Vec3)(ref val)).NormalizedCopy();
		Mat3 val2 = new Mat3(ref val, ref usedGlobalLookDirection, ref Vec3.Up);
		val2.u = Vec3.CrossProduct(val2.s, val2.f);
		Vec3 val3 = ((Mat3)(ref val2)).TransformToLocal(ref positionDifferenceDirection);
		float num3 = MathF.Atan2(val3.z, val3.x);
		float num4 = MathF.Acos(MBMath.ClampFloat(val3.y, 0f, 1f));
		float num5 = default(float);
		float num6 = default(float);
		MathF.SinCos(num3, ref num5, ref num6);
		float num7 = num * num2 / MathF.Sqrt(num * num * num5 * num5 + num2 * num2 * num6 * num6);
		float num8 = ((num4 >= num7) ? 0f : Math.Min(1f, 0.025f + (num7 - num4) / num7));
		num8 *= num8;
		if (!currentAgent.HasMount)
		{
			CapsuleData collisionCapsule = currentAgent.CollisionCapsule;
			if (!(distance <= ((CapsuleData)(ref collisionCapsule)).Radius * 6.5f))
			{
				if (currentAgent.AgentVisuals.IsValid() && currentAgent.CrouchMode)
				{
					num8 *= (currentAgentHasSpeed ? 0.9f : 0.8f);
				}
				goto IL_0124;
			}
		}
		num8 *= 15f;
		goto IL_0124;
		IL_0124:
		if ((int)currentAgent.State != 1 || currentAgent.IsAlarmed())
		{
			num8 *= 1.45f;
		}
		float num9 = Math.Max(0f, 1f - equipmentStealthBonus * 0.0025f);
		num8 *= 575f * num9;
		return num8 / (5f + distance * distance * 1.1f);
	}

	public void ResetAlarmFactor()
	{
		AlarmFactor = 0f;
	}

	private void AddAlarmFactor(float addedAlarmFactor, Agent suspiciousAgent)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		AlarmFactor += addedAlarmFactor;
		_lastAlarmTriggerTime = MissionTime.Now;
		if (AlarmFactor >= 1f && base.OwnerAgent.IsAlarmStateNormal())
		{
			base.OwnerAgent.SetAlarmState((AIStateFlag)1);
			if (suspiciousAgent != null)
			{
				SetAILastSuspiciousPositionHelper(suspiciousAgent.GetWorldPosition(), checkNavMeshForCorrection: true);
			}
			else
			{
				SetAILastSuspiciousPositionHelper(base.OwnerAgent.GetWorldPosition(), checkNavMeshForCorrection: false);
			}
			_lastSuspiciousPositionTimer.Reset();
		}
		else if ((base.OwnerAgent.IsCautious() || base.OwnerAgent.IsPatrollingCautious()) && _lastSuspiciousPositionTimer.Check(true))
		{
			if (suspiciousAgent != null)
			{
				SetAILastSuspiciousPositionHelper(suspiciousAgent.GetWorldPosition(), checkNavMeshForCorrection: true);
			}
			else
			{
				SetAILastSuspiciousPositionHelper(base.OwnerAgent.GetWorldPosition(), checkNavMeshForCorrection: false);
			}
		}
	}

	public void AddAlarmFactor(float addedAlarmFactor, in WorldPosition suspiciousPosition)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		AlarmFactor += addedAlarmFactor;
		_lastAlarmTriggerTime = MissionTime.Now;
		if (AlarmFactor >= 1f && base.OwnerAgent.IsAlarmStateNormal())
		{
			base.OwnerAgent.SetAlarmState((AIStateFlag)1);
			SetAILastSuspiciousPositionHelper(in suspiciousPosition, checkNavMeshForCorrection: true);
			_lastSuspiciousPositionTimer.Reset();
		}
		else if ((base.OwnerAgent.IsCautious() || base.OwnerAgent.IsPatrollingCautious()) && _lastSuspiciousPositionTimer.Check(true))
		{
			SetAILastSuspiciousPositionHelper(in suspiciousPosition, checkNavMeshForCorrection: true);
		}
	}

	public override void Tick(float dt, bool isSimulation)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (base.Mission.AllowAiTicking && base.OwnerAgent.IsAIControlled)
		{
			HandleMissiles(dt);
			if (Extensions.HasAllFlags<AgentFlag>(base.OwnerAgent.GetAgentFlags(), (AgentFlag)81920))
			{
				UpdateAgentAlarmState(dt);
			}
		}
		if (!base.IsActive)
		{
			return;
		}
		if (base.ScriptedBehavior != null)
		{
			if (!base.ScriptedBehavior.IsActive)
			{
				DisableAllBehaviors();
				base.ScriptedBehavior.IsActive = true;
			}
		}
		else
		{
			float num = 0f;
			int num2 = -1;
			for (int i = 0; i < Behaviors.Count; i++)
			{
				float availability = Behaviors[i].GetAvailability(isSimulation);
				if (availability > num)
				{
					num = availability;
					num2 = i;
				}
			}
			if (num > 0f && num2 != -1 && !Behaviors[num2].IsActive)
			{
				DisableAllBehaviors();
				Behaviors[num2].IsActive = true;
			}
		}
		TickActiveBehaviors(dt, isSimulation);
	}

	private void TickActiveBehaviors(float dt, bool isSimulation)
	{
		foreach (AgentBehavior behavior in Behaviors)
		{
			if (behavior.IsActive)
			{
				behavior.Tick(dt, isSimulation);
			}
		}
	}

	public override float GetScore(bool isSimulation)
	{
		if (base.OwnerAgent.IsAlarmed() || base.OwnerAgent.IsPatrollingCautious() || base.OwnerAgent.IsCautious())
		{
			if (!DisableCalmDown && _alarmedTimer.ElapsedTime > 10f && _checkCalmDownTimer.ElapsedTime > 1f)
			{
				_checkCalmDownTimer.Reset();
				if (!IsNearDanger())
				{
					base.OwnerAgent.DisableScriptedMovement();
				}
			}
			return 1f;
		}
		if (IsNearDanger())
		{
			AlarmAgent(base.OwnerAgent);
			return 1f;
		}
		return 0f;
	}

	private bool IsNearDanger()
	{
		float distanceSquared;
		Agent closestAlarmSource = GetClosestAlarmSource(out distanceSquared);
		if (closestAlarmSource != null)
		{
			if (!(distanceSquared < 225f))
			{
				return Navigator.CanSeeAgent(closestAlarmSource);
			}
			return true;
		}
		return false;
	}

	public Agent GetClosestAlarmSource(out float distanceSquared)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		distanceSquared = float.MaxValue;
		if (_missionFightHandler == null || !_missionFightHandler.IsThereActiveFight())
		{
			return null;
		}
		Agent result = null;
		foreach (Agent dangerSource in _missionFightHandler.GetDangerSources(base.OwnerAgent))
		{
			Vec3 position = dangerSource.Position;
			float num = ((Vec3)(ref position)).DistanceSquared(base.OwnerAgent.Position);
			if (num < distanceSquared)
			{
				distanceSquared = num;
				result = dangerSource;
			}
		}
		return result;
	}

	public static void AlarmAgent(Agent agent)
	{
		agent.SetWatchState((WatchState)2);
	}

	protected override void OnActivate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = new TextObject("{=!}{p0} {p1} activate alarmed behavior group.", (Dictionary<string, object>)null);
		val.SetTextVariable("p0", base.OwnerAgent.Name);
		val.SetTextVariable("p1", base.OwnerAgent.Index);
		_alarmedTimer.Reset();
		_checkCalmDownTimer.Reset();
		base.OwnerAgent.DisableScriptedMovement();
		base.OwnerAgent.ClearTargetFrame();
		Navigator.SetItemsVisibility(isVisible: false);
		if (CampaignMission.Current.Location != null)
		{
			LocationCharacter locationCharacter = CampaignMission.Current.Location.GetLocationCharacter(base.OwnerAgent.Origin);
			if (locationCharacter != null && locationCharacter.ActionSetCode != locationCharacter.AlarmedActionSetCode)
			{
				AnimationSystemData val2 = MonsterExtensions.FillAnimationSystemData(locationCharacter.GetAgentBuildData().AgentMonster, MBGlobals.GetActionSet(locationCharacter.AlarmedActionSetCode), ((BasicCharacterObject)locationCharacter.Character).GetStepSize(), false);
				base.OwnerAgent.SetActionSet(ref val2);
			}
		}
		if (Navigator.MemberOfAlley != null || MissionFightHandler.IsAgentAggressive(base.OwnerAgent))
		{
			DisableCalmDown = true;
		}
	}

	private void HandleMissiles(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		foreach (Missile item in (List<Missile>)(object)base.Mission.MissilesList)
		{
			Vec3 position = ((MBMissile)item).GetPosition();
			Vec3 velocity = ((MBMissile)item).GetVelocity();
			float num = ((Vec3)(ref velocity)).Length / 20f + 0.1f;
			float num2 = 0.1f;
			float num3 = 20f;
			float num4 = MathF.Sqrt(num * num / num2 - num3);
			if (!base.OwnerAgent.IsAlarmed() && base.OwnerAgent.IsActive() && base.OwnerAgent.IsAIControlled && Extensions.HasAnyFlag<AgentFlag>(base.OwnerAgent.GetAgentFlags(), (AgentFlag)65536) && base.OwnerAgent.RiderAgent == null && base.OwnerAgent != item.ShooterAgent)
			{
				Vec3 position2 = base.OwnerAgent.Position;
				position2.z += base.OwnerAgent.GetEyeGlobalHeight();
				Vec3 val = position + velocity;
				Vec3 closestPointOnLineSegmentToPoint = MBMath.GetClosestPointOnLineSegmentToPoint(ref position, ref val, ref position2);
				float num5 = ((Vec3)(ref closestPointOnLineSegmentToPoint)).DistanceSquared(position2);
				if (num5 < num4 * num4)
				{
					AddAlarmFactor(num * num / (num3 + num5) * dt, item.ShooterAgent);
				}
			}
		}
	}

	private void OnAddSoundAlarmFactor(Agent alarmCreatorAgent, in Vec3 soundPosition, float soundLevelSquareRoot)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (GameNetwork.IsClientOrReplay)
		{
			return;
		}
		float num = 0.7f;
		float num2 = 20f;
		float num3 = MathF.Sqrt(soundLevelSquareRoot * soundLevelSquareRoot / num - num2);
		if (base.OwnerAgent.IsActive() && !base.OwnerAgent.IsAlarmed() && base.OwnerAgent.IsAIControlled && Extensions.HasAnyFlag<AgentFlag>(base.OwnerAgent.GetAgentFlags(), (AgentFlag)65536) && base.OwnerAgent.RiderAgent == null && base.OwnerAgent != alarmCreatorAgent)
		{
			Vec3 position = base.OwnerAgent.Position;
			position.z += base.OwnerAgent.GetEyeGlobalHeight();
			Vec3 val = soundPosition;
			float num4 = ((Vec3)(ref val)).DistanceSquared(position);
			if (num4 < num3 * num3)
			{
				this.AddAlarmFactor(soundLevelSquareRoot * soundLevelSquareRoot / (num2 + num4), in ILSpyHelper_AsRefReadOnly(new WorldPosition(base.Mission.Scene, soundPosition)));
			}
		}
		static ref readonly T ILSpyHelper_AsRefReadOnly<T>(in T temp)
		{
			//ILSpy generated this function to help ensure overload resolution can pick the overload using 'in'
			return ref temp;
		}
	}

	public override void OnAgentRemoved(Agent agent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		if (agent == base.OwnerAgent)
		{
			base.Mission.OnAddSoundAlarmFactorToAgents -= new OnAddSoundAlarmFactorToAgentsDelegate(OnAddSoundAlarmFactor);
		}
	}

	protected override void OnDeactivate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		base.OnDeactivate();
		if (base.OwnerAgent.IsActive())
		{
			EquipmentIndex offhandWieldedItemIndex = base.OwnerAgent.GetOffhandWieldedItemIndex();
			if ((int)offhandWieldedItemIndex != -1 && (int)offhandWieldedItemIndex != 4)
			{
				base.Mission.AddTickAction((MissionTickAction)0, base.OwnerAgent, 1, 0);
			}
			base.Mission.AddTickAction((MissionTickAction)0, base.OwnerAgent, 0, 3);
			base.OwnerAgent.SetWatchState((WatchState)0);
			base.OwnerAgent.ResetLookAgent();
			base.OwnerAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
			base.OwnerAgent.SetActionChannel(1, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		}
	}

	public override void ForceThink(float inSeconds)
	{
	}

	private bool IsAgentCoveredByAStealthBox(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		MissionWeapon wieldedOffhandWeapon = agent.WieldedOffhandWeapon;
		ItemObject item = ((MissionWeapon)(ref wieldedOffhandWeapon)).Item;
		if (item != null && Extensions.HasAnyFlag<ItemFlags>(item.ItemFlags, (ItemFlags)1048576))
		{
			return false;
		}
		foreach (StealthBox item2 in (List<StealthBox>)(object)_stealthBoxes)
		{
			if (item2.IsAgentInside(agent) && (item2.CoversStandingAgents || agent.CrouchMode || !agent.IsActive()))
			{
				return true;
			}
		}
		return false;
	}

	public override void ConversationTick()
	{
		foreach (AgentBehavior behavior in Behaviors)
		{
			if (behavior.IsActive)
			{
				behavior.ConversationTick();
			}
		}
	}
}
