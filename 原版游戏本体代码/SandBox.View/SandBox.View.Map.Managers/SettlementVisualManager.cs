using System;
using System.Collections.Generic;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Managers;

public class SettlementVisualManager : EntityVisualManagerBase<PartyBase>
{
	private const string _emptyAttackerRangedDecalMaterialName = "decal_siege_ranged";

	private const string _attackerRamMachineDecalMaterialName = "decal_siege_ram";

	private const string _attackerTowerMachineDecalMaterialName = "decal_siege_tower";

	private const string _attackerRangedMachineDecalMaterialName = "decal_siege_ranged";

	private const string _defenderRangedMachineDecalMaterialName = "decal_defender_ranged_siege";

	private const uint _preperationOrEnemySiegeEngineDecalColor = 4287064638u;

	private const uint _normalStartSiegeEngineDecalColor = 4278394186u;

	private const float _defenderMachineCircleDecalScale = 0.25f;

	private const float _attackerMachineDecalScale = 0.38f;

	private bool _isNewDecalScaleImplementationEnabled;

	private const uint _normalEndSiegeEngineDecalColor = 4284320212u;

	private const uint _hoveredSiegeEngineDecalColor = 4293956364u;

	private const uint _withMachineSiegeEngineDecalColor = 4283683126u;

	private const float _machineDecalAnimLoopTime = 0.5f;

	private readonly Dictionary<PartyBase, SettlementVisual> _settlementVisuals = new Dictionary<PartyBase, SettlementVisual>();

	private readonly List<SettlementVisual> _visualsFlattened = new List<SettlementVisual>();

	private int _dirtyPartyVisualCount;

	private SettlementVisual[] _dirtyPartiesList = new SettlementVisual[2500];

	private UIntPtr _hoveredSiegeEntityID;

	private bool _playerSiegeMachineSlotMeshesAdded;

	private MapView _mapSiegeOverlayView;

	private GameEntity[] _defenderMachinesCircleEntities;

	private GameEntity[] _attackerRamMachinesCircleEntities;

	private GameEntity[] _attackerTowerMachinesCircleEntities;

	private GameEntity[] _attackerRangedMachinesCircleEntities;

	private float _timeSinceCreation;

	public override int Priority => 40;

	public static SettlementVisualManager Current => SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<SettlementVisualManager>();

