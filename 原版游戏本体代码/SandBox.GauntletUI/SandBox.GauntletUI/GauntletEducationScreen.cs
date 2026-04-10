using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.Education;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(EducationState))]
public class GauntletEducationScreen : ScreenBase, IGameStateListener
{
	private readonly EducationState _educationState;

	private readonly Hero _child;

	private readonly PreloadHelper _preloadHelper;

	private EducationVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private bool _startedRendering;

	private Scene _characterScene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private Camera _camera;

	private List<AgentVisuals> _agentVisuals;

	private GameEntity _cradleEntity;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	private bool _isEscapeOpen;

	public SceneLayer CharacterLayer { get; private set; }

	public GauntletEducationScreen(EducationState educationState)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		_educationState = educationState;
		_child = _educationState.Child;
		_agentVisuals = new List<AgentVisuals>();
		_preloadHelper = new PreloadHelper();
	}

	private void OnOptionSelect(EducationCharacterProperties[] characterProperties)
	{
		RefreshSceneCharacters(characterProperties);
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (CharacterLayer.SceneView.ReadyToRender() && !_startedRendering)
		{
			_preloadHelper.WaitForMeshesToBeLoaded();
			LoadingWindow.DisableGlobalLoadingWindow();
			_startedRendering = true;
		}
		Scene characterScene = _characterScene;
		if (characterScene != null)
		{
			characterScene.Tick(dt);
		}
		_agentVisuals?.ForEach(delegate(AgentVisuals v)
		{
			if (v != null)
			{
				v.TickVisuals();
			}
		});
		if (_startedRendering)
		{
			if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("ToggleEscapeMenu"))
			{
				ToggleEscapeMenu();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecutePreviousStage();
			}
			else if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm") && _dataSource.CanAdvance)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteNextStage();
			}
		}
	}

	private void ToggleEscapeMenu()
	{
		if (_isEscapeOpen)
		{
			RemoveEscapeMenu();
		}
		else
		{
			OpenEscapeMenu();
		}
	}

	private void CloseEducationScreen(bool isCancel)
	{
		Game.Current.GameStateManager.PopState(0);
	}

	private void OpenScene()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		_characterScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		SceneInitializationData val = new SceneInitializationData
		{
			InitPhysicsWorld = false
		};
		_characterScene.Read("character_menu_new", ref val, "");
		_characterScene.SetShadow(true);
		_characterScene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_characterScene);
		_camera = Camera.CreateCamera();
		_camera.SetFovVertical(MathF.PI / 4f, Screen.AspectRatio, 0.02f, 200f);
		_camera.Frame = Camera.ConstructCameraFromPositionElevationBearing(new Vec3(6.45f, 4.35f, 1.6f, -1f), -0.195f, 163.17f);
		CharacterLayer = new SceneLayer(true, true);
		CharacterLayer.SetScene(_characterScene);
		CharacterLayer.SetCamera(_camera);
		CharacterLayer.SetSceneUsesShadows(true);
		CharacterLayer.SetRenderWithPostfx(true);
		CharacterLayer.SetPostfxFromConfig();
		CharacterLayer.SceneView.SetResolutionScaling(true);
		if (!((ScreenLayer)CharacterLayer).Input.IsCategoryRegistered(HotKeyManager.GetCategory("FaceGenHotkeyCategory")))
		{
			((ScreenLayer)CharacterLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		}
		int num = -1;
		num &= -5;
		CharacterLayer.SetPostfxConfigParams(num);
		CharacterLayer.SetPostfxFromConfig();
		GameEntity obj = _characterScene.FindEntityWithName("_to_carry_bd_basket_a");
		if (obj != null)
		{
			obj.SetVisibilityExcludeParents(false);
		}
		GameEntity obj2 = _characterScene.FindEntityWithName("_to_carry_merchandise_hides_b");
		if (obj2 != null)
		{
			obj2.SetVisibilityExcludeParents(false);
		}
		GameEntity obj3 = _characterScene.FindEntityWithName("_to_carry_foods_basket_apple");
		if (obj3 != null)
		{
			obj3.SetVisibilityExcludeParents(false);
		}
		GameEntity obj4 = _characterScene.FindEntityWithName("_to_carry_bd_fabric_c");
		if (obj4 != null)
		{
			obj4.SetVisibilityExcludeParents(false);
		}
		GameEntity obj5 = _characterScene.FindEntityWithName("notebook");
		if (obj5 != null)
		{
			obj5.SetVisibilityExcludeParents(false);
		}
		GameEntity obj6 = _characterScene.FindEntityWithName("baby");
		if (obj6 != null)
		{
			obj6.SetVisibilityExcludeParents(false);
		}
		GameEntity obj7 = _characterScene.FindEntityWithName("blacksmith_hammer");
		if (obj7 != null)
		{
			obj7.SetVisibilityExcludeParents(false);
		}
		_cradleEntity = _characterScene.FindEntityWithName("cradle");
		GameEntity cradleEntity = _cradleEntity;
		if (cradleEntity != null)
		{
			cradleEntity.SetVisibilityExcludeParents(false);
		}
	}

	private void RefreshSceneCharacters(EducationCharacterProperties[] characterProperties)
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		List<float> list = new List<float>();
		GameEntity cradleEntity = _cradleEntity;
		if (cradleEntity != null)
		{
			cradleEntity.SetVisibilityExcludeParents(false);
		}
		if (_agentVisuals != null)
		{
			foreach (AgentVisuals agentVisual in _agentVisuals)
			{
				Skeleton skeleton = agentVisual.GetVisuals().GetSkeleton();
				list.Add(skeleton.GetAnimationParameterAtChannel(0));
				agentVisual.Reset();
			}
			_agentVisuals.Clear();
		}
		if (characterProperties == null || Extensions.IsEmpty<EducationCharacterProperties>((IEnumerable<EducationCharacterProperties>)characterProperties))
		{
			return;
		}
		bool flag = characterProperties.Length == 1;
		string text = "";
		for (int i = 0; i < characterProperties.Length; i++)
		{
			if (flag)
			{
				text = "spawnpoint_player_1";
			}
			else
			{
				switch (i)
				{
				case 0:
					text = "spawnpoint_player_brother_stage";
					break;
				case 1:
					text = "spawnpoint_brother_brother_stage";
					break;
				}
			}
			MatrixFrame frame = _characterScene.FindEntityWithTag(text).GetFrame();
			frame.origin.z = 0f;
			string text2 = "act_inventory_idle_start";
			if (!string.IsNullOrWhiteSpace(characterProperties[i].ActionId))
			{
				text2 = characterProperties[i].ActionId;
			}
			string prefabId = characterProperties[i].PrefabId;
			bool useOffHand = characterProperties[i].UseOffHand;
			bool flag2 = false;
			Equipment val = characterProperties[i].Equipment.Clone(false);
			if (!string.IsNullOrEmpty(prefabId) && Game.Current.ObjectManager.GetObject<ItemObject>(prefabId) != null)
			{
				ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(prefabId);
				val.AddEquipmentToSlotWithoutAgent((EquipmentIndex)(!useOffHand), new EquipmentElement(val2, (ItemModifier)null, (ItemObject)null, false));
				flag2 = true;
			}
			AgentVisuals val3 = AgentVisuals.Create(CreateAgentVisual(characterProperties[i].Character, frame, val, text2, _characterScene, _child.Culture), "facegenvisual0", false, false, false);
			val3.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, frame, true);
			if (!string.IsNullOrWhiteSpace(text2))
			{
				ActionIndexCache val4 = ActionIndexCache.Create(text2);
				MBSkeletonExtensions.SetAgentActionChannel(val3.GetVisuals().GetSkeleton(), 0, ref val4, 0f, -0.2f, true, 0f);
			}
			if (!flag2 && !string.IsNullOrEmpty(prefabId) && GameEntity.Instantiate(_characterScene, prefabId, true, true, "") != (GameEntity)null)
			{
				val3.AddPrefabToAgentVisualBoneByRealBoneIndex(prefabId, ((EducationCharacterProperties)(ref characterProperties[i])).GetUsedHandBoneIndex());
			}
			CharacterLayer.SetFocusedShadowmap(true, ref frame.origin, 0.59999996f);
			_agentVisuals.Add(val3);
			if (_child.Age < 5f && _cradleEntity != (GameEntity)null)
			{
				MatrixFrame frame2 = _cradleEntity.GetFrame();
				MatrixFrame val5 = new MatrixFrame(ref frame2.rotation, ref frame.origin);
				_cradleEntity.SetFrame(ref val5, true);
				_cradleEntity.SetVisibilityExcludeParents(true);
			}
		}
	}

	private void PreloadCharactersAndEquipment(List<BasicCharacterObject> characters, List<Equipment> equipments)
	{
		_preloadHelper.PreloadCharacters(characters);
		_preloadHelper.PreloadEquipments(equipments);
	}

	void IGameStateListener.OnActivate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_gauntletLayer = new GauntletLayer("EducationScreen", 1, false);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		OpenScene();
		((ScreenBase)this).AddLayer((ScreenLayer)(object)CharacterLayer);
		_dataSource = new EducationVM(_educationState.Child, (Action<bool>)CloseEducationScreen, (Action<EducationCharacterProperties[]>)OnOptionSelect, (Action<List<BasicCharacterObject>, List<Equipment>>)PreloadCharactersAndEquipment);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_gauntletLayer.LoadMovie("EducationScreen", (ViewModel)(object)_dataSource);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)12));
		LoadingWindow.EnableGlobalLoadingWindow();
	}

	void IGameStateListener.OnDeactivate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		((ScreenBase)this).OnDeactivate();
		((ScreenBase)this).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		LoadingWindow.EnableGlobalLoadingWindow();
	}

	void IGameStateListener.OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		((View)CharacterLayer.SceneView).SetEnable(false);
		CharacterLayer.SceneView.ClearAll(true, true);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
		_agentVisuals?.ForEach(delegate(AgentVisuals v)
		{
			if (v != null)
			{
				v.Reset();
			}
		});
		_agentVisuals = null;
		CharacterLayer = null;
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_characterScene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
		Scene characterScene = _characterScene;
		if (characterScene != null)
		{
			((NativeObject)characterScene).ManualInvalidate();
		}
		_characterScene = null;
	}

	void IGameStateListener.OnInitialize()
	{
	}

	private static AgentVisualsData CreateAgentVisual(CharacterObject character, MatrixFrame characterFrame, Equipment equipment, string actionName, Scene scene, CultureObject childsCulture)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		ActionIndexCache val = ActionIndexCache.Create(actionName);
		BodyProperties bodyProperties2 = default(BodyProperties);
		if (((BasicCharacterObject)character).Age < (float)Campaign.Current.Models.AgeModel.BecomeInfantAge)
		{
			BodyProperties bodyProperties = ((BasicCharacterObject)character).GetBodyProperties(equipment, -1);
			((BodyProperties)(ref bodyProperties2))._002Ector(new DynamicBodyProperties(3f, ((BodyProperties)(ref bodyProperties)).Weight, ((BodyProperties)(ref bodyProperties)).Build), ((BodyProperties)(ref bodyProperties)).StaticProperties);
		}
		else
		{
			bodyProperties2 = ((BasicCharacterObject)character).GetBodyProperties(equipment, -1);
		}
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)character).Race);
		AgentVisualsData val2 = new AgentVisualsData().UseMorphAnims(true).Equipment(equipment).BodyProperties(bodyProperties2)
			.Frame(characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, ((BasicCharacterObject)character).IsFemale, "_facegen"))
			.ActionCode(ref val)
			.Scene(scene)
			.Monster(baseMonsterFromRace)
			.PrepareImmediately(true)
			.UseTranslucency(true)
			.UseTesselation(true)
			.RightWieldedItemIndex(0)
			.LeftWieldedItemIndex(1)
			.SkeletonType((SkeletonType)(((BasicCharacterObject)character).IsFemale ? 1 : 0));
		if (childsCulture != null)
		{
			val2.ClothColor1(Clan.PlayerClan.Color);
			val2.ClothColor2(Clan.PlayerClan.Color2);
		}
		return val2;
	}

	private void OpenEscapeMenu()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		_escapeMenuDatasource = new EscapeMenuVM((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems(), (TextObject)null);
		_escapeMenuMovie = _gauntletLayer.LoadMovie("EscapeMenu", (ViewModel)(object)_escapeMenuDatasource);
		_isEscapeOpen = true;
	}

	private void RemoveEscapeMenu()
	{
		_gauntletLayer.ReleaseMovie(_escapeMenuMovie);
		_escapeMenuDatasource = null;
		_escapeMenuMovie = null;
		_isEscapeOpen = false;
	}

	private List<EscapeMenuItemVM> GetEscapeMenuItems()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Expected O, but got Unknown
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Expected O, but got Unknown
		TextObject ironmanDisabledReason = GameTexts.FindText("str_pause_menu_disabled_hint", "IronmanMode");
		TextObject educationDisabledReason = GameTexts.FindText("str_pause_menu_disabled_hint", "Education");
		return new List<EscapeMenuItemVM>
		{
			new EscapeMenuItemVM(new TextObject("{=UAD5gWKK}Return to Education", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				RemoveEscapeMenu();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), true),
			new EscapeMenuItemVM(new TextObject("{=PXT6aA4J}Campaign Options", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, educationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, educationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=bV75iwKa}Save", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, educationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=e0KdfaNe}Save As", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, educationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=9NuttOBC}Load", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, educationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=AbEh2y8o}Save And Exit", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, educationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=RamV6yLM}Exit to Main Menu", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				RemoveEscapeMenu();
				MBGameManager.EndGame();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(CampaignOptions.IsIronmanMode, ironmanDisabledReason)), false)
		};
	}
}
