using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

public class MissionPeerMarkerTargetVM : MissionMarkerTargetVM
{
	private const string _partyMemberColor = "#00FF00FF";

	private const string _friendColor = "#FFFF00FF";

	private const string _clanMemberColor = "#00FFFFFF";

	private bool _isFriend;

	public MissionPeer TargetPeer { get; private set; }

	public override Vec3 WorldPosition
	{
		get
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			MissionPeer targetPeer = TargetPeer;
			if (((targetPeer != null) ? targetPeer.ControlledAgent : null) != null)
			{
				return TargetPeer.ControlledAgent.Position + new Vec3(0f, 0f, TargetPeer.ControlledAgent.GetEyeGlobalHeight(), -1f);
			}
			Debug.FailedAssert("No target found!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\FlagMarker\\Targets\\MissionPeerMarkerTargetVM.cs", "WorldPosition", 27);
			return Vec3.One;
		}
	}

	protected override float HeightOffset => 0.75f;

	public MissionPeerMarkerTargetVM(MissionPeer peer, bool isFriend)
		: base(MissionMarkerType.Peer)
	{
		TargetPeer = peer;
		_isFriend = isFriend;
		base.Name = peer.DisplayedName;
		SetVisual();
	}

	private void SetVisual()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		string text = "#FFFFFFFF";
		if (NetworkMain.GameClient.IsInParty && NetworkMain.GameClient.PlayersInParty.Any(delegate(PartyPlayerInLobbyClient p)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			PlayerId playerId = p.PlayerId;
			return ((PlayerId)(ref playerId)).Equals(((PeerComponent)TargetPeer).Peer.Id);
		}))
		{
			text = "#00FF00FF";
		}
		else if (_isFriend)
		{
			text = "#FFFF00FF";
		}
		else if (NetworkMain.GameClient.IsInClan && NetworkMain.GameClient.PlayersInClan.Any(delegate(ClanPlayer p)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			PlayerId playerId = p.PlayerId;
			return ((PlayerId)(ref playerId)).Equals(((PeerComponent)TargetPeer).Peer.Id);
		}))
		{
			text = "#00FFFFFF";
		}
		Color val = Color.ConvertStringToColor("#FFFFFFFF");
		uint color = ((Color)(ref val)).ToUnsignedInteger();
		val = Color.ConvertStringToColor(text);
		uint color2 = ((Color)(ref val)).ToUnsignedInteger();
		RefreshColor(color, color2);
	}

	public override void UpdateScreenPosition(Camera missionCamera)
	{
		MissionPeer targetPeer = TargetPeer;
		if (((targetPeer != null) ? targetPeer.ControlledAgent : null) != null)
		{
			base.UpdateScreenPosition(missionCamera);
		}
	}
}