	public override void OnTick(float realDt, float dt)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		_dirtyPartyVisualCount = -1;
		TWParallel.For(0, _visualsFlattened.Count, (ParallelForAuxPredicate)delegate(int startInclusive, int endExclusive)
		{
			for (int i = startInclusive; i < endExclusive; i++)
			{
				_visualsFlattened[i].Tick(dt, ref _dirtyPartyVisualCount, ref _dirtyPartiesList);
			}
		}, 16);
		for (int num = 0; num < _dirtyPartyVisualCount + 1; num++)
		{
			_dirtyPartiesList[num].ValidateIsDirty();
		}
	}

	public override bool OnVisualIntersected(Ray mouseRay, UIntPtr[] intersectedEntityIDs, Intersection[] intersectionInfos, int entityCount, Vec3 worldMouseNear, Vec3 worldMouseFar, Vec3 terrainIntersectionPoint, ref MapEntityVisual hoveredVisual, ref MapEntityVisual selectedVisual)
	{
		bool flag = false;
		for (int num = entityCount - 1; num >= 0; num--)
		{
			UIntPtr uIntPtr = intersectedEntityIDs[num];
			if (uIntPtr != UIntPtr.Zero)
			{
				if (MapScreen.VisualsOfEntities.TryGetValue(uIntPtr, out var value) && value is SettlementVisual && value.IsVisibleOrFadingOut())
				{
					if (hoveredVisual == null)
					{
						hoveredVisual = value;
					}
					selectedVisual = value;
				}
				if (PlayerSiege.PlayerSiegeEvent != null && (object)ScreenManager.FirstHitLayer == MapScreen.Instance.SceneLayer && MapScreen.FrameAndVisualOfEngines.ContainsKey(uIntPtr))
				{
					flag = true;
					HandleSiegeEngineHover(uIntPtr);
				}
			}
		}
		if (!flag)
		{
			HandleSiegeEngineHoverEnd();
		}
		return selectedVisual != null;
	}

	public override void OnFrameTick(float dt)
	{
		RefreshMapSiegeOverlayRequired();
		if (PlayerSiege.PlayerSiegeEvent != null && _playerSiegeMachineSlotMeshesAdded)
		{
			TickSiegeMachineCircles();
		}
		if (GameStateManager.Current.ActiveStateDisabledByUser)
		{
			HandleSiegeEngineHoverEnd();
		}
		_timeSinceCreation += dt;
	}

	public override bool OnMouseClick(MapEntityVisual visualOfSelectedEntity, Vec3 intersectionPoint, PathFaceRecord mouseOverFaceIndex, bool isDoubleClick)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (MapScreen.Instance.MapState.AtMenu && _hoveredSiegeEntityID != UIntPtr.Zero)
		{
			Tuple<MatrixFrame, SettlementVisual> tuple = MapScreen.FrameAndVisualOfEngines[_hoveredSiegeEntityID];
			MapScreen.Instance.OnSiegeEngineFrameClick(tuple.Item1);
			result = true;
		}
		return result;
	}

	public override MapEntityVisual<PartyBase> GetVisualOfEntity(PartyBase partyBase)
	{
		_settlementVisuals.TryGetValue(partyBase, out var value);
		return value;
	}

	public SettlementVisual GetSettlementVisual(Settlement settlement)
	{
		return _settlementVisuals[settlement.Party];
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		foreach (Settlement item in (List<Settlement>)(object)Settlement.All)
		{
			AddNewPartyVisualForParty(item.Party);
		}
		_ = Campaign.Current.MapSceneWrapper;
	}

	protected override void OnFinalize()
	{
		foreach (SettlementVisual value in _settlementVisuals.Values)
		{
			value.ReleaseResources();
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private void TickSiegeMachineCircles()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Invalid comparison between Unknown and I4
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Invalid comparison between Unknown and I4
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Invalid comparison between Unknown and I4
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Invalid comparison between Unknown and I4
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
		bool isPlayerLeader = playerSiegeEvent != null && playerSiegeEvent.IsPlayerSiegeEvent && Campaign.Current.Models.EncounterModel.GetLeaderOfSiegeEvent(playerSiegeEvent, PlayerSiege.PlayerSide) == Hero.MainHero;
		Settlement besiegedSettlement = playerSiegeEvent.BesiegedSettlement;
		SettlementVisual settlementVisual = GetSettlementVisual(besiegedSettlement);
		Tuple<MatrixFrame, SettlementVisual> tuple = null;
		if (_hoveredSiegeEntityID != UIntPtr.Zero)
		{
			tuple = MapScreen.FrameAndVisualOfEngines[_hoveredSiegeEntityID];
		}
		MatrixFrame globalFrame;
		for (int i = 0; i < settlementVisual.GetDefenderRangedSiegeEngineFrames().Length; i++)
		{
			bool isEmpty = playerSiegeEvent.GetSiegeEventSide((BattleSideEnum)0).SiegeEngines.DeployedRangedSiegeEngines[i] == null;
			bool isEnemy = (int)PlayerSiege.PlayerSide > 0;
			string desiredMaterialName = GetDesiredMaterialName(isRanged: true, isAttacker: false, isTower: false);
			GameEntityComponent componentAtIndex = _defenderMachinesCircleEntities[i].GetComponentAtIndex(0, (ComponentType)7);
			Decal val = (Decal)(object)((componentAtIndex is Decal) ? componentAtIndex : null);
			Material material = val.GetMaterial();
			if (((material != null) ? material.Name : null) != desiredMaterialName)
			{
				val.SetMaterial(Material.GetFromResource(desiredMaterialName));
			}
			int num;
			if (tuple != null)
			{
				globalFrame = _defenderMachinesCircleEntities[i].GetGlobalFrame();
				num = (((MatrixFrame)(ref globalFrame)).NearlyEquals(tuple.Item1, 1E-05f) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool isHovered = (byte)num != 0;
			uint desiredDecalColor = GetDesiredDecalColor(isHovered, isEnemy, isEmpty, isPlayerLeader);
			if (desiredDecalColor != val.GetFactor1())
			{
				val.SetFactor1(desiredDecalColor);
			}
		}
		for (int j = 0; j < settlementVisual.GetAttackerRangedSiegeEngineFrames().Length; j++)
		{
			bool isEmpty2 = playerSiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedRangedSiegeEngines[j] == null;
			bool isEnemy2 = (int)PlayerSiege.PlayerSide != 1;
			string desiredMaterialName2 = GetDesiredMaterialName(isRanged: true, isAttacker: true, isTower: false);
			GameEntityComponent componentAtIndex2 = _attackerRangedMachinesCircleEntities[j].GetComponentAtIndex(0, (ComponentType)7);
			Decal val2 = (Decal)(object)((componentAtIndex2 is Decal) ? componentAtIndex2 : null);
			Material material2 = val2.GetMaterial();
			if (((material2 != null) ? material2.Name : null) != desiredMaterialName2)
			{
				val2.SetMaterial(Material.GetFromResource(desiredMaterialName2));
			}
			int num2;
			if (tuple != null)
			{
				globalFrame = _attackerRangedMachinesCircleEntities[j].GetGlobalFrame();
				num2 = (((MatrixFrame)(ref globalFrame)).NearlyEquals(tuple.Item1, 1E-05f) ? 1 : 0);
			}
			else
			{
				num2 = 0;
			}
			bool isHovered2 = (byte)num2 != 0;
			uint desiredDecalColor2 = GetDesiredDecalColor(isHovered2, isEnemy2, isEmpty2, isPlayerLeader);
			if (desiredDecalColor2 != val2.GetFactor1())
			{
				val2.SetFactor1(desiredDecalColor2);
			}
		}
		for (int k = 0; k < settlementVisual.GetAttackerBatteringRamSiegeEngineFrames().Length; k++)
		{
			bool isEmpty3 = playerSiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedMeleeSiegeEngines[k] == null;
			bool isEnemy3 = (int)PlayerSiege.PlayerSide != 1;
			string desiredMaterialName3 = GetDesiredMaterialName(isRanged: false, isAttacker: true, isTower: false);
			GameEntityComponent componentAtIndex3 = _attackerRamMachinesCircleEntities[k].GetComponentAtIndex(0, (ComponentType)7);
			Decal val3 = (Decal)(object)((componentAtIndex3 is Decal) ? componentAtIndex3 : null);
			Material material3 = val3.GetMaterial();
			if (((material3 != null) ? material3.Name : null) != desiredMaterialName3)
			{
				val3.SetMaterial(Material.GetFromResource(desiredMaterialName3));
			}
			int num3;
			if (tuple != null)
			{
				globalFrame = _attackerRamMachinesCircleEntities[k].GetGlobalFrame();
				num3 = (((MatrixFrame)(ref globalFrame)).NearlyEquals(tuple.Item1, 1E-05f) ? 1 : 0);
			}
			else
			{
				num3 = 0;
			}
			bool isHovered3 = (byte)num3 != 0;
			uint desiredDecalColor3 = GetDesiredDecalColor(isHovered3, isEnemy3, isEmpty3, isPlayerLeader);
			if (desiredDecalColor3 != val3.GetFactor1())
			{
				val3.SetFactor1(desiredDecalColor3);
			}
		}
		for (int l = 0; l < settlementVisual.GetAttackerTowerSiegeEngineFrames().Length; l++)
		{
			bool isEmpty4 = playerSiegeEvent.GetSiegeEventSide((BattleSideEnum)1).SiegeEngines.DeployedMeleeSiegeEngines[settlementVisual.GetAttackerBatteringRamSiegeEngineFrames().Length + l] == null;
			bool isEnemy4 = (int)PlayerSiege.PlayerSide != 1;
			string desiredMaterialName4 = GetDesiredMaterialName(isRanged: false, isAttacker: true, isTower: true);
			GameEntityComponent componentAtIndex4 = _attackerTowerMachinesCircleEntities[l].GetComponentAtIndex(0, (ComponentType)7);
			Decal val4 = (Decal)(object)((componentAtIndex4 is Decal) ? componentAtIndex4 : null);
			Material material4 = val4.GetMaterial();
			if (((material4 != null) ? material4.Name : null) != desiredMaterialName4)
			{
				val4.SetMaterial(Material.GetFromResource(desiredMaterialName4));
			}
			int num4;
			if (tuple != null)
			{
				globalFrame = _attackerTowerMachinesCircleEntities[l].GetGlobalFrame();
				num4 = (((MatrixFrame)(ref globalFrame)).NearlyEquals(tuple.Item1, 1E-05f) ? 1 : 0);
			}
			else
			{
				num4 = 0;
			}
			bool isHovered4 = (byte)num4 != 0;
			uint desiredDecalColor4 = GetDesiredDecalColor(isHovered4, isEnemy4, isEmpty4, isPlayerLeader);
			if (desiredDecalColor4 != val4.GetFactor1())
			{
				val4.SetFactor1(desiredDecalColor4);
			}
		}
	}

	private void AddNewPartyVisualForParty(PartyBase partyBase)
	{
		SettlementVisual settlementVisual = new SettlementVisual(partyBase);
		settlementVisual.OnStartup();
		_settlementVisuals.Add(partyBase, settlementVisual);
		_visualsFlattened.Add(settlementVisual);
	}

	private uint GetDesiredDecalColor(bool isHovered, bool isEnemy, bool isEmpty, bool isPlayerLeader)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!isEnemy)
		{
			if (isHovered && isPlayerLeader)
			{
				return 4293956364u;
			}
			if (!isEmpty)
			{
				return 4283683126u;
			}
			if (isPlayerLeader)
			{
				float num = MathF.PingPong(0f, 0.5f, _timeSinceCreation) / 0.5f;
				Color val = Color.FromUint(4278394186u);
				Color val2 = Color.FromUint(4284320212u);
				Color val3 = Color.Lerp(val, val2, num);
				return ((Color)(ref val3)).ToUnsignedInteger();
			}
			return 4278394186u;
		}
		return 4287064638u;
	}

	private string GetDesiredMaterialName(bool isRanged, bool isAttacker, bool isTower)
	{
		if (isRanged)
		{
			if (!isAttacker)
			{
				return "decal_defender_ranged_siege";
			}
			return "decal_siege_ranged";
		}
		if (!isTower)
		{
			return "decal_siege_ram";
		}
		return "decal_siege_tower";
	}

	private void RemoveSiegeCircleVisuals()
	{
		if (_playerSiegeMachineSlotMeshesAdded)
		{
			MapScene mapScene = Campaign.Current.MapSceneWrapper as MapScene;
			for (int i = 0; i < _defenderMachinesCircleEntities.Length; i++)
			{
				_defenderMachinesCircleEntities[i].SetVisibilityExcludeParents(false);
				mapScene.Scene.RemoveEntity(_defenderMachinesCircleEntities[i], 107);
				_defenderMachinesCircleEntities[i] = null;
			}
			for (int j = 0; j < _attackerRamMachinesCircleEntities.Length; j++)
			{
				_attackerRamMachinesCircleEntities[j].SetVisibilityExcludeParents(false);
				mapScene.Scene.RemoveEntity(_attackerRamMachinesCircleEntities[j], 108);
				_attackerRamMachinesCircleEntities[j] = null;
			}
			for (int k = 0; k < _attackerTowerMachinesCircleEntities.Length; k++)
			{
				_attackerTowerMachinesCircleEntities[k].SetVisibilityExcludeParents(false);
				mapScene.Scene.RemoveEntity(_attackerTowerMachinesCircleEntities[k], 109);
				_attackerTowerMachinesCircleEntities[k] = null;
			}
			for (int l = 0; l < _attackerRangedMachinesCircleEntities.Length; l++)
			{
				_attackerRangedMachinesCircleEntities[l].SetVisibilityExcludeParents(false);
				mapScene.Scene.RemoveEntity(_attackerRangedMachinesCircleEntities[l], 110);
				_attackerRangedMachinesCircleEntities[l] = null;
			}
			_playerSiegeMachineSlotMeshesAdded = false;
		}
	}

	private void RefreshMapSiegeOverlayRequired()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		MapScreen.Instance.MapCameraView.OnRefreshMapSiegeOverlayRequired(_mapSiegeOverlayView == null);
		if (_playerSiegeMachineSlotMeshesAdded && PlayerSiege.PlayerSiegeEvent != null)
		{
			Settlement besiegedSettlement = PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
			if (besiegedSettlement != null && (int)besiegedSettlement.CurrentSiegeState == 1)
			{
				RemoveSiegeCircleVisuals();
				_playerSiegeMachineSlotMeshesAdded = false;
				return;
			}
		}
		if (PlayerSiege.PlayerSiegeEvent == null && _mapSiegeOverlayView != null)
		{
			MapScreen.Instance.RemoveMapView(_mapSiegeOverlayView);
			_mapSiegeOverlayView = null;
			if (_playerSiegeMachineSlotMeshesAdded)
			{
				RemoveSiegeCircleVisuals();
				_playerSiegeMachineSlotMeshesAdded = false;
			}
		}
		else if (PlayerSiege.PlayerSiegeEvent != null && _mapSiegeOverlayView == null)
		{
			_mapSiegeOverlayView = MapScreen.Instance.AddMapView<MapSiegeOverlayView>(Array.Empty<object>());
			if (!_playerSiegeMachineSlotMeshesAdded)
			{
				InitializeSiegeCircleVisuals();
				_playerSiegeMachineSlotMeshesAdded = true;
			}
		}
	}

	private void InitializeSiegeCircleVisuals()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		Settlement besiegedSettlement = PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
		SettlementVisual settlementVisual = GetSettlementVisual(besiegedSettlement);
		MapScene mapScene = Campaign.Current.MapSceneWrapper as MapScene;
		MatrixFrame[] defenderRangedSiegeEngineFrames = settlementVisual.GetDefenderRangedSiegeEngineFrames();
		_defenderMachinesCircleEntities = (GameEntity[])(object)new GameEntity[defenderRangedSiegeEngineFrames.Length];
		for (int i = 0; i < defenderRangedSiegeEngineFrames.Length; i++)
		{
			MatrixFrame val = defenderRangedSiegeEngineFrames[i];
			_defenderMachinesCircleEntities[i] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
			_defenderMachinesCircleEntities[i].Name = "dRangedMachineCircle_" + i;
			Decal val2 = Decal.CreateDecal((string)null);
			val2.SetMaterial(Material.GetFromResource("decal_defender_ranged_siege"));
			val2.SetFactor1Linear(4287064638u);
			_defenderMachinesCircleEntities[i].AddComponent((GameEntityComponent)(object)val2);
			MatrixFrame val3 = val;
			if (_isNewDecalScaleImplementationEnabled)
			{
				Vec3 val4 = new Vec3(0.25f, 0.25f, 0.25f, -1f);
				((MatrixFrame)(ref val3)).Scale(ref val4);
			}
			_defenderMachinesCircleEntities[i].SetGlobalFrame(ref val3, true);
			_defenderMachinesCircleEntities[i].SetVisibilityExcludeParents(true);
			mapScene.Scene.AddDecalInstance(val2, "editor_set", true);
		}
		defenderRangedSiegeEngineFrames = settlementVisual.GetAttackerBatteringRamSiegeEngineFrames();
		_attackerRamMachinesCircleEntities = (GameEntity[])(object)new GameEntity[defenderRangedSiegeEngineFrames.Length];
		for (int j = 0; j < defenderRangedSiegeEngineFrames.Length; j++)
		{
			MatrixFrame val5 = defenderRangedSiegeEngineFrames[j];
			_attackerRamMachinesCircleEntities[j] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
			_attackerRamMachinesCircleEntities[j].Name = "InitializeSiegeCircleVisuals";
			_attackerRamMachinesCircleEntities[j].Name = "aRamMachineCircle_" + j;
			Decal val6 = Decal.CreateDecal((string)null);
			val6.SetMaterial(Material.GetFromResource("decal_siege_ram"));
			val6.SetFactor1Linear(4287064638u);
			_attackerRamMachinesCircleEntities[j].AddComponent((GameEntityComponent)(object)val6);
			MatrixFrame val7 = val5;
			if (_isNewDecalScaleImplementationEnabled)
			{
				Vec3 val4 = new Vec3(0.38f, 0.38f, 0.38f, -1f);
				((MatrixFrame)(ref val7)).Scale(ref val4);
			}
			_attackerRamMachinesCircleEntities[j].SetGlobalFrame(ref val7, true);
			_attackerRamMachinesCircleEntities[j].SetVisibilityExcludeParents(true);
			mapScene.Scene.AddDecalInstance(val6, "editor_set", true);
		}
		defenderRangedSiegeEngineFrames = settlementVisual.GetAttackerTowerSiegeEngineFrames();
		_attackerTowerMachinesCircleEntities = (GameEntity[])(object)new GameEntity[defenderRangedSiegeEngineFrames.Length];
		for (int k = 0; k < defenderRangedSiegeEngineFrames.Length; k++)
		{
			MatrixFrame val8 = defenderRangedSiegeEngineFrames[k];
			_attackerTowerMachinesCircleEntities[k] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
			_attackerTowerMachinesCircleEntities[k].Name = "aTowerMachineCircle_" + k;
			Decal val9 = Decal.CreateDecal((string)null);
			val9.SetMaterial(Material.GetFromResource("decal_siege_tower"));
			val9.SetFactor1Linear(4287064638u);
			_attackerTowerMachinesCircleEntities[k].AddComponent((GameEntityComponent)(object)val9);
			MatrixFrame val10 = val8;
			if (_isNewDecalScaleImplementationEnabled)
			{
				Vec3 val4 = new Vec3(0.38f, 0.38f, 0.38f, -1f);
				((MatrixFrame)(ref val10)).Scale(ref val4);
			}
			_attackerTowerMachinesCircleEntities[k].SetGlobalFrame(ref val10, true);
			_attackerTowerMachinesCircleEntities[k].SetVisibilityExcludeParents(true);
			mapScene.Scene.AddDecalInstance(val9, "editor_set", true);
		}
		defenderRangedSiegeEngineFrames = settlementVisual.GetAttackerRangedSiegeEngineFrames();
		_attackerRangedMachinesCircleEntities = (GameEntity[])(object)new GameEntity[defenderRangedSiegeEngineFrames.Length];
		for (int l = 0; l < defenderRangedSiegeEngineFrames.Length; l++)
		{
			MatrixFrame val11 = defenderRangedSiegeEngineFrames[l];
			_attackerRangedMachinesCircleEntities[l] = GameEntity.CreateEmpty(mapScene.Scene, true, true, true);
			_attackerRangedMachinesCircleEntities[l].Name = "aRangedMachineCircle_" + l;
			Decal val12 = Decal.CreateDecal((string)null);
			val12.SetMaterial(Material.GetFromResource("decal_siege_ranged"));
			val12.SetFactor1Linear(4287064638u);
			_attackerRangedMachinesCircleEntities[l].AddComponent((GameEntityComponent)(object)val12);
			MatrixFrame val13 = val11;
			if (_isNewDecalScaleImplementationEnabled)
			{
				Vec3 val4 = new Vec3(0.38f, 0.38f, 0.38f, -1f);
				((MatrixFrame)(ref val13)).Scale(ref val4);
			}
			_attackerRangedMachinesCircleEntities[l].SetGlobalFrame(ref val13, true);
			_attackerRangedMachinesCircleEntities[l].SetVisibilityExcludeParents(true);
			mapScene.Scene.AddDecalInstance(val12, "editor_set", true);
		}
	}

	private void HandleSiegeEngineHover(UIntPtr newID)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (_hoveredSiegeEntityID != newID)
		{
			_hoveredSiegeEntityID = newID;
			Tuple<MatrixFrame, SettlementVisual> tuple = MapScreen.FrameAndVisualOfEngines[_hoveredSiegeEntityID];
			tuple.Item2.OnMapHoverSiegeEngine(tuple.Item1);
		}
	}

	private void HandleSiegeEngineHoverEnd()
	{
		if (_hoveredSiegeEntityID != UIntPtr.Zero)
		{
			MapScreen.FrameAndVisualOfEngines[_hoveredSiegeEntityID].Item2.OnMapHoverSiegeEngineEnd();
			_hoveredSiegeEntityID = UIntPtr.Zero;
		}
	}
}
