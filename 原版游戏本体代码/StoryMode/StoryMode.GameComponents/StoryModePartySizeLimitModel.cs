using System;
using System.Collections.Generic;
using System.Linq;
using StoryMode.Quests.SecondPhase.ConspiracyQuests;
using StoryMode.Quests.ThirdPhase;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace StoryMode.GameComponents;

public class StoryModePartySizeLimitModel : PartySizeLimitModel
{
	private DefeatTheConspiracyQuestBehavior _defeatTheConspiracyQuestBehavior;

	public override int MinimumNumberOfVillagersAtVillagerParty => ((MBGameModel<PartySizeLimitModel>)this).BaseModel.MinimumNumberOfVillagersAtVillagerParty;

	private DefeatTheConspiracyQuestBehavior DefeatTheConspiracyQuestBehavior
	{
		get
		{
			if (_defeatTheConspiracyQuestBehavior != null)
			{
				return _defeatTheConspiracyQuestBehavior;
			}
			_defeatTheConspiracyQuestBehavior = Campaign.Current.GetCampaignBehavior<DefeatTheConspiracyQuestBehavior>();
			return _defeatTheConspiracyQuestBehavior;
		}
	}

	public override ExplainedNumber CalculateGarrisonPartySizeLimit(Settlement settlement, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.CalculateGarrisonPartySizeLimit(settlement, includeDescriptions);
	}

	public override TroopRoster FindAppropriateInitialRosterForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.FindAppropriateInitialRosterForMobileParty(party, partyTemplate);
	}

	public override List<Ship> FindAppropriateInitialShipsForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.FindAppropriateInitialShipsForMobileParty(party, partyTemplate);
	}

	public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
	}

	public override int GetClanTierPartySizeEffectForHero(Hero hero)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetClanTierPartySizeEffectForHero(hero);
	}

	public override int GetIdealVillagerPartySize(Village village)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetIdealVillagerPartySize(village);
	}

	public override int GetNextClanTierPartySizeEffectChangeForHero(Hero hero)
	{
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetNextClanTierPartySizeEffectChangeForHero(hero);
	}

	public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (party.IsMobile)
		{
			QuestBase val = ((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).FirstOrDefault((Func<QuestBase, bool>)((QuestBase q) => !q.IsFinalized && ((object)q).GetType() == typeof(DisruptSupplyLinesConspiracyQuest)));
			if (val != null)
			{
				MobileParty conspiracyCaravan = ((DisruptSupplyLinesConspiracyQuest)(object)val).ConspiracyCaravan;
				if (((conspiracyCaravan != null) ? conspiracyCaravan.Party : null) == party)
				{
					return new ExplainedNumber((float)((DisruptSupplyLinesConspiracyQuest)(object)val).CaravanPartySize, false, (TextObject)null);
				}
			}
			if (DefeatTheConspiracyQuestBehavior != null && DefeatTheConspiracyQuestBehavior.IsMobilePartyCreatedForQuest(party.MobileParty))
			{
				return new ExplainedNumber(600f, false, (TextObject)null);
			}
		}
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetPartyMemberSizeLimit(party, includeDescriptions);
	}

	public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<PartySizeLimitModel>)this).BaseModel.GetPartyPrisonerSizeLimit(party, includeDescriptions);
	}
}
