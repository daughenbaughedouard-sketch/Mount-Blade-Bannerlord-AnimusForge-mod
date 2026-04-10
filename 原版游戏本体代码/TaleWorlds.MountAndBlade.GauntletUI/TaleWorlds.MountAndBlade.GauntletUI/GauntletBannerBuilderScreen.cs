using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.MountAndBlade.ViewModelCollection.BannerBuilder;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[GameStateScreen(typeof(BannerBuilderState))]
public class GauntletBannerBuilderScreen : ScreenBase, IGameStateListener
{
	private BannerBuilderVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _movie;

	private SpriteCategory _bannerIconsCategory;

	private SpriteCategory _bannerBuilderCategory;

	private BannerBuilderState _state;

	private bool _isFinalized;

	private Camera _camera;

	private AgentVisuals[] _agentVisuals;

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private MatrixFrame _characterFrame;

	private Equipment _weaponEquipment;

	private Banner _currentBanner;

	private float _cameraCurrentRotation;

	private float _cameraTargetRotation;

	private float _cameraCurrentDistanceAdder;

	private float _cameraTargetDistanceAdder;

	private float _cameraCurrentElevationAdder;

	private float _cameraTargetElevationAdder;

	private int _agentVisualToShowIndex;

	private bool _refreshCharacterAndShieldNextFrame;

	private bool _refreshBannersNextFrame;

	private bool _checkWhetherAgentVisualIsReady;

	private bool _firstCharacterRender = true;

	private BasicCharacterObject _character;

	private const string DefaultBannerKey = "11.163.166.1528.1528.764.764.1.0.0.133.171.171.483.483.764.764.0.0.0";

	public SceneLayer SceneLayer { get; private set; }

	public GauntletBannerBuilderScreen(BannerBuilderState state)
	{
		_state = state;
		_character = MBObjectManager.Instance.GetObject<BasicCharacterObject>("main_hero");
	}

	protected override void OnInitialize()
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		_bannerIconsCategory = UIResourceManager.LoadSpriteCategory("ui_bannericons");
		_bannerBuilderCategory = UIResourceManager.LoadSpriteCategory("ui_bannerbuilder");
		_agentVisuals = new AgentVisuals[2];
		string text = (string.IsNullOrWhiteSpace(_state.DefaultBannerKey) ? "11.163.166.1528.1528.764.764.1.0.0.133.171.171.483.483.764.764.0.0.0" : _state.DefaultBannerKey);
		_dataSource = new BannerBuilderVM(_character, text, (Action<bool>)Exit, (Action)Refresh, (Action)CopyBannerCode);
		_gauntletLayer = new GauntletLayer("BannerBuilder", 100, false);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		_movie = _gauntletLayer.LoadMovie("BannerBuilderScreen", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		CreateScene();
		((ScreenBase)this).AddLayer((ScreenLayer)(object)SceneLayer);
		_checkWhetherAgentVisualIsReady = true;
		_firstCharacterRender = true;
		RefreshShieldAndCharacter();
		InformationManager.HideAllMessages();
	}

	private void Refresh()
	{
		RefreshShieldAndCharacter();
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		if (_isFinalized)
		{
			return;
		}
		HandleUserInput(dt);
		if (_isFinalized)
		{
			return;
		}
		UpdateCamera(dt);
		SceneLayer sceneLayer = SceneLayer;
		if (sceneLayer != null && sceneLayer.ReadyToRender())
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		Scene scene = _scene;
		if (scene != null)
		{
			scene.Tick(dt);
		}
		if (_refreshBannersNextFrame)
		{
			UpdateBanners();
			_refreshBannersNextFrame = false;
		}
		if (_refreshCharacterAndShieldNextFrame)
		{
			RefreshShieldAndCharacterAux();
			_refreshCharacterAndShieldNextFrame = false;
		}
		if (!_checkWhetherAgentVisualIsReady)
		{
			return;
		}
		int num = (_agentVisualToShowIndex + 1) % 2;
		if (_agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(_firstCharacterRender, true))
		{
			_agentVisuals[num].SetVisible(value: false);
			_agentVisuals[_agentVisualToShowIndex].SetVisible(value: true);
			_checkWhetherAgentVisualIsReady = false;
			_firstCharacterRender = false;
		}
		else
		{
			if (!_firstCharacterRender)
			{
				_agentVisuals[num].SetVisible(value: true);
			}
			_agentVisuals[_agentVisualToShowIndex].SetVisible(value: false);
		}
	}

