using System.Collections.Generic;
using System.Linq;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.AgentControllers;

public class ArcheryTournamentAgentController : AgentController
{
	private List<DestructableComponent> _targetList;

	private DestructableComponent _target;

	private TournamentArcheryMissionController _missionController;

	public override void OnInitialize()
	{
		_missionController = Mission.Current.GetMissionBehavior<TournamentArcheryMissionController>();
	}

	public void OnTick()
	{
		if (((AgentController)this).Owner.IsAIControlled)
		{
			UpdateTarget();
		}
	}

	public void SetTargets(List<DestructableComponent> targetList)
	{
		_targetList = targetList;
		_target = null;
	}

	private void UpdateTarget()
	{
		if (_target == null || _target.IsDestroyed)
		{
			SelectNewTarget();
		}
	}

	private void SelectNewTarget()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		List<KeyValuePair<float, DestructableComponent>> list = new List<KeyValuePair<float, DestructableComponent>>();
		foreach (DestructableComponent target in _targetList)
		{
			float score = GetScore(target);
			if (score > 0f)
			{
				list.Add(new KeyValuePair<float, DestructableComponent>(score, target));
			}
		}
		if (list.Count == 0)
		{
			_target = null;
			((AgentController)this).Owner.DisableScriptedCombatMovement();
			WorldPosition worldPosition = ((AgentController)this).Owner.GetWorldPosition();
			((AgentController)this).Owner.SetScriptedPosition(ref worldPosition, false, (AIScriptedFrameFlags)0);
		}
		else
		{
			List<KeyValuePair<float, DestructableComponent>> list2 = list.OrderByDescending((KeyValuePair<float, DestructableComponent> x) => x.Key).ToList();
			int num = MathF.Min(list2.Count, 5);
			_target = list2[MBRandom.RandomInt(num)].Value;
		}
		if (_target != null)
		{
			((AgentController)this).Owner.SetScriptedTargetEntity(((ScriptComponentBehavior)_target).GameEntity, (AISpecialCombatModeFlags)0, false);
		}
	}

	private float GetScore(DestructableComponent target)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!target.IsDestroyed)
		{
			Vec3 position = ((AgentController)this).Owner.Position;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)target).GameEntity;
			return 1f / ((Vec3)(ref position)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		}
		return 0f;
	}

	public void OnTargetHit(Agent agent, DestructableComponent target)
	{
		if (agent == ((AgentController)this).Owner || target == _target)
		{
			SelectNewTarget();
		}
	}
}
