using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StoryMode.Missions;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace StoryMode.ViewModelCollection.Missions;

public class TrainingFieldObjectivesVM : ViewModel
{
	private TrainingFieldObjectiveItemVM _dummyObjective;

	private string _leaveAnyTimeText;

	private string _currentObjectiveExplanationText;

	private string _timerText;

	private TrainingFieldObjectiveItemVM _activeObjective;

	private MBBindingList<TrainingFieldObjectiveItemVM> _objectiveItems;

	[DataSourceProperty]
	public string LeaveAnyTimeText
	{
		get
		{
			return _leaveAnyTimeText;
		}
		set
		{
			if (value != _leaveAnyTimeText)
			{
				_leaveAnyTimeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "LeaveAnyTimeText");
			}
		}
	}

	[DataSourceProperty]
	public string CurrentObjectiveExplanationText
	{
		get
		{
			return _currentObjectiveExplanationText;
		}
		set
		{
			if (value != _currentObjectiveExplanationText)
			{
				_currentObjectiveExplanationText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CurrentObjectiveExplanationText");
			}
		}
	}

	[DataSourceProperty]
	public string TimerText
	{
		get
		{
			return _timerText;
		}
		set
		{
			if (value != _timerText)
			{
				_timerText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "TimerText");
			}
		}
	}

	[DataSourceProperty]
	public TrainingFieldObjectiveItemVM ActiveObjective
	{
		get
		{
			return _activeObjective;
		}
		set
		{
			if (value != _activeObjective)
			{
				_activeObjective = value;
				((ViewModel)this).OnPropertyChangedWithValue<TrainingFieldObjectiveItemVM>(value, "ActiveObjective");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<TrainingFieldObjectiveItemVM> ObjectiveItems
	{
		get
		{
			return _objectiveItems;
		}
		set
		{
			if (value != _objectiveItems)
			{
				_objectiveItems = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<TrainingFieldObjectiveItemVM>>(value, "ObjectiveItems");
			}
		}
	}

	public TrainingFieldObjectivesVM()
	{
		ObjectiveItems = new MBBindingList<TrainingFieldObjectiveItemVM>();
		_dummyObjective = TrainingFieldObjectiveItemVM.CreateDummy();
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		string keyHyperlinkText = HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("Generic", 4), 1f);
		GameTexts.SetVariable("LEAVE_KEY", keyHyperlinkText);
		GameTexts.SetVariable("newline", "\n");
		LeaveAnyTimeText = ((object)GameTexts.FindText("str_leave_training_field", (string)null)).ToString();
		ObjectiveItems.ApplyActionOnAllItems((Action<TrainingFieldObjectiveItemVM>)delegate(TrainingFieldObjectiveItemVM o)
		{
			((ViewModel)o).RefreshValues();
		});
	}

	public void UpdateObjectivesWith(List<TrainingFieldMissionController.TutorialObjective> objectives)
	{
		((Collection<TrainingFieldObjectiveItemVM>)(object)ObjectiveItems).Clear();
		foreach (TrainingFieldMissionController.TutorialObjective objective in objectives)
		{
			TrainingFieldObjectiveItemVM trainingFieldObjectiveItemVM = TrainingFieldObjectiveItemVM.CreateFromObjective(objective);
			((Collection<TrainingFieldObjectiveItemVM>)(object)ObjectiveItems).Add(trainingFieldObjectiveItemVM);
			if (objective.IsActive)
			{
				ActiveObjective = trainingFieldObjectiveItemVM;
			}
		}
	}

	public void UpdateCurrentObjectiveExplanationText(TextObject currentObjectiveText)
	{
		if (ActiveObjective == null)
		{
			ActiveObjective = _dummyObjective;
		}
		CurrentObjectiveExplanationText = ((object)currentObjectiveText)?.ToString() ?? "";
	}

	public void UpdateCurrentMouseObjective(TrainingFieldMissionController.MouseObjectives currentMouseObjective, TrainingFieldMissionController.ObjectivePerformingType currentObjectivePerformingType)
	{
		ActiveObjective?.UpdateObjective(currentMouseObjective, currentObjectivePerformingType);
	}

	public void UpdateTimerText(string timerText)
	{
		TimerText = timerText;
	}
}
