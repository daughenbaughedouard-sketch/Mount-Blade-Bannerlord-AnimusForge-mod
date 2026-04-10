using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.Settlements.Building;

public class NavalBuildingTypes
{
	private BuildingType _buildingShipyard;

	public static BuildingType SettlementShipyard => Instance._buildingShipyard;

	private static NavalBuildingTypes Instance => NavalDLCManager.Instance.NavalBuildingTypes;

	public NavalBuildingTypes()
	{
		RegisterAll();
		InitializeAll();
	}

	private void RegisterAll()
	{
		_buildingShipyard = Create("building_shipyard");
	}

	private BuildingType Create(string stringId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		return Game.Current.ObjectManager.RegisterPresumedObject<BuildingType>(new BuildingType(stringId));
	}

	private void InitializeAll()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		_buildingShipyard.Initialize(GameTexts.FindText("str_shipyard", (string)null), new TextObject("{=*}Allows ship production. Enables repair, trading, and upgrades of ships.", (Dictionary<string, object>)null), new int[3] { 0, 4800, 6000 }, new Tuple<BuildingEffectEnum, BuildingEffectIncrementType, float, float, float>[2]
		{
			new Tuple<BuildingEffectEnum, BuildingEffectIncrementType, float, float, float>((BuildingEffectEnum)28, (BuildingEffectIncrementType)0, 1f, 2f, 3f),
			new Tuple<BuildingEffectEnum, BuildingEffectIncrementType, float, float, float>((BuildingEffectEnum)29, (BuildingEffectIncrementType)0, 9f, 12f, 15f)
		}, false, 0f, 1);
	}
}
