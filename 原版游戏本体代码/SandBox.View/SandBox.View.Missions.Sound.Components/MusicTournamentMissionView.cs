using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using psai.net;

namespace SandBox.View.Missions.Sound.Components;

public class MusicTournamentMissionView : MissionView, IMusicHandler
{
	private enum ArenaIntensityLevel
	{
		None,
		Low,
		Mid,
		High
	}

	private enum ReactionType
	{
		Positive,
		Negative,
		End
	}

	private const string ArenaSoundTag = "arena_sound";

	private const string ArenaIntensityParameterId = "ArenaIntensity";

	private const string ArenaPositiveReactionsSoundId = "event:/mission/ambient/arena/reaction";

	private const string ArenaNegativeReactionsSoundId = "event:/mission/ambient/arena/negative_reaction";

	private const string ArenaTournamentEndSoundId = "event:/mission/ambient/arena/reaction";

	private const int MainAgentKnocksDownAnOpponentBaseIntensityChange = 1;

	private const int MainAgentKnocksDownAnOpponentHeadShotIntensityChange = 3;

	private const int MainAgentKnocksDownAnOpponentMountedTargetIntensityChange = 1;

	private const int MainAgentKnocksDownAnOpponentRangedHitIntensityChange = 1;

	private const int MainAgentKnocksDownAnOpponentMeleeHitIntensityChange = 2;

	private const int MainAgentHeadShotFrom15MetersRangeIntensityChange = 3;

	private const int MainAgentDismountsAnOpponentIntensityChange = 3;

	private const int MainAgentBreaksAShieldIntensityChange = 2;

	private const int MainAgentWinsTournamentRoundIntensityChange = 10;

	private const int RoundEndIntensityChange = 10;

	private const int MainAgentKnocksDownATeamMateIntensityChange = -30;

	private const int MainAgentKnocksDownAFriendlyHorseIntensityChange = -20;

	private int _currentTournamentIntensity;

	private ArenaIntensityLevel _arenaIntensityLevel;

	private bool _allOneShotSoundEventsAreDisabled;

	private TournamentBehavior _tournamentBehavior;

	private TournamentMatch _currentMatch;

	private TournamentMatch _lastMatch;

	private GameEntity _arenaSoundEntity;

	private bool _isFinalRound;

	private bool _fightStarted;

	private Timer _startTimer;

	bool IMusicHandler.IsPausable => false;

