using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;

public class MPArmoryClassStatsVM : ViewModel
{
	private readonly List<IReadOnlyPerkObject> _dummyPerkList;

	private string _factionDescription;

	private string _factionName;

	private string _flavorText;

	private int _cost;

	private HintViewModel _costHint;

	private HeroInformationVM _heroInformation;

	[DataSourceProperty]
	public string FactionDescription
	{
		get
		{
			return _factionDescription;
		}
		set
		{
			if (value != _factionDescription)
			{
				_factionDescription = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionDescription");
			}
		}
	}

	[DataSourceProperty]
	public string FactionName
	{
		get
		{
			return _factionName;
		}
		set
		{
			if (value != _factionName)
			{
				_factionName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionName");
			}
		}
	}

	[DataSourceProperty]
	public string FlavorText
	{
		get
		{
			return _flavorText;
		}
		set
		{
			if (value != _flavorText)
			{
				_flavorText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FlavorText");
			}
		}
	}

	[DataSourceProperty]
	public int Cost
	{
		get
		{
			return _cost;
		}
		set
		{
			if (value != _cost)
			{
				_cost = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Cost");
			}
		}
	}

	[DataSourceProperty]
	public HintViewModel CostHint
	{
		get
		{
			return _costHint;
		}
		set
		{
			if (value != _costHint)
			{
				_costHint = value;
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "CostHint");
			}
		}
	}

	[DataSourceProperty]
	public HeroInformationVM HeroInformation
	{
		get
		{
			return _heroInformation;
		}
		set
		{
			if (value != _heroInformation)
			{
				_heroInformation = value;
				((ViewModel)this).OnPropertyChangedWithValue<HeroInformationVM>(value, "HeroInformation");
			}
		}
	}

	public MPArmoryClassStatsVM()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		_dummyPerkList = new List<IReadOnlyPerkObject>();
		FactionDescription = ((object)new TextObject("{=5Pea977J}Faction: ", (Dictionary<string, object>)null)).ToString();
		HeroInformation = new HeroInformationVM();
	}

	public override void RefreshValues()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CostHint = new HintViewModel(GameTexts.FindText("str_armory_troop_cost", (string)null), (string)null);
		((ViewModel)HeroInformation).RefreshValues();
	}

	public void RefreshWith(MPHeroClass heroClass)
	{
		FactionName = ((object)heroClass.Culture.Name).ToString();
		FlavorText = ((object)GameTexts.FindText("str_troop_description", ((MBObjectBase)heroClass).StringId)).ToString();
		HeroInformation.RefreshWith(heroClass, _dummyPerkList);
		Cost = heroClass.TroopCost;
	}
}
