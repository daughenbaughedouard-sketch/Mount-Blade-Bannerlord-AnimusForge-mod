using TaleWorlds.DotNet;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionSpectatorControlView))]
public class MissionGauntletSpectatorControl : MissionView
{
	private GauntletLayer _gauntletLayer;

	private MissionSpectatorControlVM _dataSource;

	public override void EarlyStart()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		((MissionBehavior)this).EarlyStart();
		ViewOrderPriority = 14;
		_dataSource = new MissionSpectatorControlVM(((MissionBehavior)this).Mission);
		_dataSource.SetPrevCharacterInputKey(HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey(10));
		_dataSource.SetNextCharacterInputKey(HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey(9));
		_dataSource.SetTakeControlInputKey(HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey(16));
		_gauntletLayer = new GauntletLayer("MissionSpectatorControl", ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("SpectatorControl", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		base.MissionScreen.OnSpectateAgentFocusIn += _dataSource.OnSpectatedAgentFocusIn;
		base.MissionScreen.OnSpectateAgentFocusOut += _dataSource.OnSpectatedAgentFocusOut;
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		((MissionBehavior)this).OnMissionTick(dt);
		if (_dataSource == null)
		{
			return;
		}
		SpectatorData spectatingData = base.MissionScreen.GetSpectatingData(base.MissionScreen.CombatCamera.Frame.origin);
		bool flag = (int)((SpectatorData)(ref spectatingData)).CameraType == 1 || (int)((SpectatorData)(ref spectatingData)).CameraType == 7;
		MissionSpectatorControlVM dataSource = _dataSource;
		int isEnabled;
		if ((!flag && (int)((MissionBehavior)this).Mission.Mode != 6) || (base.MissionScreen.IsCheatGhostMode && !((MissionBehavior)this).Mission.IsOrderMenuOpen))
		{
			MissionMultiplayerGameModeBaseClient missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
			if ((missionBehavior == null || missionBehavior.IsRoundInProgress) && !base.MissionScreen.LockCameraMovement)
			{
				isEnabled = (((NativeObject)(object)base.MissionScreen.CustomCamera == (NativeObject)null) ? 1 : 0);
				goto IL_00b6;
			}
		}
		isEnabled = 0;
		goto IL_00b6;
		IL_00b6:
		dataSource.IsEnabled = (byte)isEnabled != 0;
		bool flag2 = ((MissionBehavior)this).Mission.PlayerTeam != null && ((MissionBehavior)this).Mission.MainAgent == null;
		_dataSource.SetMainAgentStatus(flag2);
		_dataSource.IsTakeControlRelevant = flag2 && ((MissionBehavior)this).Mission.CanPlayerTakeControlOfAnotherAgentWhenDead;
		_dataSource.IsTakeControlEnabled = base.MissionScreen.LastFollowedAgent != null && ((MissionBehavior)this).Mission.CanTakeControlOfAgent(base.MissionScreen.LastFollowedAgent);
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		base.MissionScreen.OnSpectateAgentFocusIn -= _dataSource.OnSpectatedAgentFocusIn;
		base.MissionScreen.OnSpectateAgentFocusOut -= _dataSource.OnSpectatedAgentFocusOut;
		((ViewModel)_dataSource).OnFinalize();
	}
}
