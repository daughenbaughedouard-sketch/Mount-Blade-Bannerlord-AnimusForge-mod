using Helpers;
using NavalDLC.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.GameComponents;

public class NavalCustomBattleMissionShipParametersModel : MissionShipParametersModel
{
	public override int CalculateMainDeckCrewSize(IShipOrigin shipOrigin, Agent formationUnit)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector((float)shipOrigin.MainDeckCrewCapacity, false, (TextObject)null);
		return MathF.Min(MathF.Ceiling(((ExplainedNumber)(ref val)).ResultNumber), shipOrigin.TotalCrewCapacity);
	}

	public override float CalculateWindBonus(IShipOrigin shipOrigin, Agent captain, float baseSailForceMagnitude)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseSailForceMagnitude, false, (TextObject)null);
		if (captain != null)
		{
			BasicCharacterObject character = captain.Character;
			CharacterObject val2 = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			if (val2 != null)
			{
				int skillValue = ((BasicCharacterObject)val2).GetSkillValue(NavalSkills.Shipmaster);
				SkillHelper.AddSkillBonusForSkillLevel(NavalSkillEffects.WindBonus, ref val, skillValue);
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Shipmaster.Windborne, val2, ref val);
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override float CalculateOarForceMultiplier(Agent pilotAgent, float baseOarForceMultiplier)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(baseOarForceMultiplier, false, (TextObject)null);
		((ExplainedNumber)(ref val)).LimitMin(0f);
		object obj;
		if (pilotAgent == null)
		{
			obj = null;
		}
		else
		{
			Formation formation = pilotAgent.Formation;
			obj = ((formation != null) ? formation.Captain : null);
		}
		Agent val2 = (Agent)obj;
		if (val2 != null)
		{
			BasicCharacterObject character = val2.Character;
			CharacterObject val3 = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			if (val3 != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Shipmaster.ChainToOars, val3, ref val);
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}
}
