using System;
using System.Collections.Generic;
using Helpers;
using SandBox.View;
using SandBox.View.Map.Navigation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace NavalDLC.View.Map.Navigation;

public class ManageFleetNavigationElement : MapNavigationElementBase
{
	public override string StringId => "manage_fleet";

	public override bool IsActive => ((MapNavigationElementBase)this)._game.GameStateManager.ActiveState is PortState;

	public override bool IsLockingNavigation
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			GameState activeState = ((MapNavigationElementBase)this)._game.GameStateManager.ActiveState;
			PortState val;
			if ((val = (PortState)(object)((activeState is PortState) ? activeState : null)) != null)
			{
				return (int)val.PortScreenMode == 2;
			}
			return false;
		}
	}

	public override bool HasAlert => false;

	public ManageFleetNavigationElement(NavalMapNavigationHandler handler)
		: base((MapNavigationHandler)(object)handler)
	{
	}

	protected override TextObject GetAlertTooltip()
	{
		return TextObject.GetEmpty();
	}

	protected override TextObject GetTooltip()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!Input.IsGamepadActive)
		{
			NavigationPermissionItem permission = ((MapNavigationElementBase)this).Permission;
			if (((NavigationPermissionItem)(ref permission)).IsAuthorized || ((MapNavigationElementBase)this).IsActive)
			{
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 45)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_fleet", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_fleet", (string)null);
	}

	protected override NavigationPermissionItem GetPermission()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if (!MapNavigationHelper.IsNavigationBarEnabled(base._handler))
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (((MapNavigationElementBase)this).IsActive)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (((List<Ship>)(object)PartyBase.MainParty.Ships).Count == 0)
		{
			return new NavigationPermissionItem(false, new TextObject("{=lb2hbQyx}You don't have any ships", (Dictionary<string, object>)null));
		}
		if (Mission.Current != null)
		{
			return new NavigationPermissionItem(false, GameTexts.FindText("str_cannot_open_fleet", (string)null));
		}
		if (MobileParty.MainParty.MapEvent != null)
		{
			return new NavigationPermissionItem(false, GameTexts.FindText("str_cannot_open_fleet", (string)null));
		}
		if (MobileParty.MainParty.IsInRaftState)
		{
			return new NavigationPermissionItem(false, new TextObject("{=Lo0E5dKh}You cannot manage your fleet while you are drifting to shore", (Dictionary<string, object>)null));
		}
		if (Hero.MainHero.IsPrisoner)
		{
			return new NavigationPermissionItem(false, new TextObject("{=a8UQow7P}You cannot manage your fleet while you are imprisoned", (Dictionary<string, object>)null));
		}
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement != null && currentSettlement.HasPort)
		{
			return new NavigationPermissionItem(false, new TextObject("{=Ug3Tmhr5}You can access your fleet from the port", (Dictionary<string, object>)null));
		}
		MobileParty mainParty = MobileParty.MainParty;
		if (mainParty != null && !mainParty.IsCurrentlyAtSea)
		{
			return new NavigationPermissionItem(false, new TextObject("{=lVes97xY}You cannot access your fleet when you are on land", (Dictionary<string, object>)null));
		}
		return new NavigationPermissionItem(true, (TextObject)null);
	}

	public override void OpenView()
	{
		PrepareToOpenManageFleet(delegate
		{
			OpenManageFleetAction();
		});
	}

	public override void OpenView(params object[] parameters)
	{
		Debug.FailedAssert("Manage Fleet screen shouldn't be opened with parameters from navigation", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.View\\Map\\Navigation\\ManageFleetNavigationElement.cs", "OpenView", 106);
		((MapNavigationElementBase)this).OpenView();
	}

	public override void GoToLink()
	{
	}

	private void OpenManageFleetAction()
	{
		PortStateHelper.OpenAsManageFleet(new MBReadOnlyList<Ship>());
	}

	private void PrepareToOpenManageFleet(Action openManageFleetAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = ((MapNavigationElementBase)this).Permission;
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			ScreenBase topScreen = ScreenManager.TopScreen;
			IChangeableScreen val;
			if ((val = (IChangeableScreen)(object)((topScreen is IChangeableScreen) ? topScreen : null)) != null && val.AnyUnsavedChanges())
			{
				InformationManager.ShowInquiry(val.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry(openManageFleetAction) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
			}
			else
			{
				MapNavigationHelper.SwitchToANewScreen(openManageFleetAction);
			}
		}
	}
}
