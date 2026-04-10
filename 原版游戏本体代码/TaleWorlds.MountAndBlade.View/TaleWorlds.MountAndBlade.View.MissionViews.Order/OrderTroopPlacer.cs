using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Order;

public class OrderTroopPlacer : MissionView
{
	public enum CursorState
	{
		Invisible,
		Normal,
		Ground,
		Rotation,
		Count,
		OrderableEntity
	}

	private bool _suspendTroopPlacer;

	public bool IsDrawingForced;

	public bool IsDrawingFacing;

	public bool IsDrawingForming;

	public Action OnUnitDeployed;

	private bool _isMouseDown;

	private List<GameEntity> _orderPositionEntities;

	private List<GameEntity> _orderRotationEntities;

	private bool _formationDrawingMode;

	private Formation _mouseOverFormation;

	private Vec2 _lastMousePosition;

	private Vec2 _deltaMousePosition;

	private int _mouseOverDirection;

	private WorldPosition? _formationDrawingStartingPosition;

	private Vec2? _formationDrawingStartingPointOfMouse;

	private float? _formationDrawingStartingTime;

	private bool _restrictOrdersToDeploymentBoundaries;

	private bool _initialized;

	private Timer formationDrawTimer;

	private bool _wasDrawingForced;

	private bool _wasDrawingFacing;

	private bool _wasDrawingForming;

	private GameEntity _widthEntityLeft;

	private GameEntity _widthEntityRight;

	private bool _isDrawnThisFrame;

	private bool _wasDrawnPreviousFrame;

	private OrderController _orderController;

	public bool SuspendTroopPlacer
	{
		get
		{
			return _suspendTroopPlacer;
		}
		set
		{
			_suspendTroopPlacer = value;
			if (value)
			{
				HideOrderPositionEntities();
			}
			else
			{
				_formationDrawingStartingPosition = null;
			}
			Reset();
		}
	}

	public OrderFlag OrderFlag { get; private set; }

	private Team _playerTeam => ((MissionBehavior)this).Mission.PlayerTeam;

	protected CursorState ActiveCursorState { get; private set; }

	protected OrderController OrderController
	{
		get
		{
			if (_orderController != null)
			{
				return _orderController;
			}
			return Mission.Current.PlayerTeam.PlayerOrderController;
		}
	}

