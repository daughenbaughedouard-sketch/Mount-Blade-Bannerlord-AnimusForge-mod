using System.Collections.ObjectModel;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed.General;

public class MPGeneralKillNotificationVM : ViewModel
{
	private MBBindingList<MPGeneralKillNotificationItemVM> _notificationList;

	[DataSourceProperty]
	public MBBindingList<MPGeneralKillNotificationItemVM> NotificationList
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
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPGeneralKillNotificationItemVM>>(value, "NotificationList");
			}
		}
	}

	public MPGeneralKillNotificationVM()
	{
		NotificationList = new MBBindingList<MPGeneralKillNotificationItemVM>();
	}

	public void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, Agent assistedAgent)
	{
		((Collection<MPGeneralKillNotificationItemVM>)(object)NotificationList).Add(new MPGeneralKillNotificationItemVM(affectedAgent, affectorAgent, assistedAgent, RemoveItem));
	}

	private void RemoveItem(MPGeneralKillNotificationItemVM item)
	{
		((Collection<MPGeneralKillNotificationItemVM>)(object)NotificationList).Remove(item);
	}
}
