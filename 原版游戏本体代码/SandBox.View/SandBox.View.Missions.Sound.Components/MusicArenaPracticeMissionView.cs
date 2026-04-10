using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions.Sound.Components;

public class MusicArenaPracticeMissionView : MissionView, IMusicHandler
{
	private enum ArenaIntensityLevel
	{
		None,
		Low,
		Mid,
		High
	}

	private const string ArenaSoundTag = "arena_sound";

	private const string ArenaIntensityParameterId = "ArenaIntensity";

	private const string ArenaPositiveReactionsSoundId = "event:/mission/ambient/arena/reaction";

	private const int MainAgentKnocksDownAnOpponentBaseIntensityChange = 1;

	private const int MainAgentKnocksDownAnOpponentHeadShotIntensityChange = 3;

	private const int MainAgentKnocksDownAnOpponentMountedTargetIntensityChange = 1;

	private const int MainAgentKnocksDownAnOpponentRangedHitIntensityChange = 1;

	private const int MainAgentKnocksDownAnOpponentMeleeHitIntensityChange = 2;

	private const int MainAgentHeadShotFrom15MetersRangeIntensityChange = 3;

	private const int MainAgentDismountsAnOpponentIntensityChange = 3;

	private const int MainAgentBreaksAShieldIntensityChange = 2;

	private int _currentTournamentIntensity;

	private ArenaIntensityLevel _currentArenaIntensityLevel;

	private bool _allOneShotSoundEventsAreDisabled;

	private GameEntity _arenaSoundEntity;

	bool IMusicHandler.IsPausable => false;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		MBMusicManager.Current.DeactivateCurrentMode();
		MBMusicManager.Current.ActivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerInit((IMusicHandler)(object)this);
	}

	public override void EarlyStart()
	{
		_allOneShotSoundEventsAreDisabled = false;
		_arenaSoundEntity = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("arena_sound");
		SoundManager.SetGlobalParameter("ArenaIntensity", 0f);
	}

	public override void OnMissionScreenFinalize()
	{
		SoundManager.SetGlobalParameter("ArenaIntensity", 0f);
		MBMusicManager.Current.DeactivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerFinalize();
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (affectedAgent == null || affectorAgent == null || !affectorAgent.IsMainAgent || ((int)agentState != 4 && (int)agentState != 3))
		{
			return;
		}
		if (affectedAgent.Team != affectorAgent.Team)
		{
			if (affectedAgent.IsHuman)
			{
				_currentTournamentIntensity++;
				if (affectedAgent.HasMount)
				{
					_currentTournamentIntensity++;
				}
				if ((int)killingBlow.OverrideKillInfo == 0)
				{
					_currentTournamentIntensity += 3;
				}
				if (killingBlow.IsMissile)
				{
					_currentTournamentIntensity++;
				}
				else
				{
					_currentTournamentIntensity += 2;
				}
			}
			else if (affectedAgent.RiderAgent != null)
			{
				_currentTournamentIntensity += 3;
			}
		}
		UpdateAudienceIntensity();
	}

	public override void OnMissionTick(float dt)
	{
	}

	public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		if (affectorAgent != null && affectedAgent != null && affectorAgent.IsMainAgent && affectedAgent.IsHuman)
		{
			Vec3 position = affectedAgent.Position;
			if (((Vec3)(ref position)).Distance(affectorAgent.Position) >= 15f && ((int)blow.VictimBodyPart == 0 || (int)blow.VictimBodyPart == 1))
			{
				_currentTournamentIntensity += 3;
				UpdateAudienceIntensity();
			}
		}
	}

	public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!isCanceled && attacker != null && victim != null && attacker.IsMainAgent && victim.IsHuman && collisionData.IsShieldBroken)
		{
			_currentTournamentIntensity += 2;
			UpdateAudienceIntensity();
		}
	}

	public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!isCanceled && attacker != null && victim != null && attacker.IsMainAgent && victim.IsHuman && collisionData.IsShieldBroken)
		{
			_currentTournamentIntensity += 2;
			UpdateAudienceIntensity();
		}
	}

	private void UpdateAudienceIntensity()
	{
		bool flag = false;
		if (_currentTournamentIntensity > 60)
		{
			flag = _currentArenaIntensityLevel != ArenaIntensityLevel.High;
			_currentArenaIntensityLevel = ArenaIntensityLevel.High;
		}
		else if (_currentTournamentIntensity > 30)
		{
			flag = _currentArenaIntensityLevel != ArenaIntensityLevel.Mid;
			_currentArenaIntensityLevel = ArenaIntensityLevel.Mid;
		}
		else if (_currentTournamentIntensity <= 30)
		{
			flag = _currentArenaIntensityLevel != ArenaIntensityLevel.Low;
			_currentArenaIntensityLevel = ArenaIntensityLevel.Low;
		}
		if (flag)
		{
			SoundManager.SetGlobalParameter("ArenaIntensity", (float)_currentArenaIntensityLevel);
		}
		if (!_allOneShotSoundEventsAreDisabled)
		{
			Cheer();
		}
	}

	private void Cheer()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vec3 globalPosition = _arenaSoundEntity.GlobalPosition;
		SoundManager.StartOneShotEvent("event:/mission/ambient/arena/reaction", ref globalPosition);
	}

	public void OnUpdated(float dt)
	{
	}
}
