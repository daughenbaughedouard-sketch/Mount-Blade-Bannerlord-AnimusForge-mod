using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerVoiceChatUIHandler))]
public class MissionGauntletVoiceChat : MissionView
{
	private MultiplayerVoiceChatVM _dataSource;

	private GauntletLayer _gauntletLayer;

	public MissionGauntletVoiceChat()
	{
		base.ViewOrderPriority = 60;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenInitialize();
		_dataSource = new MultiplayerVoiceChatVM(((MissionBehavior)this).Mission);
		_gauntletLayer = new GauntletLayer("MultiplayerVoiceChat", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("MultiplayerVoiceChat", (ViewModel)(object)_dataSource);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_gauntletLayer = null;
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		_dataSource.OnTick(dt);
	}
}
