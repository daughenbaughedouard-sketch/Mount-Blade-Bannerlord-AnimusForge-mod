using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.View.Map;

public class MapGamepadEffectsView : MapView
{
	private readonly float[] _lowFrequencyLevels = new float[5];

	private readonly float[] _lowFrequencyDurations = new float[5];

	private readonly float[] _highFrequencyLevels = new float[5];

	private readonly float[] _highFrequencyDurations = new float[5];

	protected internal override void CreateLayout()
	{
		base.CreateLayout();
		RegisterEvents();
	}

	protected internal override void OnFinalize()
	{
		base.OnFinalize();
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
		CampaignEvents.VillageBeingRaided.AddNonSerializedListener((object)this, (Action<Village>)OnVillageRaid);
		CampaignEvents.OnSiegeBombardmentWallHitEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, BattleSideEnum, SiegeEngineType, bool>)OnSiegeBombardmentWallHit);
		CampaignEvents.OnSiegeEngineDestroyedEvent.AddNonSerializedListener((object)this, (Action<MobileParty, Settlement, BattleSideEnum, SiegeEngineType>)OnSiegeEngineDestroyed);
		CampaignEvents.WarDeclared.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, DeclareWarDetail>)OnWarDeclared);
		CampaignEvents.OnPeaceOfferedToPlayerEvent.AddNonSerializedListener((object)this, (Action<IFaction, int, int>)OnPeaceOfferedToPlayer);
		CampaignEvents.ArmyDispersed.AddNonSerializedListener((object)this, (Action<Army, ArmyDispersionReason, bool>)OnArmyDispersed);
		CampaignEvents.HeroLevelledUp.AddNonSerializedListener((object)this, (Action<Hero, bool>)OnHeroLevelUp);
		CampaignEvents.KingdomDecisionAdded.AddNonSerializedListener((object)this, (Action<KingdomDecision, bool>)OnKingdomDecisionAdded);
		CampaignEvents.OnMainPartyStarvingEvent.AddNonSerializedListener((object)this, (Action)OnMainPartyStarving);
		CampaignEvents.RebellionFinished.AddNonSerializedListener((object)this, (Action<Settlement, Clan>)OnRebellionFinished);
		CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase>)OnHideoutSpotted);
		CampaignEvents.HeroCreated.AddNonSerializedListener((object)this, (Action<Hero, bool>)OnHeroCreated);
		CampaignEvents.MakePeace.AddNonSerializedListener((object)this, (Action<IFaction, IFaction, MakePeaceDetail>)OnMakePeace);
		CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener((object)this, (Action<(Hero, PartyBase), (Hero, PartyBase), (int, string), bool>)OnHeroOrPartyTradedGold);
		CampaignEvents.PartyAttachedAnotherParty.AddNonSerializedListener((object)this, (Action<MobileParty>)OnPartyAttachedAnotherParty);
	}

	private void UnregisterEvents()
	{
		((IMbEventBase)CampaignEvents.VillageBeingRaided).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnSiegeBombardmentWallHitEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnSiegeEngineDestroyedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.WarDeclared).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnPeaceOfferedToPlayerEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.ArmyDispersed).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.HeroLevelledUp).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.KingdomDecisionAdded).ClearListeners((object)this);
		CampaignEvents.OnMainPartyStarvingEvent.ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.RebellionFinished).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnHideoutSpottedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.HeroCreated).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.MakePeace).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.HeroOrPartyTradedGold).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.PartyAttachedAnotherParty).ClearListeners((object)this);
	}

	private void OnVillageRaid(Village village)
	{
		if (MobileParty.MainParty.CurrentSettlement == ((SettlementComponent)village).Settlement)
		{
			SetRumbleWithRandomValues(0.2f, 0.4f);
		}
	}

	private void OnSiegeBombardmentWallHit(MobileParty besiegerParty, Settlement besiegedSettlement, BattleSideEnum side, SiegeEngineType weapon, bool isWallCracked)
	{
		if (isWallCracked && (besiegerParty == MobileParty.MainParty || besiegedSettlement == MobileParty.MainParty.CurrentSettlement))
		{
			SetRumbleWithRandomValues(0.3f, 0.8f);
		}
	}

	private void OnSiegeEngineDestroyed(MobileParty besiegerParty, Settlement besiegedSettlement, BattleSideEnum side, SiegeEngineType destroyedEngine)
	{
		if (besiegerParty == MobileParty.MainParty || besiegedSettlement == MobileParty.MainParty.CurrentSettlement)
		{
			SetRumbleWithRandomValues(0.05f, 0.3f, 4);
		}
	}

	private void OnWarDeclared(IFaction faction1, IFaction faction2, DeclareWarDetail declareWarDetail)
	{
		if (faction1 == Clan.PlayerClan.MapFaction || faction2 == Clan.PlayerClan.MapFaction)
		{
			SetRumbleWithRandomValues(0.3f, 0.5f, 3);
		}
	}

	private void OnPeaceOfferedToPlayer(IFaction opponentFaction, int tributeAmount, int tributeDurationInDays)
	{
		SetRumbleWithRandomValues(0.2f, 0.4f, 3);
	}

	private void OnArmyDispersed(Army army, ArmyDispersionReason reason, bool isPlayersArmy)
	{
		if (isPlayersArmy)
		{
			SetRumbleWithRandomValues((float)army.TotalManCount / 2000f, (float)army.TotalManCount / 1000f);
		}
	}

	private void OnHeroLevelUp(Hero hero, bool shouldNotify)
	{
		if (hero == Hero.MainHero && !(GameStateManager.Current.ActiveState is GameLoadingState))
		{
			SetRumbleWithRandomValues(0.1f, 0.3f, 3);
		}
	}

	private void OnKingdomDecisionAdded(KingdomDecision decision, bool isPlayerInvolved)
	{
		if (isPlayerInvolved)
		{
			SetRumbleWithRandomValues(0.1f, 0.3f, 2);
		}
	}

	private void OnMainPartyStarving()
	{
		SetRumbleWithRandomValues(0.2f, 0.4f);
	}

	private void OnRebellionFinished(Settlement settlement, Clan oldOwnerClan)
	{
		if (oldOwnerClan == Clan.PlayerClan)
		{
			SetRumbleWithRandomValues(0.2f, 0.4f);
		}
	}

	private void OnHideoutSpotted(PartyBase party, PartyBase hideoutParty)
	{
		SetRumbleWithRandomValues(0.1f, 0.3f, 3);
	}

	private void OnHeroCreated(Hero hero, bool isBornNaturally = false)
	{
		if (hero.Father == Hero.MainHero || hero.Mother == Hero.MainHero)
		{
			SetRumbleWithRandomValues(0.2f, 0.4f, 3);
		}
	}

	private void OnMakePeace(IFaction side1Faction, IFaction side2Faction, MakePeaceDetail detail)
	{
		if (side1Faction == Clan.PlayerClan.MapFaction || side2Faction == Clan.PlayerClan.MapFaction)
		{
			SetRumbleWithRandomValues(0.2f, 0.4f, 3);
		}
	}

	private void OnHeroOrPartyTradedGold((Hero, PartyBase) giver, (Hero, PartyBase) recipient, (int, string) goldAmount, bool showNotification)
	{
		if (giver.Item1 == Hero.MainHero && Hero.MainHero.Gold == 0)
		{
			SetRumbleWithRandomValues(0.1f, 0.3f, 3);
		}
	}

	private void OnPartyAttachedAnotherParty(MobileParty party)
	{
		if (party.Army != null && party.Army.LeaderParty == MobileParty.MainParty)
		{
			SetRumbleWithRandomValues(0.1f, 0.3f, 3);
		}
	}

	protected internal override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (Input.IsKeyDown((InputKey)14))
		{
			SetRumbleWithRandomValues(0.5f, 0f);
		}
	}

	private void SetRumbleWithRandomValues(float baseValue = 0f, float offsetRange = 1f, int frequencyCount = 5)
	{
		SetRandomRumbleValues(baseValue, offsetRange, frequencyCount);
	}

	private void SetRandomRumbleValues(float baseValue, float offsetRange, int frequencyCount)
	{
		baseValue = MBMath.ClampFloat(baseValue, 0f, 1f);
		offsetRange = MBMath.ClampFloat(offsetRange, 0f, 1f - baseValue);
		frequencyCount = MBMath.ClampInt(frequencyCount, 2, 5);
		for (int i = 0; i < frequencyCount; i++)
		{
			_lowFrequencyLevels[i] = baseValue + MBRandom.RandomFloatRanged(offsetRange);
			_lowFrequencyDurations[i] = baseValue + MBRandom.RandomFloatRanged(offsetRange);
			_highFrequencyLevels[i] = baseValue + MBRandom.RandomFloatRanged(offsetRange);
			_highFrequencyDurations[i] = baseValue + MBRandom.RandomFloatRanged(offsetRange);
		}
	}
}
