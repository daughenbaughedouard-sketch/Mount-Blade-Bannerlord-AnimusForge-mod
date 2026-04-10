using System;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.Missions.AI.Behaviors;

internal class NavalBehaviorBoardShipSubtask
{
	public enum ShipBoardingState
	{
		ApproachFromFarAway,
		GettingClose,
		AdjustingOrientation,
		InPosition,
		Connected,
		InactiveStuck
	}

	private const float MinimumBoardingDistance = 3f;

	private const float IdealBoardingDistance = 12f;

	private const float MaximumBoardingDistance = 30f;

	private const float DriftedAwayDistance = 50f;

	private MissionShip _selfShip;

	private MissionShip _givenTargetToBoard;

	private MissionShip _effectiveTarget;

	private bool _givenSideToBoardIsRight;

	private bool _effectiveSideToBoardIsRight;

	private float _cachedEffectiveDistance = float.MaxValue;

	public ShipBoardingState State { get; private set; }

	public NavalBehaviorBoardShipSubtask(MissionShip selfShip)
	{
		_selfShip = selfShip;
	}

	public void OnBehaviorActivatedAux()
	{
		State = ShipBoardingState.ApproachFromFarAway;
	}

	public void SetOwnerShip(MissionShip selfShip)
	{
		_selfShip = selfShip;
		SetTargetShipAndSide(_givenTargetToBoard, _givenSideToBoardIsRight);
	}

	public void SetTargetShipAndSide(MissionShip targetShip, bool rightSide)
	{
		if (_givenTargetToBoard != targetShip || _effectiveTarget != targetShip || _givenSideToBoardIsRight != rightSide || _effectiveSideToBoardIsRight != rightSide || State == ShipBoardingState.InactiveStuck)
		{
			_givenTargetToBoard = targetShip;
			_effectiveTarget = targetShip;
			_givenSideToBoardIsRight = rightSide;
			_effectiveSideToBoardIsRight = rightSide;
			State = ShipBoardingState.ApproachFromFarAway;
		}
	}

	public MissionShip GetCurrentGivenTarget()
	{
		return _givenTargetToBoard;
	}

	public MissionShip GetCurrentEffectiveTargetShip()
	{
		return _effectiveTarget;
	}

	public float GetEffectiveDistanceToObjective()
	{
		if (State == ShipBoardingState.Connected)
		{
			return 0f;
		}
		if (State == ShipBoardingState.InactiveStuck)
		{
			return float.MaxValue;
		}
		return _cachedEffectiveDistance;
	}

