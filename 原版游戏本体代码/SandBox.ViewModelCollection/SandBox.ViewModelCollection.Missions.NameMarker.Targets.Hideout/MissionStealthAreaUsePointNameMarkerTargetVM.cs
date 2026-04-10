using System.Collections.Generic;
using SandBox.Objects.Usables;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets.Hideout;

public class MissionStealthAreaUsePointNameMarkerTargetVM : MissionNameMarkerTargetBaseVM
{
	private StealthAreaUsePoint _usePoint;

	public MissionStealthAreaUsePointNameMarkerTargetVM(StealthAreaUsePoint usePoint)
	{
		_usePoint = usePoint;
		base.IconType = "call_troops";
		base.NameType = "Normal";
		((ViewModel)this).RefreshValues();
	}

	public override bool Equals(MissionNameMarkerTargetBaseVM other)
	{
		return false;
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_usePoint).GameEntity;
		MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		UpdatePositionWith(missionCamera, globalFrame.origin + Vec3.Up * 0.5f);
	}

	protected override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=GmjiZk9P}Call Troops", (Dictionary<string, object>)null);
	}
}
