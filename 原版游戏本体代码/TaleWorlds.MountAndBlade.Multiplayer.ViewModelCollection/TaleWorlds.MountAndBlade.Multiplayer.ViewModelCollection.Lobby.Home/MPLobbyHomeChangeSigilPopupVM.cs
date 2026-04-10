using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics.CosmeticTypes;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;

public class MPLobbyHomeChangeSigilPopupVM : ViewModel
{
	private class SigilItemUnlockStatusComparer : IComparer<MPLobbyCosmeticSigilItemVM>
	{
		public int Compare(MPLobbyCosmeticSigilItemVM x, MPLobbyCosmeticSigilItemVM y)
		{
			return y.IsUnlocked.CompareTo(x.IsUnlocked);
		}
	}

	private readonly Action<MPLobbyCosmeticSigilItemVM> _onItemObtainRequested;

	private InputKeyItemVM _cancelInputKey;

	private InputKeyItemVM _doneInputKey;

	private bool _isEnabled;

	private bool _isLoading;

	private bool _isInClan;

	private bool _isUsingClanSigil;

	private string _titleText;

	private string _changeText;

	private string _cancelText;

	private int _loot;

	private MBBindingList<MPLobbyCosmeticSigilItemVM> _sigilList;

	public MPLobbyCosmeticSigilItemVM SelectedSigil { get; private set; }

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
				OnIsEnabledChanged();
			}
		}
	}

	[DataSourceProperty]
	public bool IsLoading
	{
		get
		{
			return _isLoading;
		}
		set
		{
			if (value != _isLoading)
			{
				_isLoading = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsLoading");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInClan
	{
		get
		{
			return _isInClan;
		}
		set
		{
			if (value != _isInClan)
			{
				_isInClan = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInClan");
			}
		}
	}

	[DataSourceProperty]
	public bool IsUsingClanSigil
	{
		get
		{
			return _isUsingClanSigil;
		}
		set
		{
			if (value != _isUsingClanSigil)
			{
				_isUsingClanSigil = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUsingClanSigil");
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
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TitleText");
			}
		}
	}

	[DataSourceProperty]
	public string ChangeText
	{
		get
		{
			return _changeText;
		}
		set
		{
			if (value != _changeText)
			{
				_changeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ChangeText");
			}
		}
	}

	[DataSourceProperty]
	public string CancelText
	{
		get
		{
			return _cancelText;
		}
		set
		{
			if (value != _cancelText)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	[DataSourceProperty]
	public int Loot
	{
		get
		{
			return _loot;
		}
		set
		{
			if (value != _loot)
			{
				_loot = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Loot");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPLobbyCosmeticSigilItemVM> SigilList
	{
		get
		{
			return _sigilList;
		}
		set
		{
			if (value != _sigilList)
			{
				_sigilList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPLobbyCosmeticSigilItemVM>>(value, "SigilList");
			}
		}
	}

	public MPLobbyHomeChangeSigilPopupVM(Action<MPLobbyCosmeticSigilItemVM> onItemObtainRequested)
	{
		_onItemObtainRequested = onItemObtainRequested;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TitleText = ((object)new TextObject("{=7R0i82Nw}Change Sigil", (Dictionary<string, object>)null)).ToString();
		ChangeText = ((object)new TextObject("{=Ba50zU7Z}Change", (Dictionary<string, object>)null)).ToString();
		CancelText = ((object)GameTexts.FindText("str_cancel", (string)null)).ToString();
	}

	private void RefreshSigilList()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected I4, but got Unknown
		SigilList = new MBBindingList<MPLobbyCosmeticSigilItemVM>();
		SelectedSigil = null;
		MBReadOnlyList<CosmeticElement> cosmeticElementsList = CosmeticsManager.CosmeticElementsList;
		IReadOnlyList<string> ownedCosmetics = NetworkMain.GameClient.OwnedCosmetics;
		for (int i = 0; i < ((List<CosmeticElement>)(object)cosmeticElementsList).Count; i++)
		{
			if ((int)((List<CosmeticElement>)(object)cosmeticElementsList)[i].Type == 2)
			{
				CosmeticElement obj = ((List<CosmeticElement>)(object)cosmeticElementsList)[i];
				SigilCosmeticElement val = (SigilCosmeticElement)(object)((obj is SigilCosmeticElement) ? obj : null);
				MPLobbyCosmeticSigilItemVM mPLobbyCosmeticSigilItemVM = new MPLobbyCosmeticSigilItemVM(new Banner(val.BannerCode).GetIconMeshId(), (int)((CosmeticElement)val).Rarity, ((CosmeticElement)val).Cost, ((CosmeticElement)val).Id);
				mPLobbyCosmeticSigilItemVM.IsUnlocked = ownedCosmetics.Contains(((CosmeticElement)val).Id) || ((CosmeticElement)val).IsFree;
				((Collection<MPLobbyCosmeticSigilItemVM>)(object)SigilList).Add(mPLobbyCosmeticSigilItemVM);
			}
		}
		IsUsingClanSigil = NetworkMain.GameClient.PlayerData.IsUsingClanSigil;
		SelectPlayerSigil(NetworkMain.GameClient.PlayerData);
		Loot = NetworkMain.GameClient.PlayerData.Gold;
		SigilList.Sort((IComparer<MPLobbyCosmeticSigilItemVM>)new SigilItemUnlockStatusComparer());
	}

	private void SelectPlayerSigil(PlayerData playerData)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		int playerBannerID = new Banner(playerData.Sigil).GetIconMeshId();
		OnSigilSelected(((IEnumerable<MPLobbyCosmeticSigilItemVM>)SigilList).First((MPLobbyCosmeticSigilItemVM s) => s.IconID == playerBannerID));
	}

	public void Open()
	{
		IsInClan = NetworkMain.GameClient.IsInClan;
		IsEnabled = true;
	}

	public void ExecuteClosePopup()
	{
		IsEnabled = false;
	}

	public void ExecuteChangeSigil()
	{
		NetworkMain.GameClient.ChangeSigil(SelectedSigil.CosmeticID);
		NetworkMain.GameClient.PlayerData.IsUsingClanSigil = IsUsingClanSigil;
		IsEnabled = false;
	}

	private void OnSigilObtainRequested(MPLobbyCosmeticSigilItemVM sigilItem)
	{
		_onItemObtainRequested(sigilItem);
	}

	private void OnSigilSelected(MPLobbyCosmeticSigilItemVM sigilItem)
	{
		if (sigilItem != SelectedSigil)
		{
			if (SelectedSigil != null)
			{
				SelectedSigil.IsUsed = false;
			}
			SelectedSigil = sigilItem;
			if (SelectedSigil != null)
			{
				SelectedSigil.IsUsed = true;
			}
		}
	}

	public void OnLootUpdated(int finalLoot)
	{
		Loot = finalLoot;
	}

	private void OnIsEnabledChanged()
	{
		if (IsEnabled)
		{
			RefreshSigilList();
			MPLobbyCosmeticSigilItemVM.SetOnObtainRequestedCallback(OnSigilObtainRequested);
			MPLobbyCosmeticSigilItemVM.SetOnSelectionCallback(OnSigilSelected);
		}
		else
		{
			MPLobbyCosmeticSigilItemVM.ResetOnObtainRequestedCallback();
			MPLobbyCosmeticSigilItemVM.ResetOnSelectionCallback();
		}
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
