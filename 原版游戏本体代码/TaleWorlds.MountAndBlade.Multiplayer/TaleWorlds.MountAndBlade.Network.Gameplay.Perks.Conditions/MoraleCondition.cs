using System.Xml;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Conditions;

public class MoraleCondition : MPPerkCondition<MissionMultiplayerFlagDomination>
{
	protected static string StringType = "FlagDominationMorale";

	private float _min;

	private float _max;

	public override PerkEventFlags EventFlags => (PerkEventFlags)1;

	public override bool IsPeerCondition => true;

	protected MoraleCondition()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		string text = node?.Attributes?["min"]?.Value;
		if (text == null)
		{
			_min = -1f;
		}
		else if (!float.TryParse(text, out _min))
		{
			Debug.FailedAssert("provided 'min' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Conditions\\MoraleCondition.cs", "Deserialize", 35);
		}
		string text2 = node?.Attributes?["max"]?.Value;
		if (text2 == null)
		{
			_max = 1f;
		}
		else if (!float.TryParse(text2, out _max))
		{
			Debug.FailedAssert("provided 'max' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Conditions\\MoraleCondition.cs", "Deserialize", 45);
		}
	}

	public override bool Check(MissionPeer peer)
	{
		return ((MPPerkCondition)this).Check((peer != null) ? peer.ControlledAgent : null);
	}

	public override bool Check(Agent agent)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between Unknown and I4
		agent = ((agent != null && agent.IsMount) ? agent.RiderAgent : agent);
		Team val = ((agent != null) ? agent.Team : null);
		if (val != null)
		{
			MissionMultiplayerFlagDomination gameModeInstance = base.GameModeInstance;
			float num = (((int)val.Side == 1) ? gameModeInstance.MoraleRounded : (0f - gameModeInstance.MoraleRounded));
			if (num >= _min)
			{
				return num <= _max;
			}
			return false;
		}
		return false;
	}
}
