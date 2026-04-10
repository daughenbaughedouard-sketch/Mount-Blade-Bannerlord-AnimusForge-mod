using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Order;

public class OrderFlag
{
	private readonly GameEntity _entity;

	private readonly GameEntity _flag;

	private readonly GameEntity _gear;

	private readonly GameEntity _arrow;

	private readonly GameEntity _width;

	private readonly GameEntity _attack;

	private readonly GameEntity _flagUnavailable;

	private readonly GameEntity _widthLeft;

	private readonly GameEntity _widthRight;

	public bool IsTroop = true;

	private bool _isWidthVisible;

	private float _customWidth;

	private GameEntity _activeVisualEntity;

	protected readonly IEnumerable<IOrderableWithInteractionArea> _orderablesWithInteractionArea;

	protected readonly Mission _mission;

	protected readonly MissionScreen _missionScreen;

	private readonly float _arrowLength;

	private bool _isArrowVisible;

	private Vec2 _arrowDirection;

	public IOrderable FocusedOrderableObject { get; private set; }

	public int LatestUpdateFrameNo { get; private set; }

	public Vec3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _entity.GlobalPosition;
		}
		private set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			MatrixFrame frame = _entity.GetFrame();
			frame.origin = value;
			_entity.SetFrame(ref frame, true);
		}
	}

	public MatrixFrame Frame => _entity.GetGlobalFrame();

	public bool IsVisible
	{
		get
		{
			return _entity.IsVisibleIncludeParents();
		}
		set
		{
			_entity.SetVisibilityExcludeParents(value);
			if (!value)
			{
				FocusedOrderableObject = null;
			}
		}
	}

	public OrderFlag(Mission mission, MissionScreen missionScreen, float flagScale = 10f)
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		_mission = mission;
		_missionScreen = missionScreen;
		_entity = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_flag = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_gear = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_arrow = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_width = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_attack = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_flagUnavailable = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_widthLeft = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		_widthRight = GameEntity.CreateEmpty(_mission.Scene, true, true, true);
		GameEntity entity = _entity;
		entity.EntityFlags = (EntityFlags)(entity.EntityFlags | 0x400000);
		GameEntity flag = _flag;
		flag.EntityFlags = (EntityFlags)(flag.EntityFlags | 0x400000);
		GameEntity gear = _gear;
		gear.EntityFlags = (EntityFlags)(gear.EntityFlags | 0x400000);
		GameEntity arrow = _arrow;
		arrow.EntityFlags = (EntityFlags)(arrow.EntityFlags | 0x400000);
		GameEntity width = _width;
		width.EntityFlags = (EntityFlags)(width.EntityFlags | 0x400000);
		GameEntity attack = _attack;
		attack.EntityFlags = (EntityFlags)(attack.EntityFlags | 0x400000);
		GameEntity flagUnavailable = _flagUnavailable;
		flagUnavailable.EntityFlags = (EntityFlags)(flagUnavailable.EntityFlags | 0x400000);
		GameEntity widthLeft = _widthLeft;
		widthLeft.EntityFlags = (EntityFlags)(widthLeft.EntityFlags | 0x400000);
		GameEntity widthRight = _widthRight;
		widthRight.EntityFlags = (EntityFlags)(widthRight.EntityFlags | 0x400000);
		_flag.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_flag_a", true, false));
		MatrixFrame frame = _flag.GetFrame();
		Vec3 val = Vec3.One * flagScale;
		((MatrixFrame)(ref frame)).Scale(ref val);
		_flag.SetFrame(ref frame, true);
		_gear.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_gear", true, false));
		MatrixFrame frame2 = _gear.GetFrame();
		val = Vec3.One * flagScale;
		((MatrixFrame)(ref frame2)).Scale(ref val);
		_gear.SetFrame(ref frame2, true);
		_arrow.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_arrow_a", true, false));
		_widthLeft.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_arrow_a", true, false));
		_widthRight.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_arrow_a", true, false));
		MatrixFrame identity = MatrixFrame.Identity;
		((Mat3)(ref identity.rotation)).RotateAboutUp(-MathF.PI / 2f);
		_widthLeft.SetFrame(ref identity, true);
		identity = MatrixFrame.Identity;
		((Mat3)(ref identity.rotation)).RotateAboutUp(MathF.PI / 2f);
		_widthRight.SetFrame(ref identity, true);
		_width.AddChild(_widthLeft, false);
		_width.AddChild(_widthRight, false);
		MetaMesh copy = MetaMesh.GetCopy("destroy_icon", true, false);
		copy.RecomputeBoundingBox(true);
		MatrixFrame frame3 = copy.Frame;
		val = new Vec3(0.15f, 0.15f, 0.15f, -1f);
		((MatrixFrame)(ref frame3)).Scale(ref val);
		((MatrixFrame)(ref frame3)).Elevate(10f);
		copy.Frame = frame3;
		_attack.AddMultiMesh(copy, true);
		_flagUnavailable.AddComponent((GameEntityComponent)(object)MetaMesh.GetCopy("order_unavailable", true, false));
		_entity.AddChild(_flag, false);
		_entity.AddChild(_gear, false);
		_entity.AddChild(_arrow, false);
		_entity.AddChild(_width, false);
		_entity.AddChild(_attack, false);
		_entity.AddChild(_flagUnavailable, false);
		_flag.SetVisibilityExcludeParents(false);
		_gear.SetVisibilityExcludeParents(false);
		_arrow.SetVisibilityExcludeParents(false);
		_width.SetVisibilityExcludeParents(false);
		_attack.SetVisibilityExcludeParents(false);
		_flagUnavailable.SetVisibilityExcludeParents(false);
		SetActiveVisualEntity(_flag);
		BoundingBox boundingBox = _arrow.GetMetaMesh(0).GetBoundingBox();
		_arrowLength = boundingBox.max.y - boundingBox.min.y;
		UpdateFrame(out var _, checkForTargetEntity: false, Vec3.Invalid);
		_orderablesWithInteractionArea = ((IEnumerable)_mission.MissionObjects).OfType<IOrderableWithInteractionArea>();
	}

	public void Tick(float dt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		FocusedOrderableObject = null;
		WeakGameEntity val = WeakGameEntity.Invalid;
		bool isOnValidGround = false;
		bool flag = true;
		flag = !_mission.IsNavalBattle;
		Vec3 closestPoint = Vec3.Invalid;
		if (flag)
		{
			val = GetCollidedEntity(out closestPoint);
			if (((WeakGameEntity)(ref val)).IsValid)
			{
				BattleSideEnum side = Mission.Current.PlayerTeam.Side;
				IOrderable val4;
				IOrderable val2 = (IOrderable)((WeakGameEntity)(ref val)).GetScriptComponents().First((ScriptComponentBehavior sc) => (val4 = (IOrderable)(object)((sc is IOrderable) ? sc : null)) != null && (int)val4.GetOrder(side) > 0);
				if ((int)val2.GetOrder(side) != 0)
				{
					FocusedOrderableObject = val2;
				}
			}
		}
		UpdateFrame(out isOnValidGround, ((WeakGameEntity)(ref val)).IsValid, closestPoint);
		LatestUpdateFrameNo = Utilities.EngineFrameNo;
		if (!IsVisible)
		{
			return;
		}
		if (flag && FocusedOrderableObject == null)
		{
			FocusedOrderableObject = (IOrderable)(object)_orderablesWithInteractionArea.FirstOrDefault((Func<IOrderableWithInteractionArea, bool>)delegate(IOrderableWithInteractionArea o)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				WeakGameEntity gameEntity2 = ((ScriptComponentBehavior)o).GameEntity;
				return ((WeakGameEntity)(ref gameEntity2)).IsVisibleIncludeParents() && o.IsPointInsideInteractionArea(Position);
			});
			IOrderable focusedOrderableObject = FocusedOrderableObject;
			ScriptComponentBehavior val3;
			if ((val3 = (ScriptComponentBehavior)(object)((focusedOrderableObject is ScriptComponentBehavior) ? focusedOrderableObject : null)) != null)
			{
				WeakGameEntity gameEntity = val3.GameEntity;
				if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Scene == (NativeObject)null)
				{
					FocusedOrderableObject = null;
				}
			}
		}
		UpdateCurrentMesh(isOnValidGround);
		if (_activeVisualEntity == _flag || _activeVisualEntity == _flagUnavailable)
		{
			MatrixFrame frame = _flag.GetFrame();
			float num = MathF.Sin(MBCommon.GetApplicationTime() * 2f) + 1f;
			num *= 0.25f;
			frame.origin.z = num;
			_flag.SetFrame(ref frame, true);
			_flagUnavailable.SetFrame(ref frame, true);
		}
	}

	private void SetActiveVisualEntity(GameEntity entity)
	{
		_activeVisualEntity = entity;
		_flag.SetVisibilityExcludeParents(false);
		_gear.SetVisibilityExcludeParents(false);
		_arrow.SetVisibilityExcludeParents(false);
		_width.SetVisibilityExcludeParents(false);
		_attack.SetVisibilityExcludeParents(false);
		_flagUnavailable.SetVisibilityExcludeParents(false);
		_activeVisualEntity.SetVisibilityExcludeParents(true);
		if (_activeVisualEntity == _arrow || _activeVisualEntity == _flagUnavailable)
		{
			_flag.SetVisibilityExcludeParents(true);
		}
	}

	private void UpdateCurrentMesh(bool isOnValidGround)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		if (FocusedOrderableObject != null)
		{
			BattleSideEnum side = Mission.Current.PlayerTeam.Side;
			if ((int)FocusedOrderableObject.GetOrder(side) == 40)
			{
				SetActiveVisualEntity(_attack);
				return;
			}
			OrderType order = FocusedOrderableObject.GetOrder(side);
			if ((int)order == 39 || (int)order == 8)
			{
				SetActiveVisualEntity(_gear);
				return;
			}
		}
		if (_isArrowVisible)
		{
			SetActiveVisualEntity(_arrow);
		}
		else if (_isWidthVisible)
		{
			SetActiveVisualEntity(_width);
		}
		else
		{
			SetActiveVisualEntity(isOnValidGround ? _flag : _flagUnavailable);
		}
	}

	public void SetArrowVisibility(bool isVisible, Vec2 arrowDirection)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_isArrowVisible = isVisible;
		_arrowDirection = arrowDirection;
	}

	protected virtual Vec3 GetFlagPosition(out bool isOnValidGround, bool checkForTargetEntity, Vec3 targetCollisionPoint)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (_missionScreen.GetProjectedMousePositionOnGround(out var groundPosition, out var _, (BodyFlags)67108864, checkOccludedSurface: true))
		{
			if (!IsVisible)
			{
				isOnValidGround = false;
			}
			else if (checkForTargetEntity)
			{
				if (((Vec3)(ref targetCollisionPoint)).IsValid)
				{
					groundPosition = targetCollisionPoint;
				}
				else
				{
					Debug.FailedAssert("Collision point for target entity is invalid.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\MissionViews\\Order\\OrderFlag.cs", "GetFlagPosition", 349);
				}
				WorldPosition val = default(WorldPosition);
				((WorldPosition)(ref val))._002Ector(Mission.Current.Scene, UIntPtr.Zero, groundPosition, false);
				isOnValidGround = Mission.Current.IsOrderPositionAvailable(ref val, Mission.Current.PlayerTeam);
			}
			else
			{
				WorldPosition worldPosition = default(WorldPosition);
				((WorldPosition)(ref worldPosition))._002Ector(Mission.Current.Scene, UIntPtr.Zero, groundPosition, false);
				isOnValidGround = IsPositionOnValidGround(worldPosition);
			}
			return groundPosition;
		}
		isOnValidGround = false;
		return new Vec3(0f, 0f, -100000f, -1f);
	}

	protected virtual void UpdateFrame(out bool isOnValidGround, bool checkForTargetEntity, Vec3 targetCollisionPoint)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_missionScreen?.SceneView == (NativeObject)null)
		{
			isOnValidGround = false;
			return;
		}
		Vec3 flagPosition = GetFlagPosition(out isOnValidGround, checkForTargetEntity, targetCollisionPoint);
		if (((Vec3)(ref flagPosition)).IsValid)
		{
			Position = flagPosition;
			_missionScreen.ScreenPointToWorldRay(Vec2.One * 0.5f, out var rayBegin, out var rayEnd);
			float num;
			if (_missionScreen.LastFollowedAgent != null)
			{
				rayEnd = rayBegin - Position;
				Vec2 asVec = ((Vec3)(ref rayEnd)).AsVec2;
				num = ((Vec2)(ref asVec)).RotationInRadians;
			}
			else
			{
				MatrixFrame frame = _missionScreen.CombatCamera.Frame;
				num = ((Vec3)(ref frame.rotation.f)).RotationZ;
			}
			float num2 = num;
			MatrixFrame frame2 = _entity.GetFrame();
			frame2.rotation = Mat3.Identity;
			((Mat3)(ref frame2.rotation)).RotateAboutUp(num2);
			_entity.SetFrame(ref frame2, true);
			if (_isArrowVisible)
			{
				num2 = ((Vec2)(ref _arrowDirection)).RotationInRadians;
				Mat3 identity = Mat3.Identity;
				((Mat3)(ref identity)).RotateAboutUp(num2);
				MatrixFrame identity2 = MatrixFrame.Identity;
				identity2.rotation = ((Mat3)(ref frame2.rotation)).TransformToLocal(ref identity);
				((MatrixFrame)(ref identity2)).Advance(0f - _arrowLength);
				_arrow.SetFrame(ref identity2, true);
			}
			if (_isWidthVisible)
			{
				_widthLeft.SetLocalPosition(Vec3.Side * (_customWidth * 0.5f - 0f));
				_widthRight.SetLocalPosition(Vec3.Side * (_customWidth * -0.5f + 0f));
				_widthLeft.SetLocalPosition(Vec3.Side * (_customWidth * 0.5f - _arrowLength));
				_widthRight.SetLocalPosition(Vec3.Side * (_customWidth * -0.5f + _arrowLength));
			}
		}
	}

	public virtual bool IsPositionOnValidGround(WorldPosition worldPosition)
	{
		return Mission.Current.IsFormationUnitPositionAvailable(ref worldPosition, Mission.Current.PlayerTeam);
	}

	public static bool IsOrderPositionValid(WorldPosition orderPosition)
	{
		return Mission.Current.IsOrderPositionAvailable(ref orderPosition, Mission.Current.PlayerTeam);
	}

	private WeakGameEntity GetCollidedEntity(out Vec3 closestPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Vec2 screenPoint = (Vec2)(((int)_mission.Mode == 6) ? Input.MousePositionRanged : new Vec2(0.5f, 0.5f));
		_missionScreen.ScreenPointToWorldRay(screenPoint, out var rayBegin, out var rayEnd);
		Vec3 val = rayEnd - rayBegin;
		Vec3 val2 = ((Vec3)(ref val)).NormalizedCopy();
		rayEnd = rayBegin + val2 * 10000f;
		rayBegin = Agent.Main.GetEyeGlobalPosition();
		float num = default(float);
		WeakGameEntity parent = default(WeakGameEntity);
		_mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, ref num, ref closestPoint, ref parent, 0.3f, (BodyFlags)67188481);
		IOrderable val3;
		while (((WeakGameEntity)(ref parent)).IsValid && !((WeakGameEntity)(ref parent)).GetScriptComponents().Any((ScriptComponentBehavior sc) => (val3 = (IOrderable)(object)((sc is IOrderable) ? sc : null)) != null && (int)val3.GetOrder(Mission.Current.PlayerTeam.Side) > 0))
		{
			parent = ((WeakGameEntity)(ref parent)).Parent;
		}
		return parent;
	}

	public void SetWidthVisibility(bool isVisible, float width)
	{
		_isWidthVisible = isVisible;
		_customWidth = width;
	}
}
