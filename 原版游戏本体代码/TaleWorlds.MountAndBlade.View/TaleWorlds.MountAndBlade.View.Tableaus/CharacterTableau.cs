using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class CharacterTableau
{
	private static int _tableauIndex;

	private bool _isFinalized;

	private MatrixFrame _mountSpawnPoint;

	private MatrixFrame _bannerSpawnPoint;

	private float _animationFrequencyThreshold = 2.5f;

	private MatrixFrame _initialSpawnFrame;

	private MatrixFrame _characterMountPositionFrame;

	private MatrixFrame _mountCharacterPositionFrame;

	private AgentVisuals _agentVisuals;

	private AgentVisuals _mountVisuals;

	private int _agentVisualLoadingCounter;

	private int _mountVisualLoadingCounter;

	private AgentVisuals _oldAgentVisuals;

	private AgentVisuals _oldMountVisuals;

	private int _initialLoadingCounter;

	private ActionIndexCache _idleAction = ActionIndexCache.act_none;

	private string _idleFaceAnim;

	private Scene _tableauScene;

	private MBAgentRendererSceneController _agentRendererSceneController;

	private Camera _continuousRenderCamera;

	private float _cameraRatio;

	private MatrixFrame _camPos;

	private MatrixFrame _camPosGatheredFromScene;

	private string _charStringId;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private uint _clothColor1;

	private uint _clothColor2;

	private bool _isRotatingCharacter;

	private bool _isCharacterMountPlacesSwapped;

	private string _mountCreationKey;

	private string _equipmentCode;

	private bool _isEquipmentAnimActive;

	private float _animationGap;

	private float _mainCharacterRotation;

	private bool _isEnabled;

	private float _renderScale;

	private float _customRenderScale;

	private int _latestWidth;

	private int _latestHeight;

	private string _bodyPropertiesCode;

	private BodyProperties _bodyProperties;

	private bool _isFemale;

	private StanceTypes _stanceIndex;

	private Equipment _equipment;

	private Banner _banner;

	private int _race;

	private bool _isBannerShownInBackground;

	private ItemObject _bannerItem;

	private GameEntity _bannerEntity;

	private int _leftHandEquipmentIndex;

	private int _rightHandEquipmentIndex;

	private bool _isEquipmentIndicesDirty;

	private bool _customAnimationStartScheduled;

	private float _customAnimationTimer;

	private string _customAnimationName;

	private ActionIndexCache _customAnimation;

	private MBActionSet _characterActionSet;

	private bool _isVisualsDirty;

	private Equipment _oldEquipment;

	public Texture Texture { get; private set; }

	public bool IsRunningCustomAnimation
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if (!(_customAnimation != ActionIndexCache.act_none))
			{
				return _customAnimationStartScheduled;
			}
			return true;
		}
	}

	public bool ShouldLoopCustomAnimation { get; set; }

	public float CustomAnimationWaitDuration { get; set; }

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

	public CharacterTableau()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		Color val = new Color(1f, 1f, 1f, 1f);
		_clothColor1 = ((Color)(ref val)).ToUnsignedInteger();
		val = new Color(1f, 1f, 1f, 1f);
		_clothColor2 = ((Color)(ref val)).ToUnsignedInteger();
		_mountCreationKey = "";
		_equipmentCode = "";
		_renderScale = 1f;
		_customRenderScale = 1f;
		_latestWidth = -1;
		_latestHeight = -1;
		_bodyProperties = BodyProperties.Default;
		_customAnimation = ActionIndexCache.act_none;
		base._002Ector();
		_leftHandEquipmentIndex = -1;
		_rightHandEquipmentIndex = -1;
		_isVisualsDirty = false;
		_equipment = new Equipment();
		SetEnabled(enabled: true);
		FirstTimeInit();
	}

	public void OnTick(float dt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		if (_customAnimationStartScheduled)
		{
			StartCustomAnimation();
		}
		if (_customAnimation != ActionIndexCache.act_none && ((MBActionSet)(ref _characterActionSet)).IsValid)
		{
			_customAnimationTimer += dt;
			float actionAnimationDuration = MBActionSet.GetActionAnimationDuration(_characterActionSet, ref _customAnimation);
			if (_customAnimationTimer > actionAnimationDuration)
			{
				if (_customAnimationTimer > actionAnimationDuration + CustomAnimationWaitDuration)
				{
					if (ShouldLoopCustomAnimation)
					{
						StartCustomAnimation();
					}
					else
					{
						StopCustomAnimationIfCantContinue();
					}
				}
				else
				{
					_agentVisuals?.SetAction(GetIdleAction());
				}
			}
		}
		if (_isEnabled && _isRotatingCharacter)
		{
			UpdateCharacterRotation((int)Input.MouseMoveX);
		}
		if (_animationFrequencyThreshold > _animationGap)
		{
			_animationGap += dt;
		}
		if (_isEnabled)
		{
			_agentVisuals?.TickVisuals();
			_oldAgentVisuals?.TickVisuals();
			_mountVisuals?.TickVisuals();
			_oldMountVisuals?.TickVisuals();
		}
		if ((NativeObject)(object)View != (NativeObject)null)
		{
			if ((NativeObject)(object)_continuousRenderCamera == (NativeObject)null)
			{
				_continuousRenderCamera = Camera.CreateCamera();
			}
			View.SetDoNotRenderThisFrame(false);
		}
		if (_isVisualsDirty)
		{
			RefreshCharacterTableau(_oldEquipment);
			_oldEquipment = null;
			_isVisualsDirty = false;
		}
		if (_agentVisualLoadingCounter > 0 && _agentVisuals.GetEntity().CheckResources(true, true))
		{
			_agentVisualLoadingCounter--;
		}
		if (_mountVisualLoadingCounter > 0 && _mountVisuals.GetEntity().CheckResources(true, true))
		{
			_mountVisualLoadingCounter--;
		}
		if (_mountVisualLoadingCounter == 0 && _agentVisualLoadingCounter == 0)
		{
			_oldMountVisuals?.SetVisible(value: false);
			_mountVisuals?.SetVisible(_bodyProperties != BodyProperties.Default);
			_oldAgentVisuals?.SetVisible(value: false);
			_agentVisuals?.SetVisible(_bodyProperties != BodyProperties.Default);
		}
		if (_isEquipmentIndicesDirty)
		{
			_agentVisuals.GetVisuals().SetWieldedWeaponIndices(_rightHandEquipmentIndex, _leftHandEquipmentIndex);
			_isEquipmentIndicesDirty = false;
		}
	}

	public float GetCustomAnimationProgressRatio()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (_customAnimation != ActionIndexCache.act_none && ((MBActionSet)(ref _characterActionSet)).IsValid)
		{
			float actionAnimationDuration = MBActionSet.GetActionAnimationDuration(_characterActionSet, ref _customAnimation);
			if (actionAnimationDuration == 0f)
			{
				return -1f;
			}
			return _customAnimationTimer / actionAnimationDuration;
		}
		return -1f;
	}

	private void StopCustomAnimationIfCantContinue()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (_agentVisuals != null && _customAnimation != ActionIndexCache.act_none)
		{
			ActionIndexCache actionAnimationContinueToAction = MBActionSet.GetActionAnimationContinueToAction(_characterActionSet, ref _customAnimation);
			if (((ActionIndexCache)(ref actionAnimationContinueToAction)).Index >= 0)
			{
				_customAnimationName = ((ActionIndexCache)(ref actionAnimationContinueToAction)).GetName();
				StartCustomAnimation();
				flag = true;
			}
		}
		if (!flag)
		{
			StopCustomAnimation();
			_customAnimationTimer = -1f;
		}
	}

	private void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(_isEnabled);
		}
	}

	public void SetLeftHandWieldedEquipmentIndex(int index)
	{
		_leftHandEquipmentIndex = index;
		_isEquipmentIndicesDirty = true;
	}

	public void SetRightHandWieldedEquipmentIndex(int index)
	{
		_rightHandEquipmentIndex = index;
		_isEquipmentIndicesDirty = true;
	}

	public void SetTargetSize(int width, int height)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		_isRotatingCharacter = false;
		_latestWidth = width;
		_latestHeight = height;
		if (width <= 0 || height <= 0)
		{
			_tableauSizeX = 10;
			_tableauSizeY = 10;
		}
		else
		{
			_renderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			_tableauSizeX = (int)((float)width * _customRenderScale * _renderScale);
			_tableauSizeY = (int)((float)height * _customRenderScale * _renderScale);
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
		Texture = TableauView.AddTableau($"CharacterTableau_{_tableauIndex++}", new TextureUpdateEventHandler(CharacterTableauContinuousRenderFunction), (object)_tableauScene, _tableauSizeX, _tableauSizeY);
		((SceneView)Texture.TableauView).SetSceneUsesContour(false);
		((SceneView)Texture.TableauView).SetFocusedShadowmap(true, ref _initialSpawnFrame.origin, 2.55f);
	}

	public void SetCharStringID(string charStringId)
	{
		_charStringId = charStringId;
	}

	public void OnFinalize()
	{
		Camera continuousRenderCamera = _continuousRenderCamera;
		if ((NativeObject)(object)continuousRenderCamera != (NativeObject)null)
		{
			continuousRenderCamera.ReleaseCameraEntity();
			_continuousRenderCamera = null;
		}
		_agentVisuals?.ResetNextFrame();
		_agentVisuals = null;
		_mountVisuals?.ResetNextFrame();
		_mountVisuals = null;
		_oldAgentVisuals?.ResetNextFrame();
		_oldAgentVisuals = null;
		_oldMountVisuals?.ResetNextFrame();
		_oldMountVisuals = null;
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		if ((NativeObject)(object)_tableauScene != (NativeObject)null)
		{
			if (_bannerEntity != (GameEntity)null)
			{
				_tableauScene.RemoveEntity(_bannerEntity, 0);
				_bannerEntity = null;
			}
			if (_agentRendererSceneController != null)
			{
				if (view != null)
				{
					((View)view).SetEnable(false);
				}
				if (view != null)
				{
					((SceneView)view).AddClearTask(false);
				}
				MBAgentRendererSceneController.DestructAgentRendererSceneController(_tableauScene, _agentRendererSceneController, false);
				_agentRendererSceneController = null;
				((NativeObject)_tableauScene).ManualInvalidate();
				_tableauScene = null;
			}
			else
			{
				ThumbnailCacheManager.Current.ReturnCachedInventoryTableauScene();
				ThumbnailCacheManager.Current.ReturnCachedInventoryTableauScene();
				if (view != null)
				{
					((SceneView)view).AddClearTask(true);
				}
				Scene tableauScene = _tableauScene;
				if (tableauScene != null)
				{
					((NativeObject)tableauScene).ManualInvalidate();
				}
				_tableauScene = null;
			}
		}
		Texture texture = Texture;
		if (texture != null)
		{
			texture.Release();
		}
		Texture = null;
		_isFinalized = true;
	}

	public void SetBodyProperties(string bodyPropertiesCode)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (_bodyPropertiesCode != bodyPropertiesCode)
		{
			_bodyPropertiesCode = bodyPropertiesCode;
			BodyProperties bodyProperties = default(BodyProperties);
			if (!string.IsNullOrEmpty(bodyPropertiesCode) && BodyProperties.FromString(bodyPropertiesCode, ref bodyProperties))
			{
				_bodyProperties = bodyProperties;
			}
			else
			{
				_bodyProperties = BodyProperties.Default;
			}
			_isVisualsDirty = true;
		}
	}

	public void SetStanceIndex(int index)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_stanceIndex = (StanceTypes)index;
		_isVisualsDirty = true;
	}

	public void SetCustomRenderScale(float value)
	{
		if (!MBMath.ApproximatelyEqualsTo(_customRenderScale, value, 1E-05f))
		{
			_customRenderScale = value;
			if (_latestWidth != -1 && _latestHeight != -1)
			{
				SetTargetSize(_latestWidth, _latestHeight);
			}
		}
	}

	private void AdjustCharacterForStanceIndex()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		StanceTypes stanceIndex = _stanceIndex;
		switch ((int)stanceIndex)
		{
		case 1:
			_camPos = _camPosGatheredFromScene;
			((MatrixFrame)(ref _camPos)).Elevate(-2f);
			((MatrixFrame)(ref _camPos)).Advance(0.5f);
			_agentVisuals?.SetAction(GetIdleAction());
			_oldAgentVisuals?.SetAction(GetIdleAction());
			break;
		case 2:
		case 4:
			if (_agentVisuals != null)
			{
				_camPos = _camPosGatheredFromScene;
				EquipmentElement val = _equipment[10];
				if (((EquipmentElement)(ref val)).Item != null)
				{
					((MatrixFrame)(ref _camPos)).Advance(0.5f);
					_agentVisuals.SetAction(MBSkeletonExtensions.GetActionAtChannel(_mountVisuals.GetEntity().Skeleton, 0), _mountVisuals.GetEntity().Skeleton.GetAnimationParameterAtChannel(0));
					_oldAgentVisuals.SetAction(MBSkeletonExtensions.GetActionAtChannel(_mountVisuals.GetEntity().Skeleton, 0), _mountVisuals.GetEntity().Skeleton.GetAnimationParameterAtChannel(0));
				}
				else
				{
					((MatrixFrame)(ref _camPos)).Elevate(-2f);
					((MatrixFrame)(ref _camPos)).Advance(0.5f);
					_agentVisuals.SetAction(GetIdleAction());
					_oldAgentVisuals.SetAction(GetIdleAction());
				}
			}
			break;
		case 3:
			_agentVisuals?.SetAction(in ActionIndexCache.act_cheer_1);
			_oldAgentVisuals?.SetAction(in ActionIndexCache.act_cheer_1);
			break;
		case 0:
			_agentVisuals?.SetAction(GetIdleAction());
			_oldAgentVisuals?.SetAction(GetIdleAction());
			break;
		}
		if (_agentVisuals != null)
		{
			GameEntity entity = _agentVisuals.GetEntity();
			Skeleton skeleton = entity.Skeleton;
			skeleton.TickAnimations(0.01f, _agentVisuals.GetVisuals().GetGlobalFrame(), true);
			if (!string.IsNullOrEmpty(_idleFaceAnim))
			{
				MBSkeletonExtensions.SetFacialAnimation(skeleton, (FacialAnimChannel)1, _idleFaceAnim, false, true);
			}
			((NativeObject)entity).ManualInvalidate();
			((NativeObject)skeleton).ManualInvalidate();
		}
		if (_oldAgentVisuals != null)
		{
			GameEntity entity2 = _oldAgentVisuals.GetEntity();
			Skeleton skeleton2 = entity2.Skeleton;
			skeleton2.TickAnimations(0.01f, _oldAgentVisuals.GetVisuals().GetGlobalFrame(), true);
			if (!string.IsNullOrEmpty(_idleFaceAnim))
			{
				MBSkeletonExtensions.SetFacialAnimation(skeleton2, (FacialAnimChannel)1, _idleFaceAnim, false, true);
			}
			((NativeObject)entity2).ManualInvalidate();
			((NativeObject)skeleton2).ManualInvalidate();
		}
		if (_mountVisuals != null)
		{
			GameEntity entity3 = _mountVisuals.GetEntity();
			Skeleton skeleton3 = entity3.Skeleton;
			skeleton3.TickAnimations(0.01f, _mountVisuals.GetVisuals().GetGlobalFrame(), true);
			if (!string.IsNullOrEmpty(_idleFaceAnim))
			{
				MBSkeletonExtensions.SetFacialAnimation(skeleton3, (FacialAnimChannel)1, _idleFaceAnim, false, true);
			}
			((NativeObject)entity3).ManualInvalidate();
			((NativeObject)skeleton3).ManualInvalidate();
		}
		if (_oldMountVisuals != null)
		{
			GameEntity entity4 = _oldMountVisuals.GetEntity();
			Skeleton skeleton4 = entity4.Skeleton;
			skeleton4.TickAnimations(0.01f, _oldMountVisuals.GetVisuals().GetGlobalFrame(), true);
			((NativeObject)entity4).ManualInvalidate();
			((NativeObject)skeleton4).ManualInvalidate();
		}
	}

	private void ForceRefresh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected I4, but got Unknown
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		int stanceIndex = (int)_stanceIndex;
		_stanceIndex = (StanceTypes)0;
		SetStanceIndex(stanceIndex);
	}

	public void SetIsFemale(bool isFemale)
	{
		_isFemale = isFemale;
		_isVisualsDirty = true;
	}

	public void SetIsBannerShownInBackground(bool isBannerShownInBackground)
	{
		_isBannerShownInBackground = isBannerShownInBackground;
		_isVisualsDirty = true;
	}

	public void SetRace(int race)
	{
		_race = race;
		_isVisualsDirty = true;
	}

	public void SetIdleAction(string idleAction)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_idleAction = ActionIndexCache.Create(idleAction);
		_isVisualsDirty = true;
	}

	public void SetCustomAnimation(string animation)
	{
		_customAnimationName = animation;
	}

	public void StartCustomAnimation()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (_isVisualsDirty || _agentVisuals == null || string.IsNullOrEmpty(_customAnimationName))
		{
			_customAnimationStartScheduled = true;
			return;
		}
		StopCustomAnimation();
		_customAnimation = ActionIndexCache.Create(_customAnimationName);
		if (((ActionIndexCache)(ref _customAnimation)).Index >= 0)
		{
			_agentVisuals.SetAction(in _customAnimation);
			_customAnimationStartScheduled = false;
			_customAnimationTimer = 0f;
		}
		else
		{
			Debug.FailedAssert("Invalid custom animation in character tableau: " + _customAnimationName, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\CharacterTableau.cs", "StartCustomAnimation", 593);
		}
	}

	public void StopCustomAnimation()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (_agentVisuals != null && _customAnimation != ActionIndexCache.act_none)
		{
			ActionIndexCache actionAnimationContinueToAction = MBActionSet.GetActionAnimationContinueToAction(_characterActionSet, ref _customAnimation);
			if (((ActionIndexCache)(ref actionAnimationContinueToAction)).Index < 0)
			{
				_agentVisuals.SetAction(GetIdleAction());
			}
			_customAnimation = ActionIndexCache.act_none;
		}
	}

	public void SetIdleFaceAnim(string idleFaceAnim)
	{
		if (!string.IsNullOrEmpty(idleFaceAnim))
		{
			_idleFaceAnim = idleFaceAnim;
			_isVisualsDirty = true;
		}
	}

	public void SetEquipmentCode(string equipmentCode)
	{
		if (_equipmentCode != equipmentCode && !string.IsNullOrEmpty(equipmentCode))
		{
			_oldEquipment = Equipment.CreateFromEquipmentCode(_equipmentCode);
			_equipmentCode = equipmentCode;
			_equipment = Equipment.CreateFromEquipmentCode(equipmentCode);
			_bannerItem = GetAndRemoveBannerFromEquipment(ref _equipment);
			_isVisualsDirty = true;
		}
	}

	public void SetIsEquipmentAnimActive(bool value)
	{
		_isEquipmentAnimActive = value;
	}

	public void SetMountCreationKey(string value)
	{
		if (_mountCreationKey != value)
		{
			_mountCreationKey = value;
			_isVisualsDirty = true;
		}
	}

	public void SetBannerCode(string value)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		_banner = (string.IsNullOrEmpty(value) ? ((Banner)null) : new Banner(value));
		_isVisualsDirty = true;
	}

	public void SetArmorColor1(uint clothColor1)
	{
		if (_clothColor1 != clothColor1)
		{
			_clothColor1 = clothColor1;
			_isVisualsDirty = true;
		}
	}

	public void SetArmorColor2(uint clothColor2)
	{
		if (_clothColor2 != clothColor2)
		{
			_clothColor2 = clothColor2;
			_isVisualsDirty = true;
		}
	}

	private ActionIndexCache GetIdleAction()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!(_idleAction != ActionIndexCache.act_none))
		{
			return ActionIndexCache.act_inventory_idle_start;
		}
		return _idleAction;
	}

	private void RefreshCharacterTableau(Equipment oldEquipment = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		UpdateMount((int)_stanceIndex == 4);
		UpdateBannerItem();
		if (_mountVisuals == null && _isCharacterMountPlacesSwapped)
		{
			_isCharacterMountPlacesSwapped = false;
			_mainCharacterRotation = 0f;
		}
		if (_agentVisuals != null)
		{
			bool visibilityExcludeParents = _oldAgentVisuals.GetEntity().GetVisibilityExcludeParents();
			AgentVisuals oldAgentVisuals = _oldAgentVisuals;
			AgentVisuals agentVisuals = _agentVisuals;
			_agentVisuals = oldAgentVisuals;
			_oldAgentVisuals = agentVisuals;
			_agentVisualLoadingCounter = 1;
			AgentVisualsData copyAgentVisualsData = _agentVisuals.GetCopyAgentVisualsData();
			MatrixFrame val = (_isCharacterMountPlacesSwapped ? _characterMountPositionFrame : _initialSpawnFrame);
			if (!_isCharacterMountPlacesSwapped)
			{
				((Mat3)(ref val.rotation)).RotateAboutUp(_mainCharacterRotation);
			}
			_characterActionSet = MBGlobals.GetActionSetWithSuffix(copyAgentVisualsData.MonsterData, _isFemale, "_warrior");
			copyAgentVisualsData.BodyProperties(_bodyProperties).SkeletonType((SkeletonType)(_isFemale ? 1 : 0)).Frame(val)
				.ActionSet(_characterActionSet)
				.Equipment(_equipment)
				.Banner(_banner)
				.UseMorphAnims(true)
				.ClothColor1(_clothColor1)
				.ClothColor2(_clothColor2)
				.Race(_race);
			if (_initialLoadingCounter > 0)
			{
				_initialLoadingCounter--;
			}
			_agentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
			_agentVisuals.SetVisible(value: false);
			if (_initialLoadingCounter == 0)
			{
				_oldAgentVisuals.SetVisible(visibilityExcludeParents);
			}
			if (oldEquipment != null && _animationFrequencyThreshold <= _animationGap && _isEquipmentAnimActive)
			{
				EquipmentElement val2 = _equipment[(EquipmentIndex)8];
				if (((EquipmentElement)(ref val2)).Item != null)
				{
					val2 = oldEquipment[(EquipmentIndex)8];
					ItemObject item = ((EquipmentElement)(ref val2)).Item;
					val2 = _equipment[(EquipmentIndex)8];
					if (item != ((EquipmentElement)(ref val2)).Item)
					{
						MBSkeletonExtensions.SetAgentActionChannel(_agentVisuals.GetVisuals().GetSkeleton(), 0, ref ActionIndexCache.act_inventory_glove_equip, 0f, -0.2f, true, 0f);
						_animationGap = 0f;
						goto IL_028e;
					}
				}
				val2 = _equipment[(EquipmentIndex)6];
				if (((EquipmentElement)(ref val2)).Item != null)
				{
					val2 = oldEquipment[(EquipmentIndex)6];
					ItemObject item2 = ((EquipmentElement)(ref val2)).Item;
					val2 = _equipment[(EquipmentIndex)6];
					if (item2 != ((EquipmentElement)(ref val2)).Item)
					{
						MBSkeletonExtensions.SetAgentActionChannel(_agentVisuals.GetVisuals().GetSkeleton(), 0, ref ActionIndexCache.act_inventory_cloth_equip, 0f, -0.2f, true, 0f);
						_animationGap = 0f;
					}
				}
			}
			goto IL_028e;
		}
		goto IL_02a1;
		IL_028e:
		_agentVisuals.GetEntity().CheckResources(true, true);
		goto IL_02a1;
		IL_02a1:
		AdjustCharacterForStanceIndex();
	}

	public void RotateCharacter(bool value)
	{
		_isRotatingCharacter = value;
	}

	public void TriggerCharacterMountPlacesSwap()
	{
		_mainCharacterRotation = 0f;
		_isCharacterMountPlacesSwapped = !_isCharacterMountPlacesSwapped;
		_isVisualsDirty = true;
	}

	public void OnCharacterTableauMouseMove(int mouseMoveX)
	{
		UpdateCharacterRotation(mouseMoveX);
	}

	private void UpdateCharacterRotation(int mouseMoveX)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (_agentVisuals != null)
		{
			float num = (float)mouseMoveX * 0.005f;
			_mainCharacterRotation += num;
			if (_isCharacterMountPlacesSwapped)
			{
				MatrixFrame frame = _mountVisuals.GetEntity().GetFrame();
				((Mat3)(ref frame.rotation)).RotateAboutUp(num);
				_mountVisuals.GetEntity().SetFrame(ref frame, true);
			}
			else
			{
				MatrixFrame frame2 = _agentVisuals.GetEntity().GetFrame();
				((Mat3)(ref frame2.rotation)).RotateAboutUp(num);
				_agentVisuals.GetEntity().SetFrame(ref frame2, true);
			}
		}
	}

	private void FirstTimeInit()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_continuousRenderCamera == (NativeObject)null)
		{
			_continuousRenderCamera = Camera.CreateCamera();
		}
		if (_equipment == null)
		{
			return;
		}
		if ((NativeObject)(object)_tableauScene == (NativeObject)null)
		{
			if (ThumbnailCacheManager.Current.IsCachedInventoryTableauSceneUsed())
			{
				_tableauScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
				_tableauScene.SetName("CharacterTableau");
				_tableauScene.DisableStaticShadows(true);
				_tableauScene.SetClothSimulationState(true);
				_agentRendererSceneController = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_tableauScene);
				SceneInitializationData val = default(SceneInitializationData);
				((SceneInitializationData)(ref val))._002Ector(true);
				val.InitPhysicsWorld = false;
				val.DoNotUseLoadingScreen = true;
				_tableauScene.Read("inventory_character_scene", ref val, "");
			}
			else
			{
				_tableauScene = ThumbnailCacheManager.Current.GetCachedInventoryTableauScene();
			}
			_tableauScene.SetShadow(true);
			_tableauScene.SetClothSimulationState(true);
			_camPos = (_camPosGatheredFromScene = ThumbnailCacheManager.Current.InventorySceneCameraFrame);
			_mountSpawnPoint = _tableauScene.FindEntityWithTag("horse_inv").GetGlobalFrame();
			_bannerSpawnPoint = _tableauScene.FindEntityWithTag("banner_inv").GetGlobalFrame();
			_initialSpawnFrame = _tableauScene.FindEntityWithTag("agent_inv").GetGlobalFrame();
			_characterMountPositionFrame = new MatrixFrame(ref _initialSpawnFrame.rotation, ref _mountSpawnPoint.origin);
			((MatrixFrame)(ref _characterMountPositionFrame)).Strafe(-0.25f);
			_mountCharacterPositionFrame = new MatrixFrame(ref _mountSpawnPoint.rotation, ref _initialSpawnFrame.origin);
			((MatrixFrame)(ref _mountCharacterPositionFrame)).Strafe(0.25f);
			if (_agentRendererSceneController != null)
			{
				_tableauScene.RemoveEntity(_tableauScene.FindEntityWithTag("agent_inv"), 99);
				_tableauScene.RemoveEntity(_tableauScene.FindEntityWithTag("horse_inv"), 100);
				_tableauScene.RemoveEntity(_tableauScene.FindEntityWithTag("banner_inv"), 101);
			}
		}
		InitializeAgentVisuals();
		_isVisualsDirty = true;
	}

	private void InitializeAgentVisuals()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(_race);
		_characterActionSet = MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, _isFemale, "_warrior");
		AgentVisualsData obj = new AgentVisualsData().Banner(_banner).Equipment(_equipment).BodyProperties(_bodyProperties)
			.Race(_race)
			.Frame(_initialSpawnFrame)
			.UseMorphAnims(true)
			.ActionSet(_characterActionSet);
		ActionIndexCache idleAction = GetIdleAction();
		_oldAgentVisuals = AgentVisuals.Create(obj.ActionCode(ref idleAction).Scene(_tableauScene).Monster(baseMonsterFromRace)
			.PrepareImmediately(false)
			.SkeletonType((SkeletonType)(_isFemale ? 1 : 0))
			.ClothColor1(_clothColor1)
			.ClothColor2(_clothColor2)
			.CharacterObjectStringId(_charStringId), "CharacterTableau", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_oldAgentVisuals.SetAgentLodZeroOrMaxExternal(makeZero: true);
		_oldAgentVisuals.SetVisible(value: false);
		AgentVisualsData obj2 = new AgentVisualsData().Banner(_banner).Equipment(_equipment).BodyProperties(_bodyProperties)
			.Race(_race)
			.Frame(_initialSpawnFrame)
			.UseMorphAnims(true)
			.ActionSet(_characterActionSet);
		idleAction = GetIdleAction();
		_agentVisuals = AgentVisuals.Create(obj2.ActionCode(ref idleAction).Scene(_tableauScene).Monster(baseMonsterFromRace)
			.PrepareImmediately(false)
			.SkeletonType((SkeletonType)(_isFemale ? 1 : 0))
			.ClothColor1(_clothColor1)
			.ClothColor2(_clothColor2)
			.CharacterObjectStringId(_charStringId), "CharacterTableau", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
		_agentVisuals.SetAgentLodZeroOrMaxExternal(makeZero: true);
		_agentVisuals.SetVisible(value: false);
		_initialLoadingCounter = 2;
		if (!string.IsNullOrEmpty(_idleFaceAnim))
		{
			MBSkeletonExtensions.SetFacialAnimation(_agentVisuals.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, _idleFaceAnim, false, true);
			MBSkeletonExtensions.SetFacialAnimation(_oldAgentVisuals.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, _idleFaceAnim, false, true);
		}
	}

	private void UpdateMount(bool isRiderAgentMounted = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		EquipmentElement val = _equipment[(EquipmentIndex)10];
		ItemObject item = ((EquipmentElement)(ref val)).Item;
		if (((item != null) ? item.HorseComponent : null) != null)
		{
			val = _equipment[(EquipmentIndex)10];
			ItemObject item2 = ((EquipmentElement)(ref val)).Item;
			Monster monster = item2.HorseComponent.Monster;
			Equipment val2 = new Equipment
			{
				[(EquipmentIndex)10] = _equipment[(EquipmentIndex)10],
				[(EquipmentIndex)11] = _equipment[(EquipmentIndex)11]
			};
			MatrixFrame val3 = (_isCharacterMountPlacesSwapped ? _mountCharacterPositionFrame : _mountSpawnPoint);
			if (_isCharacterMountPlacesSwapped)
			{
				((Mat3)(ref val3.rotation)).RotateAboutUp(_mainCharacterRotation);
			}
			if (_oldMountVisuals != null)
			{
				_oldMountVisuals.ResetNextFrame();
			}
			_oldMountVisuals = _mountVisuals;
			_mountVisualLoadingCounter = 3;
			AgentVisualsData val4 = new AgentVisualsData();
			AgentVisualsData obj = val4.Banner(_banner).Equipment(val2).Frame(val3)
				.Scale(item2.ScaleFactor)
				.ActionSet(MBGlobals.GetActionSet(monster.ActionSetCode));
			ActionIndexCache val5 = ((!isRiderAgentMounted) ? GetIdleAction() : ((monster.MonsterUsage == "camel") ? ActionIndexCache.act_inventory_idle_start : ActionIndexCache.act_inventory_idle_start));
			obj.ActionCode(ref val5).Scene(_tableauScene).Monster(monster)
				.PrepareImmediately(false)
				.ClothColor1(_clothColor1)
				.ClothColor2(_clothColor2)
				.MountCreationKey(_mountCreationKey);
			_mountVisuals = AgentVisuals.Create(val4, "MountTableau", isRandomProgress: false, needBatchedVersionForWeaponMeshes: false, forceUseFaceCache: false);
			_mountVisuals.SetAgentLodZeroOrMaxExternal(makeZero: true);
			_mountVisuals.SetVisible(value: false);
			_mountVisuals.SetClothingColors(_clothColor1, _clothColor2);
			_mountVisuals.GetEntity().CheckResources(true, true);
		}
		else if (_mountVisuals != null)
		{
			_mountVisuals.Reset();
			_mountVisuals = null;
			_mountVisualLoadingCounter = 0;
		}
	}

	private void UpdateBannerItem()
	{
		if (_bannerEntity != (GameEntity)null)
		{
			_tableauScene.RemoveEntity(_bannerEntity, 0);
			_bannerEntity = null;
		}
		if (!_isBannerShownInBackground || _bannerItem == null)
		{
			return;
		}
		_bannerEntity = GameEntity.CreateEmpty(_tableauScene, true, true, true);
		_bannerEntity.SetFrame(ref _bannerSpawnPoint, true);
		_bannerEntity.AddMultiMesh(_bannerItem.GetMultiMeshCopy(), true);
		if (_banner != null)
		{
			_banner.GetTableauTextureLarge(BannerDebugInfo.CreateManual(GetType().Name), delegate(Texture t)
			{
				OnBannerTableauRenderDone(t);
			});
		}
	}

	private void OnBannerTableauRenderDone(Texture newTexture)
	{
		if (_isFinalized || _bannerEntity == (GameEntity)null)
		{
			return;
		}
		foreach (Mesh item in _bannerEntity.GetAllMeshesWithTag("banner_replacement_mesh"))
		{
			ApplyBannerTextureToMesh(item, newTexture);
		}
		Skeleton skeleton = _bannerEntity.Skeleton;
		if (((skeleton != null) ? skeleton.GetAllMeshes() : null) == null)
		{
			return;
		}
		Skeleton skeleton2 = _bannerEntity.Skeleton;
		foreach (Mesh item2 in (skeleton2 != null) ? skeleton2.GetAllMeshes() : null)
		{
			if (item2.HasTag("banner_replacement_mesh"))
			{
				ApplyBannerTextureToMesh(item2, newTexture);
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

	private ItemObject GetAndRemoveBannerFromEquipment(ref Equipment equipment)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		ItemObject result = null;
		EquipmentElement val = equipment[(EquipmentIndex)4];
		ItemObject item = ((EquipmentElement)(ref val)).Item;
		if (item != null && item.IsBannerItem)
		{
			val = equipment[(EquipmentIndex)4];
			result = ((EquipmentElement)(ref val)).Item;
			equipment[(EquipmentIndex)4] = EquipmentElement.Invalid;
		}
		return result;
	}

	internal void CharacterTableauContinuousRenderFunction(Texture sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		Scene val = (Scene)sender.UserData;
		TableauView tableauView = sender.TableauView;
		if ((NativeObject)(object)val == (NativeObject)null)
		{
			tableauView.SetContinuousRendering(false);
			tableauView.SetDeleteAfterRendering(true);
			return;
		}
		val.EnsurePostfxSystem();
		val.SetDofMode(false);
		val.SetMotionBlurMode(false);
		val.SetBloom(true);
		val.SetDynamicShadowmapCascadesRadiusMultiplier(0.31f);
		((SceneView)tableauView).SetRenderWithPostfx(true);
		float cameraRatio = _cameraRatio;
		MatrixFrame camPos = _camPos;
		Camera continuousRenderCamera = _continuousRenderCamera;
		if ((NativeObject)(object)continuousRenderCamera != (NativeObject)null)
		{
			continuousRenderCamera.SetFovVertical(MathF.PI / 4f, cameraRatio, 0.2f, 200f);
			continuousRenderCamera.Frame = camPos;
			((SceneView)tableauView).SetCamera(continuousRenderCamera);
			((SceneView)tableauView).SetScene(val);
			((SceneView)tableauView).SetSceneUsesSkybox(false);
			tableauView.SetDeleteAfterRendering(false);
			tableauView.SetContinuousRendering(true);
			tableauView.SetDoNotRenderThisFrame(true);
			((View)tableauView).SetClearColor(0u);
			((SceneView)tableauView).SetFocusedShadowmap(true, ref _initialSpawnFrame.origin, 1.55f);
		}
	}
}
