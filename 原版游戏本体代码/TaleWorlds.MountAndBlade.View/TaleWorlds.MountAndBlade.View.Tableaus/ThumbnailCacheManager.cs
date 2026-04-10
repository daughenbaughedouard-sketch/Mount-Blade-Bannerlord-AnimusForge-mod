using System;
using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class ThumbnailCacheManager
{
	private ThumbnailCreatorView _thumbnailCreatorView;

	private Scene _inventoryScene;

	private bool _inventorySceneBeingUsed;

	private MBAgentRendererSceneController _inventorySceneAgentRenderer;

	private Scene _mapConversationScene;

	private bool _mapConversationSceneBeingUsed;

	private MBAgentRendererSceneController _mapConversationSceneAgentRenderer;

	private List<IThumbnailCache> _thumbnailCaches;

	private Texture _heroSilhouetteTexture;

	public static ThumbnailCacheManager Current { get; private set; }

	public MatrixFrame InventorySceneCameraFrame { get; private set; }

	private void InitializeThumbnailCreator()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		_thumbnailCreatorView = ThumbnailCreatorView.CreateThumbnailCreatorView();
		ThumbnailCreatorView.renderCallback = (OnThumbnailRenderCompleteDelegate)Delegate.Combine((Delegate?)(object)ThumbnailCreatorView.renderCallback, (Delegate?)new OnThumbnailRenderCompleteDelegate(OnThumbnailRenderComplete));
		Scene[] tableauCharacterScenes = BannerlordTableauManager.TableauCharacterScenes;
		foreach (Scene val in tableauCharacterScenes)
		{
			_thumbnailCreatorView.RegisterScene(val, true);
		}
		SceneInitializationData val2 = default(SceneInitializationData);
		((SceneInitializationData)(ref val2))._002Ector(true);
		val2.InitPhysicsWorld = false;
		val2.DoNotUseLoadingScreen = true;
		_inventoryScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)2, "mono_renderscene");
		_inventoryScene.Read("inventory_character_scene", ref val2, "");
		_inventoryScene.SetShadow(true);
		_inventoryScene.DisableStaticShadows(true);
		InventorySceneCameraFrame = _inventoryScene.FindEntityWithTag("camera_instance").GetGlobalFrame();
		_inventorySceneAgentRenderer = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(_inventoryScene);
	}

	public bool IsCachedInventoryTableauSceneUsed()
	{
		return _inventorySceneBeingUsed;
	}

	public Scene GetCachedInventoryTableauScene()
	{
		_inventorySceneBeingUsed = true;
		return _inventoryScene;
	}

	public void ReturnCachedInventoryTableauScene()
	{
		_inventorySceneBeingUsed = false;
	}

	public bool IsCachedMapConversationTableauSceneUsed()
	{
		return _mapConversationSceneBeingUsed;
	}

	public Scene GetCachedMapConversationTableauScene()
	{
		_mapConversationSceneBeingUsed = true;
		return _mapConversationScene;
	}

	public void ReturnCachedMapConversationTableauScene()
	{
		_mapConversationSceneBeingUsed = false;
	}

	public static int GetNumberOfPendingRequests()
	{
		if (Current != null)
		{
			return Current._thumbnailCreatorView.GetNumberOfPendingRequests();
		}
		return 0;
	}

	public static bool IsNativeMemoryCleared()
	{
		if (Current != null)
		{
			return Current._thumbnailCreatorView.IsMemoryCleared();
		}
		return false;
	}

	public static void InitializeManager()
	{
		Current = new ThumbnailCacheManager();
		Current._thumbnailCaches = new List<IThumbnailCache>();
		Current.InitializeThumbnailCreator();
		Current._heroSilhouetteTexture = Texture.GetFromResource("hero_silhouette");
	}

	public void RegisterThumbnailCache(IThumbnailCache thumbnailCache)
	{
		if (_thumbnailCaches.Contains(thumbnailCache))
		{
			Debug.FailedAssert("Thumbnail cache already registered: " + thumbnailCache.GetType().Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\ThumbnailCacheManager.cs", "RegisterThumbnailCache", 139);
			return;
		}
		_thumbnailCaches.Add(thumbnailCache);
		thumbnailCache.Initialize(_thumbnailCreatorView);
	}

	public void UnregisterThumbnailCache(IThumbnailCache thumbnailCache)
	{
		if (!_thumbnailCaches.Contains(thumbnailCache))
		{
			Debug.FailedAssert("Trying to remove a thumbnail cache that is not registered: " + thumbnailCache.GetType().Name, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\ThumbnailCacheManager.cs", "UnregisterThumbnailCache", 152);
			return;
		}
		_thumbnailCaches.Remove(thumbnailCache);
		thumbnailCache.Destroy();
	}

	public static void InitializeSandboxValues()
	{
		SceneInitializationData val = default(SceneInitializationData);
		((SceneInitializationData)(ref val))._002Ector(true);
		val.InitPhysicsWorld = false;
		val.InitSkyboxFromStart = false;
		Current._mapConversationScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		Current._mapConversationScene.SetName("MapConversationTableau");
		Current._mapConversationScene.DisableStaticShadows(true);
		Current._mapConversationScene.Read("scn_conversation_tableau", ref val, "");
		Current._mapConversationScene.SetShadow(true);
		Current._mapConversationSceneAgentRenderer = MBAgentRendererSceneController.CreateNewAgentRendererSceneController(Current._mapConversationScene);
		Utilities.LoadVirtualTextureTileset("WorldMap");
	}

	public static void ReleaseSandboxValues()
	{
		MBAgentRendererSceneController.DestructAgentRendererSceneController(Current._mapConversationScene, Current._mapConversationSceneAgentRenderer, false);
		Current._mapConversationSceneAgentRenderer = null;
		Current._mapConversationScene.ClearAll();
		((NativeObject)Current._mapConversationScene).ManualInvalidate();
		Current._mapConversationScene = null;
	}

	public static void ClearManager()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		Debug.Print("ThumbnailCacheManager::ClearManager", 0, (DebugColor)12, 17592186044416uL);
		if (Current != null)
		{
			for (int i = 0; i < Current._thumbnailCaches.Count; i++)
			{
				Current._thumbnailCaches[i].Destroy();
			}
			Current._thumbnailCaches.Clear();
			Current._thumbnailCaches = null;
			MBAgentRendererSceneController.DestructAgentRendererSceneController(Current._inventoryScene, Current._inventorySceneAgentRenderer, true);
			Scene inventoryScene = Current._inventoryScene;
			if (inventoryScene != null)
			{
				((NativeObject)inventoryScene).ManualInvalidate();
			}
			Current._inventoryScene = null;
			ThumbnailCreatorView.renderCallback = (OnThumbnailRenderCompleteDelegate)Delegate.Remove((Delegate?)(object)ThumbnailCreatorView.renderCallback, (Delegate?)new OnThumbnailRenderCompleteDelegate(Current.OnThumbnailRenderComplete));
			Current._thumbnailCreatorView.ClearRequests();
			((NativeObject)Current._thumbnailCreatorView).ManualInvalidate();
			Current._thumbnailCreatorView = null;
			Current = null;
		}
	}

	private void OnThumbnailRenderComplete(string renderId, Texture renderTarget)
	{
		Texture texture = null;
		for (int i = 0; i < _thumbnailCaches.Count; i++)
		{
			IThumbnailCache thumbnailCache = _thumbnailCaches[i];
			if (thumbnailCache.GetValue(renderId, out texture) && (NativeObject)(object)texture == (NativeObject)null)
			{
				thumbnailCache.Add(renderId, renderTarget);
			}
		}
		bool flag = false;
		for (int j = 0; j < _thumbnailCaches.Count; j++)
		{
			flag = flag || _thumbnailCaches[j].OnThumbnailRenderCompleted(renderId, renderTarget);
		}
	}

	public TextureCreationInfo CreateTexture(ThumbnailCreationData thumbnailCreationData)
	{
		TextureCreationInfo result = default(TextureCreationInfo);
		for (int i = 0; i < _thumbnailCaches.Count; i++)
		{
			TextureCreationInfo textureCreationInfo = _thumbnailCaches[i].CreateTexture(thumbnailCreationData);
			if (textureCreationInfo.IsValid)
			{
				if (result.IsValid && textureCreationInfo.IsValid)
				{
					Debug.FailedAssert("Creating thumbnails in more than one caches: " + thumbnailCreationData.RenderId, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\ThumbnailCacheManager.cs", "CreateTexture", 253);
				}
				result = textureCreationInfo;
			}
		}
		return result;
	}

	public bool DestroyTexture(ThumbnailCreationData thumbnailCreationData)
	{
		bool result = false;
		for (int i = 0; i < _thumbnailCaches.Count; i++)
		{
			if (_thumbnailCaches[i].ReleaseTexture(thumbnailCreationData))
			{
				result = true;
			}
		}
		return result;
	}

	public void ForceClearAllCache(bool releaseImmediately)
	{
		for (int i = 0; i < _thumbnailCaches.Count; i++)
		{
			_thumbnailCaches[i].Clear(releaseImmediately);
		}
	}

	public Texture GetCachedHeroSilhouetteTexture()
	{
		return _heroSilhouetteTexture;
	}

	public void ClearUnusedCache()
	{
		for (int i = 0; i < _thumbnailCaches.Count; i++)
		{
			_thumbnailCaches[i].ClearUnusedCache();
		}
	}

	public void Tick(float dt)
	{
		for (int i = 0; i < _thumbnailCaches.Count; i++)
		{
			_thumbnailCaches[i].Tick(dt);
		}
	}
}
