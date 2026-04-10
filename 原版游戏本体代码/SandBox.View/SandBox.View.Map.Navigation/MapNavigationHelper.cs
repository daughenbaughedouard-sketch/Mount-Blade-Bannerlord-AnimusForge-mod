using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Navigation;

public static class MapNavigationHelper
{
	public static InquiryData GetUnsavedChangedInquiry(Action openNewScreenAction)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		return new InquiryData(string.Empty, ((object)GameTexts.FindText("str_unsaved_changes", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
		{
			ApplyCurrentChanges();
			SwitchToANewScreen(openNewScreenAction);
		}, (Action)delegate
		{
			SwitchToANewScreen(openNewScreenAction);
		}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
	}

	public static InquiryData GetUnapplicableChangedInquiry()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		return new InquiryData(string.Empty, ((object)GameTexts.FindText("str_unapplicable_changes", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
	}

	public static bool IsMapTopScreen()
	{
		return ScreenManager.TopScreen is MapScreen;
	}

	public static bool IsNavigationBarEnabled(MapNavigationHandler handler)
	{
		if (Hero.MainHero != null)
		{
			Hero mainHero = Hero.MainHero;
			if (mainHero == null || !mainHero.IsDead)
			{
				Campaign current = Campaign.Current;
				if (current == null || !current.SaveHandler.IsSaving)
				{
					if (handler != null && handler.IsNavigationLocked)
					{
						return false;
					}
					if (PlayerEncounter.CurrentBattleSimulation != null)
					{
						return false;
					}
					if (ScreenManager.TopScreen is MapScreen mapScreen && (mapScreen.IsInArmyManagement || mapScreen.IsMarriageOfferPopupActive || mapScreen.IsHeirSelectionPopupActive || mapScreen.IsMapCheatsActive || mapScreen.IsMapIncidentActive || mapScreen.EncyclopediaScreenManager.IsEncyclopediaOpen))
					{
						return false;
					}
					if (handler != null && handler.IsEscapeMenuActive)
					{
						return false;
					}
					INavigationElement[] elements = handler.GetElements();
					for (int i = 0; i < elements.Length; i++)
					{
						if (elements[i].IsLockingNavigation)
						{
							return false;
						}
					}
					return true;
				}
			}
		}
		return false;
	}

	private static void ApplyCurrentChanges()
	{
		if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
		{
			if (changeableScreen.CanChangesBeApplied())
			{
				changeableScreen.ApplyChanges();
			}
			else
			{
				changeableScreen.ResetChanges();
			}
		}
	}

	public static void SwitchToANewScreen(Action openNewScreenAction)
	{
		if (!IsMapTopScreen())
		{
			Game.Current.GameStateManager.PopState(0);
		}
		openNewScreenAction?.Invoke();
	}
}
