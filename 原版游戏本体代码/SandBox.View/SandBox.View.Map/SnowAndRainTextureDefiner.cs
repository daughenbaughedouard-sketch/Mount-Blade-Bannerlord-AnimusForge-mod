using TaleWorlds.CampaignSystem;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;

namespace SandBox.View.Map;

public class SnowAndRainTextureDefiner : ScriptComponentBehavior
{
	[EditorVisibleScriptComponentVariable(true)]
	public Texture SnowAndRainTexture;

	[EditorVisibleScriptComponentVariable(true)]
	public int WeatherNodeGridWidthAndHeight;

	protected override void OnInit()
	{
		SetDataToScene();
	}

	protected override void OnTerrainReload(int step)
	{
		if (step == 1)
		{
			SetDataToScene();
		}
	}

	protected override void OnEditorInit()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (((WeakGameEntity)(ref gameEntity)).Scene.ContainsTerrain)
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			((WeakGameEntity)(ref gameEntity)).Scene.SetDynamicSnowTexture(SnowAndRainTexture);
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (variableName == "SnowAndRainTexture")
		{
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (((WeakGameEntity)(ref gameEntity)).Scene.ContainsTerrain)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).Scene.SetDynamicSnowTexture(SnowAndRainTexture);
			}
		}
	}

	private void SetDataToScene()
	{
		if ((NativeObject)(object)SnowAndRainTexture != (NativeObject)null)
		{
			((MapScene)(object)Campaign.Current.MapSceneWrapper).SetSnowAndRainDataWithDimension(SnowAndRainTexture, WeatherNodeGridWidthAndHeight);
		}
	}
}
