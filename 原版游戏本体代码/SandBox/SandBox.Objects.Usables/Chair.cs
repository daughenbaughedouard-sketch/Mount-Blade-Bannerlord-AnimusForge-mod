using System.Collections.Generic;
using System.Linq;
using SandBox.AI;
using SandBox.Objects.AnimationPoints;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class Chair : UsableMachine
{
	public enum SittableType
	{
		Chair,
		Log,
		Sofa,
		Ground
	}

	public SittableType ChairType;

	protected override void OnInit()
	{
		((UsableMachine)this).OnInit();
		foreach (StandingPoint item in (List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)
		{
			item.AutoSheathWeapons = true;
		}
	}

	public bool IsAgentFullySitting(Agent usingAgent)
	{
		if (((List<StandingPoint>)(object)((UsableMachine)this).StandingPoints).Count > 0 && ((IEnumerable<UsableMissionObject>)((UsableMachine)this).StandingPoints).Contains(usingAgent.CurrentlyUsedGameObject))
		{
			return usingAgent.IsSitting();
		}
		return false;
	}

	public override UsableMachineAIBase CreateAIBehaviorObject()
	{
		return (UsableMachineAIBase)(object)new UsablePlaceAI((UsableMachine)(object)this);
	}

	public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		TextObject val = new TextObject(IsAgentFullySitting(Agent.Main) ? "{=QGdaakYW}{KEY} Get Up" : "{=bl2aRW8f}{KEY} Sit", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		return val;
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		return (TextObject)(ChairType switch
		{
			SittableType.Log => (object)new TextObject("{=9pgOGq7X}Log", (Dictionary<string, object>)null), 
			SittableType.Sofa => (object)new TextObject("{=GvLZKQ1U}Sofa", (Dictionary<string, object>)null), 
			SittableType.Ground => (object)new TextObject("{=L7ZQtIuM}Ground", (Dictionary<string, object>)null), 
			_ => (object)new TextObject("{=OgTUrRlR}Chair", (Dictionary<string, object>)null), 
		});
	}

	public override StandingPoint GetBestPointAlternativeTo(StandingPoint standingPoint, Agent agent)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		AnimationPoint animationPoint = standingPoint as AnimationPoint;
		if (animationPoint == null || animationPoint.GroupId < 0)
		{
			return (StandingPoint)(object)animationPoint;
		}
		WorldFrame userFrameForAgent = ((UsableMissionObject)standingPoint).GetUserFrameForAgent(agent);
		Vec3 groundVec = ((WorldPosition)(ref userFrameForAgent.Origin)).GetGroundVec3();
		float num = ((Vec3)(ref groundVec)).DistanceSquared(agent.Position);
		foreach (StandingPoint item in (List<StandingPoint>)(object)((UsableMachine)this).StandingPoints)
		{
			if (item is AnimationPoint animationPoint2 && standingPoint != item && animationPoint.GroupId == animationPoint2.GroupId && !((UsableMissionObject)animationPoint2).IsDisabledForAgent(agent))
			{
				userFrameForAgent = ((UsableMissionObject)animationPoint2).GetUserFrameForAgent(agent);
				groundVec = ((WorldPosition)(ref userFrameForAgent.Origin)).GetGroundVec3();
				float num2 = ((Vec3)(ref groundVec)).DistanceSquared(agent.Position);
				if (num2 < num)
				{
					num = num2;
					animationPoint = animationPoint2;
				}
			}
		}
		return (StandingPoint)(object)animationPoint;
	}

	public override OrderType GetOrder(BattleSideEnum side)
	{
		return (OrderType)0;
	}
}
