using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace NavalDLC.ViewModelCollection.Order;

public class NavalMissionOrderVM : MissionOrderVM
{
	private List<ClassConfiguration> _classData;

	public NavalMissionOrderVM(OrderController orderController, bool isDeployment, bool isMultiplayer)
		: base(orderController, isDeployment, isMultiplayer)
	{
		((ViewModel)this).RefreshValues();
	}

	protected override MissionOrderTroopControllerVM CreateTroopController(OrderController orderController)
	{
		return (MissionOrderTroopControllerVM)(object)new NavalMissionOrderTroopControllerVM((MissionOrderVM)(object)this, ((MissionOrderVM)this).IsDeployment, (Action)base.OnTransferFinished);
	}

	public void OnClassesSet(List<ClassConfiguration> classData)
	{
		_classData = classData;
		(((MissionOrderVM)this).TroopController as NavalMissionOrderTroopControllerVM).OnClassesSet(_classData);
	}

	public override void OnOrderLayoutTypeChanged()
	{
		((MissionOrderVM)this).OnOrderLayoutTypeChanged();
		(((MissionOrderVM)this).TroopController as NavalMissionOrderTroopControllerVM).OnClassesSet(_classData);
	}
}
