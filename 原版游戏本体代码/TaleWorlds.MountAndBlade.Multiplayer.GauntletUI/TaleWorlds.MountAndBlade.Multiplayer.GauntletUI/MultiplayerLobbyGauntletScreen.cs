using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Ranked;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.AuxiliaryKeys;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions.GameKeys;
using TaleWorlds.PlayerServices;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

[GameStateScreen(typeof(LobbyState))]
public class MultiplayerLobbyGauntletScreen : ScreenBase, IGameStateListener, ILobbyStateHandler, IChatLogHandlerScreen
{
	private List<KeyValuePair<string, InquiryData>> _feedbackInquiries;

	private string _activeFeedbackId;

	private KeybindingPopup _keybindingPopup;

	private KeyOptionVM _currentKey;

	private SpriteCategory _optionsSpriteCategory;

	private SpriteCategory _multiplayerSpriteCategory;

	private GauntletLayer _gauntletBrightnessLayer;

	private BrightnessOptionVM _brightnessOptionDataSource;

	private GauntletMovieIdentifier _brightnessOptionMovie;

	private LobbyState _lobbyState;

	private BasicCharacterObject _playerCharacter;

	private bool _isFacegenOpen;

	private SoundEvent _musicSoundEvent;

	private bool _isNavigationRestricted;

	private bool _wasChatRestricted;

	private MPCustomGameSortControllerVM.CustomServerSortOption? _cachedCustomServerSortOption;

	private MPCustomGameSortControllerVM.SortState _cachedCustomServerSortState;

	private MPCustomGameSortControllerVM.CustomServerSortOption? _cachedPremadeGameSortOption;

	private MPCustomGameSortControllerVM.SortState _cachedPremadeGameSortState;

	private bool _isLobbyActive;

	private GauntletLayer _lobbyLayer;

	private MPLobbyVM _lobbyDataSource;

	private SpriteCategory _mplobbyCategory;

	private SpriteCategory _bannerIconsCategory;

	private SpriteCategory _badgesCategory;

	public MPLobbyVM.LobbyPage CurrentPage
	{
		get
		{
			if (_lobbyDataSource != null)
			{
				return _lobbyDataSource.CurrentPage;
			}
			return MPLobbyVM.LobbyPage.NotAssigned;
		}
	}

	public MPLobbyVM DataSource => _lobbyDataSource;

	public GauntletLayer LobbyLayer => _lobbyLayer;

	public MultiplayerLobbyGauntletScreen(LobbyState lobbyState)
	{
		AvatarThumbnailCache current = AvatarThumbnailCache.Current;
		if (current != null)
		{
			current.FlushCache();
		}
		_feedbackInquiries = new List<KeyValuePair<string, InquiryData>>();
		_lobbyState = lobbyState;
		_lobbyState.Handler = this;
		GauntletFullScreenNoticeView.Initialize();
		MultiplayerGauntletGameNotification.Initialize();
		GauntletChatLogView current2 = GauntletChatLogView.Current;
		if (current2 != null)
		{
			current2.LoadMovie(true);
		}
		GauntletChatLogView current3 = GauntletChatLogView.Current;
		if (current3 != null)
		{
			current3.SetEnabled(false);
		}
		MultiplayerAdminInformationScreen.OnInitialize();
		MultiplayerReportPlayerScreen.OnInitialize();
	}

