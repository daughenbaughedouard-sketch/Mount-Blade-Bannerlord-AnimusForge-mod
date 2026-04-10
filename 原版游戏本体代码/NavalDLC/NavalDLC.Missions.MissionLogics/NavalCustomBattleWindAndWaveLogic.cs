using System;
using System.Collections.Generic;
using NavalDLC.Missions.Deployment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.MissionLogics;

public class NavalCustomBattleWindAndWaveLogic : MissionLogic
{
	private NavalCustomBattleWindConfig.Direction _windDirection;

	private TerrainType _terrainType;

	private NavalDeploymentMissionController _navalDeploymentMissionController;

	public NavalCustomBattleWindAndWaveLogic(NavalCustomBattleWindConfig.Direction windDirection, TerrainType terrainType)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		_windDirection = windDirection;
		_terrainType = terrainType;
	}

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_navalDeploymentMissionController = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalDeploymentMissionController>();
		((DeploymentMissionController)_navalDeploymentMissionController).OnAfterSetupTeams += OnAfterSetupTeams;
	}

	public override void OnRemoveBehavior()
	{
		((MissionBehavior)this).OnRemoveBehavior();
		((DeploymentMissionController)_navalDeploymentMissionController).OnAfterSetupTeams -= OnAfterSetupTeams;
	}

	public override void AfterStart()
	{
	}

	public void OnAfterSetupTeams()
	{
		UpdateSceneWindDirection();
		UpdateSceneWaterStrength();
	}

	private void UpdateSceneWindDirection()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = Vec2.Zero;
		Vec2 val2 = Vec2.Zero;
		int num = 0;
		int num2 = 0;
		foreach (Team item in (List<Team>)(object)Mission.Current.Teams)
		{
			MatrixFrame deploymentFrame;
			if ((int)item.Side == 1)
			{
				Vec2 val3 = val;
				deploymentFrame = ((MissionBehavior)this).Mission.DeploymentPlan.GetDeploymentFrame(item);
				val = val3 + ((Vec3)(ref deploymentFrame.origin)).AsVec2;
				num++;
			}
			else if ((int)item.Side == 0)
			{
				Vec2 val4 = val2;
				deploymentFrame = ((MissionBehavior)this).Mission.DeploymentPlan.GetDeploymentFrame(item);
				val2 = val4 + ((Vec3)(ref deploymentFrame.origin)).AsVec2;
				num2++;
			}
		}
		val /= (float)num;
		val2 /= (float)num2;
		Vec2 val5 = val2 - val;
		Vec2 val6 = ((Vec2)(ref val5)).Normalized();
		val5 = Mission.Current.Scene.GetGlobalWindVelocity();
		float length = ((Vec2)(ref val5)).Length;
		Vec2 val7 = length * val6;
		switch (_windDirection)
		{
		case NavalCustomBattleWindConfig.Direction.TowardsDefender:
			((Vec2)(ref val7)).RotateCCW(-MathF.PI / 6f);
			break;
		case NavalCustomBattleWindConfig.Direction.TowardsAttacker:
			val7 *= -1f;
			((Vec2)(ref val7)).RotateCCW(-MathF.PI / 6f);
			break;
		case NavalCustomBattleWindConfig.Direction.Side:
		{
			Vec3 val8 = Vec3.CrossProduct(Vec3.Up, ((Vec2)(ref val6)).ToVec3(0f));
			val7 = ((Vec3)(ref val8)).AsVec2 * length;
			break;
		}
		case NavalCustomBattleWindConfig.Direction.Random:
			val5 = new Vec2(MBRandom.RandomFloatNormal, MBRandom.RandomFloatNormal);
			val7 = length * ((Vec2)(ref val5)).Normalized();
			break;
		}
		Mission.Current.Scene.SetGlobalWindVelocity(ref val7);
	}

	private void UpdateSceneWaterStrength()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)_terrainType == 11)
		{
			Mission.Current.Scene.SetWaterStrength(0.5f);
		}
	}
}
