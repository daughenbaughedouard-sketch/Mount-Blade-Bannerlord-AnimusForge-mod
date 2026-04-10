using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapNotificationView))]
public class GauntletMapNotificationView : MapNotificationView
{
	private MapNotificationVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private INavigationHandler _mapNavigationHandler;

	private GauntletLayer _layerAsGauntletLayer;

	private bool _isHoveringOnNotification;

	private const string _defaultSound = "event:/ui/default";

	protected override void CreateLayout()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		base.CreateLayout();
		_mapNavigationHandler = base.MapScreen.NavigationHandler;
		_dataSource = new MapNotificationVM(_mapNavigationHandler, (Action<CampaignVec2>)base.MapScreen.FastMoveCameraToPosition);
		_dataSource.ReceiveNewNotification += OnReceiveNewNotification;
		_dataSource.SetRemoveInputKey(HotKeyManager.GetCategory("MapNotificationHotKeyCategory").GetHotKey("RemoveNotification"));
		base.Layer = (ScreenLayer)new GauntletLayer("MapNotification", 100, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("MapNotificationHotKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		_movie = _layerAsGauntletLayer.LoadMovie("MapNotificationUI", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
	}

	private void OnReceiveNewNotification(MapNotificationItemBaseVM newNotification)
	{
		if (!string.IsNullOrEmpty(newNotification.SoundId))
		{
			SoundEvent.PlaySound2D(newNotification.SoundId);
		}
	}

	public override void RegisterMapNotificationType(Type data, Type item)
	{
		_dataSource.RegisterMapNotificationType(data, item);
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		((ViewModel)_dataSource).OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		_dataSource.OnFrameTick(dt);
		HandleInput();
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		_dataSource.OnMenuModeTick(dt);
		HandleInput();
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

	private void HandleInput()
	{
		if (!_isHoveringOnNotification && _dataSource.FocusedNotificationItem != null)
		{
			_isHoveringOnNotification = true;
			base.Layer.IsFocusLayer = true;
			ScreenManager.TrySetFocus(base.Layer);
		}
		else if (_isHoveringOnNotification && _dataSource.FocusedNotificationItem == null)
		{
			_isHoveringOnNotification = false;
			base.Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(base.Layer);
		}
		if (_isHoveringOnNotification && _dataSource.FocusedNotificationItem != null && base.Layer.Input.IsHotKeyReleased("RemoveNotification") && !_dataSource.FocusedNotificationItem.ForceInspection)
		{
			SoundEvent.PlaySound2D("event:/ui/default");
			_dataSource.FocusedNotificationItem.ExecuteRemove();
		}
	}

	public override void ResetNotifications()
	{
		base.ResetNotifications();
		MapNotificationVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.RemoveAllNotifications();
		}
	}
}
