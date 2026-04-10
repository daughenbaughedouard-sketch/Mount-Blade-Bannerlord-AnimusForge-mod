using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame.HostGameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.HostGame;

public class MPHostGameOptionsVM : ViewModel
{
	private class OptionPreferredIndexComparer : IComparer<GenericHostGameOptionDataVM>
	{
		public int Compare(GenericHostGameOptionDataVM x, GenericHostGameOptionDataVM y)
		{
			return x.PreferredIndex.CompareTo(y.PreferredIndex);
		}
	}

	private List<GenericHostGameOptionDataVM> _hostGameItemsForNextTick = new List<GenericHostGameOptionDataVM>();

	private OptionPreferredIndexComparer _optionComparer;

	private MPCustomGameVM.CustomGameMode _customGameMode;

	private bool _isRefreshed;

	private bool _isInMission;

	private MBBindingList<GenericHostGameOptionDataVM> _generalOptions;

	[DataSourceProperty]
	public MBBindingList<GenericHostGameOptionDataVM> GeneralOptions
	{
		get
		{
			return _generalOptions;
		}
		set
		{
			if (value != _generalOptions)
			{
				_generalOptions = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<GenericHostGameOptionDataVM>>(value, "GeneralOptions");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRefreshed
	{
		get
		{
			return _isRefreshed;
		}
		set
		{
			if (value != _isRefreshed)
			{
				_isRefreshed = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRefreshed");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInMission
	{
		get
		{
			return _isInMission;
		}
		set
		{
			if (value != _isInMission)
			{
				_isInMission = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInMission");
			}
		}
	}

	public MPHostGameOptionsVM(bool isInMission, MPCustomGameVM.CustomGameMode customGameMode = MPCustomGameVM.CustomGameMode.CustomServer)
	{
		IsInMission = isInMission;
		GeneralOptions = new MBBindingList<GenericHostGameOptionDataVM>();
		_optionComparer = new OptionPreferredIndexComparer();
		_customGameMode = customGameMode;
		InitializeDefaultOptionList();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		GeneralOptions.ApplyActionOnAllItems((Action<GenericHostGameOptionDataVM>)delegate(GenericHostGameOptionDataVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	private void InitializeDefaultOptionList()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		IsRefreshed = false;
		string text = "";
		if (_customGameMode == MPCustomGameVM.CustomGameMode.CustomServer)
		{
			text = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
		}
		else
		{
			text = "Skirmish";
			MultiplayerOptions.Instance.SetValueForOptionWithMultipleSelectionFromText((OptionType)10, text);
		}
		OnGameModeChanged(text);
		foreach (GenericHostGameOptionDataVM item in ((IEnumerable<GenericHostGameOptionDataVM>)GeneralOptions).ToList())
		{
			if (((int)item.OptionType == 11 || (int)item.OptionType == 10) && item is MultipleSelectionHostGameOptionDataVM)
			{
				(item as MultipleSelectionHostGameOptionDataVM).OnChangedSelection = OnChangeSelected;
			}
		}
		IsRefreshed = true;
	}

	private void OnChangeSelected(MultipleSelectionHostGameOptionDataVM option)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		IsRefreshed = false;
		if ((int)option.OptionType == 11 || (int)option.OptionType == 10)
		{
			OnGameModeChanged(MultiplayerOptions.Instance.GetMultiplayerOptionsList((OptionType)11)[option.Selector.SelectedIndex]);
		}
		IsRefreshed = true;
	}

	private void OnGameModeChanged(string gameModeName)
	{
		_hostGameItemsForNextTick.Clear();
		if (_customGameMode == MPCustomGameVM.CustomGameMode.CustomServer)
		{
			FillOptionsForCustomServer(gameModeName);
		}
		else if (_customGameMode == MPCustomGameVM.CustomGameMode.PremadeGame)
		{
			FillOptionsForPremadeGame();
		}
		(((IEnumerable<GenericHostGameOptionDataVM>)GeneralOptions).First((GenericHostGameOptionDataVM o) => (int)o.OptionType == 13) as MultipleSelectionHostGameOptionDataVM)?.RefreshList();
		GeneralOptions.Sort((IComparer<GenericHostGameOptionDataVM>)_optionComparer);
	}

	private void FillOptionsForCustomServer(string gameModeName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		for (OptionType val = (OptionType)0; (int)val < 43; val = (OptionType)(val + 1))
		{
			MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(val);
			if ((int)val == 10 || (int)val == 12 || optionProperty == null)
			{
				continue;
			}
			int preferredIndex = (int)val;
			bool flag = optionProperty.ValidGameModes == null;
			if (optionProperty.ValidGameModes != null && optionProperty.ValidGameModes.Contains(gameModeName))
			{
				flag = true;
			}
			GenericHostGameOptionDataVM genericHostGameOptionDataVM = ((IEnumerable<GenericHostGameOptionDataVM>)GeneralOptions).FirstOrDefault((GenericHostGameOptionDataVM o) => o.PreferredIndex == preferredIndex);
			if (flag)
			{
				if (genericHostGameOptionDataVM == null)
				{
					GenericHostGameOptionDataVM item = CreateOption(val, preferredIndex);
					((Collection<GenericHostGameOptionDataVM>)(object)GeneralOptions).Add(item);
				}
				else
				{
					genericHostGameOptionDataVM.RefreshData();
				}
			}
			else if (genericHostGameOptionDataVM != null)
			{
				((Collection<GenericHostGameOptionDataVM>)(object)GeneralOptions).Remove(genericHostGameOptionDataVM);
			}
		}
	}

	private void FillOptionsForPremadeGame()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected I4, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		for (OptionType val = (OptionType)0; (int)val < 43; val = (OptionType)(val + 1))
		{
			MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(val);
			bool flag = false;
			if ((int)val == 0 || (int)val == 2 || (int)val == 14 || (int)val == 15 || (int)val == 13 || (int)val == 10 || (int)val == 12)
			{
				flag = true;
			}
			if (flag && optionProperty != null)
			{
				int preferredIndex = (int)val;
				GenericHostGameOptionDataVM genericHostGameOptionDataVM = ((IEnumerable<GenericHostGameOptionDataVM>)GeneralOptions).FirstOrDefault((GenericHostGameOptionDataVM o) => o.PreferredIndex == preferredIndex);
				if (genericHostGameOptionDataVM == null)
				{
					GenericHostGameOptionDataVM item = CreateOption(val, preferredIndex);
					((Collection<GenericHostGameOptionDataVM>)(object)GeneralOptions).Add(item);
				}
				else
				{
					genericHostGameOptionDataVM.RefreshData();
				}
			}
		}
	}

	private GenericHostGameOptionDataVM CreateOption(OptionType type, int preferredIndex)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		GenericHostGameOptionDataVM genericHostGameOptionDataVM = null;
		OptionsDataType specificHostGameOptionTypeOf = GetSpecificHostGameOptionTypeOf(type);
		switch ((int)specificHostGameOptionTypeOf)
		{
		case 0:
			genericHostGameOptionDataVM = new BooleanHostGameOptionDataVM(type, preferredIndex);
			break;
		case 1:
			genericHostGameOptionDataVM = new NumericHostGameOptionDataVM(type, preferredIndex);
			break;
		case 4:
			genericHostGameOptionDataVM = new InputHostGameOptionDataVM(type, preferredIndex);
			break;
		case 3:
			genericHostGameOptionDataVM = new MultipleSelectionHostGameOptionDataVM(type, preferredIndex);
			break;
		}
		if (genericHostGameOptionDataVM == null)
		{
			Debug.FailedAssert("Item was not added to host game options because it has an invalid type.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\HostGame\\MPHostGameOptionsVM.cs", "CreateOption", 218);
			return null;
		}
		return genericHostGameOptionDataVM;
	}

	private OptionsDataType GetSpecificHostGameOptionTypeOf(OptionType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(type);
		OptionValueType optionValueType = optionProperty.OptionValueType;
		switch ((int)optionValueType)
		{
		case 0:
			return (OptionsDataType)0;
		case 1:
			return (OptionsDataType)1;
		case 2:
			return (OptionsDataType)3;
		case 3:
			if (optionProperty.HasMultipleSelections)
			{
				return (OptionsDataType)3;
			}
			return (OptionsDataType)4;
		default:
			return (OptionsDataType)(-1);
		}
	}
}
