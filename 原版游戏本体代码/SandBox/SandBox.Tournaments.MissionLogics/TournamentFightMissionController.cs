using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Tournaments.MissionLogics;

public class TournamentFightMissionController : MissionLogic, ITournamentGameBehavior
{
	private readonly CharacterObject _defaultWeaponTemplatesIdTeamSizeOne = MBObjectManager.Instance.GetObject<CharacterObject>("tournament_template_empire_one_participant_set_v1");

	private readonly CharacterObject _defaultWeaponTemplatesIdTeamSizeTwo = MBObjectManager.Instance.GetObject<CharacterObject>("tournament_template_empire_two_participant_set_v1");

	private readonly CharacterObject _defaultWeaponTemplatesIdTeamSizeFour = MBObjectManager.Instance.GetObject<CharacterObject>("tournament_template_empire_four_participant_set_v1");

	private TournamentMatch _match;

	private bool _isLastRound;

	private BasicMissionTimer _endTimer;

	private BasicMissionTimer _cheerTimer;

	private List<GameEntity> _spawnPoints;

	private bool _isSimulated;

	private bool _forceEndMatch;

	private bool _cheerStarted;

	private CultureObject _culture;

	private List<TournamentParticipant> _aliveParticipants;

	private List<TournamentTeam> _aliveTeams;

	private List<Agent> _currentTournamentAgents;

	private List<Agent> _currentTournamentMountAgents;

	private const float XpShareForKill = 0.5f;

	private const float XpShareForDamage = 0.5f;

