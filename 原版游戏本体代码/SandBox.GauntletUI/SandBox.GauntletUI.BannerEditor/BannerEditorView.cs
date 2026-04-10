using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
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
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.BannerEditor;

public class BannerEditorView
{
	private GauntletMovieIdentifier _gauntletmovie;

	private readonly SpriteCategory _spriteCategory;

	private bool _isFinalized;

	private float _cameraCurrentRotation;

	private float _cameraTargetRotation;

	private float _cameraCurrentDistanceAdder;

	private float _cameraTargetDistanceAdder;

	private float _cameraCurrentElevationAdder;

	private float _cameraTargetElevationAdder;

	private readonly BasicCharacterObject _character;

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private AgentVisuals[] _agentVisuals;

	private int _agentVisualToShowIndex;

	private bool _checkWhetherAgentVisualIsReady;

	private bool _firstCharacterRender = true;

	private bool _refreshBannersNextFrame;

	private bool _refreshCharacterAndShieldNextFrame;

	private MatrixFrame _characterFrame;

	private MissionWeapon _shieldWeapon;

	private Equipment _weaponEquipment;

	private Banner _currentBanner;

	private Camera _camera;

	private BannerEditorTextureCreationData _latestBannerTextureCreationData;

	private BannerEditorTextureCreationData _latestShieldTextureCreationData;

	private bool _isOpenedFromCharacterCreation;

	private ControlCharacterCreationStage _affirmativeAction;

	private ControlCharacterCreationStage _negativeAction;

	private ControlCharacterCreationStageWithInt _goToIndexAction;

	public GauntletLayer GauntletLayer { get; private set; }

	public BannerEditorVM DataSource { get; private set; }

	public Banner Banner { get; private set; }

	private ItemRosterElement ShieldRosterElement => DataSource.ShieldRosterElement;

	private int ShieldSlotIndex => DataSource.ShieldSlotIndex;

	public SceneLayer SceneLayer { get; private set; }

