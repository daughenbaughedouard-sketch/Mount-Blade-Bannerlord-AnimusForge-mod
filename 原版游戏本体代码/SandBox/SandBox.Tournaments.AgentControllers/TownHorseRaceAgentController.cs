using System;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.AgentControllers;

public class TownHorseRaceAgentController : AgentController
{
	private TownHorseRaceMissionController _controller;

	private int _checkPointIndex;

	private int _tourCount;

	public override void OnInitialize()
	{
		_controller = ((AgentController)this).Mission.GetMissionBehavior<TownHorseRaceMissionController>();
		_checkPointIndex = 0;
		_tourCount = 0;
	}

	public void DisableMovement()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (((AgentController)this).Owner.IsAIControlled)
		{
			WorldPosition worldPosition = ((AgentController)this).Owner.GetWorldPosition();
			Agent owner = ((AgentController)this).Owner;
			MatrixFrame frame = ((AgentController)this).Owner.Frame;
			Vec2 asVec = ((Vec3)(ref frame.rotation.f)).AsVec2;
			owner.SetScriptedPositionAndDirection(ref worldPosition, ((Vec2)(ref asVec)).RotationInRadians, false, (AIScriptedFrameFlags)0);
		}
	}

	public void Start()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (_checkPointIndex < _controller.CheckPoints.Count)
		{
			TownHorseRaceMissionController.CheckPoint checkPoint = _controller.CheckPoints[_checkPointIndex];
			checkPoint.AddToCheckList(((AgentController)this).Owner);
			if (((AgentController)this).Owner.IsAIControlled)
			{
				WorldPosition val = default(WorldPosition);
				((WorldPosition)(ref val))._002Ector(Mission.Current.Scene, UIntPtr.Zero, checkPoint.GetBestTargetPosition(), false);
				((AgentController)this).Owner.SetScriptedPosition(ref val, false, (AIScriptedFrameFlags)8);
			}
		}
	}

	public void OnEnterCheckPoint(VolumeBox checkPoint)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		_controller.CheckPoints[_checkPointIndex].RemoveFromCheckList(((AgentController)this).Owner);
		_checkPointIndex++;
		if (_checkPointIndex < _controller.CheckPoints.Count)
		{
			if (((AgentController)this).Owner.IsAIControlled)
			{
				WorldPosition val = default(WorldPosition);
				((WorldPosition)(ref val))._002Ector(Mission.Current.Scene, UIntPtr.Zero, _controller.CheckPoints[_checkPointIndex].GetBestTargetPosition(), false);
				((AgentController)this).Owner.SetScriptedPosition(ref val, false, (AIScriptedFrameFlags)8);
			}
			_controller.CheckPoints[_checkPointIndex].AddToCheckList(((AgentController)this).Owner);
			return;
		}
		_tourCount++;
		if (_tourCount < 2)
		{
			_checkPointIndex = 0;
			if (((AgentController)this).Owner.IsAIControlled)
			{
				WorldPosition val2 = default(WorldPosition);
				((WorldPosition)(ref val2))._002Ector(Mission.Current.Scene, UIntPtr.Zero, _controller.CheckPoints[_checkPointIndex].GetBestTargetPosition(), false);
				((AgentController)this).Owner.SetScriptedPosition(ref val2, false, (AIScriptedFrameFlags)8);
			}
			_controller.CheckPoints[_checkPointIndex].AddToCheckList(((AgentController)this).Owner);
		}
	}
}
