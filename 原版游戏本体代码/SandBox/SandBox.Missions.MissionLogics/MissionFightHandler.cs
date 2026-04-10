using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class MissionFightHandler : MissionLogic
{
	private enum State
	{
		NoFight,
		Fighting,
		FightEnded
	}

	public delegate void OnFightEndDelegate(bool isPlayerSideWon);

	private static OnFightEndDelegate _onFightEnd;

	private List<Agent> _playerSideAgents;

	private List<Agent> _opponentSideAgents;

	private Dictionary<Agent, Team> _playerSideAgentsOldTeamData;

	private Dictionary<Agent, Team> _opponentSideAgentsOldTeamData;

	private State _state;

	private BasicMissionTimer _finishTimer;

	private bool _isPlayerSideWon;

	private MissionMode _oldMissionMode;

	private MissionEquipment _playerEquipment;

	private MissionEquipment _opponentEquipment;

	private static MissionFightHandler _current => Mission.Current.GetMissionBehavior<MissionFightHandler>();

	public float MinMissionEndTime { get; private set; }

	public ReadOnlyCollection<Agent> PlayerSideAgents => _playerSideAgents.AsReadOnly();

	public ReadOnlyCollection<Agent> OpponentSideAgents => _opponentSideAgents.AsReadOnly();

	public bool IsPlayerSideWon => _isPlayerSideWon;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).Mission.IsAgentInteractionAllowed_AdditionalCondition += IsAgentInteractionAllowed_AdditionalCondition;
	}

	public override void EarlyStart()
	{
		_playerSideAgents = new List<Agent>();
		_opponentSideAgents = new List<Agent>();
	}

	public override void AfterStart()
	{
	}

	public override void OnMissionTick(float dt)
	{
		if (((MissionBehavior)this).Mission.CurrentTime > MinMissionEndTime && _finishTimer != null && _finishTimer.ElapsedTime > 5f)
		{
			_finishTimer = null;
			EndFight();
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		if (_state != State.Fighting)
		{
			return;
		}
		if (affectedAgent == Agent.Main)
		{
			Mission current = Mission.Current;
			current.NextCheckTimeEndMission += 8f;
		}
		if (affectorAgent != null && _playerSideAgents.Contains(affectedAgent))
		{
			_playerSideAgents.Remove(affectedAgent);
			if (_playerSideAgents.Count == 0)
			{
				_isPlayerSideWon = false;
				_finishTimer = new BasicMissionTimer();
			}
		}
		else if (affectorAgent != null && _opponentSideAgents.Contains(affectedAgent))
		{
			_opponentSideAgents.Remove(affectedAgent);
			if (_opponentSideAgents.Count == 0)
			{
				_isPlayerSideWon = true;
				_finishTimer = new BasicMissionTimer();
			}
		}
	}

	public void StartCustomFight(List<Agent> playerSideAgents, List<Agent> opponentSideAgents, bool dropWeapons, bool isItemUseDisabled, OnFightEndDelegate onFightEndDelegate, float minimumEndTime = float.Epsilon)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		StartFightInternal(playerSideAgents, opponentSideAgents, dropWeapons, isItemUseDisabled, onFightEndDelegate, minimumEndTime);
		SetTeamsForFightAndDuel();
		_oldMissionMode = Mission.Current.Mode;
		Mission.Current.SetMissionMode((MissionMode)2, false);
	}

	public void StartFistFight(Agent opponent, OnFightEndDelegate onFightEndDelegate, float minimumEndTime = float.Epsilon)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		StartFightInternal(new List<Agent> { Agent.Main }, new List<Agent> { opponent }, dropWeapons: false, isItemUseDisabled: false, delegate(bool playerWon)
		{
			AttachCachedEquipment(Agent.Main, opponent);
			onFightEndDelegate?.Invoke(playerWon);
		}, minimumEndTime);
		SetTeamsForFightAndDuel();
		_playerEquipment = new MissionEquipment();
		_opponentEquipment = new MissionEquipment();
		RemoveWeaponsFromAgents(Agent.Main, opponent);
		_oldMissionMode = Mission.Current.Mode;
		Mission.Current.SetMissionMode((MissionMode)2, false);
	}

	private void RemoveWeaponsFromAgents(Agent main, Agent opponent)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		_playerEquipment.FillFrom(main.Equipment);
		_opponentEquipment.FillFrom(opponent.Equipment);
		main.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
		main.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
		opponent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)1);
		opponent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)1);
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			main.RemoveEquippedWeapon(val);
			opponent.RemoveEquippedWeapon(val);
		}
	}

	private void AttachCachedEquipment(Agent main, Agent opponent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = _playerEquipment[val];
			main.EquipWeaponWithNewEntity(val, ref val2);
			MissionWeapon val3 = _opponentEquipment[val];
			opponent.EquipWeaponWithNewEntity(val, ref val3);
		}
		_playerEquipment = null;
		_opponentEquipment = null;
	}

	private void StartFightInternal(List<Agent> playerSideAgents, List<Agent> opponentSideAgents, bool dropWeapons, bool isItemUseDisabled, OnFightEndDelegate onFightEndDelegate, float minimumEndTime = float.Epsilon)
	{
		_state = State.Fighting;
		_opponentSideAgents = opponentSideAgents;
		_playerSideAgents = playerSideAgents;
		_playerSideAgentsOldTeamData = new Dictionary<Agent, Team>();
		_opponentSideAgentsOldTeamData = new Dictionary<Agent, Team>();
		_onFightEnd = onFightEndDelegate;
		_isPlayerSideWon = false;
		Mission.Current.MainAgent.IsItemUseDisabled = isItemUseDisabled;
		foreach (Agent opponentSideAgent in _opponentSideAgents)
		{
			if (dropWeapons)
			{
				DropAllWeapons(opponentSideAgent);
			}
			_opponentSideAgentsOldTeamData.Add(opponentSideAgent, opponentSideAgent.Team);
			ForceAgentForFight(opponentSideAgent);
		}
		foreach (Agent playerSideAgent in _playerSideAgents)
		{
			if (dropWeapons)
			{
				DropAllWeapons(playerSideAgent);
			}
			_playerSideAgentsOldTeamData.Add(playerSideAgent, playerSideAgent.Team);
			ForceAgentForFight(playerSideAgent);
		}
		if (minimumEndTime > 0f && !MBMath.ApproximatelyEqualsTo(minimumEndTime, float.Epsilon, 1E-05f))
		{
			MinMissionEndTime = ((MissionBehavior)this).Mission.CurrentTime + minimumEndTime;
		}
		else
		{
			MinMissionEndTime = 0f;
		}
	}

	public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		canPlayerLeave = true;
		if (_state == State.Fighting && (_opponentSideAgents.Count > 0 || _playerSideAgents.Count > 0))
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=Fpk3BUBs}Your fight has not ended yet!", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			canPlayerLeave = false;
		}
		return null;
	}

	private void ForceAgentForFight(Agent agent)
	{
		if (agent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
		{
			AlarmedBehaviorGroup behaviorGroup = agent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>();
			behaviorGroup.DisableCalmDown = true;
			behaviorGroup.AddBehavior<FightBehavior>();
			behaviorGroup.SetScriptedBehavior<FightBehavior>();
		}
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).Mission.IsAgentInteractionAllowed_AdditionalCondition -= IsAgentInteractionAllowed_AdditionalCondition;
	}

	private void SetTeamsForFightAndDuel()
	{
		Mission.Current.PlayerEnemyTeam.SetIsEnemyOf(Mission.Current.PlayerTeam, true);
		foreach (Agent playerSideAgent in _playerSideAgents)
		{
			if (playerSideAgent.IsHuman)
			{
				if (playerSideAgent.IsAIControlled)
				{
					playerSideAgent.SetWatchState((WatchState)2);
				}
				playerSideAgent.SetTeam(Mission.Current.PlayerTeam, true);
			}
		}
		foreach (Agent opponentSideAgent in _opponentSideAgents)
		{
			if (opponentSideAgent.IsHuman)
			{
				if (opponentSideAgent.IsAIControlled)
				{
					opponentSideAgent.SetWatchState((WatchState)2);
				}
				opponentSideAgent.SetTeam(Mission.Current.PlayerEnemyTeam, true);
			}
		}
	}

	private void ResetTeamsForFightAndDuel()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		foreach (Agent playerSideAgent in _playerSideAgents)
		{
			if (playerSideAgent.IsAIControlled)
			{
				playerSideAgent.ResetEnemyCaches();
				playerSideAgent.InvalidateTargetAgent();
				playerSideAgent.InvalidateAIWeaponSelections();
				playerSideAgent.SetWatchState((WatchState)0);
			}
			playerSideAgent.SetTeam(new Team(_playerSideAgentsOldTeamData[playerSideAgent].MBTeam, (BattleSideEnum)(-1), ((MissionBehavior)this).Mission, uint.MaxValue, uint.MaxValue, (Banner)null), true);
		}
		foreach (Agent opponentSideAgent in _opponentSideAgents)
		{
			if (opponentSideAgent.IsAIControlled)
			{
				opponentSideAgent.ResetEnemyCaches();
				opponentSideAgent.InvalidateTargetAgent();
				opponentSideAgent.InvalidateAIWeaponSelections();
				opponentSideAgent.SetWatchState((WatchState)0);
			}
			opponentSideAgent.SetTeam(new Team(_opponentSideAgentsOldTeamData[opponentSideAgent].MBTeam, (BattleSideEnum)(-1), ((MissionBehavior)this).Mission, uint.MaxValue, uint.MaxValue, (Banner)null), true);
		}
	}

	private bool IsAgentInteractionAllowed_AdditionalCondition()
	{
		return _state != State.Fighting;
	}

	public static Agent GetAgentToSpectate()
	{
		MissionFightHandler current = _current;
		if (current._playerSideAgents.Count > 0)
		{
			return current._playerSideAgents[0];
		}
		if (current._opponentSideAgents.Count > 0)
		{
			return current._opponentSideAgents[0];
		}
		return null;
	}

	private void DropAllWeapons(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = agent.Equipment[val];
			if (!((MissionWeapon)(ref val2)).IsEmpty)
			{
				agent.DropItem(val, (WeaponClass)0);
			}
		}
	}

	private void ResetScriptedBehaviors()
	{
		foreach (Agent playerSideAgent in _playerSideAgents)
		{
			if (playerSideAgent.IsActive() && playerSideAgent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
			{
				playerSideAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().DisableScriptedBehavior();
			}
		}
		foreach (Agent opponentSideAgent in _opponentSideAgents)
		{
			if (opponentSideAgent.IsActive() && opponentSideAgent.GetComponent<CampaignAgentComponent>().AgentNavigator != null)
			{
				opponentSideAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<AlarmedBehaviorGroup>().DisableScriptedBehavior();
			}
		}
	}

	public void BeginEndFight()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		_finishTimer = new BasicMissionTimer();
	}

	public void EndFight(bool overrideDuelWonByPlayer = false)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Invalid comparison between Unknown and I4
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		ResetScriptedBehaviors();
		ResetTeamsForFightAndDuel();
		_state = State.FightEnded;
		foreach (Agent playerSideAgent in _playerSideAgents)
		{
			if (playerSideAgent.IsActive())
			{
				playerSideAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)3);
				playerSideAgent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)3);
			}
		}
		foreach (Agent opponentSideAgent in _opponentSideAgents)
		{
			if (opponentSideAgent.IsActive())
			{
				opponentSideAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)3);
				opponentSideAgent.TryToSheathWeaponInHand((HandIndex)1, (WeaponWieldActionType)3);
			}
		}
		_playerSideAgents.Clear();
		_opponentSideAgents.Clear();
		if (Mission.Current.MainAgent != null)
		{
			Mission.Current.MainAgent.IsItemUseDisabled = false;
		}
		if ((int)_oldMissionMode == 1 && !Campaign.Current.ConversationManager.IsConversationFlowActive)
		{
			_oldMissionMode = (MissionMode)0;
		}
		Mission.Current.SetMissionMode(_oldMissionMode, false);
		if (_onFightEnd != null)
		{
			_onFightEnd(_isPlayerSideWon || overrideDuelWonByPlayer);
			_isPlayerSideWon = false;
			_onFightEnd = null;
		}
	}

	public bool IsThereActiveFight()
	{
		return _state == State.Fighting;
	}

	public void AddAgentToSide(Agent agent, bool isPlayerSide)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		if (IsThereActiveFight() && !_playerSideAgents.Contains(agent) && !_opponentSideAgents.Contains(agent))
		{
			if (agent.IsAIControlled)
			{
				agent.SetWatchState((WatchState)2);
			}
			if (isPlayerSide)
			{
				agent.SetTeam(Mission.Current.PlayerTeam, true);
				_playerSideAgents.Add(agent);
				_playerSideAgentsOldTeamData.Add(agent, agent.Team);
			}
			else
			{
				agent.SetTeam(Mission.Current.PlayerEnemyTeam, true);
				_opponentSideAgents.Add(agent);
				_opponentSideAgentsOldTeamData.Add(agent, agent.Team);
			}
			if (_playerSideAgents.Count == 0 || _opponentSideAgents.Count == 0)
			{
				_finishTimer = new BasicMissionTimer();
			}
			else
			{
				_finishTimer = null;
			}
			ForceAgentForFight(agent);
		}
	}

	public IEnumerable<Agent> GetDangerSources(Agent ownerAgent)
	{
		if (!(ownerAgent.Character is CharacterObject))
		{
			Debug.FailedAssert("false", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\MissionFightHandler.cs", "GetDangerSources", 469);
			return new List<Agent>();
		}
		if (IsThereActiveFight() && !IsAgentAggressive(ownerAgent) && Agent.Main != null)
		{
			return new List<Agent> { Agent.Main };
		}
		return new List<Agent>();
	}

	public static bool IsAgentAggressive(Agent agent)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		BasicCharacterObject character = agent.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		if (!agent.HasWeapon())
		{
			if (val != null)
			{
				if ((int)val.Occupation != 2 && !IsAgentVillian(val))
				{
					return IsAgentJusticeWarrior(val);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static bool IsAgentJusticeWarrior(CharacterObject character)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if ((int)character.Occupation != 7 && (int)character.Occupation != 24)
		{
			return (int)character.Occupation == 23;
		}
		return true;
	}

	public static bool IsAgentVillian(CharacterObject character)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if ((int)character.Occupation != 27 && (int)character.Occupation != 21)
		{
			return (int)character.Occupation == 15;
		}
		return true;
	}
}
