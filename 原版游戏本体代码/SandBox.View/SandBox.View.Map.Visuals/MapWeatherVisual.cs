using System;
using SandBox.View.Map.Managers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.View.Map.Visuals;

public class MapWeatherVisual : MapEntityVisual<WeatherNode>
{
	public GameEntity Prefab;

	private WeatherEvent _previousWeatherEvent;

	private int _maskPixelIndex = -1;

	public Vec2 Position => ((CampaignVec2)(ref base.MapEntity.Position)).ToVec2();

	public Vec2 PrefabSpawnOffset
	{
		get
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			Vec2 terrainSize = Campaign.Current.MapSceneWrapper.GetTerrainSize();
			float num = ((Vec2)(ref terrainSize)).X / (float)Campaign.Current.DefaultWeatherNodeDimension;
			float num2 = ((Vec2)(ref terrainSize)).Y / (float)Campaign.Current.DefaultWeatherNodeDimension;
			return new Vec2(num * 0.5f, num2 * 0.5f);
		}
	}

	public int MaskPixelIndex
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (_maskPixelIndex == -1)
			{
				Vec2 terrainSize = Campaign.Current.MapSceneWrapper.GetTerrainSize();
				float num = ((Vec2)(ref terrainSize)).X / (float)Campaign.Current.DefaultWeatherNodeDimension;
				float num2 = ((Vec2)(ref terrainSize)).Y / (float)Campaign.Current.DefaultWeatherNodeDimension;
				Vec2 position = Position;
				int num3 = (int)(((Vec2)(ref position)).X / num);
				position = Position;
				int num4 = (int)(((Vec2)(ref position)).Y / num2);
				_maskPixelIndex = num4 * Campaign.Current.DefaultWeatherNodeDimension + num3;
			}
			return _maskPixelIndex;
		}
	}

	public override CampaignVec2 InteractionPositionForPlayer => new CampaignVec2(Position, true);

	public override MapEntityVisual AttachedTo => null;

	public override string ToString()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((object)Position/*cast due to .constrained prefix*/).ToString();
	}

	public MapWeatherVisual(WeatherNode weatherNode)
		: base(weatherNode)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_previousWeatherEvent = (WeatherEvent)0;
	}

	public void Tick()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		if (!base.MapEntity.IsVisuallyDirty)
		{
			return;
		}
		bool flag = (int)_previousWeatherEvent == 2;
		bool flag2 = (int)_previousWeatherEvent == 4;
		WeatherEvent weatherEventInPosition = Campaign.Current.Models.MapWeatherModel.GetWeatherEventInPosition(Position);
		bool flag3 = (int)weatherEventInPosition == 2;
		bool num = (int)Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(Position) == 1;
		bool flag4 = (int)weatherEventInPosition == 4;
		byte b = (byte)(num ? 125u : (flag3 ? 200u : 0u));
		byte value = (byte)Math.Max(b, flag4 ? 200 : 0);
		MapWeatherVisualManager.Current.SetRainData(MaskPixelIndex, b);
		MapWeatherVisualManager.Current.SetCloudData(MaskPixelIndex, value);
		if (Prefab == (GameEntity)null)
		{
			if (flag3)
			{
				AttachNewRainPrefabToVisual();
			}
			else if (flag4)
			{
				AttachNewBlizzardPrefabToVisual();
			}
			else if (MBRandom.RandomFloat < 0.1f)
			{
				MapWeatherVisualManager.Current.SetCloudData(MaskPixelIndex, 200);
			}
		}
		else
		{
			if (flag && !flag3 && flag4)
			{
				MapWeatherVisualManager.Current.ReleaseRainPrefab(Prefab);
				AttachNewBlizzardPrefabToVisual();
			}
			else if (flag2 && !flag4 && flag3)
			{
				MapWeatherVisualManager.Current.ReleaseBlizzardPrefab(Prefab);
				AttachNewRainPrefabToVisual();
			}
			if (!flag3 && !flag4)
			{
				if (flag)
				{
					MapWeatherVisualManager.Current.ReleaseRainPrefab(Prefab);
				}
				else if (flag2)
				{
					MapWeatherVisualManager.Current.ReleaseBlizzardPrefab(Prefab);
				}
				Prefab = null;
			}
		}
		_previousWeatherEvent = weatherEventInPosition;
		base.MapEntity.OnVisualUpdated();
	}

	private void AttachNewRainPrefabToVisual()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = new Vec3(Position + PrefabSpawnOffset, 26f, -1f);
		GameEntity rainPrefabFromPool = MapWeatherVisualManager.Current.GetRainPrefabFromPool();
		rainPrefabFromPool.SetVisibilityExcludeParents(true);
		rainPrefabFromPool.SetGlobalFrame(ref identity, true);
		Prefab = rainPrefabFromPool;
	}

	private void AttachNewBlizzardPrefabToVisual()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = new Vec3(Position + PrefabSpawnOffset, 26f, -1f);
		GameEntity blizzardPrefabFromPool = MapWeatherVisualManager.Current.GetBlizzardPrefabFromPool();
		blizzardPrefabFromPool.SetVisibilityExcludeParents(true);
		blizzardPrefabFromPool.SetGlobalFrame(ref identity, true);
		Prefab = blizzardPrefabFromPool;
	}

	public override bool OnMapClick(bool followModifierUsed)
	{
		return false;
	}

	public override void OnHover()
	{
	}

	public override void OnOpenEncyclopedia()
	{
	}

	public override bool IsVisibleOrFadingOut()
	{
		return false;
	}

	public override Vec3 GetVisualPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 interactionPositionForPlayer = InteractionPositionForPlayer;
		return ((CampaignVec2)(ref interactionPositionForPlayer)).AsVec3();
	}
}
