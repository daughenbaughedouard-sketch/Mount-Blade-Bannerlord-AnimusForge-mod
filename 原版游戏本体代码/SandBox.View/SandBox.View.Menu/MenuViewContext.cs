using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Menu;

public class MenuViewContext : IMenuContextHandler
{
	private MenuContext _menuContext;

	private MenuView _currentMenuBase;

	private MenuView _currentMenuBackground;

	private MenuView _menuCharacterDeveloper;

	private MenuView _menuOverlayBase;

	private MenuView _menuRecruitVolunteers;

	private MenuView _menuTournamentLeaderboard;

	private MenuView _menuTroopSelection;

	private MenuView _menuTownManagement;

	private SoundEvent _panelSound;

	private SoundEvent _ambientSound;

	private MenuOverlayType _currentOverlayType;

	private ScreenBase _screen;

	private bool _isActive;

	internal GameMenu CurGameMenu => _menuContext.GameMenu;

	public MenuContext MenuContext => _menuContext;

	public List<MenuView> MenuViews { get; private set; }

	public MenuViewContext(ScreenBase screen, MenuContext menuContext)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		_screen = screen;
		_menuContext = menuContext;
		MenuViews = new List<MenuView>();
		_menuContext.Handler = (IMenuContextHandler)(object)this;
		if ((int)Campaign.Current.GameMode != 2 && CurGameMenu.StringId != "siege_test_menu")
		{
			((IMenuContextHandler)this).OnMenuCreate();
			((IMenuContextHandler)this).OnMenuActivate();
		}
	}

	public void UpdateMenuContext(MenuContext menuContext)
	{
		_menuContext = menuContext;
		_menuContext.Handler = (IMenuContextHandler)(object)this;
		MenuViews.ForEach(delegate(MenuView m)
		{
			m.MenuContext = menuContext;
		});
		MenuViews.ForEach(delegate(MenuView m)
		{
			m.OnMenuContextUpdated(menuContext);
		});
		CheckAndInitializeOverlay();
	}

	public void AddLayer(ScreenLayer layer)
	{
		_screen.AddLayer(layer);
	}

	public void RemoveLayer(ScreenLayer layer)
	{
		_screen.RemoveLayer(layer);
	}

	public T FindLayer<T>() where T : ScreenLayer
	{
		return _screen.FindLayer<T>();
	}

	public T FindLayer<T>(string name) where T : ScreenLayer
	{
		return _screen.FindLayer<T>(name);
	}

	public void OnFrameTick(float dt)
	{
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuView menuView = MenuViews[i];
			menuView.OnFrameTick(dt);
			if (menuView.Removed)
			{
				i--;
			}
		}
	}

	public void OnResume()
	{
		_isActive = true;
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuViews[i].OnResume();
		}
	}

	public void OnHourlyTick()
	{
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuViews[i].OnHourlyTick();
		}
	}

	public void OnActivate()
	{
		_isActive = true;
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuViews[i].OnActivate();
		}
		MenuContext menuContext = MenuContext;
		if (!string.IsNullOrEmpty((menuContext != null) ? menuContext.CurrentAmbientSoundID : null))
		{
			PlayAmbientSound(MenuContext.CurrentAmbientSoundID);
		}
		MenuContext menuContext2 = MenuContext;
		if (!string.IsNullOrEmpty((menuContext2 != null) ? menuContext2.CurrentPanelSoundID : null))
		{
			PlayPanelSound(MenuContext.CurrentPanelSoundID);
		}
	}

	public void OnDeactivate()
	{
		_isActive = false;
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuViews[i].OnDeactivate();
		}
		StopAllSounds();
	}

	public void OnInitialize()
	{
	}

	public void OnFinalize()
	{
		ClearMenuViews();
		MBInformationManager.HideInformations();
		_menuContext = null;
	}

	private void ClearMenuViews()
	{
		MenuView[] array = MenuViews.ToArray();
		foreach (MenuView menuView in array)
		{
			RemoveMenuView(menuView);
		}
		_menuCharacterDeveloper = null;
		_menuOverlayBase = null;
		_menuRecruitVolunteers = null;
		_menuTownManagement = null;
		_menuTroopSelection = null;
	}

	public void StopAllSounds()
	{
		SoundEvent ambientSound = _ambientSound;
		if (ambientSound != null)
		{
			ambientSound.Release();
		}
		SoundEvent panelSound = _panelSound;
		if (panelSound != null)
		{
			panelSound.Release();
		}
	}

	private void PlayAmbientSound(string ambientSoundID)
	{
		if (_isActive)
		{
			SoundEvent ambientSound = _ambientSound;
			if (ambientSound != null)
			{
				ambientSound.Release();
			}
			_ambientSound = SoundEvent.CreateEventFromString(ambientSoundID, (Scene)null);
			_ambientSound.Play();
		}
	}

	private void PlayPanelSound(string panelSoundID)
	{
		if (_isActive)
		{
			SoundEvent panelSound = _panelSound;
			if (panelSound != null)
			{
				panelSound.Release();
			}
			_panelSound = SoundEvent.CreateEventFromString(panelSoundID, (Scene)null);
			_panelSound.Play();
		}
	}

	void IMenuContextHandler.OnAmbientSoundIDSet(string ambientSoundID)
	{
		PlayAmbientSound(ambientSoundID);
	}

	void IMenuContextHandler.OnPanelSoundIDSet(string panelSoundID)
	{
		PlayPanelSound(panelSoundID);
	}

	void IMenuContextHandler.OnMenuCreate()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		int num;
		if ((int)Campaign.Current.GameMode != 2)
		{
			num = ((CurGameMenu.StringId == "siege_test_menu") ? 1 : 0);
			if (num == 0)
			{
				goto IL_0041;
			}
		}
		else
		{
			num = 1;
		}
		if (_currentMenuBackground == null)
		{
			_currentMenuBackground = AddMenuView<MenuBackgroundView>(Array.Empty<object>());
		}
		goto IL_0041;
		IL_0041:
		if (_currentMenuBase == null)
		{
			_currentMenuBase = AddMenuView<MenuBaseView>(Array.Empty<object>());
		}
		if (num == 0)
		{
			CheckAndInitializeOverlay();
		}
		StopAllSounds();
	}

	void IMenuContextHandler.OnMenuActivate()
	{
		foreach (MenuView menuView in MenuViews)
		{
			menuView.OnActivate();
		}
	}

	public void OnMapConversationActivated()
	{
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuView menuView = MenuViews[i];
			menuView.OnMapConversationActivated();
			if (menuView.Removed)
			{
				i--;
			}
		}
	}

	public void OnMapConversationDeactivated()
	{
		for (int i = 0; i < MenuViews.Count; i++)
		{
			MenuView menuView = MenuViews[i];
			menuView.OnMapConversationDeactivated();
			if (menuView.Removed)
			{
				i--;
			}
		}
	}

	public void OnGameStateDeactivate()
	{
	}

	public void OnGameStateInitialize()
	{
	}

	public void OnGameStateFinalize()
	{
	}

	private void CheckAndInitializeOverlay()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		MenuOverlayType menuOverlayType = Campaign.Current.GameMenuManager.GetMenuOverlayType(_menuContext);
		if ((int)menuOverlayType != 0)
		{
			if (menuOverlayType != _currentOverlayType)
			{
				if (_menuOverlayBase != null && (((int)_currentOverlayType != 4 && (int)menuOverlayType == 4) || ((int)_currentOverlayType == 4 && ((int)menuOverlayType == 3 || (int)menuOverlayType == 2 || (int)menuOverlayType == 1))))
				{
					RemoveMenuView(_menuOverlayBase);
					_menuOverlayBase = null;
				}
				if (_menuOverlayBase == null)
				{
					_menuOverlayBase = AddMenuView<MenuOverlayBaseView>(Array.Empty<object>());
				}
				else
				{
					_menuOverlayBase.OnOverlayTypeChange(menuOverlayType);
				}
			}
			else
			{
				_menuOverlayBase?.OnOverlayTypeChange(menuOverlayType);
			}
		}
		else
		{
			if (_menuOverlayBase != null)
			{
				RemoveMenuView(_menuOverlayBase);
				_menuOverlayBase = null;
			}
			if (_currentMenuBackground != null)
			{
				RemoveMenuView(_currentMenuBackground);
				_currentMenuBackground = null;
			}
		}
		_currentOverlayType = menuOverlayType;
	}

	public void CloseCharacterDeveloper()
	{
		RemoveMenuView(_menuCharacterDeveloper);
		_menuCharacterDeveloper = null;
		foreach (MenuView menuView in MenuViews)
		{
			menuView.OnCharacterDeveloperClosed();
		}
	}

	public MenuView AddMenuView<T>(params object[] parameters) where T : MenuView, new()
	{
		MenuView menuView = SandBoxViewCreator.CreateMenuView<T>(parameters);
		menuView.MenuViewContext = this;
		menuView.MenuContext = _menuContext;
		MenuViews.Add(menuView);
		menuView.OnInitialize();
		return menuView;
	}

	public T GetMenuView<T>() where T : MenuView
	{
		foreach (MenuView menuView in MenuViews)
		{
			if (menuView is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void RemoveMenuView(MenuView menuView)
	{
		menuView.OnFinalize();
		menuView.Removed = true;
		MenuViews.Remove(menuView);
		if (menuView.ShouldUpdateMenuAfterRemoved)
		{
			MenuViews.ForEach(delegate(MenuView m)
			{
				m.OnMenuContextUpdated(_menuContext);
			});
		}
	}

	void IMenuContextHandler.OnBackgroundMeshNameSet(string name)
	{
		foreach (MenuView menuView in MenuViews)
		{
			menuView.OnBackgroundMeshNameSet(name);
		}
	}

	void IMenuContextHandler.OnOpenTownManagement()
	{
		if (_menuTownManagement == null)
		{
			_menuTownManagement = AddMenuView<MenuTownManagementView>(Array.Empty<object>());
		}
	}

	public void CloseTownManagement()
	{
		RemoveMenuView(_menuTownManagement);
		_menuTownManagement = null;
	}

	void IMenuContextHandler.OnOpenRecruitVolunteers()
	{
		if (_menuRecruitVolunteers == null)
		{
			_menuRecruitVolunteers = AddMenuView<MenuRecruitVolunteersView>(Array.Empty<object>());
		}
	}

	public void CloseRecruitVolunteers()
	{
		RemoveMenuView(_menuRecruitVolunteers);
		_menuRecruitVolunteers = null;
	}

	void IMenuContextHandler.OnOpenTournamentLeaderboard()
	{
		if (_menuTournamentLeaderboard == null)
		{
			_menuTournamentLeaderboard = AddMenuView<MenuTournamentLeaderboardView>(Array.Empty<object>());
		}
	}

	public void CloseTournamentLeaderboard()
	{
		RemoveMenuView(_menuTournamentLeaderboard);
		_menuTournamentLeaderboard = null;
	}

	void IMenuContextHandler.OnOpenTroopSelection(TroopRoster fullRoster, TroopRoster initialSelections, Func<CharacterObject, bool> canChangeStatusOfTroop, Action<TroopRoster> onDone, int maxSelectableTroopCount, int minSelectableTroopCount)
	{
		if (_menuTroopSelection == null)
		{
			_menuTroopSelection = AddMenuView<MenuTroopSelectionView>(new object[6] { fullRoster, initialSelections, canChangeStatusOfTroop, onDone, maxSelectableTroopCount, minSelectableTroopCount });
		}
	}

	public void CloseTroopSelection()
	{
		RemoveMenuView(_menuTroopSelection);
		_menuTroopSelection = null;
	}

	void IMenuContextHandler.OnMenuRefresh()
	{
		foreach (MenuView menuView in MenuViews)
		{
			menuView.OnMenuContextRefreshed();
		}
	}
}
