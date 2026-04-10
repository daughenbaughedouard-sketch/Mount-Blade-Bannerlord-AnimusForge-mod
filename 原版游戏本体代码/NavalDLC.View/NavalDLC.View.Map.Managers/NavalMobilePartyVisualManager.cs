using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.View.Map.Visuals;
using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.View.Map.Managers;

public class NavalMobilePartyVisualManager : EntityVisualManagerBase<PartyBase>
{
	private const float DamageSoundCooldown = 2f;

	private static int _shipDamageSoundEventId = SoundManager.GetEventGlobalIndex("event:/ui/campaign/ship_damage");

	private readonly Dictionary<PartyBase, NavalMobilePartyVisual> _partiesAndVisuals = new Dictionary<PartyBase, NavalMobilePartyVisual>();

	private readonly List<NavalMobilePartyVisual> _visualsFlattened = new List<NavalMobilePartyVisual>();

	private int _dirtyPartyVisualCount;

	private NavalMobilePartyVisual[] _dirtyPartiesList = new NavalMobilePartyVisual[2500];

	private float _timeElapsedSinceLastShipDamageSoundPlayed;

	private float _mainPartyPreviousShipDamageTriggerHealthPercent = 1f;

	private readonly List<NavalMobilePartyVisual> _fadingPartiesFlatten = new List<NavalMobilePartyVisual>();

	private readonly HashSet<NavalMobilePartyVisual> _fadingPartiesSet = new HashSet<NavalMobilePartyVisual>();

	private readonly List<GameEntity> _bridgeEntityCache = new List<GameEntity>();

	public static NavalMobilePartyVisualManager Current => SandBoxViewSubModule.SandBoxViewVisualManager.GetEntityComponent<NavalMobilePartyVisualManager>();

	public override int Priority => 20;

