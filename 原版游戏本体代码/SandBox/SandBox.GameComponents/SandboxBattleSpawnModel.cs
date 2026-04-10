using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace SandBox.GameComponents;

public class SandboxBattleSpawnModel : BattleSpawnModel
{
	private enum OrderOfBattleInnerClassType
	{
		None,
		PrimaryClass,
		SecondaryClass
	}

	private struct FormationOrderOfBattleConfiguration
	{
		public DeploymentFormationClass OOBFormationClass;

		public FormationClass PrimaryFormationClass;

		public int PrimaryClassTroopCount;

		public int PrimaryClassDesiredTroopCount;

		public FormationClass SecondaryFormationClass;

		public int SecondaryClassTroopCount;

		public int SecondaryClassDesiredTroopCount;

		public Hero Captain;
	}

	public override void OnMissionStart()
	{
		MissionReinforcementsHelper.OnMissionStart();
	}

	public override void OnMissionEnd()
	{
		MissionReinforcementsHelper.OnMissionEnd();
	}

	public override List<(IAgentOriginBase origin, int formationIndex)> GetInitialSpawnAssignments(BattleSideEnum battleSide, List<IAgentOriginBase> troopOrigins)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected I4, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected I4, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		List<(IAgentOriginBase, int)> list = new List<(IAgentOriginBase, int)>();
		if (GetOrderOfBattleConfigurationsForFormations(battleSide, troopOrigins, out var formationOrderOfBattleConfigurations))
		{
			foreach (IAgentOriginBase troopOrigin in troopOrigins)
			{
				OrderOfBattleInnerClassType bestClassInnerClassType;
				FormationClass val = FindBestOrderOfBattleFormationClassAssignmentForTroop(battleSide, troopOrigin, formationOrderOfBattleConfigurations, out bestClassInnerClassType);
				(IAgentOriginBase, int) item = (troopOrigin, (int)val);
				list.Add(item);
				switch (bestClassInnerClassType)
				{
				case OrderOfBattleInnerClassType.PrimaryClass:
					formationOrderOfBattleConfigurations[val].PrimaryClassTroopCount++;
					break;
				case OrderOfBattleInnerClassType.SecondaryClass:
					formationOrderOfBattleConfigurations[val].SecondaryClassTroopCount++;
					break;
				}
			}
		}
		else
		{
			foreach (IAgentOriginBase troopOrigin2 in troopOrigins)
			{
				(IAgentOriginBase, int) item2 = (troopOrigin2, (int)Mission.Current.GetAgentTroopClass(battleSide, troopOrigin2.Troop));
				list.Add(item2);
			}
		}
		return list;
	}

	public override List<(IAgentOriginBase origin, int formationIndex)> GetReinforcementAssignments(BattleSideEnum battleSide, List<IAgentOriginBase> troopOrigins)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return MissionReinforcementsHelper.GetReinforcementAssignments(battleSide, troopOrigins);
	}

	private static bool GetOrderOfBattleConfigurationsForFormations(BattleSideEnum battleSide, List<IAgentOriginBase> troopOrigins, out FormationOrderOfBattleConfiguration[] formationOrderOfBattleConfigurations)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected I4, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Invalid comparison between Unknown and I4
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Invalid comparison between Unknown and I4
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		formationOrderOfBattleConfigurations = new FormationOrderOfBattleConfiguration[8];
		Campaign current = Campaign.Current;
		OrderOfBattleCampaignBehavior val = ((current != null) ? current.GetCampaignBehavior<OrderOfBattleCampaignBehavior>() : null);
		if (val == null)
		{
			return false;
		}
		for (int i = 0; i < 8; i++)
		{
			int num = i;
			bool isSiegeBattle = Mission.Current.IsSiegeBattle;
			MobileParty mainParty = MobileParty.MainParty;
			if (val.GetFormationDataAtIndex(num, isSiegeBattle, ((mainParty != null) ? mainParty.Army : null) != null) == null)
			{
				return false;
			}
		}
		int[] array = CalculateTroopCountsPerDefaultFormation(battleSide, troopOrigins);
		for (int j = 0; j < 8; j++)
		{
			int num2 = j;
			bool isSiegeBattle2 = Mission.Current.IsSiegeBattle;
			MobileParty mainParty2 = MobileParty.MainParty;
			OrderOfBattleFormationData formationDataAtIndex = val.GetFormationDataAtIndex(num2, isSiegeBattle2, ((mainParty2 != null) ? mainParty2.Army : null) != null);
			formationOrderOfBattleConfigurations[j].OOBFormationClass = formationDataAtIndex.FormationClass;
			formationOrderOfBattleConfigurations[j].Captain = formationDataAtIndex.Captain;
			FormationClass val2 = (FormationClass)10;
			FormationClass val3 = (FormationClass)10;
			DeploymentFormationClass formationClass = formationDataAtIndex.FormationClass;
			switch (formationClass - 1)
			{
			case 0:
				val2 = (FormationClass)0;
				break;
			case 1:
				val2 = (FormationClass)1;
				break;
			case 2:
				val2 = (FormationClass)2;
				break;
			case 3:
				val2 = (FormationClass)3;
				break;
			case 4:
				val2 = (FormationClass)0;
				val3 = (FormationClass)1;
				break;
			case 5:
				val2 = (FormationClass)2;
				val3 = (FormationClass)3;
				break;
			}
			formationOrderOfBattleConfigurations[j].PrimaryFormationClass = val2;
			if ((int)val2 != 10)
			{
				formationOrderOfBattleConfigurations[j].PrimaryClassDesiredTroopCount = (int)Math.Ceiling((float)array[val2] * ((float)formationDataAtIndex.PrimaryClassWeight / 100f));
			}
			formationOrderOfBattleConfigurations[j].SecondaryFormationClass = val3;
			if ((int)val3 != 10)
			{
				formationOrderOfBattleConfigurations[j].SecondaryClassDesiredTroopCount = (int)Math.Ceiling((float)array[val3] * ((float)formationDataAtIndex.SecondaryClassWeight / 100f));
			}
		}
		return true;
	}

	private static int[] CalculateTroopCountsPerDefaultFormation(BattleSideEnum battleSide, List<IAgentOriginBase> troopOrigins)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		int[] array = new int[4];
		foreach (IAgentOriginBase troopOrigin in troopOrigins)
		{
			FormationClass val = TroopClassExtensions.DefaultClass(Mission.Current.GetAgentTroopClass(battleSide, troopOrigin.Troop));
			array[val]++;
		}
		return array;
	}

	private static FormationClass FindBestOrderOfBattleFormationClassAssignmentForTroop(BattleSideEnum battleSide, IAgentOriginBase origin, FormationOrderOfBattleConfiguration[] formationOrderOfBattleConfigurations, out OrderOfBattleInnerClassType bestClassInnerClassType)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		FormationClass val = TroopClassExtensions.DefaultClass(Mission.Current.GetAgentTroopClass(battleSide, origin.Troop));
		FormationClass result = val;
		float num = float.MinValue;
		bestClassInnerClassType = OrderOfBattleInnerClassType.None;
		for (int i = 0; i < 8; i++)
		{
			CharacterObject val2;
			if (origin.Troop.IsHero && (val2 = (CharacterObject)/*isinst with value type is only supported in some contexts*/) != null && val2.HeroObject == formationOrderOfBattleConfigurations[i].Captain)
			{
				result = (FormationClass)i;
				bestClassInnerClassType = OrderOfBattleInnerClassType.None;
				break;
			}
			if (val == formationOrderOfBattleConfigurations[i].PrimaryFormationClass)
			{
				float num2 = formationOrderOfBattleConfigurations[i].PrimaryClassDesiredTroopCount;
				float num3 = formationOrderOfBattleConfigurations[i].PrimaryClassTroopCount;
				float num4 = 1f - num3 / (num2 + 1f);
				if (num4 > num)
				{
					result = (FormationClass)i;
					bestClassInnerClassType = OrderOfBattleInnerClassType.PrimaryClass;
					num = num4;
				}
			}
			else if (val == formationOrderOfBattleConfigurations[i].SecondaryFormationClass)
			{
				float num5 = formationOrderOfBattleConfigurations[i].SecondaryClassDesiredTroopCount;
				float num6 = formationOrderOfBattleConfigurations[i].SecondaryClassTroopCount;
				float num7 = 1f - num6 / (num5 + 1f);
				if (num7 > num)
				{
					result = (FormationClass)i;
					bestClassInnerClassType = OrderOfBattleInnerClassType.SecondaryClass;
					num = num7;
				}
			}
		}
		return result;
	}
}
