using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace NavalDLC.ViewModelCollection.Order;

public class NavalMissionOrderTroopControllerVM : MissionOrderTroopControllerVM
{
	private List<ClassConfiguration> _classData;

	public NavalMissionOrderTroopControllerVM(MissionOrderVM missionOrder, bool isDeployment, Action onTransferFinised)
		: base(missionOrder, isDeployment, onTransferFinised)
	{
	}

	protected override OrderTroopItemVM CreateTroopItemVM(Formation formation, Action<OrderTroopItemVM> onSelectFormation, Func<Formation, int> getFormationMorale)
	{
		return (OrderTroopItemVM)(object)new NavalOrderTroopItemVM(formation, onSelectFormation, getFormationMorale);
	}

	public void OnClassesSet(List<ClassConfiguration> classData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (classData == null)
		{
			return;
		}
		_classData = classData;
		foreach (ClassConfiguration classItem in classData)
		{
			if (((IEnumerable<OrderTroopItemVM>)((MissionOrderTroopControllerVM)this).TroopList).FirstOrDefault((Func<OrderTroopItemVM, bool>)((OrderTroopItemVM f) => f.Formation.Index == classItem.FormationIndex)) is NavalOrderTroopItemVM navalOrderTroopItemVM)
			{
				navalOrderTroopItemVM.UpdateClassData(classItem.FormationClass);
			}
			if (((IEnumerable<OrderTroopItemVM>)((MissionOrderTroopControllerVM)this).TransferTargetList).FirstOrDefault((Func<OrderTroopItemVM, bool>)((OrderTroopItemVM f) => f.Formation.Index == classItem.FormationIndex)) is NavalOrderTroopItemVM navalOrderTroopItemVM2)
			{
				navalOrderTroopItemVM2.UpdateClassData(classItem.FormationClass);
			}
		}
	}

	protected override void OnAfterNewTroopItemAdded()
	{
		((MissionOrderTroopControllerVM)this).OnAfterNewTroopItemAdded();
		OnClassesSet(_classData);
	}

	public override void SelectAllFormations(bool uiFeedback = true)
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		foreach (OrderSetVM item in (Collection<OrderSetVM>)(object)base.MissionOrder.OrderSets)
		{
			item.ExecuteDeSelect();
		}
		if (((IEnumerable<OrderTroopItemVM>)((MissionOrderTroopControllerVM)this).TroopList).Count((OrderTroopItemVM x) => ((OrderSubjectVM)x).IsSelectable) == 1)
		{
			((MissionOrderTroopControllerVM)this).OnSelectFormation(((IEnumerable<OrderTroopItemVM>)((MissionOrderTroopControllerVM)this).TroopList).FirstOrDefault((Func<OrderTroopItemVM, bool>)((OrderTroopItemVM x) => ((OrderSubjectVM)x).IsSelectable)));
			return;
		}
		if (((IEnumerable<OrderTroopItemVM>)((MissionOrderTroopControllerVM)this).TroopList).Any((OrderTroopItemVM t) => ((OrderSubjectVM)t).IsSelectable))
		{
			((MissionOrderTroopControllerVM)this).OrderController.ClearSelectedFormations();
			if (Mission.Current.IsNavalBattle)
			{
				for (int num = 0; num < ((List<OrderTroopItemVM>)(object)((MissionOrderTroopControllerVM)this).TroopList).Count; num++)
				{
					OrderTroopItemVM val = ((List<OrderTroopItemVM>)(object)((MissionOrderTroopControllerVM)this).TroopList)[num];
					if (!NavalDLCHelpers.IsPlayerCaptainOfFormationShip(val.Formation))
					{
						((MissionOrderTroopControllerVM)this).AddSelectedFormation(val);
					}
				}
			}
			else
			{
				((MissionOrderTroopControllerVM)this).OrderController.SelectAllFormations(uiFeedback);
			}
			if (uiFeedback && ((List<Formation>)(object)((MissionOrderTroopControllerVM)this).OrderController.SelectedFormations).Count > 0)
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=xTv4tCbZ}Everybody!! Listen to me", (Dictionary<string, object>)null)).ToString()));
			}
		}
		base.MissionOrder.SetActiveOrders();
	}

	public override void AddSelectedFormation(OrderTroopItemVM item)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (!((OrderSubjectVM)item).IsSelectable)
		{
			return;
		}
		if (Mission.Current.IsNavalBattle)
		{
			if (IsOnlyPlayerFormationSelected() && !NavalDLCHelpers.IsPlayerCaptainOfFormationShip(item.Formation))
			{
				((MissionOrderTroopControllerVM)this).SetSelectedFormation(item);
				return;
			}
			if (NavalDLCHelpers.IsPlayerCaptainOfFormationShip(item.Formation))
			{
				((MissionOrderTroopControllerVM)this).OrderController.ClearSelectedFormations();
			}
		}
		Formation formation = ((MissionOrderTroopControllerVM)this).Team.GetFormation(item.InitialFormationClass);
		((MissionOrderTroopControllerVM)this).OrderController.SelectFormation(formation);
		base.MissionOrder.SetActiveOrders();
	}

	private bool IsOnlyPlayerFormationSelected()
	{
		int num = 0;
		for (int i = 0; i < ((List<OrderTroopItemVM>)(object)((MissionOrderTroopControllerVM)this).TroopList).Count; i++)
		{
			if (((OrderSubjectVM)((List<OrderTroopItemVM>)(object)((MissionOrderTroopControllerVM)this).TroopList)[i]).IsSelected)
			{
				num++;
				if (!NavalDLCHelpers.IsPlayerCaptainOfFormationShip(((List<OrderTroopItemVM>)(object)((MissionOrderTroopControllerVM)this).TroopList)[i].Formation))
				{
					return false;
				}
			}
			if (num > 1)
			{
				return false;
			}
		}
		return num == 1;
	}
}
