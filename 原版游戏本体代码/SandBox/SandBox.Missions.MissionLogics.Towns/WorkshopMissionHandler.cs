using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Objects.AreaMarkers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Missions.MissionLogics.Towns;

public class WorkshopMissionHandler : MissionLogic
{
	private Settlement _settlement;

	private string[] _propKinds = new string[6] { "a", "b", "c", "d", "e", "f" };

	private Dictionary<int, Dictionary<string, List<MatrixFrame>>> _propFrames;

	private List<GameEntity> _listOfCurrentProps;

	private List<WorkshopAreaMarker> _areaMarkers;

	private List<Tuple<Workshop, GameEntity>> _workshopSignEntities;

	public IEnumerable<Tuple<Workshop, GameEntity>> WorkshopSignEntities => _workshopSignEntities.AsEnumerable();

	public WorkshopMissionHandler(Settlement settlement)
	{
		_settlement = settlement;
	}

	public override void OnBehaviorInitialize()
	{
		_workshopSignEntities = new List<Tuple<Workshop, GameEntity>>();
		_listOfCurrentProps = new List<GameEntity>();
		_propFrames = new Dictionary<int, Dictionary<string, List<MatrixFrame>>>();
		_areaMarkers = new List<WorkshopAreaMarker>();
	}

	public override void EarlyStart()
	{
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _settlement.Town.Workshops.Length; i++)
		{
			if (!_settlement.Town.Workshops[i].WorkshopType.IsHidden)
			{
				_propFrames.Add(i, new Dictionary<string, List<MatrixFrame>>());
				string[] propKinds = _propKinds;
				foreach (string key in propKinds)
				{
					_propFrames[i].Add(key, new List<MatrixFrame>());
				}
			}
		}
		List<WorkshopAreaMarker> list = MBExtensions.FindAllWithType<WorkshopAreaMarker>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).ToList();
		_areaMarkers = list.FindAll(delegate(WorkshopAreaMarker x)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)x).GameEntity;
			return ((WeakGameEntity)(ref gameEntity)).HasTag("workshop_area_marker");
		});
		foreach (WorkshopAreaMarker areaMarker in _areaMarkers)
		{
			_ = areaMarker;
		}
		foreach (GameEntity item in ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("shop_prop").ToList())
		{
			WorkshopAreaMarker workshopAreaMarker = FindWorkshop(item);
			if (workshopAreaMarker == null || !_propFrames.ContainsKey(((AreaMarker)workshopAreaMarker).AreaIndex) || (_settlement.Town.Workshops[((AreaMarker)workshopAreaMarker).AreaIndex] != null && _settlement.Town.Workshops[((AreaMarker)workshopAreaMarker).AreaIndex].WorkshopType.IsHidden))
			{
				continue;
			}
			string[] propKinds = _propKinds;
			foreach (string text in propKinds)
			{
				if (item.HasTag(text))
				{
					_propFrames[((AreaMarker)workshopAreaMarker).AreaIndex][text].Add(item.GetGlobalFrame());
					_listOfCurrentProps.Add(item);
					break;
				}
			}
		}
		SetBenches();
	}

	public override void AfterStart()
	{
		InitShopSigns();
	}

	private WorkshopAreaMarker FindWorkshop(GameEntity prop)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		foreach (WorkshopAreaMarker areaMarker in _areaMarkers)
		{
			if (((AreaMarker)areaMarker).IsPositionInRange(prop.GlobalPosition))
			{
				return areaMarker;
			}
		}
		return null;
	}

	private void SetBenches()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		foreach (GameEntity listOfCurrentProp in _listOfCurrentProps)
		{
			listOfCurrentProp.SetVisibilityExcludeParents(false);
			MissionObject firstScriptOfType = listOfCurrentProp.GetFirstScriptOfType<MissionObject>();
			if (firstScriptOfType != null)
			{
				firstScriptOfType.SetDisabled(true);
			}
			foreach (GameEntity child in listOfCurrentProp.GetChildren())
			{
				firstScriptOfType = (MissionObject)(object)child.GetFirstScriptOfType<UsableMachine>();
				if (firstScriptOfType != null)
				{
					firstScriptOfType.SetDisabled(true);
				}
			}
		}
		_listOfCurrentProps.Clear();
		foreach (KeyValuePair<int, Dictionary<string, List<MatrixFrame>>> propFrame in _propFrames)
		{
			int key = propFrame.Key;
			foreach (KeyValuePair<string, List<MatrixFrame>> item in propFrame.Value)
			{
				List<string> prefabNames = GetPrefabNames(key, item.Key);
				if (prefabNames.Count != 0)
				{
					for (int i = 0; i < item.Value.Count; i++)
					{
						MatrixFrame val = item.Value[i];
						_listOfCurrentProps.Add(GameEntity.Instantiate(((MissionBehavior)this).Mission.Scene, prefabNames[i % prefabNames.Count], val, true, ""));
					}
				}
			}
		}
	}

	private void InitShopSigns()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Campaign.Current.GameMode != 1 || _settlement == null || !_settlement.IsTown)
		{
			return;
		}
		List<GameEntity> list = ((MissionBehavior)this).Mission.Scene.FindEntitiesWithTag("shop_sign").ToList();
		foreach (WorkshopAreaMarker item2 in MBExtensions.FindAllWithType<WorkshopAreaMarker>((IEnumerable<MissionObject>)((MissionBehavior)this).Mission.ActiveMissionObjects).ToList())
		{
			Workshop workshop = _settlement.Town.Workshops[((AreaMarker)item2).AreaIndex];
			if (!_workshopSignEntities.All((Tuple<Workshop, GameEntity> x) => x.Item1 != workshop))
			{
				continue;
			}
			for (int num = 0; num < list.Count; num++)
			{
				GameEntity val = list[num];
				if (((AreaMarker)item2).IsPositionInRange(val.GlobalPosition))
				{
					_workshopSignEntities.Add(new Tuple<Workshop, GameEntity>(workshop, val));
					list.RemoveAt(num);
					break;
				}
			}
		}
		foreach (Tuple<Workshop, GameEntity> workshopSignEntity in _workshopSignEntities)
		{
			GameEntity item = workshopSignEntity.Item2;
			WorkshopType workshopType = workshopSignEntity.Item1.WorkshopType;
			item.ClearComponents();
			MetaMesh copy = MetaMesh.GetCopy((workshopType != null) ? workshopType.SignMeshName : "shop_sign_merchantavailable", true, false);
			item.AddMultiMesh(copy, true);
		}
	}

	private List<string> GetPrefabNames(int areaIndex, string propKind)
	{
		List<string> list = new List<string>();
		Workshop val = _settlement.Town.Workshops[areaIndex];
		if (val.WorkshopType != null)
		{
			if (propKind == _propKinds[0])
			{
				list.Add(val.WorkshopType.PropMeshName1);
			}
			else if (propKind == _propKinds[1])
			{
				list.Add(val.WorkshopType.PropMeshName2);
			}
			else if (propKind == _propKinds[2])
			{
				list.AddRange(val.WorkshopType.PropMeshName3List);
			}
			else if (propKind == _propKinds[3])
			{
				list.Add(val.WorkshopType.PropMeshName4);
			}
			else if (propKind == _propKinds[4])
			{
				list.Add(val.WorkshopType.PropMeshName5);
			}
			else if (propKind == _propKinds[5])
			{
				list.Add(val.WorkshopType.PropMeshName6);
			}
		}
		return list;
	}
}
