using System;
using System.Collections.Generic;
using System.Linq;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Core.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.Admin.Internal;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade.Multiplayer.Admin;

public class DefaultAdminPanelOptionProvider : IAdminPanelOptionProvider
{
	public static class DefaultOptionIds
	{
		public const string NextGameType = "next_game_type";

		public const string NextMap = "next_map";

		public const string NextCultureTeam1 = "next_culture_team_1";

		public const string NextCultureTeam2 = "next_culture_team_2";

		public const string NextNumberOfRounds = "next_number_of_rounds";

		public const string NextMinScoreToWinDuel = "next_min_score_to_win_duel";

		public const string NextMapTimeLimit = "next_map_time_limit";

		public const string NextRoundTimeLimit = "next_round_time_limit";

		public const string NextWarmupTimeLimit = "next_warmup_time_limit";

		public const string NextMaxNumberOfPlayers = "next_max_num_players";

		public const string ApplyAndStartMission = "apply_and_start";

		public const string WelcomeMessage = "welcome_message";

		public const string AutoTeamBalanceTreshold = "auto_balance_treshold";

		public const string FriendlyFireMeleePercent = "friendly_fire_melee_percent";

		public const string FriendlyFireMeleeReflectionPercent = "friendly_fire_melee_self_percent";

		public const string FriendlyFireRangedPercent = "friendly_fire_ranged_percent";

		public const string FriendlyFireRangedReflectionPercent = "friendly_fire_ranged_self_percent";

		public const string AllowInfantry = "allow_infantry";

		public const string AllowRanged = "allow_ranged";

		public const string AllowCavalry = "allow_cavalry";

		public const string AllowHorseArchers = "allow_horse_archers";

		public const string EndWarmup = "end_warmup";

		public const string MutePlayer = "mute_player";

		public const string KickPlayer = "kick_player";

		public const string BanPlayer = "ban_player";
	}

	private class AdminPanelVotableMultiSelectionOption : AdminPanelMultiSelectionOption
	{
		protected readonly IAdminPanelMultiSelectionItem _undecidedOption;

		public bool IsUndecided { get; private set; }

		public AdminPanelVotableMultiSelectionOption(string uniqueId)
			: base(uniqueId)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			_undecidedOption = new AdminPanelMultiSelectionItem(null, new TextObject("{=b5HkM0tT}Undecided", (Dictionary<string, object>)null), isFallbackValue: true);
		}

		protected override void OnValueChanged(IAdminPanelMultiSelectionItem previousValue, IAdminPanelMultiSelectionItem newValue)
		{
			base.OnValueChanged(previousValue, newValue);
			IsUndecided = _selectedOption == _undecidedOption;
		}

