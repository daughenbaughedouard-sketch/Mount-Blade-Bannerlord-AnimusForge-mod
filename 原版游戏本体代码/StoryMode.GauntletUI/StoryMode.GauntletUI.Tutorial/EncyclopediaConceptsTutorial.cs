using SandBox.GauntletUI.Tutorial;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("EncyclopediaConceptsTutorial")]
public class EncyclopediaConceptsTutorial : EncyclopediaPageTutorialBase
{
	public EncyclopediaConceptsTutorial()
		: base((EncyclopediaPages)15, (EncyclopediaPages)8)
	{
	}
}
