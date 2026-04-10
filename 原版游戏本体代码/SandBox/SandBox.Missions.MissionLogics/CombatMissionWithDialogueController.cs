using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.AI.AgentComponents;

namespace SandBox.Missions.MissionLogics;

public class CombatMissionWithDialogueController : MissionLogic, IMissionAgentSpawnLogic, IMissionBehavior
{
	private BattleAgentLogic _battleAgentLogic;

	private readonly BasicCharacterObject _characterToTalkTo;

	private bool _isMissionInitialized;

	private bool _troopsInitialized;

	private bool _conversationInitialized;

	private int _numSpawnedTroops;

	private readonly IMissionTroopSupplier[] _troopSuppliers;

	public CombatMissionWithDialogueController(IMissionTroopSupplier[] suppliers, BasicCharacterObject characterToTalkTo)
	{
		_troopSuppliers = suppliers;
		_characterToTalkTo = characterToTalkTo;
	}

	public override void OnCreated()
	{
		((MissionBehavior)this).OnCreated();
		((MissionBehavior)this).Mission.DoesMissionRequireCivilianEquipment = true;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_battleAgentLogic = Mission.Current.GetMissionBehavior<BattleAgentLogic>();
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		((MissionBehavior)this).Mission.DeploymentPlan.MakeDefaultDeploymentPlans();
	}

	public override void OnMissionTick(float dt)
	{
		if (!_isMissionInitialized)
		{
			SpawnAgents();
			_isMissionInitialized = true;
			Mission.Current.OnDeploymentFinished();
			return;
		}
		if (!_troopsInitialized)
		{
			_troopsInitialized = true;
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				((MissionBehavior)_battleAgentLogic).OnAgentBuild(item, (Banner)null);
			}
		}
		if (_conversationInitialized || Agent.Main == null || !Agent.Main.IsActive())
		{
			return;
		}
		foreach (Agent item2 in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			ScriptedMovementComponent component = item2.GetComponent<ScriptedMovementComponent>();
			if (component != null && component.ShouldConversationStartWithAgent())
			{
				StartConversation(item2, setActionsInstantly: true);
				_conversationInitialized = true;
			}
		}
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		if (!_conversationInitialized && affectedAgent.Team != Mission.Current.PlayerTeam && affectorAgent != null && affectorAgent == Agent.Main)
		{
			_conversationInitialized = true;
			StartFight(hasPlayerChangedSide: false);
		}
	}

	public void StartFight(bool hasPlayerChangedSide)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, false);
		if (hasPlayerChangedSide)
		{
			Agent.Main.SetTeam((Agent.Main.Team == ((MissionBehavior)this).Mission.AttackerTeam) ? ((MissionBehavior)this).Mission.DefenderTeam : ((MissionBehavior)this).Mission.AttackerTeam, true);
			Mission.Current.PlayerTeam = Agent.Main.Team;
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (Agent.Main != item)
			{
				if (hasPlayerChangedSide && item.Team != Mission.Current.PlayerTeam && (object)/*isinst with value type is only supported in some contexts*/ == PartyBase.MainParty)
				{
					item.SetTeam(Mission.Current.PlayerTeam, true);
				}
				AgentFlag agentFlags = item.GetAgentFlags();
				item.SetAgentFlags((AgentFlag)(agentFlags | 0x10000));
				item.GetComponent<CampaignAgentComponent>().CreateAgentNavigator();
				item.GetComponent<CampaignAgentComponent>().AgentNavigator.AddBehaviorGroup<AlarmedBehaviorGroup>();
				item.SetAlarmState((AIStateFlag)3);
			}
		}
	}

	public void StartConversation(Agent agent, bool setActionsInstantly)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Campaign.Current.ConversationManager.SetupAndStartMissionConversation((IAgent)(object)agent, (IAgent)(object)((MissionBehavior)this).Mission.MainAgent, setActionsInstantly);
		foreach (Agent conversationAgent in Campaign.Current.ConversationManager.ConversationAgents)
		{
			conversationAgent.ForceAiBehaviorSelection();
			conversationAgent.AgentVisuals.SetClothComponentKeepStateOfAllMeshes(true);
		}
		((MissionBehavior)this).Mission.MainAgentServer.AgentVisuals.SetClothComponentKeepStateOfAllMeshes(true);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)1, setActionsInstantly);
	}

	private void SpawnAgents()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		Agent targetAgent = null;
		IMissionTroopSupplier[] troopSuppliers = _troopSuppliers;
		for (int i = 0; i < troopSuppliers.Length; i++)
		{
			foreach (IAgentOriginBase item in troopSuppliers[i].SupplyTroops(25).ToList())
			{
				Agent val = Mission.Current.SpawnTroop(item, (int)item.BattleCombatant.Side == 1, false, false, false, 0, 0, false, true, true, (Vec3?)null, (Vec2?)null, (string)null, (ItemObject)null, (FormationClass)10, false);
				_numSpawnedTroops++;
				if (!val.IsMainAgent)
				{
					val.AddComponent((AgentComponent)new ScriptedMovementComponent(val, val.Character == _characterToTalkTo, (float)(item.IsUnderPlayersCommand ? 5 : 2)));
					if (val.Character == _characterToTalkTo)
					{
						targetAgent = val;
					}
				}
			}
		}
		foreach (Agent item2 in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			ScriptedMovementComponent component = item2.GetComponent<ScriptedMovementComponent>();
			if (component != null)
			{
				if (item2.Team.Side == Mission.Current.PlayerTeam.Side)
				{
					component.SetTargetAgent(targetAgent);
				}
				else
				{
					component.SetTargetAgent(Agent.Main);
				}
			}
			item2.SetFiringOrder((RangedWeaponUsageOrderEnum)1);
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
		return false;
	}

	public float GetReinforcementInterval()
	{
		return 0f;
	}

	public bool IsSideDepleted(BattleSideEnum side)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		int num = _troopSuppliers[side].GetAllTroops().Count() - _troopSuppliers[side].NumTroopsNotSupplied - _troopSuppliers[side].NumRemovedTroops;
		if (Mission.Current.PlayerTeam == ((MissionBehavior)this).Mission.DefenderTeam)
		{
			if ((int)side == 1)
			{
				num -= MobileParty.MainParty.Party.NumberOfHealthyMembers;
			}
			else if (Agent.Main != null && Agent.Main.IsActive())
			{
				num += MobileParty.MainParty.Party.NumberOfHealthyMembers;
			}
		}
		return num == 0;
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
		throw new NotImplementedException();
	}
}
