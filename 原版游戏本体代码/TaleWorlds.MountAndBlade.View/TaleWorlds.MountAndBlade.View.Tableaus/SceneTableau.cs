using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class SceneTableau
{
	private float _animationFrequencyThreshold = 2.5f;

	private MatrixFrame _frame;

	private Scene _tableauScene;

	private Camera _continuousRenderCamera;

	private GameEntity _cameraEntity;

	private float _cameraRatio;

	private bool _initialized;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private SceneView View;

	private bool _isRotatingCharacter;

	private float _animationGap;

	private bool _isEnabled;

	private float RenderScale = 1f;

	public Texture _texture { get; private set; }

	public bool? IsReady
	{
		get
		{
			SceneView view = View;
			if (view == null)
			{
				return null;
			}
			return view.ReadyToRender();
		}
	}

	public SceneTableau()
	{
		SetEnabled(enabled: true);
	}

	private void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		SceneView view = View;
		if (view != null)
		{
			((View)view).SetEnable(_isEnabled);
		}
	}

	private void CreateTexture()
	{
		_texture = Texture.CreateRenderTarget("SceneTableau", _tableauSizeX, _tableauSizeY, true, false, false, false);
		View = SceneView.CreateSceneView();
		View.SetScene(_tableauScene);
		((View)View).SetRenderTarget(_texture);
		((View)View).SetAutoDepthTargetCreation(true);
		View.SetSceneUsesSkybox(true);
		((View)View).SetClearColor(4294902015u);
	}

	public void SetTargetSize(int width, int height)
	{
		_isRotatingCharacter = false;
		if (width <= 0 || height <= 0)
		{
			_tableauSizeX = 10;
			_tableauSizeY = 10;
		}
		else
		{
			RenderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			_tableauSizeX = (int)((float)width * RenderScale);
			_tableauSizeY = (int)((float)height * RenderScale);
		}
		_cameraRatio = (float)_tableauSizeX / (float)_tableauSizeY;
		_ = View;
		SceneView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		SceneView view2 = View;
		if (view2 != null)
		{
			view2.AddClearTask(true);
		}
		CreateTexture();
	}

	public void OnFinalize()
	{
		if ((NativeObject)(object)_continuousRenderCamera != (NativeObject)null)
		{
			_continuousRenderCamera.ReleaseCameraEntity();
			_continuousRenderCamera = null;
			_cameraEntity = null;
		}
		SceneView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		SceneView view2 = View;
		if (view2 != null)
		{
			view2.AddClearTask(false);
		}
		Texture texture = _texture;
		if (texture != null)
		{
			texture.Release();
		}
		_texture = null;
		_tableauScene = null;
	}

	public void SetScene(object scene)
	{
		Scene tableauScene;
		if ((tableauScene = (Scene)((scene is Scene) ? scene : null)) != null)
		{
			_tableauScene = tableauScene;
			if (_tableauSizeX != 0 && _tableauSizeY != 0)
			{
				CreateTexture();
			}
		}
		else
		{
			Debug.FailedAssert("Given scene object is not Scene type", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\SceneTableau.cs", "SetScene", 120);
		}
	}

	public void SetBannerCode(string value)
	{
		RefreshCharacterTableau();
	}

	private void RefreshCharacterTableau(Equipment oldEquipment = null)
	{
		if (!_initialized)
		{
			FirstTimeInit();
		}
	}

	public void RotateCharacter(bool value)
	{
		_isRotatingCharacter = value;
	}

	public void OnTick(float dt)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (_animationFrequencyThreshold > _animationGap)
		{
			_animationGap += dt;
		}
		if (!((NativeObject)(object)View != (NativeObject)null))
		{
			return;
		}
		if ((NativeObject)(object)_continuousRenderCamera == (NativeObject)null)
		{
			GameEntity val = _tableauScene.FindEntityWithTag("customcamera");
			if (val != (GameEntity)null)
			{
				_continuousRenderCamera = Camera.CreateCamera();
				Vec3 val2 = default(Vec3);
				val.GetCameraParamsFromCameraScript(_continuousRenderCamera, ref val2);
				_cameraEntity = val;
			}
		}
		PopupSceneContinuousRenderFunction();
	}

	private void FirstTimeInit()
	{
		_initialized = true;
	}

	private void PopupSceneContinuousRenderFunction()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		GameEntity val = _tableauScene.FindEntityWithTag("customcamera");
		_tableauScene.SetShadow(true);
		_tableauScene.EnsurePostfxSystem();
		_tableauScene.SetMotionBlurMode(true);
		_tableauScene.SetBloom(true);
		_tableauScene.SetDynamicShadowmapCascadesRadiusMultiplier(1f);
		View.SetRenderWithPostfx(true);
		View.SetSceneUsesShadows(true);
		View.SetScene(_tableauScene);
		View.SetSceneUsesSkybox(true);
		((View)View).SetClearColor(4278190080u);
		View.SetFocusedShadowmap(false, ref _frame.origin, 1.55f);
		((View)View).SetEnable(true);
		if (val != (GameEntity)null)
		{
			Vec3 val2 = default(Vec3);
			val.GetCameraParamsFromCameraScript(_continuousRenderCamera, ref val2);
			if ((NativeObject)(object)_continuousRenderCamera != (NativeObject)null)
			{
				Camera continuousRenderCamera = _continuousRenderCamera;
				View.SetCamera(continuousRenderCamera);
			}
		}
	}
}
