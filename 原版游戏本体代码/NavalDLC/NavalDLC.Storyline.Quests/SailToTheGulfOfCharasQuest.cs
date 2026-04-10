using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class SailToTheGulfOfCharasQuest : NavalStorylineQuestBase
{
	private const string LaharShipHullId = "ship_liburna_q2_storyline";

	private static readonly Dictionary<string, string> LaharShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl3" },
		{ "sail", "sails_lvl2" },
		{ "bow", "bow_northern_reinforced_ram_lvl3" }
	};

	private const string GunnarShipHullId = "northern_medium_ship";

	private static readonly Dictionary<string, string> GunnarShipUpgradePieces = new Dictionary<string, string>
	{
		{ "side", "side_southern_shields_lvl2" },
		{ "sail", "sails_lvl2" }
	};

	[SaveableField(1)]
	private readonly CampaignVec2 _corsairSpawnPosition;

	[SaveableField(2)]
	private readonly MapMarker _corsairHuntingGroundMarker;

	[SaveableField(3)]
	private bool _willProgressStoryline;

	public override bool WillProgressStoryline => _willProgressStoryline;

	public override TextObject Title => new TextObject("{=LMRgfeFC}Sail to the Gulf of Charas", (Dictionary<string, object>)null);

	private TextObject QuestStartLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=7i9UFPLB}Find {HERO.NAME} in her hunting grounds in the Gulf of Charas", (Dictionary<string, object>)null);
			TextObjectExtensions.SetCharacterProperties(val, "HERO", NavalStorylineData.EmiraAlFahda.CharacterObject, false);
			return val;
		}
	}

	private TextObject QuestSuccessLogText => new TextObject("{=lY5770ox}You found the corsairs.", (Dictionary<string, object>)null);

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3Quest2;

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_2_main_party_template";

	public SailToTheGulfOfCharasQuest(string questId, Hero questGiver, CampaignVec2 corsairSpawnPosition)
		: base(questId, questGiver, CampaignTime.Never, 0)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		_corsairSpawnPosition = corsairSpawnPosition;
		_willProgressStoryline = true;
		_corsairHuntingGroundMarker = Campaign.Current.MapMarkerManager.CreateMapMarker(NavalStorylineData.CorsairBanner, new TextObject("{=QLrwlirp}Corsair Hunting Grounds", (Dictionary<string, object>)null), ((CampaignVec2)(ref _corsairSpawnPosition)).AsVec3(), true, ((MBObjectBase)this).StringId);
	}

	protected override void SetDialogs()
	{
	}

	protected override void OnStartQuestInternal()
	{
		InitializeQuestParty();
		((QuestBase)this).AddLog(QuestStartLogText, false);
		((QuestBase)this).AddTrackedObject((ITrackableCampaignObject)(object)_corsairHuntingGroundMarker);
	}

	protected override void HourlyTick()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		float seeingRange = MobileParty.MainParty.SeeingRange;
		CampaignVec2 corsairSpawnPosition = _corsairSpawnPosition;
		if (seeingRange > ((CampaignVec2)(ref corsairSpawnPosition)).Distance(MobileParty.MainParty.Position))
		{
			((QuestBase)this).AddLog(QuestSuccessLogText, false);
			_corsairHuntingGroundMarker.IsVisibleOnMap = false;
			Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
			((QuestBase)new HuntDownTheEmiraAlFahdaAndTheCorsairsQuest("naval_storyline_act3_quest2_2", NavalStorylineData.Gunnar, _corsairSpawnPosition)).StartQuest();
			TextObject val = new TextObject("{=tBigbw3U}You have reached the Gulf of Charas. Winds whip across the waves, carrying dust from the deserts, and visibility comes and goes. Lahar's ship keeps station several bowshots off of your port side, and together you comb the seas for the corsairs.", (Dictionary<string, object>)null);
			InformationManager.ShowInquiry(new InquiryData("", ((object)val).ToString(), true, false, ((object)GameTexts.FindText("str_continue", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			_willProgressStoryline = false;
			((QuestBase)this).CompleteQuestWithSuccess();
		}
	}

	protected override void IsNavalQuestPartyInternal(PartyBase party, NavalStorylinePartyData data)
	{
		if (party == PartyBase.MainParty)
		{
			data.PartySize++;
		}
	}

	protected override void RegisterEventsInternal()
	{
	}

	protected override void OnFinalizeInternal()
	{
	}

	protected override void OnCanceledInternal()
	{
		EnterSettlementAction.ApplyForCharacterOnly(NavalStorylineData.Lahar, NavalStorylineData.HomeSettlement);
		NavalStorylineData.Lahar.Heal(NavalStorylineData.Lahar.MaxHitPoints, false);
	}

	private void InitializeQuestParty()
	{
		NavalStorylineData.Lahar.ChangeState((CharacterStates)1);
		NavalStorylineData.Lahar.Heal(NavalStorylineData.Lahar.MaxHitPoints, false);
		AddHeroToPartyAction.Apply(NavalStorylineData.Lahar, MobileParty.MainParty, true);
		foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
		{
			if (((MBObjectBase)item.ShipHull).StringId == "ship_liburna_q2_storyline")
			{
				item.ChangeFigurehead(DefaultFigureheads.Hawk);
				AddShipUpgradePieces(item, LaharShipUpgradePieces);
			}
			else if (((MBObjectBase)item.ShipHull).StringId == "northern_medium_ship")
			{
				item.ChangeFigurehead(DefaultFigureheads.Dragon);
				AddShipUpgradePieces(item, GunnarShipUpgradePieces);
			}
		}
	}

	private void AddShipUpgradePieces(Ship ship, Dictionary<string, string> upgradePieces)
	{
		foreach (KeyValuePair<string, string> kv in upgradePieces)
		{
			ShipUpgradePiece val = MBObjectManager.Instance.GetObject<ShipUpgradePiece>(kv.Value);
			if (((IEnumerable<KeyValuePair<string, ShipSlot>>)ship.ShipHull.AvailableSlots).Any((KeyValuePair<string, ShipSlot> slot) => slot.Key == kv.Key))
			{
				ship.EquipUpgradePiece(kv.Key, val);
			}
		}
	}
}
