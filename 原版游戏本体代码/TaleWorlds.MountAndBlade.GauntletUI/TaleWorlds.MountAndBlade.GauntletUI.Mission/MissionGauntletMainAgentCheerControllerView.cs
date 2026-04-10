using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionMainAgentCheerBarkControllerView))]
public class MissionGauntletMainAgentCheerControllerView : MissionView
{
	private const float CooldownPeriodDurationAfterCheer = 4f;

	private const float CooldownPeriodDurationAfterBark = 2f;

	private const float _minHoldTime = 0f;

	private readonly IMissionScreen _missionScreenAsInterface;

	private MissionMainAgentController _missionMainAgentController;

	private readonly TextObject _cooldownInfoText = new TextObject("{=aogZyZlR}You need to wait {SECONDS} seconds until you can cheer/shout again.", (Dictionary<string, object>)null);

	private bool _holdHandled;

	private float _holdTime;

	private bool _prevCheerKeyDown;

	private GauntletLayer _gauntletLayer;

	private MissionMainAgentCheerBarkControllerVM _dataSource;

	private float _cooldownTimeRemaining;

	private bool _isSelectingFromInput;

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

	public MissionGauntletMainAgentCheerControllerView()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		_missionScreenAsInterface = (IMissionScreen)(object)base.MissionScreen;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_gauntletLayer = new GauntletLayer("MissionCheerController", ViewOrderPriority, false);
		_missionMainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		_dataSource = new MissionMainAgentCheerBarkControllerVM((Action<int>)OnCheerSelect, (Action<int>)OnBarkSelect);
		_gauntletLayer.LoadMovie("MainAgentCheerBarkController", (ViewModel)(object)_dataSource);
		GameKeyContext category = HotKeyManager.GetCategory("CombatHotKeyCategory");
		if (_missionMainAgentController != null)
		{
			IInputContext input = _missionMainAgentController.Input;
			InputContext val = (InputContext)(object)((input is InputContext) ? input : null);
			if (val != null && !val.IsCategoryRegistered(category))
			{
				val.RegisterHotKeyCategory(category);
			}
		}
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
		_missionMainAgentController = null;
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		base.OnMissionScreenTick(dt);
		if (IsMainAgentAvailable() && (int)((MissionBehavior)this).Mission.Mode != 6 && (!base.MissionScreen.IsRadialMenuActive || _dataSource.IsActive))
		{
			TickControls(dt);
		}
		else if (_dataSource.IsActive)
		{
			HandleClosingHold(applySelection: false);
		}
	}

	private void OnMainAgentChanged(Agent oldAgent)
	{
		if (((MissionBehavior)this).Mission.MainAgent == null)
		{
			HandleClosingHold(applySelection: false);
		}
	}

	private void HandleNodeSelectionInput(CheerBarkNodeItemVM node, int nodeIndex, int parentNodeIndex = -1)
	{
		if (_missionMainAgentController == null)
		{
			return;
		}
		IInputContext input = _missionMainAgentController.Input;
		if (node.ShortcutKey == null)
		{
			return;
		}
		if (input.IsHotKeyPressed(node.ShortcutKey.HotKey.Id))
		{
			if (parentNodeIndex != -1)
			{
				_dataSource.SelectItem(parentNodeIndex, nodeIndex);
				return;
			}
			_dataSource.SelectItem(nodeIndex, -1);
			_isSelectingFromInput = node.HasSubNodes;
		}
		else
		{
			if (!input.IsHotKeyReleased(node.ShortcutKey.HotKey.Id))
			{
				return;
			}
			if (!_isSelectingFromInput)
			{
				HandleClosingHold(applySelection: true);
				_dataSource.Nodes.ApplyActionOnAllItems((Action<CheerBarkNodeItemVM>)delegate(CheerBarkNodeItemVM n)
				{
					n.ClearSelectionRecursive();
				});
			}
			_isSelectingFromInput = false;
		}
	}

	private void TickControls(float dt)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		if (_missionMainAgentController == null)
		{
			return;
		}
		IInputContext input = _missionMainAgentController.Input;
		if (GameNetwork.IsMultiplayer && _cooldownTimeRemaining > 0f)
		{
			_cooldownTimeRemaining -= dt;
			if (input.IsGameKeyDown(31))
			{
				if (!_prevCheerKeyDown && (double)_cooldownTimeRemaining >= 0.1)
				{
					_cooldownInfoText.SetTextVariable("SECONDS", _cooldownTimeRemaining.ToString("0.0"));
					InformationManager.DisplayMessage(new InformationMessage(((object)_cooldownInfoText).ToString()));
				}
				_prevCheerKeyDown = true;
			}
			else
			{
				_prevCheerKeyDown = false;
			}
			return;
		}
		if (HoldHandled && _dataSource.IsActive)
		{
			int num = -1;
			for (int i = 0; i < ((Collection<CheerBarkNodeItemVM>)(object)_dataSource.Nodes).Count; i++)
			{
				if (((Collection<CheerBarkNodeItemVM>)(object)_dataSource.Nodes)[i].IsSelected)
				{
					num = i;
					break;
				}
			}
			if (_dataSource.IsNodesCategories)
			{
				if (num != -1)
				{
					for (int j = 0; j < ((Collection<CheerBarkNodeItemVM>)(object)((Collection<CheerBarkNodeItemVM>)(object)_dataSource.Nodes)[num].SubNodes).Count; j++)
					{
						HandleNodeSelectionInput(((Collection<CheerBarkNodeItemVM>)(object)((Collection<CheerBarkNodeItemVM>)(object)_dataSource.Nodes)[num].SubNodes)[j], j, num);
					}
				}
				else if (input.IsHotKeyReleased("CheerBarkSelectFirstCategory"))
				{
					_dataSource.SelectItem(0, -1);
				}
				else if (input.IsHotKeyReleased("CheerBarkSelectSecondCategory"))
				{
					_dataSource.SelectItem(1, -1);
				}
			}
			else
			{
				for (int k = 0; k < ((Collection<CheerBarkNodeItemVM>)(object)_dataSource.Nodes).Count; k++)
				{
					HandleNodeSelectionInput(((Collection<CheerBarkNodeItemVM>)(object)_dataSource.Nodes)[k], k);
				}
			}
		}
		if (input.IsGameKeyDown(31) && !IsDisplayingADialog && !base.MissionScreen.IsRadialMenuActive)
		{
			if (_holdTime > 0f && !HoldHandled)
			{
				HandleOpenHold();
				HoldHandled = true;
			}
			_holdTime += dt;
			_prevCheerKeyDown = true;
		}
		else if (_prevCheerKeyDown && !input.IsGameKeyDown(31))
		{
			if (_holdTime < 0f)
			{
				HandleQuickRelease();
			}
			else
			{
				HandleClosingHold(applySelection: true);
			}
			HoldHandled = false;
			_holdTime = 0f;
			_prevCheerKeyDown = false;
		}
	}

	private void HandleOpenHold()
	{
		if (!_dataSource.IsActive)
		{
			_dataSource.ExecuteActivate();
			base.MissionScreen.RegisterRadialMenuObject(this);
		}
	}

	private void HandleClosingHold(bool applySelection)
	{
		if (_dataSource.IsActive)
		{
			_dataSource.ExecuteDeactivate(applySelection);
			base.MissionScreen.UnregisterRadialMenuObject(this);
		}
	}

	private void HandleQuickRelease()
	{
		OnCheerSelect(-1);
	}

	private void OnCheerSelect(int tauntIndex)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (tauntIndex < 0)
		{
			return;
		}
		if (GameNetwork.IsClient)
		{
			TauntUsageFlag actionNotUsableReason = CosmeticsManagerHelper.GetActionNotUsableReason(Agent.Main, tauntIndex);
			if ((int)actionNotUsableReason != 0)
			{
				InformationManager.DisplayMessage(new InformationMessage(TauntUsageManager.GetActionDisabledReasonText(actionNotUsableReason)));
				return;
			}
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new TauntSelected(tauntIndex));
			GameNetwork.EndModuleEventAsClient();
		}
		else
		{
			Agent main = Agent.Main;
			if (main != null)
			{
				main.HandleTaunt(tauntIndex, true);
			}
		}
		_cooldownTimeRemaining = 4f;
	}

	private void OnBarkSelect(int indexOfBark)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new BarkSelected(indexOfBark));
			GameNetwork.EndModuleEventAsClient();
		}
		else
		{
			Agent main = Agent.Main;
			if (main != null)
			{
				main.HandleBark(indexOfBark);
			}
		}
		_cooldownTimeRemaining = 2f;
	}

	private bool IsMainAgentAvailable()
	{
		Agent main = Agent.Main;
		if (main != null && main.IsActive() && !Agent.Main.IsUsingGameObject)
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
