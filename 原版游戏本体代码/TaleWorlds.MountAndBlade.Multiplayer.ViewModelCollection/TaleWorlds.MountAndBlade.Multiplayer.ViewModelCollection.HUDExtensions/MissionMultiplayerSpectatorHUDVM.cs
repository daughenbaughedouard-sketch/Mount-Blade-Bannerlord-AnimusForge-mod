using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.HUDExtensions;

public class MissionMultiplayerSpectatorHUDVM : ViewModel
{
	private readonly Mission _mission;

	private readonly bool _isTeamsEnabled;

	private readonly bool _isFlagDominationMode;

	private Agent _spectatedAgent;

	private string _spectatedPlayerName;

	private string _takeControlText;

	private int _spectatedPlayerNeutrality = -1;

	private bool _isSpectatingPlayer;

	private bool _canTakeControlOfSpectatedAgent;

	private bool _agentHasMount;

	private bool _agentHasShield;

	private bool _showAgentHealth;

	private bool _agentHasRangedWeapon;

	private bool _agentHasCompassElement;

	private float _spectatedPlayerHealthLimit;

	private float _spectatedPlayerCurrentHealth;

	private float _spectatedPlayerMountCurrentHealth;

	private float _spectatedPlayerMountHealthLimit;

	private float _spectatedPlayerShieldCurrentHealth;

	private float _spectatedPlayerShieldHealthLimit;

	private int _spectatedPlayerAmmoAmount;

	private MPTeammateCompassTargetVM _compassElement;

