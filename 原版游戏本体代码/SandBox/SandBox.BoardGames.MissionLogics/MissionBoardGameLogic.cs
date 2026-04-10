using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using SandBox.BoardGames.AI;
using SandBox.Conversation;
using SandBox.Conversation.MissionLogics;
using SandBox.Objects.Usables;
using SandBox.Source.Missions.AgentBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;

namespace SandBox.BoardGames.MissionLogics;

public class MissionBoardGameLogic : MissionLogic
{
	private const string BoardGameEntityTag = "boardgame";

	private const string SpecialTargetGamblerNpcTag = "gambler_npc";

	public IBoardGameHandler Handler;

	private PlayerTurn _startingPlayer = PlayerTurn.PlayerTwo;

	private Chair _playerChair;

	private Chair _opposingChair;

	private string _specialTagCacheOfOpposingHero;

	private bool _isTavernGame;

	private bool _startingBoardGame;

	private BoardGameState _boardGameState;

	public BoardGameBase Board { get; private set; }

	public BoardGameAIBase AIOpponent { get; private set; }

	public bool IsOpposingAgentMovingToPlayingChair => BoardGameAgentBehavior.IsAgentMovingToChair(OpposingAgent);

	public bool IsGameInProgress { get; private set; }

	public BoardGameState BoardGameFinalState => _boardGameState;

	public BoardGameType CurrentBoardGame { get; private set; }

	public AIDifficulty Difficulty { get; private set; }

	public int BetAmount { get; private set; }

	public Agent OpposingAgent { get; private set; }

	public event Action GameStarted;

