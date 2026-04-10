using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(MissionAgentStatusUIHandler))]
internal class MissionGauntletNavalAgentStatus : MissionGauntletAgentStatus
{
	private NavalShipsLogic _navalShipsLogic;

	private TextObject _selectShipText;

	private TextObject _attemptBoardingText;

	private TextObject _cancelBoardingText;

	private IShipOrigin _focusedShipOrigin;

	private bool _focusedShipIsEnemy;

	private bool _canSelectShip;

	private bool _canAttemptBoarding;

	private bool _isBoardingBlocked;

	private bool _canCancelBoarding;

	public override void OnMissionScreenInitialize()
	{
		((MissionGauntletAgentStatus)this).OnMissionScreenInitialize();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Combine(Input.OnGamepadActiveStateChanged, new Action(RefreshTexts));
		RefreshTexts();
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionGauntletAgentStatus)this).OnMissionScreenFinalize();
		Input.OnGamepadActiveStateChanged = (Action)Delegate.Remove(Input.OnGamepadActiveStateChanged, new Action(RefreshTexts));
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionGauntletAgentStatus)this).OnMissionScreenTick(dt);
		base._dataSource.IsAgentStatusPrioritized = _navalShipsLogic?.PlayerControlledShip == null;
	}

	private void RefreshTexts()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		_selectShipText = GameTexts.FindText("str_key_action", (string)null).SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 113), 1f)).SetTextVariable("ACTION", new TextObject("{=QVlyuUu6}Select Ship", (Dictionary<string, object>)null));
		_attemptBoardingText = GameTexts.FindText("str_key_action", (string)null).SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 114), 1f)).SetTextVariable("ACTION", new TextObject("{=DJA4aQ8n}Attempt Boarding", (Dictionary<string, object>)null));
		_cancelBoardingText = GameTexts.FindText("str_key_action", (string)null).SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("NavalShipControlsHotKeyCategory", 114), 1f)).SetTextVariable("ACTION", new TextObject("{=0bSBXtCi}Cancel Boarding", (Dictionary<string, object>)null));
		SetShipInteractionTexts();
	}

	public void UpdateShipInteractionTexts(IShipOrigin origin, bool isEnemy = false, bool canSelectShip = false, bool canAttemptBoarding = false, bool isBoardingBlocked = false, bool canCancelBoarding = false)
	{
		if (origin != _focusedShipOrigin || isEnemy != _focusedShipIsEnemy || canSelectShip != _canSelectShip || canAttemptBoarding != _canAttemptBoarding || isBoardingBlocked != _isBoardingBlocked || canCancelBoarding != _canCancelBoarding)
		{
			_focusedShipOrigin = origin;
			_focusedShipIsEnemy = isEnemy;
			_canSelectShip = canSelectShip;
			_canAttemptBoarding = canAttemptBoarding;
			_isBoardingBlocked = isBoardingBlocked;
			_canCancelBoarding = canCancelBoarding;
			SetShipInteractionTexts();
		}
	}

	private void SetShipInteractionTexts()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base._dataSource.InteractionInterface.ClearForcedInteractionTexts();
		if (_focusedShipOrigin != null)
		{
			TextObject val = (_focusedShipIsEnemy ? new TextObject("{=PFqAEWSt}Enemy {SHIP_NAME}", (Dictionary<string, object>)null).SetTextVariable("SHIP_NAME", _focusedShipOrigin.Hull.Name) : _focusedShipOrigin.Name);
			TextObject val2 = null;
			bool flag = false;
			if (_canSelectShip)
			{
				val2 = _selectShipText;
			}
			else if (_canAttemptBoarding)
			{
				if (_canCancelBoarding)
				{
					val2 = _cancelBoardingText;
				}
				else
				{
					val2 = _attemptBoardingText;
					flag = _isBoardingBlocked;
				}
			}
			base._dataSource.InteractionInterface.SetForcedInteractionTexts(val, false, val2, flag);
		}
		else if (_navalShipsLogic?.PlayerControlledShip != null)
		{
			base._dataSource.InteractionInterface.SetForcedInteractionTexts(TextObject.GetEmpty(), false, TextObject.GetEmpty(), false);
		}
	}
}
