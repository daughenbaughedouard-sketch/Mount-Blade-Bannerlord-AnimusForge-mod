using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;

namespace NavalDLC.GameComponents;

public class NavalDLCMilitaryPowerModel : MilitaryPowerModel
{
	private const float MarinerTroopSeaBattlePowerBonus = 1.2f;

	private static readonly Dictionary<PowerCalculationContext, float> _lightShipAttackerModifiers = new Dictionary<PowerCalculationContext, float>
	{
		{
			(PowerCalculationContext)9,
			0.2f
		},
		{
			(PowerCalculationContext)10,
			-0.2f
		},
		{
			(PowerCalculationContext)11,
			0.2f
		}
	};

	private static readonly Dictionary<PowerCalculationContext, float> _lightShipDefenderModifiers = new Dictionary<PowerCalculationContext, float>
	{
		{
			(PowerCalculationContext)9,
			0.2f
		},
		{
			(PowerCalculationContext)10,
			-0.2f
		},
		{
			(PowerCalculationContext)11,
			0.2f
		}
	};

	private static readonly Dictionary<PowerCalculationContext, float> _mediumShipAttackerModifiers = new Dictionary<PowerCalculationContext, float>
	{
		{
			(PowerCalculationContext)9,
			0f
		},
		{
			(PowerCalculationContext)10,
			0f
		},
		{
			(PowerCalculationContext)11,
			0f
		}
	};

	private static readonly Dictionary<PowerCalculationContext, float> _mediumShipDefenderModifiers = new Dictionary<PowerCalculationContext, float>
	{
		{
			(PowerCalculationContext)9,
			0f
		},
		{
			(PowerCalculationContext)10,
			0f
		},
		{
			(PowerCalculationContext)11,
			0f
		}
	};

	private static readonly Dictionary<PowerCalculationContext, float> _heavyShipAttackerModifiers = new Dictionary<PowerCalculationContext, float>
	{
		{
			(PowerCalculationContext)9,
			-0.2f
		},
		{
			(PowerCalculationContext)10,
			0.2f
		},
		{
			(PowerCalculationContext)11,
			-0.2f
		}
	};

	private static readonly Dictionary<PowerCalculationContext, float> _heavyShipDefenderModifiers = new Dictionary<PowerCalculationContext, float>
	{
		{
			(PowerCalculationContext)9,
			-0.2f
		},
		{
			(PowerCalculationContext)10,
			0.2f
		},
		{
			(PowerCalculationContext)11,
			-0.2f
		}
	};

	public override float GetPowerOfParty(PartyBase party, BattleSideEnum side, PowerCalculationContext context)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		float num = ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetPowerOfParty(party, side, context);
		if ((int)context == 9 || (int)context == 10 || (int)context == 11)
		{
			if (((List<Ship>)(object)party.Ships).Count == 0)
			{
				return 0f;
			}
			float num2 = LinQuick.AverageQ<Ship>((List<Ship>)(object)party.Ships, (Func<Ship, float>)((Ship x) => x.GetCombatFactor()));
			num *= num2;
			num *= GetTroopAccommodationRatio(party);
		}
		else if ((int)context == 12 && party.IsMobile && party.MobileParty.IsCurrentlyAtSea)
		{
			num *= GetTroopAccommodationRatio(party);
		}
		return num;
	}

	public override float GetContextModifier(CharacterObject troop, BattleSideEnum battleSideEnum, PowerCalculationContext context)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)context == 9 || (int)context == 10 || (int)context == 11)
		{
			return 0f;
		}
		return ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetContextModifier(troop, battleSideEnum, context);
	}

	public override float GetContextModifier(Ship ship, BattleSideEnum battleSide, PowerCalculationContext context)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected I4, but got Unknown
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)context == 9 || (int)context == 10 || (int)context == 11)
		{
			ShipType type = ship.ShipHull.Type;
			switch ((int)type)
			{
			case 0:
				return GetLightShipContextModifier(ship, battleSide, context);
			case 1:
				return GetMediumShipContextModifier(ship, battleSide, context);
			case 2:
				return GetHeavyShipContextModifier(ship, battleSide, context);
			}
			Debug.FailedAssert("unhandled ship type", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\GameComponents\\NavalDLCMilitaryPowerModel.cs", "GetContextModifier", 118);
		}
		return ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetContextModifier(ship, battleSide, context);
	}

	public override PowerCalculationContext GetContextForPosition(CampaignVec2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetContextForPosition(position);
	}

	public override float GetDefaultTroopPower(CharacterObject troop)
	{
		return ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetDefaultTroopPower(troop);
	}

	public override float GetPowerModifierOfHero(Hero leaderHero)
	{
		return ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetPowerModifierOfHero(leaderHero);
	}

	public override float GetTroopPower(CharacterObject troop, BattleSideEnum side, PowerCalculationContext context, float leaderModifier)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		float num = ((MBGameModel<MilitaryPowerModel>)this).BaseModel.GetTroopPower(troop, side, context, leaderModifier);
		if ((int)context == 9 && !((BasicCharacterObject)troop).IsHero && troop.IsMariner)
		{
			num *= 1.2f;
		}
		return num;
	}

	private float GetLightShipContextModifier(Ship ship, BattleSideEnum battleSide, PowerCalculationContext context)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)battleSide != 1)
		{
			return _lightShipDefenderModifiers[context];
		}
		return _lightShipAttackerModifiers[context];
	}

	private float GetMediumShipContextModifier(Ship ship, BattleSideEnum battleSide, PowerCalculationContext context)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)battleSide != 1)
		{
			return _mediumShipDefenderModifiers[context];
		}
		return _mediumShipAttackerModifiers[context];
	}

	private float GetHeavyShipContextModifier(Ship ship, BattleSideEnum battleSide, PowerCalculationContext context)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)battleSide != 1)
		{
			return _heavyShipDefenderModifiers[context];
		}
		return _heavyShipAttackerModifiers[context];
	}

	private float GetTroopAccommodationRatio(PartyBase party)
	{
		float result = 1f;
		float num = LinQuick.SumQ<Ship>((List<Ship>)(object)party.Ships, (Func<Ship, int>)((Ship x) => x.TotalCrewCapacity));
		if ((float)party.NumberOfAllMembers > num)
		{
			result = num / (float)party.NumberOfAllMembers;
		}
		return result;
	}
}
