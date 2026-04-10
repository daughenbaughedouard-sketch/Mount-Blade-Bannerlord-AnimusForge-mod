using System;
using System.Collections.ObjectModel;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionMainAgentEquipDropView))]
public class MissionGauntletMainAgentEquipDropView : MissionView
{
	private const int _missionTimeSpeedRequestID = 624;

	private const float _slowDownAmountWhileRadialIsOpen = 0.25f;

	private bool _isSlowDownApplied;

	private GauntletLayer _gauntletLayer;

	private MissionMainAgentControllerEquipDropVM _dataSource;

	private MissionMainAgentController _missionMainAgentController;

	private EquipmentControllerLeaveLogic _missionControllerLeaveLogic;

	private const float _minOpenHoldTime = 0.3f;

	private const float _minDropHoldTime = 0.5f;

	private readonly IMissionScreen _missionScreenAsInterface;

	private bool _holdHandled;

	private float _toggleHoldTime;

	private float _weaponDropHoldTime;

	private bool _prevKeyDown;

	private bool _weaponDropHandled;

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

	public MissionGauntletMainAgentEquipDropView()
	{
		_missionScreenAsInterface = (IMissionScreen)(object)base.MissionScreen;
		HoldHandled = false;
	}

	public override void EarlyStart()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		((MissionBehavior)this).EarlyStart();
		_gauntletLayer = new GauntletLayer("MissionEquipDrop", ViewOrderPriority, false);
		_dataSource = new MissionMainAgentControllerEquipDropVM((Action<EquipmentIndex>)OnToggleItem);
		_missionMainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		_missionControllerLeaveLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<EquipmentControllerLeaveLogic>();
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("CombatHotKeyCategory"));
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)0);
		_gauntletLayer.LoadMovie("MainAgentControllerEquipDrop", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionBehavior)this).Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(OnMainAgentChanged);
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveChanged));
	}

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_dataSource.InitializeMainAgentPropterties();
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(OnGamepadActiveChanged));
		((MissionBehavior)this).Mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(OnMainAgentChanged);
		((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_missionMainAgentController = null;
		_missionControllerLeaveLogic = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (_dataSource.IsActive && !IsMainAgentAvailable())
		{
			HandleClosingHold();
		}
		if (IsMainAgentAvailable() && (!base.MissionScreen.IsRadialMenuActive || _dataSource.IsActive))
		{
			TickControls(dt);
		}
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
			_dataSource.OnCancelHoldController();
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
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		if ((((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsGameKeyDown(34) || ((ScreenLayer)_gauntletLayer).Input.IsGameKeyDown(34)) && !IsDisplayingADialog && !base.MissionScreen.IsPhotoModeEnabled && (int)((MissionBehavior)this).Mission.Mode != 6 && (int)((MissionBehavior)this).Mission.Mode != 9 && !base.MissionScreen.IsRadialMenuActive)
		{
			if (_toggleHoldTime > 0.3f && !HoldHandled)
			{
				HandleOpeningHold();
				HoldHandled = true;
			}
			_toggleHoldTime += dt;
			_prevKeyDown = true;
		}
		else if (_prevKeyDown && !((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsGameKeyDown(34) && !((ScreenLayer)_gauntletLayer).Input.IsGameKeyDown(34))
		{
			if (_toggleHoldTime < 0.3f)
			{
				HandleQuickRelease();
			}
			else
			{
				HandleClosingHold();
			}
			HoldHandled = false;
			_toggleHoldTime = 0f;
			_weaponDropHoldTime = 0f;
			_prevKeyDown = false;
			_weaponDropHandled = false;
		}
		if (HoldHandled)
		{
			int keyWeaponIndex = GetKeyWeaponIndex(isReleased: false);
			int keyWeaponIndex2 = GetKeyWeaponIndex(isReleased: true);
			_dataSource.SetDropProgressForIndex((EquipmentIndex)(-1), _weaponDropHoldTime / 0.5f);
			MissionWeapon val;
			if (keyWeaponIndex != -1)
			{
				if (!_weaponDropHandled)
				{
					int num = keyWeaponIndex;
					if (_weaponDropHoldTime > 0.5f)
					{
						val = Agent.Main.Equipment[num];
						if (!((MissionWeapon)(ref val)).IsEmpty)
						{
							OnDropEquipment((EquipmentIndex)num);
							_dataSource.OnWeaponDroppedAtIndex(keyWeaponIndex);
							_weaponDropHandled = true;
						}
					}
					_dataSource.SetDropProgressForIndex((EquipmentIndex)num, _weaponDropHoldTime / 0.5f);
				}
				_weaponDropHoldTime += dt;
			}
			else if (keyWeaponIndex2 != -1)
			{
				if (!_weaponDropHandled)
				{
					int num2 = keyWeaponIndex2;
					val = Agent.Main.Equipment[num2];
					if (!((MissionWeapon)(ref val)).IsEmpty && num2 != 4)
					{
						OnToggleItem((EquipmentIndex)num2);
						_dataSource.OnWeaponEquippedAtIndex(keyWeaponIndex2);
						_weaponDropHandled = true;
					}
				}
				_weaponDropHoldTime = 0f;
			}
			else
			{
				_weaponDropHoldTime = 0f;
				_weaponDropHandled = false;
			}
		}
		else
		{
			_weaponDropHoldTime = 0f;
			_weaponDropHandled = false;
		}
	}

	private void HandleOpeningHold()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		MissionMainAgentControllerEquipDropVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnToggle(true);
		}
		base.MissionScreen.RegisterRadialMenuObject(this);
		EquipmentControllerLeaveLogic missionControllerLeaveLogic = _missionControllerLeaveLogic;
		if (missionControllerLeaveLogic != null)
		{
			missionControllerLeaveLogic.SetIsEquipmentSelectionActive(true);
		}
		if (!GameNetwork.IsMultiplayer && !_isSlowDownApplied)
		{
			((MissionBehavior)this).Mission.AddTimeSpeedRequest(new TimeSpeedRequest(0.25f, 624));
			_isSlowDownApplied = true;
		}
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
	}

	private void HandleClosingHold()
	{
		MissionMainAgentControllerEquipDropVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnToggle(false);
		}
		base.MissionScreen.UnregisterRadialMenuObject(this);
		EquipmentControllerLeaveLogic missionControllerLeaveLogic = _missionControllerLeaveLogic;
		if (missionControllerLeaveLogic != null)
		{
			missionControllerLeaveLogic.SetIsEquipmentSelectionActive(false);
		}
		if (!GameNetwork.IsMultiplayer && _isSlowDownApplied)
		{
			((MissionBehavior)this).Mission.RemoveTimeSpeedRequest(624);
			_isSlowDownApplied = false;
		}
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
	}

	private void HandleQuickRelease()
	{
		_missionMainAgentController.OnWeaponUsageToggleRequested();
		MissionMainAgentControllerEquipDropVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnToggle(false);
		}
		base.MissionScreen.UnregisterRadialMenuObject(this);
		EquipmentControllerLeaveLogic missionControllerLeaveLogic = _missionControllerLeaveLogic;
		if (missionControllerLeaveLogic != null)
		{
			missionControllerLeaveLogic.SetIsEquipmentSelectionActive(false);
		}
	}

	private void OnToggleItem(EquipmentIndex indexToToggle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		bool flag = indexToToggle == Agent.Main.GetPrimaryWieldedItemIndex();
		bool flag2 = indexToToggle == Agent.Main.GetOffhandWieldedItemIndex();
		if (flag || flag2)
		{
			Agent.Main.TryToSheathWeaponInHand((HandIndex)(!flag), (WeaponWieldActionType)0);
		}
		else
		{
			Agent.Main.TryToWieldWeaponInSlot(indexToToggle, (WeaponWieldActionType)0, false);
		}
	}

	private void OnDropEquipment(EquipmentIndex indexToDrop)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new DropWeapon(base.Input.IsGameKeyDown(10), indexToDrop));
			GameNetwork.EndModuleEventAsClient();
		}
		else
		{
			Agent.Main.HandleDropWeapon(base.Input.IsGameKeyDown(10), indexToDrop);
		}
	}

	private bool IsMainAgentAvailable()
	{
		Agent main = Agent.Main;
		if (main != null && main.IsActive())
		{
			Agent main2 = Agent.Main;
			if (main2 == null || main2.Mission.IsNavalBattle)
			{
				return !Agent.Main.IsUsingGameObject;
			}
			return true;
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

	private void OnGamepadActiveChanged()
	{
		_dataSource.OnGamepadActiveChanged(Input.IsGamepadActive);
	}

	private int GetKeyWeaponIndex(bool isReleased)
	{
		Func<string, bool> func = ((!isReleased) ? new Func<string, bool>(((ScreenLayer)_gauntletLayer).Input.IsHotKeyDown) : new Func<string, bool>(((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased));
		string text = string.Empty;
		if (func("ControllerEquipDropWeapon1"))
		{
			text = "ControllerEquipDropWeapon1";
		}
		else if (func("ControllerEquipDropWeapon2"))
		{
			text = "ControllerEquipDropWeapon2";
		}
		else if (func("ControllerEquipDropWeapon3"))
		{
			text = "ControllerEquipDropWeapon3";
		}
		else if (func("ControllerEquipDropWeapon4"))
		{
			text = "ControllerEquipDropWeapon4";
		}
		else if (func("ControllerEquipDropExtraWeapon"))
		{
			text = "ControllerEquipDropExtraWeapon";
		}
		if (!string.IsNullOrEmpty(text))
		{
			for (int i = 0; i < ((Collection<ControllerEquippedItemVM>)(object)_dataSource.EquippedWeapons).Count; i++)
			{
				InputKeyItemVM shortcutKey = ((Collection<ControllerEquippedItemVM>)(object)_dataSource.EquippedWeapons)[i].ShortcutKey;
				if (((shortcutKey != null) ? shortcutKey.HotKey.Id : null) == text)
				{
					return (int)((EquipmentActionItemVM)((Collection<ControllerEquippedItemVM>)(object)_dataSource.EquippedWeapons)[i]).Identifier;
				}
			}
			ControllerEquippedItemVM equippedExtraWeapon = _dataSource.EquippedExtraWeapon;
			object obj;
			if (equippedExtraWeapon == null)
			{
				obj = null;
			}
			else
			{
				InputKeyItemVM shortcutKey2 = equippedExtraWeapon.ShortcutKey;
				obj = ((shortcutKey2 != null) ? shortcutKey2.HotKey.Id : null);
			}
			if ((string?)obj == text)
			{
				return (int)((EquipmentActionItemVM)_dataSource.EquippedExtraWeapon).Identifier;
			}
		}
		return -1;
	}
}
