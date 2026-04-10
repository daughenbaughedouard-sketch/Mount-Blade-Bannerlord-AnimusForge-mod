using System;
using System.Collections.Generic;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.AgentControllers;

public class JoustingAgentController : AgentController
{
	public enum JoustingAgentState
	{
		GoingToBackStart,
		GoToStartPosition,
		WaitInStartPosition,
		WaitingOpponent,
		Ready,
		StartRiding,
		Riding,
		RidingAtWrongSide,
		SwordDuel
	}

	private JoustingAgentState _state;

	public int CurrentCornerIndex;

	private const float MaxDistance = 3f;

	public int Score;

	private Agent _opponentAgent;

	public JoustingAgentState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (value != _state)
			{
				_state = value;
				JoustingMissionController.OnJoustingAgentStateChanged(((AgentController)this).Owner, value);
			}
		}
	}

	public TournamentJoustingMissionController JoustingMissionController { get; private set; }

	public Agent Opponent
	{
		get
		{
			if (_opponentAgent == null)
			{
				foreach (Agent item in (List<Agent>)(object)((AgentController)this).Mission.Agents)
				{
					if (item.IsHuman && item != ((AgentController)this).Owner)
					{
						_opponentAgent = item;
					}
				}
			}
			return _opponentAgent;
		}
	}

	public bool PrepareEquipmentsAfterDismount { get; private set; }

	public override void OnInitialize()
	{
		JoustingMissionController = ((AgentController)this).Mission.GetMissionBehavior<TournamentJoustingMissionController>();
		_state = JoustingAgentState.WaitingOpponent;
	}

	public void UpdateState()
	{
		if (((AgentController)this).Owner.Character.IsPlayerCharacter)
		{
			UpdateMainAgentState();
		}
		else
		{
			UpdateAIAgentState();
		}
	}

	private void UpdateMainAgentState()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		JoustingAgentController controller = Opponent.GetController<JoustingAgentController>();
		bool flag = JoustingMissionController.CornerStartList[CurrentCornerIndex].CheckPointWithOrientedBoundingBox(((AgentController)this).Owner.Position) && !JoustingMissionController.RegionBoxList[CurrentCornerIndex].CheckPointWithOrientedBoundingBox(((AgentController)this).Owner.Position);
		Vec2 currentVelocity;
		switch (State)
		{
		case JoustingAgentState.GoToStartPosition:
			if (flag)
			{
				State = JoustingAgentState.WaitInStartPosition;
			}
			break;
		case JoustingAgentState.WaitInStartPosition:
			if (!flag)
			{
				State = JoustingAgentState.GoToStartPosition;
				break;
			}
			currentVelocity = ((AgentController)this).Owner.GetCurrentVelocity();
			if (((Vec2)(ref currentVelocity)).LengthSquared < 0.0025000002f)
			{
				State = JoustingAgentState.WaitingOpponent;
			}
			break;
		case JoustingAgentState.WaitingOpponent:
			if (!flag)
			{
				State = JoustingAgentState.GoToStartPosition;
			}
			else if (controller.State == JoustingAgentState.WaitingOpponent || controller.State == JoustingAgentState.Ready)
			{
				State = JoustingAgentState.Ready;
			}
			break;
		case JoustingAgentState.Ready:
			if (JoustingMissionController.IsAgentInTheTrack(((AgentController)this).Owner))
			{
				currentVelocity = ((AgentController)this).Owner.GetCurrentVelocity();
				if (((Vec2)(ref currentVelocity)).LengthSquared > 0.0025000002f)
				{
					State = JoustingAgentState.Riding;
					break;
				}
			}
			if (controller.State == JoustingAgentState.GoToStartPosition)
			{
				State = JoustingAgentState.WaitingOpponent;
			}
			else if (!JoustingMissionController.CornerStartList[CurrentCornerIndex].CheckPointWithOrientedBoundingBox(((AgentController)this).Owner.Position))
			{
				State = JoustingAgentState.GoToStartPosition;
			}
			break;
		case JoustingAgentState.Riding:
			if (JoustingMissionController.IsAgentInTheTrack(((AgentController)this).Owner, inCurrentTrack: false))
			{
				State = JoustingAgentState.RidingAtWrongSide;
			}
			if (JoustingMissionController.RegionExitBoxList[CurrentCornerIndex].CheckPointWithOrientedBoundingBox(((AgentController)this).Owner.Position))
			{
				State = JoustingAgentState.GoToStartPosition;
				CurrentCornerIndex = 1 - CurrentCornerIndex;
			}
			break;
		case JoustingAgentState.RidingAtWrongSide:
			if (JoustingMissionController.IsAgentInTheTrack(((AgentController)this).Owner))
			{
				State = JoustingAgentState.Riding;
			}
			else if (JoustingMissionController.CornerStartList[1 - CurrentCornerIndex].CheckPointWithOrientedBoundingBox(((AgentController)this).Owner.Position))
			{
				State = JoustingAgentState.GoToStartPosition;
				CurrentCornerIndex = 1 - CurrentCornerIndex;
			}
			break;
		case JoustingAgentState.StartRiding:
			break;
		}
	}

	private void UpdateAIAgentState()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		if (Opponent == null || !Opponent.IsActive())
		{
			return;
		}
		JoustingAgentController controller = Opponent.GetController<JoustingAgentController>();
		Vec3 position;
		Vec2 val3;
		switch (State)
		{
		case JoustingAgentState.GoingToBackStart:
			position = ((AgentController)this).Owner.Position;
			if (((Vec3)(ref position)).Distance(JoustingMissionController.CornerBackStartList[CurrentCornerIndex].origin) < 3f)
			{
				val3 = ((AgentController)this).Owner.GetCurrentVelocity();
				if (((Vec2)(ref val3)).LengthSquared < 0.0025000002f)
				{
					CurrentCornerIndex = 1 - CurrentCornerIndex;
					MatrixFrame globalFrame = JoustingMissionController.CornerStartList[CurrentCornerIndex].GetGlobalFrame();
					WorldPosition val4 = default(WorldPosition);
					((WorldPosition)(ref val4))._002Ector(Mission.Current.Scene, UIntPtr.Zero, globalFrame.origin, false);
					Agent owner = ((AgentController)this).Owner;
					val3 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
					owner.SetScriptedPositionAndDirection(ref val4, ((Vec2)(ref val3)).RotationInRadians, false, (AIScriptedFrameFlags)0);
					State = JoustingAgentState.GoToStartPosition;
				}
			}
			break;
		case JoustingAgentState.GoToStartPosition:
			if (JoustingMissionController.CornerStartList[CurrentCornerIndex].CheckPointWithOrientedBoundingBox(((AgentController)this).Owner.Position))
			{
				val3 = ((AgentController)this).Owner.GetCurrentVelocity();
				if (((Vec2)(ref val3)).LengthSquared < 0.0025000002f)
				{
					State = JoustingAgentState.WaitingOpponent;
				}
			}
			break;
		case JoustingAgentState.WaitingOpponent:
			if (controller.State == JoustingAgentState.WaitingOpponent || controller.State == JoustingAgentState.Ready)
			{
				State = JoustingAgentState.Ready;
			}
			break;
		case JoustingAgentState.Ready:
			if (controller.State == JoustingAgentState.Riding)
			{
				State = JoustingAgentState.StartRiding;
				WorldPosition val5 = default(WorldPosition);
				((WorldPosition)(ref val5))._002Ector(Mission.Current.Scene, UIntPtr.Zero, JoustingMissionController.CornerMiddleList[CurrentCornerIndex].origin, false);
				((AgentController)this).Owner.SetScriptedPosition(ref val5, false, (AIScriptedFrameFlags)8);
			}
			else if (controller.State == JoustingAgentState.Ready)
			{
				WorldPosition val6 = default(WorldPosition);
				((WorldPosition)(ref val6))._002Ector(Mission.Current.Scene, UIntPtr.Zero, JoustingMissionController.CornerStartList[CurrentCornerIndex].GetGlobalFrame().origin, false);
				((AgentController)this).Owner.SetScriptedPosition(ref val6, false, (AIScriptedFrameFlags)8);
			}
			else
			{
				State = JoustingAgentState.WaitingOpponent;
			}
			break;
		case JoustingAgentState.StartRiding:
			position = ((AgentController)this).Owner.Position;
			if (((Vec3)(ref position)).Distance(JoustingMissionController.CornerMiddleList[CurrentCornerIndex].origin) < 3f)
			{
				WorldPosition val2 = default(WorldPosition);
				((WorldPosition)(ref val2))._002Ector(Mission.Current.Scene, UIntPtr.Zero, JoustingMissionController.CornerFinishList[CurrentCornerIndex].origin, false);
				((AgentController)this).Owner.SetScriptedPosition(ref val2, false, (AIScriptedFrameFlags)8);
				State = JoustingAgentState.Riding;
			}
			break;
		case JoustingAgentState.Riding:
			position = ((AgentController)this).Owner.Position;
			if (((Vec3)(ref position)).Distance(JoustingMissionController.CornerFinishList[CurrentCornerIndex].origin) < 3f)
			{
				WorldPosition val = default(WorldPosition);
				((WorldPosition)(ref val))._002Ector(Mission.Current.Scene, UIntPtr.Zero, JoustingMissionController.CornerBackStartList[CurrentCornerIndex].origin, false);
				((AgentController)this).Owner.SetScriptedPosition(ref val, false, (AIScriptedFrameFlags)0);
				State = JoustingAgentState.GoingToBackStart;
			}
			break;
		case JoustingAgentState.WaitInStartPosition:
			break;
		}
	}

	public void PrepareAgentToSwordDuel()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (((AgentController)this).Owner.MountAgent != null)
		{
			((AgentController)this).Owner.Controller = (AgentControllerType)1;
			WorldPosition worldPosition = Opponent.GetWorldPosition();
			((AgentController)this).Owner.SetScriptedPosition(ref worldPosition, false, (AIScriptedFrameFlags)32);
			PrepareEquipmentsAfterDismount = true;
		}
		else
		{
			PrepareEquipmentsForSwordDuel();
			((AgentController)this).Owner.DisableScriptedMovement();
		}
	}

	public void PrepareEquipmentsForSwordDuel()
	{
		AddEquipmentsForSwordDuel();
		((AgentController)this).Owner.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
		PrepareEquipmentsAfterDismount = false;
	}

	private void AddEquipmentsForSwordDuel()
	{
		((AgentController)this).Owner.DropItem((EquipmentIndex)0, (WeaponClass)0);
		ItemObject obj = Game.Current.ObjectManager.GetObject<ItemObject>("wooden_sword_t1");
		IAgentOriginBase origin = ((AgentController)this).Owner.Origin;
		MissionWeapon val = default(MissionWeapon);
		((MissionWeapon)(ref val))._002Ector(obj, (ItemModifier)null, (origin != null) ? origin.Banner : null);
		((AgentController)this).Owner.EquipWeaponWithNewEntity((EquipmentIndex)2, ref val);
	}

	public bool IsRiding()
	{
		if (State != JoustingAgentState.StartRiding)
		{
			return State == JoustingAgentState.Riding;
		}
		return true;
	}
}
