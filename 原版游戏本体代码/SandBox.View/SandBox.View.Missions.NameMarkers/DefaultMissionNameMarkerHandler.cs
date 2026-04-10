using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Towns;
using SandBox.Objects;
using SandBox.Objects.AreaMarkers;
using SandBox.ViewModelCollection.Missions.NameMarker;
using SandBox.ViewModelCollection.Missions.NameMarker.Targets;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace SandBox.View.Missions.NameMarkers;

public class DefaultMissionNameMarkerHandler : MissionNameMarkerProvider
{
	private MissionMode _lastMissionMode;

	private DisguiseMissionLogic _disguiseMissionLogic;

	protected override void OnInitialize(Mission mission)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.OnInitialize(mission);
		_disguiseMissionLogic = mission.GetMissionBehavior<DisguiseMissionLogic>();
		_lastMissionMode = mission.Mode;
	}

	protected override void OnDestroy(Mission mission)
	{
		base.OnDestroy(mission);
	}

	protected override void OnTick(float dt)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.OnTick(dt);
		MissionMode lastMissionMode = _lastMissionMode;
		Mission current = Mission.Current;
		if ((MissionMode?)lastMissionMode != ((current != null) ? new MissionMode?(current.Mode) : ((MissionMode?)null)))
		{
			SetMarkersDirty();
			_lastMissionMode = Mission.Current.Mode;
		}
	}

	public override void CreateMarkers(List<MissionNameMarkerTargetBaseVM> markers)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		Mission current = Mission.Current;
		if (current.MainAgent == null || (int)current.Mode == 2 || (int)current.Mode == 6)
		{
			return;
		}
		List<MissionAgentMarkerTargetVM> list = new List<MissionAgentMarkerTargetVM>();
		foreach (Agent item in (List<Agent>)(object)current.Agents)
		{
			AddAgentTarget(item, list);
		}
		for (int i = 0; i < list.Count; i++)
		{
			markers.Add(list[i]);
		}
		if (Hero.MainHero.CurrentSettlement == null)
		{
			return;
		}
		List<CommonAreaMarker> list2 = MBExtensions.FindAllWithType<CommonAreaMarker>((IEnumerable<MissionObject>)current.ActiveMissionObjects).Where(delegate(CommonAreaMarker x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)x).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).HasTag("alley_marker");
		}).ToList();
		if (Hero.MainHero.CurrentSettlement.Alleys.Count > 0)
		{
			foreach (CommonAreaMarker item2 in list2)
			{
				Alley alley = item2.GetAlley();
				if (alley != null && ((SettlementArea)alley).Owner != null)
				{
					markers.Add(new MissionCommonAreaMarkerTargetVM(item2));
				}
			}
		}
		List<PassageUsePoint> source = MBExtensions.FindAllWithType<PassageUsePoint>((IEnumerable<MissionObject>)current.ActiveMissionObjects).ToList();
		List<string> passagePointFilter = new List<string> { "Empty Shop" };
		foreach (PassageUsePoint item3 in source.Where((PassageUsePoint passage) => passage.ToLocation != null && !passagePointFilter.Exists((string s) => passage.ToLocation.Name.Contains(s))))
		{
			if (!item3.ToLocation.CanBeReserved || item3.ToLocation.IsReserved)
			{
				markers.Add(new MissionPassageUsePointNameMarkerTargetVM(item3));
			}
		}
		foreach (BasicAreaIndicator item4 in from b in MBExtensions.FindAllWithType<BasicAreaIndicator>((IEnumerable<MissionObject>)current.ActiveMissionObjects).ToList()
			where b.IsActive
			select b)
		{
			markers.Add(new MissionBasicAreaIndicatorMarkerTargetVM(item4, ((AreaMarker)item4).GetPosition()));
		}
		if (!current.HasMissionBehavior<WorkshopMissionHandler>())
		{
			return;
		}
		foreach (Tuple<Workshop, GameEntity> item5 in from s in current.GetMissionBehavior<WorkshopMissionHandler>().WorkshopSignEntities.ToList()
			where s.Item1.WorkshopType != null
			select s)
		{
			markers.Add(new MissionWorkshopNameMarkerTargetVM(item5.Item1, item5.Item2.GlobalPosition - MissionNameMarkerHelper.DefaultHeightOffset));
		}
	}

	private void AddAgentTarget(Agent agent, List<MissionAgentMarkerTargetVM> markers, bool isAdditional = false)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Invalid comparison between Unknown and I4
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		Agent obj = agent;
		if (((obj != null) ? obj.Character : null) == null || agent == Agent.Main || !agent.IsActive() || markers.Any((MissionAgentMarkerTargetVM t) => t.Target == agent))
		{
			return;
		}
		if (!isAdditional && !agent.Character.IsHero)
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement != null)
			{
				LocationComplex locationComplex = currentSettlement.LocationComplex;
				if (((locationComplex == null) ? ((bool?)null) : locationComplex.FindCharacter((IAgent)(object)agent)?.IsVisualTracked) == true)
				{
					goto IL_0217;
				}
			}
			BasicCharacterObject character = agent.Character;
			CharacterObject val;
			if ((val = (CharacterObject)(object)((character is CharacterObject) ? character : null)) == null || ((int)val.Occupation != 9 && (int)val.Occupation != 1))
			{
				BasicCharacterObject character2 = agent.Character;
				Settlement currentSettlement2 = Settlement.CurrentSettlement;
				object obj2;
				if (currentSettlement2 == null)
				{
					obj2 = null;
				}
				else
				{
					CultureObject culture = currentSettlement2.Culture;
					obj2 = ((culture != null) ? culture.Blacksmith : null);
				}
				if (character2 != obj2)
				{
					BasicCharacterObject character3 = agent.Character;
					Settlement currentSettlement3 = Settlement.CurrentSettlement;
					object obj3;
					if (currentSettlement3 == null)
					{
						obj3 = null;
					}
					else
					{
						CultureObject culture2 = currentSettlement3.Culture;
						obj3 = ((culture2 != null) ? culture2.Barber : null);
					}
					if (character3 != obj3)
					{
						BasicCharacterObject character4 = agent.Character;
						Settlement currentSettlement4 = Settlement.CurrentSettlement;
						object obj4;
						if (currentSettlement4 == null)
						{
							obj4 = null;
						}
						else
						{
							CultureObject culture3 = currentSettlement4.Culture;
							obj4 = ((culture3 != null) ? culture3.TavernGamehost : null);
						}
						if (character4 != obj4)
						{
							BasicCharacterObject character5 = agent.Character;
							Settlement currentSettlement5 = Settlement.CurrentSettlement;
							object obj5;
							if (currentSettlement5 == null)
							{
								obj5 = null;
							}
							else
							{
								CultureObject culture4 = currentSettlement5.Culture;
								obj5 = ((culture4 != null) ? culture4.Merchant : null);
							}
							if (character5 != obj5 && !(((MBObjectBase)agent.Character).StringId == "sp_hermit"))
							{
								BasicCharacterObject character6 = agent.Character;
								Settlement currentSettlement6 = Settlement.CurrentSettlement;
								object obj6;
								if (currentSettlement6 == null)
								{
									obj6 = null;
								}
								else
								{
									CultureObject culture5 = currentSettlement6.Culture;
									obj6 = ((culture5 != null) ? culture5.Shipwright : null);
								}
								if (character6 != obj6)
								{
									DisguiseMissionLogic disguiseMissionLogic = _disguiseMissionLogic;
									if (disguiseMissionLogic == null || !disguiseMissionLogic.IsContactAgentTracked(agent))
									{
										return;
									}
								}
							}
						}
					}
				}
			}
		}
		goto IL_0217;
		IL_0217:
		MissionAgentMarkerTargetVM item = new MissionAgentMarkerTargetVM(agent);
		markers.Add(item);
	}
}
