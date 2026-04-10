using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SandBox;
using SandBox.View;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using StoryMode.Extensions;
using StoryMode.View.Permissions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace StoryMode.View;

public class StoryModeViewSubModule : MBSubModuleBase
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static CampaignCreatorDelegate _003C_003E9__8_0;

		internal Campaign _003CStartGame_003Eb__8_0()
		{
			return (Campaign)(object)new CampaignStoryMode((CampaignGameMode)1);
		}
	}

	private bool _startedStoryMode;

	public override void OnGameInitializationFinished(Game game)
	{
		((MBSubModuleBase)this).OnGameInitializationFinished(game);
		StoryModePermissionsSystem.OnInitialize();
	}

	public override void OnGameEnd(Game game)
	{
		((MBSubModuleBase)this).OnGameEnd(game);
		StoryModePermissionsSystem.OnUnload();
	}

	protected override void OnSubModuleLoad()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		((MBSubModuleBase)this).OnSubModuleLoad();
		TextObject coreContentDisabledReason = new TextObject("{=V8BXjyYq}Disabled during installation.", (Dictionary<string, object>)null);
		Module.CurrentModule.AddInitialStateOption(new InitialStateOption("StoryModeNewGame", new TextObject("{=sf_menu_storymode_new_game}New Campaign", (Dictionary<string, object>)null), 2, (Action)delegate
		{
			StartGame();
		}, (Func<ValueTuple<bool, TextObject>>)(() => (Module.CurrentModule.IsOnlyCoreContentEnabled, coreContentDisabledReason)), (TextObject)null, (Func<bool>)null));
		Module.CurrentModule.ImguiProfilerTick += OnImguiProfilerTick;
	}

	protected virtual void FillDataForCampaign()
	{
	}

	protected override void OnSubModuleUnloaded()
	{
		Module.CurrentModule.ImguiProfilerTick -= OnImguiProfilerTick;
		((MBSubModuleBase)this).OnSubModuleUnloaded();
	}

	public override void OnSubModuleDeactivated()
	{
	}

	public override void OnSubModuleActivated()
	{
	}

	private void StartGame()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		_startedStoryMode = true;
		object obj = _003C_003Ec._003C_003E9__8_0;
		if (obj == null)
		{
			CampaignCreatorDelegate val = () => (Campaign)(object)new CampaignStoryMode((CampaignGameMode)1);
			_003C_003Ec._003C_003E9__8_0 = val;
			obj = (object)val;
		}
		MBGameManager.StartNewGame((MBGameManager)new SandBoxGameManager((CampaignCreatorDelegate)obj));
		_startedStoryMode = false;
	}

	protected override void OnBeforeGameStart(MBGameManager mbGameManager, List<string> disabledModules)
	{
		SandBoxGameManager val;
		if ((val = (SandBoxGameManager)(object)((mbGameManager is SandBoxGameManager) ? mbGameManager : null)) != null && (val.LoadingSavedGame ? (!val.MetaData.HasStoryMode()) : (!_startedStoryMode)))
		{
			disabledModules.Add("StoryMode");
		}
	}

	private void OnImguiProfilerTick()
	{
		if (Campaign.Current == null)
		{
			return;
		}
		MBReadOnlyList<MobileParty> all = MobileParty.All;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<EntityVisualManagerBase<PartyBase>> components = SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents<EntityVisualManagerBase<PartyBase>>();
		foreach (MobileParty item in (List<MobileParty>)(object)all)
		{
			if (item.IsMilitia || item.IsGarrison)
			{
				continue;
			}
			if (item.IsVisible)
			{
				num++;
			}
			MapEntityVisual<PartyBase> val = null;
			foreach (EntityVisualManagerBase<PartyBase> item2 in components)
			{
				MapEntityVisual<PartyBase> visualOfEntity = item2.GetVisualOfEntity(PartyBase.MainParty);
				if (visualOfEntity != null)
				{
					val = visualOfEntity;
				}
			}
			if (val == null)
			{
				continue;
			}
			MobilePartyVisual val2;
			if ((val2 = (MobilePartyVisual)(object)((val is MobilePartyVisual) ? val : null)) != null)
			{
				if (val2.HumanAgentVisuals != null)
				{
					num2++;
				}
				if (val2.MountAgentVisuals != null)
				{
					num2++;
				}
				if (val2.CaravanMountAgentVisuals != null)
				{
					num2++;
				}
			}
			num3++;
		}
		Imgui.BeginMainThreadScope();
		Imgui.Begin("Bannerlord Campaign Statistics");
		Imgui.Columns(2, "", true);
		Imgui.Text("Name");
		Imgui.NextColumn();
		Imgui.Text("Count");
		Imgui.NextColumn();
		Imgui.Separator();
		Imgui.Text("Total Mobile Party");
		Imgui.NextColumn();
		Imgui.Text(num3.ToString());
		Imgui.NextColumn();
		Imgui.Text("Visible Mobile Party");
		Imgui.NextColumn();
		Imgui.Text(num.ToString());
		Imgui.NextColumn();
		Imgui.Text("Total Agent Visuals");
		Imgui.NextColumn();
		Imgui.Text(num2.ToString());
		Imgui.NextColumn();
		Imgui.End();
		Imgui.EndMainThreadScope();
	}
}
