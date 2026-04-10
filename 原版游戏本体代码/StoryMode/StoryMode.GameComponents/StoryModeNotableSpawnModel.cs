using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace StoryMode.GameComponents;

public class StoryModeNotableSpawnModel : NotableSpawnModel
{
	public override int GetTargetNotableCountForSettlement(Settlement settlement, Occupation occupation)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted && ((MBObjectBase)settlement).StringId == "village_ES3_2")
		{
			return 0;
		}
		return ((MBGameModel<NotableSpawnModel>)this).BaseModel.GetTargetNotableCountForSettlement(settlement, occupation);
	}
}
