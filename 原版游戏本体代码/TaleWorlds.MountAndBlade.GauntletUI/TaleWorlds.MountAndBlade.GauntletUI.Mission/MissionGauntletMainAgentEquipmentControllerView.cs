using System;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionMainAgentEquipmentControllerView))]
public class MissionGauntletMainAgentEquipmentControllerView : MissionView
{
	private const float _minHoldTime = 0.5f;

	private readonly IMissionScreen _missionScreenAsInterface;

	private bool _equipmentWasInFocusFirstFrameOfEquipDown;

	private bool _firstFrameOfEquipDownHandled;

	private bool _equipHoldHandled;

	private bool _isFocusedOnEquipment;

	private float _equipHoldTime;

	private bool _prevEquipKeyDown;

	private SpawnedItemEntity _focusedWeaponItem;

	private bool _dropHoldHandled;

	private float _dropHoldTime;

	private bool _prevDropKeyDown;

	private bool _isCurrentFocusedItemInteractable;

	private GauntletLayer _gauntletLayer;

	private MissionMainAgentEquipmentControllerVM _dataSource;

	private bool IsDisplayingADialog
	{
		get
		{
			IMissionScreen missionScreenAsInterface = _missionScreenAsInterface;
			if (missionScreenAsInterface == null)
			{
				return false;
			}
			return missionScreenAsInterface.GetDisplayDialog();
		}
	}

	private bool EquipHoldHandled
	{
		get
		{
			return _equipHoldHandled;
		}
		set
		{
			_equipHoldHandled = value;
			if (_equipHoldHandled)
			{
				base.MissionScreen?.RegisterRadialMenuObject(this);
			}
			else
			{
				base.MissionScreen?.UnregisterRadialMenuObject(this);
			}
		}
	}

	private bool DropHoldHandled
	{
		get
		{
			return _dropHoldHandled;
		}
		set
		{
			_dropHoldHandled = value;
			if (_dropHoldHandled)
			{
				base.MissionScreen?.RegisterRadialMenuObject(this);
			}
			else
			{
				base.MissionScreen?.UnregisterRadialMenuObject(this);
			}
		}
	}

	public event Action<bool> OnEquipmentDropInteractionViewToggled;

	public event Action<bool> OnEquipmentEquipInteractionViewToggled;