	private void CheckAndSwitchState()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		if (_givenTargetToBoard == null || _effectiveTarget == null)
		{
			return;
		}
		if (State != ShipBoardingState.Connected && State != ShipBoardingState.InactiveStuck && _selfShip.GetIsConnected())
		{
			if (_selfShip.SearchShipConnection(_givenTargetToBoard, isDirect: true, findEnemy: false, enforceActive: false, acceptNotBridgedConnections: true))
			{
				State = ShipBoardingState.Connected;
			}
			else
			{
				State = ShipBoardingState.InactiveStuck;
			}
			return;
		}
		MatrixFrame globalFrame = _effectiveTarget.GlobalFrame;
		MatrixFrame globalFrame2 = _selfShip.GlobalFrame;
		Vec2 val;
		Vec2 val2;
		if (!_effectiveSideToBoardIsRight)
		{
			val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			val = ((Vec2)(ref val)).RightVec();
			val2 = ((Vec2)(ref val)).Normalized();
		}
		else
		{
			val = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			val = ((Vec2)(ref val)).LeftVec();
			val2 = ((Vec2)(ref val)).Normalized();
		}
		Vec2 val3 = val2;
		if (State == ShipBoardingState.ApproachFromFarAway)
		{
			val = ((Vec3)(ref globalFrame.origin)).AsVec2 - ((Vec3)(ref globalFrame2.origin)).AsVec2;
			if (!(((Vec2)(ref val)).LengthSquared < 900f))
			{
				val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
				if (!(((Vec2)(ref val)).LengthSquared < 2500f))
				{
					goto IL_0138;
				}
			}
			State = ShipBoardingState.GettingClose;
		}
		goto IL_0138;
		IL_01b0:
		Vec2 asVec;
		if (State == ShipBoardingState.AdjustingOrientation)
		{
			val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
			if (((Vec2)(ref val)).LengthSquared > 2500f)
			{
				State = ShipBoardingState.GettingClose;
			}
			else
			{
				val = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
				val = ((Vec2)(ref val)).Normalized();
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				if (Math.Abs(((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec)).Normalized())) > 0.8f)
				{
					State = ShipBoardingState.InPosition;
				}
			}
		}
		if (State == ShipBoardingState.InPosition)
		{
			if (_selfShip.GetIsConnected())
			{
				State = ShipBoardingState.Connected;
			}
			else
			{
				val = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
				val = ((Vec2)(ref val)).Normalized();
				asVec = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				if (Math.Abs(((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec)).Normalized())) < 0.6f)
				{
					State = ShipBoardingState.AdjustingOrientation;
				}
				else
				{
					val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
					if (((Vec2)(ref val)).LengthSquared > 2500f)
					{
						State = ShipBoardingState.GettingClose;
					}
				}
			}
		}
		if (State == ShipBoardingState.Connected)
		{
			if (!_selfShip.GetIsConnected())
			{
				State = ShipBoardingState.GettingClose;
			}
			else if (!_selfShip.SearchShipConnection(_givenTargetToBoard, isDirect: true, findEnemy: false, enforceActive: false, acceptNotBridgedConnections: true))
			{
				State = ShipBoardingState.InactiveStuck;
			}
		}
		if (State == ShipBoardingState.InactiveStuck && !_selfShip.GetIsConnected())
		{
			State = ShipBoardingState.ApproachFromFarAway;
		}
		return;
		IL_0138:
		if (State == ShipBoardingState.GettingClose)
		{
			val = ((Vec3)(ref globalFrame.origin)).AsVec2 - ((Vec3)(ref globalFrame2.origin)).AsVec2;
			if (!(((Vec2)(ref val)).LengthSquared < 900f))
			{
				val = ((Vec3)(ref globalFrame.origin)).AsVec2 + val3 * 12f - ((Vec3)(ref globalFrame2.origin)).AsVec2;
				if (!(((Vec2)(ref val)).LengthSquared < 900f))
				{
					goto IL_01b0;
				}
			}
			State = ShipBoardingState.AdjustingOrientation;
		}
		goto IL_01b0;
	}

	private bool IsEffectivelyRightSide()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (State == ShipBoardingState.ApproachFromFarAway)
		{
			return _givenSideToBoardIsRight;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_selfShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)_givenTargetToBoard).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 val = asVec - ((Vec3)(ref globalPosition)).AsVec2;
		MatrixFrame globalFrame = _givenTargetToBoard.GlobalFrame;
		Vec2 asVec2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		return ((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec2)).LeftVec()) >= 0f;
	}

	private bool IsRelevantSideOfEnemyShipRight(MissionShip testedShip)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		Vec3 globalPosition;
		Vec2 val;
		MatrixFrame globalFrame;
		if (State == ShipBoardingState.ApproachFromFarAway)
		{
			gameEntity = ((ScriptComponentBehavior)_selfShip).GameEntity;
			globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
			gameEntity = ((ScriptComponentBehavior)testedShip).GameEntity;
			globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			val = asVec - ((Vec3)(ref globalPosition)).AsVec2;
			globalFrame = testedShip.GlobalFrame;
			if (!(((Vec2)(ref val)).DotProduct(((Vec3)(ref globalFrame.rotation.f)).AsVec2) >= 0f))
			{
				return _givenSideToBoardIsRight;
			}
			return !_givenSideToBoardIsRight;
		}
		gameEntity = ((ScriptComponentBehavior)_selfShip).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec2 = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)testedShip).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		val = asVec2 - ((Vec3)(ref globalPosition)).AsVec2;
		globalFrame = testedShip.GlobalFrame;
		Vec2 asVec3 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		return ((Vec2)(ref val)).DotProduct(((Vec2)(ref asVec3)).RightVec()) >= 0f;
	}

	private void DetermineEffectiveTargetShip()
	{
		_effectiveSideToBoardIsRight = IsRelevantSideOfEnemyShipRight(_givenTargetToBoard);
		_effectiveTarget = _givenTargetToBoard.GetOutermostConnectedShipFromSide(_effectiveSideToBoardIsRight, out _effectiveSideToBoardIsRight, 0uL);
	}

	private void ApproachFromDistance(MissionShip enemyShip, out Vec2 desiredPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)enemyShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)_selfShip).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 val = asVec - ((Vec3)(ref globalPosition)).AsVec2;
		Vec2 val2 = ((Vec2)(ref val)).Normalized();
		MatrixFrame globalFrame = enemyShip.GlobalFrame;
		Vec2 asVec2 = ((Vec3)(ref globalFrame.origin)).AsVec2;
		Vec2 val3;
		if (!_effectiveSideToBoardIsRight)
		{
			val = ((Vec2)(ref val2)).LeftVec();
			val3 = ((Vec2)(ref val)).Normalized();
		}
		else
		{
			val = ((Vec2)(ref val2)).RightVec();
			val3 = ((Vec2)(ref val)).Normalized();
		}
		desiredPosition = asVec2 + val3 * 12f;
	}

	private void GettingCloseCase(MissionShip enemyShip, out Vec2 desiredPosition, out Vec2 desiredDirection)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)_selfShip).GameEntity;
		Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 asVec = ((Vec3)(ref globalPosition)).AsVec2;
		gameEntity = ((ScriptComponentBehavior)enemyShip).GameEntity;
		globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		Vec2 val = asVec - ((Vec3)(ref globalPosition)).AsVec2;
		Vec2 val2;
		if (enemyShip == _givenTargetToBoard)
		{
			MatrixFrame globalFrame = enemyShip.GlobalFrame;
			Vec2 asVec2 = ((Vec3)(ref globalFrame.origin)).AsVec2;
			val2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
			Vec2 val3;
			if (!(((Vec2)(ref val)).DotProduct(((Vec2)(ref val2)).LeftVec()) >= 0f))
			{
				val2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				val2 = ((Vec2)(ref val2)).RightVec();
				val3 = ((Vec2)(ref val2)).Normalized();
			}
			else
			{
				val2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
				val2 = ((Vec2)(ref val2)).LeftVec();
				val3 = ((Vec2)(ref val2)).Normalized();
			}
			desiredPosition = asVec2 + val3 * 12f;
		}
		else
		{
			ApproachFromDistance(enemyShip, out desiredPosition);
		}
		_ = enemyShip.GlobalFrame.origin - _selfShip.GlobalFrame.origin;
		MatrixFrame globalFrame2 = enemyShip.GlobalFrame;
		val2 = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
		globalFrame2 = _selfShip.GlobalFrame;
		if (((Vec2)(ref val2)).DotProduct(((Vec3)(ref globalFrame2.rotation.f)).AsVec2) >= 0f)
		{
			globalFrame2 = enemyShip.GlobalFrame;
			val2 = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
			desiredDirection = ((Vec2)(ref val2)).Normalized();
		}
		else
		{
			globalFrame2 = enemyShip.GlobalFrame;
			val2 = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
			desiredDirection = -((Vec2)(ref val2)).Normalized();
		}
	}

	public void CalculateShipOrders(out Vec2 desiredPosition, out Vec2 desiredDirection, out MissionShip boardingTargetShip)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		CheckAndSwitchState();
		MatrixFrame globalFrame = _selfShip.GlobalFrame;
		desiredPosition = ((Vec3)(ref globalFrame.origin)).AsVec2;
		MatrixFrame globalFrame2 = _selfShip.GlobalFrame;
		Vec2 asVec = ((Vec3)(ref globalFrame2.rotation.f)).AsVec2;
		desiredDirection = ((Vec2)(ref asVec)).Normalized();
		boardingTargetShip = null;
		if (_givenTargetToBoard != null && _effectiveTarget != null)
		{
			DetermineEffectiveTargetShip();
			switch (State)
			{
			case ShipBoardingState.ApproachFromFarAway:
				ApproachFromDistance(_effectiveTarget, out desiredPosition);
				boardingTargetShip = null;
				break;
			case ShipBoardingState.GettingClose:
				GettingCloseCase(_effectiveTarget, out desiredPosition, out desiredDirection);
				boardingTargetShip = null;
				break;
			case ShipBoardingState.AdjustingOrientation:
			case ShipBoardingState.InPosition:
				GettingCloseCase(_effectiveTarget, out desiredPosition, out desiredDirection);
				boardingTargetShip = _effectiveTarget;
				break;
			case ShipBoardingState.Connected:
				boardingTargetShip = _givenTargetToBoard;
				break;
			case ShipBoardingState.InactiveStuck:
				boardingTargetShip = null;
				break;
			}
			_cachedEffectiveDistance = ((Vec2)(ref desiredPosition)).Distance(((Vec3)(ref globalFrame.origin)).AsVec2);
		}
	}
}
