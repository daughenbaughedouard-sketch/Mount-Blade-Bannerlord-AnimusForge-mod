using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "")]
public class CosmeticRopeManager : ScriptComponentBehavior
{
	private const string RopeScriptEntityTag = "simple_rope_start";

	private const float InvisibleDistanceSquared = 10000f;

	private const float LinearDistanceSquared = 2025f;

	private List<RopeSegment> _cosmeticsRopeSegments = new List<RopeSegment>();

	private bool _ropesWereInvisibleLastFrame;

	private bool _ropesWereLinearLastFrame;

	private bool _lodCheckFirstFrame = true;

	protected override void OnEditorInit()
	{
		FetchEntities();
	}

	protected override void OnInit()
	{
		FetchEntities();
	}

	protected override void OnEditorTick(float dt)
	{
		FetchEntities();
		HandleLOD();
	}

	protected override void OnTickParallel(float dt)
	{
		HandleLOD();
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)4;
	}

	private void FetchEntities()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!((WeakGameEntity)(ref gameEntity)).IsInEditorScene())
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetEntityFlags((EntityFlags)(((WeakGameEntity)(ref gameEntity2)).EntityFlags | 0x20000000));
		}
		_cosmeticsRopeSegments.Clear();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref current)).GetFirstChildEntityWithTag("simple_rope_start");
			if (((WeakGameEntity)(ref firstChildEntityWithTag)).IsValid)
			{
				RopeSegment firstScriptOfType = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstScriptOfType<RopeSegment>();
				if (firstScriptOfType != null)
				{
					_cosmeticsRopeSegments.Add(firstScriptOfType);
				}
			}
		}
	}

	private void HandleLOD()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 lastFinalRenderCameraPositionOfScene = ((WeakGameEntity)(ref gameEntity)).GetLastFinalRenderCameraPositionOfScene();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
		float num = ((Vec3)(ref lastFinalRenderCameraPositionOfScene)).DistanceSquared(origin);
		bool flag = num > 10000f;
		bool flag2 = num > 2025f;
		if (_ropesWereInvisibleLastFrame != flag || _lodCheckFirstFrame)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(!flag);
		}
		if (_ropesWereLinearLastFrame != flag2 || _lodCheckFirstFrame)
		{
			foreach (RopeSegment cosmeticsRopeSegment in _cosmeticsRopeSegments)
			{
				cosmeticsRopeSegment.SetLinearMode(flag2);
			}
		}
		_ropesWereInvisibleLastFrame = flag;
		_ropesWereLinearLastFrame = flag2;
		_lodCheckFirstFrame = false;
	}
}
