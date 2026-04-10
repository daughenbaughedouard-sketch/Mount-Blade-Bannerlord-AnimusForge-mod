using NavalDLC.Map;
using SandBox;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace NavalDLC.View.Map.Visuals;

public class StormVisual : MapEntityVisual<Storm>
{
	private enum StormVisualState
	{
		VisualNotInitialized,
		Developing,
		Active,
		Finalizing,
		ReadyToBeReleased
	}

	public const int DefaultStormVisualHeight = 0;

	private StormVisualState _visualState;

	private SoundEvent _stormSoundEvent;

	public GameEntity VisualEntity;

	private Scene _mapScene;

	public override CampaignVec2 InteractionPositionForPlayer => new CampaignVec2(base.MapEntity.CurrentPosition, true);

	public override MapEntityVisual AttachedTo => null;

	public bool IsReadyToBeReleased => base.MapEntity.IsReadyToBeFinalized;

	public StormVisual(Storm storm)
		: base(storm)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_mapScene = ((MapScene)Campaign.Current.MapSceneWrapper).Scene;
		_visualState = StormVisualState.VisualNotInitialized;
		_stormSoundEvent = SoundManager.CreateEvent("event:/map/ambient/node/hurricane", _mapScene);
		SoundEvent stormSoundEvent = _stormSoundEvent;
		Vec2 currentPosition = storm.CurrentPosition;
		stormSoundEvent.SetPosition(((Vec2)(ref currentPosition)).ToVec3(0f));
		_stormSoundEvent.SetParameter("StormIntensity", (float)storm.StormType);
	}

	public override bool OnMapClick(bool followModifierUsed)
	{
		return false;
	}

	public override void OnHover()
	{
	}

	public override void OnOpenEncyclopedia()
	{
	}

	public override bool IsVisibleOrFadingOut()
	{
		return base.MapEntity.IsActive;
	}

	public override Vec3 GetVisualPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 interactionPositionForPlayer = ((MapEntityVisual)this).InteractionPositionForPlayer;
		return ((CampaignVec2)(ref interactionPositionForPlayer)).AsVec3();
	}

	public void Tick()
	{
		StormVisualState stormVisualState = GetStormVisualState(base.MapEntity);
		if (_visualState != stormVisualState)
		{
			UpdateVisualState(stormVisualState);
		}
		if (VisualEntity != (GameEntity)null)
		{
			VisualTick();
		}
		base.MapEntity.OnVisualUpdated();
	}

	private void VisualTick()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vec3 localPosition = default(Vec3);
		((Vec3)(ref localPosition))._002Ector(base.MapEntity.CurrentPosition, 0f, -1f);
		VisualEntity.SetLocalPosition(localPosition);
		_stormSoundEvent.SetPosition(VisualEntity.GlobalPosition);
	}

	private void UpdateVisualState(StormVisualState newState)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (VisualEntity != (GameEntity)null)
		{
			_mapScene.RemoveEntity(VisualEntity, 0);
			VisualEntity = null;
		}
		_visualState = newState;
		switch (newState)
		{
		case StormVisualState.Developing:
			if (NavalDLCManager.Instance.StormManager.DebugVisualsEnabled)
			{
				VisualEntity = GameEntity.Instantiate(_mapScene, "editor_cube", MatrixFrame.Identity, true, "");
			}
			break;
		case StormVisualState.Active:
			_stormSoundEvent.Play();
			switch (base.MapEntity.StormType)
			{
			case Storm.StormTypes.Storm:
				VisualEntity = GameEntity.Instantiate(_mapScene, "psys_mapicon_lightclouds", MatrixFrame.Identity, true, "");
				break;
			case Storm.StormTypes.ThunderStorm:
				VisualEntity = GameEntity.Instantiate(_mapScene, "psys_mapicon_darkclouds", MatrixFrame.Identity, true, "");
				break;
			case Storm.StormTypes.Hurricane:
				VisualEntity = GameEntity.Instantiate(_mapScene, "psys_mapicon_typhoon", MatrixFrame.Identity, true, "");
				break;
			}
			_visualState = StormVisualState.Active;
			break;
		case StormVisualState.Finalizing:
			_stormSoundEvent.Stop();
			if (NavalDLCManager.Instance.StormManager.DebugVisualsEnabled)
			{
				VisualEntity = GameEntity.Instantiate(_mapScene, "editor_cube", MatrixFrame.Identity, true, "");
			}
			break;
		}
	}

	private StormVisualState GetStormVisualState(Storm storm)
	{
		if (storm.IsReadyToBeFinalized)
		{
			return StormVisualState.ReadyToBeReleased;
		}
		if (storm.IsActive)
		{
			return StormVisualState.Active;
		}
		if (storm.IsInDevelopingState)
		{
			return StormVisualState.Developing;
		}
		if (storm.IsInFinalizingState)
		{
			return StormVisualState.Finalizing;
		}
		return StormVisualState.VisualNotInitialized;
	}
}