	public BannerEditorView(BasicCharacterObject character, Banner banner, ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, ControlCharacterCreationStage onRefresh = null, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction = null, ControlCharacterCreationStageReturnInt getTotalStageCountAction = null, ControlCharacterCreationStageReturnInt getFurthestIndexAction = null, ControlCharacterCreationStageWithInt goToIndexAction = null)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		BannerEditorTextureCache current = BannerEditorTextureCache.Current;
		if (current != null)
		{
			current.FlushCache();
		}
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_bannericons");
		_character = character;
		Banner = banner;
		_goToIndexAction = goToIndexAction;
		if (getCurrentStageIndexAction == null || getTotalStageCountAction == null || getFurthestIndexAction == null)
		{
			DataSource = new BannerEditorVM(_character, Banner, (Action<bool>)Exit, (Action)RefreshShieldAndCharacter, 0, 0, 0, (Action<int>)GoToIndex);
			DataSource.Description = ((object)new TextObject("{=3ZO5cMLu}Customize your banner's sigil", (Dictionary<string, object>)null)).ToString();
			_isOpenedFromCharacterCreation = true;
		}
		else
		{
			DataSource = new BannerEditorVM(_character, Banner, (Action<bool>)Exit, (Action)RefreshShieldAndCharacter, getCurrentStageIndexAction.Invoke(), getTotalStageCountAction.Invoke(), getFurthestIndexAction.Invoke(), (Action<int>)GoToIndex);
			DataSource.Description = ((object)new TextObject("{=312lNJTM}Customize your personal banner by choosing your clan's sigil", (Dictionary<string, object>)null)).ToString();
			_isOpenedFromCharacterCreation = false;
		}
		DataSource.DoneText = ((object)affirmativeActionText).ToString();
		DataSource.CancelText = ((object)negativeActionText).ToString();
		GauntletLayer = new GauntletLayer("BanerEditor", 1, false);
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("FaceGenHotkeyCategory"));
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)GauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		DataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		DataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(56));
		DataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("FaceGenHotkeyCategory").GetGameKey(57));
		GameAxisKey val = ((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisX"));
		GameAxisKey val2 = ((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("FaceGenHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisY"));
		DataSource.AddCameraControlInputKey(val, Module.CurrentModule.GlobalTextManager.FindText("str_key_name", typeof(FaceGenHotkeyCategory).Name + "_" + val.Id));
		DataSource.AddCameraControlInputKey(val2, Module.CurrentModule.GlobalTextManager.FindText("str_key_name", typeof(FaceGenHotkeyCategory).Name + "_" + val2.Id));
		_affirmativeAction = affirmativeAction;
		_negativeAction = negativeAction;
		_agentVisuals = (AgentVisuals[])(object)new AgentVisuals[2];
		_currentBanner = Banner;
		CreateScene();
		Input.ClearKeys();
		Equipment weaponEquipment = _weaponEquipment;
		int shieldSlotIndex = ShieldSlotIndex;
		ItemRosterElement shieldRosterElement = ShieldRosterElement;
		weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)shieldSlotIndex, ((ItemRosterElement)(ref shieldRosterElement)).EquipmentElement);
		shieldRosterElement = ShieldRosterElement;
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref shieldRosterElement)).EquipmentElement;
		ItemObject item = ((EquipmentElement)(ref equipmentElement)).Item;
		shieldRosterElement = ShieldRosterElement;
		equipmentElement = ((ItemRosterElement)(ref shieldRosterElement)).EquipmentElement;
		_shieldWeapon = new MissionWeapon(item, ((EquipmentElement)(ref equipmentElement)).ItemModifier, Banner);
		AgentVisualsData copyAgentVisualsData = _agentVisuals[0].GetCopyAgentVisualsData();
		copyAgentVisualsData.Equipment(_weaponEquipment).RightWieldedItemIndex(-1).LeftWieldedItemIndex(ShieldSlotIndex)
			.Banner(Banner)
			.ClothColor1(Banner.GetPrimaryColor())
			.ClothColor2(Banner.GetFirstIconColor());
		_agentVisuals[0].Refresh(false, copyAgentVisualsData, true);
		_agentVisuals[0].SetVisible(false);
		_agentVisuals[0].GetEntity().CheckResources(true, true);
		AgentVisualsData copyAgentVisualsData2 = _agentVisuals[1].GetCopyAgentVisualsData();
		copyAgentVisualsData2.Equipment(_weaponEquipment).RightWieldedItemIndex(-1).LeftWieldedItemIndex(ShieldSlotIndex)
			.Banner(Banner)
			.ClothColor1(Banner.GetPrimaryColor())
			.ClothColor2(Banner.GetFirstIconColor());
		_agentVisuals[1].Refresh(false, copyAgentVisualsData2, true);
		_agentVisuals[1].SetVisible(false);
		_agentVisuals[1].GetEntity().CheckResources(true, true);
		_checkWhetherAgentVisualIsReady = true;
		_firstCharacterRender = true;
	}

	public void OnTick(float dt)
	{
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
			if (_gauntletmovie == null)
			{
				_gauntletmovie = GauntletLayer.LoadMovie("BannerEditor", (ViewModel)(object)DataSource);
			}
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
			_agentVisuals[num].SetVisible(false);
			_agentVisuals[_agentVisualToShowIndex].SetVisible(true);
			_checkWhetherAgentVisualIsReady = false;
			_firstCharacterRender = false;
		}
		else
		{
			if (!_firstCharacterRender)
			{
				_agentVisuals[num].SetVisible(true);
			}
			_agentVisuals[_agentVisualToShowIndex].SetVisible(false);
		}
	}

	public void OnFinalize()
	{
		BannerEditorTextureCache current = BannerEditorTextureCache.Current;
		if (current != null)
		{
			current.FlushCache();
		}
		if (!_isOpenedFromCharacterCreation)
		{
			_spriteCategory.Unload();
		}
		BannerEditorVM dataSource = DataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_isFinalized = true;
	}

	public void Exit(bool isCancel)
	{
		MouseManager.ActivateMouseCursor((CursorType)1);
		_gauntletmovie = null;
		if (isCancel)
		{
			_negativeAction.Invoke();
			return;
		}
		SetMapIconAsDirtyForAllPlayerClanParties();
		_affirmativeAction.Invoke();
	}

	private void SetMapIconAsDirtyForAllPlayerClanParties()
	{
		foreach (Hero item in (List<Hero>)(object)Clan.PlayerClan.AliveLords)
		{
			foreach (CaravanPartyComponent ownedCaravan in item.OwnedCaravans)
			{
				PartyBase party = ((PartyComponent)ownedCaravan).MobileParty.Party;
				if (party != null)
				{
					party.SetVisualAsDirty();
				}
				((PartyComponent)ownedCaravan).MobileParty.SetNavalVisualAsDirty();
			}
		}
		foreach (Hero item2 in (List<Hero>)(object)Clan.PlayerClan.Companions)
		{
			foreach (CaravanPartyComponent ownedCaravan2 in item2.OwnedCaravans)
			{
				PartyBase party2 = ((PartyComponent)ownedCaravan2).MobileParty.Party;
				if (party2 != null)
				{
					party2.SetVisualAsDirty();
				}
				((PartyComponent)ownedCaravan2).MobileParty.SetNavalVisualAsDirty();
			}
		}
		foreach (WarPartyComponent item3 in (List<WarPartyComponent>)(object)Clan.PlayerClan.WarPartyComponents)
		{
			PartyBase party3 = ((PartyComponent)item3).MobileParty.Party;
			if (party3 != null)
			{
				party3.SetVisualAsDirty();
			}
			((PartyComponent)item3).MobileParty.SetNavalVisualAsDirty();
		}
		foreach (Settlement item4 in (List<Settlement>)(object)Clan.PlayerClan.Settlements)
		{
			if (item4.IsVillage && item4.Village.VillagerPartyComponent != null)
			{
				PartyBase party4 = ((PartyComponent)item4.Village.VillagerPartyComponent).MobileParty.Party;
				if (party4 != null)
				{
					party4.SetVisualAsDirty();
				}
			}
			else if ((item4.IsCastle || item4.IsTown) && ((Fief)item4.Town).GarrisonParty != null)
			{
				PartyBase party5 = ((Fief)item4.Town).GarrisonParty.Party;
				if (party5 != null)
				{
					party5.SetVisualAsDirty();
				}
			}
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
		_scene.SetName("MBBannerEditorScreen");
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
		((ScreenLayer)SceneLayer).IsFocusLayer = true;
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
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		_weaponEquipment = new Equipment();
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement equipmentFromSlot = _character.Equipment.GetEquipmentFromSlot((EquipmentIndex)i);
			ItemObject item = ((EquipmentElement)(ref equipmentFromSlot)).Item;
			if (((item != null) ? item.PrimaryWeapon : null) != null)
			{
				ItemObject item2 = ((EquipmentElement)(ref equipmentFromSlot)).Item;
				if (((item2 != null) ? item2.PrimaryWeapon : null) == null || ((EquipmentElement)(ref equipmentFromSlot)).Item.PrimaryWeapon.IsShield || Extensions.HasAllFlags<ItemFlags>(((EquipmentElement)(ref equipmentFromSlot)).Item.ItemFlags, (ItemFlags)4096))
				{
					continue;
				}
			}
			_weaponEquipment.AddEquipmentToSlotWithoutAgent((EquipmentIndex)i, equipmentFromSlot);
		}
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(_character.Race);
		_agentVisuals[0] = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(ref action)
			.Scene(_scene)
			.Monster(baseMonsterFromRace)
			.SkeletonType((SkeletonType)(_character.IsFemale ? 1 : 0))
			.Race(_character.Race)
			.PrepareImmediately(true)
			.UseMorphAnims(true), "BannerEditorChar", false, false, true);
		_agentVisuals[0].SetAgentLodZeroOrMaxExternal(true);
		_agentVisuals[0].GetEntity().CheckResources(true, true);
		_agentVisuals[1] = AgentVisuals.Create(new AgentVisualsData().Equipment(_weaponEquipment).BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1)).Frame(_characterFrame)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _character.IsFemale, "_facegen"))
			.ActionCode(ref action)
			.Scene(_scene)
			.Race(_character.Race)
			.Monster(baseMonsterFromRace)
			.SkeletonType((SkeletonType)(_character.IsFemale ? 1 : 0))
			.PrepareImmediately(true)
			.UseMorphAnims(true), "BannerEditorChar", false, false, true);
		_agentVisuals[1].SetAgentLodZeroOrMaxExternal(true);
		_agentVisuals[1].GetEntity().CheckResources(true, true);
		UpdateBanners();
	}

	private void UpdateBanners()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (_latestBannerTextureCreationData != null)
		{
			ThumbnailCacheManager.Current.DestroyTexture((ThumbnailCreationData)(object)_latestBannerTextureCreationData);
			_latestBannerTextureCreationData = null;
		}
		Banner banner = Banner;
		BannerDebugInfo val = BannerDebugInfo.CreateManual(GetType().Name);
		BannerVisualExtensions.GetTableauTextureLargeForBannerEditor(banner, ref val, (Action<Texture>)delegate(Texture tex)
		{
			OnNewBannerReadyForBanners(Banner, tex);
		}, ref _latestBannerTextureCreationData);
	}

	private void OnNewBannerReadyForBanners(Banner bannerOfTexture, Texture newTexture)
	{
		if (_isFinalized || !((NativeObject)(object)_scene != (NativeObject)null) || !(_currentBanner.BannerCode == bannerOfTexture.BannerCode))
		{
			return;
		}
		GameEntity val = _scene.FindEntityWithTag("banner");
		if (val != (GameEntity)null)
		{
			Mesh firstMesh = val.GetFirstMesh();
			if ((NativeObject)(object)firstMesh != (NativeObject)null && Banner != null)
			{
				firstMesh.GetMaterial().SetTexture((MBTextureType)1, newTexture);
			}
		}
		else
		{
			val = _scene.FindEntityWithTag("banner_2");
			Mesh firstMesh2 = val.GetFirstMesh();
			if ((NativeObject)(object)firstMesh2 != (NativeObject)null && Banner != null)
			{
				firstMesh2.GetMaterial().SetTexture((MBTextureType)1, newTexture);
			}
		}
		_refreshCharacterAndShieldNextFrame = true;
	}

	private void RefreshShieldAndCharacter()
	{
		_currentBanner = Banner;
		_refreshBannersNextFrame = true;
	}

	private void RefreshShieldAndCharacterAux()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		if (_latestShieldTextureCreationData != null)
		{
			ThumbnailCacheManager.Current.DestroyTexture((ThumbnailCreationData)(object)_latestShieldTextureCreationData);
			_latestShieldTextureCreationData = null;
		}
		Banner banner = Banner;
		BannerDebugInfo val = BannerDebugInfo.CreateManual(GetType().Name);
		BannerVisualExtensions.GetTableauTextureLargeForBannerEditor(banner, ref val, (Action<Texture>)delegate(Texture tex)
		{
			OnNewBannerReadyForShield(tex);
		}, ref _latestShieldTextureCreationData);
		_ = _agentVisualToShowIndex;
		_agentVisualToShowIndex = (_agentVisualToShowIndex + 1) % 2;
		AgentVisualsData copyAgentVisualsData = _agentVisuals[_agentVisualToShowIndex].GetCopyAgentVisualsData();
		copyAgentVisualsData.Equipment(_weaponEquipment).RightWieldedItemIndex(-1).LeftWieldedItemIndex(ShieldSlotIndex)
			.Banner(Banner)
			.Frame(_characterFrame)
			.BodyProperties(_character.GetBodyProperties(_weaponEquipment, -1))
			.ClothColor1(Banner.GetPrimaryColor())
			.ClothColor2(Banner.GetFirstIconColor());
		_agentVisuals[_agentVisualToShowIndex].Refresh(false, copyAgentVisualsData, true);
		_agentVisuals[_agentVisualToShowIndex].GetEntity().CheckResources(true, true);
		_agentVisuals[_agentVisualToShowIndex].GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.001f, _characterFrame, true);
		_agentVisuals[_agentVisualToShowIndex].SetVisible(false);
		_agentVisuals[_agentVisualToShowIndex].SetVisible(true);
		_checkWhetherAgentVisualIsReady = true;
	}

	private void OnNewBannerReadyForShield(Texture newTexture)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((MissionWeapon)(ref _shieldWeapon)).GetWeaponData(false).TableauMaterial.SetTexture((MBTextureType)1, newTexture);
	}

	private bool IsHotKeyReleasedOnAnyLayer(string hotkeyName)
	{
		if (!((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased(hotkeyName))
		{
			return ((ScreenLayer)SceneLayer).Input.IsHotKeyReleased(hotkeyName);
		}
		return true;
	}

	private void HandleUserInput(float dt)
	{
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
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
		if (IsHotKeyReleasedOnAnyLayer("Confirm"))
		{
			DataSource.ExecuteDone();
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			return;
		}
		if (IsHotKeyReleasedOnAnyLayer("Exit"))
		{
			DataSource.ExecuteCancel();
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			return;
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
		_cameraTargetElevationAdder = MBMath.ClampFloat(_cameraTargetElevationAdder + num5, 0.5f, 1.9f * _agentVisuals[_agentVisualToShowIndex].GetScale());
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

	public void OnDeactivate()
	{
		_agentVisuals[0].Reset();
		_agentVisuals[1].Reset();
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, false);
		_agentRendererSceneController = null;
		_scene.ClearAll();
		((NativeObject)_scene).ManualInvalidate();
		_scene = null;
	}

	public void GoToIndex(int index)
	{
		_goToIndexAction.Invoke(index);
	}
}
