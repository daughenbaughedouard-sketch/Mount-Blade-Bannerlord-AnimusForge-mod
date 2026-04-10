using System;
using System.Collections.Generic;
using Helpers;
using SandBox.BoardGames.MissionLogics;
using SandBox.BoardGames.Pawns;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.BoardGames.AI;

public class BoardGameAISeega : BoardGameAIBase
{
	private readonly BoardGameSeega _board;

	private readonly int[,] _boardValues = new int[5, 5]
	{
		{ 3, 2, 2, 2, 3 },
		{ 2, 1, 1, 1, 2 },
		{ 2, 1, 3, 1, 2 },
		{ 2, 1, 1, 1, 2 },
		{ 3, 2, 2, 2, 3 }
	};

	public BoardGameAISeega(AIDifficulty difficulty, MissionBoardGameLogic boardGameHandler)
		: base(difficulty, boardGameHandler)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		_board = base.BoardGameHandler.Board as BoardGameSeega;
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
			MaxDepth = 2;
			break;
		case 1:
			MaxDepth = 3;
			break;
		case 2:
			MaxDepth = 4;
			break;
		}
	}

	public override Move CalculateMovementStageMove()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		Move result = Move.Invalid;
		if (_board.IsReady)
		{
			List<List<Move>> moves = _board.CalculateAllValidMoves(BoardGameSide.AI);
			if (!_board.HasMovesAvailable(ref moves))
			{
				Dictionary<PawnBase, int> blockingPawns = _board.GetBlockingPawns(playerOneBlocked: false);
				InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=1bzdDYoO}All AI pawns blocked. Removing one of the player's pawns to make a move", (Dictionary<string, object>)null)).ToString()));
				PawnBase key = Extensions.MaxBy<KeyValuePair<PawnBase, int>, int>((IEnumerable<KeyValuePair<PawnBase, int>>)blockingPawns, (Func<KeyValuePair<PawnBase, int>, int>)((KeyValuePair<PawnBase, int> x) => x.Value)).Key;
				_board.SetPawnCaptured(key);
				moves = _board.CalculateAllValidMoves(BoardGameSide.AI);
			}
			BoardGameSeega.BoardInformation board = _board.TakeBoardSnapshot();
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
							int num2 = -NegaMax(MaxDepth, -1, -2147483647, int.MaxValue);
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

	public override bool WantsToForfeit()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if (!MayForfeit)
		{
			return false;
		}
		int playerOneUnitsAlive = _board.GetPlayerOneUnitsAlive();
		int playerTwoUnitsAlive = _board.GetPlayerTwoUnitsAlive();
		int num = (((int)base.Difficulty != 2) ? 1 : 2);
		if (playerTwoUnitsAlive <= 7 && playerOneUnitsAlive >= playerTwoUnitsAlive + (num + playerTwoUnitsAlive / 2))
		{
			MayForfeit = false;
			return true;
		}
		return false;
	}

	public override Move CalculatePreMovementStageMove()
	{
		Move invalid = Move.Invalid;
		foreach (PawnSeega playerTwoUnit in _board.PlayerTwoUnits)
		{
			if (playerTwoUnit.IsPlaced || playerTwoUnit.Moving)
			{
				continue;
			}
			while (!invalid.IsValid && !base.AbortRequested)
			{
				int x = MBRandom.RandomInt(0, 5);
				int y = MBRandom.RandomInt(0, 5);
				if (_board.GetTile(x, y).PawnOnTile == null && !_board.GetTile(x, y).Entity.HasTag("obstructed_at_start"))
				{
					invalid.Unit = playerTwoUnit;
					invalid.GoalTile = _board.GetTile(x, y);
				}
			}
			break;
		}
		return invalid;
	}

	private int NegaMax(int depth, int color, int alpha, int beta)
	{
		int num = int.MinValue;
		if (depth == 0)
		{
			return color * Evaluation();
		}
		foreach (PawnSeega item in (color == 1) ? _board.PlayerTwoUnits : _board.PlayerOneUnits)
		{
			item.UpdateMoveBackAvailable();
		}
		List<List<Move>> moves = _board.CalculateAllValidMoves((color != 1) ? BoardGameSide.Player : BoardGameSide.AI);
		if (!_board.HasMovesAvailable(ref moves))
		{
			return color * Evaluation();
		}
		BoardGameSeega.BoardInformation board = _board.TakeBoardSnapshot();
		foreach (List<Move> item2 in moves)
		{
			if (item2 == null)
			{
				continue;
			}
			foreach (Move item3 in item2)
			{
				_board.AIMakeMove(item3);
				num = MathF.Max(-NegaMax(depth - 1, -color, -beta, -alpha), num);
				alpha = MathF.Max(alpha, num);
				_board.UndoMove(ref board);
				if (alpha >= beta && color == 1)
				{
					return alpha;
				}
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
			num = num * 0.7f + 0.5f;
			break;
		case 1:
			num = num * 0.5f + 0.65f;
			break;
		case 2:
			num = num * 0.35f + 0.75f;
			break;
		}
		return (int)((float)(20 * (_board.GetPlayerTwoUnitsAlive() - _board.GetPlayerOneUnitsAlive()) + (GetPlacementScore(player: false) - GetPlacementScore(player: true)) + 2 * (GetSurroundedScore(player: false) - GetSurroundedScore(player: true))) * num);
	}

	private int GetPlacementScore(bool player)
	{
		int num = 0;
		foreach (PawnSeega item in player ? _board.PlayerOneUnits : _board.PlayerTwoUnits)
		{
			if (item.IsPlaced)
			{
				num += _boardValues[item.X, item.Y];
			}
		}
		return num;
	}

	private int GetSurroundedScore(bool player)
	{
		int num = 0;
		foreach (PawnSeega item in player ? _board.PlayerOneUnits : _board.PlayerTwoUnits)
		{
			if (item.IsPlaced)
			{
				num += GetAmountSurroundingThisPawn(item);
			}
		}
		return num;
	}

	private int GetAmountSurroundingThisPawn(PawnSeega pawn)
	{
		int num = 0;
		int x = pawn.X;
		int y = pawn.Y;
		if (x > 0 && _board.GetTile(x - 1, y).PawnOnTile != null)
		{
			num++;
		}
		if (y > 0 && _board.GetTile(x, y - 1).PawnOnTile != null)
		{
			num++;
		}
		if (x < BoardGameSeega.BoardWidth - 1 && _board.GetTile(x + 1, y).PawnOnTile != null)
		{
			num++;
		}
		if (y < BoardGameSeega.BoardHeight - 1 && _board.GetTile(x, y + 1).PawnOnTile != null)
		{
			num++;
		}
		return num;
	}
}
