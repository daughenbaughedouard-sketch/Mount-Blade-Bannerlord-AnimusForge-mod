using SandBox.View.Menu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TournamentLeaderboard;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Menu;

[OverrideView(typeof(MenuTournamentLeaderboardView))]
public class GauntletMenuTournamentLeaderboardView : MenuView
{
	private GauntletLayer _layerAsGauntletLayer;

	private TournamentLeaderboardVM _dataSource;

	private GauntletMovieIdentifier _movie;

	protected override void OnInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		base.OnInitialize();
		_dataSource = new TournamentLeaderboardVM
		{
			IsEnabled = true
		};
		base.Layer = (ScreenLayer)new GauntletLayer("MapTournamentLeaderboard", 206, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_movie = _layerAsGauntletLayer.LoadMovie("GameMenuTournamentLeaderboard", (ViewModel)(object)_dataSource);
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		base.MenuViewContext.AddLayer(base.Layer);
	}

	protected override void OnFinalize()
	{
		base.Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(base.Layer);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		base.MenuViewContext.RemoveLayer(base.Layer);
		_movie = null;
		base.Layer = null;
		base.OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (base.Layer.Input.IsHotKeyReleased("Exit") || base.Layer.Input.IsHotKeyReleased("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.IsEnabled = false;
		}
		if (!_dataSource.IsEnabled)
		{
			base.MenuViewContext.CloseTournamentLeaderboard();
		}
	}

	protected override void OnMapConversationActivated()
	{
		base.OnMapConversationActivated();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, true);
		}
	}

	protected override void OnMapConversationDeactivated()
	{
		base.OnMapConversationDeactivated();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, false);
		}
	}
}
