using System;
using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.CampaignSystem.ViewModelCollection.Map.Tracker;

public abstract class MapTrackerItemVM<T> : MapTrackerItemVM where T : ITrackableCampaignObject
{
	public new T TrackedObject => (T)(object)base.TrackedObject;

	protected MapTrackerItemVM(T trackableObject)
		: base((ITrackableCampaignObject)(object)trackableObject)
	{
		base.IsTracked = Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)TrackedObject);
	}

	protected sealed override void OnUpdateProperties()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		_nameBind = ((object)((ITrackableBase)TrackedObject/*cast due to .constrained prefix*/).GetName()).ToString();
		Banner banner = ((ITrackableCampaignObject)TrackedObject/*cast due to .constrained prefix*/).GetBanner();
		_factionVisualBind = new BannerImageIdentifierVM(banner, true);
		_isVisibleOnMapBind = IsVisibleOnMap();
		_canToggleTrackBind = GetCanToggleTrack();
		_questsBind = GetRelatedQuests();
	}

	protected sealed override void OnUpdatePosition(float screenX, float screenY, float screenW)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_latestX = screenX;
		_latestY = screenY;
		_latestW = screenW;
		_partyPositionBind = new Vec2(_latestX, _latestY);
		_isBehindBind = _latestW < 0f;
	}

	protected sealed override void OnToggleTrack()
	{
		if (GetCanToggleTrack())
		{
			if (base.IsTracked)
			{
				Untrack();
			}
			else
			{
				Track();
			}
		}
	}

	protected sealed override void OnGoToPosition()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Action<CampaignVec2> onFastMoveCameraToPosition = MapTrackerItemVM.OnFastMoveCameraToPosition;
		if (onFastMoveCameraToPosition != null)
		{
			Vec3 position = ((ITrackableBase)TrackedObject/*cast due to .constrained prefix*/).GetPosition();
			onFastMoveCameraToPosition(new CampaignVec2(((Vec3)(ref position)).AsVec2, true));
		}
	}

	protected sealed override void OnRefreshBinding()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		base.Name = _nameBind;
		base.IsEnabled = _isVisibleOnMapBind;
		base.IsBehind = _isBehindBind;
		base.FactionVisual = _factionVisualBind;
		base.CanToggleTrack = _canToggleTrackBind;
		if (base.IsEnabled)
		{
			base.PartyPosition = _partyPositionBind;
		}
		if (_previousQuestsBind == _questsBind)
		{
			return;
		}
		((Collection<QuestMarkerVM>)(object)base.Quests).Clear();
		IssueQuestFlags[] issueQuestFlagsValues = CampaignUIHelper.IssueQuestFlagsValues;
		foreach (IssueQuestFlags val in issueQuestFlagsValues)
		{
			if ((int)val != 0 && (_questsBind & val) != 0)
			{
				((Collection<QuestMarkerVM>)(object)base.Quests).Add(new QuestMarkerVM(val, (TextObject)null, (TextObject)null));
			}
		}
		_previousQuestsBind = _questsBind;
	}

	private void Track()
	{
		base.IsTracked = true;
		if (!Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)TrackedObject))
		{
			Campaign.Current.VisualTrackerManager.RegisterObject((ITrackableCampaignObject)(object)TrackedObject);
		}
	}

	private void Untrack()
	{
		base.IsTracked = false;
		if (Campaign.Current.VisualTrackerManager.CheckTracked((ITrackableBase)(object)TrackedObject))
		{
			Campaign.Current.VisualTrackerManager.RemoveTrackedObject((ITrackableBase)(object)TrackedObject, false);
		}
	}
}
public abstract class MapTrackerItemVM : ViewModel
{
	public readonly ITrackableCampaignObject TrackedObject;

	protected float _latestX;

	protected float _latestY;

	protected float _latestW;

	protected IssueQuestFlags _previousQuestsBind;

	protected IssueQuestFlags _questsBind;

	protected bool _isVisibleOnMapBind;

	protected bool _isBehindBind;

	protected bool _canToggleTrackBind;

	protected string _nameBind;

	protected Vec2 _partyPositionBind;

	protected BannerImageIdentifierVM _factionVisualBind;

	public static Action<CampaignVec2> OnFastMoveCameraToPosition;

	private bool _isTracked;

	private bool _canToggleTrack;

	private bool _isEnabled;

	private bool _isBehind;

	private string _name;

	private string _trackerType;

	private Vec2 _partyPosition;

	private BannerImageIdentifierVM _factionVisual;

	private MBBindingList<QuestMarkerVM> _quests;

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
	public bool CanToggleTrack
	{
		get
		{
			return _canToggleTrack;
		}
		set
		{
			if (value != _canToggleTrack)
			{
				_canToggleTrack = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CanToggleTrack");
			}
		}
	}

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
			}
		}
	}

	[DataSourceProperty]
	public bool IsBehind
	{
		get
		{
			return _isBehind;
		}
		set
		{
			if (value != _isBehind)
			{
				_isBehind = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBehind");
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
	public string TrackerType
	{
		get
		{
			return _trackerType;
		}
		set
		{
			if (value != _trackerType)
			{
				_trackerType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TrackerType");
			}
		}
	}

	[DataSourceProperty]
	public Vec2 PartyPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _partyPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _partyPosition)
			{
				_partyPosition = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PartyPosition");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM FactionVisual
	{
		get
		{
			return _factionVisual;
		}
		set
		{
			if (value != _factionVisual)
			{
				_factionVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "FactionVisual");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<QuestMarkerVM> Quests
	{
		get
		{
			return _quests;
		}
		set
		{
			if (value != _quests)
			{
				_quests = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<QuestMarkerVM>>(value, "Quests");
			}
		}
	}

	public MapTrackerItemVM(ITrackableCampaignObject trackedObject)
	{
		TrackedObject = trackedObject;
		Quests = new MBBindingList<QuestMarkerVM>();
		UpdateProperties();
	}

	protected abstract void OnShowTooltip();

	protected abstract void OnUpdateProperties();

	protected abstract void OnUpdatePosition(float screenX, float screenY, float screenW);

	protected abstract void OnToggleTrack();

	protected abstract void OnGoToPosition();

	protected abstract void OnRefreshBinding();

	protected abstract bool IsVisibleOnMap();

	protected abstract bool GetCanToggleTrack();

	protected abstract string GetTrackerType();

	protected abstract IssueQuestFlags GetRelatedQuests();

	public void UpdateProperties()
	{
		OnUpdateProperties();
	}

	public void UpdatePosition(float screenX, float screenY, float screenW)
	{
		OnUpdatePosition(screenX, screenY, screenW);
	}

	public void ExecuteToggleTrack()
	{
		OnToggleTrack();
	}

	public void ExecuteGoToPosition()
	{
		OnGoToPosition();
	}

	public void ExecuteShowTooltip()
	{
		OnShowTooltip();
	}

	public void ExecuteHideTooltip()
	{
		MBInformationManager.HideInformations();
	}

	public void RefreshBinding()
	{
		OnRefreshBinding();
		TrackerType = GetTrackerType();
	}
}
