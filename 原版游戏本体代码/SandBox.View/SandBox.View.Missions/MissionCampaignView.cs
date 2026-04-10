using System.Collections.Generic;
using SandBox.BoardGames.MissionLogics;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Missions;

public class MissionCampaignView : MissionView
{
	private MapScreen _mapScreen;

	private MissionMainAgentController _missionMainAgentController;

	public override void OnMissionScreenPreLoad()
	{
		_mapScreen = MapScreen.Instance;
		if (_mapScreen != null && ((MissionBehavior)this).Mission.NeedsMemoryCleanup && ScreenManager.ScreenTypeExistsAtList((ScreenBase)(object)_mapScreen))
		{
			_mapScreen.ClearGPUMemory();
			Utilities.ClearShaderMemory();
		}
	}

	public override void OnMissionScreenFinalize()
	{
		if (_mapScreen?.BannerTexturedMaterialCache != null)
		{
			_mapScreen.BannerTexturedMaterialCache.Clear();
		}
	}

	[CommandLineArgumentFunction("get_face_and_helmet_info_of_followed_agent", "mission")]
	public static string GetFaceAndHelmetInfoOfFollowedAgent(List<string> strings)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		ScreenBase topScreen = ScreenManager.TopScreen;
		MissionScreen val = (MissionScreen)(object)((topScreen is MissionScreen) ? topScreen : null);
		if (val == null)
		{
			return "Only works at missions";
		}
		Agent lastFollowedAgent = val.LastFollowedAgent;
		if (lastFollowedAgent == null)
		{
			return "An agent needs to be focussed.";
		}
		string text = "";
		text += ((object)lastFollowedAgent.BodyPropertiesValue/*cast due to .constrained prefix*/).ToString();
		EquipmentElement equipmentFromSlot = lastFollowedAgent.SpawnEquipment.GetEquipmentFromSlot((EquipmentIndex)5);
		if (!((EquipmentElement)(ref equipmentFromSlot)).IsEmpty)
		{
			text = text + "\n Armor Name: " + ((object)((EquipmentElement)(ref equipmentFromSlot)).Item.Name).ToString();
			text = text + "\n Mesh Name: " + ((EquipmentElement)(ref equipmentFromSlot)).Item.MultiMeshName;
		}
		if (lastFollowedAgent.Character != null)
		{
			BasicCharacterObject character = lastFollowedAgent.Character;
			CharacterObject val2 = (CharacterObject)(object)((character is CharacterObject) ? character : null);
			if (val2 != null)
			{
				text = text + "\n Troop Id: " + ((MBObjectBase)val2).StringId;
			}
		}
		Input.SetClipboardText(text);
		return "Copied to clipboard:\n" + text;
	}

	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		_missionMainAgentController = Mission.Current.GetMissionBehavior<MissionMainAgentController>();
		MissionBoardGameLogic missionBehavior = Mission.Current.GetMissionBehavior<MissionBoardGameLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.GameStarted += _missionMainAgentController.Disable;
			missionBehavior.GameEnded += _missionMainAgentController.Enable;
		}
	}
}
