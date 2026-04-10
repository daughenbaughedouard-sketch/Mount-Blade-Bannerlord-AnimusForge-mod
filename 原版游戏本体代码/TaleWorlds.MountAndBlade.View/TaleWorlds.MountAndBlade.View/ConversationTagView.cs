using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View;

public class ConversationTagView
{
	public static string GetSkillMeshName(SkillObject skillEnum, bool isOn = false)
	{
		if (isOn)
		{
			return "skill_icon_" + ((MBObjectBase)skillEnum).StringId.ToLower() + "_on";
		}
		return "skill_icon_" + ((MBObjectBase)skillEnum).StringId.ToLower() + "_off";
	}
}
