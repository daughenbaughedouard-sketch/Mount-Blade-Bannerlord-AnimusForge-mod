using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Avatar.PlayerServices;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.Compass;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MPPlayerVM : ViewModel
{
	private MPHeroClass _cachedClass;

	private BasicCultureObject _cachedCulture;

	private readonly MissionMultiplayerGameModeBaseClient _gameMode;

	private readonly MissionRepresentativeBase _missionRepresentative;

	private readonly bool _isInParty;

	private readonly bool _isKnownPlayer;

	private TextObject _genericPlayerName = new TextObject("{=RN6zHak0}Player", (Dictionary<string, object>)null);

	private const uint _focusedContourColor = 4278255612u;

	private const uint _defaultContourColor = 0u;

	private const uint _invalidColor = 0u;

	private int _gold;

	private int _valuePercent;

	private string _name;

	private string _cultureID;

	private bool _isDead;

	private bool _isValueEnabled;

	private bool _hasSetCompassElement;

	private bool _isSpawnActive;

	private bool _isFocused;

	private MPTeammateCompassTargetVM _compassElement;

	private PlayerAvatarImageIdentifierVM _avatar;

	private MPArmoryHeroPreviewVM _preview;

	private MBBindingList<MPPerkVM> _activePerks;

	public MissionPeer Peer { get; private set; }

	private Team _playerTeam
	{
		get
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Invalid comparison between Unknown and I4
			if (!GameNetwork.IsMyPeerReady)
			{
				return null;
			}
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			if (component.Team == null || (int)component.Team.Side == -1)
			{
				return null;
			}
			return component.Team;
		}
	}

	[DataSourceProperty]
	public int Gold
	{
		get
		{
			return _gold;
		}
		set
		{
			if (value != _gold)
			{
				_gold = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Gold");
			}
		}
	}

	[DataSourceProperty]
	public int ValuePercent
	{
		get
		{
			return _valuePercent;
		}
		set
		{
			if (value != _valuePercent)
			{
				_valuePercent = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ValuePercent");
			}
		}
	}

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public string CultureID
	{
		get
		{
			return _cultureID;
		}
		set
		{
			if (value != _cultureID)
			{
				_cultureID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureID");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDead
	{
		get
		{
			return _isDead;
		}
		set
		{
			if (value != _isDead)
			{
				_isDead = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDead");
			}
		}
	}

	[DataSourceProperty]
	public bool IsValueEnabled
	{
		get
		{
			return _isValueEnabled;
		}
		set
		{
			if (value != _isValueEnabled)
			{
				_isValueEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsValueEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool HasSetCompassElement
	{
		get
		{
			return _hasSetCompassElement;
		}
		set
		{
			if (value != _hasSetCompassElement)
			{
				_hasSetCompassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSetCompassElement");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSpawnActive
	{
		get
		{
			return _isSpawnActive;
		}
		set
		{
			if (value != _isSpawnActive)
			{
				_isSpawnActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSpawnActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		set
		{
			if (value != _isFocused)
			{
				_isFocused = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFocused");
			}
		}
	}

	[DataSourceProperty]
	public MPTeammateCompassTargetVM CompassElement
	{
		get
		{
			return _compassElement;
		}
		set
		{
			if (value != _compassElement)
			{
				_compassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPTeammateCompassTargetVM>(value, "CompassElement");
			}
		}
	}

	[DataSourceProperty]
	public PlayerAvatarImageIdentifierVM Avatar
	{
		get
		{
			return _avatar;
		}
		set
		{
			if (value != _avatar)
			{
				_avatar = value;
				((ViewModel)this).OnPropertyChangedWithValue<PlayerAvatarImageIdentifierVM>(value, "Avatar");
			}
		}
	}

	[DataSourceProperty]
	public MPArmoryHeroPreviewVM Preview
	{
		get
		{
			return _preview;
		}
		set
		{
			if (value != _preview)
			{
				_preview = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPArmoryHeroPreviewVM>(value, "Preview");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPPerkVM> ActivePerks
	{
		get
		{
			return _activePerks;
		}
		set
		{
			if (value != _activePerks)
			{
				_activePerks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPerkVM>>(value, "ActivePerks");
			}
		}
	}

	public MPPlayerVM(Agent agent)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (agent != null)
		{
			MPHeroClass mPHeroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
			TargetIconType iconType = (TargetIconType)((mPHeroClassForCharacter == null) ? (-1) : ((int)mPHeroClassForCharacter.IconType));
			Team team = agent.Team;
			uint num = ((team != null) ? team.Color : 0u);
			Team team2 = agent.Team;
			uint num2 = ((team2 != null) ? team2.Color2 : 0u);
			Banner banner = new Banner(agent.Team.Banner, num, num2);
			CompassElement = new MPTeammateCompassTargetVM(iconType, num, num2, banner, isAlly: false);
		}
		else
		{
			CompassElement = new MPTeammateCompassTargetVM((TargetIconType)0, 0u, 0u, Banner.CreateOneColoredEmptyBanner(0), isAlly: false);
		}
	}

	public MPPlayerVM(MissionPeer peer)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Peer = peer;
		_gameMode = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
		_missionRepresentative = ((PeerComponent)peer).GetComponent<MissionRepresentativeBase>();
		_isInParty = NetworkMain.GameClient.IsInParty;
		_isKnownPlayer = NetworkMain.GameClient.IsKnownPlayer(((PeerComponent)Peer).Peer.Id);
		RefreshAvatar();
		Name = peer.DisplayedName;
		ActivePerks = new MBBindingList<MPPerkVM>();
		((ViewModel)this).RefreshValues();
	}

	public void UpdateDisabled()
	{
		IsDead = !Peer.IsControlledAgentActive;
	}

	public void RefreshDivision(bool useCultureColors = false)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		if (Peer == null || Peer.Culture == null)
		{
			return;
		}
		MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(Peer, false);
		TargetIconType val = (TargetIconType)((mPHeroClassForPeer == null) ? (-1) : ((int)mPHeroClassForPeer.IconType));
		if (_cachedClass == null || _cachedClass != mPHeroClassForPeer || _cachedCulture == null || _cachedCulture != Peer.Culture)
		{
			_cachedClass = mPHeroClassForPeer;
			_cachedCulture = Peer.Culture;
			Team team = Peer.Team;
			uint num = ((team != null) ? team.Color : 0u);
			Team team2 = Peer.Team;
			uint num2 = ((team2 != null) ? team2.Color2 : 0u);
			if (useCultureColors)
			{
				BasicCultureObject obj = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1));
				BasicCultureObject val2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
				MultiplayerBattleColors val3 = MultiplayerBattleColors.CreateWith(obj, val2);
				MultiplayerCultureColorInfo peerColors = ((MultiplayerBattleColors)(ref val3)).GetPeerColors(Peer);
				num = peerColors.Color1Uint;
				num2 = peerColors.Color2Uint;
			}
			Banner banner = new Banner(((PeerComponent)Peer).Peer.BannerCode, num, num2);
			uint color = num;
			uint color2 = num2;
			Team team3 = Peer.Team;
			CompassElement = new MPTeammateCompassTargetVM(val, color, color2, banner, team3 != null && team3.IsPlayerAlly);
			HasSetCompassElement = true;
			Name = Peer.DisplayedName;
			RefreshActivePerks();
			CultureID = ((MBObjectBase)_cachedCulture).StringId;
		}
		CompassElement.RefreshTargetIconType(val);
	}

	public void RefreshGold()
	{
		if (Peer != null && _gameMode.IsGameModeUsingGold)
		{
			MissionRepresentativeBase missionRepresentative = _missionRepresentative;
			FlagDominationMissionRepresentative val;
			if ((val = (FlagDominationMissionRepresentative)(object)((missionRepresentative is FlagDominationMissionRepresentative) ? missionRepresentative : null)) != null)
			{
				Gold = ((MissionRepresentativeBase)val).Gold;
				IsSpawnActive = Gold >= 100;
			}
		}
		else
		{
			IsSpawnActive = false;
		}
	}

	public void RefreshTeam()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		if (Peer != null)
		{
			string bannerCode = ((PeerComponent)Peer).Peer.BannerCode;
			Team team = Peer.Team;
			uint num = ((team != null) ? team.Color : 0);
			Team team2 = Peer.Team;
			Banner banner = new Banner(bannerCode, num, (team2 != null) ? team2.Color2 : 0u);
			MPTeammateCompassTargetVM compassElement = CompassElement;
			Team team3 = Peer.Team;
			compassElement.RefreshTeam(banner, team3 != null && team3.IsPlayerAlly);
			MPTeammateCompassTargetVM compassElement2 = CompassElement;
			Team team4 = Peer.Team;
			uint num2 = ((team4 != null) ? team4.Color : 0);
			Team team5 = Peer.Team;
			((CompassTargetVM)compassElement2).RefreshColor(num2, (team5 != null) ? team5.Color2 : 0u);
		}
	}

	public void RefreshProperties()
	{
		bool flag = MultiplayerOptionsExtensions.GetIntValue((OptionType)20, (MultiplayerOptionsAccessMode)1) > 0;
		MissionPeer peer = Peer;
		IsValueEnabled = (((peer != null) ? peer.Team : null) != null && Peer.Team == _playerTeam) || flag;
		if (IsValueEnabled)
		{
			if (flag)
			{
				ValuePercent = ((Peer.BotsUnderControlTotal != 0) ? ((int)((float)Peer.BotsUnderControlAlive / (float)Peer.BotsUnderControlTotal * 100f)) : 0);
			}
			else
			{
				ValuePercent = ((Peer.ControlledAgent != null) ? MathF.Ceiling(Peer.ControlledAgent.Health / Peer.ControlledAgent.HealthLimit * 100f) : 0);
			}
		}
	}

	public void RefreshPreview(BasicCharacterObject character, DynamicBodyProperties dynamicBodyProperties, bool isFemale)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Preview = new MPArmoryHeroPreviewVM();
		Preview.SetCharacter(character, dynamicBodyProperties, character.Race, isFemale);
	}

	public void RefreshActivePerks()
	{
		((Collection<MPPerkVM>)(object)ActivePerks).Clear();
		MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(Peer, false);
		if (Peer == null || Peer.Culture == null || mPHeroClassForPeer == null)
		{
			return;
		}
		foreach (MPPerkObject item in (List<MPPerkObject>)(object)Peer.SelectedPerks)
		{
			((Collection<MPPerkVM>)(object)ActivePerks).Add(new MPPerkVM(null, (IReadOnlyPerkObject)(object)item, isSelectable: false, 0));
		}
	}

	public void RefreshAvatar()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkMain.GameClient == null)
		{
			Debug.FailedAssert("Network is not enabled when trying to refresh avatars", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\MPPlayerVM.cs", "RefreshAvatar", 205);
			return;
		}
		if (Peer == null)
		{
			Debug.FailedAssert("Trying to refresh avatar of a player without peer!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\MPPlayerVM.cs", "RefreshAvatar", 211);
			return;
		}
		int num = (NetworkMain.GameClient.HasUserGeneratedContentPrivilege ? ((!BannerlordConfig.EnableGenericAvatars || _isKnownPlayer) ? (-1) : AvatarServices.GetForcedAvatarIndexOfPlayer(((PeerComponent)Peer).Peer.Id)) : AvatarServices.GetForcedAvatarIndexOfPlayer(((PeerComponent)Peer).Peer.Id));
		Avatar = new PlayerAvatarImageIdentifierVM(((PeerComponent)Peer).Peer.Id, num);
	}

	public void ExecuteFocusBegin()
	{
		SetFocusState(isFocused: true);
	}

	public void ExecuteFocusEnd()
	{
		SetFocusState(isFocused: false);
	}

	private void SetFocusState(bool isFocused)
	{
		uint value = (isFocused ? 4278255612u : 0u);
		if (Peer != null)
		{
			IAgentVisual agentVisualForPeer = Peer.GetAgentVisualForPeer(0);
			if (agentVisualForPeer != null)
			{
				agentVisualForPeer.GetCopyAgentVisualsData().AgentVisuals.SetContourColor((uint?)value, true);
			}
		}
		IsFocused = isFocused;
	}
}
