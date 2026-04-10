using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SandBox.AI;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Objects.Usables;

public class MusicianGroup : UsableMachine
{
	public const int GapBetweenTracks = 8;

	public const bool DisableAmbientMusic = true;

	private const int TempoMidValue = 120;

	private const int TempoSpeedUpLimit = 130;

	private const int TempoSlowDownLimit = 100;

	private List<PlayMusicPoint> _musicianPoints;

	private SoundEvent _trackEvent;

	private BasicMissionTimer _gapTimer;

	private List<SettlementMusicData> _playList;

	private int _currentTrackIndex = -1;

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		return null;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		return null;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new UsablePlaceAI((UsableMachine)(object)this);
	}

	public void SetPlayList(List<SettlementMusicData> playList)
	{
		_playList = playList.ToList();
	}

	protected override void OnInit()
	{
		((UsableMachine)this).OnInit();
		_playList = new List<SettlementMusicData>();
		_musicianPoints = ((IEnumerable)((UsableMachine)this).StandingPoints).OfType<PlayMusicPoint>().ToList();
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((UsableMachine)this).GetTickRequirement());
	}

	protected override void OnTick(float dt)
	{
		((UsableMachine)this).OnTick(dt);
		CheckNewTrackStart();
		CheckTrackEnd();
	}

	private void CheckNewTrackStart()
	{
		if (_playList.Count > 0 && _trackEvent == null && (_gapTimer == null || _gapTimer.ElapsedTime > 8f) && _musicianPoints.Any((PlayMusicPoint x) => ((UsableMissionObject)x).HasUser))
		{
			_currentTrackIndex++;
			if (_currentTrackIndex == _playList.Count)
			{
				_currentTrackIndex = 0;
			}
			SetupInstruments();
			StartTrack();
			_gapTimer = null;
		}
	}

	private void CheckTrackEnd()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		if (_trackEvent != null)
		{
			if (_trackEvent.IsPlaying() && !_musicianPoints.Any((PlayMusicPoint x) => ((UsableMissionObject)x).HasUser))
			{
				_trackEvent.Stop();
			}
			if (_trackEvent != null && !_trackEvent.IsPlaying())
			{
				_trackEvent.Release();
				_trackEvent = null;
				StopMusicians();
				_gapTimer = new BasicMissionTimer();
			}
		}
	}

	private void StopMusicians()
	{
		foreach (PlayMusicPoint musicianPoint in _musicianPoints)
		{
			if (((UsableMissionObject)musicianPoint).HasUser)
			{
				musicianPoint.EndLoop();
			}
		}
	}

	private void SetupInstruments()
	{
		List<PlayMusicPoint> list = _musicianPoints.ToList();
		Extensions.Shuffle<PlayMusicPoint>((IList<PlayMusicPoint>)list);
		SettlementMusicData settlementMusicData = _playList[_currentTrackIndex];
		foreach (InstrumentData instrumentData in (List<InstrumentData>)(object)settlementMusicData.Instruments)
		{
			PlayMusicPoint playMusicPoint = list.FirstOrDefault(delegate(PlayMusicPoint x)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				WeakGameEntity val = ((ScriptComponentBehavior)x).GameEntity;
				val = ((WeakGameEntity)(ref val)).Parent;
				return ((WeakGameEntity)(ref val)).Tags.Contains(instrumentData.Tag) || string.IsNullOrEmpty(instrumentData.Tag);
			});
			if (playMusicPoint != null)
			{
				Tuple<InstrumentData, float> instrument = new Tuple<InstrumentData, float>(instrumentData, (float)settlementMusicData.Tempo / 120f);
				playMusicPoint.ChangeInstrument(instrument);
				list.Remove(playMusicPoint);
			}
		}
		Tuple<InstrumentData, float> instrumentEmptyData = GetInstrumentEmptyData(settlementMusicData.Tempo);
		foreach (PlayMusicPoint item in list)
		{
			item.ChangeInstrument(instrumentEmptyData);
		}
	}

	private Tuple<InstrumentData, float> GetInstrumentEmptyData(int tempo)
	{
		if (tempo > 130)
		{
			return new Tuple<InstrumentData, float>(MBObjectManager.Instance.GetObject<InstrumentData>("cheerful"), 1f);
		}
		if (tempo > 100)
		{
			return new Tuple<InstrumentData, float>(MBObjectManager.Instance.GetObject<InstrumentData>("active"), 1f);
		}
		return new Tuple<InstrumentData, float>(MBObjectManager.Instance.GetObject<InstrumentData>("calm"), 1f);
	}

	private void StartTrack()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int eventIdFromString = SoundEvent.GetEventIdFromString(_playList[_currentTrackIndex].MusicPath);
		_trackEvent = SoundEvent.CreateEvent(eventIdFromString, Mission.Current.Scene);
		SoundEvent trackEvent = _trackEvent;
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		trackEvent.SetPosition(((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin);
		_trackEvent.Play();
		foreach (PlayMusicPoint musicianPoint in _musicianPoints)
		{
			musicianPoint.StartLoop(_trackEvent);
		}
	}
}
