using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using NavalDLC.Missions.Objects;
using NavalDLC.View.Map.Visuals;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.View;

public class NavalDLCViewHelpers
{
	public static class ShipVisualHelper
	{
		private const string BannerTag = "banner_with_faction_color";

		private const float AnimationSpeedMultiplier = 0.1f;

		public static GameEntity GetFlagshipEntity(PartyBase party, Scene scene)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (((List<Ship>)(object)party.Ships).Count > 0)
			{
				Ship flagShip = party.FlagShip;
				return GetShipEntityForCampaign(flagShip, scene, flagShip.GetShipVisualSlotInfos());
			}
			float num = 0.4f;
			MatrixFrame identity = MatrixFrame.Identity;
			GameEntity obj = GameEntity.CreateEmpty(scene, true, true, true);
			obj.AddMultiMesh(MetaMesh.GetCopy("boat_sail_on", true, false), true);
			((Mat3)(ref identity.rotation)).ApplyScaleLocal(num);
			obj.SetFrame(ref identity, true);
			return obj;
		}

		public static GameEntity GetShipEntity(Ship ship, Scene scene, List<ShipVisualSlotInfo> selectedPieces, bool createPhysics = false)
		{
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			MissionShipObject obj = MBObjectManager.Instance.GetObject<MissionShipObject>(ship.ShipHull.MissionShipObjectId);
			int randomValue = ship.RandomValue;
			float mapVisualScale = ship.ShipHull.MapVisualScale;
			string obj2 = ((obj != null) ? obj.Prefab : null);
			ValueTuple<uint, uint> sailColors = ShipHelper.GetSailColors((IShipOrigin)(object)ship, (IAgent)null);
			uint item = sailColors.Item1;
			uint item2 = sailColors.Item2;
			GameEntity val = VisualShipFactory.CreateVisualShip(obj2, scene, selectedPieces, randomValue, ship.HitPoints / ship.MaxHitPoints, item, item2, createPhysics);
			ShipVisual firstScriptOfType = val.GetFirstScriptOfType<ShipVisual>();
			if (firstScriptOfType != null)
			{
				foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
				{
					if (sailVisual2 is SailVisual sailVisual && sailVisual.SailTopBannerEntity != (GameEntity)null && sailVisual.SailTopBannerEntity.HasTag("banner_with_faction_color"))
					{
						SetBanner(sailVisual.SailTopBannerEntity, ShipHelper.GetShipBanner((IShipOrigin)(object)ship, (IAgent)null));
					}
				}
			}
			if (val != null)
			{
				GameEntityPhysicsExtensions.SetPhysicsState(val, false, true);
			}
			val.SetBodyFlags((BodyFlags)144);
			MatrixFrame identity = MatrixFrame.Identity;
			((Mat3)(ref identity.rotation)).ApplyScaleLocal(mapVisualScale);
			val.SetFrame(ref identity, true);
			return val;
		}

		public static GameEntity GetShipEntityForCampaign(Ship ship, Scene scene, List<ShipVisualSlotInfo> selectedPieces)
		{
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			MissionShipObject obj = MBObjectManager.Instance.GetObject<MissionShipObject>(ship.ShipHull.MissionShipObjectId);
			int randomValue = ship.RandomValue;
			string customSailPatternId = ship.CustomSailPatternId;
			float mapVisualScale = ship.ShipHull.MapVisualScale;
			string obj2 = ((obj != null) ? obj.Prefab : null);
			ValueTuple<uint, uint> sailColors = ShipHelper.GetSailColors((IShipOrigin)(object)ship, (IAgent)null);
			uint item = sailColors.Item1;
			uint item2 = sailColors.Item2;
			GameEntity val = VisualShipFactory.CreateVisualShipForCampaign(obj2, scene, selectedPieces, randomValue, customSailPatternId, item, item2);
			ShipVisual firstScriptOfType = val.GetFirstScriptOfType<ShipVisual>();
			if (firstScriptOfType != null)
			{
				foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
				{
					if (sailVisual2 is SailVisual sailVisual && sailVisual.SailTopBannerEntity != (GameEntity)null && sailVisual.SailTopBannerEntity.HasTag("banner_with_faction_color"))
					{
						SetBanner(sailVisual.SailTopBannerEntity, ShipHelper.GetShipBanner((IShipOrigin)(object)ship, (IAgent)null));
					}
				}
			}
			if (val != null)
			{
				GameEntityPhysicsExtensions.SetPhysicsState(val, false, true);
			}
			val.SetBodyFlags((BodyFlags)144);
			MatrixFrame identity = MatrixFrame.Identity;
			((Mat3)(ref identity.rotation)).ApplyScaleLocal(mapVisualScale);
			val.SetFrame(ref identity, true);
			return val;
		}

