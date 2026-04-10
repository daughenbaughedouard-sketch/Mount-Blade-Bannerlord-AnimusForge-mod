using System;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace NavalDLC.ViewModelCollection.Map;

public class MapAnchorTrackerVM : ViewModel
{
	private readonly Action _onMoveCameraToPosition;

	private bool _isVisible;

	private float _positionX;

	private float _positionY;

	private float _positionW;

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
	public float PositionX
	{
		get
		{
			return _positionX;
		}
		set
		{
			if (value != _positionX)
			{
				_positionX = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PositionX");
			}
		}
	}

	[DataSourceProperty]
	public float PositionY
	{
		get
		{
			return _positionY;
		}
		set
		{
			if (value != _positionY)
			{
				_positionY = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PositionY");
			}
		}
	}

	[DataSourceProperty]
	public float PositionW
	{
		get
		{
			return _positionW;
		}
		set
		{
			if (value != _positionW)
			{
				_positionW = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "PositionW");
			}
		}
	}

	public MapAnchorTrackerVM(Action onMoveCameraToPosition)
	{
		_onMoveCameraToPosition = onMoveCameraToPosition;
	}

	public void ExecuteGoToPosition()
	{
		_onMoveCameraToPosition?.Invoke();
	}

	public void ExecuteShowTooltip()
	{
		AnchorPoint anchor = MobileParty.MainParty.Anchor;
		if (anchor != null && anchor.IsValid)
		{
			InformationManager.ShowTooltip(typeof(AnchorPoint), new object[1] { MobileParty.MainParty.Anchor });
		}
	}

	public void ExecuteHideTooltip()
	{
		InformationManager.HideTooltip();
	}
}
