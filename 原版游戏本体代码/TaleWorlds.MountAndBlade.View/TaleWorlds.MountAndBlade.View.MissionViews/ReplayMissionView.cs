using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class ReplayMissionView : MissionView
{
	private float _resetTime;

	private bool _isInputOverridden;

	private ReplayMissionLogic _replayMissionLogic;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_resetTime = 0f;
		_replayMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<ReplayMissionLogic>();
	}

	public override void OnPreMissionTick(float dt)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnPreMissionTick(dt);
		((MissionBehavior)this).Mission.Recorder.ProcessRecordUntilTime(((MissionBehavior)this).Mission.CurrentTime - _resetTime);
		_ = _isInputOverridden;
		if ((int)((MissionBehavior)this).Mission.CurrentState == 2 && ((MissionBehavior)this).Mission.Recorder.IsEndOfRecord())
		{
			if (MBEditor._isEditorMissionOn)
			{
				MBEditor.LeaveEditMissionMode();
			}
			else
			{
				((MissionBehavior)this).Mission.EndMission();
			}
		}
	}

	public void OverrideInput(bool isOverridden)
	{
		_isInputOverridden = isOverridden;
	}

	public void ResetReplay()
	{
		_resetTime = ((MissionBehavior)this).Mission.CurrentTime;
		((MissionBehavior)this).Mission.ResetMission();
		((MissionBehavior)this).Mission.Teams.Clear();
		((MissionBehavior)this).Mission.Recorder.RestartRecord();
		MBCommon.UnPauseGameEngine();
		((MissionBehavior)this).Mission.Scene.TimeSpeed = 1f;
	}

	public void Rewind(float time)
	{
		_resetTime = MathF.Min(_resetTime + time, ((MissionBehavior)this).Mission.CurrentTime);
		((MissionBehavior)this).Mission.ResetMission();
		((MissionBehavior)this).Mission.Teams.Clear();
		((MissionBehavior)this).Mission.Recorder.RestartRecord();
	}

	public void FastForward(float time)
	{
		_resetTime -= time;
	}

	public void Pause()
	{
		if (!MBCommon.IsPaused && MBMath.ApproximatelyEqualsTo(((MissionBehavior)this).Mission.Scene.TimeSpeed, 1f, 1E-05f))
		{
			MBCommon.PauseGameEngine();
		}
	}

	public void Resume()
	{
		if (MBCommon.IsPaused || !MBMath.ApproximatelyEqualsTo(((MissionBehavior)this).Mission.Scene.TimeSpeed, 1f, 1E-05f))
		{
			MBCommon.UnPauseGameEngine();
			((MissionBehavior)this).Mission.Scene.TimeSpeed = 1f;
		}
	}
}
