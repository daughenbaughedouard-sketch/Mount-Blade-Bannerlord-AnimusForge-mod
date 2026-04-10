using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.View.Map.Managers;

internal class MapAudioManager : CampaignEntityVisualComponent
{
	private const string SeasonParameterId = "Season";

	private const string CameraHeightParameterId = "CampaignCameraHeight";

	private const string TimeOfDayParameterId = "Daytime";

	private const string WeatherEventIntensityParameterId = "Rainfall";

	private Seasons _lastCachedSeason;

	private float _lastCameraZ;

	private int _lastHourUpdate;

	private MapScene _mapScene;

	public override int Priority => 70;

	public MapAudioManager()
	{
		_mapScene = Campaign.Current.MapSceneWrapper as MapScene;
	}

	public override void OnVisualTick(MapScreen screen, float realDt, float dt)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		CampaignTime now = CampaignTime.Now;
		if (((CampaignTime)(ref now)).GetSeasonOfYear != _lastCachedSeason)
		{
			now = CampaignTime.Now;
			SoundManager.SetGlobalParameter("Season", (float)((CampaignTime)(ref now)).GetSeasonOfYear);
			now = CampaignTime.Now;
			_lastCachedSeason = ((CampaignTime)(ref now)).GetSeasonOfYear;
		}
		float lastCameraZ = _lastCameraZ;
		Vec3 lastFinalRenderCameraPosition = _mapScene.Scene.LastFinalRenderCameraPosition;
		if (Math.Abs(lastCameraZ - ((Vec3)(ref lastFinalRenderCameraPosition)).Z) > 0.1f)
		{
			lastFinalRenderCameraPosition = _mapScene.Scene.LastFinalRenderCameraPosition;
			SoundManager.SetGlobalParameter("CampaignCameraHeight", ((Vec3)(ref lastFinalRenderCameraPosition)).Z);
			lastFinalRenderCameraPosition = _mapScene.Scene.LastFinalRenderCameraPosition;
			_lastCameraZ = ((Vec3)(ref lastFinalRenderCameraPosition)).Z;
		}
		now = CampaignTime.Now;
		if ((int)((CampaignTime)(ref now)).CurrentHourInDay == _lastHourUpdate)
		{
			now = CampaignTime.Now;
			SoundManager.SetGlobalParameter("Daytime", ((CampaignTime)(ref now)).CurrentHourInDay);
			now = CampaignTime.Now;
			_lastHourUpdate = (int)((CampaignTime)(ref now)).CurrentHourInDay;
		}
	}
}
