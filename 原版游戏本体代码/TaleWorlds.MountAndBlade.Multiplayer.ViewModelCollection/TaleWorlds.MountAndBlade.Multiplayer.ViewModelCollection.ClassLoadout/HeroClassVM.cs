using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class HeroClassVM : ViewModel
{
	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	public readonly MPHeroClass HeroClass;

	private readonly Action<HeroClassVM> _onSelect;

	private Action<HeroPerkVM, MPPerkVM> _onPerkSelect;

	private bool _isSelected;

	private string _name;

	private string _iconType;

	private int _gold;

	private int _numOfTroops;

	private bool _isEnabled;

	private bool _isGoldEnabled;

	private bool _isNumOfTroopsEnabled;

	private string _cultureId;

	private string _troopTypeId;

	private Color _cultureColor;

	private MBBindingList<HeroPerkVM> _perks;

	public List<IReadOnlyPerkObject> SelectedPerks { get; private set; }

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
	public MBBindingList<HeroPerkVM> Perks
	{
		get
		{
			return _perks;
		}
		set
		{
			if (value != _perks)
			{
				_perks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<HeroPerkVM>>(value, "Perks");
			}
		}
	}

	[DataSourceProperty]
	public string CultureId
	{
		get
		{
			return _cultureId;
		}
		set
		{
			if (value != _cultureId)
			{
				_cultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureId");
			}
		}
	}

	[DataSourceProperty]
	public string TroopTypeId
	{
		get
		{
			return _troopTypeId;
		}
		set
		{
			if (value != _troopTypeId)
			{
				_troopTypeId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TroopTypeId");
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
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string IconType
	{
		get
		{
			return _iconType;
		}
		set
		{
			if (value != _iconType)
			{
				_iconType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IconType");
			}
		}
	}

	[DataSourceProperty]
	public int Gold
	{
		get
		{
			return _gold;
		}
		set
		{
			if (value != _gold)
			{
				_gold = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Gold");
			}
		}
	}

	[DataSourceProperty]
	public int NumOfTroops
	{
		get
		{
			return _numOfTroops;
		}
		set
		{
			if (value != _numOfTroops)
			{
				_numOfTroops = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NumOfTroops");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGoldEnabled
	{
		get
		{
			return _isGoldEnabled;
		}
		set
		{
			if (value != _isGoldEnabled)
			{
				_isGoldEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsGoldEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNumOfTroopsEnabled
	{
		get
		{
			return _isNumOfTroopsEnabled;
		}
		set
		{
			if (value != _isNumOfTroopsEnabled)
			{
				_isNumOfTroopsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNumOfTroopsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor)
			{
				_cultureColor = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor");
			}
		}
	}

	[DataSourceProperty]
	public HeroPerkVM FirstPerk => ((IEnumerable<HeroPerkVM>)Perks).ElementAtOrDefault(0);

	[DataSourceProperty]
	public HeroPerkVM SecondPerk => ((IEnumerable<HeroPerkVM>)Perks).ElementAtOrDefault(1);

	[DataSourceProperty]
	public HeroPerkVM ThirdPerk => ((IEnumerable<HeroPerkVM>)Perks).ElementAtOrDefault(2);

	public HeroClassVM(Action<HeroClassVM> onSelect, Action<HeroPerkVM, MPPerkVM> onPerkSelect, MPHeroClass heroClass, MultiplayerCultureColorInfo colorInfo)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between Unknown and I4
		HeroClass = heroClass;
		_onSelect = onSelect;
		_onPerkSelect = onPerkSelect;
		CultureId = ((MBObjectBase)heroClass.Culture).StringId;
		IconType = ((object)heroClass.IconType/*cast due to .constrained prefix*/).ToString();
		TroopTypeId = heroClass.ClassGroup.StringId;
		CultureColor = colorInfo.Color1;
		_gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		Gold = (_gameMode.IsGameModeUsingCasualGold ? HeroClass.TroopCasualCost : (((int)_gameMode.GameType == 3) ? HeroClass.TroopBattleCost : HeroClass.TroopCost));
		InitPerksList();
		int intValue = MultiplayerOptionsExtensions.GetIntValue((OptionType)20, (MultiplayerOptionsAccessMode)1);
		IsNumOfTroopsEnabled = !_gameMode.IsInWarmup && intValue > 0;
		if (IsNumOfTroopsEnabled)
		{
			NumOfTroops = MPPerkObject.GetTroopCount(heroClass, intValue, MPPerkObject.GetOnSpawnPerkHandler(((IEnumerable<HeroPerkVM>)_perks).Select((HeroPerkVM p) => p.SelectedPerk)));
		}
		UpdateEnabled();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)HeroClass.HeroName).ToString();
		Perks.ApplyActionOnAllItems((Action<HeroPerkVM>)delegate(HeroPerkVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	private void InitPerksList()
	{
		List<List<IReadOnlyPerkObject>> allPerksForHeroClass = MultiplayerClassDivisions.GetAllPerksForHeroClass(HeroClass, (string)null);
		if (SelectedPerks == null)
		{
			SelectedPerks = new List<IReadOnlyPerkObject>();
		}
		else
		{
			SelectedPerks.Clear();
		}
		for (int i = 0; i < allPerksForHeroClass.Count; i++)
		{
			if (allPerksForHeroClass[i].Count > 0)
			{
				SelectedPerks.Add(allPerksForHeroClass[i][0]);
			}
			else
			{
				SelectedPerks.Add(null);
			}
		}
		if (GameNetwork.IsMyPeerReady)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			int num = (component.NextSelectedTroopIndex = MultiplayerClassDivisions.GetMPHeroClasses(HeroClass.Culture).ToList().IndexOf(HeroClass));
			for (int j = 0; j < allPerksForHeroClass.Count; j++)
			{
				if (allPerksForHeroClass[j].Count > 0)
				{
					int num3 = component.GetSelectedPerkIndexWithPerkListIndex(num, j);
					if (num3 >= allPerksForHeroClass[j].Count)
					{
						num3 = 0;
					}
					IReadOnlyPerkObject value = allPerksForHeroClass[j][num3];
					SelectedPerks[j] = value;
				}
			}
		}
		MBBindingList<HeroPerkVM> val = new MBBindingList<HeroPerkVM>();
		for (int k = 0; k < allPerksForHeroClass.Count; k++)
		{
			if (allPerksForHeroClass[k].Count > 0)
			{
				((Collection<HeroPerkVM>)(object)val).Add(new HeroPerkVM(_onPerkSelect, SelectedPerks[k], allPerksForHeroClass[k], k));
			}
		}
		Perks = val;
	}

	public void UpdateEnabled()
	{
		IsEnabled = _gameMode.IsClassAvailable(HeroClass) && (_gameMode.IsInWarmup || !_gameMode.IsGameModeUsingGold || _gameMode.GetGoldAmount() >= Gold);
	}

	[UsedImplicitly]
	public void OnSelect()
	{
		_onSelect(this);
	}
}
