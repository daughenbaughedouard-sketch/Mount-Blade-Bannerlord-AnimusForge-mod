using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SandBox.Missions.BattleScore;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.BattleScore;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace SandBox.ViewModelCollection;

public class SPScoreboardVM : ScoreboardBaseVM, IBattleObserver
{
	private readonly BattleSimulation _battleSimulation;

	private SallyOutEndLogic _sallyOutEndLogic;

	private static readonly TextObject _renownStr = new TextObject("{=eiWQoW9j}You gained {A0} renown.", (Dictionary<string, object>)null);

	private static readonly TextObject _influenceStr = new TextObject("{=5zeL8sa9}You gained {A0} influence.", (Dictionary<string, object>)null);

	private static readonly TextObject _moraleStr = new TextObject("{=WAKz9xX8}You gained {A0} morale.", (Dictionary<string, object>)null);

	private static readonly TextObject _lootStr = new TextObject("{=xu5NA6AW}You earned {A0}% of the loot.", (Dictionary<string, object>)null);

	private static readonly TextObject _deadLordStr = new TextObject("{=gDKhs4lD}{A0} has died on the battlefield.", (Dictionary<string, object>)null);

	private static readonly TextObject _figureheadStr = new TextObject("{=ANoYN1yZ}You unlocked the {A0} figurehead.", (Dictionary<string, object>)null);

	private float _moraleUpdateTimer = 1f;

	private float _missionEndScoreboardDelayTimer;

	private MBBindingList<BattleResultVM> _battleResults;

	private bool _isPlayerDefendingSiege
	{
		get
		{
			Mission current = Mission.Current;
			if (current != null && current.IsSiegeBattle)
			{
				return Mission.Current.PlayerTeam.IsDefender;
			}
			return false;
		}
	}

	[DataSourceProperty]
	public override MBBindingList<BattleResultVM> BattleResults
	{
		get
		{
			return _battleResults;
		}
		set
		{
			if (value != _battleResults)
			{
				_battleResults = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<BattleResultVM>>(value, "BattleResults");
			}
		}
	}

	public static SPScoreboardVM CreateSimulation(BattleSimulation simulation)
	{
		return new SPScoreboardVM((BattleScoreContext)(object)new SandboxSimulationBattleScoreContext(simulation), simulation);
	}

	public static SPScoreboardVM CreateMission(Mission mission)
	{
		return new SPScoreboardVM((BattleScoreContext)(object)new SandboxMissionBattleScoreContext(mission), null);
	}

	public static SPScoreboardVM CreateCustom(BattleScoreContext battleScoreContext, BattleSimulation simulation = null)
	{
		return new SPScoreboardVM(battleScoreContext, simulation);
	}

	public SPScoreboardVM(BattleScoreContext scoreboardContext, BattleSimulation simulation)
		: base(scoreboardContext)
	{
		_battleSimulation = simulation;
		((ScoreboardBaseVM)this).BattleResults = new MBBindingList<BattleResultVM>();
	}

	protected override void UpdateQuitText()
	{
		if (((ScoreboardBaseVM)this).IsOver)
		{
			((ScoreboardBaseVM)this).QuitText = ((object)GameTexts.FindText("str_done", (string)null)).ToString();
		}
		else if (((ScoreboardBaseVM)this).IsMainCharacterDead && !((ScoreboardBaseVM)this).IsSimulation)
		{
			((ScoreboardBaseVM)this).QuitText = ((object)GameTexts.FindText("str_end_battle", (string)null)).ToString();
		}
		else if (_isPlayerDefendingSiege)
		{
			((ScoreboardBaseVM)this).QuitText = ((object)GameTexts.FindText("str_surrender", (string)null)).ToString();
		}
		else
		{
			((ScoreboardBaseVM)this).QuitText = ((object)GameTexts.FindText("str_retreat", (string)null)).ToString();
		}
	}

