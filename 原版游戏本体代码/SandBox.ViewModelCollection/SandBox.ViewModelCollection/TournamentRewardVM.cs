using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection;

public class TournamentRewardVM : ViewModel
{
	private string _text;

	private ItemImageIdentifierVM _imageIdentifier;

	private bool _gotImageIdentifier;

	[DataSourceProperty]
	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (value != _text)
			{
				_text = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Text");
			}
		}
	}

	[DataSourceProperty]
	public bool GotImageIdentifier
	{
		get
		{
			return _gotImageIdentifier;
		}
		set
		{
			if (value != _gotImageIdentifier)
			{
				_gotImageIdentifier = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "GotImageIdentifier");
			}
		}
	}

	[DataSourceProperty]
	public ItemImageIdentifierVM ImageIdentifier
	{
		get
		{
			return _imageIdentifier;
		}
		set
		{
			if (value != _imageIdentifier)
			{
				_imageIdentifier = value;
				((ViewModel)this).OnPropertyChangedWithValue<ItemImageIdentifierVM>(value, "ImageIdentifier");
			}
		}
	}

	public TournamentRewardVM(string text)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		Text = text;
		GotImageIdentifier = false;
		ImageIdentifier = new ItemImageIdentifierVM((ItemObject)null, "");
	}

	public TournamentRewardVM(string text, ItemImageIdentifierVM imageIdentifierVM)
	{
		Text = text;
		GotImageIdentifier = true;
		ImageIdentifier = imageIdentifierVM;
	}
}
