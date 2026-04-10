using Helpers;
using SandBox.BoardGames.MissionLogics;

namespace SandBox.BoardGames.AI;

public class BoardGameAITablut : BoardGameAIBase
{
	public static BoardGameTablut Board;

	private int _sampleCount;

	public BoardGameAITablut(AIDifficulty difficulty, MissionBoardGameLogic boardGameHandler)
		: base(difficulty, boardGameHandler)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)


	public override void Initialize()
	{
		base.Initialize();
		Board = base.BoardGameHandler.Board as BoardGameTablut;
	}

	public override void OnSetGameOver()
	{
		base.OnSetGameOver();
		Board = null;
	}

	public override Move CalculateMovementStageMove()
	{
		Move openingMove = default(Move);
		openingMove.GoalTile = null;
		openingMove.Unit = null;
		if (Board.IsReady)
		{
			BoardGameTablut.BoardInformation board = Board.TakeBoardSnapshot();
			TreeNodeTablut treeNodeTablut = TreeNodeTablut.CreateTreeAndReturnRootNode(board, MaxDepth);
			for (int i = 0; i < _sampleCount; i++)
			{
				if (base.AbortRequested)
				{
					break;
				}
				treeNodeTablut.SelectAction();
			}
			if (!base.AbortRequested)
			{
				Board.UndoMove(ref board);
				TreeNodeTablut childWithBestScore = treeNodeTablut.GetChildWithBestScore();
				if (childWithBestScore != null)
				{
					openingMove = childWithBestScore.OpeningMove;
				}
			}
		}
		if (!base.AbortRequested)
		{
			_ = openingMove.IsValid;
		}
		return openingMove;
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
			_sampleCount = 30000;
			break;
		case 1:
			MaxDepth = 4;
			_sampleCount = 47000;
			break;
		case 2:
			MaxDepth = 5;
			_sampleCount = 64000;
			break;
		}
	}
}
