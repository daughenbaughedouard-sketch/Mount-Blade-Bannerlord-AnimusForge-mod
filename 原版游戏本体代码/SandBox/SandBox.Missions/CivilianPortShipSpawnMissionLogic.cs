using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions;

public class CivilianPortShipSpawnMissionLogic : MissionLogic
{
	private const string ShipyardShipSpawnPointTag = "shipyard_ship";

	private Queue<GameEntity> _shipyardShipSpawnPoints = new Queue<GameEntity>();

	private List<Ship> _mainPartyShips = new List<Ship>();

	private List<Ship> _townLordShips = new List<Ship>();

	private Dictionary<GameEntity, MatrixFrame> _spawnedShipVisuals = new Dictionary<GameEntity, MatrixFrame>();

	public CivilianPortShipSpawnMissionLogic(List<Ship> mainPartyShips, List<Ship> townLordShips)
	{
		_mainPartyShips = mainPartyShips;
		_townLordShips = townLordShips;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("shipyard_ship"))
		{
			_shipyardShipSpawnPoints.Enqueue(item);
		}
	}

	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_shipyardShipSpawnPoints))
		{
			if (!Extensions.IsEmpty<Ship>((IEnumerable<Ship>)_mainPartyShips))
			{
				Ship randomElement = Extensions.GetRandomElement<Ship>((IReadOnlyList<Ship>)_mainPartyShips);
				SpawnShip(randomElement);
			}
			while (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_shipyardShipSpawnPoints) && !Extensions.IsEmpty<Ship>((IEnumerable<Ship>)_townLordShips))
			{
				Ship randomElement2 = Extensions.GetRandomElement<Ship>((IReadOnlyList<Ship>)_townLordShips);
				_townLordShips.Remove(randomElement2);
				SpawnShip(randomElement2);
			}
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		foreach (KeyValuePair<GameEntity, MatrixFrame> spawnedShipVisual in _spawnedShipVisuals)
		{
			TickShipAnimation(dt, spawnedShipVisual.Key, spawnedShipVisual.Value);
		}
	}

	private void SpawnShip(Ship ship)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		MissionShipObject obj = MBObjectManager.Instance.GetObject<MissionShipObject>(ship.ShipHull.MissionShipObjectId);
		ValueTuple<uint, uint> sailColors = ShipHelper.GetSailColors((IShipOrigin)(object)ship, (IAgent)null);
		uint item = sailColors.Item1;
		uint item2 = sailColors.Item2;
		GameEntity val = VisualShipFactory.CreateVisualShip(obj.Prefab, ((MissionBehavior)this).Mission.Scene, ship.GetShipVisualSlotInfos(), ship.RandomValue, ship.HitPoints / ship.MaxSailHitPoints, item, item2, true);
		MatrixFrame globalFrame = _shipyardShipSpawnPoints.Dequeue().GetGlobalFrame();
		float waterLevelAtPosition = ((MissionBehavior)this).Mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref globalFrame.origin)).AsVec2, true, true);
		globalFrame.origin.z = waterLevelAtPosition;
		if (val != null)
		{
			val.SetFrame(ref globalFrame, true);
		}
		_spawnedShipVisuals.Add(val, globalFrame);
	}

	private void TickShipAnimation(float dt, GameEntity shipVisualEntity, in MatrixFrame initialFrame)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		if (shipVisualEntity == (GameEntity)null)
		{
			return;
		}
		MatrixFrame frame = shipVisualEntity.GetFrame();
		Vec3 val = shipVisualEntity.GetBoundingBoxMin() + new Vec3(5f, 5f, 0f, -1f);
		Vec3 val2 = shipVisualEntity.GetBoundingBoxMax() - new Vec3(5f, 5f, 0f, -1f);
		Vec2[] array = (Vec2[])(object)new Vec2[32];
		for (int i = 0; i < 4; i++)
		{
			float num = (float)i / 3f;
			float num2 = MathF.Lerp(val.x, val2.x, num, 1E-05f);
			for (int j = 0; j < 8; j++)
			{
				float num3 = (float)j / 7f;
				float num4 = MathF.Lerp(val.y, val2.y, num3, 1E-05f);
				Vec3 val3 = frame.origin + new Vec3(num2, num4, 0f, -1f);
				int num5 = i * 8 + j;
				array[num5] = ((Vec3)(ref val3)).AsVec2;
			}
		}
		Vec3 val4 = Vec3.Zero;
		float num6 = 0f;
		float[] array2 = new float[array.Length];
		Vec3[] array3 = (Vec3[])(object)new Vec3[array.Length];
		((MissionBehavior)this).Mission.Scene.GetBulkWaterLevelAtPositions(array, ref array2, ref array3);
		for (int k = 0; k < array3.Length; k++)
		{
			Vec3 val5 = array3[k];
			val4 += val5;
			num6 += array2[k];
		}
		((Vec3)(ref val4)).Normalize();
		num6 /= (float)array3.Length;
		MatrixFrame val6 = initialFrame;
		val6.origin.z = num6 + 0.5f;
		Mat3 identity = Mat3.Identity;
		identity.u = val4;
		((Vec3)(ref identity.u)).Normalize();
		identity.s = Vec3.CrossProduct(Vec3.Forward, identity.u);
		((Vec3)(ref identity.s)).Normalize();
		identity.f = Vec3.CrossProduct(identity.u, identity.s);
		((Vec3)(ref identity.f)).Normalize();
		val6.rotation = identity;
		MatrixFrame val7 = MatrixFrame.Slerp(ref frame, ref val6, dt * 1.5f);
		shipVisualEntity.SetFrame(ref val7, true);
	}
}
