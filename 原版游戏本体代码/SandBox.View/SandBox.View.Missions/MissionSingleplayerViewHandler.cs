using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Missions;

public class MissionSingleplayerViewHandler : MissionView
{
	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		if (!((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.IsCategoryRegistered(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory")))
		{
			((ScreenLayer)((MissionView)this).MissionScreen.SceneLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericCampaignPanelsGameKeyCategory"));
		}
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Expected O, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Expected O, but got Unknown
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Expected O, but got Unknown
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Invalid comparison between Unknown and I4
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Expected O, but got Unknown
		((MissionView)this).OnMissionScreenTick(dt);
		if (((MissionBehavior)this).Mission == null || ((MissionView)this).MissionScreen.IsPhotoModeEnabled || ((MissionBehavior)this).Mission.MissionEnded)
		{
			return;
		}
		if (((MissionView)this).Input.IsGameKeyPressed(38))
		{
			if (((MissionBehavior)this).Mission.IsInventoryAccessAllowed)
			{
				InventoryScreenHelper.OpenScreenAsInventory((Action)OnInventoryScreenDone);
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText(((int)((MissionBehavior)this).Mission.Mode == 2 || (int)((MissionBehavior)this).Mission.Mode == 3) ? "str_cannot_reach_inventory_during_battle" : "str_cannot_reach_inventory", (string)null)).ToString()));
			}
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(42))
		{
			if (((MissionBehavior)this).Mission.IsQuestScreenAccessAllowed)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<QuestsState>(), 0);
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_quests", (string)null)).ToString()));
			}
		}
		else if (!((MissionView)this).Input.IsControlDown() && ((MissionView)this).Input.IsGameKeyPressed(43))
		{
			if (((MissionBehavior)this).Mission.IsPartyWindowAccessAllowed)
			{
				PartyScreenHelper.OpenScreenAsNormal();
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_party", (string)null)).ToString()));
			}
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(39))
		{
			if (((MissionBehavior)this).Mission.IsEncyclopediaWindowAccessAllowed)
			{
				Campaign.Current.EncyclopediaManager.GoToLink("LastPage", "");
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_encyclopedia", (string)null)).ToString()));
			}
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(40))
		{
			if (((MissionBehavior)this).Mission.IsKingdomWindowAccessAllowed && Hero.MainHero.MapFaction.IsKingdomFaction)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<KingdomState>(), 0);
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_kingdom", (string)null)).ToString()));
			}
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(41))
		{
			if (((MissionBehavior)this).Mission.IsClanWindowAccessAllowed)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<ClanState>(), 0);
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_clan", (string)null)).ToString()));
			}
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(45))
		{
			InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_fleet", (string)null)).ToString()));
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(37))
		{
			if (((MissionBehavior)this).Mission.IsCharacterWindowAccessAllowed)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<CharacterDeveloperState>(), 0);
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_character", (string)null)).ToString()));
			}
		}
		else if (((MissionView)this).Input.IsGameKeyPressed(36))
		{
			if (((MissionBehavior)this).Mission.IsBannerWindowAccessAllowed && Campaign.Current.IsBannerEditorEnabled)
			{
				Game.Current.GameStateManager.PushState((GameState)(object)Game.Current.GameStateManager.CreateState<BannerEditorState>(), 0);
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_cannot_open_banner", (string)null)).ToString()));
			}
		}
		else
		{
			if (((Campaign.Current == null || (int)Campaign.Current.GameMode != 2) && EditorGame.Current == null) || !((MissionBehavior)this).DebugInput.IsHotKeyDown("MissionSingleplayerUiHandlerHotkeyUpdateItems"))
			{
				return;
			}
			MBDebug.ShowWarning("spitems.xml and mpitems.xml will be reloaded!");
			foreach (XmlNode childNode in Game.Current.ObjectManager.LoadXMLFromFileSkipValidation(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/mpitems.xml", ModuleHelper.GetModuleFullPath("Sandbox") + "ModuleData/spitems.xsd").ChildNodes[1].ChildNodes)
			{
				XmlAttributeCollection attributes = childNode.Attributes;
				if (attributes != null)
				{
					string innerText = attributes["id"].InnerText;
					ItemObject val = Game.Current.ObjectManager.GetObject<ItemObject>(innerText);
					MBObjectManager.Instance.UnregisterObject((MBObjectBase)(object)val);
					if (val != null)
					{
						((MBObjectBase)val).Deserialize(Game.Current.ObjectManager, childNode);
					}
				}
			}
			string text = BasePath.Name + "/Modules/SandBoxCore/ModuleData/spitems";
			FileInfo[] files = new DirectoryInfo(text).GetFiles("*.xml");
			foreach (FileInfo fileInfo in files)
			{
				foreach (XmlNode childNode2 in Game.Current.ObjectManager.LoadXMLFromFileSkipValidation(text + "/" + fileInfo.Name, ModuleHelper.GetModuleFullPath("Sandbox") + "ModuleData/spitems.xsd").ChildNodes[1].ChildNodes)
				{
					XmlAttributeCollection attributes2 = childNode2.Attributes;
					if (attributes2 != null)
					{
						string innerText2 = attributes2["id"].InnerText;
						ItemObject val2 = Game.Current.ObjectManager.GetObject<ItemObject>(innerText2);
						MBObjectManager.Instance.UnregisterObject((MBObjectBase)(object)val2);
						if (val2 != null)
						{
							((MBObjectBase)val2).Deserialize(Game.Current.ObjectManager, childNode2);
						}
					}
				}
			}
		}
	}

	private void OnInventoryScreenDone()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		Mission current = Mission.Current;
		if (((current != null) ? current.Agents : null) == null)
		{
			return;
		}
		foreach (Agent item in (List<Agent>)(object)Mission.Current.Agents)
		{
			if (item == null)
			{
				continue;
			}
			CharacterObject val = (CharacterObject)item.Character;
			Campaign current3 = Campaign.Current;
			bool num;
			if (current3 == null || (int)current3.GameMode != 2)
			{
				if (!item.IsHuman || val == null || !((BasicCharacterObject)val).IsHero)
				{
					continue;
				}
				Hero heroObject = val.HeroObject;
				num = ((heroObject != null) ? heroObject.PartyBelongedTo : null) == MobileParty.MainParty;
			}
			else
			{
				if (!item.IsMainAgent)
				{
					continue;
				}
				num = val != null;
			}
			if (num)
			{
				item.UpdateSpawnEquipmentAndRefreshVisuals(Mission.Current.DoesMissionRequireCivilianEquipment ? ((BasicCharacterObject)val).FirstCivilianEquipment : ((BasicCharacterObject)val).FirstBattleEquipment);
			}
		}
	}

	[Conditional("DEBUG")]
	private void OnDebugTick()
	{
		if (((MissionBehavior)this).DebugInput.IsHotKeyDown("MissionSingleplayerUiHandlerHotkeyJoinEnemyTeam"))
		{
			((MissionBehavior)this).Mission.JoinEnemyTeam();
		}
	}
}
