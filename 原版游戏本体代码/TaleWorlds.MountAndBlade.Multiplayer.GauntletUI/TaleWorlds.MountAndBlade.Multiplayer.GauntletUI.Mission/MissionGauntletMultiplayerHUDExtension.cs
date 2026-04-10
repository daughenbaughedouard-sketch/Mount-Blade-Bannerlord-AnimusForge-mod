using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerHUDExtensionUIHandler))]
public class MissionGauntletMultiplayerHUDExtension : MissionView
{
	private MissionMultiplayerHUDExtensionVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _mpMissionCategory;

	private MissionLobbyComponent _lobbyComponent;

	public MissionGauntletMultiplayerHUDExtension()
	{
		base.ViewOrderPriority = 2;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_mpMissionCategory = UIResourceManager.LoadSpriteCategory("ui_mpmission");
		_dataSource = new MissionMultiplayerHUDExtensionVM(((MissionBehavior)this).Mission);
		_gauntletLayer = new GauntletLayer("HUDExtension", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("HUDExtension", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((MissionView)this).MissionScreen.OnSpectateAgentFocusIn += new OnSpectateAgentDelegate(_dataSource.OnSpectatedAgentFocusIn);
		((MissionView)this).MissionScreen.OnSpectateAgentFocusOut += new OnSpectateAgentDelegate(_dataSource.OnSpectatedAgentFocusOut);
		Game.Current.EventManager.RegisterEvent<MissionPlayerToggledOrderViewEvent>((Action<MissionPlayerToggledOrderViewEvent>)OnMissionPlayerToggledOrderViewEvent);
		_lobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_lobbyComponent.OnPostMatchEnded += OnPostMatchEnded;
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		_lobbyComponent.OnPostMatchEnded -= OnPostMatchEnded;
		((MissionView)this).MissionScreen.OnSpectateAgentFocusIn -= new OnSpectateAgentDelegate(_dataSource.OnSpectatedAgentFocusIn);
		((MissionView)this).MissionScreen.OnSpectateAgentFocusOut -= new OnSpectateAgentDelegate(_dataSource.OnSpectatedAgentFocusOut);
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		SpriteCategory mpMissionCategory = _mpMissionCategory;
		if (mpMissionCategory != null)
		{
			mpMissionCategory.Unload();
		}
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
		Game.Current.EventManager.UnregisterEvent<MissionPlayerToggledOrderViewEvent>((Action<MissionPlayerToggledOrderViewEvent>)OnMissionPlayerToggledOrderViewEvent);
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		_dataSource.Tick(dt);
	}

	private void OnMissionPlayerToggledOrderViewEvent(MissionPlayerToggledOrderViewEvent eventObj)
	{
		_dataSource.IsOrderActive = eventObj.IsOrderEnabled;
	}

	private void OnPostMatchEnded()
	{
		_dataSource.ShowHud = false;
	}
}
