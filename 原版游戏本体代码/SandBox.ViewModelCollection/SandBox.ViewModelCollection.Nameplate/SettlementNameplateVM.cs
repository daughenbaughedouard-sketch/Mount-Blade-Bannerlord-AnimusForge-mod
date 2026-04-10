using System;
using System.Collections.Generic;
using Helpers;
using SandBox.ViewModelCollection.Nameplate.NameplateNotifications.SettlementNotificationTypes;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.Nameplate;

public class SettlementNameplateVM : NameplateVM
{
	public enum Type
	{
		Village,
		Castle,
		Town
	}

	public enum RelationType
	{
		Neutral,
		SameFaction,
		Enemy,
		Ally
	}

	public enum IssueTypes
	{
		None,
		Possible,
		Active
	}

	public enum MainQuestTypes
	{
		None,
		Possible,
		Active
	}

	private readonly Camera _mapCamera;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private float _heightOffset;

	private bool _latestIsInsideWindow;

	private Banner _latestBanner;

	private int _latestBannerVersionNo;

	private bool _isTrackedManually;

	private readonly GameEntity _entity;

	private Vec3 _worldPos;

	private Vec3 _worldPosWithHeight;

	private IFaction _currentFaction;

	private readonly Action<CampaignVec2> _fastMoveCameraToPosition;

	private readonly bool _isVillage;

	private readonly bool _isCastle;

	private readonly bool _isTown;

	private float _wPosAfterPositionCalculation;

	private string _bindName;

	private string _bindFactionColor;

	private bool _bindIsTracked;

	private BannerImageIdentifierVM _bindBanner;

	private int _bindRelation;

	private float _bindWPos;

	private float _bindDistanceToCamera;

	private int _bindWSign;

	private bool _bindIsInside;

	private Vec2 _bindPosition;

	private bool _bindIsVisibleOnMap;

	private bool _bindIsInRange;

	private bool _bindHasPort;

	private int _bindPortLevel;

	private List<Clan> _rebelliousClans;

	private string _name;

	private int _settlementType = -1;

	private BannerImageIdentifierVM _banner;

	private int _relation;

	private int _wSign;

	private float _wPos;

	private bool _isTracked;

	private bool _isInside;

	private bool _isInRange;

	private bool _hasPort;

	private int _portLevel;

	private int _mapEventVisualType;

	private SettlementNameplateNotificationsVM _settlementNotifications;

	private SettlementNameplatePartyMarkersVM _settlementParties;

	private SettlementNameplateEventsVM _settlementEvents;

	public Settlement Settlement { get; }

	public Type SettlementTypeEnum { get; private set; }

	public SettlementNameplateNotificationsVM SettlementNotifications
	{
		get
		{
			return _settlementNotifications;
		}
		set
		{
			if (value != _settlementNotifications)
			{
				_settlementNotifications = value;
				((ViewModel)this).OnPropertyChangedWithValue<SettlementNameplateNotificationsVM>(value, "SettlementNotifications");
			}
		}
	}

	public SettlementNameplatePartyMarkersVM SettlementParties
	{
		get
		{
			return _settlementParties;
		}
		set
		{
			if (value != _settlementParties)
			{
				_settlementParties = value;
				((ViewModel)this).OnPropertyChangedWithValue<SettlementNameplatePartyMarkersVM>(value, "SettlementParties");
			}
		}
	}

	public SettlementNameplateEventsVM SettlementEvents
	{
		get
		{
			return _settlementEvents;
		}
		set
		{
			if (value != _settlementEvents)
			{
				_settlementEvents = value;
				((ViewModel)this).OnPropertyChangedWithValue<SettlementNameplateEventsVM>(value, "SettlementEvents");
			}
		}
	}

	public int Relation
	{
		get
		{
			return _relation;
		}
		set
		{
			if (value != _relation)
			{
				_relation = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Relation");
			}
		}
	}

