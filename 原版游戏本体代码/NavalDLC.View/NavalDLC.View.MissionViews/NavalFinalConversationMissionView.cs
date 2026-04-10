using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Storyline;
using SandBox.Conversation.MissionLogics;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.View.MissionViews;

public class NavalFinalConversationMissionView : MissionView
{
	private const float FadeDuration = 0.5f;

	private CharacterObject _currentConversationCharacter;

	private float _remainingSisterSpawnTime = 0.6f;

	private bool _shouldSpawnSister;

	private bool _shouldStartSisterConversation;

	public override void OnMissionTick(float dt)
	{
		_currentConversationCharacter = Campaign.Current.ConversationManager.OneToOneConversationCharacter;
		if (_shouldStartSisterConversation && !ScreenFadeController.IsFadeActive)
		{
			Agent val = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => (object)x.Character == StoryModeHeroes.LittleSister.CharacterObject));
			MissionConversationLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionConversationLogic>();
			if (missionBehavior != null)
			{
				missionBehavior.StartConversation(val, false, false);
			}
			_shouldStartSisterConversation = false;
		}
		if (_shouldSpawnSister && _remainingSisterSpawnTime > 0f)
		{
			_remainingSisterSpawnTime -= dt;
			if (_remainingSisterSpawnTime <= 0f)
			{
				TransitionToSister();
				_shouldSpawnSister = false;
			}
		}
	}

	public override void OnConversationEnd()
	{
		if (_currentConversationCharacter == NavalStorylineData.Gunnar.CharacterObject)
		{
			ScreenFadeController.BeginFadeOutAndIn(0.5f, 0.5f, 0.5f);
			_shouldSpawnSister = true;
		}
	}

	private void TransitionToSister()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		AgentBuildData val = new AgentBuildData((BasicCharacterObject)(object)StoryModeHeroes.LittleSister.CharacterObject);
		val.TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin(val.AgentCharacter, -1, (Banner)null, default(UniqueTroopDescriptor)));
		Agent? obj = ((IEnumerable<Agent>)Mission.Current.Agents).FirstOrDefault((Func<Agent, bool>)((Agent x) => (object)x.Character == NavalStorylineData.Gunnar.CharacterObject));
		Vec3 position = obj.Position;
		val.InitialPosition(ref position);
		Vec3 lookDirection = Agent.Main.LookDirection;
		Vec2 val2 = ((Vec3)(ref lookDirection)).AsVec2;
		val2 = -((Vec2)(ref val2)).Normalized();
		val.InitialDirection(ref val2);
		val.NoHorses(true);
		val.CivilianEquipment(true);
		Mission.Current.SpawnAgent(val, false);
		obj.FadeOut(true, true);
		_shouldStartSisterConversation = true;
	}
}
