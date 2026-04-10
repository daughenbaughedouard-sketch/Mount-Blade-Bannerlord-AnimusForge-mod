using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Tournaments.MissionLogics;

public class TournamentBehavior : MissionLogic, ICameraModeLogic
{
	public const int RoundCount = 4;

	public const int ParticipantCount = 16;

	public const float EndMatchTimerDuration = 6f;

	public const float CheerTimerDuration = 1f;

	private TournamentGame _tournamentGame;

	private ITournamentGameBehavior _gameBehavior;

	private TournamentParticipant[] _participants;

	private const int MaximumBet = 150;

	public const float MaximumOdd = 4f;

	public TournamentGame TournamentGame => _tournamentGame;

	public TournamentRound[] Rounds { get; private set; }

	public bool IsPlayerEliminated { get; private set; }

	public int CurrentRoundIndex { get; private set; }

	public TournamentMatch LastMatch { get; private set; }

	public TournamentRound CurrentRound => Rounds[CurrentRoundIndex];

	public TournamentRound NextRound
	{
		get
		{
			if (CurrentRoundIndex != 3)
			{
				return Rounds[CurrentRoundIndex + 1];
			}
			return null;
		}
	}

	public TournamentMatch CurrentMatch => CurrentRound.CurrentMatch;

	public TournamentParticipant Winner { get; private set; }

	public bool IsPlayerParticipating { get; private set; }

	public Settlement Settlement { get; private set; }

	public float BetOdd { get; private set; }

	public int MaximumBetInstance => MathF.Min(150, PlayerDenars);

	public int BettedDenars { get; private set; }

	public int OverallExpectedDenars { get; private set; }

	public int PlayerDenars => Hero.MainHero.Gold;

	public event Action TournamentEnd;

	public SpectatorCameraTypes GetMissionCameraLockMode(bool lockedToMainPlayer)
	{
		if (IsPlayerParticipating)
		{
			return (SpectatorCameraTypes)(-1);
		}
		return (SpectatorCameraTypes)2;
	}

	public TournamentBehavior(TournamentGame tournamentGame, Settlement settlement, ITournamentGameBehavior gameBehavior, bool isPlayerParticipating)
	{
		Settlement = settlement;
		_tournamentGame = tournamentGame;
		_gameBehavior = gameBehavior;
		Rounds = (TournamentRound[])(object)new TournamentRound[4];
		CreateParticipants(isPlayerParticipating);
		CurrentRoundIndex = -1;
		LastMatch = null;
		Winner = null;
		IsPlayerParticipating = isPlayerParticipating;
	}

	public MBList<CharacterObject> GetAllPossibleParticipants()
	{
		return _tournamentGame.GetParticipantCharacters(Settlement, true);
	}

