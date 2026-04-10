using System;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.View.Map;

public class MapCameraView : MapView
{
	public enum CameraFollowMode
	{
		Free,
		FollowParty,
		MoveToPosition
	}

	public struct InputInformation
	{
		public bool IsMainPartyValid;

		public bool IsMapReady;

		public bool IsControlDown;

		public bool IsMouseActive;

		public bool CheatModeEnabled;

		public bool LeftMouseButtonPressed;

		public bool LeftMouseButtonDown;

		public bool LeftMouseButtonReleased;

		public bool MiddleMouseButtonDown;

		public bool RightMouseButtonDown;

		public bool RotateLeftKeyDown;

		public bool RotateRightKeyDown;

		public bool PartyMoveUpKey;

		public bool PartyMoveDownKey;

		public bool PartyMoveLeftKey;

		public bool PartyMoveRightKey;

		public bool CameraFollowModeKeyPressed;

		public bool LeftButtonDraggingMode;

		public bool IsInMenu;

		public bool RayCastForClosestEntityOrTerrainCondition;

		public float MapZoomIn;

		public float MapZoomOut;

		public float DeltaMouseScroll;

		public float MouseSensitivity;

		public float MouseMoveX;

		public float MouseMoveY;

		public float HorizontalCameraInput;

		public float RX;

		public float RY;

		public float RS;

		public float Dt;

		public Vec2 MousePositionPixel;

		public Vec2 ClickedPositionPixel;

		public Vec3 ClickedPosition;

		public Vec3 ProjectedPosition;

		public Vec3 WorldMouseNear;

		public Vec3 WorldMouseFar;
	}

	private const float VerticalHalfViewAngle = 0.34906584f;

	private Vec3 _cameraTarget;

	private float _distanceToIdealCameraTargetToStopCameraSoundEventsSquared;

	private int _cameraMoveSfxSoundEventId;

	private SoundEvent _cameraMoveSfxSoundEvent;

	private bool _doFastCameraMovementToTarget;

	private float _cameraElevation;

	private CampaignVec2 _lastUsedIdealCameraTarget;

	private CampaignVec2 _cameraAnimationTarget;

	private float _cameraAnimationStopDuration;

	private readonly Scene _mapScene;

	protected float _customMaximumCameraHeight;

	private MatrixFrame _cameraFrame;

	protected virtual CameraFollowMode CurrentCameraFollowMode { get; set; }

	public virtual float CameraFastMoveMultiplier { get; protected set; }

	protected virtual float CameraBearing { get; set; }

	protected virtual float MaximumCameraHeight => Math.Max(_customMaximumCameraHeight, Campaign.MapMaximumHeight);

	protected virtual float CameraBearingVelocity { get; set; }

	public virtual float CameraDistance { get; protected set; }

	protected virtual float TargetCameraDistance { get; set; }

	protected virtual float AdditionalElevation { get; set; }

	public virtual bool CameraAnimationInProgress { get; protected set; }

	public virtual bool ProcessCameraInput { get; protected set; }

	public virtual Camera Camera { get; protected set; }

