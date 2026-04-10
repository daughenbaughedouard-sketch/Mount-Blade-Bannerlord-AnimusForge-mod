using System;
using SandBox.View.Map;
using SandBox.View.Menu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Recruitment;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.Menu;

[OverrideView(typeof(MenuRecruitVolunteersView))]
public class GauntletMenuRecruitVolunteersView : MenuView
{
	private GauntletLayer _layerAsGauntletLayer;

	private RecruitmentVM _dataSource;

	private GauntletMovieIdentifier _movie;

	public override bool ShouldUpdateMenuAfterRemoved => true;

	protected unsafe override void OnInitialize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		base.OnInitialize();
		_dataSource = new RecruitmentVM();
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_dataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_dataSource.SetRecruitAllInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("TakeAll"));
		_dataSource.SetGetKeyTextFromKeyIDFunc(new Func<string, TextObject>(Game.Current.GameTextManager, (nint)(delegate*<GameTextManager, string, TextObject>)(&GameKeyTextExtensions.GetHotKeyGameTextFromKeyID)));
		base.Layer = (ScreenLayer)new GauntletLayer("MapRecruit", 206, false);
		ref GauntletLayer layerAsGauntletLayer = ref _layerAsGauntletLayer;
		ScreenLayer layer = base.Layer;
		layerAsGauntletLayer = (GauntletLayer)(object)((layer is GauntletLayer) ? layer : null);
		base.Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		base.MenuViewContext.AddLayer(base.Layer);
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		_movie = _layerAsGauntletLayer.LoadMovie("RecruitmentPopup", (ViewModel)(object)_dataSource);
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		_dataSource.RefreshScreen();
		_dataSource.Enabled = true;
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)5));
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInRecruitment(isInRecruitment: true);
		}
	}

	protected override void OnFinalize()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		base.Layer.IsFocusLayer = false;
		ScreenManager.TryLoseFocus(base.Layer);
		((ViewModel)_dataSource).OnFinalize();
		_dataSource = null;
		_layerAsGauntletLayer.ReleaseMovie(_movie);
		base.MenuViewContext.RemoveLayer(base.Layer);
		_movie = null;
		base.Layer = null;
		_layerAsGauntletLayer = null;
		Game.Current.EventManager.TriggerEvent<TutorialContextChangedEvent>(new TutorialContextChangedEvent((TutorialContexts)4));
		if (ScreenManager.TopScreen is MapScreen mapScreen)
		{
			mapScreen.SetIsInRecruitment(isInRecruitment: false);
		}
		base.OnFinalize();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		if (base.Layer.Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteForceQuit();
		}
		else if (base.Layer.Input.IsHotKeyReleased("Confirm"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteDone();
		}
		else if (base.Layer.Input.IsHotKeyReleased("Reset"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteReset();
		}
		else if (base.Layer.Input.IsHotKeyReleased("TakeAll"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteRecruitAll();
		}
		else if (base.Layer.Input.IsGameKeyReleased(39))
		{
			if (_dataSource.FocusedVolunteerOwner != null)
			{
				_dataSource.FocusedVolunteerOwner.ExecuteOpenEncyclopedia();
			}
			else if (_dataSource.FocusedVolunteerTroop != null)
			{
				_dataSource.FocusedVolunteerTroop.ExecuteOpenEncyclopedia();
			}
		}
		if (!_dataSource.Enabled)
		{
			base.MenuViewContext.CloseRecruitVolunteers();
		}
	}

	protected override TutorialContexts GetTutorialContext()
	{
		return (TutorialContexts)5;
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
