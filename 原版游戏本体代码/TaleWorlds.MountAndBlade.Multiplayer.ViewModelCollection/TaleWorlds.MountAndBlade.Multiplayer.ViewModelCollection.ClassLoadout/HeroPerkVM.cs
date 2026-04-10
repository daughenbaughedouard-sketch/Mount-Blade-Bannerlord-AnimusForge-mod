using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class HeroPerkVM : ViewModel
{
	private readonly Action<HeroPerkVM, MPPerkVM> _onSelectPerk;

	private string _name = "";

	private string _iconType;

	private BasicTooltipViewModel _hint;

	private MBBindingList<MPPerkVM> _candidatePerks;

	public IReadOnlyPerkObject SelectedPerk { get; private set; }

	public MPPerkVM SelectedPerkItem { get; private set; }

	public int PerkIndex { get; }

	[DataSourceProperty]
	public MBBindingList<MPPerkVM> CandidatePerks
	{
		get
		{
			return _candidatePerks;
		}
		set
		{
			if (value != _candidatePerks)
			{
				_candidatePerks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPPerkVM>>(value, "CandidatePerks");
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
	public string IconType
	{
		get
		{
			return _iconType;
		}
		set
		{
			if (value != _iconType)
			{
				_iconType = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "IconType");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel Hint
	{
		get
		{
			return _hint;
		}
		set
		{
			if (value != _hint)
			{
				_hint = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "Hint");
			}
		}
	}

	public HeroPerkVM(Action<HeroPerkVM, MPPerkVM> onSelectPerk, IReadOnlyPerkObject perk, List<IReadOnlyPerkObject> candidatePerks, int perkIndex)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		HeroPerkVM heroPerkVM = this;
		Hint = new BasicTooltipViewModel((Func<string>)(() => heroPerkVM.SelectedPerkItem.Description));
		CandidatePerks = new MBBindingList<MPPerkVM>();
		PerkIndex = perkIndex;
		_onSelectPerk = onSelectPerk;
		for (int num = 0; num < candidatePerks.Count; num++)
		{
			IReadOnlyPerkObject val = candidatePerks[num];
			bool isSelectable = val != perk;
			((Collection<MPPerkVM>)(object)CandidatePerks).Add(new MPPerkVM(OnSelectPerk, val, isSelectable, num));
		}
		OnSelectPerk(((IEnumerable<MPPerkVM>)CandidatePerks).SingleOrDefault((MPPerkVM x) => x.Perk == perk));
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		Name = SelectedPerkItem.Name;
		CandidatePerks.ApplyActionOnAllItems((Action<MPPerkVM>)delegate(MPPerkVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	[UsedImplicitly]
	private void OnSelectPerk(MPPerkVM perkVm)
	{
		OnRefreshWithPerk(perkVm);
		foreach (MPPerkVM item in (Collection<MPPerkVM>)(object)CandidatePerks)
		{
			item.IsSelectable = true;
		}
		perkVm.IsSelectable = false;
		_onSelectPerk(this, perkVm);
	}

	private void OnRefreshWithPerk(MPPerkVM perk)
	{
		SelectedPerkItem = perk;
		SelectedPerk = SelectedPerkItem?.Perk;
		if (perk == null)
		{
			Name = "";
			IconType = "";
		}
		else
		{
			IconType = perk.IconType;
			((ViewModel)this).RefreshValues();
		}
	}
}
