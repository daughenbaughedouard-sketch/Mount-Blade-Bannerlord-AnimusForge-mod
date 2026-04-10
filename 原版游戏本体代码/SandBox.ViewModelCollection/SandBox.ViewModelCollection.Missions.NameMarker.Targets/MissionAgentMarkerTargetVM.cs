using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helpers;
using SandBox.Missions.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Quests;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionAgentMarkerTargetVM : MissionNameMarkerTargetVM<Agent>
{
	private class QuestMarkerComparer : IComparer<QuestMarkerVM>
	{
		public int Compare(QuestMarkerVM x, QuestMarkerVM y)
		{
			return x.QuestMarkerType.CompareTo(y.QuestMarkerType);
		}
	}

	public MissionAgentMarkerTargetVM(Agent target)
		: base(target)
	{
		base.NameType = "Normal";
		base.IconType = "character";
		BasicCharacterObject character = target.Character;
		CharacterObject val = (CharacterObject)(object)((character is CharacterObject) ? character : null);
		if (val != null)
		{
			Hero heroObject = val.HeroObject;
			if (heroObject != null && heroObject.IsLord)
			{
				base.IconType = "noble";
				base.NameType = "Noble";
				if (FactionManager.IsAtWarAgainstFaction(val.HeroObject.MapFaction, Hero.MainHero.MapFaction))
				{
					base.NameType = "Enemy";
					base.IsEnemy = true;
				}
				else if (DiplomacyHelper.IsSameFactionAndNotEliminated(val.HeroObject.MapFaction, Hero.MainHero.MapFaction))
				{
					base.NameType = "Friendly";
					base.IsFriendly = true;
				}
			}
			if (val.HeroObject != null && val.HeroObject.IsPrisoner)
			{
				base.IconType = "prisoner";
			}
			if (target.IsHuman && target != Agent.Main)
			{
				UpdateQuestStatus();
			}
			Settlement currentSettlement = Settlement.CurrentSettlement;
			object obj;
			if (currentSettlement == null)
			{
				obj = null;
			}
			else
			{
				CultureObject culture = currentSettlement.Culture;
				obj = ((culture != null) ? culture.Barber : null);
			}
			if (val == obj)
			{
				base.IconType = "barber";
			}
			else
			{
				Settlement currentSettlement2 = Settlement.CurrentSettlement;
				object obj2;
				if (currentSettlement2 == null)
				{
					obj2 = null;
				}
				else
				{
					CultureObject culture2 = currentSettlement2.Culture;
					obj2 = ((culture2 != null) ? culture2.Blacksmith : null);
				}
				if (val == obj2)
				{
					base.IconType = "blacksmith";
				}
				else
				{
					Settlement currentSettlement3 = Settlement.CurrentSettlement;
					object obj3;
					if (currentSettlement3 == null)
					{
						obj3 = null;
					}
					else
					{
						CultureObject culture3 = currentSettlement3.Culture;
						obj3 = ((culture3 != null) ? culture3.TavernGamehost : null);
					}
					if (val == obj3)
					{
						base.IconType = "game_host";
					}
					else if (((MBObjectBase)val).StringId == "sp_hermit")
					{
						base.IconType = "hermit";
					}
					else
					{
						BasicCharacterObject character2 = base.Target.Character;
						Settlement currentSettlement4 = Settlement.CurrentSettlement;
						object obj4;
						if (currentSettlement4 == null)
						{
							obj4 = null;
						}
						else
						{
							CultureObject culture4 = currentSettlement4.Culture;
							obj4 = ((culture4 != null) ? culture4.Shipwright : null);
						}
						if (character2 == obj4)
						{
							base.IconType = "shipwright";
						}
					}
				}
			}
		}
		((ViewModel)this).RefreshValues();
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, base.Target.GetEyeGlobalPosition() + MissionNameMarkerHelper.AgentHeightOffset);
	}

	protected override TextObject GetName()
	{
		return base.Target.NameTextObject;
	}

	public void UpdateQuestStatus()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		IssueQuestFlags val = (IssueQuestFlags)0;
		Agent target = base.Target;
		CharacterObject val2 = (CharacterObject)((target != null) ? target.Character : null);
		Hero val3 = (((int)val2 != 0) ? val2.HeroObject : null);
		if (val3 != null)
		{
			List<(IssueQuestFlags, TextObject, TextObject)> questStateOfHero = CampaignUIHelper.GetQuestStateOfHero(val3);
			for (int i = 0; i < questStateOfHero.Count; i++)
			{
				val |= questStateOfHero[i].Item1;
			}
		}
		if (base.Target != null)
		{
			BasicCharacterObject character = base.Target.Character;
			BasicCharacterObject obj = ((character is CharacterObject) ? character : null);
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				Hero heroObject = ((CharacterObject)obj).HeroObject;
				if (heroObject == null)
				{
					obj2 = null;
				}
				else
				{
					Clan clan = heroObject.Clan;
					obj2 = ((clan != null) ? clan.Leader : null);
				}
			}
			if (obj2 != Hero.MainHero)
			{
				Settlement currentSettlement = Settlement.CurrentSettlement;
				if (currentSettlement != null)
				{
					LocationComplex locationComplex = currentSettlement.LocationComplex;
					if (((locationComplex == null) ? ((bool?)null) : locationComplex.FindCharacter((IAgent)(object)base.Target)?.IsVisualTracked) == true)
					{
						val = (IssueQuestFlags)(val | 8);
					}
				}
			}
		}
		DisguiseMissionLogic missionBehavior = Mission.Current.GetMissionBehavior<DisguiseMissionLogic>();
		if (missionBehavior != null && missionBehavior.IsContactAgentTracked(base.Target))
		{
			val = (IssueQuestFlags)(val | 8);
		}
		IssueQuestFlags[] issueQuestFlagsValues = CampaignUIHelper.IssueQuestFlagsValues;
		foreach (IssueQuestFlags questFlag in issueQuestFlagsValues)
		{
			if ((int)questFlag != 0 && (val & questFlag) != 0 && LinQuick.AllQ<QuestMarkerVM>((IReadOnlyList<QuestMarkerVM>)base.Quests, (Func<QuestMarkerVM, bool>)((QuestMarkerVM q) => q.IssueQuestFlag != questFlag)))
			{
				((Collection<QuestMarkerVM>)(object)base.Quests).Add(new QuestMarkerVM(questFlag, (TextObject)null, (TextObject)null));
				if ((questFlag & 2) != 0 && (questFlag & 1) != 0 && (questFlag & 8) != 0)
				{
					base.IsTracked = true;
				}
				else if ((questFlag & 2) != 0 && (questFlag & 4) != 0 && (questFlag & 0x10) != 0)
				{
					base.IsQuestMainStory = true;
				}
			}
		}
		base.Quests.Sort((IComparer<QuestMarkerVM>)new QuestMarkerComparer());
	}
}
