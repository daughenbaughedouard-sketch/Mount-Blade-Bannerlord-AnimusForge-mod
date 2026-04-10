using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.Objects;

public class GenericMissionEventBox : VolumeBox
{
	public string ActivatorAgentTags;

	private List<GenericMissionEventScript> _genericMissionEvents = new List<GenericMissionEventScript>();

	protected override void OnInit()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		((VolumeBox)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick((TickRequirement)2);
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (ScriptComponentBehavior scriptComponent in ((WeakGameEntity)(ref gameEntity)).GetScriptComponents())
		{
			GenericMissionEventScript item;
			if ((item = (GenericMissionEventScript)(object)((scriptComponent is GenericMissionEventScript) ? scriptComponent : null)) != null)
			{
				_genericMissionEvents.Add(item);
			}
		}
	}

	protected override void OnTick(float dt)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		bool flag = true;
		foreach (GenericMissionEventScript genericMissionEvent in _genericMissionEvents)
		{
			if (!genericMissionEvent.IsDisabled)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		bool flag2 = false;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item.AgentVisuals.IsValid() && item.AgentVisuals.GetEntity().Tags.Any((string x) => !string.IsNullOrEmpty(x) && ActivatorAgentTags.Contains(x)) && ((VolumeBox)this).IsPointIn(item.Position))
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			return;
		}
		foreach (GenericMissionEventScript genericMissionEvent2 in _genericMissionEvents)
		{
			if (!genericMissionEvent2.IsDisabled)
			{
				Game.Current.EventManager.TriggerEvent<GenericMissionEvent>(new GenericMissionEvent(genericMissionEvent2.EventId, genericMissionEvent2.Parameter));
			}
		}
	}
}
