using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.MapSiege;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("BombardmentStep1")]
public class BombardmentStep1Tutorial : TutorialItemBase
{
	private bool _playerSelectedSiegeEngine;

	private bool _isGameMenuChangedAfterActivation;

	private bool _isActivated;

	public BombardmentStep1Tutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (!_playerSelectedSiegeEngine)
		{
			return _isGameMenuChangedAfterActivation;
		}
		return true;
	}

	public override void OnPlayerStartEngineConstruction(PlayerStartEngineConstructionEvent obj)
	{
		_playerSelectedSiegeEngine = true;
	}

	public override void OnGameMenuOptionSelected(GameMenuOption obj)
	{
		((TutorialItemBase)this).OnGameMenuOptionSelected(obj);
		if (_isActivated)
		{
			_isGameMenuChangedAfterActivation = true;
		}
	}

	public override void OnGameMenuOpened(MenuCallbackArgs obj)
	{
		((TutorialItemBase)this).OnGameMenuOpened(obj);
		if (_isActivated)
		{
			_isGameMenuChangedAfterActivation = true;
		}
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Invalid comparison between Unknown and I4
		MenuContext currentMenuContext = Campaign.Current.CurrentMenuContext;
		int isActivated;
		if (((currentMenuContext != null) ? currentMenuContext.GameMenu.StringId : null) == "menu_siege_strategies")
		{
			SiegeEvent playerSiegeEvent = PlayerSiege.PlayerSiegeEvent;
			if (playerSiegeEvent != null)
			{
				SiegeEnginesContainer siegeEngines = playerSiegeEvent.GetSiegeEventSide(PlayerSiege.PlayerSide).SiegeEngines;
				bool? obj;
				if (siegeEngines == null)
				{
					obj = null;
				}
				else
				{
					SiegeEngineConstructionProgress siegePreparations = siegeEngines.SiegePreparations;
					obj = ((siegePreparations != null) ? new bool?(siegePreparations.IsActive) : ((bool?)null));
				}
				if (obj == true)
				{
					isActivated = (((int)TutorialHelper.CurrentContext == 4) ? 1 : 0);
					goto IL_0092;
				}
			}
		}
		isActivated = 0;
		goto IL_0092;
		IL_0092:
		_isActivated = (byte)isActivated != 0;
		return _isActivated;
	}
}
