using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Storyline.MissionControllers;

public class LightScriptedFiresMissionController : MissionLogic
{
	private const string FireTagExpression = "light_scripted_fire(_\\d+)*";

	private const float FireTimerAsSeconds = 3f;

	private Queue<GameEntity> _fireEntities = new Queue<GameEntity>();

	private MissionTimer _fireTimer;

	private bool _isFiringTriggered;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTagExpression("light_scripted_fire(_\\d+)*").ToList();
		GameEntity[] array = (GameEntity[])(object)new GameEntity[list.Count];
		foreach (GameEntity item2 in list)
		{
			item2.SetVisibilityExcludeParents(false);
			int num = int.Parse(item2.Tags.FirstOrDefault().Split(new char[1] { '_' })[^1]);
			array[num - 1] = item2;
		}
		GameEntity[] array2 = array;
		foreach (GameEntity item in array2)
		{
			_fireEntities.Enqueue(item);
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionTick(dt);
		if (!_isFiringTriggered)
		{
			return;
		}
		if (_fireTimer == null)
		{
			_fireTimer = new MissionTimer(3f);
		}
		else if (_fireTimer.Check(false))
		{
			_fireTimer = null;
			_fireEntities.Dequeue().SetVisibilityExcludeParents(true);
			if (Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_fireEntities))
			{
				_isFiringTriggered = false;
			}
		}
	}

	public void TriggerFiring()
	{
		if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_fireEntities))
		{
			_isFiringTriggered = true;
		}
	}

	public void PutOutFires()
	{
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTagExpression("light_scripted_fire(_\\d+)*").ToList())
		{
			item.SetVisibilityExcludeParents(false);
		}
	}
}
