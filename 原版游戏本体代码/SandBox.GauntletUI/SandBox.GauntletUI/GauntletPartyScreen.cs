using System;
using SandBox.View;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party.PartyTroopManagerPopUp;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(PartyState))]
public class GauntletPartyScreen : ScreenBase, IGameStateListener, IChangeableScreen, IPartyScreenLogicHandler, IPartyScreenPrisonHandler, IPartyScreenTroopHandler
{
	private PartyVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _partyscreenCategory;

	private readonly PartyState _partyState;

	public bool IsTroopUpgradesDisabled
	{
		get
		{
			PartyVM dataSource = _dataSource;
			if (dataSource == null)
			{
				return false;
			}
			PartyScreenLogic partyScreenLogic = dataSource.PartyScreenLogic;
			return ((partyScreenLogic != null) ? new bool?(partyScreenLogic.IsTroopUpgradesDisabled) : ((bool?)null)) == true;
		}
	}

	public GauntletPartyScreen(PartyState partyState)
	{
		partyState.Handler = (IPartyScreenLogicHandler)(object)this;
		_partyState = partyState;
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		InformationManager.HideAllMessages();
	}

	protected override void OnFrameTick(float dt)
	{
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Invalid comparison between Unknown and I4
		((ScreenBase)this).OnFrameTick(dt);
		LoadingWindow.DisableGlobalLoadingWindow();
		_dataSource.IsFiveStackModifierActive = ((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown("FiveStackModifier");
		_dataSource.IsEntireStackModifierActive = ((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown("EntireStackModifier");
		if (!((GameState)_partyState).IsActive || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || (!((ScreenLayer)_gauntletLayer).Input.IsControlDown() && ((ScreenLayer)_gauntletLayer).Input.IsGameKeyReleased(43)))
		{
			HandleCancelInput();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			HandleDoneInput();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Reset"))
		{
			HandleResetInput();
		}
		else if (!_dataSource.IsAnyPopUpOpen)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("TakeAllTroops"))
			{
				if (_dataSource.IsOtherTroopsHaveTransferableTroops)
				{
					UISoundsHelper.PlayUISound("event:/ui/inventory/take_all");
					_dataSource.ExecuteTransferAllOtherTroops();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("GiveAllTroops"))
			{
				if (_dataSource.IsMainTroopsHaveTransferableTroops)
				{
					UISoundsHelper.PlayUISound("event:/ui/inventory/take_all");
					_dataSource.ExecuteTransferAllMainTroops();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("TakeAllPrisoners"))
			{
				if (_dataSource.CurrentFocusedCharacter != null && Input.IsGamepadActive)
				{
					if (_dataSource.CurrentFocusedCharacter.IsTroopTransferrable && (int)_dataSource.CurrentFocusedCharacter.Side == 0)
					{
						_dataSource.CurrentFocusedCharacter.ExecuteTransferSingle();
						UISoundsHelper.PlayUISound("event:/ui/transfer");
					}
				}
				else if (_dataSource.IsOtherPrisonersHaveTransferableTroops)
				{
					UISoundsHelper.PlayUISound("event:/ui/inventory/take_all");
					_dataSource.ExecuteTransferAllOtherPrisoners();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("GiveAllPrisoners"))
			{
				if (_dataSource.CurrentFocusedCharacter != null && Input.IsGamepadActive)
				{
					if (_dataSource.CurrentFocusedCharacter.IsTroopTransferrable && (int)_dataSource.CurrentFocusedCharacter.Side == 1)
					{
						_dataSource.CurrentFocusedCharacter.ExecuteTransferSingle();
						UISoundsHelper.PlayUISound("event:/ui/transfer");
					}
				}
				else if (_dataSource.IsMainPrisonersHaveTransferableTroops)
				{
					UISoundsHelper.PlayUISound("event:/ui/inventory/take_all");
					_dataSource.ExecuteTransferAllMainPrisoners();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("OpenUpgradePopup"))
			{
				if (!_dataSource.IsUpgradePopUpDisabled)
				{
					_dataSource.ExecuteOpenUpgradePopUp();
					UISoundsHelper.PlayUISound("event:/ui/default");
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("OpenRecruitPopup"))
			{
				if (!_dataSource.IsRecruitPopUpDisabled)
				{
					_dataSource.ExecuteOpenRecruitPopUp();
					UISoundsHelper.PlayUISound("event:/ui/default");
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsGameKeyReleased(39) && _dataSource.CurrentFocusedCharacter != null && Input.IsGamepadActive)
			{
				_dataSource.CurrentFocusedCharacter.ExecuteOpenTroopEncyclopedia();
			}
		}
		else
		{
			if (!Input.IsGamepadActive)
			{
				return;
			}
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("PopupItemPrimaryAction"))
			{
				if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsOpen && ((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsPrimaryActionAvailable)
				{
					UISoundsHelper.PlayUISound("event:/ui/party/upgrade");
					((PartyTroopManagerVM)_dataSource.UpgradePopUp).ExecuteItemPrimaryAction();
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("PopupItemSecondaryAction"))
			{
				if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsOpen)
				{
					if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsSecondaryActionAvailable)
					{
						UISoundsHelper.PlayUISound("event:/ui/party/upgrade");
						((PartyTroopManagerVM)_dataSource.UpgradePopUp).ExecuteItemSecondaryAction();
					}
				}
				else if (((PartyTroopManagerVM)_dataSource.RecruitPopUp).IsOpen)
				{
					PartyTroopManagerItemVM focusedTroop = ((PartyTroopManagerVM)_dataSource.RecruitPopUp).FocusedTroop;
					if (focusedTroop != null && focusedTroop.PartyCharacter.IsTroopRecruitable)
					{
						UISoundsHelper.PlayUISound("event:/ui/party/recruit_prisoner");
						((PartyTroopManagerVM)_dataSource.RecruitPopUp).ExecuteItemPrimaryAction();
					}
				}
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("GiveAllTroops"))
			{
				if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsOpen && ((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsTertiaryActionAvailable)
				{
					UISoundsHelper.PlayUISound("event:/ui/party/upgrade");
					((PartyTroopManagerVM)_dataSource.UpgradePopUp).ExecuteItemTertiaryAction();
				}
			}
			else
			{
				if (!((ScreenLayer)_gauntletLayer).Input.IsGameKeyReleased(39))
				{
					return;
				}
				if (((PartyTroopManagerVM)_dataSource.RecruitPopUp).IsOpen && ((PartyTroopManagerVM)_dataSource.RecruitPopUp).FocusedTroop != null)
				{
					((PartyTroopManagerVM)_dataSource.RecruitPopUp).FocusedTroop.PartyCharacter.ExecuteOpenTroopEncyclopedia();
				}
				else if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsOpen)
				{
					if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).FocusedTroop != null)
					{
						((PartyTroopManagerVM)_dataSource.UpgradePopUp).FocusedTroop.ExecuteOpenTroopEncyclopedia();
					}
					else if (_dataSource.CurrentFocusedUpgrade != null)
					{
						_dataSource.CurrentFocusedUpgrade.ExecuteUpgradeEncyclopediaLink();
					}
				}
			}
		}
	}

	unsafe void IGameStateListener.OnActivate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_partyscreenCategory = UIResourceManager.LoadSpriteCategory("ui_partyscreen");
		_gauntletLayer = new GauntletLayer("PartyScreen", 1, true);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PartyHotKeyCategory"));
		_dataSource = new PartyVM(_partyState.PartyScreenLogic);
		_dataSource.SetGetKeyTextFromKeyIDFunc(new Func<string, TextObject>(Game.Current.GameTextManager, (nint)(delegate*<GameTextManager, string, TextObject>)(&GameKeyTextExtensions.GetHotKeyGameTextFromKeyID)));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetTakeAllTroopsInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("TakeAllTroops"));
		_dataSource.SetDismissAllTroopsInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("GiveAllTroops"));
		_dataSource.SetTakeAllPrisonersInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("TakeAllPrisoners"));
		_dataSource.SetDismissAllPrisonersInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("GiveAllPrisoners"));
		_dataSource.SetOpenUpgradePanelInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("OpenUpgradePopup"));
		_dataSource.SetOpenRecruitPanelInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("OpenRecruitPopup"));
		((PartyTroopManagerVM)_dataSource.UpgradePopUp).SetPrimaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemPrimaryAction"));
		((PartyTroopManagerVM)_dataSource.UpgradePopUp).SetSecondaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemSecondaryAction"));
		((PartyTroopManagerVM)_dataSource.UpgradePopUp).SetTertiaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("GiveAllTroops"));
		((PartyTroopManagerVM)_dataSource.RecruitPopUp).SetPrimaryActionInputKey(HotKeyManager.GetCategory("PartyHotKeyCategory").GetHotKey("PopupItemSecondaryAction"));
		_gauntletLayer.LoadMovie("PartyScreen", (ViewModel)(object)_dataSource);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)1));
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_party_open");
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		PartyBase.MainParty.SetVisualAsDirty();
		ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		if (Campaign.Current.ConversationManager.IsConversationInProgress && !Campaign.Current.ConversationManager.IsConversationFlowActive)
		{
			Campaign.Current.ConversationManager.OnConversationActivate();
		}
	}

	void IGameStateListener.OnInitialize()
	{
		CampaignEvents.CompanionRemoved.AddNonSerializedListener((object)this, (Action<Hero, RemoveCompanionDetail>)OnCompanionRemoved);
	}

	void IGameStateListener.OnFinalize()
	{
		((IMbEventBase)CampaignEvents.CompanionRemoved).ClearListeners((object)this);
		((ViewModel)_dataSource).OnFinalize();
		_partyscreenCategory.Unload();
		_dataSource = null;
		_gauntletLayer = null;
	}

	void IPartyScreenPrisonHandler.ExecuteTakeAllPrisonersScript()
	{
		_dataSource.ExecuteTransferAllOtherPrisoners();
	}

	void IPartyScreenPrisonHandler.ExecuteDoneScript()
	{
		_dataSource.ExecuteDone();
	}

	void IPartyScreenPrisonHandler.ExecuteResetScript()
	{
		_dataSource.ExecuteReset();
	}

	void IPartyScreenPrisonHandler.ExecuteSellAllPrisoners()
	{
		_dataSource.ExecuteTransferAllMainPrisoners();
	}

	void IPartyScreenTroopHandler.PartyTroopTransfer()
	{
		_dataSource.ExecuteTransferAllMainTroops();
	}

	protected override void OnResume()
	{
		((ScreenBase)this).OnResume();
		PartyVM dataSource = _dataSource;
		if (dataSource != null && dataSource.IsInConversation)
		{
			_dataSource.IsInConversation = false;
			if (_dataSource.PartyScreenLogic.IsDoneActive())
			{
				_dataSource.PartyScreenLogic.DoneLogic(false);
			}
		}
	}

	public void RequestUserInput(string text, Action accept, Action cancel)
	{
	}

	private void HandleResetInput()
	{
		if (!_dataSource.IsAnyPopUpOpen)
		{
			_dataSource.ExecuteReset();
			UISoundsHelper.PlayUISound("event:/ui/default");
		}
	}

	private void HandleCancelInput()
	{
		if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsOpen)
		{
			((PartyTroopManagerVM)_dataSource.UpgradePopUp).ExecuteCancel();
		}
		else if (((PartyTroopManagerVM)_dataSource.RecruitPopUp).IsOpen)
		{
			((PartyTroopManagerVM)_dataSource.RecruitPopUp).ExecuteCancel();
		}
		else
		{
			_dataSource.ExecuteCancel(true);
		}
		UISoundsHelper.PlayUISound("event:/ui/default");
	}

	void IPartyScreenTroopHandler.ExecuteDoneScript()
	{
		_dataSource.ExecuteDone();
	}

	private void HandleDoneInput()
	{
		if (((PartyTroopManagerVM)_dataSource.UpgradePopUp).IsOpen)
		{
			((PartyTroopManagerVM)_dataSource.UpgradePopUp).ExecuteDone();
		}
		else if (((PartyTroopManagerVM)_dataSource.RecruitPopUp).IsOpen)
		{
			((PartyTroopManagerVM)_dataSource.RecruitPopUp).ExecuteDone();
		}
		else
		{
			_dataSource.ExecuteDone();
		}
		UISoundsHelper.PlayUISound("event:/ui/default");
	}

	private void OnCompanionRemoved(Hero arg1, RemoveCompanionDetail arg2)
	{
		((IChangeableScreen)this).ApplyChanges();
	}

	bool IChangeableScreen.AnyUnsavedChanges()
	{
		return _partyState.PartyScreenLogic.IsThereAnyChanges();
	}

	bool IChangeableScreen.CanChangesBeApplied()
	{
		return _partyState.PartyScreenLogic.IsDoneActive();
	}

	void IChangeableScreen.ApplyChanges()
	{
		_partyState.PartyScreenLogic.DoneLogic(true);
	}

	void IChangeableScreen.ResetChanges()
	{
		_partyState.PartyScreenLogic.Reset(true);
	}
}
