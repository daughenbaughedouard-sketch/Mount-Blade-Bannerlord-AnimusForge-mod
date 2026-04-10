using System;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class DrivenPropertyOnSpawnEffect : MPOnSpawnPerkEffect
{
	protected static string StringType = "DrivenPropertyOnSpawn";

	private DrivenProperty _drivenProperty;

	private float _value;

	private bool _isRatio;

	protected DrivenPropertyOnSpawnEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPOnSpawnPerkEffectBase)this).Deserialize(node);
		_isRatio = (node?.Attributes?["is_ratio"]?.Value)?.ToLower() == "true";
		if (!Enum.TryParse<DrivenProperty>(node?.Attributes?["driven_property"]?.Value, ignoreCase: true, out _drivenProperty))
		{
			Debug.FailedAssert("provided 'driven_property' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\DrivenPropertyOnSpawnEffect.cs", "Deserialize", 26);
		}
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !float.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\DrivenPropertyOnSpawnEffect.cs", "Deserialize", 33);
		}
	}

	public override float GetDrivenPropertyBonusOnSpawn(bool isPlayer, DrivenProperty drivenProperty, float baseValue)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if (drivenProperty == _drivenProperty && ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 2 || (isPlayer ? ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 0) : ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 1))))
		{
			if (!_isRatio)
			{
				return _value;
			}
			return baseValue * _value;
		}
		return 0f;
	}
}
