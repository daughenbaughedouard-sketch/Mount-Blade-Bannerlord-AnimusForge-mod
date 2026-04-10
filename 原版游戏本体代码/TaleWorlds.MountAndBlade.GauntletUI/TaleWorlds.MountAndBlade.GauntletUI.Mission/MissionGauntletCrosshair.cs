using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionCrosshair))]
public class MissionGauntletCrosshair : MissionBattleUIBaseView
{
	private GauntletLayer _layer;

	private CrosshairVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private double[] _targetGadgetOpacities = new double[4];

	protected override void OnCreateView()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Invalid comparison between Unknown and I4
		CombatLogManager.OnGenerateCombatLog += OnCombatLogGenerated;
		_dataSource = new CrosshairVM();
		_layer = new GauntletLayer("MissionCrosshair", 1, false);
		_movie = _layer.LoadMovie("Crosshair", (ViewModel)(object)_dataSource);
		if ((int)((MissionBehavior)this).Mission.Mode != 1 && (int)((MissionBehavior)this).Mission.Mode != 9)
		{
			((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_layer);
		}
	}

	protected override void OnDestroyView()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		CombatLogManager.OnGenerateCombatLog -= OnCombatLogGenerated;
		if ((int)((MissionBehavior)this).Mission.Mode != 1 && (int)((MissionBehavior)this).Mission.Mode != 9)
		{
			((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_layer);
		}
		_dataSource = null;
		_movie = null;
		_layer = null;
	}

