using System.Collections.Generic;
using System.Linq;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.HotKeyCategories;

public class PortHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "PortHotKeyCategory";

	public const string SelectLeftRoster = "SelectLeftRoster";

	public const string SelectRightRoster = "SelectRightRoster";

	public const string ToggleCameraMovement = "ToggleCameraMovement";

	public const string ResetCamera = "ResetCamera";

	public const string ControllerDeviateLeft = "ControllerDeviateLeft";

	public const string ControllerDeviateRight = "ControllerDeviateRight";

	public const string ControllerZoomIn = "ControllerZoomIn";

	public const string ControllerZoomOut = "ControllerZoomOut";

	public const string ControllerHorizontalRotationAxis = "CameraAxisX";

	public const string ControllerVerticalRotationAxis = "CameraAxisY";

	public const string CameraTargetDeviationAxis = "MovementAxisX";

	public const string ZoomAxis = "MovementAxisY";

	public PortHotKeyCategory()
		: base("PortHotKeyCategory", 0, (GameKeyContextType)0)
	{
		RegisterHotKeys();
		RegisterGameAxisKeys();
	}

	private void RegisterHotKeys()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		((GameKeyContext)this).RegisterHotKey(new HotKey("SelectLeftRoster", "PortHotKeyCategory", (InputKey)254, (Modifiers)0, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("SelectRightRoster", "PortHotKeyCategory", (InputKey)255, (Modifiers)0, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("ControllerDeviateLeft", "PortHotKeyCategory", (InputKey)248, (Modifiers)0, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("ControllerDeviateRight", "PortHotKeyCategory", (InputKey)249, (Modifiers)0, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("ControllerZoomIn", "PortHotKeyCategory", (InputKey)255, (Modifiers)0, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("ControllerZoomOut", "PortHotKeyCategory", (InputKey)254, (Modifiers)0, (Modifiers)0), true);
		List<Key> list = new List<Key>
		{
			new Key((InputKey)225),
			new Key((InputKey)252)
		};
		((GameKeyContext)this).RegisterHotKey(new HotKey("ToggleCameraMovement", "PortHotKeyCategory", list, (Modifiers)0, (Modifiers)0), true);
		list = new List<Key>
		{
			new Key((InputKey)19),
			new Key((InputKey)253)
		};
		((GameKeyContext)this).RegisterHotKey(new HotKey("ResetCamera", "PortHotKeyCategory", list, (Modifiers)0, (Modifiers)0), true);
	}

	private void RegisterGameAxisKeys()
	{
		GameAxisKey val = ((GameKeyContext)GenericGameKeyContext.Current).RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("CameraAxisX"));
		GameAxisKey val2 = ((GameKeyContext)GenericGameKeyContext.Current).RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("CameraAxisY"));
		GameAxisKey val3 = ((GameKeyContext)GenericGameKeyContext.Current).RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("MovementAxisX"));
		GameAxisKey val4 = ((GameKeyContext)GenericGameKeyContext.Current).RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("MovementAxisY"));
		((GameKeyContext)this).RegisterGameAxisKey(val, true);
		((GameKeyContext)this).RegisterGameAxisKey(val2, true);
		((GameKeyContext)this).RegisterGameAxisKey(val3, true);
		((GameKeyContext)this).RegisterGameAxisKey(val4, true);
	}
}
