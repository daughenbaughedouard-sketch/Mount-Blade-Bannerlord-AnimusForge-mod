using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace SandBox.GauntletUI.Encyclopedia;

public class EncyclopediaListViewDataController
{
	private readonly struct EncyclopediaListViewData
	{
		public readonly Dictionary<EncyclopediaFilterItem, bool> Filters;

		public readonly int SelectedSortIndex;

		public readonly string LastSelectedItemId;

		public readonly bool IsAscending;

		public EncyclopediaListViewData(MBBindingList<EncyclopediaFilterGroupVM> filters, int selectedSortIndex, string lastSelectedItemId, bool isAscending)
		{
			Dictionary<EncyclopediaFilterItem, bool> dictionary = new Dictionary<EncyclopediaFilterItem, bool>();
			foreach (EncyclopediaFilterGroupVM item in (Collection<EncyclopediaFilterGroupVM>)(object)filters)
			{
				foreach (EncyclopediaListFilterVM item2 in (Collection<EncyclopediaListFilterVM>)(object)item.Filters)
				{
					if (!dictionary.ContainsKey(item2.Filter))
					{
						dictionary.Add(item2.Filter, item2.IsSelected);
					}
				}
			}
			Filters = dictionary;
			SelectedSortIndex = selectedSortIndex;
			LastSelectedItemId = lastSelectedItemId;
			IsAscending = isAscending;
		}
	}

	private Dictionary<EncyclopediaPage, EncyclopediaListViewData> _listData;

	public EncyclopediaListViewDataController()
	{
		_listData = new Dictionary<EncyclopediaPage, EncyclopediaListViewData>();
		foreach (EncyclopediaPage encyclopediaPage in Campaign.Current.EncyclopediaManager.GetEncyclopediaPages())
		{
			if (!_listData.ContainsKey(encyclopediaPage))
			{
				_listData.Add(encyclopediaPage, new EncyclopediaListViewData(new MBBindingList<EncyclopediaFilterGroupVM>(), 0, "", isAscending: false));
			}
		}
	}

	public void SaveListData(EncyclopediaListVM list, string id)
	{
		if (list != null && _listData.ContainsKey(list.Page))
		{
			EncyclopediaListSortControllerVM sortController = ((EncyclopediaPageVM)list).SortController;
			int selectedSortIndex = ((sortController == null) ? ((int?)null) : ((SelectorVM<EncyclopediaListSelectorItemVM>)(object)sortController.SortSelection)?.SelectedIndex) ?? 0;
			Dictionary<EncyclopediaPage, EncyclopediaListViewData> listData = _listData;
			EncyclopediaPage page = list.Page;
			MBBindingList<EncyclopediaFilterGroupVM> filterGroups = ((EncyclopediaPageVM)list).FilterGroups;
			EncyclopediaListSortControllerVM sortController2 = ((EncyclopediaPageVM)list).SortController;
			listData[page] = new EncyclopediaListViewData(filterGroups, selectedSortIndex, id, sortController2 != null && sortController2.GetSortOrder());
		}
	}

	public void LoadListData(EncyclopediaListVM list)
	{
		if (list != null && _listData.ContainsKey(list.Page))
		{
			EncyclopediaListViewData encyclopediaListViewData = _listData[list.Page];
			EncyclopediaListSortControllerVM sortController = ((EncyclopediaPageVM)list).SortController;
			if (sortController != null)
			{
				sortController.SetSortSelection(encyclopediaListViewData.SelectedSortIndex);
			}
			EncyclopediaListSortControllerVM sortController2 = ((EncyclopediaPageVM)list).SortController;
			if (sortController2 != null)
			{
				sortController2.SetSortOrder(encyclopediaListViewData.IsAscending);
			}
			list.CopyFiltersFrom(encyclopediaListViewData.Filters);
			list.LastSelectedItemId = encyclopediaListViewData.LastSelectedItemId;
		}
	}
}
