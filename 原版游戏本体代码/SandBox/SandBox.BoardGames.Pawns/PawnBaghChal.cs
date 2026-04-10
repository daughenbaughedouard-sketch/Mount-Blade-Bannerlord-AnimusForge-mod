using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.BoardGames.Pawns;

public class PawnBaghChal : PawnBase
{
	public int X;

	public int Y;

	public int PrevX;

	public int PrevY;

	public override bool IsPlaced
	{
		get
		{
			if (X >= 0 && X < BoardGameBaghChal.BoardWidth && Y >= 0)
			{
				return Y < BoardGameBaghChal.BoardHeight;
			}
			return false;
		}
	}

	public MatrixFrame InitialFrame { get; }

	public bool IsTiger { get; }

	public bool IsGoat => !IsTiger;

	public PawnBaghChal(GameEntity entity, bool playerOne, bool isTiger)
		: base(entity, playerOne)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		X = -1;
		Y = -1;
		PrevX = -1;
		PrevY = -1;
		IsTiger = isTiger;
		InitialFrame = base.Entity.GetFrame();
	}

	public override void Reset()
	{
		base.Reset();
		X = -1;
		Y = -1;
		PrevX = -1;
		PrevY = -1;
	}
}
