using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public static class MountVisualCreator
{
	public static void SetMaterialProperties(ItemObject mountItem, MetaMesh mountMesh, MountCreationKey key, ref uint maneMeshMultiplier)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		HorseComponent horseComponent = mountItem.HorseComponent;
		int index = MathF.Min((int)key.MaterialIndex, ((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames).Count - 1);
		MaterialProperty val = ((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames)[index];
		Material fromResource = Material.GetFromResource(((MaterialProperty)(ref val)).Name);
		if ((int)mountItem.ItemType == 1)
		{
			int num = MathF.Min((int)key.MeshMultiplierIndex, val.MeshMultiplier.Count - 1);
			if (num != -1)
			{
				maneMeshMultiplier = val.MeshMultiplier[num].Item1;
			}
			mountMesh.SetMaterialToSubMeshesWithTag(fromResource, "horse_body");
			mountMesh.SetFactorColorToSubMeshesWithTag(maneMeshMultiplier, "horse_tail");
		}
		else
		{
			mountMesh.SetMaterial(fromResource);
		}
	}

	public static MountVisualCreationOutput AddMountMesh(MBAgentVisuals agentVisual, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Invalid comparison between Unknown and I4
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Invalid comparison between Unknown and I4
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		MetaMesh val = null;
		MetaMesh val2 = null;
		MetaMesh val3 = null;
		MetaMesh val4 = null;
		HorseComponent horseComponent = mountItem.HorseComponent;
		uint maneMeshMultiplier = uint.MaxValue;
		val2 = mountItem.GetMultiMesh(isFemale: false, useSlimVersion: false, needBatchedVersion: true);
		MountCreationKey val5 = null;
		if (string.IsNullOrEmpty(mountCreationKeyStr))
		{
			mountCreationKeyStr = MountCreationKey.GetRandomMountKeyString(mountItem, MBRandom.RandomInt());
		}
		val5 = MountCreationKey.FromString(mountCreationKeyStr);
		if ((int)mountItem.ItemType == 1)
		{
			SetHorseColors(val2, val5);
		}
		if (horseComponent.HorseMaterialNames != null && ((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames).Count > 0)
		{
			SetMaterialProperties(mountItem, val2, val5, ref maneMeshMultiplier);
		}
		int nondeterministicRandomInt = MBRandom.NondeterministicRandomInt;
		SetVoiceDefinition(agent, nondeterministicRandomInt);
		if (harnessItem != null)
		{
			val4 = harnessItem.GetMultiMesh(isFemale: false, useSlimVersion: false, needBatchedVersion: true);
		}
		foreach (KeyValuePair<string, bool> additionalMeshesName in horseComponent.AdditionalMeshesNameList)
		{
			if (additionalMeshesName.Key.Length <= 0)
			{
				continue;
			}
			string text = additionalMeshesName.Key;
			if (harnessItem == null || !additionalMeshesName.Value)
			{
				val = MetaMesh.GetCopy(text, true, false);
				if (maneMeshMultiplier != uint.MaxValue)
				{
					val.SetFactor1Linear(maneMeshMultiplier);
				}
				continue;
			}
			ArmorComponent armorComponent = harnessItem.ArmorComponent;
			if (armorComponent != null && (int)armorComponent.ManeCoverType == 3)
			{
				continue;
			}
			ArmorComponent armorComponent2 = harnessItem.ArmorComponent;
			if (armorComponent2 != null && (int)armorComponent2.ManeCoverType > 0)
			{
				string text2 = text;
				HorseHarnessCoverTypes? obj;
				if (harnessItem == null)
				{
					obj = null;
				}
				else
				{
					ArmorComponent armorComponent3 = harnessItem.ArmorComponent;
					obj = ((armorComponent3 != null) ? new HorseHarnessCoverTypes?(armorComponent3.ManeCoverType) : ((HorseHarnessCoverTypes?)null));
				}
				text = text2 + "_" + obj;
			}
			val = MetaMesh.GetCopy(text, true, false);
			if (maneMeshMultiplier != uint.MaxValue)
			{
				val.SetFactor1Linear(maneMeshMultiplier);
			}
		}
		if ((NativeObject)(object)val2 != (NativeObject)null && harnessItem != null)
		{
			ArmorComponent armorComponent4 = harnessItem.ArmorComponent;
			if (((armorComponent4 != null) ? new HorseTailCoverTypes?(armorComponent4.TailCoverType) : ((HorseTailCoverTypes?)null)) == (HorseTailCoverTypes?)1)
			{
				val2.RemoveMeshesWithTag("horse_tail");
			}
		}
		if ((NativeObject)(object)val4 != (NativeObject)null)
		{
			if ((NativeObject)(object)agentVisual != (NativeObject)null)
			{
				MetaMesh val6 = null;
				if (NativeConfig.CharacterDetail > 2 && harnessItem.ArmorComponent != null)
				{
					val6 = MetaMesh.GetCopy(harnessItem.ArmorComponent.ReinsRopeMesh, false, true);
				}
				ArmorComponent armorComponent5 = harnessItem.ArmorComponent;
				val3 = MetaMesh.GetCopy((armorComponent5 != null) ? armorComponent5.ReinsMesh : null, false, true);
				if ((NativeObject)(object)val6 != (NativeObject)null && (NativeObject)(object)val3 != (NativeObject)null)
				{
					agentVisual.AddHorseReinsClothMesh(val3, val6);
					((NativeObject)val6).ManualInvalidate();
				}
			}
			else if (harnessItem.ArmorComponent != null)
			{
				val3 = MetaMesh.GetCopy(harnessItem.ArmorComponent.ReinsMesh, true, true);
			}
		}
		return new MountVisualCreationOutput(val, val2, val3, val4);
	}

	public static void SetHorseColors(MetaMesh horseMesh, MountCreationKey mountCreationKey)
	{
		horseMesh.SetVectorArgument((float)(int)mountCreationKey._leftFrontLegColorIndex, (float)(int)mountCreationKey._rightFrontLegColorIndex, (float)(int)mountCreationKey._leftBackLegColorIndex, (float)(int)mountCreationKey._rightBackLegColorIndex);
	}

	public static void ClearMountMesh(GameEntity gameEntity)
	{
		gameEntity.RemoveAllChildren();
		gameEntity.Remove(106);
	}

	private static void SetVoiceDefinition(Agent agent, int seedForRandomVoiceTypeAndPitch)
	{
		MBAgentVisuals val = ((agent != null) ? agent.AgentVisuals : null);
		if ((NativeObject)(object)val != (NativeObject)null)
		{
			string soundAndCollisionInfoClassName = agent.GetSoundAndCollisionInfoClassName();
			int num = ((!string.IsNullOrEmpty(soundAndCollisionInfoClassName)) ? SkinVoiceManager.GetVoiceDefinitionCountWithMonsterSoundAndCollisionInfoClassName(soundAndCollisionInfoClassName) : 0);
			if (num == 0)
			{
				val.SetVoiceDefinitionIndex(-1, 0f);
				return;
			}
			int num2 = MathF.Abs(seedForRandomVoiceTypeAndPitch);
			float num3 = (float)num2 * 4.656613E-10f;
			int[] array = new int[num];
			SkinVoiceManager.GetVoiceDefinitionListWithMonsterSoundAndCollisionInfoClassName(soundAndCollisionInfoClassName, array);
			int num4 = array[num2 % num];
			val.SetVoiceDefinitionIndex(num4, num3);
		}
	}

	public static void AddMountMeshToEntity(GameEntity gameEntity, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, out MountVisualCreationOutput mountVisualCreationOutput, Agent agent = null)
	{
		mountVisualCreationOutput = AddMountMesh(null, mountItem, harnessItem, mountCreationKeyStr, agent);
		AddMultiMeshToSkeleton(mountVisualCreationOutput.HorseManeMesh, gameEntity);
		AddMultiMeshToSkeleton(mountVisualCreationOutput.MountMesh, gameEntity);
		AddMultiMeshToSkeleton(mountVisualCreationOutput.ReinMesh, gameEntity);
		AddMultiMeshToSkeleton(mountVisualCreationOutput.MountHarnessMesh, gameEntity);
	}

	public static void AddMountMeshToEntity(GameEntity gameEntity, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
	{
		AddMountMeshToEntity(gameEntity, mountItem, harnessItem, mountCreationKeyStr, out var _, agent);
	}

	public static void AddMountMeshToAgentVisual(MBAgentVisuals agentVisual, ItemObject mountItem, ItemObject harnessItem, string mountCreationKeyStr, Agent agent = null)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		MountVisualCreationOutput mountVisualCreationOutput = AddMountMesh(agentVisual, mountItem, harnessItem, mountCreationKeyStr, agent);
		AddMultiMeshToAgentVisual(mountVisualCreationOutput.HorseManeMesh, agentVisual);
		AddMultiMeshToAgentVisual(mountVisualCreationOutput.MountMesh, agentVisual);
		AddMultiMeshToAgentVisual(mountVisualCreationOutput.ReinMesh, agentVisual);
		AddMultiMeshToAgentVisual(mountVisualCreationOutput.MountHarnessMesh, agentVisual);
		if (agent != null && harnessItem != null && harnessItem.IsUsingTeamColor && (NativeObject)(object)mountVisualCreationOutput.MountHarnessMesh != (NativeObject)null)
		{
			AgentVisuals.AddTeamColorToMesh(mountVisualCreationOutput.MountHarnessMesh, agent.ClothingColor1, agent.ClothingColor2);
		}
		HorseComponent horseComponent = mountItem.HorseComponent;
		if (((horseComponent != null) ? horseComponent.SkeletonScale : null) != null)
		{
			agentVisual.ApplySkeletonScale(mountItem.HorseComponent.SkeletonScale.MountSitBoneScale, mountItem.HorseComponent.SkeletonScale.MountRadiusAdder, mountItem.HorseComponent.SkeletonScale.BoneIndices, mountItem.HorseComponent.SkeletonScale.Scales);
		}
	}

	private static void AddMultiMeshToAgentVisual(MetaMesh metaMesh, MBAgentVisuals agentVisual)
	{
		if ((NativeObject)(object)metaMesh != (NativeObject)null)
		{
			agentVisual.AddMultiMesh(metaMesh, (BodyMeshTypes)(-1));
			((NativeObject)metaMesh).ManualInvalidate();
		}
	}

	private static void AddMultiMeshToSkeleton(MetaMesh metaMesh, GameEntity gameEntity)
	{
		if ((NativeObject)(object)metaMesh != (NativeObject)null)
		{
			gameEntity.AddMultiMeshToSkeleton(metaMesh);
			((NativeObject)metaMesh).ManualInvalidate();
		}
	}
}
