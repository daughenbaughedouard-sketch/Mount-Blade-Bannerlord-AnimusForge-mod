using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.Encyclopedia;

[OverrideView(typeof(MapEncyclopediaView))]
public class GauntletMapEncyclopediaView : MapEncyclopediaView
{
	private EncyclopediaHomeVM _homeDatasource;

	private EncyclopediaNavigatorVM _navigatorDatasource;

	private EncyclopediaData _encyclopediaData;

	public EncyclopediaListViewDataController ListViewDataController;

	private SpriteCategory _spriteCategory;

	private Game _game;

	protected override void CreateLayout()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		base.CreateLayout();
		_spriteCategory = UIResourceManager.LoadSpriteCategory("ui_encyclopedia");
		_homeDatasource = new EncyclopediaHomeVM(new EncyclopediaPageArgs((object)null));
		_navigatorDatasource = new EncyclopediaNavigatorVM((Func<string, object, bool, EncyclopediaPageVM>)ExecuteLink, (Action)CloseEncyclopedia);
		ListViewDataController = new EncyclopediaListViewDataController();
		_game = Game.Current;
		Game game = _game;
		game.AfterTick = (Action<float>)Delegate.Combine(game.AfterTick, new Action<float>(OnTick));
	}

	internal void OnTick(float dt)
	{
		_encyclopediaData?.OnTick();
	}

	private EncyclopediaPageVM ExecuteLink(string pageId, object obj, bool needsRefresh)
	{
		_navigatorDatasource.NavBarString = string.Empty;
		if (_encyclopediaData == null)
		{
			_encyclopediaData = new EncyclopediaData(this, ScreenManager.TopScreen, _homeDatasource, _navigatorDatasource);
		}
		if (pageId == "LastPage")
		{
			Tuple<string, object> lastPage = _navigatorDatasource.GetLastPage();
			pageId = lastPage.Item1;
			obj = lastPage.Item2;
		}
		base.IsEncyclopediaOpen = true;
		if (!_spriteCategory.IsLoaded)
		{
			Extensions.Load(_spriteCategory);
		}
		return _encyclopediaData.ExecuteLink(pageId, obj, needsRefresh);
	}

	protected override void OnFinalize()
	{
		Game game = _game;
		game.AfterTick = (Action<float>)Delegate.Remove(game.AfterTick, new Action<float>(OnTick));
		_game = null;
		EncyclopediaHomeVM homeDatasource = _homeDatasource;
		if (homeDatasource != null)
		{
			((ViewModel)homeDatasource).OnFinalize();
		}
		_homeDatasource = null;
		EncyclopediaNavigatorVM navigatorDatasource = _navigatorDatasource;
		if (navigatorDatasource != null)
		{
			((ViewModel)navigatorDatasource).OnFinalize();
		}
		_navigatorDatasource = null;
		_encyclopediaData = null;
		base.OnFinalize();
	}

	public override void CloseEncyclopedia()
	{
		_encyclopediaData.CloseEncyclopedia();
		_encyclopediaData = null;
		base.IsEncyclopediaOpen = false;
	}

	protected override TutorialContexts GetTutorialContext()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsEncyclopediaOpen)
		{
			return (TutorialContexts)9;
		}
		return base.GetTutorialContext();
	}
}
