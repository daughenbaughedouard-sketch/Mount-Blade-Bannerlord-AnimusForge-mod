using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class ItemTableauTextureProvider : TextureProvider
{
	private readonly ItemTableau _itemTableau;

	private Texture _texture;

	private Texture _providedTexture;

	public string ItemModifierId
	{
		set
		{
			_itemTableau.SetItemModifierId(value);
		}
	}

	public string StringId
	{
		set
		{
			_itemTableau.SetStringId(value);
		}
	}

	public ItemRosterElement Item
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_itemTableau.SetItem(value);
		}
	}

	public int Ammo
	{
		set
		{
			_itemTableau.SetAmmo(value);
		}
	}

	public int AverageUnitCost
	{
		set
		{
			_itemTableau.SetAverageUnitCost(value);
		}
	}

	public string BannerCode
	{
		set
		{
			_itemTableau.SetBannerCode(value);
		}
	}

	public bool CurrentlyRotating
	{
		set
		{
			_itemTableau.RotateItem(value);
		}
	}

	public float RotateItemVertical
	{
		set
		{
			_itemTableau.RotateItemVerticalWithAmount(value);
		}
	}

	public float RotateItemHorizontal
	{
		set
		{
			_itemTableau.RotateItemHorizontalWithAmount(value);
		}
	}

	public float InitialTiltRotation
	{
		set
		{
			_itemTableau.SetInitialTiltRotation(value);
		}
	}

	public float InitialPanRotation
	{
		set
		{
			_itemTableau.SetInitialPanRotation(value);
		}
	}

	public float CurrentZoom
	{
		set
		{
			_itemTableau.Zoom(value);
		}
	}

	public ItemTableauTextureProvider()
	{
		_itemTableau = new ItemTableau();
	}

	public override void Clear(bool clearNextFrame)
	{
		_itemTableau.OnFinalize();
		((TextureProvider)this).Clear(clearNextFrame);
	}

	private void CheckTexture()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((NativeObject)(object)_texture != (NativeObject)(object)_itemTableau.Texture)
		{
			_texture = _itemTableau.Texture;
			if ((NativeObject)(object)_texture != (NativeObject)null)
			{
				EngineTexture val = new EngineTexture(_texture);
				_providedTexture = new Texture((ITexture)(object)val);
			}
			else
			{
				_providedTexture = null;
			}
		}
	}

	protected override Texture OnGetTextureForRender(TwoDimensionContext twoDimensionContext, string name)
	{
		CheckTexture();
		return _providedTexture;
	}

	public override void SetTargetSize(int width, int height)
	{
		((TextureProvider)this).SetTargetSize(width, height);
		_itemTableau.SetTargetSize(width, height);
	}

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		CheckTexture();
		_itemTableau.OnTick(dt);
	}
}
