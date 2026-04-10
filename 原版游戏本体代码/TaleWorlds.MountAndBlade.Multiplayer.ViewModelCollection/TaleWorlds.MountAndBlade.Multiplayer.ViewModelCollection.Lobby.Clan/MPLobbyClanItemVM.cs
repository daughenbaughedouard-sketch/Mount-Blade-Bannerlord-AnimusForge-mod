using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanItemVM : ViewModel
{
	private string _name;

	private string _tag;

	private string _sigilCode;

	private string _nameWithTag;

	private int _memberCount;

	private int _gamesWon;

	private int _gamesLost;

	private int _ranking;

	private bool _isOwnClan;

	private BannerImageIdentifierVM _sigilImage;

	[DataSourceProperty]
	public string NameWithTag
	{
		get
		{
			return _nameWithTag;
		}
		set
		{
			if (value != _nameWithTag)
			{
				_nameWithTag = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NameWithTag");
			}
		}
	}

	[DataSourceProperty]
	public int MemberCount
	{
		get
		{
			return _memberCount;
		}
		set
		{
			if (value != _memberCount)
			{
				_memberCount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "MemberCount");
			}
		}
	}

	[DataSourceProperty]
	public int GamesWon
	{
		get
		{
			return _gamesWon;
		}
		set
		{
			if (value != _gamesWon)
			{
				_gamesWon = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GamesWon");
			}
		}
	}

	[DataSourceProperty]
	public int GamesLost
	{
		get
		{
			return _gamesLost;
		}
		set
		{
			if (value != _gamesLost)
			{
				_gamesLost = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GamesLost");
			}
		}
	}

	[DataSourceProperty]
	public int Ranking
	{
		get
		{
			return _ranking;
		}
		set
		{
			if (value != _ranking)
			{
				_ranking = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Ranking");
			}
		}
	}

	[DataSourceProperty]
	public bool IsOwnClan
	{
		get
		{
			return _isOwnClan;
		}
		set
		{
			if (value != _isOwnClan)
			{
				_isOwnClan = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsOwnClan");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM SigilImage
	{
		get
		{
			return _sigilImage;
		}
		set
		{
			if (value != _sigilImage)
			{
				_sigilImage = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "SigilImage");
			}
		}
	}

	public MPLobbyClanItemVM(string name, string tag, string sigilCode, int gamesWon, int gamesLost, int ranking, bool isOwnClan)
	{
		_name = name;
		_tag = tag;
		_sigilCode = sigilCode;
		GamesWon = gamesWon;
		GamesLost = gamesLost;
		Ranking = ranking;
		IsOwnClan = isOwnClan;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		SigilImage = new BannerImageIdentifierVM(new Banner(_sigilCode), false);
		GameTexts.SetVariable("STR", _tag);
		string text = ((object)new TextObject("{=uTXYEAOg}[{STR}]", (Dictionary<string, object>)null)).ToString();
		GameTexts.SetVariable("STR1", _name);
		GameTexts.SetVariable("STR2", text);
		NameWithTag = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
	}
}