	protected override void OnInitialize()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		LoadingWindow.DisableGlobalLoadingWindow();
		_keybindingPopup = new KeybindingPopup((Action<Key>)SetHotKey, (ScreenBase)(object)this);
		_lobbyDataSource?.RefreshPlayerData(_lobbyState.LobbyClient.PlayerData);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		InformationManager.HideAllMessages();
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 33)
		{
			_lobbyDataSource?.OnEnableGenericAvatarsChanged();
		}
		if ((int)changedManagedOptionsType == 34)
		{
			_lobbyDataSource?.OnEnableGenericNamesChanged();
		}
	}

	void IChatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref bool isTeamChatAvailable, ref bool inputEnabled, ref bool isToggleChatHintAvailable, ref bool isMouseVisible, ref InputContext inputContext)
	{
		if (LobbyLayer == null)
		{
			return;
		}
		MPLobbyVM lobbyDataSource = _lobbyDataSource;
		int num;
		if (lobbyDataSource != null)
		{
			MPOptionsVM options = lobbyDataSource.Options;
			if (((options != null) ? new bool?(options.IsEnabled) : ((bool?)null)) == true)
			{
				num = 1;
				goto IL_0065;
			}
		}
		MPLobbyVM lobbyDataSource2 = _lobbyDataSource;
		num = ((lobbyDataSource2 != null && !lobbyDataSource2.HasNoPopupOpen()) ? 1 : 0);
		goto IL_0065;
		IL_0065:
		bool flag = (byte)num != 0;
		inputEnabled = !flag && !_wasChatRestricted;
		inputContext = ((ScreenLayer)LobbyLayer).Input;
		_wasChatRestricted = flag;
	}

	protected override void OnFinalize()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		AvatarThumbnailCache current = AvatarThumbnailCache.Current;
		if (current != null)
		{
			current.FlushCache();
		}
		if (_lobbyDataSource != null)
		{
			((ViewModel)_lobbyDataSource).OnFinalize();
			_lobbyDataSource = null;
		}
		SpriteCategory mplobbyCategory = _mplobbyCategory;
		if (mplobbyCategory != null)
		{
			mplobbyCategory.Unload();
		}
		_optionsSpriteCategory.Unload();
		_multiplayerSpriteCategory.Unload();
		SpriteCategory badgesCategory = _badgesCategory;
		if (badgesCategory != null)
		{
			badgesCategory.Unload();
		}
		GauntletGameNotification.Initialize();
		MultiplayerReportPlayerScreen.OnFinalize();
		MultiplayerAdminInformationScreen.OnRemove();
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		_lobbyState.Handler = null;
		_lobbyState = null;
		((ScreenBase)this).OnFinalize();
	}

	protected override void OnActivate()
	{
		if (_lobbyDataSource != null && _isFacegenOpen)
		{
			OnFacegenClosed(updateCharacter: true);
		}
		_lobbyDataSource?.OnActivate();
		_lobbyDataSource?.RefreshPlayerData(_lobbyState.LobbyClient.PlayerData);
		_lobbyDataSource?.RefreshRecentGames();
		LoadingWindow.DisableGlobalLoadingWindow();
	}

	private void CreateView()
	{
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Expected O, but got Unknown
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Expected O, but got Unknown
		_musicSoundEvent = SoundEvent.CreateEventFromString("event:/multiplayer/lobby_music", (Scene)null);
		_musicSoundEvent.Play();
		if (!(GameStateManager.Current.ActiveState is MissionState))
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		_mplobbyCategory = UIResourceManager.LoadSpriteCategory("ui_mplobby");
		_bannerIconsCategory = UIResourceManager.LoadSpriteCategory("ui_bannericons");
		_badgesCategory = UIResourceManager.LoadSpriteCategory("ui_mpbadges");
		_lobbyDataSource = new MPLobbyVM(_lobbyState, OnOpenFacegen, OnForceCloseFacegen, OnLogout, OnKeybindRequest, GetContinueKeyText, SetNavigationRestriction);
		GameKeyContext category = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
		_lobbyDataSource.CreateInputKeyVisuals(category.GetHotKey("Exit"), category.GetHotKey("Confirm"), category.GetHotKey("SwitchToPreviousTab"), category.GetHotKey("SwitchToNextTab"), category.GetHotKey("TakeAll"), category.GetHotKey("GiveAll"));
		_lobbyLayer = new GauntletLayer("LobbyScreen", 10, true);
		_lobbyLayer.LoadMovie("Lobby", (ViewModel)(object)_lobbyDataSource);
		((ScreenLayer)_lobbyLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_lobbyLayer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_lobbyLayer);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_lobbyLayer);
		((ScreenLayer)_lobbyLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_lobbyLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MultiplayerHotkeyCategory"));
		GameKeyContext category2 = HotKeyManager.GetCategory("MultiplayerHotkeyCategory");
		GameKeyContext genericPanelCategory = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
		_lobbyDataSource.BadgeSelectionPopup.RefreshKeyBindings(category2.GetHotKey("InspectBadgeProgression"));
		_lobbyDataSource.BadgeProgressionInformation.SetPreviousTabInputKey(genericPanelCategory.GetHotKey("SwitchToPreviousTab"));
		_lobbyDataSource.BadgeProgressionInformation.SetNextTabInputKey(genericPanelCategory.GetHotKey("SwitchToNextTab"));
		_lobbyDataSource.Armory.Cosmetics.RefreshKeyBindings(category2.GetHotKey("PerformActionOnCosmeticItem"), category2.GetHotKey("PreviewCosmeticItem"));
		_lobbyDataSource.Armory.Cosmetics.TauntSlots.ApplyActionOnAllItems((Action<MPArmoryCosmeticTauntSlotVM>)delegate(MPArmoryCosmeticTauntSlotVM t)
		{
			t.SetSelectKeyVisual(genericPanelCategory.GetHotKey("GiveAll"));
		});
		_lobbyDataSource.Armory.Cosmetics.TauntSlots.ApplyActionOnAllItems((Action<MPArmoryCosmeticTauntSlotVM>)delegate(MPArmoryCosmeticTauntSlotVM t)
		{
			t.SetEmptySlotKeyVisual(genericPanelCategory.GetHotKey("TakeAll"));
		});
		_lobbyDataSource.Friends.SetToggleFriendListKey(((IEnumerable<HotKey>)category2.RegisteredHotKeys).FirstOrDefault((Func<HotKey, bool>)((HotKey g) => g?.Id == "ToggleFriendsList")));
		_lobbyDataSource.Matchmaking.CustomServer.SortController.InitializeWithSortState(_cachedCustomServerSortOption, _cachedCustomServerSortState);
		_lobbyDataSource.Matchmaking.PremadeMatches.SortController.InitializeWithSortState(_cachedPremadeGameSortOption, _cachedPremadeGameSortState);
		((OptionsVM)_lobbyDataSource.Options).SetDoneInputKey(genericPanelCategory.GetHotKey("Confirm"));
		((OptionsVM)_lobbyDataSource.Options).SetCancelInputKey(genericPanelCategory.GetHotKey("Exit"));
		((OptionsVM)_lobbyDataSource.Options).SetResetInputKey(genericPanelCategory.GetHotKey("Reset"));
		((OptionsVM)_lobbyDataSource.Options).SetPreviousTabInputKey(genericPanelCategory.GetHotKey("TakeAll"));
		((OptionsVM)_lobbyDataSource.Options).SetNextTabInputKey(genericPanelCategory.GetHotKey("GiveAll"));
		_lobbyDataSource.Matchmaking.CustomServer.SetRefreshInputKey(genericPanelCategory.GetHotKey("Reset"));
		if (NativeOptions.GetConfig((NativeOptionsType)69) < 2f)
		{
			_brightnessOptionDataSource = new BrightnessOptionVM((Action<bool>)OnCloseBrightness)
			{
				Visible = true
			};
			_gauntletBrightnessLayer = new GauntletLayer("MultiplayerBrightness", 11, false);
			((ScreenLayer)_gauntletBrightnessLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)3);
			_brightnessOptionMovie = _gauntletBrightnessLayer.LoadMovie("BrightnessOption", (ViewModel)(object)_brightnessOptionDataSource);
			((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletBrightnessLayer);
		}
		_optionsSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_options");
		_multiplayerSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_mpmission");
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		_lobbyDataSource?.OnDeactivate();
	}

	private void OnCloseBrightness(bool isConfirm)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_gauntletBrightnessLayer.ReleaseMovie(_brightnessOptionMovie);
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletBrightnessLayer);
		_brightnessOptionDataSource = null;
		_gauntletBrightnessLayer = null;
		NativeOptions.SaveConfig();
	}

	private void OnOpenFacegen(BasicCharacterObject character)
	{
		_isFacegenOpen = true;
		_playerCharacter = character;
		LoadingWindow.EnableGlobalLoadingWindow();
		ScreenManager.PushScreen(ViewCreator.CreateMBFaceGeneratorScreen(character, true, (IFaceGeneratorCustomFilter)null));
	}

	private void OnForceCloseFacegen()
	{
		if (_isFacegenOpen)
		{
			OnFacegenClosed(updateCharacter: false);
			ScreenManager.PopScreen();
		}
	}

	private void OnFacegenClosed(bool updateCharacter)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (updateCharacter)
		{
			NetworkMain.GameClient.UpdateCharacter(_playerCharacter.GetBodyPropertiesMin(false), _playerCharacter.IsFemale);
		}
		ScreenManager.TrySetFocus((ScreenLayer)(object)_lobbyLayer);
		_lobbyDataSource.RefreshPlayerData(_lobbyState.LobbyClient.PlayerData);
		_isFacegenOpen = false;
		_playerCharacter = null;
	}

	private string GetContinueKeyText()
	{
		if (Input.IsGamepadActive)
		{
			GameTexts.SetVariable("CONSOLE_KEY_NAME", GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericPanelGameKeyCategory", "Exit"));
			return ((object)GameTexts.FindText("str_click_to_exit_console", (string)null)).ToString();
		}
		return ((object)GameTexts.FindText("str_click_to_exit", (string)null)).ToString();
	}

	private void OnLogout()
	{
		GauntletChatLogView current = GauntletChatLogView.Current;
		if (current != null)
		{
			current.SetEnabled(false);
		}
	}

	private void SetNavigationRestriction(bool isRestricted)
	{
		if (_isNavigationRestricted != isRestricted)
		{
			_isNavigationRestricted = isRestricted;
		}
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		TickInternal(dt);
	}

	private void TickInternal(float dt)
	{
		_lobbyDataSource?.OnTick(dt);
		if (_activeFeedbackId == null && _feedbackInquiries.Count > 0)
		{
			ShowNextFeedback();
		}
		if (_lobbyLayer == null)
		{
			return;
		}
		MPLobbyVM lobbyDataSource = _lobbyDataSource;
		if (lobbyDataSource != null)
		{
			MPOptionsVM options = lobbyDataSource.Options;
			if (((options != null) ? new bool?(options.IsEnabled) : ((bool?)null)) == true)
			{
				MPOptionsVM options2 = _lobbyDataSource.Options;
				KeybindingPopup keybindingPopup = _keybindingPopup;
				options2.AreHotkeysEnabled = (keybindingPopup == null || !keybindingPopup.IsActive) && !((ScreenLayer)_lobbyLayer).IsFocusedOnInput() && !InformationManager.IsAnyInquiryActive() && _lobbyDataSource.HasNoPopupOpen();
			}
		}
		KeybindingPopup keybindingPopup2 = _keybindingPopup;
		if (keybindingPopup2 != null && keybindingPopup2.IsActive)
		{
			_keybindingPopup.Tick();
		}
		else if (_lobbyDataSource != null && !_lobbyState.IsLoggingIn && !_lobbyDataSource.BlockerState.IsEnabled && !((ScreenLayer)_lobbyLayer).IsFocusedOnInput())
		{
			HandleInput(dt);
		}
	}

	private void HandleInput(float dt)
	{
		bool flag = ((ScreenLayer)_lobbyLayer).Input.IsHotKeyReleased("Confirm");
		bool flag2 = ((ScreenLayer)_lobbyLayer).Input.IsHotKeyReleased("ToggleFriendsList");
		if (flag || flag2)
		{
			if (_lobbyDataSource.Login.IsEnabled && flag)
			{
				_lobbyDataSource.Login.ExecuteLogin();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			else if (_lobbyDataSource.Options.IsEnabled && _lobbyDataSource.Options.AreHotkeysEnabled && flag)
			{
				_lobbyDataSource.Options.ExecuteApply();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
			else if (!_lobbyDataSource.HasNoPopupOpen() && flag)
			{
				_lobbyDataSource.OnConfirm();
			}
			else if (flag2)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_lobbyDataSource.Friends.IsListEnabled = !_lobbyDataSource.Friends.IsListEnabled;
				_lobbyDataSource.ForceCloseContextMenus();
			}
		}
		else if (((ScreenLayer)_lobbyLayer).Input.IsHotKeyReleased("Exit"))
		{
			_lobbyDataSource.OnEscape();
		}
		else if (((ScreenLayer)_lobbyLayer).Input.IsHotKeyPressed("TakeAll"))
		{
			if (_lobbyDataSource.RankLeaderboard.IsEnabled)
			{
				if (_lobbyDataSource.RankLeaderboard.IsPreviousPageAvailable)
				{
					UISoundsHelper.PlayUISound("event:/ui/checkbox");
				}
				_lobbyDataSource.RankLeaderboard.ExecuteLoadFirstPage();
			}
			else if (_lobbyDataSource.Armory.IsEnabled)
			{
				if (Input.IsGamepadActive && _lobbyDataSource.Armory.IsManagingTaunts)
				{
					_lobbyDataSource.Armory.ExecuteEmptyFocusedSlot();
				}
			}
			else if (_lobbyDataSource.Options.IsEnabled && _lobbyDataSource.Options.AreHotkeysEnabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				((OptionsVM)_lobbyDataSource.Options).SelectPreviousCategory();
			}
		}
		else if (((ScreenLayer)_lobbyLayer).Input.IsHotKeyPressed("GiveAll"))
		{
			if (_lobbyDataSource.RankLeaderboard.IsEnabled)
			{
				if (_lobbyDataSource.RankLeaderboard.IsNextPageAvailable)
				{
					UISoundsHelper.PlayUISound("event:/ui/checkbox");
				}
				_lobbyDataSource.RankLeaderboard.ExecuteLoadLastPage();
			}
			else if (_lobbyDataSource.Armory.IsEnabled)
			{
				if (Input.IsGamepadActive && _lobbyDataSource.Armory.IsManagingTaunts)
				{
					_lobbyDataSource.Armory.ExecuteSelectFocusedSlot();
				}
			}
			else if (_lobbyDataSource.Options.IsEnabled && _lobbyDataSource.Options.AreHotkeysEnabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				((OptionsVM)_lobbyDataSource.Options).SelectNextCategory();
			}
		}
		else if (((ScreenLayer)_lobbyLayer).Input.IsHotKeyPressed("SwitchToPreviousTab"))
		{
			if (_lobbyDataSource.RankLeaderboard.IsEnabled)
			{
				if (_lobbyDataSource.RankLeaderboard.IsPreviousPageAvailable)
				{
					UISoundsHelper.PlayUISound("event:/ui/checkbox");
				}
				_lobbyDataSource.RankLeaderboard.ExecuteLoadPreviousPage();
			}
			else if (_lobbyDataSource.BadgeProgressionInformation.IsEnabled)
			{
				if (_lobbyDataSource.BadgeProgressionInformation.CanDecreaseBadgeIndices)
				{
					UISoundsHelper.PlayUISound("event:/ui/checkbox");
					_lobbyDataSource.BadgeProgressionInformation.ExecuteDecreaseActiveBadgeIndices();
				}
			}
			else if (!_isNavigationRestricted)
			{
				SelectPreviousPage();
			}
		}
		else if (((ScreenLayer)_lobbyLayer).Input.IsHotKeyPressed("SwitchToNextTab"))
		{
			if (_lobbyDataSource.RankLeaderboard.IsEnabled)
			{
				if (_lobbyDataSource.RankLeaderboard.IsNextPageAvailable)
				{
					UISoundsHelper.PlayUISound("event:/ui/checkbox");
				}
				_lobbyDataSource.RankLeaderboard.ExecuteLoadNextPage();
			}
			else if (_lobbyDataSource.BadgeProgressionInformation.IsEnabled)
			{
				if (_lobbyDataSource.BadgeProgressionInformation.CanIncreaseBadgeIndices)
				{
					UISoundsHelper.PlayUISound("event:/ui/checkbox");
					_lobbyDataSource.BadgeProgressionInformation.ExecuteIncreaseActiveBadgeIndices();
				}
			}
			else if (!_isNavigationRestricted)
			{
				SelectNextPage();
			}
		}
		else if (((ScreenLayer)_lobbyLayer).Input.IsHotKeyReleased("Reset"))
		{
			if (_lobbyDataSource.HasNoPopupOpen() && _lobbyDataSource.Options.IsEnabled && _lobbyDataSource.Options.AreHotkeysEnabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_lobbyDataSource.Options.ExecuteCancel();
			}
			else if (_lobbyDataSource.Matchmaking.CustomServer.IsEnabled && !_lobbyDataSource.Matchmaking.CustomServer.IsCreateGamePanelActive && !_lobbyDataSource.Matchmaking.CustomServer.IsRefreshing)
			{
				_lobbyDataSource.Matchmaking.CustomServer.ExecuteRefresh();
			}
		}
	}

	private void ShowNextFeedback()
	{
		KeyValuePair<string, InquiryData> item = _feedbackInquiries[0];
		_feedbackInquiries.Remove(item);
		_activeFeedbackId = item.Key;
		InformationManager.ShowInquiry(item.Value, false, false);
	}

	[Conditional("DEBUG")]
	private void TickDebug(float dt)
	{
	}

	void ILobbyStateHandler.SetConnectionState(bool isAuthenticated)
	{
		if (_lobbyDataSource == null)
		{
			CreateView();
		}
		if (isAuthenticated && _lobbyState.LobbyClient.PlayerData != null)
		{
			_lobbyDataSource.RefreshPlayerData(_lobbyState.LobbyClient.PlayerData);
			if (_lobbyDataSource.CurrentPage == MPLobbyVM.LobbyPage.NotAssigned || _lobbyDataSource.CurrentPage == MPLobbyVM.LobbyPage.Authentication)
			{
				_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Home);
			}
		}
		else
		{
			if (_isFacegenOpen)
			{
				OnForceCloseFacegen();
			}
			_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Authentication);
		}
		_lobbyDataSource.ConnectionStateUpdated(isAuthenticated);
	}

	void ILobbyStateHandler.OnRequestedToSearchBattle()
	{
		_musicSoundEvent.SetParameter("mpMusicSwitcher", 1f);
		_lobbyDataSource?.OnRequestedToSearchBattle();
	}

	void ILobbyStateHandler.OnUpdateFindingGame(MatchmakingWaitTimeStats matchmakingWaitTimeStats, string[] gameTypeInfo)
	{
		_lobbyDataSource?.OnUpdateFindingGame(matchmakingWaitTimeStats, gameTypeInfo);
	}

	void ILobbyStateHandler.OnRequestedToCancelSearchBattle()
	{
		_lobbyDataSource?.OnRequestedToCancelSearchBattle();
	}

	void ILobbyStateHandler.OnSearchBattleCanceled()
	{
		_musicSoundEvent.SetParameter("mpMusicSwitcher", 0f);
		_lobbyDataSource?.OnSearchBattleCanceled();
	}

	void ILobbyStateHandler.OnPause()
	{
	}

	void ILobbyStateHandler.OnResume()
	{
		_lobbyDataSource?.RefreshPlayerData(_lobbyState.LobbyClient.PlayerData);
	}

	void ILobbyStateHandler.OnDisconnected()
	{
		_lobbyDataSource?.OnDisconnected();
	}

	void ILobbyStateHandler.OnPlayerDataReceived(PlayerData playerData)
	{
		_lobbyDataSource?.RefreshPlayerData(playerData);
		GauntletChatLogView current = GauntletChatLogView.Current;
		if (current != null)
		{
			current.OnSupportedFeaturesReceived(_lobbyState.LobbyClient.SupportedFeatures);
		}
	}

	void ILobbyStateHandler.OnPendingRejoin()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Rejoin);
	}

	void ILobbyStateHandler.OnEnterBattleWithParty(string[] selectedGameTypes)
	{
	}

	void ILobbyStateHandler.OnPartyInvitationReceived(PlayerId playerID)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (_lobbyState.LobbyClient.SupportedFeatures.SupportsFeatures((Features)4))
		{
			_lobbyDataSource?.PartyInvitationPopup.OpenWith(playerID);
		}
		else
		{
			_lobbyState.LobbyClient.DeclinePartyInvitation();
		}
	}

	void ILobbyStateHandler.OnPartyJoinRequestReceived(PlayerId joingPlayerId, PlayerId viaPlayerId, string viaPlayerName, bool newParty)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_lobbyState.LobbyClient.SupportedFeatures.SupportsFeatures((Features)4))
		{
			if (_lobbyDataSource != null)
			{
				if (newParty)
				{
					_lobbyDataSource.PartyJoinRequestPopup.OpenWithNewParty(joingPlayerId);
				}
				else
				{
					_lobbyDataSource.PartyJoinRequestPopup.OpenWith(joingPlayerId, viaPlayerId, viaPlayerName);
				}
			}
		}
		else
		{
			_lobbyState.LobbyClient.DeclinePartyJoinRequest(joingPlayerId, (PartyJoinDeclineReason)0);
		}
	}

	void ILobbyStateHandler.OnPartyInvitationInvalidated()
	{
		_lobbyDataSource?.PartyInvitationPopup.Close();
	}

	void ILobbyStateHandler.OnPlayerInvitedToParty(PlayerId playerId)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource?.Friends.OnPlayerInvitedToParty(playerId);
	}

	void ILobbyStateHandler.OnPlayerAddedToParty(PlayerId playerId, string playerName, bool isPartyLeader)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource?.OnPlayerAddedToParty(playerId);
	}

	void ILobbyStateHandler.OnPlayerRemovedFromParty(PlayerId playerId, PartyRemoveReason reason)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource?.OnPlayerRemovedFromParty(playerId, reason);
	}

	void ILobbyStateHandler.OnGameClientStateChange(State state)
	{
	}

	void ILobbyStateHandler.OnAdminMessageReceived(string message)
	{
		InformationManager.AddSystemNotification(message);
	}

	public void OnBattleServerInformationReceived(BattleServerInformationForClient battleServerInformation)
	{
		UISoundsHelper.PlayUISound("event:/ui/multiplayer/match_ready");
		_lobbyDataSource.Matchmaking.IsFindingMatch = false;
	}

	string ILobbyStateHandler.ShowFeedback(string title, string feedbackText)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		string id = Guid.NewGuid().ToString();
		InquiryData value = new InquiryData(title, feedbackText, false, true, "", ((object)new TextObject("{=dismissnotification}Dismiss", (Dictionary<string, object>)null)).ToString(), (Action)null, (Action)delegate
		{
			((ILobbyStateHandler)this).DismissFeedback(id);
		}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
		_feedbackInquiries.Add(new KeyValuePair<string, InquiryData>(id, value));
		return id;
	}

	string ILobbyStateHandler.ShowFeedback(InquiryData inquiryData)
	{
		string text = Guid.NewGuid().ToString();
		_feedbackInquiries.Add(new KeyValuePair<string, InquiryData>(text, inquiryData));
		return text;
	}

	void ILobbyStateHandler.DismissFeedback(string feedbackId)
	{
		if (_activeFeedbackId != null && _activeFeedbackId.Equals(feedbackId))
		{
			InformationManager.HideInquiry();
			_activeFeedbackId = null;
			return;
		}
		KeyValuePair<string, InquiryData> item = _feedbackInquiries.FirstOrDefault((KeyValuePair<string, InquiryData> q) => q.Key.Equals(feedbackId));
		if (item.Key != null)
		{
			_feedbackInquiries.Remove(item);
		}
	}

	private void SelectPreviousPage(MPLobbyVM.LobbyPage currentPage = MPLobbyVM.LobbyPage.NotAssigned)
	{
		MPLobbyVM lobbyDataSource = _lobbyDataSource;
		if (lobbyDataSource == null || !lobbyDataSource.HasNoPopupOpen())
		{
			return;
		}
		if (currentPage == MPLobbyVM.LobbyPage.NotAssigned)
		{
			currentPage = _lobbyDataSource.CurrentPage;
		}
		int num;
		switch (currentPage)
		{
		default:
			return;
		case MPLobbyVM.LobbyPage.Home:
		case MPLobbyVM.LobbyPage.Armory:
		case MPLobbyVM.LobbyPage.Matchmaking:
		case MPLobbyVM.LobbyPage.Profile:
			num = (int)(currentPage - 1);
			break;
		case MPLobbyVM.LobbyPage.Options:
			num = 7;
			break;
		}
		MPLobbyVM.LobbyPage lobbyPage = (MPLobbyVM.LobbyPage)num;
		if (_lobbyDataSource.DisallowedPages.Contains(lobbyPage))
		{
			SelectPreviousPage(lobbyPage);
			return;
		}
		if (lobbyPage == MPLobbyVM.LobbyPage.Options)
		{
			UISoundsHelper.PlayUISound("event:/ui/checkbox");
		}
		else
		{
			UISoundsHelper.PlayUISound("event:/ui/tab");
		}
		_lobbyDataSource.SetPage(lobbyPage);
	}

	private void SelectNextPage(MPLobbyVM.LobbyPage currentPage = MPLobbyVM.LobbyPage.NotAssigned)
	{
		MPLobbyVM lobbyDataSource = _lobbyDataSource;
		if (lobbyDataSource == null || !lobbyDataSource.HasNoPopupOpen())
		{
			return;
		}
		if (currentPage == MPLobbyVM.LobbyPage.NotAssigned)
		{
			currentPage = _lobbyDataSource.CurrentPage;
		}
		int num;
		switch (currentPage)
		{
		default:
			return;
		case MPLobbyVM.LobbyPage.Options:
		case MPLobbyVM.LobbyPage.Home:
		case MPLobbyVM.LobbyPage.Armory:
		case MPLobbyVM.LobbyPage.Matchmaking:
			num = (int)(currentPage + 1);
			break;
		case MPLobbyVM.LobbyPage.Profile:
			num = 3;
			break;
		}
		MPLobbyVM.LobbyPage lobbyPage = (MPLobbyVM.LobbyPage)num;
		if (_lobbyDataSource.DisallowedPages.Contains(lobbyPage))
		{
			SelectNextPage(lobbyPage);
			return;
		}
		if (lobbyPage == MPLobbyVM.LobbyPage.Options)
		{
			UISoundsHelper.PlayUISound("event:/ui/checkbox");
		}
		else
		{
			UISoundsHelper.PlayUISound("event:/ui/tab");
		}
		_lobbyDataSource.SetPage(lobbyPage);
	}

	void ILobbyStateHandler.OnActivateCustomServer()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Matchmaking, MPMatchmakingVM.MatchmakingSubPages.CustomGameList);
	}

	void ILobbyStateHandler.OnActivateHome()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Home);
	}

	void ILobbyStateHandler.OnActivateMatchmaking()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Matchmaking);
	}

	void ILobbyStateHandler.OnActivateArmory()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Armory);
	}

	void ILobbyStateHandler.OnActivateProfile()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Profile);
	}

	void ILobbyStateHandler.OnClanInvitationReceived(string clanName, string clanTag, bool isCreation)
	{
		_lobbyDataSource.ClanInvitationPopup.Open(clanName, clanTag, isCreation);
	}

	void ILobbyStateHandler.OnClanInvitationAnswered(PlayerId playerId, ClanCreationAnswer answer)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource?.ClanCreationPopup.UpdateConfirmation(playerId, answer);
		_lobbyDataSource?.ClanInvitationPopup.UpdateConfirmation(playerId, answer);
	}

	void ILobbyStateHandler.OnClanCreationSuccessful()
	{
		_lobbyDataSource?.OnClanCreationFinished();
	}

	void ILobbyStateHandler.OnClanCreationFailed()
	{
		_lobbyDataSource?.OnClanCreationFinished();
	}

	void ILobbyStateHandler.OnClanCreationStarted()
	{
		_lobbyDataSource.ClanCreationPopup.ExecuteSwitchToWaiting();
	}

	void ILobbyStateHandler.OnClanInfoChanged()
	{
		_lobbyDataSource?.OnClanInfoChanged();
	}

	void ILobbyStateHandler.OnPremadeGameEligibilityStatusReceived(bool isEligible)
	{
		_lobbyDataSource?.Matchmaking.OnPremadeGameEligibilityStatusReceived(isEligible);
	}

	void ILobbyStateHandler.OnPremadeGameCreated()
	{
		_lobbyDataSource?.OnPremadeGameCreated();
	}

	void ILobbyStateHandler.OnPremadeGameListReceived()
	{
		LobbyClient gameClient = NetworkMain.GameClient;
		object obj;
		if (gameClient == null)
		{
			obj = null;
		}
		else
		{
			PremadeGameList availablePremadeGames = gameClient.AvailablePremadeGames;
			obj = ((availablePremadeGames != null) ? availablePremadeGames.PremadeGameEntries : null);
		}
		PremadeGameEntry[] array = (PremadeGameEntry[])obj;
		if (array != null)
		{
			_lobbyDataSource?.Matchmaking.PremadeMatches.SetPremadeGameList(array);
		}
	}

	void ILobbyStateHandler.OnPremadeGameCreationCancelled()
	{
		_musicSoundEvent.SetParameter("mpMusicSwitcher", 0f);
		_lobbyDataSource?.OnSearchBattleCanceled();
	}

	void ILobbyStateHandler.OnJoinPremadeGameRequested(string clanName, string clanSigilCode, Guid partyId, PlayerId[] challengerPlayerIDs, PlayerId challengerPartyLeaderID, PremadeGameType premadeGameType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource.ClanMatchmakingRequestPopup.OpenWith(clanName, clanSigilCode, partyId, challengerPlayerIDs, challengerPartyLeaderID, premadeGameType);
	}

	void ILobbyStateHandler.OnJoinPremadeGameRequestSuccessful()
	{
		if (!_lobbyDataSource.GameSearch.IsEnabled)
		{
			_lobbyDataSource.OnPremadeGameCreated();
		}
		_lobbyDataSource.GameSearch.OnJoinPremadeGameRequestSuccessful();
	}

	void ILobbyStateHandler.OnSigilChanged()
	{
		if (_lobbyDataSource != null)
		{
			_lobbyDataSource.OnSigilChanged(_lobbyDataSource.ChangeSigilPopup.SelectedSigil.IconID);
		}
	}

	void ILobbyStateHandler.OnActivateOptions()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Options);
	}

	void ILobbyStateHandler.OnDeactivateOptions()
	{
		_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Home);
	}

	void ILobbyStateHandler.OnCustomGameServerListReceived(AvailableCustomGames customGameServerList)
	{
		_lobbyDataSource.Matchmaking.CustomServer.SetCustomGameServerList(customGameServerList);
	}

	void ILobbyStateHandler.OnMatchmakerGameOver(int oldExperience, int newExperience, List<string> badgesEarned, int lootGained, RankBarInfo oldRankBarInfo, RankBarInfo newRankBarInfo, BattleCancelReason battleCancelReason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0044: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if ((int)battleCancelReason == 0)
		{
			_lobbyDataSource?.AfterBattlePopup.OpenWith(oldExperience, newExperience, badgesEarned, lootGained, oldRankBarInfo, newRankBarInfo);
		}
		else if ((int)battleCancelReason == 1)
		{
			TextObject val = new TextObject("{=CtMEl2NP}Game is cancelled", (Dictionary<string, object>)null);
			TextObject val2 = new TextObject("{=A6OFgTIU}Game is cancelled due to a player leaving during warmup.", (Dictionary<string, object>)null);
			InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), false, true, "", ((object)GameTexts.FindText("str_dismiss", (string)null)).ToString(), (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
	}

	void ILobbyStateHandler.OnBattleServerLost()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		TextObject title = new TextObject("{=wLpJEkKY}Battle Server Crashed", (Dictionary<string, object>)null);
		TextObject message = new TextObject("{=EzeFJo65}You have been disconnected from server!", (Dictionary<string, object>)null);
		_lobbyDataSource.QueryPopup.ShowMessage(title, message);
	}

	void ILobbyStateHandler.OnRemovedFromMatchmakerGame(DisconnectType disconnectType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ShowDisconnectMessage(disconnectType);
	}

	void ILobbyStateHandler.OnRemovedFromCustomGame(DisconnectType disconnectType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ShowDisconnectMessage(disconnectType);
	}

	void ILobbyStateHandler.OnPlayerAssignedPartyLeader(PlayerId partyLeaderId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource?.OnPlayerAssignedPartyLeader(partyLeaderId);
	}

	void ILobbyStateHandler.OnPlayerSuggestedToParty(PlayerId playerId, string playerName, PlayerId suggestingPlayerId, string suggestingPlayerName)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_lobbyDataSource?.OnPlayerSuggestedToParty(playerId, playerName, suggestingPlayerId, suggestingPlayerName);
	}

	void ILobbyStateHandler.OnNotificationsReceived(LobbyNotification[] notifications)
	{
		_lobbyDataSource?.OnNotificationsReceived(notifications);
	}

	void ILobbyStateHandler.OnJoinCustomGameFailureResponse(CustomGameJoinResponse response)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected I4, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		TextObject message = new TextObject("{=4mMySbxI}Unspecified error", (Dictionary<string, object>)null);
		switch (response - 1)
		{
		case 1:
			message = new TextObject("{=IzJ7f5SQ}Server capacity is full", (Dictionary<string, object>)null);
			break;
		case 2:
			message = new TextObject("{=vkpMgobZ}Game server error", (Dictionary<string, object>)null);
			break;
		case 3:
			message = new TextObject("{=JQVixeQs}Couldn't access game server", (Dictionary<string, object>)null);
			break;
		case 4:
			message = new TextObject("{=T8IniCKU}Game server is not available", (Dictionary<string, object>)null);
			break;
		case 5:
			message = new TextObject("{=KRNdlbkq}Custom game is ending", (Dictionary<string, object>)null);
			break;
		case 6:
			message = new TextObject("{=Mm1Kb1bS}Incorrect password", (Dictionary<string, object>)null);
			break;
		case 7:
			message = new TextObject("{=srAJw3Tg}Player is banned from server", (Dictionary<string, object>)null);
			break;
		case 12:
			message = new TextObject("{=ivKntfNA}Already requested to join, waiting for server response", (Dictionary<string, object>)null);
			break;
		case 14:
			message = new TextObject("{=tlsmbvQX}Not all players are ready to join", (Dictionary<string, object>)null);
			break;
		case 0:
			message = new TextObject("{=KO2adj2I}You need to be in Lobby to join a custom game", (Dictionary<string, object>)null);
			break;
		case 13:
			message = new TextObject("{=KQrpWV1n}You need be the party leader to join a custom game", (Dictionary<string, object>)null);
			break;
		case 15:
			message = new TextObject("{=LCzAvLUB}Not all players' modules match with the server", (Dictionary<string, object>)null);
			break;
		}
		TextObject title = new TextObject("{=mO9bh5sy}Couldn't join custom game", (Dictionary<string, object>)null);
		_lobbyDataSource.QueryPopup.ShowMessage(title, message);
	}

	void ILobbyStateHandler.OnServerStatusReceived(ServerStatus serverStatus)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		_lobbyDataSource?.OnServerStatusReceived(serverStatus);
		if (serverStatus.Announcement != null)
		{
			if ((int)serverStatus.Announcement.Type == 1)
			{
				InformationManager.AddSystemNotification(((object)new TextObject(serverStatus.Announcement.Text, (Dictionary<string, object>)null)).ToString());
			}
			else if ((int)serverStatus.Announcement.Type == 0)
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject(serverStatus.Announcement.Text, (Dictionary<string, object>)null)).ToString()));
			}
		}
	}

	void ILobbyStateHandler.OnRejoinBattleRequestAnswered(bool isSuccessful)
	{
		_lobbyDataSource?.OnRejoinBattleRequestAnswered(isSuccessful);
	}

	void ILobbyStateHandler.OnFriendListUpdated()
	{
		_lobbyDataSource?.OnFriendListUpdated();
	}

	void ILobbyStateHandler.OnPlayerNameUpdated(string playerName)
	{
		_lobbyDataSource?.OnPlayerNameUpdated(playerName);
	}

	private void ShowDisconnectMessage(DisconnectType disconnectType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected I4, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		if ((int)disconnectType != 0 && (int)disconnectType != 7)
		{
			TextObject title = new TextObject("{=JluTW3Qw}Game Ended", (Dictionary<string, object>)null);
			TextObject message = new TextObject("{=aKjpbRP5}Unknown reason", (Dictionary<string, object>)null);
			switch (disconnectType - 1)
			{
			case 7:
				message = new TextObject("{=tKSxGy5p}Server not responding", (Dictionary<string, object>)null);
				break;
			case 2:
				message = new TextObject("{=wbFB3N72}You are kicked from game by poll", (Dictionary<string, object>)null);
				break;
			case 3:
				message = new TextObject("{=OhF7NqSb}You are banned from game by poll", (Dictionary<string, object>)null);
				break;
			case 4:
				message = new TextObject("{=074YAjOk}You are kicked due to inactivity", (Dictionary<string, object>)null);
				break;
			case 1:
				message = new TextObject("{=a0IHtkoa}You are kicked by game host", (Dictionary<string, object>)null);
				break;
			case 0:
				message = new TextObject("{=WvGviFgt}Your connection with the server timed out", (Dictionary<string, object>)null);
				break;
			case 8:
				message = new TextObject("{=InUAmnX4}You are kicked due to friendly damage", (Dictionary<string, object>)null);
				break;
			case 9:
				message = new TextObject("{=O1bGoaE8}Server state could not be retrieved. Please try again.", (Dictionary<string, object>)null);
				break;
			}
			_lobbyDataSource.QueryPopup.ShowMessage(title, message);
		}
	}

	private void DisableLobby()
	{
		if (_isLobbyActive)
		{
			_isLobbyActive = false;
			SpriteCategory mplobbyCategory = _mplobbyCategory;
			if (mplobbyCategory != null)
			{
				mplobbyCategory.Unload();
			}
			SpriteCategory bannerIconsCategory = _bannerIconsCategory;
			if (bannerIconsCategory != null)
			{
				bannerIconsCategory.Unload();
			}
			((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_lobbyLayer);
			_lobbyDataSource = null;
			_lobbyLayer = null;
		}
	}

	private void OnKeybindRequest(KeyOptionVM requestedHotKeyToChange)
	{
		_currentKey = requestedHotKeyToChange;
		_keybindingPopup.OnToggle(true);
	}

	private void SetHotKey(Key key)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		if (key.IsControllerInput)
		{
			MBInformationManager.AddQuickInformation(new TextObject("{=B41vvGuo}Invalid key", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			_keybindingPopup.OnToggle(false);
			return;
		}
		GameKeyOptionVM gameKey = default(GameKeyOptionVM);
		ref GameKeyOptionVM reference = ref gameKey;
		KeyOptionVM currentKey = _currentKey;
		if ((reference = (GameKeyOptionVM)(object)((currentKey is GameKeyOptionVM) ? currentKey : null)) != null)
		{
			MPLobbyVM lobbyDataSource = _lobbyDataSource;
			if (((lobbyDataSource != null) ? ((IEnumerable<GameKeyGroupVM>)((OptionsVM)lobbyDataSource.Options).GameKeyOptionGroups.GameKeyGroups).FirstOrDefault((Func<GameKeyGroupVM, bool>)((GameKeyGroupVM g) => ((Collection<GameKeyOptionVM>)(object)g.GameKeys).Contains(gameKey))) : null) == null)
			{
				Debug.FailedAssert("Could not find GameKeyGroup during SetHotKey", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.GauntletUI\\MultiplayerLobbyGauntletScreen.cs", "SetHotKey", 1295);
				MBInformationManager.AddQuickInformation(new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
				_keybindingPopup.OnToggle(false);
				return;
			}
			if (key.InputKey != ((KeyOptionVM)gameKey).CurrentKey.InputKey)
			{
				GauntletLayer lobbyLayer = _lobbyLayer;
				if (lobbyLayer == null || !((ScreenLayer)lobbyLayer).Input.IsHotKeyReleased("Exit"))
				{
					GameKeyOptionVM obj = gameKey;
					if (obj != null)
					{
						((KeyOptionVM)obj).Set(key.InputKey);
					}
					gameKey = null;
					_keybindingPopup.OnToggle(false);
					goto IL_0222;
				}
			}
			_keybindingPopup.OnToggle(false);
		}
		else
		{
			AuxiliaryKeyOptionVM auxiliaryKey = default(AuxiliaryKeyOptionVM);
			ref AuxiliaryKeyOptionVM reference2 = ref auxiliaryKey;
			KeyOptionVM currentKey2 = _currentKey;
			if ((reference2 = (AuxiliaryKeyOptionVM)(object)((currentKey2 is AuxiliaryKeyOptionVM) ? currentKey2 : null)) != null)
			{
				if (((IEnumerable<AuxiliaryKeyGroupVM>)((OptionsVM)_lobbyDataSource.Options).GameKeyOptionGroups.AuxiliaryKeyGroups).FirstOrDefault((Func<AuxiliaryKeyGroupVM, bool>)((AuxiliaryKeyGroupVM g) => ((Collection<AuxiliaryKeyOptionVM>)(object)g.HotKeys).Contains(auxiliaryKey))) == null)
				{
					Debug.FailedAssert("Could not find AuxiliaryKeyGroup during SetHotKey", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.GauntletUI\\MultiplayerLobbyGauntletScreen.cs", "SetHotKey", 1318);
					MBInformationManager.AddQuickInformation(new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
					_keybindingPopup.OnToggle(false);
					return;
				}
				if (key.InputKey != ((KeyOptionVM)auxiliaryKey).CurrentKey.InputKey)
				{
					GauntletLayer lobbyLayer2 = _lobbyLayer;
					if (lobbyLayer2 == null || !((ScreenLayer)lobbyLayer2).Input.IsHotKeyReleased("Exit"))
					{
						AuxiliaryKeyOptionVM obj2 = auxiliaryKey;
						if (obj2 != null)
						{
							((KeyOptionVM)obj2).Set(key.InputKey);
						}
						auxiliaryKey = null;
						_keybindingPopup.OnToggle(false);
						goto IL_0222;
					}
				}
				_keybindingPopup.OnToggle(false);
			}
		}
		goto IL_0222;
		IL_0222:
		MPLobbyVM lobbyDataSource2 = _lobbyDataSource;
		if (lobbyDataSource2 == null)
		{
			return;
		}
		MPOptionsVM options = lobbyDataSource2.Options;
		if (options != null)
		{
			GameKeyOptionCategoryVM gameKeyOptionGroups = ((OptionsVM)options).GameKeyOptionGroups;
			if (gameKeyOptionGroups != null)
			{
				((ViewModel)gameKeyOptionGroups).RefreshValues();
			}
		}
	}

	void IGameStateListener.OnActivate()
	{
		if (_lobbyDataSource == null)
		{
			CreateView();
			_lobbyDataSource.SetPage(MPLobbyVM.LobbyPage.Authentication);
		}
		else
		{
			_optionsSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_options");
			_multiplayerSpriteCategory = UIResourceManager.LoadSpriteCategory("ui_mpmission");
		}
	}

	void IGameStateListener.OnDeactivate()
	{
		if (_lobbyDataSource != null)
		{
			MPCustomGameSortControllerVM sortController = _lobbyDataSource.Matchmaking.CustomServer.SortController;
			_cachedCustomServerSortOption = sortController.CurrentSortOption;
			_cachedCustomServerSortState = (MPCustomGameSortControllerVM.SortState)sortController.CurrentSortState;
			MPCustomGameSortControllerVM sortController2 = _lobbyDataSource.Matchmaking.PremadeMatches.SortController;
			_cachedPremadeGameSortOption = sortController2.CurrentSortOption;
			_cachedPremadeGameSortState = (MPCustomGameSortControllerVM.SortState)sortController2.CurrentSortState;
		}
		if (((GameState)_lobbyState).GameStateManager.LastOrDefault<LobbyPracticeState>() == null)
		{
			((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_lobbyLayer);
			if (_lobbyDataSource != null)
			{
				((ViewModel)_lobbyDataSource).OnFinalize();
				_lobbyDataSource = null;
			}
			_lobbyLayer = null;
			SpriteCategory mplobbyCategory = _mplobbyCategory;
			if (mplobbyCategory != null)
			{
				mplobbyCategory.Unload();
			}
			SpriteCategory bannerIconsCategory = _bannerIconsCategory;
			if (bannerIconsCategory != null)
			{
				bannerIconsCategory.Unload();
			}
			_musicSoundEvent.Stop();
			_musicSoundEvent = null;
		}
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}
}
