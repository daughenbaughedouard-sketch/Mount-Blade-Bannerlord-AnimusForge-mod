using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class SiegeDeploymentVisualizationMissionView : MissionView
{
	public enum DeploymentVisualizationPreference
	{
		ShowUndeployed = 1,
		Line = 2,
		Arc = 4,
		Banner = 8,
		Path = 16,
		Ghost = 32,
		Contour = 64,
		LiftLadders = 128,
		Light = 256,
		AllEnabled = 1023
	}

	private static int deploymentVisualizerSelector;

	private List<DeploymentPoint> _deploymentPoints;

	private bool _deploymentPointsVisible;

	private Dictionary<DeploymentPoint, List<Vec3>> _deploymentArcs = new Dictionary<DeploymentPoint, List<Vec3>>();

	private Dictionary<DeploymentPoint, (GameEntity, GameEntity)> _deploymentBanners = new Dictionary<DeploymentPoint, (GameEntity, GameEntity)>();

	private Dictionary<DeploymentPoint, GameEntity> _deploymentLights = new Dictionary<DeploymentPoint, GameEntity>();

	private const uint EntityHighlightColor = 4289622555u;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		_deploymentPoints = (from dp in MBExtensions.FindAllWithType<DeploymentPoint>((IEnumerable<MissionObject>)Mission.Current.ActiveMissionObjects)
			where !((MissionObject)dp).IsDisabled
			select dp).ToList();
		foreach (DeploymentPoint deploymentPoint in _deploymentPoints)
		{
			deploymentPoint.OnDeploymentPointTypeDetermined += OnDeploymentPointStateSet;
			deploymentPoint.OnDeploymentStateChanged += OnDeploymentStateChanged;
		}
		_deploymentPointsVisible = true;
		Mission.Current.GetMissionBehavior<SiegeDeploymentMissionController>();
	}

	public override void OnDeploymentFinished()
	{
		((MissionBehavior)this).OnDeploymentFinished();
		TryRemoveDeploymentVisibilities();
		Mission.Current.RemoveMissionBehavior((MissionBehavior)(object)this);
	}

	protected override void OnEndMission()
	{
		((MissionBehavior)this).OnEndMission();
		TryRemoveDeploymentVisibilities();
	}

	public override void OnRemoveBehavior()
	{
		base.OnRemoveBehavior();
	}

	private void TryRemoveDeploymentVisibilities()
	{
		if (!_deploymentPointsVisible)
		{
			return;
		}
		foreach (DeploymentPoint deploymentPoint in _deploymentPoints)
		{
			RemoveDeploymentVisibility(deploymentPoint);
			deploymentPoint.OnDeploymentStateChanged -= OnDeploymentStateChanged;
		}
		_deploymentPointsVisible = false;
	}

	private void RemoveDeploymentVisibility(DeploymentPoint deploymentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		DeploymentPointType deploymentPointType = deploymentPoint.GetDeploymentPointType();
		switch ((int)deploymentPointType)
		{
		case 0:
			HideDeploymentBanners(deploymentPoint, isRemoving: true);
			SetGhostVisibility(deploymentPoint, isVisible: false);
			break;
		case 1:
			HideDeploymentBanners(deploymentPoint, isRemoving: true);
			SetGhostVisibility(deploymentPoint, isVisible: false);
			SetDeploymentTargetContourState(deploymentPoint, isHighlighted: false);
			SetLaddersUpState(deploymentPoint, isRaised: false);
			SetLightState(deploymentPoint, isVisible: false);
			break;
		case 2:
			HideDeploymentBanners(deploymentPoint, isRemoving: true);
			SetDeploymentTargetContourState(deploymentPoint, isHighlighted: false);
			SetLightState(deploymentPoint, isVisible: false);
			break;
		}
	}

	private static string GetSelectorStateDescription()
	{
		string text = "";
		for (int num = 1; num < 1023; num *= 2)
		{
			if ((deploymentVisualizerSelector & num) > 0)
			{
				string text2 = text;
				DeploymentVisualizationPreference deploymentVisualizationPreference = (DeploymentVisualizationPreference)num;
				text = text2 + " " + deploymentVisualizationPreference;
			}
		}
		return text;
	}

	[CommandLineArgumentFunction("set_deployment_visualization_selector", "mission")]
	public static string SetDeploymentVisualizationSelector(List<string> strings)
	{
		if (strings.Count == 1 && int.TryParse(strings[0], out deploymentVisualizerSelector))
		{
			return "Enabled deployment visualization options are:" + GetSelectorStateDescription();
		}
		return "Format is \"mission.set_deployment_visualization_selector [integer > 0]\".";
	}

	private void OnDeploymentStateChanged(DeploymentPoint deploymentPoint, SynchedMissionObject targetObject)
	{
		OnDeploymentPointStateSet(deploymentPoint);
	}

	private void OnDeploymentPointStateSet(DeploymentPoint deploymentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		DeploymentPointState deploymentPointState = deploymentPoint.GetDeploymentPointState();
		switch ((int)deploymentPointState)
		{
		case 1:
		case 3:
			if ((deploymentVisualizerSelector & 2) > 0)
			{
				ShowLineFromDeploymentPointToTarget(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 4) > 0)
			{
				CreateArcPoints(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 8) > 0)
			{
				ShowDeploymentBanners(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 0x10) > 0)
			{
				ShowPath(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 0x20) > 0)
			{
				SetGhostVisibility(deploymentPoint, isVisible: true);
			}
			SetLaddersUpState(deploymentPoint, isRaised: false);
			SetLightState(deploymentPoint, isVisible: false);
			break;
		case 2:
			if ((deploymentVisualizerSelector & 2) > 0)
			{
				ShowLineFromDeploymentPointToTarget(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 4) > 0)
			{
				CreateArcPoints(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 8) > 0)
			{
				ShowDeploymentBanners(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 0x40) > 0)
			{
				SetDeploymentTargetContourState(deploymentPoint, isHighlighted: true);
			}
			if ((deploymentVisualizerSelector & 0x80) > 0)
			{
				SetLaddersUpState(deploymentPoint, isRaised: true);
			}
			if ((deploymentVisualizerSelector & 0x100) > 0)
			{
				SetLightState(deploymentPoint, isVisible: true);
			}
			break;
		case 4:
			if ((deploymentVisualizerSelector & 2) > 0)
			{
				ShowLineFromDeploymentPointToTarget(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 4) > 0)
			{
				CreateArcPoints(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 8) > 0)
			{
				ShowDeploymentBanners(deploymentPoint);
			}
			if ((deploymentVisualizerSelector & 0x40) > 0)
			{
				SetDeploymentTargetContourState(deploymentPoint, isHighlighted: true);
			}
			if ((deploymentVisualizerSelector & 0x100) > 0)
			{
				SetLightState(deploymentPoint, isVisible: true);
			}
			break;
		case 0:
			if ((deploymentVisualizerSelector & 1) > 0)
			{
				if ((int)deploymentPoint.GetDeploymentPointType() == 0)
				{
					if ((deploymentVisualizerSelector & 2) > 0)
					{
						ShowLineFromDeploymentPointToTarget(deploymentPoint);
					}
					if ((deploymentVisualizerSelector & 4) > 0)
					{
						CreateArcPoints(deploymentPoint);
					}
					if ((deploymentVisualizerSelector & 8) > 0)
					{
						ShowDeploymentBanners(deploymentPoint);
					}
					if ((deploymentVisualizerSelector & 0x10) > 0)
					{
						ShowPath(deploymentPoint);
					}
					if ((deploymentVisualizerSelector & 0x20) > 0)
					{
						SetGhostVisibility(deploymentPoint, isVisible: true);
					}
				}
			}
			else if ((int)deploymentPoint.GetDeploymentPointType() == 0)
			{
				HideDeploymentBanners(deploymentPoint);
			}
			break;
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected I4, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenTick(dt);
		foreach (DeploymentPoint deploymentPoint in _deploymentPoints)
		{
			DeploymentPointState deploymentPointState = deploymentPoint.GetDeploymentPointState();
			switch ((int)deploymentPointState)
			{
			case 0:
				if ((deploymentVisualizerSelector & 1) > 0 && (int)deploymentPoint.GetDeploymentPointType() == 0)
				{
					if ((deploymentVisualizerSelector & 2) > 0)
					{
						ShowLineFromDeploymentPointToTarget(deploymentPoint);
					}
					if ((deploymentVisualizerSelector & 4) > 0)
					{
						ShowArcFromDeploymentPointToTarget(deploymentPoint);
					}
					if ((deploymentVisualizerSelector & 0x10) > 0)
					{
						ShowPath(deploymentPoint);
					}
				}
				break;
			case 2:
				if ((deploymentVisualizerSelector & 2) > 0)
				{
					ShowLineFromDeploymentPointToTarget(deploymentPoint);
				}
				if ((deploymentVisualizerSelector & 4) > 0)
				{
					ShowArcFromDeploymentPointToTarget(deploymentPoint);
				}
				break;
			case 1:
			case 3:
				if ((deploymentVisualizerSelector & 2) > 0)
				{
					ShowLineFromDeploymentPointToTarget(deploymentPoint);
				}
				if ((deploymentVisualizerSelector & 4) > 0)
				{
					ShowArcFromDeploymentPointToTarget(deploymentPoint);
				}
				if ((deploymentVisualizerSelector & 0x10) > 0)
				{
					ShowPath(deploymentPoint);
				}
				break;
			case 4:
				if ((deploymentVisualizerSelector & 2) > 0)
				{
					ShowLineFromDeploymentPointToTarget(deploymentPoint);
				}
				if ((deploymentVisualizerSelector & 4) > 0)
				{
					ShowArcFromDeploymentPointToTarget(deploymentPoint);
				}
				break;
			}
		}
	}

	private void ShowLineFromDeploymentPointToTarget(DeploymentPoint deploymentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		deploymentPoint.GetDeploymentOrigin();
		_ = deploymentPoint.DeploymentTargetPosition;
	}

	private List<Vec3> CreateArcPoints(DeploymentPoint deploymentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Vec3 deploymentOrigin = deploymentPoint.GetDeploymentOrigin();
		Vec3 deploymentTargetPosition = deploymentPoint.DeploymentTargetPosition;
		Vec3 val = deploymentTargetPosition - deploymentOrigin;
		float num = ((Vec3)(ref val)).Length / 3f;
		List<Vec3> list = new List<Vec3>();
		for (int i = 0; (float)i < num; i++)
		{
			Vec3 item = MBMath.Lerp(deploymentOrigin, deploymentTargetPosition, (float)i / num, 0f);
			float num2 = 8f - MathF.Pow(MathF.Abs((float)i - num * 0.5f) / (num * 0.5f), 1.2f) * 8f;
			item.z += num2;
			list.Add(item);
		}
		list.Add(deploymentTargetPosition);
		return list;
	}

	private void ShowArcFromDeploymentPointToTarget(DeploymentPoint deploymentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Vec3 deploymentTargetPosition = deploymentPoint.DeploymentTargetPosition;
		_deploymentArcs.TryGetValue(deploymentPoint, out var value);
		if (value == null || value[value.Count - 1] != deploymentTargetPosition)
		{
			value = CreateArcPoints(deploymentPoint);
		}
		Vec3 val = Vec3.Invalid;
		foreach (Vec3 item in value)
		{
			_ = ((Vec3)(ref val)).IsValid;
			val = item;
		}
	}

	private void ShowDeploymentBanners(DeploymentPoint deploymentPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Vec3 deploymentOrigin = deploymentPoint.GetDeploymentOrigin();
		Vec3 deploymentTargetPosition = deploymentPoint.DeploymentTargetPosition;
		_deploymentBanners.TryGetValue(deploymentPoint, out var value);
		if (value.Item1 == (GameEntity)null || value.Item2 == (GameEntity)null)
		{
			value = CreateBanners(deploymentPoint);
		}
		GameEntity item = _deploymentBanners[deploymentPoint].Item1;
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = deploymentOrigin;
		identity.origin.z += 7.5f;
		((Mat3)(ref identity.rotation)).ApplyScaleLocal(10f);
		MatrixFrame val = identity;
		item.SetFrame(ref val, true);
		item.SetVisibilityExcludeParents(true);
		item.SetAlpha(1f);
		GameEntity item2 = _deploymentBanners[deploymentPoint].Item2;
		identity = MatrixFrame.Identity;
		identity.origin = deploymentTargetPosition;
		identity.origin.z += 7.5f;
		((Mat3)(ref identity.rotation)).ApplyScaleLocal(10f);
		MatrixFrame val2 = identity;
		item2.SetFrame(ref val2, true);
		item2.SetVisibilityExcludeParents(true);
		item2.SetAlpha(1f);
	}

	private void HideDeploymentBanners(DeploymentPoint deploymentPoint, bool isRemoving = false)
	{
		_deploymentBanners.TryGetValue(deploymentPoint, out var value);
		if (value.Item1 != (GameEntity)null && value.Item2 != (GameEntity)null)
		{
			if (isRemoving)
			{
				value.Item1.Remove(104);
				value.Item2.Remove(105);
			}
			else
			{
				value.Item1.SetVisibilityExcludeParents(false);
				value.Item2.SetVisibilityExcludeParents(false);
			}
		}
	}

	private (GameEntity, GameEntity) CreateBanners(DeploymentPoint deploymentPoint)
	{
		GameEntity val = CreateBannerEntity(isTargetEntity: false);
		val.SetVisibilityExcludeParents(false);
		GameEntity val2 = CreateBannerEntity(isTargetEntity: true);
		val2.SetVisibilityExcludeParents(false);
		(GameEntity, GameEntity) tuple = (val, val2);
		_deploymentBanners.Add(deploymentPoint, tuple);
		return tuple;
	}

	private GameEntity CreateBannerEntity(bool isTargetEntity)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		GameEntity obj = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
		obj.EntityFlags = (EntityFlags)(obj.EntityFlags | 0x200);
		uint color = 4278190080u;
		uint color2 = (isTargetEntity ? 2131100887u : 2141323264u);
		obj.AddMultiMesh(MetaMesh.GetCopy("billboard_unit_mesh", true, false), true);
		obj.GetFirstMesh().Color = uint.MaxValue;
		Material val = Material.GetFromResource("formation_icon").CreateCopy();
		if (isTargetEntity)
		{
			Texture fromResource = Texture.GetFromResource("plain_yellow");
			val.SetTexture((MBTextureType)1, fromResource);
		}
		else
		{
			Texture fromResource2 = Texture.GetFromResource("plain_blue");
			val.SetTexture((MBTextureType)1, fromResource2);
		}
		obj.GetFirstMesh().SetMaterial(val);
		obj.GetFirstMesh().Color = color2;
		obj.GetFirstMesh().Color2 = color;
		obj.GetFirstMesh().SetVectorArgument(0f, 1f, 0f, 1f);
		return obj;
	}

	private void ShowPath(DeploymentPoint deploymentPoint)
	{
		SynchedMissionObject? obj = ((IEnumerable<SynchedMissionObject>)deploymentPoint.GetWeaponsUnder()).FirstOrDefault((Func<SynchedMissionObject, bool>)((SynchedMissionObject wu) => wu is IMoveableSiegeWeapon));
		((IMoveableSiegeWeapon)((obj is IMoveableSiegeWeapon) ? obj : null)).HighlightPath();
	}

	private void SetGhostVisibility(DeploymentPoint deploymentPoint, bool isVisible)
	{
	}

	private void SetDeploymentTargetContourState(DeploymentPoint deploymentPoint, bool isHighlighted)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		DeploymentPointState deploymentPointState = deploymentPoint.GetDeploymentPointState();
		WeakGameEntity gameEntity;
		if ((int)deploymentPointState == 2)
		{
			foreach (SiegeLadder associatedSiegeLadder in deploymentPoint.GetAssociatedSiegeLadders())
			{
				if (isHighlighted)
				{
					gameEntity = ((ScriptComponentBehavior)associatedSiegeLadder).GameEntity;
					((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)4289622555u, true);
				}
				else
				{
					gameEntity = ((ScriptComponentBehavior)associatedSiegeLadder).GameEntity;
					((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)null, true);
				}
			}
			return;
		}
		if ((int)deploymentPointState == 4)
		{
			if (isHighlighted)
			{
				gameEntity = ((ScriptComponentBehavior)deploymentPoint.AssociatedWallSegment).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)4289622555u, true);
			}
			else
			{
				gameEntity = ((ScriptComponentBehavior)deploymentPoint.AssociatedWallSegment).GameEntity;
				((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)null, true);
			}
		}
	}

	private void SetLaddersUpState(DeploymentPoint deploymentPoint, bool isRaised)
	{
		foreach (SiegeLadder associatedSiegeLadder in deploymentPoint.GetAssociatedSiegeLadders())
		{
			associatedSiegeLadder.SetUpStateVisibility(isRaised);
		}
	}

	private void SetLightState(DeploymentPoint deploymentPoint, bool isVisible)
	{
		_deploymentLights.TryGetValue(deploymentPoint, out var value);
		if (value != (GameEntity)null)
		{
			value.SetVisibilityExcludeParents(isVisible);
		}
		else if (isVisible)
		{
			CreateLight(deploymentPoint);
		}
	}

	private void CreateLight(DeploymentPoint deploymentPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = deploymentPoint.DeploymentTargetPosition + new Vec3(0f, 0f, ((int)deploymentPoint.GetDeploymentPointType() == 1) ? 10f : 3f, -1f);
		((Mat3)(ref identity.rotation)).RotateAboutSide(MathF.PI / 2f);
		Vec3 val = new Vec3(5f, 5f, 5f, -1f);
		((MatrixFrame)(ref identity)).Scale(ref val);
		GameEntity value = GameEntity.Instantiate(Mission.Current.Scene, "aserai_keep_interior_a_light_shaft_a", identity, true, "");
		_deploymentLights.Add(deploymentPoint, value);
	}
}
