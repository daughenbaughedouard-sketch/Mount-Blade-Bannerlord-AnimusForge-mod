using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace SandBox;

public static class SandBoxSaveHelper
{
	public enum SaveHelperState
	{
		Start,
		Inquiry,
		LoadGame
	}

	public readonly struct ModuleCheckResult
	{
		public readonly string ModuleId;

		public readonly ModuleCheckResultType Type;

		public ModuleCheckResult(string moduleId, ModuleCheckResultType type)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			ModuleId = moduleId;
			Type = type;
		}
	}

	private static readonly ApplicationVersion SaveResetVersion = new ApplicationVersion((ApplicationVersionType)2, 1, 7, 0, 0);

	private static readonly TextObject _moduleMismatchInquiryTitle = new TextObject("{=r7xdYj4q}Module Mismatch", (Dictionary<string, object>)null);

	private static readonly TextObject _errorTitle = new TextObject("{=oZrVNUOk}Error", (Dictionary<string, object>)null);

	private static readonly TextObject _saveLoadingProblemText = new TextObject("{=onLDP7mP}A problem occured while trying to load the saved game.", (Dictionary<string, object>)null);

	private static readonly TextObject _saveResetVersionProblemText = new TextObject("{=5hbSkbQg}This save file is from a game version that is older than e1.7.0. Please switch your game version to e1.7.0, load the save file and save the game. This will allow it to work on newer versions beyond e1.7.0.", (Dictionary<string, object>)null);

	private static bool _isInquiryActive;

	public static event Action<SaveHelperState> OnStateChange;

	public static void TryLoadSave(SaveGameFileInfo saveInfo, Action<LoadResult> onStartGame, Action onCancel = null)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Expected O, but got Unknown
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		GameTexts.SetVariable("newline", "\n");
		SandBoxSaveHelper.OnStateChange?.Invoke(SaveHelperState.Start);
		ApplicationVersion saveVersion = MetaDataExtensions.GetApplicationVersion(saveInfo.MetaData);
		bool flag = true;
		MBList<string> officialModules = ModuleHelper.GetOfficialModuleIds();
		MBList<ModuleCheckResult> val = Extensions.ToMBList<ModuleCheckResult>(((IEnumerable<ModuleCheckResult>)CheckMetaDataCompatibilityErrors(saveInfo.MetaData)).Where((ModuleCheckResult x) => !((IEnumerable<string>)officialModules).Any(delegate(string y)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (!(y == x.ModuleId))
			{
				if (((ApplicationVersion)(ref saveVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.0", 0)))
				{
					ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo(y);
					return ((moduleInfo != null) ? moduleInfo.Name : null) == x.ModuleId;
				}
				return false;
			}
			return true;
		})));
		if (((List<ModuleCheckResult>)(object)val).Count > 0)
		{
			IEnumerable<IGrouping<ModuleCheckResultType, ModuleCheckResult>> enumerable = from m in (IEnumerable<ModuleCheckResult>)val
				group m by m.Type;
			GameTextManager globalTextManager = Module.CurrentModule.GlobalTextManager;
			List<TextObject> list = new List<TextObject>();
			foreach (IGrouping<ModuleCheckResultType, ModuleCheckResult> item in enumerable)
			{
				TextObject val2 = new TextObject("{=!}{ERROR}{newline}- {MODULES}", (Dictionary<string, object>)null);
				val2.SetTextVariable("ERROR", globalTextManager.FindText("str_load_module_error", ((object)item.Key/*cast due to .constrained prefix*/).ToString()));
				val2.SetTextVariable("MODULES", string.Join("\n- ", item.Select((ModuleCheckResult x) => x.ModuleId)));
				list.Add(val2);
			}
			TextObject val3 = GameTextHelper.MergeTextObjectsWithSymbol(list, new TextObject("{=!}{newline}{newline}", (Dictionary<string, object>)null), (TextObject)null);
			TextObject empty = TextObject.GetEmpty();
			bool flag2 = MBSaveLoad.CurrentVersion >= saveVersion || flag;
			if (flag2)
			{
				empty = new TextObject("{=!}{STR1}{newline}{newline}{STR2}", (Dictionary<string, object>)null);
				empty.SetTextVariable("STR1", val3);
				empty.SetTextVariable("STR2", new TextObject("{=lh0so0uX}Do you want to load the saved game with different modules?", (Dictionary<string, object>)null));
			}
			else
			{
				empty = val3;
			}
			InquiryData val4 = new InquiryData(((object)_moduleMismatchInquiryTitle).ToString(), ((object)empty).ToString(), flag2, true, ((object)new TextObject("{=aeouhelq}Yes", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action)delegate
			{
				_isInquiryActive = false;
				LoadGameAction(saveInfo, onStartGame, onCancel);
			}, (Action)delegate
			{
				_isInquiryActive = false;
				onCancel?.Invoke();
			}, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
			_isInquiryActive = true;
			InformationManager.ShowInquiry(val4, false, false);
			SandBoxSaveHelper.OnStateChange?.Invoke(SaveHelperState.Inquiry);
		}
		else
		{
			LoadGameAction(saveInfo, onStartGame, onCancel);
		}
	}

	public static MBReadOnlyList<ModuleCheckResult> CheckMetaDataCompatibilityErrors(MetaData fileMetaData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		List<ModuleInfo> modules = ModuleHelper.GetModules((Func<ModuleInfo, bool>)null);
		string[] modules2 = MetaDataExtensions.GetModules(fileMetaData);
		ApplicationVersion applicationVersion = MetaDataExtensions.GetApplicationVersion(fileMetaData);
		MBList<ModuleCheckResult> val = new MBList<ModuleCheckResult>();
		string[] array = modules2;
		foreach (string text in array)
		{
			bool flag = true;
			bool flag2 = false;
			foreach (ModuleInfo item in modules)
			{
				if (item.Id == text || (((ApplicationVersion)(ref applicationVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.0", 0)) && item.Name == text))
				{
					flag = false;
					ApplicationVersion moduleVersion = MetaDataExtensions.GetModuleVersion(fileMetaData, text);
					flag2 = ((ApplicationVersion)(ref moduleVersion)).IsSame(item.Version, false);
					break;
				}
			}
			if (flag)
			{
				((List<ModuleCheckResult>)(object)val).Add(new ModuleCheckResult(text, (ModuleCheckResultType)0));
			}
			else if (!flag2)
			{
				((List<ModuleCheckResult>)(object)val).Add(new ModuleCheckResult(text, (ModuleCheckResultType)2));
			}
		}
		foreach (ModuleInfo item2 in modules)
		{
			bool flag3 = true;
			array = modules2;
			foreach (string text2 in array)
			{
				if (item2.Id == text2 || (((ApplicationVersion)(ref applicationVersion)).IsOlderThan(ApplicationVersion.FromString("v1.3.0", 0)) && item2.Name == text2))
				{
					flag3 = false;
					break;
				}
			}
			if (flag3)
			{
				((List<ModuleCheckResult>)(object)val).Add(new ModuleCheckResult(item2.Id, (ModuleCheckResultType)1));
			}
		}
		return (MBReadOnlyList<ModuleCheckResult>)(object)val;
	}

	public unsafe static bool GetIsDisabledWithReason(SaveGameFileInfo saveGameFileInfo, out TextObject reason)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Invalid comparison between Unknown and I4
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		ApplicationVersion applicationVersion = MetaDataExtensions.GetApplicationVersion(saveGameFileInfo.MetaData);
		ApplicationVersion applicationVersionWithBuildNumber = Utilities.GetApplicationVersionWithBuildNumber();
		reason = TextObject.GetEmpty();
		if (saveGameFileInfo.IsCorrupted)
		{
			reason = new TextObject("{=t6W3UjG0}Save game file appear to be corrupted. Try starting a new campaign or load another one from Saved Games menu.", (Dictionary<string, object>)null);
			return true;
		}
		foreach (ModuleCheckResult item in (List<ModuleCheckResult>)(object)CheckMetaDataCompatibilityErrors(saveGameFileInfo.MetaData))
		{
			if ((int)item.Type == 0)
			{
				if (((List<string>)(object)ModuleHelper.ModulesDisablingLoadingAfterBeingRemoved).Contains(item.ModuleId))
				{
					reason = new TextObject("{=ZidO5gkC}This save file was created with the {MODULE_NAME} module, which has been removed from the game. You need to activate the module to load this save.", (Dictionary<string, object>)null);
					reason.SetTextVariable("MODULE_NAME", GetModuleNameFromModuleId(item.ModuleId));
					return true;
				}
			}
			else if ((int)item.Type == 1 && ((List<string>)(object)ModuleHelper.ModulesDisablingLoadingAfterBeingAdded).Contains(item.ModuleId))
			{
				reason = new TextObject("{=hi6L6kTM}This save file was created without the {MODULE_NAME} module, which has been added to the game. You need to deactivate the module to load this save.", (Dictionary<string, object>)null);
				reason.SetTextVariable("MODULE_NAME", GetModuleNameFromModuleId(item.ModuleId));
				return true;
			}
		}
		if (applicationVersion < SaveResetVersion)
		{
			reason = _saveResetVersionProblemText;
			return true;
		}
		if (applicationVersionWithBuildNumber < applicationVersion)
		{
			reason = new TextObject("{=9svpUWeo}Save version ({SAVE_VERSION}) is newer than the current version ({CURRENT_VERSION}).", (Dictionary<string, object>)null);
			reason.SetTextVariable("SAVE_VERSION", ((object)(*(ApplicationVersion*)(&applicationVersion))/*cast due to .constrained prefix*/).ToString());
			reason.SetTextVariable("CURRENT_VERSION", ((object)(*(ApplicationVersion*)(&applicationVersionWithBuildNumber))/*cast due to .constrained prefix*/).ToString());
			return true;
		}
		return false;
	}

	public static string GetModuleNameFromModuleId(string id)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (id == "NavalDLC")
		{
			return ((object)new TextObject("{=!}War Sails", (Dictionary<string, object>)null)).ToString();
		}
		return id;
	}

	private static void LoadGameAction(SaveGameFileInfo saveInfo, Action<LoadResult> onStartGame, Action onCancel)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		SandBoxSaveHelper.OnStateChange?.Invoke(SaveHelperState.LoadGame);
		LoadResult val = MBSaveLoad.LoadSaveGameData(saveInfo.Name);
		if (val != null)
		{
			onStartGame?.Invoke(val);
			return;
		}
		InquiryData val2 = new InquiryData(((object)_errorTitle).ToString(), ((object)_saveLoadingProblemText).ToString(), true, false, ((object)new TextObject("{=yS7PvrTD}OK", (Dictionary<string, object>)null)).ToString(), string.Empty, (Action)delegate
		{
			_isInquiryActive = false;
			onCancel?.Invoke();
		}, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null);
		_isInquiryActive = true;
		InformationManager.ShowInquiry(val2, false, false);
		SandBoxSaveHelper.OnStateChange?.Invoke(SaveHelperState.Inquiry);
	}
}
