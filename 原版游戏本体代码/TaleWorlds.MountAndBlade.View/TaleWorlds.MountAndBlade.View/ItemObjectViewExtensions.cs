using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public static class ItemObjectViewExtensions
{
	public static MetaMesh GetCraftedMultiMesh(this ItemObject itemObject, bool needBatchedVersion)
	{
		CraftedDataView craftedDataView = CraftedDataViewManager.GetCraftedDataView(itemObject.WeaponDesign);
		if (!needBatchedVersion)
		{
			if (craftedDataView == null)
			{
				return null;
			}
			return craftedDataView.NonBatchedWeaponMesh.CreateCopy();
		}
		if (craftedDataView == null)
		{
			return null;
		}
		return craftedDataView.WeaponMesh.CreateCopy();
	}

	public static MetaMesh GetMultiMeshCopy(this ItemObject itemObject)
	{
		MetaMesh craftedMultiMesh = itemObject.GetCraftedMultiMesh(needBatchedVersion: true);
		if ((NativeObject)(object)craftedMultiMesh != (NativeObject)null)
		{
			return craftedMultiMesh;
		}
		if (string.IsNullOrEmpty(itemObject.MultiMeshName))
		{
			return null;
		}
		return MetaMesh.GetCopy(itemObject.MultiMeshName, true, false);
	}

	public static MetaMesh GetMultiMeshCopyWithGenderData(this ItemObject itemObject, bool isFemale, bool useSlimVersion, bool needBatchedVersion)
	{
		MetaMesh craftedMultiMesh = itemObject.GetCraftedMultiMesh(needBatchedVersion);
		if ((NativeObject)(object)craftedMultiMesh != (NativeObject)null)
		{
			return craftedMultiMesh;
		}
		if (string.IsNullOrEmpty(itemObject.MultiMeshName))
		{
			return null;
		}
		MetaMesh val = null;
		val = MetaMesh.GetCopy(isFemale ? (itemObject.MultiMeshName + "_female") : (itemObject.MultiMeshName + "_male"), false, true);
		if ((NativeObject)(object)val != (NativeObject)null)
		{
			return val;
		}
		string multiMeshName = itemObject.MultiMeshName;
		multiMeshName = ((!isFemale) ? (multiMeshName + (useSlimVersion ? "_slim" : "")) : (multiMeshName + (useSlimVersion ? "_converted_slim" : "_converted")));
		val = MetaMesh.GetCopy(multiMeshName, false, true);
		if ((NativeObject)(object)val != (NativeObject)null)
		{
			return val;
		}
		val = MetaMesh.GetCopy(itemObject.MultiMeshName, true, true);
		if ((NativeObject)(object)val != (NativeObject)null)
		{
			return val;
		}
		return null;
	}

	public static MatrixFrame GetScaledFrame(this ItemObject itemObject, Mat3 rotationMatrix, MetaMesh metaMesh, float scaleFactor, Vec3 positionShift)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(1000000f, 1000000f, 1000000f, -1f);
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(-1000000f, -1000000f, -1000000f, -1f);
		for (int i = 0; i != metaMesh.MeshCount; i++)
		{
			Vec3 boundingBoxMin = metaMesh.GetMeshAtIndex(i).GetBoundingBoxMin();
			Vec3 boundingBoxMax = metaMesh.GetMeshAtIndex(i).GetBoundingBoxMax();
			Vec3[] array = (Vec3[])(object)new Vec3[8];
			Vec3 val3 = new Vec3(boundingBoxMin.x, boundingBoxMin.y, boundingBoxMin.z, -1f);
			array[0] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMin.x, boundingBoxMin.y, boundingBoxMax.z, -1f);
			array[1] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMin.x, boundingBoxMax.y, boundingBoxMin.z, -1f);
			array[2] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMin.x, boundingBoxMax.y, boundingBoxMax.z, -1f);
			array[3] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMax.x, boundingBoxMin.y, boundingBoxMin.z, -1f);
			array[4] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMax.x, boundingBoxMin.y, boundingBoxMax.z, -1f);
			array[5] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMin.z, -1f);
			array[6] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			val3 = new Vec3(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMax.z, -1f);
			array[7] = ((Mat3)(ref rotationMatrix)).TransformToParent(ref val3);
			for (int j = 0; j < 8; j++)
			{
				val = Vec3.Vec3Min(val, array[j]);
				val2 = Vec3.Vec3Max(val2, array[j]);
			}
		}
		float num = 1f;
		if (itemObject.PrimaryWeapon != null && itemObject.PrimaryWeapon.IsMeleeWeapon)
		{
			num = 0.3f + (float)itemObject.WeaponComponent.PrimaryWeapon.WeaponLength / 1.6f;
			num = MBMath.ClampFloat(num, 0.5f, 1f);
		}
		Vec3 val4 = (val + val2) * 0.5f;
		float num2 = MathF.Max(val2.x - val.x, val2.y - val.y);
		float num3 = scaleFactor * num / num2;
		ref Vec3 origin = ref identity.origin;
		origin -= val4 * num3;
		ref Vec3 origin2 = ref identity.origin;
		origin2 += positionShift;
		identity.rotation = rotationMatrix;
		((Mat3)(ref identity.rotation)).ApplyScaleLocal(num3);
		return identity;
	}
}
