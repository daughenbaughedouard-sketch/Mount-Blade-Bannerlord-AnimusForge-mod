using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class LoadingWindowViewModel : ViewModel
{
	public delegate void UnloadImageDelegate(int index);

	public delegate void LoadImageDelegate(int index, out string imageName);

	private int _currentImage;

	private int _totalGenericImageCount;

	private LoadImageDelegate _loadImageDelegate;

	private UnloadImageDelegate _unloadImageDelegate;

	private bool _enabled;

	private bool _isDevelopmentMode;

	private bool _isMultiplayer;

	private bool _isNavalDLCEnabled;

	private string _loadingImageName;

	private string _titleText;

	private string _descriptionText;

	private string _gameModeText;

	public bool CurrentlyShowingMultiplayer { get; private set; }

	[DataSourceProperty]
	public bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (_enabled != value)
			{
				_enabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Enabled");
				if (value)
				{
					HandleEnable();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsDevelopmentMode
	{
		get
		{
			return _isDevelopmentMode;
		}
		set
		{
			if (_isDevelopmentMode != value)
			{
				_isDevelopmentMode = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDevelopmentMode");
			}
		}
	}

	[DataSourceProperty]
	public string TitleText
	{
		get
		{
			return _titleText;
		}
		set
		{
			if (_titleText != value)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string GameModeText
	{
		get
		{
			return _gameModeText;
		}
		set
		{
			if (_gameModeText != value)
			{
				_gameModeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameModeText");
			}
		}
	}

	[DataSourceProperty]
	public string DescriptionText
	{
		get
		{
			return _descriptionText;
		}
		set
		{
			if (_descriptionText != value)
			{
				_descriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "DescriptionText");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMultiplayer
	{
		get
		{
			return _isMultiplayer;
		}
		set
		{
			if (_isMultiplayer != value)
			{
				_isMultiplayer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMultiplayer");
			}
		}
	}

	[DataSourceProperty]
	public bool IsNavalDLCEnabled
	{
		get
		{
			return _isNavalDLCEnabled;
		}
		set
		{
			if (_isNavalDLCEnabled != value)
			{
				_isNavalDLCEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsNavalDLCEnabled");
			}
		}
	}

	[DataSourceProperty]
	public string LoadingImageName
	{
		get
		{
			return _loadingImageName;
		}
		set
		{
			if (_loadingImageName != value)
			{
				_loadingImageName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LoadingImageName");
			}
		}
	}

	public LoadingWindowViewModel(LoadImageDelegate loadImageDelegate, UnloadImageDelegate unloadImageDelegate)
	{
		_unloadImageDelegate = unloadImageDelegate;
		_loadImageDelegate = loadImageDelegate;
		if (_loadImageDelegate != null)
		{
			_loadImageDelegate(_currentImage + 1, out var imageName);
			LoadingImageName = imageName;
		}
	}

	internal void Update()
	{
		if (Enabled)
		{
			bool flag = IsEligableForMultiplayerLoading();
			if (flag && !CurrentlyShowingMultiplayer)
			{
				SetForMultiplayer();
			}
			else if (!flag && CurrentlyShowingMultiplayer)
			{
				SetForEmpty();
			}
		}
	}

	private void HandleEnable()
	{
		if (IsEligableForMultiplayerLoading())
		{
			SetForMultiplayer();
		}
		else
		{
			SetForEmpty();
		}
	}

	private bool IsEligableForMultiplayerLoading()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (_isMultiplayer && Mission.Current != null && Game.Current.GameStateManager.ActiveState is MissionState && ((MissionState)Game.Current.GameStateManager.ActiveState).MissionName != "MultiplayerPractice")
		{
			return true;
		}
		return false;
	}

	private void SetForMultiplayer()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		MissionState val = (MissionState)Game.Current.GameStateManager.ActiveState;
		string text = val.MissionName switch
		{
			"MultiplayerTeamDeathmatch" => "TeamDeathmatch", 
			"MultiplayerSiege" => "Siege", 
			"MultiplayerBattle" => "Battle", 
			"MultiplayerCaptain" => "Captain", 
			"MultiplayerSkirmish" => "Skirmish", 
			"MultiplayerDuel" => "Duel", 
			_ => val.MissionName, 
		};
		if (!string.IsNullOrEmpty(text))
		{
			DescriptionText = ((object)GameTexts.FindText("str_multiplayer_official_game_type_explainer", text)).ToString();
		}
		else
		{
			DescriptionText = "";
		}
		GameModeText = ((object)GameTexts.FindText("str_multiplayer_official_game_type_name", text)).ToString();
		TextObject val2 = default(TextObject);
		if (GameTexts.TryGetText("str_multiplayer_scene_name", ref val2, val.CurrentMission.SceneName))
		{
			TitleText = ((object)val2).ToString();
		}
		else
		{
			TitleText = val.CurrentMission.SceneName;
		}
		LoadingImageName = val.CurrentMission.SceneName;
		CurrentlyShowingMultiplayer = true;
	}

	private void SetForEmpty()
	{
		DescriptionText = "";
		TitleText = "";
		GameModeText = "";
		SetNextGenericImage();
		CurrentlyShowingMultiplayer = false;
	}

	private void SetNextGenericImage()
	{
		int index = ((_currentImage >= 1) ? _currentImage : _totalGenericImageCount);
		_currentImage = ((_currentImage >= _totalGenericImageCount) ? 1 : (_currentImage + 1));
		int index2 = ((_currentImage >= _totalGenericImageCount) ? 1 : (_currentImage + 1));
		if (_unloadImageDelegate != null)
		{
			_unloadImageDelegate(index);
		}
		if (_loadImageDelegate != null)
		{
			_loadImageDelegate(index2, out var imageName);
			LoadingImageName = imageName;
		}
		else
		{
			LoadingImageName = string.Empty;
		}
		IsDevelopmentMode = NativeConfig.IsDevelopmentMode;
	}

	public void SetTotalGenericImageCount(int totalGenericImageCount)
	{
		_totalGenericImageCount = totalGenericImageCount;
	}
}
