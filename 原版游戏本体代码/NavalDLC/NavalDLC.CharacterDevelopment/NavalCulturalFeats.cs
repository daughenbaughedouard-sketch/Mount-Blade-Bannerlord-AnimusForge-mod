using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace NavalDLC.CharacterDevelopment;

public class NavalCulturalFeats
{
	private FeatObject _nordHostileActionBonusLootFeat;

	private FeatObject _nordHostileActionSpeedFeat;

	private FeatObject _nordShipMovementFeat;

	private FeatObject _nordArmyCohesionFeat;

	public static NavalCulturalFeats Instance => NavalDLCManager.Instance.NavalCulturalFeats;

	public static FeatObject NordHostileActionBonusFeat => Instance._nordHostileActionBonusLootFeat;

	public static FeatObject NordHostileActionSpeedFeat => Instance._nordHostileActionSpeedFeat;

	public static FeatObject NordShipMovementFeat => Instance._nordShipMovementFeat;

	public static FeatObject NordArmyCohesionFeat => Instance._nordArmyCohesionFeat;

	public NavalCulturalFeats()
	{
		RegisterAll();
		InitializeAll();
	}

	private void RegisterAll()
	{
		_nordHostileActionBonusLootFeat = Create("nord_hostile_action_bonus");
		_nordHostileActionSpeedFeat = Create("nord_hostile_action_speed");
		_nordShipMovementFeat = Create("nord_ship_movemenet_increase");
		_nordArmyCohesionFeat = Create("nord_decreased_cohesion_rate");
	}

	private FeatObject Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<FeatObject>(new FeatObject(stringId));
	}

	private void InitializeAll()
	{
		_nordHostileActionSpeedFeat.Initialize("{=!}nord_hostile_action_speed", "{=eI8zKXld}20% raid speed bonus while raiding.", 0.2f, true, (AdditionType)1);
		_nordHostileActionBonusLootFeat.Initialize("{=!}nord_hostile_action_bonus", "{=hUSnaX6O}+30% more loot from villages, villagers and caravans.", 0.3f, true, (AdditionType)1);
		_nordShipMovementFeat.Initialize("{=!}nord_ship_movemenet_increase", "{=bEw6FNpM}20% ship movement speed in rivers and coastal seas.", 0.1f, true, (AdditionType)1);
		_nordArmyCohesionFeat.Initialize("{=!}nord_decreased_cohesion_rate", "{=AnanB4d6}Armies that are commanded by a Nord commander lose 30% more cohesion on land.", -0.3f, false, (AdditionType)1);
	}
}
