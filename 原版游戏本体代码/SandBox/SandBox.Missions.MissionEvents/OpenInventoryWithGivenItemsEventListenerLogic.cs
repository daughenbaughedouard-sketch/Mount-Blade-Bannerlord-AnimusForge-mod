using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.Objects.Usables;
using TaleWorlds.ObjectSystem;

namespace SandBox.Missions.MissionEvents;

public class OpenInventoryWithGivenItemsEventListenerLogic : MissionLogic
{
	private const string OpenInventoryWithGivenItemsEventId = "open_inventory_with_given_items";

	private readonly Dictionary<string, ItemRoster> _openedInventoryItemRosters = new Dictionary<string, ItemRoster>();

	public OpenInventoryWithGivenItemsEventListenerLogic()
	{
		Game.Current.EventManager.RegisterEvent<GenericMissionEvent>((Action<GenericMissionEvent>)OnGenericMissionEventTriggered);
	}

	protected override void OnEndMission()
	{
		Game.Current.EventManager.UnregisterEvent<GenericMissionEvent>((Action<GenericMissionEvent>)OnGenericMissionEventTriggered);
	}

	private void OnGenericMissionEventTriggered(GenericMissionEvent missionEvent)
	{
		if (missionEvent.EventId == "open_inventory_with_given_items")
		{
			OpenInventoryWithGivenEquipment(missionEvent.Parameter);
		}
	}

	private void OpenInventoryWithGivenEquipment(string parameters)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		string[] array = parameters.Split(new char[1] { ' ' });
		string text = array[0];
		if (!_openedInventoryItemRosters.ContainsKey(text))
		{
			_openedInventoryItemRosters.Add(text, new ItemRoster());
			string[] array2 = new string[array.Length - 2];
			Array.Copy(array, 2, array2, 0, array2.Length);
			InitializeEventItemRoster(array2, _openedInventoryItemRosters[text]);
		}
		EventTriggeringUsableMachine firstScriptOfType = Mission.Current.Scene.FindEntityWithTag(text).GetFirstScriptOfType<EventTriggeringUsableMachine>();
		for (int i = 0; i < ((List<StandingPoint>)(object)((UsableMachine)firstScriptOfType).StandingPoints).Count; i++)
		{
			if (((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)firstScriptOfType).StandingPoints)[i]).HasUser)
			{
				((UsableMissionObject)((List<StandingPoint>)(object)((UsableMachine)firstScriptOfType).StandingPoints)[i]).UserAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
		}
		TextObject descriptionText = firstScriptOfType.DescriptionText;
		string text2 = array[1];
		if (text2.Equals("battle"))
		{
			InventoryScreenHelper.OpenScreenAsReceiveItems(_openedInventoryItemRosters[text], descriptionText, (Action)DoneLogicForBattleEquipmentUpdate);
		}
		else if (text2.Equals("civilian"))
		{
			InventoryScreenHelper.OpenScreenAsReceiveItems(_openedInventoryItemRosters[text], descriptionText, (Action)DoneLogicForCivilianEquipmentUpdate);
		}
		else if (text2.Equals("stealth"))
		{
			InventoryScreenHelper.OpenScreenAsReceiveItems(_openedInventoryItemRosters[text], descriptionText, (Action)DoneLogicForStealthEquipmentUpdate);
		}
		else if (text2.Equals("none"))
		{
			InventoryScreenHelper.OpenScreenAsReceiveItems(_openedInventoryItemRosters[text], descriptionText, (Action)null);
		}
	}

	private void DoneLogicForBattleEquipmentUpdate()
	{
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(Hero.MainHero.BattleEquipment);
	}

	private void DoneLogicForCivilianEquipmentUpdate()
	{
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(Hero.MainHero.CivilianEquipment);
	}

	private void DoneLogicForStealthEquipmentUpdate()
	{
		Agent.Main.UpdateSpawnEquipmentAndRefreshVisuals(Hero.MainHero.StealthEquipment);
	}

	private void InitializeEventItemRoster(string[] itemsWithModifiers, ItemRoster eventItemRoster)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		ItemRosterElement val = default(ItemRosterElement);
		for (int i = 0; i < itemsWithModifiers.Length; i++)
		{
			string[] array = itemsWithModifiers[i].Split(new char[1] { ',' });
			string text = array[0];
			string s = array[1];
			string text2 = ((array.Length > 2) ? array[2] : "");
			((ItemRosterElement)(ref val))._002Ector(MBObjectManager.Instance.GetObject<ItemObject>(text), int.Parse(s), string.IsNullOrEmpty(text2) ? null : MBObjectManager.Instance.GetObject<ItemModifier>(text2));
			eventItemRoster.Add(val);
		}
	}
}
