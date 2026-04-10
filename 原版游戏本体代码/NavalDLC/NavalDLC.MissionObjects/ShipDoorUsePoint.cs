using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace NavalDLC.MissionObjects;

public class ShipDoorUsePoint : UsableMissionObject
{
	private const string ShipDoorHighlightTag = "ship_door_highlight";

	private GameEntity _highlight;

	private bool _isEnabled;

	[EditableScriptComponentVariable(true, "ActionStringId")]
	private string _actionStringId;

	[EditableScriptComponentVariable(true, "DescriptionStringId")]
	private string _descriptionStringId;

	public ShipDoorUsePoint()
		: base(false)
	{
		_actionStringId = string.Empty;
		_descriptionStringId = string.Empty;
	}

	protected override void OnInit()
	{
		((UsableMissionObject)this).OnInit();
		_isEnabled = false;
		base.ActionMessage = GameTexts.FindText(string.IsNullOrEmpty(_actionStringId) ? "str_open_ship_door" : _actionStringId, (string)null);
		base.ActionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13), 1f));
		base.DescriptionMessage = GameTexts.FindText(string.IsNullOrEmpty(_descriptionStringId) ? "str_ui_door" : _descriptionStringId, (string)null);
	}

	public override TextObject GetDescriptionText(WeakGameEntity gameEntity)
	{
		return base.DescriptionMessage;
	}

	public override void OnUse(Agent userAgent, sbyte agentBoneIndex)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		((UsableMissionObject)this).OnUse(userAgent, agentBoneIndex);
		if (userAgent.IsMainAgent)
		{
			Vec3 position = userAgent.Position;
			SoundManager.StartOneShotEvent("event:/mission/movement/foley/door_open", ref position);
			userAgent.StopUsingGameObject(true, (StopUsingGameObjectFlags)1);
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

	public override bool IsDisabledForAgent(Agent agent)
	{
		if (!_isEnabled)
		{
			return !agent.IsMainAgent;
		}
		return false;
	}

	public override bool IsUsableByAgent(Agent userAgent)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (_isEnabled && userAgent.IsMainAgent)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			Vec3 globalPosition = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
			return ((Vec3)(ref globalPosition)).Distance(Agent.Main.Position) <= 2f;
		}
		return false;
	}

	public void SetShipDoorUsePointEnabled(bool isEnabled)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (_isEnabled == isEnabled && !(_highlight == (GameEntity)null))
		{
			return;
		}
		_isEnabled = isEnabled;
		if (_highlight == (GameEntity)null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref gameEntity)).GetChildren())
			{
				WeakGameEntity current = child;
				if (((WeakGameEntity)(ref current)).HasTag("ship_door_highlight"))
				{
					_highlight = GameEntity.CreateFromWeakEntity(current);
				}
			}
		}
		GameEntity highlight = _highlight;
		if (highlight != null)
		{
			highlight.SetVisibilityExcludeParents(false);
		}
	}
}
