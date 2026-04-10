using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.View.Map;

public class BlockadePositionScript : ScriptComponentBehavior
{
	public int MaximumNumberOfShips = 12;

	public int NumberOfArcs = 4;

	public float DistanceBetweenShips = MathF.PI / 4f;

	public float DistanceRandomizationOnArcs = 0.1f;

	public float DistanceRandomizationBetweenArcs = 0.1f;

	public float Angle = MathF.PI / 2f;

	public string MissionShipId = "dromon_ship_nested";

	public float ShipScaleFactor = 0.052f;

	public bool IsVisualizationEnabled;

	public bool IsRandomizationEnabled;

	public bool IsShipVisualizationEnabled;

	public SimpleButton RefreshVisualization;

	private List<List<Vec3>> _pointsOfArcs;

	private Vec3 _center;

	private List<GameEntity> _shipEntities = new List<GameEntity>();

	protected override void OnEditorTick(float dt)
	{
		if (IsVisualizationEnabled)
		{
			VisualizeArcs();
		}
	}

	private void VisualizeArcs()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (_pointsOfArcs == null || !IsRandomizationEnabled)
		{
			_pointsOfArcs = GetBlockadeArc(MaximumNumberOfShips, out _center);
		}
		if (_pointsOfArcs == null)
		{
			return;
		}
		foreach (List<Vec3> pointsOfArc in _pointsOfArcs)
		{
			foreach (Vec3 item in pointsOfArc)
			{
				_ = item;
			}
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		if (!(variableName == "RefreshVisualization"))
		{
			return;
		}
		_pointsOfArcs = GetBlockadeArc(MaximumNumberOfShips, out _center);
		if (!Extensions.IsEmpty<GameEntity>((IEnumerable<GameEntity>)_shipEntities))
		{
			Utilities.DeleteEntitiesInEditorScene(_shipEntities);
		}
		_shipEntities.Clear();
		if (!IsShipVisualizationEnabled)
		{
			return;
		}
		foreach (List<Vec3> pointsOfArc in _pointsOfArcs)
		{
			foreach (Vec3 item in pointsOfArc)
			{
				Vec3 current = item;
				Vec2 val = ((Vec3)(ref current)).AsVec2 - ((Vec3)(ref _center)).AsVec2;
				MatrixFrame identity = MatrixFrame.Identity;
				identity.origin = current;
				float num = ((Vec2)(ref val)).AngleBetween(((Vec3)(ref identity.rotation.f)).AsVec2);
				((MatrixFrame)(ref identity)).Rotate(MathF.PI / 2f - num, ref Vec3.Up);
				((Mat3)(ref identity.rotation)).ApplyScaleLocal(ShipScaleFactor);
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				GameEntity val2 = GameEntity.Instantiate(((WeakGameEntity)(ref gameEntity)).Scene, MissionShipId, false, true, "");
				if (val2 == (GameEntity)null)
				{
					break;
				}
				val2.SetFrame(ref identity, true);
				_shipEntities.Add(val2);
			}
		}
	}

	public List<List<Vec3>> GetBlockadeArc(int totalNumberOfShips, out Vec3 center)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		int num = MaximumNumberOfShips;
		if (totalNumberOfShips < num)
		{
			num = totalNumberOfShips;
		}
		List<List<Vec3>> list = new List<List<Vec3>>();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("Blockade_Arc_Start");
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag2 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("Blockade_Arc_End");
		center = Vec3.Invalid;
		if (firstChildEntityWithTag == (GameEntity)null || firstChildEntityWithTag2 == (GameEntity)null)
		{
			return list;
		}
		center = FindCenterOfCircle(((WeakGameEntity)(ref firstChildEntityWithTag2)).GlobalPosition, ((WeakGameEntity)(ref firstChildEntityWithTag)).GlobalPosition);
		Vec3 val = ((WeakGameEntity)(ref firstChildEntityWithTag2)).GlobalPosition - center;
		((Vec3)(ref val)).Normalize();
		Vec3 val2 = val;
		int num2 = NumberOfArcs;
		Vec3 globalPosition = ((WeakGameEntity)(ref firstChildEntityWithTag2)).GlobalPosition;
		float num3 = ((Vec3)(ref globalPosition)).Distance(center) / (float)NumberOfArcs;
		int num4 = 0;
		float num5 = DistanceBetweenShips;
		while (num2 > 0)
		{
			int num6 = MathF.Round(Angle * (float)num2 / DistanceBetweenShips);
			if (num - num4 < num6)
			{
				num5 *= (float)num6 / (float)(num - num4);
				num6 = num - num4;
			}
			((Vec3)(ref val2)).RotateAboutZ(num5 / (float)(num2 * 2));
			List<Vec3> list2 = new List<Vec3>();
			for (int i = 0; i < num6; i++)
			{
				float num7 = MBRandom.RandomFloatRanged(0f - DistanceRandomizationOnArcs, DistanceRandomizationOnArcs);
				float num8 = MBRandom.RandomFloatRanged(0f, DistanceRandomizationBetweenArcs);
				Vec3 val3 = center + val2 * (float)num2 * num3;
				((Vec3)(ref val2)).RotateAboutZ(num5 / (float)num2);
				if (IsRandomizationEnabled)
				{
					val3 += val2 * num8;
					((Vec3)(ref val2)).RotateAboutZ(num7);
				}
				list2.Add(val3);
				num4++;
				if (num4 >= num)
				{
					break;
				}
			}
			list.Add(list2);
			if (num4 >= num)
			{
				return list;
			}
			val2 = val;
			num2--;
		}
		return list;
	}

	private Vec3 FindCenterOfCircle(Vec3 arcPointStart, Vec3 arcPointEnd)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = arcPointEnd + (arcPointStart - arcPointEnd) / 2f;
		Vec3 val2 = (arcPointStart - arcPointEnd) / 2f;
		float num = ((Vec3)(ref arcPointEnd)).Distance(val);
		float num2 = num / MathF.Tan(Angle / 2f);
		return new Vec3(((Vec3)(ref val)).X + num2 * ((Vec3)(ref val2)).Y / num, ((Vec3)(ref val)).Y - num2 * ((Vec3)(ref val2)).X / num, ((Vec3)(ref arcPointStart)).Z, -1f);
	}
}
