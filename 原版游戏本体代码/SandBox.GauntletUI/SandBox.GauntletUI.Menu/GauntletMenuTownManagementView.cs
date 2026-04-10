using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.View.Map;
using SandBox.View.Menu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Menu;

[OverrideView(typeof(MenuTownManagementView))]
public class GauntletMenuTownManagementView : MenuView
{
	private SpriteCategory _spriteCategory;

	private GauntletLayer _layerAsGauntletLayer;

	private TownManagementVM _dataSource;

	protected override void OnInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		base.OnInitialize();
		_dataSource = new TownManagementVM();
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_town_management");
		base.Layer = (ScreenLayer)new GauntletLayer("MapTownManagement", 206, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		base.MenuViewContext.AddLayer(base.Layer);
		if (!base.Layer.Input.IsCategoryRegistered(HotKeyManager.GetCategory("GenericPanelGameKeyCategory")))
		{
			base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		}
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_layerAsGauntletLayer.LoadMovie("TownManagement", (ViewModel)(object)_dataSource);
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		_dataSource.Show = true;
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInTownManagement(isInTownManagement: true);
		}
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		_spriteCategory.Unload();
		base.MenuViewContext.RemoveLayer(base.Layer);
		base.Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(base.Layer);
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInTownManagement(isInTownManagement: false);
		}
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_layerAsGauntletLayer = null;
		base.Layer = null;
		base.OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (base.Layer.Input.IsHotKeyReleased("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			if (_dataSource.ReserveControl.IsEnabled)
			{
				_dataSource.ReserveControl.ExecuteConfirm();
			}
			else
			{
				_dataSource.ExecuteDone();
			}
		}
		else if (base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			if (_dataSource.IsSelectingGovernor)
			{
				_dataSource.IsSelectingGovernor = false;
			}
			else if (_dataSource.ReserveControl.IsEnabled)
			{
				_dataSource.ReserveControl.ExecuteCancel();
			}
			else
			{
				SettlementBuildingProjectVM val = ((IEnumerable<SettlementBuildingProjectVM>)_dataSource.ProjectSelection.AvailableProjects).FirstOrDefault((Func<SettlementBuildingProjectVM, bool>)((SettlementBuildingProjectVM x) => x.IsSelected));
				if (val != null)
				{
					val.IsSelected = false;
				}
				else
				{
					UISoundsHelper.PlayUISound("event:/ui/default");
					_dataSource.ExecuteDone();
				}
			}
		}
		if (!_dataSource.Show)
		{
			base.MenuViewContext.CloseTownManagement();
		}
	}

	protected override void OnMapConversationActivated()
	{
		base.OnMapConversationActivated();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, true);
		}
	}

	protected override void OnMapConversationDeactivated()
	{
		base.OnMapConversationDeactivated();
		if (_layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_layerAsGauntletLayer, false);
		}
	}
}
