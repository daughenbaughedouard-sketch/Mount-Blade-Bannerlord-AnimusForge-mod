using System;
using System.Collections.Generic;
using SandBox.GauntletUI.Map;
using SandBox.View.Map;
using StoryMode.GameComponents.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace StoryMode.GauntletUI.Map;

[OverrideView(typeof(MapCheatsView))]
internal class GauntletStoryModeMapCheatsView : GauntletMapCheatsView
{
	protected override void CreateLayout()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		((GauntletMapCheatsView)this).CreateLayout();
		AchievementsCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<AchievementsCampaignBehavior>();
		if (campaignBehavior == null || !campaignBehavior.CheckAchievementSystemActivity(out var _))
		{
			EnableCheatMenu();
			return;
		}
		base._layerAsGauntletLayer.UIContext.ContextAlpha = 0f;
		InformationManager.ShowInquiry(new InquiryData(((object)new TextObject("{=4Ygn4OGE}Enable Cheats", (Dictionary<string, object>)null)).ToString(), ((object)new TextObject("{=YkbOfPRU}Enabling cheats will disable the achievements this game. Do you want to proceed?", (Dictionary<string, object>)null)).ToString(), true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)EnableCheatMenu, (Action)RemoveCheatMenu, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void EnableCheatMenu()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		base._layerAsGauntletLayer.UIContext.ContextAlpha = 1f;
		AchievementsCampaignBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<AchievementsCampaignBehavior>();
		if (campaignBehavior != null && campaignBehavior.CheckAchievementSystemActivity(out var _))
		{
			campaignBehavior?.DeactivateAchievements(new TextObject("{=sO8Zh3ZH}Achievements are disabled due to cheat usage.", (Dictionary<string, object>)null));
		}
	}

	private void RemoveCheatMenu()
	{
		((MapView)this).MapScreen.CloseGameplayCheats();
	}

	protected override void OnMapConversationStart()
	{
		((GauntletMapCheatsView)this).OnMapConversationStart();
		if (base._layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)base._layerAsGauntletLayer, true);
		}
	}

	protected override void OnMapConversationOver()
	{
		((GauntletMapCheatsView)this).OnMapConversationOver();
		if (base._layerAsGauntletLayer != null)
		{
			ScreenManager.SetSuspendLayer((ScreenLayer)(object)base._layerAsGauntletLayer, false);
		}
	}
}
