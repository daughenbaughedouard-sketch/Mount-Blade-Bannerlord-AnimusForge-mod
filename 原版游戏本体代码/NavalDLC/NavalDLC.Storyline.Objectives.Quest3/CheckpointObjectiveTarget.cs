using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest3;

internal class CheckpointObjectiveTarget : MissionObjectiveTarget
{
	public GameEntity GameEntity { get; private set; }

	public bool Active { get; private set; }

	public VolumeBox VolumeBox { get; private set; }

	public float Radius { get; private set; } = 20f;

	public TextObject Name { get; private set; }

	public CheckpointObjectiveTarget(GameEntity gameEntity)
	{
		GameEntity = gameEntity;
		GameEntity gameEntity2 = GameEntity;
		VolumeBox = ((gameEntity2 != null) ? gameEntity2.GetFirstScriptOfType<VolumeBox>() : null);
		Active = false;
		Name = TextObject.GetEmpty();
	}

	public void SetActive(bool isActive)
	{
		Active = isActive;
	}

	public void SetRadius(float radius)
	{
		Radius = radius;
	}

	public void SetName(TextObject name)
	{
		Name = name;
	}

	public override Vec3 GetGlobalPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GameEntity.GlobalPosition;
	}

	public bool IsInside(Vec3 position)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (VolumeBox != null)
		{
			return VolumeBox.IsPointIn(position);
		}
		Vec3 globalPosition = ((MissionObjectiveTarget)this).GetGlobalPosition();
		return ((Vec3)(ref globalPosition)).DistanceSquared(position) <= Radius * Radius;
	}

	public override TextObject GetName()
	{
		return Name;
	}

	public override bool IsActive()
	{
		return Active;
	}
}
