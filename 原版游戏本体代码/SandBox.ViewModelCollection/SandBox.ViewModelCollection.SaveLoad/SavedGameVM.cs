using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;
using TaleWorlds.ScreenSystem;

namespace SandBox.ViewModelCollection.SaveLoad;

public class SavedGameVM : ViewModel
{
	private readonly bool _isSaving;

	private readonly Action _onDone;

	private readonly Action<SavedGameVM> _onDelete;

	private readonly Action<SavedGameVM> _onSelection;

	private readonly Action _onCancelLoadSave;

	private readonly TextObject _newlineTextObject = new TextObject("{=ol0rBSrb}{STR1}{newline}{STR2}", (Dictionary<string, object>)null);

	private readonly ApplicationVersion _gameVersion;

	private readonly ApplicationVersion _saveVersion;

	private readonly MBReadOnlyList<SandBoxSaveHelper.ModuleCheckResult> _moduleCheckResults;

	private MBBindingList<SavedGamePropertyVM> _savedGameProperties;

	private MBBindingList<SavedGameModuleInfoVM> _loadedModulesInSave;

	private HintViewModel _dateTimeHint;

	private HintViewModel _updateButtonHint;

	private HintViewModel _disabledReasonHint;

	private BannerImageIdentifierVM _clanBanner;

	private CharacterViewModel _characterVisual;

	private string _deleteText;

	private string _nameText;

	private string _gameTimeText;

	private string _realTimeText1;

	private string _realTimeText2;

	private string _levelText;

	private string _characterNameText;

	private string _saveLoadText;

	private string _overwriteSaveText;

	private string _updateSaveText;

	private string _modulesText;

	private string _corruptedSaveText;

	private string _saveVersionAsString;

	private string _mainHeroVisualCode;

	private string _bannerTextCode;

	private bool _isSelected;

	private bool _isCorrupted;

	private bool _isFilteredOut;

	private bool _isDisabled;

	public SaveGameFileInfo Save { get; }

	public bool RequiresInquiryOnLoad { get; private set; }

	public bool IsModuleDiscrepancyDetected { get; private set; }

