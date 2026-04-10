using System;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.ClassFilter;

public class MPLobbyClassFilterClassGroupItemVM : ViewModel
{
	private string _name;

	private MBBindingList<MPLobbyClassFilterClassItemVM> _classes;

	public MPHeroClassGroup ClassGroup { get; set; }

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
	public MBBindingList<MPLobbyClassFilterClassItemVM> Classes
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
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyClassFilterClassItemVM>>(value, "Classes");
			}
		}
	}

	public MPLobbyClassFilterClassGroupItemVM(MPHeroClassGroup classGroup)
	{
		ClassGroup = classGroup;
		Classes = new MBBindingList<MPLobbyClassFilterClassItemVM>();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = ((object)ClassGroup.Name).ToString();
		Classes.ApplyActionOnAllItems((Action<MPLobbyClassFilterClassItemVM>)delegate(MPLobbyClassFilterClassItemVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		ClassGroup = null;
	}

	public void AddClass(BasicCultureObject culture, MPHeroClass heroClass, Action<MPLobbyClassFilterClassItemVM> onSelect)
	{
		MPLobbyClassFilterClassItemVM item = new MPLobbyClassFilterClassItemVM(culture, heroClass, onSelect);
		((Collection<MPLobbyClassFilterClassItemVM>)(object)Classes).Add(item);
	}
}
