using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.View;

public static class SandBoxViewCheats
{
	[CommandLineArgumentFunction("kill_hero", "campaign")]
	public static string KillHero(List<string> strings)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string text = "Format is \"campaign.kill_hero [HeroName]\".";
		if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		string text2 = CampaignCheats.ConcatenateString(strings);
		Hero val = default(Hero);
		string text3 = default(string);
		if (CampaignCheats.TryGetObject<Hero>(text2, ref val, ref text3, (Func<Hero, bool>)((Hero x) => x.IsActive && (x.IsLord || x.IsWanderer))))
		{
			if (!val.IsAlive)
			{
				return "Hero " + text2 + " is already dead.";
			}
			if ((int)val.DeathMark != 0)
			{
				return "Hero already has a death mark.";
			}
			if (val.CurrentSettlement != null && !val.IsNotable)
			{
				return "Hero cannot be killed while staying in a settlement.";
			}
			if (MapScreen.Instance.IsHeirSelectionPopupActive)
			{
				return "Hero cannot be killed during the heir selection.";
			}
			if (Campaign.Current.ConversationManager.OneToOneConversationHero != null)
			{
				return "Hero cannot be killed during a conversation.";
			}
			MobileParty partyBelongedTo = val.PartyBelongedTo;
			if (((partyBelongedTo != null) ? partyBelongedTo.MapEvent : null) == null)
			{
				MobileParty partyBelongedTo2 = val.PartyBelongedTo;
				if (((partyBelongedTo2 != null) ? partyBelongedTo2.SiegeEvent : null) == null)
				{
					if (!val.CanDie((KillCharacterActionDetail)1))
					{
						return "Hero can't die!";
					}
					KillCharacterAction.ApplyByMurder(val, (Hero)null, true);
					goto IL_0124;
				}
			}
			if (!val.CanDie((KillCharacterActionDetail)4))
			{
				return "Hero can't die!";
			}
			val.AddDeathMark((Hero)null, (KillCharacterActionDetail)4);
			goto IL_0124;
		}
		return text3 + "\n" + text;
		IL_0124:
		return "Hero " + text2.ToLower() + " is killed.";
	}

	[CommandLineArgumentFunction("focus_tournament", "campaign")]
	public static string FocusTournament(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		if (CampaignCheats.CheckHelp(strings))
		{
			return "Format is \"campaign.focus_tournament\".";
		}
		Settlement val = Settlement.FindFirst((Func<Settlement, bool>)((Settlement x) => x.IsTown && Campaign.Current.TournamentManager.GetTournamentGame(x.Town) != null));
		if (val == null)
		{
			return "There isn't any tournament right now.";
		}
		((MapCameraView)typeof(MapCameraView).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)).SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
		val.Party.SetAsCameraFollowParty();
		return "Success";
	}

	[CommandLineArgumentFunction("make_clan_mercenary_of_kingdom", "campaign")]
	public static string MakeClanMercenaryOfKingdom(List<string> strings)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckParameters(strings, 1) || CampaignCheats.CheckHelp(strings))
		{
			return "Format is \"campaign.MakeClanMercenaryOfKingdom [clan] | [kingdom] | [days]\".";
		}
		List<string> separatedNames = CampaignCheats.GetSeparatedNames(strings, true);
		if (separatedNames.Count < 2)
		{
			return "Format is \"campaign.MakeClanMercenaryOfKingdom [clan] | [kingdom] | [days]\".";
		}
		Clan val = default(Clan);
		string text = default(string);
		CampaignCheats.TryGetObject<Clan>(separatedNames[0], ref val, ref text, (Func<Clan, bool>)null);
		if (val == null)
		{
			return "Cant find the clan\n" + text;
		}
		Kingdom val2 = default(Kingdom);
		string result = default(string);
		CampaignCheats.TryGetObject<Kingdom>(separatedNames[1], ref val2, ref result, (Func<Kingdom, bool>)null);
		if (val2 == null)
		{
			return result;
		}
		if (!val.IsMinorFaction)
		{
			return "Clan is not suitable to be mercenary";
		}
		if (val == Clan.PlayerClan)
		{
			return "Use join_kingdom or join_kingdom_as_mercenary";
		}
		if (val.IsUnderMercenaryService)
		{
			ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(val, true);
		}
		CampaignTime val3 = CampaignTime.Zero;
		if (separatedNames.Count == 3 && int.TryParse(separatedNames[2], out var result2))
		{
			val3 = CampaignTime.DaysFromNow((float)result2);
		}
		ChangeKingdomAction.ApplyByJoinFactionAsMercenary(val, val2, val3, 50, true);
		return "Success";
	}

	[CommandLineArgumentFunction("focus_hostile_army", "campaign")]
	public static string FocusHostileArmy(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		if (CampaignCheats.CheckHelp(strings))
		{
			return "Format is \"campaign.focus_hostile_army\".";
		}
		Army val = null;
		foreach (Kingdom item in (List<Kingdom>)(object)Kingdom.All)
		{
			if ((object)item != Clan.PlayerClan.MapFaction && !Extensions.IsEmpty<Army>((IEnumerable<Army>)item.Armies) && item.IsAtWarWith(Clan.PlayerClan.MapFaction))
			{
				val = Extensions.GetRandomElement<Army>(item.Armies);
			}
			if (val != null)
			{
				break;
			}
		}
		if (val == null)
		{
			return "There isn't any hostile army right now.";
		}
		((MapCameraView)typeof(MapCameraView).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)).SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
		val.LeaderParty.Party.SetAsCameraFollowParty();
		return "Success";
	}

	[CommandLineArgumentFunction("focus_mobile_party", "campaign")]
	public static string FocusMobileParty(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string text = "Format is \"campaign.focus_mobile_party [PartyName]\".";
		if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		MobileParty val = default(MobileParty);
		string text2 = default(string);
		if (CampaignCheats.TryGetObject<MobileParty>(CampaignCheats.ConcatenateString(strings), ref val, ref text2, (Func<MobileParty, bool>)null))
		{
			MapCameraView obj = (MapCameraView)typeof(MapCameraView).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			if (!val.IsVisible && val.CurrentSettlement == null)
			{
				val.IsVisible = true;
			}
			obj.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
			val.Party.SetAsCameraFollowParty();
			return $"Focused party {val.Name}";
		}
		return text2 + " : \n" + text;
	}

	[CommandLineArgumentFunction("focus_hero", "campaign")]
	public static string FocusHero(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string text = "Format is \"campaign.focus_hero [HeroName]\".";
		if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		string text2 = CampaignCheats.ConcatenateString(strings);
		Hero val = default(Hero);
		string text3 = default(string);
		if (CampaignCheats.TryGetObject<Hero>(text2, ref val, ref text3, (Func<Hero, bool>)((Hero x) => x != Hero.MainHero && x.IsActive && (x.IsLord || x.IsWanderer))))
		{
			MapCameraView mapCameraView = (MapCameraView)typeof(MapCameraView).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			if (val.CurrentSettlement != null)
			{
				mapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
				val.CurrentSettlement.Party.SetAsCameraFollowParty();
				return "Success";
			}
			if (val.PartyBelongedTo != null)
			{
				mapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
				val.PartyBelongedTo.Party.SetAsCameraFollowParty();
				if (val.PartyBelongedTo.CurrentSettlement == null && !val.PartyBelongedTo.IsVisible)
				{
					val.PartyBelongedTo.IsVisible = true;
				}
				return "Success";
			}
			if (val.PartyBelongedToAsPrisoner != null)
			{
				mapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
				val.PartyBelongedToAsPrisoner.SetAsCameraFollowParty();
				if (val.PartyBelongedToAsPrisoner.MobileParty.CurrentSettlement == null && !val.PartyBelongedToAsPrisoner.MobileParty.IsVisible)
				{
					val.PartyBelongedToAsPrisoner.MobileParty.IsVisible = true;
				}
				return "Success";
			}
			return "Party is not found : " + text2 + "\n" + text;
		}
		return text3 + ": " + text2 + "\n" + text;
	}

	[CommandLineArgumentFunction("focus_infested_hideout", "campaign")]
	public static string FocusInfestedHideout(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string text = "Format is \"campaign.focus_infested_hideout [Optional: Number of troops]\".";
		if (CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		MBList<Settlement> val = Extensions.ToMBList<Settlement>(((IEnumerable<Settlement>)Settlement.All).Where((Settlement t) => t.IsHideout && ((List<MobileParty>)(object)t.Parties).Count > 0));
		Settlement val2 = null;
		if (Extensions.IsEmpty<Settlement>((IEnumerable<Settlement>)val))
		{
			return "All hideouts are empty!";
		}
		if (strings.Count > 0)
		{
			int troopCount = -1;
			int.TryParse(strings[0], out troopCount);
			if (troopCount == -1)
			{
				return "Incorrect input.\n" + text;
			}
			MBList<Settlement> val3 = Extensions.ToMBList<Settlement>(((IEnumerable<Settlement>)val).Where((Settlement t) => ((IEnumerable<MobileParty>)t.Parties).Sum((MobileParty p) => p.MemberRoster.TotalManCount) >= troopCount));
			if (Extensions.IsEmpty<Settlement>((IEnumerable<Settlement>)val3))
			{
				return "Can't find suitable hideout.";
			}
			val2 = Extensions.GetRandomElement<Settlement>(val3);
		}
		else
		{
			val2 = Extensions.GetRandomElement<Settlement>(val);
		}
		if (val2 != null)
		{
			((MapCameraView)typeof(MapCameraView).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)).SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
			val2.Party.SetAsCameraFollowParty();
			return "Success";
		}
		return "Unable to find such a hideout.";
	}

	[CommandLineArgumentFunction("focus_issue", "campaign")]
	public static string FocusIssues(List<string> strings)
	{
		if (!CampaignCheats.CheckCheatUsage(ref CampaignCheats.ErrorType))
		{
			return CampaignCheats.ErrorType;
		}
		string text = "Format is \"campaign.focus_issue [IssueName]\".";
		if (CampaignCheats.CheckParameters(strings, 0) || CampaignCheats.CheckHelp(strings))
		{
			return text;
		}
		MapCameraView mapCameraView = (MapCameraView)typeof(MapCameraView).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
		IssueBase val = default(IssueBase);
		string text2 = default(string);
		CampaignCheats.TryGetObject<IssueBase>(CampaignCheats.ConcatenateString(strings), ref val, ref text2, (Func<IssueBase, bool>)null);
		if (val == null)
		{
			return text2 + " " + text;
		}
		if (val.IssueSettlement != null)
		{
			mapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
			val.IssueSettlement.Party.SetAsCameraFollowParty();
		}
		else if (val.IssueOwner.PartyBelongedTo != null)
		{
			mapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
			MobileParty partyBelongedTo = val.IssueOwner.PartyBelongedTo;
			if (partyBelongedTo != null)
			{
				partyBelongedTo.Party.SetAsCameraFollowParty();
			}
		}
		else if (val.IssueOwner.CurrentSettlement != null)
		{
			mapCameraView.SetCameraMode(MapCameraView.CameraFollowMode.FollowParty);
			val.IssueOwner.CurrentSettlement.Party.SetAsCameraFollowParty();
		}
		return "Found issue: " + ((object)val.Title).ToString() + ". Issue Owner: " + ((object)val.IssueOwner.Name).ToString();
	}
}
