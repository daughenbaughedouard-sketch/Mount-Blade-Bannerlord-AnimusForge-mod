using System;
using System.Threading.Tasks;
using TaleWorlds.Diamond;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public static class InternetAvailabilityChecker
{
	public static Action<bool> OnInternetConnectionAvailabilityChanged;

	private static bool _internetConnectionAvailable;

	private static long _lastInternetConnectionCheck;

	private static bool _checkingConnection;

	private const long InternetConnectionCheckIntervalShort = 100000000L;

	private const long InternetConnectionCheckIntervalLong = 300000000L;

	public static bool InternetConnectionAvailable
	{
		get
		{
			return _internetConnectionAvailable;
		}
		private set
		{
			if (value != _internetConnectionAvailable)
			{
				_internetConnectionAvailable = value;
				OnInternetConnectionAvailabilityChanged?.Invoke(value);
			}
		}
	}

	private static async void CheckInternetConnection()
	{
		if (NetworkMain.GameClient != null)
		{
			InternetConnectionAvailable = await ((Client<LobbyClient>)(object)NetworkMain.GameClient).CheckConnection();
		}
		_lastInternetConnectionCheck = DateTime.Now.Ticks;
		_checkingConnection = false;
	}

	internal static void Tick(float dt)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		long num = (InternetConnectionAvailable ? 300000000 : 100000000);
		if (Module.CurrentModule != null && (int)Module.CurrentModule.StartupInfo.StartupType != 3 && !_checkingConnection && DateTime.Now.Ticks - _lastInternetConnectionCheck > num)
		{
			_checkingConnection = true;
			Task.Run(delegate
			{
				CheckInternetConnection();
			});
		}
	}
}
