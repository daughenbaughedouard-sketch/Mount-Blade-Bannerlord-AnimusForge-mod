using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SandBox.ViewModelCollection.SaveLoad;

public class MapSaveVM : ViewModel
{
	private readonly Action<bool> _onActiveStateChange;

	private string _savingText;

	private bool _isActive;

	[DataSourceProperty]
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				_isActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActive");
			}
		}
	}

	[DataSourceProperty]
	public string SavingText
	{
		get
		{
			return _savingText;
		}
		set
		{
			if (value != _savingText)
			{
				_savingText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SavingText");
			}
		}
	}

	public MapSaveVM(Action<bool> onActiveStateChange)
	{
		_onActiveStateChange = onActiveStateChange;
		CampaignEvents.OnSaveStartedEvent.AddNonSerializedListener((object)this, (Action)OnSaveStarted);
		CampaignEvents.OnSaveOverEvent.AddNonSerializedListener((object)this, (Action<bool, string>)OnSaveOver);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		TextObject val = new TextObject("{=cp2XDjeq}Saving...", (Dictionary<string, object>)null);
		SavingText = ((object)val).ToString();
	}

	private void OnSaveOver(bool isSuccessful, string saveName)
	{
		IsActive = false;
		_onActiveStateChange?.Invoke(obj: false);
	}

	private void OnSaveStarted()
	{
		IsActive = true;
		_onActiveStateChange?.Invoke(obj: true);
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		CampaignEvents.OnSaveStartedEvent.ClearListeners((object)this);
		((IMbEventBase)CampaignEvents.OnSaveOverEvent).ClearListeners((object)this);
	}
}
