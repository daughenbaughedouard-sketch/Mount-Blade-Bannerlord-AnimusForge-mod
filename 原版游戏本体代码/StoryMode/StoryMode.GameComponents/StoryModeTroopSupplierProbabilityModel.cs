using System.Collections.Generic;
using System.Linq;
using StoryMode.StoryModeObjects;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace StoryMode.GameComponents;

public class StoryModeTroopSupplierProbabilityModel : TroopSupplierProbabilityModel
{
	public override void EnqueueTroopSpawnProbabilitiesAccordingToUnitSpawnPrioritization(MapEventParty battleParty, FlattenedTroopRoster priorityTroops, bool includePlayers, int sizeOfSide, bool forcePriorityTroops, List<(FlattenedTroopRosterElement, MapEventParty, float)> priorityList)
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		int count = priorityList.Count;
		((MBGameModel<TroopSupplierProbabilityModel>)this).BaseModel.EnqueueTroopSpawnProbabilitiesAccordingToUnitSpawnPrioritization(battleParty, priorityTroops, includePlayers, sizeOfSide, forcePriorityTroops, priorityList);
		Settlement currentSettlement = Settlement.CurrentSettlement;
		if (currentSettlement == null || !currentSettlement.IsHideout || priorityTroops == null)
		{
			return;
		}
		if (!StoryModeManager.Current.MainStoryLine.TutorialPhase.IsCompleted)
		{
			for (int i = count; i < priorityList.Count; i++)
			{
				(FlattenedTroopRosterElement, MapEventParty, float) tuple = priorityList[i];
				CharacterObject character = ((FlattenedTroopRosterElement)(ref tuple.Item1)).Troop;
				if (character == StoryModeHeroes.Radagos.CharacterObject && ((IEnumerable<FlattenedTroopRosterElement>)priorityTroops).All((FlattenedTroopRosterElement t) => ((FlattenedTroopRosterElement)(ref t)).Troop != character))
				{
					priorityList[i] = (priorityList[i].Item1, priorityList[i].Item2, 0.01f);
					break;
				}
			}
			return;
		}
		for (int num = 0; num < priorityList.Count; num++)
		{
			(FlattenedTroopRosterElement, MapEventParty, float) tuple2 = priorityList[num];
			CharacterObject character2 = ((FlattenedTroopRosterElement)(ref tuple2.Item1)).Troop;
			if (character2 == StoryModeHeroes.RadagosHenchman.CharacterObject && ((IEnumerable<FlattenedTroopRosterElement>)priorityTroops).All((FlattenedTroopRosterElement t) => ((FlattenedTroopRosterElement)(ref t)).Troop != character2))
			{
				priorityList[num] = (priorityList[num].Item1, priorityList[num].Item2, 0.01f);
				break;
			}
		}
	}
}
