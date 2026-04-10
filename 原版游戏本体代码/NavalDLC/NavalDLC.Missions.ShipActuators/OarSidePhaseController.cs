using System;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions.ShipActuators;

public class OarSidePhaseController
{
	public enum OarSide
	{
		Left,
		Right
	}

	public const float RaisedPhase = MathF.PI;

	public const float LoweredPhase = 0f;

	private OarDeckParameters _averageDeckParameters;

	private float _lastPhase;

	private readonly MissionShip _ownerShip;

	public float Phase { get; private set; }

	public float CycleArcSizeMult { get; private set; }

	public float LastSlowDownFactor { get; private set; }

	public float VisualPhase { get; private set; }

	public float PhaseRate { get; private set; }

	public float NeededRevolutionRate { get; private set; }

	public float VisualVerticalBaseAngleOffsetFromShipRoll { get; private set; }

	public OarSide Side { get; }

	public OarSidePhaseController(MissionShip ownerShip, OarSide side)
	{
		Phase = MathF.PI;
		_lastPhase = MathF.PI;
		VisualPhase = MathF.PI;
		PhaseRate = 0f;
		NeededRevolutionRate = 0f;
		_ownerShip = ownerShip;
		Side = side;
	}

	public void SetAverageOarDeckParameters(OarDeckParameters averageDeckParameters)
	{
		_averageDeckParameters = averageDeckParameters;
	}

	public (float, float) ComputeForceAndSlowDownFactor(float rowerNeededPhaseRate, float shipForwardSpeed, float syncPhase, float targetPhaseRate, float oarsmenForceMultiplier, float oarFrictionMultiplier, float maxTipSpeed)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 1f;
		if (rowerNeededPhaseRate != 0f)
		{
			Vec3 val = MissionOar.ComputeBladeContactVelocityAux(_averageDeckParameters, syncPhase, targetPhaseRate);
			if (val.y <= 0f)
			{
				if (val.y < 0f - maxTipSpeed)
				{
					num2 = MathF.Abs(maxTipSpeed / val.y);
					val.y = 0f - maxTipSpeed;
				}
				float num3 = val.y * (float)MathF.Sign(rowerNeededPhaseRate);
				float num4 = num3 + shipForwardSpeed;
				if (num4 * rowerNeededPhaseRate <= 0f)
				{
					float num5 = MathF.Abs(MathF.Cos(syncPhase));
					float num6 = 1000f * oarsmenForceMultiplier;
					float num7 = 1.2f * oarFrictionMultiplier * 0.5f * NavalDLC.Missions.NavalPhysics.NavalPhysics.GetWaterDensity() * (0.45f * num5);
					num = num7 * num4 * num4 * (float)MathF.Sign(rowerNeededPhaseRate);
					if (MathF.Abs(num) > num6)
					{
						float num8 = MathF.Sqrt(num6 / num7);
						float num9 = (float)MathF.Sign(num4) * num8 - shipForwardSpeed;
						if (num9 * num3 < 0f || MathF.Abs(num9) < 0.8f)
						{
							num9 = (float)MathF.Sign(num9) * 0.8f;
						}
						num2 *= MathF.Abs(num9 / num3);
						if (num2 > 1f)
						{
							num2 = 1f;
						}
						num = (float)MathF.Sign(num) * num6;
					}
				}
			}
		}
		LastSlowDownFactor = num2;
		return (num, num2);
	}

	public void SetPhaseData(float phase, float phaseRate, float cycleArcSizeMult, float neededRevolutionRate)
	{
		PhaseRate = phaseRate;
		_lastPhase = Phase;
		Phase = phase;
		CycleArcSizeMult = cycleArcSizeMult;
		NeededRevolutionRate = neededRevolutionRate;
	}

	public void OnParallelTick(float dt)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		Mission.Current.Scene.GetInterpolationFactorForBodyWorldTransformSmoothing(ref num, ref num2);
		float num3 = MathF.Abs(_lastPhase - Phase);
		float num4 = MathF.Abs(_lastPhase - MathF.PI * 2f - Phase);
		float num5 = MathF.Abs(_lastPhase + MathF.PI * 2f - Phase);
		if (num3 < num4)
		{
			if (num5 < num3)
			{
				VisualPhase = MathF.Lerp(_lastPhase + MathF.PI * 2f, Phase, num, 1E-05f);
			}
			else
			{
				VisualPhase = MathF.Lerp(_lastPhase, Phase, num, 1E-05f);
			}
		}
		else if (num5 < num3)
		{
			VisualPhase = MathF.Lerp(_lastPhase + MathF.PI * 2f, Phase, num, 1E-05f);
		}
		else
		{
			VisualPhase = MathF.Lerp(_lastPhase - MathF.PI * 2f, Phase, num, 1E-05f);
		}
		VisualPhase = MBMath.WrapAngleSafe(VisualPhase);
		float num6 = 0f;
		if (PhaseRate != 0f)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)_ownerShip).GameEntity;
			MatrixFrame localFrame = ((WeakGameEntity)(ref gameEntity)).GetLocalFrame();
			num6 = 0f - ((Mat3)(ref localFrame.rotation)).GetEulerAngles().y;
			if (Side == OarSide.Left)
			{
				num6 = 0f - num6;
			}
			if (num6 < 0f)
			{
				num6 = 0f;
			}
		}
		VisualVerticalBaseAngleOffsetFromShipRoll = MBMath.Lerp(VisualVerticalBaseAngleOffsetFromShipRoll, num6, dt * 3f, 1E-05f);
	}

	public void Stop()
	{
		PhaseRate = 0f;
		NeededRevolutionRate = 0f;
	}

	public bool IsInRowingMotion()
	{
		return PhaseRate != 0f || (!MBMath.ApproximatelyEqualsTo(Phase, MathF.PI, 1E-05f) && !MBMath.ApproximatelyEqualsTo(MBMath.WrapAngleSafe(Phase), MathF.PI, 1E-05f)) || (!MBMath.ApproximatelyEqualsTo(VisualPhase, MathF.PI, 1E-05f) && !MBMath.ApproximatelyEqualsTo(MBMath.WrapAngleSafe(VisualPhase), MathF.PI, 1E-05f));
	}

	public float GetLastSubmergedHeightFactorForActuators()
	{
		return MathF.Clamp(_ownerShip.Physics.LastSubmergedHeightFactorForActuators, 0f, 1f);
	}
}
