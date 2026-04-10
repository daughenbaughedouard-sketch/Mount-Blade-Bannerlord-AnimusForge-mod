using System.Collections.ObjectModel;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed.Personal;

public class MPPersonalKillNotificationVM : ViewModel
{
	private MBBindingList<MPPersonalKillNotificationItemVM> _notificationList;

	[DataSourceProperty]
	public MBBindingList<MPPersonalKillNotificationItemVM> NotificationList
	{
		get
		{
			return _notificationList;
		}
		set
		{
			if (value != _notificationList)
			{
				_notificationList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPersonalKillNotificationItemVM>>(value, "NotificationList");
			}
		}
	}

	public MPPersonalKillNotificationVM()
	{
		NotificationList = new MBBindingList<MPPersonalKillNotificationItemVM>();
	}

	public void OnGoldChange(int changeAmount, GoldGainFlags goldGainType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Collection<MPPersonalKillNotificationItemVM>)(object)NotificationList).Add(new MPPersonalKillNotificationItemVM(changeAmount, goldGainType, RemoveItem));
	}

	public void OnPersonalHit(int damageAmount, bool isFatal, bool isMountDamage, bool isFriendlyFire, bool isHeadshot, string killedAgentName)
	{
		((Collection<MPPersonalKillNotificationItemVM>)(object)NotificationList).Add(new MPPersonalKillNotificationItemVM(damageAmount, isFatal, isMountDamage, isFriendlyFire, isHeadshot, killedAgentName, RemoveItem));
	}

	public void OnPersonalAssist(string killedAgentName)
	{
		((Collection<MPPersonalKillNotificationItemVM>)(object)NotificationList).Add(new MPPersonalKillNotificationItemVM(killedAgentName, RemoveItem));
	}

	private void RemoveItem(MPPersonalKillNotificationItemVM item)
	{
		((Collection<MPPersonalKillNotificationItemVM>)(object)NotificationList).Remove(item);
	}
}
