using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.FaceGenerator;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;

public class BodyGeneratorView : IFaceGeneratorHandler
{
	private const int ViewOrderPriority = 1;

	private const bool MakeSound = true;

	private Scene _facegenScene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private GauntletMovieIdentifier _viewMovie;

	private AgentVisuals _visualToShow;

	private List<KeyValuePair<AgentVisuals, int>> _visualsBeingPrepared;

	private readonly bool _openedFromMultiplayer;

	private AgentVisuals _nextVisualToShow;

	private int _currentAgentVisualIndex;

	private bool _refreshCharacterEntityNextFrame;

	private int _makeVoiceInFrames = -1;

	private MatrixFrame _initialCharacterFrame;

	private bool _setMorphAnimNextFrame;

	private string _nextMorphAnimToSet = "";

	private bool _nextMorphAnimLoopValue;

	private List<BodyProperties> _templateBodyProperties;

	private readonly ControlCharacterCreationStage _affirmativeAction;

	private readonly ControlCharacterCreationStage _negativeAction;

	private readonly ControlCharacterCreationStageReturnInt _getTotalStageCountAction;

	private readonly ControlCharacterCreationStageReturnInt _getCurrentStageIndexAction;

	private readonly ControlCharacterCreationStageReturnInt _getFurthestIndexAction;

	private readonly ControlCharacterCreationStageWithInt _goToIndexAction;

	public bool IsDressed;

	public SkeletonType SkeletonType;

	private readonly Equipment _dressedEquipment;

	private Camera _camera;

	private int _cameraLookMode;

	private MatrixFrame _targetCameraGlobalFrame;

	private MatrixFrame _defaultCameraGlobalFrame;

	private float _characterCurrentRotation;

	private float _characterTargetRotation;

	private float _cameraCurrentDistanceAdder;

	private float _cameraCurrentElevationAdder;

	private SpriteCategory _facegenCategory;

	private IInputContext DebugInput => Input.DebugInput;

	public FaceGenVM DataSource { get; private set; }

	public GauntletLayer GauntletLayer { get; private set; }

	public SceneLayer SceneLayer { get; private set; }

	public BodyGenerator BodyGen { get; private set; }

