using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI;

[GameStateScreen(typeof(LobbyPracticeState))]
public class LobbyPracticeStateGauntletScreen : ScreenBase, IGameStateListener
{
	private MPPracticeVM _dataSource;

	public GauntletLayer Layer { get; private set; }

	public LobbyPracticeStateGauntletScreen(LobbyPracticeState gameState)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		_dataSource = new MPPracticeVM();
		Layer = new GauntletLayer("LobbyPracticeScreen", 100, false);
		((ScreenLayer)Layer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)Layer);
		((ScreenLayer)Layer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		Layer.LoadMovie("MultiplayerPractice", (ViewModel)(object)_dataSource);
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		((ScreenLayer)Layer).InputRestrictions.ResetInputRestrictions();
		Layer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	void IGameStateListener.OnActivate()
	{
		((ScreenLayer)Layer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		ScreenManager.TrySetFocus((ScreenLayer)(object)Layer);
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}
}
