using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionBattleScoreUIHandler))]
public class MissionGauntletBattleScore : MissionView
{
	private ScoreboardBaseVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private bool _toOpen;

	private bool _isMouseEnabled;

	private static bool _forceScoreboardToggle;

	public ScoreboardBaseVM DataSource => _dataSource;

	public MissionGauntletBattleScore(ScoreboardBaseVM scoreboardVM)
	{
		_dataSource = scoreboardVM;
		ViewOrderPriority = 15;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenInitialize();
		((MissionBehavior)this).Mission.IsFriendlyMission = false;
		_dataSource.Initialize((IMissionScreen)(object)base.MissionScreen, ((MissionBehavior)this).Mission, (Action)null, (Action<bool>)ToggleScoreboard);
		CreateView();
		_dataSource.SetShortcuts(new ScoreboardHotkeys
		{
			ShowMouseHotkey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetGameKey(35),
			ShowScoreboardHotkey = HotKeyManager.GetCategory("Generic").GetGameKey(4),
			DoneInputKey = HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"),
			FastForwardKey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetHotKey("ToggleFastForward")
		});
	}

	private void CreateView()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		_gauntletLayer = new GauntletLayer("Scoreboard", ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("SPScoreboard", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
		GameKeyContext category = HotKeyManager.GetCategory("ScoreboardHotKeyCategory");
		if (!((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsCategoryRegistered(category))
		{
			((ScreenLayer)base.MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(category);
		}
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((MissionBehavior)this).Mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(Mission_OnMainAgentChanged);
		base.MissionScreen.GetSpectatedCharacter = null;
		base.OnMissionScreenFinalize();
		((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	public override bool OnEscape()
	{
		if (_dataSource.ShowScoreboard)
		{
			OnClose();
			return true;
		}
		return base.OnEscape();
	}

	public override void EarlyStart()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		((MissionBehavior)this).EarlyStart();
		((MissionBehavior)this).Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(Mission_OnMainAgentChanged);
	}

	private void Mission_OnMainAgentChanged(Agent oldAgent)
	{
		if (((MissionBehavior)this).Mission.MainAgent == null)
		{
			_dataSource.OnMainHeroDeath();
		}
		else if (((MissionBehavior)this).Mission.MainAgent.Character != Game.Current.PlayerTroop)
		{
			_dataSource.OnTakenControlOfAnotherAgent();
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		_dataSource.Tick(dt);
		int num;
		if (_forceScoreboardToggle || _dataSource.IsOver || _dataSource.IsMainCharacterDead || Input.IsGamepadActive)
		{
			if (CanOpenScoreboard())
			{
				if (!((MissionBehavior)this).Mission.InputManager.IsGameKeyPressed(4))
				{
					num = (((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(4) ? 1 : 0);
					if (num == 0)
					{
						goto IL_00c7;
					}
				}
				else
				{
					num = 1;
				}
				if (!_dataSource.ShowScoreboard)
				{
					MissionBehavior? obj = ((IEnumerable<MissionBehavior>)((MissionBehavior)this).Mission.MissionBehaviors).FirstOrDefault((Func<MissionBehavior, bool>)((MissionBehavior behavior) => behavior is IBattleEndLogic));
					MissionBehavior? obj2 = ((obj is IBattleEndLogic) ? obj : null);
					if (obj2 != null)
					{
						((IBattleEndLogic)obj2).SetNotificationDisabled(true);
					}
					_toOpen = true;
				}
			}
			else
			{
				num = 0;
			}
			goto IL_00c7;
		}
		int num2;
		if (CanOpenScoreboard())
		{
			if (!((MissionBehavior)this).Mission.InputManager.IsHotKeyDown("HoldShow"))
			{
				num2 = (((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown("HoldShow") ? 1 : 0);
				if (num2 == 0)
				{
					goto IL_01b8;
				}
			}
			else
			{
				num2 = 1;
			}
			if (!_dataSource.ShowScoreboard)
			{
				MissionBehavior? obj3 = ((IEnumerable<MissionBehavior>)((MissionBehavior)this).Mission.MissionBehaviors).FirstOrDefault((Func<MissionBehavior, bool>)((MissionBehavior behavior) => behavior is IBattleEndLogic));
				MissionBehavior? obj4 = ((obj3 is IBattleEndLogic) ? obj3 : null);
				if (obj4 != null)
				{
					((IBattleEndLogic)obj4).SetNotificationDisabled(true);
				}
				_toOpen = true;
			}
		}
		else
		{
			num2 = 0;
		}
		goto IL_01b8;
		IL_00c7:
		if (num != 0 && _dataSource.ShowScoreboard)
		{
			MissionBehavior? obj5 = ((IEnumerable<MissionBehavior>)((MissionBehavior)this).Mission.MissionBehaviors).FirstOrDefault((Func<MissionBehavior, bool>)((MissionBehavior behavior) => behavior is IBattleEndLogic));
			MissionBehavior? obj6 = ((obj5 is IBattleEndLogic) ? obj5 : null);
			if (obj6 != null)
			{
				((IBattleEndLogic)obj6).SetNotificationDisabled(false);
			}
			OnClose();
		}
		goto IL_020d;
		IL_020d:
		if (_toOpen)
		{
			OnOpen();
		}
		if (_dataSource.IsMainCharacterDead && !_dataSource.IsOver && (((MissionBehavior)this).Mission.InputManager.IsHotKeyReleased("ToggleFastForward") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("ToggleFastForward")))
		{
			_dataSource.IsFastForwarding = !_dataSource.IsFastForwarding;
			_dataSource.ExecuteFastForwardAction();
		}
		if (_dataSource.IsOver && _dataSource.ShowScoreboard && (((MissionBehavior)this).Mission.InputManager.IsHotKeyPressed("Confirm") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("Confirm")))
		{
			ExecuteQuitAction();
		}
		if (_dataSource.ShowScoreboard && !((MissionBehavior)this).DebugInput.IsControlDown() && ((MissionBehavior)this).DebugInput.IsHotKeyPressed("ShowHighlightsSummary"))
		{
			HighlightsController missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<HighlightsController>();
			if (missionBehavior != null)
			{
				missionBehavior.ShowSummary();
			}
		}
		bool flag = ((MissionBehavior)this).Mission.InputManager.IsGameKeyPressed(35) || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyPressed(35);
		if (_dataSource.ShowScoreboard && !_isMouseEnabled && flag)
		{
			SetMouseState(isEnabled: true);
		}
		return;
		IL_01b8:
		if (num2 == 0 && _dataSource.ShowScoreboard)
		{
			MissionBehavior? obj7 = ((IEnumerable<MissionBehavior>)((MissionBehavior)this).Mission.MissionBehaviors).FirstOrDefault((Func<MissionBehavior, bool>)((MissionBehavior behavior) => behavior is IBattleEndLogic));
			MissionBehavior? obj8 = ((obj7 is IBattleEndLogic) ? obj7 : null);
			if (obj8 != null)
			{
				((IBattleEndLogic)obj8).SetNotificationDisabled(false);
			}
			OnClose();
		}
		goto IL_020d;
	}

	private void ExecuteQuitAction()
	{
		_dataSource.ExecuteQuitAction();
	}

	private bool CanOpenScoreboard()
	{
		if (!base.MissionScreen.IsRadialMenuActive && !base.MissionScreen.IsPhotoModeEnabled)
		{
			return !((MissionBehavior)this).Mission.IsOrderMenuOpen;
		}
		return false;
	}

	private void ToggleScoreboard(bool value)
	{
		if (value)
		{
			_toOpen = true;
		}
		else
		{
			OnClose();
		}
	}

	private void OnOpen()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		_toOpen = false;
		if (!_dataSource.ShowScoreboard && (int)((MissionBehavior)this).Mission.Mode != 6)
		{
			base.MissionScreen.SetDisplayDialog(value: true);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
			_dataSource.ShowScoreboard = true;
			base.MissionScreen.SetCameraLockState(isLocked: true);
			if (_dataSource.IsOver || _dataSource.IsMainCharacterDead || ScreenManager.GetMouseVisibility())
			{
				SetMouseState(isEnabled: true);
			}
		}
	}

	private void OnClose()
	{
		if (_dataSource.ShowScoreboard)
		{
			base.MissionScreen.SetDisplayDialog(value: false);
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			_dataSource.ShowScoreboard = false;
			base.MissionScreen.SetCameraLockState(isLocked: false);
			SetMouseState(isEnabled: false);
		}
	}

	private void SetMouseState(bool isEnabled)
	{
		((ScreenLayer)_gauntletLayer).IsFocusLayer = isEnabled;
		if (isEnabled)
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
		else
		{
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		}
		ScoreboardBaseVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.SetMouseState(isEnabled);
		}
		_isMouseEnabled = isEnabled;
	}

	public override void OnDeploymentFinished()
	{
		((MissionBehavior)this).OnDeploymentFinished();
		ScoreboardBaseVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnDeploymentFinished();
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}

	[CommandLineArgumentFunction("force_toggle", "scoreboard")]
	public static string ForceScoreboardToggle(List<string> args)
	{
		if (args.Count == 1 && int.TryParse(args[0], out var result) && (result == 0 || result == 1))
		{
			_forceScoreboardToggle = result == 1;
			return "Force Scoreboard Toggle is: " + (_forceScoreboardToggle ? "ON" : "OFF");
		}
		return "Format is: scoreboard.force_toggle 0-1";
	}
}
