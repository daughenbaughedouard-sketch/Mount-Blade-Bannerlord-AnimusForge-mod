using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.AI.Tactics;
using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.MissionControllers;

public class NeutralWandererShipSpawnMissionController : MissionLogic
{
	private class WandererShipData
	{
		public readonly int TagNumber;

		public readonly GameEntity SpawnPointEntity;

		private readonly List<GameEntity> _targetPoints = new List<GameEntity>();

		private bool _isTargetReversed;

		public MissionShip WandererShip { get; private set; }

		public GameEntity CurrentTarget { get; private set; }

		public WandererShipData(int tagNumber, GameEntity spawnPointEntity)
		{
			TagNumber = tagNumber;
			SpawnPointEntity = spawnPointEntity;
		}

		public void AddTargetPoint(GameEntity targetPoint)
		{
			_targetPoints.Add(targetPoint);
		}

		public void SetWandererShip(MissionShip ship)
		{
			WandererShip = ship;
		}

		public void ChangeToNextTarget()
		{
			if (CurrentTarget == (GameEntity)null)
			{
				CurrentTarget = _targetPoints[0];
				return;
			}
			if (_isTargetReversed)
			{
				for (int num = _targetPoints.Count - 1; num >= 0; num--)
				{
					if (_targetPoints[num] == CurrentTarget)
					{
						if (num == 0)
						{
							_isTargetReversed = false;
							CurrentTarget = _targetPoints[num + 1];
						}
						else
						{
							CurrentTarget = _targetPoints[num - 1];
						}
						break;
					}
				}
				return;
			}
			for (int i = 0; i < _targetPoints.Count; i++)
			{
				if (_targetPoints[i] == CurrentTarget)
				{
					if (i == _targetPoints.Count - 1)
					{
						_isTargetReversed = true;
						CurrentTarget = _targetPoints[i - 1];
					}
					else
					{
						CurrentTarget = _targetPoints[i + 1];
					}
					break;
				}
			}
		}
	}

	private enum WandererShipControllerState
	{
		None,
		SpawnShips,
		SpawnTroops,
		MoveShips,
		End
	}

	private const string WandererShipSpawnPointTagExpression = "wanderer_ship(_\\d+)*_spawnpoint";

	private const string WandererShipTargetPointTagExpression = "wanderer_ship(_\\d+)*_target(_\\d+)*";

	private readonly List<string> _wandererShipIdList = new List<string> { "western_trade_ship_storyline", "sturgia_heavy_ship", "ship_lodya_storyline", "ship_birlinn_storyline" };

	private readonly List<string> _wandererShipTroopIdList = new List<string> { "sea_hounds", "gangradirs_kin_melee" };

	private readonly List<WandererShipData> _wandererShipData = new List<WandererShipData>();

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private Queue<Formation> _availableNeutralFormations = new Queue<Formation>();

	private WandererShipControllerState _currentState;

