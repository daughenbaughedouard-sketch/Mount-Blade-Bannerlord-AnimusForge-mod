using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Tutorial;

public class TutorialVM : ViewModel
{
	private const float TutorialDelayInSeconds = 0f;

	private TutorialItemVM _currentTutorialItem;

	private Action _onTutorialDisabled;

	private bool _isVisible;

	private TutorialItemVM _leftItem;

	private TutorialItemVM _rightItem;

	private TutorialItemVM _bottomItem;

	private TutorialItemVM _topItem;

	private TutorialItemVM _leftBottomItem;

	private TutorialItemVM _leftTopItem;

	private TutorialItemVM _rightBottomItem;

	private TutorialItemVM _rightTopItem;

	private TutorialItemVM _centerItem;

	public static TutorialVM Instance { get; set; }

	[DataSourceProperty]
	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		set
		{
			if (value != _isVisible)
			{
				_isVisible = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVisible");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM LeftItem
	{
		get
		{
			return _leftItem;
		}
		set
		{
			if (value != _leftItem)
			{
				_leftItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "LeftItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM RightItem
	{
		get
		{
			return _rightItem;
		}
		set
		{
			if (value != _rightItem)
			{
				_rightItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "RightItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM BottomItem
	{
		get
		{
			return _bottomItem;
		}
		set
		{
			if (value != _bottomItem)
			{
				_bottomItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "BottomItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM TopItem
	{
		get
		{
			return _topItem;
		}
		set
		{
			if (value != _topItem)
			{
				_topItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "TopItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM LeftBottomItem
	{
		get
		{
			return _leftBottomItem;
		}
		set
		{
			if (value != _leftBottomItem)
			{
				_leftBottomItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "LeftBottomItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM LeftTopItem
	{
		get
		{
			return _leftTopItem;
		}
		set
		{
			if (value != _leftTopItem)
			{
				_leftTopItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "LeftTopItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM RightBottomItem
	{
		get
		{
			return _rightBottomItem;
		}
		set
		{
			if (value != _rightBottomItem)
			{
				_rightBottomItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "RightBottomItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM RightTopItem
	{
		get
		{
			return _rightTopItem;
		}
		set
		{
			if (value != _rightTopItem)
			{
				_rightTopItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "RightTopItem");
			}
		}
	}

	[DataSourceProperty]
	public TutorialItemVM CenterItem
	{
		get
		{
			return _centerItem;
		}
		set
		{
			if (value != _centerItem)
			{
				_centerItem = value;
				((ViewModel)this).OnPropertyChangedWithValue<TutorialItemVM>(value, "CenterItem");
			}
		}
	}

	public TutorialVM(Action onTutorialDisabled)
	{
		Instance = this;
		_onTutorialDisabled = onTutorialDisabled;
		LeftItem = new TutorialItemVM();
		RightItem = new TutorialItemVM();
		BottomItem = new TutorialItemVM();
		TopItem = new TutorialItemVM();
		LeftBottomItem = new TutorialItemVM();
		LeftTopItem = new TutorialItemVM();
		RightBottomItem = new TutorialItemVM();
		RightTopItem = new TutorialItemVM();
		CenterItem = new TutorialItemVM();
		GameTexts.SetVariable("newline", "\n");
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		((ViewModel)LeftItem).RefreshValues();
		((ViewModel)RightItem).RefreshValues();
		((ViewModel)BottomItem).RefreshValues();
		((ViewModel)TopItem).RefreshValues();
		((ViewModel)LeftBottomItem).RefreshValues();
		((ViewModel)LeftTopItem).RefreshValues();
		((ViewModel)RightBottomItem).RefreshValues();
		((ViewModel)RightTopItem).RefreshValues();
		((ViewModel)CenterItem).RefreshValues();
	}

	public void SetCurrentTutorial(TutorialItemVM.ItemPlacements placement, string tutorialTypeId, bool requiresMouse)
	{
		if (_currentTutorialItem != null)
		{
			_currentTutorialItem?.CloseTutorialPanel();
			_currentTutorialItem = null;
		}
		TutorialItemVM item = GetItem(placement);
		if (!item.IsEnabled)
		{
			_currentTutorialItem = item;
			_currentTutorialItem.Init(tutorialTypeId, requiresMouse, FinalizeTutorial);
		}
	}

	private void ResetCurrentTutorial()
	{
		_currentTutorialItem.CloseTutorialPanel();
		_currentTutorialItem = null;
	}

	private TutorialItemVM GetItem(TutorialItemVM.ItemPlacements placement)
	{
		return placement switch
		{
			TutorialItemVM.ItemPlacements.Left => LeftItem, 
			TutorialItemVM.ItemPlacements.Right => RightItem, 
			TutorialItemVM.ItemPlacements.Top => TopItem, 
			TutorialItemVM.ItemPlacements.Bottom => BottomItem, 
			TutorialItemVM.ItemPlacements.TopLeft => LeftTopItem, 
			TutorialItemVM.ItemPlacements.TopRight => RightTopItem, 
			TutorialItemVM.ItemPlacements.BottomLeft => LeftBottomItem, 
			TutorialItemVM.ItemPlacements.BottomRight => RightBottomItem, 
			TutorialItemVM.ItemPlacements.Center => CenterItem, 
			_ => null, 
		};
	}

	public void Tick(float dt)
	{
	}

	public void CloseTutorialStep(bool finalizeAllSteps = false)
	{
		_currentTutorialItem?.CloseTutorialPanel();
		_currentTutorialItem = null;
	}

	public void FinalizeTutorial()
	{
		_onTutorialDisabled();
	}
}
