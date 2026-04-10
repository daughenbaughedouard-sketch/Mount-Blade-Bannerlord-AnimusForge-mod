using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions;

namespace SandBox;

public class WoundAllEnemiesCheat : GameplayCheatItem
{
	public override void ExecuteCheat()
	{
		KillAllEnemies();
	}

	private void KillAllEnemies()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Mission current = Mission.Current;
		AgentReadOnlyList val = ((current != null) ? current.Agents : null);
		Mission current2 = Mission.Current;
		Agent val2 = ((current2 != null) ? current2.MainAgent : null);
		Mission current3 = Mission.Current;
		Team val3 = ((current3 != null) ? current3.PlayerTeam : null);
		if (val == null || val3 == null)
		{
			return;
		}
		for (int num = ((List<Agent>)(object)val).Count - 1; num >= 0; num--)
		{
			Agent val4 = ((List<Agent>)(object)val)[num];
			if (val4 != val2 && Extensions.HasAnyFlag<AgentFlag>(val4.GetAgentFlags(), (AgentFlag)8) && val3 != null && val4.Team != null && val4.Team.IsValid && val3.IsEnemyOf(val4.Team))
			{
				KillAgent(val4);
			}
		}
	}

	private void KillAgent(Agent agent)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		Mission current = Mission.Current;
		Agent val = ((current != null) ? current.MainAgent : null) ?? agent;
		Blow val2 = default(Blow);
		((Blow)(ref val2))._002Ector(val.Index);
		val2.DamageType = (DamageTypes)2;
		val2.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
		val2.GlobalPosition = agent.Position;
		val2.GlobalPosition.z += agent.GetEyeGlobalHeight();
		val2.BaseMagnitude = 2000f;
		((BlowWeaponRecord)(ref val2.WeaponRecord)).FillAsMeleeBlow((ItemObject)null, (WeaponComponentData)null, -1, (sbyte)(-1));
		val2.InflictedDamage = 2000;
		val2.SwingDirection = agent.LookDirection;
		val2.Direction = val2.SwingDirection;
		val2.DamageCalculated = true;
		sbyte mainHandItemBoneIndex = val.Monster.MainHandItemBoneIndex;
		AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, (CombatCollisionResult)1, -1, 0, 2, val2.BoneIndex, (BoneBodyPartType)0, mainHandItemBoneIndex, (UsageDirection)2, -1, (CombatHitResultFlags)0, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, val2.Direction, val2.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
		agent.RegisterBlow(val2, ref attackCollisionDataForDebugPurpose);
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=FJ93PXVa}Wound All Enemies", (Dictionary<string, object>)null);
	}
}
