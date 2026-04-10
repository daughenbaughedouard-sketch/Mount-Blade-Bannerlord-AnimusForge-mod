using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class KeybindingPopup
{
	private bool _isActiveFirstFrame;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _movie;

	private ScreenBase _targetScreen;

	private Action<Key> _onDone;

	private KeybindingPopupVM _dataSource;

	public bool IsActive { get; private set; }

	public KeybindingPopup(Action<Key> onDone, ScreenBase targetScreen)
	{
		_onDone = onDone;
		_targetScreen = targetScreen;
	}

	public void Tick()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		if (!IsActive)
		{
			return;
		}
		if (Input.IsGamepadActive)
		{
			OnToggle(isActive: false);
		}
		else if (!_isActiveFirstFrame)
		{
			InputKey val = (InputKey)Input.GetFirstKeyReleasedInRange(0);
			if ((int)val != -1)
			{
				_onDone(new Key(val));
			}
		}
		else
		{
			_isActiveFirstFrame = false;
		}
	}

	public void OnToggle(bool isActive)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if (IsActive == isActive)
		{
			return;
		}
		IsActive = isActive;
		if (IsActive)
		{
			_gauntletLayer = new GauntletLayer("KeyBindingPopup", 4005, false);
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
			_dataSource = new KeybindingPopupVM(delegate
			{
				OnToggle(isActive: false);
			});
			_movie = _gauntletLayer.LoadMovie("KeybindingPopup", (ViewModel)(object)_dataSource);
			_targetScreen.AddLayer((ScreenLayer)(object)_gauntletLayer);
			_isActiveFirstFrame = true;
			return;
		}
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		if (_movie != null)
		{
			_gauntletLayer.ReleaseMovie(_movie);
			_movie = null;
		}
		_targetScreen.RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		_dataSource = null;
	}
}
