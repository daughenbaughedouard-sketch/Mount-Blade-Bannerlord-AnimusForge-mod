using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.BoardGames.Pawns;

public class PawnPuluc : PawnBase
{
	public enum MovementState
	{
		MovingForward,
		MovingBackward,
		ChangingDirection
	}

	public MovementState State;

	public PawnPuluc CapturedBy;

	public Vec3 SpawnPos;

	public bool IsInSpawn = true;

	public bool IsTopPawn = true;

	private static float _height;

	private int _x;

	public float Height
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (_height == 0f)
			{
				_height = (base.Entity.GetBoundingBoxMax() - base.Entity.GetBoundingBoxMin()).z;
			}
			return _height;
		}
	}

	public override Vec3 PosBeforeMoving => PosBeforeMovingBase - new Vec3(0f, 0f, Height * (float)PawnsBelow.Count, -1f);

	public override bool IsPlaced
	{
		get
		{
			if (InPlay || IsInSpawn)
			{
				return IsTopPawn;
			}
			return false;
		}
	}

	public int X
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
			if (value >= 0 && value < 11)
			{
				IsInSpawn = false;
			}
			else
			{
				IsInSpawn = true;
			}
		}
	}

	public List<PawnPuluc> PawnsBelow { get; }

	public bool InPlay
	{
		get
		{
			if (X >= 0)
			{
				return X < 11;
			}
			return false;
		}
	}

	public PawnPuluc(GameEntity entity, bool playerOne)
		: base(entity, playerOne)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		PawnsBelow = new List<PawnPuluc>();
		SpawnPos = base.CurrentPos;
		X = -1;
	}

	public override void Reset()
	{
		base.Reset();
		X = -1;
		State = MovementState.MovingForward;
		IsTopPawn = true;
		IsInSpawn = true;
		CapturedBy = null;
		PawnsBelow.Clear();
	}

	public override void AddGoalPosition(Vec3 goal)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (IsTopPawn)
		{
			goal.z += Height * (float)PawnsBelow.Count;
			int count = PawnsBelow.Count;
			for (int i = 0; i < count; i++)
			{
				PawnsBelow[i].AddGoalPosition(goal - new Vec3(0f, 0f, (float)(i + 1) * Height, -1f));
			}
		}
		base.GoalPositions.Add(goal);
	}

	public override void MovePawnToGoalPositions(bool instantMove, float speed, bool dragged = false)
	{
		if (base.GoalPositions.Count == 0)
		{
			return;
		}
		base.MovePawnToGoalPositions(instantMove, speed, dragged);
		if (!IsTopPawn)
		{
			return;
		}
		foreach (PawnPuluc item in PawnsBelow)
		{
			item.MovePawnToGoalPositions(instantMove, speed, dragged);
		}
	}

	public override void SetPawnAtPosition(Vec3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.SetPawnAtPosition(position);
		if (!IsTopPawn)
		{
			return;
		}
		int num = 1;
		foreach (PawnPuluc item in PawnsBelow)
		{
			item.SetPawnAtPosition(new Vec3(position.x, position.y, position.z - Height * (float)num, -1f));
			num++;
		}
	}

	public override void EnableCollisionBody()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.EnableCollisionBody();
		foreach (PawnPuluc item in PawnsBelow)
		{
			GameEntity entity = item.Entity;
			entity.BodyFlag = (BodyFlags)(entity.BodyFlag & -2);
		}
	}

	public override void DisableCollisionBody()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.DisableCollisionBody();
		foreach (PawnPuluc item in PawnsBelow)
		{
			GameEntity entity = item.Entity;
			entity.BodyFlag = (BodyFlags)(entity.BodyFlag | 1);
		}
	}

	public void MovePawnBackToSpawn(bool instantMove, float speed, bool fake = false)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		X = -1;
		State = MovementState.MovingForward;
		IsTopPawn = true;
		IsInSpawn = true;
		base.Captured = false;
		CapturedBy = null;
		PawnsBelow.Clear();
		if (!fake)
		{
			AddGoalPosition(SpawnPos);
			MovePawnToGoalPositions(instantMove, speed);
		}
	}
}
