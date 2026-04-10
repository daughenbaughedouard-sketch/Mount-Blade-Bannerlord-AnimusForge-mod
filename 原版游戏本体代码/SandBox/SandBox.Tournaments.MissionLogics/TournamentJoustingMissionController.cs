using System.Collections.Generic;
using System.Linq;
using SandBox.Tournaments.AgentControllers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.MissionLogics;

public class TournamentJoustingMissionController : MissionLogic, ITournamentGameBehavior
{
	public delegate void JoustingEventDelegate(Agent affectedAgent, Agent affectorAgent);

	public delegate void JoustingAgentStateChangedEventDelegate(Agent agent, JoustingAgentController.JoustingAgentState state);

	private Team _winnerTeam;

	public List<GameEntity> RegionBoxList;

	public List<GameEntity> RegionExitBoxList;

	public List<MatrixFrame> CornerBackStartList;

	public List<GameEntity> CornerStartList;

	public List<MatrixFrame> CornerMiddleList;

	public List<MatrixFrame> CornerFinishList;

	public bool IsSwordDuelStarted;

	private TournamentMatch _match;

	private BasicMissionTimer _endTimer;

	private bool _isSimulated;

	private CultureObject _culture;

	private readonly Equipment _joustingEquipment;

	public event JoustingEventDelegate VictoryAchieved;

	public event JoustingEventDelegate PointGanied;

	public event JoustingEventDelegate Disqualified;

	public event JoustingEventDelegate Unconscious;

	public event JoustingAgentStateChangedEventDelegate AgentStateChanged;

