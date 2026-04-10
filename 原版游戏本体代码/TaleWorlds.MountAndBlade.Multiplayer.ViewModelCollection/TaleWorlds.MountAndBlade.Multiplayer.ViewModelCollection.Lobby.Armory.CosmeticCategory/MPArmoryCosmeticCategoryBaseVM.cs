using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond.Cosmetics;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticItem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory.CosmeticCategory;

public abstract class MPArmoryCosmeticCategoryBaseVM : ViewModel
{
	public readonly CosmeticType CosmeticType;

	private string _cosmeticTypeName;

	private string _cosmeticCategoryName;

	private bool _isSelected;

	private MBBindingList<MPArmoryCosmeticItemBaseVM> _availableCosmetics;

	[DataSourceProperty]
	public string CosmeticTypeName
	{
		get
		{
			return _cosmeticTypeName;
		}
		set
		{
			if (value != _cosmeticTypeName)
			{
				_cosmeticTypeName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CosmeticTypeName");
			}
		}
	}

	[DataSourceProperty]
	public string CosmeticCategoryName
	{
		get
		{
			return _cosmeticCategoryName;
		}
		set
		{
			if (value != _cosmeticCategoryName)
			{
				_cosmeticCategoryName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CosmeticCategoryName");
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
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPArmoryCosmeticItemBaseVM> AvailableCosmetics
	{
		get
		{
			return _availableCosmetics;
		}
		set
		{
			if (value != _availableCosmetics)
			{
				_availableCosmetics = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPArmoryCosmeticItemBaseVM>>(value, "AvailableCosmetics");
			}
		}
	}

	public unsafe MPArmoryCosmeticCategoryBaseVM(CosmeticType cosmeticType)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		AvailableCosmetics = new MBBindingList<MPArmoryCosmeticItemBaseVM>();
		CosmeticType = cosmeticType;
		CosmeticTypeName = ((object)(*(CosmeticType*)(&cosmeticType))/*cast due to .constrained prefix*/).ToString();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		AvailableCosmetics.ApplyActionOnAllItems((Action<MPArmoryCosmeticItemBaseVM>)delegate(MPArmoryCosmeticItemBaseVM c)
		{
			((ViewModel)c).RefreshValues();
		});
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		AvailableCosmetics.ApplyActionOnAllItems((Action<MPArmoryCosmeticItemBaseVM>)delegate(MPArmoryCosmeticItemBaseVM c)
		{
			((ViewModel)c).OnFinalize();
		});
	}

	protected abstract void ExecuteSelectCategory();

	public void Sort(MPArmoryCosmeticsVM.CosmeticItemComparer comparer)
	{
		AvailableCosmetics.Sort((IComparer<MPArmoryCosmeticItemBaseVM>)comparer);
	}
}
