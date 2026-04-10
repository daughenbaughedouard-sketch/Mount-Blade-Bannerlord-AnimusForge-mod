using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.BannerEditor;

[GameStateScreen(typeof(BannerEditorState))]
public class GauntletBannerEditorScreen : ScreenBase, IGameStateListener
{
	private const int ViewOrderPriority = 15;

	private readonly BannerEditorView _bannerEditorLayer;

	private readonly Clan _clan;

	public GauntletBannerEditorScreen(BannerEditorState bannerEditorState)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0061: Expected O, but got Unknown
		//IL_0061: Expected O, but got Unknown
		//IL_0061: Expected O, but got Unknown
		LoadingWindow.EnableGlobalLoadingWindow();
		_clan = bannerEditorState.GetClan();
		_bannerEditorLayer = new BannerEditorView((BasicCharacterObject)(object)bannerEditorState.GetCharacter(), bannerEditorState.GetClan().Banner, new ControlCharacterCreationStage(OnDone), new TextObject("{=WiNRdfsm}Done", (Dictionary<string, object>)null), new ControlCharacterCreationStage(OnCancel), new TextObject("{=3CpNUnVl}Cancel", (Dictionary<string, object>)null));
		_bannerEditorLayer.DataSource.SetClanRelatedRules(bannerEditorState.GetClan().Kingdom == null);
	}

	protected override void OnFrameTick(float dt)
	{
		((ScreenBase)this).OnFrameTick(dt);
		_bannerEditorLayer.OnTick(dt);
	}

	public void OnDone()
	{
		uint primaryColor = _bannerEditorLayer.DataSource.BannerVM.Banner.GetPrimaryColor();
		uint firstIconColor = _bannerEditorLayer.DataSource.BannerVM.Banner.GetFirstIconColor();
		_clan.Color2 = firstIconColor;
		if (_bannerEditorLayer.DataSource.CanChangeBackgroundColor)
		{
			_clan.Color = primaryColor;
			_clan.UpdateBannerColor(primaryColor, firstIconColor);
		}
		else
		{
			_clan.UpdateBannerColor(_clan.Color, firstIconColor);
		}
		Game.Current.GameStateManager.PopState(0);
	}

	public void OnCancel()
	{
		Game.Current.GameStateManager.PopState(0);
	}

	protected override void OnInitialize()
	{
		((ScreenBase)this).OnInitialize();
		Game.Current.GameStateManager.RegisterActiveStateDisableRequest((object)this);
		InformationManager.HideAllMessages();
	}

	protected override void OnFinalize()
	{
		((ScreenBase)this).OnFinalize();
		_bannerEditorLayer.OnFinalize();
		if (LoadingWindow.IsLoadingWindowActive)
		{
			LoadingWindow.DisableGlobalLoadingWindow();
		}
		Game.Current.GameStateManager.UnregisterActiveStateDisableRequest((object)this);
	}

	protected override void OnActivate()
	{
		((ScreenBase)this).OnActivate();
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_bannerEditorLayer.GauntletLayer);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_bannerEditorLayer.SceneLayer);
	}

	protected override void OnDeactivate()
	{
		_bannerEditorLayer.OnDeactivate();
	}

	void IGameStateListener.OnActivate()
	{
	}

	void IGameStateListener.OnDeactivate()
	{
	}

	void IGameStateListener.OnInitialize()
	{
	}

	void IGameStateListener.OnFinalize()
	{
	}
}
