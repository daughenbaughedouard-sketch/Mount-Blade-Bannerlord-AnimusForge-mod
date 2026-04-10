using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class LocationItemSpawnHandler : MissionLogic
{
	private Dictionary<ItemObject, GameEntity> _spawnedEntities;

	public override void AfterStart()
	{
		if (CampaignMission.Current.Location != null && CampaignMission.Current.Location.SpecialItems.Count != 0)
		{
			SpawnSpecialItems();
		}
	}

	private void SpawnSpecialItems()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		_spawnedEntities = new Dictionary<ItemObject, GameEntity>();
		List<GameEntity> list = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_special_item").ToList();
		MissionWeapon val = default(MissionWeapon);
		foreach (ItemObject specialItem in CampaignMission.Current.Location.SpecialItems)
		{
			if (list.Count != 0)
			{
				MatrixFrame globalFrame = list[0].GetGlobalFrame();
				((MissionWeapon)(ref val))._002Ector(specialItem, (ItemModifier)null, (Banner)null);
				GameEntity value = ((MissionBehavior)this).Mission.SpawnWeaponWithNewEntity(ref val, (WeaponSpawnFlags)16, globalFrame);
				_spawnedEntities.Add(specialItem, value);
				list.RemoveAt(0);
			}
		}
	}

	public override void OnEntityRemoved(GameEntity entity)
	{
		if (_spawnedEntities == null)
		{
			return;
		}
		foreach (KeyValuePair<ItemObject, GameEntity> spawnedEntity in _spawnedEntities)
		{
			if (spawnedEntity.Value == entity)
			{
				CampaignMission.Current.Location.SpecialItems.Remove(spawnedEntity.Key);
			}
		}
	}
}
