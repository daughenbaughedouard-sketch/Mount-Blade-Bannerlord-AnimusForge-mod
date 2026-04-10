using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer.Missions;

public class MultiplayerPracticeMissionComponent : MissionLogic
{
	private LobbyClient _lobbyClient;

	private float _lastMessagePrintPassedTime;

	private bool _shutDownMissionTriggered;

	private float _shutDownMissionTimer;

	private int _shutDownMissionCount;

	private const int ShutDownDurationInSeconds = 3;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_lobbyClient = NetworkMain.GameClient;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Invalid comparison between Unknown and I4
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		((MissionBehavior)this).OnMissionTick(dt);
		_lastMessagePrintPassedTime += dt;
		if (_shutDownMissionTriggered)
		{
			_shutDownMissionTimer += dt;
			if (_shutDownMissionTimer >= 1f)
			{
				_shutDownMissionTimer -= 1f;
				_shutDownMissionCount++;
				if (_shutDownMissionCount >= 3)
				{
					((MissionBehavior)this).Mission.EndMission();
				}
				else
				{
					InformMissionDuration();
				}
			}
		}
		else if ((int)_lobbyClient.CurrentState == 8)
		{
			if (_lastMessagePrintPassedTime > 5f)
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=MrEhLbht}Still searching for a battle...", (Dictionary<string, object>)null)).ToString()));
				_lastMessagePrintPassedTime = 0f;
			}
		}
		else if ((int)_lobbyClient.CurrentState == 9 && !_shutDownMissionTriggered)
		{
			_shutDownMissionTriggered = true;
			InformationManager.DisplayMessage(new InformationMessage(((object)new TextObject("{=BN1Pmhho}Found a battle by matchmaker!", (Dictionary<string, object>)null)).ToString()));
			InformMissionDuration();
		}
	}

	private void InformMissionDuration()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		int num = 3 - _shutDownMissionCount;
		TextObject val = new TextObject("{=aNMmlya4}Shutting down mission in {REMAINING_SECONDS_TO_SHUT_DOWN_MISSION} seconds!", (Dictionary<string, object>)null);
		val.SetTextVariable("REMAINING_SECONDS_TO_SHUT_DOWN_MISSION", num.ToString());
		InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString()));
	}
}
