using System.Xml;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class GoldRecoveryEffect : MPPerkEffect
{
	protected static string StringType = "GoldRecovery";

	private int _value;

	private int _period;

	public override bool IsTickRequired => true;

	protected GoldRecoveryEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		((MPPerkEffectBase)this).IsDisabledInWarmup = (node?.Attributes?["is_disabled_in_warmup"]?.Value)?.ToLower() == "true";
		string text = node?.Attributes?["value"]?.Value;
		if (text == null || !int.TryParse(text, out _value))
		{
			Debug.FailedAssert("provided 'value' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\GoldRecoveryEffect.cs", "Deserialize", 29);
		}
		string text2 = node?.Attributes?["period"]?.Value;
		if (text2 == null || !int.TryParse(text2, out _period) || _period < 1)
		{
			Debug.FailedAssert("provided 'period' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Effects\\GoldRecoveryEffect.cs", "Deserialize", 35);
		}
	}

	public override void OnTick(Agent agent, int tickCount)
	{
		agent = ((agent != null && agent.IsMount) ? agent.RiderAgent : agent);
		MissionPeer val = ((agent != null) ? agent.MissionPeer : null) ?? ((agent != null) ? agent.OwningAgentMissionPeer : null);
		if (tickCount % _period == 0 && val != null)
		{
			Mission current = Mission.Current;
			MissionMultiplayerGameModeBase obj = ((current != null) ? current.GetMissionBehavior<MissionMultiplayerGameModeBase>() : null);
			if (obj != null)
			{
				obj.ChangeCurrentGoldForPeer(val, val.Representative.Gold + _value);
			}
		}
	}
}
