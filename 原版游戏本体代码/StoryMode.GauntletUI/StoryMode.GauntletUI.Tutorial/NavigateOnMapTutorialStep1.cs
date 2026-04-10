using System;
using SandBox.GauntletUI.Tutorial;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Tutorial;
using TaleWorlds.Core;

namespace StoryMode.GauntletUI.Tutorial;

[Tutorial("NavigateOnMapTutorialStep1")]
public class NavigateOnMapTutorialStep1 : TutorialItemBase
{
	private bool _movedPosition;

	private bool _movedRotation;

	private const float _delayInSeconds = 2f;

	private DateTime _completionTime = DateTime.MinValue;

	public NavigateOnMapTutorialStep1()
	{
		((TutorialItemBase)this).Placement = (ItemPlacements)1;
		((TutorialItemBase)this).HighlightedVisualElementID = string.Empty;
		((TutorialItemBase)this).MouseRequired = true;
	}

	public override TutorialContexts GetTutorialsRelevantContext()
	{
		return (TutorialContexts)4;
	}

	public override bool IsConditionsMetForActivation()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (!TutorialHelper.TownMenuIsOpen && !TutorialHelper.VillageMenuIsOpen)
		{
			return (int)TutorialHelper.CurrentContext == 4;
		}
		return false;
	}

	public override void OnMainMapCameraMove(MainMapCameraMoveEvent obj)
	{
		((TutorialItemBase)this).OnMainMapCameraMove(obj);
		_movedPosition = obj.PositionChanged || _movedPosition;
		_movedRotation = obj.RotationChanged || _movedRotation;
		if (_movedRotation && _movedPosition && _completionTime == DateTime.MinValue)
		{
			_completionTime = TutorialHelper.CurrentTime;
		}
	}

	public override bool IsConditionsMetForCompletion()
	{
		if (_completionTime == DateTime.MinValue)
		{
			return false;
		}
		return (TutorialHelper.CurrentTime - _completionTime).TotalSeconds > 2.0;
	}
}