	public BodyGeneratorView(ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, BasicCharacterObject character, bool openedFromMultiplayer, IFaceGeneratorCustomFilter filter, Equipment dressedEquipment = null, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction = null, ControlCharacterCreationStageReturnInt getTotalStageCountAction = null, ControlCharacterCreationStageReturnInt getFurthestIndexAction = null, ControlCharacterCreationStageWithInt goToIndexAction = null, FaceGenHistory faceGenHistory = null)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Expected O, but got Unknown
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		_affirmativeAction = affirmativeAction;
		_negativeAction = negativeAction;
		_getCurrentStageIndexAction = getCurrentStageIndexAction;
		_getTotalStageCountAction = getTotalStageCountAction;
		_getFurthestIndexAction = getFurthestIndexAction;
		_goToIndexAction = goToIndexAction;
		_openedFromMultiplayer = openedFromMultiplayer;
		BodyGen = new BodyGenerator(character);
		_dressedEquipment = dressedEquipment ?? BodyGen.Character.Equipment.Clone(false);
		EquipmentElement val = _dressedEquipment[(EquipmentIndex)4];
		if (!((EquipmentElement)(ref val)).IsEmpty)
		{
			val = _dressedEquipment[(EquipmentIndex)4];
			if (((EquipmentElement)(ref val)).Item.IsBannerItem)
			{
				_dressedEquipment[(EquipmentIndex)4] = EquipmentElement.Invalid;
			}
		}
		FaceGenerationParams faceGenerationParams = BodyGen.InitBodyGenerator(false);
		faceGenerationParams.UseCache = true;
		faceGenerationParams.UseGpuMorph = true;
		SkeletonType = (SkeletonType)(BodyGen.IsFemale ? 1 : 0);
		_facegenCategory = UIResourceManager.LoadSpriteCategory("ui_facegen");
		OpenScene();
		AddCharacterEntity();
		bool openedFromMultiplayer2 = _openedFromMultiplayer;
		if (_getCurrentStageIndexAction == null || _getTotalStageCountAction == null || _getFurthestIndexAction == null)
		{
			DataSource = new FaceGenVM(BodyGen, (IFaceGeneratorHandler)(object)this, (Action<float>)OnHeightChanged, (Action)OnAgeChanged, affirmativeActionText, negativeActionText, 0, 0, 0, (Action<int>)GoToIndex, openedFromMultiplayer2, openedFromMultiplayer, filter);
		}
		else
		{
			DataSource = new FaceGenVM(BodyGen, (IFaceGeneratorHandler)(object)this, (Action<float>)OnHeightChanged, (Action)OnAgeChanged, affirmativeActionText, negativeActionText, _getCurrentStageIndexAction.Invoke(), _getTotalStageCountAction.Invoke(), _getFurthestIndexAction.Invoke(), (Action<int>)GoToIndex, true, openedFromMultiplayer, filter);
		}
		DataSource.InitializeHistory(faceGenHistory);
		DataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		DataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		DataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		DataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(56));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(57));
		DataSource.AddCameraControlInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisX")));
		DataSource.AddCameraControlInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisY")));
		DataSource.SetFaceGenerationParams(faceGenerationParams);
		DataSource.Refresh(true);
		GauntletLayer = new GauntletLayer("Facegen", 1, false);
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		((ScreenLayer)GauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		_viewMovie = GauntletLayer.LoadMovie("FaceGen", (ViewModel)(object)DataSource);
		if (!_openedFromMultiplayer)
		{
			_templateBodyProperties = new List<BodyProperties>();
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_0").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_1").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_2").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_3").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_4").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_5").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_6").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_7").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_8").GetBodyProperties((Equipment)null, -1));
			_templateBodyProperties.Add(MBObjectManager.Instance.GetObject<BasicCharacterObject>("facgen_template_test_char_9").GetBodyProperties((Equipment)null, -1));
		}
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		DataSource.SelectedGender = (BodyGen.IsFemale ? 1 : 0);
	}

	private void OpenScene()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		_facegenScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		_facegenScene.DisableStaticShadows(true);
		SceneInitializationData val = new SceneInitializationData
		{
			InitPhysicsWorld = false
		};
		_facegenScene.Read("character_menu_new", ref val, "");
		_facegenScene.SetClothSimulationState(true);
		_facegenScene.SetShadow(true);
		_facegenScene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		GameEntity obj = _facegenScene.FindEntityWithName("cradle");
		if (obj != null)
		{
			obj.SetVisibilityExcludeParents(false);
		}
		_facegenScene.DisableStaticShadows(true);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_facegenScene);
		_camera = Camera.CreateCamera();
		_defaultCameraGlobalFrame = InitCamera(_camera, new Vec3(6.45f, 5.15f, 1.75f, -1f));
		_targetCameraGlobalFrame = _defaultCameraGlobalFrame;
		SceneLayer = new SceneLayer(true, true);
		((ScreenLayer)SceneLayer).IsFocusLayer = true;
		SceneLayer.SetScene(_facegenScene);
		SceneLayer.SetCamera(_camera);
		SceneLayer.SetSceneUsesShadows(true);
		SceneLayer.SetRenderWithPostfx(true);
		SceneLayer.SetPostfxFromConfig();
		SceneLayer.SceneView.SetResolutionScaling(true);
		int num = -1;
		num &= -5;
		SceneLayer.SetPostfxConfigParams(num);
		SceneLayer.SetPostfxFromConfig();
		SceneLayer.SceneView.SetAcceptGlobalDebugRenderObjects(true);
	}

	private void AddCharacterEntity()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = _facegenScene.FindEntityWithTag("spawnpoint_player_1");
		_initialCharacterFrame = val.GetFrame();
		_initialCharacterFrame.origin.z = 0f;
		_visualToShow = null;
		_visualsBeingPrepared = new List<KeyValuePair<AgentVisuals, int>>();
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(BodyGen.Race);
		AgentVisualsData data = new AgentVisualsData().UseMorphAnims(true).Equipment(BodyGen.Character.Equipment).BodyProperties(BodyGen.Character.GetBodyProperties(BodyGen.Character.Equipment, -1))
			.Race(BodyGen.Race)
			.Frame(_initialCharacterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, BodyGen.IsFemale, "_facegen"))
			.Scene(_facegenScene)
			.Monster(baseMonsterFromRace)
			.UseTranslucency(true)
			.UseTesselation(false)
			.PrepareImmediately(true);
		_nextVisualToShow = AgentVisuals.Create(data, "facegenvisual", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		GameEntity entity = _nextVisualToShow.GetEntity();
		MBSkeletonExtensions.SetAgentActionChannel(entity.Skeleton, 1, ref ActionIndexCache.act_inventory_idle_start, 0f, -0.2f, true, 0f);
		_nextVisualToShow.SetAgentLodZeroOrMaxExternal(makeZero: true);
		entity.CheckResources(true, true);
		_nextVisualToShow.SetVisible(value: false);
		_visualsBeingPrepared.Add(new KeyValuePair<AgentVisuals, int>(_nextVisualToShow, 1));
		SceneLayer.SetFocusedShadowmap(true, ref _initialCharacterFrame.origin, 0.59999996f);
	}

	private void SetNewBodyPropertiesAndBodyGen(BodyProperties bodyProperties)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		BodyGen.CurrentBodyProperties = bodyProperties;
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	public void ResetFaceToDefault()
	{
		MBBodyProperties.ProduceNumericKeyWithDefaultValues(ref BodyGen.CurrentBodyProperties, BodyGen.Character.Equipment.EarsAreHidden, BodyGen.Character.Equipment.MouthIsHidden, BodyGen.Race, BodyGen.IsFemale ? 1 : 0, (int)BodyGen.Character.Age);
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	private void OnHeightChanged(float sliderValue)
	{
	}

	private void OnAgeChanged()
	{
	}

	[CommandLineArgumentFunction("show_debug", "facegen")]
	public static string FaceGenShowDebug(List<string> strings)
	{
		FaceGen.ShowDebugValues = !FaceGen.ShowDebugValues;
		return "FaceGen: Show Debug Values are " + (FaceGen.ShowDebugValues ? "enabled" : "disabled");
	}

	[CommandLineArgumentFunction("toggle_update_deform_keys", "facegen")]
	public static string FaceGenUpdateDeformKeys(List<string> strings)
	{
		FaceGen.UpdateDeformKeys = !FaceGen.UpdateDeformKeys;
		return "FaceGen: update deform keys is now " + (FaceGen.UpdateDeformKeys ? "enabled" : "disabled");
	}

	public bool ReadyToRender()
	{
		if (SceneLayer != null && (NativeObject)(object)SceneLayer.SceneView != (NativeObject)null)
		{
			return SceneLayer.SceneView.ReadyToRender();
		}
		return false;
	}

	public void OnTick(float dt)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		TickInput(dt);
		if (SceneLayer != null && SceneLayer.ReadyToRender())
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		if (_makeVoiceInFrames >= 0)
		{
			if (_makeVoiceInFrames == 0)
			{
				((IFaceGeneratorHandler)this).MakeVoice();
			}
			_makeVoiceInFrames--;
		}
		if (_refreshCharacterEntityNextFrame)
		{
			RefreshCharacterEntityAux();
			_refreshCharacterEntityNextFrame = false;
		}
		if (_visualToShow != null)
		{
			Skeleton skeleton = _visualToShow.GetVisuals().GetSkeleton();
			bool flag = skeleton.GetAnimationParameterAtChannel(1) > 0.6f;
			if (MBSkeletonExtensions.GetActionAtChannel(skeleton, 1) == ActionIndexCache.act_command_leftstance && flag)
			{
				MBSkeletonExtensions.SetAgentActionChannel(_visualToShow.GetEntity().Skeleton, 1, ref ActionIndexCache.act_inventory_idle, 0f, -0.2f, true, 0f);
			}
		}
		if (!_openedFromMultiplayer)
		{
			if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetFaceKeyMin"))
			{
				BodyGen.BodyPropertiesMin = BodyGen.CurrentBodyProperties;
			}
			else if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetFaceKeyMax"))
			{
				BodyGen.BodyPropertiesMax = BodyGen.CurrentBodyProperties;
			}
			else if (DebugInput.IsHotKeyPressed("Reset"))
			{
				DataSource.ExecuteReset();
			}
		}
		if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetCurFaceKeyToMin"))
		{
			BodyGen.CurrentBodyProperties = BodyGen.BodyPropertiesMin;
			SetNewBodyPropertiesAndBodyGen(BodyGen.BodyPropertiesMin);
			DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, false, 0, -1, false);
			DataSource.UpdateFacegen();
		}
		else if (DebugInput.IsHotKeyReleased("MbFaceGeneratorScreenHotkeySetCurFaceKeyToMax"))
		{
			BodyGen.CurrentBodyProperties = BodyGen.BodyPropertiesMax;
			SetNewBodyPropertiesAndBodyGen(BodyGen.BodyPropertiesMax);
			DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, false, 0, -1, false);
			DataSource.UpdateFacegen();
		}
		if (DebugInput.IsHotKeyDown("FaceGeneratorExtendedDebugKey") && DebugInput.IsHotKeyDown("MbFaceGeneratorScreenHotkeyResetFaceToDefault"))
		{
			ResetFaceToDefault();
			DataSource.SetBodyProperties(BodyGen.CurrentBodyProperties, false, 0, -1, false);
			DataSource.UpdateFacegen();
		}
		Utilities.CheckResourceModifications();
		if (DebugInput.IsHotKeyReleased("Refresh"))
		{
			((IFaceGeneratorHandler)this).RefreshCharacterEntity();
		}
		Scene facegenScene = _facegenScene;
		if (facegenScene != null)
		{
			facegenScene.Tick(dt);
		}
		if (_visualToShow != null)
		{
			_visualToShow.TickVisuals();
		}
		foreach (KeyValuePair<AgentVisuals, int> item in _visualsBeingPrepared)
		{
			item.Key.TickVisuals();
		}
		for (int i = 0; i < _visualsBeingPrepared.Count; i++)
		{
			AgentVisuals key = _visualsBeingPrepared[i].Key;
			int value = _visualsBeingPrepared[i].Value;
			key.SetVisible(value: false);
			if (!key.GetEntity().CheckResources(false, true))
			{
				continue;
			}
			if (value > 0)
			{
				_visualsBeingPrepared[i] = new KeyValuePair<AgentVisuals, int>(key, value - 1);
				continue;
			}
			if (key == _nextVisualToShow)
			{
				if (_visualToShow != null)
				{
					_visualToShow.Reset();
				}
				_visualToShow = key;
				_visualToShow.SetVisible(value: true);
				_nextVisualToShow = null;
				if (_setMorphAnimNextFrame)
				{
					MBSkeletonExtensions.SetFacialAnimation(_visualToShow.GetEntity().Skeleton, (FacialAnimChannel)0, _nextMorphAnimToSet, true, _nextMorphAnimLoopValue);
					_setMorphAnimNextFrame = false;
				}
			}
			else
			{
				_visualsBeingPrepared[i].Key.Reset();
			}
			_visualsBeingPrepared[i] = _visualsBeingPrepared[_visualsBeingPrepared.Count - 1];
			_visualsBeingPrepared.RemoveAt(_visualsBeingPrepared.Count - 1);
			i--;
		}
		SoundManager.SetListenerFrame(_camera.Frame);
		UpdateCamera(dt);
		TickLayerInputs();
	}

	public void OnFinalize()
	{
		_facegenCategory.Unload();
		ClearAgentVisuals();
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_facegenScene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
		_facegenScene.ClearAll();
		((NativeObject)_facegenScene).ManualInvalidate();
		_facegenScene = null;
		((View)SceneLayer.SceneView).SetEnable(false);
		SceneLayer.SceneView.ClearAll(true, true);
		FaceGenVM dataSource = DataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		DataSource = null;
	}

	private void TickLayerInputs()
	{
		if (IsHotKeyReleasedOnAnyLayer("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((IFaceGeneratorHandler)this).Cancel();
		}
		else if (IsHotKeyReleasedOnAnyLayer("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((IFaceGeneratorHandler)this).Done();
		}
	}

	private void TickInput(float dt)
	{
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Expected O, but got Unknown
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		DataSource.CharacterGamepadControlsEnabled = Input.IsGamepadActive && ((ScreenLayer)SceneLayer).IsHitThisFrame;
		if (((ScreenLayer)SceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == GauntletLayer)
		{
			((ScreenLayer)GauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)GauntletLayer);
			((ScreenLayer)SceneLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)SceneLayer);
		}
		else if (!((ScreenLayer)SceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == SceneLayer)
		{
			((ScreenLayer)SceneLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)SceneLayer);
			((ScreenLayer)GauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		}
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(((ScreenLayer)SceneLayer).Input.GetNormalizedMouseMoveX() * 1920f, ((ScreenLayer)SceneLayer).Input.GetNormalizedMouseMoveY() * 1080f);
		bool flag = ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("Zoom");
		bool flag2 = ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("Rotate");
		bool flag3 = ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("Ascend");
		if (flag || flag2 || flag3)
		{
			MBWindowManager.DontChangeCursorPos();
			((ScreenLayer)GauntletLayer).InputRestrictions.SetMouseVisibility(false);
		}
		else
		{
			((ScreenLayer)GauntletLayer).InputRestrictions.SetMouseVisibility(true);
		}
		float gameKeyState = ((ScreenLayer)SceneLayer).Input.GetGameKeyState(56);
		float inputValue = ((ScreenLayer)SceneLayer).Input.GetGameKeyState(57) - gameKeyState;
		float num;
		if (Input.IsGamepadActive)
		{
			NormalizeControllerInputForDeadZone(ref inputValue, 0.1f);
			num = inputValue * 5f * dt;
		}
		else
		{
			float num2 = ((ScreenLayer)SceneLayer).Input.GetDeltaMouseScroll() * -1f;
			float num3 = (flag ? (val.y * -1f) : 0f);
			num = num2 * 0.002f + num3 * 0.004f;
		}
		Vec2 val2 = ((Vec3)(ref _targetCameraGlobalFrame.origin)).AsVec2 - ((Vec3)(ref _initialCharacterFrame.origin)).AsVec2;
		float length = ((Vec2)(ref val2)).Length;
		_cameraCurrentDistanceAdder = MBMath.ClampFloat(_cameraCurrentDistanceAdder + num, 0.3f - length, 3f - length);
		float num4;
		if (Input.IsGamepadActive)
		{
			float inputValue2 = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisX");
			NormalizeControllerInputForDeadZone(ref inputValue2, 0.1f);
			num4 = inputValue2 * 400f * dt;
		}
		else
		{
			num4 = (flag2 ? val.x : 0f) * 0.2f;
		}
		_characterTargetRotation = MBMath.WrapAngle(_characterTargetRotation + num4 * (MathF.PI / 180f));
		float num5 = ((_visualToShow != null) ? _visualToShow.GetScale() : 1f);
		float num6 = 0.15f - _targetCameraGlobalFrame.origin.z;
		float num7 = 1.9f * num5 - _targetCameraGlobalFrame.origin.z;
		float num8;
		if (Input.IsGamepadActive)
		{
			float inputValue3 = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisY");
			NormalizeControllerInputForDeadZone(ref inputValue3, 0.1f);
			num8 = inputValue3 * 2f * dt;
		}
		else
		{
			num8 = (flag3 ? val.y : 0f) * 0.002f;
		}
		_cameraCurrentElevationAdder = MBMath.ClampFloat(_cameraCurrentElevationAdder + num8, num6, num7);
		if (IsHotKeyPressedOnAnyLayer("SwitchToPreviousTab"))
		{
			UISoundsHelper.PlayUISound("event:/ui/tab");
			DataSource.SelectPreviousTab();
		}
		else if (IsHotKeyPressedOnAnyLayer("SwitchToNextTab"))
		{
			UISoundsHelper.PlayUISound("event:/ui/tab");
			DataSource.SelectNextTab();
		}
		if (!((ScreenLayer)SceneLayer).Input.IsControlDown() && !((ScreenLayer)GauntletLayer).Input.IsControlDown())
		{
			return;
		}
		if (IsHotKeyPressedOnAnyLayer("Copy"))
		{
			Input.SetClipboardText(((object)Unsafe.As<BodyProperties, BodyProperties>(ref BodyGen.CurrentBodyProperties)/*cast due to .constrained prefix*/).ToString());
		}
		else if (IsHotKeyPressedOnAnyLayer("Paste"))
		{
			BodyProperties val3 = default(BodyProperties);
			if (BodyProperties.FromString(Input.GetClipboardText(), ref val3))
			{
				DataSource.SetBodyProperties(val3, !FaceGen.ShowDebugValues, 0, -1, true);
			}
			else
			{
				InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_error", (string)null)).ToString(), ((object)GameTexts.FindText("str_facegen_error_on_paste", (string)null)).ToString(), false, true, "", ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			}
		}
	}

	private void NormalizeControllerInputForDeadZone(ref float inputValue, float controllerDeadZone)
	{
		if (MathF.Abs(inputValue) < controllerDeadZone)
		{
			inputValue = 0f;
		}
		else
		{
			inputValue = (inputValue - (float)MathF.Sign(inputValue) * controllerDeadZone) / (1f - controllerDeadZone);
		}
	}

	private bool IsHotKeyReleasedOnAnyLayer(string hotkeyName)
	{
		if (!((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased(hotkeyName))
		{
			return ((ScreenLayer)SceneLayer).Input.IsHotKeyReleased(hotkeyName);
		}
		return true;
	}

	private bool IsHotKeyPressedOnAnyLayer(string hotkeyName)
	{
		if (!((ScreenLayer)GauntletLayer).Input.IsHotKeyPressed(hotkeyName))
		{
			return ((ScreenLayer)SceneLayer).Input.IsHotKeyPressed(hotkeyName);
		}
		return true;
	}

	private void RefreshCharacterEntityAux()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		SkeletonType val = SkeletonType;
		if ((int)val < 2)
		{
			val = (SkeletonType)(BodyGen.IsFemale ? 1 : 0);
		}
		_currentAgentVisualIndex = (_currentAgentVisualIndex + 1) % 2;
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(BodyGen.Race);
		AgentVisualsData data = new AgentVisualsData().UseMorphAnims(true).Scene(_facegenScene).Monster(baseMonsterFromRace)
			.UseTranslucency(true)
			.UseTesselation(false)
			.SkeletonType(val)
			.Equipment(IsDressed ? _dressedEquipment : null)
			.BodyProperties(BodyGen.CurrentBodyProperties)
			.Race(BodyGen.Race)
			.PrepareImmediately(true);
		AgentVisuals obj = _visualToShow ?? _nextVisualToShow;
		ActionIndexCache actionAtChannel = MBSkeletonExtensions.GetActionAtChannel(obj.GetEntity().Skeleton, 1);
		float animationParameterAtChannel = obj.GetVisuals().GetSkeleton().GetAnimationParameterAtChannel(1);
		_nextVisualToShow = AgentVisuals.Create(data, "facegenvisual", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_nextVisualToShow.SetAgentLodZeroOrMax(value: true);
		MBSkeletonExtensions.SetAgentActionChannel(_nextVisualToShow.GetEntity().Skeleton, 1, ref actionAtChannel, animationParameterAtChannel, -0.2f, true, 0f);
		_nextVisualToShow.GetEntity().SetEnforcedMaximumLodLevel(0);
		_nextVisualToShow.GetEntity().CheckResources(true, true);
		_nextVisualToShow.SetVisible(value: false);
		MatrixFrame initialCharacterFrame = _initialCharacterFrame;
		((Mat3)(ref initialCharacterFrame.rotation)).RotateAboutUp(_characterCurrentRotation);
		((Mat3)(ref initialCharacterFrame.rotation)).ApplyScaleLocal(_nextVisualToShow.GetScale());
		_nextVisualToShow.GetEntity().SetFrame(ref initialCharacterFrame, true);
		_nextVisualToShow.GetVisuals().GetSkeleton().SetAnimationParameterAtChannel(1, animationParameterAtChannel);
		_nextVisualToShow.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, initialCharacterFrame, true);
		_nextVisualToShow.SetVisible(value: false);
		_visualsBeingPrepared.Add(new KeyValuePair<AgentVisuals, int>(_nextVisualToShow, 1));
	}

	void IFaceGeneratorHandler.MakeVoice()
	{
		_visualToShow?.MakeRandomVoiceForFacegen();
	}

	void IFaceGeneratorHandler.MakeVoiceDelayed()
	{
		_makeVoiceInFrames = 2;
	}

	void IFaceGeneratorHandler.RefreshCharacterEntity()
	{
		_refreshCharacterEntityNextFrame = true;
	}

	void IFaceGeneratorHandler.SetFacialAnimation(string faceAnimation, bool loop)
	{
		_setMorphAnimNextFrame = true;
		_nextMorphAnimToSet = faceAnimation;
		_nextMorphAnimLoopValue = loop;
	}

	private void ClearAgentVisuals()
	{
		if (_visualToShow != null)
		{
			_visualToShow.Reset();
			_visualToShow = null;
		}
		foreach (KeyValuePair<AgentVisuals, int> item in _visualsBeingPrepared)
		{
			item.Key.Reset();
		}
		_visualsBeingPrepared.Clear();
	}

	void IFaceGeneratorHandler.Done()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		BodyGen.SaveCurrentCharacter();
		ClearAgentVisuals();
		if (Mission.Current != null)
		{
			Mission.Current.MainAgent.UpdateBodyProperties(BodyGen.CurrentBodyProperties);
			Mission.Current.MainAgent.EquipItemsFromSpawnEquipment(false, false, false, 0);
		}
		_affirmativeAction.Invoke();
	}

	void IFaceGeneratorHandler.Cancel()
	{
		_negativeAction.Invoke();
		ClearAgentVisuals();
	}

	void IFaceGeneratorHandler.ChangeToFaceCamera()
	{
		_cameraLookMode = 1;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToEyeCamera()
	{
		_cameraLookMode = 2;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToNoseCamera()
	{
		_cameraLookMode = 3;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToMouthCamera()
	{
		_cameraLookMode = 4;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToBodyCamera()
	{
		_cameraLookMode = 0;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.ChangeToHairCamera()
	{
		_cameraLookMode = 1;
		_cameraCurrentElevationAdder = 0f;
		_cameraCurrentDistanceAdder = 0f;
	}

	void IFaceGeneratorHandler.UndressCharacterEntity()
	{
		IsDressed = false;
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	void IFaceGeneratorHandler.DressCharacterEntity()
	{
		IsDressed = true;
		((IFaceGeneratorHandler)this).RefreshCharacterEntity();
	}

	void IFaceGeneratorHandler.DefaultFace()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		FaceGenerationParams faceGenerationParams = BodyGen.InitBodyGenerator(false);
		faceGenerationParams.UseCache = true;
		faceGenerationParams.UseGpuMorph = true;
		MBBodyProperties.TransformFaceKeysToDefaultFace(ref faceGenerationParams);
		DataSource.SetFaceGenerationParams(faceGenerationParams);
		DataSource.Refresh(true);
	}

	private void GoToIndex(int index)
	{
		BodyGen.SaveCurrentCharacter();
		ClearAgentVisuals();
		_goToIndexAction.Invoke(index);
	}

	public static MatrixFrame InitCamera(Camera camera, Vec3 cameraPosition)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		camera.SetFovVertical(MathF.PI / 4f, Screen.AspectRatio, 0.02f, 200f);
		return camera.Frame = Camera.ConstructCameraFromPositionElevationBearing(cameraPosition, -0.195f, 163.17f);
	}

	private void UpdateCamera(float dt)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		_characterCurrentRotation = MathF.AngleLerp(_characterCurrentRotation, _characterTargetRotation, MathF.Min(1f, 20f * dt), 1E-05f);
		_targetCameraGlobalFrame.origin = _defaultCameraGlobalFrame.origin;
		if (_visualToShow != null)
		{
			MatrixFrame initialCharacterFrame = _initialCharacterFrame;
			((Mat3)(ref initialCharacterFrame.rotation)).RotateAboutUp(_characterCurrentRotation);
			((Mat3)(ref initialCharacterFrame.rotation)).ApplyScaleLocal(_visualToShow.GetScale());
			_visualToShow.GetEntity().SetFrame(ref initialCharacterFrame, true);
			float z = _visualToShow.GetGlobalStableEyePoint(isHumanoid: true).z;
			float z2 = _visualToShow.GetGlobalStableNeckPoint(isHumanoid: true).z;
			float scale = _visualToShow.GetScale();
			Vec2 val = default(Vec2);
			switch (_cameraLookMode)
			{
			case 1:
				((Vec2)(ref val))._002Ector(6.45f, 6.75f);
				val += (val - ((Vec3)(ref _initialCharacterFrame.origin)).AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(val, z + (z - z2) * 0.75f, -1f);
				break;
			case 2:
				((Vec2)(ref val))._002Ector(6.45f, 7f);
				val += (val - ((Vec3)(ref _initialCharacterFrame.origin)).AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(val, z + (z - z2) * 0.5f, -1f);
				break;
			case 3:
				((Vec2)(ref val))._002Ector(6.45f, 7f);
				val += (val - ((Vec3)(ref _initialCharacterFrame.origin)).AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(val, z + (z - z2) * 0.25f, -1f);
				break;
			case 4:
				((Vec2)(ref val))._002Ector(6.45f, 7f);
				val += (val - ((Vec3)(ref _initialCharacterFrame.origin)).AsVec2) * (scale - 1f);
				_targetCameraGlobalFrame.origin = new Vec3(val, z - (z - z2) * 0.25f, -1f);
				break;
			}
		}
		Vec2 val2 = ((Vec3)(ref _targetCameraGlobalFrame.origin)).AsVec2 - ((Vec3)(ref _initialCharacterFrame.origin)).AsVec2;
		Vec2 val3 = ((Vec2)(ref val2)).Normalized();
		Vec3 origin = _targetCameraGlobalFrame.origin;
		((Vec3)(ref origin)).AsVec2 = ((Vec3)(ref _targetCameraGlobalFrame.origin)).AsVec2 + val3 * _cameraCurrentDistanceAdder;
		origin.z += _cameraCurrentElevationAdder;
		Camera camera = _camera;
		MatrixFrame frame = _camera.Frame;
		ref Mat3 rotation = ref frame.rotation;
		Vec3 val4 = _camera.Frame.origin * (1f - 10f * dt) + origin * 10f * dt;
		camera.Frame = new MatrixFrame(ref rotation, ref val4);
		SceneLayer.SetCamera(_camera);
	}
}
