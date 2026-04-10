using System;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Input;

public class InputKeyItemVM : ViewModel
{
	private bool _isVisibleToConsoleOnly;

	private TextObject _forcedName;

	private string _forcedID;

	private bool? _forcedVisibility;

	private string _keyID;

	private string _keyName;

	private bool _isVisible;

	public GameKey GameKey { get; private set; }

	public HotKey HotKey { get; private set; }

	[DataSourceProperty]
	public string KeyID
	{
		get
		{
			return _keyID;
		}
		set
		{
			if (value != _keyID)
			{
				_keyID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "KeyID");
			}
		}
	}

	[DataSourceProperty]
	public string KeyName
	{
		get
		{
			return _keyName;
		}
		set
		{
			if (value != _keyName)
			{
				_keyName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "KeyName");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (value != _isVisible)
			{
				_isVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisible");
			}
		}
	}

	private InputKeyItemVM()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		HotKeyManager.OnKeybindsChanged += new OnKeybindsChangedEvent(OnKeybindsChanged);
	}

	public override void OnFinalize()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		((ViewModel)this).OnFinalize();
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveStateChanged));
		HotKeyManager.OnKeybindsChanged -= new OnKeybindsChangedEvent(OnKeybindsChanged);
	}

	private void OnGamepadActiveStateChanged()
	{
		ForceRefresh();
	}

	private void OnKeybindsChanged()
	{
		ForceRefresh();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		ForceRefresh();
	}

	public void SetForcedVisibility(bool? isVisible)
	{
		_forcedVisibility = isVisible;
		UpdateVisibility();
	}

	private void ForceRefresh()
	{
		UpdateVisibility();
		KeyID = string.Empty;
		KeyName = string.Empty;
		if (_forcedID != null)
		{
			KeyID = _forcedID;
			KeyName = ((object)_forcedName)?.ToString() ?? string.Empty;
		}
		else
		{
			KeyID = GetKeyId();
			KeyName = ((object)GetKeyName()).ToString();
		}
	}

	private string GetKeyId()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (Input.IsGamepadActive)
		{
			if (GameKey != null)
			{
				Key controllerKey = GameKey.ControllerKey;
				if (controllerKey == null)
				{
					return null;
				}
				return ((object)controllerKey.InputKey/*cast due to .constrained prefix*/).ToString();
			}
			if (HotKey != null)
			{
				Key? obj = HotKey.Keys.Find((Key k) => k.IsControllerInput);
				if (obj == null)
				{
					return null;
				}
				return ((object)obj.InputKey/*cast due to .constrained prefix*/).ToString();
			}
		}
		if (GameKey != null)
		{
			Key keyboardKey = GameKey.KeyboardKey;
			if (keyboardKey == null)
			{
				return null;
			}
			return ((object)keyboardKey.InputKey/*cast due to .constrained prefix*/).ToString();
		}
		if (HotKey != null)
		{
			Key? obj2 = HotKey.Keys.Find((Key k) => !k.IsControllerInput);
			if (obj2 == null)
			{
				return null;
			}
			return ((object)obj2.InputKey/*cast due to .constrained prefix*/).ToString();
		}
		return string.Empty;
	}

	private TextObject GetKeyName()
	{
		if (_forcedName != (TextObject)null)
		{
			return _forcedName;
		}
		if (Game.Current != null)
		{
			if (HotKey != null)
			{
				return Game.Current.GameTextManager.FindText("str_key_name", HotKey.GroupId + "_" + HotKey.Id);
			}
			if (GameKey != null)
			{
				return Game.Current.GameTextManager.FindText("str_key_name", GameKey.GroupId + "_" + GameKey.StringId);
			}
		}
		return TextObject.GetEmpty();
	}

	private void UpdateVisibility()
	{
		IsVisible = _forcedVisibility ?? (!_isVisibleToConsoleOnly || Input.IsGamepadActive);
	}

	public static InputKeyItemVM CreateFromGameKey(GameKey gameKey, bool isConsoleOnly)
	{
		InputKeyItemVM inputKeyItemVM = new InputKeyItemVM();
		inputKeyItemVM.GameKey = gameKey;
		inputKeyItemVM._isVisibleToConsoleOnly = isConsoleOnly;
		inputKeyItemVM.ForceRefresh();
		return inputKeyItemVM;
	}

	public static InputKeyItemVM CreateFromHotKey(HotKey hotKey, bool isConsoleOnly)
	{
		InputKeyItemVM inputKeyItemVM = new InputKeyItemVM();
		inputKeyItemVM.HotKey = hotKey;
		inputKeyItemVM._isVisibleToConsoleOnly = isConsoleOnly;
		inputKeyItemVM.ForceRefresh();
		return inputKeyItemVM;
	}

	public static InputKeyItemVM CreateFromHotKeyWithForcedName(HotKey hotKey, TextObject forcedName, bool isConsoleOnly)
	{
		InputKeyItemVM inputKeyItemVM = new InputKeyItemVM();
		inputKeyItemVM.HotKey = hotKey;
		inputKeyItemVM._isVisibleToConsoleOnly = isConsoleOnly;
		inputKeyItemVM._forcedName = forcedName;
		inputKeyItemVM.ForceRefresh();
		return inputKeyItemVM;
	}

	public static InputKeyItemVM CreateFromGameKeyWithForcedName(GameKey gameKey, TextObject forcedName, bool isConsoleOnly)
	{
		InputKeyItemVM inputKeyItemVM = new InputKeyItemVM();
		inputKeyItemVM.GameKey = gameKey;
		inputKeyItemVM._isVisibleToConsoleOnly = isConsoleOnly;
		inputKeyItemVM._forcedName = forcedName;
		inputKeyItemVM.ForceRefresh();
		return inputKeyItemVM;
	}

	public static InputKeyItemVM CreateFromForcedID(string forcedID, TextObject forcedName, bool isConsoleOnly)
	{
		InputKeyItemVM inputKeyItemVM = new InputKeyItemVM();
		inputKeyItemVM._forcedID = forcedID;
		inputKeyItemVM._forcedName = forcedName;
		inputKeyItemVM._isVisibleToConsoleOnly = isConsoleOnly;
		inputKeyItemVM.ForceRefresh();
		return inputKeyItemVM;
	}
}
