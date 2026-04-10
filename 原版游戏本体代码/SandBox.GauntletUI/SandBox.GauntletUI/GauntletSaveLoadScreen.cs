using System;
using SandBox.View;
using SandBox.ViewModelCollection.SaveLoad;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[OverrideView(typeof(SaveLoadScreen))]
public class GauntletSaveLoadScreen : ScreenBase
{
	private GauntletLayer _gauntletLayer;

	private SaveLoadVM _dataSource;

	private SpriteCategory _spriteCategory;

	private readonly bool _isSaving;

	public GauntletSaveLoadScreen(bool isSaving)
	{
		_isSaving = isSaving;
	}

	protected override void OnInitialize()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		bool isCampaignMapOnStack = LinQuick.FirstOrDefaultQ<GameState>(GameStateManager.Current.GameStates, (Func<GameState, bool>)((GameState s) => s is MapState)) != null;
		_dataSource = new SaveLoadVM(_isSaving, isCampaignMapOnStack);
		_dataSource.SetDeleteInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Delete"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		if (Game.Current != null)
		{
			Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
		}
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_saveload");
		_gauntletLayer = new GauntletLayer("SaveLoadScreen", 1, true);
		_gauntletLayer.LoadMovie("SaveLoadScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		if (BannerlordConfig.ForceVSyncInMenus)
		{
			Utilities.SetForceVsync(true);
		}
		InformationManager.HideAllMessages();
		_dataSource.Initialize();
	}

	protected override void OnPostFrameTick(float dt)
	{
		((ScreenBase)this).OnPostFrameTick(dt);
		UpdateInputRestrictions();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		UpdateInputRestrictions();
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			_dataSource.ExecuteDone();
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("Confirm") && !((ScreenLayer)_gauntletLayer).IsFocusedOnInput())
		{
			_dataSource.ExecuteLoadSave();
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("Delete") && !((ScreenLayer)_gauntletLayer).IsFocusedOnInput())
		{
			_dataSource.DeleteSelectedSave();
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
		}
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		if (Game.Current != null)
		{
			Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
		}
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_spriteCategory.Unload();
		Utilities.SetForceVsync(false);
	}

	private void UpdateInputRestrictions()
	{
		if (_dataSource.IsBusyWithAnAction)
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		}
		else
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}
}
