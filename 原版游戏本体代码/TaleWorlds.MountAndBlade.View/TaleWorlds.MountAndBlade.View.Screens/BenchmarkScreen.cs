using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

public class BenchmarkScreen : ScreenBase
{
	private SceneView _sceneView;

	private Scene _scene;

	private Camera _camera;

	private MatrixFrame _cameraFrame;

	private Timer _cameraTimer;

	private const string _parentEntityName = "LocationEntityParent";

	private const string _sceneName = "benchmark";

	private const string _xmlPath = "../../../Tools/TestAutomation/Attachments/benchmark_scene_performance.xml";

	private List<GameEntity> _cameraLocationEntities;

	private int _currentEntityIndex = -1;

	private PerformanceAnalyzer _analyzer = new PerformanceAnalyzer();

	protected override void OnActivate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		((ScreenBase)this).OnActivate();
		_scene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)0, "mono_renderscene");
		_scene.SetName("BenchmarkScreen");
		_scene.Read("benchmark");
		_cameraFrame = _scene.ReadAndCalculateInitialCamera();
		_scene.SetUseConstantTime(true);
		_sceneView = SceneView.CreateSceneView();
		_sceneView.SetScene(_scene);
		_sceneView.SetSceneUsesShadows(true);
		_camera = Camera.CreateCamera();
		UpdateCamera();
		_cameraTimer = new Timer(MBCommon.GetApplicationTime() - 5f, 5f, true);
		GameEntity val = _scene.FindEntityWithName("LocationEntityParent");
		_cameraLocationEntities = val.GetChildren().ToList();
	}

	public void UpdateCamera()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_camera.Frame = _cameraFrame;
		_sceneView.SetCamera(_camera);
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		_scene = null;
		_analyzer = null;
	}

	protected override void OnFrameTick(float dt)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		((ScreenBase)this).OnFrameTick(dt);
		if (_cameraTimer.Check(MBCommon.GetApplicationTime()))
		{
			_currentEntityIndex++;
			if (_currentEntityIndex >= _cameraLocationEntities.Count)
			{
				_analyzer.FinalizeAndWrite("../../../Tools/TestAutomation/Attachments/benchmark_scene_performance.xml");
				ScreenManager.PopScreen();
				return;
			}
			GameEntity val = _cameraLocationEntities[_currentEntityIndex];
			_cameraFrame = val.GetGlobalFrame();
			UpdateCamera();
			_analyzer.Start(val.Name);
			_cameraTimer.Reset(MBCommon.GetApplicationTime());
		}
		_analyzer.Tick(dt);
	}
}
