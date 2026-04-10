using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Navigation.NavigationElements;

public class KingdomNavigationElement : MapNavigationElementBase
{
	private readonly TextObject _needToBeInKingdomText;

	public override string StringId => "kingdom";

	public override bool IsActive => base._game.GameStateManager.ActiveState is KingdomState;

	public override bool IsLockingNavigation => false;

	public override bool HasAlert => false;

	public KingdomNavigationElement(MapNavigationHandler handler)
		: base(handler)
	{
		_needToBeInKingdomText = GameTexts.FindText("str_need_to_be_a_part_of_kingdom", (string)null);
	}

	protected override NavigationPermissionItem GetPermission()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!MapNavigationHelper.IsNavigationBarEnabled(_handler))
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (IsActive)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (!Hero.MainHero.MapFaction.IsKingdomFaction)
		{
			return new NavigationPermissionItem(false, _needToBeInKingdomText);
		}
		Mission current = Mission.Current;
		if (current != null && !current.IsKingdomWindowAccessAllowed)
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
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 40)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_kingdom", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_kingdom", (string)null);
	}

	protected override TextObject GetAlertTooltip()
	{
		return TextObject.GetEmpty();
	}

	public override void OpenView()
	{
		PrepareToOpenKingdomScreen(delegate
		{
			OpenKingdomAction();
		});
	}

	public override void OpenView(params object[] parameters)
	{
		if (parameters.Length == 0)
		{
			return;
		}
		object obj = parameters[0];
		Army army;
		Settlement settlement;
		Clan clan;
		PolicyObject policy;
		IFaction faction;
		KingdomDecision decision;
		if ((army = (Army)((obj is Army) ? obj : null)) != null)
		{
			PrepareToOpenKingdomScreen(delegate
			{
				OpenKingdomAction(army);
			});
		}
		else if ((settlement = (Settlement)((obj is Settlement) ? obj : null)) != null)
		{
			PrepareToOpenKingdomScreen(delegate
			{
				OpenKingdomAction(settlement);
			});
		}
		else if ((clan = (Clan)((obj is Clan) ? obj : null)) != null)
		{
			PrepareToOpenKingdomScreen(delegate
			{
				OpenKingdomAction(clan);
			});
		}
		else if ((policy = (PolicyObject)((obj is PolicyObject) ? obj : null)) != null)
		{
			PrepareToOpenKingdomScreen(delegate
			{
				OpenKingdomAction(policy);
			});
		}
		else if ((faction = (IFaction)((obj is IFaction) ? obj : null)) != null)
		{
			PrepareToOpenKingdomScreen(delegate
			{
				OpenKingdomAction(faction);
			});
		}
		else if ((decision = (KingdomDecision)((obj is KingdomDecision) ? obj : null)) != null)
		{
			PrepareToOpenKingdomScreen(delegate
			{
				OpenKingdomAction(decision);
			});
		}
		else
		{
			Debug.FailedAssert($"Invalid parameter type when opening the kingdom screen from navigation: {obj.GetType()}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\KindomNavigationElement.cs", "OpenView", 113);
		}
	}

	public override void GoToLink()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.MainHero.MapFaction.EncyclopediaLink);
	}

	private void PrepareToOpenKingdomScreen(Action openKingdomAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = base.Permission;
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
			{
				InformationManager.ShowInquiry(changeableScreen.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry(openKingdomAction) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
			}
			else
			{
				MapNavigationHelper.SwitchToANewScreen(openKingdomAction);
			}
		}
	}

	private void OpenKingdomAction()
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>();
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenKingdomAction(Army army)
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>(new object[1] { army });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenKingdomAction(Settlement settlement)
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>(new object[1] { settlement });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenKingdomAction(Clan clan)
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>(new object[1] { clan });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenKingdomAction(PolicyObject policy)
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>(new object[1] { policy });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenKingdomAction(IFaction faction)
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>(new object[1] { faction });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenKingdomAction(KingdomDecision decision)
	{
		KingdomState val = base._game.GameStateManager.CreateState<KingdomState>(new object[1] { decision });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}
}