	private void CreateScene()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		_scene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)2, "mono_renderscene");
		_scene.SetName("BannerBuilderScreen");
		SceneInitializationData val = new SceneInitializationData
		{
			InitPhysicsWorld = false
		};
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
		((ScreenLayer)SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		SceneLayer.SetScene(_scene);
		UpdateCamera(0f);
		SceneLayer.SetSceneUsesShadows(true);
		SceneLayer.SceneView.SetResolutionScaling(true);
		int num = -1;
		num &= -5;
		SceneLayer.SetPostfxConfigParams(num);
		AddCharacterEntities(in ActionIndexCache.act_walk_idle_1h_with_shield_left_stance);
	}

	private void AddCharacterEntities(in ActionIndexCache action)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		_weaponEquipment = new Equipment();
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement equipmentFromSlot = _character.Equipment.GetEquipmentFromSlot((EquipmentIndex)i);
			ItemObject item = ((EquipmentElement)(ref equipmentFromSlot)).Item;
			if (((item != null) ? item.PrimaryWeapon : null) == null || (!((EquipmentElement)(ref equipmentFromSlot)).Item.PrimaryWeapon.IsShield && !Extensions.HasAllFlags<ItemFlags>(((EquipmentElement)(ref equipmentFromSlot)).Item.ItemFlags, (ItemFlags)4096)))
			{
				_weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
			}
		}
		_weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)_dataSource.ShieldSlotIndex, ((ItemRosterElement)(ref _dataSource.ShieldRosterElement)).EquipmentElement);
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(_character.Race);
		_agentVisuals[0] = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(ref action)
			.Scene(_scene)
			.Monster(baseMonsterFromRace)
			.SkeletonType((SkeletonType)(_character.IsFemale ? 1 : 0))
			.Race(_character.Race)
			.PrepareImmediately(true)
			.RightWieldedItemIndex(-1)
			.LeftWieldedItemIndex(_dataSource.ShieldSlotIndex)
			.ClothColor1(_dataSource.CurrentBanner.GetPrimaryColor())
			.ClothColor2(_dataSource.CurrentBanner.GetFirstIconColor())
			.Banner(_dataSource.CurrentBanner)
			.UseMorphAnims(true), "BannerEditorChar", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: true);
		_agentVisuals[0].SetAgentLodZeroOrMaxExternal(makeZero: true);
		_agentVisuals[0].Refresh(needBatchedVersionForWeaponMeshes: false, _agentVisuals[0].GetCopyAgentVisualsData(), forceUseFaceCache: true);
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref _dataSource.ShieldRosterElement)).EquipmentElement;
		ItemObject item2 = ((EquipmentElement)(ref equipmentElement)).Item;
		equipmentElement = ((ItemRosterElement)(ref _dataSource.ShieldRosterElement)).EquipmentElement;
		MissionWeapon shieldWeapon = new MissionWeapon(item2, ((EquipmentElement)(ref equipmentElement)).ItemModifier, _dataSource.CurrentBanner);
		Action<Texture> setAction = delegate(Texture tex)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			((MissionWeapon)(ref shieldWeapon)).GetWeaponData(false).TableauMaterial.SetTexture((MBTextureType)1, tex);
		};
		_dataSource.CurrentBanner.GetTableauTextureLarge(BannerDebugInfo.CreateManual(((object)this).GetType().Name), setAction);
		_agentVisuals[0].SetVisible(value: false);
		_agentVisuals[0].GetEntity().CheckResources(true, true);
		_agentVisuals[1] = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(ref action)
			.Scene(_scene)
			.Race(_character.Race)
			.Monster(baseMonsterFromRace)
			.SkeletonType((SkeletonType)(_character.IsFemale ? 1 : 0))
			.PrepareImmediately(true)
			.RightWieldedItemIndex(-1)
			.LeftWieldedItemIndex(_dataSource.ShieldSlotIndex)
			.Banner(_dataSource.CurrentBanner)
			.ClothColor1(_dataSource.CurrentBanner.GetPrimaryColor())
			.ClothColor2(_dataSource.CurrentBanner.GetFirstIconColor())
			.UseMorphAnims(true), "BannerEditorChar", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: true);
		_agentVisuals[1].SetAgentLodZeroOrMaxExternal(makeZero: true);
		_agentVisuals[1].Refresh(needBatchedVersionForWeaponMeshes: false, _agentVisuals[1].GetCopyAgentVisualsData(), forceUseFaceCache: true);
		_agentVisuals[1].SetVisible(value: false);
		_agentVisuals[1].GetEntity().CheckResources(true, true);
		UpdateBanners();
	}

	private void UpdateBanners()
	{
		Banner currentBanner = _dataSource.CurrentBanner;
		_dataSource.CurrentBanner.GetTableauTextureLarge(BannerDebugInfo.CreateManual(((object)this).GetType().Name), delegate(Texture resultTexture)
		{
			OnNewBannerReadyForBanners(currentBanner, resultTexture);
		}, out var _);
	}

	private void OnNewBannerReadyForBanners(Banner bannerOfTexture, Texture newTexture)
	{
		if (_isFinalized || !((NativeObject)(object)_scene != (NativeObject)null))
		{
			return;
		}
		Banner currentBanner = _currentBanner;
		if (!(((currentBanner != null) ? currentBanner.BannerCode : null) == bannerOfTexture.BannerCode))
		{
			return;
		}
		GameEntity val = _scene.FindEntityWithTag("banner");
		if (val != (GameEntity)null)
		{
			Mesh firstMesh = val.GetFirstMesh();
			if ((NativeObject)(object)firstMesh != (NativeObject)null && _dataSource.CurrentBanner != null)
			{
				firstMesh.GetMaterial().SetTexture((MBTextureType)1, newTexture);
			}
		}
		else
		{
			val = _scene.FindEntityWithTag("banner_2");
			Mesh firstMesh2 = val.GetFirstMesh();
			if ((NativeObject)(object)firstMesh2 != (NativeObject)null && _dataSource.CurrentBanner != null)
			{
				firstMesh2.GetMaterial().SetTexture((MBTextureType)1, newTexture);
			}
		}
		_refreshCharacterAndShieldNextFrame = true;
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		_bannerIconsCategory.Unload();
		_bannerBuilderCategory.Unload();
		((ViewModel)_dataSource).OnFinalize();
		_isFinalized = true;
	}

	private void RefreshShieldAndCharacter()
	{
		_currentBanner = _dataSource.CurrentBanner;
		_dataSource.BannerCodeAsString = _currentBanner.BannerCode;
		_refreshBannersNextFrame = true;
	}

	private void RefreshShieldAndCharacterAux()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		_ = _agentVisualToShowIndex;
		_agentVisualToShowIndex = (_agentVisualToShowIndex + 1) % 2;
		AgentVisualsData copyAgentVisualsData = _agentVisuals[_agentVisualToShowIndex].GetCopyAgentVisualsData();
		copyAgentVisualsData.Equipment(_weaponEquipment).RightWieldedItemIndex(-1).LeftWieldedItemIndex(_dataSource.ShieldSlotIndex)
			.Banner(_dataSource.CurrentBanner)
			.Frame(_characterFrame)
			.BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1))
			.ClothColor1(_dataSource.CurrentBanner.GetPrimaryColor())
			.ClothColor2(_dataSource.CurrentBanner.GetFirstIconColor());
		_agentVisuals[_agentVisualToShowIndex].Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData, forceUseFaceCache: true);
		_agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(true, true);
		_agentVisuals[_agentVisualToShowIndex].GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, _characterFrame, true);
		_agentVisuals[_agentVisualToShowIndex].SetVisible(value: false);
		_agentVisuals[_agentVisualToShowIndex].SetVisible(value: true);
		_checkWhetherAgentVisualIsReady = true;
	}

	private void HandleUserInput(float dt)
	{
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		if (((ScreenLayer)_gauntletLayer).IsFocusedOnInput())
		{
			return;
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm") || ((ScreenLayer)SceneLayer).Input.IsHotKeyReleased("Confirm"))
		{
			_dataSource.ExecuteDone();
			return;
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit") || ((ScreenLayer)SceneLayer).Input.IsHotKeyReleased("Exit"))
		{
			_dataSource.ExecuteCancel();
			return;
		}
		if (((ScreenLayer)SceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == _gauntletLayer)
		{
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)SceneLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)SceneLayer);
		}
		else if (!((ScreenLayer)SceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == SceneLayer)
		{
			((ScreenLayer)SceneLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)SceneLayer);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(((ScreenLayer)SceneLayer).Input.GetNormalizedMouseMoveX() * 1920f, ((ScreenLayer)SceneLayer).Input.GetNormalizedMouseMoveY() * 1080f);
		bool flag = ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("Zoom");
		bool flag2 = ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("Rotate");
		bool flag3 = ((ScreenLayer)SceneLayer).Input.IsHotKeyDown("Ascend");
		if (flag || flag2 || flag3)
		{
			MBWindowManager.DontChangeCursorPos();
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(false);
		}
		else
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
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
		_cameraTargetElevationAdder = MBMath.ClampFloat(_cameraTargetElevationAdder + num5, 0.5f, 1.9f * _agentVisuals[_agentVisualToShowIndex].GetScale());
		if (Input.DebugInput.IsHotKeyPressed("Copy"))
		{
			CopyBannerCode();
		}
		if (Input.DebugInput.IsHotKeyPressed("Duplicate"))
		{
			_dataSource.ExecuteDuplicateCurrentLayer();
		}
		if (Input.DebugInput.IsHotKeyPressed("Paste"))
		{
			_dataSource.SetBannerCode(Input.GetClipboardText());
			RefreshShieldAndCharacter();
		}
		if (Input.DebugInput.IsKeyPressed((InputKey)211))
		{
			_dataSource.DeleteCurrentLayer();
		}
		Vec2 val2 = default(Vec2);
		((Vec2)(ref val2))._002Ector(0f, 0f);
		if (Input.DebugInput.IsKeyReleased((InputKey)203))
		{
			val2.x = -1f;
		}
		else if (Input.DebugInput.IsKeyReleased((InputKey)205))
		{
			val2.x = 1f;
		}
		if (Input.DebugInput.IsKeyReleased((InputKey)208))
		{
			val2.y = 1f;
		}
		else if (Input.DebugInput.IsKeyReleased((InputKey)200))
		{
			val2.y = -1f;
		}
		if (val2.x != 0f || val2.y != 0f)
		{
			_dataSource.TranslateCurrentLayerWith(val2);
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
		((Mat3)(ref characterFrame.rotation)).RotateAboutForward(MathF.PI * 3f / 50f);
		_camera.Frame = characterFrame;
		SceneLayer.SetCamera(_camera);
		SoundManager.SetListenerFrame(characterFrame);
	}

	private void CopyBannerCode()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		Input.SetClipboardText(_dataSource.GetBannerCode());
		InformationManager.DisplayMessage(new InformationMessage("Banner code copied to the clipboard."));
	}

	public void Exit(bool isCancel)
	{
		MouseManager.ActivateMouseCursor((CursorType)1);
		GameStateManager.Current.PopState(0);
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
		_agentVisuals[0].Reset();
		_agentVisuals[1].Reset();
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
		Scene scene = _scene;
		if (scene != null)
		{
			((NativeObject)scene).ManualInvalidate();
		}
		_scene = null;
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}
}
