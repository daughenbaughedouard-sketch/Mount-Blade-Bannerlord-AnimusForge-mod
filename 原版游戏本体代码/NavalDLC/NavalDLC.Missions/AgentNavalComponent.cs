using NavalDLC.Missions.AI.TeamAI;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.NavalPhysics;
using NavalDLC.Missions.Objects;
using NavalDLC.Missions.Objects.UsableMachines;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.Missions;

public class AgentNavalComponent : AgentComponent
{
	private const float OffShipCheckInterval = 5f;

	private const float BreatheCheckInterval = 5f;

	private const float BurnCheckInterval = 1f;

	private const float BreatheHoldMaxDurationBase = 60f;

	private AgentMovementMode _lastMovementMode;

	private float _lastBreatheTime;

	private float _lastBreatheCheckTime;

	private float _lastBurnCheckTime;

	private float _lastOffShipCheckTime;

	private float _breatheHoldMaxDurationFinal;

	private ulong _parentShipUniqueBitwiseID;

	private TeamAINavalComponent _teamAINavalComponent;

	private GameEntity _steppedEntityCache;

	private NavalDLC.Missions.NavalPhysics.NavalPhysics _steppedNavalPhysicsCached;

	private PlankBridgeSteppedAgentManager _steppedPlankBridgeSteppedAgentManagerCached;

	private readonly NavalShipsLogic _navalShipsLogic;

	private readonly NavalAgentsLogic _navalAgentsLogic;

	public MissionShip SteppedShip { get; private set; }

	public MissionShip FormationShip { get; private set; }

	public bool BlockDrowning { get; private set; }

	public bool BlockBurning { get; private set; }

	public float LastJumpOffTime { get; private set; }

