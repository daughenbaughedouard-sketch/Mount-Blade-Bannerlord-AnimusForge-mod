using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionLogics;

public class CorpseDraggingMissionLogic : MissionLogic, IPlayerInputEffector, IMissionBehavior
{
	private enum CrouchStandEvent
	{
		None,
		Crouch,
		Stand
	}

	private enum GroundMaterialCorpseDrag
	{
		Default,
		Fabric,
		Grass,
		Mud,
		Sand,
		Snow,
		Stone,
		Water,
		Wood
	}

	private static readonly Dictionary<string, GroundMaterialCorpseDrag> _corpseDragMateriels = new Dictionary<string, GroundMaterialCorpseDrag>
	{
		{
			"",
			GroundMaterialCorpseDrag.Default
		},
		{
			"default",
			GroundMaterialCorpseDrag.Default
		},
		{
			"fabric",
			GroundMaterialCorpseDrag.Fabric
		},
		{
			"grass",
			GroundMaterialCorpseDrag.Grass
		},
		{
			"mud",
			GroundMaterialCorpseDrag.Mud
		},
		{
			"sand",
			GroundMaterialCorpseDrag.Sand
		},
		{
			"snow",
			GroundMaterialCorpseDrag.Snow
		},
		{
			"stone",
			GroundMaterialCorpseDrag.Stone
		},
		{
			"water",
			GroundMaterialCorpseDrag.Water
		},
		{
			"wood",
			GroundMaterialCorpseDrag.Wood
		}
	};

	private static int _bodyDragSoundEventId = SoundManager.GetEventGlobalIndex("event:/physics/bodydrag/human/drag/default");

	private Agent _draggedCorpse;

	private bool _startedPickingUpDraggedCorpse;

	private bool _triggerBodyGrabSound;

	private Vec3 _draggedCorpseAverageVelocity = Vec3.Zero;

	private Vec3 _draggedCorpseBoneLastGlobalPosition = Vec3.Zero;

	private sbyte _draggedCorpseBoneIndex = -1;

	private float _draggedCorpseUnbindDistanceSquared = 100f;

	private EquipmentIndex _draggedCorpseCarrierLastWieldedPrimaryWeaponIndex = (EquipmentIndex)(-1);

	private bool _draggedCorpseCarrierLastCrouchState;

	private bool _previousActionKeyPressed;

	private CrouchStandEvent _crouchStandEvent;

	private SoundEvent _bodyDragSoundEvent;

