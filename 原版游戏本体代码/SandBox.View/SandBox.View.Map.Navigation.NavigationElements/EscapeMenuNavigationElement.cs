using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.View.Map.Navigation.NavigationElements;

public class EscapeMenuNavigationElement : MapNavigationElementBase
{
	public override string StringId => "escape_menu";

	public override bool IsActive
	{
		get
		{
			if (base._game.GameStateManager.ActiveState is MapState)
			{
				return MapScreen.Instance?.IsEscapeMenuOpened ?? false;
			}
			return false;
		}
	}

	public override bool IsLockingNavigation => false;

	public override bool HasAlert => false;

	public EscapeMenuNavigationElement(MapNavigationHandler handler)
		: base(handler)
	{
	}

	protected override NavigationPermissionItem GetPermission()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!MapNavigationHelper.IsNavigationBarEnabled(_handler))
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (IsActive)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		return new NavigationPermissionItem(base._game.GameStateManager.ActiveState is MapState, (TextObject)null);
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
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericPanelGameKeyCategory", "ToggleEscapeMenu")).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_escape_menu", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_escape_menu", (string)null);
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
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			MapScreen.Instance?.OpenEscapeMenu();
		}
	}

	public override void OpenView(params object[] parameters)
	{
		Debug.FailedAssert("Escape menu shouldn't be opened with parameters from navigation", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\EscapeMenuNavigationElement.cs", "OpenView", 70);
		OpenView();
	}

	public override void GoToLink()
	{
	}
}
