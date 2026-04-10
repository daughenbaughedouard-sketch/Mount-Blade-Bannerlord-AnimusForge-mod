using System;
using System.Collections.Generic;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipInput;
using TaleWorlds.CampaignSystem.ViewModelCollection.Input;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Tutorial;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.ViewModelCollection.Missions.ShipControl;

public class MissionShipControlVM : ViewModel
{
	private enum SailStateVisual
	{
		Invalid = -1,
		Raised,
		SquareSailsRaised,
		Full
	}

	private enum OarsmenStateVisual
	{
		Invalid = -1,
		Idle,
		Normal,
		Fast
	}

	private enum SailTypeVisual
	{
		Square,
		Lateen,
		Hybrid
	}

	public class ShipControlInputKeyItemVM : ViewModel
	{
		private bool _isVisible;

		private bool _isDisabled;

		private InputKeyItemVM _key;

		[DataSourceProperty]
		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				if (value != _isVisible)
				{
					_isVisible = value;
					((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisible");
				}
			}
		}

		[DataSourceProperty]
		public bool IsDisabled
		{
			get
			{
				return _isDisabled;
			}
			set
			{
				if (value != _isDisabled)
				{
					_isDisabled = value;
					((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisabled");
				}
			}
		}

		[DataSourceProperty]
		public InputKeyItemVM Key
		{
			get
			{
				return _key;
			}
			set
			{
				if (value != _key)
				{
					_key = value;
					((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "Key");
				}
			}
		}

		public ShipControlInputKeyItemVM(InputKeyItemVM key)
		{
			Key = key;
			((ViewModel)this).RefreshValues();
		}

		public override void RefreshValues()
		{
			((ViewModel)this).RefreshValues();
			((ViewModel)Key).RefreshValues();
		}

		public override void OnFinalize()
		{
			((ViewModel)this).OnFinalize();
			((ViewModel)Key).OnFinalize();
		}
	}

	public const int WindSailSegmentCount = 24;

	private OarsmenStateVisual _activeOarsmenState;

	private SailStateVisual _activeSailState;

	private SailTypeVisual _activeSailType;

	private ShipControlInputKeyItemVM _changeCameraKey;

	private ShipControlInputKeyItemVM _toggleSailKey;

	private ShipControlInputKeyItemVM _cutLooseKey;

	private ShipControlInputKeyItemVM _toggleOarsmenKey;

	private ShipControlInputKeyItemVM _toggleBallistaKey;

	private ShipControlInputKeyItemVM _attemptBoardingKey;

	private ShipControlInputKeyItemVM _stopUsingShipKey;

	private bool _isControllingShip;

	private bool _isUsingBallistaRemotely;

	private bool _isUsingBallistaDirectly;

	private bool _hasTargetedShip;

	private bool _isTargetedShipPlayerTeam;

	private bool _isTargetedShipPlayerAllyTeam;

	private bool _isTargetedShipEnemyTeam;

	private bool _targetedShipHasAction;

	private Vec2 _targetedShipPosition;

	private Vec2 _boardingTargetShipPosition;

	private bool _isSailHighlightActive;

	private bool _isOarsmenHighlightActive;

	private int _targetedShipWSign;

	private int _boardingTargetShipWSign;

	private string _sailState;

	private string _oarsmenState;

	private string _cancelText;

	private int _sailType;

	private Vec2 _projectedWindDirection;

	private int _ballistaAmmoCount;

	private bool _isAmmoCountWarned;

	private bool _isCutLooseOrderActive;

	private bool _isAttemptBoardingOrderActive;

	private bool _isCancelBoardingOrderAvailable;

	private MissionHitPointPropertiesVM _shipHitPoints;

	private MissionHitPointPropertiesVM _sailHitPoints;

	private MissionHitPointPropertiesVM _fireHitPoints;

	[DataSourceProperty]
	public ShipControlInputKeyItemVM ChangeCameraKey
	{
		get
		{
			return _changeCameraKey;
		}
		set
		{
			if (value != _changeCameraKey)
			{
				_changeCameraKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "ChangeCameraKey");
			}
		}
	}

	[DataSourceProperty]
	public ShipControlInputKeyItemVM ToggleSailKey
	{
		get
		{
			return _toggleSailKey;
		}
		set
		{
			if (value != _toggleSailKey)
			{
				_toggleSailKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "ToggleSailKey");
			}
		}
	}

