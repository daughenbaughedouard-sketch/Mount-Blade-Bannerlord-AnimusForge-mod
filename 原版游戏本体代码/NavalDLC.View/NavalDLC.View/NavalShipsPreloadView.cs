using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.View;

public class NavalShipsPreloadView : MissionView
{
	private PreloadHelper _helperInstance = new PreloadHelper();

	public override void OnBehaviorInitialize()
	{
		Mission.Current.Scene.SetDoNotAddEntitiesToTickList(true);
		DefaultNavalMissionLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<DefaultNavalMissionLogic>();
		if (missionBehavior != null)
		{
			if (missionBehavior.PlayerShips != null)
			{
				foreach (IShipOrigin item in (List<IShipOrigin>)(object)missionBehavior.PlayerShips)
				{
					PreloadShip(item);
				}
			}
			if (missionBehavior.PlayerAllyShips != null)
			{
				foreach (IShipOrigin item2 in (List<IShipOrigin>)(object)missionBehavior.PlayerAllyShips)
				{
					PreloadShip(item2);
				}
			}
			if (missionBehavior.PlayerEnemyShips != null)
			{
				foreach (IShipOrigin item3 in (List<IShipOrigin>)(object)missionBehavior.PlayerEnemyShips)
				{
					PreloadShip(item3);
				}
			}
			_helperInstance.PreloadMeshesAndPhysics();
		}
		Mission.Current.Scene.SetDoNotAddEntitiesToTickList(false);
	}

	public override void OnSceneRenderingStarted()
	{
		_helperInstance.WaitForMeshesToBeLoaded();
	}

	public void PreloadShip(IShipOrigin ship)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		MissionShipObject val = MBObjectManager.Instance.GetObject<MissionShipObject>(ship.OriginShipId);
		GameEntity val2 = GameEntity.Instantiate(((MissionBehavior)this).Mission.Scene, val.Prefab, MatrixFrame.Identity, false, "");
		MissionShipFactory.CleanNonExistingUpgrades(val2.WeakEntity, ship.GetShipVisualSlotInfos());
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity weakEntity = val2.WeakEntity;
		((WeakGameEntity)(ref weakEntity)).GetChildrenRecursive(ref list);
		list.Add(val2.WeakEntity);
		_helperInstance.PreloadEntities(list);
		val2.Remove(76);
	}
}
