using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(BarberState))]
public class GauntletBarberScreen : ScreenBase, IGameStateListener, IFaceGeneratorScreen
{
	private readonly BodyGeneratorView _facegenLayer;

	public IFaceGeneratorHandler Handler => (IFaceGeneratorHandler)(object)_facegenLayer;

	public GauntletBarberScreen(BarberState state)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0056: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		LoadingWindow.EnableGlobalLoadingWindow();
		_facegenLayer = new BodyGeneratorView(new ControlCharacterCreationStage(OnExit), GameTexts.FindText("str_done", (string)null), new ControlCharacterCreationStage(OnExit), GameTexts.FindText("str_cancel", (string)null), (BasicCharacterObject)(object)Hero.MainHero.CharacterObject, false, state.Filter, (Equipment)null, (ControlCharacterCreationStageReturnInt)null, (ControlCharacterCreationStageReturnInt)null, (ControlCharacterCreationStageReturnInt)null, (ControlCharacterCreationStageWithInt)null, (FaceGenHistory)null);
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		_facegenLayer.OnTick(dt);
	}

	public void OnExit()
	{
		Game.Current.GameStateManager.PopState(0);
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_facegenLayer.GauntletLayer);
		InformationManager.HideAllMessages();
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		if (LoadingWindow.IsLoadingWindowActive)
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_facegenLayer.SceneLayer);
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		((View)_facegenLayer.SceneLayer.SceneView).SetEnable(false);
		_facegenLayer.OnFinalize();
		LoadingWindow.EnableGlobalLoadingWindow();
		MBInformationManager.HideInformations();
	}

	void IGameStateListener.OnActivate()
	{
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
