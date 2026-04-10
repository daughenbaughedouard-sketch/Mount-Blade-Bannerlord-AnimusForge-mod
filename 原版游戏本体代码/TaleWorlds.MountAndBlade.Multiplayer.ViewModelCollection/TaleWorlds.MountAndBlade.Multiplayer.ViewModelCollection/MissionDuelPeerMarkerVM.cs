using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MissionDuelPeerMarkerVM : ViewModel
{
	private float _currentDuelRequestTimeRemaining;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private float _wPosAfterPositionCalculation;

	private TextObject _acceptDuelRequestText;

	private TextObject _sendDuelRequestText;

	private TextObject _waitingForDuelResponseText;

	private bool _isEnabled;

	private bool _isTracked;

	private bool _shouldShowInformation;

	private bool _isAgentInScreenBoundaries;

	private bool _isFocused;

	private bool _hasDuelRequestForPlayer;

	private bool _hasSentDuelRequest;

	private string _name;

	private string _actionDescriptionText;

	private int _bounty;

	private int _preferredArenaType;

	private int _wSign;

	private Vec2 _screenPosition;

	private MPTeammateCompassTargetVM _compassElement;

	private MBBindingList<MPPerkVM> _selectedPerks;

	public MissionPeer TargetPeer { get; private set; }

	public float Distance { get; private set; }

	public bool IsInDuel { get; private set; }

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
				UpdateTracked();
			}
		}
	}

	[DataSourceProperty]
	public bool IsTracked
	{
		get
		{
			return _isTracked;
		}
		set
		{
			if (value != _isTracked)
			{
				_isTracked = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTracked");
			}
		}
	}

	[DataSourceProperty]
	public bool ShouldShowInformation
	{
		get
		{
			return _shouldShowInformation;
		}
		set
		{
			if (value != _shouldShowInformation)
			{
				_shouldShowInformation = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ShouldShowInformation");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAgentInScreenBoundaries
	{
		get
		{
			return _isAgentInScreenBoundaries;
		}
		set
		{
			if (value != _isAgentInScreenBoundaries)
			{
				_isAgentInScreenBoundaries = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAgentInScreenBoundaries");
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
				SetFocused(value);
				UpdateTracked();
			}
		}
	}

	[DataSourceProperty]
	public bool HasDuelRequestForPlayer
	{
		get
		{
			return _hasDuelRequestForPlayer;
		}
		set
		{
			if (value != _hasDuelRequestForPlayer)
			{
				_hasDuelRequestForPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasDuelRequestForPlayer");
				OnInteractionChanged();
				UpdateTracked();
			}
		}
	}

	[DataSourceProperty]
	public bool HasSentDuelRequest
	{
		get
		{
			return _hasSentDuelRequest;
		}
		set
		{
			if (value != _hasSentDuelRequest)
			{
				_hasSentDuelRequest = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasSentDuelRequest");
				OnInteractionChanged();
				UpdateTracked();
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
	public string ActionDescriptionText
	{
		get
		{
			return _actionDescriptionText;
		}
		set
		{
			if (value != _actionDescriptionText)
			{
				_actionDescriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ActionDescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public int Bounty
	{
		get
		{
			return _bounty;
		}
		set
		{
			if (value != _bounty)
			{
				_bounty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Bounty");
			}
		}
	}

	[DataSourceProperty]
	public int PreferredArenaType
	{
		get
		{
			return _preferredArenaType;
		}
		set
		{
			if (value != _preferredArenaType)
			{
				_preferredArenaType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PreferredArenaType");
			}
		}
	}

	[DataSourceProperty]
	public int WSign
	{
		get
		{
			return _wSign;
		}
		set
		{
			if (value != _wSign)
			{
				_wSign = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "WSign");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 ScreenPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _screenPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (value.x != _screenPosition.x || value.y != _screenPosition.y)
			{
				_screenPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ScreenPosition");
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
	public MBBindingList<MPPerkVM> SelectedPerks
	{
		get
		{
			return _selectedPerks;
		}
		set
		{
			if (value != _selectedPerks)
			{
				_selectedPerks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPerkVM>>(value, "SelectedPerks");
			}
		}
	}

	public MissionDuelPeerMarkerVM(MissionPeer peer)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		TargetPeer = peer;
		MissionRepresentativeBase representative = peer.Representative;
		Bounty = ((DuelMissionRepresentative)((representative is DuelMissionRepresentative) ? representative : null)).Bounty;
		IsEnabled = true;
		TargetIconType iconType = MultiplayerClassDivisions.GetMPHeroClassForPeer(TargetPeer, false).IconType;
		Color white = Color.White;
		uint color = ((Color)(ref white)).ToUnsignedInteger();
		white = Color.White;
		CompassElement = new MPTeammateCompassTargetVM(iconType, color, ((Color)(ref white)).ToUnsignedInteger(), new Banner(), isAlly: true);
		SelectedPerks = new MBBindingList<MPPerkVM>();
		RefreshPerkSelection();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Name = TargetPeer.DisplayedName;
		_acceptDuelRequestText = new TextObject("{=tidE1V1k}Accept duel", (Dictionary<string, object>)null);
		_sendDuelRequestText = new TextObject("{=YLPJWgqF}Challenge", (Dictionary<string, object>)null);
		_waitingForDuelResponseText = new TextObject("{=MPgnsZoo}Waiting for response", (Dictionary<string, object>)null);
	}

	public void OnTick(float dt)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		if (Agent.Main != null && TargetPeer.ControlledAgent != null)
		{
			Distance = _latestW;
		}
		if (HasSentDuelRequest)
		{
			_currentDuelRequestTimeRemaining -= dt;
			GameTexts.SetVariable("SECONDS", (int)_currentDuelRequestTimeRemaining);
			GameTexts.SetVariable("ACTION", _waitingForDuelResponseText);
			ActionDescriptionText = ((object)new TextObject("{=HXWpxvgT}{ACTION} ({SECONDS})", (Dictionary<string, object>)null)).ToString();
			if (_currentDuelRequestTimeRemaining <= 0f)
			{
				HasSentDuelRequest = false;
			}
		}
	}

	public void UpdateScreenPosition(Camera missionCamera)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (TargetPeer.ControlledAgent != null)
		{
			WorldPosition worldPosition = TargetPeer.ControlledAgent.GetWorldPosition();
			Vec3 groundVec = ((WorldPosition)(ref worldPosition)).GetGroundVec3();
			groundVec += new Vec3(0f, 0f, TargetPeer.ControlledAgent.GetEyeGlobalHeight(), -1f);
			_latestX = 0f;
			_latestY = 0f;
			_latestW = 0f;
			MBWindowManager.WorldToScreen(missionCamera, groundVec, ref _latestX, ref _latestY, ref _latestW);
			ScreenPosition = new Vec2(_latestX, _latestY);
			IsAgentInScreenBoundaries = !(_latestX > Screen.RealScreenResolutionWidth) && !(_latestY > Screen.RealScreenResolutionHeight) && !(_latestX + 200f < 0f) && !(_latestY + 100f < 0f);
			_wPosAfterPositionCalculation = ((_latestW < 0f) ? (-1f) : 1.1f);
			WSign = (int)_wPosAfterPositionCalculation;
		}
	}

	private void OnInteractionChanged()
	{
		ActionDescriptionText = "";
		if (HasDuelRequestForPlayer)
		{
			string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f);
			GameTexts.SetVariable("KEY", keyHyperlinkText);
			GameTexts.SetVariable("ACTION", _acceptDuelRequestText);
			ActionDescriptionText = ((object)GameTexts.FindText("str_key_action", (string)null)).ToString();
		}
		else if (HasSentDuelRequest)
		{
			_currentDuelRequestTimeRemaining = 10f;
		}
	}

	private void SetFocused(bool isFocused)
	{
		if (!HasDuelRequestForPlayer && !HasSentDuelRequest)
		{
			if (isFocused)
			{
				string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f);
				GameTexts.SetVariable("KEY", keyHyperlinkText);
				GameTexts.SetVariable("ACTION", _sendDuelRequestText);
				ActionDescriptionText = ((object)GameTexts.FindText("str_key_action", (string)null)).ToString();
			}
			else
			{
				ActionDescriptionText = string.Empty;
			}
		}
	}

	public void UpdateBounty()
	{
		MissionRepresentativeBase representative = TargetPeer.Representative;
		Bounty = ((DuelMissionRepresentative)((representative is DuelMissionRepresentative) ? representative : null)).Bounty;
	}

	private void UpdateTracked()
	{
		if (!IsEnabled)
		{
			IsTracked = false;
		}
		else if (HasDuelRequestForPlayer || HasSentDuelRequest || IsFocused)
		{
			IsTracked = true;
		}
		else
		{
			IsTracked = false;
		}
		ShouldShowInformation = IsTracked || IsFocused;
	}

	public void OnDuelStarted()
	{
		IsEnabled = false;
		IsInDuel = true;
	}

	public void OnDuelEnded()
	{
		IsEnabled = true;
		IsInDuel = false;
	}

	public void UpdateCurentDuelStatus(bool isInDuel)
	{
		IsInDuel = isInDuel;
		IsEnabled = !IsInDuel;
	}

	public void RefreshPerkSelection()
	{
		((Collection<MPPerkVM>)(object)SelectedPerks).Clear();
		TargetPeer.RefreshSelectedPerks();
		foreach (MPPerkObject item in (List<MPPerkObject>)(object)TargetPeer.SelectedPerks)
		{
			((Collection<MPPerkVM>)(object)SelectedPerks).Add(new MPPerkVM(null, (IReadOnlyPerkObject)(object)item, isSelectable: true, 0));
		}
	}
}