	public override void OnBehaviorInitialize()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		((MissionBehavior)this).OnBehaviorInitialize();
		MBMusicManager.Current.DeactivateCurrentMode();
		MBMusicManager.Current.ActivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerInit((IMusicHandler)(object)this);
		_startTimer = new Timer(Mission.Current.CurrentTime, 3f, true);
	}

	public override void EarlyStart()
	{
		_allOneShotSoundEventsAreDisabled = false;
		_tournamentBehavior = Mission.Current.GetMissionBehavior<TournamentBehavior>();
		_currentMatch = null;
		_lastMatch = null;
		_arenaSoundEntity = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("arena_sound");
		SoundManager.SetGlobalParameter("ArenaIntensity", 0f);
	}

	public override void OnMissionScreenFinalize()
	{
		SoundManager.SetGlobalParameter("ArenaIntensity", 0f);
		MBMusicManager.Current.DeactivateBattleMode();
		MBMusicManager.Current.OnBattleMusicHandlerFinalize();
	}

	private void CheckIntensityFall()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
		if (((PsaiInfo)(ref psaiInfo)).effectiveThemeId >= 0)
		{
			if (float.IsNaN(((PsaiInfo)(ref psaiInfo)).currentIntensity))
			{
				MBMusicManager.Current.ChangeCurrentThemeIntensity(MusicParameters.MinIntensity);
			}
			else if (((PsaiInfo)(ref psaiInfo)).currentIntensity < MusicParameters.MinIntensity)
			{
				MBMusicManager.Current.ChangeCurrentThemeIntensity(MusicParameters.MinIntensity - ((PsaiInfo)(ref psaiInfo)).currentIntensity);
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between I4 and Unknown
		if (_fightStarted)
		{
			bool flag = affectedAgent.IsMine || (affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsMine);
			Team team = affectedAgent.Team;
			BattleSideEnum val = (BattleSideEnum)((team == null) ? (-1) : ((int)team.Side));
			int num;
			if (!flag)
			{
				if ((int)val != -1)
				{
					Team playerTeam = Mission.Current.PlayerTeam;
					num = ((((playerTeam == null) ? (-1) : ((int)playerTeam.Side)) == (int)val) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
			}
			else
			{
				num = 1;
			}
			bool flag2 = (byte)num != 0;
			if ((affectedAgent.IsHuman && (int)affectedAgent.State != 2) || flag)
			{
				float num2 = (flag2 ? MusicParameters.FriendlyTroopDeadEffectOnIntensity : MusicParameters.EnemyTroopDeadEffectOnIntensity);
				if (flag)
				{
					num2 *= MusicParameters.PlayerTroopDeadEffectMultiplierOnIntensity;
				}
				MBMusicManager.Current.ChangeCurrentThemeIntensity(num2);
			}
		}
		if (affectedAgent == null || affectorAgent == null || !affectorAgent.IsMainAgent || ((int)agentState != 4 && (int)agentState != 3))
		{
			return;
		}
		int num3 = 0;
		if (affectedAgent.Team == affectorAgent.Team)
		{
			num3 = ((!affectedAgent.IsHuman) ? (num3 + -20) : (num3 + -30));
		}
		else if (affectedAgent.IsHuman)
		{
			num3++;
			if (affectedAgent.HasMount)
			{
				num3++;
			}
			if ((int)killingBlow.OverrideKillInfo == 0)
			{
				num3 += 3;
			}
			num3 = ((!killingBlow.IsMissile) ? (num3 + 2) : (num3 + 1));
		}
		else if (affectedAgent.RiderAgent != null)
		{
			num3 += 3;
		}
		UpdateAudienceIntensity(num3);
	}

	void IMusicHandler.OnUpdated(float dt)
	{
		if (!_fightStarted && Agent.Main != null && Agent.Main.IsActive() && _startTimer.Check(Mission.Current.CurrentTime))
		{
			_fightStarted = true;
			MBMusicManager.Current.StartTheme((MusicTheme)12, 0.5f, false);
		}
		if (_fightStarted)
		{
			CheckIntensityFall();
		}
	}

	public override void OnMissionTick(float dt)
	{
		if (_tournamentBehavior == null)
		{
			return;
		}
		if (_currentMatch != _tournamentBehavior.CurrentMatch)
		{
			TournamentMatch currentMatch = _tournamentBehavior.CurrentMatch;
			if (currentMatch != null && currentMatch.IsPlayerParticipating())
			{
				Agent main = Agent.Main;
				if (main != null && main.IsActive())
				{
					_currentMatch = _tournamentBehavior.CurrentMatch;
					OnTournamentRoundBegin(_tournamentBehavior.NextRound == null);
				}
			}
		}
		if (_lastMatch != _tournamentBehavior.LastMatch)
		{
			_lastMatch = _tournamentBehavior.LastMatch;
			if (_tournamentBehavior.NextRound == null || _tournamentBehavior.LastMatch.IsPlayerParticipating())
			{
				OnTournamentRoundEnd();
			}
		}
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
				UpdateAudienceIntensity(3);
			}
		}
	}

	public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!isCanceled && attacker != null && victim != null && attacker.IsMainAgent && victim.IsHuman && collisionData.IsShieldBroken)
		{
			UpdateAudienceIntensity(2);
		}
	}

	public override void OnMeleeHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (!isCanceled && attacker != null && victim != null && attacker.IsMainAgent && victim.IsHuman && collisionData.IsShieldBroken)
		{
			UpdateAudienceIntensity(2);
		}
	}

	private void UpdateAudienceIntensity(int intensityChangeAmount, bool isEnd = false)
	{
		ReactionType reactionType = (isEnd ? ReactionType.End : ((intensityChangeAmount < 0) ? ReactionType.Negative : ReactionType.Positive));
		_currentTournamentIntensity += intensityChangeAmount;
		bool flag = false;
		if (_currentTournamentIntensity > 60)
		{
			flag = _arenaIntensityLevel != ArenaIntensityLevel.High;
			_arenaIntensityLevel = ArenaIntensityLevel.High;
		}
		else if (_currentTournamentIntensity > 30)
		{
			flag = _arenaIntensityLevel != ArenaIntensityLevel.Mid;
			_arenaIntensityLevel = ArenaIntensityLevel.Mid;
		}
		else if (_currentTournamentIntensity <= 30)
		{
			flag = _arenaIntensityLevel != ArenaIntensityLevel.Low;
			_arenaIntensityLevel = ArenaIntensityLevel.Low;
		}
		if (flag)
		{
			SoundManager.SetGlobalParameter("ArenaIntensity", (float)_arenaIntensityLevel);
		}
		if (!_allOneShotSoundEventsAreDisabled)
		{
			Cheer(reactionType);
		}
	}

	private void Cheer(ReactionType reactionType)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		string text = null;
		switch (reactionType)
		{
		case ReactionType.Positive:
			text = "event:/mission/ambient/arena/reaction";
			break;
		case ReactionType.Negative:
			text = "event:/mission/ambient/arena/negative_reaction";
			break;
		case ReactionType.End:
			text = "event:/mission/ambient/arena/reaction";
			break;
		}
		if (text != null)
		{
			string text2 = text;
			Vec3 globalPosition = _arenaSoundEntity.GlobalPosition;
			SoundManager.StartOneShotEvent(text2, ref globalPosition);
		}
	}

	public void OnTournamentRoundBegin(bool isFinalRound)
	{
		_isFinalRound = isFinalRound;
		UpdateAudienceIntensity(0);
	}

	public void OnTournamentRoundEnd()
	{
		int num = 10;
		if (_lastMatch.IsPlayerWinner())
		{
			num += 10;
		}
		UpdateAudienceIntensity(num, _isFinalRound);
	}
}