	public override void Initialize(IMissionScreen missionScreen, Mission mission, Action releaseSimulationSources, Action<bool> onToggle)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		((ScoreboardBaseVM)this).Initialize(missionScreen, mission, releaseSimulationSources, onToggle);
		((ScoreboardBaseVM)this).Attackers = new SPScoreboardSideVM(GameTexts.FindText("str_battle_result_side", "attacker"), base.ScoreboardContext.GetAttackerBanner(), true, PlayerEncounter.PlayerIsAttacker);
		((ScoreboardBaseVM)this).Defenders = new SPScoreboardSideVM(GameTexts.FindText("str_battle_result_side", "defender"), base.ScoreboardContext.GetDefenderBanner(), true, !PlayerEncounter.PlayerIsAttacker);
		if (_battleSimulation != null)
		{
			base.PlayerSide = (BattleSideEnum)(PlayerEncounter.PlayerIsAttacker ? 1 : 0);
			((ScoreboardBaseVM)this).IsSimulation = true;
			((ScoreboardBaseVM)this).IsMainCharacterDead = true;
			((ScoreboardBaseVM)this).ShowScoreboard = true;
			foreach (List<BattleResultPartyData> team in _battleSimulation.Teams)
			{
				foreach (BattleResultPartyData item in team)
				{
					PartyBase party = item.Party;
					SPScoreboardSideVM side = ((ScoreboardBaseVM)this).GetSide(party.Side);
					bool flag = ((party != null) ? party.Owner : null) == Hero.MainHero;
					foreach (TroopRosterElement item2 in (List<TroopRosterElement>)(object)party.MemberRoster.GetTroopRoster())
					{
						TroopRosterElement current = item2;
						side.UpdateScores((IBattleCombatant)(object)party, flag, (BasicCharacterObject)(object)current.Character, ((TroopRosterElement)(ref current)).Number - ((TroopRosterElement)(ref current)).WoundedNumber, 0, 0, 0, 0, 0);
					}
				}
			}
			_battleSimulation.BattleObserver = (IBattleObserver)(object)this;
			((ScoreboardBaseVM)this).PowerComparer.Update((double)((ScoreboardBaseVM)this).Defenders.CurrentPower, (double)((ScoreboardBaseVM)this).Attackers.CurrentPower, (double)((ScoreboardBaseVM)this).Defenders.CurrentPower, (double)((ScoreboardBaseVM)this).Attackers.CurrentPower);
		}
		else
		{
			((ScoreboardBaseVM)this).IsSimulation = false;
			if (Campaign.Current != null)
			{
				if (PlayerEncounter.Battle != null)
				{
					base.PlayerSide = (BattleSideEnum)(PlayerEncounter.PlayerIsAttacker ? 1 : 0);
				}
				else
				{
					base.PlayerSide = (BattleSideEnum)0;
				}
			}
			else
			{
				Debug.FailedAssert("SPScoreboard on CustomBattle", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\SPScoreboardVM.cs", "Initialize", 131);
			}
			BattleObserverMissionLogic missionBehavior = base._mission.GetMissionBehavior<BattleObserverMissionLogic>();
			if (missionBehavior != null)
			{
				missionBehavior.SetObserver((IBattleObserver)(object)this);
			}
			else
			{
				Debug.FailedAssert("SPScoreboard on CustomBattle", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\SPScoreboardVM.cs", "Initialize", 156);
			}
		}
		string text;
		string text2;
		if (MobileParty.MainParty.MapEvent != null)
		{
			PartyBase leaderParty = MobileParty.MainParty.MapEvent.DefenderSide.LeaderParty;
			if (((leaderParty != null) ? leaderParty.MapFaction : null) is Kingdom)
			{
				text = ((object)Color.FromUint(((Kingdom)MobileParty.MainParty.MapEvent.DefenderSide.LeaderParty.MapFaction).PrimaryBannerColor)/*cast due to .constrained prefix*/).ToString();
			}
			else
			{
				IFaction mapFaction = MobileParty.MainParty.MapEvent.DefenderSide.LeaderParty.MapFaction;
				text = ((object)Color.FromUint((mapFaction != null) ? mapFaction.Banner.GetPrimaryColor() : 0u)/*cast due to .constrained prefix*/).ToString();
			}
			PartyBase leaderParty2 = MobileParty.MainParty.MapEvent.AttackerSide.LeaderParty;
			if (((leaderParty2 != null) ? leaderParty2.MapFaction : null) is Kingdom)
			{
				text2 = ((object)Color.FromUint(((Kingdom)MobileParty.MainParty.MapEvent.AttackerSide.LeaderParty.MapFaction).PrimaryBannerColor)/*cast due to .constrained prefix*/).ToString();
			}
			else
			{
				IFaction mapFaction2 = MobileParty.MainParty.MapEvent.AttackerSide.LeaderParty.MapFaction;
				text2 = ((object)Color.FromUint((mapFaction2 != null) ? mapFaction2.Banner.GetPrimaryColor() : 0u)/*cast due to .constrained prefix*/).ToString();
			}
		}
		else
		{
			text2 = ((object)Color.FromUint(Mission.Current.Teams.Attacker.Color)/*cast due to .constrained prefix*/).ToString();
			text = ((object)Color.FromUint(Mission.Current.Teams.Defender.Color)/*cast due to .constrained prefix*/).ToString();
		}
		((ScoreboardBaseVM)this).PowerComparer.SetColors(text, text2);
		((ScoreboardBaseVM)this).MissionTimeInSeconds = -1;
	}

	protected override void OnTick(float dt)
	{
		if (!((ScoreboardBaseVM)this).IsSimulation)
		{
			if (_sallyOutEndLogic == null)
			{
				Mission mission = base._mission;
				_sallyOutEndLogic = ((mission != null) ? mission.GetMissionBehavior<SallyOutEndLogic>() : null);
			}
			if (!((ScoreboardBaseVM)this).IsOver)
			{
				Mission mission2 = base._mission;
				if (mission2 == null || !mission2.IsMissionEnding)
				{
					BattleEndLogic battleEndLogic = base._battleEndLogic;
					if (battleEndLogic == null || !battleEndLogic.IsEnemySideRetreating)
					{
						SallyOutEndLogic sallyOutEndLogic = _sallyOutEndLogic;
						if (sallyOutEndLogic == null || !sallyOutEndLogic.IsSallyOutOver)
						{
							goto IL_0092;
						}
					}
				}
				if (_missionEndScoreboardDelayTimer < 1.5f)
				{
					_missionEndScoreboardDelayTimer += dt;
				}
				else
				{
					OnBattleOver();
				}
			}
		}
		goto IL_0092;
		IL_0092:
		if (!((ScoreboardBaseVM)this).IsSimulation && !((ScoreboardBaseVM)this).IsOver)
		{
			((ScoreboardBaseVM)this).MissionTimeInSeconds = (int)Mission.Current.CurrentTime;
		}
		_moraleUpdateTimer += dt;
		if (_moraleUpdateTimer > 1f)
		{
			if (((ScoreboardBaseVM)this).IsSimulation)
			{
				((ScoreboardBaseVM)this).Attackers.Morale = MobileParty.MainParty.MapEvent.AttackerSide.GetSideMorale();
				((ScoreboardBaseVM)this).Defenders.Morale = MobileParty.MainParty.MapEvent.DefenderSide.GetSideMorale();
			}
			else
			{
				((ScoreboardBaseVM)this).Attackers.Morale = ((ScoreboardBaseVM)this).GetBattleMoraleOfSide((BattleSideEnum)1);
				((ScoreboardBaseVM)this).Defenders.Morale = ((ScoreboardBaseVM)this).GetBattleMoraleOfSide((BattleSideEnum)0);
			}
			_moraleUpdateTimer = 0f;
		}
	}

	public override void ExecutePlayAction()
	{
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			_battleSimulation.Play();
		}
	}

