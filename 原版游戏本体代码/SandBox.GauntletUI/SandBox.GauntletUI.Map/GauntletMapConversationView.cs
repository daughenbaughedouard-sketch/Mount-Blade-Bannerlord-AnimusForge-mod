using System;
using System.Collections.ObjectModel;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.ViewModelCollection.Barter;
using TaleWorlds.CampaignSystem.ViewModelCollection.Conversation;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapConversation;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapConversationView))]
public class GauntletMapConversationView : MapConversationView, IConversationStateHandler
{
	private GauntletMovieIdentifier _conversationMovie;

	private GauntletLayer _layerAsGauntletLayer;

	private MapConversationVM _dataSource;

	private SpriteCategory _conversationCategory;

	private MapConversationTableauData _tableauData;

	private BarterManager _barterManager;

	private GauntletMapConversationBarterView _barterView;

	private ConversationCharacterData _playerCharacterData;

	private ConversationCharacterData _conversationPartnerData;

	private bool _isSwitchingConversations;

	private int _minimumAvailableConversationInstallFrame;

	public GauntletMapConversationView()
	{
		_barterManager = Campaign.Current.BarterManager;
		_conversationCategory = UIResourceManager.GetSpriteCategory("ui_conversation");
	}

	private void OnBarterActiveStateChanged(bool isBarterActive)
	{
		_dataSource.IsBarterActive = isBarterActive;
	}

	protected override void InitializeConversation(ConversationCharacterData playerCharacterData, ConversationCharacterData conversationPartnerData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeConversation(playerCharacterData, conversationPartnerData);
		_playerCharacterData = playerCharacterData;
		_conversationPartnerData = conversationPartnerData;
		DestroyConversationTableau();
		DestroyConversationMission();
		CreateConversationMissionIfMissing();
		if (!base.IsConversationActive)
		{
			CreateConversationView();
			CreateConversationTableau();
		}
		else
		{
			_minimumAvailableConversationInstallFrame = Utilities.EngineFrameNo + 2;
			_isSwitchingConversations = true;
		}
		base.IsConversationActive = true;
	}

