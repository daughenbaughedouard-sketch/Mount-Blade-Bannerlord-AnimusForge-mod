using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed.General;

public class MPGeneralKillNotificationItemVM : ViewModel
{
	private readonly Action<MPGeneralKillNotificationItemVM> _onRemove;

	private readonly Banner DefaultBanner = Banner.CreateOneColoredEmptyBanner(92);

	private string _murdererName;

	private string _murdererType;

	private string _victimName;

	private string _victimType;

	private MPTeammateCompassTargetVM _murdererCompassElement;

	private MPTeammateCompassTargetVM _victimCompassElement;

	private Color _color1;

	private Color _color2;

	private bool _isPlayerDeath;

	private bool _isItemInitializationOver;

	private bool _isVictimBot;

	private bool _isMurdererBot;

	private bool _isDamageNotification;

	private bool _isDamagedMount;

	private bool _isRelatedToFriendlyTroop;

	private bool _isFriendlyTroopDeath;

	private string _message;

	[DataSourceProperty]
	public string MurdererName
	{
		get
		{
			return _murdererName;
		}
		set
		{
			if (value != _murdererName)
			{
				_murdererName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MurdererName");
			}
		}
	}

	[DataSourceProperty]
	public string MurdererType
	{
		get
		{
			return _murdererType;
		}
		set
		{
			if (value != _murdererType)
			{
				_murdererType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MurdererType");
			}
		}
	}

