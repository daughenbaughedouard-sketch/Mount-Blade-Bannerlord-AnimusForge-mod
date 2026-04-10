using System;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class BrightnessDemoTableau
{
	private static int _tableauIndex;

	private MatrixFrame _frame;

	private Scene _tableauScene;

	private Texture _demoTexture;

	private Camera _continuousRenderCamera;

	private bool _initialized;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private int _demoType = -1;

	private bool _isEnabled;

	private float RenderScale = 1f;

	public Texture Texture { get; private set; }

	private TableauView View
	{
		get
		{
			if ((NativeObject)(object)Texture != (NativeObject)null)
			{
				return Texture.TableauView;
			}
			return null;
		}
	}

	private void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		TableauView view = View;
		if (!_initialized)
		{
			SetScene();
		}
		if (view != null)
		{
			((View)view).SetEnable(_isEnabled);
		}
	}

	public void SetDemoType(int demoType)
	{
		_demoType = demoType;
		_initialized = false;
		RefreshDemoTableau();
	}

	public void SetTargetSize(int width, int height)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		int num = 0;
		int num2 = 0;
		if (width <= 0 || height <= 0)
		{
			num = 10;
			num2 = 10;
		}
		else
		{
			RenderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			num = (int)((float)width * RenderScale);
			num2 = (int)((float)height * RenderScale);
		}
		if (num != _tableauSizeX || num2 != _tableauSizeY)
		{
			_tableauSizeX = num;
			_tableauSizeY = num2;
			TableauView view = View;
			if (view != null)
			{
				((View)view).SetEnable(false);
			}
			TableauView view2 = View;
			if (view2 != null)
			{
				((SceneView)view2).AddClearTask(true);
			}
			Texture texture = Texture;
			if (texture != null)
			{
				texture.Release();
			}
			Texture = TableauView.AddTableau($"BrightnessDemo_{_tableauIndex++}", new TextureUpdateEventHandler(SceneTableauContinuousRenderFunction), (object)_tableauScene, _tableauSizeX, _tableauSizeY);
		}
	}

	public void OnFinalize()
	{
		if ((NativeObject)(object)_continuousRenderCamera != (NativeObject)null)
		{
			_continuousRenderCamera.ReleaseCameraEntity();
			_continuousRenderCamera = null;
		}
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		TableauView view2 = View;
		if (view2 != null)
		{
			((SceneView)view2).AddClearTask(false);
		}
		Texture = null;
		Scene tableauScene = _tableauScene;
		if (tableauScene != null)
		{
			((NativeObject)tableauScene).ManualInvalidate();
		}
		_tableauScene = null;
	}

	public void SetScene()
	{
		_tableauScene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)0, "mono_renderscene");
		switch (_demoType)
		{
		case 0:
			_demoTexture = Texture.GetFromResource("brightness_calibration_wide");
			_tableauScene.SetAtmosphereWithName("brightness_calibration_screen");
			break;
		case 1:
			_demoTexture = Texture.GetFromResource("calibration_image_1");
			_tableauScene.SetAtmosphereWithName("TOD_11_00_SemiCloudy");
			break;
		case 2:
			_demoTexture = Texture.GetFromResource("calibration_image_2");
			_tableauScene.SetAtmosphereWithName("TOD_05_00_SemiCloudy");
			break;
		case 3:
			_demoTexture = Texture.GetFromResource("calibration_image_3");
			_tableauScene.SetAtmosphereWithName("TOD_05_00_SemiCloudy");
			break;
		case 4:
			_demoTexture = Texture.GetFromResource("naval_calibration_0");
			_tableauScene.SetAtmosphereWithName("TOD_naval_10_00_SemiCloudy");
			break;
		case 5:
			_demoTexture = Texture.GetFromResource("naval_calibration_1");
			_tableauScene.SetAtmosphereWithName("TOD_naval_06_00_sunset");
			break;
		case 6:
			_demoTexture = Texture.GetFromResource("naval_calibration_2");
			_tableauScene.SetAtmosphereWithName("TOD_naval_08_00_rain_storm");
			break;
		default:
			Debug.FailedAssert($"Undefined Brightness demo type({_demoType})", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\BrightnessDemoTableau.cs", "SetScene", 145);
			break;
		}
		_tableauScene.SetDepthOfFieldParameters(0f, 0f, false);
	}

	private void RefreshDemoTableau()
	{
		if (!_initialized)
		{
			SetEnabled(enabled: true);
		}
	}

	public void OnTick(float dt)
	{
		if ((NativeObject)(object)_continuousRenderCamera == (NativeObject)null)
		{
			_continuousRenderCamera = Camera.CreateCamera();
		}
		TableauView view = View;
		if (view != null)
		{
			view.SetDoNotRenderThisFrame(false);
		}
	}

	internal void SceneTableauContinuousRenderFunction(Texture sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Scene val = (Scene)sender.UserData;
		TableauView tableauView = sender.TableauView;
		((View)tableauView).SetEnable(true);
		if ((NativeObject)(object)val == (NativeObject)null)
		{
			tableauView.SetContinuousRendering(false);
			tableauView.SetDeleteAfterRendering(true);
			return;
		}
		val.SetShadow(false);
		val.EnsurePostfxSystem();
		val.SetDofMode(false);
		val.SetMotionBlurMode(false);
		val.SetBloom(true);
		val.SetDynamicShadowmapCascadesRadiusMultiplier(0.31f);
		val.SetExternalInjectionTexture(_demoTexture);
		((SceneView)tableauView).SetRenderWithPostfx(true);
		((SceneView)tableauView).SetDoQuickExposure(true);
		if ((NativeObject)(object)_continuousRenderCamera != (NativeObject)null)
		{
			Camera continuousRenderCamera = _continuousRenderCamera;
			((SceneView)tableauView).SetCamera(continuousRenderCamera);
			((SceneView)tableauView).SetScene(val);
			((SceneView)tableauView).SetSceneUsesSkybox(false);
			tableauView.SetDeleteAfterRendering(false);
			tableauView.SetContinuousRendering(true);
			tableauView.SetDoNotRenderThisFrame(true);
			((View)tableauView).SetClearColor(4278190080u);
			((SceneView)tableauView).SetFocusedShadowmap(true, ref _frame.origin, 1.55f);
		}
	}
}
