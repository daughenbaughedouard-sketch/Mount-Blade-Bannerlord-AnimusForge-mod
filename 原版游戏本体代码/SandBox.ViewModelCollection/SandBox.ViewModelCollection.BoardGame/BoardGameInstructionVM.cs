using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.BoardGame;

public class BoardGameInstructionVM : ViewModel
{
	private readonly BoardGameType _game;

	private readonly int _instructionIndex;

	private bool _isEnabled;

	private string _titleText;

	private string _descriptionText;

	private string _gameType;

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (value != _descriptionText)
			{
				_descriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public string GameType
	{
		get
		{
			return _gameType;
		}
		set
		{
			if (value != _gameType)
			{
				_gameType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameType");
			}
		}
	}

	public BoardGameInstructionVM(BoardGameType game, int instructionIndex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_game = game;
		_instructionIndex = instructionIndex;
		GameType = ((object)Unsafe.As<BoardGameType, BoardGameType>(ref _game)/*cast due to .constrained prefix*/).ToString();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		GameTexts.SetVariable("newline", "\n");
		TitleText = ((object)GameTexts.FindText("str_board_game_title", ((object)Unsafe.As<BoardGameType, BoardGameType>(ref _game)/*cast due to .constrained prefix*/).ToString() + "_" + _instructionIndex)).ToString();
		DescriptionText = ((object)GameTexts.FindText("str_board_game_instruction", ((object)Unsafe.As<BoardGameType, BoardGameType>(ref _game)/*cast due to .constrained prefix*/).ToString() + "_" + _instructionIndex)).ToString();
	}
}
