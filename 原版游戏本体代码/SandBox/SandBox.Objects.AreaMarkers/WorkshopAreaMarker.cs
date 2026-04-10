using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Objects.AreaMarkers;

public class WorkshopAreaMarker : AreaMarker
{
	public override string Tag
	{
		get
		{
			Workshop workshop = GetWorkshop();
			if (workshop == null)
			{
				return null;
			}
			return ((SettlementArea)workshop).Tag;
		}
	}

	public Workshop GetWorkshop()
	{
		Workshop result = null;
		Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
		if (settlement != null && settlement.IsTown && settlement.Town.Workshops.Length > base.AreaIndex && base.AreaIndex > 0)
		{
			result = settlement.Town.Workshops[base.AreaIndex];
		}
		return result;
	}

	protected override void OnEditorTick(float dt)
	{
		((AreaMarker)this).OnEditorTick(dt);
		if (!MBEditor.HelpersEnabled() || !base.CheckToggle)
		{
			return;
		}
		float distanceSquared = base.AreaRadius * base.AreaRadius;
		List<GameEntity> list = new List<GameEntity>();
		((ScriptComponentBehavior)this).Scene.GetEntities(ref list);
		foreach (GameEntity item in list)
		{
			item.HasTag("shop_prop");
		}
		foreach (UsableMachine item2 in MBExtensions.FindAllWithType<UsableMachine>((IEnumerable<GameEntity>)list).Where(delegate(UsableMachine x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)x).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			return ((Vec3)(ref globalPosition)).DistanceSquared(((WeakGameEntity)(ref gameEntity)).GlobalPosition) <= distanceSquared;
		}).ToList())
		{
			_ = item2;
		}
	}

	public WorkshopType GetWorkshopType()
	{
		Workshop workshop = GetWorkshop();
		if (workshop == null)
		{
			return null;
		}
		return workshop.WorkshopType;
	}

	public override TextObject GetName()
	{
		Workshop workshop = GetWorkshop();
		if (workshop == null)
		{
			return null;
		}
		return ((SettlementArea)workshop).Name;
	}
}
