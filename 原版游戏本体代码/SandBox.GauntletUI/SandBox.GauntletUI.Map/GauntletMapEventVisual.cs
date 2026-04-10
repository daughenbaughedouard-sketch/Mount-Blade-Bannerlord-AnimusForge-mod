using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.GauntletUI.Map;

public class GauntletMapEventVisual : IMapEventVisual
{
	private static int _battleSoundEventIndex = SoundManager.GetEventGlobalIndex("event:/map/ambient/node/battle");

	private static int _navalBattleSoundEventIndex = SoundManager.GetEventGlobalIndex("event:/map/ambient/node/naval_battle_loop");

	private static int _raidSoundEventIndex = SoundManager.GetEventGlobalIndex("event:/map/ambient/node/battle_raid");

	private static int _siegeSoundEventIndex = SoundManager.GetEventGlobalIndex("event:/map/ambient/node/battle_siege");

	private static int _hideoutBattleSoundEventIndex = SoundManager.GetEventGlobalIndex("event:/map/ambient/node/battle_hideout");

	private SoundEvent _mapEventSoundEvent;

	private readonly Action<GauntletMapEventVisual> _onDeactivate;

	private readonly Action<GauntletMapEventVisual> _onInitialized;

	private readonly Action<GauntletMapEventVisual> _onVisibilityChanged;

	private Scene _mapScene;

	public MapEvent MapEvent { get; private set; }

	public Vec2 WorldPosition { get; private set; }

	public bool IsVisible { get; private set; }

	private Scene MapScene
	{
		get
		{
			if ((NativeObject)(object)_mapScene == (NativeObject)null)
			{
				Campaign current = Campaign.Current;
				if (((current != null) ? current.MapSceneWrapper : null) != null)
				{
					_mapScene = ((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene;
				}
			}
			return _mapScene;
		}
	}

	public GauntletMapEventVisual(MapEvent mapEvent, Action<GauntletMapEventVisual> onInitialized, Action<GauntletMapEventVisual> onVisibilityChanged, Action<GauntletMapEventVisual> onDeactivate)
	{
		_onDeactivate = onDeactivate;
		_onInitialized = onInitialized;
		_onVisibilityChanged = onVisibilityChanged;
		MapEvent = mapEvent;
	}

	public void Initialize(CampaignVec2 position, bool isVisible)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition = ((CampaignVec2)(ref position)).ToVec2();
		IsVisible = isVisible;
		_onInitialized?.Invoke(this);
		int num = -1;
		int num2 = 4;
		if (MapEvent.IsNavalMapEvent || MapEvent.IsBlockade || MapEvent.IsBlockadeSallyOut)
		{
			num = _navalBattleSoundEventIndex;
		}
		else if (MapEvent.IsFieldBattle || MapEvent.IsSallyOut)
		{
			num = _battleSoundEventIndex;
			num2 = GetBattleSizeValue();
		}
		else if (MapEvent.IsSiegeAssault || MapEvent.IsSiegeOutside || MapEvent.IsSiegeAmbush)
		{
			num = _siegeSoundEventIndex;
		}
		else if (MapEvent.IsRaid)
		{
			num = _raidSoundEventIndex;
		}
		else if (MapEvent.IsHideoutBattle)
		{
			num = _hideoutBattleSoundEventIndex;
		}
		if (num != -1)
		{
			float num3 = 0f;
			Settlement mapEventSettlement = MapEvent.MapEventSettlement;
			CampaignVec2 val = ((mapEventSettlement != null) ? mapEventSettlement.Position : MapEvent.Position);
			Campaign.Current.MapSceneWrapper.GetHeightAtPoint(ref val, ref num3);
			_mapEventSoundEvent = SoundEvent.CreateEvent(num, MapScene);
			_mapEventSoundEvent.SetParameter("battle_size", (float)num2);
			_mapEventSoundEvent.PlayInPosition(new Vec3(((CampaignVec2)(ref position)).X, ((CampaignVec2)(ref position)).Y, num3 + 2f, -1f));
			if (!isVisible)
			{
				_mapEventSoundEvent.Pause();
			}
		}
	}

	private int GetBattleSizeValue()
	{
		if (MapEvent.IsSiegeAssault)
		{
			return 4;
		}
		int numberOfInvolvedMen = MapEvent.GetNumberOfInvolvedMen();
		if (numberOfInvolvedMen < 30)
		{
			return 0;
		}
		if (numberOfInvolvedMen < 80)
		{
			return 1;
		}
		if (numberOfInvolvedMen >= 120)
		{
			return 3;
		}
		return 2;
	}

	public void OnMapEventEnd()
	{
		_onDeactivate?.Invoke(this);
		if (_mapEventSoundEvent != null)
		{
			_mapEventSoundEvent.Stop();
			_mapEventSoundEvent = null;
		}
	}

	public void SetVisibility(bool isVisible)
	{
		IsVisible = isVisible;
		_onVisibilityChanged?.Invoke(this);
		SoundEvent mapEventSoundEvent = _mapEventSoundEvent;
		if (mapEventSoundEvent != null && mapEventSoundEvent.IsValid)
		{
			if (isVisible && _mapEventSoundEvent.IsPaused())
			{
				_mapEventSoundEvent.Resume();
			}
			else if (!isVisible && !_mapEventSoundEvent.IsPaused())
			{
				_mapEventSoundEvent.Pause();
			}
		}
	}
}
