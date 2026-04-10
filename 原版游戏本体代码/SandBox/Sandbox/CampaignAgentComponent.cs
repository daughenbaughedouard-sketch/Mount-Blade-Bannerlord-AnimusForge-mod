using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.MountAndBlade;

namespace SandBox;

public class CampaignAgentComponent : AgentComponent
{
	public AgentNavigator AgentNavigator { get; private set; }

	public PartyBase OwnerParty
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			IAgentOriginBase origin = base.Agent.Origin;
			return (PartyBase)((origin != null) ? origin.BattleCombatant : null);
		}
	}

	public CampaignAgentComponent(Agent agent)
		: base(agent)
	{
	}

	public AgentNavigator CreateAgentNavigator(LocationCharacter locationCharacter)
	{
		AgentNavigator = new AgentNavigator(base.Agent, locationCharacter);
		return AgentNavigator;
	}

	public AgentNavigator CreateAgentNavigator()
	{
		AgentNavigator = new AgentNavigator(base.Agent);
		return AgentNavigator;
	}

	public void OnAgentRemoved(Agent agent)
	{
		AgentNavigator?.OnAgentRemoved(agent);
	}

	public override void OnTick(float dt)
	{
		if (base.Agent.Mission.AllowAiTicking && base.Agent.IsAIControlled)
		{
			AgentNavigator?.Tick(dt);
		}
	}

	public override float GetMoraleDecreaseConstant()
	{
		PartyBase ownerParty = OwnerParty;
		if (((ownerParty != null) ? ownerParty.MapEvent : null) == null || !OwnerParty.MapEvent.IsSiegeAssault)
		{
			return 1f;
		}
		if (LinQuick.FindIndexQ<MapEventParty>((List<MapEventParty>)(object)OwnerParty.MapEvent.AttackerSide.Parties, (Func<MapEventParty, bool>)((MapEventParty p) => p.Party == OwnerParty)) < 0)
		{
			return 0.5f;
		}
		return 0.33f;
	}

	public override float GetMoraleAddition()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		PartyBase ownerParty = OwnerParty;
		if (((ownerParty != null) ? ownerParty.MapEvent : null) != null)
		{
			float num2 = default(float);
			float num3 = default(float);
			OwnerParty.MapEvent.GetStrengthsRelativeToParty(OwnerParty.Side, ref num2, ref num3);
			if (OwnerParty.IsMobile)
			{
				float num4 = (OwnerParty.MobileParty.Morale - 50f) / 2f;
				num += num4;
			}
			float num5 = num2 / (num2 + num3) * 10f - 5f;
			num += num5;
		}
		return num;
	}

	public override void OnStopUsingGameObject()
	{
		if (base.Agent.IsAIControlled)
		{
			AgentNavigator?.OnStopUsingGameObject();
		}
	}
}
