using SandBox.View.Map;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapBasicView))]
public class GauntletMapBasicView : MapView
{
	public GauntletLayer GauntletLayer { get; private set; }

	public GauntletLayer GauntletNameplateLayer { get; private set; }

	protected override void CreateLayout()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		base.CreateLayout();
		GauntletLayer = new GauntletLayer("MapMenuView", 100, false);
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		((ScreenBase)base.MapScreen).AddLayer((ScreenLayer)(object)GauntletLayer);
		GauntletNameplateLayer = new GauntletLayer("MapNameplateLayer", 90, false);
		((ScreenLayer)GauntletNameplateLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)5);
		((ScreenBase)base.MapScreen).AddLayer((ScreenLayer)(object)GauntletNameplateLayer);
	}

	protected override void OnMapConversationStart()
	{
		base.OnMapConversationStart();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)GauntletLayer, true);
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)GauntletNameplateLayer, true);
	}

	protected override void OnMapConversationOver()
	{
		base.OnMapConversationOver();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)GauntletLayer, false);
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)GauntletNameplateLayer, false);
	}

	protected override void OnFinalize()
	{
		((ScreenBase)base.MapScreen).RemoveLayer((ScreenLayer)(object)GauntletLayer);
		GauntletLayer = null;
		base.OnFinalize();
	}
}
