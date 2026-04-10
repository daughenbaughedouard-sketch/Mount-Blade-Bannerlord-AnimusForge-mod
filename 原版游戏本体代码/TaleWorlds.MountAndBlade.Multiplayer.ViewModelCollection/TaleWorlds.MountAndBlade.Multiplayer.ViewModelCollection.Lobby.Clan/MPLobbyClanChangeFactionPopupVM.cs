using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanChangeFactionPopupVM : ViewModel
{
	private MPCultureItemVM _selectedFaction;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isSelected;

	private bool _canChangeFaction;

	private string _titleText;

	private string _applyText;

	private MBBindingList<MPCultureItemVM> _factionsList;

	[DataSourceProperty]
	public InputKeyItemVM CancelInputKey
	{
		get
		{
			return _cancelInputKey;
		}
		set
		{
			if (value != _cancelInputKey)
			{
				_cancelInputKey = value;
				((ViewModel)this).OnPropertyChanged("CancelInputKey");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM DoneInputKey
	{
		get
		{
			return _doneInputKey;
		}
		set
		{
			if (value != _doneInputKey)
			{
				_doneInputKey = value;
				((ViewModel)this).OnPropertyChanged("DoneInputKey");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChanged("IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool CanChangeFaction
	{
		get
		{
			return _canChangeFaction;
		}
		set
		{
			if (value != _canChangeFaction)
			{
				_canChangeFaction = value;
				((ViewModel)this).OnPropertyChanged("CanChangeFaction");
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
			if (value != _titleText)
			{
				_titleText = value;
				((ViewModel)this).OnPropertyChanged("TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string ApplyText
	{
		get
		{
			return _applyText;
		}
		set
		{
			if (value != _applyText)
			{
				_applyText = value;
				((ViewModel)this).OnPropertyChanged("ApplyText");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPCultureItemVM> FactionsList
	{
		get
		{
			return _factionsList;
		}
		set
		{
			if (value != _factionsList)
			{
				_factionsList = value;
				((ViewModel)this).OnPropertyChanged("FactionsList");
			}
		}
	}

	public MPLobbyClanChangeFactionPopupVM()
	{
		PrepareFactionsList();
		CanChangeFaction = false;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=ghjSIyIL}Choose Culture", (Dictionary<string, object>)null)).ToString();
		ApplyText = ((object)new TextObject("{=BAaS5Dkc}Apply", (Dictionary<string, object>)null)).ToString();
	}

	private void PrepareFactionsList()
	{
		_selectedFaction = null;
		MBBindingList<MPCultureItemVM> obj = new MBBindingList<MPCultureItemVM>();
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("vlandia")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("sturgia")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("battania")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("khuzait")).StringId, OnFactionSelection));
		((Collection<MPCultureItemVM>)(object)obj).Add(new MPCultureItemVM(((MBObjectBase)Game.Current.ObjectManager.GetObject<BasicCultureObject>("aserai")).StringId, OnFactionSelection));
		FactionsList = obj;
	}

	private void OnFactionSelection(MPCultureItemVM faction)
	{
		if (faction != _selectedFaction)
		{
			if (_selectedFaction != null)
			{
				_selectedFaction.IsSelected = false;
			}
			_selectedFaction = faction;
			if (_selectedFaction != null)
			{
				_selectedFaction.IsSelected = true;
				CanChangeFaction = true;
			}
		}
	}

	public void ExecuteOpenPopup()
	{
		IsSelected = true;
	}

	public void ExecuteClosePopup()
	{
		IsSelected = false;
	}

	public void ExecuteChangeFaction()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		BasicCultureObject val = Game.Current.ObjectManager.GetObject<BasicCultureObject>(_selectedFaction.CultureCode);
		Banner val2 = new Banner(NetworkMain.GameClient.ClanInfo.Sigil);
		val2.ChangeIconColors(val.ForegroundColor1);
		val2.ChangePrimaryColor(val.BackgroundColor1);
		NetworkMain.GameClient.ChangeClanSigil(val2.Serialize());
		NetworkMain.GameClient.ChangeClanFaction(_selectedFaction.CultureCode);
		ExecuteClosePopup();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		InputKeyItemVM cancelInputKey = CancelInputKey;
		if (cancelInputKey != null)
		{
			((ViewModel)cancelInputKey).OnFinalize();
		}
		InputKeyItemVM doneInputKey = DoneInputKey;
		if (doneInputKey != null)
		{
			((ViewModel)doneInputKey).OnFinalize();
		}
	}

	public void SetCancelInputKey(HotKey hotKey)
	{
		CancelInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}

	public void SetDoneInputKey(HotKey hotKey)
	{
		DoneInputKey = InputKeyItemVM.CreateFromHotKey(hotKey, true);
	}
}