	[DataSourceProperty]
	public int SpectatedPlayerNeutrality
	{
		get
		{
			return _spectatedPlayerNeutrality;
		}
		set
		{
			if (value != _spectatedPlayerNeutrality)
			{
				_spectatedPlayerNeutrality = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerNeutrality");
				IsSpectatingAgent = value >= 0;
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
	public bool IsSpectatingAgent
	{
		get
		{
			return _isSpectatingPlayer;
		}
		set
		{
			if (value != _isSpectatingPlayer)
			{
				_isSpectatingPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSpectatingAgent");
			}
		}
	}

	[DataSourceProperty]
	public bool AgentHasCompassElement
	{
		get
		{
			return _agentHasCompassElement;
		}
		set
		{
			if (value != _agentHasCompassElement)
			{
				_agentHasCompassElement = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AgentHasCompassElement");
			}
		}
	}

	[DataSourceProperty]
	public bool AgentHasMount
	{
		get
		{
			return _agentHasMount;
		}
		set
		{
			if (value != _agentHasMount)
			{
				_agentHasMount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AgentHasMount");
			}
		}
	}

	[DataSourceProperty]
	public bool ShowAgentHealth
	{
		get
		{
			return _showAgentHealth;
		}
		set
		{
			if (value != _showAgentHealth)
			{
				_showAgentHealth = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShowAgentHealth");
			}
		}
	}

	[DataSourceProperty]
	public bool AgentHasRangedWeapon
	{
		get
		{
			return _agentHasRangedWeapon;
		}
		set
		{
			if (value != _agentHasRangedWeapon)
			{
				_agentHasRangedWeapon = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AgentHasRangedWeapon");
			}
		}
	}

	[DataSourceProperty]
	public bool AgentHasShield
	{
		get
		{
			return _agentHasShield;
		}
		set
		{
			if (value != _agentHasShield)
			{
				_agentHasShield = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AgentHasShield");
			}
		}
	}

	[DataSourceProperty]
	public bool CanTakeControlOfSpectatedAgent
	{
		get
		{
			return _canTakeControlOfSpectatedAgent;
		}
		set
		{
			if (value != _canTakeControlOfSpectatedAgent)
			{
				_canTakeControlOfSpectatedAgent = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanTakeControlOfSpectatedAgent");
			}
		}
	}

	[DataSourceProperty]
	public string SpectatedPlayerName
	{
		get
		{
			return _spectatedPlayerName;
		}
		set
		{
			if (value != _spectatedPlayerName)
			{
				_spectatedPlayerName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SpectatedPlayerName");
			}
		}
	}

	[DataSourceProperty]
	public string TakeControlText
	{
		get
		{
			return _takeControlText;
		}
		set
		{
			if (value != _takeControlText)
			{
				_takeControlText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TakeControlText");
			}
		}
	}

	[DataSourceProperty]
	public float SpectatedPlayerHealthLimit
	{
		get
		{
			return _spectatedPlayerHealthLimit;
		}
		set
		{
			if (value != _spectatedPlayerHealthLimit)
			{
				_spectatedPlayerHealthLimit = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerHealthLimit");
			}
		}
	}

	[DataSourceProperty]
	public float SpectatedPlayerCurrentHealth
	{
		get
		{
			return _spectatedPlayerCurrentHealth;
		}
		set
		{
			if (value != _spectatedPlayerCurrentHealth)
			{
				_spectatedPlayerCurrentHealth = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerCurrentHealth");
			}
		}
	}

	[DataSourceProperty]
	public float SpectatedPlayerMountCurrentHealth
	{
		get
		{
			return _spectatedPlayerMountCurrentHealth;
		}
		set
		{
			if (value != _spectatedPlayerMountCurrentHealth)
			{
				_spectatedPlayerMountCurrentHealth = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerMountCurrentHealth");
			}
		}
	}

	[DataSourceProperty]
	public float SpectatedPlayerMountHealthLimit
	{
		get
		{
			return _spectatedPlayerMountHealthLimit;
		}
		set
		{
			if (value != _spectatedPlayerMountHealthLimit)
			{
				_spectatedPlayerMountHealthLimit = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerMountHealthLimit");
			}
		}
	}

	[DataSourceProperty]
	public float SpectatedPlayerShieldCurrentHealth
	{
		get
		{
			return _spectatedPlayerShieldCurrentHealth;
		}
		set
		{
			if (value != _spectatedPlayerShieldCurrentHealth)
			{
				_spectatedPlayerShieldCurrentHealth = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerShieldCurrentHealth");
			}
		}
	}

	[DataSourceProperty]
	public float SpectatedPlayerShieldHealthLimit
	{
		get
		{
			return _spectatedPlayerShieldHealthLimit;
		}
		set
		{
			if (value != _spectatedPlayerShieldHealthLimit)
			{
				_spectatedPlayerShieldHealthLimit = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerShieldHealthLimit");
			}
		}
	}

	[DataSourceProperty]
	public int SpectatedPlayerAmmoAmount
	{
		get
		{
			return _spectatedPlayerAmmoAmount;
		}
		set
		{
			if (value != _spectatedPlayerAmmoAmount)
			{
				_spectatedPlayerAmmoAmount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SpectatedPlayerAmmoAmount");
			}
		}
	}

	public MissionMultiplayerSpectatorHUDVM(Mission mission)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		_mission = mission;
		MissionLobbyComponent missionBehavior = mission.GetMissionBehavior<MissionLobbyComponent>();
		_isTeamsEnabled = (int)missionBehavior.MissionType != 1;
		_isFlagDominationMode = Mission.Current.HasMissionBehavior<MissionMultiplayerGameModeFlagDominationClient>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f);
		GameTexts.SetVariable("USE_KEY", keyHyperlinkText);
		TakeControlText = ((object)GameTexts.FindText("str_sergeant_battle_press_action_to_control_bot_2", (string)null)).ToString();
	}

	public void Tick(float dt)
	{
		if (_mission.MainAgent != null)
		{
			SpectatedPlayerNeutrality = -1;
		}
		UpdateDynamicProperties();
	}

	private void UpdateDynamicProperties()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Invalid comparison between Unknown and I4
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Invalid comparison between Unknown and I4
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Invalid comparison between Unknown and I4
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		AgentHasShield = false;
		AgentHasMount = false;
		ShowAgentHealth = false;
		AgentHasRangedWeapon = false;
		if (SpectatedPlayerNeutrality <= 0 || _spectatedAgent == null)
		{
			return;
		}
		ShowAgentHealth = true;
		SpectatedPlayerHealthLimit = _spectatedAgent.HealthLimit;
		SpectatedPlayerCurrentHealth = _spectatedAgent.Health;
		AgentHasMount = _spectatedAgent.MountAgent != null;
		if (AgentHasMount)
		{
			SpectatedPlayerMountCurrentHealth = _spectatedAgent.MountAgent.Health;
			SpectatedPlayerMountHealthLimit = _spectatedAgent.MountAgent.HealthLimit;
		}
		EquipmentIndex primaryWieldedItemIndex = _spectatedAgent.GetPrimaryWieldedItemIndex();
		EquipmentIndex offhandWieldedItemIndex = _spectatedAgent.GetOffhandWieldedItemIndex();
		int num = -1;
		MissionWeapon val;
		if ((int)primaryWieldedItemIndex != -1)
		{
			val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
			if (((MissionWeapon)(ref val)).CurrentUsageItem != null)
			{
				val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
				if (((MissionWeapon)(ref val)).CurrentUsageItem.IsRangedWeapon)
				{
					val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
					if (((MissionWeapon)(ref val)).CurrentUsageItem.IsConsumable)
					{
						int ammoAmount = _spectatedAgent.Equipment.GetAmmoAmount(primaryWieldedItemIndex);
						val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
						if (((MissionWeapon)(ref val)).ModifiedMaxAmount == 1 || ammoAmount > 0)
						{
							val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
							num = ((((MissionWeapon)(ref val)).ModifiedMaxAmount == 1) ? (-1) : ammoAmount);
						}
						goto IL_01f5;
					}
				}
				val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
				if (((MissionWeapon)(ref val)).CurrentUsageItem.IsRangedWeapon)
				{
					val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
					bool flag = (int)((MissionWeapon)(ref val)).CurrentUsageItem.WeaponClass == 17;
					int ammoAmount2 = _spectatedAgent.Equipment.GetAmmoAmount(primaryWieldedItemIndex);
					int num2;
					if (!flag)
					{
						num2 = 0;
					}
					else
					{
						val = _spectatedAgent.Equipment[primaryWieldedItemIndex];
						num2 = ((MissionWeapon)(ref val)).Ammo;
					}
					num = ammoAmount2 + num2;
				}
			}
		}
		goto IL_01f5;
		IL_01f5:
		if ((int)offhandWieldedItemIndex != -1)
		{
			val = _spectatedAgent.Equipment[offhandWieldedItemIndex];
			if (((MissionWeapon)(ref val)).CurrentUsageItem != null)
			{
				MissionWeapon val2 = _spectatedAgent.Equipment[offhandWieldedItemIndex];
				AgentHasShield = ((MissionWeapon)(ref val2)).CurrentUsageItem.IsShield;
				if (AgentHasShield)
				{
					SpectatedPlayerShieldHealthLimit = ((MissionWeapon)(ref val2)).ModifiedMaxHitPoints;
					SpectatedPlayerShieldCurrentHealth = ((MissionWeapon)(ref val2)).HitPoints;
				}
			}
		}
		AgentHasRangedWeapon = num >= 0;
		SpectatedPlayerAmmoAmount = num;
	}

