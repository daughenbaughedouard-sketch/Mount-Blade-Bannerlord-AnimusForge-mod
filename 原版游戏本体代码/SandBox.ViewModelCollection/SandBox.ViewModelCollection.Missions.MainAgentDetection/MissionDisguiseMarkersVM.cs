using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Missions.MainAgentDetection;

public class MissionDisguiseMarkersVM : ViewModel
{
	private MissionDisguiseMarkerItemVM _targetAgent;

	private MBBindingList<MissionDisguiseMarkerItemVM> _hostileAgents;

	[DataSourceProperty]
	public MissionDisguiseMarkerItemVM TargetAgent
	{
		get
		{
			return _targetAgent;
		}
		set
		{
			if (value != _targetAgent)
			{
				_targetAgent = value;
				((ViewModel)this).OnPropertyChangedWithValue<MissionDisguiseMarkerItemVM>(value, "TargetAgent");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionDisguiseMarkerItemVM> HostileAgents
	{
		get
		{
			return _hostileAgents;
		}
		set
		{
			if (value != _hostileAgents)
			{
				_hostileAgents = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionDisguiseMarkerItemVM>>(value, "HostileAgents");
			}
		}
	}

	public MissionDisguiseMarkersVM()
	{
		HostileAgents = new MBBindingList<MissionDisguiseMarkerItemVM>();
	}
}
