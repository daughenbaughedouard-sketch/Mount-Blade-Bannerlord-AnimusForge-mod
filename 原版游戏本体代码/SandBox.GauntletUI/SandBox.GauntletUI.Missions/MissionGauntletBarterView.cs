using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Barter;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Missions;

[OverrideView(typeof(BarterView))]
public class MissionGauntletBarterView : MissionView
{
	private GauntletLayer _gauntletLayer;

	private BarterVM _dataSource;

	private BarterManager _barter;

	private SpriteCategory _barterCategory;

	public override void AfterStart()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		_barter = Campaign.Current.BarterManager;
		BarterManager barter = _barter;
		barter.BarterBegin = (BarterBeginEventDelegate)Delegate.Combine((Delegate?)(object)barter.BarterBegin, (Delegate?)new BarterBeginEventDelegate(OnBarterBegin));
		BarterManager barter2 = _barter;
		barter2.Closed = (BarterCloseEventDelegate)Delegate.Combine((Delegate?)(object)barter2.Closed, (Delegate?)new BarterCloseEventDelegate(OnBarterClosed));
	}

	private void OnBarterBegin(BarterData args)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		_barterCategory = UIResourceManager.LoadSpriteCategory("ui_barter");
		_dataSource = new BarterVM(args);
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_gauntletLayer = new GauntletLayer("MissionBarter", base.ViewOrderPriority, false);
		_gauntletLayer.LoadMovie("BarterScreen", (ViewModel)(object)_dataSource);
		GameKeyContext category = HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory");
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(category);
		GameKeyContext category2 = HotKeyManager.GetCategory("GenericPanelGameKeyCategory");
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(category2);
		((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesStateAndDeactivateOthers(new string[2] { "SceneLayer", "MissionBarter" }, true);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		BarterItemVM.IsFiveStackModifierActive = IsDownInGauntletLayer("FiveStackModifier");
		BarterItemVM.IsEntireStackModifierActive = IsDownInGauntletLayer("EntireStackModifier");
		if (IsReleasedInGauntletLayer("Confirm"))
		{
			if (!_dataSource.IsOfferDisabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_dataSource.ExecuteOffer();
			}
		}
		else if (IsReleasedInGauntletLayer("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteCancel();
		}
		else if (IsReleasedInGauntletLayer("Reset"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteReset();
		}
	}

	private bool IsDownInGauntletLayer(string hotKeyID)
	{
		GauntletLayer gauntletLayer = _gauntletLayer;
		if (gauntletLayer == null)
		{
			return false;
		}
		return ((ScreenLayer)gauntletLayer).Input.IsHotKeyDown(hotKeyID);
	}

	private bool IsReleasedInGauntletLayer(string hotKeyID)
	{
		GauntletLayer gauntletLayer = _gauntletLayer;
		if (gauntletLayer == null)
		{
			return false;
		}
		return ((ScreenLayer)gauntletLayer).Input.IsHotKeyReleased(hotKeyID);
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenFinalize();
		BarterManager barter = _barter;
		barter.BarterBegin = (BarterBeginEventDelegate)Delegate.Remove((Delegate?)(object)barter.BarterBegin, (Delegate?)new BarterBeginEventDelegate(OnBarterBegin));
		BarterManager barter2 = _barter;
		barter2.Closed = (BarterCloseEventDelegate)Delegate.Remove((Delegate?)(object)barter2.Closed, (Delegate?)new BarterCloseEventDelegate(OnBarterClosed));
		_gauntletLayer = null;
		BarterVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
	}

	private void OnBarterClosed()
	{
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesState(new string[1] { "MissionBarter" }, false);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesState(new string[1] { "MissionConversation" }, true);
		((ScreenBase)((MissionView)this).MissionScreen).SetLayerCategoriesState(new string[1] { "SceneLayer" }, true);
		BarterItemVM.IsFiveStackModifierActive = false;
		BarterItemVM.IsEntireStackModifierActive = false;
		_barterCategory.Unload();
		((ScreenLayer)_gauntletLayer).IsFocusLayer = false;
		ScreenManager.TryLoseFocus((ScreenLayer)(object)_gauntletLayer);
		((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
		((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
		_gauntletLayer = null;
		_dataSource = null;
	}

	public override void OnPhotoModeActivated()
	{
		((MissionView)this).OnPhotoModeActivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 0f;
		}
	}

	public override void OnPhotoModeDeactivated()
	{
		((MissionView)this).OnPhotoModeDeactivated();
		if (_gauntletLayer != null)
		{
			_gauntletLayer.UIContext.ContextAlpha = 1f;
		}
	}

	public override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return true;
	}
}
