using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanChangeSigilPopupVM : ViewModel
{
	private MPLobbySigilItemVM _selectedSigilIcon;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isSelected;

	private bool _canChangeSigil;

	private string _titleText;

	private string _applyText;

	private MBBindingList<MPLobbySigilItemVM> _iconsList;

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
	public bool CanChangeSigil
	{
		get
		{
			return _canChangeSigil;
		}
		set
		{
			if (value != _canChangeSigil)
			{
				_canChangeSigil = value;
				((ViewModel)this).OnPropertyChanged("CanChangeSigil");
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
	public MBBindingList<MPLobbySigilItemVM> IconsList
	{
		get
		{
			return _iconsList;
		}
		set
		{
			if (value != _iconsList)
			{
				_iconsList = value;
				((ViewModel)this).OnPropertyChanged("IconsList");
			}
		}
	}

	public MPLobbyClanChangeSigilPopupVM()
	{
		PrepareSigilIconsList();
		CanChangeSigil = false;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=q7VcSSbp}Choose Sigil", (Dictionary<string, object>)null)).ToString();
		ApplyText = ((object)new TextObject("{=BAaS5Dkc}Apply", (Dictionary<string, object>)null)).ToString();
	}

	private void PrepareSigilIconsList()
	{
		IconsList = new MBBindingList<MPLobbySigilItemVM>();
		_selectedSigilIcon = null;
		foreach (BannerIconGroup item2 in (List<BannerIconGroup>)(object)BannerManager.Instance.BannerIconGroups)
		{
			if (item2.IsPattern)
			{
				continue;
			}
			foreach (KeyValuePair<int, BannerIconData> availableIcon in item2.AvailableIcons)
			{
				MPLobbySigilItemVM item = new MPLobbySigilItemVM(availableIcon.Key, OnSigilIconSelection);
				((Collection<MPLobbySigilItemVM>)(object)IconsList).Add(item);
			}
		}
	}

	private void OnSigilIconSelection(MPLobbySigilItemVM sigilIcon)
	{
		if (sigilIcon != _selectedSigilIcon)
		{
			if (_selectedSigilIcon != null)
			{
				_selectedSigilIcon.IsSelected = false;
			}
			_selectedSigilIcon = sigilIcon;
			if (_selectedSigilIcon != null)
			{
				_selectedSigilIcon.IsSelected = true;
				CanChangeSigil = true;
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

	public void ExecuteChangeSigil()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		BasicCultureObject val = Game.Current.ObjectManager.GetObject<BasicCultureObject>(NetworkMain.GameClient.ClanInfo.Faction);
		Banner val2 = new Banner(val.Banner, val.BackgroundColor1, val.ForegroundColor1);
		val2.SetIconMeshId(_selectedSigilIcon.IconID);
		NetworkMain.GameClient.ChangeClanSigil(val2.Serialize());
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
