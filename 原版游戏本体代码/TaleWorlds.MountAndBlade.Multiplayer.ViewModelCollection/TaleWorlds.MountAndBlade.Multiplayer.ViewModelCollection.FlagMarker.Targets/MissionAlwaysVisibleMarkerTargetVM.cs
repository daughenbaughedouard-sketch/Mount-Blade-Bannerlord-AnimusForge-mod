using System;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

public class MissionAlwaysVisibleMarkerTargetVM : MissionMarkerTargetVM
{
	private Vec3 _position;

	private Action<MissionAlwaysVisibleMarkerTargetVM> _onRemove;

	public MissionPeer TargetPeer { get; private set; }

	public override Vec3 WorldPosition => _position;

	protected override float HeightOffset => 0.75f;

	public MissionAlwaysVisibleMarkerTargetVM(MissionPeer peer, Vec3 position, Action<MissionAlwaysVisibleMarkerTargetVM> onRemove)
		: base(MissionMarkerType.Peer)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		TargetPeer = peer;
		_position = position;
		_onRemove = onRemove;
	}

	public void ExecuteRemove()
	{
		_onRemove?.Invoke(this);
	}
}
