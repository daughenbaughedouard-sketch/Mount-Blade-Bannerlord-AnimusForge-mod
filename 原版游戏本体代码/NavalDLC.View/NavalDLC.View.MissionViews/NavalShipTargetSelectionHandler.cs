using System;
using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.View.MissionViews;

public class NavalShipTargetSelectionHandler : MissionView
{
	public const float MaxDistanceForFocusCheck = 1000f;

	public const float MinDistanceForFocusCheck = 10f;

	public readonly float MaxDistanceToCenterForFocus = 70f * (Screen.RealScreenResolutionHeight / 1080f);

	private readonly List<(MissionShip, float)> _distanceCache = new List<(MissionShip, float)>();

	private readonly MBList<MissionShip> _focusedShipsCache = new MBList<MissionShip>();

	private readonly MBList<MissionShip> _enemyShipsCache = new MBList<MissionShip>();

	private Vec2 _centerOfScreen = new Vec2(Screen.RealScreenResolutionWidth / 2f, Screen.RealScreenResolutionHeight / 2f);

	private bool _isTargetingDisabled;

	private Camera ActiveCamera => ((MissionView)this).MissionScreen.CustomCamera ?? ((MissionView)this).MissionScreen.CombatCamera;

	public event Action<MBReadOnlyList<MissionShip>> OnShipsFocused;

	public override void OnPreDisplayMissionTick(float dt)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnPreDisplayMissionTick(dt);
		_distanceCache.Clear();
		((List<MissionShip>)(object)_focusedShipsCache).Clear();
		((List<MissionShip>)(object)_enemyShipsCache).Clear();
		NavalShipsLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
		if (missionBehavior == null)
		{
			return;
		}
		if (!_isTargetingDisabled)
		{
			missionBehavior.FillTeamShips((TeamSideEnum)2, _enemyShipsCache);
			Vec3 position = ActiveCamera.Position;
			_centerOfScreen.x = Screen.RealScreenResolutionWidth / 2f;
			_centerOfScreen.y = Screen.RealScreenResolutionHeight / 2f;
			for (int i = 0; i < ((List<MissionShip>)(object)_enemyShipsCache).Count; i++)
			{
				MissionShip missionShip = ((List<MissionShip>)(object)_enemyShipsCache)[i];
				float shipDistanceToCenter = GetShipDistanceToCenter(missionShip, position);
				_distanceCache.Add((missionShip, shipDistanceToCenter));
			}
		}
		if (_distanceCache.Count == 0)
		{
			this.OnShipsFocused?.Invoke(null);
			return;
		}
		MissionShip missionShip2 = null;
		float num = MaxDistanceToCenterForFocus;
		for (int j = 0; j < _distanceCache.Count; j++)
		{
			(MissionShip, float) tuple = _distanceCache[j];
			if (tuple.Item2 == 0f)
			{
				((List<MissionShip>)(object)_focusedShipsCache).Add(tuple.Item1);
			}
			else if (tuple.Item2 < num)
			{
				num = tuple.Item2;
				(missionShip2, _) = tuple;
			}
		}
		if (missionShip2 != null)
		{
			((List<MissionShip>)(object)_focusedShipsCache).Add(missionShip2);
		}
		this.OnShipsFocused?.Invoke((MBReadOnlyList<MissionShip>)(object)_focusedShipsCache);
	}

	private float GetShipDistanceToCenter(MissionShip ship, Vec3 cameraPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = ship.GlobalFrame.origin;
		Vec2 val = ((Vec3)(ref origin)).AsVec2;
		float num = ((Vec2)(ref val)).Distance(((Vec3)(ref cameraPosition)).AsVec2);
		if (num >= 1000f)
		{
			return 2.1474836E+09f;
		}
		if (num <= 10f)
		{
			return 0f;
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(ActiveCamera, origin + Vec3.Up * 3f, ref num2, ref num3, ref num4);
		if (num4 <= 0f)
		{
			return 2.1474836E+09f;
		}
		val = new Vec2(num2, num3);
		return ((Vec2)(ref val)).Distance(_centerOfScreen);
	}

	public void SetIsFormationTargetingDisabled(bool isDisabled)
	{
		if (_isTargetingDisabled != isDisabled)
		{
			_isTargetingDisabled = isDisabled;
			if (isDisabled)
			{
				_distanceCache.Clear();
				((List<MissionShip>)(object)_enemyShipsCache).Clear();
				((List<MissionShip>)(object)_focusedShipsCache).Clear();
				this.OnShipsFocused?.Invoke(null);
			}
		}
	}

	public override void OnRemoveBehavior()
	{
		_distanceCache.Clear();
		((List<MissionShip>)(object)_focusedShipsCache).Clear();
		this.OnShipsFocused = null;
		((MissionView)this).OnRemoveBehavior();
	}
}
