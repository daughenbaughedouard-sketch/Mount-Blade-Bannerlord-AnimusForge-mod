using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.DedicatedCustomServer.ClientHelper;

internal static class ModLogger
{
	public static void Log(string message, int logLevel = 0, DebugColor color = (DebugColor)4)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("DCS Client Helper :: " + message, logLevel, color, 17592186044416uL);
	}

	public static void Warn(string message)
	{
		Log(message, 0, (DebugColor)9);
	}
}
