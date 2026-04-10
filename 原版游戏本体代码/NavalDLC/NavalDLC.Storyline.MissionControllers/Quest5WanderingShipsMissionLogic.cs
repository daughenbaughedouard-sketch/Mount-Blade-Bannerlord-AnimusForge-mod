using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipControl;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.MissionControllers;

public class Quest5WanderingShipsMissionLogic : MissionLogic
{
	private const string PropShip1StringId = "nord_medium_ship";

	private const string PropShip2StringId = "eastern_heavy_ship";

	private const string PropShipTroopStringId = "gangster_1";

	private const int WayPoint1Count = 6;

	private const int WayPoint2Count = 6;

	private const float WayPointSuccessDistance = 10f;

	private NavalShipsLogic _navalShipsLogic;

	private NavalAgentsLogic _navalAgentsLogic;

	private MissionShip _propShip1;

	private MissionShip _propShip2;

	private List<GameEntity> _wayPoints1 = new List<GameEntity>();

	private List<GameEntity> _wayPoints2 = new List<GameEntity>();

	private int _currentWaypointIndex1;

	private int _currentWaypointIndex2;

	public override void EarlyStart()
	{
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, Clan.PlayerClan.Color, Clan.PlayerClan.Color2, Clan.PlayerClan.Banner, true, false, true);
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.DefenderTeam;
	}

	public override void AfterStart()
	{
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		_navalAgentsLogic.UpdateTeamAgentsData();
		SetupPropShips();
	}

	private void SetupPropShips()
	{
		InitializeWaypoints();
		SpawnPropShips();
	}

	private void InitializeWaypoints()
	{
		for (int i = 1; i <= 6; i++)
		{
			GameEntity item = Mission.Current.Scene.FindEntityWithTag("propship_1_waypoint_" + i);
			_wayPoints1.Add(item);
		}
		for (int j = 1; j <= 6; j++)
		{
			GameEntity item2 = Mission.Current.Scene.FindEntityWithTag("propship_2_waypoint_" + j);
			_wayPoints2.Add(item2);
		}
	}

	private void SpawnPropShips()
	{
		_propShip1 = CreateShip("nord_medium_ship", "propship_1_waypoint_1", ((MissionBehavior)this).Mission.PlayerAllyTeam.GetFormation((FormationClass)0));
		_propShip1.SetController(ShipControllerType.AI);
		SpawnPropShipAgents(_propShip1, "gangster_1");
		_propShip2 = CreateShip("eastern_heavy_ship", "propship_2_waypoint_1", ((MissionBehavior)this).Mission.PlayerAllyTeam.GetFormation((FormationClass)2));
		_propShip2.SetController(ShipControllerType.AI);
		SpawnPropShipAgents(_propShip2, "gangster_1");
	}

	private MissionShip CreateShip(string shipHullId, string spawnPointId, Formation formation, bool spawnAnchored = false, List<KeyValuePair<string, string>> additionalUpgradePieces = null, Figurehead figurehead = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		GameEntity val = Mission.Current.Scene.FindEntityWithTag(spawnPointId);
		MatrixFrame shipFrame = val.GetGlobalFrame();
		Scene scene = Mission.Current.Scene;
		Vec3 globalPosition = val.GlobalPosition;
		float waterLevelAtPosition = scene.GetWaterLevelAtPosition(((Vec3)(ref globalPosition)).AsVec2, false, false);
		shipFrame.origin = new Vec3(val.GlobalPosition.x, val.GlobalPosition.y, waterLevelAtPosition, -1f);
		Ship val2 = new Ship(((GameType)Campaign.Current).ObjectManager.GetObject<ShipHull>(shipHullId));
		if (additionalUpgradePieces != null)
		{
			foreach (KeyValuePair<string, string> additionalUpgradePiece in additionalUpgradePieces)
			{
				ShipUpgradePiece val3 = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(additionalUpgradePiece.Value);
				val2.EquipUpgradePiece(additionalUpgradePiece.Key, val3);
			}
		}
		if (figurehead != null)
		{
			val2.ChangeFigurehead(figurehead);
		}
		MissionShip missionShip = _navalShipsLogic.SpawnShip((IShipOrigin)(object)val2, in shipFrame, formation.Team, formation, spawnAnchored, (FormationClass)8);
		missionShip.ShipOrder.FormationJoinShip(formation);
		return missionShip;
	}

	private void SpawnPropShipAgents(MissionShip ship, string troopType)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		int num = ship.CrewSizeOnMainDeck / 2;
		NavalAgentsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalAgentsLogic>();
		missionBehavior.SetDesiredTroopCountOfShip(ship, num);
		BasicCharacterObject val = (BasicCharacterObject)(object)((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>(troopType);
		List<MatrixFrame> list = ((IEnumerable<MatrixFrame>)ship.OuterDeckLocalFrames).Concat((IEnumerable<MatrixFrame>)ship.InnerDeckLocalFrames).ToList();
		for (int i = 0; i < list.Count() && i < num; i++)
		{
			MatrixFrame val2 = list[i];
			Vec3 origin = val2.origin;
			Vec2 asVec = ((Vec3)(ref val2.rotation.f)).AsVec2;
			AgentBuildData val3 = new AgentBuildData(val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(ship.Team).InitialPosition(ref origin)
				.InitialDirection(ref asVec)
				.NoHorses(true)
				.NoWeapons(false);
			Agent val4 = Mission.Current.SpawnAgent(val3, false);
			missionBehavior.AddAgentToShip(val4, ship);
			val4.SetAgentFlags((AgentFlag)(val4.GetAgentFlags() & -65537));
			val4.ToggleInvulnerable();
		}
	}

	public override void OnMissionTick(float dt)
	{
		HandlePropShipOrders();
	}

	private void HandlePropShipOrders()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val2;
		if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_wayPoints1))
		{
			GameEntity val = _wayPoints1[_currentWaypointIndex1];
			val2 = val.GlobalPosition - _propShip1.GlobalFrame.origin;
			if (((Vec3)(ref val2)).LengthSquared <= 100f)
			{
				_currentWaypointIndex1 = (_currentWaypointIndex1 + 1) % 6;
				val = _wayPoints1[_currentWaypointIndex1];
			}
			ShipOrder shipOrder = _propShip1.ShipOrder;
			val2 = val.GlobalPosition;
			shipOrder.SetShipMovementOrder(((Vec3)(ref val2)).AsVec2);
		}
		if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_wayPoints2))
		{
			GameEntity val3 = _wayPoints2[_currentWaypointIndex2];
			val2 = val3.GlobalPosition - _propShip2.GlobalFrame.origin;
			if (((Vec3)(ref val2)).LengthSquared <= 100f)
			{
				_currentWaypointIndex2 = (_currentWaypointIndex2 + 1) % 6;
				val3 = _wayPoints2[_currentWaypointIndex2];
			}
			ShipOrder shipOrder2 = _propShip2.ShipOrder;
			val2 = val3.GlobalPosition;
			shipOrder2.SetShipMovementOrder(((Vec3)(ref val2)).AsVec2);
		}
	}

	public void OnPhase2Started()
	{
		if (_propShip1 != null)
		{
			_navalShipsLogic.RemoveShip(_propShip1);
		}
		if (_propShip2 != null)
		{
			_navalShipsLogic.RemoveShip(_propShip2);
		}
	}
}
