using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeCutsceneSelectionModel : CutsceneSelectionModel
{
	public override SceneNotificationData GetKingdomDestroyedSceneNotification(Kingdom kingdom)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (StoryModeManager.Current.MainStoryLine.PlayerSupportedKingdom == kingdom)
		{
			return (SceneNotificationData)new SupportedFactionDefeatedSceneNotificationItem(kingdom, StoryModeManager.Current.MainStoryLine.IsOnImperialQuestLine);
		}
		return ((MBGameModel<CutsceneSelectionModel>)this).BaseModel.GetKingdomDestroyedSceneNotification(kingdom);
	}
}
