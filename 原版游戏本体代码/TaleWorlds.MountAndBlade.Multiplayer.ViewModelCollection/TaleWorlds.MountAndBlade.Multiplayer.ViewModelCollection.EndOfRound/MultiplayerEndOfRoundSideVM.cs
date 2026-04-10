using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.EndOfRound;

public class MultiplayerEndOfRoundSideVM : ViewModel
{
	private BasicCultureObject _culture;

	private bool _isWinner;

	private string _cultureID;

	private Color _cultureColor1;

	private Color _cultureColor2;

	private string _cultureName;

	private int _score;

	[DataSourceProperty]
	public bool IsWinner
	{
		get
		{
			return _isWinner;
		}
		set
		{
			if (value != _isWinner)
			{
				_isWinner = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsWinner");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor1
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor1;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor1)
			{
				_cultureColor1 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor1");
			}
		}
	}

	[DataSourceProperty]
	public Color CultureColor2
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _cultureColor2;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _cultureColor2)
			{
				_cultureColor2 = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "CultureColor2");
			}
		}
	}

	[DataSourceProperty]
	public string CultureID
	{
		get
		{
			return _cultureID;
		}
		set
		{
			if (value != _cultureID)
			{
				_cultureID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureID");
			}
		}
	}

	[DataSourceProperty]
	public string CultureName
	{
		get
		{
			return _cultureName;
		}
		set
		{
			if (value != _cultureName)
			{
				_cultureName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureName");
			}
		}
	}

	[DataSourceProperty]
	public int Score
	{
		get
		{
			return _score;
		}
		set
		{
			if (value != _score)
			{
				_score = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Score");
			}
		}
	}

	public void SetData(BasicCultureObject culture, int score, bool isWinner, MultiplayerCultureColorInfo cultureColors)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_culture = culture;
		CultureID = ((MBObjectBase)culture).StringId;
		Score = score;
		IsWinner = isWinner;
		CultureColor1 = cultureColors.Color1;
		CultureColor2 = cultureColors.Color2;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		CultureName = ((object)_culture.Name).ToString();
	}
}