	[DataSourceProperty]
	public ShipControlInputKeyItemVM CutLooseKey
	{
		get
		{
			return _cutLooseKey;
		}
		set
		{
			if (value != _cutLooseKey)
			{
				_cutLooseKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "CutLooseKey");
			}
		}
	}

	[DataSourceProperty]
	public ShipControlInputKeyItemVM ToggleOarsmenKey
	{
		get
		{
			return _toggleOarsmenKey;
		}
		set
		{
			if (value != _toggleOarsmenKey)
			{
				_toggleOarsmenKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "ToggleOarsmenKey");
			}
		}
	}

	[DataSourceProperty]
	public ShipControlInputKeyItemVM ToggleBallistaKey
	{
		get
		{
			return _toggleBallistaKey;
		}
		set
		{
			if (value != _toggleBallistaKey)
			{
				_toggleBallistaKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "ToggleBallistaKey");
			}
		}
	}

	[DataSourceProperty]
	public ShipControlInputKeyItemVM AttemptBoardingKey
	{
		get
		{
			return _attemptBoardingKey;
		}
		set
		{
			if (value != _attemptBoardingKey)
			{
				_attemptBoardingKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "AttemptBoardingKey");
			}
		}
	}

	[DataSourceProperty]
	public ShipControlInputKeyItemVM StopUsingShipKey
	{
		get
		{
			return _stopUsingShipKey;
		}
		set
		{
			if (value != _stopUsingShipKey)
			{
				_stopUsingShipKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<ShipControlInputKeyItemVM>(value, "StopUsingShipKey");
			}
		}
	}

	[DataSourceProperty]
	public bool IsControllingShip
	{
		get
		{
			return _isControllingShip;
		}
		set
		{
			if (value != _isControllingShip)
			{
				_isControllingShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsControllingShip");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUsingBallistaRemotely
	{
		get
		{
			return _isUsingBallistaRemotely;
		}
		set
		{
			if (value != _isUsingBallistaRemotely)
			{
				_isUsingBallistaRemotely = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUsingBallistaRemotely");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUsingBallistaDirectly
	{
		get
		{
			return _isUsingBallistaDirectly;
		}
		set
		{
			if (value != _isUsingBallistaDirectly)
			{
				_isUsingBallistaDirectly = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUsingBallistaDirectly");
			}
		}
	}

	[DataSourceProperty]
	public bool HasTargetedShip
	{
		get
		{
			return _hasTargetedShip;
		}
		set
		{
			if (value != _hasTargetedShip)
			{
				_hasTargetedShip = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasTargetedShip");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTargetedShipPlayerTeam
	{
		get
		{
			return _isTargetedShipPlayerTeam;
		}
		set
		{
			if (value != _isTargetedShipPlayerTeam)
			{
				_isTargetedShipPlayerTeam = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTargetedShipPlayerTeam");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTargetedShipPlayerAllyTeam
	{
		get
		{
			return _isTargetedShipPlayerAllyTeam;
		}
		set
		{
			if (value != _isTargetedShipPlayerAllyTeam)
			{
				_isTargetedShipPlayerAllyTeam = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTargetedShipPlayerAllyTeam");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTargetedShipEnemyTeam
	{
		get
		{
			return _isTargetedShipEnemyTeam;
		}
		set
		{
			if (value != _isTargetedShipEnemyTeam)
			{
				_isTargetedShipEnemyTeam = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTargetedShipEnemyTeam");
			}
		}
	}

	[DataSourceProperty]
	public bool TargetedShipHasAction
	{
		get
		{
			return _targetedShipHasAction;
		}
		set
		{
			if (value != _targetedShipHasAction)
			{
				_targetedShipHasAction = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TargetedShipHasAction");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSailHighlightActive
	{
		get
		{
			return _isSailHighlightActive;
		}
		set
		{
			if (value != _isSailHighlightActive)
			{
				_isSailHighlightActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSailHighlightActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOarsmenHighlightActive
	{
		get
		{
			return _isOarsmenHighlightActive;
		}
		set
		{
			if (value != _isOarsmenHighlightActive)
			{
				_isOarsmenHighlightActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOarsmenHighlightActive");
			}
		}
	}

	[DataSourceProperty]
	public MissionHitPointPropertiesVM ShipHitPoints
	{
		get
		{
			return _shipHitPoints;
		}
		set
		{
			if (value != _shipHitPoints)
			{
				_shipHitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionHitPointPropertiesVM>(value, "ShipHitPoints");
			}
		}
	}

	[DataSourceProperty]
	public MissionHitPointPropertiesVM SailHitPoints
	{
		get
		{
			return _sailHitPoints;
		}
		set
		{
			if (value != _sailHitPoints)
			{
				_sailHitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionHitPointPropertiesVM>(value, "SailHitPoints");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 TargetedShipPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _targetedShipPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _targetedShipPosition)
			{
				_targetedShipPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TargetedShipPosition");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 BoardingTargetShipPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _boardingTargetShipPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _boardingTargetShipPosition)
			{
				_boardingTargetShipPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "BoardingTargetShipPosition");
			}
		}
	}

	[DataSourceProperty]
	public MissionHitPointPropertiesVM FireHitPoints
	{
		get
		{
			return _fireHitPoints;
		}
		set
		{
			if (value != _fireHitPoints)
			{
				_fireHitPoints = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionHitPointPropertiesVM>(value, "FireHitPoints");
			}
		}
	}

	[DataSourceProperty]
	public int TargetedShipWSign
	{
		get
		{
			return _targetedShipWSign;
		}
		set
		{
			if (value != _targetedShipWSign)
			{
				_targetedShipWSign = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TargetedShipWSign");
			}
		}
	}

	[DataSourceProperty]
	public int BoardingTargetShipWSign
	{
		get
		{
			return _boardingTargetShipWSign;
		}
		set
		{
			if (value != _boardingTargetShipWSign)
			{
				_boardingTargetShipWSign = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "BoardingTargetShipWSign");
			}
		}
	}

	[DataSourceProperty]
	public string SailState
	{
		get
		{
			return _sailState;
		}
		set
		{
			if (value != _sailState)
			{
				_sailState = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SailState");
			}
		}
	}

	[DataSourceProperty]
	public string OarsmenState
	{
		get
		{
			return _oarsmenState;
		}
		set
		{
			if (value != _oarsmenState)
			{
				_oarsmenState = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OarsmenState");
			}
		}
	}

	[DataSourceProperty]
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 ProjectedWindDirection
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _projectedWindDirection;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _projectedWindDirection)
			{
				_projectedWindDirection = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ProjectedWindDirection");
			}
		}
	}

	[DataSourceProperty]
	public int SailType
	{
		get
		{
			return _sailType;
		}
		set
		{
			if (value != _sailType)
			{
				_sailType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "SailType");
			}
		}
	}

	[DataSourceProperty]
	public int BallistaAmmoCount
	{
		get
		{
			return _ballistaAmmoCount;
		}
		set
		{
			if (value != _ballistaAmmoCount)
			{
				_ballistaAmmoCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "BallistaAmmoCount");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAmmoCountWarned
	{
		get
		{
			return _isAmmoCountWarned;
		}
		set
		{
			if (value != _isAmmoCountWarned)
			{
				_isAmmoCountWarned = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAmmoCountWarned");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCutLooseOrderActive
	{
		get
		{
			return _isCutLooseOrderActive;
		}
		set
		{
			if (value != _isCutLooseOrderActive)
			{
				_isCutLooseOrderActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCutLooseOrderActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsAttemptBoardingOrderActive
	{
		get
		{
			return _isAttemptBoardingOrderActive;
		}
		set
		{
			if (value != _isAttemptBoardingOrderActive)
			{
				_isAttemptBoardingOrderActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsAttemptBoardingOrderActive");
			}
		}
	}

	[DataSourceProperty]
	public bool IsCancelBoardingOrderAvailable
	{
		get
		{
			return _isCancelBoardingOrderAvailable;
		}
		set
		{
			if (value != _isCancelBoardingOrderAvailable)
			{
				_isCancelBoardingOrderAvailable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsCancelBoardingOrderAvailable");
			}
		}
	}

	public MissionShipControlVM()
	{
		_activeSailState = SailStateVisual.Invalid;
		_activeOarsmenState = OarsmenStateVisual.Invalid;
		ShipHitPoints = new MissionHitPointPropertiesVM();
		SailHitPoints = new MissionHitPointPropertiesVM();
		FireHitPoints = new MissionHitPointPropertiesVM();
		Game.Current.EventManager.RegisterEvent<TutorialNotificationElementChangeEvent>((Action<TutorialNotificationElementChangeEvent>)OnTutorialNotificationElementIDChanged);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		CancelText = ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString();
		ShipControlInputKeyItemVM changeCameraKey = ChangeCameraKey;
		if (changeCameraKey != null)
		{
			((ViewModel)changeCameraKey).RefreshValues();
		}
		ShipControlInputKeyItemVM cutLooseKey = CutLooseKey;
		if (cutLooseKey != null)
		{
			((ViewModel)cutLooseKey).RefreshValues();
		}
		ShipControlInputKeyItemVM toggleOarsmenKey = ToggleOarsmenKey;
		if (toggleOarsmenKey != null)
		{
			((ViewModel)toggleOarsmenKey).RefreshValues();
		}
		ShipControlInputKeyItemVM toggleSailKey = ToggleSailKey;
		if (toggleSailKey != null)
		{
			((ViewModel)toggleSailKey).RefreshValues();
		}
		ShipControlInputKeyItemVM toggleBallistaKey = ToggleBallistaKey;
		if (toggleBallistaKey != null)
		{
			((ViewModel)toggleBallistaKey).RefreshValues();
		}
		ShipControlInputKeyItemVM attemptBoardingKey = AttemptBoardingKey;
		if (attemptBoardingKey != null)
		{
			((ViewModel)attemptBoardingKey).RefreshValues();
		}
		ShipControlInputKeyItemVM stopUsingShipKey = StopUsingShipKey;
		if (stopUsingShipKey != null)
		{
			((ViewModel)stopUsingShipKey).RefreshValues();
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		ShipControlInputKeyItemVM changeCameraKey = ChangeCameraKey;
		if (changeCameraKey != null)
		{
			((ViewModel)changeCameraKey).OnFinalize();
		}
		ShipControlInputKeyItemVM cutLooseKey = CutLooseKey;
		if (cutLooseKey != null)
		{
			((ViewModel)cutLooseKey).OnFinalize();
		}
		ShipControlInputKeyItemVM toggleOarsmenKey = ToggleOarsmenKey;
		if (toggleOarsmenKey != null)
		{
			((ViewModel)toggleOarsmenKey).OnFinalize();
		}
		ShipControlInputKeyItemVM toggleSailKey = ToggleSailKey;
		if (toggleSailKey != null)
		{
			((ViewModel)toggleSailKey).OnFinalize();
		}
		ShipControlInputKeyItemVM toggleBallistaKey = ToggleBallistaKey;
		if (toggleBallistaKey != null)
		{
			((ViewModel)toggleBallistaKey).OnFinalize();
		}
		ShipControlInputKeyItemVM attemptBoardingKey = AttemptBoardingKey;
		if (attemptBoardingKey != null)
		{
			((ViewModel)attemptBoardingKey).OnFinalize();
		}
		ShipControlInputKeyItemVM stopUsingShipKey = StopUsingShipKey;
		if (stopUsingShipKey != null)
		{
			((ViewModel)stopUsingShipKey).OnFinalize();
		}
		Game.Current.EventManager.UnregisterEvent<TutorialNotificationElementChangeEvent>((Action<TutorialNotificationElementChangeEvent>)OnTutorialNotificationElementIDChanged);
	}

	public void SetTargetedShip(MissionShip ship, float screenX = -5000f, float screenY = -5000f, float screenW = -1f)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Invalid comparison between Unknown and I4
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (ship == null)
		{
			HasTargetedShip = false;
			IsTargetedShipPlayerTeam = false;
			IsTargetedShipPlayerAllyTeam = false;
			IsTargetedShipEnemyTeam = false;
			TargetedShipPosition = new Vec2(-5000f, -5000f);
			TargetedShipWSign = -1;
		}
		else
		{
			HasTargetedShip = true;
			Team team = ship.Team;
			IsTargetedShipPlayerTeam = team != null && (int)team.TeamSide == 0;
			Team team2 = ship.Team;
			IsTargetedShipPlayerAllyTeam = team2 != null && (int)team2.TeamSide == 1;
			Team team3 = ship.Team;
			IsTargetedShipEnemyTeam = team3 != null && (int)team3.TeamSide == 2;
			TargetedShipPosition = new Vec2(screenX, screenY);
			TargetedShipWSign = ((screenW > 0f) ? 1 : (-1));
		}
	}

	public void SetBoardingTargetShip(MissionShip ship, float screenX = -5000f, float screenY = -5000f, float screenW = -1f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (ship == null)
		{
			BoardingTargetShipPosition = new Vec2(-5000f, -5000f);
			BoardingTargetShipWSign = -1;
		}
		else
		{
			BoardingTargetShipPosition = new Vec2(screenX, screenY);
			BoardingTargetShipWSign = ((screenW > 0f) ? 1 : (-1));
		}
	}

	public void SetSailState(SailInput input)
	{
		SailStateVisual sailVisual = GetSailVisual(input);
		if (_activeSailState != sailVisual)
		{
			SailState = sailVisual.ToString();
			_activeSailState = sailVisual;
		}
	}

	public void SetOarsmanLevel(int level)
	{
		if (level != (int)_activeOarsmenState)
		{
			switch (level)
			{
			case 0:
				_activeOarsmenState = OarsmenStateVisual.Idle;
				break;
			case 1:
				_activeOarsmenState = OarsmenStateVisual.Normal;
				break;
			case 2:
				_activeOarsmenState = OarsmenStateVisual.Fast;
				break;
			default:
				Debug.FailedAssert($"Invalid oarsman state: {level}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Missions\\ShipControl\\MissionShipControlVM.cs", "SetOarsmanLevel", 157);
				break;
			}
			OarsmenState = _activeOarsmenState.ToString();
		}
	}

	public void SetSailType(bool hasLateenSail, bool hasSquareSail)
	{
		if (hasLateenSail && hasSquareSail)
		{
			_activeSailType = SailTypeVisual.Hybrid;
		}
		else if (hasLateenSail)
		{
			_activeSailType = SailTypeVisual.Lateen;
		}
		else
		{
			_activeSailType = SailTypeVisual.Square;
		}
		SailType = (int)_activeSailType;
	}

	private static SailStateVisual GetSailVisual(SailInput input)
	{
		return input switch
		{
			SailInput.Full => SailStateVisual.Full, 
			SailInput.SquareSailsRaised => SailStateVisual.SquareSailsRaised, 
			SailInput.Raised => SailStateVisual.Raised, 
			_ => SailStateVisual.Invalid, 
		};
	}

	private void OnTutorialNotificationElementIDChanged(TutorialNotificationElementChangeEvent obj)
	{
		IsSailHighlightActive = obj.NewNotificationElementID == "SailToggle";
		IsOarsmenHighlightActive = obj.NewNotificationElementID == "OarsmenToggle";
	}

	public void SetChangeCameraKey(GameKey gameKey)
	{
		ChangeCameraKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}

	public void SetToggleSailKey(GameKey gameKey)
	{
		ToggleSailKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}

	public void SetCutLooseKey(GameKey gameKey)
	{
		CutLooseKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}

	public void SetToggleOarsmenKey(GameKey gameKey)
	{
		ToggleOarsmenKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}

	public void SetToggleBallistaKey(GameKey gameKey)
	{
		ToggleBallistaKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}

	public void SetAttemptBoardingKey(GameKey gameKey)
	{
		AttemptBoardingKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}

	public void SetStopUsingShipKey(GameKey gameKey)
	{
		StopUsingShipKey = new ShipControlInputKeyItemVM(InputKeyItemVM.CreateFromGameKey(gameKey, false));
	}
}
