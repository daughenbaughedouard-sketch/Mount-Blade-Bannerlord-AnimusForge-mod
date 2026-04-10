using System;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Navigation.NavigationElements;

public class PartyNavigationElement : MapNavigationElementBase
{
	public override string StringId => "party";

	public override bool IsActive => base._game.GameStateManager.ActiveState is PartyState;

	public override bool IsLockingNavigation
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			GameStateManager current = GameStateManager.Current;
			GameState obj = ((current != null) ? current.ActiveState : null);
			PartyState val;
			if ((val = (PartyState)(object)((obj is PartyState) ? obj : null)) != null && val.PartyScreenLogic != null && (int)val.PartyScreenMode != 0)
			{
				return true;
			}
			return false;
		}
	}

	public override bool HasAlert => _viewDataTracker.IsPartyNotificationActive;

	public PartyNavigationElement(MapNavigationHandler handler)
		: base(handler)
	{
	}

	protected override NavigationPermissionItem GetPermission()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (!MapNavigationHelper.IsNavigationBarEnabled(_handler))
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (IsActive)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (MobileParty.MainParty.IsInRaftState || (int)Hero.MainHero.HeroState == 3)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (MobileParty.MainParty.MapEvent != null)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		Mission current = Mission.Current;
		if (current != null && !current.IsPartyWindowAccessAllowed)
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
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 43)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_party", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_party", (string)null);
	}

	protected override TextObject GetAlertTooltip()
	{
		if (HasAlert)
		{
			return _viewDataTracker.GetPartyNotificationText();
		}
		return TextObject.GetEmpty();
	}

	public override void OpenView()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = base.Permission;
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
			{
				InformationManager.ShowInquiry(changeableScreen.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry((Action)PartyScreenHelper.OpenScreenAsNormal) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
			}
			else
			{
				MapNavigationHelper.SwitchToANewScreen((Action)PartyScreenHelper.OpenScreenAsNormal);
			}
		}
	}

	public override void OpenView(params object[] parameters)
	{
		Debug.FailedAssert("Party screen shouldn't be opened with parameters from navigation", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\PartyNavigationElement.cs", "OpenView", 118);
		OpenView();
	}

	public override void GoToLink()
	{
	}
}