	public AgentNavalComponent(Agent agent)
		: base(agent)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_navalShipsLogic = Mission.Current.GetMissionBehavior<NavalShipsLogic>();
		_navalAgentsLogic = Mission.Current.GetMissionBehavior<NavalAgentsLogic>();
		_lastMovementMode = (AgentMovementMode)0;
		_lastBreatheTime = agent.Mission.CurrentTime;
	}

	public override void Initialize()
	{
		_breatheHoldMaxDurationFinal = MissionGameModels.Current.AgentStatCalculateModel.GetBreatheHoldMaxDuration(base.Agent, 60f) * (MBRandom.RandomFloat + 0.5f);
		Mission.Current.DeploymentFinishedEvent += OnDeploymentFinished;
		_lastBreatheCheckTime = base.Agent.Mission.CurrentTime + MBRandom.RandomFloat * 5f;
		_lastOffShipCheckTime = base.Agent.Mission.CurrentTime + MBRandom.RandomFloat * 5f;
		_lastBurnCheckTime = base.Agent.Mission.CurrentTime + MBRandom.RandomFloat * 1f;
	}

	public override void OnComponentRemoved()
	{
		Mission.Current.DeploymentFinishedEvent -= OnDeploymentFinished;
	}

	public override void OnFormationSet()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		((AgentComponent)this).OnFormationSet();
		if (base.Agent.Formation != null)
		{
			Team team = base.Agent.Formation.Team;
			_teamAINavalComponent = ((team != null) ? team.TeamAI : null) as TeamAINavalComponent;
			if (_navalShipsLogic.GetShip(base.Agent.Team.TeamSide, base.Agent.Formation.FormationIndex, out var ship))
			{
				FormationShip = ship;
				_parentShipUniqueBitwiseID = FormationShip.ShipUniqueBitwiseID;
			}
			else
			{
				FormationShip = null;
				_parentShipUniqueBitwiseID = 0uL;
			}
		}
		else
		{
			FormationShip = null;
			_parentShipUniqueBitwiseID = 0uL;
		}
	}

	public void OnShipCaptured()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (_navalShipsLogic.GetShip(base.Agent.Team.TeamSide, base.Agent.Formation.FormationIndex, out var ship))
		{
			FormationShip = ship;
			_parentShipUniqueBitwiseID = FormationShip.ShipUniqueBitwiseID;
		}
		else
		{
			FormationShip = null;
			_parentShipUniqueBitwiseID = 0uL;
		}
	}

	public void SetCanDrown(bool canDrown)
	{
		BlockDrowning = !canDrown;
	}

	public void SetCanBurn(bool canBurn)
	{
		BlockBurning = !canBurn;
	}

	public float GetBreath()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		float num = (((int)base.Agent.Controller != 1) ? (_breatheHoldMaxDurationFinal * 2f) : _breatheHoldMaxDurationFinal);
		return MBMath.ClampFloat((_lastBreatheTime + 5f + num - base.Agent.Mission.CurrentTime) / num, 0f, 1f);
	}

	public ulong GetSteppedCombinedShipIsland()
	{
		if (SteppedShip != null)
		{
			return SteppedShip.ShipIslandCombinedID;
		}
		if (_steppedPlankBridgeSteppedAgentManagerCached != null)
		{
			ShipAttachmentMachine.ShipAttachment shipAttachment = _steppedPlankBridgeSteppedAgentManagerCached?.NavmeshHolder?.CurrentAttachment;
			if (shipAttachment != null)
			{
				return shipAttachment.AttachmentSource.OwnerShip.ShipIslandCombinedID;
			}
		}
		return 0uL;
	}

	public override void OnTickParallel(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Invalid comparison between Unknown and I4
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Invalid comparison between Unknown and I4
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		AgentMovementMode val = (AgentMovementMode)(base.Agent.MovementMode & 3);
		if ((int)val == 2 || (int)val == 3)
		{
			if ((int)_lastMovementMode != 2 && (int)_lastMovementMode != 3 && base.Agent.IsHuman)
			{
				base.Agent.SaveEquipmentsOnHand();
				if ((int)base.Agent.GetPrimaryWieldedItemIndex() != -1)
				{
					base.Agent.Mission.AddTickActionMT((MissionTickAction)0, base.Agent, 0, 1);
				}
				if ((int)base.Agent.GetOffhandWieldedItemIndex() != -1)
				{
					base.Agent.Mission.AddTickActionMT((MissionTickAction)0, base.Agent, 1, 1);
				}
				if (Mission.Current.MissionResult != null)
				{
					base.Agent.SetAgentFlags((AgentFlag)(base.Agent.GetAgentFlags() & -17));
				}
				else
				{
					base.Agent.SetAgentFlags((AgentFlag)(base.Agent.GetAgentFlags() & -25));
				}
			}
		}
		else if ((int)_lastMovementMode != 1 && (int)val == 1 && base.Agent.IsHuman)
		{
			if (Mission.Current.MissionResult != null)
			{
				base.Agent.SetAgentFlags((AgentFlag)(base.Agent.GetAgentFlags() | 0x10));
			}
			else
			{
				base.Agent.SetAgentFlags((AgentFlag)(base.Agent.GetAgentFlags() | 8 | 0x10));
			}
		}
		WeakGameEntity steppedEntity = base.Agent.GetSteppedEntity();
		if (steppedEntity != _steppedEntityCache)
		{
			_steppedEntityCache = GameEntity.CreateFromWeakEntity(steppedEntity);
			WeakGameEntity root = ((WeakGameEntity)(ref steppedEntity)).Root;
			SteppedShip = ((_steppedEntityCache != (GameEntity)null) ? (((WeakGameEntity)(ref root)).GetFirstScriptWithNameHash(MissionShip.MissionShipScriptNameHash) as MissionShip) : null);
			_navalAgentsLogic.OnAgentSteppedShipChanged(base.Agent, SteppedShip);
			_steppedNavalPhysicsCached = ((WeakGameEntity)(ref root)).GetFirstScriptOfType<NavalDLC.Missions.NavalPhysics.NavalPhysics>();
			_steppedPlankBridgeSteppedAgentManagerCached = ((WeakGameEntity)(ref root)).GetFirstScriptOfType<PlankBridgeSteppedAgentManager>();
		}
		_lastMovementMode = val;
	}

	public override void OnTick(float dt)
	{
		if (dt > 0f)
		{
			if (base.Agent.IsAIControlled && _lastOffShipCheckTime + 5f <= base.Agent.Mission.CurrentTime && _navalAgentsLogic.IsDeploymentFinished)
			{
				_lastOffShipCheckTime += 5f;
				CheckAgentOffShip();
			}
			if (!BlockDrowning && !base.Agent.Mission.MissionEnded && _lastBreatheCheckTime + 5f <= base.Agent.Mission.CurrentTime && _navalAgentsLogic.IsDeploymentFinished)
			{
				_lastBreatheCheckTime += 5f;
				CheckAgentDrown();
			}
			if (!BlockBurning && !base.Agent.Mission.MissionEnded && _lastBurnCheckTime + 1f <= base.Agent.Mission.CurrentTime && _navalAgentsLogic.IsDeploymentFinished)
			{
				_lastBurnCheckTime += 1f;
				CheckAgentBurn();
			}
			_steppedNavalPhysicsCached?.AddAgentWeightAndPositionInformation(base.Agent);
			_steppedPlankBridgeSteppedAgentManagerCached?.AddAgentWeightAndPositionInformation(base.Agent);
		}
	}

	private void CheckAgentOffShip()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)_lastMovementMode == 2 || (int)_lastMovementMode == 3)
		{
			if (FormationShip != null && (FormationShip.Physics.NavalSinkingState != NavalDLC.Missions.NavalPhysics.NavalPhysics.SinkingState.Floating || FormationShip.FireHitPoints <= 0f))
			{
				Formation val = _teamAINavalComponent?.GetNearestAllyShipFormation(base.Agent);
				if (val != null && _navalShipsLogic.GetShip(val, out var ship) && ship != FormationShip)
				{
					_navalAgentsLogic.TransferAgentToShip(base.Agent, ship);
					FormationShip = ship;
					_parentShipUniqueBitwiseID = ship.ShipUniqueBitwiseID;
				}
				else
				{
					_navalAgentsLogic.RemoveAgentFromShip(base.Agent, FormationShip);
					FormationShip = null;
					_parentShipUniqueBitwiseID = 0uL;
				}
			}
			else
			{
				if (FormationShip != null)
				{
					return;
				}
				Formation val2 = _teamAINavalComponent?.GetNearestAllyShipFormation(base.Agent);
				if (val2 != null && _navalShipsLogic.GetShip(val2, out var ship2))
				{
					if (base.Agent.IsUsingGameObject || AgentComponentExtensions.AIMoveToGameObjectIsEnabled(base.Agent))
					{
						base.Agent.StopUsingGameObjectMT(true, (StopUsingGameObjectFlags)1);
					}
					_navalAgentsLogic.AddAgentToShip(base.Agent, ship2);
					FormationShip = ship2;
					_parentShipUniqueBitwiseID = ship2.ShipUniqueBitwiseID;
				}
			}
		}
		else
		{
			if (SteppedShip == null || !(SteppedShip.FireHitPoints > 0f) || (_parentShipUniqueBitwiseID & SteppedShip.ShipIslandCombinedID) != 0L)
			{
				return;
			}
			Formation val3 = _teamAINavalComponent?.GetConnectedAllyFormation(SteppedShip.ShipUniqueBitwiseID);
			if (val3 != null && _navalShipsLogic.GetShip(val3, out var ship3) && _parentShipUniqueBitwiseID != ship3.ShipUniqueBitwiseID)
			{
				if (FormationShip != null)
				{
					_navalAgentsLogic.TransferAgentToShip(base.Agent, ship3);
				}
				else
				{
					if (base.Agent.IsUsingGameObject || AgentComponentExtensions.AIMoveToGameObjectIsEnabled(base.Agent))
					{
						base.Agent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
					}
					_navalAgentsLogic.AddAgentToShip(base.Agent, ship3);
				}
				FormationShip = ship3;
				_parentShipUniqueBitwiseID = ship3.ShipUniqueBitwiseID;
			}
			else if (FormationShip != null)
			{
				_navalAgentsLogic.RemoveAgentFromShip(base.Agent, FormationShip);
				FormationShip = null;
				_parentShipUniqueBitwiseID = 0uL;
			}
		}
	}

	private void CheckAgentDrown()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		AgentMovementMode val = (AgentMovementMode)(base.Agent.MovementMode & 3);
		if ((int)val != 3 && ((int)base.Agent.Controller != 1 || (int)val != 2))
		{
			_lastBreatheTime = base.Agent.Mission.CurrentTime;
		}
		if (GetBreath() <= 0f)
		{
			DrownAgent();
		}
	}

	private void CheckAgentBurn()
	{
		if (base.Agent.IsMainAgent && SteppedShip != null && SteppedShip.FireHitPoints <= 0f)
		{
			base.Agent.Mission.AddTickAction((MissionTickAction)5, base.Agent, 0, 0);
		}
	}

	public void DrownAgent()
	{
		base.Agent.Mission.AddTickAction((MissionTickAction)4, base.Agent, 0, 0);
	}

	public void SetupAgentToJumpOffABurningShip()
	{
		if (FormationShip != null)
		{
			_navalAgentsLogic.RemoveAgentFromShip(base.Agent, FormationShip);
		}
		SteppedShip = null;
		LastJumpOffTime = Mission.Current.CurrentTime;
	}

	private void OnDeploymentFinished()
	{
		_lastBreatheCheckTime = base.Agent.Mission.CurrentTime + MBRandom.RandomFloat * 5f;
		_lastOffShipCheckTime = base.Agent.Mission.CurrentTime + MBRandom.RandomFloat * 5f;
		_lastBurnCheckTime = base.Agent.Mission.CurrentTime + MBRandom.RandomFloat * 1f;
	}
}
