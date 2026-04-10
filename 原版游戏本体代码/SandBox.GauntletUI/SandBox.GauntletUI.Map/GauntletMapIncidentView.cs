using SandBox.View.Map;
using SandBox.ViewModelCollection.Map.Incidents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapIncidentView))]
public class GauntletMapIncidentView : MapIncidentView
{
	private MapIncidentVM _dataSource;

	private GauntletLayer _gauntletLayer;

	private SpriteCategory _spriteCategory;

	private bool _controlModeLockBeforeIncident;

	private CampaignTimeControlMode _controlModeBeforeIncident;

	public GauntletMapIncidentView(Incident incident)
		: base(incident)
	{
	}

	protected override void OnMapConversationStart()
	{
		base.OnMapConversationStart();
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, true);
		}
	}

	protected override void OnMapConversationOver()
	{
		base.OnMapConversationOver();
		if (_gauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)_gauntletLayer, false);
		}
	}

	protected override void CreateLayout()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		base.CreateLayout();
		if (Incident == null)
		{
			Debug.FailedAssert("Failed to start incident view", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Map\\GauntletMapIncidentView.cs", "CreateLayout", 57);
			return;
		}
		_controlModeBeforeIncident = Campaign.Current.TimeControlMode;
		_controlModeLockBeforeIncident = Campaign.Current.TimeControlModeLock;
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		Campaign.Current.SetTimeControlModeLock(true);
		MBCommon.PauseGameEngine();
		_dataSource = new MapIncidentVM(Incident, OnCloseView);
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_gauntletLayer = new GauntletLayer("MapIncidents", 203, false);
		_gauntletLayer.LoadMovie("MapIncident", (ViewModel)(object)_dataSource);
		((ScreenLayer)_gauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)_gauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		base.Layer = (ScreenLayer)(object)_gauntletLayer;
		((ScreenBase)base.MapScreen).AddLayer(base.Layer);
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_map_incidents");
		base.Layer.IsFocusLayer = true;
		ScreenManager.TrySetFocus(base.Layer);
		base.MapScreen.SetIsMapIncidentActive(isMapIncidentActive: true);
		PlayIncidentSound();
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		Tick();
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		Tick();
	}

	protected override void OnMenuModeTick(float dt)
	{
		base.OnMenuModeTick(dt);
		Tick();
	}

	private void Tick()
	{
		if (_dataSource != null && ((ScreenLayer)_gauntletLayer).Input.IsHotKeyReleased("Confirm") && _dataSource.CanConfirm)
		{
			UISoundsHelper.PlayUISound("event:/ui/default");
			_dataSource.ExecuteConfirm();
		}
	}

	protected override bool IsOpeningEscapeMenuOnFocusChangeAllowed()
	{
		return false;
	}

	private void OnCloseView()
	{
		base.MapScreen.RemoveMapView(this);
	}

	protected override void OnFinalize()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.OnFinalize();
		if (MBCommon.IsPaused)
		{
			MBCommon.UnPauseGameEngine();
		}
		if (base.Layer != null)
		{
			_spriteCategory.Unload();
			((ViewModel)_dataSource).OnFinalize();
			_dataSource = null;
			base.Layer.IsFocusLayer = false;
			ScreenManager.TryLoseFocus(base.Layer);
			((ScreenBase)base.MapScreen).RemoveLayer(base.Layer);
			base.MapScreen.SetIsMapIncidentActive(isMapIncidentActive: false);
			Campaign.Current.TimeControlMode = _controlModeBeforeIncident;
			Campaign.Current.SetTimeControlModeLock(_controlModeLockBeforeIncident);
		}
		else if (_dataSource != null || _spriteCategory != null)
		{
			Debug.FailedAssert("Incident view is was not propertly initialized", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Map\\GauntletMapIncidentView.cs", "OnFinalize", 162);
			MapIncidentVM dataSource = _dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).OnFinalize();
			}
			SpriteCategory spriteCategory = _spriteCategory;
			if (spriteCategory != null)
			{
				spriteCategory.Unload();
			}
		}
	}

	private void PlayIncidentSound()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected I4, but got Unknown
		string text = "";
		IncidentType type = Incident.Type;
		switch ((int)type)
		{
		case 0:
			text = "event:/ui/encounter/troop_settlement";
			break;
		case 1:
			text = "event:/ui/encounter/food_spoil";
			break;
		case 2:
			text = "event:/ui/encounter/plight";
			break;
		case 3:
			text = "event:/ui/encounter/camp";
			break;
		case 4:
			text = "event:/ui/encounter/sick_animals";
			break;
		case 5:
			text = "event:/ui/encounter/illness";
			break;
		case 6:
			text = "event:/ui/encounter/hunting_foraging";
			break;
		case 7:
			text = "event:/ui/encounter/post_battle";
			break;
		case 8:
			text = "event:/ui/encounter/hard_travel";
			break;
		case 9:
			text = "event:/ui/encounter/profit";
			break;
		case 10:
			text = "event:/ui/encounter/dreams_signs";
			break;
		case 11:
			text = "event:/ui/encounter/fief";
			break;
		case 12:
			text = "event:/ui/encounter/siege";
			break;
		case 13:
			text = "event:/ui/encounter/workshops";
			break;
		default:
			Debug.FailedAssert("Incident sound cannot be found!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.GauntletUI\\Map\\GauntletMapIncidentView.cs", "PlayIncidentSound", 233);
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			UISoundsHelper.PlayUISound(text);
		}
	}
}
