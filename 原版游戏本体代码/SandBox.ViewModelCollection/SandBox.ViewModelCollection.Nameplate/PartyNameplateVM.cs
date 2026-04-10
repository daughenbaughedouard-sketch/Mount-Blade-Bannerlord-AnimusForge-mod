using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Nameplate;

public class PartyNameplateVM : NameplateVM
{
	public static string PositiveIndicator = ((object)Color.FromUint(4285650500u)/*cast due to .constrained prefix*/).ToString();

	public static string PositiveArmyIndicator = ((object)Color.FromUint(4288804731u)/*cast due to .constrained prefix*/).ToString();

	public static string NegativeIndicator = ((object)Color.FromUint(4292232774u)/*cast due to .constrained prefix*/).ToString();

	public static string NegativeArmyIndicator = ((object)Color.FromUint(4294931829u)/*cast due to .constrained prefix*/).ToString();

	public static string NeutralIndicator = ((object)Color.FromUint(4291877096u)/*cast due to .constrained prefix*/).ToString();

	public static string NeutralArmyIndicator = ((object)Color.FromUint(4294573055u)/*cast due to .constrained prefix*/).ToString();

	public static string MainPartyIndicator = ((object)Color.FromUint(4287421380u)/*cast due to .constrained prefix*/).ToString();

	public static string MainPartyArmyIndicator = ((object)Color.FromUint(4289593317u)/*cast due to .constrained prefix*/).ToString();

	public static string AllianceIndicator = ((object)Color.FromUint(4279460044u)/*cast due to .constrained prefix*/).ToString();

	public static string AllianceArmyIndicator = ((object)Color.FromUint(4279476684u)/*cast due to .constrained prefix*/).ToString();

	protected float _latestX;

	protected float _latestY;

	protected float _latestW;

	protected float _cachedSpeed;

	protected Camera _mapCamera;

	protected int _latestPrisonerAmount = -1;

	protected int _latestWoundedAmount = -1;

	protected int _latestTotalCount = -1;

	protected bool _isPartyBannerDirty;

	protected TextObject _latestNameTextObject;

	protected IssueQuestFlags _previousQuestsBind;

	protected IssueQuestFlags _questsBind;

	protected Vec2 _partyPositionBind;

	protected Vec2 _headPositionBind;

	protected bool _isHighBind;

	protected bool _isBehindBind;

	protected bool _isInArmyBind;

	protected bool _isInSettlementBind;

	protected bool _isVisibleOnMapBind;

	protected bool _isArmyBind;

	protected bool _isDisorganizedBind;

	protected bool _isCurrentlyAtSeaBind;

	protected string _factionColorBind;

	protected string _countBind;

	protected string _woundedBind;

	protected string _prisonerBind;

	protected string _extraInfoTextBind;

	protected string _fullNameBind;

	protected string _movementSpeedTextBind;

	private string _count;

	private string _wounded;

	private string _prisoner;

	private MBBindingList<QuestMarkerVM> _quests;

	private string _fullName;

	private string _extraInfoText;

	private string _movementSpeedText;

	private bool _isBehind;

	private bool _isHigh;

	private bool _shouldShowFullName;

	private bool _isInArmy;

	private bool _isArmy;

	private bool _isInSettlement;

	private bool _isDisorganized;

	private bool _isCurrentlyAtSea;

	private BannerImageIdentifierVM _partyBanner;

	private Vec2 _headPosition;

	public MobileParty Party { get; private set; }

