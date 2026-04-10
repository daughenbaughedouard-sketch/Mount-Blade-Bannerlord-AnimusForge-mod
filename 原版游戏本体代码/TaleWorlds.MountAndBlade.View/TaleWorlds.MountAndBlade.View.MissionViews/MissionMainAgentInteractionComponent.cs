using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionMainAgentInteractionComponent
{
	public delegate void MissionFocusGainedEventDelegate(Agent agent, IFocusable focusableObject, bool isInteractable);

	public delegate void MissionFocusLostEventDelegate(Agent agent, IFocusable focusableObject);

	public delegate void MissionFocusHealthChangeDelegate(IFocusable focusable, float healthPercentage, bool hideHealthbarWhenFull);

	private IFocusable _currentInteractableObject;

	private sbyte _currentInteractableObjectBoneIndex;

	private readonly MissionMainAgentController _mainAgentController;

	public IFocusable CurrentFocusedObject { get; private set; }

	public IFocusable CurrentFocusedMachine { get; private set; }

	private Mission CurrentMission => ((MissionBehavior)_mainAgentController).Mission;

	private MissionScreen CurrentMissionScreen => _mainAgentController.MissionScreen;

	private Scene CurrentMissionScene => ((MissionBehavior)_mainAgentController).Mission.Scene;

	public event MissionFocusGainedEventDelegate OnFocusGained;

	public event MissionFocusLostEventDelegate OnFocusLost;

	public event MissionFocusHealthChangeDelegate OnFocusHealthChanged;

	public void SetCurrentFocusedObject(IFocusable focusedObject, IFocusable focusedMachine, sbyte focusedObjectBoneIndex, bool isInteractable)
	{
		if (CurrentFocusedObject != null && (CurrentFocusedObject != focusedObject || (_currentInteractableObject != null && !isInteractable) || (_currentInteractableObject == null && isInteractable)))
		{
			FocusLost(CurrentFocusedObject, CurrentFocusedMachine);
			_currentInteractableObject = null;
			_currentInteractableObjectBoneIndex = -1;
			CurrentFocusedObject = null;
			CurrentFocusedMachine = null;
		}
		if (CurrentFocusedObject == null && focusedObject != null)
		{
			if (focusedObject != CurrentFocusedObject)
			{
				FocusGained(focusedObject, focusedMachine, isInteractable);
			}
			if (isInteractable)
			{
				_currentInteractableObject = focusedObject;
			}
			CurrentFocusedObject = focusedObject;
			CurrentFocusedMachine = focusedMachine;
		}
		if (_currentInteractableObject != null && _currentInteractableObject == focusedObject)
		{
			_currentInteractableObjectBoneIndex = focusedObjectBoneIndex;
		}
	}

	public void ClearFocus()
	{
		if (CurrentFocusedObject != null)
		{
			FocusLost(CurrentFocusedObject, CurrentFocusedMachine);
		}
		_currentInteractableObject = null;
		_currentInteractableObjectBoneIndex = -1;
		CurrentFocusedObject = null;
		CurrentFocusedMachine = null;
	}

	public void OnClearScene()
	{
		ClearFocus();
	}

	public MissionMainAgentInteractionComponent(MissionMainAgentController mainAgentController)
	{
		_mainAgentController = mainAgentController;
	}

	private static float GetCollisionDistanceSquaredOfIntersectionFromMainAgentEye(Vec3 rayStartPoint, Vec3 rayDirection, float rayLength)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		float result = rayLength * rayLength;
		Vec3 val = rayStartPoint + rayDirection * rayLength;
		Vec3 position = Agent.Main.Position;
		float eyeGlobalHeight = Agent.Main.GetEyeGlobalHeight();
		Vec3 val2 = default(Vec3);
		((Vec3)(ref val2))._002Ector(position.x, position.y, position.z + eyeGlobalHeight, -1f);
		float num = val.z - val2.z;
		if (num < 0f)
		{
			num = MBMath.ClampFloat(0f - num, 0f, (Agent.Main.HasMount ? (eyeGlobalHeight - Agent.Main.MountAgent.GetEyeGlobalHeight()) : eyeGlobalHeight) * 0.75f);
			val2.z -= num;
			result = ((Vec3)(ref val2)).DistanceSquared(val);
		}
		return result;
	}

	private void FocusGained(IFocusable focusedObject, IFocusable focusedMachine, bool isInteractable)
	{
		focusedObject.OnFocusGain(Agent.Main);
		if (focusedMachine != null)
		{
			focusedMachine.OnFocusGain(Agent.Main);
		}
		foreach (MissionBehavior missionBehavior in CurrentMission.MissionBehaviors)
		{
			missionBehavior.OnFocusGained(Agent.Main, focusedObject, isInteractable);
		}
		this.OnFocusGained?.Invoke(Agent.Main, focusedObject, isInteractable);
	}

	private void FocusLost(IFocusable focusedObject, IFocusable focusedMachine)
	{
		focusedObject.OnFocusLose(Agent.Main);
		if (focusedMachine != null)
		{
			focusedMachine.OnFocusLose(Agent.Main);
		}
		foreach (MissionBehavior missionBehavior in CurrentMission.MissionBehaviors)
		{
			missionBehavior.OnFocusLost(Agent.Main, focusedObject);
		}
		this.OnFocusLost?.Invoke(Agent.Main, focusedObject);
	}

	public void FocusTick()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_061f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Invalid comparison between Unknown and I4
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Invalid comparison between Unknown and I4
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Invalid comparison between Unknown and I4
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Invalid comparison between Unknown and I4
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Invalid comparison between Unknown and I4
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		IFocusable val = null;
		sbyte focusedObjectBoneIndex = -1;
		UsableMachine val2 = null;
		bool flag = false;
		bool flag2 = false;
		if ((int)Mission.Current.Mode != 1 && (int)Mission.Current.Mode != 9)
		{
			Agent main = Agent.Main;
			if (main != null && (CurrentMission.IsMainAgentItemInteractionEnabled || IsFocusMountable()) && !CurrentMission.IsOrderMenuOpen && !((ScreenLayer)CurrentMissionScreen.SceneLayer).Input.IsGameKeyDown(25) && main.IsAbleToUseMachine())
			{
				float num = 10f;
				Vec3 direction = CurrentMissionScreen.CombatCamera.Direction;
				Vec3 val3 = direction;
				Vec3 position = CurrentMissionScreen.CombatCamera.Position;
				Vec3 position2 = main.Position;
				Vec3 val4 = new Vec3(position.x, position.y, 0f, -1f);
				float num2 = ((Vec3)(ref val4)).Distance(new Vec3(position2.x, position2.y, 0f, -1f));
				Vec3 val5 = position * (1f - num2) + (position + direction) * num2;
				float num3 = default(float);
				WeakGameEntity parent = default(WeakGameEntity);
				if (CurrentMissionScene.FocusRayCastForFixedPhysics(val5, val5 + val3 * num, ref num3, ref val4, ref parent, 0.01f, (BodyFlags)(-251707585)))
				{
					num = num3;
				}
				if (CurrentMissionScene.RayCastForClosestEntityOrTerrain(val5, val5 + val3 * num, ref num3, 0.01f, (BodyFlags)(-251707585)) && num3 < num)
				{
					num = num3;
				}
				float num4 = float.MaxValue;
				Agent val6 = null;
				float num5 = default(float);
				Agent val7 = CurrentMission.RayCastForClosestAgent(val5, val5 + val3 * (num + 0.01f), main.Index, 0.3f, ref num5);
				if (val7 != null && (int)val7.State != 4 && (int)val7.State != 3 && (!val7.IsMount || (val7.RiderAgent == null && main.MountAgent == null && main.CanReachAgent(val7))))
				{
					flag2 = main.CanInteractWithAgent(val7, CurrentMissionScreen.CameraElevation);
					if (flag2 || val7.IsEnemyOf(main))
					{
						num4 = num5;
						val = (IFocusable)(object)val7;
						focusedObjectBoneIndex = -1;
					}
					else
					{
						val6 = val7;
					}
				}
				float num6 = default(float);
				sbyte b = default(sbyte);
				Agent val8 = CurrentMission.RayCastForClosestAgentsLimbs(val5, val5 + val3 * (num + 0.01f), main.Index, 0.3f, ref num6, ref b);
				if (val8 != null && ((int)val8.State == 4 || (int)val8.State == 3) && (!val8.IsMount || (val8.RiderAgent == null && main.MountAgent == null && main.CanReachAgent(val8))) && num4 > num6)
				{
					flag2 = main.CanInteractWithAgent(val8, CurrentMissionScreen.CameraElevation);
					if (flag2 || val8.IsEnemyOf(main))
					{
						num4 = num6;
						val = (IFocusable)(object)val8;
						focusedObjectBoneIndex = b;
					}
				}
				float num7 = 3f;
				num += 0.1f;
				WeakGameEntity val9 = WeakGameEntity.Invalid;
				float rayLength = 0f;
				bool flag3 = false;
				float num8 = default(float);
				WeakGameEntity val10 = default(WeakGameEntity);
				if (CurrentMissionScene.FocusRayCastForFixedPhysics(val5, val5 + val3 * num, ref num8, ref val4, ref val10, 0.2f, (BodyFlags)79617) && num8 < num && num8 < num4)
				{
					num = num8;
					rayLength = num8;
					val9 = val10;
					flag3 = ((WeakGameEntity)(ref val9)).IsValid;
				}
				bool flag4 = false;
				for (int i = 0; i < 2; i++)
				{
					float num9 = MathF.Lerp(1f, num7, (float)i / 1f, 1E-05f);
					float num10 = 0.2f * (num9 - 1f);
					if (!CurrentMissionScene.RayCastForClosestEntityOrTerrain(val5 + val3 * num10, val5 + val3 * num, ref num8, ref val10, 0.2f * num9, (BodyFlags)79617) || !(num8 + num10 < num) || !(num8 + num10 < num4))
					{
						continue;
					}
					bool flag5 = false;
					WeakGameEntity val11 = val10;
					while (((WeakGameEntity)(ref val11)).IsValid)
					{
						if (((WeakGameEntity)(ref val11)).HasScriptWithInterfaceOfType<IFocusable>() && ((WeakGameEntity)(ref val11)).GetFirstScriptWithInterfaceOfType<IFocusable>().IsFocusable)
						{
							flag5 = true;
							break;
						}
						val11 = ((WeakGameEntity)(ref val11)).Parent;
					}
					if (!flag4 || flag5)
					{
						num = num8 + num10;
						rayLength = num8 + num10;
						val9 = val10;
						flag3 = ((WeakGameEntity)(ref val9)).IsValid;
						flag4 = true;
						if (flag5)
						{
							break;
						}
					}
				}
				if (flag3)
				{
					while (!((WeakGameEntity)(ref val9)).HasScriptWithInterfaceOfType<IFocusable>() || !((WeakGameEntity)(ref val9)).GetFirstScriptWithInterfaceOfType<IFocusable>().IsFocusable)
					{
						parent = ((WeakGameEntity)(ref val9)).Parent;
						if (!((WeakGameEntity)(ref parent)).IsValid)
						{
							break;
						}
						val9 = ((WeakGameEntity)(ref val9)).Parent;
					}
					val2 = ((WeakGameEntity)(ref val9)).GetFirstScriptOfType<UsableMachine>();
					if (val2 != null && !((MissionObject)val2).IsDisabled)
					{
						WeakGameEntity validVacantReachableStandingPointForAgent = val2.GetValidVacantReachableStandingPointForAgent(main);
						if (((WeakGameEntity)(ref validVacantReachableStandingPointForAgent)).IsValid)
						{
							val9 = validVacantReachableStandingPointForAgent;
						}
					}
					UsableMissionObject firstScriptOfType = ((WeakGameEntity)(ref val9)).GetFirstScriptOfType<UsableMissionObject>();
					if (firstScriptOfType is SpawnedItemEntity)
					{
						if (CurrentMission.IsMainAgentItemInteractionEnabled && firstScriptOfType.IsFocusable && !main.IsInWater() && main.CanReachObject(firstScriptOfType, GetCollisionDistanceSquaredOfIntersectionFromMainAgentEye(val5, val3, rayLength)))
						{
							val = (IFocusable)(object)firstScriptOfType;
							focusedObjectBoneIndex = -1;
							if (main.CanUseObject(firstScriptOfType))
							{
								flag = true;
							}
						}
					}
					else if (firstScriptOfType != null)
					{
						if (firstScriptOfType.IsFocusable)
						{
							val = (IFocusable)(object)firstScriptOfType;
							focusedObjectBoneIndex = -1;
							if (CurrentMission.IsMainAgentObjectInteractionEnabled && !main.IsUsingGameObject && main.IsAbleToUseMachine() && main.ObjectHasVacantPosition(firstScriptOfType) && main.CanUseObject(firstScriptOfType))
							{
								flag = true;
							}
						}
					}
					else if (val2 != null)
					{
						if (val2.IsFocusable)
						{
							val = (IFocusable)(object)val2;
							focusedObjectBoneIndex = -1;
							flag = !val2.IsDeactivated;
						}
					}
					else
					{
						IFocusable firstScriptWithInterfaceOfType = ((WeakGameEntity)(ref val9)).GetFirstScriptWithInterfaceOfType<IFocusable>();
						if (firstScriptWithInterfaceOfType != null && firstScriptWithInterfaceOfType.IsFocusable)
						{
							val = firstScriptWithInterfaceOfType;
							focusedObjectBoneIndex = -1;
						}
					}
				}
				if ((val == null || !flag) && main.MountAgent != null && main.CanInteractWithAgent(main.MountAgent, CurrentMissionScreen.CameraElevation))
				{
					val = (IFocusable)(object)main.MountAgent;
					focusedObjectBoneIndex = -1;
					flag2 = true;
				}
				if (val == null && val6 != null)
				{
					val = (IFocusable)(object)val6;
					flag2 = true;
				}
			}
			if (val == null)
			{
				ClearFocus();
				return;
			}
			bool isInteractable = ((val is Agent) ? flag2 : flag);
			SetCurrentFocusedObject(val, (IFocusable)(object)val2, focusedObjectBoneIndex, isInteractable);
		}
		else if (CurrentFocusedObject != null && (int)Mission.Current.Mode != 1)
		{
			ClearFocus();
		}
	}

	public void FocusStateCheckTick()
	{
		if (!((ScreenLayer)CurrentMissionScreen.SceneLayer).Input.IsGameKeyPressed(13) || (!CurrentMission.IsMainAgentItemInteractionEnabled && !IsFocusMountable()) || CurrentMissionScreen.IsRadialMenuActive || CurrentMission.IsOrderMenuOpen)
		{
			return;
		}
		Agent main = Agent.Main;
		StandingPoint val;
		if (main.IsUsingGameObject && !(main.CurrentlyUsedGameObject is SpawnedItemEntity) && (!(_currentInteractableObject is Agent) || (val = (StandingPoint)/*isinst with value type is only supported in some contexts*/) == null || val.PlayerStopsUsingWhenInteractsWithOther))
		{
			main.HandleStopUsingAction();
			ClearFocus();
			return;
		}
		IFocusable currentInteractableObject = _currentInteractableObject;
		UsableMissionObject val2;
		if ((val2 = (UsableMissionObject)(object)((currentInteractableObject is UsableMissionObject) ? currentInteractableObject : null)) != null)
		{
			if (!main.IsUsingGameObject && main.IsAbleToUseMachine() && !(val2 is SpawnedItemEntity) && main.ObjectHasVacantPosition(val2))
			{
				main.HandleStartUsingAction(val2, -1);
			}
			return;
		}
		IFocusable currentInteractableObject2 = _currentInteractableObject;
		Agent val3 = (Agent)(object)((currentInteractableObject2 is Agent) ? currentInteractableObject2 : null);
		if (main.IsAbleToUseMachine() && val3 != null)
		{
			val3.OnUse(main, _currentInteractableObjectBoneIndex);
		}
	}

	private bool IsFocusMountable()
	{
		IFocusable currentInteractableObject = _currentInteractableObject;
		Agent val = (Agent)(object)((currentInteractableObject is Agent) ? currentInteractableObject : null);
		if (val != null)
		{
			return val.IsMount;
		}
		return false;
	}

	public void FocusedItemHealthTick()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		IFocusable currentFocusedObject = CurrentFocusedObject;
		UsableMissionObject val;
		if ((val = (UsableMissionObject)(object)((currentFocusedObject is UsableMissionObject) ? currentFocusedObject : null)) != null)
		{
			WeakGameEntity val2 = ((ScriptComponentBehavior)val).GameEntity;
			while (((WeakGameEntity)(ref val2)).IsValid && !((WeakGameEntity)(ref val2)).HasScriptOfType<UsableMachine>())
			{
				val2 = ((WeakGameEntity)(ref val2)).Parent;
			}
			if (((WeakGameEntity)(ref val2)).IsValid)
			{
				UsableMachine firstScriptOfType = ((WeakGameEntity)(ref val2)).GetFirstScriptOfType<UsableMachine>();
				if (((firstScriptOfType != null) ? firstScriptOfType.DestructionComponent : null) != null)
				{
					this.OnFocusHealthChanged?.Invoke(CurrentFocusedObject, firstScriptOfType.DestructionComponent.HitPoint / firstScriptOfType.DestructionComponent.MaxHitPoint, hideHealthbarWhenFull: true);
				}
			}
			return;
		}
		IFocusable currentFocusedObject2 = CurrentFocusedObject;
		UsableMachine val3;
		if ((val3 = (UsableMachine)(object)((currentFocusedObject2 is UsableMachine) ? currentFocusedObject2 : null)) != null)
		{
			if (val3.DestructionComponent != null)
			{
				this.OnFocusHealthChanged?.Invoke(CurrentFocusedObject, val3.DestructionComponent.HitPoint / val3.DestructionComponent.MaxHitPoint, hideHealthbarWhenFull: true);
			}
			return;
		}
		IFocusable currentFocusedObject3 = CurrentFocusedObject;
		DestructableComponent val4;
		if ((val4 = (DestructableComponent)(object)((currentFocusedObject3 is DestructableComponent) ? currentFocusedObject3 : null)) != null)
		{
			this.OnFocusHealthChanged?.Invoke(CurrentFocusedObject, val4.HitPoint / val4.MaxHitPoint, hideHealthbarWhenFull: true);
		}
	}
}
