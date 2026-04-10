using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MapNotificationTypes;
using TaleWorlds.Core;

namespace StoryMode.ViewModelCollection.Map;

public class ConspiracyQuestMapNotificationItemVM : MapNotificationItemBaseVM
{
	public QuestBase Quest { get; }

	public ConspiracyQuestMapNotificationItemVM(ConspiracyQuestMapNotification data)
		: base((InformationData)(object)data)
	{
		ConspiracyQuestMapNotificationItemVM conspiracyQuestMapNotificationItemVM = this;
		((MapNotificationItemBaseVM)this).NotificationIdentifier = "conspiracyquest";
		Quest = data.ConspiracyQuest;
		base._onInspect = delegate
		{
			INavigationHandler navigationHandler = ((MapNotificationItemBaseVM)conspiracyQuestMapNotificationItemVM).NavigationHandler;
			if (navigationHandler != null)
			{
				MapNavigationExtensions.OpenQuests(navigationHandler, data.ConspiracyQuest);
			}
		};
	}
}
