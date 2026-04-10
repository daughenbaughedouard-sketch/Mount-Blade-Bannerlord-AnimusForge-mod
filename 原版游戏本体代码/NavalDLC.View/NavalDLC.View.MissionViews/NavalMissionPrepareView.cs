using System;
using System.Collections.Generic;
using System.Linq;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.ShipActuators;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace NavalDLC.View.MissionViews;

public class NavalMissionPrepareView : MissionView
{
	private NavalShipsLogic _navalShipsLogic;

	private string BannerTag => "banner_with_faction_color";

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		_navalShipsLogic.ShipSpawnedEvent += OnShipSpawned;
		_navalShipsLogic.ShipCapturedEvent += StartBannerChangeAnimationForShip;
	}

	public void OnShipSpawned(MissionShip missionShip)
	{
		foreach (GameEntity item in (List<GameEntity>)(object)missionShip.BannerEntities)
		{
			SetOwnerBanner(item, missionShip.Banner);
		}
		foreach (GameEntity item2 in (List<GameEntity>)(object)missionShip.SailMeshEntities)
		{
			var (sailColor, sailColor2) = missionShip.SailColors;
			SetSailColors(item2, sailColor, sailColor2);
		}
	}

	private void SetSailColors(GameEntity sailEntity, uint sailColor1, uint sailColor2)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)sailEntity.Skeleton != (NativeObject)null)
		{
			foreach (Mesh allMesh in sailEntity.Skeleton.GetAllMeshes())
			{
				if (allMesh.HasTag("faction_color"))
				{
					allMesh.Color = sailColor1;
					allMesh.Color2 = sailColor2;
				}
			}
		}
		WeakGameEntity weakEntity = sailEntity.WeakEntity;
		foreach (Mesh item in ((WeakGameEntity)(ref weakEntity)).GetAllMeshesWithTag("faction_color"))
		{
			item.Color = sailColor1;
			item.Color2 = sailColor2;
		}
	}

	private void SetOwnerBanner(GameEntity bannerEntity, Banner ownerBanner)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		BannerDebugInfo val = BannerDebugInfo.CreateManual(((object)this).GetType().Name);
		BannerVisualExtensions.GetTableauTextureLarge(ownerBanner, ref val, (Action<Texture>)delegate(Texture tex)
		{
			OnTextureRendered(tex, bannerEntity);
		});
	}

	private void OnTextureRendered(Texture tex, GameEntity bannerEntity)
	{
		List<Mesh> list = bannerEntity.GetAllMeshesWithTag(BannerTag).ToList();
		if (Extensions.IsEmpty<Mesh>((IEnumerable<Mesh>)list))
		{
			list.Add(bannerEntity.GetFirstMesh());
		}
		foreach (Mesh item in list)
		{
			if ((NativeObject)(object)item != (NativeObject)null)
			{
				Material val = item.GetMaterial().CreateCopy();
				val.SetTexture((MBTextureType)1, tex);
				uint num = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
				ulong shaderFlags = val.GetShaderFlags();
				val.SetShaderFlags(shaderFlags | num);
				item.SetMaterial(val);
			}
		}
	}

	public override void OnRemoveBehavior()
	{
		((MissionView)this).OnRemoveBehavior();
		_navalShipsLogic.ShipSpawnedEvent -= OnShipSpawned;
		_navalShipsLogic.ShipCapturedEvent -= StartBannerChangeAnimationForShip;
	}

	public void StartBannerChangeAnimationForShip(MissionShip ship, MissionShip ship2, Formation formation, Formation formation2)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Banner banner = ship.Banner;
		BannerDebugInfo val = BannerDebugInfo.CreateManual(((object)this).GetType().Name);
		BannerVisualExtensions.GetTableauTextureLarge(banner, ref val, (Action<Texture>)delegate(Texture tex)
		{
			OnCaptureBannerTextureRendered(tex, ship);
		});
	}

	private void OnCaptureBannerTextureRendered(Texture newTexture, MissionShip ship)
	{
		foreach (MissionSail item in (List<MissionSail>)(object)ship.Sails)
		{
			item.StartShipCaptureAnimation(newTexture);
		}
	}
}
