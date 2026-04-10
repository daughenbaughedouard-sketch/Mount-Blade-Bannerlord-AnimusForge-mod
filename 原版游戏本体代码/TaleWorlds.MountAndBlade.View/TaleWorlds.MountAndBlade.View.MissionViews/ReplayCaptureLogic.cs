using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class ReplayCaptureLogic : MissionView
{
	private ReplayMissionView _replayLogic;

	private bool _renderActive;

	public const float CaptureFrameRate = 60f;

	private float _replayTimeDiff;

	private bool _frameSkip;

	private Path _path;

	private PlatformDirectoryPath _directoryPath;

	private bool _saveScreenshots;

	private readonly KeyValuePair<float, MatrixFrame> _invalid = new KeyValuePair<float, MatrixFrame>(-1f, default(MatrixFrame));

	private SortedDictionary<float, SortedDictionary<int, MatrixFrame>> _cameraKeys;

	private bool _isRendered;

	private int _lastUsedIndex;

	private int _ssNum;

	private bool RenderActive
	{
		get
		{
			return _renderActive;
		}
		set
		{
			_renderActive = value;
			CheckFixedDeltaTimeMode();
		}
	}

	private Camera MissionCamera
	{
		get
		{
			if (base.MissionScreen == null || !((NativeObject)(object)base.MissionScreen.CombatCamera != (NativeObject)null))
			{
				return null;
			}
			return base.MissionScreen.CombatCamera;
		}
	}

	private float ReplayTime => ((MissionBehavior)this).Mission.CurrentTime - _replayTimeDiff;

	private bool SaveScreenshots
	{
		get
		{
			return _saveScreenshots;
		}
		set
		{
			_saveScreenshots = value;
			CheckFixedDeltaTimeMode();
		}
	}

	private KeyValuePair<float, MatrixFrame> PreviousKey => GetPreviousKey();

	private KeyValuePair<float, MatrixFrame> NextKey => GetNextKey();

	private void CheckFixedDeltaTimeMode()
	{
		if (RenderActive && SaveScreenshots)
		{
			((MissionBehavior)this).Mission.FixedDeltaTime = 1f / 60f;
			((MissionBehavior)this).Mission.FixedDeltaTimeMode = true;
		}
		else
		{
			((MissionBehavior)this).Mission.FixedDeltaTime = 0f;
			((MissionBehavior)this).Mission.FixedDeltaTimeMode = false;
		}
	}

	private KeyValuePair<float, MatrixFrame> GetPreviousKey()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		KeyValuePair<float, MatrixFrame> result = _invalid;
		if (!_cameraKeys.Any())
		{
			return result;
		}
		foreach (KeyValuePair<float, SortedDictionary<int, MatrixFrame>> cameraKey in _cameraKeys)
		{
			if (cameraKey.Key <= ReplayTime)
			{
				result = new KeyValuePair<float, MatrixFrame>(cameraKey.Key, cameraKey.Value[cameraKey.Value.Count - 1]);
			}
		}
		return result;
	}

	private KeyValuePair<float, MatrixFrame> GetNextKey()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		KeyValuePair<float, MatrixFrame> result = _invalid;
		if (!_cameraKeys.Any())
		{
			return result;
		}
		foreach (KeyValuePair<float, SortedDictionary<int, MatrixFrame>> cameraKey in _cameraKeys)
		{
			if (cameraKey.Key > ReplayTime)
			{
				result = new KeyValuePair<float, MatrixFrame>(cameraKey.Key, cameraKey.Value[0]);
				break;
			}
		}
		return result;
	}

	public ReplayCaptureLogic()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		_cameraKeys = new SortedDictionary<float, SortedDictionary<int, MatrixFrame>>();
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_replayLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<ReplayMissionView>();
		_replayLogic.OverrideInput(isOverridden: true);
		if (!MBCommon.IsPaused)
		{
			_replayLogic.Pause();
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		if (_frameSkip && !MBCommon.IsPaused)
		{
			if (!_isRendered)
			{
				_isRendered = true;
				return;
			}
			_replayLogic.Pause();
			_frameSkip = false;
		}
		if (!RenderActive)
		{
			return;
		}
		SaveScreenshot();
		if (!((MissionBehavior)this).Mission.Recorder.IsEndOfRecord())
		{
			KeyValuePair<float, MatrixFrame> previousKey = PreviousKey;
			KeyValuePair<float, MatrixFrame> nextKey = NextKey;
			_replayLogic.Resume();
			if (nextKey.Key >= 0f)
			{
				for (int i = 0; i < _cameraKeys.Count; i++)
				{
					if (previousKey.Key == _cameraKeys.ElementAt(i).Key)
					{
						float num = nextKey.Key - previousKey.Key;
						float num2 = (ReplayTime - previousKey.Key) / num;
						int count = _cameraKeys[previousKey.Key].Count;
						MatrixFrame frame;
						if (_lastUsedIndex != i && count > 1)
						{
							frame = _cameraKeys[previousKey.Key][count - 1];
						}
						else
						{
							frame = new MatrixFrame
							{
								origin = _path.GetHermiteFrameForDt(num2, i).origin
							};
							Vec3 s = previousKey.Value.rotation.s * (1f - num2) + nextKey.Value.rotation.s * num2;
							Vec3 u = previousKey.Value.rotation.u * (1f - num2) + nextKey.Value.rotation.u * num2;
							Vec3 f = previousKey.Value.rotation.f * (1f - num2) + nextKey.Value.rotation.f * num2;
							frame.rotation.s = s;
							frame.rotation.u = u;
							frame.rotation.f = f;
						}
						((Vec3)(ref frame.rotation.s)).Normalize();
						((Vec3)(ref frame.rotation.u)).Normalize();
						((Vec3)(ref frame.rotation.f)).Normalize();
						((Mat3)(ref frame.rotation)).Orthonormalize();
						base.MissionScreen.CustomCamera.Frame = frame;
						_lastUsedIndex = i;
						break;
					}
				}
			}
			else if (previousKey.Key >= 0f)
			{
				int count2 = _cameraKeys[previousKey.Key].Count;
				if (count2 > 1)
				{
					MatrixFrame frame2 = _cameraKeys[previousKey.Key][count2 - 1];
					((Vec3)(ref frame2.rotation.s)).Normalize();
					((Vec3)(ref frame2.rotation.u)).Normalize();
					((Vec3)(ref frame2.rotation.f)).Normalize();
					((Mat3)(ref frame2.rotation)).Orthonormalize();
					base.MissionScreen.CustomCamera.Frame = frame2;
				}
			}
		}
		else
		{
			MBDebug.Print("All images are saved.", 0, (DebugColor)6, 64uL);
			RenderActive = false;
			_replayLogic.ResetReplay();
			_replayTimeDiff = ((MissionBehavior)this).Mission.CurrentTime;
			base.MissionScreen.CustomCamera = null;
			_replayLogic.Pause();
			SaveScreenshots = false;
			_ssNum = 0;
		}
	}

	private void InsertCamKey()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		float replayTime = ReplayTime;
		MatrixFrame frame = MissionCamera.Frame;
		int num = 0;
		if (_cameraKeys.ContainsKey(replayTime))
		{
			num = _cameraKeys[replayTime].Count;
			_cameraKeys[replayTime].Add(num, frame);
		}
		else
		{
			_cameraKeys.Add(replayTime, new SortedDictionary<int, MatrixFrame> { { num, frame } });
		}
		MBDebug.Print("Keyframe to \"" + replayTime + "\" has been inserted with the index: " + num + ".\n", 0, (DebugColor)4, 64uL);
	}

	private void MoveToNextFrame()
	{
		_replayLogic.FastForward(1f / 60f);
		_replayLogic.Resume();
		_frameSkip = true;
	}

	private void GoToKey(float keyTime)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!(keyTime < 0f) && _cameraKeys.ContainsKey(keyTime) && keyTime != ReplayTime)
		{
			MatrixFrame frame;
			if (keyTime < ReplayTime)
			{
				frame = _cameraKeys[keyTime][_cameraKeys[keyTime].Count - 1];
				_replayLogic.Rewind(ReplayTime - keyTime);
				_replayTimeDiff = ((MissionBehavior)this).Mission.CurrentTime;
			}
			else
			{
				frame = _cameraKeys[keyTime][0];
				_replayLogic.FastForward(keyTime - ReplayTime);
			}
			MissionCamera.Frame = frame;
		}
	}

	private void SetPath()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)((MissionBehavior)this).Mission.Scene.GetPathWithName("CameraPath") != (NativeObject)null)
		{
			((MissionBehavior)this).Mission.Scene.DeletePathWithName("CameraPath");
		}
		((MissionBehavior)this).Mission.Scene.AddPath("CameraPath");
		foreach (KeyValuePair<float, SortedDictionary<int, MatrixFrame>> cameraKey in _cameraKeys)
		{
			((MissionBehavior)this).Mission.Scene.AddPathPoint("CameraPath", cameraKey.Value[0]);
		}
		_path = ((MissionBehavior)this).Mission.Scene.GetPathWithName("CameraPath");
	}

	private void Render(bool saveScreenshots = false)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!_cameraKeys.ContainsKey(0f))
		{
			_cameraKeys.Add(0f, new SortedDictionary<int, MatrixFrame> { { 0, MissionCamera.Frame } });
		}
		else
		{
			_cameraKeys[0f] = new SortedDictionary<int, MatrixFrame> { { 0, MissionCamera.Frame } };
		}
		_replayLogic.ResetReplay();
		_replayLogic.Pause();
		_replayTimeDiff = ((MissionBehavior)this).Mission.CurrentTime;
		SetPath();
		SaveScreenshots = saveScreenshots;
		RenderActive = true;
		_lastUsedIndex = 0;
		base.MissionScreen.CustomCamera = base.MissionScreen.CombatCamera;
	}

	private void SaveScreenshot()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (SaveScreenshots)
		{
			if (string.IsNullOrEmpty(_directoryPath.Path))
			{
				PlatformDirectoryPath val = default(PlatformDirectoryPath);
				((PlatformDirectoryPath)(ref val))._002Ector((PlatformFileType)0, "Captures");
				string text = "Cap_" + $"{DateTime.Now:yyyy-MM-dd_hh-mm-ss-tt}";
				_directoryPath = val + text;
			}
			Utilities.TakeScreenshot(new PlatformFilePath(_directoryPath, "time_" + $"{_ssNum:000000}" + ".bmp"));
			_ssNum++;
		}
	}
}
