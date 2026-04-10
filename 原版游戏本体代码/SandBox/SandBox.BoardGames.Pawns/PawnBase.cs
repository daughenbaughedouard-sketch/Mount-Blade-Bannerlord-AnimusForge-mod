using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.BoardGames.Pawns;

public abstract class PawnBase
{
	public Action<PawnBase, Vec3, Vec3> OnArrivedIntermediateGoalPosition;

	public Action<PawnBase, Vec3, Vec3> OnArrivedFinalGoalPosition;

	protected Vec3 PosBeforeMovingBase;

	private int _currentGoalPos;

	private float _dtCounter;

	private float _movePauseDuration;

	private float _movePauseTimer;

	private float _moveSpeed;

	private bool _moveTiming;

	private bool _dragged;

	private bool _freePathToDestination;

	public static int PawnMoveSoundCodeID { get; set; }

	public static int PawnSelectSoundCodeID { get; set; }

	public static int PawnTapSoundCodeID { get; set; }

	public static int PawnRemoveSoundCodeID { get; set; }

	public abstract bool IsPlaced { get; }

	public virtual Vec3 PosBeforeMoving
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return PosBeforeMovingBase;
		}
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			PosBeforeMovingBase = value;
		}
	}

	public GameEntity Entity { get; }

	protected List<Vec3> GoalPositions { get; }

	protected Vec3 CurrentPos { get; private set; }

	public bool Captured { get; set; }

	public bool MovingToDifferentTile { get; set; }

	public bool Moving { get; private set; }

	public bool PlayerOne { get; private set; }

	public bool HasAnyGoalPosition
	{
		get
		{
			bool result = false;
			if (GoalPositions != null)
			{
				result = !Extensions.IsEmpty<Vec3>((IEnumerable<Vec3>)GoalPositions);
			}
			return result;
		}
	}

	protected PawnBase(GameEntity entity, bool playerOne)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Entity = entity;
		PlayerOne = playerOne;
		CurrentPos = Entity.GetGlobalFrame().origin;
		PosBeforeMoving = CurrentPos;
		Moving = false;
		_dragged = false;
		Captured = false;
		_movePauseDuration = 0.3f;
		GameEntityPhysicsExtensions.CreateVariableRatePhysics(entity, true);
		GoalPositions = new List<Vec3>();
	}

	public virtual void Reset()
	{
		ClearGoalPositions();
		Moving = false;
		MovingToDifferentTile = false;
		_movePauseDuration = 0.3f;
		_movePauseTimer = 0f;
		_moveTiming = false;
		_dragged = false;
		Captured = false;
	}

	public virtual void AddGoalPosition(Vec3 goal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		GoalPositions.Add(goal);
	}

	public virtual void SetPawnAtPosition(Vec3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = Entity.GetGlobalFrame();
		globalFrame.origin = position;
		Entity.SetGlobalFrame(ref globalFrame, true);
	}

	public virtual void MovePawnToGoalPositions(bool instantMove, float speed, bool dragged = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		PosBeforeMoving = Entity.GlobalPosition;
		_moveSpeed = speed;
		_currentGoalPos = 0;
		_movePauseTimer = 0f;
		_dtCounter = 0f;
		_moveTiming = false;
		_dragged = dragged;
		if (GoalPositions.Count == 1 && ((object)PosBeforeMoving/*cast due to .constrained prefix*/).Equals((object?)GoalPositions[0]))
		{
			instantMove = true;
		}
		if (instantMove)
		{
			MatrixFrame globalFrame = Entity.GetGlobalFrame();
			globalFrame.origin = GoalPositions[GoalPositions.Count - 1];
			Entity.SetGlobalFrame(ref globalFrame, true);
			ClearGoalPositions();
		}
		else
		{
			Moving = true;
		}
	}

	public virtual void EnableCollisionBody()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		GameEntity entity = Entity;
		entity.BodyFlag = (BodyFlags)(entity.BodyFlag & -2);
	}

	public virtual void DisableCollisionBody()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		GameEntity entity = Entity;
		entity.BodyFlag = (BodyFlags)(entity.BodyFlag | 1);
	}

	public void Tick(float dt)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		if (_moveTiming)
		{
			_movePauseTimer += dt;
			if (_movePauseTimer >= _movePauseDuration)
			{
				_moveTiming = false;
				_movePauseTimer = 0f;
			}
		}
		else
		{
			if (!Moving || !(dt > 0f))
			{
				return;
			}
			Vec3 val = default(Vec3);
			((Vec3)(ref val))._002Ector(0f, 0f, 0f, -1f);
			Vec3 val2 = GoalPositions[_currentGoalPos] - PosBeforeMoving;
			float num = ((Vec3)(ref val2)).Normalize();
			float num2 = num / _moveSpeed;
			float num3 = _dtCounter / num2;
			if (_dtCounter.Equals(0f))
			{
				float x = (Entity.GlobalBoxMax - Entity.GlobalBoxMin).x;
				float z = (Entity.GlobalBoxMax - Entity.GlobalBoxMin).z;
				Vec3 val3 = default(Vec3);
				((Vec3)(ref val3))._002Ector(0f, 0f, z / 2f, -1f);
				Vec3 val4 = Entity.GetGlobalFrame().origin + val3 + val2 * (x / 1.8f);
				Vec3 val5 = GoalPositions[_currentGoalPos] + val3;
				float num4 = default(float);
				if (Mission.Current.Scene.RayCastForClosestEntityOrTerrain(val4, val5, ref num4, 0.001f, (BodyFlags)0))
				{
					_freePathToDestination = false;
					num = num4;
				}
				else
				{
					_freePathToDestination = true;
					if (!_dragged)
					{
						PlayPawnMoveSound();
					}
					else
					{
						PlayPawnTapSound();
					}
				}
			}
			if (!_freePathToDestination)
			{
				float num5 = MathF.Sin(num3 * MathF.PI);
				float num6 = num / 6f;
				num5 *= num6;
				val += new Vec3(0f, 0f, num5, -1f);
			}
			_ = _dtCounter;
			_dtCounter += dt;
			Vec3 val8;
			if (num3 >= 1f)
			{
				_dtCounter = 0f;
				CurrentPos = GoalPositions[_currentGoalPos];
				val = Vec3.Zero;
				if (!_freePathToDestination && IsPlaced)
				{
					PlayPawnTapSound();
				}
				else if (!IsPlaced)
				{
					PlayPawnRemovedTapSound();
				}
				Vec3 val6 = GoalPositions[_currentGoalPos];
				bool flag = true;
				while (_currentGoalPos < GoalPositions.Count - 1)
				{
					_currentGoalPos++;
					Vec3 val7 = GoalPositions[_currentGoalPos];
					val8 = val6 - val7;
					if (((Vec3)(ref val8)).LengthSquared > 0f)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					OnArrivedFinalGoalPosition?.Invoke(this, PosBeforeMoving, CurrentPos);
					Moving = false;
					ClearGoalPositions();
				}
				else
				{
					OnArrivedIntermediateGoalPosition?.Invoke(this, PosBeforeMoving, CurrentPos);
					_movePauseDuration = 0.3f;
					_moveTiming = true;
				}
				PosBeforeMoving = CurrentPos;
			}
			else
			{
				Moving = true;
				CurrentPos = MBMath.Lerp(PosBeforeMoving, GoalPositions[_currentGoalPos], num3, 0.005f);
			}
			MatrixFrame globalFrame = Entity.GetGlobalFrame();
			ref Mat3 rotation = ref globalFrame.rotation;
			val8 = CurrentPos + val;
			MatrixFrame val9 = new MatrixFrame(ref rotation, ref val8);
			Entity.SetGlobalFrame(ref val9, true);
		}
	}

	public void MovePawnToGoalPositionsDelayed(bool instantMove, float speed, bool dragged, float delay)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (GoalPositions.Count > 0)
		{
			if (GoalPositions.Count == 1 && ((object)PosBeforeMoving/*cast due to .constrained prefix*/).Equals((object?)GoalPositions[0]))
			{
				ClearGoalPositions();
				return;
			}
			MovePawnToGoalPositions(instantMove, speed, dragged);
			_movePauseDuration = delay;
			_moveTiming = delay > 0f;
		}
	}

	public void SetPlayerOne(bool playerOne)
	{
		PlayerOne = playerOne;
	}

	public void ClearGoalPositions()
	{
		MovingToDifferentTile = false;
		GoalPositions.Clear();
	}

	public void UpdatePawnPosition()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		PosBeforeMoving = Entity.GlobalPosition;
	}

	public void PlayPawnSelectSound()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.MakeSound(PawnSelectSoundCodeID, CurrentPos, true, false, -1, -1);
	}

	private void PlayPawnTapSound()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.MakeSound(PawnTapSoundCodeID, CurrentPos, true, false, -1, -1);
	}

	private void PlayPawnRemovedTapSound()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.MakeSound(PawnRemoveSoundCodeID, CurrentPos, true, false, -1, -1);
	}

	private void PlayPawnMoveSound()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Mission.Current.MakeSound(PawnMoveSoundCodeID, CurrentPos, true, false, -1, -1);
	}
}