	[DataSourceProperty]
	public string VictimName
	{
		get
		{
			return _victimName;
		}
		set
		{
			if (value != _victimName)
			{
				_victimName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "VictimName");
			}
		}
	}

	[DataSourceProperty]
	public string VictimType
	{
		get
		{
			return _victimType;
		}
		set
		{
			if (value != _victimType)
			{
				_victimType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "VictimType");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDamageNotification
	{
		get
		{
			return _isDamageNotification;
		}
		set
		{
			if (value != _isDamageNotification)
			{
				_isDamageNotification = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDamageNotification");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDamagedMount
	{
		get
		{
			return _isDamagedMount;
		}
		set
		{
			if (value != _isDamagedMount)
			{
				_isDamagedMount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDamagedMount");
			}
		}
	}

	[DataSourceProperty]
	public Color Color1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _color1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _color1)
			{
				_color1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Color1");
			}
		}
	}

	[DataSourceProperty]
	public Color Color2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _color2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _color2)
			{
				_color2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Color2");
			}
		}
	}

	[DataSourceProperty]
	public MPTeammateCompassTargetVM MurdererCompassElement
	{
		get
		{
			return _murdererCompassElement;
		}
		set
		{
			if (value != _murdererCompassElement)
			{
				_murdererCompassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPTeammateCompassTargetVM>(value, "MurdererCompassElement");
			}
		}
	}

	[DataSourceProperty]
	public MPTeammateCompassTargetVM VictimCompassElement
	{
		get
		{
			return _victimCompassElement;
		}
		set
		{
			if (value != _victimCompassElement)
			{
				_victimCompassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPTeammateCompassTargetVM>(value, "VictimCompassElement");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerDeath
	{
		get
		{
			return _isPlayerDeath;
		}
		set
		{
			if (value != _isPlayerDeath)
			{
				_isPlayerDeath = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerDeath");
			}
		}
	}

	[DataSourceProperty]
	public bool IsItemInitializationOver
	{
		get
		{
			return _isItemInitializationOver;
		}
		set
		{
			if (value != _isItemInitializationOver)
			{
				_isItemInitializationOver = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsItemInitializationOver");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVictimBot
	{
		get
		{
			return _isVictimBot;
		}
		set
		{
			if (value != _isVictimBot)
			{
				_isVictimBot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVictimBot");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMurdererBot
	{
		get
		{
			return _isMurdererBot;
		}
		set
		{
			if (value != _isMurdererBot)
			{
				_isMurdererBot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMurdererBot");
			}
		}
	}

	[DataSourceProperty]
	public bool IsRelatedToFriendlyTroop
	{
		get
		{
			return _isRelatedToFriendlyTroop;
		}
		set
		{
			if (value != _isRelatedToFriendlyTroop)
			{
				_isRelatedToFriendlyTroop = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRelatedToFriendlyTroop");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFriendlyTroopDeath
	{
		get
		{
			return _isFriendlyTroopDeath;
		}
		set
		{
			if (value != _isFriendlyTroopDeath)
			{
				_isFriendlyTroopDeath = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFriendlyTroopDeath");
			}
		}
	}

	[DataSourceProperty]
	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (value != _message)
			{
				_message = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Message");
			}
		}
	}

	public MPGeneralKillNotificationItemVM(Agent affectedAgent, Agent affectorAgent, Agent assistedAgent, Action<MPGeneralKillNotificationItemVM> onRemove)
	{
		_onRemove = onRemove;
		IsDamageNotification = false;
		InitProperties(affectedAgent, affectorAgent);
		InitDeathProperties(affectedAgent, affectorAgent, assistedAgent);
	}

	public unsafe virtual void InitProperties(Agent affectedAgent, Agent affectorAgent)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		IsItemInitializationOver = false;
		GetAgentColors(affectorAgent, out var color, out var color2);
		TargetIconType multiplayerAgentType = GetMultiplayerAgentType(affectorAgent);
		Banner agentBanner = GetAgentBanner(affectorAgent);
		bool? obj;
		if (affectorAgent == null)
		{
			obj = null;
		}
		else
		{
			Team team = affectorAgent.Team;
			obj = ((team != null) ? new bool?(team.IsPlayerAlly) : ((bool?)null));
		}
		bool flag = obj ?? false;
		GetAgentColors(affectedAgent, out var color3, out var color4);
		TargetIconType multiplayerAgentType2 = GetMultiplayerAgentType(affectedAgent);
		Banner agentBanner2 = GetAgentBanner(affectedAgent);
		Team team2 = affectedAgent.Team;
		bool flag2 = team2 != null && team2.IsPlayerAlly;
		MurdererName = ((affectorAgent == null) ? "" : ((affectorAgent.MissionPeer != null) ? affectorAgent.MissionPeer.DisplayedName : affectorAgent.Name));
		MurdererType = ((object)(*(TargetIconType*)(&multiplayerAgentType))/*cast due to .constrained prefix*/).ToString();
		IsMurdererBot = affectorAgent != null && !affectorAgent.IsPlayerControlled;
		MurdererCompassElement = new MPTeammateCompassTargetVM(multiplayerAgentType, color, color2, agentBanner, flag);
		VictimName = ((affectedAgent.MissionPeer != null) ? affectedAgent.MissionPeer.DisplayedName : affectedAgent.Name);
		VictimType = ((object)(*(TargetIconType*)(&multiplayerAgentType2))/*cast due to .constrained prefix*/).ToString();
		IsVictimBot = !affectedAgent.IsPlayerControlled;
		VictimCompassElement = new MPTeammateCompassTargetVM(multiplayerAgentType2, color3, color4, agentBanner2, flag2);
		IsPlayerDeath = affectedAgent.IsMainAgent;
		if (flag && flag2)
		{
			Color1 = Color.FromUint(4278190080u);
			Color2 = Color.FromUint(uint.MaxValue);
		}
		else if (!flag && !flag2)
		{
			Color1 = Color.FromUint(4281545266u);
			Color2 = Color.FromUint(uint.MaxValue);
		}
		else
		{
			Color1 = Color.FromUint(color);
			Color2 = Color.FromUint(color2);
		}
		if (IsVictimBot)
		{
			Formation formation = affectedAgent.Formation;
			Agent main = Agent.Main;
			if (formation == ((main != null) ? main.Formation : null))
			{
				IsRelatedToFriendlyTroop = true;
				IsFriendlyTroopDeath = true;
				goto IL_0220;
			}
		}
		if (IsMurdererBot && affectorAgent != null)
		{
			Formation formation2 = affectorAgent.Formation;
			Agent main2 = Agent.Main;
			if (formation2 == ((main2 != null) ? main2.Formation : null))
			{
				IsRelatedToFriendlyTroop = true;
			}
		}
		goto IL_0220;
		IL_0220:
		IsItemInitializationOver = true;
	}

	public void InitDeathProperties(Agent affectedAgent, Agent affectorAgent, Agent assistedAgent)
	{
		IsItemInitializationOver = false;
		if (affectorAgent != null && affectorAgent.IsMainAgent)
		{
			MBTextManager.SetTextVariable("TROOP_NAME", ((object)affectedAgent.NameTextObject).ToString(), false);
			Message = ((object)GameTexts.FindText("str_kill_feed_message", (string)null)).ToString();
		}
		else if (affectedAgent.IsMainAgent)
		{
			MBTextManager.SetTextVariable("TROOP_NAME", ((object)affectorAgent)?.ToString(), false);
			Message = ((object)GameTexts.FindText("str_death_feed_message", (string)null)).ToString();
		}
		else if (assistedAgent != null && assistedAgent.IsMainAgent)
		{
			MBTextManager.SetTextVariable("TROOP_NAME", ((object)affectedAgent.NameTextObject).ToString(), false);
			Message = ((object)GameTexts.FindText("str_assist_feed_message", (string)null)).ToString();
		}
		IsItemInitializationOver = true;
	}

	protected TargetIconType GetMultiplayerAgentType(Agent agent)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (agent == null)
		{
			return (TargetIconType)(-1);
		}
		if (!agent.IsHuman)
		{
			return (TargetIconType)0;
		}
		MPHeroClass mPHeroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
		if (mPHeroClassForCharacter == null)
		{
			Debug.FailedAssert("Hero class is not set for agent: " + agent.Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\KillFeed\\General\\MPGeneralKillNotificationItemVM.cs", "GetMultiplayerAgentType", 116);
			return (TargetIconType)(-1);
		}
		return mPHeroClassForCharacter.IconType;
	}

	private Banner GetAgentBanner(Agent agent)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		Banner result = DefaultBanner;
		if (agent != null)
		{
			MissionPeer missionPeer = agent.MissionPeer;
			MissionPeer val = ((missionPeer != null) ? ((PeerComponent)missionPeer).GetComponent<MissionPeer>() : null);
			if (agent.Team != null && val != null)
			{
				result = new Banner(((PeerComponent)val).Peer.BannerCode, agent.Team.Color, agent.Team.Color2);
			}
			else if (agent.Team != null && agent.Formation != null && !string.IsNullOrEmpty(agent.Formation.BannerCode))
			{
				result = new Banner(agent.Formation.BannerCode, agent.Team.Color, agent.Team.Color2);
			}
			else if (agent.Team != null)
			{
				result = agent.Team.Banner;
			}
		}
		return result;
	}

	private void GetAgentColors(Agent agent, out uint color1, out uint color2)
	{
		if (((agent != null) ? agent.Team : null) != null)
		{
			color1 = agent.Team.Color;
			color2 = agent.Team.Color2;
		}
		else
		{
			color1 = 4284111450u;
			color2 = uint.MaxValue;
		}
	}

	public void ExecuteRemove()
	{
		_onRemove(this);
	}
}
