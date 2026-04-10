using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SceneInformationPopupTypes;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.SceneInformationPopupTypes;

public class NavalSaveSisterSceneNotificationItem : SceneNotificationData
{
	private readonly Action _onCloseAction;

	public Hero MainHero { get; private set; }

	public Hero Sister { get; private set; }

	public override string SceneID => "cutscene_saving_sister";

	public override RelevantContextType RelevantContext => (RelevantContextType)4;

	public override TextObject TitleText => new TextObject("{=kpBuCL0h}The danger has passed. Your sister is now out of harm's way.", (Dictionary<string, object>)null);

	public NavalSaveSisterSceneNotificationItem(Hero mainHero, Hero sister, Action onCloseAction)
	{
		MainHero = mainHero;
		Sister = sister;
		_onCloseAction = onCloseAction;
	}

	public override SceneNotificationCharacter[] GetSceneNotificationCharacters()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		new List<SceneNotificationCharacter>();
		Equipment val = MainHero.BattleEquipment.Clone(false);
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref val, true, false);
		Equipment val2 = Sister.BattleEquipment.Clone(false);
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref val2, true, false);
		Equipment val3 = Sister.BattleEquipment.Clone(false);
		CampaignSceneNotificationHelper.RemoveWeaponsFromEquipment(ref val3, true, false);
		return (SceneNotificationCharacter[])(object)new SceneNotificationCharacter[3]
		{
			CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(MainHero, val, false, default(BodyProperties), uint.MaxValue, uint.MaxValue, false),
			CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(Sister, val2, false, default(BodyProperties), uint.MaxValue, uint.MaxValue, false),
			CampaignSceneNotificationHelper.CreateNotificationCharacterFromHero(Sister, val3, false, default(BodyProperties), uint.MaxValue, uint.MaxValue, false)
		};
	}

	public override void OnCloseAction()
	{
		((SceneNotificationData)this).OnCloseAction();
		_onCloseAction?.Invoke();
	}
}