	public override void AfterStart()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		((MissionBehavior)this).Mission.FocusableObjectInformationProvider.AddInfoCallback(new GetFocusableObjectInteractionTextsDelegate(GetFocusableObjectInteractionInfoTexts));
		((MissionBehavior)this).Mission.Scene.SetFixedTickCallbackActive(true);
	}

	private void GetFocusableObjectInteractionInfoTexts(Agent requesterAgent, IFocusable focusableObject, bool isInteractable, out FocusableObjectInformation focusableObjectInformation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		focusableObjectInformation = default(FocusableObjectInformation);
		Agent val;
		if (requesterAgent.IsMainAgent && (val = (Agent)(object)((focusableObject is Agent) ? focusableObject : null)) != null && !val.IsActive())
		{
			BasicCharacterObject character = val.Character;
			focusableObjectInformation.PrimaryInteractionText = ((character != null) ? character.Name : null) ?? ((MBObjectBase)val.Monster).GetName();
			if (isInteractable)
			{
				MBTextManager.SetTextVariable("USE_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f), false);
				focusableObjectInformation.SecondaryInteractionText = GameTexts.FindText("str_key_action", (string)null);
				focusableObjectInformation.SecondaryInteractionText.SetTextVariable("KEY", GameTexts.FindText("str_ui_agent_interaction_use", (string)null));
				focusableObjectInformation.SecondaryInteractionText.SetTextVariable("ACTION", GameTexts.FindText("str_ui_drag", (string)null));
			}
			else
			{
				focusableObjectInformation.SecondaryInteractionText = null;
			}
			focusableObjectInformation.IsActive = true;
		}
		else
		{
			focusableObjectInformation.IsActive = false;
		}
	}

	private void SetDraggedCorpse(Agent draggedCorpse, sbyte draggedCorpseBoneIndex)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (_draggedCorpse != null)
		{
			_draggedCorpse.SetVelocityLimitsOnRagdoll(-1f, -1f);
			_draggedCorpse.EndRagdollAsCorpse();
		}
		_draggedCorpse = draggedCorpse;
		_draggedCorpseBoneIndex = draggedCorpseBoneIndex;
		_draggedCorpseUnbindDistanceSquared = 100f;
		_draggedCorpseAverageVelocity = Vec3.Zero;
		_draggedCorpseBoneLastGlobalPosition = Vec3.Zero;
		if (_bodyDragSoundEvent != null)
		{
			_bodyDragSoundEvent.Stop();
		}
		_bodyDragSoundEvent = null;
		if (_draggedCorpse != null)
		{
			if (!_draggedCorpse.IsAddedAsCorpse())
			{
				_draggedCorpse.AddAsCorpse();
			}
			MatrixFrame globalFrame = _draggedCorpse.AgentVisuals.GetGlobalFrame();
			MatrixFrame boneEntitialFrameWithIndex = _draggedCorpse.AgentVisuals.GetEntity().GetBoneEntitialFrameWithIndex(Math.Max(0, _draggedCorpseBoneIndex));
			_draggedCorpseBoneLastGlobalPosition = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref boneEntitialFrameWithIndex.origin);
			_draggedCorpse.StartRagdollAsCorpse();
			Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
			mainAgent.SetMaximumSpeedLimit(1f, false);
			mainAgent.SetDraggingMode(true);
			_crouchStandEvent = CrouchStandEvent.Crouch;
			_draggedCorpseCarrierLastWieldedPrimaryWeaponIndex = mainAgent.GetPrimaryWieldedItemIndex();
			_draggedCorpseCarrierLastCrouchState = mainAgent.CrouchMode;
			_startedPickingUpDraggedCorpse = true;
			_triggerBodyGrabSound = true;
		}
		else if (((MissionBehavior)this).Mission.MainAgent != null)
		{
			Agent mainAgent2 = ((MissionBehavior)this).Mission.MainAgent;
			mainAgent2.SetMaximumSpeedLimit(-1f, false);
			mainAgent2.SetDraggingMode(false);
			_crouchStandEvent = CrouchStandEvent.Stand;
			_startedPickingUpDraggedCorpse = false;
		}
	}

	public override void OnFixedMissionTick(float fixedDt)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if (_draggedCorpse == null || _crouchStandEvent != CrouchStandEvent.None || ((int)mainAgent.GetCurrentActionType(1) == 33 && _startedPickingUpDraggedCorpse) || (int)_draggedCorpse.AgentVisuals.GetCurrentRagdollState() != 3)
		{
			return;
		}
		_draggedCorpse.SetVelocityLimitsOnRagdoll(10f, 50f);
		GameEntity entity = mainAgent.AgentVisuals.GetEntity();
		MatrixFrame val = default(MatrixFrame);
		entity.GetQuickBoneEntitialFrame(mainAgent.Monster.MainHandItemBoneIndex, ref val);
		MatrixFrame globalFrameImpreciseForFixedTick = entity.GetGlobalFrameImpreciseForFixedTick();
		Vec3 val2 = ((MatrixFrame)(ref globalFrameImpreciseForFixedTick)).TransformToParent(ref val.origin);
		MatrixFrame val3 = default(MatrixFrame);
		_draggedCorpse.AgentVisuals.GetEntity().GetQuickBoneEntitialFrame(Math.Max(0, _draggedCorpseBoneIndex), ref val3);
		globalFrameImpreciseForFixedTick = _draggedCorpse.AgentVisuals.GetEntity().GetGlobalFrameImpreciseForFixedTick();
		Vec3 origin = ((MatrixFrame)(ref globalFrameImpreciseForFixedTick)).TransformToParent(ref val3).origin;
		Vec3 val4 = val2 - origin;
		Vec3 val5 = (origin - _draggedCorpseBoneLastGlobalPosition) / fixedDt;
		_draggedCorpseAverageVelocity = Vec3.Lerp(_draggedCorpseAverageVelocity, val5, 10f * fixedDt);
		_draggedCorpseBoneLastGlobalPosition = origin;
		Vec3 val7;
		if (_triggerBodyGrabSound)
		{
			EquipmentElement val6 = _draggedCorpse.SpawnEquipment[(EquipmentIndex)6];
			float soundParameterForArmorType = Agent.GetSoundParameterForArmorType((ArmorMaterialTypes)((!((EquipmentElement)(ref val6)).IsInvalid() && ((EquipmentElement)(ref val6)).Item.HasArmorComponent) ? ((int)((EquipmentElement)(ref val6)).Item.ArmorComponent.MaterialType) : 0));
			_bodyDragSoundEvent = SoundEvent.CreateEvent(_bodyDragSoundEventId, Mission.Current.Scene);
			_bodyDragSoundEvent.SetParameter("Armor Type", soundParameterForArmorType);
			GroundMaterialCorpseDrag value = GroundMaterialCorpseDrag.Default;
			_corpseDragMateriels.TryGetValue(PhysicsMaterial.GetNameAtIndex(_draggedCorpse.GetGroundMaterialForCollisionEffect()), out value);
			_bodyDragSoundEvent.SetParameter("BodydragMaterial", (float)value);
			_bodyDragSoundEvent.SetPosition(_draggedCorpse.Position);
			_bodyDragSoundEvent.Play();
			val7 = _draggedCorpse.Position;
			SoundManager.StartOneShotEvent("event:/physics/bodydrag/human/grab/body_grab", ref val7, "Armor Type", soundParameterForArmorType);
			_triggerBodyGrabSound = false;
		}
		if (((Vec3)(ref val4)).LengthSquared > _draggedCorpseUnbindDistanceSquared)
		{
			SetDraggedCorpse(null, -1);
			return;
		}
		Vec3 val8 = val4;
		val7 = mainAgent.Velocity;
		Vec3 val9 = val8 + new Vec3(((Vec3)(ref val7)).AsVec2 - ((Vec3)(ref _draggedCorpseAverageVelocity)).AsVec2 * 0.5f, 0f, -1f) * 0.5f;
		float num = 150000f * fixedDt;
		Vec3 val10 = val9 * num;
		if (((Vec3)(ref val10)).LengthSquared > num * num)
		{
			((Vec3)(ref val10)).Normalize();
			val10 *= num;
		}
		_draggedCorpse.ApplyForceOnRagdoll(_draggedCorpseBoneIndex, ref val10);
		_draggedCorpseUnbindDistanceSquared = Math.Max(2.25f, Math.Min(((Vec3)(ref val4)).LengthSquared * 1.3f * 1.3f, _draggedCorpseUnbindDistanceSquared));
		GroundMaterialCorpseDrag value2 = GroundMaterialCorpseDrag.Default;
		_corpseDragMateriels.TryGetValue(PhysicsMaterial.GetNameAtIndex(_draggedCorpse.GetGroundMaterialForCollisionEffect()), out value2);
		_bodyDragSoundEvent.SetParameter("BodydragMaterial", (float)value2);
		_bodyDragSoundEvent.SetPosition(_draggedCorpse.Position);
		SoundEvent bodyDragSoundEvent = _bodyDragSoundEvent;
		Vec2 asVec = ((Vec3)(ref _draggedCorpseAverageVelocity)).AsVec2;
		bodyDragSoundEvent.SetParameter("BodydragSpeed", ((Vec2)(ref asVec)).Length);
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Invalid comparison between Unknown and I4
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if (mainAgent == null || !mainAgent.IsActive())
		{
			SetDraggedCorpse(null, -1);
		}
		bool flag = mainAgent != null && Extensions.HasAnyFlag<MovementControlFlag>(mainAgent.MovementFlags, (MovementControlFlag)65536);
		if (_draggedCorpse != null && _crouchStandEvent == CrouchStandEvent.None && ((int)mainAgent.GetCurrentActionType(1) != 33 || !_startedPickingUpDraggedCorpse) && (int)_draggedCorpse.AgentVisuals.GetCurrentRagdollState() == 3 && dt > 0f)
		{
			_startedPickingUpDraggedCorpse = false;
			ActionCodeType currentActionType = mainAgent.GetCurrentActionType(0);
			if ((flag && !_previousActionKeyPressed) || (mainAgent.GetCurrentAction(0) != ActionIndexCache.act_none && (int)currentActionType != 25 && (int)currentActionType != 26) || mainAgent.GetCurrentAction(1) != ActionIndexCache.act_none || !mainAgent.CrouchMode || (int)mainAgent.GetPrimaryWieldedItemIndex() >= 0)
			{
				SetDraggedCorpse(null, -1);
			}
		}
		_previousActionKeyPressed = flag;
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		if (_draggedCorpse == null && userAgent.IsPlayerControlled)
		{
			if ((int)otherAgent.State != 4)
			{
				return (int)otherAgent.State == 3;
			}
			return true;
		}
		return false;
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		if (((MissionBehavior)this).IsThereAgentAction(userAgent, agent))
		{
			MBAgentVisuals agentVisuals = userAgent.AgentVisuals;
			Skeleton skeleton = agentVisuals.GetSkeleton();
			List<int> list = new List<int>(21)
			{
				agentVisuals.GetRealBoneIndex((HumanBone)20),
				agentVisuals.GetRealBoneIndex((HumanBone)27),
				agentVisuals.GetRealBoneIndex((HumanBone)19),
				agentVisuals.GetRealBoneIndex((HumanBone)26),
				agentVisuals.GetRealBoneIndex((HumanBone)18),
				agentVisuals.GetRealBoneIndex((HumanBone)25),
				agentVisuals.GetRealBoneIndex((HumanBone)17),
				agentVisuals.GetRealBoneIndex((HumanBone)24),
				agentVisuals.GetRealBoneIndex((HumanBone)16),
				agentVisuals.GetRealBoneIndex((HumanBone)23),
				agentVisuals.GetRealBoneIndex((HumanBone)15),
				agentVisuals.GetRealBoneIndex((HumanBone)22),
				agentVisuals.GetRealBoneIndex((HumanBone)4),
				agentVisuals.GetRealBoneIndex((HumanBone)8),
				agentVisuals.GetRealBoneIndex((HumanBone)3),
				agentVisuals.GetRealBoneIndex((HumanBone)7),
				agentVisuals.GetRealBoneIndex((HumanBone)2),
				agentVisuals.GetRealBoneIndex((HumanBone)6),
				agentVisuals.GetRealBoneIndex((HumanBone)1),
				agentVisuals.GetRealBoneIndex((HumanBone)5),
				agentVisuals.GetRealBoneIndex((HumanBone)13)
			};
			while (list.IndexOf(agentBoneIndex) >= 0)
			{
				agentBoneIndex = skeleton.GetParentBoneIndex(agentBoneIndex);
			}
			SetDraggedCorpse(agent, agentBoneIndex);
			_previousActionKeyPressed = true;
		}
	}

	public EventControlFlag OnCollectPlayerEventControlFlags()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected I4, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (_crouchStandEvent == CrouchStandEvent.Crouch)
		{
			_crouchStandEvent = CrouchStandEvent.None;
			return (EventControlFlag)8448;
		}
		if (_crouchStandEvent == CrouchStandEvent.Stand)
		{
			_crouchStandEvent = CrouchStandEvent.None;
			EventControlFlag val = (EventControlFlag)((!_draggedCorpseCarrierLastCrouchState) ? 16384 : 0);
			if ((int)((MissionBehavior)this).Mission.MainAgent.GetCurrentActionType(1) != 33)
			{
				EquipmentIndex draggedCorpseCarrierLastWieldedPrimaryWeaponIndex = _draggedCorpseCarrierLastWieldedPrimaryWeaponIndex;
				switch ((int)draggedCorpseCarrierLastWieldedPrimaryWeaponIndex)
				{
				case 0:
					val = (EventControlFlag)(val | 0x10);
					break;
				case 1:
					val = (EventControlFlag)(val | 0x20);
					break;
				case 2:
					val = (EventControlFlag)(val | 0x40);
					break;
				case 3:
					val = (EventControlFlag)(val | 0x80);
					break;
				}
			}
			_draggedCorpseCarrierLastCrouchState = false;
			_draggedCorpseCarrierLastWieldedPrimaryWeaponIndex = (EquipmentIndex)(-1);
			return val;
		}
		return (EventControlFlag)0;
	}
}
