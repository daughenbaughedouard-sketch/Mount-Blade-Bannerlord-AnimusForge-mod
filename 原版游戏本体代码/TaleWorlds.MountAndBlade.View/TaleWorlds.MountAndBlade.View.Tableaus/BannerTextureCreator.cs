using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

internal static class BannerTextureCreator
{
	private static Scene _scene;

	private static Camera _bannerCamera;

	private static Camera _nineGridBannerCamera;

	private static ThumbnailCreatorView _thumbnailCreatorView;

	private static int _bannerTableauGPUAllocationIndex;

	internal static void Initialize(ThumbnailCreatorView thumbnailCreatorView)
	{
		_thumbnailCreatorView = thumbnailCreatorView;
		_scene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		_scene.DisableStaticShadows(true);
		_scene.SetName("ThumbnailCacheManager.BannerScene");
		_scene.SetDefaultLighting();
		_thumbnailCreatorView.RegisterScene(_scene, false);
		_bannerCamera = CreateDefaultBannerCamera();
		_nineGridBannerCamera = CreateNineGridBannerCamera();
		_bannerTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("BannerTableauCache");
	}

	internal static void OnFinalize()
	{
		Scene scene = _scene;
		if (scene != null)
		{
			scene.ClearDecals();
		}
		Scene scene2 = _scene;
		if (scene2 != null)
		{
			scene2.ClearAll();
		}
		Scene scene3 = _scene;
		if (scene3 != null)
		{
			((NativeObject)scene3).ManualInvalidate();
		}
		Camera bannerCamera = _bannerCamera;
		if (bannerCamera != null)
		{
			bannerCamera.ReleaseCamera();
		}
		_bannerCamera = null;
		Camera nineGridBannerCamera = _nineGridBannerCamera;
		if (nineGridBannerCamera != null)
		{
			nineGridBannerCamera.ReleaseCamera();
		}
		_nineGridBannerCamera = null;
		_scene = null;
	}

	internal static Texture CreateTexture(BannerThumbnailCreationBaseData bannerCreationData)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		bool isTableauOrNineGrid = bannerCreationData.IsTableauOrNineGrid;
		bool isLarge = bannerCreationData.IsLarge;
		Action<Texture> setAction = bannerCreationData.SetAction;
		string renderId = bannerCreationData.RenderId;
		BannerDebugInfo debugInfo = bannerCreationData.DebugInfo;
		Banner banner = bannerCreationData.Banner;
		bool flag = !(bannerCreationData is BannerTextureCreationData);
		int num = 512;
		int num2 = 512;
		Camera val = _bannerCamera;
		if (isTableauOrNineGrid)
		{
			val = _nineGridBannerCamera;
			if (isLarge)
			{
				num = 1024;
				num2 = 1024;
			}
		}
		MatrixFrame identity = MatrixFrame.Identity;
		if (Game.Current == null)
		{
			banner.SetBannerVisual(((IBannerVisualCreator)new BannerVisualCreator()).CreateBannerVisual(banner));
		}
		string text = ThumbnailDebugUtility.CreateDebugIdFrom(renderId, "ban", debugInfo.CreateName());
		Texture val2 = Texture.CreateRenderTarget(text, num, num2, false, false, true, !flag);
		if (!flag)
		{
			setAction?.Invoke(val2);
		}
		if (!Banner.IsValidBannerCode(banner.BannerCode))
		{
			Debug.FailedAssert("Banner code is not valid: " + banner.BannerCode, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\BannerTextureCreator.cs", "CreateTexture", 93);
			return val2;
		}
		MetaMesh val3 = banner.ConvertToMultiMesh();
		GameEntity val4 = _scene.AddItemEntity(ref identity, val3);
		((NativeObject)val3).ManualInvalidate();
		val4.SetVisibilityExcludeParents(false);
		ThumbnailRenderRequest val5 = ThumbnailRenderRequest.CreateWithTexture(_scene, val, val2, val4, renderId, text, _bannerTableauGPUAllocationIndex);
		_thumbnailCreatorView.RegisterRenderRequest(ref val5);
		return val2;
	}

	internal static Camera CreateDefaultBannerCamera()
	{
		return CreateCamera(1f / 3f, 2f / 3f, -2f / 3f, -1f / 3f, 0.001f, 510f);
	}

	internal static Camera CreateNineGridBannerCamera()
	{
		return CreateCamera(0f, 1f, -1f, 0f, 0.001f, 510f);
	}

	private static Camera CreateCamera(float left, float right, float bottom, float top, float near, float far)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Camera obj = Camera.CreateCamera();
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin.z = 400f;
		obj.Frame = identity;
		obj.LookAt(new Vec3(0f, 0f, 400f, -1f), new Vec3(0f, 0f, 0f, -1f), new Vec3(0f, 1f, 0f, -1f));
		obj.SetViewVolume(false, left, right, bottom, top, near, far);
		return obj;
	}
}
