using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionLogics;

public class MountAgentLogic : MissionLogic
{
	private Agent _mainHeroMountAgent;

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		if (agent.IsMainAgent && agent.HasMount)
		{
			_mainHeroMountAgent = agent.MountAgent;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (!affectedAgent.IsMount || (int)agentState != 4 || _mainHeroMountAgent != affectedAgent)
		{
			return;
		}
		Equipment val = Hero.MainHero.BattleEquipment;
		if (Mission.Current.DoesMissionRequireCivilianEquipment)
		{
			val = Hero.MainHero.CivilianEquipment;
		}
		float randomFloat = MBRandom.RandomFloat;
		float num = 0.05f;
		float num2 = 0.2f;
		if (Hero.MainHero.GetPerkValue(Riding.WellStraped))
		{
			float primaryBonus = Riding.WellStraped.PrimaryBonus;
			num += num * primaryBonus;
			num2 += num2 * primaryBonus;
		}
		bool flag = randomFloat < num2;
		if (!(randomFloat < num))
		{
			EquipmentElement val2 = val[(EquipmentIndex)10];
			ItemModifier itemModifier = ((EquipmentElement)(ref val2)).ItemModifier;
			if (!(((itemModifier != null) ? ((MBObjectBase)itemModifier).StringId : null) == "lame_horse"))
			{
				if (flag)
				{
					ItemModifier val3 = MBObjectManager.Instance.GetObject<ItemModifier>("lame_horse");
					Equipment obj = val;
					val2 = val[(EquipmentIndex)10];
					obj[(EquipmentIndex)10] = new EquipmentElement(((EquipmentElement)(ref val2)).Item, val3, (ItemObject)null, false);
					TextObject val4 = new TextObject("{=a6hwSEAK}Your horse is turned into a {MODIFIED_NAME}, since it got seriously injured.", (Dictionary<string, object>)null);
					val2 = val[10];
					val4.SetTextVariable("MODIFIED_NAME", ((EquipmentElement)(ref val2)).GetModifiedItemName());
					MBInformationManager.AddQuickInformation(val4, 0, (BasicCharacterObject)null, (Equipment)null, "");
				}
				return;
			}
		}
		val[(EquipmentIndex)10] = EquipmentElement.Invalid;
		EquipmentElement val5 = val[(EquipmentIndex)11];
		val[(EquipmentIndex)11] = EquipmentElement.Invalid;
		if (!((EquipmentElement)(ref val5)).IsInvalid() && !((EquipmentElement)(ref val5)).IsEmpty)
		{
			MobileParty.MainParty.ItemRoster.AddToCounts(val5, 1);
		}
		MBInformationManager.AddQuickInformation(new TextObject("{=nZhPS83J}You lost your horse.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
	}
}
