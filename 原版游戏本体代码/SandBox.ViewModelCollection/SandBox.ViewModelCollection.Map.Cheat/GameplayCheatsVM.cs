using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandBox.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.Map.Cheat;

public class GameplayCheatsVM : ViewModel
{
	private readonly Action _onClose;

	private readonly IEnumerable<GameplayCheatBase> _initialCheatList;

	private readonly TextObject _mainTitleText;

	private List<CheatGroupItemVM> _activeCheatGroups;

	private string _title;

	private string _buttonCloseLabel;

	private MBBindingList<CheatItemBaseVM> _cheats;

	private InputKeyItemVM _closeInputKey;

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public string ButtonCloseLabel
	{
		get
		{
			return _buttonCloseLabel;
		}
		set
		{
			if (value != _buttonCloseLabel)
			{
				_buttonCloseLabel = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ButtonCloseLabel");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<CheatItemBaseVM> Cheats
	{
		get
		{
			return _cheats;
		}
		set
		{
			if (value != _cheats)
			{
				_cheats = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<CheatItemBaseVM>>(value, "Cheats");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM CloseInputKey
	{
		get
		{
			return _closeInputKey;
		}
		set
		{
			if (value != _closeInputKey)
			{
				_closeInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CloseInputKey");
			}
		}
	}

	public GameplayCheatsVM(Action onClose, IEnumerable<GameplayCheatBase> cheats)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		_onClose = onClose;
		_initialCheatList = cheats;
		Cheats = new MBBindingList<CheatItemBaseVM>();
		_activeCheatGroups = new List<CheatGroupItemVM>();
		_mainTitleText = new TextObject("{=OYtysXzk}Cheats", (Dictionary<string, object>)null);
		FillWithCheats(cheats);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		for (int i = 0; i < ((Collection<CheatItemBaseVM>)(object)Cheats).Count; i++)
		{
			((ViewModel)((Collection<CheatItemBaseVM>)(object)Cheats)[i]).RefreshValues();
		}
		if (_activeCheatGroups.Count > 0)
		{
			TextObject val = new TextObject("{=1tiF5JhE}{TITLE} > {SUBTITLE}", (Dictionary<string, object>)null);
			for (int j = 0; j < _activeCheatGroups.Count; j++)
			{
				if (j == 0)
				{
					val.SetTextVariable("TITLE", ((object)_mainTitleText).ToString());
				}
				else
				{
					val.SetTextVariable("TITLE", ((object)val).ToString());
				}
				val.SetTextVariable("SUBTITLE", _activeCheatGroups[j].Name.ToString());
			}
			Title = ((object)val).ToString();
			ButtonCloseLabel = ((object)GameTexts.FindText("str_back", (string)null)).ToString();
		}
		else
		{
			Title = ((object)_mainTitleText).ToString();
			ButtonCloseLabel = ((object)GameTexts.FindText("str_close", (string)null)).ToString();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM closeInputKey = CloseInputKey;
		if (closeInputKey != null)
		{
			((ViewModel)closeInputKey).OnFinalize();
		}
	}

	private void FillWithCheats(IEnumerable<GameplayCheatBase> cheats)
	{
		((Collection<CheatItemBaseVM>)(object)Cheats).Clear();
		foreach (GameplayCheatBase cheat2 in cheats)
		{
			if (cheat2 is GameplayCheatItem cheat)
			{
				((Collection<CheatItemBaseVM>)(object)Cheats).Add((CheatItemBaseVM)new CheatActionItemVM(cheat, OnCheatActionExecuted));
			}
			else if (cheat2 is GameplayCheatGroup cheatGroup)
			{
				((Collection<CheatItemBaseVM>)(object)Cheats).Add((CheatItemBaseVM)new CheatGroupItemVM(cheatGroup, OnCheatGroupSelected));
			}
		}
		((ViewModel)this).RefreshValues();
	}

	private void OnCheatActionExecuted(CheatActionItemVM cheatItem)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		_activeCheatGroups.Clear();
		FillWithCheats(_initialCheatList);
		TextObject val = new TextObject("{=1QAEyN2V}Cheat Used: {CHEAT}", (Dictionary<string, object>)null);
		val.SetTextVariable("CHEAT", cheatItem.Name.ToString());
		InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
	}

	private void OnCheatGroupSelected(CheatGroupItemVM cheatGroup)
	{
		_activeCheatGroups.Add(cheatGroup);
		FillWithCheats(cheatGroup?.CheatGroup?.GetCheats() ?? _initialCheatList);
	}

	public void ExecuteClose()
	{
		if (_activeCheatGroups.Count > 0)
		{
			_activeCheatGroups.RemoveAt(_activeCheatGroups.Count - 1);
			if (_activeCheatGroups.Count > 0)
			{
				FillWithCheats(_activeCheatGroups[_activeCheatGroups.Count - 1].CheatGroup.GetCheats());
			}
			else
			{
				FillWithCheats(_initialCheatList);
			}
		}
		else
		{
			_onClose?.Invoke();
		}
	}

	public void SetCloseInputKey(HotKey hotKey)
	{
		CloseInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
	}
}
