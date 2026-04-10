using System;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Conditions;

public class TroopRoleCondition : MPPerkCondition
{
	private enum Role
	{
		Sergeant,
		Troop,
		BannerBearer
	}

	protected static string StringType = "TroopRole";

	private Role _role;

	public override PerkEventFlags EventFlags => (PerkEventFlags)32;

	protected TroopRoleCondition()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		string text = node?.Attributes?["role"]?.Value;
		_role = Role.Sergeant;
		if (text != null && !Enum.TryParse<Role>(text, ignoreCase: true, out _role))
		{
			_role = Role.Sergeant;
			Debug.FailedAssert("provided 'role' is invalid", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer\\Perks\\Conditions\\TroopRoleCondition.cs", "Deserialize", 35);
		}
	}

	public override bool Check(MissionPeer peer)
	{
		return ((MPPerkCondition)this).Check((peer != null) ? peer.ControlledAgent : null);
	}

	public override bool Check(Agent agent)
	{
		agent = ((agent != null && agent.IsMount) ? agent.RiderAgent : agent);
		if (agent != null && MultiplayerOptionsExtensions.GetIntValue((OptionType)20, (MultiplayerOptionsAccessMode)1) > 0)
		{
			switch (_role)
			{
			case Role.Sergeant:
				return IsAgentSergeant(agent);
			case Role.BannerBearer:
				if (IsAgentBannerBearer(agent))
				{
					return !IsAgentSergeant(agent);
				}
				return false;
			case Role.Troop:
				if (!IsAgentBannerBearer(agent))
				{
					return !IsAgentSergeant(agent);
				}
				return false;
			}
		}
		return false;
	}

	private bool IsAgentSergeant(Agent agent)
	{
		return agent.Character == MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character).HeroCharacter;
	}

	private bool IsAgentBannerBearer(Agent agent)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		MissionPeer val = ((agent != null) ? agent.MissionPeer : null) ?? ((agent != null) ? agent.OwningAgentMissionPeer : null);
		Formation val2 = ((val != null) ? val.ControlledFormation : null);
		if (val2 != null)
		{
			MissionWeapon val3 = agent.Equipment[(EquipmentIndex)4];
			if (!((MissionWeapon)(ref val3)).IsEmpty && (int)((MissionWeapon)(ref val3)).Item.ItemType == 26 && new Banner(val2.BannerCode, val.Team.Color, val.Team.Color2).Serialize() == ((MissionWeapon)(ref val3)).Banner.Serialize())
			{
				return true;
			}
		}
		return false;
	}
}
