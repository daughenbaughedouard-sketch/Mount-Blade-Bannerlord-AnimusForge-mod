using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NavalDLC.Missions.BattleScore;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.BattleScore;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Scoreboard;

namespace NavalDLC.ViewModelCollection;

public class NavalScoreboardVM : SPScoreboardVM
{
	private class ScoreboardShipComparer : IComparer<SPScoreboardShipVM>
	{
		public int Compare(SPScoreboardShipVM x, SPScoreboardShipVM y)
		{
			bool value = (object)x.Owner == PartyBase.MainParty;
			int num = ((object)y.Owner == PartyBase.MainParty).CompareTo(value);
			if (num != 0)
			{
				return num;
			}
			num = y.IsPlayerTeam.CompareTo(x.IsPlayerTeam);
			if (num != 0)
			{
				return num;
			}
			IBattleCombatant owner = x.Owner;
			string? obj = ((owner != null) ? ((object)owner.Name).ToString() : null) ?? string.Empty;
			IBattleCombatant owner2 = y.Owner;
			string strB = ((owner2 != null) ? ((object)owner2.Name).ToString() : null) ?? string.Empty;
			num = obj.CompareTo(strB);
			if (num != 0)
			{
				return num;
			}
			return ResolveEquality(x, y);
		}

		private int ResolveEquality(SPScoreboardShipVM x, SPScoreboardShipVM y)
		{
			IShipOrigin ship = y.Ship;
			ref int reference = ref ((Ship)((ship is Ship) ? ship : null)).ShipHull.Value;
			IShipOrigin ship2 = x.Ship;
			return reference.CompareTo(((Ship)((ship2 is Ship) ? ship2 : null)).ShipHull.Value);
		}
	}

	private NavalShipsLogic _navalShipsLogic;

	private ScoreboardShipComparer _scoreboardShipComparer = new ScoreboardShipComparer();

	public static NavalScoreboardVM CreateSimulation(BattleSimulation simulation)
	{
		return new NavalScoreboardVM((BattleScoreContext)(object)new NavalSimulationBattleScoreContext(simulation), simulation);
	}

	public static NavalScoreboardVM CreateMission(Mission mission)
	{
		return new NavalScoreboardVM((BattleScoreContext)(object)new NavalBattleScoreContext(mission), null);
	}

	public static NavalScoreboardVM CreateCustom(BattleScoreContext battleScoreContext, BattleSimulation simulation = null)
	{
		return new NavalScoreboardVM(battleScoreContext, simulation);
	}

	private NavalScoreboardVM(BattleScoreContext scoreboardContext, BattleSimulation simulation)
		: base(scoreboardContext, simulation)
	{
		SPScoreboardShipVM.GetTooltip = GetShipTooltip;
		((ScoreboardBaseVM)this).IsNavalBattle = true;
	}

