using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class MapColorGradeManager : ScriptComponentBehavior
{
	private class ColorGradeBlendRecord
	{
		public string color1;

		public string color2;

		public float alpha;

		public ColorGradeBlendRecord()
		{
			color1 = "";
			color2 = "";
			alpha = 0f;
		}

		public ColorGradeBlendRecord(ColorGradeBlendRecord other)
		{
			color1 = other.color1;
			color2 = other.color2;
			alpha = other.alpha;
		}
	}

	public bool ColorGradeEnabled;

	public bool AtmosphereSimulationEnabled;

	public float TimeOfDay;

	public float SeasonTimeFactor;

	private string colorGradeGridName = "worldmap_colorgrade_grid";

	private const int colorGradeGridSize = 262144;

	private byte[] colorGradeGrid = new byte[262144];

	private Dictionary<byte, string> colorGradeGridMapping = new Dictionary<byte, string>();

	private ColorGradeBlendRecord primaryTransitionRecord;

	private ColorGradeBlendRecord secondaryTransitionRecord;

	private byte lastColorGrade;

	private Vec2 terrainSize = new Vec2(1f, 1f);

	private string defaultColorGradeTextureName = "worldmap_colorgrade_stratosphere";

	private const float transitionSpeedFactor = 1f;

	private float lastSceneTimeOfDay;

	private void Init()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (((ScriptComponentBehavior)this).Scene.ContainsTerrain)
		{
			Vec2i val = default(Vec2i);
			float num = default(float);
			int num2 = default(int);
			int num3 = default(int);
			((ScriptComponentBehavior)this).Scene.GetTerrainData(ref val, ref num, ref num2, ref num3);
			terrainSize.x = (float)val.X * num;
			terrainSize.y = (float)val.Y * num;
		}
		colorGradeGridMapping.Add(1, defaultColorGradeTextureName);
		colorGradeGridMapping.Add(2, "worldmap_colorgrade_night");
		ReadColorGradesXml();
		MBMapScene.GetColorGradeGridData(((ScriptComponentBehavior)this).Scene, colorGradeGrid, colorGradeGridName);
	}

	protected override void OnInit()
	{
		((ScriptComponentBehavior)this).OnInit();
		Init();
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
		Init();
		TimeOfDay = ((ScriptComponentBehavior)this).Scene.TimeOfDay;
		lastSceneTimeOfDay = TimeOfDay;
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)2;
	}

	protected override void OnTick(float dt)
	{
		TimeOfDay = ((ScriptComponentBehavior)this).Scene.TimeOfDay;
		SeasonTimeFactor = MBMapScene.GetSeasonTimeFactor(((ScriptComponentBehavior)this).Scene);
		ApplyAtmosphere(forceLoadTextures: false);
		ApplyColorGrade(dt);
	}

	protected override void OnEditorTick(float dt)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (((ScriptComponentBehavior)this).Scene.TimeOfDay != lastSceneTimeOfDay)
		{
			TimeOfDay = ((ScriptComponentBehavior)this).Scene.TimeOfDay;
			lastSceneTimeOfDay = TimeOfDay;
		}
		if (((ScriptComponentBehavior)this).Scene.ContainsTerrain)
		{
			Vec2i val = default(Vec2i);
			float num = default(float);
			int num2 = default(int);
			int num3 = default(int);
			((ScriptComponentBehavior)this).Scene.GetTerrainData(ref val, ref num, ref num2, ref num3);
			terrainSize.x = (float)val.X * num;
			terrainSize.y = (float)val.Y * num;
		}
		else
		{
			terrainSize.x = 1f;
			terrainSize.y = 1f;
		}
		if (AtmosphereSimulationEnabled)
		{
			TimeOfDay += dt;
			if (TimeOfDay >= 24f)
			{
				TimeOfDay -= 24f;
			}
			ApplyAtmosphere(forceLoadTextures: false);
		}
		if (ColorGradeEnabled)
		{
			ApplyColorGrade(dt);
		}
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		switch (variableName)
		{
		case "ColorGradeEnabled":
			if (!ColorGradeEnabled)
			{
				((ScriptComponentBehavior)this).Scene.SetColorGradeBlend("", "", -1f);
				lastColorGrade = 0;
			}
			break;
		case "TimeOfDay":
			ApplyAtmosphere(forceLoadTextures: false);
			break;
		case "SeasonTimeFactor":
			ApplyAtmosphere(forceLoadTextures: false);
			break;
		}
	}

	private void ReadColorGradesXml()
	{
		List<string> list = default(List<string>);
		XmlDocument mergedXmlForNative = MBObjectManager.GetMergedXmlForNative("soln_worldmap_color_grades", ref list);
		if (mergedXmlForNative == null)
		{
			return;
		}
		XmlNode xmlNode = mergedXmlForNative.SelectSingleNode("worldmap_color_grades");
		if (xmlNode == null)
		{
			return;
		}
		XmlNode xmlNode2 = xmlNode.SelectSingleNode("color_grade_grid");
		if (xmlNode2 != null && xmlNode2.Attributes["name"] != null)
		{
			colorGradeGridName = xmlNode2.Attributes["name"].Value;
		}
		XmlNode xmlNode3 = xmlNode.SelectSingleNode("color_grade_default");
		if (xmlNode3 != null && xmlNode3.Attributes["name"] != null)
		{
			defaultColorGradeTextureName = xmlNode3.Attributes["name"].Value;
			colorGradeGridMapping[1] = defaultColorGradeTextureName;
		}
		XmlNode xmlNode4 = xmlNode.SelectSingleNode("color_grade_night");
		if (xmlNode4 != null && xmlNode4.Attributes["name"] != null)
		{
			colorGradeGridMapping[2] = xmlNode4.Attributes["name"].Value;
		}
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("color_grade");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			if (item.Attributes["name"] != null && item.Attributes["value"] != null && byte.TryParse(item.Attributes["value"].Value, out var result))
			{
				colorGradeGridMapping[result] = item.Attributes["name"].Value;
			}
		}
	}

	public void ApplyAtmosphere(bool forceLoadTextures)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		TimeOfDay = MBMath.ClampFloat(TimeOfDay, 0f, 23.99f);
		SeasonTimeFactor = MBMath.ClampFloat(SeasonTimeFactor, 0f, 1f);
		MBMapScene.SetFrameForAtmosphere(((ScriptComponentBehavior)this).Scene, TimeOfDay * 10f, ((ScriptComponentBehavior)this).Scene.LastFinalRenderCameraFrame.origin.z, forceLoadTextures);
		float num = 0.55f;
		float num2 = -0.1f;
		float seasonTimeFactor = SeasonTimeFactor;
		Vec3 val = default(Vec3);
		((Vec3)(ref val))._002Ector(0f, 0.65f, 0f, -1f);
		val.x = MBMath.Lerp(num, num2, seasonTimeFactor, 1E-05f);
		MBMapScene.SetTerrainDynamicParams(((ScriptComponentBehavior)this).Scene, val);
	}

	public void ApplyColorGrade(float dt)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Vec3 origin = ((ScriptComponentBehavior)this).Scene.LastFinalRenderCameraFrame.origin;
		float num = 1f;
		int num2 = MathF.Floor(origin.x / ((Vec2)(ref terrainSize)).X * 512f);
		int num3 = MathF.Floor(origin.y / ((Vec2)(ref terrainSize)).Y * 512f);
		num2 = MBMath.ClampIndex(num2, 0, 512);
		num3 = MBMath.ClampIndex(num3, 0, 512);
		byte b = colorGradeGrid[num3 * 512 + num2];
		if (origin.z > 400f)
		{
			b = 1;
		}
		if (TimeOfDay > 22f || TimeOfDay < 2f)
		{
			b = 2;
		}
		if (MBMapScene.GetApplyRainColorGrade() && origin.z < 50f)
		{
			b = 160;
			num = 0.2f;
		}
		if (lastColorGrade != b)
		{
			string value = "";
			string value2 = "";
			if (!colorGradeGridMapping.TryGetValue(lastColorGrade, out value))
			{
				value = defaultColorGradeTextureName;
			}
			if (!colorGradeGridMapping.TryGetValue(b, out value2))
			{
				value2 = defaultColorGradeTextureName;
			}
			if (primaryTransitionRecord == null)
			{
				primaryTransitionRecord = new ColorGradeBlendRecord
				{
					color1 = value,
					color2 = value2,
					alpha = 0f
				};
			}
			else
			{
				secondaryTransitionRecord = new ColorGradeBlendRecord
				{
					color1 = primaryTransitionRecord.color2,
					color2 = value2,
					alpha = 0f
				};
			}
			lastColorGrade = b;
		}
		if (primaryTransitionRecord == null)
		{
			return;
		}
		if (primaryTransitionRecord.alpha < 1f)
		{
			primaryTransitionRecord.alpha = MathF.Min(primaryTransitionRecord.alpha + dt * (1f / num), 1f);
			((ScriptComponentBehavior)this).Scene.SetColorGradeBlend(primaryTransitionRecord.color1, primaryTransitionRecord.color2, primaryTransitionRecord.alpha);
			return;
		}
		primaryTransitionRecord = null;
		if (secondaryTransitionRecord != null)
		{
			primaryTransitionRecord = new ColorGradeBlendRecord(secondaryTransitionRecord);
			secondaryTransitionRecord = null;
		}
	}
}
