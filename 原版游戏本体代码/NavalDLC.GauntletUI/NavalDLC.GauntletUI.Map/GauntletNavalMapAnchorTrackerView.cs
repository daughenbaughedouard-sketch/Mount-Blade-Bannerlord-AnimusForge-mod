using NavalDLC.View.Map;
using NavalDLC.ViewModelCollection.Map;
using SandBox.View;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.GauntletUI.Map;

[OverrideView(typeof(NavalMapAnchorTrackerView))]
public class GauntletNavalMapAnchorTrackerView : MapView
{
	private GauntletLayer _gauntletLayer;

	private MapAnchorTrackerVM _dataSource;

	protected override void OnMapConversationStart()
	{
		((MapView)this).OnMapConversationStart();
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		}
	}

	protected override void OnMapConversationOver()
	{
		((MapView)this).OnMapConversationOver();
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
		}
	}

	protected override void CreateLayout()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		((MapView)this).CreateLayout();
		_dataSource = new MapAnchorTrackerVM(OnMoveCameraToAnchor);
		_gauntletLayer = new GauntletLayer("NavalAnchorTracker", 15, false);
		((SandboxView)this).Layer = (ScreenLayer)(object)_gauntletLayer;
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)3);
		_gauntletLayer.LoadMovie("AnchorTracker", (ViewModel)(object)_dataSource);
		((ScreenBase)((MapView)this).MapScreen).AddLayer(((SandboxView)this).Layer);
	}

	private void OnMoveCameraToAnchor()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		MobileParty mainParty = MobileParty.MainParty;
		if (mainParty != null)
		{
			AnchorPoint anchor = mainParty.Anchor;
			if (((anchor != null) ? new bool?(anchor.IsValid) : ((bool?)null)) == true)
			{
				((MapView)this).MapScreen.FastMoveCameraToPosition(MobileParty.MainParty.Anchor.Position);
			}
		}
	}

	protected override void OnFinalize()
	{
		((SandboxView)this).OnFinalize();
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)((MapView)this).MapScreen).RemoveLayer(((SandboxView)this).Layer);
	}

	protected override void OnMapScreenUpdate(float dt)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		((MapView)this).OnMapScreenUpdate(dt);
		AnchorPoint anchor = MobileParty.MainParty.Anchor;
		float seeingRange = MobileParty.MainParty.SeeingRange;
		CampaignVec2 position = anchor.Position;
		float num = ((CampaignVec2)(ref position)).Distance(MobileParty.MainParty.Position);
		Vec3 position2 = ((MapView)this).MapScreen.MapCameraView.Camera.Position;
		float num2 = ((Vec3)(ref position2)).Distance(MobileParty.MainParty.GetPositionAsVec3());
		float screenX = -5000f;
		float screenY = -5000f;
		float screenW = -5000f;
		if (anchor != null && anchor.IsValid && !anchor.IsDisabled && (num > seeingRange || num2 >= 110f))
		{
			GetAnchorScreenPosition(anchor, out screenX, out screenY, out screenW);
		}
		_dataSource.IsVisible = screenW >= 0f;
		_dataSource.PositionX = screenX;
		_dataSource.PositionY = screenY;
		_dataSource.PositionW = screenW;
	}

	private void GetAnchorScreenPosition(AnchorPoint anchor, out float screenX, out float screenY, out float screenW)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = anchor.GetPosition();
		screenX = -5000f;
		screenY = -5000f;
		screenW = -1f;
		MBWindowManager.WorldToScreenInsideUsableArea(((MapView)this).MapScreen.MapCameraView.Camera, position, ref screenX, ref screenY, ref screenW);
	}
}
