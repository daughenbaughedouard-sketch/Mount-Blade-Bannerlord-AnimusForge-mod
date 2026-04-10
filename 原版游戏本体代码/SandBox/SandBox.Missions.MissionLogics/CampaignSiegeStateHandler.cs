using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class CampaignSiegeStateHandler : MissionLogic
{
	private readonly MapEvent _mapEvent;

	private bool _isRetreat;

	private bool _defenderVictory;

	public bool IsSiege => _mapEvent.IsSiegeAssault;

	public bool IsSallyOut => _mapEvent.IsSallyOut;

	public Settlement Settlement => _mapEvent.MapEventSettlement;

	public CampaignSiegeStateHandler()
	{
		_mapEvent = PlayerEncounter.Battle;
	}

	public override void OnRetreatMission()
	{
		_isRetreat = true;
	}

	public override void OnMissionResultReady(MissionResult missionResult)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		_defenderVictory = (int)missionResult.BattleState == 1;
	}

	public override void OnSurrenderMission()
	{
		PlayerEncounter.PlayerSurrender = true;
	}

	protected override void OnEndMission()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		if (IsSiege && (int)_mapEvent.PlayerSide == 1 && !_isRetreat && !_defenderVictory)
		{
			Settlement.SetNextSiegeState();
		}
	}
}
