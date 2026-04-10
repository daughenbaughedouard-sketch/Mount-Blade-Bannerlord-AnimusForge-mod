using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade;

public class MissionRecentPlayersComponent : MissionNetwork
{
	private PlayerId _myId;

	public override void AfterStart()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		MissionPeer.OnTeamChanged += new OnTeamChangedDelegate(TeamChange);
		MissionPeer.OnPlayerKilled += new OnPlayerKilledDelegate(OnPlayerKilled);
		_myId = NetworkMain.GameClient.PlayerID;
	}

	private void TeamChange(NetworkCommunicator player, Team oldTeam, Team nextTeam)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (player.VirtualPlayer.Id != _myId)
		{
			RecentPlayersManager.AddOrUpdatePlayerEntry(player.VirtualPlayer.Id, player.UserName, (InteractionType)2, player.ForcedAvatarIndex);
		}
	}

	private void OnPlayerKilled(MissionPeer killerPeer, MissionPeer killedPeer)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (killerPeer != null && killedPeer != null && ((PeerComponent)killerPeer).Peer != null && ((PeerComponent)killedPeer).Peer != null)
		{
			PlayerId id = ((PeerComponent)killerPeer).Peer.Id;
			PlayerId id2 = ((PeerComponent)killedPeer).Peer.Id;
			if (id == _myId && id2 != _myId)
			{
				RecentPlayersManager.AddOrUpdatePlayerEntry(id2, ((PeerComponent)killedPeer).Name, (InteractionType)0, PeerExtensions.GetNetworkPeer((PeerComponent)(object)killedPeer).ForcedAvatarIndex);
			}
			else if (id2 == _myId && id != _myId)
			{
				RecentPlayersManager.AddOrUpdatePlayerEntry(id, ((PeerComponent)killerPeer).Name, (InteractionType)1, PeerExtensions.GetNetworkPeer((PeerComponent)(object)killerPeer).ForcedAvatarIndex);
			}
		}
	}

	public override void OnRemoveBehavior()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		MissionPeer.OnTeamChanged -= new OnTeamChangedDelegate(TeamChange);
		MissionPeer.OnPlayerKilled -= new OnPlayerKilledDelegate(OnPlayerKilled);
		((MissionNetwork)this).OnRemoveBehavior();
	}
}
