using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MultiplayerVoiceChatVM : ViewModel
{
	private readonly Mission _mission;

	private readonly VoiceChatHandler _voiceChatHandler;

	private MBBindingList<MPVoicePlayerVM> _activeVoicePlayers;

	[DataSourceProperty]
	public MBBindingList<MPVoicePlayerVM> ActiveVoicePlayers
	{
		get
		{
			return _activeVoicePlayers;
		}
		set
		{
			if (value != _activeVoicePlayers)
			{
				_activeVoicePlayers = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPVoicePlayerVM>>(value, "ActiveVoicePlayers");
			}
		}
	}

	public MultiplayerVoiceChatVM(Mission mission)
	{
		_mission = mission;
		_voiceChatHandler = _mission.GetMissionBehavior<VoiceChatHandler>();
		if (_voiceChatHandler != null)
		{
			_voiceChatHandler.OnPeerVoiceStatusUpdated += OnPeerVoiceStatusUpdated;
			_voiceChatHandler.OnVoiceRecordStarted += OnVoiceRecordStarted;
			_voiceChatHandler.OnVoiceRecordStopped += OnVoiceRecordStopped;
		}
		ActiveVoicePlayers = new MBBindingList<MPVoicePlayerVM>();
	}

	public override void OnFinalize()
	{
		if (_voiceChatHandler != null)
		{
			_voiceChatHandler.OnPeerVoiceStatusUpdated -= OnPeerVoiceStatusUpdated;
			_voiceChatHandler.OnVoiceRecordStarted -= OnVoiceRecordStarted;
			_voiceChatHandler.OnVoiceRecordStopped -= OnVoiceRecordStopped;
		}
		((ViewModel)this).OnFinalize();
	}

	public void OnTick(float dt)
	{
		for (int i = 0; i < ((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers).Count; i++)
		{
			if (!((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers)[i].IsMyPeer && ((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers)[i].UpdatesSinceSilence >= 30)
			{
				((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers).RemoveAt(i);
				i--;
			}
		}
	}

	private void OnPeerVoiceStatusUpdated(MissionPeer peer, bool isTalking)
	{
		MPVoicePlayerVM mPVoicePlayerVM = ((IEnumerable<MPVoicePlayerVM>)ActiveVoicePlayers).FirstOrDefault((MPVoicePlayerVM vp) => vp.Peer == peer);
		if (isTalking)
		{
			if (mPVoicePlayerVM == null)
			{
				((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers).Add(new MPVoicePlayerVM(peer));
			}
			else
			{
				mPVoicePlayerVM.UpdatesSinceSilence = 0;
			}
		}
		else if (!isTalking && mPVoicePlayerVM != null)
		{
			mPVoicePlayerVM.UpdatesSinceSilence++;
		}
	}

	private void OnVoiceRecordStarted()
	{
		((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers).Add(new MPVoicePlayerVM(PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer)));
	}

	private void OnVoiceRecordStopped()
	{
		MPVoicePlayerVM item = ((IEnumerable<MPVoicePlayerVM>)ActiveVoicePlayers).FirstOrDefault((MPVoicePlayerVM vp) => vp.Peer == PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer));
		((Collection<MPVoicePlayerVM>)(object)ActiveVoicePlayers).Remove(item);
	}
}
