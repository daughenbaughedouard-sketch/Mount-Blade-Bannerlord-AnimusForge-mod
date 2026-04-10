using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Helpers;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.View.Map.Visuals;

public class SettlementVisual : MapEntityVisual<PartyBase>
{
	private struct SiegeBombardmentData
	{
		public Vec3 LaunchGlobalPosition;

		public Vec3 TargetPosition;

		public MatrixFrame ShooterGlobalFrame;

		public MatrixFrame TargetAlignedShooterGlobalFrame;

		public float MissileSpeed;

		public float Gravity;

		public float LaunchAngle;

		public float RotationDuration;

		public float ReloadDuration;

		public float AimingDuration;

		public float MissileLaunchDuration;

		public float FireDuration;

		public float FlightDuration;

		public float TotalDuration;
	}

	private const string CircleTag = "map_settlement_circle";

	private const string BannerPlaceHolderTag = "map_banner_placeholder";

	private const string MapSiegeEngineTag = "map_siege_engine";

	private const string MapBreachableWallTag = "map_breachable_wall";

	private const string MapDefenderEngineTag = "map_defensive_engine";

	private const string MapSiegeEngineRamTag = "map_siege_ram";

	private const string TownPhysicalTag = "bo_town";

	private const string MapSiegeEngineTowerTag = "map_siege_tower";

	private const string MapPreparationTag = "siege_preparation";

	private const string BurnedTag = "looted";

	private GameEntity[] _attackerRangedEngineSpawnEntities;

	private GameEntity[] _attackerBatteringRamSpawnEntities;

	private GameEntity[] _defenderBreachableWallEntitiesCacheForCurrentLevel;

	private GameEntity[] _attackerSiegeTowerSpawnEntities;

	private GameEntity[] _defenderRangedEngineSpawnEntitiesForAllLevels;

	private GameEntity[] _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel;

	private GameEntity[] _defenderBreachableWallEntitiesForAllLevels;

	private readonly List<(GameEntity, BattleSideEnum, int, MatrixFrame, GameEntity)> _siegeRangedMachineEntities;

	private readonly List<(GameEntity, BattleSideEnum, int, MatrixFrame, GameEntity)> _siegeMeleeMachineEntities;

	private readonly List<(GameEntity, BattleSideEnum, int)> _siegeMissileEntities;

	private Dictionary<int, List<GameEntity>> _gateBannerEntitiesWithLevels;

	private uint _currentLevelMask;

	private MatrixFrame _hoveredSiegeEntityFrame = MatrixFrame.Identity;

	private UpgradeLevelMask _currentSettlementUpgradeLevelMask;

	private Scene _mapScene;

	private List<GameEntity> TownPhysicalEntities { get; set; }

	public override MapEntityVisual AttachedTo => null;

	public override CampaignVec2 InteractionPositionForPlayer => ((IInteractablePoint)base.MapEntity).GetInteractionPosition(MobileParty.MainParty);

	private Scene MapScene
	{
		get
		{
			if ((NativeObject)(object)_mapScene == (NativeObject)null && Campaign.Current != null && Campaign.Current.MapSceneWrapper != null)
			{
				_mapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
			}
			return _mapScene;
		}
	}

	public GameEntity StrategicEntity { get; private set; }

	public SettlementVisual(PartyBase entity)
		: base(entity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		_siegeRangedMachineEntities = new List<(GameEntity, BattleSideEnum, int, MatrixFrame, GameEntity)>();
		_siegeMeleeMachineEntities = new List<(GameEntity, BattleSideEnum, int, MatrixFrame, GameEntity)>();
		_siegeMissileEntities = new List<(GameEntity, BattleSideEnum, int)>();
		CircleLocalFrame = MatrixFrame.Identity;
	}

	public override bool IsEnemyOf(IFaction faction)
	{
		return FactionManager.IsAtWarAgainstFaction(base.MapEntity.MapFaction, faction.MapFaction);
	}

	public override bool IsInSameFaction(IFaction faction)
	{
		return DiplomacyHelper.IsSameFactionAndNotEliminated(base.MapEntity.MapFaction, faction.MapFaction);
	}

	public override bool IsAllyOf(IFaction faction)
	{
		return DiplomacyHelper.HasAllianceWithFaction(base.MapEntity.MapFaction, faction.MapFaction);
	}

	internal void OnPartyRemoved()
	{
		if (!(StrategicEntity != (GameEntity)null))
		{
			return;
		}
		MapScreen.VisualsOfEntities.Remove(((NativeObject)StrategicEntity).Pointer);
		foreach (GameEntity child in StrategicEntity.GetChildren())
		{
			MapScreen.VisualsOfEntities.Remove(((NativeObject)child).Pointer);
		}
		ReleaseResources();
		StrategicEntity.Remove(111);
	}

