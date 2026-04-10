using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

public class MPEndOfBattleSideVM : ViewModel
{
	private MissionScoreboardComponent _missionScoreboardComponent;

	private BasicCultureObject _culture;

	private string _factionName;

	private string _cultureId;

	private int _score;

	private bool _isRoundWinner;

	private Color _cultureColor1;

	private Color _cultureColor2;

	public MissionScoreboardSide Side { get; private set; }

	[DataSourceProperty]
	public string FactionName
	{
		get
		{
			return _factionName;
		}
		set
		{
			if (value != _factionName)
			{
				_factionName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "FactionName");
			}
		}
	}

	[DataSourceProperty]
	public string CultureId
	{
		get
		{
			return _cultureId;
		}
		set
		{
			if (value != _cultureId)
			{
				_cultureId = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CultureId");
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

	[DataSourceProperty]
	public bool IsRoundWinner
	{
		get
		{
			return _isRoundWinner;
		}
		set
		{
			if (value != _isRoundWinner)
			{
				_isRoundWinner = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsRoundWinner");
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

	public MPEndOfBattleSideVM(MissionScoreboardComponent missionScoreboardComponent, MissionScoreboardSide side, MultiplayerCultureColorInfo cultureColorInfo)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		_missionScoreboardComponent = missionScoreboardComponent;
		Side = side;
		_culture = cultureColorInfo.Culture;
		if (Side != null)
		{
			CultureId = ((MBObjectBase)_culture).StringId;
			Score = Side.SideScore;
			IsRoundWinner = _missionScoreboardComponent.RoundWinner == side.Side || (int)_missionScoreboardComponent.RoundWinner == -1;
		}
		CultureColor1 = cultureColorInfo.Color1;
		CultureColor2 = cultureColorInfo.Color2;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		if (Side != null)
		{
			CultureId = ((MBObjectBase)_culture).StringId;
		}
	}
}
