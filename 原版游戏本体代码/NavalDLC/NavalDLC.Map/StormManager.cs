using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Handlers;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace NavalDLC.Map;

public class StormManager : ICustomSystemManager
{
	[SaveableField(10)]
	private MBList<Storm> _spawnedStorms = new MBList<Storm>();

	public bool DebugVisualsEnabled;

	public bool DebugVisualsStopped;

	public MBReadOnlyList<Storm> SpawnedStorms => (MBReadOnlyList<Storm>)(object)_spawnedStorms;

	public StormManager()
	{
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)CampaignTick);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)HourlyTick);
	}

	private void HourlyTick()
	{
		for (int i = 0; i < ((List<Storm>)(object)_spawnedStorms).Count; i++)
		{
			((List<Storm>)(object)_spawnedStorms)[i].HourlyTick();
		}
	}

	private void CampaignTick(float campaignDt)
	{
		if (!(campaignDt > 0f))
		{
			return;
		}
		for (int num = ((List<Storm>)(object)_spawnedStorms).Count - 1; num >= 0; num--)
		{
			Storm storm = ((List<Storm>)(object)_spawnedStorms)[num];
			if (storm.IsReadyToBeFinalized)
			{
				storm.SetVisualDirty();
				((List<Storm>)(object)_spawnedStorms).RemoveAt(num);
			}
			else
			{
				storm.Tick(campaignDt);
			}
		}
		StormCollisionTick();
	}

	private void StormCollisionTick()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((List<Storm>)(object)_spawnedStorms).Count; i++)
		{
			for (int j = i + 1; j < ((List<Storm>)(object)_spawnedStorms).Count; j++)
			{
				Storm storm = ((List<Storm>)(object)_spawnedStorms)[i];
				Storm storm2 = ((List<Storm>)(object)_spawnedStorms)[j];
				Vec2 currentPosition = storm.CurrentPosition;
				if (((Vec2)(ref currentPosition)).Distance(storm2.CurrentPosition) < storm.EffectRadius + storm2.EffectRadius)
				{
					((storm.EffectRadius > storm2.EffectRadius) ? storm2 : storm).ForceDeactivate();
				}
			}
		}
	}

	public void CreateStormAtPosition(Vec2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Storm storm = new Storm(position, NavalDLCManager.Instance.GameModels.MapStormModel.GetSpawnedStormTypeForPosition(position));
		((List<Storm>)(object)_spawnedStorms).Add(storm);
		NavalDLCEvents.Instance.OnStormCreated(storm);
	}

	public void CreateStormAtPosition(Vec2 position, Storm.StormTypes stormType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		Storm storm = new Storm(position, stormType);
		((List<Storm>)(object)_spawnedStorms).Add(storm);
		NavalDLCEvents.Instance.OnStormCreated(storm);
	}

	public void OnAfterLoad()
	{
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)CampaignTick);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)HourlyTick);
		for (int i = 0; i < ((List<Storm>)(object)_spawnedStorms).Count; i++)
		{
			((List<Storm>)(object)_spawnedStorms)[i].OnAfterLoad();
		}
	}
}
