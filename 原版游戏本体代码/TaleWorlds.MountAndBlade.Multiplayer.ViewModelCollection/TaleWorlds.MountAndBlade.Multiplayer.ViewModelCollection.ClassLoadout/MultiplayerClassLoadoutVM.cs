using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class MultiplayerClassLoadoutVM : ViewModel
{
	public const float UPDATE_INTERVAL = 1f;

	private float _updateTimeElapsed;

	private readonly Action<MPHeroClass> _onRefreshSelection;

	private readonly MissionMultiplayerGameModeBaseClient _missionMultiplayerGameMode;

	private Dictionary<MissionPeer, MPPlayerVM> _enemyDictionary;

	private readonly Mission _mission;

	private bool _isTeammateAndEnemiesRelevant;

	private const float REMAINING_TIME_WARNING_THRESHOLD = 5f;

	private MissionLobbyEquipmentNetworkComponent _missionLobbyEquipmentNetworkComponent;

	private bool _isInitializing;

	private Dictionary<MissionPeer, MPPlayerVM> _teammateDictionary;

	private int _gold;

	private string _culture;

	private string _cultureId;

	private string _spawnLabelText;

	private string _spawnForfeitLabelText;

	private string _remainingTimeText;

	private bool _warnRemainingTime;

	private bool _isSpawnTimerVisible;

	private bool _isSpawnLabelVisible;

	private bool _isSpawnForfeitLabelVisible;

	private bool _isGoldEnabled;

	private bool _isInWarmup;

	private bool _showAttackerOrDefenderIcons;

	private bool _isAttacker;

	private string _warmupInfoText;

	private Color _cultureColor1;

	private Color _cultureColor2;

	private MBBindingList<HeroClassGroupVM> _classes;

	private HeroInformationVM _heroInformation;

	private HeroClassVM _currentSelectedClass;

	private MBBindingList<MPPlayerVM> _teammates;

	private MBBindingList<MPPlayerVM> _enemies;

	private MissionRepresentativeBase missionRep
	{
		get
		{
			NetworkCommunicator myPeer = GameNetwork.MyPeer;
			if (myPeer == null)
			{
				return null;
			}
			VirtualPlayer virtualPlayer = myPeer.VirtualPlayer;
			if (virtualPlayer == null)
			{
				return null;
			}
			return virtualPlayer.GetComponent<MissionRepresentativeBase>();
		}
	}

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
	public string Culture
	{
		get
		{
			return _culture;
		}
		set
		{
			if (value != _culture)
			{
				_culture = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Culture");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor1)
			{
				_cultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor2)
			{
				_cultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor2");
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
	public bool IsSpawnTimerVisible
	{
		get
		{
			return _isSpawnTimerVisible;
		}
		set
		{
			if (value != _isSpawnTimerVisible)
			{
				_isSpawnTimerVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSpawnTimerVisible");
			}
		}
	}

	[DataSourceProperty]
	public string SpawnLabelText
	{
		get
		{
			return _spawnLabelText;
		}
		set
		{
			if (value != _spawnLabelText)
			{
				_spawnLabelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SpawnLabelText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSpawnLabelVisible
	{
		get
		{
			return _isSpawnLabelVisible;
		}
		set
		{
			if (value != _isSpawnLabelVisible)
			{
				_isSpawnLabelVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSpawnLabelVisible");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowAttackerOrDefenderIcons
	{
		get
		{
			return _showAttackerOrDefenderIcons;
		}
		set
		{
			if (value != _showAttackerOrDefenderIcons)
			{
				_showAttackerOrDefenderIcons = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowAttackerOrDefenderIcons");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAttacker
	{
		get
		{
			return _isAttacker;
		}
		set
		{
			if (value != _isAttacker)
			{
				_isAttacker = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAttacker");
			}
		}
	}

	[DataSourceProperty]
	public string SpawnForfeitLabelText
	{
		get
		{
			return _spawnForfeitLabelText;
		}
		set
		{
			if (value != _spawnForfeitLabelText)
			{
				_spawnForfeitLabelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SpawnForfeitLabelText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSpawnForfeitLabelVisible
	{
		get
		{
			return _isSpawnForfeitLabelVisible;
		}
		set
		{
			if (value != _isSpawnForfeitLabelVisible)
			{
				_isSpawnForfeitLabelVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSpawnForfeitLabelVisible");
			}
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
	public MBBindingList<MPPlayerVM> Teammates
	{
		get
		{
			return _teammates;
		}
		set
		{
			if (value != _teammates)
			{
				_teammates = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, "Teammates");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPPlayerVM> Enemies
	{
		get
		{
			return _enemies;
		}
		set
		{
			if (value != _enemies)
			{
				_enemies = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, "Enemies");
			}
		}
	}

	[DataSourceProperty]
	public HeroInformationVM HeroInformation
	{
		get
		{
			return _heroInformation;
		}
		set
		{
			if (value != _heroInformation)
			{
				_heroInformation = value;
				((ViewModel)this).OnPropertyChangedWithValue<HeroInformationVM>(value, "HeroInformation");
			}
		}
	}

	[DataSourceProperty]
	public HeroClassVM CurrentSelectedClass
	{
		get
		{
			return _currentSelectedClass;
		}
		set
		{
			if (value != _currentSelectedClass)
			{
				_currentSelectedClass = value;
				((ViewModel)this).OnPropertyChangedWithValue<HeroClassVM>(value, "CurrentSelectedClass");
			}
		}
	}

	[DataSourceProperty]
	public string RemainingTimeText
	{
		get
		{
			return _remainingTimeText;
		}
		set
		{
			if (value != _remainingTimeText)
			{
				_remainingTimeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "RemainingTimeText");
			}
		}
	}

	[DataSourceProperty]
	public bool WarnRemainingTime
	{
		get
		{
			return _warnRemainingTime;
		}
		set
		{
			if (value != _warnRemainingTime)
			{
				_warnRemainingTime = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WarnRemainingTime");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<HeroClassGroupVM> Classes
	{
		get
		{
			return _classes;
		}
		set
		{
			if (value != _classes)
			{
				_classes = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<HeroClassGroupVM>>(value, "Classes");
			}
		}
	}

	[DataSourceProperty]
	public bool IsGoldEnabled
	{
		get
		{
			return _isGoldEnabled;
		}
		set
		{
			if (value != _isGoldEnabled)
			{
				_isGoldEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsGoldEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInWarmup
	{
		get
		{
			return _isInWarmup;
		}
		set
		{
			if (value != _isInWarmup)
			{
				_isInWarmup = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInWarmup");
			}
		}
	}

	[DataSourceProperty]
	public string WarmupInfoText
	{
		get
		{
			return _warmupInfoText;
		}
		set
		{
			if (value != _warmupInfoText)
			{
				_warmupInfoText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "WarmupInfoText");
			}
		}
	}

	public MultiplayerClassLoadoutVM(MissionMultiplayerGameModeBaseClient gameMode, Action<MPHeroClass> onRefreshSelection, MPHeroClass initialHeroSelection)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Invalid comparison between Unknown and I4
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Expected O, but got Unknown
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected O, but got Unknown
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Invalid comparison between Unknown and I4
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Invalid comparison between Unknown and I4
		MBTextManager.SetTextVariable("newline", "\n", false);
		_isInitializing = true;
		_onRefreshSelection = onRefreshSelection;
		_missionMultiplayerGameMode = gameMode;
		_mission = ((MissionBehavior)gameMode).Mission;
		Team team = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Team;
		Classes = new MBBindingList<HeroClassGroupVM>();
		HeroInformation = new HeroInformationVM();
		_enemyDictionary = new Dictionary<MissionPeer, MPPlayerVM>();
		_missionLobbyEquipmentNetworkComponent = Mission.Current.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
		IsGoldEnabled = _missionMultiplayerGameMode.IsGameModeUsingGold;
		if (IsGoldEnabled)
		{
			Gold = _missionMultiplayerGameMode.GetGoldAmount();
		}
		MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
		BasicCultureObject obj = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1));
		BasicCultureObject val = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		MultiplayerBattleColors val2 = MultiplayerBattleColors.CreateWith(obj, val);
		MultiplayerCultureColorInfo peerColors = ((MultiplayerBattleColors)(ref val2)).GetPeerColors(component);
		HeroClassVM heroClassVM = null;
		foreach (MPHeroClassGroup multiplayerHeroClassGroup in MultiplayerClassDivisions.MultiplayerHeroClassGroups)
		{
			HeroClassGroupVM heroClassGroupVM = new HeroClassGroupVM(RefreshCharacter, OnSelectPerk, multiplayerHeroClassGroup, peerColors);
			if (heroClassGroupVM.IsValid)
			{
				((Collection<HeroClassGroupVM>)(object)Classes).Add(heroClassGroupVM);
			}
		}
		int num = ((initialHeroSelection != null) ? (gameMode.IsGameModeUsingCasualGold ? initialHeroSelection.TroopCasualCost : (((int)gameMode.GameType == 3) ? initialHeroSelection.TroopBattleCost : initialHeroSelection.TroopCost)) : 0);
		if (initialHeroSelection == null || (IsGoldEnabled && num > Gold))
		{
			heroClassVM = ((IEnumerable<HeroClassVM>)((IEnumerable<HeroClassGroupVM>)Classes).FirstOrDefault()?.SubClasses).FirstOrDefault();
		}
		else
		{
			foreach (HeroClassGroupVM item in (Collection<HeroClassGroupVM>)(object)Classes)
			{
				foreach (HeroClassVM item2 in (Collection<HeroClassVM>)(object)item.SubClasses)
				{
					if (item2.HeroClass == initialHeroSelection)
					{
						heroClassVM = item2;
						break;
					}
				}
				if (heroClassVM != null)
				{
					break;
				}
			}
			if (heroClassVM == null)
			{
				heroClassVM = ((IEnumerable<HeroClassVM>)((IEnumerable<HeroClassGroupVM>)Classes).FirstOrDefault()?.SubClasses).FirstOrDefault();
			}
		}
		_isInitializing = false;
		RefreshCharacter(heroClassVM);
		_teammateDictionary = new Dictionary<MissionPeer, MPPlayerVM>();
		Teammates = new MBBindingList<MPPlayerVM>();
		Enemies = new MBBindingList<MPPlayerVM>();
		MissionPeer.OnEquipmentIndexRefreshed += new OnUpdateEquipmentSetIndexEventDelegate(RefreshPeerDivision);
		MissionPeer.OnPerkSelectionUpdated += new OnPerkUpdateEventDelegate(RefreshPeerPerkSelection);
		NetworkCommunicator.OnPeerComponentAdded += OnPeerComponentAdded;
		BasicCultureObject culture = component.Culture;
		CultureId = ((MBObjectBase)culture).StringId;
		CultureColor1 = peerColors.Color1;
		CultureColor2 = peerColors.Color2;
		if (Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>())
		{
			ShowAttackerOrDefenderIcons = true;
			IsAttacker = (int)team.Side == 1;
		}
		((ViewModel)this).RefreshValues();
		_isTeammateAndEnemiesRelevant = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().IsGameModeTactical && !Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>() && (int)Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().GameType != 3;
		if (_isTeammateAndEnemiesRelevant)
		{
			OnRefreshTeamMembers();
			OnRefreshEnemyMembers();
		}
	}

	public override void RefreshValues()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		UpdateSpawnAndTimerLabels();
		string strValue = MultiplayerOptionsExtensions.GetStrValue((OptionType)11, (MultiplayerOptionsAccessMode)1);
		TextObject val = new TextObject("{=XJTX8w8M}Warmup Phase - {GAME_MODE}{newline}Waiting for players to join", (Dictionary<string, object>)null);
		val.SetTextVariable("GAME_MODE", GameTexts.FindText("str_multiplayer_official_game_type_name", strValue));
		WarmupInfoText = ((object)val).ToString();
		BasicCultureObject culture = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Culture;
		Culture = ((object)culture.Name).ToString();
		Classes.ApplyActionOnAllItems((Action<HeroClassGroupVM>)delegate(HeroClassGroupVM x)
		{
			((ViewModel)x).RefreshValues();
		});
		((ViewModel)CurrentSelectedClass).RefreshValues();
		((ViewModel)HeroInformation).RefreshValues();
	}

	private void UpdateSpawnAndTimerLabels()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f);
		GameTexts.SetVariable("USE_KEY", keyHyperlinkText);
		SpawnLabelText = ((object)GameTexts.FindText("str_skirmish_battle_press_action_to_spawn", (string)null)).ToString();
		if (_missionMultiplayerGameMode.RoundComponent != null)
		{
			if (_missionMultiplayerGameMode.IsInWarmup || _missionMultiplayerGameMode.IsRoundInProgress)
			{
				IsSpawnTimerVisible = false;
				IsSpawnLabelVisible = true;
				if (_missionMultiplayerGameMode.IsRoundInProgress && _missionMultiplayerGameMode is MissionMultiplayerGameModeFlagDominationClient && (int)_missionMultiplayerGameMode.GameType == 5 && PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer) != null)
				{
					IsSpawnForfeitLabelVisible = true;
					string keyHyperlinkText2 = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", "ForfeitSpawn"), 1f);
					GameTexts.SetVariable("ALT_WEAP_KEY", keyHyperlinkText2);
					SpawnForfeitLabelText = ((object)GameTexts.FindText("str_skirmish_battle_press_alternative_to_forfeit_spawning", (string)null)).ToString();
				}
			}
			else
			{
				IsSpawnTimerVisible = true;
			}
		}
		else
		{
			IsSpawnTimerVisible = false;
			IsSpawnLabelVisible = true;
		}
	}

	public override void OnFinalize()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		((ViewModel)this).OnFinalize();
		MissionPeer.OnEquipmentIndexRefreshed -= new OnUpdateEquipmentSetIndexEventDelegate(RefreshPeerDivision);
		MissionPeer.OnPerkSelectionUpdated -= new OnPerkUpdateEventDelegate(RefreshPeerPerkSelection);
		NetworkCommunicator.OnPeerComponentAdded -= OnPeerComponentAdded;
	}

	private void RefreshCharacter(HeroClassVM heroClass)
	{
		if (_isInitializing)
		{
			return;
		}
		foreach (HeroClassGroupVM item in (Collection<HeroClassGroupVM>)(object)Classes)
		{
			foreach (HeroClassVM item2 in (Collection<HeroClassVM>)(object)item.SubClasses)
			{
				item2.IsSelected = false;
			}
		}
		heroClass.IsSelected = true;
		CurrentSelectedClass = heroClass;
		if (GameNetwork.IsMyPeerReady)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			int nextSelectedTroopIndex = MultiplayerClassDivisions.GetMPHeroClasses(heroClass.HeroClass.Culture).ToList().IndexOf(heroClass.HeroClass);
			component.NextSelectedTroopIndex = nextSelectedTroopIndex;
		}
		HeroInformation.RefreshWith(heroClass.HeroClass, heroClass.SelectedPerks);
		_missionLobbyEquipmentNetworkComponent.EquipmentUpdated();
		if (_missionMultiplayerGameMode.IsGameModeUsingGold)
		{
			Gold = _missionMultiplayerGameMode.GetGoldAmount();
		}
		List<IReadOnlyPerkObject> perks = ((IEnumerable<HeroPerkVM>)heroClass.Perks).Select((HeroPerkVM x) => x.SelectedPerk).ToList();
		HeroInformation.RefreshWith(HeroInformation.HeroClass, perks);
		List<Tuple<HeroPerkVM, MPPerkVM>> list = new List<Tuple<HeroPerkVM, MPPerkVM>>();
		foreach (HeroPerkVM item3 in (Collection<HeroPerkVM>)(object)heroClass.Perks)
		{
			list.Add(new Tuple<HeroPerkVM, MPPerkVM>(item3, item3.SelectedPerkItem));
		}
		list.ForEach(delegate(Tuple<HeroPerkVM, MPPerkVM> p)
		{
			OnSelectPerk(p.Item1, p.Item2);
		});
		_onRefreshSelection?.Invoke(heroClass.HeroClass);
	}

	private void OnSelectPerk(HeroPerkVM heroPerk, MPPerkVM candidate)
	{
		if (GameNetwork.IsMyPeerReady && HeroInformation.HeroClass != null && CurrentSelectedClass != null)
		{
			MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
			if (!GameNetwork.IsServer || component.SelectPerk(heroPerk.PerkIndex, candidate.PerkIndex, -1))
			{
				_missionLobbyEquipmentNetworkComponent.PerkUpdated(heroPerk.PerkIndex, candidate.PerkIndex);
			}
			List<IReadOnlyPerkObject> list = ((IEnumerable<HeroPerkVM>)CurrentSelectedClass.Perks).Select((HeroPerkVM x) => x.SelectedPerk).ToList();
			if (list.Count > 0)
			{
				HeroInformation.RefreshWith(HeroInformation.HeroClass, list);
			}
		}
	}

	public void RefreshPeerDivision(MissionPeer peer, int divisionType)
	{
		((IEnumerable<MPPlayerVM>)Teammates).FirstOrDefault((MPPlayerVM t) => t.Peer == peer)?.RefreshDivision();
	}

	private void RefreshPeerPerkSelection(MissionPeer peer)
	{
		((IEnumerable<MPPlayerVM>)Teammates).FirstOrDefault((MPPlayerVM t) => t.Peer == peer)?.RefreshActivePerks();
	}

	public void Tick(float dt)
	{
		if (_missionMultiplayerGameMode != null)
		{
			IsInWarmup = _missionMultiplayerGameMode.IsInWarmup;
			IsGoldEnabled = !IsInWarmup && _missionMultiplayerGameMode.IsGameModeUsingGold;
			if (IsGoldEnabled)
			{
				Gold = _missionMultiplayerGameMode.GetGoldAmount();
			}
			foreach (HeroClassGroupVM item in (Collection<HeroClassGroupVM>)(object)Classes)
			{
				foreach (HeroClassVM item2 in (Collection<HeroClassVM>)(object)item.SubClasses)
				{
					item2.IsGoldEnabled = IsGoldEnabled;
				}
			}
		}
		RefreshRemainingTime();
		_updateTimeElapsed += dt;
		if (!(_updateTimeElapsed < 1f))
		{
			_updateTimeElapsed = 0f;
			if (_isTeammateAndEnemiesRelevant)
			{
				OnRefreshTeamMembers();
				OnRefreshEnemyMembers();
			}
		}
	}

	private void OnPeerComponentAdded(PeerComponent component)
	{
		if (component.IsMine && component is MissionRepresentativeBase)
		{
			_isTeammateAndEnemiesRelevant = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().IsGameModeTactical && !Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>();
			if (_isTeammateAndEnemiesRelevant)
			{
				OnRefreshTeamMembers();
				OnRefreshEnemyMembers();
			}
		}
	}

	private void OnRefreshTeamMembers()
	{
		List<MPPlayerVM> list = ((IEnumerable<MPPlayerVM>)Teammates).ToList();
		foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
		{
			if (PeerExtensions.GetComponent<MissionPeer>(PeerExtensions.GetNetworkPeer((PeerComponent)(object)item)) != null && _playerTeam != null && item.Team == _playerTeam)
			{
				if (!_teammateDictionary.ContainsKey(item))
				{
					MPPlayerVM mPPlayerVM = new MPPlayerVM(item);
					((Collection<MPPlayerVM>)(object)Teammates).Add(mPPlayerVM);
					_teammateDictionary.Add(item, mPPlayerVM);
				}
				else
				{
					list.Remove(_teammateDictionary[item]);
				}
			}
		}
		foreach (MPPlayerVM item2 in list)
		{
			((Collection<MPPlayerVM>)(object)Teammates).Remove(item2);
			_teammateDictionary.Remove(item2.Peer);
		}
		foreach (MPPlayerVM item3 in (Collection<MPPlayerVM>)(object)Teammates)
		{
			if (item3.CompassElement == null)
			{
				item3.RefreshDivision();
			}
		}
	}

	private void OnRefreshEnemyMembers()
	{
		List<MPPlayerVM> list = ((IEnumerable<MPPlayerVM>)Enemies).ToList();
		foreach (MissionPeer item in VirtualPlayer.Peers<MissionPeer>())
		{
			if (PeerExtensions.GetComponent<MissionPeer>(PeerExtensions.GetNetworkPeer((PeerComponent)(object)item)) != null && _playerTeam != null && item.Team != null && item.Team != _playerTeam && item.Team != Mission.Current.SpectatorTeam)
			{
				if (!_enemyDictionary.ContainsKey(item))
				{
					MPPlayerVM mPPlayerVM = new MPPlayerVM(item);
					((Collection<MPPlayerVM>)(object)Enemies).Add(mPPlayerVM);
					_enemyDictionary.Add(item, mPPlayerVM);
				}
				else
				{
					list.Remove(_enemyDictionary[item]);
				}
			}
		}
		foreach (MPPlayerVM item2 in list)
		{
			((Collection<MPPlayerVM>)(object)Enemies).Remove(item2);
			_enemyDictionary.Remove(item2.Peer);
		}
		foreach (MPPlayerVM item3 in (Collection<MPPlayerVM>)(object)Enemies)
		{
			item3.RefreshDivision();
			item3.UpdateDisabled();
		}
	}

	public void OnPeerEquipmentRefreshed(MissionPeer peer)
	{
		if (_teammateDictionary.ContainsKey(peer))
		{
			_teammateDictionary[peer].RefreshActivePerks();
		}
		else if (_enemyDictionary.ContainsKey(peer))
		{
			_enemyDictionary[peer].RefreshActivePerks();
		}
	}

	public void OnGoldUpdated()
	{
		foreach (HeroClassGroupVM item in (Collection<HeroClassGroupVM>)(object)Classes)
		{
			item.SubClasses.ApplyActionOnAllItems((Action<HeroClassVM>)delegate(HeroClassVM sc)
			{
				sc.UpdateEnabled();
			});
		}
	}

	public void RefreshRemainingTime()
	{
		int num = MathF.Ceiling(_missionMultiplayerGameMode.RemainingTime);
		RemainingTimeText = TimeSpan.FromSeconds(num).ToString("mm':'ss");
		WarnRemainingTime = (float)num < 5f;
	}
}
