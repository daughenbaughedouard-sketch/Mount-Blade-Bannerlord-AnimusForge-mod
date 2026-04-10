using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;

[OverrideView(typeof(FaceGeneratorScreen))]
public class GauntletBodyGeneratorScreen : ScreenBase, IFaceGeneratorScreen
{
	private const int ViewOrderPriority = 15;

	private readonly BodyGeneratorView _facegenLayer;

	public IFaceGeneratorHandler Handler => (IFaceGeneratorHandler)(object)_facegenLayer;

	public GauntletBodyGeneratorScreen(BasicCharacterObject character, bool openedFromMultiplayer, IFaceGeneratorCustomFilter filter)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0043: Expected O, but got Unknown
		_facegenLayer = new BodyGeneratorView(new ControlCharacterCreationStage(OnExit), GameTexts.FindText("str_done", (string)null), new ControlCharacterCreationStage(OnExit), GameTexts.FindText("str_cancel", (string)null), character, openedFromMultiplayer, filter);
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		_facegenLayer.OnTick(dt);
	}

	public void OnExit()
	{
		ScreenManager.PopScreen();
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
		_facegenLayer.OnFinalize();
		LoadingWindow.EnableGlobalLoadingWindow();
		MBInformationManager.HideInformations();
		Mission current = Mission.Current;
		if (current == null)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)current.Agents)
		{
			item.EquipItemsFromSpawnEquipment(false, false, false, 0);
			item.UpdateAgentProperties();
		}
	}
}
