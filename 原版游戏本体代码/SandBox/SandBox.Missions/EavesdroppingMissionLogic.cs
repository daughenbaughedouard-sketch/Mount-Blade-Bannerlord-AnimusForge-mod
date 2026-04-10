using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.Objects.Usables;

namespace SandBox.Missions;

public class EavesdroppingMissionLogic : MissionLogic
{
	public class EavesdropSound
	{
		public TextObject Line;

		public int Priority;

		public CharacterObject Character;

		public string SoundPath;

		public EavesdropSound(TextObject line, int priority, CharacterObject character, string soundPath)
		{
			Line = line;
			Priority = priority;
			Character = character;
			SoundPath = BasePath.Name + "Modules/StoryMode/ModuleData/Languages/" + soundPath + ".ogg";
		}
	}

	private const string EavesdroppingPointTag = "eavesdropping_point";

	private const string CustomCameraTag = "customcamera";

	private const string StartEavesdroppingEventId = "start_eavesdropping";

	private readonly Dictionary<EventTriggeringUsableMachine, Camera> _eavesdroppingPoints = new Dictionary<EventTriggeringUsableMachine, Camera>();

	private readonly Queue<EavesdropSound> _eavesdropSoundQueue = new Queue<EavesdropSound>();

	private SoundEvent _currentSoundEvent;

	private Timer _waitTimer;

	public bool EavesdropStarted;

	public Camera CurrentEavesdroppingCamera;

	private EventTriggeringUsableMachine _currentEventTriggeringUsableMachine;

	private readonly CharacterObject _disguiseShadowingTargetCharacter;

	private readonly CharacterObject _disguiseOfficerCharacter;

	public EavesdroppingMissionLogic(CharacterObject disguiseShadowingTargetCharacter, CharacterObject disguiseOfficerCharacter)
	{
		_disguiseShadowingTargetCharacter = disguiseShadowingTargetCharacter;
		_disguiseOfficerCharacter = disguiseOfficerCharacter;
		Game.Current.EventManager.RegisterEvent<GenericMissionEvent>((Action<GenericMissionEvent>)OnGenericMissionEventTriggered);
	}

	protected override void OnEndMission()
	{
		Game.Current.EventManager.UnregisterEvent<GenericMissionEvent>((Action<GenericMissionEvent>)OnGenericMissionEventTriggered);
	}

	private void OnGenericMissionEventTriggered(GenericMissionEvent missionEvent)
	{
		if (!EavesdropStarted && missionEvent.EventId == "start_eavesdropping")
		{
			string[] array = missionEvent.Parameter.Split(new char[1] { ' ' });
			GameEntity val = Mission.Current.Scene.FindEntityWithTag(array[0]);
			StartEavesdropping(val.GetFirstScriptOfType<EventTriggeringUsableMachine>());
		}
	}