	public TournamentFightMissionController(CultureObject culture)
	{
		_match = null;
		_culture = culture;
		_cheerStarted = false;
		_currentTournamentAgents = new List<Agent>();
		_currentTournamentMountAgents = new List<Agent>();
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).Mission.CanAgentRout_AdditionalCondition += CanAgentRout;
	}

	public override void AfterStart()
	{
		TournamentBehavior.DeleteTournamentSetsExcept(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("tournament_fight"));
		_spawnPoints = new List<GameEntity>();
		for (int i = 0; i < 4; i++)
		{
			GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_arena_" + (i + 1));
			if (val != (GameEntity)null)
			{
				_spawnPoints.Add(val);
			}
		}
		if (_spawnPoints.Count < 4)
		{
			_spawnPoints = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_arena").ToList();
		}
	}

	public void PrepareForMatch()
	{
		List<Equipment> teamWeaponEquipmentList = GetTeamWeaponEquipmentList(_match.Teams.First().Participants.Count());
		foreach (TournamentTeam team in _match.Teams)
		{
			int num = 0;
			foreach (TournamentParticipant participant in team.Participants)
			{
				participant.MatchEquipment = teamWeaponEquipmentList[num].Clone(false);
				AddRandomClothes(_culture, participant);
				num++;
			}
		}
	}

	public void StartMatch(TournamentMatch match, bool isLastRound)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		_cheerStarted = false;
		_match = match;
		_isLastRound = isLastRound;
		PrepareForMatch();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
		List<Team> list = new List<Team>();
		int count = _spawnPoints.Count;
		int num = 0;
		foreach (TournamentTeam team in _match.Teams)
		{
			BattleSideEnum val = (BattleSideEnum)(!team.IsPlayerTeam);
			Team val2 = ((MissionBehavior)this).Mission.Teams.Add(val, team.TeamColor, uint.MaxValue, team.TeamBanner, true, false, true);
			GameEntity spawnPoint = _spawnPoints[num % count];
			foreach (TournamentParticipant participant in team.Participants)
			{
				if (((BasicCharacterObject)participant.Character).IsPlayerCharacter)
				{
					SpawnTournamentParticipant(spawnPoint, participant, val2);
					break;
				}
			}
			foreach (TournamentParticipant participant2 in team.Participants)
			{
				if (!((BasicCharacterObject)participant2.Character).IsPlayerCharacter)
				{
					SpawnTournamentParticipant(spawnPoint, participant2, val2);
				}
			}
			num++;
			list.Add(val2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = i + 1; j < list.Count; j++)
			{
				list[i].SetIsEnemyOf(list[j], true);
			}
		}
		_aliveParticipants = _match.Participants.ToList();
		_aliveTeams = _match.Teams.ToList();
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).Mission.CanAgentRout_AdditionalCondition -= CanAgentRout;
	}

	private void SpawnTournamentParticipant(GameEntity spawnPoint, TournamentParticipant participant, Team team)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = spawnPoint.GetGlobalFrame();
		((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		SpawnAgentWithRandomItems(participant, team, globalFrame);
	}

	private List<Equipment> GetTeamWeaponEquipmentList(int teamSize)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		List<Equipment> list = new List<Equipment>();
		CultureObject culture = PlayerEncounter.EncounterSettlement.Culture;
		MBReadOnlyList<CharacterObject> val = (MBReadOnlyList<CharacterObject>)(teamSize switch
		{
			2 => culture.TournamentTeamTemplatesForTwoParticipant, 
			4 => culture.TournamentTeamTemplatesForFourParticipant, 
			_ => culture.TournamentTeamTemplatesForOneParticipant, 
		});
		CharacterObject val2 = (CharacterObject)((((List<CharacterObject>)(object)val).Count <= 0) ? (teamSize switch
		{
			2 => _defaultWeaponTemplatesIdTeamSizeTwo, 
			4 => _defaultWeaponTemplatesIdTeamSizeFour, 
			_ => _defaultWeaponTemplatesIdTeamSizeOne, 
		}) : ((List<CharacterObject>)(object)val)[MBRandom.RandomInt(((List<CharacterObject>)(object)val).Count)]);
		foreach (Equipment battleEquipment in ((BasicCharacterObject)val2).BattleEquipments)
		{
			Equipment val3 = new Equipment();
			val3.FillFrom(battleEquipment, true);
			list.Add(val3);
		}
		return list;
	}

	public void SkipMatch(TournamentMatch match)
	{
		_match = match;
		PrepareForMatch();
		Simulate();
	}

	public bool IsMatchEnded()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		if (_isSimulated || _match == null)
		{
			return true;
		}
		if ((_endTimer != null && _endTimer.ElapsedTime > 6f) || _forceEndMatch)
		{
			_forceEndMatch = false;
			_endTimer = null;
			return true;
		}
		if (_cheerTimer != null && !_cheerStarted && _cheerTimer.ElapsedTime > 1f)
		{
			OnMatchResultsReady();
			_cheerTimer = null;
			_cheerStarted = true;
			AgentVictoryLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<AgentVictoryLogic>();
			foreach (Agent currentTournamentAgent in _currentTournamentAgents)
			{
				if (currentTournamentAgent.IsAIControlled)
				{
					missionBehavior.SetTimersOfVictoryReactionsOnTournamentVictoryForAgent(currentTournamentAgent, 1f, 3f);
				}
			}
			return false;
		}
		if (_endTimer == null && !CheckIfIsThereAnyEnemies())
		{
			_endTimer = new BasicMissionTimer();
			if (!_cheerStarted)
			{
				_cheerTimer = new BasicMissionTimer();
			}
		}
		return false;
	}

	public void OnMatchResultsReady()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		if (_match.IsPlayerParticipating())
		{
			if (_match.IsPlayerWinner())
			{
				if (_isLastRound)
				{
					if ((int)_match.QualificationMode == 0)
					{
						MBInformationManager.AddQuickInformation(new TextObject("{=Jn0k20c3}Round is over, you survived the final round of the tournament.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
					else
					{
						MBInformationManager.AddQuickInformation(new TextObject("{=wOqOQuJl}Round is over, your team survived the final round of the tournament.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
				}
				else if ((int)_match.QualificationMode == 0)
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=uytwdSVH}Round is over, you are qualified for the next stage of the tournament.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
				else
				{
					MBInformationManager.AddQuickInformation(new TextObject("{=fkOYvnVG}Round is over, your team is qualified for the next stage of the tournament.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
			}
			else if ((int)_match.QualificationMode == 0)
			{
				MBInformationManager.AddQuickInformation(new TextObject("{=lcVauEKV}Round is over, you are disqualified from the tournament.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
			else
			{
				MBInformationManager.AddQuickInformation(new TextObject("{=MLyBN51z}Round is over, your team is disqualified from the tournament.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
		}
		else
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=UBd0dEPp}Match is over", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		}
	}

	public void OnMatchEnded()
	{
		SandBoxHelpers.MissionHelper.FadeOutAgents(_currentTournamentAgents.Where((Agent x) => x.IsActive()), hideInstantly: true, hideMount: false);
		SandBoxHelpers.MissionHelper.FadeOutAgents(_currentTournamentMountAgents.Where((Agent x) => x.IsActive()), hideInstantly: true, hideMount: false);
		((MissionBehavior)this).Mission.ClearCorpses(false);
		((MissionBehavior)this).Mission.Teams.Clear();
		((MissionBehavior)this).Mission.RemoveSpawnedItemsAndMissiles();
		_match = null;
		_endTimer = null;
		_cheerTimer = null;
		_isSimulated = false;
		_currentTournamentAgents.Clear();
		_currentTournamentMountAgents.Clear();
	}

	private void SpawnAgentWithRandomItems(TournamentParticipant participant, Team team, MatrixFrame frame)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		((MatrixFrame)(ref frame)).Strafe((float)MBRandom.RandomInt(-2, 2) * 1f);
		((MatrixFrame)(ref frame)).Advance((float)MBRandom.RandomInt(0, 2) * 1f);
		CharacterObject character = participant.Character;
		AgentBuildData obj = new AgentBuildData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, participant.Descriptor)).Team(team).InitialPosition(ref frame.origin);
		Vec2 val = ((Vec3)(ref frame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj.InitialDirection(ref val).Equipment(participant.MatchEquipment).ClothingColor1(team.Color)
			.Banner(team.Banner)
			.Controller((AgentControllerType)((!((BasicCharacterObject)character).IsPlayerCharacter) ? 1 : 2));
		Agent val3 = ((MissionBehavior)this).Mission.SpawnAgent(val2, false);
		if (((BasicCharacterObject)character).IsPlayerCharacter)
		{
			val3.Health = character.HeroObject.HitPoints;
			((MissionBehavior)this).Mission.PlayerTeam = team;
		}
		else
		{
			val3.SetWatchState((WatchState)2);
		}
		val3.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
		_currentTournamentAgents.Add(val3);
		if (val3.HasMount)
		{
			_currentTournamentMountAgents.Add(val3.MountAgent);
		}
	}

	private void AddRandomClothes(CultureObject culture, TournamentParticipant participant)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Equipment participantArmor = Campaign.Current.Models.TournamentModel.GetParticipantArmor(participant.Character);
		for (int i = 5; i < 10; i++)
		{
			EquipmentElement equipmentFromSlot = participantArmor.GetEquipmentFromSlot((EquipmentIndex)i);
			if (((EquipmentElement)(ref equipmentFromSlot)).Item != null)
			{
				participant.MatchEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
			}
		}
	}

	private bool CheckIfTeamIsDead(TournamentTeam affectedParticipantTeam)
	{
		bool result = true;
		foreach (TournamentParticipant aliveParticipant in _aliveParticipants)
		{
			if (aliveParticipant.Team == affectedParticipantTeam)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void AddScoreToRemainingTeams()
	{
		foreach (TournamentTeam aliveTeam in _aliveTeams)
		{
			foreach (TournamentParticipant participant in aliveTeam.Participants)
			{
				participant.AddScore(1);
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (!IsMatchEnded() && affectedAgent.IsHuman)
		{
			TournamentParticipant participant = _match.GetParticipant(affectedAgent.Origin.UniqueSeed);
			_aliveParticipants.Remove(participant);
			_currentTournamentAgents.Remove(affectedAgent);
			if (CheckIfTeamIsDead(participant.Team))
			{
				_aliveTeams.Remove(participant.Team);
				AddScoreToRemainingTeams();
			}
		}
	}

	public bool CanAgentRout(Agent agent)
	{
		return false;
	}

	public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (affectorAgent == null)
		{
			return;
		}
		if (affectorAgent.IsMount && affectorAgent.RiderAgent != null)
		{
			affectorAgent = affectorAgent.RiderAgent;
		}
		if (affectorAgent.Character != null && affectedAgent.Character != null)
		{
			float num = blow.InflictedDamage;
			if (num > affectedAgent.HealthLimit)
			{
				num = affectedAgent.HealthLimit;
			}
			float num2 = num / affectedAgent.HealthLimit;
			EnemyHitReward(affectedAgent, affectorAgent, blow.MovementSpeedDamageModifier, shotDifficulty, attackerWeapon, blow.AttackType, 0.5f * num2, num, collisionData.IsSneakAttack);
		}
	}

	private void EnemyHitReward(Agent affectedAgent, Agent affectorAgent, float lastSpeedBonus, float lastShotDifficulty, WeaponComponentData lastAttackerWeapon, AgentAttackType attackType, float hitpointRatio, float damageAmount, bool isSneakAttack)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		CharacterObject val = (CharacterObject)affectedAgent.Character;
		CharacterObject val2 = (CharacterObject)affectorAgent.Character;
		if (affectedAgent.Origin != null && affectorAgent != null && affectorAgent.Origin != null)
		{
			bool flag = affectorAgent.MountAgent != null && (int)attackType == 3;
			SkillLevelingManager.OnCombatHit(val2, val, (CharacterObject)null, (Hero)null, lastSpeedBonus, lastShotDifficulty, lastAttackerWeapon, hitpointRatio, (MissionTypeEnum)2, affectorAgent.MountAgent != null, affectorAgent.Team == affectedAgent.Team, false, damageAmount, affectedAgent.Health < 1f, false, flag, isSneakAttack);
		}
	}

	public bool CheckIfIsThereAnyEnemies()
	{
		Team val = null;
		foreach (Agent currentTournamentAgent in _currentTournamentAgents)
		{
			if (currentTournamentAgent.IsHuman && currentTournamentAgent.IsActive() && currentTournamentAgent.Team != null)
			{
				if (val == null)
				{
					val = currentTournamentAgent.Team;
				}
				else if (val != currentTournamentAgent.Team)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Simulate()
	{
		_isSimulated = false;
		if (_currentTournamentAgents.Count == 0)
		{
			_aliveParticipants = _match.Participants.ToList();
			_aliveTeams = _match.Teams.ToList();
		}
		TournamentParticipant val = ((IEnumerable<TournamentParticipant>)_aliveParticipants).FirstOrDefault((Func<TournamentParticipant, bool>)((TournamentParticipant x) => x.Character == CharacterObject.PlayerCharacter));
		if (val != null)
		{
			TournamentTeam team = val.Team;
			foreach (TournamentParticipant participant in team.Participants)
			{
				participant.ResetScore();
				_aliveParticipants.Remove(participant);
			}
			_aliveTeams.Remove(team);
			AddScoreToRemainingTeams();
		}
		Dictionary<TournamentParticipant, Tuple<float, float>> dictionary = new Dictionary<TournamentParticipant, Tuple<float, float>>();
		float item = default(float);
		float item2 = default(float);
		foreach (TournamentParticipant aliveParticipant in _aliveParticipants)
		{
			aliveParticipant.Character.GetSimulationAttackPower(ref item, ref item2, aliveParticipant.MatchEquipment);
			dictionary.Add(aliveParticipant, new Tuple<float, float>(item, item2));
		}
		int num = 0;
		while (_aliveParticipants.Count > 1 && _aliveTeams.Count > 1)
		{
			num++;
			num %= _aliveParticipants.Count;
			TournamentParticipant val2 = _aliveParticipants[num];
			int num2;
			TournamentParticipant val3;
			do
			{
				num2 = MBRandom.RandomInt(_aliveParticipants.Count);
				val3 = _aliveParticipants[num2];
			}
			while (val2 == val3 || val2.Team == val3.Team);
			if (dictionary[val3].Item2 - dictionary[val2].Item1 > 0f)
			{
				dictionary[val3] = new Tuple<float, float>(dictionary[val3].Item1, dictionary[val3].Item2 - dictionary[val2].Item1);
				continue;
			}
			dictionary.Remove(val3);
			_aliveParticipants.Remove(val3);
			if (CheckIfTeamIsDead(val3.Team))
			{
				_aliveTeams.Remove(val3.Team);
				AddScoreToRemainingTeams();
			}
			if (num2 < num)
			{
				num--;
			}
		}
		_isSimulated = true;
	}

	private bool IsThereAnyPlayerAgent()
	{
		if (((MissionBehavior)this).Mission.MainAgent != null && ((MissionBehavior)this).Mission.MainAgent.IsActive())
		{
			return true;
		}
		return _currentTournamentAgents.Any((Agent agent) => agent.IsPlayerControlled);
	}

	private void SkipMatch()
	{
		Mission.Current.GetMissionBehavior<TournamentBehavior>().SkipMatch();
	}

	public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
	{
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		InquiryData result = null;
		canPlayerLeave = true;
		if (_match != null)
		{
			if (_match.IsPlayerParticipating())
			{
				MBTextManager.SetTextVariable("SETTLEMENT_NAME", Hero.MainHero.CurrentSettlement.EncyclopediaLinkWithName, false);
				if (IsThereAnyPlayerAgent())
				{
					if (((MissionBehavior)this).Mission.IsPlayerCloseToAnEnemy(5f))
					{
						canPlayerLeave = false;
						MBInformationManager.AddQuickInformation(GameTexts.FindText("str_can_not_retreat", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					}
					else if (CheckIfIsThereAnyEnemies())
					{
						result = new InquiryData(((object)GameTexts.FindText("str_tournament", (string)null)).ToString(), ((object)GameTexts.FindText("str_tournament_forfeit_game", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)SkipMatch, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
					}
					else
					{
						_forceEndMatch = true;
						canPlayerLeave = false;
					}
				}
				else if (CheckIfIsThereAnyEnemies())
				{
					result = new InquiryData(((object)GameTexts.FindText("str_tournament", (string)null)).ToString(), ((object)GameTexts.FindText("str_tournament_skip", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)SkipMatch, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
				}
				else
				{
					_forceEndMatch = true;
					canPlayerLeave = false;
				}
			}
			else if (CheckIfIsThereAnyEnemies())
			{
				result = new InquiryData(((object)GameTexts.FindText("str_tournament", (string)null)).ToString(), ((object)GameTexts.FindText("str_tournament_skip", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)SkipMatch, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
			}
			else
			{
				_forceEndMatch = true;
				canPlayerLeave = false;
			}
		}
		return result;
	}
}
