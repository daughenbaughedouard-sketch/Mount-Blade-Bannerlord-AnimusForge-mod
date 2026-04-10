using System;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.Missions;

public class RotateObjectScript : ScriptComponentBehavior
{
	private enum State
	{
		None,
		Start,
		WaitBeforeRotate,
		Rotating,
		End
	}

	[EditableScriptComponentVariable(true, "RotationAxis")]
	private string _rotationAxis = "X";

	[EditableScriptComponentVariable(true, "WaitBeforeRotateAsSeconds")]
	private float _waitBeforeRotateAsSeconds = 2f;

	[EditableScriptComponentVariable(true, "RotateAngle")]
	private float _rotateAngle = 90f;

	[EditableScriptComponentVariable(true, "RotationSpeed")]
	private float _rotationSpeed = 1f;

	public SimpleButton PreviewRotateObject;

	public SimpleButton StopMovement;

	private MatrixFrame _initialFrameCacheForPreviewRotateObjectButton;

	private State _state;

	private float _currentRotationAngle;

	private float _currentTimeDt;

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnTick(float dt)
	{
		if (_state == State.None)
		{
			_state = State.Start;
		}
		OnTickInternal(dt);
	}

	protected override void OnEditorTick(float dt)
	{
		OnTickInternal(dt);
	}

	private void OnTickInternal(float dt)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (_rotationAxis.Equals("X", StringComparison.OrdinalIgnoreCase) || _rotationAxis.Equals("Y", StringComparison.OrdinalIgnoreCase) || _rotationAxis.Equals("Z", StringComparison.OrdinalIgnoreCase))
		{
			_rotationAxis = "X";
		}
		WeakGameEntity gameEntity;
		switch (_state)
		{
		case State.Start:
			if (_waitBeforeRotateAsSeconds > 0f)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				_initialFrameCacheForPreviewRotateObjectButton = ((WeakGameEntity)(ref gameEntity)).GetFrame();
				_state = State.WaitBeforeRotate;
			}
			else
			{
				_state = State.Rotating;
			}
			break;
		case State.WaitBeforeRotate:
			_currentTimeDt += dt;
			if (_currentTimeDt >= _waitBeforeRotateAsSeconds)
			{
				_state = State.Rotating;
			}
			break;
		case State.Rotating:
		{
			int num = MathF.Sign(_rotateAngle);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
			float num2 = _rotationSpeed * (float)num * dt * (MathF.PI / 180f);
			Vec3 rotationAxis = GetRotationAxis();
			((MatrixFrame)(ref frame)).Rotate(num2, ref rotationAxis);
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetFrame(ref frame, true);
			_currentRotationAngle += _rotationSpeed * (float)num * dt;
			if (Math.Abs(_currentRotationAngle) >= Math.Abs(_rotateAngle))
			{
				_state = State.End;
			}
			break;
		}
		}
	}

	private Vec3 GetRotationAxis()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (_rotationAxis.Equals("X", StringComparison.OrdinalIgnoreCase))
		{
			return Vec3.Side;
		}
		if (_rotationAxis.Equals("Y", StringComparison.OrdinalIgnoreCase))
		{
			return Vec3.Forward;
		}
		if (_rotationAxis.Equals("Z", StringComparison.OrdinalIgnoreCase))
		{
			return Vec3.Up;
		}
		Debug.FailedAssert("Wrong rotation axis!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\RotateObjectScript.cs", "GetRotationAxis", 123);
		return Vec3.Forward;
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		WeakGameEntity gameEntity;
		if (variableName == "PreviewRotateObject")
		{
			if (_state != State.None && _state != State.End)
			{
				Debug.FailedAssert("The rotation is already started, please click the \"StopMovement\" button first!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\RotateObjectScript.cs", "OnEditorVariableChanged", 135);
			}
			else
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				_initialFrameCacheForPreviewRotateObjectButton = ((WeakGameEntity)(ref gameEntity)).GetFrame();
				_currentRotationAngle = 0f;
				_currentTimeDt = 0f;
				_state = State.Start;
			}
		}
		if (variableName == "StopMovement")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetFrame(ref _initialFrameCacheForPreviewRotateObjectButton, true);
			_currentRotationAngle = 0f;
			_currentTimeDt = 0f;
			_state = State.None;
		}
	}
}
