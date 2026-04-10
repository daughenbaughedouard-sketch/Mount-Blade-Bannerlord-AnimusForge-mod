using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map.MarriageOfferPopup;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MarriageOfferPopupView))]
public class GauntletMarriageOfferPopupView : MapView
{
	private GauntletLayer _layerAsGauntletLayer;

	private MarriageOfferPopupVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private CampaignTimeControlMode _previousTimeControlMode;

	private Hero _suitor;

	private Hero _maiden;

	public GauntletMarriageOfferPopupView(Hero suitor, Hero maiden)
	{
		_suitor = suitor;
		_maiden = maiden;
	}

	protected override void CreateLayout()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		base.CreateLayout();
		_dataSource = new MarriageOfferPopupVM(_suitor, _maiden, (Action)OnPopupClosed);
		InitializeKeyVisuals();
		base.Layer = (ScreenLayer)new GauntletLayer("MapMarriageOffer", 203, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		_movie = _layerAsGauntletLayer.LoadMovie("MarriageOfferPopup", (ViewModel)(object)_dataSource);
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		base.MapScreen.SetIsMarriageOfferPopupActive(isMarriageOfferPopupActive: true);
		_previousTimeControlMode = Campaign.Current.TimeControlMode;
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		Campaign.Current.SetTimeControlModeLock(true);
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		HandleInput();
		MarriageOfferPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Update();
		}
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		HandleInput();
		MarriageOfferPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Update();
		}
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		HandleInput();
		MarriageOfferPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.Update();
		}
	}

	protected override void OnFinalize()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
		_movie = null;
		_dataSource = null;
		base.Layer = null;
		_layerAsGauntletLayer = null;
		base.MapScreen.SetIsMarriageOfferPopupActive(isMarriageOfferPopupActive: false);
		Campaign.Current.SetTimeControlModeLock(false);
		Campaign.Current.TimeControlMode = _previousTimeControlMode;
		base.OnFinalize();
	}

	protected override bool IsEscaped()
	{
		MarriageOfferPopupVM dataSource = _dataSource;
		if (dataSource != null)
		{
			dataSource.ExecuteDeclineOffer();
		}
		return true;
	}

	protected override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return false;
	}

	private void OnPopupClosed()
	{
		base.MapScreen.CloseMarriageOfferPopup();
	}

	private void HandleInput()
	{
		if (_dataSource != null)
		{
			if (base.Layer.Input.IsGameKeyPressed(39))
			{
				base.MapScreen.OpenEncyclopedia();
			}
			else if (base.Layer.Input.IsHotKeyReleased("Confirm"))
			{
				UISoundsHelper.PlayUISound("event:/ui/panels/next");
				_dataSource.ExecuteAcceptOffer();
			}
			else if (base.Layer.Input.IsHotKeyReleased("Exit"))
			{
				UISoundsHelper.PlayUISound("event:/ui/panels/next");
				_dataSource.ExecuteDeclineOffer();
			}
		}
	}

	private void InitializeKeyVisuals()
	{
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
	}
}
