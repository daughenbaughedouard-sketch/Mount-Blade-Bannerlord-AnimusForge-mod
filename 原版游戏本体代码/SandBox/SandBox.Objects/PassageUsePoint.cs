using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects;

public class PassageUsePoint : StandingPoint
{
	public string ToLocationId = "";

	public bool IsMissionExit;

	private bool _initialized;

	private readonly MBList<Agent> _movingAgents;

	private Location _toLocation;

	private const float InteractionDistanceForAI = 0.5f;

	public MBReadOnlyList<Agent> MovingAgents => (MBReadOnlyList<Agent>)(object)_movingAgents;

	public override Agent MovingAgent
	{
		get
		{
			if (((List<Agent>)(object)_movingAgents).Count <= 0)
			{
				return null;
			}
			return ((List<Agent>)(object)_movingAgents)[0];
		}
	}

	public Location ToLocation
	{
		get
		{
			if (!_initialized)
			{
				InitializeLocation();
			}
			return _toLocation;
		}
	}

	public override bool HasAIMovingTo => ((List<Agent>)(object)_movingAgents).Count > 0;

	public override FocusableObjectType FocusableObjectType => (FocusableObjectType)4;

	public override bool DisableCombatActionsOnUse => !((UsableMissionObject)this).IsInstantUse;

	public PassageUsePoint()
	{
		((UsableMissionObject)this).IsInstantUse = true;
		_movingAgents = new MBList<Agent>();
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		if (agent.MountAgent != null || ((UsableMissionObject)this).IsDeactivated || (ToLocation == null && !IsMissionExit) || ((MissionObject)this).IsDisabled)
		{
			return true;
		}
		if (agent.IsAIControlled)
		{
			if (!IsMissionExit)
			{
				return !ToLocation.CanAIEnter(CampaignMission.Current.Location.GetLocationCharacter(agent.Origin));
			}
			return true;
		}
		return false;
	}

	public override void AfterMissionStart()
	{
		((UsableMissionObject)this).DescriptionMessage = GameTexts.FindText(IsMissionExit ? "str_mission_exit" : "str_ui_door", (string)null);
		((UsableMissionObject)this).ActionMessage = GameTexts.FindText("str_ui_default_door", (string)null);
		if (ToLocation != null || IsMissionExit)
		{
			((UsableMissionObject)this).ActionMessage = GameTexts.FindText("str_key_action", (string)null);
			((UsableMissionObject)this).ActionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
			((UsableMissionObject)this).ActionMessage.SetTextVariable("ACTION", (ToLocation == null) ? GameTexts.FindText("str_ui_default_door", (string)null) : ToLocation.DoorName);
		}
	}

	protected override void OnInit()
	{
		((StandingPoint)this).OnInit();
		((UsableMissionObject)this).LockUserPositions = true;
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if ((int)Campaign.Current.GameMode != 1 && !userAgent.IsAIControlled)
		{
			return;
		}
		((StandingPoint)this).OnUse(userAgent, agentBoneIndex);
		bool flag = false;
		if (ToLocation != null)
		{
			if (((UsableMissionObject)this).UserAgent.IsMainAgent)
			{
				if (!ToLocation.CanPlayerEnter())
				{
					InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=ILnr9eCQ}Door is locked!", (Dictionary<string, object>)null)).ToString()));
				}
				else
				{
					flag = true;
					Campaign.Current.GameMenuManager.NextLocation = ToLocation;
					Campaign.Current.GameMenuManager.PreviousLocation = CampaignMission.Current.Location;
					Mission.Current.EndMission();
				}
			}
			else if (((UsableMissionObject)this).UserAgent.IsAIControlled)
			{
				LocationCharacter locationCharacter = CampaignMission.Current.Location.GetLocationCharacter(((UsableMissionObject)this).UserAgent.Origin);
				if (!ToLocation.CanAIEnter(locationCharacter))
				{
					MBDebug.ShowWarning("AI should not try to use passage ");
				}
				else
				{
					flag = true;
					LocationComplex.Current.ChangeLocation(locationCharacter, CampaignMission.Current.Location, ToLocation);
					((UsableMissionObject)this).UserAgent.FadeOut(false, true);
				}
			}
		}
		else if (IsMissionExit)
		{
			flag = true;
			Mission.Current.EndMission();
		}
		if (flag)
		{
			Mission current = Mission.Current;
			int soundCodeMovementFoleyDoorOpen = MiscSoundContainer.SoundCodeMovementFoleyDoorOpen;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			current.MakeSound(soundCodeMovementFoleyDoorOpen, ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin, true, false, -1, -1);
		}
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		((StandingPoint)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		if (((UsableMissionObject)this).LockUserFrames || ((UsableMissionObject)this).LockUserPositions)
		{
			userAgent.ClearTargetFrame();
		}
	}

	public override bool IsUsableByAgent(Agent userAgent)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		if (userAgent.IsAIControlled)
		{
			WeakGameEntity interactionEntity = ((UsableMissionObject)this).InteractionEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref interactionEntity)).GetGlobalFrame();
			Vec2 asVec = ((Vec3)(ref globalFrame.origin)).AsVec2;
			Vec3 position = userAgent.Position;
			Vec2 val = asVec - ((Vec3)(ref position)).AsVec2;
			if (((Vec2)(ref val)).LengthSquared > 0.25f)
			{
				result = false;
			}
		}
		return result;
	}

	private void InitializeLocation()
	{
		if (string.IsNullOrEmpty(ToLocationId) || IsMissionExit)
		{
			_toLocation = null;
			_initialized = IsMissionExit;
		}
		else if (Mission.Current != null && Campaign.Current != null)
		{
			if (PlayerEncounter.LocationEncounter != null && CampaignMission.Current.Location != null)
			{
				_toLocation = CampaignMission.Current.Location.GetPassageToLocation(ToLocationId);
			}
			_initialized = true;
		}
	}

	public override int GetMovingAgentCount()
	{
		return ((List<Agent>)(object)_movingAgents).Count;
	}

	public override Agent GetMovingAgentWithIndex(int index)
	{
		return ((List<Agent>)(object)_movingAgents)[index];
	}

	public override void AddMovingAgent(Agent movingAgent)
	{
		((List<Agent>)(object)_movingAgents).Add(movingAgent);
	}

	public override void RemoveMovingAgent(Agent movingAgent)
	{
		((List<Agent>)(object)_movingAgents).Remove(movingAgent);
	}

	public override bool IsAIMovingTo(Agent agent)
	{
		return ((List<Agent>)(object)_movingAgents).Contains(agent);
	}
}
