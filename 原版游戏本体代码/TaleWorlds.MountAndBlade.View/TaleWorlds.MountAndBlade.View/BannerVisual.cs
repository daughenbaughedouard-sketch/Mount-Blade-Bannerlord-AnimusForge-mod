using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View;

public class BannerVisual : IBannerVisual
{
	public Banner Banner { get; private set; }

	public BannerVisual(Banner banner)
	{
		Banner = banner;
	}

	public void ValidateCreateTableauTextures()
	{
	}

	public Texture GetTableauTextureSmall(in BannerDebugInfo debugInfo, Action<Texture> setAction, bool isTableauOrNineGrid = true)
	{
		BannerTextureCreationData thumbnailCreationData = new BannerTextureCreationData(Banner, setAction, null, debugInfo, isTableauOrNineGrid, isLarge: false);
		return ThumbnailCacheManager.Current.CreateTexture(thumbnailCreationData).Texture;
	}

	public Texture GetTableauTextureLarge(in BannerDebugInfo debugInfo, Action<Texture> setAction, bool isTableauOrNineGrid = true)
	{
		BannerTextureCreationData thumbnailCreationData = new BannerTextureCreationData(Banner, setAction, null, debugInfo, isTableauOrNineGrid, isLarge: true);
		return ThumbnailCacheManager.Current.CreateTexture(thumbnailCreationData).Texture;
	}

	public Texture GetTableauTextureLarge(in BannerDebugInfo debugInfo, Action<Texture> setAction, out BannerTextureCreationData creationData, bool isTableauOrNineGrid = true)
	{
		creationData = new BannerTextureCreationData(Banner, setAction, null, debugInfo, isTableauOrNineGrid, isLarge: true);
		return ThumbnailCacheManager.Current.CreateTexture(creationData).Texture;
	}

	public static MatrixFrame GetMeshMatrix(ref Mesh mesh, float marginLeft, float marginTop, float width, float height, bool mirrored, float rotation, float deltaZ)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		float num = width / 1528f;
		float num2 = height / 1528f;
		float num3 = num / mesh.GetBoundingBoxWidth();
		float num4 = num2 / mesh.GetBoundingBoxHeight();
		((Mat3)(ref identity.rotation)).RotateAboutUp(rotation);
		if (mirrored)
		{
			((Mat3)(ref identity.rotation)).RotateAboutForward(MathF.PI);
		}
		ref Mat3 rotation2 = ref identity.rotation;
		Vec3 val = new Vec3(num3, num4, 1f, -1f);
		((Mat3)(ref rotation2)).ApplyScaleLocal(ref val);
		identity.origin.x = 0f;
		identity.origin.y = 0f;
		identity.origin.x += marginLeft / 1528f;
		identity.origin.y -= marginTop / 1528f;
		identity.origin.z += deltaZ;
		return identity;
	}

	public MetaMesh ConvertToMultiMesh()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		BannerData bannerDataAtIndex = Banner.GetBannerDataAtIndex(0);
		MetaMesh val = MetaMesh.CreateMetaMesh((string)null);
		Mesh fromResource = Mesh.GetFromResource(BannerManager.Instance.GetBackgroundMeshName(bannerDataAtIndex.MeshId));
		Mesh mesh = fromResource.CreateCopy();
		((NativeObject)fromResource).ManualInvalidate();
		mesh.Color = BannerManager.GetColor(bannerDataAtIndex.ColorId2);
		mesh.Color2 = BannerManager.GetColor(bannerDataAtIndex.ColorId);
		MatrixFrame meshMatrix = GetMeshMatrix(ref mesh, bannerDataAtIndex.Position.x, bannerDataAtIndex.Position.y, bannerDataAtIndex.Size.x, bannerDataAtIndex.Size.y, bannerDataAtIndex.Mirror, bannerDataAtIndex.RotationValue * 2f * MathF.PI, 0.5f);
		mesh.SetLocalFrame(meshMatrix);
		val.AddMesh(mesh);
		((NativeObject)mesh).ManualInvalidate();
		Vec2 val2 = default(Vec2);
		Vec2 val3 = default(Vec2);
		for (int i = 1; i < Banner.GetBannerDataListCount(); i++)
		{
			BannerData bannerDataAtIndex2 = Banner.GetBannerDataAtIndex(i);
			BannerIconData iconDataFromIconId = BannerManager.Instance.GetIconDataFromIconId(bannerDataAtIndex2.MeshId);
			Material fromResource2 = Material.GetFromResource(((BannerIconData)(ref iconDataFromIconId)).MaterialName);
			if ((NativeObject)(object)fromResource2 != (NativeObject)null)
			{
				Mesh mesh2 = Mesh.CreateMeshWithMaterial(fromResource2);
				float num = (float)(((BannerIconData)(ref iconDataFromIconId)).TextureIndex % 4) * 0.25f;
				float num2 = 1f - (float)(((BannerIconData)(ref iconDataFromIconId)).TextureIndex / 4) * 0.25f;
				((Vec2)(ref val2))._002Ector(num, num2);
				((Vec2)(ref val3))._002Ector(num + 0.25f, num2 - 0.25f);
				UIntPtr uIntPtr = mesh2.LockEditDataWrite();
				int num3 = mesh2.AddFaceCorner(new Vec3(-0.5f, -0.5f, 0f, -1f), new Vec3(0f, 0f, 1f, -1f), val2 + new Vec2(0f, -0.25f), uint.MaxValue, uIntPtr);
				int num4 = mesh2.AddFaceCorner(new Vec3(0.5f, -0.5f, 0f, -1f), new Vec3(0f, 0f, 1f, -1f), val3, uint.MaxValue, uIntPtr);
				int num5 = mesh2.AddFaceCorner(new Vec3(0.5f, 0.5f, 0f, -1f), new Vec3(0f, 0f, 1f, -1f), val2 + new Vec2(0.25f, 0f), uint.MaxValue, uIntPtr);
				int num6 = mesh2.AddFaceCorner(new Vec3(-0.5f, 0.5f, 0f, -1f), new Vec3(0f, 0f, 1f, -1f), val2, uint.MaxValue, uIntPtr);
				mesh2.AddFace(num3, num4, num5, uIntPtr);
				mesh2.AddFace(num5, num6, num3, uIntPtr);
				mesh2.UnlockEditDataWrite(uIntPtr);
				mesh2.SetColorAndStroke(BannerManager.GetColor(bannerDataAtIndex2.ColorId), BannerManager.GetColor(bannerDataAtIndex2.ColorId2), bannerDataAtIndex2.DrawStroke);
				meshMatrix = GetMeshMatrix(ref mesh2, bannerDataAtIndex2.Position.x, bannerDataAtIndex2.Position.y, bannerDataAtIndex2.Size.x, bannerDataAtIndex2.Size.y, bannerDataAtIndex2.Mirror, bannerDataAtIndex2.RotationValue * 2f * MathF.PI, i);
				mesh2.SetLocalFrame(meshMatrix);
				val.AddMesh(mesh2);
			}
		}
		return val;
	}
}
