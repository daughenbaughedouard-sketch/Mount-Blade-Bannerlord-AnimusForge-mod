using System.Collections.Generic;
using Helpers;
using SandBox.BoardGames.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.BoardGames.AI;

public class BoardGameAIMuTorere : BoardGameAIBase
{
	private readonly BoardGameMuTorere _board;

	public BoardGameAIMuTorere(AIDifficulty difficulty, MissionBoardGameLogic boardGameHandler)
		: base(difficulty, boardGameHandler)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		_board = base.BoardGameHandler.Board as BoardGameMuTorere;
	}

	protected override void InitializeDifficulty()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		AIDifficulty difficulty = base.Difficulty;
		switch ((int)difficulty)
		{
		case 0:
			MaxDepth = 3;
			break;
		case 1:
			MaxDepth = 5;
			break;
		case 2:
			MaxDepth = 7;
			break;
		}
	}

	public override Move CalculateMovementStageMove()
	{
		Move result = default(Move);
		result.GoalTile = null;
		result.Unit = null;
		if (_board.IsReady)
		{
			List<List<Move>> moves = _board.CalculateAllValidMoves(BoardGameSide.AI);
			BoardGameMuTorere.BoardInformation board = _board.TakePawnsSnapshot();
			if (_board.HasMovesAvailable(ref moves))
			{
				int num = int.MinValue;
				foreach (List<Move> item in moves)
				{
					if (base.AbortRequested)
					{
						break;
					}
					foreach (Move item2 in item)
					{
						if (!base.AbortRequested)
						{
							_board.AIMakeMove(item2);
							int num2 = -NegaMax(MaxDepth, -1);
							_board.UndoMove(ref board);
							if (num2 > num)
							{
								result = item2;
								num = num2;
							}
							continue;
						}
						break;
					}
				}
			}
		}
		if (!base.AbortRequested)
		{
			_ = result.IsValid;
		}
		return result;
	}

	private int NegaMax(int depth, int color)
	{
		int num = int.MinValue;
		if (depth == 0)
		{
			return color * Evaluation() * ((_board.PlayerWhoStarted == PlayerTurn.PlayerOne) ? 1 : (-1));
		}
		BoardGameMuTorere.BoardInformation board = _board.TakePawnsSnapshot();
		List<List<Move>> moves = _board.CalculateAllValidMoves((color != 1) ? BoardGameSide.Player : BoardGameSide.AI);
		if (!_board.HasMovesAvailable(ref moves))
		{
			return color * Evaluation();
		}
		foreach (List<Move> item in moves)
		{
			foreach (Move item2 in item)
			{
				_board.AIMakeMove(item2);
				num = MathF.Max(num, -NegaMax(depth - 1, -color));
				_board.UndoMove(ref board);
			}
		}
		return num;
	}

	private int Evaluation()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected I4, but got Unknown
		float num = MBRandom.RandomFloat;
		AIDifficulty difficulty = base.Difficulty;
		switch ((int)difficulty)
		{
		case 0:
			num = num * 2f - 1f;
			break;
		case 1:
			num = num * 1.7f - 0.7f;
			break;
		case 2:
			num = num * 1.4f - 0.4f;
			break;
		}
		return (int)(num * 100f * (float)(CanMove(playerOne: false) - CanMove(playerOne: true)));
	}

	private int CanMove(bool playerOne)
	{
		List<List<Move>> moves = _board.CalculateAllValidMoves(playerOne ? BoardGameSide.Player : BoardGameSide.AI);
		if (!_board.HasMovesAvailable(ref moves))
		{
			return 0;
		}
		return 1;
	}
}
