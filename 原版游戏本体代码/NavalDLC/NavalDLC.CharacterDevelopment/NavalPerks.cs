using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;

namespace NavalDLC.CharacterDevelopment;

public class NavalPerks
{
	public class Mariner
	{
		public static PerkObject RollingThunder => Instance._rollingThunder;

		public static PerkObject PiratesProwess => Instance._piratesProwess;

		public static PerkObject Forceful => Instance._forceful;

		public static PerkObject BruteForce => Instance._bruteForce;

		public static PerkObject AxeOfTheNorthwind => Instance._axeOfTheNorthwind;

		public static PerkObject SunnyDisposition => Instance._sunnyDisposition;

		public static PerkObject EnemyOfTheWood => Instance._enemyOfTheWood;

		public static PerkObject NavalFightingTraining => Instance._navalFightingTraining;

		public static PerkObject TerrorOfTheSeas => Instance._terrorOfTheSeas;

		public static PerkObject RallyingCry => Instance._rallyingCry;

		public static PerkObject ShatteringBlow => Instance._shatteringBlow;

		public static PerkObject ShatteringVolley => Instance._shatteringVolley;

		public static PerkObject Arr => Instance._arr;

		public static PerkObject PirateHunter => Instance._pirateHunter;

		public static PerkObject BoardingMaster => Instance._boardingMaster;

		public static PerkObject HomeTurfAdvantage => Instance._homeTurfAdvantage;

		public static PerkObject MightyBlows => Instance._mightyBlows;

		public static PerkObject CrewOfSpears => Instance._crewOfSpears;

		public static PerkObject TheSkysFury => Instance._theSkysFury;

		public static PerkObject WarriorsMight => Instance._warriorsMight;
	}

	public class Boatswain
	{
		public static PerkObject MerchantPrince => Instance._merchantPrince;

		public static PerkObject MasterShipwright => Instance._masterShipwright;

		public static PerkObject StreamlinedOperations => Instance._streamlinedOperations;

		public static PerkObject WellStocked => Instance._wellStocked;

		public static PerkObject NavalHorde => Instance._navalHorde;

		public static PerkObject Optimization => Instance._optimization;

		public static PerkObject GildedPurse => Instance._gildedPurse;

		public static PerkObject VeteransWisdom => Instance._veteransWisdom;

		public static PerkObject ShipwrightsInsight => Instance._shipwrightsInsight;

		public static PerkObject SpecialArrows => Instance._specialArrows;

		public static PerkObject SmoothOperator => Instance._smoothOperator;

		public static PerkObject AccuracyTraining => Instance._accuracyTraining;

		public static PerkObject EfficientCaptain => Instance._efficientCaptain;

		public static PerkObject PopularCaptain => Instance._popularCaptain;

		public static PerkObject PortAuthority => Instance._portAuthority;

		public static PerkObject BlessingsOfTheSea => Instance._blessingsOfTheSea;

		public static PerkObject ShipwrightsHand => Instance._shipwrightsHand;

		public static PerkObject Salvage => Instance._salvage;

		public static PerkObject MerchantFleet => Instance._merchantFleet;

		public static PerkObject Resilience => Instance._resilience;

		public static PerkObject NavalBombardment => Instance._navalBombardment;
	}

	public class Shipmaster
	{
		public static PerkObject MasterAngler => Instance._masterAngler;

		public static PerkObject OldSaltsTouch => Instance._oldSaltsTouch;

		public static PerkObject GhostShip => Instance._ghostShip;

		public static PerkObject WindRider => Instance._windRider;

		public static PerkObject RiverRaider => Instance._riverRaider;

		public static PerkObject NightRaider => Instance._nightRaider;

		public static PerkObject Windborne => Instance._windborne;

		public static PerkObject ShockAndAwe => Instance._shockAndAwe;

		public static PerkObject TheHelmsmansShield => Instance._theHelmsmansShield;

		public static PerkObject RavenEye => Instance._ravenEye;

		public static PerkObject FairWinds => Instance._fairWinds;

		public static PerkObject FavorableTide => Instance._favorableTide;

		public static PerkObject Unflinching => Instance._unflinching;

		public static PerkObject ShoreMaster => Instance._shoreMaster;

		public static PerkObject FleetCommander => Instance._fleetCommander;

		public static PerkObject ChainToOars => Instance._chainToOars;

		public static PerkObject Stormrider => Instance._stormrider;

		public static PerkObject MasterAndCommander => Instance._masterAndCommander;

		public static PerkObject TheCorsairsEdge => Instance._theCorsairsEdge;

		public static PerkObject SeaborneFortress => Instance._seaborneFortress;

		public static PerkObject Commodore => Instance._commodore;
	}

	private static readonly int[] TierSkillRequirements = new int[12]
	{
		25, 50, 75, 100, 125, 150, 175, 200, 225, 250,
		275, 300
	};

