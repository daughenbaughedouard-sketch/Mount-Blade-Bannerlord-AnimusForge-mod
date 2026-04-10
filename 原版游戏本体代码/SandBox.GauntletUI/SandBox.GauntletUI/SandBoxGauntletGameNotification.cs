using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI;

public class SandBoxGauntletGameNotification : GauntletGameNotification
{
	private SoundEvent _currentNotificationSoundEvent;

	public static void Initialize()
	{
		GauntletGameNotification current = GauntletGameNotification.Current;
		if (current != null)
		{
			current.OnFinalize();
		}
		GauntletGameNotification.Current = (GauntletGameNotification)(object)new SandBoxGauntletGameNotification();
		ScreenManager.AddGlobalLayer((GlobalLayer)(object)GauntletGameNotification.Current, false);
		GauntletGameNotification.Current.RegisterEvents();
	}

	protected override void OnReceiveNewNotification(GameNotificationItemVM notification)
	{
		((GauntletGameNotification)this).OnReceiveNewNotification(notification);
		SoundEvent currentNotificationSoundEvent = _currentNotificationSoundEvent;
		if (currentNotificationSoundEvent != null)
		{
			currentNotificationSoundEvent.Release();
		}
		_currentNotificationSoundEvent = null;
		if (notification != null && notification.IsDialog)
		{
			_currentNotificationSoundEvent = SoundEvent.CreateEventFromExternalFile("event:/Extra/voiceover", notification.DialogSoundPath, (Scene)null, false, false);
			SoundEvent currentNotificationSoundEvent2 = _currentNotificationSoundEvent;
			if (currentNotificationSoundEvent2 != null)
			{
				currentNotificationSoundEvent2.Play();
			}
		}
	}

	public override void OnFinalize()
	{
		((GauntletGameNotification)this).OnFinalize();
		SoundEvent currentNotificationSoundEvent = _currentNotificationSoundEvent;
		if (currentNotificationSoundEvent != null)
		{
			currentNotificationSoundEvent.Release();
		}
		_currentNotificationSoundEvent = null;
	}

	public override void RegisterEvents()
	{
		((GauntletGameNotification)this).RegisterEvents();
		CampaignInformationManager.OnDisplayDialog += base._dataSource.AddDialogNotification;
		CampaignInformationManager.OnGetStatusOfDialogNotification += base._dataSource.GetStatusOfDialogNotification;
		CampaignInformationManager.OnClearDialogNotification += base._dataSource.ClearDialogNotification;
		CampaignInformationManager.IsAnyDialogNotificationActiveOrQueued += base._dataSource.GetIsAnyDialogNotificationActiveOrQueued;
		CampaignInformationManager.OnClearAllDialogNotifications += base._dataSource.ClearAllDialogNotifications;
	}

	public override void UnregisterEvents()
	{
		((GauntletGameNotification)this).UnregisterEvents();
		CampaignInformationManager.OnDisplayDialog -= base._dataSource.AddDialogNotification;
		CampaignInformationManager.OnGetStatusOfDialogNotification -= base._dataSource.GetStatusOfDialogNotification;
		CampaignInformationManager.OnClearDialogNotification -= base._dataSource.ClearDialogNotification;
		CampaignInformationManager.IsAnyDialogNotificationActiveOrQueued -= base._dataSource.GetIsAnyDialogNotificationActiveOrQueued;
		CampaignInformationManager.OnClearAllDialogNotifications -= base._dataSource.ClearAllDialogNotifications;
	}

	protected override void OnTick(float dt)
	{
		((GauntletGameNotification)this).OnTick(dt);
		TickSoundEvent();
	}

	private void TickSoundEvent()
	{
		if (_currentNotificationSoundEvent == null)
		{
			return;
		}
		if (base._dataSource.GotNotification && base._dataSource.CurrentNotification.IsDialog)
		{
			if (!_currentNotificationSoundEvent.IsValid || _currentNotificationSoundEvent.IsStopped())
			{
				_currentNotificationSoundEvent.Release();
				_currentNotificationSoundEvent = null;
				base._dataSource.FadeOutCurrentNotification(true);
			}
			else if (base._dataSource.IsPaused && _currentNotificationSoundEvent.IsPlaying())
			{
				_currentNotificationSoundEvent.Pause();
			}
			else if (!base._dataSource.IsPaused && _currentNotificationSoundEvent.IsPaused())
			{
				_currentNotificationSoundEvent.Resume();
			}
		}
		else
		{
			_currentNotificationSoundEvent.Release();
			_currentNotificationSoundEvent = null;
		}
	}

	protected override bool GetShouldBeSuspended()
	{
		bool flag = ((GauntletGameNotification)this).GetShouldBeSuspended();
		if (base._dataSource.GotNotification && base._dataSource.CurrentNotification.IsDialog)
		{
			int num;
			if (!flag && !MBCommon.IsPaused)
			{
				GameStateManager current = GameStateManager.Current;
				num = ((current != null && current.ActiveStateDisabledByUser) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			flag = (byte)num != 0;
		}
		return flag;
	}
}
