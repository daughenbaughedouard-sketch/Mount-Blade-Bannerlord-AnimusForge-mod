using System;
using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class PortScreenWidget : Widget
{
	private float _alphaChangeTimeElapsed;

	private float _initialAlpha = 1f;

	private float _targetAlpha;

	private float _currentAlpha = 1f;

	private bool _isTransitioning;

	private bool _isAnyUpgradeSlotSelected;

	private Widget _upgradesPanel;

	private Widget _slotsPanel;

	private bool _isControllingCamera;

	private float _cameraEnabledAlpha = 0.2f;

	private Widget _topPanel;

	private Widget _bottomPanel;

	private Widget _leftPanel;

	private Widget _rightPanel;

	private PortPieceInspectionWidget _inspectionPanelWidget;

	private PortUpgradesPanelArrowWidget _upgradesPanelArrowWidget;

	public float AlphaChangeDuration { get; set; } = 0.15f;

	[Editor(false)]
	public bool IsAnyUpgradeSlotSelected
	{
		get
		{
			return _isAnyUpgradeSlotSelected;
		}
		set
		{
			if (value != _isAnyUpgradeSlotSelected)
			{
				_isAnyUpgradeSlotSelected = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "IsAnyUpgradeSlotSelected");
			}
		}
	}

	[Editor(false)]
	public Widget UpgradesPanel
	{
		get
		{
			return _upgradesPanel;
		}
		set
		{
			if (value != _upgradesPanel)
			{
				_upgradesPanel = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "UpgradesPanel");
			}
		}
	}

	[Editor(false)]
	public Widget SlotsPanel
	{
		get
		{
			return _slotsPanel;
		}
		set
		{
			if (value != _slotsPanel)
			{
				_slotsPanel = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "SlotsPanel");
			}
		}
	}

	[Editor(false)]
	public bool IsControllingCamera
	{
		get
		{
			return _isControllingCamera;
		}
		set
		{
			if (value != _isControllingCamera)
			{
				_isControllingCamera = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "IsControllingCamera");
				OnCameraControlsEnabledChanged();
			}
		}
	}

	[Editor(false)]
	public float CameraEnabledAlpha
	{
		get
		{
			return _cameraEnabledAlpha;
		}
		set
		{
			if (value != _cameraEnabledAlpha)
			{
				_cameraEnabledAlpha = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "CameraEnabledAlpha");
			}
		}
	}

	[Editor(false)]
	public Widget TopPanel
	{
		get
		{
			return _topPanel;
		}
		set
		{
			if (value != _topPanel)
			{
				_topPanel = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "TopPanel");
			}
		}
	}

	[Editor(false)]
	public Widget BottomPanel
	{
		get
		{
			return _bottomPanel;
		}
		set
		{
			if (value != _bottomPanel)
			{
				_bottomPanel = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "BottomPanel");
			}
		}
	}

	[Editor(false)]
	public Widget LeftPanel
	{
		get
		{
			return _leftPanel;
		}
		set
		{
			if (value != _leftPanel)
			{
				_leftPanel = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "LeftPanel");
			}
		}
	}

	[Editor(false)]
	public Widget RightPanel
	{
		get
		{
			return _rightPanel;
		}
		set
		{
			if (value != _rightPanel)
			{
				_rightPanel = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "RightPanel");
			}
		}
	}

	[Editor(false)]
	public PortPieceInspectionWidget InspectionPanelWidget
	{
		get
		{
			return _inspectionPanelWidget;
		}
		set
		{
			if (value != _inspectionPanelWidget)
			{
				_inspectionPanelWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<PortPieceInspectionWidget>(value, "InspectionPanelWidget");
			}
		}
	}

	[Editor(false)]
	public PortUpgradesPanelArrowWidget UpgradesPanelArrowWidget
	{
		get
		{
			return _upgradesPanelArrowWidget;
		}
		set
		{
			if (value != _upgradesPanelArrowWidget)
			{
				_upgradesPanelArrowWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<PortUpgradesPanelArrowWidget>(value, "UpgradesPanelArrowWidget");
			}
		}
	}

	public PortScreenWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnUpdate(float dt)
	{
		((Widget)this).OnUpdate(dt);
		if (!IsAnyUpgradeSlotSelected)
		{
			return;
		}
		Widget upgradesPanel = UpgradesPanel;
		if (upgradesPanel == null || !upgradesPanel.IsPointInsideMeasuredArea(((Widget)this).EventManager.MousePosition))
		{
			Widget slotsPanel = SlotsPanel;
			if (slotsPanel == null || !slotsPanel.IsPointInsideMeasuredArea(((Widget)this).EventManager.MousePosition))
			{
				HandleClickOutside();
			}
		}
	}

	private void HandleClickOutside()
	{
		InputKey[] clickKeys = ((Widget)this).Context.InputContext.GetClickKeys();
		for (int i = 0; i < clickKeys.Length; i++)
		{
			if (Input.IsKeyPressed(clickKeys[i]))
			{
				((Widget)this).EventFired("DeselectSlot", Array.Empty<object>());
				break;
			}
		}
	}

	protected override void OnLateUpdate(float dt)
	{
		if (_isTransitioning)
		{
			if (_alphaChangeTimeElapsed < AlphaChangeDuration)
			{
				_currentAlpha = MathF.Lerp(_initialAlpha, _targetAlpha, _alphaChangeTimeElapsed / AlphaChangeDuration, 1E-05f);
				Widget topPanel = TopPanel;
				if (topPanel != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(topPanel, _currentAlpha);
				}
				Widget bottomPanel = BottomPanel;
				if (bottomPanel != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(bottomPanel, _currentAlpha);
				}
				Widget leftPanel = LeftPanel;
				if (leftPanel != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(leftPanel, _currentAlpha);
				}
				Widget rightPanel = RightPanel;
				if (rightPanel != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(rightPanel, _currentAlpha);
				}
				_alphaChangeTimeElapsed += dt;
			}
			else
			{
				_currentAlpha = _targetAlpha;
				Widget topPanel2 = TopPanel;
				if (topPanel2 != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(topPanel2, _currentAlpha);
				}
				Widget bottomPanel2 = BottomPanel;
				if (bottomPanel2 != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(bottomPanel2, _currentAlpha);
				}
				Widget leftPanel2 = LeftPanel;
				if (leftPanel2 != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(leftPanel2, _currentAlpha);
				}
				Widget rightPanel2 = RightPanel;
				if (rightPanel2 != null)
				{
					GauntletExtensions.SetGlobalAlphaRecursively(rightPanel2, _currentAlpha);
				}
				_isTransitioning = false;
			}
		}
		if (InspectionPanelWidget != null)
		{
			UpdateInspectionPanelWidget();
		}
		if (UpgradesPanel != null && UpgradesPanelArrowWidget != null)
		{
			UpdateUpgradesPanelArrowWidget();
		}
	}

	private void UpdateInspectionPanelWidget()
	{
		List<Widget> mouseOveredWidgets = ((Widget)this).EventManager.MouseOveredWidgets;
		for (int i = 0; i < mouseOveredWidgets.Count; i++)
		{
			if (mouseOveredWidgets[i] is PortInspectionParentWidget targetPiece)
			{
				InspectionPanelWidget.SetTargetPiece(targetPiece);
				break;
			}
		}
	}

	private void UpdateUpgradesPanelArrowWidget()
	{
		Widget targetSlot = null;
		List<PortInspectionParentWidget> allChildrenOfTypeRecursive = SlotsPanel.GetAllChildrenOfTypeRecursive<PortInspectionParentWidget>((Func<PortInspectionParentWidget, bool>)null);
		for (int i = 0; i < allChildrenOfTypeRecursive.Count; i++)
		{
			PortInspectionParentWidget portInspectionParentWidget = allChildrenOfTypeRecursive[i];
			ButtonWidget val;
			if (((Widget)portInspectionParentWidget).GetFirstInChildrenRecursive((Func<Widget, bool>)((Widget x) => (val = (ButtonWidget)(object)((x is ButtonWidget) ? x : null)) != null && val.IsSelected)) != null)
			{
				targetSlot = (Widget)(object)portInspectionParentWidget;
				break;
			}
		}
		UpgradesPanelArrowWidget.SetTargetSlot(targetSlot);
	}

	private void OnCameraControlsEnabledChanged()
	{
		_alphaChangeTimeElapsed = 0f;
		_targetAlpha = (IsControllingCamera ? CameraEnabledAlpha : 1f);
		_initialAlpha = _currentAlpha;
		_isTransitioning = true;
	}
}
