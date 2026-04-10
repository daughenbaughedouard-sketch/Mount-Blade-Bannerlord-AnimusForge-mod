using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

public abstract class MissionGauntletEscapeMenuBase : MissionEscapeMenuView
{
	protected EscapeMenuVM DataSource;

	private GauntletLayer _gauntletLayer;

	private GauntletMovieIdentifier _movie;

	private string _viewFile;

	private bool _isRenderingStarted;

	private TutorialContexts _escapeMenuPrevTutorialContext;

	protected MissionGauntletEscapeMenuBase(string viewFile)
	{
		base.OnMissionScreenInitialize();
		_viewFile = viewFile;
		ViewOrderPriority = 50;
		Game.Current.EventManager.RegisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
	}

	protected virtual List<EscapeMenuItemVM> GetEscapeMenuItems()
	{
		return null;
	}

	public override void OnMissionScreenFinalize()
	{
		Game.Current.EventManager.UnregisterEvent<TutorialContextChangedEvent>((Action<TutorialContextChangedEvent>)OnTutorialContextChanged);
		((ViewModel)DataSource).OnFinalize();
		DataSource = null;
		_gauntletLayer = null;
		_movie = null;
		base.OnMissionScreenFinalize();
	}

	public override bool OnEscape()
	{
		if (!_isRenderingStarted)
		{
			return false;
		}
		if (!base.IsActive)
		{
			DataSource.RefreshItems((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems());
		}
		return OnEscapeMenuToggled(!base.IsActive);
	}

	protected bool OnEscapeMenuToggled(bool isOpened)
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		if (base.IsActive == isOpened)
		{
			return false;
		}
		base.IsActive = isOpened;
		if (isOpened)
		{
			Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)16));
			((ViewModel)DataSource).RefreshValues();
			if (!GameNetwork.IsMultiplayer)
			{
				MBCommon.PauseGameEngine();
				Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
			}
			_gauntletLayer = new GauntletLayer("MissionEscapeMenu", ViewOrderPriority, false);
			((ScreenLayer)_gauntletLayer).IsFocusLayer = true;
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
			_movie = _gauntletLayer.LoadMovie(_viewFile, (ViewModel)(object)DataSource);
			((ScreenBase)base.MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
			ScreenManager.TrySetFocus((ScreenLayer)(object)_gauntletLayer);
		}
		else
		{
			Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent(_escapeMenuPrevTutorialContext));
			if (!GameNetwork.IsMultiplayer)
			{
				MBCommon.UnPauseGameEngine();
				Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
			}
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			((ScreenBase)base.MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			_movie = null;
			_gauntletLayer = null;
		}
		return true;
	}

	public override void OnMissionScreenTick(float dt)
	{
		base.OnMissionScreenTick(dt);
		if (base.IsActive && (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("ToggleEscapeMenu") || ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit")))
		{
			OnEscapeMenuToggled(isOpened: false);
		}
	}

	public override void OnSceneRenderingStarted()
	{
		base.OnSceneRenderingStarted();
		_isRenderingStarted = true;
	}

	private void OnTutorialContextChanged(TutorialContextChangedEvent obj)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if ((int)obj.NewContext != 16)
		{
			_escapeMenuPrevTutorialContext = obj.NewContext;
		}
	}
}
