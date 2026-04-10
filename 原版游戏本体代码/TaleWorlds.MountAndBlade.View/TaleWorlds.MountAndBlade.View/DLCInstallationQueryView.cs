using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.View;

public class DLCInstallationQueryView
{
	public void Initialize()
	{
		EngineController.OnDLCInstalledCallback += OnModuleInstallComplete;
		EngineController.OnDLCLoadedCallback += OnModuleActivated;
	}

	private void OnModuleActivated()
	{
		MBInformationManager.AddQuickInformation(Module.CurrentModule.GlobalTextManager.FindText("str_content_activated_notification", (string)null), 1000, (BasicCharacterObject)null, (Equipment)null, "");
		GameState activeState = Module.CurrentModule.GlobalGameStateManager.ActiveState;
		InitialState val;
		if ((val = (InitialState)(object)((activeState is InitialState) ? activeState : null)) != null)
		{
			val.RefreshContentState();
		}
	}

	private void OnModuleInstallComplete()
	{
		MBInformationManager.AddQuickInformation(Module.CurrentModule.GlobalTextManager.FindText("str_content_installed_notification", (string)null), 1000, (BasicCharacterObject)null, (Equipment)null, "");
		if (!(Module.CurrentModule.GlobalGameStateManager.ActiveState is InitialState))
		{
			CreateInstallationCompleteQuery();
		}
	}

	private void CreateInstallationCompleteQuery()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		GetQueryTexts(out var title, out var description);
		InformationManager.ShowInquiry(new InquiryData(title, description, true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), (string)null, (Action)null, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void GetQueryTexts(out string title, out string description)
	{
		title = ((object)Module.CurrentModule.GlobalTextManager.FindText("str_dlc_installed_title", (string)null)).ToString();
		description = ((object)Module.CurrentModule.GlobalTextManager.FindText("str_dlc_installed_description", (string)null)).ToString();
	}

	public void OnFinalize()
	{
		EngineController.OnDLCInstalledCallback -= OnModuleInstallComplete;
		EngineController.OnDLCLoadedCallback -= OnModuleActivated;
	}
}