	private bool IsDeployment
	{
		get
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			Mission mission = ((MissionBehavior)this).Mission;
			if (mission == null)
			{
				return false;
			}
			return (int)mission.Mode == 6;
		}
	}

	public OrderTroopPlacer(OrderController orderController)
	{
		_orderController = orderController;
	}

	protected virtual OrderFlag CreateOrderFlag()
	{
		return new OrderFlag(((MissionBehavior)this).Mission, base.MissionScreen);
	}

	protected virtual bool CanUpdate()
	{
		return ((List<Formation>)(object)OrderController.SelectedFormations).Count > 0;
	}

	protected virtual bool HasSelectedFormations()
	{
		return ((List<Formation>)(object)OrderController.SelectedFormations).Count > 0;
	}

	protected virtual CursorState GetCursorState()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		CursorState cursorState = CursorState.Invisible;
		if (HasSelectedFormations())
		{
			if (!TryGetScreenMiddleToWorldPosition(out var _, out var collisionDistance, out var collidedEntity))
			{
				collisionDistance = 1000f;
			}
			if (cursorState == CursorState.Invisible && collisionDistance < 1000f)
			{
				if (!_formationDrawingMode && !((WeakGameEntity)(ref collidedEntity)).IsValid)
				{
					for (int i = 0; i < _orderRotationEntities.Count; i++)
					{
						GameEntity val = _orderRotationEntities[i];
						if (val.IsVisibleIncludeParents() && collidedEntity == val)
						{
							_mouseOverFormation = ((IEnumerable<Formation>)OrderController.SelectedFormations).ElementAt(i / 2);
							_mouseOverDirection = 1 - (i & 1);
							cursorState = CursorState.Rotation;
							break;
						}
					}
				}
				if (cursorState == CursorState.Invisible && base.MissionScreen.OrderFlag.FocusedOrderableObject != null)
				{
					cursorState = CursorState.OrderableEntity;
				}
				if (cursorState == CursorState.Invisible)
				{
					cursorState = GetGroundOrNormalCursor();
				}
			}
		}
		if (cursorState != CursorState.Ground && cursorState != CursorState.Rotation)
		{
			_mouseOverDirection = 0;
		}
		return cursorState;
	}

	protected virtual Vec3 GetGroundedVec3(WorldPosition worldPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return ((WorldPosition)(ref worldPosition)).GetGroundVec3();
	}

	protected virtual bool TryGetScreenMiddleToWorldPosition(out WorldPosition worldPosition, out float collisionDistance, out WeakGameEntity collidedEntity)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(), out var rayBegin, out var rayEnd);
		float num = default(float);
		WeakGameEntity val = default(WeakGameEntity);
		if (((MissionBehavior)this).Mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, ref num, ref val, 0.3f, (BodyFlags)67188481))
		{
			Vec3 val2 = rayEnd - rayBegin;
			((Vec3)(ref val2)).Normalize();
			collisionDistance = num;
			collidedEntity = val;
			worldPosition = new WorldPosition(((MissionBehavior)this).Mission.Scene, UIntPtr.Zero, rayBegin + val2 * collisionDistance, false);
			return true;
		}
		worldPosition = WorldPosition.Invalid;
		collisionDistance = 0f;
		collidedEntity = WeakGameEntity.Invalid;
		return false;
	}

	protected bool TryGetScreenMiddleToWorldPosition(out WorldPosition worldPosition, out float collisionDistance)
	{
		WeakGameEntity collidedEntity;
		return TryGetScreenMiddleToWorldPosition(out worldPosition, out collisionDistance, out collidedEntity);
	}

	protected bool TryGetScreenMiddleToWorldPosition(out WorldPosition worldPosition, out WeakGameEntity collidedEntity)
	{
		float collisionDistance;
		return TryGetScreenMiddleToWorldPosition(out worldPosition, out collisionDistance, out collidedEntity);
	}

	protected bool TryGetScreenMiddleToWorldPosition(out WorldPosition worldPosition)
	{
		float collisionDistance;
		WeakGameEntity collidedEntity;
		return TryGetScreenMiddleToWorldPosition(out worldPosition, out collisionDistance, out collidedEntity);
	}

	protected Vec2 GetScreenPoint()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!((ScreenBase)base.MissionScreen).MouseVisible)
		{
			return new Vec2(0.5f, 0.5f) + _deltaMousePosition;
		}
		return base.Input.GetMousePositionRanged() + _deltaMousePosition;
	}

	public CursorState GetGroundOrNormalCursor()
	{
		if (!_formationDrawingMode)
		{
			return CursorState.Normal;
		}
		return CursorState.Ground;
	}

	public override void AfterStart()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		((MissionBehavior)this).AfterStart();
		OrderFlag = CreateOrderFlag();
		_formationDrawingStartingPosition = null;
		_formationDrawingStartingPointOfMouse = null;
		_formationDrawingStartingTime = null;
		_orderRotationEntities = new List<GameEntity>();
		_orderPositionEntities = new List<GameEntity>();
		formationDrawTimer = new Timer(MBCommon.GetApplicationTime(), 1f / 30f, true);
		_widthEntityLeft = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
		_widthEntityLeft.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_arrow_a", true, false));
		_widthEntityLeft.SetVisibilityExcludeParents(false);
		_widthEntityRight = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
		_widthEntityRight.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_arrow_a", true, false));
		_widthEntityRight.SetVisibilityExcludeParents(false);
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		if (!_initialized)
		{
			MissionPeer val = (GameNetwork.IsMyPeerReady ? PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer) : null);
			if (((MissionBehavior)this).Mission.PlayerTeam != null || (val != null && (val.Team == ((MissionBehavior)this).Mission.AttackerTeam || val.Team == ((MissionBehavior)this).Mission.DefenderTeam)))
			{
				_initialized = true;
			}
		}
	}

	public void RestrictOrdersToDeploymentBoundaries(bool enabled)
	{
		_restrictOrdersToDeploymentBoundaries = enabled;
	}

	private void UpdateFormationDrawingForFacingOrder(bool giveOrder)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		_isDrawnThisFrame = true;
		Vec3 groundPosition = base.MissionScreen.GetOrderFlagPosition();
		Vec2 asVec = ((Vec3)(ref groundPosition)).AsVec2;
		Vec2 orderLookAtDirection = OrderController.GetOrderLookAtDirection((IEnumerable<Formation>)OrderController.SelectedFormations, asVec);
		List<WorldPosition> list = default(List<WorldPosition>);
		OrderController.SimulateNewFacingOrder(orderLookAtDirection, ref list);
		int num = 0;
		HideOrderPositionEntities();
		foreach (WorldPosition item in list)
		{
			int entityIndex = num;
			groundPosition = GetGroundedVec3(item);
			AddOrderPositionEntity(entityIndex, in groundPosition, giveOrder);
			num++;
		}
	}

	private void UpdateFormationDrawingForDestination(bool giveOrder)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_isDrawnThisFrame = true;
		List<WorldPosition> list = default(List<WorldPosition>);
		OrderController.SimulateDestinationFrames(ref list, 3f);
		int num = 0;
		HideOrderPositionEntities();
		foreach (WorldPosition item in list)
		{
			AddOrderPositionEntity(num, GetGroundedVec3(item), giveOrder, 0.7f);
			num++;
		}
	}

	private void UpdateFormationDrawingForFormingOrder(bool giveOrder)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		_isDrawnThisFrame = true;
		MatrixFrame orderFlagFrame = base.MissionScreen.GetOrderFlagFrame();
		Vec3 origin = orderFlagFrame.origin;
		Vec2 asVec = ((Vec3)(ref orderFlagFrame.rotation.f)).AsVec2;
		float orderFormCustomWidth = OrderController.GetOrderFormCustomWidth((IEnumerable<Formation>)OrderController.SelectedFormations, origin);
		List<WorldPosition> list = default(List<WorldPosition>);
		OrderController.SimulateNewCustomWidthOrder(orderFormCustomWidth, ref list);
		Formation val = Extensions.MaxBy<Formation, int>((IEnumerable<Formation>)OrderController.SelectedFormations, (Func<Formation, int>)((Formation f) => f.CountOfUnits));
		int num = 0;
		HideOrderPositionEntities();
		foreach (WorldPosition item in list)
		{
			WorldPosition current = item;
			((WorldPosition)(ref current)).GetNavMesh();
			AddOrderPositionEntity(num, GetGroundedVec3(current), giveOrder);
			num++;
		}
		float unitDiameter = val.UnitDiameter;
		float interval = val.Interval;
		int num2 = MathF.Max(0, (int)((orderFormCustomWidth - unitDiameter) / (interval + unitDiameter) + 1E-05f)) + 1;
		float num3 = (float)(num2 - 1) * (interval + unitDiameter);
		Vec2 val2 = default(Vec2);
		WorldPosition worldPosition = default(WorldPosition);
		for (int num4 = 0; num4 < num2; num4++)
		{
			((Vec2)(ref val2))._002Ector((float)num4 * (interval + unitDiameter) - num3 / 2f, 0f);
			Vec2 val3 = ((Vec2)(ref asVec)).TransformToParentUnitF(val2);
			((WorldPosition)(ref worldPosition))._002Ector(Mission.Current.Scene, UIntPtr.Zero, origin, false);
			((WorldPosition)(ref worldPosition)).SetVec2(((WorldPosition)(ref worldPosition)).AsVec2 + val3);
			AddOrderPositionEntity(num++, GetGroundedVec3(worldPosition), fadeOut: false);
		}
	}

	public void UpdateFormationDrawing(bool giveOrder)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		_isDrawnThisFrame = true;
		HideOrderPositionEntities();
		if (!_formationDrawingStartingPosition.HasValue)
		{
			return;
		}
		WorldPosition val = WorldPosition.Invalid;
		bool flag = false;
		if (((ScreenBase)base.MissionScreen).MouseVisible && _formationDrawingStartingPointOfMouse.HasValue)
		{
			Vec2 val2 = _formationDrawingStartingPointOfMouse.Value - base.Input.GetMousePositionPixel();
			if (MathF.Abs(val2.x) < 10f && MathF.Abs(val2.y) < 10f)
			{
				flag = true;
				val = _formationDrawingStartingPosition.Value;
			}
		}
		if (((ScreenBase)base.MissionScreen).MouseVisible && _formationDrawingStartingTime.HasValue && ((MissionBehavior)this).Mission.CurrentTime - _formationDrawingStartingTime.Value < 0.3f)
		{
			flag = true;
			val = _formationDrawingStartingPosition.Value;
		}
		if (!flag)
		{
			if (!TryGetScreenMiddleToWorldPosition(out var worldPosition))
			{
				return;
			}
			val = worldPosition;
		}
		WorldPosition val3;
		if (_mouseOverDirection == 1)
		{
			val3 = val;
			val = _formationDrawingStartingPosition.Value;
		}
		else
		{
			val3 = _formationDrawingStartingPosition.Value;
		}
		if (!OrderFlag.IsPositionOnValidGround(val3))
		{
			return;
		}
		Vec2 val4;
		if (_restrictOrdersToDeploymentBoundaries && ((MissionBehavior)this).Mission.DeploymentPlan.HasDeploymentBoundaries(((MissionBehavior)this).Mission.PlayerTeam))
		{
			IMissionDeploymentPlan deploymentPlan = ((MissionBehavior)this).Mission.DeploymentPlan;
			Team playerTeam = ((MissionBehavior)this).Mission.PlayerTeam;
			val4 = ((WorldPosition)(ref val3)).AsVec2;
			if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(playerTeam, ref val4))
			{
				return;
			}
		}
		bool isFormationLayoutVertical = !((MissionBehavior)this).DebugInput.IsControlDown();
		UpdateFormationDrawingForMovementOrder(giveOrder, val3, val, isFormationLayoutVertical);
		Vec2 deltaMousePosition = _deltaMousePosition;
		val4 = base.Input.GetMousePositionRanged() - _lastMousePosition;
		_deltaMousePosition = deltaMousePosition * MathF.Max(1f - ((Vec2)(ref val4)).Length * 10f, 0f);
		_lastMousePosition = base.Input.GetMousePositionRanged();
	}

	private void UpdateFormationDrawingForMovementOrder(bool giveOrder, WorldPosition formationRealStartingPosition, WorldPosition formationRealEndingPosition, bool isFormationLayoutVertical)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		_isDrawnThisFrame = true;
		List<WorldPosition> list = default(List<WorldPosition>);
		OrderController.SimulateNewOrderWithPositionAndDirection(formationRealStartingPosition, formationRealEndingPosition, ref list, isFormationLayoutVertical);
		if (giveOrder)
		{
			if (!isFormationLayoutVertical)
			{
				OrderController.SetOrderWithTwoPositions((OrderType)3, formationRealStartingPosition, formationRealEndingPosition);
			}
			else
			{
				OrderController.SetOrderWithTwoPositions((OrderType)2, formationRealStartingPosition, formationRealEndingPosition);
			}
		}
		int num = 0;
		foreach (WorldPosition item in list)
		{
			AddOrderPositionEntity(num, GetGroundedVec3(item), giveOrder);
			num++;
		}
	}

	private void HandleMouseDown()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		if (!HasSelectedFormations())
		{
			return;
		}
		switch (ActiveCursorState)
		{
		case CursorState.Normal:
		{
			_formationDrawingMode = true;
			if (TryGetScreenMiddleToWorldPosition(out var worldPosition2))
			{
				_formationDrawingStartingPosition = worldPosition2;
				_formationDrawingStartingPointOfMouse = base.Input.GetMousePositionPixel();
				_formationDrawingStartingTime = ((MissionBehavior)this).Mission.CurrentTime;
			}
			else
			{
				_formationDrawingStartingPosition = null;
				_formationDrawingStartingPointOfMouse = null;
				_formationDrawingStartingTime = null;
			}
			break;
		}
		case CursorState.Rotation:
			if (_mouseOverFormation.CountOfUnits > 0)
			{
				HideNonSelectedOrderRotationEntities(_mouseOverFormation);
				OrderController.ClearSelectedFormations();
				OrderController.SelectFormation(_mouseOverFormation);
				_formationDrawingMode = true;
				WorldPosition val = _mouseOverFormation.CreateNewOrderWorldPosition((WorldPositionEnforcedCache)2);
				Vec2 direction = _mouseOverFormation.Direction;
				((Vec2)(ref direction)).RotateCCW(-MathF.PI / 2f);
				_formationDrawingStartingPosition = val;
				WorldPosition value = _formationDrawingStartingPosition.Value;
				WorldPosition value2 = _formationDrawingStartingPosition.Value;
				((WorldPosition)(ref value)).SetVec2(((WorldPosition)(ref value2)).AsVec2 + direction * ((_mouseOverDirection == 1) ? 0.5f : (-0.5f)) * _mouseOverFormation.Width);
				WorldPosition worldPosition = val;
				((WorldPosition)(ref worldPosition)).SetVec2(((WorldPosition)(ref worldPosition)).AsVec2 + direction * ((_mouseOverDirection == 1) ? (-0.5f) : 0.5f) * _mouseOverFormation.Width);
				Vec2 val2 = base.MissionScreen.SceneView.WorldPointToScreenPoint(GetGroundedVec3(worldPosition));
				Vec2 screenPoint = GetScreenPoint();
				_deltaMousePosition = val2 - screenPoint;
				_lastMousePosition = base.Input.GetMousePositionRanged();
			}
			break;
		case CursorState.Invisible:
		case CursorState.Ground:
			break;
		}
	}

	private void HandleMouseUp()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (ActiveCursorState == CursorState.Ground)
		{
			if (IsDrawingFacing || _wasDrawingFacing)
			{
				UpdateFormationDrawingForFacingOrder(giveOrder: true);
			}
			else if (IsDrawingForming || _wasDrawingForming)
			{
				UpdateFormationDrawingForFormingOrder(giveOrder: true);
			}
			else
			{
				UpdateFormationDrawing(giveOrder: true);
			}
			if (IsDeployment)
			{
				OnUnitDeployed?.Invoke();
				UISoundsHelper.PlayUISound("event:/ui/mission/deploy");
			}
		}
		_formationDrawingMode = false;
		_deltaMousePosition = Vec2.Zero;
	}

	private void AddOrderPositionEntity(int entityIndex, in Vec3 groundPosition, bool fadeOut, float alpha = -1f)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		while (_orderPositionEntities.Count <= entityIndex)
		{
			GameEntity val = GameEntity.CreateEmpty(((MissionBehavior)this).Mission.Scene, true, true, true);
			val.EntityFlags = (EntityFlags)(val.EntityFlags | 0x400000);
			MetaMesh copy = MetaMesh.GetCopy("order_flag_small", true, false);
			val.AddComponent((GameEntityComponent)(object)copy);
			val.SetVisibilityExcludeParents(false);
			_orderPositionEntities.Add(val);
		}
		GameEntity val2 = _orderPositionEntities[entityIndex];
		Mat3 identity = Mat3.Identity;
		MatrixFrame val3 = new MatrixFrame(ref identity, ref groundPosition);
		val2.SetFrame(ref val3, true);
		if (alpha != -1f)
		{
			val2.SetVisibilityExcludeParents(true);
			val2.SetAlpha(alpha);
		}
		else if (fadeOut)
		{
			GameEntityExtensions.FadeOut(val2, 0.3f, false);
		}
		else
		{
			GameEntityExtensions.FadeIn(val2, true);
		}
	}

	private void HideNonSelectedOrderRotationEntities(Formation formation)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _orderRotationEntities.Count; i++)
		{
			GameEntity val = _orderRotationEntities[i];
			if (val == (GameEntity)null && val.IsVisibleIncludeParents() && ((IEnumerable<Formation>)OrderController.SelectedFormations).ElementAt(i / 2) != formation)
			{
				val.SetVisibilityExcludeParents(false);
				val.BodyFlag = (BodyFlags)(val.BodyFlag | 1);
			}
		}
	}

	private void HideOrderPositionEntities()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		foreach (GameEntity orderPositionEntity in _orderPositionEntities)
		{
			GameEntityExtensions.HideIfNotFadingOut(orderPositionEntity);
		}
		for (int i = 0; i < _orderRotationEntities.Count; i++)
		{
			GameEntity obj = _orderRotationEntities[i];
			obj.SetVisibilityExcludeParents(false);
			obj.BodyFlag = (BodyFlags)(obj.BodyFlag | 1);
		}
	}

	[Conditional("DEBUG")]
	private void DebugTick(float dt)
	{
		_ = _initialized;
	}

	private void Reset()
	{
		_isMouseDown = false;
		_formationDrawingMode = false;
		_formationDrawingStartingPosition = null;
		_formationDrawingStartingPointOfMouse = null;
		_formationDrawingStartingTime = null;
		_mouseOverFormation = null;
		ActiveCursorState = GetCursorState();
	}

	public override void OnMissionScreenTick(float dt)
	{
		if (!_initialized)
		{
			return;
		}
		ActiveCursorState = GetCursorState();
		base.OnMissionScreenTick(dt);
		if (!CanUpdate())
		{
			return;
		}
		_isDrawnThisFrame = false;
		if (SuspendTroopPlacer)
		{
			return;
		}
		if (base.Input.IsKeyPressed((InputKey)224) || base.Input.IsKeyPressed((InputKey)255))
		{
			_isMouseDown = true;
			HandleMouseDown();
		}
		if ((base.Input.IsKeyReleased((InputKey)224) || base.Input.IsKeyReleased((InputKey)255)) && _isMouseDown)
		{
			_isMouseDown = false;
			HandleMouseUp();
		}
		else if ((base.Input.IsKeyDown((InputKey)224) || base.Input.IsKeyDown((InputKey)255)) && _isMouseDown)
		{
			if (formationDrawTimer.Check(MBCommon.GetApplicationTime()) && !IsDrawingFacing && !IsDrawingForming && ActiveCursorState == CursorState.Ground && GetGroundOrNormalCursor() == CursorState.Ground)
			{
				UpdateFormationDrawing(giveOrder: false);
			}
		}
		else if (IsDrawingForced)
		{
			if (formationDrawTimer.Check(MBCommon.GetApplicationTime()))
			{
				Reset();
				HandleMouseDown();
				UpdateFormationDrawing(giveOrder: false);
			}
		}
		else if (IsDrawingFacing || _wasDrawingFacing)
		{
			if (IsDrawingFacing)
			{
				Reset();
				UpdateFormationDrawingForFacingOrder(giveOrder: false);
			}
		}
		else if (IsDrawingForming || _wasDrawingForming)
		{
			if (IsDrawingForming)
			{
				Reset();
				UpdateFormationDrawingForFormingOrder(giveOrder: false);
			}
		}
		else if (_wasDrawingForced)
		{
			Reset();
		}
		else
		{
			UpdateFormationDrawingForDestination(giveOrder: false);
		}
		if (!base.Input.IsKeyDown((InputKey)224) && !base.Input.IsKeyDown((InputKey)255) && _isMouseDown)
		{
			Reset();
		}
		foreach (GameEntity orderPositionEntity in _orderPositionEntities)
		{
			orderPositionEntity.SetPreviousFrameInvalid();
		}
		foreach (GameEntity orderRotationEntity in _orderRotationEntities)
		{
			orderRotationEntity.SetPreviousFrameInvalid();
		}
		_wasDrawingForced = IsDrawingForced;
		_wasDrawingFacing = IsDrawingFacing;
		_wasDrawingForming = IsDrawingForming;
		_wasDrawnPreviousFrame = _isDrawnThisFrame;
	}
}
