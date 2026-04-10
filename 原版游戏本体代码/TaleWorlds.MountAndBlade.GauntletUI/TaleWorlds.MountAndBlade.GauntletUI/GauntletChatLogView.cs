using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletChatLogView : GlobalLayer
{
	private MPChatVM _dataSource;

	private ChatLogMessageManager _chatLogMessageManager;

	private bool _canFocusWhileInMission = true;

	private bool _isTeamChatAvailable;

	private GauntletMovieIdentifier _movie;

	private bool _isEnabled = true;

	private const int MaxHistoryCountForSingleplayer = 250;

	private const int MaxHistoryCountForMultiplayer = 100;

	public static GauntletChatLogView Current { get; private set; }

	public GauntletChatLogView()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		_dataSource = new MPChatVM();
		_dataSource.SetGetKeyTextFromKeyIDFunc((Func<TextObject>)GetToggleChatKeyText);
		_dataSource.SetGetCycleChannelKeyTextFunc((Func<TextObject>)GetCycleChannelsKeyText);
		_dataSource.SetGetSendMessageKeyTextFunc((Func<TextObject>)GetSendMessageKeyText);
		_dataSource.SetGetCancelSendingKeyTextFunc((Func<TextObject>)GetCancelSendingKeyText);
		_dataSource.SetChatDisabledStateChangedCallback((Action<bool>)OnChatDisabledStateChanged);
		GauntletLayer val = new GauntletLayer("ChatLog", 15300, false);
		_movie = val.LoadMovie("SPChatLog", (ViewModel)(object)_dataSource);
		((ScreenLayer)val).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)val).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)val).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ChatLogHotKeyCategory"));
		((GlobalLayer)this).Layer = (ScreenLayer)(object)val;
		_chatLogMessageManager = new ChatLogMessageManager(_dataSource);
		MessageManager.SetMessageManager((MessageManagerBase)(object)_chatLogMessageManager);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionsChanged));
	}

	public static void Initialize()
	{
		if (Current == null)
		{
			Current = new GauntletChatLogView();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
		}
	}

	private void OnManagedOptionsChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		bool num = (int)changedManagedOptionsType == 45 && Mission.Current != null && BannerlordConfig.HideBattleUI;
		bool flag = (int)changedManagedOptionsType == 47 && !GameNetwork.IsMultiplayer && !BannerlordConfig.EnableSingleplayerChatBox;
		bool flag2 = (int)changedManagedOptionsType == 48 && GameNetwork.IsMultiplayer && !BannerlordConfig.EnableMultiplayerChatBox;
		if (num || flag || flag2)
		{
			_dataSource.Clear();
			CloseChat();
		}
	}

	private void CloseChat()
	{
		if (_dataSource.IsTypingText || _dataSource.IsInspectingMessages || ((GlobalLayer)this).Layer.IsFocusLayer)
		{
			if (_dataSource.IsInspectingMessages)
			{
				_dataSource.StopInspectingMessages();
			}
			else if (_dataSource.IsTypingText)
			{
				_dataSource.StopTyping(true);
			}
			UpdateFocusLayer();
		}
	}

	protected override void OnTick(float dt)
	{
		((GlobalLayer)this).OnTick(dt);
		if (_dataSource.IsChatAllowedByOptions())
		{
			_chatLogMessageManager.Update();
		}
		_dataSource.UpdateObjects(Game.Current, Mission.Current);
		_dataSource.Tick(dt);
		_dataSource.ShouldHaveOffset = GetShouldHaveOffset();
	}

	protected override void OnLateTick(float dt)
	{
		((GlobalLayer)this).OnLateTick(dt);
		bool chatOpened = false;
		bool chatClosed = false;
		if (!_isEnabled || _dataSource.IsChatDisabled)
		{
			MPChatVM dataSource = _dataSource;
			if (dataSource != null && dataSource.IsInspectingMessages)
			{
				chatClosed = true;
				_dataSource.StopTyping(_dataSource.IsChatDisabled);
			}
		}
		if (_isEnabled)
		{
			MPChatVM dataSource2 = _dataSource;
			if (dataSource2 != null && dataSource2.IsChatAllowedByOptions())
			{
				HandleInput(ref chatOpened, ref chatClosed);
			}
		}
		MPChatVM dataSource3 = _dataSource;
		if ((dataSource3 == null || !dataSource3.IsInspectingMessages) && ((GlobalLayer)this).Layer.InputRestrictions.MouseVisibility)
		{
			((GlobalLayer)this).Layer.InputRestrictions.SetMouseVisibility(false);
		}
		if (chatOpened || chatClosed)
		{
			OnChatOpenedOrClosed(chatOpened, chatClosed);
		}
	}

	private bool GetShouldHaveOffset()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		if (!_dataSource.IsTypingText && !_dataSource.IsInspectingMessages)
		{
			Mission current = Mission.Current;
			if (current != null && current.IsOrderMenuOpen && (int)Mission.Current.Mode != 6)
			{
				return !Input.IsGamepadActive;
			}
		}
		return false;
	}

	private void HandleInput(ref bool chatOpened, ref bool chatClosed)
	{
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Invalid comparison between Unknown and I4
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Invalid comparison between Unknown and I4
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Invalid comparison between Unknown and I4
		bool inputEnabled = false;
		bool isToggleChatHintAvailable = false;
		bool flag = true;
		bool isMouseVisible = true;
		InputContext inputContext = null;
		_isTeamChatAvailable = true;
		if (ScreenManager.TopScreen is IChatLogHandlerScreen chatLogHandlerScreen)
		{
			chatLogHandlerScreen.TryUpdateChatLogLayerParameters(ref _isTeamChatAvailable, ref inputEnabled, ref isToggleChatHintAvailable, ref isMouseVisible, ref inputContext);
			_dataSource.ShowHideShowHint = isToggleChatHintAvailable;
		}
		if (isMouseVisible != ((GlobalLayer)this).Layer.InputRestrictions.MouseVisibility)
		{
			((GlobalLayer)this).Layer.InputRestrictions.SetMouseVisibility(isMouseVisible);
		}
		ScreenLayer focusedLayer = ScreenManager.FocusedLayer;
		GauntletLayer val;
		if ((val = (GauntletLayer)(object)((focusedLayer is GauntletLayer) ? focusedLayer : null)) != null && (object)val != ((GlobalLayer)this).Layer && val.UIContext.EventManager.FocusedWidget is EditableTextWidget)
		{
			inputEnabled = false;
		}
		if (inputEnabled)
		{
			GameKeyContext category = HotKeyManager.GetCategory("ChatLogHotKeyCategory");
			if (inputContext != null && !inputContext.IsCategoryRegistered(category))
			{
				inputContext.RegisterHotKeyCategory(category);
			}
			if (flag)
			{
				if (_dataSource.IsInspectingMessages)
				{
					if (((GlobalLayer)this).Layer.Input.IsHotKeyReleased("ToggleEscapeMenu") || ((GlobalLayer)this).Layer.Input.IsHotKeyReleased("Exit"))
					{
						bool isGamepadActive = Input.IsGamepadActive;
						_dataSource.StopTyping(isGamepadActive);
						chatClosed = true;
					}
					else if (((GlobalLayer)this).Layer.Input.IsGameKeyReleased(8) || ((GlobalLayer)this).Layer.Input.IsHotKeyReleased("FinalizeChatAlternative") || ((GlobalLayer)this).Layer.Input.IsHotKeyReleased("SendMessage"))
					{
						if ((Input.IsGamepadActive && ((GlobalLayer)this).Layer.Input.IsHotKeyReleased("SendMessage")) || !Input.IsGamepadActive)
						{
							_dataSource.SendCurrentlyTypedMessage();
						}
						_dataSource.StopTyping(false);
						chatClosed = true;
					}
					if (((GlobalLayer)this).Layer.Input.IsHotKeyReleased("CycleChatTypes"))
					{
						if ((int)_dataSource.ActiveChannelType == 2)
						{
							_dataSource.TypeToChannelAll(false);
						}
						else if ((int)_dataSource.ActiveChannelType == 1 && _isTeamChatAvailable)
						{
							_dataSource.TypeToChannelTeam(false);
						}
					}
				}
				else
				{
					if (inputContext == null)
					{
						return;
					}
					if (_canFocusWhileInMission && inputContext.IsGameKeyReleased(6))
					{
						_dataSource.TypeToChannelAll(true);
						chatOpened = true;
					}
					else if (_canFocusWhileInMission && _isTeamChatAvailable && inputContext.IsGameKeyReleased(7))
					{
						_dataSource.TypeToChannelTeam(true);
						chatOpened = true;
					}
					if (_canFocusWhileInMission && (inputContext.IsGameKeyReleased(8) || inputContext.IsHotKeyReleased("FinalizeChatAlternative")))
					{
						if ((int)_dataSource.ActiveChannelType == -1)
						{
							_dataSource.TypeToChannelAll(true);
						}
						else
						{
							_dataSource.StartTyping();
						}
						chatOpened = true;
					}
				}
			}
			else if (_canFocusWhileInMission && inputContext != null && (inputContext.IsGameKeyReleased(8) || inputContext.IsHotKeyReleased("FinalizeChatAlternative")))
			{
				if (!_dataSource.IsInspectingMessages)
				{
					_dataSource.StartInspectingMessages();
					chatOpened = true;
				}
				else
				{
					_dataSource.StopInspectingMessages();
					chatClosed = true;
				}
			}
		}
		else if (_dataSource.IsTypingText)
		{
			_dataSource.StopTyping(false);
			chatClosed = true;
		}
		else if (_dataSource.IsInspectingMessages)
		{
			_dataSource.StopInspectingMessages();
			chatClosed = true;
		}
	}

	private void OnChatOpenedOrClosed(bool chatOpened, bool chatClosed)
	{
		UpdateFocusLayer();
		if (ScreenManager.TopScreen is MissionScreen { SceneLayer: not null } missionScreen)
		{
			missionScreen.Mission.GetMissionBehavior<MissionMainAgentController>().IsChatOpen = chatOpened && !chatClosed;
		}
	}

	private void UpdateFocusLayer()
	{
		if (_dataSource.IsTypingText || _dataSource.IsInspectingMessages)
		{
			if (_dataSource.IsTypingText && !((GlobalLayer)this).Layer.IsFocusLayer)
			{
				((GlobalLayer)this).Layer.IsFocusLayer = true;
				ScreenManager.TrySetFocus(((GlobalLayer)this).Layer);
			}
			((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		}
		else
		{
			((GlobalLayer)this).Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(((GlobalLayer)this).Layer);
			((GlobalLayer)this).Layer.InputRestrictions.ResetInputRestrictions();
		}
	}

	public void SetCanFocusWhileInMission(bool canFocusInMission)
	{
		_canFocusWhileInMission = canFocusInMission;
	}

	public void OnSupportedFeaturesReceived(SupportedFeatures supportedFeatures)
	{
		SetEnabled(supportedFeatures.SupportsFeatures((Features)32));
	}

	public void SetEnabled(bool isEnabled)
	{
		if (_isEnabled != isEnabled)
		{
			_isEnabled = isEnabled;
		}
	}

	public void LoadMovie(bool forMultiplayer)
	{
		if (_movie != null)
		{
			ScreenLayer layer = ((GlobalLayer)this).Layer;
			ScreenLayer obj = ((layer is GauntletLayer) ? layer : null);
			if (obj != null)
			{
				((GauntletLayer)obj).ReleaseMovie(_movie);
			}
		}
		if (forMultiplayer)
		{
			Game current = Game.Current;
			if (current != null)
			{
				ChatBox gameHandler = current.GetGameHandler<ChatBox>();
				if (gameHandler != null)
				{
					gameHandler.InitializeForMultiplayer();
				}
			}
			ScreenLayer layer2 = ((GlobalLayer)this).Layer;
			ScreenLayer obj2 = ((layer2 is GauntletLayer) ? layer2 : null);
			_movie = ((obj2 != null) ? ((GauntletLayer)obj2).LoadMovie("MPChatLog", (ViewModel)(object)_dataSource) : null);
			_dataSource.SetMessageHistoryCapacity(100);
		}
		else
		{
			SetEnabled(isEnabled: true);
			Game current2 = Game.Current;
			if (current2 != null)
			{
				current2.GetGameHandler<ChatBox>().InitializeForSinglePlayer();
			}
			ScreenLayer layer3 = ((GlobalLayer)this).Layer;
			ScreenLayer obj3 = ((layer3 is GauntletLayer) ? layer3 : null);
			_movie = ((obj3 != null) ? ((GauntletLayer)obj3).LoadMovie("SPChatLog", (ViewModel)(object)_dataSource) : null);
			_dataSource.ChatBoxSizeX = BannerlordConfig.ChatBoxSizeX;
			_dataSource.ChatBoxSizeY = BannerlordConfig.ChatBoxSizeY;
			_dataSource.SetMessageHistoryCapacity(250);
		}
	}

	private TextObject GetToggleChatKeyText()
	{
		if (Input.IsGamepadActive)
		{
			Game current = Game.Current;
			if (current == null)
			{
				return null;
			}
			GameTextManager gameTextManager = current.GameTextManager;
			if (gameTextManager == null)
			{
				return null;
			}
			return GameKeyTextExtensions.GetHotKeyGameTextFromKeyID(gameTextManager, "controllerloption");
		}
		Game current2 = Game.Current;
		if (current2 == null)
		{
			return null;
		}
		GameTextManager gameTextManager2 = current2.GameTextManager;
		if (gameTextManager2 == null)
		{
			return null;
		}
		return GameKeyTextExtensions.GetHotKeyGameTextFromKeyID(gameTextManager2, "enter");
	}

	private TextObject GetCycleChannelsKeyText()
	{
		Game current = Game.Current;
		object obj;
		if (current == null)
		{
			obj = null;
		}
		else
		{
			GameTextManager gameTextManager = current.GameTextManager;
			obj = ((gameTextManager != null) ? GameKeyTextExtensions.GetHotKeyGameText(gameTextManager, "ChatLogHotKeyCategory", "CycleChatTypes") : null);
		}
		if (obj == null)
		{
			obj = TextObject.GetEmpty();
		}
		return (TextObject)obj;
	}

	private TextObject GetSendMessageKeyText()
	{
		Game current = Game.Current;
		object obj;
		if (current == null)
		{
			obj = null;
		}
		else
		{
			GameTextManager gameTextManager = current.GameTextManager;
			obj = ((gameTextManager != null) ? GameKeyTextExtensions.GetHotKeyGameText(gameTextManager, "ChatLogHotKeyCategory", "SendMessage") : null);
		}
		if (obj == null)
		{
			obj = TextObject.GetEmpty();
		}
		return (TextObject)obj;
	}

	private TextObject GetCancelSendingKeyText()
	{
		Game current = Game.Current;
		object obj;
		if (current == null)
		{
			obj = null;
		}
		else
		{
			GameTextManager gameTextManager = current.GameTextManager;
			obj = ((gameTextManager != null) ? GameKeyTextExtensions.GetHotKeyGameText(gameTextManager, "GenericPanelGameKeyCategory", "Exit") : null);
		}
		if (obj == null)
		{
			obj = TextObject.GetEmpty();
		}
		return (TextObject)obj;
	}

	private void OnChatDisabledStateChanged(bool chatDisabled)
	{
		if (!chatDisabled)
		{
			_dataSource.StopTyping(true);
			OnChatOpenedOrClosed(chatOpened: false, chatClosed: true);
		}
	}
}
