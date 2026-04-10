using System.Collections.Generic;
using SandBox.Missions.MissionLogics;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace SandBox.View.Missions;

public class MissionItemCalatogView : MissionView
{
	private ItemCatalogController _itemCatalogController;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_itemCatalogController = ((MissionBehavior)this).Mission.GetMissionBehavior<ItemCatalogController>();
		_itemCatalogController.BeforeCatalogTick += OnBeforeCatalogTick;
		_itemCatalogController.AfterCatalogTick += OnAfterCatalogTick;
	}

	private void OnBeforeCatalogTick(int currentItemIndex)
	{
		Utilities.TakeScreenshot(string.Concat("ItemCatalog/", ((List<ItemObject>)(object)_itemCatalogController.AllItems)[currentItemIndex - 1].Name, ".bmp"));
	}

	private void OnAfterCatalogTick()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame frame = default(MatrixFrame);
		Vec3 lookDirection = ((MissionBehavior)this).Mission.MainAgent.LookDirection;
		frame.origin = ((MissionBehavior)this).Mission.MainAgent.Position + lookDirection * 2f + new Vec3(0f, 0f, 1.273f, -1f);
		frame.rotation.u = lookDirection;
		frame.rotation.s = new Vec3(1f, 0f, 0f, -1f);
		frame.rotation.f = new Vec3(0f, 0f, 1f, -1f);
		((Mat3)(ref frame.rotation)).Orthonormalize();
		((MissionBehavior)this).Mission.SetCameraFrame(ref frame, 1f);
		Camera val = Camera.CreateCamera();
		val.Frame = frame;
		((MissionView)this).MissionScreen.CustomCamera = val;
	}
}
