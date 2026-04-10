using SandBox.Missions.MissionLogics.Arena;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Missions;

public class MissionArenaPracticeFightVM : ViewModel
{
	private readonly Mission _mission;

	private readonly ArenaPracticeFightMissionController _practiceMissionController;

	private string _opponentsBeatenText;

	private string _opponentsRemainingText;

	private bool _isPlayerPracticing;

	private string _prizeText;

	[DataSourceProperty]
	public string OpponentsBeatenText
	{
		get
		{
			return _opponentsBeatenText;
		}
		set
		{
			if (_opponentsBeatenText != value)
			{
				_opponentsBeatenText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OpponentsBeatenText");
			}
		}
	}

	[DataSourceProperty]
	public string PrizeText
	{
		get
		{
			return _prizeText;
		}
		set
		{
			if (_prizeText != value)
			{
				_prizeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PrizeText");
			}
		}
	}

	[DataSourceProperty]
	public string OpponentsRemainingText
	{
		get
		{
			return _opponentsRemainingText;
		}
		set
		{
			if (_opponentsRemainingText != value)
			{
				_opponentsRemainingText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "OpponentsRemainingText");
			}
		}
	}

	public bool IsPlayerPracticing
	{
		get
		{
			return _isPlayerPracticing;
		}
		set
		{
			if (_isPlayerPracticing != value)
			{
				_isPlayerPracticing = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerPracticing");
			}
		}
	}

	public MissionArenaPracticeFightVM(ArenaPracticeFightMissionController practiceMissionController)
	{
		_practiceMissionController = practiceMissionController;
		_mission = ((MissionBehavior)practiceMissionController).Mission;
	}

	public void Tick()
	{
		IsPlayerPracticing = _practiceMissionController.IsPlayerPracticing;
		Agent mainAgent = _mission.MainAgent;
		if (mainAgent != null && mainAgent.IsActive())
		{
			int killCount = _mission.MainAgent.KillCount;
			GameTexts.SetVariable("BEATEN_OPPONENT_COUNT", killCount);
			OpponentsBeatenText = ((object)GameTexts.FindText("str_beaten_opponent", (string)null)).ToString();
		}
		int remainingOpponentCount = _practiceMissionController.RemainingOpponentCount;
		GameTexts.SetVariable("REMAINING_OPPONENT_COUNT", remainingOpponentCount);
		OpponentsRemainingText = ((object)GameTexts.FindText("str_remaining_opponent", (string)null)).ToString();
		UpdatePrizeText();
	}

	public void UpdatePrizeText()
	{
		int remainingOpponentCount = _practiceMissionController.RemainingOpponentCount;
		int opponentCountBeatenByPlayer = _practiceMissionController.OpponentCountBeatenByPlayer;
		int num = 0;
		if (remainingOpponentCount == 0)
		{
			num = 250;
		}
		else if (opponentCountBeatenByPlayer >= 3)
		{
			num = ((opponentCountBeatenByPlayer < 6) ? 5 : ((opponentCountBeatenByPlayer < 10) ? 10 : ((opponentCountBeatenByPlayer >= 20) ? 60 : 25)));
		}
		GameTexts.SetVariable("DENAR_AMOUNT", num);
		GameTexts.SetVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">");
		PrizeText = ((object)GameTexts.FindText("str_earned_denar", (string)null)).ToString();
	}
}