	public virtual MatrixFrame CameraFrame
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cameraFrame;
		}
		protected set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_cameraFrame = value;
		}
	}

	protected virtual Vec3 IdealCameraTarget { get; set; }

	private static MapCameraView Instance { get; set; }

	public MapCameraView()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		Camera = Camera.CreateCamera();
		Camera.SetViewVolume(true, -0.1f, 0.1f, -0.07f, 0.07f, 0.2f, 300f);
		Camera.Position = new Vec3(0f, 0f, 10f, -1f);
		CameraBearing = 0f;
		_cameraElevation = 1f;
		CameraDistance = 38f;
		TargetCameraDistance = 38f;
		ProcessCameraInput = true;
		CameraFastMoveMultiplier = 4f;
		_cameraFrame = MatrixFrame.Identity;
		CurrentCameraFollowMode = CameraFollowMode.FollowParty;
		_mapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
		Instance = this;
	}

	public virtual void OnActivate(bool leftButtonDraggingMode, Vec3 clickedPosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SetCameraMode(CameraFollowMode.FollowParty);
		CameraBearingVelocity = 0f;
		UpdateMapCamera(leftButtonDraggingMode, clickedPosition);
	}

	public virtual void Initialize()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (MobileParty.MainParty != null && PartyBase.MainParty.IsValid)
		{
			float num = 0f;
			IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
			CampaignVec2 position = MobileParty.MainParty.Position;
			mapSceneWrapper.GetHeightAtPoint(ref position, ref num);
			position = MobileParty.MainParty.Position;
			IdealCameraTarget = new Vec3(((CampaignVec2)(ref position)).ToVec2(), num + 1f, -1f);
		}
		_cameraMoveSfxSoundEventId = SoundEvent.GetEventIdFromString("event:/ui/campaign/focus");
		_cameraTarget = IdealCameraTarget;
	}

	protected internal override void OnFinalize()
	{
		base.OnFinalize();
		Instance = null;
	}

	public virtual void SetCameraMode(CameraFollowMode cameraMode)
	{
		CurrentCameraFollowMode = cameraMode;
	}

	public virtual void ResetCamera(bool resetDistance, bool teleportToMainParty)
	{
		if (teleportToMainParty)
		{
			TeleportCameraToMainParty();
		}
		if (resetDistance)
		{
			TargetCameraDistance = 15f;
			CameraDistance = 15f;
		}
		CameraBearing = 0f;
		_cameraElevation = 1f;
	}

	public virtual void TeleportCameraToMainParty()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		CurrentCameraFollowMode = CameraFollowMode.FollowParty;
		Campaign.Current.CameraFollowParty = MobileParty.MainParty.Party;
		IdealCameraTarget = GetCameraTargetForParty(Campaign.Current.CameraFollowParty);
		Vec3 idealCameraTarget = IdealCameraTarget;
		_lastUsedIdealCameraTarget = new CampaignVec2(((Vec3)(ref idealCameraTarget)).AsVec2, !MobileParty.MainParty.IsCurrentlyAtSea);
		_cameraTarget = IdealCameraTarget;
	}

	public virtual void FastMoveCameraToMainParty()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		CurrentCameraFollowMode = CameraFollowMode.FollowParty;
		Campaign.Current.CameraFollowParty = MobileParty.MainParty.Party;
		IdealCameraTarget = GetCameraTargetForParty(Campaign.Current.CameraFollowParty);
		_doFastCameraMovementToTarget = true;
		TargetCameraDistance = 15f;
		OnFastMoveCameraMovementStart();
	}

	public virtual void FastMoveCameraToPosition(CampaignVec2 target, bool isInMenu)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!isInMenu)
		{
			CurrentCameraFollowMode = CameraFollowMode.MoveToPosition;
			IdealCameraTarget = GetCameraTargetForPosition(target);
			_doFastCameraMovementToTarget = true;
			TargetCameraDistance = 15f;
			OnFastMoveCameraMovementStart();
		}
	}

	public void OnFastMoveCameraMovementStart()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Vec3 idealCameraTarget = IdealCameraTarget;
		_distanceToIdealCameraTargetToStopCameraSoundEventsSquared = ((Vec3)(ref idealCameraTarget)).DistanceSquared(_cameraTarget) * 0.15f;
		if (_cameraMoveSfxSoundEvent == null || !_cameraMoveSfxSoundEvent.IsPlaying())
		{
			_cameraMoveSfxSoundEvent = SoundEvent.CreateEvent(_cameraMoveSfxSoundEventId, _mapScene);
			_cameraMoveSfxSoundEvent.Play();
		}
	}

	public void StopCameraMovementSoundEvents()
	{
		if (_cameraMoveSfxSoundEvent != null && _cameraMoveSfxSoundEvent.IsPlaying())
		{
			_cameraMoveSfxSoundEvent.Release();
		}
	}

	public virtual bool IsCameraLockedToPlayerParty()
	{
		if (CurrentCameraFollowMode == CameraFollowMode.FollowParty)
		{
			return Campaign.Current.CameraFollowParty == MobileParty.MainParty.Party;
		}
		return false;
	}

	public virtual void StartCameraAnimation(CampaignVec2 targetPosition, float animationStopDuration)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		CameraAnimationInProgress = true;
		_cameraAnimationTarget = targetPosition;
		_cameraAnimationStopDuration = animationStopDuration;
		Campaign.Current.SetTimeSpeed(0);
		Campaign.Current.SetTimeControlModeLock(true);
	}

	public virtual void SiegeEngineClick(MatrixFrame siegeEngineFrame)
	{
		if (TargetCameraDistance > 18f)
		{
			TargetCameraDistance = 18f;
		}
	}

	public virtual void OnExit()
	{
		ProcessCameraInput = true;
	}

	public virtual void OnEscapeMenuToggled(bool isOpened)
	{
		ProcessCameraInput = !isOpened;
	}

	public virtual void HandleMouse(bool rightMouseButtonPressed, float verticalCameraInput, float mouseMoveY, float dt)
	{
		float num = 0.3f / 700f;
		float num2 = (0f - (700f - MathF.Min(700f, MathF.Max(50f, CameraDistance)))) * num;
		float num3 = MathF.Max(num2 + 1E-05f, MathF.PI * 99f / 200f - CalculateCameraElevation(CameraDistance));
		if (rightMouseButtonPressed)
		{
			AdditionalElevation = MBMath.ClampFloat(AdditionalElevation + mouseMoveY * 0.0015f, num2, num3);
		}
		if (verticalCameraInput != 0f)
		{
			AdditionalElevation = MBMath.ClampFloat(AdditionalElevation - verticalCameraInput * dt, num2, num3);
		}
	}

	public virtual void HandleLeftMouseButtonClick(bool isMouseActive)
	{
		if (isMouseActive && !Hero.MainHero.IsPrisoner)
		{
			CurrentCameraFollowMode = CameraFollowMode.FollowParty;
			Campaign.Current.CameraFollowParty = PartyBase.MainParty;
		}
	}

	public virtual void OnSetMapSiegeOverlayState(bool isActive, bool isMapSiegeOverlayViewNull)
	{
		if (isActive && isMapSiegeOverlayViewNull && PlayerSiege.PlayerSiegeEvent != null)
		{
			TargetCameraDistance = 13f;
		}
	}

	public virtual void OnRefreshMapSiegeOverlayRequired(bool isMapSiegeOverlayViewNull)
	{
		if (PlayerSiege.PlayerSiegeEvent != null && isMapSiegeOverlayViewNull)
		{
			TargetCameraDistance = 13f;
		}
	}

	public virtual void OnBeforeTick(in InputInformation inputInformation)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Min(1f, MathF.Max(0f, 1f - CameraFrame.rotation.f.z)) + 0.15f;
		_mapScene.SetDepthOfFieldParameters(0.05f, num * 1000f, true);
		_mapScene.SetDepthOfFieldFocus(0.05f);
		MobileParty mainParty = MobileParty.MainParty;
		if (inputInformation.IsMainPartyValid && CameraAnimationInProgress)
		{
			Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
			if (_cameraAnimationStopDuration > 0f)
			{
				if (((CampaignVec2)(ref _cameraAnimationTarget)).DistanceSquared(((Vec3)(ref _cameraTarget)).AsVec2) < 0.0001f)
				{
					_cameraAnimationStopDuration = MathF.Max(_cameraAnimationStopDuration - inputInformation.Dt, 0f);
				}
				else
				{
					IdealCameraTarget = ((CampaignVec2)(ref _cameraAnimationTarget)).AsVec3() + Vec3.Up;
				}
			}
			else
			{
				CampaignVec2 position = MobileParty.MainParty.Position;
				if (((CampaignVec2)(ref position)).DistanceSquared(((Vec3)(ref _cameraTarget)).AsVec2) < 0.0001f)
				{
					CameraAnimationInProgress = false;
					Campaign.Current.SetTimeControlModeLock(false);
				}
				else
				{
					position = MobileParty.MainParty.Position;
					IdealCameraTarget = ((CampaignVec2)(ref position)).AsVec3() + Vec3.Up;
				}
			}
		}
		bool flag = CameraAnimationInProgress;
		if (ProcessCameraInput && !CameraAnimationInProgress && inputInformation.IsMapReady)
		{
			flag = GetMapCameraInput(inputInformation);
		}
		if (flag)
		{
			Vec3 val = IdealCameraTarget - _cameraTarget;
			Vec3 val2 = 10f * val * inputInformation.Dt;
			float num2 = MathF.Sqrt(MathF.Max(CameraDistance, 20f)) * 0.15f;
			float num3 = (_doFastCameraMovementToTarget ? (num2 * 5f) : num2);
			if (((Vec3)(ref val2)).LengthSquared > num3 * num3)
			{
				val2 = ((Vec3)(ref val2)).NormalizedCopy() * num3;
			}
			if (((Vec3)(ref val2)).LengthSquared < num2 * num2)
			{
				_doFastCameraMovementToTarget = false;
			}
			if (_distanceToIdealCameraTargetToStopCameraSoundEventsSquared > ((Vec3)(ref val)).LengthSquared)
			{
				StopCameraMovementSoundEvents();
			}
			_cameraTarget += val2;
		}
		else
		{
			_cameraTarget = IdealCameraTarget;
			_doFastCameraMovementToTarget = false;
			StopCameraMovementSoundEvents();
		}
		if (inputInformation.IsMainPartyValid)
		{
			if (inputInformation.CameraFollowModeKeyPressed)
			{
				CurrentCameraFollowMode = CameraFollowMode.FollowParty;
			}
			if (!inputInformation.IsInMenu && !inputInformation.MiddleMouseButtonDown && (MobileParty.MainParty == null || MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty) && (inputInformation.PartyMoveRightKey || inputInformation.PartyMoveLeftKey || inputInformation.PartyMoveUpKey || inputInformation.PartyMoveDownKey))
			{
				float num4 = 0f;
				float num5 = 0f;
				float num6 = default(float);
				float num7 = default(float);
				MathF.SinCos(CameraBearing, ref num6, ref num7);
				float num8 = default(float);
				float num9 = default(float);
				MathF.SinCos(CameraBearing + MathF.PI / 2f, ref num8, ref num9);
				float num10 = 0.5f;
				if (inputInformation.PartyMoveUpKey)
				{
					num5 += num7 * num10;
					num4 += num6 * num10;
					mainParty.ForceAiNoPathMode = true;
				}
				if (inputInformation.PartyMoveDownKey)
				{
					num5 -= num7 * num10;
					num4 -= num6 * num10;
					mainParty.ForceAiNoPathMode = true;
				}
				if (inputInformation.PartyMoveLeftKey)
				{
					num5 -= num9 * num10;
					num4 -= num8 * num10;
					mainParty.ForceAiNoPathMode = true;
				}
				if (inputInformation.PartyMoveRightKey)
				{
					num5 += num9 * num10;
					num4 += num8 * num10;
					mainParty.ForceAiNoPathMode = true;
				}
				CurrentCameraFollowMode = CameraFollowMode.FollowParty;
				CampaignVec2 val3 = mainParty.Position + new Vec2(num4, num5);
				NavigationType val4 = default(NavigationType);
				if (NavigationHelper.CanPlayerNavigateToPosition(val3, ref val4))
				{
					mainParty.SetMoveGoToPoint(val3, mainParty.NavigationCapability);
					Campaign.Current.TimeControlMode = (CampaignTimeControlMode)3;
				}
			}
			else if (mainParty.ForceAiNoPathMode)
			{
				mainParty.SetMoveGoToPoint(mainParty.Position, mainParty.NavigationCapability);
			}
		}
		UpdateMapCamera(inputInformation.LeftButtonDraggingMode, inputInformation.ClickedPosition);
	}

	protected virtual void UpdateMapCamera(bool _leftButtonDraggingMode, Vec3 _clickedPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_070b: Unknown result type (might be due to invalid IL or missing references)
		//IL_070d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0718: Unknown result type (might be due to invalid IL or missing references)
		//IL_0727: Unknown result type (might be due to invalid IL or missing references)
		//IL_072c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0738: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_0747: Unknown result type (might be due to invalid IL or missing references)
		//IL_074c: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0772: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0787: Unknown result type (might be due to invalid IL or missing references)
		//IL_0789: Unknown result type (might be due to invalid IL or missing references)
		//IL_078e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Unknown result type (might be due to invalid IL or missing references)
		//IL_079d: Expected I4, but got Unknown
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057a: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = IdealCameraTarget;
		_lastUsedIdealCameraTarget = new CampaignVec2(((Vec3)(ref val)).AsVec2, true);
		MatrixFrame cameraFrame = ComputeMapCamera(ref _cameraTarget, CameraBearing, _cameraElevation, CameraDistance, ref _lastUsedIdealCameraTarget);
		bool flag = !((Vec3)(ref cameraFrame.origin)).NearlyEquals(ref _cameraFrame.origin, 1E-05f);
		bool flag2 = !((Mat3)(ref cameraFrame.rotation)).NearlyEquals(ref _cameraFrame.rotation, 1E-05f);
		if (flag2 || flag)
		{
			Game.Current.EventManager.TriggerEvent<MapScreen.MainMapCameraMoveEvent>(new MapScreen.MainMapCameraMoveEvent(flag2, flag));
		}
		bool isCurrentlyAtSea = MobileParty.MainParty.IsCurrentlyAtSea;
		_cameraFrame = cameraFrame;
		float num = 0f;
		IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
		CampaignVec2 val2 = new CampaignVec2(((Vec3)(ref _cameraFrame.origin)).AsVec2, !isCurrentlyAtSea);
		mapSceneWrapper.GetHeightAtPoint(ref val2, ref num);
		num += 0.5f;
		if (_cameraFrame.origin.z < num)
		{
			if (_leftButtonDraggingMode)
			{
				Vec3 val3 = _clickedPosition;
				val3 -= Vec3.DotProduct(val3 - _cameraFrame.origin, _cameraFrame.rotation.s) * _cameraFrame.rotation.s;
				val = val3 - _cameraFrame.origin;
				Vec3 val4 = ((Vec3)(ref val)).NormalizedCopy();
				val = val3 - (_cameraFrame.origin + new Vec3(0f, 0f, num - _cameraFrame.origin.z, -1f));
				Vec3 val5 = Vec3.CrossProduct(val4, ((Vec3)(ref val)).NormalizedCopy());
				float num2 = ((Vec3)(ref val5)).Normalize();
				_cameraFrame.origin.z = num;
				_cameraFrame.rotation.u = ((Vec3)(ref _cameraFrame.rotation.u)).RotateAboutAnArbitraryVector(val5, num2);
				ref Mat3 rotation = ref _cameraFrame.rotation;
				val = Vec3.CrossProduct(_cameraFrame.rotation.u, _cameraFrame.rotation.s);
				rotation.f = ((Vec3)(ref val)).NormalizedCopy();
				_cameraFrame.rotation.s = Vec3.CrossProduct(_cameraFrame.rotation.f, _cameraFrame.rotation.u);
				Vec3 val6 = -Vec3.Up;
				Vec3 val7 = -_cameraFrame.rotation.u;
				Vec3 idealCameraTarget = IdealCameraTarget;
				float num3 = default(float);
				if (MBMath.GetRayPlaneIntersectionPoint(ref val6, ref idealCameraTarget, ref _cameraFrame.origin, ref val7, ref num3))
				{
					IdealCameraTarget = _cameraFrame.origin + val7 * num3;
					_cameraTarget = IdealCameraTarget;
				}
				Vec2 val8 = ((Vec3)(ref _cameraFrame.rotation.f)).AsVec2;
				val8 = new Vec2(((Vec2)(ref val8)).Length, _cameraFrame.rotation.f.z);
				_cameraElevation = 0f - ((Vec2)(ref val8)).RotationInRadians;
				val = _cameraFrame.origin - IdealCameraTarget;
				CameraDistance = ((Vec3)(ref val)).Length - 2f;
				TargetCameraDistance = CameraDistance;
				AdditionalElevation = _cameraElevation - CalculateCameraElevation(CameraDistance);
				val = IdealCameraTarget;
				_lastUsedIdealCameraTarget = new CampaignVec2(((Vec3)(ref val)).AsVec2, true);
				ComputeMapCamera(ref _cameraTarget, CameraBearing, _cameraElevation, CameraDistance, ref _lastUsedIdealCameraTarget);
			}
			else
			{
				float num4 = 0.47123894f;
				int num5 = 0;
				do
				{
					_cameraElevation += ((_cameraFrame.origin.z < num) ? num4 : (0f - num4));
					float num6 = (700f - MathF.Min(700f, MathF.Max(50f, CameraDistance))) * -1f * 0.00042857145f;
					float num7 = MathF.Max(num6 + 1E-05f, MathF.PI * 99f / 200f - CalculateCameraElevation(CameraDistance));
					AdditionalElevation = _cameraElevation - CalculateCameraElevation(CameraDistance);
					AdditionalElevation = MBMath.ClampFloat(AdditionalElevation, num6, num7);
					_cameraElevation = AdditionalElevation + CalculateCameraElevation(CameraDistance);
					CampaignVec2 lastUsedIdealCameraTarget = CampaignVec2.Zero;
					_cameraFrame = ComputeMapCamera(ref _cameraTarget, CameraBearing, _cameraElevation, CameraDistance, ref lastUsedIdealCameraTarget);
					IMapScene mapSceneWrapper2 = Campaign.Current.MapSceneWrapper;
					val2 = new CampaignVec2(((Vec3)(ref _cameraFrame.origin)).AsVec2, !isCurrentlyAtSea);
					mapSceneWrapper2.GetHeightAtPoint(ref val2, ref num);
					num += 0.5f;
					if (num4 > 0.0001f)
					{
						num4 *= 0.5f;
					}
					else
					{
						num5++;
					}
				}
				while (num4 > 0.0001f || (_cameraFrame.origin.z < num && num5 < 5));
				if (_cameraFrame.origin.z < num)
				{
					_cameraFrame.origin.z = num;
					Vec3 val9 = -Vec3.Up;
					Vec3 val10 = -_cameraFrame.rotation.u;
					Vec3 idealCameraTarget2 = IdealCameraTarget;
					float num8 = default(float);
					if (MBMath.GetRayPlaneIntersectionPoint(ref val9, ref idealCameraTarget2, ref _cameraFrame.origin, ref val10, ref num8) && CurrentCameraFollowMode != CameraFollowMode.MoveToPosition)
					{
						IdealCameraTarget = _cameraFrame.origin + val10 * num8;
						_cameraTarget = IdealCameraTarget;
						val = _cameraFrame.origin - IdealCameraTarget;
						CameraDistance = ((Vec3)(ref val)).Length - 2f;
					}
					val = IdealCameraTarget;
					_lastUsedIdealCameraTarget = new CampaignVec2(((Vec3)(ref val)).AsVec2, true);
					ComputeMapCamera(ref _cameraTarget, CameraBearing, _cameraElevation, CameraDistance, ref _lastUsedIdealCameraTarget);
					TargetCameraDistance = MathF.Max(TargetCameraDistance, CameraDistance);
				}
			}
		}
		Camera.Frame = _cameraFrame;
		Camera.SetFovVertical(0.6981317f, Screen.AspectRatio, 0.01f, MaximumCameraHeight * 4f);
		_mapScene.SetDepthOfFieldFocus(0f);
		_mapScene.SetDepthOfFieldParameters(0f, 0f, false);
		MatrixFrame identity = MatrixFrame.Identity;
		identity.rotation = _cameraFrame.rotation;
		identity.origin = _cameraTarget;
		IMapScene mapSceneWrapper3 = Campaign.Current.MapSceneWrapper;
		val2 = new CampaignVec2(((Vec3)(ref identity.origin)).AsVec2, true);
		mapSceneWrapper3.GetHeightAtPoint(ref val2, ref identity.origin.z);
		identity.origin = MBMath.Lerp(identity.origin, _cameraFrame.origin, 0.075f, 1E-05f);
		val2 = new CampaignVec2(((Vec3)(ref identity.origin)).AsVec2, true);
		PathFaceRecord face = ((CampaignVec2)(ref val2)).Face;
		if (!((PathFaceRecord)(ref face)).IsValid())
		{
			val2 = new CampaignVec2(((Vec3)(ref identity.origin)).AsVec2, false);
			face = ((CampaignVec2)(ref val2)).Face;
		}
		if (((PathFaceRecord)(ref face)).IsValid())
		{
			TerrainType faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(face);
			MBMapScene.TickAmbientSounds(_mapScene, (int)faceTerrainType);
		}
		SoundManager.SetListenerFrame(identity);
	}

	protected virtual Vec3 GetCameraTargetForPosition(CampaignVec2 targetPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return ((CampaignVec2)(ref targetPosition)).AsVec3() + Vec3.Up;
	}

	protected virtual Vec3 GetCameraTargetForParty(PartyBase party)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 zero = CampaignVec2.Zero;
		if (party.IsMobile && party.MobileParty.CurrentSettlement != null)
		{
			zero = party.MobileParty.CurrentSettlement.Position;
		}
		else if (party.IsMobile && party.MobileParty.BesiegedSettlement != null)
		{
			if (PlayerSiege.PlayerSiegeEvent != null)
			{
				MatrixFrame val = party.MobileParty.BesiegedSettlement.Town.BesiegerCampPositions1.First();
				Vec2 asVec = ((Vec3)(ref val.origin)).AsVec2;
				CampaignVec2 targetPosition = party.MobileParty.TargetPosition;
				Vec2 val2 = Vec2.Lerp(((CampaignVec2)(ref targetPosition)).ToVec2(), asVec, 0.75f);
				zero = new CampaignVec2(val2, zero.IsOnLand);
			}
			else
			{
				zero = party.MobileParty.TargetPosition;
			}
		}
		else
		{
			zero = party.Position;
		}
		return GetCameraTargetForPosition(zero);
	}

	protected virtual bool GetMapCameraInput(InputInformation inputInformation)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0759: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0797: Unknown result type (might be due to invalid IL or missing references)
		//IL_074e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0753: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_072e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0733: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0700: Unknown result type (might be due to invalid IL or missing references)
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool result = !inputInformation.LeftButtonDraggingMode;
		if (inputInformation.IsControlDown && inputInformation.CheatModeEnabled)
		{
			flag = true;
			if (inputInformation.DeltaMouseScroll > 0.01f)
			{
				CameraFastMoveMultiplier *= 1.25f;
			}
			else if (inputInformation.DeltaMouseScroll < -0.01f)
			{
				CameraFastMoveMultiplier *= 0.8f;
			}
			CameraFastMoveMultiplier = MBMath.ClampFloat(CameraFastMoveMultiplier, 1f, 37.252903f);
		}
		Vec2 val = Vec2.Zero;
		Vec3 val2;
		if (!inputInformation.LeftMouseButtonPressed && inputInformation.LeftMouseButtonDown && !inputInformation.LeftMouseButtonReleased && ((Vec2)(ref inputInformation.MousePositionPixel)).DistanceSquared(inputInformation.ClickedPositionPixel) > 300f && !inputInformation.IsInMenu)
		{
			if (!inputInformation.LeftButtonDraggingMode)
			{
				IdealCameraTarget = _cameraTarget;
				val2 = IdealCameraTarget;
				_lastUsedIdealCameraTarget = new CampaignVec2(((Vec3)(ref val2)).AsVec2, true);
			}
			val2 = inputInformation.WorldMouseFar - inputInformation.WorldMouseNear;
			Vec3 val3 = ((Vec3)(ref val2)).NormalizedCopy();
			Vec3 val4 = -Vec3.Up;
			float num = default(float);
			if (MBMath.GetRayPlaneIntersectionPoint(ref val4, ref inputInformation.ClickedPosition, ref inputInformation.WorldMouseNear, ref val3, ref num))
			{
				CurrentCameraFollowMode = CameraFollowMode.Free;
				Vec3 val5 = inputInformation.WorldMouseNear + val3 * num;
				val = ((Vec3)(ref inputInformation.ClickedPosition)).AsVec2 - ((Vec3)(ref val5)).AsVec2;
			}
		}
		if (inputInformation.MiddleMouseButtonDown)
		{
			TargetCameraDistance += 0.01f * (CameraDistance + 20f) * inputInformation.MouseSensitivity * inputInformation.MouseMoveY;
		}
		if (inputInformation.RotateLeftKeyDown)
		{
			CameraBearingVelocity = inputInformation.Dt * 2f;
		}
		else if (inputInformation.RotateRightKeyDown)
		{
			CameraBearingVelocity = inputInformation.Dt * -2f;
		}
		CameraBearingVelocity += inputInformation.HorizontalCameraInput * 1.75f * inputInformation.Dt;
		if (inputInformation.RightMouseButtonDown)
		{
			CameraBearingVelocity += 0.01f * inputInformation.MouseSensitivity * inputInformation.MouseMoveX;
		}
		float num2 = 0.1f;
		if (!inputInformation.IsMouseActive)
		{
			num2 *= inputInformation.Dt * 10f;
		}
		if (!flag)
		{
			TargetCameraDistance -= inputInformation.MapZoomIn * num2 * (CameraDistance + 20f);
			TargetCameraDistance += inputInformation.MapZoomOut * num2 * (CameraDistance + 20f);
		}
		PartyBase cameraFollowParty = Campaign.Current.CameraFollowParty;
		TargetCameraDistance = MBMath.ClampFloat(TargetCameraDistance, 2.5f, (cameraFollowParty != null && cameraFollowParty.IsMobile && (cameraFollowParty.MobileParty.BesiegedSettlement != null || (cameraFollowParty.MobileParty.CurrentSettlement != null && cameraFollowParty.MobileParty.CurrentSettlement.IsUnderSiege))) ? 30f : MaximumCameraHeight);
		float num3 = TargetCameraDistance - CameraDistance;
		float num4 = MathF.Abs(num3);
		float cameraDistance = ((num4 > 0.001f) ? (CameraDistance + num3 * inputInformation.Dt * 8f) : TargetCameraDistance);
		if (CurrentCameraFollowMode == CameraFollowMode.Free && !inputInformation.RightMouseButtonDown && !inputInformation.LeftMouseButtonDown && num4 >= 0.001f)
		{
			val2 = inputInformation.WorldMouseFar - CameraFrame.origin;
			if (((Vec3)(ref val2)).NormalizedCopy().z < -0.2f && inputInformation.RayCastForClosestEntityOrTerrainCondition)
			{
				MatrixFrame val6 = ComputeMapCamera(ref _cameraTarget, CameraBearing + CameraBearingVelocity, MathF.Min(CalculateCameraElevation(cameraDistance) + AdditionalElevation, MathF.PI * 99f / 200f), cameraDistance, ref _lastUsedIdealCameraTarget);
				Vec3 val7 = -Vec3.Up;
				val2 = inputInformation.WorldMouseFar - CameraFrame.origin;
				Vec3 val8 = ((Vec3)(ref val2)).NormalizedCopy();
				ref Mat3 rotation = ref val6.rotation;
				MatrixFrame cameraFrame = CameraFrame;
				val2 = ((Mat3)(ref cameraFrame.rotation)).TransformToLocal(ref val8);
				Vec3 val9 = ((Mat3)(ref rotation)).TransformToParent(ref val2);
				float num5 = default(float);
				if (MBMath.GetRayPlaneIntersectionPoint(ref val7, ref inputInformation.ProjectedPosition, ref val6.origin, ref val9, ref num5))
				{
					Vec2 asVec = ((Vec3)(ref inputInformation.ProjectedPosition)).AsVec2;
					val2 = val6.origin + val9 * num5;
					val = asVec - ((Vec3)(ref val2)).AsVec2;
					result = false;
				}
			}
		}
		if (inputInformation.RX != 0f || inputInformation.RY != 0f || ((Vec2)(ref val)).IsNonZero())
		{
			float num6 = 0.001f * (CameraDistance * 0.55f + 15f);
			Vec2 val10 = Vec2.FromRotation(0f - CameraBearing);
			val2 = IdealCameraTarget;
			Vec2 val11 = ((Vec3)(ref val2)).AsVec2 - ((CampaignVec2)(ref _lastUsedIdealCameraTarget)).ToVec2();
			if (((Vec2)(ref val11)).LengthSquared > 0.010000001f)
			{
				IdealCameraTarget = ((CampaignVec2)(ref _lastUsedIdealCameraTarget)).AsVec3();
				_cameraTarget = IdealCameraTarget;
			}
			if (!((Vec2)(ref val)).IsNonZero())
			{
				IdealCameraTarget = _cameraTarget;
			}
			Vec2 val12 = inputInformation.Dt * 500f * inputInformation.RX * ((Vec2)(ref val10)).RightVec() * num6 + inputInformation.Dt * 500f * inputInformation.RY * val10 * num6;
			IdealCameraTarget = new Vec3(IdealCameraTarget.x + val.x + val12.x, IdealCameraTarget.y + val.y + val12.y, IdealCameraTarget.z, -1f);
			if (((Vec2)(ref val)).IsNonZero())
			{
				_cameraTarget = IdealCameraTarget;
			}
			ref Vec3 cameraTarget = ref _cameraTarget;
			((Vec3)(ref cameraTarget)).AsVec2 = ((Vec3)(ref cameraTarget)).AsVec2 + val12;
			if (inputInformation.RX != 0f || inputInformation.RY != 0f)
			{
				CurrentCameraFollowMode = CameraFollowMode.Free;
			}
		}
		CameraBearing += CameraBearingVelocity;
		CameraBearingVelocity = 0f;
		CameraDistance = cameraDistance;
		_cameraElevation = MathF.Min(CalculateCameraElevation(cameraDistance) + AdditionalElevation, MathF.PI * 99f / 200f);
		if (CurrentCameraFollowMode == CameraFollowMode.FollowParty && cameraFollowParty != null && cameraFollowParty.IsValid)
		{
			CampaignVec2 val13 = default(CampaignVec2);
			((CampaignVec2)(ref val13))._002Ector(Vec2.Zero, cameraFollowParty.IsMobile && cameraFollowParty.MobileParty.IsCurrentlyAtSea);
			if (cameraFollowParty.IsMobile)
			{
				Settlement val14 = cameraFollowParty.MobileParty.CurrentSettlement ?? cameraFollowParty.MobileParty.BesiegedSettlement;
				if (val14 != null)
				{
					if (val14.SiegeEvent != null)
					{
						if (val14.HasPort && val14.SiegeEvent.IsBlockadeActive)
						{
							CampaignVec2 val15 = val14.PortPosition;
							Vec2 val16 = ((CampaignVec2)(ref val15)).ToVec2() * 0.25f;
							val15 = val14.GatePosition;
							((CampaignVec2)(ref val13))._002Ector(val16 + ((CampaignVec2)(ref val15)).ToVec2() * 0.75f, true);
						}
						else
						{
							val13 = val14.GatePosition;
						}
					}
					else
					{
						val13 = cameraFollowParty.MobileParty.CurrentSettlement.Position;
					}
				}
				else
				{
					val13 = cameraFollowParty.Position;
				}
			}
			else
			{
				val13 = cameraFollowParty.Position;
			}
			float num7 = 0f;
			Campaign.Current.MapSceneWrapper.GetHeightAtPoint(ref val13, ref num7);
			IdealCameraTarget = new Vec3(((CampaignVec2)(ref val13)).X, ((CampaignVec2)(ref val13)).Y, num7 + 1f, -1f);
		}
		return result;
	}

	protected virtual MatrixFrame ComputeMapCamera(ref Vec3 cameraTarget, float cameraBearing, float cameraElevation, float cameraDistance, ref CampaignVec2 lastUsedIdealCameraTarget)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		Vec2 asVec = ((Vec3)(ref cameraTarget)).AsVec2;
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = cameraTarget;
		((Mat3)(ref identity.rotation)).RotateAboutSide(MathF.PI / 2f);
		((Mat3)(ref identity.rotation)).RotateAboutForward(0f - cameraBearing);
		((Mat3)(ref identity.rotation)).RotateAboutSide(0f - cameraElevation);
		ref Vec3 origin = ref identity.origin;
		origin += identity.rotation.u * (cameraDistance + 2f);
		Vec2 val = (Campaign.MapMinimumPosition + Campaign.MapMaximumPosition) * 0.5f;
		float num = Campaign.MapMaximumPosition.y - val.y;
		float num2 = Campaign.MapMaximumPosition.x - val.x;
		asVec.x = MBMath.ClampFloat(asVec.x, val.x - num2, val.x + num2);
		asVec.y = MBMath.ClampFloat(asVec.y, val.y - num, val.y + num);
		float num3 = MBMath.ClampFloat(((CampaignVec2)(ref lastUsedIdealCameraTarget)).X, val.x - num2, val.x + num2);
		float num4 = MBMath.ClampFloat(((CampaignVec2)(ref lastUsedIdealCameraTarget)).Y, val.y - num, val.y + num);
		lastUsedIdealCameraTarget = new CampaignVec2(new Vec2(num3, num4), lastUsedIdealCameraTarget.IsOnLand);
		identity.origin.x += asVec.x - cameraTarget.x;
		identity.origin.y += asVec.y - cameraTarget.y;
		return identity;
	}

	protected virtual float CalculateCameraElevation(float cameraDistance)
	{
		return cameraDistance * 0.0075f + 0.35f;
	}
}
