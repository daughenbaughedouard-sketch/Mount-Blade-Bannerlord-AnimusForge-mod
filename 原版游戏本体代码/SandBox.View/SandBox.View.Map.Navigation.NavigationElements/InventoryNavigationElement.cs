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

public class InventoryNavigationElement : MapNavigationElementBase
{
	public override string StringId => "inventory";

	public override bool IsActive => base._game.GameStateManager.ActiveState is InventoryState;

	public override bool IsLockingNavigation
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			GameStateManager current = GameStateManager.Current;
			GameState obj = ((current != null) ? current.ActiveState : null);
			InventoryState val;
			if ((val = (InventoryState)(object)((obj is InventoryState) ? obj : null)) != null && val.InventoryLogic != null && (int)val.InventoryMode != 0)
			{
				return true;
			}
			return false;
		}
	}

	public override bool HasAlert => false;

	public InventoryNavigationElement(MapNavigationHandler handler)
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
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
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
		Mission current = Mission.Current;
		if (current != null && !current.IsInventoryAccessAllowed)
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
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 38)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_inventory", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_inventory", (string)null);
	}

	protected override TextObject GetAlertTooltip()
	{
		return TextObject.GetEmpty();
	}

	public override void OpenView()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = base.Permission;
		if (!((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			return;
		}
		if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
		{
			InformationManager.ShowInquiry(changeableScreen.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry(delegate
			{
				InventoryScreenHelper.OpenScreenAsInventory((Action)null);
			}) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
		}
		else
		{
			MapNavigationHelper.SwitchToANewScreen(delegate
			{
				InventoryScreenHelper.OpenScreenAsInventory((Action)null);
			});
		}
	}

	public override void OpenView(params object[] parameters)
	{
		Debug.FailedAssert("Inventory screen shouldn't be opened with parameters from navigation", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\InventoryNavigationElement.cs", "OpenView", 106);
		OpenView();
	}

	public override void GoToLink()
	{
	}
}
