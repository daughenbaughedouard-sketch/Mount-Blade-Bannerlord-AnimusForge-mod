using TaleWorlds.InputSystem;

namespace NavalDLC.HotKeyCategories;

public class NavalCheatsHotKeyCategory : GameKeyContext
{
	public const string CategoryId = "NavalCheatsHotKeyCategory";

	public const string DebugSailingMoveToRight = "DebugSailingMoveToRight";

	public const string DebugSailingMoveToLeft = "DebugSailingMoveToLeft";

	public const string DebugRammingCollision = "DebugRammingCollision";

	public const string DebugDealSiegeEngineDamage = "DebugDealSiegeEngineDamage";

	public const string DebugSetWindDirection = "DebugSetWindDirection";

	public NavalCheatsHotKeyCategory()
		: base("NavalCheatsHotKeyCategory", 0, (GameKeyContextType)1)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		((GameKeyContext)this).RegisterHotKey(new HotKey("DebugSailingMoveToLeft", "NavalCheatsHotKeyCategory", (InputKey)30, (Modifiers)2, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("DebugSailingMoveToRight", "NavalCheatsHotKeyCategory", (InputKey)32, (Modifiers)2, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("DebugRammingCollision", "NavalCheatsHotKeyCategory", (InputKey)19, (Modifiers)3, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("DebugDealSiegeEngineDamage", "NavalCheatsHotKeyCategory", (InputKey)48, (Modifiers)3, (Modifiers)0), true);
		((GameKeyContext)this).RegisterHotKey(new HotKey("DebugSetWindDirection", "NavalCheatsHotKeyCategory", (InputKey)17, (Modifiers)3, (Modifiers)0), true);
	}
}
