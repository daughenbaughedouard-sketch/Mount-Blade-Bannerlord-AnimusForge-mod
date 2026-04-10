using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.ProfileSelection;
using TaleWorlds.PlatformService;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[GameStateScreen(typeof(ProfileSelectionState))]
public class GauntletProfileSelectionScreen : MBProfileSelectionScreenBase
{
	private GauntletLayer _gauntletLayer;

	private ProfileSelectionVM _dataSource;

	private ProfileSelectionState _state;

	public GauntletProfileSelectionScreen(ProfileSelectionState state)
		: base(state)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		_state = state;
		_state.OnProfileSelection += new OnProfileSelectionEvent(OnProfileSelection);
	}

	private void OnProfileSelection()
	{
		ProfileSelectionVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnActivate(_state.IsDirectPlayPossible);
		}
	}

	protected override void OnInitialize()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		_gauntletLayer = new GauntletLayer("ProfileSelection", 1, false);
		_dataSource = new ProfileSelectionVM(_state.IsDirectPlayPossible);
		ProfileSelectionVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnActivate(_state.IsDirectPlayPossible);
		}
		_gauntletLayer.LoadMovie("ProfileSelectionScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		MouseManager.ShowCursor(false);
		MouseManager.ShowCursor(true);
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		ProfileSelectionVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnActivate(_state.IsDirectPlayPossible);
		}
	}

	protected override void OnFinalize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		((ScreenBase)this).OnFinalize();
		_state.OnProfileSelection -= new OnProfileSelectionEvent(OnProfileSelection);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	protected override void OnProfileSelectionTick(float dt)
	{
		((MBProfileSelectionScreenBase)this).OnProfileSelectionTick(dt);
		if (_state.IsDirectPlayPossible && ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Play"))
		{
			if (PlatformServices.Instance.UserLoggedIn)
			{
				_state.StartGame();
			}
			else
			{
				((MBProfileSelectionScreenBase)this).OnActivateProfileSelection();
			}
		}
		else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("SelectProfile"))
		{
			((MBProfileSelectionScreenBase)this).OnActivateProfileSelection();
		}
	}
}