	protected override void OnSuspendView()
	{
		if (_layer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, true);
		}
	}

	protected override void OnResumeView()
	{
		if (_layer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, false);
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Invalid comparison between Unknown and I4
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Invalid comparison between Unknown and I4
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Invalid comparison between Unknown and I4
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Expected I4, but got Unknown
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Invalid comparison between Unknown and I4
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Invalid comparison between Unknown and I4
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Expected I4, but got Unknown
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Expected I4, but got Unknown
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Invalid comparison between Unknown and I4
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Invalid comparison between Unknown and I4
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Invalid comparison between Unknown and I4
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Invalid comparison between Unknown and I4
		base.OnMissionScreenTick(dt);
		if (((MissionBehavior)this).DebugInput.IsKeyReleased((InputKey)63) && base.IsViewCreated)
		{
			OnDestroyView();
			OnCreateView();
		}
		if (!base.IsViewCreated)
		{
			return;
		}
		if (base.IsViewSuspended != ((ScreenLayer)_layer).IsActive)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layer, base.IsViewSuspended);
		}
		_dataSource.IsVisible = GetShouldCrosshairBeVisible();
		bool flag = true;
		bool isTargetInvalid = false;
		for (int i = 0; i < _targetGadgetOpacities.Length; i++)
		{
			_targetGadgetOpacities[i] = 0.0;
		}
		if (GetShouldArrowsBeVisible())
		{
			_dataSource.CrosshairType = BannerlordConfig.CrosshairType;
			Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
			double num = base.MissionScreen.CameraViewAngle * (MathF.PI / 180f);
			double num2 = 2.0 * Math.Tan((double)(mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5 / Math.Tan(num * 0.5)));
			_dataSource.SetProperties(num2, (double)(1f + (base.MissionScreen.CombatCamera.HorizontalFov - MathF.PI / 2f) / (MathF.PI / 2f)));
			WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo((HandIndex)0);
			Vec3 lookDirection = mainAgent.LookDirection;
			Vec2 val = ((Vec3)(ref lookDirection)).AsVec2;
			float rotationInRadians = ((Vec2)(ref val)).RotationInRadians;
			val = mainAgent.GetMovementDirection();
			float num3 = MBMath.WrapAngle(rotationInRadians - ((Vec2)(ref val)).RotationInRadians);
			if (((WeaponInfo)(ref wieldedWeaponInfo)).IsValid && ((WeaponInfo)(ref wieldedWeaponInfo)).IsRangedWeapon && BannerlordConfig.DisplayTargetingReticule)
			{
				ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
				MissionWeapon wieldedWeapon = mainAgent.WieldedWeapon;
				if (((MissionWeapon)(ref wieldedWeapon)).ReloadPhaseCount > 1 && ((MissionWeapon)(ref wieldedWeapon)).IsReloading && (int)currentActionType == 18)
				{
					StackArray10FloatFloatTuple reloadPhases = default(StackArray10FloatFloatTuple);
					ActionIndexCache itemUsageReloadActionCode = MBItem.GetItemUsageReloadActionCode(((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.ItemUsage, 9, mainAgent.HasMount, -1, mainAgent.GetIsLeftStance(), mainAgent.IsLookDirectionLow);
					FillReloadDurationsFromActions(ref reloadPhases, ((MissionWeapon)(ref wieldedWeapon)).ReloadPhaseCount, mainAgent, itemUsageReloadActionCode);
					float num4 = mainAgent.GetCurrentActionProgress(1);
					ActionIndexCache currentAction = mainAgent.GetCurrentAction(1);
					if (currentAction != ActionIndexCache.act_none)
					{
						float num5 = 1f - MBActionSet.GetActionBlendOutStartProgress(mainAgent.ActionSet, ref currentAction);
						num4 += num5;
					}
					float animationParameter = MBAnimation.GetAnimationParameter2(mainAgent.AgentVisuals.GetSkeleton().GetAnimationAtChannel(1));
					bool flag2 = num4 > animationParameter;
					float item = (flag2 ? 1f : (num4 / animationParameter));
					short reloadPhase = ((MissionWeapon)(ref wieldedWeapon)).ReloadPhase;
					for (int j = 0; j < reloadPhase; j++)
					{
						((StackArray10FloatFloatTuple)(ref reloadPhases))[j] = (1f, ((StackArray10FloatFloatTuple)(ref reloadPhases))[j].Item2);
					}
					if (!flag2)
					{
						((StackArray10FloatFloatTuple)(ref reloadPhases))[(int)reloadPhase] = (item, ((StackArray10FloatFloatTuple)(ref reloadPhases))[(int)reloadPhase].Item2);
						_dataSource.SetReloadProperties(ref reloadPhases, (int)((MissionWeapon)(ref wieldedWeapon)).ReloadPhaseCount);
					}
					flag = false;
				}
				if ((int)currentActionType == 15)
				{
					Vec2 bodyRotationConstraint = mainAgent.GetBodyRotationConstraint(1);
					isTargetInvalid = ((MissionBehavior)this).Mission.MainAgent.MountAgent != null && !MBMath.IsBetween(num3, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f);
				}
			}
			else if (!((WeaponInfo)(ref wieldedWeaponInfo)).IsValid || ((WeaponInfo)(ref wieldedWeaponInfo)).IsMeleeWeapon)
			{
				ActionCodeType currentActionType2 = mainAgent.GetCurrentActionType(1);
				UsageDirection currentActionDirection = mainAgent.GetCurrentActionDirection(1);
				if (BannerlordConfig.DisplayAttackDirection && ((int)currentActionType2 == 19 || MBMath.IsBetween((int)currentActionType2, 1, 15)))
				{
					if ((int)currentActionType2 == 19)
					{
						UsageDirection attackDirection = mainAgent.AttackDirection;
						switch ((int)attackDirection)
						{
						case 0:
							_targetGadgetOpacities[0] = 0.7;
							break;
						case 3:
							_targetGadgetOpacities[1] = 0.7;
							break;
						case 1:
							_targetGadgetOpacities[2] = 0.7;
							break;
						case 2:
							_targetGadgetOpacities[3] = 0.7;
							break;
						}
					}
					else
					{
						isTargetInvalid = true;
						switch (currentActionDirection - 4)
						{
						case 0:
							_targetGadgetOpacities[0] = 0.7;
							break;
						case 3:
							_targetGadgetOpacities[1] = 0.7;
							break;
						case 1:
							_targetGadgetOpacities[2] = 0.7;
							break;
						case 2:
							_targetGadgetOpacities[3] = 0.7;
							break;
						}
					}
				}
				else if (BannerlordConfig.DisplayAttackDirection)
				{
					UsageDirection val2 = mainAgent.PlayerAttackDirection();
					if ((int)val2 >= 0 && (int)val2 < 4)
					{
						if ((int)val2 == 0)
						{
							_targetGadgetOpacities[0] = 0.7;
						}
						else if ((int)val2 == 3)
						{
							_targetGadgetOpacities[1] = 0.7;
						}
						else if ((int)val2 == 1)
						{
							_targetGadgetOpacities[2] = 0.7;
						}
						else if ((int)val2 == 2)
						{
							_targetGadgetOpacities[3] = 0.7;
						}
					}
				}
			}
		}
		if (flag)
		{
			StackArray10FloatFloatTuple val3 = default(StackArray10FloatFloatTuple);
			_dataSource.SetReloadProperties(ref val3, 0);
		}
		_dataSource.SetArrowProperties(_targetGadgetOpacities[0], _targetGadgetOpacities[1], _targetGadgetOpacities[2], _targetGadgetOpacities[3]);
		_dataSource.IsTargetInvalid = isTargetInvalid;
	}

	protected virtual bool GetShouldArrowsBeVisible()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		if (!base.IsViewSuspended && ((MissionBehavior)this).Mission.MainAgent != null && (int)((MissionBehavior)this).Mission.Mode != 1 && (int)((MissionBehavior)this).Mission.Mode != 9 && (int)((MissionBehavior)this).Mission.Mode != 6 && !base.MissionScreen.IsViewingCharacter() && !IsMissionScreenUsingCustomCamera())
		{
			return !ScreenManager.GetMouseVisibility();
		}
		return false;
	}

	protected virtual bool GetShouldCrosshairBeVisible()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (GetShouldArrowsBeVisible() && BannerlordConfig.DisplayTargetingReticule)
		{
			MissionWeapon wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
			if (!((MissionWeapon)(ref wieldedWeapon)).IsEmpty)
			{
				wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
				if (((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.IsRangedWeapon)
				{
					wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
					if ((int)((MissionWeapon)(ref wieldedWeapon)).CurrentUsageItem.WeaponClass == 17)
					{
						wieldedWeapon = ((MissionBehavior)this).Mission.MainAgent.WieldedWeapon;
						return !((MissionWeapon)(ref wieldedWeapon)).IsReloading;
					}
					return true;
				}
			}
		}
		return false;
	}

	private bool IsMissionScreenUsingCustomCamera()
	{
		return (NativeObject)(object)base.MissionScreen.CustomCamera != (NativeObject)null;
	}

	private void OnCombatLogGenerated(CombatLogData logData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		bool isAttackerAgentMine = logData.IsAttackerAgentMine;
		bool flag = !logData.IsVictimAgentSameAsAttackerAgent && !logData.IsFriendlyFire;
		bool flag2 = logData.IsAttackerAgentHuman && (int)logData.BodyPartHit == 0;
		if (isAttackerAgentMine && flag && ((CombatLogData)(ref logData)).TotalDamage > 0)
		{
			CrosshairVM dataSource = _dataSource;
			if (dataSource != null)
			{
				dataSource.ShowHitMarker(logData.IsFatalDamage, flag2);
			}
		}
	}

	private void FillReloadDurationsFromActions(ref StackArray10FloatFloatTuple reloadPhases, int reloadPhaseCount, Agent mainAgent, ActionIndexCache reloadAction)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < reloadPhaseCount; i++)
		{
			if (reloadAction != ActionIndexCache.act_none)
			{
				float num2 = MBAnimation.GetAnimationParameter2(MBActionSet.GetAnimationIndexOfAction(mainAgent.ActionSet, ref reloadAction)) * MBActionSet.GetActionAnimationDuration(mainAgent.ActionSet, ref reloadAction);
				((StackArray10FloatFloatTuple)(ref reloadPhases))[i] = (((StackArray10FloatFloatTuple)(ref reloadPhases))[i].Item1, num2);
				if (num2 > num)
				{
					num = num2;
				}
				reloadAction = MBActionSet.GetActionAnimationContinueToAction(mainAgent.ActionSet, ref reloadAction);
			}
		}
		if (num > 1E-05f)
		{
			for (int j = 0; j < reloadPhaseCount; j++)
			{
				((StackArray10FloatFloatTuple)(ref reloadPhases))[j] = (((StackArray10FloatFloatTuple)(ref reloadPhases))[j].Item1, ((StackArray10FloatFloatTuple)(ref reloadPhases))[j].Item2 / num);
			}
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (base.IsViewCreated)
		{
			_layer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (base.IsViewCreated)
		{
			_layer.UIContext.ContextAlpha = 1f;
		}
	}
}
