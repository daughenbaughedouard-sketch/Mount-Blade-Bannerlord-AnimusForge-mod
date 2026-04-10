using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace SandBox.CampaignBehaviors;

public class BarberCampaignBehavior : CampaignBehaviorBase, IFacegenCampaignBehavior, ICampaignBehavior
{
	private class BarberFaceGeneratorCustomFilter : IFaceGeneratorCustomFilter
	{
		private readonly int[] _haircutIndices;

		private readonly int[] _facialHairIndices;

		private readonly bool _defaultStages;

		public BarberFaceGeneratorCustomFilter(bool useDefaultStages, int[] haircutIndices, int[] faircutIndices)
		{
			_haircutIndices = haircutIndices;
			_facialHairIndices = faircutIndices;
			_defaultStages = useDefaultStages;
		}

		public int[] GetHaircutIndices(BasicCharacterObject character)
		{
			return _haircutIndices;
		}

		public int[] GetFacialHairIndices(BasicCharacterObject character)
		{
			return _facialHairIndices;
		}

		public FaceGeneratorStage[] GetAvailableStages()
		{
			if (_defaultStages)
			{
				FaceGeneratorStage[] array = new FaceGeneratorStage[7];
				RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
				return (FaceGeneratorStage[])(object)array;
			}
			return (FaceGeneratorStage[])(object)new FaceGeneratorStage[1] { (FaceGeneratorStage)5 };
		}
	}

	private const int BarberCost = 100;

	private bool _isOpenedFromBarberDialogue;