	private void CreateParticipants(bool includePlayer)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		_participants = (TournamentParticipant[])(object)new TournamentParticipant[_tournamentGame.MaximumParticipantCount];
		MBList<CharacterObject> participantCharacters = _tournamentGame.GetParticipantCharacters(Settlement, includePlayer);
		Extensions.Shuffle<CharacterObject>((IList<CharacterObject>)participantCharacters);
		for (int i = 0; i < ((List<CharacterObject>)(object)participantCharacters).Count && i < _tournamentGame.MaximumParticipantCount; i++)
		{
			_participants[i] = new TournamentParticipant(((List<CharacterObject>)(object)participantCharacters)[i], default(UniqueTroopDescriptor));
		}
	}

	public static void DeleteTournamentSetsExcept(GameEntity selectedSetEntity)
	{
		List<GameEntity> list = Mission.Current.Scene.FindEntitiesWithTag("arena_set").ToList();
		list.Remove(selectedSetEntity);
		foreach (GameEntity item in list)
		{
			item.Remove(93);
		}
	}

	public static void DeleteAllTournamentSets()
	{
		foreach (GameEntity item in Mission.Current.Scene.FindEntitiesWithTag("arena_set").ToList())
		{
			item.Remove(94);
		}
	}

	public override void AfterStart()
	{
		CurrentRoundIndex = 0;
		CreateTournamentTree();
		FillParticipants(_participants.ToList());
		CalculateBet();
	}

	public override void OnMissionTick(float dt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (CurrentMatch != null && (int)CurrentMatch.State == 1 && _gameBehavior.IsMatchEnded())
		{
			EndCurrentMatch(isLeave: false);
		}
	}

	public void StartMatch()
	{
		if (CurrentMatch.IsPlayerParticipating())
		{
			Campaign.Current.TournamentManager.OnPlayerJoinMatch(((object)_tournamentGame).GetType());
		}
		CurrentMatch.Start();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)7, true);
		_gameBehavior.StartMatch(CurrentMatch, NextRound == null);
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnPlayerStartedTournamentMatch(Settlement.Town);
	}

	public void SkipMatch(bool isLeave = false)
	{
		CurrentMatch.Start();
		_gameBehavior.SkipMatch(CurrentMatch);
		EndCurrentMatch(isLeave);
	}

	private void EndCurrentMatch(bool isLeave)
	{
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		LastMatch = CurrentMatch;
		CurrentRound.EndMatch();
		_gameBehavior.OnMatchEnded();
		if (LastMatch.IsPlayerParticipating())
		{
			if (LastMatch.Winners.All((TournamentParticipant x) => x.Character != CharacterObject.PlayerCharacter))
			{
				OnPlayerEliminated();
			}
			else
			{
				OnPlayerWinMatch();
			}
		}
		if (NextRound != null)
		{
			while (LastMatch.Winners.Any((TournamentParticipant x) => !x.IsAssigned))
			{
				foreach (TournamentParticipant winner in LastMatch.Winners)
				{
					if (!winner.IsAssigned)
					{
						NextRound.AddParticipant(winner, false);
						winner.IsAssigned = true;
					}
				}
			}
		}
		if (CurrentRound.CurrentMatch != null)
		{
			return;
		}
		if (CurrentRoundIndex < 3)
		{
			CurrentRoundIndex++;
			CalculateBet();
			MissionGameModels current2 = MissionGameModels.Current;
			if (current2 != null)
			{
				AgentStatCalculateModel agentStatCalculateModel = current2.AgentStatCalculateModel;
				if (agentStatCalculateModel != null)
				{
					agentStatCalculateModel.SetAILevelMultiplier(1f + (float)CurrentRoundIndex / 3f);
				}
			}
			return;
		}
		MissionGameModels current3 = MissionGameModels.Current;
		if (current3 != null)
		{
			AgentStatCalculateModel agentStatCalculateModel2 = current3.AgentStatCalculateModel;
			if (agentStatCalculateModel2 != null)
			{
				agentStatCalculateModel2.ResetAILevelMultiplier();
			}
		}
		CalculateBet();
		MBInformationManager.AddQuickInformation(new TextObject("{=tWzLqegB}Tournament is over.", (Dictionary<string, object>)null), 0, (BasicCharacterObject)null, (Equipment)null, "");
		Winner = LastMatch.Winners.FirstOrDefault();
		if (((BasicCharacterObject)Winner.Character).IsHero)
		{
			if (Winner.Character == CharacterObject.PlayerCharacter)
			{
				OnPlayerWinTournament();
			}
			Campaign.Current.TournamentManager.GivePrizeToWinner(_tournamentGame, Winner.Character.HeroObject, true);
			Campaign.Current.TournamentManager.AddLeaderboardEntry(Winner.Character.HeroObject);
		}
		MBList<CharacterObject> val = new MBList<CharacterObject>(_participants.Length);
		TournamentParticipant[] participants = _participants;
		foreach (TournamentParticipant val2 in participants)
		{
			((List<CharacterObject>)(object)val).Add(val2.Character);
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnTournamentFinished(Winner.Character, (MBReadOnlyList<CharacterObject>)(object)val, Settlement.Town, _tournamentGame.Prize);
		if (this.TournamentEnd != null && !isLeave)
		{
			this.TournamentEnd();
		}
	}

	public void EndTournamentViaLeave()
	{
		while (CurrentMatch != null)
		{
			SkipMatch(isLeave: true);
		}
	}

	private void OnPlayerEliminated()
	{
		IsPlayerEliminated = true;
		BetOdd = 0f;
		if (BettedDenars > 0)
		{
			GiveGoldAction.ApplyForCharacterToSettlement((Hero)null, Settlement.CurrentSettlement, BettedDenars, false);
		}
		OverallExpectedDenars = 0;
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).OnPlayerEliminatedFromTournament(CurrentRoundIndex, Settlement.Town);
	}

	private void OnPlayerWinMatch()
	{
		Campaign.Current.TournamentManager.OnPlayerWinMatch(((object)_tournamentGame).GetType());
	}

	private void OnPlayerWinTournament()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Campaign.Current.GameMode == 1)
		{
			if (Hero.MainHero.MapFaction.IsKingdomFaction && Hero.MainHero.MapFaction.Leader != Hero.MainHero)
			{
				GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, 1f);
			}
			if (OverallExpectedDenars > 0)
			{
				GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, OverallExpectedDenars, false);
			}
			Campaign.Current.TournamentManager.OnPlayerWinTournament(((object)_tournamentGame).GetType());
		}
	}

	private void CreateTournamentTree()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		int num = 16;
		int num2 = (int)MathF.Log((float)_tournamentGame.MaxTeamSize, 2f);
		for (int i = 0; i < 4; i++)
		{
			int num3 = (int)MathF.Log((float)num, 2f);
			int num4 = MBRandom.RandomInt(1, MathF.Min(MathF.Min(3, num3), _tournamentGame.MaxTeamNumberPerMatch));
			int num5 = MathF.Min(num3 - num4, num2);
			int num6 = MathF.Ceiling(MathF.Log((float)(1 + MBRandom.RandomInt((int)MathF.Pow(2f, (float)num5))), 2f));
			int num7 = num3 - (num4 + num6);
			Rounds[i] = new TournamentRound(num, MathF.PowTwo32(num7), MathF.PowTwo32(num4), num / 2, _tournamentGame.Mode);
			num /= 2;
		}
	}

	private void FillParticipants(List<TournamentParticipant> participants)
	{
		foreach (TournamentParticipant participant in participants)
		{
			Rounds[CurrentRoundIndex].AddParticipant(participant, true);
		}
	}

	public override InquiryData OnEndMissionRequest(out bool canPlayerLeave)
	{
		canPlayerLeave = false;
		return null;
	}

	public void PlaceABet(int bet)
	{
		BettedDenars += bet;
		OverallExpectedDenars += GetExpectedDenarsForBet(bet);
		GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, (Hero)null, bet, true);
	}

	public int GetExpectedDenarsForBet(int bet)
	{
		return (int)(BetOdd * (float)bet);
	}

	public int GetMaximumBet()
	{
		int num = 150;
		if (Hero.MainHero.GetPerkValue(Roguery.DeepPockets))
		{
			num *= (int)Roguery.DeepPockets.PrimaryBonus;
		}
		return num;
	}

	private void CalculateBet()
	{
		if (!IsPlayerParticipating)
		{
			return;
		}
		if (CurrentRound.CurrentMatch == null)
		{
			BetOdd = 0f;
			return;
		}
		if (IsPlayerEliminated || !IsPlayerParticipating)
		{
			OverallExpectedDenars = 0;
			BetOdd = 0f;
			return;
		}
		List<KeyValuePair<Hero, int>> leaderboard = Campaign.Current.TournamentManager.GetLeaderboard();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < leaderboard.Count; i++)
		{
			if (leaderboard[i].Key == Hero.MainHero)
			{
				num = leaderboard[i].Value;
			}
			if (leaderboard[i].Value > num2)
			{
				num2 = leaderboard[i].Value;
			}
		}
		float num3 = 30f + (float)Hero.MainHero.Level + (float)MathF.Max(0, num * 12 - num2 * 2);
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		TournamentMatch[] matches = CurrentRound.Matches;
		foreach (TournamentMatch val in matches)
		{
			foreach (TournamentTeam team in val.Teams)
			{
				float num7 = 0f;
				foreach (TournamentParticipant participant in team.Participants)
				{
					if (participant.Character == CharacterObject.PlayerCharacter)
					{
						continue;
					}
					int num8 = 0;
					if (((BasicCharacterObject)participant.Character).IsHero)
					{
						for (int k = 0; k < leaderboard.Count; k++)
						{
							if (leaderboard[k].Key == participant.Character.HeroObject)
							{
								num8 = leaderboard[k].Value;
							}
						}
					}
					num7 += (float)(((BasicCharacterObject)participant.Character).Level + MathF.Max(0, num8 * 8 - num2 * 2));
				}
				if (team.Participants.Any((TournamentParticipant x) => x.Character == CharacterObject.PlayerCharacter))
				{
					num5 = num7;
					foreach (TournamentTeam team2 in val.Teams)
					{
						if (team == team2)
						{
							continue;
						}
						foreach (TournamentParticipant participant2 in team2.Participants)
						{
							int num9 = 0;
							if (((BasicCharacterObject)participant2.Character).IsHero)
							{
								for (int num10 = 0; num10 < leaderboard.Count; num10++)
								{
									if (leaderboard[num10].Key == participant2.Character.HeroObject)
									{
										num9 = leaderboard[num10].Value;
									}
								}
							}
							num6 += (float)(((BasicCharacterObject)participant2.Character).Level + MathF.Max(0, num9 * 8 - num2 * 2));
						}
					}
				}
				num4 += num7;
			}
		}
		float num11 = (num5 + num3) / (num6 + num5 + num3);
		float num12 = num3 / (num5 + num3 + 0.5f * (num4 - (num5 + num6)));
		float num13 = num11 * num12;
		float num14 = MathF.Clamp(MathF.Pow(1f / num13, 0.75f), 1.1f, 4f);
		BetOdd = (float)(int)(num14 * 10f) / 10f;
	}
}
