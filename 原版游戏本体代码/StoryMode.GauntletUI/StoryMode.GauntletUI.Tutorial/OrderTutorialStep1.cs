using System.Collections.Generic;
using System.Linq;
using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("OrderTutorial1TutorialStep1")]
public class OrderTutorialStep1 : TutorialItemBase
{
	private bool _hasPlayerOrderedFollowMe;

	private bool _registeredToOrderEvent;

	public OrderTutorialStep1()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)5;
		((TutorialItemBase)this).HighlightedVisualElementID = "";
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		if (!_registeredToOrderEvent)
		{
			Mission current = Mission.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				Team playerTeam = current.PlayerTeam;
				obj = ((playerTeam != null) ? playerTeam.PlayerOrderController : null);
			}
			if (obj != null)
			{
				Mission current2 = Mission.Current;
				if (current2 != null && (int)current2.Mode == 2)
				{
					Mission.Current.PlayerTeam.PlayerOrderController.OnOrderIssued += new OnOrderIssuedDelegate(OnPlayerOrdered);
					_registeredToOrderEvent = true;
				}
			}
		}
		return _hasPlayerOrderedFollowMe;
	}

	public override void OnDeactivate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		((TutorialItemBase)this).OnDeactivate();
		if (_registeredToOrderEvent)
		{
			Mission current = Mission.Current;
			object obj;
			if (current == null)
			{
				obj = null;
			}
			else
			{
				Team playerTeam = current.PlayerTeam;
				obj = ((playerTeam != null) ? playerTeam.PlayerOrderController : null);
			}
			if (obj != null)
			{
				Mission.Current.PlayerTeam.PlayerOrderController.OnOrderIssued -= new OnOrderIssuedDelegate(OnPlayerOrdered);
			}
		}
		_registeredToOrderEvent = false;
	}

	private void OnPlayerOrdered(OrderType orderType, IEnumerable<Formation> appliedFormations, OrderController orderController, params object[] delegateParams)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		_hasPlayerOrderedFollowMe = _hasPlayerOrderedFollowMe || ((int)orderType == 7 && appliedFormations.Any());
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		if ((int)TutorialHelper.CurrentContext == 8 && TutorialHelper.IsPlayerInABattleMission && (int)Mission.Current.Mode != 6)
		{
			return TutorialHelper.IsOrderingAvailable;
		}
		return false;
	}
}
