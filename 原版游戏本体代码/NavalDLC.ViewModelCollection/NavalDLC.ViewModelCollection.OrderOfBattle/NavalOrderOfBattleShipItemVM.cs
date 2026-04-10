using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.ViewModelCollection.OrderOfBattle;

public class NavalOrderOfBattleShipItemVM : ViewModel
{
	public readonly IShipOrigin ShipOrigin;

	public MissionShip MissionShip;

	private readonly Action<NavalOrderOfBattleShipItemVM, bool> _onSelected;

	private readonly Func<NavalOrderOfBattleShipItemVM, NavalOrderOfBattleFormationItemVM> _findFormationOfShip;

	private List<TooltipProperty> _cachedTooltipProperties;

	private bool _isDisabled;

	private bool _isSelected;

	private bool _isFlagship;

	private string _prefabId;

	private string _shipName;

	private float _healthRatio;

	private string _healthPercentageAsString;

	private int _mainDeckCrewCount;

	private int _reserveCrewCount;

	private int _mainDeckCrewCapacity;

	private string _crewCountAsString;

	private string _reserveCrewCountAsString;

	private float _mainDeckCrewRatio;

	private float _totalCrewRatio;

	private BasicTooltipViewModel _tooltip;

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
	public bool IsFlagship
	{
		get
		{
			return _isFlagship;
		}
		set
		{
			if (value != _isFlagship)
			{
				_isFlagship = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFlagship");
			}
		}
	}

	[DataSourceProperty]
	public string PrefabId
	{
		get
		{
			return _prefabId;
		}
		set
		{
			if (value != _prefabId)
			{
				_prefabId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PrefabId");
			}
		}
	}

	[DataSourceProperty]
	public string ShipName
	{
		get
		{
			return _shipName;
		}
		set
		{
			if (value != _shipName)
			{
				_shipName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ShipName");
			}
		}
	}

	[DataSourceProperty]
	public float HealthRatio
	{
		get
		{
			return _healthRatio;
		}
		set
		{
			if (value != _healthRatio)
			{
				_healthRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HealthRatio");
			}
		}
	}

