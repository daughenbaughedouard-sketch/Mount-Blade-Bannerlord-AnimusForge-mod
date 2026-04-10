using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.Missions.Deployment;

public class NavalFormationDeploymentPlan : IFormationDeploymentPlan
{
	private MatrixFrame _spawnFrame;

	private readonly FormationClass _class;

	private bool _hasFrame;

	private Mission _mission;

	public FormationClass Class => _class;

	public FormationClass SpawnClass => _class;

	public float PlannedWidth
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (ShipObject == null)
			{
				return 0f;
			}
			Vec2 deploymentArea = ShipObject.DeploymentArea;
			return ((Vec2)(ref deploymentArea)).X;
		}
	}

	public float PlannedDepth
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (ShipObject == null)
			{
				return 0f;
			}
			Vec2 deploymentArea = ShipObject.DeploymentArea;
			return ((Vec2)(ref deploymentArea)).Y;
		}
	}

	public int PlannedTroopCount
	{
		get
		{
			if (ShipObject == null)
			{
				return 0;
			}
			return ShipOrigin.TotalCrewCapacity;
		}
	}

	public bool HasDimensions
	{
		get
		{
			if (PlannedWidth >= 1E-05f)
			{
				return PlannedDepth >= 1E-05f;
			}
			return false;
		}
	}

	public bool HasShipObject => ShipObject != null;

	public IShipOrigin ShipOrigin { get; private set; }

	public MissionShipObject ShipObject { get; private set; }

	public NavalFormationDeploymentPlan(FormationClass fClass, Mission mission)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		_class = fClass;
		Clear();
		_hasFrame = false;
		ShipOrigin = null;
		ShipObject = null;
		_mission = mission;
	}

	public bool HasFrame()
	{
		return _hasFrame;
	}

	public FormationDeploymentFlank GetDefaultFlank()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected I4, but got Unknown
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		FormationDeploymentFlank val = (FormationDeploymentFlank)4;
		if (!HasShipObject)
		{
			val = (FormationDeploymentFlank)3;
		}
		FormationClass val2 = _class;
		switch (val2 - 1)
		{
		case 1:
		case 6:
			return (FormationDeploymentFlank)1;
		case 2:
		case 5:
			return (FormationDeploymentFlank)2;
		case 0:
		case 7:
		case 8:
		case 9:
			return (FormationDeploymentFlank)3;
		default:
			return (FormationDeploymentFlank)0;
		}
	}

	public MatrixFrame GetFrame()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		UpdateFrameZ();
		return _spawnFrame;
	}

	public Vec3 GetPosition()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		UpdateFrameZ();
		return _spawnFrame.origin;
	}

	public Vec2 GetDirection()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ((Vec3)(ref _spawnFrame.rotation.f)).AsVec2;
	}

	public WorldPosition CreateNewDeploymentWorldPosition(WorldPositionEnforcedCache worldPositionEnforcedCache)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return WorldPosition.Invalid;
	}

	public void Clear()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_spawnFrame = MatrixFrame.Identity;
		_hasFrame = false;
		ShipOrigin = null;
		ShipObject = null;
	}

	public void SetShipOrigin(IShipOrigin shipOrigin)
	{
		if (shipOrigin != null)
		{
			ShipOrigin = shipOrigin;
		}
		else
		{
			ShipOrigin = null;
		}
		if (ShipOrigin != null)
		{
			ShipObject = MBObjectManager.Instance.GetObject<MissionShipObject>(ShipOrigin.OriginShipId);
		}
		else
		{
			ShipObject = null;
		}
	}

	public void SetFrame(in Vec2 deployPosition, in Vec2 deployDirection)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Vec2 val = deployDirection;
		Vec3 val2 = ((Vec2)(ref val)).ToVec3(0f);
		Mat3 val3 = Mat3.CreateMat3WithForward(ref val2);
		val = deployPosition;
		val2 = ((Vec2)(ref val)).ToVec3(0f);
		_spawnFrame = new MatrixFrame(ref val3, ref val2);
		UpdateFrameZ();
		_hasFrame = true;
	}

	private void UpdateFrameZ()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		_spawnFrame.origin.z = _mission.Scene.GetWaterLevelAtPosition(((Vec3)(ref _spawnFrame.origin)).AsVec2, true, false);
	}
}
