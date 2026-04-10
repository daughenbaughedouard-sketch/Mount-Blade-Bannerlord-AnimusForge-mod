using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;

public class MissionDuelLandmarkMarkerVM : ViewModel
{
	public readonly GameEntity Entity;

	public readonly IFocusable FocusableComponent;

	private float _latestX;

	private float _latestY;

	private float _latestW;

	private bool _isFocused;

	private int _troopType;

	private string _actionDescriptionText;

	public bool IsInScreenBoundaries { get; private set; }

	[DataSourceProperty]
	public bool IsFocused
	{
		get
		{
			return _isFocused;
		}
		set
		{
			if (value != _isFocused)
			{
				_isFocused = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsFocused");
			}
		}
	}

	[DataSourceProperty]
	public int TroopType
	{
		get
		{
			return _troopType;
		}
		set
		{
			if (value != _troopType)
			{
				_troopType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "TroopType");
			}
		}
	}

	[DataSourceProperty]
	public string ActionDescriptionText
	{
		get
		{
			return _actionDescriptionText;
		}
		set
		{
			if (value != _actionDescriptionText)
			{
				_actionDescriptionText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ActionDescriptionText");
			}
		}
	}

	public MissionDuelLandmarkMarkerVM(GameEntity entity)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected I4, but got Unknown
		Entity = entity;
		FocusableComponent = (IFocusable)(object)Entity.GetFirstScriptOfType<DuelZoneLandmark>();
		TroopType = (int)Entity.GetFirstScriptOfType<DuelZoneLandmark>().ZoneTroopType;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f);
		GameTexts.SetVariable("KEY", keyHyperlinkText);
		GameTexts.SetVariable("ACTION", new TextObject("{=7jMnNlXG}Change Arena Preference", (Dictionary<string, object>)null));
		ActionDescriptionText = ((object)GameTexts.FindText("str_key_action", (string)null)).ToString();
	}

	public void UpdateScreenPosition(Camera missionCamera)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Vec3 globalPosition = Entity.GlobalPosition;
		_latestX = 0f;
		_latestY = 0f;
		_latestW = 0f;
		MBWindowManager.WorldToScreen(missionCamera, globalPosition, ref _latestX, ref _latestY, ref _latestW);
		IsInScreenBoundaries = _latestW > 0f && !(_latestX > Screen.RealScreenResolutionWidth) && !(_latestY > Screen.RealScreenResolutionHeight) && !(_latestX + 200f < 0f) && !(_latestY + 100f < 0f);
	}
}
