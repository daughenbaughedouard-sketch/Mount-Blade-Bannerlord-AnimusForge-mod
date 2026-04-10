using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionGenericMarkerTargetVM : MissionNameMarkerTargetBaseVM
{
	public readonly string Identifier;

	private readonly Vec3 _position;

	private readonly TextObject _name;

	public MissionGenericMarkerTargetVM(string identifier, string nameType, string iconType, Vec3 position, TextObject name)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Identifier = identifier;
		base.NameType = nameType;
		base.IconType = iconType;
		_position = position;
		_name = name;
		((ViewModel)this).RefreshValues();
	}

	public override bool Equals(MissionNameMarkerTargetBaseVM other)
	{
		if (other is MissionGenericMarkerTargetVM missionGenericMarkerTargetVM)
		{
			return missionGenericMarkerTargetVM.Identifier == Identifier;
		}
		return false;
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, _position + MissionNameMarkerHelper.DefaultHeightOffset);
	}

	protected override TextObject GetName()
	{
		return _name;
	}
}
