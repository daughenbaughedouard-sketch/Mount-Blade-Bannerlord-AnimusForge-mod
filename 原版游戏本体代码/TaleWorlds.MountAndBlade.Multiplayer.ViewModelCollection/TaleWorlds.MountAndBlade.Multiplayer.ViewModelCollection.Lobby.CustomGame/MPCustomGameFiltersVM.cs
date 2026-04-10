using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Lobby;
using TaleWorlds.MountAndBlade.Diamond.Lobby.LocalData;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

public class MPCustomGameFiltersVM : ViewModel
{
	public enum CustomGameFilterType
	{
		Name,
		NotFull,
		HasPlayers,
		HasPasswordProtection,
		IsOfficial,
		ModuleCompatible,
		Favorite
	}

	public Action OnFiltersApplied;

	private string _titleText;

	private string _searchInitialText;

	private string _searchText;

	private MBBindingList<MPCustomGameFilterItemVM> _items;

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
	public string SearchInitialText
	{
		get
		{
			return _searchInitialText;
		}
		set
		{
			if (value != _searchInitialText)
			{
				_searchInitialText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SearchInitialText");
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
				_searchText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SearchText");
				OnAnyFilterChange();
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPCustomGameFilterItemVM> Items
	{
		get
		{
			return _items;
		}
		set
		{
			if (value != _items)
			{
				_items = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPCustomGameFilterItemVM>>(value, "Items");
			}
		}
	}

	public MPCustomGameFiltersVM()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		SearchText = string.Empty;
		MBBindingList<MPCustomGameFilterItemVM> obj = new MBBindingList<MPCustomGameFilterItemVM>();
		((Collection<MPCustomGameFilterItemVM>)(object)obj).Add(new MPCustomGameFilterItemVM(CustomGameFilterType.IsOfficial, new TextObject("{=Tlc2buKG}Is Official", (Dictionary<string, object>)null), (GameServerEntry x) => x.IsOfficial, OnAnyFilterChange));
		((Collection<MPCustomGameFilterItemVM>)(object)obj).Add(new MPCustomGameFilterItemVM(CustomGameFilterType.HasPlayers, new TextObject("{=aB4Md0if}Has players", (Dictionary<string, object>)null), (GameServerEntry x) => x.PlayerCount > 0, OnAnyFilterChange));
		((Collection<MPCustomGameFilterItemVM>)(object)obj).Add(new MPCustomGameFilterItemVM(CustomGameFilterType.HasPasswordProtection, new TextObject("{=v6J8ILV3}No password", (Dictionary<string, object>)null), (GameServerEntry x) => !x.PasswordProtected, OnAnyFilterChange));
		((Collection<MPCustomGameFilterItemVM>)(object)obj).Add(new MPCustomGameFilterItemVM(CustomGameFilterType.NotFull, new TextObject("{=W4DLzPSb}Server not full", (Dictionary<string, object>)null), (GameServerEntry x) => x.MaxPlayerCount - x.PlayerCount > 0, OnAnyFilterChange));
		((Collection<MPCustomGameFilterItemVM>)(object)obj).Add(new MPCustomGameFilterItemVM(CustomGameFilterType.ModuleCompatible, new TextObject("{=CNR4cZwZ}Modules compatible", (Dictionary<string, object>)null), FilterByCompatibleModules, OnAnyFilterChange));
		((Collection<MPCustomGameFilterItemVM>)(object)obj).Add(new MPCustomGameFilterItemVM(CustomGameFilterType.Favorite, new TextObject("{=BDdVhfuJ}Favorite", (Dictionary<string, object>)null), FilterByFavorites, OnAnyFilterChange));
		Items = obj;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=OwqFpPwa}Filters", (Dictionary<string, object>)null)).ToString();
		SearchInitialText = ((object)new TextObject("{=NLKmdNbt}Search", (Dictionary<string, object>)null)).ToString();
		Items.ApplyActionOnAllItems((Action<MPCustomGameFilterItemVM>)delegate(MPCustomGameFilterItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public List<GameServerEntry> GetFilteredServerList(IEnumerable<GameServerEntry> unfilteredList)
	{
		List<GameServerEntry> list = unfilteredList.ToList();
		IEnumerable<MPCustomGameFilterItemVM> enabledFilterItems = ((IEnumerable<MPCustomGameFilterItemVM>)Items).Where((MPCustomGameFilterItemVM filterItem) => filterItem.IsSelected);
		if (enabledFilterItems.Any())
		{
			list.RemoveAll((GameServerEntry s) => enabledFilterItems.Any((MPCustomGameFilterItemVM fi) => !fi.GetIsApplicaple(s)));
		}
		if (!string.IsNullOrEmpty(SearchText))
		{
			list = list.Where((GameServerEntry i) => i.ServerName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
		}
		return list;
	}

	private bool FilterByCompatibleModules(GameServerEntry serverEntry)
	{
		return ModuleInfoModelExtensions.IsCompatibleWith((IEnumerable<ModuleInfoModel>)NetworkMain.GameClient.LoadedUnofficialModules, (IEnumerable<ModuleInfoModel>)serverEntry.LoadedModules, serverEntry.AllowsOptionalModules);
	}

	private bool FilterByFavorites(GameServerEntry serverEntry)
	{
		FavoriteServerData val = default(FavoriteServerData);
		return MultiplayerLocalDataManager.Instance.FavoriteServers.TryGetServerData(serverEntry, ref val);
	}

	private void OnAnyFilterChange()
	{
		OnFiltersApplied?.Invoke();
	}
}
