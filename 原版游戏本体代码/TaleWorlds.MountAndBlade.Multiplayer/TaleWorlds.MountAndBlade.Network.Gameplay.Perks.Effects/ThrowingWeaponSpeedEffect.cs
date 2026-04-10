using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class ThrowingWeaponSpeedEffect : MPPerkEffect
{
	protected static string StringType = "ThrowingWeaponSpeed";

	private float _value;

	protected ThrowingWeaponSpeedEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPPerkEffectBase)this).IsDisabledInWarmup = (node?.Attributes?["is_disabled_in_warmup"]?.Value)?.ToLower() == "true";
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !float.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\ThrowingWeaponSpeedEffect.cs", "Deserialize", 24);
		}
	}

	public override float GetThrowingWeaponSpeed(WeaponComponentData attackerWeapon)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if (attackerWeapon == null || (int)WeaponComponentData.GetItemTypeFromWeaponClass(attackerWeapon.WeaponClass) != 12)
		{
			return 0f;
		}
		return _value;
	}
}
