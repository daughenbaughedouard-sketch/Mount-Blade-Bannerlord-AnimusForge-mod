using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.GameComponents;

public class NavalDLCClanFinanceModel : ClanFinanceModel
{
	private const int payGarrisonWagesTreshold = 8000;

	private const int payClanPartiesTreshold = 4000;

	public override int PartyGoldLowerThreshold => ((MBGameModel<ClanFinanceModel>)this).BaseModel.PartyGoldLowerThreshold;

	public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals, includeDetails);
		if (clan.Kingdom != null && clan.Kingdom.HasPolicy(NavalPolicies.CoastalGuardEdict))
		{
			ExplainedNumber val2 = default(ExplainedNumber);
			((ExplainedNumber)(ref val2))._002Ector(0f, false, (TextObject)null);
			foreach (Town item in (List<Town>)(object)clan.Fiefs)
			{
				if (((SettlementComponent)item).Settlement.HasPort && ((Fief)item).GarrisonParty != null && ((Fief)item).GarrisonParty.IsActive)
				{
					int num = AddPartyExpense(((Fief)item).GarrisonParty, clan, val, applyWithdrawals);
					((ExplainedNumber)(ref val2)).Add((float)num, (TextObject)null, (TextObject)null);
				}
			}
			((ExplainedNumber)(ref val)).Add(((ExplainedNumber)(ref val2)).ResultNumber * -0.15f, ((PropertyObject)NavalPolicies.CoastalGuardEdict).Name, (TextObject)null);
		}
		return val;
	}

	public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals, includeDetails);
	}

	public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false, bool includeDetails = false)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber val = ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals, includeDetails);
		if (clan.Kingdom != null && clan.Kingdom.HasPolicy(NavalPolicies.CoastalGuardEdict))
		{
			ExplainedNumber val2 = default(ExplainedNumber);
			((ExplainedNumber)(ref val2))._002Ector(0f, false, (TextObject)null);
			foreach (Town item in (List<Town>)(object)clan.Fiefs)
			{
				if (((SettlementComponent)item).Settlement.HasPort && ((Fief)item).GarrisonParty != null && ((Fief)item).GarrisonParty.IsActive)
				{
					int num = AddPartyExpense(((Fief)item).GarrisonParty, clan, val, applyWithdrawals);
					((ExplainedNumber)(ref val2)).Add((float)num, (TextObject)null, (TextObject)null);
				}
			}
			((ExplainedNumber)(ref val)).Add(((ExplainedNumber)(ref val2)).ResultNumber * 0.15f, ((PropertyObject)NavalPolicies.CoastalGuardEdict).Name, (TextObject)null);
		}
		return val;
	}

	public override ExplainedNumber CalculateTownIncomeFromTariffs(Clan clan, Town town, bool applyWithdrawals = false)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		ExplainedNumber result = ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateTownIncomeFromTariffs(clan, town, applyWithdrawals);
		if (clan.Kingdom != null && clan.Kingdom.HasPolicy(NavalPolicies.ArsenalDepositoryAct))
		{
			((ExplainedNumber)(ref result)).AddFactor(-0.1f, ((PropertyObject)NavalPolicies.ArsenalDepositoryAct).Name);
		}
		return result;
	}

	public override int CalculateTownIncomeFromProjects(Town town)
	{
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateTownIncomeFromProjects(town);
	}

	public override int CalculateNotableDailyGoldChange(Hero hero, bool applyWithdrawals)
	{
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateNotableDailyGoldChange(hero, applyWithdrawals);
	}

	public override int CalculateVillageIncome(Clan clan, Village village, bool applyWithdrawals = false)
	{
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateVillageIncome(clan, village, applyWithdrawals);
	}

	public override int CalculateOwnerIncomeFromCaravan(MobileParty caravan)
	{
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateOwnerIncomeFromCaravan(caravan);
	}

	public override int CalculateOwnerIncomeFromWorkshop(Workshop workshop)
	{
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.CalculateOwnerIncomeFromWorkshop(workshop);
	}

	public override float RevenueSmoothenFraction()
	{
		return ((MBGameModel<ClanFinanceModel>)this).BaseModel.RevenueSmoothenFraction();
	}

	private int AddPartyExpense(MobileParty party, Clan clan, ExplainedNumber goldChange, bool applyWithdrawals)
	{
		int num = clan.Gold + (int)((ExplainedNumber)(ref goldChange)).ResultNumber;
		int num2 = num;
		if (num < (party.IsGarrison ? 8000 : 4000) && applyWithdrawals && clan != Clan.PlayerClan)
		{
			num2 = ((party.LeaderHero != null && party.PartyTradeGold < 500) ? MathF.Min(num, 250) : 0);
		}
		int num3 = CalculatePartyWage(party, num2, applyWithdrawals);
		int partyTradeGold = party.PartyTradeGold;
		if (applyWithdrawals)
		{
			if (party.IsLordParty && party.LeaderHero == null)
			{
				Hero leader = party.ActualClan.Leader;
				leader.Gold -= num3;
			}
			else
			{
				party.PartyTradeGold -= num3;
			}
		}
		partyTradeGold -= num3;
		if (partyTradeGold < ((ClanFinanceModel)this).PartyGoldLowerThreshold)
		{
			int num4 = ((ClanFinanceModel)this).PartyGoldLowerThreshold - partyTradeGold;
			if (party.IsLordParty && party.LeaderHero == null)
			{
				num4 = num3;
			}
			if (applyWithdrawals)
			{
				num4 = MathF.Min(num4, num2);
				party.PartyTradeGold += num4;
			}
			return -num4;
		}
		return 0;
	}

	private static int CalculatePartyWage(MobileParty mobileParty, int budget, bool applyWithdrawals)
	{
		int totalWage = mobileParty.TotalWage;
		int num = totalWage;
		if (applyWithdrawals)
		{
			num = MathF.Min(totalWage, budget);
			ApplyMoraleEffect(mobileParty, totalWage, num);
		}
		return num;
	}

	private static void ApplyMoraleEffect(MobileParty mobileParty, int wage, int paymentAmount)
	{
		if (paymentAmount < wage && wage > 0)
		{
			float num = 1f - (float)paymentAmount / (float)wage;
			float num2 = (float)Campaign.Current.Models.PartyMoraleModel.GetDailyNoWageMoralePenalty(mobileParty) * num;
			if (mobileParty.HasUnpaidWages < num)
			{
				num2 += (float)Campaign.Current.Models.PartyMoraleModel.GetDailyNoWageMoralePenalty(mobileParty) * (num - mobileParty.HasUnpaidWages);
			}
			mobileParty.RecentEventsMorale += num2;
			mobileParty.HasUnpaidWages = num;
			MBTextManager.SetTextVariable("reg1", MathF.Round(MathF.Abs(num2), 1), 2);
			if (mobileParty == MobileParty.MainParty)
			{
				MBInformationManager.AddQuickInformation(GameTexts.FindText("str_party_loses_moral_due_to_insufficent_funds", (string)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
		}
		else
		{
			mobileParty.HasUnpaidWages = 0f;
		}
	}
}
