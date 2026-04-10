using System;
using System.Collections.Generic;
using NavalDLC.Storyline;
using StoryMode;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NavalDLC.CampaignBehaviors;

public class NavalInitializationCampaignBehavior : CampaignBehaviorBase
{
	private bool _hasIntroductionPopUpBeenShown;

	public override void RegisterEvents()
	{
		if (((GameType)Campaign.Current).CurrentGame.GameType is CampaignStoryMode)
		{
			StoryModeEvents.OnStealthTutorialActivatedEvent.AddNonSerializedListener((object)this, (Action)OnStealthTutorialActivated);
		}
		CampaignEvents.OnCharacterCreationIsOverEvent.AddNonSerializedListener((object)this, (Action)OnCharacterCreationIsOver);
		CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnAfterSessionLaunched);
	}

	private void OnCharacterCreationIsOver()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0046: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		if (!(((GameType)Campaign.Current).CurrentGame.GameType is CampaignStoryMode) && !_hasIntroductionPopUpBeenShown)
		{
			TextObject val = new TextObject("{=SJT8Nl5a}Call of the Oceans", (Dictionary<string, object>)null);
			TextObject val2 = new TextObject("{=XcaoQSjv}Often, when you were growing up, you wondered if your destiny might lie upon the sea. You listened closely to old sailors telling their tales of daring and peril: steering longships through the icy storms of the north, outfoxing corsairs along the pirate-infested coasts of the south, or standing in the forecastle of a dromon as it crashed through the enemy battle-line. You wonder what opportunities lie for you to seize on the seas of Calradia: a fortune made in the commerce and intrigues of a bustling port, or glory won on the bloodied deck of a foe's flagship.", (Dictionary<string, object>)null);
			TextObject val3 = new TextObject("{=DM6luo3c}Continue", (Dictionary<string, object>)null);
			InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, false, ((object)val3).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}
		HeroDeveloper heroDeveloper = Hero.MainHero.HeroDeveloper;
		heroDeveloper.UnspentFocusPoints += 6;
	}

	private void OnStealthTutorialActivated()
	{
		ShowIntroductionPopUp();
	}

	private void ShowIntroductionPopUp()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_003b: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		_hasIntroductionPopUpBeenShown = true;
		TextObject val = new TextObject("{=F6qA5Mmo}Troubled Waters", (Dictionary<string, object>)null);
		TextObject val2 = new TextObject("{=Iq2YN7o3}Throughout your travels, you overheard hushed conversations about a new menace, a pirate confederacy terrorizing the coasts of Calradia. These northern corsairs have built a reputation for trading in captives from bandits and raiders such as the ones who attacked your family. Do you want to go to Ostican now to try to pick up your sister's trail and embark on an hunt across the seas of Calradia?", (Dictionary<string, object>)null);
		TextObject val3 = new TextObject("{=0aD2pdmB}Take me to Ostican now", (Dictionary<string, object>)null);
		TextObject val4 = new TextObject("{=fRRkHsZR}I'll go there myself", (Dictionary<string, object>)null);
		InformationManager.ShowInquiry(new InquiryData(((object)val).ToString(), ((object)val2).ToString(), true, true, ((object)val3).ToString(), ((object)val4).ToString(), (Action)OnStorylinePopUpAccepted, (Action)OnStorylinePopUpDeclined, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), true, false);
	}

	private void OnStorylinePopUpAccepted()
	{
		NavalStorylineData.StartNavalStoryline();
		NavalStorylineData.TeleportMainPartyBackToBase();
	}

	private void OnStorylinePopUpDeclined()
	{
		NavalStorylineData.StartNavalStoryline();
	}

	private void OnAfterSessionLaunched(CampaignGameStarter gameStarter)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!MBSaveLoad.IsUpdatingGameVersion)
		{
			return;
		}
		ApplicationVersion lastLoadedGameVersion = MBSaveLoad.LastLoadedGameVersion;
		if (!((ApplicationVersion)(ref lastLoadedGameVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.12", 0)))
		{
			return;
		}
		foreach (Hero item in (List<Hero>)(object)Hero.AllAliveHeroes)
		{
			HeroDeveloper heroDeveloper = item.HeroDeveloper;
			heroDeveloper.UnspentFocusPoints += 6;
		}
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<bool>("_hasIntroductionPopUpBeenShown", ref _hasIntroductionPopUpBeenShown);
	}
}
