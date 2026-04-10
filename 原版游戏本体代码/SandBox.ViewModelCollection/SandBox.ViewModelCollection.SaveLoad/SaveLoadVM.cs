using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SandBox.ViewModelCollection.Input;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.ScreenSystem;

namespace SandBox.ViewModelCollection.SaveLoad;

public class SaveLoadVM : ViewModel
{
	private const int _maxSaveFileNameLength = 30;

	private readonly TextObject _categorizedSaveGroupName = new TextObject("{=nVGqjtaa}Campaign {ID}", (Dictionary<string, object>)null);

	private readonly TextObject _uncategorizedSaveGroupName = new TextObject("{=uncategorized_save}Uncategorized", (Dictionary<string, object>)null);

	private readonly TextObject _textIsEmptyReasonText = new TextObject("{=7AI8jA0b}Input text cannot be empty.", (Dictionary<string, object>)null);

	private readonly TextObject _textHasSpecialCharReasonText = new TextObject("{=kXRdeawC}Input text cannot include special characters.", (Dictionary<string, object>)null);

	private readonly TextObject _textTooLongReasonText = new TextObject("{=B3W6fcQX}Input text cannot be longer than {MAX_LENGTH} characters.", (Dictionary<string, object>)null);

	private readonly TextObject _saveAlreadyExistsReasonText = new TextObject("{=aG6XMhA1}A saved game file already exists with this name.", (Dictionary<string, object>)null);

	private readonly TextObject _saveNameReservedReasonText = new TextObject("{=M4WMKyE1}Input text includes reserved text.", (Dictionary<string, object>)null);

	private readonly TextObject _allSpaceReasonText = new TextObject("{=Rtakaivj}Input text needs to include at least one non-space character.", (Dictionary<string, object>)null);

	private readonly TextObject _visualIsDisabledText = new TextObject("{=xlEZ02Qw}Character visual is disabled during 'Save As' on the campaign map.", (Dictionary<string, object>)null);

	private bool _isLoadingSaves;

	private bool _isBusyWithAnAction;

	private bool _isSearchAvailable;

	private string _searchText;

	private string _searchPlaceholderText;

	private string _doneText;

	private string _createNewSaveSlotText;

	private string _titleText;

	private string _visualDisabledText;

	private bool _isSaving;

	private bool _isActionEnabled;

	private bool _isAnyItemSelected;

	private bool _canCreateNewSave;

	private bool _isVisualDisabled;

	private string _saveLoadText;

	private string _cancelText;

	private HintViewModel _createNewSaveHint;

	private MBBindingList<SavedGameGroupVM> _saveGroups;

	private SavedGameVM _currentSelectedSave;

	private InputKeyItemVM _doneInputKey;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _deleteInputKey;

	private IEnumerable<SaveGameFileInfo> _allSavedGames => ((IEnumerable<SavedGameGroupVM>)SaveGroups).SelectMany((SavedGameGroupVM s) => ((IEnumerable<SavedGameVM>)s.SavedGamesList).Select((SavedGameVM v) => v.Save));

