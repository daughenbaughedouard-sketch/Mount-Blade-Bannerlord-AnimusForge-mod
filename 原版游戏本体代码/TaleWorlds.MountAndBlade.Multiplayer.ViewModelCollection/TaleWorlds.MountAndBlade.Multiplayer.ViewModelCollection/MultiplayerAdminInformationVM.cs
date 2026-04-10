using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MultiplayerAdminInformationVM : ViewModel
{
	private MBBindingList<StringItemWithActionVM> _messageQueue;

	[DataSourceProperty]
	public MBBindingList<StringItemWithActionVM> MessageQueue
	{
		get
		{
			return _messageQueue;
		}
		set
		{
			if (value != _messageQueue)
			{
				_messageQueue = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringItemWithActionVM>>(value, "MessageQueue");
			}
		}
	}

	public MultiplayerAdminInformationVM()
	{
		MessageQueue = new MBBindingList<StringItemWithActionVM>();
	}

	public void OnNewMessageReceived(string message)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		StringItemWithActionVM item = new StringItemWithActionVM((Action<object>)ExecuteRemoveMessage, message, (object)message);
		((Collection<StringItemWithActionVM>)(object)MessageQueue).Add(item);
	}

	private void ExecuteRemoveMessage(object messageToRemove)
	{
		int index = Extensions.FindIndex<StringItemWithActionVM>((IReadOnlyList<StringItemWithActionVM>)MessageQueue, (Func<StringItemWithActionVM, bool>)((StringItemWithActionVM m) => m.ActionText == messageToRemove as string));
		((Collection<StringItemWithActionVM>)(object)MessageQueue).RemoveAt(index);
	}
}
