using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StoryMode.ViewModelCollection.Missions;

public class TrainingObjectiveKeyVM : ViewModel
{
	public enum MovementTypes
	{
		None,
		MoveLeft,
		MoveRight,
		MoveUp,
		MoveDown
	}

	public enum InputTypes
	{
		MouseAndClick,
		Key,
		ControllerStick
	}

	public struct MouseAndClickInput
	{
		public MovementTypes CurrentMovementType;

		public MouseClickTypes CurrentClickType;

		public MouseAndClickInput(MovementTypes movementType, MouseClickTypes mouseClickType)
		{
			CurrentMovementType = movementType;
			CurrentClickType = mouseClickType;
		}
	}

	public struct KeyInput
	{
		public InputKeyItemVM InputKeyItemVM;

		public KeyInput(int gameKeyDefinition, bool isCombatHotKey)
		{
			GameKey val = ((!isCombatHotKey) ? HotKeyManager.GetCategory("Generic").GetGameKey(gameKeyDefinition) : HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey(gameKeyDefinition));
			InputKeyItemVM = InputKeyItemVM.CreateFromGameKey(val, false);
		}
	}

	public struct ControllerStickInput
	{
		public MovementTypes CurrentMovementType;

		public bool IsLeftStick;

		public ControllerStickInput(MovementTypes movementType, bool isLeftStick)
		{
			CurrentMovementType = movementType;
			IsLeftStick = isLeftStick;
		}
	}

	public enum MouseClickTypes
	{
		Left,
		Middle,
		Right
	}

	private InputKeyItemVM _key;

	private string _forcedKeyId;

	private string _forcedKeyName;

	private int _movementType;

	private int _mouseClick;

	private int _inputType;

	[DataSourceProperty]
	public InputKeyItemVM Key
	{
		get
		{
			return _key;
		}
		set
		{
			if (value != _key)
			{
				_key = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "Key");
			}
		}
	}

	[DataSourceProperty]
	public string ForcedKeyId
	{
		get
		{
			return _forcedKeyId;
		}
		set
		{
			if (value != _forcedKeyId)
			{
				_forcedKeyId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ForcedKeyId");
			}
		}
	}

	[DataSourceProperty]
	public string ForcedKeyName
	{
		get
		{
			return _forcedKeyName;
		}
		set
		{
			if (value != _forcedKeyName)
			{
				_forcedKeyName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ForcedKeyName");
			}
		}
	}

	[DataSourceProperty]
	public int MovementType
	{
		get
		{
			return _movementType;
		}
		set
		{
			if (value != _movementType)
			{
				_movementType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MovementType");
			}
		}
	}

	[DataSourceProperty]
	public int MouseClick
	{
		get
		{
			return _mouseClick;
		}
		set
		{
			if (value != _mouseClick)
			{
				_mouseClick = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MouseClick");
			}
		}
	}

	[DataSourceProperty]
	public int InputType
	{
		get
		{
			return _inputType;
		}
		set
		{
			if (value != _inputType)
			{
				_inputType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "InputType");
			}
		}
	}

	public TrainingObjectiveKeyVM(MouseAndClickInput mouseAndClickInput)
	{
		MovementType = (int)mouseAndClickInput.CurrentMovementType;
		MouseClick = (int)mouseAndClickInput.CurrentClickType;
		InputType = 0;
		switch (MouseClick)
		{
		case 0:
			ForcedKeyId = "mouse_left_click";
			break;
		case 1:
			ForcedKeyId = "mouse_middle_click";
			break;
		case 2:
			ForcedKeyId = "mouse_right_click";
			break;
		}
	}

	public TrainingObjectiveKeyVM(ControllerStickInput controllerStickInput)
	{
		MovementType = (int)controllerStickInput.CurrentMovementType;
		InputType = 2;
		OnForcedKeyNameChanged(controllerStickInput);
	}

	public TrainingObjectiveKeyVM(KeyInput keyInput)
	{
		Key = keyInput.InputKeyItemVM;
		InputType = 1;
	}

	private void OnForcedKeyNameChanged(ControllerStickInput controllerStickInput)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		string forcedKeyName = (controllerStickInput.IsLeftStick ? ((object)new TextObject("{=leftstickabbreviated}LS", (Dictionary<string, object>)null)).ToString() : ((object)new TextObject("{=rightstickabbreviated}RS", (Dictionary<string, object>)null)).ToString());
		ForcedKeyName = forcedKeyName;
	}
}