		public override AdminPanelMultiSelectionOption BuildAvailableOptions(MBReadOnlyList<IAdminPanelMultiSelectionItem> options)
		{
			base.BuildAvailableOptions(options);
			AddUndecidedOption();
			if (!((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Contains(base.CurrentValue) && ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count > 0)
			{
				BuildInitialValue(((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[0]);
				SetValue(((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[0]);
			}
			return this;
		}

		public override AdminPanelMultiSelectionOption BuildAvailableOptions(OptionType optionType, bool buildDefaultValue = true)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			base.BuildAvailableOptions(optionType, buildDefaultValue: false);
			AddUndecidedOption();
			if (!((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Contains(base.CurrentValue) && ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count > 0)
			{
				BuildInitialValue(((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[0]);
				SetValue(((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[0]);
			}
			return this;
		}

		protected void AddUndecidedOption()
		{
			for (int i = 0; i < ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count; i++)
			{
				if (((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[i] == _undecidedOption || ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[i].Value == _undecidedOption.Value)
				{
					return;
				}
			}
			if (!GetIsDisabled(out var _))
			{
				((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Insert(0, _undecidedOption);
				BuildDefaultValue(_undecidedOption);
				BuildInitialValue(_undecidedOption);
				SetValue(_undecidedOption);
			}
		}

		protected void RemoveUndecidedOption()
		{
			bool flag = false;
			for (int i = 0; i < ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count; i++)
			{
				if (((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[i] == _undecidedOption || ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[i].Value == _undecidedOption.Value)
				{
					((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (flag && ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count > 0)
			{
				IAdminPanelMultiSelectionItem value = ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[0];
				BuildDefaultValue(value);
				BuildInitialValue(value);
				SetValue(value);
			}
		}
	}

	private class AdminPanelCultureOption : AdminPanelVotableMultiSelectionOption
	{
		private bool _shouldKeepUndecidedOption;

		private AdminPanelCultureOption _otherOption;

		public AdminPanelCultureOption(string uniqueId)
			: base(uniqueId)
		{
		}

		public AdminPanelCultureOption BuildOtherCultureOption(AdminPanelCultureOption otherOption)
		{
			_otherOption?.RemoveValueChangedCallback(OnOtherOptionValueChanged);
			_otherOption = otherOption;
			_otherOption?.AddValueChangedCallback(OnOtherOptionValueChanged);
			return this;
		}

		public override void OnFinalize()
		{
			base.OnFinalize();
			_otherOption?.RemoveValueChangedCallback(OnOtherOptionValueChanged);
		}

		protected override void OnValueChanged(IAdminPanelMultiSelectionItem previousValue, IAdminPanelMultiSelectionItem newValue)
		{
			bool isUndecided = base.IsUndecided;
			base.OnValueChanged(previousValue, newValue);
			if (isUndecided && !base.IsUndecided)
			{
				_shouldKeepUndecidedOption = true;
			}
			else if (!isUndecided && base.IsUndecided)
			{
				_shouldKeepUndecidedOption = false;
			}
		}

		private void OnOtherOptionValueChanged()
		{
			if (_otherOption.IsUndecided)
			{
				AddUndecidedOption();
			}
			else if (!_shouldKeepUndecidedOption)
			{
				RemoveUndecidedOption();
			}
		}
	}

	private class AdminPanelUsableMapsOption : AdminPanelVotableMultiSelectionOption
	{
		private const string _disabledOptionTag = "map_option_disabled";

		private const string _undecidedOptionTag = "map_option_undecided";

		private readonly Dictionary<string, MBList<IAdminPanelMultiSelectionItem>> _optionsByGameType;

		private readonly IAdminPanelMultiSelectionItem _disabledOption;

		private bool _isUpdatingOptions;

		private AdminPanelVotableMultiSelectionOption _gameTypeOption;

		public AdminPanelUsableMapsOption(string uniqueId)
			: base(uniqueId)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			_optionsByGameType = new Dictionary<string, MBList<IAdminPanelMultiSelectionItem>>();
			_disabledOption = new AdminPanelMultiSelectionItem(null, new TextObject("{=1JlzQIXE}Disabled", (Dictionary<string, object>)null), isFallbackValue: false, isDisabled: true);
			Dictionary<string, MBList<IAdminPanelMultiSelectionItem>> optionsByGameType = _optionsByGameType;
			MBList<IAdminPanelMultiSelectionItem> obj = new MBList<IAdminPanelMultiSelectionItem>();
			((List<IAdminPanelMultiSelectionItem>)(object)obj).Add(_disabledOption);
			optionsByGameType["map_option_disabled"] = obj;
			Dictionary<string, MBList<IAdminPanelMultiSelectionItem>> optionsByGameType2 = _optionsByGameType;
			MBList<IAdminPanelMultiSelectionItem> obj2 = new MBList<IAdminPanelMultiSelectionItem>();
			((List<IAdminPanelMultiSelectionItem>)(object)obj2).Add(_undecidedOption);
			optionsByGameType2["map_option_undecided"] = obj2;
		}

		public AdminPanelUsableMapsOption BuildGameTypeOption(AdminPanelVotableMultiSelectionOption gameTypeOption)
		{
			_gameTypeOption = gameTypeOption;
			_gameTypeOption?.AddValueChangedCallback(UpdateOptions);
			UpdateOptions();
			return this;
		}

		public override void OnFinalize()
		{
			base.OnFinalize();
			_gameTypeOption?.RemoveValueChangedCallback(UpdateOptions);
			_gameTypeOption = null;
		}

		public override bool GetIsDisabled(out string reason)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			if (((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions).Count == 1 && ((List<IAdminPanelMultiSelectionItem>)(object)_availableOptions)[0] == _disabledOption)
			{
				reason = ((object)new TextObject("{=2WOGNYG4}No available maps added for game type", (Dictionary<string, object>)null)).ToString();
				return true;
			}
			reason = string.Empty;
			return false;
		}

		private void UpdateOptions()
		{
			if (_isUpdatingOptions)
			{
				return;
			}
			_isUpdatingOptions = true;
			IAdminPanelMultiSelectionItem value = _gameTypeOption.GetValue();
			List<string> usableMaps = MultiplayerIntermissionVotingManager.Instance.GetUsableMaps(value.Value);
			FilterAvailableOptions(usableMaps);
			string key = (_gameTypeOption.IsUndecided ? "map_option_undecided" : ((usableMaps == null || usableMaps.Count <= 0) ? "map_option_disabled" : value.Value));
			if (_optionsByGameType.TryGetValue(key, out var value2))
			{
				if (!((IEnumerable<IAdminPanelMultiSelectionItem>)_availableOptions).SequenceEqual((IEnumerable<IAdminPanelMultiSelectionItem>)value2))
				{
					BuildAvailableOptions((MBReadOnlyList<IAdminPanelMultiSelectionItem>)(object)value2);
				}
				_isUpdatingOptions = false;
				return;
			}
			MBList<IAdminPanelMultiSelectionItem> val = new MBList<IAdminPanelMultiSelectionItem>();
			for (int i = 0; i < usableMaps.Count; i++)
			{
				AdminPanelMultiSelectionItem item = new AdminPanelMultiSelectionItem(usableMaps[i], null);
				((List<IAdminPanelMultiSelectionItem>)(object)val).Add((IAdminPanelMultiSelectionItem)item);
			}
			BuildAvailableOptions((MBReadOnlyList<IAdminPanelMultiSelectionItem>)(object)val);
			_optionsByGameType[key] = val;
			_isUpdatingOptions = false;
		}

		private void FilterAvailableOptions(List<string> availableOptions)
		{
			if (availableOptions.Count == 0)
			{
				return;
			}
			MBReadOnlyList<MultiplayerGameTypeInfo> multiplayerGameTypes = Module.CurrentModule.GetMultiplayerGameTypes();
			List<string> list = new List<string>();
			MultiplayerGameTypeInfo val = ((IEnumerable<MultiplayerGameTypeInfo>)multiplayerGameTypes).FirstOrDefault((Func<MultiplayerGameTypeInfo, bool>)((MultiplayerGameTypeInfo x) => x.GameType == _gameTypeOption.GetValue()?.Value));
			if (val == null)
			{
				return;
			}
			IEnumerable<string> source = ((IEnumerable<MultiplayerGameTypeInfo>)multiplayerGameTypes).SelectMany((MultiplayerGameTypeInfo g) => g.Scenes);
			for (int num = 0; num < availableOptions.Count; num++)
			{
				string text = availableOptions[num];
				if (source.Contains(text) && !val.Scenes.Contains(text))
				{
					list.Add(text);
				}
			}
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				string item = list[num2];
				availableOptions.Remove(item);
			}
		}
	}

	private class AdminPanelStartMissionAction : AdminPanelAction
	{
		private MBReadOnlyList<IAdminPanelOptionGroup> _optionGroups;

		public AdminPanelStartMissionAction(string uniqueId)
			: base(uniqueId)
		{
		}

		public AdminPanelStartMissionAction BuildOptionGroups(MBReadOnlyList<IAdminPanelOptionGroup> optionGroups)
		{
			_optionGroups = optionGroups;
			return this;
		}

		public override bool GetIsDisabled(out string reason)
		{
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Expected O, but got Unknown
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Expected O, but got Unknown
			reason = string.Empty;
			if (_optionGroups != null)
			{
				for (int i = 0; i < ((List<IAdminPanelOptionGroup>)(object)_optionGroups).Count; i++)
				{
					for (int j = 0; j < ((List<IAdminPanelOption>)(object)((List<IAdminPanelOptionGroup>)(object)_optionGroups)[i].Options).Count; j++)
					{
						IAdminPanelOption adminPanelOption = ((List<IAdminPanelOption>)(object)((List<IAdminPanelOptionGroup>)(object)_optionGroups)[i].Options)[j];
						if (adminPanelOption.IsRequired && adminPanelOption.GetIsAvailable() && adminPanelOption.GetIsDisabled(out var _))
						{
							reason = ((object)new TextObject("{=TrY4VS1R}Please select valid values for options.", (Dictionary<string, object>)null)).ToString();
							return true;
						}
					}
				}
			}
			if (!MultiplayerIntermissionVotingManager.Instance.IsAutomatedBattleSwitchingEnabled)
			{
				reason = ((object)new TextObject("{=0WDSCBNa}Server does not support automated battle switching.", (Dictionary<string, object>)null)).ToString();
				return true;
			}
			return false;
		}

		public override void OnFinalize()
		{
			base.OnFinalize();
			_optionGroups = null;
		}
	}

	private class AdminPanelGameTypeDependentNumericOption : AdminPanelNumericOption
	{
		private AdminPanelVotableMultiSelectionOption _gameTypeOption;

		private List<string> _invalidGameTypes;

		private List<string> _requiredGameTypes;

		public AdminPanelGameTypeDependentNumericOption(string uniqueId)
			: base(uniqueId)
		{
		}

		public override bool GetIsAvailable()
		{
			if (_gameTypeOption == null)
			{
				Debug.Print("Game type option is not set for game type dependent option: " + base.Name, 0, (DebugColor)12, 17592186044416uL);
				Debug.FailedAssert("Game type option is not set for game type dependent option: " + base.Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Admin\\DefaultAdminPanelOptionProvider.cs", "GetIsAvailable", 994);
				return true;
			}
			if (_gameTypeOption.IsUndecided)
			{
				return true;
			}
			string value = _gameTypeOption.GetValue().Value;
			if (string.IsNullOrEmpty(value))
			{
				return true;
			}
			if (_invalidGameTypes != null)
			{
				return !_invalidGameTypes.Contains(value);
			}
			if (_requiredGameTypes != null)
			{
				return _requiredGameTypes.Contains(value);
			}
			return true;
		}

		public AdminPanelGameTypeDependentNumericOption BuildGameTypeOption(AdminPanelVotableMultiSelectionOption gameTypeOption)
		{
			_gameTypeOption = gameTypeOption;
			return this;
		}

		public AdminPanelGameTypeDependentNumericOption BuildInvalidGameTypes(string[] gameTypes)
		{
			_invalidGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_invalidGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}

		public AdminPanelGameTypeDependentNumericOption BuildRequiredGameTypes(string[] gameTypes)
		{
			_requiredGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_requiredGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}
	}

	private class AdminPanelGameTypeDependentAction : AdminPanelAction
	{
		private AdminPanelVotableMultiSelectionOption _gameTypeOption;

		private List<string> _invalidGameTypes;

		private List<string> _requiredGameTypes;

		public AdminPanelGameTypeDependentAction(string uniqueId)
			: base(uniqueId)
		{
		}

		public override bool GetIsAvailable()
		{
			if (_gameTypeOption == null)
			{
				Debug.Print("Game type option is not set for game type dependent option: " + base.Name, 0, (DebugColor)12, 17592186044416uL);
				Debug.FailedAssert("Game type option is not set for game type dependent option: " + base.Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Admin\\DefaultAdminPanelOptionProvider.cs", "GetIsAvailable", 1080);
				return true;
			}
			if (_gameTypeOption.IsUndecided)
			{
				return true;
			}
			string value = _gameTypeOption.GetValue().Value;
			if (string.IsNullOrEmpty(value))
			{
				return true;
			}
			if (_invalidGameTypes != null)
			{
				return !_invalidGameTypes.Contains(value);
			}
			if (_requiredGameTypes != null)
			{
				return _requiredGameTypes.Contains(value);
			}
			return true;
		}

		public AdminPanelGameTypeDependentAction BuildGameTypeOption(AdminPanelVotableMultiSelectionOption gameTypeOption)
		{
			_gameTypeOption = gameTypeOption;
			return this;
		}

		public AdminPanelGameTypeDependentAction BuildInvalidGameTypes(string[] gameTypes)
		{
			_invalidGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_invalidGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}

		public AdminPanelGameTypeDependentAction BuildRequiredGameTypes(string[] gameTypes)
		{
			_requiredGameTypes = new List<string>();
			if (gameTypes != null)
			{
				for (int i = 0; i < gameTypes.Length; i++)
				{
					_requiredGameTypes.Add(gameTypes[i]);
				}
			}
			return this;
		}
	}

	private readonly MultiplayerAdminComponent _multiplayerAdminComponent;

	private readonly MissionLobbyComponent _missionLobbyComponent;

	private MBList<IAdminPanelOptionGroup> _optionGroups;

	private AdminPanelVotableMultiSelectionOption _gameTypeOption;

	public DefaultAdminPanelOptionProvider(MultiplayerAdminComponent adminComponent, MissionLobbyComponent missionLobbyComponent)
	{
		_multiplayerAdminComponent = adminComponent;
		_missionLobbyComponent = missionLobbyComponent;
		_optionGroups = new MBList<IAdminPanelOptionGroup>();
	}

	public void OnTick(float dt)
	{
		for (int i = 0; i < ((List<IAdminPanelOptionGroup>)(object)_optionGroups).Count; i++)
		{
			if (((List<IAdminPanelOptionGroup>)(object)_optionGroups)[i] is IAdminPanelTickable adminPanelTickable)
			{
				adminPanelTickable.OnTick(dt);
			}
		}
	}

	public void OnFinalize()
	{
		if (_optionGroups != null)
		{
			for (int i = 0; i < ((List<IAdminPanelOptionGroup>)(object)_optionGroups).Count; i++)
			{
				((List<IAdminPanelOptionGroup>)(object)_optionGroups)[i].OnFinalize();
			}
		}
		_gameTypeOption = null;
	}

	public IAdminPanelOption GetOptionWithId(string id)
	{
		foreach (IAdminPanelOptionGroup item in (List<IAdminPanelOptionGroup>)(object)_optionGroups)
		{
			foreach (IAdminPanelOption item2 in (List<IAdminPanelOption>)(object)item.Options)
			{
				if (item2.UniqueId == id)
				{
					return item2;
				}
			}
		}
		return null;
	}

	public IAdminPanelAction GetActionWithId(string id)
	{
		foreach (IAdminPanelOptionGroup item in (List<IAdminPanelOptionGroup>)(object)_optionGroups)
		{
			foreach (IAdminPanelAction item2 in (List<IAdminPanelAction>)(object)item.Actions)
			{
				if (item2.UniqueId == id)
				{
					return item2;
				}
			}
		}
		return null;
	}

	public void ApplyOptions()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		AdminUpdateMultiplayerOptions val = new AdminUpdateMultiplayerOptions();
		IEnumerable<IAdminPanelOption> enumerable = ((IEnumerable<IAdminPanelOptionGroup>)_optionGroups).SelectMany((IAdminPanelOptionGroup x) => (IEnumerable<IAdminPanelOption>)x.Options);
		foreach (IAdminPanelOption item in enumerable)
		{
			if (!(item is IAdminPanelOptionInternal adminPanelOptionInternal))
			{
				continue;
			}
			OptionType optionType = adminPanelOptionInternal.GetOptionType();
			MultiplayerOptionsAccessMode optionAccessMode = adminPanelOptionInternal.GetOptionAccessMode();
			if ((int)optionType != 43 && (int)optionAccessMode != 3)
			{
				if (item is IAdminPanelOption<bool> adminPanelOption)
				{
					val.AddMultiplayerOption(optionType, optionAccessMode, adminPanelOption.GetValue());
				}
				if (item is IAdminPanelOption<int> adminPanelOption2)
				{
					val.AddMultiplayerOption(optionType, optionAccessMode, adminPanelOption2.GetValue());
				}
				if (item is IAdminPanelOption<string> adminPanelOption3)
				{
					val.AddMultiplayerOption(optionType, optionAccessMode, adminPanelOption3.GetValue());
				}
				if (item is IAdminPanelMultiSelectionOption adminPanelMultiSelectionOption)
				{
					val.AddMultiplayerOption(optionType, optionAccessMode, adminPanelMultiSelectionOption.GetValue().Value);
				}
			}
		}
		GameNetwork.BeginModuleEventAsClient();
		GameNetwork.WriteMessage((GameNetworkMessage)(object)val);
		GameNetwork.EndModuleEventAsClient();
		foreach (IAdminPanelOption item2 in enumerable)
		{
			if (item2 is IAdminPanelOptionInternal adminPanelOptionInternal2)
			{
				adminPanelOptionInternal2.OnApplyChanges();
			}
		}
	}

	public MBReadOnlyList<IAdminPanelOptionGroup> GetOptionGroups()
	{
		((List<IAdminPanelOptionGroup>)(object)_optionGroups).Clear();
		if (MultiplayerIntermissionVotingManager.Instance.IsAutomatedBattleSwitchingEnabled)
		{
			((List<IAdminPanelOptionGroup>)(object)_optionGroups).Add((IAdminPanelOptionGroup)GetMissionOptions());
		}
		((List<IAdminPanelOptionGroup>)(object)_optionGroups).Add((IAdminPanelOptionGroup)GetImmediateEffectOptions());
		((List<IAdminPanelOptionGroup>)(object)_optionGroups).Add((IAdminPanelOptionGroup)GetActions());
		return (MBReadOnlyList<IAdminPanelOptionGroup>)(object)_optionGroups;
	}

	private T GetValueFromOption<T>(string optionId)
	{
		if (((IAdminPanelOptionProvider)this).GetOptionWithId(optionId) is IAdminPanelOption<T> adminPanelOption)
		{
			return adminPanelOption.GetValue();
		}
		Debug.FailedAssert($"Failed to find \"{typeof(T)}\" type option with id: {optionId}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Admin\\DefaultAdminPanelOptionProvider.cs", "GetValueFromOption", 185);
		return default(T);
	}

	private AdminPanelOptionGroup GetMissionOptions()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Expected O, but got Unknown
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Expected O, but got Unknown
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Expected O, but got Unknown
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Expected O, but got Unknown
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Expected O, but got Unknown
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Expected O, but got Unknown
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Expected O, but got Unknown
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Expected O, but got Unknown
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Expected O, but got Unknown
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Expected O, but got Unknown
		AdminPanelOptionGroup adminPanelOptionGroup = new AdminPanelOptionGroup("mission_options", new TextObject("{=xa8i1dM1}Mission Options", (Dictionary<string, object>)null), requiresRestart: true);
		AdminPanelOption<IAdminPanelMultiSelectionItem> adminPanelOption = new AdminPanelVotableMultiSelectionOption("next_game_type").BuildAvailableOptions((OptionType)11).BuildOptionType((OptionType)11, (MultiplayerOptionsAccessMode)2, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=JPimShCw}Game Type", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=ueFrMu6i}Next game type.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true);
		_gameTypeOption = adminPanelOption as AdminPanelVotableMultiSelectionOption;
		adminPanelOptionGroup.AddOption(adminPanelOption);
		adminPanelOptionGroup.AddOption(new AdminPanelUsableMapsOption("next_map").BuildGameTypeOption(adminPanelOption as AdminPanelVotableMultiSelectionOption).BuildOptionType((OptionType)13, (MultiplayerOptionsAccessMode)2, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=w9m11T1y}Map", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=ok1CD7dH}Next map to play.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		AdminPanelCultureOption adminPanelCultureOption = new AdminPanelCultureOption("next_culture_team_1").BuildAvailableOptions((OptionType)14).BuildOptionType((OptionType)14, (MultiplayerOptionsAccessMode)2, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=sGDo0mxT}Attacker Culture", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=wsOUaxf4}Culture of the attacker team in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true) as AdminPanelCultureOption;
		AdminPanelCultureOption adminPanelCultureOption2 = new AdminPanelCultureOption("next_culture_team_2").BuildAvailableOptions((OptionType)15).BuildOptionType((OptionType)15, (MultiplayerOptionsAccessMode)2, buildDefaultValue: false, buildInitialValue: false).BuildName(new TextObject("{=CeERJpan}Defender Culture", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=0jMXI0qT}Culture of the defender team in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true) as AdminPanelCultureOption;
		adminPanelCultureOption.BuildOtherCultureOption(adminPanelCultureOption2);
		adminPanelCultureOption2.BuildOtherCultureOption(adminPanelCultureOption);
		adminPanelOptionGroup.AddOption(adminPanelCultureOption);
		adminPanelOptionGroup.AddOption(adminPanelCultureOption2);
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_number_of_rounds").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[3]
		{
			((object)(MultiplayerGameType)0/*cast due to .constrained prefix*/).ToString(),
			((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString(),
			((object)(MultiplayerGameType)2/*cast due to .constrained prefix*/).ToString()
		}).SetMinimumAndMaximumFrom((OptionType)30)
			.BuildOptionType((OptionType)30, (MultiplayerOptionsAccessMode)2)
			.BuildName(new TextObject("{=VwveHldM}Number of Rounds", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=ndCjGgEj}Total number of rounds in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_min_score_to_win_duel").BuildGameTypeOption(_gameTypeOption).BuildRequiredGameTypes(new string[1] { ((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString() }).SetMinimumAndMaximumFrom((OptionType)37)
			.BuildOptionType((OptionType)37, (MultiplayerOptionsAccessMode)2)
			.BuildName(new TextObject("{=JISyGr4E}Minimum Score to Win Duel", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=5V30jDb7}Minimum score required to win duels.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelNumericOption("next_map_time_limit").SetMinimumAndMaximumFrom((OptionType)27).BuildOptionType((OptionType)27, (MultiplayerOptionsAccessMode)2).BuildName(new TextObject("{=lf1eQ0tB}Map Time Limit", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=xgps8dXU}Time limit in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_round_time_limit").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[3]
		{
			((object)(MultiplayerGameType)0/*cast due to .constrained prefix*/).ToString(),
			((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString(),
			((object)(MultiplayerGameType)2/*cast due to .constrained prefix*/).ToString()
		}).SetMinimumAndMaximumFrom((OptionType)28)
			.BuildOptionType((OptionType)28, (MultiplayerOptionsAccessMode)2)
			.BuildName(new TextObject("{=9k0H0xu0}Round Time Limit", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=ApQhQe6u}Round time limit in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("next_warmup_time_limit").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[2]
		{
			((object)(MultiplayerGameType)0/*cast due to .constrained prefix*/).ToString(),
			((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString()
		}).SetMinimumAndMaximumFrom((OptionType)26)
			.BuildOptionType((OptionType)26, (MultiplayerOptionsAccessMode)2)
			.BuildName(new TextObject("{=XwZTiF8l}Warmup Time Limit", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=S5Ayobba}Warmup time limit in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddOption(new AdminPanelNumericOption("next_max_num_players").SetMinimumAndMaximumFrom((OptionType)16).BuildOptionType((OptionType)16, (MultiplayerOptionsAccessMode)2).BuildName(new TextObject("{=tzcK3R0v}Maximum Number of Players", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=RENeJbg5}Maximum number of players in the next game.", (Dictionary<string, object>)null))
			.BuildIsRequired(isRequired: true));
		adminPanelOptionGroup.AddAction(new AdminPanelStartMissionAction("apply_and_start").BuildOptionGroups((MBReadOnlyList<IAdminPanelOptionGroup>)(object)_optionGroups).BuildName(new TextObject("{=kwo09aDm}Apply and Start Mission", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=8D8KuKxk}Apply all changes and start a new mission.", (Dictionary<string, object>)null))
			.BuildOnActionExecutedCallback(delegate
			{
				ApplyOptions();
				_multiplayerAdminComponent.ChangeAdminMenuActiveState(isActive: false);
				_multiplayerAdminComponent.AdminEndMission();
			}));
		return adminPanelOptionGroup;
	}

	private AdminPanelOptionGroup GetImmediateEffectOptions()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected O, but got Unknown
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected O, but got Unknown
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Expected O, but got Unknown
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Expected O, but got Unknown
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Expected O, but got Unknown
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Expected O, but got Unknown
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Expected O, but got Unknown
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Expected O, but got Unknown
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Expected O, but got Unknown
		AdminPanelOptionGroup adminPanelOptionGroup = new AdminPanelOptionGroup("immediate_effects", new TextObject("{=TcBcNdSE}Immediate Effects", (Dictionary<string, object>)null));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<string>("welcome_message").BuildOptionType((OptionType)1, (MultiplayerOptionsAccessMode)1).BuildName(new TextObject("{=t2Oh6uty}Welcome Message", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=v1DiZaoK}Change the server welcome message.", (Dictionary<string, object>)null)));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("auto_balance_treshold").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { ((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString() }).SetMinimumAndMaximumFrom((OptionType)39)
			.BuildOptionType((OptionType)39, (MultiplayerOptionsAccessMode)1)
			.BuildName(new TextObject("{=YdnTEREg}Team Balance Threshold", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=DenCZPAg}Change the team balance threshold value.", (Dictionary<string, object>)null)));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_melee_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { ((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString() }).SetMinimumAndMaximumFrom((OptionType)22)
			.BuildOptionType((OptionType)22, (MultiplayerOptionsAccessMode)1)
			.BuildName(new TextObject("{=VpQZquwB}Friendly Melee Damage", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=3HgzxHqT}Change the value of friendly melee damage.", (Dictionary<string, object>)null)));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_melee_self_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { ((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString() }).SetMinimumAndMaximumFrom((OptionType)21)
			.BuildOptionType((OptionType)21, (MultiplayerOptionsAccessMode)1)
			.BuildName(new TextObject("{=wLTiwbBt}Friendly Reflective Melee Damage", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=daq8AjgZ}Change the value of reflective friendly melee damage.", (Dictionary<string, object>)null)));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_ranged_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { ((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString() }).SetMinimumAndMaximumFrom((OptionType)24)
			.BuildOptionType((OptionType)24, (MultiplayerOptionsAccessMode)1)
			.BuildName(new TextObject("{=pzudHx88}Friendly Ranged Damage", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=0H1Pg2RF}Change the value of friendly ranged damage.", (Dictionary<string, object>)null)));
		adminPanelOptionGroup.AddOption(new AdminPanelGameTypeDependentNumericOption("friendly_fire_ranged_self_percent").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[1] { ((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString() }).SetMinimumAndMaximumFrom((OptionType)23)
			.BuildOptionType((OptionType)23, (MultiplayerOptionsAccessMode)1)
			.BuildName(new TextObject("{=ZYw87dlh}Friendly Reflective Ranged Damage", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=ih2t4B8E}Change the value of reflective friendly ranged damage.", (Dictionary<string, object>)null)));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_infantry").BuildName(new TextObject("{=H72xVNwz}Allow Infantry", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=FB9tHuWF}Allow usage of infantry troops in game.", (Dictionary<string, object>)null)).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable((FormationClass)0))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable((FormationClass)0))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction((FormationClass)0, !val);
			}));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_ranged").BuildName(new TextObject("{=wFlbhObU}Allow Archers", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=3MiLBVAH}Allow usage of archer troops in game.", (Dictionary<string, object>)null)).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable((FormationClass)1))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable((FormationClass)1))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction((FormationClass)1, !val);
			}));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_cavalry").BuildName(new TextObject("{=nboyCQpj}Allow Cavalry", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=iTZkSZXI}Allow usage of cavalry troops in game.", (Dictionary<string, object>)null)).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable((FormationClass)2))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable((FormationClass)2))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction((FormationClass)2, !val);
			}));
		adminPanelOptionGroup.AddOption(new AdminPanelOption<bool>("allow_horse_archers").BuildName(new TextObject("{=6yTHziN5}Allow Horse Archers", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=P8dk4qSf}Allow usage of horse archer troops in game.", (Dictionary<string, object>)null)).BuildDefaultValue(_missionLobbyComponent.IsClassAvailable((FormationClass)3))
			.BuildInitialValue(_missionLobbyComponent.IsClassAvailable((FormationClass)3))
			.BuildOnAppliedCallback(delegate(bool val)
			{
				_multiplayerAdminComponent.ChangeClassRestriction((FormationClass)3, !val);
			}));
		return adminPanelOptionGroup;
	}

	private AdminPanelOptionGroup GetActions()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Expected O, but got Unknown
		AdminPanelOptionGroup adminPanelOptionGroup = new AdminPanelOptionGroup("actions", new TextObject("{=Za3U3MY4}Actions", (Dictionary<string, object>)null));
		adminPanelOptionGroup.AddAction(new AdminPanelGameTypeDependentAction("end_warmup").BuildGameTypeOption(_gameTypeOption).BuildInvalidGameTypes(new string[2]
		{
			((object)(MultiplayerGameType)0/*cast due to .constrained prefix*/).ToString(),
			((object)(MultiplayerGameType)1/*cast due to .constrained prefix*/).ToString()
		}).BuildName(new TextObject("{=AVDDCWhv}End Warmup", (Dictionary<string, object>)null))
			.BuildDescription(new TextObject("{=Q6HPNb6Q}Set warmup timer to maximum of 30 seconds.", (Dictionary<string, object>)null))
			.BuildOnActionExecutedCallback(delegate
			{
				_multiplayerAdminComponent.EndWarmup();
			}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("mute_player").BuildName(new TextObject("{=QvxOnnZg}Mute Players", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=qMJsMUtO}Select players to mute.", (Dictionary<string, object>)null)).BuildOnActionExecutedCallback(delegate
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Expected O, but got Unknown
			List<InquiryElement> list = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
			{
				if (!MultiplayerGlobalMutedPlayersManager.IsUserMuted(networkPeer.VirtualPlayer.Id))
				{
					list.Add(new InquiryElement((object)networkPeer, networkPeer.UserName, (ImageIdentifier)null));
				}
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=QvxOnnZg}Mute Players", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=qMJsMUtO}Select players to mute.", (Dictionary<string, object>)null)).ToString(), list, true, 0, 1, ((object)new TextObject("{=SfJgnzdq}Mute", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedPlayers)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator val = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (val != null)
					{
						_multiplayerAdminComponent.GlobalMuteUnmutePlayer(val, unmute: false);
					}
				}
			}, (Action<List<InquiryElement>>)null, string.Empty, true), false, false);
		}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("mute_player").BuildName(new TextObject("{=NkDBzEzd}Unmute Players", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=9zJaIpIZ}Select players to unmute.", (Dictionary<string, object>)null)).BuildOnActionExecutedCallback(delegate
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Expected O, but got Unknown
			List<InquiryElement> list = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer2 in GameNetwork.NetworkPeers)
			{
				if (MultiplayerGlobalMutedPlayersManager.IsUserMuted(networkPeer2.VirtualPlayer.Id))
				{
					list.Add(new InquiryElement((object)networkPeer2, networkPeer2.UserName, (ImageIdentifier)null));
				}
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=NkDBzEzd}Unmute Players", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=9zJaIpIZ}Select players to unmute.", (Dictionary<string, object>)null)).ToString(), list, true, 0, 1, ((object)new TextObject("{=HyG3eUFN}Unmute", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedPlayers)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator val = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (val != null)
					{
						_multiplayerAdminComponent.GlobalMuteUnmutePlayer(val, unmute: true);
					}
				}
			}, (Action<List<InquiryElement>>)null, string.Empty, true), false, false);
		}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("kick_player").BuildName(new TextObject("{=cPbHqGrI}Kick Player", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=lZxxVl17}Select a player to kick.", (Dictionary<string, object>)null)).BuildOnActionExecutedCallback(delegate
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Expected O, but got Unknown
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			List<InquiryElement> list = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer3 in GameNetwork.NetworkPeers)
			{
				list.Add(new InquiryElement((object)networkPeer3, networkPeer3.UserName, (ImageIdentifier)null));
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=cPbHqGrI}Kick Player", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=RKNTl0Tn}Select player to kick", (Dictionary<string, object>)null)).ToString(), list, true, 0, 1, ((object)new TextObject("{=DdOgvhsV}Kick", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedPlayers)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator val = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (val != null)
					{
						_multiplayerAdminComponent.KickPlayer(val, banPlayer: false);
					}
				}
			}, (Action<List<InquiryElement>>)null, string.Empty, true), false, false);
		}));
		adminPanelOptionGroup.AddAction(new AdminPanelAction("ban_player").BuildName(new TextObject("{=pbp0GQdO}Ban Player", (Dictionary<string, object>)null)).BuildDescription(new TextObject("{=aJGlM29l}Select a player to ban.", (Dictionary<string, object>)null)).BuildOnActionExecutedCallback(delegate
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Expected O, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Expected O, but got Unknown
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			List<InquiryElement> list = new List<InquiryElement>();
			foreach (NetworkCommunicator networkPeer4 in GameNetwork.NetworkPeers)
			{
				list.Add(new InquiryElement((object)networkPeer4, networkPeer4.UserName, (ImageIdentifier)null));
			}
			MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(((object)new TextObject("{=pbp0GQdO}Ban Player", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=jw2VQYeK}Select player to ban", (Dictionary<string, object>)null)).ToString(), list, true, 0, 1, ((object)new TextObject("{=HjqcmY6X}Ban", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null)).ToString(), (Action<List<InquiryElement>>)delegate(List<InquiryElement> selectedPlayers)
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Expected O, but got Unknown
				if (selectedPlayers != null && selectedPlayers.Count == 1)
				{
					NetworkCommunicator val = (NetworkCommunicator)selectedPlayers[0].Identifier;
					if (val != null)
					{
						_multiplayerAdminComponent.KickPlayer(val, banPlayer: true);
					}
				}
			}, (Action<List<InquiryElement>>)null, string.Empty, true), false, false);
		}));
		return adminPanelOptionGroup;
	}
}
