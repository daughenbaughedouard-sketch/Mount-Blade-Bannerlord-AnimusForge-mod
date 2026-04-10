using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[OverrideView(typeof(MissionBoundaryCrossingView))]
public class MissionGauntletBoundaryCrossingView : MissionBattleUIBaseView
{
	private GauntletLayer _gauntletLayer;

	private BoundaryCrossingVM _dataSource;

	protected override void OnCreateView()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		_dataSource = new BoundaryCrossingVM(((MissionBehavior)this).Mission, (Action<bool>)OnEscapeMenuToggled);
		_gauntletLayer = new GauntletLayer("BoundaryCrossing", 47, false);
		_gauntletLayer.LoadMovie("BoundaryCrossing", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
	}

	protected override void OnDestroyView()
	{
		_gauntletLayer = null;
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
	}

	protected override void OnSuspendView()
	{
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		}
	}

	protected override void OnResumeView()
	{
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
		}
	}

	private void OnEscapeMenuToggled(bool isOpened)
	{
		if (base.IsViewCreated)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, !isOpened);
		}
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		if (base.IsViewCreated)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		if (base.IsViewCreated)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}
}
