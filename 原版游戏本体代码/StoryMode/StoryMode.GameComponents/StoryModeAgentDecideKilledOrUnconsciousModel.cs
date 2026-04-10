using StoryMode.StoryModeObjects;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace StoryMode.GameComponents;

public class StoryModeAgentDecideKilledOrUnconsciousModel : AgentDecideKilledOrUnconsciousModel
{
	public override float GetAgentStateProbability(Agent affectorAgent, Agent effectedAgent, DamageTypes damageType, WeaponFlags weaponFlags, out float useSurgeryProbability)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		useSurgeryProbability = 1f;
		if (effectedAgent.Character.IsHero && ((object)effectedAgent.Character == StoryModeHeroes.ElderBrother.CharacterObject || (object)effectedAgent.Character == StoryModeHeroes.Radagos.CharacterObject || (object)effectedAgent.Character == StoryModeHeroes.RadagosHenchman.CharacterObject) && !StoryModeManager.Current.MainStoryLine.IsCompleted)
		{
			return 0f;
		}
		if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted && Mission.Current.GetMemberCountOfSide(effectedAgent.Team.Side) > 4)
		{
			return 0f;
		}
		return ((MBGameModel<AgentDecideKilledOrUnconsciousModel>)this).BaseModel.GetAgentStateProbability(affectorAgent, effectedAgent, damageType, weaponFlags, ref useSurgeryProbability);
	}
}
