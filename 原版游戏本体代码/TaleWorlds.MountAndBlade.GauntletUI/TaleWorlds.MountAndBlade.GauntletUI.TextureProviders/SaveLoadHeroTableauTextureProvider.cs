using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class SaveLoadHeroTableauTextureProvider : TextureProvider
{
	private string _characterCode;

	private BasicCharacterTableau _tableau;

	private Texture _texture;

	private Texture _providedTexture;

	public string HeroVisualCode
	{
		set
		{
			_characterCode = value;
			DeserializeCharacterCode(_characterCode);
		}
	}

	public string BannerCode
	{
		set
		{
			_tableau.SetBannerCode(value);
		}
	}

	public bool IsVersionCompatible => _tableau.IsVersionCompatible;

	public bool CurrentlyRotating
	{
		set
		{
			_tableau.RotateCharacter(value);
		}
	}

	public SaveLoadHeroTableauTextureProvider()
	{
		_tableau = new BasicCharacterTableau();
	}

	public override void Tick(float dt)
	{
		CheckTexture();
		_tableau.OnTick(dt);
	}

	public override void SetTargetSize(int width, int height)
	{
		((TextureProvider)this).SetTargetSize(width, height);
		_tableau.SetTargetSize(width, height);
	}

	private void DeserializeCharacterCode(string characterCode)
	{
		if (!string.IsNullOrEmpty(characterCode))
		{
			_tableau.DeserializeCharacterCode(characterCode);
		}
	}

	private void CheckTexture()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((NativeObject)(object)_texture != (NativeObject)(object)_tableau.Texture)
		{
			_texture = _tableau.Texture;
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

	public override void Clear(bool clearNextFrame)
	{
		_tableau.OnFinalize();
		((TextureProvider)this).Clear(clearNextFrame);
	}

	protected override Texture OnGetTextureForRender(TwoDimensionContext twoDimensionContext, string name)
	{
		CheckTexture();
		return _providedTexture;
	}
}