	private void StartEavesdropping(EventTriggeringUsableMachine eventTriggeringUsableMachine)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		_eavesdropSoundQueue.Enqueue(new EavesdropSound(new TextObject("{=YAWCkOYa}The tracks look fresh, and I've seen some smoke on the horizon. They can't move too quickly if they're still looting and raiding. No, I'm pretty sure we'll be able to rescue the little ones... or die trying.", (Dictionary<string, object>)null), 0, _disguiseShadowingTargetCharacter, "VoicedLines/EN/PC/tutorial_npc_brother_009"));
		_eavesdropSoundQueue.Enqueue(new EavesdropSound(new TextObject("{=R5kLv5kg}I am what they call Palaic. Palaic is a language that is no longer spoken, except by a few old people. Even the word 'Palaic' is imperial. We are a people who have forgotten who we are.[if:convo_focused_voice]", (Dictionary<string, object>)null), 0, _disguiseOfficerCharacter, "VoicedLines/EN/PC/storymode_imperial_mentor_arzagos_009"));
		_eavesdropSoundQueue.Enqueue(new EavesdropSound(new TextObject("{=phavdGYA}Are you sure about that?", (Dictionary<string, object>)null), 0, _disguiseShadowingTargetCharacter, "VoicedLines/EN/PC/tutorial_npc_brother_005"));
		_eavesdropSoundQueue.Enqueue(new EavesdropSound(new TextObject("{=dPb2Vph3}My informants will tell me once you pledged your support...[ib:normal2][if:convo_nonchalant]", (Dictionary<string, object>)null), 0, _disguiseOfficerCharacter, "VoicedLines/EN/PC/storymode_imperial_mentor_arzagos_044"));
		_eavesdropSoundQueue.Enqueue(new EavesdropSound(new TextObject("{=9ACSEvzD}Let's go on then.", (Dictionary<string, object>)null), 0, _disguiseShadowingTargetCharacter, "VoicedLines/EN/PC/tutorial_npc_brother_004"));
		_waitTimer = new Timer(((MissionBehavior)this).Mission.CurrentTime, 1.7f, true);
		EavesdropStarted = true;
		CurrentEavesdroppingCamera = _eavesdroppingPoints[eventTriggeringUsableMachine];
		_currentEventTriggeringUsableMachine = eventTriggeringUsableMachine;
	}

	public override void AfterStart()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		List<GameEntity> list = new List<GameEntity>();
		Mission.Current.Scene.GetAllEntitiesWithScriptComponent<EventTriggeringUsableMachine>(ref list);
		foreach (GameEntity item in list)
		{
			if (item.HasTag("eavesdropping_point"))
			{
				EventTriggeringUsableMachine firstScriptOfType = item.GetFirstScriptOfType<EventTriggeringUsableMachine>();
				Vec3 invalid = Vec3.Invalid;
				Camera val = Camera.CreateCamera();
				item.GetFirstChildEntityWithTag("customcamera").GetCameraParamsFromCameraScript(val, ref invalid);
				val.SetFovVertical(val.GetFovVertical(), Screen.AspectRatio, val.Near, val.Far);
				_eavesdroppingPoints.Add(firstScriptOfType, val);
			}
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!EavesdropStarted)
		{
			return;
		}
		Timer waitTimer = _waitTimer;
		if (waitTimer == null || !waitTimer.Check(((MissionBehavior)this).Mission.CurrentTime) || (_currentSoundEvent != null && _currentSoundEvent.IsPlaying()))
		{
			return;
		}
		SoundEvent currentSoundEvent = _currentSoundEvent;
		if (currentSoundEvent != null)
		{
			currentSoundEvent.Stop();
		}
		if (Extensions.IsEmpty<EavesdropSound>((IEnumerable<EavesdropSound>)_eavesdropSoundQueue))
		{
			_waitTimer = null;
			EavesdropStarted = false;
			CurrentEavesdroppingCamera = null;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_currentEventTriggeringUsableMachine).GameEntity;
			foreach (GenericMissionEventScript scriptComponent in ((WeakGameEntity)(ref gameEntity)).GetScriptComponents<GenericMissionEventScript>())
			{
				if (scriptComponent.EventId == "start_eavesdropping")
				{
					scriptComponent.IsDisabled = true;
				}
			}
			for (int i = 0; i < ((List<StandingPoint>)(object)((UsableMachine)_currentEventTriggeringUsableMachine).StandingPoints).Count; i++)
			{
				if (((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)_currentEventTriggeringUsableMachine).StandingPoints)[i]).HasUser)
				{
					((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)_currentEventTriggeringUsableMachine).StandingPoints)[i]).UserAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
				}
			}
			_currentEventTriggeringUsableMachine = null;
		}
		else
		{
			EavesdropSound eavesdropSound = _eavesdropSoundQueue.Dequeue();
			MBInformationManager.AddQuickInformation(eavesdropSound.Line, eavesdropSound.Priority, (BasicCharacterObject)(object)eavesdropSound.Character, (Equipment)null, "");
			_currentSoundEvent = SoundEvent.CreateEventFromExternalFile("event:/Extra/voiceover", eavesdropSound.SoundPath, Mission.Current.Scene, true, false);
			_currentSoundEvent.Play();
		}
	}
}
