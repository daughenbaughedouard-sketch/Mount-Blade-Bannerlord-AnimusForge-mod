using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class PopupSceneSwitchCameraSequence : PopupSceneSequence
{
	public string EntityName = "";

	private GameEntity _switchEntity;

	protected override void OnInit()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_switchEntity = ((WeakGameEntity)(ref gameEntity)).Scene.GetFirstEntityWithName(EntityName);
	}

	public override void OnInitialState()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (_switchEntity != (GameEntity)null)
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			GameEntity obj = ((WeakGameEntity)(ref gameEntity)).Scene.FindEntityWithTag("customcamera");
			if (obj != null)
			{
				obj.RemoveTag("customcamera");
			}
			_switchEntity.AddTag("customcamera");
		}
	}

	public override void OnPositiveState()
	{
	}

	public override void OnNegativeState()
	{
	}
}
