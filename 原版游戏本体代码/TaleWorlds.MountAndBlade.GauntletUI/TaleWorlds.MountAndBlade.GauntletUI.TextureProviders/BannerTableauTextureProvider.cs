using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class BannerTableauTextureProvider : TextureProvider
{
	private BannerTableau _bannerTableau;

	private Texture _texture;

	private Texture _providedTexture;

	private bool _isHidden;

	public string BannerCodeText
	{
		set
		{
			_bannerTableau.SetBannerCode(value);
		}
	}

	public bool IsNineGrid
	{
		set
		{
			_bannerTableau.SetIsNineGrid(value);
		}
	}

	public float CustomRenderScale
	{
		set
		{
			_bannerTableau.SetCustomRenderScale(value);
		}
	}

	public Vec2 UpdatePositionValueManual
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_bannerTableau.SetUpdatePositionValueManual(value);
		}
	}

	public Vec2 UpdateSizeValueManual
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_bannerTableau.SetUpdateSizeValueManual(value);
		}
	}

	public (float, bool) UpdateRotationValueManualWithMirror
	{
		set
		{
			_bannerTableau.SetUpdateRotationValueManual(value);
		}
	}

	public int MeshIndexToUpdate
	{
		set
		{
			_bannerTableau.SetMeshIndexToUpdate(value);
		}
	}

	public bool IsHidden
	{
		get
		{
			return _isHidden;
		}
		set
		{
			if (_isHidden != value)
			{
				_isHidden = value;
			}
		}
	}

	public BannerTableauTextureProvider()
	{
		_bannerTableau = new BannerTableau();
	}

	public override void Clear(bool clearNextFrame)
	{
		_bannerTableau.OnFinalize();
		((TextureProvider)this).Clear(clearNextFrame);
	}

	private void CheckTexture()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((NativeObject)(object)_texture != (NativeObject)(object)_bannerTableau.Texture)
		{
			_texture = _bannerTableau.Texture;
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
		_bannerTableau.SetTargetSize(width, height);
	}

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		CheckTexture();
		_bannerTableau.OnTick(dt);
	}
}
