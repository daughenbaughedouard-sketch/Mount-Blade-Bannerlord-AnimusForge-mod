using System;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class RewardGoldOnDeathEffect : MPPerkEffect
{
	private enum OrderBy
	{
		Random = 0,
		WealthAscending = 1,
		WealthDescending = 2,
		DistanceAscending = 3,
		DistanceDescending = 4,
		DeadCanReceiveEnd = 3
	}

	protected static string StringType = "RewardGoldOnDeath";

	private int _value;

	private int _count;

	private OrderBy _orderBy;

	protected RewardGoldOnDeathEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPPerkEffectBase)this).IsDisabledInWarmup = (node?.Attributes?["is_disabled_in_warmup"]?.Value)?.ToLower() == "true";
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !int.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\RewardGoldOnDeathEffect.cs", "Deserialize", 41);
		}
		string text2 = node?.Attributes?["number_of_receivers"]?.Value;
		if (text2 == null || !int.TryParse(text2, out _count))
		{
			Debug.FailedAssert("provided 'number_of_receivers' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\RewardGoldOnDeathEffect.cs", "Deserialize", 47);
		}
		string text3 = node?.Attributes?["order_by"]?.Value;
		_orderBy = OrderBy.Random;
		if (text3 != null && !Enum.TryParse<OrderBy>(text3, ignoreCase: true, out _orderBy))
		{
			_orderBy = OrderBy.Random;
			Debug.FailedAssert("provided 'order_by' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\RewardGoldOnDeathEffect.cs", "Deserialize", 55);
		}
	}

	public override bool GetIsTeamRewardedOnDeath()
	{
		return true;
	}

	public override void CalculateRewardedGoldOnDeath(Agent agent, List<(MissionPeer, int)> teamMembers)
	{
		Agent obj = agent;
		if (((obj != null) ? obj.MissionPeer : null) == null)
		{
			Agent obj2 = agent;
			if (obj2 != null)
			{
				_ = obj2.OwningAgentMissionPeer;
			}
		}
		switch (_orderBy)
		{
		case OrderBy.WealthAscending:
			teamMembers.Sort(((MissionPeer, int) a, (MissionPeer, int) b) => a.Item1.Representative.Gold.CompareTo(b.Item1.Representative.Gold));
			break;
		case OrderBy.WealthDescending:
			teamMembers.Sort(((MissionPeer, int) a, (MissionPeer, int) b) => b.Item1.Representative.Gold.CompareTo(a.Item1.Representative.Gold));
			break;
		case OrderBy.DistanceAscending:
			teamMembers.Sort(((MissionPeer, int) a, (MissionPeer, int) b) => SortByDistance(agent, a.Item1.Representative.ControlledAgent, b.Item1.Representative.ControlledAgent));
			break;
		case OrderBy.DistanceDescending:
			teamMembers.Sort(((MissionPeer, int) a, (MissionPeer, int) b) => SortByDistance(agent, b.Item1.Representative.ControlledAgent, a.Item1.Representative.ControlledAgent));
			break;
		}
		int num = _count;
		for (int num2 = 0; num2 < teamMembers.Count && num > 0; num2++)
		{
			if (_orderBy >= OrderBy.DistanceAscending)
			{
				MissionRepresentativeBase representative = teamMembers[num2].Item1.Representative;
				if (representative == null)
				{
					continue;
				}
				Agent controlledAgent = representative.ControlledAgent;
				if (((controlledAgent != null) ? new bool?(controlledAgent.IsActive()) : ((bool?)null)) != true)
				{
					continue;
				}
			}
			(MissionPeer, int) value = teamMembers[num2];
			value.Item2 += _value;
			teamMembers[num2] = value;
			num--;
		}
	}

	private int SortByDistance(Agent from, Agent a, Agent b)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (a == null || !a.IsActive())
		{
			if (b != null && b.IsActive())
			{
				return 1;
			}
			return 0;
		}
		if (b == null || !b.IsActive())
		{
			return -1;
		}
		Vec3 position = a.Position;
		float num = ((Vec3)(ref position)).DistanceSquared(from.Position);
		position = b.Position;
		return num.CompareTo(((Vec3)(ref position)).DistanceSquared(from.Position));
	}
}
