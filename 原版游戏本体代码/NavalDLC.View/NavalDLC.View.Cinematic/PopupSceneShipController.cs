using NavalDLC.Missions.NavalPhysics;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.View.Cinematic;

public class PopupSceneShipController : ScriptComponentBehavior
{
	[EditableScriptComponentVariable(true, "")]
	private Vec3 _continousForce = Vec3.Zero;

	[EditableScriptComponentVariable(true, "")]
	private bool _isAnchored;

	[EditableScriptComponentVariable(true, "")]
	private string _targetShipEntityTag = string.Empty;

	private GameEntity _targetShipEntity;

	private MatrixFrame _initialShipFrame;

	private bool _isApplyingForce;

	public SimpleButton StartApplyingForce;

	public SimpleButton StopApplyingForce;

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)16;
	}

	public PopupSceneShipController()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		StartApplyingForce = new SimpleButton();
		StopApplyingForce = new SimpleButton();
	}

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		_isApplyingForce = true;
		_targetShipEntity = ((ScriptComponentBehavior)this).Scene.FindEntityWithTag(_targetShipEntityTag);
	}

	protected override void OnFixedTick(float fixedDt)
	{
		ApplyForce(fixedDt);
	}

	protected override void OnEditorTick(float dt)
	{
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		ApplyForce(0.016f);
	}

	protected override void OnParallelFixedTick(float fixedDt)
	{
		((ScriptComponentBehavior)this).OnParallelFixedTick(fixedDt);
		ApplyForce(fixedDt);
	}

	private void ApplyForce(float dt)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		GameEntity targetShipEntity = _targetShipEntity;
		if ((NativeObject)(object)((targetShipEntity != null) ? targetShipEntity.Scene : null) != (NativeObject)(object)((ScriptComponentBehavior)this).Scene)
		{
			_targetShipEntity = ((ScriptComponentBehavior)this).Scene.FindEntityWithTag(_targetShipEntityTag);
		}
		GameEntity targetShipEntity2 = _targetShipEntity;
		if ((NativeObject)(object)((targetShipEntity2 != null) ? targetShipEntity2.Scene : null) != (NativeObject)(object)((ScriptComponentBehavior)this).Scene)
		{
			return;
		}
		NavalPhysics firstScriptOfType = _targetShipEntity.GetFirstScriptOfType<NavalPhysics>();
		if (_isAnchored)
		{
			if (firstScriptOfType != null)
			{
				firstScriptOfType.SetAnchorFrame(in Vec2.Zero, in Vec2.Forward);
				firstScriptOfType.SetAnchor(isAnchored: true);
			}
		}
		else if (_isApplyingForce)
		{
			Vec3 val = _continousForce * _targetShipEntity.Mass * dt;
			GameEntity targetShipEntity3 = _targetShipEntity;
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntityPhysicsExtensions.ApplyLocalForceAtLocalPosToDynamicBody(targetShipEntity3, ((WeakGameEntity)(ref gameEntity)).CenterOfMass, val, (ForceMode)0);
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		if (variableName == "StartApplyingForce")
		{
			if (!_isApplyingForce)
			{
				_targetShipEntity = ((ScriptComponentBehavior)this).Scene.FindEntityWithTag(_targetShipEntityTag);
				if (_targetShipEntity != (GameEntity)null)
				{
					_isApplyingForce = true;
					_initialShipFrame = _targetShipEntity.GetGlobalFrame();
				}
			}
		}
		else if (variableName == "StopApplyingForce" && _isApplyingForce)
		{
			_targetShipEntity = ((ScriptComponentBehavior)this).Scene.FindEntityWithTag(_targetShipEntityTag);
			if (_targetShipEntity != (GameEntity)null)
			{
				_targetShipEntity.SetGlobalFrame(ref _initialShipFrame, true);
				GameEntityPhysicsExtensions.SetAngularVelocity(_targetShipEntity, Vec3.Zero);
				GameEntityPhysicsExtensions.SetLinearVelocity(_targetShipEntity, Vec3.Zero);
				_isApplyingForce = false;
			}
		}
	}
}
