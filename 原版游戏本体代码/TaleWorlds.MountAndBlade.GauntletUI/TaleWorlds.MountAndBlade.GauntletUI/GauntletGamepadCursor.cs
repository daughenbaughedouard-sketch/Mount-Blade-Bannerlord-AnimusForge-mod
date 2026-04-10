using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletGamepadCursor : GlobalLayer
{
	private GamepadCursorViewModel _dataSource;

	private GauntletLayer _layer;

	private static GauntletGamepadCursor _current;

	public GauntletGamepadCursor()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		_dataSource = new GamepadCursorViewModel();
		_layer = new GauntletLayer("GamepadCusor", 115001, false);
		_layer.LoadMovie("GamepadCursor", (ViewModel)(object)_dataSource);
		((ScreenLayer)_layer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)0);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_layer;
	}

	public static void Initialize()
	{
		if (_current == null)
		{
			_current = new GauntletGamepadCursor();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)_current, false);
		}
	}

	protected override void OnLateTick(float dt)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		((GlobalLayer)this).OnLateTick(dt);
		if (ScreenManager.IsMouseCursorHidden())
		{
			_dataSource.IsGamepadCursorVisible = true;
			_dataSource.IsConsoleMouseVisible = false;
			Vec2 cursorPosition = GetCursorPosition();
			_dataSource.CursorPositionX = ((Vec2)(ref cursorPosition)).X;
			_dataSource.CursorPositionY = ((Vec2)(ref cursorPosition)).Y;
		}
		else
		{
			_dataSource.IsGamepadCursorVisible = false;
			_dataSource.IsConsoleMouseVisible = false;
		}
	}

	private Vec2 GetCursorPosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vec2 mousePositionPixel = Input.MousePositionPixel;
		Vec2 val = Vec2.One - ScreenManager.UsableArea;
		float num = val.x * Screen.RealScreenResolution.x / 2f;
		float num2 = val.y * Screen.RealScreenResolution.y / 2f;
		return new Vec2(((Vec2)(ref mousePositionPixel)).X - num, ((Vec2)(ref mousePositionPixel)).Y - num2);
	}
}
