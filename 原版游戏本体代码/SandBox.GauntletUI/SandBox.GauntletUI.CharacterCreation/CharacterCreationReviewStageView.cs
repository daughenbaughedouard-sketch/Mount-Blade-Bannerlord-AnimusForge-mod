using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.View.CharacterCreation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
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

[CharacterCreationStageView(typeof(CharacterCreationReviewStage))]
public class CharacterCreationReviewStageView : CharacterCreationStageViewBase
{
	protected readonly TextObject _affirmativeActionText;

	protected readonly TextObject _negativeActionText;

	private readonly GauntletMovieIdentifier _movie;

	private GauntletLayer GauntletLayer;

	private CharacterCreationReviewStageVM _dataSource;

	private readonly CharacterCreationManager _characterCreationManager;

	private Scene _characterScene;

	private Camera _camera;

	private MatrixFrame _initialCharacterFrame;

	private AgentVisuals _agentVisuals;

	private GameEntity _mountEntity;

	private float _charRotationAmount;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	public SceneLayer CharacterLayer { get; private set; }

	public CharacterCreationReviewStageView(CharacterCreationManager characterCreationManager, ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, ControlCharacterCreationStage onRefresh, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction, ControlCharacterCreationStageReturnInt getTotalStageCountAction, ControlCharacterCreationStageReturnInt getFurthestIndexAction, ControlCharacterCreationStageWithInt goToIndexAction)
		: base(affirmativeAction, negativeAction, onRefresh, getCurrentStageIndexAction, getTotalStageCountAction, getFurthestIndexAction, goToIndexAction)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		_characterCreationManager = characterCreationManager;
		_affirmativeActionText = new TextObject("{=Rvr1bcu8}Next", (Dictionary<string, object>)null);
		_negativeActionText = negativeActionText;
		GauntletLayer = new GauntletLayer("CharacterCreationReview", 1, false);
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)GauntletLayer).IsFocusLayer = true;
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		bool flag = _characterCreationManager.GetStage<CharacterCreationBannerEditorStage>() != null && _characterCreationManager.GetStage<CharacterCreationClanNamingStage>() != null;
		_dataSource = new CharacterCreationReviewStageVM(_characterCreationManager, (Action)NextStage, _affirmativeActionText, (Action)PreviousStage, _negativeActionText, flag);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		GameAxisKey val = ((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisX"));
		_dataSource.AddCameraControlInputKey(val, Module.CurrentModule.GlobalTextManager.FindText("str_key_name", typeof(FaceGenHotkeyCategory).Name + "_" + val.Id));
		_movie = GauntletLayer.LoadMovie("CharacterCreationReviewStage", (ViewModel)(object)_dataSource);
	}

	public override void SetGenericScene(Scene scene)
	{
		OpenScene(scene);
		AddCharacterEntity();
		RefreshMountEntity();
	}

	private void OpenScene(Scene cachedScene)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		_characterScene = cachedScene;
		_characterScene.SetShadow(true);
		_characterScene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		GameEntity obj = _characterScene.FindEntityWithName("cradle");
		if (obj != null)
		{
			obj.SetVisibilityExcludeParents(false);
		}
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
		((ScreenLayer)CharacterLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		((ScreenLayer)CharacterLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
	}

	private void AddCharacterEntity()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = _characterScene.FindEntityWithTag("spawnpoint_player_1");
		_initialCharacterFrame = val.GetFrame();
		_initialCharacterFrame.origin.z = 0f;
		CharacterObject characterObject = Hero.MainHero.CharacterObject;
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)characterObject).Race);
		AgentVisualsData val2 = new AgentVisualsData().UseMorphAnims(true).Equipment(((BasicCharacterObject)characterObject).Equipment).BodyProperties(((BasicCharacterObject)characterObject).GetBodyProperties(((BasicCharacterObject)characterObject).Equipment, -1))
			.SkeletonType((SkeletonType)(((BasicCharacterObject)characterObject).IsFemale ? 1 : 0))
			.Frame(_initialCharacterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, ((BasicCharacterObject)characterObject).IsFemale, "_facegen"))
			.ActionCode(ref ActionIndexCache.act_childhood_schooled)
			.Scene(_characterScene)
			.Race(((BasicCharacterObject)characterObject).Race)
			.Monster(baseMonsterFromRace)
			.UseTranslucency(true)
			.UseTesselation(true);
		GameState activeState = GameStateManager.Current.ActiveState;
		CharacterCreationContent characterCreationContent = ((CharacterCreationState)((activeState is CharacterCreationState) ? activeState : null)).CharacterCreationManager.CharacterCreationContent;
		Banner selectedBanner = characterCreationContent.SelectedBanner;
		CultureObject selectedCulture = characterCreationContent.SelectedCulture;
		if (selectedBanner != null)
		{
			val2.ClothColor1(selectedBanner.GetPrimaryColor());
			val2.ClothColor2(selectedBanner.GetFirstIconColor());
		}
		else if (characterCreationContent.SelectedCulture != null)
		{
			val2.ClothColor1(((BasicCultureObject)selectedCulture).Color);
			val2.ClothColor2(((BasicCultureObject)selectedCulture).Color2);
		}
		_agentVisuals = AgentVisuals.Create(val2, "facegenvisual", false, false, true);
		CharacterLayer.SetFocusedShadowmap(true, ref _initialCharacterFrame.origin, 0.59999996f);
	}

	private void RefreshCharacterEntityFrame()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (_agentVisuals != null)
		{
			MatrixFrame initialCharacterFrame = _initialCharacterFrame;
			((Mat3)(ref initialCharacterFrame.rotation)).RotateAboutUp(_charRotationAmount);
			((Mat3)(ref initialCharacterFrame.rotation)).ApplyScaleLocal(_agentVisuals.GetScale());
			_agentVisuals.GetEntity().SetFrame(ref initialCharacterFrame, true);
		}
	}

	private void RefreshMountEntity()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		RemoveMount();
		if (((BasicCharacterObject)CharacterObject.PlayerCharacter).HasMount())
		{
			EquipmentElement val = ((BasicCharacterObject)CharacterObject.PlayerCharacter).Equipment[(EquipmentIndex)10];
			ItemObject item = ((EquipmentElement)(ref val)).Item;
			MountCreationKey randomMountKey = MountCreationKey.GetRandomMountKey(item, ((BasicCharacterObject)CharacterObject.PlayerCharacter).GetMountKeySeed());
			GameEntity obj = _characterScene.FindEntityWithTag("spawnpoint_mount_1");
			HorseComponent horseComponent = item.HorseComponent;
			Monster monster = horseComponent.Monster;
			_mountEntity = GameEntity.CreateEmpty(_characterScene, true, true, true);
			AnimationSystemData val2 = MonsterExtensions.FillAnimationSystemData(monster, MBGlobals.GetActionSet(horseComponent.Monster.ActionSetCode), 1f, false);
			GameEntityExtensions.CreateSkeletonWithActionSet(_mountEntity, ref val2);
			MBSkeletonExtensions.SetAgentActionChannel(_mountEntity.Skeleton, 0, ref ActionIndexCache.act_inventory_idle_start, 0f, -0.2f, true, 0f);
			val = ((BasicCharacterObject)CharacterObject.PlayerCharacter).Equipment[(EquipmentIndex)11];
			ItemObject item2 = ((EquipmentElement)(ref val)).Item;
			MountVisualCreator.AddMountMeshToEntity(_mountEntity, item, item2, ((object)randomMountKey).ToString(), (Agent)null);
			MatrixFrame globalFrame = obj.GetGlobalFrame();
			_mountEntity.SetFrame(ref globalFrame, true);
			_agentVisuals.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, _initialCharacterFrame, true);
		}
	}

	private void RemoveMount()
	{
		if (_mountEntity != (GameEntity)null)
		{
			_mountEntity.Remove(118);
		}
		_mountEntity = null;
	}

	public override void Tick(float dt)
	{
		base.Tick(dt);
		HandleEscapeMenu(this, (ScreenLayer)(object)CharacterLayer);
		Scene characterScene = _characterScene;
		if (characterScene != null)
		{
			characterScene.Tick(dt);
		}
		AgentVisuals agentVisuals = _agentVisuals;
		if (agentVisuals != null)
		{
			agentVisuals.TickVisuals();
		}
		TickInput(dt);
		HandleLayerInput();
	}

	private void HandleLayerInput()
	{
		if (IsHotKeyReleasedOnAnyLayer("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((CharacterCreationStageBaseVM)_dataSource).OnPreviousStage();
		}
		else if (IsHotKeyReleasedOnAnyLayer("Confirm") && ((CharacterCreationStageBaseVM)_dataSource).CanAdvance)
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((CharacterCreationStageBaseVM)_dataSource).OnNextStage();
		}
	}

	private void TickInput(float dt)
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		_dataSource.CharacterGamepadControlsEnabled = Input.IsGamepadActive && ((ScreenLayer)CharacterLayer).IsHitThisFrame;
		if (((ScreenLayer)CharacterLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == GauntletLayer)
		{
			((ScreenLayer)GauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)GauntletLayer);
			((ScreenLayer)CharacterLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)CharacterLayer);
		}
		else if (!((ScreenLayer)CharacterLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == CharacterLayer)
		{
			((ScreenLayer)CharacterLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)CharacterLayer);
			((ScreenLayer)GauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		}
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(((ScreenLayer)CharacterLayer).Input.GetNormalizedMouseMoveX() * 1920f, ((ScreenLayer)CharacterLayer).Input.GetNormalizedMouseMoveY() * 1080f);
		bool flag = ((ScreenLayer)CharacterLayer).Input.IsHotKeyDown("Rotate");
		if (flag)
		{
			MBWindowManager.DontChangeCursorPos();
			((ScreenLayer)GauntletLayer).InputRestrictions.SetMouseVisibility(false);
		}
		else
		{
			((ScreenLayer)GauntletLayer).InputRestrictions.SetMouseVisibility(true);
		}
		float num;
		if (Input.IsGamepadActive)
		{
			float inputValue = ((ScreenLayer)CharacterLayer).Input.GetGameKeyAxis("CameraAxisX");
			NormalizeControllerInputForDeadZone(ref inputValue, 0.1f);
			num = inputValue * 400f * dt;
		}
		else
		{
			num = (flag ? val.x : 0f) * 0.2f;
		}
		_charRotationAmount = MBMath.WrapAngle(_charRotationAmount + num * (MathF.PI / 180f));
		RefreshCharacterEntityFrame();
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
			return ((ScreenLayer)CharacterLayer).Input.IsHotKeyReleased(hotkeyName);
		}
		return true;
	}

	public override void NextStage()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		TextObject val = GameTexts.FindText("str_generic_character_firstname", (string)null);
		val.SetTextVariable("CHARACTER_FIRSTNAME", new TextObject(_dataSource.Name, (Dictionary<string, object>)null));
		TextObject val2 = GameTexts.FindText("str_generic_character_name", (string)null);
		val2.SetTextVariable("CHARACTER_NAME", new TextObject(_dataSource.Name, (Dictionary<string, object>)null));
		val2.SetTextVariable("CHARACTER_GENDER", Hero.MainHero.IsFemale ? 1 : 0);
		val.SetTextVariable("CHARACTER_GENDER", Hero.MainHero.IsFemale ? 1 : 0);
		Hero.MainHero.SetName(val2, val);
		RemoveMount();
		_affirmativeAction.Invoke();
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		((View)CharacterLayer.SceneView).SetEnable(false);
		CharacterLayer.SceneView.ClearAll(false, false);
		_agentVisuals.Reset();
		_agentVisuals = null;
		GauntletLayer = null;
		CharacterCreationReviewStageVM dataSource = _dataSource;
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
		return 1;
	}

	public override void PreviousStage()
	{
		RemoveMount();
		_negativeAction.Invoke();
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