	[DataSourceProperty]
	public string HealthPercentageAsString
	{
		get
		{
			return _healthPercentageAsString;
		}
		set
		{
			if (value != _healthPercentageAsString)
			{
				_healthPercentageAsString = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "HealthPercentageAsString");
			}
		}
	}

	[DataSourceProperty]
	public int MainDeckCrewCount
	{
		get
		{
			return _mainDeckCrewCount;
		}
		set
		{
			if (value != _mainDeckCrewCount)
			{
				_mainDeckCrewCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MainDeckCrewCount");
			}
		}
	}

	[DataSourceProperty]
	public int ReserveCrewCount
	{
		get
		{
			return _reserveCrewCount;
		}
		set
		{
			if (value != _reserveCrewCount)
			{
				_reserveCrewCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ReserveCrewCount");
			}
		}
	}

	[DataSourceProperty]
	public int MainDeckCrewCapacity
	{
		get
		{
			return _mainDeckCrewCapacity;
		}
		set
		{
			if (value != _mainDeckCrewCapacity)
			{
				_mainDeckCrewCapacity = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MainDeckCrewCapacity");
			}
		}
	}

	[DataSourceProperty]
	public string CrewCountAsString
	{
		get
		{
			return _crewCountAsString;
		}
		set
		{
			if (value != _crewCountAsString)
			{
				_crewCountAsString = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CrewCountAsString");
			}
		}
	}

	[DataSourceProperty]
	public string ReserveCrewCountAsString
	{
		get
		{
			return _reserveCrewCountAsString;
		}
		set
		{
			if (value != _reserveCrewCountAsString)
			{
				_reserveCrewCountAsString = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ReserveCrewCountAsString");
			}
		}
	}

	[DataSourceProperty]
	public float MainDeckCrewRatio
	{
		get
		{
			return _mainDeckCrewRatio;
		}
		set
		{
			if (value != _mainDeckCrewRatio)
			{
				_mainDeckCrewRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MainDeckCrewRatio");
			}
		}
	}

	[DataSourceProperty]
	public float TotalCrewRatio
	{
		get
		{
			return _totalCrewRatio;
		}
		set
		{
			if (value != _totalCrewRatio)
			{
				_totalCrewRatio = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TotalCrewRatio");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel Tooltip
	{
		get
		{
			return _tooltip;
		}
		set
		{
			if (value != _tooltip)
			{
				_tooltip = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "Tooltip");
			}
		}
	}

	public NavalOrderOfBattleShipItemVM(IShipOrigin shipOrigin, Action<NavalOrderOfBattleShipItemVM, bool> onSelected, Func<NavalOrderOfBattleShipItemVM, NavalOrderOfBattleFormationItemVM> findFormationOfShip)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		_onSelected = onSelected;
		_findFormationOfShip = findFormationOfShip;
		ShipOrigin = shipOrigin;
		PrefabId = NavalUIHelper.GetPrefabIdOfShipHull(shipOrigin.Hull);
		IShipOrigin shipOrigin2 = ShipOrigin;
		Ship val;
		IsFlagship = (val = (Ship)(object)((shipOrigin2 is Ship) ? shipOrigin2 : null)) != null && val == NavalUIHelper.GetFlagship(val.Owner);
		Tooltip = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => _cachedTooltipProperties));
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		ShipName = ((object)ShipOrigin.Name).ToString();
		if (MissionShip != null)
		{
			HealthRatio = MissionShip.HitPoints / MissionShip.MaxHealth;
			MainDeckCrewCount = MissionShip.Formation.CountOfUnits;
			ReserveCrewCount = Mission.Current.GetMissionBehavior<NavalAgentsLogic>().GetReservedTroopsCountOfShip(MissionShip);
			MainDeckCrewCapacity = MissionShip.CrewSizeOnMainDeck;
			MainDeckCrewRatio = (float)MainDeckCrewCount / (float)(MainDeckCrewCapacity + ReserveCrewCount);
			TotalCrewRatio = (float)(MainDeckCrewCount + ReserveCrewCount) / (float)(MainDeckCrewCapacity + ReserveCrewCount);
		}
		else
		{
			HealthRatio = ShipOrigin.HitPoints / ShipOrigin.MaxHitPoints;
			MainDeckCrewCount = 0;
			ReserveCrewCount = 0;
			MainDeckCrewCapacity = ShipOrigin.MainDeckCrewCapacity;
			MainDeckCrewRatio = 0f;
			TotalCrewRatio = 0f;
		}
		HealthPercentageAsString = ((object)new TextObject("{=gYATKZJp}{NUMBER}%", (Dictionary<string, object>)null).SetTextVariable("NUMBER", ((int)(HealthRatio * 100f)).ToString())).ToString();
		CrewCountAsString = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", MainDeckCrewCount).SetTextVariable("RIGHT", MainDeckCrewCapacity)).ToString();
		if (ReserveCrewCount > 0)
		{
			string text = ((object)GameTexts.FindText("str_plus_with_number", (string)null).SetTextVariable("NUMBER", ReserveCrewCount)).ToString();
			ReserveCrewCountAsString = ((object)GameTexts.FindText("str_STR_in_parentheses", (string)null).SetTextVariable("STR", text)).ToString();
		}
		else
		{
			ReserveCrewCountAsString = string.Empty;
		}
		_cachedTooltipProperties = GetTooltip();
	}

	public void ExecuteSelect()
	{
		if (!IsDisabled)
		{
			_onSelected?.Invoke(this, arg2: true);
		}
	}

	public void ExecuteToggleSelect()
	{
		if (!IsDisabled)
		{
			_onSelected?.Invoke(this, !IsSelected);
		}
	}

	public void ExecuteDeselect()
	{
		if (!IsDisabled)
		{
			_onSelected?.Invoke(this, arg2: false);
		}
	}

	private List<TooltipProperty> GetTooltip()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Expected O, but got Unknown
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Expected O, but got Unknown
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Expected O, but got Unknown
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Expected O, but got Unknown
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Expected O, but got Unknown
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Expected O, but got Unknown
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Expected O, but got Unknown
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Expected O, but got Unknown
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Expected O, but got Unknown
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Expected O, but got Unknown
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>
		{
			new TooltipProperty(ShipName, string.Empty, 0, false, (TooltipPropertyFlags)4096)
		};
		if (IsDisabled)
		{
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=cIpPMkry}You can only change your formation's ship when you are not the general.", (Dictionary<string, object>)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0));
		}
		IShipOrigin shipOrigin = ShipOrigin;
		Ship val;
		if ((val = (Ship)(object)((shipOrigin is Ship) ? shipOrigin : null)) != null)
		{
			list.Add(new TooltipProperty(((object)GameTexts.FindText("str_owner", (string)null)).ToString(), ((object)val.Owner.Name).ToString(), 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(((object)new TextObject("{=wEmx6fZi}Hull", (Dictionary<string, object>)null)).ToString(), ((object)val.ShipHull.Name).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		list.Add(new TooltipProperty(((object)new TextObject("{=sqdzHOPe}Class", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("str_ship_type", ((object)ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString().ToLowerInvariant())).ToString(), 0, false, (TooltipPropertyFlags)0));
		if (MissionShip == null)
		{
			string text = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", (int)ShipOrigin.HitPoints).SetTextVariable("RIGHT", (int)ShipOrigin.MaxHitPoints)).ToString();
			list.Add(new TooltipProperty(((object)new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null)).ToString(), text, 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(((object)new TextObject("{=TrbfOCyF}Main Deck Crew Capacity", (Dictionary<string, object>)null)).ToString(), ShipOrigin.MainDeckCrewCapacity.ToString(), 0, false, (TooltipPropertyFlags)0));
			int num = ShipOrigin.TotalCrewCapacity - ShipOrigin.MainDeckCrewCapacity;
			if (num > 0)
			{
				list.Add(new TooltipProperty(((object)new TextObject("{=saS6Sub2}Reserve Crew Capacity", (Dictionary<string, object>)null)).ToString(), num.ToString(), 0, false, (TooltipPropertyFlags)0));
			}
		}
		else
		{
			string text2 = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", (int)MissionShip.HitPoints).SetTextVariable("RIGHT", (int)MissionShip.MaxHealth)).ToString();
			list.Add(new TooltipProperty(((object)new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null)).ToString(), text2, 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(((object)new TextObject("{=LfOIa8eh}Troops On Deck", (Dictionary<string, object>)null)).ToString(), CrewCountAsString, 0, false, (TooltipPropertyFlags)0));
			if (ReserveCrewCount > 0)
			{
				string text3 = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", ReserveCrewCount).SetTextVariable("RIGHT", MissionShip.CrewSizeOnLowerDeck)).ToString();
				list.Add(new TooltipProperty(((object)new TextObject("{=25fleLuY}Troops In Reserve", (Dictionary<string, object>)null)).ToString(), text3, 0, false, (TooltipPropertyFlags)0));
			}
		}
		List<ShipSlotAndPieceName> shipSlotAndPieceNames = ShipOrigin.GetShipSlotAndPieceNames();
		if (shipSlotAndPieceNames.Count > 0)
		{
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024)
			{
				OnlyShowWhenExtended = true
			});
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=zMvUzdKR}Ship Upgrades", (Dictionary<string, object>)null)).ToString(), -1, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenExtended = true
			});
			foreach (ShipSlotAndPieceName item in shipSlotAndPieceNames)
			{
				list.Add(new TooltipProperty(item.SlotName, item.PieceName, 0, false, (TooltipPropertyFlags)0)
				{
					OnlyShowWhenExtended = true
				});
			}
		}
		if (shipSlotAndPieceNames.Count > 0)
		{
			if (Input.IsGamepadActive)
			{
				GameTexts.SetVariable("EXTEND_KEY", ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", "MapFollowModifier")).ToString());
			}
			else
			{
				GameTexts.SetVariable("EXTEND_KEY", ((object)Game.Current.GameTextManager.FindText("str_game_key_text", "anyalt")).ToString());
			}
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
			list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_map_tooltip_info", (string)null)).ToString(), -1, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
		}
		return list;
	}

	public bool GetCanBeUnassignedOrMoved()
	{
		if (!IsDisabled)
		{
			Func<NavalOrderOfBattleShipItemVM, NavalOrderOfBattleFormationItemVM> findFormationOfShip = _findFormationOfShip;
			if (findFormationOfShip == null)
			{
				return true;
			}
			return findFormationOfShip(this)?.Captain?.IsMainHero != true;
		}
		return false;
	}
}
