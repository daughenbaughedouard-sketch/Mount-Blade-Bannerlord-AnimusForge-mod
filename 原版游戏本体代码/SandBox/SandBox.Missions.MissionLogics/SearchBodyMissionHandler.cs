using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionLogics;

public class SearchBodyMissionHandler : MissionLogic
{
	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode != 1)
		{
			return;
		}
		if (Game.Current.GameStateManager.ActiveState is MissionState)
		{
			if ((int)((MissionBehavior)this).Mission.Mode != 1 && (int)((MissionBehavior)this).Mission.Mode != 2 && IsSearchable(agent))
			{
				AddItemsToPlayer(agent);
			}
		}
		else
		{
			Debug.FailedAssert("Agent interaction must occur in MissionState.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\SearchBodyMissionHandler.cs", "OnAgentInteraction", 26);
		}
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		if ((int)Mission.Current.Mode != 2 && (int)((MissionBehavior)this).Mission.Mode != 3 && (int)((MissionBehavior)this).Mission.Mode != 1 && IsSearchable(otherAgent))
		{
			return true;
		}
		return false;
	}

	private bool IsSearchable(Agent agent)
	{
		if (!agent.IsActive() && agent.IsHuman && agent.Character.IsHero)
		{
			return true;
		}
		return false;
	}

	private void AddItemsToPlayer(Agent interactedAgent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		CharacterObject val = (CharacterObject)interactedAgent.Character;
		if (MBRandom.RandomInt(2) == 0)
		{
			((List<ItemObject>)(object)val.HeroObject.SpecialItems).Add(MBObjectManager.Instance.GetObject<ItemObject>("leafblade_throwing_knife"));
		}
		else
		{
			((List<ItemObject>)(object)val.HeroObject.SpecialItems).Add(MBObjectManager.Instance.GetObject<ItemObject>("falchion_sword_t2"));
			((List<ItemObject>)(object)val.HeroObject.SpecialItems).Add(MBObjectManager.Instance.GetObject<ItemObject>("cleaver_sword_t3"));
		}
		foreach (ItemObject item in (List<ItemObject>)(object)val.HeroObject.SpecialItems)
		{
			PartyBase.MainParty.ItemRoster.AddToCounts(item, 1);
			MBTextManager.SetTextVariable("ITEM_NAME", item.Name, false);
			InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_item_taken", (string)null)).ToString()));
		}
		((List<ItemObject>)(object)val.HeroObject.SpecialItems).Clear();
	}
}