	public override void OnTick(float realDt, float dt)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!((EntityVisualManagerBase)this).MapScene.HasTerrainHeightmap || !((EntityVisualManagerBase)this).MapScene.ContainsTerrain)
		{
			return;
		}
		_dirtyPartyVisualCount = -1;
		TWParallel.For(0, _visualsFlattened.Count, (ParallelForAuxPredicate)delegate(int startInclusive, int endExclusive)
		{
			for (int i = startInclusive; i < endExclusive; i++)
			{
				_visualsFlattened[i].Tick(dt, realDt, ref _dirtyPartyVisualCount, ref _dirtyPartiesList);
			}
		}, 16);
		for (int num = 0; num < _dirtyPartyVisualCount + 1; num++)
		{
			_dirtyPartiesList[num].ValidateIsDirty();
		}
		for (int num2 = _fadingPartiesFlatten.Count - 1; num2 >= 0; num2--)
		{
			_fadingPartiesFlatten[num2].TickFadingState(realDt);
		}
		if (dt > 0f && _timeElapsedSinceLastShipDamageSoundPlayed < 0f)
		{
			_timeElapsedSinceLastShipDamageSoundPlayed += realDt;
		}
		if (_timeElapsedSinceLastShipDamageSoundPlayed >= 0f && MobileParty.MainParty.IsCurrentlyAtSea && ((IEnumerable<Ship>)MobileParty.MainParty.Ships).Any())
		{
			TriggerShipDamageSound();
		}
	}

	public override void ClearVisualMemory()
	{
		foreach (NavalMobilePartyVisual item in _visualsFlattened)
		{
			item.ClearVisualMemory();
		}
	}

	public override MapEntityVisual<PartyBase> GetVisualOfEntity(PartyBase partyBase)
	{
		MobileParty mobileParty = partyBase.MobileParty;
		if (mobileParty != null && mobileParty.IsCurrentlyAtSea)
		{
			_partiesAndVisuals.TryGetValue(partyBase, out var value);
			return value;
		}
		return null;
	}

	public override bool OnVisualIntersected(Ray mouseRay, UIntPtr[] intersectedEntityIDs, Intersection[] intersectionInfos, int entityCount, Vec3 worldMouseNear, Vec3 worldMouseFar, Vec3 terrainIntersectionPoint, ref MapEntityVisual hoveredVisual, ref MapEntityVisual selectedVisual)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		for (int num = entityCount - 1; num >= 0; num--)
		{
			UIntPtr uIntPtr = intersectedEntityIDs[num];
			if (uIntPtr != UIntPtr.Zero && MapScreen.VisualsOfEntities.TryGetValue(uIntPtr, out var value) && value is NavalMobilePartyVisual navalMobilePartyVisual && value.IsVisibleOrFadingOut() && (!((MapEntityVisual<PartyBase>)navalMobilePartyVisual).MapEntity.IsMobile || ((MapEntityVisual<PartyBase>)navalMobilePartyVisual).MapEntity.MobileParty.IsMainParty || !((MapEntityVisual<PartyBase>)navalMobilePartyVisual).MapEntity.MobileParty.IsInRaftState))
			{
				Intersection val = intersectionInfos[num];
				Vec3 val2 = worldMouseNear - val.IntersectionPoint;
				_ = ((Vec3)(ref val2)).Length;
				if (value.AttachedTo == null)
				{
					hoveredVisual = value;
				}
				else
				{
					hoveredVisual = value.AttachedTo;
				}
				if (!value.IsMainEntity && (value.AttachedTo == null || !value.AttachedTo.IsMainEntity))
				{
					if (value.AttachedTo != null)
					{
						selectedVisual = value.AttachedTo;
					}
					else
					{
						selectedVisual = value;
					}
				}
			}
		}
		return selectedVisual != null;
	}

	protected override void OnInitialize()
	{
		((CampaignEntityVisualComponent)this).OnInitialize();
		foreach (MobileParty item in (List<MobileParty>)(object)MobileParty.All)
		{
			AddNewPartyVisualForParty(item, shouldTick: true);
		}
		CampaignEvents.MobilePartyDestroyed.AddNonSerializedListener((object)this, (Action<MobileParty, PartyBase>)OnMobilePartyDestroyed);
		CampaignEvents.MobilePartyCreated.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyCreated);
		CampaignEvents.OnMobilePartyNavigationStateChangedEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyNavigationStateChanged);
		CampaignEvents.OnMobilePartyJoinedToSiegeEventEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyJoinedToSiegeEvent);
		CampaignEvents.OnMobilePartyLeftSiegeEventEvent.AddNonSerializedListener((object)this, (Action<MobileParty>)OnMobilePartyLeftSiegeEvent);
		if (((IEnumerable<Ship>)MobileParty.MainParty.Ships).Any())
		{
			_mainPartyPreviousShipDamageTriggerHealthPercent = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).Average((Ship s) => s.HitPoints / s.MaxHitPoints);
		}
		_bridgeEntityCache.AddRange(((EntityVisualManagerBase)this).MapScene.FindEntitiesWithTag("bridge"));
	}

	protected override void OnFinalize()
	{
		foreach (NavalMobilePartyVisual value in _partiesAndVisuals.Values)
		{
			((MapEntityVisual)value).ReleaseResources();
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	public NavalMobilePartyVisual GetPartyVisual(PartyBase partyBase)
	{
		return _partiesAndVisuals[partyBase];
	}

	internal void RegisterFadingVisual(NavalMobilePartyVisual visual)
	{
		if (!_fadingPartiesSet.Contains(visual))
		{
			_fadingPartiesFlatten.Add(visual);
			_fadingPartiesSet.Add(visual);
		}
	}

	internal GameEntity GetNearbyBridgeToParty(PartyBase partyBase)
	{
		if (_partiesAndVisuals.TryGetValue(partyBase, out var visual))
		{
			return ((IEnumerable<GameEntity>)_bridgeEntityCache).FirstOrDefault((Func<GameEntity, bool>)delegate(GameEntity x)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				Vec3 globalPosition = x.GlobalPosition;
				return ((Vec3)(ref globalPosition)).Distance(visual.StrategicEntity.GlobalPosition) < 3f;
			});
		}
		return null;
	}

	private void OnMobilePartyNavigationStateChanged(MobileParty mobileParty)
	{
		if (mobileParty.IsCurrentlyAtSea && ((List<Ship>)(object)mobileParty.Ships).Count > 0)
		{
			if (mobileParty.IsMainParty)
			{
				SoundEvent.PlaySound2D("event:/ui/ship_disembark");
			}
		}
		else if (mobileParty.IsMainParty)
		{
			SoundEvent.PlaySound2D("event:/ui/ship_embark");
		}
	}

	private void TriggerShipDamageSound()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = ((IEnumerable<Ship>)MobileParty.MainParty.Ships).Average((Ship s) => s.HitPoints / s.MaxHitPoints);
		float num2 = _mainPartyPreviousShipDamageTriggerHealthPercent - num;
		if (num2 > 0.01f)
		{
			_mainPartyPreviousShipDamageTriggerHealthPercent = num;
			_timeElapsedSinceLastShipDamageSoundPlayed = -2f;
			SoundEventParameter val = default(SoundEventParameter);
			((SoundEventParameter)(ref val))._002Ector("Campaign Ship Damage", num2 * 10f);
			MBSoundEvent.PlaySound(_shipDamageSoundEventId, ref val, Vec3.Zero);
		}
	}

	private void OnMobilePartyLeftSiegeEvent(MobileParty mobileParty)
	{
		if (mobileParty.SiegeEvent == null || !mobileParty.SiegeEvent.BesiegedSettlement.HasPort || mobileParty.SiegeEvent.BlockadeShouldBeActivated || !((IEnumerable<Ship>)mobileParty.Ships).Any())
		{
			return;
		}
		mobileParty.SetNavalVisualAsDirty();
		foreach (PartyBase item in mobileParty.BesiegerCamp.GetInvolvedPartiesForEventType((BattleTypes)9))
		{
			item.MobileParty.SetNavalVisualAsDirty();
		}
	}

	private void OnMobilePartyJoinedToSiegeEvent(MobileParty mobileParty)
	{
		SiegeEvent siegeEvent = mobileParty.SiegeEvent;
		if (siegeEvent == null || !siegeEvent.IsBlockadeActive || !((IEnumerable<Ship>)mobileParty.Ships).Any())
		{
			return;
		}
		foreach (PartyBase item in mobileParty.BesiegerCamp.GetInvolvedPartiesForEventType((BattleTypes)9))
		{
			item.MobileParty.SetNavalVisualAsDirty();
		}
	}

	private void OnMobilePartyDestroyed(MobileParty mobileParty, PartyBase _)
	{
		RemovePartyVisualForParty(mobileParty);
	}

	private void OnMobilePartyCreated(MobileParty mobileParty)
	{
		AddNewPartyVisualForParty(mobileParty);
	}

	internal void UnRegisterFadingVisual(NavalMobilePartyVisual visual)
	{
		if (_fadingPartiesSet.Contains(visual))
		{
			int index = _fadingPartiesFlatten.IndexOf(visual);
			_fadingPartiesFlatten[index] = _fadingPartiesFlatten[_fadingPartiesFlatten.Count - 1];
			_fadingPartiesFlatten.Remove(_fadingPartiesFlatten[_fadingPartiesFlatten.Count - 1]);
			_fadingPartiesSet.Remove(visual);
		}
	}

	private void AddNewPartyVisualForParty(MobileParty mobileParty, bool shouldTick = false)
	{
		if (mobileParty.IsGarrison || mobileParty.IsMilitia || _partiesAndVisuals.ContainsKey(mobileParty.Party))
		{
			return;
		}
		NavalMobilePartyVisual navalMobilePartyVisual = new NavalMobilePartyVisual(mobileParty.Party);
		navalMobilePartyVisual.OnStartup();
		_partiesAndVisuals.Add(mobileParty.Party, navalMobilePartyVisual);
		_visualsFlattened.Add(navalMobilePartyVisual);
		if (shouldTick)
		{
			navalMobilePartyVisual.Tick(0.1f, 0.1f, ref _dirtyPartyVisualCount, ref _dirtyPartiesList);
			if (mobileParty.IsTransitionInProgress)
			{
				mobileParty.SetNavalVisualAsDirty();
				navalMobilePartyVisual.UpdateEntityPosition(0.1f, 0.1f);
			}
		}
	}

	private void RemovePartyVisualForParty(MobileParty mobileParty)
	{
		if (_partiesAndVisuals.TryGetValue(mobileParty.Party, out var value))
		{
			value.OnPartyRemoved();
			_visualsFlattened.Remove(value);
			_partiesAndVisuals.Remove(mobileParty.Party);
		}
	}
}
