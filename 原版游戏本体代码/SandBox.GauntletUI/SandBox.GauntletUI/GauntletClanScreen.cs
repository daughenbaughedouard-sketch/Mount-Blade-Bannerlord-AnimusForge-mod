using System;
using Helpers;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(ClanState))]
public class GauntletClanScreen : ScreenBase, IGameStateListener
{
	protected GauntletLayer _gauntletLayer;

	protected SpriteCategory _clanCategory;

	protected readonly ClanState _clanState;

	protected bool _isCreatingPartyWithMembers;

	public ClanManagementVM _dataSource { get; private set; }

	public GauntletClanScreen(ClanState clanState)
	{
		_clanState = clanState;
	}

	protected virtual ClanManagementVM CreateDataSource()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		return new ClanManagementVM((Action)CloseClanScreen, (Action<Hero>)ShowHeroOnMap, (Action<Hero>)OpenPartyScreenForNewClanParty, (Action)OpenBannerEditorWithPlayerClan);
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		InformationManager.HideAllMessages();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		LoadingWindow.DisableGlobalLoadingWindow();
		ClanManagementVM dataSource = _dataSource;
		ClanCardSelectionPopupVM cardSelectionPopup = _dataSource.CardSelectionPopup;
		dataSource.CanSwitchTabs = (cardSelectionPopup == null || !cardSelectionPopup.IsVisible) && (!Input.IsGamepadActive || (!InformationManager.GetIsAnyTooltipActiveAndExtended() && ((ScreenLayer)_gauntletLayer).IsHitThisFrame));
		ClanManagementVM dataSource2 = _dataSource;
		if (dataSource2 != null)
		{
			ClanCardSelectionPopupVM cardSelectionPopup2 = dataSource2.CardSelectionPopup;
			if (((cardSelectionPopup2 != null) ? new bool?(cardSelectionPopup2.IsVisible) : ((bool?)null)) == true)
			{
				if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
				{
					if (_dataSource.CardSelectionPopup.IsDoneEnabled)
					{
						UISoundsHelper.PlayUISound("event:/ui/default");
						_dataSource.CardSelectionPopup.ExecuteDone();
					}
				}
				else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
				{
					UISoundsHelper.PlayUISound("event:/ui/default");
					_dataSource.CardSelectionPopup.ExecuteCancel();
				}
				return;
			}
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			if (IsRoleSelectionPopupActive())
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ClanParties.CurrentSelectedParty.IsRoleSelectionPopupVisible = false;
			}
			else
			{
				CloseClanScreen();
			}
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(41) || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			CloseClanScreen();
		}
		else if (_dataSource.CanSwitchTabs)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SwitchToPreviousTab"))
			{
				UISoundsHelper.PlayUISound("event:/ui/tab");
				_dataSource.SelectPreviousCategory();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SwitchToNextTab"))
			{
				UISoundsHelper.PlayUISound("event:/ui/tab");
				_dataSource.SelectNextCategory();
			}
		}
	}

	protected bool IsRoleSelectionPopupActive()
	{
		ClanPartiesVM clanParties = _dataSource.ClanParties;
		if (clanParties.IsSelected && clanParties.IsAnyValidPartySelected)
		{
			return clanParties.CurrentSelectedParty.IsRoleSelectionPopupVisible;
		}
		return false;
	}

	protected void OpenPartyScreenForNewClanParty(Hero hero)
	{
		_isCreatingPartyWithMembers = true;
		PartyScreenHelper.OpenScreenAsCreateClanPartyForHero(hero, (PartyScreenClosedDelegate)null, (IsTroopTransferableDelegate)null);
	}

	protected void OpenBannerEditorWithPlayerClan()
	{
		Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
	}

	void IGameStateListener.OnActivate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_clanCategory = UIResourceManager.LoadSpriteCategory("ui_clan");
		_gauntletLayer = new GauntletLayer("ClanScreen", 1, true);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_dataSource = CreateDataSource();
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_dataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		if (_isCreatingPartyWithMembers)
		{
			_dataSource.SelectParty(PartyBase.MainParty);
			_isCreatingPartyWithMembers = false;
		}
		else if (_clanState.InitialSelectedHero != null)
		{
			_dataSource.SelectHero(_clanState.InitialSelectedHero);
		}
		else if (_clanState.InitialSelectedParty != null)
		{
			_dataSource.SelectParty(_clanState.InitialSelectedParty);
			if (_clanState.InitialSelectedParty.LeaderHero == null)
			{
				ClanPartiesVM clanParties = _dataSource.ClanParties;
				if (clanParties != null)
				{
					ClanPartyItemVM currentSelectedParty = clanParties.CurrentSelectedParty;
					if (((currentSelectedParty != null) ? new bool?(currentSelectedParty.IsChangeLeaderEnabled) : ((bool?)null)) == true)
					{
						_dataSource.ClanParties.OnShowChangeLeaderPopup();
					}
				}
			}
		}
		else if (_clanState.InitialSelectedSettlement != null)
		{
			_dataSource.SelectSettlement(_clanState.InitialSelectedSettlement);
		}
		else if (_clanState.InitialSelectedWorkshop != null)
		{
			_dataSource.SelectWorkshop(_clanState.InitialSelectedWorkshop);
		}
		else if (_clanState.InitialSelectedAlley != null)
		{
			_dataSource.SelectAlley(_clanState.InitialSelectedAlley);
		}
		_gauntletLayer.LoadMovie("ClanScreen", (ViewModel)(object)_dataSource);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)6));
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_clan_open");
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
	}

	protected void ShowHeroOnMap(Hero hero)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		CloseClanScreen();
		MapScreen.Instance.FastMoveCameraToPosition(hero.GetCampaignPosition());
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
		_clanCategory.Unload();
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		ClanManagementVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.RefreshCategoryValues();
		}
		ClanManagementVM dataSource2 = _dataSource;
		if (dataSource2 != null)
		{
			dataSource2.UpdateBannerVisuals();
		}
	}

	protected void CloseClanScreen()
	{
		Game.Current.GameStateManager.PopState(0);
		UISoundsHelper.PlayUISound("event:/ui/default");
	}
}
