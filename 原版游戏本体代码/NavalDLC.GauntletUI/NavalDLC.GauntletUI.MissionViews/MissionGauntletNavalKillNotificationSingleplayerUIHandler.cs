using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace NavalDLC.GauntletUI.MissionViews;

[OverrideView(typeof(MissionSingleplayerKillNotificationUIHandler))]
internal class MissionGauntletNavalKillNotificationSingleplayerUIHandler : MissionGauntletKillNotificationSingleplayerUIHandler
{
	private NavalShipsLogic _navalShipsLogic;

	public override void OnMissionScreenInitialize()
	{
		((MissionGauntletKillNotificationSingleplayerUIHandler)this).OnMissionScreenInitialize();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		if (_navalShipsLogic != null)
		{
			_navalShipsLogic.ShipRammingEvent += OnShipRamming;
		}
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionGauntletKillNotificationSingleplayerUIHandler)this).OnMissionScreenFinalize();
		if (_navalShipsLogic != null)
		{
			_navalShipsLogic.ShipRammingEvent -= OnShipRamming;
		}
	}

	private void OnShipRamming(MissionShip rammingShip, MissionShip rammedShip, float damagePercent, bool isFirstImpact, CapsuleData capsuleData, int ramQuality)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		if (base._isPersonalFeedEnabled && base._dataSource != null && isFirstImpact && damagePercent > 0f && rammingShip != null && rammedShip != null && rammingShip.IsPlayerShip && rammingShip.CanDealDamage(rammedShip))
		{
			string text;
			switch (ramQuality)
			{
			case 1:
				text = ((object)new TextObject("{=P49bHPbv}Ineffective Ram!", (Dictionary<string, object>)null)).ToString();
				break;
			case 2:
				text = ((object)new TextObject("{=SdAhadD3}Weak Ram!", (Dictionary<string, object>)null)).ToString();
				break;
			case 3:
				text = ((object)new TextObject("{=CbaYmAuR}Average Ram!", (Dictionary<string, object>)null)).ToString();
				break;
			case 4:
				text = ((object)new TextObject("{=GaCMFRjH}Good Ram!", (Dictionary<string, object>)null)).ToString();
				break;
			case 5:
				text = ((object)new TextObject("{=DKukCkai}Excellent Ram!", (Dictionary<string, object>)null)).ToString();
				break;
			default:
				Debug.FailedAssert("Ram quality is out of bounds!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.GauntletUI\\MissionViews\\MissionGauntletNavalKillNotificationSingleplayerUIHandler.cs", "OnShipRamming", 70);
				text = ((object)new TextObject("{=CbaYmAuR}Average Ram!", (Dictionary<string, object>)null)).ToString();
				break;
			}
			base._dataSource.OnPersonalMessage(text);
		}
	}
}
