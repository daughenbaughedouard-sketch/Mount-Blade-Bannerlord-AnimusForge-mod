using Helpers;
using NavalDLC.CharacterDevelopment;
using NavalDLC.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace NavalDLC.GameComponents;

public class NavalMissionSiegeEngineCalculationModel : MissionSiegeEngineCalculationModel
{
	public override float CalculateReloadSpeed(Agent userAgent, float baseSpeed)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		float num = ((MBGameModel<MissionSiegeEngineCalculationModel>)this).BaseModel.CalculateReloadSpeed(userAgent, baseSpeed);
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector(num, false, (TextObject)null);
		if (Mission.Current.IsNavalBattle)
		{
			object obj;
			if (userAgent == null)
			{
				obj = null;
			}
			else
			{
				Formation formation = userAgent.Formation;
				if (formation == null)
				{
					obj = null;
				}
				else
				{
					Agent captain = formation.Captain;
					obj = ((captain != null) ? captain.Character : null);
				}
			}
			CharacterObject val2 = (CharacterObject)obj;
			if ((object)((userAgent != null) ? userAgent.Character : null) == val2)
			{
				val2 = null;
			}
			if (val2 != null)
			{
				PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Boatswain.StreamlinedOperations, val2, ref val);
			}
			AgentNavalComponent agentNavalComponent = ((userAgent != null) ? userAgent.GetComponent<AgentNavalComponent>() : null);
			if (agentNavalComponent != null && agentNavalComponent.SteppedShip != null)
			{
				IShipOrigin shipOrigin = agentNavalComponent.SteppedShip.ShipOrigin;
				Figurehead figurehead = ((Ship)((shipOrigin is Ship) ? shipOrigin : null)).Figurehead;
				if (figurehead != null && figurehead == DefaultFigureheads.Viper)
				{
					((ExplainedNumber)(ref val)).AddFactor(figurehead.EffectAmount, (TextObject)null);
				}
			}
		}
		return ((ExplainedNumber)(ref val)).ResultNumber;
	}

	public override int CalculateShipSiegeWeaponAmmoCount(IShipOrigin shipOrigin, Agent captain, RangedSiegeWeapon weapon)
	{
		ExplainedNumber val = default(ExplainedNumber);
		((ExplainedNumber)(ref val))._002Ector((float)weapon.AmmoCount, false, (TextObject)null);
		BasicCharacterObject obj = ((captain != null) ? captain.Character : null);
		CharacterObject val2 = (CharacterObject)(object)((obj is CharacterObject) ? obj : null);
		if (val2 != null && weapon is Ballista)
		{
			PerkHelper.AddPerkBonusFromCaptain(NavalPerks.Boatswain.SmoothOperator, val2, ref val);
		}
		return MathF.Ceiling(((ExplainedNumber)(ref val)).ResultNumber);
	}

	public override int CalculateDamage(Agent attackerAgent, float baseDamage)
	{
		int num = ((MBGameModel<MissionSiegeEngineCalculationModel>)this).BaseModel.CalculateDamage(attackerAgent, baseDamage);
		Formation formation = attackerAgent.Formation;
		object obj;
		if (formation == null)
		{
			obj = null;
		}
		else
		{
			Agent captain = formation.Captain;
			obj = ((captain != null) ? captain.Character : null);
		}
		CharacterObject val = (CharacterObject)((obj is CharacterObject) ? obj : null);
		ExplainedNumber val2 = default(ExplainedNumber);
		((ExplainedNumber)(ref val2))._002Ector((float)num, false, (TextObject)null);
		if (val != null)
		{
			if ((object)((attackerAgent != null) ? attackerAgent.Character : null) == val)
			{
				val = null;
			}
			if (val != null && val.GetPerkValue(NavalPerks.Boatswain.ShipwrightsInsight))
			{
				((ExplainedNumber)(ref val2)).AddFactor(NavalPerks.Boatswain.ShipwrightsInsight.PrimaryBonus, (TextObject)null);
			}
		}
		return MBMath.ClampInt(MathF.Ceiling(((ExplainedNumber)(ref val2)).ResultNumber), 0, 2000);
	}
}