	private PerkObject _rollingThunder;

	private PerkObject _piratesProwess;

	private PerkObject _forceful;

	private PerkObject _bruteForce;

	private PerkObject _axeOfTheNorthwind;

	private PerkObject _sunnyDisposition;

	private PerkObject _enemyOfTheWood;

	private PerkObject _navalFightingTraining;

	private PerkObject _terrorOfTheSeas;

	private PerkObject _rallyingCry;

	private PerkObject _shatteringBlow;

	private PerkObject _shatteringVolley;

	private PerkObject _arr;

	private PerkObject _pirateHunter;

	private PerkObject _boardingMaster;

	private PerkObject _homeTurfAdvantage;

	private PerkObject _mightyBlows;

	private PerkObject _crewOfSpears;

	private PerkObject _theSkysFury;

	private PerkObject _warriorsMight;

	private PerkObject _merchantPrince;

	private PerkObject _masterShipwright;

	private PerkObject _streamlinedOperations;

	private PerkObject _wellStocked;

	private PerkObject _navalHorde;

	private PerkObject _optimization;

	private PerkObject _gildedPurse;

	private PerkObject _veteransWisdom;

	private PerkObject _shipwrightsInsight;

	private PerkObject _specialArrows;

	private PerkObject _smoothOperator;

	private PerkObject _accuracyTraining;

	private PerkObject _efficientCaptain;

	private PerkObject _popularCaptain;

	private PerkObject _portAuthority;

	private PerkObject _blessingsOfTheSea;

	private PerkObject _shipwrightsHand;

	private PerkObject _salvage;

	private PerkObject _merchantFleet;

	private PerkObject _resilience;

	private PerkObject _navalBombardment;

	private PerkObject _masterAngler;

	private PerkObject _oldSaltsTouch;

	private PerkObject _ghostShip;

	private PerkObject _windRider;

	private PerkObject _riverRaider;

	private PerkObject _nightRaider;

	private PerkObject _windborne;

	private PerkObject _shockAndAwe;

	private PerkObject _theHelmsmansShield;

	private PerkObject _ravenEye;

	private PerkObject _fairWinds;

	private PerkObject _favorableTide;

	private PerkObject _unflinching;

	private PerkObject _shoreMaster;

	private PerkObject _fleetCommander;

	private PerkObject _chainToOars;

	private PerkObject _stormrider;

	private PerkObject _masterAndCommander;

	private PerkObject _theCorsairsEdge;

	private PerkObject _seaborneFortress;

	private PerkObject _commodore;

	private static NavalPerks Instance => NavalDLCManager.Instance.NavalPerks;

	public NavalPerks()
	{
		RegisterAll();
		InitializeAll();
	}

	private void RegisterAll()
	{
		_rollingThunder = Create("RollingThunder");
		_piratesProwess = Create("PiratesProwess");
		_forceful = Create("Forceful");
		_bruteForce = Create("BruteForce");
		_axeOfTheNorthwind = Create("AxeOfTheNorthwind");
		_sunnyDisposition = Create("SunnyDisposition");
		_enemyOfTheWood = Create("EnemyOfTheWood");
		_navalFightingTraining = Create("NavalFightingTraining");
		_terrorOfTheSeas = Create("TerrorOfTheSeas");
		_rallyingCry = Create("RallyingCry");
		_shatteringBlow = Create("ShatteringBlow");
		_shatteringVolley = Create("ShatteringVolley");
		_arr = Create("Arr");
		_pirateHunter = Create("PirateHunter");
		_boardingMaster = Create("BoardingMaster");
		_homeTurfAdvantage = Create("HomeTurfAdvantage");
		_mightyBlows = Create("MightyBlows");
		_crewOfSpears = Create("CrewOfSpears");
		_theSkysFury = Create("TheSkysFury");
		_warriorsMight = Create("WarriorsMight");
		_merchantPrince = Create("MerchantPrince");
		_masterShipwright = Create("MasterShipwright");
		_streamlinedOperations = Create("StreamlinedOperations");
		_wellStocked = Create("WellStocked");
		_navalHorde = Create("NavalHorde");
		_optimization = Create("Optimization");
		_gildedPurse = Create("GildedPurse");
		_veteransWisdom = Create("VeteransWisdom");
		_shipwrightsInsight = Create("ShipwrightsInsight");
		_specialArrows = Create("SpecialArrows");
		_smoothOperator = Create("SmoothOperator");
		_accuracyTraining = Create("Accuracytraining");
		_efficientCaptain = Create("EfficientCaptain");
		_popularCaptain = Create("PopularCaptain");
		_portAuthority = Create("PortAuthority");
		_blessingsOfTheSea = Create("BlessingsOfTheSea");
		_shipwrightsHand = Create("ShipwrightsHand");
		_salvage = Create("Salvage");
		_merchantFleet = Create("MerchantFleet");
		_resilience = Create("Resilience");
		_navalBombardment = Create("NavalBombardment");
		_masterAngler = Create("MasterAngler");
		_oldSaltsTouch = Create("OldSaltsTouch");
		_ghostShip = Create("GhostShip");
		_windRider = Create("WindRider");
		_riverRaider = Create("RiverRaider");
		_nightRaider = Create("NightRaider");
		_windborne = Create("Windborne");
		_shockAndAwe = Create("ShockAndAwe");
		_theHelmsmansShield = Create("TheHelmsmansShield");
		_ravenEye = Create("RavenEye");
		_fairWinds = Create("FairWinds");
		_favorableTide = Create("FavorableTide");
		_unflinching = Create("Unflinching");
		_shoreMaster = Create("ShoreMaster");
		_fleetCommander = Create("FleetCommander");
		_chainToOars = Create("ChainToOars");
		_stormrider = Create("Stormrider");
		_masterAndCommander = Create("MasterAndCommander");
		_theCorsairsEdge = Create("TheCorsairsEdge");
		_seaborneFortress = Create("SeaborneFortress");
		_commodore = Create("Commodore");
	}

