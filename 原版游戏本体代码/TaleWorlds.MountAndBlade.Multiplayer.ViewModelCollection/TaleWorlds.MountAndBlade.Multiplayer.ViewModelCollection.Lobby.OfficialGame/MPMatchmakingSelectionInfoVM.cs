using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.OfficialGame;

public class MPMatchmakingSelectionInfoVM : ViewModel
{
	private string _playersDescription;

	private string _averagePlaytimeDescription;

	private string _roundsDescription;

	private string _roundTimeDescription;

	private string _objectivesDescription;

	private string _troopsDescription;

	private string _name;

	private string _description;

	private bool _isEnabled;

	private MBBindingList<StringPairItemVM> _extraInfos;

	[DataSourceProperty]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			if (value != _name)
			{
				_name = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Name");
			}
		}
	}

	[DataSourceProperty]
	public bool IsEnabled
	{
		get
		{
			return _isEnabled;
		}
		set
		{
			if (value != _isEnabled)
			{
				_isEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Description");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemVM> ExtraInfos
	{
		get
		{
			return _extraInfos;
		}
		set
		{
			if (value != _extraInfos)
			{
				_extraInfos = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemVM>>(value, "ExtraInfos");
			}
		}
	}

	public MPMatchmakingSelectionInfoVM()
	{
		Name = "";
		Description = "";
		ExtraInfos = new MBBindingList<StringPairItemVM>();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		_playersDescription = ((object)new TextObject("{=RfXJdNye}Players", (Dictionary<string, object>)null)).ToString();
		_averagePlaytimeDescription = ((object)new TextObject("{=YAaAlbkX}Avg. Playtime", (Dictionary<string, object>)null)).ToString();
		_roundsDescription = ((object)new TextObject("{=iKtIhlbo}Rounds", (Dictionary<string, object>)null)).ToString();
		_roundTimeDescription = ((object)new TextObject("{=r5WzivPb}Round Time", (Dictionary<string, object>)null)).ToString();
		_objectivesDescription = ((object)new TextObject("{=gqNxq11A}Objectives", (Dictionary<string, object>)null)).ToString();
		_troopsDescription = ((object)new TextObject("{=5k4dxUEJ}Troops", (Dictionary<string, object>)null)).ToString();
	}

	public void UpdateForGameType(string gameTypeStr)
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		Name = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", gameTypeStr)).ToString();
		MBTextManager.SetTextVariable("newline", "\n", false);
		Description = ((object)GameTexts.FindText("str_multiplayer_official_game_type_description", gameTypeStr)).ToString();
		((Collection<StringPairItemVM>)(object)ExtraInfos).Clear();
		int num = MultiplayerOptions.Instance.GetNumberOfPlayersForGameMode(gameTypeStr) / 2;
		int roundCountForGameMode = MultiplayerOptions.Instance.GetRoundCountForGameMode(gameTypeStr);
		int roundTimeLimitInMinutesForGameMode = MultiplayerOptions.Instance.GetRoundTimeLimitInMinutesForGameMode(gameTypeStr);
		int num2 = ((roundCountForGameMode == 1) ? 1 : (roundCountForGameMode / 2 + 1));
		int num3 = num2 * roundTimeLimitInMinutesForGameMode;
		MBTextManager.SetTextVariable("PLAYER_COUNT", num.ToString(), false);
		string text = ((object)GameTexts.FindText("str_multiplayer_official_game_type_player_info_for_versus", (string)null)).ToString();
		MBTextManager.SetTextVariable("PLAY_TIME", num3.ToString(), false);
		string text2 = ((object)GameTexts.FindText("str_multiplayer_official_game_type_playtime_info_in_minutes", (string)null)).ToString();
		MBTextManager.SetTextVariable("ROUND_COUNT", num2.ToString(), false);
		string text3 = ((object)GameTexts.FindText("str_multiplayer_official_game_type_rounds_info_for_best_of", (string)null)).ToString();
		MBTextManager.SetTextVariable("PLAY_TIME", roundTimeLimitInMinutesForGameMode.ToString(), false);
		string text4 = ((object)GameTexts.FindText("str_multiplayer_official_game_type_playtime_info_in_minutes", (string)null)).ToString();
		string text5 = ((object)GameTexts.FindText("str_multiplayer_official_game_type_objective_info", gameTypeStr)).ToString();
		string text6 = ((object)GameTexts.FindText("str_multiplayer_official_game_type_troops_info", gameTypeStr)).ToString();
		((Collection<StringPairItemVM>)(object)ExtraInfos).Add(new StringPairItemVM(_playersDescription, text, (BasicTooltipViewModel)null));
		((Collection<StringPairItemVM>)(object)ExtraInfos).Add(new StringPairItemVM(_averagePlaytimeDescription, text2, (BasicTooltipViewModel)null));
		((Collection<StringPairItemVM>)(object)ExtraInfos).Add(new StringPairItemVM(_roundsDescription, text3, (BasicTooltipViewModel)null));
		((Collection<StringPairItemVM>)(object)ExtraInfos).Add(new StringPairItemVM(_roundTimeDescription, text4, (BasicTooltipViewModel)null));
		((Collection<StringPairItemVM>)(object)ExtraInfos).Add(new StringPairItemVM(_objectivesDescription, text5, (BasicTooltipViewModel)null));
		((Collection<StringPairItemVM>)(object)ExtraInfos).Add(new StringPairItemVM(_troopsDescription, text6, (BasicTooltipViewModel)null));
	}

	public void SetEnabled(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}
}
