using SandBox.Tournaments.MissionLogics;
using SandBox.View.Missions.Tournaments;
using SandBox.ViewModelCollection.Tournament;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(MissionTournamentView))]
public class MissionGauntletTournamentView : MissionView
{
	private TournamentBehavior _behavior;

	private Camera _customCamera;

	private bool _viewEnabled = true;

	private GauntletMovieIdentifier _gauntletMovie;

	private GauntletLayer _gauntletLayer;

	private TournamentVM _dataSource;

	public MissionGauntletTournamentView()
	{
		base.ViewOrderPriority = 48;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource = new TournamentVM(DisableUi, _behavior);
		_gauntletLayer = new GauntletLayer("MissionTournament", base.ViewOrderPriority, false);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_gauntletMovie = _gauntletLayer.LoadMovie("Tournament", (ViewModel)(object)_dataSource);
		((MissionView)this).MissionScreen.CustomCamera = _customCamera;
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		_gauntletMovie = null;
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void AfterStart()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		_behavior = ((MissionBehavior)this).Mission.GetMissionBehavior<TournamentBehavior>();
		GameEntity obj = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("camera_instance");
		_customCamera = Camera.CreateCamera();
		Vec3 val = default(Vec3);
		obj.GetCameraParamsFromCameraScript(_customCamera, ref val);
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Invalid comparison between Unknown and I4
		if (_behavior == null)
		{
			return;
		}
		if (((ScreenLayer)_gauntletLayer).IsFocusLayer && _dataSource.IsCurrentMatchActive)
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		}
		else if (!((ScreenLayer)_gauntletLayer).IsFocusLayer && !_dataSource.IsCurrentMatchActive)
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
		if (_dataSource.IsBetWindowEnabled)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteBet();
				_dataSource.IsBetWindowEnabled = false;
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.IsBetWindowEnabled = false;
			}
		}
		if (!_viewEnabled && ((_behavior.LastMatch != null && _behavior.CurrentMatch == null) || _behavior.CurrentMatch.IsReady))
		{
			_dataSource.Refresh();
			ShowUi();
		}
		if (!_viewEnabled && _dataSource.CurrentMatch.IsValid)
		{
			TournamentMatch currentMatch = _behavior.CurrentMatch;
			if (currentMatch != null && (int)currentMatch.State == 1)
			{
				_dataSource.CurrentMatch.RefreshActiveMatch();
			}
		}
		if (_dataSource.IsOver && _viewEnabled && !((MissionBehavior)this).DebugInput.IsControlDown() && ((MissionBehavior)this).DebugInput.IsHotKeyPressed("ShowHighlightsSummary"))
		{
			HighlightsController missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<HighlightsController>();
			if (missionBehavior != null)
			{
				missionBehavior.ShowSummary();
			}
		}
	}

	private void DisableUi()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (_viewEnabled)
		{
			((MissionView)this).MissionScreen.UpdateFreeCamera(_customCamera.Frame);
			((MissionView)this).MissionScreen.CustomCamera = null;
			_viewEnabled = false;
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		}
	}

	private void ShowUi()
	{
		if (!_viewEnabled)
		{
			((MissionView)this).MissionScreen.CustomCamera = _customCamera;
			_viewEnabled = true;
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		}
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return !_viewEnabled;
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
		_dataSource.OnAgentRemoved(affectedAgent);
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
