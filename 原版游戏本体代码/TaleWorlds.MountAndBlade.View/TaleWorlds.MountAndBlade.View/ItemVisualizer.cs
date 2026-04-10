using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public class ItemVisualizer : ScriptComponentBehavior
{
	private MBGameManager _editorGameManager;

	private bool isFinished;

	protected override void OnEditorInit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		((ScriptComponentBehavior)this).OnEditorInit();
		if (Game.Current == null)
		{
			_editorGameManager = (MBGameManager)new EditorGameManager();
		}
	}

	protected override void OnEditorTick(float dt)
	{
		if (!isFinished && _editorGameManager != null)
		{
			isFinished = !((GameManagerBase)_editorGameManager).DoLoadingForGameManager();
			if (isFinished)
			{
				SpawnItems();
			}
		}
	}

	private void SpawnItems()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Scene scene = ((WeakGameEntity)(ref gameEntity)).Scene;
		MBReadOnlyList<ItemObject> objectTypeList = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(100f, 100f, 0f, -1f);
		val.z = 2f;
		float num = 0f;
		float num2 = 200f;
		foreach (ItemObject item in (IEnumerable<ItemObject>)objectTypeList)
		{
			if (item.MultiMeshName.Length <= 0)
			{
				continue;
			}
			MetaMesh copy = MetaMesh.GetCopy(item.MultiMeshName, true, true);
			if ((NativeObject)(object)copy != (NativeObject)null)
			{
				GameEntity obj = GameEntity.CreateEmpty(scene, true, true, true);
				obj.EntityFlags = (EntityFlags)(obj.EntityFlags | 0x20000);
				obj.AddMultiMesh(copy, true);
				obj.Name = ((object)item.Name).ToString();
				obj.RecomputeBoundingBox();
				float boundingBoxRadius = obj.GetBoundingBoxRadius();
				if (boundingBoxRadius > num)
				{
					num = boundingBoxRadius;
				}
				val.x += boundingBoxRadius;
				if (val.x > num2)
				{
					val.x = 100f;
					val.y += num * 3f;
					num = 0f;
				}
				obj.SetLocalPosition(val);
				val.x += boundingBoxRadius;
				if (val.x > num2)
				{
					val.x = 100f;
					val.y += num * 3f;
					num = 0f;
				}
			}
		}
	}
}
