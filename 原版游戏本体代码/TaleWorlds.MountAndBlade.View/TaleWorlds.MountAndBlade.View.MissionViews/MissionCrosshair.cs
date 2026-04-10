using System;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

[DefaultView]
public class MissionCrosshair : MissionView
{
	private GameEntity[] _crosshairEntities;

	private GameEntity[] _arrowEntities;

	private float[] _gadgetOpacities;

	private const int GadgetCount = 7;

	public override void OnMissionScreenInitialize()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		base.OnMissionScreenInitialize();
		_crosshairEntities = (GameEntity[])(object)new GameEntity[3];
		_arrowEntities = (GameEntity[])(object)new GameEntity[4];
		_gadgetOpacities = new float[7];
		for (int i = 0; i < 3; i++)
		{
			_crosshairEntities[i] = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
			string text = i switch
			{
				1 => "crosshair_left", 
				0 => "crosshair_top", 
				_ => "crosshair_right", 
			};
			MetaMesh copy = MetaMesh.GetCopy(text, true, false);
			int meshCount = copy.MeshCount;
			for (int j = 0; j < meshCount; j++)
			{
				Mesh meshAtIndex = copy.GetMeshAtIndex(j);
				meshAtIndex.SetMeshRenderOrder(200);
				meshAtIndex.VisibilityMask = (VisibilityMaskFlags)1;
			}
			_crosshairEntities[i].AddComponent((GameEntityComponent)(object)copy);
			MatrixFrame identity = MatrixFrame.Identity;
			_crosshairEntities[i].Name = text;
			_crosshairEntities[i].SetFrame(ref identity, true);
			_crosshairEntities[i].SetVisibilityExcludeParents(false);
		}
		for (int k = 0; k < 4; k++)
		{
			_arrowEntities[k] = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
			string text2 = k switch
			{
				2 => "arrow_down", 
				1 => "arrow_right", 
				0 => "arrow_up", 
				_ => "arrow_left", 
			};
			MetaMesh copy2 = MetaMesh.GetCopy(text2, true, false);
			int meshCount2 = copy2.MeshCount;
			for (int l = 0; l < meshCount2; l++)
			{
				Mesh meshAtIndex2 = copy2.GetMeshAtIndex(l);
				meshAtIndex2.SetMeshRenderOrder(200);
				meshAtIndex2.VisibilityMask = (VisibilityMaskFlags)1;
			}
			_arrowEntities[k].AddComponent((GameEntityComponent)(object)copy2);
			MatrixFrame identity2 = MatrixFrame.Identity;
			_arrowEntities[k].Name = text2;
			_arrowEntities[k].SetFrame(ref identity2, true);
			_arrowEntities[k].SetVisibilityExcludeParents(false);
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Invalid comparison between Unknown and I4
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Invalid comparison between Unknown and I4
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b3: Invalid comparison between Unknown and I4
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Invalid comparison between Unknown and I4
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Invalid comparison between Unknown and I4
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Invalid comparison between Unknown and I4
		//IL_0680: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Invalid comparison between Unknown and I4
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0621: Unknown result type (might be due to invalid IL or missing references)
		//IL_0637: Expected I4, but got Unknown
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Expected I4, but got Unknown
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Invalid comparison between Unknown and I4
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Invalid comparison between Unknown and I4
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Invalid comparison between Unknown and I4
		base.OnMissionScreenTick(dt);
		bool flag = false;
		float[] array = new float[8];
		for (int i = 0; i < 7; i++)
		{
			array[i] = 0f;
		}
		if (MBEditor.EditModeEnabled && (_crosshairEntities[0] == (GameEntity)null || _arrowEntities[0] == (GameEntity)null))
		{
			return;
		}
		_crosshairEntities[0].SetVisibilityExcludeParents(false);
		_crosshairEntities[1].SetVisibilityExcludeParents(false);
		_crosshairEntities[2].SetVisibilityExcludeParents(false);
		_arrowEntities[0].SetVisibilityExcludeParents(false);
		_arrowEntities[1].SetVisibilityExcludeParents(false);
		_arrowEntities[2].SetVisibilityExcludeParents(false);
		_arrowEntities[3].SetVisibilityExcludeParents(false);
		if ((int)((MissionBehavior)this).Mission.Mode == 1 || (int)((MissionBehavior)this).Mission.Mode == 9)
		{
			return;
		}
		_ = base.MissionScreen.CombatCamera.Near;
		float num = 4.7f + (base.MissionScreen.CombatCamera.HorizontalFov - 0.64f) * 7.14f;
		if (((MissionBehavior)this).Mission.MainAgent != null)
		{
			Agent mainAgent = ((MissionBehavior)this).Mission.MainAgent;
			float num2 = base.MissionScreen.CameraViewAngle * (MathF.PI / 180f);
			float num3 = MathF.Tan((mainAgent.CurrentAimingError + mainAgent.CurrentAimingTurbulance) * (0.5f / MathF.Tan(num2 * 0.5f)));
			new Vec2(0.5f, 0.375f);
			Vec2 val = default(Vec2);
			((Vec2)(ref val))._002Ector(0f, num3);
			MatrixFrame frame = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame)).Elevate(-5f);
			ref Mat3 rotation = ref frame.rotation;
			Vec3 val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame)).Strafe(val.x);
			((MatrixFrame)(ref frame)).Advance(val.y);
			_crosshairEntities[0].SetFrame(ref frame, true);
			Vec2 val3 = val;
			((Vec2)(ref val3)).RotateCCW(MathF.PI * 13f / 18f);
			MatrixFrame frame2 = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame2)).Elevate(-5f);
			ref Mat3 rotation2 = ref frame2.rotation;
			val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation2)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame2)).Strafe(val3.x);
			((MatrixFrame)(ref frame2)).Advance(val3.y);
			_crosshairEntities[1].SetFrame(ref frame2, true);
			Vec2 val4 = val;
			((Vec2)(ref val4)).RotateCCW(MathF.PI * -13f / 18f);
			MatrixFrame frame3 = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame3)).Elevate(-5f);
			ref Mat3 rotation3 = ref frame3.rotation;
			val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation3)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame3)).Strafe(val4.x);
			((MatrixFrame)(ref frame3)).Advance(val4.y);
			_crosshairEntities[2].SetFrame(ref frame3, true);
			MatrixFrame frame4 = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame4)).Elevate(-5f);
			ref Mat3 rotation4 = ref frame4.rotation;
			val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation4)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame4)).Strafe(0f);
			((MatrixFrame)(ref frame4)).Advance(0.07499999f);
			_arrowEntities[0].SetFrame(ref frame4, true);
			MatrixFrame frame5 = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame5)).Elevate(-5f);
			ref Mat3 rotation5 = ref frame5.rotation;
			val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation5)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame5)).Strafe(0.14999998f);
			((MatrixFrame)(ref frame5)).Advance(-0.025000006f);
			_arrowEntities[1].SetFrame(ref frame5, true);
			MatrixFrame frame6 = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame6)).Elevate(-5f);
			ref Mat3 rotation6 = ref frame6.rotation;
			val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation6)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame6)).Strafe(0f);
			((MatrixFrame)(ref frame6)).Advance(-0.07499999f);
			_arrowEntities[2].SetFrame(ref frame6, true);
			MatrixFrame frame7 = base.MissionScreen.CombatCamera.Frame;
			((MatrixFrame)(ref frame7)).Elevate(-5f);
			ref Mat3 rotation7 = ref frame7.rotation;
			val2 = new Vec3(num, num, num, -1f);
			((Mat3)(ref rotation7)).ApplyScaleLocal(ref val2);
			((MatrixFrame)(ref frame7)).Strafe(-0.15f);
			((MatrixFrame)(ref frame7)).Advance(-0.025000006f);
			_arrowEntities[3].SetFrame(ref frame7, true);
			WeaponInfo wieldedWeaponInfo = mainAgent.GetWieldedWeaponInfo((HandIndex)0);
			val2 = mainAgent.LookDirection;
			Vec2 val5 = ((Vec3)(ref val2)).AsVec2;
			float rotationInRadians = ((Vec2)(ref val5)).RotationInRadians;
			val5 = mainAgent.GetMovementDirection();
			float num4 = MBMath.WrapAngle(rotationInRadians - ((Vec2)(ref val5)).RotationInRadians);
			if (((WeaponInfo)(ref wieldedWeaponInfo)).IsValid && !base.MissionScreen.IsViewingCharacter())
			{
				if (((WeaponInfo)(ref wieldedWeaponInfo)).IsRangedWeapon && BannerlordConfig.DisplayTargetingReticule)
				{
					Vec2 bodyRotationConstraint = mainAgent.GetBodyRotationConstraint(1);
					if (((MissionBehavior)this).Mission.MainAgent.MountAgent == null || MBMath.IsBetween(num4, bodyRotationConstraint.x, bodyRotationConstraint.y))
					{
						array[0] = 0.9f;
						array[1] = 0.9f;
						array[2] = 0.9f;
					}
					else if (((MissionBehavior)this).Mission.MainAgent.MountAgent != null && !MBMath.IsBetween(num4, bodyRotationConstraint.x, bodyRotationConstraint.y) && (bodyRotationConstraint.x < -0.1f || bodyRotationConstraint.y > 0.1f))
					{
						flag = true;
					}
				}
				else if (((WeaponInfo)(ref wieldedWeaponInfo)).IsMeleeWeapon)
				{
					ActionCodeType currentActionType = mainAgent.GetCurrentActionType(1);
					UsageDirection currentActionDirection = mainAgent.GetCurrentActionDirection(1);
					if (BannerlordConfig.DisplayAttackDirection && ((int)currentActionType == 19 || (int)currentActionDirection != -1))
					{
						if ((int)currentActionType == 19)
						{
							UsageDirection attackDirection = mainAgent.AttackDirection;
							switch ((int)attackDirection)
							{
							case 0:
								array[3] = 0.7f;
								break;
							case 3:
								array[4] = 0.7f;
								break;
							case 1:
								array[5] = 0.7f;
								break;
							case 2:
								array[6] = 0.7f;
								break;
							}
						}
						else
						{
							flag = true;
							switch (currentActionDirection - 4)
							{
							case 0:
								array[3] = 0.7f;
								break;
							case 3:
								array[4] = 0.7f;
								break;
							case 1:
								array[5] = 0.7f;
								break;
							case 2:
								array[6] = 0.7f;
								break;
							}
						}
					}
					else if (BannerlordConfig.DisplayAttackDirection || BannerlordConfig.AttackDirectionControl == 0)
					{
						UsageDirection val6 = mainAgent.PlayerAttackDirection();
						if ((int)val6 >= 0 && (int)val6 < 4)
						{
							if ((int)val6 == 0)
							{
								array[3] = 0.7f;
							}
							else if ((int)val6 == 3)
							{
								array[4] = 0.7f;
							}
							else if ((int)val6 == 1)
							{
								array[5] = 0.7f;
							}
							else if ((int)val6 == 2)
							{
								array[6] = 0.7f;
							}
						}
					}
				}
			}
		}
		for (int j = 0; j < 7; j++)
		{
			float num5 = dt;
			num5 = ((j >= 3) ? (num5 * 3f) : (num5 * 5f));
			if (array[j] > _gadgetOpacities[j])
			{
				_gadgetOpacities[j] += 1.2f * num5;
				if (_gadgetOpacities[j] > array[j])
				{
					_gadgetOpacities[j] = array[j];
				}
			}
			else if (array[j] < _gadgetOpacities[j])
			{
				_gadgetOpacities[j] -= num5;
				if (_gadgetOpacities[j] < array[j])
				{
					_gadgetOpacities[j] = array[j];
				}
			}
			int num6 = (int)(255f * _gadgetOpacities[j]);
			if (num6 > 0)
			{
				if (j < 3)
				{
					_crosshairEntities[j].SetVisibilityExcludeParents(true);
				}
				else
				{
					_arrowEntities[j - 3].SetVisibilityExcludeParents(true);
				}
			}
			num6 <<= 24;
			if (j < 3)
			{
				Mesh firstMesh = _crosshairEntities[j].GetFirstMesh();
				if (flag)
				{
					firstMesh.Color = (uint)(0xFF0000 | num6);
				}
				else
				{
					firstMesh.Color = (uint)(0xFFFFFF | num6);
				}
			}
			else
			{
				Mesh firstMesh2 = _arrowEntities[j - 3].GetFirstMesh();
				if (flag)
				{
					firstMesh2.Color = (uint)(0x33AAFF | num6);
				}
				else
				{
					firstMesh2.Color = (uint)(0xFFBB11 | num6);
				}
			}
		}
	}
}
