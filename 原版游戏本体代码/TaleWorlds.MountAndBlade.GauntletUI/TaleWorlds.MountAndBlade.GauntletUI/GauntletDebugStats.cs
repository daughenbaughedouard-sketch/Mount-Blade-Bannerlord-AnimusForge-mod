using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletDebugStats : GlobalLayer
{
	private DebugStatsVM _dataSource;

	public void Initialize()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		_dataSource = new DebugStatsVM();
		GauntletLayer val = new GauntletLayer("DebugStats", 30000, false);
		val.LoadMovie("DebugStats", (ViewModel)(object)_dataSource);
		((ScreenLayer)val).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)0);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)val;
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)this, true);
	}
}
