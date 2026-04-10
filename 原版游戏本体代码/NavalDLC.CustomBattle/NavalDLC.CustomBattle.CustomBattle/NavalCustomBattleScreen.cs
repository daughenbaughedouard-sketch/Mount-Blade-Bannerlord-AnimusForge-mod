using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.CustomBattle.CustomBattle;

[GameStateScreen(typeof(NavalCustomBattleState))]
public class NavalCustomBattleScreen : ScreenBase, IGameStateListener
{
	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _gauntletMovie;

	private NavalCustomBattleVM _dataSource;

	private bool _isMovieLoaded;

	private int _isFirstFrameCounter;

	public NavalCustomBattleScreen(NavalCustomBattleState customBattleState)
	{
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}

	protected override void OnInitialize()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		_dataSource = new NavalCustomBattleVM();
		_dataSource.SetStartInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetRandomizeInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Randomize"));
		_dataSource.SetCycleTierInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		_dataSource.TroopTypeSelectionPopUp?.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_gauntletLayer = new GauntletLayer("NavalCustomBattle", 1, true);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		LoadMovie();
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		_dataSource.SetActiveState(isActive: true);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		InformationManager.HideAllMessages();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (_isFirstFrameCounter >= 0)
		{
			if (_isFirstFrameCounter == 0)
			{
				LoadingWindow.DisableGlobalLoadingWindow();
			}
			_isFirstFrameCounter--;
		}
		if (((ScreenLayer)_gauntletLayer).IsFocusedOnInput())
		{
			return;
		}
		NavalCustomBattleTroopTypeSelectionPopUpVM troopTypeSelectionPopUp = _dataSource.TroopTypeSelectionPopUp;
		if (troopTypeSelectionPopUp != null && troopTypeSelectionPopUp.IsOpen)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopTypeSelectionPopUp.ExecuteCancel();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopTypeSelectionPopUp.ExecuteDone();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Reset"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.TroopTypeSelectionPopUp.ExecuteReset();
			}
			return;
		}
		NavalCustomBattleShipSelectionPopUpVM shipSelectionPopUp = _dataSource.ShipSelectionPopUp;
		if (shipSelectionPopUp != null && shipSelectionPopUp.IsOpen)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ShipSelectionPopUp.ExecuteClose();
			}
			return;
		}
		if (_dataSource.FocusedShipItem != null && ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SwitchToNextTab"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.FocusedShipItem.ExecuteCycleUpgradeTier();
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteBack();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Randomize"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteRandomize();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteStart();
		}
	}

	protected override void OnFinalize()
	{
		UnloadMovie();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
		((ScreenBase)this).OnFinalize();
	}

	protected override void OnActivate()
	{
		LoadMovie();
		_dataSource?.SetActiveState(isActive: true);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		_isFirstFrameCounter = 2;
		((ScreenBase)this).OnActivate();
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		UnloadMovie();
		_dataSource?.SetActiveState(isActive: false);
	}

	public override void UpdateLayout()
	{
		((ScreenBase)this).UpdateLayout();
		if (!_isMovieLoaded)
		{
			NavalCustomBattleVM dataSource = _dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).RefreshValues();
			}
		}
	}

	private void LoadMovie()
	{
		if (!_isMovieLoaded)
		{
			_gauntletMovie = _gauntletLayer.LoadMovie("NavalCustomBattleScreen", (ViewModel)(object)_dataSource);
			_isMovieLoaded = true;
		}
	}

	private void UnloadMovie()
	{
		if (_isMovieLoaded)
		{
			_gauntletLayer.ReleaseMovie(_gauntletMovie);
			_gauntletMovie = null;
			_isMovieLoaded = false;
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}
}
