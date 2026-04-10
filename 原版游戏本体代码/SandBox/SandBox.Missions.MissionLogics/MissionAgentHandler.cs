using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Helpers;
using SandBox.Objects;
using SandBox.Objects.AnimationPoints;
using SandBox.Objects.Usables;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.Source.Objects;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionLogics;

public class MissionAgentHandler : MissionLogic
{
	private const float PassageUsageDeltaTime = 30f;

	private static readonly uint[] _tournamentTeamColors = new uint[11]
	{
		4294110933u, 4290269521u, 4291535494u, 4286151096u, 4290286497u, 4291600739u, 4291868275u, 4287285710u, 4283204487u, 4287282028u,
		4290300789u
	};

	private static readonly uint[] _villagerClothColors = new uint[35]
	{
		4292860590u, 4291351206u, 4289117081u, 4288460959u, 4287541416u, 4288922566u, 4292654718u, 4289243320u, 4290286483u, 4290288531u,
		4290156159u, 4291136871u, 4289233774u, 4291205980u, 4291735684u, 4292722283u, 4293119406u, 4293911751u, 4294110933u, 4291535494u,
		4289955192u, 4289631650u, 4292133587u, 4288785593u, 4286288275u, 4286222496u, 4287601851u, 4286622134u, 4285898909u, 4285638289u,
		4289830302u, 4287593853u, 4289957781u, 4287071646u, 4284445583u
	};

	private static int _disabledFaceId = -1;

	private static int _disabledFaceIdForAnimals = 1;

	private readonly Dictionary<string, List<UsableMachine>> _usablePoints;

	private readonly Dictionary<string, List<UsableMachine>> _pairedUsablePoints;

	private readonly HashSet<UsableMachine> _usedSpawnPoints;

	private List<UsableMachine> _disabledPassages;

	private readonly List<(LocationCharacter, MatrixFrame, GameEntity, bool, bool, Timer)> _spawnTimers = new List<(LocationCharacter, MatrixFrame, GameEntity, bool, bool, Timer)>();

	private float _passageUsageTime;

	public List<UsableMachine> TownPassageProps
	{
		get
		{
			_usablePoints.TryGetValue("npc_passage", out var value);
			return value;
		}
	}

	public List<UsableMachine> DisabledPassages => _disabledPassages;

	public List<UsableMachine> UsablePoints
	{
		get
		{
			List<UsableMachine> list = new List<UsableMachine>();
			foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
			{
				list.AddRange(usablePoint.Value);
			}
			foreach (KeyValuePair<string, List<UsableMachine>> pairedUsablePoint in _pairedUsablePoints)
			{
				list.AddRange(pairedUsablePoint.Value);
			}
			return list;
		}
	}

