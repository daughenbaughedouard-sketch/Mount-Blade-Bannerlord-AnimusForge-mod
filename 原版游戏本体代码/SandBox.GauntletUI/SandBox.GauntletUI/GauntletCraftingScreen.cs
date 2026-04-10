using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

[GameStateScreen(typeof(CraftingState))]
public class GauntletCraftingScreen : ScreenBase, ICraftingStateHandler, IGameStateListener
{
	private struct CameraParameters
	{
		public float HorizontalRotation;

		public float VerticalRotation;

		public float SelfRotation;

		public float Zoom;

		public CameraParameters(float horizontalRotation, float verticalRotation, float selfRotation, float zoom)
		{
			HorizontalRotation = horizontalRotation;
			VerticalRotation = verticalRotation;
			SelfRotation = selfRotation;
			Zoom = zoom;
		}
	}

	private static bool _enableLegacyCraftingCameraControls;

	private Scene _craftingScene;

	private SceneLayer _sceneLayer;

	private readonly CraftingState _craftingState;

	private CraftingVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _gauntletMovie;

	private SpriteCategory _craftingCategory;

	private Camera _camera;

	private MatrixFrame _initialCameraFrame;

	private Vec3 _dofParams;

	private Vec3 _cameraZoomDirection;

	private CameraParameters _currentCameraValues;

	private CameraParameters _targetCameraValues;

	private GameEntity _craftingEntity;

	private MatrixFrame _initialEntityFrame;

	private WeaponDesign _craftedData;

	private bool _isInitialized;

	private static KeyValuePair<string, string> _reloadXmlPath;

	private SceneView SceneView => _sceneLayer.SceneView;

	public GauntletCraftingScreen(CraftingState craftingState)
	{
		_craftingState = craftingState;
		_craftingState.Handler = (ICraftingStateHandler)(object)this;
	}

	private void ReloadPieces()
	{
		string key = _reloadXmlPath.Key;
		string text = _reloadXmlPath.Value;
		if (!text.EndsWith(".xml"))
		{
			text += ".xml";
		}
		_reloadXmlPath = new KeyValuePair<string, string>(null, null);
		XmlDocument xmlDocument = Game.Current.ObjectManager.LoadXMLFromFileSkipValidation(ModuleHelper.GetModuleFullPath(key) + "ModuleData/" + text, "");
		if (xmlDocument == null)
		{
			return;
		}
		foreach (XmlNode childNode in xmlDocument.ChildNodes[1].ChildNodes)
		{
			XmlAttributeCollection attributes = childNode.Attributes;
			if (attributes != null)
			{
				string innerText = attributes["id"].InnerText;
				CraftingPiece val = Game.Current.ObjectManager.GetObject<CraftingPiece>(innerText);
				if (val != null)
				{
					((MBObjectBase)val).Deserialize(Game.Current.ObjectManager, childNode);
				}
			}
		}
		_craftingState.CraftingLogic.ReIndex(true);
		RefreshItemEntity(_dataSource.IsInCraftingMode);
		_dataSource.WeaponDesign.RefreshItem();
	}

