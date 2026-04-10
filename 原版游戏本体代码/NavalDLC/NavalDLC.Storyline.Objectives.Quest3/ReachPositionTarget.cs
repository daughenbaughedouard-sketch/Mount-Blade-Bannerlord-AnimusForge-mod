using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Objectives;

namespace NavalDLC.Storyline.Objectives.Quest3;

internal class ReachPositionTarget : MissionObjectiveTarget
{
	private readonly Vec3 _position;

	private readonly TextObject _name;

	internal ReachPositionTarget(Vec3 escapePosition, TextObject name)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		_name = name;
		_position = escapePosition;
	}

	public override Vec3 GetGlobalPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return _position + Vec3.Up * 3f;
	}

	public override TextObject GetName()
	{
		return _name;
	}

	public override bool IsActive()
	{
		return true;
	}
}
