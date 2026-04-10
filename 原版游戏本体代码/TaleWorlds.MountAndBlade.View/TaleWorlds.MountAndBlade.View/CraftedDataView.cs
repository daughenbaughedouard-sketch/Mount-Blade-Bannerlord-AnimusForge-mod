using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public class CraftedDataView
{
	public delegate void OnMeshBuiltDelegate(WeaponDesign weaponDesign, ref MetaMesh builtMesh);

	public static OnMeshBuiltDelegate OnWeaponMeshBuilt;

	public static OnMeshBuiltDelegate OnHolsterMeshBuilt;

	public static OnMeshBuiltDelegate OnHolsterMeshWithWeaponBuilt;

	private MetaMesh _weaponMesh;

	private MetaMesh _holsterMesh;

	private MetaMesh _holsterMeshWithWeapon;

	private MetaMesh _nonBatchedWeaponMesh;

	private MetaMesh _nonBatchedHolsterMesh;

	private MetaMesh _nonBatchedHolsterMeshWithWeapon;

	public WeaponDesign CraftedData { get; private set; }

	public MetaMesh WeaponMesh
	{
		get
		{
			if (!((NativeObject)(object)_weaponMesh != (NativeObject)null) || !_weaponMesh.HasVertexBufferOrEditDataOrPackageItem())
			{
				return _weaponMesh = GenerateWeaponMesh(batchMeshes: true);
			}
			return _weaponMesh;
		}
	}

	public MetaMesh HolsterMesh => _holsterMesh ?? (_holsterMesh = GenerateHolsterMesh());

	public MetaMesh HolsterMeshWithWeapon
	{
		get
		{
			if (!((NativeObject)(object)_holsterMeshWithWeapon != (NativeObject)null) || !_holsterMeshWithWeapon.HasVertexBufferOrEditDataOrPackageItem())
			{
				return _holsterMeshWithWeapon = GenerateHolsterMeshWithWeapon(batchMeshes: true);
			}
			return _holsterMeshWithWeapon;
		}
	}

	public MetaMesh NonBatchedWeaponMesh => _nonBatchedWeaponMesh ?? (_nonBatchedWeaponMesh = GenerateWeaponMesh(batchMeshes: false));

	public MetaMesh NonBatchedHolsterMesh => _nonBatchedHolsterMesh ?? (_nonBatchedHolsterMesh = GenerateHolsterMesh());

	public MetaMesh NonBatchedHolsterMeshWithWeapon => _nonBatchedHolsterMeshWithWeapon ?? (_nonBatchedHolsterMeshWithWeapon = GenerateHolsterMeshWithWeapon(batchMeshes: false));

	public CraftedDataView(WeaponDesign craftedData)
	{
		CraftedData = craftedData;
	}

	public void Clear()
	{
		_weaponMesh = null;
		_holsterMesh = null;
		_holsterMeshWithWeapon = null;
		_nonBatchedWeaponMesh = null;
		_nonBatchedHolsterMesh = null;
		_nonBatchedHolsterMeshWithWeapon = null;
	}

	private MetaMesh GenerateWeaponMesh(bool batchMeshes)
	{
		if (CraftedData.UsedPieces != null)
		{
			return BuildWeaponMesh(CraftedData, 0f, pieceTypeHidingEnabledForHolster: false, batchMeshes);
		}
		return null;
	}

	private MetaMesh GenerateHolsterMesh()
	{
		if (CraftedData.UsedPieces != null)
		{
			return BuildHolsterMesh(CraftedData);
		}
		return null;
	}

	private MetaMesh GenerateHolsterMeshWithWeapon(bool batchMeshes)
	{
		if (CraftedData.UsedPieces != null)
		{
			return BuildHolsterMeshWithWeapon(CraftedData, 0f, batchMeshes);
		}
		return null;
	}

	public static MetaMesh BuildWeaponMesh(WeaponDesign craftedData, float pivotDiff, bool pieceTypeHidingEnabledForHolster, bool batchAllMeshes)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		CraftingTemplate template = craftedData.Template;
		MetaMesh builtMesh = MetaMesh.CreateMetaMesh((string)null);
		List<MetaMesh> list = new List<MetaMesh>();
		List<MetaMesh> list2 = new List<MetaMesh>();
		List<MetaMesh> list3 = new List<MetaMesh>();
		PieceData[] buildOrders = template.BuildOrders;
		for (int i = 0; i < buildOrders.Length; i++)
		{
			PieceData val = buildOrders[i];
			if (pieceTypeHidingEnabledForHolster && template.IsPieceTypeHiddenOnHolster(((PieceData)(ref val)).PieceType))
			{
				continue;
			}
			WeaponDesignElement val2 = craftedData.UsedPieces[((PieceData)(ref val)).PieceType];
			float num = craftedData.PiecePivotDistances[((PieceData)(ref val)).PieceType];
			if (val2 != null && val2.IsValid && !float.IsNaN(num))
			{
				MetaMesh copy = MetaMesh.GetCopy(val2.CraftingPiece.MeshName, true, false);
				if (!batchAllMeshes)
				{
					copy.ClearMeshesForOtherLods(0);
				}
				Mat3 identity = Mat3.Identity;
				Vec3 val3 = num * Vec3.Up;
				MatrixFrame frame = new MatrixFrame(ref identity, ref val3);
				if (val2.IsPieceScaled)
				{
					Vec3 val4 = (Vec3)(val2.CraftingPiece.FullScale ? (Vec3.One * val2.ScaleFactor) : new Vec3(1f, 1f, val2.ScaleFactor, -1f));
					((MatrixFrame)(ref frame)).Scale(ref val4);
				}
				copy.Frame = frame;
				if (copy.HasClothData())
				{
					list3.Add(copy);
				}
				else
				{
					list2.Add(copy);
				}
			}
		}
		foreach (MetaMesh item in list2)
		{
			if (batchAllMeshes)
			{
				list.Add(item);
			}
			else
			{
				builtMesh.MergeMultiMeshes(item);
			}
		}
		if (batchAllMeshes)
		{
			builtMesh.BatchMultiMeshesMultiple(list);
		}
		foreach (MetaMesh item2 in list3)
		{
			builtMesh.MergeMultiMeshes(item2);
			builtMesh.AssignClothBodyFrom(item2);
		}
		builtMesh.SetEditDataPolicy((EditDataPolicy)1);
		if (batchAllMeshes)
		{
			builtMesh.SetLodBias(1);
		}
		MatrixFrame frame2 = builtMesh.Frame;
		((MatrixFrame)(ref frame2)).Elevate(pivotDiff);
		builtMesh.Frame = frame2;
		if (OnWeaponMeshBuilt != null)
		{
			OnWeaponMeshBuilt(craftedData, ref builtMesh);
		}
		return builtMesh;
	}

	public static MetaMesh BuildHolsterMesh(WeaponDesign craftedData)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		if (craftedData.Template.UseWeaponAsHolsterMesh)
		{
			return null;
		}
		BladeData bladeData = craftedData.UsedPieces[0].CraftingPiece.BladeData;
		if (craftedData.Template.AlwaysShowHolsterWithWeapon || string.IsNullOrEmpty(bladeData.HolsterMeshName))
		{
			return null;
		}
		float num = craftedData.PiecePivotDistances[0];
		MetaMesh copy = MetaMesh.GetCopy(bladeData.HolsterMeshName, false, false);
		MatrixFrame frame = copy.Frame;
		ref Vec3 origin = ref frame.origin;
		origin += new Vec3(0f, 0f, num, -1f);
		WeaponDesignElement val = craftedData.UsedPieces[0];
		if (MathF.Abs(val.ScaledLength - val.CraftingPiece.Length) > 1E-05f)
		{
			Vec3 val2 = (Vec3)(val.CraftingPiece.FullScale ? (Vec3.One * val.ScaleFactor) : new Vec3(1f, 1f, val.ScaleFactor, -1f));
			((MatrixFrame)(ref frame)).Scale(ref val2);
		}
		copy.Frame = frame;
		MetaMesh builtMesh = MetaMesh.CreateMetaMesh(bladeData.HolsterMeshName);
		builtMesh.MergeMultiMeshes(copy);
		if (OnHolsterMeshBuilt != null)
		{
			OnHolsterMeshBuilt(craftedData, ref builtMesh);
		}
		return builtMesh;
	}

	public static MetaMesh BuildHolsterMeshWithWeapon(WeaponDesign craftedData, float pivotDiff, bool batchAllMeshes)
	{
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Invalid comparison between Unknown and I4
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Invalid comparison between Unknown and I4
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		if (craftedData.Template.UseWeaponAsHolsterMesh)
		{
			return null;
		}
		WeaponDesignElement val = craftedData.UsedPieces[0];
		BladeData bladeData = val.CraftingPiece.BladeData;
		if (string.IsNullOrEmpty(bladeData.HolsterMeshName))
		{
			return null;
		}
		MetaMesh builtMesh = MetaMesh.CreateMetaMesh((string)null);
		MetaMesh copy = MetaMesh.GetCopy(bladeData.HolsterMeshName, false, true);
		string text = bladeData.HolsterMeshName + "_skeleton";
		if (Skeleton.SkeletonModelExist(text))
		{
			MetaMesh val2 = BuildWeaponMesh(craftedData, 0f, pieceTypeHidingEnabledForHolster: true, batchAllMeshes);
			float num = craftedData.PiecePivotDistances[0];
			float scaledDistanceToPreviousPiece = craftedData.UsedPieces[0].ScaledDistanceToPreviousPiece;
			float num2 = num - scaledDistanceToPreviousPiece;
			List<MetaMesh> list = new List<MetaMesh>();
			Skeleton val3 = Skeleton.CreateFromModel(text);
			for (sbyte b = 1; b < val3.GetBoneCount(); b++)
			{
				MatrixFrame boneEntitialRestFrame = val3.GetBoneEntitialRestFrame(b, false);
				if (craftedData.Template.RotateWeaponInHolster)
				{
					((Mat3)(ref boneEntitialRestFrame.rotation)).RotateAboutForward(MathF.PI);
				}
				MetaMesh val4 = val2.CreateCopy();
				MatrixFrame frame = new MatrixFrame(ref boneEntitialRestFrame.rotation, ref boneEntitialRestFrame.origin);
				((MatrixFrame)(ref frame)).Elevate(0f - num2);
				val4.Frame = frame;
				if (batchAllMeshes)
				{
					int num3 = 8 - (b - 1);
					val4.SetShaderToMaterial("quiver_deformer");
					val4.SetFactor1Linear((uint)(419430400uL * (ulong)num3));
					list.Add(val4);
				}
				else
				{
					builtMesh.MergeMultiMeshes(val4);
				}
			}
			if (list.Count > 0)
			{
				builtMesh.BatchMultiMeshesMultiple(list);
			}
			if ((int)craftedData.Template.PieceTypeToScaleHolsterWith != -1)
			{
				WeaponDesignElement val5 = craftedData.UsedPieces[craftedData.Template.PieceTypeToScaleHolsterWith];
				MatrixFrame frame2 = copy.Frame;
				int num4 = -MathF.Sign(val3.GetBoneEntitialRestFrame((sbyte)0, false).rotation.u.z);
				float num5 = val.CraftingPiece.BladeData.HolsterMeshLength * (val5.ScaleFactor - 1f) * 0.5f * (float)num4;
				WeaponDesignElement val6 = craftedData.UsedPieces[craftedData.Template.PieceTypeToScaleHolsterWith];
				if (val6.IsPieceScaled)
				{
					Vec3 val7 = (Vec3)(val6.CraftingPiece.FullScale ? (Vec3.One * val6.ScaleFactor) : new Vec3(1f, 1f, val6.ScaleFactor, -1f));
					((MatrixFrame)(ref frame2)).Scale(ref val7);
				}
				ref Vec3 origin = ref frame2.origin;
				origin += new Vec3(0f, 0f, 0f - num5, -1f);
				copy.Frame = frame2;
			}
		}
		else
		{
			if ((int)craftedData.Template.PieceTypeToScaleHolsterWith != -1)
			{
				MatrixFrame frame3 = copy.Frame;
				ref Vec3 origin2 = ref frame3.origin;
				origin2 += new Vec3(0f, 0f, craftedData.PiecePivotDistances[craftedData.Template.PieceTypeToScaleHolsterWith], -1f);
				WeaponDesignElement val8 = craftedData.UsedPieces[craftedData.Template.PieceTypeToScaleHolsterWith];
				if (val8.IsPieceScaled)
				{
					Vec3 val9 = (Vec3)(val8.CraftingPiece.FullScale ? (Vec3.One * val8.ScaleFactor) : new Vec3(1f, 1f, val8.ScaleFactor, -1f));
					((MatrixFrame)(ref frame3)).Scale(ref val9);
				}
				copy.Frame = frame3;
			}
			builtMesh.MergeMultiMeshes(BuildWeaponMesh(craftedData, 0f, pieceTypeHidingEnabledForHolster: true, batchAllMeshes));
		}
		builtMesh.MergeMultiMeshes(copy);
		MatrixFrame frame4 = builtMesh.Frame;
		ref Vec3 origin3 = ref frame4.origin;
		origin3 += new Vec3(0f, 0f, pivotDiff, -1f);
		builtMesh.Frame = frame4;
		if (OnHolsterMeshWithWeaponBuilt != null)
		{
			OnHolsterMeshWithWeaponBuilt(craftedData, ref builtMesh);
		}
		return builtMesh;
	}
}
