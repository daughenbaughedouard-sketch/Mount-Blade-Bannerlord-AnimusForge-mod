using System;
using System.Collections.Generic;
using SandBox.Tournaments.AgentControllers;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace SandBox.View.Missions.Tournaments;

public class MissionTournamentJoustingView : MissionView
{
	private MissionScoreUIHandler _scoreUIHandler;

	private MissionMessageUIHandler _messageUIHandler;

	private TournamentJoustingMissionController _tournamentJoustingMissionController;

	private Game _gameSystem;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_gameSystem = Game.Current;
		_messageUIHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMessageUIHandler>();
		_scoreUIHandler = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionScoreUIHandler>();
		_tournamentJoustingMissionController = ((MissionBehavior)this).Mission.GetMissionBehavior<TournamentJoustingMissionController>();
		_tournamentJoustingMissionController.VictoryAchieved += OnVictoryAchieved;
		_tournamentJoustingMissionController.PointGanied += OnPointGanied;
		_tournamentJoustingMissionController.Disqualified += OnDisqualified;
		_tournamentJoustingMissionController.Unconscious += OnUnconscious;
		_tournamentJoustingMissionController.AgentStateChanged += OnAgentStateChanged;
		int num = 0;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.IsHuman)
			{
				_scoreUIHandler.SetName(item.Name.ToString(), num);
				num++;
			}
		}
		SetJoustingBanners();
	}

	private void RefreshScoreBoard()
	{
		int num = 0;
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.IsHuman)
			{
				JoustingAgentController controller = item.GetController<JoustingAgentController>();
				_scoreUIHandler.SaveScore(controller.Score, num);
				num++;
			}
		}
	}

	private void SetJoustingBanners()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		GameEntity banner0 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("banner_0");
		GameEntity banner1 = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("banner_1");
		Banner val = Banner.CreateOneColoredEmptyBanner(6);
		Banner val2 = Banner.CreateOneColoredEmptyBanner(8);
		BannerDebugInfo val3;
		if (banner0 != (GameEntity)null)
		{
			Action<Texture> action = delegate(Texture tex)
			{
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Invalid comparison between Unknown and I4
				Material val4 = Mesh.GetFromResource("banner_test").GetMaterial().CreateCopy();
				if ((int)Campaign.Current.GameMode == 1)
				{
					val4.SetTexture((MBTextureType)1, tex);
				}
				banner0.SetMaterialForAllMeshes(val4);
			};
			val3 = BannerDebugInfo.CreateManual(((object)this).GetType().Name);
			BannerVisualExtensions.GetTableauTextureLarge(val, ref val3, action);
		}
		if (!(banner1 != (GameEntity)null))
		{
			return;
		}
		Action<Texture> action2 = delegate(Texture tex)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			Material val4 = Mesh.GetFromResource("banner_test").GetMaterial().CreateCopy();
			if ((int)Campaign.Current.GameMode == 1)
			{
				val4.SetTexture((MBTextureType)1, tex);
			}
			banner1.SetMaterialForAllMeshes(val4);
		};
		val3 = BannerDebugInfo.CreateManual(((object)this).GetType().Name);
		BannerVisualExtensions.GetTableauTextureLarge(val2, ref val3, action2);
	}

	public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon attackerWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
	{
		RefreshScoreBoard();
	}

	private void OnVictoryAchieved(Agent affectorAgent, Agent affectedAgent)
	{
		ShowMessage(affectorAgent, ((object)GameTexts.FindText("str_tournament_joust_player_victory", (string)null)).ToString(), 8f);
		ShowMessage(affectedAgent, ((object)GameTexts.FindText("str_tournament_joust_opponent_victory", (string)null)).ToString(), 8f);
	}

	private void OnPointGanied(Agent affectorAgent, Agent affectedAgent)
	{
		ShowMessage(affectorAgent, ((object)GameTexts.FindText("str_tournament_joust_you_gain_point", (string)null)).ToString(), 5f);
		ShowMessage(affectedAgent, ((object)GameTexts.FindText("str_tournament_joust_opponent_gain_point", (string)null)).ToString(), 5f);
	}

	private void OnDisqualified(Agent affectorAgent, Agent affectedAgent)
	{
		ShowMessage(affectedAgent, ((object)GameTexts.FindText("str_tournament_joust_opponent_disqualified", (string)null)).ToString(), 5f);
		ShowMessage(affectorAgent, ((object)GameTexts.FindText("str_tournament_joust_you_disqualified", (string)null)).ToString(), 5f);
	}

	private void OnUnconscious(Agent affectorAgent, Agent affectedAgent)
	{
		ShowMessage(affectedAgent, ((object)GameTexts.FindText("str_tournament_joust_you_become_unconscious", (string)null)).ToString(), 5f);
		ShowMessage(affectorAgent, ((object)GameTexts.FindText("str_tournament_joust_opponent_become_unconscious", (string)null)).ToString(), 5f);
	}

	public void ShowMessage(string str, float duration, bool hasPriority = true)
	{
		_messageUIHandler.ShowMessage(str, duration, hasPriority);
	}

	public void ShowMessage(Agent agent, string str, float duration, bool hasPriority = true)
	{
		if (agent.Character == _gameSystem.PlayerTroop)
		{
			ShowMessage(str, duration, hasPriority);
		}
	}

	public void DeleteMessage(string str)
	{
		_messageUIHandler.DeleteMessage(str);
	}

	public void DeleteMessage(Agent agent, string str)
	{
		DeleteMessage(str);
	}

	private void OnAgentStateChanged(Agent agent, JoustingAgentController.JoustingAgentState state)
	{
		string text = "";
		text = state switch
		{
			JoustingAgentController.JoustingAgentState.GoingToBackStart => "", 
			JoustingAgentController.JoustingAgentState.GoToStartPosition => "str_tournament_joust_go_to_starting_position", 
			JoustingAgentController.JoustingAgentState.WaitInStartPosition => "str_tournament_joust_wait_in_starting_position", 
			JoustingAgentController.JoustingAgentState.WaitingOpponent => "str_tournament_joust_wait_opponent_to_go_starting_position", 
			JoustingAgentController.JoustingAgentState.Ready => "str_tournament_joust_go", 
			JoustingAgentController.JoustingAgentState.StartRiding => "", 
			JoustingAgentController.JoustingAgentState.Riding => "", 
			JoustingAgentController.JoustingAgentState.RidingAtWrongSide => "str_tournament_joust_wrong_side", 
			JoustingAgentController.JoustingAgentState.SwordDuel => "", 
			_ => throw new ArgumentOutOfRangeException("value"), 
		};
		if (text == "")
		{
			ShowMessage(agent, "", 15f);
		}
		else
		{
			ShowMessage(agent, ((object)GameTexts.FindText(text, (string)null)).ToString(), float.PositiveInfinity);
		}
		if (state == JoustingAgentController.JoustingAgentState.SwordDuel)
		{
			ShowMessage(agent, ((object)GameTexts.FindText("str_tournament_joust_duel_on_foot", (string)null)).ToString(), 8f);
		}
	}
}
