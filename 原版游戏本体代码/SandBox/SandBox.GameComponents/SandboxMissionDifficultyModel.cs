using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxMissionDifficultyModel : MissionDifficultyModel
{
	public override float GetDamageMultiplierOfCombatDifficulty(Agent victimAgent, Agent attackerAgent = null)
	{
		float result = 1f;
		victimAgent = (victimAgent.IsMount ? victimAgent.RiderAgent : victimAgent);
		if (victimAgent != null)
		{
			if (victimAgent.IsMainAgent)
			{
				result = Mission.Current.DamageToPlayerMultiplier;
			}
			else
			{
				IAgentOriginBase origin = victimAgent.Origin;
				IBattleCombatant obj = ((origin != null) ? origin.BattleCombatant : null);
				PartyBase val;
				if ((val = (PartyBase)(object)((obj is PartyBase) ? obj : null)) != null)
				{
					Mission current = Mission.Current;
					object obj2;
					if (current == null)
					{
						obj2 = null;
					}
					else
					{
						Agent mainAgent = current.MainAgent;
						if (mainAgent == null)
						{
							obj2 = null;
						}
						else
						{
							IAgentOriginBase origin2 = mainAgent.Origin;
							obj2 = ((origin2 != null) ? origin2.BattleCombatant : null);
						}
					}
					PartyBase val2;
					if ((val2 = (PartyBase)((obj2 is PartyBase) ? obj2 : null)) != null && val == val2)
					{
						if (attackerAgent != null)
						{
							Mission current2 = Mission.Current;
							if (attackerAgent == ((current2 != null) ? current2.MainAgent : null))
							{
								result = Mission.Current.DamageFromPlayerToFriendsMultiplier;
								goto IL_00b7;
							}
						}
						result = Mission.Current.DamageToFriendsMultiplier;
					}
				}
			}
		}
		goto IL_00b7;
		IL_00b7:
		return result;
	}
}
