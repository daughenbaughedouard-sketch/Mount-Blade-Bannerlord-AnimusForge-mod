using System;
using System.Collections.Generic;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

[OverrideView(typeof(MissionMultiplayerPracticeEscapeMenu))]
public class MissionGauntletMultiplayerPracticeEscapeMenu : MissionGauntletEscapeMenuBase
{
	public MissionGauntletMultiplayerPracticeEscapeMenu()
		: base("MultiplayerEscapeMenu")
	{
	}

	public override void OnMissionScreenInitialize()
	{
		((MissionView)this).OnMissionScreenInitialize();
		base.DataSource = (EscapeMenuVM)(object)new MPEscapeMenuVM(null);
	}

	public override void OnMissionScreenTick(float dt)
	{
		((MissionGauntletEscapeMenuBase)this).OnMissionScreenTick(dt);
		base.DataSource.Tick(dt);
	}

	protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		return new List<EscapeMenuItemVM>
		{
			new EscapeMenuItemVM(new TextObject("{=e139gKZc}Return to the Game", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false),
			new EscapeMenuItemVM(new TextObject("{=EXqcmGy4}Return to Lobby", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				((MissionGauntletEscapeMenuBase)this).OnEscapeMenuToggled(false);
				((MissionBehavior)this).Mission.EndMission();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false)
		};
	}
}