	public override void OnAfterMissionCreated()
	{
		((MissionBehavior)this).OnAfterMissionCreated();
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		Team playerAllyTeam = ((MissionBehavior)this).Mission.PlayerAllyTeam;
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)0));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)1));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)2));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)3));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)4));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)5));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)6));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)7));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)8));
		_availableNeutralFormations.Enqueue(playerAllyTeam.GetFormation((FormationClass)9));
		playerAllyTeam.SetIsEnemyOf(Mission.GetTeam((TeamSideEnum)2), false);
		playerAllyTeam.SetIsEnemyOf(Mission.GetTeam((TeamSideEnum)0), false);
		CollectWandererShipData();
		_currentState = WandererShipControllerState.SpawnShips;
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		switch (_currentState)
		{
		case WandererShipControllerState.SpawnShips:
			SpawnWandererShips();
			_currentState = WandererShipControllerState.SpawnTroops;
			break;
		case WandererShipControllerState.SpawnTroops:
			SpawnWandererShipTroops();
			_currentState = WandererShipControllerState.MoveShips;
			break;
		case WandererShipControllerState.MoveShips:
			HandleWandererShipMovements();
			break;
		case WandererShipControllerState.None:
		case WandererShipControllerState.End:
			break;
		}
	}

	private void CollectWandererShipData()
	{
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTagExpression("wanderer_ship(_\\d+)*_spawnpoint"))
		{
			int tagNumber = int.Parse(item.Tags.FirstOrDefault().Split(new char[1] { '_' })[2]);
			_wandererShipData.Add(new WandererShipData(tagNumber, item));
		}
		Dictionary<int, List<GameEntity>> dictionary = new Dictionary<int, List<GameEntity>>();
		foreach (GameEntity item2 in Mission.Current.Scene.FindEntitiesWithTagExpression("wanderer_ship(_\\d+)*_target(_\\d+)*"))
		{
			int key = int.Parse(item2.Tags.FirstOrDefault().Split(new char[1] { '_' })[2]);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<GameEntity>();
			}
			dictionary[key].Add(item2);
		}
		foreach (KeyValuePair<int, List<GameEntity>> targetKvp in dictionary)
		{
			GameEntity[] array = (GameEntity[])(object)new GameEntity[targetKvp.Value.Count];
			foreach (GameEntity item3 in targetKvp.Value)
			{
				int num = int.Parse(item3.Tags.FirstOrDefault().Split(new char[1] { '_' })[^1]);
				array[num - 1] = item3;
			}
			WandererShipData wandererShipData = _wandererShipData.First((WandererShipData d) => d.TagNumber == targetKvp.Key);
			GameEntity[] array2 = array;
			foreach (GameEntity targetPoint in array2)
			{
				wandererShipData.AddTargetPoint(targetPoint);
			}
		}
	}

	private void SpawnWandererShips()
	{
		foreach (WandererShipData wandererShipDatum in _wandererShipData)
		{
			if (!Extensions.IsEmpty<Formation>((IEnumerable<Formation>)_availableNeutralFormations))
			{
				MissionShip wandererShip = CreateShip(Extensions.GetRandomElement<string>((IReadOnlyList<string>)_wandererShipIdList), wandererShipDatum.SpawnPointEntity, _availableNeutralFormations.Dequeue());
				wandererShipDatum.SetWandererShip(wandererShip);
			}
		}
	}

	private MissionShip CreateShip(string shipHullId, GameEntity spawnPoint, Formation formation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		MatrixFrame shipFrame = spawnPoint.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = spawnPoint.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, false, false);
		shipFrame.origin = new Vec3(spawnPoint.GlobalPosition.x, spawnPoint.GlobalPosition.y, waterLevelAtPosition, -1f);
		Ship shipOrigin = new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(shipHullId));
		MissionShip missionShip = _navalShipsLogic.SpawnShip((IShipOrigin)(object)shipOrigin, in shipFrame, formation.Team, formation, spawnAnchored: false, (FormationClass)8);
		missionShip.ShipOrder.FormationJoinShip(formation);
		return missionShip;
	}

	private void SpawnWandererShipTroops()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		Team playerAllyTeam = ((MissionBehavior)this).Mission.PlayerAllyTeam;
		TeamAINavalComponent teamAINavalComponent = new TeamAINavalComponent(((MissionBehavior)this).Mission, playerAllyTeam, 5f, 1f);
		playerAllyTeam.AddTeamAI((TeamAIComponent)(object)teamAINavalComponent, false);
		playerAllyTeam.AddTacticOption((TacticComponent)(object)new TacticNavalBalancedOffense(playerAllyTeam));
		_navalAgentsLogic.SetDeploymentMode(value: true);
		_navalShipsLogic.SetDeploymentMode(value: true);
		foreach (WandererShipData wandererShipDatum in _wandererShipData)
		{
			CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>(Extensions.GetRandomElement<string>((IReadOnlyList<string>)_wandererShipTroopIdList));
			int num = MBRandom.RandomInt(7, 13);
			_navalAgentsLogic.SetDesiredTroopCountOfShip(wandererShipDatum.WandererShip, num);
			for (int i = 0; i < num; i++)
			{
				_navalAgentsLogic.AddReservedTroopToShip((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor)), wandererShipDatum.WandererShip);
			}
		}
		_navalAgentsLogic.SetDeploymentMode(value: false);
		_navalShipsLogic.SetDeploymentMode(value: false);
	}

	private void HandleWandererShipMovements()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		foreach (WandererShipData wandererShipDatum in _wandererShipData)
		{
			if (!(wandererShipDatum.CurrentTarget == (GameEntity)null))
			{
				MatrixFrame globalFrame = wandererShipDatum.WandererShip.GlobalFrame;
				if (!(((Vec3)(ref globalFrame.origin)).Distance(wandererShipDatum.CurrentTarget.GlobalPosition) <= 100f))
				{
					ShipOrder shipOrder = wandererShipDatum.WandererShip.ShipOrder;
					Vec3 globalPosition = wandererShipDatum.CurrentTarget.GlobalPosition;
					shipOrder.SetShipMovementOrder(((Vec3)(ref globalPosition)).AsVec2);
					continue;
				}
			}
			wandererShipDatum.ChangeToNextTarget();
		}
	}
}
