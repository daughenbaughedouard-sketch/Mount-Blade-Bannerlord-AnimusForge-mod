using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.EndOfRound;

public class MultiplayerEndOfRoundVM : ViewModel
{
	private readonly MissionScoreboardComponent _scoreboardComponent;

	private readonly MissionLobbyComponent _missionLobbyComponent;

	private readonly IRoundComponent _multiplayerRoundComponent;

	private readonly string _victoryText;

	private readonly string _defeatText;

	private readonly TextObject _roundEndReasonAllyTeamSideDepletedTextObject;

	private readonly TextObject _roundEndReasonEnemyTeamSideDepletedTextObject;

	private readonly TextObject _roundEndReasonAllyTeamRoundTimeEndedTextObject;

	private readonly TextObject _roundEndReasonEnemyTeamRoundTimeEndedTextObject;

	private readonly TextObject _roundEndReasonAllyTeamGameModeSpecificEndedTextObject;

	private readonly TextObject _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject;

	private readonly TextObject _roundEndReasonRoundTimeEndedWithDrawTextObject;

	private bool _isShown;

	private bool _hasAttackerMVP;

	private bool _hasDefenderMVP;

	private string _title;

	private string _description;

	private string _cultureId;

	private bool _isRoundWinner;

	private MultiplayerEndOfRoundSideVM _attackerSide;

	private MultiplayerEndOfRoundSideVM _defenderSide;

	private MPPlayerVM _attackerMVP;

	private MPPlayerVM _defenderMVP;

	private string _attackerMVPTitleText;

	private string _defenderMVPTitleText;

	[DataSourceProperty]
	public bool IsShown
	{
		get
		{
			return _isShown;
		}
		set
		{
			if (value != _isShown)
			{
				_isShown = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsShown");
				OnIsShownChanged();
			}
		}
	}

	[DataSourceProperty]
	public bool HasAttackerMVP
	{
		get
		{
			return _hasAttackerMVP;
		}
		set
		{
			if (value != _hasAttackerMVP)
			{
				_hasAttackerMVP = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasAttackerMVP");
			}
		}
	}

