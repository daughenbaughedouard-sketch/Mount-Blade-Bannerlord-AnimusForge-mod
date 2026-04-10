using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.CustomBattle;

namespace NavalDLC.CustomBattle;

public class NavalDLCCustomBattleSubModule : MBSubModuleBase
{
	protected override void OnSubModuleLoad()
	{
		((MBSubModuleBase)this).OnSubModuleLoad();
		CustomBattleFactory.RegisterProvider<NavalCustomBattleProvider>();
		TauntUsageManager.Initialize();
	}
}
