using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map.Navigation.NavigationElements;

public class ClanNavigationElement : MapNavigationElementBase
{
	private readonly ClanScreenPermissionEvent _clanScreenPermissionEvent;

	private NavigationPermissionItem? _mostRecentClanScreenPermission;

	public override string StringId => "clan";

	public override bool IsActive => base._game.GameStateManager.ActiveState is ClanState;

	public override bool IsLockingNavigation => false;

	public override bool HasAlert => false;

	public ClanNavigationElement(MapNavigationHandler handler)
		: base(handler)
	{
		_clanScreenPermissionEvent = new ClanScreenPermissionEvent(OnClanScreenPermission);
	}

	protected override NavigationPermissionItem GetPermission()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!MapNavigationHelper.IsNavigationBarEnabled(_handler))
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		if (IsActive)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		Mission current = Mission.Current;
		if (current != null && !current.IsClanWindowAccessAllowed)
		{
			return new NavigationPermissionItem(false, (TextObject)null);
		}
		_mostRecentClanScreenPermission = null;
		Game.Current.EventManager.TriggerEvent<ClanScreenPermissionEvent>(_clanScreenPermissionEvent);
		return (NavigationPermissionItem)(((_003F?)_mostRecentClanScreenPermission) ?? new NavigationPermissionItem(true, (TextObject)null));
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
				string text = ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "GenericCampaignPanelsGameKeyCategory", 41)).ToString();
				TextObject obj = GameTexts.FindText("str_hotkey_with_hint", (string)null);
				obj.SetTextVariable("TEXT", ((object)GameTexts.FindText("str_clan", (string)null)).ToString());
				obj.SetTextVariable("HOTKEY", text);
				return obj;
			}
		}
		return GameTexts.FindText("str_clan", (string)null);
	}

	protected override TextObject GetAlertTooltip()
	{
		return TextObject.GetEmpty();
	}

	public override void OpenView()
	{
		PrepareToOpenClanScreen(delegate
		{
			OpenClanScreenAction();
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
		PartyBase party;
		Settlement settlement;
		Workshop workshop;
		Alley alley;
		if ((hero = (Hero)((obj is Hero) ? obj : null)) != null)
		{
			PrepareToOpenClanScreen(delegate
			{
				OpenClanScreenAction(hero);
			});
		}
		else if ((party = (PartyBase)((obj is PartyBase) ? obj : null)) != null)
		{
			PrepareToOpenClanScreen(delegate
			{
				OpenClanScreenAction(party);
			});
		}
		else if ((settlement = (Settlement)((obj is Settlement) ? obj : null)) != null)
		{
			PrepareToOpenClanScreen(delegate
			{
				OpenClanScreenAction(settlement);
			});
		}
		else if ((workshop = (Workshop)((obj is Workshop) ? obj : null)) != null)
		{
			PrepareToOpenClanScreen(delegate
			{
				OpenClanScreenAction(workshop);
			});
		}
		else if ((alley = (Alley)((obj is Alley) ? obj : null)) != null)
		{
			PrepareToOpenClanScreen(delegate
			{
				OpenClanScreenAction(alley);
			});
		}
		else
		{
			Debug.FailedAssert($"Invalid parameter type when opening the clan screen from navigation: {obj.GetType()}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\Navigation\\NavigationElements\\ClanNavigationElement.cs", "OpenView", 110);
		}
	}

	public override void GoToLink()
	{
		Campaign.Current.EncyclopediaManager.GoToLink(Hero.MainHero.Clan.EncyclopediaLink);
	}

	public void OnClanScreenPermission(bool isAvailable, TextObject reasonString)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!isAvailable)
		{
			_mostRecentClanScreenPermission = new NavigationPermissionItem(isAvailable, reasonString);
		}
	}

	private void PrepareToOpenClanScreen(Action openClanScreenAction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NavigationPermissionItem permission = base.Permission;
		if (((NavigationPermissionItem)(ref permission)).IsAuthorized)
		{
			if (ScreenManager.TopScreen is IChangeableScreen changeableScreen && changeableScreen.AnyUnsavedChanges())
			{
				InformationManager.ShowInquiry(changeableScreen.CanChangesBeApplied() ? MapNavigationHelper.GetUnsavedChangedInquiry(openClanScreenAction) : MapNavigationHelper.GetUnapplicableChangedInquiry(), false, false);
			}
			else
			{
				MapNavigationHelper.SwitchToANewScreen(openClanScreenAction);
			}
		}
	}

	private void OpenClanScreenAction()
	{
		ClanState val = base._game.GameStateManager.CreateState<ClanState>();
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenClanScreenAction(Hero hero)
	{
		ClanState val = base._game.GameStateManager.CreateState<ClanState>(new object[1] { hero });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenClanScreenAction(PartyBase party)
	{
		ClanState val = base._game.GameStateManager.CreateState<ClanState>(new object[1] { party });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenClanScreenAction(Settlement settlement)
	{
		ClanState val = base._game.GameStateManager.CreateState<ClanState>(new object[1] { settlement });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenClanScreenAction(Workshop workshop)
	{
		ClanState val = base._game.GameStateManager.CreateState<ClanState>(new object[1] { workshop });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}

	private void OpenClanScreenAction(Alley alley)
	{
		ClanState val = base._game.GameStateManager.CreateState<ClanState>(new object[1] { alley });
		base._game.GameStateManager.PushState((GameState)(object)val, 0);
	}
}
