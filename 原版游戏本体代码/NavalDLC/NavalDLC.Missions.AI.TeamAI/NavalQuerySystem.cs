using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.AI.TeamAI;

public class NavalQuerySystem
{
	private readonly MBList<Tuple<Formation, Vec2>> _temporaryFormationPositionTupleContainer = new MBList<Tuple<Formation, Vec2>>();

	private readonly MBList<MissionShip> _temporaryMissionShipContainer = new MBList<MissionShip>();

	private readonly Dictionary<(MissionShip, MissionShip), bool> _shipsInCriticalZoneContainer = new Dictionary<(MissionShip, MissionShip), bool>();

	private readonly QueryData<Vec2> _averageShipPosition;

	private readonly QueryData<Vec2> _averageEnemyShipPosition;

	private readonly QueryData<MBReadOnlyList<Formation>> _formationsInShipsInLeftToRightOrder;

	private readonly QueryData<MBReadOnlyList<MissionShip>> _enemyShipsInLeftToRightOrder;

	private readonly QueryData<MBReadOnlyList<MissionShip>> _enemyShipsWithFormationsInLeftToRightOrder;

	private readonly QueryData<MBReadOnlyList<MissionShip>> _teamShipsWithFormationsInLeftToRightOrder;

	private readonly QueryData<Dictionary<(MissionShip, MissionShip), bool>> _shipInCriticalZoneDictionary;

	private readonly QueryData<float> _closestDistanceSquaredToEnemyShip;

	private NavalShipsLogic _navalShipsLogic;

	private Team _team;

	public Vec2 AverageShipPosition => _averageShipPosition.Value;

	public Vec2 AverageEnemyShipPosition => _averageEnemyShipPosition.Value;

	public MBReadOnlyList<Formation> FormationsInShipsInLeftToRightOrder => (MBReadOnlyList<Formation>)(object)Extensions.ToMBList<Formation>((List<Formation>)(object)_formationsInShipsInLeftToRightOrder.Value);

	public MBReadOnlyList<MissionShip> EnemyShipsInLeftToRightOrder => _enemyShipsInLeftToRightOrder.Value;

	public MBReadOnlyList<MissionShip> EnemyShipsWithFormationsInLeftToRightOrder => _enemyShipsWithFormationsInLeftToRightOrder.Value;

	public MBReadOnlyList<MissionShip> TeamShipsWithFormationsInLeftToRightOrder => _teamShipsWithFormationsInLeftToRightOrder.Value;

	public float ClosestDistanceSquaredToEnemyShip => _closestDistanceSquaredToEnemyShip.Value;

