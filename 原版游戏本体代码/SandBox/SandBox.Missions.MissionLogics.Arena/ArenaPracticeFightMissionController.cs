using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Conversation.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionLogics.Arena;

public class ArenaPracticeFightMissionController : MissionLogic
{
	private const int AIParticipantCount = 30;

	private const int MaxAliveAgentCount = 6;

	private const int MaxSpawnInterval = 14;

	private const int MinSpawnDistanceSquared = 144;

	private const int TotalStageCount = 3;

	private const int PracticeFightTroopTierLimit = 3;

	public int TeleportTime = 5;

	private Settlement _settlement;

	private int _spawnedOpponentAgentCount;

	private int _aliveOpponentCount;

	private float _nextSpawnTime;

	private List<MatrixFrame> _initialSpawnFrames;

	private List<MatrixFrame> _spawnFrames;

	private List<Team> _AIParticipantTeams;

	private List<Agent> _participantAgents;

	private Team _tournamentMasterTeam;

	private BasicMissionTimer _teleportTimer;

	private List<CharacterObject> _participantCharacters;

	private const float XpShareForKill = 0.5f;

	private const float XpShareForDamage = 0.5f;

	private int AISpawnIndex => _spawnedOpponentAgentCount;

	public int RemainingOpponentCountFromLastPractice { get; private set; }

	public bool IsPlayerPracticing { get; private set; }

	public int OpponentCountBeatenByPlayer { get; private set; }

	public int RemainingOpponentCount => 30 - _spawnedOpponentAgentCount + _aliveOpponentCount;

	public bool IsPlayerSurvived { get; private set; }

	public bool AfterPractice { get; set; }

