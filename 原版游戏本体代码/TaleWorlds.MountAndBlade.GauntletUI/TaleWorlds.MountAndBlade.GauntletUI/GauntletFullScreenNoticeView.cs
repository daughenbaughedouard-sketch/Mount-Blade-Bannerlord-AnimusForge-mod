using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletFullScreenNoticeView : GlobalLayer
{
	private readonly FullScreenNoticeVM _dataSource;

	public static GauntletFullScreenNoticeView Current { get; private set; }

	public GauntletFullScreenNoticeView()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		_dataSource = new FullScreenNoticeVM();
		GauntletLayer val = new GauntletLayer("FullScreenNotice", 15010, false);
		val.LoadMovie("FullScreenNotice", (ViewModel)(object)_dataSource);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)val;
		((GlobalLayer)this).Layer.IsFocusLayer = true;
		((GlobalLayer)this).Layer.InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)val).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
	}

	public static void Initialize()
	{
		if (Current == null && !BannerlordConfig.IAPNoticeConfirmed)
		{
			Current = new GauntletFullScreenNoticeView();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
		}
	}

	public static void SkipNotice()
	{
		GauntletFullScreenNoticeView current = Current;
		if (current != null)
		{
			FullScreenNoticeVM dataSource = current._dataSource;
			if (dataSource != null)
			{
				dataSource.ExecuteCloseNotice();
			}
		}
	}

	protected override void OnTick(float dt)
	{
		((GlobalLayer)this).OnTick(dt);
		if (Current?._dataSource == null)
		{
			return;
		}
		if (Current._dataSource.IsNoticeActive)
		{
			ScreenManager.TrySetFocus(((GlobalLayer)this).Layer);
			if (((GlobalLayer)this).Layer.Input.IsHotKeyReleased("Confirm"))
			{
				SkipNotice();
				UISoundsHelper.PlayUISound("event:/ui/default");
			}
		}
		else
		{
			ScreenManager.RemoveGlobalLayer((GlobalLayer)(object)Current);
			((ViewModel)Current._dataSource).OnFinalize();
			Current = null;
		}
	}
}
