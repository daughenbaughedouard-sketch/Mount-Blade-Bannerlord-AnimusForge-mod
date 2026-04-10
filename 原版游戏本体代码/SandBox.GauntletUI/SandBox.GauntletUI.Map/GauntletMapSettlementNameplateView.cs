using System;
using System.Collections.Generic;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.ViewModelCollection.Nameplate;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapSettlementNameplateView))]
public class GauntletMapSettlementNameplateView : MapView, IGauntletMapEventVisualHandler
{
	private GauntletLayer _layerAsGauntletLayer;

	private GauntletMovieIdentifier _movie;

	private SettlementNameplatesVM _dataSource;

	protected override void CreateLayout()
	{
		base.CreateLayout();
		_dataSource = new SettlementNameplatesVM(base.MapScreen.MapCameraView.Camera, base.MapScreen.FastMoveCameraToPosition);
		GauntletMapBasicView mapView = base.MapScreen.GetMapView<GauntletMapBasicView>();
		base.Layer = (ScreenLayer)(object)mapView.GauntletNameplateLayer;
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		_movie = _layerAsGauntletLayer.LoadMovie("SettlementNameplate", (ViewModel)(object)_dataSource);
		List<Tuple<Settlement, GameEntity>> list = new List<Tuple<Settlement, GameEntity>>();
		foreach (Settlement item2 in (List<Settlement>)(object)Settlement.All)
		{
			GameEntity strategicEntity = SettlementVisualManager.Current.GetSettlementVisual(item2).StrategicEntity;
			Tuple<Settlement, GameEntity> item = new Tuple<Settlement, GameEntity>(item2, strategicEntity);
			list.Add(item);
		}
		CampaignEvents.OnHideoutSpottedEvent.AddNonSerializedListener((object)this, (Action<PartyBase, PartyBase>)OnHideoutSpotted);
		_dataSource.Initialize(list);
		if (!(Campaign.Current.VisualCreator.MapEventVisualCreator is GauntletMapEventVisualCreator gauntletMapEventVisualCreator))
		{
			return;
		}
		gauntletMapEventVisualCreator.Handlers.Add(this);
		foreach (GauntletMapEventVisual currentEvent in gauntletMapEventVisualCreator.GetCurrentEvents())
		{
			GetNameplateOfMapEvent(currentEvent)?.OnMapEventStartedOnSettlement(currentEvent.MapEvent);
		}
	}

	protected override void OnResume()
	{
		base.OnResume();
		_dataSource.RefreshDynamicPropertiesOfNameplates(forceUpdate: true);
	}

	protected override void OnMapScreenUpdate(float dt)
	{
		base.OnMapScreenUpdate(dt);
		_dataSource.Update();
		bool flag = ((ScreenLayer)base.MapScreen.SceneLayer).Input.IsGameKeyDown(5);
		TextObject val = default(TextObject);
		for (int i = 0; i < ((List<SettlementNameplateVM>)(object)_dataSource.AllNameplates).Count; i++)
		{
			SettlementNameplateVM settlementNameplateVM = ((List<SettlementNameplateVM>)(object)_dataSource.AllNameplates)[i];
			if (settlementNameplateVM.IsInside && settlementNameplateVM.IsVisibleOnMap)
			{
				settlementNameplateVM.CanParley = flag && Campaign.Current.Models.EncounterModel.CanMainHeroDoParleyWithParty(settlementNameplateVM.Settlement.Party, ref val);
			}
		}
	}

	protected override void OnFinalize()
	{
		if (Campaign.Current.VisualCreator.MapEventVisualCreator is GauntletMapEventVisualCreator gauntletMapEventVisualCreator)
		{
			gauntletMapEventVisualCreator.Handlers.Remove(this);
		}
		((IMbEventBase)CampaignEvents.OnHideoutSpottedEvent).ClearListeners((object)this);
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		((ViewModel)_dataSource).OnFinalize();
		_layerAsGauntletLayer = null;
		base.Layer = null;
		_movie = null;
		_dataSource = null;
		base.OnFinalize();
	}

	protected override void OnMapConversationStart()
	{
		base.OnMapConversationStart();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, true);
		}
	}

	protected override void OnMapConversationOver()
	{
		base.OnMapConversationOver();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, false);
		}
	}

	private void OnHideoutSpotted(PartyBase party, PartyBase hideoutParty)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/ui/notification/hideout_found"), hideoutParty.Settlement.GetPosition());
	}

	private SettlementNameplateVM GetNameplateOfMapEvent(GauntletMapEventVisual mapEvent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Invalid comparison between Unknown and I4
		int num;
		if ((int)mapEvent.MapEvent.EventType == 2)
		{
			Settlement mapEventSettlement = mapEvent.MapEvent.MapEventSettlement;
			num = (((mapEventSettlement != null && mapEventSettlement.IsUnderRaid) || (mapEvent != null && mapEvent.MapEvent.IsFinalized)) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		int num2;
		if ((int)mapEvent.MapEvent.EventType == 5)
		{
			Settlement mapEventSettlement2 = mapEvent.MapEvent.MapEventSettlement;
			num2 = (((mapEventSettlement2 != null && mapEventSettlement2.IsUnderSiege) || (mapEvent != null && mapEvent.MapEvent.IsFinalized)) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag2 = (byte)num2 != 0;
		int num3;
		if ((int)mapEvent.MapEvent.EventType == 7 || (int)mapEvent.MapEvent.EventType == 10)
		{
			Settlement mapEventSettlement3 = mapEvent.MapEvent.MapEventSettlement;
			num3 = (((mapEventSettlement3 != null && mapEventSettlement3.IsUnderSiege) || (mapEvent != null && mapEvent.MapEvent.IsFinalized)) ? 1 : 0);
		}
		else
		{
			num3 = 0;
		}
		bool flag3 = (byte)num3 != 0;
		if (mapEvent.MapEvent.MapEventSettlement != null && (flag2 || flag || flag3))
		{
			return _dataSource.GetNameplateOfSettlement(mapEvent.MapEvent.MapEventSettlement);
		}
		return null;
	}

	void IGauntletMapEventVisualHandler.OnNewEventStarted(GauntletMapEventVisual newEvent)
	{
		GetNameplateOfMapEvent(newEvent)?.OnMapEventStartedOnSettlement(newEvent.MapEvent);
	}

	void IGauntletMapEventVisualHandler.OnInitialized(GauntletMapEventVisual newEvent)
	{
		GetNameplateOfMapEvent(newEvent)?.OnMapEventStartedOnSettlement(newEvent.MapEvent);
	}

	void IGauntletMapEventVisualHandler.OnEventEnded(GauntletMapEventVisual newEvent)
	{
		GetNameplateOfMapEvent(newEvent)?.OnMapEventEndedOnSettlement();
	}

	void IGauntletMapEventVisualHandler.OnEventVisibilityChanged(GauntletMapEventVisual visibilityChangedEvent)
	{
	}
}
