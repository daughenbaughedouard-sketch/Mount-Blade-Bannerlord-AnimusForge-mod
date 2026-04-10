using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public class LobbyPracticeState : GameState
{
	private bool _practiceOpened;

	protected override void OnActivate()
	{
		((GameState)this).OnActivate();
		if (_practiceOpened)
		{
			((GameState)this).GameStateManager.PopState(0);
		}
	}

	protected override void OnTick(float dt)
	{
		((GameState)this).OnTick(dt);
		if (!_practiceOpened)
		{
			OpenPracticeMission();
			_practiceOpened = true;
		}
	}

	private void OpenPracticeMission()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		BasicCharacterObject val = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("mp_heavy_cavalry_empire_hero");
		BasicCharacterObject val2 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("mp_skirmisher_battania_troop");
		BasicCharacterObject val3 = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("mp_light_ranged_khuzait_troop");
		Game.Current.PlayerTroop = val;
		BasicCultureObject val5;
		BasicCultureObject val4 = (val5 = Game.Current.ObjectManager.GetObject<BasicCultureObject>("empire"));
		Banner banner = val5.Banner;
		Banner banner2 = val4.Banner;
		CustomBattleCombatant val6 = new CustomBattleCombatant(new TextObject("{=sSJSTe5p}Player Party", (Dictionary<string, object>)null), val5, banner);
		CustomBattleCombatant val7 = new CustomBattleCombatant(new TextObject("{=0xC75dN6}Enemy Party", (Dictionary<string, object>)null), val4, banner2);
		val6.AddCharacter(val, 1);
		val7.AddCharacter(val, 1);
		val6.AddCharacter(val2, 3);
		val7.AddCharacter(val2, 3);
		val6.AddCharacter(val3, 8);
		val7.AddCharacter(val3, 8);
		val6.SetGeneral(val);
		val7.SetGeneral(val);
		val6.Side = (BattleSideEnum)1;
		val7.Side = (BattleSideEnum)0;
		MultiplayerPracticeMissions.OpenMultiplayerPracticeMission("mp_practice_battle", val, val6, val7, isPlayerGeneral: true, null, "", "summer");
	}
}
