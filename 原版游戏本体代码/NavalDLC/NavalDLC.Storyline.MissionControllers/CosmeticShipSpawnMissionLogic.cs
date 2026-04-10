using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline.MissionControllers;

public class CosmeticShipSpawnMissionLogic : MissionLogic
{
	private const string CosmeticShipSpawnPointTag = "cosmetic_ship_spawn_point";

	private const float AnimationSpeedMultiplier = 0.1f;

	private List<string> _cosmeticShipIdList = new List<string> { "nord_medium_ship", "khuzait_heavy_ship", "eastern_medium_ship", "empire_trade_ship" };

	private Queue<GameEntity> _cosmeticShipSpawnPointEntities = new Queue<GameEntity>();

	private Dictionary<GameEntity, MatrixFrame> _spawnedShipVisuals = new Dictionary<GameEntity, MatrixFrame>();

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("cosmetic_ship_spawn_point"))
		{
			_cosmeticShipSpawnPointEntities.Enqueue(item);
		}
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		while (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_cosmeticShipSpawnPointEntities))
		{
			ShipHull shipHull = MBObjectManager.Instance.GetObject<ShipHull>(Extensions.GetRandomElement<string>((IReadOnlyList<string>)_cosmeticShipIdList));
			SpawnShip(shipHull);
		}
	}

	private void SpawnShip(ShipHull shipHull)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		MissionShipObject obj = MBObjectManager.Instance.GetObject<MissionShipObject>(shipHull.MissionShipObjectId);
		uint num = 4291609515u;
		uint num2 = 4291609515u;
		GameEntity val = VisualShipFactory.CreateVisualShip(obj.Prefab, ((MissionBehavior)this).Mission.Scene, new List<ShipVisualSlotInfo>(), MBRandom.RandomInt(), 1f, num, num2, true);
		MatrixFrame globalFrame = _cosmeticShipSpawnPointEntities.Dequeue().GetGlobalFrame();
		((Mat3)(ref globalFrame.rotation)).MakeUnit();
		float waterLevelAtPosition = ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref globalFrame.origin)).AsVec2, true, true);
		globalFrame.origin.z = waterLevelAtPosition;
		val.SetFrame(ref globalFrame, true);
		List<SailVisual> sailVisuals = new List<SailVisual>();
		CollectSailVisuals(val.WeakEntity, sailVisuals);
		FoldSails(sailVisuals);
		_spawnedShipVisuals.Add(val, globalFrame);
	}

	private void CollectSailVisuals(WeakGameEntity shipEntity, List<SailVisual> sailVisuals)
	{
		sailVisuals.Clear();
		ShipVisual firstScriptOfType = ((WeakGameEntity)(ref shipEntity)).GetFirstScriptOfType<ShipVisual>();
		if (firstScriptOfType == null)
		{
			return;
		}
		foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
		{
			if (sailVisual2 is SailVisual sailVisual)
			{
				sailVisual.SailEnabled = false;
				sailVisual.SetFoldSailStepMultiplier(0.3f);
				sailVisual.SetFoldSailDuration(0.4f);
				sailVisual.SetUnfoldSailDuration(0.2f);
				sailVisual.FoldAnimationEnabled = false;
				sailVisuals.Add(sailVisual);
			}
		}
	}

	private void FoldSails(List<SailVisual> sailVisuals)
	{
		foreach (SailVisual sailVisual in sailVisuals)
		{
			sailVisual.SailEnabled = false;
		}
	}
}
