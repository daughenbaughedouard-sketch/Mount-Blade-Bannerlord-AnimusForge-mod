using System;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class HeroClassGroupVM : ViewModel
{
	public readonly MPHeroClassGroup HeroClassGroup;

	private readonly Action<HeroPerkVM, MPPerkVM> _onPerkSelect;

	private string _name;

	private string _iconType;

	private string _iconPath;

	private MBBindingList<HeroClassVM> _subClasses;

	public bool IsValid => ((Collection<HeroClassVM>)(object)SubClasses).Count > 0;

	[DataSourceProperty]
	public MBBindingList<HeroClassVM> SubClasses
	{
		get
		{
			return _subClasses;
		}
		set
		{
			if (value != _subClasses)
			{
				_subClasses = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<HeroClassVM>>(value, "SubClasses");
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
	public string IconType
	{
		get
		{
			return _iconType;
		}
		set
		{
			if (value != _iconType)
			{
				_iconType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IconType");
				IconPath = "TroopBanners\\ClassType_" + value;
			}
		}
	}

	[DataSourceProperty]
	public string IconPath
	{
		get
		{
			return _iconPath;
		}
		set
		{
			if (value != _iconPath)
			{
				_iconPath = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IconPath");
			}
		}
	}

	public HeroClassGroupVM(Action<HeroClassVM> onSelect, Action<HeroPerkVM, MPPerkVM> onPerkSelect, MPHeroClassGroup heroClassGroup, MultiplayerCultureColorInfo colorInfo)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		HeroClassGroup = heroClassGroup;
		_onPerkSelect = onPerkSelect;
		IconType = heroClassGroup.StringId;
		SubClasses = new MBBindingList<HeroClassVM>();
		_ = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Team;
		foreach (MPHeroClass item in from h in MultiplayerClassDivisions.GetMPHeroClasses(PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer).Culture)
			where ((object)h.ClassGroup).Equals((object?)heroClassGroup)
			select h)
		{
			((Collection<HeroClassVM>)(object)SubClasses).Add(new HeroClassVM(onSelect, _onPerkSelect, item, colorInfo));
		}
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)HeroClassGroup.Name).ToString();
		SubClasses.ApplyActionOnAllItems((Action<HeroClassVM>)delegate(HeroClassVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}
}
