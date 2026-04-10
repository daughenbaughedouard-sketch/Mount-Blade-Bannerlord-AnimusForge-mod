using System;
using System.Collections.Generic;
using SandBox.BoardGames;
using SandBox.BoardGames.MissionLogics;
using SandBox.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.BoardGame;

public class BoardGameVM : ViewModel
{
	private readonly MissionBoardGameLogic _missionBoardGameHandler;

	private BoardGameInstructionsVM _instructions;

	private string _turnOwnerText;

	private string _boardGameType;

	private bool _isGameUsingDice;

	private bool _isPlayersTurn;

	private bool _canRoll;

	private string _diceResult;

	private string _rollDiceText;

	private string _closeText;

	private string _forfeitText;

	private InputKeyItemVM _rollDiceKey;

	[DataSourceProperty]
	public BoardGameInstructionsVM Instructions
	{
		get
		{
			return _instructions;
		}
		set
		{
			if (value != _instructions)
			{
				_instructions = value;
				((ViewModel)this).OnPropertyChangedWithValue<BoardGameInstructionsVM>(value, "Instructions");
			}
		}
	}

	[DataSourceProperty]
	public bool CanRoll
	{
		get
		{
			return _canRoll;
		}
		set
		{
			if (value != _canRoll)
			{
				_canRoll = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanRoll");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayersTurn
	{
		get
		{
			return _isPlayersTurn;
		}
		set
		{
			if (value != _isPlayersTurn)
			{
				_isPlayersTurn = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayersTurn");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGameUsingDice
	{
		get
		{
			return _isGameUsingDice;
		}
		set
		{
			if (value != _isGameUsingDice)
			{
				_isGameUsingDice = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsGameUsingDice");
			}
		}
	}

	[DataSourceProperty]
	public string DiceResult
	{
		get
		{
			return _diceResult;
		}
		set
		{
			if (value != _diceResult)
			{
				_diceResult = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DiceResult");
			}
		}
	}

	[DataSourceProperty]
	public string RollDiceText
	{
		get
		{
			return _rollDiceText;
		}
		set
		{
			if (value != _rollDiceText)
			{
				_rollDiceText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RollDiceText");
			}
		}
	}

	[DataSourceProperty]
	public string TurnOwnerText
	{
		get
		{
			return _turnOwnerText;
		}
		set
		{
			if (value != _turnOwnerText)
			{
				_turnOwnerText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TurnOwnerText");
			}
		}
	}

	[DataSourceProperty]
	public string BoardGameType
	{
		get
		{
			return _boardGameType;
		}
		set
		{
			if (value != _boardGameType)
			{
				_boardGameType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BoardGameType");
			}
		}
	}

	[DataSourceProperty]
	public string CloseText
	{
		get
		{
			return _closeText;
		}
		set
		{
			if (value != _closeText)
			{
				_closeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CloseText");
			}
		}
	}

	[DataSourceProperty]
	public string ForfeitText
	{
		get
		{
			return _forfeitText;
		}
		set
		{
			if (value != _forfeitText)
			{
				_forfeitText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ForfeitText");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM RollDiceKey
	{
		get
		{
			return _rollDiceKey;
		}
		set
		{
			if (value != _rollDiceKey)
			{
				_rollDiceKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "RollDiceKey");
			}
		}
	}

	public BoardGameVM()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		_missionBoardGameHandler = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		BoardGameType = ((object)_missionBoardGameHandler.CurrentBoardGame/*cast due to .constrained prefix*/).ToString();
		IsGameUsingDice = _missionBoardGameHandler.RequiresDiceRolling();
		DiceResult = "-";
		Instructions = new BoardGameInstructionsVM(_missionBoardGameHandler.CurrentBoardGame);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		RollDiceText = ((object)GameTexts.FindText("str_roll_dice", (string)null)).ToString();
		CloseText = ((object)GameTexts.FindText("str_close", (string)null)).ToString();
		ForfeitText = ((object)GameTexts.FindText("str_forfeit", (string)null)).ToString();
	}

	public void Activate()
	{
		SwitchTurns();
	}

	public void DiceRoll(int roll)
	{
		DiceResult = roll.ToString();
	}

	public void SwitchTurns()
	{
		IsPlayersTurn = _missionBoardGameHandler.Board.PlayerTurn == PlayerTurn.PlayerOne || _missionBoardGameHandler.Board.PlayerTurn == PlayerTurn.PlayerOneWaiting;
		TurnOwnerText = (IsPlayersTurn ? ((object)GameTexts.FindText("str_your_turn", (string)null)).ToString() : ((object)GameTexts.FindText("str_opponents_turn", (string)null)).ToString());
		DiceResult = "-";
		CanRoll = IsPlayersTurn && IsGameUsingDice;
	}

	public void ExecuteRoll()
	{
		if (CanRoll)
		{
			_missionBoardGameHandler.RollDice();
			CanRoll = false;
		}
	}

	public void ExecuteForfeit()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		if (_missionBoardGameHandler.Board.IsReady && _missionBoardGameHandler.IsGameInProgress)
		{
			TextObject val = new TextObject("{=azJulvrp}{?IS_BETTING}You are going to lose {BET_AMOUNT}{GOLD_ICON} if you forfeit.{newline}{?}{\\?}Do you really want to forfeit?", (Dictionary<string, object>)null);
			val.SetTextVariable("IS_BETTING", (_missionBoardGameHandler.BetAmount > 0) ? 1 : 0);
			val.SetTextVariable("BET_AMOUNT", _missionBoardGameHandler.BetAmount);
			val.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
			val.SetTextVariable("newline", "{=!}\n");
			InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_forfeit", (string)null)).ToString(), ((object)val).ToString(), true, true, ((object)new TextObject("{=aeouhelq}Yes", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=8OkPHu4f}No", (Dictionary<string, object>)null)).ToString(), (Action)_missionBoardGameHandler.ForfeitGame, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM rollDiceKey = RollDiceKey;
		if (rollDiceKey != null)
		{
			((ViewModel)rollDiceKey).OnFinalize();
		}
	}

	public void SetRollDiceKey(HotKey key)
	{
		RollDiceKey = InputKeyItemVM.CreateFromHotKey(key, isConsoleOnly: false);
	}
}
