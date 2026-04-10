using System.Linq;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.HotKeyCategories;

public class NavalShipControlsHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "NavalShipControlsHotKeyCategory";

	public const string AccelerationAxis = "MovementAxisY";

	public const string TurnAxis = "MovementAxisX";

	public const int ToggleSail = 110;

	public const int ToggleOarsmen = 111;

	public const int ChangeShipCamera = 112;

	public const int SelectShip = 113;

	public const int AttemptBoarding = 114;

	public const int ToggleRangedWeaponOrderMode = 115;

	public NavalShipControlsHotKeyCategory()
		: base("NavalShipControlsHotKeyCategory", 116, (GameKeyContextType)0)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		GameAxisKey val = ((GameKeyContext)GenericGameKeyContext.Current).RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("MovementAxisY"));
		GameAxisKey val2 = ((GameKeyContext)GenericGameKeyContext.Current).RegisteredGameAxisKeys.First((GameAxisKey g) => g.Id.Equals("MovementAxisX"));
		((GameKeyContext)this).RegisterGameAxisKey(val, true);
		((GameKeyContext)this).RegisterGameAxisKey(val2, true);
		((GameKeyContext)this).RegisterGameKey(new GameKey(110, "ToggleSail", "NavalShipControlsHotKeyCategory", (InputKey)44, (InputKey)240, GameKeyMainCategories.ShipControlsCategory), true);
		((GameKeyContext)this).RegisterGameKey(new GameKey(111, "ToggleOarsmen", "NavalShipControlsHotKeyCategory", (InputKey)45, (InputKey)241, GameKeyMainCategories.ShipControlsCategory), true);
		((GameKeyContext)this).RegisterGameKey(new GameKey(112, "ChangeShipCamera", "NavalShipControlsHotKeyCategory", (InputKey)46, (InputKey)243, GameKeyMainCategories.ShipControlsCategory), true);
		((GameKeyContext)this).RegisterGameKey(new GameKey(113, "SelectShip", "NavalShipControlsHotKeyCategory", (InputKey)18, (InputKey)252, GameKeyMainCategories.ShipControlsCategory), true);
		((GameKeyContext)this).RegisterGameKey(new GameKey(114, "AttemptBoarding", "NavalShipControlsHotKeyCategory", (InputKey)19, (InputKey)253, GameKeyMainCategories.ShipControlsCategory), true);
		((GameKeyContext)this).RegisterGameKey(new GameKey(115, "ToggleRangedWeaponOrderMode", "NavalShipControlsHotKeyCategory", (InputKey)225, (InputKey)254, GameKeyMainCategories.ShipControlsCategory), true);
	}
}