	public Vec2 HeadPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _headPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _headPosition)
			{
				_headPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HeadPosition");
			}
		}
	}

	public string Count
	{
		get
		{
			return _count;
		}
		set
		{
			if (value != _count)
			{
				_count = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Count");
			}
		}
	}

	public string Prisoner
	{
		get
		{
			return _prisoner;
		}
		set
		{
			if (value != _prisoner)
			{
				_prisoner = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Prisoner");
			}
		}
	}

	public MBBindingList<QuestMarkerVM> Quests
	{
		get
		{
			return _quests;
		}
		set
		{
			if (value != _quests)
			{
				_quests = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<QuestMarkerVM>>(value, "Quests");
			}
		}
	}

	public string Wounded
	{
		get
		{
			return _wounded;
		}
		set
		{
			if (value != _wounded)
			{
				_wounded = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Wounded");
			}
		}
	}

	public string ExtraInfoText
	{
		get
		{
			return _extraInfoText;
		}
		set
		{
			if (value != _extraInfoText)
			{
				_extraInfoText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ExtraInfoText");
			}
		}
	}

	public string MovementSpeedText
	{
		get
		{
			return _movementSpeedText;
		}
		set
		{
			if (value != _movementSpeedText)
			{
				_movementSpeedText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MovementSpeedText");
			}
		}
	}

	public string FullName
	{
		get
		{
			return _fullName;
		}
		set
		{
			if (value != _fullName)
			{
				_fullName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FullName");
			}
		}
	}

	public bool IsInArmy
	{
		get
		{
			return _isInArmy;
		}
		set
		{
			if (value != _isInArmy)
			{
				_isInArmy = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInArmy");
			}
		}
	}

	public bool IsInSettlement
	{
		get
		{
			return _isInSettlement;
		}
		set
		{
			if (value != _isInSettlement)
			{
				_isInSettlement = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInSettlement");
			}
		}
	}

	public bool IsDisorganized
	{
		get
		{
			return _isDisorganized;
		}
		set
		{
			if (value != _isDisorganized)
			{
				_isDisorganized = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisorganized");
			}
		}
	}

	public bool IsCurrentlyAtSea
	{
		get
		{
			return _isCurrentlyAtSea;
		}
		set
		{
			if (value != _isCurrentlyAtSea)
			{
				_isCurrentlyAtSea = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCurrentlyAtSea");
			}
		}
	}

	public bool IsArmy
	{
		get
		{
			return _isArmy;
		}
		set
		{
			if (value != _isArmy)
			{
				_isArmy = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsArmy");
			}
		}
	}

	public bool IsBehind
	{
		get
		{
			return _isBehind;
		}
		set
		{
			if (value != _isBehind)
			{
				_isBehind = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBehind");
			}
		}
	}

	public bool IsHigh
	{
		get
		{
			return _isHigh;
		}
		set
		{
			if (value != _isHigh)
			{
				_isHigh = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsHigh");
			}
		}
	}

	public bool ShouldShowFullName
	{
		get
		{
			if (!_shouldShowFullName)
			{
				return base.IsTargetedByTutorial;
			}
			return true;
		}
		set
		{
			if (value != _shouldShowFullName)
			{
				_shouldShowFullName = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShouldShowFullName");
			}
		}
	}

	public BannerImageIdentifierVM PartyBanner
	{
		get
		{
			return _partyBanner;
		}
		set
		{
			if (value != _partyBanner)
			{
				_partyBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "PartyBanner");
			}
		}
	}

	public PartyNameplateVM()
	{
		Quests = new MBBindingList<QuestMarkerVM>();
	}

	public void InitializeWith(MobileParty party, Camera mapCamera)
	{
		_mapCamera = mapCamera;
		Party = party;
		_isPartyBannerDirty = true;
		((Collection<QuestMarkerVM>)(object)Quests).Clear();
		RegisterEvents();
	}

	public virtual void Clear()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_mapCamera = null;
		Party = null;
		_isPartyBannerDirty = false;
		_latestNameTextObject = null;
		_previousQuestsBind = (IssueQuestFlags)0;
		((Collection<QuestMarkerVM>)(object)Quests).Clear();
		((ViewModel)this).OnFinalize();
		UnregisterEvents();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		RefreshDynamicProperties(forceUpdate: true);
	}

	public void RegisterEvents()
	{
		CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementDetail>)OnSettlementOwnerChanged);
		CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener((object)this, (Action<Clan, Kingdom, Kingdom, ChangeKingdomActionDetail, bool>)OnClanChangeKingdom);
		CampaignEvents.OnClanLeaderChangedEvent.AddNonSerializedListener((object)this, (Action<Hero, Hero>)OnClanLeaderChanged);
		CampaignEvents.OnHeroTeleportationRequestedEvent.AddNonSerializedListener((object)this, (Action<Hero, Settlement, MobileParty, TeleportationDetail>)OnHeroTeleportationRequested);
		if (Game.Current != null)
		{
			Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>((Action<TutorialNotificationElementChangeEvent>)base.OnTutorialNotificationElementChanged);
		}
	}

	public void UnregisterEvents()
	{
		((IMbEventBase)CampaignEvents.OnSettlementOwnerChangedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnClanChangedKingdomEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnClanLeaderChangedEvent).ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnHeroTeleportationRequestedEvent).ClearListeners((object)this);
		Game.Current.EventManager.UnregisterEvent<TutorialNotificationElementChangeEvent>((Action<TutorialNotificationElementChangeEvent>)base.OnTutorialNotificationElementChanged);
	}

	private void AddQuestBindFlagsForParty(MobileParty party)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (party == MobileParty.MainParty || party == Party)
		{
			return;
		}
		Hero leaderHero = party.LeaderHero;
		if (((leaderHero != null) ? leaderHero.Issue : null) != null && (_questsBind & 8) == 0 && ((_questsBind & 1) == 0 || (_questsBind & 2) == 0))
		{
			_questsBind |= CampaignUIHelper.GetIssueType(party.LeaderHero.Issue);
		}
		if (((_questsBind & 0x10) != 0 || (_questsBind & 2) != 0) && (_questsBind & 4) != 0)
		{
			return;
		}
		List<QuestBase> questsRelatedToParty = CampaignUIHelper.GetQuestsRelatedToParty(party);
		for (int i = 0; i < questsRelatedToParty.Count; i++)
		{
			QuestBase val = questsRelatedToParty[i];
			if (party.LeaderHero != null && val.QuestGiver == party.LeaderHero)
			{
				if (val.IsSpecialQuest && (_questsBind & 4) == 0)
				{
					_questsBind = (IssueQuestFlags)(_questsBind | 4);
				}
				else if (!val.IsSpecialQuest && (_questsBind & 2) == 0)
				{
					_questsBind = (IssueQuestFlags)(_questsBind | 2);
				}
			}
			else if (val.IsSpecialQuest && (_questsBind & 0x10) == 0)
			{
				_questsBind = (IssueQuestFlags)(_questsBind | 0x10);
			}
			else if (!val.IsSpecialQuest && (_questsBind & 8) == 0)
			{
				_questsBind = (IssueQuestFlags)(_questsBind | 8);
			}
		}
	}

	public override void RefreshDynamicProperties(bool forceUpdate)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Expected O, but got Unknown
		base.RefreshDynamicProperties(forceUpdate);
		if (_isVisibleOnMapBind || forceUpdate)
		{
			MobileParty party = Party;
			object obj;
			if (party == null)
			{
				obj = null;
			}
			else
			{
				Hero leaderHero = party.LeaderHero;
				obj = ((leaderHero != null) ? leaderHero.Issue : null);
			}
			IssueBase val = (IssueBase)obj;
			_questsBind = (IssueQuestFlags)0;
			if (Party != MobileParty.MainParty)
			{
				if (val != null)
				{
					_questsBind |= CampaignUIHelper.GetIssueType(val);
				}
				List<QuestBase> questsRelatedToParty = CampaignUIHelper.GetQuestsRelatedToParty(Party);
				for (int i = 0; i < questsRelatedToParty.Count; i++)
				{
					QuestBase val2 = questsRelatedToParty[i];
					if (val2.QuestGiver != null && val2.QuestGiver == Party.LeaderHero)
					{
						_questsBind = (IssueQuestFlags)(_questsBind | (val2.IsSpecialQuest ? 4 : 2));
					}
					else
					{
						_questsBind = (IssueQuestFlags)(_questsBind | (val2.IsSpecialQuest ? 16 : 8));
					}
				}
			}
		}
		_isInArmyBind = Party.Army != null && Party.AttachedTo != null;
		_isArmyBind = Party.Army != null && Party.Army.LeaderParty == Party;
		MobileParty party2 = Party;
		_isInSettlementBind = ((party2 != null) ? party2.CurrentSettlement : null) != null;
		if (_isArmyBind && (_isVisibleOnMapBind || forceUpdate))
		{
			AddQuestBindFlagsForParty(Party.Army.LeaderParty);
			for (int j = 0; j < ((List<MobileParty>)(object)Party.Army.LeaderParty.AttachedParties).Count; j++)
			{
				MobileParty party3 = ((List<MobileParty>)(object)Party.Army.LeaderParty.AttachedParties)[j];
				AddQuestBindFlagsForParty(party3);
			}
		}
		if (_isArmyBind || !_isInArmy || forceUpdate)
		{
			int partyHealthyCount = SandBoxUIHelper.GetPartyHealthyCount(Party);
			if (partyHealthyCount != _latestTotalCount)
			{
				_latestTotalCount = partyHealthyCount;
				_countBind = (Party.IsInfoHidden ? "?" : partyHealthyCount.ToString());
			}
			int allWoundedMembersAmount = SandBoxUIHelper.GetAllWoundedMembersAmount(Party);
			int allPrisonerMembersAmount = SandBoxUIHelper.GetAllPrisonerMembersAmount(Party);
			if (_latestWoundedAmount != allWoundedMembersAmount || _latestPrisonerAmount != allPrisonerMembersAmount)
			{
				if (_latestWoundedAmount != allWoundedMembersAmount)
				{
					_woundedBind = ((allWoundedMembersAmount == 0) ? "" : (Party.IsInfoHidden ? "?" : SandBoxUIHelper.GetPartyWoundedText(allWoundedMembersAmount)));
					_latestWoundedAmount = allWoundedMembersAmount;
				}
				if (_latestPrisonerAmount != allPrisonerMembersAmount)
				{
					_prisonerBind = ((allPrisonerMembersAmount == 0) ? "" : (Party.IsInfoHidden ? "?" : SandBoxUIHelper.GetPartyPrisonerText(allPrisonerMembersAmount)));
					_latestPrisonerAmount = allPrisonerMembersAmount;
				}
				_extraInfoTextBind = _woundedBind + _prisonerBind;
			}
		}
		if (!Party.IsMainParty)
		{
			Army army = Party.Army;
			if (army == null || !((List<MobileParty>)(object)army.LeaderParty.AttachedParties).Contains(MobileParty.MainParty) || !((List<MobileParty>)(object)Party.Army.LeaderParty.AttachedParties).Contains(Party))
			{
				Hero mainHero = Hero.MainHero;
				if (((mainHero != null) ? mainHero.MapFaction : null) != null)
				{
					IFaction mapFaction = Party.MapFaction;
					Hero mainHero2 = Hero.MainHero;
					if (FactionManager.IsAtWarAgainstFaction(mapFaction, (mainHero2 != null) ? mainHero2.MapFaction : null))
					{
						_factionColorBind = ((Party.Army != null && Party.Army.LeaderParty == Party) ? NegativeArmyIndicator : NegativeIndicator);
						goto IL_04cd;
					}
				}
				if (DiplomacyHelper.IsSameFactionAndNotEliminated(Party.MapFaction, Hero.MainHero.MapFaction))
				{
					_factionColorBind = ((Party.Army != null && Party.Army.LeaderParty == Party) ? PositiveArmyIndicator : PositiveIndicator);
				}
				else
				{
					IFaction mapFaction2 = Party.MapFaction;
					Hero mainHero3 = Hero.MainHero;
					if (DiplomacyHelper.HasAllianceWithFaction(mapFaction2, (mainHero3 != null) ? mainHero3.MapFaction : null))
					{
						_factionColorBind = ((Party.Army != null && Party.Army.LeaderParty == Party) ? AllianceArmyIndicator : AllianceIndicator);
					}
					else
					{
						_factionColorBind = ((Party.Army != null && Party.Army.LeaderParty == Party) ? NeutralArmyIndicator : NeutralIndicator);
					}
				}
				goto IL_04cd;
			}
		}
		_factionColorBind = ((Party.Army != null && Party.Army.LeaderParty == Party) ? MainPartyArmyIndicator : MainPartyIndicator);
		goto IL_04cd;
		IL_04cd:
		if (_isPartyBannerDirty || forceUpdate)
		{
			PartyBanner = new BannerImageIdentifierVM(Party.Banner, true);
			_isPartyBannerDirty = false;
		}
		if (_isVisibleOnMapBind && (_isInArmyBind || _isInSettlementBind || (!Party.IsMainParty && Party.IsInRaftState)))
		{
			_isVisibleOnMapBind = false;
		}
		Army army2 = Party.Army;
		TextObject val3 = ((army2 != null && army2.DoesLeaderPartyAndAttachedPartiesContain(Party)) ? Party.ArmyName : ((Party.LeaderHero == null) ? Party.Name : Party.LeaderHero.Name));
		_isDisorganizedBind = Party.IsDisorganized;
		if (_latestNameTextObject == (TextObject)null || forceUpdate || !_latestNameTextObject.Equals(val3))
		{
			_latestNameTextObject = val3;
			_fullNameBind = ((object)_latestNameTextObject).ToString();
		}
		if (Party.IsActive && !MBMath.ApproximatelyEqualsTo(_cachedSpeed, Party.Speed, 0.01f))
		{
			_cachedSpeed = Party.Speed;
			_movementSpeedTextBind = _cachedSpeed.ToString("F1");
		}
		_isCurrentlyAtSeaBind = Party.IsCurrentlyAtSea;
	}

	public override void RefreshPosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshPosition();
		CampaignVec2 val = Party.Position + Party.EventPositionAdder;
		Vec3 val2 = ((CampaignVec2)(ref val)).AsVec3();
		Vec3 val3 = val2 + new Vec3(0f, 0f, 0.8f, -1f);
		_latestX = 0f;
		_latestY = 0f;
		_latestW = 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, val2, ref _latestX, ref _latestY, ref _latestW);
		_partyPositionBind = new Vec2(_latestX, _latestY);
		MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, val3, ref _latestX, ref _latestY, ref _latestW);
		_headPositionBind = new Vec2(_latestX, _latestY);
		base.DistanceToCamera = ((Vec3)(ref val2)).Distance(_mapCamera.Position);
	}

	public override void RefreshTutorialStatus(string newTutorialHighlightElementID)
	{
		base.RefreshTutorialStatus(newTutorialHighlightElementID);
		MobileParty party = Party;
		object obj;
		if (party == null)
		{
			obj = null;
		}
		else
		{
			PartyBase party2 = party.Party;
			obj = ((party2 != null) ? party2.Id : null);
		}
		if (obj == null)
		{
			Debug.FailedAssert("Mobile party id is null when refreshing tutorial status", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\PartyNameplateVM.cs", "RefreshTutorialStatus", 350);
		}
		else
		{
			_bindIsTargetedByTutorial = ((Party.Party.Id == newTutorialHighlightElementID) ? true : false);
		}
	}

	public void DetermineIsVisibleOnMap()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_isVisibleOnMapBind = _latestW < 100f && _latestW > 0f && _mapCamera.Position.z < 200f;
	}

	private bool IsInsideWindow()
	{
		if (!(_latestX > Screen.RealScreenResolutionWidth) && !(_latestY > Screen.RealScreenResolutionHeight) && !(_latestX + 100f < 0f))
		{
			return !(_latestY + 30f < 0f);
		}
		return false;
	}

	public virtual void RefreshBinding()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		base.Position = _partyPositionBind;
		HeadPosition = _headPositionBind;
		base.IsVisibleOnMap = _isVisibleOnMapBind;
		IsInSettlement = _isInSettlementBind;
		base.FactionColor = _factionColorBind;
		IsHigh = _isHighBind;
		Count = _countBind;
		Prisoner = _prisonerBind;
		Wounded = _woundedBind;
		IsBehind = _isBehindBind;
		FullName = _fullNameBind;
		base.IsTargetedByTutorial = _bindIsTargetedByTutorial;
		IsInArmy = _isInArmyBind;
		IsArmy = _isArmyBind;
		ExtraInfoText = _extraInfoTextBind;
		IsDisorganized = _isDisorganizedBind;
		MovementSpeedText = _movementSpeedTextBind;
		IsCurrentlyAtSea = _isCurrentlyAtSeaBind;
		if (_previousQuestsBind == _questsBind)
		{
			return;
		}
		((Collection<QuestMarkerVM>)(object)Quests).Clear();
		for (int i = 0; i < CampaignUIHelper.IssueQuestFlagsValues.Length; i++)
		{
			IssueQuestFlags val = CampaignUIHelper.IssueQuestFlagsValues[i];
			if ((int)val != 0 && (_questsBind & val) != 0)
			{
				((Collection<QuestMarkerVM>)(object)Quests).Add(new QuestMarkerVM(val, (TextObject)null, (TextObject)null));
			}
		}
		_previousQuestsBind = _questsBind;
	}

	private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementDetail detail)
	{
		bool flag = Party.HomeSettlement != null && (Party.HomeSettlement.IsVillage ? ((List<Village>)(object)settlement.BoundVillages).Contains(Party.HomeSettlement.Village) : (Party.HomeSettlement == settlement));
		if ((Party.IsCaravan || Party.IsVillager) && flag)
		{
			_isPartyBannerDirty = true;
		}
	}

	private void OnClanChangeKingdom(Clan arg1, Kingdom arg2, Kingdom arg3, ChangeKingdomActionDetail arg4, bool showNotification)
	{
		Hero leaderHero = Party.LeaderHero;
		if (((leaderHero != null) ? leaderHero.Clan : null) == arg1)
		{
			_isPartyBannerDirty = true;
		}
	}

	private void OnClanLeaderChanged(Hero arg1, Hero arg2)
	{
		if (arg2.MapFaction == Party.MapFaction)
		{
			_isPartyBannerDirty = true;
		}
	}

	private void OnHeroTeleportationRequested(Hero arg1, Settlement arg2, MobileParty arg3, TeleportationDetail arg4)
	{
		if (arg1.MapFaction == Party.MapFaction)
		{
			_isPartyBannerDirty = true;
		}
	}
}
