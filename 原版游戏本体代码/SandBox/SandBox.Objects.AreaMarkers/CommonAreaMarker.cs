using System.Collections.Generic;
using System.Linq;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Objects.AreaMarkers;

public class CommonAreaMarker : AreaMarker
{
	public string Type = "";

	public List<MatrixFrame> HiddenSpawnFrames { get; private set; }

	public override string Tag
	{
		get
		{
			Alley alley = GetAlley();
			if (alley == null)
			{
				return null;
			}
			return ((SettlementArea)alley).Tag;
		}
	}

	protected override void OnInit()
	{
		HiddenSpawnFrames = new List<MatrixFrame>();
	}

	public override List<UsableMachine> GetUsableMachinesInRange(string excludeTag = null)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		List<UsableMachine> usableMachinesInRange = ((AreaMarker)this).GetUsableMachinesInRange((string)null);
		WeakGameEntity gameEntity;
		for (int num = usableMachinesInRange.Count - 1; num >= 0; num--)
		{
			UsableMachine obj = usableMachinesInRange[num];
			gameEntity = ((ScriptComponentBehavior)obj).GameEntity;
			string[] tags = ((WeakGameEntity)(ref gameEntity)).Tags;
			gameEntity = ((ScriptComponentBehavior)obj).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).HasScriptOfType<Passage>() || (!tags.Contains("npc_common") && !tags.Contains("npc_common_limited") && !tags.Contains("sp_guard") && !tags.Contains("sp_guard_unarmed") && !tags.Contains("sp_notable")))
			{
				usableMachinesInRange.RemoveAt(num);
			}
		}
		List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTag("sp_common_hidden").ToList();
		GameEntity val = null;
		float num2 = float.MaxValue;
		float num3 = base.AreaRadius * base.AreaRadius;
		for (int num4 = list.Count - 1; num4 >= 0; num4--)
		{
			Vec3 globalPosition = list[num4].GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			float num5 = ((Vec3)(ref globalPosition)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition);
			if (num5 < num2)
			{
				val = list[num4];
				num2 = num5;
			}
			if (num5 < num3)
			{
				HiddenSpawnFrames.Add(list[num4].GetGlobalFrame());
			}
		}
		if (Extensions.IsEmpty<MatrixFrame>((IEnumerable<MatrixFrame>)HiddenSpawnFrames) && val != (GameEntity)null)
		{
			HiddenSpawnFrames.Add(val.GetGlobalFrame());
		}
		return usableMachinesInRange;
	}

	public Alley GetAlley()
	{
		Alley result = null;
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (settlement != null && ((settlement != null) ? settlement.Alleys : null) != null && base.AreaIndex > 0 && base.AreaIndex <= settlement.Alleys.Count)
		{
			result = settlement.Alleys[base.AreaIndex - 1];
		}
		return result;
	}

	public override TextObject GetName()
	{
		Alley alley = GetAlley();
		if (alley == null)
		{
			return null;
		}
		return ((SettlementArea)alley).Name;
	}
}
