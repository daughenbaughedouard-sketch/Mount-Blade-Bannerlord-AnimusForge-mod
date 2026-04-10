using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Library.NewsManager;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;

public class MPNewsVM : ViewModel
{
	private NewsManager _newsManager;

	private const int _numOfNewsItemsToShow = 4;

	private MBReadOnlyList<NewsItem> _newsItemsCached;

	private bool _hasValidNews;

	private MPNewsItemVM _mainNews;

	private MBBindingList<MPNewsItemVM> _importantNews;

	[DataSourceProperty]
	public bool HasValidNews
	{
		get
		{
			return _hasValidNews;
		}
		set
		{
			if (value != _hasValidNews)
			{
				_hasValidNews = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasValidNews");
			}
		}
	}

	[DataSourceProperty]
	public MPNewsItemVM MainNews
	{
		get
		{
			return _mainNews;
		}
		set
		{
			if (value != _mainNews)
			{
				_mainNews = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPNewsItemVM>(value, "MainNews");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MPNewsItemVM> ImportantNews
	{
		get
		{
			return _importantNews;
		}
		set
		{
			if (value != _importantNews)
			{
				_importantNews = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MPNewsItemVM>>(value, "ImportantNews");
			}
		}
	}

	public MPNewsVM(NewsManager newsManager)
	{
		_newsManager = newsManager;
		ImportantNews = new MBBindingList<MPNewsItemVM>();
		GetNewsItems();
	}

	private async void GetNewsItems()
	{
		if (_newsManager == null)
		{
			Debug.FailedAssert("News manager is null!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Home\\MPNewsVM.cs", "GetNewsItems", 27);
			return;
		}
		_newsItemsCached = await _newsManager.GetNewsItems(false);
		RefreshNews();
	}

	private unsafe void RefreshNews()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		MainNews = null;
		((Collection<MPNewsItemVM>)(object)ImportantNews).Clear();
		HasValidNews = false;
		if (_newsItemsCached == null)
		{
			Debug.FailedAssert("News items list is null!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\Lobby\\Home\\MPNewsVM.cs", "RefreshNews", 43);
			return;
		}
		List<IGrouping<int, NewsItem>> list = (from i in ((IEnumerable<NewsItem>)_newsItemsCached).Where((NewsItem n) => ((NewsItem)(ref n)).Feeds.Any((NewsType t) => (int)((NewsType)(ref t)).Type == 2) && !string.IsNullOrEmpty(((NewsItem)(ref n)).Title) && !string.IsNullOrEmpty(((NewsItem)(ref n)).NewsLink) && !string.IsNullOrEmpty(((NewsItem)(ref n)).ImageSourcePath)).ToList().GroupBy(delegate(NewsItem i)
			{
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				NewsType val2 = ((NewsItem)(ref i)).Feeds.First((NewsType t) => (int)((NewsType)(ref t)).Type == 2);
				return ((NewsType)(ref val2)).Index;
			})
				.ToList()
			orderby i.Key
			select i).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			if (((Collection<MPNewsItemVM>)(object)ImportantNews).Count + 1 >= 4)
			{
				break;
			}
			NewsItem val = list[num].First();
			NewsItem item = (NewsItem)(((object)(*(NewsItem*)(&val))/*cast due to .constrained prefix*/).Equals((object)default(NewsItem)) ? default(NewsItem) : val);
			if (num == 0)
			{
				MainNews = new MPNewsItemVM(item);
			}
			else
			{
				((Collection<MPNewsItemVM>)(object)ImportantNews).Add(new MPNewsItemVM(item));
			}
		}
		if (MainNews != null)
		{
			HasValidNews = true;
		}
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		_newsManager = null;
	}
}
