using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.MissionLogics;
using SandBox.Tournaments.AgentControllers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.MissionLogics;

public class TournamentArcheryMissionController : MissionLogic, ITournamentGameBehavior
{
	private readonly List<ArcheryTournamentAgentController> _agentControllers;

	private TournamentMatch _match;

	private BasicMissionTimer _endTimer;

	private List<GameEntity> _spawnPoints;

	private bool _isSimulated;

	private CultureObject _culture;

	private List<DestructableComponent> _targets;

	public List<GameEntity> ShootingPositions;

	private readonly Equipment _archeryEquipment;

	public IEnumerable<ArcheryTournamentAgentController> AgentControllers => _agentControllers;

	public TournamentArcheryMissionController(CultureObject culture)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		_culture = culture;
		ShootingPositions = new List<GameEntity>();
		_agentControllers = new List<ArcheryTournamentAgentController>();
		_archeryEquipment = new Equipment();
		_archeryEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("nordic_shortbow"), (ItemModifier)null, (ItemObject)null, false));
		_archeryEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)1, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("blunt_arrows"), (ItemModifier)null, (ItemObject)null, false));
		_archeryEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)6, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("desert_lamellar"), (ItemModifier)null, (ItemObject)null, false));
		_archeryEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)8, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("reinforced_mail_mitten"), (ItemModifier)null, (ItemObject)null, false));
		_archeryEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)7, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("leather_cavalier_boots"), (ItemModifier)null, (ItemObject)null, false));
	}

	public override void AfterStart()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		TournamentBehavior.DeleteTournamentSetsExcept(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("tournament_archery"));
		_spawnPoints = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_arena").ToList();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
		_targets = MBExtensions.FindAllWithType<DestructableComponent>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).Where(delegate(DestructableComponent x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)x).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).HasTag("archery_target");
		}).ToList();
		foreach (DestructableComponent target in _targets)
		{
			target.OnDestroyed += new OnHitTakenAndDestroyedDelegate(OnTargetDestroyed);
		}
	}

	public void StartMatch(TournamentMatch match, bool isLastRound)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		_match = match;
		ResetTargets();
		int count = _spawnPoints.Count;
		int num = 0;
		int num2 = 0;
		foreach (TournamentTeam team2 in _match.Teams)
		{
			Team team = ((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)(-1), MissionAgentHandler.GetRandomTournamentTeamColor(num2), uint.MaxValue, (Banner)null, true, false, true);
			foreach (TournamentParticipant participant in team2.Participants)
			{
				participant.MatchEquipment = _archeryEquipment.Clone(false);
				MatrixFrame globalFrame = _spawnPoints[num % count].GetGlobalFrame();
				((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
				SetItemsAndSpawnCharacter(participant, team, globalFrame);
				num++;
			}
			num2++;
		}
	}

	public void SkipMatch(TournamentMatch match)
	{
		_match = match;
		Simulate();
	}

	private void Simulate()
	{
		_isSimulated = false;
		List<TournamentParticipant> list = _match.Participants.ToList();
		int num = _targets.Count;
		while (num > 0)
		{
			foreach (TournamentParticipant item in list)
			{
				if (num == 0)
				{
					break;
				}
				if (MBRandom.RandomFloat < GetDeadliness(item))
				{
					item.AddScore(1);
					num--;
				}
			}
		}
		_isSimulated = true;
	}

	public bool IsMatchEnded()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		if (_isSimulated || _match == null)
		{
			return true;
		}
		if (_endTimer != null && _endTimer.ElapsedTime > 6f)
		{
			_endTimer = null;
			return true;
		}
		if (_endTimer == null && (!IsThereAnyTargetLeft() || !IsThereAnyArrowLeft()))
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

	private void ResetTargets()
	{
		foreach (DestructableComponent target in _targets)
		{
			target.Reset();
		}
	}

	private void SetItemsAndSpawnCharacter(TournamentParticipant participant, Team team, MatrixFrame frame)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData obj = new AgentBuildData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)participant.Character, -1, (Banner)null, participant.Descriptor)).Team(team).Equipment(participant.MatchEquipment).InitialPosition(ref frame.origin);
		Vec2 val = ((Vec3)(ref frame.rotation.f)).AsVec2;
		val = ((Vec2)(ref val)).Normalized();
		AgentBuildData val2 = obj.InitialDirection(ref val).Controller((AgentControllerType)((!((BasicCharacterObject)participant.Character).IsPlayerCharacter) ? 1 : 2));
		Agent val3 = ((MissionBehavior)this).Mission.SpawnAgent(val2, false);
		val3.Health = val3.HealthLimit;
		ArcheryTournamentAgentController archeryTournamentAgentController = val3.AddController(typeof(ArcheryTournamentAgentController)) as ArcheryTournamentAgentController;
		archeryTournamentAgentController.SetTargets(_targets);
		_agentControllers.Add(archeryTournamentAgentController);
		if (((BasicCharacterObject)participant.Character).IsPlayerCharacter)
		{
			val3.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
			((MissionBehavior)this).Mission.PlayerTeam = team;
		}
		else
		{
			val3.SetWatchState((WatchState)2);
		}
	}

	public void OnTargetDestroyed(DestructableComponent destroyedComponent, Agent destroyerAgent, in MissionWeapon attackerWeapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
	{
		foreach (ArcheryTournamentAgentController agentController in AgentControllers)
		{
			agentController.OnTargetHit(destroyerAgent, destroyedComponent);
			_match.GetParticipant(destroyerAgent.Origin.UniqueSeed).AddScore(1);
		}
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (IsMatchEnded())
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			item.GetController<ArcheryTournamentAgentController>()?.OnTick();
		}
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		((MissionBehavior)this).Mission.EndMission();
	}

	private bool IsThereAnyTargetLeft()
	{
		return _targets.Any((DestructableComponent e) => !e.IsDestroyed);
	}

	private bool IsThereAnyArrowLeft()
	{
		return ((IEnumerable<Agent>)((MissionBehavior)this).Mission.Agents).Any((Agent agent) => agent.Equipment.GetAmmoAmount((EquipmentIndex)0) > 0);
	}

	private float GetDeadliness(TournamentParticipant participant)
	{
		return 0.01f + (float)((BasicCharacterObject)participant.Character).GetSkillValue(DefaultSkills.Bow) / 300f * 0.19f;
	}
}
