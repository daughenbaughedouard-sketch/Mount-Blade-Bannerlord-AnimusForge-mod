using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Storyline.Quests;

public class GoToSkatriaIslandsQuest : NavalStorylineQuestBase
{
	private static readonly Dictionary<string, string> PlayerShipUpgradePieces = new Dictionary<string, string>
	{
		{ "sail", "sails_lvl2" },
		{ "side", "side_northern_shields_lvl2" }
	};

	[SaveableField(1)]
	private CampaignVec2 _corsairSpawnPosition;

	[SaveableField(2)]
	private readonly MapMarker _skatriaIslandMarker;

	[SaveableField(3)]
	private bool _willProgressStoryline;

	public override TextObject Title => new TextObject("{=HEpykTDR}Go to the Skatria Islands", (Dictionary<string, object>)null);

	private TextObject QuestSuccessLogText => new TextObject("{=U6O5y26b}You found the Skatria Islands.", (Dictionary<string, object>)null);

	public override NavalStorylineData.NavalStorylineStage Stage => NavalStorylineData.NavalStorylineStage.Act3Quest4;

	public override bool WillProgressStoryline => _willProgressStoryline;

	protected override string MainPartyTemplateStringId => "storyline_act3_quest_4_main_party_template";

	private TextObject QuestStartLogText
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			TextObject val = new TextObject("{=5ygak6Ob}Sail to the Skatria Islands off {SETTLEMENT_NAME}", (Dictionary<string, object>)null);
			val.SetTextVariable("SETTLEMENT_NAME", NavalStorylineData.Act3Quest4TargetSettlement.Name);
			return val;
		}
	}

	public GoToSkatriaIslandsQuest(string questId, Hero questGiver, CampaignVec2 corsairSpawnPosition)
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
		_skatriaIslandMarker = Campaign.Current.MapMarkerManager.CreateMapMarker(NavalStorylineData.CorsairBanner, new TextObject("{=9EIh8xRM}Skatria Islands", (Dictionary<string, object>)null), ((CampaignVec2)(ref _corsairSpawnPosition)).AsVec3(), true, ((MBObjectBase)this).StringId);
	}

	protected override void RegisterEventsInternal()
	{
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)OnTick);
	}

	protected override void SetDialogs()
	{
	}

	protected override void InitializeQuestOnGameLoadInternal()
	{
	}

	protected override void OnStartQuestInternal()
	{
		InitializeQuestParty();
		((QuestBase)this).AddLog(QuestStartLogText, false);
		_skatriaIslandMarker.IsVisibleOnMap = true;
	}

	protected override void OnFinalizeInternal()
	{
		base.OnFinalizeInternal();
		_skatriaIslandMarker.IsVisibleOnMap = false;
	}

	private void InitializeQuestParty()
	{
		NavalStorylineData.Bjolgur.ChangeState((CharacterStates)1);
		AddHeroToPartyAction.Apply(NavalStorylineData.Bjolgur, MobileParty.MainParty, true);
		foreach (Ship item in (List<Ship>)(object)MobileParty.MainParty.Ships)
		{
			foreach (KeyValuePair<string, string> playerShipUpgradePiece in PlayerShipUpgradePieces)
			{
				if (item.HasSlot(playerShipUpgradePiece.Key))
				{
					item.EquipUpgradePiece(playerShipUpgradePiece.Key, MBObjectManager.Instance.GetObject<ShipUpgradePiece>(playerShipUpgradePiece.Value));
				}
			}
			item.ChangeFigurehead(DefaultFigureheads.Raven);
		}
		Ship val = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).FirstOrDefault();
		if (val != null)
		{
			val.ChangeFigurehead(DefaultFigureheads.Dragon);
		}
	}

	private void OnTick(float deltaTime)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (MobileParty.MainParty.SeeingRange + 5f > ((CampaignVec2)(ref _corsairSpawnPosition)).Distance(MobileParty.MainParty.Position) && !Campaign.Current.QuestManager.IsThereActiveQuestWithType(typeof(CaptureTheImperialMerchantPrusas)))
		{
			((QuestBase)this).AddLog(QuestSuccessLogText, false);
			_willProgressStoryline = false;
			_skatriaIslandMarker.IsVisibleOnMap = false;
			((QuestBase)this).CompleteQuestWithSuccess();
			((QuestBase)new CaptureTheImperialMerchantPrusas("naval_storyline_act3_quest4_2", NavalStorylineData.Gunnar, _corsairSpawnPosition)).StartQuest();
		}
	}
}
