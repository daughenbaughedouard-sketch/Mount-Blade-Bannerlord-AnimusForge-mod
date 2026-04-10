using System.Collections.Generic;
using Helpers;
using NavalDLC.Storyline.CampaignBehaviors;
using NavalDLC.Storyline.Quests;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Storyline;

public class NavalStorylineData
{
	public enum NavalStorylineStage
	{
		None = -1,
		Act1,
		Act2,
		Act3Quest1,
		Act3Quest2,
		Act3SpeakToSailors,
		Act3Quest4,
		Act3Quest5,
		Act3SpeakToGunnarAndSister
	}

	public enum NavalStorylineCheckpoint
	{
		None,
		Act1PortMenu,
		Act1PortFightSucceeded,
		Act1CaptivitySucceeded,
		Act2EncounterMenu,
		Act2Finalized,
		Act3Quest1SetPieceEncounterMenu,
		Act3Quest1SetPieceSucceeded,
		Act3Quest2EncounterMenu,
		Act3Quest2Succeeded,
		Act3Quest3EncounterMenu,
		Act3Quest3Succeeded,
		Act3Quest3InterceptedMenu,
		Act3Quest4EncounterMenu,
		Act3Quest4Succeeded,
		Act3Quest5EncounterMenu,
		Act3Quest5MissionMenu,
		Act3Quest5Succeeded
	}

	public enum StorylineCancelDetail
	{
		ByDialogue,
		ByRansom
	}

	public enum NavalStorylineSetPieceBattleMissionTypes
	{
		None = -1,
		Act1,
		Act2,
		Act3Quest1,
		Act3Quest2,
		Act3Quest3,
		Act3Quest4,
		Act3Quest5
	}

	public const string NavalStorylineSpecialQuestType = "NavalStoryline";

	private const string GunnarStringId = "naval_storyline_gangradir";

	private const string BjolgurStringId = "naval_storyline_bjolgur";

	private const string PurigStringId = "naval_storyline_northerner";

	private const string EmiraAlFahdaStringId = "naval_storyline_emira_al_fahda";

	private const string LaharStringId = "naval_storyline_lahar";

	private const string PrusasStringId = "naval_storyline_crusas";

	public const string NavalStoryLineOutOfTownMenuId = "naval_storyline_outside_town";

	public const string NavalStoryLineEncounterBlockingMenuId = "naval_storyline_encounter_blocking";

	public const string NavalStoryLineVirtualPortMenuId = "naval_storyline_virtualport";

	public const string NavalStoryLineEncounterMeetingMenuId = "naval_storyline_encounter_meeting";

	public const string NavalStoryLineEncounterMenuId = "naval_storyline_encounter";

	public const string NavalStoryLineJoinEncounterMenuId = "naval_storyline_join_encounter";

	private const string HomeSettlementStringId = "town_V8";

	private const string Act3Quest1TargetSettlementStringId = "town_N1";

	private const string Act3Quest2TargetSettlementStringId = "town_A1";

	private const string Act3Quest3TargetSettlementStringId = "town_S3";

	private const string Act3Quest4TargetSettlementStringId = "town_V7";

	public const string GunnarsVillageStringId = "village_N1_2";

	public const string InquireAtOsticanCharacterSpawnPointTag = "sp_storyline_npc";

	private Hero _gunnar;

	private Hero _bjolgur;

	private Hero _purig;

	private Hero _lahar;

	private Hero _emiraAlFahda;

	private Hero _prusas;

	private Banner _corsairBanner;

	private Settlement _homeSettlement;

	private Settlement _act3Quest1TargetSettlement;

	private Settlement _act3Quest2TargetSettlement;

	private Settlement _act3Quest3TargetSettlement;

	private Settlement _act3Quest4TargetSettlement;

	public static Hero Gunnar => NavalDLCManager.Instance.NavalStorylineData._gunnar;

	public static Hero Bjolgur => NavalDLCManager.Instance.NavalStorylineData._bjolgur;

	public static Hero Purig => NavalDLCManager.Instance.NavalStorylineData._purig;

	public static Hero Lahar => NavalDLCManager.Instance.NavalStorylineData._lahar;

	public static Hero EmiraAlFahda => NavalDLCManager.Instance.NavalStorylineData._emiraAlFahda;

	public static Hero Prusas => NavalDLCManager.Instance.NavalStorylineData._prusas;

	public static Settlement HomeSettlement => NavalDLCManager.Instance.NavalStorylineData._homeSettlement;

	public static Settlement Act3Quest1TargetSettlement => NavalDLCManager.Instance.NavalStorylineData._act3Quest1TargetSettlement;