	public event Action GameEnded;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_opposingChair = ((IEnumerable<Chair>)MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<Chair>(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("gambler_npc"))).FirstOrDefault();
		_playerChair = ((IEnumerable<Chair>)MBExtensions.CollectScriptComponentsIncludingChildrenRecursive<Chair>(((MissionBehavior)this).Mission.Scene.FindEntityWithTag("gambler_player"))).FirstOrDefault();
		foreach (StandingPoint item in (List<StandingPoint>)(object)((UsableMachine)_opposingChair).StandingPoints)
		{
			((UsableMissionObject)item).IsDisabledForPlayers = true;
		}
	}

	public void SetStartingPlayer(bool playerOneStarts)
	{
		_startingPlayer = ((!playerOneStarts) ? PlayerTurn.PlayerTwo : PlayerTurn.PlayerOne);
	}

	public void StartBoardGame()
	{
		_startingBoardGame = true;
	}

	private void BoardGameInit(BoardGameType game)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected I4, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		if (Board == null)
		{
			switch ((int)game)
			{
			case 0:
				Board = new BoardGameSeega(this, _startingPlayer);
				AIOpponent = new BoardGameAISeega(Difficulty, this);
				break;
			case 1:
				Board = new BoardGamePuluc(this, _startingPlayer);
				AIOpponent = new BoardGameAIPuluc(Difficulty, this);
				break;
			case 3:
				Board = new BoardGameMuTorere(this, _startingPlayer);
				AIOpponent = new BoardGameAIMuTorere(Difficulty, this);
				break;
			case 2:
				Board = new BoardGameKonane(this, _startingPlayer);
				AIOpponent = new BoardGameAIKonane(Difficulty, this);
				break;
			case 5:
				Board = new BoardGameBaghChal(this, _startingPlayer);
				AIOpponent = new BoardGameAIBaghChal(Difficulty, this);
				break;
			case 4:
				Board = new BoardGameTablut(this, _startingPlayer);
				AIOpponent = new BoardGameAITablut(Difficulty, this);
				break;
			default:
				Debug.FailedAssert("[DEBUG]No board with this name was found.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\BoardGames\\MissionLogics\\MissionBoardGameLogic.cs", "BoardGameInit", 119);
				break;
			}
			Board.Initialize();
			if (AIOpponent != null)
			{
				AIOpponent.Initialize();
			}
		}
		else
		{
			Board.SetStartingPlayer(_startingPlayer);
			Board.InitializeUnits();
			Board.InitializeCapturedUnitsZones();
			Board.Reset();
			if (AIOpponent != null)
			{
				AIOpponent.SetDifficulty(Difficulty);
				AIOpponent.Initialize();
			}
		}
		if (Handler != null)
		{
			Handler.Install();
		}
		_boardGameState = (BoardGameState)0;
		IsGameInProgress = true;
		_isTavernGame = CampaignMission.Current.Location == Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("tavern");
	}

	public override void OnMissionTick(float dt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!((MissionBehavior)this).Mission.IsInPhotoMode)
		{
			if (_startingBoardGame)
			{
				_startingBoardGame = false;
				BoardGameInit(CurrentBoardGame);
				this.GameStarted?.Invoke();
			}
			else if (IsGameInProgress)
			{
				Board.Tick(dt);
			}
			else if (OpposingAgent != null && OpposingAgent.IsHero && Hero.OneToOneConversationHero == null && CheckIfBothSidesAreSitting())
			{
				StartBoardGame();
			}
		}
	}

	public void DetectOpposingAgent()
	{
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item == ConversationMission.OneToOneConversationAgent)
			{
				OpposingAgent = item;
				if (item.IsHero)
				{
					BoardGameAgentBehavior.AddTargetChair(OpposingAgent, _opposingChair);
				}
				AgentNavigator agentNavigator = OpposingAgent.GetComponent<CampaignAgentComponent>().AgentNavigator;
				_specialTagCacheOfOpposingHero = agentNavigator.SpecialTargetTag;
				agentNavigator.SpecialTargetTag = "gambler_npc";
				break;
			}
		}
	}

	public bool CheckIfBothSidesAreSitting()
	{
		if (Agent.Main != null && OpposingAgent != null && _playerChair.IsAgentFullySitting(Agent.Main))
		{
			return _opposingChair.IsAgentFullySitting(OpposingAgent);
		}
		return false;
	}

	public void PlayerOneWon(string message = "str_boardgame_victory_message")
	{
		Agent opposingAgent = OpposingAgent;
		SetGameOver(GameOverEnum.PlayerOneWon);
		ShowInquiry(message, opposingAgent);
	}

	public void PlayerTwoWon(string message = "str_boardgame_defeat_message")
	{
		Agent opposingAgent = OpposingAgent;
		SetGameOver(GameOverEnum.PlayerTwoWon);
		ShowInquiry(message, opposingAgent);
	}

	public void GameWasDraw(string message = "str_boardgame_draw_message")
	{
		Agent opposingAgent = OpposingAgent;
		SetGameOver(GameOverEnum.Draw);
		ShowInquiry(message, opposingAgent);
	}

	private void ShowInquiry(string message, Agent conversationAgent)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_boardgame", (string)null)).ToString(), ((object)GameTexts.FindText(message, (string)null)).ToString(), true, false, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), "", (Action)delegate
		{
			StartConversationWithOpponentAfterGameEnd(conversationAgent);
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void StartConversationWithOpponentAfterGameEnd(Agent conversationAgent)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		MissionConversationLogic.Current.StartConversation(conversationAgent, setActionsInstantly: false);
		_boardGameState = (BoardGameState)0;
	}

	public void SetGameOver(GameOverEnum gameOverInfo)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).Mission.MainAgent.ClearTargetFrame();
		if (Handler != null && gameOverInfo != GameOverEnum.PlayerCanceledTheGame)
		{
			Handler.Uninstall();
		}
		Hero val = (OpposingAgent.IsHero ? ((CharacterObject)OpposingAgent.Character).HeroObject : null);
		switch (gameOverInfo)
		{
		case GameOverEnum.PlayerOneWon:
			_boardGameState = (BoardGameState)1;
			break;
		case GameOverEnum.PlayerTwoWon:
			_boardGameState = (BoardGameState)2;
			break;
		case GameOverEnum.Draw:
			_boardGameState = (BoardGameState)3;
			break;
		case GameOverEnum.PlayerCanceledTheGame:
			_boardGameState = (BoardGameState)0;
			break;
		}
		if (gameOverInfo != GameOverEnum.PlayerCanceledTheGame)
		{
			((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnPlayerBoardGameOver(val, _boardGameState);
		}
		this.GameEnded?.Invoke();
		BoardGameAgentBehavior.RemoveBoardGameBehaviorOfAgent(OpposingAgent);
		OpposingAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SpecialTargetTag = _specialTagCacheOfOpposingHero;
		OpposingAgent = null;
		IsGameInProgress = false;
		AIOpponent?.OnSetGameOver();
	}

	public void ForfeitGame()
	{
		Board.SetGameOverInfo(GameOverEnum.PlayerTwoWon);
		Agent opposingAgent = OpposingAgent;
		SetGameOver(Board.GameOverInfo);
		StartConversationWithOpponentAfterGameEnd(opposingAgent);
	}

	public void AIForfeitGame()
	{
		Board.SetGameOverInfo(GameOverEnum.PlayerOneWon);
		SetGameOver(Board.GameOverInfo);
	}

	public void RollDice()
	{
		Board.RollDice();
	}

	public bool RequiresDiceRolling()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		BoardGameType currentBoardGame = CurrentBoardGame;
		return (int)currentBoardGame switch
		{
			0 => false, 
			1 => true, 
			2 => false, 
			3 => false, 
			4 => false, 
			5 => false, 
			_ => false, 
		};
	}

	public void SetBetAmount(int bet)
	{
		BetAmount = bet;
	}

	public void SetCurrentDifficulty(AIDifficulty difficulty)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Difficulty = difficulty;
	}

	public void SetBoardGame(BoardGameType game)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		CurrentBoardGame = game;
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		if (IsGameInProgress)
		{
			SetGameOver(GameOverEnum.PlayerCanceledTheGame);
		}
	}

	public override InquiryData OnEndMissionRequest(out bool canLeave)
	{
		canLeave = true;
		return null;
	}

	public static bool IsBoardGameAvailable()
	{
		Mission current = Mission.Current;
		MissionBoardGameLogic missionBoardGameLogic = ((current != null) ? current.GetMissionBehavior<MissionBoardGameLogic>() : null);
		Mission current2 = Mission.Current;
		if ((NativeObject)(object)((current2 != null) ? current2.Scene : null) != (NativeObject)null && missionBoardGameLogic != null && Mission.Current.Scene.FindEntityWithTag("boardgame") != (GameEntity)null)
		{
			return missionBoardGameLogic.OpposingAgent == null;
		}
		return false;
	}

	public static bool IsThereActiveBoardGameWithHero(Hero hero)
	{
		Mission current = Mission.Current;
		MissionBoardGameLogic missionBoardGameLogic = ((current != null) ? current.GetMissionBehavior<MissionBoardGameLogic>() : null);
		Mission current2 = Mission.Current;
		if ((NativeObject)(object)((current2 != null) ? current2.Scene : null) != (NativeObject)null && Mission.Current.Scene.FindEntityWithTag("boardgame") != (GameEntity)null && missionBoardGameLogic != null)
		{
			Agent opposingAgent = missionBoardGameLogic.OpposingAgent;
			return (object)((opposingAgent != null) ? opposingAgent.Character : null) == hero.CharacterObject;
		}
		return false;
	}

	public override void OnAgentInteraction(Agent userAgent, Agent agent, sbyte agentBoneIndex)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode == 1 && !Campaign.Current.ConversationManager.IsConversationInProgress && ((MissionBehavior)this).IsThereAgentAction(userAgent, agent))
		{
			Mission.Current.GetMissionBehavior<MissionConversationLogic>().StartConversation(agent, setActionsInstantly: false);
		}
	}

	public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
	{
		if (userAgent.IsMainAgent && _playerChair.IsAgentFullySitting(Agent.Main))
		{
			return _opposingChair.IsAgentFullySitting(otherAgent);
		}
		return false;
	}
}