	public override Vec3 GetVisualPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 position = base.MapEntity.Position;
		return ((CampaignVec2)(ref position)).AsVec3();
	}

	public override bool IsVisibleOrFadingOut()
	{
		return base.MapEntity.IsVisible;
	}

	public override void OnHover()
	{
		if (base.MapEntity.MapEvent != null)
		{
			InformationManager.ShowTooltip(typeof(MapEvent), new object[1] { base.MapEntity.MapEvent });
		}
		else if (base.MapEntity.IsSettlement && base.MapEntity.IsVisible)
		{
			if (base.MapEntity.Settlement.SiegeEvent != null)
			{
				InformationManager.ShowTooltip(typeof(SiegeEvent), new object[1] { base.MapEntity.Settlement.SiegeEvent });
			}
			else
			{
				InformationManager.ShowTooltip(typeof(Settlement), new object[1] { base.MapEntity.Settlement });
			}
		}
	}

	public override void OnTrackAction()
	{
		Settlement settlement = base.MapEntity.Settlement;
		if (settlement != null)
		{
			if (Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)settlement))
			{
				Campaign.Current.VisualTrackerManager.RemoveTrackedObject((ITrackableBase)(object)settlement, false);
			}
			else
			{
				Campaign.Current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)(object)settlement);
			}
		}
	}

	public override bool OnMapClick(bool followModifierUsed)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (followModifierUsed)
		{
			TextObject val = default(TextObject);
			if (Campaign.Current.Models.EncounterModel.CanMainHeroDoParleyWithParty(base.MapEntity, ref val))
			{
				base.MapScreen.BeginParleyWith(base.MapEntity);
			}
			else if (!TextObject.IsNullOrEmpty(val))
			{
				MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "");
			}
		}
		else if (base.MapEntity.IsVisible)
		{
			NavigationType val2 = default(NavigationType);
			NavigationType val3 = default(NavigationType);
			if (MobileParty.MainParty.IsCurrentlyAtSea && base.MapEntity.Settlement.IsFortification && base.MapEntity.Settlement.HasPort && NavigationHelper.CanPlayerNavigateToPosition(base.MapEntity.Settlement.PortPosition, ref val2))
			{
				MobileParty.MainParty.SetMoveGoToSettlement(base.MapEntity.Settlement, val2, true);
			}
			else if (NavigationHelper.CanPlayerNavigateToPosition(base.MapEntity.Settlement.GatePosition, ref val3))
			{
				MobileParty.MainParty.SetMoveGoToSettlement(base.MapEntity.Settlement, val3, false);
			}
		}
		return true;
	}

	public override void OnOpenEncyclopedia()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(base.MapEntity.Settlement.EncyclopediaLink);
	}

	public override void ReleaseResources()
	{
		RemoveSiege();
		ResetPartyIcon();
	}

	private void ResetPartyIcon()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (StrategicEntity != (GameEntity)null)
		{
			if ((StrategicEntity.EntityFlags & 0x10000000) != 0)
			{
				StrategicEntity.RemoveFromPredisplayEntity();
			}
			StrategicEntity.ClearComponents();
		}
	}

	internal void ValidateIsDirty()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		RefreshPartyIcon();
		if (base.MapEntity.IsVisible)
		{
			StrategicEntity.SetVisibilityExcludeParents(true);
			StrategicEntity.SetAlpha(1f);
			GameEntity strategicEntity = StrategicEntity;
			strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags & -536870913);
		}
		else
		{
			StrategicEntity.SetAlpha(0f);
			StrategicEntity.SetVisibilityExcludeParents(false);
			GameEntity strategicEntity2 = StrategicEntity;
			strategicEntity2.EntityFlags = (EntityFlags)(strategicEntity2.EntityFlags | 0x20000000);
		}
	}

	internal Dictionary<int, List<GameEntity>> GetGateBannerEntitiesWithLevels()
	{
		return _gateBannerEntitiesWithLevels;
	}

	public Vec3 GetBannerPositionForParty(MobileParty mobileParty)
	{
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		if (mobileParty.CurrentSettlement == base.MapEntity.Settlement && base.MapEntity.Settlement.IsFortification && _gateBannerEntitiesWithLevels != null && !Extensions.IsEmpty<KeyValuePair<int, List<GameEntity>>>((IEnumerable<KeyValuePair<int, List<GameEntity>>>)_gateBannerEntitiesWithLevels))
		{
			int wallLevel = base.MapEntity.Settlement.Town.GetWallLevel();
			int count = _gateBannerEntitiesWithLevels[wallLevel].Count;
			if (_gateBannerEntitiesWithLevels[wallLevel].Count > 0)
			{
				int num = 0;
				foreach (MobileParty item in (List<MobileParty>)(object)base.MapEntity.Settlement.Parties)
				{
					if (item == mobileParty)
					{
						break;
					}
					Hero leaderHero = item.LeaderHero;
					if (((leaderHero != null) ? leaderHero.ClanBanner : null) != null)
					{
						num++;
					}
				}
				GameEntity val = _gateBannerEntitiesWithLevels[wallLevel][num % count];
				GameEntity child = val.GetChild(0);
				MatrixFrame val2 = ((child != (GameEntity)null) ? child.GetGlobalFrame() : val.GetGlobalFrame());
				num /= count;
				int num2 = ((IEnumerable<MobileParty>)base.MapEntity.Settlement.Parties).Count(delegate(MobileParty p)
				{
					Hero leaderHero2 = p.LeaderHero;
					return ((leaderHero2 != null) ? leaderHero2.ClanBanner : null) != null;
				});
				float num3 = 0.75f / (float)MathF.Max(1, num2 / (count * 2));
				int num4 = ((num % 2 != 0) ? 1 : (-1));
				Vec3 val3 = val2.rotation.f / 2f * (float)num4;
				if (((Vec3)(ref val3)).Length < ((Vec3)(ref val2.rotation.s)).Length)
				{
					val3 = val2.rotation.s / 2f * (float)num4;
				}
				return val2.origin + val3 * (float)((num + 1) / 2) * (float)(num % 2 * 2 - 1) * num3 * (float)num4;
			}
			Debug.FailedAssert($"{base.MapEntity.Settlement.Name} - has no Banner Entities at level {wallLevel}.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Visuals\\SettlementVisual.cs", "GetBannerPositionForParty", 312);
		}
		return Vec3.Invalid;
	}

	internal void OnMapHoverSiegeEngineEnd()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_hoveredSiegeEntityFrame = MatrixFrame.Identity;
		MBInformationManager.HideInformations();
	}

	private void RefreshPartyIcon()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		if (!base.MapEntity.IsVisualDirty)
		{
			return;
		}
		base.MapEntity.OnVisualsUpdated();
		RemoveSiege();
		StrategicEntity.RemoveAllParticleSystems();
		GameEntity strategicEntity = StrategicEntity;
		strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags | 0x20000000);
		if (base.MapEntity.Settlement.IsFortification)
		{
			UpdateDefenderSiegeEntitiesCache();
		}
		AddSiegeIconComponents(base.MapEntity);
		SetSettlementLevelVisibility();
		RefreshWallState();
		RefreshTownPhysicalEntitiesState(base.MapEntity);
		RefreshSiegePreparations(base.MapEntity);
		bool flag = false;
		if (base.MapEntity.Settlement.IsVillage)
		{
			MapEvent mapEvent = base.MapEntity.MapEvent;
			if (mapEvent != null && mapEvent.IsRaid)
			{
				GameEntity strategicEntity2 = StrategicEntity;
				strategicEntity2.EntityFlags = (EntityFlags)(strategicEntity2.EntityFlags & -536870913);
				StrategicEntity.AddParticleSystemComponent("psys_fire_smoke_env_point");
				if ((StrategicEntity.EntityFlags & 0x10000000) != 0)
				{
					StrategicEntity.RemoveFromPredisplayEntity();
				}
				flag = true;
			}
			else if (base.MapEntity.Settlement.IsRaided)
			{
				GameEntity strategicEntity3 = StrategicEntity;
				strategicEntity3.EntityFlags = (EntityFlags)(strategicEntity3.EntityFlags & -536870913);
				StrategicEntity.AddParticleSystemComponent("map_icon_village_plunder_fx");
				if ((StrategicEntity.EntityFlags & 0x10000000) != 0)
				{
					StrategicEntity.RemoveFromPredisplayEntity();
				}
				flag = true;
			}
		}
		if (!flag && (StrategicEntity.EntityFlags & 0x10000000) == 0)
		{
			StrategicEntity.SetAsPredisplayEntity();
		}
		StrategicEntity.CheckResources(true, false);
	}

	internal void OnStartup()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		StrategicEntity = MapScene.GetCampaignEntityWithName(base.MapEntity.Id);
		if (StrategicEntity == (GameEntity)null)
		{
			IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
			string stringId = ((MBObjectBase)base.MapEntity.Settlement).StringId;
			CampaignVec2 position = base.MapEntity.Settlement.Position;
			mapSceneWrapper.AddNewEntityToMapScene(stringId, ref position);
			StrategicEntity = MapScene.GetCampaignEntityWithName(base.MapEntity.Id);
		}
		bool flag2 = false;
		if (base.MapEntity.Settlement.IsFortification)
		{
			List<GameEntity> list = new List<GameEntity>();
			StrategicEntity.GetChildrenRecursive(ref list);
			PopulateSiegeEngineFrameListsFromChildren(list);
			UpdateDefenderSiegeEntitiesCache();
			TownPhysicalEntities = list.FindAll((GameEntity x) => x.HasTag("bo_town"));
			List<GameEntity> list2 = new List<GameEntity>();
			Dictionary<int, List<GameEntity>> dictionary = new Dictionary<int, List<GameEntity>>
			{
				{
					1,
					new List<GameEntity>()
				},
				{
					2,
					new List<GameEntity>()
				},
				{
					3,
					new List<GameEntity>()
				}
			};
			foreach (GameEntity item in list)
			{
				if (item.HasTag("main_map_city_gate"))
				{
					MatrixFrame globalFrame = item.GetGlobalFrame();
					NavigationHelper.IsPositionValidForNavigationType(new CampaignVec2(((Vec3)(ref globalFrame.origin)).AsVec2, true), (NavigationType)1);
					flag2 = true;
					list2.Add(item);
				}
				if (item.HasTag("map_settlement_circle"))
				{
					CircleLocalFrame = item.GetGlobalFrame();
					flag = true;
					item.SetVisibilityExcludeParents(false);
					list2.Add(item);
				}
				if (item.HasTag("map_banner_placeholder"))
				{
					int upgradeLevelOfEntity = item.Parent.GetUpgradeLevelOfEntity();
					if (upgradeLevelOfEntity == 0)
					{
						dictionary[1].Add(item);
						dictionary[2].Add(item);
						dictionary[3].Add(item);
					}
					else
					{
						dictionary[upgradeLevelOfEntity].Add(item);
					}
					list2.Add(item);
				}
			}
			_gateBannerEntitiesWithLevels = dictionary;
			if (base.MapEntity.Settlement.IsFortification)
			{
				List<MatrixFrame> list3 = default(List<MatrixFrame>);
				List<MatrixFrame> list4 = default(List<MatrixFrame>);
				Campaign.Current.MapSceneWrapper.GetSiegeCampFrames(base.MapEntity.Settlement, ref list3, ref list4);
				base.MapEntity.Settlement.Town.BesiegerCampPositions1 = list3.ToArray();
				base.MapEntity.Settlement.Town.BesiegerCampPositions2 = list4.ToArray();
			}
			foreach (GameEntity item2 in list2)
			{
				item2.Remove(112);
			}
			if (!flag2 && !base.MapEntity.Settlement.IsTown)
			{
				_ = base.MapEntity.Settlement.IsCastle;
			}
			bool flag3 = false;
			if (base.MapEntity.IsSettlement)
			{
				foreach (GameEntity child in StrategicEntity.GetChildren())
				{
					if (child.HasTag("main_map_city_port"))
					{
						MatrixFrame globalFrame2 = child.GetGlobalFrame();
						NavigationHelper.IsPositionValidForNavigationType(new CampaignVec2(((Vec3)(ref globalFrame2.origin)).AsVec2, false), (NavigationType)2);
						flag3 = true;
					}
				}
				if ((flag3 || !base.MapEntity.Settlement.HasPort) && flag3)
				{
					_ = base.MapEntity.Settlement.HasPort;
				}
			}
		}
		if (!flag)
		{
			CircleLocalFrame = MatrixFrame.Identity;
			MatrixFrame circleLocalFrame = CircleLocalFrame;
			Mat3 rotation = circleLocalFrame.rotation;
			if (base.MapEntity.Settlement.IsVillage)
			{
				((Mat3)(ref rotation)).ApplyScaleLocal(1.75f);
			}
			else if (base.MapEntity.Settlement.IsTown)
			{
				((Mat3)(ref rotation)).ApplyScaleLocal(5.75f);
			}
			else if (base.MapEntity.Settlement.IsCastle)
			{
				((Mat3)(ref rotation)).ApplyScaleLocal(2.75f);
			}
			else
			{
				((Mat3)(ref rotation)).ApplyScaleLocal(1.75f);
			}
			circleLocalFrame.rotation = rotation;
			CircleLocalFrame = circleLocalFrame;
		}
		StrategicEntity.SetVisibilityExcludeParents(base.MapEntity.IsVisible);
		StrategicEntity.SetReadyToRender(true);
		StrategicEntity.SetEntityEnvMapVisibility(false);
		List<GameEntity> list5 = new List<GameEntity>();
		StrategicEntity.GetChildrenRecursive(ref list5);
		if (!MapScreen.VisualsOfEntities.ContainsKey(((NativeObject)StrategicEntity).Pointer))
		{
			MapScreen.VisualsOfEntities.Add(((NativeObject)StrategicEntity).Pointer, this);
		}
		foreach (GameEntity item3 in list5)
		{
			if (!MapScreen.VisualsOfEntities.ContainsKey(((NativeObject)item3).Pointer) && !MapScreen.FrameAndVisualOfEngines.ContainsKey(((NativeObject)item3).Pointer))
			{
				MapScreen.VisualsOfEntities.Add(((NativeObject)item3).Pointer, this);
			}
		}
		StrategicEntity.SetAsPredisplayEntity();
	}

	internal void Tick(float dt, ref int dirtyPartiesCount, ref SettlementVisual[] dirtyPartiesList)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_071d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0737: Unknown result type (might be due to invalid IL or missing references)
		//IL_0750: Unknown result type (might be due to invalid IL or missing references)
		//IL_0770: Unknown result type (might be due to invalid IL or missing references)
		//IL_0774: Unknown result type (might be due to invalid IL or missing references)
		//IL_0789: Unknown result type (might be due to invalid IL or missing references)
		//IL_078e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Unknown result type (might be due to invalid IL or missing references)
		//IL_079f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a5: Invalid comparison between Unknown and I4
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07be: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c1: Invalid comparison between Unknown and I4
		//IL_07b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0891: Unknown result type (might be due to invalid IL or missing references)
		//IL_0896: Unknown result type (might be due to invalid IL or missing references)
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_089d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		//IL_080c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Invalid comparison between Unknown and I4
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0607: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0643: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		if (StrategicEntity == (GameEntity)null)
		{
			return;
		}
		if (base.MapEntity.IsVisualDirty)
		{
			int num = Interlocked.Increment(ref dirtyPartiesCount);
			dirtyPartiesList[num] = this;
		}
		else
		{
			CampaignTime val = CampaignTime.Now;
			double toHours = ((CampaignTime)(ref val)).ToHours;
			Vec2 val3 = default(Vec2);
			Vec2 val5 = default(Vec2);
			Vec3 val7;
			foreach (var siegeMissileEntity in _siegeMissileEntities)
			{
				GameEntity item = siegeMissileEntity.Item1;
				ISiegeEventSide siegeEventSide = base.MapEntity.Settlement.SiegeEvent.GetSiegeEventSide(siegeMissileEntity.Item2);
				int item2 = siegeMissileEntity.Item3;
				bool flag = false;
				if (((List<SiegeEngineMissile>)(object)siegeEventSide.SiegeEngineMissiles).Count > item2)
				{
					SiegeEngineMissile val2 = ((List<SiegeEngineMissile>)(object)siegeEventSide.SiegeEngineMissiles)[item2];
					val = val2.CollisionTime;
					double toHours2 = ((CampaignTime)(ref val)).ToHours;
					CalculateDataAndDurationsForSiegeMachine(val2.ShooterSlotIndex, val2.ShooterSiegeEngineType, siegeEventSide.BattleSide, val2.TargetType, val2.TargetSlotIndex, out var bombardmentData);
					float num2 = bombardmentData.MissileSpeed * MathF.Cos(bombardmentData.LaunchAngle);
					if (toHours > toHours2 - (double)bombardmentData.TotalDuration)
					{
						bool flag2 = toHours - (double)dt > toHours2 - (double)bombardmentData.FlightDuration && toHours - (double)dt < toHours2;
						bool flag3 = toHours > toHours2 - (double)bombardmentData.FlightDuration && toHours < toHours2;
						if (flag3)
						{
							flag = true;
							float num3 = (float)(toHours - (toHours2 - (double)bombardmentData.FlightDuration));
							float num4 = bombardmentData.MissileSpeed * MathF.Sin(bombardmentData.LaunchAngle);
							((Vec2)(ref val3))._002Ector(num2 * num3, num4 * num3 - bombardmentData.Gravity * 0.5f * num3 * num3);
							Vec3 val4 = bombardmentData.LaunchGlobalPosition + ((Vec3)(ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation.f)).NormalizedCopy() * val3.x + ((Vec3)(ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation.u)).NormalizedCopy() * val3.y;
							float num5 = num3 + 0.1f;
							((Vec2)(ref val5))._002Ector(num2 * num5, num4 * num5 - bombardmentData.Gravity * 0.5f * num5 * num5);
							Vec3 val6 = bombardmentData.LaunchGlobalPosition + ((Vec3)(ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation.f)).NormalizedCopy() * val5.x + ((Vec3)(ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation.u)).NormalizedCopy() * val5.y;
							Mat3 rotation = item.GetGlobalFrame().rotation;
							rotation.f = val6 - val4;
							((Mat3)(ref rotation)).Orthonormalize();
							val7 = base.MapScreen.PrefabEntityCache.GetScaleForSiegeEngine(val2.ShooterSiegeEngineType, siegeEventSide.BattleSide);
							((Mat3)(ref rotation)).ApplyScaleLocal(ref val7);
							MatrixFrame val8 = new MatrixFrame(ref rotation, ref val4);
							item.SetGlobalFrame(ref val8, true);
						}
						WeakGameEntity val9 = item.WeakEntity;
						val9 = ((WeakGameEntity)(ref val9)).GetChild(0);
						((WeakGameEntity)(ref val9)).SetVisibilityExcludeParents(flag3);
						int num6 = -1;
						if (!flag2 && flag3)
						{
							num6 = ((val2.ShooterSiegeEngineType != DefaultSiegeEngineTypes.Ballista && val2.ShooterSiegeEngineType != DefaultSiegeEngineTypes.FireBallista) ? ((val2.ShooterSiegeEngineType != DefaultSiegeEngineTypes.Catapult && val2.ShooterSiegeEngineType != DefaultSiegeEngineTypes.FireCatapult && val2.ShooterSiegeEngineType != DefaultSiegeEngineTypes.Onager && val2.ShooterSiegeEngineType != DefaultSiegeEngineTypes.FireOnager) ? MiscSoundContainer.SoundCodeAmbientNodeSiegeTrebuchetFire : MiscSoundContainer.SoundCodeAmbientNodeSiegeMangonelFire) : MiscSoundContainer.SoundCodeAmbientNodeSiegeBallistaFire);
						}
						else if (flag2 && !flag3)
						{
							StrategicEntity.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(((int)val2.TargetType == 2) ? "psys_game_ballista_destruction" : "psys_campaign_boulder_stone_coll"), item.GetGlobalFrame());
							num6 = ((val2.ShooterSiegeEngineType == DefaultSiegeEngineTypes.Ballista || val2.ShooterSiegeEngineType == DefaultSiegeEngineTypes.FireBallista) ? MiscSoundContainer.SoundCodeAmbientNodeSiegeBallistaHit : MiscSoundContainer.SoundCodeAmbientNodeSiegeBoulderHit);
						}
						MBSoundEvent.PlaySound(num6, item.GlobalPosition);
						if (!(toHours < toHours2 - (double)(bombardmentData.TotalDuration - bombardmentData.RotationDuration - bombardmentData.ReloadDuration)))
						{
							MatrixFrame val10;
							MatrixFrame val11;
							if (toHours < toHours2 - (double)(bombardmentData.TotalDuration - bombardmentData.RotationDuration - bombardmentData.ReloadDuration - bombardmentData.AimingDuration))
							{
								if (siegeEventSide.SiegeEngines.DeployedRangedSiegeEngines[val2.ShooterSlotIndex] != null && siegeEventSide.SiegeEngines.DeployedRangedSiegeEngines[val2.ShooterSlotIndex].SiegeEngine == val2.ShooterSiegeEngineType)
								{
									foreach (var siegeRangedMachineEntity in _siegeRangedMachineEntities)
									{
										if (!flag && siegeRangedMachineEntity.Item2 == siegeEventSide.BattleSide && siegeRangedMachineEntity.Item3 == val2.ShooterSlotIndex)
										{
											GameEntity item3 = siegeRangedMachineEntity.Item5;
											if (item3 != (GameEntity)null)
											{
												flag = true;
												val10 = item3.GetGlobalFrame();
												val11 = MBSkeletonExtensions.GetBoneEntitialFrame(item3.Skeleton, Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapProjectileBoneIndex(val2.ShooterSiegeEngineType, siegeEventSide.BattleSide), false);
												val10 = ((MatrixFrame)(ref val10)).TransformToParent(ref val11);
												item.SetGlobalFrame(ref val10, true);
											}
										}
									}
								}
							}
							else if (toHours < toHours2 - (double)(bombardmentData.TotalDuration - bombardmentData.RotationDuration - bombardmentData.ReloadDuration - bombardmentData.AimingDuration - bombardmentData.FireDuration) && !flag3 && siegeEventSide.SiegeEngines.DeployedRangedSiegeEngines[val2.ShooterSlotIndex] != null && siegeEventSide.SiegeEngines.DeployedRangedSiegeEngines[val2.ShooterSlotIndex].SiegeEngine == val2.ShooterSiegeEngineType)
							{
								foreach (var siegeRangedMachineEntity2 in _siegeRangedMachineEntities)
								{
									if (!flag && siegeRangedMachineEntity2.Item2 == siegeEventSide.BattleSide && siegeRangedMachineEntity2.Item3 == val2.ShooterSlotIndex)
									{
										GameEntity item4 = siegeRangedMachineEntity2.Item5;
										if (item4 != (GameEntity)null)
										{
											flag = true;
											val11 = item4.GetGlobalFrame();
											val10 = MBSkeletonExtensions.GetBoneEntitialFrame(item4.Skeleton, Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapProjectileBoneIndex(val2.ShooterSiegeEngineType, siegeEventSide.BattleSide), false);
											val11 = ((MatrixFrame)(ref val11)).TransformToParent(ref val10);
											item.SetGlobalFrame(ref val11, true);
										}
									}
								}
							}
						}
					}
				}
				item.SetVisibilityExcludeParents(flag);
			}
			foreach (var siegeRangedMachineEntity3 in _siegeRangedMachineEntities)
			{
				GameEntity item5 = siegeRangedMachineEntity3.Item1;
				BattleSideEnum item6 = siegeRangedMachineEntity3.Item2;
				int item7 = siegeRangedMachineEntity3.Item3;
				GameEntity item8 = siegeRangedMachineEntity3.Item5;
				SiegeEngineType siegeEngine = base.MapEntity.Settlement.SiegeEvent.GetSiegeEventSide(item6).SiegeEngines.DeployedRangedSiegeEngines[item7].SiegeEngine;
				if (!(item8 != (GameEntity)null))
				{
					continue;
				}
				Skeleton skeleton = item8.Skeleton;
				string siegeEngineMapFireAnimationName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapFireAnimationName(siegeEngine, item6);
				string siegeEngineMapReloadAnimationName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapReloadAnimationName(siegeEngine, item6);
				RangedSiegeEngine rangedSiegeEngine = base.MapEntity.Settlement.SiegeEvent.GetSiegeEventSide(item6).SiegeEngines.DeployedRangedSiegeEngines[item7].RangedSiegeEngine;
				CalculateDataAndDurationsForSiegeMachine(item7, siegeEngine, item6, rangedSiegeEngine.CurrentTargetType, rangedSiegeEngine.CurrentTargetIndex, out var bombardmentData2);
				MatrixFrame shooterGlobalFrame = bombardmentData2.ShooterGlobalFrame;
				if (rangedSiegeEngine.PreviousTargetIndex >= 0)
				{
					Vec3 val12 = (((int)rangedSiegeEngine.PreviousDamagedTargetType != 1) ? (((int)item6 == 1) ? _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[rangedSiegeEngine.PreviousTargetIndex].GetGlobalFrame().origin : _attackerRangedEngineSpawnEntities[rangedSiegeEngine.PreviousTargetIndex].GetGlobalFrame().origin) : _defenderBreachableWallEntitiesCacheForCurrentLevel[rangedSiegeEngine.PreviousTargetIndex].GlobalPosition);
					ref Vec3 f = ref shooterGlobalFrame.rotation.f;
					val7 = val12 - shooterGlobalFrame.origin;
					((Vec3)(ref f)).AsVec2 = ((Vec3)(ref val7)).AsVec2;
					((Vec3)(ref shooterGlobalFrame.rotation.f)).NormalizeWithoutChangingZ();
					((Mat3)(ref shooterGlobalFrame.rotation)).Orthonormalize();
				}
				item5.SetGlobalFrame(ref shooterGlobalFrame, true);
				skeleton.TickAnimations(dt, MatrixFrame.Identity, false);
				val = rangedSiegeEngine.NextProjectileCollisionTime;
				double toHours3 = ((CampaignTime)(ref val)).ToHours;
				if (!(toHours > toHours3 - (double)bombardmentData2.TotalDuration))
				{
					continue;
				}
				if (toHours < toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration))
				{
					val7 = bombardmentData2.TargetPosition - shooterGlobalFrame.origin;
					Vec2 asVec = ((Vec3)(ref val7)).AsVec2;
					float rotationInRadians = ((Vec2)(ref asVec)).RotationInRadians;
					asVec = ((Vec3)(ref shooterGlobalFrame.rotation.f)).AsVec2;
					float rotationInRadians2 = ((Vec2)(ref asVec)).RotationInRadians;
					float num7 = rotationInRadians - rotationInRadians2;
					float num8 = MathF.Abs(num7);
					float num9 = (float)(toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration) - toHours);
					if (num8 > num9 * 2f)
					{
						((Vec3)(ref shooterGlobalFrame.rotation.f)).AsVec2 = Vec2.FromRotation(rotationInRadians2 + (float)MathF.Sign(num7) * (num8 - num9 * 2f));
						((Vec3)(ref shooterGlobalFrame.rotation.f)).NormalizeWithoutChangingZ();
						((Mat3)(ref shooterGlobalFrame.rotation)).Orthonormalize();
						item5.SetGlobalFrame(ref shooterGlobalFrame, true);
					}
				}
				else if (toHours < toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration - bombardmentData2.ReloadDuration))
				{
					item5.SetGlobalFrame(ref bombardmentData2.TargetAlignedShooterGlobalFrame, true);
					MBSkeletonExtensions.SetAnimationAtChannel(skeleton, siegeEngineMapReloadAnimationName, 0, 1f, 0f, (float)((toHours - (toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration))) / (double)bombardmentData2.ReloadDuration));
				}
				else if (toHours < toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration - bombardmentData2.ReloadDuration - bombardmentData2.AimingDuration))
				{
					item5.SetGlobalFrame(ref bombardmentData2.TargetAlignedShooterGlobalFrame, true);
					MBSkeletonExtensions.SetAnimationAtChannel(skeleton, siegeEngineMapReloadAnimationName, 0, 1f, 0f, 1f);
				}
				else if (toHours < toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration - bombardmentData2.ReloadDuration - bombardmentData2.AimingDuration - bombardmentData2.FireDuration))
				{
					item5.SetGlobalFrame(ref bombardmentData2.TargetAlignedShooterGlobalFrame, true);
					MBSkeletonExtensions.SetAnimationAtChannel(skeleton, siegeEngineMapFireAnimationName, 0, 1f, 0f, (float)((toHours - (toHours3 - (double)(bombardmentData2.TotalDuration - bombardmentData2.RotationDuration - bombardmentData2.ReloadDuration - bombardmentData2.AimingDuration))) / (double)bombardmentData2.FireDuration));
				}
				else
				{
					item5.SetGlobalFrame(ref bombardmentData2.TargetAlignedShooterGlobalFrame, true);
					MBSkeletonExtensions.SetAnimationAtChannel(skeleton, siegeEngineMapFireAnimationName, 0, 1f, 0f, 1f);
				}
			}
		}
		if (base.MapEntity.LevelMaskIsDirty)
		{
			RefreshLevelMask();
		}
	}

	internal void OnMapHoverSiegeEngine(MatrixFrame engineFrame)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSiege.PlayerSiegeEvent == null)
		{
			return;
		}
		for (int i = 0; i < _attackerBatteringRamSpawnEntities.Length; i++)
		{
			MatrixFrame globalFrame = _attackerBatteringRamSpawnEntities[i].GetGlobalFrame();
			if (((MatrixFrame)(ref globalFrame)).NearlyEquals(engineFrame, 1E-05f))
			{
				if ((ref _hoveredSiegeEntityFrame) != (ref globalFrame))
				{
					SiegeEngineConstructionProgress engineInProgress = PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedMeleeSiegeEngines[i];
					InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetSiegeEngineInProgressTooltip(engineInProgress) });
				}
				return;
			}
		}
		for (int j = 0; j < _attackerSiegeTowerSpawnEntities.Length; j++)
		{
			MatrixFrame globalFrame2 = _attackerSiegeTowerSpawnEntities[j].GetGlobalFrame();
			if (((MatrixFrame)(ref globalFrame2)).NearlyEquals(engineFrame, 1E-05f))
			{
				if ((ref _hoveredSiegeEntityFrame) != (ref globalFrame2))
				{
					SiegeEngineConstructionProgress engineInProgress2 = PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedMeleeSiegeEngines[_attackerBatteringRamSpawnEntities.Length + j];
					InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetSiegeEngineInProgressTooltip(engineInProgress2) });
				}
				return;
			}
		}
		for (int k = 0; k < _attackerRangedEngineSpawnEntities.Length; k++)
		{
			MatrixFrame globalFrame3 = _attackerRangedEngineSpawnEntities[k].GetGlobalFrame();
			if (((MatrixFrame)(ref globalFrame3)).NearlyEquals(engineFrame, 1E-05f))
			{
				if ((ref _hoveredSiegeEntityFrame) != (ref globalFrame3))
				{
					SiegeEngineConstructionProgress engineInProgress3 = PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedRangedSiegeEngines[k];
					InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetSiegeEngineInProgressTooltip(engineInProgress3) });
				}
				return;
			}
		}
		for (int l = 0; l < _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel.Length; l++)
		{
			MatrixFrame globalFrame4 = _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[l].GetGlobalFrame();
			if (((MatrixFrame)(ref globalFrame4)).NearlyEquals(engineFrame, 1E-05f))
			{
				if ((ref _hoveredSiegeEntityFrame) != (ref globalFrame4))
				{
					SiegeEngineConstructionProgress engineInProgress4 = PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide((BattleSideEnum)0).SiegeEngines.DeployedRangedSiegeEngines[l];
					InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetSiegeEngineInProgressTooltip(engineInProgress4) });
				}
				return;
			}
		}
		for (int m = 0; m < _defenderBreachableWallEntitiesCacheForCurrentLevel.Length; m++)
		{
			MatrixFrame globalFrame5 = _defenderBreachableWallEntitiesCacheForCurrentLevel[m].GetGlobalFrame();
			if (((MatrixFrame)(ref globalFrame5)).NearlyEquals(engineFrame, 1E-05f))
			{
				if ((ref _hoveredSiegeEntityFrame) != (ref globalFrame5) && base.MapEntity.IsSettlement)
				{
					InformationManager.ShowTooltip(typeof(List<TooltipProperty>), new object[1] { SandBoxUIHelper.GetWallSectionTooltip(base.MapEntity.Settlement, m) });
				}
				return;
			}
		}
		_hoveredSiegeEntityFrame = MatrixFrame.Identity;
	}

	private void RemoveSiege()
	{
		foreach (var siegeRangedMachineEntity in _siegeRangedMachineEntities)
		{
			StrategicEntity.RemoveChild(siegeRangedMachineEntity.Item1, false, false, true, 36);
		}
		foreach (var siegeMissileEntity in _siegeMissileEntities)
		{
			StrategicEntity.RemoveChild(siegeMissileEntity.Item1, false, false, true, 37);
		}
		foreach (var siegeMeleeMachineEntity in _siegeMeleeMachineEntities)
		{
			StrategicEntity.RemoveChild(siegeMeleeMachineEntity.Item1, false, false, true, 38);
		}
		_siegeRangedMachineEntities.Clear();
		_siegeMeleeMachineEntities.Clear();
		_siegeMissileEntities.Clear();
	}

	private void AddSiegeIconComponents(PartyBase party)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		if (!party.Settlement.IsUnderSiege)
		{
			return;
		}
		int wallLevel = -1;
		if (party.Settlement.SiegeEvent.BesiegedSettlement.IsTown || party.Settlement.SiegeEvent.BesiegedSettlement.IsCastle)
		{
			wallLevel = party.Settlement.SiegeEvent.BesiegedSettlement.Town.GetWallLevel();
		}
		SiegeEngineConstructionProgress[] deployedRangedSiegeEngines = party.Settlement.SiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedRangedSiegeEngines;
		for (int i = 0; i < deployedRangedSiegeEngines.Length; i++)
		{
			SiegeEngineConstructionProgress obj = deployedRangedSiegeEngines[i];
			if (obj != null && obj.IsActive && i < _attackerRangedEngineSpawnEntities.Length)
			{
				MatrixFrame globalFrame = _attackerRangedEngineSpawnEntities[i].GetGlobalFrame();
				((Mat3)(ref globalFrame.rotation)).MakeUnit();
				AddSiegeMachine(deployedRangedSiegeEngines[i].SiegeEngine, globalFrame, (BattleSideEnum)1, wallLevel, i);
			}
		}
		SiegeEngineConstructionProgress[] deployedMeleeSiegeEngines = party.Settlement.SiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedMeleeSiegeEngines;
		for (int j = 0; j < deployedMeleeSiegeEngines.Length; j++)
		{
			SiegeEngineConstructionProgress obj2 = deployedMeleeSiegeEngines[j];
			if (obj2 == null || !obj2.IsActive)
			{
				continue;
			}
			if (deployedMeleeSiegeEngines[j].SiegeEngine == DefaultSiegeEngineTypes.SiegeTower)
			{
				int num = j - _attackerBatteringRamSpawnEntities.Length;
				if (num >= 0)
				{
					MatrixFrame globalFrame2 = _attackerSiegeTowerSpawnEntities[num].GetGlobalFrame();
					((Mat3)(ref globalFrame2.rotation)).MakeUnit();
					AddSiegeMachine(deployedMeleeSiegeEngines[j].SiegeEngine, globalFrame2, (BattleSideEnum)1, wallLevel, j);
				}
			}
			else if (deployedMeleeSiegeEngines[j].SiegeEngine == DefaultSiegeEngineTypes.Ram || deployedMeleeSiegeEngines[j].SiegeEngine == DefaultSiegeEngineTypes.ImprovedRam)
			{
				int num2 = j;
				if (num2 >= 0)
				{
					MatrixFrame globalFrame3 = _attackerBatteringRamSpawnEntities[num2].GetGlobalFrame();
					((Mat3)(ref globalFrame3.rotation)).MakeUnit();
					AddSiegeMachine(deployedMeleeSiegeEngines[j].SiegeEngine, globalFrame3, (BattleSideEnum)1, wallLevel, j);
				}
			}
		}
		SiegeEngineConstructionProgress[] deployedRangedSiegeEngines2 = party.Settlement.SiegeEvent.GetSiegeEventSide((BattleSideEnum)0).SiegeEngines.DeployedRangedSiegeEngines;
		for (int k = 0; k < deployedRangedSiegeEngines2.Length; k++)
		{
			SiegeEngineConstructionProgress obj3 = deployedRangedSiegeEngines2[k];
			if (obj3 != null && obj3.IsActive)
			{
				MatrixFrame globalFrame4 = _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[k].GetGlobalFrame();
				((Mat3)(ref globalFrame4.rotation)).MakeUnit();
				AddSiegeMachine(deployedRangedSiegeEngines2[k].SiegeEngine, globalFrame4, (BattleSideEnum)0, wallLevel, k);
			}
		}
		for (int l = 0; l < 2; l++)
		{
			BattleSideEnum val = (BattleSideEnum)(l == 0);
			MBReadOnlyList<SiegeEngineMissile> siegeEngineMissiles = party.Settlement.SiegeEvent.GetSiegeEventSide(val).SiegeEngineMissiles;
			for (int m = 0; m < ((List<SiegeEngineMissile>)(object)siegeEngineMissiles).Count; m++)
			{
				AddSiegeMissile(((List<SiegeEngineMissile>)(object)siegeEngineMissiles)[m].ShooterSiegeEngineType, StrategicEntity.GetGlobalFrame(), val, m);
			}
		}
	}

	private void AddSiegeMachine(SiegeEngineType type, MatrixFrame globalFrame, BattleSideEnum side, int wallLevel, int slotIndex)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		string siegeEngineMapPrefabName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapPrefabName(type, wallLevel, side);
		GameEntity val = GameEntity.Instantiate(MapScene, siegeEngineMapPrefabName, true, true, "");
		if (!(val != (GameEntity)null))
		{
			return;
		}
		StrategicEntity.AddChild(val, false);
		MatrixFrame val2 = default(MatrixFrame);
		val.GetLocalFrame(ref val2);
		MatrixFrame val3 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2);
		val.SetGlobalFrame(ref val3, true);
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity weakEntity = val.WeakEntity;
		((WeakGameEntity)(ref weakEntity)).GetChildrenRecursive(ref list);
		GameEntity val4 = null;
		if (list.Any((WeakGameEntity entity) => ((WeakGameEntity)(ref entity)).HasTag("siege_machine_mapicon_skeleton")))
		{
			WeakGameEntity val5 = list.Find((WeakGameEntity entity) => ((WeakGameEntity)(ref entity)).HasTag("siege_machine_mapicon_skeleton"));
			if ((NativeObject)(object)((WeakGameEntity)(ref val5)).Skeleton != (NativeObject)null)
			{
				val4 = GameEntity.CreateFromWeakEntity(val5);
				string siegeEngineMapFireAnimationName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapFireAnimationName(type, side);
				MBSkeletonExtensions.SetAnimationAtChannel(val4.Skeleton, siegeEngineMapFireAnimationName, 0, 1f, 0f, 1f);
			}
		}
		if (type.IsRanged)
		{
			_siegeRangedMachineEntities.Add(ValueTuple.Create<GameEntity, BattleSideEnum, int, MatrixFrame, GameEntity>(val, side, slotIndex, globalFrame, val4));
		}
		else
		{
			_siegeMeleeMachineEntities.Add(ValueTuple.Create<GameEntity, BattleSideEnum, int, MatrixFrame, GameEntity>(val, side, slotIndex, globalFrame, val4));
		}
	}

	private void AddSiegeMissile(SiegeEngineType type, MatrixFrame globalFrame, BattleSideEnum side, int missileIndex)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		string siegeEngineMapProjectilePrefabName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapProjectilePrefabName(type);
		GameEntity val = GameEntity.Instantiate(MapScene, siegeEngineMapProjectilePrefabName, true, true, "");
		if (val != (GameEntity)null)
		{
			_siegeMissileEntities.Add(ValueTuple.Create<GameEntity, BattleSideEnum, int>(val, side, missileIndex));
			StrategicEntity.AddChild(val, false);
			GameEntity strategicEntity = StrategicEntity;
			strategicEntity.EntityFlags = (EntityFlags)(strategicEntity.EntityFlags & -536870913);
			MatrixFrame val2 = default(MatrixFrame);
			val.GetLocalFrame(ref val2);
			MatrixFrame val3 = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val2);
			val.SetGlobalFrame(ref val3, true);
			val.SetVisibilityExcludeParents(false);
		}
	}

	private void SetLevelMask(uint newMask)
	{
		_currentLevelMask = newMask;
		base.MapEntity.SetVisualAsDirty();
	}

	private void RefreshLevelMask()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		uint num = 0u;
		if (base.MapEntity.Settlement.IsVillage)
		{
			num = (((int)base.MapEntity.Settlement.Village.VillageState != 4) ? (num | Campaign.Current.MapSceneWrapper.GetSceneLevel("civilian")) : (num | Campaign.Current.MapSceneWrapper.GetSceneLevel("looted")));
			num |= GetLevelOfProduction(base.MapEntity.Settlement);
		}
		else if (base.MapEntity.Settlement.IsTown || base.MapEntity.Settlement.IsCastle)
		{
			if (base.MapEntity.Settlement.Town.GetWallLevel() == 1)
			{
				num |= Campaign.Current.MapSceneWrapper.GetSceneLevel("level_1");
			}
			else if (base.MapEntity.Settlement.Town.GetWallLevel() == 2)
			{
				num |= Campaign.Current.MapSceneWrapper.GetSceneLevel("level_2");
			}
			else if (base.MapEntity.Settlement.Town.GetWallLevel() == 3)
			{
				num |= Campaign.Current.MapSceneWrapper.GetSceneLevel("level_3");
			}
			num = ((base.MapEntity.Settlement.SiegeEvent == null) ? (num | Campaign.Current.MapSceneWrapper.GetSceneLevel("civilian")) : (num | Campaign.Current.MapSceneWrapper.GetSceneLevel("siege")));
		}
		else if (base.MapEntity.Settlement.IsHideout)
		{
			num |= Campaign.Current.MapSceneWrapper.GetSceneLevel("level_1");
		}
		if (_currentLevelMask != num)
		{
			SetLevelMask(num);
		}
		base.MapEntity.OnLevelMaskUpdated();
	}

	private static uint GetLevelOfProduction(Settlement settlement)
	{
		uint num = 0u;
		if (settlement.Village.Hearth < 200f)
		{
			return num | Campaign.Current.MapSceneWrapper.GetSceneLevel("level_1");
		}
		if (settlement.Village.Hearth < 600f)
		{
			return num | Campaign.Current.MapSceneWrapper.GetSceneLevel("level_2");
		}
		return num | Campaign.Current.MapSceneWrapper.GetSceneLevel("level_3");
	}

	private void SetSettlementLevelVisibility()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity weakEntity = StrategicEntity.WeakEntity;
		((WeakGameEntity)(ref weakEntity)).GetChildrenRecursive(ref list);
		foreach (WeakGameEntity item in list)
		{
			WeakGameEntity current = item;
			if ((int)(((WeakGameEntity)(ref current)).GetUpgradeLevelMask() & _currentLevelMask) == (int)_currentLevelMask)
			{
				((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(true);
				GameEntityPhysicsExtensions.SetPhysicsState(current, true, true);
			}
			else
			{
				((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(false);
				GameEntityPhysicsExtensions.SetPhysicsState(current, false, true);
			}
		}
	}

	private void PopulateSiegeEngineFrameListsFromChildren(List<GameEntity> children)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		_attackerRangedEngineSpawnEntities = (from e in children.FindAll((GameEntity x) => x.Tags.Any((string t) => t.Contains("map_siege_engine")))
			orderby e.Tags.First((string s) => s.Contains("map_siege_engine"))
			select e).ToArray();
		GameEntity[] attackerRangedEngineSpawnEntities = _attackerRangedEngineSpawnEntities;
		foreach (GameEntity val in attackerRangedEngineSpawnEntities)
		{
			if (val.ChildCount > 0 && !MapScreen.FrameAndVisualOfEngines.ContainsKey(((NativeObject)val.GetChild(0)).Pointer))
			{
				MapScreen.FrameAndVisualOfEngines.Add(((NativeObject)val.GetChild(0)).Pointer, new Tuple<MatrixFrame, SettlementVisual>(val.GetGlobalFrame(), this));
			}
		}
		_defenderRangedEngineSpawnEntitiesForAllLevels = (from e in children.FindAll((GameEntity x) => x.Tags.Any((string t) => t.Contains("map_defensive_engine")))
			orderby e.Tags.First((string s) => s.Contains("map_defensive_engine"))
			select e).ToArray();
		attackerRangedEngineSpawnEntities = _defenderRangedEngineSpawnEntitiesForAllLevels;
		foreach (GameEntity val2 in attackerRangedEngineSpawnEntities)
		{
			if (val2.ChildCount > 0 && !MapScreen.FrameAndVisualOfEngines.ContainsKey(((NativeObject)val2.GetChild(0)).Pointer))
			{
				MapScreen.FrameAndVisualOfEngines.Add(((NativeObject)val2.GetChild(0)).Pointer, new Tuple<MatrixFrame, SettlementVisual>(val2.GetGlobalFrame(), this));
			}
		}
		_attackerBatteringRamSpawnEntities = children.FindAll((GameEntity x) => x.HasTag("map_siege_ram")).ToArray();
		attackerRangedEngineSpawnEntities = _attackerBatteringRamSpawnEntities;
		foreach (GameEntity val3 in attackerRangedEngineSpawnEntities)
		{
			if (val3.ChildCount > 0 && !MapScreen.FrameAndVisualOfEngines.ContainsKey(((NativeObject)val3.GetChild(0)).Pointer))
			{
				MapScreen.FrameAndVisualOfEngines.Add(((NativeObject)val3.GetChild(0)).Pointer, new Tuple<MatrixFrame, SettlementVisual>(val3.GetGlobalFrame(), this));
			}
		}
		_attackerSiegeTowerSpawnEntities = children.FindAll((GameEntity x) => x.HasTag("map_siege_tower")).ToArray();
		attackerRangedEngineSpawnEntities = _attackerSiegeTowerSpawnEntities;
		foreach (GameEntity val4 in attackerRangedEngineSpawnEntities)
		{
			if (val4.ChildCount > 0 && !MapScreen.FrameAndVisualOfEngines.ContainsKey(((NativeObject)val4.GetChild(0)).Pointer))
			{
				MapScreen.FrameAndVisualOfEngines.Add(((NativeObject)val4.GetChild(0)).Pointer, new Tuple<MatrixFrame, SettlementVisual>(val4.GetGlobalFrame(), this));
			}
		}
		_defenderBreachableWallEntitiesForAllLevels = children.FindAll((GameEntity x) => x.HasTag("map_breachable_wall")).ToArray();
		attackerRangedEngineSpawnEntities = _defenderBreachableWallEntitiesForAllLevels;
		foreach (GameEntity val5 in attackerRangedEngineSpawnEntities)
		{
			if (val5.ChildCount > 0 && !MapScreen.FrameAndVisualOfEngines.ContainsKey(((NativeObject)val5.GetChild(0)).Pointer))
			{
				MapScreen.FrameAndVisualOfEngines.Add(((NativeObject)val5.GetChild(0)).Pointer, new Tuple<MatrixFrame, SettlementVisual>(val5.GetGlobalFrame(), this));
			}
		}
	}

	private void UpdateDefenderSiegeEntitiesCache()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		UpgradeLevelMask currentSettlementUpgradeLevelMask = (UpgradeLevelMask)0;
		if (base.MapEntity.IsSettlement && base.MapEntity.Settlement.IsFortification)
		{
			if (base.MapEntity.Settlement.Town.GetWallLevel() == 1)
			{
				currentSettlementUpgradeLevelMask = (UpgradeLevelMask)2;
			}
			else if (base.MapEntity.Settlement.Town.GetWallLevel() == 2)
			{
				currentSettlementUpgradeLevelMask = (UpgradeLevelMask)4;
			}
			else if (base.MapEntity.Settlement.Town.GetWallLevel() == 3)
			{
				currentSettlementUpgradeLevelMask = (UpgradeLevelMask)8;
			}
		}
		_currentSettlementUpgradeLevelMask = currentSettlementUpgradeLevelMask;
		_defenderRangedEngineSpawnEntitiesCacheForCurrentLevel = _defenderRangedEngineSpawnEntitiesForAllLevels.Where((GameEntity e) => (UpgradeLevelMask)(e.GetUpgradeLevelMask() & _currentSettlementUpgradeLevelMask) == _currentSettlementUpgradeLevelMask).ToArray();
		_defenderBreachableWallEntitiesCacheForCurrentLevel = _defenderBreachableWallEntitiesForAllLevels.Where((GameEntity e) => (UpgradeLevelMask)(e.GetUpgradeLevelMask() & _currentSettlementUpgradeLevelMask) == _currentSettlementUpgradeLevelMask).ToArray();
	}

	private void RefreshWallState()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		if (_defenderBreachableWallEntitiesForAllLevels == null)
		{
			return;
		}
		PartyBase mapEntity = base.MapEntity;
		MBReadOnlyList<float> val = ((((mapEntity != null) ? mapEntity.Settlement : null) != null && (base.MapEntity.Settlement == null || base.MapEntity.Settlement.IsFortification)) ? base.MapEntity.Settlement.SettlementWallSectionHitPointsRatioList : null);
		if (val == null)
		{
			return;
		}
		if (((List<float>)(object)val).Count == 0)
		{
			Debug.FailedAssert("Town (" + ((object)base.MapEntity.Settlement.Name).ToString() + ") doesn't have wall entities defined for it's current level(" + base.MapEntity.Settlement.Town.GetWallLevel() + ")", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Visuals\\SettlementVisual.cs", "RefreshWallState", 1309);
			return;
		}
		for (int i = 0; i < _defenderBreachableWallEntitiesForAllLevels.Length; i++)
		{
			bool flag = ((List<float>)(object)val)[i % ((List<float>)(object)val).Count] <= 0f;
			WeakGameEntity weakEntity = _defenderBreachableWallEntitiesForAllLevels[i].WeakEntity;
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref weakEntity)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).HasTag("map_solid_wall"))
				{
					((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(!flag);
				}
				else if (((WeakGameEntity)(ref current)).HasTag("map_broken_wall"))
				{
					((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(flag);
				}
			}
		}
	}

	private void RefreshTownPhysicalEntitiesState(PartyBase party)
	{
		if (((party != null) ? party.Settlement : null) == null || !party.Settlement.IsFortification || TownPhysicalEntities == null)
		{
			return;
		}
		if (PlayerSiege.PlayerSiegeEvent != null && PlayerSiege.PlayerSiegeEvent.BesiegedSettlement == party.Settlement)
		{
			TownPhysicalEntities.ForEach(delegate(GameEntity p)
			{
				p.AddBodyFlags((BodyFlags)1, true);
			});
		}
		else
		{
			TownPhysicalEntities.ForEach(delegate(GameEntity p)
			{
				p.RemoveBodyFlags((BodyFlags)1, true);
			});
		}
	}

	private void RefreshSiegePreparations(PartyBase party)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		List<WeakGameEntity> list = new List<WeakGameEntity>();
		WeakGameEntity weakEntity = StrategicEntity.WeakEntity;
		((WeakGameEntity)(ref weakEntity)).GetChildrenRecursive(ref list);
		List<WeakGameEntity> list2 = list.FindAll((WeakGameEntity x) => ((WeakGameEntity)(ref x)).HasTag("siege_preparation"));
		bool flag = false;
		if (party.Settlement != null && party.Settlement.IsUnderSiege)
		{
			SiegeEngineConstructionProgress siegePreparations = party.Settlement.SiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.SiegePreparations;
			if (siegePreparations != null && siegePreparations.Progress >= 1f)
			{
				flag = true;
				foreach (WeakGameEntity item in list2)
				{
					WeakGameEntity current = item;
					((WeakGameEntity)(ref current)).SetVisibilityExcludeParents(true);
				}
			}
		}
		if (flag)
		{
			return;
		}
		foreach (WeakGameEntity item2 in list2)
		{
			WeakGameEntity current2 = item2;
			((WeakGameEntity)(ref current2)).SetVisibilityExcludeParents(false);
		}
	}

	public MatrixFrame[] GetAttackerTowerSiegeEngineFrames()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame[] array = (MatrixFrame[])(object)new MatrixFrame[_attackerSiegeTowerSpawnEntities.Length];
		for (int i = 0; i < _attackerSiegeTowerSpawnEntities.Length; i++)
		{
			array[i] = _attackerSiegeTowerSpawnEntities[i].GetGlobalFrame();
		}
		return array;
	}

	public MatrixFrame[] GetAttackerBatteringRamSiegeEngineFrames()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame[] array = (MatrixFrame[])(object)new MatrixFrame[_attackerBatteringRamSpawnEntities.Length];
		for (int i = 0; i < _attackerBatteringRamSpawnEntities.Length; i++)
		{
			array[i] = _attackerBatteringRamSpawnEntities[i].GetGlobalFrame();
		}
		return array;
	}

	public MatrixFrame[] GetAttackerRangedSiegeEngineFrames()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame[] array = (MatrixFrame[])(object)new MatrixFrame[_attackerRangedEngineSpawnEntities.Length];
		for (int i = 0; i < _attackerRangedEngineSpawnEntities.Length; i++)
		{
			array[i] = _attackerRangedEngineSpawnEntities[i].GetGlobalFrame();
		}
		return array;
	}

	public MatrixFrame[] GetDefenderRangedSiegeEngineFrames()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame[] array = (MatrixFrame[])(object)new MatrixFrame[_defenderRangedEngineSpawnEntitiesCacheForCurrentLevel.Length];
		for (int i = 0; i < _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel.Length; i++)
		{
			array[i] = _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[i].GetGlobalFrame();
		}
		return array;
	}

	public MatrixFrame[] GetBreachableWallFrames()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame[] array = (MatrixFrame[])(object)new MatrixFrame[_defenderBreachableWallEntitiesCacheForCurrentLevel.Length];
		for (int i = 0; i < _defenderBreachableWallEntitiesCacheForCurrentLevel.Length; i++)
		{
			array[i] = _defenderBreachableWallEntitiesCacheForCurrentLevel[i].GetGlobalFrame();
		}
		return array;
	}

	private void CalculateDataAndDurationsForSiegeMachine(int machineSlotIndex, SiegeEngineType machineType, BattleSideEnum side, SiegeBombardTargets targetType, int targetSlotIndex, out SiegeBombardmentData bombardmentData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Invalid comparison between Unknown and I4
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Invalid comparison between Unknown and I4
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Invalid comparison between Unknown and I4
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		bombardmentData = default(SiegeBombardmentData);
		MatrixFrame shooterGlobalFrame = (((int)side == 0) ? _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[machineSlotIndex].GetGlobalFrame() : _attackerRangedEngineSpawnEntities[machineSlotIndex].GetGlobalFrame());
		((Mat3)(ref shooterGlobalFrame.rotation)).MakeUnit();
		bombardmentData.ShooterGlobalFrame = shooterGlobalFrame;
		string siegeEngineMapFireAnimationName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapFireAnimationName(machineType, side);
		string siegeEngineMapReloadAnimationName = Campaign.Current.Models.SiegeEventModel.GetSiegeEngineMapReloadAnimationName(machineType, side);
		bombardmentData.ReloadDuration = MBAnimation.GetAnimationDuration(siegeEngineMapReloadAnimationName) * 0.25f;
		bombardmentData.AimingDuration = 0.25f;
		bombardmentData.RotationDuration = 0.4f;
		bombardmentData.FireDuration = MBAnimation.GetAnimationDuration(siegeEngineMapFireAnimationName) * 0.25f;
		float animationParameter = MBAnimation.GetAnimationParameter1(siegeEngineMapFireAnimationName);
		bombardmentData.MissileLaunchDuration = bombardmentData.FireDuration * animationParameter;
		bombardmentData.MissileSpeed = 14f;
		bombardmentData.Gravity = ((machineType == DefaultSiegeEngineTypes.Ballista || machineType == DefaultSiegeEngineTypes.FireBallista) ? 10f : 40f);
		Vec3 val2;
		if ((int)targetType == 2)
		{
			bombardmentData.TargetPosition = (((int)side == 1) ? _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[targetSlotIndex].GetGlobalFrame().origin : _attackerRangedEngineSpawnEntities[targetSlotIndex].GetGlobalFrame().origin);
		}
		else if ((int)targetType == 1)
		{
			bombardmentData.TargetPosition = _defenderBreachableWallEntitiesCacheForCurrentLevel[targetSlotIndex].GlobalPosition;
		}
		else if (targetSlotIndex == -1)
		{
			bombardmentData.TargetPosition = Vec3.Zero;
		}
		else
		{
			bombardmentData.TargetPosition = (((int)side == 1) ? _defenderRangedEngineSpawnEntitiesCacheForCurrentLevel[targetSlotIndex].GetGlobalFrame().origin : _attackerRangedEngineSpawnEntities[targetSlotIndex].GetGlobalFrame().origin);
			ref Vec3 targetPosition = ref bombardmentData.TargetPosition;
			Vec3 val = targetPosition;
			val2 = bombardmentData.TargetPosition - bombardmentData.ShooterGlobalFrame.origin;
			targetPosition = val + ((Vec3)(ref val2)).NormalizedCopy() * 2f;
			IMapScene mapSceneWrapper = Campaign.Current.MapSceneWrapper;
			CampaignVec2 val3 = new CampaignVec2(((Vec3)(ref bombardmentData.TargetPosition)).AsVec2, true);
			mapSceneWrapper.GetHeightAtPoint(ref val3, ref bombardmentData.TargetPosition.z);
		}
		bombardmentData.TargetAlignedShooterGlobalFrame = bombardmentData.ShooterGlobalFrame;
		ref Vec3 f = ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation.f;
		val2 = bombardmentData.TargetPosition - bombardmentData.ShooterGlobalFrame.origin;
		((Vec3)(ref f)).AsVec2 = ((Vec3)(ref val2)).AsVec2;
		((Vec3)(ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation.f)).NormalizeWithoutChangingZ();
		((Mat3)(ref bombardmentData.TargetAlignedShooterGlobalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		ref MatrixFrame targetAlignedShooterGlobalFrame = ref bombardmentData.TargetAlignedShooterGlobalFrame;
		MatrixFrame launchEntitialFrameForSiegeEngine = base.MapScreen.PrefabEntityCache.GetLaunchEntitialFrameForSiegeEngine(machineType, side);
		bombardmentData.LaunchGlobalPosition = ((MatrixFrame)(ref targetAlignedShooterGlobalFrame)).TransformToParent(ref launchEntitialFrameForSiegeEngine.origin);
		Vec2 val4 = ((Vec3)(ref bombardmentData.LaunchGlobalPosition)).AsVec2 - ((Vec3)(ref bombardmentData.TargetPosition)).AsVec2;
		float lengthSquared = ((Vec2)(ref val4)).LengthSquared;
		float num = MathF.Sqrt(lengthSquared);
		float num2 = bombardmentData.LaunchGlobalPosition.z - bombardmentData.TargetPosition.z;
		float num3 = bombardmentData.MissileSpeed * bombardmentData.MissileSpeed;
		float num4 = num3 * num3;
		float num5 = num4 - bombardmentData.Gravity * (bombardmentData.Gravity * lengthSquared - 2f * num2 * num3);
		if (num5 >= 0f)
		{
			bombardmentData.LaunchAngle = MathF.Atan((num3 - MathF.Sqrt(num5)) / (bombardmentData.Gravity * num));
		}
		else
		{
			bombardmentData.Gravity = 1f;
			num5 = num4 - bombardmentData.Gravity * (bombardmentData.Gravity * lengthSquared - 2f * num2 * num3);
			bombardmentData.LaunchAngle = MathF.Atan((num3 - MathF.Sqrt(num5)) / (bombardmentData.Gravity * num));
		}
		float num6 = bombardmentData.MissileSpeed * MathF.Cos(bombardmentData.LaunchAngle);
		bombardmentData.FlightDuration = num / num6;
		bombardmentData.TotalDuration = bombardmentData.RotationDuration + bombardmentData.ReloadDuration + bombardmentData.AimingDuration + bombardmentData.MissileLaunchDuration + bombardmentData.FlightDuration;
	}
}
