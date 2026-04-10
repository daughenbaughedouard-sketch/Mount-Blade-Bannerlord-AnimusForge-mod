using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Tournament;

public class TournamentParticipantVM : ViewModel
{
	public enum TournamentPlayerState
	{
		EmptyPlayer,
		GenericPlayer,
		MainPlayer
	}

	private TournamentParticipant _latestParticipant;

	private bool _isInitialized;

	private bool _isValid;

	private string _name = "";

	private string _score = "-";

	private bool _isQualifiedForNextRound;

	private int _state = -1;

	private CharacterImageIdentifierVM _visual;

	private Color _teamColor;

	private bool _isDead;

	private bool _isMainHero;

	private CharacterViewModel _character;

	public TournamentParticipant Participant { get; private set; }

	[DataSourceProperty]
	public bool IsInitialized
	{
		get
		{
			return _isInitialized;
		}
		set
		{
			if (value != _isInitialized)
			{
				_isInitialized = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInitialized");
			}
		}
	}

	[DataSourceProperty]
	public bool IsValid
	{
		get
		{
			return _isValid;
		}
		set
		{
			if (value != _isValid)
			{
				_isValid = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsValid");
			}
		}
	}

	[DataSourceProperty]
	public bool IsDead
	{
		get
		{
			return _isDead;
		}
		set
		{
			if (value != _isDead)
			{
				_isDead = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDead");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMainHero
	{
		get
		{
			return _isMainHero;
		}
		set
		{
			if (value != _isMainHero)
			{
				_isMainHero = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMainHero");
			}
		}
	}

	[DataSourceProperty]
	public Color TeamColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _teamColor;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (value != _teamColor)
			{
				_teamColor = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TeamColor");
			}
		}
	}

	[DataSourceProperty]
	public CharacterImageIdentifierVM Visual
	{
		get
		{
			return _visual;
		}
		set
		{
			if (value != _visual)
			{
				_visual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterImageIdentifierVM>(value, "Visual");
			}
		}
	}

	[DataSourceProperty]
	public int State
	{
		get
		{
			return _state;
		}
		set
		{
			if (value != _state)
			{
				_state = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "State");
			}
		}
	}

	[DataSourceProperty]
	public bool IsQualifiedForNextRound
	{
		get
		{
			return _isQualifiedForNextRound;
		}
		set
		{
			if (value != _isQualifiedForNextRound)
			{
				_isQualifiedForNextRound = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsQualifiedForNextRound");
			}
		}
	}

	[DataSourceProperty]
	public string Score
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Score");
			}
		}
	}

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
	public CharacterViewModel Character
	{
		get
		{
			return _character;
		}
		set
		{
			if (value != _character)
			{
				_character = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterViewModel>(value, "Character");
			}
		}
	}

	public TournamentParticipantVM()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		_visual = new CharacterImageIdentifierVM((CharacterCode)null);
		_character = new CharacterViewModel((StanceTypes)3);
	}

	public override void RefreshValues()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		if (IsInitialized)
		{
			Refresh(Participant, TeamColor);
		}
	}

	public void Refresh(TournamentParticipant participant, Color teamColor)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		Participant = participant;
		TeamColor = teamColor;
		State = ((participant != null) ? ((participant.Character != CharacterObject.PlayerCharacter) ? 1 : 2) : 0);
		IsInitialized = true;
		_latestParticipant = participant;
		if (participant != null)
		{
			Name = ((object)((BasicCharacterObject)participant.Character).Name).ToString();
			CharacterCode characterCode = SandBoxUIHelper.GetCharacterCode(participant.Character);
			Character = new CharacterViewModel((StanceTypes)3);
			Character.FillFrom((BasicCharacterObject)(object)participant.Character, -1, (string)null);
			Visual = new CharacterImageIdentifierVM(characterCode);
			IsValid = true;
			IsMainHero = ((BasicCharacterObject)participant.Character).IsPlayerCharacter;
		}
	}

	public void ExecuteOpenEncyclopedia()
	{
		TournamentParticipant participant = Participant;
		if (((participant != null) ? participant.Character : null) != null)
		{
			Campaign.Current.EncyclopediaManager.GoToLink(Participant.Character.EncyclopediaLink);
		}
	}

	public void Refresh()
	{
		((ViewModel)this).OnPropertyChanged("Name");
		((ViewModel)this).OnPropertyChanged("Visual");
		((ViewModel)this).OnPropertyChanged("Score");
		((ViewModel)this).OnPropertyChanged("State");
		((ViewModel)this).OnPropertyChanged("TeamColor");
		((ViewModel)this).OnPropertyChanged("IsDead");
		TournamentParticipant latestParticipant = _latestParticipant;
		IsMainHero = latestParticipant != null && ((BasicCharacterObject)latestParticipant.Character).IsPlayerCharacter;
	}
}
