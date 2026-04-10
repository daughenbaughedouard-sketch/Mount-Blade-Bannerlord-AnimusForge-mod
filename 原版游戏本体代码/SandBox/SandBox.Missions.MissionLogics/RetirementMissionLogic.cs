using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class RetirementMissionLogic : MissionLogic
{
	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		SpawnHermit();
		_ = (LeaveMissionLogic)(object)((IEnumerable<MissionLogic>)((MissionBehavior)this).Mission.MissionLogics).FirstOrDefault((Func<MissionLogic, bool>)((MissionLogic x) => x is LeaveMissionLogic));
	}

	private void SpawnHermit()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		List<GameEntity> list = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("sp_hermit").ToList();
		MatrixFrame globalFrame = list[MBRandom.RandomInt(list.Count())].GetGlobalFrame();
		CharacterObject val = ((GameType)Campaign.Current).ObjectManager.GetObject<CharacterObject>("sp_hermit");
		AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(((MissionBehavior)this).Mission.SpectatorTeam).InitialPosition(ref globalFrame.origin);
		Vec2 val2 = ((Vec3)(ref globalFrame.rotation.f)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		AgentBuildData val3 = obj.InitialDirection(ref val2).CivilianEquipment(true).NoHorses(true)
			.NoWeapons(true)
			.ClothingColor1(((MissionBehavior)this).Mission.PlayerTeam.Color)
			.ClothingColor2(((MissionBehavior)this).Mission.PlayerTeam.Color2);
		((MissionBehavior)this).Mission.SpawnAgent(val3, false).SetMortalityState((MortalityState)1);
	}
}
