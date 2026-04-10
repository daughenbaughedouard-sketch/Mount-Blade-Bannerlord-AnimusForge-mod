using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerEscapeMenu))]
public class MissionGauntletMultiplayerEscapeMenu : MissionGauntletEscapeMenuBase
{
	private MissionOptionsComponent _missionOptionsComponent;

	private MissionLobbyComponent _missionLobbyComponent;

	private MultiplayerAdminComponent _missionAdminComponent;

	private MultiplayerTeamSelectComponent _missionTeamSelectComponent;

	private MissionMultiplayerGameModeBaseClient _gameModeClient;

	private readonly string _gameType;

	private EscapeMenuItemVM _changeTroopItem;

	private EscapeMenuItemVM _changeCultureItem;

	public MissionGauntletMultiplayerEscapeMenu(string gameType)
		: base("MultiplayerEscapeMenu")
	{
		_gameType = gameType;
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		_missionOptionsComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionOptionsComponent>();
		_missionLobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_missionAdminComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerAdminComponent>();
		_missionTeamSelectComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerTeamSelectComponent>();
		_gameModeClient = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		TextObject title = GameTexts.FindText("str_multiplayer_game_type", _gameType);
		base.DataSource = (EscapeMenuVM)(object)new MPEscapeMenuVM(null, title);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionGauntletEscapeMenuBase)this).OnMissionScreenTick(dt);
		base.DataSource.Tick(dt);
	}

	public override bool OnEscape()
	{
		bool result = ((MissionGauntletEscapeMenuBase)this).OnEscape();
		if (((MissionEscapeMenuView)this).IsActive)
		{
			if (_gameModeClient.IsGameModeUsingAllowTroopChange)
			{
				_changeTroopItem.IsDisabled = !_gameModeClient.CanRequestTroopChange();
			}
			if (_gameModeClient.IsGameModeUsingAllowCultureChange)
			{
				_changeCultureItem.IsDisabled = !_gameModeClient.CanRequestCultureChange();
			}
		}
		return result;
	}

	protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Invalid comparison between Unknown and I4
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_0264: Expected O, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected O, but got Unknown
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected O, but got Unknown
		List<EscapeMenuItemVM> list = new List<EscapeMenuItemVM>();
		list.Add(new EscapeMenuItemVM(new TextObject("{=e139gKZc}Return to the Game", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
		list.Add(new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
			MissionOptionsComponent missionOptionsComponent = _missionOptionsComponent;
			if (missionOptionsComponent != null)
			{
				missionOptionsComponent.OnAddOptionsUIHandler();
			}
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
		MultiplayerTeamSelectComponent missionTeamSelectComponent = _missionTeamSelectComponent;
		if (missionTeamSelectComponent != null && missionTeamSelectComponent.TeamSelectionEnabled)
		{
			list.Add(new EscapeMenuItemVM(new TextObject("{=2SEofGth}Change Team", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
				if (_missionTeamSelectComponent != null)
				{
					_missionTeamSelectComponent.SelectTeam();
				}
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
		}
		if (_gameModeClient.IsGameModeUsingAllowCultureChange)
		{
			_changeCultureItem = new EscapeMenuItemVM(new TextObject("{=aGGq9lJT}Change Culture", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
				_missionLobbyComponent.RequestCultureSelection();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false);
			list.Add(_changeCultureItem);
		}
		if (_gameModeClient.IsGameModeUsingAllowTroopChange)
		{
			_changeTroopItem = new EscapeMenuItemVM(new TextObject("{=Yza0JYJt}Change Troop", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
				_missionLobbyComponent.RequestTroopSelection();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false);
			list.Add(_changeTroopItem);
		}
		if ((int)((MissionBehavior)this).Mission.CurrentState == 2 && ((MissionBehavior)this).Mission.GetMissionEndTimerValue() < 0f && (GameNetwork.MyPeer.IsAdmin || GameNetwork.IsServer))
		{
			EscapeMenuItemVM item = new EscapeMenuItemVM(new TextObject("{=xILeUbY3}Admin Panel", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
				if (_missionAdminComponent != null)
				{
					_missionAdminComponent.ChangeAdminMenuActiveState(isActive: true);
				}
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false);
			list.Add(item);
		}
		list.Add(new EscapeMenuItemVM(new TextObject("{=InGwtrWt}Quit", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected O, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=InGwtrWt}Quit", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=lxq6SaQn}Are you sure want to quit?", (Dictionary<string, object>)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)delegate
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Invalid comparison between Unknown and I4
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Invalid comparison between Unknown and I4
				LobbyClient gameClient = NetworkMain.GameClient;
				CommunityClient communityClient = NetworkMain.CommunityClient;
				if (communityClient.IsInGame)
				{
					communityClient.QuitFromGame();
				}
				else if ((int)gameClient.CurrentState == 16)
				{
					gameClient.QuitFromCustomGame();
				}
				else if ((int)gameClient.CurrentState == 14)
				{
					gameClient.EndCustomGame();
				}
				else
				{
					gameClient.QuitFromMatchmakerGame();
				}
			}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
		return list;
	}
}
