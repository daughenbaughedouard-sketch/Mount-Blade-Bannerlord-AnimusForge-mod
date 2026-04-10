using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public static class MultiplayerPlayerContextMenuHelper
{
	public static void AddLobbyViewProfileOptions(MPLobbyPlayerBaseVM player, MBBindingList<StringPairItemWithActionVM> contextMenuOptions)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		((Collection<StringPairItemWithActionVM>)(object)contextMenuOptions).Add(new StringPairItemWithActionVM((Action<object>)ExecuteViewProfile, ((object)new TextObject("{=bjJkW9dO}View Profile", (Dictionary<string, object>)null)).ToString(), "ViewProfile", (object)player));
		AddPlatformProfileCardOption(ExecuteViewPlatformProfileCardLobby, player, player.ProvidedID, contextMenuOptions);
	}

	public static void AddMissionViewProfileOptions(MPPlayerVM player, MBBindingList<StringPairItemWithActionVM> contextMenuOptions)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		AddPlatformProfileCardOption(ExecuteViewPlatformProfileCardMission, player, ((PeerComponent)player.Peer).Peer.Id, contextMenuOptions);
	}

	private static void AddPlatformProfileCardOption(Action<object> onExecuted, object target, PlayerId playerId, MBBindingList<StringPairItemWithActionVM> contextMenuOptions)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		if (PlatformServices.Instance.IsPlayerProfileCardAvailable(NetworkMain.GameClient.PlayerID) && PlatformServices.Instance.IsPlayerProfileCardAvailable(playerId) && PlayerIdExtensions.SupportsPlayerCard(((PlayerId)(ref playerId)).ProvidedType))
		{
			TextObject empty = TextObject.GetEmpty();
			Debug.FailedAssert("Platform profile is supported but \"Show Profile\" text is not defined!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\MultiplayerPlayerContextMenuHelper.cs", "AddPlatformProfileCardOption", 51);
			if (!empty.IsEmpty())
			{
				((Collection<StringPairItemWithActionVM>)(object)contextMenuOptions).Add(new StringPairItemWithActionVM(onExecuted, ((object)empty).ToString(), "ViewProfile", target));
			}
		}
	}

	private static void ExecuteViewProfile(object playerObj)
	{
		(playerObj as MPLobbyPlayerBaseVM).ExecuteShowProfile();
	}

	private static void ExecuteViewPlatformProfileCardLobby(object playerObj)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		PlatformServices.Instance.ShowPlayerProfileCard((playerObj as MPLobbyPlayerBaseVM).ProvidedID);
	}

	private static void ExecuteViewPlatformProfileCardMission(object playerObj)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		PlatformServices.Instance.ShowPlayerProfileCard(((PeerComponent)(playerObj as MPPlayerVM).Peer).Peer.Id);
	}
}
