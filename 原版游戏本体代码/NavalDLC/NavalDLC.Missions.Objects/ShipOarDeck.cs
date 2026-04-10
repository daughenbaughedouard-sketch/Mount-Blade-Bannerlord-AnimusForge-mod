using System;
using System.Collections.Generic;
using NavalDLC.Missions.ShipActuators;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.Objects;

public class ShipOarDeck : ScriptComponentBehavior
{
	public const string OarEntityName = "oar";

	public const string OarRetractedFrameEntityName = "retracted_frame";

	public const string RightOarMachinesHolderName = "right_oar_machines";

	public const string LeftOarMachinesHolderName = "left_oar_machines";

	public const string LeftOarGateTag = "oar_gate_left";

	public const string RightOarGateTag = "oar_gate_right";

	public const string HandTargetEntityName = "hand_position";

	public const string OarEntityTag = "oar_entity";

	public const string RetractedEntityTag = "retracted_entity";

	public const string HandTargetEntityTag = "hand_target_entity";

	public const string SeatLocationEntity = "seat_location_entity";

	public const string ShipBodyPhysicsEntityTag = "body_mesh";

	public const string SeatMeshTag = "seat_mesh_entity";

	[EditableScriptComponentVariable(true, "")]
	private float _verticalBaseAngle = 15f;

	[EditableScriptComponentVariable(true, "")]
	private float _lateralBaseAngle;

	[EditableScriptComponentVariable(true, "")]
	private float _verticalRotationAngle = 10f;

	[EditableScriptComponentVariable(true, "")]
	private float _lateralRotationAngle = 17.2f;

	private float _oarLength;

	private OarDeckParameters _oarDeckParameters;

