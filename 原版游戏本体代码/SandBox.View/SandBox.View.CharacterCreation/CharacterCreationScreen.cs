using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.CharacterCreation;

[GameStateScreen(typeof(CharacterCreationState))]
public class CharacterCreationScreen : ScreenBase, ICharacterCreationStateHandler, IGameStateListener
{
	private const string CultureParameterId = "MissionCulture";

	private readonly CharacterCreationState _characterCreationStateState;

	private IEnumerable<ScreenLayer> _shownLayers;

	private CharacterCreationStageViewBase _currentStageView;

	private readonly Dictionary<Type, Type> _stageViews;

	private SoundEvent _cultureAmbientSoundEvent;

	private Scene _genericScene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	public CharacterCreationScreen(CharacterCreationState characterCreationState)
	{
		_characterCreationStateState = characterCreationState;
		characterCreationState.Handler = (ICharacterCreationStateHandler)(object)this;
		_stageViews = new Dictionary<Type, Type>();
		CollectUnorderedStages();
		_cultureAmbientSoundEvent = SoundEvent.CreateEventFromString("event:/mission/ambient/special/charactercreation", (Scene)null);
		_cultureAmbientSoundEvent.Play();
		CreateGenericScene();
	}

	private void CreateGenericScene()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		_genericScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		SceneInitializationData val = new SceneInitializationData
		{
			InitPhysicsWorld = false
		};
		_genericScene.Read("character_menu_new", ref val, "");
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_genericScene);
	}

	private void StopSound()
	{
		SoundManager.SetGlobalParameter("MissionCulture", 0f);
		SoundEvent cultureAmbientSoundEvent = _cultureAmbientSoundEvent;
		if (cultureAmbientSoundEvent != null)
		{
			cultureAmbientSoundEvent.Stop();
		}
		_cultureAmbientSoundEvent = null;
	}

	void ICharacterCreationStateHandler.OnCharacterCreationFinalized()
	{
		LoadingWindow.EnableGlobalLoadingWindow();
	}

	void ICharacterCreationStateHandler.OnRefresh()
	{
		if (_shownLayers != null)
		{
			ScreenLayer[] array = _shownLayers.ToArray();
			foreach (ScreenLayer val in array)
			{
				((ScreenBase)this).RemoveLayer(val);
			}
		}
		if (_currentStageView == null)
		{
			return;
		}
		_shownLayers = _currentStageView.GetLayers();
		if (_shownLayers != null)
		{
			ScreenLayer[] array = _shownLayers.ToArray();
			foreach (ScreenLayer val2 in array)
			{
				((ScreenBase)this).AddLayer(val2);
			}
		}
	}

	void ICharacterCreationStateHandler.OnStageCreated(CharacterCreationStageBase stage)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		if (_stageViews.TryGetValue(((object)stage).GetType(), out var value))
		{
			_currentStageView = Activator.CreateInstance(value, _characterCreationStateState.CharacterCreationManager, (object)new ControlCharacterCreationStage(_characterCreationStateState.CharacterCreationManager.NextStage), (object)new TextObject("{=Rvr1bcu8}Next", (Dictionary<string, object>)null), (object)new ControlCharacterCreationStage(_characterCreationStateState.CharacterCreationManager.PreviousStage), (object)new TextObject("{=WXAaWZVf}Previous", (Dictionary<string, object>)null), (object)new ControlCharacterCreationStage(_characterCreationStateState.Refresh), (object)new ControlCharacterCreationStageReturnInt(_characterCreationStateState.CharacterCreationManager.GetIndexOfCurrentStage), (object)new ControlCharacterCreationStageReturnInt(_characterCreationStateState.CharacterCreationManager.GetTotalStagesCount), (object)new ControlCharacterCreationStageReturnInt(_characterCreationStateState.CharacterCreationManager.GetFurthestIndex), (object)new ControlCharacterCreationStageWithInt(_characterCreationStateState.CharacterCreationManager.GoToStage)) as CharacterCreationStageViewBase;
			stage.Listener = (ICharacterCreationStageListener)(object)_currentStageView;
			_currentStageView.SetGenericScene(_genericScene);
		}
		else
		{
			_currentStageView = null;
		}
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (LoadingWindow.IsLoadingWindowActive)
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		_currentStageView?.Tick(dt);
	}

	void IGameStateListener.OnActivate()
	{
		((ScreenBase)this).OnActivate();
	}

	void IGameStateListener.OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
	}

	void IGameStateListener.OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
	}

	void IGameStateListener.OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		StopSound();
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_genericScene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
		_genericScene.ClearAll();
		((NativeObject)_genericScene).ManualInvalidate();
		_genericScene = null;
	}

	private void CollectUnorderedStages()
	{
		Assembly assembly = typeof(CharacterCreationStageViewAttribute).Assembly;
		Assembly[] activeReferencingGameAssembliesSafe = Extensions.GetActiveReferencingGameAssembliesSafe(assembly);
		CollectStagesFromAssembly(assembly);
		Assembly[] array = activeReferencingGameAssembliesSafe;
		foreach (Assembly assembly2 in array)
		{
			CollectStagesFromAssembly(assembly2);
		}
	}

	private void CollectStagesFromAssembly(Assembly assembly)
	{
		foreach (Type item in Extensions.GetTypesSafe(assembly, (Func<Type, bool>)null))
		{
			if (typeof(CharacterCreationStageViewBase).IsAssignableFrom(item) && Extensions.GetCustomAttributesSafe(item, typeof(CharacterCreationStageViewAttribute), true).FirstOrDefault() is CharacterCreationStageViewAttribute characterCreationStageViewAttribute)
			{
				if (_stageViews.ContainsKey(characterCreationStageViewAttribute.StageType))
				{
					_stageViews[characterCreationStageViewAttribute.StageType] = item;
				}
				else
				{
					_stageViews.Add(characterCreationStageViewAttribute.StageType, item);
				}
			}
		}
	}
}
