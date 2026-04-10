using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipCommandingShipsTutorial")]
public class ShipCommandingShipsTutorial : TutorialItemBase
{
	private int _lastController;

	private bool _registeredToOrderEvent;

	private bool _hasOrderedCharge;

	public ShipCommandingShipsTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		Mission current = Mission.Current;
		PirateBattleMissionController pirateBattleMissionController = ((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null);
		if (pirateBattleMissionController != null && pirateBattleMissionController.HasSelectedShip)
		{
			if (_lastController != ((object)pirateBattleMissionController).GetHashCode())
			{
				_hasOrderedCharge = false;
				_registeredToOrderEvent = false;
				_lastController = ((object)pirateBattleMissionController).GetHashCode();
			}
			if (pirateBattleMissionController.HasSelectedShip)
			{
				if (!_registeredToOrderEvent)
				{
					Mission current2 = Mission.Current;
					object obj;
					if (current2 == null)
					{
						obj = null;
					}
					else
					{
						Team playerTeam = current2.PlayerTeam;
						obj = ((playerTeam != null) ? playerTeam.PlayerOrderController : null);
					}
					if (obj != null)
					{
						Mission current3 = Mission.Current;
						if (current3 != null && (int)current3.Mode == 2)
						{
							Mission.Current.PlayerTeam.PlayerOrderController.OnOrderIssued += new OnOrderIssuedDelegate(OnPlayerOrdered);
							_registeredToOrderEvent = true;
						}
					}
				}
				return _hasOrderedCharge;
			}
		}
		return false;
	}

	private void OnPlayerOrdered(OrderType orderType, MBReadOnlyList<Formation> appliedFormations, OrderController orderController, object[] delegateParams)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		_hasOrderedCharge = _hasOrderedCharge || (int)orderType == 12;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
		if (missionBehavior != null && missionBehavior.IsFirstShipCleared)
		{
			return missionBehavior.HasSelectedShip;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
