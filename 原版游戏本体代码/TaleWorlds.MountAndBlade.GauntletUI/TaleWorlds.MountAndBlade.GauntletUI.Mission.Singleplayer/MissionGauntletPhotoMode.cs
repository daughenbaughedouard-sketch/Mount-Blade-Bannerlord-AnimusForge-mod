using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(PhotoModeView))]
public class MissionGauntletPhotoMode : MissionView
{
	private readonly TextObject _screenShotTakenMessage = new TextObject("{=1e12bdjj}Screenshot has been saved in {PATH}", (Dictionary<string, object>)null);

	private const float _cameraRollAmount = 0.1f;

	private const float _cameraFocusAmount = 0.1f;

	private GauntletLayer _gauntletLayer;

	private PhotoModeVM _dataSource;

	private bool _registered;

	private SpriteCategory _photoModeCategory;

	private float _cameraRoll;

	private bool _photoModeOrbitState;

	private bool _suspended = true;

	private bool _vignetteMode;

	private bool _hideAgentsMode;

	private int _takePhoto = -1;

	private bool _saveAmbientOcclusionPass;

	private bool _saveObjectIdPass;

	private bool _saveShadowPass;

	private bool _prevUIDisabled;

	private bool _prevMouseEnabled;

	private Scene _missionScene => base.MissionScreen.Mission.Scene;

	private InputContext _input => ((ScreenLayer)base.MissionScreen.SceneLayer).Input;

