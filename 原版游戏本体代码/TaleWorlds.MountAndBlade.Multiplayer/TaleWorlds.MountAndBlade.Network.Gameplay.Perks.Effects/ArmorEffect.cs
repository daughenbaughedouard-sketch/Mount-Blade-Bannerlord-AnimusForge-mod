using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class ArmorEffect : MPOnSpawnPerkEffect
{
	protected static string StringType = "ArmorOnSpawn";

	private float _value;

	protected ArmorEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPOnSpawnPerkEffectBase)this).Deserialize(node);
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !float.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\ArmorEffect.cs", "Deserialize", 21);
		}
	}

	public override float GetDrivenPropertyBonusOnSpawn(bool isPlayer, DrivenProperty drivenProperty, float baseValue)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (((int)drivenProperty == 55 || (int)drivenProperty == 56 || (int)drivenProperty == 57 || (int)drivenProperty == 58) && ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 2 || (isPlayer ? ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 0) : ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 1))))
		{
			return _value;
		}
		return 0f;
	}
}
