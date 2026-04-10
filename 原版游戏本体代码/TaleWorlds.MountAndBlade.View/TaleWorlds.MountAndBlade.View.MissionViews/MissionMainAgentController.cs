using System;
using System.Collections.Generic;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

[DefaultView]
public class MissionMainAgentController : MissionView
{
	public enum OverrideMainAgentControlFlag
	{
		None = 0,
		Walk = 1,
		Run = 2,
		Crouch = 4,
		Stand = 8
	}

	public delegate void OnLockedAgentChangedDelegate(Agent newAgent);

	public delegate void OnPotentialLockedAgentChangedDelegate(Agent newPotentialAgent);

	private const float MinValueForAimStart = 0.2f;

	private const float MaxValueForAttackEnd = 0.6f;

	private float _lastForwardKeyPressTime;

	private float _lastBackwardKeyPressTime;

	private float _lastLeftKeyPressTime;

	private float _lastRightKeyPressTime;

	private float _lastWieldNextPrimaryWeaponTriggerTime;

	private float _lastWieldNextOffhandWeaponTriggerTime;

	private bool _activated = true;

	private bool _strafeModeActive;

	private bool _autoDismountModeActive;

	private bool _isPlayerAgentAdded;

	private bool _isPlayerAiming;

	private bool _isPlayerOrderOpen;

	private bool _isTargetLockEnabled;

	private MovementControlFlag _lastMovementKeyPressed = (MovementControlFlag)1;

	private Agent _lockedAgent;

	private Agent _potentialLockTargetAgent;

	private OverrideMainAgentControlFlag _overrideControlsThisFrame;

	private float _lastLockKeyPressTime;

	private float _lastLockedAgentHeightDifference;

	public readonly MissionMainAgentInteractionComponent InteractionComponent;

	public bool IsChatOpen;

	private bool _weaponUsageToggleRequested;

	public bool IsDisabled { get; set; }

	public Vec3 CustomLookDir { get; set; }

	public bool IsPlayerAiming
	{
		get
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			if (_isPlayerAiming)
			{
				return true;
			}
			if (((MissionBehavior)this).Mission.MainAgent == null)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (base.Input != null)
			{
				flag2 = base.Input.IsGameKeyDown(9);
			}
			if (((MissionBehavior)this).Mission.MainAgent != null)
			{
				MissionWeapon wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
				if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem != null)
				{
					wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
					int num;
					if (!((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsRangedWeapon)
					{
						wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
						num = (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsAmmo ? 1 : 0);
					}
					else
					{
						num = 1;
					}
					flag = (byte)num != 0;
				}
				flag3 = Extensions.HasAnyFlag<MovementControlFlag>(((MissionBehavior)this).Mission.MainAgent.MovementFlags, (MovementControlFlag)960);
			}
			return flag && flag2 && flag3;
		}
	}

	public Agent LockedAgent
	{
		get
		{
			return _lockedAgent;
		}
		private set
		{
			if (_lockedAgent != value)
			{
				_lockedAgent = value;
				this.OnLockedAgentChanged?.Invoke(value);
			}
		}
	}

	public Agent PotentialLockTargetAgent
	{
		get
		{
			return _potentialLockTargetAgent;
		}
		private set
		{
			if (_potentialLockTargetAgent != value)
			{
				_potentialLockTargetAgent = value;
				this.OnPotentialLockedAgentChanged?.Invoke(value);
			}
		}
	}

	public event OnLockedAgentChangedDelegate OnLockedAgentChanged;

	public event OnPotentialLockedAgentChangedDelegate OnPotentialLockedAgentChanged;

