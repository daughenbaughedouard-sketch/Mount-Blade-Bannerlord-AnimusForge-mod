using System.Collections.Generic;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Missions.AI.UsableMachineAIs;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipControllerMachine : UsableMachine
{
	public const float CaptureTime = 3f;

	private const string ControllerEntityName = "controller";

	private const string HandTargetEntityName = "hand_position";

	private const string CameraTargetEntityName = "camera_target";

	private const string ShoulderCameraTargetEntityName = "shoulder_camera_target";

	private const string FrontCameraTargetEntityName = "front_camera_target";

	private const string RudderRotationEntityTag = "rudder_rotation_entity";

	private GameEntity _cameraTargetEntity;

	public GameEntity _rudderRotationEntity;

	private MatrixFrame _rudderRotationEntityInitialLocalFrame;

	private GameEntity _shoulderCameraTargetEntity;

	private GameEntity _frontCameraTargetEntity;

	private ActionIndexCache _shipControlActionPushLeftIndex = ActionIndexCache.act_none;

	private ActionIndexCache _shipControlActionPullRightIndex = ActionIndexCache.act_none;

	private ActionIndexCache _shipControlActionRelaxedIndex = ActionIndexCache.act_none;

	private ActionIndexCache _shipCaptureActionIndex = ActionIndexCache.act_none;

	private TextObject _overridenDescriptionForActiveEnemyShipControllerMachine;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	[EditableScriptComponentVariable(true, "")]
	private Vec3 _cameraOffset = new Vec3(0f, -20f, 5f, -1f);

	[EditableScriptComponentVariable(true, "")]
	private string _shipCaptureAction = "act_rudder_backward_stand_idle";

	[EditableScriptComponentVariable(true, "")]
	private string _shipControlActionTurnLeft = "act_rudder_backward_push_idle";

	[EditableScriptComponentVariable(true, "")]
	private string _shipControlActionTurnRight = "act_rudder_backward_pull_idle";

	[EditableScriptComponentVariable(true, "")]
	private string _shipControlActionRelaxed = "act_rudder_backward_stand_idle";

	[EditableScriptComponentVariable(true, "")]
	private bool _isRightHandOnly;

	[EditableScriptComponentVariable(true, "")]
	private Vec3 _shoulderCameraOffset = new Vec3(0f, 0f, 0f, -1f);

	[EditableScriptComponentVariable(true, "")]
	private bool _isLeftHandOnly;

	[EditableScriptComponentVariable(true, "")]
	private Vec3 _frontCameraOffset = new Vec3(0f, -10f, 2f, -1f);

	[EditableScriptComponentVariable(true, "")]
	private float _shoulderCameraDistance = 2f;

	[EditableScriptComponentVariable(true, "")]
	private float _frontCameraDistance = 10f;

	[EditableScriptComponentVariable(true, "")]
	private float _cameraFovMultiplier = 1f;

	[EditableScriptComponentVariable(true, "")]
	private float _shoulderCameraFovMultiplier = 1f;

	[EditableScriptComponentVariable(true, "")]
	private float _frontCameraFovMultiplier = 1f;

	private float _captureTimer = -1f;

	public GameEntity ControllerEntity { get; private set; }

	public MissionShip AttachedShip { get; private set; }

	public GameEntity HandTargetEntity { get; private set; }

	public Vec3 BackCameraOffset => _cameraOffset;

	public float CaptureTimer => _captureTimer;

	public Vec3 ShoulderCameraOffset => _shoulderCameraOffset;

	public Vec3 FrontCameraOffset => _frontCameraOffset;

	public float ShoulderCameraDistance => _shoulderCameraDistance;

	public float FrontCameraDistance => _frontCameraDistance;

	public float BackCameraFovMultiplier => _cameraFovMultiplier;

	public float ShoulderCameraFovMultiplier => _shoulderCameraFovMultiplier;

	public float FrontCameraFovMultiplier => _frontCameraFovMultiplier;

	public Vec3 BackCameraTargetLocalPosition
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			GameEntity cameraTargetEntity = _cameraTargetEntity;
			if (cameraTargetEntity == null)
			{
				return Vec3.Zero;
			}
			return cameraTargetEntity.GetFrame().origin;
		}
	}

	public Vec3 ShoulderCameraTargetLocalPosition
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			GameEntity shoulderCameraTargetEntity = _shoulderCameraTargetEntity;
			if (shoulderCameraTargetEntity == null)
			{
				return Vec3.Zero;
			}
			return shoulderCameraTargetEntity.GetFrame().origin;
		}
	}

	public Vec3 FrontCameraTargetLocalPosition
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			GameEntity frontCameraTargetEntity = _frontCameraTargetEntity;
			if (frontCameraTargetEntity == null)
			{
				return Vec3.Zero;
			}
			return frontCameraTargetEntity.GetFrame().origin;
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!((WeakGameEntity)(ref gameEntity)).IsGhostObject())
		{
			UpdateVisualizer();
		}
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		AttachedShip = ((WeakGameEntity)(ref val)).GetFirstScriptOfTypeInFamily<MissionShip>();
		val = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref val)).GetChildren())
		{
			WeakGameEntity current = child;
			if (((WeakGameEntity)(ref current)).Name == "controller")
			{
				ControllerEntity = GameEntity.CreateFromWeakEntity(current);
				_rudderRotationEntity = ControllerEntity;
				_rudderRotationEntityInitialLocalFrame = _rudderRotationEntity.GetFrame();
				foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref current)).GetChildren())
				{
					WeakGameEntity current2 = child2;
					if (((WeakGameEntity)(ref current2)).Name == "hand_position")
					{
						HandTargetEntity = GameEntity.CreateFromWeakEntity(current2);
					}
				}
			}
			else if (((WeakGameEntity)(ref current)).Name == "hand_position")
			{
				HandTargetEntity = GameEntity.CreateFromWeakEntity(current);
			}
			else if (((WeakGameEntity)(ref current)).Name == "camera_target")
			{
				_cameraTargetEntity = GameEntity.CreateFromWeakEntity(current);
			}
			else if (((WeakGameEntity)(ref current)).Name == "shoulder_camera_target")
			{
				_shoulderCameraTargetEntity = GameEntity.CreateFromWeakEntity(current);
			}
			else if (((WeakGameEntity)(ref current)).Name == "front_camera_target")
			{
				_frontCameraTargetEntity = GameEntity.CreateFromWeakEntity(current);
			}
		}
		if (_rudderRotationEntity == (GameEntity)null)
		{
			List<WeakGameEntity> list = new List<WeakGameEntity>();
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			((WeakGameEntity)(ref val)).GetChildrenWithTagRecursive(list, "rudder_rotation_entity");
			foreach (WeakGameEntity item in list)
			{
				_rudderRotationEntity = GameEntity.CreateFromWeakEntity(item);
				_rudderRotationEntityInitialLocalFrame = _rudderRotationEntity.GetFrame();
			}
		}
		_shipControlActionPushLeftIndex = ActionIndexCache.Create(_shipControlActionTurnLeft);
		_shipControlActionPullRightIndex = ActionIndexCache.Create(_shipControlActionTurnRight);
		_shipControlActionRelaxedIndex = ActionIndexCache.Create(_shipControlActionRelaxed);
		_shipCaptureActionIndex = ActionIndexCache.Create(_shipCaptureAction);
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		base.EnemyRangeToStopUsing = 5f;
	}

	public bool CheckControllerMachineFlags(bool editMode)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref val)).GetChildrenRecursive(ref list);
		bool flag = false;
		list.Add(((ScriptComponentBehavior)this).GameEntity);
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			if (!Extensions.HasAnyFlag<EntityFlags>(((WeakGameEntity)(ref current)).EntityFlags, (EntityFlags)131072) && !Extensions.HasAnyFlag<EntityFlags>(((WeakGameEntity)(ref current)).EntityFlags, (EntityFlags)4096))
			{
				flag = true;
			}
		}
		if (flag)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Root;
			string name = ((WeakGameEntity)(ref val)).Name;
			val = ((ScriptComponentBehavior)this).GameEntity;
			string text = $"In Root Entity {name}, {((WeakGameEntity)(ref val)).Name}'s every descendant including itself must have Does not Affect Parent's Local Bounding Box flag.";
			if (editMode)
			{
				MBEditor.AddEntityWarning(((ScriptComponentBehavior)this).GameEntity, text);
			}
		}
		return flag;
	}

	public override void OnDeploymentFinished()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		EnsureStandingPointComponents();
		if (AttachedShip.BattleSide != Mission.Current.PlayerTeam.Side)
		{
			((UsableMachine)this).PilotStandingPoint.SetUsableByAIOnly();
		}
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
	}

	private void EnsureStandingPointComponents()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).GetComponent<ResetAnimationOnStopUsageComponent>() == null)
		{
			((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none, false));
			((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new ClearHandInverseKinematicsOnStopUsageComponent());
			((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)(object)((NavalDLCManager.Instance.NavalPerks != null) ? new UserDamageCalculateComponent(NavalPerks.Shipmaster.TheHelmsmansShield, isPrimaryBonus: true, -0.6f) : new UserDamageCalculateComponent(null, isPrimaryBonus: false, -0.6f)));
		}
	}

	public override void OnPilotAssignedDuringSpawn()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		EnsureStandingPointComponents();
		ActionIndexCache animationBlendsWithActionIndex = MBAnimation.GetAnimationBlendsWithActionIndex(MBActionSet.GetAnimationIndexOfAction(((UsableMachine)this).PilotAgent.ActionSet, ref _shipControlActionRelaxedIndex));
		bool flag = ((ActionIndexCache)(ref animationBlendsWithActionIndex)).Index >= 0;
		((UsableMachine)this).PilotAgent.SetActionChannel(1, ref _shipControlActionRelaxedIndex, false, (AnimFlags)71, flag ? 0.5f : 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)((UsableMachine)this).PilotStandingPoint).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		((UsableMachine)this).PilotAgent.TeleportToPosition(globalFrame.origin);
		((UsableMachine)this).PilotAgent.DisableScriptedMovement();
		Agent pilotAgent = ((UsableMachine)this).PilotAgent;
		Vec2 val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		pilotAgent.SetMovementDirection(ref val);
	}

	protected override void OnTick(float dt)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnTick(dt);
		if (_rudderRotationEntity != (GameEntity)null)
		{
			MatrixFrame rudderRotationEntityInitialLocalFrame = _rudderRotationEntityInitialLocalFrame;
			((Mat3)(ref rudderRotationEntityInitialLocalFrame.rotation)).RotateAboutUp(AttachedShip.VisualRudderRotation);
			_rudderRotationEntity.SetLocalFrame(ref rudderRotationEntityInitialLocalFrame, false);
		}
		if (_navalShipsLogic != null)
		{
			Agent main = Agent.Main;
			if (((main == null) ? null : main.Formation?.Team) != null && AttachedShip.BattleSide != Agent.Main.Formation.Team.Side)
			{
				((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).IsDisabledForPlayers = !AttachedShip.CanBeTakenOver || !IsAttachedShipVacant() || !MissionShip.AreShipsConnected(_navalShipsLogic.GetShipAssignment(Agent.Main.Formation.Team.TeamSide, Agent.Main.Formation.FormationIndex).MissionShip, AttachedShip);
			}
		}
		if (((UsableMachine)this).PilotAgent == null)
		{
			_captureTimer = -1f;
		}
		if (((UsableMachine)this).PilotAgent == null)
		{
			return;
		}
		if (IsAttachedShipVacant() && ((UsableMachine)this).PilotAgent.Formation != null)
		{
			MissionShip missionShip = _navalShipsLogic.GetShipAssignment(((UsableMachine)this).PilotAgent.Formation.Team.TeamSide, ((UsableMachine)this).PilotAgent.Formation.FormationIndex).MissionShip;
			if (MissionShip.AreShipsConnected(missionShip, AttachedShip))
			{
				if (!((UsableMachine)this).PilotAgent.SetActionChannel(0, ref _shipCaptureActionIndex, false, (AnimFlags)0, 0.5f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
				{
					return;
				}
				if (_captureTimer > 0f)
				{
					_captureTimer -= dt;
					if (_captureTimer <= 0f)
					{
						Agent pilotAgent = ((UsableMachine)this).PilotAgent;
						((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
						OnShipCapturedByAgent(pilotAgent);
						missionShip.InvalidateActiveFormationTroopOnShipCache();
						AttachedShip.InvalidateActiveFormationTroopOnShipCache();
					}
				}
				else
				{
					_captureTimer = 3f;
				}
			}
			else
			{
				_captureTimer = -1f;
				((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
			return;
		}
		float visualRudderRotationPercentage = AttachedShip.VisualRudderRotationPercentage;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		float num = visualRudderRotationPercentage * (float)MathF.Sign(((WeakGameEntity)(ref gameEntity)).GetGlobalScale().x);
		num = MBMath.Map(num, -1f, 1f, 0.95f, 0.05f);
		ActionIndexCache val = ((AttachedShip.VisualRudderPullDirection == 0f) ? _shipControlActionRelaxedIndex : ((!(AttachedShip.VisualRudderPullDirection > 0f)) ? _shipControlActionPushLeftIndex : _shipControlActionPullRightIndex));
		int animationIndexOfAction = MBActionSet.GetAnimationIndexOfAction(((UsableMachine)this).PilotAgent.ActionSet, ref val);
		bool flag = MBAnimation.GetAnimationBlendsWithActionIndex(animationIndexOfAction) != ActionIndexCache.act_none;
		AnimFlags val2 = (AnimFlags)17592202822215L;
		if (((UsableMachine)this).PilotAgent.SetActionChannel(1, ref val, false, val2, flag ? num : 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
		{
			if (!(HandTargetEntity != (GameEntity)null))
			{
				return;
			}
			Vec3 origin = HandTargetEntity.GetGlobalFrame().origin;
			float currentActionProgress = ((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1);
			MatrixFrame frame = ((UsableMachine)this).PilotAgent.Frame;
			MBAgentVisuals agentVisuals = ((UsableMachine)this).PilotAgent.AgentVisuals;
			MatrixFrame boneEntitialFrame = agentVisuals.GetBoneEntitialFrame(((UsableMachine)this).PilotAgent.Monster.MainHandBoneIndex, false);
			MatrixFrame boneEntitialFrame2 = agentVisuals.GetBoneEntitialFrame(((UsableMachine)this).PilotAgent.Monster.OffHandBoneIndex, false);
			MatrixFrame boneEntitialFrameAtAnimationProgress = ((UsableMachine)this).PilotAgent.GetBoneEntitialFrameAtAnimationProgress(((UsableMachine)this).PilotAgent.Monster.MainHandBoneIndex, animationIndexOfAction, currentActionProgress);
			MatrixFrame boneEntitialFrameAtAnimationProgress2 = ((UsableMachine)this).PilotAgent.GetBoneEntitialFrameAtAnimationProgress(((UsableMachine)this).PilotAgent.Monster.OffHandBoneIndex, animationIndexOfAction, currentActionProgress);
			Vec3 val3 = ((MatrixFrame)(ref frame)).TransformToParent(ref boneEntitialFrameAtAnimationProgress.origin);
			Vec3 val4 = ((MatrixFrame)(ref frame)).TransformToParent(ref boneEntitialFrameAtAnimationProgress2.origin);
			float num2 = MathF.Clamp(dt * 15f, 0f, 1f);
			MatrixFrame val5 = default(MatrixFrame);
			val5.origin = boneEntitialFrameAtAnimationProgress.origin;
			val5.rotation = Mat3.SlerpFPSIndependent(ref boneEntitialFrame.rotation, ref boneEntitialFrameAtAnimationProgress.rotation, num2);
			MatrixFrame val6 = default(MatrixFrame);
			val6.origin = boneEntitialFrameAtAnimationProgress2.origin;
			val6.rotation = Mat3.SlerpFPSIndependent(ref boneEntitialFrame2.rotation, ref boneEntitialFrameAtAnimationProgress2.rotation, num2);
			MatrixFrame val7 = ((MatrixFrame)(ref frame)).TransformToParent(ref val5);
			MatrixFrame val8 = ((MatrixFrame)(ref frame)).TransformToParent(ref val6);
			MatrixFrame val9;
			if (_isLeftHandOnly)
			{
				val8.origin = origin;
				Agent pilotAgent2 = ((UsableMachine)this).PilotAgent;
				val9 = MatrixFrame.Identity;
				pilotAgent2.SetHandInverseKinematicsFrame(ref val8, ref val9);
				return;
			}
			if (_isRightHandOnly)
			{
				val7.origin = origin;
				Agent pilotAgent3 = ((UsableMachine)this).PilotAgent;
				val9 = MatrixFrame.Identity;
				pilotAgent3.SetHandInverseKinematicsFrame(ref val9, ref val7);
				return;
			}
			Vec3 val10;
			if (!(ControllerEntity != (GameEntity)null))
			{
				gameEntity = ((ScriptComponentBehavior)((UsableMachine)this).PilotStandingPoint).GameEntity;
				val9 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				val10 = ((Vec3)(ref val9.rotation.s)).NormalizedCopy();
			}
			else
			{
				val9 = ControllerEntity.GetGlobalFrame();
				val10 = ((Vec3)(ref val9.rotation.s)).NormalizedCopy();
			}
			Vec3 val11 = val10;
			float num3 = Vec3.DotProduct(val11, val3 - val4);
			val7.origin = origin + 0.5f * num3 * val11;
			val8.origin = origin - 0.5f * num3 * val11;
			((UsableMachine)this).PilotAgent.SetHandInverseKinematicsFrame(ref val8, ref val7);
		}
		else if (((UsableMachine)this).PilotAgent.IsInBeingStruckAction)
		{
			((UsableMachine)this).PilotAgent.ClearHandInverseKinematics();
		}
		else
		{
			((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
		}
	}

	private void OnShipCapturedByAgent(Agent captorAgent)
	{
		_navalShipsLogic?.OnShipCaptured(AttachedShip, captorAgent.Formation);
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		TextObject val = new TextObject("{=!}{KEY}", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	protected override float GetDetachmentWeightAux(BattleSideEnum side)
	{
		return float.MinValue;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (AttachedShip.BattleSide == Mission.Current.PlayerTeam.Side)
		{
			return new TextObject("{=OGY9BKOM}Control the Ship", (Dictionary<string, object>)null);
		}
		if (AttachedShip.CanBeTakenOver)
		{
			if (IsAttachedShipVacant())
			{
				MissionShip missionShip = null;
				if (_navalShipsLogic != null && Agent.Main.Formation?.Team != null)
				{
					missionShip = _navalShipsLogic.GetShipAssignment(Agent.Main.Formation.Team.TeamSide, Agent.Main.Formation.FormationIndex)?.MissionShip;
				}
				if (missionShip != null && MissionShip.AreShipsConnected(missionShip, AttachedShip))
				{
					return new TextObject("{=fOX1aVDv}Capture the ship", (Dictionary<string, object>)null);
				}
				if (!(_overridenDescriptionForActiveEnemyShipControllerMachine != (TextObject)null))
				{
					return new TextObject("{=lS53LgyN}You need to be boarded to capture the ship", (Dictionary<string, object>)null);
				}
				return _overridenDescriptionForActiveEnemyShipControllerMachine;
			}
			return new TextObject("{=UrBktTYi}Clear the crew to capture the ship", (Dictionary<string, object>)null);
		}
		return null;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new ShipControllerMachineAI(this);
	}

	private void UpdateVisualizer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity val = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("visualizer");
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		StandingPoint firstScriptOfTypeRecursive = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfTypeRecursive<StandingPoint>();
		bool flag = false;
		if (_shipControlActionRelaxedIndex == ActionIndexCache.act_none || ((ActionIndexCache)(ref _shipControlActionRelaxedIndex)).GetName() != _shipControlActionRelaxed)
		{
			_shipControlActionRelaxedIndex = ActionIndexCache.Create(_shipControlActionRelaxed);
			if (_shipControlActionRelaxedIndex != ActionIndexCache.act_none)
			{
				flag = MBAnimation.GetAnimationBlendsWithActionIndex(MBActionSet.GetAnimationIndexOfAction(MBActionSet.GetActionSetWithIndex(0), ref _shipControlActionRelaxedIndex)) != ActionIndexCache.act_none;
			}
		}
		if (_shipControlActionRelaxedIndex != ActionIndexCache.act_none && firstScriptOfTypeRecursive != null)
		{
			_ = ((ScriptComponentBehavior)firstScriptOfTypeRecursive).GameEntity;
			if (!((WeakGameEntity)(ref val)).IsValid)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				GameEntity val2 = GameEntity.CreateEmpty(((WeakGameEntity)(ref gameEntity)).Scene, false, true, true);
				val = val2.WeakEntity;
				((WeakGameEntity)(ref val)).SetEntityFlags((EntityFlags)(((WeakGameEntity)(ref val)).EntityFlags | 0x20000));
				((WeakGameEntity)(ref val)).SetName("visualizer");
				((WeakGameEntity)(ref val)).AddTag("visualizer");
				MBActionSet actionSetWithIndex = MBActionSet.GetActionSetWithIndex(0);
				GameEntityExtensions.CreateAgentSkeleton(val, "human_skeleton", true, actionSetWithIndex, "human", MBObjectManager.Instance.GetObject<Monster>("human"));
				MBSkeletonExtensions.SetAgentActionChannel(((WeakGameEntity)(ref val)).Skeleton, 0, ref _shipControlActionRelaxedIndex, 0f, 0f, true, flag ? 0.5f : 0f);
				((WeakGameEntity)(ref val)).AddMultiMeshToSkeleton(MetaMesh.GetCopy("roman_cloth_tunic_a", true, false));
				((WeakGameEntity)(ref val)).AddMultiMeshToSkeleton(MetaMesh.GetCopy("casual_02_boots", true, false));
				((WeakGameEntity)(ref val)).AddMultiMeshToSkeleton(MetaMesh.GetCopy("hands_male_a", true, false));
				((WeakGameEntity)(ref val)).AddMultiMeshToSkeleton(MetaMesh.GetCopy("head_male_a", true, false));
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).AddChild(val2.WeakEntity, false);
			}
		}
		if (((WeakGameEntity)(ref val)).IsValid)
		{
			gameEntity = ((ScriptComponentBehavior)firstScriptOfTypeRecursive).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			((WeakGameEntity)(ref val)).SetGlobalFrame(ref globalFrame, true);
			if (MBSkeletonExtensions.GetActionAtChannel(((WeakGameEntity)(ref val)).Skeleton, 0) != _shipControlActionRelaxedIndex)
			{
				MBSkeletonExtensions.SetAgentActionChannel(((WeakGameEntity)(ref val)).Skeleton, 0, ref _shipControlActionRelaxedIndex, 0f, 0f, true, flag ? 0.5f : 0f);
			}
		}
	}

	public override bool ShouldAutoLeaveDetachmentWhenDisabled(BattleSideEnum sideEnum)
	{
		return false;
	}

	public bool IsAttachedShipVacant()
	{
		if (AttachedShip.Formation != null)
		{
			if (!AttachedShip.AnyActiveFormationTroopOnShip)
			{
				NavalAgentsLogic navalAgentsLogic = _navalAgentsLogic;
				if (navalAgentsLogic == null)
				{
					return false;
				}
				return navalAgentsLogic.GetReservedTroopsCountOfShip(AttachedShip) <= 0;
			}
			return false;
		}
		return true;
	}

	public override void OnMissionEnded()
	{
	}

	public void SetOverridenDescriptionForActiveEnemyShipControllerMachine(TextObject description)
	{
		_overridenDescriptionForActiveEnemyShipControllerMachine = description;
	}
}
