using System.Xml;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class HitpointsEffect : MPOnSpawnPerkEffect
{
	protected static string StringType = "HitpointsOnSpawn";

	private float _value;

	protected HitpointsEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPOnSpawnPerkEffectBase)this).Deserialize(node);
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !float.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\HitpointsEffect.cs", "Deserialize", 20);
		}
	}

	public override float GetHitpoints(bool isPlayer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 2 || (isPlayer ? ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 0) : ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 1)))
		{
			return _value;
		}
		return 0f;
	}
}