	public TournamentJoustingMissionController(CultureObject culture)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		_culture = culture;
		_match = null;
		RegionBoxList = new List<GameEntity>(2);
		RegionExitBoxList = new List<GameEntity>(2);
		CornerBackStartList = new List<MatrixFrame>();
		CornerStartList = new List<GameEntity>(2);
		CornerMiddleList = new List<MatrixFrame>();
		CornerFinishList = new List<MatrixFrame>();
		IsSwordDuelStarted = false;
		_joustingEquipment = new Equipment();
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)10, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("charger"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)11, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("horse_harness_e"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("vlandia_lance_2_t4"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)1, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("leather_round_shield"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)6, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("desert_lamellar"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)5, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("nasal_helmet_with_mail"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)8, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("reinforced_mail_mitten"), (ItemModifier)null, (ItemObject)null, false));
		_joustingEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)7, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("leather_cavalier_boots"), (ItemModifier)null, (ItemObject)null, false));
	}

	public override void AfterStart()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		TournamentBehavior.DeleteTournamentSetsExcept(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("tournament_jousting"));
		for (int i = 0; i < 2; i++)
		{
			GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_jousting_back_" + i);
			GameEntity item = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_jousting_start_" + i);
			GameEntity val2 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_jousting_middle_" + i);
			GameEntity val3 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_jousting_finish_" + i);
			CornerBackStartList.Add(val.GetGlobalFrame());
			CornerStartList.Add(item);
			CornerMiddleList.Add(val2.GetGlobalFrame());
			CornerFinishList.Add(val3.GetGlobalFrame());
		}
		GameEntity item2 = ((MissionBehavior)this).Mission.Scene.FindEntityWithName("region_box_0");
		RegionBoxList.Add(item2);
		GameEntity item3 = ((MissionBehavior)this).Mission.Scene.FindEntityWithName("region_box_1");
		RegionBoxList.Add(item3);
		GameEntity item4 = ((MissionBehavior)this).Mission.Scene.FindEntityWithName("region_end_box_0");
		RegionExitBoxList.Add(item4);
		GameEntity item5 = ((MissionBehavior)this).Mission.Scene.FindEntityWithName("region_end_box_1");
		RegionExitBoxList.Add(item5);
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
	}

	public void StartMatch(TournamentMatch match, bool isLastRound)
	{
		_match = match;
		int num = 0;
		foreach (TournamentTeam team2 in _match.Teams)
		{
			Team team = ((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)(-1), uint.MaxValue, uint.MaxValue, (Banner)null, true, false, true);
			foreach (TournamentParticipant participant in team2.Participants)
			{
				participant.MatchEquipment = _joustingEquipment.Clone(false);
				SetItemsAndSpawnCharacter(participant, team, num);
			}
			num++;
		}
		List<Team> list = ((IEnumerable<Team>)((MissionBehavior)this).Mission.Teams).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = i + 1; j < list.Count; j++)
			{
				list[i].SetIsEnemyOf(list[j], true);
			}
		}
		((MissionBehavior)this).Mission.Scene.SetAbilityOfFacesWithId(1, false);
		((MissionBehavior)this).Mission.Scene.SetAbilityOfFacesWithId(2, false);
	}

	public void SkipMatch(TournamentMatch match)
	{
		_match = match;
		Simulate();
	}

	public bool IsMatchEnded()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		if (_isSimulated || _match == null)
		{
			return true;
		}
		if (_endTimer != null && _endTimer.ElapsedTime > 6f)
		{
			_endTimer = null;
			return true;
		}
		if (_endTimer == null && _winnerTeam != null)
		{
			_endTimer = new BasicMissionTimer();
		}
		return false;
	}

	public void OnMatchEnded()
	{
		SandBoxHelpers.MissionHelper.FadeOutAgents((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents, hideInstantly: true, hideMount: false);
		((MissionBehavior)this).Mission.ClearCorpses(false);
		((MissionBehavior)this).Mission.Teams.Clear();
		((MissionBehavior)this).Mission.RemoveSpawnedItemsAndMissiles();
		_match = null;
		_endTimer = null;
		_isSimulated = false;
	}

	private void Simulate()
	{
		_isSimulated = false;
		List<TournamentParticipant> participants = _match.Participants.ToList();
		while (participants.Count > 1 && participants.Any((TournamentParticipant x) => x.Team != participants[0].Team) && !participants.Any((TournamentParticipant x) => x.Score >= 3))
		{
			TournamentParticipant val = participants[MBRandom.RandomInt(participants.Count)];
			TournamentParticipant val2 = participants[MBRandom.RandomInt(participants.Count)];
			while (val == val2 || val.Team == val2.Team)
			{
				val2 = participants[MBRandom.RandomInt(participants.Count)];
			}
			val.AddScore(1);
		}
		_isSimulated = true;
	}

	private void SetItemsAndSpawnCharacter(TournamentParticipant participant, Team team, int cornerIndex)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)participant.Character, -1, (Banner)null, participant.Descriptor)).Team(team).InitialFrameFromSpawnPointEntity(CornerStartList[cornerIndex]).Equipment(participant.MatchEquipment)
			.Controller((AgentControllerType)((!((BasicCharacterObject)participant.Character).IsPlayerCharacter) ? 1 : 2));
		Agent val2 = ((MissionBehavior)this).Mission.SpawnAgent(val, false);
		val2.Health = val2.HealthLimit;
		AddJoustingAgentController(val2);
		val2.GetController<JoustingAgentController>().CurrentCornerIndex = cornerIndex;
		if (((BasicCharacterObject)participant.Character).IsPlayerCharacter)
		{
			val2.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
			((MissionBehavior)this).Mission.PlayerTeam = team;
		}
		else
		{
			val2.SetWatchState((WatchState)2);
		}
	}

	private void AddJoustingAgentController(Agent agent)
	{
		agent.AddController(typeof(JoustingAgentController));
	}

	public bool IsAgentInTheTrack(Agent agent, bool inCurrentTrack = true)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (agent != null)
		{
			JoustingAgentController controller = agent.GetController<JoustingAgentController>();
			int index = (inCurrentTrack ? controller.CurrentCornerIndex : (1 - controller.CurrentCornerIndex));
			result = RegionBoxList[index].CheckPointWithOrientedBoundingBox(agent.Position);
		}
		return result;
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (((MissionBehavior)this).Mission.IsMissionEnding)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			item.GetController<JoustingAgentController>()?.UpdateState();
		}
		CheckStartOfSwordDuel();
	}

	private void CheckStartOfSwordDuel()
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Invalid comparison between Unknown and O
		if (((MissionBehavior)this).Mission.IsMissionEnding)
		{
			return;
		}
		if (!IsSwordDuelStarted)
		{
			if (((List<Agent>)(object)((MissionBehavior)this).Mission.Agents).Count <= 0 || ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Count((Agent a) => a.IsMount) >= 2)
			{
				return;
			}
			IsSwordDuelStarted = true;
			RemoveBarriers();
			((MissionBehavior)this).Mission.Scene.SetAbilityOfFacesWithId(2, true);
			{
				foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
				{
					if (item.IsHuman)
					{
						JoustingAgentController controller = item.GetController<JoustingAgentController>();
						controller.State = JoustingAgentController.JoustingAgentState.SwordDuel;
						controller.PrepareAgentToSwordDuel();
					}
				}
				return;
			}
		}
		foreach (Agent item2 in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (!item2.IsHuman)
			{
				continue;
			}
			JoustingAgentController controller2 = item2.GetController<JoustingAgentController>();
			controller2.State = JoustingAgentController.JoustingAgentState.SwordDuel;
			if (controller2.PrepareEquipmentsAfterDismount && item2.MountAgent == null)
			{
				CharacterObject val = (CharacterObject)item2.Character;
				controller2.PrepareEquipmentsForSwordDuel();
				item2.DisableScriptedMovement();
				if ((object)val == CharacterObject.PlayerCharacter)
				{
					item2.Controller = (AgentControllerType)2;
				}
			}
		}
	}

	private void RemoveBarriers()
	{
		foreach (GameEntity item in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("jousting_barrier").ToList())
		{
			item.Remove(95);
		}
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		if (((MissionBehavior)this).Mission.IsMissionEnding || IsSwordDuelStarted || !affectedAgent.IsHuman || affectorAgent == null || !affectorAgent.IsHuman || affectedAgent == affectorAgent)
		{
			return;
		}
		JoustingAgentController controller = affectorAgent.GetController<JoustingAgentController>();
		JoustingAgentController controller2 = affectedAgent.GetController<JoustingAgentController>();
		if (IsAgentInTheTrack(affectorAgent) && controller2.IsRiding() && controller.IsRiding())
		{
			_match.GetParticipant(affectorAgent.Origin.UniqueSeed).AddScore(1);
			controller.Score++;
			if (controller.Score >= 3)
			{
				_winnerTeam = affectorAgent.Team;
				this.VictoryAchieved?.Invoke(affectorAgent, affectedAgent);
			}
			else
			{
				this.PointGanied?.Invoke(affectorAgent, affectedAgent);
			}
		}
		else
		{
			_match.GetParticipant(affectorAgent.Origin.UniqueSeed).AddScore(-100);
			_winnerTeam = affectedAgent.Team;
			MBTextManager.SetTextVariable("OPPONENT_GENDER", affectorAgent.Character.IsFemale ? "0" : "1", false);
			this.Disqualified?.Invoke(affectorAgent, affectedAgent);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (((MissionBehavior)this).Mission.IsMissionEnding || !affectedAgent.IsHuman || affectorAgent == null || !affectorAgent.IsHuman || affectedAgent == affectorAgent)
		{
			return;
		}
		if (IsAgentInTheTrack(affectorAgent) || IsSwordDuelStarted)
		{
			_match.GetParticipant(affectorAgent.Origin.UniqueSeed).AddScore(100);
			_winnerTeam = affectorAgent.Team;
			if (this.Unconscious != null)
			{
				this.Unconscious(affectorAgent, affectedAgent);
			}
			return;
		}
		_match.GetParticipant(affectorAgent.Origin.UniqueSeed).AddScore(-100);
		_winnerTeam = affectedAgent.Team;
		MBTextManager.SetTextVariable("OPPONENT_GENDER", affectorAgent.Character.IsFemale ? "0" : "1", false);
		if (this.Disqualified != null)
		{
			this.Disqualified(affectorAgent, affectedAgent);
		}
	}

	public void OnJoustingAgentStateChanged(Agent agent, JoustingAgentController.JoustingAgentState state)
	{
		if (this.AgentStateChanged != null)
		{
			this.AgentStateChanged(agent, state);
		}
	}
}
