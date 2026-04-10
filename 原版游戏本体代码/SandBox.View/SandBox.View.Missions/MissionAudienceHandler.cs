using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionAudienceHandler : MissionView
{
	private const int GapBetweenCheerSmallInSeconds = 10;

	private const int GapBetweenCheerMedium = 4;

	private float _minChance;

	private float _maxChance;

	private float _minDist;

	private float _maxDist;

	private float _minHeight;

	private float _maxHeight;

	private List<GameEntity> _audienceMidPoints;

	private List<KeyValuePair<GameEntity, float>> _audienceList;

	private readonly float _density;

	private GameEntity _arenaSoundEntity;

	private SoundEvent _ambientSoundEvent;

	private MissionTime _lastOneShotSoundEventStarted;

	private bool _allOneShotSoundEventsAreDisabled;

	public MissionAudienceHandler(float density)
	{
		_density = density;
	}

	public override void EarlyStart()
	{
		_allOneShotSoundEventsAreDisabled = true;
		_audienceMidPoints = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("audience_mid_point").ToList();
		_arenaSoundEntity = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("arena_sound");
		_audienceList = new List<KeyValuePair<GameEntity, float>>();
		if (_audienceMidPoints.Count > 0)
		{
			OnInit();
		}
	}

	public void OnInit()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		_minChance = MathF.Max(_density - 0.5f, 0f);
		_maxChance = _density;
		GetAudienceEntities();
		SpawnAudienceAgents();
		_lastOneShotSoundEventStarted = MissionTime.Zero;
		_allOneShotSoundEventsAreDisabled = false;
		_ambientSoundEvent = SoundManager.CreateEvent("event:/mission/ambient/detail/arena/arena", ((MissionBehavior)this).Mission.Scene);
		_ambientSoundEvent.Play();
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		if (affectorAgent != null && affectorAgent.IsHuman && affectedAgent.IsHuman)
		{
			Cheer();
		}
	}

	private void Cheer(bool onEnd = false)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!_allOneShotSoundEventsAreDisabled)
		{
			string text = null;
			if (onEnd)
			{
				text = "event:/mission/ambient/detail/arena/cheer_big";
				_allOneShotSoundEventsAreDisabled = true;
			}
			else if (((MissionTime)(ref _lastOneShotSoundEventStarted)).ElapsedSeconds > 4f && ((MissionTime)(ref _lastOneShotSoundEventStarted)).ElapsedSeconds < 10f)
			{
				text = "event:/mission/ambient/detail/arena/cheer_medium";
			}
			else if (((MissionTime)(ref _lastOneShotSoundEventStarted)).ElapsedSeconds > 10f)
			{
				text = "event:/mission/ambient/detail/arena/cheer_small";
			}
			if (text != null)
			{
				Vec3 val = ((_arenaSoundEntity != (GameEntity)null) ? _arenaSoundEntity.GlobalPosition : (_audienceMidPoints.Any() ? Extensions.GetRandomElement<GameEntity>((IReadOnlyList<GameEntity>)_audienceMidPoints).GlobalPosition : Vec3.Zero));
				SoundManager.StartOneShotEvent(text, ref val);
				_lastOneShotSoundEventStarted = MissionTime.Now;
			}
		}
	}

	private void GetAudienceEntities()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		_maxDist = 0f;
		_minDist = float.MaxValue;
		_maxHeight = 0f;
		_minHeight = float.MaxValue;
		foreach (GameEntity item in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("audience"))
		{
			float distanceSquareToArena = GetDistanceSquareToArena(item);
			_maxDist = ((distanceSquareToArena > _maxDist) ? distanceSquareToArena : _maxDist);
			_minDist = ((distanceSquareToArena < _minDist) ? distanceSquareToArena : _minDist);
			float z = item.GetFrame().origin.z;
			_maxHeight = ((z > _maxHeight) ? z : _maxHeight);
			_minHeight = ((z < _minHeight) ? z : _minHeight);
			_audienceList.Add(new KeyValuePair<GameEntity, float>(item, distanceSquareToArena));
			item.SetVisibilityExcludeParents(false);
		}
	}

	private float GetDistanceSquareToArena(GameEntity audienceEntity)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		foreach (GameEntity audienceMidPoint in _audienceMidPoints)
		{
			Vec3 globalPosition = audienceMidPoint.GlobalPosition;
			float num2 = ((Vec3)(ref globalPosition)).DistanceSquared(audienceEntity.GlobalPosition);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	private CharacterObject GetRandomAudienceCharacterToSpawn()
	{
		Settlement currentSettlement = Settlement.CurrentSettlement;
		CharacterObject val = MBRandom.ChooseWeighted<CharacterObject>((IReadOnlyList<ValueTuple<CharacterObject, float>>)new List<(CharacterObject, float)>
		{
			(currentSettlement.Culture.Townswoman, 0.2f),
			(currentSettlement.Culture.Townsman, 0.2f),
			(currentSettlement.Culture.Armorer, 0.1f),
			(currentSettlement.Culture.Merchant, 0.1f),
			(currentSettlement.Culture.Musician, 0.1f),
			(currentSettlement.Culture.Weaponsmith, 0.1f),
			(currentSettlement.Culture.RansomBroker, 0.1f),
			(currentSettlement.Culture.Barber, 0.05f),
			(currentSettlement.Culture.FemaleDancer, 0.05f)
		});
		if (val == null)
		{
			val = ((MBRandom.RandomFloat < 0.65f) ? currentSettlement.Culture.Townsman : currentSettlement.Culture.Townswoman);
		}
		return val;
	}

	private void SpawnAudienceAgents()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _audienceList.Count - 1; num >= 0; num--)
		{
			KeyValuePair<GameEntity, float> keyValuePair = _audienceList[num];
			float num2 = _minChance + (1f - (keyValuePair.Value - _minDist) / (_maxDist - _minDist)) * (_maxChance - _minChance);
			float num3 = _minChance + (1f - MathF.Pow((keyValuePair.Key.GetFrame().origin.z - _minHeight) / (_maxHeight - _minHeight), 2f)) * (_maxChance - _minChance);
			float num4 = num2 * 0.4f + num3 * 0.6f;
			if (MBRandom.RandomFloat < num4)
			{
				MatrixFrame globalFrame = keyValuePair.Key.GetGlobalFrame();
				CharacterObject randomAudienceCharacterToSpawn = GetRandomAudienceCharacterToSpawn();
				AgentBuildData obj = new AgentBuildData((BasicCharacterObject)(object)randomAudienceCharacterToSpawn).InitialPosition(ref globalFrame.origin);
				Vec2 val = new Vec2(0f - ((Vec3)(ref globalFrame.rotation.f)).AsVec2.x, 0f - ((Vec3)(ref globalFrame.rotation.f)).AsVec2.y);
				AgentBuildData val2 = obj.InitialDirection(ref val).TroopOrigin((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)randomAudienceCharacterToSpawn, -1, (Banner)null, default(UniqueTroopDescriptor))).Team(Team.Invalid)
					.ClothingColor1(Settlement.CurrentSettlement.MapFaction.Color)
					.ClothingColor2(Settlement.CurrentSettlement.MapFaction.Color2)
					.CanSpawnOutsideOfMissionBoundary(true);
				Agent obj2 = Mission.Current.SpawnAgent(val2, false);
				MBActionSet actionSetWithIndex = MBActionSet.GetActionSetWithIndex(0);
				AnimationSystemData val3 = MonsterExtensions.FillAnimationSystemData(val2.AgentMonster, actionSetWithIndex, ((BasicCharacterObject)randomAudienceCharacterToSpawn).GetStepSize(), false);
				obj2.SetActionSet(ref val3);
				MBAnimation.PrefetchAnimationClip(obj2.ActionSet, ActionIndexCache.act_arena_spectator);
				obj2.SetActionChannel(0, ref ActionIndexCache.act_arena_spectator, true, (AnimFlags)0, 0f, MBRandom.RandomFloatRanged(0.75f, 1f), -0.2f, 0.4f, MBRandom.RandomFloatRanged(0.01f, 1f), false, -0.2f, 0, true);
				obj2.Controller = (AgentControllerType)0;
				obj2.ToggleInvulnerable();
			}
		}
	}

	public override void OnMissionTick(float dt)
	{
		if (_audienceMidPoints != null && ((MissionBehavior)this).Mission.MissionEnded)
		{
			Cheer(onEnd: true);
		}
	}

	public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)oldMissionMode == 2 && (int)Mission.Current.Mode == 0 && Agent.Main != null && Agent.Main.IsActive())
		{
			Cheer(onEnd: true);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		SoundEvent ambientSoundEvent = _ambientSoundEvent;
		if (ambientSoundEvent != null)
		{
			ambientSoundEvent.Release();
		}
	}
}
