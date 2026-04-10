using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class HeroSkillHandler : MissionLogic
{
	private MissionTime _nextCaptainSkillMoraleBoostTime;

	private bool _boostMorale;

	private int _nextMoraleTeam;

	public override void AfterStart()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_nextCaptainSkillMoraleBoostTime = MissionTime.SecondsFromNow(10f);
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (((MissionTime)(ref _nextCaptainSkillMoraleBoostTime)).IsPast)
		{
			_boostMorale = true;
			_nextMoraleTeam = 0;
			_nextCaptainSkillMoraleBoostTime = MissionTime.SecondsFromNow(10f);
		}
		if (_boostMorale)
		{
			if (_nextMoraleTeam >= ((List<Team>)(object)((MissionBehavior)this).Mission.Teams).Count)
			{
				_boostMorale = false;
				return;
			}
			Team team = ((List<Team>)(object)((MissionBehavior)this).Mission.Teams)[_nextMoraleTeam];
			BoostMoraleForTeam(team);
			_nextMoraleTeam++;
		}
	}

	private void BoostMoraleForTeam(Team team)
	{
	}
}
