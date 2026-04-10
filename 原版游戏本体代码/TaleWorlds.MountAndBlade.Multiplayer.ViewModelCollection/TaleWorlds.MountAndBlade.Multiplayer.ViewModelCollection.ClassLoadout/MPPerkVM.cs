using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class MPPerkVM : ViewModel
{
	public readonly IReadOnlyPerkObject Perk;

	private readonly Action<MPPerkVM> _onSelectPerk;

	private string _iconType;

	private string _name;

	private string _description;

	private bool _isSelectable;

	private HintViewModel _hint;

	public int PerkIndex { get; private set; }

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
	public HintViewModel Hint
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
				((ViewModel)this).OnPropertyChangedWithValue<HintViewModel>(value, "Hint");
			}
		}
	}

	[DataSourceProperty]
	public string Description
	{
		get
		{
			return _description;
		}
		set
		{
			if (value != _description)
			{
				_description = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Description");
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
	public bool IsSelectable
	{
		get
		{
			return _isSelectable;
		}
		set
		{
			if (value != _isSelectable)
			{
				_isSelectable = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelectable");
			}
		}
	}

	public MPPerkVM(Action<MPPerkVM> onSelectPerk, IReadOnlyPerkObject perk, bool isSelectable, int perkIndex)
	{
		Perk = perk;
		PerkIndex = perkIndex;
		_onSelectPerk = onSelectPerk;
		IconType = perk.IconId;
		IsSelectable = isSelectable;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		Name = ((object)Perk.Name).ToString();
		Description = ((object)Perk.Description).ToString();
		GameTexts.SetVariable("newline", "\n");
		Hint = new HintViewModel(Perk.Description, (string)null);
	}

	public void ExecuteSelectPerk()
	{
		_onSelectPerk(this);
	}
}
