using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;

namespace SandBox.CampaignBehaviors;

public class DumpIntegrityCampaignBehavior : CampaignBehaviorBase
{
	private readonly List<KeyValuePair<string, string>> _saveIntegrityDumpInfo = new List<KeyValuePair<string, string>>();

	private readonly List<KeyValuePair<string, string>> _usedModulesDumpInfo = new List<KeyValuePair<string, string>>();

	private readonly List<KeyValuePair<string, string>> _usedVersionsDumpInfo = new List<KeyValuePair<string, string>>();

	public override void SyncData(IDataStore dataStore)
	{
		IsGameIntegrityAchieved(out var _);
		UpdateDumpInfo();
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnConfigChangedEvent.AddNonSerializedListener((object)this, (Action)OnConfigChanged);
		CampaignEvents.OnNewGameCreatedPartialFollowUpEndEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnNewGameCreatedPartialFollowUpEnd);
	}

	private void OnConfigChanged()
	{
		IsGameIntegrityAchieved(out var _);
		UpdateDumpInfo();
	}

	private void OnNewGameCreatedPartialFollowUpEnd(CampaignGameStarter campaignGameStarter)
	{
		IsGameIntegrityAchieved(out var _);
		UpdateDumpInfo();
	}

	private void UpdateDumpInfo()
	{
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		_saveIntegrityDumpInfo.Clear();
		_usedModulesDumpInfo.Clear();
		_usedVersionsDumpInfo.Clear();
		if (Campaign.Current.NewGameVersion != null)
		{
			_saveIntegrityDumpInfo.Add(new KeyValuePair<string, string>("New Game Version", Campaign.Current.NewGameVersion));
		}
		_saveIntegrityDumpInfo.Add(new KeyValuePair<string, string>("Has Used Cheats", (!CheckCheatUsage()).ToString()));
		Campaign current = Campaign.Current;
		if (((current != null) ? current.PreviouslyUsedModules : null) != null && Campaign.Current.UsedGameVersions != null)
		{
			if (CheckIfModulesAreDefault(out var unofficialModulesCode))
			{
				_saveIntegrityDumpInfo.Add(new KeyValuePair<string, string>("Has Installed Unofficial Modules", "False"));
			}
			else
			{
				_saveIntegrityDumpInfo.Add(new KeyValuePair<string, string>("Has Installed Unofficial Modules", unofficialModulesCode));
			}
			CheckIfVersionIntegrityIsAchieved(out var message);
			_saveIntegrityDumpInfo.Add(new KeyValuePair<string, string>("Has Reverted to Older Versions", message));
			_saveIntegrityDumpInfo.Add(new KeyValuePair<string, string>("Game Integrity is Achieved", IsGameIntegrityAchieved(out var _).ToString()));
		}
		Campaign current2 = Campaign.Current;
		if (((current2 != null) ? current2.PreviouslyUsedModules : null) != null && ((List<string>)(object)Campaign.Current.PreviouslyUsedModules).Count > 0)
		{
			string[] array = ((IEnumerable<string>)Campaign.Current.PreviouslyUsedModules).Last().Split(new char[1] { MBSaveLoad.ModuleCodeSeperator });
			foreach (string text in array)
			{
				string text2 = text.Split(new char[1] { MBSaveLoad.ModuleVersionSeperator })[0];
				string text3 = text.Split(new char[1] { MBSaveLoad.ModuleVersionSeperator })[1];
				ModuleInfo moduleInfo = ModuleHelper.GetModuleInfo(text2);
				string text4 = "Does not exist";
				if (moduleInfo != null)
				{
					text4 = "Exists But Inactive";
					if (moduleInfo.IsActive)
					{
						text4 = "Exists and Active";
					}
				}
				text4 = text4 + " (Version: " + text3 + ")";
				_usedModulesDumpInfo.Add(new KeyValuePair<string, string>(text2, text4));
			}
		}
		if (Campaign.Current.UsedGameVersions != null)
		{
			foreach (string item in (List<string>)(object)Campaign.Current.UsedGameVersions)
			{
				string value = ((item == ((object)MBSaveLoad.CurrentVersion/*cast due to .constrained prefix*/).ToString()) ? "1" : "0");
				_usedVersionsDumpInfo.Add(new KeyValuePair<string, string>(item, value));
			}
		}
		SendDataToWatchdog();
	}

	private void SendDataToWatchdog()
	{
		foreach (KeyValuePair<string, string> item in _saveIntegrityDumpInfo)
		{
			Utilities.SetWatchdogValue("crash_tags.txt", "Campaign Dump Integrity", item.Key, item.Value);
		}
		foreach (KeyValuePair<string, string> item2 in _usedModulesDumpInfo)
		{
			Utilities.SetWatchdogValue("crash_tags.txt", "Used Modules", item2.Key, item2.Value);
		}
		foreach (KeyValuePair<string, string> item3 in _usedVersionsDumpInfo)
		{
			Utilities.SetWatchdogValue("crash_tags.txt", "Used Versions", item3.Key, item3.Value);
		}
	}

	public static bool IsGameIntegrityAchieved(out TextObject reason)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		bool result = true;
		string unofficialModulesCode;
		if (!CheckCheatUsage())
		{
			reason = new TextObject("{=sO8Zh3ZH}Achievements are disabled due to cheat usage.", (Dictionary<string, object>)null);
			result = false;
		}
		else if (!CheckIfModulesAreDefault(out unofficialModulesCode))
		{
			reason = new TextObject("{=R0AbAxqX}Achievements are disabled due to unofficial modules.", (Dictionary<string, object>)null);
			result = false;
		}
		else if (!CheckIfVersionIntegrityIsAchieved(out unofficialModulesCode))
		{
			reason = new TextObject("{=dt00CQCM}Achievements are disabled due to version downgrade.", (Dictionary<string, object>)null);
			result = false;
		}
		else
		{
			reason = null;
		}
		return result;
	}

	private static bool CheckIfVersionIntegrityIsAchieved(out string message)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		message = "False";
		MBReadOnlyList<string> usedGameVersions = Campaign.Current.UsedGameVersions;
		for (int i = 0; i < ((List<string>)(object)usedGameVersions).Count; i++)
		{
			if (i < ((List<string>)(object)usedGameVersions).Count - 1)
			{
				ApplicationVersion val = ApplicationVersion.FromString(((List<string>)(object)usedGameVersions)[i], 0);
				if (((ApplicationVersion)(ref val)).IsNewerThan(ApplicationVersion.FromString(((List<string>)(object)usedGameVersions)[i + 1], 0)))
				{
					message = "Version downgrade from " + ((List<string>)(object)usedGameVersions)[i + 1] + " to " + ((List<string>)(object)usedGameVersions)[i];
					Debug.Print("Dump integrity is compromised due to version downgrade from " + ((List<string>)(object)usedGameVersions)[i + 1] + " to " + ((List<string>)(object)usedGameVersions)[i], 0, (DebugColor)0, 17592186044416uL);
					return false;
				}
			}
		}
		return true;
	}

	private static bool CheckIfModulesAreDefault(out string unofficialModulesCode)
	{
		MBList<string> officialModuleIds = ModuleHelper.GetOfficialModuleIds();
		MBList<string> val = new MBList<string>();
		foreach (string item in (List<string>)(object)Campaign.Current.PreviouslyUsedModules)
		{
			string[] array = item.Split(new char[1] { MBSaveLoad.ModuleCodeSeperator });
			foreach (string text in array)
			{
				string moduleId = text.Split(new char[1] { MBSaveLoad.ModuleVersionSeperator })[0];
				if (!((IEnumerable<string>)officialModuleIds).Any((string x) => moduleId.Equals(x, StringComparison.InvariantCultureIgnoreCase)) && !((List<string>)(object)val).Contains(moduleId))
				{
					((List<string>)(object)val).Add(moduleId);
				}
			}
		}
		unofficialModulesCode = string.Join(MBSaveLoad.ModuleCodeSeperator.ToString(), (IEnumerable<string?>)val);
		if (((List<string>)(object)val).Count > 0)
		{
			Debug.Print("Unofficial modules are used: " + unofficialModulesCode, 0, (DebugColor)0, 17592186044416uL);
		}
		return ((List<string>)(object)val).Count == 0;
	}

	private static bool CheckCheatUsage()
	{
		if (!Campaign.Current.EnabledCheatsBefore && Game.Current.CheatMode)
		{
			Campaign.Current.EnabledCheatsBefore = Game.Current.CheatMode;
		}
		if (Campaign.Current.EnabledCheatsBefore)
		{
			Debug.Print("Dump integrity is compromised due to cheat usage", 0, (DebugColor)0, 17592186044416uL);
		}
		return !Campaign.Current.EnabledCheatsBefore;
	}
}
