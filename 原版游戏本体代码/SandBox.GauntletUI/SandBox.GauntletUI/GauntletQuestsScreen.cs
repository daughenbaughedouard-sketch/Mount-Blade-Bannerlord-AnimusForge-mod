using System;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
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

[GameStateScreen(typeof(QuestsState))]
public class GauntletQuestsScreen : ScreenBase, IGameStateListener
{
	protected QuestsVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _questCategory;

	private readonly QuestsState _questsState;

	public GauntletQuestsScreen(QuestsState questsState)
	{
		_questsState = questsState;
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
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm") || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyReleased(42))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteClose();
		}
	}

	void IGameStateListener.OnActivate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_questCategory = UIResourceManager.LoadSpriteCategory("ui_quest");
		_dataSource = new QuestsVM((Action)CloseQuestsScreen);
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_gauntletLayer = new GauntletLayer("QuestScreen", 1, true);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		_gauntletLayer.LoadMovie("QuestsScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)11));
		if (_questsState.InitialSelectedIssue != null)
		{
			_dataSource.SetSelectedIssue(_questsState.InitialSelectedIssue);
		}
		else if (_questsState.InitialSelectedQuest != null)
		{
			_dataSource.SetSelectedQuest(_questsState.InitialSelectedQuest);
		}
		else if (_questsState.InitialSelectedLog != null)
		{
			_dataSource.SetSelectedLog(_questsState.InitialSelectedLog);
		}
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_quest_open");
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		_questCategory.Unload();
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
		QuestsVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		_gauntletLayer = null;
	}

	private void CloseQuestsScreen()
	{
		Game.Current.GameStateManager.PopState(0);
	}
}
