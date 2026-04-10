using System;
using System.Collections.Generic;
using SandBox.View.CharacterCreation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.CharacterCreation;

[CharacterCreationStageView(typeof(CharacterCreationNarrativeStage))]
public class CharacterCreationNarrativeStageView : CharacterCreationStageViewBase
{
	protected readonly TextObject _affirmativeActionText;

	protected readonly TextObject _negativeActionText;

	private GauntletMovieIdentifier _movie;

	private GauntletLayer GauntletLayer;

	private CharacterCreationNarrativeStageVM _dataSource;

	private readonly CharacterCreationManager _characterCreationManager;

	private Scene _characterScene;

	private Camera _camera;

	private List<AgentVisuals> _currentMenuAgentVisuals;

	private List<GameEntity> _currentMenuMountEntities;

	private bool _isAgentVisualsDirty;

	private bool _isAgentVisualVisibilitiesDirty;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	public SceneLayer CharacterLayer { get; private set; }

	public CharacterCreationNarrativeStageView(CharacterCreationManager characterCreationManager, ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, ControlCharacterCreationStage onRefresh, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction, ControlCharacterCreationStageReturnInt getTotalStageCountAction, ControlCharacterCreationStageReturnInt getFurthestIndexAction, ControlCharacterCreationStageWithInt goToIndexAction)
		: base(affirmativeAction, negativeAction, onRefresh, getCurrentStageIndexAction, getTotalStageCountAction, getFurthestIndexAction, goToIndexAction)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		_characterCreationManager = characterCreationManager;
		_affirmativeActionText = affirmativeActionText;
		_negativeActionText = negativeActionText;
		_currentMenuAgentVisuals = new List<AgentVisuals>();
		_currentMenuMountEntities = new List<GameEntity>();
		GauntletLayer = new GauntletLayer("CharacterCreationNarrative", 1, false);
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)GauntletLayer).IsFocusLayer = true;
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		_characterCreationManager.StartNarrativeStage();
		_dataSource = new CharacterCreationNarrativeStageVM(_characterCreationManager, (Action)NextStage, _affirmativeActionText, (Action)PreviousStage, _negativeActionText, (Action)OnMenuChanged)
		{
			OnOptionSelection = OnSelectionChanged
		};
		_dataSource.RefreshMenu();
		CreateHotKeyVisuals();
		_movie = GauntletLayer.LoadMovie("CharacterCreationNarrativeStage", (ViewModel)(object)_dataSource);
	}

	public override void SetGenericScene(Scene scene)
	{
		OpenScene(scene);
		RefreshAgentVisuals();
	}

	private void CreateHotKeyVisuals()
	{
		CharacterCreationNarrativeStageVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		}
		CharacterCreationNarrativeStageVM dataSource2 = _dataSource;
		if (dataSource2 != null)
		{
			dataSource2.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		}
	}

	private void OpenScene(Scene cachedScene)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		_characterScene = cachedScene;
		_characterScene.SetShadow(true);
		_characterScene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_characterScene.SetDoNotWaitForLoadingStatesToRender(true);
		_characterScene.DisableStaticShadows(true);
		_camera = Camera.CreateCamera();
		BodyGeneratorView.InitCamera(_camera, _cameraPosition);
		CharacterLayer = new SceneLayer(false, true);
		CharacterLayer.SetScene(_characterScene);
		CharacterLayer.SetCamera(_camera);
		CharacterLayer.SetSceneUsesShadows(true);
		CharacterLayer.SetRenderWithPostfx(true);
		CharacterLayer.SetPostfxFromConfig();
		CharacterLayer.SceneView.SetResolutionScaling(true);
		int num = -1;
		num &= -5;
		CharacterLayer.SetPostfxConfigParams(num);
		CharacterLayer.SetPostfxFromConfig();
		GameEntity obj = _characterScene.FindEntityWithName("cradle");
		if (obj != null)
		{
			obj.SetVisibilityExcludeParents(false);
		}
	}

	private void RefreshAgentVisuals()
	{
		if ((NativeObject)(object)_characterScene == (NativeObject)null)
		{
			_isAgentVisualsDirty = true;
			return;
		}
		ClearCharacterVisuals();
		ClearMountEntities();
		NarrativeMenu currentMenu = _characterCreationManager.CurrentMenu;
		for (int i = 0; i < currentMenu.Characters.Count; i++)
		{
			NarrativeMenuCharacter val = currentMenu.Characters[i];
			if (val.IsHuman)
			{
				SpawnHumanNarrativeMenuCharacter(val);
			}
			else
			{
				SpawnNonHumanNarrativeMenuCharacter(val);
			}
		}
		_isAgentVisualsDirty = false;
		_isAgentVisualVisibilitiesDirty = true;
	}

	private void SpawnHumanNarrativeMenuCharacter(NarrativeMenuCharacter character)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		GameEntity obj = _characterScene.FindEntityWithTag(character.SpawnPointEntityId);
		MatrixFrame val = ((obj != null) ? obj.GetGlobalFrame() : MatrixFrame.Identity);
		val.origin.z = 0f;
		AgentVisuals val2 = AgentVisuals.Create(CreateAgentVisual(character, val), "facegenvisual" + character.StringId, false, false, false);
		val2.SetVisible(false);
		_currentMenuAgentVisuals.Add(val2);
		val2.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(MBRandom.RandomFloat, val, true);
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(character.Race);
		if (character.IsHuman)
		{
			if (!string.IsNullOrEmpty(character.Item1Id) && GameEntity.Instantiate(_characterScene, character.Item1Id, true, true, "") != (GameEntity)null)
			{
				val2.AddPrefabToAgentVisualBoneByRealBoneIndex(character.Item1Id, baseMonsterFromRace.MainHandItemBoneIndex);
			}
			if (!string.IsNullOrEmpty(character.Item2Id) && GameEntity.Instantiate(_characterScene, character.Item2Id, true, true, "") != (GameEntity)null)
			{
				val2.AddPrefabToAgentVisualBoneByRealBoneIndex(character.Item2Id, baseMonsterFromRace.OffHandItemBoneIndex);
			}
		}
		val2.SetAgentLodZeroOrMax(true);
		val2.GetEntity().SetEnforcedMaximumLodLevel(0);
		val2.GetEntity().CheckResources(true, true);
		CharacterLayer.SetFocusedShadowmap(true, ref val.origin, 0.59999996f);
	}

	private void SpawnNonHumanNarrativeMenuCharacter(NarrativeMenuCharacter character)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		ClearMountEntities();
		GameEntity obj = _characterScene.FindEntityWithTag(character.SpawnPointEntityId);
		ItemObject val = Game.Current.ObjectManager.GetObject<ItemObject>(character.Item1Id);
		HorseComponent horseComponent = val.HorseComponent;
		Monster monster = horseComponent.Monster;
		GameEntity val2 = GameEntity.CreateEmpty(_characterScene, true, true, true);
		AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(monster, MBGlobals.GetActionSet(horseComponent.Monster.ActionSetCode), 1f, false);
		GameEntityExtensions.CreateSkeletonWithActionSet(val2, ref val3);
		ActionIndexCache val4 = ActionIndexCache.Create(character.AnimationId);
		MBSkeletonExtensions.SetAgentActionChannel(val2.Skeleton, 0, ref val4, 0f, -0.2f, true, 0f);
		ItemObject val5 = Game.Current.ObjectManager.GetObject<ItemObject>(character.Item2Id);
		MountVisualCreator.AddMountMeshToEntity(val2, val, val5, ((object)character.MountCreationKey).ToString(), (Agent)null);
		MatrixFrame globalFrame = obj.GetGlobalFrame();
		val2.SetFrame(ref globalFrame, true);
		val2.SetVisibilityExcludeParents(false);
		val2.SetEnforcedMaximumLodLevel(0);
		val2.CheckResources(true, false);
		_currentMenuMountEntities.Add(val2);
	}

	private void ClearCharacterVisuals()
	{
		for (int i = 0; i < _currentMenuAgentVisuals.Count; i++)
		{
			_currentMenuAgentVisuals[i].Reset();
		}
		_currentMenuAgentVisuals.Clear();
	}

	private void ClearMountEntities()
	{
		for (int i = 0; i < _currentMenuMountEntities.Count; i++)
		{
			_currentMenuMountEntities[i].Remove(116);
		}
		_currentMenuMountEntities.Clear();
	}

	private AgentVisualsData CreateAgentVisual(NarrativeMenuCharacter character, MatrixFrame characterFrame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected I4, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected I4, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		EquipmentIndex val = character.RightHandEquipmentIndex;
		EquipmentIndex val2 = character.LeftHandEquipmentIndex;
		MBEquipmentRoster equipment = character.Equipment;
		Equipment val3 = ((equipment != null) ? equipment.DefaultEquipment.Clone(false) : null);
		if (character.IsHuman)
		{
			if (!string.IsNullOrEmpty(character.Item1Id))
			{
				ItemObject val4 = Game.Current.ObjectManager.GetObject<ItemObject>(character.Item1Id);
				if (val4 != null)
				{
					val = (EquipmentIndex)0;
					val3.AddEquipmentToSlotWithoutAgent(val, new EquipmentElement(val4, (ItemModifier)null, (ItemObject)null, false));
				}
			}
			if (!string.IsNullOrEmpty(character.Item2Id))
			{
				ItemObject val5 = Game.Current.ObjectManager.GetObject<ItemObject>(character.Item2Id);
				if (val5 != null)
				{
					val2 = (EquipmentIndex)1;
					val3.AddEquipmentToSlotWithoutAgent(val2, new EquipmentElement(val5, (ItemModifier)null, (ItemObject)null, false));
				}
			}
		}
		ActionIndexCache val6 = ActionIndexCache.Create(character.AnimationId);
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(character.Race);
		AgentVisualsData val7 = new AgentVisualsData().UseMorphAnims(true).Equipment(val3).BodyProperties(character.BodyProperties)
			.Frame(characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, character.IsFemale, "_facegen"))
			.ActionCode(ref val6)
			.Scene(_characterScene)
			.Monster(baseMonsterFromRace)
			.UseTranslucency(true)
			.UseTesselation(true)
			.RightWieldedItemIndex((int)val)
			.LeftWieldedItemIndex((int)val2)
			.Race(((BasicCharacterObject)CharacterObject.PlayerCharacter).Race)
			.SkeletonType((SkeletonType)(character.IsFemale ? 1 : 0));
		CharacterCreationContent characterCreationContent = ((CharacterCreationState)GameStateManager.Current.ActiveState).CharacterCreationManager.CharacterCreationContent;
		if (characterCreationContent.SelectedCulture != null)
		{
			val7.ClothColor1(((BasicCultureObject)characterCreationContent.SelectedCulture).Color);
			val7.ClothColor2(((BasicCultureObject)characterCreationContent.SelectedCulture).Color2);
		}
		return val7;
	}

	private void OnMenuChanged()
	{
		RefreshAgentVisuals();
	}

	private void OnSelectionChanged()
	{
		RefreshAgentVisuals();
	}

	public override void Tick(float dt)
	{
		base.Tick(dt);
		HandleEscapeMenu(this, (ScreenLayer)(object)CharacterLayer);
		if ((NativeObject)(object)_characterScene != (NativeObject)null)
		{
			if (_isAgentVisualsDirty)
			{
				RefreshAgentVisuals();
			}
			_characterScene.Tick(dt);
		}
		bool flag = _currentMenuAgentVisuals.Count > 0 || _currentMenuMountEntities.Count > 0;
		for (int i = 0; i < _currentMenuAgentVisuals.Count; i++)
		{
			AgentVisuals obj = _currentMenuAgentVisuals[i];
			obj.TickVisuals();
			if (!obj.GetEntity().CheckResources(false, true))
			{
				flag = false;
			}
		}
		for (int j = 0; j < _currentMenuMountEntities.Count; j++)
		{
			GameEntity val = _currentMenuMountEntities[j];
			if (val != (GameEntity)null && !val.CheckResources(false, true))
			{
				flag = false;
			}
		}
		if (_isAgentVisualVisibilitiesDirty && flag)
		{
			for (int k = 0; k < _currentMenuAgentVisuals.Count; k++)
			{
				_currentMenuAgentVisuals[k].SetVisible(true);
			}
			for (int l = 0; l < _currentMenuMountEntities.Count; l++)
			{
				_currentMenuMountEntities[l].SetVisibilityExcludeParents(true);
			}
			_isAgentVisualVisibilitiesDirty = false;
		}
		HandleLayerInput();
	}

	private void HandleLayerInput()
	{
		if (((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((CharacterCreationStageBaseVM)_dataSource).OnPreviousStage();
		}
		else if (((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased("Confirm") && ((CharacterCreationStageBaseVM)_dataSource).CanAdvance)
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((CharacterCreationStageBaseVM)_dataSource).OnNextStage();
		}
	}

	public override void NextStage()
	{
		ClearMountEntities();
		_affirmativeAction.Invoke();
	}

	public override void PreviousStage()
	{
		ClearMountEntities();
		_negativeAction.Invoke();
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		ClearCharacterVisuals();
		ClearMountEntities();
		((View)CharacterLayer.SceneView).SetEnable(false);
		CharacterLayer.SceneView.ClearAll(false, false);
		_currentMenuAgentVisuals = null;
		GauntletLayer = null;
		CharacterCreationNarrativeStageVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		CharacterLayer = null;
		_characterScene = null;
	}

	public override int GetVirtualStageCount()
	{
		return _characterCreationManager.CharacterCreationMenuCount;
	}

	public override IEnumerable<ScreenLayer> GetLayers()
	{
		return new List<ScreenLayer>
		{
			(ScreenLayer)(object)CharacterLayer,
			(ScreenLayer)(object)GauntletLayer
		};
	}

	public override void LoadEscapeMenuMovie()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		_escapeMenuDatasource = new EscapeMenuVM((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems(this), (TextObject)null);
		_escapeMenuMovie = GauntletLayer.LoadMovie("EscapeMenu", (ViewModel)(object)_escapeMenuDatasource);
	}

	public override void ReleaseEscapeMenuMovie()
	{
		GauntletLayer.ReleaseMovie(_escapeMenuMovie);
		_escapeMenuDatasource = null;
		_escapeMenuMovie = null;
	}
}