	public bool HasPassages()
	{
		if (_usablePoints.TryGetValue("npc_passage", out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public MissionAgentHandler()
	{
		_usablePoints = new Dictionary<string, List<UsableMachine>>();
		_pairedUsablePoints = new Dictionary<string, List<UsableMachine>>();
		_usedSpawnPoints = new HashSet<UsableMachine>();
		_disabledPassages = new List<UsableMachine>();
	}

	public override void EarlyStart()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		_passageUsageTime = ((MissionBehavior)this).Mission.CurrentTime + 30f;
		GetAllProps();
		MapWeatherModel mapWeatherModel = Campaign.Current.Models.MapWeatherModel;
		CampaignVec2 position = Settlement.CurrentSettlement.Position;
		WeatherEvent weatherEventInPosition = mapWeatherModel.GetWeatherEventInPosition(((CampaignVec2)(ref position)).ToVec2());
		if ((int)weatherEventInPosition != 2 && (int)weatherEventInPosition != 4)
		{
			InitializePairedUsableObjects();
		}
		((MissionBehavior)this).Mission.SetReportStuckAgentsMode(true);
	}

	public override void OnRenderingStarted()
	{
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		float currentTime = ((MissionBehavior)this).Mission.CurrentTime;
		if (currentTime > _passageUsageTime)
		{
			_passageUsageTime = currentTime + 30f;
			if (PlayerEncounter.LocationEncounter != null && LocationComplex.Current != null)
			{
				LocationComplex.Current.AgentPassageUsageTick();
			}
		}
		for (int num = _spawnTimers.Count - 1; num >= 0; num--)
		{
			if (_spawnTimers[num].Item6.Check(currentTime))
			{
				SpawnWanderingAgentWithInitialFrame(_spawnTimers[num].Item1, _spawnTimers[num].Item2, _spawnTimers[num].Item3.WeakEntity, _spawnTimers[num].Item4, _spawnTimers[num].Item5);
				_spawnTimers.RemoveAt(num);
			}
		}
	}

	protected override void OnEndMission()
	{
		_usablePoints.Clear();
		_pairedUsablePoints.Clear();
		_disabledPassages.Clear();
		_usedSpawnPoints.Clear();
	}

	public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		if (atStart || ((int)((MissionBehavior)this).Mission.Mode != 2 && (int)oldMissionMode != 2))
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.IsHuman && !item.IsPlayerControlled)
			{
				item.SetAgentExcludeStateForFaceGroupId(_disabledFaceId, (int)item.CurrentWatchState != 2);
			}
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			item.GetComponent<CampaignAgentComponent>()?.OnAgentRemoved(affectedAgent);
		}
	}

	private void InitializePairedUsableObjects()
	{
		Dictionary<string, List<UsableMachine>> dictionary = new Dictionary<string, List<UsableMachine>>();
		foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
		{
			foreach (UsableMachine item in usablePoint.Value)
			{
				foreach (StandingPoint item2 in (List<StandingPoint>)(object)item.StandingPoints)
				{
					if (!(item2 is AnimationPoint animationPoint) || !(animationPoint.PairEntity != (GameEntity)null))
					{
						continue;
					}
					if (_pairedUsablePoints.ContainsKey(usablePoint.Key))
					{
						if (!_pairedUsablePoints[usablePoint.Key].Contains(item))
						{
							_pairedUsablePoints[usablePoint.Key].Add(item);
						}
					}
					else
					{
						_pairedUsablePoints.Add(usablePoint.Key, new List<UsableMachine> { item });
					}
					if (dictionary.ContainsKey(usablePoint.Key))
					{
						dictionary[usablePoint.Key].Add(item);
						continue;
					}
					dictionary.Add(usablePoint.Key, new List<UsableMachine> { item });
				}
			}
		}
		foreach (KeyValuePair<string, List<UsableMachine>> item3 in dictionary)
		{
			foreach (KeyValuePair<string, List<UsableMachine>> usablePoint2 in _usablePoints)
			{
				foreach (UsableMachine item4 in dictionary[item3.Key])
				{
					usablePoint2.Value.Remove(item4);
				}
			}
		}
	}

	private void GetAllProps()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_057d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("navigation_mesh_deactivator");
		if (val != (GameEntity)null)
		{
			NavigationMeshDeactivator firstScriptOfType = val.GetFirstScriptOfType<NavigationMeshDeactivator>();
			_disabledFaceId = firstScriptOfType.DisableFaceWithId;
			_disabledFaceIdForAnimals = firstScriptOfType.DisableFaceWithIdForAnimals;
		}
		_usablePoints.Clear();
		WeakGameEntity val2;
		foreach (UsableMachine item in MBExtensions.FindAllWithType<UsableMachine>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.MissionObjects))
		{
			val2 = ((ScriptComponentBehavior)item).GameEntity;
			string[] tags = ((WeakGameEntity)(ref val2)).Tags;
			foreach (string text in tags)
			{
				if (!_usablePoints.ContainsKey(text))
				{
					_usablePoints.Add(text, new List<UsableMachine>());
				}
				if (!(text != "sp_guard"))
				{
					val2 = ((ScriptComponentBehavior)item).GameEntity;
					if (((WeakGameEntity)(ref val2)).HasTag("sp_guard_with_spear"))
					{
						continue;
					}
				}
				_usablePoints[text].Add(item);
			}
		}
		if (Settlement.CurrentSettlement != null && (Settlement.CurrentSettlement.IsTown || Settlement.CurrentSettlement.IsVillage))
		{
			foreach (AreaMarker item2 in MBExtensions.FindAllWithType<AreaMarker>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).ToList())
			{
				string tag = item2.Tag;
				List<UsableMachine> usableMachinesInRange = item2.GetUsableMachinesInRange(item2.Tag.Contains("workshop") ? "unaffected_by_area" : null);
				if (!_usablePoints.ContainsKey(tag))
				{
					_usablePoints.Add(tag, new List<UsableMachine>());
				}
				foreach (UsableMachine item3 in usableMachinesInRange)
				{
					foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
					{
						if (usablePoint.Value.Contains(item3))
						{
							usablePoint.Value.Remove(item3);
						}
					}
					val2 = ((ScriptComponentBehavior)item3).GameEntity;
					if (((WeakGameEntity)(ref val2)).HasTag("hold_tag_always"))
					{
						val2 = ((ScriptComponentBehavior)item3).GameEntity;
						string text2 = ((WeakGameEntity)(ref val2)).Tags[0] + "_" + item2.Tag;
						val2 = ((ScriptComponentBehavior)item3).GameEntity;
						((WeakGameEntity)(ref val2)).AddTag(text2);
						if (!_usablePoints.ContainsKey(text2))
						{
							_usablePoints.Add(text2, new List<UsableMachine>());
							_usablePoints[text2].Add(item3);
						}
						else
						{
							_usablePoints[text2].Add(item3);
						}
						continue;
					}
					foreach (UsableMachine item4 in usableMachinesInRange)
					{
						val2 = ((ScriptComponentBehavior)item4).GameEntity;
						if (!((WeakGameEntity)(ref val2)).HasTag(tag))
						{
							val2 = ((ScriptComponentBehavior)item4).GameEntity;
							((WeakGameEntity)(ref val2)).AddTag(tag);
						}
					}
				}
				if (_usablePoints.ContainsKey(tag))
				{
					usableMachinesInRange.RemoveAll((UsableMachine x) => _usablePoints[tag].Contains(x));
					if (usableMachinesInRange.Count > 0)
					{
						_usablePoints[tag].AddRange(usableMachinesInRange);
					}
				}
				foreach (UsableMachine item5 in item2.GetUsableMachinesWithTagInRange("unaffected_by_area"))
				{
					val2 = ((ScriptComponentBehavior)item5).GameEntity;
					string key = ((WeakGameEntity)(ref val2)).Tags[0];
					foreach (KeyValuePair<string, List<UsableMachine>> usablePoint2 in _usablePoints)
					{
						if (usablePoint2.Value.Contains(item5))
						{
							usablePoint2.Value.Remove(item5);
						}
					}
					if (_usablePoints.ContainsKey(key))
					{
						_usablePoints[key].Add(item5);
						continue;
					}
					_usablePoints.Add(key, new List<UsableMachine>());
					_usablePoints[key].Add(item5);
				}
			}
		}
		List<GameEntity> list = new List<GameEntity>();
		((MissionBehavior)this).Mission.Scene.GetAllEntitiesWithScriptComponent<DynamicPatrolAreaParent>(ref list);
		foreach (GameEntity item6 in list)
		{
			foreach (GameEntity child in item6.GetChildren())
			{
				PatrolPoint firstScriptOfType2 = child.GetChild(0).GetFirstScriptOfType<PatrolPoint>();
				if (firstScriptOfType2 != null && !((MissionObject)firstScriptOfType2).IsDisabled && !string.IsNullOrEmpty(firstScriptOfType2.SpawnGroupTag))
				{
					if (_usablePoints.ContainsKey(firstScriptOfType2.SpawnGroupTag))
					{
						List<UsableMachine> list2 = _usablePoints[firstScriptOfType2.SpawnGroupTag];
						val2 = ((ScriptComponentBehavior)firstScriptOfType2).GameEntity;
						val2 = ((WeakGameEntity)(ref val2)).Parent;
						list2.Add((UsableMachine)(object)((WeakGameEntity)(ref val2)).GetFirstScriptOfType<UsablePlace>());
					}
					else
					{
						_usablePoints.Add(firstScriptOfType2.SpawnGroupTag, new List<UsableMachine>());
						List<UsableMachine> list3 = _usablePoints[firstScriptOfType2.SpawnGroupTag];
						val2 = ((ScriptComponentBehavior)firstScriptOfType2).GameEntity;
						val2 = ((WeakGameEntity)(ref val2)).Parent;
						list3.Add((UsableMachine)(object)((WeakGameEntity)(ref val2)).GetFirstScriptOfType<UsablePlace>());
					}
				}
			}
		}
		DisableUnavailableWaypoints();
		RemoveDeactivatedUsablePlacesFromList();
	}

	[Conditional("DEBUG")]
	public void DetectMissingEntities()
	{
		if (CampaignMission.Current.Location == null || Utilities.CommandLineArgumentExists("CampaignGameplayTest"))
		{
			return;
		}
		IEnumerable<LocationCharacter> characterList = CampaignMission.Current.Location.GetCharacterList();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (LocationCharacter item in characterList)
		{
			if (item.SpecialTargetTag != null)
			{
				if (dictionary.ContainsKey(item.SpecialTargetTag))
				{
					dictionary[item.SpecialTargetTag]++;
				}
				else
				{
					dictionary.Add(item.SpecialTargetTag, 1);
				}
			}
		}
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			string key = item2.Key;
			int value = item2.Value;
			int num = 0;
			if (_usablePoints.TryGetValue(key, out var value2))
			{
				num += value2.Count;
				foreach (UsableMachine item3 in value2)
				{
					num += GetPointCountOfUsableMachine(item3, checkForUnusedOnes: false);
				}
			}
			if (_pairedUsablePoints.TryGetValue(key, out var value3))
			{
				num += value3.Count;
				foreach (UsableMachine item4 in value3)
				{
					num += GetPointCountOfUsableMachine(item4, checkForUnusedOnes: false);
				}
			}
			if (num < value)
			{
				_ = "Trying to spawn " + value + " npc with \"" + key + "\" but there are " + num + " suitable spawn points in scene " + ((MissionBehavior)this).Mission.SceneName;
				if (TestCommonBase.BaseInstance != null)
				{
					_ = TestCommonBase.BaseInstance.IsTestEnabled;
				}
			}
		}
	}

	private void RemoveDeactivatedUsablePlacesFromList()
	{
		Dictionary<string, List<UsableMachine>> dictionary = new Dictionary<string, List<UsableMachine>>();
		foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
		{
			foreach (UsableMachine item in usablePoint.Value)
			{
				if (item.IsDeactivated)
				{
					if (dictionary.ContainsKey(usablePoint.Key))
					{
						dictionary[usablePoint.Key].Add(item);
						continue;
					}
					dictionary.Add(usablePoint.Key, new List<UsableMachine>());
					dictionary[usablePoint.Key].Add(item);
				}
			}
		}
		foreach (KeyValuePair<string, List<UsableMachine>> item2 in dictionary)
		{
			foreach (UsableMachine item3 in item2.Value)
			{
				_usablePoints[item2.Key].Remove(item3);
			}
		}
	}

	public Dictionary<string, int> FindUnusedUsablePointCount()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
		{
			int num = 0;
			foreach (UsableMachine item in usablePoint.Value)
			{
				if (!_usedSpawnPoints.Contains(item))
				{
					num += GetPointCountOfUsableMachine(item, checkForUnusedOnes: true);
				}
			}
			if (num > 0)
			{
				dictionary.Add(usablePoint.Key, num);
			}
		}
		foreach (KeyValuePair<string, List<UsableMachine>> pairedUsablePoint in _pairedUsablePoints)
		{
			int num2 = 0;
			foreach (UsableMachine item2 in pairedUsablePoint.Value)
			{
				if (!_usedSpawnPoints.Contains(item2))
				{
					num2 += GetPointCountOfUsableMachine(item2, checkForUnusedOnes: true);
				}
			}
			if (num2 > 0)
			{
				if (!dictionary.ContainsKey(pairedUsablePoint.Key))
				{
					dictionary.Add(pairedUsablePoint.Key, num2);
				}
				else
				{
					dictionary[pairedUsablePoint.Key] += num2;
				}
			}
		}
		return dictionary;
	}

	private void DisableUnavailableWaypoints()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		bool isNight = Campaign.Current.IsNight;
		string text = "";
		int num = 0;
		foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < usablePoint.Value.Count; i++)
			{
				UsableMachine val = usablePoint.Value[i];
				Mission current2 = Mission.Current;
				WeakGameEntity gameEntity = ((ScriptComponentBehavior)val).GameEntity;
				Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
				if (!current2.IsPositionInsideBoundaries(((Vec3)(ref globalPosition)).AsVec2))
				{
					foreach (StandingPoint item in (List<StandingPoint>)(object)val.StandingPoints)
					{
						((UsableMissionObject)item).IsDeactivated = true;
						num++;
					}
				}
				if (val is Chair)
				{
					foreach (StandingPoint item2 in (List<StandingPoint>)(object)val.StandingPoints)
					{
						gameEntity = ((ScriptComponentBehavior)item2).GameEntity;
						Vec3 origin = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
						PathFaceRecord nullFaceRecord = PathFaceRecord.NullFaceRecord;
						((MissionBehavior)this).Mission.Scene.GetNavMeshFaceIndex(ref nullFaceRecord, origin, true);
						if (!((PathFaceRecord)(ref nullFaceRecord)).IsValid() || (_disabledFaceId != -1 && nullFaceRecord.FaceGroupIndex == _disabledFaceId))
						{
							((UsableMissionObject)item2).IsDeactivated = true;
							num2++;
						}
					}
				}
				else if (val is Passage)
				{
					Passage passage = val as Passage;
					if (passage.ToLocation != null && passage.ToLocation.CanPlayerSee())
					{
						continue;
					}
					foreach (StandingPoint item3 in (List<StandingPoint>)(object)((UsableMachine)passage).StandingPoints)
					{
						((UsableMissionObject)item3).IsDeactivated = true;
					}
					((UsableMachine)passage).Disable();
					_disabledPassages.Add(val);
					_ = passage.ToLocation;
					usablePoint.Value.RemoveAt(i);
					i--;
					num3++;
				}
				else
				{
					if (!(val is UsablePlace))
					{
						continue;
					}
					foreach (StandingPoint item4 in (List<StandingPoint>)(object)val.StandingPoints)
					{
						gameEntity = ((ScriptComponentBehavior)item4).GameEntity;
						Vec3 origin2 = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame().origin;
						PathFaceRecord nullFaceRecord2 = PathFaceRecord.NullFaceRecord;
						((MissionBehavior)this).Mission.Scene.GetNavMeshFaceIndex(ref nullFaceRecord2, origin2, true);
						if (((PathFaceRecord)(ref nullFaceRecord2)).IsValid() && (_disabledFaceId == -1 || nullFaceRecord2.FaceGroupIndex != _disabledFaceId))
						{
							if (isNight)
							{
								gameEntity = ((ScriptComponentBehavior)val).GameEntity;
								if (((WeakGameEntity)(ref gameEntity)).HasTag("disable_at_night"))
								{
									goto IL_029c;
								}
							}
							if (isNight)
							{
								continue;
							}
							gameEntity = ((ScriptComponentBehavior)val).GameEntity;
							if (!((WeakGameEntity)(ref gameEntity)).HasTag("enable_at_night"))
							{
								continue;
							}
						}
						goto IL_029c;
						IL_029c:
						((UsableMissionObject)item4).IsDeactivated = true;
						num4++;
					}
				}
			}
			if (num4 + num2 + num3 > 0)
			{
				text = text + "_____________________________________________\n\"" + usablePoint.Key + "\" :\n";
				if (num4 > 0)
				{
					text = text + "Disabled standing point : " + num4 + "\n";
				}
				if (num2 > 0)
				{
					text = text + "Disabled chair use point : " + num2 + "\n";
				}
				if (num3 > 0)
				{
					text = text + "Disabled passage info : " + num3 + "\n";
				}
			}
		}
	}

	public void SpawnLocationCharacters(string overridenTagValue = null)
	{
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).LocationCharactersAreReadyToSpawn(FindUnusedUsablePointCount());
		foreach (LocationCharacter character in CampaignMission.Current.Location.GetCharacterList())
		{
			if (!IsAlreadySpawned(character.AgentOrigin))
			{
				if (!string.IsNullOrEmpty(overridenTagValue))
				{
					character.SpecialTargetTag = overridenTagValue;
				}
				Agent obj = SpawnDefaultLocationCharacter(character);
				if (obj != null)
				{
					obj.SetAgentExcludeStateForFaceGroupId(_disabledFaceId, true);
				}
			}
		}
		List<Passage> list = new List<Passage>();
		if (TownPassageProps != null)
		{
			foreach (UsableMachine townPassageProp in TownPassageProps)
			{
				if (townPassageProp is Passage passage && !townPassageProp.IsDeactivated)
				{
					((UsableMachine)passage).Deactivate();
					list.Add(passage);
				}
			}
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			SimulateAgent(item);
		}
		foreach (Passage item2 in list)
		{
			((UsableMachine)item2).Activate();
		}
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).LocationCharactersSimulated();
	}

	private bool IsAlreadySpawned(IAgentOriginBase agentOrigin)
	{
		if (Mission.Current != null)
		{
			return ((IEnumerable<Agent>)Mission.Current.Agents).Any((Agent x) => x.Origin == agentOrigin);
		}
		return false;
	}

	public Agent SpawnDefaultLocationCharacter(LocationCharacter locationCharacter, bool simulateAgentAfterSpawn = false)
	{
		Agent val = SpawnWanderingAgent(locationCharacter);
		if (val != null)
		{
			if (simulateAgentAfterSpawn)
			{
				SimulateAgent(val);
			}
			if (locationCharacter.IsVisualTracked)
			{
				Mission.Current.GetMissionBehavior<VisualTrackerMissionBehavior>()?.RegisterLocalOnlyObject((ITrackableBase)(object)val);
			}
		}
		return val;
	}

	public void SimulateAgent(Agent agent)
	{
		if (!agent.IsHuman)
		{
			return;
		}
		AgentNavigator agentNavigator = agent.GetComponent<CampaignAgentComponent>().AgentNavigator;
		int num = MBRandom.RandomInt(35, 50);
		agent.PreloadForRendering();
		for (int i = 0; i < num; i++)
		{
			agentNavigator?.Tick(0.1f, isSimulation: true);
			if (agent.IsUsingGameObject)
			{
				agent.CurrentlyUsedGameObject.SimulateTick(0.1f);
			}
		}
	}

	private void GetFrameForFollowingAgent(Agent followedAgent, out MatrixFrame frame)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		frame = followedAgent.Frame;
		ref Vec3 origin = ref frame.origin;
		origin += -(frame.rotation.f * 1.5f);
	}

	public void FadeoutExitingLocationCharacter(LocationCharacter locationCharacter)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and O
		if ((int)((MissionBehavior)this).Mission.CurrentState == 3 || (int)((MissionBehavior)this).Mission.CurrentState == 4)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if ((object)(CharacterObject)item.Character == locationCharacter.Character)
			{
				item.FadeOut(false, true);
				break;
			}
		}
	}

	public void SpawnEnteringLocationCharacter(LocationCharacter locationCharacter, Location fromLocation)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (fromLocation != null)
		{
			bool flag = false;
			{
				foreach (UsableMachine townPassageProp in TownPassageProps)
				{
					Passage passage = townPassageProp as Passage;
					if (passage.ToLocation == fromLocation)
					{
						WeakGameEntity gameEntity = ((ScriptComponentBehavior)((UsableMachine)passage).PilotStandingPoint).GameEntity;
						MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
						((Mat3)(ref globalFrame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
						globalFrame.origin.z = ((MissionBehavior)this).Mission.Scene.GetGroundHeightAtPosition(globalFrame.origin, (BodyFlags)544321929);
						Vec3 f = globalFrame.rotation.f;
						((Vec3)(ref f)).Normalize();
						ref Vec3 origin = ref globalFrame.origin;
						origin -= 0.3f * f;
						((Mat3)(ref globalFrame.rotation)).RotateAboutUp(MathF.PI);
						gameEntity = ((ScriptComponentBehavior)townPassageProp).GameEntity;
						bool hasTorch = ((WeakGameEntity)(ref gameEntity)).HasTag("torch");
						Agent obj = SpawnWanderingAgentWithInitialFrame(locationCharacter, globalFrame, ((ScriptComponentBehavior)((UsableMachine)passage).PilotStandingPoint).GameEntity, noHorses: true, hasTorch);
						obj.SetAgentExcludeStateForFaceGroupId(_disabledFaceId, true);
						((MissionBehavior)this).Mission.MakeSound(MiscSoundContainer.SoundCodeMovementFoleyDoorClose, globalFrame.origin, true, false, -1, -1);
						obj.FadeIn();
						flag = true;
						break;
					}
				}
				return;
			}
		}
		SpawnDefaultLocationCharacter(locationCharacter, simulateAgentAfterSpawn: true);
	}

	private void SetUsablePlaceUsed(string spawnTag, GameEntity gameEntity)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		foreach (UsableMachine item in GetAllUsablePointsWithTag(spawnTag))
		{
			if (!_usedSpawnPoints.Contains(item) && ((ScriptComponentBehavior)item).GameEntity == gameEntity)
			{
				_usedSpawnPoints.Add(item);
			}
		}
	}

	private bool GetInitialFrameForSpawnTag(string spawnTag, ref WeakGameEntity spawnedOnGameEntity, ref MatrixFrame frame)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		List<UsableMachine> allUsablePointsWithTag = GetAllUsablePointsWithTag(spawnTag);
		if (allUsablePointsWithTag.Count > 0)
		{
			foreach (UsableMachine item in allUsablePointsWithTag)
			{
				if (!_usedSpawnPoints.Contains(item) && GetSpawnFrameFromUsableMachine(item, out var frame2))
				{
					frame = frame2;
					spawnedOnGameEntity = ((ScriptComponentBehavior)item).GameEntity;
					_usedSpawnPoints.Add(item);
					return true;
				}
			}
		}
		return false;
	}

	public bool HasUsablePointWithTag(string tag)
	{
		if (!_usablePoints.ContainsKey(tag))
		{
			return _pairedUsablePoints.ContainsKey(tag);
		}
		return true;
	}

	public IEnumerable<string> GetAllSpawnTags()
	{
		return _usablePoints.Keys.ToList().Concat(_pairedUsablePoints.Keys.ToList());
	}

	public List<UsableMachine> GetAllUsablePointsWithTag(string tag)
	{
		List<UsableMachine> list = new List<UsableMachine>();
		List<UsableMachine> value = new List<UsableMachine>();
		if (_usablePoints.TryGetValue(tag, out value))
		{
			list.AddRange(value);
		}
		List<UsableMachine> value2 = new List<UsableMachine>();
		if (_pairedUsablePoints.TryGetValue(tag, out value2))
		{
			list.AddRange(value2);
		}
		return list;
	}

	public Agent SpawnWanderingAgent(LocationCharacter locationCharacter)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity spawnedOnGameEntity = WeakGameEntity.Invalid;
		bool flag = false;
		MatrixFrame frame = MatrixFrame.Identity;
		if (locationCharacter.SpecialTargetTag != null)
		{
			flag = GetInitialFrameForSpawnTag(locationCharacter.SpecialTargetTag, ref spawnedOnGameEntity, ref frame);
		}
		if (!locationCharacter.ForceSpawnInSpecialTargetTag)
		{
			if (!flag)
			{
				flag = GetInitialFrameForSpawnTag("npc_common_limited", ref spawnedOnGameEntity, ref frame);
			}
			if (!flag)
			{
				flag = GetInitialFrameForSpawnTag("npc_common", ref spawnedOnGameEntity, ref frame);
			}
			if (!flag && _usablePoints.Count > 0)
			{
				foreach (KeyValuePair<string, List<UsableMachine>> usablePoint in _usablePoints)
				{
					if (usablePoint.Value.Count <= 0)
					{
						continue;
					}
					foreach (UsableMachine item in usablePoint.Value)
					{
						if (GetSpawnFrameFromUsableMachine(item, out var frame2))
						{
							frame = frame2;
							flag = true;
							spawnedOnGameEntity = ((ScriptComponentBehavior)item).GameEntity;
							break;
						}
					}
				}
			}
			if (!flag && _pairedUsablePoints.Count > 0)
			{
				foreach (KeyValuePair<string, List<UsableMachine>> pairedUsablePoint in _pairedUsablePoints)
				{
					if (pairedUsablePoint.Value.Count <= 0)
					{
						continue;
					}
					foreach (UsableMachine item2 in pairedUsablePoint.Value)
					{
						if (GetSpawnFrameFromUsableMachine(item2, out var frame3))
						{
							frame = frame3;
							flag = true;
							spawnedOnGameEntity = ((ScriptComponentBehavior)item2).GameEntity;
							break;
						}
					}
				}
			}
		}
		if (flag)
		{
			frame.rotation.f.z = 0f;
			((Vec3)(ref frame.rotation.f)).Normalize();
			frame.rotation.u = Vec3.Up;
			frame.rotation.s = Vec3.CrossProduct(frame.rotation.f, frame.rotation.u);
			((Mat3)(ref frame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
			bool hasTorch = ((WeakGameEntity)(ref spawnedOnGameEntity)).HasTag("torch") && !Campaign.Current.IsDay;
			Agent obj = SpawnWanderingAgentWithInitialFrame(locationCharacter, frame, spawnedOnGameEntity, noHorses: true, hasTorch);
			obj.SetAgentExcludeStateForFaceGroupId(_disabledFaceId, true);
			return obj;
		}
		return null;
	}

	private bool GetSpawnFrameFromUsableMachine(UsableMachine usableMachine, out MatrixFrame frame)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		frame = MatrixFrame.Identity;
		StandingPoint randomElementWithPredicate = Extensions.GetRandomElementWithPredicate<StandingPoint>(usableMachine.StandingPoints, (Func<StandingPoint, bool>)((StandingPoint x) => !((UsableMissionObject)x).HasUser && !((UsableMissionObject)x).IsDeactivated && !((MissionObject)x).IsDisabled));
		if (randomElementWithPredicate != null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)randomElementWithPredicate).GameEntity;
			frame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			return true;
		}
		return false;
	}

	public void SpawnWanderingAgentWithDelay(LocationCharacter locationCharacter, MatrixFrame matrixFrame, GameEntity spawnEntity, bool noHorses = true, bool hasTorch = false, float delay = 3f)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (delay > 0f)
		{
			_spawnTimers.Add((locationCharacter, matrixFrame, spawnEntity, noHorses, hasTorch, new Timer(((MissionBehavior)this).Mission.CurrentTime, delay, false)));
		}
		else
		{
			Debug.FailedAssert("delay > 0", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\MissionAgentHandler.cs", "SpawnWanderingAgentWithDelay", 1035);
		}
	}

	public Agent SpawnWanderingAgentWithInitialFrame(LocationCharacter locationCharacter, MatrixFrame spawnPointFrame, WeakGameEntity spawnEntity, bool noHorses = true, bool hasTorch = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Invalid comparison between Unknown and I4
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		Team val = Team.Invalid;
		CharacterRelations characterRelation = locationCharacter.CharacterRelation;
		switch ((int)characterRelation)
		{
		case 0:
			val = Team.Invalid;
			break;
		case 1:
			val = ((MissionBehavior)this).Mission.PlayerAllyTeam;
			break;
		case 2:
			val = ((MissionBehavior)this).Mission.PlayerEnemyTeam;
			break;
		}
		spawnPointFrame.origin.z = ((MissionBehavior)this).Mission.Scene.GetGroundHeightAtPosition(spawnPointFrame.origin, (BodyFlags)544321929);
		(uint, uint) agentSettlementColors = GetAgentSettlementColors(locationCharacter);
		AgentBuildData obj = locationCharacter.GetAgentBuildData().Team(val).InitialPosition(ref spawnPointFrame.origin);
		Vec2 val2 = ((Vec3)(ref spawnPointFrame.rotation.f)).AsVec2;
		val2 = ((Vec2)(ref val2)).Normalized();
		AgentBuildData obj2 = obj.InitialDirection(ref val2).ClothingColor1(agentSettlementColors.Item1).ClothingColor2(agentSettlementColors.Item2)
			.CivilianEquipment(locationCharacter.UseCivilianEquipment)
			.NoHorses(noHorses);
		CharacterObject character = locationCharacter.Character;
		object obj3;
		if (character == null)
		{
			obj3 = null;
		}
		else
		{
			Hero heroObject = character.HeroObject;
			if (heroObject == null)
			{
				obj3 = null;
			}
			else
			{
				Clan clan = heroObject.Clan;
				obj3 = ((clan != null) ? clan.Banner : null);
			}
		}
		AgentBuildData val3 = obj2.Banner((Banner)obj3);
		if (hasTorch)
		{
			Equipment val4 = ((BasicCharacterObject)locationCharacter.Character).Equipment.Clone(false);
			val4[(EquipmentIndex)4] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>("torch"), (ItemModifier)null, (ItemObject)null, false);
			val3 = val3.Equipment(val4);
		}
		Agent val5 = ((MissionBehavior)this).Mission.SpawnAgent(val3, false);
		val5.SetAgentExcludeStateForFaceGroupId(_disabledFaceId, true);
		if (hasTorch)
		{
			EquipmentIndex val6 = default(EquipmentIndex);
			EquipmentIndex val7 = default(EquipmentIndex);
			bool flag = default(bool);
			val5.SpawnEquipment.GetInitialWeaponIndicesToEquip(ref val6, ref val7, ref flag, (InitialWeaponEquipPreference)0);
			if ((int)val7 != -1)
			{
				val5.TryToWieldWeaponInSlot(val7, (WeaponWieldActionType)2, true);
			}
		}
		AnimationSystemData val8 = MonsterExtensions.FillAnimationSystemData(val3.AgentMonster, MBGlobals.GetActionSet(locationCharacter.ActionSetCode), ((BasicCharacterObject)locationCharacter.Character).GetStepSize(), false);
		val5.SetActionSet(ref val8);
		val5.GetComponent<CampaignAgentComponent>().CreateAgentNavigator(locationCharacter);
		locationCharacter.AddBehaviors.Invoke((IAgent)(object)val5);
		AfterAgentCreatedDelegate afterAgentCreated = locationCharacter.AfterAgentCreated;
		if (afterAgentCreated != null)
		{
			afterAgentCreated.Invoke((IAgent)(object)val5);
		}
		Game.Current.EventManager.TriggerEvent<LocationCharacterAgentSpawnedMissionEvent>(new LocationCharacterAgentSpawnedMissionEvent(locationCharacter, val5, spawnEntity));
		return val5;
	}

	public static uint GetRandomTournamentTeamColor(int teamIndex)
	{
		return _tournamentTeamColors[teamIndex % _tournamentTeamColors.Length];
	}

	public static (uint color1, uint color2) GetAgentSettlementColors(LocationCharacter locationCharacter)
	{
		CharacterObject character = locationCharacter.Character;
		if (((BasicCharacterObject)character).IsHero)
		{
			if (character.HeroObject.Clan == CharacterObject.PlayerCharacter.HeroObject.Clan)
			{
				return (color1: Clan.PlayerClan.MapFaction.Color, color2: Clan.PlayerClan.MapFaction.Color2);
			}
			if (!character.HeroObject.IsNotable)
			{
				return (color1: locationCharacter.AgentData.AgentClothingColor1, color2: locationCharacter.AgentData.AgentClothingColor2);
			}
			MobileParty partyBelongedTo = character.HeroObject.PartyBelongedTo;
			return CharacterHelper.GetDeterministicColorsForCharacter(character, (partyBelongedTo != null) ? partyBelongedTo.Party : null);
		}
		if (((BasicCharacterObject)character).IsSoldier)
		{
			return (color1: Settlement.CurrentSettlement.MapFaction.Color, color2: Settlement.CurrentSettlement.MapFaction.Color2);
		}
		return (color1: _villagerClothColors[MBRandom.RandomInt(_villagerClothColors.Length)], color2: _villagerClothColors[MBRandom.RandomInt(_villagerClothColors.Length)]);
	}

	public UsableMachine FindUnusedPointWithTagForAgent(Agent agent, string tag)
	{
		UsableMachine val = FindUnusedPointForAgent(agent, _pairedUsablePoints, tag);
		if (val == null || ((IEnumerable<StandingPoint>)val.StandingPoints).Any((StandingPoint x) => ((UsableMissionObject)x).HasUser && ((UsableMissionObject)x).UserAgent == agent))
		{
			val = FindUnusedPointForAgent(agent, _usablePoints, tag);
		}
		return val;
	}

	public List<UsableMachine> FindUnusedPoints(string tag)
	{
		if (_usablePoints.TryGetValue(tag, out var value))
		{
			return value;
		}
		return null;
	}

	private UsableMachine FindUnusedPointForAgent(Agent agent, Dictionary<string, List<UsableMachine>> usableMachinesList, string primaryTag)
	{
		if (usableMachinesList.TryGetValue(primaryTag, out var value) && value.Count > 0)
		{
			int num = MBRandom.RandomInt(0, value.Count);
			for (int i = 0; i < value.Count; i++)
			{
				UsableMachine val = value[(num + i) % value.Count];
				if (!((MissionObject)val).IsDisabled && !val.IsDestroyed && val.IsStandingPointAvailableForAgent(agent))
				{
					return val;
				}
			}
		}
		return null;
	}

	public List<UsableMachine> FindAllUnusedPoints(Agent agent, string primaryTag)
	{
		List<UsableMachine> list = new List<UsableMachine>();
		List<UsableMachine> list2 = new List<UsableMachine>();
		_usablePoints.TryGetValue(primaryTag, out var value);
		_pairedUsablePoints.TryGetValue(primaryTag, out var value2);
		value2 = value2?.Distinct().ToList();
		if (value != null && value.Count > 0)
		{
			list.AddRange(value);
		}
		if (value2 != null && value2.Count > 0)
		{
			list.AddRange(value2);
		}
		if (list.Count > 0)
		{
			foreach (UsableMachine item in list)
			{
				if (((List<StandingPoint>)(object)item.StandingPoints).Exists((Predicate<StandingPoint>)((StandingPoint sp) => (((UsableMissionObject)sp).IsInstantUse || (!((UsableMissionObject)sp).HasUser && !((UsableMissionObject)sp).HasAIMovingTo)) && !((UsableMissionObject)sp).IsDisabledForAgent(agent))))
				{
					list2.Add(item);
				}
			}
		}
		return list2;
	}

	public void TeleportTargetAgentNearReferenceAgent(Agent referenceAgent, Agent teleportAgent, bool teleportFollowers, bool teleportOpposite)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Invalid comparison between Unknown and I4
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		Vec3 position = referenceAgent.Position;
		Vec3 lookDirection = referenceAgent.LookDirection;
		Vec3 val = position + ((Vec3)(ref lookDirection)).NormalizedCopy() * 4f;
		Vec3 val2;
		if (teleportOpposite)
		{
			val2 = val;
			val2.z = ((MissionBehavior)this).Mission.Scene.GetGroundHeightAtPosition(val2, (BodyFlags)544321929);
		}
		else
		{
			val2 = Mission.Current.GetRandomPositionAroundPoint(referenceAgent.Position, 2f, 4f, true);
			val2.z = ((MissionBehavior)this).Mission.Scene.GetGroundHeightAtPosition(val2, (BodyFlags)544321929);
		}
		WorldFrame val3 = default(WorldFrame);
		((WorldFrame)(ref val3))._002Ector(referenceAgent.Frame.rotation, new WorldPosition(((MissionBehavior)this).Mission.Scene, referenceAgent.Frame.origin));
		Vec3 val4 = default(Vec3);
		((Vec3)(ref val4))._002Ector(((WorldPosition)(ref val3.Origin)).AsVec2 - ((Vec3)(ref val2)).AsVec2, 0f, -1f);
		teleportAgent.LookDirection = ((Vec3)(ref val4)).NormalizedCopy();
		teleportAgent.TeleportToPosition(val2);
		if (!teleportFollowers || (int)teleportAgent.Controller != 2)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			LocationCharacter locationCharacter = CampaignMission.Current.Location.GetLocationCharacter(item.Origin);
			AccompanyingCharacter accompanyingCharacter = PlayerEncounter.LocationEncounter.GetAccompanyingCharacter(locationCharacter);
			if (item.GetComponent<CampaignAgentComponent>().AgentNavigator != null && accompanyingCharacter != null && accompanyingCharacter.IsFollowingPlayerAtMissionStart)
			{
				GetFrameForFollowingAgent(teleportAgent, out var frame);
				item.TeleportToPosition(frame.origin);
			}
		}
	}

	public static int GetPointCountOfUsableMachine(UsableMachine usableMachine, bool checkForUnusedOnes)
	{
		int num = 0;
		List<AnimationPoint> list = new List<AnimationPoint>();
		foreach (StandingPoint item in (List<StandingPoint>)(object)usableMachine.StandingPoints)
		{
			if (((UsableMissionObject)item).IsDeactivated || ((MissionObject)item).IsDisabled || ((UsableMissionObject)item).IsInstantUse || (checkForUnusedOnes && (((UsableMissionObject)item).HasUser || ((UsableMissionObject)item).HasAIMovingTo)))
			{
				continue;
			}
			if (item is AnimationPoint { IsActive: not false } animationPoint)
			{
				List<AnimationPoint> alternatives = animationPoint.GetAlternatives();
				if (alternatives.Count == 0)
				{
					num++;
				}
				else if (!list.Contains(animationPoint) && (!checkForUnusedOnes || !alternatives.Any((AnimationPoint x) => ((UsableMissionObject)x).HasUser && ((UsableMissionObject)x).HasAIMovingTo)))
				{
					list.AddRange(alternatives);
					num++;
				}
			}
			else
			{
				num++;
			}
		}
		return num;
	}
}
