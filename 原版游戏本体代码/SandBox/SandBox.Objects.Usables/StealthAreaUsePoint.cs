using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SandBox.Objects.Usables;

public class StealthAreaUsePoint : UsableMissionObject
{
	private const string HighlightEntityName = "highlight_pointer_glow_ground";

	private bool _isEnabled = true;

	private bool _isAlreadyUsed;

	private WeakGameEntity _highlightGameEntity;

	public string ActionStringId;

	public string DescriptionStringId;

	public StealthAreaUsePoint()
		: base(false)
	{
	}

	protected override void OnInit()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		((UsableMissionObject)this).OnInit();
		_isAlreadyUsed = false;
		base.ActionMessage = GameTexts.FindText(string.IsNullOrEmpty(ActionStringId) ? "str_call_troops" : ActionStringId, (string)null);
		base.ActionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		base.DescriptionMessage = (string.IsNullOrEmpty(DescriptionStringId) ? TextObject.GetEmpty() : GameTexts.FindText(DescriptionStringId, (string)null));
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
		{
			WeakGameEntity current = child;
			foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref current)).GetChildren())
			{
				WeakGameEntity current2 = child2;
				if (((WeakGameEntity)(ref current2)).Name.Equals("highlight_pointer_glow_ground"))
				{
					_highlightGameEntity = current2;
					break;
				}
			}
			if (_highlightGameEntity != (GameEntity)null)
			{
				break;
			}
		}
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		return base.DescriptionMessage;
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((UsableMissionObject)this).OnUse(userAgent, agentBoneIndex);
		if (!IsInCombat())
		{
			if (userAgent.IsMainAgent)
			{
				Vec3 position = userAgent.Position;
				SoundManager.StartOneShotEvent("event:/mission/combat/pickup_arrows", ref position);
				_isAlreadyUsed = true;
				((WeakGameEntity)(ref _highlightGameEntity)).SetVisibilityExcludeParents(false);
				userAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
			}
			DisableAgentAIs();
		}
	}

	public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
	{
		((UsableMissionObject)this).OnUseStopped(userAgent, isSuccessful, preferenceIndex);
		if (((UsableMissionObject)this).LockUserFrames || ((UsableMissionObject)this).LockUserPositions)
		{
			userAgent.ClearTargetFrame();
		}
	}

	public void DisableAgentAIs()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		WorldPosition val = default(WorldPosition);
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item.IsActive() && item.IsAIControlled)
			{
				item.SetIsAIPaused(true);
				((WorldPosition)(ref val))._002Ector(Mission.Current.Scene, item.Position);
				item.SetScriptedPosition(ref val, false, (AIScriptedFrameFlags)0);
			}
		}
	}

	public override bool IsDisabledForAgent(Agent agent)
	{
		if (!agent.IsMainAgent)
		{
			if (!_isAlreadyUsed)
			{
				return !_isEnabled;
			}
			return true;
		}
		return false;
	}

	public override bool IsUsableByAgent(Agent userAgent)
	{
		if (userAgent.IsMainAgent && !_isAlreadyUsed && _isEnabled)
		{
			return !IsInCombat();
		}
		return false;
	}

	private bool IsInCombat()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		foreach (Agent item in (List<Agent>)(object)Mission.Current.AllAgents)
		{
			if (item.IsActive())
			{
				AIStateFlag val = (AIStateFlag)3;
				if ((AIStateFlag)(item.AIStateFlags & val) == val)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public void EnableStealthAreaUsePoint()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!_isEnabled)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			SoundManager.StartOneShotEvent("event:/ui/notification/quest_update", ref globalPosition);
		}
		((WeakGameEntity)(ref _highlightGameEntity)).SetVisibilityExcludeParents(true);
		_isEnabled = true;
	}

	public void DisableStealthAreaUsePoint()
	{
		_isEnabled = false;
		((WeakGameEntity)(ref _highlightGameEntity)).SetVisibilityExcludeParents(false);
	}
}
