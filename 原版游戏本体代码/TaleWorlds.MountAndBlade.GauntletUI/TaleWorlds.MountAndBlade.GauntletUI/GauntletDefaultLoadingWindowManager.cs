using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletDefaultLoadingWindowManager : GlobalLayer, ILoadingWindowManager
{
	private GauntletMovieIdentifier _movie;

	private GauntletLayer _gauntletLayer;

	private LoadingWindowViewModel _loadingWindowViewModel;

	private SpriteCategory _sploadingCategory;

	private SpriteCategory _mpLoadingCategory;

	private SpriteCategory _mpBackgroundCategory;

	private bool _isMultiplayer;

	void ILoadingWindowManager.Initialize()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		string spriteCategoryName = GetSpriteCategoryName();
		UIResourceManager.SpriteData.SpriteCategories.ContainsKey(spriteCategoryName);
		_sploadingCategory = UIResourceManager.GetSpriteCategory(spriteCategoryName);
		_sploadingCategory.InitializePartialLoad();
		_loadingWindowViewModel = new LoadingWindowViewModel(LoadImage, UnloadImage);
		_loadingWindowViewModel.Enabled = false;
		_loadingWindowViewModel.SetTotalGenericImageCount(_sploadingCategory.SpriteSheetCount);
		_loadingWindowViewModel.IsNavalDLCEnabled = ModuleHelper.IsModuleActive("NavalDLC");
		bool flag = false;
		_gauntletLayer = new GauntletLayer("LoadingWindow", 115003, flag);
		_movie = _gauntletLayer.LoadMovie("LoadingWindow", (ViewModel)(object)_loadingWindowViewModel);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_gauntletLayer;
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)this, false);
	}

	void ILoadingWindowManager.Destroy()
	{
		if (_gauntletLayer == null)
		{
			Debug.FailedAssert("Trying to destroy loading window but it was not initialized", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletDefaultLoadingWindowManager.cs", "Destroy", 64);
			return;
		}
		LoadingWindowViewModel loadingWindowViewModel = _loadingWindowViewModel;
		if (loadingWindowViewModel != null)
		{
			((ViewModel)loadingWindowViewModel).OnFinalize();
		}
		GauntletLayer gauntletLayer = _gauntletLayer;
		if (gauntletLayer != null)
		{
			gauntletLayer.ReleaseMovie(_movie);
		}
		ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)this);
	}

	void ILoadingWindowManager.EnableLoadingWindow()
	{
		_loadingWindowViewModel.Enabled = true;
		((GlobalLayer)this).Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(((GlobalLayer)this).Layer);
		((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		Utilities.StartLoadingStuckCheckState(720f);
	}

	void ILoadingWindowManager.DisableLoadingWindow()
	{
		_loadingWindowViewModel.Enabled = false;
		((GlobalLayer)this).Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(((GlobalLayer)this).Layer);
		((GlobalLayer)this).Layer.InputRestrictions.ResetInputRestrictions();
		GauntletGamepadNavigationManager instance = GauntletGamepadNavigationManager.Instance;
		if (instance != null)
		{
			instance.SetAllDirty();
		}
		Utilities.EndLoadingStuckCheckState();
	}

	protected virtual string GetSpriteCategoryName()
	{
		return "ui_loading";
	}

	protected override void OnLateTick(float dt)
	{
		((GlobalLayer)this).OnLateTick(dt);
		_loadingWindowViewModel.Update();
	}

	public void SetCurrentModeIsMultiplayer(bool isMultiplayer)
	{
		if (_isMultiplayer != isMultiplayer)
		{
			_isMultiplayer = isMultiplayer;
			_loadingWindowViewModel.IsMultiplayer = isMultiplayer;
			if (isMultiplayer)
			{
				_mpLoadingCategory = UIResourceManager.LoadSpriteCategory("ui_mploading");
				_mpBackgroundCategory = UIResourceManager.LoadSpriteCategory("ui_mpbackgrounds");
			}
			else
			{
				_mpLoadingCategory.Unload();
				_mpBackgroundCategory.Unload();
			}
		}
	}

	private void LoadImage(int index, out string imageName)
	{
		if (_sploadingCategory != null)
		{
			_sploadingCategory.PartialLoadAtIndex((ITwoDimensionResourceContext)(object)UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot, index);
			imageName = _sploadingCategory.SpriteParts[index - 1].Name;
		}
		else
		{
			imageName = string.Empty;
		}
	}

	private void UnloadImage(int index)
	{
		if (_sploadingCategory != null)
		{
			_sploadingCategory.PartialUnloadAtIndex(index);
		}
	}
}
