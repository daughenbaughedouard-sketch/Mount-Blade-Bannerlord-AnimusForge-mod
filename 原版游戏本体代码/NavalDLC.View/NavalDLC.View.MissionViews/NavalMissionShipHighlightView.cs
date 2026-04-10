using System.Collections.Generic;
using NavalDLC.Missions.MissionLogics;
using NavalDLC.Missions.Objects;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace NavalDLC.View.MissionViews;

public class NavalMissionShipHighlightView : MissionView
{
	private NavalShipsLogic _navalShipsLogic;

	private Dictionary<MissionShip, (bool, uint)> _contourCache = new Dictionary<MissionShip, (bool, uint)>();

	private MissionShip _focusedShip;

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		_navalShipsLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<NavalShipsLogic>();
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionView)this).OnMissionScreenTick(dt);
		UpdateSelectedShipContours();
	}

	public override void OnMissionScreenDeactivate()
	{
		((MissionView)this).OnMissionScreenDeactivate();
		_contourCache.Clear();
	}

	public void OnShipFocused(MissionShip focusedShip)
	{
		_focusedShip = focusedShip;
	}

	private void UpdateSelectedShipContours()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected I4, but got Unknown
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity;
		if (_navalShipsLogic?.AllShips == null)
		{
			foreach (KeyValuePair<MissionShip, (bool, uint)> item in _contourCache)
			{
				MissionShip key = item.Key;
				if (key != null)
				{
					gameEntity = ((ScriptComponentBehavior)key).GameEntity;
					if (((WeakGameEntity)(ref gameEntity)).IsValid)
					{
						MissionShip key2 = item.Key;
						if (key2 != null)
						{
							gameEntity = ((ScriptComponentBehavior)key2).GameEntity;
							((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)null, false);
						}
					}
				}
			}
			return;
		}
		for (int i = 0; i < ((List<MissionShip>)(object)_navalShipsLogic.AllShips).Count; i++)
		{
			MissionShip missionShip = ((List<MissionShip>)(object)_navalShipsLogic.AllShips)[i];
			if (missionShip == null)
			{
				continue;
			}
			gameEntity = ((ScriptComponentBehavior)missionShip).GameEntity;
			if (!((WeakGameEntity)(ref gameEntity)).IsValid)
			{
				continue;
			}
			uint num = 0u;
			bool flag;
			if ((int)((MissionBehavior)this).Mission.Mode == 6 || ((MissionBehavior)this).Mission.IsOrderMenuOpen)
			{
				flag = missionShip.Formation != null && (missionShip.Captain == null || missionShip.Captain != Agent.Main) && ((List<Formation>)(object)((MissionBehavior)this).Mission.PlayerTeam.PlayerOrderController.SelectedFormations).Contains(missionShip.Formation);
				num = 4294105105u;
			}
			else
			{
				flag = _focusedShip == missionShip && ((MissionView)this).Input.IsGameKeyDown(5);
				if (_focusedShip?.Team != null)
				{
					TeamSideEnum teamSide = _focusedShip.Team.TeamSide;
					switch ((int)teamSide)
					{
					case 0:
						num = 4282512610u;
						break;
					case 1:
						num = 4282578006u;
						break;
					case 2:
						num = 4294197569u;
						break;
					}
				}
			}
			bool flag2 = false;
			if (_contourCache.TryGetValue(missionShip, out var value))
			{
				if (value.Item1 != flag || value.Item2 != num)
				{
					flag2 = true;
					_contourCache[missionShip] = (flag, num);
				}
			}
			else
			{
				flag2 = true;
				_contourCache[missionShip] = (flag, num);
			}
			if (flag2)
			{
				if (flag)
				{
					gameEntity = ((ScriptComponentBehavior)missionShip).GameEntity;
					((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)num, true);
				}
				else
				{
					gameEntity = ((ScriptComponentBehavior)missionShip).GameEntity;
					((WeakGameEntity)(ref gameEntity)).SetContourColor((uint?)null, false);
				}
			}
		}
	}
}
