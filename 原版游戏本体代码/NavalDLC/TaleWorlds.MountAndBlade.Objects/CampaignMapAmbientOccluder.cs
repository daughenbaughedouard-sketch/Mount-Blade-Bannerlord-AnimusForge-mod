using System.Collections.Generic;
using NavalDLC;
using NavalDLC.Map;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Objects;

internal class CampaignMapAmbientOccluder : ScriptComponentBehavior
{
	private const int MaximumSpecialStormNumber = 2;

	private readonly List<GameEntity> _questStorms = new List<GameEntity>();

	private int MaximumNumberOfStorms => NavalDLCManager.Instance.GameModels.MapStormModel.MaximumNumberOfStorms + 2;

	protected override void OnInit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		Mesh firstMesh = ((WeakGameEntity)(ref gameEntity)).GetFirstMesh();
		int num = MathF.Max(MaximumNumberOfStorms, 16);
		firstMesh.SetupAdditionalBoneBuffer(num);
		for (int i = 0; i < num; i++)
		{
			MatrixFrame zero = MatrixFrame.Zero;
			firstMesh.SetAdditionalBoneFrame(i, ref zero);
		}
	}

	protected override void OnTick(float dt)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		foreach (Storm item in (List<Storm>)(object)NavalDLCManager.Instance.StormManager.SpawnedStorms)
		{
			Vec2 currentPosition = item.CurrentPosition;
			SetBoneFrame(((Vec2)(ref currentPosition)).ToVec3(0f), ((ScriptComponentBehavior)this).GameEntity, i++);
		}
		foreach (GameEntity questStorm in _questStorms)
		{
			SetBoneFrame(questStorm.GlobalPosition, ((ScriptComponentBehavior)this).GameEntity, i++);
		}
		for (int num = MathF.Max(MaximumNumberOfStorms, 16); i < num; i++)
		{
			MatrixFrame zero = MatrixFrame.Zero;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).SetBoneFrameToAllMeshes(i, ref zero);
		}
	}

	protected override void OnEditorInit()
	{
	}

	protected override void OnEditorTick(float dt)
	{
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	public void RegisterQuestStorm(GameEntity stormEntity)
	{
		_questStorms.Add(stormEntity);
	}

	public void UnregisterQuestStorm(GameEntity stormEntity)
	{
		_questStorms.Remove(stormEntity);
	}

	private void SetBoneFrame(Vec3 origin, WeakGameEntity gameEntity, int boneIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		Vec3 val = new Vec3(60f, 0f, 0f, -1f);
		((MatrixFrame)(ref identity)).Scale(ref val);
		identity.origin = origin;
		WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)this).GameEntity;
		((WeakGameEntity)(ref gameEntity2)).SetBoneFrameToAllMeshes(boneIndex, ref identity);
	}
}
