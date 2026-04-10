using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.Objects;

internal class ShipClothFixer : ScriptComponentBehavior
{
	private struct ClothData
	{
		internal ClothSimulatorComponent ClothComponent;

		internal MatrixFrame ShipLocalFrame;
	}

	private List<ClothData> _shipCloths = new List<ClothData>();

	private MatrixFrame _prevPrevShipFrame = MatrixFrame.Identity;

	private MatrixFrame _prevShipFrame = MatrixFrame.Identity;

	private float _fixedDt;

	private int _frameCounter;

	private ShipClothFixer()
	{
	}//IL_000c: Unknown result type (might be due to invalid IL or missing references)
	//IL_0011: Unknown result type (might be due to invalid IL or missing references)
	//IL_0017: Unknown result type (might be due to invalid IL or missing references)
	//IL_001c: Unknown result type (might be due to invalid IL or missing references)


	protected override void OnEditorInit()
	{
		FetchClothComponents();
	}

	protected override void OnInit()
	{
		FetchClothComponents();
	}

	protected override void OnEditorTick(float dt)
	{
		foreach (ClothData shipCloth in _shipCloths)
		{
			SetPrevFrameToCloth(shipCloth);
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)36;
	}

	protected override void OnParallelFixedTick(float fixedDt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_prevPrevShipFrame = _prevShipFrame;
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		_prevShipFrame = ((WeakGameEntity)(ref val)).GetBodyWorldTransform();
		_fixedDt = fixedDt;
		_frameCounter++;
	}

	protected override void OnTickParallel(float dt)
	{
		foreach (ClothData shipCloth in _shipCloths)
		{
			SetPrevFrameToCloth(shipCloth);
		}
	}

	private void FetchClothComponents()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		_shipCloths.Clear();
		WeakGameEntity val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		val = ((ScriptComponentBehavior)this).GameEntity;
		val = ((WeakGameEntity)(ref val)).Root;
		((WeakGameEntity)(ref val)).GetChildrenRecursive(ref list);
		foreach (WeakGameEntity item3 in list)
		{
			WeakGameEntity current = item3;
			int componentCount = ((WeakGameEntity)(ref current)).GetComponentCount((ComponentType)3);
			for (int i = 0; i < componentCount; i++)
			{
				ClothData item = new ClothData
				{
					ClothComponent = (ClothSimulatorComponent)/*isinst with value type is only supported in some contexts*/
				};
				MatrixFrame globalFrame2 = ((WeakGameEntity)(ref current)).GetGlobalFrame();
				item.ShipLocalFrame = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2);
				_shipCloths.Add(item);
			}
			if ((NativeObject)(object)((WeakGameEntity)(ref current)).Skeleton != (NativeObject)null)
			{
				int componentCount2 = ((WeakGameEntity)(ref current)).Skeleton.GetComponentCount((ComponentType)3);
				for (int j = 0; j < componentCount2; j++)
				{
					ClothData item2 = new ClothData
					{
						ClothComponent = (ClothSimulatorComponent)/*isinst with value type is only supported in some contexts*/
					};
					MatrixFrame globalFrame2 = ((WeakGameEntity)(ref current)).GetGlobalFrame();
					item2.ShipLocalFrame = ((MatrixFrame)(ref globalFrame)).TransformToLocal(ref globalFrame2);
					_shipCloths.Add(item2);
				}
			}
		}
	}

	private void SetPrevFrameToCloth(ClothData clothData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = Vec3.Zero;
		if (_frameCounter > 2)
		{
			val = (((MatrixFrame)(ref _prevShipFrame)).TransformToParent(ref clothData.ShipLocalFrame.origin) - ((MatrixFrame)(ref _prevPrevShipFrame)).TransformToParent(ref clothData.ShipLocalFrame.origin)) / _fixedDt;
		}
		clothData.ClothComponent.SetForcedVelocity(ref val);
	}
}
