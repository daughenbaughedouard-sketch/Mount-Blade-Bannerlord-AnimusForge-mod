using System;
using SandBox.Conversation.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionConversationCameraView : MissionView
{
	private const string CustomCameraMultiAgentTag = "custom_camera_multi_agent";

	private MissionMainAgentController _missionMainAgentController;

	private ConversationMissionLogic _conversationMissionLogic;

	private Camera _customConversationCamera;

	private GameEntity _customMultiAgentConversationCameraEntity;

	private Agent _speakerAgent;

	private Agent _listenerAgent;

	public bool IsCameraOverridden => (NativeObject)(object)_customConversationCamera != (NativeObject)null;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_conversationMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<ConversationMissionLogic>();
	}

	public override void AfterStart()
	{
		_missionMainAgentController = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMainAgentController>();
		_customMultiAgentConversationCameraEntity = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("custom_camera_multi_agent");
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		((MissionView)this).OnMissionScreenTick(dt);
		if (_conversationMissionLogic == null || !_conversationMissionLogic.IsMultiAgentConversation)
		{
			return;
		}
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		if (conversationManager.ConversationAgents == null || conversationManager.ConversationAgents.Count <= 0)
		{
			return;
		}
		Agent speakerAgent = (Agent)conversationManager.SpeakerAgent;
		Agent listenerAgent = (Agent)conversationManager.ListenerAgent;
		if (_speakerAgent == null || _listenerAgent == null)
		{
			return;
		}
		_speakerAgent = speakerAgent;
		_listenerAgent = listenerAgent;
		if (_speakerAgent != Agent.Main && _listenerAgent != Agent.Main)
		{
			if ((NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera == (NativeObject)null)
			{
				GameEntity customMultiAgentConversationCameraEntity = _customMultiAgentConversationCameraEntity;
				Vec3 invalid = Vec3.Invalid;
				Camera val = Camera.CreateCamera();
				customMultiAgentConversationCameraEntity.GetCameraParamsFromCameraScript(val, ref invalid);
				val.SetFovVertical(val.GetFovVertical(), Screen.AspectRatio, val.Near, val.Far);
				((MissionView)this).MissionScreen.CustomCamera = val;
			}
			UpdateAgentLooksForConversation();
		}
		else
		{
			((MissionView)this).MissionScreen.CustomCamera = null;
		}
	}

	public override bool UpdateOverridenCamera(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		MissionMode mode = ((MissionBehavior)this).Mission.Mode;
		if (((int)mode == 1 || (int)mode == 5) && !((MissionView)this).MissionScreen.IsCheatGhostMode)
		{
			if (_conversationMissionLogic?.CustomConversationCameraEntity != (GameEntity)null)
			{
				if ((NativeObject)(object)_customConversationCamera == (NativeObject)null && _conversationMissionLogic?.CustomConversationCameraEntity != (GameEntity)null)
				{
					Vec3 invalid = Vec3.Invalid;
					_customConversationCamera = Camera.CreateCamera();
					ConversationMissionLogic conversationMissionLogic = _conversationMissionLogic;
					if (conversationMissionLogic != null)
					{
						conversationMissionLogic.CustomConversationCameraEntity.GetCameraParamsFromCameraScript(_customConversationCamera, ref invalid);
					}
					_customConversationCamera.SetFovVertical(_customConversationCamera.GetFovVertical(), Screen.AspectRatio, _customConversationCamera.Near, _customConversationCamera.Far);
				}
				SetConversationLookToPointOfInterest(_customConversationCamera.Position);
				((MissionView)this).MissionScreen.CustomCamera = _customConversationCamera;
			}
			else
			{
				UpdateAgentLooksForConversation();
			}
		}
		else
		{
			Vec3 customLookDir = _missionMainAgentController.CustomLookDir;
			if (((Vec3)(ref customLookDir)).IsNonZero)
			{
				_missionMainAgentController.CustomLookDir = Vec3.Zero;
			}
		}
		return false;
	}

	private void UpdateAgentLooksForConversation()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		Agent val = null;
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		if (conversationManager.ConversationAgents != null && conversationManager.ConversationAgents.Count > 0)
		{
			_speakerAgent = (Agent)conversationManager.SpeakerAgent;
			_listenerAgent = (Agent)conversationManager.ListenerAgent;
			val = Agent.Main.GetLookAgent();
			if (_speakerAgent == null)
			{
				return;
			}
			foreach (IAgent conversationAgent in conversationManager.ConversationAgents)
			{
				if ((object)conversationAgent != _speakerAgent)
				{
					MakeAgentLookToSpeaker((Agent)conversationAgent);
				}
			}
			MakeSpeakerLookToListener();
		}
		SetFocusedObjectForCameraFocus();
		if (Agent.Main.GetLookAgent() != val && _speakerAgent != null)
		{
			SpeakerAgentIsChanged();
		}
	}

	private void SpeakerAgentIsChanged()
	{
		Mission.Current.ConversationCharacterChanged();
	}

	private void SetFocusedObjectForCameraFocus()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val5;
		if ((NativeObject)(object)((MissionView)this).MissionScreen.CustomCamera != (NativeObject)null)
		{
			Agent val = _speakerAgent;
			Agent val2 = _listenerAgent;
			MatrixFrame identity = MatrixFrame.Identity;
			Vec3 val3 = val2.Position - val.Position;
			((Vec3)(ref val3)).RotateAboutZ(-MathF.PI / 3f);
			val3 += val.Position;
			Vec3 val4 = val.Position - val2.Position;
			((Vec3)(ref val4)).RotateAboutZ(-MathF.PI / 3f);
			val4 += val2.Position;
			if (((Vec3)(ref val3)).Distance(Agent.Main.Position) > ((Vec3)(ref val4)).Distance(Agent.Main.Position))
			{
				val = _listenerAgent;
				val2 = _speakerAgent;
				val3 = val4;
			}
			val3.z += Agent.Main.GetEyeGlobalHeight();
			val5 = (val2.Position - val.Position) / 2f + val.Position - val3;
			Vec3 u = -((Vec3)(ref val5)).NormalizedCopy();
			identity.origin = val3;
			identity.rotation.s = Vec3.Side;
			identity.rotation.f = Vec3.Up;
			identity.rotation.u = u;
			((Mat3)(ref identity.rotation)).Orthonormalize();
			((MissionView)this).MissionScreen.CustomCamera.SetFovHorizontal(MathF.PI / 2f, Screen.AspectRatio, 0.1f, 2000f);
			((MissionView)this).MissionScreen.CustomCamera.Frame = identity;
			Agent.Main.AgentVisuals.SetVisible(false);
		}
		else if (_speakerAgent == Agent.Main)
		{
			_missionMainAgentController.InteractionComponent.SetCurrentFocusedObject((IFocusable)(object)_listenerAgent, (IFocusable)null, (sbyte)(-1), true);
			MissionMainAgentController missionMainAgentController = _missionMainAgentController;
			val5 = _listenerAgent.Position - Agent.Main.Position;
			missionMainAgentController.CustomLookDir = ((Vec3)(ref val5)).NormalizedCopy();
			Agent.Main.SetLookAgent(_listenerAgent);
		}
		else
		{
			_missionMainAgentController.InteractionComponent.SetCurrentFocusedObject((IFocusable)(object)_speakerAgent, (IFocusable)null, (sbyte)(-1), true);
			MissionMainAgentController missionMainAgentController2 = _missionMainAgentController;
			val5 = _speakerAgent.Position - Agent.Main.Position;
			missionMainAgentController2.CustomLookDir = ((Vec3)(ref val5)).NormalizedCopy();
			Agent.Main.SetLookAgent(_speakerAgent);
		}
	}

	private void MakeAgentLookToSpeaker(Agent agent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = agent.Position;
		Vec3 position2 = _speakerAgent.Position;
		position.z = agent.AgentVisuals.GetGlobalStableEyePoint(true).z;
		position2.z = _speakerAgent.AgentVisuals.GetGlobalStableEyePoint(true).z;
		agent.SetLookToPointOfInterest(_speakerAgent.AgentVisuals.GetGlobalStableEyePoint(true));
		agent.AgentVisuals.GetSkeleton().ForceUpdateBoneFrames();
		Vec3 val = position2 - position;
		agent.LookDirection = ((Vec3)(ref val)).NormalizedCopy();
		agent.SetLookAgent(_speakerAgent);
	}

	private void MakeSpeakerLookToListener()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = _speakerAgent.Position;
		Vec3 position2 = _listenerAgent.Position;
		position.z = _speakerAgent.AgentVisuals.GetGlobalStableEyePoint(true).z;
		position2.z = _listenerAgent.AgentVisuals.GetGlobalStableEyePoint(true).z;
		_speakerAgent.SetLookToPointOfInterest(_listenerAgent.AgentVisuals.GetGlobalStableEyePoint(true));
		_speakerAgent.AgentVisuals.GetSkeleton().ForceUpdateBoneFrames();
		Agent speakerAgent = _speakerAgent;
		Vec3 val = position2 - position;
		speakerAgent.LookDirection = ((Vec3)(ref val)).NormalizedCopy();
		_speakerAgent.SetLookAgent(_listenerAgent);
	}

	private void SetConversationLookToPointOfInterest(Vec3 pointOfInterest)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		ConversationManager conversationManager = Campaign.Current.ConversationManager;
		if (conversationManager.ConversationAgents != null && conversationManager.ConversationAgents.Count > 0)
		{
			_speakerAgent = (Agent)conversationManager.SpeakerAgent;
			_listenerAgent = (Agent)conversationManager.ListenerAgent;
			MakeAgentLookToSpeaker(_listenerAgent);
			MakeSpeakerLookToListener();
			Agent.Main.TeleportToPosition(pointOfInterest);
			Agent.Main.AgentVisuals.SetVisible(false);
			MakeAgentLookToSpeaker(Agent.Main);
		}
	}
}