	[DataSourceProperty]
	public MBBindingList<SavedGamePropertyVM> SavedGameProperties
	{
		get
		{
			return _savedGameProperties;
		}
		set
		{
			if (value != _savedGameProperties)
			{
				_savedGameProperties = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SavedGamePropertyVM>>(value, "SavedGameProperties");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<SavedGameModuleInfoVM> LoadedModulesInSave
	{
		get
		{
			return _loadedModulesInSave;
		}
		set
		{
			if (value != _loadedModulesInSave)
			{
				_loadedModulesInSave = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<SavedGameModuleInfoVM>>(value, "LoadedModulesInSave");
			}
		}
	}

	[DataSourceProperty]
	public string SaveVersionAsString
	{
		get
		{
			return _saveVersionAsString;
		}
		set
		{
			if (value != _saveVersionAsString)
			{
				_saveVersionAsString = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SaveVersionAsString");
			}
		}
	}

	[DataSourceProperty]
	public string DeleteText
	{
		get
		{
			return _deleteText;
		}
		set
		{
			if (value != _deleteText)
			{
				_deleteText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DeleteText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCorrupted
	{
		get
		{
			return _isCorrupted;
		}
		set
		{
			if (value != _isCorrupted)
			{
				_isCorrupted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCorrupted");
			}
		}
	}

	[DataSourceProperty]
	public string BannerTextCode
	{
		get
		{
			return _bannerTextCode;
		}
		set
		{
			if (value != _bannerTextCode)
			{
				_bannerTextCode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BannerTextCode");
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

	[DataSourceProperty]
	public string OverrideSaveText
	{
		get
		{
			return _overwriteSaveText;
		}
		set
		{
			if (value != _overwriteSaveText)
			{
				_overwriteSaveText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OverrideSaveText");
			}
		}
	}

	[DataSourceProperty]
	public string UpdateSaveText
	{
		get
		{
			return _updateSaveText;
		}
		set
		{
			if (value != _updateSaveText)
			{
				_updateSaveText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "UpdateSaveText");
			}
		}
	}

	[DataSourceProperty]
	public string ModulesText
	{
		get
		{
			return _modulesText;
		}
		set
		{
			if (value != _modulesText)
			{
				_modulesText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ModulesText");
			}
		}
	}

	[DataSourceProperty]
	public string CorruptedSaveText
	{
		get
		{
			return _corruptedSaveText;
		}
		set
		{
			if (value != _corruptedSaveText)
			{
				_corruptedSaveText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CorruptedSaveText");
			}
		}
	}

	[DataSourceProperty]
	public string NameText
	{
		get
		{
			return _nameText;
		}
		set
		{
			if (value != _nameText)
			{
				_nameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NameText");
			}
		}
	}

	[DataSourceProperty]
	public string GameTimeText
	{
		get
		{
			return _gameTimeText;
		}
		set
		{
			if (value != _gameTimeText)
			{
				_gameTimeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameTimeText");
			}
		}
	}

	[DataSourceProperty]
	public string CharacterNameText
	{
		get
		{
			return _characterNameText;
		}
		set
		{
			if (value != _characterNameText)
			{
				_characterNameText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CharacterNameText");
			}
		}
	}

	[DataSourceProperty]
	public string MainHeroVisualCode
	{
		get
		{
			return _mainHeroVisualCode;
		}
		set
		{
			if (value != _mainHeroVisualCode)
			{
				_mainHeroVisualCode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MainHeroVisualCode");
			}
		}
	}

	[DataSourceProperty]
	public CharacterViewModel CharacterVisual
	{
		get
		{
			return _characterVisual;
		}
		set
		{
			if (value != _characterVisual)
			{
				_characterVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterViewModel>(value, "CharacterVisual");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM ClanBanner
	{
		get
		{
			return _clanBanner;
		}
		set
		{
			if (value != _clanBanner)
			{
				_clanBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "ClanBanner");
			}
		}
	}

	[DataSourceProperty]
	public string RealTimeText1
	{
		get
		{
			return _realTimeText1;
		}
		set
		{
			if (value != _realTimeText1)
			{
				_realTimeText1 = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RealTimeText1");
			}
		}
	}

	[DataSourceProperty]
	public string RealTimeText2
	{
		get
		{
			return _realTimeText2;
		}
		set
		{
			if (value != _realTimeText2)
			{
				_realTimeText2 = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RealTimeText2");
			}
		}
	}

	[DataSourceProperty]
	public string LevelText
	{
		get
		{
			return _levelText;
		}
		set
		{
			if (value != _levelText)
			{
				_levelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LevelText");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DateTimeHint
	{
		get
		{
			return _dateTimeHint;
		}
		set
		{
			if (value != _dateTimeHint)
			{
				_dateTimeHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DateTimeHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel UpdateButtonHint
	{
		get
		{
			return _updateButtonHint;
		}
		set
		{
			if (value != _updateButtonHint)
			{
				_updateButtonHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "UpdateButtonHint");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel DisabledReasonHint
	{
		get
		{
			return _disabledReasonHint;
		}
		set
		{
			if (value != _disabledReasonHint)
			{
				_disabledReasonHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "DisabledReasonHint");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFilteredOut
	{
		get
		{
			return _isFilteredOut;
		}
		set
		{
			if (value != _isFilteredOut)
			{
				_isFilteredOut = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFilteredOut");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			if (value != _isDisabled)
			{
				_isDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisabled");
			}
		}
	}

	public SavedGameVM(SaveGameFileInfo save, bool isSaving, Action<SavedGameVM> onDelete, Action<SavedGameVM> onSelection, Action onCancelLoadSave, Action onDone, bool isCorruptedSave = false, bool isIronman = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		Save = save;
		_isSaving = isSaving;
		_onDelete = onDelete;
		_onSelection = onSelection;
		_onCancelLoadSave = onCancelLoadSave;
		_onDone = onDone;
		IsCorrupted = isCorruptedSave;
		SavedGameProperties = new MBBindingList<SavedGamePropertyVM>();
		LoadedModulesInSave = new MBBindingList<SavedGameModuleInfoVM>();
		if (isIronman)
		{
			GameTexts.SetVariable("RANK", MetaDataExtensions.GetCharacterName(Save.MetaData));
			GameTexts.SetVariable("NUMBER", new TextObject("{=Fm0rjkH7}Ironman", (Dictionary<string, object>)null));
			NameText = ((object)new TextObject("{=AVoWvlue}{RANK} ({NUMBER})", (Dictionary<string, object>)null)).ToString();
		}
		else
		{
			NameText = Save.Name;
		}
		_newlineTextObject.SetTextVariable("newline", "\n");
		_gameVersion = MBSaveLoad.CurrentVersion;
		_saveVersion = MetaDataExtensions.GetApplicationVersion(Save.MetaData);
		_moduleCheckResults = SandBoxSaveHelper.CheckMetaDataCompatibilityErrors(save.MetaData);
		IsModuleDiscrepancyDetected = isCorruptedSave || ((IEnumerable<SandBoxSaveHelper.ModuleCheckResult>)_moduleCheckResults).Any((SandBoxSaveHelper.ModuleCheckResult x) => ((int)x.Type == 1 && ((List<string>)(object)ModuleHelper.ModulesDisablingLoadingAfterBeingAdded).Contains(x.ModuleId)) || ((int)x.Type == 0 && ((List<string>)(object)ModuleHelper.ModulesDisablingLoadingAfterBeingRemoved).Contains(x.ModuleId)));
		MainHeroVisualCode = (IsModuleDiscrepancyDetected ? string.Empty : MetaDataExtensions.GetCharacterVisualCode(Save.MetaData));
		BannerTextCode = (IsModuleDiscrepancyDetected ? string.Empty : MetaDataExtensions.GetClanBannerCode(Save.MetaData));
		IsDisabled = SandBoxSaveHelper.GetIsDisabledWithReason(Save, out var reason);
		DisabledReasonHint = new HintViewModel(reason, (string)null);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Expected O, but got Unknown
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Expected O, but got Unknown
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Expected O, but got Unknown
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Expected O, but got Unknown
		//IL_0313: Expected O, but got Unknown
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Expected O, but got Unknown
		//IL_0349: Expected O, but got Unknown
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Expected O, but got Unknown
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Expected O, but got Unknown
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Expected O, but got Unknown
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Expected O, but got Unknown
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Expected O, but got Unknown
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Expected O, but got Unknown
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Expected O, but got Unknown
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Expected O, but got Unknown
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Expected O, but got Unknown
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Expected O, but got Unknown
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		((Collection<SavedGameModuleInfoVM>)(object)LoadedModulesInSave).Clear();
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Clear();
		SaveVersionAsString = ((object)_saveVersion/*cast due to .constrained prefix*/).ToString();
		if (_gameVersion != _saveVersion)
		{
			RequiresInquiryOnLoad = true;
		}
		string[] modules = MetaDataExtensions.GetModules(Save.MetaData);
		foreach (string text in modules)
		{
			string value = ((object)MetaDataExtensions.GetModuleVersion(Save.MetaData, text)/*cast due to .constrained prefix*/).ToString();
			((Collection<SavedGameModuleInfoVM>)(object)LoadedModulesInSave).Add(new SavedGameModuleInfoVM(SandBoxSaveHelper.GetModuleNameFromModuleId(text), "", value));
		}
		CharacterNameText = MetaDataExtensions.GetCharacterName(Save.MetaData);
		ClanBanner = new BannerImageIdentifierVM(new Banner(MetaDataExtensions.GetClanBannerCode(Save.MetaData)), true);
		DeleteText = ((object)new TextObject("{=deleteaction}Delete", (Dictionary<string, object>)null)).ToString();
		ModulesText = ((object)new TextObject("{=JXyxj1J5}Modules", (Dictionary<string, object>)null)).ToString();
		DateTime creationTime = MetaDataExtensions.GetCreationTime(Save.MetaData);
		RealTimeText1 = LocalizedTextManager.GetDateFormattedByLanguage(BannerlordConfig.Language, creationTime);
		RealTimeText2 = LocalizedTextManager.GetTimeFormattedByLanguage(BannerlordConfig.Language, creationTime);
		int playerHealthPercentage = MetaDataExtensions.GetPlayerHealthPercentage(Save.MetaData);
		TextObject val = new TextObject("{=gYATKZJp}{NUMBER}%", (Dictionary<string, object>)null);
		val.SetTextVariable("NUMBER", playerHealthPercentage.ToString());
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.Health, val, new TextObject("{=hZrwUIaq}Health", (Dictionary<string, object>)null)));
		int mainHeroGold = MetaDataExtensions.GetMainHeroGold(Save.MetaData);
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.Gold, GetAbbreviatedValueTextFromValue(mainHeroGold), new TextObject("{=Hxf6bzmR}Current Denars", (Dictionary<string, object>)null)));
		int valueAmount = (int)MetaDataExtensions.GetClanInfluence(Save.MetaData);
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.Influence, GetAbbreviatedValueTextFromValue(valueAmount), new TextObject("{=RVPidk5a}Influence", (Dictionary<string, object>)null)));
		if (HasNavalDLC(Save.MetaData))
		{
			int mainPartyShipCount = MetaDataExtensions.GetMainPartyShipCount(Save.MetaData);
			((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.Ships, GetAbbreviatedValueTextFromValue(mainPartyShipCount), new TextObject("{=2zD7LFSj}Ships Owned", (Dictionary<string, object>)null)));
		}
		int num = MetaDataExtensions.GetMainPartyHealthyMemberCount(Save.MetaData) + MetaDataExtensions.GetMainPartyWoundedMemberCount(Save.MetaData);
		int mainPartyPrisonerMemberCount = MetaDataExtensions.GetMainPartyPrisonerMemberCount(Save.MetaData);
		TextObject val2;
		if (mainPartyPrisonerMemberCount > 0)
		{
			val2 = new TextObject("{=6qYaQkDD}{COUNT} + {PRISONER_COUNT}p", (Dictionary<string, object>)null);
			val2.SetTextVariable("COUNT", num);
			val2.SetTextVariable("PRISONER_COUNT", mainPartyPrisonerMemberCount);
		}
		else
		{
			val2 = new TextObject(num, (Dictionary<string, object>)null);
		}
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.PartySize, val2, new TextObject("{=IXwOaa98}Party Size", (Dictionary<string, object>)null)));
		int num2 = (int)MetaDataExtensions.GetMainPartyFood(Save.MetaData);
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.Food, new TextObject(num2, (Dictionary<string, object>)null), new TextObject("{=qSi4DlT4}Food", (Dictionary<string, object>)null)));
		int clanFiefs = MetaDataExtensions.GetClanFiefs(Save.MetaData);
		((Collection<SavedGamePropertyVM>)(object)SavedGameProperties).Add(new SavedGamePropertyVM(SavedGamePropertyVM.SavedGameProperty.Fiefs, new TextObject(clanFiefs, (Dictionary<string, object>)null), new TextObject("{=SRjrhb0A}Owned Fief Count", (Dictionary<string, object>)null)));
		TextObject val3 = new TextObject("{=GZWPHmAw}Day : {DAY}", (Dictionary<string, object>)null);
		string text2 = ((int)MetaDataExtensions.GetDayLong(Save.MetaData)).ToString();
		val3.SetTextVariable("DAY", text2);
		GameTimeText = ((object)val3).ToString();
		TextObject val4 = new TextObject("{=IwhpeT8C}Level : {PLAYER_LEVEL}", (Dictionary<string, object>)null);
		val4.SetTextVariable("PLAYER_LEVEL", MetaDataExtensions.GetMainHeroLevel(Save.MetaData).ToString());
		LevelText = ((object)val4).ToString();
		DateTimeHint = new HintViewModel(new TextObject("{=!}" + RealTimeText1, (Dictionary<string, object>)null), (string)null);
		UpdateButtonHint = new HintViewModel(new TextObject("{=ZDPIq4hi}Load the selected save game, overwrite it with the current version of the game and get back to this screen.", (Dictionary<string, object>)null), (string)null);
		SaveLoadText = (_isSaving ? ((object)new TextObject("{=bV75iwKa}Save", (Dictionary<string, object>)null)).ToString() : ((object)new TextObject("{=9NuttOBC}Load", (Dictionary<string, object>)null)).ToString());
		OverrideSaveText = ((object)new TextObject("{=hYL3CFHX}Do you want to overwrite this saved game?", (Dictionary<string, object>)null)).ToString();
		UpdateSaveText = ((object)new TextObject("{=FFiPLPbs}Update Save", (Dictionary<string, object>)null)).ToString();
		CorruptedSaveText = ((object)new TextObject("{=RoYPofhK}Corrupted Save", (Dictionary<string, object>)null)).ToString();
	}

	private bool HasNavalDLC(MetaData metaData)
	{
		string value = "NavalDLC";
		return MetaDataExtensions.GetModules(metaData).Contains(value);
	}

	public void ExecuteSaveLoad()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		if (IsCorrupted || IsDisabled)
		{
			return;
		}
		if (_isSaving)
		{
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=Q1HIlJxe}Overwrite", (Dictionary<string, object>)null)).ToString(), OverrideSaveText, true, true, ((object)new TextObject("{=aeouhelq}Yes", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=8OkPHu4f}No", (Dictionary<string, object>)null)).ToString(), (Action)OnOverrideSaveAccept, (Action)delegate
			{
				_onCancelLoadSave?.Invoke();
			}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		else
		{
			SandBoxSaveHelper.TryLoadSave(Save, StartGame, _onCancelLoadSave);
		}
	}

	private void StartGame(LoadResult loadResult)
	{
		if (Game.Current != null)
		{
			ScreenManager.PopScreen();
			GameStateManager.Current.CleanStates(0);
			GameStateManager.Current = Module.CurrentModule.GlobalGameStateManager;
		}
		MBSaveLoad.OnStartGame(loadResult);
		MBGameManager.StartNewGame((MBGameManager)(object)new SandBoxGameManager(loadResult));
	}

	private void OnOverrideSaveAccept()
	{
		Campaign.Current.SaveHandler.SaveAs(Save.Name);
		_onDone();
	}

	private static TextObject GetAbbreviatedValueTextFromValue(int valueAmount)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		string text = "";
		decimal num = valueAmount;
		if (valueAmount >= 10000)
		{
			if (valueAmount >= 10000 && valueAmount < 1000000)
			{
				text = ((object)new TextObject("{=thousandabbr}k", (Dictionary<string, object>)null)).ToString();
				num /= 1000m;
			}
			else if (valueAmount >= 1000000 && valueAmount < 1000000000)
			{
				text = ((object)new TextObject("{=millionabbr}m", (Dictionary<string, object>)null)).ToString();
				num /= 1000000m;
			}
			else if (valueAmount >= 1000000000 && valueAmount <= int.MaxValue)
			{
				text = ((object)new TextObject("{=billionabbr}b", (Dictionary<string, object>)null)).ToString();
				num /= 1000000000m;
			}
			int num2 = (int)num;
			string text2 = num2.ToString();
			if (text2.Length < 3)
			{
				text2 += ".";
				string text3 = num.ToString("F3").Split(new char[1] { '.' }).ElementAtOrDefault(1);
				if (text3 != null)
				{
					for (int i = 0; i < 3 - num2.ToString().Length; i++)
					{
						if (text3.ElementAtOrDefault(i) != 0)
						{
							text2 += text3.ElementAtOrDefault(i);
						}
					}
				}
			}
			TextObject val = new TextObject("{=mapbardenarvalue}{DENAR_AMOUNT}{VALUE_ABBREVIATION}", (Dictionary<string, object>)null);
			val.SetTextVariable("DENAR_AMOUNT", text2);
			val.SetTextVariable("VALUE_ABBREVIATION", text);
			return val;
		}
		return new TextObject(valueAmount, (Dictionary<string, object>)null);
	}

	public void ExecuteUpdate()
	{
	}

	public void ExecuteDelete()
	{
		_onDelete(this);
	}

	public void ExecuteSelection()
	{
		_onSelection(this);
	}
}