		public static void CollectSailVisuals(WeakGameEntity shipEntity, List<SailVisual> sailVisuals)
		{
			sailVisuals.Clear();
			ShipVisual firstScriptOfType = ((WeakGameEntity)(ref shipEntity)).GetFirstScriptOfType<ShipVisual>();
			if (firstScriptOfType == null)
			{
				return;
			}
			foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
			{
				if (sailVisual2 is SailVisual sailVisual)
				{
					sailVisual.SailEnabled = false;
					sailVisual.SetFoldSailStepMultiplier(0.3f);
					sailVisual.SetFoldSailDuration(0.4f);
					sailVisual.SetUnfoldSailDuration(0.2f);
					sailVisual.FoldAnimationEnabled = false;
					sailVisuals.Add(sailVisual);
				}
			}
		}

		public static void FoldSails(List<SailVisual> sailVisuals)
		{
			foreach (SailVisual sailVisual in sailVisuals)
			{
				sailVisual.SailEnabled = false;
			}
		}

		public static void UnfoldSails(List<SailVisual> sailVisuals)
		{
			foreach (SailVisual sailVisual in sailVisuals)
			{
				sailVisual.SailEnabled = true;
			}
		}

		public static void RefreshShipVisuals(WeakGameEntity shipEntity, Ship ship, List<SailVisual> sailVisuals)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			VisualShipFactory.RefreshUpgrades(shipEntity, ship.GetShipVisualSlotInfos());
			(uint, uint) sailColors = ShipHelper.GetSailColors((IShipOrigin)(object)ship, (IAgent)null);
			foreach (SailVisual sailVisual in sailVisuals)
			{
				sailVisual.ShipVisual.SailColors = sailColors;
				sailVisual.ShipVisual.Health = ship.HitPoints / ship.MaxHitPoints;
				sailVisual.RefreshSailVisual();
			}
			UpdateBanner(ShipHelper.GetShipBanner((IShipOrigin)(object)ship, (IAgent)null), sailVisuals);
			foreach (Mesh item in ((WeakGameEntity)(ref shipEntity)).GetAllMeshesWithTag("faction_color"))
			{
				item.Color = sailColors.Item1;
				item.Color2 = sailColors.Item2;
			}
		}

		public static void RefreshShipVisuals(GameEntity shipEntity, List<ShipVisualSlotInfo> selectedPieces, uint sailColor1, uint sailColor2, Banner banner, float healthPercent)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			VisualShipFactory.RefreshUpgrades(shipEntity.WeakEntity, selectedPieces);
			ShipVisual firstScriptOfType = shipEntity.GetFirstScriptOfType<ShipVisual>();
			if (firstScriptOfType != null)
			{
				firstScriptOfType.SailColors = (sailColor1, sailColor2);
				firstScriptOfType.Health = healthPercent;
				foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
				{
					if (sailVisual2 is SailVisual sailVisual)
					{
						if (sailVisual.SailTopBannerEntity != (GameEntity)null && sailVisual.SailTopBannerEntity.HasTag("banner_with_faction_color"))
						{
							SetBanner(sailVisual.SailTopBannerEntity, banner);
						}
						sailVisual.RefreshSailVisual();
					}
				}
			}
			WeakGameEntity weakEntity = shipEntity.WeakEntity;
			foreach (Mesh item in ((WeakGameEntity)(ref weakEntity)).GetAllMeshesWithTag("faction_color"))
			{
				item.Color = sailColor1;
				item.Color2 = sailColor2;
			}
		}

		private static void UpdateBanner(Banner banner, List<SailVisual> sailVisuals)
		{
			foreach (SailVisual sailVisual in sailVisuals)
			{
				if (sailVisual.SailTopBannerEntity != (GameEntity)null && sailVisual.SailTopBannerEntity.HasTag("banner_with_faction_color"))
				{
					SetBanner(sailVisual.SailTopBannerEntity, banner, isUpdated: true);
				}
			}
		}

		private static void SetBanner(GameEntity bannerEntity, Banner banner, bool isUpdated = false)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			BannerDebugInfo val = BannerDebugInfo.CreateManual("SetBanner");
			BannerVisualExtensions.GetTableauTextureLarge(banner, ref val, (Action<Texture>)onTextureRendered);
			void onTextureRendered(Texture tex)
			{
				if ((NativeObject)(object)bannerEntity.Scene != (NativeObject)null)
				{
					List<Mesh> list = bannerEntity.GetAllMeshesWithTag("banner_with_faction_color").ToList();
					if (Extensions.IsEmpty<Mesh>((IEnumerable<Mesh>)list) && (NativeObject)(object)bannerEntity.GetFirstMesh() != (NativeObject)null)
					{
						list.Add(bannerEntity.GetFirstMesh());
					}
					foreach (Mesh item in list)
					{
						Material material = item.GetMaterial();
						Material val2 = (isUpdated ? material : material.CreateCopy());
						val2.SetTexture((MBTextureType)1, tex);
						uint num = (uint)val2.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
						ulong shaderFlags = val2.GetShaderFlags();
						val2.SetShaderFlags(shaderFlags | num);
						item.SetMaterial(val2);
					}
				}
			}
		}
	}

	public static class BannerVisualHelper
	{
		public static MetaMesh GetBannerOfCharacter(Banner banner, string bannerMeshName)
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			MetaMesh copy = MetaMesh.GetCopy(bannerMeshName, true, false);
			for (int i = 0; i < copy.MeshCount; i++)
			{
				Mesh meshAtIndex = copy.GetMeshAtIndex(i);
				if (meshAtIndex.HasTag("dont_use_tableau"))
				{
					continue;
				}
				Material material = meshAtIndex.GetMaterial();
				Material tableauMaterial = null;
				Tuple<Material, Banner> key = new Tuple<Material, Banner>(material, banner);
				if (MapScreen.Instance.CharacterBannerMaterialCache.ContainsKey(key))
				{
					tableauMaterial = MapScreen.Instance.CharacterBannerMaterialCache[key];
				}
				else
				{
					tableauMaterial = material.CreateCopy();
					Action<Texture> action = delegate(Texture tex)
					{
						tableauMaterial.SetTexture((MBTextureType)1, tex);
						uint num = (uint)tableauMaterial.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
						ulong shaderFlags = tableauMaterial.GetShaderFlags();
						tableauMaterial.SetShaderFlags(shaderFlags | num);
					};
					BannerDebugInfo val = BannerDebugInfo.CreateManual("GetBannerOfCharacter");
					BannerVisualExtensions.GetTableauTextureLarge(banner, ref val, action);
					MapScreen.Instance.CharacterBannerMaterialCache[key] = tableauMaterial;
				}
				meshAtIndex.SetMaterial(tableauMaterial);
			}
			return copy;
		}
	}

	public static class BlockadeVisualHelper
	{
		private const float AnimationSpeedMultiplier = 0.1f;

		public static List<Vec3> GetPositionsOnBlockadeArc(Settlement settlement, int numberOfArcs, int numberOfPositions, float angle, float distanceBetweenArcs)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			CampaignVec2 portPosition = settlement.PortPosition;
			CampaignVec2 val = settlement.PortPosition;
			Vec2 val2 = ((CampaignVec2)(ref val)).ToVec2();
			val = settlement.Position;
			Vec2 val3 = val2 - ((CampaignVec2)(ref val)).ToVec2();
			List<Vec3> list = new List<Vec3>();
			Vec2 val4 = ((Vec2)(ref val3)).Normalized();
			((Vec2)(ref val4)).RotateCCW((0f - angle) / 2f);
			Vec2 val5 = val4;
			for (int i = 1; numberOfArcs >= i; i++)
			{
				if (numberOfPositions <= 0)
				{
					break;
				}
				int num = MathF.Min(i, numberOfPositions);
				for (int j = 0; j < num; j++)
				{
					val = ((i == 1) ? portPosition : (portPosition + val5 * (float)(i - 1) * distanceBetweenArcs));
					Vec3 item = ((CampaignVec2)(ref val)).AsVec3();
					((Vec2)(ref val5)).RotateCCW(angle / (float)MathF.Max(1, num - 1));
					list.Add(item);
				}
				val5 = val4;
				numberOfPositions -= i;
			}
			return list;
		}

		public static void AddBlockadeVisuals(Dictionary<Ship, NavalMobilePartyVisual.BlockadeShipVisual> shipToBlockadeShipVisualCache, PartyBase party, GameEntity strategicEntity)
		{
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_031e: Unknown result type (might be due to invalid IL or missing references)
			//IL_032a: Unknown result type (might be due to invalid IL or missing references)
			int num = 0;
			int num2 = 0;
			SiegeEvent siegeEvent = party.MobileParty.SiegeEvent;
			Settlement besiegedSettlement = siegeEvent.BesiegedSettlement;
			BlockadePositionScript firstScriptOfType = SettlementVisualManager.Current.GetSettlementVisual(besiegedSettlement).StrategicEntity.GetFirstScriptOfType<BlockadePositionScript>();
			IEnumerable<PartyBase> involvedPartiesForEventType = siegeEvent.BesiegerCamp.GetInvolvedPartiesForEventType((BattleTypes)5);
			MobileParty leaderParty = siegeEvent.BesiegerCamp.LeaderParty;
			if (firstScriptOfType == null)
			{
				return;
			}
			if (!Extensions.IsEmpty<KeyValuePair<Ship, NavalMobilePartyVisual.BlockadeShipVisual>>((IEnumerable<KeyValuePair<Ship, NavalMobilePartyVisual.BlockadeShipVisual>>)shipToBlockadeShipVisualCache))
			{
				foreach (KeyValuePair<Ship, NavalMobilePartyVisual.BlockadeShipVisual> item in shipToBlockadeShipVisualCache)
				{
					item.Value.ShipEntity.SetVisibilityExcludeParents(false);
				}
			}
			Vec3 centerOfArc = default(Vec3);
			List<List<Vec3>> blockadeArc = firstScriptOfType.GetBlockadeArc(involvedPartiesForEventType.Sum((PartyBase p) => ((List<Ship>)(object)p.Ships).Count), ref centerOfArc);
			int num3 = ((((List<Ship>)(object)leaderParty.Ships).Count > 0) ? (blockadeArc[0].Count / 2) : (-1));
			foreach (PartyBase item2 in involvedPartiesForEventType)
			{
				if (num == blockadeArc.Count)
				{
					break;
				}
				if (Extensions.IsEmpty<Ship>((IEnumerable<Ship>)item2.Ships))
				{
					continue;
				}
				Ship flagShip = item2.FlagShip;
				if (leaderParty.Party == item2)
				{
					if (item2 == party)
					{
						if (!shipToBlockadeShipVisualCache.TryGetValue(flagShip, out var value))
						{
							value = (shipToBlockadeShipVisualCache[flagShip] = CreateBlockadeShipVisual(ShipVisualHelper.GetFlagshipEntity(item2, strategicEntity.Scene)));
						}
						InitializeBlockadeVisual(blockadeArc[0][num3], value.ShipEntity, centerOfArc);
					}
				}
				else
				{
					if (num2 == num3 && num == 0)
					{
						num2++;
					}
					if (num2 < blockadeArc[num].Count && item2 == party)
					{
						if (!shipToBlockadeShipVisualCache.TryGetValue(flagShip, out var value2))
						{
							value2 = (shipToBlockadeShipVisualCache[flagShip] = CreateBlockadeShipVisual(ShipVisualHelper.GetFlagshipEntity(item2, strategicEntity.Scene)));
						}
						InitializeBlockadeVisual(blockadeArc[num][num2], value2.ShipEntity, centerOfArc);
					}
					num2++;
				}
				if (num2 >= blockadeArc[num].Count)
				{
					num++;
					num2 = 0;
				}
			}
			if (num >= blockadeArc.Count)
			{
				return;
			}
			foreach (PartyBase item3 in involvedPartiesForEventType)
			{
				if (num == blockadeArc.Count)
				{
					break;
				}
				if (((IEnumerable<Ship>)item3.Ships).Count() <= 1)
				{
					continue;
				}
				foreach (Ship item4 in (item3 == party) ? ((List<Ship>)(object)Extensions.ToMBList<Ship>((IEnumerable<Ship>)((IEnumerable<Ship>)item3.Ships).OrderByDescending((Ship x) => x.FlagshipScore))) : ((List<Ship>)(object)item3.Ships))
				{
					if (num == blockadeArc.Count)
					{
						break;
					}
					if (item4 == item3.FlagShip)
					{
						continue;
					}
					if (num2 == num3 && num == 0)
					{
						num2++;
					}
					if (item3 == party)
					{
						if (!shipToBlockadeShipVisualCache.TryGetValue(item4, out var value3))
						{
							value3 = (shipToBlockadeShipVisualCache[item4] = CreateBlockadeShipVisual(ShipVisualHelper.GetShipEntityForCampaign(item4, strategicEntity.Scene, item4.GetShipVisualSlotInfos())));
						}
						InitializeBlockadeVisual(blockadeArc[num][num2], value3.ShipEntity, centerOfArc);
					}
					num2++;
					if (num2 >= blockadeArc[num].Count)
					{
						num++;
						num2 = 0;
					}
				}
			}
		}

		private static NavalMobilePartyVisual.BlockadeShipVisual CreateBlockadeShipVisual(GameEntity shipEntity)
		{
			return new NavalMobilePartyVisual.BlockadeShipVisual
			{
				ShipEntity = shipEntity,
				RockingPhase = MBRandom.RandomFloatRanged(-MathF.PI, MathF.PI)
			};
		}

		private static void InitializeBlockadeVisual(Vec3 position, GameEntity shipEntity, Vec3 centerOfArc)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			Vec2 asVec = ((Vec3)(ref position)).AsVec2;
			Vec2 val = asVec - ((Vec3)(ref centerOfArc)).AsVec2;
			MatrixFrame frame = shipEntity.GetFrame();
			CampaignVec2 val2 = default(CampaignVec2);
			((CampaignVec2)(ref val2))._002Ector(asVec, false);
			Vec3 val3 = ((CampaignVec2)(ref val2)).AsVec3();
			position.z = ((Vec3)(ref val3)).Z;
			frame.origin = position;
			float num = ((Vec2)(ref val)).AngleBetween(((Vec3)(ref frame.rotation.f)).AsVec2);
			((MatrixFrame)(ref frame)).Rotate(MathF.PI / 2f - num, ref Vec3.Up);
			shipEntity.SetFrame(ref frame, true);
			shipEntity.SetVisibilityExcludeParents(true);
			ShipVisual firstScriptOfType = shipEntity.GetFirstScriptOfType<ShipVisual>();
			if (firstScriptOfType == null)
			{
				return;
			}
			foreach (ScriptComponentBehavior sailVisual2 in firstScriptOfType.SailVisuals)
			{
				if (sailVisual2 is SailVisual sailVisual)
				{
					sailVisual.SailEnabled = false;
					sailVisual.SetFoldSailStepMultiplier(0.3f);
					sailVisual.SetFoldSailDuration(0.4f);
					sailVisual.SetUnfoldSailDuration(0.2f);
					sailVisual.FoldAnimationEnabled = false;
				}
			}
		}
	}
}
