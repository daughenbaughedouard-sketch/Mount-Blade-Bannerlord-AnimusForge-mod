using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.AI.UsableMachineAIs;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.NavalPhysics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects.Usables;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class ShipAttachmentMachine : UsableMachine
{
	public class ShipBridgeNavmeshHolder : MissionObject
	{
		private const float StepWidth = 0.8f;

		private Vec3 _startLeftPosition;

		private Vec3 _startRightPosition;

		private Vec3 _endLeftPosition;

		private Vec3 _endRightPosition;

		private int[] _customVertexIndices;

		private Vec3[] _bridgeCustomVertexPositionsArray;

		private PathFaceRecord _face1PathFaceRecord;

		private PathFaceRecord _face2PathFaceRecord;

		private Vec3 _rightVector;

		private Vec3 _leftVector;

		private int _attachedFaceCount;

		public int BridgeNavmeshId { get; private set; }

		public ShipAttachment CurrentAttachment { get; private set; }

		public int GetFace1GroupIndex()
		{
			return _face1PathFaceRecord.FaceGroupIndex;
		}

		public int GetFace2GroupIndex()
		{
			return _face2PathFaceRecord.FaceGroupIndex;
		}

		public void Initialize(int bridgeNavmeshId, ShipAttachmentMachine attachmentSource)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			_face1PathFaceRecord = PathFaceRecord.NullFaceRecord;
			_face2PathFaceRecord = PathFaceRecord.NullFaceRecord;
			BridgeNavmeshId = bridgeNavmeshId;
			CurrentAttachment = attachmentSource.CurrentAttachment;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.ImportNavigationMeshPrefab("ship_connection_plank_navmesh_1", BridgeNavmeshId);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AttachNavigationMeshFaces(BridgeNavmeshId, false, false, false, false, false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AttachNavigationMeshFaces(BridgeNavmeshId + 1, false, false, false, false, false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AttachNavigationMeshFaces(BridgeNavmeshId + 2, false, false, false, false, false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AttachNavigationMeshFaces(BridgeNavmeshId + 3, false, false, false, false, false);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).AttachNavigationMeshFaces(BridgeNavmeshId + 4, false, false, false, false, true);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetUpdateValidtyOnFrameChangedOfFacesWithId(BridgeNavmeshId + 1, true);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetUpdateValidtyOnFrameChangedOfFacesWithId(BridgeNavmeshId + 2, true);
			Mission.Current.Scene.SetAbilityOfFacesWithId(BridgeNavmeshId + 3, false);
			Mission.Current.Scene.SetAbilityOfFacesWithId(BridgeNavmeshId + 4, false);
			_customVertexIndices = new int[6];
			_bridgeCustomVertexPositionsArray = (Vec3[])(object)new Vec3[6];
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_attachedFaceCount = ((WeakGameEntity)(ref gameEntity)).GetAttachedNavmeshFaceCount();
			PathFaceRecord[] array = (PathFaceRecord[])(object)new PathFaceRecord[_attachedFaceCount];
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetAttachedNavmeshFaceRecords(array);
			PathFaceRecord[] array2 = array;
			foreach (PathFaceRecord val in array2)
			{
				if (val.FaceGroupIndex == BridgeNavmeshId + 1)
				{
					_face1PathFaceRecord = val;
				}
				else if (val.FaceGroupIndex == BridgeNavmeshId + 2)
				{
					_face2PathFaceRecord = val;
				}
			}
			int[] array3 = new int[4];
			int[] array4 = new int[4];
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetAttachedNavmeshFaceVertexIndices(ref _face1PathFaceRecord, array3);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).GetAttachedNavmeshFaceVertexIndices(ref _face2PathFaceRecord, array4);
			int num = -1;
			int num2 = -1;
			int num3 = -1;
			int num4 = -1;
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					if (array3[j] == array4[k])
					{
						if (num == -1 && num3 == -1)
						{
							num = j;
							num3 = k;
						}
						else
						{
							num2 = j;
							num4 = k;
						}
						break;
					}
				}
			}
			int num5 = (num + 1) % 4;
			int num6 = (num + 2) % 4;
			int num7 = (num4 + 1) % 4;
			int num8 = (num4 + 2) % 4;
			SetCustomNavmeshVertexIndices(array4[num7], array3[num2], array3[num6], array4[num8], array3[num], array3[num5]);
			CurrentAttachment.AttachmentSource.SteppedAgentManager.SetNavmeshHolder(this);
		}

		public void SetCustomNavmeshVertexIndices(int v1, int v2, int v3, int v4, int v5, int v6)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			_customVertexIndices[0] = v1;
			_customVertexIndices[1] = v2;
			_customVertexIndices[2] = v3;
			_customVertexIndices[3] = v4;
			_customVertexIndices[4] = v5;
			_customVertexIndices[5] = v6;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetCustomVertexPositionEnabled(true);
		}

		public void SetShipBridgeStartEndPositions(Vec3 startLeftPosition, Vec3 startRightPosition, Vec3 endLeftPosition, Vec3 endRightPosition)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			_startLeftPosition = startLeftPosition;
			_startRightPosition = startRightPosition;
			_endLeftPosition = endLeftPosition;
			_endRightPosition = endRightPosition;
			_rightVector = _endRightPosition - _startRightPosition;
			_leftVector = _endLeftPosition - _startLeftPosition;
		}

		protected override void OnDynamicNavmeshVertexUpdate()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			float num = 0.25f;
			for (int i = 1; i < 4; i++)
			{
				Vec3 val = _startRightPosition + _rightVector * num;
				Vec3 val2 = _startLeftPosition + _leftVector * num;
				Vec3 val3 = (val + val2) * 0.5f;
				Vec3 val4 = (val2 - val) * 0.5f;
				_bridgeCustomVertexPositionsArray[i - 1] = val3 - val4 * 0.8f;
				_bridgeCustomVertexPositionsArray[i + 2] = val3 + val4 * 0.8f;
				num += 0.25f;
			}
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetPositionsForAttachedNavmeshVertices(_customVertexIndices, 6, _bridgeCustomVertexPositionsArray);
		}
	}

	public class ShipBridge : MissionObject
	{
	}

	public class ShipAttachmentJoint
	{
		private const string RopeSnapSoundEvent = "event:/mission/movement/vessel/rope_snap";

		private const float LeftoverImpulseDecay = 0.9f;

		private readonly int RopeStressSoundEventId = SoundManager.GetEventGlobalIndex("event:/mission/movement/vessel/rope_stress");

		private readonly GameEntity _shipSource;

		private readonly GameEntity _shipTarget;

		private readonly MissionShip _shipSourceScript;

		private readonly MissionShip _shipTargetScript;

		private readonly ShipAttachmentMachine _attachmentEntitySource;

		private readonly ShipAttachmentPointMachine _attachmentEntityTarget;

		private float _age;

		private float _stiffness;

		private bool _unbreakableJoint;

		private Vec3 _ropeLeftoverImpulse;

		private Vec3 _bridgeDirectionLeftoverImpulse;

		private Vec3 _bridgeAlignmentLeftoverImpulse;

		private Vec3 _bridgeXYLeftoverImpulse;

		private ShipAttachment.ShipAttachmentState _currentAttachmentState;

		private float _currentPullSpeed;

		private float _prevDistanceLambda;

		private float _ropesPullDt;

		private NavalShipsLogic _navalShipsLogic;

		private SoundEvent _ropeStressSoundEvent;

		public float AccumulatedDistanceError { get; private set; }

		public float AccumulatedXYError { get; private set; }

		public float AccumulatedAlignmentError { get; private set; }

		public float CurrentXYError { get; private set; }

		public float CurrentAlignmentError { get; private set; }

		public bool IsBroken { get; private set; }

		public float CurrentDistanceError { get; private set; }

		public ShipAttachmentJoint(ShipAttachmentMachine attachmentSource, ShipAttachmentPointMachine attachmentTarget, bool unbreakableJoint = false)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)attachmentSource).GameEntity;
			_shipSource = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref gameEntity)).Root);
			gameEntity = ((ScriptComponentBehavior)attachmentTarget).GameEntity;
			_shipTarget = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref gameEntity)).Root);
			_attachmentEntitySource = attachmentSource;
			_attachmentEntityTarget = attachmentTarget;
			_shipSourceScript = _shipSource.GetFirstScriptOfType<MissionShip>();
			_shipTargetScript = _shipTarget.GetFirstScriptOfType<MissionShip>();
			_unbreakableJoint = unbreakableJoint;
			InitializeJointParameters();
			UpdateRopeMinLength();
			_currentPullSpeed = 0f;
			_prevDistanceLambda = 0f;
			_ropesPullDt = 0f;
			_ropeStressSoundEvent = SoundEvent.CreateEvent(RopeStressSoundEventId, Mission.Current.Scene);
			_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		}

		public void OnBreak()
		{
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			if (_currentAttachmentState == ShipAttachment.ShipAttachmentState.RopesPulling)
			{
				WeakGameEntity gameEntity;
				if (Agent.Main != null && Agent.Main.IsActive())
				{
					if (_attachmentEntitySource.OwnerShip.GetIsAgentOnShip(Agent.Main))
					{
						gameEntity = ((ScriptComponentBehavior)_attachmentEntitySource).GameEntity;
						MatrixFrame globalFrameImpreciseForFixedTick = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick();
						SoundManager.StartOneShotEvent("event:/mission/movement/vessel/rope_snap", ref globalFrameImpreciseForFixedTick.origin, "isPlayer", 1f);
					}
					else if (_attachmentEntitySource.OwnerShip.GetIsAgentOnShip(Agent.Main))
					{
						gameEntity = ((ScriptComponentBehavior)_attachmentEntityTarget).GameEntity;
						MatrixFrame globalFrameImpreciseForFixedTick = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick();
						SoundManager.StartOneShotEvent("event:/mission/movement/vessel/rope_snap", ref globalFrameImpreciseForFixedTick.origin, "isPlayer", 1f);
					}
				}
				else
				{
					gameEntity = ((ScriptComponentBehavior)_attachmentEntityTarget).GameEntity;
					Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick().origin;
					gameEntity = ((ScriptComponentBehavior)_attachmentEntityTarget).GameEntity;
					Vec3 val = (origin + ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick().origin) * 0.5f;
					SoundManager.StartOneShotEvent("event:/mission/movement/vessel/rope_snap", ref val, "isPlayer", 0f);
				}
				if (_ropeStressSoundEvent != null)
				{
					_ropeStressSoundEvent.Stop();
					_ropeStressSoundEvent = null;
				}
			}
			_navalShipsLogic.OnShipAttachmentLost(_attachmentEntitySource.OwnerShip, _attachmentEntityTarget.OwnerShip);
		}

		public void OnFixedTick(float fixedDt, ShipAttachment currentAttachment, ref float currentRopeLength)
		{
			if (_attachmentEntitySource.IsShipAttachmentJointPhysicsEnabled)
			{
				StabilizeShipUps(15f);
				AlignShips();
				Update(fixedDt, ref currentRopeLength, currentAttachment);
				ReduceRelativeDrift(1f, 15f);
			}
			UpdateRopeLength(fixedDt, ref currentRopeLength, currentAttachment);
		}

		private void StabilizeShipUps(float correctionTorqueCoefficient)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			int num = _shipSourceScript.ComputeActiveShipAttachmentCount();
			int num2 = _shipTargetScript.ComputeActiveShipAttachmentCount();
			Mat3 rotation = _shipSource.GetBodyWorldTransform().rotation;
			Mat3 rotation2 = _shipTarget.GetBodyWorldTransform().rotation;
			float mass = _shipSourceScript.Physics.Mass;
			float mass2 = _shipTargetScript.Physics.Mass;
			Vec3 u = rotation.u;
			Vec3 u2 = rotation2.u;
			Vec3 f = rotation.f;
			Vec3 f2 = rotation2.f;
			Vec3 val = ((Vec3)(ref u)).CrossProductWithUp() * (correctionTorqueCoefficient * mass * _stiffness);
			Vec3 val2 = ((Vec3)(ref u2)).CrossProductWithUp() * (correctionTorqueCoefficient * mass2 * _stiffness);
			val = Vec3.DotProduct(val, f) * f;
			val2 = Vec3.DotProduct(val2, f2) * f2;
			_shipSourceScript.Physics.ApplyTorque(val / (float)num, (ForceMode)0);
			_shipTargetScript.Physics.ApplyTorque(val2 / (float)num2, (ForceMode)0);
		}

		public void UpdateRopeMinLength()
		{
			_attachmentEntitySource.RopeMinLength = CalculatePossibleRopeMinLength(_attachmentEntitySource, _attachmentEntityTarget);
			if (_attachmentEntitySource.BridgeConnectionLengthSquared < _attachmentEntitySource.RopeMinLength * _attachmentEntitySource.RopeMinLength)
			{
				float num = _attachmentEntitySource.RopeMinLength + 1f;
				_attachmentEntitySource.BridgeConnectionLengthSquared = num * num;
			}
		}

		public static float CalculatePossibleBridgeConnectionLengthSquared(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine)
		{
			float num = CalculatePossibleRopeMinLength(attachmentMachine, attachmentPointMachine) + 2.5f;
			return num * num;
		}

		public static float CalculatePossibleRopeMinLength(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			MissionShip ownerShip = attachmentMachine.OwnerShip;
			MissionShip ownerShip2 = attachmentPointMachine.OwnerShip;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)ownerShip).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			gameEntity = ((ScriptComponentBehavior)ownerShip2).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			Vec3 origin = attachmentMachine.ConnectionClipPlaneEntity.GetGlobalFrame().origin;
			Vec3 origin2 = attachmentPointMachine.ConnectionClipPlaneEntity.GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)attachmentMachine).GameEntity;
			MatrixFrame globalFrame3 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			gameEntity = ((ScriptComponentBehavior)attachmentPointMachine).GameEntity;
			MatrixFrame globalFrame4 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			float num = Vec3.DotProduct(globalFrame.rotation.f, globalFrame2.rotation.f);
			Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref origin);
			Vec3 val2 = ((MatrixFrame)(ref globalFrame2)).TransformToLocal(ref origin2);
			float num2 = val.z - ownerShip.Physics.StabilitySubmergedHeightOfShip;
			float num3 = val2.z - ownerShip2.Physics.StabilitySubmergedHeightOfShip;
			float num4 = MathF.Abs(num2 - num3);
			MatrixFrame val3 = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame3);
			MatrixFrame val4 = ((MatrixFrame)(ref globalFrame2)).TransformToLocal(ref globalFrame4);
			Vec2[] localPhysicsBoundingBoxXYPlaneVertices = ownerShip.GetLocalPhysicsBoundingBoxXYPlaneVertices(0.9f);
			Vec2[] localPhysicsBoundingBoxXYPlaneVertices2 = ownerShip2.GetLocalPhysicsBoundingBoxXYPlaneVertices(0.9f);
			float num5 = Vec2.DotProduct(((Vec3)(ref globalFrame3.rotation.f)).AsVec2, ((Vec3)(ref globalFrame.rotation.s)).AsVec2);
			float num6 = Vec2.DotProduct(((Vec3)(ref globalFrame4.rotation.f)).AsVec2, ((Vec3)(ref globalFrame2.rotation.s)).AsVec2);
			Vec2 val5 = ((num5 > 0f) ? localPhysicsBoundingBoxXYPlaneVertices[3] : localPhysicsBoundingBoxXYPlaneVertices[0]);
			Vec2 val6 = ((num5 > 0f) ? localPhysicsBoundingBoxXYPlaneVertices[2] : localPhysicsBoundingBoxXYPlaneVertices[1]);
			Vec2 val7 = ((num5 > 0f) ? Vec2.Side : (-Vec2.Side));
			Vec2 val8 = ((num6 > 0f) ? localPhysicsBoundingBoxXYPlaneVertices2[3] : localPhysicsBoundingBoxXYPlaneVertices2[0]);
			Vec2 val9 = ((num6 > 0f) ? localPhysicsBoundingBoxXYPlaneVertices2[2] : localPhysicsBoundingBoxXYPlaneVertices2[1]);
			Vec2 val10 = ((num6 > 0f) ? Vec2.Side : (-Vec2.Side));
			float num7 = default(float);
			Vec2 val11 = default(Vec2);
			MBMath.CheckLineToLineSegmentIntersection(((Vec3)(ref val3.origin)).AsVec2, val7, val5, val6, ref num7, ref val11);
			float num8 = default(float);
			Vec2 val12 = default(Vec2);
			MBMath.CheckLineToLineSegmentIntersection(((Vec3)(ref val4.origin)).AsVec2, val10, val8, val9, ref num8, ref val12);
			float num9 = num7;
			float num10 = num8;
			float num11 = MathF.Abs(val5.y - val6.y);
			float num12 = (val.y - val5.y) / num11;
			float num13 = MathF.Abs(val8.y - val9.y);
			float num14 = (val2.y - val8.y) / num13;
			if (num < 0f)
			{
				num14 = 1f - num14;
			}
			float num15 = MathF.Abs(num12 - num14);
			float num16 = 1.5f + (num9 + num10) * (1f - num15);
			return MathF.Sqrt(num16 * num16 + num4 * num4);
		}

		public void InitializeJointParameters()
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			_age = 0f;
			_stiffness = 0f;
			AccumulatedDistanceError = 0f;
			AccumulatedXYError = 0f;
			AccumulatedAlignmentError = 0f;
			CurrentDistanceError = 0f;
			CurrentXYError = 0f;
			CurrentAlignmentError = 0f;
			IsBroken = false;
			_ropeLeftoverImpulse = new Vec3(0f, 0f, 0f, -1f);
			_bridgeDirectionLeftoverImpulse = new Vec3(0f, 0f, 0f, -1f);
			_bridgeAlignmentLeftoverImpulse = new Vec3(0f, 0f, 0f, -1f);
			_bridgeXYLeftoverImpulse = new Vec3(0f, 0f, 0f, -1f);
		}

		private void SmoothApproachRopeLength(float dt, ref float currentLength, float target)
		{
			_ropesPullDt += dt;
			float num = MathF.Sin(_ropesPullDt * 2f * MathF.PI * 1f) * 0.5f + 0.5f;
			float num2 = 0.25f * (1f + 0.6f * num);
			_currentPullSpeed = MathF.Min(_currentPullSpeed + num2 * dt, 0.65f);
			float num3 = _currentPullSpeed * dt;
			currentLength = Math.Max(target, currentLength - num3);
		}

		private void UpdateRopeLength(float fixedDt, ref float currentRopeLength, ShipAttachment currentAttachment)
		{
			if (currentAttachment.State == ShipAttachment.ShipAttachmentState.RopesPulling)
			{
				float currentDistanceError = CurrentDistanceError;
				float num = 10f;
				if (currentDistanceError > num * 0.75f)
				{
					float num2 = currentDistanceError / num;
					currentRopeLength = Math.Max(_attachmentEntitySource.RopeMinLength, currentRopeLength + 0.05f * num2 * fixedDt);
					_currentPullSpeed = 0f;
				}
				else
				{
					float ropeMinLength = _attachmentEntitySource.RopeMinLength;
					SmoothApproachRopeLength(fixedDt, ref currentRopeLength, ropeMinLength);
				}
			}
			else if (currentRopeLength < _attachmentEntitySource.RopeMinLength)
			{
				currentRopeLength = MathF.Min(_attachmentEntitySource.RopeMinLength, currentRopeLength + 0.25f * fixedDt);
			}
			else
			{
				currentRopeLength = Math.Max(_attachmentEntitySource.RopeMinLength, currentRopeLength - 0.25f * fixedDt);
			}
		}

		private void Update(float fixedDt, ref float currentRopeLength, ShipAttachment currentAttachment)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			if (IsBroken)
			{
				return;
			}
			_ropeLeftoverImpulse *= 0.9f;
			_bridgeDirectionLeftoverImpulse *= 0.9f;
			_bridgeAlignmentLeftoverImpulse *= 0.9f;
			_bridgeXYLeftoverImpulse *= 0.9f;
			if (currentAttachment.State != _currentAttachmentState)
			{
				if (_ropeStressSoundEvent != null && _currentAttachmentState != ShipAttachment.ShipAttachmentState.RopesPulling)
				{
					_ropeStressSoundEvent.Stop();
					_ropeStressSoundEvent = null;
				}
				InitializeJointParameters();
				_currentAttachmentState = currentAttachment.State;
				if (_currentAttachmentState == ShipAttachment.ShipAttachmentState.BridgeConnected)
				{
					_navalShipsLogic.OnBridgeConnected(_shipSourceScript, _shipTargetScript);
				}
			}
			_age += fixedDt;
			_stiffness = MathF.Min(_age / 5f, 1f);
			CurrentDistanceError = 0f;
			CurrentXYError = 0f;
			CurrentAlignmentError = 0f;
			MatrixFrame globalMassFrame = _shipSourceScript.Physics.GetGlobalMassFrame();
			MatrixFrame globalMassFrame2 = _shipTargetScript.Physics.GetGlobalMassFrame();
			Vec3 origin = _attachmentEntitySource.ConnectionClipPlaneEntity.GetGlobalFrameImpreciseForFixedTick().origin;
			Vec3 origin2 = _attachmentEntityTarget.ConnectionClipPlaneEntity.GetGlobalFrameImpreciseForFixedTick().origin;
			Vec3 linearVelocityAtGlobalPointForEntityWithDynamicBody = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(_shipSource, origin);
			Vec3 relativeVelocityVector = GameEntityPhysicsExtensions.GetLinearVelocityAtGlobalPointForEntityWithDynamicBody(_shipTarget, origin2) - linearVelocityAtGlobalPointForEntityWithDynamicBody;
			float mass = _shipSourceScript.Physics.Mass;
			float mass2 = _shipTargetScript.Physics.Mass;
			if (_currentAttachmentState == ShipAttachment.ShipAttachmentState.RopesPulling)
			{
				UpdateRopeConstraint(fixedDt, currentRopeLength, globalMassFrame, globalMassFrame2, origin, origin2, mass, mass2, relativeVelocityVector);
			}
			else if (_currentAttachmentState == ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				UpdateBridgeConstraints(fixedDt, currentRopeLength, globalMassFrame, globalMassFrame2, origin, origin2, mass, mass2, relativeVelocityVector);
			}
			else if (_currentAttachmentState == ShipAttachment.ShipAttachmentState.BridgeThrown)
			{
				UpdateBridgeConstraints(fixedDt, currentRopeLength, globalMassFrame, globalMassFrame2, origin, origin2, mass, mass2, relativeVelocityVector);
			}
			if (!_unbreakableJoint)
			{
				CheckBreaking(fixedDt, currentAttachment);
			}
		}

		private void AlignShips()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			Mat3 rotation = _shipSource.GetBodyWorldTransform().rotation;
			Mat3 rotation2 = _shipTarget.GetBodyWorldTransform().rotation;
			float num = MathF.Atan2(rotation.f.y, rotation.f.x);
			float num2 = MathF.Atan2(rotation2.f.y, rotation2.f.x) - num;
			num2 = MBMath.WrapAngle(num2);
			if (MathF.Abs(num2) > MathF.PI / 2f)
			{
				num2 = ((!(num2 > 0f)) ? (num2 + MathF.PI) : (num2 - MathF.PI));
			}
			if (MathF.Abs(num2) >= 0.017f)
			{
				int num3 = _shipSourceScript.ComputeActiveShipAttachmentCount();
				int num4 = _shipTargetScript.ComputeActiveShipAttachmentCount();
				float num5 = num2 * 0.5f;
				float num6 = (0f - num2) * 0.5f;
				float num7 = (_shipSourceScript.Physics.Mass + _shipTargetScript.Physics.Mass) * 0.5f;
				float num8 = num5 * num7 * 25f * _stiffness;
				float num9 = num6 * num7 * 25f * _stiffness;
				num8 -= _shipSourceScript.Physics.AngularVelocity.z * num7 * 50f;
				num9 -= _shipTargetScript.Physics.AngularVelocity.z * num7 * 50f;
				float num10 = ((_currentAttachmentState != ShipAttachment.ShipAttachmentState.RopesPulling) ? 1f : 0.25f);
				_shipSourceScript.Physics.ApplyTorque(new Vec3(0f, 0f, num8 / (float)num3 * num10, -1f), (ForceMode)0);
				_shipTargetScript.Physics.ApplyTorque(new Vec3(0f, 0f, num9 / (float)num4 * num10, -1f), (ForceMode)0);
			}
		}

		private void UpdateRopeConstraint(float fixedDt, float currentRopeLength, MatrixFrame shipSourceGlobalFrame, MatrixFrame shipTargetGlobalFrame, Vec3 sourceAttachmentPosition, Vec3 targetAttachmentPosition, float sourceShipMass, float targetShipMass, Vec3 relativeVelocityVector)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			Vec3 val = targetAttachmentPosition - sourceAttachmentPosition;
			if (!(((Vec3)(ref val)).LengthSquared > currentRopeLength * currentRopeLength))
			{
				return;
			}
			float num = ((Vec3)(ref val)).Normalize();
			float relativeVelocity = Vec3.DotProduct(relativeVelocityVector, val);
			float num2 = (CurrentDistanceError = num - currentRopeLength);
			float num4 = 2f;
			float num5 = MathF.Clamp(num2 / num4, 0f, 1f);
			float num6 = MBMath.SmoothStep(0f, num4, num5);
			num6 = (float)MathF.Sign(num2) * num6;
			if (_ropeStressSoundEvent != null)
			{
				if (num5 > 2f)
				{
					if (!_ropeStressSoundEvent.IsPlaying())
					{
						_ropeStressSoundEvent.Play();
					}
					else if (_ropeStressSoundEvent.IsPaused())
					{
						_ropeStressSoundEvent.Resume();
					}
					SoundEvent ropeStressSoundEvent = _ropeStressSoundEvent;
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)_attachmentEntitySource).GameEntity;
					Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick().origin;
					gameEntity = ((ScriptComponentBehavior)_attachmentEntityTarget).GameEntity;
					ropeStressSoundEvent.SetPosition((origin + ((WeakGameEntity)(ref gameEntity)).GetGlobalFrameImpreciseForFixedTick().origin) * 0.5f);
				}
				else if (_ropeStressSoundEvent.IsPlaying())
				{
					_ropeStressSoundEvent.Pause();
				}
			}
			float reducedMass = sourceShipMass * targetShipMass / (sourceShipMass + targetShipMass);
			float beta = 0.1f * _stiffness;
			float damping = 0.1f;
			float num7 = MathF.Min(CurrentDistanceError / 10f, 1f);
			float num8 = sourceShipMass + targetShipMass;
			float num9 = MathF.Min(sourceShipMass, targetShipMass) * 2f / num8;
			float maxAcceleration = MathF.Lerp(1.2f, 5f, num7 * (1f - num9), 1E-05f);
			float num10 = SolveImpulseConstraint(relativeVelocity, num6, reducedMass, beta, damping, fixedDt);
			num10 = MathF.Abs(num10) * (float)MathF.Sign(num6);
			float num11 = (_prevDistanceLambda = MathF.Lerp(_prevDistanceLambda, num10, fixedDt * 2f, 1E-05f));
			ApplyConstraintImpulse(val * num11, shipSourceGlobalFrame, shipTargetGlobalFrame, sourceAttachmentPosition, targetAttachmentPosition, maxAcceleration, sourceShipMass, targetShipMass, fixedDt, ref _ropeLeftoverImpulse);
		}

		public float SolveSpringMassSystemFromTargetPeriod(float dt, float reducedMass, float targetPeriod, float dampingRatio, float distance, float relativeSpeed)
		{
			float num = MathF.PI * 2f / targetPeriod;
			float num2 = reducedMass * num * num;
			float num3 = 2f * reducedMass * dampingRatio * num;
			return ((0f - num2) * distance - num3 * relativeSpeed) * dt;
		}

		private void UpdateBridgeConstraints(float dt, float currentRopeLength, MatrixFrame shipSourceGlobalFrame, MatrixFrame shipTargetGlobalFrame, Vec3 sourceAttachmentPosition, Vec3 targetAttachmentPosition, float sourceShipMass, float targetShipMass, Vec3 relativeVelocityVector)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			float reducedMass = sourceShipMass * targetShipMass / (sourceShipMass + targetShipMass);
			Vec3 val = targetAttachmentPosition - sourceAttachmentPosition;
			float num = (CurrentDistanceError = ((Vec3)(ref val)).Normalize() - currentRopeLength);
			float relativeSpeed = Vec3.DotProduct(relativeVelocityVector, val);
			float num3 = SolveSpringMassSystemFromTargetPeriod(dt, reducedMass, 2f, 0.3f, num, relativeSpeed);
			float maxAcceleration = MathF.Lerp(0.01f, 5f, MathF.Min(1f, MathF.Abs(num)), 1E-05f);
			ApplyConstraintImpulse((0f - num3) * val * _stiffness, shipSourceGlobalFrame, shipTargetGlobalFrame, sourceAttachmentPosition, targetAttachmentPosition, maxAcceleration, sourceShipMass, targetShipMass, dt, ref _bridgeDirectionLeftoverImpulse);
			float num4 = Vec3.DotProduct(shipSourceGlobalFrame.rotation.f, shipTargetGlobalFrame.rotation.f);
			Vec3 val2 = shipTargetGlobalFrame.rotation.f;
			if (num4 < 1E-05f)
			{
				val2 = -1f * shipTargetGlobalFrame.rotation.f;
			}
			Vec2 val3 = ((Vec3)(ref shipSourceGlobalFrame.rotation.f)).AsVec2;
			Vec2 val4 = ((Vec2)(ref val3)).Normalized();
			val3 = ((Vec3)(ref val2)).AsVec2;
			val3 = val4 + ((Vec2)(ref val3)).Normalized();
			val3 = ((Vec2)(ref val3)).Normalized();
			Vec3 val5 = ((Vec2)(ref val3)).ToVec3(0f);
			float num5 = (CurrentAlignmentError = Vec3.DotProduct(val, val5));
			float relativeSpeed2 = Vec3.DotProduct(relativeVelocityVector, val5);
			float num7 = SolveSpringMassSystemFromTargetPeriod(dt, reducedMass, 1.75f, 0.8f, num5, relativeSpeed2);
			float maxAcceleration2 = MathF.Lerp(0.01f, 5f, MathF.Min(1f, MathF.Abs(num5)), 1E-05f);
			ApplyConstraintImpulse((0f - num7) * val5 * _stiffness, shipSourceGlobalFrame, shipTargetGlobalFrame, sourceAttachmentPosition, targetAttachmentPosition, maxAcceleration2, sourceShipMass, targetShipMass, dt, ref _bridgeAlignmentLeftoverImpulse);
			Vec3 val6 = targetAttachmentPosition - sourceAttachmentPosition;
			Vec2 val7 = default(Vec2);
			((Vec2)(ref val7))._002Ector(val6.x, val6.y);
			float num8 = ((Vec2)(ref val7)).Normalize();
			float num9 = (CurrentXYError = currentRopeLength * MathF.Sin(MathF.PI * 13f / 36f) - num8);
			if (num9 > 0f)
			{
				float num11 = Vec2.DotProduct(((Vec3)(ref relativeVelocityVector)).AsVec2, val7);
				float num12 = SolveSpringMassSystemFromTargetPeriod(dt, reducedMass, 0.75f, 0.5f, num9, 0f - num11);
				float maxAcceleration3 = MathF.Lerp(0.01f, 15f, MathF.Min(1f, MathF.Abs(num9)), 1E-05f);
				ApplyConstraintImpulse(num12 * ((Vec2)(ref val7)).ToVec3(0f) * _stiffness, shipSourceGlobalFrame, shipTargetGlobalFrame, sourceAttachmentPosition, targetAttachmentPosition, maxAcceleration3, sourceShipMass, targetShipMass, dt, ref _bridgeXYLeftoverImpulse);
			}
		}

		private float SolveImpulseConstraint(float relativeVelocity, float positionError, float reducedMass, float beta, float damping, float fixedDt)
		{
			return ((0f - beta / fixedDt) * positionError - damping * relativeVelocity) * reducedMass;
		}

		private void ApplyConstraintImpulse(Vec3 impulse, MatrixFrame shipSourceGlobalFrame, MatrixFrame shipTargetGlobalFrame, Vec3 attachmentSourceGlobalPosition, Vec3 attachmentTargetGlobalPosition, float maxAcceleration, float sourceShipMass, float targetShipMass, float fixedDt, ref Vec3 leftoverImpulse)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			float num = ((Vec3)(ref impulse)).Normalize();
			Vec3 val = impulse;
			float num2 = MathF.Abs(num);
			float num3 = sourceShipMass * maxAcceleration * fixedDt;
			float num4 = targetShipMass * maxAcceleration * fixedDt;
			float num5 = MathF.Min(num3, num4);
			float num6 = MathF.Min(num2, num5);
			float num7 = num6 * (float)MathF.Sign(num);
			float num8 = num2 - num6;
			leftoverImpulse += num8 * 0.5f * val;
			Vec3 globalForceVec = val * num7;
			Vec3 globalForceVec2 = -globalForceVec;
			_shipSourceScript.Physics.ApplyGlobalForceAtLocalPos(((MatrixFrame)(ref shipSourceGlobalFrame)).TransformToLocal(ref attachmentSourceGlobalPosition), in globalForceVec, (ForceMode)1);
			_shipTargetScript.Physics.ApplyGlobalForceAtLocalPos(((MatrixFrame)(ref shipTargetGlobalFrame)).TransformToLocal(ref attachmentTargetGlobalPosition), in globalForceVec2, (ForceMode)1);
		}

		private void CheckBreaking(float dt, ShipAttachment currentAttachment)
		{
			float num = ((_currentAttachmentState == ShipAttachment.ShipAttachmentState.BridgeThrown || _currentAttachmentState == ShipAttachment.ShipAttachmentState.BridgeConnected) ? 5f : 10f);
			if (CurrentDistanceError > num * 0.5f)
			{
				AccumulatedDistanceError += CurrentDistanceError * 4f * dt;
				if (CurrentDistanceError > num || AccumulatedDistanceError > num)
				{
					IsBroken = true;
				}
			}
			if (CurrentAlignmentError > 0.95f)
			{
				AccumulatedAlignmentError += CurrentAlignmentError * 4f * dt;
				if (AccumulatedAlignmentError > 20f)
				{
					IsBroken = true;
				}
			}
			if (CurrentXYError > 2.0625f)
			{
				AccumulatedXYError += CurrentXYError * 4f * dt;
				if (CurrentXYError > 2.75f || AccumulatedXYError > 2.75f)
				{
					IsBroken = true;
				}
			}
			if (IsBroken)
			{
				OnBreak();
			}
		}

		private void ReduceRelativeDrift(float linearDamping, float angularDamping)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			//IL_024c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			int num = _shipSourceScript.ComputeActiveShipAttachmentCount();
			int num2 = _shipTargetScript.ComputeActiveShipAttachmentCount();
			int num3 = num + num2;
			Vec3 linearVelocity = _shipSourceScript.Physics.LinearVelocity;
			Vec3 linearVelocity2 = _shipTargetScript.Physics.LinearVelocity;
			Vec3 angularVelocity = _shipSourceScript.Physics.AngularVelocity;
			Vec3 angularVelocity2 = _shipTargetScript.Physics.AngularVelocity;
			float mass = _shipSourceScript.Physics.Mass;
			float mass2 = _shipTargetScript.Physics.Mass;
			Vec2 val = (((Vec3)(ref linearVelocity)).AsVec2 * mass + ((Vec3)(ref linearVelocity2)).AsVec2 * mass2) / (mass + mass2);
			Vec2 val2 = val * mass;
			Vec2 val3 = val * mass2;
			float num4 = 2f * mass * 9.806f;
			float num5 = 2f * mass2 * 9.806f;
			((Vec2)(ref val2)).ClampMagnitude(0f, num4);
			((Vec2)(ref val3)).ClampMagnitude(0f, num5);
			NavalDLC.Missions.NavalPhysics.NavalPhysics physics = _shipSourceScript.Physics;
			Vec2 val4 = -val2 * linearDamping * _stiffness / (float)num3;
			physics.ApplyForceToDynamicBody(((Vec2)(ref val4)).ToVec3(0f), (ForceMode)0);
			NavalDLC.Missions.NavalPhysics.NavalPhysics physics2 = _shipTargetScript.Physics;
			val4 = -val3 * linearDamping * _stiffness / (float)num3;
			physics2.ApplyForceToDynamicBody(((Vec2)(ref val4)).ToVec3(0f), (ForceMode)0);
			float num6 = (angularVelocity.z * mass + angularVelocity2.z * mass2) / (mass + mass2);
			if (num6 != 0f)
			{
				float num7 = num6 * mass;
				float num8 = num6 * mass2;
				float num9 = MathF.PI / 9f * mass;
				float num10 = MathF.PI / 9f * mass2;
				num7 = MathF.Clamp(num7, 0f - num9, num9);
				num8 = MathF.Clamp(num8, 0f - num10, num10);
				_shipSourceScript.Physics.ApplyTorque(new Vec3(0f, 0f, 0f - num7, -1f) * angularDamping * _stiffness / (float)num3, (ForceMode)0);
				_shipTargetScript.Physics.ApplyTorque(new Vec3(0f, 0f, 0f - num8, -1f) * angularDamping * _stiffness / (float)num3, (ForceMode)0);
			}
		}
	}

	public class ShipAttachment
	{
		public struct FlightData
		{
			public Vec3 SourceGlobalPosition;

			public Vec3 TargetGlobalPosition;

			public Vec3 GlobalPositionError;

			public Vec3 GlobalVelocity;

			public float AngleDegree;

			public float Time;

			public bool IsUnderWater;

			public FlightData(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition, in Vec3 globalVelocity, float angleDegree, float time)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				SourceGlobalPosition = sourceGlobalPosition;
				TargetGlobalPosition = targetGlobalPosition;
				GlobalVelocity = globalVelocity;
				AngleDegree = angleDegree;
				Time = time;
				GlobalPositionError = Vec3.Zero;
				IsUnderWater = false;
			}
		}

		internal struct BridgeFlightData
		{
			internal float DtSinceFlightStart;

			internal float CurveLerpVelocity;

			internal float CurveLerpValue;

			internal float ThrowFinishValue;

			internal float CurrentFrameTotalLightTime;

			internal Vec3 CurrentFrameInitialVelocity;
		}

		internal struct RopeSegment
		{
			internal GameEntity ParentEntity;

			internal GameEntity RopeStart;

			internal GameEntity RopeEnd;

			internal int StartSegmentIndex;

			internal int EndSegmentIndex;

			internal float SideStartShift;

			internal float SideEndShift;
		}

		public enum ShipAttachmentState
		{
			RopeThrown,
			RopesPulling,
			BridgeThrown,
			BridgeConnected,
			BrokenAndWaitingForRemoval,
			RopeFailedAndReloading
		}

		private const string NavMeshHolderTag = "navmesh_holder";

		private const string HookImpactWater = "event:/mission/movement/vessel/hook_impact_fail_water_splash";

		private const string HookImpactAttachSuccess = "event:/mission/movement/vessel/hook_impact_attach";

		private const string HookImpactAttachFail = "event:/mission/movement/vessel/hook_impact_fail_to_attach";

		private const string HookThrowingSoundEvent = "event:/mission/movement/vessel/hook_throw";

		private const string BridgeThrownSoundEvent = "event:/mission/movement/vessel/bridge_connect";

		private const string BridgeBrokenSoundEvent = "event:/mission/movement/vessel/bridge_fall";

		private const string HookBeforeAttachmentSoundEvent = "event:/mission/movement/vessel/hook_attach_point_snap";

		private const float ForwardRotationLimitAngleCos = 0.17364818f;

		private const float RopesPullingInteractionDistanceSquared = 2500f;

		private const float BridgeConnectedInteractionDistanceSquared = 100f;

		private const float BridgeConnectedAngleCosLimit = 0.18f;

		private const int BridgeCurveLinearSampleCount = 16;

		private const int MaximumPlankCount = 80;

		private static readonly Comparer<KeyValuePair<float, Vec3>> _cacheCompareDelegate = Comparer<KeyValuePair<float, Vec3>>.Create((KeyValuePair<float, Vec3> x, KeyValuePair<float, Vec3> y) => x.Key.CompareTo(y.Key));

		private bool _attachmentInitializedByPlayer;

		private static List<string> _shipConnectionPlankVariations = new List<string> { "ship_connection_plank_no_physics_a", "ship_connection_plank_no_physics_b", "ship_connection_plank_no_physics_c", "ship_connection_plank_no_physics_d" };

		private static List<string> _ropeClothFragmentPrefabList = new List<string> { "cloth_fragment_a", "cloth_fragment_b", "cloth_fragment_c", "cloth_fragment_g", "cloth_fragment_i", "cloth_fragment_d" };

		private float _shipBetweenAttachmentsCheckTimer;

		private MissionTimer _ropesPullingTimer;

		private GameEntity _bridge;

		private GameEntity _navMeshBridge;

		private GameEntity _navMeshBridgeNavMeshHolder;

		private ShipBridgeNavmeshHolder _shipBridgeNavmeshHolder;

		private int _bridgeNavmeshId;

		private List<GameEntity> _planks = new List<GameEntity>();

		private List<GameEntity> _targetSafetyPlanks = new List<GameEntity>();

		private List<GameEntity> _sourceSafetyPlanks = new List<GameEntity>();

		private KeyValuePair<float, Vec3>[] _bridgeCurveLinearAccessCache = new KeyValuePair<float, Vec3>[16];

		private int _previousNumberOfPlanksNeeded = 80;

		private int _numberOfPlanksNeeded = 80;

		private List<RopeSegment> _ropes = new List<RopeSegment>();

		private BridgeFlightData _bridgeFlightData;

		private bool _isNavmeshBridgeDisabled;

		private float _plankVerticalSize;

		private float _plankHorizontalSize;

		private ShipAttachmentState _state;

		private PhysicsMaterial _woodPhysicsMaterialCached;

		private PhysicsMaterial _defaultPhysicsMaterialCached;

		private Vec3[] _sideBarrierQuadsCached = (Vec3[])(object)new Vec3[4];

		private UIntPtr _sideBarriersQuadPinnedPointer = UIntPtr.Zero;

		private GCHandle _sideBarriersQuadPinnedGCHandler;

		private UIntPtr _sideBarriersIndicesPinnedPointer = UIntPtr.Zero;

		private GCHandle _sideBarriersIndicesPinnedGCHandler;

		private int[] _sideBarrierIndicesCached = new int[6];

		private Vec3[] _vFoldQuadsCached = (Vec3[])(object)new Vec3[4];

		private UIntPtr _vFoldQuadPinnedPointer = UIntPtr.Zero;

		private GCHandle _vFoldQuadPinnedGCHandler;

		private UIntPtr _vFoldIndicesPinnedPointer = UIntPtr.Zero;

		private GCHandle _vFoldIndicesPinnedGCHandler;

		private int[] _vFoldQuadsIndicesCached = new int[6];

		private int[] _alreadyAddedVertexDataForPhysicsClipPlaneIntersection = new int[4];

		private Vec3[] _registeredVerticesAfterPhysicsClipPlaneIntersection = (Vec3[])(object)new Vec3[5];

		private Vec3[] _quadVerticesCCWCached = (Vec3[])(object)new Vec3[4];

		private Vec3[] _currentFramePlankPhysicsVertices = (Vec3[])(object)new Vec3[200];

		private UIntPtr _currentFramePlankPhysicsVerticesPinnedPointer = UIntPtr.Zero;

		private GCHandle _currentFramePlankPhysicsVerticesPinnedGCHandler;

		private int _currentFramePlankPhysicsVertexCount;

		private int[] _currentFramePlankPhysicsIndices = new int[300];

		private int _currentFramePlankPhysicsIndexCount;

		private UIntPtr _currentFramePlankPhysicsIndicesPinnedPointer = UIntPtr.Zero;

		private GCHandle _currentFramePlankPhysicsIndicesPinnedGCHandler;

		private bool _faceSwapSideOneDone = true;

		private bool _faceSwapSideTwoDone = true;

		private bool _bridgeCreated;

		private bool _hookAttachSoundAlreadyTriggered;

		private Timer _bridgeSwapTimer;

		private float _ropeThrownTimer;

		private MatrixFrame _hookGlobalFrame;

		private FlightData _launchFlightData;

		private bool _currentRopeLengthFirstReachedFinalValue = true;

		private float _currentRopeLength;

		public ShipAttachmentMachine AttachmentSource { get; private set; }

		public ShipAttachmentPointMachine AttachmentTarget { get; private set; }

		public Vec3 CommittedWeightedPosition { get; private set; }

		public float CommittedTotalMass { get; private set; }

		public float CommittedAgentCount { get; private set; }

		public bool BridgeConnectionInteractionDistanceCheck { get; private set; }

		public ShipAttachmentState State => _state;

		public MatrixFrame HookGlobalFrame => _hookGlobalFrame;

		public bool IsNavmeshConnected
		{
			get
			{
				if (_state == ShipAttachmentState.BridgeConnected && _faceSwapSideOneDone)
				{
					return _faceSwapSideTwoDone;
				}
				return false;
			}
		}

		public bool ShipIslandsConnected { get; private set; } = true;

		public ShipAttachmentJoint ShipAttachmentJoint { get; private set; }

		public void ClearCommittedAgentInformation()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			CommittedTotalMass = 0f;
			CommittedWeightedPosition = Vec3.Zero;
			CommittedAgentCount = 0f;
		}

		public void SetAttachmentState(ShipAttachmentState state)
		{
			if (_state != state)
			{
				ShipAttachmentState state2 = _state;
				_state = state;
				UpdateAttachmentMachineEntityVisibilities(state2);
				if (state == ShipAttachmentState.BrokenAndWaitingForRemoval)
				{
					AttachmentSource.OwnerShip.ShipsLogic.OnAttachmentBroken(AttachmentSource, AttachmentTarget);
				}
			}
		}

		public ShipAttachment(ShipAttachmentMachine attachmentSource, ShipAttachmentPointMachine attachmentTarget, in Vec3 globalPosition, in Vec3 globalDirection, bool bridgeConnectionInteractionDistanceCheck = true, bool attachmentInitializedByPlayer = false)
		{
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			_state = ShipAttachmentState.RopeThrown;
			AttachmentSource = attachmentSource;
			AttachmentTarget = attachmentTarget;
			_ropesPullingTimer = new MissionTimer(30f);
			_shipBetweenAttachmentsCheckTimer = 0.1f;
			_attachmentInitializedByPlayer = attachmentInitializedByPlayer;
			BridgeConnectionInteractionDistanceCheck = bridgeConnectionInteractionDistanceCheck;
			WeakGameEntity gameEntity;
			if (AttachmentTarget != null)
			{
				gameEntity = ((ScriptComponentBehavior)AttachmentTarget).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				Vec3 hookAttachLocalPosition = AttachmentTarget.HookAttachLocalPosition;
				InitializeRopeFlightDataAccordingToTargetPoint(in globalPosition, ((MatrixFrame)(ref globalFrame)).TransformToParent(ref hookAttachLocalPosition));
			}
			else
			{
				InitializeRopeFlightDataAccordingToTargetDirection(in globalPosition, in globalDirection);
			}
			gameEntity = ((ScriptComponentBehavior)AttachmentSource.RopeVisual).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
			SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_throw", ref globalPosition);
			SpawnPlankEntities();
			_woodPhysicsMaterialCached = PhysicsMaterial.GetFromName("wood_nonstick");
			_defaultPhysicsMaterialCached = PhysicsMaterial.GetFromName("default");
			_currentFramePlankPhysicsVerticesPinnedGCHandler = GCHandle.Alloc(_currentFramePlankPhysicsVertices, GCHandleType.Pinned);
			_currentFramePlankPhysicsVerticesPinnedPointer = (UIntPtr)(ulong)(long)_currentFramePlankPhysicsVerticesPinnedGCHandler.AddrOfPinnedObject();
			_currentFramePlankPhysicsIndicesPinnedGCHandler = GCHandle.Alloc(_currentFramePlankPhysicsIndices, GCHandleType.Pinned);
			_currentFramePlankPhysicsIndicesPinnedPointer = (UIntPtr)(ulong)(long)_currentFramePlankPhysicsIndicesPinnedGCHandler.AddrOfPinnedObject();
			_sideBarriersQuadPinnedGCHandler = GCHandle.Alloc(_sideBarrierQuadsCached, GCHandleType.Pinned);
			_sideBarriersQuadPinnedPointer = (UIntPtr)(ulong)(long)_sideBarriersQuadPinnedGCHandler.AddrOfPinnedObject();
			_sideBarriersIndicesPinnedGCHandler = GCHandle.Alloc(_sideBarrierIndicesCached, GCHandleType.Pinned);
			_sideBarriersIndicesPinnedPointer = (UIntPtr)(ulong)(long)_sideBarriersIndicesPinnedGCHandler.AddrOfPinnedObject();
			_vFoldQuadPinnedGCHandler = GCHandle.Alloc(_vFoldQuadsCached, GCHandleType.Pinned);
			_vFoldQuadPinnedPointer = (UIntPtr)(ulong)(long)_vFoldQuadPinnedGCHandler.AddrOfPinnedObject();
			_vFoldIndicesPinnedGCHandler = GCHandle.Alloc(_vFoldQuadsIndicesCached, GCHandleType.Pinned);
			_vFoldIndicesPinnedPointer = (UIntPtr)(ulong)(long)_vFoldIndicesPinnedGCHandler.AddrOfPinnedObject();
			ClearCommittedAgentInformation();
		}

		private void UpdateAttachmentMachineEntityVisibilities(ShipAttachmentState oldState)
		{
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			bool flag;
			bool visibilityExcludeParents;
			bool visibilityExcludeParents2;
			bool connectionPhysicsEntitiesVisibility;
			switch (_state)
			{
			case ShipAttachmentState.RopeThrown:
			case ShipAttachmentState.RopesPulling:
				flag = false;
				visibilityExcludeParents = true;
				visibilityExcludeParents2 = true;
				connectionPhysicsEntitiesVisibility = false;
				break;
			case ShipAttachmentState.BridgeThrown:
				flag = true;
				visibilityExcludeParents = false;
				visibilityExcludeParents2 = false;
				connectionPhysicsEntitiesVisibility = true;
				break;
			case ShipAttachmentState.BridgeConnected:
				flag = true;
				visibilityExcludeParents = false;
				visibilityExcludeParents2 = false;
				connectionPhysicsEntitiesVisibility = true;
				SetOarsAvailability(value: false);
				SetShieldsVisibility(visible: false);
				break;
			case ShipAttachmentState.BrokenAndWaitingForRemoval:
				flag = false;
				visibilityExcludeParents = false;
				visibilityExcludeParents2 = true;
				connectionPhysicsEntitiesVisibility = false;
				break;
			case ShipAttachmentState.RopeFailedAndReloading:
				flag = false;
				visibilityExcludeParents = true;
				visibilityExcludeParents2 = true;
				connectionPhysicsEntitiesVisibility = false;
				break;
			default:
				flag = false;
				visibilityExcludeParents = false;
				visibilityExcludeParents2 = false;
				connectionPhysicsEntitiesVisibility = false;
				break;
			}
			if (oldState == ShipAttachmentState.BridgeConnected)
			{
				SetShieldsVisibility(visible: true);
				SetOarsAvailability(value: true);
			}
			foreach (GameEntity item in (List<GameEntity>)(object)AttachmentSource.RampPhysicsList)
			{
				item.SetVisibilityExcludeParents(flag);
			}
			AttachmentSource.RampVisualEntity.SetVisibilityExcludeParents(flag);
			AttachmentSource.RampBarrier.SetVisibilityExcludeParents(!flag);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)AttachmentSource.RopeVisual).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(visibilityExcludeParents);
			AttachmentSource.Hook.SetVisibilityExcludeParents(visibilityExcludeParents2);
			AttachmentSource.SetConnectionPhysicsEntitiesVisibility(connectionPhysicsEntitiesVisibility);
			if (AttachmentTarget == null)
			{
				return;
			}
			AttachmentTarget.RampVisualEntity.SetVisibilityExcludeParents(flag);
			foreach (GameEntity item2 in (List<GameEntity>)(object)AttachmentTarget.RampPhysicsList)
			{
				item2.SetVisibilityExcludeParents(flag);
			}
			AttachmentTarget.RampBarrier.SetVisibilityExcludeParents(!flag);
		}

		public bool ShouldLookForBetterConnections()
		{
			return AttachmentTarget != null;
		}

		public void OnParallelTick(float dt)
		{
			if (_state == ShipAttachmentState.BridgeConnected)
			{
				ArrangePlanksMT();
			}
		}

		public void OnTick(float dt)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0340: Unknown result type (might be due to invalid IL or missing references)
			ClearCommittedAgentInformation();
			WeakGameEntity gameEntity;
			MatrixFrame globalFrame;
			if (_state == ShipAttachmentState.BrokenAndWaitingForRemoval)
			{
				RopePileBaked ropeVisual = AttachmentSource.RopeVisual;
				gameEntity = ((ScriptComponentBehavior)AttachmentSource.RopeVisual).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				ref Vec3 origin = ref globalFrame.origin;
				gameEntity = ((ScriptComponentBehavior)AttachmentSource.RopeVisual).GameEntity;
				MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				ropeVisual.UpdateRopeMeshVisualAccordingToTargetPointLinear(in origin, in globalFrame2.origin);
				return;
			}
			if (_state == ShipAttachmentState.RopeThrown || _state == ShipAttachmentState.RopeFailedAndReloading)
			{
				UpdateRopeThrowingBehavior(dt);
			}
			else
			{
				gameEntity = ((ScriptComponentBehavior)AttachmentTarget).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				Vec3 val = AttachmentTarget.HookAttachLocalPosition;
				Vec3 targetGlobalPosition = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val);
				gameEntity = ((ScriptComponentBehavior)AttachmentSource.RopeVisual).GameEntity;
				Vec3 sourceGlobalPosition = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
				_hookGlobalFrame.origin = AttachmentSource.RopeVisual.UpdateRopeMeshVisualAccordingToTargetPointLinear(in sourceGlobalPosition, in targetGlobalPosition);
				ref Mat3 rotation = ref _hookGlobalFrame.rotation;
				val = targetGlobalPosition - sourceGlobalPosition;
				rotation.f = ((Vec3)(ref val)).NormalizedCopy();
				ref Mat3 rotation2 = ref _hookGlobalFrame.rotation;
				val = ((Vec3)(ref _hookGlobalFrame.rotation.f)).CrossProductWithUp();
				rotation2.s = ((Vec3)(ref val)).NormalizedCopy();
				_hookGlobalFrame.rotation.u = Vec3.CrossProduct(_hookGlobalFrame.rotation.s, _hookGlobalFrame.rotation.f);
				((Mat3)(ref _hookGlobalFrame.rotation)).RotateAboutSide(-MathF.PI / 2f);
				if (_currentRopeLengthFirstReachedFinalValue && MBMath.ApproximatelyEquals(_currentRopeLength, AttachmentSource.RopeMinLength, 0.05f))
				{
					_ropesPullingTimer.Reset();
					_currentRopeLengthFirstReachedFinalValue = false;
				}
				if (_state == ShipAttachmentState.RopesPulling)
				{
					CheckAndConnectBridge();
				}
				else if (_state == ShipAttachmentState.BridgeThrown)
				{
					TickThrownBridge(dt);
					ArrangeNavmeshBridgeSideBarriersAndVFoldQuads();
				}
				else if (_state == ShipAttachmentState.BridgeConnected)
				{
					ArrangePlanks();
					ArrangeNavmeshBridgeSideBarriersAndVFoldQuads();
				}
			}
			if (AttachmentTarget != null)
			{
				CheckAndBreakAttachment(dt);
			}
			if (_state == ShipAttachmentState.BridgeConnected || _state == ShipAttachmentState.BridgeThrown)
			{
				if ((!_faceSwapSideOneDone || !_faceSwapSideTwoDone) && _bridgeSwapTimer.Check(Mission.Current.CurrentTime))
				{
					if (!_faceSwapSideOneDone && Mission.Current.Scene.SwapFaceConnectionsWithID(_bridgeNavmeshId + 1, _bridgeNavmeshId + 3, AttachmentTarget.RelatedShipNavmeshOffset + AttachmentTarget.OwnerShip.GetDynamicNavmeshIdStart(), true))
					{
						_faceSwapSideOneDone = true;
					}
					if (!_faceSwapSideTwoDone && Mission.Current.Scene.SwapFaceConnectionsWithID(_bridgeNavmeshId + 2, _bridgeNavmeshId + 4, AttachmentSource.RelatedShipNavmeshOffset + AttachmentSource.OwnerShip.GetDynamicNavmeshIdStart(), true))
					{
						_faceSwapSideTwoDone = true;
					}
					_bridgeCreated = true;
				}
				if (_faceSwapSideOneDone && _faceSwapSideTwoDone && !ShipIslandsConnected)
				{
					ShipIslandsConnected = true;
					MissionShip.MergeShipIslands(AttachmentSource.OwnerShip, AttachmentTarget.OwnerShip);
				}
			}
			if (_state == ShipAttachmentState.BridgeThrown || _state == ShipAttachmentState.BridgeConnected)
			{
				CommittedWeightedPosition = AttachmentSource.SteppedAgentManager.WeightedPosition;
				CommittedAgentCount = AttachmentSource.SteppedAgentManager.AgentCount;
				CommittedTotalMass = AttachmentSource.SteppedAgentManager.TotalMass;
				AttachmentSource.SteppedAgentManager.ClearAgentWeightAndPositionInformation();
			}
		}

		private void CheckAndBreakAttachment(float dt)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			_shipBetweenAttachmentsCheckTimer -= dt;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)AttachmentSource).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			gameEntity = ((ScriptComponentBehavior)AttachmentTarget).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			if (globalFrame.rotation.u.z < 0.17364818f || globalFrame2.rotation.u.z < 0.17364818f)
			{
				SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
				return;
			}
			if (_shipBetweenAttachmentsCheckTimer <= 0f)
			{
				_shipBetweenAttachmentsCheckTimer = MBRandom.RandomFloatRanged(0.1f, 0.15f);
				if (IsShipBetweenAttachments(AttachmentSource, AttachmentTarget))
				{
					SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
					return;
				}
			}
			if (!CheckAttachmentsFacingEachOther(AttachmentSource, AttachmentTarget))
			{
				SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
				return;
			}
			ShipAttachmentMachine attachmentSource = AttachmentSource;
			if (attachmentSource != null && attachmentSource.OwnerShip?.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating)
			{
				ShipAttachmentPointMachine attachmentTarget = AttachmentTarget;
				if (attachmentTarget != null && attachmentTarget.OwnerShip?.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating)
				{
					if (_state == ShipAttachmentState.RopesPulling && ((_ropesPullingTimer.Check(false) && (MBMath.ApproximatelyEquals(_currentRopeLength, AttachmentSource.RopeMinLength, 0.05f) || (AttachmentSource.OwnerShip.Team != null && (AttachmentSource.OwnerShip.Team.TeamAI as TeamAINavalComponent).TeamNavalQuerySystem.IsAnyShipInCriticalZoneBetween(AttachmentSource.OwnerShip, AttachmentTarget.OwnerShip)))) || CheckIntersectionsBetweenConnectionsWithState(AttachmentSource, AttachmentTarget, ShipAttachmentState.BridgeConnected)))
					{
						SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
					}
					else if (ShipAttachmentJoint != null && ShipAttachmentJoint.IsBroken)
					{
						SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
					}
					return;
				}
			}
			SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
		}

		public void InitializeRopeFlightDataAccordingToTargetPoint(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			float num = CalculateLaunchAngleDegree(sourceGlobalPosition, targetGlobalPosition, 20f);
			if (num == float.MinValue)
			{
				num = MathF.Clamp(num, Math.Min(44.9999f, CalculateDifferenceVectorAngle(in sourceGlobalPosition, in targetGlobalPosition) + 0.1f), 45f);
			}
			(Vec3, float) tuple = CalculateInitialVelocityAndTime(sourceGlobalPosition, targetGlobalPosition, num);
			_launchFlightData = new FlightData(in sourceGlobalPosition, in targetGlobalPosition, in tuple.Item1, num, tuple.Item2);
		}

		public void InitializeRopeFlightDataAccordingToTargetDirection(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalDirection)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			_launchFlightData = new FlightData(in sourceGlobalPosition, in Vec3.Zero, targetGlobalDirection * 25f, MathF.Asin(targetGlobalDirection.z) * 180f / MathF.PI, 0f);
		}

		private Vec3 CalculateRelativeVelocityBetweenAttachments()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			MissionShip ownerShip = AttachmentSource.OwnerShip;
			MissionShip ownerShip2 = AttachmentTarget.OwnerShip;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)ownerShip).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			gameEntity = ((ScriptComponentBehavior)ownerShip2).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			Vec3 localCenterOfMass = ownerShip.Physics.LocalCenterOfMass;
			Vec3 val = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref localCenterOfMass);
			localCenterOfMass = ownerShip2.Physics.LocalCenterOfMass;
			Vec3 val2 = ((MatrixFrame)(ref globalFrame2)).TransformToParent(ref localCenterOfMass);
			MatrixFrame globalFrame3 = AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrame();
			MatrixFrame globalFrame4 = AttachmentTarget.ConnectionClipPlaneEntity.GetGlobalFrame();
			Vec3 val3 = ownerShip.Physics.LinearVelocity + Vec3.CrossProduct(ownerShip.Physics.AngularVelocity, globalFrame3.origin - val);
			return ownerShip2.Physics.LinearVelocity + Vec3.CrossProduct(ownerShip2.Physics.AngularVelocity, globalFrame4.origin - val2) - val3;
		}

		private void UpdateRopeMeshVisualAccordingToTargetPoint(in Vec3 sourceGlobalPosition, in Vec3 targetGlobalPosition, float throwingAngleDegree)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			throwingAngleDegree = MathF.Clamp(throwingAngleDegree, Math.Min(89.99f, CalculateDifferenceVectorAngle(in sourceGlobalPosition, in targetGlobalPosition) + 0.1f), 89.999f);
			(Vec3, float) tuple = CalculateInitialVelocityAndTime(sourceGlobalPosition, targetGlobalPosition, throwingAngleDegree);
			_hookGlobalFrame = AttachmentSource.RopeVisual.UpdateRopeMeshVisualAccordingToTargetPoint(in sourceGlobalPosition, in targetGlobalPosition, in tuple.Item1, tuple.Item2);
		}

		public void CheckAndConnectBridge(bool forceBridge = false)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_025f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0275: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			MatrixFrame globalFrame = AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrame();
			MatrixFrame globalFrame2 = AttachmentTarget.ConnectionClipPlaneEntity.GetGlobalFrame();
			float num = ((Vec3)(ref globalFrame.origin)).DistanceSquared(globalFrame2.origin);
			Vec3 val = CalculateRelativeVelocityBetweenAttachments();
			float lengthSquared = ((Vec3)(ref val)).LengthSquared;
			Vec3 val2 = globalFrame2.origin - globalFrame.origin;
			((Vec3)(ref val2)).Normalize();
			Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			float num2 = Vec2.DotProduct(((Vec2)(ref asVec)).Normalized(), ((Vec3)(ref val2)).AsVec2);
			asVec = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
			float num3 = Vec2.DotProduct(((Vec2)(ref asVec)).Normalized(), -((Vec3)(ref val2)).AsVec2);
			float num4 = (num2 + num3) * 0.5f;
			ShipAttachmentPointMachine shipAttachmentPointMachine = null;
			if (!forceBridge)
			{
				foreach (ShipAttachmentPointMachine item in (List<ShipAttachmentPointMachine>)(object)AttachmentTarget.OwnerShip?.AttachmentPointMachines)
				{
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)item).GameEntity;
					MatrixFrame globalFrame3 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
					Vec3 val3 = globalFrame3.origin - globalFrame.origin;
					float lengthSquared2 = ((Vec3)(ref val3)).LengthSquared;
					((Vec3)(ref val3)).Normalize();
					asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
					float num5 = Vec2.DotProduct(((Vec2)(ref asVec)).Normalized(), ((Vec3)(ref val3)).AsVec2);
					asVec = ((Vec3)(ref globalFrame3.rotation.f)).AsVec2;
					float num6 = Vec2.DotProduct(((Vec2)(ref asVec)).Normalized(), -((Vec3)(ref val3)).AsVec2);
					float num7 = (num5 + num6) * 0.5f;
					if (item.CurrentAttachment == null && item.LinkedAttachmentMachine?.CurrentAttachment == null && lengthSquared2 < ShipAttachmentJoint.CalculatePossibleBridgeConnectionLengthSquared(AttachmentSource, item) && lengthSquared <= 4f && num5 > 0.18f && num6 > 0.18f && num7 > num4 && !CheckIntersectionsBetweenConnections(AttachmentSource, item))
					{
						shipAttachmentPointMachine = item;
					}
				}
			}
			if (shipAttachmentPointMachine != null)
			{
				Destroy();
				AttachmentSource.ConnectWithAttachmentPointMachine(shipAttachmentPointMachine, forceBridge: true);
			}
			else if (forceBridge || (num < AttachmentSource.BridgeConnectionLengthSquared && lengthSquared <= 4f && num2 > 0.18f && num3 > 0.18f))
			{
				StartBridgeThrowAnimation();
				val = (globalFrame.origin + globalFrame2.origin) / 2f;
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/bridge_connect", ref val);
			}
		}

		public void InitializeShipAttachmentJoint(Vec3 attachmentSourceGlobalPosition, Vec3 attachmentTargetGlobalPosition, bool unbreakableJoint = false)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected O, but got Unknown
			Vec2 asVec = ((Vec3)(ref attachmentSourceGlobalPosition)).AsVec2;
			_currentRopeLength = ((Vec2)(ref asVec)).Distance(((Vec3)(ref attachmentTargetGlobalPosition)).AsVec2) + 0.1f;
			ShipAttachmentJoint = new ShipAttachmentJoint(AttachmentSource, AttachmentTarget, unbreakableJoint);
			SetAttachmentState(ShipAttachmentState.RopesPulling);
			GameEntity val = GameEntity.Instantiate(Mission.Current.Scene, _shipConnectionPlankVariations[0], false, true, "");
			Vec3 val2 = val.GetBoundingBoxMax() - val.GetBoundingBoxMin();
			_plankVerticalSize = val.GetLocalScale().y * val2.y;
			_plankHorizontalSize = val.GetLocalScale().x * val2.x;
			val.Remove(78);
			_bridgeSwapTimer = new Timer(Mission.Current.CurrentTime, 0f, true);
			if (!unbreakableJoint && !_hookAttachSoundAlreadyTriggered)
			{
				bool flag = Agent.Main != null && Agent.Main.IsActive() && (AttachmentSource.OwnerShip.GetIsAgentOnShip(Agent.Main) || AttachmentTarget.OwnerShip.GetIsAgentOnShip(Agent.Main));
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_impact_attach", ref attachmentTargetGlobalPosition, "isPlayer", flag ? 1f : 0f);
			}
			_hookAttachSoundAlreadyTriggered = false;
			AttachmentSource.OwnerShip.ShipsLogic.OnSuccessfulHookThrow(AttachmentSource.OwnerShip, AttachmentTarget.OwnerShip);
			_sideBarrierIndicesCached[0] = 0;
			_sideBarrierIndicesCached[1] = 1;
			_sideBarrierIndicesCached[2] = 2;
			_sideBarrierIndicesCached[3] = 0;
			_sideBarrierIndicesCached[4] = 2;
			_sideBarrierIndicesCached[5] = 3;
			_vFoldQuadsIndicesCached[0] = 2;
			_vFoldQuadsIndicesCached[1] = 1;
			_vFoldQuadsIndicesCached[2] = 0;
			_vFoldQuadsIndicesCached[3] = 3;
			_vFoldQuadsIndicesCached[4] = 2;
			_vFoldQuadsIndicesCached[5] = 0;
		}

		private void UpdateRopeThrowingBehavior(float dt)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0603: Unknown result type (might be due to invalid IL or missing references)
			//IL_0608: Unknown result type (might be due to invalid IL or missing references)
			//IL_0623: Unknown result type (might be due to invalid IL or missing references)
			//IL_062f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0507: Unknown result type (might be due to invalid IL or missing references)
			//IL_050c: Unknown result type (might be due to invalid IL or missing references)
			//IL_050f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0514: Unknown result type (might be due to invalid IL or missing references)
			//IL_051e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0523: Unknown result type (might be due to invalid IL or missing references)
			//IL_0526: Unknown result type (might be due to invalid IL or missing references)
			//IL_052b: Unknown result type (might be due to invalid IL or missing references)
			//IL_052d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0535: Unknown result type (might be due to invalid IL or missing references)
			//IL_053a: Unknown result type (might be due to invalid IL or missing references)
			//IL_053f: Unknown result type (might be due to invalid IL or missing references)
			//IL_06d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0648: Unknown result type (might be due to invalid IL or missing references)
			//IL_064a: Unknown result type (might be due to invalid IL or missing references)
			//IL_065b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0660: Unknown result type (might be due to invalid IL or missing references)
			//IL_066b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0670: Unknown result type (might be due to invalid IL or missing references)
			//IL_0675: Unknown result type (might be due to invalid IL or missing references)
			//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05de: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_078f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0794: Unknown result type (might be due to invalid IL or missing references)
			//IL_0797: Unknown result type (might be due to invalid IL or missing references)
			//IL_079c: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_072a: Unknown result type (might be due to invalid IL or missing references)
			//IL_072f: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0405: Unknown result type (might be due to invalid IL or missing references)
			//IL_040a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0420: Unknown result type (might be due to invalid IL or missing references)
			//IL_0425: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_042b: Unknown result type (might be due to invalid IL or missing references)
			//IL_042e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0433: Unknown result type (might be due to invalid IL or missing references)
			//IL_0453: Unknown result type (might be due to invalid IL or missing references)
			//IL_0458: Unknown result type (might be due to invalid IL or missing references)
			//IL_045b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0460: Unknown result type (might be due to invalid IL or missing references)
			//IL_047b: Unknown result type (might be due to invalid IL or missing references)
			//IL_048b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0490: Unknown result type (might be due to invalid IL or missing references)
			//IL_0495: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0302: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_0322: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0337: Unknown result type (might be due to invalid IL or missing references)
			//IL_033c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0969: Unknown result type (might be due to invalid IL or missing references)
			//IL_096e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0971: Unknown result type (might be due to invalid IL or missing references)
			//IL_0976: Unknown result type (might be due to invalid IL or missing references)
			//IL_097c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0981: Unknown result type (might be due to invalid IL or missing references)
			//IL_0984: Unknown result type (might be due to invalid IL or missing references)
			//IL_0989: Unknown result type (might be due to invalid IL or missing references)
			//IL_0991: Unknown result type (might be due to invalid IL or missing references)
			//IL_0993: Unknown result type (might be due to invalid IL or missing references)
			//IL_0995: Unknown result type (might be due to invalid IL or missing references)
			//IL_099a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0806: Unknown result type (might be due to invalid IL or missing references)
			//IL_080b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0811: Unknown result type (might be due to invalid IL or missing references)
			//IL_0816: Unknown result type (might be due to invalid IL or missing references)
			//IL_0819: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a4e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a50: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a87: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a8c: Unknown result type (might be due to invalid IL or missing references)
			//IL_082f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0834: Unknown result type (might be due to invalid IL or missing references)
			//IL_0837: Unknown result type (might be due to invalid IL or missing references)
			//IL_083c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0841: Unknown result type (might be due to invalid IL or missing references)
			//IL_0846: Unknown result type (might be due to invalid IL or missing references)
			//IL_084a: Unknown result type (might be due to invalid IL or missing references)
			//IL_084f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0855: Unknown result type (might be due to invalid IL or missing references)
			//IL_085a: Unknown result type (might be due to invalid IL or missing references)
			//IL_085e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0863: Unknown result type (might be due to invalid IL or missing references)
			//IL_0893: Unknown result type (might be due to invalid IL or missing references)
			//IL_0898: Unknown result type (might be due to invalid IL or missing references)
			//IL_089e: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
			_ropeThrownTimer += dt;
			if (((Vec3)(ref _launchFlightData.GlobalPositionError)).LengthSquared > 1.0000001E-06f)
			{
				ref Vec3 globalPositionError = ref _launchFlightData.GlobalPositionError;
				globalPositionError *= 1f - dt * 8f;
			}
			else
			{
				_launchFlightData.GlobalPositionError = Vec3.Zero;
			}
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)AttachmentSource.RopeVisual).GameEntity;
			Vec3 sourceGlobalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			Vec3 val2;
			if (_state == ShipAttachmentState.RopeFailedAndReloading)
			{
				_launchFlightData.GlobalVelocity = _launchFlightData.GlobalVelocity * (1f - dt * (_launchFlightData.IsUnderWater ? 8f : 1f)) + MBGlobals.GravitationalAcceleration * dt;
				Vec3 sourceGlobalPosition2 = _launchFlightData.SourceGlobalPosition;
				ref Vec3 sourceGlobalPosition3 = ref _launchFlightData.SourceGlobalPosition;
				sourceGlobalPosition3 += _launchFlightData.GlobalVelocity * dt;
				if (((Vec3)(ref _launchFlightData.SourceGlobalPosition)).DistanceSquared(sourceGlobalPosition) > 1600f)
				{
					ref FlightData launchFlightData = ref _launchFlightData;
					Vec3 val = sourceGlobalPosition;
					val2 = _launchFlightData.SourceGlobalPosition - sourceGlobalPosition;
					launchFlightData.SourceGlobalPosition = val + ((Vec3)(ref val2)).NormalizedCopy() * 40f;
					_launchFlightData.GlobalVelocity = (_launchFlightData.SourceGlobalPosition - sourceGlobalPosition2) / dt;
				}
				if (_launchFlightData.IsUnderWater)
				{
					_launchFlightData.SourceGlobalPosition.z = Math.Min(((ScriptComponentBehavior)AttachmentSource).Scene.GetWaterLevelAtPosition(((Vec3)(ref _launchFlightData.SourceGlobalPosition)).AsVec2, true, false), _launchFlightData.SourceGlobalPosition.z);
				}
				else if (((ScriptComponentBehavior)AttachmentSource).Scene.GetWaterLevelAtPosition(((Vec3)(ref _launchFlightData.SourceGlobalPosition)).AsVec2, true, false) > _launchFlightData.SourceGlobalPosition.z)
				{
					_launchFlightData.IsUnderWater = true;
				}
				if (_currentRopeLength <= 0f)
				{
					if (!_launchFlightData.IsUnderWater)
					{
						_ropeThrownTimer -= dt * 0.8f;
					}
					float num = MathF.Clamp(MathF.Pow(_ropeThrownTimer / _launchFlightData.Time, 1.3f), 0f, 1f);
					if (num >= 1f)
					{
						_currentRopeLength = ((Vec3)(ref sourceGlobalPosition)).Distance(_launchFlightData.SourceGlobalPosition);
						_hookGlobalFrame.origin = AttachmentSource.RopeVisual.UpdateRopeMeshVisualAccordingToTargetPointLinear(in sourceGlobalPosition, in _launchFlightData.SourceGlobalPosition);
						ref Mat3 rotation = ref _hookGlobalFrame.rotation;
						val2 = _launchFlightData.SourceGlobalPosition - sourceGlobalPosition;
						rotation.f = ((Vec3)(ref val2)).NormalizedCopy();
						ref Mat3 rotation2 = ref _hookGlobalFrame.rotation;
						val2 = ((Vec3)(ref _hookGlobalFrame.rotation.f)).CrossProductWithUp();
						rotation2.s = ((Vec3)(ref val2)).NormalizedCopy();
						_hookGlobalFrame.rotation.u = Vec3.CrossProduct(_hookGlobalFrame.rotation.s, _hookGlobalFrame.rotation.f);
						((Mat3)(ref _hookGlobalFrame.rotation)).RotateAboutSide(-MathF.PI / 2f);
					}
					else
					{
						UpdateRopeMeshVisualAccordingToTargetPoint(in sourceGlobalPosition, in _launchFlightData.SourceGlobalPosition, _launchFlightData.AngleDegree - num * (_launchFlightData.AngleDegree - CalculateDifferenceVectorAngle(in sourceGlobalPosition, in _launchFlightData.SourceGlobalPosition) - 0.1f));
					}
					return;
				}
				_currentRopeLength -= dt * 4f;
				ref FlightData launchFlightData2 = ref _launchFlightData;
				Vec3 val3 = sourceGlobalPosition;
				val2 = _launchFlightData.SourceGlobalPosition - sourceGlobalPosition;
				launchFlightData2.SourceGlobalPosition = val3 + ((Vec3)(ref val2)).NormalizedCopy() * _currentRopeLength;
				_hookGlobalFrame.origin = AttachmentSource.RopeVisual.UpdateRopeMeshVisualAccordingToTargetPointLinear(in sourceGlobalPosition, in _launchFlightData.SourceGlobalPosition);
				ref Mat3 rotation3 = ref _hookGlobalFrame.rotation;
				val2 = _launchFlightData.SourceGlobalPosition - sourceGlobalPosition;
				rotation3.f = ((Vec3)(ref val2)).NormalizedCopy();
				ref Mat3 rotation4 = ref _hookGlobalFrame.rotation;
				val2 = ((Vec3)(ref _hookGlobalFrame.rotation.f)).CrossProductWithUp();
				rotation4.s = ((Vec3)(ref val2)).NormalizedCopy();
				_hookGlobalFrame.rotation.u = Vec3.CrossProduct(_hookGlobalFrame.rotation.s, _hookGlobalFrame.rotation.f);
				((Mat3)(ref _hookGlobalFrame.rotation)).RotateAboutSide(-MathF.PI / 2f);
				if (_currentRopeLength <= 0f)
				{
					_currentRopeLength = 0f;
					SetAttachmentState(ShipAttachmentState.BrokenAndWaitingForRemoval);
				}
				return;
			}
			float num2 = _launchFlightData.AngleDegree - _ropeThrownTimer * 5f;
			MatrixFrame globalFrame;
			if (_launchFlightData.Time > 0f)
			{
				gameEntity = ((ScriptComponentBehavior)AttachmentTarget).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				val2 = AttachmentTarget.HookAttachLocalPosition;
				Vec3 val4 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2);
				Vec3 targetGlobalPosition = val4 + _launchFlightData.GlobalPositionError;
				if (_ropeThrownTimer >= _launchFlightData.Time)
				{
					float num3 = MathF.Clamp(MathF.Pow((_ropeThrownTimer - _launchFlightData.Time) / _launchFlightData.Time, 1.3f), 0f, 1f);
					UpdateRopeMeshVisualAccordingToTargetPoint(in sourceGlobalPosition, in targetGlobalPosition, num2 - num3 * (num2 - CalculateDifferenceVectorAngle(in sourceGlobalPosition, in targetGlobalPosition) - 0.1f));
					if (num3 >= 1f)
					{
						InitializeShipAttachmentJoint(sourceGlobalPosition, val4);
					}
				}
				else
				{
					Vec3 launchProjectileCurrentGlobalPosition = GetLaunchProjectileCurrentGlobalPosition(_ropeThrownTimer);
					UpdateRopeMeshVisualAccordingToTargetPoint(in sourceGlobalPosition, launchProjectileCurrentGlobalPosition + (targetGlobalPosition - _launchFlightData.TargetGlobalPosition), num2);
				}
				return;
			}
			Vec3 targetGlobalPosition2 = GetLaunchProjectileCurrentGlobalPosition(_ropeThrownTimer);
			UpdateRopeMeshVisualAccordingToTargetPoint(in sourceGlobalPosition, in targetGlobalPosition2, num2);
			if (((ScriptComponentBehavior)AttachmentSource).Scene.GetWaterLevelAtPosition(((Vec3)(ref targetGlobalPosition2)).AsVec2, true, false) > targetGlobalPosition2.z)
			{
				SetAttachmentState(ShipAttachmentState.RopeFailedAndReloading);
				_launchFlightData.SourceGlobalPosition = targetGlobalPosition2;
				ref Vec3 globalVelocity = ref _launchFlightData.GlobalVelocity;
				globalVelocity += MBGlobals.GravitationalAcceleration * _ropeThrownTimer;
				_launchFlightData.AngleDegree = num2;
				_launchFlightData.Time = Math.Min(2.5f, _ropeThrownTimer);
				_launchFlightData.IsUnderWater = true;
				_ropeThrownTimer = 0f;
				_currentRopeLength = 0f;
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_impact_fail_water_splash", ref targetGlobalPosition2);
				return;
			}
			if (((Vec3)(ref targetGlobalPosition2)).DistanceSquared(sourceGlobalPosition) > 1600f)
			{
				SetAttachmentState(ShipAttachmentState.RopeFailedAndReloading);
				_launchFlightData.SourceGlobalPosition = targetGlobalPosition2;
				_launchFlightData.GlobalVelocity = new Vec3(0f, 0f, _launchFlightData.GlobalVelocity.z - 9.806f * _ropeThrownTimer, -1f);
				_launchFlightData.AngleDegree = num2;
				_launchFlightData.Time = Math.Min(2.5f, _ropeThrownTimer);
				_ropeThrownTimer = 0f;
				_currentRopeLength = 0f;
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_impact_fail_to_attach", ref targetGlobalPosition2);
				return;
			}
			gameEntity = ((ScriptComponentBehavior)AttachmentSource).GameEntity;
			WeakGameEntity attachmentSourceHolderEntity = ((WeakGameEntity)(ref gameEntity)).Parent;
			IEnumerable<WeakGameEntity> enumerable = from x in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<ShipAttachmentPointMachine>()
				where ((WeakGameEntity)(ref x)).Parent != attachmentSourceHolderEntity
				select x;
			ShipAttachmentPointMachine shipAttachmentPointMachine = null;
			foreach (WeakGameEntity item in enumerable)
			{
				WeakGameEntity current = item;
				ShipAttachmentPointMachine firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<ShipAttachmentPointMachine>();
				if (firstScriptOfType.CurrentAttachment != null || firstScriptOfType.LinkedAttachmentMachine?.CurrentAttachment != null)
				{
					continue;
				}
				globalFrame = ((WeakGameEntity)(ref current)).GetGlobalFrame();
				val2 = firstScriptOfType.HookAttachLocalPosition;
				if (((Vec3)(ref targetGlobalPosition2)).DistanceSquared(((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2)) < 9f)
				{
					gameEntity = ((ScriptComponentBehavior)firstScriptOfType).GameEntity;
					Vec3 f = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.f;
					Vec3 val5 = targetGlobalPosition2;
					globalFrame = ((WeakGameEntity)(ref current)).GetGlobalFrame();
					Vec3 hookAttachLocalPosition = firstScriptOfType.HookAttachLocalPosition;
					if (Vec3.DotProduct(f, val5 - ((MatrixFrame)(ref globalFrame)).TransformToParent(ref hookAttachLocalPosition)) < 0f && ComputePotentialAttachmentValue(AttachmentSource, firstScriptOfType, checkInteractionDistance: false, checkConnectionBlock: true, allowWiderAngleBetweenConnections: true) > 0f)
					{
						shipAttachmentPointMachine = firstScriptOfType;
						globalFrame = ((WeakGameEntity)(ref current)).GetGlobalFrame();
						val2 = firstScriptOfType.HookAttachLocalPosition;
						((Vec3)(ref targetGlobalPosition2)).DistanceSquared(((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2));
						break;
					}
				}
			}
			if (shipAttachmentPointMachine != null)
			{
				if (_attachmentInitializedByPlayer && AttachmentSource.OwnerShip != null && AttachmentSource.OwnerShip.Team != null && AttachmentSource.OwnerShip.Team.IsPlayerTeam)
				{
					AttachmentSource.OwnerShip?.ShipOrder?.SetBoardingTargetShip(shipAttachmentPointMachine.OwnerShip);
				}
				shipAttachmentPointMachine.AssignConnection(this);
				AttachmentTarget = shipAttachmentPointMachine;
				UpdateAttachmentMachineEntityVisibilities(_state);
				_launchFlightData.Time = _ropeThrownTimer;
				gameEntity = ((ScriptComponentBehavior)shipAttachmentPointMachine).GameEntity;
				globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				val2 = shipAttachmentPointMachine.HookAttachLocalPosition;
				Vec3 val6 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2);
				_launchFlightData.GlobalPositionError = targetGlobalPosition2 - val6;
				if ((((UsableMissionObject)((UsableMachine)AttachmentSource).PilotStandingPoint).UserAgent != null && ((UsableMissionObject)((UsableMachine)AttachmentSource).PilotStandingPoint).UserAgent.IsMainAgent) || (((UsableMissionObject)((UsableMachine)AttachmentSource).PilotStandingPoint).UserAgent == null && ((UsableMissionObject)((UsableMachine)AttachmentSource).PilotStandingPoint).PreviousUserAgent != null && ((UsableMissionObject)((UsableMachine)AttachmentSource).PilotStandingPoint).PreviousUserAgent.IsMainAgent))
				{
					_hookAttachSoundAlreadyTriggered = SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_impact_attach", ref val6, "isPlayer", 1f);
				}
			}
			if (AttachmentTarget != null && CheckIntersectionsBetweenConnectionsWithState(AttachmentSource, AttachmentTarget, ShipAttachmentState.BridgeConnected))
			{
				SetAttachmentState(ShipAttachmentState.RopeFailedAndReloading);
				_launchFlightData.SourceGlobalPosition = targetGlobalPosition2;
				_launchFlightData.GlobalVelocity = new Vec3(0f, 0f, _launchFlightData.GlobalVelocity.z - 9.806f * _ropeThrownTimer, -1f);
				_launchFlightData.AngleDegree = num2;
				_launchFlightData.Time = Math.Min(2.5f, _ropeThrownTimer);
				_ropeThrownTimer = 0f;
				_currentRopeLength = 0f;
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_impact_fail_to_attach", ref targetGlobalPosition2);
			}
		}

		public void OnFixedTick(float fixedDt)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			if (_state == ShipAttachmentState.RopesPulling || _state == ShipAttachmentState.BridgeConnected || _state == ShipAttachmentState.BridgeThrown)
			{
				ShipAttachmentJoint.OnFixedTick(fixedDt, this, ref _currentRopeLength);
			}
			if ((_state == ShipAttachmentState.BridgeConnected || _state == ShipAttachmentState.BridgeThrown) && CommittedAgentCount > 0f && CommittedTotalMass > 0f && CommittedWeightedPosition != Vec3.Zero)
			{
				Vec3 val = CommittedWeightedPosition / CommittedTotalMass;
				if (((Vec3)(ref val)).DistanceSquared(AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrameImpreciseForFixedTick().origin) < 25f)
				{
					MatrixFrame globalFrameImpreciseForFixedTick = AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrameImpreciseForFixedTick();
					Vec3 val2 = AttachmentTarget.ConnectionClipPlaneEntity.GetGlobalFrameImpreciseForFixedTick().origin - globalFrameImpreciseForFixedTick.origin;
					float num = ((Vec3)(ref val2)).Normalize();
					Vec3 val3 = val - globalFrameImpreciseForFixedTick.origin;
					float num2 = Vec3.DotProduct(val2, val3) / num;
					MissionShip ownerShip = AttachmentSource.OwnerShip;
					MissionShip ownerShip2 = AttachmentSource.OwnerShip;
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)ownerShip).GameEntity;
					MatrixFrame bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
					Vec3 localPos = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val);
					gameEntity = ((ScriptComponentBehavior)ownerShip2).GameEntity;
					bodyWorldTransform = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform();
					Vec3 localPos2 = ((MatrixFrame)(ref bodyWorldTransform)).TransformToLocal(ref val);
					float stepAgentWeightMultiplier = ownerShip.Physics.PhysicsParameters.StepAgentWeightMultiplier;
					float stepAgentWeightMultiplier2 = ownerShip2.Physics.PhysicsParameters.StepAgentWeightMultiplier;
					Vec3 val4 = CommittedTotalMass * MBGlobals.GravitationalAcceleration;
					ownerShip.Physics.ApplyGlobalForceAtLocalPos(in localPos, val4 * ((1f - num2) * stepAgentWeightMultiplier), (ForceMode)0);
					ownerShip2.Physics.ApplyGlobalForceAtLocalPos(in localPos2, val4 * (num2 * stepAgentWeightMultiplier2), (ForceMode)0);
				}
			}
			ClearCommittedAgentInformation();
		}

		private void ArrangeBarrier(GameEntity barrier, Vec3 startPosition, Vec3 endPosition, float height)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			MatrixFrame val = default(MatrixFrame);
			val.origin = Vec3.Zero;
			val.rotation = Mat3.Identity;
			Vec3[] sideBarrierQuadsCached = _sideBarrierQuadsCached;
			Vec3 val2 = startPosition + new Vec3(0f, 0f, height, -1f);
			sideBarrierQuadsCached[0] = ((MatrixFrame)(ref val)).TransformToLocal(ref val2);
			Vec3[] sideBarrierQuadsCached2 = _sideBarrierQuadsCached;
			val2 = endPosition + new Vec3(0f, 0f, height, -1f);
			sideBarrierQuadsCached2[1] = ((MatrixFrame)(ref val)).TransformToLocal(ref val2);
			_sideBarrierQuadsCached[2] = ((MatrixFrame)(ref val)).TransformToLocal(ref endPosition);
			_sideBarrierQuadsCached[3] = ((MatrixFrame)(ref val)).TransformToLocal(ref startPosition);
			GameEntityPhysicsExtensions.ReplacePhysicsBodyWithQuadPhysicsBody(barrier, _sideBarriersQuadPinnedPointer, 4, _woodPhysicsMaterialCached, (BodyFlags)272, _sideBarriersIndicesPinnedPointer, 6);
			barrier.SetGlobalFrame(ref val, true);
		}

		private void ConnectBridge()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < 4; i++)
			{
				string text = _shipConnectionPlankVariations[MBRandom.RandomInt(0, _shipConnectionPlankVariations.Count - 1)];
				GameEntity val = GameEntity.Instantiate(Mission.Current.Scene, text, MatrixFrame.Identity, true, "");
				_bridge.AddChild(val, false);
				_targetSafetyPlanks.Add(val);
			}
			for (int j = 0; j < 4; j++)
			{
				string text2 = _shipConnectionPlankVariations[MBRandom.RandomInt(0, _shipConnectionPlankVariations.Count - 1)];
				GameEntity val2 = GameEntity.Instantiate(Mission.Current.Scene, text2, MatrixFrame.Identity, true, "");
				_bridge.AddChild(val2, false);
				_sourceSafetyPlanks.Add(val2);
			}
			_bridgeNavmeshId = Mission.Current.GetNextDynamicNavMeshIdStart();
			_navMeshBridge = GameEntity.Instantiate(Mission.Current.Scene, "ship_connection_nav_mesh_plank", MatrixFrame.Identity, true, "");
			_navMeshBridgeNavMeshHolder = _navMeshBridge.GetFirstChildEntityWithTag("navmesh_holder");
			_navMeshBridgeNavMeshHolder.CreateAndAddScriptComponent("ShipBridgeNavmeshHolder", true);
			_shipBridgeNavmeshHolder = _navMeshBridgeNavMeshHolder.GetFirstScriptOfType<ShipBridgeNavmeshHolder>();
			_shipBridgeNavmeshHolder.Initialize(_bridgeNavmeshId, AttachmentSource);
			SetAttachmentState(ShipAttachmentState.BridgeConnected);
			ArrangePlanksMT();
			ArrangePlanks();
			ArrangeNavmeshBridgeSideBarriersAndVFoldQuads();
			AddRopesToBridge();
			_bridge.CreateAndAddScriptComponent("ShipBridge", true);
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_shipBridgeNavmeshHolder).GameEntity;
			((WeakGameEntity)(ref gameEntity)).UpdateAttachedNavigationMeshFaces();
			_bridgeSwapTimer.Reset(Mission.Current.CurrentTime, 0.05f);
			_faceSwapSideOneDone = false;
			_faceSwapSideTwoDone = false;
			ShipIslandsConnected = false;
			AttachmentSource.OwnerShip.ShipsLogic.OnShipsConnected(AttachmentSource.OwnerShip, AttachmentTarget.OwnerShip);
		}

		private void SetShieldsVisibility(bool visible)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			MBReadOnlyList<ShipShieldComponent> shields = AttachmentSource.OwnerShip.Shields;
			WeakGameEntity gameEntity;
			if (((List<ShipShieldComponent>)(object)shields).Count > 0)
			{
				Vec3 origin = AttachmentSource.ConnectionClipPlaneEntity.ComputePreciseGlobalFrameForFixedTickSlow().origin;
				foreach (ShipShieldComponent item in (List<ShipShieldComponent>)(object)shields)
				{
					gameEntity = ((ScriptComponentBehavior)item).GameEntity;
					if (!((WeakGameEntity)(ref gameEntity)).IsValid)
					{
						continue;
					}
					if (visible)
					{
						item.DeregisterRampEntityDisablingShield(AttachmentSource.ConnectionClipPlaneEntity);
						continue;
					}
					gameEntity = ((ScriptComponentBehavior)item).GameEntity;
					Vec3 origin2 = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow().origin;
					if (((Vec3)(ref origin2)).DistanceSquared(origin) < 3f)
					{
						item.RegisterRampEntityDisablingShield(AttachmentSource.ConnectionClipPlaneEntity);
					}
				}
			}
			if (AttachmentTarget == null)
			{
				return;
			}
			MBReadOnlyList<ShipShieldComponent> shields2 = AttachmentTarget.OwnerShip.Shields;
			if (((List<ShipShieldComponent>)(object)shields2).Count <= 0)
			{
				return;
			}
			Vec3 origin3 = AttachmentTarget.ConnectionClipPlaneEntity.ComputePreciseGlobalFrameForFixedTickSlow().origin;
			foreach (ShipShieldComponent item2 in (List<ShipShieldComponent>)(object)shields2)
			{
				gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
				if (!((WeakGameEntity)(ref gameEntity)).IsValid)
				{
					continue;
				}
				if (visible)
				{
					item2.DeregisterRampEntityDisablingShield(AttachmentTarget.ConnectionClipPlaneEntity);
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
				Vec3 origin4 = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow().origin;
				if (((Vec3)(ref origin4)).DistanceSquared(origin3) < 3f)
				{
					item2.RegisterRampEntityDisablingShield(AttachmentTarget.ConnectionClipPlaneEntity);
				}
			}
		}

		private void ArrangeNavmeshBridgeSideBarriersAndVFoldQuads()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			MatrixFrame globalFrame = AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrame();
			MatrixFrame globalFrame2 = AttachmentTarget.ConnectionClipPlaneEntity.GetGlobalFrame();
			Vec3 s = globalFrame.rotation.s;
			((Vec3)(ref s)).Normalize();
			Vec3 s2 = globalFrame2.rotation.s;
			((Vec3)(ref s2)).Normalize();
			Vec3 val = globalFrame2.origin - s2 * _plankHorizontalSize * 0.5f;
			Vec3 val2 = globalFrame2.origin + s2 * _plankHorizontalSize * 0.5f;
			Vec3 val3 = globalFrame.origin + s * _plankHorizontalSize * 0.5f;
			Vec3 val4 = globalFrame.origin - s * _plankHorizontalSize * 0.5f;
			Vec3 val5 = val - val3;
			((Vec3)(ref val5)).Normalize();
			Vec3 val6 = val2 - val4;
			((Vec3)(ref val6)).Normalize();
			val += val5 * 0.05f;
			val2 += val6 * 0.05f;
			val3 -= val5 * 0.05f;
			val4 -= val6 * 0.05f;
			ArrangeBarrier(AttachmentSource.BarrierSource, val2, val4, 6f);
			ArrangeBarrier(AttachmentSource.BarrierTarget, val3, val, 6f);
			ArrangeVFoldQuads(val3, val4, val2, val);
			ArrangeNavMeshBridge(val3, val4, val, val2);
		}

		private void ArrangeVFoldQuads(Vec3 leftSource, Vec3 rightSource, Vec3 rightTarget, Vec3 leftTarget)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			Vec3 val = (leftSource + rightSource) * 0.5f - Vec3.Up * 0.5f;
			Vec3 val2 = (leftTarget + rightTarget) * 0.5f - Vec3.Up * 0.5f;
			MatrixFrame val3 = default(MatrixFrame);
			val3.origin = (leftSource + leftTarget + rightSource + rightTarget) * 0.25f;
			val3.rotation = Mat3.Identity;
			_vFoldQuadsCached[0] = ((MatrixFrame)(ref val3)).TransformToLocal(ref leftSource);
			_vFoldQuadsCached[1] = ((MatrixFrame)(ref val3)).TransformToLocal(ref leftTarget);
			_vFoldQuadsCached[2] = ((MatrixFrame)(ref val3)).TransformToLocal(ref val2);
			_vFoldQuadsCached[3] = ((MatrixFrame)(ref val3)).TransformToLocal(ref val);
			GameEntityPhysicsExtensions.ReplacePhysicsBodyWithQuadPhysicsBody(AttachmentSource.VFoldSource, _vFoldQuadPinnedPointer, 4, _defaultPhysicsMaterialCached, (BodyFlags)2097168, _vFoldIndicesPinnedPointer, 6);
			AttachmentSource.VFoldSource.SetGlobalFrame(ref val3, true);
			_vFoldQuadsCached[0] = ((MatrixFrame)(ref val3)).TransformToLocal(ref rightSource);
			_vFoldQuadsCached[1] = ((MatrixFrame)(ref val3)).TransformToLocal(ref val);
			_vFoldQuadsCached[2] = ((MatrixFrame)(ref val3)).TransformToLocal(ref val2);
			_vFoldQuadsCached[3] = ((MatrixFrame)(ref val3)).TransformToLocal(ref rightTarget);
			GameEntityPhysicsExtensions.ReplacePhysicsBodyWithQuadPhysicsBody(AttachmentSource.VFoldTarget, _vFoldQuadPinnedPointer, 4, _defaultPhysicsMaterialCached, (BodyFlags)2097168, _vFoldIndicesPinnedPointer, 6);
			AttachmentSource.VFoldTarget.SetGlobalFrame(ref val3, true);
		}

		private void StartBridgeThrowAnimation()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			_targetSafetyPlanks.Clear();
			_sourceSafetyPlanks.Clear();
			_bridgeFlightData.DtSinceFlightStart = 0f;
			_bridgeFlightData.CurveLerpVelocity = 0f;
			_bridgeFlightData.CurveLerpValue = 0f;
			_bridgeFlightData.ThrowFinishValue = 7f;
			MatrixFrame val = AttachmentSource.ConnectionClipPlaneEntity.ComputePreciseGlobalFrameForFixedTickSlow();
			_currentRopeLength = ((Vec3)(ref val.origin)).Distance(AttachmentTarget.ConnectionClipPlaneEntity.ComputePreciseGlobalFrameForFixedTickSlow().origin);
			SetAttachmentState(ShipAttachmentState.BridgeThrown);
		}

		private void TickThrownBridge(float dt)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			MatrixFrame globalFrame = AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrame();
			Vec3 initialPosition = globalFrame.origin;
			Vec3 destination = AttachmentTarget.ConnectionClipPlaneEntity.GetGlobalFrame().origin;
			if (MBMath.ApproximatelyEquals(((Vec3)(ref destination)).DistanceSquared(initialPosition), 0f, 1E-05f))
			{
				destination += globalFrame.rotation.f * 0.1f + globalFrame.rotation.u * 0.1f;
			}
			float launchSpeed = 10.327f;
			float num = CalculateLaunchAngleDegree(initialPosition, destination, launchSpeed);
			if (num == float.MinValue)
			{
				num = MathF.Clamp(num, MathF.Min(44.9999f, CalculateDifferenceVectorAngle(in initialPosition, in destination) + 0.1f), 45f);
			}
			(_bridgeFlightData.CurrentFrameInitialVelocity, _bridgeFlightData.CurrentFrameTotalLightTime) = CalculateInitialVelocityAndTime(initialPosition, destination, num);
			_bridgeFlightData.DtSinceFlightStart += dt;
			_bridgeFlightData.CurveLerpVelocity += dt * 3f;
			if (_bridgeFlightData.CurrentFrameTotalLightTime <= _bridgeFlightData.DtSinceFlightStart)
			{
				_bridgeFlightData.CurveLerpValue += _bridgeFlightData.CurveLerpVelocity * dt;
				if (_bridgeFlightData.CurveLerpValue > _bridgeFlightData.ThrowFinishValue)
				{
					ConnectBridge();
					return;
				}
			}
			ArrangePlanksMT();
			ArrangePlanks();
		}

		private void SetOarsAvailability(bool value)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			Vec3 origin = AttachmentSource.ConnectionClipPlaneEntity.ComputePreciseGlobalFrameForFixedTickSlow().origin;
			WeakGameEntity gameEntity;
			foreach (ShipOarMachine item in (List<ShipOarMachine>)(object)AttachmentSource.OwnerShip.LeftSideShipOarMachines)
			{
				if (value)
				{
					item.DeregisterRampEntityDisablingOar(AttachmentSource.ConnectionClipPlaneEntity);
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				Vec3 origin2 = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow().origin;
				if (((Vec3)(ref origin2)).DistanceSquared(origin) < 9f)
				{
					item.RegisterRampEntityDisablingOar(AttachmentSource.ConnectionClipPlaneEntity);
				}
			}
			foreach (ShipOarMachine item2 in (List<ShipOarMachine>)(object)AttachmentSource.OwnerShip.RightSideShipOarMachines)
			{
				if (value)
				{
					item2.DeregisterRampEntityDisablingOar(AttachmentSource.ConnectionClipPlaneEntity);
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
				Vec3 origin3 = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow().origin;
				if (((Vec3)(ref origin3)).DistanceSquared(origin) < 9f)
				{
					item2.RegisterRampEntityDisablingOar(AttachmentSource.ConnectionClipPlaneEntity);
				}
			}
			if (AttachmentTarget == null)
			{
				return;
			}
			Vec3 origin4 = AttachmentTarget.ConnectionClipPlaneEntity.ComputePreciseGlobalFrameForFixedTickSlow().origin;
			foreach (ShipOarMachine item3 in (List<ShipOarMachine>)(object)AttachmentTarget.OwnerShip.LeftSideShipOarMachines)
			{
				if (value)
				{
					item3.DeregisterRampEntityDisablingOar(AttachmentTarget.ConnectionClipPlaneEntity);
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)item3).GameEntity;
				Vec3 origin5 = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow().origin;
				if (((Vec3)(ref origin5)).DistanceSquared(origin4) < 9f)
				{
					item3.RegisterRampEntityDisablingOar(AttachmentTarget.ConnectionClipPlaneEntity);
				}
			}
			foreach (ShipOarMachine item4 in (List<ShipOarMachine>)(object)AttachmentTarget.OwnerShip.RightSideShipOarMachines)
			{
				if (value)
				{
					item4.DeregisterRampEntityDisablingOar(AttachmentTarget.ConnectionClipPlaneEntity);
					continue;
				}
				gameEntity = ((ScriptComponentBehavior)item4).GameEntity;
				Vec3 origin6 = ((WeakGameEntity)(ref gameEntity)).ComputePreciseGlobalFrameForFixedTickSlow().origin;
				if (((Vec3)(ref origin6)).DistanceSquared(origin4) < 9f)
				{
					item4.RegisterRampEntityDisablingOar(AttachmentTarget.ConnectionClipPlaneEntity);
				}
			}
		}

		private void AddRopesToBridge()
		{
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			_ = _numberOfPlanksNeeded;
			int num = (int)((float)_numberOfPlanksNeeded * 0.16f + MBRandom.RandomFloat * (float)_numberOfPlanksNeeded * 0.16f);
			for (int i = 0; i < num; i++)
			{
				RopeSegment item = default(RopeSegment);
				int num2 = 1 + MBRandom.RandomInt(3);
				int num3 = _numberOfPlanksNeeded - 5;
				item.StartSegmentIndex = (int)(3f + MBRandom.RandomFloat * (float)(num3 - 3));
				item.EndSegmentIndex = item.StartSegmentIndex + num2;
				item.SideStartShift = MBRandom.RandomFloat - 0.5f;
				item.SideEndShift = MBRandom.RandomFloat - 0.5f;
				if (item.StartSegmentIndex >= item.EndSegmentIndex || item.StartSegmentIndex <= 0 || item.EndSegmentIndex <= 0 || item.StartSegmentIndex >= _numberOfPlanksNeeded || item.EndSegmentIndex >= _numberOfPlanksNeeded)
				{
					continue;
				}
				GameEntity val = GameEntity.Instantiate(Mission.Current.Scene, "simple_rope_nested", MatrixFrame.Identity, true, "");
				_bridge.AddChild(val, false);
				item.ParentEntity = val;
				item.ParentEntity.SetDoNotCheckVisibility(true);
				item.RopeStart = val.GetFirstChildEntityWithTag("simple_rope_start");
				item.RopeEnd = val.GetFirstChildEntityWithTag("simple_rope_end");
				if (!(item.RopeStart != (GameEntity)null) || !(item.RopeEnd != (GameEntity)null))
				{
					continue;
				}
				NavalDLC.Missions.Objects.RopeSegment firstScriptOfType = item.RopeStart.GetFirstScriptOfType<NavalDLC.Missions.Objects.RopeSegment>();
				if (firstScriptOfType != null)
				{
					firstScriptOfType.SetAsFixedEntity();
					firstScriptOfType.SetRuntimeLooseMultiplier(2f);
				}
				_ropes.Add(item);
				if (MBRandom.RandomFloat > 0.6f)
				{
					int num4 = MBRandom.RandomInt(1, 2);
					for (int j = 0; j < num4; j++)
					{
						string text = _ropeClothFragmentPrefabList[MBRandom.RandomInt(0, _ropeClothFragmentPrefabList.Count - 1)];
						GameEntity val2 = GameEntity.Instantiate(Mission.Current.Scene, text, MatrixFrame.Identity, true, "");
						item.RopeStart.AddChild(val2, false);
					}
				}
			}
		}

		private void ArrangeNavMeshBridge(Vec3 leftSource, Vec3 rightSource, Vec3 leftTarget, Vec3 rightTarget)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			if (!(_navMeshBridge == (GameEntity)null) && AttachmentSource != null && AttachmentTarget != null)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)AttachmentSource).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)AttachmentTarget).GameEntity;
				Vec3 globalPosition2 = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				((Vec3)(ref globalPosition)).Distance(globalPosition2);
				MatrixFrame val = MatrixFrame.CenterFrameOfTwoPoints(ref globalPosition, ref globalPosition2, Vec3.Up);
				val.origin.z += 1.1f;
				((Mat3)(ref val.rotation)).Orthonormalize();
				_navMeshBridge.SetFrame(ref val, true);
				_shipBridgeNavmeshHolder.SetShipBridgeStartEndPositions(leftSource, rightSource, leftTarget, rightTarget);
				bool flag = IsNavmeshBridgeEntityUpsideDown();
				if (flag != _isNavmeshBridgeDisabled)
				{
					SetAbilityOfNavmeshBridgeFaces(!flag);
					_isNavmeshBridgeDisabled = flag;
				}
			}
		}

		public void Destroy()
		{
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			if (_bridgeCreated)
			{
				bool num = _faceSwapSideOneDone || _faceSwapSideTwoDone;
				if (_faceSwapSideOneDone)
				{
					Mission.Current.Scene.SwapFaceConnectionsWithID(_bridgeNavmeshId + 1, AttachmentTarget.RelatedShipNavmeshOffset + AttachmentTarget.OwnerShip.GetDynamicNavmeshIdStart(), _bridgeNavmeshId + 3, true);
					_faceSwapSideOneDone = false;
				}
				if (_faceSwapSideTwoDone)
				{
					Mission.Current.Scene.SwapFaceConnectionsWithID(_bridgeNavmeshId + 2, AttachmentSource.RelatedShipNavmeshOffset + AttachmentSource.OwnerShip.GetDynamicNavmeshIdStart(), _bridgeNavmeshId + 4, true);
					_faceSwapSideTwoDone = false;
				}
				if (num)
				{
					AttachmentSource.OwnerShip.SeparateFromShip(AttachmentTarget.OwnerShip);
				}
				MatrixFrame globalFrame = AttachmentSource.PlankBridgePhysicsEntity.GetGlobalFrame();
				SoundManager.StartOneShotEvent("event:/mission/movement/vessel/bridge_fall", ref globalFrame.origin);
			}
			AttachmentSource.CurrentAttachment = null;
			AttachmentTarget?.AssignConnection(null);
			if (_planks != null)
			{
				foreach (GameEntity plank in _planks)
				{
					plank.Remove(78);
				}
				_planks = null;
			}
			if (_targetSafetyPlanks != null)
			{
				foreach (GameEntity targetSafetyPlank in _targetSafetyPlanks)
				{
					targetSafetyPlank.Remove(35);
				}
				_targetSafetyPlanks = null;
			}
			if (_sourceSafetyPlanks != null)
			{
				foreach (GameEntity sourceSafetyPlank in _sourceSafetyPlanks)
				{
					sourceSafetyPlank.Remove(35);
				}
				_sourceSafetyPlanks = null;
			}
			if (_navMeshBridge != (GameEntity)null)
			{
				_navMeshBridge.Remove(78);
				Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId, false);
				Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 1, false);
				Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 2, false);
				Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 3, false);
				Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 4, false);
				_navMeshBridge = null;
			}
			AttachmentSource.SetConnectionPhysicsEntitiesVisibility(visible: false);
			if (_ropes != null)
			{
				foreach (RopeSegment rope in _ropes)
				{
					rope.ParentEntity.Remove(45);
				}
				_ropes = null;
			}
			if (_bridge != (GameEntity)null)
			{
				_bridge.Remove(78);
				_bridge = null;
			}
			_bridgeCurveLinearAccessCache = null;
			if (_currentFramePlankPhysicsVerticesPinnedPointer != UIntPtr.Zero)
			{
				_currentFramePlankPhysicsVerticesPinnedGCHandler.Free();
				_currentFramePlankPhysicsVerticesPinnedPointer = UIntPtr.Zero;
			}
			if (_currentFramePlankPhysicsIndicesPinnedPointer != UIntPtr.Zero)
			{
				_currentFramePlankPhysicsIndicesPinnedGCHandler.Free();
				_currentFramePlankPhysicsIndicesPinnedPointer = UIntPtr.Zero;
			}
			if (_sideBarriersQuadPinnedPointer != UIntPtr.Zero)
			{
				_sideBarriersQuadPinnedGCHandler.Free();
				_sideBarriersQuadPinnedPointer = UIntPtr.Zero;
			}
			if (_sideBarriersIndicesPinnedPointer != UIntPtr.Zero)
			{
				_sideBarriersIndicesPinnedGCHandler.Free();
				_sideBarriersIndicesPinnedPointer = UIntPtr.Zero;
			}
			if (_vFoldQuadPinnedPointer != UIntPtr.Zero)
			{
				_vFoldQuadPinnedGCHandler.Free();
				_vFoldQuadPinnedPointer = UIntPtr.Zero;
			}
			if (_vFoldIndicesPinnedPointer != UIntPtr.Zero)
			{
				_vFoldIndicesPinnedGCHandler.Free();
				_vFoldIndicesPinnedPointer = UIntPtr.Zero;
			}
		}

		private Vec3 GetCurvePositionFromLength(float currentLength)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			int num = Array.BinarySearch(_bridgeCurveLinearAccessCache, new KeyValuePair<float, Vec3>(currentLength, Vec3.Zero), _cacheCompareDelegate);
			if (num >= 0)
			{
				return _bridgeCurveLinearAccessCache[num].Value;
			}
			int num2 = ~num;
			int num3 = num2 - 1;
			KeyValuePair<float, Vec3> keyValuePair = _bridgeCurveLinearAccessCache[num3];
			KeyValuePair<float, Vec3> keyValuePair2 = _bridgeCurveLinearAccessCache[num2];
			float num4 = (currentLength - keyValuePair.Key) / (keyValuePair2.Key - keyValuePair.Key);
			return Vec3.Lerp(keyValuePair.Value, keyValuePair2.Value, num4);
		}

		private void SetRopeMeshParams(Mesh ropeMesh, Vec3 start, Vec3 end, float length)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			if ((NativeObject)(object)ropeMesh != (NativeObject)null)
			{
				MatrixFrame identity = MatrixFrame.Identity;
				identity.rotation.s = start;
				identity.origin = end;
				ropeMesh.SetAdditionalBoneFrame(0, ref identity);
				MatrixFrame identity2 = MatrixFrame.Identity;
				ropeMesh.SetAdditionalBoneFrame(1, ref identity2);
				Vec3 vectorArgument = ropeMesh.GetVectorArgument();
				vectorArgument.x = length;
				vectorArgument.y = 25.9f;
				vectorArgument.z = 1f;
				ropeMesh.SetVectorArgument(vectorArgument.x, vectorArgument.y, vectorArgument.z, vectorArgument.w);
			}
		}

		private static Vec3 GetPositionAtProjectileCurveProgress(in Vec3 globalVelocity, in Vec3 sourceGlobalPosition, float time, float progressInterval)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			time *= progressInterval;
			return sourceGlobalPosition + globalVelocity * time + 0.5f * MBGlobals.GravitationalAcceleration * time * time;
		}

		private void SetAbilityOfNavmeshBridgeFaces(bool enable)
		{
			Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId, enable);
			Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 1, enable);
			Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 2, enable);
			Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 3, enable);
			Mission.Current.Scene.SetAbilityOfFacesWithId(_bridgeNavmeshId + 4, enable);
		}

		private bool IsNavmeshBridgeEntityUpsideDown()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return _navMeshBridge.GetGlobalFrame().rotation.u.z <= 0.35f;
		}

		private void AddNewClipPlaneIntersectionPoint(ref int numberOfValidVertices, in Vec3 currentCorner)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (numberOfValidVertices < 5)
			{
				_registeredVerticesAfterPhysicsClipPlaneIntersection[numberOfValidVertices] = currentCorner;
				numberOfValidVertices++;
			}
		}

		private void ArrangePlankPhysicsWithClipPlanes(Vec3[] quadVerticesCCW, MatrixFrame firstClipFrame, MatrixFrame secondClipFrame)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0387: Unknown result type (might be due to invalid IL or missing references)
			//IL_038c: Unknown result type (might be due to invalid IL or missing references)
			//IL_039a: Unknown result type (might be due to invalid IL or missing references)
			//IL_039f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0304: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0337: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
			_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[0] = 0;
			_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[1] = 0;
			_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[2] = 0;
			_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[3] = 0;
			int numberOfValidVertices = 0;
			bool flag = false;
			float num3 = default(float);
			float num5 = default(float);
			for (int i = 0; i < 4; i++)
			{
				Vec3 currentCorner = quadVerticesCCW[i];
				int num = (i + 1) % 4;
				Vec3 currentCorner2 = quadVerticesCCW[num];
				Vec3 val2;
				if (MBMath.PointLiesAheadOfPlane(ref firstClipFrame.rotation.f, ref firstClipFrame.origin, ref currentCorner))
				{
					Vec3 val = currentCorner2 - currentCorner;
					float num2 = ((Vec3)(ref val)).Normalize();
					val2 = -firstClipFrame.rotation.f;
					if (MBMath.GetRayPlaneIntersectionPoint(ref val2, ref firstClipFrame.origin, ref currentCorner, ref val, ref num3) && num3 < num2)
					{
						Vec3 currentCorner3 = currentCorner + val * num3;
						if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[i] == 0)
						{
							AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner);
							_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[i] = 1;
						}
						AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner3);
						flag = true;
						continue;
					}
					if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[i] == 0)
					{
						AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner);
						_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[i] = 1;
					}
					if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num] == 0)
					{
						AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner2);
						_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num] = 1;
					}
					continue;
				}
				flag = true;
				Vec3 val3 = currentCorner - currentCorner2;
				float num4 = ((Vec3)(ref val3)).Normalize();
				val2 = -firstClipFrame.rotation.f;
				if (MBMath.GetRayPlaneIntersectionPoint(ref val2, ref firstClipFrame.origin, ref currentCorner2, ref val3, ref num5) && num5 < num4)
				{
					AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, currentCorner2 + val3 * num5);
					if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num] == 0)
					{
						AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner2);
						_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num] = 1;
					}
				}
			}
			if (!flag)
			{
				_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[0] = 0;
				_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[1] = 0;
				_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[2] = 0;
				_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[3] = 0;
				numberOfValidVertices = 0;
				float num8 = default(float);
				float num10 = default(float);
				for (int j = 0; j < 4; j++)
				{
					Vec3 currentCorner4 = quadVerticesCCW[j];
					int num6 = (j + 1) % 4;
					Vec3 currentCorner5 = quadVerticesCCW[num6];
					Vec3 val2;
					if (MBMath.PointLiesAheadOfPlane(ref secondClipFrame.rotation.f, ref secondClipFrame.origin, ref currentCorner4))
					{
						Vec3 val4 = currentCorner5 - currentCorner4;
						float num7 = ((Vec3)(ref val4)).Normalize();
						val2 = -secondClipFrame.rotation.f;
						if (MBMath.GetRayPlaneIntersectionPoint(ref val2, ref secondClipFrame.origin, ref currentCorner4, ref val4, ref num8) && num8 < num7)
						{
							Vec3 currentCorner6 = currentCorner4 + val4 * num8;
							if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[j] == 0)
							{
								AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner4);
								_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[j] = 1;
							}
							AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner6);
							continue;
						}
						if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[j] == 0)
						{
							AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner4);
							_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[j] = 1;
						}
						if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num6] == 0)
						{
							AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner5);
							_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num6] = 1;
						}
						continue;
					}
					Vec3 val5 = currentCorner4 - currentCorner5;
					float num9 = ((Vec3)(ref val5)).Normalize();
					val2 = -secondClipFrame.rotation.f;
					if (MBMath.GetRayPlaneIntersectionPoint(ref val2, ref secondClipFrame.origin, ref currentCorner5, ref val5, ref num10) && num10 < num9)
					{
						AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, currentCorner5 + val5 * num10);
						if (_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num6] == 0)
						{
							AddNewClipPlaneIntersectionPoint(ref numberOfValidVertices, in currentCorner5);
							_alreadyAddedVertexDataForPhysicsClipPlaneIntersection[num6] = 1;
						}
					}
				}
			}
			if (numberOfValidVertices < 3)
			{
				return;
			}
			bool flag2 = true;
			for (int k = 0; k < numberOfValidVertices; k++)
			{
				Vec3 val6 = _registeredVerticesAfterPhysicsClipPlaneIntersection[k];
				Vec3 val7 = _registeredVerticesAfterPhysicsClipPlaneIntersection[(k + 1) % numberOfValidVertices];
				if (((Vec3)(ref val6)).DistanceSquared(val7) < 1E-06f)
				{
					flag2 = false;
					break;
				}
			}
			if (!flag2)
			{
				return;
			}
			int num11 = 0;
			for (int l = 0; l < numberOfValidVertices; l++)
			{
				int num12 = AddNewVertexToPlankPhysics(_registeredVerticesAfterPhysicsClipPlaneIntersection[l]);
				if (num12 == -1)
				{
					return;
				}
				if (l == 0)
				{
					num11 = num12;
				}
			}
			int num13 = numberOfValidVertices - 2;
			for (int m = 0; m < num13; m++)
			{
				AddNewIndexToPlankPhysics(num11);
				AddNewIndexToPlankPhysics(num11 + m + 1);
				AddNewIndexToPlankPhysics(num11 + m + 2);
			}
		}

		private int AddNewVertexToPlankPhysics(Vec3 vertex)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (_currentFramePlankPhysicsVertices.Length > _currentFramePlankPhysicsVertexCount)
			{
				_currentFramePlankPhysicsVertices[_currentFramePlankPhysicsVertexCount] = vertex;
				int currentFramePlankPhysicsVertexCount = _currentFramePlankPhysicsVertexCount;
				_currentFramePlankPhysicsVertexCount++;
				return currentFramePlankPhysicsVertexCount;
			}
			return -1;
		}

		private void AddNewIndexToPlankPhysics(int index)
		{
			if (_currentFramePlankPhysicsIndices.Length > _currentFramePlankPhysicsIndexCount)
			{
				_currentFramePlankPhysicsIndices[_currentFramePlankPhysicsIndexCount] = index;
				_currentFramePlankPhysicsIndexCount++;
			}
		}

		private void TransformCurrentFramePlankPhysicsVerticesToPhysicsEntityLocal(Vec3 physicsEntityGlobalPosition)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < _currentFramePlankPhysicsVertices.Length; i++)
			{
				ref Vec3 reference = ref _currentFramePlankPhysicsVertices[i];
				reference -= physicsEntityGlobalPosition;
			}
		}

		private void SpawnPlankEntities()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			_bridge = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
			for (int i = _planks.Count; i < 80; i++)
			{
				string text = _shipConnectionPlankVariations[MBRandom.RandomInt(0, _shipConnectionPlankVariations.Count - 1)];
				GameEntity val = GameEntity.Instantiate(Mission.Current.Scene, text, MatrixFrame.Identity, true, "");
				_bridge.AddChild(val, false);
				_planks.Add(val);
				val.SetupAdditionalBoneBufferForMeshes(1);
			}
		}

		private void FillBridgeCurveAccessData(in Vec3 plankTargetOrigin, in Vec3 plankSourceOrigin, in float curvedLength)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			_bridgeCurveLinearAccessCache[0] = new KeyValuePair<float, Vec3>(0f, plankTargetOrigin);
			Vec3 val = plankTargetOrigin;
			float num = 1f / 15f;
			float num2 = 0f;
			for (int i = 1; i < 15; i++)
			{
				Vec3 val2 = NavalDLC.Missions.Objects.RopeSegment.CalculateAutoCurvePosition(plankTargetOrigin, plankSourceOrigin, curvedLength, (float)i * num);
				float num3 = ((Vec3)(ref val2)).Distance(val);
				num2 += num3;
				_bridgeCurveLinearAccessCache[i] = new KeyValuePair<float, Vec3>(num2, val2);
				val = val2;
			}
			_bridgeCurveLinearAccessCache[15] = new KeyValuePair<float, Vec3>(curvedLength, plankSourceOrigin);
		}

		private void ArrangePlanksMT()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0228: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0236: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0291: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Unknown result type (might be due to invalid IL or missing references)
			//IL_0298: Unknown result type (might be due to invalid IL or missing references)
			//IL_029d: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_030b: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_032c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0342: Unknown result type (might be due to invalid IL or missing references)
			//IL_0347: Unknown result type (might be due to invalid IL or missing references)
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0365: Unknown result type (might be due to invalid IL or missing references)
			//IL_036e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0370: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_0377: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0403: Unknown result type (might be due to invalid IL or missing references)
			//IL_0405: Unknown result type (might be due to invalid IL or missing references)
			//IL_040a: Unknown result type (might be due to invalid IL or missing references)
			//IL_040f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0411: Unknown result type (might be due to invalid IL or missing references)
			//IL_0416: Unknown result type (might be due to invalid IL or missing references)
			//IL_041b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0420: Unknown result type (might be due to invalid IL or missing references)
			//IL_0437: Unknown result type (might be due to invalid IL or missing references)
			//IL_043c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0476: Unknown result type (might be due to invalid IL or missing references)
			//IL_0478: Unknown result type (might be due to invalid IL or missing references)
			//IL_0485: Unknown result type (might be due to invalid IL or missing references)
			//IL_048a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0606: Unknown result type (might be due to invalid IL or missing references)
			//IL_060b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0498: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0512: Unknown result type (might be due to invalid IL or missing references)
			//IL_0517: Unknown result type (might be due to invalid IL or missing references)
			//IL_0520: Unknown result type (might be due to invalid IL or missing references)
			//IL_0522: Unknown result type (might be due to invalid IL or missing references)
			//IL_0524: Unknown result type (might be due to invalid IL or missing references)
			//IL_0529: Unknown result type (might be due to invalid IL or missing references)
			//IL_052e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0652: Unknown result type (might be due to invalid IL or missing references)
			//IL_0657: Unknown result type (might be due to invalid IL or missing references)
			//IL_067f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0684: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0704: Unknown result type (might be due to invalid IL or missing references)
			//IL_0706: Unknown result type (might be due to invalid IL or missing references)
			//IL_0712: Unknown result type (might be due to invalid IL or missing references)
			//IL_0714: Unknown result type (might be due to invalid IL or missing references)
			//IL_0720: Unknown result type (might be due to invalid IL or missing references)
			//IL_0722: Unknown result type (might be due to invalid IL or missing references)
			//IL_072e: Unknown result type (might be due to invalid IL or missing references)
			//IL_072f: Unknown result type (might be due to invalid IL or missing references)
			//IL_057a: Unknown result type (might be due to invalid IL or missing references)
			//IL_057f: Unknown result type (might be due to invalid IL or missing references)
			//IL_059d: Unknown result type (might be due to invalid IL or missing references)
			//IL_059f: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07be: Unknown result type (might be due to invalid IL or missing references)
			//IL_07c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_07da: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_080d: Unknown result type (might be due to invalid IL or missing references)
			//IL_080f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0814: Unknown result type (might be due to invalid IL or missing references)
			//IL_0819: Unknown result type (might be due to invalid IL or missing references)
			//IL_081b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0820: Unknown result type (might be due to invalid IL or missing references)
			//IL_0825: Unknown result type (might be due to invalid IL or missing references)
			//IL_082a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0848: Unknown result type (might be due to invalid IL or missing references)
			//IL_084a: Unknown result type (might be due to invalid IL or missing references)
			//IL_084f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0854: Unknown result type (might be due to invalid IL or missing references)
			//IL_0856: Unknown result type (might be due to invalid IL or missing references)
			//IL_085b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0860: Unknown result type (might be due to invalid IL or missing references)
			//IL_0865: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0898: Unknown result type (might be due to invalid IL or missing references)
			//IL_089d: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_08a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_08c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_08d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_08e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_08e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_091c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0921: Unknown result type (might be due to invalid IL or missing references)
			//IL_0949: Unknown result type (might be due to invalid IL or missing references)
			//IL_094e: Unknown result type (might be due to invalid IL or missing references)
			//IL_097e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0983: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_09b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_09c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_09d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_09dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_09de: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_09ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_09f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a23: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a28: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bd6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bdb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a46: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a4b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a4f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a51: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a56: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a58: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a5d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a68: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a77: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a83: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a85: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a98: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a9a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0a9b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aa0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ad7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b04: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b34: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b39: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b68: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b6d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b84: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b86: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b8e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b90: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b98: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b9a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ba2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ba3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c24: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c25: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c26: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c2b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c63: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c65: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c6a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c6f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c7a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c7f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c88: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c8a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c8f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c94: Unknown result type (might be due to invalid IL or missing references)
			//IL_0c9f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ca4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ce4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0ce9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cfa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0cff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d04: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d06: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d0b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d26: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d2b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d3c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d41: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d46: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d48: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d4d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0d9f: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)AttachmentSource).GameEntity;
			Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)AttachmentTarget).GameEntity;
			Vec3 localPosition = (origin + ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin) * 0.5f;
			AttachmentSource.PlankBridgePhysicsEntity.SetLocalPosition(localPosition);
			_currentFramePlankPhysicsIndexCount = 0;
			_currentFramePlankPhysicsVertexCount = 0;
			MatrixFrame globalFrame = AttachmentSource.ConnectionClipPlaneEntity.GetGlobalFrame();
			Vec3 plankSourceOrigin = globalFrame.origin;
			MatrixFrame globalFrame2 = AttachmentTarget.ConnectionClipPlaneEntity.GetGlobalFrame();
			Vec3 plankTargetOrigin = globalFrame2.origin;
			Vec3 f = plankSourceOrigin - plankTargetOrigin;
			((Vec3)(ref f)).Normalize();
			MatrixFrame identity = MatrixFrame.Identity;
			identity.rotation.f = f;
			identity.rotation.s = ((Vec3)(ref f)).CrossProductWithUp();
			((Vec3)(ref identity.rotation.s)).Normalize();
			identity.rotation.u = Vec3.CrossProduct(identity.rotation.s, identity.rotation.f);
			((Vec3)(ref identity.rotation.u)).Normalize();
			float num = ((Vec3)(ref plankSourceOrigin)).Distance(plankTargetOrigin);
			float num2 = 1.035f;
			if (_state == ShipAttachmentState.BridgeThrown)
			{
				float num3 = MathF.Sin(_bridgeFlightData.CurveLerpVelocity * MathF.PI);
				float num4 = (_bridgeFlightData.ThrowFinishValue - _bridgeFlightData.CurveLerpValue) / _bridgeFlightData.ThrowFinishValue;
				float num5 = Math.Min((_bridgeFlightData.CurveLerpValue - 0.5f) * 2f, 1f);
				num2 += num3 * num4 * num5 * 0.028f;
			}
			_previousNumberOfPlanksNeeded = _numberOfPlanksNeeded;
			float curvedLength = num * num2;
			_numberOfPlanksNeeded = MathF.Max(MathF.Ceiling(curvedLength / _plankVerticalSize), 2);
			_numberOfPlanksNeeded = Math.Min(_numberOfPlanksNeeded, 80);
			FillBridgeCurveAccessData(in plankTargetOrigin, in plankSourceOrigin, in curvedLength);
			Vec3 val = -globalFrame.rotation.s;
			MatrixFrame identity2 = MatrixFrame.Identity;
			identity2.origin = GetCurvePositionFromLength(0f);
			Vec3 curvePositionFromLength = GetCurvePositionFromLength(MathF.Min(_plankVerticalSize, curvedLength));
			identity2.rotation.f = curvePositionFromLength - identity2.origin;
			((Vec3)(ref identity2.rotation.f)).Normalize();
			Vec3 val2 = ((Vec3)(ref identity2.rotation.f)).CrossProductWithUp();
			((Vec3)(ref val2)).Normalize();
			val2 = globalFrame2.rotation.s;
			((Vec3)(ref val2)).Normalize();
			Vec3 val3 = Vec3.CrossProduct(globalFrame.rotation.f, val);
			((Vec3)(ref val3)).Normalize();
			val = Vec3.CrossProduct(val3, globalFrame.rotation.f);
			((Vec3)(ref val)).Normalize();
			float num6 = (float)Math.Acos(Vec3.DotProduct(val, val2));
			if (Vec3.DotProduct(Vec3.CrossProduct(val2, val), globalFrame.rotation.f) < 0f)
			{
				num6 *= -1f;
			}
			float num7 = num6 / (float)_numberOfPlanksNeeded;
			Vec3 s = val2;
			Vec3 val6 = default(Vec3);
			Vec3 val7 = default(Vec3);
			Vec3 val8 = default(Vec3);
			Vec3 val9 = default(Vec3);
			for (int i = 0; i < _numberOfPlanksNeeded; i++)
			{
				bool visibilityExcludeParents = true;
				GameEntity obj = _planks[i];
				MatrixFrame val4 = MatrixFrame.Identity;
				val4.origin = GetCurvePositionFromLength(MathF.Min((float)i * _plankVerticalSize, curvedLength));
				Vec3 curvePositionFromLength2 = GetCurvePositionFromLength(MathF.Min((float)(i + 1) * _plankVerticalSize, curvedLength));
				val4.rotation.f = curvePositionFromLength2 - val4.origin;
				if (((Vec3)(ref val4.rotation.f)).LengthSquared > 0f)
				{
					((Vec3)(ref val4.rotation.f)).Normalize();
				}
				else
				{
					val4.rotation.f = f;
				}
				ref Vec3 f2 = ref val4.rotation.f;
				f2 *= 1.06f;
				val4.rotation.s = s;
				((Vec3)(ref val4.rotation.s)).Normalize();
				val4.rotation.u = Vec3.CrossProduct(val4.rotation.s, val4.rotation.f);
				((Vec3)(ref val4.rotation.u)).Normalize();
				MatrixFrame identity3 = MatrixFrame.Identity;
				((Mat3)(ref identity3.rotation)).RotateAboutForward(num7);
				obj.SetBoneFrameToAllMeshes(0, ref identity3);
				obj.SetVectorArgument(1f / _plankVerticalSize, 0f, 0f, 0f);
				s = Vec3.Lerp(val2, val, (float)i / (float)_numberOfPlanksNeeded);
				if (_state == ShipAttachmentState.BridgeThrown)
				{
					MatrixFrame identity4 = MatrixFrame.Identity;
					float time = MathF.Min(_bridgeFlightData.DtSinceFlightStart, _bridgeFlightData.CurrentFrameTotalLightTime);
					int num8 = _numberOfPlanksNeeded - i - 1;
					identity4.origin = GetPositionAtProjectileCurveProgress(progressInterval: (float)num8 / (float)(_numberOfPlanksNeeded - 1), globalVelocity: in _bridgeFlightData.CurrentFrameInitialVelocity, sourceGlobalPosition: in plankSourceOrigin, time: time);
					Vec3 val5 = GetPositionAtProjectileCurveProgress(progressInterval: (float)(num8 - 1) / (float)(_numberOfPlanksNeeded - 1), globalVelocity: in _bridgeFlightData.CurrentFrameInitialVelocity, sourceGlobalPosition: in plankSourceOrigin, time: time);
					identity4.rotation.f = val5 - identity4.origin;
					if ((double)((Vec3)(ref identity4.rotation.f)).LengthSquared < 0.1)
					{
						visibilityExcludeParents = false;
					}
					else
					{
						((Vec3)(ref identity4.rotation.f)).Normalize();
						identity4.rotation.s = ((Vec3)(ref identity4.rotation.f)).CrossProductWithUp();
						((Vec3)(ref identity4.rotation.s)).Normalize();
						identity4.rotation.u = Vec3.CrossProduct(identity4.rotation.s, identity4.rotation.f);
						((Vec3)(ref identity4.rotation.u)).Normalize();
					}
					float num9 = Math.Min(_bridgeFlightData.CurveLerpValue, 1f);
					val4 = MatrixFrame.Lerp(ref identity4, ref val4, num9);
				}
				obj.SetGlobalFrame(ref val4, true);
				obj.SetVisibilityExcludeParents(visibilityExcludeParents);
				obj.SetCustomClipPlane(Vec3.Zero, Vec3.Zero, true);
				if (_state == ShipAttachmentState.BridgeConnected || _state == ShipAttachmentState.BridgeThrown)
				{
					((Vec3)(ref val6))._002Ector((0f - _plankHorizontalSize) * 0.5f, -0.2f, 0f, -1f);
					val6 = ((MatrixFrame)(ref val4)).TransformToParent(ref val6);
					((Vec3)(ref val7))._002Ector(_plankHorizontalSize * 0.5f, -0.2f, 0f, -1f);
					val7 = ((MatrixFrame)(ref val4)).TransformToParent(ref val7);
					((Vec3)(ref val8))._002Ector((0f - _plankHorizontalSize) * 0.5f, 0.2f + _plankVerticalSize, 0f, -1f);
					val8 = ((MatrixFrame)(ref val4)).TransformToParent(ref val8);
					((Vec3)(ref val9))._002Ector(_plankHorizontalSize * 0.5f, 0.2f + _plankVerticalSize, 0f, -1f);
					val9 = ((MatrixFrame)(ref val4)).TransformToParent(ref val9);
					_quadVerticesCCWCached[0] = val6;
					_quadVerticesCCWCached[1] = val7;
					_quadVerticesCCWCached[2] = val9;
					_quadVerticesCCWCached[3] = val8;
					ArrangePlankPhysicsWithClipPlanes(_quadVerticesCCWCached, globalFrame, globalFrame2);
				}
			}
			for (int j = _numberOfPlanksNeeded; j < _previousNumberOfPlanksNeeded; j++)
			{
				_planks[j].SetVisibilityExcludeParents(false);
			}
			if ((_state == ShipAttachmentState.BridgeConnected || _state == ShipAttachmentState.BridgeThrown) && _numberOfPlanksNeeded > 0)
			{
				MatrixFrame globalFrame3 = _planks[_numberOfPlanksNeeded - 1].GetGlobalFrame();
				Vec3 val10 = globalFrame3.origin + globalFrame3.rotation.f * _plankVerticalSize;
				MatrixFrame identity5 = MatrixFrame.Identity;
				identity5.rotation.u = globalFrame3.rotation.u;
				((Vec3)(ref identity5.rotation.u)).Normalize();
				identity5.rotation.s = Vec3.CrossProduct(globalFrame3.rotation.f, identity5.rotation.u);
				((Vec3)(ref identity5.rotation.s)).Normalize();
				identity5.rotation.f = Vec3.CrossProduct(identity5.rotation.u, identity5.rotation.s);
				((Vec3)(ref identity5.rotation.f)).Normalize();
				Vec3 val11 = default(Vec3);
				Vec3 val12 = default(Vec3);
				Vec3 val13 = default(Vec3);
				Vec3 val14 = default(Vec3);
				for (int k = 0; k < _sourceSafetyPlanks.Count; k++)
				{
					GameEntity obj2 = _sourceSafetyPlanks[k];
					obj2.SetVisibilityExcludeParents(false);
					MatrixFrame identity6 = MatrixFrame.Identity;
					identity6.origin = val10 + identity5.rotation.f * _plankVerticalSize * (float)k;
					identity6.rotation = identity5.rotation;
					obj2.SetGlobalFrame(ref identity6, true);
					obj2.SetCustomClipPlane(plankSourceOrigin, globalFrame.rotation.f, true);
					((Vec3)(ref val11))._002Ector((0f - _plankHorizontalSize) * 0.5f, -0.2f, 0f, -1f);
					val11 = ((MatrixFrame)(ref identity6)).TransformToParent(ref val11);
					((Vec3)(ref val12))._002Ector(_plankHorizontalSize * 0.5f, -0.2f, 0f, -1f);
					val12 = ((MatrixFrame)(ref identity6)).TransformToParent(ref val12);
					((Vec3)(ref val13))._002Ector((0f - _plankHorizontalSize) * 0.5f, 0.2f + _plankVerticalSize, 0f, -1f);
					val13 = ((MatrixFrame)(ref identity6)).TransformToParent(ref val13);
					((Vec3)(ref val14))._002Ector(_plankHorizontalSize * 0.5f, 0.2f + _plankVerticalSize, 0f, -1f);
					val14 = ((MatrixFrame)(ref identity6)).TransformToParent(ref val14);
					_quadVerticesCCWCached[0] = val11;
					_quadVerticesCCWCached[1] = val12;
					_quadVerticesCCWCached[2] = val14;
					_quadVerticesCCWCached[3] = val13;
					ArrangePlankPhysicsWithClipPlanes(_quadVerticesCCWCached, globalFrame, globalFrame2);
				}
				MatrixFrame globalFrame4 = _planks[0].GetGlobalFrame();
				Vec3 val15 = default(Vec3);
				Vec3 val16 = default(Vec3);
				Vec3 val17 = default(Vec3);
				Vec3 val18 = default(Vec3);
				for (int l = 0; l < _targetSafetyPlanks.Count; l++)
				{
					GameEntity obj3 = _targetSafetyPlanks[l];
					obj3.SetVisibilityExcludeParents(false);
					MatrixFrame identity7 = MatrixFrame.Identity;
					identity7.origin = globalFrame4.origin - globalFrame4.rotation.f * _plankVerticalSize * (float)(l + 1);
					identity7.rotation = globalFrame4.rotation;
					obj3.SetGlobalFrame(ref identity7, true);
					obj3.SetCustomClipPlane(plankTargetOrigin, globalFrame2.rotation.f, true);
					((Vec3)(ref val15))._002Ector((0f - _plankHorizontalSize) * 0.5f, -0.2f, 0f, -1f);
					val15 = ((MatrixFrame)(ref identity7)).TransformToParent(ref val15);
					((Vec3)(ref val16))._002Ector(_plankHorizontalSize * 0.5f, -0.2f, 0f, -1f);
					val16 = ((MatrixFrame)(ref identity7)).TransformToParent(ref val16);
					((Vec3)(ref val17))._002Ector((0f - _plankHorizontalSize) * 0.5f, 0.2f + _plankVerticalSize, 0f, -1f);
					val17 = ((MatrixFrame)(ref identity7)).TransformToParent(ref val17);
					((Vec3)(ref val18))._002Ector(_plankHorizontalSize * 0.5f, 0.2f + _plankVerticalSize, 0f, -1f);
					val18 = ((MatrixFrame)(ref identity7)).TransformToParent(ref val18);
					ArrangePlankPhysicsWithClipPlanes((Vec3[])(object)new Vec3[4] { val15, val16, val18, val17 }, globalFrame, globalFrame2);
				}
			}
			for (int m = 0; m < 3 && m < _planks.Count; m++)
			{
				_planks[m].SetCustomClipPlane(plankTargetOrigin, globalFrame2.rotation.f, true);
			}
			for (int n = 0; n < 3; n++)
			{
				int num10 = _numberOfPlanksNeeded - 1 - n;
				if (num10 >= 0)
				{
					_planks[num10].SetCustomClipPlane(plankSourceOrigin, globalFrame.rotation.f, true);
				}
			}
			foreach (RopeSegment rope in _ropes)
			{
				Vec3 val19 = rope.SideStartShift * identity.rotation.s * _plankHorizontalSize;
				Vec3 val20 = rope.SideEndShift * identity.rotation.s * _plankHorizontalSize;
				int startSegmentIndex = rope.StartSegmentIndex;
				int num11 = Math.Min(rope.EndSegmentIndex, _numberOfPlanksNeeded - 1);
				if (startSegmentIndex >= num11)
				{
					rope.ParentEntity.SetVisibilityExcludeParents(false);
					continue;
				}
				MatrixFrame globalFrame5 = rope.RopeStart.GetGlobalFrame();
				globalFrame5.origin = _planks[startSegmentIndex].GetGlobalFrame().origin + val19;
				rope.RopeStart.SetGlobalFrame(ref globalFrame5, true);
				MatrixFrame globalFrame6 = rope.RopeEnd.GetGlobalFrame();
				globalFrame6.origin = _planks[num11].GetGlobalFrame().origin + val20;
				rope.RopeEnd.SetGlobalFrame(ref globalFrame6, true);
				rope.ParentEntity.SetVisibilityExcludeParents(true);
			}
			if (_currentFramePlankPhysicsIndexCount > 0)
			{
				TransformCurrentFramePlankPhysicsVerticesToPhysicsEntityLocal(AttachmentSource.PlankBridgePhysicsEntity.GlobalPosition);
			}
		}

		private void ArrangePlanks()
		{
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			if (_currentFramePlankPhysicsIndexCount > 0)
			{
				GameEntityPhysicsExtensions.ReplacePhysicsBodyWithQuadPhysicsBody(AttachmentSource.PlankBridgePhysicsEntity, _currentFramePlankPhysicsVerticesPinnedPointer, _currentFramePlankPhysicsVertexCount, _woodPhysicsMaterialCached, (BodyFlags)2099220, _currentFramePlankPhysicsIndicesPinnedPointer, _currentFramePlankPhysicsIndexCount);
				BodyFlags physicsDescBodyFlag = AttachmentSource.PlankBridgePhysicsEntity.PhysicsDescBodyFlag;
				if (Extensions.HasAnyFlag<BodyFlags>(physicsDescBodyFlag, (BodyFlags)1))
				{
					AttachmentSource.PlankBridgePhysicsEntity.SetBodyFlags((BodyFlags)(physicsDescBodyFlag & -2));
				}
			}
			else
			{
				BodyFlags physicsDescBodyFlag2 = AttachmentSource.PlankBridgePhysicsEntity.PhysicsDescBodyFlag;
				if (!Extensions.HasAnyFlag<BodyFlags>(physicsDescBodyFlag2, (BodyFlags)1))
				{
					AttachmentSource.PlankBridgePhysicsEntity.SetBodyFlags((BodyFlags)(physicsDescBodyFlag2 | 1));
				}
			}
		}

		public Vec3 GetLaunchProjectileCurrentGlobalPosition(float time)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			return _launchFlightData.SourceGlobalPosition + _launchFlightData.GlobalVelocity * time + 0.5f * MBGlobals.GravitationalAcceleration * time * time;
		}

		private static (Vec3, float) CalculateInitialVelocityAndTime(Vec3 initialPosition, Vec3 destination, float verticalLaunchAngleDegree)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			float num = destination.x - initialPosition.x;
			float num2 = destination.y - initialPosition.y;
			float deltaZ = destination.z - initialPosition.z;
			float num3 = verticalLaunchAngleDegree * MathF.PI / 180f;
			float num4 = (float)Math.Sqrt(num * num + num2 * num2);
			float num5 = CalculateInitialVelocityMagnitude(num4, deltaZ, num3);
			float num6 = (float)Math.Atan2(num2, num);
			float num7 = num5 * (float)Math.Cos(num3) * (float)Math.Cos(num6);
			float num8 = num5 * (float)Math.Cos(num3) * (float)Math.Sin(num6);
			float num9 = num5 * (float)Math.Sin(num3);
			Vec3 item = new Vec3(num7, num8, num9, -1f);
			float item2 = num4 / (num5 * (float)Math.Cos(num3));
			return (item, item2);
		}

		private static float CalculateLaunchAngleDegree(Vec3 initialPosition, Vec3 targetPosition, float launchSpeed)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			Vec3 val = targetPosition - initialPosition;
			float num = launchSpeed * launchSpeed;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			float length = ((Vec2)(ref asVec)).Length;
			float z = val.z;
			float num2 = num * num;
			float num3 = 9.806f * (9.806f * length * length + 2f * z * num);
			if (num2 >= num3)
			{
				float num4 = MathF.Sqrt(num2 - num3);
				return MathF.Atan((num - num4) / (9.806f * length)) * 180f / MathF.PI;
			}
			return float.MinValue;
		}

		private static float CalculateInitialVelocityMagnitude(float distanceXY, float deltaZ, float thetaZ)
		{
			float num = (float)Math.Tan(thetaZ);
			float num2 = (float)Math.Cos(thetaZ);
			float num3 = 9.806f * distanceXY * distanceXY;
			float num4 = 2f * num2 * num2 * (distanceXY * num - deltaZ);
			return (float)Math.Sqrt(num3 / num4);
		}

		private static float CalculateDifferenceVectorAngle(in Vec3 initialPosition, in Vec3 destination)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Vec3 val = destination - initialPosition;
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			float length = ((Vec2)(ref asVec)).Length;
			return (float)Math.Atan2(val.z, length) * (180f / MathF.PI);
		}
	}

	public const float AgentOarLeaveAttachmentLengthSquared = 64f;

	public const float AgentOarLeaveRelativeSpeedThreshold = 4f;

	public const float MaximumRopeLength = 40f;

	public const float MinimumBridgeDistanceToKeep = 2.2f;

	public const float MaximumRopesPullingDuration = 30f;

	public const float BridgeConnectionRelativeSpeedThreshold = 4f;

	public const float RopesPullingFrequency = 1f;

	public const float RopesPullingRelaxSpeed = 0.05f;

	public const float RopesPullingRelaxThresholdRatio = 0.75f;

	public const float RopesPullingPullSpeed = 0.65f;

	public const float RopesPullingPullAcceleration = 0.25f;

	public const float RopesPullingWaveAmp = 0.6f;

	public const float StiffnessRampTime = 5f;

	public const float MaxDistanceError = 10f;

	public const float MaxDistanceErrorBridge = 5f;

	public const float MaxXYError = 2.75f;

	public const float MaxAlignmentError = 0.95f;

	public const float MaxAccumulatedAlignmentError = 20f;

	public const float InteractionDistance = 40f;

	public const float FatigueRate = 4f;

	public const float RopeBeta = 0.1f;

	public const float StretchLimit = 2f;

	public const float Damping = 0.1f;

	public const float RopeMaxAccelerationLowTension = 1.2f;

	public const float RopeMaxAccelerationHighTension = 5f;

	public const float BridgeDirectionDampingRatio = 0.3f;

	public const float BridgeDirectionTargetPeriod = 2f;

	public const float BridgeDirectionMaxAcceleration = 5f;

	public const float AlignmentDampingRatio = 0.8f;

	private const bool CanConnectToFriends = false;

	public const float AlignmentTargetPeriod = 1.75f;

	public const float AlignmentMaxAcceleration = 5f;

	public const float XYDampingRatio = 0.5f;

	public const float XYTargetPeriod = 0.75f;

	public const float XYMaxAcceleration = 15f;

	public const float MaxInclineAngle = MathF.PI * 13f / 36f;

	private const string HookItemID = "hook";

	private const string HookGrabSoundEvent = "event:/mission/movement/vessel/hook_grab";

	public const string ConnectionClipPointTag = "connection_point";

	public const string RampBarrierTag = "connection_barrier";

	public const string RampCapsulePhysicsTag = "step_capsule";

	public const string RampSourceVisualTag = "bridge_source";

	public const string RampTargetVisualTag = "bridge_target";

	public const string PileHangedStaticVisualTag = "pile_hanged_static";

	public const string PileFloorStaticVisualTag = "pile_floor_static";

	[EditableScriptComponentVariable(true, "")]
	public int RelatedShipNavmeshOffset;

	private MissionShip _preferredTargetShip;

	private bool _checkedInitialConnections;

	private WeakGameEntity _staticRopeVisual;

	private ItemObject _hookItem;

	private GameEntity _focusObject;

	private MatrixFrame _initialHookLocalFrame;

	private MBList<GameEntity> _rampPhysicsList;

	private bool _physicsEntitiesVisibility;

	private Vec3[] _defaultPhysicsQuad;

	private int[] _defaultIndicesCached;

	private NavalShipsLogic _navalShipsLogicCached;

	public float BridgeConnectionLengthSquared { get; private set; }

	public MissionShip OwnerShip { get; private set; }

	public ShipAttachment CurrentAttachment { get; private set; }

	public RopePileBaked RopeVisual { get; private set; }

	public ShipAttachmentPointMachine LinkedAttachmentPointMachine { get; private set; }

	public GameEntity ConnectionClipPlaneEntity { get; private set; }

	public GameEntity RampBarrier { get; private set; }

	public float RopeMinLength { get; private set; }

	internal MBReadOnlyList<GameEntity> RampPhysicsList => (MBReadOnlyList<GameEntity>)(object)_rampPhysicsList;

	internal GameEntity RampVisualEntity { get; private set; }

	public GameEntity BarrierSource { get; private set; }

	public GameEntity BarrierTarget { get; private set; }

	public GameEntity VFoldSource { get; private set; }

	public GameEntity Hook { get; private set; }

	public GameEntity VFoldTarget { get; private set; }

	public GameEntity PlankBridgePhysicsEntity { get; private set; }

	public PlankBridgeSteppedAgentManager SteppedAgentManager { get; private set; }

	public bool IsShipAttachmentJointPhysicsEnabled { get; private set; }

	public NavalShipsLogic NavalShipsLogicCached
	{
		get
		{
			if (_navalShipsLogicCached == null)
			{
				_navalShipsLogicCached = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
			}
			return _navalShipsLogicCached;
		}
	}

	public void SetShipAttachmentJointPhysicsEnabled(bool enabled)
	{
		IsShipAttachmentJointPhysicsEnabled = enabled;
	}

	public bool IsShipAttachmentMachineBridged()
	{
		if (CurrentAttachment != null)
		{
			if (CurrentAttachment.State != ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				return CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeThrown;
			}
			return true;
		}
		return false;
	}

	public bool IsShipAttachmentMachineBridgeWithEnemy()
	{
		if (CurrentAttachment != null)
		{
			Team val = CurrentAttachment?.AttachmentSource?.OwnerShip?.Team;
			Team val2 = CurrentAttachment?.AttachmentTarget?.OwnerShip?.Team;
			if (val != null && val2 != null && val.IsEnemyOf(val2))
			{
				return CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected;
			}
			return false;
		}
		return false;
	}

	public bool IsShipAttachmentMachineConnectedToEnemy()
	{
		if (CurrentAttachment != null && (CurrentAttachment.State == ShipAttachment.ShipAttachmentState.RopesPulling || CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeThrown || CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected) && CurrentAttachment.AttachmentSource.OwnerShip.Team != null && CurrentAttachment.AttachmentTarget.OwnerShip.Team != null)
		{
			return CurrentAttachment.AttachmentSource.OwnerShip.Team.IsEnemyOf(CurrentAttachment.AttachmentTarget.OwnerShip.Team);
		}
		return false;
	}

	public static bool DoesShipAttachmentMachineSatisfyOarsmenGetUpCondition(ShipAttachment currentAttachment)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		if (currentAttachment != null && (currentAttachment.State == ShipAttachment.ShipAttachmentState.RopesPulling || currentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeThrown) && currentAttachment.AttachmentSource.OwnerShip.Team != null && currentAttachment.AttachmentTarget.OwnerShip.Team != null && currentAttachment.AttachmentSource.OwnerShip.Team.IsEnemyOf(currentAttachment.AttachmentTarget.OwnerShip.Team))
		{
			MissionShip ownerShip = currentAttachment.AttachmentSource.OwnerShip;
			MissionShip ownerShip2 = currentAttachment.AttachmentTarget.OwnerShip;
			Vec3 angularVelocity = ownerShip.Physics.AngularVelocity;
			Vec3 angularVelocity2 = ownerShip2.Physics.AngularVelocity;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)ownerShip).GameEntity;
			Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin;
			gameEntity = ((ScriptComponentBehavior)ownerShip2).GameEntity;
			Vec3 origin2 = ((WeakGameEntity)(ref gameEntity)).GetBodyWorldTransform().origin;
			gameEntity = ((ScriptComponentBehavior)currentAttachment.AttachmentSource).GameEntity;
			Vec3 origin3 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
			gameEntity = ((ScriptComponentBehavior)currentAttachment.AttachmentTarget).GameEntity;
			Vec3 origin4 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
			Vec3 val = origin3 - origin;
			Vec3 val2 = ((Vec3)(ref val)).NormalizedCopy();
			val = origin4 - origin2;
			Vec3 val3 = ((Vec3)(ref val)).NormalizedCopy();
			Vec3 val4 = ownerShip.Physics.LinearVelocity + Vec3.CrossProduct(val2, angularVelocity);
			Vec3 val5 = ownerShip2.Physics.LinearVelocity + Vec3.CrossProduct(val3, angularVelocity2) - val4;
			val = origin4 - origin3;
			float lengthSquared = ((Vec3)(ref val)).LengthSquared;
			if (((Vec3)(ref val5)).LengthSquared <= 16f && lengthSquared <= 64f)
			{
				foreach (ShipOarMachine item in (List<ShipOarMachine>)(object)ownerShip.LeftSideShipOarMachines)
				{
					if (MBRandom.RandomFloat > 0.6f)
					{
						Agent pilotAgent = ((UsableMachine)item).PilotAgent;
						if (pilotAgent != null)
						{
							pilotAgent.YellAfterDelay(0.25f + MBRandom.RandomFloat);
						}
					}
				}
				foreach (ShipOarMachine item2 in (List<ShipOarMachine>)(object)ownerShip.RightSideShipOarMachines)
				{
					if (MBRandom.RandomFloat > 0.6f)
					{
						Agent pilotAgent2 = ((UsableMachine)item2).PilotAgent;
						if (pilotAgent2 != null)
						{
							pilotAgent2.YellAfterDelay(0.25f + MBRandom.RandomFloat);
						}
					}
				}
				return true;
			}
		}
		return false;
	}

	public override bool ShouldAutoLeaveDetachmentWhenDisabled(BattleSideEnum sideEnum)
	{
		return false;
	}

	public override void Disable()
	{
		if (CurrentAttachment != null)
		{
			CurrentAttachment.Destroy();
			CurrentAttachment = null;
		}
		RemoveConnectionPhysicsEntities();
		((UsableMachine)this).Disable();
	}

	public void SetConnectionPhysicsEntitiesVisibility(bool visible)
	{
		if (_physicsEntitiesVisibility != visible)
		{
			BarrierSource.SetVisibilityExcludeParents(visible);
			BarrierTarget.SetVisibilityExcludeParents(visible);
			VFoldSource.SetVisibilityExcludeParents(visible);
			VFoldTarget.SetVisibilityExcludeParents(visible);
			PlankBridgePhysicsEntity.SetVisibilityExcludeParents(visible);
			GameEntityPhysicsExtensions.SetPhysicsStateOnlyVariable(BarrierSource, visible, false);
			GameEntityPhysicsExtensions.SetPhysicsStateOnlyVariable(BarrierTarget, visible, false);
			GameEntityPhysicsExtensions.SetPhysicsStateOnlyVariable(VFoldSource, visible, false);
			GameEntityPhysicsExtensions.SetPhysicsStateOnlyVariable(VFoldTarget, visible, false);
			GameEntityPhysicsExtensions.SetPhysicsStateOnlyVariable(PlankBridgePhysicsEntity, visible, false);
			_physicsEntitiesVisibility = visible;
		}
	}

	private void RemoveConnectionPhysicsEntities()
	{
		BarrierSource.Remove(78);
		BarrierTarget.Remove(78);
		VFoldSource.Remove(78);
		VFoldTarget.Remove(78);
		PlankBridgePhysicsEntity.Remove(35);
	}

	private void InitializeConnectionPhysicsEntities()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		PhysicsMaterial.GetFromName("wood_nonstick");
		_defaultPhysicsQuad = (Vec3[])(object)new Vec3[4];
		_defaultPhysicsQuad[0] = new Vec3(-0.5f, -0.5f, 0f, -1f);
		_defaultPhysicsQuad[1] = new Vec3(0.5f, -0.5f, 0f, -1f);
		_defaultPhysicsQuad[2] = new Vec3(0.5f, 0.5f, 0f, -1f);
		_defaultPhysicsQuad[3] = new Vec3(-0.5f, 0.5f, 0f, -1f);
		_defaultIndicesCached = new int[6];
		_defaultIndicesCached[0] = 0;
		_defaultIndicesCached[1] = 1;
		_defaultIndicesCached[2] = 2;
		_defaultIndicesCached[3] = 0;
		_defaultIndicesCached[4] = 2;
		_defaultIndicesCached[5] = 3;
		BarrierSource = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
		BarrierSource.Name = "Bridge_barrier_source";
		BarrierTarget = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
		BarrierTarget.Name = "Bridge_barrier_target";
		VFoldSource = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
		VFoldSource.Name = "Bridge_vFold_source";
		VFoldTarget = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
		VFoldTarget.Name = "Bridge_vFold_target";
		PlankBridgePhysicsEntity = GameEntity.CreateEmpty(Mission.Current.Scene, false, true, true);
		GameEntity plankBridgePhysicsEntity = PlankBridgePhysicsEntity;
		MatrixFrame identity = MatrixFrame.Identity;
		plankBridgePhysicsEntity.SetGlobalFrame(ref identity, true);
		PlankBridgePhysicsEntity.Name = "Plank Bridge Physics";
		PlankBridgePhysicsEntity.CreateAndAddScriptComponent("PlankBridgeSteppedAgentManager", true);
		SteppedAgentManager = PlankBridgePhysicsEntity.GetFirstScriptOfType<PlankBridgeSteppedAgentManager>();
		SetConnectionPhysicsEntitiesVisibility(visible: false);
	}

	public bool CheckAttachmentMachineFlags(bool editMode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		IEnumerable<WeakGameEntity> children = ((WeakGameEntity)(ref val)).GetChildren();
		string[] source = new string[3] { "hook", "pilot", "pile" };
		foreach (WeakGameEntity item in children)
		{
			WeakGameEntity current = item;
			if (!Extensions.HasAnyFlag<EntityFlags>(((WeakGameEntity)(ref current)).EntityFlags, (EntityFlags)131072) && source.Contains(((WeakGameEntity)(ref current)).Name) && !Extensions.HasAnyFlag<EntityFlags>(((WeakGameEntity)(ref current)).EntityFlags, (EntityFlags)4096))
			{
				val = ((ScriptComponentBehavior)this).GameEntity;
				val = ((WeakGameEntity)(ref val)).Root;
				string name = ((WeakGameEntity)(ref val)).Name;
				val = ((ScriptComponentBehavior)this).GameEntity;
				string text = $"Root Entity: {name} {((WeakGameEntity)(ref val)).Name}'s child {((WeakGameEntity)(ref current)).Name} must have Does not Affect Parent's Local Bounding Box flag.";
				if (editMode)
				{
					MBEditor.AddEntityWarning(current, text);
				}
				return false;
			}
		}
		return true;
	}

	protected override void OnRemoved(int removeReason)
	{
		_navalShipsLogicCached = null;
	}

	protected override void OnInit()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		((UsableMachine)this).OnInit();
		IsShipAttachmentJointPhysicsEnabled = true;
		BridgeConnectionLengthSquared = 20.25f;
		InitializeConnectionPhysicsEntities();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity parent = ((WeakGameEntity)(ref val)).Parent;
		while (OwnerShip == null && ((WeakGameEntity)(ref parent)).IsValid)
		{
			OwnerShip = ((WeakGameEntity)(ref parent)).GetFirstScriptOfType<MissionShip>();
			parent = ((WeakGameEntity)(ref parent)).Parent;
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Parent;
		if (((WeakGameEntity)(ref val)).GetScriptCountOfTypeRecursive<ShipAttachmentPointMachine>() == 1)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			val = ((WeakGameEntity)(ref val)).Parent;
			LinkedAttachmentPointMachine = ((WeakGameEntity)(ref val)).GetFirstScriptOfTypeRecursive<ShipAttachmentPointMachine>();
		}
		val = ((ScriptComponentBehavior)this).GameEntity;
		int childCount = ((WeakGameEntity)(ref val)).ChildCount;
		for (int i = 0; i < childCount; i++)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			WeakGameEntity child = ((WeakGameEntity)(ref val)).GetChild(i);
			if (((WeakGameEntity)(ref child)).Name == "hook")
			{
				Hook = GameEntity.CreateFromWeakEntity(child);
				val = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
				MatrixFrame globalFrame2 = ((WeakGameEntity)(ref child)).GetGlobalFrame();
				_initialHookLocalFrame = ((MatrixFrame)(ref globalFrame)).TransformToLocalNonOrthogonal(ref globalFrame2);
			}
			else if (((WeakGameEntity)(ref child)).Name == "focus_object")
			{
				_focusObject = GameEntity.CreateFromWeakEntity(child);
			}
		}
		_hookItem = Game.Current.ObjectManager.GetObject<ItemObject>("hook");
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		RopeVisual = MBExtensions.GetFirstScriptInFamilyDescending<RopePileBaked>(((ScriptComponentBehavior)this).GameEntity);
		val = ((ScriptComponentBehavior)this).GameEntity;
		_staticRopeVisual = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("pile_hanged_static");
		if (_staticRopeVisual == (GameEntity)null)
		{
			val = ((ScriptComponentBehavior)this).GameEntity;
			_staticRopeVisual = ((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("pile_floor_static");
		}
		base.EnemyRangeToStopUsing = 5f;
		val = ((ScriptComponentBehavior)LinkedAttachmentPointMachine).GameEntity;
		RampBarrier = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTag("connection_barrier"));
		val = ((ScriptComponentBehavior)LinkedAttachmentPointMachine).GameEntity;
		ConnectionClipPlaneEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("connection_point"));
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		val = ((ScriptComponentBehavior)LinkedAttachmentPointMachine).GameEntity;
		((WeakGameEntity)(ref val)).GetChildrenWithTagRecursive(list, "step_capsule");
		_rampPhysicsList = new MBList<GameEntity>();
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			if (((WeakGameEntity)(ref current)).GetVisibilityExcludeParents())
			{
				((List<GameEntity>)(object)_rampPhysicsList).Add(GameEntity.CreateFromWeakEntity(current));
			}
		}
		val = ((ScriptComponentBehavior)LinkedAttachmentPointMachine).GameEntity;
		RampVisualEntity = GameEntity.CreateFromWeakEntity(((WeakGameEntity)(ref val)).GetFirstChildEntityWithTagRecursive("bridge_source"));
		RampVisualEntity.SetVisibilityExcludeParents(false);
		base.IsDisabledForAttackerAIDueToEnemyInRange = new QueryData<bool>((Func<bool>)(() => OwnerShip?.ShipOrder != null && OwnerShip.ShipOrder.IsEnemyOnShip), 1f);
		base.IsDisabledForDefenderAIDueToEnemyInRange = new QueryData<bool>((Func<bool>)(() => OwnerShip?.ShipOrder != null && OwnerShip.ShipOrder.IsEnemyOnShip), 1f);
	}

	public void CheckCurrentAttachmentAndInitializeRopeBoundingBox()
	{
		if (CurrentAttachment == null)
		{
			RopeVisual.SetRopeBoundingBoxToInitialState();
		}
	}

	protected override float GetDetachmentWeightAux(BattleSideEnum side)
	{
		return float.MinValue;
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(0x16 | ((UsableMachine)this).GetTickRequirement());
	}

	public void SetPreferredTargetShip(MissionShip newTarget)
	{
		_preferredTargetShip = newTarget;
	}

	public MissionShip GetPreferredTargetShip()
	{
		return _preferredTargetShip;
	}

	public bool CalculateCanConnectToTargetShip(MissionShip targetShip)
	{
		if ((targetShip != null && targetShip.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sinking) || (targetShip != null && targetShip.Physics.NavalSinkingState == NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Sunk))
		{
			return false;
		}
		foreach (ShipAttachmentPointMachine item in (List<ShipAttachmentPointMachine>)(object)targetShip.AttachmentPointMachines)
		{
			if (item.CurrentAttachment == null && ComputePotentialAttachmentValue(this, item, checkInteractionDistance: false, checkConnectionBlock: false, allowWiderAngleBetweenConnections: true) > 0f)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOnCorrectSide(MissionShip targetShip)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)OwnerShip).GameEntity;
		MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		gameEntity = ((ScriptComponentBehavior)targetShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec3 val = ((MatrixFrame)(ref frame)).TransformToLocal(ref globalPosition);
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)OwnerShip).GameEntity;
		frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec3 val2 = ((MatrixFrame)(ref frame)).TransformToLocal(ref val);
		return ((Vec2)(ref asVec)).DotProduct(((Vec3)(ref val2)).AsVec2) >= 0f;
	}

	public void SetCanConnectToFriends(bool canConnectToFriends)
	{
		_checkedInitialConnections = false;
	}

	public bool HasCheckedInitialConnections()
	{
		return _checkedInitialConnections;
	}

	public void ConnectWithAttachmentPointMachine(ShipAttachmentPointMachine attachmentPointMachine, bool forceBridge = false, bool unbreakableBridge = false, bool connectionInitializedByPlayer = false)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val;
		Vec3 val2;
		WeakGameEntity gameEntity;
		if (((UsableMachine)this).PilotAgent != null)
		{
			MatrixFrame frame = ((UsableMachine)this).PilotAgent.Frame;
			val = ((UsableMachine)this).PilotAgent.GetBoneEntitialFrame(((UsableMachine)this).PilotAgent.Monster.MainHandItemBoneIndex, false);
			val2 = ((MatrixFrame)(ref frame)).TransformToParent(ref val.origin);
		}
		else
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			val2 = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		}
		Vec3 val3 = val2;
		Vec3 val4 = val2;
		gameEntity = ((ScriptComponentBehavior)RopeVisual).GameEntity;
		Vec3 globalPosition = val4 - ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		globalPosition = val3 + ((Vec3)(ref globalPosition)).NormalizedCopy() * 0.5f;
		Agent pilotAgent = ((UsableMachine)this).PilotAgent;
		ShipAttachment shipAttachment = (CurrentAttachment = new ShipAttachment(this, attachmentPointMachine, in globalPosition, (pilotAgent != null) ? pilotAgent.LookDirection : Vec3.Zero, bridgeConnectionInteractionDistanceCheck: false, connectionInitializedByPlayer));
		attachmentPointMachine?.AssignConnection(shipAttachment);
		if (forceBridge)
		{
			gameEntity = ((ScriptComponentBehavior)RopeVisual).GameEntity;
			Vec3 globalPosition2 = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)attachmentPointMachine).GameEntity;
			val = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			globalPosition = attachmentPointMachine.HookAttachLocalPosition;
			Vec3 attachmentTargetGlobalPosition = ((MatrixFrame)(ref val)).TransformToParent(ref globalPosition);
			shipAttachment.InitializeShipAttachmentJoint(globalPosition2, attachmentTargetGlobalPosition, unbreakableBridge);
			shipAttachment.CheckAndConnectBridge(forceBridge: true);
		}
	}

	public ShipAttachmentPointMachine GetBestEnemyAttachment(bool checkAttachmentAlreadyExists = false, bool checkInteractionDistance = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		ShipAttachmentPointMachine shipAttachmentPointMachine = null;
		float num = 0f;
		Vec3 origin = OwnerShip.GlobalFrame.origin;
		MatrixFrame globalFrame;
		if (_preferredTargetShip != null)
		{
			globalFrame = _preferredTargetShip.GlobalFrame;
			if (((Vec3)(ref globalFrame.origin)).DistanceSquared(origin) <= 14400f)
			{
				if (!_preferredTargetShip.IsConnectionBlocked())
				{
					foreach (ShipAttachmentPointMachine item in (List<ShipAttachmentPointMachine>)(object)_preferredTargetShip.AttachmentPointMachines)
					{
						if (item.CurrentAttachment == null && item.LinkedAttachmentMachine?.CurrentAttachment == null)
						{
							float num2 = ComputePotentialAttachmentValue(this, item, checkInteractionDistance, checkConnectionBlock: false, allowWiderAngleBetweenConnections: true);
							if (num2 > num && (!checkAttachmentAlreadyExists || item.CurrentAttachment == null))
							{
								num = num2;
								shipAttachmentPointMachine = item;
							}
						}
					}
				}
				if (shipAttachmentPointMachine == null)
				{
					foreach (MissionShip item2 in (List<MissionShip>)(object)OwnerShip.ShipsLogic.AllShips)
					{
						if (item2 == OwnerShip || item2 == _preferredTargetShip || !MissionShip.AreShipsConnected(item2, _preferredTargetShip))
						{
							continue;
						}
						globalFrame = item2.GlobalFrame;
						if (!(((Vec3)(ref globalFrame.origin)).DistanceSquared(origin) <= 14400f) || item2.IsConnectionBlocked() || OwnerShip.SearchShipConnection(item2, isDirect: false, findEnemy: false, enforceActive: false, acceptNotBridgedConnections: false))
						{
							continue;
						}
						foreach (ShipAttachmentPointMachine item3 in (List<ShipAttachmentPointMachine>)(object)item2.AttachmentPointMachines)
						{
							if (item3.CurrentAttachment == null && item3.LinkedAttachmentMachine?.CurrentAttachment == null)
							{
								float num3 = ComputePotentialAttachmentValue(this, item3, checkInteractionDistance: true, checkConnectionBlock: false, allowWiderAngleBetweenConnections: true);
								if (num3 > num && (!checkAttachmentAlreadyExists || item3.CurrentAttachment == null))
								{
									num = num3;
									shipAttachmentPointMachine = item3;
								}
							}
						}
					}
				}
			}
		}
		else
		{
			foreach (MissionShip item4 in (List<MissionShip>)(object)OwnerShip.ShipsLogic.AllShips)
			{
				if (item4 == OwnerShip)
				{
					continue;
				}
				globalFrame = item4.GlobalFrame;
				if (((Vec3)(ref globalFrame.origin)).DistanceSquared(origin) > 14400f || item4.IsConnectionBlocked() || OwnerShip.SearchShipConnection(item4, isDirect: false, findEnemy: false, enforceActive: false, acceptNotBridgedConnections: false))
				{
					continue;
				}
				foreach (ShipAttachmentPointMachine item5 in (List<ShipAttachmentPointMachine>)(object)item4.AttachmentPointMachines)
				{
					if (item5.CurrentAttachment == null && item5.LinkedAttachmentMachine?.CurrentAttachment == null && ((((UsableMachine)this).PilotAgent != null && !((UsableMachine)this).PilotAgent.IsAIControlled) || item4 == _preferredTargetShip || (_preferredTargetShip == null && item4.BattleSide != OwnerShip.BattleSide) || (_preferredTargetShip != null && _preferredTargetShip.ShipIslandCombinedID == item4.ShipIslandCombinedID)))
					{
						float num4 = ComputePotentialAttachmentValue(this, item5, checkInteractionDistance: true, checkConnectionBlock: false, allowWiderAngleBetweenConnections: true);
						if (num4 > num && (!checkAttachmentAlreadyExists || item5.CurrentAttachment == null))
						{
							num = num4;
							shipAttachmentPointMachine = item5;
						}
					}
				}
			}
		}
		return shipAttachmentPointMachine;
	}

	public override void OnDeploymentFinished()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none, false));
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new RemoveExtraWeaponOnStopUsageComponent());
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).LockUserFrames = false;
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).LockUserPositions = true;
	}

	protected override void OnTickParallel(float dt)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current != null)
		{
			if (CurrentAttachment != null && CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				CurrentAttachment.OnParallelTick(dt);
			}
			if (CurrentAttachment == null && ((UsableMachine)this).PilotAgent == null)
			{
				RopePileBaked ropeVisual = RopeVisual;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)RopeVisual).GameEntity;
				Vec3 sourceGlobalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				gameEntity = ((ScriptComponentBehavior)RopeVisual).GameEntity;
				ropeVisual.UpdateRopeMeshVisualAccordingToTargetPointLinearWithoutBoundingBoxUpdate(in sourceGlobalPosition, ((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			}
		}
	}

	protected override void OnTick(float dt)
	{
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0671: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		if (Mission.Current == null || OwnerShip == null)
		{
			return;
		}
		if (!Mission.Current.MissionEnded)
		{
			bool flag = LinkedAttachmentPointMachine?.CurrentAttachment != null || (((UsableMachine)this).PilotAgent == null && CurrentAttachment != null && (CurrentAttachment.State != ShipAttachment.ShipAttachmentState.BridgeConnected || OwnerShip.IsDisconnectionBlocked()));
			((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).SetIsDeactivatedSynched(flag);
			((UsableMachine)this).PilotStandingPoint.AutoSheathWeapons = CurrentAttachment != null && CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected;
			if (_focusObject.GetVisibilityExcludeParents() == flag)
			{
				_focusObject.SetVisibilityExcludeParents(!flag);
			}
		}
		WeakGameEntity gameEntity;
		if (((UsableMachine)this).PilotAgent != null)
		{
			if (CurrentAttachment == null)
			{
				((UsableMachine)this).PilotAgent.AgentVisuals.SetAttachedPositionForMeshAfterAnimationPostIntegrate(((ScriptComponentBehavior)RopeVisual).GameEntity, ((UsableMachine)this).PilotAgent.Monster.MainHandItemBoneIndex);
				if (((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_usage_hook_ready, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
				{
					gameEntity = ((ScriptComponentBehavior)RopeVisual).GameEntity;
					((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
					Hook.SetVisibilityExcludeParents(false);
					((WeakGameEntity)(ref _staticRopeVisual)).SetVisibilityExcludeParents(false);
					MissionWeapon wieldedWeapon = ((UsableMachine)this).PilotAgent.WieldedWeapon;
					if (((MissionWeapon)(ref wieldedWeapon)).Item != _hookItem)
					{
						Vec3 position = ((UsableMachine)this).PilotAgent.Position;
						SoundManager.StartOneShotEvent("event:/mission/movement/vessel/hook_grab", ref position);
						MissionWeapon val = default(MissionWeapon);
						((MissionWeapon)(ref val))._002Ector(_hookItem, (ItemModifier)null, (Banner)null);
						((UsableMachine)this).PilotAgent.EquipWeaponToExtraSlotAndWield(ref val);
					}
					if (((UsableMachine)this).PilotAgent.IsAIControlled)
					{
						if (GetBestEnemyAttachment() != null && !((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_usage_hook_release, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
						{
							((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
						}
					}
					else if (((UsableMachine)this).PilotAgent.Mission.InputManager.IsGameKeyReleased(9))
					{
						gameEntity = ((ScriptComponentBehavior)this).GameEntity;
						if (Vec3.DotProduct(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.f, ((UsableMachine)this).PilotAgent.LookRotation.f) >= 0f && !((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_usage_hook_release, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
						{
							((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
						}
					}
					_checkedInitialConnections = true;
				}
				else if (((UsableMachine)this).PilotAgent.GetCurrentAction(1) == ActionIndexCache.act_usage_hook_release)
				{
					if (((UsableMachine)this).PilotAgent.IsAIControlled)
					{
						ShipAttachmentPointMachine bestEnemyAttachment = GetBestEnemyAttachment();
						if (bestEnemyAttachment == null)
						{
							if (!((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_none, false, (AnimFlags)12, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true) || !((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_usage_hook_ready, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
							{
								((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
							}
						}
						else if (((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1) > MBAnimation.GetAnimationParameter1("usage_hook_release"))
						{
							ConnectWithAttachmentPointMachine(bestEnemyAttachment);
							((UsableMachine)this).PilotAgent.RemoveEquippedWeapon((EquipmentIndex)4);
							Hook.SetVisibilityExcludeParents(true);
						}
					}
					else if (((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1) > MBAnimation.GetAnimationParameter1("usage_hook_release"))
					{
						ConnectWithAttachmentPointMachine(null, forceBridge: false, unbreakableBridge: false, connectionInitializedByPlayer: true);
						((UsableMachine)this).PilotAgent.RemoveEquippedWeapon((EquipmentIndex)4);
						Hook.SetVisibilityExcludeParents(true);
					}
				}
				else if (!((UsableMachine)this).PilotAgent.IsInBeingStruckAction)
				{
					((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
			}
			else if (((UsableMachine)this).PilotAgent.GetCurrentAction(1) == ActionIndexCache.act_usage_hook_release)
			{
				if (((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1) > 0.99f)
				{
					((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
			}
			else if (CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				if (((UsableMachine)this).PilotAgent.SetActionChannel(1, ref ActionIndexCache.act_ship_connection_break, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
				{
					if (((UsableMachine)this).PilotAgent == Agent.Main && ((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1) < 0.1f)
					{
						MissionShip ownerShip = OwnerShip;
						if (ownerShip != null)
						{
							Team team = ownerShip.Team;
							if (((team != null) ? new bool?(team.IsPlayerTeam) : ((bool?)null)) == true)
							{
								OwnerShip?.ShipOrder?.SetCutLoose(enable: true);
							}
						}
					}
					if (((UsableMachine)this).PilotAgent.GetCurrentActionProgress(1) > 0.99f)
					{
						DisconnectAttachment();
						((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
					}
				}
				else
				{
					((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
			}
		}
		else if (CurrentAttachment == null)
		{
			gameEntity = ((ScriptComponentBehavior)RopeVisual).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
			Hook.SetVisibilityExcludeParents(true);
			((WeakGameEntity)(ref _staticRopeVisual)).SetVisibilityExcludeParents(true);
		}
		if (CurrentAttachment != null)
		{
			bool num = CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected;
			CurrentAttachment.OnTick(dt);
			if (!num && CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected)
			{
				CurrentAttachment.AttachmentSource.OwnerShip.OnShipConnected(CurrentAttachment);
				CurrentAttachment.AttachmentTarget.OwnerShip.OnShipConnected(CurrentAttachment);
			}
			if (CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval)
			{
				CurrentAttachment.Destroy();
				CheckCurrentAttachmentAndInitializeRopeBoundingBox();
			}
		}
		MatrixFrame val2;
		if (Hook.GetVisibilityExcludeParents())
		{
			if (CurrentAttachment != null && (CurrentAttachment.State == ShipAttachment.ShipAttachmentState.RopeThrown || CurrentAttachment.State == ShipAttachment.ShipAttachmentState.RopesPulling || CurrentAttachment.State == ShipAttachment.ShipAttachmentState.BridgeConnected || CurrentAttachment.State == ShipAttachment.ShipAttachmentState.RopeFailedAndReloading))
			{
				GameEntity hook = Hook;
				val2 = CurrentAttachment.HookGlobalFrame;
				hook.SetGlobalFrame(ref val2, true);
			}
			else
			{
				GameEntity hook2 = Hook;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				val2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				val2 = ((MatrixFrame)(ref val2)).TransformToParent(ref _initialHookLocalFrame);
				hook2.SetGlobalFrame(ref val2, true);
			}
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (Extensions.HasAllFlags<BodyFlags>(((WeakGameEntity)(ref gameEntity)).BodyFlag, (BodyFlags)1073741824))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float num2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin.z + ((UsableMachine)this).SinkingReferenceOffset;
			Scene scene = ((ScriptComponentBehavior)this).Scene;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			val2 = ((WeakGameEntity)(ref gameEntity)).GetFrame();
			if (num2 < scene.GetWaterLevelAtPosition(((Vec3)(ref val2.origin)).AsVec2, true, false))
			{
				((UsableMachine)this).Disable();
			}
		}
	}

	public void DisconnectAttachment()
	{
		CurrentAttachment.SetAttachmentState(ShipAttachment.ShipAttachmentState.BrokenAndWaitingForRemoval);
		CurrentAttachment.AttachmentSource.OwnerShip.OnShipDisconnected(CurrentAttachment);
		CurrentAttachment.AttachmentTarget.OwnerShip.OnShipDisconnected(CurrentAttachment);
	}

	private static bool CheckIntersectionsBetweenConnectionsAux(Vec2 attachmentMachineSourcePosition, Vec2 attachmentMachineTargetPosition, ShipAttachment testAttachment)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)testAttachment.AttachmentSource).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)testAttachment.AttachmentTarget).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		return MBMath.CheckLineSegmentToLineSegmentIntersection(attachmentMachineSourcePosition, attachmentMachineTargetPosition, asVec, ((Vec3)(ref globalPosition)).AsVec2);
	}

	private static bool CheckIntersectionsBetweenConnectionsWithState(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine, ShipAttachment.ShipAttachmentState state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)attachmentMachine).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)attachmentPointMachine).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec2 = ((Vec3)(ref globalPosition)).AsVec2;
		MissionShip ownerShip = attachmentMachine.OwnerShip;
		MissionShip ownerShip2 = attachmentPointMachine.OwnerShip;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ownerShip.AttachmentMachines)
		{
			if (item != attachmentMachine && item.CurrentAttachment != null && item.CurrentAttachment.State == state && item.CurrentAttachment.AttachmentTarget != null && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item.CurrentAttachment))
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)ownerShip.AttachmentPointMachines)
		{
			if (item2.CurrentAttachment != null && item2.CurrentAttachment.State == state && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item2.CurrentAttachment))
			{
				return true;
			}
		}
		foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)ownerShip2.AttachmentMachines)
		{
			if (item3.CurrentAttachment != null && item3.CurrentAttachment.State == state && item3.CurrentAttachment.AttachmentTarget != null && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item3.CurrentAttachment))
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item4 in (List<ShipAttachmentPointMachine>)(object)ownerShip2.AttachmentPointMachines)
		{
			if (item4 != attachmentPointMachine && item4.CurrentAttachment != null && item4.CurrentAttachment.State == state && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item4.CurrentAttachment))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckAttachmentsFacingEachOther(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)attachmentMachine).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		gameEntity = ((ScriptComponentBehavior)attachmentPointMachine).GameEntity;
		MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		Vec2 asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		Vec2 asVec2 = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
		Vec2 val = ((Vec3)(ref globalFrame2.origin)).AsVec2 - ((Vec3)(ref globalFrame.origin)).AsVec2;
		if (Vec2.DotProduct(asVec, asVec2) < 0f)
		{
			return Vec2.DotProduct(val, asVec2) < 0f;
		}
		return false;
	}

	private static bool CheckIntersectionsBetweenConnections(ShipAttachmentMachine attachmentMachine, ShipAttachmentPointMachine attachmentPointMachine)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)attachmentMachine).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)attachmentPointMachine).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec2 = ((Vec3)(ref globalPosition)).AsVec2;
		MissionShip ownerShip = attachmentMachine.OwnerShip;
		MissionShip ownerShip2 = attachmentPointMachine.OwnerShip;
		foreach (ShipAttachmentMachine item in (List<ShipAttachmentMachine>)(object)ownerShip.AttachmentMachines)
		{
			if (item != attachmentMachine && item.CurrentAttachment != null && item.CurrentAttachment.AttachmentTarget != null && item.CurrentAttachment.AttachmentTarget != attachmentPointMachine && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item.CurrentAttachment))
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item2 in (List<ShipAttachmentPointMachine>)(object)ownerShip.AttachmentPointMachines)
		{
			if (item2.CurrentAttachment != null && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item2.CurrentAttachment))
			{
				return true;
			}
		}
		foreach (ShipAttachmentMachine item3 in (List<ShipAttachmentMachine>)(object)ownerShip2.AttachmentMachines)
		{
			if (item3.CurrentAttachment != null && item3.CurrentAttachment.AttachmentTarget != null && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item3.CurrentAttachment))
			{
				return true;
			}
		}
		foreach (ShipAttachmentPointMachine item4 in (List<ShipAttachmentPointMachine>)(object)ownerShip2.AttachmentPointMachines)
		{
			if (item4 != attachmentPointMachine && item4.CurrentAttachment != null && item4.CurrentAttachment.AttachmentSource != attachmentMachine && CheckIntersectionsBetweenConnectionsAux(asVec, asVec2, item4.CurrentAttachment))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsShipNearAttachmentMachines(MissionShip ship, MatrixFrame shipFrame, Vec2 sourceGlobalPos, Vec2 targetGlobalPos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		float radius = ship.Physics.PhysicsBoundingBoxWithoutChildren.radius;
		Vec3 center = ship.Physics.PhysicsBoundingBoxWithoutChildren.center;
		Vec3 val = ((MatrixFrame)(ref shipFrame)).TransformToParent(ref center);
		Vec2 asVec = ((Vec3)(ref val)).AsVec2;
		Vec2 val2 = (sourceGlobalPos + targetGlobalPos) * 0.5f;
		float num = ((Vec2)(ref val2)).Distance(sourceGlobalPos) + radius;
		return ((Vec2)(ref asVec)).DistanceSquared(val2) <= num * num;
	}

	public static bool IsShipBetweenAttachments(ShipAttachmentMachine attachmentMachineSource, ShipAttachmentPointMachine attachmentMachineTarget)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)attachmentMachineSource).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)attachmentMachineTarget).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec2 = ((Vec3)(ref globalPosition)).AsVec2;
		foreach (MissionShip item in (List<MissionShip>)(object)attachmentMachineSource.NavalShipsLogicCached.AllShips)
		{
			if (item != attachmentMachineSource.OwnerShip && item != attachmentMachineTarget.OwnerShip)
			{
				gameEntity = ((ScriptComponentBehavior)item).GameEntity;
				MatrixFrame shipFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
				Vec2[] physicsBoundingBoxPointsOfShip = item.CalculateBoundingXYGlobalPlaneFromLocal(in shipFrame);
				if (EarlyCrossCheckForShipIntersectingAttachmentMachine(physicsBoundingBoxPointsOfShip, asVec, asVec2) && IsShipNearAttachmentMachines(item, shipFrame, asVec, asVec2) && IsLineSegmentIntersectingShipBoundingXYPlane(physicsBoundingBoxPointsOfShip, asVec, asVec2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool EarlyCrossCheckForShipIntersectingAttachmentMachine(Vec2[] physicsBoundingBoxPointsOfShip, Vec2 attachmentSourceGlobalPosition, Vec2 attachmentTargetGlobalPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = attachmentSourceGlobalPosition - attachmentTargetGlobalPosition;
		float num = Vec2.CCW(physicsBoundingBoxPointsOfShip[0] - attachmentTargetGlobalPosition, val);
		for (int i = 1; i < physicsBoundingBoxPointsOfShip.Length; i++)
		{
			float num2 = Vec2.CCW(physicsBoundingBoxPointsOfShip[i] - attachmentTargetGlobalPosition, val);
			if (num * num2 <= 0f)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsLineSegmentIntersectingShipBoundingXYPlane(Vec2[] physicsBoundingBoxPointsOfShip, Vec2 attachment0Position, Vec2 attachment1Position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (MBMath.CheckLineSegmentToLineSegmentIntersection(physicsBoundingBoxPointsOfShip[0], physicsBoundingBoxPointsOfShip[1], attachment0Position, attachment1Position))
		{
			return true;
		}
		if (MBMath.CheckLineSegmentToLineSegmentIntersection(physicsBoundingBoxPointsOfShip[1], physicsBoundingBoxPointsOfShip[2], attachment0Position, attachment1Position))
		{
			return true;
		}
		if (MBMath.CheckLineSegmentToLineSegmentIntersection(physicsBoundingBoxPointsOfShip[2], physicsBoundingBoxPointsOfShip[3], attachment0Position, attachment1Position))
		{
			return true;
		}
		if (MBMath.CheckLineSegmentToLineSegmentIntersection(physicsBoundingBoxPointsOfShip[3], physicsBoundingBoxPointsOfShip[0], attachment0Position, attachment1Position))
		{
			return true;
		}
		if (MBMath.CheckPointInsidePolygon(ref physicsBoundingBoxPointsOfShip[0], ref physicsBoundingBoxPointsOfShip[1], ref physicsBoundingBoxPointsOfShip[2], ref physicsBoundingBoxPointsOfShip[3], ref attachment0Position) || MBMath.CheckPointInsidePolygon(ref physicsBoundingBoxPointsOfShip[0], ref physicsBoundingBoxPointsOfShip[1], ref physicsBoundingBoxPointsOfShip[2], ref physicsBoundingBoxPointsOfShip[3], ref attachment1Position))
		{
			return true;
		}
		return false;
	}

	public static float ComputePotentialAttachmentValue(ShipAttachmentMachine attachmentSource, ShipAttachmentPointMachine attachmentTarget, bool checkInteractionDistance, bool checkConnectionBlock, bool allowWiderAngleBetweenConnections)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		if (!checkConnectionBlock || !attachmentSource.OwnerShip.IsConnectionBlocked())
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)attachmentSource).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			Vec3 val = ((Vec3)(ref globalFrame.rotation.f)).NormalizedCopy();
			gameEntity = ((ScriptComponentBehavior)attachmentTarget).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			Vec3 val2 = globalFrame2.origin - globalFrame.origin;
			float num = ((Vec3)(ref val2)).Normalize();
			if (!checkInteractionDistance || num <= 40f)
			{
				float num2 = Vec3.DotProduct(val2, val);
				if (num2 > (allowWiderAngleBetweenConnections ? 0.1736f : 0.4226f))
				{
					if (IsShipBetweenAttachments(attachmentSource, attachmentTarget))
					{
						return -1f;
					}
					if (CheckIntersectionsBetweenConnections(attachmentSource, attachmentTarget))
					{
						return -1f;
					}
					if (!CheckAttachmentsFacingEachOther(attachmentSource, attachmentTarget))
					{
						return -1f;
					}
					Vec3 val3 = ((Vec3)(ref globalFrame2.rotation.f)).NormalizedCopy();
					float num3 = Vec3.DotProduct(-val2, val3);
					if (num3 > 0.1736f)
					{
						return 10000f * num2 * num3 / num;
					}
				}
			}
		}
		return -1f;
	}

	protected override void OnFixedTick(float fixedDt)
	{
		if (CurrentAttachment != null)
		{
			CurrentAttachment.OnFixedTick(fixedDt);
		}
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		TextObject val = (((CurrentAttachment == null || CurrentAttachment.State != ShipAttachment.ShipAttachmentState.BridgeConnected) && (LinkedAttachmentPointMachine?.CurrentAttachment == null || LinkedAttachmentPointMachine.CurrentAttachment.State != ShipAttachment.ShipAttachmentState.BridgeConnected)) ? new TextObject("{=fEQAPJ2e}{KEY} Use", (Dictionary<string, object>)null) : new TextObject("{=PUbT3s7W}{KEY} Cut Loose", (Dictionary<string, object>)null));
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if ((CurrentAttachment == null || CurrentAttachment.State != ShipAttachment.ShipAttachmentState.BridgeConnected) && (LinkedAttachmentPointMachine?.CurrentAttachment == null || LinkedAttachmentPointMachine.CurrentAttachment.State != ShipAttachment.ShipAttachmentState.BridgeConnected))
		{
			return new TextObject("{=7zCPG8TR}Hook", (Dictionary<string, object>)null);
		}
		return new TextObject("{=kCMGJl1W}Bridge", (Dictionary<string, object>)null);
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new ShipAttachmentMachineAI(this);
	}
}