	[DataSourceProperty]
	public bool HasDefenderMVP
	{
		get
		{
			return _hasDefenderMVP;
		}
		set
		{
			if (value != _hasDefenderMVP)
			{
				_hasDefenderMVP = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasDefenderMVP");
			}
		}
	}

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Title");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Description");
			}
		}
	}

	[DataSourceProperty]
	public string CultureId
	{
		get
		{
			return _cultureId;
		}
		set
		{
			if (value != _cultureId)
			{
				_cultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureId");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRoundWinner
	{
		get
		{
			return _isRoundWinner;
		}
		set
		{
			if (value != _isRoundWinner)
			{
				_isRoundWinner = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRoundWinner");
			}
		}
	}

	[DataSourceProperty]
	public MultiplayerEndOfRoundSideVM AttackerSide
	{
		get
		{
			return _attackerSide;
		}
		set
		{
			if (value != _attackerSide)
			{
				_attackerSide = value;
				((ViewModel)this).OnPropertyChangedWithValue<MultiplayerEndOfRoundSideVM>(value, "AttackerSide");
			}
		}
	}

	[DataSourceProperty]
	public MultiplayerEndOfRoundSideVM DefenderSide
	{
		get
		{
			return _defenderSide;
		}
		set
		{
			if (value != _defenderSide)
			{
				_defenderSide = value;
				((ViewModel)this).OnPropertyChangedWithValue<MultiplayerEndOfRoundSideVM>(value, "DefenderSide");
			}
		}
	}

	[DataSourceProperty]
	public MPPlayerVM AttackerMVP
	{
		get
		{
			return _attackerMVP;
		}
		set
		{
			if (value != _attackerMVP)
			{
				_attackerMVP = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPPlayerVM>(value, "AttackerMVP");
			}
		}
	}

	[DataSourceProperty]
	public MPPlayerVM DefenderMVP
	{
		get
		{
			return _defenderMVP;
		}
		set
		{
			if (value != _defenderMVP)
			{
				_defenderMVP = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPPlayerVM>(value, "DefenderMVP");
			}
		}
	}

	[DataSourceProperty]
	public string AttackerMVPTitleText
	{
		get
		{
			return _attackerMVPTitleText;
		}
		set
		{
			if (value != _attackerMVPTitleText)
			{
				_attackerMVPTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "AttackerMVPTitleText");
			}
		}
	}

	[DataSourceProperty]
	public string DefenderMVPTitleText
	{
		get
		{
			return _defenderMVPTitleText;
		}
		set
		{
			if (value != _defenderMVPTitleText)
			{
				_defenderMVPTitleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DefenderMVPTitleText");
			}
		}
	}

	public MultiplayerEndOfRoundVM(MissionScoreboardComponent scoreboardComponent, MissionLobbyComponent missionLobbyComponent, IRoundComponent multiplayerRoundComponent)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Invalid comparison between Unknown and I4
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Invalid comparison between Unknown and I4
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Invalid comparison between Unknown and I4
		_scoreboardComponent = scoreboardComponent;
		_multiplayerRoundComponent = multiplayerRoundComponent;
		_missionLobbyComponent = missionLobbyComponent;
		_victoryText = ((object)new TextObject("{=RCuCoVgd}ROUND WON", (Dictionary<string, object>)null)).ToString();
		_defeatText = ((object)new TextObject("{=Dbkx4v90}ROUND LOST", (Dictionary<string, object>)null)).ToString();
		_roundEndReasonAllyTeamSideDepletedTextObject = new TextObject("{=9M4G8DDd}Your team was wiped out", (Dictionary<string, object>)null);
		_roundEndReasonEnemyTeamSideDepletedTextObject = new TextObject("{=jPXglGWT}Enemy team was wiped out", (Dictionary<string, object>)null);
		_roundEndReasonAllyTeamRoundTimeEndedTextObject = new TextObject("{=x1HZy70i}Your team had the upper hand at timeout", (Dictionary<string, object>)null);
		_roundEndReasonEnemyTeamRoundTimeEndedTextObject = new TextObject("{=Dc3fFblo}Enemy team had the upper hand at timeout", (Dictionary<string, object>)null);
		_roundEndReasonRoundTimeEndedWithDrawTextObject = new TextObject("{=i3dJSlD0}No team had the upper hand at timeout", (Dictionary<string, object>)null);
		if ((int)_missionLobbyComponent.MissionType == 3 || (int)_missionLobbyComponent.MissionType == 4 || (int)_missionLobbyComponent.MissionType == 5)
		{
			_roundEndReasonAllyTeamGameModeSpecificEndedTextObject = new TextObject("{=xxuzZJ3G}Your team ran out of morale", (Dictionary<string, object>)null);
			_roundEndReasonEnemyTeamGameModeSpecificEndedTextObject = new TextObject("{=c6c9eYrD}Enemy team ran out of morale", (Dictionary<string, object>)null);
		}
		else
		{
			_roundEndReasonAllyTeamGameModeSpecificEndedTextObject = TextObject.GetEmpty();
			_roundEndReasonEnemyTeamGameModeSpecificEndedTextObject = TextObject.GetEmpty();
		}
		AttackerSide = new MultiplayerEndOfRoundSideVM();
		DefenderSide = new MultiplayerEndOfRoundSideVM();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		if (_multiplayerRoundComponent != null)
		{
			Refresh();
		}
	}

	public void Refresh()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Invalid comparison between Unknown and I4
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Invalid comparison between Unknown and I4
		BattleSideEnum allyBattleSideEnum = (BattleSideEnum)(-1);
		BattleSideEnum val = (BattleSideEnum)(-1);
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		MissionPeer val2 = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
		if (val2 != null && val2.Team != null)
		{
			allyBattleSideEnum = val2.Team.Side;
			val = (BattleSideEnum)((int)allyBattleSideEnum != 1);
		}
		bool num = (int)allyBattleSideEnum == 1;
		MissionScoreboardSide val3 = ((IEnumerable<MissionScoreboardSide>)_scoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && (int)s.Side == 1));
		MissionScoreboardSide val4 = ((IEnumerable<MissionScoreboardSide>)_scoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && (int)s.Side == 0));
		MissionScoreboardSide val5 = (num ? val3 : val4);
		MissionScoreboardSide val6 = (num ? val4 : val3);
		BasicCultureObject culture = val5.GetCulture();
		BasicCultureObject culture2 = val6.GetCulture();
		bool isWinner = _multiplayerRoundComponent.RoundWinner == allyBattleSideEnum;
		bool isWinner2 = _multiplayerRoundComponent.RoundWinner == val;
		AttackerMVPTitleText = GetMVPTitleText(culture);
		DefenderMVPTitleText = GetMVPTitleText(culture2);
		MultiplayerBattleColors val7 = MultiplayerBattleColors.CreateWith(culture, culture2);
		AttackerSide.SetData(culture, val5.SideScore, isWinner, val7.AttackerColors);
		DefenderSide.SetData(culture2, val6.SideScore, isWinner2, val7.DefenderColors);
		if (((IEnumerable<MissionScoreboardSide>)_scoreboardComponent.Sides).FirstOrDefault((Func<MissionScoreboardSide, bool>)((MissionScoreboardSide s) => s != null && s.Side == allyBattleSideEnum)) != null && _multiplayerRoundComponent != null)
		{
			bool flag = false;
			if (_multiplayerRoundComponent.RoundWinner == allyBattleSideEnum)
			{
				IsRoundWinner = true;
				Title = _victoryText;
			}
			else if (_multiplayerRoundComponent.RoundWinner == val)
			{
				IsRoundWinner = false;
				Title = _defeatText;
			}
			else
			{
				flag = true;
			}
			RoundEndReason roundEndReason = _multiplayerRoundComponent.RoundEndReason;
			if ((int)roundEndReason == 0)
			{
				Description = (IsRoundWinner ? ((object)_roundEndReasonEnemyTeamSideDepletedTextObject).ToString() : ((object)_roundEndReasonAllyTeamSideDepletedTextObject).ToString());
			}
			else if ((int)roundEndReason == 2)
			{
				Description = (IsRoundWinner ? ((object)_roundEndReasonEnemyTeamGameModeSpecificEndedTextObject).ToString() : ((object)_roundEndReasonAllyTeamGameModeSpecificEndedTextObject).ToString());
			}
			else if ((int)roundEndReason == 1)
			{
				Description = (IsRoundWinner ? ((object)_roundEndReasonAllyTeamRoundTimeEndedTextObject).ToString() : (flag ? ((object)_roundEndReasonRoundTimeEndedWithDrawTextObject).ToString() : ((object)_roundEndReasonEnemyTeamRoundTimeEndedTextObject).ToString()));
			}
		}
	}

	public void OnMVPSelected(MissionPeer mvpPeer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		BasicCharacterObject val = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
		val.UpdatePlayerCharacterBodyProperties(((PeerComponent)mvpPeer).Peer.BodyProperties, ((PeerComponent)mvpPeer).Peer.Race, ((PeerComponent)mvpPeer).Peer.IsFemale);
		BodyProperties bodyProperties = ((PeerComponent)mvpPeer).Peer.BodyProperties;
		val.Age = ((BodyProperties)(ref bodyProperties)).Age;
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		MissionPeer obj = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
		Team team = mvpPeer.Team;
		BattleSideEnum? val2 = ((team != null) ? new BattleSideEnum?(team.Side) : ((BattleSideEnum?)null));
		Team team2 = obj.Team;
		if (val2 == ((team2 != null) ? new BattleSideEnum?(team2.Side) : ((BattleSideEnum?)null)))
		{
			AttackerMVP = new MPPlayerVM(mvpPeer);
			AttackerMVP.RefreshDivision();
			MPPlayerVM attackerMVP = AttackerMVP;
			bodyProperties = ((PeerComponent)mvpPeer).Peer.BodyProperties;
			attackerMVP.RefreshPreview(val, ((BodyProperties)(ref bodyProperties)).DynamicProperties, ((PeerComponent)mvpPeer).Peer.IsFemale);
			HasAttackerMVP = true;
		}
		else
		{
			DefenderMVP = new MPPlayerVM(mvpPeer);
			DefenderMVP.RefreshDivision();
			MPPlayerVM defenderMVP = DefenderMVP;
			bodyProperties = ((PeerComponent)mvpPeer).Peer.BodyProperties;
			defenderMVP.RefreshPreview(val, ((BodyProperties)(ref bodyProperties)).DynamicProperties, ((PeerComponent)mvpPeer).Peer.IsFemale);
			HasDefenderMVP = true;
		}
	}

	private string GetMVPTitleText(BasicCultureObject culture)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		if (((MBObjectBase)culture).StringId == "vlandia")
		{
			return ((object)new TextObject("{=3VosbFR0}Vlandian Champion", (Dictionary<string, object>)null)).ToString();
		}
		if (((MBObjectBase)culture).StringId == "sturgia")
		{
			return ((object)new TextObject("{=AGUXiN8u}Voivode", (Dictionary<string, object>)null)).ToString();
		}
		if (((MBObjectBase)culture).StringId == "khuzait")
		{
			return ((object)new TextObject("{=F2h2cT4q}Khan's Chosen", (Dictionary<string, object>)null)).ToString();
		}
		if (((MBObjectBase)culture).StringId == "battania")
		{
			return ((object)new TextObject("{=eWPN3HmE}Hero of Battania", (Dictionary<string, object>)null)).ToString();
		}
		if (((MBObjectBase)culture).StringId == "aserai")
		{
			return ((object)new TextObject("{=5zNfxZ7B}War Prince", (Dictionary<string, object>)null)).ToString();
		}
		if (((MBObjectBase)culture).StringId == "empire")
		{
			return ((object)new TextObject("{=wwbIcqsq}Conqueror", (Dictionary<string, object>)null)).ToString();
		}
		Debug.FailedAssert("Invalid Culture ID for MVP Title", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\EndOfRound\\MultiplayerEndOfRoundVM.cs", "GetMVPTitleText", 205);
		return string.Empty;
	}

	private void OnIsShownChanged()
	{
		if (!IsShown)
		{
			HasAttackerMVP = false;
			HasDefenderMVP = false;
		}
	}
}
