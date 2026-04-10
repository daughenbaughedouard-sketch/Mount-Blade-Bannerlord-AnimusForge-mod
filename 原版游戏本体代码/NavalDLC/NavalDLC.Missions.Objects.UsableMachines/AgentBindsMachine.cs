using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects.UsableMachines;

public class AgentBindsMachine : UsableMachine
{
	private readonly ActionIndexCache _breakChainsShortAction = ActionIndexCache.Create("act_cutscene_break_chains_short");

	public ShipOarMachine ShipOarMachine { get; private set; }

	public bool HasCaptive => ((UsableMissionObject)((UsableMachine)ShipOarMachine).PilotStandingPoint).HasUser;

	public void SetOarMachine(ShipOarMachine shipOarMachine)
	{
		ShipOarMachine = shipOarMachine;
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		((UsableMachine)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).AddComponent((UsableMissionObjectComponent)new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none, false));
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnTick(float dt)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		ShipOarMachine shipOarMachine = ShipOarMachine;
		Agent val = ((shipOarMachine != null) ? ((UsableMachine)shipOarMachine).PilotAgent : null);
		((UsableMissionObject)((UsableMachine)this).PilotStandingPoint).SetIsDeactivatedSynched(val == null);
		if (((UsableMachine)this).PilotAgent == null)
		{
			return;
		}
		if (((UsableMachine)this).PilotAgent.SetActionChannel(0, ref _breakChainsShortAction, false, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
		{
			if (((UsableMachine)this).PilotAgent.GetCurrentActionProgress(0) > 0.99f)
			{
				((UsableMachine)this).PilotAgent.SetActionChannel(0, ref ActionIndexCache.act_none, true, (AnimFlags)0, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
				((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				if (val != null)
				{
					val.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
					val.ClearHandInverseKinematics();
				}
			}
		}
		else
		{
			((UsableMachine)this).PilotAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			val.MakeVoice(VoiceType.MpThanks, (CombatVoiceNetworkPredictionType)2);
		}
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		TextObject val = new TextObject("{=fEQAPJ2e}{KEY} Use", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=ut9C8hA9}Chains", (Dictionary<string, object>)null);
	}
}
