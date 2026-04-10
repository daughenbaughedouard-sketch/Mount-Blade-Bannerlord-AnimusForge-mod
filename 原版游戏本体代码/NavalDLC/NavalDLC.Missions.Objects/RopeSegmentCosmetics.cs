using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace NavalDLC.Missions.Objects;

[ScriptComponentParams("ship_visual_only", "rope_segment_cosmetics")]
internal class RopeSegmentCosmetics : ScriptComponentBehavior
{
	[EditableScriptComponentVariable(true, "Normalized Location wrt Rope")]
	private float _ropeLocalPosition = 0.5f;

	public bool IsBurningNode { get; private set; }

	public float RopeLocalPosition
	{
		get
		{
			return _ropeLocalPosition;
		}
		set
		{
			_ropeLocalPosition = value;
		}
	}

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		FetchEntities();
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
		FetchEntities();
	}

	protected override void OnEditorTick(float dt)
	{
		FetchEntities();
	}

	private void FetchEntities()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		IsBurningNode = ((WeakGameEntity)(ref gameEntity)).HasTag("burning_node");
	}
}
