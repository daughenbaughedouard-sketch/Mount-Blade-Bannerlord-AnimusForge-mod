using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics.Towns;

public class AlleyFightMissionHandler : MissionLogic, IMissionAgentSpawnLogic, IMissionBehavior
{
	private TroopRoster _playerSideTroops;

	private TroopRoster _rivalSideTroops;

	private List<Agent> _playerSideAliveAgents = new List<Agent>();

	private List<Agent> _rivalSideAliveAgents = new List<Agent>();

	public AlleyFightMissionHandler(TroopRoster playerSideTroops, TroopRoster rivalSideTroops)
	{
		_playerSideTroops = playerSideTroops;
		_rivalSideTroops = rivalSideTroops;
	}

	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, Clan.PlayerClan.Color, Clan.PlayerClan.Color2, Clan.PlayerClan.Banner, true, false, true);
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, Clan.BanditFactions.First().Color, Clan.BanditFactions.First().Color2, Clan.BanditFactions.First().Banner, true, false, true);
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.DefenderTeam;
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
		if (_playerSideAliveAgents.Contains(affectedAgent))
		{
			_playerSideAliveAgents.Remove(affectedAgent);
			_003F val = _playerSideTroops;
			BasicCharacterObject character = affectedAgent.Character;
			((TroopRoster)val).RemoveTroop((CharacterObject)(object)((character is CharacterObject) ? character : null), 1, default(UniqueTroopDescriptor), 0);
		}
		else if (_rivalSideAliveAgents.Contains(affectedAgent))
		{
			_rivalSideAliveAgents.Remove(affectedAgent);
			_003F val2 = _rivalSideTroops;
			BasicCharacterObject character2 = affectedAgent.Character;
			((TroopRoster)val2).RemoveTroop((CharacterObject)(object)((character2 is CharacterObject) ? character2 : null), 1, default(UniqueTroopDescriptor), 0);
		}
		if (affectedAgent == Agent.Main)
		{
			Campaign.Current.GetCampaignBehavior<IAlleyCampaignBehavior>().OnPlayerDiedInMission();
		}
	}

	public override void AfterStart()
	{
		DefaultMissionDeploymentPlan val = default(DefaultMissionDeploymentPlan);
		((MissionBehavior)this).Mission.GetDeploymentPlan<DefaultMissionDeploymentPlan>(ref val);
		val.AddTroops(((MissionBehavior)this).Mission.DefenderTeam, (FormationClass)0, _playerSideTroops.TotalManCount, 0, false);
		val.AddTroops(((MissionBehavior)this).Mission.AttackerTeam, (FormationClass)0, _rivalSideTroops.TotalManCount, 0, false);
		((MissionBehavior)this).Mission.DeploymentPlan.MakeDefaultDeploymentPlans();
	}

	public override InquiryData OnEndMissionRequest(out bool canLeave)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		canLeave = true;
		return new InquiryData("", ((object)GameTexts.FindText("str_give_up_fight", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), ((object)GameTexts.FindText("str_cancel", (string)null)).ToString(), (Action)((MissionBehavior)this).Mission.OnEndMissionResult, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
	}

	public override void OnRetreatMission()
	{
		Campaign.Current.GetCampaignBehavior<IAlleyCampaignBehavior>().OnPlayerRetreatedFromMission();
	}

	public override void OnRenderingStarted()
	{
		Mission.Current.SetMissionMode((MissionMode)2, true);
		SpawnAgentsForBothSides();
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SetOrder((OrderType)4);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SelectAllFormations(false);
		((MissionBehavior)this).Mission.PlayerEnemyTeam.MasterOrderController.SetOrder((OrderType)4);
	}

	private void SpawnAgentsForBothSides()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.PlayerEnemyTeam.SetIsEnemyOf(Mission.Current.PlayerTeam, true);
		foreach (TroopRosterElement item in (List<TroopRosterElement>)(object)_playerSideTroops.GetTroopRoster())
		{
			TroopRosterElement current = item;
			for (int i = 0; i < ((TroopRosterElement)(ref current)).Number; i++)
			{
				SpawnATroop(current.Character, isPlayerSide: true);
			}
		}
		foreach (TroopRosterElement item2 in (List<TroopRosterElement>)(object)_rivalSideTroops.GetTroopRoster())
		{
			TroopRosterElement current2 = item2;
			for (int j = 0; j < ((TroopRosterElement)(ref current2)).Number; j++)
			{
				SpawnATroop(current2.Character, isPlayerSide: false);
			}
		}
	}

	private void SpawnATroop(CharacterObject character, bool isPlayerSide)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		SimpleAgentOrigin val = new SimpleAgentOrigin((BasicCharacterObject)(object)character, -1, (Banner)null, default(UniqueTroopDescriptor));
		Agent val2 = Mission.Current.SpawnTroop((IAgentOriginBase)(object)val, isPlayerSide, true, false, false, 0, 0, true, true, true, (Vec3?)null, (Vec2?)null, (string)null, (ItemObject)null, (FormationClass)10, false);
		if (isPlayerSide)
		{
			_playerSideAliveAgents.Add(val2);
		}
		else
		{
			_rivalSideAliveAgents.Add(val2);
		}
		AgentFlag agentFlags = val2.GetAgentFlags();
		val2.SetAgentFlags((AgentFlag)((agentFlags | 0x10000) & -1048577));
		if (val2.IsAIControlled)
		{
			val2.SetWatchState((WatchState)2);
		}
		if (isPlayerSide)
		{
			val2.SetTeam(Mission.Current.PlayerTeam, true);
		}
		else
		{
			val2.SetTeam(Mission.Current.PlayerEnemyTeam, true);
		}
	}

	public void StartSpawner(BattleSideEnum side)
	{
	}

	public void StopSpawner(BattleSideEnum side)
	{
	}

	public bool IsSideSpawnEnabled(BattleSideEnum side)
	{
		return true;
	}

	public bool IsSideDepleted(BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)side != 1)
		{
			return _playerSideAliveAgents.Count == 0;
		}
		return _rivalSideAliveAgents.Count == 0;
	}

	public float GetReinforcementInterval()
	{
		return float.MaxValue;
	}

	public IEnumerable<IAgentOriginBase> GetAllTroopsForSide(BattleSideEnum side)
	{
		throw new NotImplementedException();
	}

	public int GetNumberOfPlayerControllableTroops()
	{
		throw new NotImplementedException();
	}

	public bool GetSpawnHorses(BattleSideEnum side)
	{
		return false;
	}
}
