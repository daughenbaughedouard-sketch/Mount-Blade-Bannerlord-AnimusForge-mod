using NavalDLC.Storyline;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.GauntletUI.Tutorial;

[Tutorial("ShipBoardingTroopChargeTutorial")]
public class ShipBoardingTroopChargeTutorial : TutorialItemBase
{
	private int _lastControllerHashCode;

	private bool _registeredToOrderEvent;

	private bool _hasOrderedCharge;

	public ShipBoardingTroopChargeTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		Mission current = Mission.Current;
		PirateBattleMissionController pirateBattleMissionController = ((current != null) ? current.GetMissionBehavior<PirateBattleMissionController>() : null);
		if (pirateBattleMissionController != null)
		{
			if (_lastControllerHashCode != ((object)pirateBattleMissionController).GetHashCode())
			{
				_hasOrderedCharge = false;
				_registeredToOrderEvent = false;
				_lastControllerHashCode = ((object)pirateBattleMissionController).GetHashCode();
			}
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
		return false;
	}

	private void OnPlayerOrdered(OrderType orderType, MBReadOnlyList<Formation> appliedFormations, OrderController orderController, object[] delegateParams)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		_hasOrderedCharge = _hasOrderedCharge || (int)orderType == 4;
	}

	public override bool IsConditionsMetForActivation()
	{
		if (Mission.Current == null || !Mission.Current.IsNavalBattle)
		{
			return false;
		}
		PirateBattleMissionController missionBehavior = Mission.Current.GetMissionBehavior<PirateBattleMissionController>();
		if (missionBehavior != null)
		{
			return !missionBehavior.IsFirstShipCleared;
		}
		return false;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}
}