	public override void OnMissionScreenInitialize()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_photoModeCategory = UIResourceManager.LoadSpriteCategory("ui_photomode");
		_dataSource = new PhotoModeVM(_missionScene, (Func<bool>)(() => _vignetteMode), (Func<bool>)(() => _hideAgentsMode));
		_cameraRoll = 0f;
		_photoModeOrbitState = _missionScene.GetPhotoModeOrbit();
		_vignetteMode = false;
		_hideAgentsMode = false;
		_saveAmbientOcclusionPass = false;
		_saveObjectIdPass = false;
		_saveShadowPass = false;
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(107));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(93));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(94));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(100));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(99));
		if (_missionScene.ContainsTerrain)
		{
			_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(97));
		}
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(98));
		_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(92));
		if (Utilities.EditModeEnabled)
		{
			_dataSource.AddKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(96));
		}
		_dataSource.AddTakePictureKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetGameKey(95));
		_dataSource.AddFasterCameraKey(HotKeyManager.GetCategory("PhotoModeHotKeyCategory").GetHotKey("FasterCamera"));
		_dataSource.AddHotkeyWithForcedName(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("ToggleEscapeMenu"), new TextObject("{=exitMenuOption}Exit", (Dictionary<string, object>)null));
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Expected O, but got Unknown
		base.OnMissionScreenTick(dt);
		if (_takePhoto != -1)
		{
			if (Utilities.GetNumberOfShaderCompilationsInProgress() > 0)
			{
				_takePhoto++;
			}
			else if (_takePhoto > 6)
			{
				if (_saveObjectIdPass)
				{
					string text = _missionScene.TakePhotoModePicture(false, true, false);
					MBDebug.DisableAllUI = _prevUIDisabled;
					_screenShotTakenMessage.SetTextVariable("PATH", text);
					InformationManager.DisplayMessage(new InformationMessage(((object)_screenShotTakenMessage).ToString()));
					Utilities.SetForceDrawEntityID(false);
					Utilities.SetRenderMode((EngineRenderDisplayMode)0);
				}
				_takePhoto = -1;
			}
			else if (_takePhoto == 2)
			{
				string text2 = _missionScene.TakePhotoModePicture(_saveAmbientOcclusionPass, false, _saveShadowPass);
				_screenShotTakenMessage.SetTextVariable("PATH", text2);
				InformationManager.DisplayMessage(new InformationMessage(((object)_screenShotTakenMessage).ToString()));
				if (_saveObjectIdPass)
				{
					Utilities.SetForceDrawEntityID(true);
					Utilities.SetRenderMode((EngineRenderDisplayMode)14);
					_takePhoto++;
				}
				else
				{
					MBDebug.DisableAllUI = _prevUIDisabled;
					_takePhoto = -1;
				}
			}
			else
			{
				_takePhoto++;
			}
		}
		if (base.MissionScreen.IsPhotoModeEnabled)
		{
			_dataSource.UpdateTakePictureKeyVisibility(GetCanTakePicture());
			_dataSource.UpdateFasterCameraKeyVisibility(GetCanMoveCamera());
			if (_takePhoto != -1)
			{
				return;
			}
			if (!_registered)
			{
				GameKeyContext category = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
				if (!_input.IsCategoryRegistered(category))
				{
					_input.RegisterHotKeyCategory(category);
				}
				GameKeyContext category2 = HotKeyManager.GetCategory("PhotoModeHotKeyCategory");
				if (!_input.IsCategoryRegistered(category2))
				{
					_input.RegisterHotKeyCategory(category2);
				}
				_registered = true;
			}
			if (_suspended)
			{
				_suspended = false;
				_gauntletLayer = new GauntletLayer("MissionPhotoMode", 100000, false);
				((ViewModel)_dataSource).RefreshValues();
				_gauntletLayer.LoadMovie("PhotoMode", (ViewModel)(object)_dataSource);
				((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)3);
				((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
				GauntletChatLogView.Current.SetCanFocusWhileInMission(canFocusInMission: false);
			}
			if (_input.IsGameKeyPressed(95) && GetCanTakePicture())
			{
				_prevUIDisabled = MBDebug.DisableAllUI;
				MBDebug.DisableAllUI = true;
				_saveAmbientOcclusionPass = false;
				_saveObjectIdPass = false;
				_saveShadowPass = false;
				_takePhoto = 0;
			}
			else if (Utilities.EditModeEnabled && _input.IsGameKeyPressed(96))
			{
				_prevUIDisabled = MBDebug.DisableAllUI;
				MBDebug.DisableAllUI = true;
				_saveAmbientOcclusionPass = true;
				_saveObjectIdPass = Utilities.EditModeEnabled;
				_saveShadowPass = true;
				_takePhoto = 0;
			}
			else if (_input.IsGameKeyPressed(92))
			{
				MBDebug.DisableAllUI = !MBDebug.DisableAllUI;
			}
			else if (_input.IsGameKeyPressed(97))
			{
				_photoModeOrbitState = !_photoModeOrbitState;
				_missionScene.SetPhotoModeOrbit(_photoModeOrbitState);
			}
			else if (_input.IsGameKeyPressed(98))
			{
				base.MissionScreen.SetPhotoModeRequiresMouse(!base.MissionScreen.PhotoModeRequiresMouse);
			}
			else if (_input.IsGameKeyPressed(99))
			{
				_vignetteMode = !_vignetteMode;
				_missionScene.SetPhotoModeVignette(_vignetteMode);
			}
			else if (_input.IsGameKeyPressed(100))
			{
				_hideAgentsMode = !_hideAgentsMode;
				Utilities.SetRenderAgents(!_hideAgentsMode);
			}
			else if (_input.IsGameKeyPressed(107))
			{
				ResetChanges();
			}
			else if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsKeyPressed((InputKey)225))
			{
				_prevMouseEnabled = base.MissionScreen.PhotoModeRequiresMouse;
				base.MissionScreen.SetPhotoModeRequiresMouse(isRequired: false);
			}
			else if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsKeyReleased((InputKey)225))
			{
				base.MissionScreen.SetPhotoModeRequiresMouse(_prevMouseEnabled);
			}
			if (_input.IsGameKeyDown(93))
			{
				_cameraRoll -= 0.1f;
				_missionScene.SetPhotoModeRoll(_cameraRoll);
			}
			else if (_input.IsGameKeyDown(94))
			{
				_cameraRoll += 0.1f;
				_missionScene.SetPhotoModeRoll(_cameraRoll);
			}
		}
		else if (!_suspended)
		{
			_suspended = true;
			if (_gauntletLayer != null)
			{
				((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
				_gauntletLayer = null;
			}
			GauntletChatLogView.Current.SetCanFocusWhileInMission(canFocusInMission: true);
		}
	}

	private bool GetCanTakePicture()
	{
		if (Input.IsGamepadActive)
		{
			return !base.MissionScreen.PhotoModeRequiresMouse;
		}
		return true;
	}

	private bool GetCanMoveCamera()
	{
		if (!_photoModeOrbitState)
		{
			if (Input.IsGamepadActive)
			{
				return !base.MissionScreen.PhotoModeRequiresMouse;
			}
			return true;
		}
		return false;
	}

	private void ResetChanges()
	{
		_photoModeOrbitState = false;
		_missionScene.SetPhotoModeOrbit(_photoModeOrbitState);
		_vignetteMode = false;
		_hideAgentsMode = false;
		_saveAmbientOcclusionPass = false;
		_saveObjectIdPass = false;
		_saveShadowPass = false;
		_missionScene.SetPhotoModeFocus(0f, 0f, 0f, 0f);
		_missionScene.SetPhotoModeVignette(_vignetteMode);
		Utilities.SetRenderAgents(!_hideAgentsMode);
		_cameraRoll = 0f;
		_missionScene.SetPhotoModeRoll(_cameraRoll);
		_dataSource.Reset();
	}

	public override bool OnEscape()
	{
		if (base.MissionScreen.IsPhotoModeEnabled)
		{
			base.MissionScreen.SetPhotoModeEnabled(isEnabled: false);
			((MissionBehavior)this).Mission.IsInPhotoMode = false;
			MBDebug.DisableAllUI = false;
			ResetChanges();
			return true;
		}
		return false;
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return !base.MissionScreen.IsPhotoModeEnabled;
	}

	public override void OnMissionScreenFinalize()
	{
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_photoModeCategory.Unload();
		base.OnMissionScreenFinalize();
	}
}
