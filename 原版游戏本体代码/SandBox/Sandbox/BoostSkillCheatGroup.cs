using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace SandBox;

public class BoostSkillCheatGroup : GameplayCheatGroup
{
	public class BoostSkillCheeat : GameplayCheatItem
	{
		private readonly SkillObject _skillToBoost;

		public BoostSkillCheeat(SkillObject skillToBoost)
		{
			_skillToBoost = skillToBoost;
		}

		public override void ExecuteCheat()
		{
			int num = 50;
			if (Hero.MainHero.GetSkillValue(_skillToBoost) + num > 330)
			{
				num = 330 - Hero.MainHero.GetSkillValue(_skillToBoost);
			}
			Hero.MainHero.HeroDeveloper.ChangeSkillLevel(_skillToBoost, num, false);
		}

		public override TextObject GetName()
		{
			return ((MBObjectBase)_skillToBoost).GetName();
		}
	}

	public override IEnumerable<GameplayCheatBase> GetCheats()
	{
		foreach (SkillObject item in (List<SkillObject>)(object)Skills.All)
		{
			yield return new BoostSkillCheeat(item);
		}
	}

	public override TextObject GetName()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new TextObject("{=SFn4UFd4}Boost Skill", (Dictionary<string, object>)null);
	}
}
