using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.TeamSelection;

public class MultiplayerCultureSelectVM : ViewModel
{
	private BasicCultureObject _firstCulture;

	private BasicCultureObject _secondCulture;

	private Action<BasicCultureObject> _onCultureSelected;

	private Action _onClose;

	private string _gameModeText;

	private string _cultureSelectionText;

	private string _firstCultureName;

	private string _secondCultureName;

	private Color _firstCultureColor1;

	private Color _firstCultureColor2;

	private Color _secondCultureColor1;

	private Color _secondCultureColor2;

	private string _firstCultureCode;

	private string _secondCultureCode;

	[DataSourceProperty]
	public string GameModeText
	{
		get
		{
			return _gameModeText;
		}
		set
		{
			if (value != _gameModeText)
			{
				_gameModeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameModeText");
			}
		}
	}

	[DataSourceProperty]
	public string CultureSelectionText
	{
		get
		{
			return _cultureSelectionText;
		}
		set
		{
			if (value != _cultureSelectionText)
			{
				_cultureSelectionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureSelectionText");
			}
		}
	}

	[DataSourceProperty]
	public string FirstCultureName
	{
		get
		{
			return _firstCultureName;
		}
		set
		{
			if (value != _firstCultureName)
			{
				_firstCultureName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FirstCultureName");
			}
		}
	}

	[DataSourceProperty]
	public string SecondCultureName
	{
		get
		{
			return _secondCultureName;
		}
		set
		{
			if (value != _secondCultureName)
			{
				_secondCultureName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SecondCultureName");
			}
		}
	}

	[DataSourceProperty]
	public string FirstCultureCode
	{
		get
		{
			return _firstCultureCode;
		}
		set
		{
			if (value != _firstCultureCode)
			{
				_firstCultureCode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FirstCultureCode");
			}
		}
	}

	[DataSourceProperty]
	public string SecondCultureCode
	{
		get
		{
			return _secondCultureCode;
		}
		set
		{
			if (value != _secondCultureCode)
			{
				_secondCultureCode = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SecondCultureCode");
			}
		}
	}

	[DataSourceProperty]
	public Color FirstCultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _firstCultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _firstCultureColor1)
			{
				_firstCultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FirstCultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color FirstCultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _firstCultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _firstCultureColor2)
			{
				_firstCultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "FirstCultureColor2");
			}
		}
	}

	[DataSourceProperty]
	public Color SecondCultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _secondCultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _secondCultureColor1)
			{
				_secondCultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SecondCultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color SecondCultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _secondCultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _secondCultureColor2)
			{
				_secondCultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SecondCultureColor2");
			}
		}
	}

	public MultiplayerCultureSelectVM(Action<BasicCultureObject> onCultureSelected, Action onClose)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		_onCultureSelected = onCultureSelected;
		_onClose = onClose;
		_firstCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1));
		_secondCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		FirstCultureCode = ((MBObjectBase)_firstCulture).StringId;
		SecondCultureCode = ((MBObjectBase)_secondCulture).StringId;
		MultiplayerBattleColors val = MultiplayerBattleColors.CreateWith(_firstCulture, _secondCulture);
		FirstCultureColor1 = val.AttackerColors.Color1;
		FirstCultureColor2 = val.AttackerColors.Color2;
		SecondCultureColor1 = val.DefenderColors.Color1;
		SecondCultureColor2 = val.DefenderColors.Color2;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		GameModeText = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1))).ToString();
		CultureSelectionText = ((object)new TextObject("{=yQ0p8Glo}Select Culture", (Dictionary<string, object>)null)).ToString();
		FirstCultureName = ((object)_firstCulture.Name).ToString();
		SecondCultureName = ((object)_secondCulture.Name).ToString();
	}

	public void ExecuteSelectCulture(int cultureIndex)
	{
		switch (cultureIndex)
		{
		case 0:
			_onCultureSelected?.Invoke(_firstCulture);
			break;
		case 1:
			_onCultureSelected?.Invoke(_secondCulture);
			break;
		default:
			Debug.FailedAssert("Invalid Culture Index!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\TeamSelection\\MultiplayerCultureSelectVM.cs", "ExecuteSelectCulture", 65);
			break;
		}
	}

	public void ExecuteClose()
	{
		_onClose?.Invoke();
	}
}
