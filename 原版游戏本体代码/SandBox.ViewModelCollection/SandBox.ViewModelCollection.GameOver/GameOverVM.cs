using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SandBox.ViewModelCollection.Input;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.GameOver;

public class GameOverVM : ViewModel
{
	private readonly Action _onClose;

	private readonly GameOverStatsProvider _statsProvider;

	private readonly GameOverReason _reason;

	private GameOverStatCategoryVM _currentCategory;

	private string _closeText;

	private string _titleText;

	private string _reasonAsString;

	private string _statisticsTitle;

	private bool _isPositiveGameOver;

	private InputKeyItemVM _closeInputKey;

	private BannerImageIdentifierVM _clanBanner;

	private MBBindingList<GameOverStatCategoryVM> _categories;

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
	public string StatisticsTitle
	{
		get
		{
			return _statisticsTitle;
		}
		set
		{
			if (value != _statisticsTitle)
			{
				_statisticsTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "StatisticsTitle");
			}
		}
	}

	[DataSourceProperty]
	public string ReasonAsString
	{
		get
		{
			return _reasonAsString;
		}
		set
		{
			if (value != _reasonAsString)
			{
				_reasonAsString = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ReasonAsString");
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
	public bool IsPositiveGameOver
	{
		get
		{
			return _isPositiveGameOver;
		}
		set
		{
			if (value != _isPositiveGameOver)
			{
				_isPositiveGameOver = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPositiveGameOver");
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

	[DataSourceProperty]
	public MBBindingList<GameOverStatCategoryVM> Categories
	{
		get
		{
			return _categories;
		}
		set
		{
			if (value != _categories)
			{
				_categories = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<GameOverStatCategoryVM>>(value, "Categories");
			}
		}
	}

	public GameOverVM(GameOverReason reason, Action onClose)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_onClose = onClose;
		_reason = reason;
		_statsProvider = new GameOverStatsProvider();
		Categories = new MBBindingList<GameOverStatCategoryVM>();
		IsPositiveGameOver = (int)_reason == 2;
		ClanBanner = new BannerImageIdentifierVM(Hero.MainHero.ClanBanner, true);
		ReasonAsString = Enum.GetName(typeof(GameOverReason), _reason);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CloseText = (IsPositiveGameOver ? ((object)new TextObject("{=AdgAJbAP}Return To The Map", (Dictionary<string, object>)null)).ToString() : ((object)GameTexts.FindText("str_main_menu", (string)null)).ToString());
		TitleText = ((object)GameTexts.FindText("str_game_over_title", ReasonAsString)).ToString();
		StatisticsTitle = ((object)GameTexts.FindText("str_statistics", (string)null)).ToString();
		((Collection<GameOverStatCategoryVM>)(object)Categories).Clear();
		foreach (StatCategory gameOverStat in _statsProvider.GetGameOverStats())
		{
			((Collection<GameOverStatCategoryVM>)(object)Categories).Add(new GameOverStatCategoryVM(gameOverStat, OnCategorySelection));
		}
		OnCategorySelection(((Collection<GameOverStatCategoryVM>)(object)Categories)[0]);
	}

	private void OnCategorySelection(GameOverStatCategoryVM newCategory)
	{
		if (_currentCategory != null)
		{
			_currentCategory.IsSelected = false;
		}
		_currentCategory = newCategory;
		if (_currentCategory != null)
		{
			_currentCategory.IsSelected = true;
		}
	}

	public void ExecuteClose()
	{
		Action onClose = _onClose;
		if (onClose != null)
		{
			Common.DynamicInvokeWithLog((Delegate)onClose, Array.Empty<object>());
		}
	}

	public void SetCloseInputKey(HotKey hotKey)
	{
		CloseInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, isConsoleOnly: true);
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
}
