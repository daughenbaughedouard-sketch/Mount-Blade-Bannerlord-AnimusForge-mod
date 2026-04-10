using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class PortPieceImageBrushWidget : BrushWidget
{
	private string _identifier;

	public string Identifier
	{
		get
		{
			return _identifier;
		}
		set
		{
			if (value != _identifier)
			{
				_identifier = value;
				((PropertyOwnerObject)this).OnPropertyChanged<string>(value, "Identifier");
				UpdateIcon();
			}
		}
	}

	public PortPieceImageBrushWidget(UIContext context)
		: base(context)
	{
	}

	private void UpdateIcon()
	{
		if (((BrushWidget)this).Brush == null)
		{
			return;
		}
		Sprite sprite = ((Widget)this).Context.SpriteData.GetSprite("PieceThumbnails\\" + Identifier);
		((BrushWidget)this).Brush.Sprite = sprite;
		foreach (BrushLayer layer in ((BrushWidget)this).Brush.Layers)
		{
			layer.Sprite = sprite;
		}
	}
}
