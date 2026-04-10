using System;
using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class KeybindingPopupVM : ViewModel
{
	private readonly Action _onCancel;

	private string _pressKeyText;

	private string _cancelText;

	[DataSourceProperty]
	public string PressKeyText
	{
		get
		{
			return _pressKeyText;
		}
		set
		{
			if (_pressKeyText != value)
			{
				_pressKeyText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "PressKeyText");
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
			if (_cancelText != value)
			{
				_cancelText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "CancelText");
			}
		}
	}

	public KeybindingPopupVM(Action onCancel)
	{
		_onCancel = onCancel;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		PressKeyText = ((object)new TextObject("{=hvaDkG4w}Press any key.", (Dictionary<string, object>)null)).ToString();
		TextObject val = new TextObject("{=5U8vXv4E}Press {KEY} to cancel", (Dictionary<string, object>)null);
		val.SetTextVariable("KEY", ((object)HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit")).ToString());
		CancelText = ((object)val).ToString();
	}

	public void ExecuteCancel()
	{
		_onCancel?.Invoke();
	}
}