	public override void AfterStart()
	{
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		_settlement = PlayerEncounter.LocationEncounter.Settlement;
		InitializeTeams();
		GameEntity item = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("tournament_practice") ?? ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("tournament_fight");
		List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTag("arena_set").ToList();
		list.Remove(item);
		foreach (GameEntity item2 in list)
		{
			item2.Remove(88);
		}
		_initialSpawnFrames = (from e in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_arena")
			select e.GetGlobalFrame()).ToList();
		_spawnFrames = (from e in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_arena_respawn")
			select e.GetGlobalFrame()).ToList();
		for (int num = 0; num < _initialSpawnFrames.Count; num++)
		{
			MatrixFrame value = _initialSpawnFrames[num];
			((Mat3)(ref value.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			_initialSpawnFrames[num] = value;
		}
		for (int num2 = 0; num2 < _spawnFrames.Count; num2++)
		{
			MatrixFrame value2 = _spawnFrames[num2];
			((Mat3)(ref value2.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			_spawnFrames[num2] = value2;
		}
		IsPlayerPracticing = false;
		_participantAgents = new List<Agent>();
		StartPractice();
		MissionAgentHandler missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionAgentHandler>();
		SandBoxHelpers.MissionHelper.SpawnPlayer(civilianEquipment: true, noHorses: true);
		missionBehavior.SpawnLocationCharacters();
	}

	private void SpawnPlayerNearTournamentMaster()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_player_near_arena_master");
		((MissionBehavior)this).Mission.SpawnAgent(new AgentBuildData((BasicCharacterObject)(object)CharacterObject.PlayerCharacter).Team(((MissionBehavior)this).Mission.PlayerTeam).InitialFrameFromSpawnPointEntity(val).NoHorses(true)
			.CivilianEquipment(true)
			.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)CharacterObject.PlayerCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)))
			.Controller((AgentControllerType)2), false);
		Mission.Current.SetMissionMode((MissionMode)0, false);
	}

	private Agent SpawnArenaAgent(Team team, MatrixFrame frame)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		CharacterObject val;
		int spawnIndex;
		if (team == ((MissionBehavior)this).Mission.PlayerTeam)
		{
			val = CharacterObject.PlayerCharacter;
			spawnIndex = 0;
		}
		else
		{
			val = _participantCharacters[AISpawnIndex];
			spawnIndex = AISpawnIndex;
		}
		Equipment val2 = new Equipment();
		AddRandomWeapons(val2, spawnIndex);
		AddRandomClothes(val, val2);
		Mission mission = ((MissionBehavior)this).Mission;
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).Team(team).InitialPosition(ref frame.origin);
		Vec2 val3 = ((Vec3)(ref frame.rotation.f)).AsVec2;
		val3 = ((Vec2)(ref val3)).Normalized();
		Agent val4 = mission.SpawnAgent(obj.InitialDirection(ref val3).NoHorses(true).Equipment(val2)
			.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor)))
			.Controller((AgentControllerType)((val != CharacterObject.PlayerCharacter) ? 1 : 2)), false);
		val4.FadeIn();
		if (val != CharacterObject.PlayerCharacter)
		{
			_aliveOpponentCount++;
			_spawnedOpponentAgentCount++;
		}
		if (val4.IsAIControlled)
		{
			val4.SetWatchState((WatchState)2);
		}
		return val4;
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

	private void EnemyHitReward(Agent affectedAgent, Agent affectorAgent, float lastSpeedBonus, float lastShotDifficulty, WeaponComponentData attackerWeapon, AgentAttackType attackType, float hitpointRatio, float damageAmount, bool isSneakAttack)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		CharacterObject val = (CharacterObject)affectedAgent.Character;
		CharacterObject val2 = (CharacterObject)affectorAgent.Character;
		if (affectedAgent.Origin != null && affectorAgent != null && affectorAgent.Origin != null)
		{
			bool flag = affectorAgent.MountAgent != null;
			bool flag2 = flag && (int)attackType == 3;
			SkillLevelingManager.OnCombatHit(val2, val, (CharacterObject)null, (Hero)null, lastSpeedBonus, lastShotDifficulty, attackerWeapon, hitpointRatio, (MissionTypeEnum)1, flag, affectorAgent.Team == affectedAgent.Team, false, damageAmount, affectedAgent.Health < 1f, false, flag2, isSneakAttack);
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionTick(dt);
		if (_aliveOpponentCount < 6 && _spawnedOpponentAgentCount < 30 && (_aliveOpponentCount == 2 || _nextSpawnTime < ((MissionBehavior)this).Mission.CurrentTime))
		{
			Team team = SelectRandomAiTeam();
			Agent item = SpawnArenaAgent(team, GetSpawnFrame(considerPlayerDistance: true, isInitialSpawn: false));
			_participantAgents.Add(item);
			_nextSpawnTime = ((MissionBehavior)this).Mission.CurrentTime + 14f - (float)_spawnedOpponentAgentCount / 3f;
			if (_spawnedOpponentAgentCount == 30 && !IsPlayerPracticing)
			{
				_spawnedOpponentAgentCount = 0;
			}
		}
		if (_teleportTimer == null && IsPlayerPracticing && CheckPracticeEndedForPlayer())
		{
			_teleportTimer = new BasicMissionTimer();
			IsPlayerSurvived = ((MissionBehavior)this).Mission.MainAgent != null && ((MissionBehavior)this).Mission.MainAgent.IsActive();
			if (IsPlayerSurvived)
			{
				MBInformationManager.AddQuickInformation(new TextObject("{=seyti8xR}Victory!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/mission/arena_victory");
			}
			AfterPractice = true;
		}
		if (_teleportTimer != null && _teleportTimer.ElapsedTime > (float)TeleportTime)
		{
			_teleportTimer = null;
			RemainingOpponentCountFromLastPractice = RemainingOpponentCount;
			IsPlayerPracticing = false;
			StartPractice();
			SpawnPlayerNearTournamentMaster();
			Agent agent = ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => x.Character != null && (int)((CharacterObject)x.Character).Occupation == 5));
			MissionConversationLogic.Current.StartConversation(agent, setActionsInstantly: true);
		}
	}

	private Team SelectRandomAiTeam()
	{
		Team val = null;
		foreach (Team aIParticipantTeam in _AIParticipantTeams)
		{
			if (!aIParticipantTeam.HasBots)
			{
				val = aIParticipantTeam;
				break;
			}
		}
		if (val == null)
		{
			val = _AIParticipantTeams[MBRandom.RandomInt(_AIParticipantTeams.Count - 1) + 1];
		}
		return val;
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (affectedAgent != null && affectedAgent.IsHuman)
		{
			if (affectedAgent != Agent.Main)
			{
				_aliveOpponentCount--;
			}
			if (affectorAgent != null && affectorAgent.IsHuman && affectorAgent == Agent.Main && affectedAgent != Agent.Main)
			{
				OpponentCountBeatenByPlayer++;
			}
		}
		if (_participantAgents.Contains(affectedAgent))
		{
			_participantAgents.Remove(affectedAgent);
		}
	}

	public override bool MissionEnded(ref MissionResult missionResult)
	{
		return false;
	}

	public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		canPlayerLeave = true;
		if (!IsPlayerPracticing)
		{
			return null;
		}
		return new InquiryData(((object)new TextObject("{=zv49qE35}Practice Fight", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("str_give_up_fight", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), ((object)GameTexts.FindText("str_cancel", (string)null)).ToString(), (Action)((MissionBehavior)this).Mission.OnEndMissionResult, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
	}

	public void StartPlayerPractice()
	{
		IsPlayerPracticing = true;
		AfterPractice = false;
		StartPractice();
	}

	private void StartPractice()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		InitializeParticipantCharacters();
		SandBoxHelpers.MissionHelper.FadeOutAgents(((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Where((Agent agent) => _participantAgents.Contains(agent) || agent.IsMount || agent.IsPlayerControlled), hideInstantly: true, hideMount: false);
		_spawnedOpponentAgentCount = 0;
		_aliveOpponentCount = 0;
		_participantAgents.Clear();
		Mission.Current.ClearCorpses(false);
		((MissionBehavior)this).Mission.RemoveSpawnedItemsAndMissiles();
		ArrangePlayerTeamEnmity();
		if (IsPlayerPracticing)
		{
			Agent val = SpawnArenaAgent(((MissionBehavior)this).Mission.PlayerTeam, GetSpawnFrame(considerPlayerDistance: false, isInitialSpawn: true));
			val.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
			OpponentCountBeatenByPlayer = 0;
			_participantAgents.Add(val);
		}
		int count = _AIParticipantTeams.Count;
		int num = 0;
		while (_spawnedOpponentAgentCount < 6)
		{
			_participantAgents.Add(SpawnArenaAgent(_AIParticipantTeams[num % count], GetSpawnFrame(considerPlayerDistance: false, isInitialSpawn: true)));
			num++;
		}
		_nextSpawnTime = ((MissionBehavior)this).Mission.CurrentTime + 14f;
	}

	private bool CheckPracticeEndedForPlayer()
	{
		if (((MissionBehavior)this).Mission.MainAgent != null && ((MissionBehavior)this).Mission.MainAgent.IsActive())
		{
			return RemainingOpponentCount == 0;
		}
		return true;
	}

	private void AddRandomWeapons(Equipment equipment, int spawnIndex)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		int num = 1 + spawnIndex * 3 / 30;
		List<Equipment> list = ((BasicCharacterObject)(Game.Current.ObjectManager.GetObject<CharacterObject>("weapon_practice_stage_" + num + "_" + ((MBObjectBase)_settlement.MapFaction.Culture).StringId) ?? Game.Current.ObjectManager.GetObject<CharacterObject>("weapon_practice_stage_" + num + "_empire"))).BattleEquipments.ToList();
		int index = MBRandom.RandomInt(list.Count);
		for (int i = 0; i <= 3; i++)
		{
			EquipmentElement equipmentFromSlot = list[index].GetEquipmentFromSlot((EquipmentIndex)i);
			if (((EquipmentElement)(ref equipmentFromSlot)).Item != null)
			{
				equipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
			}
		}
	}

	private void AddRandomClothes(CharacterObject troop, Equipment equipment)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Equipment participantArmor = Campaign.Current.Models.TournamentModel.GetParticipantArmor(troop);
		for (int i = 0; i < 12; i++)
		{
			if (i > 4 && i != 10 && i != 11)
			{
				EquipmentElement equipmentFromSlot = participantArmor.GetEquipmentFromSlot((EquipmentIndex)i);
				if (((EquipmentElement)(ref equipmentFromSlot)).Item != null)
				{
					equipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
				}
			}
		}
	}

	private void InitializeTeams()
	{
		_AIParticipantTeams = new List<Team>();
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, (Banner)null, true, false, true);
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.DefenderTeam;
		_tournamentMasterTeam = ((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)(-1), _settlement.MapFaction.Color, _settlement.MapFaction.Color2, (Banner)null, true, false, true);
		while (_AIParticipantTeams.Count < 6)
		{
			_AIParticipantTeams.Add(((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, uint.MaxValue, uint.MaxValue, (Banner)null, true, false, true));
		}
		for (int i = 0; i < _AIParticipantTeams.Count; i++)
		{
			_AIParticipantTeams[i].SetIsEnemyOf(_tournamentMasterTeam, false);
			for (int j = i + 1; j < _AIParticipantTeams.Count; j++)
			{
				_AIParticipantTeams[i].SetIsEnemyOf(_AIParticipantTeams[j], true);
			}
		}
	}

	private void InitializeParticipantCharacters()
	{
		List<CharacterObject> participantCharacters = GetParticipantCharacters(_settlement);
		_participantCharacters = participantCharacters.OrderBy((CharacterObject x) => ((BasicCharacterObject)x).Level).ToList();
	}

	public static List<CharacterObject> GetParticipantCharacters(Settlement settlement)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		int num = 30;
		List<CharacterObject> list = new List<CharacterObject>();
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		if (list.Count < num && ((Fief)settlement.Town).GarrisonParty != null)
		{
			foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)((Fief)settlement.Town).GarrisonParty.MemberRoster.GetTroopRoster())
			{
				int num5 = num - list.Count;
				if (!list.Contains(item.Character) && item.Character.Tier == 3 && (float)num5 * 0.4f > (float)num2)
				{
					list.Add(item.Character);
					num2++;
				}
				else if (!list.Contains(item.Character) && item.Character.Tier == 4 && (float)num5 * 0.4f > (float)num3)
				{
					list.Add(item.Character);
					num3++;
				}
				else if (!list.Contains(item.Character) && item.Character.Tier == 5 && (float)num5 * 0.2f > (float)num4)
				{
					list.Add(item.Character);
					num4++;
				}
				if (list.Count >= num)
				{
					break;
				}
			}
		}
		if (list.Count < num)
		{
			List<CharacterObject> list2 = new List<CharacterObject>();
			GetUpgradeTargets(((settlement != null) ? settlement.Culture : Game.Current.ObjectManager.GetObject<CultureObject>("empire")).BasicTroop, ref list2);
			int num6 = num - list.Count;
			foreach (CharacterObject item2 in list2)
			{
				if (!list.Contains(item2) && item2.Tier == 3 && (float)num6 * 0.4f > (float)num2)
				{
					list.Add(item2);
					num2++;
				}
				else if (!list.Contains(item2) && item2.Tier == 4 && (float)num6 * 0.4f > (float)num3)
				{
					list.Add(item2);
					num3++;
				}
				else if (!list.Contains(item2) && item2.Tier == 5 && (float)num6 * 0.2f > (float)num4)
				{
					list.Add(item2);
					num4++;
				}
				if (list.Count >= num)
				{
					break;
				}
			}
			while (list.Count < num)
			{
				for (int i = 0; i < list2.Count; i++)
				{
					if (list.Count >= num)
					{
						break;
					}
					list.Add(list2[i]);
				}
			}
		}
		return list;
	}

	private static void GetUpgradeTargets(CharacterObject troop, ref List<CharacterObject> list)
	{
		if (!list.Contains(troop) && troop.Tier >= 3)
		{
			list.Add(troop);
		}
		CharacterObject[] upgradeTargets = troop.UpgradeTargets;
		for (int i = 0; i < upgradeTargets.Length; i++)
		{
			GetUpgradeTargets(upgradeTargets[i], ref list);
		}
	}

	private void ArrangePlayerTeamEnmity()
	{
		foreach (Team aIParticipantTeam in _AIParticipantTeams)
		{
			aIParticipantTeam.SetIsEnemyOf(((MissionBehavior)this).Mission.PlayerTeam, IsPlayerPracticing);
		}
	}

	private Team GetStrongestTeamExceptPlayerTeam()
	{
		Team result = null;
		int num = -1;
		foreach (Team aIParticipantTeam in _AIParticipantTeams)
		{
			int num2 = CalculateTeamPower(aIParticipantTeam);
			if (num2 > num)
			{
				result = aIParticipantTeam;
				num = num2;
			}
		}
		return result;
	}

	private int CalculateTeamPower(Team team)
	{
		int num = 0;
		foreach (Agent item in (List<Agent>)(object)team.ActiveAgents)
		{
			num += item.Character.Level * item.KillCount + (int)MathF.Sqrt(item.Health);
		}
		return num;
	}

	private MatrixFrame GetSpawnFrame(bool considerPlayerDistance, bool isInitialSpawn)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		List<MatrixFrame> list = ((isInitialSpawn || Extensions.IsEmpty<MatrixFrame>((IEnumerable<MatrixFrame>)_spawnFrames)) ? _initialSpawnFrames : _spawnFrames);
		if (list.Count == 1)
		{
			Debug.FailedAssert("Spawn point count is wrong! Arena practice spawn point set should be used in arena scenes.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\Arena\\ArenaPracticeFightMissionController.cs", "GetSpawnFrame", 616);
			return list[0];
		}
		MatrixFrame result;
		if (considerPlayerDistance && Agent.Main != null && Agent.Main.IsActive())
		{
			int num = MBRandom.RandomInt(list.Count);
			result = list[num];
			float num2 = float.MinValue;
			for (int i = num + 1; i < num + list.Count; i++)
			{
				MatrixFrame val = list[i % list.Count];
				float num3 = CalculateLocationScore(val);
				if (num3 >= 100f)
				{
					result = val;
					break;
				}
				if (num3 > num2)
				{
					result = val;
					num2 = num3;
				}
			}
		}
		else
		{
			int num4 = _spawnedOpponentAgentCount;
			if (IsPlayerPracticing && Agent.Main != null)
			{
				num4++;
			}
			result = list[num4 % list.Count];
		}
		return result;
	}

	private float CalculateLocationScore(MatrixFrame matrixFrame)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		float num = 100f;
		float num2 = 0.25f;
		float num3 = 0.75f;
		if (((Vec3)(ref matrixFrame.origin)).DistanceSquared(Agent.Main.Position) < 144f)
		{
			num *= num2;
		}
		for (int i = 0; i < _participantAgents.Count; i++)
		{
			Vec3 position = _participantAgents[i].Position;
			if (((Vec3)(ref position)).DistanceSquared(matrixFrame.origin) < 144f)
			{
				num *= num3;
			}
		}
		return num;
	}
}
