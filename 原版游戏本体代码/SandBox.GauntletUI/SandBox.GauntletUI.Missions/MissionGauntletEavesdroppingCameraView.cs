using SandBox.View.Missions;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(EavesdroppingMissionCameraView))]
public class MissionGauntletEavesdroppingCameraView : EavesdroppingMissionCameraView
{
	private class EavesdroppingGauntletLayer : GauntletLayer
	{
		public EavesdroppingGauntletLayer(int localOrder, bool shouldClear = false)
			: base("MissionEavesdropping", localOrder, shouldClear)
		{
		}

		public override bool HitTest()
		{
			return true;
		}
	}

	private EavesdroppingGauntletLayer _gauntletLayer;

	public MissionGauntletEavesdroppingCameraView()
	{
		_gauntletLayer = new EavesdroppingGauntletLayer(10);
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
	}

	protected override void SetPlayerMovementEnabled(bool isPlayerMovementEnabled)
	{
		base.SetPlayerMovementEnabled(isPlayerMovementEnabled);
		for (int i = 0; i < ((MissionBehavior)this).Mission.MissionBehaviors.Count; i++)
		{
			MissionBehavior obj = ((MissionBehavior)this).Mission.MissionBehaviors[i];
			MissionBattleUIBaseView val;
			if ((val = (MissionBattleUIBaseView)(object)((obj is MissionBattleUIBaseView) ? obj : null)) != null)
			{
				if (!isPlayerMovementEnabled)
				{
					((MissionView)val).SuspendView();
				}
				else
				{
					((MissionView)val).ResumeView();
				}
			}
		}
		if (isPlayerMovementEnabled)
		{
			((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
			ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		}
		else
		{
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		}
	}
}