	public int MapEventVisualType
	{
		get
		{
			return _mapEventVisualType;
		}
		set
		{
			if (value != _mapEventVisualType)
			{
				_mapEventVisualType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MapEventVisualType");
			}
		}
	}

	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (value != _wSign)
			{
				_wSign = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WSign");
			}
		}
	}

	public float WPos
	{
		get
		{
			return _wPos;
		}
		set
		{
			if (value != _wPos)
			{
				_wPos = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WPos");
			}
		}
	}

	public BannerImageIdentifierVM Banner
	{
		get
		{
			return _banner;
		}
		set
		{
			if (value != _banner)
			{
				_banner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "Banner");
			}
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	public bool IsTracked
	{
		get
		{
			if (!_isTracked)
			{
				return _bindIsTargetedByTutorial;
			}
			return true;
		}
		set
		{
			if (value != _isTracked)
			{
				_isTracked = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTracked");
			}
		}
	}

	public bool IsInside
	{
		get
		{
			return _isInside;
		}
		set
		{
			if (value != _isInside)
			{
				_isInside = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInside");
			}
		}
	}

	public bool IsInRange
	{
		get
		{
			return _isInRange;
		}
		set
		{
			if (value != _isInRange)
			{
				_isInRange = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInRange");
				if (IsInRange)
				{
					SettlementNotifications.RegisterEvents();
					SettlementParties.RegisterEvents();
					SettlementEvents?.RegisterEvents();
				}
				else
				{
					SettlementNotifications.UnloadEvents();
					SettlementParties.UnloadEvents();
					SettlementEvents?.UnloadEvents();
				}
			}
		}
	}

	public bool HasPort
	{
		get
		{
			return _hasPort;
		}
		set
		{
			if (value != _hasPort)
			{
				_hasPort = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasPort");
			}
		}
	}

	public int PortLevel
	{
		get
		{
			return _portLevel;
		}
		set
		{
			if (value != _portLevel)
			{
				_portLevel = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PortLevel");
			}
		}
	}

	public int SettlementType
	{
		get
		{
			return _settlementType;
		}
		set
		{
			if (value != _settlementType)
			{
				_settlementType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SettlementType");
			}
		}
	}

	public SettlementNameplateVM(Settlement settlement, GameEntity entity, Camera mapCamera, Action<CampaignVec2> fastMoveCameraToPosition)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		Settlement = settlement;
		_mapCamera = mapCamera;
		_entity = entity;
		_fastMoveCameraToPosition = fastMoveCameraToPosition;
		SettlementNotifications = new SettlementNameplateNotificationsVM(settlement);
		SettlementParties = new SettlementNameplatePartyMarkersVM(settlement);
		SettlementEvents = new SettlementNameplateEventsVM(settlement);
		Name = ((object)Settlement.Name).ToString();
		IsTracked = Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)settlement);
		if (Settlement.IsCastle)
		{
			SettlementTypeEnum = Type.Castle;
			_isCastle = true;
		}
		else if (Settlement.IsVillage)
		{
			SettlementTypeEnum = Type.Village;
			_isVillage = true;
		}
		else if (Settlement.IsTown)
		{
			SettlementTypeEnum = Type.Town;
			_isTown = true;
		}
		else
		{
			SettlementTypeEnum = Type.Village;
			_isTown = true;
		}
		SettlementType = (int)SettlementTypeEnum;
		if (_entity != (GameEntity)null)
		{
			_worldPos = _entity.GlobalPosition;
		}
		else
		{
			_worldPos = Settlement.GetPositionAsVec3();
		}
		RefreshDynamicProperties(forceUpdate: false);
		_rebelliousClans = new List<Clan>();
		if (Game.Current != null)
		{
			Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>((Action<TutorialNotificationElementChangeEvent>)base.OnTutorialNotificationElementChanged);
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		SettlementNotifications.UnloadEvents();
		SettlementParties.UnloadEvents();
		Game.Current.EventManager.UnregisterEvent<TutorialNotificationElementChangeEvent>((Action<TutorialNotificationElementChangeEvent>)base.OnTutorialNotificationElementChanged);
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)Settlement.Name).ToString();
	}

	public override void RefreshDynamicProperties(bool forceUpdate)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		base.RefreshDynamicProperties(forceUpdate);
		if ((_bindIsVisibleOnMap && _currentFaction != Settlement.MapFaction) || forceUpdate)
		{
			IFaction mapFaction = Settlement.MapFaction;
			_bindFactionColor = "#" + Color.UIntToColorString((mapFaction != null) ? mapFaction.Color : uint.MaxValue);
			Banner banner = Settlement.Banner;
			int num = ((banner != null) ? banner.GetVersionNo() : 0);
			if ((_latestBanner != banner && !BannerExtensions.IsContentsSameWith(_latestBanner, banner)) || _latestBannerVersionNo != num)
			{
				_bindBanner = ((banner != null) ? new BannerImageIdentifierVM(banner, true) : new BannerImageIdentifierVM((Banner)null, false));
				_latestBannerVersionNo = num;
				_latestBanner = banner;
			}
			_currentFaction = Settlement.MapFaction;
		}
		PartyBase party = Settlement.Party;
		if ((party != null && party.IsVisualDirty) || forceUpdate)
		{
			_bindName = ((object)Settlement.Party.Name).ToString();
		}
		_bindIsTracked = Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)Settlement);
		if (Settlement.IsHideout)
		{
			SettlementComponent settlementComponent = Settlement.SettlementComponent;
			ISpottable val = (ISpottable)(object)((settlementComponent is ISpottable) ? settlementComponent : null);
			_bindIsInRange = val != null && val.IsSpotted;
		}
		else
		{
			_bindIsInRange = Settlement.IsInspected;
		}
		if (!(_bindIsVisibleOnMap || forceUpdate))
		{
			return;
		}
		_bindHasPort = Settlement.HasPort;
		_bindPortLevel = 0;
		if (!_bindHasPort)
		{
			return;
		}
		MBList<Building> val2 = Settlement?.Town?.Buildings;
		if (val2 == null)
		{
			return;
		}
		for (int i = 0; i < ((List<Building>)(object)val2).Count; i++)
		{
			if (((MBObjectBase)((List<Building>)(object)val2)[i].BuildingType).StringId == "building_shipyard")
			{
				_bindPortLevel = ((List<Building>)(object)val2)[i].CurrentLevel;
			}
		}
	}

	public override void RefreshRelationStatus()
	{
		_bindRelation = 0;
		if (Settlement.OwnerClan != null)
		{
			if (FactionManager.IsAtWarAgainstFaction(Settlement.MapFaction, Hero.MainHero.MapFaction))
			{
				_bindRelation = 2;
			}
			else if (DiplomacyHelper.IsSameFactionAndNotEliminated(Settlement.MapFaction, Hero.MainHero.MapFaction))
			{
				_bindRelation = 1;
			}
			else if (DiplomacyHelper.HasAllianceWithFaction(Settlement.MapFaction, Hero.MainHero.MapFaction))
			{
				_bindRelation = 3;
			}
		}
	}

	public override void RefreshPosition()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshPosition();
		_bindWPos = _wPosAfterPositionCalculation;
		_bindWSign = (int)_bindWPos;
		_bindIsInside = _latestIsInsideWindow;
		if (_bindIsVisibleOnMap)
		{
			_bindPosition = new Vec2(_latestX, _latestY);
		}
		else
		{
			_bindPosition = new Vec2(-1000f, -1000f);
		}
	}

	public override void RefreshTutorialStatus(string newTutorialHighlightElementID)
	{
		base.RefreshTutorialStatus(newTutorialHighlightElementID);
		Settlement settlement = Settlement;
		object obj;
		if (settlement == null)
		{
			obj = null;
		}
		else
		{
			PartyBase party = settlement.Party;
			obj = ((party != null) ? party.Id : null);
		}
		if (obj == null)
		{
			Debug.FailedAssert("Settlement party id is null when refreshing tutorial status", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\Nameplate\\SettlementNameplateVM.cs", "RefreshTutorialStatus", 271);
		}
		else
		{
			_bindIsTargetedByTutorial = Settlement.Party.Id == newTutorialHighlightElementID;
		}
	}

	public void OnSiegeEventStartedOnSettlement(SiegeEvent siegeEvent)
	{
		MapEventVisualType = 2;
		if (Settlement.MapFaction == Hero.MainHero.MapFaction && (BannerlordConfig.AutoTrackAttackedSettlements == 0 || (BannerlordConfig.AutoTrackAttackedSettlements == 1 && Settlement.MapFaction.Leader == Hero.MainHero)))
		{
			Track();
		}
	}

	public void OnSiegeEventEndedOnSettlement(SiegeEvent siegeEvent)
	{
		Settlement settlement = Settlement;
		object obj;
		if (settlement == null)
		{
			obj = null;
		}
		else
		{
			PartyBase party = settlement.Party;
			obj = ((party != null) ? party.MapEvent : null);
		}
		if (obj != null && !Settlement.Party.MapEvent.IsFinalized)
		{
			OnMapEventStartedOnSettlement(Settlement.Party.MapEvent);
		}
		else
		{
			OnMapEventEndedOnSettlement();
		}
		if (!_isTrackedManually && BannerlordConfig.AutoTrackAttackedSettlements < 2 && Settlement.MapFaction == Hero.MainHero.MapFaction)
		{
			Untrack();
		}
	}

	public void OnMapEventStartedOnSettlement(MapEvent mapEvent)
	{
		MapEventVisualType = (int)SandBoxUIHelper.GetMapEventVisualTypeFromMapEvent(mapEvent);
		if (Settlement.MapFaction == Hero.MainHero.MapFaction && (Settlement.IsUnderRaid || Settlement.IsUnderSiege || Settlement.InRebelliousState) && (BannerlordConfig.AutoTrackAttackedSettlements == 0 || (BannerlordConfig.AutoTrackAttackedSettlements == 1 && Settlement.MapFaction.Leader == Hero.MainHero)))
		{
			Track();
		}
	}

	public void OnMapEventEndedOnSettlement()
	{
		MapEventVisualType = 0;
		if (!_isTrackedManually && BannerlordConfig.AutoTrackAttackedSettlements < 2 && !Settlement.IsUnderSiege && !Settlement.IsUnderRaid && !Settlement.InRebelliousState)
		{
			Untrack();
		}
	}

	public void OnRebelliousClanFormed(Clan clan)
	{
		MapEventVisualType = 4;
		_rebelliousClans.Add(clan);
		if (Settlement.MapFaction == Hero.MainHero.MapFaction && (BannerlordConfig.AutoTrackAttackedSettlements == 0 || (BannerlordConfig.AutoTrackAttackedSettlements == 1 && Settlement.MapFaction.Leader == Hero.MainHero)))
		{
			Track();
		}
	}

	public void OnRebelliousClanDisbanded(Clan clan)
	{
		_rebelliousClans.Remove(clan);
		if (!Extensions.IsEmpty<Clan>((IEnumerable<Clan>)_rebelliousClans))
		{
			return;
		}
		if (Settlement.IsUnderSiege)
		{
			MapEventVisualType = 2;
			return;
		}
		MapEventVisualType = 0;
		if (!_isTrackedManually && BannerlordConfig.AutoTrackAttackedSettlements < 2)
		{
			Untrack();
		}
	}

	public void UpdateNameplateMT(Vec3 cameraPosition)
	{
		CalculatePosition(in cameraPosition);
		DetermineIsInsideWindow();
		DetermineIsVisibleOnMap(in cameraPosition);
		RefreshPosition();
		RefreshDynamicProperties(forceUpdate: false);
	}

	private void CalculatePosition(in Vec3 cameraPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		_worldPosWithHeight = _worldPos;
		if (_isVillage)
		{
			_heightOffset = 0.5f + MathF.Clamp(cameraPosition.z / 30f, 0f, 1f) * 2.5f;
		}
		else if (_isCastle)
		{
			_heightOffset = 0.5f + MathF.Clamp(cameraPosition.z / 30f, 0f, 1f) * 3f;
		}
		else if (_isTown)
		{
			_heightOffset = 0.5f + MathF.Clamp(cameraPosition.z / 30f, 0f, 1f) * 6f;
		}
		else
		{
			_heightOffset = 1f;
		}
		_worldPosWithHeight += new Vec3(0f, 0f, _heightOffset, -1f);
		if (((Vec3)(ref _worldPosWithHeight)).IsValidXYZW)
		{
			Vec3 position = _mapCamera.Position;
			if (((Vec3)(ref position)).IsValidXYZW)
			{
				_latestX = 0f;
				_latestY = 0f;
				_latestW = 0f;
				MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, _worldPosWithHeight, ref _latestX, ref _latestY, ref _latestW);
			}
		}
		bool flag = _latestW < 0f;
		_wPosAfterPositionCalculation = (flag ? (-1f) : 1.1f);
	}

	private void DetermineIsVisibleOnMap(in Vec3 cameraPosition)
	{
		_bindIsVisibleOnMap = IsVisible(in cameraPosition);
	}

	private void DetermineIsInsideWindow()
	{
		_latestIsInsideWindow = IsInsideWindow();
	}

	public void RefreshBindValues()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		base.FactionColor = _bindFactionColor;
		Banner = _bindBanner;
		Relation = _bindRelation;
		WPos = _bindWPos;
		WSign = _bindWSign;
		IsInside = _bindIsInside;
		base.Position = _bindPosition;
		base.IsVisibleOnMap = _bindIsVisibleOnMap;
		IsInRange = _bindIsInRange;
		HasPort = _bindHasPort;
		PortLevel = _bindPortLevel;
		IsTracked = _bindIsTracked;
		base.IsTargetedByTutorial = _bindIsTargetedByTutorial;
		base.DistanceToCamera = _bindDistanceToCamera;
		Name = _bindName;
		if (SettlementNotifications.IsEventsRegistered)
		{
			SettlementNotifications.Tick();
		}
		if (SettlementEvents.IsEventsRegistered)
		{
			SettlementEvents.Tick();
		}
	}

	private bool IsVisible(in Vec3 cameraPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_bindDistanceToCamera = ((Vec3)(ref _worldPos)).Distance(cameraPosition);
		if (!IsTracked)
		{
			if (WPos >= 0f && _latestIsInsideWindow)
			{
				if (cameraPosition.z > 400f)
				{
					return Settlement.IsTown;
				}
				if (cameraPosition.z > 200f)
				{
					return Settlement.IsFortification;
				}
				return _bindDistanceToCamera < cameraPosition.z + 100f;
			}
			return false;
		}
		return true;
	}

	private bool IsInsideWindow()
	{
		float num = Screen.RealScreenResolutionWidth * 0.00052083336f;
		if (_latestX <= Screen.RealScreenResolutionWidth + 200f * num && _latestY <= Screen.RealScreenResolutionHeight + 100f * num && _latestX + 200f * num >= 0f)
		{
			return _latestY + 100f * num >= 0f;
		}
		return false;
	}

	public void ExecuteTrack()
	{
		if (IsTracked)
		{
			Untrack();
			_isTrackedManually = false;
		}
		else
		{
			Track();
			_isTrackedManually = true;
		}
	}

	private void Track()
	{
		IsTracked = true;
		if (!Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)Settlement))
		{
			Campaign.Current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)(object)Settlement);
		}
	}

	private void Untrack()
	{
		IsTracked = false;
		if (Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)Settlement))
		{
			Campaign.Current.VisualTrackerManager.RemoveTrackedObject((ITrackableBase)(object)Settlement, false);
		}
	}

	public void ExecuteSetCameraPosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_fastMoveCameraToPosition(Settlement.Position);
	}

	public void ExecuteOpenEncyclopedia()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Settlement.EncyclopediaLink);
	}
}
