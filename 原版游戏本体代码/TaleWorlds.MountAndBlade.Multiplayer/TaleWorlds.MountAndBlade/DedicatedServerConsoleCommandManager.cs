using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network;

namespace TaleWorlds.MountAndBlade;

public static class DedicatedServerConsoleCommandManager
{
	private static readonly List<Type> _commandHandlerTypes;

	static DedicatedServerConsoleCommandManager()
	{
		_commandHandlerTypes = new List<Type>();
		AddType(typeof(DedicatedServerConsoleCommandManager));
	}

	public static void AddType(Type type)
	{
		_commandHandlerTypes.Add(type);
	}

	internal static void HandleConsoleCommand(string command)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Invalid comparison between Unknown and I4
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Invalid comparison between Unknown and I4
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Invalid comparison between Unknown and I4
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		int num = command.IndexOf(' ');
		string text = "";
		string text2;
		if (num > 0)
		{
			text2 = command.Substring(0, num);
			text = command.Substring(num + 1);
		}
		else
		{
			text2 = command;
		}
		bool flag = false;
		OptionType val = default(OptionType);
		MultiplayerOptionsProperty val2 = default(MultiplayerOptionsProperty);
		if (MultiplayerOptions.TryGetOptionTypeFromString(text2, ref val, ref val2))
		{
			if (text == "?")
			{
				Debug.Print(string.Concat("--", val, ": ", val2.Description), 0, (DebugColor)12, 17179869184uL);
				Debug.Print("--" + (val2.HasBounds ? ("Min: " + val2.BoundsMin + ", Max: " + val2.BoundsMax + ". ") : "") + "Current value: " + MultiplayerOptionsExtensions.GetValueText(val, (MultiplayerOptionsAccessMode)1), 0, (DebugColor)12, 17179869184uL);
			}
			else if (text != "")
			{
				if ((int)val2.OptionValueType == 3)
				{
					MultiplayerOptionsExtensions.SetValue(val, text, (MultiplayerOptionsAccessMode)1);
				}
				else if ((int)val2.OptionValueType == 1)
				{
					if (int.TryParse(text, out var result))
					{
						MultiplayerOptionsExtensions.SetValue(val, result, (MultiplayerOptionsAccessMode)1);
					}
				}
				else if ((int)val2.OptionValueType == 2)
				{
					if (int.TryParse(text, out var result2))
					{
						MultiplayerOptionsExtensions.SetValue(val, result2, (MultiplayerOptionsAccessMode)1);
					}
				}
				else if ((int)val2.OptionValueType == 0)
				{
					if (bool.TryParse(text, out var result3))
					{
						MultiplayerOptionsExtensions.SetValue(val, result3, (MultiplayerOptionsAccessMode)1);
					}
				}
				else
				{
					Debug.FailedAssert("No valid type found for multiplayer option.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\DedicatedServerConsoleCommandManager.cs", "HandleConsoleCommand", 81);
				}
				Debug.Print(string.Concat("--Changed: ", val, ", to: ", MultiplayerOptionsExtensions.GetValueText(val, (MultiplayerOptionsAccessMode)1)), 0, (DebugColor)12, 17179869184uL);
			}
			else
			{
				Debug.Print(string.Concat("--Value of: ", val, ", is: ", MultiplayerOptionsExtensions.GetValueText(val, (MultiplayerOptionsAccessMode)1)), 0, (DebugColor)12, 17179869184uL);
			}
			flag = true;
		}
		if (!flag)
		{
			foreach (Type commandHandlerType in _commandHandlerTypes)
			{
				MethodInfo[] methods = commandHandlerType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					object[] customAttributesSafe = Extensions.GetCustomAttributesSafe(methodInfo, false);
					foreach (object obj in customAttributesSafe)
					{
						ConsoleCommandMethod val3 = (ConsoleCommandMethod)((obj is ConsoleCommandMethod) ? obj : null);
						if (val3 != null && val3.CommandName.Equals(text2))
						{
							if (text == "?")
							{
								Debug.Print("--" + val3.CommandName + ": " + val3.Description, 0, (DebugColor)12, 17179869184uL);
							}
							else
							{
								methodInfo.Invoke(null, (string.IsNullOrEmpty(text) ? null : new List<object> { text })?.ToArray());
							}
							flag = true;
						}
					}
				}
			}
		}
		if (!flag)
		{
			bool flag2 = default(bool);
			string text3 = CommandLineFunctionality.CallFunction(text2, text, ref flag2);
			if (flag2)
			{
				Debug.Print(text3, 0, (DebugColor)12, 17179869184uL);
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.Print("--Invalid command is given.", 0, (DebugColor)12, 17179869184uL);
		}
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("list", "Displays a list of all multiplayer options, their values and other possible commands")]
	private static void ListAllCommands()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Print("--List of all multiplayer command and their current values:", 0, (DebugColor)12, 17179869184uL);
		for (OptionType val = (OptionType)0; (int)val < 43; val = (OptionType)(val + 1))
		{
			Debug.Print(string.Concat("----", val, ": ", MultiplayerOptionsExtensions.GetValueText(val, (MultiplayerOptionsAccessMode)1)), 0, (DebugColor)12, 17179869184uL);
		}
		Debug.Print("--List of additional commands:", 0, (DebugColor)12, 17179869184uL);
		foreach (Type commandHandlerType in _commandHandlerTypes)
		{
			MethodInfo[] methods = commandHandlerType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
			for (int i = 0; i < methods.Length; i++)
			{
				object[] customAttributesSafe = Extensions.GetCustomAttributesSafe(methods[i], false);
				foreach (object obj in customAttributesSafe)
				{
					ConsoleCommandMethod val2 = (ConsoleCommandMethod)((obj is ConsoleCommandMethod) ? obj : null);
					if (val2 != null)
					{
						Debug.Print("----" + val2.CommandName, 0, (DebugColor)12, 17179869184uL);
					}
				}
			}
		}
		Debug.Print("--Add '?' after a command to get a more detailed description.", 0, (DebugColor)12, 17179869184uL);
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("set_winner_team", "Sets the winner team of flag domination based multiplayer missions.")]
	private static void SetWinnerTeam(string winnerTeamAsString)
	{
		MissionMultiplayerFlagDomination.SetWinnerTeam(int.Parse(winnerTeamAsString));
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("set_server_bandwidth_limit_in_mbps", "Overrides server's older bandwidth limit in megabit(s) per second.")]
	private static void SetServerBandwidthLimitInMbps(string bandwidthLimitAsString)
	{
		GameNetwork.SetServerBandwidthLimitInMbps(double.Parse(bandwidthLimitAsString));
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("set_server_tickrate", "Overrides server's older tickrate setting.")]
	private static void SetServerTickRate(string tickrateAsString)
	{
		double num = double.Parse(tickrateAsString);
		GameNetwork.SetServerTickRate(num);
		GameNetwork.SetServerFrameRate(num);
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("stats", "Displays some game statistics, like FPS and players on the server.")]
	private static void ShowStats()
	{
		Debug.Print("--Current FPS: " + Utilities.GetFps(), 0, (DebugColor)12, 17179869184uL);
		Debug.Print("--Active Players: " + GameNetwork.NetworkPeers.Count((NetworkCommunicator x) => x.IsSynchronized), 0, (DebugColor)12, 17179869184uL);
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("open_monitor", "Opens up the monitor window with continuous data-representations on server performance and network usage.")]
	private static void OpenMonitor()
	{
		DebugNetworkEventStatistics.ControlActivate();
		DebugNetworkEventStatistics.OpenExternalMonitor();
	}

	[UsedImplicitly]
	[ConsoleCommandMethod("crash_game", "Crashes the game process.")]
	private static void CrashGame()
	{
		Debug.Print("Crashing the process...", 0, (DebugColor)12, 17179869184uL);
		throw new Exception("Game crashed by user command");
	}
}
