using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.BoardGame;

public class BoardGameInstructionsVM : ViewModel
{
	private readonly BoardGameType _boardGameType;

	private int _currentInstructionIndex;

	private bool _isPreviousButtonEnabled;

	private bool _isNextButtonEnabled;

	private string _instructionsText;

	private string _previousText;

	private string _nextText;

	private string _currentPageText;

	private MBBindingList<BoardGameInstructionVM> _instructionList;

	[DataSourceProperty]
	public bool IsPreviousButtonEnabled
	{
		get
		{
			return _isPreviousButtonEnabled;
		}
		set
		{
			if (value != _isPreviousButtonEnabled)
			{
				_isPreviousButtonEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPreviousButtonEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNextButtonEnabled
	{
		get
		{
			return _isNextButtonEnabled;
		}
		set
		{
			if (value != _isNextButtonEnabled)
			{
				_isNextButtonEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNextButtonEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string InstructionsText
	{
		get
		{
			return _instructionsText;
		}
		set
		{
			if (value != _instructionsText)
			{
				_instructionsText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "InstructionsText");
			}
		}
	}

	[DataSourceProperty]
	public string PreviousText
	{
		get
		{
			return _previousText;
		}
		set
		{
			if (value != _previousText)
			{
				_previousText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PreviousText");
			}
		}
	}

	[DataSourceProperty]
	public string NextText
	{
		get
		{
			return _nextText;
		}
		set
		{
			if (value != _nextText)
			{
				_nextText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NextText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentPageText
	{
		get
		{
			return _currentPageText;
		}
		set
		{
			if (value != _currentPageText)
			{
				_currentPageText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentPageText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<BoardGameInstructionVM> InstructionList
	{
		get
		{
			return _instructionList;
		}
		set
		{
			if (value != _instructionList)
			{
				_instructionList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<BoardGameInstructionVM>>(value, "InstructionList");
			}
		}
	}

	public BoardGameInstructionsVM(BoardGameType boardGameType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		_boardGameType = boardGameType;
		InstructionList = new MBBindingList<BoardGameInstructionVM>();
		for (int i = 0; i < GetNumberOfInstructions(_boardGameType); i++)
		{
			((Collection<BoardGameInstructionVM>)(object)InstructionList).Add(new BoardGameInstructionVM(_boardGameType, i));
		}
		_currentInstructionIndex = 0;
		if (((Collection<BoardGameInstructionVM>)(object)InstructionList).Count > 0)
		{
			((Collection<BoardGameInstructionVM>)(object)InstructionList)[0].IsEnabled = true;
		}
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		InstructionsText = ((object)GameTexts.FindText("str_how_to_play", (string)null)).ToString();
		PreviousText = ((object)GameTexts.FindText("str_previous", (string)null)).ToString();
		NextText = ((object)GameTexts.FindText("str_next", (string)null)).ToString();
		InstructionList.ApplyActionOnAllItems((Action<BoardGameInstructionVM>)delegate(BoardGameInstructionVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		if (_currentInstructionIndex >= 0 && _currentInstructionIndex < ((Collection<BoardGameInstructionVM>)(object)InstructionList).Count)
		{
			TextObject val = new TextObject("{=hUSmlhNh}{CURRENT_PAGE}/{TOTAL_PAGES}", (Dictionary<string, object>)null);
			val.SetTextVariable("CURRENT_PAGE", (_currentInstructionIndex + 1).ToString());
			val.SetTextVariable("TOTAL_PAGES", ((Collection<BoardGameInstructionVM>)(object)InstructionList).Count.ToString());
			CurrentPageText = ((object)val).ToString();
			IsPreviousButtonEnabled = _currentInstructionIndex != 0;
			IsNextButtonEnabled = _currentInstructionIndex < ((Collection<BoardGameInstructionVM>)(object)InstructionList).Count - 1;
		}
	}

	public void ExecuteShowPrevious()
	{
		if (_currentInstructionIndex > 0 && _currentInstructionIndex < ((Collection<BoardGameInstructionVM>)(object)InstructionList).Count)
		{
			((Collection<BoardGameInstructionVM>)(object)InstructionList)[_currentInstructionIndex].IsEnabled = false;
			_currentInstructionIndex--;
			((Collection<BoardGameInstructionVM>)(object)InstructionList)[_currentInstructionIndex].IsEnabled = true;
			((ViewModel)this).RefreshValues();
		}
	}

	public void ExecuteShowNext()
	{
		if (_currentInstructionIndex >= 0 && _currentInstructionIndex < ((Collection<BoardGameInstructionVM>)(object)InstructionList).Count - 1)
		{
			((Collection<BoardGameInstructionVM>)(object)InstructionList)[_currentInstructionIndex].IsEnabled = false;
			_currentInstructionIndex++;
			((Collection<BoardGameInstructionVM>)(object)InstructionList)[_currentInstructionIndex].IsEnabled = true;
			((ViewModel)this).RefreshValues();
		}
	}

	private int GetNumberOfInstructions(BoardGameType game)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected I4, but got Unknown
		return (int)game switch
		{
			5 => 4, 
			2 => 3, 
			3 => 2, 
			1 => 5, 
			0 => 4, 
			4 => 4, 
			_ => 0, 
		};
	}
}
