using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class ThrowingWeaponDamageEffect : MPPerkEffect
{
	protected static string StringType = "ThrowingWeaponDamage";

	private float _value;

	protected ThrowingWeaponDamageEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPPerkEffectBase)this).IsDisabledInWarmup = (node?.Attributes?["is_disabled_in_warmup"]?.Value)?.ToLower() == "true";
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !float.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\ThrowingWeaponDamageEffect.cs", "Deserialize", 24);
		}
	}

	public override float GetDamage(WeaponComponentData attackerWeapon, DamageTypes damageType, bool isAlternativeAttack)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if (isAlternativeAttack || attackerWeapon == null || (int)WeaponComponentData.GetItemTypeFromWeaponClass(attackerWeapon.WeaponClass) != 12)
		{
			return 0f;
		}
		return _value;
	}
}
