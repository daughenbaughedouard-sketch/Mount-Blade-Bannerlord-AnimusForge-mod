using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.Missions;

public class CameraJumpScript : ScriptComponentBehavior
{
	[EditableScriptComponentVariable(true, "WaitBeforeCameraJump")]
	private float _waitBeforeCameraJump = 2f;

	[EditableScriptComponentVariable(true, "CameraJumpPosition")]
	private Vec3 _cameraJumpPosition;

	[EditableScriptComponentVariable(true, "CameraJumpRotation")]
	private Vec3 _cameraJumpRotation;

	public SimpleButton SetCurrentCameraTransform;

	public SimpleButton Preview;

	public SimpleButton Reset;

	private MatrixFrame _initialGlobalFrame;

	private float _elapsedDuration = -1f;

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnInit()
	{
		_elapsedDuration = 0f;
	}

	protected override void OnEditorInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_initialGlobalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
	}

	protected override void OnTick(float dt)
	{
		OnJumpTick(dt);
	}

	protected override void OnEditorTick(float dt)
	{
		OnJumpTick(dt);
	}

	private void OnJumpTick(float dt)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_elapsedDuration >= 0f)
		{
			_elapsedDuration += dt;
			if (_elapsedDuration >= _waitBeforeCameraJump)
			{
				Mat3 identity = Mat3.Identity;
				((Mat3)(ref identity)).ApplyEulerAngles(ref _cameraJumpRotation);
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				MatrixFrame val = new MatrixFrame(ref identity, ref _cameraJumpPosition);
				((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref val, true);
			}
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (variableName == "Preview")
		{
			_elapsedDuration = 0f;
		}
		WeakGameEntity gameEntity;
		if (variableName == "Reset")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetGlobalFrame(ref _initialGlobalFrame, true);
			_elapsedDuration = -1f;
		}
		if (variableName == "SetCurrentCameraTransform")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			_cameraJumpPosition = globalFrame.origin;
			_cameraJumpRotation = ((Mat3)(ref globalFrame.rotation)).GetEulerAngles();
		}
	}
}
