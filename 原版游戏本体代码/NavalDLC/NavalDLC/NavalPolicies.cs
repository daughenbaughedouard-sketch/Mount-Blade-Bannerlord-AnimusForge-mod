using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC;

public class NavalPolicies
{
	private PolicyObject _policyFraternalFleetDoctrine;

	private PolicyObject _policyKingsTitheOnKeels;

	private PolicyObject _policyRoyalRansomClaim;

	private PolicyObject _policyRoyalNavyPrerogative;

	private PolicyObject _policyMaritimeWealEdict;

	private PolicyObject _policyKingsPardonForPirates;

	private PolicyObject _policyRaidersSpoils;

	private PolicyObject _policyCoastalGuardEdict;

	private PolicyObject _policyBolsterTheFyrd;

	private PolicyObject _policyNavalConjoiningStatute;

	private PolicyObject _policyArsenalDepositoryAct;

	private static NavalPolicies Instance => NavalDLCManager.Instance.NavalPolicies;

	public static PolicyObject FraternalFleetDoctrine => Instance._policyFraternalFleetDoctrine;

	public static PolicyObject KingsTitheOnKeels => Instance._policyKingsTitheOnKeels;

	public static PolicyObject RoyalRansomClaim => Instance._policyRoyalRansomClaim;

	public static PolicyObject RoyalNavyPrerogative => Instance._policyRoyalNavyPrerogative;

	public static PolicyObject MaritimeWealEdict => Instance._policyMaritimeWealEdict;

	public static PolicyObject KingsPardonForPirates => Instance._policyKingsPardonForPirates;

	public static PolicyObject RaidersSpoils => Instance._policyRaidersSpoils;

	public static PolicyObject CoastalGuardEdict => Instance._policyCoastalGuardEdict;

	public static PolicyObject BolsterTheFyrd => Instance._policyBolsterTheFyrd;

	public static PolicyObject NavalConjoiningStatute => Instance._policyNavalConjoiningStatute;

	public static PolicyObject ArsenalDepositoryAct => Instance._policyArsenalDepositoryAct;

	public NavalPolicies()
	{
		RegisterAll();
		InitializeAll();
	}

	private void RegisterAll()
	{
		_policyFraternalFleetDoctrine = Create("policy_fraternal_fleet_doctrine");
		_policyKingsTitheOnKeels = Create("policy_kings_tithe_on_keels");
		_policyRoyalRansomClaim = Create("policy_royal_ransom_claim");
		_policyRoyalNavyPrerogative = Create("policy_royal_navy_prerogative");
		_policyMaritimeWealEdict = Create("policy_maritime_weal_edict");
		_policyKingsPardonForPirates = Create("policy_Kings_pardon_for_pirates");
		_policyRaidersSpoils = Create("policy_raiders_spoils");
		_policyCoastalGuardEdict = Create("policy_coastal_guard_edict");
		_policyBolsterTheFyrd = Create("policy_bolster_the_fyrd");
		_policyNavalConjoiningStatute = Create("policy_naval_conjoining_statute");
		_policyArsenalDepositoryAct = Create("policy_arsenal_depository_act");
	}