	private void InitializeAll()
	{
		_rollingThunder.Initialize("{=AtNKfDDP}Rolling Thunder", NavalSkills.Mariner, GetTierCost(1), _piratesProwess, "{=aYaJGhh9}{VALUE}% accuracy penalty from ship roll.", (PartyRole)12, -0.3f, (EffectIncrementType)1, "{=mbcN7l2f}{VALUE}% conformity gain for pirate prisoners in your party while at sea.", (PartyRole)5, 0.5f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_piratesProwess.Initialize("{=csEbtEj5}Pirate's Prowess", NavalSkills.Mariner, GetTierCost(1), _rollingThunder, "{=7MtEpwKM}{VALUE}% melee weapon handling.", (PartyRole)12, 0.25f, (EffectIncrementType)1, "{=bNZPW3qe}{VALUE}% loot from defeated merchant convoys.", (PartyRole)5, 0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_forceful.Initialize("{=4f7RcvYj}Forceful", NavalSkills.Mariner, GetTierCost(2), _bruteForce, "{=F1CJo2wX}{VALUE}% shield hitpoints.", (PartyRole)12, 0.3f, (EffectIncrementType)1, "{=yfpyCOuu}{VALUE}% damage to shields dealt by crew.", (PartyRole)13, 0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_bruteForce.Initialize("{=DLcRb2jH}Brute Force", NavalSkills.Mariner, GetTierCost(2), _forceful, "{=Jbc2m29I}{VALUE}% kicking and bashing damage.", (PartyRole)12, 0.5f, (EffectIncrementType)1, "{=kbkKPD3A}{VALUE}% melee knock back dealt by crew.", (PartyRole)13, 0.5f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)4);
		_axeOfTheNorthwind.Initialize("{=mhzGQYl9}Axe of the North Wind", NavalSkills.Mariner, GetTierCost(3), _sunnyDisposition, "{=SlnKlVOl}{VALUE}% damage dealt by axes.", (PartyRole)12, 0.2f, (EffectIncrementType)1, "{=8STt46ci}{VALUE} morale for mariner troops at start of battle.", (PartyRole)5, 20f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_sunnyDisposition.Initialize("{=MKOmGiqt}Sunny Disposition", NavalSkills.Mariner, GetTierCost(3), _axeOfTheNorthwind, "{=VB9rTE73}{VALUE}% damage dealt by swords.", (PartyRole)12, 0.2f, (EffectIncrementType)1, "{=BYY8MiFe}{VALUE} morale for regular troops at start of battle.", (PartyRole)5, 20f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_enemyOfTheWood.Initialize("{=Tf7zOvfL}Enemy of the Wood", NavalSkills.Mariner, GetTierCost(4), _navalFightingTraining, "{=McqgoySh}{VALUE} morale to enemy for each ship destroyed in battle.", (PartyRole)4, -10f, (EffectIncrementType)0, "{=6YsSNwTW}{VALUE}% fire damage dealt by your ship and crew to enemy sails.", (PartyRole)13, 0.25f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_navalFightingTraining.Initialize("{=cvOhFtKn}Naval Fighting Training", NavalSkills.Mariner, GetTierCost(4), _enemyOfTheWood, "{=pRTSU12h}{VALUE}% to xp gained by party companions after each naval battle.", (PartyRole)5, 0.1f, (EffectIncrementType)1, "{=F8cJJlZn}{VALUE}% Increase to militia veterancy in coastal settlements.", (PartyRole)3, 0.1f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_terrorOfTheSeas.Initialize("{=nUUAag3J}Terror of the Seas", NavalSkills.Mariner, GetTierCost(5), _rallyingCry, "{=VAFEpyau}{VALUE}% to morale loss suffered by enemy ships in battle.", (PartyRole)5, 0.2f, (EffectIncrementType)1, "{=hMFAmWkE}{VALUE}% melee damage taken by crew while on enemy ships.", (PartyRole)13, -0.1f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_rallyingCry.Initialize("{=5S1QiUvh}Rallying Cry", NavalSkills.Mariner, GetTierCost(5), _terrorOfTheSeas, "{=GTY1d7RX}{VALUE}% morale boost for crew while on own ship.", (PartyRole)13, 0.2f, (EffectIncrementType)1, "{=GX5M0gbo}{VALUE}% melee damage taken by crew while on own ships.", (PartyRole)13, -0.1f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)0);
		_shatteringBlow.Initialize("{=mbaYZ0QB}Shattering Blow", NavalSkills.Mariner, GetTierCost(6), _shatteringVolley, "{=MUsv10MO}{VALUE}% armor penetration for melee weapons.", (PartyRole)12, 0.5f, (EffectIncrementType)1, "{=vmBVfzVL}{VALUE}% armor penetration for melee weapons wielded by crew.", (PartyRole)13, 0.5f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)4);
		_shatteringVolley.Initialize("{=InUgc3PT}Shattering Volley", NavalSkills.Mariner, GetTierCost(6), _shatteringBlow, "{=pKk0fKba}{VALUE}% armor penetration for ranged weapons.", (PartyRole)12, 0.5f, (EffectIncrementType)1, "{=MzQBOs13}{VALUE}% armor penetration for ranged weapons wielded by crew.", (PartyRole)13, 0.5f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)8);
		_arr.Initialize("{=OlvwVG3b}Arr!", NavalSkills.Mariner, GetTierCost(7), _pirateHunter, "{=Sa7FPVnT}Surrendering pirate parties can be recruited.", (PartyRole)12, 0f, (EffectIncrementType)0, "{=eHg3h3j7}{VALUE}% xp gain after each battle for mariner troops under character's command.", (PartyRole)13, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_pirateHunter.Initialize("{=qlMgDT7y}Pirate Hunter", NavalSkills.Mariner, GetTierCost(7), _arr, "{=HnyLGFbu}{VALUE}% bonus when crew is sent to confront pirates.", (PartyRole)5, 0.2f, (EffectIncrementType)1, "{=sIOdlPOA}{VALUE}% xp gain after each battle for regular troops under character's command.", (PartyRole)13, 0.1f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_boardingMaster.Initialize("{=gkJ1fRSM}Boarding Master", NavalSkills.Mariner, GetTierCost(8), _homeTurfAdvantage, "{=HAbSFYFz}{VALUE}% melee damage dealt by character when fighting on other ships.", (PartyRole)12, 0.15f, (EffectIncrementType)1, "{=pR8afW3c}{VALUE}% melee damage dealt by crew when fighting on other ships.", (PartyRole)13, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)4);
		_homeTurfAdvantage.Initialize("{=n5g7EvDQ}Home Turf Advantage", NavalSkills.Mariner, GetTierCost(8), _boardingMaster, "{=RLrenzWj}{VALUE}% melee damage dealt by character when fighting on own ship.", (PartyRole)12, 0.2f, (EffectIncrementType)1, "{=8Qp9IKuG}{VALUE}% melee damage dealt by crew when fighting on own ship.", (PartyRole)13, 0.2f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)4);
		_mightyBlows.Initialize("{=RSMwW4mr}Mighty Blows", NavalSkills.Mariner, GetTierCost(9), _crewOfSpears, "{=YbAddajR}Better cleave with two handed weapons swings. (Two handed weapons lose {VALUE}% less damage when they cut through the first opponent.)", (PartyRole)12, -0.5f, (EffectIncrementType)1, "{=fjettvEU}{VALUE}% melee damage dealt by crew armed with two-handed weapons.", (PartyRole)13, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)64);
		_crewOfSpears.Initialize("{=BDp8MzPJ}Crew of Spears", NavalSkills.Mariner, GetTierCost(9), _mightyBlows, "{=IQhRdMoc}Impale shields with thrown javelins, and throwing axes deals damage after if they break a shield.", (PartyRole)12, 0f, (EffectIncrementType)0, "{=wMwRA172}{VALUE}% ranged damage dealt by crew armed with throwing weapons.", (PartyRole)13, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)512);
		_theSkysFury.Initialize("{=fS6ZrhCH}The Sky's Fury", NavalSkills.Mariner, GetTierCost(10), _warriorsMight, "{=aP1gxbRF}{VALUE}% ranged damage dealt by character.", (PartyRole)12, 0.15f, (EffectIncrementType)1, "{=MHC4pkJE}{VALUE}% to bow and crossbow damage dealt by crew.", (PartyRole)13, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)1280);
		_warriorsMight.Initialize("{=CuNzwLc3}Warrior's Might", NavalSkills.Mariner, GetTierCost(10), _theSkysFury, "{=H7Hs2E3D}{VALUE}% melee damage dealt by character.", (PartyRole)12, 0.2f, (EffectIncrementType)1, "{=KvGLih9i}{VALUE}% to throwing damage dealt by crew.", (PartyRole)13, 0.2f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)512);
		_merchantPrince.Initialize("{=P79raYEW}Merchant Prince", NavalSkills.Boatswain, GetTierCost(1), _masterShipwright, "{=UL4LyWhF}{VALUE}% to ship repair cost", (PartyRole)14, -0.3f, (EffectIncrementType)1, "{=bQ5iyRiM}{VALUE} denars for each ship bought or sold in governed settlement.", (PartyRole)3, 500f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_masterShipwright.Initialize("{=DQ7KWQJq}Master Shipwright", NavalSkills.Boatswain, GetTierCost(1), _merchantPrince, "{=R0akt6jI}{VALUE}% to cost of ship upgrades", (PartyRole)14, -0.3f, (EffectIncrementType)1, "{=WvBaUVAr}{VALUE} denars from each ship repaired at governed settlement", (PartyRole)3, 30f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_streamlinedOperations.Initialize("{=oUtZ27Id}Streamlined Operations", NavalSkills.Boatswain, GetTierCost(2), _wellStocked, "{=Trsw67ag}{VALUE}% ballista reload time.", (PartyRole)14, 0.1f, (EffectIncrementType)1, "{=ZWOY78k2}{VALUE}% to shipyard production rate in governed settlement.", (PartyRole)3, 0.2f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)65535);
		_wellStocked.Initialize("{=ReTUWtie}Well Stocked", NavalSkills.Boatswain, GetTierCost(2), _streamlinedOperations, "{=Myea2YPh}{VALUE}% ammunition per stack for crew under command.", (PartyRole)14, 0.3f, (EffectIncrementType)1, "{=4XCAUAee}{VALUE} extra ammunition per stack for thrown weapons of the crew.", (PartyRole)14, 2f, (EffectIncrementType)0, (TroopUsageFlags)0, (TroopUsageFlags)512);
		_navalHorde.Initialize("{=1uWha4cw}Naval Horde", NavalSkills.Boatswain, GetTierCost(3), _optimization, "{=1aCQf9Xf}{VALUE}% to wages for cavalry troops while at sea.", (PartyRole)14, -0.3f, (EffectIncrementType)1, "{=9Hsd1fuX}{VALUE}% to weight of mounts when in ship's cargo.", (PartyRole)14, -0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_optimization.Initialize("{=ON5j1Gwp}Optimization", NavalSkills.Boatswain, GetTierCost(3), _navalHorde, "{=KVrphJkB}{VALUE}% to wages of non-mariner troops while at sea.", (PartyRole)14, -0.1f, (EffectIncrementType)1, "{=wdnSjdLE}{VALUE}% to weight of pack animals and livestock when in ship's cargo", (PartyRole)14, -0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_gildedPurse.Initialize("{=tXOmhbFz}Gilded Purse", NavalSkills.Boatswain, GetTierCost(4), _veteransWisdom, "{=xI8UK8Wp}{VALUE}% to chance of capturing ships after battle.", (PartyRole)5, 0.25f, (EffectIncrementType)1, "{=bUXtraYH}{VALUE}% to weight of trade goods in ship's cargo.", (PartyRole)14, -0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_veteransWisdom.Initialize("{=Nlz7g0GX}Veteran's Wisdom", NavalSkills.Boatswain, GetTierCost(4), _gildedPurse, "{=ziKgHMqy}Daily bonus of {VALUE}x character level xp to companions.", (PartyRole)5, 10f, (EffectIncrementType)0, "{=jzwCbxni}{VALUE}% overburden penalty for ships with too much cargo.", (PartyRole)14, -0.2f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_shipwrightsInsight.Initialize("{=6gQTNK1Q}Shipwright's Insight", NavalSkills.Boatswain, GetTierCost(5), _specialArrows, "{=fEiCOPxK}{VALUE}% damage to hulls of enemy ships dealt by ballista.", (PartyRole)14, 0.3f, (EffectIncrementType)1, "{=9WinSM8I}{VALUE}% extra ammo for crew.", (PartyRole)14, 0.25f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)0);
		_specialArrows.Initialize("{=sSwzYLVp}Special Arrows", NavalSkills.Boatswain, GetTierCost(5), _shipwrightsInsight, "{=KqEZ5bht}{VALUE} armor for low-tier troops.", (PartyRole)13, 5f, (EffectIncrementType)0, "{=8kHlbgJZ}{VALUE}% damage dealt by crew to enemy sails.", (PartyRole)14, 0.4f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)0);
		_smoothOperator.Initialize("{=k5xfZsXE}Smooth Operator", NavalSkills.Boatswain, GetTierCost(6), _accuracyTraining, "{=elLNH0Ys}{VALUE} ammo for ballista.", (PartyRole)14, 5f, (EffectIncrementType)0, "{=JLP7pNIv}{VALUE}% food consumption at sea.", (PartyRole)14, -0.3f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)65535);
		_accuracyTraining.Initialize("{=09T5s6fh}Accuracy Training", NavalSkills.Boatswain, GetTierCost(6), _smoothOperator, "{=TAcbw7Ac}{VALUE}% damage dealt to shields with ranged weapons wielded by crew.", (PartyRole)14, 0.3f, (EffectIncrementType)1, "{=MDDAbNbX}{VALUE} militia in coastal towns and villages.", (PartyRole)3, 2f, (EffectIncrementType)0, (TroopUsageFlags)8, (TroopUsageFlags)65535);
		_efficientCaptain.Initialize("{=bb4nRJwq}Efficient Captain", NavalSkills.Boatswain, GetTierCost(7), _popularCaptain, "{=OFa86K25}{VALUE}% upgrade cost for mariner troops.", (PartyRole)5, -0.3f, (EffectIncrementType)1, "{=VbWCb1JQ}{VALUE} morale for the party while waiting in a port.", (PartyRole)5, 5f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_popularCaptain.Initialize("{=di0OsDy8}Popular Captain", NavalSkills.Boatswain, GetTierCost(7), _efficientCaptain, "{=V4ornHeA}{VALUE}% recruitment cost for mariner troops.", (PartyRole)5, -0.3f, (EffectIncrementType)1, "{=oIWHDTkm}{VALUE}% combat deck size for the ship under the character's command.", (PartyRole)5, 0.05f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_portAuthority.Initialize("{=MATYHBox}Port Authority", NavalSkills.Boatswain, GetTierCost(8), _blessingsOfTheSea, "{=2mzGeKT1}{VALUE} ship to command limit.", (PartyRole)5, 1f, (EffectIncrementType)0, "{=dW9eIKTx}{VALUE}% to production of walrus ivory and whale oil at villages of the governed settlement.", (PartyRole)3, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_blessingsOfTheSea.Initialize("{=eeY1vDcp}Blessings Of The Sea", NavalSkills.Boatswain, GetTierCost(8), _portAuthority, "{=2mzGeKT1}{VALUE} ship to command limit.", (PartyRole)5, 1f, (EffectIncrementType)0, "{=astRi69P}{VALUE}% to production of fish at villages governed of the governed settlement.", (PartyRole)3, 0.25f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_shipwrightsHand.Initialize("{=2DY8xbnU}Shipwright's Hand", NavalSkills.Boatswain, GetTierCost(9), _salvage, "{=MsFlwAwg}Discarded ships repair party ships.", (PartyRole)5, 0f, (EffectIncrementType)0, "{=XSAXjPX3}{VALUE} recruit to garrison for each merchant convoy entering port.", (PartyRole)3, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_salvage.Initialize("{=LkaTaAyq}Salvage", NavalSkills.Boatswain, GetTierCost(9), _shipwrightsHand, "{=0zuAboaA}Discarded ships provide influence.", (PartyRole)5, 0f, (EffectIncrementType)0, "{=L7uvT9VN}{VALUE} denars gained for each merchant convoy entering port", (PartyRole)3, 40f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_merchantFleet.Initialize("{=0xz4b4wl}Merchant Fleet", NavalSkills.Boatswain, GetTierCost(10), _resilience, "{=Iu7QpMVa}{VALUE} to command limit of ships in battle.", (PartyRole)5, 1f, (EffectIncrementType)0, "{=JfZESBwx}{VALUE} influence gained for each ship built in the governed settlement.", (PartyRole)3, 5f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_resilience.Initialize("{=9mNUfKMo}Resilience", NavalSkills.Boatswain, GetTierCost(10), _merchantFleet, "{=qVyf6an2}{VALUE}% of the health points lost in a battle are recovered after a victory.", (PartyRole)12, 0.3f, (EffectIncrementType)1, "{=KkCqaWLc}{VALUE}% to troops' healing rate while at sea.", (PartyRole)14, 0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_navalBombardment.Initialize("{=21meODRf}Naval Bombardment", NavalSkills.Boatswain, GetTierCost(11), (PerkObject)null, "{=61FfgHSc}Shipboard ballistas fire during sieges", (PartyRole)5, 0f, (EffectIncrementType)0, "", (PartyRole)0, 0f, (EffectIncrementType)(-1), (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_masterAngler.Initialize("{=DWBiOEdQ}Master Angler", NavalSkills.Shipmaster, GetTierCost(1), _oldSaltsTouch, "{=DrNqwl3D}{VALUE}% chance per hour of campaign time to catch fish while sailing on campaign map.", (PartyRole)15, 0.25f, (EffectIncrementType)1, "{=CHCbc79h}{VALUE}% to catch brought in by fishing boats from settlements.", (PartyRole)3, 0.15f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_oldSaltsTouch.Initialize("{=vcyysBJb}Old Salt's Touch", NavalSkills.Shipmaster, GetTierCost(1), _masterAngler, "{=l6P2ivTu}{VALUE}% to travel speed at sea.", (PartyRole)15, 0.02f, (EffectIncrementType)1, "{=1UIq6BRz}{VALUE}% to swimming endurance of crew", (PartyRole)13, 0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_ghostShip.Initialize("{=Sya9pgBv}Ghost Ship", NavalSkills.Shipmaster, GetTierCost(2), _windRider, "{=nTPPc5CT}{VALUE}% fewer troops lost when running a blockade and {VALUE}% chance to avoid leaving ships behind when escaping engagements.", (PartyRole)15, 0.2f, (EffectIncrementType)1, "{=B9AfUcsl}{VALUE}% to campaign map speed of town's fishing boats.", (PartyRole)3, 0.2f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_windRider.Initialize("{=bIFiogWa}Wind Rider", NavalSkills.Shipmaster, GetTierCost(2), _ghostShip, "{=kIb7qvy8}{VALUE}% to deck movement penalty.", (PartyRole)12, -0.5f, (EffectIncrementType)1, "{=m2ZMZ27i}{VALUE}% to deck movement penalty for crew.", (PartyRole)13, -0.5f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_riverRaider.Initialize("{=46X8OsCn}River Raider", NavalSkills.Shipmaster, GetTierCost(3), _nightRaider, "{=fMe3Yas2}{VALUE}% to coastal movement speed penalty.", (PartyRole)15, -0.03f, (EffectIncrementType)1, "{=XbbSGawE}{VALUE}% to chance of capturing enemy troops as prisoners.", (PartyRole)5, 0.1f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_nightRaider.Initialize("{=MQlcI2bf}Night Raider", NavalSkills.Shipmaster, GetTierCost(3), _riverRaider, "{=CRSaWc9S}{VALUE}% to night spotting range penalty on campaign map.", (PartyRole)15, -0.5f, (EffectIncrementType)1, "{=tYqawD5G}{VALUE} fish per day produced by coastal villages.", (PartyRole)3, 5f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_windborne.Initialize("{=49NFcwEM}Windborne", NavalSkills.Shipmaster, GetTierCost(4), _shockAndAwe, "{=6JZ8ZAqD}{VALUE}% to sail forces in missions.", (PartyRole)13, 0.2f, (EffectIncrementType)1, "{=dnCVmLuI}{VALUE}% to duration of disorganized state while at sea.", (PartyRole)15, -0.5f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)65535);
		_shockAndAwe.Initialize("{=M1RMMBzF}Shock and Awe", NavalSkills.Shipmaster, GetTierCost(4), _windborne, "{=nxYOlJuf}{VALUE}% morale boost when ship rams enemy ship.", (PartyRole)13, 0.3f, (EffectIncrementType)1, "{=YEpGBEOb}{VALUE}% to speed on campaign map when against the wind.", (PartyRole)15, 0.1f, (EffectIncrementType)1, (TroopUsageFlags)0, (TroopUsageFlags)65535);
		_theHelmsmansShield.Initialize("{=nO4nrVeF}The Helmsman's Shield", NavalSkills.Shipmaster, GetTierCost(5), _ravenEye, "{=IEQgidZB}{VALUE}% to ranged damage suffered by character while steering the ship.", (PartyRole)12, -0.5f, (EffectIncrementType)1, "{=ZV0bkXpX}{VALUE} prosperity for each fishing boat returning to port.", (PartyRole)3, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_ravenEye.Initialize("{=NMgbVLbx}Raven Eye", NavalSkills.Shipmaster, GetTierCost(5), _theHelmsmansShield, "{=zEJLYSYa}{VALUE}% to spotting range on campaign map.", (PartyRole)15, 0.2f, (EffectIncrementType)1, "{=5bFACRPa}{VALUE} loyalty for each fishing boat returning to port.", (PartyRole)3, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_fairWinds.Initialize("{=LOZkPSV1}Fair Winds", NavalSkills.Shipmaster, GetTierCost(6), _favorableTide, "{=qC0VbVsB}{VALUE}% to campaign map travel speed when running before the wind.", (PartyRole)15, 0.1f, (EffectIncrementType)1, "{=alTcdq2M}{VALUE}% to hearth growth rate in coastal villages", (PartyRole)3, 0.1f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_favorableTide.Initialize("{=uZo1RgiX}Favorable Tide", NavalSkills.Shipmaster, GetTierCost(6), _fairWinds, "{=MgNvqdV5}{VALUE}% to campaign map travel speed ", (PartyRole)15, 0.05f, (EffectIncrementType)1, "{=5fwthk1U}{VALUE} building material in settlement for each merchant convoy visiting port", (PartyRole)3, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_unflinching.Initialize("{=WdFTSzc1}Unflinching", NavalSkills.Shipmaster, GetTierCost(7), _shoreMaster, "{=K7hL8TgJ}{VALUE}% to disembarkation speed.", (PartyRole)5, 1f, (EffectIncrementType)1, "", (PartyRole)0, 0f, (EffectIncrementType)(-1), (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_shoreMaster.Initialize("{=YluQBFDG}Shore Master", NavalSkills.Shipmaster, GetTierCost(7), _unflinching, "{=qhsfjbv6}{VALUE}% to ship recall time.", (PartyRole)5, -0.5f, (EffectIncrementType)1, "{=mbT5BIx3}{VALUE}% to fleet size penalty for campaign map movement", (PartyRole)15, -0.3f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_fleetCommander.Initialize("{=ZJyEHWfa}Fleet Commander", NavalSkills.Shipmaster, GetTierCost(8), _chainToOars, "{=mbT5BIx3}{VALUE}% to fleet size penalty for campaign map movement", (PartyRole)5, -0.3f, (EffectIncrementType)1, "{=F4zoUlg1}{VALUE}% to skeleton crew requirement for any ship in party", (PartyRole)5, -0.2f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_chainToOars.Initialize("{=R1izgjgK}Chain to Oars", NavalSkills.Shipmaster, GetTierCost(8), _fleetCommander, "{=vSJEt7l8}{VALUE}% to oar force in mission", (PartyRole)13, 0.2f, (EffectIncrementType)1, "{=vRjKbjOa}Prisoners help meet skeleton crew requirement", (PartyRole)5, 0f, (EffectIncrementType)0, (TroopUsageFlags)0, (TroopUsageFlags)65535);
		_stormrider.Initialize("{=uLPXg0qS}Stormrider", NavalSkills.Shipmaster, GetTierCost(9), _masterAndCommander, "{=EAlm5gSh}{VALUE} xp gained by each troop once per day when entering storms", (PartyRole)5, 30f, (EffectIncrementType)0, "{=2mzGeKT1}{VALUE} ship to command limit.", (PartyRole)5, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_masterAndCommander.Initialize("{=tivTy0RA}Master and commander", NavalSkills.Shipmaster, GetTierCost(9), _stormrider, "{=dGZAPFdQ}{VALUE} xp gained by each troop per hour at sea", (PartyRole)5, 1f, (EffectIncrementType)0, "{=2mzGeKT1}{VALUE} ship to command limit.", (PartyRole)5, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_theCorsairsEdge.Initialize("{=JvwosrT8}The Corsair's Edge", NavalSkills.Shipmaster, GetTierCost(10), _seaborneFortress, "{=QUI46tx7}{VALUE}% damage when wielding one-handed weapons at sea", (PartyRole)12, 0.1f, (EffectIncrementType)1, "{=DRb8Us5b}{VALUE} fishing boat produced by each coastal settlement.", (PartyRole)3, 1f, (EffectIncrementType)0, (TroopUsageFlags)65535, (TroopUsageFlags)65535);
		_seaborneFortress.Initialize("{=Dyy3HcUI}Seaborne Fortress", NavalSkills.Shipmaster, GetTierCost(10), _theCorsairsEdge, "{=kAajH6yd}{VALUE}% to damage sustained by ships when crew is sent to confront the enemies.", (PartyRole)5, -0.1f, (EffectIncrementType)1, "{=OszgWi0t}{VALUE}% to ranged damage taken by crew if not boarded.", (PartyRole)13, -0.2f, (EffectIncrementType)1, (TroopUsageFlags)65535, (TroopUsageFlags)0);
		_commodore.Initialize("{=NbZeB1RT}Commodore", NavalSkills.Shipmaster, GetTierCost(11), (PerkObject)null, "{=1g9TufnU}Flagship figurehead provides bonus to all allied ships.", (PartyRole)4, 0f, (EffectIncrementType)0, "", (PartyRole)0, 0f, (EffectIncrementType)(-1), (TroopUsageFlags)65535, (TroopUsageFlags)65535);
	}

	private PerkObject Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<PerkObject>(new PerkObject(stringId));
	}

	private static int GetTierCost(int tierIndex)
	{
		return TierSkillRequirements[tierIndex - 1];
	}
}
