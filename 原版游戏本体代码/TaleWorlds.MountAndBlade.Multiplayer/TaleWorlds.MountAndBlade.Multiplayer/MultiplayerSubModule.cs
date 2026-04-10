using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public class MultiplayerSubModule : MBSubModuleBase
{
	private bool _isConnectingToMultiplayer;

	protected internal override void OnSubModuleLoad()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Invalid comparison between Unknown and I4
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		((MBSubModuleBase)this).OnSubModuleLoad();
		Module.CurrentModule.AddMultiplayerGameMode((MultiplayerGameMode)(object)new MissionBasedMultiplayerGameMode("TeamDeathmatch"));
		Module.CurrentModule.AddMultiplayerGameMode((MultiplayerGameMode)(object)new MissionBasedMultiplayerGameMode("Duel"));
		Module.CurrentModule.AddMultiplayerGameMode((MultiplayerGameMode)(object)new MissionBasedMultiplayerGameMode("Siege"));
		Module.CurrentModule.AddMultiplayerGameMode((MultiplayerGameMode)(object)new MissionBasedMultiplayerGameMode("Captain"));
		Module.CurrentModule.AddMultiplayerGameMode((MultiplayerGameMode)(object)new MissionBasedMultiplayerGameMode("Skirmish"));
		Module.CurrentModule.AddMultiplayerGameMode((MultiplayerGameMode)(object)new MissionBasedMultiplayerGameMode("Battle"));
		TextObject coreContentDisabledReason = new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null);
		if ((int)Module.CurrentModule.StartupInfo.StartupType != 3)
		{
			Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Multiplayer", new TextObject("{=YDYnuBmC}Multiplayer", (Dictionary<string, object>)null), 9997, (Action)StartMultiplayer, (Func<ValueTuple<bool, TextObject>>)(() => (Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)), (TextObject)null, (Func<bool>)null));
		}
		TauntUsageManager.Initialize();
	}

	public override void OnGameLoaded(Game game, object initializerObject)
	{
		((MBSubModuleBase)this).OnGameLoaded(game, initializerObject);
		MultiplayerMain.Initialize((IGameNetworkHandler)(object)new GameNetworkHandler());
	}

	protected internal override void OnApplicationTick(float dt)
	{
		((MBSubModuleBase)this).OnApplicationTick(dt);
	}

	protected internal override void OnBeforeInitialModuleScreenSetAsRoot()
	{
		((MBSubModuleBase)this).OnBeforeInitialModuleScreenSetAsRoot();
		if (GameNetwork.IsDedicatedServer)
		{
			MBGameManager.StartNewGame((MBGameManager)(object)new MultiplayerGameManager());
		}
	}

	public override void OnInitialState()
	{
		((MBSubModuleBase)this).OnInitialState();
		if (Utilities.CommandLineArgumentExists("+connect_lobby"))
		{
			MBGameManager.StartNewGame((MBGameManager)(object)new MultiplayerGameManager());
		}
		else if (!Module.CurrentModule.IsOnlyCoreContentEnabled && Module.CurrentModule.MultiplayerRequested)
		{
			MBGameManager.StartNewGame((MBGameManager)(object)new MultiplayerGameManager());
		}
	}

	private async void StartMultiplayer()
	{
		if (_isConnectingToMultiplayer)
		{
			return;
		}
		_isConnectingToMultiplayer = true;
		bool flag = NetworkMain.GameClient != null && await ((Client<LobbyClient>)(object)NetworkMain.GameClient).CheckConnection();
		bool isConnected = flag;
		PlatformServices.Instance.CheckPrivilege((Privilege)0, true, (PrivilegeResult)delegate(bool result)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			if (!isConnected || !result)
			{
				string? text = ((object)new TextObject("{=ksq1IBh3}No connection", (Dictionary<string, object>)null)).ToString();
				string text2 = ((object)new TextObject("{=5VIbo2Cb}No connection could be established to the lobby server. Check your internet connection and try again.", (Dictionary<string, object>)null)).ToString();
				InformationManager.ShowInquiry(new InquiryData(text, text2, false, true, "", ((object)new TextObject("{=dismissnotification}Dismiss", (Dictionary<string, object>)null)).ToString(), (Action)null, (Action)delegate
				{
					InformationManager.HideInquiry();
				}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
			else
			{
				MBGameManager.StartNewGame((MBGameManager)(object)new MultiplayerGameManager());
			}
		});
		_isConnectingToMultiplayer = false;
	}

	protected internal override void OnNetworkTick(float dt)
	{
		((MBSubModuleBase)this).OnNetworkTick(dt);
		MultiplayerMain.Tick(dt);
		InternetAvailabilityChecker.Tick(dt);
	}
}
