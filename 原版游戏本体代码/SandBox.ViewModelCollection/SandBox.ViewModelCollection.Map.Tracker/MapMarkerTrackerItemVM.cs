using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.Tracker;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.Map.Tracker;

public class MapMarkerTrackerItemVM : MapTrackerItemVM<MapMarker>
{
	public MapMarkerTrackerItemVM(MapMarker marker)
		: base(marker)
	{
	}

	protected override void OnShowTooltip()
	{
		InformationManager.ShowTooltip(typeof(MapMarker), new object[3] { base.TrackedObject, true, false });
	}

	protected override bool IsVisibleOnMap()
	{
		return base.TrackedObject.IsVisibleOnMap;
	}

	protected override bool GetCanToggleTrack()
	{
		return true;
	}

	protected override string GetTrackerType()
	{
		return "Default";
	}

	protected override IssueQuestFlags GetRelatedQuests()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		IssueQuestFlags result = (IssueQuestFlags)0;
		QuestBase val = ((IEnumerable<QuestBase>)Campaign.Current.QuestManager.Quests).FirstOrDefault((Func<QuestBase, bool>)((QuestBase q) => ((MBObjectBase)q).StringId == base.TrackedObject.QuestId));
		if (val != null)
		{
			result = (IssueQuestFlags)(val.IsSpecialQuest ? 4 : 2);
		}
		return result;
	}
}
