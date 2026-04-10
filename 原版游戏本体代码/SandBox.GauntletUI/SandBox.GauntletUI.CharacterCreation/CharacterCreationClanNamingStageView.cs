using System;
using System.Collections.Generic;
using System.Linq;
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
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.CharacterCreation;

[CharacterCreationStageView(typeof(CharacterCreationClanNamingStage))]
public class CharacterCreationClanNamingStageView : CharacterCreationStageViewBase
{
	private CharacterCreationManager _characterCreationManager;

	private GauntletLayer GauntletLayer;

	private CharacterCreationClanNamingStageVM _dataSource;

	private GauntletMovieIdentifier _clanNamingStageMovie;

	private TextObject _affirmativeActionText;

	private TextObject _negativeActionText;

	private Banner _banner;

	private float _cameraCurrentRotation;

	private float _cameraTargetRotation;

	private float _cameraCurrentDistanceAdder;

	private float _cameraTargetDistanceAdder;

	private float _cameraCurrentElevationAdder;

	private float _cameraTargetElevationAdder;

	private readonly BasicCharacterObject _character;

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private AgentVisuals _agentVisuals;

	private MatrixFrame _characterFrame;

	private Equipment _weaponEquipment;

	private Camera _camera;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	private ItemRosterElement ShieldRosterElement => _dataSource.ShieldRosterElement;

	private int ShieldSlotIndex => _dataSource.ShieldSlotIndex;

	public SceneLayer SceneLayer { get; private set; }

