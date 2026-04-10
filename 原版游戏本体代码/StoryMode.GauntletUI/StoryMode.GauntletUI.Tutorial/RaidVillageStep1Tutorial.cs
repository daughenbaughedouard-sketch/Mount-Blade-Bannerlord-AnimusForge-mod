using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("RaidVillageStep1")]
public class RaidVillageStep1Tutorial : TutorialItemBase
{
	private bool _gameMenuChanged;

	private bool _villageRaidMenuOpened;

	public RaidVillageStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)2;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		return _gameMenuChanged;
	}

	public override void OnGameMenuOpened(MenuCallbackArgs obj)
	{
		if (_villageRaidMenuOpened && obj.MenuContext.GameMenu.StringId != TutorialHelper.ActiveVillageRaidGameMenuID)
		{
			_gameMenuChanged = true;
		}
	}

	public override void OnGameMenuOptionSelected(GameMenuOption obj)
	{
		((TutorialItemBase)this).OnGameMenuOptionSelected(obj);
		if (!_villageRaidMenuOpened)
		{
			return;
		}
		Campaign current = Campaign.Current;
		object obj2;
		if (current == null)
		{
			obj2 = null;
		}
		else
		{
			MenuContext currentMenuContext = current.CurrentMenuContext;
			if (currentMenuContext == null)
			{
				obj2 = null;
			}
			else
			{
				GameMenu gameMenu = currentMenuContext.GameMenu;
				obj2 = ((gameMenu != null) ? gameMenu.StringId : null);
			}
		}
		if ((string?)obj2 == TutorialHelper.ActiveVillageRaidGameMenuID)
		{
			_gameMenuChanged = true;
		}
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		_villageRaidMenuOpened = TutorialHelper.IsActiveVillageRaidGameMenuOpen;
		return _villageRaidMenuOpened;
	}
}
