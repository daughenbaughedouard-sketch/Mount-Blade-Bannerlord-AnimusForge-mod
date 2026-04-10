using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace TaleWorlds.MountAndBlade.View;

public class SimpleSceneTestWithMission
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static InitializeMissionBehaviorsDelegate _003C_003E9__5_0;

		internal IEnumerable<MissionBehavior> _003COpenSceneWithMission_003Eb__5_0(Mission missionController)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Expected O, but got Unknown
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			return (IEnumerable<MissionBehavior>)(object)new MissionBehavior[6]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new BasicLeaveMissionLogic(false, 0),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
		}
	}

	private Mission _mission;

	private string _sceneName;

	private DecalAtlasGroup _customDecalGroup;

	public SimpleSceneTestWithMission(string sceneName, DecalAtlasGroup atlasGroup = (DecalAtlasGroup)0)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		_sceneName = sceneName;
		_customDecalGroup = atlasGroup;
		_mission = OpenSceneWithMission(_sceneName);
	}

	public bool LoadingFinished()
	{
		if (_mission.IsLoadingFinished)
		{
			return Utilities.GetNumberOfShaderCompilationsInProgress() == 0;
		}
		return false;
	}

	private Mission OpenSceneWithMission(string scene, string sceneLevels = "")
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected I4, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		LoadingWindow.DisableGlobalLoadingWindow();
		MissionInitializerRecord val = default(MissionInitializerRecord);
		((MissionInitializerRecord)(ref val))._002Ector(scene);
		val.PlayingInCampaignMode = false;
		val.AtmosphereOnCampaign = AtmosphereInfo.GetInvalidAtmosphereInfo();
		val.DecalAtlasGroup = (int)_customDecalGroup;
		val.SceneLevels = sceneLevels;
		MissionInitializerRecord val2 = val;
		object obj = _003C_003Ec._003C_003E9__5_0;
		if (obj == null)
		{
			InitializeMissionBehaviorsDelegate val3 = (Mission missionController) => (IEnumerable<MissionBehavior>)(object)new MissionBehavior[6]
			{
				(MissionBehavior)new MissionOptionsComponent(),
				(MissionBehavior)new BasicLeaveMissionLogic(false, 0),
				(MissionBehavior)new MissionHardBorderPlacer(),
				(MissionBehavior)new MissionBoundaryPlacer(),
				(MissionBehavior)new MissionBoundaryCrossingHandler(10f),
				(MissionBehavior)new EquipmentControllerLeaveLogic()
			};
			_003C_003Ec._003C_003E9__5_0 = val3;
			obj = (object)val3;
		}
		return MissionState.OpenNew("SimpleSceneTestWithMission", val2, (InitializeMissionBehaviorsDelegate)obj, true, true);
	}
}
