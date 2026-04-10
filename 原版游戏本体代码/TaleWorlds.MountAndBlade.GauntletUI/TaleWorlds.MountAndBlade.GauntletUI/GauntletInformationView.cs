using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletInformationView : GlobalLayer
{
	private TooltipBaseVM _dataSource;

	private GauntletMovieIdentifier _movie;

	private GauntletLayer _layerAsGauntletLayer;

	private static GauntletInformationView _current;

	private const float _tooltipExtendTreshold = 0.18f;

	private float _gamepadTooltipExtendTimer;

	private GauntletInformationView()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		_layerAsGauntletLayer = new GauntletLayer("Tooltip", 115000, false);
		InformationManager.OnShowTooltip += OnShowTooltip;
		InformationManager.OnHideTooltip += OnHideTooltip;
		InformationManager.IsAnyTooltipActiveInternal = (IsAnyTooltipActiveDelegate)Delegate.Combine((Delegate?)(object)InformationManager.IsAnyTooltipActiveInternal, (Delegate?)new IsAnyTooltipActiveDelegate(OnGetIsAnyTooltipActive));
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_layerAsGauntletLayer;
	}

	public static void Initialize()
	{
		if (_current == null)
		{
			_current = new GauntletInformationView();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)_current, false);
			PropertyBasedTooltipVM.AddKeyType("MapClick", (Func<string>)(() => _current.GetKey("MapHotKeyCategory", "MapClick")));
			PropertyBasedTooltipVM.AddKeyType("FollowModifier", (Func<string>)(() => _current.GetKey("MapHotKeyCategory", "MapFollowModifier")));
			PropertyBasedTooltipVM.AddKeyType("ExtendModifier", (Func<string>)(() => _current.GetExtendTooltipKeyText()));
		}
	}

	public static void OnFinalize()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		if (_current != null)
		{
			InformationManager.OnShowTooltip -= _current.OnShowTooltip;
			InformationManager.OnHideTooltip -= _current.OnHideTooltip;
			InformationManager.IsAnyTooltipActiveInternal = (IsAnyTooltipActiveDelegate)Delegate.Remove((Delegate?)(object)InformationManager.IsAnyTooltipActiveInternal, (Delegate?)new IsAnyTooltipActiveDelegate(_current.OnGetIsAnyTooltipActive));
		}
	}

	protected override void OnTick(float dt)
	{
		((GlobalLayer)this).OnTick(dt);
		if (_dataSource != null && (Input.IsKeyDown((InputKey)56) || Input.IsKeyDown((InputKey)184) || Input.IsKeyDown((InputKey)248)))
		{
			_gamepadTooltipExtendTimer += dt;
		}
		else
		{
			_gamepadTooltipExtendTimer = 0f;
		}
		if (_dataSource != null)
		{
			_dataSource.Tick(dt);
			_dataSource.IsExtended = (Input.IsGamepadActive ? (_gamepadTooltipExtendTimer > 0.18f) : (_gamepadTooltipExtendTimer > 0f));
		}
	}

	private string GetExtendTooltipKeyText()
	{
		if (Input.IsGamepadActive)
		{
			return GetKey("MapHotKeyCategory", "MapFollowModifier");
		}
		return ((object)Game.Current.GameTextManager.FindText("str_game_key_text", "anyalt")).ToString();
	}

	private string GetKey(string categoryId, string keyId)
	{
		return ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, categoryId, keyId)).ToString();
	}

	private string GetKey(string categoryId, int keyId)
	{
		return ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, categoryId, keyId)).ToString();
	}

	private void OnShowTooltip(Type type, object[] args)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		OnHideTooltip();
		if (InformationManager.RegisteredTypes.TryGetValue(type, out var value))
		{
			try
			{
				ref TooltipBaseVM dataSource = ref _dataSource;
				object? obj = Activator.CreateInstance(value.TooltipType, type, args);
				dataSource = (TooltipBaseVM)((obj is TooltipBaseVM) ? obj : null);
				_movie = _layerAsGauntletLayer.LoadMovie(value.MovieName, (ViewModel)(object)_dataSource);
				return;
			}
			catch (Exception arg)
			{
				Debug.FailedAssert($"Failed to display tooltip of type: {type.FullName}. Exception: {arg}", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletInformationView.cs", "OnShowTooltip", 112);
				return;
			}
		}
		Debug.FailedAssert("Unable to show tooltip. Either the given type or the corresponding tooltip type is not added to TooltipMappingProvider. Given type: " + type.FullName, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletInformationView.cs", "OnShowTooltip", 117);
	}

	private void OnHideTooltip()
	{
		TooltipBaseVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		if (_movie != null)
		{
			_layerAsGauntletLayer.ReleaseMovie(_movie);
		}
		_dataSource = null;
		_movie = null;
	}

	private void OnGetIsAnyTooltipActive(out bool isAnyTooltipActive, out bool isAnyTooltipExtended)
	{
		TooltipBaseVM dataSource = _dataSource;
		isAnyTooltipActive = dataSource != null && dataSource.IsActive;
		TooltipBaseVM dataSource2 = _dataSource;
		isAnyTooltipExtended = dataSource2 != null && dataSource2.IsExtended;
	}
}