	public void Initialize()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		_craftingCategory = UIResourceManager.LoadSpriteCategory("ui_crafting");
		_gauntletLayer = new GauntletLayer("CraftingScreen", 1, false);
		_gauntletMovie = _gauntletLayer.LoadMovie("Crafting", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_gauntletLayer);
		OpenScene();
		RefreshItemEntity(isItemVisible: true);
		_isInitialized = true;
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)14));
		}
		UISoundsHelper.PlayUISound("event:/ui/panels/panel_settlement_enter_smithy");
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		Initialize();
		((ScreenLayer)_sceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Generic"));
		((ScreenLayer)_sceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		((ScreenLayer)_sceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("CraftingHotkeyCategory"));
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("CraftingHotkeyCategory"));
		InformationManager.HideAllMessages();
	}

	protected override void OnFinalize()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((ScreenBase)this).OnFinalize();
		Game current = Game.Current;
		if (current != null)
		{
			current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)0));
		}
		Scene craftingScene = _craftingScene;
		if (craftingScene != null)
		{
			((NativeObject)craftingScene).ManualInvalidate();
		}
		_craftingScene = null;
		SceneView.ClearAll(true, true);
		_craftingCategory.Unload();
		CraftingVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
	}

	protected override void OnFrameTick(float dt)
	{
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		LoadingWindow.DisableGlobalLoadingWindow();
		((ScreenBase)this).OnFrameTick(dt);
		if (!((ScreenLayer)_gauntletLayer).IsFocusedOnInput() && (((ScreenLayer)_sceneLayer).Input.IsControlDown() || ((ScreenLayer)_gauntletLayer).Input.IsControlDown()))
		{
			if (((ScreenLayer)_sceneLayer).Input.IsHotKeyPressed("Copy") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("Copy"))
			{
				CopyXmlCode();
			}
			else if (((ScreenLayer)_sceneLayer).Input.IsHotKeyPressed("Paste") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyPressed("Paste"))
			{
				PasteXmlCode();
			}
		}
		if (_craftingState.CraftingLogic.CurrentCraftingTemplate == null)
		{
			return;
		}
		Scene craftingScene = _craftingScene;
		if (craftingScene != null)
		{
			craftingScene.Tick(dt);
		}
		bool flag = false;
		if (_reloadXmlPath.Key != null && _reloadXmlPath.Value != null)
		{
			ReloadPieces();
			flag = true;
		}
		if (flag)
		{
			return;
		}
		if (((ScreenBase)this).DebugInput.IsHotKeyPressed("Reset"))
		{
			ResetEntityAndCamera();
		}
		_dataSource.CanSwitchTabs = !Input.IsGamepadActive || !InformationManager.GetIsAnyTooltipActiveAndExtended();
		_dataSource.AreGamepadControlHintsEnabled = Input.IsGamepadActive && ((ScreenLayer)_sceneLayer).IsHitThisFrame && _dataSource.IsInCraftingMode;
		if (_dataSource.IsInCraftingMode)
		{
			TickCameraInput(dt);
		}
		Scene craftingScene2 = _craftingScene;
		if (craftingScene2 != null)
		{
			craftingScene2.SetDepthOfFieldParameters(_dofParams.x, _dofParams.z, false);
		}
		Scene craftingScene3 = _craftingScene;
		if (craftingScene3 != null)
		{
			craftingScene3.SetDepthOfFieldFocus(((Vec3)(ref _initialEntityFrame.origin)).Distance(_camera.Frame.origin));
		}
		SceneView.SetCamera(_camera);
		if (!Input.IsGamepadActive && (((ScreenLayer)_gauntletLayer).IsFocusedOnInput() || ((ScreenLayer)_sceneLayer).IsFocusedOnInput()))
		{
			return;
		}
		if (IsHotKeyReleasedInAnyLayer("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteCancel();
		}
		else if (IsHotKeyReleasedInAnyLayer("Confirm"))
		{
			bool isInCraftingMode = _dataSource.IsInCraftingMode;
			bool isInRefinementMode = _dataSource.IsInRefinementMode;
			bool isInSmeltingMode = _dataSource.IsInSmeltingMode;
			var (flag2, flag3) = _dataSource.ExecuteConfirm();
			if (!flag2)
			{
				return;
			}
			if (flag3)
			{
				if (isInCraftingMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/craft_success");
				}
				else if (isInRefinementMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/refine_success");
				}
				else if (isInSmeltingMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/smelt_success");
				}
			}
			else
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
		}
		else
		{
			if (!_dataSource.CanSwitchTabs)
			{
				return;
			}
			if (IsHotKeyReleasedInAnyLayer("SwitchToPreviousTab"))
			{
				if (_dataSource.IsInSmeltingMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/refine_tab");
					_dataSource.ExecuteSwitchToRefinement();
				}
				else if (_dataSource.IsInCraftingMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/smelt_tab");
					_dataSource.ExecuteSwitchToSmelting();
				}
				else if (_dataSource.IsInRefinementMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/craft_tab");
					_dataSource.ExecuteSwitchToCrafting();
				}
			}
			else if (IsHotKeyReleasedInAnyLayer("SwitchToNextTab"))
			{
				if (_dataSource.IsInSmeltingMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/craft_tab");
					_dataSource.ExecuteSwitchToCrafting();
				}
				else if (_dataSource.IsInCraftingMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/refine_tab");
					_dataSource.ExecuteSwitchToRefinement();
				}
				else if (_dataSource.IsInRefinementMode)
				{
					UISoundsHelper.PlayUISound("event:/ui/crafting/smelt_tab");
					_dataSource.ExecuteSwitchToSmelting();
				}
			}
		}
	}

	private void OnClose()
	{
		ICampaignMission current = CampaignMission.Current;
		if (current != null)
		{
			current.EndMission();
		}
		Game.Current.GameStateManager.PopState(0);
	}

	private void OnResetCamera()
	{
		ResetEntityAndCamera();
	}

	private void OnWeaponCrafted()
	{
		WeaponDesignResultPopupVM craftingResultPopup = _dataSource.WeaponDesign.CraftingResultPopup;
		if (craftingResultPopup != null)
		{
			craftingResultPopup.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		}
	}

	public void OnCraftingLogicInitialized()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0069: Expected O, but got Unknown
		CraftingVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = new CraftingVM(_craftingState.CraftingLogic, (Action)OnClose, (Action)OnResetCamera, (Action)OnWeaponCrafted, (Func<WeaponComponentData, ItemUsageSetFlags>)GetItemUsageSetFlag)
		{
			OnItemRefreshed = new OnItemRefreshedDelegate(RefreshItemEntity)
		};
		_dataSource.WeaponDesign.CraftingHistory.SetDoneKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.WeaponDesign.CraftingHistory.SetCancelKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.CraftingHeroPopup.SetExitInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetConfirmInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetExitInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetPreviousTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToPreviousTab"));
		_dataSource.SetNextTabInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("SwitchToNextTab"));
		_dataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("CraftingHotkeyCategory").GetGameKey(56));
		_dataSource.AddCameraControlInputKey(HotKeyManager.GetCategory("CraftingHotkeyCategory").GetGameKey(57));
		_dataSource.AddCameraControlInputKey(((IEnumerable<GameAxisKey>)HotKeyManager.GetCategory("CraftingHotkeyCategory").RegisteredGameAxisKeys).FirstOrDefault((Func<GameAxisKey, bool>)((GameAxisKey x) => x.Id == "CameraAxisX")));
	}

	public void OnCraftingLogicRefreshed()
	{
		_dataSource.OnCraftingLogicRefreshed(_craftingState.CraftingLogic);
		if (_isInitialized)
		{
			RefreshItemEntity(isItemVisible: true);
		}
	}

	private void OpenScene()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		_craftingScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		_craftingScene.SetName("GauntletCraftingScreen");
		SceneInitializationData val = new SceneInitializationData
		{
			InitPhysicsWorld = false
		};
		_craftingScene.Read("crafting_menu_outdoor", ref val, "");
		_craftingScene.DisableStaticShadows(true);
		_craftingScene.SetShadow(true);
		_craftingScene.SetClothSimulationState(true);
		InitializeEntityAndCamera();
		_sceneLayer = new SceneLayer(true, true);
		((ScreenLayer)_sceneLayer).IsFocusLayer = true;
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_sceneLayer);
		SceneView.SetScene(_craftingScene);
		SceneView.SetCamera(_camera);
		SceneView.SetSceneUsesShadows(true);
		SceneView.SetAcceptGlobalDebugRenderObjects(true);
		SceneView.SetRenderWithPostfx(true);
		SceneView.SetResolutionScaling(true);
	}

	private void InitializeEntityAndCamera()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = _craftingScene.FindEntityWithTag("weapon_point");
		MatrixFrame globalFrame = val.GetGlobalFrame();
		_craftingScene.RemoveEntity(val, 114);
		((MatrixFrame)(ref globalFrame)).Elevate(1.6f);
		_initialEntityFrame = globalFrame;
		_craftingEntity = GameEntity.CreateEmpty(_craftingScene, true, true, true);
		_craftingEntity.SetFrame(ref globalFrame, true);
		_camera = Camera.CreateCamera();
		_dofParams = default(Vec3);
		GameEntity val2 = _craftingScene.FindEntityWithTag("camera_point");
		val2.GetCameraParamsFromCameraScript(_camera, ref _dofParams);
		float fovVertical = _camera.GetFovVertical();
		float aspectRatio = Screen.AspectRatio;
		float near = _camera.Near;
		float far = _camera.Far;
		_camera.SetFovVertical(fovVertical, aspectRatio, near, far);
		_craftingScene.SetDepthOfFieldParameters(_dofParams.x, _dofParams.z, false);
		_craftingScene.SetDepthOfFieldFocus(_dofParams.y);
		_initialCameraFrame = val2.GetFrame();
		_cameraZoomDirection = _initialEntityFrame.origin - _initialCameraFrame.origin;
	}

	private void RefreshItemEntity(bool isItemVisible)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		_dataSource.WeaponDesign.CurrentWeaponHasScabbard = false;
		MatrixFrame val = _initialEntityFrame;
		if (_craftingEntity != (GameEntity)null)
		{
			val = _craftingEntity.GetFrame();
			_craftingEntity.Remove(115);
			_craftingEntity = null;
		}
		if (!isItemVisible)
		{
			return;
		}
		_craftingEntity = GameEntity.CreateEmpty(_craftingScene, true, true, true);
		_craftingEntity.SetFrame(ref val, true);
		_craftedData = _craftingState.CraftingLogic.CurrentWeaponDesign;
		if (!(_craftedData != (WeaponDesign)null))
		{
			return;
		}
		val = _craftingEntity.GetFrame();
		float num = _craftedData.CraftedWeaponLength / 2f;
		BladeData bladeData = _craftedData.UsedPieces[0].CraftingPiece.BladeData;
		_dataSource.WeaponDesign.CurrentWeaponHasScabbard = !string.IsNullOrEmpty(bladeData.HolsterMeshName);
		MetaMesh val2;
		if (!_dataSource.WeaponDesign.IsScabbardVisible)
		{
			val2 = CraftedDataView.BuildWeaponMesh(_craftedData, 0f - num, false, false);
		}
		else
		{
			val2 = CraftedDataView.BuildHolsterMeshWithWeapon(_craftedData, 0f - num, false);
			if ((NativeObject)(object)val2 == (NativeObject)null)
			{
				val2 = CraftedDataView.BuildWeaponMesh(_craftedData, 0f - num, false, false);
			}
		}
		_craftingEntity = _craftingScene.AddItemEntity(ref val, val2);
	}

	private void TickCameraInput(float dt)
	{
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		if (((ScreenLayer)_sceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == _gauntletLayer)
		{
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_sceneLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_sceneLayer);
		}
		else if (!((ScreenLayer)_sceneLayer).IsHitThisFrame && (object)ScreenManager.FocusedLayer == _sceneLayer)
		{
			((ScreenLayer)_sceneLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_sceneLayer);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
		Vec2 val = default(Vec2);
		((Vec2)(ref val))._002Ector(((ScreenLayer)_sceneLayer).Input.GetNormalizedMouseMoveX() * 1920f, ((ScreenLayer)_sceneLayer).Input.GetNormalizedMouseMoveY() * 1080f);
		bool num = _enableLegacyCraftingCameraControls && !Input.IsGamepadActive;
		bool flag = ((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("Rotate");
		bool flag2 = !num && ((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("Zoom");
		bool flag3 = num && ((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("Zoom");
		bool flag4 = false;
		if (flag || flag2 || flag3 || flag4)
		{
			MBWindowManager.DontChangeCursorPos();
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(false);
		}
		else
		{
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetMouseVisibility(true);
		}
		if (!((ScreenBase)this).DebugInput.IsControlDown() && !((ScreenBase)this).DebugInput.IsAltDown())
		{
			if (((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("Rotate") && ((ScreenLayer)_sceneLayer).Input.IsHotKeyDown("Zoom"))
			{
				ResetEntityAndCamera();
				return;
			}
			float num2;
			if (Input.IsGamepadActive)
			{
				float gameKeyState = ((ScreenLayer)_sceneLayer).Input.GetGameKeyState(56);
				float gameKeyState2 = ((ScreenLayer)_sceneLayer).Input.GetGameKeyState(57);
				float inputValue = gameKeyState - gameKeyState2;
				NormalizeControllerInputForDeadZone(ref inputValue, 0.1f);
				num2 = inputValue * 4f * dt;
			}
			else
			{
				float deltaMouseScroll = ((ScreenLayer)_sceneLayer).Input.GetDeltaMouseScroll();
				float num3 = (flag2 ? val.y : 0f);
				num2 = deltaMouseScroll * 0.001f + num3 * 0.002f;
			}
			_targetCameraValues.Zoom = MBMath.ClampFloat(_targetCameraValues.Zoom + num2, -0.5f, 0.5f);
			float num4;
			if (Input.IsGamepadActive)
			{
				float inputValue2 = ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("CameraAxisX");
				NormalizeControllerInputForDeadZone(ref inputValue2, 0.1f);
				num4 = inputValue2 * 400f * dt;
			}
			else
			{
				num4 = (flag ? val.x : 0f) * 0.2f;
			}
			_targetCameraValues.HorizontalRotation = MBMath.WrapAngle(_targetCameraValues.HorizontalRotation + num4 * (MathF.PI / 180f));
			float num5;
			if (Input.IsGamepadActive)
			{
				float inputValue3 = ((ScreenLayer)_sceneLayer).Input.GetGameKeyAxis("CameraAxisY");
				NormalizeControllerInputForDeadZone(ref inputValue3, 0.1f);
				num5 = inputValue3 * 400f * dt;
			}
			else
			{
				num5 = (flag ? (val.y * -1f) : 0f) * 0.2f;
			}
			_targetCameraValues.VerticalRotation = MBMath.WrapAngle(_targetCameraValues.VerticalRotation + num5 * (MathF.PI / 180f));
			float num6;
			if (Input.IsGamepadActive)
			{
				num6 = 0f;
			}
			else
			{
				float num7 = ((MathF.Abs(val.x) > MathF.Abs(val.y)) ? val.x : val.y);
				num6 = (flag3 ? num7 : 0f) * 0.2f;
			}
			_targetCameraValues.SelfRotation = MBMath.WrapAngle(_targetCameraValues.SelfRotation + num6 * (MathF.PI / 180f));
		}
		UpdateCamera(dt);
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
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Min(1f, 10f * dt);
		CameraParameters currentCameraValues = new CameraParameters(MathF.AngleLerp(_currentCameraValues.HorizontalRotation, _targetCameraValues.HorizontalRotation, num, 1E-05f), MathF.AngleLerp(_currentCameraValues.VerticalRotation, _targetCameraValues.VerticalRotation, num, 1E-05f), MathF.AngleLerp(_currentCameraValues.SelfRotation, _targetCameraValues.SelfRotation, num, 1E-05f), MathF.Lerp(_currentCameraValues.Zoom, _targetCameraValues.Zoom, num, 1E-05f));
		CameraParameters cameraParameters = new CameraParameters(currentCameraValues.HorizontalRotation - _currentCameraValues.HorizontalRotation, currentCameraValues.VerticalRotation - _currentCameraValues.VerticalRotation, currentCameraValues.SelfRotation - _currentCameraValues.SelfRotation, currentCameraValues.Zoom - _currentCameraValues.Zoom);
		_currentCameraValues = currentCameraValues;
		MatrixFrame frame = _craftingEntity.GetFrame();
		if (_enableLegacyCraftingCameraControls && !Input.IsGamepadActive)
		{
			((Mat3)(ref frame.rotation)).RotateAboutSide(cameraParameters.VerticalRotation);
			((Mat3)(ref frame.rotation)).RotateAboutAnArbitraryVector(ref Vec3.Up, cameraParameters.HorizontalRotation);
			((Mat3)(ref frame.rotation)).RotateAboutUp(cameraParameters.SelfRotation);
		}
		else
		{
			((Mat3)(ref frame.rotation)).RotateAboutUp(cameraParameters.HorizontalRotation);
			((Mat3)(ref frame.rotation)).RotateAboutSide(cameraParameters.VerticalRotation);
		}
		_craftingEntity.SetFrame(ref frame, true);
		MatrixFrame frame2 = _camera.Frame;
		ref Vec3 origin = ref frame2.origin;
		origin += _cameraZoomDirection * cameraParameters.Zoom;
		_camera.Frame = frame2;
	}

	private void ResetEntityAndCamera()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		_currentCameraValues = new CameraParameters(0f, 0f, 0f, 0f);
		_targetCameraValues = new CameraParameters(0f, 0f, 0f, 0f);
		_craftingEntity.SetFrame(ref _initialEntityFrame, true);
		_camera.Frame = _initialCameraFrame;
	}

	private void CopyXmlCode()
	{
		Input.SetClipboardText(_craftingState.CraftingLogic.GetXmlCodeForCurrentItem(_craftingState.CraftingLogic.GetCurrentCraftedItemObject(false, (string)null)));
	}

	private void PasteXmlCode()
	{
		string clipboardText = Input.GetClipboardText();
		if (!string.IsNullOrEmpty(clipboardText))
		{
			ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(clipboardText);
			CraftingTemplate val2 = default(CraftingTemplate);
			(CraftingPiece, int)[] array = default((CraftingPiece, int)[]);
			if (val != null)
			{
				SwithToCraftedItem(val);
			}
			else if (_craftingState.CraftingLogic.TryGetWeaponPropertiesFromXmlCode(clipboardText, ref val2, ref array))
			{
				_dataSource.SetCurrentDesignManually(val2, array);
			}
		}
	}

	private void SwithToCraftedItem(ItemObject itemObject)
	{
		if (itemObject == null || !itemObject.IsCraftedWeapon)
		{
			return;
		}
		if (!_dataSource.IsInCraftingMode)
		{
			_dataSource.ExecuteSwitchToCrafting();
		}
		WeaponDesign weaponDesign = itemObject.WeaponDesign;
		if (_craftingState.CraftingLogic.CurrentCraftingTemplate != weaponDesign.Template)
		{
			_dataSource.WeaponDesign.SelectPrimaryWeaponClass(weaponDesign.Template);
		}
		WeaponDesignElement[] usedPieces = weaponDesign.UsedPieces;
		foreach (WeaponDesignElement val in usedPieces)
		{
			if (val.IsValid)
			{
				_dataSource.WeaponDesign.SwitchToPiece(val);
			}
		}
	}

	private ItemUsageSetFlags GetItemUsageSetFlag(WeaponComponentData item)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(item.ItemUsage))
		{
			return MBItem.GetItemUsageSetFlags(item.ItemUsage);
		}
		return (ItemUsageSetFlags)0;
	}

	private bool IsHotKeyReleasedInAnyLayer(string hotKeyId)
	{
		if (!((ScreenLayer)_sceneLayer).Input.IsHotKeyReleased(hotKeyId))
		{
			return ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased(hotKeyId);
		}
		return true;
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}
}
