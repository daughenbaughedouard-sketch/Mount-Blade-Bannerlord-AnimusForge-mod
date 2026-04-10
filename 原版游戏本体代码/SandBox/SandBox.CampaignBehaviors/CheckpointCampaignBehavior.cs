using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace SandBox.CampaignBehaviors;

public class CheckpointCampaignBehavior : CampaignBehaviorBase
{
	public int LastUsedMissionCheckpointId = -1;

	public List<AgentSaveData> CorpseList = new List<AgentSaveData>();

	public override void RegisterEvents()
	{
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<int>("LastUsedMissionCheckpointId", ref LastUsedMissionCheckpointId);
		dataStore.SyncData<List<AgentSaveData>>("CorpseList", ref CorpseList);
	}
}
