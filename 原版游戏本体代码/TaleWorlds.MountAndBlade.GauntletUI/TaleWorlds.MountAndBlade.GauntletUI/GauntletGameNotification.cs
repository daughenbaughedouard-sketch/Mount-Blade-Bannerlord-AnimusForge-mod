using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.GauntletUI.SceneNotification;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletGameNotification : GlobalLayer
{
	protected GameNotificationVM _dataSource;

	private readonly GauntletLayer _layer;

	private bool _isSuspended;

	protected static GauntletGameNotification Current { get; set; }

	protected virtual string MovieName => "GameNotificationUI";

	protected GauntletGameNotification()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		_dataSource = new GameNotificationVM();
		_dataSource.CurrentNotificationChanged += OnReceiveNewNotification;
		_layer = new GauntletLayer("GameNotification", 19007, false);
		_layer.LoadMovie(MovieName, (ViewModel)(object)_dataSource);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_layer;
		((ScreenLayer)_layer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)3);
	}

	protected virtual void OnReceiveNewNotification(GameNotificationItemVM notification)
	{
		if (!string.IsNullOrEmpty((notification != null) ? notification.NotificationSoundId : null))
		{
			SoundEvent.PlaySound2D(notification.NotificationSoundId);
		}
	}

	public static void Initialize()
	{
		Current?.OnFinalize();
		Current = new GauntletGameNotification();
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
		Current.RegisterEvents();
	}

	public virtual void OnFinalize()
	{
		GameNotificationVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.ClearNotifications();
		}
		UnregisterEvents();
		ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)this);
		_dataSource = null;
	}

	public virtual void RegisterEvents()
	{
		MBInformationManager.FiringQuickInformation += _dataSource.AddGameNotification;
	}

	public virtual void UnregisterEvents()
	{
		MBInformationManager.FiringQuickInformation -= _dataSource.AddGameNotification;
	}

	protected override void OnTick(float dt)
	{
		((GlobalLayer)this).OnTick(dt);
		bool shouldBeSuspended = GetShouldBeSuspended();
		if (shouldBeSuspended != _isSuspended)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)Current._layer, shouldBeSuspended);
			_isSuspended = shouldBeSuspended;
		}
		_dataSource.IsPaused = _isSuspended;
	}

	protected virtual bool GetShouldBeSuspended()
	{
		if (!GauntletSceneNotification.Current.IsActive)
		{
			return LoadingWindow.IsLoadingWindowActive;
		}
		return true;
	}
}