	private static PolicyObject Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<PolicyObject>(new PolicyObject(stringId));
	}

	private void InitializeAll()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0046: Expected O, but got Unknown
		//IL_0046: Expected O, but got Unknown
		//IL_0046: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_008c: Expected O, but got Unknown
		//IL_008c: Expected O, but got Unknown
		//IL_008c: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00d2: Expected O, but got Unknown
		//IL_00d2: Expected O, but got Unknown
		//IL_00d2: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0118: Expected O, but got Unknown
		//IL_0118: Expected O, but got Unknown
		//IL_0118: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		//IL_015e: Expected O, but got Unknown
		//IL_015e: Expected O, but got Unknown
		//IL_015e: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01a4: Expected O, but got Unknown
		//IL_01a4: Expected O, but got Unknown
		//IL_01a4: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Expected O, but got Unknown
		//IL_01ea: Expected O, but got Unknown
		//IL_01ea: Expected O, but got Unknown
		//IL_01ea: Expected O, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		//IL_0230: Expected O, but got Unknown
		//IL_0230: Expected O, but got Unknown
		//IL_0230: Expected O, but got Unknown
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Expected O, but got Unknown
		//IL_0276: Expected O, but got Unknown
		//IL_0276: Expected O, but got Unknown
		//IL_0276: Expected O, but got Unknown
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Expected O, but got Unknown
		//IL_02bc: Expected O, but got Unknown
		//IL_02bc: Expected O, but got Unknown
		//IL_02bc: Expected O, but got Unknown
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Expected O, but got Unknown
		//IL_0302: Expected O, but got Unknown
		//IL_0302: Expected O, but got Unknown
		//IL_0302: Expected O, but got Unknown
		_policyFraternalFleetDoctrine.Initialize(new TextObject("{=wNt5Bfkb}Auxiliaries to the Fleet", (Dictionary<string, object>)null), new TextObject("{=BhGsrGwR}Troops are required to spend part of their time training at sea, practicing shooting and fighting on an unstable deck.", (Dictionary<string, object>)null), new TextObject("{=vJcdw5Ht}requiring troops to train to fight at sea.", (Dictionary<string, object>)null), new TextObject("{=BaTU5lic}Naval combat morale of lord parties is increased by 20%{newline}Troop XP gain is reduced by 15%", (Dictionary<string, object>)null), -0.7f, 0.1f, -0.8f);
		_policyKingsTitheOnKeels.Initialize(new TextObject("{=lydo4bTx}Tithe on Keels", (Dictionary<string, object>)null), new TextObject("{=5r88g5cS}The ruler is given a share of the revenue whenever one of the great houses of the realm sells a ship.", (Dictionary<string, object>)null), new TextObject("{=4wBzaO3j}allowing the ruler to collect a share of the revenue from the sales of ships.", (Dictionary<string, object>)null), new TextObject("{=UMacLoNj}The ruler's clan receives 15% of the revenue from ship sales of other clans.", (Dictionary<string, object>)null), 0.9f, -0.5f, -0.5f);
		_policyRoyalRansomClaim.Initialize(new TextObject("{=nSgFcRRj}Royal Ransom Claim", (Dictionary<string, object>)null), new TextObject("{=oPHK8IQh}The ruler is granted a share of all ransoms collected by the lords of the realm.", (Dictionary<string, object>)null), new TextObject("{=akNKrmKX}granting the ruler a share of all ransoms.", (Dictionary<string, object>)null), new TextObject("{=JBN6C6Nb}The ruling clan collects a 15% commission on ransom payments to other clans.", (Dictionary<string, object>)null), 0.8f, -0.6f, -0.4f);
		_policyRoyalNavyPrerogative.Initialize(new TextObject("{=njYCvNGT}Royal Navy Prerogative", (Dictionary<string, object>)null), new TextObject("{=RH9vo6xc}The ruler has the right to purchase the finest ship lumber and fittings from smiths and carpenters at a fixed price.", (Dictionary<string, object>)null), new TextObject("{=lgAVWSwJ}granting the ruler the right to purchase lumber and fittings at a discount.", (Dictionary<string, object>)null), new TextObject("{=2F0GnVTR}Purchase price and upgrade costs of the ruling clan's ships is decreased by 10%{newline}Wood and smithy workshop output is decreased by 5% kingdom-wide.", (Dictionary<string, object>)null), 0.5f, -0.6f, -0.6f);
		_policyMaritimeWealEdict.Initialize(new TextObject("{=4aDWXjRX}Maritime Weal Edict", (Dictionary<string, object>)null), new TextObject("{=DZ3R3ROK}The edict requires inland towns to support coastal settlements with materials and trained workmen.", (Dictionary<string, object>)null), new TextObject("{=xcJON23P}issuing an edict granting special privileges to enterprises in coastal towns.", (Dictionary<string, object>)null), new TextObject("{=HzrCHG0N}Production in coastal settlement workshops and their bounded villages is increased by 25%{newline}Settlement project building speed in non-coastal towns is decreased by 20%", (Dictionary<string, object>)null), 0.1f, 0.5f, -0.4f);
		_policyKingsPardonForPirates.Initialize(new TextObject("{=ITgQ4Le0}Amnesties for Pirates", (Dictionary<string, object>)null), new TextObject("{=ShbbtHrT}Local magistrates are authorized to issue pardons to pirates in the name of the ruler, bringing their members into the garrison. This will reduce the immediate threat to maritime trade, but could also undercut deterrence.", (Dictionary<string, object>)null), new TextObject("{=qWBxsOti}allowing officials to issue pardons to pirates.", (Dictionary<string, object>)null), new TextObject("{=amZ3pGbZ}Each day, a pirate party operating within the kingdom's coastline has a 5% chance to surrender to the nearest coastal town. Upon surrendering, its ships are donated to the town's shipyard, and some of its members join the town's garrison.{newline}For each surrendered pirate party that settlement's security is immediately decreased by 5.", (Dictionary<string, object>)null), 0.2f, 0.1f, -0.3f);
		_policyRaidersSpoils.Initialize(new TextObject("{=1cLKUsq6}Writs of Reprisal", (Dictionary<string, object>)null), new TextObject("{=tprteUZD}Lords are issued commendations for successful raids on enemy territory, building support for long-ranging pillaging expeditions but also attracting a more lawless element to the armies.", (Dictionary<string, object>)null), new TextObject("{=32yg996m}encouraging lords to pillage enemy territory.", (Dictionary<string, object>)null), new TextObject("{=5JhZWebu}Successfully raiding a village grants the raiding clan +5 influence.{newline}For each lord party currently staying in a town owned by the kingdom, town security is reduced by 1 daily", (Dictionary<string, object>)null), -0.8f, -0.1f, 0.6f);
		_policyCoastalGuardEdict.Initialize(new TextObject("{=YXzJMHPx}Coastal Guard Edict", (Dictionary<string, object>)null), new TextObject("{=kC2dxF1F}Towns are given extra funds to keep a small squadron of ships standing by, ready to sail forth into coastal waters to assist against enemy fleets or pirates.", (Dictionary<string, object>)null), new TextObject("{=cyzUKVhI}providing towns with finances to maintain small coastal squadrons", (Dictionary<string, object>)null), new TextObject("{=AvPX6JUF}A coastal guard force stands ready to assist allies in battles within the town's territorial waters{newline}Coastal town garrisons wages are increased by 15%", (Dictionary<string, object>)null), -0.1f, -0.1f, 0.4f);
		_policyBolsterTheFyrd.Initialize(new TextObject("{=mumHAaVd}Bolster the Militia", (Dictionary<string, object>)null), new TextObject("{=ZQUwHH7T}This act requires villagers to provide stores to the local militia, including weapons, food, clothing and pack animals.", (Dictionary<string, object>)null), new TextObject("{=swkQVma8}requiring villages to set aside some goods for the militia.", (Dictionary<string, object>)null), new TextObject("{=6fuARR0S}Kingdom-wide militia generation boost: +25%{newline}Kingdom-wide village production penalty: -5%", (Dictionary<string, object>)null), -0.2f, 0.4f, -0.1f);
		_policyNavalConjoiningStatute.Initialize(new TextObject("{=WKaTn7zA}Naval Wardenships", (Dictionary<string, object>)null), new TextObject("{=XBzBDoga}This statute grants special titles and ceremonial rights to lords who own powerful warships, to aid in the defense of the seas.", (Dictionary<string, object>)null), new TextObject("{=cWYn4vt4}encouraging lords to maintain powerful warships.", (Dictionary<string, object>)null), new TextObject("{=FpPhYbAk}Clans with a heavy ship gain +1 influence daily.{newline}Clans possessing no heavy or medium ships lose -1 influence daily", (Dictionary<string, object>)null), 0f, 0.2f, -0.8f);
		_policyArsenalDepositoryAct.Initialize(new TextObject("{=eVr68fw7}Arsenals of State", (Dictionary<string, object>)null), new TextObject("{=tlbrO7bJ}The act creates arsenals to stockpile naval supplies at all ports, providing timber, ropes, tar and sailcloth to any nobles building ships.", (Dictionary<string, object>)null), new TextObject("{=GvXCPsq8}establishing arsenals to stockpile naval supplies.", (Dictionary<string, object>)null), new TextObject("{=a9qqNqSS}All clans within the kingdom benefit from a -15% reduction in ship purchase costs on own kingdom's ports.{newline}-10% tariff income", (Dictionary<string, object>)null), -0.2f, 0.1f, 0.6f);
	}
}
