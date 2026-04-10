using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MPDeathCardVM : ViewModel
{
	private readonly TextObject _killedByStrayHorse = GameTexts.FindText("str_killed_by_stray_horse", (string)null);

	private readonly TextObject _killedSelfText = GameTexts.FindText("str_killed_self", (string)null);

	private readonly TextObject _killedByText = GameTexts.FindText("str_killed_by", (string)null);

	private readonly TextObject _enemyText = GameTexts.FindText("str_death_card_enemy", (string)null);

	private readonly TextObject _allyText = GameTexts.FindText("str_death_card_ally", (string)null);

	private bool _isActive;

	private bool _isSelfInflicted;

	private bool _killCountsEnabled;

	private int _numOfTimesPlayerKilled;

	private int _numOfTimesPlayerGotKilled;

	private string _titleText;

	private string _usedWeaponName;

	private string _killerName;

	private string _killerText;

	private string _youText;

	private MPPlayerVM _playerProperties;

	private int _bodyPartHit;

	[DataSourceProperty]
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				_isActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelfInflicted
	{
		get
		{
			return _isSelfInflicted;
		}
		set
		{
			if (value != _isSelfInflicted)
			{
				_isSelfInflicted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelfInflicted");
			}
		}
	}

	[DataSourceProperty]
	public bool KillCountsEnabled
	{
		get
		{
			return _killCountsEnabled;
		}
		set
		{
			if (value != _killCountsEnabled)
			{
				_killCountsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "KillCountsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string UsedWeaponName
	{
		get
		{
			return _usedWeaponName;
		}
		set
		{
			if (value != _usedWeaponName)
			{
				_usedWeaponName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "UsedWeaponName");
			}
		}
	}

	[DataSourceProperty]
	public string KillerName
	{
		get
		{
			return _killerName;
		}
		set
		{
			if (value != _killerName)
			{
				_killerName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "KillerName");
			}
		}
	}

	[DataSourceProperty]
	public string KillerText
	{
		get
		{
			return _killerText;
		}
		set
		{
			if (value != _killerText)
			{
				_killerText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "KillerText");
			}
		}
	}

	[DataSourceProperty]
	public string YouText
	{
		get
		{
			return _youText;
		}
		set
		{
			if (value != _youText)
			{
				_youText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "YouText");
			}
		}
	}

	[DataSourceProperty]
	public MPPlayerVM PlayerProperties
	{
		get
		{
			return _playerProperties;
		}
		set
		{
			if (value != _playerProperties)
			{
				_playerProperties = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPPlayerVM>(value, "PlayerProperties");
			}
		}
	}

	[DataSourceProperty]
	public int BodyPartHit
	{
		get
		{
			return _bodyPartHit;
		}
		set
		{
			if (value != _bodyPartHit)
			{
				_bodyPartHit = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "BodyPartHit");
			}
		}
	}

	[DataSourceProperty]
	public int NumOfTimesPlayerKilled
	{
		get
		{
			return _numOfTimesPlayerKilled;
		}
		set
		{
			if (value != _numOfTimesPlayerKilled)
			{
				_numOfTimesPlayerKilled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NumOfTimesPlayerKilled");
			}
		}
	}

	[DataSourceProperty]
	public int NumOfTimesPlayerGotKilled
	{
		get
		{
			return _numOfTimesPlayerGotKilled;
		}
		set
		{
			if (value != _numOfTimesPlayerGotKilled)
			{
				_numOfTimesPlayerGotKilled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "NumOfTimesPlayerGotKilled");
			}
		}
	}

	public MPDeathCardVM(MultiplayerGameType gameType)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Invalid comparison between Unknown and I4
		KillCountsEnabled = (int)gameType != 4;
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		YouText = ((object)GameTexts.FindText("str_death_card_you", (string)null)).ToString();
		Deactivate();
	}

	public void OnMainAgentRemoved(Agent affectorAgent, KillingBlow blow)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		ResetProperties();
		if (affectorAgent != null && affectorAgent == Agent.Main)
		{
			TitleText = ((object)_killedSelfText).ToString();
			IsSelfInflicted = true;
		}
		else if (affectorAgent != null && affectorAgent.IsMount && affectorAgent.RiderAgent == null)
		{
			_killedByStrayHorse.SetTextVariable("MOUNT_NAME", affectorAgent.NameTextObject);
			TitleText = ((object)_killedByStrayHorse).ToString();
			IsSelfInflicted = true;
		}
		else
		{
			IsSelfInflicted = false;
			TitleText = ((object)_killedByText).ToString();
		}
		Team obj = ((affectorAgent != null) ? affectorAgent.Team : null);
		Agent main = Agent.Main;
		KillerText = ((obj == ((main != null) ? main.Team : null)) ? ((object)_allyText).ToString() : ((object)_enemyText).ToString());
		if (IsSelfInflicted)
		{
			PlayerProperties = new MPPlayerVM(PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer));
			PlayerProperties.RefreshDivision();
		}
		else
		{
			KillerName = ((affectorAgent != null) ? affectorAgent.Name : null) ?? "";
			if (blow.WeaponItemKind >= 0)
			{
				UsedWeaponName = ((object)ItemObject.GetItemFromWeaponKind(blow.WeaponItemKind).Name).ToString();
			}
			else
			{
				UsedWeaponName = ((object)new TextObject("{=GAZ5QLZi}Unarmed", (Dictionary<string, object>)null)).ToString();
			}
			bool isServerOrRecorder = GameNetwork.IsServerOrRecorder;
			if (((affectorAgent != null) ? affectorAgent.MissionPeer : null) != null)
			{
				PlayerProperties = new MPPlayerVM(affectorAgent.MissionPeer);
				PlayerProperties.RefreshDivision();
				NumOfTimesPlayerKilled = Agent.Main.MissionPeer.GetNumberOfTimesPeerKilledPeer(affectorAgent.MissionPeer);
				NumOfTimesPlayerGotKilled = affectorAgent.MissionPeer.GetNumberOfTimesPeerKilledPeer(Agent.Main.MissionPeer) + ((!isServerOrRecorder) ? 1 : 0);
			}
			else if (((affectorAgent != null) ? affectorAgent.OwningAgentMissionPeer : null) != null)
			{
				PlayerProperties = new MPPlayerVM(affectorAgent.OwningAgentMissionPeer);
				PlayerProperties.RefreshDivision();
			}
			else
			{
				PlayerProperties = new MPPlayerVM(affectorAgent);
			}
		}
		IsActive = true;
	}

	private void ResetProperties()
	{
		IsActive = false;
		TitleText = "";
		UsedWeaponName = "";
		BodyPartHit = -1;
	}

	public void Deactivate()
	{
		IsActive = false;
	}
}
