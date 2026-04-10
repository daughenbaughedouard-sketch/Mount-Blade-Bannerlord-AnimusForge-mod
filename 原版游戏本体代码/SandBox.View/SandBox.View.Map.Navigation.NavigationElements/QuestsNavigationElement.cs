using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Navigation.NavigationElements;

public class QuestsNavigationElement : MapNavigationElementBase
{
	public override string StringId => "quest";

	public override bool IsActive => base._game.GameStateManager.ActiveState is QuestsState;

	public override bool IsLockingNavigation => false;

	public override bool HasAlert => _viewDataTracker.IsQuestNotificationActive;

	public QuestsNavigationElement(MapNavigationHandler handler)
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
		if (current != null && !current.IsQuestScreenAccessAllowed)
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
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 42)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_quest", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_quest", (string)null);
	}

	protected override TextObject GetAlertTooltip()
	{
		if (HasAlert)
		{
			return _viewDataTracker.GetQuestNotificationText();
		}
		return TextObject.GetEmpty();
	}

	public override void OpenView()
	{
		PrepareToOpenQuestsScreen(delegate
		{
			OpenQuestsAction();
		});
	}

	public override void OpenView(params object[] parameters)
	{
		if (parameters.Length == 0)
		{
			return;
		}
		object obj = parameters[0];
		IssueBase issue;
		QuestBase quest;
		JournalLogEntry log;
		if ((issue = (IssueBase)((obj is IssueBase) ? obj : null)) != null)
		{
			PrepareToOpenQuestsScreen(delegate
			{
				OpenQuestsAction(issue);
			});
		}
		else if ((quest = (QuestBase)((obj is QuestBase) ? obj : null)) != null)
		{
			PrepareToOpenQuestsScreen(delegate
			{
				OpenQuestsAction(quest);
			});
		}
		else if ((log = (JournalLogEntry)((obj is JournalLogEntry) ? obj : null)) != null)
		{
			PrepareToOpenQuestsScreen(delegate
			{
				OpenQuestsAction(log);
			});
		}
		else
		{
			Debug.FailedAssert($"Invalid parameter type when opening the quest screen from navigation: {obj.GetType()}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\QuestsNavigationElement.cs", "OpenView", 97);
		}
	}

	public override void GoToLink()
	{
	}

	private void PrepareToOpenQuestsScreen(Action openQuestsAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = base.Permission;
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
			{
				InformationManager.ShowInquiry(changeableScreen.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry(openQuestsAction) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
			}
			else
			{
				MapNavigationHelper.SwitchToANewScreen(openQuestsAction);
			}
		}
	}

	private void OpenQuestsAction()
	{
		QuestsState val = base._game.GameStateManager.CreateState<QuestsState>();
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenQuestsAction(IssueBase issue)
	{
		QuestsState val = base._game.GameStateManager.CreateState<QuestsState>(new object[1] { issue });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenQuestsAction(QuestBase quest)
	{
		QuestsState val = base._game.GameStateManager.CreateState<QuestsState>(new object[1] { quest });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenQuestsAction(JournalLogEntry log)
	{
		QuestsState val = base._game.GameStateManager.CreateState<QuestsState>(new object[1] { log });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}
}
