using System;
using System.Collections.Generic;
using SandBox.ViewModelCollection.Map.Cheat;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionCheatView))]
public class MissionGauntletCheatView : MissionCheatView
{
	private GauntletLayer _gauntletLayer;

	private GameplayCheatsVM _dataSource;

	private bool _isActive;

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((MissionCheatView)this).FinalizeScreen();
	}

	public override bool GetIsCheatsAvailable()
	{
		return true;
	}

	public override void InitializeScreen()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (!_isActive)
		{
			_isActive = true;
			IEnumerable<GameplayCheatBase> missionCheatList = GameplayCheatsManager.GetMissionCheatList();
			_dataSource = new GameplayCheatsVM((Action)((MissionCheatView)this).FinalizeScreen, missionCheatList);
			InitializeKeyVisuals();
			_gauntletLayer = new GauntletLayer("MissionCheat", 4500, false);
			_gauntletLayer.LoadMovie("MapCheats", (ViewModel)(object)_dataSource);
			((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		}
	}

	public override void FinalizeScreen()
	{
		if (_isActive)
		{
			_isActive = false;
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			GameplayCheatsVM dataSource = _dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).OnFinalize();
			}
			_gauntletLayer = null;
			_dataSource = null;
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		if (_isActive)
		{
			HandleInput();
		}
	}

	private void HandleInput()
	{
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource?.ExecuteClose();
		}
	}

	private void InitializeKeyVisuals()
	{
		_dataSource.SetCloseInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
	}
}
