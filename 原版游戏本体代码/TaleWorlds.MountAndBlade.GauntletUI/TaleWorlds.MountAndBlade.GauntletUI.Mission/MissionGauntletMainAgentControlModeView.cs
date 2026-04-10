using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.WalkMode;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionMainAgentControlModeView))]
public class MissionGauntletMainAgentControlModeView : MissionView
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static GetCanChangeWalkModeActivatedDelegate _003C_003E9__24_2;

		public static Func<WalkModeItemVM, bool> _003C_003E9__24_6;

		internal bool _003CInitializeWalkModes_003Eb__24_2()
		{
			return true;
		}

		internal bool _003CInitializeWalkModes_003Eb__24_6(WalkModeItemVM w)
		{
			return w.TypeId == "crouch";
		}
	}

	private const int _missionTimeSpeedRequestID = 813;

	private readonly IMissionScreen _missionScreenAsInterface;

	private GauntletLayer _gauntletLayer;

	private MissionMainAgentWalkModeControllerVM _dataSource;

	private MissionMainAgentController _mainAgentController;

	private bool _isSlowDownApplied;

	private bool _holdHandled;

	private bool _prevKeyDown;

	private float _toggleHoldTime;

	private float _playerDismountTimer;

	private float _slowDownAmountWhileRadialIsOpen => 0.25f;

	private float _minOpenHoldTime => 0.22f;

	private bool IsDisplayingADialog
	{
		get
		{
			IMissionScreen missionScreenAsInterface = _missionScreenAsInterface;
			if ((missionScreenAsInterface == null || !missionScreenAsInterface.GetDisplayDialog()) && !base.MissionScreen.IsRadialMenuActive)
			{
				return ((MissionBehavior)this).Mission.IsOrderMenuOpen;
			}
			return true;
		}
	}

	private bool HoldHandled
	{
		get
		{
			return _holdHandled;
		}
		set
		{
			_holdHandled = value;
		}
	}

	public MissionGauntletMainAgentControlModeView()
	{
		_missionScreenAsInterface = (IMissionScreen)(object)base.MissionScreen;
		HoldHandled = false;
	}

	public override void EarlyStart()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		((MissionBehavior)this).EarlyStart();
		_gauntletLayer = new GauntletLayer("MissionAgentControlMode", 3, false);
		_dataSource = new MissionMainAgentWalkModeControllerVM();
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("CombatHotKeyCategory"));
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)0);
		_gauntletLayer.LoadMovie("MainAgentControlMode", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		_mainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		((MissionBehavior)this).Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(OnMainAgentChanged);
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		InitializeWalkModes();
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		((MissionBehavior)this).Mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(OnMainAgentChanged);
		((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
		if (mainAgent == null || mainAgent.HasMount)
		{
			_playerDismountTimer = 0f;
		}
		else if (_playerDismountTimer < 2f)
		{
			_playerDismountTimer += dt;
		}
		if (IsMainAgentAvailable() && (!base.MissionScreen.IsRadialMenuActive || _dataSource.IsEnabled))
		{
			TickControls(dt);
		}
		else if (_dataSource.IsEnabled)
		{
			HandleClosingHold();
		}
	}

	private void InitializeWalkModes()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0069: Expected O, but got Unknown
		//IL_0069: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00b4: Expected O, but got Unknown
		//IL_00b4: Expected O, but got Unknown
		//IL_00b4: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		GameKeyContext category = HotKeyManager.GetCategory("CombatHotKeyCategory");
		MissionMainAgentWalkModeControllerVM dataSource = _dataSource;
		TextObject val = new TextObject("{=zmS2FpJH}Toggle Walk", (Dictionary<string, object>)null);
		GetIsWalkModeActivatedDelegate val2 = () => ((MissionBehavior)this).Mission.MainAgent != null && ((MissionBehavior)this).Mission.MainAgent.WalkMode;
		SetIsWalkModeActivatedDelegate val3 = delegate(bool value)
		{
			if (((MissionBehavior)this).Mission.MainAgent != null)
			{
				if (value)
				{
					_mainAgentController.AddOverrideControlsForFrame(MissionMainAgentController.OverrideMainAgentControlFlag.Walk);
				}
				else
				{
					_mainAgentController.AddOverrideControlsForFrame(MissionMainAgentController.OverrideMainAgentControlFlag.Run);
				}
			}
		};
		object obj = _003C_003Ec._003C_003E9__24_2;
		if (obj == null)
		{
			GetCanChangeWalkModeActivatedDelegate val4 = () => true;
			_003C_003Ec._003C_003E9__24_2 = val4;
			obj = (object)val4;
		}
		dataSource.AddWalkMode("walk", val, val2, val3, (GetCanChangeWalkModeActivatedDelegate)obj, category.GetHotKey("ControllerToggleWalk"), true);
		_dataSource.AddWalkMode("crouch", new TextObject("{=0pd93SuK}Toggle Crouch", (Dictionary<string, object>)null), (GetIsWalkModeActivatedDelegate)(() => ((MissionBehavior)this).Mission.MainAgent != null && ((MissionBehavior)this).Mission.MainAgent.CrouchMode), (SetIsWalkModeActivatedDelegate)delegate(bool value)
		{
			if (((MissionBehavior)this).Mission.MainAgent != null)
			{
				if (value)
				{
					_mainAgentController.AddOverrideControlsForFrame(MissionMainAgentController.OverrideMainAgentControlFlag.Crouch);
				}
				else
				{
					_mainAgentController.AddOverrideControlsForFrame(MissionMainAgentController.OverrideMainAgentControlFlag.Stand);
				}
			}
		}, (GetCanChangeWalkModeActivatedDelegate)(() => ((MissionBehavior)this).Mission.MainAgent == null || ((MissionBehavior)this).Mission.MainAgent.IsCrouchingAllowed()), category.GetHotKey("ControllerToggleCrouch"), true);
		_dataSource.LastUsedItem = ((IEnumerable<WalkModeItemVM>)_dataSource.ControlModes).FirstOrDefault((Func<WalkModeItemVM, bool>)((WalkModeItemVM w) => w.TypeId == "crouch"));
	}

	private void OnMainAgentChanged(Agent oldAgent)
	{
		if (((MissionBehavior)this).Mission.MainAgent == null)
		{
			if (HoldHandled)
			{
				HoldHandled = false;
			}
			_toggleHoldTime = 0f;
			_dataSource.SetEnabled(false);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
	{
		if (affectedAgent == Agent.Main)
		{
			HandleClosingHold();
		}
	}

	private void TickControls(float dt)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsHotKeyDown("ControlModeToggle") && !base.MissionScreen.IsPhotoModeEnabled && !IsDisplayingADialog && (int)((MissionBehavior)this).Mission.Mode != 6 && (int)((MissionBehavior)this).Mission.Mode != 9 && !base.MissionScreen.IsRadialMenuActive)
		{
			if (_toggleHoldTime > _minOpenHoldTime && !HoldHandled)
			{
				HandleOpeningHold();
				HoldHandled = true;
			}
			_toggleHoldTime += dt;
			_prevKeyDown = true;
		}
		else if (_prevKeyDown && !((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsHotKeyDown("ControlModeToggle"))
		{
			if (_toggleHoldTime < _minOpenHoldTime)
			{
				HandleQuickRelease();
			}
			else
			{
				HandleClosingHold();
			}
			HoldHandled = false;
			_toggleHoldTime = 0f;
			_prevKeyDown = false;
		}
		if (!_dataSource.IsEnabled)
		{
			return;
		}
		for (int i = 0; i < ((Collection<WalkModeItemVM>)(object)_dataSource.ControlModes).Count; i++)
		{
			WalkModeItemVM val = ((Collection<WalkModeItemVM>)(object)_dataSource.ControlModes)[i];
			InputKeyItemVM toggleInputKey = val.ToggleInputKey;
			if (((toggleInputKey.HotKey != null && base.Input.IsHotKeyReleased(toggleInputKey.HotKey.Id)) || (toggleInputKey.GameKey != null && base.Input.IsGameKeyReleased(toggleInputKey.GameKey.Id))) && !val.IsDisabled)
			{
				val.ToggleState();
				HandleClosingHold();
				break;
			}
		}
	}

	private void HandleOpeningHold()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		MissionMainAgentWalkModeControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.SetEnabled(true);
		}
		base.MissionScreen.RegisterRadialMenuObject(this);
		if (!GameNetwork.IsMultiplayer && !_isSlowDownApplied)
		{
			((MissionBehavior)this).Mission.AddTimeSpeedRequest(new TimeSpeedRequest(_slowDownAmountWhileRadialIsOpen, 813));
			_isSlowDownApplied = true;
		}
	}

	private void HandleClosingHold()
	{
		MissionMainAgentWalkModeControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.SetEnabled(false);
		}
		base.MissionScreen.UnregisterRadialMenuObject(this);
		if (!GameNetwork.IsMultiplayer && _isSlowDownApplied)
		{
			((MissionBehavior)this).Mission.RemoveTimeSpeedRequest(813);
			_isSlowDownApplied = false;
		}
	}

	private void HandleQuickRelease()
	{
		MissionMainAgentWalkModeControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			WalkModeItemVM lastUsedItem = dataSource.LastUsedItem;
			if (lastUsedItem != null)
			{
				lastUsedItem.ToggleState();
			}
		}
		MissionMainAgentWalkModeControllerVM dataSource2 = _dataSource;
		if (dataSource2 != null)
		{
			dataSource2.SetEnabled(false);
		}
		base.MissionScreen.UnregisterRadialMenuObject(this);
	}

	private bool IsMainAgentAvailable()
	{
		Agent main = Agent.Main;
		if (main != null && main.IsActive() && Agent.Main.MountAgent == null && _playerDismountTimer >= 2f && !Agent.Main.IsUsingGameObject)
		{
			return !Agent.Main.IsInWater();
		}
		return false;
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
