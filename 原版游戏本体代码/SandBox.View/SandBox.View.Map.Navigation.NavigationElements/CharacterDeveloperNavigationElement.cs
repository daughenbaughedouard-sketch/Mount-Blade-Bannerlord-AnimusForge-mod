using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Navigation.NavigationElements;

public class CharacterDeveloperNavigationElement : MapNavigationElementBase
{
	public override string StringId => "character_developer";

	public override bool IsActive => base._game.GameStateManager.ActiveState is CharacterDeveloperState;

	public override bool IsLockingNavigation => false;

	public override bool HasAlert => _viewDataTracker.IsCharacterNotificationActive;

	public CharacterDeveloperNavigationElement(MapNavigationHandler handler)
		: base(handler)
	{
	}

	protected override NavigationPermissionItem GetPermission()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!MapNavigationHelper.IsNavigationBarEnabled(_handler))
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (IsActive)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		Mission current = Mission.Current;
		if (current != null && !current.IsCharacterWindowAccessAllowed)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		return new NavigationPermissionItem(true, (TextObject)null);
	}

	protected override TextObject GetTooltip()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.IsGamepadActive)
		{
			NavigationPermissionItem permission = base.Permission;
			if (((NavigationPermissionItem)(ref permission)).IsAuthorized || IsActive)
			{
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 37)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_character", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_character", (string)null);
	}

	protected override TextObject GetAlertTooltip()
	{
		if (HasAlert)
		{
			return _viewDataTracker.GetCharacterNotificationText();
		}
		return TextObject.GetEmpty();
	}

	public override void OpenView()
	{
		PrepareToOpenCharacterDeveloper(delegate
		{
			OpenCharacterDeveloperScreenAction();
		});
	}

	public override void OpenView(params object[] parameters)
	{
		if (parameters.Length == 0)
		{
			return;
		}
		object obj = parameters[0];
		Hero hero;
		if ((hero = (Hero)((obj is Hero) ? obj : null)) != null)
		{
			PrepareToOpenCharacterDeveloper(delegate
			{
				OpenCharacterDeveloperScreenAction(hero);
			});
		}
		else
		{
			Debug.FailedAssert($"Invalid parameter type when opening the character developer screen from navigation: {obj.GetType()}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\CharacterDeveloperNavigationElement.cs", "OpenView", 90);
		}
	}

	public override void GoToLink()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.MainHero.EncyclopediaLink);
	}

	private void PrepareToOpenCharacterDeveloper(Action openCharacterDeveloperAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = base.Permission;
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
			{
				InformationManager.ShowInquiry(changeableScreen.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry(openCharacterDeveloperAction) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
			}
			else
			{
				MapNavigationHelper.SwitchToANewScreen(openCharacterDeveloperAction);
			}
		}
	}

	private void OpenCharacterDeveloperScreenAction()
	{
		CharacterDeveloperState val = base._game.GameStateManager.CreateState<CharacterDeveloperState>();
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenCharacterDeveloperScreenAction(Hero hero)
	{
		CharacterDeveloperState val = base._game.GameStateManager.CreateState<CharacterDeveloperState>(new object[1] { hero });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}
}