	public override void Initialize(IMissionScreen missionScreen, Mission mission, Action releaseSimulationSources, Action<bool> onToggle)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Invalid comparison between Unknown and I4
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		((SPScoreboardVM)this).Initialize(missionScreen, mission, releaseSimulationSources, onToggle);
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			MobileParty mainParty = MobileParty.MainParty;
			if (mainParty != null)
			{
				MapEvent mapEvent = mainParty.MapEvent;
				if (((mapEvent != null) ? new bool?(mapEvent.IsNavalMapEvent) : ((bool?)null)) == true)
				{
					goto IL_006e;
				}
			}
			Debug.FailedAssert("Naval scoreboard initialized in simulation mode, but the current map event isn't naval!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalScoreboardVM.cs", "Initialize", 54);
			return;
		}
		goto IL_006e;
		IL_006e:
		if (!((ScoreboardBaseVM)this).IsSimulation)
		{
			Mission current = Mission.Current;
			if (current == null || !current.IsNavalBattle)
			{
				Debug.FailedAssert("Naval scoreboard initialized in mission mode, but the current mission isn't naval!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\NavalScoreboardVM.cs", "Initialize", 59);
				return;
			}
		}
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			bool flag = (int)MobileParty.MainParty.MapEvent.PlayerSide == 1;
			using (List<Ship>.Enumerator enumerator = ((List<Ship>)(object)MobileParty.MainParty.MapEvent.AttackerSide.SimulationShipList).GetEnumerator())
			{
				Ship current2;
				TeamSideEnum val;
				for (; enumerator.MoveNext(); ((ScoreboardBaseVM)this).Attackers.GetShipAddIfNotExists((IShipOrigin)(object)current2, ((object)current2.ShipHull.Type/*cast due to .constrained prefix*/).ToString(), (IBattleCombatant)(object)current2.Owner, val))
				{
					current2 = enumerator.Current;
					if (flag)
					{
						if (current2.Owner != PartyBase.MainParty)
						{
							Army army = MobileParty.MainParty.Army;
							if (army == null || !army.DoesLeaderPartyAndAttachedPartiesContain(current2.Owner.MobileParty))
							{
								val = (TeamSideEnum)1;
								continue;
							}
						}
						val = (TeamSideEnum)0;
					}
					else
					{
						val = (TeamSideEnum)2;
					}
				}
			}
			using (List<Ship>.Enumerator enumerator = ((List<Ship>)(object)MobileParty.MainParty.MapEvent.DefenderSide.SimulationShipList).GetEnumerator())
			{
				Ship current3;
				TeamSideEnum val2;
				for (; enumerator.MoveNext(); ((ScoreboardBaseVM)this).Defenders.GetShipAddIfNotExists((IShipOrigin)(object)current3, ((object)current3.ShipHull.Type/*cast due to .constrained prefix*/).ToString(), (IBattleCombatant)(object)current3.Owner, val2))
				{
					current3 = enumerator.Current;
					if (flag)
					{
						val2 = (TeamSideEnum)2;
						continue;
					}
					if (current3.Owner != PartyBase.MainParty)
					{
						Army army2 = MobileParty.MainParty.Army;
						if (army2 == null || !army2.DoesLeaderPartyAndAttachedPartiesContain(current3.Owner.MobileParty))
						{
							val2 = (TeamSideEnum)1;
							continue;
						}
					}
					val2 = (TeamSideEnum)0;
				}
			}
			((ScoreboardBaseVM)this).Attackers.Ships.Sort((IComparer<SPScoreboardShipVM>)_scoreboardShipComparer);
			((ScoreboardBaseVM)this).Defenders.Ships.Sort((IComparer<SPScoreboardShipVM>)_scoreboardShipComparer);
		}
		else
		{
			_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		}
	}

	protected override void OnTick(float dt)
	{
		((SPScoreboardVM)this).OnTick(dt);
		if (((ScoreboardBaseVM)this).IsSimulation)
		{
			for (int i = 0; i < ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships).Count; i++)
			{
				((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships)[i].CurrentHealth = ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships)[i].Ship.HitPoints;
			}
			for (int j = 0; j < ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships).Count; j++)
			{
				((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships)[j].CurrentHealth = ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships)[j].Ship.HitPoints;
			}
		}
		else
		{
			if (_navalShipsLogic == null)
			{
				return;
			}
			UpdateTeamShips(removeOld: false, addNew: true, sort: false);
			for (int k = 0; k < ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships).Count; k++)
			{
				SPScoreboardShipVM val = ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships)[k];
				ShipAssignment shipAssignment;
				bool flag = _navalShipsLogic.FindAssignmentOfShipOrigin(val.Ship, out shipAssignment);
				val.CurrentHealth = (flag ? shipAssignment.MissionShip.HitPoints : 0f);
				int isInactive;
				if (flag)
				{
					Formation formation = shipAssignment.Formation;
					isInactive = ((formation != null && formation.CountOfUnits == 0) ? 1 : 0);
				}
				else
				{
					isInactive = 1;
				}
				val.IsInactive = (byte)isInactive != 0;
			}
			for (int l = 0; l < ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships).Count; l++)
			{
				SPScoreboardShipVM val2 = ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships)[l];
				ShipAssignment shipAssignment2;
				bool flag2 = _navalShipsLogic.FindAssignmentOfShipOrigin(val2.Ship, out shipAssignment2);
				val2.CurrentHealth = (flag2 ? shipAssignment2.MissionShip.HitPoints : 0f);
				int isInactive2;
				if (flag2)
				{
					Formation formation2 = shipAssignment2.Formation;
					isInactive2 = ((formation2 != null && formation2.CountOfUnits == 0) ? 1 : 0);
				}
				else
				{
					isInactive2 = 1;
				}
				val2.IsInactive = (byte)isInactive2 != 0;
			}
		}
	}

	public override void OnFinalize()
	{
		((ScoreboardBaseVM)this).OnFinalize();
		SPScoreboardShipVM.GetTooltip = null;
	}

	public override void OnDeploymentFinished()
	{
		((ScoreboardBaseVM)this).OnDeploymentFinished();
		UpdateTeamShips(removeOld: true, addNew: true, sort: true);
	}

	private void UpdateTeamShips(bool removeOld, bool addNew, bool sort)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		if (removeOld)
		{
			ShipAssignment shipAssignment;
			for (int num = ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships).Count - 1; num >= 0; num--)
			{
				if (!_navalShipsLogic.FindAssignmentOfShipOrigin(((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships)[num].Ship, out shipAssignment))
				{
					((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Attackers.Ships).RemoveAt(num);
				}
			}
			for (int num2 = ((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships).Count - 1; num2 >= 0; num2--)
			{
				if (!_navalShipsLogic.FindAssignmentOfShipOrigin(((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships)[num2].Ship, out shipAssignment))
				{
					((Collection<SPScoreboardShipVM>)(object)((ScoreboardBaseVM)this).Defenders.Ships).RemoveAt(num2);
				}
			}
		}
		if (addNew)
		{
			MBList<MissionShip> val = new MBList<MissionShip>();
			_navalShipsLogic.FillTeamShips(Mission.Current.AttackerTeam.TeamSide, val);
			MBList<MissionShip> val2 = new MBList<MissionShip>();
			if (Mission.Current.AttackerAllyTeam != null)
			{
				_navalShipsLogic.FillTeamShips(Mission.Current.AttackerAllyTeam.TeamSide, val2);
			}
			MBList<MissionShip> val3 = new MBList<MissionShip>();
			_navalShipsLogic.FillTeamShips(Mission.Current.DefenderTeam.TeamSide, val3);
			MBList<MissionShip> val4 = new MBList<MissionShip>();
			if (Mission.Current.DefenderAllyTeam != null)
			{
				_navalShipsLogic.FillTeamShips(Mission.Current.DefenderAllyTeam.TeamSide, val4);
			}
			for (int i = 0; i < ((List<MissionShip>)(object)val).Count; i++)
			{
				MissionShip missionShip = ((List<MissionShip>)(object)val)[i];
				_003F val5 = ((ScoreboardBaseVM)this).Attackers;
				IShipOrigin shipOrigin = missionShip.ShipOrigin;
				string? text = ((object)missionShip.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString();
				IShipOrigin shipOrigin2 = missionShip.ShipOrigin;
				((SPScoreboardSideVM)val5).GetShipAddIfNotExists(shipOrigin, text, (IBattleCombatant)(object)((Ship)((shipOrigin2 is Ship) ? shipOrigin2 : null)).Owner, Mission.Current.AttackerTeam.TeamSide);
			}
			for (int j = 0; j < ((List<MissionShip>)(object)val2).Count; j++)
			{
				MissionShip missionShip2 = ((List<MissionShip>)(object)val2)[j];
				_003F val6 = ((ScoreboardBaseVM)this).Attackers;
				IShipOrigin shipOrigin3 = missionShip2.ShipOrigin;
				string? text2 = ((object)missionShip2.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString();
				IShipOrigin shipOrigin4 = missionShip2.ShipOrigin;
				((SPScoreboardSideVM)val6).GetShipAddIfNotExists(shipOrigin3, text2, (IBattleCombatant)(object)((Ship)((shipOrigin4 is Ship) ? shipOrigin4 : null)).Owner, Mission.Current.AttackerAllyTeam.TeamSide);
			}
			for (int k = 0; k < ((List<MissionShip>)(object)val3).Count; k++)
			{
				MissionShip missionShip3 = ((List<MissionShip>)(object)val3)[k];
				_003F val7 = ((ScoreboardBaseVM)this).Defenders;
				IShipOrigin shipOrigin5 = missionShip3.ShipOrigin;
				string? text3 = ((object)missionShip3.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString();
				IShipOrigin shipOrigin6 = missionShip3.ShipOrigin;
				((SPScoreboardSideVM)val7).GetShipAddIfNotExists(shipOrigin5, text3, (IBattleCombatant)(object)((Ship)((shipOrigin6 is Ship) ? shipOrigin6 : null)).Owner, Mission.Current.DefenderTeam.TeamSide);
			}
			for (int l = 0; l < ((List<MissionShip>)(object)val4).Count; l++)
			{
				MissionShip missionShip4 = ((List<MissionShip>)(object)val4)[l];
				_003F val8 = ((ScoreboardBaseVM)this).Defenders;
				IShipOrigin shipOrigin7 = missionShip4.ShipOrigin;
				string? text4 = ((object)missionShip4.ShipOrigin.Hull.Type/*cast due to .constrained prefix*/).ToString();
				IShipOrigin shipOrigin8 = missionShip4.ShipOrigin;
				((SPScoreboardSideVM)val8).GetShipAddIfNotExists(shipOrigin7, text4, (IBattleCombatant)(object)((Ship)((shipOrigin8 is Ship) ? shipOrigin8 : null)).Owner, Mission.Current.DefenderAllyTeam.TeamSide);
			}
		}
		if (sort)
		{
			((ScoreboardBaseVM)this).Attackers.Ships.Sort((IComparer<SPScoreboardShipVM>)_scoreboardShipComparer);
			((ScoreboardBaseVM)this).Defenders.Ships.Sort((IComparer<SPScoreboardShipVM>)_scoreboardShipComparer);
		}
	}

	private List<TooltipProperty> GetShipTooltip(SPScoreboardShipVM shipVM)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Expected O, but got Unknown
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Expected O, but got Unknown
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Expected O, but got Unknown
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Expected O, but got Unknown
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Expected O, but got Unknown
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Expected O, but got Unknown
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Expected O, but got Unknown
		IShipOrigin ship = shipVM.Ship;
		List<TooltipProperty> list = new List<TooltipProperty>
		{
			new TooltipProperty(((object)ship.Name).ToString(), string.Empty, 0, false, (TooltipPropertyFlags)4096)
		};
		if (shipVM.IsDestroyed)
		{
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=w8Yzf0F0}Destroyed", (Dictionary<string, object>)null)).ToString(), -1, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0));
		}
		if (shipVM.Owner != null)
		{
			list.Add(new TooltipProperty(((object)GameTexts.FindText("str_owner", (string)null)).ToString(), ((object)shipVM.Owner.Name).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		list.Add(new TooltipProperty(((object)new TextObject("{=wEmx6fZi}Hull", (Dictionary<string, object>)null)).ToString(), ((object)ship.Hull.Name).ToString(), 0, false, (TooltipPropertyFlags)0));
		list.Add(new TooltipProperty(((object)new TextObject("{=sqdzHOPe}Class", (Dictionary<string, object>)null)).ToString(), ((object)GameTexts.FindText("str_ship_type", ((object)ship.Hull.Type/*cast due to .constrained prefix*/).ToString().ToLowerInvariant())).ToString(), 0, false, (TooltipPropertyFlags)0));
		MissionShip missionShip = null;
		if (_navalShipsLogic != null && _navalShipsLogic.FindAssignmentOfShipOrigin(ship, out var shipAssignment))
		{
			missionShip = shipAssignment.MissionShip;
		}
		if (missionShip == null)
		{
			string text = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", (int)ship.HitPoints).SetTextVariable("RIGHT", (int)ship.MaxHitPoints)).ToString();
			list.Add(new TooltipProperty(((object)new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null)).ToString(), text, 0, false, (TooltipPropertyFlags)0));
		}
		else
		{
			string text2 = ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", (int)missionShip.HitPoints).SetTextVariable("RIGHT", (int)ship.MaxHitPoints)).ToString();
			list.Add(new TooltipProperty(((object)new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null)).ToString(), text2, 0, false, (TooltipPropertyFlags)0));
			TextObject obj = GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null);
			Formation formation = missionShip.Formation;
			string text3 = ((object)obj.SetTextVariable("LEFT", (formation != null) ? formation.CountOfUnits : 0).SetTextVariable("RIGHT", missionShip.CrewSizeOnMainDeck)).ToString();
			list.Add(new TooltipProperty(((object)new TextObject("{=aClquusd}Troop Count", (Dictionary<string, object>)null)).ToString(), text3, 0, false, (TooltipPropertyFlags)0));
		}
		List<ShipSlotAndPieceName> shipSlotAndPieceNames = ship.GetShipSlotAndPieceNames();
		if (shipSlotAndPieceNames.Count > 0)
		{
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024)
			{
				OnlyShowWhenExtended = true
			});
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=zMvUzdKR}Ship Upgrades", (Dictionary<string, object>)null)).ToString(), -1, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenExtended = true
			});
			foreach (ShipSlotAndPieceName item in shipSlotAndPieceNames)
			{
				list.Add(new TooltipProperty(item.SlotName, item.PieceName, 0, false, (TooltipPropertyFlags)0)
				{
					OnlyShowWhenExtended = true
				});
			}
		}
		if (shipSlotAndPieceNames.Count > 0)
		{
			if (Input.IsGamepadActive)
			{
				GameTexts.SetVariable("EXTEND_KEY", ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", "MapFollowModifier")).ToString());
			}
			else
			{
				GameTexts.SetVariable("EXTEND_KEY", ((object)Game.Current.GameTextManager.FindText("str_game_key_text", "anyalt")).ToString());
			}
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
			list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_map_tooltip_info", (string)null)).ToString(), -1, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
		}
		return list;
	}
}