	public MissionMainAgentController()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		InteractionComponent = new MissionMainAgentInteractionComponent(this);
		CustomLookDir = Vec3.Zero;
		IsChatOpen = false;
	}

	public override void EarlyStart()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		((MissionBehavior)this).EarlyStart();
		Game.Current.EventManager.RegisterEvent<MissionPlayerToggledOrderViewEvent>((Action<MissionPlayerToggledOrderViewEvent>)OnPlayerToggleOrder);
		((MissionBehavior)this).Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(Mission_OnMainAgentChanged);
		MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		if (((missionBehavior != null) ? missionBehavior.RoundComponent : null) != null)
		{
			missionBehavior.RoundComponent.OnRoundStarted += Disable;
			missionBehavior.RoundComponent.OnPreparationEnded += Enable;
		}
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		UpdateLockTargetOption();
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		((MissionBehavior)this).Mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(Mission_OnMainAgentChanged);
		Game.Current.EventManager.UnregisterEvent<MissionPlayerToggledOrderViewEvent>((Action<MissionPlayerToggledOrderViewEvent>)OnPlayerToggleOrder);
		MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		if (((missionBehavior != null) ? missionBehavior.RoundComponent : null) != null)
		{
			missionBehavior.RoundComponent.OnRoundStarted -= Disable;
			missionBehavior.RoundComponent.OnPreparationEnded -= Enable;
		}
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	public override bool IsReady()
	{
		bool result = true;
		if (((MissionBehavior)this).Mission.MainAgent != null)
		{
			result = ((MissionBehavior)this).Mission.MainAgent.AgentVisuals.CheckResources(true);
		}
		return result;
	}

	private void Mission_OnMainAgentChanged(Agent oldAgent)
	{
		if (((MissionBehavior)this).Mission.MainAgent != null)
		{
			_isPlayerAgentAdded = true;
			_strafeModeActive = false;
			_autoDismountModeActive = false;
		}
	}

	public override void OnPreMissionTick(float dt)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnPreMissionTick(dt);
		if (base.MissionScreen == null)
		{
			return;
		}
		if (((MissionBehavior)this).Mission.MainAgent == null && GameNetwork.MyPeer != null)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			if (component != null)
			{
				if (component.HasSpawnedAgentVisuals)
				{
					AgentVisualsMovementCheck();
				}
				else if (component.FollowedAgent != null)
				{
					RequestToSpawnAsBotCheck();
				}
			}
		}
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if (mainAgent != null && (int)mainAgent.State == 1 && !base.MissionScreen.IsCheatGhostMode && !((MissionBehavior)this).Mission.MainAgent.IsAIControlled && !base.MissionScreen.IsPhotoModeEnabled && !IsDisabled && _activated)
		{
			InteractionComponent.FocusTick();
			InteractionComponent.FocusedItemHealthTick();
			ControlTick();
			InteractionComponent.FocusStateCheckTick();
			LookTick(dt);
		}
		else
		{
			InteractionComponent.ClearFocus();
			LockedAgent = null;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if ((object)InteractionComponent.CurrentFocusedObject == affectedAgent || affectedAgent == ((MissionBehavior)this).Mission.MainAgent)
		{
			InteractionComponent.ClearFocus();
		}
	}

	public override void OnAgentDeleted(Agent affectedAgent)
	{
		if ((object)InteractionComponent.CurrentFocusedObject == affectedAgent)
		{
			InteractionComponent.ClearFocus();
		}
	}

	public override void OnClearScene()
	{
		InteractionComponent.OnClearScene();
	}

	private void LookTick(float dt)
	{
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Invalid comparison between Unknown and I4
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		if (IsDisabled)
		{
			return;
		}
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if (mainAgent == null)
		{
			return;
		}
		if (_isPlayerAgentAdded)
		{
			_isPlayerAgentAdded = false;
			mainAgent.LookDirectionAsAngle = mainAgent.MovementDirectionAsAngle;
		}
		if (!(((MissionBehavior)this).Mission.ClearSceneTimerElapsedTime >= 0f))
		{
			return;
		}
		Vec3 val4;
		Vec3 lookDirection;
		if (LockedAgent != null)
		{
			float num = 0f;
			float agentScale = LockedAgent.AgentScale;
			float agentScale2 = mainAgent.AgentScale;
			num = ((!Extensions.HasAnyFlag<AgentFlag>(LockedAgent.GetAgentFlags(), (AgentFlag)2048)) ? (num + LockedAgent.Monster.BodyCapsulePoint1.z * agentScale) : (LockedAgent.HasMount ? (num + ((LockedAgent.MountAgent.Monster.RiderCameraHeightAdder + LockedAgent.MountAgent.Monster.BodyCapsulePoint1.z + LockedAgent.MountAgent.Monster.BodyCapsuleRadius) * LockedAgent.MountAgent.AgentScale + LockedAgent.Monster.CrouchEyeHeight * agentScale)) : ((!LockedAgent.CrouchMode && !LockedAgent.IsSitting()) ? (num + (LockedAgent.Monster.StandingEyeHeight + 0.2f) * agentScale) : (num + (LockedAgent.Monster.CrouchEyeHeight + 0.2f) * agentScale))));
			num = ((!Extensions.HasAnyFlag<AgentFlag>(mainAgent.GetAgentFlags(), (AgentFlag)2048)) ? (num - LockedAgent.Monster.BodyCapsulePoint1.z * agentScale2) : (mainAgent.HasMount ? (num - ((mainAgent.MountAgent.Monster.RiderCameraHeightAdder + mainAgent.MountAgent.Monster.BodyCapsulePoint1.z + mainAgent.MountAgent.Monster.BodyCapsuleRadius) * mainAgent.MountAgent.AgentScale + mainAgent.Monster.CrouchEyeHeight * agentScale2)) : ((!mainAgent.CrouchMode && !mainAgent.IsSitting()) ? (num - (mainAgent.Monster.StandingEyeHeight + 0.2f) * agentScale2) : (num - (mainAgent.Monster.CrouchEyeHeight + 0.2f) * agentScale2))));
			if (Extensions.HasAnyFlag<AgentFlag>(LockedAgent.GetAgentFlags(), (AgentFlag)2048))
			{
				num -= 0.3f * agentScale;
			}
			num = (_lastLockedAgentHeightDifference = MBMath.Lerp(_lastLockedAgentHeightDifference, num, MathF.Min(8f * dt, 1f), 1E-05f));
			Vec3 visualPosition = LockedAgent.VisualPosition;
			Vec3 val;
			Vec2 movementDirection;
			if (LockedAgent.MountAgent == null)
			{
				val = Vec3.Zero;
			}
			else
			{
				movementDirection = LockedAgent.MountAgent.GetMovementDirection();
				val = ((Vec2)(ref movementDirection)).ToVec3(0f) * LockedAgent.MountAgent.Monster.RiderBodyCapsuleForwardAdder;
			}
			Vec3 val2 = visualPosition + val + new Vec3(0f, 0f, num, -1f);
			Vec3 visualPosition2 = mainAgent.VisualPosition;
			Vec3 val3;
			if (mainAgent.MountAgent == null)
			{
				val3 = Vec3.Zero;
			}
			else
			{
				movementDirection = mainAgent.MountAgent.GetMovementDirection();
				val3 = ((Vec2)(ref movementDirection)).ToVec3(0f) * mainAgent.MountAgent.Monster.RiderBodyCapsuleForwardAdder;
			}
			val4 = val2 - (visualPosition2 + val3);
			lookDirection = ((Vec3)(ref val4)).NormalizedCopy();
		}
		else
		{
			val4 = CustomLookDir;
			if (((Vec3)(ref val4)).IsNonZero)
			{
				lookDirection = CustomLookDir;
			}
			else
			{
				Mat3 identity = Mat3.Identity;
				((Mat3)(ref identity)).RotateAboutUp(base.MissionScreen.CameraBearing);
				((Mat3)(ref identity)).RotateAboutSide(base.MissionScreen.CameraElevation);
				lookDirection = identity.f;
			}
		}
		if (!base.MissionScreen.IsViewingCharacter() && !mainAgent.IsLookDirectionLocked && (int)mainAgent.MovementLockedState != 2)
		{
			mainAgent.LookDirection = lookDirection;
		}
		mainAgent.HeadCameraMode = ((MissionBehavior)this).Mission.CameraIsFirstPerson;
	}

	private void AgentVisualsMovementCheck()
	{
		if (base.Input.IsGameKeyReleased(13))
		{
			BreakAgentVisualsInvulnerability();
		}
	}

	public void BreakAgentVisualsInvulnerability()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new AgentVisualsBreakInvulnerability());
			GameNetwork.EndModuleEventAsClient();
		}
		else
		{
			Mission.Current.GetMissionBehavior<SpawnComponent>().SetEarlyAgentVisualsDespawning(PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer), true);
		}
	}

	private void RequestToSpawnAsBotCheck()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (base.Input.IsGameKeyPressed(13))
		{
			if (GameNetwork.IsClient)
			{
				GameNetwork.BeginModuleEventAsClient();
				GameNetwork.WriteMessage((GameNetworkMessage)new RequestToSpawnAsBot());
				GameNetwork.EndModuleEventAsClient();
			}
			else if (PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).HasSpawnTimerExpired)
			{
				PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).WantsToSpawnAsBot = true;
			}
		}
	}

	private Agent FindTargetedLockableAgent(Agent player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		Vec3 direction = base.MissionScreen.CombatCamera.Direction;
		Vec3 val = direction;
		Vec3 position = base.MissionScreen.CombatCamera.Position;
		Vec3 visualPosition = player.VisualPosition;
		Vec3 val2 = new Vec3(position.x, position.y, 0f, -1f);
		float num = ((Vec3)(ref val2)).Distance(new Vec3(visualPosition.x, visualPosition.y, 0f, -1f));
		Vec3 val3 = position * (1f - num) + (position + direction) * num;
		float num2 = 0f;
		Agent val4 = null;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if ((!item.IsMount || item.RiderAgent == null || !item.RiderAgent.IsEnemyOf(player)) && (item.IsMount || !item.IsEnemyOf(player)))
			{
				continue;
			}
			Vec3 val5 = item.GetChestGlobalPosition() - val3;
			float num3 = ((Vec3)(ref val5)).Normalize();
			if (!(num3 < 20f))
			{
				continue;
			}
			Vec2 asVec = ((Vec3)(ref val)).AsVec2;
			Vec2 val6 = ((Vec2)(ref asVec)).Normalized();
			asVec = ((Vec3)(ref val5)).AsVec2;
			float num4 = Vec2.DotProduct(val6, ((Vec2)(ref asVec)).Normalized());
			asVec = ((Vec3)(ref val)).AsVec2;
			Vec2 val7 = new Vec2(((Vec2)(ref asVec)).Length, val.z);
			asVec = ((Vec3)(ref val5)).AsVec2;
			float num5 = Vec2.DotProduct(val7, new Vec2(((Vec2)(ref asVec)).Length, val5.z));
			if (num4 > 0.95f && num5 > 0.95f)
			{
				float num6 = num4 * num4 * num4 / MathF.Pow(num3, 0.15f);
				if (num6 > num2)
				{
					num2 = num6;
					val4 = item;
				}
			}
		}
		if (val4 != null && val4.IsMount && val4.RiderAgent != null)
		{
			return val4.RiderAgent;
		}
		return val4;
	}

	private void ControlTick()
	{
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Invalid comparison between Unknown and I4
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Invalid comparison between Unknown and I4
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Invalid comparison between Unknown and I4
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Invalid comparison between Unknown and I4
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Invalid comparison between Unknown and I4
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0415: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_079e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_078e: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0630: Invalid comparison between Unknown and I4
		//IL_07c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0641: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_063e: Invalid comparison between Unknown and I4
		//IL_09ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a09: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_082d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0832: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0841: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Invalid comparison between Unknown and I4
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Invalid comparison between Unknown and I4
		//IL_08e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0854: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Unknown result type (might be due to invalid IL or missing references)
		//IL_0725: Unknown result type (might be due to invalid IL or missing references)
		//IL_072a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0752: Unknown result type (might be due to invalid IL or missing references)
		//IL_0757: Unknown result type (might be due to invalid IL or missing references)
		//IL_075c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0761: Unknown result type (might be due to invalid IL or missing references)
		//IL_0765: Unknown result type (might be due to invalid IL or missing references)
		//IL_076b: Invalid comparison between Unknown and I4
		//IL_0b62: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0941: Unknown result type (might be due to invalid IL or missing references)
		//IL_0946: Unknown result type (might be due to invalid IL or missing references)
		//IL_0948: Unknown result type (might be due to invalid IL or missing references)
		//IL_094b: Invalid comparison between Unknown and I4
		//IL_0870: Unknown result type (might be due to invalid IL or missing references)
		//IL_0875: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0961: Unknown result type (might be due to invalid IL or missing references)
		//IL_0964: Invalid comparison between Unknown and I4
		//IL_094f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0959: Unknown result type (might be due to invalid IL or missing references)
		//IL_0885: Unknown result type (might be due to invalid IL or missing references)
		//IL_088a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b9a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba0: Unknown result type (might be due to invalid IL or missing references)
		//IL_097a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0968: Unknown result type (might be due to invalid IL or missing references)
		//IL_0972: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0992: Unknown result type (might be due to invalid IL or missing references)
		//IL_0995: Invalid comparison between Unknown and I4
		//IL_0980: Unknown result type (might be due to invalid IL or missing references)
		//IL_098a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c05: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c0c: Invalid comparison between Unknown and I4
		//IL_0d25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0999: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c46: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d71: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d02: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d14: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0caf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0da7: Unknown result type (might be due to invalid IL or missing references)
		if ((base.MissionScreen != null && base.MissionScreen.IsPhotoModeEnabled) || IsChatOpen)
		{
			return;
		}
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		bool flag = false;
		MissionWeapon wieldedWeapon;
		SpectatorData spectatingData;
		if (LockedAgent != null)
		{
			if (LinQuick.ContainsQ<Agent>((List<Agent>)(object)((MissionBehavior)this).Mission.Agents, LockedAgent) && LockedAgent.IsActive())
			{
				Vec3 position = LockedAgent.Position;
				if (!(((Vec3)(ref position)).DistanceSquared(mainAgent.Position) > 625f) && !base.Input.IsGameKeyReleased(26) && !base.Input.IsGameKeyDown(25) && ((int)((MissionBehavior)this).Mission.Mode == 2 || (int)((MissionBehavior)this).Mission.Mode == 4))
				{
					wieldedWeapon = mainAgent.WieldedWeapon;
					if (!((MissionWeapon)(ref wieldedWeapon)).IsEmpty)
					{
						wieldedWeapon = mainAgent.WieldedWeapon;
						if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsRangedWeapon)
						{
							goto IL_0124;
						}
					}
					if (base.MissionScreen != null)
					{
						spectatingData = base.MissionScreen.GetSpectatingData(base.MissionScreen.CombatCamera.Frame.origin);
						if ((int)((SpectatorData)(ref spectatingData)).CameraType == 1 && !IsThereAnyCustomCameraAddition())
						{
							goto IL_012d;
						}
					}
				}
			}
			goto IL_0124;
		}
		goto IL_012d;
		IL_06a7:
		PotentialLockTargetAgent = null;
		goto IL_06ae;
		IL_09be:
		if (!base.MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen)
		{
			if (base.Input.IsGameKeyPressed(16) && (mainAgent.KickClear() || mainAgent.MountAgent != null))
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x8000);
			}
			if (base.Input.IsGameKeyPressed(18))
			{
				mainAgent.TryToWieldWeaponInSlot((EquipmentIndex)0, (WeaponWieldActionType)0, false);
			}
			else if (base.Input.IsGameKeyPressed(19))
			{
				mainAgent.TryToWieldWeaponInSlot((EquipmentIndex)1, (WeaponWieldActionType)0, false);
			}
			else if (base.Input.IsGameKeyPressed(20))
			{
				mainAgent.TryToWieldWeaponInSlot((EquipmentIndex)2, (WeaponWieldActionType)0, false);
			}
			else if (base.Input.IsGameKeyPressed(21))
			{
				mainAgent.TryToWieldWeaponInSlot((EquipmentIndex)3, (WeaponWieldActionType)0, false);
			}
			else if (base.Input.IsGameKeyPressed(11) && _lastWieldNextPrimaryWeaponTriggerTime + 0.2f < Time.ApplicationTime)
			{
				_lastWieldNextPrimaryWeaponTriggerTime = Time.ApplicationTime;
				mainAgent.WieldNextWeapon((HandIndex)0, (WeaponWieldActionType)0);
			}
			else if (base.Input.IsGameKeyPressed(12) && _lastWieldNextOffhandWeaponTriggerTime + 0.2f < Time.ApplicationTime)
			{
				_lastWieldNextOffhandWeaponTriggerTime = Time.ApplicationTime;
				mainAgent.WieldNextWeapon((HandIndex)1, (WeaponWieldActionType)0);
			}
			else if (base.Input.IsGameKeyPressed(23))
			{
				mainAgent.TryToSheathWeaponInHand((HandIndex)0, (WeaponWieldActionType)0);
			}
			if (base.Input.IsGameKeyPressed(17) || _weaponUsageToggleRequested)
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x400);
				_weaponUsageToggleRequested = false;
			}
			if (Extensions.HasAnyFlag<OverrideMainAgentControlFlag>(_overrideControlsThisFrame, (!mainAgent.WalkMode) ? OverrideMainAgentControlFlag.Walk : OverrideMainAgentControlFlag.Run) || base.Input.IsGameKeyPressed(30))
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | (mainAgent.WalkMode ? 4096 : 2048));
			}
			if (mainAgent.IsInWater())
			{
				if (base.Input.IsGameKeyDown(14))
				{
					mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 8);
				}
				if (base.Input.IsGameKeyDown(15))
				{
					mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x2000);
				}
			}
			if (mainAgent.MountAgent != null)
			{
				if (base.Input.IsGameKeyPressed(15) || _autoDismountModeActive)
				{
					if (mainAgent.GetCurrentVelocity().y < 0.5f && (int)mainAgent.MountAgent.GetCurrentActionType(0) != 47)
					{
						mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 1);
					}
					else if (base.Input.IsGameKeyPressed(15))
					{
						_autoDismountModeActive = true;
						mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags & -458753);
						mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x20000);
					}
				}
			}
			else if (Extensions.HasAnyFlag<OverrideMainAgentControlFlag>(_overrideControlsThisFrame, Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512) ? OverrideMainAgentControlFlag.Stand : OverrideMainAgentControlFlag.Crouch) || (!Input.IsGamepadActive && base.Input.IsGameKeyPressed(15)) || (Extensions.HasAnyFlag<EventControlFlag>(mainAgent.EventControlFlags, (EventControlFlag)8192) && !Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512)) || (Extensions.HasAnyFlag<EventControlFlag>(mainAgent.EventControlFlags, (EventControlFlag)16384) && Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512)))
			{
				if (Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512))
				{
					mainAgent.SetScriptedFlags((AIScriptedFrameFlags)(mainAgent.GetScriptedFlags() & -513));
				}
				else if (mainAgent.IsCrouchingAllowed())
				{
					mainAgent.SetScriptedFlags((AIScriptedFrameFlags)(mainAgent.GetScriptedFlags() | 0x200));
				}
			}
			if (Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512) && (Extensions.HasAnyFlag<EventControlFlag>(mainAgent.EventControlFlags, (EventControlFlag)49163) || mainAgent.HasMount || mainAgent.IsInWater()))
			{
				mainAgent.SetScriptedFlags((AIScriptedFrameFlags)(mainAgent.GetScriptedFlags() & -513));
			}
			if (mainAgent.CrouchMode != Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512))
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | (Extensions.HasAnyFlag<AIScriptedFrameFlags>(mainAgent.GetScriptedFlags(), (AIScriptedFrameFlags)512) ? 8192 : 16384));
			}
		}
		goto IL_0dad;
		IL_08fe:
		int num;
		if (num == 0 && base.Input.IsGameKeyDown(10))
		{
			if (ManagedOptions.GetConfig((ManagedOptionsType)2) == 2f && MissionGameModels.Current.AutoBlockModel != null)
			{
				UsageDirection blockDirection = MissionGameModels.Current.AutoBlockModel.GetBlockDirection(((MissionBehavior)this).Mission);
				if ((int)blockDirection == 2)
				{
					mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x800);
				}
				else if ((int)blockDirection == 3)
				{
					mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x400);
				}
				else if ((int)blockDirection == 0)
				{
					mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x1000);
				}
				else if ((int)blockDirection == 1)
				{
					mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x2000);
				}
			}
			else
			{
				mainAgent.MovementFlags |= mainAgent.GetDefendMovementFlag();
			}
		}
		goto IL_09be;
		IL_0124:
		LockedAgent = null;
		flag = true;
		goto IL_012d;
		IL_012d:
		bool flag2;
		bool flag3;
		bool flag4;
		bool flag5;
		Vec2 val = default(Vec2);
		if ((int)((MissionBehavior)this).Mission.Mode == 1)
		{
			mainAgent.MovementFlags = (MovementControlFlag)0;
			mainAgent.MovementInputVector = Vec2.Zero;
		}
		else if (((MissionBehavior)this).Mission.ClearSceneTimerElapsedTime >= 0f && (int)mainAgent.State == 1)
		{
			flag2 = false;
			flag3 = false;
			flag4 = false;
			flag5 = false;
			((Vec2)(ref val))._002Ector(base.Input.GetGameKeyAxis("MovementAxisX"), base.Input.GetGameKeyAxis("MovementAxisY"));
			if (_autoDismountModeActive)
			{
				if (!base.Input.IsGameKeyDown(0) && mainAgent.MountAgent != null)
				{
					if (mainAgent.GetCurrentVelocity().y > 0f)
					{
						val.y = -1f;
					}
				}
				else
				{
					_autoDismountModeActive = false;
				}
			}
			if (MathF.Abs(val.x) < 0.2f)
			{
				val.x = 0f;
			}
			if (MathF.Abs(val.y) < 0.2f)
			{
				val.y = 0f;
			}
			if (((Vec2)(ref val)).IsNonZero())
			{
				float rotationInRadians = ((Vec2)(ref val)).RotationInRadians;
				if (rotationInRadians > -MathF.PI / 4f && rotationInRadians < MathF.PI / 4f)
				{
					flag3 = true;
				}
				else if (rotationInRadians < MathF.PI * -3f / 4f || rotationInRadians > MathF.PI * 3f / 4f)
				{
					flag5 = true;
				}
				else if (rotationInRadians < 0f)
				{
					flag2 = true;
				}
				else
				{
					flag4 = true;
				}
			}
			mainAgent.EventControlFlags = (EventControlFlag)0;
			mainAgent.MovementFlags = (MovementControlFlag)0;
			mainAgent.MovementInputVector = Vec2.Zero;
			foreach (MissionBehavior missionBehavior in ((MissionBehavior)this).Mission.MissionBehaviors)
			{
				IPlayerInputEffector val2;
				if ((val2 = (IPlayerInputEffector)(object)((missionBehavior is IPlayerInputEffector) ? missionBehavior : null)) != null)
				{
					mainAgent.EventControlFlags |= val2.OnCollectPlayerEventControlFlags();
				}
			}
			if (!base.MissionScreen.IsRadialMenuActive && !((MissionBehavior)this).Mission.IsOrderMenuOpen)
			{
				if (base.Input.IsGameKeyPressed(14))
				{
					if (mainAgent.MountAgent != null)
					{
						Vec2 movementVelocity = mainAgent.MovementVelocity;
						if (!(((Vec2)(ref movementVelocity)).LengthSquared > 0.09f))
						{
							mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 4);
							goto IL_034a;
						}
					}
					mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 8);
				}
				goto IL_034a;
			}
			goto IL_036b;
		}
		goto IL_0dad;
		IL_077a:
		if (mainAgent.MountAgent != null && !_strafeModeActive)
		{
			if (flag2 || val.x > 0f)
			{
				mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x10);
			}
			else if (flag4 || val.x < 0f)
			{
				mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x20);
			}
		}
		mainAgent.MovementInputVector = val;
		if (!((ScreenBase)base.MissionScreen).MouseVisible && !base.MissionScreen.IsRadialMenuActive && !_isPlayerOrderOpen && mainAgent.CombatActionsEnabled)
		{
			if (NativeOptions.GetConfig((NativeOptionsType)18) != 0f && Input.IsGamepadActive)
			{
				wieldedWeapon = mainAgent.WieldedWeapon;
				WeaponComponentData currentUsageItem = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
				if (currentUsageItem != null && Extensions.HasAllFlags<WeaponFlags>(currentUsageItem.WeaponFlags, (WeaponFlags)3072))
				{
					num = 1;
					goto IL_08b1;
				}
				wieldedWeapon = mainAgent.WieldedWeapon;
				WeaponComponentData currentUsageItem2 = ((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem;
				if (currentUsageItem2 != null && currentUsageItem2.IsRangedWeapon)
				{
					wieldedWeapon = mainAgent.WieldedWeapon;
					if (!((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsConsumable)
					{
						wieldedWeapon = mainAgent.WieldedWeapon;
						num = ((!Extensions.HasAllFlags<WeaponFlags>(((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.WeaponFlags, (WeaponFlags)3072)) ? 1 : 0);
						if (num != 0)
						{
							goto IL_08b1;
						}
						goto IL_08c8;
					}
				}
				num = 0;
			}
			else
			{
				num = 0;
			}
			goto IL_08c8;
		}
		goto IL_09be;
		IL_0dad:
		_overrideControlsThisFrame = OverrideMainAgentControlFlag.None;
		return;
		IL_08c8:
		if (base.Input.IsGameKeyDown(9) && Extensions.HasAnyFlag<AgentFlag>(mainAgent.GetAgentFlags(), (AgentFlag)8))
		{
			mainAgent.MovementFlags |= mainAgent.AttackDirectionToMovementFlag(mainAgent.GetAttackDirection());
		}
		goto IL_08fe;
		IL_08b1:
		if (Extensions.HasAnyFlag<AgentFlag>(mainAgent.GetAgentFlags(), (AgentFlag)8))
		{
			HandleRangedWeaponAttackAlternativeAiming(mainAgent);
		}
		goto IL_08fe;
		IL_034a:
		if (base.Input.IsGameKeyPressed(13))
		{
			mainAgent.MovementFlags = (MovementControlFlag)(mainAgent.MovementFlags | 0x10000);
		}
		goto IL_036b;
		IL_06ae:
		if (LockedAgent == null && !flag && base.Input.IsGameKeyReleased(26) && !GameNetwork.IsMultiplayer)
		{
			_lastLockKeyPressTime = 0f;
			if (!base.Input.IsGameKeyDown(25) && ((int)((MissionBehavior)this).Mission.Mode == 2 || (int)((MissionBehavior)this).Mission.Mode == 4))
			{
				wieldedWeapon = mainAgent.WieldedWeapon;
				if (!((MissionWeapon)(ref wieldedWeapon)).IsEmpty)
				{
					wieldedWeapon = mainAgent.WieldedWeapon;
					if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsRangedWeapon)
					{
						goto IL_077a;
					}
				}
				if (base.MissionScreen != null)
				{
					spectatingData = base.MissionScreen.GetSpectatingData(base.MissionScreen.CombatCamera.Frame.origin);
					if ((int)((SpectatorData)(ref spectatingData)).CameraType == 1)
					{
						LockedAgent = FindTargetedLockableAgent(mainAgent);
					}
				}
			}
		}
		goto IL_077a;
		IL_036b:
		if (mainAgent.MountAgent != null && mainAgent.GetCurrentVelocity().y < 0.5f && (base.Input.IsGameKeyDown(3) || base.Input.IsGameKeyDown(2)))
		{
			if (base.Input.IsGameKeyPressed(16))
			{
				_strafeModeActive = true;
			}
		}
		else
		{
			_strafeModeActive = false;
		}
		MovementControlFlag val3 = _lastMovementKeyPressed;
		if (base.Input.IsGameKeyPressed(0))
		{
			val3 = (MovementControlFlag)1;
		}
		else if (base.Input.IsGameKeyPressed(1))
		{
			val3 = (MovementControlFlag)2;
		}
		else if (base.Input.IsGameKeyPressed(2))
		{
			val3 = (MovementControlFlag)8;
		}
		else if (base.Input.IsGameKeyPressed(3))
		{
			val3 = (MovementControlFlag)4;
		}
		if (val3 != _lastMovementKeyPressed)
		{
			_lastMovementKeyPressed = val3;
			Game current2 = Game.Current;
			if (current2 != null)
			{
				current2.EventManager.TriggerEvent<MissionPlayerMovementFlagsChangeEvent>(new MissionPlayerMovementFlagsChangeEvent(_lastMovementKeyPressed));
			}
		}
		if (!base.Input.GetIsMouseActive())
		{
			bool flag6 = true;
			if (flag3)
			{
				val3 = (MovementControlFlag)1;
			}
			else if (flag5)
			{
				val3 = (MovementControlFlag)2;
			}
			else if (flag4)
			{
				val3 = (MovementControlFlag)8;
			}
			else if (flag2)
			{
				val3 = (MovementControlFlag)4;
			}
			else
			{
				flag6 = false;
			}
			if (flag6)
			{
				((MissionBehavior)this).Mission.SetLastMovementKeyPressed(val3);
			}
		}
		else
		{
			((MissionBehavior)this).Mission.SetLastMovementKeyPressed(_lastMovementKeyPressed);
		}
		if (base.Input.IsGameKeyPressed(0))
		{
			if (_lastForwardKeyPressTime + 0.3f > Time.ApplicationTime)
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags & -458753);
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x10000);
			}
			_lastForwardKeyPressTime = Time.ApplicationTime;
		}
		if (base.Input.IsGameKeyPressed(1))
		{
			if (_lastBackwardKeyPressTime + 0.3f > Time.ApplicationTime)
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags & -458753);
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x20000);
			}
			_lastBackwardKeyPressTime = Time.ApplicationTime;
		}
		if (base.Input.IsGameKeyPressed(2))
		{
			if (_lastLeftKeyPressTime + 0.3f > Time.ApplicationTime)
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags & -458753);
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x30000);
			}
			_lastLeftKeyPressTime = Time.ApplicationTime;
		}
		if (base.Input.IsGameKeyPressed(3))
		{
			if (_lastRightKeyPressTime + 0.3f > Time.ApplicationTime)
			{
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags & -458753);
				mainAgent.EventControlFlags = (EventControlFlag)(mainAgent.EventControlFlags | 0x40000);
			}
			_lastRightKeyPressTime = Time.ApplicationTime;
		}
		if (_isTargetLockEnabled && !IsThereAnyCustomCameraAddition())
		{
			if (base.Input.IsGameKeyDown(26) && LockedAgent == null && !base.Input.IsGameKeyDown(25) && ((int)((MissionBehavior)this).Mission.Mode == 2 || (int)((MissionBehavior)this).Mission.Mode == 4))
			{
				wieldedWeapon = mainAgent.WieldedWeapon;
				if (!((MissionWeapon)(ref wieldedWeapon)).IsEmpty)
				{
					wieldedWeapon = mainAgent.WieldedWeapon;
					if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsRangedWeapon)
					{
						goto IL_06a7;
					}
				}
				if (!GameNetwork.IsMultiplayer)
				{
					float applicationTime = Time.ApplicationTime;
					if (_lastLockKeyPressTime <= 0f)
					{
						_lastLockKeyPressTime = applicationTime;
					}
					if (applicationTime > _lastLockKeyPressTime + 0.3f)
					{
						PotentialLockTargetAgent = FindTargetedLockableAgent(mainAgent);
					}
					goto IL_06ae;
				}
			}
			goto IL_06a7;
		}
		goto IL_077a;
	}

	private void HandleRangedWeaponAttackAlternativeAiming(Agent player)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (base.Input.GetKeyState((InputKey)254).x > 0.2f)
		{
			if (base.Input.GetKeyState((InputKey)255).x < 0.6f)
			{
				player.MovementFlags |= player.AttackDirectionToMovementFlag(player.GetAttackDirection());
			}
			_isPlayerAiming = true;
		}
		else if (_isPlayerAiming)
		{
			player.MovementFlags = (MovementControlFlag)(player.MovementFlags | 0x1000);
			_isPlayerAiming = false;
		}
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		if (otherAgent.IsMount)
		{
			return otherAgent.IsActive();
		}
		return false;
	}

	public void Disable()
	{
		_activated = false;
	}

	public void Enable()
	{
		_activated = true;
	}

	private void OnPlayerToggleOrder(MissionPlayerToggledOrderViewEvent obj)
	{
		_isPlayerOrderOpen = obj.IsOrderEnabled;
	}

	public void OnWeaponUsageToggleRequested()
	{
		_weaponUsageToggleRequested = true;
	}

	public void AddOverrideControlsForFrame(OverrideMainAgentControlFlag overrideFlag)
	{
		_overrideControlsThisFrame |= overrideFlag;
	}

	private void OnManagedOptionChanged(ManagedOptionsType optionType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)optionType == 17)
		{
			UpdateLockTargetOption();
		}
	}

	private void UpdateLockTargetOption()
	{
		_isTargetLockEnabled = ManagedOptions.GetConfig((ManagedOptionsType)17) == 1f;
		LockedAgent = null;
		PotentialLockTargetAgent = null;
		_lastLockKeyPressTime = 0f;
		_lastLockedAgentHeightDifference = 0f;
	}

	private bool IsThereAnyCustomCameraAddition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = ((MissionBehavior)this).Mission.CustomCameraTargetLocalOffset;
		if (!((Vec3)(ref val)).IsNonZero)
		{
			val = ((MissionBehavior)this).Mission.CustomCameraLocalOffset;
			if (!((Vec3)(ref val)).IsNonZero)
			{
				val = ((MissionBehavior)this).Mission.CustomCameraLocalOffset2;
				if (!((Vec3)(ref val)).IsNonZero)
				{
					val = ((MissionBehavior)this).Mission.CustomCameraGlobalOffset;
					if (!((Vec3)(ref val)).IsNonZero)
					{
						val = ((MissionBehavior)this).Mission.CustomCameraLocalRotationalOffset;
						if (!((Vec3)(ref val)).IsNonZero)
						{
							return ((MissionBehavior)this).Mission.CustomCameraFixedDistance != float.MinValue;
						}
					}
				}
			}
		}
		return true;
	}
}