	protected override void FinalizeConversation()
	{
		base.FinalizeConversation();
		DestroyConversationTableau();
		DestroyConversationView();
		DestroyConversationMission();
		_minimumAvailableConversationInstallFrame = Utilities.EngineFrameNo + 2;
		base.IsConversationActive = false;
		if (!base.MapScreen.IsReady)
		{
			LoadingWindow.EnableGlobalLoadingWindow();
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, false);
		}
		if (base.IsConversationActive)
		{
			_conversationMovie = _layerAsGauntletLayer.LoadMovie("MapConversation", (ViewModel)(object)_dataSource);
			if (_barterView.IsCreated && !_barterView.IsActive)
			{
				_barterView.Activate();
			}
			Extensions.Load(_conversationCategory);
			_dataSource.TableauData = _tableauData;
		}
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, true);
		}
		if (base.IsConversationActive)
		{
			_dataSource.TableauData = null;
			_layerAsGauntletLayer.ReleaseMovie(_conversationMovie);
			if (_barterView.IsCreated && _barterView.IsActive)
			{
				_barterView.Deactivate();
			}
			_conversationCategory.Unload();
		}
	}

	private void Tick(float dt)
	{
		if (!base.IsConversationActive || _layerAsGauntletLayer == null)
		{
			return;
		}
		if (_isSwitchingConversations)
		{
			_isSwitchingConversations = false;
		}
		if (base.IsConversationActive && (object)ScreenManager.TopScreen == base.MapScreen && ScreenManager.FocusedLayer != base.Layer)
		{
			ScreenManager.TrySetFocus(base.Layer);
		}
		MapConversationVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Tick(dt);
		}
		MapConversationVM dataSource2 = _dataSource;
		if (dataSource2 != null)
		{
			MissionConversationVM dialogController = dataSource2.DialogController;
			if (((dialogController != null) ? new int?(((Collection<ConversationItemVM>)(object)dialogController.AnswerList).Count) : ((int?)null)) <= 0 && !_barterView.IsCreated && base.IsConversationActive && ((ScreenLayer)_layerAsGauntletLayer).Input.IsHotKeyReleased("ContinueKey"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				((IConversationStateHandler)this).ExecuteConversationContinue();
			}
		}
		if (!base.IsConversationActive || _layerAsGauntletLayer == null)
		{
			return;
		}
		if (_barterView.IsCreated)
		{
			_barterView.TickInput();
		}
		else
		{
			if (base.IsConversationActive && _tableauData == null && Utilities.EngineFrameNo > _minimumAvailableConversationInstallFrame)
			{
				CreateConversationTableau();
			}
			if (!ScreenFadeController.IsFadeActive && ((ScreenLayer)_layerAsGauntletLayer).Input.IsHotKeyReleased("ToggleEscapeMenu"))
			{
				MapScreen mapScreen = base.MapScreen;
				if (mapScreen != null && mapScreen.IsEscapeMenuOpened)
				{
					base.MapScreen.CloseEscapeMenu();
				}
				else
				{
					base.MapScreen?.OpenEscapeMenu();
				}
			}
		}
		BarterItemVM.IsFiveStackModifierActive = ((ScreenLayer)_layerAsGauntletLayer).Input.IsHotKeyDown("FiveStackModifier");
		BarterItemVM.IsEntireStackModifierActive = ((ScreenLayer)_layerAsGauntletLayer).Input.IsHotKeyDown("EntireStackModifier");
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		if (base.IsConversationActive)
		{
			FinalizeConversation();
		}
	}

	private void CreateConversationView()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		base.Layer = (ScreenLayer)new GauntletLayer("MapConversation", 205, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_barterView = new GauntletMapConversationBarterView(_layerAsGauntletLayer, OnBarterActiveStateChanged);
		BarterManager barterManager = _barterManager;
		barterManager.BarterBegin = (BarterBeginEventDelegate)Delegate.Combine((Delegate?)(object)barterManager.BarterBegin, (Delegate?)new BarterBeginEventDelegate(_barterView.CreateBarterView));
		BarterManager barterManager2 = _barterManager;
		barterManager2.Closed = (BarterCloseEventDelegate)Delegate.Combine((Delegate?)(object)barterManager2.Closed, (Delegate?)new BarterCloseEventDelegate(_barterView.DestroyBarterView));
		_dataSource = new MapConversationVM((Action)OnContinue, (Func<string>)GetContinueKeyText);
		_conversationMovie = _layerAsGauntletLayer.LoadMovie("MapConversation", (ViewModel)(object)_dataSource);
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ConversationHotKeyCategory"));
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		Extensions.Load(_conversationCategory);
		Campaign.Current.ConversationManager.Handler = (IConversationStateHandler)(object)this;
		Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
	}

	private void OnContinue()
	{
		if (!base.IsConversationActive)
		{
			return;
		}
		MapConversationVM dataSource = _dataSource;
		if (dataSource != null)
		{
			MissionConversationVM dialogController = dataSource.DialogController;
			if (((dialogController != null) ? new int?(((Collection<ConversationItemVM>)(object)dialogController.AnswerList).Count) : ((int?)null)) <= 0 && !_barterView.IsCreated)
			{
				((IConversationStateHandler)this).ExecuteConversationContinue();
			}
		}
	}

	private void DestroyConversationView()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		base.Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(base.Layer);
		if (_barterView.IsCreated)
		{
			_barterView.DestroyBarterView();
		}
		((ViewModel)_dataSource).OnFinalize();
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		SpriteCategory conversationCategory = _conversationCategory;
		if (conversationCategory != null && conversationCategory.IsLoaded)
		{
			_conversationCategory.Unload();
		}
		BarterManager barterManager = _barterManager;
		barterManager.BarterBegin = (BarterBeginEventDelegate)Delegate.Remove((Delegate?)(object)barterManager.BarterBegin, (Delegate?)new BarterBeginEventDelegate(_barterView.CreateBarterView));
		BarterManager barterManager2 = _barterManager;
		barterManager2.Closed = (BarterCloseEventDelegate)Delegate.Remove((Delegate?)(object)barterManager2.Closed, (Delegate?)new BarterCloseEventDelegate(_barterView.DestroyBarterView));
		base.Layer = null;
		_layerAsGauntletLayer = null;
		_dataSource = null;
		Campaign.Current.ConversationManager.Handler = null;
		Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
	}

	protected override bool IsEscaped()
	{
		return base.IsConversationActive;
	}

	protected override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return true;
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		Tick(dt);
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		Tick(dt);
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		Tick(dt);
	}

	private void CreateConversationTableau()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Invalid comparison between Unknown and I4
		CampaignTime now = CampaignTime.Now;
		float timeOfDay = ((CampaignTime)(ref now)).CurrentHourInDay * (float)(24 / CampaignTime.HoursInDay);
		MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
		CampaignVec2 position = MobileParty.MainParty.Position;
		WeatherEvent weatherEventInPosition = mapWeatherModel.GetWeatherEventInPosition(((CampaignVec2)(ref position)).ToVec2());
		bool isCurrentTerrainUnderSnow = (int)weatherEventInPosition == 3 || (int)weatherEventInPosition == 4;
		string locationId = null;
		if (_conversationPartnerData.Character.HeroObject != null)
		{
			LocationComplex current = LocationComplex.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				Location locationOfCharacter = current.GetLocationOfCharacter(_conversationPartnerData.Character.HeroObject);
				obj = ((locationOfCharacter != null) ? locationOfCharacter.StringId : null);
			}
			locationId = (string)obj;
		}
		_tableauData = MapConversationTableauData.CreateFrom(_playerCharacterData, _conversationPartnerData, Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace), timeOfDay, isCurrentTerrainUnderSnow, Hero.MainHero.CurrentSettlement, locationId, (int)weatherEventInPosition == 2, (int)weatherEventInPosition == 4);
		_dataSource.TableauData = _tableauData;
		_layerAsGauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(1, (Func<bool>)null);
	}

	private void DestroyConversationTableau()
	{
		if (_dataSource != null)
		{
			_dataSource.TableauData = null;
		}
		_tableauData = null;
	}

	void IConversationStateHandler.OnConversationUninstall()
	{
		if (!_isSwitchingConversations)
		{
			MapState obj = Game.Current.GameStateManager.LastOrDefault<MapState>();
			if (obj != null)
			{
				obj.OnMapConversationOver();
			}
		}
	}

	private static string GetContinueKeyText()
	{
		if (Input.IsGamepadActive)
		{
			return ((object)GameTexts.FindText("str_click_to_continue_console", (string)null).SetTextVariable("CONSOLE_KEY_NAME", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("ConversationHotKeyCategory", "ContinueClick"), 1f))).ToString();
		}
		return ((object)GameTexts.FindText("str_click_to_continue", (string)null)).ToString();
	}

	void IConversationStateHandler.OnConversationInstall()
	{
	}

	void IConversationStateHandler.OnConversationActivate()
	{
	}

	void IConversationStateHandler.OnConversationDeactivate()
	{
		MBInformationManager.HideInformations();
	}

	void IConversationStateHandler.OnConversationContinue()
	{
		_dataSource.DialogController.OnConversationContinue();
	}

	void IConversationStateHandler.ExecuteConversationContinue()
	{
		_dataSource.DialogController.ExecuteContinue();
	}
}
