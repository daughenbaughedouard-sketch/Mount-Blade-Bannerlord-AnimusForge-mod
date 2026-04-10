using System;
using System.Collections.ObjectModel;
using SandBox.View;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
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

[GameStateScreen(typeof(CharacterDeveloperState))]
public class GauntletCharacterDeveloperScreen : ScreenBase, IGameStateListener, IChangeableScreen, ICharacterDeveloperStateHandler
{
	private CharacterDeveloperVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _characterdeveloper;

	private readonly CharacterDeveloperState _characterDeveloperState;

	public GauntletCharacterDeveloperScreen(CharacterDeveloperState clanState)
	{
		_characterDeveloperState = clanState;
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
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(37))
		{
			if (_dataSource.CurrentCharacter.IsInspectingAnAttribute)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.CurrentCharacter.ExecuteStopInspectingCurrentAttribute();
			}
			else if (_dataSource.CurrentCharacter.PerkSelection.IsActive)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.CurrentCharacter.PerkSelection.ExecuteDeactivate();
			}
			else
			{
				CloseCharacterDeveloperScreen();
			}
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			ExecuteConfirm();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Reset"))
		{
			ExecuteReset();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("SwitchToPreviousTab"))
		{
			ExecuteSwitchToPreviousTab();
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("SwitchToNextTab"))
		{
			ExecuteSwitchToNextTab();
		}
	}

	unsafe void IGameStateListener.OnActivate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_characterdeveloper = UIResourceManager.LoadSpriteCategory("ui_characterdeveloper");
		_dataSource = new CharacterDeveloperVM((Action)CloseCharacterDeveloperScreen);
		_dataSource.SetGetKeyTextFromKeyIDFunc(new Func<string, TextObject>(Game.Current.GameTextManager, (nint)(delegate*<GameTextManager, string, TextObject>)(&GameKeyTextExtensions.GetHotKeyGameTextFromKeyID)));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetPreviousCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_dataSource.SetNextCharacterInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		if (_characterDeveloperState.InitialSelectedHero != null)
		{
			_dataSource.SelectHero(_characterDeveloperState.InitialSelectedHero);
		}
		_gauntletLayer = new GauntletLayer("CharacterDeveloper", 1, true);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		_gauntletLayer.LoadMovie("CharacterDeveloper", (ViewModel)(object)_dataSource);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)3));
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_character_open");
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
		_characterdeveloper.Unload();
	}

	private void CloseCharacterDeveloperScreen()
	{
		UISoundsHelper.PlayUISound("event:/ui/default");
		Game.Current.GameStateManager.PopState(0);
	}

	private void ExecuteConfirm()
	{
		UISoundsHelper.PlayUISound("event:/ui/default");
		_dataSource.ExecuteDone();
	}

	private void ExecuteReset()
	{
		UISoundsHelper.PlayUISound("event:/ui/default");
		_dataSource.ExecuteReset();
	}

	private void ExecuteSwitchToPreviousTab()
	{
		MBBindingList<SelectorItemVM> itemList = _dataSource.CharacterList.ItemList;
		if (itemList != null && ((Collection<SelectorItemVM>)(object)itemList).Count > 1)
		{
			UISoundsHelper.PlayUISound("event:/ui/checkbox");
		}
		_dataSource.CharacterList.ExecuteSelectPreviousItem();
	}

	private void ExecuteSwitchToNextTab()
	{
		MBBindingList<SelectorItemVM> itemList = _dataSource.CharacterList.ItemList;
		if (itemList != null && ((Collection<SelectorItemVM>)(object)itemList).Count > 1)
		{
			UISoundsHelper.PlayUISound("event:/ui/checkbox");
		}
		_dataSource.CharacterList.ExecuteSelectNextItem();
	}

	bool IChangeableScreen.AnyUnsavedChanges()
	{
		return _dataSource.IsThereAnyChanges();
	}

	bool IChangeableScreen.CanChangesBeApplied()
	{
		return true;
	}

	void IChangeableScreen.ApplyChanges()
	{
		_dataSource.ApplyAllChanges();
	}

	void IChangeableScreen.ResetChanges()
	{
		_dataSource.ExecuteReset();
	}
}
