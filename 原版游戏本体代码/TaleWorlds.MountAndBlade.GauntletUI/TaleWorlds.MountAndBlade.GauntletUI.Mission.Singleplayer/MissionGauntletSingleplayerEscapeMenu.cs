using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;

[OverrideView(typeof(MissionSingleplayerEscapeMenu))]
public class MissionGauntletSingleplayerEscapeMenu : MissionGauntletEscapeMenuBase
{
	private MissionOptionsComponent _missionOptionsComponent;

	private bool _isIronmanMode;

	public MissionGauntletSingleplayerEscapeMenu(bool isIronmanMode)
		: base("EscapeMenu")
	{
		_isIronmanMode = isIronmanMode;
	}

	public override void OnMissionScreenInitialize()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		base.OnMissionScreenInitialize();
		_missionOptionsComponent = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionOptionsComponent>();
		DataSource = new EscapeMenuVM((IEnumerable<EscapeMenuItemVM>)null, (TextObject)null);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.OnMissionScreenFinalize();
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
	}

	private void OnManagedOptionChanged(ManagedOptionsType changedManagedOptionsType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)changedManagedOptionsType == 45)
		{
			EscapeMenuVM dataSource = DataSource;
			if (dataSource != null)
			{
				dataSource.RefreshItems((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems());
			}
		}
	}

	public override void OnFocusChangeOnGameWindow(bool focusGained)
	{
		base.OnFocusChangeOnGameWindow(focusGained);
		if (!focusGained && BannerlordConfig.StopGameOnFocusLost && base.MissionScreen.IsOpeningEscapeMenuOnFocusChangeAllowed() && !GameStateManager.Current.ActiveStateDisabledByUser && !LoadingWindow.IsLoadingWindowActive && !base.IsActive)
		{
			OnEscape();
		}
	}

	public override void OnSceneRenderingStarted()
	{
		base.OnSceneRenderingStarted();
		if (base.MissionScreen.IsFocusLost)
		{
			OnFocusChangeOnGameWindow(focusGained: false);
		}
	}

	protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		//IL_00f3: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		TextObject ironmanDisabledReason = GameTexts.FindText("str_pause_menu_disabled_hint", "IronmanMode");
		List<EscapeMenuItemVM> list = new List<EscapeMenuItemVM>();
		list.Add(new EscapeMenuItemVM(new TextObject("{=e139gKZc}Return to the Game", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			OnEscapeMenuToggled(isOpened: false);
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), true));
		list.Add(new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			OnEscapeMenuToggled(isOpened: false);
			MissionOptionsComponent missionOptionsComponent = _missionOptionsComponent;
			if (missionOptionsComponent != null)
			{
				missionOptionsComponent.OnAddOptionsUIHandler();
			}
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
		if (BannerlordConfig.HideBattleUI)
		{
			list.Add(new EscapeMenuItemVM(new TextObject("{=asCeKZXx}Re-enable Battle UI", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				ManagedOptions.SetConfig((ManagedOptionsType)45, 0f);
				ManagedOptions.SaveConfig();
				DataSource.RefreshItems((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems());
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
		}
		if (Input.IsGamepadActive)
		{
			MissionCheatView missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionCheatView>();
			if (missionBehavior != null && missionBehavior.GetIsCheatsAvailable())
			{
				list.Add(new EscapeMenuItemVM(new TextObject("{=WA6Sk6cH}Cheat Menu", (Dictionary<string, object>)null), (Action<object>)delegate
				{
					base.MissionScreen.Mission.GetMissionBehavior<MissionCheatView>().InitializeScreen();
				}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false));
			}
		}
		list.Add(new EscapeMenuItemVM(new TextObject("{=VklN5Wm6}Photo Mode", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			OnEscapeMenuToggled(isOpened: false);
			base.MissionScreen.SetPhotoModeEnabled(isEnabled: true);
			((MissionBehavior)this).Mission.IsInPhotoMode = true;
			InformationManager.HideAllMessages();
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => GetIsPhotoModeDisabled()), false));
		list.Add(new EscapeMenuItemVM(new TextObject("{=RamV6yLM}Exit to Main Menu", (Dictionary<string, object>)null), (Action<object>)delegate
		{
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Expected O, but got Unknown
			Game current = Game.Current;
			if (!(((current != null) ? current.GameType : null) is EditorGame))
			{
				Game current2 = Game.Current;
				if (!(((current2 != null) ? ((object)current2.GameType).GetType().Name : null) == "CustomGame"))
				{
					InformationManager.ShowInquiry(new InquiryData(((object)GameTexts.FindText("str_exit", (string)null)).ToString(), ((object)GameTexts.FindText("str_mission_exit_query", (string)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)OnExitToMainMenu, (Action)delegate
					{
						OnEscapeMenuToggled(isOpened: false);
					}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
					return;
				}
			}
			OnExitToMainMenu();
		}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(_isIronmanMode, ironmanDisabledReason)), false));
		return list;
	}

	private Tuple<bool, TextObject> GetIsPhotoModeDisabled()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		if (base.MissionScreen.IsDeploymentActive)
		{
			return new Tuple<bool, TextObject>(item1: true, new TextObject("{=rZSjkCpw}Cannot use photo mode during deployment.", (Dictionary<string, object>)null));
		}
		if (base.MissionScreen.IsConversationActive)
		{
			return new Tuple<bool, TextObject>(item1: true, new TextObject("{=ImQnhIQ5}Cannot use photo mode during conversation.", (Dictionary<string, object>)null));
		}
		if (base.MissionScreen.IsPhotoModeEnabled)
		{
			return new Tuple<bool, TextObject>(item1: true, new TextObject("{=79bODbwZ}Photo mode is already active.", (Dictionary<string, object>)null));
		}
		if (Module.CurrentModule.IsOnlyCoreContentEnabled)
		{
			return new Tuple<bool, TextObject>(item1: true, new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null));
		}
		if ((int)base.MissionScreen.Mission.Mode == 9)
		{
			return new Tuple<bool, TextObject>(item1: true, new TextObject("{=WazbgBDJ}Disabled during cutscenes.", (Dictionary<string, object>)null));
		}
		if ((NativeObject)(object)base.MissionScreen.CustomCamera != (NativeObject)null || !base.MissionScreen.IsPhotoModeAllowed())
		{
			return new Tuple<bool, TextObject>(item1: true, new TextObject("{=7xU2wu7M}Photo mode isn't allowed.", (Dictionary<string, object>)null));
		}
		return new Tuple<bool, TextObject>(item1: false, null);
	}

	private void OnExitToMainMenu()
	{
		OnEscapeMenuToggled(isOpened: false);
		InformationManager.HideInquiry();
		MBGameManager.EndGame();
	}
}