	public override void ExecuteFastForwardAction()
	{
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			((ScoreboardBaseVM)this).IsPaused = false;
			if (!((ScoreboardBaseVM)this).IsFastForwarding)
			{
				_battleSimulation.Play();
			}
			else
			{
				_battleSimulation.FastForward();
			}
		}
		else
		{
			Mission.Current.SetFastForwardingFromUI(((ScoreboardBaseVM)this).IsFastForwarding);
		}
	}

	public override void ExecutePauseSimulationAction()
	{
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			((ScoreboardBaseVM)this).IsFastForwarding = false;
			if (!((ScoreboardBaseVM)this).IsPaused)
			{
				_battleSimulation.Play();
			}
			else
			{
				_battleSimulation.Pause();
			}
		}
	}

	public override void ExecuteEndSimulationAction()
	{
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			((ScoreboardBaseVM)this).IsPaused = false;
			((ScoreboardBaseVM)this).IsFastForwarding = false;
			_battleSimulation.Skip();
		}
	}

	public override void ExecuteQuitAction()
	{
		OnExitBattle();
	}

	private void GetBattleRewards(bool playerVictory)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Invalid comparison between Unknown and I4
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Expected O, but got Unknown
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Expected O, but got Unknown
		((Collection<BattleResultVM>)(object)((ScoreboardBaseVM)this).BattleResults).Clear();
		if (playerVictory && PlayerEncounter.Current != null)
		{
			ExplainedNumber renownExplained = default(ExplainedNumber);
			ExplainedNumber influenceExplained = default(ExplainedNumber);
			ExplainedNumber moraleExplained = default(ExplainedNumber);
			float playerEarnedLootRate = default(float);
			Figurehead playerEarnedFigurehead = default(Figurehead);
			PlayerEncounter.Current.GetBattleRewards(ref renownExplained, ref influenceExplained, ref moraleExplained, ref playerEarnedLootRate, ref playerEarnedFigurehead);
			float resultNumber = ((ExplainedNumber)(ref renownExplained)).ResultNumber;
			float resultNumber2 = ((ExplainedNumber)(ref influenceExplained)).ResultNumber;
			float resultNumber3 = ((ExplainedNumber)(ref moraleExplained)).ResultNumber;
			if (resultNumber > 0.1f)
			{
				((Collection<BattleResultVM>)(object)((ScoreboardBaseVM)this).BattleResults).Add(new BattleResultVM(_renownStr.Format(resultNumber), (Func<List<TooltipProperty>>)(() => SandBoxUIHelper.GetExplainedNumberTooltip(ref renownExplained)), (CharacterCode)null));
			}
			if (resultNumber2 > 0.1f)
			{
				((Collection<BattleResultVM>)(object)((ScoreboardBaseVM)this).BattleResults).Add(new BattleResultVM(_influenceStr.Format(resultNumber2), (Func<List<TooltipProperty>>)(() => SandBoxUIHelper.GetExplainedNumberTooltip(ref influenceExplained)), (CharacterCode)null));
			}
			if (resultNumber3 > 0.1f || resultNumber3 < -0.1f)
			{
				((Collection<BattleResultVM>)(object)((ScoreboardBaseVM)this).BattleResults).Add(new BattleResultVM(_moraleStr.Format(resultNumber3), (Func<List<TooltipProperty>>)(() => SandBoxUIHelper.GetExplainedNumberTooltip(ref moraleExplained)), (CharacterCode)null));
			}
			int num = (((int)base.PlayerSide == 1) ? ((Collection<SPScoreboardPartyVM>)(object)((ScoreboardBaseVM)this).Attackers.Parties).Count : ((Collection<SPScoreboardPartyVM>)(object)((ScoreboardBaseVM)this).Defenders.Parties).Count);
			if (playerEarnedLootRate > 0.1f && num > 1)
			{
				playerEarnedLootRate *= 100f;
				((Collection<BattleResultVM>)(object)((ScoreboardBaseVM)this).BattleResults).Add(new BattleResultVM(_lootStr.Format(playerEarnedLootRate), (Func<List<TooltipProperty>>)(() => SandBoxUIHelper.GetBattleLootAwardTooltip(playerEarnedLootRate)), (CharacterCode)null));
			}
			if (playerEarnedFigurehead != null)
			{
				Figurehead obj = playerEarnedFigurehead;
				if (((obj != null) ? ((PropertyObject)obj).Name : null) != (TextObject)null)
				{
					((Collection<BattleResultVM>)(object)((ScoreboardBaseVM)this).BattleResults).Add(new BattleResultVM(((object)_figureheadStr.SetTextVariable("A0", ((object)((PropertyObject)playerEarnedFigurehead).Name)?.ToString() ?? "")).ToString(), (Func<List<TooltipProperty>>)(() => SandBoxUIHelper.GetFigureheadTooltip(playerEarnedFigurehead)), (CharacterCode)null));
				}
				else
				{
					Debug.FailedAssert("Battle rewards contain an invalid figurehead (null or name missing)", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\SPScoreboardVM.cs", "GetBattleRewards", 358);
				}
			}
		}
		foreach (SPScoreboardPartyVM item in (Collection<SPScoreboardPartyVM>)(object)((ScoreboardBaseVM)this).Defenders.Parties)
		{
			foreach (SPScoreboardUnitVM item2 in ((IEnumerable<SPScoreboardUnitVM>)item.Members).Where((SPScoreboardUnitVM member) => member.IsHero && member.Score.Dead > 0))
			{
				if (item2.Character == null)
				{
					Debug.FailedAssert("Scoreboard has a member element without a character", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\SPScoreboardVM.cs", "GetBattleRewards", 375);
					continue;
				}
				MBBindingList<BattleResultVM> battleResults = ((ScoreboardBaseVM)this).BattleResults;
				string? text = ((object)_deadLordStr.SetTextVariable("A0", item2.Character.Name)).ToString();
				Func<List<TooltipProperty>> obj2 = () => new List<TooltipProperty>();
				BasicCharacterObject character = item2.Character;
				((Collection<BattleResultVM>)(object)battleResults).Add(new BattleResultVM(text, obj2, SandBoxUIHelper.GetCharacterCode((CharacterObject)(object)((character is CharacterObject) ? character : null))));
			}
		}
		foreach (SPScoreboardPartyVM item3 in (Collection<SPScoreboardPartyVM>)(object)((ScoreboardBaseVM)this).Attackers.Parties)
		{
			foreach (SPScoreboardUnitVM item4 in ((IEnumerable<SPScoreboardUnitVM>)item3.Members).Where((SPScoreboardUnitVM member) => member.IsHero && member.Score.Dead > 0))
			{
				if (item4.Character == null)
				{
					Debug.FailedAssert("Scoreboard has a member element without a character", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.ViewModelCollection\\SPScoreboardVM.cs", "GetBattleRewards", 392);
					continue;
				}
				MBBindingList<BattleResultVM> battleResults2 = ((ScoreboardBaseVM)this).BattleResults;
				string? text2 = ((object)_deadLordStr.SetTextVariable("A0", item4.Character.Name)).ToString();
				Func<List<TooltipProperty>> obj3 = () => new List<TooltipProperty>();
				BasicCharacterObject character2 = item4.Character;
				((Collection<BattleResultVM>)(object)battleResults2).Add(new BattleResultVM(text2, obj3, SandBoxUIHelper.GetCharacterCode((CharacterObject)(object)((character2 is CharacterObject) ? character2 : null))));
			}
		}
	}

	private void UpdateSimulationResult(bool playerVictory)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			if (playerVictory)
			{
				if (((IEnumerable<MapEventParty>)PlayerEncounter.Battle.PartiesOnSide(base.PlayerSide)).Sum((MapEventParty x) => x.Party.NumberOfHealthyMembers) < 70)
				{
					((ScoreboardBaseVM)this).SimulationResult = "SimulationVictorySmall";
				}
				else
				{
					((ScoreboardBaseVM)this).SimulationResult = "SimulationVictoryLarge";
				}
			}
			else
			{
				((ScoreboardBaseVM)this).SimulationResult = "SimulationDefeat";
			}
		}
		else
		{
			((ScoreboardBaseVM)this).SimulationResult = "NotSimulation";
		}
	}

	public void OnBattleOver()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected I4, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Invalid comparison between Unknown and I4
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected I4, but got Unknown
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Expected I4, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Invalid comparison between Unknown and I4
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Invalid comparison between Unknown and I4
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Invalid comparison between Unknown and I4
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected I4, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Invalid comparison between Unknown and I4
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Invalid comparison between Unknown and I4
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		BattleResultType val = (BattleResultType)(-1);
		if (PlayerEncounter.IsActive && PlayerEncounter.Battle != null)
		{
			((ScoreboardBaseVM)this).IsOver = true;
			bool playerVictory = false;
			if (PlayerEncounter.WinningSide == base.PlayerSide)
			{
				val = (BattleResultType)1;
				playerVictory = true;
			}
			else
			{
				CampaignBattleResult campaignBattleResult = PlayerEncounter.CampaignBattleResult;
				val = ((campaignBattleResult == null || !campaignBattleResult.EnemyPulledBack) ? ((BattleResultType)0) : ((BattleResultType)2));
			}
			GetBattleRewards(playerVictory);
		}
		else
		{
			Mission current = Mission.Current;
			if (current != null && current.MissionEnded)
			{
				((ScoreboardBaseVM)this).IsOver = true;
				val = (((!Mission.Current.HasMissionBehavior<SallyOutEndLogic>() || Mission.Current.MissionResult.BattleResolved) && !Mission.Current.MissionResult.PlayerVictory) ? (((int)Mission.Current.MissionResult.BattleState != 3 || (int)Mission.Current.PlayerTeam.Side != 1) ? ((BattleResultType)0) : ((BattleResultType)2)) : ((BattleResultType)1));
			}
			else
			{
				BattleEndLogic battleEndLogic = base._battleEndLogic;
				if (battleEndLogic != null && battleEndLogic.IsEnemySideRetreating)
				{
					((ScoreboardBaseVM)this).IsOver = true;
				}
			}
		}
		switch (val - -1)
		{
		case 1:
			((ScoreboardBaseVM)this).BattleResult = ((object)GameTexts.FindText("str_defeat", (string)null)).ToString();
			((ScoreboardBaseVM)this).BattleResultIndex = (int)val;
			break;
		case 2:
			if (PlayerEncounter.Battle != null && PlayerEncounter.Battle.EndedByRetreat)
			{
				((ScoreboardBaseVM)this).BattleResult = (((int)PlayerEncounter.Battle.RetreatingSide == 1) ? ((object)GameTexts.FindText("str_attackers_retreated", (string)null)).ToString() : ((object)GameTexts.FindText("str_defenders_retreated", (string)null)).ToString());
			}
			else
			{
				((ScoreboardBaseVM)this).BattleResult = ((object)GameTexts.FindText("str_victory", (string)null)).ToString();
			}
			((ScoreboardBaseVM)this).BattleResultIndex = (int)val;
			break;
		case 3:
			((ScoreboardBaseVM)this).BattleResult = ((object)GameTexts.FindText("str_battle_result_retreat", (string)null)).ToString();
			((ScoreboardBaseVM)this).BattleResultIndex = (int)val;
			break;
		}
		if ((int)val != -1)
		{
			UpdateSimulationResult((int)val == 1 || (int)val == 2);
		}
	}

	public void OnExitBattle()
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Invalid comparison between Unknown and I4
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Invalid comparison between Unknown and I4
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Invalid comparison between Unknown and I4
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Invalid comparison between Unknown and I4
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			if (_battleSimulation.IsSimulationFinished)
			{
				base._releaseSimulationSources();
				_battleSimulation.OnFinished();
				return;
			}
			Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
			InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_order_Retreat", (string)null)).ToString(), ((object)GameTexts.FindText("str_retreat_question", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_ok", (string)null)).ToString(), ((object)GameTexts.FindText("str_cancel", (string)null)).ToString(), (Action)delegate
			{
				Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
				base._releaseSimulationSources();
				_battleSimulation.OnPlayerRetreat();
			}, (Action)delegate
			{
				Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
			}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
			return;
		}
		BattleEndLogic missionBehavior = base._mission.GetMissionBehavior<BattleEndLogic>();
		BasicMissionHandler missionBehavior2 = base._mission.GetMissionBehavior<BasicMissionHandler>();
		ExitResult val = (ExitResult)((missionBehavior == null) ? ((!base._mission.MissionEnded) ? 1 : 3) : ((int)missionBehavior.TryExit()));
		if ((int)val == 1 || (int)val == 2)
		{
			base.OnToggle(obj: false);
			missionBehavior2.CreateWarningWidgetForResult(val);
		}
		else if ((int)val == 0)
		{
			InformationManager.ShowInquiry(base._retreatInquiryData, false, false);
		}
		else if (missionBehavior == null && (int)val == 3)
		{
			base._mission.EndMission();
		}
	}

	public void TroopNumberChanged(BattleSideEnum side, IBattleCombatant battleCombatant, BasicCharacterObject character, int number = 0, int numberDead = 0, int numberWounded = 0, int numberRouted = 0, int numberKilled = 0, int numberReadyToUpgrade = 0)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		IBattleCombatant obj = ((battleCombatant is PartyBase) ? battleCombatant : null);
		bool flag = ((obj != null) ? ((PartyBase)obj).Owner : null) == Hero.MainHero;
		((ScoreboardBaseVM)this).GetSide(side).UpdateScores(battleCombatant, flag, character, number, numberDead, numberWounded, numberRouted, numberKilled, numberReadyToUpgrade);
		((ScoreboardBaseVM)this).PowerComparer.Update((double)((ScoreboardBaseVM)this).Defenders.CurrentPower, (double)((ScoreboardBaseVM)this).Attackers.CurrentPower, (double)((ScoreboardBaseVM)this).Defenders.InitialPower, (double)((ScoreboardBaseVM)this).Attackers.InitialPower);
	}

	public void HeroSkillIncreased(BattleSideEnum side, IBattleCombatant battleCombatant, BasicCharacterObject heroCharacter, SkillObject upgradedSkill)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		IBattleCombatant obj = ((battleCombatant is PartyBase) ? battleCombatant : null);
		bool flag = ((obj != null) ? ((PartyBase)obj).Owner : null) == Hero.MainHero;
		((ScoreboardBaseVM)this).GetSide(side).UpdateHeroSkills(battleCombatant, flag, heroCharacter, upgradedSkill);
	}

	public void BattleResultsReady()
	{
		if (!((ScoreboardBaseVM)this).IsOver)
		{
			OnBattleOver();
		}
	}

	public void TroopSideChanged(BattleSideEnum prevSide, BattleSideEnum newSide, IBattleCombatant battleCombatant, BasicCharacterObject character)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		SPScoreboardStatsVM val = ((ScoreboardBaseVM)this).GetSide(prevSide).RemoveTroop(battleCombatant, character);
		SPScoreboardSideVM side = ((ScoreboardBaseVM)this).GetSide(newSide);
		IBattleCombatant obj = ((battleCombatant is PartyBase) ? battleCombatant : null);
		side.GetPartyAddIfNotExists(battleCombatant, ((obj != null) ? ((PartyBase)obj).Owner : null) == Hero.MainHero);
		((ScoreboardBaseVM)this).GetSide(newSide).AddTroop(battleCombatant, character, val);
	}
}
