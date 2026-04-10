using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace SandBox.CampaignBehaviors;

public class ConversationAnimationToolCampaignBehavior : CampaignBehaviorBase
{
	private static bool _isToolEnabled = false;

	private static int _characterType = -1;

	private static int _characterState = -1;

	private static int _characterGender = -1;

	private static int _characterAge = -1;

	private static int _characterWoundedState = -1;

	private static int _equipmentType = -1;

	private static int _relationType = -1;

	private static int _personaType = -1;

	public override void RegisterEvents()
	{
		CampaignEvents.TickEvent.AddNonSerializedListener((object)this, (Action<float>)Tick);
	}

	public override void SyncData(IDataStore dataStore)
	{
	}

	private void Tick(float dt)
	{
		if (_isToolEnabled)
		{
			StartImGUIWindow("Conversation Animation Test Tool");
			ImGUITextArea("Character Type:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("0 for noble, 1 for notable, 2 for companion, 3 for troop", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter character type: ", ref _characterType, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character State:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("0 for active, 1 for prisoner", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter character state: ", ref _characterState, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character Gender:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("0 for male, 1 for female", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter character gender: ", ref _characterGender, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character Age:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("Enter a custom age or leave -1 for not changing the age value", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter character age: ", ref _characterAge, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character Wounded State:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("Change to 1 to change character state to wounded", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter character wounded state: ", ref _characterWoundedState, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character Equipment Type:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("Change to 1 to change to equipment to civilian, default equipment is battle", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter equipment type: ", ref _equipmentType, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character Relation With Main Hero:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("Leave -1 for no change, 0 for enemy, 1 for neutral, 2 for friend", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter relation type: ", ref _relationType, separatorNeeded: false, onSameLine: false);
			Separator();
			ImGUITextArea("Character Persona Type:", separatorNeeded: false, onSameLine: false);
			ImGUITextArea("Leave -1 for no change, 0 for curt, 1 for earnest, 2 for ironic, 3 for softspoken", separatorNeeded: false, onSameLine: false);
			ImGUIIntegerField("Enter persona type: ", ref _personaType, separatorNeeded: false, onSameLine: false);
			Separator();
			if (ImGUIButton(" Start Conversation ", smallButton: true))
			{
				StartConversation();
			}
			EndImGUIWindow();
		}
	}

	public static void CloseConversationAnimationTool()
	{
		_isToolEnabled = false;
		_characterType = -1;
		_characterState = -1;
		_characterGender = -1;
		_characterAge = -1;
		_characterWoundedState = -1;
		_equipmentType = -1;
		_relationType = -1;
		_personaType = -1;
	}

	private static void StartConversation()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		bool flag2 = true;
		Occupation val = (Occupation)0;
		switch (_characterType)
		{
		case 0:
			val = (Occupation)3;
			break;
		case 1:
			val = (Occupation)18;
			break;
		case 2:
			val = (Occupation)16;
			break;
		case 3:
			val = (Occupation)7;
			flag2 = false;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return;
		}
		bool flag3 = false;
		bool flag4 = false;
		if (_characterState == 0)
		{
			flag3 = true;
		}
		else if (_characterState == 1)
		{
			flag4 = true;
		}
		else
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		bool flag5 = false;
		if (_characterGender == 1)
		{
			flag5 = true;
		}
		else if (_characterGender == 0)
		{
			flag5 = false;
		}
		else
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		bool flag6 = false;
		if (_characterAge == -1)
		{
			flag6 = false;
		}
		else if (_characterAge > 0 && _characterAge <= 128)
		{
			flag6 = true;
		}
		else
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		bool flag7 = _characterWoundedState == 1;
		bool flag8 = _equipmentType == 1;
		if (_relationType != 0 && _relationType != 1 && _relationType != 2)
		{
			return;
		}
		CharacterObject val2 = null;
		if (flag2)
		{
			Hero val3 = null;
			foreach (Hero item in (List<Hero>)(object)Hero.AllAliveHeroes)
			{
				if (item != Hero.MainHero && item.Occupation == val && item.IsFemale == flag5 && (item.PartyBelongedTo == null || item.PartyBelongedTo.MapEvent == null))
				{
					val3 = item;
					break;
				}
			}
			if (val3 == null)
			{
				val3 = HeroCreator.CreateNotable(val, (Settlement)null);
			}
			if (flag6)
			{
				val3.SetBirthDay(HeroHelper.GetRandomBirthDayForAge((float)_characterAge));
			}
			if (flag4)
			{
				TakePrisonerAction.Apply(PartyBase.MainParty, val3);
			}
			if (flag7)
			{
				val3.MakeWounded((Hero)null, (KillCharacterActionDetail)0);
			}
			if (flag3)
			{
				val3.ChangeState((CharacterStates)1);
			}
			val3.IsFemale = flag5;
			val2 = val3.CharacterObject;
		}
		else
		{
			foreach (CharacterObject item2 in (List<CharacterObject>)(object)CharacterObject.All)
			{
				if (item2.Occupation == val && ((BasicCharacterObject)item2).IsFemale == flag5)
				{
					val2 = item2;
					break;
				}
			}
			if (val2 == null)
			{
				val2 = ((GameType)Campaign.Current).ObjectManager.GetObject<CultureObject>("empire").BasicTroop;
			}
		}
		if (val2 == null)
		{
			return;
		}
		if (((BasicCharacterObject)val2).IsHero && _relationType != -1)
		{
			Hero heroObject = val2.HeroObject;
			float relationWithPlayer = heroObject.GetRelationWithPlayer();
			float num = 0f;
			if (_relationType == 0 && !heroObject.IsEnemy(Hero.MainHero))
			{
				num = 0f - relationWithPlayer - 15f;
			}
			else if (_relationType == 1 && !heroObject.IsNeutral(Hero.MainHero))
			{
				num = 0f - relationWithPlayer;
			}
			else if (_relationType == 2 && !heroObject.IsFriend(Hero.MainHero))
			{
				num = 0f - relationWithPlayer + 15f;
			}
			ChangeRelationAction.ApplyPlayerRelation(heroObject, (int)num, true, true);
		}
		CampaignMapConversation.OpenConversation(new ConversationCharacterData(CharacterObject.PlayerCharacter, (PartyBase)null, false, false, false, false, false, false), new ConversationCharacterData(val2, (PartyBase)null, false, false, false, flag8, flag8, false));
		CloseConversationAnimationTool();
	}

	private static void StartImGUIWindow(string str)
	{
		Imgui.BeginMainThreadScope();
		Imgui.Begin(str);
	}

	private static void ImGUITextArea(string text, bool separatorNeeded, bool onSameLine)
	{
		Imgui.Text(text);
		ImGUISeparatorSameLineHandler(separatorNeeded, onSameLine);
	}

	private static bool ImGUIButton(string buttonText, bool smallButton)
	{
		if (smallButton)
		{
			return Imgui.SmallButton(buttonText);
		}
		return Imgui.Button(buttonText);
	}

	private static void ImGUIIntegerField(string fieldText, ref int value, bool separatorNeeded, bool onSameLine)
	{
		Imgui.InputInt(fieldText, ref value);
		ImGUISeparatorSameLineHandler(separatorNeeded, onSameLine);
	}

	private static void ImGUICheckBox(string text, ref bool is_checked, bool separatorNeeded, bool onSameLine)
	{
		Imgui.Checkbox(text, ref is_checked);
		ImGUISeparatorSameLineHandler(separatorNeeded, onSameLine);
	}

	private static void ImGUISeparatorSameLineHandler(bool separatorNeeded, bool onSameLine)
	{
		if (separatorNeeded)
		{
			Separator();
		}
		if (onSameLine)
		{
			OnSameLine();
		}
	}

	private static void OnSameLine()
	{
		Imgui.SameLine(0f, 0f);
	}

	private static void Separator()
	{
		Imgui.Separator();
	}

	private static void EndImGUIWindow()
	{
		Imgui.End();
		Imgui.EndMainThreadScope();
	}
}