	[DataSourceProperty]
	public bool IsLoadingSaves
	{
		get
		{
			return _isLoadingSaves;
		}
		set
		{
			if (value != _isLoadingSaves)
			{
				_isLoadingSaves = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLoadingSaves");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBusyWithAnAction
	{
		get
		{
			return _isBusyWithAnAction;
		}
		set
		{
			if (value != _isBusyWithAnAction)
			{
				_isBusyWithAnAction = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBusyWithAnAction");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSearchAvailable
	{
		get
		{
			return _isSearchAvailable;
		}
		set
		{
			if (value != _isSearchAvailable)
			{
				_isSearchAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSearchAvailable");
			}
		}
	}

	[DataSourceProperty]
	public string SearchText
	{
		get
		{
			return _searchText;
		}
		set
		{
			if (value != _searchText)
			{
				value.IndexOf(_searchText ?? "");
				_searchText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SearchText");
			}
		}
	}

	[DataSourceProperty]
	public string SearchPlaceholderText
	{
		get
		{
			return _searchPlaceholderText;
		}
		set
		{
			if (value != _searchPlaceholderText)
			{
				_searchPlaceholderText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SearchPlaceholderText");
			}
		}
	}

	[DataSourceProperty]
	public string VisualDisabledText
	{
		get
		{
			return _visualDisabledText;
		}
		set
		{
			if (value != _visualDisabledText)
			{
				_visualDisabledText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "VisualDisabledText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SavedGameGroupVM> SaveGroups
	{
		get
		{
			return _saveGroups;
		}
		set
		{
			if (value != _saveGroups)
			{
				_saveGroups = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SavedGameGroupVM>>(value, "SaveGroups");
			}
		}
	}

	[DataSourceProperty]
	public SavedGameVM CurrentSelectedSave
	{
		get
		{
			return _currentSelectedSave;
		}
		set
		{
			if (value != _currentSelectedSave)
			{
				_currentSelectedSave = value;
				((ViewModel)this).OnPropertyChangedWithValue<SavedGameVM>(value, "CurrentSelectedSave");
			}
		}
	}

	[DataSourceProperty]
	public string CreateNewSaveSlotText
	{
		get
		{
			return _createNewSaveSlotText;
		}
		set
		{
			if (value != _createNewSaveSlotText)
			{
				_createNewSaveSlotText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CreateNewSaveSlotText");
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
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSaving
	{
		get
		{
			return _isSaving;
		}
		set
		{
			if (value != _isSaving)
			{
				_isSaving = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSaving");
			}
		}
	}

	[DataSourceProperty]
	public bool CanCreateNewSave
	{
		get
		{
			return _canCreateNewSave;
		}
		set
		{
			if (value != _canCreateNewSave)
			{
				_canCreateNewSave = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanCreateNewSave");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVisualDisabled
	{
		get
		{
			return _isVisualDisabled;
		}
		set
		{
			if (value != _isVisualDisabled)
			{
				_isVisualDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisualDisabled");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CreateNewSaveHint
	{
		get
		{
			return _createNewSaveHint;
		}
		set
		{
			if (value != _createNewSaveHint)
			{
				_createNewSaveHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CreateNewSaveHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsActionEnabled
	{
		get
		{
			return _isActionEnabled;
		}
		set
		{
			if (value != _isActionEnabled)
			{
				_isActionEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActionEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAnyItemSelected
	{
		get
		{
			return _isAnyItemSelected;
		}
		set
		{
			if (value != _isAnyItemSelected)
			{
				_isAnyItemSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAnyItemSelected");
			}
		}
	}

	[DataSourceProperty]
	public string DoneText
	{
		get
		{
			return _doneText;
		}
		set
		{
			if (value != _doneText)
			{
				_doneText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DoneText");
			}
		}
	}

	[DataSourceProperty]
	public string SaveLoadText
	{
		get
		{
			return _saveLoadText;
		}
		set
		{
			if (value != _saveLoadText)
			{
				_saveLoadText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SaveLoadText");
			}
		}
	}

	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "DoneInputKey");
			}
		}
	}

	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "CancelInputKey");
			}
		}
	}

	public InputKeyItemVM DeleteInputKey
	{
		get
		{
			return _deleteInputKey;
		}
		set
		{
			if (value != _deleteInputKey)
			{
				_deleteInputKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "DeleteInputKey");
			}
		}
	}

	public SaveLoadVM(bool isSaving, bool isCampaignMapOnStack)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		_isSaving = isSaving;
		SaveGroups = new MBBindingList<SavedGameGroupVM>();
		IsVisualDisabled = false;
	}

	public async void Initialize()
	{
		IsBusyWithAnAction = true;
		IsLoadingSaves = true;
		int num = 0;
		SaveGameFileInfo[] saveFiles = MBSaveLoad.GetSaveFiles((Func<SaveGameFileInfo, bool>)null);
		IEnumerable<SaveGameFileInfo> enumerable = saveFiles.Where((SaveGameFileInfo s) => s.IsCorrupted);
		foreach (IGrouping<string, SaveGameFileInfo> item in from m in saveFiles
			where !m.IsCorrupted
			group m by MetaDataExtensions.GetUniqueGameId(m.MetaData) into s
			orderby GetMostRecentSaveInGroup(s) descending
			select s)
		{
			SavedGameGroupVM savedGameGroupVM = new SavedGameGroupVM();
			if (string.IsNullOrWhiteSpace(item.Key))
			{
				savedGameGroupVM.IdentifierID = ((object)_uncategorizedSaveGroupName).ToString();
			}
			else
			{
				num++;
				_categorizedSaveGroupName.SetTextVariable("ID", num);
				savedGameGroupVM.IdentifierID = ((object)_categorizedSaveGroupName).ToString();
			}
			foreach (SaveGameFileInfo item2 in item.OrderByDescending((SaveGameFileInfo s) => MetaDataExtensions.GetCreationTime(s.MetaData)))
			{
				bool ironmanMode = MetaDataExtensions.GetIronmanMode(item2.MetaData);
				((Collection<SavedGameVM>)(object)savedGameGroupVM.SavedGamesList).Add(new SavedGameVM(item2, IsSaving, OnDeleteSavedGame, OnSaveSelection, OnCancelLoadSave, ExecuteDone, isCorruptedSave: false, ironmanMode));
			}
			((Collection<SavedGameGroupVM>)(object)SaveGroups).Add(savedGameGroupVM);
		}
		if (enumerable.Any())
		{
			SavedGameGroupVM savedGameGroupVM2 = new SavedGameGroupVM
			{
				IdentifierID = ((object)new TextObject("{=o9PIe7am}Corrupted", (Dictionary<string, object>)null)).ToString()
			};
			foreach (SaveGameFileInfo item3 in enumerable)
			{
				((Collection<SavedGameVM>)(object)savedGameGroupVM2.SavedGamesList).Add(new SavedGameVM(item3, IsSaving, OnDeleteSavedGame, OnSaveSelection, OnCancelLoadSave, ExecuteDone, isCorruptedSave: true));
			}
			((Collection<SavedGameGroupVM>)(object)SaveGroups).Add(savedGameGroupVM2);
		}
		RefreshCanCreateNewSave();
		RefreshCanSearch();
		OnSaveSelection(GetFirstAvailableSavedGame());
		((ViewModel)this).RefreshValues();
		await Task.Delay(1);
		IsBusyWithAnAction = false;
		IsLoadingSaves = false;
	}

	private SavedGameVM GetFirstAvailableSavedGame()
	{
		for (int i = 0; i < ((Collection<SavedGameGroupVM>)(object)SaveGroups).Count; i++)
		{
			SavedGameGroupVM savedGameGroupVM = ((Collection<SavedGameGroupVM>)(object)SaveGroups)[i];
			for (int j = 0; j < ((Collection<SavedGameVM>)(object)savedGameGroupVM.SavedGamesList).Count; j++)
			{
				SavedGameVM savedGameVM = ((Collection<SavedGameVM>)(object)savedGameGroupVM.SavedGamesList)[j];
				if (!savedGameVM.IsCorrupted && !savedGameVM.IsDisabled)
				{
					return savedGameVM;
				}
			}
		}
		return null;
	}

	private void RefreshCanCreateNewSave()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		CanCreateNewSave = !MBSaveLoad.IsMaxNumberOfSavesReached();
		CreateNewSaveHint = new HintViewModel(CanCreateNewSave ? ((TextObject)null) : new TextObject("{=DeXfSjgY}Cannot create a new save. Save limit reached.", (Dictionary<string, object>)null), (string)null);
	}

	private void RefreshCanSearch()
	{
		IsSearchAvailable = true;
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=hiCxFj4E}Saved Campaigns", (Dictionary<string, object>)null)).ToString();
		DoneText = ((object)new TextObject("{=WiNRdfsm}Done", (Dictionary<string, object>)null)).ToString();
		CreateNewSaveSlotText = ((object)new TextObject("{=eL8nhkhQ}Create New Save Slot", (Dictionary<string, object>)null)).ToString();
		CancelText = ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString();
		SaveLoadText = (_isSaving ? ((object)new TextObject("{=bV75iwKa}Save", (Dictionary<string, object>)null)).ToString() : ((object)new TextObject("{=9NuttOBC}Load", (Dictionary<string, object>)null)).ToString());
		SearchPlaceholderText = ((object)new TextObject("{=tQOPRBFg}Search...", (Dictionary<string, object>)null)).ToString();
		if (IsVisualDisabled)
		{
			VisualDisabledText = ((object)_visualIsDisabledText).ToString();
		}
		SaveGroups.ApplyActionOnAllItems((Action<SavedGameGroupVM>)delegate(SavedGameGroupVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		SavedGameVM currentSelectedSave = CurrentSelectedSave;
		if (currentSelectedSave != null)
		{
			((ViewModel)currentSelectedSave).RefreshValues();
		}
	}

	private DateTime GetMostRecentSaveInGroup(IGrouping<string, SaveGameFileInfo> group)
	{
		SaveGameFileInfo? obj = group.OrderByDescending((SaveGameFileInfo g) => MetaDataExtensions.GetCreationTime(g.MetaData)).FirstOrDefault();
		if (obj == null)
		{
			return default(DateTime);
		}
		return MetaDataExtensions.GetCreationTime(obj.MetaData);
	}

	private void OnSaveSelection(SavedGameVM save)
	{
		if (save != CurrentSelectedSave)
		{
			if (CurrentSelectedSave != null)
			{
				CurrentSelectedSave.IsSelected = false;
			}
			CurrentSelectedSave = save;
			if (CurrentSelectedSave != null)
			{
				CurrentSelectedSave.IsSelected = true;
			}
			IsAnyItemSelected = CurrentSelectedSave != null;
			IsActionEnabled = IsAnyItemSelected && !CurrentSelectedSave.IsCorrupted && !CurrentSelectedSave.IsDisabled;
		}
	}

	public void ExecuteCreateNewSaveGame()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		InformationManager.ShowTextInquiry(new TextInquiryData(((object)new TextObject("{=7WdWK2Dt}Save Game", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=WDlVhNuq}Name your save file", (Dictionary<string, object>)null)).ToString(), true, true, ((object)new TextObject("{=WiNRdfsm}Done", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action<string>)OnSaveAsDone, (Action)null, false, (Func<string, Tuple<bool, string>>)IsSaveGameNameApplicable, "", ""), false, false);
	}

	private Tuple<bool, string> IsSaveGameNameApplicable(string inputText)
	{
		string item = string.Empty;
		bool item2 = true;
		if (string.IsNullOrEmpty(inputText))
		{
			item = ((object)_textIsEmptyReasonText).ToString();
			item2 = false;
		}
		else if (inputText.All((char c) => char.IsWhiteSpace(c)))
		{
			item = ((object)_allSpaceReasonText).ToString();
			item2 = false;
		}
		else if (inputText.Any((char c) => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
		{
			item = ((object)_textHasSpecialCharReasonText).ToString();
			item2 = false;
		}
		else if (inputText.Length >= 30)
		{
			_textTooLongReasonText.SetTextVariable("MAX_LENGTH", 30);
			item = ((object)_textTooLongReasonText).ToString();
			item2 = false;
		}
		else if (MBSaveLoad.IsSaveFileNameReserved(inputText))
		{
			item = ((object)_saveNameReservedReasonText).ToString();
			item2 = false;
		}
		else if (_allSavedGames.Any((SaveGameFileInfo s) => string.Equals(s.Name, inputText, StringComparison.InvariantCultureIgnoreCase)))
		{
			item = ((object)_saveAlreadyExistsReasonText).ToString();
			item2 = false;
		}
		return new Tuple<bool, string>(item2, item);
	}

	private void OnSaveAsDone(string saveName)
	{
		Campaign.Current.SaveHandler.SaveAs(saveName);
		ExecuteDone();
	}

	public void ExecuteDone()
	{
		ScreenManager.PopScreen();
	}

	public void ExecuteLoadSave()
	{
		LoadSelectedSave();
	}

	private void LoadSelectedSave()
	{
		if (!IsBusyWithAnAction && CurrentSelectedSave != null && !CurrentSelectedSave.IsCorrupted && !CurrentSelectedSave.IsDisabled)
		{
			CurrentSelectedSave.ExecuteSaveLoad();
			IsBusyWithAnAction = true;
		}
	}

	private void OnCancelLoadSave()
	{
		IsBusyWithAnAction = false;
	}

	private void ExecuteResetCurrentSave()
	{
		CurrentSelectedSave = null;
	}

	private void OnDeleteSavedGame(SavedGameVM savedGame)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		if (IsBusyWithAnAction)
		{
			return;
		}
		IsBusyWithAnAction = true;
		string text = ((object)new TextObject("{=M1AEHJ76}Please notice that this save is created for a session which has Ironman mode enabled. There is no other save file for the related session. Are you sure you want to delete this save game?", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=HH2mZq8J}Are you sure you want to delete this save game?", (Dictionary<string, object>)null)).ToString();
		string? text3 = ((object)new TextObject("{=QHV8aeEg}Delete Save", (Dictionary<string, object>)null)).ToString();
		string text4 = (MetaDataExtensions.GetIronmanMode(savedGame.Save.MetaData) ? text : text2);
		InformationManager.ShowInquiry(new InquiryData(text3, text4, true, true, ((object)new TextObject("{=aeouhelq}Yes", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=8OkPHu4f}No", (Dictionary<string, object>)null)).ToString(), (Action)delegate
		{
			IsBusyWithAnAction = true;
			bool num = MBSaveLoad.DeleteSaveGame(savedGame.Save.Name);
			IsBusyWithAnAction = false;
			if (num)
			{
				DeleteSave(savedGame);
				OnSaveSelection(GetFirstAvailableSavedGame());
				RefreshCanCreateNewSave();
				RefreshCanSearch();
			}
			else
			{
				OnDeleteSaveUnsuccessful();
			}
		}, (Action)delegate
		{
			IsBusyWithAnAction = false;
		}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void OnDeleteSaveUnsuccessful()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		string? text = ((object)new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=PY00wRz4}Failed to delete the save file.", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, false, ((object)new TextObject("{=WiNRdfsm}Done", (Dictionary<string, object>)null)).ToString(), string.Empty, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void DeleteSave(SavedGameVM save)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		foreach (SavedGameGroupVM item in (Collection<SavedGameGroupVM>)(object)SaveGroups)
		{
			if (((Collection<SavedGameVM>)(object)item.SavedGamesList).Contains(save))
			{
				((Collection<SavedGameVM>)(object)item.SavedGamesList).Remove(save);
				break;
			}
		}
		if (string.IsNullOrEmpty(BannerlordConfig.LatestSaveGameName) || save.Save.Name == BannerlordConfig.LatestSaveGameName)
		{
			BannerlordConfig.LatestSaveGameName = GetFirstAvailableSavedGame()?.Save.Name ?? string.Empty;
			BannerlordConfig.Save();
		}
	}

	public void DeleteSelectedSave()
	{
		if (CurrentSelectedSave != null)
		{
			OnDeleteSavedGame(CurrentSelectedSave);
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).OnFinalize();
		}
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
		InputKeyItemVM deleteInputKey = DeleteInputKey;
		if (deleteInputKey != null)
		{
			((ViewModel)deleteInputKey).OnFinalize();
		}
	}

	public void SetDoneInputKey(HotKey hotkey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, isConsoleOnly: true);
	}

	public void SetCancelInputKey(HotKey hotkey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, isConsoleOnly: true);
	}

	public void SetDeleteInputKey(HotKey hotkey)
	{
		DeleteInputKey = InputKeyItemVM.CreateFromHotKey(hotkey, isConsoleOnly: true);
	}
}
