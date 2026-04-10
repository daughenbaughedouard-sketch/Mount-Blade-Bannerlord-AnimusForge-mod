using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class WhileEnteringSettlementBattleMissionController : MissionLogic, IMissionAgentSpawnLogic, IMissionBehavior
{
	private const int GuardSpawnPointAndPlayerSpawnPointPositionDelta = 20;

	private BattleAgentLogic _battleAgentLogic;

	private bool _isMissionInitialized;

	private bool _troopsInitialized;

	private int _numberOfMaxTroopForPlayer;

	private int _numberOfMaxTroopForEnemy;

	private int _playerSideSpawnedTroopCount;

	private int _otherSideSpawnedTroopCount;

	private readonly IMissionTroopSupplier[] _troopSuppliers;

	public WhileEnteringSettlementBattleMissionController(IMissionTroopSupplier[] suppliers, int numberOfMaxTroopForPlayer, int numberOfMaxTroopForEnemy)
	{
		_troopSuppliers = suppliers;
		_numberOfMaxTroopForPlayer = numberOfMaxTroopForPlayer;
		_numberOfMaxTroopForEnemy = numberOfMaxTroopForEnemy;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_battleAgentLogic = Mission.Current.GetMissionBehavior<BattleAgentLogic>();
	}

	public override void OnMissionTick(float dt)
	{
		if (!_isMissionInitialized)
		{
			SpawnAgents();
			_isMissionInitialized = true;
		}
		else
		{
			if (_troopsInitialized)
			{
				return;
			}
			_troopsInitialized = true;
			foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
			{
				((MissionBehavior)_battleAgentLogic).OnAgentBuild(item, (Banner)null);
			}
		}
	}

	private void SpawnAgents()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("sp_outside_near_town_main_gate");
		IMissionTroopSupplier[] troopSuppliers = _troopSuppliers;
		WorldFrame val2 = default(WorldFrame);
		for (int i = 0; i < troopSuppliers.Length; i++)
		{
			foreach (IAgentOriginBase item in troopSuppliers[i].SupplyTroops(_numberOfMaxTroopForPlayer + _numberOfMaxTroopForEnemy).ToList())
			{
				bool flag = item.IsUnderPlayersCommand || item.Troop.IsPlayerCharacter;
				if ((!flag || _numberOfMaxTroopForPlayer >= _playerSideSpawnedTroopCount) && (flag || _numberOfMaxTroopForEnemy >= _otherSideSpawnedTroopCount))
				{
					((WorldFrame)(ref val2))._002Ector(val.GetGlobalFrame().rotation, new WorldPosition(((MissionBehavior)this).Mission.Scene, val.GetGlobalFrame().origin));
					if (!flag)
					{
						((WorldPosition)(ref val2.Origin)).SetVec2(((WorldPosition)(ref val2.Origin)).AsVec2 + ((Vec3)(ref val2.Rotation.f)).AsVec2 * 20f);
						ref Mat3 rotation = ref val2.Rotation;
						MatrixFrame globalFrame = val.GetGlobalFrame();
						Vec2 val3 = ((Vec3)(ref globalFrame.origin)).AsVec2 - ((WorldPosition)(ref val2.Origin)).AsVec2;
						rotation.f = ((Vec2)(ref val3)).ToVec3(0f);
						ref WorldPosition origin = ref val2.Origin;
						Vec3 randomPositionAroundPoint = ((MissionBehavior)this).Mission.GetRandomPositionAroundPoint(((WorldPosition)(ref val2.Origin)).GetNavMeshVec3(), 0f, 2.5f, false);
						((WorldPosition)(ref origin)).SetVec2(((Vec3)(ref randomPositionAroundPoint)).AsVec2);
					}
					((Mat3)(ref val2.Rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
					((MissionBehavior)this).Mission.SpawnTroop(item, flag, false, false, false, 0, 0, true, false, false, (Vec3?)((WorldPosition)(ref val2.Origin)).GetGroundVec3(), (Vec2?)((Vec3)(ref val2.Rotation.f)).AsVec2, (string)null, (ItemObject)null, (FormationClass)10, false).Defensiveness = 1f;
					if (flag)
					{
						_playerSideSpawnedTroopCount++;
					}
					else
					{
						_otherSideSpawnedTroopCount++;
					}
				}
			}
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
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (side == ((MissionBehavior)this).Mission.PlayerTeam.Side)
		{
			return _troopSuppliers[side].NumRemovedTroops == _playerSideSpawnedTroopCount;
		}
		return _troopSuppliers[side].NumRemovedTroops == _otherSideSpawnedTroopCount;
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
