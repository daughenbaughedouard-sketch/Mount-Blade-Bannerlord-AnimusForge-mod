using System.Collections.ObjectModel;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;

namespace SandBox.ViewModelCollection.Missions.NameMarker;

public abstract class MissionNameMarkerTargetVM<T> : MissionNameMarkerTargetBaseVM
{
	public T Target { get; private set; }

	protected MissionNameMarkerTargetVM(T target)
	{
		Target = target;
	}

	public override bool Equals(MissionNameMarkerTargetBaseVM other)
	{
		if (other is MissionNameMarkerTargetVM<T> { Target: var target } missionNameMarkerTargetVM && target.Equals(Target) && AreQuestsEqual(missionNameMarkerTargetVM))
		{
			return base.IsPersistent == missionNameMarkerTargetVM.IsPersistent;
		}
		return false;
	}

	private bool AreQuestsEqual(MissionNameMarkerTargetVM<T> tOther)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (tOther.Quests != null && base.Quests != null)
		{
			if (((Collection<QuestMarkerVM>)(object)tOther.Quests).Count != ((Collection<QuestMarkerVM>)(object)base.Quests).Count)
			{
				return false;
			}
			for (int i = 0; i < ((Collection<QuestMarkerVM>)(object)base.Quests).Count; i++)
			{
				QuestMarkerVM val = ((Collection<QuestMarkerVM>)(object)base.Quests)[i];
				QuestMarkerVM val2 = ((Collection<QuestMarkerVM>)(object)tOther.Quests)[i];
				if (val.IssueQuestFlag != val2.IssueQuestFlag || val.QuestMarkerType != val2.QuestMarkerType)
				{
					return false;
				}
			}
			return true;
		}
		if (tOther.Quests == null && base.Quests == null)
		{
			return true;
		}
		return false;
	}
}