	internal void OnSpectatedAgentFocusIn(Agent followedAgent)
	{
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		_spectatedAgent = followedAgent;
		int spectatedPlayerNeutrality = 0;
		MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
		if (component != null && component.Team != _mission.SpectatorTeam && component.Team == followedAgent.Team && _isTeamsEnabled)
		{
			spectatedPlayerNeutrality = 1;
		}
		SpectatedPlayerNeutrality = spectatedPlayerNeutrality;
		MissionPeer missionPeer = followedAgent.MissionPeer;
		SpectatedPlayerName = ((missionPeer != null) ? missionPeer.DisplayedName : null) ?? followedAgent.Name.ToString();
		CanTakeControlOfSpectatedAgent = _isFlagDominationMode && ((component != null) ? component.ControlledFormation : null) != null && component.ControlledFormation == followedAgent.Formation;
		CompassElement = null;
		AgentHasCompassElement = false;
		object obj = followedAgent.MissionPeer;
		if (obj == null)
		{
			Formation formation = followedAgent.Formation;
			if (formation == null)
			{
				obj = null;
			}
			else
			{
				Agent playerOwner = formation.PlayerOwner;
				obj = ((playerOwner != null) ? playerOwner.MissionPeer : null);
			}
		}
		MissionPeer val = (MissionPeer)obj;
		if (val != null)
		{
			MPHeroClass mPHeroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(val, false);
			TargetIconType iconType = (TargetIconType)((mPHeroClassForPeer == null) ? (-1) : ((int)mPHeroClassForPeer.IconType));
			Banner banner = new Banner(((PeerComponent)val).Peer.BannerCode, val.Team.Color, val.Team.Color2);
			CompassElement = new MPTeammateCompassTargetVM(iconType, val.Team.Color, val.Team.Color2, banner, val.Team.IsPlayerAlly);
			AgentHasCompassElement = true;
		}
	}

	internal void OnSpectatedAgentFocusOut(Agent followedPeer)
	{
		_spectatedAgent = null;
		SpectatedPlayerNeutrality = -1;
	}
}
