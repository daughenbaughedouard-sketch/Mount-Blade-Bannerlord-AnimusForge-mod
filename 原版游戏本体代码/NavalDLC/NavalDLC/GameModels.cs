using System.Collections.Generic;
using NavalDLC.ComponentInterfaces;
using TaleWorlds.Core;

namespace NavalDLC;

public sealed class GameModels : GameModelsManager
{
	public static GameModels Instance => NavalDLCManager.Instance.GameModels;

	public ShipPhysicsParametersModel ShipPhysicsParametersModel { get; private set; }

	public ClanShipOwnershipModel ClanShipOwnershipModel { get; private set; }

	public ShipDistributionModel ShipDistributionModel { get; set; }

	public ShipDeploymentModel ShipDeploymentModel { get; private set; }

	public MapStormModel MapStormModel { get; private set; }

	public GameModels(IEnumerable<GameModel> inputComponents)
		: base(inputComponents)
	{
		GetDefaultGameModels();
	}

	private void GetDefaultGameModels()
	{
		ShipPhysicsParametersModel = ((GameModelsManager)this).GetGameModel<ShipPhysicsParametersModel>();
		ClanShipOwnershipModel = ((GameModelsManager)this).GetGameModel<ClanShipOwnershipModel>();
		ShipDistributionModel = ((GameModelsManager)this).GetGameModel<ShipDistributionModel>();
		ShipDeploymentModel = ((GameModelsManager)this).GetGameModel<ShipDeploymentModel>();
		MapStormModel = ((GameModelsManager)this).GetGameModel<MapStormModel>();
	}
}
