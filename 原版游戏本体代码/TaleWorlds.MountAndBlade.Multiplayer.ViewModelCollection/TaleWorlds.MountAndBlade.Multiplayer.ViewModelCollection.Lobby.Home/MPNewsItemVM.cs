using System.Diagnostics;
using TaleWorlds.Library;
using TaleWorlds.Library.NewsManager;
using TaleWorlds.PlatformService;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Home;

public class MPNewsItemVM : ViewModel
{
	private readonly string _link;

	private string _newsImageUrl;

	private string _category;

	private string _title;

	[DataSourceProperty]
	public string NewsImageUrl
	{
		get
		{
			return _newsImageUrl;
		}
		set
		{
			if (value != _newsImageUrl)
			{
				_newsImageUrl = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "NewsImageUrl");
			}
		}
	}

	[DataSourceProperty]
	public string Category
	{
		get
		{
			return _category;
		}
		set
		{
			if (value != _category)
			{
				_category = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Category");
			}
		}
	}

	[DataSourceProperty]
	public string Title
	{
		get
		{
			return _title;
		}
		set
		{
			if (value != _title)
			{
				_title = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Title");
			}
		}
	}

	public MPNewsItemVM(NewsItem item)
	{
		NewsImageUrl = ((NewsItem)(ref item)).ImageSourcePath;
		Category = ((NewsItem)(ref item)).Title;
		Title = ((NewsItem)(ref item)).Description;
		_link = ((NewsItem)(ref item)).NewsLink + "?referrer=lobby";
	}

	private void ExecuteOpenLink()
	{
		if (!string.IsNullOrEmpty(_link) && !PlatformServices.Instance.ShowOverlayForWebPage(_link).Result)
		{
			Process.Start(new ProcessStartInfo(_link)
			{
				UseShellExecute = true
			});
		}
	}
}
