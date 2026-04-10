using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Tournaments.AgentControllers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.MissionLogics;

public class TownHorseRaceMissionController : MissionLogic, ITournamentGameBehavior
{
	public class CheckPoint
	{
		private readonly VolumeBox _volumeBox;

		private readonly List<GameEntity> _bestTargetList;

		public string Name
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)_volumeBox).GameEntity;
				return ((WeakGameEntity)(ref gameEntity)).Name;
			}
		}

		public CheckPoint(VolumeBox volumeBox)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			_volumeBox = volumeBox;
			_bestTargetList = MBExtensions.CollectChildrenEntitiesWithTag(GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)_volumeBox).GameEntity), "best_target_point");
			_volumeBox.SetIsOccupiedDelegate(new VolumeBoxDelegate(OnAgentsEnterCheckBox));
		}

		public Vec3 GetBestTargetPosition()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			if (_bestTargetList.Count > 0)
			{
				return _bestTargetList[MBRandom.RandomInt(_bestTargetList.Count)].GetGlobalFrame().origin;
			}
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_volumeBox).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
		}

		public void AddToCheckList(Agent agent)
		{
			_volumeBox.AddToCheckList(agent);
		}

		public void RemoveFromCheckList(Agent agent)
		{
			_volumeBox.RemoveFromCheckList(agent);
		}

		private void OnAgentsEnterCheckBox(VolumeBox volumeBox, List<Agent> agentsInVolume)
		{
			foreach (Agent item in agentsInVolume)
			{
				item.GetController<TownHorseRaceAgentController>().OnEnterCheckPoint(volumeBox);
			}
		}
	}

	public const int TourCount = 2;

	private readonly List<TownHorseRaceAgentController> _agents;

	private List<Team> _teams;

	private List<GameEntity> _startPoints;

	private BasicMissionTimer _startTimer;

	private CultureObject _culture;

	public List<CheckPoint> CheckPoints { get; private set; }

	public TownHorseRaceMissionController(CultureObject culture)
	{
		_culture = culture;
		_agents = new List<TownHorseRaceAgentController>();
	}

	public override void AfterStart()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		CollectCheckPointsAndStartPoints();
		foreach (TownHorseRaceAgentController agent in _agents)
		{
			agent.DisableMovement();
		}
		_startTimer = new BasicMissionTimer();
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (_startTimer == null || !(_startTimer.ElapsedTime > 3f))
		{
			return;
		}
		foreach (TownHorseRaceAgentController agent in _agents)
		{
			agent.Start();
		}
	}

	private void CollectCheckPointsAndStartPoints()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		CheckPoints = new List<CheckPoint>();
		foreach (WeakGameEntity item in ((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).Select((MissionObject amo) => ((ScriptComponentBehavior)amo).GameEntity))
		{
			WeakGameEntity current = item;
			VolumeBox firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<VolumeBox>();
			if (firstScriptOfType != null)
			{
				CheckPoints.Add(new CheckPoint(firstScriptOfType));
			}
		}
		CheckPoints = CheckPoints.OrderBy((CheckPoint x) => x.Name).ToList();
		_startPoints = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_horse_race").ToList();
	}

	private MatrixFrame GetStartFrame(int index)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = ((index >= _startPoints.Count) ? ((_startPoints.Count > 0) ? _startPoints[0].GetGlobalFrame() : MatrixFrame.Identity) : _startPoints[index].GetGlobalFrame());
		((Mat3)(ref result.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		return result;
	}

	private void SetItemsAndSpawnCharacter(CharacterObject troop)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		int count = _agents.Count;
		Equipment val = new Equipment();
		val.AddEquipmentToSlotWithoutAgent((EquipmentIndex)10, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("charger"), (ItemModifier)null, (ItemObject)null, false));
		val.AddEquipmentToSlotWithoutAgent((EquipmentIndex)11, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("horse_harness_e"), (ItemModifier)null, (ItemObject)null, false));
		val.AddEquipmentToSlotWithoutAgent((EquipmentIndex)0, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("horse_whip"), (ItemModifier)null, (ItemObject)null, false));
		val.AddEquipmentToSlotWithoutAgent((EquipmentIndex)6, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>("short_padded_robe"), (ItemModifier)null, (ItemObject)null, false));
		MatrixFrame startFrame = GetStartFrame(count);
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)troop).Team(_teams[count]).InitialPosition(ref startFrame.origin);
		Vec2 val2 = ((Vec3)(ref startFrame.rotation.f)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		AgentBuildData val3 = obj.InitialDirection(ref val2).Equipment(val).Controller((AgentControllerType)((troop != CharacterObject.PlayerCharacter) ? 1 : 2));
		Agent val4 = ((MissionBehavior)this).Mission.SpawnAgent(val3, false);
		val4.Health = val4.Monster.HitPoints;
		val4.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
		_agents.Add(AddHorseRaceAgentController(val4));
		if (troop == CharacterObject.PlayerCharacter)
		{
			((MissionBehavior)this).Mission.PlayerTeam = _teams[count];
		}
	}

	private TownHorseRaceAgentController AddHorseRaceAgentController(Agent agent)
	{
		return agent.AddController(typeof(TownHorseRaceAgentController)) as TownHorseRaceAgentController;
	}

	private void InitializeTeams(int count)
	{
		_teams = new List<Team>();
		for (int i = 0; i < count; i++)
		{
			_teams.Add(((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)(-1), uint.MaxValue, uint.MaxValue, (Banner)null, true, false, true));
		}
	}

	public void StartMatch(TournamentMatch match, bool isLastRound)
	{
		throw new NotImplementedException();
	}

	public void SkipMatch(TournamentMatch match)
	{
		throw new NotImplementedException();
	}

	public bool IsMatchEnded()
	{
		throw new NotImplementedException();
	}

	public void OnMatchEnded()
	{
		throw new NotImplementedException();
	}
}