	public CharacterCreationClanNamingStageView(CharacterCreationManager characterCreationManager, ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, ControlCharacterCreationStage refreshAction, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction, ControlCharacterCreationStageReturnInt getTotalStageCountAction, ControlCharacterCreationStageReturnInt getFurthestIndexAction, ControlCharacterCreationStageWithInt goToIndexAction)
		: base(affirmativeAction, negativeAction, refreshAction, getCurrentStageIndexAction, getTotalStageCountAction, getFurthestIndexAction, goToIndexAction)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		_characterCreationManager = characterCreationManager;
		_affirmativeActionText = affirmativeActionText;
		_negativeActionText = negativeActionText;
		GauntletLayer = new GauntletLayer("CharacterCreationClanNaming", 1, false)
		{
			IsFocusLayer = true
		};
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		_character = (BasicCharacterObject)(object)CharacterObject.PlayerCharacter;
		_banner = Clan.PlayerClan.Banner;
		_dataSource = new CharacterCreationClanNamingStageVM(_character, _characterCreationManager, (Action)NextStage, _affirmativeActionText, (Action)PreviousStage, _negativeActionText);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(56));
		_dataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(57));
		GameAxisKey val = ((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisX"));
		GameAxisKey val2 = ((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisY"));
		_dataSource.AddCameraControlInputKey(val, Module.CurrentModule.GlobalTextManager.FindText("str_key_name", typeof(FaceGenHotkeyCategory).Name + "_" + val.Id));
		_dataSource.AddCameraControlInputKey(val2, Module.CurrentModule.GlobalTextManager.FindText("str_key_name", typeof(FaceGenHotkeyCategory).Name + "_" + val2.Id));
		_clanNamingStageMovie = GauntletLayer.LoadMovie("CharacterCreationClanNamingStage", (ViewModel)(object)_dataSource);
		CreateScene();
		RefreshCharacterEntity();
	}

	public override void Tick(float dt)
	{
		HandleUserInput(dt);
		UpdateCamera(dt);
		if (SceneLayer != null && SceneLayer.ReadyToRender())
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		if ((NativeObject)(object)_scene != (NativeObject)null)
		{
			_scene.Tick(dt);
		}
		HandleEscapeMenu(this, (ScreenLayer)(object)GauntletLayer);
		HandleLayerInput();
	}

	private void CreateScene()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		_scene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		_scene.SetName("MBBannerEditorScreen");
		SceneInitializationData val = default(SceneInitializationData);
		((SceneInitializationData)(ref val))._002Ector(true);
		_scene.Read("banner_editor_scene", ref val, "");
		_scene.SetShadow(true);
		_scene.DisableStaticShadows(true);
		_scene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_scene);
		float aspectRatio = Screen.AspectRatio;
		GameEntity val2 = _scene.FindEntityWithTag("spawnpoint_player");
		_characterFrame = val2.GetFrame();
		((Mat3)(ref _characterFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		_cameraTargetDistanceAdder = 3.5f;
		_cameraCurrentDistanceAdder = _cameraTargetDistanceAdder;
		_cameraTargetElevationAdder = 1.15f;
		_cameraCurrentElevationAdder = _cameraTargetElevationAdder;
		_camera = Camera.CreateCamera();
		_camera.SetFovVertical(0.6981317f, aspectRatio, 0.2f, 200f);
		SceneLayer = new SceneLayer(true, true);
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		SceneLayer.SetScene(_scene);
		UpdateCamera(0f);
		SceneLayer.SetSceneUsesShadows(true);
		SceneLayer.SceneView.SetResolutionScaling(true);
		int num = -1;
		num &= -5;
		SceneLayer.SetPostfxConfigParams(num);
		AddCharacterEntity(in ActionIndexCache.act_walk_idle_1h_with_shield_left_stance);
	}

	private void AddCharacterEntity(in ActionIndexCache action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		_weaponEquipment = new Equipment();
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement equipmentFromSlot = _character.Equipment.GetEquipmentFromSlot((EquipmentIndex)i);
			ItemObject item = ((EquipmentElement)(ref equipmentFromSlot)).Item;
			if (((item != null) ? item.PrimaryWeapon : null) != null)
			{
				ItemObject item2 = ((EquipmentElement)(ref equipmentFromSlot)).Item;
				if (((item2 != null) ? item2.PrimaryWeapon : null) == null || ((EquipmentElement)(ref equipmentFromSlot)).Item.PrimaryWeapon.IsShield)
				{
					continue;
				}
			}
			_weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
		}
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(_character.Race);
		_agentVisuals = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(ref action)
			.Scene(_scene)
			.Race(_character.Race)
			.Monster(baseMonsterFromRace)
			.SkeletonType((SkeletonType)(_character.IsFemale ? 1 : 0))
			.PrepareImmediately(true)
			.UseMorphAnims(true), "BannerEditorChar", false, false, true);
		_agentVisuals.SetAgentLodZeroOrMaxExternal(true);
		UpdateBanners();
	}

	private void UpdateBanners()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Banner banner = _banner;
		BannerDebugInfo val = BannerDebugInfo.CreateManual(GetType().Name);
		BannerVisualExtensions.GetTableauTextureLarge(banner, ref val, (Action<Texture>)OnNewBannerReadyForBanners);
	}

	private void OnNewBannerReadyForBanners(Texture newTexture)
	{
		if ((NativeObject)(object)_scene == (NativeObject)null)
		{
			return;
		}
		GameEntity val = _scene.FindEntityWithTag("banner");
		if (val == (GameEntity)null)
		{
			return;
		}
		Mesh firstMesh = val.GetFirstMesh();
		if ((NativeObject)(object)firstMesh != (NativeObject)null && _banner != null)
		{
			firstMesh.GetMaterial().SetTexture((MBTextureType)1, newTexture);
		}
		val = _scene.FindEntityWithTag("banner_2");
		if (!(val == (GameEntity)null))
		{
			firstMesh = val.GetFirstMesh();
			if ((NativeObject)(object)firstMesh != (NativeObject)null && _banner != null)
			{
				firstMesh.GetMaterial().SetTexture((MBTextureType)1, newTexture);
			}
		}
	}

	private void RefreshCharacterEntity()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		Equipment weaponEquipment = _weaponEquipment;
		int shieldSlotIndex = ShieldSlotIndex;
		ItemRosterElement shieldRosterElement = ShieldRosterElement;
		weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)shieldSlotIndex, ((ItemRosterElement)(ref shieldRosterElement)).EquipmentElement);
		AgentVisualsData copyAgentVisualsData = _agentVisuals.GetCopyAgentVisualsData();
		copyAgentVisualsData.Equipment(_weaponEquipment).RightWieldedItemIndex(-1).LeftWieldedItemIndex(ShieldSlotIndex)
			.Banner(_banner)
			.ClothColor1(_banner.GetPrimaryColor())
			.ClothColor2(_banner.GetFirstIconColor());
		_agentVisuals.Refresh(false, copyAgentVisualsData, false);
		shieldRosterElement = ShieldRosterElement;
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref shieldRosterElement)).EquipmentElement;
		ItemObject item = ((EquipmentElement)(ref equipmentElement)).Item;
		shieldRosterElement = ShieldRosterElement;
		equipmentElement = ((ItemRosterElement)(ref shieldRosterElement)).EquipmentElement;
		MissionWeapon shieldWeapon = new MissionWeapon(item, ((EquipmentElement)(ref equipmentElement)).ItemModifier, _banner);
		Action<Texture> action = delegate(Texture tex)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			((MissionWeapon)(ref shieldWeapon)).GetWeaponData(false).TableauMaterial.SetTexture((MBTextureType)1, tex);
		};
		Banner banner = _banner;
		BannerDebugInfo val = BannerDebugInfo.CreateManual(GetType().Name);
		BannerVisualExtensions.GetTableauTextureLarge(banner, ref val, action);
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

	private void HandleUserInput(float dt)
	{
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		_dataSource.CharacterGamepadControlsEnabled = Input.IsGamepadActive && ((ScreenLayer)SceneLayer).IsHitThisFrame;
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
		_cameraTargetDistanceAdder = MBMath.ClampFloat(_cameraTargetDistanceAdder + num, 1.5f, 5f);
		float num4;
		if (Input.IsGamepadActive)
		{
			float inputValue2 = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisX") * -1f;
			NormalizeControllerInputForDeadZone(ref inputValue2, 0.1f);
			num4 = inputValue2 * 600f * ((ScreenLayer)SceneLayer).Input.GetMouseSensitivity() * dt;
		}
		else
		{
			num4 = (flag2 ? (val.x * -1f) : 0f) * 0.3f * ((ScreenLayer)SceneLayer).Input.GetMouseSensitivity();
		}
		_cameraTargetRotation = MBMath.WrapAngle(_cameraTargetRotation + num4 * (MathF.PI / 180f));
		float num5;
		if (Input.IsGamepadActive)
		{
			float inputValue3 = ((ScreenLayer)SceneLayer).Input.GetGameKeyAxis("CameraAxisY");
			NormalizeControllerInputForDeadZone(ref inputValue3, 0.1f);
			num5 = inputValue3 * 2f * dt;
		}
		else
		{
			num5 = (flag3 ? val.y : 0f) * 0.002f;
		}
		_cameraTargetElevationAdder = MBMath.ClampFloat(_cameraTargetElevationAdder + num5, 0.5f, 1.9f * _agentVisuals.GetScale());
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

	private void UpdateCamera(float dt)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Min(1f, 10f * dt);
		_cameraCurrentRotation = MathF.AngleLerp(_cameraCurrentRotation, _cameraTargetRotation, num, 1E-05f);
		_cameraCurrentElevationAdder = MathF.Lerp(_cameraCurrentElevationAdder, _cameraTargetElevationAdder, num, 1E-05f);
		_cameraCurrentDistanceAdder = MathF.Lerp(_cameraCurrentDistanceAdder, _cameraTargetDistanceAdder, num, 1E-05f);
		MatrixFrame characterFrame = _characterFrame;
		((Mat3)(ref characterFrame.rotation)).RotateAboutUp(_cameraCurrentRotation);
		ref Vec3 origin = ref characterFrame.origin;
		origin += _cameraCurrentElevationAdder * characterFrame.rotation.u + _cameraCurrentDistanceAdder * characterFrame.rotation.f;
		((Mat3)(ref characterFrame.rotation)).RotateAboutSide(-MathF.PI / 2f);
		((Mat3)(ref characterFrame.rotation)).RotateAboutUp(MathF.PI);
		((Mat3)(ref characterFrame.rotation)).RotateAboutForward(MathF.PI * -3f / 50f);
		_camera.Frame = characterFrame;
		SceneLayer.SetCamera(_camera);
		SoundManager.SetListenerFrame(characterFrame);
	}

	public override IEnumerable<ScreenLayer> GetLayers()
	{
		return new List<ScreenLayer>
		{
			(ScreenLayer)(object)SceneLayer,
			(ScreenLayer)(object)GauntletLayer
		};
	}

	public override int GetVirtualStageCount()
	{
		return 1;
	}

	public override void NextStage()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		TextObject val = new TextObject(_dataSource.ClanName, (Dictionary<string, object>)null);
		TextObject val2 = GameTexts.FindText("str_generic_clan_name", (string)null);
		val2.SetTextVariable("CLAN_NAME", val);
		Clan.PlayerClan.ChangeClanName(val2, val2);
		ControlCharacterCreationStage affirmativeAction = _affirmativeAction;
		if (affirmativeAction != null)
		{
			affirmativeAction.Invoke();
		}
	}

	public override void PreviousStage()
	{
		ControlCharacterCreationStage negativeAction = _negativeAction;
		if (negativeAction != null)
		{
			negativeAction.Invoke();
		}
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		((View)SceneLayer.SceneView).SetEnable(false);
		SceneLayer.SceneView.ClearAll(true, true);
		GauntletLayer = null;
		SceneLayer = null;
		CharacterCreationClanNamingStageVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		_clanNamingStageMovie = null;
		_agentVisuals.Reset();
		_agentVisuals = null;
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
		Scene scene = _scene;
		if (scene != null)
		{
			((NativeObject)scene).ManualInvalidate();
		}
		_scene = null;
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

	private bool IsHotKeyReleasedOnAnyLayer(string hotkeyName)
	{
		if (!((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased(hotkeyName))
		{
			return ((ScreenLayer)SceneLayer).Input.IsHotKeyReleased(hotkeyName);
		}
		return true;
	}
}
