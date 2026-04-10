using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.SceneNotification;
using TaleWorlds.MountAndBlade.View.Scripts;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;

public class GauntletSceneNotification : GlobalLayer
{
	private class SceneNotificationQueueItem
	{
		public SceneNotificationData Data;

		public int FramesUntilDisplay;
	}

	private readonly GauntletLayer _gauntletLayer;

	private readonly Queue<SceneNotificationQueueItem> _notificationQueue;

	private readonly List<ISceneNotificationContextProvider> _contextProviders;

	private SceneNotificationVM _dataSource;

	private bool _isActive;

	private bool _isLastActiveGameStatePaused;

	private bool _isPendingSceneLoad;

	private Scene _scene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private List<PopupSceneSpawnPoint> _sceneCharacterScripts;

	private PopupSceneCameraPath _cameraPathScript;

	private Dictionary<string, GameEntity> _customPrefabBannerEntities;

	public static GauntletSceneNotification Current { get; private set; }

	public bool IsActive => _isActive;

	private GauntletSceneNotification()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		_dataSource = new SceneNotificationVM((Action)OnPositiveAction, (Action)CloseNotification, (Func<string>)GetContinueKeyText);
		_notificationQueue = new Queue<SceneNotificationQueueItem>();
		_contextProviders = new List<ISceneNotificationContextProvider>();
		_gauntletLayer = new GauntletLayer("SceneNotification", 19600, false);
		_gauntletLayer.LoadMovie("SceneNotification", (ViewModel)(object)_dataSource);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_gauntletLayer;
		MBInformationManager.OnShowSceneNotification += OnShowSceneNotification;
		MBInformationManager.OnHideSceneNotification += OnHideSceneNotification;
		MBInformationManager.IsAnySceneNotificationActive += IsAnySceneNotifiationActive;
		_gauntletLayer.GamepadNavigationContext.GainNavigationAfterFrames(2, (Func<bool>)null);
	}

	private bool IsAnySceneNotifiationActive()
	{
		return _isActive;
	}

	public static void Initialize()
	{
		if (Current == null)
		{
			Current = new GauntletSceneNotification();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
			ScreenManager.SetSuspendLayer(((GlobalLayer)Current).Layer, true);
		}
	}

	private void OnHideSceneNotification()
	{
		CloseNotification();
	}

	private void OnShowSceneNotification(SceneNotificationData campaignNotification)
	{
		_notificationQueue.Enqueue(new SceneNotificationQueueItem
		{
			Data = campaignNotification,
			FramesUntilDisplay = 2
		});
	}

	protected override void OnTick(float dt)
	{
		((GlobalLayer)this).OnTick(dt);
		if (_isActive && (MBGameManager.Current == null || Game.Current == null || MBGameManager.Current == null || MBGameManager.Current.IsEnding))
		{
			_notificationQueue.Clear();
			CloseNotification();
			return;
		}
		if (_dataSource != null)
		{
			_dataSource.EndProgress = _cameraPathScript?.GetCameraFade() ?? 0f;
			_cameraPathScript?.SetIsReady(_dataSource.IsReady);
			if (_dataSource.IsReady && (NativeObject)(object)_scene != (NativeObject)null)
			{
				_scene.WaitWaterRendererCPUSimulation();
				_scene.Tick(dt);
			}
		}
		if (_isPendingSceneLoad)
		{
			if (_isActive)
			{
				OpenScene();
				((GlobalLayer)this).Layer.IsFocusLayer = true;
				ScreenManager.TrySetFocus(((GlobalLayer)this).Layer);
				((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			}
			else
			{
				Debug.FailedAssert("Scene load was pending but scene notification is not active", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\SceneNotification\\GauntletSceneNotification.cs", "OnTick", 121);
			}
			_isPendingSceneLoad = false;
		}
		QueueTick();
	}

	private void QueueTick()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (_isActive || _notificationQueue.Count <= 0)
		{
			return;
		}
		SceneNotificationQueueItem sceneNotificationQueueItem = _notificationQueue.Peek();
		if (sceneNotificationQueueItem.FramesUntilDisplay > 0)
		{
			sceneNotificationQueueItem.FramesUntilDisplay--;
			return;
		}
		RelevantContextType relevantContext = sceneNotificationQueueItem.Data.RelevantContext;
		if (IsGivenContextApplicableToCurrentContext(relevantContext))
		{
			SceneNotificationQueueItem sceneNotificationQueueItem2 = _notificationQueue.Dequeue();
			CreateSceneNotification(sceneNotificationQueueItem2.Data);
		}
	}

	private void OnPositiveAction()
	{
		_cameraPathScript?.SetPositiveState();
		foreach (PopupSceneSpawnPoint sceneCharacterScript in _sceneCharacterScripts)
		{
			sceneCharacterScript.SetPositiveState();
		}
	}

	private void OpenScene()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		SceneNotificationData activeData = _dataSource.ActiveData;
		SceneNotificationCharacter[] sceneNotificationCharacters = activeData.GetSceneNotificationCharacters();
		Banner[] banners = activeData.GetBanners();
		SceneNotificationShip[] ships = activeData.GetShips();
		_scene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)2, "mono_renderscene");
		_scene.SetUsesDeleteLaterSystem(true);
		SceneInitializationData val = default(SceneInitializationData);
		((SceneInitializationData)(ref val))._002Ector(true);
		val.InitPhysicsWorld = activeData.SceneProperties.InitializePhysics;
		if (val.InitPhysicsWorld)
		{
			_scene.EnableInclusiveAsyncPhysx();
		}
		_scene.Read(activeData.SceneID, ref val, "");
		if (val.InitPhysicsWorld)
		{
			_scene.EnableFixedTick();
			_scene.SetFixedTickCallbackActive(true);
		}
		_scene.DisableStaticShadows(activeData.SceneProperties.DisableStaticShadows);
		if (activeData.SceneProperties.OverriddenWaterStrength.HasValue)
		{
			_scene.SetWaterStrength(activeData.SceneProperties.OverriddenWaterStrength.Value);
		}
		_scene.SetClothSimulationState(true);
		_scene.SetShadow(true);
		_scene.SetDynamicShadowmapCascadesRadiusMultiplier(0.1f);
		_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_scene);
		_agentRendererSceneController.SetEnforcedVisibilityForAllAgents(_scene);
		_sceneCharacterScripts = new List<PopupSceneSpawnPoint>();
		_customPrefabBannerEntities = new Dictionary<string, GameEntity>();
		GameEntity firstEntityWithScriptComponent = _scene.GetFirstEntityWithScriptComponent<PopupSceneCameraPath>();
		_cameraPathScript = ((firstEntityWithScriptComponent != null) ? firstEntityWithScriptComponent.GetFirstScriptOfType<PopupSceneCameraPath>() : null);
		_cameraPathScript?.Initialize();
		_cameraPathScript?.SetInitialState();
		if (sceneNotificationCharacters != null)
		{
			int num = 1;
			foreach (SceneNotificationCharacter val2 in sceneNotificationCharacters)
			{
				BasicCharacterObject character = val2.Character;
				if (character == null)
				{
					num++;
					continue;
				}
				string text = "spawnpoint_player_" + num;
				GameEntity val3 = _scene.FindEntitiesWithTag(text).ToList().FirstOrDefault();
				if (val3 == (GameEntity)null)
				{
					num++;
					continue;
				}
				PopupSceneSpawnPoint firstScriptOfType = val3.GetFirstScriptOfType<PopupSceneSpawnPoint>();
				MatrixFrame frame = val3.GetFrame();
				Equipment val4 = character.FirstBattleEquipment;
				if (val2.OverriddenEquipment != null)
				{
					val4 = val2.OverriddenEquipment;
				}
				else if (val2.UseCivilianEquipment)
				{
					val4 = character.FirstCivilianEquipment;
				}
				BodyProperties val5 = character.GetBodyProperties(character.Equipment, -1);
				if (val2.OverriddenBodyProperties != default(BodyProperties))
				{
					val5 = val2.OverriddenBodyProperties;
				}
				uint num2 = character.Culture.Color;
				uint num3 = character.Culture.Color2;
				if (val2.CustomColor1 != uint.MaxValue)
				{
					num2 = val2.CustomColor1;
				}
				if (val2.CustomColor2 != uint.MaxValue)
				{
					num3 = val2.CustomColor2;
				}
				Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(character.Race);
				AgentVisuals agentVisuals = AgentVisuals.Create(new AgentVisualsData().UseMorphAnims(true).Equipment(val4).Race(character.Race)
					.BodyProperties(val5)
					.SkeletonType((SkeletonType)(character.IsFemale ? 1 : 0))
					.Frame(frame)
					.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, character.IsFemale, "_facegen"))
					.Scene(_scene)
					.Monster(baseMonsterFromRace)
					.PrepareImmediately(true)
					.UseTranslucency(true)
					.UseTesselation(true)
					.ClothColor1(num2)
					.ClothColor2(num3), "notification_agent_visuals_" + num, isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
				AgentVisuals agentVisuals2 = null;
				if (val2.UseHorse)
				{
					EquipmentElement val6 = val4[(EquipmentIndex)10];
					ItemObject item = ((EquipmentElement)(ref val6)).Item;
					string randomMountKeyString = MountCreationKey.GetRandomMountKeyString(item, character.GetMountKeySeed());
					MBActionSet actionSet = MBGlobals.GetActionSet(item.HorseComponent.Monster.ActionSetCode);
					agentVisuals2 = AgentVisuals.Create(new AgentVisualsData().Equipment(val4).Frame(frame).ActionSet(actionSet)
						.Scene(_scene)
						.Monster(item.HorseComponent.Monster)
						.Scale(item.ScaleFactor)
						.PrepareImmediately(true)
						.UseTranslucency(true)
						.UseTesselation(true)
						.MountCreationKey(randomMountKeyString), "notification_mount_visuals_" + num, isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
				}
				firstScriptOfType.InitializeWithAgentVisuals(agentVisuals, agentVisuals2);
				agentVisuals.SetAgentLodZeroOrMaxExternal(makeZero: true);
				agentVisuals2?.SetAgentLodZeroOrMaxExternal(makeZero: true);
				firstScriptOfType.SetInitialState();
				_sceneCharacterScripts.Add(firstScriptOfType);
				if (!string.IsNullOrEmpty(firstScriptOfType.BannerTagToUseForAddedPrefab) && (NativeObject)(object)firstScriptOfType.AddedPrefabComponent != (NativeObject)null)
				{
					_customPrefabBannerEntities.Add(firstScriptOfType.BannerTagToUseForAddedPrefab, GameEntity.CreateFromWeakEntity(((GameEntityComponent)firstScriptOfType.AddedPrefabComponent).GetEntity()));
				}
				num++;
			}
		}
		if (banners != null)
		{
			for (int j = 0; j < banners.Length; j++)
			{
				Banner val7 = banners[j];
				string text2 = "banner_" + (j + 1);
				GameEntity bannerEntity = _scene.FindEntityWithTag(text2);
				GameEntity entity;
				if (bannerEntity != (GameEntity)null)
				{
					((BannerVisual)(object)val7.BannerVisual).GetTableauTextureLarge(BannerDebugInfo.CreateManual(((object)this).GetType().Name), delegate(Texture t)
					{
						OnBannerTableauRenderDone(bannerEntity, t);
					});
				}
				else if (_customPrefabBannerEntities.TryGetValue(text2, out entity))
				{
					((BannerVisual)(object)val7.BannerVisual).GetTableauTextureLarge(BannerDebugInfo.CreateManual(((object)this).GetType().Name), delegate(Texture t)
					{
						OnBannerTableauRenderDone(entity, t);
					});
				}
			}
		}
		if (ships != null)
		{
			int num4 = 1;
			foreach (SceneNotificationShip val8 in ships)
			{
				if (string.IsNullOrEmpty(val8.ShipPrefabId))
				{
					num4++;
					Debug.FailedAssert("Scene notification ship does not have a valid prefab", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\SceneNotification\\GauntletSceneNotification.cs", "OpenScene", 366);
					continue;
				}
				string text3 = "spawnpoint_ship_" + num4;
				GameEntity val9 = _scene.FindEntityWithTag(text3);
				if (val9 == (GameEntity)null)
				{
					Debug.FailedAssert("Ship spawn point entity with tag: " + text3 + " was not found", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\SceneNotification\\GauntletSceneNotification.cs", "OpenScene", 374);
					num4++;
					continue;
				}
				List<GameEntity> list = val9.GetChildren().ToList();
				for (int num6 = 0; num6 < list.Count; num6++)
				{
					GameEntity val10 = list[num6];
					_scene.RemoveEntity(val10, 62);
				}
				val9.GetFirstScriptOfType<PopupSceneShipSpawnPoint>();
				GameEntity val11 = VisualShipFactory.CreateVisualShip(val8.ShipPrefabId, _scene, val8.ShipUpgrades, val8.ShipSeed, val8.ShipHitPointRatio, val8.SailColor1, val8.SailColor2, true);
				val9.AddChild(val11, false);
				num4++;
			}
		}
		_dataSource.Scene = _scene;
	}

	private void OnBannerTableauRenderDone(GameEntity bannerEntity, Texture bannerTexture)
	{
		if (!(bannerEntity != (GameEntity)null))
		{
			return;
		}
		foreach (Mesh item in bannerEntity.GetAllMeshesWithTag("banner_replacement_mesh"))
		{
			ApplyBannerTextureToMesh(item, bannerTexture);
		}
		Skeleton skeleton = bannerEntity.Skeleton;
		if (((skeleton != null) ? skeleton.GetAllMeshes() : null) == null)
		{
			return;
		}
		Skeleton skeleton2 = bannerEntity.Skeleton;
		foreach (Mesh item2 in (skeleton2 != null) ? skeleton2.GetAllMeshes() : null)
		{
			if (item2.HasTag("banner_replacement_mesh"))
			{
				ApplyBannerTextureToMesh(item2, bannerTexture);
			}
		}
	}

	private void ApplyBannerTextureToMesh(Mesh bannerMesh, Texture bannerTexture)
	{
		if ((NativeObject)(object)bannerMesh != (NativeObject)null)
		{
			Material val = bannerMesh.GetMaterial().CreateCopy();
			val.SetTexture((MBTextureType)1, bannerTexture);
			uint num = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
			ulong shaderFlags = val.GetShaderFlags();
			val.SetShaderFlags(shaderFlags | num);
			bannerMesh.SetMaterial(val);
		}
	}

	private void CreateSceneNotification(SceneNotificationData data)
	{
		if (_isActive)
		{
			Debug.FailedAssert("Trying to create scene notification while another notification is playing", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\SceneNotification\\GauntletSceneNotification.cs", "CreateSceneNotification", 441);
			return;
		}
		_isActive = true;
		_dataSource.CreateNotification(data);
		ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, false);
		_isLastActiveGameStatePaused = data.PauseActiveState;
		if (_isLastActiveGameStatePaused)
		{
			GameStateManager.Current.RegisterActiveStateDisableRequest((object)this);
			MBCommon.PauseGameEngine();
		}
		_dataSource.EndProgress = 0f;
		_isPendingSceneLoad = true;
	}

	private void CloseNotification()
	{
		if (!_isActive)
		{
			return;
		}
		_dataSource.ClearData();
		_isActive = false;
		((GlobalLayer)this).Layer.InputRestrictions.ResetInputRestrictions();
		ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, true);
		((GlobalLayer)this).Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(((GlobalLayer)this).Layer);
		if (_isLastActiveGameStatePaused)
		{
			GameStateManager current = GameStateManager.Current;
			if (current != null)
			{
				current.UnregisterActiveStateDisableRequest((object)this);
			}
			MBCommon.UnPauseGameEngine();
		}
		_cameraPathScript?.Destroy();
		if (_sceneCharacterScripts != null)
		{
			foreach (PopupSceneSpawnPoint sceneCharacterScript in _sceneCharacterScripts)
			{
				sceneCharacterScript.Destroy();
			}
			_sceneCharacterScripts = null;
		}
		MBAgentRendererSceneController.DestructAgentRendererSceneController(_scene, _agentRendererSceneController, false);
		_scene.ClearAll();
		((NativeObject)_scene).ManualInvalidate();
		_scene = null;
	}

	private string GetContinueKeyText()
	{
		Module currentModule = Module.CurrentModule;
		GameTextManager val = ((currentModule != null) ? currentModule.GlobalTextManager : null);
		if (val != null)
		{
			if (Input.IsGamepadActive)
			{
				return ((object)val.FindText("str_click_to_continue_console", (string)null).SetTextVariable("CONSOLE_KEY_NAME", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("ConversationHotKeyCategory", "ContinueClick"), 1f))).ToString();
			}
			return ((object)val.FindText("str_click_to_continue", (string)null)).ToString();
		}
		return string.Empty;
	}

	public void OnFinalize()
	{
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	public void RegisterContextProvider(ISceneNotificationContextProvider provider)
	{
		_contextProviders.Add(provider);
	}

	public bool RemoveContextProvider(ISceneNotificationContextProvider provider)
	{
		return _contextProviders.Remove(provider);
	}

	private bool IsGivenContextApplicableToCurrentContext(RelevantContextType givenContextType)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (LoadingWindow.IsLoadingWindowActive)
		{
			return false;
		}
		if ((int)givenContextType == 0)
		{
			return true;
		}
		for (int i = 0; i < _contextProviders.Count; i++)
		{
			ISceneNotificationContextProvider obj = _contextProviders[i];
			if (obj != null && !obj.IsContextAllowed(givenContextType))
			{
				return false;
			}
		}
		return true;
	}
}
