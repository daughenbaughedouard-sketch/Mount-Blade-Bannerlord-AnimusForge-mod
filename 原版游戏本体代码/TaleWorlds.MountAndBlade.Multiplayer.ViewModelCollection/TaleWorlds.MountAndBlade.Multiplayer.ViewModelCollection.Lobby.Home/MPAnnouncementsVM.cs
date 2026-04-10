using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;

public class MPAnnouncementsVM : ViewModel
{
	private readonly float? _announcementUpdateIntervalInSeconds;

	private float? _updateTimer;

	private bool _isRefreshingAnnouncements;

	private bool _hasValidAnnouncements;

	private string _titleText;

	private MBBindingList<MPAnnouncementItemVM> _announcementList;

	[DataSourceProperty]
	public bool HasValidAnnouncements
	{
		get
		{
			return _hasValidAnnouncements;
		}
		set
		{
			if (value != _hasValidAnnouncements)
			{
				_hasValidAnnouncements = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasValidAnnouncements");
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
	public MBBindingList<MPAnnouncementItemVM> AnnouncementList
	{
		get
		{
			return _announcementList;
		}
		set
		{
			if (value != _announcementList)
			{
				_announcementList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPAnnouncementItemVM>>(value, "AnnouncementList");
			}
		}
	}

	public MPAnnouncementsVM(float? announcementUpdateIntervalInSeconds)
	{
		_updateTimer = null;
		_announcementUpdateIntervalInSeconds = announcementUpdateIntervalInSeconds;
		AnnouncementList = new MBBindingList<MPAnnouncementItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=lQ0T2pbY}Events & Announcements", (Dictionary<string, object>)null)).ToString();
	}

	public void OnTick(float dt)
	{
		if (!NetworkMain.GameClient.AtLobby)
		{
			_updateTimer = null;
		}
		else if (_announcementUpdateIntervalInSeconds.HasValue && _announcementUpdateIntervalInSeconds > 0f && !_isRefreshingAnnouncements)
		{
			if (!_updateTimer.HasValue || _updateTimer > _announcementUpdateIntervalInSeconds)
			{
				RefreshAnnouncements();
				_updateTimer = 0f;
			}
			else
			{
				_updateTimer += dt;
			}
		}
	}

	private void RefreshAnnouncements()
	{
		_isRefreshingAnnouncements = true;
		UpdateAnnouncements();
	}

	public async void UpdateAnnouncements()
	{
		PublishedLobbyNewsArticle[] array = await NetworkMain.GameClient.GetLobbyNews();
		((Collection<MPAnnouncementItemVM>)(object)AnnouncementList).Clear();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				MPAnnouncementItemVM item = new MPAnnouncementItemVM(array[i]);
				((Collection<MPAnnouncementItemVM>)(object)AnnouncementList).Add(item);
			}
		}
		HasValidAnnouncements = ((Collection<MPAnnouncementItemVM>)(object)AnnouncementList).Count > 0 && ApplicationPlatform.IsPlatformWindows() && (int)ApplicationPlatform.CurrentPlatform != 8;
		_isRefreshingAnnouncements = false;
	}
}
