using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class BannerTableau
{
	private static int _tableauIndex;

	private bool _isFinalized;

	private bool _isEnabled;

	private bool _isNineGrid;

	private bool _isDirty;

	private Banner _banner;

	private int _latestWidth = -1;

	private int _latestHeight = -1;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private float RenderScale = 1f;

	private float _customRenderScale = 1f;

	private Scene _scene;

	private Camera _defaultCamera;

	private Camera _nineGridCamera;

	private MetaMesh _currentMultiMesh;

	private GameEntity _currentMeshEntity;

	private int _meshIndexToUpdate;

	public Texture Texture { get; private set; }

	internal Camera CurrentCamera
	{
		get
		{
			if (!_isNineGrid)
			{
				return _defaultCamera;
			}
			return _nineGridCamera;
		}
	}

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

	public BannerTableau()
	{
		SetEnabled(enabled: true);
		FirstTimeInit();
	}

	public void OnTick(float dt)
	{
		if (_isEnabled && !_isFinalized)
		{
			Refresh();
			TableauView view = View;
			if (view != null)
			{
				view.SetDoNotRenderThisFrame(false);
			}
		}
	}

	private void FirstTimeInit()
	{
		_scene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
		_scene.DisableStaticShadows(true);
		_scene.SetName("BannerTableau.Scene");
		_scene.SetDefaultLighting();
		_defaultCamera = BannerTextureCreator.CreateDefaultBannerCamera();
		_nineGridCamera = BannerTextureCreator.CreateNineGridBannerCamera();
		_isDirty = true;
	}

	private void Refresh()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!_isDirty)
		{
			return;
		}
		if (_currentMeshEntity != (GameEntity)null)
		{
			_scene.RemoveEntity(_currentMeshEntity, 111);
		}
		if (_banner != null)
		{
			MatrixFrame identity = MatrixFrame.Identity;
			if (Banner.IsValidBannerCode(_banner.BannerCode))
			{
				_currentMultiMesh = _banner.ConvertToMultiMesh();
				_currentMeshEntity = _scene.AddItemEntity(ref identity, _currentMultiMesh);
				((NativeObject)_currentMeshEntity).ManualInvalidate();
				((NativeObject)_currentMultiMesh).ManualInvalidate();
			}
			else
			{
				Debug.FailedAssert("Banner code is not valid: " + _banner.BannerCode, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\BannerTableau.cs", "Refresh", 109);
			}
			_isDirty = false;
		}
	}

	private void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(_isEnabled);
		}
	}

	public void SetTargetSize(int width, int height)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		_latestWidth = width;
		_latestHeight = height;
		if (width <= 0 || height <= 0)
		{
			_tableauSizeX = 10;
			_tableauSizeY = 10;
		}
		else
		{
			RenderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			_tableauSizeX = (int)((float)width * _customRenderScale * RenderScale);
			_tableauSizeY = (int)((float)height * _customRenderScale * RenderScale);
		}
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
		Texture = TableauView.AddTableau($"BannerTableau_{_tableauIndex++}", new TextureUpdateEventHandler(BannerTableauContinuousRenderFunction), (object)_scene, _tableauSizeX, _tableauSizeY);
		((SceneView)Texture.TableauView).SetSceneUsesContour(false);
	}

	public void SetBannerCode(string value)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (string.IsNullOrEmpty(value))
		{
			_banner = null;
		}
		else
		{
			_banner = new Banner(value);
		}
		_isDirty = true;
	}

	public void OnFinalize()
	{
		if (!_isFinalized)
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
			_scene = null;
			TableauView view = View;
			if (view != null)
			{
				((View)view).SetEnable(false);
			}
			Texture texture = Texture;
			if (texture != null)
			{
				texture.Release();
			}
			Texture = null;
			Camera defaultCamera = _defaultCamera;
			if (defaultCamera != null)
			{
				defaultCamera.ReleaseCamera();
			}
			_defaultCamera = null;
			Camera nineGridCamera = _nineGridCamera;
			if (nineGridCamera != null)
			{
				nineGridCamera.ReleaseCamera();
			}
			_nineGridCamera = null;
		}
		_isFinalized = true;
	}

	public void SetCustomRenderScale(float value)
	{
		if (!MBMath.ApproximatelyEqualsTo(_customRenderScale, value, 1E-05f))
		{
			_customRenderScale = value;
			if (_latestWidth != -1 && _latestHeight != -1)
			{
				SetTargetSize(_latestWidth, _latestHeight);
			}
		}
	}

	internal void BannerTableauContinuousRenderFunction(Texture sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Scene val = (Scene)sender.UserData;
		TableauView tableauView = sender.TableauView;
		if ((NativeObject)(object)val == (NativeObject)null)
		{
			tableauView.SetContinuousRendering(false);
			tableauView.SetDeleteAfterRendering(true);
			return;
		}
		val.EnsurePostfxSystem();
		val.SetDofMode(false);
		val.SetMotionBlurMode(false);
		val.SetBloom(false);
		val.SetDynamicShadowmapCascadesRadiusMultiplier(0.31f);
		((SceneView)tableauView).SetRenderWithPostfx(false);
		((SceneView)tableauView).SetScene(val);
		((SceneView)tableauView).SetCamera(CurrentCamera);
		((SceneView)tableauView).SetSceneUsesSkybox(false);
		tableauView.SetDeleteAfterRendering(false);
		tableauView.SetContinuousRendering(true);
		tableauView.SetDoNotRenderThisFrame(true);
		((View)tableauView).SetClearColor(0u);
	}

	public void SetIsNineGrid(bool value)
	{
		_isNineGrid = value;
		_isDirty = true;
	}

	public void SetMeshIndexToUpdate(int value)
	{
		_meshIndexToUpdate = value;
	}

	public void SetUpdatePositionValueManual(Vec2 value)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (_currentMultiMesh.MeshCount >= 1 && _meshIndexToUpdate >= 0 && _meshIndexToUpdate < _currentMultiMesh.MeshCount)
		{
			Mesh meshAtIndex = _currentMultiMesh.GetMeshAtIndex(_meshIndexToUpdate);
			MatrixFrame localFrame = meshAtIndex.GetLocalFrame();
			localFrame.origin.x = 0f;
			localFrame.origin.y = 0f;
			localFrame.origin.x += ((Vec2)(ref value)).X / 1528f;
			localFrame.origin.y -= ((Vec2)(ref value)).Y / 1528f;
			meshAtIndex.SetLocalFrame(localFrame);
		}
	}

	public void SetUpdateSizeValueManual(Vec2 value)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (_currentMultiMesh.MeshCount >= 1 && _meshIndexToUpdate >= 0 && _meshIndexToUpdate < _currentMultiMesh.MeshCount)
		{
			Mesh meshAtIndex = _currentMultiMesh.GetMeshAtIndex(_meshIndexToUpdate);
			MatrixFrame localFrame = meshAtIndex.GetLocalFrame();
			float num = ((Vec2)(ref value)).X / 1528f / meshAtIndex.GetBoundingBoxWidth();
			float num2 = ((Vec2)(ref value)).Y / 1528f / meshAtIndex.GetBoundingBoxHeight();
			Vec3 eulerAngles = ((Mat3)(ref localFrame.rotation)).GetEulerAngles();
			localFrame.rotation = Mat3.Identity;
			((Mat3)(ref localFrame.rotation)).ApplyEulerAngles(ref eulerAngles);
			ref Mat3 rotation = ref localFrame.rotation;
			Vec3 val = new Vec3(num, num2, 1f, -1f);
			((Mat3)(ref rotation)).ApplyScaleLocal(ref val);
			meshAtIndex.SetLocalFrame(localFrame);
		}
	}

	public void SetUpdateRotationValueManual((float, bool) value)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if (_currentMultiMesh.MeshCount >= 1 && _meshIndexToUpdate >= 0 && _meshIndexToUpdate < _currentMultiMesh.MeshCount)
		{
			Mesh meshAtIndex = _currentMultiMesh.GetMeshAtIndex(_meshIndexToUpdate);
			MatrixFrame localFrame = meshAtIndex.GetLocalFrame();
			float num = value.Item1 * 2f * MathF.PI;
			Vec3 scaleVector = ((Mat3)(ref localFrame.rotation)).GetScaleVector();
			localFrame.rotation = Mat3.Identity;
			((Mat3)(ref localFrame.rotation)).RotateAboutUp(num);
			((Mat3)(ref localFrame.rotation)).ApplyScaleLocal(ref scaleVector);
			if (value.Item2)
			{
				((Mat3)(ref localFrame.rotation)).RotateAboutForward(MathF.PI);
			}
			meshAtIndex.SetLocalFrame(localFrame);
		}
	}
}
