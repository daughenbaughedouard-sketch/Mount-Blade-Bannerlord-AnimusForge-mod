using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.Admin;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.AdminPanel;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MultiplayerAdminPanelUIHandler))]
public class MissionGauntletAdminPanel : MissionView
{
	public delegate IAdminPanelOptionProvider CreateOptionProviderDelegeate();

	public delegate MultiplayerAdminPanelOptionBaseVM CreateOptionViewModelDelegate(IAdminPanelOption option);

	public delegate MultiplayerAdminPanelOptionBaseVM CreateActionViewModelDelegate(IAdminPanelAction action);

	private GauntletLayer _gauntletLayer;

	private MultiplayerAdminPanelVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private bool _isActive;

	private MultiplayerAdminComponent _multiplayerAdminComponent;

	private MissionLobbyComponent _missionLobbyComponent;

	private readonly MBList<CreateOptionProviderDelegeate> _optionProviderCreators;

	private readonly MBList<CreateOptionViewModelDelegate> _optionViewModelCreators;

	private readonly MBList<CreateActionViewModelDelegate> _actionViewModelCreators;

	public MissionGauntletAdminPanel()
	{
		base.ViewOrderPriority = 45;
		_optionProviderCreators = new MBList<CreateOptionProviderDelegeate>();
		_optionViewModelCreators = new MBList<CreateOptionViewModelDelegate>();
		_actionViewModelCreators = new MBList<CreateActionViewModelDelegate>();
		AddOptionViewModelCreator(CreateDefaultOptionViewModels);
		AddActionViewModelCreator(CreateDefaultActionViewModels);
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		_missionLobbyComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionLobbyComponent>();
		_multiplayerAdminComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MultiplayerAdminComponent>();
		_multiplayerAdminComponent.OnSetAdminMenuActiveState += OnShowAdminPanel;
		AddOptionProviderCreator(CreateDefaultAdminPanelOptionProvider);
	}

	private IAdminPanelOptionProvider CreateDefaultAdminPanelOptionProvider()
	{
		return new DefaultAdminPanelOptionProvider(_multiplayerAdminComponent, _missionLobbyComponent);
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		((MissionView)this).OnMissionScreenTick(dt);
		if (_isActive && (int)((MissionBehavior)this).Mission.CurrentState != 2)
		{
			OnExitAdminPanel();
		}
		if (_isActive)
		{
			_dataSource.OnTick(dt);
		}
	}

	public override void OnMissionScreenFinalize()
	{
		_multiplayerAdminComponent.OnSetAdminMenuActiveState -= OnShowAdminPanel;
		OnEscapeMenuToggled(isOpened: false);
		MultiplayerAdminPanelVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		((List<CreateOptionProviderDelegeate>)(object)_optionProviderCreators).Clear();
		((MissionView)this).OnMissionScreenFinalize();
	}

	public override bool OnEscape()
	{
		if (_isActive)
		{
			OnExitAdminPanel();
			return true;
		}
		return ((MissionView)this).OnEscape();
	}

	public void AddOptionProviderCreator(CreateOptionProviderDelegeate creator)
	{
		if (creator != null)
		{
			((List<CreateOptionProviderDelegeate>)(object)_optionProviderCreators).Add(creator);
		}
	}

	private void OnExitAdminPanel()
	{
		OnEscapeMenuToggled(isOpened: false);
	}

	private void OnShowAdminPanel(bool show)
	{
		OnEscapeMenuToggled(show);
	}

	public void AddOptionViewModelCreator(CreateOptionViewModelDelegate creator)
	{
		((List<CreateOptionViewModelDelegate>)(object)_optionViewModelCreators).Add(creator);
	}

	public void AddActionViewModelCreator(CreateActionViewModelDelegate creator)
	{
		((List<CreateActionViewModelDelegate>)(object)_actionViewModelCreators).Add(creator);
	}

