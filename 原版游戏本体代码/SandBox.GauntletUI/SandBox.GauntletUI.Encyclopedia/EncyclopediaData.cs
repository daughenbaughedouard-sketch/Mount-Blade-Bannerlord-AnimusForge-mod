using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.List;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Encyclopedia;

public class EncyclopediaData
{
	private Dictionary<string, EncyclopediaPage> _pages;

	private string _previousPageID;

	private EncyclopediaHomeVM _homeDatasource;

	private GauntletMovieIdentifier _homeGauntletMovie;

	private Dictionary<EncyclopediaPage, EncyclopediaListVM> _lists;

	private EncyclopediaPageVM _activeDatasource;

	private GauntletLayer _activeGauntletLayer;

	private GauntletMovieIdentifier _activeGauntletMovie;

	private EncyclopediaNavigatorVM _navigatorDatasource;

	private GauntletMovieIdentifier _navigatorActiveGauntletMovie;

	private readonly ScreenBase _screen;

	private TutorialContexts _prevContext;

	private readonly GauntletMapEncyclopediaView _manager;

	private object _initialState;

	public EncyclopediaData(GauntletMapEncyclopediaView manager, ScreenBase screen, EncyclopediaHomeVM homeDatasource, EncyclopediaNavigatorVM navigatorDatasource)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		_manager = manager;
		_screen = screen;
		_pages = new Dictionary<string, EncyclopediaPage>();
		foreach (EncyclopediaPage encyclopediaPage in Campaign.Current.EncyclopediaManager.GetEncyclopediaPages())
		{
			string[] identifierNames = encyclopediaPage.GetIdentifierNames();
			foreach (string key in identifierNames)
			{
				if (!_pages.ContainsKey(key))
				{
					_pages.Add(key, encyclopediaPage);
				}
			}
		}
		_homeDatasource = homeDatasource;
		_lists = new Dictionary<EncyclopediaPage, EncyclopediaListVM>();
		foreach (EncyclopediaPage encyclopediaPage2 in Campaign.Current.EncyclopediaManager.GetEncyclopediaPages())
		{
			if (!_lists.ContainsKey(encyclopediaPage2))
			{
				EncyclopediaListVM val = new EncyclopediaListVM(new EncyclopediaPageArgs((object)encyclopediaPage2));
				_manager.ListViewDataController.LoadListData(val);
				_lists.Add(encyclopediaPage2, val);
			}
		}
		_navigatorDatasource = navigatorDatasource;
		_navigatorDatasource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_navigatorDatasource.SetPreviousPageInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_navigatorDatasource.SetNextPageInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		Game.Current.EventManager.RegisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
	}

	private void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((int)obj.NewContext != 9)
		{
			_prevContext = obj.NewContext;
		}
	}

	internal void OnTick()
	{
		_navigatorDatasource.CanSwitchTabs = !Input.IsGamepadActive || !InformationManager.GetIsAnyTooltipActiveAndExtended();
		if (((ScreenLayer)_activeGauntletLayer).Input.IsHotKeyReleased("Exit") || (((ScreenLayer)_activeGauntletLayer).Input.IsGameKeyReleased(39) && !((ScreenLayer)_activeGauntletLayer).IsFocusedOnInput()))
		{
			if (_navigatorDatasource.IsSearchResultsShown)
			{
				_navigatorDatasource.SearchText = string.Empty;
			}
			else
			{
				_manager.CloseEncyclopedia();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
		}
		else if (!((ScreenLayer)_activeGauntletLayer).IsFocusedOnInput() && _navigatorDatasource.CanSwitchTabs)
		{
			if ((Input.IsKeyPressed((InputKey)14) && _navigatorDatasource.IsBackEnabled) || ((ScreenLayer)_activeGauntletLayer).Input.IsHotKeyReleased("SwitchToPreviousTab"))
			{
				_navigatorDatasource.ExecuteBack();
			}
			else if (((ScreenLayer)_activeGauntletLayer).Input.IsHotKeyReleased("SwitchToNextTab"))
			{
				_navigatorDatasource.ExecuteForward();
			}
		}
		if (_activeGauntletLayer != null)
		{
			object initialState = _initialState;
			Game current = Game.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				GameStateManager gameStateManager = current.GameStateManager;
				obj = ((gameStateManager != null) ? gameStateManager.ActiveState : null);
			}
			if (initialState != obj)
			{
				_manager.CloseEncyclopedia();
			}
		}
		EncyclopediaPageVM activeDatasource = _activeDatasource;
		if (activeDatasource != null)
		{
			activeDatasource.OnTick();
		}
	}

	private void SetEncyclopediaPage(string pageId, object obj)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		GauntletLayer activeGauntletLayer = _activeGauntletLayer;
		if (_activeGauntletLayer != null && _activeGauntletMovie != null)
		{
			_activeGauntletLayer.ReleaseMovie(_activeGauntletMovie);
		}
		EncyclopediaPageVM activeDatasource = _activeDatasource;
		EncyclopediaListVM val;
		if ((val = (EncyclopediaListVM)(object)((activeDatasource is EncyclopediaListVM) ? activeDatasource : null)) != null)
		{
			EncyclopediaListItemVM val2 = ((IEnumerable<EncyclopediaListItemVM>)((EncyclopediaPageVM)val).Items).FirstOrDefault((Func<EncyclopediaListItemVM, bool>)((EncyclopediaListItemVM x) => x.Object == obj));
			_manager.ListViewDataController.SaveListData(val, (val2 != null) ? val2.Id : val.LastSelectedItemId);
		}
		if (_activeGauntletLayer == null)
		{
			_activeGauntletLayer = new GauntletLayer("EncyclopediaBar", 310, false);
			_navigatorActiveGauntletMovie = _activeGauntletLayer.LoadMovie("EncyclopediaBar", (ViewModel)(object)_navigatorDatasource);
			_navigatorDatasource.PageName = ((EncyclopediaPageVM)_homeDatasource).GetName();
			((ScreenLayer)_activeGauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_activeGauntletLayer);
			((ScreenLayer)_activeGauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			((ScreenLayer)_activeGauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
			Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
			Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)9));
			_initialState = Game.Current.GameStateManager.ActiveState;
		}
		if (pageId == "Home")
		{
			_activeGauntletMovie = _activeGauntletLayer.LoadMovie("EncyclopediaHome", (ViewModel)(object)_homeDatasource);
			_homeGauntletMovie = _activeGauntletMovie;
			_activeDatasource = (EncyclopediaPageVM)(object)_homeDatasource;
			_activeDatasource.Refresh();
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)1, false));
		}
		else if (pageId == "ListPage")
		{
			object obj2 = obj;
			EncyclopediaPage val3 = (EncyclopediaPage)((obj2 is EncyclopediaPage) ? obj2 : null);
			_activeDatasource = (EncyclopediaPageVM)(object)_lists[val3];
			_activeGauntletMovie = _activeGauntletLayer.LoadMovie("EncyclopediaItemList", (ViewModel)(object)_activeDatasource);
			_activeDatasource.Refresh();
			EncyclopediaListViewDataController listViewDataController = _manager.ListViewDataController;
			EncyclopediaPageVM activeDatasource2 = _activeDatasource;
			listViewDataController.LoadListData((EncyclopediaListVM)(object)((activeDatasource2 is EncyclopediaListVM) ? activeDatasource2 : null));
			SetTutorialListPageContext(val3);
		}
		else
		{
			EncyclopediaPage val4 = _pages[pageId];
			_activeDatasource = GetEncyclopediaPageInstance(val4, obj);
			EncyclopediaPageVM activeDatasource3 = _activeDatasource;
			EncyclopediaPageVM obj3 = ((activeDatasource3 is EncyclopediaContentPageVM) ? activeDatasource3 : null);
			if (obj3 != null)
			{
				((EncyclopediaContentPageVM)obj3).InitializeQuickNavigation(_lists[val4]);
			}
			_activeGauntletMovie = _activeGauntletLayer.LoadMovie(_pages[pageId].GetViewFullyQualifiedName(), (ViewModel)(object)_activeDatasource);
			SetTutorialPageContext(_activeDatasource);
		}
		_navigatorDatasource.NavBarString = _activeDatasource.GetNavigationBarURL();
		if (activeGauntletLayer != null && activeGauntletLayer != _activeGauntletLayer)
		{
			_screen.RemoveLayer((ScreenLayer)(object)activeGauntletLayer);
			_screen.AddLayer((ScreenLayer)(object)_activeGauntletLayer);
		}
		else if (activeGauntletLayer == null && _activeGauntletLayer != null)
		{
			_screen.AddLayer((ScreenLayer)(object)_activeGauntletLayer);
		}
		((ScreenLayer)_activeGauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		_previousPageID = pageId;
	}

	internal EncyclopediaPageVM ExecuteLink(string pageId, object obj, bool needsRefresh)
	{
		SetEncyclopediaPage(pageId, obj);
		return _activeDatasource;
	}

	private EncyclopediaPageVM GetEncyclopediaPageInstance(EncyclopediaPage page, object o)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Type type = null;
		EncyclopediaPageArgs val = default(EncyclopediaPageArgs);
		((EncyclopediaPageArgs)(ref val))._002Ector(o);
		Assembly assembly = typeof(EncyclopediaHomeVM).Assembly;
		Type[] types = assembly.GetTypes();
		foreach (Type type2 in types)
		{
			if (IsEncyclopediaPageType(page, type2))
			{
				type = type2;
			}
		}
		Assembly[] activeReferencingGameAssembliesSafe = Extensions.GetActiveReferencingGameAssembliesSafe(assembly);
		for (int j = 0; j < activeReferencingGameAssembliesSafe.Length; j++)
		{
			List<Type> typesSafe = Extensions.GetTypesSafe(activeReferencingGameAssembliesSafe[j], (Func<Type, bool>)null);
			for (int k = 0; k < typesSafe.Count; k++)
			{
				Type type3 = typesSafe[k];
				if (IsEncyclopediaPageType(page, type3))
				{
					type = type3;
				}
			}
		}
		if (type != null)
		{
			object? obj = Activator.CreateInstance(type, val);
			return (EncyclopediaPageVM)((obj is EncyclopediaPageVM) ? obj : null);
		}
		return null;
	}

	private static bool IsEncyclopediaPageType(EncyclopediaPage page, Type type)
	{
		if (typeof(EncyclopediaPageVM).IsAssignableFrom(type))
		{
			object[] customAttributesSafe = Extensions.GetCustomAttributesSafe(type, typeof(EncyclopediaViewModel), false);
			foreach (object obj in customAttributesSafe)
			{
				EncyclopediaViewModel val;
				if ((val = (EncyclopediaViewModel)((obj is EncyclopediaViewModel) ? obj : null)) != null && page.HasIdentifierType(val.PageTargetType))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnFinalize()
	{
		Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
		_pages = null;
		_homeDatasource = null;
		foreach (KeyValuePair<EncyclopediaPage, EncyclopediaListVM> list in _lists)
		{
			EncyclopediaListVM value = list.Value;
			if (value != null)
			{
				((ViewModel)value).OnFinalize();
			}
		}
		_lists = null;
		_activeGauntletMovie = null;
		_activeDatasource = null;
		_activeGauntletLayer = null;
		_navigatorActiveGauntletMovie = null;
		_navigatorDatasource = null;
		_initialState = null;
		Game.Current.EventManager.UnregisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
	}

	public void CloseEncyclopedia()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		EncyclopediaPageVM activeDatasource = _activeDatasource;
		EncyclopediaListVM val;
		if ((val = (EncyclopediaListVM)(object)((activeDatasource is EncyclopediaListVM) ? activeDatasource : null)) != null)
		{
			_manager.ListViewDataController.SaveListData(val, val.LastSelectedItemId);
		}
		ResetPageFilters();
		_activeGauntletLayer.ReleaseMovie(_activeGauntletMovie);
		_screen.RemoveLayer((ScreenLayer)(object)_activeGauntletLayer);
		((ScreenLayer)_activeGauntletLayer).InputRestrictions.ResetInputRestrictions();
		OnFinalize();
		Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)0, false));
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent(_prevContext));
	}

	private void ResetPageFilters()
	{
		foreach (EncyclopediaListVM value in _lists.Values)
		{
			foreach (EncyclopediaFilterGroupVM item in (Collection<EncyclopediaFilterGroupVM>)(object)((EncyclopediaPageVM)value).FilterGroups)
			{
				foreach (EncyclopediaListFilterVM item2 in (Collection<EncyclopediaListFilterVM>)(object)item.Filters)
				{
					item2.IsSelected = false;
				}
			}
		}
	}

	private void SetTutorialPageContext(EncyclopediaPageVM _page)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		if (_page is EncyclopediaClanPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)9, false));
		}
		else if (_page is EncyclopediaConceptPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)15, false));
		}
		else if (_page is EncyclopediaFactionPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)12, false));
		}
		else if (_page is EncyclopediaUnitPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)11, false));
		}
		else if (_page is EncyclopediaHeroPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)13, false));
		}
		else if (_page is EncyclopediaShipPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)14, false));
		}
		else if (_page is EncyclopediaSettlementPageVM)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)10, false));
		}
	}

	private void SetTutorialListPageContext(EncyclopediaPage _page)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		if (_page is DefaultEncyclopediaClanPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)4, false));
		}
		else if (_page is DefaultEncyclopediaConceptPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)8, false));
		}
		else if (_page is DefaultEncyclopediaFactionPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)5, false));
		}
		else if (_page is DefaultEncyclopediaUnitPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)3, false));
		}
		else if (_page is DefaultEncyclopediaHeroPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)6, false));
		}
		else if (_page is DefaultEncyclopediaShipPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)7, false));
		}
		else if (_page is DefaultEncyclopediaSettlementPage)
		{
			Game.Current.EventManager.TriggerEvent<EncyclopediaPageChangedEvent>(new EncyclopediaPageChangedEvent((EncyclopediaPages)2, false));
		}
	}
}
