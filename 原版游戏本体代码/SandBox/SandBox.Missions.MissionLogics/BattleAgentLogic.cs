using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class BattleAgentLogic : MissionLogic
{
	private BattleObserverMissionLogic _battleObserverMissionLogic;

	private const float XpShareForKill = 0.5f;

	private const float XpShareForDamage = 0.5f;

	private MissionTime _nextMoraleCheckTime;

	private TroopUpgradeTracker _troopUpgradeTracker => MapEvent.PlayerMapEvent.TroopUpgradeTracker;

	public override void AfterStart()
	{
		_battleObserverMissionLogic = Mission.Current.GetMissionBehavior<BattleObserverMissionLogic>();
		CheckPerkEffectsOnTeams();
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		if (_battleObserverMissionLogic == null || agent.Character == null || agent.Origin == null)
		{
			return;
		}
		PartyBase val = (PartyBase)agent.Origin.BattleCombatant;
		CharacterObject val2 = (CharacterObject)agent.Character;
		if (val != null)
		{
			TroopUpgradeTracker troopUpgradeTracker = _troopUpgradeTracker;
			if (troopUpgradeTracker != null)
			{
				troopUpgradeTracker.AddTrackedTroop(val, val2);
			}
		}
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (affectedAgent.Character != null && affectorAgent != null && affectorAgent.Character != null && (int)affectedAgent.State == 1 && affectorAgent != null)
		{
			bool flag = affectedAgent.Health - (float)blow.InflictedDamage < 1f;
			bool flag2 = false;
			if (affectedAgent.Team != null && affectorAgent.Team != null)
			{
				flag2 = affectedAgent.Team.Side == affectorAgent.Team.Side;
			}
			IAgentOriginBase origin = affectorAgent.Origin;
			BasicCharacterObject character = affectedAgent.Character;
			Formation formation = affectorAgent.Formation;
			object obj;
			if (formation == null)
			{
				obj = null;
			}
			else
			{
				Agent captain = formation.Captain;
				obj = ((captain != null) ? captain.Character : null);
			}
			int inflictedDamage = blow.InflictedDamage;
			bool num = flag2;
			MissionWeapon val = attackerWeapon;
			origin.OnScoreHit(character, (BasicCharacterObject)obj, inflictedDamage, flag, num, ((MissionWeapon)(ref val)).CurrentUsageItem);
		}
	}

	public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		if (prevTeam == null || prevTeam == Team.Invalid || newTeam == null || prevTeam == newTeam)
		{
			return;
		}
		BattleObserverMissionLogic battleObserverMissionLogic = _battleObserverMissionLogic;
		if (battleObserverMissionLogic != null)
		{
			IBattleObserver battleObserver = battleObserverMissionLogic.BattleObserver;
			if (battleObserver != null)
			{
				battleObserver.TroopSideChanged((BattleSideEnum)((prevTeam == null) ? (-1) : ((int)prevTeam.Side)), (BattleSideEnum)((newTeam == null) ? (-1) : ((int)newTeam.Side)), (IBattleCombatant)(PartyBase)agent.Origin.BattleCombatant, agent.Character);
			}
		}
	}

	public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
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
			EnemyHitReward(affectedAgent, affectorAgent, blow.MovementSpeedDamageModifier, shotDifficulty, isSiegeEngineHit, attackerWeapon, blow.AttackType, 0.5f * num2, num, collisionData.IsSneakAttack);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (affectorAgent == null && affectedAgent.IsMount && (int)agentState == 2)
		{
			return;
		}
		CharacterObject val = (CharacterObject)affectedAgent.Character;
		CharacterObject val2 = (CharacterObject)((affectorAgent != null) ? affectorAgent.Character : null);
		if (affectedAgent.Origin == null)
		{
			return;
		}
		PartyBase val3 = (PartyBase)affectedAgent.Origin.BattleCombatant;
		if ((int)agentState == 3)
		{
			affectedAgent.Origin.SetWounded();
		}
		else if ((int)agentState == 4)
		{
			affectedAgent.Origin.SetKilled();
			Hero val4 = (affectedAgent.IsHuman ? val.HeroObject : null);
			Hero val5 = ((affectorAgent == null) ? null : (affectorAgent.IsHuman ? val2.HeroObject : null));
			if (val4 != null && val5 != null)
			{
				((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnCharacterDefeated(val5, val4);
			}
			if (val3 != null)
			{
				CheckUpgrade(affectedAgent.Team.Side, val3, val);
			}
		}
		else
		{
			bool flag = AgentComponentExtensions.GetMorale(affectedAgent) < 0.01f;
			affectedAgent.Origin.SetRouted(!flag);
		}
	}

	public override void OnAgentFleeing(Agent affectedAgent)
	{
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		UpdateMorale();
		if (((MissionTime)(ref _nextMoraleCheckTime)).IsPast)
		{
			_nextMoraleCheckTime = MissionTime.SecondsFromNow(10f);
		}
	}

	private void CheckPerkEffectsOnTeams()
	{
	}

	private void UpdateMorale()
	{
	}

	private void EnemyHitReward(Agent affectedAgent, Agent affectorAgent, float lastSpeedBonus, float lastShotDifficulty, bool isSiegeEngineHit, WeaponComponentData lastAttackerWeapon, AgentAttackType attackType, float hitpointRatio, float damageAmount, bool isSneakAttack)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Invalid comparison between Unknown and I4
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject val = (CharacterObject)affectedAgent.Character;
		CharacterObject val2 = (CharacterObject)affectorAgent.Character;
		if (affectedAgent.Origin == null || affectorAgent == null || affectorAgent.Origin == null || affectorAgent.Team == null || !affectorAgent.Team.IsValid || affectedAgent.Team == null || !affectedAgent.Team.IsValid)
		{
			return;
		}
		PartyBase val3 = (PartyBase)affectorAgent.Origin.BattleCombatant;
		Hero captain = GetCaptain(affectorAgent);
		Hero val4 = ((affectorAgent.Team.Leader != null && affectorAgent.Team.Leader.Character.IsHero) ? ((CharacterObject)affectorAgent.Team.Leader.Character).HeroObject : null);
		bool flag = affectorAgent.Team.Side == affectedAgent.Team.Side;
		bool flag2 = affectorAgent.MountAgent != null;
		bool flag3 = flag2 && (int)attackType == 3;
		SkillLevelingManager.OnCombatHit(val2, val, (captain != null) ? captain.CharacterObject : null, val4, lastSpeedBonus, lastShotDifficulty, lastAttackerWeapon, hitpointRatio, (MissionTypeEnum)0, flag2, flag, val4 != null && (object)affectorAgent.Character != val4.CharacterObject && (val4 != Hero.MainHero || affectorAgent.Formation == null || !affectorAgent.Formation.IsAIControlled), damageAmount, affectedAgent.Health < 1f, isSiegeEngineHit, flag3, isSneakAttack);
		BattleObserverMissionLogic battleObserverMissionLogic = _battleObserverMissionLogic;
		if (((battleObserverMissionLogic != null) ? battleObserverMissionLogic.BattleObserver : null) == null || affectorAgent.Character == null)
		{
			return;
		}
		if (affectorAgent.Character.IsHero)
		{
			Hero heroObject = val2.HeroObject;
			{
				foreach (SkillObject item in _troopUpgradeTracker.CheckSkillUpgrades(heroObject))
				{
					_battleObserverMissionLogic.BattleObserver.HeroSkillIncreased(affectorAgent.Team.Side, (IBattleCombatant)(object)val3, (BasicCharacterObject)(object)val2, item);
				}
				return;
			}
		}
		CheckUpgrade(affectorAgent.Team.Side, val3, val2);
	}

	private static Hero GetCaptain(Agent affectorAgent)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Hero result = null;
		if (affectorAgent.Formation != null)
		{
			Agent captain = affectorAgent.Formation.Captain;
			if (captain != null)
			{
				float captainRadius = Campaign.Current.Models.CombatXpModel.CaptainRadius;
				Vec3 position = captain.Position;
				if (((Vec3)(ref position)).Distance(affectorAgent.Position) < captainRadius)
				{
					result = ((CharacterObject)captain.Character).HeroObject;
				}
			}
		}
		return result;
	}

	private void CheckUpgrade(BattleSideEnum side, PartyBase party, CharacterObject character)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		BattleObserverMissionLogic battleObserverMissionLogic = _battleObserverMissionLogic;
		if (((battleObserverMissionLogic != null) ? battleObserverMissionLogic.BattleObserver : null) != null)
		{
			int num = _troopUpgradeTracker.CheckUpgradedCount(party, character);
			if (num != 0)
			{
				_battleObserverMissionLogic.BattleObserver.TroopNumberChanged(side, (IBattleCombatant)(object)party, (BasicCharacterObject)(object)character, 0, 0, 0, 0, 0, num);
			}
		}
	}
}
