using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Barter;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Map;

public class GauntletMapConversationBarterView
{
	public delegate void OnBarterActiveStateChanged(bool isBarterActive);

	private readonly GauntletLayer _gauntletLayer;

	private readonly OnBarterActiveStateChanged _onActiveStateChanged;

	private SpriteCategory _barterCategory;

	private BarterVM _barterDataSource;

	private GauntletMovieIdentifier _barterMovie;

	public bool IsCreated { get; private set; }

	public bool IsActive { get; private set; }

	public GauntletMapConversationBarterView(GauntletLayer layer, OnBarterActiveStateChanged onActiveStateChanged)
	{
		_gauntletLayer = layer;
		_onActiveStateChanged = onActiveStateChanged;
	}

	public void CreateBarterView(BarterData args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		_barterDataSource = new BarterVM(args);
		_barterDataSource.SetResetInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Reset"));
		_barterDataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_barterDataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_onActiveStateChanged?.Invoke(isBarterActive: true);
		_barterCategory = UIResourceManager.GetSpriteCategory("ui_barter");
		Activate();
		IsCreated = true;
	}

	public void DestroyBarterView()
	{
		Deactivate();
		((ViewModel)_barterDataSource).OnFinalize();
		_barterDataSource = null;
		_barterCategory = null;
		_onActiveStateChanged?.Invoke(isBarterActive: false);
		BarterItemVM.IsFiveStackModifierActive = false;
		BarterItemVM.IsEntireStackModifierActive = false;
		IsCreated = false;
	}

	public void Activate()
	{
		_barterMovie = _gauntletLayer.LoadMovie("BarterScreen", (ViewModel)(object)_barterDataSource);
		Extensions.Load(_barterCategory);
		_onActiveStateChanged?.Invoke(isBarterActive: true);
		IsActive = true;
	}

	public void Deactivate()
	{
		_gauntletLayer.ReleaseMovie(_barterMovie);
		_barterCategory.Unload();
		IsActive = false;
	}

	public void TickInput()
	{
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_barterDataSource.ExecuteCancel();
			return;
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm"))
		{
			BarterVM barterDataSource = _barterDataSource;
			if (barterDataSource != null && !barterDataSource.IsOfferDisabled)
			{
				UISoundsHelper.PlayUISound("event:/ui/default");
				_barterDataSource.ExecuteOffer();
				return;
			}
		}
		if (((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Reset"))
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_barterDataSource.ExecuteReset();
		}
	}
}