	public static Settlement Act3Quest2TargetSettlement => NavalDLCManager.Instance.NavalStorylineData._act3Quest2TargetSettlement;

	public static Settlement Act3Quest3TargetSettlement => NavalDLCManager.Instance.NavalStorylineData._act3Quest3TargetSettlement;

	public static Settlement Act3Quest4TargetSettlement => NavalDLCManager.Instance.NavalStorylineData._act3Quest4TargetSettlement;

	public static Banner CorsairBanner => NavalDLCManager.Instance.NavalStorylineData._corsairBanner;

	public static void OnCheckpointReached(NavalStorylineCheckpoint checkpoint)
	{
		Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.OnCheckpointReached(checkpoint);
	}

	public void Initialize()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		if (!IsNavalStorylineCanceled())
		{
			CacheNavalStorylineSettlements();
			CreateStorylineHero("naval_storyline_gangradir", out _gunnar);
			CreateStorylineHero("naval_storyline_bjolgur", out _bjolgur);
			CreateStorylineHero("naval_storyline_northerner", out _purig);
			CreateStorylineHero("naval_storyline_lahar", out _lahar);
			CreateStorylineHero("naval_storyline_emira_al_fahda", out _emiraAlFahda);
			CreateStorylineHero("naval_storyline_crusas", out _prusas);
			_corsairBanner = new Banner("11.97.166.1528.1528.764.764.1.0.0.500.35.171.555.555.764.764.0.0.0.167.35.171.350.350.764.764.0.0.0");
		}
	}

	private void CacheNavalStorylineSettlements()
	{
		_homeSettlement = Settlement.Find("town_V8");
		_act3Quest1TargetSettlement = Settlement.Find("town_N1");
		_act3Quest2TargetSettlement = Settlement.Find("town_A1");
		_act3Quest3TargetSettlement = Settlement.Find("town_S3");
		_act3Quest4TargetSettlement = Settlement.Find("town_V7");
	}

	private void CreateStorylineHero(string stringId, out Hero hero)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		hero = Campaign.Current.CampaignObjectManager.Find<Hero>(stringId);
		if (hero == null)
		{
			CharacterObject val = MBObjectManager.Instance.GetObject<CharacterObject>(stringId);
			HeroCreator.CreateBasicHero(stringId, val, ref hero, true);
			hero.SetName(((BasicCharacterObject)val).Name, ((BasicCharacterObject)val).Name);
			CampaignTime randomBirthDayForAge = HeroHelper.GetRandomBirthDayForAge(((BasicCharacterObject)val).Age);
			hero.SetBirthDay(randomBirthDayForAge);
			hero.SetNewOccupation((Occupation)31);
			if (((MBObjectBase)val.Culture).StringId == "aserai")
			{
				hero.BornSettlement = Act3Quest2TargetSettlement;
			}
			else
			{
				hero.BornSettlement = HomeSettlement;
			}
			SetHeroText(hero);
		}
		hero.CharacterObject.SetTransferableInPartyScreen(false);
	}

	public static void SetHeroText(Hero hero)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		if (hero == Gunnar)
		{
			TextObject val = new TextObject("{=eW6Pbefi}Gunnar of {LAGSHOFN.NAME} is a Nord warrior from the island of Beinland. He won a reputation for courage fighting in a rebellion against {VOLBJORN.NAME}, first king of the Nordvyg, but after the rebels' defeat he made his peace with the victors. He has recently embarked on a campaign against the Sea Hounds, a pirate confederation that has terrorized the northern seas of Calradia.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("VOLBJORN", CharacterObject.Find("dead_lord_7_1"), val, false);
			StringHelpers.SetSettlementProperties("LAGSHOFN", Settlement.Find("village_N1_2"), val, false);
			hero.EncyclopediaText = val;
		}
		else if (hero == Purig)
		{
			TextObject val2 = new TextObject("{=bbBAWYbu}Purig is a Nord warrior who fought in the rebellion against {VOLBJORN.NAME}, first ruler of the Nordvyg. Following the king's victory he joined with other defeated rebels to form the Sea Hounds, a pirate confederation that is terrorizing the northern seas of Calradia.", (Dictionary<string, object>)null);
			StringHelpers.SetCharacterProperties("VOLBJORN", CharacterObject.Find("dead_lord_7_1"), val2, false);
			hero.EncyclopediaText = val2;
		}
		else if (hero == Lahar)
		{
			TextObject encyclopediaText = new TextObject("{=7y7cF9dC}Lahar is a sea captain and former corsair, now currently in the employ of the merchants of Quyaz.", (Dictionary<string, object>)null);
			hero.EncyclopediaText = encyclopediaText;
		}
		else if (hero == EmiraAlFahda)
		{
			TextObject encyclopediaText2 = new TextObject("{=MADDAmO5}The Emira al-Fahda is a noblewoman from the city of Quyaz. She fell out with her uncles over an inheritance dispute and turned pirate, allying with the Sea Hounds and ravaging Quyazi shipping.", (Dictionary<string, object>)null);
			hero.EncyclopediaText = encyclopediaText2;
		}
		else if (hero == Prusas)
		{
			TextObject encyclopediaText3 = new TextObject("{=A0Zr68nk}Salautas Crusas is an imperial merchant who owns sulfur mines in the Gulf of Charas. He uses slaves, which he purchases from the Sea Hounds.", (Dictionary<string, object>)null);
			hero.EncyclopediaText = encyclopediaText3;
		}
		else if (hero == Bjolgur)
		{
			TextObject encyclopediaText4 = new TextObject("{=8qAnEXE5}Bjolgur of Agilting is a Nord warrior who fought alongside Gunnar in the rebellion against King Volbjorn. After the rebels' defeat he joined up with the Skolderbroda, a mercenary brotherhood.", (Dictionary<string, object>)null);
			hero.EncyclopediaText = encyclopediaText4;
		}
	}

	public static bool IsNavalStorylineHero(Hero hero)
	{
		if (hero != Gunnar && hero != Purig && hero != Bjolgur && hero != Lahar && hero != EmiraAlFahda)
		{
			return hero == Prusas;
		}
		return true;
	}

	public static void StartNavalStoryline()
	{
		((QuestBase)new InquireAtOstican()).StartQuest();
	}

	public static bool IsStorylineActivationPossible()
	{
		if (Campaign.Current.IsMainHeroDisguised || MobileParty.MainParty.Army != null)
		{
			return false;
		}
		return !IsWaitingForSistersReturn();
	}

	public static void ActivateNavalStoryline()
	{
		NavalStorylineCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>();
		if (campaignBehavior != null && !campaignBehavior.IsNavalStorylineActive())
		{
			campaignBehavior.ChangeNavalStorylineActivity(activity: true);
		}
	}

	public static void DeactivateNavalStoryline()
	{
		NavalStorylineCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>();
		if (campaignBehavior != null && campaignBehavior.IsNavalStorylineActive())
		{
			campaignBehavior.ChangeNavalStorylineActivity(activity: false);
		}
	}

	public static bool IsMainPartyAllowed()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		AccessDetails val = default(AccessDetails);
		Campaign.Current.Models.SettlementAccessModel.CanMainHeroEnterSettlement(Settlement.CurrentSettlement, ref val);
		if ((int)val.AccessLevel == 2 && !Clan.PlayerClan.MapFaction.IsAtWarWith(Settlement.CurrentSettlement.MapFaction))
		{
			if (Settlement.CurrentSettlement.SiegeEvent != null)
			{
				if (!Settlement.CurrentSettlement.SiegeEvent.IsBlockadeActive)
				{
					return MobileParty.MainParty.HasNavalNavigationCapability;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool IsTutorialSkipped()
	{
		return Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.IsTutorialSkipped() ?? false;
	}

	public static bool IsNavalStoryLineActive()
	{
		return Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.IsNavalStorylineActive() ?? false;
	}

	public static bool HasCompletedLast(NavalStorylineStage stage)
	{
		NavalStorylineCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>();
		if (campaignBehavior != null)
		{
			return stage == campaignBehavior.GetNavalStorylineStage();
		}
		return false;
	}

	public static NavalStorylineStage GetStorylineStage()
	{
		return Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.GetNavalStorylineStage() ?? NavalStorylineStage.None;
	}

	public static NavalStorylineSetPieceBattleMissionTypes GetNavalStorylineSetPieceBattleMissionType()
	{
		return Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.GetNavalStorylineSetPieceBattleMissionType() ?? NavalStorylineSetPieceBattleMissionTypes.None;
	}

	public static bool IsWaitingForSistersReturn()
	{
		return Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.IsWaitingForSistersReturn() ?? false;
	}

	public static void SetNavalStorylineSetPieceBattleMissionType(NavalStorylineSetPieceBattleMissionTypes missionType)
	{
		Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.SetNavalStorylineSetPieceBattleMissionType(missionType);
	}

	public static bool IsNavalStorylineCanceled()
	{
		return Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.GetIsNavalStorylineCanceled() ?? true;
	}

	public static void OnStorylineProgress(NavalStorylineQuestBase navalQuest)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ActivateNavalStoryline();
		FadeToBlack();
		if (navalQuest.Template != null)
		{
			MobileParty.MainParty.InitializeMobilePartyAtPosition(navalQuest.Template, MobileParty.MainParty.Position);
			foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
			{
				item.IsTradeable = false;
				item.IsUsedByQuest = true;
			}
		}
		AddGunnarToMainParty();
		GiveProvisionsToPlayer();
		MobileParty.MainParty.SetSailAtPosition(Settlement.CurrentSettlement.PortPosition);
		PlayerEncounter.Finish(true);
	}

	private static void GiveProvisionsToPlayer()
	{
		Campaign.Current.GetCampaignBehavior<NavalStorylineCampaignBehavior>()?.GiveProvisionsToPlayer();
	}

	public static void AddGunnarToMainParty()
	{
		Gunnar.Heal(Gunnar.MaxHitPoints, false);
		MobileParty.MainParty.MemberRoster.AddToCounts(Gunnar.CharacterObject, 1, false, 0, 0, true, -1);
	}

	public static void TeleportMainPartyBackToBase()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		FadeToBlack();
		if (MobileParty.MainParty.CurrentSettlement != HomeSettlement)
		{
			MobileParty.MainParty.Position = (MobileParty.MainParty.HasNavalNavigationCapability ? HomeSettlement.PortPosition : HomeSettlement.GatePosition);
			MobileParty.MainParty.IsCurrentlyAtSea = MobileParty.MainParty.HasNavalNavigationCapability;
			if (PlayerEncounter.Current != null)
			{
				PlayerEncounter.Finish(true);
			}
			if (MobileParty.MainParty.IsInRaftState)
			{
				RaftStateChangeAction.DeactivateRaftStateForParty(MobileParty.MainParty);
			}
			if (Hero.MainHero.IsPrisoner)
			{
				EndCaptivityAction.ApplyByReleasedAfterBattle(Hero.MainHero);
			}
			if (MobileParty.MainParty.Anchor.IsValid)
			{
				MobileParty.MainParty.Anchor.SetPosition(new CampaignVec2(Vec2.Invalid, false));
			}
			EncounterManager.StartSettlementEncounter(MobileParty.MainParty, HomeSettlement);
		}
		MobileParty.MainParty.SetMoveModeHold();
		GameState activeState = GameStateManager.Current.ActiveState;
		MapState val;
		if ((val = (MapState)(object)((activeState is MapState) ? activeState : null)) != null)
		{
			val.Handler.TeleportCameraToMainParty();
		}
		UpdateVisibilityAndInspectedAroundHomeSettlement();
	}

	public static void TeleportMainHeroAndGunnarBackToBase()
	{
		TeleportMainPartyBackToBase();
		Gunnar.Heal(Gunnar.MaxHitPoints, false);
		EnterSettlementAction.ApplyForCharacterOnly(Gunnar, HomeSettlement);
		GameMenu.ActivateGameMenu("naval_storyline_outside_town");
	}

	private static void UpdateVisibilityAndInspectedAroundHomeSettlement()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		float seeingRange = MobileParty.MainParty.SeeingRange;
		CampaignVec2 position = HomeSettlement.Position;
		LocatableSearchData<MobileParty> val = MobileParty.StartFindingLocatablesAroundPosition(((CampaignVec2)(ref position)).ToVec2(), Campaign.Current.Models.MapVisibilityModel.MaximumSeeingRange() + 5f);
		HomeSettlement.Party.UpdateVisibilityAndInspected(position, seeingRange);
		for (MobileParty val2 = MobileParty.FindNextLocatable(ref val); val2 != null; val2 = MobileParty.FindNextLocatable(ref val))
		{
			if (!val2.IsMilitia && !val2.IsGarrison)
			{
				val2.Party.UpdateVisibilityAndInspected(position, seeingRange);
			}
		}
	}

	private static void FadeToBlack()
	{
		if (Game.Current.GameStateManager.ActiveState is MapState)
		{
			ScreenFadeController.BeginFadeOutAndIn(0.1f, 0.5f, 0.35f);
		}
	}

	public static MissionInitializerRecord GetNavalMissionInitializerTemplate(string sceneName)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		MissionInitializerRecord result = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref result))._002Ector(sceneName);
		result.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		result.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
		result.DecalAtlasGroup = 2;
		return result;
	}

	public static void OnPlayerPostponedQuestStart()
	{
		if (!IsMainPartyAllowed())
		{
			Mission current = Mission.Current;
			if (current != null)
			{
				current.EndMission();
			}
		}
	}
}
