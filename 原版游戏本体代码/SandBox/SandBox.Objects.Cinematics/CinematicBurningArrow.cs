using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.Objects.Cinematics;

public class CinematicBurningArrow : ScriptComponentBehavior
{
	private enum BurningArrowState
	{
		None,
		StartMovement,
		MovementInProgress,
		EndMovement
	}

	private const float Gravity = 9.8f;

	private BurningArrowState _state;

	private float _speedCache;

	[EditableScriptComponentVariable(true, "")]
	private float _speed = 10f;

	private Vec3 _speedVector = Vec3.Zero;

	private float _arrowMovementTimer;

	private SoundEvent _arrowSound;

	private MatrixFrame _initialFrameCacheForShootArrowButton;

	private MatrixFrame _initialGlobalFrameCacheForShootArrowButton;

	public SimpleButton ShootArrow;

	public SimpleButton StopMovement;

	public void StartMovement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_initialFrameCacheForShootArrowButton = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_initialGlobalFrameCacheForShootArrowButton = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
		_state = BurningArrowState.StartMovement;
		_speedVector = _speed * _initialFrameCacheForShootArrowButton.rotation.u;
		_arrowSound = SoundEvent.CreateEventFromString("event:/mission/ambient/special/alert_arrow", ((ScriptComponentBehavior)this).Scene);
		_arrowSound.Play();
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnInit()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
	}

	protected override void OnTick(float dt)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Tick(dt);
		if (!_speed.Equals(_speedCache))
		{
			_speedCache = _speed;
			_speedVector = _speed * _initialFrameCacheForShootArrowButton.rotation.u;
		}
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		Tick(dt);
		Vec3 startPosition;
		Vec3 speedVector;
		if (_state == BurningArrowState.None)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			startPosition = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
			float speed = _speed;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			speedVector = speed * ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().rotation.u;
		}
		else
		{
			startPosition = _initialGlobalFrameCacheForShootArrowButton.origin;
			speedVector = _speed * _initialGlobalFrameCacheForShootArrowButton.rotation.u;
		}
		List<Vec3> list = new List<Vec3>();
		list.Add(startPosition);
		float num = 0f;
		float num2 = _speed * 100f / 15f;
		for (int i = 1; (float)i < num2; i++)
		{
			num += 0.03f;
			list.Add(GetPositionAtTime(in startPosition, in speedVector, num));
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (j != list.Count - 1)
			{
				_ = list[j];
				_ = list[j + 1];
			}
		}
	}

	private Vec3 GetPositionAtTime(in Vec3 startPosition, in Vec3 speedVector, float time)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Vec3 zero = Vec3.Zero;
		zero.x = startPosition.x + speedVector.x * time;
		zero.y = startPosition.y + speedVector.y * time;
		zero.z = startPosition.z + speedVector.z * time - 4.9f * time * time;
		return zero;
	}

	private void Tick(float dt)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (_state != BurningArrowState.EndMovement)
		{
			if (_state == BurningArrowState.StartMovement)
			{
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(true);
				_state = BurningArrowState.MovementInProgress;
			}
			if (_state == BurningArrowState.MovementInProgress)
			{
				Move(dt);
			}
		}
	}

	private void Move(float dt)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (_speed <= 0f || _arrowMovementTimer >= 4f)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetVisibilityExcludeParents(false);
			_state = BurningArrowState.EndMovement;
			_arrowSound.Stop();
			_arrowMovementTimer = 0f;
			return;
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		MatrixFrame frame = ((WeakGameEntity)(ref gameEntity)).GetFrame();
		_speedVector.z -= 9.8f * dt;
		Vec3 origin = frame.origin + _speedVector * dt;
		LookAtWithZAsForward(ref frame, ((Vec3)(ref _speedVector)).NormalizedCopy(), Vec3.Up);
		frame.origin = origin;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity)).SetFrame(ref frame, true);
		SoundEvent arrowSound = _arrowSound;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		arrowSound.SetPosition(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
		_arrowMovementTimer += dt;
	}

	private void LookAtWithZAsForward(ref MatrixFrame frame, Vec3 direction, Vec3 upVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = direction;
		Vec3 val2 = upVector;
		((Vec3)(ref val)).Normalize();
		Vec3 val3 = Vec3.CrossProduct(val, val2);
		((Vec3)(ref val3)).Normalize();
		val2 = Vec3.CrossProduct(val3, val);
		((Vec3)(ref val2)).Normalize();
		frame.rotation.s = val3;
		frame.rotation.f = val2;
		frame.rotation.u = -val;
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		WeakGameEntity gameEntity;
		if (variableName == "ShootArrow")
		{
			if (_state != BurningArrowState.None)
			{
				_state = BurningArrowState.None;
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetFrame(ref _initialFrameCacheForShootArrowButton, true);
			}
			StartMovement();
		}
		if (variableName == "StopMovement")
		{
			_state = BurningArrowState.None;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetFrame(ref _initialFrameCacheForShootArrowButton, true);
			_arrowMovementTimer = 0f;
		}
	}
}
