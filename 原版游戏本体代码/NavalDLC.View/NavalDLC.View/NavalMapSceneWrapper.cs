using System;
using System.Collections.Generic;
using System.Linq;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.View;

public class NavalMapSceneWrapper : INavalMapSceneWrapper
{
	private const string VillageDropOffPointTag = "main_map_village_dropoff";

	private MapScene _mapScene;

	private Dictionary<string, List<(CampaignVec2, float)>> _pirateSpawnPoints = new Dictionary<string, List<(CampaignVec2, float)>>();

	public NavalMapSceneWrapper()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		_mapScene = (MapScene)Campaign.Current.MapSceneWrapper;
		InitializePirateSpawnPoints();
		InitializeDropOffLocations();
		InitializeMapWaterWake();
	}

	public void Tick(float dt)
	{
	}

	private void InitializePirateSpawnPoints()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = new List<GameEntity>();
		_mapScene.Scene.GetAllEntitiesWithScriptComponent<PirateSpawnPoint>(ref list);
		CampaignVec2 item = default(CampaignVec2);
		for (int i = 0; i < list.Count; i++)
		{
			PirateSpawnPoint firstScriptOfType = list[i].GetFirstScriptOfType<PirateSpawnPoint>();
			string clanStringId = firstScriptOfType.ClanStringId;
			if (!_pirateSpawnPoints.TryGetValue(clanStringId, out var _))
			{
				_pirateSpawnPoints[clanStringId] = new List<(CampaignVec2, float)>();
			}
			((CampaignVec2)(ref item))._002Ector(firstScriptOfType.GetPosition(), false);
			_pirateSpawnPoints[clanStringId].Add((item, firstScriptOfType.Radius));
		}
	}

	public List<(CampaignVec2, float)> GetSpawnPoints(string stringId)
	{
		if (_pirateSpawnPoints.TryGetValue(stringId, out var value))
		{
			return value;
		}
		return new List<(CampaignVec2, float)>();
	}

	private List<(CampaignVec2, float)> GetSpawnPoints()
	{
		List<(CampaignVec2, float)> list = new List<(CampaignVec2, float)>();
		foreach (KeyValuePair<string, List<(CampaignVec2, float)>> pirateSpawnPoint in _pirateSpawnPoints)
		{
			list.AddRange(pirateSpawnPoint.Value);
		}
		return list;
	}

	private void InitializeDropOffLocations()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 portPosition = default(CampaignVec2);
		foreach (GameEntity entity in _mapScene.Scene.FindEntitiesWithTag("main_map_village_dropoff"))
		{
			Village? obj = ((IEnumerable<Village>)Village.All).FirstOrDefault((Func<Village, bool>)((Village x) => ((MBObjectBase)((SettlementComponent)x).Settlement).StringId == entity.Parent.Name));
			Vec3 globalPosition = entity.GlobalPosition;
			((CampaignVec2)(ref portPosition))._002Ector(((Vec3)(ref globalPosition)).AsVec2, false);
			((SettlementComponent)obj).Settlement.SetPortPosition(portPosition);
		}
	}

	public Vec2 GetWindAtPosition(Vec2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return _mapScene.GetWindAtPosition(position);
	}

	private void InitializeMapWaterWake()
	{
		_mapScene.SetupWaterWake(128f, 8f);
	}
}
