using System;
using System.Collections.Generic;
using System.Threading;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens.Scripts;

public class MultiThreadedStressTestsScreen : ScreenBase
{
	public static class MultiThreadedTestFunctions
	{
		public static void MeshMerger(InputLayout layout)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected I4, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected I4, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected I4, but got Unknown
			//IL_0030: Expected I4, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			Mesh randomMeshWithVdecl = Mesh.GetRandomMeshWithVdecl((int)layout);
			randomMeshWithVdecl = randomMeshWithVdecl.CreateCopy();
			UIntPtr uIntPtr = randomMeshWithVdecl.LockEditDataWrite();
			Mesh randomMeshWithVdecl2 = Mesh.GetRandomMeshWithVdecl((int)layout);
			randomMeshWithVdecl2 = randomMeshWithVdecl2.CreateCopy();
			Mesh randomMeshWithVdecl3 = Mesh.GetRandomMeshWithVdecl((int)layout);
			Mesh randomMeshWithVdecl4 = Mesh.GetRandomMeshWithVdecl((int)layout);
			randomMeshWithVdecl.AddMesh(randomMeshWithVdecl3, MatrixFrame.Identity);
			randomMeshWithVdecl2.AddMesh(randomMeshWithVdecl4, MatrixFrame.Identity);
			randomMeshWithVdecl.AddMesh(randomMeshWithVdecl2, MatrixFrame.Identity);
			int num = randomMeshWithVdecl.AddFaceCorner(new Vec3(0f, 0f, 1f, -1f), new Vec3(0f, 0f, 1f, -1f), new Vec2(0f, 1f), 268435455u, uIntPtr);
			int num2 = randomMeshWithVdecl.AddFaceCorner(new Vec3(0f, 1f, 0f, -1f), new Vec3(0f, 0f, 1f, -1f), new Vec2(1f, 0f), 268435455u, uIntPtr);
			int num3 = randomMeshWithVdecl.AddFaceCorner(new Vec3(0f, 1f, 1f, -1f), new Vec3(0f, 0f, 1f, -1f), new Vec2(1f, 1f), 268435455u, uIntPtr);
			randomMeshWithVdecl.AddFace(num, num2, num3, uIntPtr);
			randomMeshWithVdecl.UnlockEditDataWrite(uIntPtr);
		}

		public static void SceneHandler(SceneView view)
		{
			int num = 0;
			while (num < 500)
			{
				view.SetSceneUsesShadows(false);
				view.SetRenderWithPostfx(false);
				Thread.Sleep(5000);
				view.SetSceneUsesShadows(true);
				view.SetRenderWithPostfx(true);
				Thread.Sleep(5000);
				view.SetSceneUsesContour(true);
				Thread.Sleep(5000);
			}
		}
	}

	private List<Thread> _workerThreads;

	private Scene _scene;

	private SceneView _sceneView;

	protected override void OnActivate()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		((ScreenBase)this).OnActivate();
		_scene = Scene.CreateNewScene(true, true, (DecalAtlasGroup)0, "mono_renderscene");
		_scene.Read("mp_ruins_2");
		_sceneView = SceneView.CreateSceneView();
		_sceneView.SetScene(_scene);
		_sceneView.SetSceneUsesShadows(true);
		Camera val = Camera.CreateCamera();
		val.Frame = _scene.ReadAndCalculateInitialCamera();
		_sceneView.SetCamera(val);
		_workerThreads = new List<Thread>();
		Thread thread = new Thread((ThreadStart)delegate
		{
			MultiThreadedTestFunctions.MeshMerger((InputLayout)0);
		});
		thread.Name = "StressTester|Mesh Merger Thread";
		_workerThreads.Add(thread);
		Thread thread2 = new Thread((ThreadStart)delegate
		{
			MultiThreadedTestFunctions.MeshMerger((InputLayout)1);
		});
		thread2.Name = "StressTester|Mesh Merger Thread";
		_workerThreads.Add(thread2);
		Thread thread3 = new Thread((ThreadStart)delegate
		{
			MultiThreadedTestFunctions.MeshMerger((InputLayout)2);
		});
		thread3.Name = "StressTester|Mesh Merger Thread";
		_workerThreads.Add(thread3);
		for (int num = 0; num < _workerThreads.Count; num++)
		{
			_workerThreads[num].Start();
		}
	}

	protected override void OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
		_sceneView = null;
		_scene = null;
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		bool flag = true;
		for (int i = 0; i < _workerThreads.Count; i++)
		{
			if (_workerThreads[i].IsAlive)
			{
				flag = false;
			}
		}
		if (flag)
		{
			ScreenManager.PopScreen();
		}
	}
}