	public MissionGauntletMainAgentEquipmentControllerView()
	{
		_missionScreenAsInterface = (IMissionScreen)(object)base.MissionScreen;
		EquipHoldHandled = false;
		DropHoldHandled = false;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_gauntletLayer = new GauntletLayer("MissionEquipmentController", ViewOrderPriority, false);
		_dataSource = new MissionMainAgentEquipmentControllerVM((Action<EquipmentIndex>)OnDropEquipment, (Action<SpawnedItemEntity, EquipmentIndex>)OnEquipItem);
		_gauntletLayer.LoadMovie("MainAgentEquipmentController", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)0);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionBehavior)this).Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(OnMainAgentChanged);
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
		if (IsMainAgentAvailable() && ((MissionBehavior)this).Mission.IsMainAgentItemInteractionEnabled)
		{
			DropWeaponTick(dt);
			EquipWeaponTick(dt);
		}
		else
		{
			_prevDropKeyDown = false;
			_prevEquipKeyDown = false;
		}
	}

	public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnFocusGained(agent, focusableObject, isInteractable);
		UsableMissionObject val;
		SpawnedItemEntity val2;
		if ((val = (UsableMissionObject)(object)((focusableObject is UsableMissionObject) ? focusableObject : null)) != null && (val2 = (SpawnedItemEntity)(object)((val is SpawnedItemEntity) ? val : null)) != null)
		{
			_isCurrentFocusedItemInteractable = isInteractable;
			MissionWeapon weaponCopy = val2.WeaponCopy;
			if (!((MissionWeapon)(ref weaponCopy)).IsEmpty)
			{
				_isFocusedOnEquipment = true;
				_focusedWeaponItem = val2;
				_dataSource.SetCurrentFocusedWeaponEntity(_focusedWeaponItem);
			}
		}
	}

	public override void OnFocusLost(Agent agent, IFocusable focusableObject)
	{
		((MissionBehavior)this).OnFocusLost(agent, focusableObject);
		_isCurrentFocusedItemInteractable = false;
		_isFocusedOnEquipment = false;
		_focusedWeaponItem = null;
		MissionMainAgentEquipmentControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.SetCurrentFocusedWeaponEntity(_focusedWeaponItem);
		}
		if (EquipHoldHandled)
		{
			EquipHoldHandled = false;
			_equipHoldTime = 0f;
			MissionMainAgentEquipmentControllerVM dataSource2 = _dataSource;
			if (dataSource2 != null)
			{
				dataSource2.OnCancelEquipController();
			}
			this.OnEquipmentEquipInteractionViewToggled?.Invoke(obj: false);
			_equipmentWasInFocusFirstFrameOfEquipDown = false;
		}
	}

	private void OnMainAgentChanged(Agent oldAgent)
	{
		if (((MissionBehavior)this).Mission.MainAgent == null)
		{
			if (EquipHoldHandled)
			{
				EquipHoldHandled = false;
				this.OnEquipmentEquipInteractionViewToggled?.Invoke(obj: false);
			}
			_equipHoldTime = 0f;
			_dataSource.OnCancelEquipController();
			if (DropHoldHandled)
			{
				this.OnEquipmentDropInteractionViewToggled?.Invoke(obj: false);
				DropHoldHandled = false;
			}
			_dropHoldTime = 0f;
			_dataSource.OnCancelDropController();
		}
	}

	private void EquipWeaponTick(float dt)
	{
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsGameKeyDown(13) && !_prevDropKeyDown && !IsDisplayingADialog && IsMainAgentAvailable() && !base.MissionScreen.Mission.IsOrderMenuOpen)
		{
			if (!_firstFrameOfEquipDownHandled)
			{
				_equipmentWasInFocusFirstFrameOfEquipDown = _isFocusedOnEquipment;
				_firstFrameOfEquipDownHandled = true;
			}
			if (_equipmentWasInFocusFirstFrameOfEquipDown)
			{
				_equipHoldTime += dt;
				if (_equipHoldTime > 0.5f && !EquipHoldHandled && _isFocusedOnEquipment && _isCurrentFocusedItemInteractable)
				{
					HandleOpeningHoldEquip();
					EquipHoldHandled = true;
				}
			}
			_prevEquipKeyDown = true;
		}
		else
		{
			if (!_prevEquipKeyDown || ((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsGameKeyDown(13))
			{
				return;
			}
			if (_equipmentWasInFocusFirstFrameOfEquipDown)
			{
				if (_equipHoldTime < 0.5f)
				{
					if (_focusedWeaponItem != null)
					{
						Agent main = Agent.Main;
						if (main != null && main.CanQuickPickUp(_focusedWeaponItem))
						{
							HandleQuickReleaseEquip();
						}
					}
				}
				else
				{
					HandleClosingHoldEquip();
				}
			}
			if (EquipHoldHandled)
			{
				EquipHoldHandled = false;
			}
			_equipHoldTime = 0f;
			_firstFrameOfEquipDownHandled = false;
			_prevEquipKeyDown = false;
		}
	}

	private void DropWeaponTick(float dt)
	{
		if (((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsGameKeyDown(22) && !_prevEquipKeyDown && !IsDisplayingADialog && IsMainAgentAvailable() && IsMainAgentHasAtLeastOneItem() && !base.MissionScreen.Mission.IsOrderMenuOpen)
		{
			_dropHoldTime += dt;
			if (_dropHoldTime > 0.5f && !DropHoldHandled)
			{
				HandleOpeningHoldDrop();
				DropHoldHandled = true;
			}
			_prevDropKeyDown = true;
		}
		else if (_prevDropKeyDown && !((ScreenLayer)base.MissionScreen.SceneLayer).Input.IsGameKeyDown(22))
		{
			if (_dropHoldTime < 0.5f)
			{
				HandleQuickReleaseDrop();
			}
			else
			{
				HandleClosingHoldDrop();
			}
			DropHoldHandled = false;
			_dropHoldTime = 0f;
			_prevDropKeyDown = false;
		}
	}

	private void HandleOpeningHoldEquip()
	{
		MissionMainAgentEquipmentControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnEquipControllerToggle(true);
		}
		this.OnEquipmentEquipInteractionViewToggled?.Invoke(obj: true);
	}

	private void HandleClosingHoldEquip()
	{
		MissionMainAgentEquipmentControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnEquipControllerToggle(false);
		}
		this.OnEquipmentEquipInteractionViewToggled?.Invoke(obj: false);
	}

	private void HandleQuickReleaseEquip()
	{
		OnEquipItem(_focusedWeaponItem, (EquipmentIndex)(-1));
	}

	private void HandleOpeningHoldDrop()
	{
		MissionMainAgentEquipmentControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnDropControllerToggle(true);
		}
		this.OnEquipmentDropInteractionViewToggled?.Invoke(obj: true);
	}

	private void HandleClosingHoldDrop()
	{
		MissionMainAgentEquipmentControllerVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.OnDropControllerToggle(false);
		}
		this.OnEquipmentDropInteractionViewToggled?.Invoke(obj: false);
	}

	private void HandleQuickReleaseDrop()
	{
		OnDropEquipment((EquipmentIndex)(-1));
	}

	private void OnEquipItem(SpawnedItemEntity itemToEquip, EquipmentIndex indexToEquipItTo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)itemToEquip).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).IsValid)
		{
			Agent main = Agent.Main;
			if (main != null)
			{
				main.HandleStartUsingAction((UsableMissionObject)(object)itemToEquip, (int)indexToEquipItTo);
			}
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
		if (main == null)
		{
			return false;
		}
		return main.IsActive();
	}

	private bool IsMainAgentHasAtLeastOneItem()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (EquipmentIndex val = (EquipmentIndex)0; (int)val < 5; val = (EquipmentIndex)(val + 1))
		{
			MissionWeapon val2 = Agent.Main.Equipment[val];
			if (!((MissionWeapon)(ref val2)).IsEmpty)
			{
				return true;
			}
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
