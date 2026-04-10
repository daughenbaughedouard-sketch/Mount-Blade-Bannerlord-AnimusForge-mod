using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.PlayerServices;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

public class MultiplayerReportPlayerScreen : GlobalLayer
{
	private MultiplayerReportPlayerVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private bool _isActive;

	public static MultiplayerReportPlayerScreen Current { get; private set; }

	public MultiplayerReportPlayerScreen()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		_dataSource = new MultiplayerReportPlayerVM(OnReportDone, OnClose);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		GauntletLayer val = new GauntletLayer("MultiplayerReportPlayer", 15350, false);
		_movie = val.LoadMovie("MultiplayerReportPlayer", (ViewModel)(object)_dataSource);
		((ScreenLayer)val).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((GlobalLayer)this).Layer = (ScreenLayer)(object)val;
	}

	protected override void OnTick(float dt)
	{
		if (_isActive)
		{
			if (((GlobalLayer)this).Layer.Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteDone();
			}
			else if (((GlobalLayer)this).Layer.Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteCancel();
			}
		}
	}

	private void OnClose()
	{
		if (_isActive)
		{
			_isActive = false;
			((GlobalLayer)this).Layer.InputRestrictions.ResetInputRestrictions();
			ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, true);
			((GlobalLayer)this).Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(((GlobalLayer)this).Layer);
		}
	}

	public static void OnInitialize()
	{
		if (Current == null)
		{
			Current = new MultiplayerReportPlayerScreen();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
			MultiplayerReportPlayerManager.ReportHandlers += Current.OnReportRequest;
			Current._isActive = false;
			ScreenManager.SetSuspendLayer(((GlobalLayer)Current).Layer, true);
		}
	}

	public static void OnFinalize()
	{
		if (Current != null)
		{
			ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)Current);
			ScreenLayer layer = ((GlobalLayer)Current).Layer;
			((GauntletLayer)((layer is GauntletLayer) ? layer : null)).ReleaseMovie(Current._movie);
			MultiplayerReportPlayerManager.ReportHandlers -= Current.OnReportRequest;
			((ViewModel)Current._dataSource).OnFinalize();
			Current._dataSource = null;
			Current = null;
		}
	}

	private void OnReportRequest(string gameId, PlayerId playerId, string playerName, bool isRequestedFromMission)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!_isActive)
		{
			_isActive = true;
			ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, false);
			((GlobalLayer)this).Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(((GlobalLayer)this).Layer);
			((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			_dataSource.OpenNewReportWithGamePlayerId(gameId, playerId, playerName, isRequestedFromMission);
		}
	}

	private void OnReportDone(string gameId, PlayerId playerId, string playerName, PlayerReportType reportReason, string reasonText)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (_isActive)
		{
			OnClose();
			NetworkMain.GameClient.ReportPlayer(gameId, playerId, playerName, reportReason, reasonText);
			MultiplayerReportPlayerManager.OnPlayerReported(playerId);
		}
	}
}
