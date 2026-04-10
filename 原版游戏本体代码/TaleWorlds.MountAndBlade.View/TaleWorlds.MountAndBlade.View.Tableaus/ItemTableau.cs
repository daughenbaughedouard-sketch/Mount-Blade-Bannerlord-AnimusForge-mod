using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class ItemTableau
{
	private static int _tableauIndex;

	private Scene _tableauScene;

	private GameEntity _itemTableauEntity;

	private MatrixFrame _itemTableauFrame = MatrixFrame.Identity;

	private bool _isRotating;

	private bool _isTranslating;

	private bool _isRotatingByDefault;

	private bool _initialized;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private float _cameraRatio;

	private Camera _camera;

	private Vec3 _midPoint;

	private const float InitialCamFov = 1f;

	private float _curZoomSpeed;

	private Vec3 _curCamDisplacement = Vec3.Zero;

	private bool _isEnabled;

	private float _panRotation;

	private float _tiltRotation;

	private bool _hasInitialTiltRotation;

	private float _initialTiltRotation;

	private bool _hasInitialPanRotation;

	private float _initialPanRotation;

	private float RenderScale = 1f;

	private string _stringId = "";

	private int _ammo;

	private int _averageUnitCost;

	private string _itemModifierId = "";

	private string _bannerCode = "";

	private ItemRosterElement _itemRosterElement;

	private MatrixFrame _initialFrame;

	private bool _lockMouse;

	public Texture Texture { get; private set; }

	private TableauView View
	{
		get
		{
			if ((NativeObject)(object)Texture != (NativeObject)null)
			{
				return Texture.TableauView;
			}
			return null;
		}
	}

	private bool _isSizeValid
	{
		get
		{
			if (_tableauSizeX > 0)
			{
				return _tableauSizeY > 0;
			}
			return false;
		}
	}

	public ItemTableau()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		SetEnabled(enabled: true);
	}

	public void SetTargetSize(int width, int height)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		bool isSizeValid = _isSizeValid;
		_isRotating = false;
		if (width <= 0 || height <= 0)
		{
			_tableauSizeX = 10;
			_tableauSizeY = 10;
		}
		else
		{
			RenderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			_tableauSizeX = (int)((float)width * RenderScale);
			_tableauSizeY = (int)((float)height * RenderScale);
		}
		_cameraRatio = (float)_tableauSizeX / (float)_tableauSizeY;
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		TableauView view2 = View;
		if (view2 != null)
		{
			((SceneView)view2).AddClearTask(true);
		}
		Texture texture = Texture;
		if (texture != null)
		{
			texture.Release();
		}
		if (!isSizeValid && _isSizeValid)
		{
			Recalculate();
		}
		Texture = TableauView.AddTableau($"ItemTableau_{_tableauIndex++}", new TextureUpdateEventHandler(TableauMaterialTabInventoryItemTooltipOnRender), (object)_tableauScene, _tableauSizeX, _tableauSizeY);
	}

	public void OnFinalize()
	{
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		Camera camera = _camera;
		if (camera != null)
		{
			camera.ReleaseCameraEntity();
		}
		_camera = null;
		TableauView view2 = View;
		if (view2 != null)
		{
			((SceneView)view2).AddClearTask(false);
		}
		Scene tableauScene = _tableauScene;
		if (tableauScene != null)
		{
			((NativeObject)tableauScene).ManualInvalidate();
		}
		_tableauScene = null;
		Texture = null;
		_initialized = false;
		if (_lockMouse)
		{
			UpdateMouseLock(forceUnlock: true);
		}
	}

	protected void SetEnabled(bool enabled)
	{
		_isRotatingByDefault = true;
		_isRotating = false;
		ResetCamera();
		_isEnabled = enabled;
		TableauView view = View;
		if ((NativeObject)(object)view != (NativeObject)null)
		{
			((View)view).SetEnable(_isEnabled);
		}
	}

	public void SetStringId(string stringId)
	{
		_stringId = stringId;
		Recalculate();
	}

	public void SetAmmo(int ammo)
	{
		_ammo = ammo;
		Recalculate();
	}

	public void SetAverageUnitCost(int averageUnitCost)
	{
		_averageUnitCost = averageUnitCost;
		Recalculate();
	}

	public void SetItemModifierId(string itemModifierId)
	{
		_itemModifierId = itemModifierId;
		Recalculate();
	}

	public void SetBannerCode(string bannerCode)
	{
		_bannerCode = bannerCode;
		Recalculate();
	}

	public void Recalculate()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Invalid comparison between Unknown and I4
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Invalid comparison between Unknown and I4
		if (UiStringHelper.IsStringNoneOrEmptyForUi(_stringId) || !_isSizeValid)
		{
			return;
		}
		ItemModifier val = null;
		ItemObject val2 = MBObjectManager.Instance.GetObject<ItemObject>(_stringId);
		if (val2 == null)
		{
			val2 = ((IEnumerable<ItemObject>)Game.Current.ObjectManager.GetObjectTypeList<ItemObject>()).FirstOrDefault((Func<ItemObject, bool>)((ItemObject item) => item.IsCraftedWeapon && item.WeaponDesign.HashedCode == _stringId));
		}
		if (!string.IsNullOrEmpty(_itemModifierId))
		{
			val = MBObjectManager.Instance.GetObject<ItemModifier>(_itemModifierId);
		}
		if (val2 == null)
		{
			return;
		}
		_itemRosterElement = new ItemRosterElement(val2, _ammo, val);
		RefreshItemTableau();
		if (_itemTableauEntity != (GameEntity)null)
		{
			float num = Screen.RealScreenResolutionWidth / (float)_tableauSizeX;
			float num2 = Screen.RealScreenResolutionHeight / (float)_tableauSizeY;
			float num3 = ((num > num2) ? num : num2);
			if (num3 < 1f)
			{
				Vec3 globalBoxMax = _itemTableauEntity.GlobalBoxMax;
				Vec3 globalBoxMin = _itemTableauEntity.GlobalBoxMin;
				_itemTableauFrame = _itemTableauEntity.GetFrame();
				float length = ((Vec3)(ref _itemTableauFrame.rotation.f)).Length;
				((Mat3)(ref _itemTableauFrame.rotation)).Orthonormalize();
				((Mat3)(ref _itemTableauFrame.rotation)).ApplyScaleLocal(length * num3);
				_itemTableauEntity.SetFrame(ref _itemTableauFrame, true);
				Vec3 globalBoxMax2 = _itemTableauEntity.GlobalBoxMax;
				if (((Vec3)(ref globalBoxMax)).NearlyEquals(ref globalBoxMax2, 1E-05f))
				{
					Vec3 globalBoxMin2 = _itemTableauEntity.GlobalBoxMin;
					if (((Vec3)(ref globalBoxMin)).NearlyEquals(ref globalBoxMin2, 1E-05f))
					{
						_itemTableauEntity.SetBoundingboxDirty();
						_itemTableauEntity.RecomputeBoundingBox();
					}
				}
				ref Vec3 origin = ref _itemTableauFrame.origin;
				origin += (globalBoxMax + globalBoxMin - _itemTableauEntity.GlobalBoxMax - _itemTableauEntity.GlobalBoxMin) * 0.5f;
				_itemTableauEntity.SetFrame(ref _itemTableauFrame, true);
				_midPoint = (_itemTableauEntity.GlobalBoxMax + _itemTableauEntity.GlobalBoxMin) * 0.5f + (globalBoxMax + globalBoxMin - _itemTableauEntity.GlobalBoxMax - _itemTableauEntity.GlobalBoxMin) * 0.5f;
			}
			else
			{
				_midPoint = (_itemTableauEntity.GlobalBoxMax + _itemTableauEntity.GlobalBoxMin) * 0.5f;
			}
			EquipmentElement equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
			if ((int)((EquipmentElement)(ref equipmentElement)).Item.ItemType != 17)
			{
				equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
				if ((int)((EquipmentElement)(ref equipmentElement)).Item.ItemType != 8)
				{
					goto IL_02d3;
				}
			}
			_midPoint *= 1.2f;
			goto IL_02d3;
		}
		goto IL_02d9;
		IL_02d3:
		ResetCamera();
		goto IL_02d9;
		IL_02d9:
		_isRotatingByDefault = true;
		_isRotating = false;
	}

	public void Initialize()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		_isRotatingByDefault = true;
		_isRotating = false;
		_isTranslating = false;
		_tableauScene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)0, "mono_renderscene");
		_tableauScene.SetName("ItemTableau");
		_tableauScene.DisableStaticShadows(true);
		_tableauScene.SetAtmosphereWithName("character_menu_a");
		Vec3 val = new Vec3(2.15f, 2.2f, 2.25f, -1f) * 2.25f;
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(1f, -1f, -1f, -1f);
		_tableauScene.SetSunLight(ref val, ref val2);
		_tableauScene.SetClothSimulationState(false);
		ResetCamera();
		_initialized = true;
	}

	private void TranslateCamera(bool value)
	{
		TranslateCameraAux(value);
	}

	private void TranslateCameraAux(bool value)
	{
		_isRotatingByDefault = !value && _isRotatingByDefault;
		_isTranslating = value;
		UpdateMouseLock();
	}

	private void ResetCamera()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		_curCamDisplacement = Vec3.Zero;
		_curZoomSpeed = 0f;
		if ((NativeObject)(object)_camera != (NativeObject)null)
		{
			_camera.Frame = MatrixFrame.Identity;
			SetCamFovHorizontal(1f);
			MakeCameraLookMidPoint();
		}
	}

	public void RotateItem(bool value)
	{
		_isRotatingByDefault = !value && _isRotatingByDefault;
		_isRotating = value;
		UpdateMouseLock();
	}

	public void RotateItemVerticalWithAmount(float value)
	{
		UpdateRotation(0f, value / -2f);
	}

	public void RotateItemHorizontalWithAmount(float value)
	{
		UpdateRotation(value / 2f, 0f);
	}

	public void OnTick(float dt)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		float mouseMoveX = Input.MouseMoveX;
		Vec2 keyState = Input.GetKeyState((InputKey)222);
		float num = mouseMoveX + ((Vec2)(ref keyState)).X * 1000f * dt;
		float mouseMoveY = Input.MouseMoveY;
		keyState = Input.GetKeyState((InputKey)222);
		float num2 = mouseMoveY + ((Vec2)(ref keyState)).Y * -1000f * dt;
		if (_isEnabled && (_isRotating || _isTranslating) && (!MBMath.ApproximatelyEqualsTo(num, 0f, 1E-05f) || !MBMath.ApproximatelyEqualsTo(num2, 0f, 1E-05f)))
		{
			if (_isRotating)
			{
				UpdateRotation(num, num2);
			}
			if (_isTranslating)
			{
				UpdatePosition(num, num2);
			}
		}
		TickCameraZoom(dt);
	}

	private void UpdatePosition(float mouseMoveX, float mouseMoveY)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized)
		{
			Vec3 val = default(Vec3);
			((Vec3)(ref val))._002Ector(mouseMoveX / (float)(-_tableauSizeX), mouseMoveY / (float)_tableauSizeY, 0f, -1f);
			val *= 2.2f * _camera.HorizontalFov;
			_curCamDisplacement += val;
			MakeCameraLookMidPoint();
		}
	}

	private void UpdateRotation(float mouseMoveX, float mouseMoveY)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized)
		{
			_panRotation += mouseMoveX * 0.004363323f;
			_tiltRotation += mouseMoveY * 0.004363323f;
			_tiltRotation = MathF.Clamp(_tiltRotation, MathF.PI * -19f / 20f, -MathF.PI / 20f);
			MatrixFrame val = _itemTableauEntity.GetFrame();
			Vec3 val2 = (_itemTableauEntity.GetBoundingBoxMax() + _itemTableauEntity.GetBoundingBoxMin()) * 0.5f;
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = val2;
			MatrixFrame identity2 = MatrixFrame.Identity;
			identity2.origin = -val2;
			val = (ref val) * (ref identity);
			val.rotation = Mat3.Identity;
			ref Mat3 rotation = ref val.rotation;
			Vec3 scaleVector = ((Mat3)(ref _initialFrame.rotation)).GetScaleVector();
			((Mat3)(ref rotation)).ApplyScaleLocal(ref scaleVector);
			((Mat3)(ref val.rotation)).RotateAboutSide(_tiltRotation);
			((Mat3)(ref val.rotation)).RotateAboutUp(_panRotation);
			val = (ref val) * (ref identity2);
			_itemTableauEntity.SetFrame(ref val, true);
		}
	}

	public void SetInitialTiltRotation(float amount)
	{
		_hasInitialTiltRotation = true;
		_initialTiltRotation = amount;
	}

	public void SetInitialPanRotation(float amount)
	{
		_hasInitialPanRotation = true;
		_initialPanRotation = amount;
	}

	public void Zoom(double value)
	{
		_curZoomSpeed -= (float)(value / 1000.0);
	}

	public void SetItem(ItemRosterElement itemRosterElement)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_itemRosterElement = itemRosterElement;
		RefreshItemTableau();
	}

	private void RefreshItemTableau()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Invalid comparison between I4 and Unknown
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Invalid comparison between I4 and Unknown
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Invalid comparison between Unknown and I4
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0502: Invalid comparison between I4 and Unknown
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Invalid comparison between Unknown and I4
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Invalid comparison between I4 and Unknown
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Invalid comparison between Unknown and I4
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05da: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Invalid comparison between I4 and Unknown
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Invalid comparison between Unknown and I4
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0630: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Invalid comparison between I4 and Unknown
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Invalid comparison between Unknown and I4
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Invalid comparison between Unknown and I4
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Invalid comparison between Unknown and I4
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Expected O, but got Unknown
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_069b: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Invalid comparison between Unknown and I4
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Expected O, but got Unknown
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			Initialize();
		}
		if (_itemTableauEntity != (GameEntity)null)
		{
			_itemTableauEntity.Remove(102);
			_itemTableauEntity = null;
		}
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
		if (((EquipmentElement)(ref equipmentElement)).Item == null)
		{
			return;
		}
		equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
		ItemTypeEnum itemType = ((EquipmentElement)(ref equipmentElement)).Item.ItemType;
		if (_itemTableauEntity == (GameEntity)null)
		{
			MatrixFrame itemFrameForItemTooltip = _itemRosterElement.GetItemFrameForItemTooltip();
			itemFrameForItemTooltip.origin.z += 2.5f;
			MetaMesh itemMeshForInventory = _itemRosterElement.GetItemMeshForInventory();
			uint color = 0u;
			uint color2 = 0u;
			if (!string.IsNullOrEmpty(_bannerCode))
			{
				Banner val = new Banner(_bannerCode);
				color = val.GetPrimaryColor();
				BannerColor val2 = default(BannerColor);
				if (((List<BannerData>)(object)val.BannerDataList).Count > 0 && BannerManager.Instance.ReadOnlyColorPalette.TryGetValue(((List<BannerData>)(object)val.BannerDataList)[1].ColorId, ref val2))
				{
					color2 = ((BannerColor)(ref val2)).Color;
				}
			}
			if ((NativeObject)(object)itemMeshForInventory != (NativeObject)null)
			{
				if ((int)itemType == 17)
				{
					_itemTableauEntity = GameEntity.CreateEmpty(_tableauScene, true, true, true);
					AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(Game.Current.DefaultMonster, MBActionSet.GetActionSet(Game.Current.DefaultMonster.ActionSetCode), 1f, false);
					GameEntityExtensions.CreateSkeletonWithActionSet(_itemTableauEntity, ref val3);
					_itemTableauEntity.SetFrame(ref itemFrameForItemTooltip, true);
					MBSkeletonExtensions.SetAgentActionChannel(_itemTableauEntity.Skeleton, 0, ref ActionIndexCache.act_tableau_hand_armor_pose, 0f, -0.2f, true, 0f);
					_itemTableauEntity.AddMultiMeshToSkeleton(itemMeshForInventory);
					MBSkeletonExtensions.TickActionChannels(_itemTableauEntity.Skeleton);
					_itemTableauEntity.Skeleton.TickAnimationsAndForceUpdate(0.01f, itemFrameForItemTooltip, true);
				}
				else if ((int)itemType == 1 || (int)itemType == 21)
				{
					equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
					HorseComponent horseComponent = ((EquipmentElement)(ref equipmentElement)).Item.HorseComponent;
					Monster monster = horseComponent.Monster;
					_itemTableauEntity = GameEntity.CreateEmpty(_tableauScene, true, true, true);
					AnimationSystemData val4 = MonsterExtensions.FillAnimationSystemData(monster, MBGlobals.GetActionSet(horseComponent.Monster.ActionSetCode), 1f, false);
					GameEntityExtensions.CreateSkeletonWithActionSet(_itemTableauEntity, ref val4);
					MBSkeletonExtensions.SetAgentActionChannel(_itemTableauEntity.Skeleton, 0, ref ActionIndexCache.act_inventory_idle_start, 0f, -0.2f, true, 0f);
					_itemTableauEntity.SetFrame(ref itemFrameForItemTooltip, true);
					_itemTableauEntity.AddMultiMeshToSkeleton(itemMeshForInventory);
				}
				else
				{
					if ((int)itemType == 25)
					{
						equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
						if (((EquipmentElement)(ref equipmentElement)).Item.ArmorComponent != null)
						{
							_itemTableauEntity = _tableauScene.AddItemEntity(ref itemFrameForItemTooltip, itemMeshForInventory);
							equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
							MetaMesh copy = MetaMesh.GetCopy(((EquipmentElement)(ref equipmentElement)).Item.ArmorComponent.ReinsMesh, true, true);
							if ((NativeObject)(object)copy != (NativeObject)null)
							{
								_itemTableauEntity.AddMultiMesh(copy, true);
							}
							goto IL_0478;
						}
					}
					if ((int)itemType == 8)
					{
						if (!string.IsNullOrEmpty(_bannerCode))
						{
							Banner val5 = new Banner(_bannerCode);
							equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
							if (((EquipmentElement)(ref equipmentElement)).Item.IsUsingTableau && !val5.IsBannerDataListEmpty())
							{
								equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
								itemMeshForInventory.SetMaterial(((EquipmentElement)(ref equipmentElement)).Item.GetTableauMaterial(val5));
							}
						}
						_itemTableauEntity = _tableauScene.AddItemEntity(ref itemFrameForItemTooltip, itemMeshForInventory);
					}
					else if ((int)itemType == 26)
					{
						if (!string.IsNullOrEmpty(_bannerCode))
						{
							Banner val6 = new Banner(_bannerCode);
							equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
							if (((EquipmentElement)(ref equipmentElement)).Item.IsUsingTableau && !val6.IsBannerDataListEmpty())
							{
								equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
								itemMeshForInventory.SetMaterial(((EquipmentElement)(ref equipmentElement)).Item.GetTableauMaterial(val6));
							}
							for (int i = 0; i < itemMeshForInventory.MeshCount; i++)
							{
								itemMeshForInventory.GetMeshAtIndex(i).Color = color;
								itemMeshForInventory.GetMeshAtIndex(i).Color2 = color2;
							}
						}
						_itemTableauEntity = _tableauScene.AddItemEntity(ref itemFrameForItemTooltip, itemMeshForInventory);
					}
					else
					{
						_itemTableauEntity = _tableauScene.AddItemEntity(ref itemFrameForItemTooltip, itemMeshForInventory);
					}
				}
			}
			else
			{
				equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
				MBDebug.ShowWarning("[DEBUG]Item with " + ((MBObjectBase)((EquipmentElement)(ref equipmentElement)).Item).StringId + "[DEBUG] string id cannot be found");
			}
		}
		goto IL_0478;
		IL_0478:
		MetaMesh val7 = null;
		SkinMask val8 = (SkinMask)481;
		equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
		if (((EquipmentElement)(ref equipmentElement)).Item.HasArmorComponent)
		{
			equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
			val8 = ((EquipmentElement)(ref equipmentElement)).Item.ArmorComponent.MeshesMask;
		}
		string text = "";
		equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
		bool flag = Extensions.HasAnyFlag<ItemFlags>(((EquipmentElement)(ref equipmentElement)).Item.ItemFlags, (ItemFlags)2048);
		bool flag2 = false;
		if (14 == (int)itemType || 24 == (int)itemType)
		{
			text = "base_head";
			flag2 = true;
		}
		else if (15 == (int)itemType)
		{
			if (Extensions.HasAnyFlag<SkinMask>(val8, (SkinMask)32))
			{
				text = "base_body";
				flag2 = true;
			}
		}
		else if (16 == (int)itemType)
		{
			if (Extensions.HasAnyFlag<SkinMask>(val8, (SkinMask)256))
			{
				text = "base_foot";
				flag2 = true;
			}
		}
		else if (17 == (int)itemType)
		{
			if (Extensions.HasAnyFlag<SkinMask>(val8, (SkinMask)128))
			{
				MetaMesh copy2 = MetaMesh.GetCopy(flag ? "base_hand_female" : "base_hand", false, false);
				_itemTableauEntity.AddMultiMeshToSkeleton(copy2);
			}
		}
		else if (25 == (int)itemType)
		{
			text = "horse_base_mesh";
			flag2 = false;
		}
		if (text.Length > 0)
		{
			if (flag2 && flag)
			{
				text += "_female";
			}
			val7 = MetaMesh.GetCopy(text, false, false);
			_itemTableauEntity.AddMultiMesh(val7, true);
		}
		TableauView view = View;
		if ((NativeObject)(object)view != (NativeObject)null)
		{
			Vec3 val9 = _itemTableauEntity.GetBoundingBoxMax() - _itemTableauEntity.GetBoundingBoxMin();
			float num = ((Vec3)(ref val9)).Length * 2f;
			Vec3 origin = _itemTableauEntity.GetGlobalFrame().origin;
			((SceneView)view).SetFocusedShadowmap(true, ref origin, num);
		}
		if (_itemTableauEntity != (GameEntity)null)
		{
			_initialFrame = _itemTableauEntity.GetFrame();
			Vec3 eulerAngles = ((Mat3)(ref _initialFrame.rotation)).GetEulerAngles();
			_panRotation = eulerAngles.x;
			_tiltRotation = eulerAngles.z;
			if (_hasInitialPanRotation)
			{
				_panRotation = _initialPanRotation;
			}
			else if ((int)itemType == 8)
			{
				_panRotation = -MathF.PI;
			}
			if (_hasInitialTiltRotation)
			{
				_tiltRotation = _initialTiltRotation;
			}
			else if ((int)itemType == 8)
			{
				_tiltRotation = 0f;
			}
			else
			{
				_tiltRotation = -MathF.PI / 2f;
			}
		}
	}

	private void TableauMaterialTabInventoryItemTooltipOnRender(Texture sender, EventArgs e)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			return;
		}
		TableauView val = View;
		if ((NativeObject)(object)val == (NativeObject)null)
		{
			val = sender.TableauView;
			((View)val).SetEnable(_isEnabled);
		}
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref _itemRosterElement)).EquipmentElement;
		if (((EquipmentElement)(ref equipmentElement)).Item == null)
		{
			val.SetContinuousRendering(false);
			val.SetDeleteAfterRendering(true);
			return;
		}
		((SceneView)val).SetRenderWithPostfx(true);
		((View)val).SetClearColor(0u);
		((SceneView)val).SetScene(_tableauScene);
		if ((NativeObject)(object)_camera == (NativeObject)null)
		{
			_camera = Camera.CreateCamera();
			_camera.SetViewVolume(true, -0.5f, 0.5f, -0.5f, 0.5f, 0.01f, 100f);
			ResetCamera();
			((SceneView)val).SetSceneUsesSkybox(false);
		}
		((SceneView)val).SetCamera(_camera);
		if (_isRotatingByDefault)
		{
			UpdateRotation(1f, 0f);
		}
		val.SetDeleteAfterRendering(false);
		val.SetContinuousRendering(true);
	}

	private void MakeCameraLookMidPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame frame = _camera.Frame;
		Vec3 val = ((Mat3)(ref frame.rotation)).TransformToParent(ref _curCamDisplacement);
		Vec3 val2 = _midPoint + val;
		float num = ((Vec3)(ref _midPoint)).Length * 0.5263158f;
		Vec3 position = val2 - _camera.Direction * num;
		_camera.Position = position;
	}

	private void SetCamFovHorizontal(float camFov)
	{
		_camera.SetFovHorizontal(camFov, 1f, 0.1f, 50f);
	}

	private void UpdateMouseLock(bool forceUnlock = false)
	{
		_lockMouse = (_isRotating || _isTranslating) && !forceUnlock;
		MouseManager.LockCursorAtCurrentPosition(_lockMouse);
		MouseManager.ShowCursor(!_lockMouse);
	}

	private void TickCameraZoom(float dt)
	{
		if ((NativeObject)(object)_camera != (NativeObject)null)
		{
			float horizontalFov = _camera.HorizontalFov;
			horizontalFov += _curZoomSpeed;
			float num = 0.61086524f;
			float num2 = MathF.PI * 13f / 36f;
			horizontalFov = MBMath.ClampFloat(horizontalFov, num, num2);
			SetCamFovHorizontal(horizontalFov);
			if (dt > 0f)
			{
				_curZoomSpeed = MBMath.Lerp(_curZoomSpeed, 0f, MBMath.ClampFloat(dt * 25.9f, 0f, 1f), 1E-05f);
			}
		}
	}
}
