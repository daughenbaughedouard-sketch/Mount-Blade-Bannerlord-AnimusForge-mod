using System;
using System.Collections.Generic;
using Helpers;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace NavalDLC;

public class MissionShipFactory
{
	private static uint _shipUniqueBitwiseIDNext = 1u;

	public static MissionObject CreateMissionShip(int shipIndex, ShipAssignment shipAssignment, NavalShipsLogic shipsLogic, in MatrixFrame initialFrame)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("MissionShipFactory.CreateMissionShip: " + shipAssignment.MissionShipObject.Prefab, 0, (DebugColor)12, 17592186044416uL);
		MissionObject obj = ((MissionBehavior)shipsLogic).Mission.CreateMissionObjectFromPrefab(shipAssignment.MissionShipObject.Prefab, initialFrame, (Action<GameEntity>)delegate(GameEntity entity)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			CleanNonExistingUpgrades(entity.WeakEntity, shipAssignment.ShipOrigin.GetShipVisualSlotInfos());
			entity.CreateAndAddScriptComponent(typeof(ShipVisual).Name, false);
			ShipVisual firstScriptOfType2 = entity.GetFirstScriptOfType<ShipVisual>();
			firstScriptOfType2.SailColors = ShipHelper.GetSailColors(shipAssignment.ShipOrigin, (IAgent)null);
			firstScriptOfType2.Initialize(shipAssignment.ShipOrigin.RandomValue, shipAssignment.ShipOrigin.CustomSailPatternId);
			firstScriptOfType2.Health = shipAssignment.ShipOrigin.HitPoints / shipAssignment.ShipOrigin.MaxHitPoints;
		});
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)obj).GameEntity;
		MissionShip firstScriptOfType = ((WeakGameEntity)(ref gameEntity)).GetFirstScriptOfType<MissionShip>();
		firstScriptOfType.InitForMission(shipIndex, _shipUniqueBitwiseIDNext, shipAssignment, shipsLogic);
		shipAssignment.SetMissionShip(firstScriptOfType);
		_shipUniqueBitwiseIDNext <<= 1;
		return obj;
	}

	public static void ResetShipUniqueBitwiseIDNext()
	{
		_shipUniqueBitwiseIDNext = 1u;
	}

	public static void CleanNonExistingUpgrades(WeakGameEntity shipEntity, List<ShipVisualSlotInfo> upgrades)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = ((WeakGameEntity)(ref shipEntity)).CollectChildrenEntitiesWithTag("upgrade_slot");
		List<WeakGameEntity> list2 = new List<WeakGameEntity>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			WeakGameEntity val = list[num];
			bool flag = false;
			foreach (ShipVisualSlotInfo upgrade in upgrades)
			{
				if (!((WeakGameEntity)(ref val)).HasTag(upgrade.VisualSlotTag))
				{
					continue;
				}
				for (int num2 = ((WeakGameEntity)(ref val)).ChildCount - 1; num2 >= 0; num2--)
				{
					WeakGameEntity child = ((WeakGameEntity)(ref val)).GetChild(num2);
					if (!((WeakGameEntity)(ref child)).HasTag(upgrade.VisualPieceId))
					{
						if (!((WeakGameEntity)(ref child)).HasTag("base"))
						{
							if (((WeakGameEntity)(ref child)).HasTag("platform"))
							{
								list2.Add(child);
							}
							else
							{
								((WeakGameEntity)(ref child)).Remove(77);
							}
						}
					}
					else
					{
						flag = true;
					}
				}
			}
			bool flag2 = false;
			for (int num3 = ((WeakGameEntity)(ref val)).ChildCount - 1; num3 >= 0; num3--)
			{
				WeakGameEntity child2 = ((WeakGameEntity)(ref val)).GetChild(num3);
				if (((WeakGameEntity)(ref child2)).HasTag("base"))
				{
					if (flag)
					{
						((WeakGameEntity)(ref child2)).Remove(77);
					}
					flag2 = true;
				}
			}
			if (!flag)
			{
				foreach (WeakGameEntity item in list2)
				{
					WeakGameEntity current2 = item;
					((WeakGameEntity)(ref current2)).Remove(77);
				}
				if (flag2)
				{
					for (int num4 = ((WeakGameEntity)(ref val)).ChildCount - 1; num4 >= 0; num4--)
					{
						WeakGameEntity child3 = ((WeakGameEntity)(ref val)).GetChild(num4);
						if (!((WeakGameEntity)(ref child3)).HasTag("base"))
						{
							((WeakGameEntity)(ref child3)).Remove(77);
						}
					}
				}
				else
				{
					((WeakGameEntity)(ref val)).Remove(77);
				}
			}
			list2.Clear();
		}
	}
}