	public NavalQuerySystem(Team team)
	{
		_ = Mission.Current;
		_team = team;
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_averageShipPosition = new QueryData<Vec2>((Func<Vec2>)delegate
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			Vec2 val = default(Vec2);
			((Vec2)(ref val))._002Ector(0f, 0f);
			int num = 0;
			foreach (Formation item in (List<Formation>)(object)_team.FormationsIncludingEmpty)
			{
				if (item.CountOfUnits > 0)
				{
					_navalShipsLogic.GetShip(_team.TeamSide, item.FormationIndex, out var ship);
					Vec2 val2 = val;
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
					Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					val = val2 + ((Vec3)(ref globalPosition)).AsVec2;
					num++;
				}
			}
			return (num <= 0) ? val : (val / (float)num);
		}, 1f);
		_averageEnemyShipPosition = new QueryData<Vec2>((Func<Vec2>)delegate
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			Vec2 val = default(Vec2);
			((Vec2)(ref val))._002Ector(0f, 0f);
			int num = 0;
			foreach (Team item2 in (List<Team>)(object)Mission.Current.Teams)
			{
				if (_team.IsEnemyOf(item2))
				{
					foreach (Formation item3 in (List<Formation>)(object)item2.FormationsIncludingEmpty)
					{
						if (item3.CountOfUnits > 0)
						{
							_navalShipsLogic.GetShip(item2.TeamSide, item3.FormationIndex, out var ship);
							Vec2 val2 = val;
							WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
							Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
							val = val2 + ((Vec3)(ref globalPosition)).AsVec2;
							num++;
						}
					}
				}
			}
			return (num <= 0) ? val : (val / (float)num);
		}, 1f);
		_formationsInShipsInLeftToRightOrder = new QueryData<MBReadOnlyList<Formation>>((Func<MBReadOnlyList<Formation>>)delegate
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			((List<Tuple<Formation, Vec2>>)(object)_temporaryFormationPositionTupleContainer).Clear();
			foreach (Formation item4 in (List<Formation>)(object)_team.FormationsIncludingEmpty)
			{
				if (item4.CountOfUnits > 0 && _navalShipsLogic.GetShip(_team.TeamSide, item4.FormationIndex, out var ship))
				{
					MBList<Tuple<Formation, Vec2>> temporaryFormationPositionTupleContainer = _temporaryFormationPositionTupleContainer;
					WeakGameEntity gameEntity = ((ScriptComponentBehavior)ship).GameEntity;
					Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
					((List<Tuple<Formation, Vec2>>)(object)temporaryFormationPositionTupleContainer).Add(new Tuple<Formation, Vec2>(item4, ((Vec3)(ref globalPosition)).AsVec2));
				}
			}
			return (MBReadOnlyList<Formation>)(object)Extensions.ToMBList<Formation>(from fst in ((IEnumerable<Tuple<Formation, Vec2>>)_temporaryFormationPositionTupleContainer).OrderByDescending(delegate(Tuple<Formation, Vec2> fst)
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					//IL_0015: Unknown result type (might be due to invalid IL or missing references)
					//IL_001b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0020: Unknown result type (might be due to invalid IL or missing references)
					//IL_0025: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Unknown result type (might be due to invalid IL or missing references)
					Vec2 val = fst.Item2 - AverageShipPosition;
					Vec2 val2 = AverageEnemyShipPosition - AverageShipPosition;
					return ((Vec2)(ref val)).DotProduct(((Vec2)(ref val2)).LeftVec());
				})
				select fst.Item1);
		}, 5f);
		_enemyShipsInLeftToRightOrder = new QueryData<MBReadOnlyList<MissionShip>>((Func<MBReadOnlyList<MissionShip>>)delegate
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
			foreach (Team item5 in (List<Team>)(object)Mission.Current.Teams)
			{
				if (MBExtensions.IsOpponentOf(_team.Side, item5.Side))
				{
					_navalShipsLogic.FillTeamShips(item5.TeamSide, _temporaryMissionShipContainer);
				}
			}
			return (MBReadOnlyList<MissionShip>)(object)Extensions.ToMBList<MissionShip>((IEnumerable<MissionShip>)((IEnumerable<MissionShip>)_temporaryMissionShipContainer).OrderByDescending(delegate(MissionShip sl)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)sl).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				Vec2 val = ((Vec3)(ref globalPosition)).AsVec2 - AverageEnemyShipPosition;
				Vec2 val2 = AverageShipPosition - AverageEnemyShipPosition;
				return ((Vec2)(ref val)).DotProduct(((Vec2)(ref val2)).LeftVec());
			}));
		}, 5f);
		_enemyShipsWithFormationsInLeftToRightOrder = new QueryData<MBReadOnlyList<MissionShip>>((Func<MBReadOnlyList<MissionShip>>)delegate
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
			foreach (Team item6 in (List<Team>)(object)Mission.Current.Teams)
			{
				if (MBExtensions.IsOpponentOf(_team.Side, item6.Side))
				{
					foreach (Formation item7 in (List<Formation>)(object)item6.FormationsIncludingEmpty)
					{
						if (item7.CountOfUnits > 0 && _navalShipsLogic.GetShip(item6.TeamSide, item7.FormationIndex, out var ship))
						{
							((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(ship);
						}
					}
				}
			}
			return (MBReadOnlyList<MissionShip>)(object)Extensions.ToMBList<MissionShip>((IEnumerable<MissionShip>)((IEnumerable<MissionShip>)_temporaryMissionShipContainer).OrderByDescending(delegate(MissionShip sl)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)sl).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				Vec2 val = ((Vec3)(ref globalPosition)).AsVec2 - AverageEnemyShipPosition;
				Vec2 val2 = AverageShipPosition - AverageEnemyShipPosition;
				return ((Vec2)(ref val)).DotProduct(((Vec2)(ref val2)).LeftVec());
			}));
		}, 5f);
		_teamShipsWithFormationsInLeftToRightOrder = new QueryData<MBReadOnlyList<MissionShip>>((Func<MBReadOnlyList<MissionShip>>)delegate
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			((List<MissionShip>)(object)_temporaryMissionShipContainer).Clear();
			foreach (Formation item8 in (List<Formation>)(object)_team.FormationsIncludingEmpty)
			{
				if (item8.CountOfUnits > 0 && _navalShipsLogic.GetShip(_team.TeamSide, item8.FormationIndex, out var ship))
				{
					((List<MissionShip>)(object)_temporaryMissionShipContainer).Add(ship);
				}
			}
			return (MBReadOnlyList<MissionShip>)(object)Extensions.ToMBList<MissionShip>((IEnumerable<MissionShip>)((IEnumerable<MissionShip>)_temporaryMissionShipContainer).OrderByDescending(delegate(MissionShip sl)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)sl).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				Vec2 val = ((Vec3)(ref globalPosition)).AsVec2 - AverageShipPosition;
				Vec2 val2 = AverageShipPosition - AverageShipPosition;
				return ((Vec2)(ref val)).DotProduct(((Vec2)(ref val2)).LeftVec());
			}));
		}, 5f);
		_shipInCriticalZoneDictionary = new QueryData<Dictionary<(MissionShip, MissionShip), bool>>((Func<Dictionary<(MissionShip, MissionShip), bool>>)delegate
		{
			MBReadOnlyList<MissionShip> allShips = _navalShipsLogic.AllShips;
			foreach (MissionShip item9 in (List<MissionShip>)(object)allShips)
			{
				foreach (MissionShip item10 in (List<MissionShip>)(object)item9.GetConnectedShips())
				{
					(MissionShip, MissionShip) key = ((((object)item9).GetHashCode() < ((object)item10).GetHashCode()) ? (item9, item10) : (item10, item9));
					if (item9.IsShipInCriticalZoneBetween(item10, allShips))
					{
						_shipsInCriticalZoneContainer[key] = true;
					}
					else
					{
						_shipsInCriticalZoneContainer[key] = false;
					}
				}
			}
			return _shipsInCriticalZoneContainer;
		}, 5f);
		_closestDistanceSquaredToEnemyShip = new QueryData<float>((Func<float>)delegate
		{
			float num = float.MaxValue;
			foreach (Formation item11 in (List<Formation>)(object)FormationsInShipsInLeftToRightOrder)
			{
				if (item11.CountOfUnits > 0 && item11.CachedClosestEnemyFormationDistanceSquared < num)
				{
					num = item11.CachedClosestEnemyFormationDistanceSquared;
				}
			}
			return num;
		}, 1f);
		InitializeTelemetryScopeNames();
	}

	public void ForceExpireSameSideShipLists()
	{
		_teamShipsWithFormationsInLeftToRightOrder.Expire();
		_formationsInShipsInLeftToRightOrder.Expire();
	}

	public void ForceExpireAll()
	{
		_averageShipPosition.Expire();
		_averageEnemyShipPosition.Expire();
		_formationsInShipsInLeftToRightOrder.Expire();
		_enemyShipsInLeftToRightOrder.Expire();
		_enemyShipsWithFormationsInLeftToRightOrder.Expire();
		_teamShipsWithFormationsInLeftToRightOrder.Expire();
		_shipInCriticalZoneDictionary.Expire();
		_closestDistanceSquaredToEnemyShip.Expire();
	}

	public bool IsAnyShipInCriticalZoneBetween(MissionShip ship1, MissionShip ship2)
	{
		if (_shipInCriticalZoneDictionary == null || _shipInCriticalZoneDictionary.Value == null)
		{
			return false;
		}
		Dictionary<(MissionShip, MissionShip), bool> value = _shipInCriticalZoneDictionary.Value;
		(MissionShip, MissionShip) key = ((((object)ship1).GetHashCode() < ((object)ship2).GetHashCode()) ? (ship1, ship2) : (ship2, ship1));
		bool value2;
		return value.TryGetValue(key, out value2) && value2;
	}

	private void InitializeTelemetryScopeNames()
	{
	}
}
