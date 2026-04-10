using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View;

public class AgentVisuals : IAgentVisual
{
	public const float RandomGlossinessRange = 0.05f;

	public const float RandomClothingColor1HueRange = 4f;

	public const float RandomClothingColor1SaturationRange = 0.2f;

	public const float RandomClothingColor1BrightnessRange = 0.2f;

	public const float RandomClothingColor2HueRange = 8f;

	public const float RandomClothingColor2SaturationRange = 0.5f;

	public const float RandomClothingColor2BrightnessRange = 0.3f;

	private AgentVisualsData _data;

	private float _scale;

	public bool IsFemale
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Invalid comparison between Unknown and I4
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			if ((int)_data.SkeletonTypeData != 1 && (int)_data.SkeletonTypeData != 5 && (int)_data.SkeletonTypeData != 6)
			{
				return (int)_data.SkeletonTypeData == 7;
			}
			return true;
		}
	}

	public MBAgentVisuals GetVisuals()
	{
		return _data.AgentVisuals;
	}

	public void Reset()
	{
		_data.AgentVisuals.Reset();
	}

	public void ResetNextFrame()
	{
		_data.AgentVisuals.ResetNextFrame();
	}

	public MatrixFrame GetFrame()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _data.FrameData;
	}

	public BodyProperties GetBodyProperties()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _data.BodyPropertiesData;
	}

	public void SetBodyProperties(BodyProperties bodyProperties)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_data.BodyProperties(bodyProperties);
	}

	public bool GetIsFemale()
	{
		return IsFemale;
	}

	public string GetCharacterObjectID()
	{
		return _data.CharacterObjectStringIdData;
	}

	public void SetCharacterObjectID(string id)
	{
		_data.CharacterObjectStringId(id);
	}

	public Equipment GetEquipment()
	{
		return _data.EquipmentData;
	}

	private AgentVisuals(AgentVisualsData data, string name, bool isRandomProgress, bool needBatchedVersionForWeaponMeshes, bool forceUseFaceCache)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_data = data;
		_data.AgentVisuals = MBAgentVisuals.CreateAgentVisuals(_data.SceneData, name, data.MonsterData.EyeOffsetWrtHead);
		if (data.EntityData != (GameEntity)null)
		{
			_data.AgentVisuals.SetEntity(data.EntityData);
		}
		_scale = ((_data.ScaleData <= 1E-05f) ? 1f : _data.ScaleData);
		Refresh(needBatchedVersionForWeaponMeshes, removeSkeleton: false, null, isRandomProgress, forceUseFaceCache);
	}

	public AgentVisualsData GetCopyAgentVisualsData()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new AgentVisualsData(_data);
	}

	public GameEntity GetEntity()
	{
		return _data.AgentVisuals.GetEntity();
	}

	public WeakGameEntity GetWeakEntity()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return _data.AgentVisuals.GetWeakEntity();
	}

	public void SetVisible(bool value)
	{
		_data.AgentVisuals.SetVisible(value);
	}

	public Vec3 GetGlobalStableEyePoint(bool isHumanoid)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return _data.AgentVisuals.GetGlobalStableEyePoint(isHumanoid);
	}

	public Vec3 GetGlobalStableNeckPoint(bool isHumanoid)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return _data.AgentVisuals.GetGlobalStableNeckPoint(isHumanoid);
	}

	public CompositeComponent AddPrefabToAgentVisualBoneByBoneType(string prefabName, HumanBone boneType)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return _data.AgentVisuals.AddPrefabToAgentVisualBoneByBoneType(prefabName, boneType);
	}

	public CompositeComponent AddPrefabToAgentVisualBoneByRealBoneIndex(string prefabName, sbyte realBoneIndex)
	{
		return _data.AgentVisuals.AddPrefabToAgentVisualBoneByRealBoneIndex(prefabName, realBoneIndex);
	}

	public void SetAgentLodZeroOrMax(bool value)
	{
		_data.AgentVisuals.SetAgentLodZeroOrMax(value);
	}

	public float GetScale()
	{
		return _scale;
	}

	public void SetAction(in ActionIndexCache actionIndex, float startProgress = 0f, bool forceFaceMorphRestart = true)
	{
		if ((NativeObject)(object)_data.AgentVisuals != (NativeObject)null)
		{
			Skeleton skeleton = _data.AgentVisuals.GetSkeleton();
			if ((NativeObject)(object)skeleton != (NativeObject)null)
			{
				MBSkeletonExtensions.SetAgentActionChannel(skeleton, 0, ref actionIndex, startProgress, -0.2f, forceFaceMorphRestart, 0f);
				((NativeObject)skeleton).ManualInvalidate();
			}
		}
	}

	public bool DoesActionContinueWithCurrentAction(in ActionIndexCache actionIndex)
	{
		bool result = false;
		if ((NativeObject)(object)_data.AgentVisuals != (NativeObject)null)
		{
			Skeleton skeleton = _data.AgentVisuals.GetSkeleton();
			if ((NativeObject)(object)skeleton != (NativeObject)null)
			{
				result = MBSkeletonExtensions.DoesActionContinueWithCurrentActionAtChannel(skeleton, 0, ref actionIndex);
			}
		}
		return result;
	}

	public float GetAnimationParameterAtChannel(int channelIndex)
	{
		float result = 0f;
		if ((NativeObject)(object)_data.AgentVisuals != (NativeObject)null && (NativeObject)(object)_data.AgentVisuals.GetSkeleton() != (NativeObject)null)
		{
			result = _data.AgentVisuals.GetSkeleton().GetAnimationParameterAtChannel(channelIndex);
		}
		return result;
	}

	public void Refresh(bool needBatchedVersionForWeaponMeshes, AgentVisualsData data, bool forceUseFaceCache = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		AgentVisualsData data2 = _data;
		_data = data;
		bool removeSkeleton = data2.SkeletonTypeData != _data.SkeletonTypeData;
		Equipment equipmentData = _data.EquipmentData;
		Refresh(needBatchedVersionForWeaponMeshes, removeSkeleton, equipmentData, isRandomProgress: false, forceUseFaceCache);
	}

	public void SetClothWindToWeaponAtIndex(Vec3 localWindVector, bool isLocal, EquipmentIndex weaponIndex)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_data.AgentVisuals.SetClothWindToWeaponAtIndex(localWindVector, isLocal, weaponIndex);
	}

	private void Refresh(bool needBatchedVersionForWeaponMeshes, bool removeSkeleton = false, Equipment oldEquipment = null, bool isRandomProgress = false, bool forceUseFaceCache = false)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		string text = "";
		bool flag = Extensions.HasAnyFlag<AgentFlag>(_data.MonsterData.Flags, (AgentFlag)2048);
		Skeleton skeleton = _data.AgentVisuals.GetSkeleton();
		float num3 = -0.2f;
		MBActionSet actionSetData;
		ActionIndexCache val;
		if ((NativeObject)(object)skeleton != (NativeObject)null)
		{
			actionSetData = _data.ActionSetData;
			if (((MBActionSet)(ref actionSetData)).IsValid)
			{
				num = skeleton.GetAnimationParameterAtChannel(0);
				val = MBSkeletonExtensions.GetActionAtChannel(skeleton, 0);
				num3 = 0f;
				if (flag)
				{
					num2 = MBSkeletonExtensions.GetSkeletonFaceAnimationTime(skeleton);
					text = MBSkeletonExtensions.GetSkeletonFaceAnimationName(skeleton);
				}
				goto IL_00a2;
			}
		}
		val = _data.ActionCodeData;
		goto IL_00a2;
		IL_00a2:
		if ((NativeObject)(object)skeleton != (NativeObject)null)
		{
			((NativeObject)skeleton).ManualInvalidate();
		}
		_data.AgentVisuals.SetSetupMorphNode(_data.UseMorphAnimsData);
		_data.AgentVisuals.UseScaledWeapons(_data.UseScaledWeaponsData);
		MatrixFrame frameData = _data.FrameData;
		_scale = ((_data.ScaleData == 0f) ? MBBodyProperties.GetScaleFromKey(_data.RaceData, IsFemale ? 1 : 0, _data.BodyPropertiesData) : _data.ScaleData);
		((Mat3)(ref frameData.rotation)).ApplyScaleLocal(_scale);
		_data.AgentVisuals.SetFrame(ref frameData);
		bool num4 = !removeSkeleton && (NativeObject)(object)skeleton != (NativeObject)null && oldEquipment != null;
		bool flag2 = false;
		if (num4)
		{
			flag2 = ClearAndAddChangedVisualComponentsOfWeapons(oldEquipment, needBatchedVersionForWeaponMeshes);
		}
		if (!num4 || !flag2)
		{
			_data.AgentVisuals.ClearVisualComponents(false, false);
			actionSetData = _data.ActionSetData;
			if (((MBActionSet)(ref actionSetData)).IsValid && text != "facegen_teeth")
			{
				AnimationSystemData val2 = MonsterExtensions.FillAnimationSystemData(_data.MonsterData, _data.ActionSetData, 1f, _data.HasClippingPlaneData);
				Skeleton val3 = MBSkeletonExtensions.CreateWithActionSet(ref val2);
				_data.AgentVisuals.SetSkeleton(val3);
				((NativeObject)val3).ManualInvalidate();
			}
			if (_data.EquipmentData == null)
			{
				int mask = 481;
				AddSkinMeshesToEntity(mask, !needBatchedVersionForWeaponMeshes, forceUseFaceCache);
			}
			else if (!string.IsNullOrEmpty(_data.MountCreationKeyData) || !flag)
			{
				GameEntity entity = GetEntity();
				EquipmentElement val4 = _data.EquipmentData[(EquipmentIndex)10];
				ItemObject item = ((EquipmentElement)(ref val4)).Item;
				val4 = _data.EquipmentData[(EquipmentIndex)11];
				MountVisualCreator.AddMountMeshToEntity(entity, item, ((EquipmentElement)(ref val4)).Item, _data.MountCreationKeyData, out var mountVisualCreationOutput);
				val4 = _data.EquipmentData[(EquipmentIndex)11];
				ItemObject item2 = ((EquipmentElement)(ref val4)).Item;
				if (item2 != null && item2.IsUsingTeamColor && (NativeObject)(object)mountVisualCreationOutput.MountHarnessMesh != (NativeObject)null)
				{
					AddTeamColorToMesh(mountVisualCreationOutput.MountHarnessMesh, _data.ClothColor1Data, _data.ClothColor2Data);
				}
			}
			else
			{
				AddSkinArmorWeaponMultiMeshesToEntity(_data.ClothColor1Data, _data.ClothColor2Data, needBatchedVersionForWeaponMeshes, forceUseFaceCache);
			}
		}
		actionSetData = _data.ActionSetData;
		if (!((MBActionSet)(ref actionSetData)).IsValid || !(val != ActionIndexCache.act_none))
		{
			return;
		}
		if (isRandomProgress)
		{
			num = MBRandom.RandomFloat;
		}
		skeleton = _data.AgentVisuals.GetSkeleton();
		if ((NativeObject)(object)skeleton != (NativeObject)null)
		{
			MBSkeletonExtensions.SetAgentActionChannel(skeleton, 0, ref val, num, num3, true, 0f);
			if (num2 > 0f)
			{
				MBSkeletonExtensions.SetSkeletonFaceAnimationTime(skeleton, num2);
			}
			((NativeObject)skeleton).ManualInvalidate();
		}
	}

	public void TickVisuals()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		MBActionSet actionSetData = _data.ActionSetData;
		if (((MBActionSet)(ref actionSetData)).IsValid)
		{
			MBSkeletonExtensions.TickActionChannels(_data.AgentVisuals.GetSkeleton());
		}
	}

	public void Tick(AgentVisuals parentAgentVisuals, float dt, bool isEntityMoving = false, float speed = 0f)
	{
		_data.AgentVisuals.Tick(parentAgentVisuals?._data.AgentVisuals, dt, isEntityMoving, speed);
	}

	public static AgentVisuals Create(AgentVisualsData data, string name, bool isRandomProgress, bool needBatchedVersionForWeaponMeshes, bool forceUseFaceCache)
	{
		return new AgentVisuals(data, name, isRandomProgress, needBatchedVersionForWeaponMeshes, forceUseFaceCache);
	}

	public static float GetRandomGlossFactor(Random randomGenerator)
	{
		return 1f + (Extensions.NextFloat(randomGenerator) * 2f - 1f) * 0.05f;
	}

	public static void GetRandomClothingColors(int seed, Color inputColor1, Color inputColor2, out Color color1, out Color color2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		MBFastRandom val = new MBFastRandom((uint)seed);
		color1 = ColorExtensions.AddFactorInHSB(inputColor1, (2f * val.NextFloat() - 1f) * 4f, (2f * val.NextFloat() - 1f) * 0.2f, (2f * val.NextFloat() - 1f) * 0.2f);
		color2 = ColorExtensions.AddFactorInHSB(inputColor2, (2f * val.NextFloat() - 1f) * 8f, (2f * val.NextFloat() - 1f) * 0.5f, (2f * val.NextFloat() - 1f) * 0.3f);
	}

	private void AddSkinArmorWeaponMultiMeshesToEntity(uint teamColor1, uint teamColor2, bool needBatchedVersion, bool forceUseFaceCache = false)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected I4, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		AddSkinMeshesToEntity((int)MBEquipmentMissionExtensions.GetSkinMeshesMask(_data.EquipmentData), !needBatchedVersion, forceUseFaceCache);
		AddArmorMultiMeshesToAgentEntity(teamColor1, teamColor2);
		int hashCode = ((object)_data.BodyPropertiesData/*cast due to .constrained prefix*/).GetHashCode();
		EquipmentElement val;
		MissionWeapon val2 = default(MissionWeapon);
		for (int i = 0; i < 5; i++)
		{
			val = _data.EquipmentData[i];
			if (!((EquipmentElement)(ref val)).IsEmpty)
			{
				val = _data.EquipmentData[i];
				ItemObject item = ((EquipmentElement)(ref val)).Item;
				val = _data.EquipmentData[i];
				((MissionWeapon)(ref val2))._002Ector(item, ((EquipmentElement)(ref val)).ItemModifier, _data.BannerData);
				if (_data.AddColorRandomnessData)
				{
					((MissionWeapon)(ref val2)).SetRandomGlossMultiplier(hashCode);
				}
				WeaponData weaponData = ((MissionWeapon)(ref val2)).GetWeaponData(needBatchedVersion);
				WeaponData ammoWeaponData = ((MissionWeapon)(ref val2)).GetAmmoWeaponData(needBatchedVersion);
				_data.AgentVisuals.AddWeaponToAgentEntity(i, ref weaponData, ((MissionWeapon)(ref val2)).GetWeaponStatsData(), ref ammoWeaponData, ((MissionWeapon)(ref val2)).GetAmmoWeaponStatsData(), _data.GetCachedWeaponEntity((EquipmentIndex)i));
				((WeaponData)(ref weaponData)).DeinitializeManagedPointers();
				((WeaponData)(ref ammoWeaponData)).DeinitializeManagedPointers();
			}
		}
		_data.AgentVisuals.SetWieldedWeaponIndices(_data.RightWieldedItemIndexData, _data.LeftWieldedItemIndexData);
		for (int j = 0; j < 5; j++)
		{
			val = _data.EquipmentData[j];
			if (((EquipmentElement)(ref val)).IsEmpty)
			{
				continue;
			}
			val = _data.EquipmentData[j];
			if (((EquipmentElement)(ref val)).Item.PrimaryWeapon.IsConsumable)
			{
				val = _data.EquipmentData[j];
				short num = ((EquipmentElement)(ref val)).Item.PrimaryWeapon.MaxDataValue;
				if (j == _data.RightWieldedItemIndexData)
				{
					num--;
				}
				_data.AgentVisuals.UpdateQuiverMeshesWithoutAgent(j, (int)num);
			}
		}
	}

	private void AddSkinMeshesToEntity(int mask, bool useGPUMorph, bool forceUseFaceCache = false)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Expected I4, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected I4, but got Unknown
		//IL_00d2: Expected I4, but got Unknown
		//IL_00d2: Expected I4, but got Unknown
		//IL_00d2: Expected I4, but got Unknown
		//IL_00d2: Expected I4, but got Unknown
		SkinGenerationParams val = default(SkinGenerationParams);
		if (_data.EquipmentData != null)
		{
			BodyProperties bodyPropertiesData = _data.BodyPropertiesData;
			bool flag = ((BodyProperties)(ref bodyPropertiesData)).Age >= 14f && (int)_data.SkeletonTypeData == 1;
			((SkinGenerationParams)(ref val))._002Ector(mask, _data.EquipmentData.GetUnderwearType(flag), (int)_data.EquipmentData.BodyMeshType, (int)_data.EquipmentData.HairCoverType, (int)_data.EquipmentData.BeardCoverType, (int)_data.EquipmentData.BodyDeformType, _data.PrepareImmediatelyData, 0f, (int)_data.SkeletonTypeData, _data.RaceData, _data.UseTranslucencyData, _data.UseTesselationData, 0);
		}
		else
		{
			((SkinGenerationParams)(ref val))._002Ector(mask, (UnderwearTypes)1, 0, 4, 0, 0, _data.PrepareImmediatelyData, 0f, (int)_data.SkeletonTypeData, _data.RaceData, _data.UseTranslucencyData, _data.UseTesselationData, 0);
		}
		if (_data.CharacterObjectStringIdData != null)
		{
			MBObjectManager.Instance.GetObject<BasicCharacterObject>(_data.CharacterObjectStringIdData);
		}
		_data.AgentVisuals.AddSkinMeshes(val, _data.BodyPropertiesData, useGPUMorph, false);
	}

	public void SetFaceGenerationParams(FaceGenerationParams faceGenerationParams)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_data.AgentVisuals.SetFaceGenerationParams(faceGenerationParams);
	}

	public void SetVoiceDefinitionIndex(int voiceDefinitionIndex, float voicePitch)
	{
		_data.AgentVisuals.SetVoiceDefinitionIndex(voiceDefinitionIndex, voicePitch);
	}

	public void StartRhubarbRecord(string path, int soundId)
	{
		_data.AgentVisuals.StartRhubarbRecord(path, soundId);
	}

	public void SetAgentLodZeroOrMaxExternal(bool makeZero)
	{
		_data.AgentVisuals.SetAgentLodZeroOrMax(makeZero);
	}

	public void SetAgentLocalSpeed(Vec2 speed)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_data.AgentVisuals.SetAgentLocalSpeed(speed);
	}

	public void SetLookDirection(Vec3 direction)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_data.AgentVisuals.SetLookDirection(direction);
	}

	public unsafe void AddArmorMultiMeshesToAgentEntity(uint teamColor1, uint teamColor2)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Invalid comparison between Unknown and I4
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected I4, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected I4, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Invalid comparison between Unknown and I4
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Invalid comparison between Unknown and I4
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Invalid comparison between Unknown and I4
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Invalid comparison between Unknown and I4
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Expected I4, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		Random randomGenerator = null;
		BodyProperties bodyPropertiesData;
		uint color3;
		uint color4;
		if (_data.AddColorRandomnessData)
		{
			bodyPropertiesData = _data.BodyPropertiesData;
			int hashCode = ((object)(*(BodyProperties*)(&bodyPropertiesData))/*cast due to .constrained prefix*/).GetHashCode();
			randomGenerator = new Random(hashCode);
			GetRandomClothingColors(hashCode, Color.FromUint(teamColor1), Color.FromUint(teamColor2), out var color, out var color2);
			color3 = ((Color)(ref color)).ToUnsignedInteger();
			color4 = ((Color)(ref color2)).ToUnsignedInteger();
		}
		else
		{
			color3 = teamColor1;
			color4 = teamColor2;
		}
		for (EquipmentIndex val = (EquipmentIndex)11; (int)val >= 0; val = (EquipmentIndex)(val - 1))
		{
			if ((int)val != 5 && (int)val != 6 && (int)val != 7 && (int)val != 8 && (int)val != 9)
			{
				continue;
			}
			EquipmentElement val2 = _data.EquipmentData[(int)val];
			ItemObject item = ((EquipmentElement)(ref val2)).Item;
			ItemObject val3 = _data.EquipmentData[(int)val].CosmeticItem ?? item;
			if (val3 == null)
			{
				continue;
			}
			bodyPropertiesData = _data.BodyPropertiesData;
			bool isFemale = ((BodyProperties)(ref bodyPropertiesData)).Age >= 14f && (int)_data.SkeletonTypeData == 1;
			int num;
			if ((int)val == 6)
			{
				val2 = _data.EquipmentData[(EquipmentIndex)8];
				if (((EquipmentElement)(ref val2)).Item != null)
				{
					val2 = _data.EquipmentData[(EquipmentIndex)8];
					num = ((!((EquipmentElement)(ref val2)).Item.ArmorComponent.IsNoSlim) ? 1 : 0);
					goto IL_013d;
				}
			}
			num = 0;
			goto IL_013d;
			IL_013d:
			bool useSlimVersion = (byte)num != 0;
			MetaMesh val4 = null;
			val4 = _data.EquipmentData[(int)val].GetMultiMesh(isFemale, useSlimVersion, needBatchedVersion: true);
			if (!((NativeObject)(object)val4 != (NativeObject)null))
			{
				continue;
			}
			if (_data.AddColorRandomnessData)
			{
				val4.SetGlossMultiplier(GetRandomGlossFactor(randomGenerator));
			}
			if (val3.IsUsingTableau && _data.BannerData != null)
			{
				for (int i = 0; i < val4.MeshCount; i++)
				{
					Mesh currentMesh = val4.GetMeshAtIndex(i);
					Mesh obj = currentMesh;
					if (obj != null && !obj.HasTag("dont_use_tableau"))
					{
						Mesh obj2 = currentMesh;
						if (obj2 != null && obj2.HasTag("banner_replacement_mesh"))
						{
							((BannerVisual)(object)_data.BannerData.BannerVisual).GetTableauTextureLarge(BannerDebugInfo.CreateManual(GetType().Name), delegate(Texture t)
							{
								ApplyBannerTextureToMesh(currentMesh, t);
							});
							((NativeObject)currentMesh).ManualInvalidate();
							break;
						}
					}
					((NativeObject)currentMesh).ManualInvalidate();
				}
			}
			else if (val3.IsUsingTeamColor)
			{
				AddTeamColorToMesh(val4, color3, color4);
			}
			if (val3.UsingFacegenScaling)
			{
				Skeleton skeleton = _data.AgentVisuals.GetSkeleton();
				val4.UseHeadBoneFaceGenScaling(skeleton, _data.MonsterData.HeadLookDirectionBoneIndex, _data.AgentVisuals.GetFacegenScalingMatrix());
				((NativeObject)skeleton).ManualInvalidate();
			}
			_data.AgentVisuals.AddMultiMesh(val4, MBAgentVisuals.GetBodyMeshIndex(val));
			((NativeObject)val4).ManualInvalidate();
		}
	}

	private void ApplyBannerTextureToMesh(Mesh armorMesh, Texture bannerTexture)
	{
		if ((NativeObject)(object)armorMesh != (NativeObject)null)
		{
			Material val = armorMesh.GetMaterial().CreateCopy();
			val.SetTexture((MBTextureType)1, bannerTexture);
			uint num = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
			ulong shaderFlags = val.GetShaderFlags();
			val.SetShaderFlags(shaderFlags | num);
			armorMesh.SetMaterial(val);
		}
	}

	public void MakeRandomVoiceForFacegen()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		GameEntity entity = _data.AgentVisuals.GetEntity();
		Vec3 origin = entity.Skeleton.GetBoneEntitialFrame(_data.MonsterData.HeadLookDirectionBoneIndex).origin;
		MatrixFrame frame = entity.GetFrame();
		Vec3 val = ((MatrixFrame)(ref frame)).TransformToParent(ref origin);
		MBSkeletonExtensions.SetAgentActionChannel(entity.Skeleton, 1, ref ActionIndexCache.act_command_leftstance, 0f, -0.2f, true, 0f);
		SkinVoiceType[] obj = new SkinVoiceType[12]
		{
			VoiceType.Yell,
			VoiceType.Victory,
			VoiceType.Charge,
			VoiceType.Advance,
			VoiceType.Stop,
			VoiceType.FallBack,
			VoiceType.UseLadders,
			VoiceType.Infantry,
			VoiceType.FireAtWill,
			VoiceType.FormLine,
			VoiceType.FormShieldWall,
			VoiceType.FormCircle
		};
		int index = ((SkinVoiceType)(ref obj[MBRandom.RandomInt(obj.Length)])).Index;
		_data.AgentVisuals.MakeVoice(index, val);
	}

	private bool ClearAndAddChangedVisualComponentsOfWeapons(Equipment oldEquipment, bool needBatchedVersionForMeshes)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		EquipmentElement val;
		for (int i = 0; i <= 3; i++)
		{
			val = oldEquipment[i];
			if (!((EquipmentElement)(ref val)).IsEqualTo(_data.EquipmentData[i]))
			{
				num++;
			}
		}
		if (num > 1)
		{
			return false;
		}
		bool flag = false;
		for (int j = 0; j <= 3; j++)
		{
			val = oldEquipment[j];
			if (!((EquipmentElement)(ref val)).IsEqualTo(_data.EquipmentData[j]))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			_data.AgentVisuals.ClearAllWeaponMeshes();
			int num2 = 0;
			int num3 = 0;
			MissionWeapon val2 = default(MissionWeapon);
			while (num2 < 5)
			{
				val = _data.EquipmentData[num2];
				if (!((EquipmentElement)(ref val)).IsEmpty)
				{
					val = _data.EquipmentData[num2];
					ItemObject item = ((EquipmentElement)(ref val)).Item;
					val = _data.EquipmentData[num2];
					((MissionWeapon)(ref val2))._002Ector(item, ((EquipmentElement)(ref val)).ItemModifier, _data.BannerData);
					if (_data.AddColorRandomnessData)
					{
						((MissionWeapon)(ref val2)).SetRandomGlossMultiplier(((object)_data.BodyPropertiesData/*cast due to .constrained prefix*/).GetHashCode());
					}
					val = _data.EquipmentData[num2];
					ItemTypeEnum ammoTypeForItemType = ItemObject.GetAmmoTypeForItemType(WeaponComponentData.GetItemTypeFromWeaponClass(((EquipmentElement)(ref val)).Item.PrimaryWeapon.WeaponClass));
					bool flag2 = false;
					MissionWeapon val3 = default(MissionWeapon);
					for (int k = 0; k < 5; k++)
					{
						val = _data.EquipmentData[k];
						if (((EquipmentElement)(ref val)).IsEmpty)
						{
							continue;
						}
						val = _data.EquipmentData[k];
						if (WeaponComponentData.GetItemTypeFromWeaponClass(((EquipmentElement)(ref val)).Item.PrimaryWeapon.WeaponClass) == ammoTypeForItemType)
						{
							flag2 = true;
							val = _data.EquipmentData[k];
							ItemObject item2 = ((EquipmentElement)(ref val)).Item;
							val = _data.EquipmentData[k];
							((MissionWeapon)(ref val3))._002Ector(item2, ((EquipmentElement)(ref val)).ItemModifier, _data.BannerData);
							if (_data.AddColorRandomnessData)
							{
								((MissionWeapon)(ref val3)).SetRandomGlossMultiplier(((object)_data.BodyPropertiesData/*cast due to .constrained prefix*/).GetHashCode());
							}
						}
					}
					WeaponData weaponData = ((MissionWeapon)(ref val2)).GetWeaponData(needBatchedVersionForMeshes);
					WeaponData val4 = (flag2 ? ((MissionWeapon)(ref val3)).GetWeaponData(needBatchedVersionForMeshes) : WeaponData.InvalidWeaponData);
					WeaponStatsData[] array = (flag2 ? ((MissionWeapon)(ref val3)).GetWeaponStatsData() : null);
					_data.AgentVisuals.AddWeaponToAgentEntity(num2, ref weaponData, ((MissionWeapon)(ref val2)).GetWeaponStatsData(), ref val4, array, (GameEntity)null);
				}
				num2++;
				num3++;
			}
			_data.AgentVisuals.SetWieldedWeaponIndices(_data.RightWieldedItemIndexData, _data.LeftWieldedItemIndexData);
		}
		return flag;
	}

	public static void AddTeamColorToMesh(MetaMesh metaMesh, uint color1, uint color2)
	{
		for (int i = 0; i < metaMesh.MeshCount; i++)
		{
			Mesh meshAtIndex = metaMesh.GetMeshAtIndex(i);
			if (!meshAtIndex.HasTag("no_team_color"))
			{
				meshAtIndex.Color = color1;
				meshAtIndex.Color2 = color2;
				Material val = meshAtIndex.GetMaterial().CreateCopy();
				val.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
				meshAtIndex.SetMaterial(val);
			}
			((NativeObject)meshAtIndex).ManualInvalidate();
		}
	}

	public void SetClothingColors(uint color1, uint color2)
	{
		_data.ClothColor1(color1);
		_data.ClothColor2(color2);
	}

	public void GetClothingColors(out uint color1, out uint color2)
	{
		color1 = _data.ClothColor1Data;
		color2 = _data.ClothColor2Data;
	}

	public void SetEntity(GameEntity entity)
	{
		_data.AgentVisuals.SetEntity(entity);
	}

	void IAgentVisual.SetAction(in ActionIndexCache actionName, float startProgress, bool forceFaceMorphRestart)
	{
		SetAction(in actionName, startProgress, forceFaceMorphRestart);
	}
}
