using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI;

public class NavalDLCGauntletUISubModule : MBSubModuleBase
{
	private const int NumberOfWaitFramesToLoad = 5;

	private bool _initializedLoadingCategory;

	private bool _loadBackgroundCategory;

	private int _frameCounterToLoad;

	private SpriteCategory _fullBackgroundsCategory;

	protected override void OnApplicationTick(float dt)
	{
		((MBSubModuleBase)this).OnApplicationTick(dt);
		if (!_initializedLoadingCategory)
		{
			LoadingWindow.InitializeWith<GauntletNavalLoadingWindowManager>();
			_initializedLoadingCategory = true;
		}
		if (!_loadBackgroundCategory && 5 == _frameCounterToLoad)
		{
			_fullBackgroundsCategory = UIResourceManager.LoadSpriteCategory("ui_naval_fullbackgrounds");
			_loadBackgroundCategory = true;
		}
		else if (!_loadBackgroundCategory)
		{
			_frameCounterToLoad++;
		}
	}

	protected override void OnSubModuleLoad()
	{
		((MBSubModuleBase)this).OnSubModuleLoad();
		GauntletGameVersionView.AddModuleVersionInfo("War Sails", NavalVersion.GetApplicationVersionBuildNumber());
	}

	protected override void OnSubModuleUnloaded()
	{
		((MBSubModuleBase)this).OnSubModuleUnloaded();
		SpriteCategory fullBackgroundsCategory = _fullBackgroundsCategory;
		if (fullBackgroundsCategory != null)
		{
			fullBackgroundsCategory.Unload();
		}
		GauntletGameVersionView.RemoveModuleVersionInfo("War Sails");
	}
}
