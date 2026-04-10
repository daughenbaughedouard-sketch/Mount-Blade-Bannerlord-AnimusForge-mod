using SandBox.GauntletUI.Tutorial;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("MovementInMissionTutorial")]
public class MovementInMissionTutorial : TutorialItemBase
{
	private bool _playerMovedForward;

	private bool _playerMovedBackward;

	private bool _playerMovedLeft;

	private bool _playerMovedRight;

	public MovementInMissionTutorial()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = false;
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (_playerMovedBackward && _playerMovedLeft && _playerMovedRight)
		{
			return _playerMovedForward;
		}
		return false;
	}

	public override void OnPlayerMovementFlagChanged(MissionPlayerMovementFlagsChangeEvent obj)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Invalid comparison between Unknown and I4
		((TutorialItemBase)this).OnPlayerMovementFlagChanged(obj);
		_playerMovedRight = _playerMovedRight || (obj.MovementFlag & 4) == 4;
		_playerMovedLeft = _playerMovedLeft || (obj.MovementFlag & 8) == 8;
		_playerMovedForward = _playerMovedForward || (obj.MovementFlag & 1) == 1;
		_playerMovedBackward = _playerMovedBackward || (obj.MovementFlag & 2) == 2;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)8;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (Mission.Current != null && (int)Mission.Current.Mode != 6 && !TutorialHelper.PlayerIsInAConversation)
		{
			return (int)TutorialHelper.CurrentContext == 8;
		}
		return false;
	}
}
