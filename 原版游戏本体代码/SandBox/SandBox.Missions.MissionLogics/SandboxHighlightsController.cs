using System.Collections.Generic;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class SandboxHighlightsController : MissionLogic
{
	private List<HighlightType> _highlightTypes = new List<HighlightType>
	{
		new HighlightType("hlid_tournament_last_match_kill", "Champion of the Arena", "grpid_incidents", -5000, 3000, 0f, float.MaxValue, true)
	};

	private HighlightsController _highlightsController;

	public override void AfterStart()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_highlightsController = Mission.Current.GetMissionBehavior<HighlightsController>();
		foreach (HighlightType highlightType in _highlightTypes)
		{
			HighlightsController.AddHighlightType(highlightType);
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (affectorAgent == null || !affectorAgent.IsMainAgent || affectedAgent == null || !affectedAgent.IsHuman)
		{
			return;
		}
		TournamentBehavior missionBehavior = Mission.Current.GetMissionBehavior<TournamentBehavior>();
		if (missionBehavior == null || missionBehavior.CurrentMatch == null || missionBehavior.NextRound != null)
		{
			return;
		}
		foreach (TournamentParticipant participant in missionBehavior.CurrentMatch.Participants)
		{
			if ((object)affectorAgent.Character == participant.Character && (object)affectedAgent.Character != participant.Character)
			{
				Highlight val = new Highlight
				{
					Start = Mission.Current.CurrentTime,
					End = Mission.Current.CurrentTime,
					HighlightType = _highlightsController.GetHighlightTypeWithId("hlid_tournament_last_match_kill")
				};
				_highlightsController.SaveHighlight(val, affectedAgent.Position);
				break;
			}
		}
	}
}
