using System;
using System.Collections.ObjectModel;
using SandBox.Conversation.MissionLogics;
using SandBox.View.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.ViewModelCollection.Conversation;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionConversationView))]
public class MissionGauntletConversationView : MissionView, IConversationStateHandler
{
	private MissionConversationVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private MissionConversationCameraView _conversationCameraView;

	private MissionGauntletEscapeMenuBase _escapeView;

	private SpriteCategory _conversationCategory;

	public MissionConversationLogic ConversationHandler { get; private set; }

	public MissionGauntletConversationView()
	{
		base.ViewOrderPriority = 49;
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Invalid comparison between Unknown and I4
		((MissionView)this).OnMissionScreenTick(dt);
		MissionGauntletEscapeMenuBase escapeView = _escapeView;
		if ((escapeView != null && ((MissionEscapeMenuView)escapeView).IsActive) || _gauntletLayer == null)
		{
			return;
		}
		SceneLayer sceneLayer = ((MissionView)this).MissionScreen.SceneLayer;
		if (sceneLayer != null && ((ScreenLayer)sceneLayer).Input.IsKeyDown((InputKey)225))
		{
			MissionConversationCameraView conversationCameraView = _conversationCameraView;
			if (conversationCameraView == null || !conversationCameraView.IsCameraOverridden)
			{
				((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(false);
				goto IL_008a;
			}
		}
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
		goto IL_008a;
		IL_008a:
		if (IsGameKeyReleasedInAnyLayer("ContinueKey"))
		{
			MissionConversationVM dataSource = _dataSource;
			if (dataSource != null && ((Collection<ConversationItemVM>)(object)dataSource.AnswerList).Count <= 0 && (int)((MissionBehavior)this).Mission.Mode != 5)
			{
				MissionConversationVM dataSource2 = _dataSource;
				if (dataSource2 != null && !dataSource2.SelectedAnOptionOrLinkThisFrame)
				{
					MissionConversationVM dataSource3 = _dataSource;
					if (dataSource3 != null)
					{
						dataSource3.ExecuteContinue();
					}
				}
			}
		}
		if (_dataSource != null)
		{
			_dataSource.Tick(dt);
			_dataSource.SelectedAnOptionOrLinkThisFrame = false;
		}
		if (_gauntletLayer != null && IsGameKeyReleasedInAnyLayer("ToggleEscapeMenu"))
		{
			((MissionView)this).MissionScreen.OnEscape();
		}
	}

	public override void OnMissionScreenFinalize()
	{
		Campaign.Current.ConversationManager.Handler = null;
		if (_dataSource != null)
		{
			MissionConversationVM dataSource = _dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).OnFinalize();
			}
			_dataSource = null;
		}
		_gauntletLayer = null;
		ConversationHandler = null;
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		ConversationHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationLogic>();
		_conversationCameraView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionConversationCameraView>();
		Campaign.Current.ConversationManager.Handler = (IConversationStateHandler)(object)this;
	}

	public override void OnMissionScreenActivate()
	{
		((MissionView)this).OnMissionScreenActivate();
		if (_dataSource != null)
		{
			((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndDeactivateOthers(new string[2] { "MissionConversation", "SceneLayer" }, true);
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}

	void IConversationStateHandler.OnConversationInstall()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		((MissionView)this).MissionScreen.SetConversationActive(true);
		_conversationCategory = UIResourceManager.LoadSpriteCategory("ui_conversation");
		_dataSource = new MissionConversationVM((Func<string>)GetContinueKeyText, false);
		_gauntletLayer = new GauntletLayer("MissionConversation", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("SPConversation", (ViewModel)(object)_dataSource);
		GameKeyContext category = HotKeyManager.GetCategory("ConversationHotKeyCategory");
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(category);
		if (!((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsCategoryRegistered(category))
		{
			((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(category);
		}
		GameKeyContext category2 = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(category2);
		if (!((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsCategoryRegistered(category2))
		{
			((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(category2);
		}
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		_escapeView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletEscapeMenuBase>();
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndDeactivateOthers(new string[2] { "MissionConversation", "SceneLayer" }, true);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		InformationManager.HideAllMessages();
	}

	public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnMissionModeChange(oldMissionMode, atStart);
		if ((int)oldMissionMode == 5 && (int)((MissionBehavior)this).Mission.Mode == 1)
		{
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
	}

	void IConversationStateHandler.OnConversationUninstall()
	{
		((MissionView)this).MissionScreen.SetConversationActive(false);
		if (_dataSource != null)
		{
			MissionConversationVM dataSource = _dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).OnFinalize();
			}
			_dataSource = null;
		}
		_conversationCategory.Unload();
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndToggleOthers(new string[1] { "MissionConversation" }, false);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesState(new string[1] { "SceneLayer" }, true);
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		_escapeView = null;
	}

	private string GetContinueKeyText()
	{
		if (Input.IsGamepadActive)
		{
			return ((object)GameTexts.FindText("str_click_to_continue_console", (string)null).SetTextVariable("CONSOLE_KEY_NAME", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("ConversationHotKeyCategory", "ContinueClick"), 1f))).ToString();
		}
		return ((object)GameTexts.FindText("str_click_to_continue", (string)null)).ToString();
	}

	void IConversationStateHandler.OnConversationActivate()
	{
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndDeactivateOthers(new string[2] { "MissionConversation", "SceneLayer" }, true);
	}

	void IConversationStateHandler.OnConversationDeactivate()
	{
		MBInformationManager.HideInformations();
	}

	void IConversationStateHandler.OnConversationContinue()
	{
		_dataSource.OnConversationContinue();
	}

	void IConversationStateHandler.ExecuteConversationContinue()
	{
		_dataSource.ExecuteContinue();
	}

	private bool IsGameKeyReleasedInAnyLayer(string hotKeyID)
	{
		bool num = IsReleasedInSceneLayer(hotKeyID);
		bool flag = IsReleasedInGauntletLayer(hotKeyID);
		return num || flag;
	}

	private bool IsReleasedInSceneLayer(string hotKeyID)
	{
		SceneLayer sceneLayer = ((MissionView)this).MissionScreen.SceneLayer;
		if (sceneLayer == null)
		{
			return false;
		}
		return ((ScreenLayer)sceneLayer).Input.IsHotKeyReleased(hotKeyID);
	}

	private bool IsReleasedInGauntletLayer(string hotKeyID)
	{
		GauntletLayer gauntletLayer = _gauntletLayer;
		if (gauntletLayer == null)
		{
			return false;
		}
		return ((ScreenLayer)gauntletLayer).Input.IsHotKeyReleased(hotKeyID);
	}
}