	private StaticBodyProperties _previousBodyProperties;

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener((object)this, (Action<Dictionary<string, int>>)LocationCharactersAreReadyToSpawn);
	}

	public override void SyncData(IDataStore store)
	{
	}

	private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
	{
		AddDialogs(campaignGameStarter);
	}

	private void AddDialogs(CampaignGameStarter campaignGameStarter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0035: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_006b: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00ad: Expected O, but got Unknown
		//IL_00ad: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		//IL_0151: Expected O, but got Unknown
		//IL_0151: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01a7: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Expected O, but got Unknown
		campaignGameStarter.AddDialogLine("barber_start_talk_beggar", "start", "close_window", "{=pWzdxd7O}May the Heavens bless you, my poor {?PLAYER.GENDER}lady{?}fellow{\\?}, but I can't spare a coin right now.", new OnConditionDelegate(InDisguiseSpeakingToBarber), new OnConsequenceDelegate(InitializeBarberConversation), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("barber_start_talk", "start", "barber_question1", "{=2aXYYNBG}Come to have your hair cut, {?PLAYER.GENDER}my lady{?}my lord{\\?}? A new look for a new day?", new OnConditionDelegate(IsConversationAgentBarber), new OnConsequenceDelegate(InitializeBarberConversation), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_accept_haircut", "barber_question1", "start_cut_token", "{=Q7wBRXtR}Yes, I have. ({GOLD_COST} {GOLD_ICON})", new OnConditionDelegate(GivePlayerAHaircutCondition), new OnConsequenceDelegate(GivePlayerAHaircut), 100, new OnClickableConditionDelegate(DoesPlayerHaveEnoughGold), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_refuse_haircut", "barber_question1", "no_haircut_conversation_token", "{=xPAAZAaI}My hair is fine as it is, thank you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("barber_ask_if_done", "start_cut_token", "finish_cut_token", "{=M3K8wUOO}So... Does this please you, {?PLAYER.GENDER}my lady{?}my lord{\\?}?", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_done_with_haircut", "finish_cut_token", "finish_barber", "{=zTF4bJm0}Yes, it's fine.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_not_done_with_haircut", "finish_cut_token", "start_cut_token", "{=BnoSOi3r}Actually...", new OnConditionDelegate(GivePlayerAHaircutCondition), new OnConsequenceDelegate(GivePlayerAHaircut), 100, new OnClickableConditionDelegate(DoesPlayerHaveEnoughGold), (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddDialogLine("barber_no_haircut_talk", "no_haircut_conversation_token", "close_window", "{=BusYGTrN}Excellent! Have a good day, then, {?PLAYER.GENDER}my lady{?}my lord{\\?}.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("barber_haircut_finished", "finish_barber", "player_had_a_haircut_token", "{=akqJbZpH}Marvellous! You cut a splendid appearance, {?PLAYER.GENDER}my lady{?}my lord{\\?}, if you don't mind my saying. Most splendid.", new OnConditionDelegate(DidPlayerHaveAHaircut), new OnConsequenceDelegate(ChargeThePlayer), 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddDialogLine("barber_haircut_no_change", "finish_barber", "player_did_not_cut_token", "{=yLIZlaS1}Very well. Do come back when you're ready, {?PLAYER.GENDER}my lady{?}my lord{\\?}.", new OnConditionDelegate(DidPlayerNotHaveAHaircut), (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_no_haircut_finish_talk", "player_did_not_cut_token", "close_window", "{=oPUVNuhN}I'll keep you in mind", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		campaignGameStarter.AddPlayerLine("player_haircut_finish_talk", "player_had_a_haircut_token", "close_window", "{=F9Xjbchh}Thank you.", (OnConditionDelegate)null, (OnConsequenceDelegate)null, 100, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
	}

	private bool InDisguiseSpeakingToBarber()
	{
		if (IsConversationAgentBarber())
		{
			return Campaign.Current.IsMainHeroDisguised;
		}
		return false;
	}

	private bool DoesPlayerHaveEnoughGold(out TextObject explanation)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (Hero.MainHero.Gold < 100)
		{
			explanation = new TextObject("{=RYJdU43V}Not Enough Gold", (Dictionary<string, object>)null);
			return false;
		}
		explanation = null;
		return true;
	}

	private void ChargeThePlayer()
	{
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, 100, false);
	}

	private bool DidPlayerNotHaveAHaircut()
	{
		return !DidPlayerHaveAHaircut();
	}

	private bool DidPlayerHaveAHaircut()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		BodyProperties bodyProperties = Hero.MainHero.BodyProperties;
		return ((BodyProperties)(ref bodyProperties)).StaticProperties != _previousBodyProperties;
	}

	private bool IsConversationAgentBarber()
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		return ((currentSettlement != null) ? currentSettlement.Culture.Barber : null) == CharacterObject.OneToOneConversationCharacter;
	}

	private bool GivePlayerAHaircutCondition()
	{
		MBTextManager.SetTextVariable("GOLD_COST", 100);
		return true;
	}

	private void GivePlayerAHaircut()
	{
		_isOpenedFromBarberDialogue = true;
		BarberState val = Game.Current.GameStateManager.CreateState<BarberState>(new object[2]
		{
			Hero.MainHero.CharacterObject,
			GetFaceGenFilter()
		});
		_isOpenedFromBarberDialogue = false;
		GameStateManager.Current.PushState((GameState)(object)val, 0);
	}

	private void InitializeBarberConversation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		BodyProperties bodyProperties = Hero.MainHero.BodyProperties;
		_previousBodyProperties = ((BodyProperties)(ref bodyProperties)).StaticProperties;
	}

	private LocationCharacter CreateBarber(CultureObject culture, CharacterRelations relation)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		CharacterObject barber = culture.Barber;
		int num = default(int);
		int num2 = default(int);
		Campaign.Current.Models.AgeModel.GetAgeLimitForLocation(barber, ref num, ref num2, "Barber");
		AgentData obj = new AgentData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)barber, -1, (Banner)null, default(UniqueTroopDescriptor))).Monster(FaceGen.GetMonsterWithSuffix(((BasicCharacterObject)barber).Race, "_settlement_slow")).Age(MBRandom.RandomInt(num, num2));
		IAgentBehaviorManager agentBehaviorManager = SandBoxManager.Instance.AgentBehaviorManager;
		return new LocationCharacter(obj, new AddBehaviorsDelegate(agentBehaviorManager.AddWandererBehaviors), "sp_barber", true, relation, (string)null, true, false, (ItemObject)null, false, false, true, (AfterAgentCreatedDelegate)null, false);
	}

	private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
		if (CampaignMission.Current.Location == locationWithId && Campaign.Current.IsDay && unusedUsablePointCount.TryGetValue("sp_merchant_notary", out var _))
		{
			locationWithId.AddLocationCharacters(new CreateLocationCharacterDelegate(CreateBarber), Settlement.CurrentSettlement.Culture, (CharacterRelations)0, 1);
		}
	}

	public IFaceGeneratorCustomFilter GetFaceGenFilter()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		if (Settlement.CurrentSettlement != null)
		{
			list.AddRange(Campaign.Current.Models.BodyPropertiesModel.GetHairIndicesForCulture(((BasicCharacterObject)Hero.MainHero.CharacterObject).Race, Hero.MainHero.IsFemale ? 1 : 0, Hero.MainHero.Age, Settlement.CurrentSettlement.Culture));
			list2.AddRange(Campaign.Current.Models.BodyPropertiesModel.GetBeardIndicesForCulture(((BasicCharacterObject)Hero.MainHero.CharacterObject).Race, Hero.MainHero.IsFemale ? 1 : 0, Hero.MainHero.Age, Settlement.CurrentSettlement.Culture));
		}
		else
		{
			foreach (CultureObject item in (List<CultureObject>)(object)MBObjectManager.Instance.GetObjectTypeList<CultureObject>())
			{
				list.AddRange(Campaign.Current.Models.BodyPropertiesModel.GetHairIndicesForCulture(((BasicCharacterObject)Hero.MainHero.CharacterObject).Race, Hero.MainHero.IsFemale ? 1 : 0, Hero.MainHero.Age, item));
				list2.AddRange(Campaign.Current.Models.BodyPropertiesModel.GetBeardIndicesForCulture(((BasicCharacterObject)Hero.MainHero.CharacterObject).Race, Hero.MainHero.IsFemale ? 1 : 0, Hero.MainHero.Age, item));
			}
		}
		return (IFaceGeneratorCustomFilter)(object)new BarberFaceGeneratorCustomFilter(!_isOpenedFromBarberDialogue, list.Distinct().ToArray(), list2.Distinct().ToArray());
	}
}