	private MultiplayerAdminPanelOptionBaseVM CreateDefaultOptionViewModels(IAdminPanelOption option)
	{
		if (option is IAdminPanelMultiSelectionOption option2)
		{
			return new MultiplayerAdminPanelMultiSelectionOptionVM(option2);
		}
		if (option is IAdminPanelAction option3)
		{
			return new MultiplayerAdminPanelActionOptionVM(option3);
		}
		if (option is IAdminPanelOption<string> option4)
		{
			return new MultiplayerAdminPanelStringOptionVM(option4);
		}
		if (option is IAdminPanelNumericOption option5)
		{
			return new MultiplayerAdminPanelNumericOptionVM(option5);
		}
		if (option is IAdminPanelOption<bool> option6)
		{
			return new MultiplayerAdminPanelToggleOptionVM(option6);
		}
		return null;
	}

	private MultiplayerAdminPanelOptionBaseVM CreateDefaultActionViewModels(IAdminPanelAction action)
	{
		return new MultiplayerAdminPanelActionOptionVM(action);
	}

	private MultiplayerAdminPanelOptionBaseVM OnCreateOptionViewModel(IAdminPanelOption option)
	{
		for (int num = ((List<CreateOptionViewModelDelegate>)(object)_optionViewModelCreators).Count - 1; num >= 0; num--)
		{
			MultiplayerAdminPanelOptionBaseVM multiplayerAdminPanelOptionBaseVM = ((List<CreateOptionViewModelDelegate>)(object)_optionViewModelCreators)[num]?.Invoke(option);
			if (multiplayerAdminPanelOptionBaseVM != null)
			{
				return multiplayerAdminPanelOptionBaseVM;
			}
		}
		return null;
	}

	private MultiplayerAdminPanelOptionBaseVM OnCreateActionViewModel(IAdminPanelAction action)
	{
		for (int num = ((List<CreateActionViewModelDelegate>)(object)_actionViewModelCreators).Count - 1; num >= 0; num--)
		{
			MultiplayerAdminPanelOptionBaseVM multiplayerAdminPanelOptionBaseVM = ((List<CreateActionViewModelDelegate>)(object)_actionViewModelCreators)[num]?.Invoke(action);
			if (multiplayerAdminPanelOptionBaseVM != null)
			{
				return multiplayerAdminPanelOptionBaseVM;
			}
		}
		return null;
	}

	private void OnEscapeMenuToggled(bool isOpened)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		if (isOpened == _isActive || !((MissionView)this).MissionScreen.SetDisplayDialog(isOpened))
		{
			return;
		}
		_isActive = isOpened;
		if (isOpened)
		{
			if (_dataSource == null)
			{
				MBList<IAdminPanelOptionProvider> val = new MBList<IAdminPanelOptionProvider>();
				for (int i = 0; i < ((List<CreateOptionProviderDelegeate>)(object)_optionProviderCreators).Count; i++)
				{
					IAdminPanelOptionProvider adminPanelOptionProvider = ((List<CreateOptionProviderDelegeate>)(object)_optionProviderCreators)[i]?.Invoke();
					if (adminPanelOptionProvider != null)
					{
						((List<IAdminPanelOptionProvider>)(object)val).Add(adminPanelOptionProvider);
					}
				}
				_dataSource = new MultiplayerAdminPanelVM(OnEscapeMenuToggled, (MBReadOnlyList<IAdminPanelOptionProvider>)(object)val, OnCreateOptionViewModel, OnCreateActionViewModel);
			}
			_gauntletLayer = new GauntletLayer("MultiplayerAdminPanel", base.ViewOrderPriority, false);
			_movie = _gauntletLayer.LoadMovie("MultiplayerAdminPanel", (ViewModel)(object)_dataSource);
			((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
			((ScreenBase)((MissionView)this).MissionScreen).AddLayer((ScreenLayer)(object)_gauntletLayer);
		}
		else
		{
			MultiplayerAdminPanelVM dataSource = _dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).OnFinalize();
			}
			_dataSource = null;
			((ScreenBase)((MissionView)this).MissionScreen).RemoveLayer((ScreenLayer)(object)_gauntletLayer);
			((ScreenLayer)_gauntletLayer).InputRestrictions.ResetInputRestrictions();
			_gauntletLayer = null;
		}
	}
}