	public OarDeckParameters GetParameters()
	{
		if (_oarDeckParameters == null)
		{
			_oarDeckParameters = new OarDeckParameters(_verticalBaseAngle * (MathF.PI / 180f), _lateralBaseAngle * (MathF.PI / 180f), _verticalRotationAngle * (MathF.PI / 180f), _lateralRotationAngle * (MathF.PI / 180f), _oarLength);
		}
		else
		{
			_oarDeckParameters.SetParameters(_verticalBaseAngle * (MathF.PI / 180f), _lateralBaseAngle * (MathF.PI / 180f), _verticalRotationAngle * (MathF.PI / 180f), _lateralRotationAngle * (MathF.PI / 180f), _oarLength);
		}
		return _oarDeckParameters;
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
		UpdateOarLength();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity item in ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("seat_mesh_entity"))
		{
			WeakGameEntity firstChildEntityWithName = MBExtensions.GetFirstChildEntityWithName(item, "floor");
			if (firstChildEntityWithName != (GameEntity)null)
			{
				((WeakGameEntity)(ref firstChildEntityWithName)).Remove(78);
			}
		}
	}

	internal void UpdateOarLength()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		List<WeakGameEntity> list = ((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("oar_gate_left");
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		list.AddRange(((WeakGameEntity)(ref gameEntity)).CollectChildrenEntitiesWithTag("oar_gate_right"));
		if (list.Count > 0)
		{
			float num = -1f;
			foreach (WeakGameEntity item in list)
			{
				WeakGameEntity current = item;
				Mesh firstMesh = ((WeakGameEntity)(ref current)).GetFirstMesh();
				WeakGameEntity val = current;
				if ((NativeObject)(object)firstMesh == (NativeObject)null)
				{
					WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref current)).GetFirstChildEntityWithTag("upgrade_slot");
					if (((WeakGameEntity)(ref firstChildEntityWithTag)).ChildCount > 0)
					{
						WeakGameEntity val2 = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetFirstChildEntityWithTag("base");
						if (!((WeakGameEntity)(ref val2)).IsValid)
						{
							val2 = ((WeakGameEntity)(ref firstChildEntityWithTag)).GetChild(0);
						}
						firstMesh = ((WeakGameEntity)(ref val2)).GetFirstMesh();
						val = val2;
					}
				}
				if (!((NativeObject)(object)firstMesh != (NativeObject)null))
				{
					continue;
				}
				float num2 = float.MinValue;
				if (((WeakGameEntity)(ref val)).MultiMeshComponentCount == 0)
				{
					Vec3 boundingBoxMax = firstMesh.GetBoundingBoxMax();
					num2 = MathF.Max(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMax.z);
				}
				else
				{
					for (int i = 0; i < ((WeakGameEntity)(ref val)).MultiMeshComponentCount; i++)
					{
						MetaMesh metaMesh = ((WeakGameEntity)(ref val)).GetMetaMesh(i);
						for (int j = 0; j < metaMesh.MeshCount; j++)
						{
							Vec3 boundingBoxMax2 = metaMesh.GetMeshAtIndex(j).GetBoundingBoxMax();
							num2 = MathF.Max(MathF.Max(boundingBoxMax2.x, boundingBoxMax2.y, boundingBoxMax2.z), num2);
						}
					}
				}
				if (num >= 0f)
				{
					MBMath.ApproximatelyEquals(num2, num, 1E-05f);
					num = MathF.Max(num, num2);
				}
				else
				{
					num = num2;
				}
			}
			_oarLength = num;
		}
		else
		{
			_oarLength = 0f;
		}
	}

	public static WeakGameEntity GetOarEntity(WeakGameEntity oarScriptEntity)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity result = ((WeakGameEntity)(ref oarScriptEntity)).GetFirstChildEntityWithTag("oar_entity");
		if (!((WeakGameEntity)(ref result)).IsValid)
		{
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref oarScriptEntity)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).Name == "oar")
				{
					result = current;
				}
			}
		}
		return result;
	}

	public static void LoadOarScriptEntity(WeakGameEntity oarScriptEntity, out WeakGameEntity oarEntity, ref MatrixFrame oarExtractedEntitialFrame, ref MatrixFrame oarRetractedEntitialFrame, out WeakGameEntity handTargetEntity)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		handTargetEntity = WeakGameEntity.Invalid;
		oarEntity = GetOarEntity(oarScriptEntity);
		WeakGameEntity val = ((WeakGameEntity)(ref oarScriptEntity)).GetFirstChildEntityWithTag("retracted_entity");
		if (!((WeakGameEntity)(ref oarEntity)).IsValid)
		{
			return;
		}
		oarExtractedEntitialFrame = ((WeakGameEntity)(ref oarEntity)).GetFrame();
		handTargetEntity = ((WeakGameEntity)(ref oarEntity)).GetFirstChildEntityWithTag("hand_target_entity");
		if (((WeakGameEntity)(ref val)).IsValid)
		{
			oarRetractedEntitialFrame = ((WeakGameEntity)(ref val)).GetFrame();
		}
		if (!((WeakGameEntity)(ref handTargetEntity)).IsValid)
		{
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref oarEntity)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).Name == "hand_position")
				{
					handTargetEntity = current;
				}
			}
		}
		if (!((WeakGameEntity)(ref val)).IsValid)
		{
			foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref oarEntity)).GetChildren())
			{
				WeakGameEntity current2 = child2;
				if (((WeakGameEntity)(ref current2)).Name == "retracted_frame")
				{
					oarRetractedEntitialFrame = ((WeakGameEntity)(ref current2)).GetFrame();
					val = current2;
				}
			}
		}
		if (val != (GameEntity)null)
		{
			((WeakGameEntity)(ref val)).Remove(66);
		}
	}

	private static WeakGameEntity GetRetractedFrameEntity(WeakGameEntity oarMachine)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity result = ((WeakGameEntity)(ref oarMachine)).GetFirstChildEntityWithTag("retracted_entity");
		if (((WeakGameEntity)(ref result)).IsValid)
		{
			return result;
		}
		WeakGameEntity oarEntity = GetOarEntity(oarMachine);
		if (((WeakGameEntity)(ref oarEntity)).IsValid && !((WeakGameEntity)(ref result)).IsValid)
		{
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref oarEntity)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).Name == "retracted_frame")
				{
					result = current;
				}
			}
		}
		return result;
	}
}
